using Avalonia.Headless.XUnit;
using Microsoft.Extensions.DependencyInjection;
using Nikse.SubtitleEdit;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Tools.RemoveTextForHearingImpaired;

namespace UITests.Features.Tools.RemoveTextForHearingImpaired;

public class RemoveTextForHearingImpairedViewModelTests
{
    private static RemoveTextForHearingImpairedViewModel Resolve()
    {
        var services = new ServiceCollection();
        services.AddSubtitleEditServices();
        return services.BuildServiceProvider().GetRequiredService<RemoveTextForHearingImpairedViewModel>();
    }

    [AvaloniaFact]
    public void Apply_IsHidden_WhenNoCallbackProvided()
    {
        var vm = Resolve();
        vm.Initialize(new Subtitle());
        Assert.False(vm.IsApplyVisible);
    }

    [AvaloniaFact]
    public void Apply_PushesTickedFixesToCallback_WithoutNeedingOk()
    {
        var vm = Resolve();
        var sub = new Subtitle();
        sub.Paragraphs.Add(new Paragraph("[door slams]", 0, 1000));
        sub.Paragraphs.Add(new Paragraph("Hello there", 1000, 2000));

        Subtitle? applied = null;
        vm.Initialize(sub, s => applied = s);

        Assert.True(vm.IsApplyVisible);

        // Simulate the preview: line 0 becomes empty (removed), line 1 unchanged.
        vm.Fixes.Add(new RemoveItem(true, 0, "[door slams]", string.Empty, sub.Paragraphs[0]));

        vm.ApplyCommand.Execute(null);

        Assert.NotNull(applied);
        Assert.False(vm.OkPressed); // Apply must not close / set OkPressed
        Assert.Single(applied!.Paragraphs); // the emptied line was dropped
        Assert.Equal("Hello there", applied.Paragraphs[0].Text);
    }
}
