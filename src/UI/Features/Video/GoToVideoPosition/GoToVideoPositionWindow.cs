using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Layout;
using Nikse.SubtitleEdit.Controls;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Video.GoToVideoPosition;

public class GoToVideoPositionWindow : Window
{
    private readonly GoToVideoPositionViewModel _vm;

    public GoToVideoPositionWindow(GoToVideoPositionViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.Video.GoToVideoPosition;
        SizeToContent = SizeToContent.WidthAndHeight;
        CanResize = false;

        _vm = vm;
        vm.Window = this;
        DataContext = vm;

        vm.UpDown = new TimeCodeUpDown
        {
            VerticalAlignment = VerticalAlignment.Center,
            [!TimeCodeUpDown.ValueProperty] = new Binding(nameof(_vm.Time))
            {
                Mode = BindingMode.TwoWay,
            },
        };

        var panel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 15,
            Margin = new Thickness(10, 20, 10, 10),
            Children =
            {
                new Label
                {
                    Content = Se.Language.General.VideoPosition,
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
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        _vm.OnKeyDown(e);
    }
}