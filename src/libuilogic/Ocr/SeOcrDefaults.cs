namespace Nikse.SubtitleEdit.UiLogic.Ocr;

/// <summary>
/// Defaults shared by every caller of the llama.cpp OCR engines, so the UI settings default, the
/// engine's own fallback and seconv cannot drift apart (they were three hand-kept copies of the
/// same string).
/// </summary>
public static class SeOcrDefaults
{
    /// <summary>
    /// Prompt sent to the llama.cpp vision OCR models (GLM-OCR, PaddleOCR-VL, HunyuanOCR,
    /// LightOnOCR); <c>{language}</c> is replaced with the OCR language's English name.
    /// <para>
    /// Making the model count the lines before transcribing is what keeps two-line subtitles from
    /// being merged into one: these models work line by line and reconstruct the break from
    /// context alone, so an instruction to "preserve line breaks" gives them nothing to preserve
    /// (#14221 - thanks tekk42). Measured 2026-08-28 on 134 subtitle bitmaps from four Blu-ray
    /// .sup files (67 of them two-line) against GLM-OCR 0.9B Q8_0 with SE's own flags and square
    /// padding: the previous prompt ("Extract all text exactly as written. The language is
    /// {language}. Preserve line breaks.") lost 8 line breaks, this one loses 2. Keeping "exactly
    /// as written" matters - the issue's wording without it lost 3, and dropping it also cost
    /// PaddleOCR-VL 4 more breaks on a 30-image subset. Output is otherwise identical: across all
    /// 134 images every candidate prompt returned character-identical text once newlines are
    /// flattened, and results were byte-identical over three repeats, so the prompt moves line
    /// breaks and nothing else.
    /// </para>
    /// <para>
    /// One prompt is deliberately shared by the OCR-specialist models rather than tuned per model.
    /// Only GLM-OCR - the default and top-ranked engine - is prompt-sensitive here; PaddleOCR-VL,
    /// HunyuanOCR and LightOnOCR merge 70-93% of two-line subtitles under every prompt tried, so
    /// per-model tuning was measured to buy 3 line breaks in 120 image-model pairs, against
    /// shared-prompt plumbing at four OCR entry points and a prompt box that would change under
    /// the user on every model switch. The one exception is a general-purpose vision model that
    /// obeys this prompt too literally: <see cref="LlamaCppOcrPromptLfm25Vl"/>, applied through
    /// <see cref="Nikse.SubtitleEdit.UiLogic.LlamaCpp.LlamaCppServerManager.ResolveOcrPrompt"/>
    /// only while this shared prompt is unedited, so the prompt box never changes under the user.
    /// </para>
    /// </summary>
    public const string LlamaCppOcrPrompt =
        "Identify the number of lines, then extract the text of each line exactly as written. The language is {language}.";

    /// <summary>
    /// Prompt for LFM2.5-VL-3B, a general instruction-following vision model rather than an OCR
    /// specialist. Given <see cref="LlamaCppOcrPrompt"/> it does what it is told and answers the
    /// "identify the number of lines" clause literally - every result starts with a bare "2" line -
    /// and it drops ♪ note marks unless the prompt names them. Measured 2026-09-05 on the 14-image
    /// subtitle corpus: this wording scores 13/14 exact, 14/14 text-only, 0% character error and
    /// keeps every line break and note mark; the shared prompt scores 0/14 exact (the count prefix).
    /// Used only when the user has not edited the shared prompt - see
    /// <see cref="Nikse.SubtitleEdit.UiLogic.LlamaCpp.LlamaCppServerManager.ResolveOcrPrompt"/>.
    /// </summary>
    public const string LlamaCppOcrPromptLfm25Vl =
        "Transcribe the subtitle in the image exactly as written, one output line per line of text in the image, keeping symbols such as ♪. Output only the text. The language is {language}.";
}
