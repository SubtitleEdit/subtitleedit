using Nikse.SubtitleEdit.UiLogic.Ocr;

namespace UITests.Logic.Ocr;

public class NOcrCharTests
{
    private static NOcrChar SaveAndReload(NOcrChar original)
    {
        var fileName = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName() + ".nocr");
        try
        {
            var db1 = new NOcrDb(fileName);
            db1.Add(original);
            db1.Save();

            var db2 = new NOcrDb(fileName);
            return original.ExpandCount > 0
                ? db2.OcrCharactersExpanded[0]
                : db2.OcrCharacters[0];
        }
        finally
        {
            if (File.Exists(fileName))
            {
                File.Delete(fileName);
            }
        }
    }

    [Fact]
    public void SaveLoad_FewLines_RoundTripsCorrectly()
    {
        var original = new NOcrChar("A")
        {
            Width = 10,
            Height = 20,
            MarginTop = 5,
            Italic = true,
        };
        original.LinesForeground.Add(new NOcrLine(new OcrPoint(1, 2), new OcrPoint(8, 15)));
        original.LinesForeground.Add(new NOcrLine(new OcrPoint(3, 5), new OcrPoint(7, 18)));
        original.LinesBackground.Add(new NOcrLine(new OcrPoint(0, 0), new OcrPoint(2, 2)));

        var loaded = SaveAndReload(original);

        Assert.True(loaded.LoadedOk);
        Assert.Equal(original.Text, loaded.Text);
        Assert.Equal(original.Width, loaded.Width);
        Assert.Equal(original.Height, loaded.Height);
        Assert.Equal(original.MarginTop, loaded.MarginTop);
        Assert.Equal(original.Italic, loaded.Italic);
        Assert.Equal(2, loaded.LinesForeground.Count);
        Assert.Single(loaded.LinesBackground);
        Assert.Equal(original.LinesForeground[0].Start.X, loaded.LinesForeground[0].Start.X);
        Assert.Equal(original.LinesForeground[0].Start.Y, loaded.LinesForeground[0].Start.Y);
        Assert.Equal(original.LinesForeground[0].End.X, loaded.LinesForeground[0].End.X);
        Assert.Equal(original.LinesForeground[0].End.Y, loaded.LinesForeground[0].End.Y);
        Assert.Equal(original.LinesBackground[0].Start.X, loaded.LinesBackground[0].Start.X);
        Assert.Equal(original.LinesBackground[0].End.Y, loaded.LinesBackground[0].End.Y);
    }

    [Fact]
    public void SaveLoad_MoreThan255ForegroundLines_RoundTripsCorrectly()
    {
        var original = new NOcrChar("B")
        {
            Width = 100,
            Height = 80,
            MarginTop = 10,
        };
        for (var i = 0; i < 260; i++)
        {
            original.LinesForeground.Add(new NOcrLine(new OcrPoint(i % 98 + 1, i % 78 + 1), new OcrPoint(i % 98 + 2, i % 78 + 2)));
        }
        original.LinesBackground.Add(new NOcrLine(new OcrPoint(0, 0), new OcrPoint(5, 5)));

        var loaded = SaveAndReload(original);

        Assert.True(loaded.LoadedOk);
        Assert.Equal(original.Text, loaded.Text);
        Assert.Equal(original.Width, loaded.Width);
        Assert.Equal(original.Height, loaded.Height);
        Assert.Equal(original.MarginTop, loaded.MarginTop);
        Assert.Equal(260, loaded.LinesForeground.Count);
        Assert.Single(loaded.LinesBackground);
        Assert.Equal(original.LinesForeground[0].Start.X, loaded.LinesForeground[0].Start.X);
        Assert.Equal(original.LinesForeground[0].Start.Y, loaded.LinesForeground[0].Start.Y);
        Assert.Equal(original.LinesForeground[259].End.X, loaded.LinesForeground[259].End.X);
        Assert.Equal(original.LinesForeground[259].End.Y, loaded.LinesForeground[259].End.Y);
        Assert.Equal(original.LinesBackground[0].End.X, loaded.LinesBackground[0].End.X);
    }

    [Fact]
    public void SaveLoad_MarginTopMoreThan255_RoundTripsCorrectly()
    {
        var original = new NOcrChar("C")
        {
            Width = 50,
            Height = 60,
            MarginTop = 300,
        };
        original.LinesForeground.Add(new NOcrLine(new OcrPoint(5, 10), new OcrPoint(45, 55)));
        original.LinesBackground.Add(new NOcrLine(new OcrPoint(0, 0), new OcrPoint(3, 3)));

        var loaded = SaveAndReload(original);

        Assert.True(loaded.LoadedOk);
        Assert.Equal(original.Text, loaded.Text);
        Assert.Equal(original.Width, loaded.Width);
        Assert.Equal(original.Height, loaded.Height);
        Assert.Equal(300, loaded.MarginTop);
        Assert.Single(loaded.LinesForeground);
        Assert.Single(loaded.LinesBackground);
        Assert.Equal(original.LinesForeground[0].Start.X, loaded.LinesForeground[0].Start.X);
        Assert.Equal(original.LinesForeground[0].End.Y, loaded.LinesForeground[0].End.Y);
    }

    [Fact]
    public void NOcrDb_SaveLoad_ViaFile_RoundTripsCorrectly()
    {
        var original = new NOcrChar("X")
        {
            Width = 15,
            Height = 25,
            MarginTop = 3,
            Italic = false,
        };
        original.LinesForeground.Add(new NOcrLine(new OcrPoint(2, 3), new OcrPoint(12, 20)));
        original.LinesBackground.Add(new NOcrLine(new OcrPoint(0, 0), new OcrPoint(4, 4)));

        var loaded = SaveAndReload(original);

        Assert.Equal("X", loaded.Text);
        Assert.Equal(15, loaded.Width);
        Assert.Equal(25, loaded.Height);
        Assert.Equal(3, loaded.MarginTop);
        Assert.False(loaded.Italic);
        Assert.Single(loaded.LinesForeground);
        Assert.Single(loaded.LinesBackground);
        Assert.Equal(2, loaded.LinesForeground[0].Start.X);
        Assert.Equal(3, loaded.LinesForeground[0].Start.Y);
        Assert.Equal(12, loaded.LinesForeground[0].End.X);
        Assert.Equal(20, loaded.LinesForeground[0].End.Y);
    }
}
