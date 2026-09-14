using System;
using System.IO;
using Avalonia.Headless.XUnit;
using Nikse.SubtitleEdit.Features.Video.BurnIn;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Media;

namespace UITests.Features.Video;

/// <summary>
/// Batch job rows survive between Generate clicks, and "Skipped" was only ever written - never
/// cleared - so a row skipped in one run (missing subtitle) stayed skipped in the next run even
/// after the user had picked a subtitle file for it.
/// </summary>
public class BurnInJobItemStatusTests
{
    private sealed class NullServiceProvider : IServiceProvider
    {
        public object? GetService(Type serviceType) => null;
    }

    private static BurnInViewModel BuildViewModel()
    {
        return new BurnInViewModel(
            new FolderHelper(),
            new FileHelper(),
            new WindowService(new NullServiceProvider()));
    }

    [AvaloniaFact]
    public void MakeAssa_ResetsAStaleSkippedStatus_WhenTheSubtitleIsNowReadable()
    {
        var vm = BuildViewModel();
        var subtitleFileName = Path.Combine(Path.GetTempPath(), $"burn-in-status-{Guid.NewGuid():N}.srt");
        File.WriteAllText(subtitleFileName, "1\r\n00:00:01,000 --> 00:00:02,000\r\nHello\r\n\r\n");
        try
        {
            var jobItem = new BurnInJobItem("input.mp4", 1280, 720)
            {
                SubtitleFileName = subtitleFileName,
                Status = "Skipped", // left over from a run where the subtitle file was missing
            };

            var assaFileName = vm.MakeAssa(jobItem, subtitleFileName);

            Assert.NotEqual("Skipped", jobItem.Status);
            Assert.Equal(BurnInViewModel.StatusWaiting, jobItem.Status);
            Assert.False(string.IsNullOrEmpty(assaFileName));
        }
        finally
        {
            File.Delete(subtitleFileName);
        }
    }

    [AvaloniaFact]
    public void MakeAssa_MarksTheRowSkipped_WhenTheSubtitleFileIsMissing()
    {
        var vm = BuildViewModel();
        var jobItem = new BurnInJobItem("input.mp4", 1280, 720);

        var assaFileName = vm.MakeAssa(jobItem, Path.Combine(Path.GetTempPath(), $"missing-{Guid.NewGuid():N}.srt"));

        Assert.Equal("Skipped", jobItem.Status);
        Assert.Equal(string.Empty, assaFileName);
    }
}
