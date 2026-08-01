using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Layout;
using System.Collections;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.ValueConverters;

namespace Nikse.SubtitleEdit.Features.Shared.Bookmarks;

public class BookmarksListWindow : Window
{
    public BookmarksListWindow(BookmarksListViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.General.BookmarksList;
        CanResize = true;
        Width = 600;
        Height = 700;
        MinWidth = 600;
        MinHeight = 400;
        vm.Window = this;
        DataContext = vm;

        var buttonGoTo = UiUtil.MakeButton(Se.Language.General.GoTo, vm.GoToCommand).WithBindIsEnabled(nameof(vm.HasBookmarks));
        var buttonClear = UiUtil.MakeButton(Se.Language.General.Clear, vm.ClearCommand).WithBindIsEnabled(nameof(vm.HasBookmarks));
        var buttonCancel = UiUtil.MakeButtonDone(vm.CancelCommand);
        var panelButtons = UiUtil.MakeButtonBar(buttonGoTo, buttonClear, buttonCancel);

        var labelCount = new TextBlock
        {
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Bottom,
            Margin = new Thickness(0, 0, 0, 10),
        };
        labelCount.Bind(TextBlock.TextProperty, new Binding($"{nameof(vm.Subtitles)}.{nameof(vm.Subtitles.Count)}")
        {
            Source = vm,
            Mode = BindingMode.OneWay,
            StringFormat = Se.Language.General.Count + ": {0}",
        });

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

        grid.Add(MakeBookmarkGridView(vm), 0);
        grid.Add(labelCount, 1);
        grid.Add(panelButtons, 1);

        Content = grid;

        Activated += delegate { buttonCancel.Focus(); }; // hack to make OnKeyDown work
        KeyDown += (_, e) => vm.OnKeyDown(e);
    }

    private static Border MakeBookmarkGridView(BookmarksListViewModel vm)
    {
        var dataGrid = TableViewExtras.MakeTableView();
        dataGrid.Height = double.NaN; // auto size inside scroll viewer
        dataGrid.Margin = new Thickness(2);
        dataGrid.ItemsSource = vm.Subtitles;
        dataGrid.DataContext = vm.Subtitles;

        dataGrid.DoubleTapped += vm.OnBookmarksGridDoubleTapped;

        var fullTimeConverter = new TimeSpanToDisplayFullConverter();
        var shortTimeConverter = new TimeSpanToDisplayShortConverter();

        // Columns
        dataGrid.Columns.Add(new SeTableViewColumn
        {
            Header = Se.Language.General.NumberSymbol,
            Binding = new Binding(nameof(SubtitleLineViewModel.Number)),
            Width = new GridLength(50), // was Auto; TableView treats Auto as star
            CellTheme = UiUtil.TableViewCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
        });
        dataGrid.Columns.Add(new SeTableViewColumn
        {
            Header = Se.Language.General.Text,
            Binding = new Binding(nameof(SubtitleLineViewModel.Bookmark)),
            CellTheme = UiUtil.TableViewCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
            Width = new GridLength(1, GridUnitType.Star) // star sizing to take all available space
        });
        dataGrid.Columns.Add(new SeTableViewColumn
        {
            Header = Se.Language.General.Delete,
            CellTheme = UiUtil.TableViewNoPaddingCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
            CellTemplate = new FuncDataTemplate<SubtitleLineViewModel>((item, _) =>
            {
                var deleteButton = new Button
                {
                    Command = vm.DeleteSelectedLineCommand,
                    CommandParameter = item,
                    DataContext = vm,
                    Margin = new Thickness(2),
                };
                Optris.Icons.Avalonia.Attached.SetIcon(deleteButton, IconNames.Trash);
                return deleteButton;
            }),
            Width = new GridLength(80) // was Auto; TableView treats Auto as star
        });

        dataGrid.Bind(TableView.SelectedItemProperty, new Binding(nameof(vm.SelectedSubtitle))
        {
            Source = vm,
            Mode = BindingMode.TwoWay
        });
        dataGrid.SelectionChanged += vm.GridSelectionChanged;
        dataGrid.DoubleTapped += (s, e) => vm.GoToCommand.Execute(null);    
        dataGrid.KeyDown += (s, e) => vm.GridKeyDown(e);
        dataGrid.AddHandler(InputElement.KeyDownEvent, (object? _, KeyEventArgs e) =>
        {
            if (e.Key is Key.Home or Key.End && dataGrid.ItemsSource is IList items && items.Count > 0)
            {
                var target = e.Key == Key.Home ? items[0] : items[^1];
                dataGrid.SelectedItem = target;
                if (target != null)
                {
                    dataGrid.ScrollIntoView(target);
                }

                e.Handled = true;
            }
        }, Avalonia.Interactivity.RoutingStrategies.Tunnel);

        var flyout = new MenuFlyout();
        var deleteMenuItem = new MenuItem 
        { 
            Header = Se.Language.General.Delete,
            Command = vm.DeleteSelectedLineCommand,
            [!MenuItem.CommandParameterProperty] = new Binding(nameof(vm.SelectedSubtitle))
            {
                Source = vm
            }
        };
        flyout.Items.Add(deleteMenuItem);
        dataGrid.ContextFlyout = flyout;
        UiUtil.AttachMacContextFlyoutHandler(dataGrid);

        return UiUtil.MakeBorderForControlNoPadding(dataGrid);
    }
}
