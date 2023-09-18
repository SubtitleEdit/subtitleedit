using System;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Logic
{
    public static class TaskDelayHelper
    {
        private static readonly TaskScheduler UiTaskScheduler = TaskScheduler.FromCurrentSynchronizationContext();

        public static void RunDelayed(TimeSpan delay, Action action)
        {
            Task.Delay(delay).ContinueWith(task => { action.Invoke(); }, UiTaskScheduler);
        }
    }
}
