namespace TechNotes.API.Execution;

public static class SqlDialectRegistry
{
    public const string Sqlite = "sqlite";
    public const string SqlServer = "sqlserver";
    public const string Postgres = "postgresql";

    public static IReadOnlyList<string> All { get; } = [Sqlite, SqlServer, Postgres];

    public static string Normalize(string? dialect)
    {
        var value = (dialect ?? string.Empty).Trim().ToLowerInvariant();

        return value switch
        {
            "" or "sql" or "sqlite" or "sqlite3" => Sqlite,
            "sqlserver" or "sql-server" or "mssql" or "tsql" or "t-sql" => SqlServer,
            "postgres" or "postgresql" or "pgsql" or "psql" => Postgres,
            _ => value
        };
    }

    public static string Label(string dialect)
    {
        return dialect switch
        {
            SqlServer => "SQL Server 2022",
            Postgres => "PostgreSQL 17",
            _ => "SQLite 3.36"
        };
    }
}
