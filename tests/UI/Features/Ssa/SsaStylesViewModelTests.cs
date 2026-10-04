using System.Threading.Tasks;
using Avalonia.Headless.XUnit;
using Microsoft.Extensions.DependencyInjection;
using Nikse.SubtitleEdit;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Features.Ssa;

namespace UITests.Features.Ssa;

public class SsaStylesViewModelTests
{
    private static SsaStylesViewModel MakeInitializedVm()
    {
        var services = new ServiceCollection();
        services.AddSubtitleEditServices();
        var provider = services.BuildServiceProvider();
        var vm = provider.GetRequiredService<SsaStylesViewModel>();

        var header = AdvancedSubStationAlpha.GetHeaderAndStylesFromAdvancedSubStationAlpha(
            AdvancedSubStationAlpha.DefaultHeader,
            new() { new SsaStyle { Name = "Default" }, new SsaStyle { Name = "Top" } });
        var subtitle = new Subtitle { Header = SubStationAlpha.GetHeaderAndStylesFromAdvancedSubStationAlpha(header, string.Empty) };
        subtitle.Paragraphs.Add(new Paragraph("a", 0, 1000) { Extra = "Default" });
        vm.Initialize(subtitle, new SubStationAlpha(), "test.ssa", "Default", null);
        return vm;
    }

    /// <summary>
    /// "Clear" on the file styles also lets go of the current style - the style editor must not
    /// keep editing a style that is no longer in the list.
    /// </summary>
    [AvaloniaFact]
    public async Task FileRemoveAll_ClearsFileStylesAndCurrentStyle()
    {
        var vm = MakeInitializedVm();
        Assert.Equal(2, vm.FileStyles.Count);
        Assert.NotNull(vm.CurrentStyle);

        await vm.FileRemoveAllCommand.ExecuteAsync(null);

        Assert.Empty(vm.FileStyles);
        Assert.Null(vm.CurrentStyle);
        Assert.Null(vm.SelectedFileStyle);
        vm.OnClosingCleanup();
    }
}
