using System.ComponentModel.DataAnnotations;

namespace TechNotes.API.Execution;

public sealed class ExecuteCodeRequest
{
    [Required]
    [MaxLength(32)]
    public string Language { get; set; } = string.Empty;

    [Required]
    [MaxLength(131_072)]
    public string Code { get; set; } = string.Empty;

    [MaxLength(32)]
    public string? Dialect { get; set; }
}

public sealed record ExecutionDiagnostic(int Line, int Column, string Id, string Message);

public sealed record ExecuteCodeResponse(
    bool Success,
    string Action,
    string Output,
    string? Error,
    IReadOnlyList<ExecutionDiagnostic> Diagnostics,
    long DurationMs,
    string Language,
    string? Version);

public sealed record ExecutionLanguage(
    string Id,
    string Label,
    string Action,
    IReadOnlyList<string> Aliases,
    string? Runtime,
    IReadOnlyList<string>? Dialects = null);
