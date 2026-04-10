using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.UndoRedo;

namespace UITests.Logic.UndoRedo;

public class UndoRedoManagerTests
{
    // -----------------------------------------------------------------------
    // Test doubles
    // -----------------------------------------------------------------------

    private sealed class FakeClient : IUndoRedoClient
    {
        public int Hash { get; set; }
        public bool Typing { get; set; }
        public SubtitleLineViewModel[] Subtitles { get; set; } = [];

        public int GetFastHash() => Hash;
        public bool IsTyping() => Typing;
        public UndoRedoItem MakeUndoRedoObject(string description) =>
            MakeItem(description, Hash, Subtitles);
    }

    // -----------------------------------------------------------------------
    // Helpers
    // -----------------------------------------------------------------------

    private static SubtitleLineViewModel MakeLine(string text, int startMs = 1000) =>
        new() { Number = 1, Text = text, StartTime = TimeSpan.FromMilliseconds(startMs), EndTime = TimeSpan.FromMilliseconds(startMs + 1000) };

    private static UndoRedoItem MakeItem(string description, int hash, SubtitleLineViewModel[]? subtitles = null) =>
        new(description, subtitles ?? [], hash, null, [], 0, 0);

    // -----------------------------------------------------------------------
    // Do()
    // -----------------------------------------------------------------------

    [Fact]
    public void Do_AddsItemToUndoStack()
    {
        var manager = new UndoRedoManager();

        manager.Do(MakeItem("action", 1));

        Assert.Equal(1, manager.UndoCount);
    }

    [Fact]
    public void Do_ClearsRedoStack()
    {
        var client = new FakeClient { Hash = 2 };
        var manager = new UndoRedoManager();
        manager.SetupChangeDetection(client);
        manager.Do(MakeItem("a", 1));
        manager.Do(MakeItem("b", 2));
        manager.Undo();
        Assert.True(manager.CanRedo);

        manager.Do(MakeItem("c", 3));

        Assert.False(manager.CanRedo);
    }

    [Fact]
    public void Do_DoesNotExceedMaxUndoItems()
    {
        var manager = new UndoRedoManager();

        for (var i = 0; i < 110; i++)
            manager.Do(MakeItem($"action-{i}", i));

        Assert.True(manager.UndoCount <= 100);
    }

    // -----------------------------------------------------------------------
    // CanUndo / CanRedo
    // -----------------------------------------------------------------------

    [Fact]
    public void CanUndo_IsFalse_WhenStackIsEmpty()
    {
        var manager = new UndoRedoManager();

        Assert.False(manager.CanUndo);
    }

    [Fact]
    public void CanUndo_IsFalse_WhenSingleBaselineItemMatchesLiveHash()
    {
        var client = new FakeClient { Hash = 1 };
        var manager = new UndoRedoManager();
        manager.SetupChangeDetection(client);
        manager.Do(MakeItem("baseline", 1));

        Assert.False(manager.CanUndo);
    }

    [Fact]
    public void CanUndo_IsTrue_WhenMultipleItemsExist()
    {
        var client = new FakeClient { Hash = 2 };
        var manager = new UndoRedoManager();
        manager.SetupChangeDetection(client);
        manager.Do(MakeItem("a", 1));
        manager.Do(MakeItem("b", 2));

        Assert.True(manager.CanUndo);
    }

    [Fact]
    public void CanRedo_IsFalse_Initially()
    {
        var manager = new UndoRedoManager();

        Assert.False(manager.CanRedo);
    }

    [Fact]
    public void CanRedo_IsTrue_AfterUndo()
    {
        var client = new FakeClient { Hash = 2 };
        var manager = new UndoRedoManager();
        manager.SetupChangeDetection(client);
        manager.Do(MakeItem("a", 1));
        manager.Do(MakeItem("b", 2));

        manager.Undo();

        Assert.True(manager.CanRedo);
    }

    // -----------------------------------------------------------------------
    // Undo()
    // -----------------------------------------------------------------------

    [Fact]
    public void Undo_ReturnsNull_WhenStackIsEmpty()
    {
        var manager = new UndoRedoManager();

        Assert.Null(manager.Undo());
    }

    [Fact]
    public void Undo_ReturnsPreviousState()
    {
        var client = new FakeClient { Hash = 2 };
        var manager = new UndoRedoManager();
        manager.SetupChangeDetection(client);
        manager.Do(MakeItem("state-a", 1));
        manager.Do(MakeItem("state-b", 2));

        var result = manager.Undo();

        Assert.NotNull(result);
        Assert.Equal("state-a", result.Description);
        Assert.Equal(1, result.Hash);
    }

    [Fact]
    public void Undo_MovesTopToRedoStack()
    {
        var client = new FakeClient { Hash = 2 };
        var manager = new UndoRedoManager();
        manager.SetupChangeDetection(client);
        manager.Do(MakeItem("a", 1));
        manager.Do(MakeItem("b", 2));

        manager.Undo();

        Assert.True(manager.CanRedo);
    }

    [Fact]
    public void Undo_WhenLiveHashMatchesTop_SkipsTopAndReturnsDeeperItem()
    {
        // Regression: the "press undo twice" bug.
        // When the live hash matches the top item, a single Undo() call must
        // push that top entry to redo and return the item beneath it.
        var client = new FakeClient { Hash = 2 };
        var manager = new UndoRedoManager();
        manager.SetupChangeDetection(client);
        manager.Do(MakeItem("state-a", 1));
        manager.Do(MakeItem("state-b", 2));

        var result = manager.Undo();

        Assert.NotNull(result);
        Assert.Equal(1, result.Hash);
    }

    [Fact]
    public void Undo_ReturnsNull_WhenOnlyBaselineExistsWithMatchingHash()
    {
        var client = new FakeClient { Hash = 1 };
        var manager = new UndoRedoManager();
        manager.SetupChangeDetection(client);
        manager.Do(MakeItem("baseline", 1));

        var result = manager.Undo();

        Assert.Null(result);
    }

    [Fact]
    public void Undo_KeepsBaselineItemOnStack()
    {
        var client = new FakeClient { Hash = 2 };
        var manager = new UndoRedoManager();
        manager.SetupChangeDetection(client);
        manager.Do(MakeItem("a", 1));
        manager.Do(MakeItem("b", 2));

        manager.Undo();

        Assert.Equal(1, manager.UndoCount);
    }

    // -----------------------------------------------------------------------
    // Redo()
    // -----------------------------------------------------------------------

    [Fact]
    public void Redo_ReturnsNull_WhenRedoStackIsEmpty()
    {
        var manager = new UndoRedoManager();

        Assert.Null(manager.Redo());
    }

    [Fact]
    public void Redo_ReappliesUndoneState()
    {
        var client = new FakeClient { Hash = 2 };
        var manager = new UndoRedoManager();
        manager.SetupChangeDetection(client);
        manager.Do(MakeItem("state-a", 1));
        manager.Do(MakeItem("state-b", 2));
        manager.Undo();

        client.Hash = 1; // simulates state restored by caller after Undo
        var result = manager.Redo();

        Assert.NotNull(result);
        Assert.Equal("state-b", result.Description);
    }

    [Fact]
    public void Redo_DoesNotClearRemainingRedoEntries()
    {
        // Regression: Redo previously called DoCore() which cleared the redo
        // stack, making only the first Redo in a multi-step sequence work.
        var client = new FakeClient { Hash = 3 };
        var manager = new UndoRedoManager();
        manager.SetupChangeDetection(client);
        manager.Do(MakeItem("state-a", 1));
        manager.Do(MakeItem("state-b", 2));
        manager.Do(MakeItem("state-c", 3));

        manager.Undo();        // redo: [c, b]
        client.Hash = 2;
        manager.Undo();        // redo: [c, b, a]
        client.Hash = 1;
        manager.Undo();        // already at baseline — no change

        client.Hash = 1;
        manager.Redo();        // restore b; redo should still contain c

        Assert.True(manager.CanRedo, "Remaining redo entries must survive the first Redo call.");
    }

    [Fact]
    public void Redo_AddsRestoredItemBackToUndoStack()
    {
        var client = new FakeClient { Hash = 2 };
        var manager = new UndoRedoManager();
        manager.SetupChangeDetection(client);
        manager.Do(MakeItem("a", 1));
        manager.Do(MakeItem("b", 2));
        var undoCountBeforeUndo = manager.UndoCount;

        manager.Undo();
        Assert.Equal(undoCountBeforeUndo - 1, manager.UndoCount);

        client.Hash = 1;
        manager.Redo();

        Assert.Equal(undoCountBeforeUndo, manager.UndoCount);
    }

    [Fact]
    public void UndoRedoItemClone_PreservesExactTimeSpanTicks()
    {
        var line = new SubtitleLineViewModel
        {
            Number = 1,
            Text = "tick-precision",
            StartTime = TimeSpan.FromTicks(TimeSpan.TicksPerMillisecond * 1000 + 1234),
            EndTime = TimeSpan.FromTicks(TimeSpan.TicksPerMillisecond * 2000 + 5678)
        };

        var item = MakeItem("state", 1, [line]);
        var cloned = UndoRedoItem.Clone(item);

        Assert.NotNull(cloned);
        Assert.Single(cloned.Subtitles);
        Assert.Equal(line.StartTime.Ticks, cloned.Subtitles[0].StartTime.Ticks);
        Assert.Equal(line.EndTime.Ticks, cloned.Subtitles[0].EndTime.Ticks);
    }

    [Fact]
    public void UndoRedoUndo_KeepsExpectedStateBetweenActions()
    {
        var client = new FakeClient { Hash = 2 };
        var manager = new UndoRedoManager();
        manager.SetupChangeDetection(client);
        manager.Do(MakeItem("state-a", 1));
        manager.Do(MakeItem("state-b", 2));

        Assert.Equal(2, manager.UndoCount);
        Assert.Equal(0, manager.RedoCount);

        var undo1 = manager.Undo();

        Assert.NotNull(undo1);
        Assert.Equal(1, undo1.Hash);
        Assert.Equal(1, manager.UndoCount);
        Assert.Equal(2, manager.RedoCount);
        Assert.True(manager.CanRedo);

        client.Hash = undo1.Hash; // simulate caller restoring state-a
        var redo = manager.Redo();

        Assert.NotNull(redo);
        Assert.Equal(2, redo.Hash);
        Assert.Equal(2, manager.UndoCount);
        Assert.Equal(0, manager.RedoCount);
        Assert.False(manager.CanRedo);

        client.Hash = redo.Hash; // simulate caller restoring state-b
        var undo2 = manager.Undo();

        Assert.NotNull(undo2);
        Assert.Equal(1, undo2.Hash);
        Assert.Equal(1, manager.UndoCount);
        Assert.Equal(2, manager.RedoCount);
        Assert.True(manager.CanRedo);
    }

    // -----------------------------------------------------------------------
    // CheckForChanges()
    // -----------------------------------------------------------------------

    [Fact]
    public void CheckForChanges_AddsEntry_WhenLineCountIncreases()
    {
        Se.Language.General.LinesAddedX = "{0} lines added";
        var lines1 = new[] { MakeLine("hello") };
        var client = new FakeClient { Hash = 1, Subtitles = lines1 };
        var manager = new UndoRedoManager();
        manager.SetupChangeDetection(client, TimeSpan.FromHours(1));
        manager.StartChangeDetection();
        manager.Do(MakeItem("initial", 1, lines1));

        // Simulate adding a line
        client.Hash = 2;
        client.Subtitles = [MakeLine("hello"), MakeLine("world")];
        manager.CheckForChanges(null);

        Assert.Equal(2, manager.UndoCount);
    }

    [Fact]
    public void CheckForChanges_DoesNotAddEntry_WhenHashAlreadyTracked()
    {
        var lines = new[] { MakeLine("hello") };
        var client = new FakeClient { Hash = 1, Subtitles = lines };
        var manager = new UndoRedoManager();
        manager.SetupChangeDetection(client, TimeSpan.FromHours(1));
        manager.StartChangeDetection();
        manager.Do(MakeItem("initial", 1, lines));

        manager.CheckForChanges(null); // same hash — nothing changed

        Assert.Equal(1, manager.UndoCount);
    }

    [Fact]
    public void CheckForChanges_DoesNotAddEntry_WhenIsTyping()
    {
        var client = new FakeClient { Hash = 42, Typing = true };
        var manager = new UndoRedoManager();
        manager.SetupChangeDetection(client, TimeSpan.FromHours(1));
        manager.StartChangeDetection();

        manager.CheckForChanges(null);

        Assert.Equal(0, manager.UndoCount);
    }

    // -----------------------------------------------------------------------
    // Reset()
    // -----------------------------------------------------------------------

    [Fact]
    public void Reset_ClearsBothStacks()
    {
        var client = new FakeClient { Hash = 2 };
        var manager = new UndoRedoManager();
        manager.SetupChangeDetection(client, TimeSpan.FromHours(1));
        manager.Do(MakeItem("a", 1));
        manager.Do(MakeItem("b", 2));
        manager.Undo();

        manager.Reset();
        manager.StopChangeDetection();

        Assert.Equal(0, manager.UndoCount);
        Assert.Equal(0, manager.RedoCount);
    }
}
