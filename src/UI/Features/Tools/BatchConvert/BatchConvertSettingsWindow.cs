using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Layout;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

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
            IsEnabled = vm.UseOutputFolder,
            Width = 400,
        };

        var buttonBrowse = UiUtil.MakeButtonBrowse(vm.BrowseOutputFolderCommand);

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
        var comboBoxTesseractLanguages = UiUtil.MakeComboBox(vm.TesseractDictionaryItems, vm, nameof(vm.SelectedTesseractDictionaryItem))
            .WithBindVisible(nameof(vm.IsTesseractOcrVisible));
        var comboBoxPaddleLanguages = UiUtil.MakeComboBox(vm.PaddleOcrLanguages, vm, nameof(vm.SelectedPaddleOcrLanguage))
            .WithBindVisible(nameof(vm.IsPaddleOCrVisible));
        var panelOcrEngine = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Margin = new Avalonia.Thickness(0, 30, 0, 0),
            Children = { labelOcrEngine, comboBoxOcrEngine, labelOcLanguage, comboBoxTesseractLanguages, comboBoxPaddleLanguages }
        };
        comboBoxOcrEngine.SelectionChanged += (s, e) => vm.OnOcrEngineChanged();

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
        grid.Add(panelLanguagePostFix, 6, 0);
        grid.Add(panelButtons, 7, 0);


        Content = grid;

        Activated += delegate { buttonOk.Focus(); }; // hack to make OnKeyDown work
        KeyDown += (s, e) => vm.OnKeyDown(e);
    }
}
