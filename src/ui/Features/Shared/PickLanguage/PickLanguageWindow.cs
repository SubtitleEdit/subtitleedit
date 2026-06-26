using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.Styling;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Shared.PickLanguage;

public class PickLanguageWindow : Window
{
    public PickLanguageWindow(PickLanguageViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.Options.Settings.FavoriteLanguages;
        CanResize = true;
        Width = 500;
        Height = 600;
        MinWidth = 350;
        MinHeight = 400;
        vm.Window = this;
        DataContext = vm;

        var labelSearch = UiUtil.MakeLabel(Se.Language.General.Search);
        var textBoxSearch = new TextBox
        {
            Margin = new Thickness(5, 0, 0, 0),
            Width = 250,
        };
        textBoxSearch.Bind(TextBox.TextProperty, new Binding(nameof(vm.SearchText)) { Source = vm });
        textBoxSearch.TextChanged += (_, _) => vm.SearchTextChanged();

        var panelSearch = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Left,
            Margin = new Thickness(0, 0, 0, 10),
            Children =
            {
                labelSearch,
                textBoxSearch,
            }
        };

        var listBoxLanguages = new ListBox
        {
            Height = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
        };
        listBoxLanguages.Styles.Add(new Style(x => x.OfType<ListBoxItem>())
        {
            Setters =
            {
                new Setter(ListBoxItem.PaddingProperty, new Thickness(4, 2)),
                new Setter(ListBoxItem.MarginProperty, new Thickness(0)),
            }
        });
        listBoxLanguages.Bind(ItemsControl.ItemsSourceProperty, new Binding(nameof(vm.Languages)));
        listBoxLanguages.Bind(Avalonia.Controls.Primitives.SelectingItemsControl.SelectedItemProperty, new Binding(nameof(vm.SelectedLanguage)) { Mode = BindingMode.TwoWay });
        listBoxLanguages.DoubleTapped += (_, _) => vm.OkCommand.Execute(null);

        var listBoxBorder = UiUtil.MakeBorderForControl(listBoxLanguages);

        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);
        var buttonPanel = UiUtil.MakeButtonBar(buttonOk, buttonCancel);

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },  // Search
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },  // ListBox
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },  // Buttons
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
            Margin = UiUtil.MakeWindowMargin(),
            ColumnSpacing = 10,
            RowSpacing = 0,
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        grid.Add(panelSearch, 0);
        grid.Add(listBoxBorder, 1);
        grid.Add(buttonPanel, 2);

        Content = grid;

        Activated += delegate { textBoxSearch.Focus(); };
        KeyDown += (_, e) => vm.OnKeyDown(e);
    }
}
