using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

namespace TechNotes.API.Execution;

public sealed record CSharpCompileResult(
    bool Success,
    byte[]? Assembly,
    bool IsLibrary,
    string? Error,
    IReadOnlyList<ExecutionDiagnostic> Diagnostics);

public sealed class CSharpSnippetCompiler
{
    private const string GlobalUsings = """
        global using System;
        global using System.Collections.Generic;
        global using System.IO;
        global using System.Linq;
        global using System.Net.Http;
        global using System.Threading;
        global using System.Threading.Tasks;
        """;

    private static readonly Lazy<IReadOnlyList<MetadataReference>> References = new(CreateReferences);

    public CSharpCompileResult Compile(string code)
    {
        var tree = CSharpSyntaxTree.ParseText(
            SourceText.From(code, Encoding.UTF8),
            new CSharpParseOptions(LanguageVersion.Preview),
            path: "Snippet.cs");

        var globals = CSharpSyntaxTree.ParseText(
            SourceText.From(GlobalUsings, Encoding.UTF8),
            new CSharpParseOptions(LanguageVersion.Preview),
            path: "GlobalUsings.g.cs");

        var executable = Compile([tree, globals], OutputKind.ConsoleApplication);
        if (executable.Success)
        {
            return executable;
        }

        if (executable.Diagnostics.Count == 1 && executable.Diagnostics[0].Id == "CS5001")
        {
            var library = Compile([tree, globals], OutputKind.DynamicallyLinkedLibrary);
            if (library.Success)
            {
                return library with { IsLibrary = true };
            }
        }

        return executable;
    }

    private static CSharpCompileResult Compile(IReadOnlyList<SyntaxTree> trees, OutputKind outputKind)
    {
        var compilation = CSharpCompilation.Create(
            "Snippet",
            trees,
            References.Value,
            new CSharpCompilationOptions(
                outputKind,
                optimizationLevel: OptimizationLevel.Release,
                allowUnsafe: true,
                nullableContextOptions: NullableContextOptions.Enable,
                deterministic: true,
                concurrentBuild: false));

        using var assembly = new MemoryStream();
        var result = compilation.Emit(assembly);
        var diagnostics = result.Diagnostics
            .Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
            .Select(ToDiagnostic)
            .ToArray();

        if (!result.Success)
        {
            var error = string.Join(
                Environment.NewLine,
                diagnostics.Select(diagnostic =>
                    $"error {diagnostic.Id}: {diagnostic.Message} (line {diagnostic.Line})"));

            return new CSharpCompileResult(false, null, false, error, diagnostics);
        }

        return new CSharpCompileResult(
            true,
            assembly.ToArray(),
            outputKind == OutputKind.DynamicallyLinkedLibrary,
            null,
            []);
    }

    private static ExecutionDiagnostic ToDiagnostic(Diagnostic diagnostic)
    {
        var span = diagnostic.Location == Location.None
            ? default
            : diagnostic.Location.GetLineSpan();

        return new ExecutionDiagnostic(
            span.StartLinePosition.Line + 1,
            span.StartLinePosition.Character + 1,
            diagnostic.Id,
            diagnostic.GetMessage());
    }

    private static IReadOnlyList<MetadataReference> CreateReferences()
    {
        var trustedAssemblies = (string?)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES");
        if (string.IsNullOrWhiteSpace(trustedAssemblies))
        {
            throw new InvalidOperationException("The .NET runtime did not expose its trusted assemblies.");
        }

        return trustedAssemblies
            .Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries)
            .Where(path => path.EndsWith(".dll", StringComparison.OrdinalIgnoreCase))
            .Where(File.Exists)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Select(path => MetadataReference.CreateFromFile(path))
            .ToArray();
    }
}
