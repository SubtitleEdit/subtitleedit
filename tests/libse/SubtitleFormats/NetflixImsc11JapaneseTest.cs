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
