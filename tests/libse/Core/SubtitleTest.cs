using Nikse.SubtitleEdit.Core.Common;

namespace LibSETests.Core;

public class SubtitleTest
{

    [Fact]
    public void TestRemoveParagraphsByIds1()
    {
        var sub = new Subtitle();
        var p1 = new Paragraph("1", 0, 1000);
        var p2 = new Paragraph("2", 1000, 2000);
        var p3 = new Paragraph("3", 2000, 3000);
        sub.Paragraphs.Add(p1);
        sub.Paragraphs.Add(p2);
        sub.Paragraphs.Add(p3);

        int removedCount = sub.RemoveParagraphsByIds(new List<string> { p2.Id });
        Assert.Equal(1, removedCount);
        Assert.Equal(2, sub.Paragraphs.Count);
        Assert.Equal(sub.Paragraphs[0], p1);
        Assert.Equal(sub.Paragraphs[1], p3);
    }

    [Fact]
    public void TestRemoveParagraphsByIds2()
    {
        var sub = new Subtitle();
        var p1 = new Paragraph("1", 0, 1000);
        var p2 = new Paragraph("2", 1000, 2000);
        var p3 = new Paragraph("3", 2000, 3000);
        sub.Paragraphs.Add(p1);
        sub.Paragraphs.Add(p2);
        sub.Paragraphs.Add(p3);

        int removedCount = sub.RemoveParagraphsByIds(new List<string> { p2.Id, p3.Id });
        Assert.Equal(2, removedCount);
        Assert.Single(sub.Paragraphs);
        Assert.Equal(sub.Paragraphs[0], p1);
    }

    [Fact]
    public void TestRemoveParagraphsByIdices1()
    {
        var sub = new Subtitle();
        var p1 = new Paragraph("1", 0, 1000);
        var p2 = new Paragraph("2", 1000, 2000);
        var p3 = new Paragraph("3", 2000, 3000);
        sub.Paragraphs.Add(p1);
        sub.Paragraphs.Add(p2);
        sub.Paragraphs.Add(p3);

        int removedCount = sub.RemoveParagraphsByIndices(new List<int> { 1 });
        Assert.Equal(1, removedCount);
        Assert.Equal(2, sub.Paragraphs.Count);
        Assert.Equal(sub.Paragraphs[0], p1);
        Assert.Equal(sub.Paragraphs[1], p3);
    }

    [Fact]
    public void TestRemoveParagraphsByIdices2()
    {
        var sub = new Subtitle();
        var p1 = new Paragraph("1", 0, 1000);
        var p2 = new Paragraph("2", 1000, 2000);
        var p3 = new Paragraph("3", 2000, 3000);
        sub.Paragraphs.Add(p1);
        sub.Paragraphs.Add(p2);
        sub.Paragraphs.Add(p3);

        int removedCount = sub.RemoveParagraphsByIndices(new List<int> { 1, 2 });
        Assert.Equal(2, removedCount);
        Assert.Single(sub.Paragraphs);
        Assert.Equal(sub.Paragraphs[0], p1);
    }

    [Fact]
    public void TestRemoveEmptyLines()
    {
        var sub = new Subtitle();
        var p1 = new Paragraph("1", 0, 1000);
        var p2 = new Paragraph(" ", 1000, 2000);
        var p3 = new Paragraph("3", 2000, 3000);
        sub.Paragraphs.Add(p1);
        sub.Paragraphs.Add(p2);
        sub.Paragraphs.Add(p3);

        int removedCount = sub.RemoveEmptyLines();
        Assert.Equal(1, removedCount);
        Assert.Equal(2, sub.Paragraphs.Count);
        Assert.Equal(sub.Paragraphs[0], p1);
        Assert.Equal(sub.Paragraphs[1], p3);
    }

    [Fact]
    public void TestChangeFrameRate1()
    {
        var sub = new Subtitle();
        var p1 = new Paragraph("1", 0, 1000);
        var p2 = new Paragraph("2", 2000, 3000);
        sub.Paragraphs.Add(p1);
        sub.Paragraphs.Add(p2);

        sub.ChangeFrameRate(25.0, 25.0);
        Assert.Equal(2, sub.Paragraphs.Count);
        Assert.Equal(0, sub.Paragraphs[0].StartTime.TotalMilliseconds);
        Assert.Equal(1000, sub.Paragraphs[0].EndTime.TotalMilliseconds);
        Assert.Equal(2000, sub.Paragraphs[1].StartTime.TotalMilliseconds);
        Assert.Equal(3000, sub.Paragraphs[1].EndTime.TotalMilliseconds);
    }

    [Fact]
    public void TestChangeFrameRate2()
    {
        var sub = new Subtitle();
        var p1 = new Paragraph("1", 0, 1000);
        var p2 = new Paragraph("2", 2000, 3000);
        sub.Paragraphs.Add(p1);
        sub.Paragraphs.Add(p2);

        sub.ChangeFrameRate(25.0, 30.0);
        Assert.Equal(2, sub.Paragraphs.Count);
        Assert.Equal(0, sub.Paragraphs[0].StartTime.TotalMilliseconds);
        Assert.True(Math.Abs(sub.Paragraphs[0].EndTime.TotalMilliseconds - 833.33333333333) < 0.01);
        Assert.True(Math.Abs(sub.Paragraphs[1].StartTime.TotalMilliseconds - 1666.6666666666667) < 0.01);
        Assert.True(Math.Abs(sub.Paragraphs[1].EndTime.TotalMilliseconds - 2500) < 0.01);
    }

    [Fact]
    public void RenumberNormal()
    {
        var sub = new Subtitle();
        var p1 = new Paragraph("0", 0, 1000);
        var p2 = new Paragraph("0", 2000, 3000);
        sub.Paragraphs.Add(p1);
        sub.Paragraphs.Add(p2);
        sub.Renumber();
        Assert.Equal(2, sub.Paragraphs.Count);
        Assert.Equal(1, sub.Paragraphs[0].Number);
        Assert.Equal(2, sub.Paragraphs[1].Number);
    }

    [Fact]
    public void RenumberStartWith2()
    {
        var sub = new Subtitle();
        var p1 = new Paragraph("0", 0, 1000);
        var p2 = new Paragraph("0", 2000, 3000);
        sub.Paragraphs.Add(p1);
        sub.Paragraphs.Add(p2);
        sub.Renumber(2);
        Assert.Equal(2, sub.Paragraphs.Count);
        Assert.Equal(2, sub.Paragraphs[0].Number);
        Assert.Equal(3, sub.Paragraphs[1].Number);
    }


}
