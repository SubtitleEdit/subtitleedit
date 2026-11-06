using Nikse.SubtitleEdit.Core.Common;

namespace LibSETests.Core;

public class HtmFixCasingTestlUtilTest
{
    [Fact]
    public void FixStutter1()
    {
        var fc = new FixCasing("en");
        var s = new Subtitle(new List<Paragraph> { new Paragraph("W-what time is it? W-what time is it?", 0, 2000) });
        fc.FixMakeLowercase = false;
        fc.FixMakeUppercase = false;
        fc.FixNormal = true;
        fc.Fix(s);
        Assert.Equal("W-What time is it? W-What time is it?", s.Paragraphs[0].Text);
    }

    [Fact]
    public void FixStutter2()
    {
        var fc = new FixCasing("en");
        var s = new Subtitle(new List<Paragraph> { new Paragraph("W-w-what time is it? W-w-what time is it?", 0, 2000) });
        fc.FixMakeLowercase = false;
        fc.FixMakeUppercase = false;
        fc.FixNormal = true;
        fc.Fix(s);
        Assert.Equal("W-W-What time is it? W-W-What time is it?", s.Paragraphs[0].Text);
    }

    [Fact]
    public void FixMakeUppercaseSharpS()
    {
        var fc = new FixCasing("de");
        var s = new Subtitle(new List<Paragraph> { new Paragraph("Es ist <i>hei\u00DF</i>.", 0, 2000) });
        fc.FixNormal = false;
        fc.FixMakeLowercase = false;
        fc.FixMakeUppercase = true;
        fc.Fix(s);
        Assert.Equal("ES IST <i>HEI\u1E9E</i>.", s.Paragraphs[0].Text);
    }
}
