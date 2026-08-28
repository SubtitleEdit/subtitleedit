using Nikse.SubtitleEdit.UiLogic.Ocr;
using SeConv.Core;
using SeConv.Helpers;
using Xunit;

namespace SeConvTests.Core;

/// <summary>
/// Covers <c>--ocr-prompt</c> (#14221): the option itself, and the fact that seconv's OCR default
/// is the same string the OCR window uses instead of a third hand-kept copy.
/// </summary>
public class OcrPromptOptionTest
{
    [Fact]
    public void OcrPrompt_IsBoundAsAnOption()
    {
        var option = Assert.Single(CliSchema.Options, o => o.Name == "--ocr-prompt");

        Assert.Contains("--ocrprompt", option.Aliases);
        Assert.Equal("option", option.Group);
    }

    // The prompt reader is shared with --translate-prompt; only the wording of its errors and the
    // placeholder hint change, so the OCR caller gets the same file/inline handling for free.
    [Fact]
    public void ReadPromptOption_InlineOcrPrompt_StaysInline()
    {
        const string prompt = "Read every line. The language is {language}.";

        Assert.Equal(prompt, AutoTranslateRunner.ReadPromptOption(prompt, "--ocr-prompt", "{language}"));
    }

    [Fact]
    public void ReadPromptOption_OcrPromptFile_IsReadFromDisk()
    {
        var path = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName() + ".txt");
        File.WriteAllText(path, "Count the lines, then read them. Language: {language}.\n");
        try
        {
            Assert.Equal(
                "Count the lines, then read them. Language: {language}.",
                AutoTranslateRunner.ReadPromptOption(path, "--ocr-prompt", "{language}"));
        }
        finally
        {
            File.Delete(path);
        }
    }

    // A path-shaped value that does not exist must fail loudly rather than be sent to the model
    // as the prompt - the same trap --translate-prompt guards against, and the message has to
    // name the option the user actually typed.
    [Fact]
    public void ReadPromptOption_MissingOcrPromptFile_NamesTheOcrOption()
    {
        var ex = Assert.Throws<InvalidOperationException>(
            () => AutoTranslateRunner.ReadPromptOption("no-such-ocr-prompt.txt", "--ocr-prompt", "{language}"));

        Assert.Contains("not found", ex.Message);
        Assert.Contains("--ocr-prompt", ex.Message);
        Assert.Contains("{language}", ex.Message);
        Assert.DoesNotContain("--translate-prompt", ex.Message);
    }

    [Fact]
    public void ReadPromptOption_EmptyOcrPrompt_NamesTheOcrOption()
    {
        var ex = Assert.Throws<InvalidOperationException>(
            () => AutoTranslateRunner.ReadPromptOption("   ", "--ocr-prompt", "{language}"));

        Assert.Contains("--ocr-prompt is empty", ex.Message);
    }

    [Fact]
    public void ReadPromptOption_DefaultsStillDescribeTranslate()
    {
        var ex = Assert.Throws<InvalidOperationException>(() => AutoTranslateRunner.ReadPromptOption("   "));

        Assert.Contains("--translate-prompt is empty", ex.Message);
    }

    // seconv used to hold its own copy of the prompt, kept in sync by a comment. #14221 changed
    // the default and the copy would have been missed, so both now read the shared constant.
    [Fact]
    public void SharedDefaultPrompt_CountsLinesAndKeepsThePlaceholder()
    {
        Assert.Contains("number of lines", SeOcrDefaults.LlamaCppOcrPrompt);
        Assert.Contains("exactly as written", SeOcrDefaults.LlamaCppOcrPrompt);
        Assert.Contains("{language}", SeOcrDefaults.LlamaCppOcrPrompt);
    }
}
