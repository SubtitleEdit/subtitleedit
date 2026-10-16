using Avalonia;
using Avalonia.Automation;
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

/// <summary>
/// The bookmarks of the subtitle, in the editor look (see <see cref="EditorChrome"/>): the list in a
/// frame, count and Clear on the left of the button bar, Go to and Done on the right.
/// </summary>
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
        EditorChrome.ApplyWindow(this, EditorWindowStyles.Dialog);
        vm.Window = this;
        DataContext = vm;

        var buttonGoTo = UiUtil.MakeButton(Se.Language.General.GoTo, vm.GoToCommand).WithBindIsEnabled(nameof(vm.HasBookmarks));
        var buttonClear = EditorChrome.MakeTextToolButton("fa-solid fa-trash-can", Se.Language.General.Clear, vm.ClearCommand).WithBindIsEnabled(nameof(vm.HasBookmarks));
        var buttonCancel = UiUtil.MakeButtonDone(vm.CancelCommand);
        buttonCancel.Classes.Add("primary");

        var labelCount = new TextBlock
        {
            FontSize = 12,
            Foreground = EditorChrome.DimTextBrush,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 0, 12, 0),
        };
        labelCount.Bind(TextBlock.TextProperty, new Binding($"{nameof(vm.Subtitles)}.{nameof(vm.Subtitles.Count)}")
        {
            Source = vm,
            Mode = BindingMode.OneWay,
            StringFormat = Se.Language.General.Count + ": {0}",
        });

        var left = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            VerticalAlignment = VerticalAlignment.Center,
            Children = { labelCount, buttonClear },
        };

        var list = new Border
        {
            Child = MakeBookmarkGridView(vm),
            Padding = new Thickness(14),
        };

        Content = EditorChrome.MakeRoot(list, EditorChrome.MakeDockedButtonBar(left, buttonGoTo, buttonCancel));

        UiUtil.FocusOnFirstActivation(this, buttonCancel); // hack to make OnKeyDown work
        KeyDown += (_, e) => vm.OnKeyDown(e);
    }

    private static Border MakeBookmarkGridView(BookmarksListViewModel vm)
    {
        var dataGrid = TableViewExtras.MakeTableView();
        dataGrid.Height = double.NaN; // auto size inside scroll viewer
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
                    Width = 26,
                    Height = 26,
                    Margin = new Thickness(2),
                    HorizontalAlignment = HorizontalAlignment.Center,
                    HorizontalContentAlignment = HorizontalAlignment.Center,
                    VerticalContentAlignment = VerticalAlignment.Center,
                };
                deleteButton.Classes.Add("tool");
                Optris.Icons.Avalonia.Attached.SetIcon(deleteButton, IconNames.Trash);
                AutomationProperties.SetName(deleteButton, Se.Language.General.Delete);
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

        AutomationProperties.SetName(dataGrid, Se.Language.General.BookmarksList);

        return EditorChrome.MakeListFrame(dataGrid, clearListBackground: true);
    }
}
