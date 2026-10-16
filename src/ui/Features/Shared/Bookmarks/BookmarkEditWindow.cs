using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Shared.Bookmarks;

/// <summary>
/// Add/edit a bookmark text, in the editor look (see <see cref="EditorChrome"/>): the text, the
/// list/remove actions on the left of the button bar, Cancel and OK on the right.
/// </summary>
public class BookmarkEditWindow : Window
{
    public BookmarkEditWindow(BookmarkEditViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Bind(TitleProperty, new Avalonia.Data.Binding(nameof(vm.Title)));
        CanResize = true;
        Width = 600;
        Height = 400;
        MinWidth = 500;
        MinHeight = 300;
        EditorChrome.ApplyWindow(this, EditorWindowStyles.Dialog);
        vm.Window = this;
        DataContext = vm;

        var textBox = new TextBox
        {
            VerticalAlignment = VerticalAlignment.Stretch,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalContentAlignment = VerticalAlignment.Top,
            TextWrapping = TextWrapping.Wrap,
        };
        textBox.Classes.Add("text");
        textBox.Bind(TextBox.TextProperty, new Avalonia.Data.Binding(nameof(vm.BookmarkText)) { Mode = Avalonia.Data.BindingMode.TwoWay });
        Avalonia.Automation.AutomationProperties.SetName(textBox, Se.Language.General.Text);
        textBox.KeyDown += (sender, args) => vm.OnTextBoxKeyDown(args);

        var buttonList = EditorChrome.MakeTextToolButton("fa-solid fa-list", Se.Language.General.BookmarksList, vm.ListCommand).WithBindIsVisible(nameof(vm.ShowListButton));
        var buttonRemove = EditorChrome.MakeTextToolButton("fa-solid fa-trash-can", Se.Language.General.Remove, vm.DeleteCommand).WithBindIsVisible(nameof(vm.ShowRemoveButton));
        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        buttonOk.Classes.Add("primary");
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);

        var leftActions = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 4,
            VerticalAlignment = VerticalAlignment.Center,
            Children = { buttonList, buttonRemove },
        };

        var body = new Border
        {
            Child = textBox,
            Padding = new Thickness(14),
        };

        Content = EditorChrome.MakeRoot(body, EditorChrome.MakeDockedButtonBar(leftActions, buttonCancel, buttonOk));

        UiUtil.FocusOnFirstActivation(this, textBox); // hack to make OnKeyDown work
        KeyDown += (_, e) => vm.OnKeyDown(e);
    }
}
