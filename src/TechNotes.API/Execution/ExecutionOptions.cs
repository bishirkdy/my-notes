namespace TechNotes.API.Execution;

public sealed class ExecutionOptions
{
    public const string SectionName = "Execution";

    public bool Enabled { get; set; } = true;

    public string PistonBaseUrl { get; set; } = "http://127.0.0.1:2000";
    public int MaxCodeBytes { get; set; } = 131_072;
    public int MaxOutputBytes { get; set; } = 65_536;
    public int MaxConcurrentRuns { get; set; } = 2;
    public int RateLimitPermitLimit { get; set; } = 6;
    public int CompileTimeoutSeconds { get; set; } = 15;
    public int RunTimeoutSeconds { get; set; } = 8;
    public int SqlQueryTimeoutSeconds { get; set; } = 8;
    public int SqlProvisionTimeoutSeconds { get; set; } = 30;
    public int SqlMaxResultRows { get; set; } = 100;
    public string SqlServerAdminConnectionString { get; set; } = string.Empty;
    public string SqlServerSandboxConnectionString { get; set; } = string.Empty;
    public string PostgresAdminConnectionString { get; set; } = string.Empty;
    public string PostgresSandboxConnectionString { get; set; } = string.Empty;
}
