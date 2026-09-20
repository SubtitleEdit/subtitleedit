using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;

namespace LibSETests.SubtitleFormats;

public class LambdaCapTest
{
    private static Subtitle Load(params string[] cueLines)
    {
        var lines = new List<string> { "Lambda字幕V4\tDF0+1\tSCENE\"和文標準\"", "" };
        lines.AddRange(cueLines);
        var subtitle = new Subtitle();
        new LambdaCap().LoadSubtitle(subtitle, lines, "test.cap");
        return subtitle;
    }

    private static string LoadText(params string[] cueLines)
    {
        var subtitle = Load(cueLines);
        Assert.Single(subtitle.Paragraphs);
        return subtitle.Paragraphs[0].Text;
    }

    private static string Save(string text)
    {
        var subtitle = new Subtitle();
        subtitle.Paragraphs.Add(new Paragraph(text, 0, 1000));
        subtitle.Renumber();
        return new LambdaCap().ToText(subtitle, "test");
    }

    [Fact]
    public void RubyBecomesJapaneseMarkup()
    {
        var text = LoadText("1\t01002619/01002902\t中国 ＠ルビ上［成都｜せいと］＠\t＠横下\t＠中頭");

        Assert.Equal("中国 <ruby-container><ruby-base>成都</ruby-base><ruby-text>せいと</ruby-text></ruby-container>", text);
    }

    [Fact]
    public void RubyBelowBecomesRubyTextAfter()
    {
        var text = LoadText("1\t01002619/01002902\t＠ルビ下［成都｜せいと］＠\t＠横下");

        Assert.Equal("<ruby-container><ruby-base>成都</ruby-base><ruby-text-after>せいと</ruby-text-after></ruby-container>", text);
    }

    [Fact]
    public void RubyHoldingOnlyAMarkerBecomesBouten()
    {
        var text = LoadText("1\t01002619/01002902\tでも＠ルビ上［誰｜↓］＠が恋しいのよ\t＠横下\t＠中頭");

        Assert.Equal("でも<bouten-dot-before>誰</bouten-dot-before>が恋しいのよ", text);
    }

    [Fact]
    public void TateChuYokoBecomesHorizontalDigit()
    {
        var text = LoadText("1\t01002619/01002902\t＠組［12］＠時間\t＠横下");

        Assert.Equal("<horizontalDigit>12</horizontalDigit>時間", text);
    }

    [Fact]
    public void InlineItalicIsDecoded()
    {
        var text = LoadText("1\t01002619/01002902\t＠斜３［ABC］＠ですね\t＠横下");

        Assert.Equal("<i>ABC</i>ですね", text);
    }

    [Fact]
    public void ItalicCodeAppliesToTheWholeCue()
    {
        var text = LoadText(
            "1\t01002619/01002902\t中国\t＠横下\t＠中頭\t＠斜３",
            "\t\t\t\t24時間 耐久洗礼式");

        Assert.Equal("<i>中国" + Environment.NewLine + "24時間 耐久洗礼式</i>", text);
    }

    [Fact]
    public void SecondLineIsKept()
    {
        var text = LoadText(
            "1\t01002619/01002902\tイエスの名において\t＠横下\t＠中頭",
            "\t\t\t\t洗礼しましょう");

        Assert.Equal("イエスの名において" + Environment.NewLine + "洗礼しましょう", text);
    }

    [Theory]
    [InlineData("＠横下\t＠中頭", "")]
    [InlineData("＠横下\t＠中央", "")]
    [InlineData("＠横上", "{\\an8}")]
    [InlineData("＠縦右\t＠行頭", "{\\an9}")]
    [InlineData("＠縦左\t＠行頭", "{\\an7}")]
    [InlineData("＠縦右", "{\\an6}")]
    [InlineData("＠縦左", "{\\an4}")]
    public void LayoutCodesBecomeAlignment(string codes, string expectedTag)
    {
        var text = LoadText("1\t01002619/01002902\tテスト\t" + codes);

        Assert.Equal(expectedTag + "テスト", text);
    }

    [Fact]
    public void UnknownControlCodeIsDropped()
    {
        var text = LoadText("1\t01002619/01002902\tテスト\t＠横下\t＠謎コード");

        Assert.Equal("テスト", text);
    }

    [Fact]
    public void LayoutCodesWrittenBehindTheTextAreStillRead()
    {
        // Older Subtitle Edit versions wrote the codes space separated instead of in their own fields.
        var text = LoadText("1\t01002619/01002902\tテスト ＠縦左 ＠行頭");

        Assert.Equal("{\\an7}テスト", text);
    }

    [Fact]
    public void WideDashIsDecoded()
    {
        var text = LoadText("1\t01002619/01002902\t＠幅広［―］＠そう\t＠横下");

        Assert.Equal("―そう", text);
    }

    [Fact]
    public void SaveWritesControlCodesInTheirOwnFields()
    {
        var text = Save("中国 <ruby-container><ruby-base>成都</ruby-base><ruby-text>せいと</ruby-text></ruby-container>");

        Assert.Contains("\t中国 ＠ルビ上［成都｜せいと］＠\t＠横下\t＠中頭", text);
    }

    [Fact]
    public void SaveWritesItalicAsACueLevelCode()
    {
        var text = Save("<i>中国" + Environment.NewLine + "24時間</i>");

        Assert.Contains("\t中国\t＠横下\t＠中頭\t＠斜３" + Environment.NewLine + "\t\t\t\t24時間", text);
    }

    [Fact]
    public void SaveKeepsProlongedSoundMark()
    {
        // "ー" is a normal letter inside katakana words - only the horizontal bar is a wide dash.
        var text = Save("スムーズ");

        Assert.Contains("スムーズ", text);
        Assert.DoesNotContain("幅広", text);
    }

    [Fact]
    public void SaveAndLoadRoundTrip()
    {
        var expected = "{\\an9}<i>〝ケルビン" + Environment.NewLine +
                       "でも<bouten-dot-before>誰</bouten-dot-before> " +
                       "<ruby-container><ruby-base>成都</ruby-base><ruby-text>せいと</ruby-text></ruby-container>" +
                       "<horizontalDigit>12</horizontalDigit></i>";

        var saved = Save(expected);
        var reloaded = new Subtitle();
        new LambdaCap().LoadSubtitle(reloaded, saved.SplitToLines(), "test.cap");

        Assert.Single(reloaded.Paragraphs);
        Assert.Equal(expected, reloaded.Paragraphs[0].Text);
    }

    [Fact]
    public void ConvertingToNetflixImsc11JapaneseKeepsRubyAndTateChuYoko()
    {
        var subtitle = Load("1\t01002619/01002902\t＠ルビ上［炉心溶融｜メルトダウン］＠を起こしかけている\t＠横下\t＠中頭",
                            "2\t01003000/01003200\t＠組［２０１２］＠年10月５日\t＠縦右");
        var imsc = new NetflixImsc11Japanese();

        new LambdaCap().RemoveNativeFormatting(subtitle, imsc);
        var xml = System.Text.RegularExpressions.Regex.Replace(imsc.ToText(subtitle, "test"), @">\s+<", "><");

        Assert.Contains("<span style=\"ruby-container\"><span style=\"ruby-base\">炉心溶融</span><span style=\"ruby-text\">メルトダウン</span></span>", xml);
        Assert.Contains("<span style=\"horizontalDigit\">２０１２</span>", xml);
    }

    [Theory]
    [InlineData("＠縦右", "right")]
    [InlineData("＠縦左", "left")]
    [InlineData("＠縦右\t＠行頭", "right")]
    [InlineData("＠縦左\t＠行頭", "left")]
    public void VerticalCueIsExportedToVerticalRegion(string codes, string expectedRegion)
    {
        var subtitle = Load("1\t01002619/01002902\tテスト\t" + codes);

        var xml = new NetflixImsc11Japanese().ToText(subtitle, "test");

        Assert.Contains("region=\"" + expectedRegion + "\"", xml);
    }

    [Theory]
    [InlineData("＠縦右")]
    [InlineData("＠縦左")]
    [InlineData("＠縦右\t＠行頭")]
    [InlineData("＠縦左\t＠行頭")]
    public void VerticalCodesRoundTrip(string codes)
    {
        var saved = Save(LoadText("1\t01002619/01002902\tテスト\t" + codes));

        Assert.Contains("テスト\t" + codes + Environment.NewLine, saved);
    }

    [Fact]
    public void ConvertingFromNetflixImsc11JapaneseKeepsRuby()
    {
        var subtitle = new Subtitle();
        subtitle.Paragraphs.Add(new Paragraph("<ruby-container><ruby-base>成都</ruby-base><ruby-text>せいと</ruby-text></ruby-container>", 0, 1000));
        subtitle.Renumber();

        new NetflixImsc11Japanese().RemoveNativeFormatting(subtitle, new LambdaCap());
        var text = new LambdaCap().ToText(subtitle, "test");

        Assert.Contains("＠ルビ上［成都｜せいと］＠", text);
    }

    [Fact]
    public void ConvertingToPlainTextFormatStillRemovesMarkup()
    {
        var subtitle = Load("1\t01002619/01002902\t＠ルビ上［成都｜せいと］＠\t＠横下");

        new LambdaCap().RemoveNativeFormatting(subtitle, new SubRip());

        Assert.Equal("成都せいと", subtitle.Paragraphs[0].Text);
    }

    [Fact]
    public void HeaderFieldsAreTabSeparated()
    {
        var text = Save("テスト");

        Assert.StartsWith("Lambda字幕V4\t", text);
    }
}
