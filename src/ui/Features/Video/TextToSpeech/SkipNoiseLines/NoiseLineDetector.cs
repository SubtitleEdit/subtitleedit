using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Video.SpeechToText;
using System.Collections.Generic;
using System.Linq;

namespace Nikse.SubtitleEdit.Features.Video.TextToSpeech.SkipNoiseLines;

/// <summary>
/// Finds subtitle lines that carry no speech - SDH sound and music annotations like "♪",
/// "[door slams]" or "(sighs)", and lines that are empty once formatting tags are stripped.
/// Sent to a TTS engine, such lines get read aloud or hallucinated into made-up words
/// (issue #14106); the generate flow offers to leave them silent instead.
/// </summary>
public static class NoiseLineDetector
{
    /// <summary>
    /// True when the line holds nothing a voice should say: whitespace, tags only, or only
    /// bracketed/parenthesised groups and music symbols. A sound annotation followed by real
    /// speech ("[gunshot] Get down!") is speech and is kept.
    /// </summary>
    public static bool IsNoiseOnly(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return true;
        }

        if (HtmlUtil.RemoveHtmlTags(text, true).Trim().Length == 0)
        {
            return true;
        }

        return SpeechToTextQualityReport.IsNonSpeechLine(text);
    }

    public static List<Paragraph> Detect(Subtitle subtitle)
    {
        return subtitle.Paragraphs.Where(p => IsNoiseOnly(p.Text)).ToList();
    }
}
