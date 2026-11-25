using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using Microsoft.Extensions.DependencyInjection;
using Nikse.SubtitleEdit;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Features.Main.MainHelpers;
using Nikse.SubtitleEdit.Logic;
using System.Reflection;

namespace UITests.Features.Main;

/// <summary>
/// Point sync and Point sync via other subtitle with an original open: the display-only reference
/// rows (original lines no translated line matches) are not lines of the subtitle being synced.
/// They showed up in the dialog as "0" rows with no text, and their times gave the "Gap after"
/// sync hints false neighbours (issue #17183).
/// </summary>
public class PointSyncReferenceRowsTests
{
    [AvaloniaFact]
    public void DialogRows_LeaveOutReferenceRows_AndApplyPutsThemBack()
    {
        var (window, vm) = CreateMainViewModel();
        try
        {
            AddLine(vm, "One", 0, 2000);
            AddLine(vm, "Two", 10000, 12000);
            AddLine(vm, "Three", 20000, 22000);
            AddLine(vm, "Four", 30000, 32000);

            var reference = new Subtitle();
            reference.Paragraphs.Add(new Paragraph("One", 0, 2000));
            reference.Paragraphs.Add(new Paragraph("Two", 10000, 12000));
            reference.Paragraphs.Add(new Paragraph("Reference only", 15000, 17000));
            reference.Paragraphs.Add(new Paragraph("Three", 20000, 22000));
            reference.Paragraphs.Add(new Paragraph("Four", 30000, 32000));

            vm.ShowColumnOriginalText = true;
            ImportReference(vm, reference);
            Dispatcher.UIThread.RunJobs();

            var referenceRow = Assert.Single(vm.Subtitles, r => r.IsReferenceOnly);
            Assert.Equal(2, vm.Subtitles.IndexOf(referenceRow));
            var workingRows = vm.Subtitles.Where(r => !r.IsReferenceOnly).ToList();

            // Selecting the reference row starts the dialog on the working row before it.
            vm.SelectedSubtitle = referenceRow;
            var before = InvokeCaptureGridPosition(vm);
            var (copies, selectedIndex) = InvokeCopyWorkingRowsForDialog(vm, referenceRow);

            Assert.Equal(4, copies.Count);
            Assert.DoesNotContain(copies, r => r.IsReferenceOnly);
            Assert.Equal(new[] { "One", "Two", "Three", "Four" }, copies.Select(r => r.Text));
            Assert.Equal(1, selectedIndex);

            // What a sync does: every line 7 seconds later - "Two" now starts after the reference row.
            foreach (var copy in copies)
            {
                copy.SetStartTimeKeepDuration(copy.StartTime + TimeSpan.FromSeconds(7));
            }

            InvokeApplyDialogRowsKeepingReferenceRows(vm, copies, before);
            Dispatcher.UIThread.RunJobs();

            Assert.Equal(5, vm.Subtitles.Count);
            Assert.Same(referenceRow, Assert.Single(vm.Subtitles, r => r.IsReferenceOnly));
            Assert.Equal(TimeSpan.FromSeconds(15), referenceRow.StartTime);
            Assert.Equal(workingRows, vm.Subtitles.Where(r => !r.IsReferenceOnly));
            Assert.Equal(
                new[] { 7.0, 15.0, 17.0, 27.0, 37.0 },
                vm.Subtitles.Select(r => r.StartTime.TotalSeconds));
        }
        finally
        {
            window.Close();
        }
    }

    private static object InvokeCaptureGridPosition(MainViewModel vm) =>
        typeof(MainViewModel)
            .GetMethod("CaptureGridPosition", BindingFlags.Instance | BindingFlags.NonPublic)!
            .Invoke(vm, null)!;

    private static (List<SubtitleLineViewModel> Rows, int SelectedIndex) InvokeCopyWorkingRowsForDialog(MainViewModel vm, SubtitleLineViewModel? selectedRow)
    {
        var args = new object?[] { selectedRow, 0 };
        var rows = (List<SubtitleLineViewModel>)typeof(MainViewModel)
            .GetMethod("CopyWorkingRowsForDialog", BindingFlags.Instance | BindingFlags.NonPublic)!
            .Invoke(vm, args)!;
        return (rows, (int)args[1]!);
    }

    private static void InvokeApplyDialogRowsKeepingReferenceRows(MainViewModel vm, List<SubtitleLineViewModel> lines, object before) =>
        typeof(MainViewModel)
            .GetMethod("ApplyDialogRowsKeepingReferenceRows", BindingFlags.Instance | BindingFlags.NonPublic)!
            .Invoke(vm, new[] { lines, before });

    private static void ImportReference(MainViewModel vm, Subtitle reference)
    {
        var match = ImportOriginalHelper.MatchOriginalLines(vm.Subtitles, reference);
        typeof(MainViewModel)
            .GetMethod("ImportOriginalSubtitle", BindingFlags.Instance | BindingFlags.NonPublic)!
            .Invoke(vm, new object?[] { 0, "reference.srt", reference, match, true });
    }

    private static void AddLine(MainViewModel vm, string text, int startMs, int endMs)
    {
        vm.Subtitles.Add(new SubtitleLineViewModel(new Paragraph(text, startMs, endMs), null!)
        {
            Number = vm.Subtitles.Count + 1,
        });
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
