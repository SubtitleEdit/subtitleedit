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

        var lineLabel = new Label
        {
            Content = "Teletext line:",
            VerticalAlignment = VerticalAlignment.Center,
        };

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

        var alignmentLabel = new Label
        {
            Content = "Alignment:",
            VerticalAlignment = VerticalAlignment.Center,
        };

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
var previewCheckBox = new CheckBox
{
    Content = "Vorschau",
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

        grid.Add(lineLabel, 0, 0);
        grid.Add(lineBox, 0, 1);

        grid.Add(alignmentLabel, 1, 0);
        grid.Add(alignmentBox, 1, 1);

        grid.Add(previewCheckBox, 2, 1);

        var buttonPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 10,
            HorizontalAlignment = HorizontalAlignment.Right,
        };

        buttonPanel.Children.Add(okButton);
        buttonPanel.Children.Add(cancelButton);

        grid.Add(buttonPanel, 3, 1);

        Content = grid;

        Activated += delegate { lineBox.Focus(); };
        KeyDown += (_, e) => vm.OnKeyDown(e);
    }
}
