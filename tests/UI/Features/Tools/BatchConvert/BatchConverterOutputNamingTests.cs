using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Features.Tools.BatchConvert;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.UiLogic.AutoTranslate;
using Nikse.SubtitleEdit.UiLogic.BatchConvert;
using Nikse.SubtitleEdit.UiLogic.Translate;

namespace UITests.Features.Tools.BatchConvert;

/// <summary>
/// Pins the output naming rules of <see cref="BatchConverter"/>: two same-language tracks
/// from one file must never clobber each other (not even in overwrite mode), and
/// auto-translate names its output after the target language (#13707) - "way.zh.srt"
/// instead of the old "way_1.srt" collision rotation.
/// </summary>
public class BatchConverterOutputNamingTests
{
    private const string InputSrt = @"1
00:00:01,000 --> 00:00:03,000
Hello world.
";

    private sealed class EchoTranslator : IAutoTranslator
    {
        public string Name => "Echo";
        public string Url => string.Empty;
        public string Error { get; set; } = string.Empty;
        public int MaxCharacters => 1000;
        public void Initialize() { }
        public List<TranslationPair> GetSupportedSourceLanguages() => new() { new TranslationPair("English", "en") };
        public List<TranslationPair> GetSupportedTargetLanguages() => new() { new TranslationPair("Chinese", "zh-CN") };
        public Task<string> Translate(string text, string sourceLanguageCode, string targetLanguageCode, CancellationToken cancellationToken) => Task.FromResult(text);
    }

    private static async Task RunWithLanguagePostFix(string postFix, Func<DirectoryInfo, BatchConverter, Task> test)
    {
        var oldPostFix = Se.Settings.Tools.BatchConvert.LanguagePostFix;
        Se.Settings.Tools.BatchConvert.LanguagePostFix = postFix;
        var dir = Directory.CreateTempSubdirectory("se-batch-naming-test");
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

    [Fact]
    public async Task TwoSameLanguageTracks_OverwriteOn_SecondGetsTrackNumberName()
    {
        await RunWithLanguagePostFix(Se.Language.General.TwoLetterLanguageCode, async (dir, converter) =>
        {
            var inputFile = Path.Combine(dir.FullName, "video.mkv");
            await File.WriteAllTextAsync(inputFile, "x", TestContext.Current.CancellationToken);
            var outputFolder = Path.Combine(dir.FullName, "out");
            Directory.CreateDirectory(outputFolder);

            var config = new BatchConvertConfig
            {
                SaveInSourceFolder = false,
                OutputFolder = outputFolder,
                Overwrite = true,
                TargetFormatName = SubRip.NameOfFormat,
            };
            converter.Initialize(config);

            foreach (var trackNumber in new[] { "3", "4" })
            {
                var subtitle = new Subtitle();
                new SubRip().LoadSubtitle(subtitle, InputSrt.SplitToLines(), inputFile);
                var item = new BatchConvertItem(inputFile, 1, new SubRip().Name, subtitle)
                {
                    LanguageCode = "eng",
                    TrackNumber = trackNumber,
                };
                await converter.Convert(item, TestContext.Current.CancellationToken);
            }

            Assert.True(File.Exists(Path.Combine(outputFolder, "video.en.srt")), "first track's output is missing");
            Assert.True(File.Exists(Path.Combine(outputFolder, "video.#4.en.srt")), "second track's output is missing or was clobbered");
        });
    }

    [Fact]
    public async Task AutoTranslate_PlainSrt_IsNamedAfterTargetLanguage()
    {
        await RunWithLanguagePostFix(Se.Language.General.TwoLetterLanguageCode, async (dir, converter) =>
        {
            var inputFile = Path.Combine(dir.FullName, "way.srt");
            await File.WriteAllTextAsync(inputFile, InputSrt, TestContext.Current.CancellationToken);

            var config = new BatchConvertConfig
            {
                SaveInSourceFolder = true,
                Overwrite = false,
                TargetFormatName = SubRip.NameOfFormat,
            };
            config.AutoTranslate.IsActive = true;
            config.AutoTranslate.SourceLanguage = new TranslationPair("English", "en");
            config.AutoTranslate.TargetLanguage = new TranslationPair("Chinese", "zh-CN");
            config.AutoTranslate.Translator = new EchoTranslator();
            converter.Initialize(config);

            var subtitle = Subtitle.Parse(inputFile);
            var item = new BatchConvertItem(inputFile, new FileInfo(inputFile).Length, new SubRip().Name, subtitle);
            await converter.Convert(item, TestContext.Current.CancellationToken);

            // The old behavior collided with the input and rotated to "way_1.srt".
            Assert.True(File.Exists(Path.Combine(dir.FullName, "way.zh.srt")), "translated output is not named after the target language");
            Assert.False(File.Exists(Path.Combine(dir.FullName, "way_1.srt")), "translated output fell back to the old collision rotation");
        });
    }
}
