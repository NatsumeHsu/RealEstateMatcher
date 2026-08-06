using System.Text.RegularExpressions;

namespace RealEstateMatcher.Services;

public static partial class LayoutParser
{
    public static LayoutParts Parse(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return new(null, null, null);

        return new LayoutParts(
            Extract(value, BedroomRegex()),
            Extract(value, LivingRoomRegex()),
            Extract(value, BathroomRegex()));
    }

    public static string Format(int? bedrooms, int? livingRooms, int? bathrooms)
    {
        return $"{Part(bedrooms, "房")}{Part(livingRooms, "廳")}{Part(bathrooms, "衛")}";
    }

    private static int? Extract(string value, Regex regex)
    {
        Match match = regex.Match(value);
        return match.Success && int.TryParse(match.Groups["count"].Value, out int count)
            ? count
            : null;
    }

    private static string Part(int? value, string unit) => value.HasValue ? $"{value}{unit}" : string.Empty;

    [GeneratedRegex(@"(?<count>\d+)\s*房")]
    private static partial Regex BedroomRegex();

    [GeneratedRegex(@"(?<count>\d+)\s*[廳听]")]
    private static partial Regex LivingRoomRegex();

    [GeneratedRegex(@"(?<count>\d+)\s*[衛卫]")]
    private static partial Regex BathroomRegex();
}

public sealed record LayoutParts(int? Bedrooms, int? LivingRooms, int? Bathrooms);
