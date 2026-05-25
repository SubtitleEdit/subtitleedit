using System.Collections.ObjectModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Nikse.SubtitleEdit.Features.Video.TextToSpeech.Engines;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Video.TextToSpeech.ActorVoices;

// Per-row advanced settings dialog for one cast row.
//
//   ┌────────────────────────────────────────────────────────┐
//   │ <icon> Voice settings for "Joe"                        │
//   │        ElevenLabs / Aria                               │
//   ├────────────────────────────────────────────────────────┤
//   │ Voice instruction                                      │
//   │  ┌────────────────────────────────────────────────────┐ │
//   │  │ Speak in a calm and friendly tone...              │ │
//   │  └────────────────────────────────────────────────────┘ │
//   │                                                        │
//   │ Voice design  (OmniVoice only)                         │
//   │   Gender [Female ▾]  Age [Adult ▾]                     │
//   │   Pitch  [Low    ▾]  Accent [British ▾]                │
//   │   ☐ Whisper                                            │
//   ├────────────────────────────────────────────────────────┤
//   │                                       [OK] [Cancel]    │
//   └────────────────────────────────────────────────────────┘
public class ActorVoiceRowSettingsWindow : Window
{
    private readonly ActorVoiceRowSettingsViewModel _vm;

    public ActorVoiceRowSettingsWindow(ActorVoiceRowSettingsViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.Video.TextToSpeech.ActorVoicesRowSettingsTitle;
        SizeToContent = SizeToContent.WidthAndHeight;
        CanResize = false;
        MinWidth = 460;

        _vm = vm;
        vm.Window = this;
        DataContext = vm;

        var header = BuildHeader(vm);
        var instructionBlock = BuildInstructionBlock(vm);
        var omniBlock = BuildOmniVoicePicker(vm);

        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);
        var buttons = UiUtil.MakeButtonBar(buttonOk, buttonCancel);

        var stack = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 12,
            Margin = UiUtil.MakeWindowMargin(),
            Children = { header, instructionBlock, omniBlock, buttons },
        };

        Content = stack;

        Loaded += (_, _) => UiUtil.RestoreWindowPosition(this);
    }

    private static Control BuildHeader(ActorVoiceRowSettingsViewModel vm)
    {
        var title = new TextBlock
        {
            FontSize = 15,
            FontWeight = FontWeight.SemiBold,
            [!TextBlock.TextProperty] = new Binding(nameof(vm.Actor))
            {
                Mode = BindingMode.OneWay,
                StringFormat = Se.Language.Video.TextToSpeech.VoiceSettingsForX,
            },
        };

        var sub = new TextBlock
        {
            FontSize = 11,
            Foreground = UiUtil.GetTextColor(0.6d),
            [!TextBlock.TextProperty] = new MultiBinding
            {
                StringFormat = "{0} / {1}",
                Bindings =
                {
                    new Binding(nameof(vm.EngineName)) { Mode = BindingMode.OneWay },
                    new Binding(nameof(vm.VoiceName)) { Mode = BindingMode.OneWay },
                },
            },
        };

        return new StackPanel { Orientation = Orientation.Vertical, Spacing = 2, Children = { title, sub } };
    }

    private static Border BuildInstructionBlock(ActorVoiceRowSettingsViewModel vm)
    {
        var label = new Label
        {
            Content = Se.Language.Video.TextToSpeech.VoiceInstruction,
            FontWeight = FontWeight.SemiBold,
        };

        var textBox = new TextBox
        {
            AcceptsReturn = true,
            TextWrapping = TextWrapping.Wrap,
            Height = 90,
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalContentAlignment = VerticalAlignment.Top,
            PlaceholderText = Se.Language.Video.TextToSpeech.VoiceInstructionHint,
            [!TextBox.TextProperty] = new Binding(nameof(vm.Instruction))
            {
                Mode = BindingMode.TwoWay,
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
            },
        };

        var hint = new TextBlock
        {
            Text = Se.Language.Video.TextToSpeech.VoiceInstructionFreeTextHint,
            FontSize = 11,
            Foreground = UiUtil.GetTextColor(0.55d),
            TextWrapping = TextWrapping.Wrap,
        };

        var inner = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 6,
            Children = { label, textBox, hint },
        };

        var border = UiUtil.MakeBorderForControl(inner);
        border.Bind(Visual.IsVisibleProperty, new Binding(nameof(vm.IsInstructionTextVisible)) { Mode = BindingMode.OneWay });
        return border;
    }

    private static Border BuildOmniVoicePicker(ActorVoiceRowSettingsViewModel vm)
    {
        var label = new Label
        {
            Content = Se.Language.Video.TextToSpeech.VoiceDesign,
            FontWeight = FontWeight.SemiBold,
        };

        var grid = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
            },
            ColumnSpacing = 10,
            RowSpacing = 6,
            [!InputElement.IsEnabledProperty] = new Binding(nameof(vm.IsOmniVoicePickerEnabled)) { Mode = BindingMode.OneWay },
        };

        AddPickerRow(grid, 0, Se.Language.Video.TextToSpeech.VoiceGender, vm, vm.OmniVoiceGenders, nameof(vm.SelectedOmniVoiceGender));
        AddPickerRow(grid, 1, Se.Language.Video.TextToSpeech.VoiceAge, vm, vm.OmniVoiceAges, nameof(vm.SelectedOmniVoiceAge));
        AddPickerRow(grid, 2, Se.Language.Video.TextToSpeech.VoicePitch, vm, vm.OmniVoicePitches, nameof(vm.SelectedOmniVoicePitch));
        AddPickerRow(grid, 3, Se.Language.Video.TextToSpeech.VoiceAccent, vm, vm.OmniVoiceAccents, nameof(vm.SelectedOmniVoiceAccent));

        var whisper = new CheckBox
        {
            // Use the engine's own constant — same label as the main TTS window and Review window.
            Content = OmniVoiceTtsCpp.InstructionWhisper,
            Margin = new Thickness(0, 4, 0, 0),
            [!CheckBox.IsCheckedProperty] = new Binding(nameof(vm.OmniVoiceWhisper)) { Mode = BindingMode.TwoWay },
        };
        grid.Add(whisper, 4, 0, 1, 2);

        var clonedNote = new TextBlock
        {
            Text = Se.Language.Video.TextToSpeech.VoiceInstructionClonedVoiceNote,
            TextWrapping = TextWrapping.Wrap,
            FontSize = 11,
            Foreground = UiUtil.GetTextColor(0.7d),
            Margin = new Thickness(0, 6, 0, 0),
            [!Visual.IsVisibleProperty] = new Binding(nameof(vm.IsClonedVoiceNoteVisible)) { Mode = BindingMode.OneWay },
        };

        var inner = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 6,
            Children = { label, grid, clonedNote },
        };

        var border = UiUtil.MakeBorderForControl(inner);
        border.Bind(Visual.IsVisibleProperty, new Binding(nameof(vm.IsOmniVoicePickerVisible)) { Mode = BindingMode.OneWay });
        return border;
    }

    private static void AddPickerRow(Grid grid, int row, string label, object vm,
        ObservableCollection<string> items, string selectedPropertyPath)
    {
        grid.Add(new Label
        {
            Content = label,
            VerticalAlignment = VerticalAlignment.Center,
            MinWidth = 60,
        }, row, 0);
        grid.Add(UiUtil.MakeComboBox(items, vm, selectedPropertyPath).WithWidth(200), row, 1);
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        _vm.OnKeyDown(e);
    }

    protected override void OnClosing(WindowClosingEventArgs e)
    {
        base.OnClosing(e);
        _vm.OnClosing(e);
    }
}
