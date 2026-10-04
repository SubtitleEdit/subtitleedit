using Avalonia.Media.TextFormatting;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace Nikse.SubtitleEdit.Controls;

/// <summary>
/// Works around an Avalonia 12.1.3 bug in BidiReorderer.BidiReorder: the final loop writes
/// <c>indexedTextRuns[visualPosition].RunIndex = logicalIndex</c> - the inverse of the mapping
/// TextLineImpl expects (<c>indexedTextRuns[logicalIndex].RunIndex = visualPosition</c>).
///
/// The inverse equals the mapping itself for a plain reversal, so it only shows when the
/// reorder is not self-inverse - e.g. a Latin stretch split into two or more style runs inside
/// a right-to-left line, which is exactly what the tag coloring produces for
/// "أريد أن أقول...&lt;font size=30&gt;\N". Hit testing then measures the wrong runs: the
/// selection is drawn in the wrong place and GetTextBounds throws "Covered length must be
/// greater than zero" from pointer, selection and render code, freezing and crashing SE (#15531).
///
/// The fix recomputes each RunIndex from the line's visual run order, so it is a no-op once
/// Avalonia is fixed. If Avalonia renames the internals, it silently does nothing
/// (the canary test BidiRunIndexFixerTests catches that at upgrade time).
/// </summary>
internal static class BidiRunIndexFixer
{
    private static readonly FieldInfo? IndexedTextRunsField;
    private static readonly PropertyInfo? RunIndexProperty;
    private static readonly PropertyInfo? TextRunProperty;

    static BidiRunIndexFixer()
    {
        var textLineImplType = typeof(TextLine).Assembly.GetType("Avalonia.Media.TextFormatting.TextLineImpl");
        var indexedTextRunType = typeof(TextLine).Assembly.GetType("Avalonia.Media.TextFormatting.IndexedTextRun");
        if (textLineImplType == null || indexedTextRunType == null)
        {
            return;
        }

        IndexedTextRunsField = textLineImplType.GetField("_indexedTextRuns", BindingFlags.Instance | BindingFlags.NonPublic);
        RunIndexProperty = indexedTextRunType.GetProperty("RunIndex", BindingFlags.Instance | BindingFlags.Public);
        TextRunProperty = indexedTextRunType.GetProperty("TextRun", BindingFlags.Instance | BindingFlags.Public);
    }

    /// <summary>
    /// True when the Avalonia internals the fix relies on were found.
    /// </summary>
    internal static bool IsAvailable =>
        IndexedTextRunsField != null &&
        RunIndexProperty is { CanRead: true, CanWrite: true } &&
        TextRunProperty is { CanRead: true };

    /// <summary>
    /// Corrects the run indices of every line in <paramref name="layout"/>.
    /// </summary>
    public static void Fix(TextLayout layout)
    {
        if (!IsAvailable)
        {
            return;
        }

        foreach (var line in layout.TextLines)
        {
            Fix(line);
        }
    }

    private static void Fix(TextLine line)
    {
        var visualRuns = line.TextRuns;
        if (visualRuns.Count < 3)
        {
            return; // two runs are always a plain reversal or identity - the inverse is the same
        }

        if (!IsAvailable || IndexedTextRunsField!.DeclaringType?.IsInstanceOfType(line) != true ||
            IndexedTextRunsField.GetValue(line) is not IList indexedRuns)
        {
            return;
        }

        foreach (var indexedRun in indexedRuns)
        {
            if (indexedRun == null || TextRunProperty!.GetValue(indexedRun) is not TextRun textRun)
            {
                continue;
            }

            var visualIndex = IndexOfReference(visualRuns, textRun);
            if (visualIndex >= 0 && !Equals(RunIndexProperty!.GetValue(indexedRun), visualIndex))
            {
                RunIndexProperty.SetValue(indexedRun, visualIndex);
            }
        }
    }

    private static int IndexOfReference(IReadOnlyList<TextRun> runs, TextRun run)
    {
        for (var i = 0; i < runs.Count; i++)
        {
            if (ReferenceEquals(runs[i], run))
            {
                return i;
            }
        }

        return -1;
    }
}
