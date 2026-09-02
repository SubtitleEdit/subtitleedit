using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Logic.Config;
using SkiaSharp;
using System;
using System.Diagnostics.CodeAnalysis;

namespace Nikse.SubtitleEdit.Logic.Media;

/// <summary>
/// Draws an EBU STL or DVB teletext subtitle in the video preview the way a teletext decoder
/// would: the box behind the text and the double height rows.
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
    public static bool IsStlHeader([NotNullWhen(true)] string? header)
    {
        return header != null && header.Length > 20 && header.AsSpan(3, 3).SequenceEqual("STL");
    }

    /// <summary>
    /// True when the subtitle in the video preview is still a teletext format - EBU STL, or DVB
    /// teletext (.dvbttx), which draws the same boxed double height rows.
    /// </summary>
    /// <remarks>
    /// The GSI block (and the dvbteletext marker) stays on the subtitle when the format is
    /// switched in the toolbar, so the header on its own says only which file the subtitle was
    /// read from - a subtitle shown as SubRip must lose the teletext box and the double height
    /// with the format.
    /// </remarks>
    public static bool IsTeletextPreview([NotNullWhen(true)] string? header, Type? uiFormatType)
    {
        return (uiFormatType == typeof(Ebu) && IsStlHeader(header)) ||
               (uiFormatType == typeof(DvbTeletext) && DvbTeletext.IsDvbTeletextHeader(header));
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
        if (DvbTeletext.IsDvbTeletextHeader(sourceHeader))
        {
            // A .dvbttx is always teletext, and its writer always boxes and double-heights the
            // rows (ManzanitaTeletextWriter.GetRow) - the EBU save options play no part here.
            useBox = true;
            useDoubleHeight = true;
        }
        else
        {
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
        }

        var defaultStyle = new SsaStyle(previewStyle);

        // Preview only, and never written to the file - an STL carries a character table, not a
        // typeface. It is here for someone with a teletext face installed who wants the preview to
        // look like a decoder, so it applies whatever the display standard is: a house font is as
        // valid for open subtitling as a teletext font is for teletext.
        var previewFontName = Se.Settings.File.EbuSaveOptions.PreviewFontName;
        if (!string.IsNullOrWhiteSpace(previewFontName))
        {
            defaultStyle.FontName = previewFontName;
        }

        var boxStyle = new SsaStyle(defaultStyle)
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
