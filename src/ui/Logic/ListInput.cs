using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Nikse.SubtitleEdit.Logic;

/// <summary>
/// Keyboard and drag-drop behavior shared by the editable lists in dialogs (batch file
/// lists, style lists, profile lists), so they all act the same as Join and Batch convert.
/// </summary>
public static class ListInput
{
    /// <summary>
    /// Delete on <paramref name="list"/> runs <paramref name="command"/>, the same command as
    /// the list's Remove/Delete button. Ignored while a text box inside the list has focus
    /// (it needs Delete for itself), when the command can't run, and when
    /// <paramref name="isEnabled"/> says no - pass the condition that disables the button.
    /// </summary>
    public static void RunOnDeleteKey(Control list, ICommand command, Func<bool>? isEnabled = null)
    {
        list.KeyDown += (_, e) =>
        {
            if (e.Key != Key.Delete || e.KeyModifiers != KeyModifiers.None || e.Source is TextBox ||
                !command.CanExecute(null) || isEnabled?.Invoke() == false)
            {
                return;
            }

            e.Handled = true;
            command.Execute(null);
        };
    }

    /// <summary>
    /// Wraps <paramref name="list"/> in a transparent border that accepts dropped files, so a
    /// drop also works on the empty area below the rows. Folders are skipped.
    /// </summary>
    public static Border MakeFileDropHost(Control list, Func<string[], Task> addFiles)
    {
        var host = new Border
        {
            Background = Brushes.Transparent,
            Child = list,
        };

        DragDrop.SetAllowDrop(host, true);
        host.AddHandler(DragDrop.DragOverEvent, (_, e) =>
        {
            e.DragEffects = e.DataTransfer.Contains(DataFormat.File) ? DragDropEffects.Copy : DragDropEffects.None;
            e.Handled = true;
        }, RoutingStrategies.Bubble);
        host.AddHandler(DragDrop.DropEvent, (_, e) =>
        {
            if (!e.DataTransfer.Contains(DataFormat.File))
            {
                return;
            }

            var fileNames = e.DataTransfer.TryGetFiles()?
                .OfType<IStorageFile>()
                .Select(p => p.Path.LocalPath)
                .Where(File.Exists)
                .ToArray();
            if (fileNames is { Length: > 0 })
            {
                e.Handled = true;
                Dispatcher.UIThread.Post(async () => await addFiles(fileNames));
            }
        }, RoutingStrategies.Bubble);

        return host;
    }
}
