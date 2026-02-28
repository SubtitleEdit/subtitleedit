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
    void RemoveColorTags(List<SubtitleLineViewModel> subtitles);
    void SetColor(List<SubtitleLineViewModel> subtitles, Color color, Subtitle subtitle, SubtitleFormat subtitleFormat);
    string SetColorTag(string input, Color color, bool isAssa, bool isWebVtt, Subtitle subtitle);
    string RemoveColorTag(string input, Color color, bool isAssa, bool isWebVtt, Subtitle subtitle);
    bool ContainsColor(Color color, SubtitleLineViewModel subtitleLineViewModel, SubtitleFormat selectedSubtitleFormat);
    bool ContainsColor(Color color, string text, SubtitleFormat selectedSubtitleFormat);
}

public class ColorService : IColorService
{
    public void RemoveColorTags(List<SubtitleLineViewModel> subtitles)
    {
        foreach (var p in subtitles)
        {
            RemoveColorTags(p);
        }
    }

    private static void RemoveColorTags(SubtitleLineViewModel p)
    {
        if (!p.Text.Contains("<font", StringComparison.OrdinalIgnoreCase))
        {
            if (p.Text.Contains("\\c") || p.Text.Contains("\\1c"))
            {
                p.Text = HtmlUtil.RemoveAssaColor(p.Text);
            }
        }

        p.Text = HtmlUtil.RemoveColorTags(p.Text);
    }

    public void SetColor(List<SubtitleLineViewModel> subtitles, Color color, Subtitle subtitle, SubtitleFormat subtitleFormat)
    {
        var isAssa = subtitleFormat is AdvancedSubStationAlpha;
        var isWebVtt = subtitleFormat is WebVTT;

        foreach (var p in subtitles)
        {
            RemoveColorTags(p);
            p.Text = SetColorTag(p.Text, color, isAssa, isWebVtt, subtitle);
        }
    }

    public string SetColorTag(string input, Color color, bool isAssa, bool isWebVtt, Subtitle subtitle)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return input;
        }

        var text = input;
        if (isAssa)
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

        if (isWebVtt)
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

    public string RemoveColorTag(string input, Color color, bool isAssa, bool isWebVtt, Subtitle subtitle)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return input;
        }

        var text = input;
        if (isAssa)
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

        if (isWebVtt)
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
        var isAssa = subtitleFormat is AdvancedSubStationAlpha;
        var isWebVtt = subtitleFormat is WebVTT;

        var tag = SetColorTag("ø", color, isAssa, isWebVtt, new Subtitle());
        var colorStart = tag.Substring(0, tag.IndexOf('ø', StringComparison.Ordinal));
        return text.Contains(colorStart, StringComparison.OrdinalIgnoreCase);
    }
}
