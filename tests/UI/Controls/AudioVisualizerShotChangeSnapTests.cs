using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Threading;
using Nikse.SubtitleEdit.Controls.AudioVisualizerControl;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Media;
using System;
using System.Collections.Generic;
using Configuration = Nikse.SubtitleEdit.Core.Common.Configuration;

namespace UITests.Controls;

/// <summary>
/// "Snap to shot changes" in the waveform (issue #13953).
///
/// The setting is one promise - a cue dragged near a cut lands on the cut - so every drag that
/// moves a cue has to keep it, not just the two edge-resize drags. Dragging a whole paragraph
/// snapped in SE4 and did not in SE5, which reads as the checkbox being broken.
///
/// A captured cue lands the beautify profile's configured gap away from the cut - in cues after it,
/// out cues before it, so an out cue doesn't bleed onto the next shot (issue #13984). A whole-paragraph drag preserves its duration, so whichever cue the cut captures,
/// the other one moves with it.
/// </summary>
public class AudioVisualizerShotChangeSnapTests
{
    private const int SampleRate = 126; // px per second at zoom 1
    private const double WidthPx = 800;
    private const double HeightPx = 200;
    private const double Fps = 25;

    // Default beautify profile: in cues capture within max(3, 5) frames, out cues within
    // max(10, 3) frames.
    private const double InCueSnapSeconds = 5 / Fps;
    private const double OutCueSnapSeconds = 10 / Fps;

    // Where a captured cue lands: the profile's in/out cues gap, in frames, either side of the cut.
    // Pinned to non-zero values so the tests would catch a regression back to the old hard-coded
    // offsets (exactly on the cut / one frame before it) - see issue #13984.
    private const int InCuesGapFrames = 2;
    private const int OutCuesGapFrames = 4;
    private const double InCuesGapSeconds = InCuesGapFrames / Fps;
    private const double OutCuesGapSeconds = OutCuesGapFrames / Fps;

    /// <summary>Pins every setting the snap maths reads, so the test does not drift with defaults.</summary>
    private sealed class SnapSettings : IDisposable
    {
        private readonly bool _snapToShotChanges = Se.Settings.Waveform.SnapToShotChanges;
        private readonly bool _snapToFrames = Se.Settings.Waveform.SnapToFrames;
        private readonly double _frameRate = Se.Settings.General.CurrentFrameRate;
        // The app keeps these two in lockstep (every writer sets both - see MainViewModel's
        // frame-rate paths and Se.cs). The snap distance reads the Se copy and the gap reads the
        // libse copy, so a test that pinned only one would measure two different frame rates.
        private readonly double _coreFrameRate = Configuration.Settings.General.CurrentFrameRate;
        private readonly int _inLeft, _inRight, _outLeft, _outRight, _inGap, _outGap;

        public SnapSettings(bool snapToShotChanges = true)
        {
            var p = Configuration.Settings.BeautifyTimeCodes.Profile;
            _inLeft = p.InCuesLeftRedZone;
            _inRight = p.InCuesRightRedZone;
            _outLeft = p.OutCuesLeftRedZone;
            _outRight = p.OutCuesRightRedZone;
            _inGap = p.InCuesGap;
            _outGap = p.OutCuesGap;

            Se.Settings.Waveform.SnapToShotChanges = snapToShotChanges;
            Se.Settings.Waveform.SnapToFrames = false;
            Se.Settings.General.CurrentFrameRate = Fps;
            Configuration.Settings.General.CurrentFrameRate = Fps;
            p.InCuesLeftRedZone = 3;
            p.InCuesRightRedZone = 5;
            p.OutCuesLeftRedZone = 10;
            p.OutCuesRightRedZone = 3;
            p.InCuesGap = InCuesGapFrames;
            p.OutCuesGap = OutCuesGapFrames;
        }

        public void Dispose()
        {
            var p = Configuration.Settings.BeautifyTimeCodes.Profile;
            Se.Settings.Waveform.SnapToShotChanges = _snapToShotChanges;
            Se.Settings.Waveform.SnapToFrames = _snapToFrames;
            Se.Settings.General.CurrentFrameRate = _frameRate;
            Configuration.Settings.General.CurrentFrameRate = _coreFrameRate;
            p.InCuesLeftRedZone = _inLeft;
            p.InCuesRightRedZone = _inRight;
            p.OutCuesLeftRedZone = _outLeft;
            p.OutCuesRightRedZone = _outRight;
            p.InCuesGap = _inGap;
            p.OutCuesGap = _outGap;
        }
    }

    private static WavePeakData2 MakePeaks(int seconds)
    {
        var peaks = new WavePeak2[SampleRate * seconds];
        for (var i = 0; i < peaks.Length; i++)
        {
            peaks[i] = new WavePeak2(8000, -8000);
        }

        return new WavePeakData2(SampleRate, peaks);
    }

    private static SubtitleLineViewModel Line(double startSeconds, double endSeconds) => new()
    {
        Text = "text",
        StartTime = TimeSpan.FromSeconds(startSeconds),
        EndTime = TimeSpan.FromSeconds(endSeconds),
    };

    private static (Window Window, AudioVisualizer Av) Open(List<double> shotChanges, params SubtitleLineViewModel[] lines)
    {
        var av = new AudioVisualizer
        {
            WavePeaks = MakePeaks(60),
            Width = WidthPx,
            Height = HeightPx,
        };

        av.SetPosition(0, new List<SubtitleLineViewModel>(lines), 0, 0, new List<SubtitleLineViewModel>());
        av.ShotChanges = shotChanges;

        var window = new Window { Width = WidthPx, Height = HeightPx, Content = av };
        window.Show();
        Dispatcher.UIThread.RunJobs();
        window.UpdateLayout();
        Dispatcher.UIThread.RunJobs();

        return (window, av);
    }

    private static void Drag(Window window, double fromX, double toX)
    {
        window.MouseDown(new Point(fromX, 100), MouseButton.Left, RawInputModifiers.None);
        window.MouseMove(new Point(toX, 100), RawInputModifiers.None);
        Dispatcher.UIThread.RunJobs();
        window.MouseUp(new Point(toX, 100), MouseButton.Left, RawInputModifiers.None);
    }

    // The reported bug: this drag did nothing special in SE5 - the paragraph slid straight past
    // the cut - while SE4 parked its start on it.
    [AvaloniaFact]
    public void MoveWholeLine_StartNearAShotChange_SnapsStartTheInCuesGapAfterIt()
    {
        using var _ = new SnapSettings();
        var (window, av) = Open(new List<double> { 1.5 }, Line(1, 3));
        var line = av.SelectedParagraph!;

        // Grab the middle of the line (2 s) and drag +60 px, putting the start at ~1.476 s -
        // inside the in cue capture distance of the cut at 1.5 s.
        Drag(window, 252, 312);

        Assert.Equal(1.5 + InCuesGapSeconds, line.StartTime.TotalSeconds, 6);
        Assert.Equal(2, line.Duration.TotalSeconds, 6); // a whole-line move keeps its duration
        window.Close();
    }

    [AvaloniaFact]
    public void MoveWholeLine_EndNearAShotChange_SnapsEndTheOutCuesGapBeforeIt()
    {
        using var _ = new SnapSettings();
        var (window, av) = Open(new List<double> { 3.5 }, Line(1, 3));
        var line = av.SelectedParagraph!;

        // Same drag, but now only the END lands near a cut.
        Drag(window, 252, 312);

        Assert.Equal(3.5 - OutCuesGapSeconds, line.EndTime.TotalSeconds, 6);
        Assert.Equal(3.5 - OutCuesGapSeconds - 2, line.StartTime.TotalSeconds, 6);
        Assert.Equal(2, line.Duration.TotalSeconds, 6);
        window.Close();
    }

    [AvaloniaFact]
    public void MoveWholeLine_NoShotChangeInRange_FollowsThePointer()
    {
        using var _ = new SnapSettings();
        var (window, av) = Open(new List<double> { 30.0 }, Line(1, 3));
        var line = av.SelectedParagraph!;

        Drag(window, 252, 312);

        Assert.Equal(1 + 60.0 / SampleRate, line.StartTime.TotalSeconds, 6);
        Assert.Equal(2, line.Duration.TotalSeconds, 6);
        window.Close();
    }

    [AvaloniaFact]
    public void MoveWholeLine_SnapToShotChangesOff_FollowsThePointer()
    {
        using var _ = new SnapSettings(snapToShotChanges: false);
        var (window, av) = Open(new List<double> { 1.5 }, Line(1, 3));
        var line = av.SelectedParagraph!;

        Drag(window, 252, 312);

        Assert.Equal(1 + 60.0 / SampleRate, line.StartTime.TotalSeconds, 6);
        window.Close();
    }

    [AvaloniaFact]
    public void MoveWholeLine_StartCaptureWinsOverEndCapture()
    {
        using var _ = new SnapSettings();
        // Cuts near both cues; the start is what the paragraph parks on.
        var (window, av) = Open(new List<double> { 1.5, 3.5 }, Line(1, 3));
        var line = av.SelectedParagraph!;

        Drag(window, 252, 312);

        Assert.Equal(1.5 + InCuesGapSeconds, line.StartTime.TotalSeconds, 6);
        window.Close();
    }

    // Regression guards: the resize drags kept their behaviour when the snap rule moved into a
    // shared helper.
    [AvaloniaFact]
    public void ResizeLeft_NearAShotChange_SnapsTheInCuesGapAfterIt()
    {
        using var _ = new SnapSettings();
        var (window, av) = Open(new List<double> { 1.5 }, Line(1, 3));
        var line = av.SelectedParagraph!;

        Drag(window, 126, 186); // left edge at 1 s, +60 px

        Assert.Equal(1.5 + InCuesGapSeconds, line.StartTime.TotalSeconds, 6);
        Assert.Equal(3, line.EndTime.TotalSeconds, 6); // the other edge stays put
        window.Close();
    }

    [AvaloniaFact]
    public void ResizeRight_NearAShotChange_SnapsTheOutCuesGapBeforeIt()
    {
        using var _ = new SnapSettings();
        var (window, av) = Open(new List<double> { 3.5 }, Line(1, 3));
        var line = av.SelectedParagraph!;

        Drag(window, 378, 438); // right edge at 3 s, +60 px

        Assert.Equal(3.5 - OutCuesGapSeconds, line.EndTime.TotalSeconds, 6);
        Assert.Equal(1, line.StartTime.TotalSeconds, 6);
        window.Close();
    }

    // Issue #13984: the landing offset is the profile's gap, not a hard-coded one frame. A profile
    // configured with a wider gap must actually widen the space between the cue and the cut.
    [AvaloniaFact]
    public void SnapEnd_LandingOffsetFollowsTheProfilesOutCuesGap()
    {
        using var _ = new SnapSettings();
        var profile = Configuration.Settings.BeautifyTimeCodes.Profile;

        profile.OutCuesGap = 1; // what the offset used to be hard-coded to
        var (window, av) = Open(new List<double> { 3.5 }, Line(1, 3));
        var line = av.SelectedParagraph!;
        Drag(window, 378, 438);
        var narrow = 3.5 - line.EndTime.TotalSeconds;
        Assert.Equal(1 / Fps, narrow, 6);
        window.Close();

        profile.OutCuesGap = 8;
        var (window2, av2) = Open(new List<double> { 3.5 }, Line(1, 3));
        var line2 = av2.SelectedParagraph!;
        Drag(window2, 378, 438);
        var wide = 3.5 - line2.EndTime.TotalSeconds;
        Assert.Equal(8 / Fps, wide, 6);
        Assert.True(wide > narrow);
        window2.Close();
    }

    // Out cues get a wider capture distance than in cues (the profile's out cues red zones are
    // larger), so the same offset that is too far for a start still catches an end.
    [AvaloniaFact]
    public void SnapDistances_ComeFromTheBeautifyProfileRedZones()
    {
        using var _ = new SnapSettings();
        const double Offset = 0.3; // between the 0.2 s in cue and 0.4 s out cue distances
        Assert.True(InCueSnapSeconds < Offset && Offset < OutCueSnapSeconds);

        var draggedStart = 1 + 60.0 / SampleRate;
        var draggedEnd = draggedStart + 2;

        // Too far for the start to be captured.
        var (window, av) = Open(new List<double> { draggedStart + Offset }, Line(1, 3));
        var line = av.SelectedParagraph!;
        Drag(window, 252, 312);
        Assert.Equal(draggedStart, line.StartTime.TotalSeconds, 6);
        window.Close();

        // Same offset, but an end at that range is still captured.
        var (window2, av2) = Open(new List<double> { draggedEnd + Offset }, Line(1, 3));
        var line2 = av2.SelectedParagraph!;
        Drag(window2, 252, 312);
        Assert.Equal(draggedEnd + Offset - OutCuesGapSeconds, line2.EndTime.TotalSeconds, 6);
        window2.Close();
    }
}
