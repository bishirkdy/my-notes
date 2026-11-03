using System.Globalization;
using System.Text;

namespace TechNotes.API.Execution;

public sealed record SqlResultSet(
    IReadOnlyList<string> Columns,
    IReadOnlyList<IReadOnlyList<string>> Rows,
    bool Truncated = false);

public static class SqlResultFormatter
{
    private const int MaxCellWidth = 60;

    public static string FormatValue(object? value)
    {
        return value switch
        {
            null or DBNull => "NULL",
            bool boolean => boolean ? "true" : "false",
            DateTime dateTime => dateTime.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture),
            DateTimeOffset dateTimeOffset => dateTimeOffset.ToString("yyyy-MM-dd HH:mm:ss zzz", CultureInfo.InvariantCulture),
            DateOnly dateOnly => dateOnly.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            TimeOnly timeOnly => timeOnly.ToString("HH:mm:ss", CultureInfo.InvariantCulture),
            TimeSpan timeSpan => timeSpan.ToString("c", CultureInfo.InvariantCulture),
            byte[] bytes => "0x" + Convert.ToHexString(bytes),
            Array array => "{" + string.Join(",", array.Cast<object?>().Select(FormatValue)) + "}",
            IFormattable formattable => formattable.ToString(null, CultureInfo.InvariantCulture) ?? string.Empty,
            _ => value.ToString() ?? string.Empty
        };
    }

    public static string Format(
        IReadOnlyList<SqlResultSet> resultSets,
        int affectedRows,
        IReadOnlyList<string> messages,
        int maxBytes)
    {
        var blocks = new List<string>();

        foreach (var message in messages.Where(value => !string.IsNullOrWhiteSpace(value)))
        {
            blocks.Add(message.Trim());
        }

        for (var index = 0; index < resultSets.Count; index++)
        {
            var resultSet = resultSets[index];
            if (resultSet.Columns.Count == 0)
            {
                continue;
            }

            blocks.Add(FormatResultSet(resultSet, index, resultSets.Count));
        }

        if (affectedRows > 0)
        {
            blocks.Add($"{affectedRows} row(s) affected.");
        }

        if (blocks.Count == 0)
        {
            return "Command completed successfully.";
        }

        return Truncate(string.Join("\n\n", blocks), maxBytes);
    }

    private static string FormatResultSet(SqlResultSet resultSet, int index, int count)
    {
        var rows = resultSet.Rows
            .Select(row => row.Select(CleanCell).ToArray())
            .ToArray();

        var widths = new int[resultSet.Columns.Count];
        for (var column = 0; column < resultSet.Columns.Count; column++)
        {
            widths[column] = Math.Min(
                MaxCellWidth,
                Math.Max(
                    resultSet.Columns[column].Length,
                    rows.Length == 0 ? 0 : rows.Max(row => row[column].Length)));
        }

        var builder = new StringBuilder();
        if (count > 1)
        {
            builder.AppendLine($"Result set {index + 1}:");
        }

        AppendRow(builder, resultSet.Columns.Select(CleanCell).ToArray(), widths);
        builder.AppendLine(string.Join("-+-", widths.Select(width => new string('-', width))));

        foreach (var row in rows)
        {
            AppendRow(builder, row, widths);
        }

        builder.Append($"({rows.Length} row(s))");
        if (resultSet.Truncated)
        {
            builder.Append(" - showing the first rows only");
        }

        return builder.ToString();
    }

    private static void AppendRow(StringBuilder builder, IReadOnlyList<string> cells, IReadOnlyList<int> widths)
    {
        for (var column = 0; column < widths.Count; column++)
        {
            if (column > 0)
            {
                builder.Append(" | ");
            }

            builder.Append(cells[column].PadRight(widths[column]));
        }

        builder.AppendLine();
    }

    private static string CleanCell(string value)
    {
        var clean = value
            .Replace("\r", " ", StringComparison.Ordinal)
            .Replace("\n", " ", StringComparison.Ordinal)
            .Replace("\t", " ", StringComparison.Ordinal);

        return clean.Length <= MaxCellWidth ? clean : clean[..(MaxCellWidth - 3)] + "...";
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
}
