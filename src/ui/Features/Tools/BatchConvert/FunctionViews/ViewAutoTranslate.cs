using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Layout;
using Nikse.SubtitleEdit.Features.Translate;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Tools.BatchConvert.FunctionViews;

public static class ViewAutoTranslate
{
    public static Control Make(BatchConvertViewModel vm)
    {
        var cbEngines = new ComboBox
        {
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalContentAlignment = HorizontalAlignment.Left,
            VerticalContentAlignment = VerticalAlignment.Center,
            MinWidth = 220,
        };
        AutomationProperties.SetName(cbEngines, Se.Language.General.Engine);
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
        var textBoxModel = EditorChrome.MakeTextField(nameof(vm.AutoTranslateModel), Se.Language.General.Model);
        textBoxModel.Bind(Visual.IsVisibleProperty, new Binding(nameof(vm.AutoTranslateModelIsVisible)));
        var buttonModel = UiUtil.MakeButtonBrowse(vm.AutoTranslateBrowseModelCommand, nameof(vm.AutoTranslateModelBrowseIsVisible), Se.Language.General.Model);
        buttonModel.Margin = new Thickness(6, 0, 0, 0);
        DockPanel.SetDock(buttonModel, Dock.Right);

        var crispAsrModelCombo = FunctionViewChrome.Compact(UiUtil.MakeComboBox(vm.CrispAsrModels, vm, nameof(vm.SelectedCrispAsrModel), nameof(vm.CrispAsrModelComboIsVisible)), Se.Language.General.Model, 220);
        crispAsrModelCombo.ItemTemplate = AutoTranslateCombos.CrispAsrModelItemTemplate();

        var llamaCppModelCombo = FunctionViewChrome.Compact(UiUtil.MakeComboBox(vm.LlamaCppModels, vm, nameof(vm.SelectedLlamaCppModel), nameof(vm.LlamaCppModelComboIsVisible)), Se.Language.General.Model, 220);
        llamaCppModelCombo.ItemTemplate = AutoTranslateCombos.LlamaCppModelItemTemplate();

        var panelModelControls = new DockPanel
        {
            Children =
            {
                buttonModel,
                new Panel { Children = { textBoxModel, crispAsrModelCombo, llamaCppModelCombo } },
            },
        };
        var rowModel = FunctionViewChrome.MakeRow(Se.Language.General.Model, panelModelControls);
        rowModel.Bind(Visual.IsVisibleProperty, new MultiBinding
        {
            Converter = BoolConverters.Or,
            Bindings =
            {
                new Binding(nameof(vm.AutoTranslateModelIsVisible)),
                new Binding(nameof(vm.CrispAsrModelComboIsVisible)),
                new Binding(nameof(vm.LlamaCppModelComboIsVisible)),
            },
        });

        // Installed backend, pinned release and install state of llama.cpp itself. It sits directly
        // after the engine combo because it configures the engine, not the model or the prompt.
        var buttonEngineSettings = EditorChrome.CreateToolButton("fa-solid fa-gear", Se.Language.General.LlamaCppEngineSettings, vm.ShowLlamaCppEngineSettingsCommand, 28);
        buttonEngineSettings.Bind(Button.IsVisibleProperty, new Binding(nameof(vm.LlamaCppEngineSettingsButtonIsVisible)));

        var buttonAdvanced = UiUtil.MakeButton(Se.Language.General.AdvancedDotDotDot, vm.ShowLlamaCppAdvancedSettingsCommand)
            .WithAccessibleName(Se.Language.General.AdvancedSettings);
        buttonAdvanced.Bind(Button.IsVisibleProperty, new Binding(nameof(vm.LlamaCppAdvancedButtonIsVisible)));
        if (Se.Settings.Appearance.ShowHints)
        {
            ToolTip.SetTip(buttonAdvanced, Se.Language.General.AdvancedSettings);
        }

        // Prompt and the shared request settings for the regular llama.cpp engine (the advanced
        // engine has its own window instead). Right-aligned and away from the engine gear, the same
        // way the Auto-translate window separates the two.
        var buttonSettings = EditorChrome.CreateToolButton("fa-solid fa-sliders", Se.Language.General.Settings, vm.ShowLlamaCppTranslateSettingsCommand, 28);
        buttonSettings.HorizontalAlignment = HorizontalAlignment.Right;
        buttonSettings.VerticalAlignment = VerticalAlignment.Center;
        buttonSettings.Bind(Button.IsVisibleProperty, new Binding(nameof(vm.LlamaCppSettingsButtonIsVisible)));

        var panelEngineControls = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 6,
            Children = { buttonEngineSettings, buttonAdvanced },
        };

        var panelEngineRow = new Grid
        {
            ColumnSpacing = 6,
            ColumnDefinitions = new ColumnDefinitions("Auto,Auto,*"),
        };
        panelEngineRow.Add(cbEngines, 0, 0);
        panelEngineRow.Add(panelEngineControls, 0, 1);
        panelEngineRow.Add(buttonSettings, 0, 2);

        // Batch convert's own local/external llama-server switch (#14005) - independent of the one
        // in the Auto-translate window. Shown only for the llama.cpp engines.
        var switchLlamaCppRemote = FunctionViewChrome.MakeRowWithControl(new Panel(),
            EditorChrome.MakeSwitchRow(nameof(vm.LlamaCppUseRemoteServer), Se.Language.General.LlamaCppUseRemoteServer));
        switchLlamaCppRemote.Bind(Visual.IsVisibleProperty, new Binding(nameof(vm.LlamaCppRemoteToggleIsVisible)));

        var sourceLangCombo = FunctionViewChrome.Compact(UiUtil.MakeComboBox(vm.SourceLanguages, vm, nameof(vm.SelectedSourceLanguage)), Se.Language.General.From);

        var targetLangCombo = FunctionViewChrome.Compact(UiUtil.MakeComboBox(vm.TargetLanguages, vm, nameof(vm.SelectedTargetLanguage)), Se.Language.General.To);

        // More "To" languages, one combo box each - every language gives its own output file
        // ("movie.da.srt", "movie.sv.srt")
        var extraTargetLanguages = new ItemsControl
        {
            DataContext = vm,
            ItemsPanel = new FuncTemplate<Panel?>(() => new WrapPanel { Orientation = Orientation.Horizontal }),
            ItemTemplate = new FuncDataTemplate<ExtraTargetLanguageItem>((_, _) =>
            {
                var combo = new ComboBox
                {
                    ItemsSource = vm.TargetLanguages,
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(6, 0, 0, 6),
                    MinWidth = 140,
                }.WithAccessibleName(Se.Language.General.To);
                combo.Bind(Avalonia.Controls.Primitives.SelectingItemsControl.SelectedItemProperty,
                    new Binding(nameof(ExtraTargetLanguageItem.SelectedLanguage)) { Mode = BindingMode.TwoWay });

                var buttonRemove = EditorChrome.CreateToolButton("fa-solid fa-xmark", Se.Language.General.Remove, vm.RemoveExtraTargetLanguageCommand, 28);
                buttonRemove.Bind(Button.CommandParameterProperty, new Binding("."));
                buttonRemove.Margin = new Thickness(2, 0, 0, 6);

                return new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    Children = { combo, buttonRemove },
                };
            }, true),
        };
        extraTargetLanguages.Bind(ItemsControl.ItemsSourceProperty, new Binding(nameof(vm.ExtraTargetLanguages)));

        targetLangCombo.Margin = new Thickness(0, 0, 0, 6);
        var buttonAddTargetLanguage = EditorChrome.CreateToolButton("fa-solid fa-plus", Se.Language.General.Add, vm.AddExtraTargetLanguageCommand, 28);
        buttonAddTargetLanguage.Margin = new Thickness(4, 0, 0, 6);

        // The bottom margins keep wrapped rows apart; the negative margin takes the last one back
        var panelTargetLanguages = new WrapPanel
        {
            Orientation = Orientation.Horizontal,
            Margin = new Thickness(0, 0, 0, -6),
            Children = { targetLangCombo, extraTargetLanguages, buttonAddTargetLanguage },
        };

        var textBoxUrl = EditorChrome.MakeTextField(nameof(vm.AutoTranslateUrl), Se.Language.General.Url);

        // Masked API key with its reveal button; the text box takes the width the button leaves
        var apiKeyPanel = UiUtil.MakeApiKeyTextBox(double.NaN, vm, nameof(vm.AutoTranslateApiKey));
        var textBoxApiKey = (TextBox)apiKeyPanel.Children[0];
        var buttonReveal = (Button)apiKeyPanel.Children[1];
        apiKeyPanel.Children.Clear();
        textBoxApiKey.Classes.Add("text");
        textBoxApiKey.HorizontalAlignment = HorizontalAlignment.Stretch;
        buttonReveal.Margin = new Thickness(6, 0, 0, 0);
        DockPanel.SetDock(buttonReveal, Dock.Right);
        var panelApiKey = new DockPanel { Children = { buttonReveal, textBoxApiKey } };

        return FunctionViewChrome.MakePanel(vm, Se.Language.General.AutoTranslate,
            FunctionViewChrome.MakeRow(Se.Language.General.Engine, panelEngineRow),
            switchLlamaCppRemote,
            rowModel,
            FunctionViewChrome.MakeRow(Se.Language.General.From, sourceLangCombo),
            FunctionViewChrome.MakeRow(Se.Language.General.To, panelTargetLanguages),
            FunctionViewChrome.MakeRow(Se.Language.General.Url, textBoxUrl, nameof(vm.AutoTranslateUrlIsVisible)),
            FunctionViewChrome.MakeRow(Se.Language.General.ApiKey, panelApiKey, nameof(vm.AutoTranslateApiKeyIsVisible)));
    }
}
