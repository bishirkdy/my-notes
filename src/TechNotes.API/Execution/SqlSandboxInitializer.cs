using System.Text.RegularExpressions;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using Npgsql;

namespace TechNotes.API.Execution;

public sealed partial class SqlSandboxInitializer(
    IOptions<ExecutionOptions> options,
    ILogger<SqlSandboxInitializer> logger) : BackgroundService
{
    private readonly ExecutionOptions _options = options.Value;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var sqlServerReady = string.IsNullOrWhiteSpace(_options.SqlServerAdminConnectionString)
            || string.IsNullOrWhiteSpace(_options.SqlServerSandboxConnectionString);
        var postgresReady = string.IsNullOrWhiteSpace(_options.PostgresAdminConnectionString)
            || string.IsNullOrWhiteSpace(_options.PostgresSandboxConnectionString);

        for (var attempt = 0; attempt < 60 && !(sqlServerReady && postgresReady); attempt++)
        {
            if (!sqlServerReady)
            {
                sqlServerReady = await TryInitializeSqlServerAsync(stoppingToken);
            }

            if (!postgresReady)
            {
                postgresReady = await TryInitializePostgresAsync(stoppingToken);
            }

            if (!sqlServerReady || !postgresReady)
            {
                await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);
            }
        }

        if (!sqlServerReady)
        {
            logger.LogWarning("The SQL Server sandbox login could not be initialized.");
        }

        if (!postgresReady)
        {
            logger.LogWarning("The PostgreSQL sandbox role could not be initialized.");
        }
    }

    private async Task<bool> TryInitializeSqlServerAsync(CancellationToken cancellationToken)
    {
        try
        {
            var sandbox = new SqlConnectionStringBuilder(_options.SqlServerSandboxConnectionString);
            var login = sandbox.UserID;
            if (string.IsNullOrWhiteSpace(login) || !SqlIdentifier().IsMatch(login))
            {
                logger.LogWarning("The SQL Server sandbox connection string has no valid SQL login.");
                return true;
            }

            var admin = new SqlConnectionStringBuilder(_options.SqlServerAdminConnectionString)
            {
                InitialCatalog = "master",
                Pooling = false
            };

            await using var connection = new SqlConnection(admin.ConnectionString);
            await connection.OpenAsync(cancellationToken);
            await using var command = connection.CreateCommand();
            command.CommandText = $"""
                IF NOT EXISTS (SELECT 1 FROM sys.sql_logins WHERE name = @login)
                BEGIN
                    CREATE LOGIN {QuoteSqlIdentifier(login)}
                    WITH PASSWORD = N'{EscapeSqlLiteral(sandbox.Password)}',
                         CHECK_POLICY = OFF,
                         CHECK_EXPIRATION = OFF;
                END
                ELSE
                BEGIN
                    ALTER LOGIN {QuoteSqlIdentifier(login)}
                    WITH PASSWORD = N'{EscapeSqlLiteral(sandbox.Password)}';
                END;

                IF IS_SRVROLEMEMBER('dbcreator', @login) = 0
                BEGIN
                    ALTER SERVER ROLE dbcreator ADD MEMBER {QuoteSqlIdentifier(login)};
                END;
                """;
            command.CommandTimeout = 30;
            command.Parameters.AddWithValue("@login", login);
            await command.ExecuteNonQueryAsync(cancellationToken);

            logger.LogInformation("SQL Server sandbox login is ready.");
            return true;
        }
        catch (Exception exception) when (exception is SqlException or InvalidOperationException)
        {
            logger.LogDebug(exception, "Waiting for the SQL Server sandbox.");
            return false;
        }
    }

    private async Task<bool> TryInitializePostgresAsync(CancellationToken cancellationToken)
    {
        try
        {
            var sandbox = new NpgsqlConnectionStringBuilder(_options.PostgresSandboxConnectionString);
            var role = sandbox.Username;
            if (string.IsNullOrWhiteSpace(role) || !PostgresIdentifier().IsMatch(role))
            {
                logger.LogWarning("The PostgreSQL sandbox connection string has no valid role.");
                return true;
            }

            var admin = new NpgsqlConnectionStringBuilder(_options.PostgresAdminConnectionString)
            {
                Database = "postgres",
                Pooling = false
            };

            await using var connection = new NpgsqlConnection(admin.ConnectionString);
            await connection.OpenAsync(cancellationToken);
            await using var command = connection.CreateCommand();
            command.CommandText = $"""
                DO $$
                BEGIN
                    IF NOT EXISTS (SELECT 1 FROM pg_roles WHERE rolname = '{EscapePostgresLiteral(role)}') THEN
                        CREATE ROLE {QuotePostgresIdentifier(role)}
                        LOGIN CREATEDB PASSWORD '{EscapePostgresLiteral(sandbox.Password ?? string.Empty)}';
                    ELSE
                        ALTER ROLE {QuotePostgresIdentifier(role)}
                        LOGIN CREATEDB PASSWORD '{EscapePostgresLiteral(sandbox.Password ?? string.Empty)}';
                    END IF;
                END
                $$;
                """;
            command.CommandTimeout = 30;
            await command.ExecuteNonQueryAsync(cancellationToken);

            logger.LogInformation("PostgreSQL sandbox role is ready.");
            return true;
        }
        catch (Exception exception) when (exception is NpgsqlException or InvalidOperationException)
        {
            logger.LogDebug(exception, "Waiting for the PostgreSQL sandbox.");
            return false;
        }
    }

    private static string QuoteSqlIdentifier(string value)
    {
        return "[" + value.Replace("]", "]]", StringComparison.Ordinal) + "]";
    }

    private static string QuotePostgresIdentifier(string value)
    {
        return "\"" + value.Replace("\"", "\"\"", StringComparison.Ordinal) + "\"";
    }

    private static string EscapeSqlLiteral(string value)
    {
        return value.Replace("'", "''", StringComparison.Ordinal);
    }

    private static string EscapePostgresLiteral(string value)
    {
        return value.Replace("'", "''", StringComparison.Ordinal);
    }

    [GeneratedRegex(@"^[A-Za-z_][A-Za-z0-9_]{0,62}$")]
    private static partial Regex SqlIdentifier();

    [GeneratedRegex(@"^[A-Za-z_][A-Za-z0-9_]{0,62}$")]
    private static partial Regex PostgresIdentifier();
}
