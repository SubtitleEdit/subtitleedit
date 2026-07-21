using Nikse.SubtitleEdit.UiLogic.Export;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Skia;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Core.BluRaySup;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Features.Shared;
using Nikse.SubtitleEdit.Features.Shared.PromptFileSaved;
using Nikse.SubtitleEdit.Features.Video.BurnIn;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Media;
using SkiaSharp;
using SkiaSharp.HarfBuzz;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

namespace Nikse.SubtitleEdit.Features.Files.ExportImageBased;

public partial class ExportImageBasedViewModel : ObservableObject
{
    [ObservableProperty] private string _title;
    [ObservableProperty] private ObservableCollection<SubtitleLineViewModel> _subtitles;
    [ObservableProperty] SubtitleLineViewModel? _selectedSubtitle;
    [ObservableProperty] private ObservableCollection<string> _fontFamilies;
    [ObservableProperty] string? _selectedFontFamily;
    [ObservableProperty] private ObservableCollection<int> _fontSizes;
    [ObservableProperty] int _selectedFontSize;
    [ObservableProperty] private ObservableCollection<ResolutionItem> _resolutions;
    [ObservableProperty] ResolutionItem? _selectedResolution;
    [ObservableProperty] private ObservableCollection<int> _topBottomMargins;
    [ObservableProperty] int _selectedTopBottomMargin;
    [ObservableProperty] private ObservableCollection<int> _leftRightMargins;
    [ObservableProperty] int _selectedLeftRightMargin;
    [ObservableProperty] private ObservableCollection<double> _outlineWidths;
    [ObservableProperty] double _selectedOutlineWidth;
    [ObservableProperty] private ObservableCollection<double> _shadowWidths;
    [ObservableProperty] double _selectedShadowWidth;
    [ObservableProperty] private bool _isBold;
    [ObservableProperty] private bool _isRightToLeft;
    [ObservableProperty] private Color _fontColor;
    [ObservableProperty] private Color _shadowColor;
    [ObservableProperty] private Bitmap _bitmapPreview;
    [ObservableProperty] private Color _outlineColor;
    [ObservableProperty] private double _lineGapPercentage;
    [ObservableProperty] private Color _boxColor;
    [ObservableProperty] private ObservableCollection<double> _boxCornerRadiusList;
    [ObservableProperty] private double _selectedBoxCornerRadius;
    [ObservableProperty] private string _progressText;
    [ObservableProperty] private double _progressValue;
    [ObservableProperty] private bool _isGenerating;
    [ObservableProperty] private bool _isExportButtonVisible;
    [ObservableProperty] private ObservableCollection<ExportAlignmentDisplay> _alignments;
    [ObservableProperty] ExportAlignmentDisplay _selectedAlignment;
    [ObservableProperty] private ObservableCollection<ExportContentAlignmentDisplay> _contentAlignments;
    [ObservableProperty] ExportContentAlignmentDisplay _selectedContentAlignment;
    [ObservableProperty] private ObservableCollection<int> _lineSpacings;
    [ObservableProperty] int _selectedLineSpacing;
    [ObservableProperty] private ObservableCollection<SeExportImagesProfile> _profiles;
    [ObservableProperty] private SeExportImagesProfile? _selectedProfile;
    [ObservableProperty] private string _imageInfo;
    [ObservableProperty] private ObservableCollection<int> _paddingsLeftRight;
    [ObservableProperty] private int _selectedPaddingLeftRight;
    [ObservableProperty] private ObservableCollection<int> _paddingsTopBottom;
    [ObservableProperty] private int _selectedPaddingTopBottom;
    [ObservableProperty] private ObservableCollection<FontBoxItem> _boxTypes = null!;
    [ObservableProperty] private FontBoxItem _selectedBoxType = null!;
    [ObservableProperty] private int _boxPaddingLeft;
    [ObservableProperty] private int _boxPaddingRight;
    [ObservableProperty] private int _boxPaddingTop;
    [ObservableProperty] private int _boxPaddingBottom;
    public ObservableCollection<int> BoxPaddingValues { get; } = new ObservableCollection<int>(Enumerable.Range(0, 100));

    private string _outlineColorText = string.Empty;
    public string OutlineColorText
    {
        get => _outlineColorText;
        set => SetProperty(ref _outlineColorText, value);
    }

    private string _shadowColorText = string.Empty;
    public string ShadowColorText
    {
        get => _shadowColorText;
        set => SetProperty(ref _shadowColorText, value);
    }

    private string _boxColorText = string.Empty;
    public string BoxColorText
    {
        get => _boxColorText;
        set => SetProperty(ref _boxColorText, value);
    }

    public Window? Window { get; set; }
    public bool OkPressed { get; private set; }
    public DataGrid SubtitleGrid { get; set; }

    private List<SubtitleLineViewModel>? _selectedSubtitles;
    private bool _dirty;
    private readonly Lock _generateLock;
    private bool _isCtrlDown;
    private IExportHandler? _exportImageHandler;
    private string? _subtitleFileName;
    private readonly CancellationTokenSource _cancellationTokenSource;
    private readonly System.Timers.Timer _timerUpdatePreview;
    private readonly IFileHelper _fileHelper;
    private readonly IFolderHelper _folderHelper;
    private readonly IWindowService _windowService;

    public ExportImageBasedViewModel(IFileHelper fileHelper, IFolderHelper folderHelper, IWindowService windowService)
    {
        _fileHelper = fileHelper;
        _folderHelper = folderHelper;
        _windowService = windowService;

        Subtitles = new ObservableCollection<SubtitleLineViewModel>();
        // Individual face names ("Segoe UI Semibold", "Arial Black", ...) like the ASSA style
        // editor and SE4's GDI list - Avalonia's font list only has typographic family names,
        // which hid every named face from this dialog (issue #12537). ImageRenderer resolves
        // these via FontFaces, so a face renders with its own weight.
        FontFamilies = new ObservableCollection<string>(FontHelper.GetLibAssaFonts());
        SelectedFontFamily = FontFamilies.FirstOrDefault();
        FontSizes = new ObservableCollection<int>(Enumerable.Range(15, 486));
        SelectedFontSize = 26;
        Resolutions = new ObservableCollection<ResolutionItem>(ResolutionItem.GetResolutions());
        SelectedResolution = Resolutions.FirstOrDefault(r => r.Width == 1920);
        TopBottomMargins = new ObservableCollection<int>(Enumerable.Range(0, 501));
        SelectedTopBottomMargin = 10;
        LeftRightMargins = new ObservableCollection<int>(Enumerable.Range(0, 501));
        SelectedLeftRightMargin = 10;
        OutlineWidths = new ObservableCollection<double>(Enumerable.Range(0, 16).Select(i => (double)i));
        SelectedOutlineWidth = OutlineWidths.First();
        ShadowWidths = new ObservableCollection<double>(Enumerable.Range(0, 16).Select(i => (double)i));
        BoxCornerRadiusList = new ObservableCollection<double>(Enumerable.Range(0, 101).Select(i => (double)i));
        SelectedBoxCornerRadius = 0;
        BoxPaddingLeft = 5;
        BoxPaddingRight = 5;
        BoxPaddingTop = 3;
        BoxPaddingBottom = 3;
        SelectedShadowWidth = 3;
        FontColor = Colors.White;
        ShadowColor = Colors.Black;
        BoxColor = Color.FromArgb(180, 0, 0, 0);
        Alignments = new ObservableCollection<ExportAlignmentDisplay>(ExportAlignmentDisplay.GetAlignments());
        SelectedAlignment = Alignments[0];
        ContentAlignments =
            new ObservableCollection<ExportContentAlignmentDisplay>(ExportContentAlignmentDisplay.GetAlignments());
        SelectedContentAlignment = ContentAlignments[0];
        LineSpacings = new ObservableCollection<int>(Enumerable.Range(-50, 501));
        SelectedLineSpacing = 0;
        SubtitleGrid = new DataGrid();
        Title = string.Empty;
        BitmapPreview = new SKBitmap(1, 1, false).ToAvaloniaBitmap();
        OutlineColor = Colors.Black;
        ProgressText = string.Empty;
        ProgressValue = 0;
        Profiles = new ObservableCollection<SeExportImagesProfile>();
        ImageInfo = string.Empty;
        PaddingsLeftRight = new ObservableCollection<int>(Enumerable.Range(0, 501));
        SelectedPaddingLeftRight = 10;
        PaddingsTopBottom = new ObservableCollection<int>(Enumerable.Range(0, 501));
        SelectedPaddingTopBottom = 10;
        BoxTypes = new ObservableCollection<FontBoxItem>
        {
            new FontBoxItem(FontBoxType.None, Se.Language.General.None),
            new FontBoxItem(FontBoxType.OneBox, Se.Language.Video.BurnIn.OneBox),
            new FontBoxItem(FontBoxType.BoxPerLine, Se.Language.Video.BurnIn.BoxPerLine),
        };
        SelectedBoxType = BoxTypes[0];
        UpdateBoxTypeLabels();

        _generateLock = new Lock();
        _cancellationTokenSource = new CancellationTokenSource();

        _timerUpdatePreview = new System.Timers.Timer();
        _timerUpdatePreview.Interval = 250;
        _timerUpdatePreview.Elapsed += TimerUpdatePreviewElapsed;
        LoadSettings();
    }

    private void LoadSettings()
    {
        var settings = Se.Settings.File.ExportImages;
        if (settings.Profiles.Count == 0)
        {
            settings.Profiles.Add(new SeExportImagesProfile());
        }

        var profile = settings.Profiles.FirstOrDefault() ?? new SeExportImagesProfile();
        if (!string.IsNullOrEmpty(Se.Settings.File.ExportImages.LastProfileName))
        {
            var lastProfile = settings.Profiles.FirstOrDefault(p => p.ProfileName == settings.LastProfileName);
            if (lastProfile != null)
            {
                profile = lastProfile;
            }
        }

        SelectedProfile = profile;

        Profiles.Clear();
        Profiles.AddRange(settings.Profiles);

        LoadProfile(profile);
    }

    private void SaveSettings()
    {
        SaveProfile(SelectedProfile);
        Se.SaveSettings();
    }

    private void TimerUpdatePreviewElapsed(object? sender, ElapsedEventArgs e)
    {
        if (_dirty)
        {
            _dirty = false;
            Dispatcher.UIThread.Post(SubtitleLineChanged);
        }
    }

    [RelayCommand]
    private void Cancel()
    {
        if (IsGenerating)
        {
            _cancellationTokenSource.Cancel();
            IsGenerating = false;
            return;
        }

        Close();
    }

    private void SaveProfiles()
    {
        Se.Settings.File.ExportImages.Profiles.Clear();
        foreach (var profile in Profiles)
        {
            Se.Settings.File.ExportImages.Profiles.Add(profile);
        }

        Se.Settings.File.ExportImages.LastProfileName = SelectedProfile?.ProfileName ?? "Default";
    }

    private void LoadProfiles()
    {
        Profiles.Clear();
        var profiles = Se.Settings.File.ExportImages.Profiles;
        if (profiles.Count == 0)
        {
            profiles.Add(new SeExportImagesProfile());
        }

        foreach (var profile in profiles)
        {
            Profiles.Add(profile);
        }
    }

    [RelayCommand]
    private async Task ShowProfile()
    {
        SaveProfiles();

        var result =
            await _windowService.ShowDialogAsync<ImageBasedProfileWindow, ImageBasedProfileViewModel>(Window!,
                vm => { vm.Initialize(Profiles, SelectedProfile); });

        if (result.OkPressed)
        {
            LoadProfiles();
            var profile = Profiles.FirstOrDefault(p => p.ProfileName == result.SelectedProfile?.Name);
            SelectedProfile = profile ?? Profiles.FirstOrDefault();
        }
    }

    [RelayCommand]
    private void ToggleLinesItalic()
    {
        var selectedItems = _selectedSubtitles?.ToList() ?? new List<SubtitleLineViewModel>();
        if (selectedItems.Any())
        {
            foreach (var item in selectedItems)
            {
                item.Text = item.Text.Contains("<i>")
                    ? item.Text.Replace("<i>", "").Replace("</i>", "")
                    : $"<i>{item.Text}</i>";
            }
        }

        _dirty = true;
    }

    [RelayCommand]
    private void ToggleLinesBold()
    {
        var selectedItems = _selectedSubtitles?.ToList() ?? new List<SubtitleLineViewModel>();
        if (selectedItems.Any())
        {
            foreach (var item in selectedItems)
            {
                item.Text = item.Text.Contains("<b>")
                    ? item.Text.Replace("<b>", "").Replace("</b>", "")
                    : $"<b>{item.Text}</b>";
            }
        }

        _dirty = true;
    }

    [RelayCommand]
    private async Task DeleteSelectedLines()
    {
        var selectedItems = _selectedSubtitles?.ToList() ?? new List<SubtitleLineViewModel>();
        if (Window == null || selectedItems.Count == 0)
        {
            return;
        }

        if (Se.Settings.General.PromptBeforeDelete)
        {
            var answer = await MessageBox.Show(
                Window,
                "Delete lines?",
                $"Do you want to delete {selectedItems.Count} lines?",
                MessageBoxButtons.YesNoCancel,
                MessageBoxIcon.Question);

            if (answer != MessageBoxResult.Yes)
            {
                return;
            }
        }

        foreach (var item in selectedItems)
        {
            Subtitles.Remove(item);
        }

        Renumber();

        _dirty = true;
    }

    private void Renumber()
    {
        for (var index = 0; index < Subtitles.Count; index++)
        {
            Subtitles[index].Number = index + 1;
        }
    }

    [RelayCommand]
    private async Task Export()
    {
        if (_exportImageHandler == null)
        {
            return;
        }

        string fileOrFolderName;
        if (_exportImageHandler.UseFileName)
        {
            var suggestedFileName = string.Empty;
            if (!string.IsNullOrWhiteSpace(_subtitleFileName))
            {
                // Keep the folder: the picker uses it as the start location and shows only the
                // file name, so the export defaults next to the subtitle being exported.
                suggestedFileName = Utilities.GetPathAndFileNameWithoutExtension(_subtitleFileName);
            }
                
            fileOrFolderName = await _fileHelper.PickSaveSubtitleFile(Window!, _exportImageHandler.Extension,
                suggestedFileName, _exportImageHandler.Title);
        }
        else
        {
            fileOrFolderName = await _folderHelper.PickFolderAsync(Window!, _exportImageHandler.Title);
        }

        if (string.IsNullOrEmpty(fileOrFolderName))
        {
            return;
        }

        IsGenerating = true;
        var imageParameters = new List<ImageParameter>();
        for (var i = 0; i < Subtitles.Count; i++)
        {
            var imageParameter = GetImageParameter(i);
            imageParameters.Add(imageParameter);
        }

        var total = Subtitles.Count;
        var completed = 0;
        await Task.Run(() =>
        {
            Parallel.For(0, total, i =>
            {
                if (_cancellationTokenSource.IsCancellationRequested)
                {
                    return;
                }

                var ip = imageParameters[i];
                ip.Bitmap = GenerateBitmap(ip);
                _exportImageHandler.CreateParagraph(ip);

                lock (_generateLock)
                {
                    completed++;
                    var percent = completed * 100.0 / total;

                    Dispatcher.UIThread.Post(() =>
                    {
                        ProgressValue = percent;
                        ProgressText = string.Format(Se.Language.General.GeneratingImageXofY, completed, total);
                    });
                }
            });

            if (_cancellationTokenSource.IsCancellationRequested)
            {
                return;
            }

            ProgressValue = 100;
            ProgressText = Se.Language.General.SavingDotDotDot;

            _exportImageHandler.WriteHeader(fileOrFolderName, GetImageParameter(0));
            for (var i = 0; i < Subtitles.Count; i++)
            {
                var ip = imageParameters[i];
                _exportImageHandler.WriteParagraph(ip);
            }

            _exportImageHandler.WriteFooter();
            IsGenerating = false;

            if (!_cancellationTokenSource.IsCancellationRequested && Window != null)
            {
                Dispatcher.UIThread.Post(async void () =>
                {
                    try
                    {
                        _ = await _windowService.ShowDialogAsync<PromptFileSavedWindow, PromptFileSavedViewModel>(Window,
                            vm =>
                            {
                                vm.Initialize(Se.Language.File.Export.ImageBasedSubtitleSaved,
                                    string.Format(Se.Language.General.SubtitleFileSavedToX, fileOrFolderName), fileOrFolderName, true,
                                    _exportImageHandler.UseFileName);
                            });
                    }
                    catch (Exception e)
                    {
                        Se.LogError(e);
                    }
                });
            }
        });
    }

    public void SubtitleGrid_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        SubtitleGridSelectionChanged();
        SubtitleLineChanged();
    }

    private void SubtitleGridSelectionChanged()
    {
        var selectedItems = SubtitleGrid.SelectedItems;

        if (selectedItems == null)
        {
            SelectedSubtitle = null;
            _selectedSubtitles = null;
            return;
        }

        _selectedSubtitles = selectedItems.Cast<SubtitleLineViewModel>().ToList();
        if (selectedItems.Count > 1)
        {
            return;
        }

        var item = _selectedSubtitles.FirstOrDefault();
        if (item == null)
        {
            SelectedSubtitle = null;
            return;
        }

        SelectedSubtitle = item;
    }

    private ImageParameter GetImageParameter(int i)
    {
        var subtitle = Subtitles[i];
        var imageParameter = new ImageParameter
        {
            Alignment = GetContentAlignment(subtitle.Text, SelectedAlignment.Alignment),
            ContentAlignment = SelectedContentAlignment.ContentAlignment,
            PaddingLeftRight = SelectedPaddingLeftRight,
            PaddingTopBottom = SelectedPaddingTopBottom,
            Index = i,
            Text = HtmlUtil.RemoveAssAlignmentTags(subtitle.Text),
            StartTime = subtitle.StartTime,
            EndTime = subtitle.EndTime,
            FontColor = FontColor.ToSKColor(),
            FontName = SelectedFontFamily ?? FontFamilies.First(),
            FontSize = SelectedFontSize,
            IsBold = IsBold,
            LineSpacingPercent = SelectedLineSpacing,
            OutlineColor = OutlineColor.ToSKColor(),
            OutlineWidth = SelectedOutlineWidth,
            ShadowColor = ShadowColor.ToSKColor(),
            ShadowWidth = SelectedShadowWidth,
            BackgroundColor = BoxColor.ToSKColor(),
            BackgroundCornerRadius = SelectedBoxCornerRadius,
            BoxType = SelectedBoxType.BoxType switch
            {
                FontBoxType.OneBox => ExportBoxType.OneBox,
                FontBoxType.BoxPerLine => ExportBoxType.BoxPerLine,
                _ => ExportBoxType.None,
            },
            BoxPaddingLeft = BoxPaddingLeft,
            BoxPaddingRight = BoxPaddingRight,
            BoxPaddingTop = BoxPaddingTop,
            BoxPaddingBottom = BoxPaddingBottom,
            ScreenWidth = SelectedResolution?.Width ?? 1920,
            ScreenHeight = SelectedResolution?.Height ?? 1080,
            BottomTopMargin = SelectedTopBottomMargin,
            LeftRightMargin = SelectedLeftRightMargin,
            IsRightToLeft = IsRightToLeft,
        };

        return imageParameter;
    }

    private ExportAlignment GetContentAlignment(string text, ExportAlignment alignment)
    {
        var s = text.Trim();
        if (s.StartsWith("{\\an1"))
        {
            return ExportAlignment.BottomLeft;
        }

        if (s.StartsWith("{\\an2"))
        {
            return ExportAlignment.BottomCenter;
        }

        if (s.StartsWith("{\\an3"))
        {
            return ExportAlignment.BottomRight;
        }

        if (s.StartsWith("{\\an4"))
        {
            return ExportAlignment.MiddleLeft;
        }

        if (s.StartsWith("{\\an5"))
        {
            return ExportAlignment.MiddleCenter;
        }

        if (s.StartsWith("{\\an6"))
        {
            return ExportAlignment.MiddleRight;
        }

        if (s.StartsWith("{\\an7"))
        {
            return ExportAlignment.TopLeft;
        }

        if (s.StartsWith("{\\an8"))
        {
            return ExportAlignment.TopCenter;
        }

        if (s.StartsWith("{\\an9"))
        {
            return ExportAlignment.TopRight;
        }

        return alignment;
    }

    [RelayCommand]
    private async Task SavePreview()
    {
        if (SelectedSubtitle == null || Window == null)
        {
            return;
        }

        var imageIndex = Subtitles.IndexOf(SelectedSubtitle!);
        var fileName =
            await _fileHelper.PickSaveSubtitleFile(Window!, ".png", $"image{imageIndex}",
                Se.Language.General.SaveImageAs);
        if (string.IsNullOrEmpty(fileName))
        {
            return;
        }

        BitmapPreview.Save(fileName, PngBitmapEncoderOptions.Default);

        Dispatcher.UIThread.Post(async void () =>
        {
            try
            {
                _ = await _windowService.ShowDialogAsync<PromptFileSavedWindow, PromptFileSavedViewModel>(Window,
                    vm =>
                    {
                        vm.Initialize(Se.Language.General.ImageSaved,
                            string.Format(Se.Language.General.FileSavedToX, fileName), fileName, true, true);
                    });
            }
            catch (Exception e)
            {
                Se.LogError(e);
            }
        });

    }

    [RelayCommand]
    private async Task ShowPreview()
    {
        if (SelectedSubtitle == null)
        {
            return;
        }

        var result = await _windowService.ShowDialogAsync<ImageBasedPreviewWindow, ImageBasedPreviewViewModel>(Window!,
            vm =>
            {
                var ip = GetImageParameter(Subtitles.IndexOf(SelectedSubtitle));
                var bitmap = GenerateBitmap(ip);
                var position = CalculatePosition(ip, bitmap.Width, bitmap.Height);
                vm.Initialize(bitmap, ip.ScreenWidth, ip.ScreenHeight, position.X, position.Y);
            });
    }

    private void Close()
    {
        Dispatcher.UIThread.Post(() => { Window?.Close(); });
    }

    public void Initialize(
        IExportHandler exportHandler,
        ObservableCollection<SubtitleLineViewModel> subtitles,
        string? subtitleFileName,
        string? videoFileName,
        bool hideExportButton = false)
    {
        Subtitles.Clear();
        Subtitles.AddRange(subtitles);
        IsExportButtonVisible = !hideExportButton;
        _exportImageHandler = exportHandler;
        _subtitleFileName = subtitleFileName;
        Title = exportHandler.Title;

        SelectedSubtitle = Subtitles.FirstOrDefault();

        if (!string.IsNullOrEmpty(videoFileName))
        {
            _ = Task.Run(() =>
            {
                var mediaInfo = FfmpegMediaInfo2.Parse(videoFileName);
                if (mediaInfo?.Dimension is { Width: > 0, Height: > 0 } dimension)
                {
                    Dispatcher.UIThread.Post(() =>
                    {
                        // Make the source video's resolution available in the combo, but do NOT
                        // override a resolution the active profile already set - otherwise a loaded
                        // video silently replaces the profile's resolution and, on close, overwrites
                        // it (#12244). Only fall back to the video size when the profile left no
                        // valid resolution selected.
                        var item = EnsureResolutionItem(dimension.Width, dimension.Height);
                        SelectedResolution ??= item;
                        _dirty = true;
                    });
                }
            });
        }
    }

    private void SubtitleLineChanged()
    {
        var selected = SelectedSubtitle;
        if (selected == null)
        {
            return;
        }

        var text = selected.Text;
        if (string.IsNullOrEmpty(text))
        {
            BitmapPreview = new SKBitmap(1, 1, false).ToAvaloniaBitmap();
            return;
        }

        var idx = Subtitles.IndexOf(selected);
        var ip = GetImageParameter(idx);
        BitmapPreview = GenerateBitmap(ip).ToAvaloniaBitmap();
        var position = CalculatePosition(ip, BitmapPreview.Size.Width, BitmapPreview.Size.Height);
        ImageInfo = $"{BitmapPreview.Size.Width}x{BitmapPreview.Size.Height} @ {position.X},{position.Y}";
    }

    private static SKPointI CalculatePosition(ImageParameter ip, double width, double height)
    {
        var x = 0;
        var y = 0;

        if (ip.Alignment == ExportAlignment.TopLeft ||
            ip.Alignment == ExportAlignment.MiddleLeft ||
            ip.Alignment == ExportAlignment.BottomLeft)
        {
            x = ip.LeftRightMargin;
        }
        else if (ip.Alignment == ExportAlignment.TopCenter ||
                 ip.Alignment == ExportAlignment.MiddleCenter ||
                 ip.Alignment == ExportAlignment.BottomCenter)
        {
            x = (int)((ip.ScreenWidth - width) / 2);
        }
        else if (ip.Alignment == ExportAlignment.TopRight ||
                 ip.Alignment == ExportAlignment.MiddleRight ||
                 ip.Alignment == ExportAlignment.BottomRight)
        {
            x = (int)(ip.ScreenWidth - width - ip.LeftRightMargin);
        }

        if (ip.Alignment == ExportAlignment.TopLeft ||
            ip.Alignment == ExportAlignment.TopCenter ||
            ip.Alignment == ExportAlignment.TopRight)
        {
            y = ip.BottomTopMargin;
        }
        else if (ip.Alignment == ExportAlignment.MiddleLeft ||
                 ip.Alignment == ExportAlignment.MiddleCenter ||
                 ip.Alignment == ExportAlignment.MiddleRight)
        {
            y = (int)((ip.ScreenHeight - height) / 2);
        }
        else if (ip.Alignment == ExportAlignment.BottomLeft ||
                 ip.Alignment == ExportAlignment.BottomCenter ||
                 ip.Alignment == ExportAlignment.BottomRight)
        {
            y = (int)(ip.ScreenHeight - height - ip.BottomTopMargin);
        }

        return new SKPointI(x, y);
    }

    public static SKBitmap GenerateBitmap(ImageParameter ip)
    {
        return ImageRenderer.GenerateBitmap(ip);
    }


    internal void ComboChanged(object? sender, SelectionChangedEventArgs e)
    {
        _dirty = true;
    }

    partial void OnSelectedBoxTypeChanged(FontBoxItem value)
    {
        UpdateBoxTypeLabels();
        _dirty = true;
    }

    private void UpdateBoxTypeLabels()
    {
        if (SelectedBoxType?.BoxType == FontBoxType.BoxPerLine)
        {
            OutlineColorText = Se.Language.General.BoxColor;
            ShadowColorText = Se.Language.General.ShadowColor;
            BoxColorText = Se.Language.General.BoxColor;
        }
        else if (SelectedBoxType?.BoxType == FontBoxType.OneBox)
        {
            OutlineColorText = Se.Language.General.OutlineColor;
            ShadowColorText = Se.Language.General.ShadowColor;
            BoxColorText = Se.Language.General.BoxColor;
        }
        else
        {
            OutlineColorText = Se.Language.General.OutlineColor;
            ShadowColorText = Se.Language.General.ShadowColor;
            BoxColorText = Se.Language.General.BoxColor;
        }
    }

    internal void CheckBoxChanged(object? sender, RoutedEventArgs e)
    {
        _dirty = true;
    }

    public void OnLoaded()
    {
        _dirty = true;
        _timerUpdatePreview.Start();
    }

    partial void OnFontColorChanged(Color value) => _dirty = true;
    partial void OnOutlineColorChanged(Color value) => _dirty = true;
    partial void OnShadowColorChanged(Color value) => _dirty = true;
    partial void OnBoxColorChanged(Color value) => _dirty = true;
    partial void OnBoxPaddingLeftChanged(int value) => _dirty = true;
    partial void OnBoxPaddingRightChanged(int value) => _dirty = true;
    partial void OnBoxPaddingTopChanged(int value) => _dirty = true;
    partial void OnBoxPaddingBottomChanged(int value) => _dirty = true;

    public void ComboResolutionChanged(object? sender, SelectionChangedEventArgs e)
    {
        _dirty = true;
        var res = SelectedResolution;
        if (res == null)
        {
            return;
        }

        _dirty = true;
        if (res is { Width: > 0, Height: > 0 })
        {
            return;
        }

        Dispatcher.UIThread.Post(async void () =>
        {
            try
            {
                var videoFileName =
                    await _fileHelper.PickOpenVideoFile(Window!, Se.Language.General.OpenVideoFileTitle);
                if (string.IsNullOrWhiteSpace(videoFileName))
                {
                    if (e.RemovedItems.Count > 0 && e.RemovedItems[0] is ResolutionItem item)
                    {
                        SelectedResolution = item;
                    }
                    else
                    {
                        SelectedResolution = Resolutions.FirstOrDefault(p => p.Width == 1920);
                    }

                    return;
                }

                _ = Task.Run(() =>
                {
                    var mediaInfo = FfmpegMediaInfo2.Parse(videoFileName);
                    if (mediaInfo?.Dimension is { Width: > 0, Height: > 0 })
                    {
                        var resolutionItem = new ResolutionItem(string.Empty, mediaInfo.Dimension.Width,
                            mediaInfo.Dimension.Height);

                        Dispatcher.UIThread.Post(() =>
                        {
                            Resolutions.Insert(1, resolutionItem);
                            SelectedResolution = resolutionItem;
                            _dirty = true;
                        });
                    }
                });
            }
            catch (Exception exception)
            {
                Se.LogError(exception);
            }
        });
    }

    internal void OnClosing()
    {
        SaveSettings();
    }

    internal void ProfileChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (e.RemovedItems.Count == 1)
        {
            SaveProfile(e.RemovedItems[0]);
        }

        if (e.AddedItems.Count == 1)
        {
            LoadProfile(e.AddedItems[0]);
        }
    }

    /// <summary>
    /// Returns the resolution item matching <paramref name="width"/> x <paramref name="height"/>,
    /// inserting a custom item (just below the "pick from video" entry) when that exact size is not
    /// already in the list. Matching on BOTH dimensions - not width alone - and materialising
    /// non-preset sizes is what lets a profile's custom resolution (e.g. 1920x960) round-trip on
    /// reopen instead of snapping to a same-width preset or the 1920x1080 default (#12244). Returns
    /// null for non-positive sizes.
    /// </summary>
    private ResolutionItem? EnsureResolutionItem(int width, int height)
    {
        if (width <= 0 || height <= 0)
        {
            return null;
        }

        var existing = Resolutions.FirstOrDefault(r => r.Width == width && r.Height == height);
        if (existing != null)
        {
            return existing;
        }

        var item = new ResolutionItem(string.Empty, width, height);
        // Index 0 is the "Pick resolution from video..." entry; keep custom sizes right below it.
        Resolutions.Insert(1, item);
        return item;
    }

    private void LoadProfile(object? v)
    {
        if (v is SeExportImagesProfile profile)
        {
            SelectedFontSize = (int)profile.FontSize;
            SelectedResolution = EnsureResolutionItem(profile.ScreenWidth, profile.ScreenHeight)
                                 ?? Resolutions.FirstOrDefault(r => r.Width == 1920);
            SelectedTopBottomMargin = profile.BottomTopMargin;
            SelectedLeftRightMargin = profile.LeftRightMargin;
            SelectedOutlineWidth = profile.OutlineWidth;
            SelectedShadowWidth = profile.ShadowWidth;
            SelectedAlignment = Alignments.FirstOrDefault(p => p.Alignment == profile.Alignment) ?? Alignments[0];
            SelectedContentAlignment =
                ContentAlignments.FirstOrDefault(p => p.ContentAlignment == profile.ContentAlignment) ??
                ContentAlignments[0];
            IsBold = profile.IsBold;
            SelectedFontFamily = profile.FontName;

            if (string.IsNullOrEmpty(SelectedFontFamily))
            {
                var preferredFonts = new[]
                {
                    "Sarabun",
                    "DejaVu Sans",
                    "Tahoma",
                    "Segoe UI",
                    "Helvetica Neue",
                    "Arial Unicode MS",
                };

                SelectedFontFamily = preferredFonts.FirstOrDefault(font => FontFamilies.Contains(font));
            }

            FontColor = profile.FontColor.FromHex().ToAvaloniaColor();
            OutlineColor = profile.OutlineColor.FromHex().ToAvaloniaColor();
            ShadowColor = profile.ShadowColor.FromHex().ToAvaloniaColor();
            BoxColor = profile.BackgroundColor.FromHex().ToAvaloniaColor();
            SelectedBoxCornerRadius = profile.BackgroundCornerRadius;
            SelectedBoxType = BoxTypes.FirstOrDefault(b => b.BoxType == profile.BoxType) ?? BoxTypes[0];
            BoxPaddingLeft = profile.BoxPaddingLeft;
            BoxPaddingRight = profile.BoxPaddingRight;
            BoxPaddingTop = profile.BoxPaddingTop;
            BoxPaddingBottom = profile.BoxPaddingBottom;
            SelectedPaddingLeftRight = profile.PaddingLeftRight;
            SelectedPaddingTopBottom = profile.PaddingTopBottom;
            SelectedLineSpacing = profile.LineSpacingPercent;
        }
    }

    private void SaveProfile(object? v)
    {
        if (v is SeExportImagesProfile profile)
        {
            profile.FontSize = SelectedFontSize;
            profile.ScreenWidth = SelectedResolution?.Width ?? 1920;
            profile.ScreenHeight = SelectedResolution?.Height ?? 1080;
            profile.BottomTopMargin = SelectedTopBottomMargin;
            profile.LeftRightMargin = SelectedLeftRightMargin;
            profile.OutlineWidth = SelectedOutlineWidth;
            profile.ShadowWidth = SelectedShadowWidth;
            profile.Alignment = SelectedAlignment.Alignment;
            profile.ContentAlignment = SelectedContentAlignment.ContentAlignment;
            profile.IsBold = IsBold;
            profile.FontName = SelectedFontFamily ?? string.Empty;
            profile.FontColor = FontColor.FromColorToHex(true);
            profile.OutlineColor = OutlineColor.FromColorToHex(true);
            profile.ShadowColor = ShadowColor.FromColorToHex(true);
            profile.BackgroundColor = BoxColor.FromColorToHex(true);
            profile.BackgroundCornerRadius = SelectedBoxCornerRadius;
            profile.BoxType = SelectedBoxType?.BoxType ?? FontBoxType.None;
            profile.BoxPaddingLeft = BoxPaddingLeft;
            profile.BoxPaddingRight = BoxPaddingRight;
            profile.BoxPaddingTop = BoxPaddingTop;
            profile.BoxPaddingBottom = BoxPaddingBottom;
            profile.PaddingLeftRight = SelectedPaddingLeftRight;
            profile.PaddingTopBottom = SelectedPaddingTopBottom;
            profile.LineSpacingPercent = SelectedLineSpacing;
        }
    }

    internal void OnKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            e.Handled = true;
            Close();
        }
        else if (e.Key == Key.LeftCtrl || e.Key == Key.RightCtrl)
        {
            _isCtrlDown = true;
        }
        else if (e.Key == Key.I && _isCtrlDown)
        {
            ToggleLinesItalic();
        }
        else if (e.Key == Key.B && _isCtrlDown)
        {
            ToggleLinesBold();
        }
        else if (e.Key == Key.P && _isCtrlDown)
        {
            Dispatcher.UIThread.Post(async void () =>
            {
                try
                {
                    await ShowPreview();
                }
                catch (Exception exception)
                {
                    Se.LogError(exception);
                }
            });
        }
    }

    internal void OnKeyUp(KeyEventArgs e)
    {
        if (e.Key == Key.LeftCtrl || e.Key == Key.RightCtrl)
        {
            _isCtrlDown = false;
            e.Handled = true;
        }
    }
}
