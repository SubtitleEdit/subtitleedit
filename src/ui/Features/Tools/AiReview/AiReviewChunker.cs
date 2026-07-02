using Nikse.SubtitleEdit.Core.Common;
using System.Collections.Generic;

namespace Nikse.SubtitleEdit.Features.Tools.AiReview;

public record ReviewLine(int Number, string Text);

public class ReviewChunk
{
    public List<ReviewLine> ContextBefore { get; } = new();
    public List<ReviewLine> Lines { get; } = new();
    public List<ReviewLine> ContextAfter { get; } = new();
}

/// <summary>
/// Splits the subtitle into review batches without ever splitting a sentence that spans
/// multiple lines: consecutive lines are grouped into "sentence units" (a line joins the next
/// when it does not end a sentence), and chunk boundaries only fall between units. Each chunk
/// carries a couple of read-only context lines on both sides so the model sees the dialogue flow.
/// </summary>
public static class AiReviewChunker
{
    private const int MaxLinesPerUnit = 4;
    private const int ContextLines = 2;

    /// <summary>Unit id per line, aligned with <paramref name="lines"/> - lines sharing an id form one sentence.</summary>
    public static int[] BuildUnitIds(IReadOnlyList<ReviewLine> lines)
    {
        var ids = new int[lines.Count];
        var unit = 0;
        var unitLength = 1;
        for (var i = 0; i < lines.Count; i++)
        {
            ids[i] = unit;
            if (EndsSentence(lines[i].Text) || unitLength >= MaxLinesPerUnit)
            {
                unit++;
                unitLength = 1;
            }
            else
            {
                unitLength++;
            }
        }

        return ids;
    }

    public static List<ReviewChunk> BuildChunks(IReadOnlyList<ReviewLine> lines, int maxLinesPerChunk)
    {
        var chunks = new List<ReviewChunk>();
        if (lines.Count == 0)
        {
            return chunks;
        }

        if (maxLinesPerChunk < 1)
        {
            maxLinesPerChunk = 1;
        }

        var unitIds = BuildUnitIds(lines);
        var chunk = new ReviewChunk();
        var i = 0;
        while (i < lines.Count)
        {
            // take a whole unit
            var unitStart = i;
            var unitId = unitIds[i];
            while (i < lines.Count && unitIds[i] == unitId)
            {
                i++;
            }

            var unitLength = i - unitStart;
            if (chunk.Lines.Count > 0 && chunk.Lines.Count + unitLength > maxLinesPerChunk)
            {
                chunks.Add(chunk);
                chunk = new ReviewChunk();
            }

            for (var j = unitStart; j < i; j++)
            {
                chunk.Lines.Add(lines[j]);
            }
        }

        if (chunk.Lines.Count > 0)
        {
            chunks.Add(chunk);
        }

        // read-only context around each chunk
        var indexByNumber = new Dictionary<int, int>();
        for (var k = 0; k < lines.Count; k++)
        {
            indexByNumber[lines[k].Number] = k;
        }

        foreach (var c in chunks)
        {
            var first = indexByNumber[c.Lines[0].Number];
            var last = indexByNumber[c.Lines[^1].Number];
            for (var k = System.Math.Max(0, first - ContextLines); k < first; k++)
            {
                c.ContextBefore.Add(lines[k]);
            }

            for (var k = last + 1; k <= System.Math.Min(lines.Count - 1, last + ContextLines); k++)
            {
                c.ContextAfter.Add(lines[k]);
            }
        }

        return chunks;
    }

    internal static bool EndsSentence(string text)
    {
        var s = HtmlUtil.RemoveHtmlTags(text, true).TrimEnd();
        if (s.Length == 0)
        {
            return true;
        }

        // skip closing quotes/brackets after the sentence-final mark
        var i = s.Length - 1;
        while (i >= 0 && "\"'’”»)]".IndexOf(s[i]) >= 0)
        {
            i--;
        }

        if (i < 0)
        {
            return true;
        }

        return ".!?…".IndexOf(s[i]) >= 0;
    }
}
