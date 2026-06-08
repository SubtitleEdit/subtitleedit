using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Options.Settings;

public class FixContinuationStyleSettingsWindow : Window
{
    public FixContinuationStyleSettingsWindow(FixContinuationStyleSettingsViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.Options.Settings.FixContinuationStyleSettings;
        CanResize = false;
        Width = 860;
        MinWidth = 680;
        SizeToContent = SizeToContent.Height;
        vm.Window = this;
        DataContext = vm;

        var checkBoxAllCaps = UiUtil.MakeCheckBox(Se.Language.Options.Settings.UncheckInsertsAllCaps, vm, nameof(vm.FixContinuationStyleUncheckInsertsAllCaps));
        var checkBoxItalic = UiUtil.MakeCheckBox(Se.Language.Options.Settings.UncheckInsertsItalic, vm, nameof(vm.FixContinuationStyleUncheckInsertsItalic));
        var checkBoxLowercase = UiUtil.MakeCheckBox(Se.Language.Options.Settings.UncheckInsertsLowercase, vm, nameof(vm.FixContinuationStyleUncheckInsertsLowercase));
        var checkBoxHideWithoutName = UiUtil.MakeCheckBox(Se.Language.Options.Settings.HideContinuationCandidatesWithoutName, vm, nameof(vm.FixContinuationStyleHideContinuationCandidatesWithoutName));
        var checkBoxIgnoreLyrics = UiUtil.MakeCheckBox(Se.Language.Options.Settings.IgnoreLyrics, vm, nameof(vm.FixContinuationStyleIgnoreLyrics));

        var labelPause = UiUtil.MakeLabel(Se.Language.Options.Settings.ContinuationPause);
        var numericPause = UiUtil.MakeNumericUpDownInt(0, 10_000, 300, 120, vm, nameof(vm.ContinuationPause));
        var textPauseUnit = UiUtil.MakeLabel("ms");
        var panelPause = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 8,
            Children =
            {
                labelPause,
                numericPause,
                textPauseUnit,
            }
        };

        var buttonEditCustomStyle = UiUtil.MakeButton(
            Se.Language.Options.Settings.EditContinuationStyleCustom,
            vm.ShowEditCustomContinuationStyleCommand).WithLeftAlignment();

        var noteCustomContinuationStyle = new TextBlock
        {
            Text = Se.Language.Options.Settings.CustomContinuationStyleNote,
            TextWrapping = Avalonia.Media.TextWrapping.Wrap,
            Foreground = UiUtil.GetBorderBrush(),
        };

        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);
        var panelButtons = UiUtil.MakeButtonBar(buttonOk, buttonCancel);

        var stackPanel = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 10,
            Margin = UiUtil.MakeWindowMargin(),
            Children =
            {
                checkBoxAllCaps,
                checkBoxItalic,
                checkBoxLowercase,
                checkBoxHideWithoutName,
                checkBoxIgnoreLyrics,
                panelPause.WithMarginTop(12),
                buttonEditCustomStyle.WithMarginTop(12),
                noteCustomContinuationStyle,
                panelButtons.WithMarginTop(12),
            }
        };

        Content = stackPanel;

        Activated += delegate { buttonOk.Focus(); };
        KeyDown += (_, e) => vm.OnKeyDown(e);
    }
}
