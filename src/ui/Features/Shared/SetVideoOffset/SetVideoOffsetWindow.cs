using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Layout;
using Nikse.SubtitleEdit.Controls;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Shared.SetVideoOffset;

/// <summary>
/// Set video offset, in the editor look (see <see cref="EditorChrome"/>): the offset with
/// the recently used ones, the options as switches, Reset on the left of the button bar.
/// </summary>
public class SetVideoOffsetWindow : Window
{
    public SetVideoOffsetWindow(SetVideoOffsetViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.General.SetVideoOffset;
        SizeToContent = SizeToContent.WidthAndHeight;
        CanResize = false;
        EditorChrome.ApplyWindow(this, EditorWindowStyles.Picker);
        vm.Window = this;
        DataContext = vm;

        var timeCodeUpDownOffset = new TimeCodeUpDown
        {
            DataContext = vm,
            VerticalAlignment = VerticalAlignment.Center,
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

        AutomationProperties.SetName(timeCodeUpDownOffset, Se.Language.General.VideoOffset);
        var buttonTenHours = UiUtil.MakeButton(Se.Language.General.TenHours, vm.SetTenHoursCommand).WithFontSize(11);
        buttonTenHours.Margin = new Thickness(0);
        buttonTenHours.MinWidth = 0;
        buttonTenHours.MinHeight = 28;
        buttonTenHours.VerticalAlignment = VerticalAlignment.Center;

        var panelTimeCode = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 8,
            Children =
            {
                timeCodeUpDownOffset,
                comboBoxHistory,
                buttonTenHours,
            }
        };

        var panelOptions = new StackPanel
        {
            Spacing = 8,
            Children =
            {
                EditorChrome.MakeSwitchRow(nameof(vm.RelativeToCurrentVideoPosition), Se.Language.General.RelativeToCurrentVideoPosition),
                EditorChrome.MakeSwitchRow(nameof(vm.KeepTimeCodes), Se.Language.General.KeepExistingTimeCodes),
            }
        };

        var sections = new StackPanel
        {
            Children =
            {
                EditorChrome.MakeEditorSection(Se.Language.General.VideoOffset, panelTimeCode, first: true),
                EditorChrome.MakeEditorSection(Se.Language.General.Options, panelOptions),
            }
        };

        var buttonReset = UiUtil.MakeButton(Se.Language.General.Reset, vm.ResetCommand);
        var buttonBar = EditorChrome.MakeButtonBar(buttonReset, UiUtil.MakeButtonCancel(vm.CancelCommand), UiUtil.MakeButton(Se.Language.General.Apply, vm.ApplyCommand), EditorChrome.AsPrimary(UiUtil.MakeButtonOk(vm.OkCommand)));

        Content = EditorChrome.MakeRoot(sections, buttonBar);

        UiUtil.FocusOnFirstActivation(this, timeCodeUpDownOffset);
        KeyDown += (sender, args) => vm.OnKeyDown(args);
    }
}
