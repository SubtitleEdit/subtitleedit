using Avalonia.Media;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Logic;

/// <summary>
/// Syntax highlighting for Scenarist SCC (.scc). An SCC line is a time code followed by CEA-608
/// byte pairs in hex, which cannot be read by eye - so each word is decoded and colored by what it
/// is: control command (RCL, EOC, EDM...), preamble address code (row / indent), mid-row code
/// (color / italics), special or extended character, plain text, or padding. The text words stay
/// in the default color, so the caption text is what stands out.
/// </summary>
public partial class SccSourceSyntaxHighlighting : ISourceSyntaxHighlighter
{
    public enum WordKind
    {
        Unknown,
        Padding,
        Text,
        Command,
        PreambleAddress,
        MidRow,
        SpecialCharacter,
    }

    private static Color HeaderColor => AssaSourceSyntaxHighlighting.SectionColor;
    private static Color TimeColor => SubRipSourceSyntaxHighlighting.TimeColor;
    private static Color CommandColor => AssaSourceSyntaxHighlighting.KeywordColor;
    private static Color PreambleAddressColor => AssaSourceSyntaxHighlighting.PropertyColor;
    private static Color MidRowColor => AssaSourceSyntaxHighlighting.ValueColor;
    private static Color SpecialCharacterColor => SubtitleSyntaxTokenizer.StyleColor;
    private static Color PaddingColor => SubtitleSyntaxTokenizer.CharsColor;

    // "00:00:01:12" non-drop frame, "00:00:01;12" drop frame.
    [GeneratedRegex(@"^\d{2}:\d{2}:\d{2}[:;.,]\d{2}")]
    private static partial Regex TimeCodeRegex();

    [GeneratedRegex(@"(?<=\s)[0-9A-Fa-f]{4}(?=\s|$)")]
    private static partial Regex WordRegex();

    public void HighlightLine(string lineText, SourceSyntaxLineStyler styler)
    {
        if (string.IsNullOrEmpty(lineText))
        {
            return;
        }

        if (lineText.StartsWith("Scenarist_SCC", System.StringComparison.Ordinal))
        {
            styler.Apply(0, lineText.Length, HeaderColor, bold: true);
            return;
        }

        var timeCode = TimeCodeRegex().Match(lineText);
        if (!timeCode.Success)
        {
            return;
        }

        styler.Apply(0, timeCode.Length, TimeColor, bold: true, defaultFont: true);

        foreach (Match word in WordRegex().Matches(lineText, timeCode.Length))
        {
            var kind = Classify(word.ValueSpan);
            var color = kind switch
            {
                WordKind.Command => CommandColor,
                WordKind.PreambleAddress => PreambleAddressColor,
                WordKind.MidRow => MidRowColor,
                WordKind.SpecialCharacter => SpecialCharacterColor,
                WordKind.Padding => PaddingColor,
                _ => (Color?)null, // text and anything undecodable keep the default color
            };

            if (color != null)
            {
                styler.Apply(word.Index, word.Length, color.Value, bold: kind == WordKind.Command);
            }
        }
    }

    /// <summary>
    /// Decodes one SCC word (two bytes with odd parity) the way a CEA-608 decoder does: the parity
    /// bit is dropped, and the first byte picks the code family. Channel 2 codes differ from
    /// channel 1 only in bit 0x08 of the first byte.
    /// </summary>
    internal static WordKind Classify(System.ReadOnlySpan<char> hex)
    {
        if (!int.TryParse(hex, System.Globalization.NumberStyles.HexNumber, null, out var word))
        {
            return WordKind.Unknown;
        }

        var b1 = (word >> 8) & 0x7F;
        var b2 = word & 0x7F;

        if (b1 == 0 && b2 == 0)
        {
            return WordKind.Padding;
        }

        if (b1 >= 0x20)
        {
            return WordKind.Text;
        }

        if (b1 < 0x10)
        {
            return WordKind.Unknown; // XDS data
        }

        var code = b1 & 0x77; // channel bit off: 0x10..0x17
        var isCommandRange = b2 >= 0x20 && b2 <= 0x2F;

        if ((code == 0x14 || code == 0x15) && isCommandRange)
        {
            return WordKind.Command; // RCL, BS, EDM, CR, ENM, EOC, RU2-4, RDC, TR, RTD...
        }

        if (code == 0x17 && b2 >= 0x21 && b2 <= 0x23)
        {
            return WordKind.Command; // tab offsets 1-3
        }

        if (code == 0x11 && isCommandRange)
        {
            return WordKind.MidRow; // color, italics, underline
        }

        if ((code == 0x10 || code == 0x17) && isCommandRange)
        {
            return WordKind.MidRow; // background and foreground attributes
        }

        if (code == 0x11 && b2 >= 0x30 && b2 <= 0x3F)
        {
            return WordKind.SpecialCharacter; // ®, °, ½, ♪...
        }

        if ((code == 0x12 || code == 0x13) && b2 >= 0x20 && b2 <= 0x3F)
        {
            return WordKind.SpecialCharacter; // extended Western European characters
        }

        if (b2 >= 0x40)
        {
            return WordKind.PreambleAddress; // row, color / indent
        }

        return WordKind.Unknown;
    }
}
