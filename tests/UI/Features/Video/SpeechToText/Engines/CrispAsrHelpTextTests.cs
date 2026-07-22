using Avalonia.Headless.XUnit;
using Nikse.SubtitleEdit.Features.Video.SpeechToText.Engines;

namespace UITests.Features.Video.SpeechToText.Engines;

/// <summary>
/// <see cref="CrispAsrEngineBase.GetHelpText"/> derives its asset name from <c>Name</c>, so a
/// backend whose .txt is missing used to throw straight into the caller's RelayCommand. Help
/// text is not worth taking the dialog down for - it must degrade to the shared text instead.
/// </summary>
public class CrispAsrHelpTextTests
{
    /// <summary>
    /// MADLAD has no CrispASRMADLAD.txt - it drives the shared download windows and is
    /// deliberately not registered as an STT backend, so it is the natural probe for the
    /// missing-asset path.
    /// </summary>
    [AvaloniaFact]
    public async Task GetHelpText_FallsBackToCommonText_WhenBackendAssetIsMissing()
    {
        var engine = new CrispAsrMadlad();

        var helpText = await engine.GetHelpText();

        // Wording from CrispASRCommon.txt: the shared text survives even with no backend header.
        Assert.False(string.IsNullOrWhiteSpace(helpText));
        Assert.Contains("--threads", helpText);
    }

    /// <summary>A backend that does ship a .txt must still get its own header plus the common text.</summary>
    [AvaloniaFact]
    public async Task GetHelpText_IncludesBackendHeader_WhenAssetExists()
    {
        var engine = new CrispAsrParakeet();

        var helpText = await engine.GetHelpText();

        // From CrispASRParakeet.txt, i.e. the per-backend header rather than only the common text.
        Assert.Contains("Parakeet", helpText);
    }
}
