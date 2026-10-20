using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Layout;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Shared.PickTeletextAlignment;

/// <summary>
/// Teletext line/alignment, in the editor look (see <see cref="EditorChrome"/>): the opt-in
/// edit rows in one section, the view options as switches below.
/// </summary>
public class PickTeletextAlignmentWindow : Window
{
    public PickTeletextAlignmentWindow(PickTeletextAlignmentViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.General.TeletextAlignment;
        CanResize = false;
        SizeToContent = SizeToContent.WidthAndHeight;
        MinWidth = 520;
        EditorChrome.ApplyWindow(this, EditorWindowStyles.Picker);
        vm.Window = this;
        DataContext = vm;

        var lineCheckBox = MakeApplyCheckBox(Se.Language.General.TeletextLine, nameof(vm.ApplyTeletextLine));
        var lineBox = MakeLineBox(1, 23, nameof(vm.TeletextLine), Se.Language.General.TeletextLine);
        lineBox.HorizontalAlignment = HorizontalAlignment.Left;

        var alignmentCheckBox = MakeApplyCheckBox(Se.Language.General.Alignment, nameof(vm.ApplyHorizontalAlignment));
        var alignmentBox = new ComboBox
        {
            ItemsSource = new[]
            {
                Se.Language.General.Left,
                Se.Language.General.Center,
                Se.Language.General.Right,
            },
            Width = 150,
            HorizontalAlignment = HorizontalAlignment.Left,
        };
        Avalonia.Automation.AutomationProperties.SetName(alignmentBox, Se.Language.General.Alignment);
        alignmentBox.Bind(ComboBox.SelectedItemProperty, new Binding(nameof(vm.HorizontalAlignment))
        {
            Mode = BindingMode.TwoWay,
        });

        var shiftCheckBox = MakeApplyCheckBox(Se.Language.General.ShiftLineBy, nameof(vm.ApplyLineShift));
        var shiftBox = MakeLineBox(-22, 22, nameof(vm.LineShift), Se.Language.General.ShiftLineBy);
        shiftBox.HorizontalAlignment = HorizontalAlignment.Left;

        var replaceLineCheckBox = MakeApplyCheckBox(Se.Language.General.ReplaceLine, nameof(vm.ApplyLineReplace));
        var replaceLinePanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 8,
            HorizontalAlignment = HorizontalAlignment.Left,
            Children =
            {
                EditorChrome.MakeDimLabel(Se.Language.General.From + ":", new Thickness(0)),
                MakeLineBox(1, 23, nameof(vm.ReplaceFromLine), Se.Language.General.From),
                EditorChrome.MakeDimLabel(Se.Language.General.To + ":", new Thickness(0)),
                MakeLineBox(1, 23, nameof(vm.ReplaceToLine), Se.Language.General.To),
            },
        };

        // View options, separated from the opt-in edit rows above.
        var previewSwitch = EditorChrome.MakeSwitchRow(nameof(vm.Preview), Se.Language.General.PreviewAlignmentInListView);
        var showTeletextSwitch = EditorChrome.MakeSwitchRow(nameof(vm.ShowTeletextColumn), Se.Language.General.ShowTeletext);

        var buttonOk = EditorChrome.AsPrimary(UiUtil.MakeButton(Se.Language.General.Ok, vm.OkCommand));
        var buttonCancel = UiUtil.MakeButton(Se.Language.General.Cancel, vm.CancelCommand);

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition(GridLength.Auto),
                new RowDefinition(GridLength.Auto),
                new RowDefinition(GridLength.Auto),
                new RowDefinition(GridLength.Auto),
            },
            ColumnDefinitions =
            {
                new ColumnDefinition(GridLength.Auto),
                new ColumnDefinition(new GridLength(1, GridUnitType.Star)),
            },
            ColumnSpacing = 16,
            RowSpacing = 8,
        };

        grid.Add(lineCheckBox, 0, 0);
        grid.Add(lineBox, 0, 1);

        grid.Add(alignmentCheckBox, 1, 0);
        grid.Add(alignmentBox, 1, 1);

        grid.Add(shiftCheckBox, 2, 0);
        grid.Add(shiftBox, 2, 1);

        grid.Add(replaceLineCheckBox, 3, 0);
        grid.Add(replaceLinePanel, 3, 1);

        var viewPanel = new StackPanel
        {
            Spacing = 8,
            Children = { previewSwitch, showTeletextSwitch },
        };

        var sections = new StackPanel
        {
            Children =
            {
                EditorChrome.MakeEditorSection(Se.Language.General.TeletextAlignment, grid, first: true),
                EditorChrome.MakeEditorSection(Se.Language.General.Preview, viewPanel),
            },
        };

        Content = EditorChrome.MakeRoot(sections, EditorChrome.MakeButtonBar(null, buttonCancel, buttonOk));

        UiUtil.FocusOnFirstActivation(this, lineBox);
        KeyDown += (_, e) => vm.OnKeyDown(e);
    }

    /// <summary>
    /// Each row of the dialog is opt-in, so its label doubles as the checkbox that enables it.
    /// </summary>
    private static CheckBox MakeApplyCheckBox(string header, string applyPropertyName)
    {
        var checkBox = new CheckBox
        {
            Content = header + ":",
            VerticalAlignment = VerticalAlignment.Center,
        };
        Avalonia.Automation.AutomationProperties.SetName(checkBox, header);

        checkBox.Bind(CheckBox.IsCheckedProperty, new Binding(applyPropertyName)
        {
            Mode = BindingMode.TwoWay,
        });

        return checkBox;
    }

    private static NumericUpDown MakeLineBox(decimal minimum, decimal maximum, string propertyName, string accessibleName)
    {
        var numericUpDown = new NumericUpDown
        {
            Minimum = minimum,
            Maximum = maximum,
            Increment = 1,
            Width = 120,
        };
        numericUpDown.Classes.Add("field"); // the compact field, not the theme's big spinner
        Avalonia.Automation.AutomationProperties.SetName(numericUpDown, accessibleName);

        numericUpDown.Bind(NumericUpDown.ValueProperty, new Binding(propertyName)
        {
            Mode = BindingMode.TwoWay,
        });

        return numericUpDown;
    }
}
