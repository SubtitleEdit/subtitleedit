using Nikse.SubtitleEdit.Core.SubtitleFormats;
using SeConv.Core;
using Xunit;

namespace SeConvTests.Core;

/// <summary>
/// `seconv formats` marked every binary format "(input)" although PAC, Cavena 890, Cheetah,
/// CapMaker and Ayato are written fine, listed EBU STL and DVB Teletext as "text", and a
/// conversion to a format that really is input-only wrote its stub ToText output.
/// </summary>
public class FormatCatalogTest : IDisposable
{
    private readonly string _tempRoot;

    public FormatCatalogTest()
    {
        _tempRoot = Path.Combine(Path.GetTempPath(), "FmtCat_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_tempRoot);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempRoot))
            Directory.Delete(_tempRoot, recursive: true);
    }

    private static string KindOf(string name)
    {
        return LibSEIntegration.GetAvailableFormats().Single(e => e.Format.Name == name).Kind;
    }

    [Fact]
    public void WritableBinaryFormats_AreNotMarkedInputOnly()
    {
        Assert.Equal("binary", KindOf(Pac.NameOfFormat));
        Assert.Equal("binary", KindOf(Cavena890.NameOfFormat));
        Assert.Equal("binary", KindOf(CheetahCaption.NameOfFormat));
        Assert.Equal("binary", KindOf(CapMakerPlus.NameOfFormat));
        Assert.Equal("binary", KindOf(new Ayato().Name));
    }

    [Fact]
    public void RegisteredBinaryFormats_AreBinaryNotText()
    {
        Assert.Equal("binary", KindOf(Ebu.NameOfFormat));
        Assert.Equal("binary", KindOf(new DvbTeletext().Name));
        Assert.Equal("text", KindOf(SubRip.NameOfFormat));
    }

    [Fact]
    public void ReadOnlyFormats_AreMarkedInputOnly()
    {
        Assert.Equal("binary (input)", KindOf(Chk.NameOfFormat));
        Assert.Equal("text (input)", KindOf(new JsonTypeOnlyLoad1().Name));
    }

    [Fact]
    public void EveryListedFormat_AgreesWithCanWrite()
    {
        foreach (var entry in LibSEIntegration.GetAvailableFormats())
        {
            Assert.Equal(LibSEIntegration.CanWrite(entry.Format), !entry.Kind.Contains("(input)"));
        }
    }

    [Theory]
    [InlineData("chk")]
    [InlineData("JsonTypeOnlyload1")]
    public async Task ConvertAsync_ToInputOnlyFormat_FailsWithoutWritingAFile(string format)
    {
        var input = Path.Combine(_tempRoot, "in.srt");
        await File.WriteAllTextAsync(input, "1\n00:00:01,000 --> 00:00:04,000\nHello, World!\n", TestContext.Current.CancellationToken);
        var outFolder = Path.Combine(_tempRoot, "out");

        var result = await new SubtitleConverter().ConvertAsync(new ConversionOptions
        {
            Patterns = [input],
            Format = format,
            OutputFolder = outFolder,
            Overwrite = true,
        });

        Assert.False(result.Success);
        Assert.Contains(result.Errors, e => e.Contains("can be read but not written", StringComparison.Ordinal));
        Assert.True(!Directory.Exists(outFolder) || Directory.GetFiles(outFolder).Length == 0);
    }
}
