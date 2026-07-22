using System.Timers;

namespace Nikse.SubtitleEdit.Logic;

public static class TimerExtensions
{
    public static void StopAndDispose(this Timer timer, ElapsedEventHandler elapsedHandler)
    {
        timer.Elapsed -= elapsedHandler;
        timer.Stop();
        timer.Dispose();
    }
}
