using Nikse.SubtitleEdit.Logic;

namespace UITests.Logic;

/// <summary>
/// "Delete all empty lines" threw the grid to the end of the list: rows were removed one by one
/// from an AlwaysSelected grid, which picks a replacement row itself and drags the view along
/// (issue #13822). The row to keep is decided before anything is removed.
/// </summary>
public class GridSelectionAnchorTests
{
    // false = the row stays, true = it is about to be removed.
    private static bool[] Rows(params bool[] removed) => removed;

    [Fact]
    public void SelectedRowSurvives_ItStaysSelected()
    {
        var index = GridSelectionAnchor.PickSurvivorIndex(Rows(false, true, false, false), 2);

        Assert.Equal(2, index);
    }

    [Fact]
    public void SelectedRowIsRemoved_TheNextSurvivorTakesOver()
    {
        var index = GridSelectionAnchor.PickSurvivorIndex(Rows(false, true, true, false), 1);

        Assert.Equal(3, index);
    }

    // Everything after the selection goes: the user stays as close as possible, just above.
    [Fact]
    public void NothingSurvivesAfterTheSelection_TheRowBeforeItTakesOver()
    {
        var index = GridSelectionAnchor.PickSurvivorIndex(Rows(false, false, true, true), 3);

        Assert.Equal(1, index);
    }

    [Fact]
    public void EverythingIsRemoved_NoSelection()
    {
        var index = GridSelectionAnchor.PickSurvivorIndex(Rows(true, true), 0);

        Assert.Equal(-1, index);
    }

    [Fact]
    public void EmptyGrid_NoSelection()
    {
        Assert.Equal(-1, GridSelectionAnchor.PickSurvivorIndex(Rows(), 0));
    }

    [Theory]
    [InlineData(-1)]  // nothing selected
    [InlineData(99)]  // selection past the end
    public void SelectionOutsideTheList_FallsBackToASurvivingRow(int selectedIndex)
    {
        var index = GridSelectionAnchor.PickSurvivorIndex(Rows(false, true, false), selectedIndex);

        Assert.True(index is 0 or 2);
    }
}
