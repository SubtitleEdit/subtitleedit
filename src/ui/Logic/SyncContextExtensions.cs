using System;
using System.Threading;

namespace Nikse.SubtitleEdit.Logic
{
    public static class SyncContextExtensions
    {
        public static void Post(this SynchronizationContext context, TimeSpan delay, Action action)
        {
            Timer timer = null;
            timer = new Timer(ignore =>
            {
                timer?.Dispose();
                context.Post(ignore2 => action(), null);
            }, null, delay, TimeSpan.FromMilliseconds(-1));
        }
    }
}
