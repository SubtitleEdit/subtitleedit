using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Microsoft.Extensions.DependencyInjection;
using Nikse.SubtitleEdit;
using Nikse.SubtitleEdit.Features.Tools.BatchConvert;

namespace UITests.Features.Tools.BatchConvert;

/// <summary>
/// The functions list can be narrowed with the search box in the "Options" header, and its
/// rows are 24 px rather than the checkbox theme's fixed 32 px.
/// </summary>
public class BatchConvertFunctionSearchTests
{
    private static BatchConvertViewModel MakeViewModel()
    {
        var services = new ServiceCollection();
        services.AddSubtitleEditServices();
        return services.BuildServiceProvider().GetRequiredService<BatchConvertViewModel>();
    }

    [AvaloniaFact]
    public void Search_NarrowsTheList_AndSelectsTheFirstMatch()
    {
        var vm = MakeViewModel();
        Assert.Equal(vm.BatchFunctions.Count, vm.VisibleBatchFunctions.Count);

        var target = vm.BatchFunctions[^1];
        vm.FunctionSearchText = target.Name.ToUpperInvariant();

        Assert.Contains(target, vm.VisibleBatchFunctions);
        Assert.All(vm.VisibleBatchFunctions, f => Assert.Contains(target.Name, f.Name, StringComparison.CurrentCultureIgnoreCase));
        Assert.Same(vm.VisibleBatchFunctions[0], vm.SelectedBatchFunction);

        vm.FunctionSearchText = string.Empty;
        Assert.Equal(vm.BatchFunctions, vm.VisibleBatchFunctions);
    }

    [AvaloniaFact]
    public void Search_KeepsTheSelection_WhileItStillMatches()
    {
        var vm = MakeViewModel();
        var selected = vm.BatchFunctions[2];
        vm.SelectedBatchFunction = selected;

        vm.FunctionSearchText = selected.Name[..3];
        Assert.Same(selected, vm.SelectedBatchFunction);

        vm.FunctionSearchText = "no function is called this";
        Assert.Empty(vm.VisibleBatchFunctions);
        Assert.Null(vm.SelectedBatchFunction);
    }

    [AvaloniaFact]
    public void FunctionRows_AreShorterThanTheCheckBoxTheme()
    {
        var vm = MakeViewModel();
        var window = new BatchConvertWindow(vm);
        window.Show();
        Dispatcher.UIThread.RunJobs();

        var checkBox = window.GetVisualDescendants().OfType<CheckBox>()
            .First(c => c.DataContext is BatchConvertFunction);
        Assert.Equal(24, checkBox.Bounds.Height);

        window.Close();
    }
}
