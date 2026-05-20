using Avalonia.Threading;
using Nikse.SubtitleEdit.Core.Common;
using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Logic;

public static class DispatcherExtensions
{
    // Use instead of Post(async () => ...): a plain Post takes an Action, so an async lambda
    // becomes async void and any exception thrown after the first await is silently swallowed.
    public static void PostSafe(this Dispatcher dispatcher, Func<Task> callback, [CallerMemberName] string caller = "")
    {
        dispatcher.Post(() => _ = RunSafeAsync(callback, caller));
    }

    private static async Task RunSafeAsync(Func<Task> callback, string caller)
    {
        try
        {
            await callback();
        }
        catch (Exception ex)
        {
            SeLogger.Error(ex, $"Unhandled exception in dispatcher callback from {caller}");
        }
    }
}
