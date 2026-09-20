using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.UiLogic.Ocr;

namespace UITests.Logic.Config;

public class LlamaCppOcrPromptMigrationTests
{
    private const string LegacyPrompt = "Extract all text exactly as written. The language is {language}. Preserve line breaks.";

    [Fact]
    public void LegacyPromptIsReplaced()
    {
        var ocr = new SeOcr { LlamaCppOcrPrompt = LegacyPrompt };

        Se.MigrateLlamaCppOcrPrompt(ocr);

        Assert.Equal(SeOcrDefaults.LlamaCppOcrPrompt, ocr.LlamaCppOcrPrompt);
    }

    [Fact]
    public void DefaultPromptIsUnchanged()
    {
        var ocr = new SeOcr();

        Se.MigrateLlamaCppOcrPrompt(ocr);

        Assert.Equal(SeOcrDefaults.LlamaCppOcrPrompt, ocr.LlamaCppOcrPrompt);
    }

    // The whole point of matching verbatim: anything the user typed themselves survives.
    [Theory]
    [InlineData("Read the text. Language: {language}.")]
    [InlineData("Extract all text exactly as written. The language is {language}. Preserve line breaks. Keep music notes.")]
    [InlineData("Extract all text exactly as written. Preserve line breaks.")]
    public void CustomizedPromptIsKept(string prompt)
    {
        var ocr = new SeOcr { LlamaCppOcrPrompt = prompt };

        Se.MigrateLlamaCppOcrPrompt(ocr);

        Assert.Equal(prompt, ocr.LlamaCppOcrPrompt);
    }

    [Fact]
    public void EmptyPromptIsLeftAlone()
    {
        // An empty prompt already means "use the engine default" in LlamaCppOcr.Ocr.
        var ocr = new SeOcr { LlamaCppOcrPrompt = string.Empty };

        Se.MigrateLlamaCppOcrPrompt(ocr);

        Assert.Equal(string.Empty, ocr.LlamaCppOcrPrompt);
    }

    [Fact]
    public void NullPromptDoesNotThrow()
    {
        var ocr = new SeOcr { LlamaCppOcrPrompt = null! };

        Se.MigrateLlamaCppOcrPrompt(ocr);

        Assert.Null(ocr.LlamaCppOcrPrompt);
    }

    // #14221: the new prompt makes the model count the lines first. Both halves matter - the
    // line-counting instruction and "exactly as written" - so guard the wording, and the
    // {language} placeholder the OCR settings dialog validates for.
    [Fact]
    public void DefaultPromptCountsLinesAndKeepsTheLanguagePlaceholder()
    {
        Assert.Contains("number of lines", SeOcrDefaults.LlamaCppOcrPrompt);
        Assert.Contains("exactly as written", SeOcrDefaults.LlamaCppOcrPrompt);
        Assert.Contains("{language}", SeOcrDefaults.LlamaCppOcrPrompt);
        Assert.DoesNotContain("Preserve line breaks", SeOcrDefaults.LlamaCppOcrPrompt);
    }
}
