using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.Forms;
using Nikse.SubtitleEdit.Logic.Config;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Nikse.SubtitleEdit.Features.Main.AssistedMove;

/// <summary>
/// Builds the ranked list of move suggestions shown in the "Assisted move" window.
///
/// The list is context aware: word/fragment moves across a subtitle boundary are only
/// offered when the sentence actually continues across that boundary - moving words into
/// a neighbor that starts a new sentence (or a new dialog) would corrupt both lines, so
/// those directions are left out entirely. A subtitle that continues into the next only
/// gets "with next" options; one that continues from the previous only gets "with
/// previous" options. Word moves use the real <see cref="MoveWordUpDown"/> logic (tag
/// handling and auto-break included), so the previews match exactly what applying does.
/// </summary>
public static class AssistedMoveCandidateGenerator
{
    private const int MaxCandidates = 6;
    private const string SentenceEndChars = ".!?…。！？؟";
    private const string ClosingChars = "\"'”’»)]";

    public static List<AssistedMoveCandidate> Generate(
        SubtitleLineViewModel current,
        SubtitleLineViewModel? previous,
        SubtitleLineViewModel? next,
        string languageCode)
    {
        var result = new List<AssistedMoveCandidate>();
        var currentText = (current.Text ?? string.Empty).Trim();
        if (currentText.Length == 0)
        {
            return result;
        }

        var seen = new HashSet<string>();

        void TryAdd(AssistedMoveCandidate? candidate)
        {
            if (candidate == null || result.Count >= MaxCandidates)
            {
                return;
            }

            if (!seen.Add(candidate.Kind + "|" + candidate.NewCurrentText + "|" + candidate.NewOtherText))
            {
                return;
            }

            candidate.Number = result.Count + 1;
            result.Add(candidate);
        }

        var continuesToNext = next != null && SentenceContinues(currentText, next.Text ?? string.Empty);
        var continuesFromPrevious = previous != null && SentenceContinues(previous.Text ?? string.Empty, currentText);

        if (continuesToNext && next != null)
        {
            TryAdd(MakeFragmentMove(current, next, moveDown: true, AssistedMoveKind.WithNext,
                Se.Language.General.MoveUnfinishedSentenceToNextSubtitle, languageCode));
            TryAdd(MakeBalancedMove(current, next, AssistedMoveKind.WithNext,
                Se.Language.General.BalanceWithNextSubtitle, languageCode));
            TryAdd(MakeWordMove(current, next, moveDown: true, AssistedMoveKind.WithNext,
                Se.Language.Options.Shortcuts.MoveLastWordToNextSubtitle, languageCode));
            TryAdd(MakeWordMove(current, next, moveDown: false, AssistedMoveKind.WithNext,
                Se.Language.General.FetchFirstWordFromNextSubtitle, languageCode));
            TryAdd(MakeFragmentMove(current, next, moveDown: false, AssistedMoveKind.WithNext,
                Se.Language.General.FetchRestOfSentenceFromNextSubtitle, languageCode));
        }

        if (continuesFromPrevious && previous != null)
        {
            TryAdd(MakeFragmentMove(previous, current, moveDown: false, AssistedMoveKind.WithPrevious,
                Se.Language.General.MoveRestOfSentenceToPreviousSubtitle, languageCode));
            TryAdd(MakeBalancedMove(previous, current, AssistedMoveKind.WithPrevious,
                Se.Language.General.BalanceWithPreviousSubtitle, languageCode));
            TryAdd(MakeWordMove(previous, current, moveDown: false, AssistedMoveKind.WithPrevious,
                Se.Language.General.MoveFirstWordToPreviousSubtitle, languageCode));
            TryAdd(MakeWordMove(previous, current, moveDown: true, AssistedMoveKind.WithPrevious,
                Se.Language.General.FetchLastWordFromPreviousSubtitle, languageCode));
            TryAdd(MakeFragmentMove(previous, current, moveDown: true, AssistedMoveKind.WithPrevious,
                Se.Language.General.FetchUnfinishedSentenceFromPreviousSubtitle, languageCode));
        }

        TryAdd(MakeWithin(current, moveDown: true, languageCode,
            Se.Language.Options.Shortcuts.MoveLastWordFromFirstLineDownCurrentSubtitle));
        TryAdd(MakeWithin(current, moveDown: false, languageCode,
            Se.Language.Options.Shortcuts.MoveFirstWordFromNextLineUpCurrentSubtitle));

        return result;
    }

    /// <summary>
    /// True when the text of <paramref name="firstText"/> flows on into
    /// <paramref name="secondText"/> - i.e. the first does not finish its sentence, and the
    /// second does not open a new dialog. An ellipsis ending still continues when the next
    /// part starts with a lowercase letter (the "…" / "..." line-continuation convention).
    /// </summary>
    private static bool SentenceContinues(string firstText, string secondText)
    {
        var a = HtmlUtil.RemoveHtmlTags(firstText, true).TrimEnd();
        var b = HtmlUtil.RemoveHtmlTags(secondText, true).TrimStart();
        if (a.Length == 0 || b.Length == 0)
        {
            return false;
        }

        if (b.StartsWith('-') || b.StartsWith('–'))
        {
            return false; // the next part opens a dialog - never merge words into it
        }

        var endsWithEllipsis = a.EndsWith('…') || a.EndsWith("...");
        a = a.TrimEnd(ClosingChars.ToCharArray()).TrimEnd();
        if (a.Length == 0)
        {
            return false;
        }

        if (!SentenceEndChars.Contains(a[^1]))
        {
            return true;
        }

        var firstLetter = b.FirstOrDefault(char.IsLetter);
        return endsWithEllipsis && firstLetter != default && char.IsLower(firstLetter);
    }

    /// <summary>
    /// A single-word move between two subtitles. <paramref name="first"/> is always the
    /// earlier subtitle: (current, next) for WithNext, (previous, current) for WithPrevious.
    /// </summary>
    private static AssistedMoveCandidate? MakeWordMove(
        SubtitleLineViewModel first,
        SubtitleLineViewModel second,
        bool moveDown,
        AssistedMoveKind kind,
        string title,
        string languageCode)
    {
        var firstText = (first.Text ?? string.Empty).Trim();
        var secondText = (second.Text ?? string.Empty).Trim();

        var upDown = new MoveWordUpDown(firstText, secondText);
        if (moveDown)
        {
            upDown.MoveWordDown();
        }
        else
        {
            upDown.MoveWordUp();
        }

        if (upDown.S1 == firstText && upDown.S2 == secondText)
        {
            return null;
        }

        return MakeBetweenCandidate(first, second, upDown.S1, upDown.S2, kind, title, languageCode);
    }

    /// <summary>
    /// Moves as many words as it takes across the boundary - in whichever direction the
    /// text is heavier - so both subtitles end up with roughly the same amount of text.
    /// Word by word via <see cref="MoveWordUpDown"/>, so tag handling stays correct.
    /// </summary>
    private static AssistedMoveCandidate? MakeBalancedMove(
        SubtitleLineViewModel first,
        SubtitleLineViewModel second,
        AssistedMoveKind kind,
        string title,
        string languageCode)
    {
        var firstText = (first.Text ?? string.Empty).Trim();
        var secondText = (second.Text ?? string.Empty).Trim();
        var firstLength = CountVisibleCharacters(firstText);
        var secondLength = CountVisibleCharacters(secondText);
        var ideal = (firstLength + secondLength) / 2.0;
        var moveDown = firstLength > secondLength;

        var bestFirst = firstText;
        var bestSecond = secondText;
        var bestDistance = Math.Abs(firstLength - ideal);
        var s1 = firstText;
        var s2 = secondText;

        for (var i = 0; i < 20; i++)
        {
            var upDown = new MoveWordUpDown(s1, s2);
            if (moveDown)
            {
                upDown.MoveWordDown();
            }
            else
            {
                upDown.MoveWordUp();
            }

            if ((upDown.S1 == s1 && upDown.S2 == s2) ||
                string.IsNullOrWhiteSpace(upDown.S1) ||
                string.IsNullOrWhiteSpace(upDown.S2))
            {
                break;
            }

            s1 = upDown.S1;
            s2 = upDown.S2;

            var distance = Math.Abs(CountVisibleCharacters(s1) - ideal);
            if (distance >= bestDistance)
            {
                break; // walked past the balance point - the previous step was the best
            }

            bestDistance = distance;
            bestFirst = s1;
            bestSecond = s2;
        }

        if (bestFirst == firstText && bestSecond == secondText)
        {
            return null;
        }

        return MakeBetweenCandidate(first, second, bestFirst, bestSecond, kind, title, languageCode);
    }

    /// <summary>
    /// A whole-fragment move: the unfinished part of a sentence crosses the boundary in one
    /// go instead of word by word. Down = the sentence tail of <paramref name="first"/> (the
    /// text after its last sentence end) joins the start of <paramref name="second"/>;
    /// up = the sentence head of <paramref name="second"/> (the text up to and including its
    /// first sentence end) joins the end of <paramref name="first"/>.
    /// </summary>
    private static AssistedMoveCandidate? MakeFragmentMove(
        SubtitleLineViewModel first,
        SubtitleLineViewModel second,
        bool moveDown,
        AssistedMoveKind kind,
        string title,
        string languageCode)
    {
        var firstText = (first.Text ?? string.Empty).Trim();
        var secondText = (second.Text ?? string.Empty).Trim();
        string newFirst;
        string newSecond;

        if (moveDown)
        {
            var idx = FindLastSentenceEnd(firstText);
            if (idx <= 0 || idx >= firstText.Length)
            {
                return null; // no sentence end, or nothing after it - no tail to move
            }

            newFirst = RebreakIfTooLong(firstText.Substring(0, idx).Trim(), languageCode);
            newSecond = JoinFragments(firstText.Substring(idx).Trim(), secondText, languageCode);
        }
        else
        {
            var idx = FindFirstSentenceEnd(secondText);
            if (idx <= 0 || idx >= secondText.Length)
            {
                return null; // no sentence end, or it swallows the whole subtitle
            }

            newFirst = JoinFragments(firstText, secondText.Substring(0, idx).Trim(), languageCode);
            newSecond = RebreakIfTooLong(secondText.Substring(idx).Trim(), languageCode);
        }

        if (newFirst == firstText && newSecond == secondText)
        {
            return null;
        }

        return MakeBetweenCandidate(first, second, newFirst, newSecond, kind, title, languageCode);
    }

    private static AssistedMoveCandidate? MakeBetweenCandidate(
        SubtitleLineViewModel first,
        SubtitleLineViewModel second,
        string newFirst,
        string newSecond,
        AssistedMoveKind kind,
        string title,
        string languageCode)
    {
        if (string.IsNullOrWhiteSpace(newFirst) || string.IsNullOrWhiteSpace(newSecond))
        {
            return null;
        }

        // A move leaves the old line break where it was ("...everyone in the theater" /
        // "started") - re-break both sides so their lines end up balanced again.
        newFirst = AutoBalanceLines(newFirst, languageCode);
        newSecond = AutoBalanceLines(newSecond, languageCode);

        // Moving a dialog dash as a "word" - or leaving one behind alone on a line -
        // corrupts the dialog formatting; such a move is never a useful option.
        if (HasDashOnlyLine(newFirst) || HasDashOnlyLine(newSecond))
        {
            return null;
        }

        // The receiving side must still fit a subtitle (at most two full lines) - but a
        // side that was already over the limit may stay over it, as long as the move does
        // not make it grow (otherwise no option at all would exist for exactly the
        // too-long lines this window is for).
        var maxLen = Math.Max(1, Configuration.Settings.General.SubtitleLineMaximumLength);
        var limit = maxLen * 2;
        var newFirstLength = CountVisibleCharacters(newFirst);
        var newSecondLength = CountVisibleCharacters(newSecond);
        if ((newFirstLength > limit && newFirstLength > CountVisibleCharacters(first.Text ?? string.Empty)) ||
            (newSecondLength > limit && newSecondLength > CountVisibleCharacters(second.Text ?? string.Empty)))
        {
            return null;
        }

        // Move the boundary time with the text: keep the original gap and the outer
        // start/end, and divide the span proportionally to the new text lengths.
        var (newFirstEnd, newSecondStart) = ComputeTimeSplit(first, second, newFirstLength, newSecondLength);

        return new AssistedMoveCandidate
        {
            Title = title,
            Kind = kind,
            NewCurrentText = kind == AssistedMoveKind.WithNext ? newFirst : newSecond,
            NewOtherText = kind == AssistedMoveKind.WithNext ? newSecond : newFirst,
            NewFirstEnd = newFirstEnd,
            NewSecondStart = newSecondStart,
            FirstText = newFirst,
            SecondText = newSecond,
            FirstInfo = MakeInfo(newFirst, first.StartTime, newFirstEnd),
            SecondInfo = MakeInfo(newSecond, newSecondStart, second.EndTime),
        };
    }

    /// <summary>
    /// Divides the combined time span of the two subtitles proportionally to their new
    /// text lengths, preserving the outer start/end and the gap between them. The ratio is
    /// clamped so neither side collapses to a sliver.
    /// </summary>
    private static (TimeSpan firstEnd, TimeSpan secondStart) ComputeTimeSplit(
        SubtitleLineViewModel first, SubtitleLineViewModel second, int firstLength, int secondLength)
    {
        var gapMs = (second.StartTime - first.EndTime).TotalMilliseconds;
        if (gapMs < 0)
        {
            gapMs = 0;
        }

        var availableMs = (second.EndTime - first.StartTime).TotalMilliseconds - gapMs;
        var totalLength = firstLength + secondLength;
        if (availableMs <= 0 || totalLength <= 0)
        {
            return (first.EndTime, second.StartTime); // degenerate timing - leave it alone
        }

        var ratio = Math.Clamp((double)firstLength / totalLength, 0.15, 0.85);
        var firstEnd = first.StartTime + TimeSpan.FromMilliseconds(availableMs * ratio);
        var secondStart = firstEnd + TimeSpan.FromMilliseconds(gapMs);
        return (firstEnd, secondStart);
    }

    /// <summary>A word move between the two lines of the current subtitle.</summary>
    private static AssistedMoveCandidate? MakeWithin(
        SubtitleLineViewModel current,
        bool moveDown,
        string languageCode,
        string title)
    {
        var text = (current.Text ?? string.Empty).Trim();
        var lines = text.SplitToLines();
        if (lines.Count > 2)
        {
            lines = Utilities.AutoBreakLine(Utilities.UnbreakLine(text), languageCode).SplitToLines();
        }

        if (lines.Count != 2)
        {
            return null;
        }

        // The two lines of a dialog belong to different speakers - never mix their words.
        if (lines.Any(line => line.TrimStart().StartsWith('-') || line.TrimStart().StartsWith('–')))
        {
            return null;
        }

        var upDown = new MoveWordUpDown(lines[0].Trim(), lines[1].Trim());
        if (moveDown)
        {
            upDown.MoveWordDown();
        }
        else
        {
            upDown.MoveWordUp();
        }

        if (string.IsNullOrWhiteSpace(upDown.S1) || string.IsNullOrWhiteSpace(upDown.S2))
        {
            return null;
        }

        var newText = upDown.S1 + Environment.NewLine + upDown.S2;
        if (newText.SplitToLines().Count > 2)
        {
            newText = Utilities.AutoBreakLine(Utilities.UnbreakLine(newText), languageCode);
        }

        if (newText == text || HasDashOnlyLine(newText))
        {
            return null;
        }

        return new AssistedMoveCandidate
        {
            Title = title,
            Kind = AssistedMoveKind.WithinSubtitle,
            NewCurrentText = newText,
            FirstText = newText,
            FirstInfo = MakeInfo(newText, current.StartTime, current.EndTime),
        };
    }

    /// <summary>
    /// Joins an incoming sentence fragment with existing text (fragment first) into one
    /// flowing text, re-broken to at most two lines.
    /// </summary>
    private static string JoinFragments(string firstPart, string secondPart, string languageCode)
    {
        var joined = Utilities.UnbreakLine(firstPart.Trim() + " " + secondPart.Trim());
        return RebreakIfTooLong(joined, languageCode);
    }

    /// <summary>
    /// Unbreaks and re-breaks the text so its lines are evenly balanced. Dialog texts keep
    /// their per-speaker lines and are only re-broken when a line is over the limit.
    /// </summary>
    private static string AutoBalanceLines(string text, string languageCode)
    {
        var isDialog = text.SplitToLines()
            .Any(line => line.TrimStart().StartsWith('-') || line.TrimStart().StartsWith('–'));
        if (isDialog)
        {
            return RebreakIfTooLong(text, languageCode);
        }

        return Utilities.AutoBreakLine(Utilities.UnbreakLine(text), languageCode);
    }

    private static string RebreakIfTooLong(string text, string languageCode)
    {
        var maxLen = Configuration.Settings.General.SubtitleLineMaximumLength;
        if (maxLen <= 0)
        {
            return text;
        }

        var tooLong = HtmlUtil.RemoveHtmlTags(text, true)
            .SplitToLines()
            .Any(line => line.Length > maxLen);

        return tooLong ? Utilities.AutoBreakLine(Utilities.UnbreakLine(text), languageCode) : text;
    }

    /// <summary>
    /// Index just after the last sentence end (punctuation plus closing quotes) in
    /// <paramref name="text"/>, skipping tags; -1 when there is none.
    /// </summary>
    private static int FindLastSentenceEnd(string text)
    {
        var last = -1;
        foreach (var idx in GetSentenceEndIndices(text))
        {
            last = idx;
        }

        return last;
    }

    /// <summary>Index just after the first sentence end; -1 when there is none.</summary>
    private static int FindFirstSentenceEnd(string text)
    {
        foreach (var idx in GetSentenceEndIndices(text))
        {
            return idx;
        }

        return -1;
    }

    private static IEnumerable<int> GetSentenceEndIndices(string text)
    {
        var inTag = MakeTagMask(text);
        for (var i = 0; i < text.Length; i++)
        {
            if (inTag[i] || !SentenceEndChars.Contains(text[i]))
            {
                continue;
            }

            // Skip decimal numbers like "1.5" and web addresses like "nikse.dk".
            if (text[i] == '.' &&
                i > 0 && i + 1 < text.Length &&
                !char.IsWhiteSpace(text[i + 1]))
            {
                continue;
            }

            // Consume a run of end punctuation ("...", "?!") and trailing closing quotes.
            var end = i;
            while (end + 1 < text.Length && SentenceEndChars.Contains(text[end + 1]))
            {
                end++;
            }

            while (end + 1 < text.Length && ClosingChars.Contains(text[end + 1]))
            {
                end++;
            }

            var splitIndex = end + 1;
            i = end;

            if (splitIndex >= text.Length || !char.IsWhiteSpace(text[splitIndex]))
            {
                continue;
            }

            yield return splitIndex;
        }
    }

    // True at positions inside "<...>" or "{...}" blocks, so fragment cuts never land inside a tag.
    private static bool[] MakeTagMask(string text)
    {
        var mask = new bool[text.Length];
        var inAngle = false;
        var inCurly = false;
        for (var i = 0; i < text.Length; i++)
        {
            var c = text[i];
            if (c == '<')
            {
                inAngle = true;
            }
            else if (c == '{')
            {
                inCurly = true;
            }

            mask[i] = inAngle || inCurly;

            if (c == '>')
            {
                inAngle = false;
            }
            else if (c == '}')
            {
                inCurly = false;
            }
        }

        return mask;
    }

    private static bool HasDashOnlyLine(string text)
    {
        return HtmlUtil.RemoveHtmlTags(text, true)
            .SplitToLines()
            .Any(line => line.Trim() is "-" or "–");
    }

    private static string MakeInfo(string newText, TimeSpan start, TimeSpan end)
    {
        var startText = new TimeCode(start).ToShortDisplayString();
        var endText = new TimeCode(end).ToShortDisplayString();
        var chars = CountVisibleCharacters(newText);
        var seconds = (end - start).TotalSeconds;
        var cps = seconds > 0.001 ? chars / seconds : 0;
        return $"{startText} → {endText}      {chars} chars, {cps:0.#} CPS";
    }

    private static int CountVisibleCharacters(string text)
    {
        // Line breaks (CR/LF) are the only control characters left after tag stripping.
        var stripped = HtmlUtil.RemoveHtmlTags(text, true);
        return stripped.Count(c => !char.IsControl(c));
    }
}
