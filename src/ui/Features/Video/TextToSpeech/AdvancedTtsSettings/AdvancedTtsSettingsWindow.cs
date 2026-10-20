using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Styling;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using System.IO;

namespace Nikse.SubtitleEdit.Features.Video.TextToSpeech.AdvancedTtsSettings;

/// <summary>
/// Every option used to carry its explanation as a paragraph of wrapped text underneath it, which
/// made the window several screens tall - on a laptop it was clamped to the working area and the
/// OK/Cancel buttons ended up off-screen (#14331). The explanations now live in hover hints, the
/// options are grouped, and the content scrolls with the buttons pinned below it.
/// In the editor look (see <see cref="EditorChrome"/>): sections on the panel color, switches for
/// the on/off options, compact fields, Cancel/OK in the button bar.
/// </summary>
public class AdvancedTtsSettingsWindow : Window
{
    private readonly AdvancedTtsSettingsViewModel _vm;

    public AdvancedTtsSettingsWindow(AdvancedTtsSettingsViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.Video.TextToSpeech.AdvancedTtsSettings;

        // Explicit width + height-only auto sizing: SizeToContent.WidthAndHeight renders far too
        // wide on macOS.
        Width = 560;
        SizeToContent = SizeToContent.Height;
        CanResize = false;
        Background = EditorChrome.WindowBrush;
        EditorChrome.AddEditorStyles(Styles);
        AddStyles();

        _vm = vm;
        vm.Window = this;
        DataContext = vm;

        var l = Se.Language.Video.TextToSpeech;

        var switchProAudio = MakeSwitch(l.ProAudioPostProcessing, nameof(vm.DoProAudioChain), out var toggleProAudio);
        var switchDucking = MakeSwitch(l.AudioDucking, nameof(vm.DoAudioDucking), out var toggleDucking);
        var switchRemoveSpeech = MakeSwitch(l.RemoveOriginalSpeech, nameof(vm.DoRemoveOriginalSpeech), out var toggleRemoveSpeech);
        var switchVad = MakeSwitch(l.VadSilenceCompression, nameof(vm.DoVadSilenceCompression), out var toggleVad);
        var switchTimeStretch = MakeSwitch(l.HighQualityTimeStretch, nameof(vm.DoHighQualityTimeStretch), out var toggleTimeStretch);
        var switchDeleteTempFiles = MakeSwitch(l.DeleteTempFiles, nameof(vm.DoDeleteTempFiles), out var toggleDeleteTempFiles);
        var switchCleanCloneReferences = MakeSwitch(l.CleanCloneReferences, nameof(vm.DoCleanCloneReferences), out var toggleCleanCloneReferences);

        var speechRemovalModel = new ComboBox
        {
            Width = 240,
            ItemsSource = vm.SpeechRemovalModels,
            [!SelectingItemsControl.SelectedIndexProperty] = new Binding(nameof(vm.SpeechRemovalModelIndex)) { Mode = BindingMode.TwoWay },
            [!IsEnabledProperty] = new Binding(nameof(vm.DoRemoveOriginalSpeech)) { Mode = BindingMode.OneWay },
        };

        var groupAudio = EditorChrome.MakeEditorSection(l.AdvancedTtsAudioProcessing, new StackPanel
        {
            Spacing = 8,
            Children =
            {
                MakeRow(switchProAudio, l.ProAudioPostProcessingDescription, describes: toggleProAudio),

                MakeRow(switchDucking, l.AudioDuckingDescription,
                    MakeField(l.OriginalVolumePercent,
                        MakeNumber(UiUtil.MakeNumericUpDownInt(0, 100, 15, 90, vm, nameof(vm.AudioDuckingVolume)), l.OriginalVolumePercent),
                        nameof(vm.DoAudioDucking)),
                    describes: toggleDucking),

                MakeRow(switchRemoveSpeech, l.RemoveOriginalSpeechDescription, describes: toggleRemoveSpeech),

                MakeRow(MakeLabel(Se.Language.General.Model), l.SpeechRemovalModelDescription, speechRemovalModel),

                MakeRow(switchVad, l.VadSilenceCompressionDescription,
                    MakeField(l.MaxSilenceMs,
                        MakeNumber(UiUtil.MakeNumericUpDownInt(0, 5000, 150, 90, vm, nameof(vm.VadMaxSilenceMs)), l.MaxSilenceMs),
                        nameof(vm.DoVadSilenceCompression)),
                    describes: toggleVad),

                MakeRow(switchTimeStretch, l.HighQualityTimeStretchDescription,
                    status: MakeStatusLabel(nameof(vm.RubberbandStatus)), describes: toggleTimeStretch),
            }
        }, first: true);

        var groupOutput = EditorChrome.MakeEditorSection(Se.Language.General.Output, new StackPanel
        {
            Spacing = 8,
            Children =
            {
                MakeRow(MakeLabel(l.SilencePaddingMs), l.SilencePaddingMsDescription,
                    MakeNumber(UiUtil.MakeNumericUpDownInt(0, 10000, 0, 110, vm, nameof(vm.SilencePaddingMs)), null)),

                MakeRow(MakeLabel(l.OutputSampleRate), l.OutputSampleRateDescription,
                    MakeNumber(UiUtil.MakeNumericUpDownInt(0, 192000, 0, 110, vm, nameof(vm.OutputSampleRate)), null)),
            }
        });

        var groupVoiceCloning = EditorChrome.MakeEditorSection(l.AdvancedTtsVoiceCloning, new StackPanel
        {
            Spacing = 8,
            Children =
            {
                MakeRow(switchCleanCloneReferences, l.CleanCloneReferencesDescription, describes: toggleCleanCloneReferences),
            }
        });

        var groupFolder = EditorChrome.MakeEditorSection(l.GenerationFolder, MakeGenerationFolderContent(vm, switchDeleteTempFiles, toggleDeleteTempFiles));

        var groupEdgeTts = EditorChrome.MakeEditorSection("Edge-TTS", new StackPanel
        {
            Spacing = 8,
            Children =
            {
                MakeRow(MakeLabel(l.EdgeTtsRate), l.EdgeTtsRateDescription,
                    MakeTextBox(UiUtil.MakeTextBox(110, vm, nameof(vm.EdgeTtsRate)))),

                MakeRow(MakeLabel(l.EdgeTtsPitch), l.EdgeTtsPitchDescription,
                    MakeTextBox(UiUtil.MakeTextBox(110, vm, nameof(vm.EdgeTtsPitch)))),

                MakeRow(MakeLabel(l.EdgeTtsVolume), l.EdgeTtsVolumeDescription,
                    MakeTextBox(UiUtil.MakeTextBox(110, vm, nameof(vm.EdgeTtsVolume)))),
            }
        });
        groupEdgeTts[!IsVisibleProperty] = new Binding(nameof(vm.IsEdgeTtsEngine)) { Mode = BindingMode.OneWay };

        var content = new StackPanel
        {
            Children = { groupAudio, groupVoiceCloning, groupOutput, groupFolder, groupEdgeTts },
        };

        // Keeps OK/Cancel reachable when the window is clamped to a small working area (#14331).
        var scrollViewer = new ScrollViewer
        {
            Content = content,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
        };

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
                new RowDefinition { Height = GridLength.Auto },
            },
        };
        grid.Add(new Border { Background = EditorChrome.PanelBrush, Child = scrollViewer }, 0);
        grid.Add(MakeButtonBar(vm), 1);

        Content = grid;

        // initial focus on an input, not an action button - a focused button clicks on bare Space
        UiUtil.FocusOnFirstActivation(this, () => toggleProAudio.Focus());
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

    /// <summary>
    /// An on/off option: a compact switch with its title beside it. The title wraps instead of
    /// widening the row for long translations.
    /// </summary>
    private static Grid MakeSwitch(string title, string binding, out ToggleSwitch toggle)
    {
        toggle = new ToggleSwitch
        {
            OnContent = null,
            OffContent = null,
            MinWidth = 0,
            VerticalAlignment = VerticalAlignment.Center,
            [!ToggleSwitch.IsCheckedProperty] = new Binding(binding) { Mode = BindingMode.TwoWay },
        };
        AutomationProperties.SetName(toggle, title);

        var row = new Grid
        {
            ColumnSpacing = 8,
            ColumnDefinitions = { new ColumnDefinition(GridLength.Auto), new ColumnDefinition(GridLength.Auto) },
        };
        row.Add(toggle, 0, 0);
        row.Add(new TextBlock { Text = title, FontSize = 12.5, VerticalAlignment = VerticalAlignment.Center, TextWrapping = TextWrapping.Wrap }, 0, 1);
        return row;
    }

    private static Label MakeLabel(string text)
    {
        var label = UiUtil.MakeLabel(text);
        label.FontSize = 12.5;
        label.Padding = new Thickness(0);
        label.VerticalAlignment = VerticalAlignment.Center;
        return label;
    }

    private static NumericUpDown MakeNumber(NumericUpDown field, string? name)
    {
        field.Classes.Add("field");
        if (name != null)
        {
            AutomationProperties.SetName(field, name);
        }

        return field;
    }

    private static TextBox MakeTextBox(TextBox textBox)
    {
        textBox.FontSize = 12.5;
        textBox.MinHeight = 28;
        textBox.Background = EditorChrome.FieldBrush;
        textBox.BorderBrush = EditorChrome.FieldBorderBrush;
        textBox.CornerRadius = new CornerRadius(6);
        textBox.VerticalContentAlignment = VerticalAlignment.Center;
        return textBox;
    }

    private static TextBlock MakeStatusLabel(string statusBinding)
    {
        return new TextBlock
        {
            FontSize = 12,
            Foreground = EditorChrome.DimTextBrush,
            FontStyle = FontStyle.Italic,
            VerticalAlignment = VerticalAlignment.Center,
            [!TextBlock.TextProperty] = new Binding(statusBinding) { Mode = BindingMode.OneWay },
        };
    }

    /// <summary>
    /// One option: its label/switch plus hint icon on the left, its input right-aligned so the
    /// inputs of a group line up.
    /// </summary>
    private static Grid MakeRow(Control label, string hint, Control? input = null, Control? status = null, Control? describes = null)
    {
        var leftPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 6,
            VerticalAlignment = VerticalAlignment.Center,
            Children = { label },
        };

        if (status != null)
        {
            leftPanel.Children.Add(status);
        }

        // Screen readers get the hint as help text on the control it belongs to: the switch when
        // the option is a toggle, otherwise the input the label names.
        leftPanel.Children.Add(UiUtil.MakeHintIcon(hint, describes ?? input ?? label));

        var grid = new Grid
        {
            ColumnSpacing = 10,
            MinHeight = 28,
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                new ColumnDefinition { Width = GridLength.Auto },
            },
        };
        grid.Add(leftPanel, 0);

        if (input != null)
        {
            input.HorizontalAlignment = HorizontalAlignment.Right;
            input.VerticalAlignment = VerticalAlignment.Center;
            grid.Add(input, 0, 1);
            // The label sits inside leftPanel, not beside the input in the grid, so link it by hand (#12087).
            if (label is Label && !AccessibleLabels.HasAccessibleName(input))
            {
                input.WithLabeledBy(label);
            }
        }

        return grid;
    }

    /// <summary>
    /// A sub-label plus input, greyed out while the option it belongs to is off.
    /// </summary>
    private static Control MakeField(string label, Control input, string isEnabledBinding)
    {
        var panel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 6,
            VerticalAlignment = VerticalAlignment.Center,
            Children =
            {
                new Label
                {
                    Content = label,
                    FontSize = 12,
                    Foreground = EditorChrome.DimTextBrush,
                    VerticalAlignment = VerticalAlignment.Center,
                    Padding = new Thickness(0),
                },
                input,
            },
            [!IsEnabledProperty] = new Binding(isEnabledBinding) { Mode = BindingMode.OneWay },
        };

        return panel;
    }

    /// <summary>
    /// Where the per-line clips are written during a run, and whether they are swept when the
    /// window closes. Before #13332 they went loose into the system temp folder and stayed there.
    /// </summary>
    private static Control MakeGenerationFolderContent(AdvancedTtsSettingsViewModel vm, Control switchDeleteTempFiles, ToggleSwitch toggleDeleteTempFiles)
    {
        var l = Se.Language.Video.TextToSpeech;

        var textBoxFolder = MakeTextBox(new TextBox
        {
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            PlaceholderText = Path.GetTempPath(),
            [!TextBox.TextProperty] = new Binding(nameof(vm.GenerationFolder)) { Mode = BindingMode.TwoWay, UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged },
        });
        AutomationProperties.SetName(textBoxFolder, l.GenerationFolder);

        var buttonBrowse = EditorChrome.CreateToolButton(IconNames.FolderOpen, l.GenerationFolder, vm.BrowseGenerationFolderCommand, 28);

        var folderRow = new Grid
        {
            ColumnSpacing = 6,
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                new ColumnDefinition { Width = GridLength.Auto },
                new ColumnDefinition { Width = GridLength.Auto },
            },
        };
        folderRow.Add(textBoxFolder, 0);
        folderRow.Add(buttonBrowse, 0, 1);
        folderRow.Add(UiUtil.MakeHintIcon(l.GenerationFolderDescription, textBoxFolder), 0, 2);

        return new StackPanel
        {
            Spacing = 8,
            Children =
            {
                folderRow,
                MakeRow(switchDeleteTempFiles, l.DeleteTempFilesDescription, describes: toggleDeleteTempFiles),
            }
        };
    }

    private static Border MakeButtonBar(AdvancedTtsSettingsViewModel vm)
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
