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
    /// One prompt is deliberately shared by all four models rather than tuned per model. Only
    /// GLM-OCR - the default and top-ranked engine - is prompt-sensitive here; the other three
    /// merge 70-93% of two-line subtitles under every prompt tried, so per-model tuning was
    /// measured to buy 3 line breaks in 120 image-model pairs, against shared-prompt plumbing at
    /// four OCR entry points and a prompt box that would change under the user on every model
    /// switch.
    /// </para>
    /// </summary>
    public const string LlamaCppOcrPrompt =
        "Identify the number of lines, then extract the text of each line exactly as written. The language is {language}.";
}
