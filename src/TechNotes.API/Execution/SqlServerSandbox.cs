using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;

namespace TechNotes.API.Execution;

public sealed class SqlServerSandbox(
    IOptions<ExecutionOptions> options,
    ILogger<SqlServerSandbox> logger) : ISqlSandbox
{
    private readonly ExecutionOptions _options = options.Value;

    public string Dialect => SqlDialectRegistry.SqlServer;

    public string Version => "SQL Server 2022";

    public async Task<SqlSandboxResult> ExecuteAsync(string code, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_options.SqlServerSandboxConnectionString))
        {
            return Failure("The SQL Server sandbox is not configured on this server.");
        }

        var databaseName = "technotes_sql_" + Guid.NewGuid().ToString("N");
        var masterBuilder = new SqlConnectionStringBuilder(_options.SqlServerSandboxConnectionString)
        {
            InitialCatalog = "master",
            Pooling = false
        };

        var databaseBuilder = new SqlConnectionStringBuilder(masterBuilder.ConnectionString)
        {
            InitialCatalog = databaseName
        };

        try
        {
            await using var master = await OpenAsync(masterBuilder.ConnectionString, cancellationToken);
            await using (var create = master.CreateCommand())
            {
                create.CommandText = $"CREATE DATABASE {QuoteIdentifier(databaseName)};";
                create.CommandTimeout = _options.SqlProvisionTimeoutSeconds;
                await create.ExecuteNonQueryAsync(cancellationToken);
            }

            await using var connection = await OpenAsync(databaseBuilder.ConnectionString, cancellationToken);
            await using (var seed = connection.CreateCommand())
            {
                seed.CommandText = SqlSandboxSeed.SqlServer;
                seed.CommandTimeout = _options.SqlProvisionTimeoutSeconds;
                await seed.ExecuteNonQueryAsync(cancellationToken);
            }

            return await RunScriptAsync(connection, code, cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (SqlException exception)
        {
            logger.LogWarning(exception, "SQL Server sandbox execution failed.");
            return Failure(
                "The SQL Server sandbox is unavailable or could not start. "
                + "Check the sqlserver container and its credentials.");
        }
        finally
        {
            await DropDatabaseAsync(masterBuilder.ConnectionString, databaseName);
        }
    }

    private async Task<SqlSandboxResult> RunScriptAsync(
        SqlConnection connection,
        string code,
        CancellationToken cancellationToken)
    {
        var resultSets = new List<SqlResultSet>();
        var messages = new List<string>();
        var affectedRows = 0;
        var batches = SqlBatchSplitter.Split(code);

        connection.InfoMessage += (_, eventArgs) =>
        {
            foreach (SqlError error in eventArgs.Errors)
            {
                if (!string.IsNullOrWhiteSpace(error.Message))
                {
                    messages.Add(error.Message);
                }
            }
        };

        foreach (var batch in batches)
        {
            try
            {
                await using var command = connection.CreateCommand();
                command.CommandText = batch.Text;
                command.CommandTimeout = _options.SqlQueryTimeoutSeconds;

                await using var reader = await command.ExecuteReaderAsync(
                    CommandBehavior.Default,
                    cancellationToken);

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
                    affectedRows += reader.RecordsAffected;
                }
            }
            catch (SqlException exception) when (exception.Number == -2)
            {
                return Failure("The SQL Server query timed out.");
            }
            catch (SqlException exception)
            {
                var line = Math.Max(1, batch.StartLine + Math.Max(0, exception.LineNumber - 1));
                return new SqlSandboxResult(
                    false,
                    string.Empty,
                    exception.Message,
                    [new ExecutionDiagnostic(line, 0, exception.Number.ToString(), exception.Message)]);
            }
        }

        return new SqlSandboxResult(
            true,
            SqlResultFormatter.Format(resultSets, affectedRows, messages, _options.MaxOutputBytes),
            null,
            []);
    }

    private async Task DropDatabaseAsync(string masterConnectionString, string databaseName)
    {
        try
        {
            await using var connection = new SqlConnection(masterConnectionString);
            await connection.OpenAsync();
            await using var command = connection.CreateCommand();
            command.CommandText = $"""
                IF DB_ID(@name) IS NOT NULL
                BEGIN
                    DECLARE @sql NVARCHAR(MAX) =
                        N'ALTER DATABASE ' + QUOTENAME(@name) + N' SET SINGLE_USER WITH ROLLBACK IMMEDIATE; '
                        + N'DROP DATABASE ' + QUOTENAME(@name) + N';';
                    EXEC sp_executesql @sql;
                END
                """;
            command.CommandTimeout = _options.SqlProvisionTimeoutSeconds;
            command.Parameters.AddWithValue("@name", databaseName);
            await command.ExecuteNonQueryAsync();
        }
        catch (Exception exception)
        {
            logger.LogWarning(exception, "Could not drop SQL Server sandbox database {DatabaseName}.", databaseName);
        }
    }

    private static async Task<SqlConnection> OpenAsync(string connectionString, CancellationToken cancellationToken)
    {
        Exception? lastError = null;

        for (var attempt = 0; attempt < 8; attempt++)
        {
            var connection = new SqlConnection(connectionString);
            try
            {
                await connection.OpenAsync(cancellationToken);
                return connection;
            }
            catch (SqlException exception) when (attempt < 7)
            {
                lastError = exception;
                await connection.DisposeAsync();
                await Task.Delay(TimeSpan.FromMilliseconds(250 * (attempt + 1)), cancellationToken);
            }
        }

        throw lastError ?? new InvalidOperationException("The SQL Server sandbox could not be reached.");
    }

    private static string QuoteIdentifier(string value)
    {
        return "[" + value.Replace("]", "]]", StringComparison.Ordinal) + "]";
    }

    private static SqlSandboxResult Failure(string message)
    {
        return new SqlSandboxResult(false, string.Empty, message, []);
    }
}
