using Avalonia.Controls;
using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Media;
using Nikse.SubtitleEdit.UiLogic.BatchConvert;
using Nikse.SubtitleEdit.UiLogic.Media;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Tools.BatchConvert;

/// <summary>
/// Batch convert's "Transport Stream settings" dialog - SE4's BatchConvertTsSettings form.
/// Edits <see cref="SeBatchConvert"/>'s Ts* fields; saved on OK only.
/// </summary>
public partial class BatchConvertTsSettingsViewModel : ObservableObject
{
    [ObservableProperty] private bool _overrideXPosition;
    [ObservableProperty] private ObservableCollection<string> _horizontalAlignments;
    [ObservableProperty] private string _selectedHorizontalAlignment;
    [ObservableProperty] private int _horizontalMarginPercent;

    [ObservableProperty] private bool _overrideYPosition;
    [ObservableProperty] private int _bottomMarginPercent;

    [ObservableProperty] private bool _overrideVideoSize;
    [ObservableProperty] private int _videoWidth;
    [ObservableProperty] private int _videoHeight;

    [ObservableProperty] private string _fileNameEnding;
    [ObservableProperty] private string _fileNameEndingSample;

    [ObservableProperty] private bool _onlyTeletext;

    public Window? Window { get; set; }
    public bool OkPressed { get; private set; }

    /// <summary>Set by the window so the placeholder menu can insert at the caret.</summary>
    public Func<int>? GetFileNameEndingCaretIndex { get; set; }
    public Action<int>? SetFileNameEndingCaretIndex { get; set; }

    private readonly IFileHelper _fileHelper;

    public BatchConvertTsSettingsViewModel(IFileHelper fileHelper)
    {
        _fileHelper = fileHelper;

        HorizontalAlignments = new ObservableCollection<string>
        {
            Se.Language.General.Left,
            Se.Language.General.Center,
            Se.Language.General.Right,
        };
        _selectedHorizontalAlignment = HorizontalAlignments[1];
        _fileNameEnding = string.Empty;
        _fileNameEndingSample = string.Empty;

        LoadSettings();
    }

    private void LoadSettings()
    {
        var s = Se.Settings.Tools.BatchConvert;
        OverrideXPosition = s.TsOverrideXPosition;
        HorizontalMarginPercent = Math.Clamp(s.TsOverrideHMargin, 0, 100);
        OverrideYPosition = s.TsOverrideYPosition;
        BottomMarginPercent = Math.Clamp(s.TsOverrideBottomMargin, 0, 100);
        OverrideVideoSize = s.TsOverrideScreenSize;
        VideoWidth = s.TsScreenWidth > 0 ? s.TsScreenWidth : 1920;
        VideoHeight = s.TsScreenHeight > 0 ? s.TsScreenHeight : 1080;
        FileNameEnding = s.TsFileNameAppend ?? string.Empty;
        OnlyTeletext = s.TsOnlyTeletext;

        if (string.Equals(s.TsOverrideHAlign, TransportStreamExportSettings.HAlignLeft, StringComparison.OrdinalIgnoreCase))
        {
            SelectedHorizontalAlignment = HorizontalAlignments[0];
        }
        else if (string.Equals(s.TsOverrideHAlign, TransportStreamExportSettings.HAlignRight, StringComparison.OrdinalIgnoreCase))
        {
            SelectedHorizontalAlignment = HorizontalAlignments[2];
        }
        else
        {
            SelectedHorizontalAlignment = HorizontalAlignments[1];
        }
    }

    private void SaveSettings()
    {
        var s = Se.Settings.Tools.BatchConvert;
        s.TsOverrideXPosition = OverrideXPosition;
        s.TsOverrideHMargin = HorizontalMarginPercent;
        s.TsOverrideYPosition = OverrideYPosition;
        s.TsOverrideBottomMargin = BottomMarginPercent;
        s.TsOverrideScreenSize = OverrideVideoSize;
        s.TsScreenWidth = VideoWidth;
        s.TsScreenHeight = VideoHeight;
        s.TsFileNameAppend = FileNameEnding ?? string.Empty;
        s.TsOnlyTeletext = OnlyTeletext;

        var alignmentIndex = HorizontalAlignments.IndexOf(SelectedHorizontalAlignment);
        s.TsOverrideHAlign = alignmentIndex switch
        {
            0 => TransportStreamExportSettings.HAlignLeft,
            2 => TransportStreamExportSettings.HAlignRight,
            _ => TransportStreamExportSettings.HAlignCenter,
        };

        Se.SaveSettings();
    }

    partial void OnFileNameEndingChanged(string value)
    {
        FileNameEndingSample = TransportStreamFileNameEnding.MakeSample(value);
    }

    [RelayCommand]
    private void InsertPlaceholder(string placeholder)
    {
        var text = FileNameEnding ?? string.Empty;
        var index = GetFileNameEndingCaretIndex?.Invoke() ?? text.Length;
        index = Math.Clamp(index, 0, text.Length);
        FileNameEnding = text.Insert(index, placeholder);
        SetFileNameEndingCaretIndex?.Invoke(index + placeholder.Length);
    }

    /// <summary>Pick a video and take its frame size as the override size (SE4's "..." button).</summary>
    [RelayCommand]
    private async Task GetSizeFromVideo()
    {
        if (Window == null)
        {
            return;
        }

        var fileName = await _fileHelper.PickOpenVideoFile(Window, Se.Language.General.OpenVideoFileTitle);
        if (string.IsNullOrEmpty(fileName))
        {
            return;
        }

        var mediaInfo = FfmpegMediaInfo.Parse(fileName);
        if (mediaInfo.Dimension.Width > 0 && mediaInfo.Dimension.Height > 0)
        {
            VideoWidth = mediaInfo.Dimension.Width;
            VideoHeight = mediaInfo.Dimension.Height;
            OverrideVideoSize = true;
        }
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
