using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using Microsoft.Extensions.DependencyInjection;
using Nikse.SubtitleEdit;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Logic.Config;
using System.Reflection;

namespace UITests.Features.Main;

/// <summary>
/// A bulk rebuild detaches the grid's ItemsSource. The TableView kept its SelectedIndex across the
/// detach and re-applied it on reattach, after which assigning SelectedItem no longer moved the
/// grid's own SelectedItem - and the grid double-click reads that, so after a sort every
/// double-click seeked the video to the same line (#15579). A second sort happened to fix it.
/// </summary>
public class SubtitleGridSelectionAfterRebuildTests : IDisposable
{
    private readonly List<Window> _windows = new();

    public void Dispose()
    {
        foreach (var window in _windows)
        {
            window.Close();
        }

        _windows.Clear();
    }

    [AvaloniaFact]
    public async Task SortByStyle_GridSelectedItemFollowsNewSelection()
    {
        var (window, vm) = ShowMainWindowWithLines(40);
        vm.SelectAndScrollToSubtitle(vm.Subtitles[3]);
        await SettleAsync(window);

        vm.SortByStyleCommand.Execute(null);
        await SettleAsync(window);

        Assert.Same(vm.SelectedSubtitle, vm.SubtitleGrid.SelectedItem);
        AssertSelectingRowsMovesGridSelectedItem(vm, window);
    }

    [AvaloniaFact]
    public async Task ReplaceWithCopies_GridSelectedItemFollowsNewSelection()
    {
        var (window, vm) = ShowMainWindowWithLines(40);
        vm.SelectAndScrollToSubtitle(vm.Subtitles[3]);
        await SettleAsync(window);

        // What the Sort subtitles dialog hands back: sorted copies of the rows.
        var copies = vm.Subtitles.OrderBy(p => p.Style).Select(p => new SubtitleLineViewModel(p)).ToList();
        typeof(MainViewModel).GetMethod("ReplaceSubtitles", BindingFlags.Instance | BindingFlags.NonPublic)!
            .Invoke(vm, new object[] { copies });
        await SettleAsync(window);

        AssertSelectingRowsMovesGridSelectedItem(vm, window);
    }

    private static void AssertSelectingRowsMovesGridSelectedItem(MainViewModel vm, Window window)
    {
        foreach (var index in new[] { 5, 8, 11 })
        {
            vm.SubtitleGrid.SelectedItem = vm.Subtitles[index];
            Settle(window);

            Assert.Same(vm.Subtitles[index], vm.SubtitleGrid.SelectedItem);
            Assert.Equal(index, vm.SubtitleGrid.SelectedIndex);
            Assert.Same(vm.Subtitles[index], vm.SelectedSubtitle);
        }
    }

    private (Window Window, MainViewModel Vm) ShowMainWindowWithLines(int lineCount)
    {
        var services = new ServiceCollection();
        services.AddSubtitleEditServices();
        Locator.Services = services.BuildServiceProvider();

        var window = new Window { Width = 1400, Height = 900 };
        _windows.Add(window);
        MainView.NextHostWindow = window;
        var view = new MainView();
        window.Content = view;
        window.Show();
        Dispatcher.UIThread.RunJobs();
        window.UpdateLayout();

        var vm = (MainViewModel)view.DataContext!;
        window.SuppressSaveChangesPromptOnClose(vm);
        for (var i = 0; i < lineCount; i++)
        {
            // Alternating styles, so sorting by style moves every row.
            vm.Subtitles.Add(new SubtitleLineViewModel(new Paragraph($"Line {i + 1}", i * 2000, i * 2000 + 1500), null!)
            {
                Number = i + 1,
                Style = i % 2 == 0 ? "B" : "A",
            });
        }

        Settle(window);
        return (window, vm);
    }

    private static async Task SettleAsync(Window window)
    {
        Settle(window);
        await Task.Delay(50);
        Settle(window);
    }

    private static void Settle(Window window)
    {
        for (var pump = 0; pump < 8; pump++)
        {
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();
        }
    }
}
