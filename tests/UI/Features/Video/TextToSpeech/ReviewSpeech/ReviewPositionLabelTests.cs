using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Features.Video.TextToSpeech.ReviewSpeech;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Media;

namespace UITests.Features.Video.TextToSpeech.ReviewSpeech;

/// <summary>
/// The review window shows the waveform playhead as a time code so a spot can be compared with
/// the original video (#15211).
/// </summary>
public class ReviewPositionLabelTests : IDisposable
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

    [AvaloniaFact]
    public void MovingThePlayheadUpdatesThePositionText()
    {
        var vm = new ReviewSpeechViewModel(new FolderHelper(), new StubWindowService());
        _windows.Add(new ReviewSpeechWindow(vm));
        var offsetSeconds = Se.Settings.General.CurrentVideoOffsetInMs / 1000.0;

        vm.AudioVisualizer!.CurrentVideoPositionSeconds = 373.5;

        Assert.Equal(TimeCode.FromSeconds(373.5 + offsetSeconds).ToDisplayString(), vm.PositionText);
    }

    [AvaloniaFact]
    public void GoingToAPositionOutsideTheViewScrollsTheWaveformThere()
    {
        var vm = new ReviewSpeechViewModel(new FolderHelper(), new StubWindowService());
        var window = new ReviewSpeechWindow(vm);
        _windows.Add(window);
        var peaks = new WavePeak2[126 * 600];
        vm.AudioVisualizer!.WavePeaks = new WavePeakData2(126, peaks);
        vm.WavePeakData = vm.AudioVisualizer.WavePeaks;
        window.Show();

        vm.GoToPosition(400);

        Assert.Equal(400, vm.AudioVisualizer.CurrentVideoPositionSeconds, 3);
        Assert.Equal(398, vm.AudioVisualizer.StartPositionSeconds, 3);
    }
}
