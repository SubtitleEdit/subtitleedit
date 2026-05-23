using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Layout;
using Nikse.SubtitleEdit.Logic;

namespace Nikse.SubtitleEdit.Features.Ocr.FallbackDatabase;

public class OcrFallbackDatabaseWindow : Window
{
    public OcrFallbackDatabaseWindow(OcrFallbackDatabaseViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = vm.Title;
        SizeToContent = SizeToContent.WidthAndHeight;
        CanResize = false;
        MinWidth = 400;
        vm.Window = this;
        DataContext = vm;

        var labelDatabase = new TextBlock
        {
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 0, 0, 2),
        };
        labelDatabase.Bind(TextBlock.TextProperty, new Binding(nameof(vm.Label)));

        var comboBoxDatabases = new ComboBox
        {
            Width = 300,
            [!ComboBox.SelectedItemProperty] = new Binding(nameof(vm.SelectedDatabase)) { Mode = BindingMode.TwoWay },
            [!ComboBox.ItemsSourceProperty] = new Binding(nameof(vm.Databases)) { Mode = BindingMode.TwoWay },
        };

        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);
        var buttonPanel = UiUtil.MakeButtonBar(buttonOk, buttonCancel);

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
            Margin = UiUtil.MakeWindowMargin(),
            ColumnSpacing = 2,
            RowSpacing = 0,
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        grid.Add(labelDatabase, 0);
        grid.Add(comboBoxDatabases, 1);
        grid.Add(buttonPanel, 2);

        Content = grid;

        Activated += delegate { comboBoxDatabases.Focus(); };
        KeyDown += vm.KeyDown;
    }
}
