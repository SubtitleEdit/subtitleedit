using Nikse.SubtitleEdit.UiLogic.Ocr;
using SkiaSharp;

namespace UITests.Logic.Ocr;

public class BinaryOcrDbTests
{
    private static NikseBitmap2 MakeBitmap(int width, int height, params (int X, int Y)[] coloredPixels)
    {
        var bmp = new NikseBitmap2(width, height);
        foreach (var (x, y) in coloredPixels)
        {
            bmp.SetPixel(x, y, new SKColor(255, 255, 255, 255));
        }
        return bmp;
    }

    private static BinaryOcrDb SaveAndReload(BinaryOcrDb original)
    {
        var fileName = original.FileName;
        original.Save();
        return new BinaryOcrDb(fileName);
    }

    [Fact]
    public void SaveLoad_SingleRegularBitmap_RoundTripsCorrectly()
    {
        var fileName = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName() + ".db");
        try
        {
            var nbmp = MakeBitmap(8, 12, (1, 1), (2, 2), (3, 3), (4, 4));
            var bob = new BinaryOcrBitmap(nbmp, italic: false, expandCount: 0, text: "A", x: 5, y: 7);

            var db = new BinaryOcrDb(fileName, loadCompareImages: false);
            db.Add(bob);

            var reloaded = SaveAndReload(db);
            Assert.Single(reloaded.CompareImages);
            Assert.Empty(reloaded.CompareImagesExpanded);

            var loaded = reloaded.CompareImages[0];
            Assert.Equal("A", loaded.Text);
            Assert.Equal(8, loaded.Width);
            Assert.Equal(12, loaded.Height);
            Assert.Equal(5, loaded.X);
            Assert.Equal(7, loaded.Y);
            Assert.Equal(4, loaded.NumberOfColoredPixels);
            Assert.Equal(bob.Hash, loaded.Hash);
            Assert.False(loaded.Italic);
            Assert.Equal(0, loaded.ExpandCount);
            Assert.True(loaded.AreColorsEqual(bob));
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
    public void SaveLoad_PreservesItalicAndUtf8Text()
    {
        var fileName = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName() + ".db");
        try
        {
            var nbmp = MakeBitmap(6, 10, (0, 0), (5, 9));
            var bob = new BinaryOcrBitmap(nbmp, italic: true, expandCount: 0, text: "Æ", x: 3, y: 4);

            var db = new BinaryOcrDb(fileName, loadCompareImages: false);
            db.Add(bob);

            var reloaded = SaveAndReload(db);
            var loaded = reloaded.CompareImages[0];

            Assert.Equal("Æ", loaded.Text);
            Assert.True(loaded.Italic);
            Assert.True(loaded.AreColorsEqual(bob));
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
    public void SaveLoad_ExpandedBitmap_RoundTripsWithSubImages()
    {
        var fileName = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName() + ".db");
        try
        {
            var parentNbmp = MakeBitmap(20, 12, (1, 1), (2, 2), (3, 3));
            var sub1Nbmp = MakeBitmap(7, 12, (0, 0), (6, 11));
            var sub2Nbmp = MakeBitmap(7, 12, (3, 5));

            var parent = new BinaryOcrBitmap(parentNbmp, italic: false, expandCount: 3, text: "fi.", x: 0, y: 0)
            {
                ExpandedList = new List<BinaryOcrBitmap>
                {
                    new BinaryOcrBitmap(sub1Nbmp),
                    new BinaryOcrBitmap(sub2Nbmp),
                },
            };

            var db = new BinaryOcrDb(fileName, loadCompareImages: false);
            db.Add(parent);

            var reloaded = SaveAndReload(db);
            Assert.Empty(reloaded.CompareImages);
            Assert.Single(reloaded.CompareImagesExpanded);

            var loaded = reloaded.CompareImagesExpanded[0];
            Assert.Equal("fi.", loaded.Text);
            Assert.Equal(3, loaded.ExpandCount);
            Assert.Equal(2, loaded.ExpandedList.Count);
            Assert.True(loaded.AreColorsEqual(parent));
            Assert.Null(loaded.ExpandedList[0].Text);
            Assert.Null(loaded.ExpandedList[1].Text);
            Assert.True(loaded.ExpandedList[0].AreColorsEqual(parent.ExpandedList[0]));
            Assert.True(loaded.ExpandedList[1].AreColorsEqual(parent.ExpandedList[1]));
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
    public void SaveLoad_MixedRegularAndExpanded_PreservesBoth()
    {
        var fileName = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName() + ".db");
        try
        {
            var db = new BinaryOcrDb(fileName, loadCompareImages: false);
            db.Add(new BinaryOcrBitmap(MakeBitmap(5, 8, (1, 1)), false, 0, "x", 0, 0));
            db.Add(new BinaryOcrBitmap(MakeBitmap(6, 8, (2, 2)), false, 0, "y", 0, 0));

            var parent = new BinaryOcrBitmap(MakeBitmap(12, 8, (3, 3)), false, 2, "ll", 0, 0)
            {
                ExpandedList = new List<BinaryOcrBitmap> { new BinaryOcrBitmap(MakeBitmap(6, 8, (4, 4))) },
            };
            db.Add(parent);

            var reloaded = SaveAndReload(db);
            Assert.Equal(2, reloaded.CompareImages.Count);
            Assert.Single(reloaded.CompareImagesExpanded);
            Assert.Contains(reloaded.CompareImages, b => b.Text == "x");
            Assert.Contains(reloaded.CompareImages, b => b.Text == "y");
            Assert.Equal("ll", reloaded.CompareImagesExpanded[0].Text);
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
    public void Add_DuplicateRegular_Throws()
    {
        var fileName = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName() + ".db");
        try
        {
            var nbmp = MakeBitmap(5, 8, (1, 1), (2, 2));
            var first = new BinaryOcrBitmap(nbmp, false, 0, "z", 0, 0);
            var duplicate = new BinaryOcrBitmap(nbmp, false, 0, "z", 0, 0);

            var db = new BinaryOcrDb(fileName, loadCompareImages: false);
            db.Add(first);

            Assert.Throws<Exception>(() => db.Add(duplicate));
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
    public void Add_ExpandedWithMismatchedSubImageCount_Throws()
    {
        var fileName = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName() + ".db");
        try
        {
            // ExpandCount = 3 needs 2 sub-images (count - 1); we provide only 1.
            var parent = new BinaryOcrBitmap(MakeBitmap(10, 8, (1, 1)), false, 3, "abc", 0, 0)
            {
                ExpandedList = new List<BinaryOcrBitmap> { new BinaryOcrBitmap(MakeBitmap(5, 8)) },
            };

            var db = new BinaryOcrDb(fileName, loadCompareImages: false);
            Assert.Throws<Exception>(() => db.Add(parent));
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
    public void LoadCompareImages_MissingFile_LeavesEmpty()
    {
        var fileName = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName() + ".db");
        Assert.False(File.Exists(fileName));

        var db = new BinaryOcrDb(fileName);
        Assert.Empty(db.CompareImages);
        Assert.Empty(db.CompareImagesExpanded);
    }

    [Fact]
    public void FindExactMatch_ReturnsIndexAfterRoundTrip()
    {
        var fileName = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName() + ".db");
        try
        {
            var nbmp = MakeBitmap(5, 8, (1, 1), (3, 3));
            var bob = new BinaryOcrBitmap(nbmp, false, 0, "k", 0, 0);

            var db = new BinaryOcrDb(fileName, loadCompareImages: false);
            db.Add(bob);

            var reloaded = SaveAndReload(db);
            var probe = new BinaryOcrBitmap(MakeBitmap(5, 8, (1, 1), (3, 3)), false, 0, null!, 0, 0);
            Assert.Equal(0, reloaded.FindExactMatch(probe));
        }
        finally
        {
            if (File.Exists(fileName))
            {
                File.Delete(fileName);
            }
        }
    }
}
