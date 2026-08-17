using Avalonia.Controls;
using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Files.FormatProperties.WebVttProperties;

public partial class WebVttPropertiesViewModel : ObservableObject
{
    [ObservableProperty] private string _cueAn1;
    [ObservableProperty] private string _cueAn2;
    [ObservableProperty] private string _cueAn3;
    [ObservableProperty] private string _cueAn4;
    [ObservableProperty] private string _cueAn5;
    [ObservableProperty] private string _cueAn6;
    [ObservableProperty] private string _cueAn7;
    [ObservableProperty] private string _cueAn8;
    [ObservableProperty] private string _cueAn9;
    [ObservableProperty] private bool _useXTimestampMap;
    [ObservableProperty] private bool _mergeLinesWithSameText;
    [ObservableProperty] private bool _mergeStyleTags;

    public Window? Window { get; set; }

    public bool OkPressed { get; private set; }

    public WebVttPropertiesViewModel()
    {
        var formats = Se.Settings.Formats;
        _cueAn1 = formats.WebVttCueAn1 ?? string.Empty;
        _cueAn2 = formats.WebVttCueAn2 ?? string.Empty;
        _cueAn3 = formats.WebVttCueAn3 ?? string.Empty;
        _cueAn4 = formats.WebVttCueAn4 ?? string.Empty;
        _cueAn5 = formats.WebVttCueAn5 ?? string.Empty;
        _cueAn6 = formats.WebVttCueAn6 ?? string.Empty;
        _cueAn7 = formats.WebVttCueAn7 ?? string.Empty;
        _cueAn8 = formats.WebVttCueAn8 ?? string.Empty;
        _cueAn9 = formats.WebVttCueAn9 ?? string.Empty;
        _useXTimestampMap = formats.WebVttUseXTimestampMap;
        _mergeLinesWithSameText = formats.WebVttMergeLinesWithSameText;
        _mergeStyleTags = !formats.WebVttDoNoMergeTags;
    }

    private void SaveSettings()
    {
        var formats = Se.Settings.Formats;
        formats.WebVttCueAn1 = CueAn1 ?? string.Empty;
        formats.WebVttCueAn2 = CueAn2 ?? string.Empty;
        formats.WebVttCueAn3 = CueAn3 ?? string.Empty;
        formats.WebVttCueAn4 = CueAn4 ?? string.Empty;
        formats.WebVttCueAn5 = CueAn5 ?? string.Empty;
        formats.WebVttCueAn6 = CueAn6 ?? string.Empty;
        formats.WebVttCueAn7 = CueAn7 ?? string.Empty;
        formats.WebVttCueAn8 = CueAn8 ?? string.Empty;
        formats.WebVttCueAn9 = CueAn9 ?? string.Empty;
        formats.WebVttUseXTimestampMap = UseXTimestampMap;
        formats.WebVttMergeLinesWithSameText = MergeLinesWithSameText;
        formats.WebVttDoNoMergeTags = !MergeStyleTags;

        Se.SaveSettings();
    }

    [RelayCommand]
    private void ResetCueSettings()
    {
        var defaults = new SeFormats();
        CueAn1 = defaults.WebVttCueAn1;
        CueAn2 = defaults.WebVttCueAn2;
        CueAn3 = defaults.WebVttCueAn3;
        CueAn4 = defaults.WebVttCueAn4;
        CueAn5 = defaults.WebVttCueAn5;
        CueAn6 = defaults.WebVttCueAn6;
        CueAn7 = defaults.WebVttCueAn7;
        CueAn8 = defaults.WebVttCueAn8;
        CueAn9 = defaults.WebVttCueAn9;
    }

    [RelayCommand]
    private void Ok()
    {
        SaveSettings();
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
    }
}
