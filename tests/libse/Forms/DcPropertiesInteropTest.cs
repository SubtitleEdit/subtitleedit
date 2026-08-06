using Nikse.SubtitleEdit.Core.Forms;

namespace LibSETests.Forms;

public class DcPropertiesInteropTest
{
    [Fact]
    public void SaveAndLoad_RoundTripsAllProperties()
    {
        var fileName = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N") + ".DCinema-interop-profile");
        try
        {
            var exporter = new DcPropertiesInterop
            {
                GenerateIdAuto = "True",
                ReelNumber = "3",
                Language = "English",
                FontId = "Font1",
                FontUri = "Arial.ttf",
                FontColor = "#FFFFFFFF",
                Effect = "Border",
                EffectColor = "#FF000000",
                FontSize = "42",
                TopBottomMargin = "8",
                FadeUpTime = "2",
                FadeDownTime = "4",
                ZPosition = "-1.25",
            };

            Assert.True(exporter.Save(fileName));

            var importer = new DcPropertiesInterop();
            Assert.True(importer.Load(fileName));

            Assert.Equal("True", importer.GenerateIdAuto);
            Assert.Equal("3", importer.ReelNumber);
            Assert.Equal("English", importer.Language);
            Assert.Equal("Font1", importer.FontId);
            Assert.Equal("Arial.ttf", importer.FontUri);
            Assert.Equal("#FFFFFFFF", importer.FontColor);
            Assert.Equal("Border", importer.Effect);
            Assert.Equal("#FF000000", importer.EffectColor);
            Assert.Equal("42", importer.FontSize);
            Assert.Equal("8", importer.TopBottomMargin);
            Assert.Equal("2", importer.FadeUpTime);
            Assert.Equal("4", importer.FadeDownTime);
            Assert.Equal("-1.25", importer.ZPosition);
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
    public void Load_MissingFile_ReturnsFalse()
    {
        var importer = new DcPropertiesInterop();
        Assert.False(importer.Load(Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"))));
    }
}
