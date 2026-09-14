using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.UiLogic.Ocr.Service;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace LibUiLogicTests.GoogleCloudVision;

/// <summary>
/// Issue #14884: on large fonts the geometric "insert a space" rule in the Vision text merge
/// fired on ordinary letter gaps, turning "tribunal" into "tribu n a l". Vision already marks
/// word boundaries via detectedBreak; the geometric rule is only a safety net for gaps Vision
/// missed. The French path also kept the space before "." and ",", giving "ici .".
/// </summary>
public class GoogleCloudVisionSpaceThresholdTests
{
    private const int GlyphWidth = 70;
    private const int GlyphHeight = 100;
    private const int LetterGap = 28; // 0.4 x glyph width - above the old "/ 2.7" threshold
    private const int WordGap = 80;

    /// <summary>
    /// Builds a fullTextAnnotation response. Each line is a list of Vision "words"; a word is
    /// its text, the detectedBreak type on its last symbol ("" for none), and any extra
    /// horizontal gap to leave before the next word.
    /// </summary>
    private static string BuildJson(params List<(string text, string lastBreak, int extraGap)>[] lines)
    {
        var sb = new StringBuilder();
        sb.Append("{\"responses\":[{\"fullTextAnnotation\":{\"pages\":[{\"blocks\":[{\"paragraphs\":[{\"words\":[");
        var first = true;
        var y = 10;
        foreach (var line in lines)
        {
            var x = 100;
            foreach (var (text, lastBreak, extraGap) in line)
            {
                if (!first)
                {
                    sb.Append(',');
                }

                first = false;
                sb.Append("{\"symbols\":[");
                for (var i = 0; i < text.Length; i++)
                {
                    if (i > 0)
                    {
                        sb.Append(',');
                    }

                    var isLast = i == text.Length - 1;
                    sb.Append('{');
                    if (isLast && lastBreak.Length > 0)
                    {
                        sb.Append("\"property\":{\"detectedBreak\":{\"type\":\"" + lastBreak + "\"}},");
                    }

                    sb.Append("\"boundingBox\":{\"vertices\":[" + Box(x, y) + "]},");
                    sb.Append("\"text\":\"" + text[i] + "\"}");
                    x += GlyphWidth + LetterGap;
                }

                sb.Append("]}");

                // A SPACE break means a real word gap; anything else stays a letter gap.
                if (lastBreak == "SPACE")
                {
                    x += WordGap - LetterGap;
                }

                x += extraGap;
            }

            y += GlyphHeight + 60;
        }

        // Real responses end fullTextAnnotation with the plain "text" member; SeJsonParser needs
        // a member after the "pages" array to close the object correctly.
        sb.Append("]}]}]}],\"text\":\"\"}}]}");
        return sb.ToString();
    }

    private static string Box(int x, int y)
    {
        var x2 = x + GlyphWidth;
        var y2 = y + GlyphHeight;
        return string.Format(CultureInfo.InvariantCulture,
            "{{\"x\":{0},\"y\":{1}}},{{\"x\":{2},\"y\":{1}}},{{\"x\":{2},\"y\":{3}}},{{\"x\":{0},\"y\":{3}}}",
            x, y, x2, y2);
    }

    [Fact]
    public void LargeFontLetterGapsDoNotBecomeSpaces()
    {
        Configuration.Settings.Tools.OcrGoogleCloudVisionSeHandlesTextMerge = true;
        var json = BuildJson(
            new List<(string, string, int)> { ("Le", "SPACE", 0), ("tribunal", "SPACE", 0), ("a", "SPACE", 0), ("donné", "SPACE", 0), ("à", "SPACE", 0), ("Leena", "LINE_BREAK", 0) },
            new List<(string, string, int)> { ("l'autorisation", "SPACE", 0), ("de", "SPACE", 0), ("s'installer", "SPACE", 0), ("ici", "", 0), (".", "LINE_BREAK", 0) });

        var lines = GoogleCloudVisionApi.JsonToStringList("fr", json);

        Assert.Single(lines);
        Assert.Equal("Le tribunal a donné à Leena" + Environment.NewLine + "l'autorisation de s'installer ici.", lines[0]);
    }

    [Fact]
    public void FrenchKeepsSpaceBeforeQuestionMarkButNotBeforePeriodAndComma()
    {
        Configuration.Settings.Tools.OcrGoogleCloudVisionSeHandlesTextMerge = true;
        var json = BuildJson(
            new List<(string, string, int)> { ("Oui", "", 0), (",", "SPACE", 0), ("ça", "SPACE", 0), ("va", "SPACE", 0), ("?", "LINE_BREAK", 0) });

        Assert.Equal("Oui, ça va ?", GoogleCloudVisionApi.JsonToStringList("fr", json)[0]);
        Assert.Equal("Oui, ça va?", GoogleCloudVisionApi.JsonToStringList("en", json)[0]);
    }

    [Fact]
    public void GapOfMoreThanOneGlyphWidthStillInsertsSpaceWhenVisionMissedIt()
    {
        Configuration.Settings.Tools.OcrGoogleCloudVisionSeHandlesTextMerge = true;

        // Two words, no SPACE break, but two glyph widths of nothing between them.
        var json = BuildJson(new List<(string, string, int)> { ("hello", "", 2 * GlyphWidth), ("world", "LINE_BREAK", 0) });

        Assert.Equal("hello world", GoogleCloudVisionApi.JsonToStringList("en", json)[0]);
    }
}
