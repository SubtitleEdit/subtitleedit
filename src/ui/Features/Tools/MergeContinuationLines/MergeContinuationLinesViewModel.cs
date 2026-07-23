using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Nikse.SubtitleEdit.Features.Tools.MergeContinuationLines;

public partial class MergeContinuationLinesViewModel : ObservableObject, IClosingCleanup
{
    [ObservableProperty] private ObservableCollection<MergeContinuationLinesCandidate> _candidates;
    [ObservableProperty] private MergeContinuationLinesCandidate? _selectedCandidate;

    [ObservableProperty] private int _maxMillisecondsBetweenLines;
    [ObservableProperty] private int _maxCharacters;
    [ObservableProperty] private string _candidatesInfo;

    public Window? Window { get; set; }
    public bool OkPressed { get; private set; }
    public List<SubtitleLineViewModel> AllSubtitlesFixed { get; private set; }

    private List<SubtitleLineViewModel> _allSubtitles;
    private string? _language;
    private readonly System.Timers.Timer _previewTimer;
    private volatile bool _isClosing;
    private bool _isDirty;

    public MergeContinuationLinesViewModel()
    {
        Candidates = new ObservableCollection<MergeContinuationLinesCandidate>();
        _allSubtitles = new List<SubtitleLineViewModel>();
        AllSubtitlesFixed = new List<SubtitleLineViewModel>();
        CandidatesInfo = string.Empty;

        MaxMillisecondsBetweenLines = 500;
        MaxCharacters = Se.Settings.General.SubtitleLineMaximumLength * Se.Settings.General.MaxNumberOfLines;

        _previewTimer = new System.Timers.Timer(250);
        _previewTimer.Elapsed += PreviewTimerElapsed;
    }

    private void PreviewTimerElapsed(object? sender, System.Timers.ElapsedEventArgs e)
    {
        if (_isClosing)
        {
            return;
        }

        _previewTimer.Stop();
        if (_isDirty)
        {
            _isDirty = false;
            UpdatePreview();
        }

        // Guard the restart: OnClosingCleanup may have disposed the timer while this handler ran (#12739).
        if (!_isClosing)
        {
            _previewTimer.Start();
        }
    }

    public void OnClosingCleanup()
    {
        _isClosing = true;
        _previewTimer.StopAndDispose(PreviewTimerElapsed);
    }

    public void Initialize(List<SubtitleLineViewModel> subtitles, string? language, int? maxGapMs = null, int? maxCharacters = null)
    {
        _allSubtitles = subtitles;
        _language = language;
        if (maxGapMs.HasValue)
        {
            MaxMillisecondsBetweenLines = maxGapMs.Value;
        }
        if (maxCharacters.HasValue)
        {
            MaxCharacters = maxCharacters.Value;
        }
        _previewTimer.Start();
        _isDirty = true;
    }

    public bool HasAnyCandidates()
    {
        return MergeContinuationLinesHelper.Detect(_allSubtitles, _language, MaxMillisecondsBetweenLines, MaxCharacters).Count > 0;
    }

    private void UpdatePreview()
    {
        Dispatcher.UIThread.Post(() =>
        {
            Candidates.Clear();
            var detected = MergeContinuationLinesHelper.Detect(_allSubtitles, _language, MaxMillisecondsBetweenLines, MaxCharacters);
            foreach (var c in detected)
            {
                Candidates.Add(c);
            }

            CandidatesInfo = detected.Count == 0
                ? Se.Language.Tools.MergeContinuationLines.NoCandidatesFound
                : string.Format(Se.Language.Tools.MergeContinuationLines.CandidatesFoundX, detected.Count);
        });
    }

    [RelayCommand]
    private void Ok()
    {
        AllSubtitlesFixed = MergeContinuationLinesHelper.Apply(_allSubtitles, Candidates, _language);
        OkPressed = true;
        Window?.Close();
    }

    [RelayCommand]
    private void Cancel()
    {
        Window?.Close();
    }

    [RelayCommand]
    private void SelectAll()
    {
        foreach (var c in Candidates)
        {
            c.IsSelected = true;
        }
    }

    [RelayCommand]
    private void InverseSelection()
    {
        foreach (var c in Candidates)
        {
            c.IsSelected = !c.IsSelected;
        }
    }

    internal void SetChanged()
    {
        _isDirty = true;
    }

    internal void KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            e.Handled = true;
            Window?.Close();
        }
    }

    internal void Loaded()
    {
        _isDirty = true;
    }
}
