using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using Microsoft.Extensions.DependencyInjection;
using Nikse.SubtitleEdit;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Logic;

namespace UITests.Features.Main;

/// <summary>
/// Ctrl+V in the subtitle grid must leave the pasted lines selected, like SE4 - the selection
/// (and with it the edit text box) used to stay on the line selected before the paste (#13705).
/// </summary>
public class MainGridPasteSelectionTests
{
    [AvaloniaFact]
    public void Paste_SelectsThePastedLine()
    {
        var (window, vm) = CreateMainViewModel();
        try
        {
            AddLine(vm, "Existing one", 1000, 2000);
            AddLine(vm, "Existing two", 3000, 4000);
            SelectRow(vm, 0);

            Paste(window, vm, Srt(("Pasted one", "00:00:10,000", "00:00:11,000")));

            Assert.Equal(3, vm.Subtitles.Count);
            Assert.Equal("Pasted one", vm.Subtitles[1].Text);

            // The pasted line is the selected one, so the edit box shows it.
            Assert.Same(vm.Subtitles[1], vm.SelectedSubtitle);
            Assert.Same(vm.Subtitles[1], Assert.Single(vm.SubtitleGridSelectedItems));
        }
        finally
        {
            CloseWindow(window, vm);
        }
    }

    [AvaloniaFact]
    public void Paste_SelectsAllPastedLines_WithTheFirstAsTheCurrentRow()
    {
        var (window, vm) = CreateMainViewModel();
        try
        {
            AddLine(vm, "Existing one", 1000, 2000);
            AddLine(vm, "Existing two", 3000, 4000);
            SelectRow(vm, 0);

            Paste(window, vm, Srt(
                ("Pasted one", "00:00:10,000", "00:00:11,000"),
                ("Pasted two", "00:00:12,000", "00:00:13,000")));

            Assert.Equal(4, vm.Subtitles.Count);
            var selected = vm.SubtitleGridSelectedItems;
            Assert.Equal(2, selected.Count);
            Assert.Same(vm.Subtitles[1], selected[0]);
            Assert.Same(vm.Subtitles[2], selected[1]);
            Assert.Same(vm.Subtitles[1], vm.SelectedSubtitle);
        }
        finally
        {
            CloseWindow(window, vm);
        }
    }

    // Pasting with nothing selected appends at the end (#13200); the appended lines get selected
    // too, so the grid never keeps pointing at a line the user did not paste.
    [AvaloniaFact]
    public void Paste_WithoutSelection_SelectsTheAppendedLine()
    {
        var (window, vm) = CreateMainViewModel();
        try
        {
            AddLine(vm, "Existing one", 1000, 2000);
            vm.SubtitleGrid.SelectedItem = null;
            vm.SelectedSubtitleIndex = null;
            Dispatcher.UIThread.RunJobs();

            Paste(window, vm, Srt(("Pasted one", "00:00:10,000", "00:00:11,000")));

            Assert.Equal(2, vm.Subtitles.Count);
            Assert.Same(vm.Subtitles[1], vm.SelectedSubtitle);
        }
        finally
        {
            CloseWindow(window, vm);
        }
    }

    /// <summary>
    /// More than one row selected and a subtitle format on the clipboard: the selected lines are
    /// replaced, not pushed down - SE4's "multiple lines selected - first delete, then insert"
    /// (#13682). The pasted line count does not have to match the selection.
    /// </summary>
    [AvaloniaFact]
    public void Paste_SubtitleOverMultipleSelectedLines_ReplacesThem()
    {
        var (window, vm) = CreateMainViewModel();
        try
        {
            AddLine(vm, "Keep before", 1000, 2000);
            AddLine(vm, "Replace one", 3000, 4000);
            AddLine(vm, "Replace two", 5000, 6000);
            AddLine(vm, "Replace three", 7000, 8000);
            AddLine(vm, "Keep after", 9000, 10000);
            SelectRows(vm, 1, 2, 3);

            Paste(window, vm, Srt(
                ("Pasted one", "00:00:10,000", "00:00:11,000"),
                ("Pasted two", "00:00:12,000", "00:00:13,000")));

            Assert.Equal(new[] { "Keep before", "Pasted one", "Pasted two", "Keep after" },
                vm.Subtitles.Select(p => p.Text).ToArray());

            // the pasted time codes are kept, like SE4
            Assert.Equal(10000, vm.Subtitles[1].StartTime.TotalMilliseconds);
            Assert.Equal(13000, vm.Subtitles[2].EndTime.TotalMilliseconds);

            var selected = vm.SubtitleGridSelectedItems;
            Assert.Equal(2, selected.Count);
            Assert.Same(vm.Subtitles[1], selected[0]);
            Assert.Same(vm.Subtitles[2], selected[1]);
        }
        finally
        {
            CloseWindow(window, vm);
        }
    }

    /// <summary>
    /// The rows are replaced from the topmost selected one whichever way the selection was made -
    /// the grid's own SelectedItems follows the order the rows were picked in, and anchoring on
    /// that made the same paste land in a different place for a bottom-to-top selection (#13682).
    /// </summary>
    [AvaloniaFact]
    public void Paste_SubtitleOverASelectionPickedBottomToTop_ReplacesTheSameLines()
    {
        var (window, vm) = CreateMainViewModel();
        try
        {
            AddLine(vm, "Keep before", 1000, 2000);
            AddLine(vm, "Replace one", 3000, 4000);
            AddLine(vm, "Replace two", 5000, 6000);
            AddLine(vm, "Keep after", 7000, 8000);
            SelectRows(vm, 2, 1); // bottom row first

            Paste(window, vm, Srt(("Pasted one", "00:00:10,000", "00:00:11,000")));

            Assert.Equal(new[] { "Keep before", "Pasted one", "Keep after" },
                vm.Subtitles.Select(p => p.Text).ToArray());
        }
        finally
        {
            CloseWindow(window, vm);
        }
    }

    /// <summary>
    /// Plain text (translations copied out of a text document) writes one clipboard line into each
    /// selected row and leaves the time codes alone - what "Column > Paste from clipboard > text
    /// only + replace existing cells" does, without the menus and the dialog (#13682).
    /// </summary>
    [AvaloniaFact]
    public void Paste_PlainTextOverMultipleSelectedLines_ReplacesTheTextOnly()
    {
        var (window, vm) = CreateMainViewModel();
        try
        {
            AddLine(vm, "Keep before", 1000, 2000);
            AddLine(vm, "Original one", 3000, 4000);
            AddLine(vm, "Original two", 5000, 6000);
            SelectRows(vm, 1, 2);

            Paste(window, vm, "Translated one" + Environment.NewLine + "Translated two" + Environment.NewLine);

            Assert.Equal(new[] { "Keep before", "Translated one", "Translated two" },
                vm.Subtitles.Select(p => p.Text).ToArray());
            Assert.Equal(3000, vm.Subtitles[1].StartTime.TotalMilliseconds);
            Assert.Equal(6000, vm.Subtitles[2].EndTime.TotalMilliseconds);
        }
        finally
        {
            CloseWindow(window, vm);
        }
    }

    // More clipboard lines than selected rows: the extra lines are dropped rather than written
    // into lines the user did not select.
    [AvaloniaFact]
    public void Paste_PlainTextWithMoreLinesThanSelectedRows_StopsAtTheSelection()
    {
        var (window, vm) = CreateMainViewModel();
        try
        {
            AddLine(vm, "Original one", 1000, 2000);
            AddLine(vm, "Original two", 3000, 4000);
            AddLine(vm, "Keep after", 5000, 6000);
            SelectRows(vm, 0, 1);

            Paste(window, vm, string.Join(Environment.NewLine, "Translated one", "Translated two", "Translated three"));

            Assert.Equal(new[] { "Translated one", "Translated two", "Keep after" },
                vm.Subtitles.Select(p => p.Text).ToArray());
        }
        finally
        {
            CloseWindow(window, vm);
        }
    }

    // A single selected row still inserts below it - the SE4 behavior for the everyday paste.
    [AvaloniaFact]
    public void Paste_PlainTextWithOneRowSelected_StillInserts()
    {
        var (window, vm) = CreateMainViewModel();
        try
        {
            AddLine(vm, "Original one", 1000, 2000);
            AddLine(vm, "Original two", 3000, 4000);
            SelectRow(vm, 0);

            Paste(window, vm, "Pasted text");

            Assert.Equal(new[] { "Original one", "Pasted text", "Original two" },
                vm.Subtitles.Select(p => p.Text).ToArray());
        }
        finally
        {
            CloseWindow(window, vm);
        }
    }

    private static void Paste(Window window, MainViewModel vm, string clipboardText)
    {
        ClipboardHelper.SetTextAsync(window, clipboardText).GetAwaiter().GetResult();
        Dispatcher.UIThread.RunJobs();

        vm.SubtitleGridPasteCommand.ExecuteAsync(null).GetAwaiter().GetResult();
        Dispatcher.UIThread.RunJobs();
    }

    private static string Srt(params (string Text, string Start, string End)[] lines)
    {
        var sb = new System.Text.StringBuilder();
        for (var i = 0; i < lines.Length; i++)
        {
            sb.AppendLine((i + 1).ToString());
            sb.AppendLine(lines[i].Start + " --> " + lines[i].End);
            sb.AppendLine(lines[i].Text);
            sb.AppendLine();
        }

        return sb.ToString();
    }

    private static void SelectRow(MainViewModel vm, int index)
    {
        Dispatcher.UIThread.RunJobs();
        vm.SubtitleGrid.SelectedItem = vm.Subtitles[index];
        Dispatcher.UIThread.RunJobs();
    }

    /// <summary>
    /// Selects several rows in the order given - the grid's own SelectedItems keeps that order, so
    /// passing the rows bottom-to-top reproduces an upwards shift-selection.
    /// </summary>
    private static void SelectRows(MainViewModel vm, params int[] indexes)
    {
        Dispatcher.UIThread.RunJobs();
        vm.SubtitleGrid.SelectedItem = vm.Subtitles[indexes[0]];
        Dispatcher.UIThread.RunJobs();

        var selectedItems = vm.SubtitleGrid.SelectedItems!;
        selectedItems.Clear();
        foreach (var index in indexes)
        {
            selectedItems.Add(vm.Subtitles[index]);
        }

        Dispatcher.UIThread.RunJobs();
    }

    private static (Window Window, MainViewModel Vm) CreateMainViewModel()
    {
        var services = new ServiceCollection();
        services.AddSubtitleEditServices();
        Locator.Services = services.BuildServiceProvider();

        var window = new Window { Width = 1200, Height = 800 };
        MainView.NextHostWindow = window;
        var view = new MainView();
        window.Content = view;
        window.Show();
        Dispatcher.UIThread.RunJobs();

        return (window, (MainViewModel)view.DataContext!);
    }

    private static void AddLine(MainViewModel vm, string text, int startMs, int endMs)
    {
        vm.Subtitles.Add(new SubtitleLineViewModel(new Paragraph(text, startMs, endMs), null!)
        {
            Number = vm.Subtitles.Count + 1,
        });
    }

    private static void CloseWindow(Window window, MainViewModel vm)
    {
        foreach (var ownedWindow in window.OwnedWindows.ToArray())
        {
            ownedWindow.Close();
        }

        window.Closing -= vm.OnClosing;
        if (window.IsVisible)
        {
            window.Close();
        }
    }
}
