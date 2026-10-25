using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.UiLogic.Ocr.FixEngine;
using Nikse.SubtitleEdit.UiLogic.SpellCheck;

namespace LibUiLogicTests.Ocr;

/// <summary>
/// Issue #16031 follow-up: a sans-serif capital I is a plain bar, so nOCR reads "Ik", "Is" and
/// "In" as "lk", "ls" and "ln", and an italic l as I ("aI"). The shipped Dutch list repairs
/// them; its "lk" rules used to require two lowercase letters after the word and never fired.
/// </summary>
public class DutchOcrFixReplaceListTests
{
    private static OcrFixReplaceList2 LoadShippedList()
    {
        var dir = new DirectoryInfo(Directory.GetCurrentDirectory());
        while (dir != null && !File.Exists(Path.Combine(dir.FullName, "SubtitleEdit.sln")))
        {
            dir = dir.Parent;
        }

        Assert.NotNull(dir);
        return new OcrFixReplaceList2(Path.Combine(dir!.FullName, "Dictionaries", "nld_OCRFixReplaceList.xml"));
    }

    [Theory]
    [InlineData("lk? Hoezo?", "Ik? Hoezo?")]
    [InlineData("Het moet.\nlk sprak hem maandag.", "Het moet.\nIk sprak hem maandag.")]
    [InlineData("ls dat zo?", "Is dat zo?")]
    [InlineData("ln de houding.", "In de houding.")]
    [InlineData("lkzelf weet het.", "lkzelf weet het.")] // not the whole word
    [InlineData("Ik dacht aI dat u zou komen.", "Ik dacht al dat u zou komen.")]
    [InlineData("AI is here.", "AI is here.")] // uppercase A: not an italic l
    public void SansSerifCapitalI_ReadAsL_IsRepaired(string input, string expected)
    {
        var list = LoadShippedList();

        var result = list.FixOcrErrorViaLineReplaceList(input, new Subtitle(), 0, new FakeDutchSpellChecker(), new List<string>(), spelledOK: false);

        Assert.Equal(expected.Replace("\r\n", "\n"), result.Replace("\r\n", "\n"));
    }

    private sealed class FakeDutchSpellChecker : ISpellChecker
    {
        private static readonly HashSet<string> Words = new(StringComparer.Ordinal)
        {
            "Ik", "ik", "Is", "is", "In", "in", "al", "dat", "zo", "Hoezo", "dacht", "u", "zou", "komen", "Het", "moet",
            "sprak", "hem", "maandag", "de", "houding", "weet", "het", "here", "AI",
        };

        public bool Initialize(string dictionaryFile, string twoLetterLanguageCode) => true;
        public bool IsWordCorrect(string word) => Words.Contains(word);
        public List<string> GetSuggestions(string word) => new();
    }
}
