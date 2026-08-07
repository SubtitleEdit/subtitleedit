using System;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Media;
using Avalonia.Threading;
using Microsoft.Extensions.DependencyInjection;
using Nikse.SubtitleEdit;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.WebVtt;

namespace UITests.Features.WebVtt;

/// <summary>
/// Drives the real WebVTT style manager window headless, built exactly like in production, so
/// the bindings and the header rewrite are exercised and not just the view model in isolation.
/// </summary>
public class WebVttStylesViewModelTests : IDisposable
{
    private const string Header =
        "WEBVTT\r\n" +
        "\r\n" +
        "NOTE this comment must survive\r\n" +
        "\r\n" +
        "STYLE\r\n" +
        "::cue(.red) { color:rgb(255,0,0) }\r\n" +
        "::cue(.big) { font-size:30px; font-weight:bold }\r\n";

    // Every window opened by a test is closed again in Dispose: an unclosed window would
    // outlive the test and race with the headless session teardown.
    private readonly List<Window> _windows = new();

    public void Dispose()
    {
        foreach (var window in _windows)
        {
            window.Close();
        }

        _windows.Clear();
    }

    private static WebVttStylesViewModel MakeVm()
    {
        var services = new ServiceCollection();
        services.AddSubtitleEditServices();
        var provider = services.BuildServiceProvider();
        return provider.GetRequiredService<WebVttStylesViewModel>();
    }

    private WebVttStylesViewModel ShowWindow(Subtitle subtitle)
    {
        var vm = MakeVm();
        var window = new WebVttStylesWindow(vm);
        _windows.Add(window);
        window.Show();
        vm.Initialize(subtitle, "test.vtt", null, null);
        Dispatcher.UIThread.RunJobs();
        return vm;
    }

    private static Subtitle MakeSubtitle()
    {
        var subtitle = new Subtitle { Header = Header };
        subtitle.Paragraphs.Add(new Paragraph("<c.red>Red line</c>", 0, 1000));
        subtitle.Paragraphs.Add(new Paragraph("<c.red.big>Both</c>", 1000, 2000));
        subtitle.Paragraphs.Add(new Paragraph("Plain", 2000, 3000));
        return subtitle;
    }

    [AvaloniaFact]
    public void LoadsStylesFromHeader()
    {
        var vm = ShowWindow(MakeSubtitle());

        Assert.Equal(new[] { "red", "big" }, vm.Styles.Select(p => p.Name).ToArray());
        Assert.True(vm.Styles[0].UseColor);
        Assert.Equal(Colors.Red, vm.Styles[0].Color);
        Assert.Equal(30, vm.Styles[1].FontSize);
        Assert.True(vm.Styles[1].Bold);
    }

    [AvaloniaFact]
    public void CountsUsagesPerStyle()
    {
        var vm = ShowWindow(MakeSubtitle());

        Assert.Equal(2, vm.Styles.Single(p => p.Name == "red").UsageCount);
        Assert.Equal(1, vm.Styles.Single(p => p.Name == "big").UsageCount);
    }

    [AvaloniaFact]
    public void OkRewritesTheStyleBlockAndKeepsTheRestOfTheHeader()
    {
        var vm = ShowWindow(MakeSubtitle());

        vm.Styles[0].Italic = true;
        vm.OkCommand.Execute(null);
        Dispatcher.UIThread.RunJobs();

        Assert.True(vm.OkPressed);
        Assert.Contains("NOTE this comment must survive", vm.Header);
        Assert.Contains("font-style:italic", vm.Header);

        // Exactly one STYLE block - the old one is replaced, not appended to.
        Assert.Equal(1, vm.Header.Split('\n').Count(line => line.Trim() == "STYLE"));

        var readBack = WebVttHelper.GetStyles(vm.Header);
        Assert.Equal(new[] { ".red", ".big" }, readBack.Select(p => p.Name).ToArray());
        Assert.True(readBack[0].Italic);
    }

    [AvaloniaFact]
    public void RemovingEveryStyleLeavesNoStyleBlock()
    {
        var vm = ShowWindow(MakeSubtitle());

        vm.RemoveAllCommand.Execute(null);
        vm.OkCommand.Execute(null);
        Dispatcher.UIThread.RunJobs();

        Assert.Empty(WebVttHelper.GetStyles(vm.Header));
        Assert.DoesNotContain("::cue(", vm.Header);
        Assert.StartsWith("WEBVTT", vm.Header);
    }

    [AvaloniaFact]
    public void NewStyleGetsAUniqueName()
    {
        var vm = ShowWindow(MakeSubtitle());

        vm.NewCommand.Execute(null);
        vm.NewCommand.Execute(null);
        Dispatcher.UIThread.RunJobs();

        Assert.Equal(4, vm.Styles.Count);
        Assert.Equal(vm.Styles.Count, vm.Styles.Select(p => p.Name).Distinct().Count());
    }

    [AvaloniaFact]
    public void DuplicateKeepsThePropertiesButNotTheName()
    {
        var vm = ShowWindow(MakeSubtitle());

        vm.SelectedStyle = vm.Styles[0];
        vm.DuplicateCommand.Execute(null);
        Dispatcher.UIThread.RunJobs();

        var copy = vm.Styles.Last();
        Assert.NotEqual("red", copy.Name);
        Assert.True(copy.UseColor);
        Assert.Equal(Colors.Red, copy.Color);
    }

    [AvaloniaFact]
    public void DuplicateNamesAreFlagged()
    {
        var vm = ShowWindow(MakeSubtitle());

        Assert.False(vm.HasDuplicateStyleNames);

        vm.Styles[1].Name = "red";
        Dispatcher.UIThread.RunJobs();

        Assert.True(vm.HasDuplicateStyleNames);
        Assert.Contains("red", vm.DuplicateStyleNames);
    }

    [AvaloniaFact]
    public void EmptyNameIsFlagged()
    {
        var vm = ShowWindow(MakeSubtitle());

        vm.SelectedStyle = vm.Styles[0];
        Dispatcher.UIThread.RunJobs();
        Assert.False(vm.IsNameInvalid);

        vm.Styles[0].Name = string.Empty;
        Dispatcher.UIThread.RunJobs();

        Assert.True(vm.IsNameInvalid);
    }

    [AvaloniaFact]
    public void MoveDownReordersTheHeader()
    {
        var vm = ShowWindow(MakeSubtitle());

        vm.StyleGrid.SelectedItem = vm.Styles[0];
        Dispatcher.UIThread.RunJobs();
        vm.MoveDownCommand.Execute(null);
        vm.OkCommand.Execute(null);
        Dispatcher.UIThread.RunJobs();

        Assert.Equal(new[] { ".big", ".red" }, WebVttHelper.GetStyles(vm.Header).Select(p => p.Name).ToArray());
    }

    [AvaloniaFact]
    public void CancelLeavesTheHeaderAlone()
    {
        var subtitle = MakeSubtitle();
        var vm = ShowWindow(subtitle);

        vm.Styles[0].Italic = true;
        vm.CancelCommand.Execute(null);
        Dispatcher.UIThread.RunJobs();

        Assert.False(vm.OkPressed);
        Assert.Equal(Header, subtitle.Header);
    }

    [AvaloniaFact]
    public void HandlesASubtitleWithoutAnyStyleBlock()
    {
        var subtitle = new Subtitle { Header = "WEBVTT" };
        subtitle.Paragraphs.Add(new Paragraph("Hello", 0, 1000));

        var vm = ShowWindow(subtitle);
        Assert.Empty(vm.Styles);

        vm.NewCommand.Execute(null);
        vm.OkCommand.Execute(null);
        Dispatcher.UIThread.RunJobs();

        Assert.Single(WebVttHelper.GetStyles(vm.Header));
    }
}
