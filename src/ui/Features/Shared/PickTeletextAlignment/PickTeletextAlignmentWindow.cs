using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Data;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Shared.PickTeletextAlignment;

public class PickTeletextAlignmentWindow : Window
{
    public PickTeletextAlignmentWindow(PickTeletextAlignmentViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = "Teletext alignment";
        CanResize = false;
        SizeToContent = SizeToContent.WidthAndHeight;
        vm.Window = this;
        DataContext = vm;

        var lineCheckBox = new CheckBox
{
    Content = "Teletext line:",
    VerticalAlignment = VerticalAlignment.Center,
};

lineCheckBox.Bind(
    CheckBox.IsCheckedProperty,
    new Binding(nameof(vm.ApplyTeletextLine))
    {
        Mode = BindingMode.TwoWay
    });

        var lineBox = new NumericUpDown
        {
            Minimum = 1,
            Maximum = 23,
            Increment = 1,
            Width = 100,
        };
        lineBox.Bind(
            NumericUpDown.ValueProperty,
            new Binding(nameof(vm.TeletextLine))
            {
                Mode = BindingMode.TwoWay
            });

        var alignmentCheckBox = new CheckBox
{
    Content = "Alignment:",
    VerticalAlignment = VerticalAlignment.Center,
};

alignmentCheckBox.Bind(
    CheckBox.IsCheckedProperty,
    new Binding(nameof(vm.ApplyHorizontalAlignment))
    {
        Mode = BindingMode.TwoWay
    });

        var alignmentBox = new ComboBox
        {
            ItemsSource = new[] { "Left", "Center", "Right" },
            Width = 150,
        };
        alignmentBox.Bind(
            ComboBox.SelectedItemProperty,
            new Binding(nameof(vm.HorizontalAlignment))
            {
                Mode = BindingMode.TwoWay
            });
          var shiftCheckBox = new CheckBox
{
    Content = "Shift line by:",
    VerticalAlignment = VerticalAlignment.Center,
};

shiftCheckBox.Bind(
    CheckBox.IsCheckedProperty,
    new Binding(nameof(vm.ApplyLineShift))
    {
        Mode = BindingMode.TwoWay
    });

var shiftBox = new NumericUpDown
{
    Minimum = -22,
    Maximum = 22,
    Increment = 1,
    Width = 100,
};

shiftBox.Bind(
    NumericUpDown.ValueProperty,
    new Binding(nameof(vm.LineShift))
    {
        Mode = BindingMode.TwoWay
    });  
var previewCheckBox = new CheckBox
{
    Content = "Preview",
};

previewCheckBox.Bind(
    CheckBox.IsCheckedProperty,
    new Binding(nameof(vm.Preview))
    {
        Mode = BindingMode.TwoWay
    });
    
        var okButton = UiUtil.MakeButton("OK", vm.OkCommand).WithMinWidth(100);
        var cancelButton = UiUtil.MakeButton("Cancel", vm.CancelCommand).WithMinWidth(100);

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

        grid.Add(previewCheckBox, 3, 0);

        var buttonPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 10,
            HorizontalAlignment = HorizontalAlignment.Right,
        };

        buttonPanel.Children.Add(okButton);
        buttonPanel.Children.Add(cancelButton);

        grid.Add(buttonPanel, 4, 1);

        Content = grid;

        Activated += delegate { lineBox.Focus(); };
        KeyDown += (_, e) => vm.OnKeyDown(e);
    }
}
