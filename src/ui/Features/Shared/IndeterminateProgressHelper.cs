using Avalonia.Threading;
using System;
using System.Timers;

namespace Nikse.SubtitleEdit.Features.Shared;

public class IndeterminateProgressHelper : IDisposable
{
    private Timer? _indeterminateTimer;
    private DateTime _indeterminateStartUtc;
    private const int IndeterminateIntervalMs = 16; // ~60 FPS
    private const double PositionCycleSeconds = 10; // seconds per sweep 0 -> 100 (increase to slow, decrease to speed up)
    private const double PulseFrequencyHz = 1.2; // opacity pulse frequency
    private const double MinOpacity = 0.35; // lower to make pulse more visible
    private const double MaxOpacity = 1.0;

    private readonly Action<double> _setProgressValue;
    private readonly Action<double> _setProgressOpacity;
    private readonly Func<bool> _isCancellationRequested;

    public IndeterminateProgressHelper(
        Action<double> setProgressValue,
        Action<double> setProgressOpacity,
        Func<bool>? isCancellationRequested = null)
    {
        _setProgressValue = setProgressValue;
        _setProgressOpacity = setProgressOpacity;
        _isCancellationRequested = isCancellationRequested ?? (() => false);
    }

    public void Start()
    {
        Stop();
        _setProgressOpacity(1.0);
        _setProgressValue(0);
        _indeterminateStartUtc = DateTime.UtcNow;
        _indeterminateTimer = new Timer(IndeterminateIntervalMs);
        _indeterminateTimer.Elapsed += (_, __) =>
        {
            if (_isCancellationRequested())
            {
                Stop();
                return;
            }

            // Time since start
            var t = (DateTime.UtcNow - _indeterminateStartUtc).TotalSeconds;

            // Forward-only sweep with easing (0 ->100), then reset (marquee-like)
            var pos01 = (t % PositionCycleSeconds) / PositionCycleSeconds; // 0..1
            pos01 = EaseInOutCubic(pos01);
            var nextValue = pos01 * 100.0;

            // Opacity pulse using sine wave
            var s = Math.Sin(2 * Math.PI * PulseFrequencyHz * t); // -1..1
            var nextOpacity = MinOpacity + (MaxOpacity - MinOpacity) * ((s + 1) / 2.0); // Min..Max

            Dispatcher.UIThread.Post(() =>
            {
                _setProgressValue(nextValue);
                _setProgressOpacity(nextOpacity);
            });
        };
        _indeterminateTimer.AutoReset = true;
        _indeterminateTimer.Start();
    }

    public void Stop()
    {
        if (_indeterminateTimer != null)
        {
            try
            {
                _indeterminateTimer.Stop();
                _indeterminateTimer.Dispose();
            }
            catch
            {
                // ignore
            }
            finally
            {
                _indeterminateTimer = null;
            }
        }

        // Restore opacity
        _setProgressOpacity(1.0);
    }

    private static double EaseInOutCubic(double x)
    {
        if (x <= 0) return 0;
        if (x >= 1) return 1;
        return x < 0.5 ? 4 * x * x * x : 1 - Math.Pow(-2 * x + 2, 3) / 2;
    }

    public void Dispose()
    {
        Stop();
    }
}
