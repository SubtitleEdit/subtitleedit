using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using Microsoft.Extensions.DependencyInjection;
using Nikse.SubtitleEdit;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Assa;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace UITests.Features.Assa;

/// <summary>
/// SE 4's attachments window could reorder the list with move up/down/to top/to bottom. The
/// order is not presentation-only: it is the order the attachments are written back into the
/// subtitle's [Fonts]/[Graphics] sections on OK, which is why these grids have no header sort.
/// </summary>
public class AssaAttachmentsMoveTests : IDisposable
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

    private (AssaAttachmentsWindow Window, AssaAttachmentsViewModel Vm) MakeWindowWithThreeFonts()
    {
        var services = new ServiceCollection();
        services.AddSubtitleEditServices();
        Locator.Services = services.BuildServiceProvider();

        var vm = Locator.Services.GetRequiredService<AssaAttachmentsViewModel>();
        foreach (var name in new[] { "a.ttf", "b.ttf", "c.ttf" })
        {
            vm.Attachments.Add(new AssaAttachmentItem
            {
                FileName = name,
                Category = Se.Language.General.Fonts,
                Content = "M" + name,
                Bytes = [1, 2, 3],
                Size = "3 bytes",
            });
        }

        var window = new AssaAttachmentsWindow(vm);
        _windows.Add(window);
        window.Show();
        Dispatcher.UIThread.RunJobs();
        window.UpdateLayout();

        return (window, vm);
    }

    private static void Select(AssaAttachmentsViewModel vm, AssaAttachmentItem item)
    {
        vm.AttachmentGrid.SelectedItem = item;
        Dispatcher.UIThread.RunJobs();
    }

    [AvaloniaFact]
    public void MoveDownReordersTheAttachmentsAndKeepsTheSelection()
    {
        var (_, vm) = MakeWindowWithThreeFonts();
        var first = vm.Attachments[0];

        Select(vm, first);
        vm.MoveDownCommand.Execute(null);
        Dispatcher.UIThread.RunJobs();

        Assert.Equal(new[] { "b.ttf", "a.ttf", "c.ttf" }, vm.Attachments.Select(a => a.FileName));
        Assert.Same(first, vm.AttachmentGrid.SelectedItem);
    }

    [AvaloniaFact]
    public void MoveToBottomWritesTheNewOrderToTheFooter()
    {
        var (_, vm) = MakeWindowWithThreeFonts();

        Select(vm, vm.Attachments[0]);
        vm.MoveToBottomCommand.Execute(null);
        Dispatcher.UIThread.RunJobs();

        Assert.Equal(new[] { "b.ttf", "c.ttf", "a.ttf" }, vm.Attachments.Select(a => a.FileName));

        vm.OkCommand.Execute(null);

        var fontNames = vm.Footer.SplitToLines()!
            .Where(line => line.StartsWith("fontname: ", StringComparison.Ordinal))
            .Select(line => line.Substring("fontname: ".Length))
            .ToArray();
        Assert.Equal(new[] { "b.ttf", "c.ttf", "a.ttf" }, fontNames);
    }

    [AvaloniaFact]
    public void MoveToTopMovesTheSelectedAttachmentFirst()
    {
        var (_, vm) = MakeWindowWithThreeFonts();

        Select(vm, vm.Attachments[2]);
        vm.MoveToTopCommand.Execute(null);
        Dispatcher.UIThread.RunJobs();

        Assert.Equal(new[] { "c.ttf", "a.ttf", "b.ttf" }, vm.Attachments.Select(a => a.FileName));
    }
}
