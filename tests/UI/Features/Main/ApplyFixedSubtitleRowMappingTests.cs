using System.Reflection;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using Microsoft.Extensions.DependencyInjection;
using Nikse.SubtitleEdit;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Logic;

namespace UITests.Features.Main;

/// <summary>
/// The subtitle a dialog returns (Multiple replace, Fix common errors, Fix Netflix errors, AI
/// review) is applied onto the very grid rows it was built from, matched by paragraph id.
///
/// Rebuilding the rows instead dropped everything a row holds beside the paragraph: the original
/// text of a translation lives on the row and nowhere else, so Multiple replace emptied the
/// original column and saving the original then wrote empty lines (#14053). Display-only original
/// rows disappeared the same way, as they are never part of the dialog's subtitle.
/// </summary>
public class ApplyFixedSubtitleRowMappingTests
{
    [AvaloniaFact]
    public void ReplacedText_KeepsTheOriginalTextAndTheRowItself()
    {
        var (window, vm) = CreateMainViewModel();
        try
        {
            AddLine(vm, "Translated one", "Original one", 0, 2000);
            AddLine(vm, "Translated two", "Original two", 2000, 4000);
            vm.ShowColumnOriginalText = true;
            var rows = vm.Subtitles.ToList();

            var fixedSubtitle = OpenDialogWith(vm, out var rowMap);
            fixedSubtitle.Paragraphs[0].Text = "Replaced one";
            ApplyFixedSubtitle(vm, fixedSubtitle, rowMap);

            Assert.Equal(new[] { "Replaced one", "Translated two" }, vm.Subtitles.Select(p => p.Text));
            Assert.Equal(new[] { "Original one", "Original two" }, vm.Subtitles.Select(p => p.OriginalText));

            // Same row instances - nothing that hangs off a row (selection, the waveform's match by
            // id, the original text) is invalidated by an edit.
            Assert.Same(rows[0], vm.Subtitles[0]);
            Assert.Same(rows[1], vm.Subtitles[1]);
        }
        finally
        {
            CloseWindow(window, vm);
        }
    }

    /// <summary>
    /// Fix common errors removes empty lines and Fix Netflix errors calls RemoveEmptyLines, so a
    /// result with fewer lines than the grid is an everyday case - and the lines that stay must keep
    /// their original text.
    /// </summary>
    [AvaloniaFact]
    public void DeletedLine_RemovesThatRowOnlyAndKeepsTheOtherOriginals()
    {
        var (window, vm) = CreateMainViewModel();
        try
        {
            AddLine(vm, "Translated one", "Original one", 0, 2000);
            AddLine(vm, "Translated two", "Original two", 2000, 4000);
            AddLine(vm, "Translated three", "Original three", 4000, 6000);
            vm.ShowColumnOriginalText = true;
            var rows = vm.Subtitles.ToList();

            var fixedSubtitle = OpenDialogWith(vm, out var rowMap);
            fixedSubtitle.Paragraphs.RemoveAt(1);
            ApplyFixedSubtitle(vm, fixedSubtitle, rowMap);

            Assert.Equal(new[] { "Translated one", "Translated three" }, vm.Subtitles.Select(p => p.Text));
            Assert.Equal(new[] { "Original one", "Original three" }, vm.Subtitles.Select(p => p.OriginalText));
            Assert.Same(rows[0], vm.Subtitles[0]);
            Assert.Same(rows[2], vm.Subtitles[1]);

            // The remaining lines are renumbered, and the saved original follows the grid.
            Assert.Equal(new[] { 1, 2 }, vm.Subtitles.Select(p => p.Number));
            Assert.Equal(
                new[] { "Original one", "Original three" },
                vm.GetUpdateSubtitleOriginal().Paragraphs.Select(p => p.Text));
        }
        finally
        {
            CloseWindow(window, vm);
        }
    }

    /// <summary>
    /// A removal above the current row shifts the indexes: the row that was current must stay
    /// current, not whichever row now sits at its old index (Remove text for hearing impaired
    /// drops emptied lines this way).
    /// </summary>
    [AvaloniaFact]
    public async Task DeletedLineAboveTheCurrentRow_KeepsTheCurrentRow()
    {
        var (window, vm) = CreateMainViewModel();
        try
        {
            AddLine(vm, "One", "Original one", 0, 2000);
            AddLine(vm, "Two", "Original two", 2000, 4000);
            AddLine(vm, "Three", "Original three", 4000, 6000);
            AddLine(vm, "Four", "Original four", 6000, 8000);
            var rows = vm.Subtitles.ToList();
            vm.SubtitleGrid.SelectedItems?.Clear();
            vm.SubtitleGrid.SelectedItem = rows[2];
            Settle(window);

            var fixedSubtitle = OpenDialogWith(vm, out var rowMap);
            fixedSubtitle.Paragraphs.RemoveAt(0);
            ApplyFixedSubtitle(vm, fixedSubtitle, rowMap, selectedIndex: 2);

            // The selection is posted, so give the dispatcher a real tick as well.
            Settle(window);
            await Task.Delay(50);
            Settle(window);

            Assert.Equal(new[] { "Two", "Three", "Four" }, vm.Subtitles.Select(p => p.Text));
            Assert.Same(rows[2], vm.SelectedSubtitle);
            Assert.Same(rows[2], vm.SubtitleGrid.SelectedItem);
        }
        finally
        {
            CloseWindow(window, vm);
        }
    }

    [AvaloniaFact]
    public void AddedLine_ComesInAsANewRow()
    {
        var (window, vm) = CreateMainViewModel();
        try
        {
            AddLine(vm, "Translated one", "Original one", 0, 2000);
            AddLine(vm, "Translated two", "Original two", 4000, 6000);
            vm.ShowColumnOriginalText = true;
            var rows = vm.Subtitles.ToList();

            var fixedSubtitle = OpenDialogWith(vm, out var rowMap);
            fixedSubtitle.Paragraphs.Insert(1, new Paragraph("Added line", 2000, 4000));
            ApplyFixedSubtitle(vm, fixedSubtitle, rowMap);

            Assert.Equal(
                new[] { "Translated one", "Added line", "Translated two" },
                vm.Subtitles.Select(p => p.Text));
            Assert.Equal(
                new[] { "Original one", string.Empty, "Original two" },
                vm.Subtitles.Select(p => p.OriginalText));
            Assert.Same(rows[0], vm.Subtitles[0]);
            Assert.Same(rows[1], vm.Subtitles[2]);
            Assert.Equal(new[] { 1, 2, 3 }, vm.Subtitles.Select(p => p.Number));
        }
        finally
        {
            CloseWindow(window, vm);
        }
    }

    /// <summary>
    /// Display-only rows hold original lines with no counterpart in the working subtitle, so they
    /// are not in the dialog's subtitle at all: they must survive the apply, and stay where they
    /// were relative to the lines around them.
    /// </summary>
    [AvaloniaFact]
    public void DeletedLine_KeepsTheDisplayOnlyOriginalRowsInPlace()
    {
        var (window, vm) = CreateMainViewModel();
        try
        {
            AddLine(vm, "Translated one", "Original one", 0, 2000);
            AddLine(vm, "Translated two", "Original two", 2000, 4000);
            var referenceRow = AddLine(vm, string.Empty, "Original with no translation", 4000, 6000);
            referenceRow.IsReferenceOnly = true;
            AddLine(vm, "Translated three", "Original three", 6000, 8000);
            vm.ShowColumnOriginalText = true;

            var fixedSubtitle = OpenDialogWith(vm, out var rowMap);

            // Only the three working lines are handed to a dialog.
            Assert.Equal(3, fixedSubtitle.Paragraphs.Count);

            fixedSubtitle.Paragraphs.RemoveAt(1);
            ApplyFixedSubtitle(vm, fixedSubtitle, rowMap);

            Assert.Equal(
                new[] { "Translated one", string.Empty, "Translated three" },
                vm.Subtitles.Select(p => p.Text));
            Assert.Same(referenceRow, vm.Subtitles[1]);
            Assert.True(vm.Subtitles[1].IsReferenceOnly);
            Assert.Equal("Original with no translation", vm.Subtitles[1].OriginalText);

            // A display-only row still takes no number.
            Assert.Equal(new[] { 1, 2 }, vm.Subtitles.Where(p => !p.IsReferenceOnly).Select(p => p.Number));
        }
        finally
        {
            CloseWindow(window, vm);
        }
    }

    /// <summary>
    /// Multiple replace's "Apply" keeps the window open for another round, so the same mapping is
    /// used again - after it has already deleted rows. A row that is gone from the grid must not be
    /// resurrected by a later round.
    /// </summary>
    [AvaloniaFact]
    public void SecondApplyRound_DoesNotBringBackARowTheFirstOneDeleted()
    {
        var (window, vm) = CreateMainViewModel();
        try
        {
            AddLine(vm, "Translated one", "Original one", 0, 2000);
            AddLine(vm, "Translated two", "Original two", 2000, 4000);
            vm.ShowColumnOriginalText = true;
            var rows = vm.Subtitles.ToList();

            var firstRound = OpenDialogWith(vm, out var rowMap);
            firstRound.Paragraphs.RemoveAt(1);
            ApplyFixedSubtitle(vm, firstRound, rowMap);
            Assert.Single(vm.Subtitles);

            // The dialog carries its own subtitle over to the next round with the ids intact.
            var secondRound = new Subtitle(firstRound, false);
            secondRound.Paragraphs[0].Text = "Replaced one";
            ApplyFixedSubtitle(vm, secondRound, rowMap);

            Assert.Single(vm.Subtitles);
            Assert.Same(rows[0], vm.Subtitles[0]);
            Assert.Equal("Replaced one", vm.Subtitles[0].Text);
            Assert.Equal("Original one", vm.Subtitles[0].OriginalText);
        }
        finally
        {
            CloseWindow(window, vm);
        }
    }

    /// <summary>
    /// A rule may empty the grid - a subtitle of nothing but empty lines run through Fix common
    /// errors, say - and there is then no row left to select.
    /// </summary>
    [AvaloniaFact]
    public void EveryLineDeleted_LeavesAnEmptyGrid()
    {
        var (window, vm) = CreateMainViewModel();
        try
        {
            AddLine(vm, "Translated one", "Original one", 0, 2000);
            AddLine(vm, "Translated two", "Original two", 2000, 4000);
            vm.ShowColumnOriginalText = true;

            var fixedSubtitle = OpenDialogWith(vm, out var rowMap);
            fixedSubtitle.Paragraphs.Clear();
            ApplyFixedSubtitle(vm, fixedSubtitle, rowMap);

            Assert.Empty(vm.Subtitles);
        }
        finally
        {
            CloseWindow(window, vm);
        }
    }

    /// <summary>
    /// The safety net for a dialog that hands back a subtitle with fresh paragraph ids: nothing
    /// matches, but as long as the line count is unchanged the rows are still updated in place
    /// rather than rebuilt.
    /// </summary>
    [AvaloniaFact]
    public void ResultWithUnknownIds_AndTheSameLineCount_StillUpdatesTheRowsInPlace()
    {
        var (window, vm) = CreateMainViewModel();
        try
        {
            AddLine(vm, "Translated one", "Original one", 0, 2000);
            AddLine(vm, "Translated two", "Original two", 2000, 4000);
            vm.ShowColumnOriginalText = true;
            var rows = vm.Subtitles.ToList();

            OpenDialogWith(vm, out var rowMap);

            // generateNewId defaults to true, so not one id of this copy is in the map.
            var fixedSubtitle = new Subtitle(vm.GetUpdateSubtitle());
            fixedSubtitle.Paragraphs[1].Text = "Replaced two";
            ApplyFixedSubtitle(vm, fixedSubtitle, rowMap);

            Assert.Equal(new[] { "Translated one", "Replaced two" }, vm.Subtitles.Select(p => p.Text));
            Assert.Equal(new[] { "Original one", "Original two" }, vm.Subtitles.Select(p => p.OriginalText));
            Assert.Same(rows[0], vm.Subtitles[0]);
            Assert.Same(rows[1], vm.Subtitles[1]);
        }
        finally
        {
            CloseWindow(window, vm);
        }
    }

    /// <summary>
    /// What a dialog is handed and the mapping that puts its result back, exactly as the call sites
    /// do it.
    /// </summary>
    private static Subtitle OpenDialogWith(MainViewModel vm, out IReadOnlyDictionary<Guid, SubtitleLineViewModel> rowMap)
    {
        var method = typeof(MainViewModel).GetMethod(
                         "GetUpdateSubtitleWithRowMap", BindingFlags.Instance | BindingFlags.NonPublic)
                     ?? throw new InvalidOperationException("GetUpdateSubtitleWithRowMap not found");

        var args = new object?[] { null };
        var subtitle = (Subtitle)method.Invoke(vm, args)!;
        rowMap = (IReadOnlyDictionary<Guid, SubtitleLineViewModel>)args[0]!;

        // The dialogs work on a copy that keeps the paragraph ids.
        return new Subtitle(subtitle, false);
    }

    private static void ApplyFixedSubtitle(
        MainViewModel vm, Subtitle fixedSubtitle, IReadOnlyDictionary<Guid, SubtitleLineViewModel> rowMap, int selectedIndex = 0)
    {
        var method = typeof(MainViewModel).GetMethod(
                         "ApplyFixedSubtitle",
                         BindingFlags.Instance | BindingFlags.NonPublic,
                         null,
                         new[]
                         {
                             typeof(Subtitle),
                             typeof(IReadOnlyDictionary<Guid, SubtitleLineViewModel>),
                             typeof(int),
                             typeof(SubtitleFormat),
                         },
                         null)
                     ?? throw new InvalidOperationException("ApplyFixedSubtitle not found");

        method.Invoke(vm, new object?[] { fixedSubtitle, rowMap, selectedIndex, null });
        Dispatcher.UIThread.RunJobs();
    }

    private static void Settle(Window window)
    {
        for (var pump = 0; pump < 8; pump++)
        {
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();
        }
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

    private static SubtitleLineViewModel AddLine(
        MainViewModel vm, string text, string originalText, int startMs, int endMs)
    {
        var line = new SubtitleLineViewModel(new Paragraph(text, startMs, endMs), null!)
        {
            OriginalText = originalText,
            Number = vm.Subtitles.Count + 1,
        };

        vm.Subtitles.Add(line);
        return line;
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
