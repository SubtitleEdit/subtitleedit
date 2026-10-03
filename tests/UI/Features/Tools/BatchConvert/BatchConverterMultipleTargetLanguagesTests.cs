using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Features.Tools.BatchConvert;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.UiLogic.AutoTranslate;
using Nikse.SubtitleEdit.UiLogic.BatchConvert;
using Nikse.SubtitleEdit.UiLogic.Translate;

namespace UITests.Features.Tools.BatchConvert;

/// <summary>
/// Batch Convert can translate one file into several languages in one run (Reddit request):
/// each language starts from the untranslated text and gets its own output file, and the
/// steps after the translation use the target language's rules.
/// </summary>
public class BatchConverterMultipleTargetLanguagesTests
{
    private const string InputSrt = @"1
00:00:01,000 --> 00:00:03,000
Hello world.
";

    /// <summary>Prefixes the text with the target code, and fails for <see cref="FailFor"/>.</summary>
    private sealed class TaggingTranslator : IAutoTranslator
    {
        public string? FailFor { get; init; }
        public List<string> Calls { get; } = new();
        public string Name => "Tagging";
        public string Url => string.Empty;
        public string Error { get; set; } = string.Empty;
        public int MaxCharacters => 1000;
        public void Initialize() { }
        public List<TranslationPair> GetSupportedSourceLanguages() => new() { new TranslationPair("English", "en") };
        public List<TranslationPair> GetSupportedTargetLanguages() => new();

        public Task<string> Translate(string text, string sourceLanguageCode, string targetLanguageCode, CancellationToken cancellationToken)
        {
            Calls.Add(targetLanguageCode);
            if (FailFor == "*" || targetLanguageCode == FailFor)
            {
                throw new InvalidOperationException("engine down for " + targetLanguageCode);
            }

            return Task.FromResult($"[{targetLanguageCode}] {text}");
        }
    }

    private static async Task Run(string postFix, Func<DirectoryInfo, BatchConverter, Task> test)
    {
        var oldPostFix = Se.Settings.Tools.BatchConvert.LanguagePostFix;
        Se.Settings.Tools.BatchConvert.LanguagePostFix = postFix;
        var dir = Directory.CreateTempSubdirectory("se-batch-multi-target-test");
        try
        {
            await test(dir, new BatchConverter(null!, null!, null!));
        }
        finally
        {
            Se.Settings.Tools.BatchConvert.LanguagePostFix = oldPostFix;
            dir.Delete(recursive: true);
        }
    }

    private static BatchConvertConfig MakeConfig(IAutoTranslator translator, string target, params string[] extraTargets)
    {
        var config = new BatchConvertConfig
        {
            SaveInSourceFolder = true,
            Overwrite = false,
            TargetFormatName = SubRip.NameOfFormat,
        };
        config.AutoTranslate.IsActive = true;
        config.AutoTranslate.SourceLanguage = new TranslationPair("English", "en");
        config.AutoTranslate.TargetLanguage = new TranslationPair(target, target);
        config.AutoTranslate.ExtraTargetLanguages = extraTargets.Select(p => new TranslationPair(p, p)).ToList();
        config.AutoTranslate.Translator = translator;
        return config;
    }

    private static async Task<BatchConvertItem> ConvertFile(BatchConverter converter, DirectoryInfo dir)
    {
        var inputFile = Path.Combine(dir.FullName, "movie.srt");
        await File.WriteAllTextAsync(inputFile, InputSrt, TestContext.Current.CancellationToken);
        var item = new BatchConvertItem(inputFile, new FileInfo(inputFile).Length, new SubRip().Name, Subtitle.Parse(inputFile));
        await converter.Convert(item, TestContext.Current.CancellationToken);
        return item;
    }

    private static string ReadText(DirectoryInfo dir, string fileName)
    {
        var subtitle = Subtitle.Parse(Path.Combine(dir.FullName, fileName));
        return subtitle.Paragraphs.Single().Text;
    }

    [Fact]
    public async Task SeveralTargets_EachTranslatedFromSource_IntoOwnFile_EvenWithNoLanguageCodeSetting()
    {
        await Run(Se.Language.General.NoLanguageCode, async (dir, converter) =>
        {
            converter.Initialize(MakeConfig(new TaggingTranslator(), "de", "fr", "da"));

            await ConvertFile(converter, dir);

            // "No language code" would name all three "movie.srt", "movie_2.srt", ... - with
            // several targets the language code is what tells them apart.
            Assert.Equal("[de] Hello world.", ReadText(dir, "movie.de.srt"));
            Assert.Equal("[fr] Hello world.", ReadText(dir, "movie.fr.srt"));
            Assert.Equal("[da] Hello world.", ReadText(dir, "movie.da.srt"));
            Assert.False(File.Exists(Path.Combine(dir.FullName, "movie_2.srt")));
        });
    }

    [Fact]
    public async Task OneTargetFails_OthersAreStillWritten_AndStatusNamesTheFailure()
    {
        await Run(Se.Language.General.TwoLetterLanguageCode, async (dir, converter) =>
        {
            converter.Initialize(MakeConfig(new TaggingTranslator { FailFor = "fr" }, "de", "fr", "da"));

            var item = await ConvertFile(converter, dir);

            Assert.True(File.Exists(Path.Combine(dir.FullName, "movie.de.srt")));
            Assert.True(File.Exists(Path.Combine(dir.FullName, "movie.da.srt")));
            Assert.False(File.Exists(Path.Combine(dir.FullName, "movie.fr.srt")));
            Assert.Contains("fr", item.Status);
        });
    }

    [Fact]
    public async Task AllTargetsFail_Throws()
    {
        await Run(Se.Language.General.TwoLetterLanguageCode, async (dir, converter) =>
        {
            converter.Initialize(MakeConfig(new TaggingTranslator { FailFor = "*" }, "de", "fr"));

            await Assert.ThrowsAnyAsync<Exception>(() => ConvertFile(converter, dir));
        });
    }

    [Fact]
    public void ExtraTargets_SkipDuplicatesAndSourceLanguage()
    {
        var converter = new BatchConverter(null!, null!, null!);
        converter.Initialize(MakeConfig(new TaggingTranslator(), "de", "de", "en", "fr", "FR"));

        var codes = converter.GetTargetLanguages().Select(p => p.Code).ToList();

        Assert.Equal(new[] { "de", "fr" }, codes);
    }

    [Fact]
    public async Task SingleTarget_UnchangedNaming()
    {
        await Run(Se.Language.General.TwoLetterLanguageCode, async (dir, converter) =>
        {
            var translator = new TaggingTranslator();
            converter.Initialize(MakeConfig(translator, "de"));

            await ConvertFile(converter, dir);

            Assert.Equal("[de] Hello world.", ReadText(dir, "movie.de.srt"));
            Assert.Equal(new[] { "de" }, translator.Calls.Distinct());
        });
    }

    [Fact]
    public async Task AfterTranslation_LanguageIsTheTarget()
    {
        // Casing, auto balance, remove text for HI, fix common errors etc. run after the
        // translation and used the detected source language ("en") for translated text.
        await Run(Se.Language.General.TwoLetterLanguageCode, async (dir, converter) =>
        {
            converter.Initialize(MakeConfig(new TaggingTranslator(), "ar"));

            await ConvertFile(converter, dir);

            Assert.Equal("ar", converter.Language);
        });
    }

    [Theory]
    [InlineData("de", "de")]
    [InlineData("pt-BR", "pt")]
    [InlineData("zho_Hans", "zh")]
    [InlineData("deu", "de")]
    [InlineData("", null)]
    [InlineData("Klingonese", null)]
    public void GetTwoLetterLanguageCode_MapsTranslatorCodes(string code, string? expected)
    {
        Assert.Equal(expected, BatchConverter.GetTwoLetterLanguageCode(code));
    }
}
