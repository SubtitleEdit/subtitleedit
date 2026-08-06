using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Layout;
using Nikse.SubtitleEdit.Features.Ocr.Engines;
using Nikse.SubtitleEdit.Features.Translate;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.UiLogic.Ocr;

namespace Nikse.SubtitleEdit.Features.Tools.BatchConvert;

public class BatchConvertSettingsWindow : Window
{
    public BatchConvertSettingsWindow(BatchConvertSettingsViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.Tools.BatchConvert.BatchConvertSettings;
        SizeToContent = SizeToContent.WidthAndHeight;
        CanResize = false;
        vm.Window = this;
        DataContext = vm;

        var labelTargetEncoding = UiUtil.MakeLabel(Se.Language.General.TargetEncoding).WithMarginLeft(5);
        var comboBoxTargetEncoding = UiUtil.MakeComboBox(vm.TargetEncodings, vm, nameof(vm.SelectedTargetEncoding));
        var panelTargetEncoding = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Children = { labelTargetEncoding, comboBoxTargetEncoding }
        };

        var checkBoxOverwrite = new CheckBox
        {
            Content = Se.Language.General.OverwriteExistingFiles,
            IsChecked = vm.Overwrite,
            VerticalAlignment = VerticalAlignment.Center,
            [!CheckBox.IsCheckedProperty] = new Binding(nameof(vm.Overwrite)) { Mode = BindingMode.TwoWay, UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged },
        };

        var checkBoxUseSourceFolder = new RadioButton
        {
            Content = Se.Language.General.UseSourceFolder,
            IsChecked = vm.UseSourceFolder,
            VerticalAlignment = VerticalAlignment.Center,
            [!RadioButton.IsCheckedProperty] = new Binding(nameof(vm.UseSourceFolder)) { Mode = BindingMode.TwoWay, UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged },
        };

        var checkBoxUseOutputFolder = new RadioButton
        {
            Content = Se.Language.General.UseOutputFolder,
            IsChecked = vm.UseOutputFolder,
            VerticalAlignment = VerticalAlignment.Center,
            [!RadioButton.IsCheckedProperty] = new Binding(nameof(vm.UseOutputFolder)) { Mode = BindingMode.TwoWay, UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged },
        };

        var textBoxOutputFolder = new TextBox
        {
            Text = vm.OutputFolder,
            VerticalAlignment = VerticalAlignment.Center,
            [!TextBox.TextProperty] = new Binding(nameof(vm.OutputFolder)) { Mode = BindingMode.TwoWay, UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged },
            [!Control.IsEnabledProperty] = new Binding(nameof(vm.UseOutputFolder)) { Mode = BindingMode.OneWay },
            Width = 400,
        };

        var buttonBrowse = UiUtil.MakeButtonBrowse(vm.BrowseOutputFolderCommand, accessibleName: Se.Language.General.UseOutputFolder);

        var panelOutputFolder = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            VerticalAlignment = VerticalAlignment.Center,
            Spacing = 5,
            Children =
            {
                textBoxOutputFolder,
                buttonBrowse
            }
        };

        var labelOcrEngine = UiUtil.MakeLabel(Se.Language.Ocr.OcrEngine);
        var comboBoxOcrEngine = UiUtil.MakeComboBox(vm.OcrEngines, vm, nameof(vm.SelectedOcrEngine));
        var labelOcLanguage = UiUtil.MakeLabel(Se.Language.General.Language).WithBindVisible(vm, nameof(vm.IsOcrLanguageVisible)).WithMarginLeft(10);
        var labelBinaryOcrDatabase = UiUtil.MakeLabel(Se.Language.Ocr.Database).WithBindVisible(vm, nameof(vm.IsBinaryOcrVisible)).WithMarginLeft(10);
        var comboBoxTesseractLanguages = UiUtil.MakeComboBox(vm.TesseractDictionaryItems, vm, nameof(vm.SelectedTesseractDictionaryItem))
            .WithBindVisible(nameof(vm.IsTesseractOcrVisible));
        var labelTesseractEngineMode = UiUtil.MakeLabel(Se.Language.Ocr.TesseractEngineMode).WithBindVisible(vm, nameof(vm.IsTesseractOcrVisible)).WithMarginLeft(10);
        var comboBoxTesseractEngineMode = UiUtil.MakeComboBox(vm.TesseractEngineModes, vm, nameof(vm.SelectedTesseractEngineMode))
            .WithBindVisible(nameof(vm.IsTesseractOcrVisible));
        var comboBoxPaddleLanguages = UiUtil.MakeComboBox(vm.PaddleOcrLanguages, vm, nameof(vm.SelectedPaddleOcrLanguage))
            .WithBindVisible(nameof(vm.IsPaddleOCrVisible));
        var comboBoxBinaryOcrDatabases = UiUtil.MakeComboBox(vm.BinaryOcrDatabases, vm, nameof(vm.SelectedBinaryOcrDatabase))
            .WithBindVisible(nameof(vm.IsBinaryOcrVisible));
        var labelBinaryOcrFallback = UiUtil.MakeLabel(Se.Language.Ocr.BinaryOcrNOcrFallbackDatabase).WithBindVisible(vm, nameof(vm.IsBinaryOcrVisible)).WithMarginLeft(10);
        var comboBoxBinaryOcrFallback = UiUtil.MakeComboBox(vm.BinaryOcrFallbackNOcrDatabases, vm, nameof(vm.SelectedBinaryOcrFallbackNOcrDatabase))
            .WithBindVisible(nameof(vm.IsBinaryOcrVisible));
        var labelNOcrDatabase = UiUtil.MakeLabel(Se.Language.Ocr.Database).WithBindVisible(vm, nameof(vm.IsNOcrVisible)).WithMarginLeft(10);
        var comboBoxNOcrDatabases = UiUtil.MakeComboBox(vm.NOcrDatabases, vm, nameof(vm.SelectedNOcrDatabase))
            .WithBindVisible(nameof(vm.IsNOcrVisible));
        var labelNOcrFallback = UiUtil.MakeLabel(Se.Language.Ocr.NOcrBinaryOcrFallbackDatabase).WithBindVisible(vm, nameof(vm.IsNOcrVisible)).WithMarginLeft(10);
        var comboBoxNOcrFallback = UiUtil.MakeComboBox(vm.NOcrFallbackBinaryOcrDatabases, vm, nameof(vm.SelectedNOcrFallbackBinaryOcrDatabase))
            .WithBindVisible(nameof(vm.IsNOcrVisible));
        var labelOllamaModel = UiUtil.MakeLabel(Se.Language.General.Model).WithBindVisible(vm, nameof(vm.IsOllamaVisible)).WithMarginLeft(10);
        var comboBoxOllamaModels = UiUtil.MakeComboBox(vm.OllamaModels, vm, nameof(vm.SelectedOllamaModel))
            .WithBindVisible(nameof(vm.IsOllamaVisible));
        var buttonOllamaModelBrowse = UiUtil.MakeButtonBrowse(vm.PickOllamaModelCommand, nameof(vm.IsOllamaVisible), Se.Language.General.Model).WithMarginLeft(3);
        var labelLlamaCppModel = UiUtil.MakeLabel(Se.Language.General.Model).WithBindVisible(vm, nameof(vm.IsLlamaCppVisible)).WithMarginLeft(10);
        var comboBoxLlamaCppModels = UiUtil.MakeComboBox(vm.LlamaCppOcrModels, vm, nameof(vm.SelectedLlamaCppOcrModel))
            .WithBindVisible(nameof(vm.IsLlamaCppVisible));
        comboBoxLlamaCppModels.ItemTemplate = StatusDots.ComboItemTemplate<LlamaCppModelDisplay>(
            model => model.Model.DisplayName,
            model => model.Model.Size,
            model => model.IsInstalled ? DownloadDotStatus.UpToDate : DownloadDotStatus.NotInstalled);
        var labelCrispEmbedBackend = UiUtil.MakeLabel(Se.Language.General.Backend).WithBindVisible(vm, nameof(vm.IsCrispEmbedVisible)).WithMarginLeft(10);
        var comboBoxCrispEmbedBackends = UiUtil.MakeComboBox(vm.CrispEmbedBackends, vm, nameof(vm.SelectedCrispEmbedBackend))
            .WithBindVisible(nameof(vm.IsCrispEmbedVisible));
        var labelCrispEmbedModel = UiUtil.MakeLabel(Se.Language.General.Model).WithBindVisible(vm, nameof(vm.IsCrispEmbedVisible)).WithMarginLeft(10);
        var comboBoxCrispEmbedModels = UiUtil.MakeComboBox(vm.CrispEmbedModels, vm, nameof(vm.SelectedCrispEmbedModel))
            .WithBindVisible(nameof(vm.IsCrispEmbedVisible));
        comboBoxCrispEmbedModels.ItemTemplate = MakeCrispEmbedModelItemTemplate();
        vm.RefreshCrispEmbedModelCombo = () => comboBoxCrispEmbedModels.ItemTemplate = MakeCrispEmbedModelItemTemplate();
        var panelOcrEngine = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Margin = new Avalonia.Thickness(0, 30, 0, 0),
            Children = { labelOcrEngine, comboBoxOcrEngine, labelOcLanguage, comboBoxTesseractLanguages, labelTesseractEngineMode, comboBoxTesseractEngineMode, comboBoxPaddleLanguages, labelBinaryOcrDatabase, comboBoxBinaryOcrDatabases, labelBinaryOcrFallback, comboBoxBinaryOcrFallback, labelNOcrDatabase, comboBoxNOcrDatabases, labelNOcrFallback, comboBoxNOcrFallback, labelOllamaModel, comboBoxOllamaModels, buttonOllamaModelBrowse, labelLlamaCppModel, comboBoxLlamaCppModels, labelCrispEmbedBackend, comboBoxCrispEmbedBackends, labelCrispEmbedModel, comboBoxCrispEmbedModels }
        };
        comboBoxOcrEngine.SelectionChanged += (s, e) => vm.OnOcrEngineChanged();

        var checkBoxVobSubIsolateColors = new CheckBox
        {
            Content = Se.Language.Ocr.VobSubIsolateColors,
            IsChecked = vm.VobSubIsolateColors,
            VerticalAlignment = VerticalAlignment.Center,
            [!CheckBox.IsCheckedProperty] = new Binding(nameof(vm.VobSubIsolateColors)) { Mode = BindingMode.TwoWay, UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged },
        };
        ToolTip.SetTip(checkBoxVobSubIsolateColors, Se.Language.Ocr.VobSubIsolateColorsHint);

        var labelLanguagePostFix = UiUtil.MakeLabel(Se.Language.General.LanguagePostFix);
        var comboBoxLanguagePostFix = UiUtil.MakeComboBox(vm.LanguagePostFixes, vm, nameof(vm.SelectedLanguagePostFix));
        var panelLanguagePostFix = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Margin = new Avalonia.Thickness(0, 30, 0, 0),
            Children = { labelLanguagePostFix, comboBoxLanguagePostFix }
        };

        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);
        var panelButtons = UiUtil.MakeButtonBar(buttonOk, buttonCancel);

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
            },
            Margin = UiUtil.MakeWindowMargin(),
            ColumnSpacing = 10,
            RowSpacing = 10,
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        grid.Add(panelTargetEncoding, 0, 0);
        grid.Add(checkBoxOverwrite, 1, 0);
        grid.Add(checkBoxUseSourceFolder, 2, 0);
        grid.Add(checkBoxUseOutputFolder, 3, 0);
        grid.Add(panelOutputFolder, 4, 0);
        grid.Add(panelOcrEngine, 5, 0);
        grid.Add(checkBoxVobSubIsolateColors, 6, 0);
        grid.Add(panelLanguagePostFix, 7, 0);
        grid.Add(panelButtons, 8, 0);


        Content = grid;

        Activated += delegate { comboBoxTargetEncoding.Focus(); }; // initial focus on an input, not an action button - a focused button clicks on bare Space
        KeyDown += (s, e) => vm.OnKeyDown(e);
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
