using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using System.IO;

namespace Nikse.SubtitleEdit.Features.Video.TextToSpeech.AdvancedTtsSettings;

/// <summary>
/// Every option used to carry its explanation as a paragraph of wrapped text underneath it, which
/// made the window several screens tall - on a laptop it was clamped to the working area and the
/// OK/Cancel buttons ended up off-screen (#14331). The explanations now live in hover hints, the
/// options are grouped, and the content scrolls with the buttons pinned below it.
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

        _vm = vm;
        vm.Window = this;
        DataContext = vm;

        var l = Se.Language.Video.TextToSpeech;

        var checkBoxProAudio = MakeCheckBox(l.ProAudioPostProcessing, nameof(vm.DoProAudioChain));
        var checkBoxDucking = MakeCheckBox(l.AudioDucking, nameof(vm.DoAudioDucking));
        var checkBoxVad = MakeCheckBox(l.VadSilenceCompression, nameof(vm.DoVadSilenceCompression));
        var checkBoxTimeStretch = MakeCheckBox(l.HighQualityTimeStretch, nameof(vm.DoHighQualityTimeStretch));
        var checkBoxDeleteTempFiles = MakeCheckBox(l.DeleteTempFiles, nameof(vm.DoDeleteTempFiles));

        var groupAudio = MakeGroupBox(l.AdvancedTtsAudioProcessing, new StackPanel
        {
            Spacing = 6,
            Children =
            {
                MakeRow(checkBoxProAudio, l.ProAudioPostProcessingDescription),

                MakeRow(checkBoxDucking, l.AudioDuckingDescription,
                    MakeField(l.OriginalVolumePercent,
                        UiUtil.MakeNumericUpDownInt(0, 100, 15, 110, vm, nameof(vm.AudioDuckingVolume)),
                        nameof(vm.DoAudioDucking))),

                MakeRow(checkBoxVad, l.VadSilenceCompressionDescription,
                    MakeField(l.MaxSilenceMs,
                        UiUtil.MakeNumericUpDownInt(0, 5000, 150, 110, vm, nameof(vm.VadMaxSilenceMs)),
                        nameof(vm.DoVadSilenceCompression))),

                MakeRow(checkBoxTimeStretch, l.HighQualityTimeStretchDescription,
                    status: MakeStatusLabel(nameof(vm.RubberbandStatus))),
            }
        });

        var groupOutput = MakeGroupBox(l.AdvancedTtsOutput, new StackPanel
        {
            Spacing = 6,
            Children =
            {
                MakeRow(UiUtil.MakeLabel(l.SilencePaddingMs), l.SilencePaddingMsDescription,
                    UiUtil.MakeNumericUpDownInt(0, 10000, 0, 110, vm, nameof(vm.SilencePaddingMs))),

                MakeRow(UiUtil.MakeLabel(l.OutputSampleRate), l.OutputSampleRateDescription,
                    UiUtil.MakeNumericUpDownInt(0, 192000, 0, 120, vm, nameof(vm.OutputSampleRate))),
            }
        });

        var groupFolder = MakeGroupBox(l.GenerationFolder, MakeGenerationFolderContent(vm, checkBoxDeleteTempFiles));

        var groupEdgeTts = MakeGroupBox("Edge-TTS", new StackPanel
        {
            Spacing = 6,
            Children =
            {
                MakeRow(UiUtil.MakeLabel(l.EdgeTtsRate), l.EdgeTtsRateDescription,
                    UiUtil.MakeTextBox(110, vm, nameof(vm.EdgeTtsRate))),

                MakeRow(UiUtil.MakeLabel(l.EdgeTtsPitch), l.EdgeTtsPitchDescription,
                    UiUtil.MakeTextBox(110, vm, nameof(vm.EdgeTtsPitch))),

                MakeRow(UiUtil.MakeLabel(l.EdgeTtsVolume), l.EdgeTtsVolumeDescription,
                    UiUtil.MakeTextBox(110, vm, nameof(vm.EdgeTtsVolume))),
            }
        });
        groupEdgeTts[!IsVisibleProperty] = new Binding(nameof(vm.IsEdgeTtsEngine)) { Mode = BindingMode.OneWay };

        var content = new StackPanel
        {
            Spacing = 10,
            Children = { groupAudio, groupOutput, groupFolder, groupEdgeTts },
        };

        // Keeps OK/Cancel reachable when the window is clamped to a small working area (#14331).
        var scrollViewer = new ScrollViewer
        {
            Content = content,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
        };

        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);
        var panelButtons = UiUtil.MakeButtonBar(buttonOk, buttonCancel);

        var grid = new Grid
        {
            Margin = UiUtil.MakeWindowMargin(),
            RowSpacing = 10,
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
                new RowDefinition { Height = GridLength.Auto },
            },
        };
        grid.Add(scrollViewer, 0);
        grid.Add(panelButtons, 1);

        Content = grid;

        // initial focus on an input, not an action button - a focused button clicks on bare Space
        UiUtil.FocusOnFirstActivation(this, () => checkBoxProAudio.Focus());
    }

    private static CheckBox MakeCheckBox(string title, string binding)
    {
        return new CheckBox
        {
            // A TextBlock (not a bare string) so long translations wrap instead of widening the row.
            Content = new TextBlock { Text = title, TextWrapping = TextWrapping.Wrap },
            VerticalAlignment = VerticalAlignment.Center,
            [!CheckBox.IsCheckedProperty] = new Binding(binding) { Mode = BindingMode.TwoWay },
        };
    }

    private static Label MakeStatusLabel(string statusBinding)
    {
        return new Label
        {
            Opacity = 0.6,
            FontStyle = FontStyle.Italic,
            VerticalAlignment = VerticalAlignment.Center,
            Padding = new Thickness(0),
            [!ContentControl.ContentProperty] = new Binding(statusBinding) { Mode = BindingMode.OneWay },
        };
    }

    /// <summary>
    /// One option: its label/check box plus hint icon on the left, its input right-aligned so the
    /// inputs of a group line up.
    /// </summary>
    private static Grid MakeRow(Control label, string hint, Control? input = null, Control? status = null)
    {
        var leftPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 5,
            VerticalAlignment = VerticalAlignment.Center,
            Children = { label },
        };

        if (status != null)
        {
            leftPanel.Children.Add(status);
        }

        // Screen readers get the hint as help text on the control it belongs to: the check box when
        // the option is a toggle, otherwise the input the label names.
        leftPanel.Children.Add(UiUtil.MakeHintIcon(hint, label is CheckBox ? label : input ?? label));

        var grid = new Grid
        {
            ColumnSpacing = 10,
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
            grid.Add(input, 0, 1);
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
            Spacing = 5,
            VerticalAlignment = VerticalAlignment.Center,
            Children =
            {
                new Label { Content = label, VerticalAlignment = VerticalAlignment.Center, Padding = new Thickness(0) },
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
    private static Control MakeGenerationFolderContent(AdvancedTtsSettingsViewModel vm, CheckBox checkBoxDeleteTempFiles)
    {
        var l = Se.Language.Video.TextToSpeech;

        var textBoxFolder = new TextBox
        {
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            PlaceholderText = Path.GetTempPath(),
            [!TextBox.TextProperty] = new Binding(nameof(vm.GenerationFolder)) { Mode = BindingMode.TwoWay, UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged },
        };
        AutomationProperties.SetName(textBoxFolder, l.GenerationFolder);

        var buttonBrowse = UiUtil.MakeButtonBrowse(vm.BrowseGenerationFolderCommand, accessibleName: l.GenerationFolder);

        var folderRow = new Grid
        {
            ColumnSpacing = 5,
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
            Spacing = 6,
            Children =
            {
                folderRow,
                MakeRow(checkBoxDeleteTempFiles, l.DeleteTempFilesDescription),
            }
        };
    }

    private static Border MakeGroupBox(string title, Control content)
    {
        var header = new TextBlock
        {
            Text = title,
            FontWeight = FontWeight.SemiBold,
            Margin = new Thickness(0, 0, 0, 6),
        };

        return UiUtil.MakeBorderForControl(new StackPanel
        {
            Children = { header, content },
        });
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        _vm.OnKeyDown(e);
    }
}
