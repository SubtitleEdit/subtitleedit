using Nikse.SubtitleEdit.Logic.Config;

namespace UITests.Logic;

/// <summary>
/// The Tesseract folder used to be version-stamped ("Tesseract550"), so bumping Tesseract
/// orphaned every downloaded model - on macOS/Linux, where the binary comes from brew/apt,
/// the models were the only thing in there. The folder is now version-less and the old one
/// is moved across once, models included.
/// </summary>
public class TesseractFolderMigrationTests : IDisposable
{
    private readonly string _dataFolder = Path.Combine(
        Path.GetTempPath(),
        "SeTesseractMigrationTests_" + Guid.NewGuid().ToString("N"));

    private string Current => Path.Combine(_dataFolder, "Tesseract");
    private string Legacy => Path.Combine(_dataFolder, "Tesseract550");

    public TesseractFolderMigrationTests()
    {
        Directory.CreateDirectory(_dataFolder);
    }

    public void Dispose()
    {
        try
        {
            Directory.Delete(_dataFolder, true);
        }
        catch
        {
            // best effort
        }

        GC.SuppressFinalize(this);
    }

    [Fact]
    public void ReturnsVersionLessFolderOnAFreshInstall()
    {
        var folder = Se.ResolveTesseractDataFolder(_dataFolder);

        Assert.Equal(Current, folder);
        Assert.False(Directory.Exists(Legacy));
    }

    [Fact]
    public void MovesTheLegacyFolderAcrossWithTheDownloadedModels()
    {
        Directory.CreateDirectory(Path.Combine(Legacy, "tessdata"));
        File.WriteAllText(Path.Combine(Legacy, "tessdata", "deu.traineddata"), "model");
        File.WriteAllText(Path.Combine(Legacy, "tesseract.exe"), "binary");

        var folder = Se.ResolveTesseractDataFolder(_dataFolder);

        Assert.Equal(Current, folder);
        Assert.False(Directory.Exists(Legacy));
        Assert.Equal("model", File.ReadAllText(Path.Combine(Current, "tessdata", "deu.traineddata")));
        Assert.True(File.Exists(Path.Combine(Current, "tesseract.exe")));
    }

    [Fact]
    public void KeepsTheAlreadyMigratedFolderWhenBothExist()
    {
        // An older SE build run in between can recreate the legacy folder; the migrated one wins
        // rather than being overwritten by it.
        Directory.CreateDirectory(Path.Combine(Current, "tessdata"));
        File.WriteAllText(Path.Combine(Current, "tessdata", "eng.traineddata"), "migrated");
        Directory.CreateDirectory(Path.Combine(Legacy, "tessdata"));
        File.WriteAllText(Path.Combine(Legacy, "tessdata", "eng.traineddata"), "stale");

        var folder = Se.ResolveTesseractDataFolder(_dataFolder);

        Assert.Equal(Current, folder);
        Assert.Equal("migrated", File.ReadAllText(Path.Combine(Current, "tessdata", "eng.traineddata")));
    }

    [Fact]
    public void IsIdempotent()
    {
        Directory.CreateDirectory(Path.Combine(Legacy, "tessdata"));
        File.WriteAllText(Path.Combine(Legacy, "tessdata", "eng.traineddata"), "model");

        var first = Se.ResolveTesseractDataFolder(_dataFolder);
        var second = Se.ResolveTesseractDataFolder(_dataFolder);

        Assert.Equal(first, second);
        Assert.Equal("model", File.ReadAllText(Path.Combine(Current, "tessdata", "eng.traineddata")));
    }
}
