using System;
using System.Threading;

namespace Nikse.SubtitleEdit.Logic
{
    public static class SyncContextExtensions
    {
        public static void Post(this SynchronizationContext context, TimeSpan delay, Action action)
        {
            var timer = new System.Windows.Forms.Timer { Interval = (int)delay.TotalMilliseconds };
            timer.Tick += delegate
            {
                action.Invoke();
                timer.Stop();
                timer.Dispose();
            };
            timer.Start();
        }
    }
}
