using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using Microsoft.Extensions.DependencyInjection;
using Nikse.SubtitleEdit;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Features.Main.MainHelpers;
using Nikse.SubtitleEdit.Logic;
using System;
using System.Collections.Specialized;
using System.Reflection;

namespace UITests.Features.Main;

/// <summary>
/// Rebuilding the read-only reference projection churns the row collection: display-only rows are
/// removed, brought back and glided into order, one row at a time. Each of those makes the
/// virtualizing panel re-estimate its extent, which <see cref="TableViewScrollAnchor"/> reads as
/// "the panel moved under the view" and answers with a full restore - PrePositionScroll, a
/// ScrollIntoView and up to three synchronous layout passes.
///
/// Paying that per row is the slow scroll up and back down after merging that survived the #13962
/// fix and was reported again as #14003. The rebuild therefore suspends the anchor for its whole
/// run: it keeps following the view, it just stops moving it, so the burst settles once at the end.
/// </summary>
public class ReferenceRebuildScrollAnchorTests
{
    [AvaloniaFact]
    public void ReferenceRebuild_SuspendsTheScrollAnchorWhileTheRowsChurn()
    {
        var (window, vm) = CreateMainViewModel();
        try
        {
            AddLine(vm, "Translated one", "Reference one", 0, 2000);
            AddLine(vm, "Translated two", "Reference two", 4000, 6000);
            vm.ShowColumnOriginalText = true;
            ImportReference(vm, BuildSampleReference());
            Dispatcher.UIThread.RunJobs();

            var anchor = TableViewScrollAnchor.GetFor(vm.SubtitleGrid);
            Assert.NotNull(anchor);

            // Watch the suspend count at the exact moment the rows move.
            var sawChurn = false;
            var suspendedThroughout = true;
            void OnChanged(object? _, NotifyCollectionChangedEventArgs __)
            {
                sawChurn = true;
                if (SuspendCountOf(anchor!) == 0)
                {
                    suspendedThroughout = false;
                }
            }

            // Drop a row the reference still has a line for, so the rebuild has to bring it back
            // as a display-only row - that re-insert is the churn under test. Done before the
            // handler is attached: this removal is the test's own setup, not part of the rebuild.
            vm.Subtitles.RemoveAt(1);

            vm.Subtitles.CollectionChanged += OnChanged;
            try
            {
                InvokeReapplyOriginalReference(vm);
            }
            finally
            {
                vm.Subtitles.CollectionChanged -= OnChanged;
            }

            Assert.True(sawChurn, "the rebuild did not touch the row collection, so nothing was measured");
            Assert.True(suspendedThroughout, "the scroll anchor was live while the reference rebuild moved rows");
        }
        finally
        {
            window.Close();
        }
    }

    // The suspension must not leak: once the rebuild is done the anchor has to be doing its job
    // again, or ordinary editing stops holding the view steady (#13619).
    [AvaloniaFact]
    public void ReferenceRebuild_ReleasesTheAnchorAfterwards()
    {
        var (window, vm) = CreateMainViewModel();
        try
        {
            AddLine(vm, "Translated one", "Reference one", 0, 2000);
            AddLine(vm, "Translated two", "Reference two", 4000, 6000);
            vm.ShowColumnOriginalText = true;
            ImportReference(vm, BuildSampleReference());
            Dispatcher.UIThread.RunJobs();

            var anchor = TableViewScrollAnchor.GetFor(vm.SubtitleGrid);
            Assert.NotNull(anchor);

            vm.Subtitles.RemoveAt(1);
            InvokeReapplyOriginalReference(vm);
            InvokeReapplyOriginalReference(vm); // twice: a leak would stack

            Assert.Equal(0, SuspendCountOf(anchor!));
        }
        finally
        {
            window.Close();
        }
    }

    private static int SuspendCountOf(TableViewScrollAnchor anchor) =>
        (int)typeof(TableViewScrollAnchor)
            .GetField("_suspendCount", BindingFlags.NonPublic | BindingFlags.Instance)!
            .GetValue(anchor)!;

    private static void InvokeReapplyOriginalReference(MainViewModel vm) =>
        typeof(MainViewModel)
            .GetMethod("ReapplyOriginalReference", BindingFlags.Instance | BindingFlags.NonPublic)!
            .Invoke(vm, new object?[] { true });

    private static void ImportReference(MainViewModel vm, Subtitle reference)
    {
        var match = ImportOriginalHelper.MatchOriginalLines(vm.Subtitles, reference);
        typeof(MainViewModel)
            .GetMethod("ImportOriginalSubtitle", BindingFlags.Instance | BindingFlags.NonPublic)!
            .Invoke(vm, new object?[] { 0, "reference.srt", reference, match, true });
    }

    private static Subtitle BuildSampleReference()
    {
        var reference = new Subtitle();
        reference.Paragraphs.Add(new Paragraph("Reference one", 0, 2000));
        reference.Paragraphs.Add(new Paragraph("Reference only - no translation", 2000, 4000));
        reference.Paragraphs.Add(new Paragraph("Reference two", 4000, 6000));
        return reference;
    }

    private static SubtitleLineViewModel AddLine(MainViewModel vm, string text, string originalText, int startMs, int endMs)
    {
        var line = new SubtitleLineViewModel(new Paragraph(text, startMs, endMs), null!)
        {
            OriginalText = originalText,
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

        return (window, (MainViewModel)view.DataContext!);
    }
}
