using Avalonia.Headless.XUnit;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Features.Shared.Bookmarks;

namespace UITests.Features.Shared;

/// <summary>
/// The bookmarks list export must read back with import: bookmarks land on the line with the
/// same start time, else the same number, and multi-line bookmarks keep their line breaks.
/// </summary>
public class BookmarksFileTests
{
    private static List<SubtitleLineViewModel> MakeLines() =>
    [
        new SubtitleLineViewModel { Number = 1, Text = "One", StartTime = TimeSpan.FromSeconds(1), EndTime = TimeSpan.FromSeconds(2) },
        new SubtitleLineViewModel { Number = 2, Text = "Two\nlines", StartTime = TimeSpan.FromMilliseconds(3456), EndTime = TimeSpan.FromSeconds(4) },
        new SubtitleLineViewModel { Number = 3, Text = "Three", StartTime = TimeSpan.FromSeconds(5), EndTime = TimeSpan.FromSeconds(6) },
    ];

    [AvaloniaFact]
    public void ExportThenImport_RestoresBookmarks()
    {
        var source = MakeLines();
        source[1].Bookmark = "Check this" + Environment.NewLine + "and this";
        source[2].Bookmark = string.Empty;
        var text = BookmarksFile.ToText(source.Where(p => p.Bookmark != null));

        var target = MakeLines();
        var count = BookmarksFile.Apply(text, target);

        Assert.Equal(2, count);
        Assert.Null(target[0].Bookmark);
        Assert.Equal(source[1].Bookmark, target[1].Bookmark);
        Assert.Equal(string.Empty, target[2].Bookmark);
    }

    [AvaloniaFact]
    public void ExportThenImport_RestoresColors()
    {
        var source = MakeLines();
        source[0].Bookmark = "Default";
        source[1].Bookmark = "Second";
        source[1].BookmarkColor = 1;
        source[2].Bookmark = "Third";
        source[2].BookmarkColor = 2;
        var text = BookmarksFile.ToText(source);

        var rows = text.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        Assert.Equal(4, rows[0].TrimEnd('\r').Split('\t').Length);
        Assert.EndsWith("\t2", rows[1].TrimEnd('\r'));
        Assert.EndsWith("\t3", rows[2].TrimEnd('\r'));

        var target = MakeLines();
        target[0].Bookmark = "Old";
        target[0].BookmarkColor = 2;
        BookmarksFile.Apply(text, target);

        Assert.Equal(new[] { 0, 1, 2 }, target.Select(p => p.BookmarkColor));
        Assert.Equal("Third", target[2].Bookmark);
    }

    [AvaloniaFact]
    public void Import_BadColor_IsDefaultColor()
    {
        var target = MakeLines();

        BookmarksFile.Apply("#1\t00:00:01,000\tNote\tOne\t9", target);

        Assert.Equal("Note", target[0].Bookmark);
        Assert.Equal(0, target[0].BookmarkColor);
    }

    [AvaloniaFact]
    public void Import_NoTimeMatch_FallsBackToNumber()
    {
        var target = MakeLines();

        var count = BookmarksFile.Apply("#3\t01:00:00,000\tLate\tThree", target);

        Assert.Equal(1, count);
        Assert.Equal("Late", target[2].Bookmark);
    }

    [AvaloniaFact]
    public void Import_TimeMatchWinsOverNumber()
    {
        var target = MakeLines();

        BookmarksFile.Apply("#1\t00:00:05,000\tMoved\tThree", target);

        Assert.Null(target[0].Bookmark);
        Assert.Equal("Moved", target[2].Bookmark);
    }

    [AvaloniaFact]
    public void Import_SkipsLinesNotInTheFormat()
    {
        var target = MakeLines();

        var count = BookmarksFile.Apply("some header\n\n#x\t00:00:01,000\tNo", target);

        Assert.Equal(0, count);
        Assert.All(target, p => Assert.Null(p.Bookmark));
    }
}
