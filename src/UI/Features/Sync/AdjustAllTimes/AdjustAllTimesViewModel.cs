using Avalonia.Controls;
using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Shared;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Sync.AdjustAllTimes;

public partial class AdjustAllTimesViewModel : ObservableObject
{
    [ObservableProperty] private TimeSpan _adjustment;
    [ObservableProperty] private bool _adjustAll;
    [ObservableProperty] private bool _adjustSelectedLines;
    [ObservableProperty] private bool _adjustSelectedLinesAndForward;
    [ObservableProperty] private string _statusText;

    private CancellationToken _cancellationToken;
    private CancellationTokenSource _cancellationTokenSource;
    private IAdjustCallback? _adjustCallback;
    private readonly List<StatusMessage> _statusMessages = new();
    private readonly object _statusLock = new();
    private bool isNegativeAdjustment = false;

    public Window? Window { get; set; }

    public bool OkPressed { get; private set; }

    public AdjustAllTimesViewModel()
    {
        LoadSettings();
        _cancellationTokenSource = new CancellationTokenSource();
        _cancellationToken = _cancellationTokenSource.Token;
        StatusText = string.Empty;  
    }

    public void Initialize(IAdjustCallback adjustCallback, int selectedLinesCount)
    {
        _adjustCallback = adjustCallback;
        if (selectedLinesCount > 1)
        {
            AdjustSelectedLines = true;
        }
        else if (selectedLinesCount == 1 && AdjustSelectedLines)
        {
            AdjustSelectedLines = false;
            AdjustAll = true;
        }

        string choice = Se.Settings.Synchronization.AdjustAllTimesLineSelectionChoice;
        if (Se.Settings.Synchronization.AdjustAllTimesRememberLineSelectionChoice && !string.IsNullOrEmpty(choice))
        {
            AdjustAll = choice == "All";
            AdjustSelectedLines = choice == "Selected";
            AdjustSelectedLinesAndForward = choice == "SelectedAndForward";
        }
    }

    private void LoadSettings()
    {
        Adjustment = TimeSpan.FromSeconds(Se.Settings.Synchronization.AdjustAllTimes.Seconds);
        if (Se.Settings.Synchronization.AdjustAllTimes.IsSelectedLinesAndForwardSelected)
        {
            AdjustSelectedLinesAndForward = true;
        }
        else if (Se.Settings.Synchronization.AdjustAllTimes.IsSelectedLinesSelected)
        {
            AdjustSelectedLines = true;
        }
        else
        {
            AdjustAll = true;
        }
    }

    private void SaveSettings()
    {
        Se.Settings.Synchronization.AdjustAllTimes.Seconds = Adjustment.TotalSeconds;
        Se.Settings.Synchronization.AdjustAllTimes.IsSelectedLinesAndForwardSelected = AdjustSelectedLinesAndForward;
        Se.Settings.Synchronization.AdjustAllTimes.IsSelectedLinesSelected = AdjustSelectedLines;
        Se.Settings.Synchronization.AdjustAllTimes.IsAllSelected = AdjustAll;

        Se.SaveSettings();
    }

    [RelayCommand]
    private void ShowEarlier()
    {
        ShowStatus(string.Format(Se.Language.Sync.AdjustmentX, "-" + new TimeCode(Adjustment).ToShortDisplayString()));
        isNegativeAdjustment = true;
        Apply();
    }

    [RelayCommand]
    private void ShowEarlierTimeSpan(TimeSpan ts)
    {
        Adjustment = ts;
        ShowStatus(string.Format(Se.Language.Sync.AdjustmentX, "-" + new TimeCode(Adjustment).ToShortDisplayString()));
        isNegativeAdjustment = true;
        Apply();
    }

    [RelayCommand]
    private void ShowLater()
    {
        ShowStatus(string.Format(Se.Language.Sync.AdjustmentX, new TimeCode(Adjustment).ToShortDisplayString()));
        isNegativeAdjustment = false;
        Apply();
    }

    [RelayCommand]
    private void ShowLaterTimeSpan(TimeSpan ts)
    {
        Adjustment = ts;
        ShowStatus(string.Format(Se.Language.Sync.AdjustmentX, new TimeCode(Adjustment).ToShortDisplayString()));
        isNegativeAdjustment = false;
        Apply();
    }

    [RelayCommand]
    private async Task ShowHelp()
    {
        if (Window == null)
        {
            return;
        }

        await MessageBox.Show(
               Window,
               Se.Language.General.Information,
               Se.Language.Sync.AdjustAllShortcuts,
               MessageBoxButtons.OK,
               MessageBoxIcon.Information);
    }

    private void Apply()
    {
        SaveSettings();
        InvokeAdjustCallback();
    }

    [RelayCommand]
    private void Ok()
    {
        if (Se.Settings.Synchronization.AdjustAllTimesRememberLineSelectionChoice)
        {
            if (AdjustAll)
            {
                Se.Settings.Synchronization.AdjustAllTimesLineSelectionChoice = "All";
            }
            else if (AdjustSelectedLines)
            {
                Se.Settings.Synchronization.AdjustAllTimesLineSelectionChoice = "Selected";
            }
            else if (AdjustSelectedLinesAndForward)
            {
                Se.Settings.Synchronization.AdjustAllTimesLineSelectionChoice = "SelectedAndForward";
            }
        }

        OkPressed = true;
        Window?.Close();
    }

    private void ShowStatus(string statusText)
    {
        var statusMessage = new StatusMessage
        {
            Text = statusText,
            Timestamp = DateTime.Now
        };

        lock (_statusLock)
        {
            if (_statusMessages.Count >= 4)
            {
                _statusMessages.RemoveAt(0);
            }
            _statusMessages.Add(statusMessage);
            UpdateStatusDisplay();
        }

        _ = Task.Run(async () =>
        {
            await Task.Delay(1000, _cancellationToken);
            
            lock (_statusLock)
            {
                _statusMessages.Remove(statusMessage);
                UpdateStatusDisplay();
            }
        });
    }

    private void UpdateStatusDisplay()
    {
        if (_statusMessages.Count == 0)
        {
            StatusText = string.Empty;
            return;
        }

        if (_statusMessages.Count == 1)
        {
            StatusText = _statusMessages[0].Text;
            return;
        }

        // Stack messages with line breaks
        var stackedMessages = new List<string>();
        foreach (var msg in _statusMessages)
        {
            stackedMessages.Add(msg.Text);
        }

        StatusText = string.Join(Environment.NewLine, stackedMessages);
    }

    private class StatusMessage
    {
        public string Text { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
    }

    private void InvokeAdjustCallback()
    {
        var ts = isNegativeAdjustment ? -Adjustment : Adjustment;

        _adjustCallback?.Adjust(
            ts,
            AdjustAll,
            AdjustSelectedLines,
            AdjustSelectedLinesAndForward);
    }

    internal void OnKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            e.Handled = true;
            Window?.Close();
        }
        else if ((e.Key == Key.Right || e.Key == Key.FnRightArrow) && e.KeyModifiers == KeyModifiers.Shift)
        {
            ShowLaterTimeSpan(TimeSpan.FromMilliseconds(10));
        }
        else if ((e.Key == Key.Right || e.Key == Key.FnRightArrow) && e.KeyModifiers == KeyModifiers.Control)
        {
            ShowLaterTimeSpan(TimeSpan.FromMilliseconds(100));
        }
        else if ((e.Key == Key.Right || e.Key == Key.FnRightArrow) && e.KeyModifiers == KeyModifiers.Alt)
        {
            ShowLaterTimeSpan(TimeSpan.FromMilliseconds(500));
        }
        else if ((e.Key == Key.Left || e.Key == Key.FnLeftArrow) && e.KeyModifiers == KeyModifiers.Shift)
        {
            ShowEarlierTimeSpan(TimeSpan.FromMilliseconds(10));
        }
        else if ((e.Key == Key.Left || e.Key == Key.FnLeftArrow) && e.KeyModifiers == KeyModifiers.Control)
        {
            ShowEarlierTimeSpan(TimeSpan.FromMilliseconds(100));
        }
        else if ((e.Key == Key.Left || e.Key == Key.FnLeftArrow) && e.KeyModifiers == KeyModifiers.Alt)
        {
            ShowEarlierTimeSpan(TimeSpan.FromMilliseconds(500));
        }
        else if (UiUtil.IsHelp(e))
        {
            e.Handled = true;
            UiUtil.ShowHelp("features/adjust-all-times");
        }
    }

    internal void OnClosing(WindowClosingEventArgs e)
    {
        _cancellationTokenSource.Cancel();
    }
}