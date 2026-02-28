using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Layout;
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

        var buttonGoTo = UiUtil.MakeButton(Se.Language.General.GoTo, vm.GoToCommand).WithBindIsEnabled(nameof(vm.HasShotChanges));
        var buttonClear = UiUtil.MakeButton(Se.Language.General.Clear, vm.ClearCommand).WithBindIsEnabled(nameof(vm.HasShotChanges));
        var buttonCancel = UiUtil.MakeButtonDone(vm.CancelCommand);
        var panelButtons = UiUtil.MakeButtonBar(buttonGoTo, buttonClear, buttonCancel);

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
        grid.Add(panelButtons, 1);

        Content = grid;

        Activated += delegate { buttonCancel.Focus(); }; // hack to make OnKeyDown work
    }

    private static Border MakeBookmarkGridView(ShotChangeListViewModel vm)
    {
        var dataGrid = new DataGrid
        {
            Height = double.NaN, // auto size inside scroll viewer
            Margin = new Thickness(2),
            ItemsSource = vm.ShotChanges, // Use ItemsSource instead of Items
            CanUserSortColumns = false,
            IsReadOnly = true,
            SelectionMode = DataGridSelectionMode.Extended,
            DataContext = vm,
        };

        dataGrid.DoubleTapped += vm.OnShotChangeGridDoubleTapped;

        var fullTimeConverter = new TimeSpanToDisplayFullConverter();
        var shortTimeConverter = new TimeSpanToDisplayShortConverter();

        // Columns
        dataGrid.Columns.Add(new DataGridTextColumn
        {
            Header = Se.Language.General.NumberSymbol,
            Binding = new Binding(nameof(ShotChangeItem.Index)),
            CellTheme = UiUtil.DataGridNoBorderCellTheme,
        });
        dataGrid.Columns.Add(new DataGridTextColumn
        {
            Header = Se.Language.General.Text,
            Binding = new Binding(nameof(ShotChangeItem.TimeText)),
            CellTheme = UiUtil.DataGridNoBorderCellTheme,
            Width = new DataGridLength(1, DataGridLengthUnitType.Star) // star sizing to take all available space
        });
        dataGrid.Bind(DataGrid.SelectedItemProperty, new Binding(nameof(vm.SelectedShotChange))
        {
            Source = vm,
            Mode = BindingMode.TwoWay
        });
        dataGrid.SelectionChanged += vm.GridSelectionChanged;
        dataGrid.DoubleTapped += (s, e) => vm.GoToCommand.Execute(null);    
        dataGrid.KeyDown += (s, e) => vm.GridKeyDown(e);  

        var flyout = new MenuFlyout();
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
        dataGrid.ContextFlyout = flyout;

        return UiUtil.MakeBorderForControl(dataGrid);
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        _vm.OnKeyDown(e);
    }
}
