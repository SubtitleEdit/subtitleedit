using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Layout;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.ValueConverters;

namespace Nikse.SubtitleEdit.Features.Video.ShotChanges;

public class ShotChangeListWindow : Window
{
    private readonly ShotChangeListViewModel _vm;

    public ShotChangeListWindow(ShotChangeListViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.Video.ShotChanges.ShotChangesList;
        CanResize = true;
        Width = 600;
        Height = 700;
        MinWidth = 600;
        MinHeight = 400;

        _vm = vm;
        vm.Window = this;
        DataContext = vm;

        EditorChrome.ApplyWindow(this, EditorWindowStyles.VideoTool);

        var buttonGoTo = EditorChrome.CreateToolButton("fa-solid fa-location-arrow", Se.Language.General.GoTo, vm.GoToCommand, 26)
            .WithBindIsEnabled(nameof(vm.HasShotChanges));
        var buttonExport = EditorChrome.CreateToolButton("fa-solid fa-file-export", Se.Language.General.ExportDotDotDot, vm.ExportCommand, 26)
            .WithBindIsEnabled(nameof(vm.HasShotChanges));
        var buttonClear = EditorChrome.CreateToolButton("fa-solid fa-trash", Se.Language.General.Clear, vm.ClearCommand, 26)
            .WithBindIsEnabled(nameof(vm.HasShotChanges));
        var actions = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 2, VerticalAlignment = VerticalAlignment.Center };
        actions.Children.Add(buttonGoTo);
        actions.Children.Add(buttonExport);
        actions.Children.Add(EditorChrome.MakeToolbarSeparator());
        actions.Children.Add(buttonClear);

        var buttonCancel = UiUtil.MakeButtonDone(vm.CancelCommand).AsPrimary();

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
            },
        };

        grid.Add(new Border
        {
            Background = EditorChrome.PanelBrush,
            Child = EditorChrome.MakeListPane(Se.Language.Video.ShotChanges.ShotChangesList, MakeBookmarkGridView(vm), actions),
        }, 0);
        grid.Add(EditorChrome.MakeButtonBar(null, buttonCancel), 1);

        Content = grid;

        UiUtil.FocusOnFirstActivation(this, buttonCancel); // hack to make OnKeyDown work
    }

    private static Border MakeBookmarkGridView(ShotChangeListViewModel vm)
    {
        var tableView = TableViewExtras.MakeTableView();
        tableView.Height = double.NaN; // auto size inside scroll viewer
        tableView.Margin = new Thickness(2);
        tableView.ItemsSource = vm.ShotChanges;
        tableView.DataContext = vm;

        tableView.DoubleTapped += vm.OnShotChangeGridDoubleTapped;

        // Columns
        tableView.Columns.Add(new SeTableViewColumn
        {
            Header = Se.Language.General.NumberSymbol,
            Binding = new Binding(nameof(ShotChangeItem.Index)),
            Width = new GridLength(60),
            CellTheme = UiUtil.TableViewCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
        });
        tableView.Columns.Add(new SeTableViewColumn
        {
            Header = Se.Language.General.Text,
            Binding = new Binding(nameof(ShotChangeItem.TimeText)),
            Width = new GridLength(1, GridUnitType.Star), // star sizing to take all available space
            CellTheme = UiUtil.TableViewCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
        });
        TableViewExtras.BindSelectedItem(tableView, vm, nameof(vm.SelectedShotChange));
        tableView.DoubleTapped += (s, e) => vm.GoToCommand.Execute(null);
        tableView.KeyDown += (s, e) => vm.GridKeyDown(e);
        tableView.KeyDown += async (s, e) =>
        {
            if (e.Key == Key.C && (e.KeyModifiers.HasFlag(KeyModifiers.Control) || e.KeyModifiers.HasFlag(KeyModifiers.Meta)))
            {
                e.Handled = true;
                await vm.CopyToClipboard(GetSelected(tableView));
            }
        };
        tableView.AddHandler(InputElement.KeyDownEvent, (object? _, KeyEventArgs e) =>
        {
            if (e.Key is Key.Home or Key.End && tableView.ItemsSource is IList items && items.Count > 0)
            {
                var target = e.Key == Key.Home ? items[0] : items[^1];
                if (target != null)
                {
                    tableView.SelectedItem = target;
                    tableView.ScrollIntoView(target);
                }

                e.Handled = true;
            }
        }, Avalonia.Interactivity.RoutingStrategies.Tunnel);

        var flyout = new MenuFlyout();
        var copyMenuItem = new MenuItem { Header = Se.Language.General.Copy };
        copyMenuItem.Click += async (_, _) => await vm.CopyToClipboard(GetSelected(tableView));
        flyout.Items.Add(copyMenuItem);
        var deleteMenuItem = new MenuItem 
        { 
            Header = Se.Language.General.Delete,
            Command = vm.DeleteSelectedLineCommand,
            [!MenuItem.CommandParameterProperty] = new Binding(nameof(vm.SelectedShotChange))
            {
                Source = vm
            }
        };
        flyout.Items.Add(deleteMenuItem);
        tableView.ContextFlyout = flyout;
        UiUtil.AttachMacContextFlyoutHandler(tableView);

        tableView.WithAccessibleName(Se.Language.Video.ShotChanges.ShotChangesList);
        return EditorChrome.MakeListFrame(tableView);
    }

    private static List<ShotChangeItem> GetSelected(TableView tableView)
    {
        return tableView.SelectedItems?.OfType<ShotChangeItem>().ToList() ?? new List<ShotChangeItem>();
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        _vm.OnKeyDown(e);
    }
}
