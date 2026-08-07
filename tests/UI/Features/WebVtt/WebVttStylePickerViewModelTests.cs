using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using Microsoft.Extensions.DependencyInjection;
using Nikse.SubtitleEdit;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.WebVtt;

namespace UITests.Features.WebVtt;

public class WebVttStylePickerViewModelTests : IDisposable
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

    private WebVttStylePickerViewModel ShowWindow(List<WebVttStyleDisplay> styles)
    {
        var services = new ServiceCollection();
        services.AddSubtitleEditServices();
        var vm = services.BuildServiceProvider().GetRequiredService<WebVttStylePickerViewModel>();

        var window = new WebVttStylePickerWindow(vm);
        _windows.Add(window);
        window.Show();
        vm.Initialize("Pick styles", "OK", styles);
        Dispatcher.UIThread.RunJobs();
        return vm;
    }

    private static List<WebVttStyleDisplay> MakeStyles() => new()
    {
        new WebVttStyleDisplay(new WebVttStyle { Name = ".red" }),
        new WebVttStyleDisplay(new WebVttStyle { Name = ".big", FontSize = 30 }),
    };

    [AvaloniaFact]
    public void OnlyCheckedStylesAreReturned()
    {
        var vm = ShowWindow(MakeStyles());

        vm.Styles[1].IsSelected = true;
        vm.OkCommand.Execute(null);
        Dispatcher.UIThread.RunJobs();

        Assert.True(vm.OkPressed);
        Assert.Equal(new[] { "big" }, vm.CheckedStyles.Select(p => p.Name).ToArray());
    }

    [AvaloniaFact]
    public void SelectAllChecksEverything()
    {
        var vm = ShowWindow(MakeStyles());

        vm.SelectAllCommand.Execute(null);
        Dispatcher.UIThread.RunJobs();

        Assert.Equal(2, vm.CheckedStyles.Count);
    }

    [AvaloniaFact]
    public void InvertSelectionFlipsEveryCheck()
    {
        var styles = MakeStyles();
        styles[0].IsSelected = true;
        var vm = ShowWindow(styles);

        vm.InvertSelectionCommand.Execute(null);
        Dispatcher.UIThread.RunJobs();

        Assert.Equal(new[] { "big" }, vm.CheckedStyles.Select(p => p.Name).ToArray());
    }

    [AvaloniaFact]
    public void SelectingAStyleShowsItsCss()
    {
        var vm = ShowWindow(MakeStyles());

        vm.SelectedStyle = vm.Styles[1];
        Dispatcher.UIThread.RunJobs();

        Assert.Contains("font-size:30px", vm.SelectedStyleCss);
    }

    [AvaloniaFact]
    public void CheckedStylesApplyAsCueClassesOnTheLine()
    {
        // What the main window does with the picker result.
        var vm = ShowWindow(MakeStyles());
        vm.SelectAllCommand.Execute(null);
        vm.OkCommand.Execute(null);
        Dispatcher.UIThread.RunJobs();

        var paragraph = new Paragraph("Hello", 0, 1000);
        var text = WebVttHelper.SetParagraphStyles(paragraph, vm.CheckedStyles.Select(p => p.ToWebVttStyle()).ToList());

        Assert.Equal("<c.red.big>Hello</c>", text);
    }

    [AvaloniaFact]
    public void ClearingEveryCheckRemovesTheCueClasses()
    {
        var vm = ShowWindow(MakeStyles());
        vm.OkCommand.Execute(null);
        Dispatcher.UIThread.RunJobs();

        var paragraph = new Paragraph("<c.red.big>Hello</c>", 0, 1000);
        var text = WebVttHelper.SetParagraphStyles(paragraph, vm.CheckedStyles.Select(p => p.ToWebVttStyle()).ToList());

        Assert.Equal("Hello", text);
    }
}
