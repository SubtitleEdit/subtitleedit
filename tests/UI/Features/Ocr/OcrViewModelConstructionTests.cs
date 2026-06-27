using Avalonia.Headless.XUnit;
using Microsoft.Extensions.DependencyInjection;
using Nikse.SubtitleEdit;
using Nikse.SubtitleEdit.Features.Ocr;

namespace UITests.Features.Ocr;

public class OcrViewModelConstructionTests
{
    /// <summary>
    /// Regression guard (#11907 follow-up / "cannot open .sup"): the OcrViewModel constructor sets a
    /// language property whose [ObservableProperty] OnChanged auto-selects a spell-check dictionary;
    /// that ran before Dictionaries was initialized and threw a NullReferenceException, so the OCR
    /// window failed to construct and opening .sup / VobSub / any OCR source silently did nothing.
    /// Resolving it from the real DI container must not throw.
    /// </summary>
    [AvaloniaFact]
    public void OcrViewModel_ResolvesFromDiContainer_WithoutThrowing()
    {
        var services = new ServiceCollection();
        services.AddSubtitleEditServices();
        using var provider = services.BuildServiceProvider();

        var viewModel = provider.GetRequiredService<OcrViewModel>();

        Assert.NotNull(viewModel);
    }
}
