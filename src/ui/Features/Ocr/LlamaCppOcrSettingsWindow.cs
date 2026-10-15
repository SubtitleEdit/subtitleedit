using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.Media;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Chrome = Nikse.SubtitleEdit.Features.Ocr.OcrDialogChrome;

namespace Nikse.SubtitleEdit.Features.Ocr;

public class LlamaCppOcrSettingsWindow : Window
{
    private const int LabelWidth = 165;

    public LlamaCppOcrSettingsWindow(LlamaCppOcrSettingsViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.Ocr.LlamaCppOcrSettingsTitle;
        Width = 680;
        Height = 500;
        MinWidth = 540;
        MinHeight = 440;
        CanResize = true;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        vm.Window = this;
        DataContext = vm;
        Chrome.Apply(this);

        var textBoxUrl = new TextBox
        {
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Center,
            DataContext = vm,
        };
        textBoxUrl.Classes.Add(Chrome.TextClass);
        AutomationProperties.SetName(textBoxUrl, Se.Language.General.Url);
        textBoxUrl.Bind(IsEnabledProperty, new Binding(nameof(vm.UseRemoteServer)));
        textBoxUrl.Bind(TextBox.TextProperty, new Binding(nameof(vm.Url))
        {
            Mode = BindingMode.TwoWay,
            UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
        });

        // Off: SE downloads and runs its own llama-server, so the URL is not used. On: OCR posts
        // to the user's own running server at the URL (#15854).
        var switchRemote = Chrome.Switch(nameof(vm.UseRemoteServer), Se.Language.General.LlamaCppUseRemoteServer);

        var numericTimeout = UiUtil.MakeNumericUpDownInt(1, 120, 5, 140, vm, nameof(vm.TimeoutMinutes));
        numericTimeout.Classes.Add("field");
        numericTimeout.HorizontalAlignment = HorizontalAlignment.Left;
        AutomationProperties.SetName(numericTimeout, Se.Language.Ocr.LlamaCppOcrTimeoutMinutes);

        var textBoxPrompt = new TextBox
        {
            AcceptsReturn = true,
            AcceptsTab = false,
            TextWrapping = TextWrapping.Wrap,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
            VerticalContentAlignment = VerticalAlignment.Top,
            MinHeight = 90,
            DataContext = vm,
        };
        textBoxPrompt.Classes.Add(Chrome.TextClass);
        AutomationProperties.SetName(textBoxPrompt, Se.Language.General.OpenAiCompatibleSttPrompt);
        textBoxPrompt.Bind(TextBox.TextProperty, new Binding(nameof(vm.Prompt))
        {
            Mode = BindingMode.TwoWay,
            UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
        });

        var hintPrompt = Chrome.Hint(Se.Language.Ocr.LlamaCppOcrPromptHint);

        // Engine: status, own server or remote one, timeout
        var serverRows = new StackPanel
        {
            Spacing = 10,
            Children =
            {
                Chrome.Row(Se.Language.General.Status, Chrome.StatusPanel(nameof(vm.EngineBrush), nameof(vm.EngineLabel)), LabelWidth),
                Chrome.Row(string.Empty, Chrome.SwitchRow(switchRemote, Se.Language.General.LlamaCppUseRemoteServer), LabelWidth),
                Chrome.Row(Se.Language.General.Url, textBoxUrl, LabelWidth),
                Chrome.Row(Se.Language.Ocr.LlamaCppOcrTimeoutMinutes, numericTimeout, LabelWidth),
            },
        };

        // Prompt: the text box takes the rest of the height
        var promptGrid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition(GridLength.Auto),
                new RowDefinition(new GridLength(1, GridUnitType.Star)),
                new RowDefinition(GridLength.Auto),
            },
            RowSpacing = 8,
            Margin = new Thickness(14, 12, 14, 14),
        };
        promptGrid.Add(EditorChrome.MakeSectionHeader(Se.Language.General.OpenAiCompatibleSttPrompt), 0, 0);
        promptGrid.Add(textBoxPrompt, 1, 0);
        promptGrid.Add(hintPrompt, 2, 0);

        var sections = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition(GridLength.Auto),
                new RowDefinition(GridLength.Auto),
                new RowDefinition(new GridLength(1, GridUnitType.Star)),
            },
        };
        sections.Add(Chrome.MakeHeader(Se.Language.Ocr.LlamaCppOcr, Se.Language.Ocr.LlamaCppOcrDescription), 0, 0);
        sections.Add(EditorChrome.MakeEditorSection(Se.Language.General.Engine, serverRows), 1, 0);
        sections.Add(new Border
        {
            Child = promptGrid,
            BorderBrush = EditorChrome.LineBrush,
            BorderThickness = new Thickness(0, 1, 0, 0),
        }, 2, 0);

        var buttonRedownload = Chrome.Flat(UiUtil.MakeButton(string.Empty, vm.RedownloadCommand)
            .WithIconLeftBindText(IconNames.Download, nameof(vm.DownloadButtonLabel)));

        var buttonOk = Chrome.MakePrimary(UiUtil.MakeButtonOk(vm.OkCommand));
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);
        var buttonBar = Chrome.ButtonBar(buttonRedownload, buttonCancel, buttonOk);
        buttonRedownload.HorizontalAlignment = HorizontalAlignment.Left;

        Content = Chrome.Root(sections, buttonBar, stretch: true);

        UiUtil.FocusOnFirstActivation(this, switchRemote);
        KeyDown += (_, e) => vm.OnKeyDown(e);
    }
}
