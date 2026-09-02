using Avalonia.Media;
using Avalonia.Skia;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.ContainerFormats.TransportStream;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Features.Main;
using System;
using System.Collections.Generic;

namespace Nikse.SubtitleEdit.Logic;

public interface IColorService
{
    void RemoveColorTags(List<SubtitleLineViewModel> subtitles, Subtitle subtitle, SubtitleFormat subtitleFormat);
    void SetColor(List<SubtitleLineViewModel> subtitles, Color color, Subtitle subtitle, SubtitleFormat subtitleFormat);
    string SetColorTag(string input, Color color, Subtitle subtitle, SubtitleFormat subtitleFormat);
    string RemoveColorTag(string input, Color color, Subtitle subtitle, SubtitleFormat subtitleFormat);
    bool ContainsColor(Color color, SubtitleLineViewModel subtitleLineViewModel, SubtitleFormat selectedSubtitleFormat);
    bool ContainsColor(Color color, string text, SubtitleFormat selectedSubtitleFormat);
}

public class ColorService : IColorService
{
    // Parsing a WebVTT header splits the whole style block into lines and re-reads every
    // "::cue(...)" rule. Set/remove color runs per selected line and asked for the styles up to
    // five times per line, so colorizing a large selection re-parsed the same header thousands
    // of times. Memo on the header instance: the only thing that changes it mid-batch is
    // AddStyleToHeader, which produces a new string and so misses the memo exactly once.
    // The returned list is only ever read - no WebVttHelper method mutates it.
    private string? _cachedStylesHeader;
    private List<WebVttStyle> _cachedStyles = new();

    private static bool HasWebVttHeader(string header)
    {
        return !string.IsNullOrEmpty(header) && header.Contains("WEBVTT");
    }

    private List<WebVttStyle> GetWebVttStyles(string header)
    {
        if (!ReferenceEquals(header, _cachedStylesHeader))
        {
            _cachedStyles = WebVttHelper.GetStyles(header);
            _cachedStylesHeader = header;
        }

        return _cachedStyles;
    }

    public void RemoveColorTags(List<SubtitleLineViewModel> subtitles, Subtitle subtitle, SubtitleFormat subtitleFormat)
    {
        foreach (var p in subtitles)
        {
            RemoveColorTags(p, subtitle, subtitleFormat);
        }
    }

    private void RemoveColorTags(SubtitleLineViewModel p, Subtitle subtitle, SubtitleFormat subtitleFormat)
    {
        if (subtitleFormat is WebVTT or WebVTTFileWithLineNumber)
        {
            var styles = GetWebVttStyles(subtitle.Header);
            foreach (var style in styles)
            {
                if (style.Color.HasValue &&
                    style.Bold == null &&
                    style.Italic == null &&
                    style.FontName == null &&
                    style.FontSize == null &&
                    style.ShadowColor == null &&
                    style.BackgroundColor == null &&
                    style.Underline == null &&
                    style.StrikeThrough == null)
                {
                    p.Text = WebVttHelper.RemoveColorTag(p.Text, style.Color.Value, styles);
                }
            }

            p.Text = WebVttHelper.RemoveDefaultColorClasses(p.Text);

            return;
        }

        if (!p.Text.Contains("<font", StringComparison.OrdinalIgnoreCase))
        {
            if (p.Text.Contains("\\c") || p.Text.Contains("\\1c"))
            {
                p.Text = HtmlUtil.RemoveAssaColor(p.Text);
            }
        }

        p.Text = HtmlUtil.RemoveColorTags(p.Text);
        p.Text = WebVttHelper.RemoveDefaultColorClasses(p.Text);
    }

    public void SetColor(List<SubtitleLineViewModel> subtitles, Color color, Subtitle subtitle, SubtitleFormat subtitleFormat)
    {
        foreach (var p in subtitles)
        {
            RemoveColorTags(p, subtitle, subtitleFormat);
            p.Text = SetColorTag(p.Text, color, subtitle, subtitleFormat);
        }
    }

    public string SetColorTag(string input, Color color, Subtitle subtitle, SubtitleFormat subtitleFormat)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return input;
        }

        var text = input;
        if (subtitleFormat is AdvancedSubStationAlpha)
        {
            try
            {
                text = HtmlUtil.RemoveAssaColor(text);
                text = "{\\" + AdvancedSubStationAlpha.GetSsaColorStringForEvent(color.ToSKColor()) + "&}" + text;
            }
            catch
            {
                // ignore
            }

            return text;
        }

        if (subtitleFormat is WebVTT or WebVTTFileWithLineNumber)
        {
            try
            {
                var hasWebVttHeader = HasWebVttHeader(subtitle.Header);
                var styles = hasWebVttHeader ? GetWebVttStyles(subtitle.Header) : new List<WebVttStyle>();
                var style = hasWebVttHeader ? WebVttHelper.GetOnlyColorStyle(color.ToSKColor(), styles) : null;
                if (style == null)
                {
                    style = WebVttHelper.AddStyleFromColor(color.ToSKColor());
                    subtitle.Header = WebVttHelper.AddStyleToHeader(subtitle.Header, style);
                    hasWebVttHeader = HasWebVttHeader(subtitle.Header);
                    styles = GetWebVttStyles(subtitle.Header);
                }

                text = WebVttHelper.AddStyleToText(text, style, styles);
                if (hasWebVttHeader && styles.Count > 1)
                {
                    text = WebVttHelper.RemoveUnusedColorStylesFromText(text, styles);
                }
            }
            catch
            {
                // ignore
            }

            return text;
        }

        // An STL file carries eight teletext colours, and Ebu.Save snaps whatever it finds to the
        // nearest of them - so a shortcut colour like orange was shown orange in the grid and in
        // the video preview and came out yellow in the file. Snap when the tag is written instead:
        // grid, preview and file then agree. Written as the colour name the STL reader itself
        // produces, so the shortcut also toggles off a colour that came from a file.
        var colorText = ToHex(color);
        if (subtitleFormat is Ebu)
        {
            colorText = Ebu.GetNearestColorName(colorText) ?? colorText;
        }
        else if (subtitleFormat is DvbTeletext)
        {
            // A .dvbttx colour map entry holds four bits per component (Level 2.5), so snap the
            // tag to that grid - grid, preview and the written file then agree, and the tag
            // matches what the teletext reader produces so ContainsColor can toggle it off.
            colorText = TeletextTables.ColorToHtml(TeletextColorMap.QuantizeRgb(color.R, color.G, color.B));
        }

        string pre = string.Empty;
        if (text.StartsWith("{\\", StringComparison.Ordinal) && text.IndexOf('}') >= 0)
        {
            int endIndex = text.IndexOf('}') + 1;
            pre = text.Substring(0, endIndex);
            text = text.Remove(0, endIndex);
        }

        string s = text;
        if (s.StartsWith("<font ", StringComparison.OrdinalIgnoreCase))
        {
            int end = s.IndexOf('>');
            if (end > 0)
            {
                string f = s.Substring(0, end);

                if (f.Contains(" face=", StringComparison.OrdinalIgnoreCase) && !f.Contains(" color=", StringComparison.OrdinalIgnoreCase))
                {
                    var start = s.IndexOf(" face=", StringComparison.OrdinalIgnoreCase);
                    s = s.Insert(start, string.Format(" color=\"{0}\"", colorText));
                    text = pre + s;
                    return text;
                }

                var colorStart = f.IndexOf(" color=", StringComparison.OrdinalIgnoreCase);
                if (colorStart >= 0)
                {
                    var valueStart = colorStart + " color=".Length;
                    var quoteEnd = s.IndexOf('"', valueStart);
                    if (quoteEnd > 0)
                    {
                        // Quoted value: the closing quote comes from the tail we keep.
                        s = s.Substring(0, colorStart) + string.Format(" color=\"{0}", colorText) + s.Substring(quoteEnd);
                    }
                    else
                    {
                        // Unquoted value ("<font color=red>"). "end" is the '>' here, so the old
                        // code emitted an opening quote and then kept the rest of the tag,
                        // producing the broken '<font color="#0000FF>'. Replace the whole
                        // unquoted value with a properly quoted one instead.
                        var valueEnd = valueStart;
                        while (valueEnd < end && !char.IsWhiteSpace(s[valueEnd]))
                        {
                            valueEnd++;
                        }

                        s = s.Substring(0, colorStart) + string.Format(" color=\"{0}\"", colorText) + s.Substring(valueEnd);
                    }

                    text = pre + s;
                    return text;
                }
            }
        }

        return $"{pre}<font color=\"{colorText}\">{text}</font>";
    }

    public string RemoveColorTag(string input, Color color, Subtitle subtitle, SubtitleFormat subtitleFormat)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return input;
        }

        var text = input;
        if (subtitleFormat is AdvancedSubStationAlpha)
        {
            try
            {
                text = HtmlUtil.RemoveAssaColor(text);
            }
            catch
            {
                // ignore
            }

            return text;
        }

        if (subtitleFormat is WebVTT or WebVTTFileWithLineNumber)
        {
            try
            {
                text = WebVttHelper.RemoveColorTag(text, color.ToSKColor(), GetWebVttStyles(subtitle.Header));
            }
            catch
            {
                // ignore
            }

            return text;
        }

        string pre = string.Empty;
        if (text.StartsWith("{\\", StringComparison.Ordinal) && text.IndexOf('}') >= 0)
        {
            int endIndex = text.IndexOf('}') + 1;
            pre = text.Substring(0, endIndex);
            text = text.Remove(0, endIndex);
        }

        string s = text;
        if (s.StartsWith("<font ", StringComparison.OrdinalIgnoreCase) && s.EndsWith("</font>"))
        {
            s = s.Substring(0, s.Length - 7);
            int end = s.IndexOf('>');
            if (end > 0)
            {
                var content = s.Remove(0, end + 1);
                return content;
            }
        }

        return text;
    }

    private string ToHex(Color color)
    {
        return $"#{color.R:X2}{color.G:X2}{color.B:X2}";
    }

    public bool ContainsColor(Color color, SubtitleLineViewModel subtitleLineViewModel, SubtitleFormat subtitleFormat)
    {
        return ContainsColor(color, subtitleLineViewModel.Text, subtitleFormat);
    }

    public bool ContainsColor(Color color, string text, SubtitleFormat subtitleFormat)
    {
        var tag = SetColorTag("ø", color, new Subtitle(), subtitleFormat);
        var colorStart = tag.Substring(0, tag.IndexOf('ø', StringComparison.Ordinal));
        return text.Contains(colorStart, StringComparison.OrdinalIgnoreCase);
    }
}
