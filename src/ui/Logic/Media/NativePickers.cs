using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Logic.Media;

/// <summary>
/// Wrappers around the StorageProvider pickers that suspend Topmost on all app windows while
/// the native dialog is open.
///
/// Dialogs shown via WindowService.ShowDialogAsync are Topmost while active (so they stay above
/// the undocked video/audio windows, #11971). On Windows a native file/folder picker is a plain
/// non-topmost window owned by the window that opened it - and a non-topmost window can never
/// sit above a topmost one. So a picker opened from such a dialog (e.g. the text-to-speech
/// window's Import) appeared *behind* its own topmost owner: the user saw the picker and the
/// owner vanish behind the main window, reachable only via the taskbar (#12093). Dropping
/// Topmost for the picker's lifetime avoids that; the previous values are restored afterwards
/// (and the owner's re-activation re-asserts the correct state via KeepTopmostWhileOwnerActive).
/// </summary>
public static class NativePickers
{
    public static Task<IReadOnlyList<IStorageFile>> OpenFilePickerAsync(TopLevel topLevel, FilePickerOpenOptions options)
    {
        return RunWithTopmostSuspendedAsync(() => topLevel.StorageProvider.OpenFilePickerAsync(options));
    }

    public static Task<IStorageFile?> SaveFilePickerAsync(TopLevel topLevel, FilePickerSaveOptions options)
    {
        return RunWithTopmostSuspendedAsync(() => topLevel.StorageProvider.SaveFilePickerAsync(options));
    }

    public static Task<IReadOnlyList<IStorageFolder>> OpenFolderPickerAsync(TopLevel topLevel, FolderPickerOpenOptions options)
    {
        return RunWithTopmostSuspendedAsync(() => topLevel.StorageProvider.OpenFolderPickerAsync(options));
    }

    private static async Task<T> RunWithTopmostSuspendedAsync<T>(Func<Task<T>> showNativeDialog)
    {
        var suspended = new List<Window>();
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            foreach (var window in desktop.Windows)
            {
                if (window.Topmost)
                {
                    window.Topmost = false;
                    suspended.Add(window);
                }
            }
        }

        try
        {
            return await showNativeDialog();
        }
        finally
        {
            foreach (var window in suspended)
            {
                window.Topmost = true;
            }
        }
    }
}
