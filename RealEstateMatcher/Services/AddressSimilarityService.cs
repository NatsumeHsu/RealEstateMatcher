using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace RealEstateMatcher.Services;

public sealed partial class AddressSimilarityService
{
    public double Calculate(string? left, string? right)
    {
        if (string.IsNullOrWhiteSpace(left) || string.IsNullOrWhiteSpace(right))
        {
            return 1.0;
        }

        string normalizedLeft = Normalize(left);
        string normalizedRight = Normalize(right);

        if (normalizedLeft.Length == 0 || normalizedRight.Length == 0)
        {
            return 1.0;
        }

        if (string.Equals(normalizedLeft, normalizedRight, StringComparison.Ordinal))
        {
            return 1.0;
        }

        string shorter = normalizedLeft.Length <= normalizedRight.Length
            ? normalizedLeft
            : normalizedRight;
        string longer = normalizedLeft.Length > normalizedRight.Length
            ? normalizedLeft
            : normalizedRight;

        double score;

        // 常見情境：買方只填「板橋區」，賣方填完整門牌。
        if (longer.Contains(shorter, StringComparison.Ordinal))
        {
            double lengthRatio = (double)shorter.Length / longer.Length;
            score = 0.92 + (0.08 * lengthRatio);
        }
        else
        {
            score = CalculateDiceCoefficient(normalizedLeft, normalizedRight);
        }

        string? leftCity = ExtractGroup(CityRegex(), normalizedLeft, "value");
        string? rightCity = ExtractGroup(CityRegex(), normalizedRight, "value");
        if (leftCity is not null && rightCity is not null && leftCity != rightCity)
        {
            score *= 0.35;
        }

        string? leftDistrict = ExtractDistrict(normalizedLeft);
        string? rightDistrict = ExtractDistrict(normalizedRight);
        if (leftDistrict is not null && rightDistrict is not null && leftDistrict != rightDistrict)
        {
            score *= 0.25;
        }

        return Math.Clamp(score, 0.0, 1.0);
    }

    public string Normalize(string value)
    {
        string normalized = value
            .Normalize(NormalizationForm.FormKC)
            .Trim()
            .ToLower(CultureInfo.InvariantCulture)
            .Replace("臺", "台", StringComparison.Ordinal)
            .Replace("台灣省", string.Empty, StringComparison.Ordinal)
            .Replace("號之", "號", StringComparison.Ordinal);

        normalized = NonAddressCharacterRegex().Replace(normalized, string.Empty);
        return normalized;
    }

    private static double CalculateDiceCoefficient(string left, string right)
    {
        if (left.Length == 1 || right.Length == 1)
        {
            return left == right ? 1.0 : 0.0;
        }

        Dictionary<string, int> leftBigrams = BuildBigramMultiset(left);
        Dictionary<string, int> rightBigrams = BuildBigramMultiset(right);

        int intersection = 0;
        foreach ((string bigram, int leftCount) in leftBigrams)
        {
            if (rightBigrams.TryGetValue(bigram, out int rightCount))
            {
                intersection += Math.Min(leftCount, rightCount);
            }
        }

        int leftTotal = leftBigrams.Values.Sum();
        int rightTotal = rightBigrams.Values.Sum();

        return (2.0 * intersection) / (leftTotal + rightTotal);
    }

    private static Dictionary<string, int> BuildBigramMultiset(string value)
    {
        var result = new Dictionary<string, int>(StringComparer.Ordinal);

        for (int i = 0; i < value.Length - 1; i++)
        {
            string bigram = value.Substring(i, 2);
            result.TryGetValue(bigram, out int count);
            result[bigram] = count + 1;
        }

        return result;
    }

    private static string? ExtractGroup(Regex regex, string value, string groupName)
    {
        Match match = regex.Match(value);
        return match.Success ? match.Groups[groupName].Value : null;
    }

    private static string? ExtractDistrict(string value)
    {
        Match cityMatch = CityRegex().Match(value);
        string remainder = cityMatch.Success
            ? value[(cityMatch.Index + cityMatch.Length)..]
            : value;

        Match districtMatch = DistrictRegex().Match(remainder);
        return districtMatch.Success
            ? districtMatch.Groups["value"].Value
            : null;
    }

    [GeneratedRegex(@"[^0-9a-z\u4e00-\u9fff]", RegexOptions.Compiled)]
    private static partial Regex NonAddressCharacterRegex();

    [GeneratedRegex(@"(?<value>[\u4e00-\u9fff]{2,3}[縣市])", RegexOptions.Compiled)]
    private static partial Regex CityRegex();

    [GeneratedRegex(@"(?<value>[\u4e00-\u9fff]{1,5}[區鄉鎮市])", RegexOptions.Compiled)]
    private static partial Regex DistrictRegex();
}
