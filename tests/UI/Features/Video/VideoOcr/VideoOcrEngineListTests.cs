using System;
using System.Linq;
using Avalonia.Headless.XUnit;
using Microsoft.Extensions.DependencyInjection;
using Nikse.SubtitleEdit;
using Nikse.SubtitleEdit.Features.Ocr.Engines;
using Nikse.SubtitleEdit.Features.Video.VideoOcr;

namespace UITests.Features.Video.VideoOcr;

/// <summary>
/// The Video OCR engine list: ordered best-measured-first per platform, and without the
/// pip-based Paddle Python engine - the one path that needs a manually managed Python
/// install, while every listed engine is either built in or downloaded automatically.
/// </summary>
public class VideoOcrEngineListTests
{
    [Fact]
    public void Engines_DoNotOfferPaddlePython()
    {
        Assert.DoesNotContain(VideoOcrEngineItem.GetEngines(),
            engine => engine.EngineType == OcrEngineType.PaddleOcrPython);
    }

    [Fact]
    public void Engines_BestMeasuredEngineComesFirst()
    {
        var first = VideoOcrEngineItem.GetEngines()[0].EngineType;

        if (AppleVisionOcr.IsAvailable())
        {
            Assert.Equal(OcrEngineType.AppleVision, first);
        }
        else if (OperatingSystem.IsWindows() || OperatingSystem.IsLinux())
        {
            Assert.Equal(OcrEngineType.PaddleOcrStandalone, first);
        }
    }

    [Fact]
    public void Engines_CloudEngineComesLast()
    {
        Assert.Equal(OcrEngineType.Glm, VideoOcrEngineItem.GetEngines()[^1].EngineType);
    }
}

/// <summary>
/// The video window offers only the CrispEmbed backends that measured well on burned-in
/// video, best first - not the full list the subtitle-bitmap OCR window shows.
/// </summary>
public class VideoOcrCrispEmbedBackendListTests
{
    [AvaloniaFact]
    public void CrispEmbedBackends_AreTheMeasuredVideoSet_BestFirst()
    {
        var services = new ServiceCollection();
        services.AddSubtitleEditServices();
        using var provider = services.BuildServiceProvider();
        var viewModel = provider.GetRequiredService<VideoOcrViewModel>();

        Assert.Equal(new[] { "GLM-OCR", "DeepSeek-OCR-2", "PP-OCRv6" },
            viewModel.CrispEmbedBackends.Select(p => p.Name));
    }
}
