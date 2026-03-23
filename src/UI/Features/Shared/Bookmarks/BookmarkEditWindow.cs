using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Shared.Bookmarks;

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
        vm.Window = this;
        DataContext = vm;

        var textBox = new TextBox
        {
            VerticalAlignment = VerticalAlignment.Stretch,  
            HorizontalAlignment = HorizontalAlignment.Stretch,
            TextWrapping = TextWrapping.Wrap,
        };
        textBox.Bind(TextBox.TextProperty, new Avalonia.Data.Binding(nameof(vm.BookmarkText)) { Mode = Avalonia.Data.BindingMode.TwoWay }); 
        textBox.KeyDown += (sender, args) => vm.OnTextBoxKeyDown(args);

        var buttonList = UiUtil.MakeButton(Se.Language.General.BookmarksList, vm.ListCommand).WithBindIsVisible(nameof(vm.ShowListButton));
        var buttonRemove = UiUtil.MakeButton(Se.Language.General.Remove, vm.DeleteCommand).WithBindIsVisible(nameof(vm.ShowRemoveButton));
        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);
        var panelButtons = UiUtil.MakeButtonBar(buttonList, buttonRemove, buttonOk, buttonCancel);

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
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

        grid.Add(textBox, 0);
        grid.Add(panelButtons, 1);

        Content = grid;

        Activated += delegate { textBox.Focus(); }; // hack to make OnKeyDown work
        KeyDown += (_, e) => vm.OnKeyDown(e);
    }
}
