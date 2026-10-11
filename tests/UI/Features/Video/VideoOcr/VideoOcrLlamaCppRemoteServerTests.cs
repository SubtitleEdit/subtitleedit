using Avalonia.Headless.XUnit;
using Microsoft.Extensions.DependencyInjection;
using Nikse.SubtitleEdit;
using Nikse.SubtitleEdit.Features.Ocr.Engines;
using Nikse.SubtitleEdit.Features.Video.VideoOcr;
using Nikse.SubtitleEdit.Logic.Config;
using System.Linq;

namespace UITests.Features.Video.VideoOcr;

/// <summary>
/// Video OCR shares the llama.cpp OCR settings with image OCR, so the "use external server"
/// setting (#15854) must switch it to the URL instead of the local model and server.
/// </summary>
public class VideoOcrLlamaCppRemoteServerTests
{
    private static SettingsScope OcrScope() => new(
        "Ocr.LlamaCppUseRemoteServer",
        "Ocr.LlamaCppUrl");

    [AvaloniaFact]
    public void RemoteServer_ShowsUrlInsteadOfLocalModel()
    {
        using var _ = OcrScope();
        Se.Settings.Ocr.LlamaCppUseRemoteServer = true;
        Se.Settings.Ocr.LlamaCppUrl = "http://192.168.1.10:8080/v1/chat/completions";

        var vm = MakeViewModel();
        SelectLlamaCpp(vm);

        Assert.Equal("http://192.168.1.10:8080/v1/chat/completions", vm.LlamaCppUrl);
        Assert.True(vm.IsLlamaCppRemoteVisible);
        Assert.False(vm.IsLlamaCppLocalVisible);
    }

    [AvaloniaFact]
    public void LocalServer_ShowsModelControls()
    {
        using var _ = OcrScope();
        Se.Settings.Ocr.LlamaCppUseRemoteServer = false;

        var vm = MakeViewModel();
        SelectLlamaCpp(vm);

        Assert.True(vm.IsLlamaCppLocalVisible);
        Assert.False(vm.IsLlamaCppRemoteVisible);
    }

    private static void SelectLlamaCpp(VideoOcrViewModel vm)
    {
        var llamaCpp = vm.Engines.First(e => e.EngineType == OcrEngineType.LlamaCpp);
        vm.SelectedEngine = vm.Engines.First(e => e != llamaCpp);
        vm.SelectedEngine = llamaCpp;
    }

    private static VideoOcrViewModel MakeViewModel()
    {
        var services = new ServiceCollection();
        services.AddSubtitleEditServices();
        using var provider = services.BuildServiceProvider();
        return provider.GetRequiredService<VideoOcrViewModel>();
    }
}
