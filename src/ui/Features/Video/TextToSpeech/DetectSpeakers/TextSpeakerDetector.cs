using Nikse.SubtitleEdit.Core.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Features.Video.TextToSpeech.DetectSpeakers;

/// <summary>
/// Finds SDH speaker tags written into the subtitle text - "MIKE: text", "[NARRATOR] text",
/// "(Speaker 1) text" - so the TTS flow can move them into the actor field and give each
/// speaker their own voice instead of reading the name aloud (issue #14106).
/// </summary>
public static partial class TextSpeakerDetector
{
    /// <summary>
    /// "NAME:" at the start of a line - one to three words of letters (plus ' - . for names like
    /// O'Brien or Mr. Smith), capped at name length so "Warning: do not open the door" is not a
    /// speaker. The tag may sit alone on its line with the speech on the next line.
    /// </summary>
    [GeneratedRegex(@"^(?<name>\p{Lu}[\p{L}'.\-]*(?:[ ]\p{L}[\p{L}'.\-]*){0,2}):\s*(?<text>.*)$",
        RegexOptions.CultureInvariant)]
    private static partial Regex ColonTagRegex();

    /// <summary>"[NAME]" or "(NAME)" at the start of a line.</summary>
    [GeneratedRegex(@"^(?:\[(?<name>[^\]\r\n]{1,30})\]|\((?<name>[^)\r\n]{1,30})\))\s*(?<text>.*)$",
        RegexOptions.CultureInvariant)]
    private static partial Regex BracketTagRegex();

    private const int MaxNameLength = 30;

    /// <summary>
    /// Splits one text line into its speaker tag and the words actually spoken.
    /// A bracketed tag with nothing after it on the line is only a speaker when
    /// <paramref name="hasFollowingLine"/> - alone it is a sound annotation ("[door slams]"),
    /// which is the noise prompt's business, not a name.
    /// </summary>
    public static bool TrySplit(string line, bool hasFollowingLine, out string speaker, out string spokenText)
    {
        speaker = string.Empty;
        spokenText = line;
        if (string.IsNullOrWhiteSpace(line))
        {
            return false;
        }

        var s = line.Trim();

        var match = BracketTagRegex().Match(s);
        if (!match.Success)
        {
            match = ColonTagRegex().Match(s);
        }

        if (!match.Success)
        {
            return false;
        }

        var name = NormalizeSpaces(match.Groups["name"].Value);
        var text = match.Groups["text"].Value.Trim();
        if (name.Length == 0 || name.Length > MaxNameLength)
        {
            return false;
        }

        if (text.Length == 0 && !hasFollowingLine)
        {
            return false;
        }

        speaker = name;
        spokenText = text;
        return true;
    }

    /// <summary>
    /// A name written the SDH way - ALL UPPERCASE, or a bracketed "Speaker N" from diarization -
    /// is confidently a speaker; anything else ("Warning:", "Note:") is listed for the user to
    /// judge, but not pre-checked.
    /// </summary>
    public static bool IsConfidentSpeakerName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return false;
        }

        if (Regex.IsMatch(name, @"^speaker[ _-]*\d+$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
        {
            return true;
        }

        return name.Any(char.IsLetter) && !name.Any(char.IsLower);
    }

    public static List<TextSpeakerCandidate> Detect(Subtitle subtitle)
    {
        var candidates = new List<TextSpeakerCandidate>();
        foreach (var paragraph in subtitle.Paragraphs)
        {
            var lines = (paragraph.Text ?? string.Empty).SplitToLines();
            for (var i = 0; i < lines.Count; i++)
            {
                if (TrySplit(lines[i], i + 1 < lines.Count, out var speaker, out _))
                {
                    candidates.Add(new TextSpeakerCandidate(paragraph, speaker, i));
                }
            }
        }

        return candidates;
    }

    /// <summary>
    /// Puts the confirmed speakers into the actor field and takes their tags out of the text, in
    /// place. With <paramref name="stickySpeakers"/>, a line without a tag continues the previous
    /// speaker - the SDH convention of naming only the changes (issue #14106). A paragraph with
    /// two different confirmed speakers gets the first as its actor; the voice can only be one.
    /// </summary>
    public static int Apply(Subtitle subtitle, IReadOnlyCollection<TextSpeakerCandidate> confirmed, bool stickySpeakers)
    {
        var byParagraph = confirmed
            .GroupBy(c => c.Paragraph)
            .ToDictionary(g => g.Key, g => g.OrderBy(c => c.LineIndex).ToList());

        var applied = 0;
        var previousSpeaker = string.Empty;
        foreach (var paragraph in subtitle.Paragraphs)
        {
            if (!byParagraph.TryGetValue(paragraph, out var tags))
            {
                if (stickySpeakers && previousSpeaker.Length > 0 && string.IsNullOrWhiteSpace(paragraph.Actor))
                {
                    paragraph.Actor = previousSpeaker;
                }

                continue;
            }

            var confirmedLineIndexes = tags.Select(t => t.LineIndex).ToHashSet();
            var lines = (paragraph.Text ?? string.Empty).SplitToLines();
            var newLines = new List<string>();
            for (var i = 0; i < lines.Count; i++)
            {
                if (confirmedLineIndexes.Contains(i) &&
                    TrySplit(lines[i], i + 1 < lines.Count, out _, out var spokenText))
                {
                    if (spokenText.Length > 0)
                    {
                        newLines.Add(spokenText);
                    }
                }
                else
                {
                    newLines.Add(lines[i]);
                }
            }

            paragraph.Actor = tags[0].Speaker;
            paragraph.Text = string.Join(Environment.NewLine, newLines).Trim();
            previousSpeaker = tags[tags.Count - 1].Speaker;
            applied++;
        }

        return applied;
    }

    private static string NormalizeSpaces(string s)
    {
        return string.Join(" ", s.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));
    }
}

/// <summary>One speaker tag found in the text: which paragraph, which of its lines, whose name.</summary>
public sealed class TextSpeakerCandidate
{
    public Paragraph Paragraph { get; }
    public string Speaker { get; }
    public int LineIndex { get; }

    public TextSpeakerCandidate(Paragraph paragraph, string speaker, int lineIndex)
    {
        Paragraph = paragraph;
        Speaker = speaker;
        LineIndex = lineIndex;
    }
}
