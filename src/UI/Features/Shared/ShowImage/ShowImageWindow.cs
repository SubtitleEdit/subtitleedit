using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.Media;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Shared.ShowImage;

public class ShowImageWindow : Window
{
    public ShowImageWindow(ShowImageViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Bind(TitleProperty, new Binding(nameof(vm.Title)));
        Width = 1000;
        Height = 760;
        MinWidth = 400;
        MinHeight = 300;
        CanResize = true;
        vm.Window = this;
        DataContext = vm;

        var image = new Image
        {
            [!Image.SourceProperty] = new Binding(nameof(vm.PreviewImage)),
            DataContext = vm,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Top,
            Stretch = Stretch.Uniform,
        };

        var flyout = new MenuFlyout();
        image.ContextFlyout = flyout;

        var menuItemDelete = new MenuItem
        {
            Header = Se.Language.General.CopyImageToClipboard,
            DataContext = vm,
            Command = vm.CopyImageToClipboardCommand,
        };
        flyout.Items.Add(menuItemDelete);

        var menuItemClear = new MenuItem
        {
            Header = Se.Language.General.SaveImageAsDotDotDot,
            DataContext = vm,
            Command = vm.SaveImageAsCommand,
        };
        flyout.Items.Add(menuItemClear);


        var label = UiUtil.MakeLabel().WithBindText(vm, nameof(vm.Text)).WithFontSize(30).WithMarginTop(20).WithAlignmentCenter();

        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        var buttonPanel = UiUtil.MakeButtonBar(buttonOk);

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
            Margin = UiUtil.MakeWindowMargin(),
            ColumnSpacing = 10,
            RowSpacing = 10,
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        grid.Add(image, 0);
        grid.Add(label, 1);
        grid.Add(buttonPanel, 2);

        Content = grid;

        Loaded += delegate { buttonOk.Focus(); vm.Loaded(); }; // hack to make OnKeyDown work
        Closing += (_, e) => vm.Closing(e);
        KeyDown += (_, e) => vm.OnKeyDown(e);
    }
}
