using Avalonia.Headless.XUnit;
using Nikse.SubtitleEdit.UiLogic.AudioToText;
using Nikse.SubtitleEdit.Features.Video.SpeechToText.Engines;

namespace UITests.Features.Video.SpeechToText.Engines;

/// <summary>
/// GigaAM-v3 is Russian-only and its "e2e" revisions are the ones that write punctuation and
/// capital letters. Both facts are encoded in the model list and language list rather than in
/// runtime logic, so they are what these tests pin.
/// </summary>
public class CrispAsrGigaAmTests
{
    [Fact]
    public void Languages_AreRussianOnly()
    {
        var engine = new CrispAsrGigaAm();

        // crispasr prints "'en' ignored — GigaAM-v3 is a Russian-only model" for anything else,
        // so offering a second language in the combo would only produce a silent no-op.
        var language = Assert.Single(engine.Languages);
        Assert.Equal("ru", language.Code);
        Assert.Equal("ru", engine.DefaultLanguage);
    }

    [Fact]
    public void Models_ListPunctuatingRevisionsFirst()
    {
        var engine = new CrispAsrGigaAm();

        // The first entry is what the model combo lands on by default; the plain ctc/rnnt heads
        // return lowercase text with no punctuation, which is the wrong default for subtitles.
        Assert.StartsWith("gigaam-v3-e2e-", engine.Models[0].Name);
        Assert.All(engine.Models.Take(6), m => Assert.StartsWith("gigaam-v3-e2e-", m.Name));
        Assert.All(engine.Models.Skip(6), m => Assert.DoesNotContain("-e2e-", m.Name));
    }

    [Fact]
    public void Models_AllPointAtTheGigaAmV3Repo()
    {
        var engine = new CrispAsrGigaAm();

        Assert.Equal(12, engine.Models.Count);
        foreach (var model in engine.Models)
        {
            var url = Assert.Single(model.Urls);
            Assert.Equal($"https://huggingface.co/cstr/gigaam-v3-GGUF/resolve/main/{model.Name}", url);
        }
    }

    [Fact]
    public void Backend_IsRegisteredWithTheCrispAsrEngine()
    {
        var engine = new CrispAsrEngine();

        Assert.True(engine.TrySelectBackendChoice(WhisperChoice.CrispAsrGigaAm));
        Assert.Equal("gigaam", engine.BackendName);
    }

    /// <summary>
    /// The help asset name is derived from <c>Name</c> with spaces removed, so a rename that
    /// isn't mirrored in Assets/SpeechToText silently drops the backend header.
    /// </summary>
    [AvaloniaFact]
    public async Task GetHelpText_IncludesTheGigaAmHeader()
    {
        var engine = new CrispAsrGigaAm();

        var helpText = await engine.GetHelpText();

        Assert.Contains("GigaAM-v3", helpText);
    }
}
