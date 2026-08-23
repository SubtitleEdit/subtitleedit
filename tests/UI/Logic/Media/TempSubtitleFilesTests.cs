using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Logic.Media;

namespace UITests.Logic.Media;

/// <summary>
/// Burn-in, transparent render, cut, re-encode and blank video all hand ffmpeg a temp subtitle
/// file. They used to build the name as <c>Path.GetTempFileName() + extension</c>, which leaves
/// two files behind per call - the empty <c>tmpXXXX.tmp</c> the call itself creates plus the
/// <c>tmpXXXX.tmp.ass</c> actually written - and deleted neither (#13332).
/// </summary>
public class TempSubtitleFilesTests
{
    [Fact]
    public void Write_CreatesExactlyOneFileWithTheFormatExtension()
    {
        var files = new TempSubtitleFiles();
        try
        {
            var fileName = files.Write(MakeSubtitle(), new AdvancedSubStationAlpha());

            Assert.True(File.Exists(fileName));
            Assert.Equal(".ass", Path.GetExtension(fileName));
            Assert.Contains("Hello", File.ReadAllText(fileName), StringComparison.Ordinal);

            // The old idiom's giveaway: a sibling with the same name minus the extension.
            var strippedOfExtension = Path.Combine(
                Path.GetDirectoryName(fileName)!,
                Path.GetFileNameWithoutExtension(fileName));
            Assert.False(File.Exists(strippedOfExtension));
        }
        finally
        {
            files.Delete();
        }
    }

    [Fact]
    public void Write_GivesEveryCallItsOwnFile()
    {
        var files = new TempSubtitleFiles();
        try
        {
            var first = files.Write(MakeSubtitle(), new SubRip());
            var second = files.Write(MakeSubtitle(), new SubRip());

            Assert.NotEqual(first, second);
            Assert.True(File.Exists(first));
            Assert.True(File.Exists(second));
        }
        finally
        {
            files.Delete();
        }
    }

    [Fact]
    public void Delete_RemovesEveryFileWrittenSoFar()
    {
        var files = new TempSubtitleFiles();
        var written = files.Write(MakeSubtitle(), new SubRip());
        var reserved = files.GetFileName(".ass");
        File.WriteAllText(reserved, "[Script Info]");

        files.Delete();

        Assert.False(File.Exists(written));
        Assert.False(File.Exists(reserved));
    }

    [Fact]
    public void Delete_ToleratesAFileThatIsAlreadyGone()
    {
        var files = new TempSubtitleFiles();
        var fileName = files.Write(MakeSubtitle(), new SubRip());
        File.Delete(fileName);

        files.Delete();
        files.Delete();
    }

    [Fact]
    public void GetFileName_DoesNotCreateTheFileItself()
    {
        // Callers that write the content themselves (the Netflix Japanese ASSA conversion) must
        // get a name only - creating an empty file up front is the leak this class exists to fix.
        var files = new TempSubtitleFiles();
        var fileName = files.GetFileName(".ass");

        Assert.False(File.Exists(fileName));
        Assert.Equal(".ass", Path.GetExtension(fileName));

        files.Delete();
    }

    private static Subtitle MakeSubtitle()
    {
        var subtitle = new Subtitle();
        subtitle.Paragraphs.Add(new Paragraph("Hello", 0, 1000));
        return subtitle;
    }
}
