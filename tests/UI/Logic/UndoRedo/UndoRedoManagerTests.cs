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

    private sealed class BlockingHashClient : IUndoRedoClient
    {
        public ManualResetEventSlim HashEntered { get; } = new();
        public ManualResetEventSlim ReleaseHash { get; } = new();
        public int HashCalls;

        public int GetFastHash()
        {
            Interlocked.Increment(ref HashCalls);
            HashEntered.Set();
            ReleaseHash.Wait(TimeSpan.FromSeconds(10));
            return 1;
        }

        public bool IsTyping() => false;
        public UndoRedoItem MakeUndoRedoObject(string description) => MakeItem(description, 1);
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
    public void Undo_WhenLiveHashUnrecorded_SnapshotsLiveOntoRedoStack()
    {
        // Regression for #11280 — when the user undoes between polling ticks,
        // the live state has changes that aren't yet in the undo stack. Those
        // changes used to be discarded silently (the comment said "we can't
        // snapshot it here"), which is why users couldn't redo a quick
        // text-edit + waveform-drag combo. Now the live state is captured onto
        // redo before stepping back.
        var client = new FakeClient { Hash = 99 };  // live hash != any in undo stack
        var manager = new UndoRedoManager();
        manager.SetupChangeDetection(client);
        manager.Do(MakeItem("state-a", 1));
        manager.Do(MakeItem("state-b", 2));

        var result = manager.Undo();

        Assert.NotNull(result);
        Assert.Equal(2, result.Hash);            // stepped back to state-b
        Assert.Equal(1, manager.RedoCount);      // live state was snapshotted onto redo
        Assert.Equal("Unrecorded changes", manager.RedoList[0].Description);
    }

    [Fact]
    public void Undo_WhenLiveHashUnrecorded_ClearsStaleRedoBeforeSnapshot()
    {
        // The unrecorded edit diverges from any prior redo timeline (same
        // semantic as a regular Do() clearing redo). If we left stale redo
        // entries in place, the user could redo into a future that's
        // incoherent with the live state they just diverged from.
        var client = new FakeClient { Hash = 2 };
        var manager = new UndoRedoManager();
        manager.SetupChangeDetection(client);
        manager.Do(MakeItem("state-a", 1));
        manager.Do(MakeItem("state-b", 2));
        manager.Undo(); // pops state-b onto redo. UndoList=[state-a], RedoList=[state-b]
        Assert.Equal(1, manager.RedoCount);

        // Now simulate an unrecorded edit — live hash diverges from top of undo.
        client.Hash = 99;

        manager.Undo();

        // Stale state-b entry on redo should be gone; replaced by the live snapshot.
        Assert.Equal(1, manager.RedoCount);
        Assert.Equal("Unrecorded changes", manager.RedoList[0].Description);
        Assert.Equal(99, manager.RedoList[0].Hash);
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
        Assert.Equal(1, manager.RedoCount);
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
        Assert.Equal(1, manager.RedoCount);
        Assert.True(manager.CanRedo);
    }

    [Fact]
    public void UndoRedoCycle_PreservesIntermediateStates_Issue10471()
    {
        // Regression for https://github.com/SubtitleEdit/subtitleedit/issues/10471
        // — undo→redo→undo cycles previously dropped intermediate states from
        // the undo stack, causing later undos to skip multiple steps.
        var client = new FakeClient { Hash = 4 };
        var manager = new UndoRedoManager();
        manager.SetupChangeDetection(client);
        manager.Do(MakeItem("s0", 1));
        manager.Do(MakeItem("s1", 2));
        manager.Do(MakeItem("s2", 3));
        manager.Do(MakeItem("s3", 4));

        var u1 = manager.Undo();           // s3 → s2
        Assert.Equal(3, u1!.Hash);
        client.Hash = 3;

        var r1 = manager.Redo();           // s2 → s3
        Assert.Equal(4, r1!.Hash);
        client.Hash = 4;

        // Each subsequent undo must step back exactly one state.
        var u2 = manager.Undo();           // s3 → s2
        Assert.Equal(3, u2!.Hash);
        client.Hash = 3;

        var u3 = manager.Undo();           // s2 → s1  (was jumping past s1 before fix)
        Assert.Equal(2, u3!.Hash);
        client.Hash = 2;

        var u4 = manager.Undo();           // s1 → s0
        Assert.Equal(1, u4!.Hash);
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

    [Fact]
    public void CheckForChanges_OverlappingTick_ReturnsWithoutCallingClient()
    {
        // Regression for issue #12683: the 250ms timer keeps firing while a previous
        // tick is still blocked inside the client (marshaling to a busy UI thread);
        // each overlapping tick stacked another blocked thread-pool thread — the
        // reported dump had 492 threads. Overlapping ticks must bail immediately.
        var client = new BlockingHashClient();
        var manager = new UndoRedoManager();
        manager.SetupChangeDetection(client, TimeSpan.FromHours(1));
        manager.StartChangeDetection();

        var firstTick = Task.Run(() => manager.CheckForChanges(null));
        Assert.True(client.HashEntered.Wait(TimeSpan.FromSeconds(10)));

        manager.CheckForChanges(null); // overlapping tick while the first is blocked

        Assert.Equal(1, Volatile.Read(ref client.HashCalls));

        client.ReleaseHash.Set();
        Assert.True(firstTick.Wait(TimeSpan.FromSeconds(10)));
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

    // -----------------------------------------------------------------------
    // UndoRedoItem.Clone
    // -----------------------------------------------------------------------

    [Fact]
    public void Clone_PreservesCreatedTimestamp()
    {
        // Regression: Clone used to overwrite Created with DateTime.Now via the
        // constructor, so every Undo/Redo round-trip restamped the entry as
        // "created now" — surprising for any UI/log that displays Created and
        // breaks chronological ordering of the undo history.
        var originalCreated = new DateTime(2020, 1, 15, 12, 30, 45, DateTimeKind.Utc);
        var original = new UndoRedoItem("test", [], 1, null, [], 0, 0)
        {
            Created = originalCreated,
        };

        var clone = UndoRedoItem.Clone(original);

        Assert.NotNull(clone);
        Assert.Equal(originalCreated, clone.Created);
        // Sanity: hash and description copied too (so the test exercises Clone
        // generally, not just the Created field).
        Assert.Equal(original.Hash, clone.Hash);
        Assert.Equal(original.Description, clone.Description);
    }
}
