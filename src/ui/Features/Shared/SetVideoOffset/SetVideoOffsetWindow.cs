using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Layout;
using Nikse.SubtitleEdit.Controls;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Shared.SetVideoOffset;

public class SetVideoOffsetWindow : Window
{
    public SetVideoOffsetWindow(SetVideoOffsetViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.General.SetVideoOffset;
        SizeToContent = SizeToContent.WidthAndHeight;
        CanResize = false;
        vm.Window = this;
        DataContext = vm;

        var timeCodeUpDownOffset = new TimeCodeUpDown
        {
            DataContext = vm,
            [!TimeCodeUpDown.ValueProperty] = new Binding(nameof(vm.TimeOffset))
            {
                Mode = BindingMode.TwoWay,
            }
        };

        // Previously used offsets - picking one fills the time code box, so switching between the
        // handful of offsets a user keeps coming back to takes one click instead of retyping.
        var comboBoxHistory = UiUtil.MakeComboBox(vm.OffsetHistory, vm, nameof(vm.SelectedOffsetHistoryItem));

        // Wide enough for a time code, so the box does not shrink to an arrow stub while the
        // typed offset matches no entry and nothing is selected.
        comboBoxHistory.MinWidth = 130;
        AutomationProperties.SetName(comboBoxHistory, Se.Language.General.RecentlyUsedVideoOffsets);
        if (Se.Settings.Appearance.ShowHints)
        {
            ToolTip.SetTip(comboBoxHistory, Se.Language.General.RecentlyUsedVideoOffsets);
        }

        var panelTimeCode = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 15,
            Margin = new Thickness(10, 20, 10, 10),
            Children =
            {
                UiUtil.MakeLabel(Se.Language.General.VideoOffset),
                timeCodeUpDownOffset,
                comboBoxHistory,
                UiUtil.MakeButton(Se.Language.General.TenHours, vm.SetTenHoursCommand).WithFontSize(10),
            }
        };

        var checkBoxRelativeToCurrentVideoPosition = UiUtil.MakeCheckBox(Se.Language.General.RelativeToCurrentVideoPosition,
            vm,
            nameof(vm.RelativeToCurrentVideoPosition));

        var checkBoxKeepTimeCodes = UiUtil.MakeCheckBox(Se.Language.General.KeepExistingTimeCodes,
            vm,
            nameof(vm.KeepTimeCodes));

        var buttonPanel = UiUtil.MakeButtonBar(
            UiUtil.MakeButtonOk(vm.OkCommand),
            UiUtil.MakeButton(Se.Language.General.Apply, vm.ApplyCommand),
            UiUtil.MakeButton(Se.Language.General.Reset, vm.ResetCommand),
            UiUtil.MakeButtonCancel(vm.CancelCommand));

        var contentPanel = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 15,
            Margin = UiUtil.MakeWindowMargin(),
            Children =
            {
                panelTimeCode,
                checkBoxRelativeToCurrentVideoPosition,
                checkBoxKeepTimeCodes,
                buttonPanel,
            }
        };

        Content = contentPanel;

        Activated += delegate { timeCodeUpDownOffset.Focus(); };
        KeyDown += (sender, args) => vm.OnKeyDown(args);
    }
}
