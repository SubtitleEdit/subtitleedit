using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Nikse.SubtitleEdit.Logic;
using System.Linq;

namespace UITests.Logic;

/// <summary>
/// The "Columns..." dialog (#14369) reorders the subtitle grid through
/// <see cref="TableViewColumnManager.ApplyOrder"/>. The manager's Sync() can only insert
/// missing columns - a naive permutation through it duplicated columns - and a saved order
/// from an older version must neither hide nor displace columns added since, so both the
/// live-list rebuild and the unknown-key merge are pinned down here.
/// </summary>
public class TableViewColumnManagerApplyOrderTests
{
    private static (TableView Grid, TableViewColumnManager Manager) MakeGrid(params SeTableViewColumn[] columns)
    {
        var grid = TableViewExtras.MakeTableView();
        var manager = new TableViewColumnManager(grid);
        foreach (var column in columns)
        {
            manager.Add(column);
        }

        return (grid, manager);
    }

    private static string[] Keys(System.Collections.Generic.IEnumerable<TableViewColumn> columns)
        => columns.OfType<SeTableViewColumn>().Select(c => (string)c.Tag!).ToArray();

    [AvaloniaFact]
    public void ApplyOrder_PermutesMasterAndLiveList_WithoutDuplicates()
    {
        var (grid, manager) = MakeGrid(
            new SeTableViewColumn { Tag = "A" },
            new SeTableViewColumn { Tag = "B" },
            new SeTableViewColumn { Tag = "C" });

        manager.ApplyOrder(new[] { "C", "A", "B" });

        Assert.Equal(new[] { "C", "A", "B" }, Keys(manager.Columns));
        Assert.Equal(new[] { "C", "A", "B" }, Keys(grid.Columns));
        Assert.Equal(3, grid.Columns.Count);
    }

    [AvaloniaFact]
    public void ApplyOrder_KeepsHiddenColumnsHiddenButOrdered()
    {
        var hidden = new SeTableViewColumn { Tag = "B", IsVisible = false };
        var (grid, manager) = MakeGrid(
            new SeTableViewColumn { Tag = "A" },
            hidden,
            new SeTableViewColumn { Tag = "C" });

        manager.ApplyOrder(new[] { "B", "C", "A" });

        Assert.Equal(new[] { "B", "C", "A" }, Keys(manager.Columns));
        Assert.Equal(new[] { "C", "A" }, Keys(grid.Columns));

        // Showing the hidden column later must surface it at its reordered position.
        hidden.IsVisible = true;
        Assert.Equal(new[] { "B", "C", "A" }, Keys(grid.Columns));
    }

    [AvaloniaFact]
    public void ApplyOrder_UnknownAndMissingKeys_KeepDefaultPositions()
    {
        var (grid, manager) = MakeGrid(
            new SeTableViewColumn { Tag = "A" },
            new SeTableViewColumn { Tag = "New" }, // not in the saved order (added after it was saved)
            new SeTableViewColumn { Tag = "B" });

        manager.ApplyOrder(new[] { "B", "Gone", "A" }); // "Gone" no longer exists

        Assert.Equal(new[] { "B", "New", "A" }, Keys(manager.Columns));
        Assert.Equal(new[] { "B", "New", "A" }, Keys(grid.Columns));
    }

    [AvaloniaFact]
    public void ApplyOrder_EmptyOrder_LeavesOrderUntouched()
    {
        var (grid, manager) = MakeGrid(
            new SeTableViewColumn { Tag = "A" },
            new SeTableViewColumn { Tag = "B" });

        manager.ApplyOrder(System.Array.Empty<string>());
        manager.ApplyOrder(null);

        Assert.Equal(new[] { "A", "B" }, Keys(manager.Columns));
        Assert.Equal(new[] { "A", "B" }, Keys(grid.Columns));
    }
}
