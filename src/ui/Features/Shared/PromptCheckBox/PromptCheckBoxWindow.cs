using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Nikse.SubtitleEdit.Logic;

namespace Nikse.SubtitleEdit.Features.Shared.PromptCheckBox;

/// <summary>
/// A yes/no option prompt in the editor look (see <see cref="EditorChrome"/>).
/// </summary>
public class PromptCheckBoxWindow : Window
{
    public PromptCheckBoxWindow(PromptCheckBoxViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Bind(TitleProperty, new Binding(nameof(vm.Title)));
        SizeToContent = SizeToContent.WidthAndHeight;
        MinWidth = 300;
        CanResize = false;
        EditorChrome.ApplyWindow(this, EditorWindowStyles.Picker);
        vm.Window = this;
        DataContext = vm;

        var checkBox = new CheckBox
        {
            [!ToggleButton.IsCheckedProperty] = new Binding(nameof(vm.IsChecked)) { Mode = BindingMode.TwoWay },
            [!ContentControl.ContentProperty] = new Binding(nameof(vm.CheckBoxText)),
        };
        AutomationProperties.SetName(checkBox, vm.CheckBoxText);

        var buttonOk = EditorChrome.AsPrimary(UiUtil.MakeButtonOk(vm.OkCommand));
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);

        var panel = new Border
        {
            Child = checkBox,
            Padding = new Thickness(16, 16, 16, 18),
        };

        Content = EditorChrome.MakeRoot(panel, EditorChrome.MakeButtonBar(null, buttonCancel, buttonOk));

        UiUtil.FocusOnFirstActivation(this, checkBox); // hack to make OnKeyDown work
        KeyDown += (_, e) => vm.OnKeyDown(e);
    }
}
