using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Nikse.SubtitleEdit.Core.SubtitleFormats;

namespace Nikse.SubtitleEdit.Features.Main.FlowEditing;

/// <summary>
/// A tag-free view of subtitle text together with Teletext colors expressed in visible-text
/// offsets. This is deliberately independent of Flow's editing decisions; callers can move or
/// replace visible ranges without putting HTML tags in the Flow text box.
/// </summary>
internal sealed class FlowInlineColorProjection
{
    private static readonly Regex HiddenTokenRegex = new(
        "(?<alignment>\\{\\\\an[1-9]\\})|(?<fontOpen><font\\b[^>]*>)|(?<fontClose></font\\s*>)",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private static readonly Regex ColorAttributeRegex = new(
        "\\bcolor\\s*=\\s*(?:\"(?<quoted>[^\"]+)\"|'(?<single>[^']+)'|(?<bare>[^\\s>]+))",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private readonly List<FlowInlineColorRun> _colorRuns;
    private readonly List<FlowInlineHiddenToken> _hiddenTokens;

    private FlowInlineColorProjection(
        string visibleText,
        IEnumerable<FlowInlineColorRun> colorRuns,
        IEnumerable<FlowInlineHiddenToken> hiddenTokens)
    {
        VisibleText = visibleText;
        _colorRuns = NormalizeRuns(colorRuns, visibleText.Length);
        _hiddenTokens = hiddenTokens
            .Where(p => p.Offset >= 0 && p.Offset <= visibleText.Length)
            .OrderBy(p => p.Offset)
            .ThenBy(p => p.Order)
            .ToList();
    }

    public string VisibleText { get; }

    public IReadOnlyList<FlowInlineColorRun> ColorRuns => _colorRuns;

    public static FlowInlineColorProjection Parse(string? canonicalText)
    {
        var source = canonicalText ?? string.Empty;
        var visible = new StringBuilder(source.Length);
        var runs = new List<FlowInlineColorRun>();
        var tokens = new List<FlowInlineHiddenToken>();
        var fontStack = new Stack<FontScope>();
        string? activeColor = null;
        var sourceIndex = 0;
        var tokenOrder = 0;

        foreach (Match match in HiddenTokenRegex.Matches(source))
        {
            AppendVisible(source.AsSpan(sourceIndex, match.Index - sourceIndex), visible, activeColor, runs);

            if (match.Groups["alignment"].Success)
            {
                tokens.Add(new FlowInlineHiddenToken(
                    visible.Length,
                    match.Value,
                    tokenOrder++,
                    BeforeColorTransition: true));
            }
            else if (match.Groups["fontOpen"].Success)
            {
                var color = GetTeletextColor(match.Value);
                var isTeletextColor = color != null;
                fontStack.Push(new FontScope(activeColor, isTeletextColor));
                if (isTeletextColor)
                {
                    activeColor = color;
                }
                else
                {
                    // Current Flow hides every font tag, including unrelated font attributes.
                    // Keep such a tag anchored so a round-trip does not discard it.
                    tokens.Add(new FlowInlineHiddenToken(
                        visible.Length,
                        match.Value,
                        tokenOrder++,
                        BeforeColorTransition: true));
                }
            }
            else if (fontStack.Count > 0)
            {
                var scope = fontStack.Pop();
                if (scope.IsTeletextColor)
                {
                    activeColor = scope.PreviousColor;
                }
                else
                {
                    tokens.Add(new FlowInlineHiddenToken(
                        visible.Length,
                        match.Value,
                        tokenOrder++,
                        BeforeColorTransition: false));
                }
            }
            else
            {
                tokens.Add(new FlowInlineHiddenToken(
                    visible.Length,
                    match.Value,
                    tokenOrder++,
                    BeforeColorTransition: false));
            }

            sourceIndex = match.Index + match.Length;
        }

        AppendVisible(source.AsSpan(sourceIndex), visible, activeColor, runs);
        return new FlowInlineColorProjection(visible.ToString(), runs, tokens);
    }

    public FlowInlineColorProjection ApplyColor(int start, int length, string color)
    {
        ValidateRange(start, length);
        var normalizedColor = NormalizeColor(color) ??
                              throw new ArgumentException("The color is not an EBU Teletext color.", nameof(color));
        if (length == 0)
        {
            return this;
        }

        var runs = RemoveColorFromRuns(start, length);
        runs.Add(new FlowInlineColorRun(start, length, normalizedColor));
        return new FlowInlineColorProjection(VisibleText, runs, _hiddenTokens);
    }

    public FlowInlineColorProjection RemoveColor(int start, int length)
    {
        ValidateRange(start, length);
        return length == 0
            ? this
            : new FlowInlineColorProjection(VisibleText, RemoveColorFromRuns(start, length), _hiddenTokens);
    }

    /// <summary>
    /// Applies one ordinary text-box edit while retaining metadata on the unchanged prefix and
    /// suffix. A replacement wholly inside one color run inherits that run's color.
    /// </summary>
    public FlowInlineColorProjection ApplyVisibleEdit(string editedText)
    {
        ArgumentNullException.ThrowIfNull(editedText);
        if (editedText == VisibleText)
        {
            return this;
        }

        var prefixLength = 0;
        while (prefixLength < VisibleText.Length &&
               prefixLength < editedText.Length &&
               VisibleText[prefixLength] == editedText[prefixLength])
        {
            prefixLength++;
        }

        var suffixLength = 0;
        while (suffixLength < VisibleText.Length - prefixLength &&
               suffixLength < editedText.Length - prefixLength &&
               VisibleText[VisibleText.Length - suffixLength - 1] ==
               editedText[editedText.Length - suffixLength - 1])
        {
            suffixLength++;
        }

        var removedLength = VisibleText.Length - prefixLength - suffixLength;
        var insertedLength = editedText.Length - prefixLength - suffixLength;
        var replacementColor = removedLength > 0 && insertedLength > 0
            ? _colorRuns.FirstOrDefault(p =>
                p.Start < prefixLength && p.End > prefixLength + removedLength)?.Color
            : null;

        var result = Delete(prefixLength, removedLength);
        if (insertedLength == 0)
        {
            return result;
        }

        result = result.Insert(
            prefixLength,
            editedText.Substring(prefixLength, insertedLength));
        return replacementColor == null
            ? result
            : result.ApplyColor(prefixLength, insertedLength, replacementColor);
    }

    /// <summary>
    /// Transfers this projection's colors to canonical text whose non-whitespace characters are
    /// unchanged and in the same order. This covers Flow wrapping, trimming and line-break
    /// changes while retaining the target's alignment and unrelated hidden tokens.
    /// </summary>
    public FlowInlineColorProjection TransferColorsTo(string canonicalTarget)
    {
        ArgumentNullException.ThrowIfNull(canonicalTarget);
        var target = Parse(canonicalTarget);
        var remappedRuns = RemapColorRuns(target.VisibleText);
        return new FlowInlineColorProjection(target.VisibleText, remappedRuns, target._hiddenTokens);
    }

    public FlowInlineColorProjection ReflowVisibleText(string visibleText)
    {
        ArgumentNullException.ThrowIfNull(visibleText);
        return new FlowInlineColorProjection(visibleText, RemapColorRuns(visibleText), _hiddenTokens);
    }

    public FlowInlineColorProjection Insert(int offset, string text, bool inheritColor = true)
    {
        ArgumentNullException.ThrowIfNull(text);
        ValidateOffset(offset);
        if (text.Length == 0)
        {
            return this;
        }

        var inheritedColor = inheritColor ? GetColorStrictlyInside(offset) : null;
        var runs = new List<FlowInlineColorRun>();
        foreach (var run in _colorRuns)
        {
            if (run.End <= offset)
            {
                runs.Add(run);
            }
            else if (run.Start >= offset)
            {
                runs.Add(run with { Start = run.Start + text.Length });
            }
            else
            {
                runs.Add(run with { Length = offset - run.Start });
                runs.Add(new FlowInlineColorRun(offset + text.Length, run.End - offset, run.Color));
            }
        }

        if (inheritedColor != null)
        {
            runs.Add(new FlowInlineColorRun(offset, text.Length, inheritedColor));
        }

        var tokens = _hiddenTokens.Select(p => p.Offset > offset ? p with { Offset = p.Offset + text.Length } : p);
        return new FlowInlineColorProjection(VisibleText.Insert(offset, text), runs, tokens);
    }

    public FlowInlineColorProjection Insert(int offset, FlowInlineColorProjection fragment)
    {
        ArgumentNullException.ThrowIfNull(fragment);
        ValidateOffset(offset);

        var result = Insert(offset, fragment.VisibleText, inheritColor: false);
        var runs = result._colorRuns.Concat(fragment._colorRuns.Select(p => p with { Start = p.Start + offset }));
        var nextOrder = result._hiddenTokens.Count == 0 ? 0 : result._hiddenTokens.Max(p => p.Order) + 1;
        var tokens = result._hiddenTokens.Concat(fragment._hiddenTokens.Select(p =>
            new FlowInlineHiddenToken(
                p.Offset + offset,
                p.Token,
                nextOrder + p.Order,
                p.BeforeColorTransition)));
        return new FlowInlineColorProjection(result.VisibleText, runs, tokens);
    }

    public FlowInlineColorProjection Delete(int start, int length)
    {
        ValidateRange(start, length);
        if (length == 0)
        {
            return this;
        }

        var end = start + length;
        var runs = new List<FlowInlineColorRun>();
        foreach (var run in _colorRuns)
        {
            var beforeLength = Math.Max(0, Math.Min(run.End, start) - run.Start);
            if (beforeLength > 0)
            {
                runs.Add(new FlowInlineColorRun(run.Start, beforeLength, run.Color));
            }

            var afterStart = Math.Max(run.Start, end);
            if (afterStart < run.End)
            {
                runs.Add(new FlowInlineColorRun(afterStart - length, run.End - afterStart, run.Color));
            }
        }

        // Hidden alignment/formatting tokens do not belong to a visible character. Keep tokens
        // in a deleted range at the edit boundary rather than silently throwing formatting away.
        var tokens = _hiddenTokens.Select(p => p.Offset <= start
            ? p
            : p.Offset >= end
                ? p with { Offset = p.Offset - length }
                : p with { Offset = start });
        return new FlowInlineColorProjection(VisibleText.Remove(start, length), runs, tokens);
    }

    public FlowInlineColorProjection Extract(int start, int length)
    {
        ValidateRange(start, length);
        var end = start + length;
        var runs = _colorRuns
            .Where(p => p.Start < end && p.End > start)
            .Select(p => new FlowInlineColorRun(
                Math.Max(p.Start, start) - start,
                Math.Min(p.End, end) - Math.Max(p.Start, start),
                p.Color));
        var tokens = _hiddenTokens
            .Where(p => p.Offset >= start && p.Offset <= end)
            .Select(p => p with { Offset = p.Offset - start });
        return new FlowInlineColorProjection(VisibleText.Substring(start, length), runs, tokens);
    }

    public string Serialize()
    {
        var result = new StringBuilder(VisibleText.Length + _colorRuns.Count * 30);
        var tokensByOffset = _hiddenTokens.GroupBy(p => p.Offset).ToDictionary(p => p.Key, p => p.OrderBy(x => x.Order));
        string? openColor = null;

        for (var offset = 0; offset <= VisibleText.Length; offset++)
        {
            if (tokensByOffset.TryGetValue(offset, out var tokens))
            {
                // Alignment and opening unrelated font tags stay outside a color span that
                // starts at this offset. Closing unrelated font tags come after a color span
                // ends, preventing crossed tags such as </font-face></font-color>.
                foreach (var token in tokens.Where(p => p.BeforeColorTransition))
                {
                    result.Append(token.Token);
                }
            }

            var color = offset < VisibleText.Length
                ? _colorRuns.FirstOrDefault(p => p.Start <= offset && p.End > offset)?.Color
                : null;

            if (!string.Equals(openColor, color, StringComparison.OrdinalIgnoreCase))
            {
                if (openColor != null)
                {
                    result.Append("</font>");
                }

                if (color != null)
                {
                    result.Append("<font color=\"").Append(color).Append("\">");
                }

                openColor = color;
            }

            if (tokensByOffset.TryGetValue(offset, out tokens))
            {
                foreach (var token in tokens.Where(p => !p.BeforeColorTransition))
                {
                    result.Append(token.Token);
                }
            }

            if (offset < VisibleText.Length)
            {
                result.Append(VisibleText[offset]);
            }
        }

        return result.ToString();
    }

    public int GetCanonicalOffset(int visibleOffset)
    {
        ValidateOffset(visibleOffset);
        var canonical = Serialize();
        var canonicalIndex = 0;
        var currentVisibleOffset = 0;
        foreach (Match match in HiddenTokenRegex.Matches(canonical))
        {
            var visibleLength = match.Index - canonicalIndex;
            if (visibleOffset <= currentVisibleOffset + visibleLength && visibleLength > 0)
            {
                return canonicalIndex + visibleOffset - currentVisibleOffset;
            }

            currentVisibleOffset += visibleLength;
            canonicalIndex = match.Index + match.Length;
        }

        return canonicalIndex + visibleOffset - currentVisibleOffset;
    }

    private List<FlowInlineColorRun> RemoveColorFromRuns(int start, int length)
    {
        var end = start + length;
        var runs = new List<FlowInlineColorRun>();
        foreach (var run in _colorRuns)
        {
            if (run.End <= start || run.Start >= end)
            {
                runs.Add(run);
                continue;
            }

            if (run.Start < start)
            {
                runs.Add(new FlowInlineColorRun(run.Start, start - run.Start, run.Color));
            }

            if (run.End > end)
            {
                runs.Add(new FlowInlineColorRun(end, run.End - end, run.Color));
            }
        }

        return runs;
    }

    private string? GetColorStrictlyInside(int offset) => _colorRuns
        .FirstOrDefault(p => p.Start < offset && p.End > offset)
        ?.Color;

    private List<FlowInlineColorRun> RemapColorRuns(string targetText)
    {
        if (targetText == VisibleText)
        {
            return _colorRuns.ToList();
        }

        var sourceCharacters = Enumerable.Range(0, VisibleText.Length)
            .Where(p => !char.IsWhiteSpace(VisibleText[p]))
            .ToArray();
        var targetCharacters = Enumerable.Range(0, targetText.Length)
            .Where(p => !char.IsWhiteSpace(targetText[p]))
            .ToArray();

        if (sourceCharacters.Length != targetCharacters.Length ||
            sourceCharacters.Where((p, i) => VisibleText[p] != targetText[targetCharacters[i]]).Any())
        {
            return ApplyVisibleEdit(targetText)._colorRuns.ToList();
        }

        var sourceColors = GetColorsByOffset(VisibleText.Length);
        var targetColors = new string?[targetText.Length];
        for (var i = 0; i < sourceCharacters.Length; i++)
        {
            targetColors[targetCharacters[i]] = sourceColors[sourceCharacters[i]];
        }

        for (var gap = 0; gap <= sourceCharacters.Length; gap++)
        {
            var sourceStart = gap == 0 ? 0 : sourceCharacters[gap - 1] + 1;
            var sourceEnd = gap == sourceCharacters.Length ? VisibleText.Length : sourceCharacters[gap];
            var targetStart = gap == 0 ? 0 : targetCharacters[gap - 1] + 1;
            var targetEnd = gap == targetCharacters.Length ? targetText.Length : targetCharacters[gap];
            var sourceLength = sourceEnd - sourceStart;
            var targetLength = targetEnd - targetStart;
            if (sourceLength == targetLength)
            {
                for (var i = 0; i < sourceLength; i++)
                {
                    targetColors[targetStart + i] = sourceColors[sourceStart + i];
                }
            }
            else if (sourceLength > 0)
            {
                var color = sourceColors[sourceStart];
                var isUniform = Enumerable.Range(sourceStart, sourceLength)
                    .All(p => string.Equals(sourceColors[p], color, StringComparison.OrdinalIgnoreCase));
                if (isUniform)
                {
                    for (var i = targetStart; i < targetEnd; i++)
                    {
                        targetColors[i] = color;
                    }
                }
            }
        }

        return MakeRuns(targetColors);
    }

    private string?[] GetColorsByOffset(int length)
    {
        var colors = new string?[length];
        foreach (var run in _colorRuns)
        {
            for (var i = run.Start; i < run.End; i++)
            {
                colors[i] = run.Color;
            }
        }

        return colors;
    }

    private static List<FlowInlineColorRun> MakeRuns(IReadOnlyList<string?> colors)
    {
        var runs = new List<FlowInlineColorRun>();
        var start = 0;
        while (start < colors.Count)
        {
            var color = colors[start];
            if (color == null)
            {
                start++;
                continue;
            }

            var end = start + 1;
            while (end < colors.Count &&
                   string.Equals(colors[end], color, StringComparison.OrdinalIgnoreCase))
            {
                end++;
            }

            runs.Add(new FlowInlineColorRun(start, end - start, color));
            start = end;
        }

        return runs;
    }

    private void ValidateOffset(int offset)
    {
        if (offset < 0 || offset > VisibleText.Length)
        {
            throw new ArgumentOutOfRangeException(nameof(offset));
        }
    }

    private void ValidateRange(int start, int length)
    {
        if (start < 0 || length < 0 || start > VisibleText.Length - length)
        {
            throw new ArgumentOutOfRangeException(nameof(length));
        }
    }

    private static void AppendVisible(
        ReadOnlySpan<char> text,
        StringBuilder visible,
        string? activeColor,
        ICollection<FlowInlineColorRun> runs)
    {
        if (text.Length == 0)
        {
            return;
        }

        var start = visible.Length;
        visible.Append(text);
        if (activeColor != null)
        {
            runs.Add(new FlowInlineColorRun(start, text.Length, activeColor));
        }
    }

    private static string? GetTeletextColor(string fontTag)
    {
        var match = ColorAttributeRegex.Match(fontTag);
        if (!match.Success)
        {
            return null;
        }

        var value = match.Groups["quoted"].Success
            ? match.Groups["quoted"].Value
            : match.Groups["single"].Success
                ? match.Groups["single"].Value
                : match.Groups["bare"].Value;
        return NormalizeColor(value);
    }

    private static string? NormalizeColor(string color) => Ebu.GetNearestColorName(color.Trim());

    private static List<FlowInlineColorRun> NormalizeRuns(
        IEnumerable<FlowInlineColorRun> source,
        int visibleLength)
    {
        var ordered = source
            .Where(p => p.Length > 0 && p.Start >= 0 && p.End <= visibleLength)
            .OrderBy(p => p.Start)
            .ThenBy(p => p.End)
            .ToList();
        var result = new List<FlowInlineColorRun>();
        foreach (var run in ordered)
        {
            if (result.Count > 0 && run.Start < result[^1].End)
            {
                throw new ArgumentException("Color runs must not overlap.", nameof(source));
            }

            if (result.Count > 0 && run.Start == result[^1].End &&
                string.Equals(run.Color, result[^1].Color, StringComparison.OrdinalIgnoreCase))
            {
                result[^1] = result[^1] with { Length = result[^1].Length + run.Length };
            }
            else
            {
                result.Add(run);
            }
        }

        return result;
    }

    private sealed record FontScope(string? PreviousColor, bool IsTeletextColor);
}

internal sealed record FlowInlineColorRun(int Start, int Length, string Color)
{
    public int End => Start + Length;
}

internal sealed record FlowInlineHiddenToken(
    int Offset,
    string Token,
    int Order,
    bool BeforeColorTransition);
