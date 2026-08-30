using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Features.Edit.MultipleReplace;
using Nikse.SubtitleEdit.Features.Options.Settings;
using Nikse.SubtitleEdit.Features.Tools.BatchConvert;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.UiLogic.BatchConvert;
using System;
using System.Diagnostics;

namespace UITests.Features.Tools.BatchConvert;

/// <summary>
/// Batch convert runs the Multiple replace rules unattended, so neither a saved rule that will
/// not compile nor one that backtracks catastrophically may take the run down with it. The
/// window learned this in #13534; the batch path had the same two holes: it built its regexes
/// with the indexer and no try/catch (one bad rule failed every file in the batch with an opaque
/// "parsing ..." status naming neither rule nor category), and with no match timeout at all.
/// </summary>
public class BatchConverterMultipleReplaceRegexTests : IDisposable
{
    private readonly ShortRegexTimeout _shortRegexTimeout = new();

    public void Dispose() => _shortRegexTimeout.Dispose();

    // 30 a's and no "b": "(a+)+b" has to try every way of splitting them before giving up.
    private const string EvilPattern = "(a+)+b";
    private static readonly string EvilLine = new string('a', 30) + "c";

    private const int MaxSeconds = 60;

    private static SeEditMultipleReplace.MultipleReplaceCategory Category(params MultipleReplaceRule[] rules)
    {
        var category = new SeEditMultipleReplace.MultipleReplaceCategory { Name = "test", IsActive = true };
        foreach (var rule in rules)
        {
            category.Rules.Add(rule);
        }

        return category;
    }

    private static MultipleReplaceRule Rule(string find, string replaceWith, MultipleReplaceType type) =>
        new() { Active = true, Find = find, ReplaceWith = replaceWith, Type = type };

    private static async Task<string> ConvertAsync(string inputText, SeEditMultipleReplace.MultipleReplaceCategory category)
    {
        Se.Settings.Edit.MultipleReplace.Categories.Clear();
        Se.Settings.Edit.MultipleReplace.Categories.Add(category);

        var dir = Directory.CreateTempSubdirectory("se-batch-multiple-replace-test");
        try
        {
            var inputFile = Path.Combine(dir.FullName, "input.srt");
            await File.WriteAllTextAsync(
                inputFile,
                "1\r\n00:00:01,000 --> 00:00:03,000\r\n" + inputText + "\r\n\r\n",
                TestContext.Current.CancellationToken);

            var outputFolder = Path.Combine(dir.FullName, "out");
            Directory.CreateDirectory(outputFolder);

            var subtitle = Subtitle.Parse(inputFile);
            var item = new BatchConvertItem(inputFile, new FileInfo(inputFile).Length, new SubRip().Name, subtitle);

            var config = new BatchConvertConfig
            {
                SaveInSourceFolder = false,
                OutputFolder = outputFolder,
                Overwrite = true,
                TargetFormatName = SubRip.NameOfFormat,
            };
            config.MultipleReplace.IsActive = true;

            var converter = new BatchConverter(null!, null!, null!);
            converter.Initialize(config);
            await converter.Convert(item, TestContext.Current.CancellationToken);

            var outputFile = Path.Combine(outputFolder, "input.srt");
            Assert.True(File.Exists(outputFile), "converted file was not written - status: " + item.Status);

            return Subtitle.Parse(outputFile).Paragraphs[0].Text;
        }
        finally
        {
            Se.Settings.Edit.MultipleReplace.Categories.Clear();
            dir.Delete(recursive: true);
        }
    }

    [Fact]
    public async Task InvalidRegexRule_DoesNotFailTheFile()
    {
        var text = await ConvertAsync(
            "The colour is red.",
            Category(
                Rule("(unclosed", string.Empty, MultipleReplaceType.RegularExpression),
                Rule("colour", "color", MultipleReplaceType.CaseInsensitive)));

        // The whole file used to come back as "Error: parsing '(unclosed' ..." with nothing applied.
        Assert.Equal("The color is red.", text);
    }

    // A skipped rule is only useful in the log if the log says which rule. RuleInfo is
    // "category: description", and the description is optional and not unique, so the pattern -
    // the one thing that identifies the rule in the user's list - has to be in there.
    [Theory]
    [InlineData("cat: strip dashes", "rule '(a+)+b' (cat: strip dashes)")]
    [InlineData("cat: ", "rule '(a+)+b' (cat)")]
    [InlineData("", "rule '(a+)+b'")]
    public void DescribeRule_AlwaysNamesThePattern(string ruleInfo, string expected)
    {
        var expression = new ReplaceExpression("(a+)+b", "x", ReplaceExpression.SearchTypeRegularExpression, ruleInfo);

        Assert.Equal(expected, BatchConverter.DescribeRule(expression));
    }

    [Fact]
    public async Task CatastrophicPattern_GivesUpAndLeavesTheRestOfTheRulesWorking()
    {
        var stopwatch = Stopwatch.StartNew();
        var text = await ConvertAsync(
            EvilLine + " colour",
            Category(
                Rule(EvilPattern, "x", MultipleReplaceType.RegularExpression),
                Rule("colour", "color", MultipleReplaceType.CaseInsensitive)));
        stopwatch.Stop();

        Assert.Equal(EvilLine + " color", text);
        Assert.True(stopwatch.Elapsed.TotalSeconds < MaxSeconds, $"the conversion took {stopwatch.Elapsed.TotalSeconds:0.0}s");
    }
}
