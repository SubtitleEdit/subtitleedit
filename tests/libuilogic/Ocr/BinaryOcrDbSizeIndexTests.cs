using Nikse.SubtitleEdit.UiLogic.Ocr;

namespace LibUiLogicTests.Ocr;

/// <summary>
/// The matcher reads compare images through a by-size index, but <see cref="BinaryOcrDb.CompareImages"/>
/// is a public list the UI adds to and removes from - the index has to follow it, in list order.
/// </summary>
public class BinaryOcrDbSizeIndexTests
{
    private static BinaryOcrBitmap Make(int width, int height, string text)
    {
        return new BinaryOcrBitmap(width, height) { Text = text };
    }

    [Fact]
    public void GetCompareImagesBySize_ReturnsOnlyThatSize_InListOrder()
    {
        var db = new BinaryOcrDb(string.Empty, false);
        var a = Make(10, 20, "a");
        var b = Make(11, 20, "b");
        var c = Make(10, 20, "c");
        db.CompareImages.AddRange(new[] { a, b, c });

        Assert.Equal(new[] { a, c }, db.GetCompareImagesBySize(10, 20));
        Assert.Equal(new[] { b }, db.GetCompareImagesBySize(11, 20));
        Assert.Empty(db.GetCompareImagesBySize(20, 10));
    }

    [Fact]
    public void GetCompareImagesBySize_FollowsAddAndRemoveOnTheList()
    {
        var db = new BinaryOcrDb(string.Empty, false);
        var a = Make(10, 20, "a");
        var b = Make(10, 20, "b");
        db.CompareImages.Add(a);
        db.CompareImages.Add(b);
        Assert.Equal(2, db.GetCompareImagesBySize(10, 20).Count);

        db.CompareImages.Remove(a);
        Assert.Equal(new[] { b }, db.GetCompareImagesBySize(10, 20));

        var c = Make(10, 20, "c");
        db.CompareImages.Add(c);
        Assert.Equal(new[] { b, c }, db.GetCompareImagesBySize(10, 20));

        // same count, same first row: only the last row tells
        db.CompareImages.Remove(c);
        var d = Make(10, 20, "d");
        db.CompareImages.Add(d);
        Assert.Equal(new[] { b, d }, db.GetCompareImagesBySize(10, 20));
    }

    [Fact]
    public void GetCompareImagesBySize_FollowsAReplacedList()
    {
        var db = new BinaryOcrDb(string.Empty, false);
        db.CompareImages.Add(Make(10, 20, "a"));
        Assert.Single(db.GetCompareImagesBySize(10, 20));

        var b = Make(10, 20, "b");
        db.CompareImages = new List<BinaryOcrBitmap> { b };
        Assert.Equal(new[] { b }, db.GetCompareImagesBySize(10, 20));
    }

    [Fact]
    public void MoveToFront_MovesInTheListAndInTheBucket()
    {
        var db = new BinaryOcrDb(string.Empty, false);
        var a = Make(10, 20, "a");
        var b = Make(11, 20, "b");
        var c = Make(10, 20, "c");
        db.CompareImages.AddRange(new[] { a, b, c });
        Assert.Equal(new[] { a, c }, db.GetCompareImagesBySize(10, 20));

        db.MoveToFront(c);

        Assert.Equal(new[] { c, a, b }, db.CompareImages);
        Assert.Equal(new[] { c, a }, db.GetCompareImagesBySize(10, 20));
    }

    [Fact]
    public void MoveToFront_ImageNotInTheList_DoesNothing()
    {
        var db = new BinaryOcrDb(string.Empty, false);
        var a = Make(10, 20, "a");
        db.CompareImages.Add(a);

        db.MoveToFront(Make(10, 20, "x"));

        Assert.Equal(new[] { a }, db.CompareImages);
    }

    [Fact]
    public void FindExactMatchItem_PicksTheSameImageAsFindExactMatch()
    {
        var db = new BinaryOcrDb(string.Empty, false);
        var a = Make(10, 20, "a");
        var b = Make(10, 20, "b"); // same size, hash and pixel count as a: the first one wins
        var c = Make(12, 20, "c");
        db.CompareImages.AddRange(new[] { a, b, c });

        var probe = Make(10, 20, "?");
        var index = db.FindExactMatch(probe);
        Assert.True(index >= 0);
        Assert.Same(db.CompareImages[index], db.FindExactMatchItem(probe));

        Assert.Equal(-1, db.FindExactMatch(Make(13, 20, "?")));
        Assert.Null(db.FindExactMatchItem(Make(13, 20, "?")));
    }
}
