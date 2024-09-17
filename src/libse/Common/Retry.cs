using System;
using System.Threading;

namespace Nikse.SubtitleEdit.Core.Common
{
    /// <summary>
    /// The Retry class provides functionality to execute an action with a retry mechanism.
    /// </summary>
    public class Retry
    {
        /// <summary>
        /// Executes the specified action with a retry mechanism. If the action fails, it will be retried
        /// after the specified interval until the maximum number of attempts is reached.
        /// </summary>
        /// <param name="action">The action to be executed.</param>
        /// <param name="retryInterval">The interval between each retry attempt.</param>
        /// <param name="maxAttemptCount">The maximum number of retry attempts.</param>
        public static void Do(Action action, TimeSpan retryInterval, int maxAttemptCount)
        {
            Do<object>(() =>
            {
                action();
                return null;
            }, retryInterval, maxAttemptCount);
        }

        /// <summary>
        /// Executes the specified action with a retry mechanism. If the action fails, it will be retried
        /// after the specified interval until the maximum number of attempts is reached.
        /// </summary>
        /// <param name="action">The action to be executed.</param>
        /// <param name="retryInterval">The interval between each retry attempt.</param>
        /// <param name="maxAttemptCount">The maximum number of retry attempts.</param>
        /// <typeparam name="T">The type of the return value of the action.</typeparam>
        /// <returns>The result of the action if it succeeds.</returns>
        /// <exception cref="Exception">Throws the last caught exception if the action fails on all attempts.</exception>
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
