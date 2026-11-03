using System.Text.RegularExpressions;

namespace TechNotes.API.Execution;

public sealed record SqlBatch(string Text, int StartLine);

public static partial class SqlBatchSplitter
{
    public static IReadOnlyList<SqlBatch> Split(string script)
    {
        var lines = script.Replace("\r\n", "\n", StringComparison.Ordinal).Split('\n');
        var batches = new List<SqlBatch>();
        var current = new List<string>();
        var startLine = 1;

        for (var index = 0; index < lines.Length; index++)
        {
            var match = GoLine().Match(lines[index].Trim());
            if (!match.Success)
            {
                current.Add(lines[index]);
                continue;
            }

            AddBatch(batches, current, startLine, match.Groups[1].Success
                ? Math.Clamp(int.Parse(match.Groups[1].Value), 1, 100)
                : 1);

            current = [];
            startLine = index + 2;
        }

        AddBatch(batches, current, startLine, 1);
        return batches;
    }

    private static void AddBatch(List<SqlBatch> batches, List<string> lines, int startLine, int repeat)
    {
        var text = string.Join("\n", lines).Trim();
        if (text.Length == 0)
        {
            return;
        }

        for (var index = 0; index < repeat; index++)
        {
            batches.Add(new SqlBatch(text, startLine));
        }
    }

    [GeneratedRegex(@"^GO(?:\s+(\d+))?$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex GoLine();
}
