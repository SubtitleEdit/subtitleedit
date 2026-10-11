using Avalonia.Input;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Features.Sync.VisualSync;
using Nikse.SubtitleEdit.Logic.Config;

namespace UITests.Features.Sync;

/// <summary>
/// Visual sync arrow-key steps: SE 4's Ctrl = 100 ms, Alt = 500 ms, Ctrl+Shift = 1 s (SE 5 had
/// Ctrl = 1 s, which made the finest step 500 ms), plus the main window's "Move start/end X ms"
/// shortcuts for steps finer than 100 ms (Reddit: hitting waveform edges exactly), and the main
/// window's video seek shortcuts - bare Left/Right = one second by default (#15789).
/// </summary>
public class VisualSyncSeekKeysTests : IDisposable
{
    private readonly List<SeShortCut> _shortcuts = Se.Settings.Shortcuts.ToList();
    private readonly int _stepMs = Se.Settings.General.MoveStartEndStepMs;
    private readonly double _frameRate = Se.Settings.General.CurrentFrameRate;
    private readonly int _custom1Back = Se.Settings.Video.MoveVideoPositionCustom1Back;

    public void Dispose()
    {
        Se.Settings.Shortcuts.Clear();
        Se.Settings.Shortcuts.AddRange(_shortcuts);
        Se.Settings.General.MoveStartEndStepMs = _stepMs;
        Se.Settings.General.CurrentFrameRate = _frameRate;
        Se.Settings.Video.MoveVideoPositionCustom1Back = _custom1Back;
    }

    private static KeyEventArgs Press(Key key, KeyModifiers modifiers) => new() { Key = key, KeyModifiers = modifiers };

    [Theory]
    [InlineData(Key.Left, KeyModifiers.Control, -0.1)]
    [InlineData(Key.Right, KeyModifiers.Control, 0.1)]
    [InlineData(Key.Left, KeyModifiers.Alt, -0.5)]
    [InlineData(Key.Right, KeyModifiers.Alt, 0.5)]
    [InlineData(Key.Left, KeyModifiers.Control | KeyModifiers.Shift, -1.0)]
    [InlineData(Key.Right, KeyModifiers.Control | KeyModifiers.Shift, 1.0)]
    public void ArrowSteps_AreSe4Steps(Key key, KeyModifiers modifiers, double expected)
    {
        RemoveXMsBindings();

        Assert.Equal(expected, VisualSyncViewModel.GetSeekSeconds(Press(key, modifiers))!.Value, 3);
    }

    [Theory]
    [InlineData(Key.Up, KeyModifiers.None)]
    [InlineData(Key.Up, KeyModifiers.Control)]
    [InlineData(Key.A, KeyModifiers.Control)]
    public void OtherKeys_DoNotSeek(Key key, KeyModifiers modifiers)
    {
        RemoveXMsBindings();

        Assert.Null(VisualSyncViewModel.GetSeekSeconds(Press(key, modifiers)));
    }

    [Fact]
    public void MoveStartEndXMsShortcuts_StepBySetting_AndWinOverBuiltInSteps()
    {
        RemoveXMsBindings();
        Se.Settings.General.MoveStartEndStepMs = 10;
        Se.Settings.Shortcuts.Add(new SeShortCut(nameof(MainViewModel.MoveStartXMsBackCommand), ["Ctrl", "Left"]));
        Se.Settings.Shortcuts.Add(new SeShortCut(nameof(MainViewModel.MoveEndXMsForwardCommand), ["Shift", "Right"]));

        Assert.Equal(-0.01, VisualSyncViewModel.GetSeekSeconds(Press(Key.Left, KeyModifiers.Control))!.Value, 4);
        Assert.Equal(0.01, VisualSyncViewModel.GetSeekSeconds(Press(Key.Right, KeyModifiers.Shift))!.Value, 4);
        Assert.Equal(0.1, VisualSyncViewModel.GetSeekSeconds(Press(Key.Right, KeyModifiers.Control))!.Value, 4);
    }

    [Theory]
    [InlineData(Key.Left, -1.0)]
    [InlineData(Key.Right, 1.0)]
    public void BareArrows_StepOneSecond_LikeMainWindowDefault(Key key, double expected)
    {
        RemoveXMsBindings();

        Assert.Equal(expected, VisualSyncViewModel.GetSeekSeconds(Press(key, KeyModifiers.None))!.Value, 3);
    }

    [Fact]
    public void MainWindowVideoSeekShortcuts_StepLikeMainWindow_AndWinOverBuiltInSteps()
    {
        RemoveXMsBindings();
        Se.Settings.General.CurrentFrameRate = 25;
        Se.Settings.Video.MoveVideoPositionCustom1Back = 2500;
        Se.Settings.Shortcuts.Add(new SeShortCut(nameof(MainViewModel.VideoOneFrameForwardCommand), ["Ctrl", "Right"]));
        Se.Settings.Shortcuts.Add(new SeShortCut(nameof(MainViewModel.VideoMoveCustom1BackCommand), ["Shift", "Left"]));
        Se.Settings.Shortcuts.Add(new SeShortCut(nameof(MainViewModel.Video500MsBackCommand), ["Alt", "J"]));
        Se.Settings.Shortcuts.Add(new SeShortCut(nameof(MainViewModel.VideoOneSecondBackCommand), []));

        Assert.Equal(0.04, VisualSyncViewModel.GetSeekSeconds(Press(Key.Right, KeyModifiers.Control))!.Value, 4);
        Assert.Equal(-2.5, VisualSyncViewModel.GetSeekSeconds(Press(Key.Left, KeyModifiers.Shift))!.Value, 4);
        Assert.Equal(-0.5, VisualSyncViewModel.GetSeekSeconds(Press(Key.J, KeyModifiers.Alt))!.Value, 4);
        Assert.Equal(-0.1, VisualSyncViewModel.GetSeekSeconds(Press(Key.Left, KeyModifiers.Control))!.Value, 4);
        Assert.Null(VisualSyncViewModel.GetSeekSeconds(Press(Key.Left, KeyModifiers.None)));
    }

    private static void RemoveXMsBindings()
    {
        Se.Settings.Shortcuts.RemoveAll(s =>
            s.ActionName is nameof(MainViewModel.MoveStartXMsBackCommand) or nameof(MainViewModel.MoveStartXMsForwardCommand) or
                nameof(MainViewModel.MoveEndXMsBackCommand) or nameof(MainViewModel.MoveEndXMsForwardCommand) or
                nameof(MainViewModel.VideoOneSecondBackCommand) or nameof(MainViewModel.VideoOneSecondForwardCommand) or
                nameof(MainViewModel.Video100MsBackCommand) or nameof(MainViewModel.Video100MsForwardCommand) or
                nameof(MainViewModel.Video500MsBackCommand) or nameof(MainViewModel.Video500MsForwardCommand) or
                nameof(MainViewModel.VideoOneFrameBackCommand) or nameof(MainViewModel.VideoOneFrameForwardCommand) or
                nameof(MainViewModel.VideoMoveCustom1BackCommand) or nameof(MainViewModel.VideoMoveCustom1ForwardCommand) or
                nameof(MainViewModel.VideoMoveCustom2BackCommand) or nameof(MainViewModel.VideoMoveCustom2ForwardCommand) or
                nameof(MainViewModel.VideoMoveCustom3BackCommand) or nameof(MainViewModel.VideoMoveCustom3ForwardCommand) or
                nameof(MainViewModel.VideoMoveCustom4BackCommand) or nameof(MainViewModel.VideoMoveCustom4ForwardCommand));
    }
}
