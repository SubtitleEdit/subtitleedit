using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Ocr;
using Nikse.SubtitleEdit.Features.Ocr.FixEngine;
using Nikse.SubtitleEdit.Features.Ocr.OcrSubtitle;
using Nikse.SubtitleEdit.Features.SpellCheck;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Dictionaries;
using System;
using System.IO;

namespace UITests.Features.Ocr.FixEngine;

public class OcrFixEngineUserDictionaryTests : IDisposable
{
    private const string LanguageName = "en_US";
    private readonly string _testWord = $"zzzxqvnonceword{Guid.NewGuid():N}".ToLowerInvariant();

    public OcrFixEngineUserDictionaryTests()
    {
        Directory.CreateDirectory(Se.DictionariesFolder);
        UserWordsHelper.RemoveWord(_testWord, LanguageName);
    }

    [Fact]
    public void AddUserWord_ShouldMakeWordCorrectWithoutReinitializingEngine()
    {
        var subtitle = new Subtitle();
        subtitle.Paragraphs.Add(new Paragraph(new TimeCode(0), new TimeCode(1000), _testWord));

        var ocrSubtitle = new OcrSubtitleDummy(subtitle);
        var items = ocrSubtitle.MakeOcrSubtitleItems();
        items[0].Text = _testWord;

        var spellCheckManager = new SpellCheckManager();
        IOcrFixEngine ocrFixEngine = new OcrFixEngine(spellCheckManager);
        ocrFixEngine.Initialize(items, "eng", new SpellCheckDictionaryDisplay
        {
            Name = "English [en_US]",
            DictionaryFileName = GetEnglishDictionaryFileName(),
        });

        var beforeAddResult = ocrFixEngine.FixOcrErrors(0, items[0], doTryToGuessUnknownWords: false);
        var beforeAddWord = Assert.Single(beforeAddResult.Words, w => w.LinePartType == OcrFixLinePartType.Word);
        Assert.False(beforeAddWord.IsSpellCheckedOk);

        Assert.True(ocrFixEngine.AddUserWord(_testWord));

        var afterAddResult = ocrFixEngine.FixOcrErrors(0, items[0], doTryToGuessUnknownWords: false);
        var afterAddWord = Assert.Single(afterAddResult.Words, w => w.LinePartType == OcrFixLinePartType.Word);
        Assert.True(afterAddWord.IsSpellCheckedOk);
    }

    public void Dispose()
    {
        UserWordsHelper.RemoveWord(_testWord, LanguageName);
    }

    private static string GetEnglishDictionaryFileName()
    {
        var repoRoot = FindRepoRoot();
        var dictionaryFileName = Path.Combine(repoRoot, "Dictionaries", "en_US.dic");
        Assert.True(File.Exists(dictionaryFileName), $"Dictionary file not found: {dictionaryFileName}");
        return dictionaryFileName;
    }

    private static string FindRepoRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory != null && !File.Exists(Path.Combine(directory.FullName, "SubtitleEdit.sln")))
        {
            directory = directory.Parent;
        }

        Assert.NotNull(directory);
        return directory!.FullName;
    }
}
