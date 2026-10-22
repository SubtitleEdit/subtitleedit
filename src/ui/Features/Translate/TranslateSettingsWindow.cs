using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Layout;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Translate;

/// <summary>
/// Auto-translate settings in the editor look (see <see cref="EditorChrome"/>): line merge, delay,
/// request size and music lines in a section, the engine's prompt (if it has one) under it.
/// </summary>
public class TranslateSettingsWindow : Window
{
    private const double LabelWidth = 250;

    private readonly TranslateSettingsViewModel _vm;

    public TranslateSettingsWindow(TranslateSettingsViewModel vm)
    {
        _vm = vm;
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.General.Settings;
        DataContext = vm;
        vm.Window = this;

        if (vm.PromptIsVisible)
        {
            Width = 750;
            MinWidth = 600;
            Height = 440;
            MinHeight = 380;
        }
        else
        {
            // No prompt: just the settings, sized to them
            Width = MinWidth = 420;
            SizeToContent = SizeToContent.Height;
            CanResize = false;
        }

        EditorChrome.ApplyWindow(this, EditorWindowStyles.Dialog);

        var comboMerge = UiUtil.MakeComboBox(vm.MergeOptions, vm, nameof(vm.SelectedMergeOptions))
            .WithAccessibleName(Se.Language.Translate.LineMerge);
        comboMerge.HorizontalAlignment = HorizontalAlignment.Stretch;
        comboMerge.IsEnabled = vm.MergeIsVisible;

        var delayNumericUpDown = new NumericUpDown
        {
            Minimum = 0,
            Maximum = 60,
            Value = vm.ServerDelaySeconds,
            Width = 120,
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Left,
            Increment = 1,
            FormatString = "#,###,##0",
        };
        delayNumericUpDown.Classes.Add("field");
        delayNumericUpDown.Bind(NumericUpDown.ValueProperty, new Binding
        {
            Path = nameof(vm.ServerDelaySeconds),
            Mode = BindingMode.TwoWay,
            Source = vm,
        });
        AutomationProperties.SetName(delayNumericUpDown, Se.Language.Translate.DelayInSecondsBetweenRequests);

        var maxBytesNumericUpDown = UiUtil.MakeNumericUpDownInt(0, 100_000, 1000, 120, vm, nameof(vm.MaxBytesRequest));
        maxBytesNumericUpDown.Increment = 100;
        maxBytesNumericUpDown.FormatString = "#,###,##0";
        maxBytesNumericUpDown.HorizontalAlignment = HorizontalAlignment.Left;
        maxBytesNumericUpDown.Classes.Add("field");
        AutomationProperties.SetName(maxBytesNumericUpDown, Se.Language.Translate.MaxBytesPerRequest);

        var switchKeepMusicLines = EditorChrome.MakeSwitch(nameof(vm.KeepMusicLinesUntranslated), Se.Language.Translate.KeepMusicLinesUntranslated);

        var settingsRows = new StackPanel
        {
            Spacing = 10,
            Children =
            {
                EditorChrome.MakeLabeledRow(Se.Language.Translate.LineMerge, comboMerge, LabelWidth, spacing: 6),
                EditorChrome.MakeLabeledRow(Se.Language.Translate.DelayInSecondsBetweenRequests, delayNumericUpDown, LabelWidth, spacing: 6),
                EditorChrome.MakeLabeledRow(Se.Language.Translate.MaxBytesPerRequest, maxBytesNumericUpDown, LabelWidth, spacing: 6),
                EditorChrome.MakeSwitchRow(switchKeepMusicLines, Se.Language.Translate.KeepMusicLinesUntranslated),
            },
        };

        var buttonResetPrompt = EditorChrome.CreateToolButton(IconNames.Restore, Se.Language.Translate.ResetPromptToDefault, vm.ResetPromptCommand, 26);

        var promptTextBox = new TextBox
        {
            AcceptsReturn = true,
            // A tab is not meaningful in a translation prompt, and accepting it trapped the
            // keyboard in this box - Tab could not reach the buttons below (#14313).
            AcceptsTab = false,
            VerticalAlignment = VerticalAlignment.Stretch,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalContentAlignment = VerticalAlignment.Top,
            Width = double.NaN,
            Height = double.NaN,
            MinHeight = 80,
            TextWrapping = Avalonia.Media.TextWrapping.Wrap,
        }.BindText(vm, nameof(vm.PromptText));
        promptTextBox.Classes.Add("text");
        AutomationProperties.SetName(promptTextBox, Se.Language.Translate.PromptText);

        // Prompt: header with the reset button, the text box takes the rest of the height
        var promptHeader = new DockPanel();
        DockPanel.SetDock(buttonResetPrompt, Dock.Right);
        promptHeader.Children.Add(buttonResetPrompt);
        promptHeader.Children.Add(EditorChrome.MakeSectionHeader(Se.Language.Translate.PromptText));
        var promptGrid = new Grid
        {
            RowDefinitions = new RowDefinitions("Auto,*"),
            RowSpacing = 8,
            Margin = new Thickness(14, 10, 14, 14),
        };
        promptGrid.Add(promptHeader, 0, 0);
        promptGrid.Add(promptTextBox, 1, 0);
        var promptSection = new Border
        {
            Child = promptGrid,
            BorderBrush = EditorChrome.LineBrush,
            BorderThickness = new Thickness(0, 1, 0, 0),
        }.BindIsVisible(vm, nameof(vm.PromptIsVisible));

        var sections = new Grid
        {
            RowDefinitions = new RowDefinitions("Auto,*"),
        };
        sections.Add(EditorChrome.MakeEditorSection(Se.Language.General.Settings, settingsRows, first: true), 0, 0);
        sections.Add(promptSection, 1, 0);

        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        buttonOk.Classes.Add("primary");
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);
        var buttonBar = EditorChrome.MakeButtonBar(null, buttonCancel, buttonOk);

        Content = EditorChrome.MakeRoot(sections, buttonBar);
        if (!vm.PromptIsVisible)
        {
            // Sized to the settings, so the panel row must not ask for the rest of the height
            ((Grid)Content).RowDefinitions[0].Height = GridLength.Auto;
        }

        UiUtil.FocusOnFirstActivation(this, comboMerge); // initial focus on an input, not an action button - a focused button clicks on bare Space
        Loaded += vm.Onloaded;
        Closing += vm.OnClosing;
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        _vm.OnKeyDown(e);
    }
}