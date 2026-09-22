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
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Nikse.SubtitleEdit.UiLogic.Media;

namespace Nikse.SubtitleEdit.Features.Sync.ChangeFrameRate;

public partial class ChangeFrameRateViewModel : ObservableObject
{
    [ObservableProperty] private ObservableCollection<double> _fromFrameRates;
    [ObservableProperty] private double _selectedFromFrameRate;

    [ObservableProperty] private ObservableCollection<double> _toFrameRates;
    [ObservableProperty] private double _selectedToFrameRate;

    [ObservableProperty] private bool _hasVideo;
    [ObservableProperty] private string _videoFileName = string.Empty;
    [ObservableProperty] private string _videoInfoText = string.Empty;

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

    /// <summary>
    /// Presets the dialog the way SE 4 did (#15177): the current frame rate - the toolbar value,
    /// which follows the loaded video - is the <b>from</b> rate, because a subtitle demuxed from
    /// that video was authored at it. <b>To</b> keeps the last used value, unless that equals
    /// <b>from</b>, where the PAL/NTSC-film counterpart beats a no-op conversion. The video line
    /// shows which file the detected rate came from.
    /// </summary>
    public void Initialize(string? videoFileName, double videoFrameRate, double currentFrameRate)
    {
        if (!string.IsNullOrEmpty(videoFileName) && videoFrameRate > 0)
        {
            VideoFileName = videoFileName;
            VideoInfoText = string.Format(Se.Language.Sync.VideoXFrameRateY, Path.GetFileName(videoFileName),
                videoFrameRate.ToString("0.###", CultureInfo.InvariantCulture));
            HasVideo = true;
            FromFrameRates = WithRate(FromFrameRates, videoFrameRate);
            ToFrameRates = WithRate(ToFrameRates, videoFrameRate);
        }

        var fromRate = currentFrameRate > 0 ? currentFrameRate : videoFrameRate;
        if (fromRate <= 0)
        {
            return;
        }

        FromFrameRates = WithRate(FromFrameRates, fromRate);
        SelectedFromFrameRate = GetClosestFrameRate(FromFrameRates, fromRate);
        _autoFromRate = SelectedFromFrameRate;

        if (Math.Abs(SelectedToFrameRate - SelectedFromFrameRate) < 0.001)
        {
            var counterpart = Math.Abs(SelectedFromFrameRate - 25.0) < 0.01 ? 23.976 : 25.0;
            SelectedToFrameRate = GetClosestFrameRate(ToFrameRates, counterpart);
            _autoToRate = SelectedToFrameRate;
        }
    }

    private static ObservableCollection<double> WithRate(IEnumerable<double> rates, double rate)
    {
        return new ObservableCollection<double>(rates.Append(rate).Distinct().OrderBy(r => r));
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
        SubtitleLineViewModel? previous = null;
        var previousOriginalEndMs = 0d;
        foreach (var line in subtitles)
        {
            var originalStartMs = line.StartTime.TotalMilliseconds;
            var originalEndMs = line.EndTime.TotalMilliseconds;

            // Round to whole milliseconds via start + scaled duration, not start and end
            // independently, so lines of equal length keep equal durations after scaling (#14056).
            var newStart = TimeSpanExtensions.FromMillisecondsWholeMilliseconds(originalStartMs * ratio);
            var newDuration = TimeSpanExtensions.FromMillisecondsWholeMilliseconds((originalEndMs - originalStartMs) * ratio);
            line.SetTimes(newStart, newStart + newDuration);

            // The two roundings can land the previous line's end 1 ms past this line's start,
            // turning a clean join into an overlap the source never had. Clip the previous end
            // back; overlaps that were already in the source are left as they were.
            if (previous != null && previousOriginalEndMs <= originalStartMs && previous.EndTime > line.StartTime)
            {
                previous.SetTimes(previous.StartTime, line.StartTime);
            }

            previous = line;
            previousOriginalEndMs = originalEndMs;
        }
    }
}