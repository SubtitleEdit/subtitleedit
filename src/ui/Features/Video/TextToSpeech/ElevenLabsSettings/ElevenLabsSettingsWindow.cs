using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.ValueConverters;

namespace Nikse.SubtitleEdit.Features.Video.TextToSpeech.ElevenLabsSettings;

/// <summary>
/// ElevenLabs voice settings, in the editor look (see <see cref="EditorChrome"/>): one section with
/// a slider row per setting (value + help), more info and reset as icon buttons in its header,
/// Cancel/OK in the button bar.
/// </summary>
public class ElevenLabsSettingsWindow : Window
{
    private const double LabelWidth = 130;

    private readonly ElevenLabsSettingsViewModel _vm;

    public ElevenLabsSettingsWindow(ElevenLabsSettingsViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.Video.TextToSpeech.ElevenLabsSettings;
        SizeToContent = SizeToContent.WidthAndHeight;
        CanResize = false;
        Background = EditorChrome.WindowBrush;
        EditorChrome.AddEditorStyles(Styles);

        _vm = vm;
        vm.Window = this;
        DataContext = vm;

        var l = Se.Language.Video.TextToSpeech;
        var sliderStability = MakeSlider(0, 1, vm.Stability, nameof(ElevenLabsSettingsViewModel.Stability), l.Stability);
        var sliderSimilarity = MakeSlider(0, 1, vm.Similarity, nameof(ElevenLabsSettingsViewModel.Similarity), l.Similarity);
        var sliderSpeakerBoost = MakeSlider(0, 100, vm.SpeakerBoost, nameof(ElevenLabsSettingsViewModel.SpeakerBoost), l.SpeakerBoost);
        var sliderSpeed = MakeSlider(0.7, 1.2, vm.Speed, nameof(ElevenLabsSettingsViewModel.Speed), Se.Language.General.Speed);
        var sliderStyleExaggeration = MakeSlider(0.0, 1.0, vm.StyleExaggeration, nameof(ElevenLabsSettingsViewModel.StyleExaggeration), Se.Language.General.StyleExaggeration);

        var rows = new StackPanel { Spacing = 6 };
        rows.Children.Add(SliderRow(l.Stability, sliderStability, nameof(ElevenLabsSettingsViewModel.Stability), vm.ShowStabilityHelpCommand));
        rows.Children.Add(SliderRow(l.Similarity, sliderSimilarity, nameof(ElevenLabsSettingsViewModel.Similarity), vm.ShowSimilarityHelpCommand));
        rows.Children.Add(SliderRow(l.SpeakerBoost, sliderSpeakerBoost, nameof(ElevenLabsSettingsViewModel.SpeakerBoost), vm.ShowSpeakerBoostHelpCommand));
        rows.Children.Add(SliderRow(Se.Language.General.Speed, sliderSpeed, nameof(ElevenLabsSettingsViewModel.Speed), vm.ShowSpeedHelpCommand));
        rows.Children.Add(SliderRow(Se.Language.General.StyleExaggeration, sliderStyleExaggeration, nameof(ElevenLabsSettingsViewModel.StyleExaggeration), vm.ShowStyleExaggerationHelpCommand));

        var headerActions = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 2 };
        headerActions.Children.Add(EditorChrome.CreateToolButton(IconNames.Web, Se.Language.General.MoreInfo, vm.ShowMoreOnWebCommand, 26));
        headerActions.Children.Add(EditorChrome.CreateToolButton(IconNames.Repeat, Se.Language.General.Reset, vm.ResetCommand, 26));

        var sections = new StackPanel { Background = EditorChrome.PanelBrush };
        sections.Children.Add(EditorChrome.MakeEditorSection(Se.Language.General.Voice, rows, headerActions, first: true));

        var root = new DockPanel();
        var buttonBar = MakeButtonBar(vm);
        DockPanel.SetDock(buttonBar, Dock.Bottom);
        root.Children.Add(buttonBar);
        root.Children.Add(sections);
        Content = root;

        UiUtil.FocusOnFirstActivation(this, sliderStability); // initial focus on an input, not an action button - a focused button clicks on bare Space
    }

    private static Slider MakeSlider(double minimum, double maximum, double value, string property, string name)
    {
        var slider = new Slider
        {
            Minimum = minimum,
            Maximum = maximum,
            Value = value,
            Width = 220,
            VerticalAlignment = VerticalAlignment.Center,
            [!Slider.ValueProperty] = new Binding(property),
        };
        AutomationProperties.SetName(slider, name);
        return slider;
    }

    /// <summary>
    /// "Label  [slider]  value  (?)" - the label of fixed width so the sliders line up.
    /// </summary>
    private static Grid SliderRow(string label, Slider slider, string property, System.Windows.Input.ICommand helpCommand)
    {
        var text = EditorChrome.MakeDimLabel(label, new Thickness(0));
        text.TextTrimming = TextTrimming.CharacterEllipsis;
        var value = new TextBlock
        {
            FontSize = EditorChrome.FieldFontSize,
            MinWidth = 40,
            TextAlignment = TextAlignment.Right,
            VerticalAlignment = VerticalAlignment.Center,
            [!TextBlock.TextProperty] = new Binding(property) { Converter = new DoubleToTwoDecimalConverter() },
        };
        var help = EditorChrome.CreateToolButton(IconNames.Help, $"{label} - {Se.Language.General.Help}", helpCommand, 26);

        var grid = new Grid
        {
            ColumnSpacing = 10,
            ColumnDefinitions =
            {
                new ColumnDefinition(new GridLength(LabelWidth)),
                new ColumnDefinition(GridLength.Auto),
                new ColumnDefinition(GridLength.Auto),
                new ColumnDefinition(GridLength.Auto),
            },
        };
        grid.Add(text, 0, 0);
        grid.Add(slider, 0, 1);
        grid.Add(value, 0, 2);
        grid.Add(help, 0, 3);
        return grid;
    }

    private static Border MakeButtonBar(ElevenLabsSettingsViewModel vm)
    {
        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        buttonOk.Classes.Add("primary");
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);

        return EditorChrome.WrapButtonBar(new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Right,
            Spacing = 10,
            Children = { buttonCancel, buttonOk },
        });
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        _vm.OnKeyDown(e);
    }
}
