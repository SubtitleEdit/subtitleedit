using System.Collections.Generic;
using System.Linq;
using Avalonia.Headless.XUnit;
using Microsoft.Extensions.DependencyInjection;
using Nikse.SubtitleEdit;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Features.Assa;
using Nikse.SubtitleEdit.Features.Ssa;

namespace UITests.Features.Assa;

/// <summary>
/// The border type combo is not bound to the current style; it writes its selection into it.
/// It must follow every change of the current style - Initialize used to leave it at the last
/// file style's border type, only corrected later by the grid's selection event.
/// </summary>
public class StylesDialogBorderTypeTests
{
    private static Subtitle MakeSubtitle()
    {
        var styles = new List<SsaStyle>
        {
            new() { Name = "Default", BorderStyle = "1" },
            new() { Name = "Boxed", BorderStyle = "3" },
        };
        var subtitle = new Subtitle
        {
            Header = AdvancedSubStationAlpha.GetHeaderAndStylesFromAdvancedSubStationAlpha(AdvancedSubStationAlpha.DefaultHeader, styles),
        };
        subtitle.Paragraphs.Add(new Paragraph("one", 0, 1000) { Extra = "Default" });
        subtitle.Paragraphs.Add(new Paragraph("two", 1000, 2000) { Extra = "Boxed" });
        return subtitle;
    }

    [AvaloniaFact]
    public void AssaStyles_BorderTypeFollowsCurrentStyle()
    {
        var services = new ServiceCollection();
        services.AddSubtitleEditServices();
        using var provider = services.BuildServiceProvider();
        var vm = provider.GetRequiredService<AssaStylesViewModel>();

        vm.Initialize(MakeSubtitle(), new AdvancedSubStationAlpha(), "test.ass", "Default", null);

        Assert.Equal("Default", vm.CurrentStyle!.Name);
        Assert.Same(vm.CurrentStyle.BorderStyle, vm.SelectedBorderType);
        Assert.Equal(BorderStyleType.Outline, vm.SelectedBorderType!.Style);

        vm.CurrentStyle = vm.FileStyles.Single(p => p.Name == "Boxed");
        Assert.Equal(BorderStyleType.BoxPerLine, vm.SelectedBorderType!.Style);

        vm.CurrentStyle = null;
        Assert.Same(vm.BorderTypes[0], vm.SelectedBorderType);
    }

    [AvaloniaFact]
    public void SsaStyles_BorderTypeFollowsCurrentStyle()
    {
        var services = new ServiceCollection();
        services.AddSubtitleEditServices();
        using var provider = services.BuildServiceProvider();
        var vm = provider.GetRequiredService<SsaStylesViewModel>();

        vm.Initialize(MakeSubtitle(), new SubStationAlpha(), "test.ssa", "Default", null);

        Assert.Equal("Default", vm.CurrentStyle!.Name);
        Assert.Equal(BorderStyleType.Outline, vm.SelectedBorderType!.Style);

        vm.CurrentStyle = vm.FileStyles.Single(p => p.Name == "Boxed");
        Assert.Equal(BorderStyleType.BoxPerLine, vm.SelectedBorderType!.Style);
    }
}
