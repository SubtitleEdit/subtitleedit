using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Layout;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Shared.GoToLineNumber;

public class GoToLineNumberWindow : Window
{
    public GoToLineNumberWindow(GoToLineNumberViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.General.GoToLineNumber;
        SizeToContent = SizeToContent.WidthAndHeight;
        CanResize = false;
        vm.Window = this;
        DataContext = vm;

        vm.UpDown = new NumericUpDown
        {
            VerticalAlignment = VerticalAlignment.Center,
            Width = 150,
            VerticalContentAlignment = VerticalAlignment.Center,
            [!NumericUpDown.ValueProperty] = new Binding(nameof(vm.LineNumber))
            {
                Mode = BindingMode.TwoWay,
            },
            [!NumericUpDown.MaximumProperty] = new Binding(nameof(vm.MaxLineNumber))
            {
                Mode = BindingMode.OneWay,
            },
            Minimum = 1,
            Increment = 1,          // Only step in whole numbers
            FormatString = "F0",    // Show 0 decimal places
        };
        vm.UpDown.KeyDown += (sender, args) => vm.OnKeyDown(args);

        var panel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 15,
            Margin = new Thickness(10, 20, 10, 10),
            Children =
            {
                new Label
                {
                    Content = Se.Language.General.GoToLineNumber,
                    VerticalAlignment = VerticalAlignment.Center,
                },
                vm.UpDown,
            }
        };

        var buttonPanel = UiUtil.MakeButtonBar(
            UiUtil.MakeButtonOk(vm.OkCommand),
            UiUtil.MakeButtonCancel(vm.CancelCommand));

        var contentPanel = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 15,
            Margin = UiUtil.MakeWindowMargin(),
            Children =
            {
                panel,
                buttonPanel,
            }
        };

        Content = contentPanel;

        Activated += delegate { vm.Activated(); };
        KeyDown += (_, args) => vm.OnKeyDown(args);
    }
}