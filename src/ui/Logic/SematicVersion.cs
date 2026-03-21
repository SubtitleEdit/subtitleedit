using System;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Logic;

public class SemanticVersion
{
    public int Major { get; set; }
    public int Minor { get; set; }
    public int Patch { get; set; }
    public string PreRelease { get; set; }
    public int PreReleaseNumber { get; set; }
    public int PreReleaseRank { get; set; }

    public SemanticVersion(string input)
    {
        var trimmedInput = input.Trim();
        PreRelease = string.Empty;
        var version = trimmedInput.StartsWith("v") ? trimmedInput.Substring(1) : trimmedInput;

        var match = Regex.Match(version, @"^(\d+)\.(\d+)\.(\d+)(?:-([a-z]+)(\d+)?)?$", RegexOptions.IgnoreCase);
        if (!match.Success)
        {
            throw new ArgumentException("Invalid version format", nameof(trimmedInput));
        }

        Major = int.Parse(match.Groups[1].Value);
        Minor = int.Parse(match.Groups[2].Value);
        Patch = int.Parse(match.Groups[3].Value);

        if (match.Groups[4].Success)
        {
            PreRelease = match.Groups[4].Value.ToLowerInvariant();
            PreReleaseRank = GetPreReleaseRank(PreRelease);
            PreReleaseNumber = match.Groups[5].Success ? int.Parse(match.Groups[5].Value) : 0;
        }
        else
        {
            PreRelease = string.Empty;
            PreReleaseRank = int.MaxValue; // final/stable is considered highest
        }
    }

    private static int GetPreReleaseRank(string label) => label switch
    {
        "alpha" => 10,
        "preview" => 20,
        "beta" => 30,
        "rc" => 40,
        _ => 1 // unknown tags treated lowest
    };

    private int CompareTo(SemanticVersion? other)
    {
        if (other == null)
        {
            return 1; // null is considered less than any version
        }

        int result = Major.CompareTo(other.Major);
        if (result != 0)
        {
            return result;
        }

        result = Minor.CompareTo(other.Minor);
        if (result != 0)
        {
            return result;
        }

        result = Patch.CompareTo(other.Patch);
        if (result != 0)
        {
            return result;
        }

        result = PreReleaseRank.CompareTo(other.PreReleaseRank);
        if (result != 0)
        {
            return result;
        }

        return PreReleaseNumber.CompareTo(other.PreReleaseNumber);
    }

    public bool IsGreaterThan(SemanticVersion other) => CompareTo(other) > 0;
    public bool IsLessThan(SemanticVersion other) => CompareTo(other) < 0;
    public bool IsEqualTo(SemanticVersion other) => CompareTo(other) == 0;

    public override string ToString()
    {
        return $"v{Major}.{Minor}.{Patch}{(PreRelease != null ? $"-{PreRelease}{(PreReleaseNumber > 0 ? PreReleaseNumber.ToString() : string.Empty)}" : string.Empty)}";
    }
}
