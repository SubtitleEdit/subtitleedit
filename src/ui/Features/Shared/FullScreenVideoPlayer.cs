using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Declarative;
using Avalonia.Media;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Features.Options.Shortcuts;
using Nikse.SubtitleEdit.Logic;
using System;
using System.Collections.Generic;

namespace Nikse.SubtitleEdit.Features.Shared;

public class FullScreenVideoWindow : Window
{
    private DispatcherTimer? _mouseMoveDetectionTimer;
    private (int X, int Y) _lastCursorPosition;
    private (int X, int Y) _lastPointerMovedCursorPosition;

    public FullScreenVideoWindow(
        Controls.VideoPlayer.VideoPlayerControl videoPlayer,
        string videoFileName,
        string subtitleFileName,
        double position,
        double volume,
        Action onClose,
        List<string>? toggleShortcutKeys = null,
        List<string>? showMediaInformationKeys = null,
        Action<Window>? showMediaInformation = null,
        IReadOnlyList<(string name, List<string> keys, IRelayCommand command)>? extraBindings = null)
    {
        WindowState = WindowState.FullScreen;
        WindowDecorations = WindowDecorations.None;


        var grid = new Grid
        {
            Background = Brushes.Transparent // Enable hit testing for pointer events
        };
        grid.Children.Add(videoPlayer);

        Content = grid;

        const int mouseMovementMinPixels = 20;

        var shortcutManager = new ShortcutManager();
        if (toggleShortcutKeys != null && toggleShortcutKeys.Count > 0)
        {
            shortcutManager.RegisterShortcut(new ShortCut(
                nameof(FullScreenVideoWindow),
                toggleShortcutKeys,
                ShortcutCategory.General,
                new RelayCommand(Close)));
        }

        // Handle the Show-Media-Information shortcut locally so the dialog can be
        // opened with the fullscreen window as owner (otherwise it would appear
        // behind the fullscreen window which has the main window as its owner).
        if (showMediaInformationKeys != null && showMediaInformationKeys.Count > 0 && showMediaInformation != null)
        {
            shortcutManager.RegisterShortcut(new ShortCut(
                nameof(FullScreenVideoWindow) + "_ShowMediaInformation",
                showMediaInformationKeys,
                ShortcutCategory.General,
                new RelayCommand(() => showMediaInformation(this))));
        }

        // Honor the user-configured video shortcuts (jump back/forward, play/pause,
        // brightness, etc.) by registering them on the local shortcut manager.
        // The commands themselves come from the main view model and operate on
        // the fullscreen player because GetVideoPlayerControl() returns it while
        // fullscreen is open.
        if (extraBindings != null)
        {
            foreach (var (name, keys, command) in extraBindings)
            {
                if (keys == null || keys.Count == 0)
                {
                    continue;
                }

                shortcutManager.RegisterShortcut(new ShortCut(
                    nameof(FullScreenVideoWindow) + "_" + name,
                    keys,
                    ShortcutCategory.General,
                    command));
            }
        }

        // Poll for actual cursor position using platform APIs
        // This works regardless of Avalonia event handling or MPV
        _mouseMoveDetectionTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(100) };
        _mouseMoveDetectionTimer.Tick += (s, e) =>
        {
            try
            {
                var cursorPos = CursorPositionHelper.GetCursorPosition();
                if (cursorPos.HasValue)
                {
                    if (Math.Abs(cursorPos.Value.X - _lastCursorPosition.X) > mouseMovementMinPixels ||
                        Math.Abs(cursorPos.Value.Y - _lastCursorPosition.Y) > mouseMovementMinPixels)
                    {
                        _lastCursorPosition = cursorPos.Value;
                        videoPlayer.NotifyUserActivity();
                    }
                }
            }
            catch
            {
                // Ignore errors
            }
        };

        // Keep these handlers as fallback if native APIs fail
        grid.PointerMoved += (_, e) =>
        {
            var pos = e.GetCurrentPoint(this);
            if (Math.Abs(pos.Position.X - _lastPointerMovedCursorPosition.X) > mouseMovementMinPixels ||
                Math.Abs(pos.Position.Y - _lastPointerMovedCursorPosition.Y) > mouseMovementMinPixels)
            {
                videoPlayer.NotifyUserActivity();
                _lastPointerMovedCursorPosition = ((int)pos.Position.X, (int)pos.Position.Y);
            }

            videoPlayer.IsFullScreen = true;
        };

        KeyDown += (_, e) =>
        {
            // Window-management keys always win — Esc/F11 must close the
            // fullscreen window regardless of user shortcut bindings.
            if (e.Key == Key.Escape || e.Key == Key.F11)
            {
                e.Handled = true;
                Close();
                videoPlayer.NotifyUserActivity();
                return;
            }

            // User-configured shortcuts win over hardcoded defaults. Without
            // this, a user with "1 sec forward" bound to Right would still get
            // a hardcoded 2-second jump (issue #11392).
            shortcutManager.OnKeyPressed(this, e);
            var command = shortcutManager.CheckShortcuts(e, ShortcutCategory.General.ToString());
            if (command != null)
            {
                e.Handled = true;
                command.Execute(null);
                videoPlayer.NotifyUserActivity();
                return;
            }

            // Fallbacks for keys the user hasn't bound to anything.
            // Volume Up/Down stay here because there are no Volume RelayCommands
            // in the main view model — these are the only way to control volume
            // from the keyboard in fullscreen.
            switch (e.Key)
            {
                case Key.Space:
                    videoPlayer.TogglePlayPause();
                    break;
                case Key.Right:
                    videoPlayer.Position += 2;
                    break;
                case Key.Left:
                    videoPlayer.Position -= 2;
                    break;
                case Key.Up when e.KeyModifiers == KeyModifiers.None:
                    videoPlayer.Volume += 2;
                    break;
                case Key.Down when e.KeyModifiers == KeyModifiers.None:
                    videoPlayer.Volume -= 2;
                    break;
                default:
                    videoPlayer.NotifyUserActivity();
                    return;
            }

            e.Handled = true;
            videoPlayer.NotifyUserActivity();
        };

        KeyUp += (_, e) => shortcutManager.OnKeyReleased(this, e);

        videoPlayer.FullscreenCollapseRequested += Close;

        Closing += (_, _) =>
        {
            onClose?.Invoke();
            _mouseMoveDetectionTimer?.Stop();
            _mouseMoveDetectionTimer = null;
            videoPlayer.FullscreenCollapseRequested -= Close;
            videoPlayer.VideoPlayer.CloseFile();
        };

        Activated += delegate { Focus(); }; // hack to make OnKeyDown work
        Loaded += async(_, _) =>
        {
            WindowState = WindowState.Maximized;
            WindowState = WindowState.FullScreen;

            // Start polling for cursor movement
            _mouseMoveDetectionTimer?.Start();

            await videoPlayer.Open(videoFileName);
            await videoPlayer.WaitForPlayersReadyAsync();
            videoPlayer.VideoPlayer.Pause();
            videoPlayer.VideoPlayer.Position = position;
            videoPlayer.Position = position;
            videoPlayer.Volume = volume;
        };
    }
}
