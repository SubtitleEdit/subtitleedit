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
/// In cues land exactly on the cut; out cues land one frame before it, so they don't bleed onto
/// the next shot. A whole-paragraph drag preserves its duration, so whichever cue the cut captures,
/// the other one moves with it.
/// </summary>
public class AudioVisualizerShotChangeSnapTests
{
    private const int SampleRate = 126; // px per second at zoom 1
    private const double WidthPx = 800;
    private const double HeightPx = 200;
    private const double Fps = 25;
    private const double OneFrame = 1.0 / Fps;

    // Default beautify profile: in cues capture within max(3, 5) frames, out cues within
    // max(10, 3) frames.
    private const double InCueSnapSeconds = 5 / Fps;
    private const double OutCueSnapSeconds = 10 / Fps;

    /// <summary>Pins every setting the snap maths reads, so the test does not drift with defaults.</summary>
    private sealed class SnapSettings : IDisposable
    {
        private readonly bool _snapToShotChanges = Se.Settings.Waveform.SnapToShotChanges;
        private readonly bool _snapToFrames = Se.Settings.Waveform.SnapToFrames;
        private readonly double _frameRate = Se.Settings.General.CurrentFrameRate;
        private readonly int _inLeft, _inRight, _outLeft, _outRight;

        public SnapSettings(bool snapToShotChanges = true)
        {
            var p = Configuration.Settings.BeautifyTimeCodes.Profile;
            _inLeft = p.InCuesLeftRedZone;
            _inRight = p.InCuesRightRedZone;
            _outLeft = p.OutCuesLeftRedZone;
            _outRight = p.OutCuesRightRedZone;

            Se.Settings.Waveform.SnapToShotChanges = snapToShotChanges;
            Se.Settings.Waveform.SnapToFrames = false;
            Se.Settings.General.CurrentFrameRate = Fps;
            p.InCuesLeftRedZone = 3;
            p.InCuesRightRedZone = 5;
            p.OutCuesLeftRedZone = 10;
            p.OutCuesRightRedZone = 3;
        }

        public void Dispose()
        {
            var p = Configuration.Settings.BeautifyTimeCodes.Profile;
            Se.Settings.Waveform.SnapToShotChanges = _snapToShotChanges;
            Se.Settings.Waveform.SnapToFrames = _snapToFrames;
            Se.Settings.General.CurrentFrameRate = _frameRate;
            p.InCuesLeftRedZone = _inLeft;
            p.InCuesRightRedZone = _inRight;
            p.OutCuesLeftRedZone = _outLeft;
            p.OutCuesRightRedZone = _outRight;
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
    public void MoveWholeLine_StartNearAShotChange_SnapsStartOntoIt()
    {
        using var _ = new SnapSettings();
        var (window, av) = Open(new List<double> { 1.5 }, Line(1, 3));
        var line = av.SelectedParagraph!;

        // Grab the middle of the line (2 s) and drag +60 px, putting the start at ~1.476 s -
        // inside the in cue capture distance of the cut at 1.5 s.
        Drag(window, 252, 312);

        Assert.Equal(1.5, line.StartTime.TotalSeconds, 6);
        Assert.Equal(2, line.Duration.TotalSeconds, 6); // a whole-line move keeps its duration
        window.Close();
    }

    [AvaloniaFact]
    public void MoveWholeLine_EndNearAShotChange_SnapsEndOneFrameBeforeIt()
    {
        using var _ = new SnapSettings();
        var (window, av) = Open(new List<double> { 3.5 }, Line(1, 3));
        var line = av.SelectedParagraph!;

        // Same drag, but now only the END lands near a cut.
        Drag(window, 252, 312);

        Assert.Equal(3.5 - OneFrame, line.EndTime.TotalSeconds, 6);
        Assert.Equal(3.5 - OneFrame - 2, line.StartTime.TotalSeconds, 6);
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

        Assert.Equal(1.5, line.StartTime.TotalSeconds, 6);
        window.Close();
    }

    // Regression guards: the resize drags kept their behaviour when the snap rule moved into a
    // shared helper.
    [AvaloniaFact]
    public void ResizeLeft_NearAShotChange_SnapsOntoIt()
    {
        using var _ = new SnapSettings();
        var (window, av) = Open(new List<double> { 1.5 }, Line(1, 3));
        var line = av.SelectedParagraph!;

        Drag(window, 126, 186); // left edge at 1 s, +60 px

        Assert.Equal(1.5, line.StartTime.TotalSeconds, 6);
        Assert.Equal(3, line.EndTime.TotalSeconds, 6); // the other edge stays put
        window.Close();
    }

    [AvaloniaFact]
    public void ResizeRight_NearAShotChange_SnapsOneFrameBeforeIt()
    {
        using var _ = new SnapSettings();
        var (window, av) = Open(new List<double> { 3.5 }, Line(1, 3));
        var line = av.SelectedParagraph!;

        Drag(window, 378, 438); // right edge at 3 s, +60 px

        Assert.Equal(3.5 - OneFrame, line.EndTime.TotalSeconds, 6);
        Assert.Equal(1, line.StartTime.TotalSeconds, 6);
        window.Close();
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
        Assert.Equal(draggedEnd + Offset - OneFrame, line2.EndTime.TotalSeconds, 6);
        window2.Close();
    }
}
