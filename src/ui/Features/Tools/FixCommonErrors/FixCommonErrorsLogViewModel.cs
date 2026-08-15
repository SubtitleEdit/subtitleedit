using Avalonia.Controls;
using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Optris.Icons.Avalonia;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Tools.FixCommonErrors;

/// <summary>
/// Shows the messages the fix rules reported through <see cref="Core.Interfaces.IFixCallbacks.LogStatus"/> -
/// mostly errors that were found but could not be fixed, e.g. a display time that is too short with no
/// room to extend it. This is SE4's "Log" tab in step 2 of fix common errors (#13645).
/// </summary>
public partial class FixCommonErrorsLogViewModel : ObservableObject
{
    [ObservableProperty] private string _logText = string.Empty;
    [ObservableProperty] private string _importantMessagesText = string.Empty;
    [ObservableProperty] private bool _importantMessagesIsVisible;

    public Window? Window { get; set; }
    public Button? CopyButton { get; set; }

    /// <summary>
    /// The scan half first (what is still wrong), then the applied half (what the applies changed),
    /// separated by a blank line - the order SE4's log tab used.
    /// </summary>
    public void Initialize(IEnumerable<string> logEntries, IEnumerable<string> appliedLogEntries, int numberOfImportantLogMessages)
    {
        var separator = Environment.NewLine + Environment.NewLine;
        var sections = new[]
        {
            string.Join(separator, logEntries),
            string.Join(Environment.NewLine, appliedLogEntries),
        };

        LogText = string.Join(separator, sections.Where(s => !string.IsNullOrEmpty(s)));
        ImportantMessagesIsVisible = numberOfImportantLogMessages > 0;
        ImportantMessagesText = ImportantMessagesIsVisible
            ? string.Format(Se.Language.Tools.FixCommonErrors.NumberOfImportantLogMessages, numberOfImportantLogMessages)
            : string.Empty;
    }

    [RelayCommand]
    private async Task Copy()
    {
        if (Window == null || string.IsNullOrEmpty(LogText))
        {
            return;
        }

        await ClipboardHelper.SetTextAsync(Window, LogText);

        if (CopyButton != null)
        {
            Attached.SetIcon(CopyButton, IconNames.Check);
            await Task.Delay(1500);
            Attached.SetIcon(CopyButton, IconNames.Copy);
        }
    }

    [RelayCommand]
    private void Close()
    {
        Window?.Close();
    }

    internal void OnKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            e.Handled = true;
            Window?.Close();
        }
    }
}
