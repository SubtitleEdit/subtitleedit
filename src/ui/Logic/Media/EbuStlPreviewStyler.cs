using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using SkiaSharp;
using System;

namespace Nikse.SubtitleEdit.Logic.Media;

/// <summary>
/// Draws an EBU STL subtitle in the video preview the way a teletext decoder would: the box behind
/// the text and the double height rows.
/// </summary>
/// <remarks>
/// Shared by the mpv and the VLC reloader. They used to carry a copy each, and only the mpv copy
/// was ever fixed - the VLC one still drew the box in a hard coded 12 pt Tahoma that threw away
/// the font, size, color and margins the preview was set up with.
/// </remarks>
internal static class EbuStlPreviewStyler
{
    /// <summary>
    /// True when <paramref name="header"/> is the verbatim GSI block Ebu.LoadSubtitle keeps:
    /// 3 characters of code page number, then the disk format code ("STL25.01").
    /// </summary>
    public static bool IsStlHeader(string header)
    {
        return header != null && header.Length > 20 && header.AsSpan(3, 3).SequenceEqual("STL");
    }

    /// <summary>
    /// Replaces the preview header with a boxed and an unboxed style and points every paragraph at
    /// the right one.
    /// </summary>
    /// <param name="subtitle">Preview copy of the subtitle - the header and the paragraphs are rewritten in place.</param>
    /// <param name="sourceHeader">The GSI block of the STL file the subtitle came from.</param>
    /// <param name="previewStyle">The style the preview is configured with - font, size, colors, alignment, margins.</param>
    /// <param name="title">Script title for the generated ASSA header.</param>
    public static void Apply(Subtitle subtitle, string sourceHeader, SsaStyle previewStyle, string title)
    {
        // The box and the double height are teletext control codes, so display standard "0"
        // (open subtitling) is written without either however the save options are set - see
        // Ebu.EncodeText, which gates both the same way.
        var useBox = false;
        var useDoubleHeight = false;
        try
        {
            var encoding = Ebu.GetEncoding(sourceHeader[..3]);
            var header = Ebu.ReadHeader(encoding.GetBytes(sourceHeader));
            if (header.DisplayStandardCode != "0")
            {
                var subtitleSettings = Configuration.Settings.SubtitleSettings;
                useBox = subtitleSettings.EbuStlTeletextUseBox;
                useDoubleHeight = subtitleSettings.EbuStlTeletextUseDoubleHeight;
            }
        }
        catch
        {
            // ignore - an unreadable header is previewed as plain text
        }

        var defaultStyle = new SsaStyle(previewStyle);
        var boxStyle = new SsaStyle(previewStyle)
        {
            Name = "Box",
            BorderStyle = "3", // opaque box
            Outline = SKColors.Black, // border style 3 fills the box with the outline color, and a teletext box is black
            ShadowWidth = 0,
        };

        if (boxStyle.OutlineWidth < 1)
        {
            boxStyle.OutlineWidth = 1; // the box would otherwise cling to the glyphs
        }

        if (useDoubleHeight)
        {
            // A double height row is the same glyphs drawn at twice the height - the width is
            // untouched, which is why a double height line still fits the same 40 characters.
            // Both styles get it: the code is written per text field, so a file that uses double
            // height uses it for every line, boxed or not.
            defaultStyle.ScaleY = 200;
            boxStyle.ScaleY = 200;
        }

        subtitle.Header = string.Format(
            AdvancedSubStationAlpha.HeaderNoStyles,
            title,
            boxStyle.ToRawAss(SsaStyle.DefaultAssStyleFormat) + Environment.NewLine +
            defaultStyle.ToRawAss(SsaStyle.DefaultAssStyleFormat));

        for (var index = 0; index < subtitle.Paragraphs.Count; index++)
        {
            var p = subtitle.Paragraphs[index];

            p.Extra = useBox ? "Box" : "Default";

            if (p.Text.Contains("<box>", StringComparison.Ordinal))
            {
                p.Extra = "Box";
                p.Text = p.Text.Replace("<box>", string.Empty).Replace("</box>", string.Empty);
            }
        }
    }
}
