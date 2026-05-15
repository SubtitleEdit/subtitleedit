using Avalonia.Controls;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Logic.Config;
using System;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading;

namespace Nikse.SubtitleEdit.Features.Video.SpeechToText.OpenAiCompatible;

public partial class TranscriptionProgressViewModel : ObservableObject
{
    [ObservableProperty]
    private string _statusText = string.Empty;

    [ObservableProperty]
    private string _streamedText = string.Empty;

    [ObservableProperty]
    private ObservableCollection<string> _receivedSegments = new();

    [ObservableProperty]
    private int _segmentCount;

    [ObservableProperty]
    private bool _isCompleted;

    [ObservableProperty]
    private string _modelName = string.Empty;

    [ObservableProperty]
    private string _serverUrl = string.Empty;

    private readonly CancellationTokenSource _cts = new();

    public Window? Window { get; set; }

    public CancellationToken CancellationToken => _cts.Token;

    public void UpdateStreamedText(string delta)
    {
        Dispatcher.UIThread.Post(() =>
        {
            StreamedText += delta;
        });
    }

    public void AddSegment(string segmentText, double start, double end)
    {
        Dispatcher.UIThread.Post(() =>
        {
            ReceivedSegments.Add($"[{TimeSpan.FromSeconds(start):mm\\:ss\\.fff} -> {TimeSpan.FromSeconds(end):mm\\:ss\\.fff}] {segmentText}");
            SegmentCount = ReceivedSegments.Count;
        });
    }

    public void SetCompleted()
    {
        Dispatcher.UIThread.Post(() =>
        {
            IsCompleted = true;
            StatusText = Se.Language.General.TranscriptionComplete;
        });
    }

    [RelayCommand]
    private void Cancel()
    {
        _cts.Cancel();
        Window?.Close();
    }

    [RelayCommand]
    private void Close()
    {
        Window?.Close();
    }
}
