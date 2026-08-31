using System;
using System.IO;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.UiLogic.Common;

namespace LibUiLogicTests.Common;

public class SubtitleMarksPersistenceTests : IDisposable
{
    private readonly string _fileName;

    public SubtitleMarksPersistenceTests()
    {
        _fileName = Path.Combine(Path.GetTempPath(), "se-marks-" + Guid.NewGuid().ToString("N") + ".srt");
    }

    public void Dispose()
    {
        foreach (var f in new[] { _fileName, _fileName + ".SE.bookmarks" })
        {
            if (File.Exists(f))
            {
                File.Delete(f);
            }
        }
    }

    private static Subtitle MakeSubtitle(params int[] startSeconds)
    {
        var subtitle = new Subtitle();
        foreach (var start in startSeconds)
        {
            subtitle.Paragraphs.Add(new Paragraph("Line at " + start, start * 1000.0, start * 1000.0 + 900.0));
        }

        return subtitle;
    }

    [Fact]
    public void Save_NoMarks_WritesNothing()
    {
        var subtitle = MakeSubtitle(1, 2, 3);

        Assert.True(new SubtitleMarksPersistence(subtitle, _fileName).Save());
        Assert.False(File.Exists(_fileName + ".SE.bookmarks"));
    }

    [Fact]
    public void Save_ClearingMarks_DeletesTheSidecar()
    {
        var subtitle = MakeSubtitle(1, 2, 3);
        subtitle.Paragraphs[1].Bookmark = "note";
        new SubtitleMarksPersistence(subtitle, _fileName).Save();
        Assert.True(File.Exists(_fileName + ".SE.bookmarks"));

        subtitle.Paragraphs[1].Bookmark = null;
        new SubtitleMarksPersistence(subtitle, _fileName).Save();

        Assert.False(File.Exists(_fileName + ".SE.bookmarks"));
    }

    [Fact]
    public void RoundTrip_BookmarksAndForced()
    {
        var subtitle = MakeSubtitle(1, 2, 3, 4);
        subtitle.Paragraphs[0].Bookmark = "with \"quotes\", and a comma";
        subtitle.Paragraphs[2].Bookmark = string.Empty;
        subtitle.Paragraphs[1].Forced = true;
        subtitle.Paragraphs[2].Forced = true;
        new SubtitleMarksPersistence(subtitle, _fileName).Save();

        var loaded = MakeSubtitle(1, 2, 3, 4);
        Assert.True(new SubtitleMarksPersistence(loaded, _fileName).Load());

        Assert.Equal("with \"quotes\", and a comma", loaded.Paragraphs[0].Bookmark);
        Assert.Null(loaded.Paragraphs[1].Bookmark);
        Assert.Equal(string.Empty, loaded.Paragraphs[2].Bookmark);
        Assert.Equal(new[] { false, true, true, false }, new[]
        {
            loaded.Paragraphs[0].Forced,
            loaded.Paragraphs[1].Forced,
            loaded.Paragraphs[2].Forced,
            loaded.Paragraphs[3].Forced,
        });
    }

    [Fact]
    public void Load_AfterALineIsInsertedAbove_MarksStayOnTheirOwnLines()
    {
        var subtitle = MakeSubtitle(10, 20, 30);
        subtitle.Paragraphs[2].Bookmark = "third";
        subtitle.Paragraphs[2].Forced = true;
        new SubtitleMarksPersistence(subtitle, _fileName).Save();

        // Same subtitle with an extra line inserted at the front: with index keying every mark
        // moved one line down; with start-time keying they stay put.
        var shifted = MakeSubtitle(5, 10, 20, 30);
        new SubtitleMarksPersistence(shifted, _fileName).Load();

        Assert.Equal("third", shifted.Paragraphs[3].Bookmark);
        Assert.True(shifted.Paragraphs[3].Forced);
        Assert.Null(shifted.Paragraphs[0].Bookmark);
        Assert.Null(shifted.Paragraphs[1].Bookmark);
        Assert.Null(shifted.Paragraphs[2].Bookmark);
    }

    [Fact]
    public void Load_FrameRoundedTimes_StillMatch()
    {
        var subtitle = MakeSubtitle(10, 20);
        subtitle.Paragraphs[1].Forced = true;
        new SubtitleMarksPersistence(subtitle, _fileName).Save();

        // Saving to a frame based format and reading it back moves the times by up to a frame.
        var rounded = new Subtitle();
        rounded.Paragraphs.Add(new Paragraph("Line at 10", 10_042, 10_900));
        rounded.Paragraphs.Add(new Paragraph("Line at 20", 19_958, 20_900));
        new SubtitleMarksPersistence(rounded, _fileName).Load();

        Assert.False(rounded.Paragraphs[0].Forced);
        Assert.True(rounded.Paragraphs[1].Forced);
    }

    [Fact]
    public void Load_TwoLinesWithTheSameStartTime_GetOneMarkEach()
    {
        var subtitle = MakeSubtitle(10, 10, 10);
        subtitle.Paragraphs[0].Bookmark = "a";
        subtitle.Paragraphs[2].Bookmark = "b";
        new SubtitleMarksPersistence(subtitle, _fileName).Save();

        var loaded = MakeSubtitle(10, 10, 10);
        new SubtitleMarksPersistence(loaded, _fileName).Load();

        // Nothing tells them apart by time, but no line may take two marks and none may be lost.
        var marks = new[] { loaded.Paragraphs[0].Bookmark, loaded.Paragraphs[1].Bookmark, loaded.Paragraphs[2].Bookmark };
        Assert.Equal(2, marks.Count(m => m != null));
        Assert.Contains("a", marks);
        Assert.Contains("b", marks);
    }

    [Fact]
    public void Load_OldIndexOnlySidecar_StillResolves()
    {
        File.WriteAllText(
            _fileName + ".SE.bookmarks",
            "{\"bookmarks\":[\n{\"idx\":1,\"txt\":\"legacy\"}]}\n");

        var subtitle = MakeSubtitle(1, 2, 3);
        Assert.True(new SubtitleMarksPersistence(subtitle, _fileName).Load());

        Assert.Equal("legacy", subtitle.Paragraphs[1].Bookmark);
        Assert.Null(subtitle.Paragraphs[0].Bookmark);
        Assert.Null(subtitle.Paragraphs[2].Bookmark);
    }

    [Fact]
    public void Load_SidecarWithNoForcedArray_IsNotAnError()
    {
        File.WriteAllText(
            _fileName + ".SE.bookmarks",
            "{\"bookmarks\":[\n{\"ms\":2000,\"idx\":1,\"txt\":\"note\"}]}\n");

        var subtitle = MakeSubtitle(1, 2, 3);
        Assert.True(new SubtitleMarksPersistence(subtitle, _fileName).Load());

        Assert.Equal("note", subtitle.Paragraphs[1].Bookmark);
        Assert.DoesNotContain(subtitle.Paragraphs, p => p.Forced);
    }

    [Fact]
    public void Load_MarkWithNoMatchingLine_IsDropped()
    {
        var subtitle = MakeSubtitle(10, 20, 30);
        subtitle.Paragraphs[2].Forced = true;
        new SubtitleMarksPersistence(subtitle, _fileName).Save();

        // The marked line is gone and only two lines are left, so its stored index is out of
        // range too - the mark must not land on some other line.
        var shorter = MakeSubtitle(10, 20);
        new SubtitleMarksPersistence(shorter, _fileName).Load();

        Assert.DoesNotContain(shorter.Paragraphs, p => p.Forced);
    }

    [Fact]
    public void Save_UntitledSubtitle_WritesNothing()
    {
        var subtitle = MakeSubtitle(1);
        subtitle.Paragraphs[0].Forced = true;

        Assert.False(new SubtitleMarksPersistence(subtitle, string.Empty).Save());
        Assert.False(File.Exists(".SE.bookmarks"));
    }
}
