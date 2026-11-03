using Npgsql;
using Microsoft.Extensions.Options;

namespace TechNotes.API.Execution;

public sealed class PostgresSandbox(
    IOptions<ExecutionOptions> options,
    ILogger<PostgresSandbox> logger) : ISqlSandbox
{
    private readonly ExecutionOptions _options = options.Value;

    public string Dialect => SqlDialectRegistry.Postgres;

    public string Version => "PostgreSQL 17";

    public async Task<SqlSandboxResult> ExecuteAsync(string code, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_options.PostgresSandboxConnectionString))
        {
            return Failure("The PostgreSQL sandbox is not configured on this server.");
        }

        var databaseName = "technotes_pg_" + Guid.NewGuid().ToString("N");
        var maintenanceBuilder = new NpgsqlConnectionStringBuilder(_options.PostgresSandboxConnectionString)
        {
            Database = "postgres",
            Pooling = false,
            Timeout = 5
        };

        var databaseBuilder = new NpgsqlConnectionStringBuilder(maintenanceBuilder.ConnectionString)
        {
            Database = databaseName
        };

        try
        {
            await using var maintenance = await OpenAsync(maintenanceBuilder.ConnectionString, cancellationToken);
            await using (var create = maintenance.CreateCommand())
            {
                create.CommandText = $"CREATE DATABASE {QuoteIdentifier(databaseName)};";
                create.CommandTimeout = _options.SqlProvisionTimeoutSeconds;
                await create.ExecuteNonQueryAsync(cancellationToken);
            }

            await using var connection = await OpenAsync(databaseBuilder.ConnectionString, cancellationToken);
            await using (var seed = connection.CreateCommand())
            {
                seed.CommandText = SqlSandboxSeed.Postgres;
                seed.CommandTimeout = _options.SqlProvisionTimeoutSeconds;
                await seed.ExecuteNonQueryAsync(cancellationToken);
            }

            return await RunScriptAsync(connection, code, cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (NpgsqlException exception)
        {
            logger.LogWarning(exception, "PostgreSQL sandbox execution failed.");
            return Failure(
                "The PostgreSQL sandbox is unavailable or could not start. "
                + "Check the postgres-exec container and its credentials.");
        }
        finally
        {
            await DropDatabaseAsync(maintenanceBuilder.ConnectionString, databaseName);
        }
    }

    private async Task<SqlSandboxResult> RunScriptAsync(
        NpgsqlConnection connection,
        string code,
        CancellationToken cancellationToken)
    {
        var resultSets = new List<SqlResultSet>();
        var messages = new List<string>();
        var affectedRows = 0;

        connection.Notice += (_, eventArgs) =>
        {
            if (!string.IsNullOrWhiteSpace(eventArgs.Notice.MessageText))
            {
                messages.Add(eventArgs.Notice.MessageText);
            }
        };

        try
        {
            await using var command = connection.CreateCommand();
            command.CommandText = code;
            command.CommandTimeout = _options.SqlQueryTimeoutSeconds;

            await using var reader = await command.ExecuteReaderAsync(cancellationToken);

            do
            {
                if (reader.FieldCount == 0)
                {
                    continue;
                }

                var columns = Enumerable
                    .Range(0, reader.FieldCount)
                    .Select(reader.GetName)
                    .ToArray();

                var rows = new List<IReadOnlyList<string>>();
                var truncated = false;

                while (await reader.ReadAsync(cancellationToken))
                {
                    if (rows.Count >= _options.SqlMaxResultRows)
                    {
                        truncated = true;
                        continue;
                    }

                    var row = new string[reader.FieldCount];
                    for (var column = 0; column < reader.FieldCount; column++)
                    {
                        row[column] = reader.IsDBNull(column)
                            ? "NULL"
                            : SqlResultFormatter.FormatValue(reader.GetValue(column));
                    }

                    rows.Add(row);
                }

                resultSets.Add(new SqlResultSet(columns, rows, truncated));
            } while (await reader.NextResultAsync(cancellationToken));

            if (reader.RecordsAffected > 0)
            {
                affectedRows = reader.RecordsAffected;
            }
        }
        catch (PostgresException exception) when (exception.SqlState == "57014")
        {
            return Failure("The PostgreSQL query timed out.");
        }
        catch (PostgresException exception)
        {
            var (line, column) = PositionToLineColumn(code, exception.Position);
            var message = string.IsNullOrWhiteSpace(exception.Detail)
                ? exception.MessageText
                : $"{exception.MessageText} {exception.Detail}";

            return new SqlSandboxResult(
                false,
                string.Empty,
                message,
                [new ExecutionDiagnostic(line, column, exception.SqlState, message)]);
        }
        catch (NpgsqlException exception) when (exception.InnerException is TimeoutException)
        {
            return Failure("The PostgreSQL query timed out.");
        }

        return new SqlSandboxResult(
            true,
            SqlResultFormatter.Format(resultSets, affectedRows, messages, _options.MaxOutputBytes),
            null,
            []);
    }

    private async Task DropDatabaseAsync(string maintenanceConnectionString, string databaseName)
    {
        try
        {
            await using var connection = new NpgsqlConnection(maintenanceConnectionString);
            await connection.OpenAsync();
            await using var command = connection.CreateCommand();
            command.CommandText = $"DROP DATABASE IF EXISTS {QuoteIdentifier(databaseName)} WITH (FORCE);";
            command.CommandTimeout = _options.SqlProvisionTimeoutSeconds;
            await command.ExecuteNonQueryAsync();
        }
        catch (Exception exception)
        {
            logger.LogWarning(exception, "Could not drop PostgreSQL sandbox database {DatabaseName}.", databaseName);
        }
    }

    private static async Task<NpgsqlConnection> OpenAsync(string connectionString, CancellationToken cancellationToken)
    {
        Exception? lastError = null;

        for (var attempt = 0; attempt < 8; attempt++)
        {
            var connection = new NpgsqlConnection(connectionString);
            try
            {
                await connection.OpenAsync(cancellationToken);
                return connection;
            }
            catch (NpgsqlException exception) when (attempt < 7)
            {
                lastError = exception;
                await connection.DisposeAsync();
                await Task.Delay(TimeSpan.FromMilliseconds(250 * (attempt + 1)), cancellationToken);
            }
        }

        throw lastError ?? new InvalidOperationException("The PostgreSQL sandbox could not be reached.");
    }

    private static (int Line, int Column) PositionToLineColumn(string code, int position)
    {
        if (position <= 0)
        {
            return (1, 0);
        }

        var line = 1;
        var column = 1;
        var limit = Math.Min(position - 1, code.Length);

        for (var index = 0; index < limit; index++)
        {
            if (code[index] == '\n')
            {
                line++;
                column = 1;
            }
            else
            {
                column++;
            }
        }

        return (line, column);
    }

    private static string QuoteIdentifier(string value)
    {
        return "\"" + value.Replace("\"", "\"\"", StringComparison.Ordinal) + "\"";
    }

    private static SqlSandboxResult Failure(string message)
    {
        return new SqlSandboxResult(false, string.Empty, message, []);
    }
}
