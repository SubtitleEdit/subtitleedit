using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Nikse.SubtitleEdit.Controls.AudioVisualizerControl;
using Nikse.SubtitleEdit.Logic.Media;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Features.Tools.ImproveTimeCodes;

namespace UITests.Features.Tools.ImproveTimeCodes;

/// <summary>
/// A waveform only draws the blocks around the view it was last handed, and this window only
/// handed them over when the selected row or the result changed - so scrolling on from the
/// selected line ran into waveform with no blocks on it, in both waveforms (#15102).
/// </summary>
public class ImproveTimeCodesWaveformScrollTests : IDisposable
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

    private static List<SubtitleLineViewModel> Displayed(AudioVisualizer av)
    {
        var displayed = new List<SubtitleLineViewModel>();
        av.CopyDisplayableParagraphs(displayed);
        return displayed;
    }

    [AvaloniaFact]
    public void ScrollingOneWaveformLoadsTheBlocksNowInViewInBoth()
    {
        // One line every 10 seconds for 10 minutes.
        var lines = Enumerable.Range(0, 60).Select(i => new SubtitleLineViewModel
        {
            Number = i + 1,
            Text = $"Line {i}",
            StartTime = TimeSpan.FromSeconds(i * 10),
            EndTime = TimeSpan.FromSeconds((i * 10) + 2),
        }).ToList();

        var vm = new ImproveTimeCodesViewModel(new StubWindowService());
        vm.Initialize(lines, new AudioVisualizer(), "video.mkv", -1, "en");
        _windows.Add(new ImproveTimeCodesWindow(vm));

        var peaks = new WavePeak2[126 * 600];
        for (var i = 0; i < peaks.Length; i++)
        {
            peaks[i] = new WavePeak2(200, -200);
        }

        var original = vm.AudioVisualizerOriginal!;
        var aligned = vm.AudioVisualizerAligned!;
        original.WavePeaks = new WavePeakData2(126, peaks);
        aligned.WavePeaks = original.WavePeaks;
        vm.ReloadWaveformParagraphs(original);
        vm.ReloadWaveformParagraphs(aligned);
        Assert.Contains(Displayed(original), p => p.Number == 1);
        Assert.DoesNotContain(Displayed(original), p => p.Number == 41);

        original.StartPositionSeconds = 400;

        Assert.Equal(400, aligned.StartPositionSeconds, 3);
        Assert.Contains(Displayed(original), p => p.Number == 41);
        Assert.Contains(Displayed(aligned), p => p.Number == 41);
        Assert.DoesNotContain(Displayed(original), p => p.Number == 1);
    }
}
