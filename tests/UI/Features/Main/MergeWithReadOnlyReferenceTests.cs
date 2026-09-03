using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using Microsoft.Extensions.DependencyInjection;
using Nikse.SubtitleEdit;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Features.Main.MainHelpers;
using Nikse.SubtitleEdit.Logic;
using System.Collections.Specialized;
using System.Reflection;

namespace UITests.Features.Main;

/// <summary>
/// Merging lines while a read-only original is open. Every merge leaves one more display-only
/// reference row behind - the line the merged-away row displayed comes back, the file being
/// authoritative - so anything a merge does per reference row gets slower with every merge the
/// user makes. That is what made the grid crawl up and back down after each merge, reported as
/// #13962, re-reported as #14003 and again against 5.2.0-rc1 as #14468.
///
/// <see cref="MainViewModel.WithoutReferenceOnlyRows"/> therefore only detaches the reference rows
/// the command can reach, and the posted scroll owns the offset while it is pending.
/// </summary>
public class MergeWithReadOnlyReferenceTests
{
    /// <summary>
    /// The regression test for the slowdown: a merge nowhere near the reference rows must not
    /// touch them. Before, every one of them was removed and re-inserted on the collection the
    /// grid is bound to - two change notifications each, per merge, growing with every merge.
    /// </summary>
    [AvaloniaFact]
    public void Merge_DoesNotChurnTheReferenceRowsItCannotReach()
    {
        var (window, vm) = CreateMainViewModel();
        try
        {
            // 40 working rows, and a reference with 20 extra lines - all of them up at the top,
            // far from the rows merged down at the bottom.
            for (var i = 0; i < 40; i++)
            {
                AddLine(vm, "Translated " + i, i * 10000, i * 10000 + 5000);
            }

            var reference = new Subtitle();
            for (var i = 0; i < 40; i++)
            {
                reference.Paragraphs.Add(new Paragraph("Reference " + i, i * 10000, i * 10000 + 5000));
                if (i < 20)
                {
                    reference.Paragraphs.Add(new Paragraph("Reference only " + i, i * 10000 + 6000, i * 10000 + 7000));
                }
            }

            vm.ShowColumnOriginalText = true;
            ImportReference(vm, reference);
            Dispatcher.UIThread.RunJobs();

            Assert.True(vm.IsShowingOriginalNonMatchingLines);
            Assert.True(vm.IsOriginalReadOnly);
            Assert.Equal(20, CountReferenceRows(vm));

            var changes = 0;
            void OnChanged(object? _, NotifyCollectionChangedEventArgs __) => changes++;
            vm.Subtitles.CollectionChanged += OnChanged;
            try
            {
                vm.SelectedSubtitle = vm.Subtitles[^2];
                SelectInGrid(vm, vm.Subtitles[^2]);
                vm.MergeWithLineAfterCommand.Execute(null);
                Dispatcher.UIThread.RunJobs();
            }
            finally
            {
                vm.Subtitles.CollectionChanged -= OnChanged;
            }

            // The merge itself removes one row and the refresh brings the merged-away original
            // line back as one more reference row. Detaching the 20 untouched reference rows and
            // putting them back would add 40 notifications on top of that.
            Assert.True(changes <= 5, $"the merge raised {changes} collection changes - it is still churning the reference rows");
            Assert.Equal(21, CountReferenceRows(vm));
        }
        finally
        {
            window.Close();
        }
    }

    /// <summary>
    /// The reference rows the command <i>can</i> reach still have to come out: a merge whose two
    /// rows have a display-only row between them must merge the two translated lines, not swallow
    /// the reference row and stretch the survivor over its span (#13449).
    /// </summary>
    [AvaloniaFact]
    public void MergeWithLineAfter_SkipsAReferenceRowSittingBetweenTheTwoLines()
    {
        var (window, vm) = CreateMainViewModel();
        try
        {
            AddLine(vm, "Translated one", 0, 2000);
            AddLine(vm, "Translated two", 4000, 6000);

            var reference = new Subtitle();
            reference.Paragraphs.Add(new Paragraph("Reference one", 0, 2000));
            reference.Paragraphs.Add(new Paragraph("Reference only", 2000, 4000));
            reference.Paragraphs.Add(new Paragraph("Reference two", 4000, 6000));

            vm.ShowColumnOriginalText = true;
            ImportReference(vm, reference);
            Dispatcher.UIThread.RunJobs();

            // grid: [Translated one] [Reference only] [Translated two]
            Assert.True(vm.Subtitles[1].IsReferenceOnly);

            vm.SelectedSubtitle = vm.Subtitles[0];
            SelectInGrid(vm, vm.Subtitles[0]);
            vm.MergeWithLineAfterCommand.Execute(null);
            Dispatcher.UIThread.RunJobs();

            var working = vm.Subtitles.Where(r => !r.IsReferenceOnly).ToList();
            Assert.Single(working);
            Assert.Contains("Translated one", working[0].Text);
            Assert.Contains("Translated two", working[0].Text);
            Assert.Equal(6000, working[0].EndTime.TotalMilliseconds);

            // The line the merged-away row displayed is back, next to the one that never matched.
            Assert.Equal(2, CountReferenceRows(vm));
        }
        finally
        {
            window.Close();
        }
    }

    /// <summary>
    /// The same one row up: "merge with line before" must reach past a display-only row to the
    /// previous translated line.
    /// </summary>
    [AvaloniaFact]
    public void MergeWithLineBefore_SkipsAReferenceRowSittingBetweenTheTwoLines()
    {
        var (window, vm) = CreateMainViewModel();
        try
        {
            AddLine(vm, "Translated one", 0, 2000);
            AddLine(vm, "Translated two", 4000, 6000);

            var reference = new Subtitle();
            reference.Paragraphs.Add(new Paragraph("Reference one", 0, 2000));
            reference.Paragraphs.Add(new Paragraph("Reference only", 2000, 4000));
            reference.Paragraphs.Add(new Paragraph("Reference two", 4000, 6000));

            vm.ShowColumnOriginalText = true;
            ImportReference(vm, reference);
            Dispatcher.UIThread.RunJobs();

            var second = vm.Subtitles.Last(r => !r.IsReferenceOnly);
            vm.SelectedSubtitle = second;
            SelectInGrid(vm, second);
            vm.MergeWithLineBeforeCommand.Execute(null);
            Dispatcher.UIThread.RunJobs();

            var working = vm.Subtitles.Where(r => !r.IsReferenceOnly).ToList();
            Assert.Single(working);
            Assert.Contains("Translated one", working[0].Text);
            Assert.Contains("Translated two", working[0].Text);
        }
        finally
        {
            window.Close();
        }
    }

    /// <summary>
    /// The scroll a command posts owns the offset until it has run: the scroll anchor would
    /// otherwise correct the extent the restructured rows re-estimate and haul the view back to
    /// the row that was on top, only for the posted scroll to travel to the target again. The
    /// suspension must not leak, or ordinary editing stops holding the view steady (#13619).
    /// </summary>
    [AvaloniaFact]
    public void PostedScroll_HoldsTheScrollAnchorUntilItHasRun()
    {
        var (window, vm) = CreateMainViewModel();
        try
        {
            AddLine(vm, "Translated one", 0, 2000);
            AddLine(vm, "Translated two", 4000, 6000);
            Dispatcher.UIThread.RunJobs();

            var anchor = TableViewScrollAnchor.GetFor(vm.SubtitleGrid);
            Assert.NotNull(anchor);
            Assert.Equal(0, SuspendCountOf(anchor!));

            InvokeSelectAndScrollToRow(vm, vm.Subtitles[1]);
            Assert.True(SuspendCountOf(anchor!) > 0, "the posted scroll did not take the anchor");

            Dispatcher.UIThread.RunJobs();
            Assert.Equal(0, SuspendCountOf(anchor!));

            // Twice: a leak would stack.
            InvokeSelectAndScrollToRow(vm, vm.Subtitles[0]);
            Dispatcher.UIThread.RunJobs();
            Assert.Equal(0, SuspendCountOf(anchor!));
        }
        finally
        {
            window.Close();
        }
    }

    private static int CountReferenceRows(MainViewModel vm) => vm.Subtitles.Count(r => r.IsReferenceOnly);

    private static int SuspendCountOf(TableViewScrollAnchor anchor) =>
        (int)typeof(TableViewScrollAnchor)
            .GetField("_suspendCount", BindingFlags.NonPublic | BindingFlags.Instance)!
            .GetValue(anchor)!;

    private static void InvokeSelectAndScrollToRow(MainViewModel vm, SubtitleLineViewModel row) =>
        typeof(MainViewModel)
            .GetMethod(
                "SelectAndScrollToRow",
                BindingFlags.Instance | BindingFlags.NonPublic,
                new[] { typeof(SubtitleLineViewModel), typeof(bool?) })!
            .Invoke(vm, new object?[] { row, null });

    // The detach window is read from the grid's selection, so the tests have to set it - assigning
    // SelectedSubtitle alone leaves SubtitleGrid.SelectedItems empty.
    private static void SelectInGrid(MainViewModel vm, SubtitleLineViewModel row)
    {
        vm.SubtitleGrid.SelectedItems?.Clear();
        vm.SubtitleGrid.SelectedItem = row;
    }

    private static void ImportReference(MainViewModel vm, Subtitle reference)
    {
        var match = ImportOriginalHelper.MatchOriginalLines(vm.Subtitles, reference);
        typeof(MainViewModel)
            .GetMethod("ImportOriginalSubtitle", BindingFlags.Instance | BindingFlags.NonPublic)!
            .Invoke(vm, new object?[] { 0, "reference.srt", reference, match, true });
    }

    private static SubtitleLineViewModel AddLine(MainViewModel vm, string text, int startMs, int endMs)
    {
        var line = new SubtitleLineViewModel(new Paragraph(text, startMs, endMs), null!)
        {
            Number = vm.Subtitles.Count + 1,
        };

        vm.Subtitles.Add(line);
        return line;
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
        var vm = (MainViewModel)view.DataContext!;
        window.SuppressSaveChangesPromptOnClose(vm);
        return (window, vm);
    }
}
