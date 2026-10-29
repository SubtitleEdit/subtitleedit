using System.Collections.Concurrent;
using System.Reflection;
using Nikse.SubtitleEdit.Logic.VideoPlayers.LibMpvDynamic;

namespace UITests.Logic.VideoPlayers;

/// <summary>
/// The render context must be completely freed before the mpv core is destroyed. Closing a window
/// marks the player, its graphics thread starts mpv_render_context_free, and the owner's Dispose
/// on a worker thread used to see the handle already cleared and destroy the core mid-free
/// (SIGSEGV in free_option_data on macOS). The native calls are replaced with recorders here.
/// </summary>
public class LibMpvTeardownRaceTests
{
    private const BindingFlags Flags = BindingFlags.Instance | BindingFlags.NonPublic;

    private sealed class FakeMpv
    {
        public readonly ConcurrentQueue<string> Calls = new();
        public readonly ManualResetEventSlim FreeStarted = new();
        public readonly ManualResetEventSlim ReleaseFree = new();

        public void RenderContextFree(IntPtr context)
        {
            Calls.Enqueue("free-start");
            FreeStarted.Set();
            ReleaseFree.Wait(TimeSpan.FromSeconds(10));
            Calls.Enqueue("free-end");
        }

        public IntPtr TerminateDestroy(IntPtr handle)
        {
            Calls.Enqueue("terminate");
            return IntPtr.Zero;
        }
    }

    [Fact]
    public void Dispose_DuringRenderContextFree_DestroysTheCoreOnlyAfterTheFree()
    {
        var player = new LibMpvDynamicPlayer();
        var fake = new FakeMpv();
        var type = typeof(LibMpvDynamicPlayer);
        type.GetField("_renderContext", Flags)!.SetValue(player, new IntPtr(1));
        type.GetField("_mpv", Flags)!.SetValue(player, new IntPtr(2));
        type.GetField("_renderContextNeedsGraphicsContext", Flags)!.SetValue(player, true);
        var freeField = type.GetField("_mpvRenderContextFree", Flags)!;
        freeField.SetValue(player, Delegate.CreateDelegate(freeField.FieldType, fake, nameof(FakeMpv.RenderContextFree)));
        var terminateField = type.GetField("_mpvTerminateDestroy", Flags)!;
        terminateField.SetValue(player, Delegate.CreateDelegate(terminateField.FieldType, fake, nameof(FakeMpv.TerminateDestroy)));

        // Closing: mark, then the graphics thread starts freeing the render context
        player.MarkForDispose();
        var graphicsThread = new Thread(player.FreeRenderContextIfDisposePending);
        graphicsThread.Start();
        Assert.True(fake.FreeStarted.Wait(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken));

        // ...while the owner's worker thread disposes the player
        var disposeThread = new Thread(player.Dispose);
        disposeThread.Start();
        Thread.Sleep(200);
        Assert.DoesNotContain("terminate", fake.Calls);

        fake.ReleaseFree.Set();
        Assert.True(graphicsThread.Join(TimeSpan.FromSeconds(5)));
        Assert.True(disposeThread.Join(TimeSpan.FromSeconds(5)));
        Assert.True(SpinWait.SpinUntil(() => fake.Calls.Contains("terminate"), TimeSpan.FromSeconds(5)));

        Assert.Equal(["free-start", "free-end", "terminate"], fake.Calls.ToArray());
    }
}
