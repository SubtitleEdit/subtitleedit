using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Features.Video.BurnIn;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Media;
using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Assa;

public partial class AssaPropertiesViewModel : ObservableObject
{
    [ObservableProperty] private string _title;
    [ObservableProperty] private string _scriptTitle;
    [ObservableProperty] private string _originalScript;
    [ObservableProperty] private string _translation;
    [ObservableProperty] private string _editing;
    [ObservableProperty] private string _timing;
    [ObservableProperty] private string _syncPoint;
    [ObservableProperty] private string _updatedBy;
    [ObservableProperty] private string _updateDetails;
    [ObservableProperty] private int _videoWidth;
    [ObservableProperty] private int _videoHeight;
    [ObservableProperty] private bool _showGetResolutionFromCurrentVideo;
    [ObservableProperty] private ObservableCollection<WrapStyleItem> _wrapStyles;
    [ObservableProperty] private WrapStyleItem _selectedWrapStyle;
    [ObservableProperty] private ObservableCollection<BorderAndShadowScalingItem> _borderAndShadowScalingStyles;
    [ObservableProperty] private BorderAndShadowScalingItem _selectedBorderAndShadowScalingStyle;

    public Window? Window { get; internal set; }
    public bool OkPressed { get; private set; }
    public string Header { get; set; }

    private readonly IFileHelper _fileHelper;
    private readonly IWindowService _windowService;
    private string _videoFileName;

    public AssaPropertiesViewModel(IFileHelper fileHelper, IWindowService windowService)
    {
        _fileHelper = fileHelper;
        _windowService = windowService;

        Title = string.Empty;
        ScriptTitle = string.Empty;
        OriginalScript = string.Empty;
        Translation = string.Empty;
        Editing = string.Empty;
        Timing = string.Empty;
        SyncPoint = string.Empty;
        UpdatedBy = string.Empty;
        UpdateDetails = string.Empty;
        WrapStyles = new ObservableCollection<WrapStyleItem>(WrapStyleItem.List());
        SelectedWrapStyle = WrapStyles[2];
        BorderAndShadowScalingStyles = new ObservableCollection<BorderAndShadowScalingItem>(BorderAndShadowScalingItem.List());
        SelectedBorderAndShadowScalingStyle = BorderAndShadowScalingStyles[2];

        _videoFileName = string.Empty;
        Header = string.Empty;
    }

    [RelayCommand]
    private void Ok()
    {
        OkPressed = true;
        UpdateHeader();
        Close();
    }

    private void UpdateHeader()
    {
        UpdateTag("Original Script", OriginalScript, string.IsNullOrWhiteSpace(OriginalScript));
        UpdateTag("Original Translation", Translation, string.IsNullOrWhiteSpace(Translation));
        UpdateTag("Original Editing", Editing, string.IsNullOrWhiteSpace(Editing));
        UpdateTag("Original Timing", Timing, string.IsNullOrWhiteSpace(Timing));
        UpdateTag("Synch Point", SyncPoint, string.IsNullOrWhiteSpace(SyncPoint));
        UpdateTag("Script Updated By", UpdatedBy, string.IsNullOrWhiteSpace(UpdatedBy));
        UpdateTag("Update Details", UpdateDetails, string.IsNullOrWhiteSpace(UpdateDetails));
        UpdateTag("PlayResX", VideoWidth.ToString(CultureInfo.InvariantCulture), VideoWidth == 0);
        UpdateTag("PlayResY", VideoHeight.ToString(CultureInfo.InvariantCulture), VideoHeight == 0);
        UpdateTag("WrapStyle", ((int)SelectedWrapStyle.Style).ToString(CultureInfo.InvariantCulture), false);

        if (SelectedBorderAndShadowScalingStyle.Style == BorderAndShadowScalingType.Yes)
        {
            UpdateTag("ScaledBorderAndShadow", "Yes", false);
        }
        else if (SelectedBorderAndShadowScalingStyle.Style == BorderAndShadowScalingType.No)
        {
            UpdateTag("ScaledBorderAndShadow", "No", false);
        }
        else
        {
            UpdateTag("ScaledBorderAndShadow", "Hide", true);
        }
    }

    private void UpdateTag(string tag, string text, bool remove)
    {
        var scriptInfoOn = false;
        var sb = new StringBuilder();
        var found = false;
        foreach (var line in Header.SplitToLines())
        {
            if (line.StartsWith("[script info]", StringComparison.OrdinalIgnoreCase))
            {
                scriptInfoOn = true;
            }
            else if (line.StartsWith('['))
            {
                if (!found && scriptInfoOn && !remove)
                {
                    sb = new StringBuilder(sb.ToString().Trim() + Environment.NewLine);
                    sb.AppendLine(tag + ": " + text);
                }
                sb = new StringBuilder(sb.ToString().TrimEnd());
                sb.AppendLine();
                sb.AppendLine();
                scriptInfoOn = false;
            }

            var s = line.ToLowerInvariant();
            if (s.StartsWith(tag.ToLowerInvariant() + ":", StringComparison.Ordinal))
            {
                if (!remove)
                {
                    sb.AppendLine(line.Substring(0, tag.Length) + ": " + text);
                }

                found = true;
            }
            else
            {
                sb.AppendLine(line);
            }
        }

        Header = sb.ToString().Trim();
    }

    [RelayCommand]
    private void Cancel()
    {
        Close();
    }

    [RelayCommand]
    private async Task BrowseResolution()
    {
        if (Window == null)
        {
            return;
        }

        var result = await _windowService.ShowDialogAsync<BurnInResolutionPickerWindow, BurnInResolutionPickerViewModel>(Window, vm =>
        {
            vm.RemoveUseSourceResolution();
        });

        if (!result.OkPressed || result.SelectedResolution == null)
        {
            return;
        }

        if (result.SelectedResolution.ItemType == ResolutionItemType.PickResolution)
        {
            var videoFileName = await _fileHelper.PickOpenVideoFile(Window!, Se.Language.General.OpenVideoFileTitle);
            if (string.IsNullOrWhiteSpace(videoFileName))
            {
                return;
            }

            var mediaInfo = FfmpegMediaInfo2.Parse(videoFileName);
            VideoWidth = mediaInfo.Dimension.Width;
            VideoHeight = mediaInfo.Dimension.Height;
        }
        else if (result.SelectedResolution.ItemType == ResolutionItemType.Resolution)
        {
            VideoWidth = result.SelectedResolution.Width;
            VideoHeight = result.SelectedResolution.Height;
        }
    }

    [RelayCommand]
    private void GetResolutionFromCurrentVideo()
    {
        _ = Task.Run(() =>
        {
            var mediaInfo = FfmpegMediaInfo2.Parse(_videoFileName);
            if (mediaInfo?.Dimension is { Width: > 0, Height: > 0 })
            {
                var resolutionItem = new ResolutionItem(string.Empty, mediaInfo.Dimension.Width, mediaInfo.Dimension.Height);
                Dispatcher.UIThread.Post(() =>
                {
                    VideoWidth = mediaInfo.Dimension.Width;
                    VideoHeight = mediaInfo.Dimension.Height;
                });
            }
        });
    }

    public void Initialize(Subtitle subtitle, SubtitleFormat format, string fileName, string videoFileName)
    {
        Title = string.Format(Se.Language.Assa.PropertiesTitleX, fileName);
        Header = subtitle.Header;
        _videoFileName = videoFileName;
        ShowGetResolutionFromCurrentVideo = !string.IsNullOrWhiteSpace(videoFileName) && System.IO.File.Exists(videoFileName);

        if (Header == null || !Header.Contains("style:", StringComparison.OrdinalIgnoreCase))
        {
            ResetHeader();
        }

        LoadFromHeader();
    }

    private void LoadFromHeader()
    {
        foreach (var line in Header.SplitToLines())
        {
            var s = line.ToLowerInvariant().Trim();
            if (s.StartsWith("title:", StringComparison.Ordinal))
            {
                ScriptTitle = line.Trim().Remove(0, 6).Trim();
            }
            else if (s.StartsWith("original script:", StringComparison.Ordinal))
            {
                OriginalScript = line.Trim().Remove(0, 16).Trim();
            }
            else if (s.StartsWith("original translation:", StringComparison.Ordinal))
            {
                Translation = line.Trim().Remove(0, 21).Trim();
            }
            else if (s.StartsWith("original editing:", StringComparison.Ordinal))
            {
                Editing = line.Trim().Remove(0, 17).Trim();
            }
            else if (s.StartsWith("original timing:", StringComparison.Ordinal))
            {
                Timing = line.Trim().Remove(0, 16).Trim();
            }
            else if (s.StartsWith("synch point:", StringComparison.Ordinal))
            {
                SyncPoint = line.Trim().Remove(0, 12).Trim();
            }
            else if (s.StartsWith("script updated by:", StringComparison.Ordinal))
            {
                UpdatedBy = line.Trim().Remove(0, 18).Trim();
            }
            else if (s.StartsWith("update details:", StringComparison.Ordinal))
            {
                UpdateDetails = line.Trim().Remove(0, 15).Trim();
            }
            else if (s.StartsWith("playresx:", StringComparison.Ordinal))
            {
                if (int.TryParse(line.Trim().Remove(0, 9).Trim(), out var number))
                {
                    VideoWidth = number;
                }
            }
            else if (s.StartsWith("playresy:", StringComparison.Ordinal))
            {
                if (int.TryParse(line.Trim().Remove(0, 9).Trim(), out var number))
                {
                    VideoHeight = number;
                }
            }
            else if (s.StartsWith("scaledborderandshadow:", StringComparison.Ordinal))
            {
                var scale = line.Trim().Remove(0, 22).Trim().ToLowerInvariant();
                if (scale == "yes")
                {
                    SelectedBorderAndShadowScalingStyle = BorderAndShadowScalingStyles.First(p => p.Style == BorderAndShadowScalingType.Yes);
                }
                else if (scale == "no")
                {
                    SelectedBorderAndShadowScalingStyle = BorderAndShadowScalingStyles.First(p => p.Style == BorderAndShadowScalingType.No);
                }
                else
                {
                    SelectedBorderAndShadowScalingStyle = BorderAndShadowScalingStyles.First(p => p.Style == BorderAndShadowScalingType.NotSet);
                }
            }
            else if (s.StartsWith("wrapstyle:", StringComparison.Ordinal))
            {
                var wrapStyle = line.Trim().Remove(0, 10).Trim();
                SelectedWrapStyle = WrapStyles.First(p => ((int)p.Style).ToString().Equals(wrapStyle, StringComparison.OrdinalIgnoreCase));
                foreach (var ws in WrapStyles)
                {
                    if (((int)ws.Style).ToString().Equals(wrapStyle, StringComparison.OrdinalIgnoreCase))
                    {
                        SelectedWrapStyle = ws;
                    }
                }
            }
        }
    }

    private void ResetHeader()
    {
        var format = new AdvancedSubStationAlpha();
        var sub = new Subtitle();
        var text = format.ToText(sub, string.Empty);
        var lines = text.SplitToLines();
        format.LoadSubtitle(sub, lines, string.Empty);
        Header = sub.Header;
    }

    private void Close()
    {
        Dispatcher.UIThread.Post(() =>
        {
            Window?.Close();
        });
    }

    internal void KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            Close();
        }
    }
}
