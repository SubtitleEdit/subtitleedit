using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Layout;
using Nikse.SubtitleEdit.Features.Ocr.Engines;
using Nikse.SubtitleEdit.Features.Translate;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Tools.BatchConvert;

/// <summary>
/// Batch convert settings, in the editor look (see <see cref="EditorChrome"/>): output on the left,
/// adding files and OCR on the right, OK in the button bar.
/// </summary>
public class BatchConvertSettingsWindow : Window
{
    private const double LabelWidth = 150;
    private const double ColumnWidth = 440;

    public BatchConvertSettingsWindow(BatchConvertSettingsViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.Tools.BatchConvert.BatchConvertSettings;
        SizeToContent = SizeToContent.WidthAndHeight;
        CanResize = false;
        EditorChrome.ApplyWindow(this, EditorWindowStyles.Video);
        vm.Window = this;
        DataContext = vm;

        var comboBoxTargetEncoding = Combo(vm.TargetEncodings, vm, nameof(vm.SelectedTargetEncoding), Se.Language.General.TargetEncoding);

        var radioUseSourceFolder = new RadioButton
        {
            Content = Se.Language.General.UseSourceFolder,
            IsChecked = vm.UseSourceFolder,
            FontSize = EditorChrome.FieldFontSize,
            VerticalAlignment = VerticalAlignment.Center,
            [!RadioButton.IsCheckedProperty] = new Binding(nameof(vm.UseSourceFolder)) { Mode = BindingMode.TwoWay, UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged },
        };

        var radioUseOutputFolder = new RadioButton
        {
            Content = Se.Language.General.UseOutputFolder,
            IsChecked = vm.UseOutputFolder,
            FontSize = EditorChrome.FieldFontSize,
            VerticalAlignment = VerticalAlignment.Center,
            [!RadioButton.IsCheckedProperty] = new Binding(nameof(vm.UseOutputFolder)) { Mode = BindingMode.TwoWay, UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged },
        };

        var textBoxOutputFolder = EditorChrome.MakeTextField(nameof(vm.OutputFolder), Se.Language.General.OutputFolder);
        textBoxOutputFolder.Text = vm.OutputFolder;
        textBoxOutputFolder.Bind(IsEnabledProperty, new Binding(nameof(vm.UseOutputFolder)) { Mode = BindingMode.OneWay });

        var buttonBrowse = EditorChrome.CreateToolButton("fa-solid fa-folder-open", Se.Language.General.UseOutputFolder, vm.BrowseOutputFolderCommand, 28);

        var panelOutputFolder = new Grid
        {
            Margin = new Thickness(28, 0, 0, 0), // under the "Use output folder" radio button
            ColumnSpacing = 4,
            ColumnDefinitions = { new ColumnDefinition(new GridLength(1, GridUnitType.Star)), new ColumnDefinition(GridLength.Auto) },
        };
        panelOutputFolder.Add(textBoxOutputFolder, 0, 0);
        panelOutputFolder.Add(buttonBrowse, 0, 1);

        var folder = new StackPanel { Spacing = 4, Children = { radioUseSourceFolder, radioUseOutputFolder, panelOutputFolder } };

        var comboBoxLanguagePostFix = Combo(vm.LanguagePostFixes, vm, nameof(vm.SelectedLanguagePostFix), Se.Language.General.LanguagePostFix);

        var output = new StackPanel
        {
            Spacing = 8,
            Children =
            {
                EditorChrome.MakeLabeledRow(Se.Language.General.TargetEncoding, comboBoxTargetEncoding, LabelWidth, wrap: true),
                folder,
                EditorChrome.MakeSwitchRow(nameof(vm.Overwrite), Se.Language.General.OverwriteExistingFiles),
                EditorChrome.MakeSwitchRow(nameof(vm.KeepSourceTimestamp), Se.Language.Tools.BatchConvert.KeepSourceFileTimestamp),
                EditorChrome.MakeLabeledRow(Se.Language.General.LanguagePostFix, comboBoxLanguagePostFix, LabelWidth, wrap: true),
            }
        };

        var comboBoxScanFolderVideoFiles = Combo(vm.ScanFolderVideoFilesOptions, vm, nameof(vm.SelectedScanFolderVideoFilesOption), Se.Language.Tools.BatchConvert.VideoFilesWhenAddingFolder);
        var addingFiles = new StackPanel
        {
            Spacing = 8,
            Children =
            {
                EditorChrome.MakeSwitchRow(nameof(vm.ScanFolderRecursive), Se.Language.Tools.BatchConvert.IncludeSubfolders),
                EditorChrome.MakeLabeledRow(Se.Language.Tools.BatchConvert.VideoFilesWhenAddingFolder, comboBoxScanFolderVideoFiles, LabelWidth, wrap: true),
            }
        };

        var comboBoxOcrEngine = Combo(vm.OcrEngines, vm, nameof(vm.SelectedOcrEngine), Se.Language.Ocr.OcrEngine);
        var comboBoxTesseractLanguages = Combo(vm.TesseractDictionaryItems, vm, nameof(vm.SelectedTesseractDictionaryItem), Se.Language.General.Language)
            .WithBindVisible(nameof(vm.IsTesseractOcrVisible));
        var comboBoxTesseractEngineMode = Combo(vm.TesseractEngineModes, vm, nameof(vm.SelectedTesseractEngineMode), Se.Language.Ocr.TesseractEngineMode);
        var comboBoxPaddleLanguages = Combo(vm.PaddleOcrLanguages, vm, nameof(vm.SelectedPaddleOcrLanguage), Se.Language.General.Language)
            .WithBindVisible(nameof(vm.IsPaddleOCrVisible));
        var comboBoxBinaryOcrDatabases = Combo(vm.BinaryOcrDatabases, vm, nameof(vm.SelectedBinaryOcrDatabase), Se.Language.Ocr.Database);
        var comboBoxBinaryOcrFallback = Combo(vm.BinaryOcrFallbackNOcrDatabases, vm, nameof(vm.SelectedBinaryOcrFallbackNOcrDatabase), Se.Language.Ocr.BinaryOcrNOcrFallbackDatabase);
        var comboBoxNOcrDatabases = Combo(vm.NOcrDatabases, vm, nameof(vm.SelectedNOcrDatabase), Se.Language.Ocr.Database);
        var comboBoxNOcrFallback = Combo(vm.NOcrFallbackBinaryOcrDatabases, vm, nameof(vm.SelectedNOcrFallbackBinaryOcrDatabase), Se.Language.Ocr.NOcrBinaryOcrFallbackDatabase);
        var comboBoxOllamaModels = Combo(vm.OllamaModels, vm, nameof(vm.SelectedOllamaModel), Se.Language.General.Model);
        var buttonOllamaModelBrowse = EditorChrome.CreateToolButton("fa-solid fa-ellipsis", Se.Language.General.Model, vm.PickOllamaModelCommand, 28);
        var ollama = new Grid
        {
            ColumnSpacing = 4,
            ColumnDefinitions = { new ColumnDefinition(new GridLength(1, GridUnitType.Star)), new ColumnDefinition(GridLength.Auto) },
        };
        ollama.Add(comboBoxOllamaModels, 0, 0);
        ollama.Add(buttonOllamaModelBrowse, 0, 1);
        var comboBoxLlamaCppModels = Combo(vm.LlamaCppOcrModels, vm, nameof(vm.SelectedLlamaCppOcrModel), Se.Language.General.Model);
        comboBoxLlamaCppModels.ItemTemplate = StatusDots.ComboItemTemplate<LlamaCppModelDisplay>(
            model => model.Model.DisplayName,
            model => model.Model.Size,
            model => model.IsInstalled ? DownloadDotStatus.UpToDate : DownloadDotStatus.NotInstalled);
        // Apple Vision: a language and nothing else - no model, no backend, no download.
        var comboBoxAppleVisionLanguages = Combo(vm.AppleVisionLanguages, vm, nameof(vm.SelectedAppleVisionLanguage), Se.Language.General.Language);
        var comboBoxCrispEmbedBackends = Combo(vm.CrispEmbedBackends, vm, nameof(vm.SelectedCrispEmbedBackend), Se.Language.General.Backend);
        var comboBoxCrispEmbedModels = Combo(vm.CrispEmbedModels, vm, nameof(vm.SelectedCrispEmbedModel), Se.Language.General.Model);
        comboBoxCrispEmbedModels.ItemTemplate = MakeCrispEmbedModelItemTemplate();
        vm.RefreshCrispEmbedModelCombo = () => comboBoxCrispEmbedModels.ItemTemplate = MakeCrispEmbedModelItemTemplate();
        comboBoxOcrEngine.SelectionChanged += (s, e) => vm.OnOcrEngineChanged();

        var vobSubIsolateColors = EditorChrome.MakeSwitchRow(nameof(vm.VobSubIsolateColors), Se.Language.Ocr.VobSubIsolateColors, out var toggleVobSubIsolateColors);
        if (Se.Settings.Appearance.ShowHints)
        {
            ToolTip.SetTip(toggleVobSubIsolateColors, Se.Language.Ocr.VobSubIsolateColorsHint);
        }

        // One row per engine setting; each shows only for the engine it belongs to
        var ocr = new StackPanel
        {
            Spacing = 8,
            Children =
            {
                EditorChrome.MakeLabeledRow(Se.Language.Ocr.OcrEngine, comboBoxOcrEngine, LabelWidth, wrap: true),
                EditorChrome.MakeLabeledRow(Se.Language.General.Language, new Panel { Children = { comboBoxTesseractLanguages, comboBoxPaddleLanguages } }, LabelWidth, wrap: true, visibleProperty: nameof(vm.IsOcrLanguageVisible)),
                EditorChrome.MakeLabeledRow(Se.Language.Ocr.TesseractEngineMode, comboBoxTesseractEngineMode, LabelWidth, wrap: true, visibleProperty: nameof(vm.IsTesseractOcrVisible)),
                EditorChrome.MakeLabeledRow(Se.Language.Ocr.Database, comboBoxBinaryOcrDatabases, LabelWidth, wrap: true, visibleProperty: nameof(vm.IsBinaryOcrVisible)),
                EditorChrome.MakeLabeledRow(Se.Language.Ocr.BinaryOcrNOcrFallbackDatabase, comboBoxBinaryOcrFallback, LabelWidth, wrap: true, visibleProperty: nameof(vm.IsBinaryOcrVisible)),
                EditorChrome.MakeLabeledRow(Se.Language.Ocr.Database, comboBoxNOcrDatabases, LabelWidth, wrap: true, visibleProperty: nameof(vm.IsNOcrVisible)),
                EditorChrome.MakeLabeledRow(Se.Language.Ocr.NOcrBinaryOcrFallbackDatabase, comboBoxNOcrFallback, LabelWidth, wrap: true, visibleProperty: nameof(vm.IsNOcrVisible)),
                EditorChrome.MakeLabeledRow(Se.Language.General.Model, ollama, LabelWidth, wrap: true, visibleProperty: nameof(vm.IsOllamaVisible)),
                EditorChrome.MakeLabeledRow(Se.Language.General.Model, comboBoxLlamaCppModels, LabelWidth, wrap: true, visibleProperty: nameof(vm.IsLlamaCppVisible)),
                EditorChrome.MakeLabeledRow(Se.Language.General.Backend, comboBoxCrispEmbedBackends, LabelWidth, wrap: true, visibleProperty: nameof(vm.IsCrispEmbedVisible)),
                EditorChrome.MakeLabeledRow(Se.Language.General.Model, comboBoxCrispEmbedModels, LabelWidth, wrap: true, visibleProperty: nameof(vm.IsCrispEmbedVisible)),
                EditorChrome.MakeLabeledRow(Se.Language.General.Language, comboBoxAppleVisionLanguages, LabelWidth, wrap: true, visibleProperty: nameof(vm.IsAppleVisionVisible)),
                vobSubIsolateColors,
            }
        };

        var other = EditorChrome.MakeSwitchRow(nameof(vm.PreventSleep), Se.Language.Tools.BatchConvert.PreventSleepWhileConverting);

        var left = new Border
        {
            Width = ColumnWidth,
            BorderBrush = EditorChrome.LineBrush,
            BorderThickness = new Thickness(0, 0, 1, 0),
            Child = EditorChrome.MakeSectionStack(EditorChrome.MakeEditorSection(Se.Language.General.Output, output, first: true), EditorChrome.MakeEditorSection(Se.Language.General.Other, other)),
        };
        var right = new Border
        {
            Width = ColumnWidth,
            Child = EditorChrome.MakeSectionStack(EditorChrome.MakeEditorSection(Se.Language.Tools.BatchConvert.SettingsSectionAddingFiles, addingFiles, first: true), EditorChrome.MakeEditorSection(Se.Language.Tools.BatchConvert.SettingsSectionImageBasedSubtitles, ocr)),
        };

        var columns = new Grid
        {
            ColumnDefinitions = { new ColumnDefinition(GridLength.Auto), new ColumnDefinition(GridLength.Auto) },
        };
        columns.Add(left, 0, 0);
        columns.Add(right, 0, 1);

        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        buttonOk.Classes.Add("primary");
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);

        // No scroller of its own: a window clamped to a short screen wraps it all in one (#16026)
        Content = new StackPanel
        {
            Children =
            {
                EditorChrome.MakePanel(columns),
                EditorChrome.MakeButtonBar(null, buttonCancel, buttonOk),
            }
        };

        UiUtil.FocusOnFirstActivation(this, comboBoxTargetEncoding); // initial focus on an input, not an action button - a focused button clicks on bare Space
        KeyDown += (s, e) => vm.OnKeyDown(e);
    }

    private static ComboBox Combo<T>(System.Collections.ObjectModel.ObservableCollection<T> items, BatchConvertSettingsViewModel vm, string property, string name)
    {
        var comboBox = UiUtil.MakeComboBox(items, vm, property);
        comboBox.HorizontalAlignment = HorizontalAlignment.Stretch;
        AutomationProperties.SetName(comboBox, name);
        return comboBox;
    }

    // Model combo item template: a dot (green = downloaded, grey = not downloaded yet) plus the
    // model's download size - same treatment as the OCR window's CrispEmbed model combo.
    private static FuncDataTemplate<CrispEmbedModelDisplay> MakeCrispEmbedModelItemTemplate()
    {
        return StatusDots.ComboItemTemplate<CrispEmbedModelDisplay>(
            model => model.Model.Name,
            model => string.IsNullOrEmpty(model.Model.Size) ? null : model.Model.Size,
            model => model.Backend.IsModelInstalled(model.Model)
                ? DownloadDotStatus.UpToDate
                : DownloadDotStatus.NotInstalled);
    }
}
