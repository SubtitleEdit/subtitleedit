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
    [ObservableProperty] private int _layoutWidth;
    [ObservableProperty] private int _layoutHeight;
    [ObservableProperty] private bool _showGetResolutionFromCurrentVideo;
    [ObservableProperty] private ObservableCollection<WrapStyleItem> _wrapStyles;
    [ObservableProperty] private WrapStyleItem _selectedWrapStyle;
    [ObservableProperty] private ObservableCollection<BorderAndShadowScalingItem> _borderAndShadowScalingStyles;
    [ObservableProperty] private BorderAndShadowScalingItem _selectedBorderAndShadowScalingStyle;
    [ObservableProperty] private ObservableCollection<YCbCrMatrixItem> _yCbCrMatrixItems;
    [ObservableProperty] private YCbCrMatrixItem _selectedYCbCrMatrix;

    public Window? Window { get; internal set; }
    public bool OkPressed { get; private set; }
    public string Header { get; set; }

    private readonly IFileHelper _fileHelper;
    private readonly IWindowService _windowService;
    private string _videoFileName;
    private int _currentVideoWidth;
    private int _currentVideoHeight;
    private int _currentDisplayWidth;
    private int _currentDisplayHeight;

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
        YCbCrMatrixItems = new ObservableCollection<YCbCrMatrixItem>(YCbCrMatrixItem.List());
        SelectedYCbCrMatrix = YCbCrMatrixItems.First(p => p.Style == YCbCrMatrixType.None);

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
        UpdateTag("Title", ScriptTitle, string.IsNullOrWhiteSpace(ScriptTitle));
        UpdateTag("Original Script", OriginalScript, string.IsNullOrWhiteSpace(OriginalScript));
        UpdateTag("Original Translation", Translation, string.IsNullOrWhiteSpace(Translation));
        UpdateTag("Original Editing", Editing, string.IsNullOrWhiteSpace(Editing));
        UpdateTag("Original Timing", Timing, string.IsNullOrWhiteSpace(Timing));
        UpdateTag("Synch Point", SyncPoint, string.IsNullOrWhiteSpace(SyncPoint));
        UpdateTag("Script Updated By", UpdatedBy, string.IsNullOrWhiteSpace(UpdatedBy));
        UpdateTag("Update Details", UpdateDetails, string.IsNullOrWhiteSpace(UpdateDetails));
        UpdateTag("PlayResX", VideoWidth.ToString(CultureInfo.InvariantCulture), VideoWidth == 0);
        UpdateTag("PlayResY", VideoHeight.ToString(CultureInfo.InvariantCulture), VideoHeight == 0);
        var hasLayoutRes = LayoutWidth > 0 && LayoutHeight > 0;
        UpdateTag("LayoutResX", LayoutWidth.ToString(CultureInfo.InvariantCulture), !hasLayoutRes);
        UpdateTag("LayoutResY", LayoutHeight.ToString(CultureInfo.InvariantCulture), !hasLayoutRes);
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

        UpdateTag("YCbCr Matrix", SelectedYCbCrMatrix.Name, SelectedYCbCrMatrix.Style == YCbCrMatrixType.NotSet);
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
                    sb.AppendLine(tag + ": " + text);
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
        // The main window already parsed the open video's dimensions when it was loaded, so
        // use those directly - reliable and instant. Only fall back to re-parsing the file
        // with ffmpeg if the dimensions were not supplied.
        if (_currentVideoWidth > 0 && _currentVideoHeight > 0)
        {
            VideoWidth = _currentVideoWidth;
            VideoHeight = _currentVideoHeight;
            SetLayoutResolutionIfUnset(_currentDisplayWidth > 0 ? _currentDisplayWidth : _currentVideoWidth,
                _currentDisplayHeight > 0 ? _currentDisplayHeight : _currentVideoHeight);
            return;
        }

        _ = Task.Run(() =>
        {
            var mediaInfo = FfmpegMediaInfo2.Parse(_videoFileName);
            if (mediaInfo?.Dimension is { Width: > 0, Height: > 0 })
            {
                Dispatcher.UIThread.Post(() =>
                {
                    VideoWidth = mediaInfo.Dimension.Width;
                    VideoHeight = mediaInfo.Dimension.Height;
                    SetLayoutResolutionIfUnset(mediaInfo.DisplayDimension.Width, mediaInfo.DisplayDimension.Height);
                });
            }
        });
    }

    /// <summary>
    /// Taking the script resolution from the video means the script is authored for it: a script
    /// without LayoutRes gets the video's display size too, as the libass guide asks for new
    /// scripts. An existing LayoutRes is kept - it names the video the script was laid out for.
    /// </summary>
    private void SetLayoutResolutionIfUnset(int displayWidth, int displayHeight)
    {
        if (LayoutWidth > 0 && LayoutHeight > 0 || displayWidth <= 0 || displayHeight <= 0)
        {
            return;
        }

        LayoutWidth = displayWidth;
        LayoutHeight = displayHeight;
    }

    [RelayCommand]
    private void GetLayoutResolutionFromCurrentVideo()
    {
        if (_currentDisplayWidth > 0 && _currentDisplayHeight > 0)
        {
            LayoutWidth = _currentDisplayWidth;
            LayoutHeight = _currentDisplayHeight;
            return;
        }

        if (_currentVideoWidth > 0 && _currentVideoHeight > 0)
        {
            LayoutWidth = _currentVideoWidth;
            LayoutHeight = _currentVideoHeight;
            return;
        }

        _ = Task.Run(() =>
        {
            var mediaInfo = FfmpegMediaInfo2.Parse(_videoFileName);
            if (mediaInfo?.Dimension is { Width: > 0, Height: > 0 })
            {
                var display = mediaInfo.DisplayDimension;
                Dispatcher.UIThread.Post(() =>
                {
                    LayoutWidth = display.Width;
                    LayoutHeight = display.Height;
                });
            }
        });
    }

    [RelayCommand]
    private void ClearLayoutResolution()
    {
        LayoutWidth = 0;
        LayoutHeight = 0;
    }

    public void Initialize(Subtitle subtitle, SubtitleFormat format, string fileName, string videoFileName, int videoWidth = 0, int videoHeight = 0,
        int displayWidth = 0, int displayHeight = 0)
    {
        Title = UiUtil.FormatTitleWithFileName(Se.Language.Assa.PropertiesTitleX, fileName);
        Header = subtitle.Header;
        _videoFileName = videoFileName;
        _currentVideoWidth = videoWidth;
        _currentVideoHeight = videoHeight;
        _currentDisplayWidth = displayWidth;
        _currentDisplayHeight = displayHeight;
        ShowGetResolutionFromCurrentVideo = (videoWidth > 0 && videoHeight > 0) ||
                                            (!string.IsNullOrWhiteSpace(videoFileName) && System.IO.File.Exists(videoFileName));

        if (Header == null || !Header.Contains("style:", StringComparison.OrdinalIgnoreCase))
        {
            ResetHeader();
        }

        LoadFromHeader();
    }

    private void LoadFromHeader()
    {
        foreach (var custom in YCbCrMatrixItems.Where(p => p.Style == YCbCrMatrixType.Custom).ToList())
        {
            YCbCrMatrixItems.Remove(custom);
        }

        // "None" (no color mangling) is the libass-recommended value, so it is the default for
        // files without the header; N/A stays available to leave the header out.
        SelectedYCbCrMatrix = YCbCrMatrixItems.First(p => p.Style == YCbCrMatrixType.None);

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
            else if (s.StartsWith("layoutresx:", StringComparison.Ordinal))
            {
                if (int.TryParse(line.Trim().Remove(0, 11).Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var number) && number > 0)
                {
                    LayoutWidth = number;
                }
            }
            else if (s.StartsWith("layoutresy:", StringComparison.Ordinal))
            {
                if (int.TryParse(line.Trim().Remove(0, 11).Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var number) && number > 0)
                {
                    LayoutHeight = number;
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
            else if (s.StartsWith("ycbcr matrix:", StringComparison.Ordinal))
            {
                var value = line.Trim().Remove(0, 13).Trim();

                // libass parses the value case-insensitively, so match the same way.
                var match = YCbCrMatrixItems.FirstOrDefault(p => p.Style != YCbCrMatrixType.NotSet &&
                                                                 p.Name.Equals(value, StringComparison.OrdinalIgnoreCase));
                if (value.Length == 0)
                {
                    SelectedYCbCrMatrix = YCbCrMatrixItems.First(p => p.Style == YCbCrMatrixType.NotSet);
                }
                else if (match != null)
                {
                    SelectedYCbCrMatrix = match;
                }
                else
                {
                    // Not a value libass knows - show and keep it as-is, so OK does not change it
                    // and N/A can still remove it.
                    var custom = new YCbCrMatrixItem(value, YCbCrMatrixType.Custom);
                    YCbCrMatrixItems.Add(custom);
                    SelectedYCbCrMatrix = custom;
                }
            }
            else if (s.StartsWith("wrapstyle:", StringComparison.Ordinal))
            {
                var wrapStyle = line.Trim().Remove(0, 10).Trim();

                // WrapStyles only holds 0-3; First() threw on a header with an empty or
                // out-of-range WrapStyle. The loop below already does the same lookup safely.
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
        else if (UiUtil.IsHelp(e))
        {
            e.Handled = true;
            UiUtil.ShowHelp("features/assa-properties");
        }
    }
}
