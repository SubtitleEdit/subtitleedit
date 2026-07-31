using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using System.Collections;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Shared.PickLayerFilter;

public class PickLayerFilterWindow : Window
{
    public PickLayerFilterWindow(PickLayerFilterViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.Tools.FilterLayersTitle;
        CanResize = true;
        Width = 800;
        Height = 700;
        MinWidth = 500;
        MinHeight = 400;
        vm.Window = this;
        DataContext = vm;


        var settingsView = MakeSettings(vm);

        var fontsView = MakeLayersView(vm);

        var buttonRemoveFilter = UiUtil.MakeButton(Se.Language.General.RemoveFilter, vm.RemoveFilterCommand);
        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);
        var buttonPanel = UiUtil.MakeButtonBar(buttonRemoveFilter, buttonOk, buttonCancel);

        var buttonSelectAll = UiUtil.MakeButton(Se.Language.General.SelectAll, vm.SelectAllCommand);
        var buttonInverseSelection = UiUtil.MakeButton(Se.Language.General.InvertSelection, vm.InvertSelectionCommand);
        var panelButtonTools = UiUtil.MakeHorizontalPanel(buttonSelectAll, buttonInverseSelection);

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(2, GridUnitType.Star) },
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

        grid.Add(settingsView, 0);
        grid.Add(fontsView, 1);
        grid.Add(panelButtonTools, 2);
        grid.Add(buttonPanel, 2);

        Content = grid;

        Loaded += delegate 
        { 
            buttonOk.Focus();
            vm.Loaded();
        }; // hack to make OnKeyDown work
        KeyDown += vm.KeyDown;
    }

    private static Border MakeSettings(PickLayerFilterViewModel vm)
    {
        var checkBoxHideFromWaveform = UiUtil.MakeCheckBox(Se.Language.Tools.FilterLayersHideFromWaveform, vm, nameof(vm.HideFromWaveform));
        var checkBoxHideFromGridView = UiUtil.MakeCheckBox(Se.Language.Tools.FilterLayersHideFromSubtitleGrid, vm, nameof(vm.HideFromGridView));
        var checkBoxHideFromVideoPreview = UiUtil.MakeCheckBox(Se.Language.Tools.FilterLayersHideFromVideoPreview, vm, nameof(vm.HideFromVideoPreview));

        var panel = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 5,
            Children =
            {
                checkBoxHideFromWaveform,
                checkBoxHideFromGridView,
                checkBoxHideFromVideoPreview,
            }
        };

        return UiUtil.MakeBorderForControl(panel);
    }

    private static Border MakeLayersView(PickLayerFilterViewModel vm)
    {
        var dataGrid = TableViewExtras.MakeTableView(multiSelect: false);
        dataGrid.Width = double.NaN;
        dataGrid.Height = double.NaN;
        dataGrid.DataContext = vm;
        dataGrid.ItemsSource = vm.Layers;

        var visibleColumn = new SeTableViewColumn
        {
            Header = Se.Language.General.Visible,
            CellTheme = UiUtil.TableViewNoPaddingCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
            CellTemplate = new FuncDataTemplate<LayerItem>((item, _) =>
            new Border
            {
                Background = Brushes.Transparent, // Prevents highlighting
                Padding = new Thickness(4),
                Child = new CheckBox
                {
                    Focusable = false,
                    [!ToggleButton.IsCheckedProperty] = new Binding(nameof(LayerItem.IsSelected)),
                    HorizontalAlignment = HorizontalAlignment.Center
                }
            }),
            // Content-sized (Auto) on the DataGrid; TableView treats Auto as star.
            Width = new GridLength(80),
        };
        dataGrid.Columns.Add(visibleColumn);

        var layerColumn = new SeTableViewColumn
        {
            Header = Se.Language.General.Layer,
            CellTheme = UiUtil.TableViewCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
            Binding = new Binding(nameof(LayerItem.Layer)),
            Width = new GridLength(100),
        };
        dataGrid.Columns.Add(layerColumn);

        var usagesColumn = new SeTableViewColumn
        {
            Header = Se.Language.General.Usages,
            CellTheme = UiUtil.TableViewCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
            Binding = new Binding(nameof(LayerItem.UsageCount)),
            Width = new GridLength(1, GridUnitType.Star),
        };
        dataGrid.Columns.Add(usagesColumn);

        dataGrid.Bind(TableView.SelectedItemProperty, new Binding(nameof(vm.SelectedLayer)));
        vm.LayerGrid = dataGrid;
        dataGrid.KeyDown += (s, e) => vm.LayerGridKeyDown(e);

        // Space toggles the checkbox (tunnel handler, so it runs before the ListBox
        // machinery can swallow the key).
        TableViewExtras.AddSpaceToggle<LayerItem>(dataGrid,
            item => item.IsSelected, (item, v) => item.IsSelected = v);

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

        // Layer list order is presentation-only: OK collects the checked layers as a
        // set of layer numbers, so in-place sorting is safe.
        new TableViewHeaderSorter(dataGrid)
            .AddSortable<LayerItem, int>(layerColumn, x => x.Layer)
            .AddSortable<LayerItem, int>(usagesColumn, x => x.UsageCount);

        return UiUtil.MakeBorderForControlNoPadding(dataGrid);
    }
}
