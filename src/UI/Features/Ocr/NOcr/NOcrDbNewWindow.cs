using Avalonia.Controls;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Ocr.NOcr;

public class NOcrDbNewWindow : Window
{
    public NOcrDbNewWindow(NOcrDbNewViewModel vm)
    {
        Title = "New/rename nOCR database";
        vm.Window = this;
        UiUtil.InitializeWindow(this, GetType().Name);
        SizeToContent = SizeToContent.WidthAndHeight;
        CanResize = false;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        DataContext = vm;

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) }, // Select action
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) }, // Buttons
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
            Margin = UiUtil.MakeWindowMargin(),
            ColumnSpacing = 10,
            Width = double.NaN,
            Height = double.NaN,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Stretch,
        };

        var labelTitle = UiUtil.MakeLabel(Se.Language.Ocr.EditNOcrDatabase);
        var textBoxDatabaseName = UiUtil.MakeTextBox(200, vm, nameof(vm.DatabaseName));

        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);
        var buttonBar = UiUtil.MakeButtonBar(buttonOk, buttonCancel);

        grid.Add(labelTitle, 0, 0);
        grid.Add(textBoxDatabaseName, 0, 1);
        grid.Add(buttonBar, 1, 0, 1, 2);

        Content = grid;

        Activated += delegate
        {
            textBoxDatabaseName.Focus(); // hack to make OnKeyDown work
        };

        textBoxDatabaseName.KeyDown += vm.TextBoxDatabaseNameKeyDown;
        KeyDown += (_, e) => vm.KeyDown(e);
        Loaded += (_, _) => Title = vm.Title;
    }
}
