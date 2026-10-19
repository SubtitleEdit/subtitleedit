using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Features.Tools.BatchConvert;
using Nikse.SubtitleEdit.UiLogic.BatchConvert;

namespace UITests.Features.Tools.BatchConvert;

public class BatchConverterFixRightToLeftTests
{
    // "Fix right-to-left" is for Hebrew/Arabic subtitles only. Its default mode, "Reverse RTL
    // start/end", moves the punctuation of every line to the other end, so a user who left the box
    // ticked while converting English files got ".Hello world" in all of them. The function now
    // leaves a subtitle without any right-to-left letters alone.

    private const string EnglishSrt = @"1
00:00:01,000 --> 00:00:03,000
Hello world.

2
00:00:04,000 --> 00:00:06,000
- Is that you?
- Yes, it's me...
";

    private const string HebrewSrt = @"1
00:00:01,000 --> 00:00:03,000
Credits in English first.

2
00:00:04,000 --> 00:00:06,000
שלום עולם.
";

    private static async Task<Subtitle> RunConvertAsync(string srt, Action<BatchConvertConfig.RightToLeftSettings> configure)
    {
        var dir = Directory.CreateTempSubdirectory("se-fix-rtl-test");
        try
        {
            var inputFile = Path.Combine(dir.FullName, "input.srt");
            await File.WriteAllTextAsync(inputFile, srt);

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
            config.RightToLeft.IsActive = true;
            configure(config.RightToLeft);

            var converter = new BatchConverter(null!, null!, null!);
            converter.Initialize(config);
            await converter.Convert(item, CancellationToken.None);

            var outputFile = Path.Combine(outputFolder, "input.srt");
            Assert.True(File.Exists(outputFile), "converted file was not written");
            return Subtitle.Parse(outputFile);
        }
        finally
        {
            dir.Delete(recursive: true);
        }
    }

    [Fact]
    public async Task ReverseStartEnd_EnglishSubtitle_IsLeftAlone()
    {
        var result = await RunConvertAsync(EnglishSrt, rtl => rtl.ReverseStartEnd = true);

        Assert.Equal("Hello world.", result.Paragraphs[0].Text);
        Assert.Equal("- Is that you?" + Environment.NewLine + "- Yes, it's me...", result.Paragraphs[1].Text);
    }

    [Fact]
    public async Task FixViaUnicode_EnglishSubtitle_IsLeftAlone()
    {
        var result = await RunConvertAsync(EnglishSrt, rtl => rtl.FixViaUnicode = true);

        Assert.Equal("Hello world.", result.Paragraphs[0].Text);
    }

    [Fact]
    public async Task ReverseStartEnd_HebrewSubtitle_StillReversed()
    {
        // The RTL letters are only in the second line - the whole subtitle is checked, not just the start.
        var result = await RunConvertAsync(HebrewSrt, rtl => rtl.ReverseStartEnd = true);

        Assert.Equal(".Credits in English first", result.Paragraphs[0].Text);
        Assert.Equal(".שלום עולם", result.Paragraphs[1].Text);
    }

    [Fact]
    public async Task RemoveUnicode_EnglishSubtitle_StillRuns()
    {
        var srt = EnglishSrt.Replace("Hello world.", "‫Hello world.‬");
        var result = await RunConvertAsync(srt, rtl => rtl.RemoveUnicode = true);

        Assert.Equal("Hello world.", result.Paragraphs[0].Text);
    }
}
