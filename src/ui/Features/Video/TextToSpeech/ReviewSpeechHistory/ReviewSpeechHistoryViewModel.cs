using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Shared;
using Nikse.SubtitleEdit.Features.Video.TextToSpeech.ReviewSpeech;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.VideoPlayers.LibMpvDynamic;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

namespace Nikse.SubtitleEdit.Features.Video.TextToSpeech.ElevenLabsSettings;

public partial class ReviewSpeechHistoryViewModel : ObservableObject
{
    [ObservableProperty] private ObservableCollection<ReviewHistoryRow> _historyItems;
    [ObservableProperty] private ReviewHistoryRow? _selectedHistoryItem;

    public Window? Window { get; set; }

    public bool OkPressed { get; private set; }

    private LibMpvDynamicPlayer? _mpvContext;
    private Lock _playLock;
    private readonly System.Timers.Timer _timer;
    private CancellationTokenSource _cancellationTokenSource;
    private CancellationToken _cancellationToken;

    public ReviewSpeechHistoryViewModel()
    {
        HistoryItems = new ObservableCollection<ReviewHistoryRow>();

        _cancellationTokenSource = new CancellationTokenSource();
        _cancellationToken = _cancellationTokenSource.Token;
        _playLock = new Lock();
        _timer = new System.Timers.Timer(200);
        _timer.Elapsed += OnTimerOnElapsed;
        _timer.Start();
    }

    private void OnTimerOnElapsed(object? sender, ElapsedEventArgs e)
    {
        _timer.Stop();

        // Same shape as the review window's watchdog: read mpv state under the lock (the
        // null check used to happen outside it, racing the dispose in OnWindowClosing) and
        // decide first, act after - the row properties are UI-bound and must be set on the
        // dispatcher, not on this threadpool thread.
        var stop = false;
        lock (_playLock)
        {
            if (_cancellationTokenSource.IsCancellationRequested || _mpvContext == null || _mpvContext.IsPaused)
            {
                stop = true;
            }
        }

        if (stop)
        {
            Dispatcher.UIThread.Post(StopPlay);
            return;
        }

        _timer.Start();
    }

    private void StopPlay()
    {
        lock (_playLock)
        {
            _mpvContext?.Stop();
            _mpvContext?.Dispose();
            _mpvContext = null;
        }

        foreach (ReviewHistoryRow row in HistoryItems)
        {
            row.IsPlaying = false;
            row.IsPlayingEnabled = true;
        }
    }

    private async Task PlayAudio(string fileName)
    {
        lock (_playLock)
        {
            _mpvContext?.Stop();
            _mpvContext?.Dispose();

            _mpvContext = new LibMpvDynamicPlayer();
            _mpvContext.LoadLib(); // core not initialized"
            var err = _mpvContext.Initialize();
            if (err < 0)
            {
                throw new InvalidOperationException($"Failed to initialize mpv: {_mpvContext.GetErrorString(err)}");
            }
        }

        await _mpvContext.LoadAudio(fileName);

        foreach (var row in HistoryItems)
        {
            row.IsPlayingEnabled = false;
        }

        _timer.Start();
    }

    [RelayCommand]
    private void Ok()
    {
        OkPressed = true;
        Window?.Close();
    }

    [RelayCommand]
    private void Cancel()
    {
        Window?.Close();
    }

    [RelayCommand]
    private async Task PlayItem(ReviewHistoryRow? item)
    {
        if (item == null)
        {
            return;
        }

        // A history entry can point at a missing file (exported without audio, cleaned temp
        // folder). mpv's failed loadfile is fire-and-forget, so playing it wedged the dialog in
        // "playing" with every row disabled for its lifetime.
        if (string.IsNullOrEmpty(item.FileName) || !File.Exists(item.FileName))
        {
            SeLogger.Error($"ReviewSpeechHistory: cannot play missing audio file \"{item.FileName}\"");
            if (Window != null)
            {
                await MessageBox.Show(
                    Window,
                    Se.Language.General.Error,
                    "The audio file for this history entry does not exist:" + Environment.NewLine + item.FileName,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }

            return;
        }

        // A CancellationTokenSource is one-shot: after StopItem cancels it, the timer's
        // IsCancellationRequested check killed every later play within 200 ms - one Stop
        // permanently broke playback in this dialog. Renew it per play. (The old instance is
        // intentionally not disposed here; the timer thread may still be reading it.)
        if (_cancellationTokenSource.IsCancellationRequested)
        {
            _cancellationTokenSource = new CancellationTokenSource();
            _cancellationToken = _cancellationTokenSource.Token;
        }

        item.IsPlaying = true;
        await PlayAudio(item.FileName);
    }

    [RelayCommand]
    private void StopItem(ReviewHistoryRow? item)
    {
        if (item == null)
        {
            return;
        }

        item.IsPlaying = false;
        _cancellationTokenSource.Cancel();
    }

    internal void OnKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            e.Handled = true;
            Window?.Close();
        }
    }

    internal void Initialize(ReviewRow line)
    {
        foreach (var item in line.HistoryItems)
        {
            HistoryItems.Add(item);
        }

        if (HistoryItems.Count > 0)
        {
            SelectedHistoryItem = HistoryItems[0];
        }
    }

    internal void OnWindowClosing(WindowClosingEventArgs e)
    {
        _timer.Stop();
        _cancellationTokenSource.Cancel();
        lock (_playLock)
        {
            _mpvContext?.Stop();
            _mpvContext?.Dispose();
            _mpvContext = null;
        }

        UiUtil.SaveWindowPosition(Window);
    }

    internal void OnWindowLoaded()
    {
        UiUtil.RestoreWindowPosition(Window);
    }
}