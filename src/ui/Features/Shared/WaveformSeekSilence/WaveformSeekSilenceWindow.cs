using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Layout;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Shared.WaveformSeekSilence;

/// <summary>
/// Seek silence, in the editor look (see <see cref="EditorChrome"/>).
/// </summary>
public class WaveformSeekSilenceWindow : Window
{
    public WaveformSeekSilenceWindow(WaveformSeekSilenceViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.Waveform.SeekSilence;
        CanResize = false;
        SizeToContent = SizeToContent.WidthAndHeight;
        EditorChrome.ApplyWindow(this, EditorWindowStyles.Picker);
        vm.Window = this;
        DataContext = vm;

        var checkBoxSeekForward = UiUtil.MakeRadioButton(Se.Language.General.Forward, vm, nameof(vm.SeekForward), "seek");
        var checkBoxSeekBackward = UiUtil.MakeRadioButton(Se.Language.General.Backward, vm, nameof(vm.SeekBackward), "seek");
        var panelDirection = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 16,
            Children = { checkBoxSeekForward, checkBoxSeekBackward },
        };

        var numericUpDownMinSilenceDuration = UiUtil.MakeNumericUpDownThreeDecimals(0, 10000, 140, vm, nameof(vm.SilenceMinDuration));
        AutomationProperties.SetName(numericUpDownMinSilenceDuration, Se.Language.Waveform.MinSilenceDurationSeconds);
        var numericUpDownMaxSilenceVolume = UiUtil.MakeNumericUpDownTwoDecimals(0, 1, 140, vm, nameof(vm.SilenceMaxVolume));
        AutomationProperties.SetName(numericUpDownMaxSilenceVolume, Se.Language.Waveform.MaxSilenceVolume);

        var gridSettings = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = GridLength.Auto },
                new ColumnDefinition { Width = GridLength.Auto },
            },
            RowDefinitions =
            {
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Auto },
            },
            ColumnSpacing = 12,
            RowSpacing = 8,
        };
        gridSettings.Add(EditorChrome.MakeDimLabel(Se.Language.Waveform.MinSilenceDurationSeconds, new Thickness(0)), 0, 0);
        gridSettings.Add(numericUpDownMinSilenceDuration, 0, 1);
        gridSettings.Add(EditorChrome.MakeDimLabel(Se.Language.Waveform.MaxSilenceVolume, new Thickness(0)), 1, 0);
        gridSettings.Add(numericUpDownMaxSilenceVolume, 1, 1);

        var sections = new StackPanel
        {
            Children =
            {
                EditorChrome.MakeEditorSection(Se.Language.General.SearchDirection, panelDirection, first: true),
                EditorChrome.MakeEditorSection(Se.Language.General.Settings, gridSettings),
            },
        };

        var buttonOk = EditorChrome.AsPrimary(UiUtil.MakeButtonOk(vm.OkCommand));
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);

        Content = EditorChrome.MakeRoot(sections, EditorChrome.MakeButtonBar(null, buttonCancel, buttonOk));

        UiUtil.FocusOnFirstActivation(this, checkBoxSeekForward); // initial focus on an input, not an action button - a focused button clicks on bare Space
        KeyDown += vm.KeyDown;
    }
}
