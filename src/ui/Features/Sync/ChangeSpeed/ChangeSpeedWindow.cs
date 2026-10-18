using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.Media;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.ValueConverters;

namespace Nikse.SubtitleEdit.Features.Sync.ChangeSpeed;

/// <summary>
/// Change speed, in the editor look (see <see cref="EditorChrome"/>): the speed (with the
/// drop-frame presets) and which lines to adjust side by side, Apply and Done at the bottom.
/// </summary>
public class ChangeSpeedWindow : Window
{
    public ChangeSpeedWindow(ChangeSpeedViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = UiUtil.MakeWindowTitle(Se.Language.General.ChangeSpeed);
        SizeToContent = SizeToContent.WidthAndHeight;
        CanResize = false;
        EditorChrome.ApplyWindow(this, EditorWindowStyles.TimeCodes);
        vm.Window = this;
        DataContext = vm;

        var numericUpDownSpeed = new NumericUpDown
        {
            Width = 120,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center,
            Minimum = 1, // a speed of 0% means a 100/0 factor (divide by zero / infinite times)
            Maximum = 1000,
            Increment = 0.1m,
            DataContext = vm,
            [!NumericUpDown.ValueProperty] = new Binding(nameof(vm.SpeedPercent))
            {
                Mode = BindingMode.TwoWay,
                Converter = new DoubleToFourDecimalConverter(),
            },
        };
        numericUpDownSpeed.Classes.Add("field");
        AutomationProperties.SetName(numericUpDownSpeed, Se.Language.Sync.SpeedInPercentage);

        // Real (secondary) buttons: as flat text tool buttons they read as labels
        var buttonFromDropFrame = UiUtil.MakeButton(Se.Language.Sync.FromDropFrameValue, vm.SetFromDropFrameValueCommand).WithFontSize(12);
        var buttonToDropFrame = UiUtil.MakeButton(Se.Language.Sync.ToDropFrameValue, vm.SetToDropFrameValueCommand).WithFontSize(12);
        foreach (var button in new[] { buttonFromDropFrame, buttonToDropFrame })
        {
            button.Margin = new Thickness(0);
            button.MinHeight = 28;
            button.HorizontalAlignment = HorizontalAlignment.Stretch;
            button.HorizontalContentAlignment = HorizontalAlignment.Center;
        }

        var panelFromToButtons = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 6,
            Children =
            {
                buttonFromDropFrame,
                buttonToDropFrame,
            },
        };

        var speedRows = new StackPanel { Spacing = 8 };
        speedRows.Children.Add(EditorChrome.MakeLabeledRow(Se.Language.Sync.SpeedInPercentage, numericUpDownSpeed, 80));
        speedRows.Children.Add(panelFromToButtons);
        var speedSection = EditorChrome.MakeEditorSection(Se.Language.General.Speed, speedRows, first: true);
        speedSection.MinWidth = 250;

        var panelRadioButtons = SyncTimingChrome.MakeApplyToRadioButtons(
            nameof(vm.AdjustAll),
            nameof(vm.AdjustSelectedLines),
            nameof(vm.AdjustSelectedLinesAndForward),
            nameof(vm.IsSelectionAvailable));
        var linesSection = EditorChrome.MakeEditorSection(Se.Language.General.ApplyTo, panelRadioButtons, first: true);

        var buttonApply = UiUtil.MakeButton(Se.Language.General.Apply, vm.ApplyCommand);
        var buttonDone = UiUtil.MakeButtonDone(vm.DoneCommand);
        buttonDone.Classes.Add("primary");

        Content = EditorChrome.MakeRoot(EditorChrome.MakeSideBySide(speedSection, linesSection), EditorChrome.MakeDockedButtonBar(null, buttonApply, buttonDone));

        UiUtil.FocusOnFirstActivation(this, buttonDone); // hack to make OnKeyDown work
        Loaded += (_, _) => UiUtil.RestoreWindowPosition(this);
        Closing += (_, _) => UiUtil.SaveWindowPosition(this);
        KeyDown += (_, e) => vm.OnKeyDown(e);
    }
}
