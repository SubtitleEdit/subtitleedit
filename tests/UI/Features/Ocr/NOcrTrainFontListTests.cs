using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Microsoft.Extensions.DependencyInjection;
using Nikse.SubtitleEdit;
using Nikse.SubtitleEdit.Features.Ocr.NOcr;

namespace UITests.Features.Ocr;

/// <summary>
/// The Train nOCR font list is a virtualized ListBox whose item template originally read the
/// FuncDataTemplate build parameter instead of binding. Scrolling recycled containers with a
/// null item, throwing a NullReferenceException mid-layout; the global dispatcher handler
/// swallowed it, leaving the panel visually corrupted (duplicate/missing rows, broken scroll).
/// These tests scroll the list through recycling and assert every realized row still shows
/// its own item's name, typeface, and checked state.
/// </summary>
public class NOcrTrainFontListTests
{
    private static NOcrTrainViewModel MakeViewModel(int fontCount)
    {
        var services = new ServiceCollection();
        services.AddSubtitleEditServices();
        var provider = services.BuildServiceProvider();
        var vm = provider.GetRequiredService<NOcrTrainViewModel>();

        vm.Fonts.Clear();
        for (var i = 0; i < fontCount; i++)
        {
            vm.Fonts.Add(new NOcrTrainFontItem($"Font {i:000}", i % 3 == 0));
        }

        return vm;
    }

    private static void PumpLayout(Window window)
    {
        Dispatcher.UIThread.RunJobs();
        window.UpdateLayout();
    }

    private static void AssertRealizedRowsMatchTheirItems(ListBox listBox)
    {
        var containers = listBox.GetRealizedContainers().OfType<ListBoxItem>().ToList();
        Assert.NotEmpty(containers);
        foreach (var container in containers)
        {
            var item = Assert.IsType<NOcrTrainFontItem>(container.DataContext);
            var checkBox = container.GetVisualDescendants().OfType<CheckBox>().First();
            Assert.Equal(item.Name, checkBox.Content);
            Assert.Equal(item.Name, checkBox.FontFamily.Name);
            Assert.Equal(item.IsSelected, checkBox.IsChecked);
        }
    }

    [AvaloniaFact]
    public void ScrollFontList_RecyclesContainers_WithoutCrashOrStaleRows()
    {
        var vm = MakeViewModel(200);
        var window = new NOcrTrainWindow(vm);
        window.Show();
        PumpLayout(window);

        var listBox = window.GetVisualDescendants().OfType<ListBox>().First();
        var scrollViewer = listBox.GetVisualDescendants().OfType<ScrollViewer>().First();

        AssertRealizedRowsMatchTheirItems(listBox);

        // page down through the list to force container recycling
        for (var i = 0; i < 5; i++)
        {
            scrollViewer.Offset = new Vector(0, scrollViewer.Offset.Y + scrollViewer.Viewport.Height);
            PumpLayout(window);
            AssertRealizedRowsMatchTheirItems(listBox);
        }

        // jump to the end, then back to the top
        scrollViewer.Offset = new Vector(0, scrollViewer.Extent.Height);
        PumpLayout(window);
        AssertRealizedRowsMatchTheirItems(listBox);

        Assert.True(scrollViewer.Offset.Y > 0, "scrolling to the end should move the offset");

        scrollViewer.Offset = new Vector(0, 0);
        PumpLayout(window);
        AssertRealizedRowsMatchTheirItems(listBox);

        window.Close();
    }

    [AvaloniaFact]
    public void ToggleCheckBox_AfterScrolling_TogglesTheRowsOwnItem()
    {
        var vm = MakeViewModel(200);
        var window = new NOcrTrainWindow(vm);
        window.Show();
        PumpLayout(window);

        var listBox = window.GetVisualDescendants().OfType<ListBox>().First();
        var scrollViewer = listBox.GetVisualDescendants().OfType<ScrollViewer>().First();

        scrollViewer.Offset = new Vector(0, scrollViewer.Extent.Height / 2);
        PumpLayout(window);

        var container = listBox.GetRealizedContainers().OfType<ListBoxItem>().Last();
        var item = Assert.IsType<NOcrTrainFontItem>(container.DataContext);
        var checkBox = container.GetVisualDescendants().OfType<CheckBox>().First();

        var before = item.IsSelected;
        checkBox.IsChecked = !before;
        Assert.Equal(!before, item.IsSelected);

        window.Close();
    }
}
