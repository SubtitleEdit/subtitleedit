using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Layout;
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
        EditorChrome.ApplyWindow(this, EditorWindowStyles.Picker);
        vm.Window = this;
        DataContext = vm;

        var settingsView = MakeSettings(vm);

        // Check all / invert on the list header, like the other check lists
        var actions = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 2 };
        actions.Children.Add(EditorChrome.CreateToolButton("fa-solid fa-square-check", Se.Language.General.SelectAll, vm.SelectAllCommand, 26));
        actions.Children.Add(EditorChrome.CreateToolButton("fa-solid fa-right-left", Se.Language.General.InvertSelection, vm.InvertSelectionCommand, 26));
        var layersView = EditorChrome.MakePickerListSection(Se.Language.General.Layer, MakeLayersView(vm), actions, lineOnTop: true);

        var sections = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
            },
        };
        sections.Add(settingsView, 0);
        sections.Add(layersView, 1);

        var buttonRemoveFilter = EditorChrome.MakeTextToolButton("fa-solid fa-filter-circle-xmark", Se.Language.General.RemoveFilter, vm.RemoveFilterCommand);
        var buttonOk = EditorChrome.AsPrimary(UiUtil.MakeButtonOk(vm.OkCommand));
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);

        Content = EditorChrome.MakeRoot(sections, EditorChrome.MakeButtonBar(buttonRemoveFilter, buttonCancel, buttonOk));

        UiUtil.FocusOnFirstActivation(this, () => TableViewExtras.FocusRow(vm.LayerGrid));
        Loaded += delegate { vm.Loaded(); };
        KeyDown += vm.KeyDown;
    }

    private static Border MakeSettings(PickLayerFilterViewModel vm)
    {
        var panel = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 8,
            Children =
            {
                EditorChrome.MakeSwitchRow(nameof(vm.HideFromWaveform), Se.Language.Tools.FilterLayersHideFromWaveform),
                EditorChrome.MakeSwitchRow(nameof(vm.HideFromGridView), Se.Language.Tools.FilterLayersHideFromSubtitleGrid),
                EditorChrome.MakeSwitchRow(nameof(vm.HideFromVideoPreview), Se.Language.Tools.FilterLayersHideFromVideoPreview),
            }
        };

        return EditorChrome.MakeEditorSection(Se.Language.General.Options, panel, first: true);
    }

    private static TableView MakeLayersView(PickLayerFilterViewModel vm)
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
            CellTemplate = TableViewExtras.MakeCheckBoxCellTemplate<LayerItem>(nameof(LayerItem.IsSelected)),
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

        AutomationProperties.SetName(dataGrid, Se.Language.General.Layer);
        return dataGrid;
    }
}
