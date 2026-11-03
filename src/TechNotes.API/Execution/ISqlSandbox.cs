namespace TechNotes.API.Execution;

public sealed record SqlSandboxResult(
    bool Success,
    string Output,
    string? Error,
    IReadOnlyList<ExecutionDiagnostic> Diagnostics);

public interface ISqlSandbox
{
    string Dialect { get; }
    string Version { get; }
    Task<SqlSandboxResult> ExecuteAsync(string code, CancellationToken cancellationToken);
}
