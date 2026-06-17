using Avalonia.Controls;
using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Media;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Sync.ChangeFrameRate;

public partial class ChangeFrameRateViewModel : ObservableObject
{
    [ObservableProperty] private ObservableCollection<double> _fromFrameRates;
    [ObservableProperty] private double _selectedFromFrameRate;

    [ObservableProperty] private ObservableCollection<double> _toFrameRates;
    [ObservableProperty] private double _selectedToFrameRate;

    private static readonly List<double> StandardFrameRates = new List<double> { 23.976, 24, 25, 29.97, 30, 50, 59.94, 60 };

    private double _autoFromRate = double.NaN;
    private double _autoToRate = double.NaN;

    private IFileHelper _fileHelper;

    public Window? Window { get; set; }

    public bool OkPressed { get; private set; }

    public ChangeFrameRateViewModel(IFileHelper fileHelper)
    {
        _fileHelper = fileHelper;

        var savedFrom = Se.Settings.Synchronization.ChangeFrameRateFrom;
        var savedTo = Se.Settings.Synchronization.ChangeFrameRateTo;

        FromFrameRates = new ObservableCollection<double>(StandardFrameRates.Append(savedFrom).Distinct().OrderBy(r => r));
        ToFrameRates = new ObservableCollection<double>(StandardFrameRates.Append(savedTo).Distinct().OrderBy(r => r));

        SelectedFromFrameRate = GetClosestFrameRate(FromFrameRates, savedFrom);
        SelectedToFrameRate = GetClosestFrameRate(ToFrameRates, savedTo);
    }

    public void Initialize(string? videoFileName, FfmpegMediaInfo2? mediaInfo)
    {
        if (mediaInfo == null || mediaInfo.FramesRate <= 0)
        {
            return;
        }

        var detectedRate = (double)mediaInfo.FramesRate;

        FromFrameRates = new ObservableCollection<double>(FromFrameRates.Append(detectedRate).Distinct().OrderBy(r => r));
        ToFrameRates = new ObservableCollection<double>(ToFrameRates.Append(detectedRate).Distinct().OrderBy(r => r));

        SelectedToFrameRate = GetClosestFrameRate(ToFrameRates, detectedRate);

        var preferredFromRate = Math.Abs(SelectedToFrameRate - 25.0) < 0.01 ? 23.976 : 25.0;
        SelectedFromFrameRate = GetClosestFrameRate(FromFrameRates, preferredFromRate);

        _autoToRate = SelectedToFrameRate;
        _autoFromRate = SelectedFromFrameRate;
    }

    [RelayCommand]
    private void SwitchFrameRates()
    {
        (FromFrameRates, ToFrameRates) = (ToFrameRates, FromFrameRates);
        (SelectedFromFrameRate, SelectedToFrameRate) = (SelectedToFrameRate, SelectedFromFrameRate);
    }

    [RelayCommand]
    private async Task BrowseFromFrameRate()
    {
        if (Window == null)
        {
            return;
        }

        var videoFileName = await _fileHelper.PickOpenVideoFile(Window, Se.Language.General.OpenVideoFileTitle);
        if (string.IsNullOrWhiteSpace(videoFileName))
        {
            return;
        }

        var mediaInfo = await Task.Run(() => FfmpegMediaInfo2.Parse(videoFileName));
        if (mediaInfo.FramesRate < 1)
        {
            return;
        }

        var detectedRate = (double)mediaInfo.FramesRate;
        var ratesWithDetected = StandardFrameRates.Append(detectedRate).Distinct().OrderBy(r => r).ToArray();
        FromFrameRates = new ObservableCollection<double>(ratesWithDetected);
        SelectedFromFrameRate = GetClosestFrameRate(FromFrameRates, detectedRate);
    }

    [RelayCommand]
    private async Task BrowseToFrameRate()
    {
        if (Window == null)
        {
            return;
        }

        var videoFileName = await _fileHelper.PickOpenVideoFile(Window, Se.Language.General.OpenVideoFileTitle);
        if (string.IsNullOrWhiteSpace(videoFileName))
        {
            return;
        }

        var mediaInfo = await Task.Run(() => FfmpegMediaInfo2.Parse(videoFileName));
        if (mediaInfo.FramesRate < 1)
        {
            return;
        }

        var detectedRate = (double)mediaInfo.FramesRate;
        var ratesWithDetected = StandardFrameRates.Append(detectedRate).Distinct().OrderBy(r => r).ToArray();
        ToFrameRates = new ObservableCollection<double>(ratesWithDetected);
        SelectedToFrameRate = GetClosestFrameRate(ToFrameRates, detectedRate);
    }

    [RelayCommand]
    private void Ok()
    {
        if (double.IsNaN(_autoFromRate) || Math.Abs(SelectedFromFrameRate - _autoFromRate) > 0.001)
            Se.Settings.Synchronization.ChangeFrameRateFrom = SelectedFromFrameRate;
        if (double.IsNaN(_autoToRate) || Math.Abs(SelectedToFrameRate - _autoToRate) > 0.001)
            Se.Settings.Synchronization.ChangeFrameRateTo = SelectedToFrameRate;
        Se.SaveSettings();

        OkPressed = true;
        Window?.Close();
    }

    [RelayCommand]
    private void Cancel()
    {
        Window?.Close();
    }

    internal void OnKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            e.Handled = true;
            Window?.Close();
        }
        else if (UiUtil.IsHelp(e))
        {
            e.Handled = true;
            UiUtil.ShowHelp("features/change-frame-rate");
        }
    }

    private static double GetClosestFrameRate(ObservableCollection<double> frameRates, double target)
    {
        return frameRates.MinBy(r => Math.Abs(r - target));
    }

    /// <summary>
    /// Scaling ratio applied to time codes when changing frame rate.
    /// Always <c>from / to</c> - a higher target frame rate makes time codes earlier.
    /// Shared so every change-frame-rate path (text and binary) stays consistent with
    /// libse's <see cref="Nikse.SubtitleEdit.Core.Common.Subtitle.ChangeFrameRate"/>.
    /// </summary>
    internal static double GetFrameRateRatio(double fromFrameRate, double toFrameRate)
    {
        return SubtitleFormat.GetFrameForCalculation(fromFrameRate) / SubtitleFormat.GetFrameForCalculation(toFrameRate);
    }

    internal static void ChangeFrameRate(ObservableCollection<SubtitleLineViewModel> subtitles, double fromFrameRate, double toFrameRate)
    {
        double ratio = GetFrameRateRatio(fromFrameRate, toFrameRate);
        foreach (var line in subtitles)
        {
            line.SetTimes(
                TimeSpan.FromMilliseconds(line.StartTime.TotalMilliseconds * ratio),
                TimeSpan.FromMilliseconds(line.EndTime.TotalMilliseconds * ratio));
        }
    }
}