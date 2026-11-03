namespace TechNotes.API.Execution;

public static class ExecutionLanguageRegistry
{
    private static readonly ExecutionLanguage[] Languages =
    [
        new(
            "csharp",
            "C#",
            "run",
            ["cs", "c#", "c#.net", "dotnet"],
            "dotnet 10.0.401"),
        new(
            "javascript",
            "JavaScript",
            "run",
            ["js", "node", "nodejs"],
            "node 20.11.1"),
        new(
            "bash",
            "Bash",
            "run",
            ["sh", "shell"],
            "bash 5.2.0"),
        new(
            "sql",
            "SQL",
            "run",
            ["sqlite", "sqlite3", "sqlserver", "mssql", "tsql", "postgres", "postgresql", "pgsql"],
            "sqlite 3.36.0 / SQL Server 2022 / PostgreSQL 17",
            SqlDialectRegistry.All),
        new(
            "html",
            "HTML",
            "preview",
            ["htm"],
            null),
        new(
            "css",
            "CSS",
            "preview",
            [],
            null)
    ];

    public static IReadOnlyList<ExecutionLanguage> All => Languages;

    public static string Normalize(string language)
    {
        var value = language.Trim().ToLowerInvariant();

        return Languages
            .FirstOrDefault(item =>
                item.Id == value ||
                item.Aliases.Contains(value, StringComparer.OrdinalIgnoreCase))
            ?.Id ?? value;
    }
}
