using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media.Imaging;
using Avalonia.Skia;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Features.Shared;
using Nikse.SubtitleEdit.Features.Shared.PromptTextBox;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Media;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Assa;

public partial class AssaStylesViewModel : ObservableObject, IClosingCleanup
{
    [ObservableProperty] private string _title;
    [ObservableProperty] private ObservableCollection<StyleDisplay> _fileStyles;
    [ObservableProperty] private StyleDisplay? _selectedFileStyle;
    [ObservableProperty] private ObservableCollection<StyleDisplay> _storageStyles;
    [ObservableProperty] private StyleDisplay? _selectedStorageStyle;
    [ObservableProperty] private ObservableCollection<string> _storageCategories;
    [ObservableProperty] private string _selectedStorageCategory;
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
    [ObservableProperty] private bool _isCategoryActionVisible;

    public Window? Window { get; set; }
    public bool OkPressed { get; private set; }
    public string Header { get; set; }
    public DataGrid FileStyleGrid { get; set; }
    public DataGrid StorageStyleGrid { get; set; }
    public DataGridCollectionView StorageStylesView { get; }
    public Subtitle ResultSubtitle => _subtitle;

    private readonly IFileHelper _fileHelper;
    private readonly IWindowService _windowService;
    private IApplyAssaStyles? _applyAssaStyles;
    private Subtitle _subtitle;
    private string _subtitleFileName;
    private volatile bool _isClosing;
    private readonly System.Timers.Timer _timerUpdatePreview;
    private readonly List<string> _extraCategories = new();

    public AssaStylesViewModel(IFileHelper fileHelper, IWindowService windowService)
    {
        _fileHelper = fileHelper;
        _windowService = windowService;

        Title = string.Empty;
        FileStyles = new ObservableCollection<StyleDisplay>();
        StorageStyles = new ObservableCollection<StyleDisplay>();
        StorageCategories = new ObservableCollection<string>();
        SelectedStorageCategory = Se.Language.Assa.AllCategories;
        Fonts = new ObservableCollection<string>();
        BorderTypes = new ObservableCollection<BorderStyleItem>(BorderStyleItem.List());
        SelectedBorderType = BorderTypes[0];
        CurrentTitle = string.Empty;
        FileStyleGrid = new DataGrid();
        StorageStyleGrid = new DataGrid();

        Header = string.Empty;
        _subtitle = new Subtitle();
        _subtitleFileName = string.Empty;

        LoadSettings();

        StorageStylesView = new DataGridCollectionView(StorageStyles)
        {
            Filter = o => o is StyleDisplay s && IsStyleInSelectedCategory(s),
        };
        RebuildStorageCategories();

        _timerUpdatePreview = new System.Timers.Timer(500);
        _timerUpdatePreview.Elapsed += TimerUpdatePreviewElapsed;
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
    private void Ok()
    {
        OkPressed = true;
        SaveFileStylesToHeader();
        SaveSettings();
        Close();
    }

    [RelayCommand]
    private void Apply()
    {
        OkPressed = true;
        SaveFileStylesToHeader();
        SaveSettings();
        _applyAssaStyles?.ApplyAssaStyles(this);
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

        var format = new AdvancedSubStationAlpha();
        var fileName = await _fileHelper.PickOpenFile(Window, "Open subtitle file to import styles from", format.Name, "*" + format.Extension, "Aegisub style file", "*.sty");
        if (string.IsNullOrEmpty(fileName))
        {
            return;
        }

        var ssaStyles = LoadStylesFromImportFile(fileName);
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
            vm.Initialize(Se.Language.General.Import, ssaStyles.Select(p => new StyleDisplay(p) { IsSelected = true, Name = MakeUniqueName(p.Name, FileStyles) }).ToList(), Se.Language.General.Import, false);
        });

        var selectedStyles = result.Styles.Where(p => p.IsSelected).ToList();
        if (!result.OkPressed || selectedStyles.Count == 0)
        {
            return;
        }

        FileStyles.AddRange(selectedStyles);

        UpdateUsages();
    }

    [RelayCommand]
    private async Task BrowseFontName()
    {
        if (Window == null)
        {
            return;
        }

        var result = await _windowService.ShowDialogAsync<AssaAttachmentsWindow, AssaAttachmentsViewModel>(Window, vm =>
        {
            vm.Initialize(_subtitle, new AdvancedSubStationAlpha(), _subtitleFileName);
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
                _subtitle.Footer = result.Footer;
            }
        }
    }

    private static List<SsaStyle> LoadStylesFromImportFile(string fileName)
    {
        if (fileName.EndsWith(".sty", StringComparison.OrdinalIgnoreCase))
        {
            var content = System.IO.File.ReadAllText(fileName);
            var header = "[V4+ Styles]" + Environment.NewLine +
                         SsaStyle.DefaultAssStyleFormat + Environment.NewLine +
                         content;
            return AdvancedSubStationAlpha.GetSsaStylesFromHeader(header);
        }

        var s = Subtitle.Parse(fileName, new AdvancedSubStationAlpha());
        if (s == null || string.IsNullOrEmpty(s.Header))
        {
            return new List<SsaStyle>();
        }

        return AdvancedSubStationAlpha.GetSsaStylesFromHeader(s.Header);
    }

    private static string MakeUniqueName(string name, ObservableCollection<StyleDisplay> styles)
    {
        var newName = name;
        if (styles.Any(p => p.Name.Equals(newName, StringComparison.OrdinalIgnoreCase)))
        {
            var count = 2;
            var doRepeat = true;
            while (doRepeat)
            {
                newName = name + "_" + count;
                doRepeat = styles.Any(p => p.Name.Equals(newName, StringComparison.OrdinalIgnoreCase));
                count++;
            }
        }

        return newName;
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
        FileStyles.Add(new StyleDisplay(style));
        UpdateUsages();
    }

    [RelayCommand]
    private void FileRemove()
    {
        var selectedItems = FileStyleGrid.SelectedItems.Cast<StyleDisplay>().ToList();
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
    private void FileRemoveAll()
    {
        FileStyles.Clear();
    }

    [RelayCommand]
    private void FilesDuplicate()
    {
        var selectedItems = FileStyleGrid.SelectedItems.Cast<StyleDisplay>().ToList();
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
            FileStyles.Add(new StyleDisplay(style));
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

        var fileName = await _fileHelper.PickSaveFile(Window, ".ass", "export-styles.ass", "Choose export file name");
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
        s.Header = AdvancedSubStationAlpha.GetHeaderAndStylesFromAdvancedSubStationAlpha(
            AdvancedSubStationAlpha.DefaultHeader,
            styles);
        var text = s.ToText(new AdvancedSubStationAlpha());
        await System.IO.File.WriteAllTextAsync(fileName, text);
    }

    [RelayCommand]
    private void FileCopyToStorage()
    {
        var selectedItems = FileStyleGrid.SelectedItems.Cast<StyleDisplay>().ToList();
        if (Window == null || selectedItems.Count == 0)
        {
            return;
        }

        foreach (var item in selectedItems)
        {
            var style = item.ToSsaStyle();
            style.Name = MakeUniqueName(style.Name, StorageStyles);
            StorageStyles.Add(new StyleDisplay(style) { Category = CategoryForNewStyle() });
        }
    }

    [RelayCommand]
    private async Task FileTakeUsagesFrom()
    {
        var selectedStyle = SelectedFileStyle;
        if (Window == null || selectedStyle == null)
        {
            return;
        }

        var ssaStyles = FileStyles.Where(p => p.UsageCount > 0 && p.Name != selectedStyle.Name).Select(p => p.ToSsaStyle()).ToList();
        var result = await _windowService.ShowDialogAsync<AssaStylePickerWindow, AssaStylePickerViewModel>(Window, vm =>
        {
            var styles = ssaStyles.Select(p => new StyleDisplay(p)).ToList();
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
        var selectedItems = FileStyleGrid.SelectedItems.Cast<StyleDisplay>().ToList();
        if (Window == null || selectedItems.Count == 0)
        {
            return;
        }

        var oldNames = selectedItems.Select(p => p.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);

        // Candidate targets: the other file styles, plus storage styles not already in the file.
        var candidates = FileStyles.Where(p => !oldNames.Contains(p.Name)).ToList();
        candidates.AddRange(StorageStyles.Where(s => FileStyles.All(f => !f.Name.Equals(s.Name, StringComparison.OrdinalIgnoreCase))));
        if (candidates.Count == 0)
        {
            return;
        }

        var result = await _windowService.ShowDialogAsync<AssaStylePickerWindow, AssaStylePickerViewModel>(Window, vm =>
        {
            var styles = candidates.Select(p => new StyleDisplay(p.ToSsaStyle())).ToList();
            vm.Initialize(Se.Language.Assa.ReplaceStyleWithDotDotDot, styles, Se.Language.General.Ok, false);
        });

        var target = result.Styles.FirstOrDefault(p => p.IsSelected) ?? result.SelectedStyle;
        if (!result.OkPressed || target == null)
        {
            return;
        }

        // If the target came from storage and isn't in the file yet, add it.
        var targetInFile = FileStyles.FirstOrDefault(f => f.Name.Equals(target.Name, StringComparison.OrdinalIgnoreCase));
        if (targetInFile == null)
        {
            targetInFile = new StyleDisplay(target.ToSsaStyle());
            FileStyles.Add(targetInFile);
        }

        // Re-point every line that used one of the replaced styles to the target.
        RepointParagraphsToStyle(_subtitle, oldNames, targetInFile.Name);

        // Remove the replaced styles (but never the target itself).
        foreach (var old in selectedItems.Where(s => !s.Name.Equals(targetInFile.Name, StringComparison.OrdinalIgnoreCase)))
        {
            FileStyles.Remove(old);
        }

        SelectedFileStyle = targetInFile;
        CurrentStyle = targetInFile;
        UpdateUsages();
    }

    // Re-points every paragraph that uses one of <paramref name="oldNames"/> (its style, via Extra,
    // optionally prefixed with '*') to <paramref name="targetName"/>. Used by "Replace style with...".
    internal static void RepointParagraphsToStyle(Subtitle subtitle, ISet<string> oldNames, string targetName)
    {
        foreach (var paragraph in subtitle.Paragraphs)
        {
            if (paragraph.Extra != null && oldNames.Contains(paragraph.Extra.TrimStart('*')))
            {
                paragraph.Extra = targetName;
            }
        }
    }

    [RelayCommand]
    private async Task StorageImport()
    {
        if (Window == null)
        {
            return;
        }

        var format = new AdvancedSubStationAlpha();
        var fileName = await _fileHelper.PickOpenFile(Window, Se.Language.Assa.OpenStyleImportFile, format.Name, "*" + format.Extension, "Aegisub style file", "*.sty");
        if (string.IsNullOrEmpty(fileName))
        {
            return;
        }

        var ssaStyles = LoadStylesFromImportFile(fileName);

        var result = await _windowService.ShowDialogAsync<AssaStylePickerWindow, AssaStylePickerViewModel>(Window, vm =>
        {
            vm.Initialize(Se.Language.General.Import, ssaStyles.Select(p => new StyleDisplay(p) { IsSelected = true, Name = MakeUniqueName(p.Name, StorageStyles) }).ToList(), Se.Language.General.Import, false);
        });

        var selectedStyles = result.Styles.Where(p => p.IsSelected).ToList();
        if (!result.OkPressed || selectedStyles.Count == 0)
        {
            return;
        }

        var category = CategoryForNewStyle();
        foreach (var style in selectedStyles)
        {
            style.Category = category;
        }

        StorageStyles.AddRange(selectedStyles);

        UpdateUsages();
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
        StorageStyles.Add(new StyleDisplay(style) { Category = CategoryForNewStyle() });
    }

    [RelayCommand]
    private void StorageRemove()
    {
        var selectedItems = StorageStyleGrid.SelectedItems.Cast<StyleDisplay>().ToList();
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
                        $"Do you want to delete style \"{selectedItems[0].Name}\" from storage?",
                        MessageBoxButtons.YesNoCancel,
                        MessageBoxIcon.Question);
                }
                else
                {
                    answer = await MessageBox.Show(
                        Window!,
                        Se.Language.Assa.DeleteStylesQuestion,
                        $"Do you want to delete {selectedItems.Count} styles from storage?",
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

            StorageStyleGrid.Focus();
        });
    }

    [RelayCommand]
    private void StorageRemoveAll()
    {
        StorageStyles.Clear();
    }

    [RelayCommand]
    private void StorageDuplicate()
    {
        var selectedItems = StorageStyleGrid.SelectedItems.Cast<StyleDisplay>().ToList();
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
            StorageStyles.Add(new StyleDisplay(style) { Category = CategoryForNewStyle() });
        }
    }

    [RelayCommand]
    private async Task StorageExport()
    {
        if (Window == null)
        {
            return;
        }

        var fileName = await _fileHelper.PickSaveFile(Window, ".ass", "export-styles.ass", "Choose export file name");
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
        s.Header = AdvancedSubStationAlpha.GetHeaderAndStylesFromAdvancedSubStationAlpha(
            AdvancedSubStationAlpha.DefaultHeader,
            styles);
        var text = s.ToText(new AdvancedSubStationAlpha());
        await System.IO.File.WriteAllTextAsync(fileName, text);
    }

    [RelayCommand]
    private void StorageCopyToFiles()
    {
        var selectedItems = StorageStyleGrid.SelectedItems.Cast<StyleDisplay>().ToList();
        if (Window == null || selectedItems.Count == 0)
        {
            return;
        }

        foreach (var item in selectedItems)
        {
            var style = item.ToSsaStyle();
            style.Name = MakeUniqueName(style.Name, FileStyles);
            FileStyles.Add(new StyleDisplay(style));
        }
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

    private string DefaultCategoryLabel => Se.Language.Assa.DefaultCategory;
    private string AllCategoriesLabel => Se.Language.Assa.AllCategories;

    private string CategoryLabelToStored(string label)
        => label == DefaultCategoryLabel || label == AllCategoriesLabel ? string.Empty : label;

    private string StoredToCategoryLabel(string stored)
        => string.IsNullOrEmpty(stored) ? DefaultCategoryLabel : stored;

    private bool IsStyleInSelectedCategory(StyleDisplay style)
        => SelectedStorageCategory == AllCategoriesLabel ||
           StoredToCategoryLabel(style.Category) == SelectedStorageCategory;

    private string CategoryForNewStyle()
        => SelectedStorageCategory == AllCategoriesLabel ? string.Empty : CategoryLabelToStored(SelectedStorageCategory);

    private void RebuildStorageCategories()
    {
        var previous = SelectedStorageCategory;

        var labels = StorageStyles
            .Select(s => StoredToCategoryLabel(s.Category))
            .Concat(_extraCategories)
            .Where(l => l != DefaultCategoryLabel && l != AllCategoriesLabel)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(l => l, StringComparer.OrdinalIgnoreCase)
            .ToList();

        StorageCategories.Clear();
        StorageCategories.Add(AllCategoriesLabel);
        StorageCategories.Add(DefaultCategoryLabel);
        foreach (var label in labels)
        {
            StorageCategories.Add(label);
        }

        SelectedStorageCategory = StorageCategories.Contains(previous) ? previous : AllCategoriesLabel;
    }

    partial void OnSelectedStorageCategoryChanged(string value)
    {
        StorageStylesView?.Refresh();
        IsCategoryActionVisible = value != AllCategoriesLabel && value != DefaultCategoryLabel;
    }

    [RelayCommand]
    private async Task NewCategory()
    {
        if (Window == null)
        {
            return;
        }

        var result = await _windowService.ShowDialogAsync<PromptTextBoxWindow, PromptTextBoxViewModel>(Window, vm =>
        {
            vm.Initialize(Se.Language.Assa.NewCategory, string.Empty, 250, 20, true);
        });

        if (!result.OkPressed || string.IsNullOrWhiteSpace(result.Text))
        {
            return;
        }

        var name = result.Text.Trim();
        if (name == AllCategoriesLabel || name == DefaultCategoryLabel)
        {
            return;
        }

        if (!_extraCategories.Contains(name, StringComparer.OrdinalIgnoreCase))
        {
            _extraCategories.Add(name);
        }

        RebuildStorageCategories();
        SelectedStorageCategory = StorageCategories.FirstOrDefault(c => c.Equals(name, StringComparison.OrdinalIgnoreCase)) ?? SelectedStorageCategory;
    }

    [RelayCommand]
    private async Task RenameCategory()
    {
        if (Window == null || SelectedStorageCategory == AllCategoriesLabel || SelectedStorageCategory == DefaultCategoryLabel)
        {
            return;
        }

        var oldName = SelectedStorageCategory;
        var result = await _windowService.ShowDialogAsync<PromptTextBoxWindow, PromptTextBoxViewModel>(Window, vm =>
        {
            vm.Initialize(Se.Language.Assa.RenameCategory, oldName, 250, 20, true);
        });

        if (!result.OkPressed || string.IsNullOrWhiteSpace(result.Text))
        {
            return;
        }

        var newName = result.Text.Trim();
        if (newName == oldName || newName == AllCategoriesLabel || newName == DefaultCategoryLabel)
        {
            return;
        }

        var newStored = CategoryLabelToStored(newName);
        foreach (var style in StorageStyles.Where(s => StoredToCategoryLabel(s.Category) == oldName))
        {
            style.Category = newStored;
        }

        _extraCategories.RemoveAll(c => c.Equals(oldName, StringComparison.OrdinalIgnoreCase));
        if (!_extraCategories.Contains(newName, StringComparer.OrdinalIgnoreCase))
        {
            _extraCategories.Add(newName);
        }

        RebuildStorageCategories();
        SelectedStorageCategory = StorageCategories.FirstOrDefault(c => c.Equals(newName, StringComparison.OrdinalIgnoreCase)) ?? AllCategoriesLabel;
    }

    [RelayCommand]
    private void DeleteCategory()
    {
        if (Window == null || SelectedStorageCategory == AllCategoriesLabel || SelectedStorageCategory == DefaultCategoryLabel)
        {
            return;
        }

        var name = SelectedStorageCategory;
        Dispatcher.UIThread.Post(async void () =>
        {
            var answer = await MessageBox.Show(
                Window!,
                Se.Language.Assa.DeleteCategory,
                string.Format(Se.Language.Assa.DeleteCategoryQuestion, name),
                MessageBoxButtons.YesNoCancel,
                MessageBoxIcon.Question);

            if (answer != MessageBoxResult.Yes)
            {
                return;
            }

            foreach (var style in StorageStyles.Where(s => StoredToCategoryLabel(s.Category) == name))
            {
                style.Category = string.Empty;
            }

            _extraCategories.RemoveAll(c => c.Equals(name, StringComparison.OrdinalIgnoreCase));
            RebuildStorageCategories();
            SelectedStorageCategory = AllCategoriesLabel;
        });
    }

    [RelayCommand]
    private async Task MoveToCategory()
    {
        var selectedItems = StorageStyleGrid.SelectedItems.Cast<StyleDisplay>().ToList();
        if (Window == null || selectedItems.Count == 0)
        {
            return;
        }

        var result = await _windowService.ShowDialogAsync<PromptTextBoxWindow, PromptTextBoxViewModel>(Window, vm =>
        {
            vm.Initialize(Se.Language.Assa.MoveToCategoryDotDotDot, DefaultCategoryLabel, 250, 20, true);
        });

        if (!result.OkPressed || string.IsNullOrWhiteSpace(result.Text))
        {
            return;
        }

        var label = result.Text.Trim();
        var stored = CategoryLabelToStored(label);
        if (!string.IsNullOrEmpty(stored) && !_extraCategories.Contains(label, StringComparer.OrdinalIgnoreCase))
        {
            _extraCategories.Add(label);
        }

        foreach (var style in selectedItems)
        {
            style.Category = stored;
        }

        RebuildStorageCategories();
        SelectedStorageCategory = StorageCategories.Contains(label) ? label : SelectedStorageCategory;
        StorageStylesView.Refresh();
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
        IApplyAssaStyles? applyAssaStyles)
    {
        Title = string.Format(Se.Language.Assa.StylesTitleX, fileName);
        Header = subtitle.Header;
        _subtitle = new Subtitle(subtitle, false);
        _subtitleFileName = fileName;
        _applyAssaStyles = applyAssaStyles;
        IsApplyVisible = applyAssaStyles != null;


        if (Header != null && Header.Contains("http://www.w3.org/ns/ttml"))
        {
            var s = new Subtitle { Header = Header };
            AdvancedSubStationAlpha.LoadStylesFromTimedText10(s, string.Empty, Header,
                AdvancedSubStationAlpha.HeaderNoStyles, new StringBuilder());
            Header = s.Header;
        }
        else if (Header != null && Header.StartsWith("WEBVTT", StringComparison.Ordinal))
        {
            _subtitle = WebVttToAssa.Convert(subtitle, new SsaStyle(), 0, 0);
            Header = _subtitle.Header;
        }

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
                var display = new StyleDisplay(style);
                SelectedBorderType = display.BorderStyle;
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
        IsTakeUsagesFromVisible = FileStyleGrid.SelectedItems.Count == 1;

        _timerUpdatePreview.Start();
    }

    private void LoadFonts()
    {
        var fonts = FontHelper.GetLibAssaFonts();

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
            style.UsageCount = _subtitle.Paragraphs.Count(p => p.Extra != null && p.Extra.TrimStart('*').Equals(style.Name.TrimStart('*')));
        }
    }

    private void SaveFileStylesToHeader()
    {
        var styles = FileStyles.Select(p => p.ToSsaStyle()).ToList();
        Header = AdvancedSubStationAlpha.GetHeaderAndStylesFromAdvancedSubStationAlpha(Header, styles);
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

    private void SaveSettings()
    {
        Se.Settings.Assa.StoredStyles.Clear();
        foreach (var style in StorageStyles)
        {
            var s = new SeAssaStyle(style);
            Se.Settings.Assa.StoredStyles.Add(s);
        }

        Se.SaveSettings();
    }

    private void LoadSettings()
    {
        StorageStyles.Clear();
        foreach (var style in Se.Settings.Assa.StoredStyles)
        {
            var display = new StyleDisplay(style);
            StorageStyles.Add(display);
        }
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
            bitmap = TextToImageGenerator.GenerateImageWithPadding(
                text,
                style.FontName,
                fontSize,
                style.Bold,
                style.ColorPrimary.ToSKColor(),
                style.ColorShadow.ToSKColor(),
                style.ColorOutline.ToSKColor(),
                style.ColorOutline.ToSKColor(),
                0,
                (float)style.ShadowWidth,
                isItalic: style.Italic,
                isUnderline: style.Underline,
                isStrikeout: style.Strikeout);

            if (style.ShadowWidth > 0)
            {
                bitmap = TextToImageGenerator.AddShadowToBitmap(bitmap,
                    (int)Math.Round(style.ShadowWidth, MidpointRounding.AwayFromZero), style.ColorShadow.ToSKColor());
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

        var frame = TextToImageGenerator.ComposeOnPreviewFrame(bitmap, GetAlignment(style), style.MarginLeft, style.MarginRight, style.MarginVertical);
        ImagePreview = frame.ToAvaloniaBitmap();
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
        IsTakeUsagesFromVisible = FileStyleGrid.SelectedItems.Count == 1;
    }

    private void SwitchToStorageStyle()
    {
        var selectedStyle = SelectedStorageStyle;
        CurrentStyle = selectedStyle;
        CurrentTitle = Se.Language.Assa.StylesSaved;
        SelectedBorderType = selectedStyle?.BorderStyle ?? BorderTypes[0];
        IsStorageStyleSelected = selectedStyle != null;
        IsSetStyleAsDefaultVisible = StorageStyleGrid.SelectedItems.Count == 1;
        IsCopyToFileStylesVisible = StorageStyleGrid.SelectedItems.Count > 0;
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

    internal void FileStylesKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Delete)
        {
            var selectedStyle = SelectedFileStyle;
            DeleteFileStyle(selectedStyle);
            e.Handled = true;
        }
    }

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
                    $"Do you want to delete style \"{selectedStyle.Name}\" from current file?",
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

            FileStyleGrid.Focus();
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
                    $"Do you want to delete {selectedStyles.Count} styles?",
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
            FileStyleGrid.Focus();
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
    }

    internal void StoreContextMenuOpening(object? sender, EventArgs e)
    {
        IsDeleteAllVisible = StorageStyles.Count > 0;
        IsDeleteVisible = SelectedStorageStyle != null;
    }
}
