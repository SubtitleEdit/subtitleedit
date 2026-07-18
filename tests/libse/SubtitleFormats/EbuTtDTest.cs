using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;

namespace LibSETests.SubtitleFormats;

public class EbuTtDTest
{
    private static Subtitle RoundTrip(Subtitle subtitle, out string raw)
    {
        var format = new EbuTtD();
        raw = subtitle.ToText(format);
        var result = new Subtitle();
        format.LoadSubtitle(result, raw.SplitToLines(), null);
        return result;
    }

    [Fact]
    public void BasicRoundTrip()
    {
        var input = "Hello world!" + Environment.NewLine + "Second line.";
        var sub = new Subtitle();
        sub.Paragraphs.Add(new Paragraph(input, 1000, 3520));

        var result = RoundTrip(sub, out _);

        Assert.Single(result.Paragraphs);
        Assert.Equal(input, result.Paragraphs[0].Text);
        Assert.Equal(1000, result.Paragraphs[0].StartTime.TotalMilliseconds);
        Assert.Equal(3520, result.Paragraphs[0].EndTime.TotalMilliseconds);
    }

    [Fact]
    public void ItalicRoundTrip()
    {
        var input = "This is an <i>italic</i> word!";
        var sub = new Subtitle();
        sub.Paragraphs.Add(new Paragraph(input, 0, 3000));

        var result = RoundTrip(sub, out _);

        Assert.Single(result.Paragraphs);
        Assert.Equal(input, result.Paragraphs[0].Text);
    }

    [Fact]
    public void WholeLineItalicRoundTrip()
    {
        var input = "<i>All italic here</i>";
        var sub = new Subtitle();
        sub.Paragraphs.Add(new Paragraph(input, 0, 3000));

        var result = RoundTrip(sub, out _);

        Assert.Single(result.Paragraphs);
        Assert.Equal(input, result.Paragraphs[0].Text);
    }

    [Fact]
    public void TopAlignmentRoundTrip()
    {
        var input = "{\\an8}Top text";
        var sub = new Subtitle();
        sub.Paragraphs.Add(new Paragraph(input, 0, 2000));

        var result = RoundTrip(sub, out var raw);

        Assert.Contains("region=\"top\"", raw);
        Assert.Single(result.Paragraphs);
        Assert.Equal(input, result.Paragraphs[0].Text);
    }

    [Fact]
    public void OutputIsConformantShape()
    {
        var sub = new Subtitle();
        sub.Paragraphs.Add(new Paragraph("Hi & bye <i>now</i>", 0, 2000));

        RoundTrip(sub, out var raw);

        // Load-bearing EBU-TT-D bits: conformance urn, media timebase, cell resolution,
        // line padding style attribute, all text inside spans, HH:MM:SS.mmm times.
        Assert.Contains("urn:ebu:tt:distribution:2018-04", raw);
        Assert.Contains("ttp:timeBase=\"media\"", raw);
        Assert.Contains("ttp:cellResolution=\"32 15\"", raw);
        Assert.Contains("ebutts:linePadding", raw);
        Assert.Contains("begin=\"00:00:00.000\"", raw);
        Assert.Contains("end=\"00:00:02.000\"", raw);
        Assert.Contains("<span style=\"textStyle\">Hi &amp; bye </span>", raw);
        Assert.Contains("<span style=\"textStyle italicStyle\">now</span>", raw);
        Assert.DoesNotContain("xmlns=\"\"", raw);
    }

    [Fact]
    public void DetectsAsEbuTtDNotTimedText()
    {
        var sub = new Subtitle();
        sub.Paragraphs.Add(new Paragraph("Detection test", 0, 2000));
        RoundTrip(sub, out var raw);

        // First format whose IsMine claims the document wins - EbuTtD must beat the
        // generic TTML formats for its own output.
        var lines = raw.SplitToLines();
        SubtitleFormat? detected = null;
        foreach (var candidate in SubtitleFormat.AllSubtitleFormats)
        {
            if (candidate.IsMine(lines, null))
            {
                detected = candidate;
                break;
            }
        }

        Assert.NotNull(detected);
        Assert.Equal(new EbuTtD().Name, detected.Name);
    }

    [Fact]
    public void LoadsExternalSample()
    {
        // Typical broadcaster-style document: prefixed elements, multiple style refs,
        // nested spans, region defined top.
        var raw = """
            <?xml version="1.0" encoding="UTF-8"?>
            <tt:tt xmlns:tt="http://www.w3.org/ns/ttml" xmlns:tts="http://www.w3.org/ns/ttml#styling"
                   xmlns:ttp="http://www.w3.org/ns/ttml#parameter" xmlns:ebuttm="urn:ebu:tt:metadata"
                   xmlns:ebutts="urn:ebu:tt:style" ttp:timeBase="media" ttp:cellResolution="32 15" xml:lang="de">
              <tt:head>
                <tt:metadata>
                  <ebuttm:documentMetadata>
                    <ebuttm:conformsToStandard>urn:ebu:tt:distribution:2014-01</ebuttm:conformsToStandard>
                  </ebuttm:documentMetadata>
                </tt:metadata>
                <tt:styling>
                  <tt:style xml:id="s0" tts:fontFamily="Verdana, Arial" tts:fontSize="160%"/>
                  <tt:style xml:id="s1" tts:color="#FFFFFF" tts:backgroundColor="#000000c2" ebutts:linePadding="0.5c"/>
                  <tt:style xml:id="s2" tts:fontStyle="italic"/>
                </tt:styling>
                <tt:layout>
                  <tt:region xml:id="r0" tts:origin="10% 10%" tts:extent="80% 80%" tts:displayAlign="after"/>
                </tt:layout>
              </tt:head>
              <tt:body tts:textAlign="center" style="s0">
                <tt:div>
                  <tt:p xml:id="sub0" begin="00:00:01.240" end="00:00:03.120" region="r0">
                    <tt:span style="s1">Erste Zeile</tt:span>
                    <tt:br/>
                    <tt:span style="s1 s2">kursive zweite Zeile</tt:span>
                  </tt:p>
                </tt:div>
              </tt:body>
            </tt:tt>
            """;

        var format = new EbuTtD();
        Assert.True(format.IsMine(raw.SplitToLines(), "sample.xml"));

        var sub = new Subtitle();
        format.LoadSubtitle(sub, raw.SplitToLines(), null);

        Assert.Single(sub.Paragraphs);
        Assert.Equal(1240, sub.Paragraphs[0].StartTime.TotalMilliseconds);
        Assert.Equal(3120, sub.Paragraphs[0].EndTime.TotalMilliseconds);
        Assert.Equal("Erste Zeile" + Environment.NewLine + "<i>kursive zweite Zeile</i>", sub.Paragraphs[0].Text);
    }
}
