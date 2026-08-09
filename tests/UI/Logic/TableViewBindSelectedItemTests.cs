using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using Nikse.SubtitleEdit.Logic;

namespace UITests.Logic;

/// <summary>
/// TableViews from <see cref="TableViewExtras.MakeTableView"/> keep SelectionMode.AlwaysSelected,
/// so the control selects row 0 as soon as ItemsSource is assigned - before any SelectedItem
/// binding exists. A binding's first push goes source to target, so for a list that was already
/// populated the grid ends up showing row 0 while the view model still sees null.
/// <see cref="TableViewExtras.BindSelectedItem"/> is the fix; these tests pin both halves.
/// </summary>
public partial class ProbeViewModel : ObservableObject
{
    [ObservableProperty] private string? _selected;
}

public class TableViewBindSelectedItemTests : IDisposable
{
    // Every window opened by a test is closed again in Dispose: if a test stops early, an
    // unclosed window would outlive the test and race with the headless session teardown.
    private readonly List<Window> _windows = new();

    public void Dispose()
    {
        foreach (var window in _windows)
        {
            window.Close();
        }

        _windows.Clear();
    }

    private static TableView MakeGrid(params string[] items)
    {
        var grid = TableViewExtras.MakeTableView(multiSelect: false);
        grid.Columns.Add(new SeTableViewColumn { Binding = new Binding(".") });
        grid.ItemsSource = items.ToList();
        return grid;
    }

    private void Show(TableView grid)
    {
        var window = new Window { Content = grid };
        _windows.Add(window);
        window.Show();
        Dispatcher.UIThread.RunJobs();
        window.UpdateLayout();
        Dispatcher.UIThread.RunJobs();
    }

    [AvaloniaFact]
    public void PlainBind_LeavesTheViewModelOutOfSync()
    {
        // Documents the trap the helper exists for: the grid is on row 0, the view model is not.
        var vm = new ProbeViewModel();
        var grid = MakeGrid("a", "b");

        grid.Bind(TableView.SelectedItemProperty, new Binding(nameof(vm.Selected)) { Source = vm });
        Show(grid);

        Assert.Equal(0, grid.SelectedIndex);
        Assert.Null(vm.Selected);
    }

    [AvaloniaFact]
    public void BindSelectedItem_PushesTheGridsInitialSelectionIntoTheViewModel()
    {
        var vm = new ProbeViewModel();
        var grid = MakeGrid("a", "b");

        TableViewExtras.BindSelectedItem(grid, vm, nameof(vm.Selected));
        Show(grid);

        Assert.Equal(0, grid.SelectedIndex);
        Assert.Equal("a", vm.Selected);
    }

    [AvaloniaFact]
    public void BindSelectedItem_KeepsAViewModelSelectionThatIsAlreadySet()
    {
        var vm = new ProbeViewModel { Selected = "b" };
        var grid = MakeGrid("a", "b");

        TableViewExtras.BindSelectedItem(grid, vm, nameof(vm.Selected));
        Show(grid);

        Assert.Equal("b", vm.Selected);
        Assert.Equal(1, grid.SelectedIndex);
    }

    [AvaloniaFact]
    public void BindSelectedItem_StillRoundTripsLaterSelectionChanges()
    {
        var vm = new ProbeViewModel();
        var grid = MakeGrid("a", "b");

        TableViewExtras.BindSelectedItem(grid, vm, nameof(vm.Selected));
        Show(grid);

        grid.SelectedIndex = 1;
        Dispatcher.UIThread.RunJobs();

        Assert.Equal("b", vm.Selected);
    }

    [AvaloniaFact]
    public void BindSelectedItem_OnAnEmptyGridSelectsNothing()
    {
        var vm = new ProbeViewModel();
        var grid = MakeGrid();

        TableViewExtras.BindSelectedItem(grid, vm, nameof(vm.Selected));
        Show(grid);

        Assert.Null(vm.Selected);
    }
}
