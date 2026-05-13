using Avalonia.Media;
using Avalonia.Skia;
using Nikse.SubtitleEdit.Core.Common;
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
    public void RemoveColorTags(List<SubtitleLineViewModel> subtitles, Subtitle subtitle, SubtitleFormat subtitleFormat)
    {
        foreach (var p in subtitles)
        {
            RemoveColorTags(p, subtitle, subtitleFormat);
        }
    }

    private static void RemoveColorTags(SubtitleLineViewModel p, Subtitle subtitle, SubtitleFormat subtitleFormat)
    {
        if (subtitleFormat is WebVTT or WebVTTFileWithLineNumber)
        {
            var styles = WebVttHelper.GetStyles(subtitle.Header);
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

        if (subtitleFormat is WebVTT)
        {
            try
            {
                var existingStyle = WebVttHelper.GetOnlyColorStyle(color.ToSKColor(), subtitle.Header);
                if (existingStyle != null)
                {
                    text = WebVttHelper.AddStyleToText(text, existingStyle, WebVttHelper.GetStyles(subtitle.Header));
                    text = WebVttHelper.RemoveUnusedColorStylesFromText(text, subtitle.Header);
                }
                else
                {
                    var styleWithColor = WebVttHelper.AddStyleFromColor(color.ToSKColor());
                    subtitle.Header = WebVttHelper.AddStyleToHeader(subtitle.Header, styleWithColor);
                    text = WebVttHelper.AddStyleToText(text, styleWithColor, WebVttHelper.GetStyles(subtitle.Header));
                    text = WebVttHelper.RemoveUnusedColorStylesFromText(text, subtitle.Header);
                }
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
        if (s.StartsWith("<font ", StringComparison.OrdinalIgnoreCase))
        {
            int end = s.IndexOf('>');
            if (end > 0)
            {
                string f = s.Substring(0, end);

                if (f.Contains(" face=", StringComparison.OrdinalIgnoreCase) && !f.Contains(" color=", StringComparison.OrdinalIgnoreCase))
                {
                    var start = s.IndexOf(" face=", StringComparison.OrdinalIgnoreCase);
                    s = s.Insert(start, string.Format(" color=\"{0}\"", ToHex(color)));
                    text = pre + s;
                    return text;
                }

                var colorStart = f.IndexOf(" color=", StringComparison.OrdinalIgnoreCase);
                if (colorStart >= 0)
                {
                    if (s.IndexOf('"', colorStart + 8) > 0)
                    {
                        end = s.IndexOf('"', colorStart + 8);
                    }

                    s = s.Substring(0, colorStart) + string.Format(" color=\"{0}", ToHex(color)) + s.Substring(end);
                    text = pre + s;
                    return text;
                }
            }
        }

        return $"{pre}<font color=\"{ToHex(color)}\">{text}</font>";
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

        if (subtitleFormat is WebVTT)
        {
            try
            {
                text = WebVttHelper.RemoveColorTag(text, color.ToSKColor(), WebVttHelper.GetStyles(subtitle.Header));
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
