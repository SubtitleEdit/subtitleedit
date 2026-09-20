using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Features.Video.TextToSpeech.ReviewSpeech;
using Nikse.SubtitleEdit.Logic.Media;

namespace UITests.Features.Video.TextToSpeech.ReviewSpeech;

/// <summary>
/// The waveform only draws the blocks around the view it was last handed, and the review window
/// only handed them over when the selected row changed - so scrolling on from the selected line
/// ran into waveform with no blocks on it (#15102).
/// </summary>
public class ReviewWaveformScrollTests : IDisposable
{
    private readonly List<Window> _windows = new();

    public void Dispose()
    {
        foreach (var window in _windows)
        {
            window.Close();
        }

        _windows.Clear();
    }

    private ReviewSpeechViewModel BuildWindowWithLines()
    {
        var vm = new ReviewSpeechViewModel(new FolderHelper(), new StubWindowService());
        _windows.Add(new ReviewSpeechWindow(vm));

        // One line every 10 seconds for 10 minutes.
        for (var i = 0; i < 60; i++)
        {
            var line = new SubtitleLineViewModel
            {
                Number = i + 1,
                Text = $"Line {i}",
                StartTime = TimeSpan.FromSeconds(i * 10),
                EndTime = TimeSpan.FromSeconds((i * 10) + 2),
            };
            line.UpdateDuration();
            vm.WaveformParagraphs.Add(line);
        }

        var peaks = new WavePeak2[126 * 600];
        for (var i = 0; i < peaks.Length; i++)
        {
            peaks[i] = new WavePeak2(200, -200);
        }

        vm.AudioVisualizer!.WavePeaks = new WavePeakData2(126, peaks);
        vm.WavePeakData = vm.AudioVisualizer.WavePeaks;
        return vm;
    }

    private static List<SubtitleLineViewModel> Displayed(ReviewSpeechViewModel vm)
    {
        var displayed = new List<SubtitleLineViewModel>();
        vm.AudioVisualizer!.CopyDisplayableParagraphs(displayed);
        return displayed;
    }

    [AvaloniaFact]
    public void ScrollingAwayFromTheLoadedBlocksLoadsTheOnesNowInView()
    {
        var vm = BuildWindowWithLines();
        vm.ReloadWaveformParagraphs();
        Assert.Contains(vm.WaveformParagraphs[0], Displayed(vm));
        Assert.DoesNotContain(vm.WaveformParagraphs[40], Displayed(vm));

        vm.AudioVisualizer!.StartPositionSeconds = 400;

        Assert.Contains(vm.WaveformParagraphs[40], Displayed(vm));
        Assert.DoesNotContain(vm.WaveformParagraphs[0], Displayed(vm));
    }

    [AvaloniaFact]
    public void ReloadingKeepsTheViewWhereTheUserScrolledIt()
    {
        var vm = BuildWindowWithLines();

        vm.AudioVisualizer!.StartPositionSeconds = 400;

        Assert.Equal(400, vm.AudioVisualizer.StartPositionSeconds, 3);
    }
}
