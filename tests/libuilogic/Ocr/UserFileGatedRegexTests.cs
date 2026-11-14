using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.UiLogic.Ocr.FixEngine;
using Nikse.SubtitleEdit.UiLogic.SpellCheck;

namespace LibUiLogicTests.Ocr;

// #16377: a <RegularExpressionsIfSpelledCorrectly> rule in spa_OCRFixReplaceList_User.xml never
// ran - only the shipped list's gated section was read - while a plain <RegularExpressions> rule
// in the same file did. Here "¡" misread as "i" before a capitalized word.
public class UserFileGatedRegexTests : IDisposable
{
    private const string MainList =
        "<?xml version=\"1.0\" encoding=\"utf-8\"?><OCRFixReplaceList>" +
        "<RegularExpressionsIfSpelledCorrectly>" +
        "<RegEx find=\"\\bde([A-Z][a-z]+)\\b\" spellCheck=\"$1\" replaceWith=\"de $1\" />" +
        "</RegularExpressionsIfSpelledCorrectly>" +
        "</OCRFixReplaceList>";

    private const string UserRule =
        "<RegEx find=\"\\b[iI]([A-ZÁÉÍÓÚÑÜ][a-záéíóúñü]+)\" spellCheck=\"$1\" replaceWith=\"¡$1\" />";

    private readonly string _folder;

    public UserFileGatedRegexTests()
    {
        _folder = Path.Combine(Path.GetTempPath(), "SeOcrUserGated_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_folder);
        File.WriteAllText(Path.Combine(_folder, "spa_OCRFixReplaceList.xml"), MainList);
    }

    [Theory]
    [InlineData("No irás a... - iFuera!", "No irás a... - ¡Fuera!")]
    [InlineData("Está jodido, iEstá todo jodido!", "Está jodido, ¡Está todo jodido!")]
    [InlineData("iTheo!", "iTheo!")] // "Theo" is not a dictionary word - the gate keeps it
    public void GatedRuleInUserFile_IsApplied(string text, string expected)
    {
        WriteUserList("<RegularExpressionsIfSpelledCorrectly>" + UserRule + "</RegularExpressionsIfSpelledCorrectly>");

        Assert.Equal(expected, Fix(text));
    }

    [Fact]
    public void ShippedGatedRules_StillRunWithAUserFile()
    {
        WriteUserList("<RegularExpressionsIfSpelledCorrectly>" + UserRule + "</RegularExpressionsIfSpelledCorrectly>");

        Assert.Equal("Vengo de Madrid, ¡Fuera!", Fix("Vengo deMadrid, iFuera!"));
    }

    [Fact]
    public void WithoutAUserFile_ShippedGatedRulesRun()
    {
        Assert.Equal("Vengo de Madrid, iFuera!", Fix("Vengo deMadrid, iFuera!"));
    }

    private void WriteUserList(string sections)
    {
        File.WriteAllText(
            Path.Combine(_folder, "spa_OCRFixReplaceList_User.xml"),
            "<?xml version=\"1.0\" encoding=\"utf-8\"?><OCRFixReplaceList>" + sections + "</OCRFixReplaceList>");
    }

    private string Fix(string text)
    {
        var checker = new FakeSpanishSpellChecker();
        var list = new OcrFixReplaceList2(Path.Combine(_folder, "spa_OCRFixReplaceList.xml"));

        return list.FixOcrErrorViaLineReplaceList(
            text, new Subtitle(), 0, checker, new List<string>(), spelledOK: false, checker.IsWordCorrect);
    }

    public void Dispose()
    {
        try
        {
            Directory.Delete(_folder, recursive: true);
        }
        catch
        {
            // Best-effort cleanup.
        }
    }

    // Case-insensitive, like Hunspell accepting a capitalized dictionary word.
    private sealed class FakeSpanishSpellChecker : ISpellChecker
    {
        private static readonly HashSet<string> Words = new(StringComparer.OrdinalIgnoreCase)
        {
            "no", "irás", "a", "fuera", "está", "jodido", "todo", "vengo", "de", "madrid",
        };

        public bool Initialize(string dictionaryFile, string twoLetterLanguageCode) => true;

        public bool IsWordCorrect(string word) => string.IsNullOrWhiteSpace(word) || Words.Contains(word);

        public List<string> GetSuggestions(string word) => new();
    }
}
