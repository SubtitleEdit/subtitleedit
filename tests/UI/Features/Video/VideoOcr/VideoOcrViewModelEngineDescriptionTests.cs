using Avalonia.Headless.XUnit;
using Microsoft.Extensions.DependencyInjection;
using Nikse.SubtitleEdit;
using Nikse.SubtitleEdit.Features.Video.VideoOcr;
using System.Linq;

namespace UITests.Features.Video.VideoOcr;

/// <summary>
/// The engine description line under the Video OCR engine combo. Every engine carries a
/// one-line description; these guard that it is actually populated and that it follows the
/// selection, since the property was dead plumbing before it had a home in the window.
/// </summary>
public class VideoOcrViewModelEngineDescriptionTests
{
    [AvaloniaFact]
    public void EveryEngine_HasADescription()
    {
        Assert.All(VideoOcrEngineItem.GetEngines(),
            engine => Assert.False(string.IsNullOrWhiteSpace(engine.Description)));
    }

    [AvaloniaFact]
    public void Description_FollowsTheSelectedEngine()
    {
        var viewModel = MakeViewModel();

        foreach (var engine in viewModel.Engines)
        {
            viewModel.SelectedEngine = engine;
            Assert.Equal(engine.Description, viewModel.SelectedEngineDescription);
        }
    }

    [AvaloniaFact]
    public void Description_IsSetForTheInitialSelection()
    {
        var viewModel = MakeViewModel();

        Assert.False(string.IsNullOrWhiteSpace(viewModel.SelectedEngineDescription));
        Assert.Equal(viewModel.SelectedEngine.Description, viewModel.SelectedEngineDescription);
    }

    private static VideoOcrViewModel MakeViewModel()
    {
        var services = new ServiceCollection();
        services.AddSubtitleEditServices();
        using var provider = services.BuildServiceProvider();
        return provider.GetRequiredService<VideoOcrViewModel>();
    }
}
