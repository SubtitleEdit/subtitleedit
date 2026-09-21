using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Ocr;
using Nikse.SubtitleEdit.UiLogic.BatchConvert;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Nikse.SubtitleEdit.Features.Tools.BatchConvert;

/// <summary>
/// The OCR language of one batch item. A batch often holds the tracks of one disc in several
/// languages ("movie [eng].sup", "movie [fra].sup", ...), so the language the source declares -
/// container track language, idx language, or a language tag in the file name, like the OCR
/// window uses - wins over the single language from the batch settings. The configured language
/// stays the fallback when the source declares nothing or the engine lacks that language.
/// </summary>
internal static class BatchOcrLanguage
{
    internal static Iso639Dash2LanguageCode? ResolveSourceLanguage(BatchConvertItem item, IReadOnlyCollection<string>? idxLanguageCodes = null)
    {
        // A container track knows its own language; the file name then describes the whole
        // container, not the track.
        if (!string.IsNullOrEmpty(item.LanguageCode))
        {
            return OcrViewModel.ResolveIsoLanguage(item.LanguageCode);
        }

        // All tracks of a multi-language idx are still converted as one, so only a single
        // language is a usable hint.
        var idxCodes = idxLanguageCodes?.Where(p => !string.IsNullOrEmpty(p)).Distinct().ToList();
        if (idxCodes?.Count == 1)
        {
            var idxLanguage = OcrViewModel.ResolveIsoLanguage(idxCodes[0]);
            if (idxLanguage != null)
            {
                return idxLanguage;
            }
        }

        return OcrViewModel.ResolveIsoLanguage(OcrViewModel.DetectLanguageCodeFromFileName(item.FileName));
    }

    /// <summary>Tesseract model name ("fra") - only when that model is installed.</summary>
    internal static string ForTesseract(Iso639Dash2LanguageCode? source, string configured, string modelFolder)
    {
        if (source == null || string.IsNullOrEmpty(modelFolder))
        {
            return configured;
        }

        return File.Exists(Path.Combine(modelFolder, source.ThreeLetterCode + ".traineddata"))
            ? source.ThreeLetterCode
            : configured;
    }

    /// <summary>Two-letter code, when the engine lists it (PaddleOCR).</summary>
    internal static string ForTwoLetterEngine(Iso639Dash2LanguageCode? source, string configured, IEnumerable<string> engineLanguageCodes)
    {
        if (source == null)
        {
            return configured;
        }

        return engineLanguageCodes.FirstOrDefault(p => p.Equals(source.TwoLetterCode, StringComparison.OrdinalIgnoreCase)) ?? configured;
    }

    /// <summary>
    /// BCP-47 tag ("fr-FR") of an engine that lists regional tags (Apple Vision). The configured
    /// tag is kept when it already is of the source language, so "en-GB" is not replaced by "en-US".
    /// </summary>
    internal static string ForBcp47Engine(Iso639Dash2LanguageCode? source, string configured, IEnumerable<string> engineLanguageCodes)
    {
        if (source == null || LanguagePart(configured).Equals(source.TwoLetterCode, StringComparison.OrdinalIgnoreCase))
        {
            return configured;
        }

        return engineLanguageCodes.FirstOrDefault(p => LanguagePart(p).Equals(source.TwoLetterCode, StringComparison.OrdinalIgnoreCase)) ?? configured;
    }

    /// <summary>English language name for the prompt of the vision model engines.</summary>
    internal static string ForLanguageNameEngine(Iso639Dash2LanguageCode? source, string configured)
    {
        return source?.EnglishName ?? configured;
    }

    private static string LanguagePart(string? bcp47)
    {
        if (string.IsNullOrEmpty(bcp47))
        {
            return string.Empty;
        }

        var dash = bcp47.IndexOf('-');
        return dash < 0 ? bcp47 : bcp47.Substring(0, dash);
    }
}
