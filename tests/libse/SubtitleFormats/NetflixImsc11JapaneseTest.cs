using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;

namespace LibSETests.SubtitleFormats;

public class NetflixImsc11JapaneseTest
{
    private static SubtitleFormat? Detect(string raw)
    {
        var lines = raw.SplitToLines();
        foreach (var candidate in SubtitleFormat.AllSubtitleFormats)
        {
            if (candidate.IsMine(lines, "subtitle.xml"))
            {
                return candidate;
            }
        }

        return null;
    }

    // Styling block from https://github.com/SubtitleEdit/subtitleedit/issues/13836 -
    // a real Netflix IMSC 1.1 Japanese file with ruby/furigana styles but no bouten
    // (emphasis) styles. It must not be claimed by EBU-TT-D.
    private const string RubyOnlyDocument = """
<?xml version="1.0" encoding="UTF-8" standalone="no"?>
<tt xml:lang="ja" xmlns="http://www.w3.org/ns/ttml" xmlns:tts="http://www.w3.org/ns/ttml#styling" xmlns:ttp="http://www.w3.org/ns/ttml#parameter" xmlns:ebutts="urn:ebu:tt:style" ttp:timeBase="media" ttp:contentProfiles="http://www.w3.org/ns/ttml/profile/imsc1.1/text">
  <head>
    <styling>
      <initial tts:backgroundColor="transparent" tts:color="white" tts:extent="80.000% 80.000%" tts:fontFamily="Japanese" tts:opacity="1.000" tts:origin="10.000% 10.000%" tts:showBackground="whenActive" tts:textOutline="black 0.050em" tts:writingMode="lrtb"/>
      <style xml:id="style0" tts:fontSize="100.000%" tts:rubyReserve="outside" tts:textAlign="center"/>
      <style xml:id="style2" tts:fontSize="100.000%" tts:ruby="container"/>
      <style xml:id="style3" tts:fontSize="100.000%" tts:ruby="base"/>
      <style xml:id="style4" tts:ruby="text" tts:rubyAlign="center" tts:rubyPosition="outside"/>
    </styling>
    <layout>
      <region xml:id="region0" tts:displayAlign="after" tts:extent="80.000% 80.000%" tts:origin="10.000% 10.000%"/>
    </layout>
  </head>
  <body>
    <div>
      <p begin="00:00:01.000" end="00:00:03.000" region="region0" style="style0">こんにちは</p>
    </div>
  </body>
</tt>
""";

    [Fact]
    public void DetectsRubyOnlyDocumentAsNetflixImsc11Japanese()
    {
        var detected = Detect(RubyOnlyDocument);

        Assert.NotNull(detected);
        Assert.Equal(new NetflixImsc11Japanese().Name, detected.Name);
    }

    // The real Netflix document from https://github.com/SubtitleEdit/subtitleedit/issues/13861 -
    // generated style/region ids ("style0", "region1"), so nothing can be matched by name: ruby,
    // vertical writing and the shear all have to be read off the attributes.
    private const string GeneratedIdsDocument = """
<?xml version="1.0" encoding="utf-8"?>
<tt ttp:contentProfiles="http://www.w3.org/ns/ttml/profile/imsc1.1/text" xmlns="http://www.w3.org/ns/ttml" xmlns:ebutts="urn:ebu:tt:style" xmlns:ttp="http://www.w3.org/ns/ttml#parameter" xmlns:tts="http://www.w3.org/ns/ttml#styling" ttp:tickRate="10000000" ttp:timeBase="media" xml:lang="ja">
<head>
<styling>
<initial tts:backgroundColor="transparent" tts:color="white" tts:extent="80.000% 80.000%" tts:fontFamily="Japanese" tts:origin="10.000% 10.000%" tts:writingMode="lrtb"/>
<style xml:id="style0" tts:fontSize="100.000%" tts:rubyReserve="outside" tts:textAlign="center"/>
<style xml:id="style1" tts:fontSize="100.000%" tts:rubyReserve="outside"/>
<style xml:id="style2" tts:fontSize="100.000%" tts:ruby="container"/>
<style xml:id="style3" tts:fontSize="100.000%" tts:ruby="base"/>
<style xml:id="style4" tts:ruby="text" tts:rubyAlign="center" tts:rubyPosition="outside"/>
<style xml:id="style5" tts:fontSize="100.000%" tts:rubyReserve="outside" tts:shear="16.670%" tts:textAlign="center"/>
<style xml:id="style6" tts:fontSize="100.000%" tts:rubyReserve="outside" tts:shear="16.670%"/>
<style xml:id="style7" tts:fontSize="100.000%" tts:textEmphasis="filled sesame outside"/>
<style xml:id="style10" tts:fontSize="100.000%" tts:rubyReserve="outside" tts:textAlign="start"/>
</styling>
<layout>
<region xml:id="region0" ebutts:multiRowAlign="start" tts:displayAlign="after" tts:extent="80.000% 80.000%" tts:origin="10.000% 10.000%" tts:writingMode="lrtb"/>
<region xml:id="region1" ebutts:multiRowAlign="start" tts:displayAlign="before" tts:extent="80.000% 80.000%" tts:origin="10.000% 10.000%" tts:writingMode="tbrl"/>
<region xml:id="region2" ebutts:multiRowAlign="start" tts:displayAlign="after" tts:extent="80.000% 80.000%" tts:origin="10.000% 10.000%" tts:writingMode="tbrl"/>
<region xml:id="region3" tts:displayAlign="before" tts:extent="80.000% 20.000%" tts:origin="10.000% 5.000%"/>
</layout>
</head>
<body>
<div>
<p begin="00:00:01.000" end="00:00:03.000" region="region0" style="style0"><span style="style1">（バシン）<span style="style2"><span style="style3">遅</span><span style="style4">おく</span></span>れた～</span></p>
<p begin="00:00:04.000" end="00:00:06.000" region="region1" style="style10"><span style="style1">（兵士）ハッ…</span></p>
<p begin="00:00:07.000" end="00:00:09.000" region="region0" style="style5"><span style="style6">（ナレーション）</span><br/>ついに 決戦の時は来た</p>
<p begin="00:00:10.000" end="00:00:12.000" region="region2" style="style0">下から</p>
<p begin="00:00:13.000" end="00:00:15.000" region="region3" style="style0"><span style="style7">強調</span></p>
</div>
</body>
</tt>
""";

    private static Subtitle LoadGeneratedIdsDocument()
    {
        var lines = GeneratedIdsDocument.SplitToLines();
        var subtitle = new Subtitle();
        new NetflixImsc11Japanese().LoadSubtitle(subtitle, lines, "subtitle.xml");
        return subtitle;
    }

    [Fact]
    public void ReadsRubyFromGeneratedStyleIds()
    {
        var subtitle = LoadGeneratedIdsDocument();

        Assert.Equal("（バシン）<ruby-container><ruby-base>遅</ruby-base><ruby-text>おく</ruby-text></ruby-container>れた～", subtitle.Paragraphs[0].Text);
    }

    [Fact]
    public void ReadsVerticalRegionAsCornerAlignment()
    {
        var subtitle = LoadGeneratedIdsDocument();

        // tbrl stacks columns right to left, so displayAlign "before" is the right hand side...
        Assert.StartsWith(@"{\an9}", subtitle.Paragraphs[1].Text);

        // ...and "after" is the left hand side.
        Assert.StartsWith(@"{\an7}", subtitle.Paragraphs[3].Text);
    }

    [Fact]
    public void ReadsShearOnParagraphStyleAsItalic()
    {
        var subtitle = LoadGeneratedIdsDocument();

        // The shear sits on the <p> style, so it has to reach the bare text after the <br/> too.
        Assert.Equal("<i>（ナレーション）</i>" + Environment.NewLine + "<i>ついに 決戦の時は来た</i>", subtitle.Paragraphs[2].Text);
    }

    [Fact]
    public void ReadsTextEmphasisAsBouten()
    {
        var subtitle = LoadGeneratedIdsDocument();

        Assert.Equal(@"{\an8}<bouten-filled-sesame-outside>強調</bouten-filled-sesame-outside>", subtitle.Paragraphs[4].Text);
    }

    [Fact]
    public void DoesNotInventFontTagsFromInitialStyles()
    {
        var subtitle = LoadGeneratedIdsDocument();

        // "initial" carries a document wide color and font family - they are the defaults, not a
        // per span override, so they must not turn into <font> tags around every line.
        Assert.All(subtitle.Paragraphs, p => Assert.DoesNotContain("<font", p.Text, StringComparison.Ordinal));
    }

    [Fact]
    public void KeepsOwnStyleNamesWhenRoundTripping()
    {
        var sub = new Subtitle();
        sub.Paragraphs.Add(new Paragraph("<ruby-container><ruby-base>私</ruby-base><ruby-text-after>わたし</ruby-text-after></ruby-container>は<i>元気</i>", 1000, 3000));
        var raw = sub.ToText(new NetflixImsc11Japanese());

        var reloaded = new Subtitle();
        new NetflixImsc11Japanese().LoadSubtitle(reloaded, raw.SplitToLines(), "subtitle.xml");

        Assert.Equal(sub.Paragraphs[0].Text, reloaded.Paragraphs[0].Text);
    }

    [Fact]
    public void GeneratedIdsDocumentSurvivesAllTheWayToTheAssPreview()
    {
        var dialogue = NetflixImsc11JapaneseToAss.Convert(LoadGeneratedIdsDocument(), 1280, 720)
            .SplitToLines()
            .Where(l => l.StartsWith("Dialogue:", StringComparison.Ordinal))
            .ToList();

        // The reading gets its own render line above the base...
        Assert.Contains(dialogue, l => l.Contains("{\\fs20}おく", StringComparison.Ordinal));

        // ...the vertical cue is stacked one character per line with vertical brackets...
        Assert.Contains(dialogue, l => l.Contains(@"{\an9\pos(", StringComparison.Ordinal) && l.EndsWith(@"︵\N兵\N士\N︶\Nハ\Nッ\N⋮", StringComparison.Ordinal));

        // ...and the sheared paragraph is italic on both of its lines.
        Assert.Equal(2, dialogue.Count(l => l.Contains("{\\i1}", StringComparison.Ordinal)));

        // Nothing the video player cannot draw is left in the text.
        Assert.DoesNotContain(dialogue, l => l.Contains("<ruby", StringComparison.Ordinal) || l.Contains("<bouten", StringComparison.Ordinal));
    }

    [Fact]
    public void OwnOutputStillDetectsAsNetflixImsc11Japanese()
    {
        var sub = new Subtitle();
        sub.Paragraphs.Add(new Paragraph("こんにちは", 1000, 3000));
        var raw = sub.ToText(new NetflixImsc11Japanese());

        var detected = Detect(raw);

        Assert.NotNull(detected);
        Assert.Equal(new NetflixImsc11Japanese().Name, detected.Name);
    }
}
