using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Media;

namespace UITests.Features.Main;

/// <summary>
/// Guards the video lookup an OCR import does when it finishes (issue #13236): OCR'ing an image
/// subtitle left the video player empty, while opening a text subtitle with the same base name
/// opened the video. The import path now runs the same lookup, so what is verified here is that
/// the lookup resolves the file names OCR sources actually carry.
/// </summary>
public class OcrImportVideoLookupTests : IDisposable
{
    private readonly string _directory;

    public OcrImportVideoLookupTests()
    {
        _directory = Path.Combine(Path.GetTempPath(), "SubtitleEdit.OcrImportVideoLookupTests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_directory);
        Se.Settings.Video.OpenSearchParentFolder = false;
    }

    public void Dispose()
    {
        if (Directory.Exists(_directory))
        {
            Directory.Delete(_directory, recursive: true);
        }
    }

    [Fact]
    public void BluRaySupWithLanguageSuffix_FindsVideoWithSameName()
    {
        // The exact shape reported in #13236: "<name>.eng.sup" OCR'd next to "<name>.mkv".
        var video = CreateFile("{imdb-tt0090568} 1080p Bluray Remux h264 DTS-HD MA 2.0.mkv");
        var sup = Path.Combine(_directory, "{imdb-tt0090568} 1080p Bluray Remux h264 DTS-HD MA 2.0.eng.sup");

        Assert.True(FindVideoFileName.TryFindVideoFileName(sup, out var found));
        Assert.Equal(video, found);
    }

    [Fact]
    public void VobSub_FindsVideoWithSameName()
    {
        var video = CreateFile("movie.mp4");
        var sub = Path.Combine(_directory, "movie.sub");

        Assert.True(FindVideoFileName.TryFindVideoFileName(sub, out var found));
        Assert.Equal(video, found);
    }

    [Fact]
    public void NoMatchingVideo_LeavesPlayerAlone()
    {
        var sup = Path.Combine(_directory, "movie.eng.sup");

        Assert.False(FindVideoFileName.TryFindVideoFileName(sup, out var found));
        Assert.Equal(string.Empty, found);
    }

    [Fact]
    public void MatroskaVideo_IsOpenedAsItsOwnVideo()
    {
        // A track OCR'd out of an .mkv plays from the container itself...
        Assert.True(MainViewModel.IsMatroskaVideoFileName("/videos/movie.mkv"));
        Assert.True(MainViewModel.IsMatroskaVideoFileName("/videos/movie.MKV"));
    }

    [Fact]
    public void SubtitleOnlyMatroska_IsNotAVideo()
    {
        // ...but a .mks holds subtitles only, so it must fall through to the by-name lookup
        // instead of being handed to the player.
        Assert.False(MainViewModel.IsMatroskaVideoFileName("/videos/movie.mks"));
    }

    private string CreateFile(string name)
    {
        var path = Path.Combine(_directory, name);
        File.WriteAllText(path, string.Empty);
        return path;
    }
}
