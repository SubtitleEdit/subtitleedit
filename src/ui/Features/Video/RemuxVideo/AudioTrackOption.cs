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
            return $"Track #{Index + 1}: [{Language}] {langName} - {Details}";
        }
    }

    public static string GetLanguageDisplayName(string isoCode)
    {
        if (string.IsNullOrWhiteSpace(isoCode))
        {
            return "Chưa xác định (Undetermined)";
        }

        return isoCode.ToLowerInvariant() switch
        {
            "vie" or "vi" => "Tiếng Việt (Vietnamese)",
            "eng" or "en" => "Tiếng Anh (English)",
            "jpn" or "ja" => "Tiếng Nhật (Japanese)",
            "zho" or "chi" or "zh" => "Tiếng Trung (Chinese)",
            "kor" or "ko" => "Tiếng Hàn (Korean)",
            "fre" or "fra" or "fr" => "Tiếng Pháp (French)",
            "ger" or "deu" or "de" => "Tiếng Đức (German)",
            "spa" or "es" => "Tiếng Tây Ban Nha (Spanish)",
            "rus" or "ru" => "Tiếng Nga (Russian)",
            "ita" or "it" => "Tiếng Ý (Italian)",
            "por" or "pt" => "Tiếng Bồ Đào Nha (Portuguese)",
            "und" => "Chưa xác định (Undetermined)",
            _ => isoCode.ToUpperInvariant(),
        };
    }
}
