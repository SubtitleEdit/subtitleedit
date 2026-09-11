using Avalonia.Controls;
using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Logic.Config;
using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;

namespace Nikse.SubtitleEdit.Features.Options.Settings.MinGapCalculate;

/// <summary>
/// Works out a minimum gap in milliseconds from a frame rate and a number of frames, so a gap
/// specified by a delivery spec as "two frames" can be entered as the milliseconds the rest of
/// Subtitle Edit works in - without the user reaching for a calculator (issue #13906).
/// </summary>
public partial class MinGapCalculateViewModel : ObservableObject
{
    [ObservableProperty] private ObservableCollection<string> _frameRates = [];

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CalculationText))]
    [NotifyPropertyChangedFor(nameof(UseAsNewGapText))]
    private string _selectedFrameRate = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CalculationText))]
    [NotifyPropertyChangedFor(nameof(UseAsNewGapText))]
    private int? _frames;

    public Window? Window { get; set; }

    public bool OkPressed { get; private set; }

    /// <summary>Result of the calculation - the value the caller should adopt when OK was pressed.</summary>
    public int MinGapMs => (int)Math.Round(1000.0 / GetFrameRate() * (Frames ?? 0));

    public string CalculationText => string.Format(
        Se.Language.Options.Settings.MinGapCalculateXFramesAtYGivesZMs,
        Frames ?? 0,
        FormatFrameRate(GetFrameRate()),
        MinGapMs);

    public string UseAsNewGapText => string.Format(
        Se.Language.Options.Settings.MinGapCalculateUseXAsNewGap,
        MinGapMs);

    public MinGapCalculateViewModel()
    {
        FrameRates = new ObservableCollection<string>(
            new[] { 23.976, 24.0, 25.0, 29.97, 30.0, 50.0, 59.94, 60.0 }.Select(FormatFrameRate));
    }

    /// <summary>
    /// Starts from the frame rate of the video currently loaded, so the common case - "two frames
    /// at the frame rate I am working with" - needs no picking at all.
    /// </summary>
    public void Initialize(int frames)
    {
        var current = FormatFrameRate(Configuration.Settings.General.CurrentFrameRate);
        if (!FrameRates.Contains(current))
        {
            FrameRates.Insert(0, current);
        }

        SelectedFrameRate = current;
        Frames = frames;
    }

    private double GetFrameRate()
    {
        // The box is editable, so anything can be in it; fall back to the current video's frame
        // rate rather than dividing by zero or by a half-typed number.
        if (double.TryParse(SelectedFrameRate, NumberStyles.AllowDecimalPoint, CultureInfo.CurrentCulture, out var frameRate) &&
            frameRate > 0)
        {
            return frameRate;
        }

        if (double.TryParse(SelectedFrameRate, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out frameRate) &&
            frameRate > 0)
        {
            return frameRate;
        }

        var current = Configuration.Settings.General.CurrentFrameRate;
        return current > 0 ? current : 25.0;
    }

    private static string FormatFrameRate(double frameRate)
    {
        return frameRate.ToString("0.###", CultureInfo.CurrentCulture);
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

    public void OnKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            e.Handled = true;
            Cancel();
        }
        else if (e.Key == Key.Enter)
        {
            e.Handled = true; // the OK button is IsDefault and would run OK again on the same Enter
            Ok();
        }
    }
}
