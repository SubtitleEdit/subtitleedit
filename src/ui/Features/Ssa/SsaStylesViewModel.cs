using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Skia;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Features.Assa;
using Nikse.SubtitleEdit.Features.Shared;
using Nikse.SubtitleEdit.Features.Shared.PickFontName;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Media;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Ssa;

public partial class SsaStylesViewModel : ObservableObject, IClosingCleanup
{
    [ObservableProperty] private string _title;
    [ObservableProperty] private ObservableCollection<StyleDisplay> _fileStyles;
    [ObservableProperty] private StyleDisplay? _selectedFileStyle;
    [ObservableProperty] private ObservableCollection<StyleDisplay> _storageStyles;
    [ObservableProperty] private StyleDisplay? _selectedStorageStyle;
    [ObservableProperty] private StyleDisplay? _currentStyle;
    [ObservableProperty] private ObservableCollection<string> _fonts;
    [ObservableProperty] private ObservableCollection<BorderStyleItem> _borderTypes;
    [ObservableProperty] private BorderStyleItem _selectedBorderType;
    [ObservableProperty] private string _currentTitle;
    [ObservableProperty] private bool _isFileStylesFocused;
    [ObservableProperty] private bool _isApplyVisible;
    [ObservableProperty] private Bitmap? _imagePreview;
    [ObservableProperty] private bool _isDeleteVisible;
    [ObservableProperty] private bool _isDeleteAllVisible;
    [ObservableProperty] private bool _isFileStyleSelected;
    [ObservableProperty] private bool _isStorageStyleSelected;
    [ObservableProperty] private bool _isTakeUsagesFromVisible;
    [ObservableProperty] private bool _isSetStyleAsDefaultVisible;
    [ObservableProperty] private bool _isCopyToFileStylesVisible;
    [ObservableProperty] private bool _isMoveVisible;

    public Window? Window { get; set; }
    public bool OkPressed { get; private set; }
    public string Header { get; set; }
    public TableView FileStyleGrid { get; set; }
    public TableView StorageStyleGrid { get; set; }
    public Subtitle ResultSubtitle => _subtitle;

    private readonly IFileHelper _fileHelper;
    private readonly IWindowService _windowService;
    private IApplySsaStyles? _applySsaStyles;
    private readonly FileStyleRenameTracker _renameTracker;
    private Subtitle _subtitle;
    private string _subtitleFileName;
    private volatile bool _isClosing;
    private readonly System.Timers.Timer _timerUpdatePreview;

    public SsaStylesViewModel(IFileHelper fileHelper, IWindowService windowService)
    {
        _fileHelper = fileHelper;
        _windowService = windowService;

        Title = string.Empty;
        FileStyles = new ObservableCollection<StyleDisplay>();
        StorageStyles = new ObservableCollection<StyleDisplay>();
        Fonts = new ObservableCollection<string>();
        BorderTypes = new ObservableCollection<BorderStyleItem>(BorderStyleItem.List());
        SelectedBorderType = BorderTypes[0];
        CurrentTitle = string.Empty;
        FileStyleGrid = new TableView();
        StorageStyleGrid = new TableView();

        Header = string.Empty;
        _subtitle = new Subtitle();
        _subtitleFileName = string.Empty;

        LoadSettings();

        _renameTracker = new FileStyleRenameTracker(FileStyles, () => _subtitle, UpdateUsages);

        _timerUpdatePreview = new System.Timers.Timer(500);
        _timerUpdatePreview.Elapsed += TimerUpdatePreviewElapsed;
    }

    /// <summary>
    /// The font combo box binds SelectedItem to CurrentStyle.FontName; a font missing from
    /// the item list would make Avalonia clear the selection and null out the style's font.
    /// Make sure the font is listed before the style becomes current (#13101).
    /// </summary>
    /// <summary>
    /// The border type combo is not bound to CurrentStyle - it writes its selection into the
    /// current style (BorderTypeChanged). Keep it in sync on every change of the current style,
    /// or it would show (and on its next selection change write) another style's border type:
    /// Initialize left it at the last file style's, and deleting a file style kept the deleted
    /// one's. Both were only corrected by the grid's selection event.
    /// </summary>
    partial void OnCurrentStyleChanged(StyleDisplay? value)
    {
        SelectedBorderType = value?.BorderStyle ?? BorderTypes[0];
    }

    partial void OnCurrentStyleChanging(StyleDisplay? value)
    {
        var fontName = value?.FontName;
        if (!string.IsNullOrEmpty(fontName) && !Fonts.Contains(fontName))
        {
            Fonts.Insert(0, fontName);
        }
    }

    private void TimerUpdatePreviewElapsed(object? sender, System.Timers.ElapsedEventArgs e)
    {
        if (_isClosing)
        {
            return;
        }

        _timerUpdatePreview.Stop();
        UpdatePreview();

        // Guard the restart: OnClosingCleanup may have disposed the timer while this
        // handler ran, and Start() on a disposed timer throws ObjectDisposedException,
        // crashing the app from a thread-pool thread. (#12739)
        if (!_isClosing)
        {
            _timerUpdatePreview.Start();
        }
    }

    [RelayCommand]
    private async Task Ok()
    {
        if (!await ValidateFileStyleNames())
        {
            return;
        }

        OkPressed = true;
        SaveFileStylesToHeader();
        SaveSettings();
        Close();
    }

    /// <summary>
    /// Hands the current styles to the main window without closing. OkPressed stays false: it
    /// is only for OK, so a later Cancel keeps what was applied instead of also applying the
    /// edits made after Apply.
    /// </summary>
    [RelayCommand]
    private async Task Apply()
    {
        if (!await ValidateFileStyleNames())
        {
            return;
        }

        SaveFileStylesToHeader();
        SaveSettings();
        _applySsaStyles?.ApplySsaStyles(this);
    }

    // An empty or duplicate name would be written to the header as is (see FileStyleNameValidator).
    // The offending style is selected so it can be fixed.
    private async Task<bool> ValidateFileStyleNames()
    {
        var invalid = FileStyleNameValidator.FindInvalidName(FileStyles);
        if (invalid == null)
        {
            return true;
        }

        SelectedFileStyle = invalid.Value.Style;
        if (Window != null)
        {
            await MessageBox.Show(
                Window,
                Se.Language.General.Error,
                invalid.Value.Message,
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }

        return false;
    }

    [RelayCommand]
    private void Cancel()
    {
        Close();
    }

    [RelayCommand]
    private async Task FileImport()
    {
        if (Window == null)
        {
            return;
        }

        var fileName = await _fileHelper.PickOpenFile(Window, Se.Language.Assa.OpenStyleImportFile, Se.Language.Assa.SsaStyleImportFiles, SsaStyleImportExtensions);
        if (string.IsNullOrEmpty(fileName))
        {
            return;
        }

        var ssaStyles = StyleFileImportHelper.LoadStylesForSsa(fileName);
        if (ssaStyles.Count == 0)
        {
            await MessageBox.Show(
                Window,
                Se.Language.General.Error,
                "Nothing to import",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
            return;
        }

        var result = await _windowService.ShowDialogAsync<AssaStylePickerWindow, AssaStylePickerViewModel>(Window, vm =>
        {
            vm.Initialize(Se.Language.General.Import, ssaStyles.Select(p => StripAlpha(new StyleDisplay(p) { IsSelected = true })).ToList(), Se.Language.General.Import, false);
        });

        var selectedStyles = result.Styles.Where(p => p.IsSelected).ToList();
        if (!result.OkPressed || selectedStyles.Count == 0)
        {
            return;
        }

        // Same overwrite / keep both prompt as the ASSA window - importing a style to replace
        // the file's style of the same name always added a "_2" copy instead
        await CopyStyles(selectedStyles, FileStyles, Se.Language.Assa.StyleXAlreadyExistsInFile);

        UpdateUsages();
    }

    // .ass and Aegisub .sty styles are accepted too - the [V4 Styles] header is written on OK
    private const string SsaStyleImportExtensions = "*.ssa;*.ass;*.sty";

    /// <summary>
    /// Opens the font picker (installed fonts + fonts collected in SE's Fonts folder)
    /// and assigns the picked font to the current style.
    /// </summary>
    [RelayCommand]
    private async Task PickFontName()
    {
        if (Window == null || CurrentStyle == null)
        {
            return;
        }

        var currentFontName = CurrentStyle.FontName;
        var result = await _windowService.ShowDialogAsync<PickFontNameWindow, PickFontNameViewModel>(Window, vm =>
        {
            vm.Initialize();
            if (!string.IsNullOrEmpty(currentFontName))
            {
                vm.SelectedFontName = currentFontName;
            }
        });

        if (result.OkPressed && !string.IsNullOrEmpty(result.SelectedFontName) && CurrentStyle != null)
        {
            if (!Fonts.Contains(result.SelectedFontName))
            {
                Fonts.Insert(0, result.SelectedFontName);
            }

            CurrentStyle.FontName = result.SelectedFontName;

            if (result.SelectedCollectedFont != null)
            {
                EmbedCollectedFont(result.SelectedCollectedFont);
            }
        }
    }

    /// <summary>
    /// A font picked from the "Collected fonts" tab need not be installed on the machine
    /// that plays the subtitle, so its file is embedded in the [Fonts] attachment section.
    /// Reaches the main subtitle only on OK/Apply, like the rest of this dialog's changes.
    /// </summary>
    private void EmbedCollectedFont(CollectedFont font)
    {
        try
        {
            var bytes = File.ReadAllBytes(font.FilePath);
            _subtitle.Footer = AssaFontEmbedder.AddFontToFooter(_subtitle.Footer, font.FilePath, bytes);
        }
        catch (Exception exception)
        {
            Se.LogError(exception, "Could not embed collected font " + font.FilePath);
        }
    }

    [RelayCommand]
    private async Task BrowseFontName()
    {
        if (Window == null)
        {
            return;
        }

        var result = await _windowService.ShowDialogAsync<SsaAttachmentsWindow, SsaAttachmentsViewModel>(Window, vm =>
        {
            vm.Initialize(_subtitle, new SubStationAlpha(), _subtitleFileName);
        });

        if (result.OkPressed)
        {
            _subtitle.Footer = result.Footer;

            if (CurrentStyle != null && result.SelectedAttachment != null && !string.IsNullOrEmpty(result.SelectedAttachment.FontName))
            {
                if (!Fonts.Contains(result.SelectedAttachment.FontName))
                {
                    Fonts.Insert(0, result.SelectedAttachment.FontName);
                }

                CurrentStyle.FontName = result.SelectedAttachment.FontName;
            }
        }
    }

    /// <summary>
    /// Copies styles with <see cref="StylesDialogHelper.CopyStyles(Avalonia.Controls.Window, List{StyleDisplay}, ObservableCollection{StyleDisplay}, Func{StyleDisplay, bool}, string, Func{SsaStyle, StyleDisplay}, Action{StyleDisplay}?)"/> -
    /// a name clash asks whether to overwrite or keep both (#15312).
    /// </summary>
    private Task CopyStyles(List<StyleDisplay> sourceStyles, ObservableCollection<StyleDisplay> target, string alreadyExistsFormat)
    {
        return StylesDialogHelper.CopyStyles(
            Window!,
            sourceStyles,
            target,
            _ => true,
            alreadyExistsFormat,
            style => StripAlpha(new StyleDisplay(style)),
            existing =>
            {
                StripAlpha(existing);
                if (ReferenceEquals(existing, CurrentStyle))
                {
                    SelectedBorderType = existing.BorderStyle;
                }
            });
    }

    [RelayCommand]
    private void FileNew()
    {
        var name = Se.Language.General.New;
        if (FileStyles.Any(p => p.Name.Equals(name, StringComparison.OrdinalIgnoreCase)))
        {
            var count = 2;
            var doRepeat = true;
            while (doRepeat)
            {
                name = Se.Language.General.New + count;
                doRepeat = FileStyles.Any(p => p.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
                count++;
            }
        }

        var style = new SsaStyle { Name = name };
        FileStyles.Add(StripAlpha(new StyleDisplay(style)));
        UpdateUsages();
    }

    [RelayCommand]
    private void FileRemove()
    {
        var selectedItems = FileStyleGrid.SelectedItems?.Cast<StyleDisplay>().ToList() ?? new List<StyleDisplay>();
        if (Window == null || selectedItems.Count == 0)
        {
            return;
        }

        if (selectedItems.Count == 1)
        {
            DeleteFileStyle(selectedItems[0]);
            return;
        }

        DeleteFileStyles(selectedItems);
    }

    [RelayCommand]
    private async Task FileRemoveAll()
    {
        if (FileStyles.Count == 0)
        {
            return;
        }

        // Asks first, as deleting selected styles does - "Clear" used to wipe the list silently
        if (!await StylesDialogHelper.ConfirmDeleteStyles(Window, string.Format(Se.Language.Assa.DeleteXStylesQuestion, FileStyles.Count)))
        {
            return;
        }

        if (CurrentStyle != null && FileStyles.Contains(CurrentStyle))
        {
            SelectedFileStyle = null;
            CurrentStyle = null;
        }

        FileStyles.Clear();
    }

    [RelayCommand]
    private void FileMoveUp() => MoveFileStyles(ListMoveDirection.Up);

    [RelayCommand]
    private void FileMoveDown() => MoveFileStyles(ListMoveDirection.Down);

    [RelayCommand]
    private void FileMoveToTop() => MoveFileStyles(ListMoveDirection.Top);

    [RelayCommand]
    private void FileMoveToBottom() => MoveFileStyles(ListMoveDirection.Bottom);

    /// <summary>
    /// Reorders the selected file styles. The list order is not presentation-only - it is
    /// the order the styles are written to the file header on OK (#13056).
    /// </summary>
    private void MoveFileStyles(ListMoveDirection direction)
    {
        TableViewExtras.MoveSelectedRows(FileStyleGrid, FileStyles, direction);
    }

    [RelayCommand]
    private void StorageMoveUp() => MoveStorageStyles(ListMoveDirection.Up);

    [RelayCommand]
    private void StorageMoveDown() => MoveStorageStyles(ListMoveDirection.Down);

    [RelayCommand]
    private void StorageMoveToTop() => MoveStorageStyles(ListMoveDirection.Top);

    [RelayCommand]
    private void StorageMoveToBottom() => MoveStorageStyles(ListMoveDirection.Bottom);

    /// <summary>
    /// Reorders the selected storage styles (#15312) - saved to settings in list order on OK.
    /// </summary>
    private void MoveStorageStyles(ListMoveDirection direction)
    {
        TableViewExtras.MoveSelectedRows(StorageStyleGrid, StorageStyles, direction);
    }

    [RelayCommand]
    private void FilesDuplicate()
    {
        var selectedItems = FileStyleGrid.SelectedItems?.Cast<StyleDisplay>().ToList() ?? new List<StyleDisplay>();
        if (Window == null || selectedItems.Count == 0)
        {
            return;
        }

        foreach (var selectedStyle in selectedItems)
        {
            var name = selectedStyle.Name + " - " + Se.Language.General.Copy;
            if (FileStyles.Any(p => p.Name.Equals(name, StringComparison.OrdinalIgnoreCase)))
            {
                var count = 2;
                var doRepeat = true;
                while (doRepeat)
                {
                    name = selectedStyle.Name + " - " + Se.Language.General.Copy + count;
                    doRepeat = FileStyles.Any(p => p.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
                    count++;
                }
            }

            var style = selectedStyle.ToSsaStyle();
            style.Name = name;
            FileStyles.Add(StripAlpha(new StyleDisplay(style)));
        }

        UpdateUsages();
    }

    [RelayCommand]
    private async Task FileExport()
    {
        if (Window == null)
        {
            return;
        }

        var fileName = await _fileHelper.PickSaveFile(Window, ".ssa", "export-styles.ssa", "Choose export file name");
        if (string.IsNullOrEmpty(fileName))
        {
            return;
        }

        var styles = new List<SsaStyle>();
        foreach (var style in FileStyles)
        {
            styles.Add(style.ToSsaStyle());
        }

        var s = new Subtitle();
        s.Header = SubStationAlpha.GetHeaderAndStylesFromAdvancedSubStationAlpha(
            AdvancedSubStationAlpha.GetHeaderAndStylesFromAdvancedSubStationAlpha(
                AdvancedSubStationAlpha.DefaultHeader,
                styles),
            string.Empty);
        var text = s.ToText(new SubStationAlpha());
        await System.IO.File.WriteAllTextAsync(fileName, text);
    }

    [RelayCommand]
    private async Task FileCopyToStorage()
    {
        var selectedItems = FileStyleGrid.SelectedItems?.Cast<StyleDisplay>().ToList() ?? new List<StyleDisplay>();
        if (Window == null || selectedItems.Count == 0)
        {
            return;
        }

        await CopyStyles(selectedItems, StorageStyles, Se.Language.Assa.StyleXAlreadyExistsInStorage);
    }

    [RelayCommand]
    private async Task FileTakeUsagesFrom()
    {
        var selectedStyle = SelectedFileStyle;
        if (Window == null || selectedStyle == null)
        {
            return;
        }

        var usedStyles = FileStyles.Where(p => p.UsageCount > 0 && p.Name != selectedStyle.Name).ToList();
        var result = await _windowService.ShowDialogAsync<AssaStylePickerWindow, AssaStylePickerViewModel>(Window, vm =>
        {
            // ToSsaStyle() does not carry the usage count, which is the column this picker shows
            var styles = usedStyles.Select(p => StripAlpha(new StyleDisplay(p.ToSsaStyle()) { UsageCount = p.UsageCount })).ToList();
            vm.Initialize(Se.Language.Assa.TakeUsagesFromDotDotDot, styles, Se.Language.General.Ok, true);
        });

        var selectedStyles = result.Styles.Where(p => p.IsSelected).ToList();
        if (!result.OkPressed || selectedStyles.Count == 0)
        {
            return;
        }

        foreach (var paragraph in _subtitle.Paragraphs)
        {
            var style = selectedStyles.FirstOrDefault(p => p.Name.Equals(paragraph.Extra.TrimStart('*'), StringComparison.OrdinalIgnoreCase));
            if (style != null)
            {
                paragraph.Extra = selectedStyle.Name;
            }
        }

        UpdateUsages();
    }

    [RelayCommand]
    private async Task FileReplaceWith()
    {
        var selectedItems = FileStyleGrid.SelectedItems?.Cast<StyleDisplay>().ToList() ?? new List<StyleDisplay>();
        if (Window == null || selectedItems.Count == 0)
        {
            return;
        }

        var oldNames = selectedItems.Select(p => p.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var candidates = StylesDialogHelper.GetReplaceWithCandidates(FileStyles, StorageStyles, oldNames);
        if (candidates.Count == 0)
        {
            return;
        }

        var result = await _windowService.ShowDialogAsync<AssaStylePickerWindow, AssaStylePickerViewModel>(Window, vm =>
        {
            var styles = candidates.Select(p => StripAlpha(new StyleDisplay(p.ToSsaStyle()))).ToList();
            vm.Initialize(Se.Language.Assa.ReplaceStyleWithDotDotDot, styles, Se.Language.General.Ok, false);
        });

        var target = result.Styles.FirstOrDefault(p => p.IsSelected) ?? result.SelectedStyle;
        if (!result.OkPressed || target == null)
        {
            return;
        }

        var targetInFile = StylesDialogHelper.ReplaceStylesWith(_subtitle, FileStyles, selectedItems, target, style => StripAlpha(new StyleDisplay(style)));
        SelectedFileStyle = targetInFile;
        CurrentStyle = targetInFile;
        UpdateUsages();
    }

    [RelayCommand]
    private async Task StorageImport()
    {
        if (Window == null)
        {
            return;
        }

        var fileName = await _fileHelper.PickOpenFile(Window, Se.Language.Assa.OpenStyleImportFile, Se.Language.Assa.SsaStyleImportFiles, SsaStyleImportExtensions);
        if (string.IsNullOrEmpty(fileName))
        {
            return;
        }

        var ssaStyles = StyleFileImportHelper.LoadStylesForSsa(fileName);
        if (ssaStyles.Count == 0)
        {
            await MessageBox.Show(
                Window,
                Se.Language.General.Error,
                "Nothing to import",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
            return;
        }

        var result = await _windowService.ShowDialogAsync<AssaStylePickerWindow, AssaStylePickerViewModel>(Window, vm =>
        {
            vm.Initialize(Se.Language.General.Import, ssaStyles.Select(p => StripAlpha(new StyleDisplay(p) { IsSelected = true })).ToList(), Se.Language.General.Import, false);
        });

        var selectedStyles = result.Styles.Where(p => p.IsSelected).ToList();
        if (!result.OkPressed || selectedStyles.Count == 0)
        {
            return;
        }

        await CopyStyles(selectedStyles, StorageStyles, Se.Language.Assa.StyleXAlreadyExistsInStorage);
    }

    [RelayCommand]
    private void StorageNew()
    {
        var name = Se.Language.General.New;
        if (StorageStyles.Any(p => p.Name.Equals(name, StringComparison.OrdinalIgnoreCase)))
        {
            var count = 2;
            var doRepeat = true;
            while (doRepeat)
            {
                name = Se.Language.General.New + count;
                doRepeat = StorageStyles.Any(p => p.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
                count++;
            }
        }

        var style = new SsaStyle { Name = name };
        StorageStyles.Add(StripAlpha(new StyleDisplay(style)));
    }

    [RelayCommand]
    private void StorageRemove()
    {
        var selectedItems = StorageStyleGrid.SelectedItems?.Cast<StyleDisplay>().ToList() ?? new List<StyleDisplay>();
        if (Window == null || selectedItems.Count == 0)
        {
            return;
        }

        Dispatcher.UIThread.Post(async void () =>
        {
            var answer = MessageBoxResult.Yes;

            if (Se.Settings.General.PromptBeforeDelete)
            {
                if (selectedItems.Count == 1)
                {
                    answer = await MessageBox.Show(
                        Window!,
                        Se.Language.Assa.DeleteStyleQuestion,
                        string.Format(Se.Language.Assa.DeleteStyleXFromStorageQuestion, selectedItems[0].Name),
                        MessageBoxButtons.YesNoCancel,
                        MessageBoxIcon.Question);
                }
                else
                {
                    answer = await MessageBox.Show(
                        Window!,
                        Se.Language.Assa.DeleteStylesQuestion,
                        string.Format(Se.Language.Assa.DeleteXStylesFromStorageQuestion, selectedItems.Count),
                        MessageBoxButtons.YesNoCancel,
                        MessageBoxIcon.Question);
                }
            }

            if (answer != MessageBoxResult.Yes)
            {
                return;
            }

            foreach (var selectedStyle in selectedItems)
            {
                var idx = StorageStyles.IndexOf(selectedStyle);
                StorageStyles.Remove(selectedStyle);
                SelectedStorageStyle = null;
                CurrentStyle = null;

                if (StorageStyles.Count > 0)
                {
                    if (idx >= StorageStyles.Count)
                    {
                        idx = StorageStyles.Count - 1;
                    }

                    SelectedStorageStyle = StorageStyles[idx];
                    CurrentStyle = SelectedStorageStyle;
                }
            }

            TableViewExtras.FocusRow(StorageStyleGrid);
        });
    }

    [RelayCommand]
    private async Task StorageRemoveAll()
    {
        if (StorageStyles.Count == 0)
        {
            return;
        }

        // Asks first, as the ASSA window does - it used to clear the storage without asking
        if (!await StylesDialogHelper.ConfirmDeleteStyles(Window, string.Format(Se.Language.Assa.DeleteXStylesFromStorageQuestion, StorageStyles.Count)))
        {
            return;
        }

        if (CurrentStyle != null && StorageStyles.Contains(CurrentStyle))
        {
            SelectedStorageStyle = null;
            CurrentStyle = null;
        }

        StorageStyles.Clear();
    }

    [RelayCommand]
    private void StorageDuplicate()
    {
        var selectedItems = StorageStyleGrid.SelectedItems?.Cast<StyleDisplay>().ToList() ?? new List<StyleDisplay>();
        if (Window == null || selectedItems.Count == 0)
        {
            return;
        }

        foreach (var selectedStyle in selectedItems)
        {
            var name = selectedStyle.Name + " - " + Se.Language.General.Copy;
            if (StorageStyles.Any(p => p.Name.Equals(name, StringComparison.OrdinalIgnoreCase)))
            {
                var count = 2;
                var doRepeat = true;
                while (doRepeat)
                {
                    name = selectedStyle.Name + " - " + Se.Language.General.Copy + count;
                    doRepeat = StorageStyles.Any(p => p.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
                    count++;
                }
            }

            var style = selectedStyle.ToSsaStyle();
            style.Name = name;
            StorageStyles.Add(StripAlpha(new StyleDisplay(style)));
        }
    }

    [RelayCommand]
    private async Task StorageExport()
    {
        if (Window == null)
        {
            return;
        }

        var fileName = await _fileHelper.PickSaveFile(Window, ".ssa", "export-styles.ssa", "Choose export file name");
        if (string.IsNullOrEmpty(fileName))
        {
            return;
        }

        var styles = new List<SsaStyle>();
        foreach (var style in StorageStyles)
        {
            styles.Add(style.ToSsaStyle());
        }

        var s = new Subtitle();
        s.Header = SubStationAlpha.GetHeaderAndStylesFromAdvancedSubStationAlpha(
            AdvancedSubStationAlpha.GetHeaderAndStylesFromAdvancedSubStationAlpha(
                AdvancedSubStationAlpha.DefaultHeader,
                styles),
            string.Empty);
        var text = s.ToText(new SubStationAlpha());
        await System.IO.File.WriteAllTextAsync(fileName, text);
    }

    [RelayCommand]
    private async Task StorageCopyToFiles()
    {
        var selectedItems = StorageStyleGrid.SelectedItems?.Cast<StyleDisplay>().ToList() ?? new List<StyleDisplay>();
        if (Window == null || selectedItems.Count == 0)
        {
            return;
        }

        await CopyStyles(selectedItems, FileStyles, Se.Language.Assa.StyleXAlreadyExistsInFile);
        UpdateUsages();
    }

    [RelayCommand]
    private void StorageSetDefault()
    {
        var selectedStyle = SelectedStorageStyle;
        if (Window == null || selectedStyle == null)
        {
            return;
        }

        foreach (var style in StorageStyles)
        {
            style.IsDefault = false;
        }

        selectedStyle.IsDefault = true;
    }

    private void Close()
    {
        Dispatcher.UIThread.Post(() => { Window?.Close(); });
    }

    public void OnClosingCleanup()
    {
        _isClosing = true;
        _timerUpdatePreview.StopAndDispose(TimerUpdatePreviewElapsed);
    }

    public void Initialize(
        Subtitle subtitle,
        SubtitleFormat format,
        string fileName,
        string selectedStyleName,
        IApplySsaStyles? applySsaStyles)
    {
        Title = UiUtil.FormatTitleWithFileName(Se.Language.Assa.StylesTitleX, fileName);
        Header = subtitle.Header;
        _subtitle = new Subtitle(subtitle, false);
        _subtitleFileName = fileName;
        _applySsaStyles = applySsaStyles;
        IsApplyVisible = applySsaStyles != null;

        if (Header == null || !Header.Contains("style:", StringComparison.OrdinalIgnoreCase))
        {
            ResetHeader();
        }

        FileStyles.Clear();
        foreach (var styleName in AdvancedSubStationAlpha.GetStylesFromHeader(Header))
        {
            var style = AdvancedSubStationAlpha.GetSsaStyle(styleName, Header);
            if (style != null)
            {
                var display = StripAlpha(new StyleDisplay(style));
                FileStyles.Add(display);

                var fontName = display.FontName;
                if (!string.IsNullOrEmpty(fontName) && !Fonts.Contains(fontName))
                {
                    Fonts.Insert(0, fontName);
                }
            }
        }

        Task.Run(() => LoadFonts());

        UpdateUsages();

        if (FileStyles.Count > 0)
        {
            SelectedFileStyle =
                FileStyles.FirstOrDefault(p => p.Name.Equals(selectedStyleName, StringComparison.OrdinalIgnoreCase));
            if (SelectedFileStyle == null)
            {
                SelectedFileStyle = FileStyles[0];
            }

            CurrentStyle = SelectedFileStyle;
            CurrentTitle = Se.Language.Assa.StylesInFile;
        }

        IsFileStyleSelected = SelectedFileStyle != null;
        IsTakeUsagesFromVisible = FileStyleGrid.SelectedItems?.Count == 1;

        _timerUpdatePreview.Start();
    }

    private void LoadFonts()
    {
        var fonts = StylesDialogHelper.GetStyleEditorFontNames();

        Dispatcher.UIThread.Post(() =>
        {
            foreach (var font in fonts)
            {
                if (!Fonts.Contains(font))
                {
                    Fonts.Add(font);
                }
            }
        });
    }

    private void UpdateUsages()
    {
        foreach (var style in FileStyles)
        {
            style.UsageCount = _subtitle.Paragraphs.Count(p => p.Extra != null && p.Extra.TrimStart('*').Equals(style.Name.TrimStart('*'), StringComparison.OrdinalIgnoreCase));
        }
    }

    private void SaveFileStylesToHeader()
    {
        var styles = FileStyles.Select(p => p.ToSsaStyle()).ToList();
        var assaHeader = AdvancedSubStationAlpha.GetHeaderAndStylesFromAdvancedSubStationAlpha(
            string.IsNullOrEmpty(Header) ? AdvancedSubStationAlpha.DefaultHeader : Header,
            styles);
        Header = SubStationAlpha.GetHeaderAndStylesFromAdvancedSubStationAlpha(assaHeader, string.Empty);
    }

    private void ResetHeader()
    {
        var format = new SubStationAlpha();
        var sub = new Subtitle();
        var text = format.ToText(sub, string.Empty);
        var lines = text.SplitToLines();
        format.LoadSubtitle(sub, lines, string.Empty);
        Header = sub.Header;
    }

    private void SaveSettings()
    {
        Se.Settings.Ssa.StoredStyles.Clear();
        foreach (var style in StorageStyles)
        {
            var s = new SeAssaStyle(style);
            Se.Settings.Ssa.StoredStyles.Add(s);
        }

        Se.SaveSettings();
    }

    private void LoadSettings()
    {
        StorageStyles.Clear();
        foreach (var style in Se.Settings.Ssa.StoredStyles)
        {
            var display = StripAlpha(new StyleDisplay(style));
            StorageStyles.Add(display);
        }
    }

    internal static StyleDisplay StripAlpha(StyleDisplay display)
    {
        display.ColorPrimary = WithoutAlpha(display.ColorPrimary);
        display.ColorSecondary = WithoutAlpha(display.ColorSecondary);
        display.ColorOutline = WithoutAlpha(display.ColorOutline);
        display.ColorShadow = WithoutAlpha(display.ColorShadow);
        return display;
    }

    internal static Color WithoutAlpha(Color c)
    {
        return Color.FromArgb(255, c.R, c.G, c.B);
    }

    private void UpdatePreview()
    {
        var style = CurrentStyle;
        if (style == null)
        {
            ImagePreview = new SKBitmap(1, 1, true).ToAvaloniaBitmap();
            return;
        }

        var text = "This is a test";

        // Scale the rendered font size to the preview canvas height (~360px) the same way
        // libass scales fonts against PlayResY. Default to 288 (libass default) when missing.
        var fontSize = (float)style.FontSize * 360f / GetPlayResY(_subtitle.Header);
        var libAssFontName = FontHelper.GetSkiaFontNameFromLibAssaFontName(style.FontName);
        SKBitmap bitmap;

        if (style.BorderStyle.Style == BorderStyleType.BoxPerLine)
        {
            // The other two branches resolve the libass face name to a Skia family and pass
            // (outlineColor, shadowColor) in that order; this one used the raw face name and had
            // the two colors the wrong way round, so the shadow came out in the outline color.
            bitmap = TextToImageGenerator.GenerateImageWithPadding(
                text,
                libAssFontName,
                fontSize,
                style.Bold,
                style.ColorPrimary.ToSKColor(),
                style.ColorOutline.ToSKColor(),
                style.ColorShadow.ToSKColor(),
                style.ColorOutline.ToSKColor(),
                0,
                (float)style.ShadowWidth,
                isItalic: style.Italic,
                isUnderline: style.Underline,
                isStrikeout: style.Strikeout);

            if (style.ShadowWidth > 0)
            {
                var withShadow = TextToImageGenerator.AddShadowToBitmap(bitmap,
                    (int)Math.Round(style.ShadowWidth, MidpointRounding.AwayFromZero), style.ColorShadow.ToSKColor());
                bitmap.Dispose();
                bitmap = withShadow;
            }
        }
        else if (style.BorderStyle.Style == BorderStyleType.OneBox)
        {
            bitmap = TextToImageGenerator.GenerateImageWithPadding(
                text,
                libAssFontName,
                fontSize,
                style.Bold,
                style.ColorPrimary.ToSKColor(),
                style.ColorOutline.ToSKColor(),
                SKColors.Red,
                style.ColorShadow.ToSKColor(),
                (float)style.OutlineWidth,
                0,
                1.0f,
                (int)Math.Round(style.ShadowWidth),
                isItalic: style.Italic,
                isUnderline: style.Underline,
                isStrikeout: style.Strikeout);
        }
        else // FontBoxType.None
        {
            bitmap = TextToImageGenerator.GenerateImageWithPadding(
                text,
                libAssFontName,
                fontSize,
                style.Bold,
                style.ColorPrimary.ToSKColor(),
                style.ColorOutline.ToSKColor(),
                style.ColorShadow.ToSKColor(),
                SKColors.Transparent,
                (float)style.OutlineWidth,
                (float)style.ShadowWidth,
                isItalic: style.Italic,
                isUnderline: style.Underline,
                isStrikeout: style.Strikeout);
        }

        // A 500 ms timer re-renders this for as long as the dialog is open, and ToAvaloniaBitmap
        // copies the pixels out, so both native bitmaps must go or the preview leaks ~1 MB a tick.
        using var frame = TextToImageGenerator.ComposeOnPreviewFrame(bitmap, GetAlignment(style), style.MarginLeft, style.MarginRight, style.MarginVertical);
        ImagePreview = frame.ToAvaloniaBitmap();
        bitmap.Dispose();
    }

    private static int GetPlayResY(string? header)
    {
        if (string.IsNullOrEmpty(header))
        {
            return 288;
        }

        var value = AdvancedSubStationAlpha.GetTagValueFromHeader("PlayResY", "[Script Info]", header);
        if (int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var y) && y > 0)
        {
            return y;
        }

        return 288;
    }

    private static int GetAlignment(StyleDisplay style)
    {
        if (style.AlignmentAn1)
        {
            return 1;
        }
        if (style.AlignmentAn2)
        {
            return 2;
        }
        if (style.AlignmentAn3)
        {
            return 3;
        }
        if (style.AlignmentAn4)
        {
            return 4;
        }
        if (style.AlignmentAn5)
        {
            return 5;
        }
        if (style.AlignmentAn6)
        {
            return 6;
        }
        if (style.AlignmentAn7)
        {
            return 7;
        }
        if (style.AlignmentAn8)
        {
            return 8;
        }
        if (style.AlignmentAn9)
        {
            return 9;
        }
        return 2;
    }

    internal void FileStylesChanged(object? sender, SelectionChangedEventArgs e)
    {
        SwitchToFileStyle();
    }

    internal void StorageStylesChanged(object? sender, SelectionChangedEventArgs e)
    {
        SwitchToStorageStyle();
    }

    // Also switch context when a grid merely gains focus (e.g. clicking the row that is already
    // selected), otherwise SelectionChanged never fires and the title/editor stays on the other grid.
    internal void FileStylesGotFocus(object? sender, FocusChangedEventArgs e)
    {
        SwitchToFileStyle();
    }

    internal void StorageStylesGotFocus(object? sender, FocusChangedEventArgs e)
    {
        SwitchToStorageStyle();
    }

    private void SwitchToFileStyle()
    {
        var selectedStyle = SelectedFileStyle;
        CurrentStyle = selectedStyle;
        CurrentTitle = Se.Language.Assa.StylesInFile;
        SelectedBorderType = selectedStyle?.BorderStyle ?? BorderTypes[0];
        IsFileStyleSelected = selectedStyle != null;
        IsTakeUsagesFromVisible = FileStyleGrid.SelectedItems?.Count == 1;
    }

    private void SwitchToStorageStyle()
    {
        var selectedStyle = SelectedStorageStyle;
        CurrentStyle = selectedStyle;
        CurrentTitle = Se.Language.Assa.StylesSaved;
        SelectedBorderType = selectedStyle?.BorderStyle ?? BorderTypes[0];
        IsStorageStyleSelected = selectedStyle != null;
        IsSetStyleAsDefaultVisible = StorageStyleGrid.SelectedItems?.Count == 1;
        IsCopyToFileStylesVisible = StorageStyleGrid.SelectedItems?.Count > 0;
    }

    internal void BorderTypeChanged(object? sender, SelectionChangedEventArgs e)
    {
        var selectedStyle = CurrentStyle;
        if (selectedStyle == null)
        {
            return;
        }

        selectedStyle.BorderStyle = SelectedBorderType;
    }

    // Delete removes all selected rows, as the Delete button does - not just the focused one
    internal void FileStylesKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Delete)
        {
            FileRemove();
            e.Handled = true;
        }
    }

    internal void StorageStylesKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Delete)
        {
            StorageRemove();
            e.Handled = true;
        }
    }

    /// <summary>
    /// Ctrl+Up/Ctrl+Down reorder the selected styles (tunneled, see <see cref="StylesDialogHelper.HandleMoveKeyDown"/>).
    /// </summary>
    internal void FileStylesMoveKeyDown(object? sender, KeyEventArgs e)
        => StylesDialogHelper.HandleMoveKeyDown(e, MoveFileStyles);

    internal void StorageStylesMoveKeyDown(object? sender, KeyEventArgs e)
        => StylesDialogHelper.HandleMoveKeyDown(e, MoveStorageStyles);

    private void DeleteFileStyle(StyleDisplay? selectedStyle)
    {
        if (selectedStyle == null)
        {
            return;
        }

        Dispatcher.UIThread.Post(async void () =>
        {
            var answer = MessageBoxResult.Yes;

            if (Se.Settings.General.PromptBeforeDelete)
            {
                answer = await MessageBox.Show(
                    Window!,
                    Se.Language.Assa.DeleteStyleQuestion,
                    string.Format(Se.Language.Assa.DeleteStyleXFromFileQuestion, selectedStyle.Name),
                    MessageBoxButtons.YesNoCancel,
                    MessageBoxIcon.Question);
            }

            if (answer != MessageBoxResult.Yes)
            {
                return;
            }

            if (selectedStyle != null)
            {
                var idx = FileStyles.IndexOf(selectedStyle);
                FileStyles.Remove(selectedStyle);
                SelectedFileStyle = null;
                CurrentStyle = null;
                if (FileStyles.Count > 0)
                {
                    if (idx >= FileStyles.Count)
                    {
                        idx = FileStyles.Count - 1;
                    }

                    SelectedFileStyle = FileStyles[idx];
                    CurrentStyle = SelectedFileStyle;
                }

                UpdateUsages();
            }

            TableViewExtras.FocusRow(FileStyleGrid);
        });
    }

    private void DeleteFileStyles(List<StyleDisplay> selectedStyles)
    {
        if (selectedStyles.Count == 0)
        {
            return;
        }

        Dispatcher.UIThread.Post(async void () =>
        {
            var answer = MessageBoxResult.Yes;

            if (Se.Settings.General.PromptBeforeDelete)
            {
                answer = await MessageBox.Show(
                    Window!,
                    Se.Language.Assa.DeleteStylesQuestion,
                    string.Format(Se.Language.Assa.DeleteXStylesQuestion, selectedStyles.Count),
                    MessageBoxButtons.YesNoCancel,
                    MessageBoxIcon.Question);
            }

            if (answer != MessageBoxResult.Yes)
            {
                return;
            }

            foreach (var selectedStyle in selectedStyles)
            {
                var idx = FileStyles.IndexOf(selectedStyle);
                FileStyles.Remove(selectedStyle);
                SelectedFileStyle = null;
                CurrentStyle = null;
                if (FileStyles.Count > 0)
                {
                    if (idx >= FileStyles.Count)
                    {
                        idx = FileStyles.Count - 1;
                    }

                    SelectedFileStyle = FileStyles[idx];
                    CurrentStyle = SelectedFileStyle;
                }

            }

            UpdateUsages();
            TableViewExtras.FocusRow(FileStyleGrid);
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
            UiUtil.ShowHelp("features/assa-styles");
        }
    }

    internal void FilesContextMenuOpening(object? sender, EventArgs e)
    {
        IsDeleteAllVisible = FileStyles.Count > 0;
        IsDeleteVisible = SelectedFileStyle != null;
        IsMoveVisible = FileStyles.Count > 1 && FileStyleGrid.SelectedItems?.Count > 0;
    }

    internal void StoreContextMenuOpening(object? sender, EventArgs e)
    {
        // The storage menu's "Delete"/"Clear" must follow the storage list, not the file list -
        // reading FileStyles here hid "Delete" whenever no file style happened to be selected,
        // and offered "Clear" on an empty storage list.
        IsDeleteAllVisible = StorageStyles.Count > 0;
        IsDeleteVisible = SelectedStorageStyle != null;
        IsMoveVisible = StorageStyles.Count > 1 && StorageStyleGrid.SelectedItems?.Count > 0;
    }
}
