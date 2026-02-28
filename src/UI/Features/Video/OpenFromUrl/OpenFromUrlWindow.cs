using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Layout;
using Nikse.SubtitleEdit.Logic;

namespace Nikse.SubtitleEdit.Features.Video.OpenFromUrl;

public class OpenFromUrlWindow : Window
{
    private readonly OpenFromUrlViewModel _vm;
    
    public OpenFromUrlWindow(OpenFromUrlViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = "Open video file from URL";
        SizeToContent = SizeToContent.WidthAndHeight;
        CanResize = false;

        _vm = vm;
        vm.Window = this;
        DataContext = vm;

        var label = new Label
        {
            Content = "Url",
            VerticalAlignment = VerticalAlignment.Center,
        };

        var textBox = new TextBox
        {
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            MinWidth = 380,
            [!TextBox.TextProperty] = new Binding(nameof(vm.Url))
            {
                Mode = BindingMode.TwoWay
            },
            VerticalAlignment = VerticalAlignment.Center,
        };

        var buttonPanel = UiUtil.MakeButtonBar(UiUtil.MakeButtonOk(vm.OkCommand), UiUtil.MakeButtonCancel(vm.CancelCommand));

        var grid = new Grid
        {
            Margin = UiUtil.MakeWindowMargin(),
            ColumnSpacing = 10,
            RowSpacing = 10,
        };
        grid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
        grid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
        grid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
        grid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));

        grid.Children.Add(label);
        Grid.SetRow(label, 0);
        Grid.SetColumn(label, 0);

        grid.Children.Add(textBox);
        Grid.SetRow(textBox, 0);
        Grid.SetColumn(textBox, 1);

        grid.Children.Add(buttonPanel);
        Grid.SetRow(buttonPanel, 1);
        Grid.SetColumn(buttonPanel, 1);

        Content = grid;
        
        Activated += delegate { textBox.Focus(); }; // hack to make OnKeyDown work
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        _vm.OnKeyDown(e);
    }
}
