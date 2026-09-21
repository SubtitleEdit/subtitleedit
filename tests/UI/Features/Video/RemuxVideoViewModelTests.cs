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

    [Theory]
    [InlineData(45, "00:45")]
    [InlineData(754, "12:34")]
    [InlineData(11645, "3:14:05")]
    public void FormatDuration_FormatsHoursAndMinutesCorrectly(int totalSeconds, string expected)
    {
        Assert.Equal(expected, RemuxFileItem.FormatDuration(TimeSpan.FromSeconds(totalSeconds)));
    }

    [AvaloniaFact]
    public void RemuxFileItem_SetDuration_UpdatesDetails()
    {
        var tempFile = WriteTempFile();
        try
        {
            var item = new RemuxFileItem(tempFile);
            item.SetDuration(TimeSpan.FromMinutes(5));
            Assert.Equal("05:00", item.DurationDisplay);
            Assert.StartsWith("05:00  -  ", item.Details);
        }
        finally
        {
            if (File.Exists(tempFile))
            {
                File.Delete(tempFile);
            }
        }
    }

    [Theory]
    [InlineData(10, 50, "00:10 elapsed · ~00:10 left")]
    [InlineData(9, 90, "00:09 elapsed · ~00:01 left")]
    [InlineData(60, 25, "01:00 elapsed · ~03:00 left")]
    [InlineData(3723, 50, "1:02:03 elapsed · ~1:02:03 left")]
    public void FormatProgressTime_WithValidPercent_IncludesElapsedAndRemaining(int elapsedSeconds, double percent, string expected)
    {
        Assert.Equal(expected, RemuxVideoViewModel.FormatProgressTime(TimeSpan.FromSeconds(elapsedSeconds), percent));
    }

    [Theory]
    [InlineData(10, 0, "00:10 elapsed")]
    [InlineData(10, 100, "00:10 elapsed")]
    [InlineData(0, 50, "00:00 elapsed")]
    public void FormatProgressTime_BoundaryPercent_ReturnsElapsedOnly(int elapsedSeconds, double percent, string expected)
    {
        Assert.Equal(expected, RemuxVideoViewModel.FormatProgressTime(TimeSpan.FromSeconds(elapsedSeconds), percent));
    }

    /// <summary>
    /// The dialog is modeless and remuxes any video, so closing it may only hand the output to
    /// the main window after "Done" on a finished remux of the video the main window still has
    /// loaded (or when it has none) - never over a video the user opened in the meantime.
    /// </summary>
    [AvaloniaTheory]
    [InlineData(true, true, "same", true)]
    [InlineData(true, true, "SAME-CASE", true)]
    [InlineData(true, true, "none", true)]
    [InlineData(true, true, "other", false)]
    [InlineData(false, true, "same", false)] // closed via Cancel / Escape / title-bar X
    [InlineData(true, false, "same", false)] // nothing was remuxed (or the inputs changed since)
    public void ShouldLoadOutputOnClose_OnlyTakesOverTheRemuxedVideo(bool donePressed, bool completed, string mainVideo, bool expected)
    {
        var vm = BuildViewModel();
        var outputFileName = WriteTempFile();
        try
        {
            var input = Path.Combine(Path.GetTempPath(), "remux-test-input-does-not-exist.mkv");
            vm.VideoFileName = input; // resets IsCompleted, so set it first
            vm.OutputFileName = outputFileName;
            vm.IsCompleted = completed;
            if (donePressed)
            {
                vm.DoneCommand.Execute(null);
            }

            var current = mainVideo switch
            {
                "same" => input,
                "SAME-CASE" => input.ToUpperInvariant(),
                "none" => null,
                _ => Path.Combine(Path.GetTempPath(), "another-video.mkv"),
            };

            Assert.Equal(expected, vm.ShouldLoadOutputOnClose(current));
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
    public void ShouldLoadOutputOnClose_MissingOutputFile_ReturnsFalse()
    {
        var vm = BuildViewModel();
        var input = Path.Combine(Path.GetTempPath(), "remux-test-input-does-not-exist.mkv");
        vm.VideoFileName = input;
        vm.OutputFileName = Path.Combine(Path.GetTempPath(), $"remux-test-missing-{Guid.NewGuid():N}.mkv");
        vm.IsCompleted = true;
        vm.DoneCommand.Execute(null);

        Assert.False(vm.ShouldLoadOutputOnClose(input));
    }
}
