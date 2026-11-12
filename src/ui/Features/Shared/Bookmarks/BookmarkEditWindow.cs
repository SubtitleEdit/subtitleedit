using Avalonia;
using Avalonia.Controls;
using Avalonia.Data.Converters;
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
        Closing += delegate { UiUtil.SaveWindowPosition(this); };
        Loaded += delegate { UiUtil.RestoreWindowPosition(this); };
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
            AcceptsReturn = true,
            Watermark = Se.Language.General.BookmarkNewLineHint,
        };
        textBox.Classes.Add("text");
        textBox.Bind(TextBox.TextProperty, new Avalonia.Data.Binding(nameof(vm.BookmarkText)) { Mode = Avalonia.Data.BindingMode.TwoWay });
        Avalonia.Automation.AutomationProperties.SetName(textBox, Se.Language.General.Text);

        // Tunnel: with AcceptsReturn the TextBox would insert the line break before a bubbling
        // handler sees Enter. Plain Enter is OK, Shift+Enter is a new line in the note.
        textBox.AddHandler(KeyDownEvent, (_, args) => vm.OnTextBoxKeyDown(args), Avalonia.Interactivity.RoutingStrategies.Tunnel);

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

        var colorPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 6,
            Margin = new Thickness(0, 0, 0, 10),
        };
        string[] colorNames =
        [
            Se.Language.Options.Settings.BookmarkColor,
            Se.Language.Options.Settings.BookmarkColor2,
            Se.Language.Options.Settings.BookmarkColor3,
        ];
        for (var i = 0; i < BookmarkColors.Count; i++)
        {
            colorPanel.Children.Add(MakeColorButton(vm, i, colorNames[i]));
        }

        var bodyPanel = new DockPanel();
        DockPanel.SetDock(colorPanel, Dock.Top);
        bodyPanel.Children.Add(colorPanel);
        bodyPanel.Children.Add(textBox);

        var body = new Border
        {
            Child = bodyPanel,
            Padding = new Thickness(14),
        };

        Content = EditorChrome.MakeRoot(body, EditorChrome.MakeDockedButtonBar(leftActions, buttonCancel, buttonOk));

        UiUtil.FocusOnFirstActivation(this, textBox); // hack to make OnKeyDown work
        KeyDown += (_, e) => vm.OnKeyDown(e);
    }

    /// <summary>
    /// A button showing the bookmark icon in color <paramref name="index"/>, ringed when the
    /// bookmark uses it. Not a ToggleButton: its checked accent background hid a blue swatch.
    /// </summary>
    private static Border MakeColorButton(BookmarkEditViewModel vm, int index, string name)
    {
        var button = new Button
        {
            Width = 34,
            Height = 28,
            Padding = new Thickness(0),
            HorizontalContentAlignment = HorizontalAlignment.Center,
            VerticalContentAlignment = VerticalAlignment.Center,
            Content = new Optris.Icons.Avalonia.Icon
            {
                Value = IconNames.Bookmark,
                FontSize = 18,
                Foreground = new SolidColorBrush(BookmarkColors.Get(index)),
            },
        };
        button.Click += (_, _) => vm.ColorIndex = index;
        Avalonia.Automation.AutomationProperties.SetName(button, name);
        if (Se.Settings.Appearance.ShowHints)
        {
            ToolTip.SetTip(button, name);
        }

        return new Border
        {
            BorderThickness = new Thickness(2),
            CornerRadius = new CornerRadius(6),
            Padding = new Thickness(1),
            Child = button,
            [!Border.BorderBrushProperty] = new Avalonia.Data.Binding(nameof(vm.ColorIndex))
            {
                Converter = new FuncValueConverter<int, IBrush>(selected => selected == index ? EditorChrome.StrongTextBrush : Brushes.Transparent),
            },
        };
    }
}
