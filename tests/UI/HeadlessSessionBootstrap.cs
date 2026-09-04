using Avalonia.Headless;
using UITests;

[assembly: AssemblyFixture(typeof(HeadlessSessionBootstrap))]

namespace UITests;

/// <summary>
/// Starts the shared headless Avalonia session before any test in this assembly runs.
///
/// Avalonia binds its UI thread to whichever thread constructs the process's first
/// <c>Dispatcher</c> ("the first created dispatcher becomes the UI thread one"), and every
/// <c>AvaloniaObject</c> - a <c>SolidColorBrush</c>, a view model's static error brush, a
/// <c>Pen</c> - creates a dispatcher for its constructing thread on demand. With
/// <c>AvaloniaTestIsolationLevel.PerAssembly</c> the session never resets that global
/// (only PerTest isolation does), so whenever a plain <c>[Fact]</c> that touches such an
/// object happens to be scheduled before the first <c>[AvaloniaFact]</c>, the xUnit worker
/// thread becomes the UI thread and the session can no longer initialize: every Avalonia
/// test then fails in about 1 ms with "The calling thread cannot access this object" out of
/// <c>AvaloniaHeadlessPlatform.Initialize</c> → <c>DefaultRenderLoop.Add</c>, with no earlier
/// failure to point at (1104 of 4631 tests on one CI attempt). Test class order is not
/// stable across processes, so this is a 1-in-N run, and the workflow's retry cannot save
/// a run that dies this way.
///
/// An assembly fixture is constructed before the first test of the assembly executes.
/// Dispatching a no-op here forces <c>EnsureSharedApplication</c> to run on the session's
/// own thread first, so that thread owns the UI dispatcher for the whole process. (A
/// <c>[ModuleInitializer]</c> cannot do this: it holds the module's initialization lock
/// while the session thread calls back into <c>TestAppBuilder</c>, and the process hangs.)
/// </summary>
public sealed class HeadlessSessionBootstrap
{
    public HeadlessSessionBootstrap()
    {
        var session = HeadlessUnitTestSession.GetOrStartForAssembly(typeof(HeadlessSessionBootstrap).Assembly);
        session.Dispatch(static () => { }, CancellationToken.None).GetAwaiter().GetResult();
    }
}
