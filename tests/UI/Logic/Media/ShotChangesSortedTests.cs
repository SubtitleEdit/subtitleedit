using Avalonia.Headless.XUnit;
using Nikse.SubtitleEdit.Controls.AudioVisualizerControl;
using Nikse.SubtitleEdit.Logic.Media;
using System.Collections.Generic;

namespace UITests.Logic.Media;

/// <summary>
/// Extend to next/previous shot change, snapping and drawing binary-search the waveform's shot
/// changes, but pasted or imported lists come in their own order - so the waveform keeps them sorted.
/// </summary>
public class ShotChangesSortedTests
{
    private const double NoMaxDuration = 100000;

    [AvaloniaFact]
    public void AudioVisualizer_UnsortedShotChanges_AreSorted()
    {
        var audioVisualizer = new AudioVisualizer { ShotChanges = new List<double> { 2, 0.5, 10 } };

        Assert.Equal(new List<double> { 0.5, 2, 10 }, audioVisualizer.ShotChanges);
    }

    [AvaloniaFact]
    public void ExtendEnd_UnsortedShotChanges_StopsAtTheNextShotChange()
    {
        var audioVisualizer = new AudioVisualizer { ShotChanges = new List<double> { 2, 0.5, 10 } };

        var result = ShotChangesHelper.GetExtendedEndMs(
            audioVisualizer.ShotChanges, startMs: 500, endMs: 1000, nextStartMs: null,
            outCuesGapMs: 0, minGapMs: 24, maxDurationMs: NoMaxDuration);

        Assert.Equal(2000, result);
    }

    [AvaloniaFact]
    public void ExtendStart_UnsortedShotChanges_StopsAtThePreviousShotChange()
    {
        var audioVisualizer = new AudioVisualizer { ShotChanges = new List<double> { 10, 2, 0.5 } };

        var result = ShotChangesHelper.GetExtendedStartMs(
            audioVisualizer.ShotChanges, startMs: 3000, endMs: 4000, previousEndMs: null,
            inCuesGapMs: 0, minGapMs: 24, maxDurationMs: NoMaxDuration);

        Assert.Equal(2000, result);
    }
}
