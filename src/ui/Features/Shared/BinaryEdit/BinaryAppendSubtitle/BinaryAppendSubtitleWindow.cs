using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Layout;
using Nikse.SubtitleEdit.Controls;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Shared.BinaryEdit.BinaryAppendSubtitle;

public class BinaryAppendSubtitleWindow : Window
{
    public BinaryAppendSubtitleWindow(BinaryAppendSubtitleViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.Tools.ImageBasedEdit.AppendSubtitleDotDotDot;
        SizeToContent = SizeToContent.WidthAndHeight;
        CanResize = false;
        vm.Window = this;
        DataContext = vm;

        var radioAppend = new RadioButton
        {
            Content = Se.Language.Tools.JoinSubtitles.AppendTimeCodes,
            [!RadioButton.IsCheckedProperty] = new Binding(nameof(vm.AppendTimeCodes)) { Mode = BindingMode.TwoWay },
            Margin = new Thickness(0, 0, 0, 4),
        };

        var timeCodeUpDown = new TimeCodeUpDown
        {
            Margin = new Thickness(20, 0, 0, 8),
            [!TimeCodeUpDown.ValueProperty] = new Binding(nameof(vm.TimeOffset)) { Mode = BindingMode.TwoWay },
            [!IsVisibleProperty] = new Binding(nameof(vm.IsTimeOffsetVisible)),
        };

        var radioKeep = new RadioButton
        {
            Content = Se.Language.Tools.JoinSubtitles.KeepTimeCodes,
            [!RadioButton.IsCheckedProperty] = new Binding(nameof(vm.KeepTimeCodes)) { Mode = BindingMode.TwoWay },
        };

        var panel = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Margin = UiUtil.MakeWindowMargin(),
            Children =
            {
                radioAppend,
                timeCodeUpDown,
                radioKeep,
            },
        };

        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);
        var buttonBar = UiUtil.MakeButtonBar(buttonOk, buttonCancel);

        var root = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Children = { panel, buttonBar },
        };

        Content = root;

        Activated += delegate { buttonOk.Focus(); };
        KeyDown += (_, e) => vm.OnKeyDown(e);
    }
}
