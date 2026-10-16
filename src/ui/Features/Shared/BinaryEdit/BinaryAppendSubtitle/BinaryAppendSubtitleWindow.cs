using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Layout;
using Nikse.SubtitleEdit.Controls;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Shared.BinaryEdit.BinaryAppendSubtitle;

/// <summary>
/// Append an image-based subtitle, in the editor look (see <see cref="EditorChrome"/>): shift the
/// appended time codes (by an offset) or keep them, Cancel/OK at the bottom.
/// </summary>
public class BinaryAppendSubtitleWindow : Window
{
    public BinaryAppendSubtitleWindow(BinaryAppendSubtitleViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.Tools.ImageBasedEdit.AppendSubtitleDotDotDot;
        SizeToContent = SizeToContent.WidthAndHeight;
        CanResize = false;
        BinaryEditChrome.Apply(this);
        vm.Window = this;
        DataContext = vm;

        var radioAppend = new RadioButton
        {
            Content = Se.Language.Tools.ImageBasedEdit.ShiftTimeCodes,
            FontSize = 12.5,
            [!RadioButton.IsCheckedProperty] = new Binding(nameof(vm.AppendTimeCodes)) { Mode = BindingMode.TwoWay },
        };

        var timeCodeUpDown = new TimeCodeUpDown
        {
            Margin = new Thickness(28, 0, 0, 6),
            HorizontalAlignment = HorizontalAlignment.Left,
            [!TimeCodeUpDown.ValueProperty] = new Binding(nameof(vm.TimeOffset)) { Mode = BindingMode.TwoWay },
            [!IsVisibleProperty] = new Binding(nameof(vm.IsTimeOffsetVisible)),
        };
        AutomationProperties.SetName(timeCodeUpDown, Se.Language.Tools.ImageBasedEdit.ShiftTimeCodes);

        var radioKeep = new RadioButton
        {
            Content = Se.Language.Tools.JoinSubtitles.KeepTimeCodes,
            FontSize = 12.5,
            [!RadioButton.IsCheckedProperty] = new Binding(nameof(vm.KeepTimeCodes)) { Mode = BindingMode.TwoWay },
        };

        var panel = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 2,
            Children =
            {
                radioAppend,
                timeCodeUpDown,
                radioKeep,
            },
        };
        var section = EditorChrome.MakeEditorSection(Se.Language.General.Timing, panel, first: true);
        section.MinWidth = 320;

        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);
        Content = EditorChrome.MakeRoot(section, EditorChrome.MakeCancelOkBar(null, buttonCancel, buttonOk));

        UiUtil.FocusOnFirstActivation(this, radioAppend); // initial focus on an input, not an action button - a focused button clicks on bare Space
        KeyDown += (_, e) => vm.OnKeyDown(e);
    }
}
