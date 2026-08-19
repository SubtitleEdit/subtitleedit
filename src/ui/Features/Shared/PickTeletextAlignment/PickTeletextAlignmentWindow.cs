using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Layout;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Shared.PickTeletextAlignment;

public class PickTeletextAlignmentWindow : Window
{
    public PickTeletextAlignmentWindow(PickTeletextAlignmentViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.General.TeletextAlignment;
        CanResize = false;
        SizeToContent = SizeToContent.WidthAndHeight;
        MinWidth = 520;
        vm.Window = this;
        DataContext = vm;

        var lineCheckBox = MakeApplyCheckBox(Se.Language.General.TeletextLine, nameof(vm.ApplyTeletextLine));
        var lineBox = MakeLineBox(1, 23, nameof(vm.TeletextLine));
        lineBox.HorizontalAlignment = HorizontalAlignment.Right;

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
            HorizontalAlignment = HorizontalAlignment.Right,
        };
        alignmentBox.Bind(ComboBox.SelectedItemProperty, new Binding(nameof(vm.HorizontalAlignment))
        {
            Mode = BindingMode.TwoWay,
        });

        var shiftCheckBox = MakeApplyCheckBox(Se.Language.General.ShiftLineBy, nameof(vm.ApplyLineShift));
        var shiftBox = MakeLineBox(-22, 22, nameof(vm.LineShift));
        shiftBox.HorizontalAlignment = HorizontalAlignment.Right;

        var replaceLineCheckBox = MakeApplyCheckBox(Se.Language.General.ReplaceLine, nameof(vm.ApplyLineReplace));
        var replaceLinePanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 8,
            HorizontalAlignment = HorizontalAlignment.Right,
            Children =
            {
                new Label
                {
                    Content = Se.Language.General.From + ":",
                    VerticalAlignment = VerticalAlignment.Center,
                },
                MakeLineBox(1, 23, nameof(vm.ReplaceFromLine)),
                new Label
                {
                    Content = Se.Language.General.To + ":",
                    VerticalAlignment = VerticalAlignment.Center,
                },
                MakeLineBox(1, 23, nameof(vm.ReplaceToLine)),
            },
        };

        var previewCheckBox = new CheckBox { Content = Se.Language.General.Preview };
        previewCheckBox.Bind(CheckBox.IsCheckedProperty, new Binding(nameof(vm.Preview))
        {
            Mode = BindingMode.TwoWay,
        });

        var showTeletextCheckBox = new CheckBox { Content = Se.Language.General.ShowTeletext };
        showTeletextCheckBox.Bind(CheckBox.IsCheckedProperty, new Binding(nameof(vm.ShowTeletextColumn))
        {
            Mode = BindingMode.TwoWay,
        });

        var buttonPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 10,
            HorizontalAlignment = HorizontalAlignment.Right,
            Children =
            {
                UiUtil.MakeButton(Se.Language.General.Ok, vm.OkCommand).WithMinWidth(100),
                UiUtil.MakeButton(Se.Language.General.Cancel, vm.CancelCommand).WithMinWidth(100),
            },
        };

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition(GridLength.Auto),
                new RowDefinition(GridLength.Auto),
                new RowDefinition(GridLength.Auto),
                new RowDefinition(GridLength.Auto),
                new RowDefinition(GridLength.Auto),
                new RowDefinition(GridLength.Auto),
                new RowDefinition(GridLength.Auto),
            },
            ColumnDefinitions =
            {
                new ColumnDefinition(GridLength.Auto),
                new ColumnDefinition(GridLength.Auto),
            },
            Margin = UiUtil.MakeWindowMargin(),
            ColumnSpacing = 10,
            RowSpacing = 10,
        };

        grid.Add(lineCheckBox, 0, 0);
        grid.Add(lineBox, 0, 1);

        grid.Add(alignmentCheckBox, 1, 0);
        grid.Add(alignmentBox, 1, 1);

        grid.Add(shiftCheckBox, 2, 0);
        grid.Add(shiftBox, 2, 1);

        grid.Add(replaceLineCheckBox, 3, 0);
        grid.Add(replaceLinePanel, 3, 1);

        grid.Add(previewCheckBox, 4, 0);
        grid.Add(showTeletextCheckBox, 5, 0);
        grid.Add(buttonPanel, 6, 1);

        Content = grid;

        Activated += delegate { lineBox.Focus(); };
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

        checkBox.Bind(CheckBox.IsCheckedProperty, new Binding(applyPropertyName)
        {
            Mode = BindingMode.TwoWay,
        });

        return checkBox;
    }

    private static NumericUpDown MakeLineBox(decimal minimum, decimal maximum, string propertyName)
    {
        var numericUpDown = new NumericUpDown
        {
            Minimum = minimum,
            Maximum = maximum,
            Increment = 1,
            Width = 100,
        };

        numericUpDown.Bind(NumericUpDown.ValueProperty, new Binding(propertyName)
        {
            Mode = BindingMode.TwoWay,
        });

        return numericUpDown;
    }
}
