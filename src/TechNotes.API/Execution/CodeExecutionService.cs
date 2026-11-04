using System.Diagnostics;
using System.Text;
using Microsoft.Extensions.Options;

namespace TechNotes.API.Execution;

public sealed class CodeExecutionService
{
    private const string CSharpRuntimeConfig = """
        {
          "runtimeOptions": {
            "tfm": "net10.0",
            "framework": {
              "name": "Microsoft.NETCore.App",
              "version": "10.0.0"
            },
            "configProperties": {
              "System.Runtime.Serialization.EnableUnsafeBinaryFormatterSerialization": false
            }
          }
        }
        """;

    private readonly CSharpSnippetCompiler _csharpCompiler;
    private readonly PistonClient _piston;
    private readonly SqlServerSandbox _sqlServer;
    private readonly PostgresSandbox _postgres;
    private readonly ExecutionOptions _options;
    private readonly SemaphoreSlim _concurrency;
    private readonly ILogger<CodeExecutionService> _logger;

    public CodeExecutionService(
        CSharpSnippetCompiler csharpCompiler,
        PistonClient piston,
        SqlServerSandbox sqlServer,
        PostgresSandbox postgres,
        IOptions<ExecutionOptions> options,
        ILogger<CodeExecutionService> logger)
    {
        _csharpCompiler = csharpCompiler;
        _piston = piston;
        _sqlServer = sqlServer;
        _postgres = postgres;
        _options = options.Value;
        _concurrency = new SemaphoreSlim(Math.Max(1, _options.MaxConcurrentRuns));
        _logger = logger;
    }

    public async Task<ExecuteCodeResponse> ExecuteAsync(
        ExecuteCodeRequest request,
        CancellationToken cancellationToken)
    {
        var started = Stopwatch.GetTimestamp();
        var language = ExecutionLanguageRegistry.Normalize(request.Language);
        var code = request.Code ?? string.Empty;

        if (Encoding.UTF8.GetByteCount(code) > _options.MaxCodeBytes)
        {
            return Failure(language, "The code is larger than the 128 KB execution limit.", started);
        }

        if (!await _concurrency.WaitAsync(TimeSpan.FromSeconds(15), cancellationToken))
        {
            return Failure(language, "The execution queue is busy. Please try again shortly.", started);
        }

        try
        {
            return language switch
            {
                "csharp" => await RunCSharpAsync(code, cancellationToken),
                "javascript" => await RunPistonAsync(
                    language,
                    "javascript",
                    "20.11.1",
                    [new PistonFile("main.js", code)],
                    started,
                    cancellationToken),
                "bash" => await RunPistonAsync(
                    language,
                    "bash",
                    "5.2.0",
                    [new PistonFile("main.sh", code)],
                    started,
                    cancellationToken),
                "sql" => await RunSqlAsync(code, request.Dialect, started, cancellationToken),
                _ => Failure(language, "This language is not available for server-side execution.", started)
            };
        }
        catch (PistonException exception)
        {
            _logger.LogWarning(exception, "Piston rejected an execution for {Language}.", language);
            return Failure(language, exception.Message, started);
        }
        catch (HttpRequestException exception)
        {
            _logger.LogWarning(exception, "The execution runtime is unavailable for {Language}.", language);
            return Failure(
                language,
                "The code execution runtime is unavailable on this deployment.",
                started);
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            return Failure(language, "The execution timed out.", started);
        }
        finally
        {
            _concurrency.Release();
        }
    }

    private async Task<ExecuteCodeResponse> RunCSharpAsync(
        string code,
        CancellationToken cancellationToken)
    {
        var started = Stopwatch.GetTimestamp();
        var compile = await Task.Run(() => _csharpCompiler.Compile(code), cancellationToken);

        if (!compile.Success)
        {
            return new ExecuteCodeResponse(
                false,
                "run",
                string.Empty,
                compile.Error,
                compile.Diagnostics,
                Elapsed(started),
                "csharp",
                "10.0.401");
        }

        if (compile.IsLibrary)
        {
            return new ExecuteCodeResponse(
                true,
                "check",
                "Compiled successfully.",
                null,
                [],
                Elapsed(started),
                "csharp",
                "10.0.401");
        }

        var result = await _piston.ExecuteAsync(
            "csharp.net",
            "10.0.401",
            [
                new PistonFile(
                    "main.cs",
                    "// Precompiled by the TechNotes execution service.",
                    "utf8"),
                new PistonFile(
                    "app.dll",
                    Convert.ToBase64String(compile.Assembly!),
                    "base64"),
                new PistonFile(
                    "app.runtimeconfig.json",
                    Convert.ToBase64String(Encoding.UTF8.GetBytes(CSharpRuntimeConfig)),
                    "base64")
            ],
            cancellationToken);

        return ToResponse("csharp", "run", "10.0.401", result, started);
    }

    private async Task<ExecuteCodeResponse> RunSqlAsync(
        string code,
        string? dialect,
        long started,
        CancellationToken cancellationToken)
    {
        return SqlDialectRegistry.Normalize(dialect) switch
        {
            SqlDialectRegistry.Sqlite => await RunPistonAsync(
                "sql",
                "sqlite3",
                "3.36.0",
                [new PistonFile("main.sql", code)],
                started,
                cancellationToken),
            SqlDialectRegistry.SqlServer => await RunSandboxAsync(
                _sqlServer,
                code,
                started,
                cancellationToken),
            SqlDialectRegistry.Postgres => await RunSandboxAsync(
                _postgres,
                code,
                started,
                cancellationToken),
            var unknown => Failure(
                "sql",
                $"The {unknown} SQL sandbox is not configured on this server.",
                started)
        };
    }

    private async Task<ExecuteCodeResponse> RunSandboxAsync(
        ISqlSandbox sandbox,
        string code,
        long started,
        CancellationToken cancellationToken)
    {
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeout.CancelAfter(TimeSpan.FromSeconds(
            _options.SqlProvisionTimeoutSeconds + _options.SqlQueryTimeoutSeconds + 10));

        var result = await sandbox.ExecuteAsync(code, timeout.Token);

        return new ExecuteCodeResponse(
            result.Success,
            "run",
            result.Output,
            result.Error,
            result.Diagnostics,
            Elapsed(started),
            "sql",
            sandbox.Version);
    }

    private async Task<ExecuteCodeResponse> RunPistonAsync(
        string language,
        string runtimeLanguage,
        string version,
        IReadOnlyList<PistonFile> files,
        long started,
        CancellationToken cancellationToken)
    {
        var result = await _piston.ExecuteAsync(runtimeLanguage, version, files, cancellationToken);
        return ToResponse(language, "run", version, result, started);
    }

    private static ExecuteCodeResponse ToResponse(
        string language,
        string action,
        string version,
        PistonExecutionResult result,
        long started)
    {
        return new ExecuteCodeResponse(
            result.Success,
            action,
            result.Output,
            result.Error,
            [],
            Elapsed(started),
            language,
            result.Version ?? version);
    }

    private static ExecuteCodeResponse Failure(string language, string error, long started)
    {
        return new ExecuteCodeResponse(
            false,
            "run",
            string.Empty,
            error,
            [],
            Elapsed(started),
            language,
            null);
    }

    private static long Elapsed(long started)
    {
        return (long)Stopwatch.GetElapsedTime(started).TotalMilliseconds;
    }
}
