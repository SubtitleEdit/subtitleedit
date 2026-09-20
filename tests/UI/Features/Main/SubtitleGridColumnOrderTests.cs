using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using Microsoft.Extensions.DependencyInjection;
using Nikse.SubtitleEdit;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Features.Main.Layout;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace UITests.Features.Main;

/// <summary>
/// The "Columns..." dialog (#14369) saves the grid column order as SubtitleGridColumnOrder;
/// the layout builder must re-apply it on every grid rebuild, and an order saved by an older
/// version must not displace columns it does not know about.
/// </summary>
public class SubtitleGridColumnOrderTests : IDisposable
{
    private readonly List<Window> _windows = new();
    private readonly List<string> _savedOrder = Se.Settings.General.SubtitleGridColumnOrder;

    public void Dispose()
    {
        Se.Settings.General.SubtitleGridColumnOrder = _savedOrder;

        foreach (var window in _windows)
        {
            window.Close();
        }

        _windows.Clear();
    }

    private (Window Window, MainViewModel Vm) ShowMainWindow()
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

        var vm = (MainViewModel)view.DataContext!;
        window.SuppressSaveChangesPromptOnClose(vm);
        vm.Subtitles.Add(new SubtitleLineViewModel(new Paragraph("Hello", 0, 2000), null!));

        for (var pump = 0; pump < 5; pump++)
        {
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();
        }

        return (window, vm);
    }

    private static string?[] VisibleColumnKeys(MainViewModel vm) =>
        vm.SubtitleGrid.Columns.OfType<SeTableViewColumn>().Select(p => p.Tag as string).ToArray();

    [AvaloniaFact]
    public void SavedOrder_IsAppliedWhenTheGridIsBuilt()
    {
        Se.Settings.General.SubtitleGridColumnOrder = new List<string>
        {
            InitListViewAndEditBox.SubtitleGridColumnKeys.Number,
            InitListViewAndEditBox.SubtitleGridColumnKeys.Duration,
            InitListViewAndEditBox.SubtitleGridColumnKeys.Start,
            InitListViewAndEditBox.SubtitleGridColumnKeys.End,
            InitListViewAndEditBox.SubtitleGridColumnKeys.Text,
        };

        var (window, vm) = ShowMainWindow();

        var keys = VisibleColumnKeys(vm);
        var duration = Array.IndexOf(keys, InitListViewAndEditBox.SubtitleGridColumnKeys.Duration);
        var start = Array.IndexOf(keys, InitListViewAndEditBox.SubtitleGridColumnKeys.Start);
        var end = Array.IndexOf(keys, InitListViewAndEditBox.SubtitleGridColumnKeys.End);
        Assert.True(duration >= 0 && duration < start, $"Duration should precede Start: {string.Join(",", keys)}");
        Assert.True(start < end, $"Start should precede End: {string.Join(",", keys)}");

        window.Close();
    }

    [AvaloniaFact]
    public void SavedOrderFromAnOlderVersion_KeepsUnknownColumnsAtTheirDefaultPosition()
    {
        // An "old" order knowing only three columns must not push Number/Text/etc. around.
        Se.Settings.General.SubtitleGridColumnOrder = new List<string>
        {
            InitListViewAndEditBox.SubtitleGridColumnKeys.End,
            InitListViewAndEditBox.SubtitleGridColumnKeys.Start,
            "NotAColumnAnymore",
        };

        var (window, vm) = ShowMainWindow();

        var keys = VisibleColumnKeys(vm);
        var number = Array.IndexOf(keys, InitListViewAndEditBox.SubtitleGridColumnKeys.Number);
        var end = Array.IndexOf(keys, InitListViewAndEditBox.SubtitleGridColumnKeys.End);
        var start = Array.IndexOf(keys, InitListViewAndEditBox.SubtitleGridColumnKeys.Start);
        var text = Array.IndexOf(keys, InitListViewAndEditBox.SubtitleGridColumnKeys.Text);
        Assert.True(end < start, $"Saved End-before-Start order lost: {string.Join(",", keys)}");
        Assert.True(number >= 0, "Number column disappeared");
        Assert.True(text > start, $"Text should keep its default position after the times: {string.Join(",", keys)}");

        window.Close();
    }
}
