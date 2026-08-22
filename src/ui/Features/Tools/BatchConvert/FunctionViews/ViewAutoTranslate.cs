using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Layout;
using Nikse.SubtitleEdit.Features.Translate;
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

        // Install-status dots for the engines Subtitle Edit downloads itself, same as in the
        // Auto-translate window. Re-assigning the template is what re-evaluates them, so the view
        // model gets a hook to call after a run has downloaded the engine.
        cbEngines.ItemTemplate = AutoTranslateCombos.EngineItemTemplate();
        vm.RefreshAutoTranslateEngineDots = () => cbEngines.ItemTemplate = AutoTranslateCombos.EngineItemTemplate();

        // Model gets a row of its own - the engine row is already crowded with the engine combo and
        // its settings buttons, and the three model editors (Ollama text box + browse, the CrispASR
        // combo, the llama.cpp combo) are never visible at the same time, so they share the row.
        var labelModel = UiUtil.MakeLabel(Se.Language.General.Model).WithBindVisible(vm, nameof(vm.AutoTranslateModelIsVisible));
        var textBoxModel = UiUtil.MakeTextBox(150, vm, nameof(vm.AutoTranslateModel), nameof(vm.AutoTranslateModelIsVisible));
        var buttonModel = UiUtil.MakeButtonBrowse(vm.AutoTranslateBrowseModelCommand, nameof(vm.AutoTranslateModelBrowseIsVisible), Se.Language.General.Model).WithMarginLeft(3);

        var labelCrispAsrModel = UiUtil.MakeLabel(Se.Language.General.Model).WithBindVisible(vm, nameof(vm.CrispAsrModelComboIsVisible));
        var crispAsrModelCombo = UiUtil.MakeComboBox(vm.CrispAsrModels, vm, nameof(vm.SelectedCrispAsrModel), nameof(vm.CrispAsrModelComboIsVisible))
            .WithAccessibleName(Se.Language.General.Model);
        crispAsrModelCombo.ItemTemplate = AutoTranslateCombos.CrispAsrModelItemTemplate();

        var labelLlamaCppModel = UiUtil.MakeLabel(Se.Language.General.Model).WithBindVisible(vm, nameof(vm.LlamaCppModelComboIsVisible));
        var llamaCppModelCombo = UiUtil.MakeComboBox(vm.LlamaCppModels, vm, nameof(vm.SelectedLlamaCppModel), nameof(vm.LlamaCppModelComboIsVisible))
            .WithWidth(220)
            .WithAccessibleName(Se.Language.General.Model);
        llamaCppModelCombo.ItemTemplate = AutoTranslateCombos.LlamaCppModelItemTemplate();

        var panelModelLabels = UiUtil.MakeHorizontalPanel(labelModel, labelCrispAsrModel, labelLlamaCppModel);
        var panelModelControls = UiUtil.MakeHorizontalPanel(
            textBoxModel,
            buttonModel,
            crispAsrModelCombo,
            llamaCppModelCombo);

        // Installed backend, pinned release and install state of llama.cpp itself. It sits directly
        // after the engine combo because it configures the engine, not the model or the prompt.
        var buttonEngineSettings = UiUtil.MakeButton(vm.ShowLlamaCppEngineSettingsCommand, IconNames.Settings)
            .WithMarginLeft(5)
            .WithAccessibleName(Se.Language.General.LlamaCppEngineSettings);
        buttonEngineSettings.Bind(Button.IsVisibleProperty, new Binding(nameof(vm.LlamaCppEngineSettingsButtonIsVisible)));
        if (Se.Settings.Appearance.ShowHints)
        {
            ToolTip.SetTip(buttonEngineSettings, Se.Language.General.LlamaCppEngineSettings);
        }

        var buttonAdvanced = UiUtil.MakeButton(Se.Language.Translate.AdvancedDotDotDot, vm.ShowLlamaCppAdvancedSettingsCommand)
            .WithMarginLeft(10)
            .WithAccessibleName(Se.Language.Translate.AdvancedSettings);
        buttonAdvanced.Bind(Button.IsVisibleProperty, new Binding(nameof(vm.LlamaCppAdvancedButtonIsVisible)));
        if (Se.Settings.Appearance.ShowHints)
        {
            ToolTip.SetTip(buttonAdvanced, Se.Language.Translate.AdvancedSettings);
        }

        // Batch convert's own local/external llama-server switch (#14005) - independent of the one
        // in the Auto-translate window. Shown only for the llama.cpp engines.
        var checkBoxLlamaCppRemote = UiUtil.MakeCheckBox(Se.Language.General.LlamaCppUseRemoteServer, vm, nameof(vm.LlamaCppUseRemoteServer))
            .WithMarginLeft(10);
        checkBoxLlamaCppRemote.Bind(CheckBox.IsVisibleProperty, new Binding(nameof(vm.LlamaCppRemoteToggleIsVisible)));

        var panelEngineControls = UiUtil.MakeHorizontalPanel(
            cbEngines,
            buttonEngineSettings,
            buttonAdvanced,
            checkBoxLlamaCppRemote);

        // Prompt and the shared request settings for the regular llama.cpp engine (the advanced
        // engine has its own window instead). Right-aligned and away from the engine gear, the same
        // way the Auto-translate window separates the two.
        var buttonSettings = UiUtil.MakeButton(vm.ShowLlamaCppTranslateSettingsCommand, IconNames.Settings)
            .WithMarginLeft(10)
            .WithAccessibleName(Se.Language.General.Settings);
        buttonSettings.HorizontalAlignment = HorizontalAlignment.Right;
        buttonSettings.VerticalAlignment = VerticalAlignment.Center;
        buttonSettings.Bind(Button.IsVisibleProperty, new Binding(nameof(vm.LlamaCppSettingsButtonIsVisible)));
        if (Se.Settings.Appearance.ShowHints)
        {
            ToolTip.SetTip(buttonSettings, Se.Language.General.Settings);
        }

        var panelEngineRow = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("*,Auto"),
        };
        panelEngineRow.Add(panelEngineControls, 0, 0);
        panelEngineRow.Add(buttonSettings, 0, 1);

        var labelSourceLanguage = UiUtil.MakeLabel(Se.Language.General.From);
        var sourceLangCombo = UiUtil.MakeComboBox(vm.SourceLanguages, vm, nameof(vm.SelectedSourceLanguage));

        var labelTargetLanguage = UiUtil.MakeLabel(Se.Language.General.To);
        var targetLangCombo = UiUtil.MakeComboBox(vm.TargetLanguages, vm, nameof(vm.SelectedTargetLanguage));

        var labelUrl = UiUtil.MakeLabel(Se.Language.General.Url).WithBindVisible(vm, nameof(vm.AutoTranslateUrlIsVisible));
        var textBoxUrl = UiUtil.MakeTextBox(300, vm, nameof(vm.AutoTranslateUrl), nameof(vm.AutoTranslateUrlIsVisible));

        var labelApiKey = UiUtil.MakeLabel(Se.Language.General.ApiKey).WithBindVisible(vm, nameof(vm.AutoTranslateApiKeyIsVisible));
        var textBoxApiKey = UiUtil.MakeApiKeyTextBox(300, vm, nameof(vm.AutoTranslateApiKey), nameof(vm.AutoTranslateApiKeyIsVisible));


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
        grid.Add(panelEngineRow, 1, 1);

        grid.Add(panelModelLabels, 2, 0);
        grid.Add(panelModelControls, 2, 1);

        grid.Add(labelSourceLanguage, 3, 0);
        grid.Add(sourceLangCombo, 3, 1);

        grid.Add(labelTargetLanguage, 4, 0);
        grid.Add(targetLangCombo, 4, 1);

        grid.Add(labelUrl, 5, 0);
        grid.Add(textBoxUrl, 5, 1);

        grid.Add(labelApiKey, 6, 0);
        grid.Add(textBoxApiKey, 6, 1);

        return grid;
    }
}
