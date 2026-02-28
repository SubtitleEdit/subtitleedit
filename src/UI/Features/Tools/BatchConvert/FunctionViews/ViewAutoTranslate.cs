using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Layout;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Tools.BatchConvert.FunctionViews;

public static class ViewAutoTranslate
{
    public static Control Make(BatchConvertViewModel vm)
    {
        var labelHeader = new Label
        {
            Content = Se.Language.General.AutoTranslate,
            VerticalAlignment = VerticalAlignment.Center,
            FontWeight = Avalonia.Media.FontWeight.Bold
        };

        var labelEngine = UiUtil.MakeLabel(Se.Language.General.Engine);

        var cbEngines = new ComboBox
        {
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalContentAlignment = HorizontalAlignment.Left,
            VerticalContentAlignment = VerticalAlignment.Center,
        };
        cbEngines.DataContext = vm;
        cbEngines.Bind(ComboBox.ItemsSourceProperty, new Binding(nameof(vm.AutoTranslators)));
        cbEngines.Bind(ComboBox.SelectedItemProperty, new Binding(nameof(vm.SelectedAutoTranslator)) { Mode = BindingMode.TwoWay });
        cbEngines.SelectionChanged += (s, e) => vm.OnAutoTranslatorChanged();
        var labelModel = UiUtil.MakeLabel(Se.Language.General.Model).WithBindVisible(vm, nameof(vm.AutoTranslateModelIsVisible)).WithMarginLeft(10).WithMarginRight(3);
        var textBoxModel = UiUtil.MakeTextBox(150, vm, nameof(vm.AutoTranslateModel), nameof(vm.AutoTranslateModelIsVisible));
        var buttonModel = UiUtil.MakeButtonBrowse(vm.AutoTranslateBrowseModelCommand, nameof(vm.AutoTranslateModelBrowseIsVisible)).WithMarginLeft(3);
        var panelEngineControls = UiUtil.MakeHorizontalPanel(
            cbEngines,
            labelModel,
            textBoxModel,
            buttonModel);

        var labelSourceLanguage = UiUtil.MakeLabel(Se.Language.General.From);
        var sourceLangCombo = UiUtil.MakeComboBox(vm.SourceLanguages, vm, nameof(vm.SelectedSourceLanguage));

        var labelTargetLanguage = UiUtil.MakeLabel(Se.Language.General.To);
        var targetLangCombo = UiUtil.MakeComboBox(vm.TargetLanguages, vm, nameof(vm.SelectedTargetLanguage));

        var labelUrl = UiUtil.MakeLabel(Se.Language.General.Url).WithBindVisible(vm, nameof(vm.AutoTranslateUrlIsVisible));
        var textBoxUrl = UiUtil.MakeTextBox(300, vm, nameof(vm.AutoTranslateUrl), nameof(vm.AutoTranslateUrlIsVisible));

        var labelApiKey = UiUtil.MakeLabel(Se.Language.General.ApiKey).WithBindVisible(vm, nameof(vm.AutoTranslateApiKeyIsVisible));
        var textBoxApiKey = UiUtil.MakeTextBox(300, vm, nameof(vm.AutoTranslateApiKey), nameof(vm.AutoTranslateApiKeyIsVisible));


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
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            ColumnSpacing = 10,
            RowSpacing = 10,
        };

        grid.Add(labelHeader, 0, 0);

        grid.Add(labelEngine, 1, 0);
        grid.Add(panelEngineControls, 1, 1);

        grid.Add(labelSourceLanguage, 2, 0);
        grid.Add(sourceLangCombo, 2, 1);

        grid.Add(labelTargetLanguage, 3, 0);
        grid.Add(targetLangCombo, 3, 1);

        grid.Add(labelUrl, 4, 0);
        grid.Add(textBoxUrl, 4, 1);

        grid.Add(labelApiKey, 5, 0);
        grid.Add(textBoxApiKey, 5, 1);

        return grid;
    }
}
