using Avalonia.Controls;
using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Features.Video.TextToSpeech.ReviewSpeech;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.VideoPlayers.LibMpvDynamic;
using System.Collections.ObjectModel;
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

        if (_cancellationTokenSource.IsCancellationRequested || _mpvContext == null)
        {
            lock (_playLock)
            {
                StopPlay();
                return;
            }
        }
        else if (_mpvContext != null)
        {
            lock (_playLock)
            {
                var paused = _mpvContext.IsPaused;
                if (paused)
                {
                    StopPlay();
                    return;
                }
            }
        }

        _timer.Start();
    }

    private void StopPlay()
    {
        _mpvContext?.Stop();
        _mpvContext?.Dispose();
        _mpvContext = null;
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
        }
        
        await _mpvContext.LoadFile(fileName);

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