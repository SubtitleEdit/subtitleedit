using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using Microsoft.Extensions.DependencyInjection;
using Nikse.SubtitleEdit;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Features.Main.Layout;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace UITests.Features.Main;

/// <summary>
/// WebVTT has styles (cue classes) and voices, but keeps both inside the cue text instead of in
/// the style/actor fields the ASSA columns read - so the grid showed an empty Style column (and
/// none at all, since the column was ASSA-only) plus an "Actor" column that could never fill.
/// These tests pin down that a WebVTT file gets its own Style and Voice columns instead.
/// </summary>
public class SubtitleGridWebVttColumnTests : IDisposable
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
        vm.ShowColumnStyle = true;
        vm.ShowColumnActor = true;
        vm.Subtitles.Add(new SubtitleLineViewModel(
            new Paragraph("<v Joe><c.loud>Hello</c>", 0, 2000) { Actor = "Ann", Style = "Default" }, null!));

        Settle(window);
        return (window, vm);
    }

    private static void Settle(Window window)
    {
        for (var pump = 0; pump < 5; pump++)
        {
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();
        }
    }

    private static void SetFormat(Window window, MainViewModel vm, string formatName)
    {
        vm.SelectedSubtitleFormat = vm.SubtitleFormats.First(p => p.Name == formatName);
        Settle(window);
    }

    private static string?[] VisibleColumnKeys(MainViewModel vm) =>
        vm.SubtitleGrid.Columns.OfType<SeTableViewColumn>().Select(p => p.Tag as string).ToArray();

    [AvaloniaFact]
    public void WebVtt_ShowsTheWebVttStyleAndVoiceColumns()
    {
        var (window, vm) = ShowMainWindow();

        SetFormat(window, vm, new WebVTT().Name);

        var keys = VisibleColumnKeys(vm);
        Assert.Contains(InitListViewAndEditBox.SubtitleGridColumnKeys.WebVttStyle, keys);
        Assert.Contains(InitListViewAndEditBox.SubtitleGridColumnKeys.WebVttVoice, keys);
        Assert.DoesNotContain(InitListViewAndEditBox.SubtitleGridColumnKeys.Style, keys);
        Assert.DoesNotContain(InitListViewAndEditBox.SubtitleGridColumnKeys.Actor, keys);

        // ... and they show what the cue text holds, not the (unused) style/actor fields.
        Assert.Equal("loud", vm.Subtitles[0].WebVttStyle);
        Assert.Equal("Joe", vm.Subtitles[0].WebVttVoice);

        window.Close();
    }

    [AvaloniaFact]
    public void Assa_KeepsTheStyleAndActorColumns()
    {
        var (window, vm) = ShowMainWindow();

        SetFormat(window, vm, new AdvancedSubStationAlpha().Name);

        var keys = VisibleColumnKeys(vm);
        Assert.Contains(InitListViewAndEditBox.SubtitleGridColumnKeys.Style, keys);
        Assert.Contains(InitListViewAndEditBox.SubtitleGridColumnKeys.Actor, keys);
        Assert.DoesNotContain(InitListViewAndEditBox.SubtitleGridColumnKeys.WebVttStyle, keys);
        Assert.DoesNotContain(InitListViewAndEditBox.SubtitleGridColumnKeys.WebVttVoice, keys);

        window.Close();
    }

    /// <summary>
    /// SubRip and friends have no styles at all - the Style column stays away, the Actor column
    /// stays (it is what "Convert actors" and the ASSA import fill in).
    /// </summary>
    [AvaloniaFact]
    public void SubRip_HasNoStyleColumnButKeepsActor()
    {
        var (window, vm) = ShowMainWindow();

        SetFormat(window, vm, new SubRip().Name);

        var keys = VisibleColumnKeys(vm);
        Assert.DoesNotContain(InitListViewAndEditBox.SubtitleGridColumnKeys.Style, keys);
        Assert.DoesNotContain(InitListViewAndEditBox.SubtitleGridColumnKeys.WebVttStyle, keys);
        Assert.DoesNotContain(InitListViewAndEditBox.SubtitleGridColumnKeys.WebVttVoice, keys);
        Assert.Contains(InitListViewAndEditBox.SubtitleGridColumnKeys.Actor, keys);

        window.Close();
    }

    /// <summary>The one show/hide toggle serves both columns, so it has to be named for the one it shows.</summary>
    [AvaloniaFact]
    public void ShowActorColumnMenuHeader_SaysVoiceForWebVtt()
    {
        var (window, vm) = ShowMainWindow();

        SetFormat(window, vm, new WebVTT().Name);
        Assert.Equal(Se.Language.File.WebVtt.ShowVoiceColumn, vm.ShowActorColumnMenuHeader);

        SetFormat(window, vm, new SubRip().Name);
        Assert.Equal(Se.Language.General.ShowActorColumn, vm.ShowActorColumnMenuHeader);

        window.Close();
    }
}
