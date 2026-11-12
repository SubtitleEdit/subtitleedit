using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Features.Tools.BatchConvert;
using Nikse.SubtitleEdit.Features.Tools.MergeTwoSubtitles;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.UiLogic.AutoTranslate;
using Nikse.SubtitleEdit.UiLogic.BatchConvert;
using Nikse.SubtitleEdit.UiLogic.Translate;

namespace UITests.Features.Tools.BatchConvert;

/// <summary>
/// "Merge translation with original": after auto-translate each output file holds the original
/// text and the translation, like Tools -> Merge two subtitles does for two files - each in its
/// own style in Advanced Sub Station Alpha, the translation below the original text otherwise.
/// </summary>
public class BatchConverterMergeTranslationWithOriginalTests
{
    private const string InputSrt = @"1
00:00:01,000 --> 00:00:03,000
Hello world.

2
00:00:04,000 --> 00:00:06,000
How are you?
";

    private const string TemplateHeader = @"[Script Info]
ScriptType: v4.00+
PlayResX: 640
PlayResY: 360

[V4+ Styles]
Format: Name,Fontname,Fontsize,PrimaryColour,SecondaryColour,OutlineColour,BackColour,Bold,Italic,Underline,Strikeout,ScaleX,ScaleY,Spacing,Angle,BorderStyle,Outline,Shadow,Alignment,MarginL,MarginR,MarginV,Encoding
Style: Default,Trebuchet MS,24,&H00FFFFFF,&H000000FF,&H00000000,&H00000000,0,0,0,0,100,100,0,0,1,2,1,2,0010,0010,0015,1";

    /// <summary>Prefixes every line with the target code - several subtitle lines can come in one call.</summary>
    private sealed class TaggingTranslator : IAutoTranslator
    {
        public string Name => "Tagging";
        public string Url => string.Empty;
        public string Error { get; set; } = string.Empty;
        public int MaxCharacters => 1000;
        public void Initialize() { }
        public List<TranslationPair> GetSupportedSourceLanguages() => new() { new TranslationPair("English", "en") };
        public List<TranslationPair> GetSupportedTargetLanguages() => new();

        public Task<string> Translate(string text, string sourceLanguageCode, string targetLanguageCode, CancellationToken cancellationToken)
        {
            var lines = text.SplitToLines().Select(line => line.Trim() is "" or "." ? line : $"[{targetLanguageCode}] {line}");
            return Task.FromResult(string.Join(Environment.NewLine, lines));
        }
    }

    private static BatchConvertConfig MakeConfig(string targetFormatName, string outputFolder, params string[] targets)
    {
        var config = new BatchConvertConfig
        {
            SaveInSourceFolder = false,
            OutputFolder = outputFolder,
            Overwrite = true,
            TargetFormatName = targetFormatName,
        };
        config.AutoTranslate.IsActive = true;
        config.AutoTranslate.SourceLanguage = new TranslationPair("English", "en");
        config.AutoTranslate.TargetLanguage = new TranslationPair(targets[0], targets[0]);
        config.AutoTranslate.ExtraTargetLanguages = targets.Skip(1).Select(p => new TranslationPair(p, p)).ToList();
        config.AutoTranslate.Translator = new TaggingTranslator();
        config.MergeTranslationWithOriginal.IsActive = true;
        return config;
    }

    /// <summary>Converts one input file and returns the output files by file name.</summary>
    private static async Task<Dictionary<string, Subtitle>> Convert(
        BatchConverter converter,
        Func<string, BatchConvertConfig> makeConfig,
        string inputFileName = "movie.srt",
        string input = InputSrt)
    {
        var oldPostFix = Se.Settings.Tools.BatchConvert.LanguagePostFix;
        Se.Settings.Tools.BatchConvert.LanguagePostFix = Se.Language.General.TwoLetterLanguageCode;
        var dir = Directory.CreateTempSubdirectory("se-batch-merge-translation-test");
        try
        {
            var outputFolder = Path.Combine(dir.FullName, "out");
            Directory.CreateDirectory(outputFolder);
            converter.Initialize(makeConfig(outputFolder));

            var inputFile = Path.Combine(dir.FullName, inputFileName);
            await File.WriteAllTextAsync(inputFile, input, TestContext.Current.CancellationToken);
            var subtitle = Subtitle.Parse(inputFile);
            var item = new BatchConvertItem(inputFile, new FileInfo(inputFile).Length, subtitle.OriginalFormat.Name, subtitle);
            await converter.Convert(item, TestContext.Current.CancellationToken);

            return Directory.GetFiles(outputFolder).ToDictionary(p => Path.GetFileName(p), p => Subtitle.Parse(p));
        }
        finally
        {
            Se.Settings.Tools.BatchConvert.LanguagePostFix = oldPostFix;
            dir.Delete(recursive: true);
        }
    }

    private static List<string> TextsInStyle(Subtitle subtitle, string styleName) =>
        subtitle.Paragraphs
            .Where(p => p.Extra == styleName)
            .OrderBy(p => p.StartTime.TotalMilliseconds)
            .Select(p => p.Text)
            .ToList();

    [Fact]
    public async Task Assa_OriginalAndTranslationInOwnStyles()
    {
        var files = await Convert(new BatchConverter(null!, null!, null!), outputFolder =>
        {
            var config = MakeConfig(AdvancedSubStationAlpha.NameOfFormat, outputFolder, "de");
            config.MergeTranslationWithOriginal.StyleOriginal.FontName = "Verdana";
            config.MergeTranslationWithOriginal.StyleTranslation.FontSize = 30;
            return config;
        });

        var (fileName, subtitle) = Assert.Single(files);
        Assert.Equal("movie.de.ass", fileName);

        var styles = AdvancedSubStationAlpha.GetSsaStylesFromHeader(subtitle.Header);
        Assert.Equal(2, styles.Count);
        var original = styles.Single(p => p.Name == MergeTwoSubtitlesHelper.StyleName1);
        var translation = styles.Single(p => p.Name == MergeTwoSubtitlesHelper.StyleName2);
        Assert.Equal("Verdana", original.FontName);
        Assert.Equal("2", original.Alignment);
        Assert.Equal(30, translation.FontSize);
        Assert.Equal("8", translation.Alignment);

        Assert.Equal(new[] { "Hello world.", "How are you?" }, TextsInStyle(subtitle, MergeTwoSubtitlesHelper.StyleName1));
        Assert.Equal(new[] { "[de] Hello world.", "[de] How are you?" }, TextsInStyle(subtitle, MergeTwoSubtitlesHelper.StyleName2));
    }

    [Fact]
    public async Task SubRip_TranslationAboveOriginalInSameLine()
    {
        var converter = new BatchConverter(null!, null!, null!);

        var files = await Convert(converter, outputFolder => MakeConfig(SubRip.NameOfFormat, outputFolder, "de"));

        var (fileName, subtitle) = Assert.Single(files);
        Assert.Equal("movie.de.srt", fileName);
        Assert.Equal(2, subtitle.Paragraphs.Count);
        Assert.Equal(new[] { "[de] Hello world.", "Hello world." }, subtitle.Paragraphs[0].Text.SplitToLines());
        Assert.Equal(new[] { "[de] How are you?", "How are you?" }, subtitle.Paragraphs[1].Text.SplitToLines());
        Assert.Equal(1000, subtitle.Paragraphs[0].StartTime.TotalMilliseconds);

        // The original text is processed with the source language's rules - the language must
        // be the target's again afterwards.
        Assert.Equal("de", converter.Language);
    }

    [Fact]
    public async Task SubRip_OriginalSetToTop_IsTheFirstLine()
    {
        var files = await Convert(new BatchConverter(null!, null!, null!), outputFolder =>
        {
            var config = MakeConfig(SubRip.NameOfFormat, outputFolder, "de");
            config.MergeTranslationWithOriginal.StyleOriginal.Alignment = "8";
            config.MergeTranslationWithOriginal.StyleTranslation.Alignment = "2";
            return config;
        });

        var (_, subtitle) = Assert.Single(files);
        Assert.Equal(new[] { "Hello world.", "[de] Hello world." }, subtitle.Paragraphs[0].Text.SplitToLines());
    }

    [Fact]
    public async Task SeveralTargetLanguages_EachFileHasOriginalAndItsTranslation()
    {
        var files = await Convert(new BatchConverter(null!, null!, null!), outputFolder => MakeConfig(SubRip.NameOfFormat, outputFolder, "de", "fr"));

        Assert.Equal(2, files.Count);
        Assert.Equal(new[] { "[de] Hello world.", "Hello world." }, files["movie.de.srt"].Paragraphs[0].Text.SplitToLines());
        Assert.Equal(new[] { "[fr] Hello world.", "Hello world." }, files["movie.fr.srt"].Paragraphs[0].Text.SplitToLines());
    }

    [Fact]
    public async Task OffsetTimeCodes_MovesOriginalAndTranslation()
    {
        var files = await Convert(new BatchConverter(null!, null!, null!), outputFolder =>
        {
            var config = MakeConfig(AdvancedSubStationAlpha.NameOfFormat, outputFolder, "de");
            config.OffsetTimeCodes.IsActive = true;
            config.OffsetTimeCodes.Forward = true;
            config.OffsetTimeCodes.Milliseconds = 500;
            return config;
        });

        var subtitle = files["movie.de.ass"];
        Assert.Equal(4, subtitle.Paragraphs.Count);
        Assert.Equal(new[] { 1500d, 1500d, 4500d, 4500d }, subtitle.Paragraphs.Select(p => p.StartTime.TotalMilliseconds).Order());
    }

    [Fact]
    public async Task AssaHeaderTemplate_DoesNotReplaceTheTwoStyles()
    {
        var files = await Convert(new BatchConverter(null!, null!, null!), outputFolder =>
        {
            var config = MakeConfig(AdvancedSubStationAlpha.NameOfFormat, outputFolder, "de");
            config.AssaUseSourceStylesIfPossible = false;
            config.AssaHeader = TemplateHeader;
            return config;
        });

        var styles = AdvancedSubStationAlpha.GetSsaStylesFromHeader(files["movie.de.ass"].Header);
        Assert.Equal(new[] { MergeTwoSubtitlesHelper.StyleName1, MergeTwoSubtitlesHelper.StyleName2 }, styles.Select(p => p.Name).Order());
        Assert.DoesNotContain(styles, p => p.FontName == "Trebuchet MS");
    }

    [Fact]
    public async Task WebVttToAssa_SourceFormattingIsConvertedInBothParts()
    {
        const string inputVtt = @"WEBVTT

00:00:01.000 --> 00:00:03.000
<c.loud>Hello</c> world.
";

        var files = await Convert(
            new BatchConverter(null!, null!, null!),
            outputFolder => MakeConfig(AdvancedSubStationAlpha.NameOfFormat, outputFolder, "de"),
            "movie.vtt",
            inputVtt);

        var subtitle = files["movie.de.ass"];
        Assert.Equal(new[] { "Hello world." }, TextsInStyle(subtitle, MergeTwoSubtitlesHelper.StyleName1));
        Assert.Equal(new[] { "[de] Hello world." }, TextsInStyle(subtitle, MergeTwoSubtitlesHelper.StyleName2));
    }

    [Fact]
    public async Task WithoutAutoTranslate_NothingIsMerged()
    {
        var files = await Convert(new BatchConverter(null!, null!, null!), outputFolder =>
        {
            var config = MakeConfig(SubRip.NameOfFormat, outputFolder, "de");
            config.AutoTranslate.IsActive = false;
            return config;
        });

        var subtitle = Assert.Single(files).Value;
        Assert.Equal(new[] { "Hello world.", "How are you?" }, subtitle.Paragraphs.Select(p => p.Text));
    }
}
