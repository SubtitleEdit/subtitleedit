using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Threading;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.ValueConverters;

namespace Nikse.SubtitleEdit.Features.Shared.PickDvdTitle;

public class PickDvdTitleWindow : Window
{
    public PickDvdTitleWindow(PickDvdTitleViewModel vm)
    {
        vm.Window = this;
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = vm.WindowTitle;
        Width = 900;
        Height = 450;
        MinWidth = 600;
        MinHeight = 300;
        CanResize = true;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        Closing += delegate { UiUtil.SaveWindowPosition(this); };
        Loaded += delegate { UiUtil.RestoreWindowPosition(this); };
        DataContext = vm;

        var titlesView = MakeTitlesView(vm);
        var labelVideoInfo = UiUtil.MakeLabel().WithBindText(vm, nameof(vm.VideoInfo));

        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);
        var panelButtons = UiUtil.MakeButtonBar(buttonOk, buttonCancel);

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
            },
            Margin = UiUtil.MakeWindowMargin(),
            RowSpacing = 5,
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        grid.Add(titlesView, 0);
        grid.Add(labelVideoInfo, 1);
        grid.Add(panelButtons, 2);

        Content = grid;

        AddHandler(KeyDownEvent, vm.OnKeyDownHandler, RoutingStrategies.Tunnel | RoutingStrategies.Bubble, handledEventsToo: false);

        Loaded += (_, _) =>
        {
            Dispatcher.UIThread.InvokeAsync(() =>
            {
                vm.SelectAndScrollToSelected();
                TableViewExtras.FocusRow(vm.TitlesGrid);
            }, DispatcherPriority.Input);
        };
    }

    private static Border MakeTitlesView(PickDvdTitleViewModel vm)
    {
        var dataGrid = TableViewExtras.MakeTableView(multiSelect: false);
        dataGrid.WithAccessibleName(Se.Language.General.Title);
        dataGrid.Width = double.NaN;
        dataGrid.Height = double.NaN;
        dataGrid.DataContext = vm;
        dataGrid.ItemsSource = vm.Titles;

        var nameColumn = new SeTableViewColumn
        {
            Header = Se.Language.General.Title,
            CellTheme = UiUtil.TableViewCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
            Binding = new Binding(nameof(DvdTitleDisplay.Name)),
            Width = new GridLength(70),
        };
        var durationColumn = new SeTableViewColumn
        {
            Header = Se.Language.General.Duration,
            CellTheme = UiUtil.TableViewCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
            Binding = new Binding(nameof(DvdTitleDisplay.Duration)) { Converter = new TimeSpanToDisplayFullConverter() },
            Width = new GridLength(120),
        };
        var chaptersColumn = new SeTableViewColumn
        {
            Header = Se.Language.File.Chapters,
            CellTheme = UiUtil.TableViewCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
            Binding = new Binding(nameof(DvdTitleDisplay.Chapters)),
            Width = new GridLength(80),
        };
        var languagesColumn = new SeTableViewColumn
        {
            Header = Se.Language.General.Language,
            CellTheme = UiUtil.TableViewCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
            Binding = new Binding(nameof(DvdTitleDisplay.Languages)),
            Width = new GridLength(1, GridUnitType.Star),
        };
        var statusColumn = new SeTableViewColumn
        {
            Header = Se.Language.General.Status,
            CellTheme = UiUtil.TableViewCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
            Binding = new Binding(nameof(DvdTitleDisplay.Status)),
            Width = new GridLength(160),
        };

        dataGrid.Columns.Add(nameColumn);
        dataGrid.Columns.Add(durationColumn);
        dataGrid.Columns.Add(chaptersColumn);
        dataGrid.Columns.Add(languagesColumn);
        dataGrid.Columns.Add(statusColumn);

        dataGrid.Bind(TableView.SelectedItemProperty, new Binding(nameof(vm.SelectedTitle)));
        dataGrid.DoubleTapped += (_, _) => vm.OkCommand.Execute(null);
        vm.TitlesGrid = dataGrid;

        return UiUtil.MakeBorderForControlNoPadding(dataGrid);
    }
}