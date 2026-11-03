using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;

namespace TechNotes.API.Execution;

public sealed record PistonFile(string Name, string Content, string Encoding = "utf8");

public sealed record PistonExecutionResult(
    bool Success,
    string Output,
    string? Error,
    string? Version);

public sealed class PistonClient(HttpClient httpClient, IOptions<ExecutionOptions> options)
{
    private readonly ExecutionOptions _options = options.Value;

    public async Task<PistonExecutionResult> ExecuteAsync(
        string language,
        string version,
        IReadOnlyList<PistonFile> files,
        CancellationToken cancellationToken)
    {
        var payload = new
        {
            language,
            version,
            files = files.Select(file => new
            {
                name = file.Name,
                content = file.Content,
                encoding = file.Encoding
            }),
            stdin = string.Empty,
            args = Array.Empty<string>(),
            compile_timeout = _options.CompileTimeoutSeconds * 1000,
            run_timeout = _options.RunTimeoutSeconds * 1000
        };

        using var response = await httpClient.PostAsJsonAsync(
            "/api/v2/execute",
            payload,
            cancellationToken);

        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            throw new PistonException(
                $"The execution runtime rejected the request ({(int)response.StatusCode}). {Truncate(body, 500)}");
        }

        using var document = JsonDocument.Parse(body);
        var root = document.RootElement;
        var versionValue = root.TryGetProperty("version", out var versionElement)
            ? versionElement.GetString()
            : version;

        var compile = TryReadStage(root, "compile");
        if (compile?.Code is int compileCode && compileCode != 0)
        {
            var compileOutput = string.IsNullOrWhiteSpace(compile.Stderr)
                ? compile.Output
                : compile.Stderr;

            return new PistonExecutionResult(
                false,
                string.Empty,
                Truncate(compileOutput, _options.MaxOutputBytes),
                versionValue);
        }

        var run = TryReadStage(root, "run");
        if (run is null)
        {
            return new PistonExecutionResult(
                false,
                string.Empty,
                "The execution runtime did not return a result.",
                versionValue);
        }

        var exitCode = run.Code ?? 0;
        var output = Truncate(run.Output, _options.MaxOutputBytes);
        var error = run.Signal is not null
            ? $"Execution stopped by {run.Signal}."
            : exitCode != 0
                ? Truncate(string.IsNullOrWhiteSpace(run.Stderr) ? run.Output : run.Stderr, _options.MaxOutputBytes)
                : null;

        if (error is not null && IsNetworkFailure(error))
        {
            output = string.Empty;
            error = "Outbound network access is disabled in the code sandbox.";
        }

        return new PistonExecutionResult(
            error is null,
            output,
            error,
            versionValue);
    }

    private static PistonStage? TryReadStage(JsonElement root, string name)
    {
        if (!root.TryGetProperty(name, out var stage) || stage.ValueKind == JsonValueKind.Null)
        {
            return null;
        }

        return new PistonStage(
            GetString(stage, "stdout"),
            GetString(stage, "stderr"),
            GetString(stage, "output"),
            stage.TryGetProperty("code", out var code) && code.ValueKind == JsonValueKind.Number
                ? code.GetInt32()
                : null,
            stage.TryGetProperty("signal", out var signal) && signal.ValueKind == JsonValueKind.String
                ? signal.GetString()
                : null);
    }

    private static bool IsNetworkFailure(string value)
    {
        return value.Contains("Resource temporarily unavailable", StringComparison.OrdinalIgnoreCase)
            || value.Contains("Temporary failure in name resolution", StringComparison.OrdinalIgnoreCase)
            || value.Contains("Name or service not known", StringComparison.OrdinalIgnoreCase)
            || value.Contains("Connection refused", StringComparison.OrdinalIgnoreCase);
    }

    private static string GetString(JsonElement element, string property)
    {
        return element.TryGetProperty(property, out var value) && value.ValueKind == JsonValueKind.String
            ? value.GetString() ?? string.Empty
            : string.Empty;
    }

    private static string Truncate(string value, int maxBytes)
    {
        if (Encoding.UTF8.GetByteCount(value) <= maxBytes)
        {
            return value;
        }

        var builder = new StringBuilder();
        var bytes = 0;

        foreach (var character in value)
        {
            var size = Encoding.UTF8.GetByteCount([character]);
            if (bytes + size > maxBytes - 20)
            {
                break;
            }

            builder.Append(character);
            bytes += size;
        }

        builder.Append("\n[output truncated]");
        return builder.ToString();
    }

    private sealed record PistonStage(
        string Stdout,
        string Stderr,
        string Output,
        int? Code,
        string? Signal);
}

public sealed class PistonException(string message) : Exception(message);
