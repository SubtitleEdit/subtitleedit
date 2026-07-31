using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Nikse.SubtitleEdit.Features.Options.Settings;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Shared.PickRuleProfile;

public class PickRuleProfileWindow : Window
{
    private static TableView? _profileGrid;

    public PickRuleProfileWindow(PickRuleProfileViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.General.Profiles;
        CanResize = true;
        Width = 1100;
        Height = 750;
        MinWidth = 800;
        MinHeight = 700;
        vm.Window = this;
        DataContext = vm;

        var label = new Label
        {
            Content = Se.Language.Tools.AdjustDurations.AdjustVia,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(10, 0, 0, 0),
        };

        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);
        var panelButtons = UiUtil.MakeButtonBar(buttonOk, buttonCancel);

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

        grid.Add(MakeProfilesView(vm), 0);
        grid.Add(panelButtons, 1);

        Content = grid;

        Activated += delegate
        {
            if (_profileGrid != null)
            {
                TableViewExtras.FocusRow(_profileGrid); // hack to make OnKeyDown work
            }
        };
        KeyDown += vm.KeyDown;
    }

    private static Border MakeProfilesView(PickRuleProfileViewModel vm)
    {
        var dataGrid = TableViewExtras.MakeTableView(multiSelect: false);
        dataGrid.Width = double.NaN;
        dataGrid.Height = double.NaN;
        dataGrid.DataContext = vm;

        // Content-sized (Auto) on the DataGrid; TableView treats Auto as star, so the
        // number columns get fixed widths and Name becomes the star column (the old
        // grid's star was on the trailing max-CPS column, which only fills space).
        var nameColumn = new SeTableViewColumn
        {
            Header = Se.Language.General.Name,
            CellTheme = UiUtil.TableViewCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
            Binding = new Binding(nameof(ProfileDisplay.Name)),
            Width = new GridLength(1, GridUnitType.Star),
        };
        var singleLineMaxLengthColumn = new SeTableViewColumn
        {
            Header = Se.Language.Options.Settings.SingleLineMaxLength,
            CellTheme = UiUtil.TableViewCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
            Binding = new Binding(nameof(ProfileDisplay.SingleLineMaxLength)),
            Width = new GridLength(180),
        };
        var maxCpsColumn = new SeTableViewColumn
        {
            Header = Se.Language.General.MaxCharactersPerSecond,
            CellTheme = UiUtil.TableViewCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
            Binding = new Binding(nameof(ProfileDisplay.MaxCharsPerSec)),
            Width = new GridLength(180),
        };

        dataGrid.Columns.Add(nameColumn);
        dataGrid.Columns.Add(singleLineMaxLengthColumn);
        dataGrid.Columns.Add(maxCpsColumn);

        dataGrid.Bind(TableView.ItemsSourceProperty, new Binding(nameof(vm.Profiles)) { Source = vm });
        dataGrid.Bind(TableView.SelectedItemProperty, new Binding(nameof(vm.SelectedProfile)) { Source = vm });
        dataGrid.AddHandler(KeyDownEvent, vm.ProfileGridKeyDown, RoutingStrategies.Tunnel);
        dataGrid.DoubleTapped += vm.ProfileGridDoubleTapped;
        _profileGrid = dataGrid;

        // Profile list order is presentation-only (OK uses the selected item), so the
        // in-place header sorter is safe.
        new TableViewHeaderSorter(dataGrid)
            .AddSortable<ProfileDisplay, string>(nameColumn, x => x.Name)
            .AddSortable<ProfileDisplay, int?>(singleLineMaxLengthColumn, x => x.SingleLineMaxLength)
            .AddSortable<ProfileDisplay, double?>(maxCpsColumn, x => x.MaxCharsPerSec);

        return UiUtil.MakeBorderForControlNoPadding(dataGrid);
    }
}
