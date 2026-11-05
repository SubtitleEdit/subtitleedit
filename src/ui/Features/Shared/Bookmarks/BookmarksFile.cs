using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Main;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nikse.SubtitleEdit.Features.Shared.Bookmarks;

/// <summary>
/// The bookmarks export file: one tab-separated line per bookmark with the line number, the start
/// time (always hh:mm:ss,fff - not the display format, so frame numbers or a video offset do not
/// break the import), the bookmark text and the subtitle text. Line breaks are written as
/// &lt;br /&gt;. The subtitle text is there for reading only - import ignores it.
/// </summary>
public static class BookmarksFile
{
    private const string LineBreak = "<br />";

    public static string ToText(IEnumerable<SubtitleLineViewModel> bookmarkedLines)
    {
        var sb = new StringBuilder();
        foreach (var line in bookmarkedLines)
        {
            sb.Append('#').Append(line.Number).Append('\t')
                .Append(new TimeCode(line.StartTime).ToString()).Append('\t')
                .Append(Encode(line.Bookmark ?? string.Empty)).Append('\t')
                .AppendLine(Encode(line.Text));
        }

        return sb.ToString();
    }

    /// <summary>
    /// Sets the bookmarks in <paramref name="text"/> on <paramref name="allLines"/>: on the line
    /// with the same start time, else on the line with the same number. Lines that are not in
    /// the format are skipped. Returns the number of bookmarks set.
    /// </summary>
    public static int Apply(string text, IList<SubtitleLineViewModel> allLines)
    {
        var count = 0;
        foreach (var row in text.SplitToLines())
        {
            var parts = row.Split('\t');
            if (parts.Length < 3 ||
                !int.TryParse(parts[0].TrimStart('#').Trim(), out var number))
            {
                continue;
            }

            var startMs = Math.Round(TimeCode.ParseToMilliseconds(parts[1].Trim()));
            var target = allLines.FirstOrDefault(p => Math.Round(p.StartTime.TotalMilliseconds) == startMs);
            target ??= allLines.FirstOrDefault(p => p.Number == number);
            if (target == null)
            {
                continue;
            }

            target.Bookmark = Decode(parts[2]);
            count++;
        }

        return count;
    }

    private static string Encode(string s) => s.Replace("\t", " ").Replace("\r\n", LineBreak).Replace("\n", LineBreak);

    private static string Decode(string s) => s.Replace(LineBreak, Environment.NewLine);
}
