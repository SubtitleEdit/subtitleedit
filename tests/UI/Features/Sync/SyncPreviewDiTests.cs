using Avalonia.Headless.XUnit;
using Microsoft.Extensions.DependencyInjection;
using Nikse.SubtitleEdit;
using Nikse.SubtitleEdit.Features.Sync.VisualSync;
using Nikse.SubtitleEdit.Logic.Media;

namespace UITests.Features.Sync;

/// <summary>
/// Visual sync asks for two <see cref="IVideoPreviewSubtitle"/> - one per player. They have to be
/// two objects: each keeps the temp subtitle file it handed to its own player, so a single shared
/// instance would have the two players overwriting each other's file.
/// </summary>
public class SyncPreviewDiTests
{
    [AvaloniaFact]
    public void Container_GivesVisualSyncOnePreviewPerPlayer()
    {
        var collection = new ServiceCollection();
        collection.AddSubtitleEditServices();
        using var provider = collection.BuildServiceProvider();

        var first = provider.GetRequiredService<IVideoPreviewSubtitle>();
        var second = provider.GetRequiredService<IVideoPreviewSubtitle>();
        Assert.NotSame(first, second);

        var vm = provider.GetRequiredService<VisualSyncViewModel>();
        Assert.NotNull(vm);
    }
}
