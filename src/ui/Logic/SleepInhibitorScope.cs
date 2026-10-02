using System;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Logic;

/// <summary>
/// Holds a <see cref="SleepInhibitor"/> while a window's job runs. Windows whose run state is one
/// flag that every end-of-run path already resets (speech to text, text to speech, burn-in) call
/// <see cref="SetActive"/> from that flag's change handler, so no done/failed/aborted path can
/// forget to release it. Dispose on window close: a close mid-run does not always reset the flag.
/// Safe to call from any thread; acquiring runs in the background (the Linux portal call is async).
/// </summary>
public sealed class SleepInhibitorScope(string reason) : IDisposable
{
    private readonly object _lock = new();
    private IDisposable? _inhibitor;
    private bool _active;
    private int _generation;

    public void SetActive(bool active)
    {
        IDisposable? toRelease;
        int generation;
        lock (_lock)
        {
            if (_active == active)
            {
                return;
            }

            _active = active;
            generation = ++_generation;
            toRelease = _inhibitor;
            _inhibitor = null;
        }

        toRelease?.Dispose();
        if (!active)
        {
            return;
        }

        _ = Task.Run(async () =>
        {
            var inhibitor = await SleepInhibitor.AcquireAsync(reason);
            lock (_lock)
            {
                // Still the same run: keep it. Otherwise the run ended (or a new one began) while
                // acquiring, and this one is stale.
                if (_active && _generation == generation)
                {
                    _inhibitor = inhibitor;
                    return;
                }
            }

            inhibitor.Dispose();
        });
    }

    public void Dispose() => SetActive(false);
}
