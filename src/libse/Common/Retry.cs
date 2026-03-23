using System;
using System.Threading;

namespace Nikse.SubtitleEdit.Core.Common
{
    public class Retry
    {
        public static void Do(Action action, TimeSpan retryInterval, int maxAttemptCount)
        {
            Do<object>(() =>
            {
                action();
                return null;
            }, retryInterval, maxAttemptCount);
        }

        public static T Do<T>(Func<T> action, TimeSpan retryInterval, int maxAttemptCount)
        {
            var lastException = new Exception();
            for (int attempted = 0; attempted < maxAttemptCount; attempted++)
            {
                try
                {
                    if (attempted > 0)
                    {
                        Thread.Sleep(retryInterval);
                    }
                    return action();
                }
                catch (Exception ex)
                {
                    lastException = ex;
                }
            }

            throw lastException;
        }
    }
}
