using Avalonia.Input;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Features.Sync.VisualSync;
using Nikse.SubtitleEdit.Logic.Config;

namespace UITests.Features.Sync;

/// <summary>
/// Visual sync arrow-key steps: SE 4's Ctrl = 100 ms, Alt = 500 ms, Ctrl+Shift = 1 s (SE 5 had
/// Ctrl = 1 s, which made the finest step 500 ms), plus the main window's "Move start/end X ms"
/// shortcuts for steps finer than 100 ms (Reddit: hitting waveform edges exactly).
/// </summary>
public class VisualSyncSeekKeysTests : IDisposable
{
    private readonly List<SeShortCut> _shortcuts = Se.Settings.Shortcuts.ToList();
    private readonly int _stepMs = Se.Settings.General.MoveStartEndStepMs;

    public void Dispose()
    {
        Se.Settings.Shortcuts.Clear();
        Se.Settings.Shortcuts.AddRange(_shortcuts);
        Se.Settings.General.MoveStartEndStepMs = _stepMs;
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
    [InlineData(Key.Left, KeyModifiers.None)]
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

    private static void RemoveXMsBindings()
    {
        Se.Settings.Shortcuts.RemoveAll(s =>
            s.ActionName is nameof(MainViewModel.MoveStartXMsBackCommand) or nameof(MainViewModel.MoveStartXMsForwardCommand) or
                nameof(MainViewModel.MoveEndXMsBackCommand) or nameof(MainViewModel.MoveEndXMsForwardCommand));
    }
}
