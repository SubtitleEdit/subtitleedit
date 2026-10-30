using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Threading;
using Nikse.SubtitleEdit.Controls.VideoPlayer;
using Nikse.SubtitleEdit.Features.Shared;

namespace UITests.Features.Shared;

/// <summary>
/// The fullscreen video window must take keyboard focus itself (#16039). Avalonia sends every key
/// to the one globally focused element, and the window only used to get focus from its first
/// activation - which some X11 window managers (Hyprland's XWayland) never report. Escape and the
/// seek keys then went to the main window instead.
/// </summary>
public class FullScreenVideoWindowFocusTests : IDisposable
{
    private readonly List<Window> _windows = new();

    public void Dispose()
    {
        for (var i = _windows.Count - 1; i >= 0; i--)
        {
            _windows[i].Close();
        }

        _windows.Clear();
    }

    private (Window Owner, TextBox OwnerTextBox, FullScreenVideoWindow FullScreen) ShowFullScreenOverFocusedTextBox()
    {
        var textBox = new TextBox();
        var owner = new Window { Width = 800, Height = 600, Content = textBox };
        _windows.Add(owner);
        owner.Show();
        textBox.Focus();
        Dispatcher.UIThread.RunJobs();

        var fullScreen = new FullScreenVideoWindow(
            new VideoPlayerControl(new EmptyVideoPlayer()),
            string.Empty,
            string.Empty,
            0,
            100,
            () => { });
        _windows.Add(fullScreen);
        fullScreen.Show(owner);
        Dispatcher.UIThread.RunJobs();

        return (owner, textBox, fullScreen);
    }

    [AvaloniaFact]
    public void Show_MovesKeyboardFocusIntoFullScreenWindow()
    {
        var (_, textBox, fullScreen) = ShowFullScreenOverFocusedTextBox();

        var focused = fullScreen.FocusManager?.GetFocusedElement() as Visual;

        Assert.False(textBox.IsFocused);
        Assert.NotNull(focused);
        Assert.Same(fullScreen, TopLevel.GetTopLevel(focused));
    }

    [AvaloniaFact]
    public void FocusVideo_AfterFocusWentBackToOwner_TakesFocusAgain()
    {
        var (_, textBox, fullScreen) = ShowFullScreenOverFocusedTextBox();
        textBox.Focus();
        Assert.True(textBox.IsFocused);

        fullScreen.FocusVideo();

        Assert.False(textBox.IsFocused);
        Assert.Same(fullScreen, TopLevel.GetTopLevel(fullScreen.FocusManager?.GetFocusedElement() as Visual));
    }

    [AvaloniaFact]
    public void HandleForwardedKeyDown_Escape_ClosesWindow()
    {
        var (_, _, fullScreen) = ShowFullScreenOverFocusedTextBox();
        var closed = false;
        fullScreen.Closed += (_, _) => closed = true;

        var e = new KeyEventArgs { RoutedEvent = InputElement.KeyDownEvent, Key = Key.Escape };
        fullScreen.HandleForwardedKeyDown(e);
        Dispatcher.UIThread.RunJobs();

        Assert.True(e.Handled);
        Assert.True(closed);
        _windows.Remove(fullScreen);
    }
}
