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
    /// obeys this prompt too literally: <see cref="LlamaCppOcrPromptLfmVl"/>, applied through
    /// <see cref="Nikse.SubtitleEdit.UiLogic.LlamaCpp.LlamaCppServerManager.ResolveOcrPrompt"/>
    /// only while this shared prompt is unedited, so the prompt box never changes under the user.
    /// </para>
    /// </summary>
    public const string LlamaCppOcrPrompt =
        "Identify the number of lines, then extract the text of each line exactly as written. The language is {language}.";

    /// <summary>
    /// Prompt for Liquid AI's LFM vision models (LFM3-VL, and LFM2.5-VL before it), general
    /// instruction-following models rather than OCR specialists. Given <see cref="LlamaCppOcrPrompt"/>
    /// they do what they are told and answer the "identify the number of lines" clause literally -
    /// LFM2.5-VL starts every result with a bare "2" line, LFM3-VL with "There are two lines of text
    /// in the image" - and they drop ♪ note marks unless the prompt names them. Measured 2026-09-05
    /// on LFM2.5-VL (14 synthetic images: 13/14 exact with this wording, 0/14 with the shared
    /// prompt) and 2026-10-29 on LFM3-VL (19 synthetic: 18/19 vs 0/19; 220 real Blu-ray images:
    /// 214/220). Also given to a self-supplied LFM vision model - see
    /// <see cref="Nikse.SubtitleEdit.UiLogic.LlamaCpp.LlamaCppServerManager.CreateCustomOcrModel"/>.
    /// Used only when the user has not edited the shared prompt - see
    /// <see cref="Nikse.SubtitleEdit.UiLogic.LlamaCpp.LlamaCppServerManager.ResolveOcrPrompt"/>.
    /// </summary>
    public const string LlamaCppOcrPromptLfmVl =
        "Transcribe the subtitle in the image exactly as written, one output line per line of text in the image, keeping symbols such as ♪. Output only the text. The language is {language}.";
}
