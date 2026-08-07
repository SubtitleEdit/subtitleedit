using Nikse.SubtitleEdit.Features.Video.SpeechToText;

namespace UITests.Features.Video.SpeechToText;

public class SpeechToTextSubtitleFileNameTests : IDisposable
{
    private readonly string _folder;

    public SpeechToTextSubtitleFileNameTests()
    {
        _folder = Path.Combine(Path.GetTempPath(), "se-stt-name-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_folder);
    }

    public void Dispose()
    {
        Directory.Delete(_folder, true);
    }

    private string Video(string name) => Path.Combine(_folder, name);

    [Fact]
    public void NoLanguageCode_UsesVideoName()
    {
        var result = SpeechToTextViewModel.GetSubtitleFileName(Video("My Video.mkv"), null);

        Assert.Equal(Video("My Video.srt"), result);
    }

    [Fact]
    public void LanguageCode_IsInsertedBeforeExtension()
    {
        var result = SpeechToTextViewModel.GetSubtitleFileName(Video("My Video.mkv"), "fr");

        Assert.Equal(Video("My Video.fr.srt"), result);
    }

    [Fact]
    public void EmptyLanguageCode_UsesVideoName()
    {
        var result = SpeechToTextViewModel.GetSubtitleFileName(Video("My Video.mkv"), string.Empty);

        Assert.Equal(Video("My Video.srt"), result);
    }

    [Fact]
    public void ExistingFile_GetsCounter()
    {
        File.WriteAllText(Video("My Video.srt"), string.Empty);

        var result = SpeechToTextViewModel.GetSubtitleFileName(Video("My Video.mkv"), null);

        Assert.Equal(Video("My Video_2.srt"), result);
    }

    [Fact]
    public void ExistingFileWithLanguageCode_CounterKeepsLanguageTokenBeforeExtension()
    {
        // "My Video.fr_2.srt" would break media player language detection - the
        // counter must go on the base name: "My Video_2.fr.srt".
        File.WriteAllText(Video("My Video.fr.srt"), string.Empty);
        File.WriteAllText(Video("My Video_2.fr.srt"), string.Empty);

        var result = SpeechToTextViewModel.GetSubtitleFileName(Video("My Video.mkv"), "fr");

        Assert.Equal(Video("My Video_3.fr.srt"), result);
    }
}
