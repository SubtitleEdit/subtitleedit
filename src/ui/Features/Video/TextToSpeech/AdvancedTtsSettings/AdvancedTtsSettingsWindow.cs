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
        Title = Se.Language.Video.TextToSpeech.AdvancedTtsSettings;
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
                    Se.Language.Video.TextToSpeech.ProAudioPostProcessing,
                    nameof(vm.DoProAudioChain),
                    Se.Language.Video.TextToSpeech.ProAudioPostProcessingDescription),

                MakeSection(vm,
                    Se.Language.Video.TextToSpeech.AudioDucking,
                    nameof(vm.DoAudioDucking),
                    Se.Language.Video.TextToSpeech.AudioDuckingDescription,
                    Se.Language.Video.TextToSpeech.OriginalVolumePercent,
                    nameof(vm.AudioDuckingVolume),
                    50),

                MakeSection(vm,
                    Se.Language.Video.TextToSpeech.VadSilenceCompression,
                    nameof(vm.DoVadSilenceCompression),
                    Se.Language.Video.TextToSpeech.VadSilenceCompressionDescription,
                    Se.Language.Video.TextToSpeech.MaxSilenceMs,
                    nameof(vm.VadMaxSilenceMs),
                    50),

                MakeSectionWithStatus(vm,
                    Se.Language.Video.TextToSpeech.HighQualityTimeStretch,
                    nameof(vm.DoHighQualityTimeStretch),
                    nameof(vm.RubberbandStatus),
                    Se.Language.Video.TextToSpeech.HighQualityTimeStretchDescription),

                MakeFieldRow(vm, Se.Language.Video.TextToSpeech.SilencePaddingMs, nameof(vm.SilencePaddingMs), 50,
                    Se.Language.Video.TextToSpeech.SilencePaddingMsDescription),

                MakeFieldRow(vm, Se.Language.Video.TextToSpeech.OutputSampleRate, nameof(vm.OutputSampleRate), 60,
                    Se.Language.Video.TextToSpeech.OutputSampleRateDescription),

                MakeFieldRow(vm, Se.Language.Video.TextToSpeech.EdgeTtsRate, nameof(vm.EdgeTtsRate), 120,
                    Se.Language.Video.TextToSpeech.EdgeTtsRateDescription,
                    nameof(vm.IsEdgeTtsEngine)),

                MakeFieldRow(vm, Se.Language.Video.TextToSpeech.EdgeTtsPitch, nameof(vm.EdgeTtsPitch), 120,
                    Se.Language.Video.TextToSpeech.EdgeTtsPitchDescription,
                    nameof(vm.IsEdgeTtsEngine)),

                MakeFieldRow(vm, Se.Language.Video.TextToSpeech.EdgeTtsVolume, nameof(vm.EdgeTtsVolume), 120,
                    Se.Language.Video.TextToSpeech.EdgeTtsVolumeDescription,
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
