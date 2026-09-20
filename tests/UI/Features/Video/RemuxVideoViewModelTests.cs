using System;
using System.IO;
using Avalonia.Headless.XUnit;
using Nikse.SubtitleEdit.Features.Video.RemuxVideo;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Media;

namespace UITests.Features.Video;

public class RemuxVideoViewModelTests
{
    private sealed class NullServiceProvider : IServiceProvider
    {
        public object? GetService(Type serviceType) => null;
    }

    private static RemuxVideoViewModel BuildViewModel()
    {
        return new RemuxVideoViewModel(
            new FileHelper(),
            new FolderHelper(),
            new WindowService(new NullServiceProvider()));
    }

    private static string WriteTempFile()
    {
        var fileName = Path.Combine(Path.GetTempPath(), $"remux-test-{Guid.NewGuid():N}.mkv");
        File.WriteAllBytes(fileName, new byte[] { 1, 2, 3 });
        return fileName;
    }

    /// <summary>
    /// ffmpeg splits "-metadata key=value" at the first "=" only and does not unescape the
    /// value, so a backslash-escaped "=" ended up in the track title verbatim.
    /// </summary>
    [Theory]
    [InlineData("Track 1=EN", "Track 1=EN")]
    [InlineData("a\"b", "a'b")]
    [InlineData("a\\b", "a_b")]
    [InlineData("Plain title", "Plain title")]
    [InlineData("", "")]
    public void EscapeFfmpegMetadata_KeepsEqualsAndNeutralisesQuotesAndBackslashes(string input, string expected)
    {
        Assert.Equal(expected, RemuxVideoViewModel.EscapeFfmpegMetadata(input));
    }

    [AvaloniaFact]
    public void OnClosing_WhileRemuxing_DeletesThePartialOutputWithoutThrowing()
    {
        var vm = BuildViewModel();
        var outputFileName = WriteTempFile();
        try
        {
            vm.OutputFileName = outputFileName;
            vm.IsRemuxing = true; // as if ffmpeg were running; no process object exists here

            vm.OnClosing();

            Assert.False(File.Exists(outputFileName));
        }
        finally
        {
            if (File.Exists(outputFileName))
            {
                File.Delete(outputFileName);
            }
        }
    }

    [AvaloniaFact]
    public void OnClosing_WhenIdle_KeepsTheFinishedOutput()
    {
        var vm = BuildViewModel();
        var outputFileName = WriteTempFile();
        try
        {
            vm.OutputFileName = outputFileName;
            vm.IsRemuxing = false;

            vm.OnClosing();

            Assert.True(File.Exists(outputFileName));
        }
        finally
        {
            if (File.Exists(outputFileName))
            {
                File.Delete(outputFileName);
            }
        }
    }
}
