using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using System.Collections;
using Avalonia.Media;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.ValueConverters;

namespace Nikse.SubtitleEdit.Features.Files.ImportImages;

public class ImportImagesWindow : Window
{
    public ImportImagesWindow(ImportImagesViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.File.Import.TitleImportImages;
        CanResize = true;
        Width = 1200;
        Height = 850;
        MinWidth = 1100;
        MinHeight = 600;

        vm.Window = this;
        DataContext = vm;

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
            ColumnSpacing = 5,
            RowSpacing = 5,
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        var labelImportInfo = new TextBlock
        {
            Text = Se.Language.File.Import.ImportFileLabel,
        };

        if (Se.Settings.Appearance.ShowHints)
        {
            ToolTip.SetTip(labelImportInfo, Se.Language.File.Import.ImportFilesInfo);
        }

        var buttonImport = UiUtil.MakeButton(Se.Language.General.ImportDotDotDot, vm.FileImportCommand).WithLeftAlignment();

        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);
        var panelButtons = UiUtil.MakeButtonBar(buttonOk, buttonCancel);

        grid.Add(labelImportInfo, 0);
        grid.Add(MakeImagesView(vm, out var imagesGrid), 1);
        grid.Add(panelButtons, 3, 0);
        grid.Add(buttonImport, 3, 0);

        Content = grid;

        Activated += delegate { TableViewExtras.FocusRow(imagesGrid); }; // initial focus on an input, not an action button - a focused button clicks on bare Space
        KeyDown += vm.KeyDown;
    }

    private static Border MakeImagesView(ImportImagesViewModel vm, out TableView imagesGrid)
    {
        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        var fullTimeConverter = new TimeSpanToDisplayFullConverter();
        var shortTimeConverter = new TimeSpanToDisplayShortConverter();
        var fileSizeConverter = new FileSizeConverter();
        // No header sorting (the DataGrid's CanUserSortColumns is not carried over):
        // the caller feeds result.Images to OCR in collection order, so reordering the
        // backing collection would reorder the resulting subtitle.
        var dataGrid = TableViewExtras.MakeTableView(multiSelect: false);
        imagesGrid = dataGrid;
        dataGrid.DataContext = vm;
        dataGrid.ItemsSource = vm.Images;
        dataGrid.Columns.AddRange(new TableViewColumn[]
        {
            new SeTableViewColumn
            {
                Header = Se.Language.General.FileName,
                CellTheme = UiUtil.TableViewCellTheme,
                HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
                Binding = new Binding(nameof(ImportImageItem.FileName)),
                Width = new GridLength(1, GridUnitType.Star),
            },
            new SeTableViewColumn
            {
                Header = Se.Language.General.Size,
                CellTheme = UiUtil.TableViewCellTheme,
                HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
                Binding = new Binding(nameof(ImportImageItem.Size)) { Converter = fileSizeConverter, Mode = BindingMode.OneWay },
                Width = new GridLength(100),
            },
            new SeTableViewColumn
            {
                Header = Se.Language.General.Show,
                CellTheme = UiUtil.TableViewCellTheme,
                HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
                Binding = new Binding(nameof(ImportImageItem.Start)) { Converter = fullTimeConverter, Mode = BindingMode.OneWay },
                Width = new GridLength(130),
            },
            new SeTableViewColumn
            {
                Header = Se.Language.General.Hide,
                CellTheme = UiUtil.TableViewCellTheme,
                HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
                Binding = new Binding(nameof(ImportImageItem.End)){ Converter = fullTimeConverter, Mode = BindingMode.OneWay },
                Width = new GridLength(130),
            },
            new SeTableViewColumn
            {
                Header = Se.Language.General.Duration,
                CellTheme = UiUtil.TableViewCellTheme,
                HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
                Binding = new Binding(nameof(ImportImageItem.Duration)){ Converter = shortTimeConverter, Mode = BindingMode.OneWay },
                Width = new GridLength(110),
            },
        });
        dataGrid.Bind(TableView.SelectedItemProperty, new Binding(nameof(vm.SelectedImage)) { Source = vm });
        dataGrid.SelectionChanged += vm.DataGridSelectionChanged;
        dataGrid.KeyDown += vm.AttachmentsDataGridKeyDown;
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

        // hack to make drag and drop work on the grid - also on empty rows
        var dropHost = new Border
        {
            Background = Brushes.Transparent,
            Child = dataGrid,
        };
        DragDrop.SetAllowDrop(dropHost, true);
        dropHost.AddHandler(DragDrop.DragOverEvent, vm.FileGridOnDragOver, RoutingStrategies.Bubble);
        dropHost.AddHandler(DragDrop.DropEvent, vm.FileGridOnDrop, RoutingStrategies.Bubble);

        dropHost.Tapped += (s, e) =>
        {
            vm.FileGridTapped();
        };

        var flyout = new MenuFlyout();
        flyout.Opening += vm.ImagesContextMenuOpening;
        dropHost.ContextFlyout = flyout;
        UiUtil.AttachMacContextFlyoutHandler(dropHost);

        var menuItemDelete = new MenuItem
        {
            Header = Se.Language.General.Remove,
            DataContext = vm,
            Command = vm.ImageRemoveCommand,
        };
        menuItemDelete.Bind(MenuItem.IsVisibleProperty, new Binding(nameof(vm.IsDeleteVisible)) { Source = vm });
        flyout.Items.Add(menuItemDelete);

        var menuItemClear = new MenuItem
        {
            Header = Se.Language.General.Clear,
            DataContext = vm,
            Command = vm.AttachemntsRemoveAllCommand,
        };
        menuItemClear.Bind(MenuItem.IsVisibleProperty, new Binding(nameof(vm.IsDeleteAllVisible)) { Source = vm });
        flyout.Items.Add(menuItemClear);

        var menuItemImport = new MenuItem
        {
            Header = Se.Language.General.ImportDotDotDot,
            DataContext = vm,
            Command = vm.FileImportCommand,
        };
        flyout.Items.Add(menuItemImport);

        grid.Add(dropHost, 0);

        return UiUtil.MakeBorderForControlNoPadding(grid);
    }
}
