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
        Title = Se.Language.General.TeletextAlignment;
        CanResize = false;
        SizeToContent = SizeToContent.WidthAndHeight;
        MinWidth = 520;
        vm.Window = this;
        DataContext = vm;

        var lineCheckBox = new CheckBox
{
    Content = Se.Language.General.TeletextLine + ":",
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
            HorizontalAlignment = HorizontalAlignment.Right,
        };
        lineBox.Bind(
            NumericUpDown.ValueProperty,
            new Binding(nameof(vm.TeletextLine))
            {
                Mode = BindingMode.TwoWay
            });

        var alignmentCheckBox = new CheckBox
{
    Content = Se.Language.General.Alignment + ":",
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
            ItemsSource = new[]
{
    Se.Language.General.Left,
    Se.Language.General.Center,
    Se.Language.General.Right
},
            Width = 150,
            HorizontalAlignment = HorizontalAlignment.Right,
        };
        alignmentBox.Bind(
            ComboBox.SelectedItemProperty,
            new Binding(nameof(vm.HorizontalAlignment))
            {
                Mode = BindingMode.TwoWay
            });
          var shiftCheckBox = new CheckBox
{
    Content = Se.Language.General.ShiftLineBy + ":",
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
    HorizontalAlignment = HorizontalAlignment.Right,
};

shiftBox.Bind(
    NumericUpDown.ValueProperty,
    new Binding(nameof(vm.LineShift))
    {
        Mode = BindingMode.TwoWay
    });  

var replaceLineCheckBox = new CheckBox
{
    Content = Se.Language.General.ReplaceLine + ":",
    VerticalAlignment = VerticalAlignment.Center,
};

replaceLineCheckBox.Bind(
    CheckBox.IsCheckedProperty,
    new Binding(nameof(vm.ApplyLineReplace))
    {
        Mode = BindingMode.TwoWay
    });

var replaceLinePanel = new StackPanel
{
    Orientation = Orientation.Horizontal,
    Spacing = 8,
    HorizontalAlignment = HorizontalAlignment.Right,
};

var fromLabel = new Label
{
    Content = Se.Language.General.From + ":",
    VerticalAlignment = VerticalAlignment.Center,
};

var replaceFromBox = new NumericUpDown
{
    Minimum = 1,
    Maximum = 23,
    Increment = 1,
    Width = 100,
};

replaceFromBox.Bind(
    NumericUpDown.ValueProperty,
    new Binding(nameof(vm.ReplaceFromLine))
    {
        Mode = BindingMode.TwoWay
    });

var toLabel = new Label
{
    Content = Se.Language.General.To + ":",
    VerticalAlignment = VerticalAlignment.Center,
};

var replaceToBox = new NumericUpDown
{
    Minimum = 1,
    Maximum = 23,
    Increment = 1,
    Width = 100,
};

replaceToBox.Bind(
    NumericUpDown.ValueProperty,
    new Binding(nameof(vm.ReplaceToLine))
    {
        Mode = BindingMode.TwoWay
    });

replaceLinePanel.Children.Add(fromLabel);
replaceLinePanel.Children.Add(replaceFromBox);
replaceLinePanel.Children.Add(toLabel);
replaceLinePanel.Children.Add(replaceToBox);

var previewCheckBox = new CheckBox
{
    Content = Se.Language.General.Preview,
};

previewCheckBox.Bind(
    CheckBox.IsCheckedProperty,
    new Binding(nameof(vm.Preview))
    {
        Mode = BindingMode.TwoWay
    });
    
var showTeletextCheckBox = new CheckBox
{
    Content = Se.Language.General.ShowTeletext,
};

showTeletextCheckBox.Bind(
    CheckBox.IsCheckedProperty,
    new Binding(nameof(vm.ShowTeletextColumn))
    {
        Mode = BindingMode.TwoWay
    });

        var okButton = UiUtil.MakeButton(Se.Language.General.Ok, vm.OkCommand).WithMinWidth(100);
var cancelButton = UiUtil.MakeButton(Se.Language.General.Cancel, vm.CancelCommand).WithMinWidth(100);
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

        var buttonPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 10,
            HorizontalAlignment = HorizontalAlignment.Right,
        };

        buttonPanel.Children.Add(okButton);
        buttonPanel.Children.Add(cancelButton);

        grid.Add(buttonPanel, 6, 1);

        Content = grid;

        Activated += delegate { lineBox.Focus(); };
        KeyDown += (_, e) => vm.OnKeyDown(e);
    }
}
