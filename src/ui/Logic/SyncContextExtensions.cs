using System;
using System.Threading;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Logic
{
    public static class SyncContextExtensions
    {
        private static readonly TaskScheduler UiTaskScheduler = TaskScheduler.FromCurrentSynchronizationContext();

        public static void Post(this SynchronizationContext context, TimeSpan delay, Action action)
        {
            Task.Delay(delay).ContinueWith(task => { action.Invoke(); }, UiTaskScheduler);
        }
    }
}
