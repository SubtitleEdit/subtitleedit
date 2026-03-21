using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Features.Main;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Logic;

public interface IFontNameService
{
    void RemoveFontNames(SubtitleLineViewModel p, bool isAssa);
    void RemoveFontNames(List<SubtitleLineViewModel> subtitles, SubtitleFormat subtitleFormat);
    void SetFontName(List<SubtitleLineViewModel> subtitles, string fontName, SubtitleFormat subtitleFormat);
    string SetFontName(string text, string fontName, bool isAssa);
}

public class FontNameService : IFontNameService
{
    public void RemoveFontNames(List<SubtitleLineViewModel> subtitles, SubtitleFormat subtitleFormat)
    {
        var isAssa = subtitleFormat is AdvancedSubStationAlpha;

        foreach (var p in subtitles)
        {
            RemoveFontNames(p, isAssa);
        }
    }

    public void SetFontName(List<SubtitleLineViewModel> subtitles, string fontName, SubtitleFormat subtitleFormat)
    {
        var isAssa = subtitleFormat is AdvancedSubStationAlpha;

        foreach (var p in subtitles)
        {
            p.Text = SetFontName(p.Text, fontName, isAssa);
        }
    }

    public string SetFontName(string input, string fontName, bool isAssa)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return input;
        }

        var text = input;
        if (isAssa)
        {
            text = Regex.Replace(text, "{\\\\fn[^\\\\]+}", string.Empty);
            text = Regex.Replace(text, "\\\\fn[a-zA-Z \\d]+\\\\", string.Empty);
            text = "{\\fn" + fontName + "}" + text;
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
            var end = s.IndexOf('>');
            if (end > 0)
            {
                var f = s.Substring(0, end);

                if (f.Contains(" color=", StringComparison.OrdinalIgnoreCase) && !f.Contains(" face=", StringComparison.OrdinalIgnoreCase))
                {
                    var start = s.IndexOf(" color=", StringComparison.OrdinalIgnoreCase);
                    text = pre + s.Insert(start, string.Format(" face=\"{0}\"", fontName));
                    return text;
                }

                var faceStart = f.IndexOf(" face=", StringComparison.OrdinalIgnoreCase);
                if (f.Contains(" face=", StringComparison.OrdinalIgnoreCase))
                {
                    if (s.IndexOf('"', faceStart + 7) > 0)
                    {
                        end = s.IndexOf('"', faceStart + 7);
                    }

                    text = pre + s.Substring(0, faceStart) + string.Format(" face=\"{0}", fontName) + s.Substring(end);
                    return text;
                }
            }
        }

        return $"{pre}<font face=\"{fontName}\">{s}</font>";
    }

    public void RemoveFontNames(SubtitleLineViewModel p, bool isAssa)
    {

    }
}
