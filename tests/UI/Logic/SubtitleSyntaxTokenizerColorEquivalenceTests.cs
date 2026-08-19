using Avalonia.Media;
using Nikse.SubtitleEdit.Logic;
using System;
using System.Collections.Generic;

namespace UITests.Logic;

/// <summary>
/// The colour parsers were rewritten to work on spans (they run per colour tag, per visible row,
/// on every grid repaint, and again per keystroke for the edit box). These pin the rewrite against
/// verbatim copies of the implementations it replaced, over a battery that covers every branch and
/// the odd inputs a subtitle file can carry: wrong lengths, non-hex digits, surrounding and
/// embedded whitespace, empty and whitespace-only values, and casing.
/// </summary>
public class SubtitleSyntaxTokenizerColorEquivalenceTests
{
    [Fact]
    public void TryParseColor_MatchesTheImplementationItReplaced()
    {
        var diffs = new List<string>();
        foreach (var input in Battery())
        {
            var expected = ReferenceTryParseColor(input);
            var actual = SubtitleSyntaxTokenizer.TryParseColor(input);
            if (expected != actual)
            {
                diffs.Add($"[{input}] expected={expected?.ToString() ?? "null"} actual={actual?.ToString() ?? "null"}");
            }
        }

        Assert.True(diffs.Count == 0, string.Join("\n", diffs));
    }

    [Fact]
    public void TryParseAssColor_MatchesTheImplementationItReplaced()
    {
        var diffs = new List<string>();
        foreach (var input in Battery())
        {
            var expected = ReferenceTryParseAssColor(input);
            var actual = SubtitleSyntaxTokenizer.TryParseAssColor(input);
            if (expected != actual)
            {
                diffs.Add($"[{input}] expected={expected?.ToString() ?? "null"} actual={actual?.ToString() ?? "null"}");
            }
        }

        Assert.True(diffs.Count == 0, string.Join("\n", diffs));
    }

    [Fact]
    public void TryParseColor_ParsesTheCommonForms()
    {
        Assert.Equal(Color.FromRgb(0xFF, 0x88, 0x00), SubtitleSyntaxTokenizer.TryParseColor("#ff8800"));
        Assert.Equal(Color.FromRgb(0xAA, 0xBB, 0xCC), SubtitleSyntaxTokenizer.TryParseColor("#abc"));
        Assert.Equal(Color.FromRgb(255, 0, 0), SubtitleSyntaxTokenizer.TryParseColor("Red"));
        Assert.Null(SubtitleSyntaxTokenizer.TryParseColor("#gg8800"));
    }

    [Fact]
    public void TryParseAssColor_ParsesTheCommonForms()
    {
        // &HBBGGRR&
        Assert.Equal(Color.FromRgb(0x00, 0xFF, 0x00), SubtitleSyntaxTokenizer.TryParseAssColor("&H00FF00&"));
        Assert.Equal(Color.FromRgb(0x33, 0x22, 0x11), SubtitleSyntaxTokenizer.TryParseAssColor("&H112233&"));
        Assert.Null(SubtitleSyntaxTokenizer.TryParseAssColor("&H00FF00"));
    }

    private static IEnumerable<string> Battery()
    {
        string[] cores =
        {
            "", " ", "\t", "#", "#a", "#ab", "#abc", "#ABC", "#f0f", "#ff8800", "#FF8800",
            "#ff88", "#ff880", "#ff8800ff", "#ffff8800", "#gg8800", "#ff88zz", "#-f8800",
            "#+f8800", "#ff 800", "# ff8800", "#0x8800", "red", "Red", "RED", "notacolor",
            "&H00FF00&", "&h00ff00&", "&H112233&", "&HFF112233&", "&H00FF00", "H00FF00&",
            "&H&", "&H1&", "&H12345&", "&H1234567&", "&HGGFF00&", "&H00 F00&", "&H-0FF00&",
            "&H00FF00&&", "&&H00FF00&",
        };

        foreach (var core in cores)
        {
            yield return core;
            yield return " " + core;
            yield return core + " ";
            yield return "  " + core + "  ";
            yield return "\t" + core + "\n";
        }
    }

    // ---- verbatim copies of the string implementations that were replaced ----

    private static Color? ReferenceTryParseColor(string colorValue)
    {
        if (string.IsNullOrWhiteSpace(colorValue))
        {
            return null;
        }

        colorValue = colorValue.Trim();

        if (NamedColors.TryGetValue(colorValue, out var namedColor))
        {
            return namedColor;
        }

        if (colorValue.StartsWith('#'))
        {
            var hex = colorValue[1..];
            try
            {
                if (hex.Length == 3)
                {
                    var r = Convert.ToByte(new string(hex[0], 2), 16);
                    var g = Convert.ToByte(new string(hex[1], 2), 16);
                    var b = Convert.ToByte(new string(hex[2], 2), 16);
                    return Color.FromRgb(r, g, b);
                }

                if (hex.Length == 6)
                {
                    var r = Convert.ToByte(hex[..2], 16);
                    var g = Convert.ToByte(hex[2..4], 16);
                    var b = Convert.ToByte(hex[4..6], 16);
                    return Color.FromRgb(r, g, b);
                }

                if (hex.Length == 8)
                {
                    var r = Convert.ToByte(hex[2..4], 16);
                    var g = Convert.ToByte(hex[4..6], 16);
                    var b = Convert.ToByte(hex[6..8], 16);
                    return Color.FromRgb(r, g, b);
                }
            }
            catch
            {
                return null;
            }
        }

        return null;
    }

    private static Color? ReferenceTryParseAssColor(string colorValue)
    {
        if (string.IsNullOrWhiteSpace(colorValue))
        {
            return null;
        }

        colorValue = colorValue.Trim();

        if (colorValue.StartsWith("&H", StringComparison.OrdinalIgnoreCase) && colorValue.EndsWith('&'))
        {
            var hex = colorValue.Substring(2, colorValue.Length - 3);
            try
            {
                if (hex.Length == 6)
                {
                    var b = Convert.ToByte(hex[..2], 16);
                    var g = Convert.ToByte(hex[2..4], 16);
                    var r = Convert.ToByte(hex[4..6], 16);
                    return Color.FromRgb(r, g, b);
                }

                if (hex.Length == 8)
                {
                    var b = Convert.ToByte(hex[2..4], 16);
                    var g = Convert.ToByte(hex[4..6], 16);
                    var r = Convert.ToByte(hex[6..8], 16);
                    return Color.FromRgb(r, g, b);
                }
            }
            catch
            {
                return null;
            }
        }

        return null;
    }

    /// <summary>
    /// The production named-colour table, read by reflection so the reference above can only
    /// differ from it in loop shape, never in data.
    /// </summary>
    private static readonly Dictionary<string, Color> NamedColors = ReadNamedColors();

    private static Dictionary<string, Color> ReadNamedColors()
    {
        var field = typeof(SubtitleSyntaxTokenizer).GetField(
            "NamedColors",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
        Assert.NotNull(field);
        return (Dictionary<string, Color>)field!.GetValue(null)!;
    }
}
