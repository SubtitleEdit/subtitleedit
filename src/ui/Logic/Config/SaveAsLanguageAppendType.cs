namespace Nikse.SubtitleEdit.Logic.Config;

public enum SaveAsLanguageAppendType
{
    None,
    TwoLetterLanguageCode,
    // ISO 639-2/T (terminology). Enum name kept as-is so existing persisted settings
    // continue to round-trip; the user-facing label was updated to "(ISO 639-2/T)".
    ThreeLEtterLanguageCode,
    // ISO 639-2/B (bibliographic) — uses the English-tradition codes (fre, ger, dut, ...)
    // for the ~20 languages where they differ from the terminology form.
    ThreeLetterLanguageCodeBibliographic,
    FullLanguageName,
}
