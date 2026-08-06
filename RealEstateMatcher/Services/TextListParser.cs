namespace RealEstateMatcher.Services;

public static class TextListParser
{
    private static readonly char[] Separators = [',', '，', ';', '；', '、', '|', '\r', '\n'];

    public static IReadOnlyList<string> Parse(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Array.Empty<string>();
        }

        return value
            .Split(Separators, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    public static string Join(IEnumerable<string> values)
    {
        return string.Join("、", values.OrderBy(x => x, StringComparer.CurrentCulture));
    }
}
