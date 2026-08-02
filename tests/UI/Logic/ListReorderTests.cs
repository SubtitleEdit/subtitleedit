using Nikse.SubtitleEdit.Logic;
using System.Collections.ObjectModel;
using System.Linq;

namespace UITests.Logic;

public class ListReorderTests
{
    private static ObservableCollection<string> MakeList() =>
        new(new[] { "a", "b", "c", "d", "e" });

    private static int[] IndicesOf(ObservableCollection<string> list, params string[] items) =>
        items.Select(list.IndexOf).ToArray();

    [Fact]
    public void Up_MovesSingleRowOneStep()
    {
        var list = MakeList();
        ListReorder.Move(list, IndicesOf(list, "c"), ListMoveDirection.Up);
        Assert.Equal(new[] { "a", "c", "b", "d", "e" }, list);
    }

    [Fact]
    public void Up_KeepsTopRowInPlace()
    {
        var list = MakeList();
        ListReorder.Move(list, IndicesOf(list, "a"), ListMoveDirection.Up);
        Assert.Equal(new[] { "a", "b", "c", "d", "e" }, list);
    }

    [Fact]
    public void Up_MovesContiguousBlockAsOne()
    {
        var list = MakeList();
        ListReorder.Move(list, IndicesOf(list, "c", "d"), ListMoveDirection.Up);
        Assert.Equal(new[] { "a", "c", "d", "b", "e" }, list);
    }

    [Fact]
    public void Up_PinnedTopRowsHoldTheRestBack()
    {
        // {a, c, d} selected: 'a' is already at the top, so 'c' and 'd' collapse against it.
        var list = MakeList();
        ListReorder.Move(list, IndicesOf(list, "a", "c", "d"), ListMoveDirection.Up);
        Assert.Equal(new[] { "a", "c", "d", "b", "e" }, list);
    }

    [Fact]
    public void Up_IsIdempotentOnceTheSelectionIsStackedAtTheTop()
    {
        var list = MakeList();
        ListReorder.Move(list, IndicesOf(list, "a", "b"), ListMoveDirection.Up);
        Assert.Equal(new[] { "a", "b", "c", "d", "e" }, list);
    }

    [Fact]
    public void Down_MovesSingleRowOneStep()
    {
        var list = MakeList();
        ListReorder.Move(list, IndicesOf(list, "c"), ListMoveDirection.Down);
        Assert.Equal(new[] { "a", "b", "d", "c", "e" }, list);
    }

    [Fact]
    public void Down_KeepsBottomRowInPlace()
    {
        var list = MakeList();
        ListReorder.Move(list, IndicesOf(list, "e"), ListMoveDirection.Down);
        Assert.Equal(new[] { "a", "b", "c", "d", "e" }, list);
    }

    [Fact]
    public void Down_PinnedBottomRowsHoldTheRestBack()
    {
        var list = MakeList();
        ListReorder.Move(list, IndicesOf(list, "b", "c", "e"), ListMoveDirection.Down);
        Assert.Equal(new[] { "a", "d", "b", "c", "e" }, list);
    }

    [Fact]
    public void Top_MovesScatteredSelectionKeepingItsOrder()
    {
        var list = MakeList();
        ListReorder.Move(list, IndicesOf(list, "b", "d"), ListMoveDirection.Top);
        Assert.Equal(new[] { "b", "d", "a", "c", "e" }, list);
    }

    [Fact]
    public void Bottom_MovesScatteredSelectionKeepingItsOrder()
    {
        var list = MakeList();
        ListReorder.Move(list, IndicesOf(list, "b", "d"), ListMoveDirection.Bottom);
        Assert.Equal(new[] { "a", "c", "e", "b", "d" }, list);
    }

    [Fact]
    public void Top_SelectionOrderFollowsListOrderNotSelectionOrder()
    {
        var list = MakeList();
        ListReorder.Move(list, new[] { 3, 1 }, ListMoveDirection.Top);
        Assert.Equal(new[] { "b", "d", "a", "c", "e" }, list);
    }

    [Fact]
    public void Move_IgnoresOutOfRangeAndDuplicateIndices()
    {
        var list = MakeList();
        ListReorder.Move(list, new[] { 2, 2, -1, 99 }, ListMoveDirection.Up);
        Assert.Equal(new[] { "a", "c", "b", "d", "e" }, list);
    }

    [Fact]
    public void Move_NoOpOnEmptySelection()
    {
        var list = MakeList();
        ListReorder.Move(list, new int[0], ListMoveDirection.Down);
        Assert.Equal(new[] { "a", "b", "c", "d", "e" }, list);
    }

    [Fact]
    public void Move_NoOpOnSingleItemList()
    {
        var list = new ObservableCollection<string> { "a" };
        ListReorder.Move(list, new[] { 0 }, ListMoveDirection.Bottom);
        Assert.Equal(new[] { "a" }, list);
    }

    [Fact]
    public void Move_WholeSelectionIsAlwaysANoOp()
    {
        var list = MakeList();
        var all = Enumerable.Range(0, list.Count).ToArray();
        foreach (var direction in new[]
                 {
                     ListMoveDirection.Up, ListMoveDirection.Down,
                     ListMoveDirection.Top, ListMoveDirection.Bottom,
                 })
        {
            ListReorder.Move(list, all, direction);
            Assert.Equal(new[] { "a", "b", "c", "d", "e" }, list);
        }
    }
}
