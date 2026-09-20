using System;
using System.Linq;
using Nikse.SubtitleEdit.Core.Common;

namespace Nikse.SubtitleEdit.Features.Video.RemuxVideo;

public class AudioTrackOption
{
    public int Index { get; set; }
    public string Language { get; set; } = string.Empty;
    public string Details { get; set; } = string.Empty;

    public string DisplayName
    {
        get
        {
            var langName = GetLanguageDisplayName(Language);
            var langCode = string.IsNullOrWhiteSpace(Language) ? "und" : Language;
            return string.IsNullOrWhiteSpace(Details)
                ? $"Track #{Index + 1}: [{langCode}] {langName}"
                : $"Track #{Index + 1}: [{langCode}] {langName} - {Details}";
        }
    }

    public static string GetLanguageDisplayName(string isoCode)
    {
        if (string.IsNullOrWhiteSpace(isoCode) || isoCode.Equals("und", StringComparison.OrdinalIgnoreCase))
        {
            return "Undetermined";
        }

        var lang = Iso639Dash2LanguageCode.List.FirstOrDefault(p =>
            p.ThreeLetterCode.Equals(isoCode, StringComparison.OrdinalIgnoreCase) ||
            p.TwoLetterCode.Equals(isoCode, StringComparison.OrdinalIgnoreCase));

        return !string.IsNullOrEmpty(lang?.EnglishName) ? lang.EnglishName : isoCode.ToUpperInvariant();
    }
}

