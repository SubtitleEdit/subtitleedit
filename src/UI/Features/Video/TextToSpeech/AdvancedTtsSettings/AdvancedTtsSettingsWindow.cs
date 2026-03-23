using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Video.TextToSpeech.AdvancedTtsSettings;

public class AdvancedTtsSettingsWindow : Window
{
    private readonly AdvancedTtsSettingsViewModel _vm;

    public AdvancedTtsSettingsWindow(AdvancedTtsSettingsViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = "Advanced TTS settings";
        SizeToContent = SizeToContent.WidthAndHeight;
        CanResize = false;
        MaxWidth = 550;

        _vm = vm;
        vm.Window = this;
        DataContext = vm;

        var content = new StackPanel
        {
            Margin = UiUtil.MakeWindowMargin(),
            Spacing = 5,
            Children =
            {
                MakeSection(vm,
                    "Pro audio post-processing",
                    nameof(vm.DoProAudioChain),
                    "Applies EQ warmth, noise gate, compression, loudness normalization (-16 LUFS), and fade in/out to each segment."),

                MakeSection(vm,
                    "Audio ducking",
                    nameof(vm.DoAudioDucking),
                    "Reduces the original video audio volume and mixes it with the TTS audio, so the original soundtrack is still faintly audible.",
                    "Original volume %",
                    nameof(vm.AudioDuckingVolume),
                    50),

                MakeSection(vm,
                    "VAD silence compression",
                    nameof(vm.DoVadSilenceCompression),
                    "Shortens pauses between words before changing tempo. Uses Voice Activity Detection to compress only silence gaps " +
                    "while keeping speech untouched. This is the preferred first step — it reduces duration without any quality loss.",
                    "Max silence (ms)",
                    nameof(vm.VadMaxSilenceMs),
                    50),

                MakeSectionWithStatus(vm,
                    "High-quality time-stretch (WSOLA/rubberband)",
                    nameof(vm.DoHighQualityTimeStretch),
                    nameof(vm.RubberbandStatus),
                    "Uses the rubberband algorithm (WSOLA) instead of the default atempo filter for pitch-preserving speed changes. " +
                    "Produces more natural-sounding speech, especially at higher speed factors. " +
                    "Requires librubberband in your FFmpeg build — falls back to atempo automatically if unavailable."),

                MakeFieldRow(vm, "Silence padding (ms)", nameof(vm.SilencePaddingMs), 50,
                    "Adds a short silence at the end of each segment. Useful for breathing room between sentences."),

                MakeFieldRow(vm, "Output sample rate (0 = default)", nameof(vm.OutputSampleRate), 60,
                    "Resamples all segments to the specified sample rate (e.g. 44100, 48000). Set to 0 to keep the original rate."),

                MakeFieldRow(vm, "Edge-TTS rate", nameof(vm.EdgeTtsRate), 120,
                    "Speech rate for Edge-TTS, e.g. \"+50%\", \"-30%\", or \"+0%\" for default.",
                    nameof(vm.IsEdgeTtsEngine)),

                MakeFieldRow(vm, "Edge-TTS pitch", nameof(vm.EdgeTtsPitch), 120,
                    "Pitch adjustment for Edge-TTS, e.g. \"+10Hz\", \"-5Hz\", or \"+0Hz\" for default.",
                    nameof(vm.IsEdgeTtsEngine)),

                MakeFieldRow(vm, "Edge-TTS volume", nameof(vm.EdgeTtsVolume), 120,
                    "Volume adjustment for Edge-TTS, e.g. \"+20%\", \"-10%\", or \"+0%\" for default.",
                    nameof(vm.IsEdgeTtsEngine)),
            }
        };

        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);
        var panelButtons = UiUtil.MakeButtonBar(buttonOk, buttonCancel);

        var mainPanel = new StackPanel
        {
            Children =
            {
                content,
                panelButtons,
            }
        };

        Content = mainPanel;

        Activated += delegate { buttonOk.Focus(); };
    }

    private static StackPanel MakeSection(AdvancedTtsSettingsViewModel vm, string title, string checkBoxBinding, string description,
        string? fieldLabel = null, string? fieldBinding = null, int fieldWidth = 50)
    {
        var checkBox = new CheckBox
        {
            Content = title,
            FontWeight = FontWeight.SemiBold,
            [!CheckBox.IsCheckedProperty] = new Binding(checkBoxBinding) { Mode = BindingMode.TwoWay },
        };

        var descBlock = new TextBlock
        {
            Text = description,
            TextWrapping = TextWrapping.Wrap,
            Opacity = 0.7,
            Margin = new Thickness(26, 0, 0, 0),
        };

        var section = new StackPanel
        {
            Spacing = 2,
            Margin = new Thickness(0, 0, 0, 10),
            Children = { checkBox, descBlock },
        };

        if (fieldLabel != null && fieldBinding != null)
        {
            var fieldRow = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(26, 4, 0, 0),
                Children =
                {
                    new Label { Content = fieldLabel, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 0, 5, 0) },
                    UiUtil.MakeTextBox(fieldWidth, vm, fieldBinding),
                }
            };
            section.Children.Add(fieldRow);
        }

        return section;
    }

    private static StackPanel MakeSectionWithStatus(AdvancedTtsSettingsViewModel vm, string title, string checkBoxBinding, string statusBinding, string description)
    {
        var checkBox = new CheckBox
        {
            Content = title,
            FontWeight = FontWeight.SemiBold,
            [!CheckBox.IsCheckedProperty] = new Binding(checkBoxBinding) { Mode = BindingMode.TwoWay },
        };

        var statusLabel = new Label
        {
            Opacity = 0.6,
            FontStyle = FontStyle.Italic,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(5, 0, 0, 0),
            [!Label.ContentProperty] = new Binding(statusBinding) { Mode = BindingMode.OneWay },
        };

        var headerRow = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Children = { checkBox, statusLabel },
        };

        var descBlock = new TextBlock
        {
            Text = description,
            TextWrapping = TextWrapping.Wrap,
            Opacity = 0.7,
            Margin = new Thickness(26, 0, 0, 0),
        };

        return new StackPanel
        {
            Spacing = 2,
            Margin = new Thickness(0, 0, 0, 10),
            Children = { headerRow, descBlock },
        };
    }

    private static StackPanel MakeFieldRow(AdvancedTtsSettingsViewModel vm, string label, string binding, int fieldWidth, string description, string? isVisibleBinding = null)
    {
        var panel = new StackPanel
        {
            Spacing = 2,
            Margin = new Thickness(0, 0, 0, 10),
            Children =
            {
                new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    Children =
                    {
                        new Label { Content = label, VerticalAlignment = VerticalAlignment.Center, FontWeight = FontWeight.SemiBold, Margin = new Thickness(0, 0, 5, 0) },
                        UiUtil.MakeTextBox(fieldWidth, vm, binding),
                    }
                },
                new TextBlock
                {
                    Text = description,
                    TextWrapping = TextWrapping.Wrap,
                    Opacity = 0.7,
                    Margin = new Thickness(26, 0, 0, 0),
                },
            }
        };

        if (isVisibleBinding != null)
        {
            panel[!StackPanel.IsVisibleProperty] = new Binding(isVisibleBinding) { Mode = BindingMode.OneWay };
        }

        return panel;
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        _vm.OnKeyDown(e);
    }
}
