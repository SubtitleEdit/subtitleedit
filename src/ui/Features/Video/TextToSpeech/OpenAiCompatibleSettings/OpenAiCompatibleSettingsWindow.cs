using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Styling;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Video.TextToSpeech.OpenAiCompatibleSettings;

/// <summary>
/// OpenAI-compatible TTS server settings, in the editor look (see <see cref="EditorChrome"/>):
/// a server section (url, models, voices) and a voice section (instructions, speed, format),
/// more info and reset as icon buttons in the first header, Cancel/OK in the button bar.
/// </summary>
public class OpenAiCompatibleSettingsWindow : Window
{
    private const double LabelWidth = 120;
    private const double InputWidth = 380;

    private readonly OpenAiCompatibleSettingsViewModel _vm;

    public OpenAiCompatibleSettingsWindow(OpenAiCompatibleSettingsViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.Video.TextToSpeech.OpenAiCompatibleSettings;
        SizeToContent = SizeToContent.WidthAndHeight;
        CanResize = false;
        Background = EditorChrome.WindowBrush;
        EditorChrome.AddEditorStyles(Styles);
        AddStyles();

        _vm = vm;
        vm.Window = this;
        DataContext = vm;

        var l = Se.Language.Video.TextToSpeech;

        var textBoxUrl = MakeTextBox(UiUtil.MakeTextBox(InputWidth, vm, nameof(vm.CustomUrl)), l.CustomServerUrl);
        var textBoxModels = MakeTextBox(UiUtil.MakeTextBox(InputWidth, vm, nameof(vm.CustomModels)), l.CustomModels);
        var textBoxVoices = MakeTextBox(UiUtil.MakeTextBox(InputWidth, vm, nameof(vm.CustomVoices)), l.CustomVoices);
        var textBoxInstructions = MakeTextBox(UiUtil.MakeTextBox(InputWidth, vm, nameof(vm.Instructions)), l.Instructions);
        textBoxInstructions.AcceptsReturn = true;
        textBoxInstructions.TextWrapping = TextWrapping.Wrap;
        textBoxInstructions.Height = 70;
        textBoxInstructions.VerticalContentAlignment = VerticalAlignment.Top;
        textBoxInstructions.Padding = new Thickness(8, 6);
        var numericSpeed = UiUtil.MakeNumericUpDownTwoDecimals(0.25m, 4.0m, 120, vm, nameof(vm.Speed), defaultValue: 1.0m);
        numericSpeed.Classes.Add("field");
        numericSpeed.HorizontalAlignment = HorizontalAlignment.Left;
        AutomationProperties.SetName(numericSpeed, Se.Language.General.Speed);
        var comboBoxResponseFormat = UiUtil.MakeComboBox(vm.ResponseFormats, vm, nameof(vm.SelectedResponseFormat));
        comboBoxResponseFormat.MinWidth = 120;
        comboBoxResponseFormat.HorizontalAlignment = HorizontalAlignment.Left;
        AutomationProperties.SetName(comboBoxResponseFormat, Se.Language.General.OutputFormat);

        var server = new StackPanel { Spacing = 8 };
        server.Children.Add(Row(l.CustomServerUrl, textBoxUrl, l.CustomServerUrlHint));
        server.Children.Add(Row(l.CustomModels, textBoxModels, l.CustomModelsHint));
        server.Children.Add(Row(l.CustomVoices, textBoxVoices, l.CustomVoicesHint));

        var voice = new StackPanel { Spacing = 8 };
        voice.Children.Add(Row(l.Instructions, textBoxInstructions, l.InstructionsHint));
        voice.Children.Add(Row(Se.Language.General.Speed, numericSpeed, l.OpenAiSpeedHint));
        voice.Children.Add(Row(Se.Language.General.OutputFormat, comboBoxResponseFormat, l.OutputFormatHint));

        var headerActions = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 2 };
        headerActions.Children.Add(EditorChrome.CreateToolButton(IconNames.Web, Se.Language.General.MoreInfo, vm.ShowMoreOnWebCommand, 26));
        headerActions.Children.Add(EditorChrome.CreateToolButton(IconNames.Repeat, Se.Language.General.Reset, vm.ResetCommand, 26));

        var sections = new StackPanel { Background = EditorChrome.PanelBrush };
        sections.Children.Add(EditorChrome.MakeEditorSection(Se.Language.General.Settings, server, headerActions, first: true));
        sections.Children.Add(EditorChrome.MakeEditorSection(Se.Language.General.Voice, voice));

        var root = new DockPanel();
        var buttonBar = MakeButtonBar(vm);
        DockPanel.SetDock(buttonBar, Dock.Bottom);
        root.Children.Add(buttonBar);
        root.Children.Add(sections);
        Content = root;

        UiUtil.FocusOnFirstActivation(this, textBoxUrl); // initial focus on an input, not an action button - a focused button clicks on bare Space
    }

    /// <summary>
    /// Compact combo boxes.
    /// </summary>
    private void AddStyles()
    {
        Styles.Add(new Style(x => x.OfType<ComboBox>())
        {
            Setters =
            {
                new Setter(TemplatedControl.FontSizeProperty, 12.5),
                new Setter(TemplatedControl.MinHeightProperty, 28.0),
                new Setter(TemplatedControl.PaddingProperty, new Thickness(8, 2, 4, 2)),
                new Setter(TemplatedControl.CornerRadiusProperty, new CornerRadius(6)),
            },
        });
    }

    private static TextBox MakeTextBox(TextBox textBox, string name)
    {
        textBox.FontSize = 12.5;
        textBox.MinHeight = 28;
        textBox.Background = EditorChrome.FieldBrush;
        textBox.BorderBrush = EditorChrome.FieldBorderBrush;
        textBox.CornerRadius = new CornerRadius(6);
        textBox.VerticalContentAlignment = VerticalAlignment.Center;
        AutomationProperties.SetName(textBox, name);
        return textBox;
    }

    /// <summary>
    /// "Label  [input]  (i)" - the label of fixed width so the inputs line up; a multi-line input
    /// gets its label and hint at the top.
    /// </summary>
    private static Grid Row(string label, Control input, string hint)
    {
        var alignment = input is TextBox { AcceptsReturn: true } ? VerticalAlignment.Top : VerticalAlignment.Center;
        var text = EditorChrome.MakeDimLabel(label, new Thickness(0, alignment == VerticalAlignment.Top ? 6 : 0, 0, 0));
        text.VerticalAlignment = alignment;
        text.TextTrimming = TextTrimming.CharacterEllipsis;
        var hintIcon = UiUtil.MakeHintIcon(hint, input);
        hintIcon.VerticalAlignment = alignment;
        if (alignment == VerticalAlignment.Top)
        {
            hintIcon.Margin = new Thickness(0, 6, 0, 0);
        }

        var grid = new Grid
        {
            ColumnSpacing = 8,
            ColumnDefinitions =
            {
                new ColumnDefinition(new GridLength(LabelWidth)),
                new ColumnDefinition(new GridLength(InputWidth)),
                new ColumnDefinition(GridLength.Auto),
            },
        };
        grid.Add(text, 0, 0);
        grid.Add(input, 0, 1);
        grid.Add(hintIcon, 0, 2);
        return grid;
    }

    private static Border MakeButtonBar(OpenAiCompatibleSettingsViewModel vm)
    {
        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        buttonOk.Classes.Add("primary");
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);

        return new Border
        {
            Child = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right,
                Spacing = 10,
                Children = { buttonCancel, buttonOk },
            },
            Background = EditorChrome.ButtonBarBrush,
            BorderBrush = EditorChrome.LineBrush,
            BorderThickness = new Thickness(0, 1, 0, 0),
            Padding = new Thickness(16, 10),
        };
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        _vm.OnKeyDown(e);
    }
}
