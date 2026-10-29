using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.Media;
using Nikse.SubtitleEdit.Logic;

namespace Nikse.SubtitleEdit.Features.Shared.PromptTextBox;

/// <summary>
/// A text prompt in the editor look (see <see cref="EditorChrome"/>). Bare Space in the text
/// box types a space (#15028) - no window key handling may take it.
/// </summary>
public class PromptTextBoxWindow : Window
{
    public PromptTextBoxWindow(PromptTextBoxViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Bind(TitleProperty, new Binding(nameof(vm.Title)));
        SizeToContent = SizeToContent.WidthAndHeight;
        CanResize = false;
        EditorChrome.ApplyWindow(this, EditorWindowStyles.Picker);
        vm.Window = this;
        DataContext = vm;

        var textBox = new TextBox
        {
            [!TextBox.WidthProperty] = new Binding(nameof(vm.TextBoxWidth)) { Mode = BindingMode.TwoWay },
            [!TextBox.HeightProperty] = new Binding(nameof(vm.TextBoxHeight)) { Mode = BindingMode.TwoWay },
            [Avalonia.Automation.AutomationProperties.NameProperty] = Nikse.SubtitleEdit.Logic.Config.Se.Language.General.Text,
            [!TextBox.TextProperty] = new Binding(nameof(vm.Text)) { Mode = BindingMode.TwoWay },
            [!TextBox.IsReadOnlyProperty] = new Binding(nameof(vm.IsReadOnly)) { Mode = BindingMode.TwoWay },
            VerticalAlignment = VerticalAlignment.Center,
            TextWrapping = TextWrapping.Wrap,
        };

        textBox.Classes.Add("text");
        textBox.VerticalContentAlignment = VerticalAlignment.Top;

        var buttonExtra = UiUtil.MakeButton(string.Empty, vm.ExtraButtonCommand)
            .WithBindIsVisible(nameof(vm.IsExtraButtonVisible));
        buttonExtra.Bind(ContentControl.ContentProperty, new Binding(nameof(vm.ExtraButtonText)));
        var buttonOk = EditorChrome.AsPrimary(UiUtil.MakeButtonOk(vm.OkCommand));
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);

        var panel = new Border
        {
            Child = textBox,
            Padding = new Thickness(16),
        };

        Content = EditorChrome.MakeRoot(panel, EditorChrome.MakeButtonBar(buttonExtra, buttonCancel, buttonOk));

        UiUtil.FocusOnFirstActivation(this, textBox); // hack to make OnKeyDown work
        KeyDown += (_, e) => vm.OnKeyDown(e);
    }
}
