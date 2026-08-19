using Nikse.SubtitleEdit.Core.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Features.Video.TextToSpeech.AutoCast;

/// <summary>
/// Reads the speaker a diarizing speech-to-text engine wrote in front of each line.
/// </summary>
/// <remarks>
/// Diarization comes back as text: MOSS-Transcribe-Diarize writes "(Speaker 1) Hello there." and
/// other engines write "[SPEAKER 2]" or "SPEAKER_00:". That is fine to read but wrong to keep - a
/// subtitle whose text starts with the speaker's name gets it spoken out loud by every TTS engine,
/// and no dialogue format expects it there. So the label comes out of the text and into the
/// paragraph's actor field, which is where Subtitle Edit's cast already looks for it.
/// </remarks>
public static partial class SpeakerLabelParser
{
    /// <summary>
    /// "(Speaker 1)", "[SPEAKER 2]", "SPEAKER_00:", "Speaker 3:" - bracketed or followed by a
    /// colon, at the very start of the line. Anything else is left alone: a line that merely
    /// mentions a speaker is not a label, and guessing wrong silently renames somebody's dialogue.
    /// </summary>
    [GeneratedRegex(@"^\s*(?:[\(\[]\s*(?<name>speaker[ _-]*\d+)\s*[\)\]]|(?<name>speaker[ _-]*\d+)\s*:)\s*",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex SpeakerPrefixRegex();

    /// <summary>
    /// Splits a diarized line into its speaker and the words actually spoken.
    /// </summary>
    /// <returns>False when the line carries no speaker label, leaving the text untouched.</returns>
    public static bool TrySplit(string? text, out string speaker, out string spokenText)
    {
        speaker = string.Empty;
        spokenText = text ?? string.Empty;
        if (string.IsNullOrWhiteSpace(text))
        {
            return false;
        }

        var match = SpeakerPrefixRegex().Match(text);
        if (!match.Success)
        {
            return false;
        }

        speaker = Normalize(match.Groups["name"].Value);
        spokenText = text[match.Length..].TrimStart();
        return true;
    }

    /// <summary>
    /// "SPEAKER_00", "speaker 1" and "Speaker-1" are the same speaker written three ways; this is
    /// the one spelling used as the actor name.
    /// </summary>
    private static string Normalize(string rawName)
    {
        var digits = new string(rawName.Where(char.IsDigit).ToArray()).TrimStart('0');
        return "Speaker " + (digits.Length == 0 ? "0" : digits);
    }

    /// <summary>
    /// Moves every speaker label out of <paramref name="subtitle"/>'s text and into
    /// <see cref="Paragraph.Actor"/>, in place.
    /// </summary>
    /// <returns>How many lines carried a label.</returns>
    public static int MoveLabelsToActors(Subtitle subtitle)
    {
        var moved = 0;
        foreach (var paragraph in subtitle.Paragraphs)
        {
            if (!TrySplit(paragraph.Text, out var speaker, out var spokenText))
            {
                continue;
            }

            paragraph.Actor = speaker;
            paragraph.Text = spokenText;
            moved++;
        }

        return moved;
    }

    /// <summary>
    /// The speaker each of <paramref name="lines"/> belongs to, taken from the diarized
    /// <paramref name="segments"/> by how much time the two share.
    /// </summary>
    /// <remarks>
    /// This is the path for a subtitle that already exists: the user's own lines and translation
    /// are kept, and diarization is used only to say who speaks them. Overlap rather than
    /// start-time nearest, because a subtitle line and a diarized segment rarely start together -
    /// what matters is which speaker is talking for most of the line. Lines that overlap nothing
    /// (music, on-screen text, a line before the first segment) are left without an actor rather
    /// than guessed at.
    /// </remarks>
    public static Dictionary<Paragraph, string> AssignSpeakersByOverlap(
        IReadOnlyList<Paragraph> lines,
        IReadOnlyList<Paragraph> segments)
    {
        var byLine = new Dictionary<Paragraph, string>();
        var labelled = segments
            .Select(s => (Speaker: TrySplit(s.Text, out var speaker, out _) ? speaker : string.Empty, Segment: s))
            .Where(s => !string.IsNullOrEmpty(s.Speaker))
            .ToList();
        if (labelled.Count == 0)
        {
            return byLine;
        }

        foreach (var line in lines)
        {
            var bySpeaker = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase);
            foreach (var (speaker, segment) in labelled)
            {
                var overlap = Math.Min(line.EndTime.TotalMilliseconds, segment.EndTime.TotalMilliseconds)
                              - Math.Max(line.StartTime.TotalMilliseconds, segment.StartTime.TotalMilliseconds);
                if (overlap <= 0)
                {
                    continue;
                }

                bySpeaker.TryGetValue(speaker, out var running);
                bySpeaker[speaker] = running + overlap;
            }

            if (bySpeaker.Count > 0)
            {
                byLine[line] = bySpeaker.OrderByDescending(p => p.Value).First().Key;
            }
        }

        return byLine;
    }
}
