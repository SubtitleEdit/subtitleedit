using System.Collections.ObjectModel;
using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
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
//   │   (o) Whisper                                          │
//   ├────────────────────────────────────────────────────────┤
//   │                                       [Cancel] [OK]    │
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
        EditorChrome.ApplyWindow(this, EditorWindowStyles.TextFieldsUncentered | EditorWindowStyles.MuteDisabledPrimary);

        _vm = vm;
        vm.Window = this;
        DataContext = vm;

        var header = BuildHeader(vm);
        var instructionBlock = BuildInstructionBlock(vm);
        var omniBlock = BuildOmniVoicePicker(vm);

        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        buttonOk.Classes.Add("primary");
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);
        var buttons = EditorChrome.MakeFlushButtonBar(null, buttonCancel, buttonOk);

        var sections = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Children = { instructionBlock, omniBlock },
        };

        // Sections scroll on a short screen, so the buttons stay reachable.
        var root = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition(GridLength.Auto),
                new RowDefinition(new GridLength(1, GridUnitType.Star)),
                new RowDefinition(GridLength.Auto),
            },
        };
        root.Add(header, 0, 0);
        root.Add(new ScrollViewer
        {
            Content = new Border { Background = EditorChrome.PanelBrush, Child = sections },
            VerticalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto,
            HorizontalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Disabled,
        }, 1, 0);
        root.Add(buttons, 2, 0);

        Content = root;

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
            FontSize = 12,
            Foreground = EditorChrome.DimTextBrush,
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

        return new Border
        {
            Background = EditorChrome.PanelBrush,
            BorderBrush = EditorChrome.LineBrush,
            BorderThickness = new Thickness(0, 0, 0, 1),
            Padding = new Thickness(16, 12),
            Child = new StackPanel { Orientation = Orientation.Vertical, Spacing = 2, Children = { title, sub } },
        };
    }

    private static Border BuildInstructionBlock(ActorVoiceRowSettingsViewModel vm)
    {
        var textBox = new TextBox
        {
            AcceptsReturn = true,
            TextWrapping = TextWrapping.Wrap,
            Height = 90,
            Width = double.NaN,
            MinWidth = 420,
            FontSize = 12.5,
            Background = EditorChrome.FieldBrush,
            BorderBrush = EditorChrome.FieldBorderBrush,
            CornerRadius = new CornerRadius(6),
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalContentAlignment = VerticalAlignment.Top,
            PlaceholderText = Se.Language.Video.TextToSpeech.VoiceInstructionHint,
            [!TextBox.TextProperty] = new Binding(nameof(vm.Instruction))
            {
                Mode = BindingMode.TwoWay,
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
            },
        };
        AutomationProperties.SetName(textBox, Se.Language.Video.TextToSpeech.VoiceInstruction);

        var hint = EditorChrome.MakeHint(Se.Language.Video.TextToSpeech.VoiceInstructionFreeTextHint);
        hint.MaxWidth = 430;
        hint.HorizontalAlignment = HorizontalAlignment.Left;

        var inner = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 6,
            Children = { textBox, hint },
        };

        var border = EditorChrome.MakeEditorSection(Se.Language.Video.TextToSpeech.VoiceInstruction, inner, first: true);
        border.Bind(Visual.IsVisibleProperty, new Binding(nameof(vm.IsInstructionTextVisible)) { Mode = BindingMode.OneWay });
        return border;
    }

    private static Border BuildOmniVoicePicker(ActorVoiceRowSettingsViewModel vm)
    {
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

        // Use the engine's own constant — same label as the main TTS window and Review window.
        var whisper = EditorChrome.MakeToggleRow(nameof(vm.OmniVoiceWhisper), OmniVoiceTtsCpp.InstructionWhisper);
        whisper.Margin = new Thickness(0, 4, 0, 0);
        grid.Add(whisper, 4, 0, 1, 2);

        var clonedNote = new TextBlock
        {
            Text = Se.Language.Video.TextToSpeech.VoiceInstructionClonedVoiceNote,
            TextWrapping = TextWrapping.Wrap,
            FontSize = 11.5,
            Foreground = EditorChrome.DimTextBrush,
            MaxWidth = 430,
            HorizontalAlignment = HorizontalAlignment.Left,
            Margin = new Thickness(0, 6, 0, 0),
            [!Visual.IsVisibleProperty] = new Binding(nameof(vm.IsClonedVoiceNoteVisible)) { Mode = BindingMode.OneWay },
        };

        var inner = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 6,
            Children = { grid, clonedNote },
        };

        // A top line only when the instruction section is shown above it.
        var border = EditorChrome.MakeEditorSection(Se.Language.Video.TextToSpeech.VoiceDesign, inner);
        border.Bind(Border.BorderThicknessProperty, new Binding(nameof(vm.IsInstructionTextVisible))
        {
            Mode = BindingMode.OneWay,
            Converter = new Avalonia.Data.Converters.FuncValueConverter<bool, Thickness>(v => new Thickness(0, v ? 1 : 0, 0, 0)),
        });
        border.Bind(Visual.IsVisibleProperty, new Binding(nameof(vm.IsOmniVoicePickerVisible)) { Mode = BindingMode.OneWay });
        return border;
    }

    private static void AddPickerRow(Grid grid, int row, string label, object vm,
        ObservableCollection<string> items, string selectedPropertyPath)
    {
        grid.Add(EditorChrome.MakeDimLabel(label, new Thickness(0)).WithMinWidth(70), row, 0);
        var combo = UiUtil.MakeComboBox(items, vm, selectedPropertyPath).WithWidth(200);
        AutomationProperties.SetName(combo, label);
        grid.Add(combo, row, 1);
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
