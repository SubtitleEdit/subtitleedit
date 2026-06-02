using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using System.Collections.Generic;
using System.Linq;

namespace LibSETests.SubtitleFormats;

public class ScenaristClosedCaptionsTest
{
    private static Subtitle LoadScc(params string[] timedRows)
    {
        var lines = new List<string> { "Scenarist_SCC V1.0", "" };
        foreach (var row in timedRows)
        {
            lines.Add(row);
            lines.Add(string.Empty);
        }

        var subtitle = new Subtitle();
        new ScenaristClosedCaptions().LoadSubtitle(subtitle, lines, "test.scc");
        return subtitle;
    }

    [Fact]
    public void ImportValidCaption()
    {
        // "OK" encoded with CEA-608 odd parity: O = 4f, K = cb (0x4b with the parity bit set).
        var subtitle = LoadScc(
            "00:00:00:00\t94ae 94ae 9420 9420 9470 9470 4fcb 942f 942f",
            "00:00:04:00\t942c 942c");

        Assert.Single(subtitle.Paragraphs);
        Assert.Equal("OK", HtmlUtil.RemoveHtmlTags(subtitle.Paragraphs[0].Text, true));
    }

    [Fact]
    public void DoesNotEmitRawCea608BytesAsText()
    {
        // Synthetic repro from issue #11341: a row of raw CEA-608 data words (no proper
        // positioning codes) used to be decoded byte-pair-by-byte-pair into one long line of
        // mojibake and emitted as a subtitle cue. It must be rejected instead.
        var subtitle = LoadScc(
            "00:00:00:00\t94ae 94ae 9420 9420 9470 9470 4fcb 942f 942f",
            "00:00:04:00\t942c 942c",
            "00:00:25:00\t94ae 94ae 9420 9420 e640 e640 f06d 4250 20f0 6e52 f0f0 f750 2050 d061 5050 e53e f0c0 e550 e2f2 4f20 e262 5256 e0e5 f0e6 e2c0 402c 48e9 3068 c076 52d6 f0e0 e320 fe60 5020 de50 e020 f6e0 5020 5e60 e0e0 d061 de50 5252 ff70 4050 61e6 40c0 e942 e640 5ec0 52e6 4040 af20 9137 9137 e0e0 50f2 ff70 4050 61e6 40c0 e942 e640 ff70 4050 61e6 40c0 e942 e640 942f 942f",
            "00:45:36:04\t942c 942c");

        // Only the single valid caption survives; the bogus 608-data row is dropped.
        Assert.Single(subtitle.Paragraphs);
        Assert.Equal("OK", HtmlUtil.RemoveHtmlTags(subtitle.Paragraphs[0].Text, true));
        Assert.DoesNotContain(subtitle.Paragraphs, p => p.Text.Contains("f@"));
    }

    [Fact]
    public void KeepsLongCaptionWithPositioningCode()
    {
        // A genuine two-row caption whose decoded text happens to exceed 32 characters (because
        // the decoder does not always re-insert the line break) must still be kept: it has a
        // Preamble Address Code (1340 / 13e0), so it is real caption text, not raw 608 data.
        var subtitle = LoadScc(
            "00:00:25:00\t94ae 94ae 9420 9420 1340 1340 9723 9723 cec1 5252 c154 4f52 ba20 616e 6420 68e9 7320 e6f2 e9e5 6e64 732c 13e0 13e0 9723 9723 2073 f4ef 7020 e9ec ece5 6794 2c94 2c61 ec20 e9e3 e520 e6e9 7368 e5f2 6de5 6e80 94d6 94d6 20e3 efec 64ae aeae 942f 942f",
            "00:45:36:04\t942c 942c");

        Assert.Single(subtitle.Paragraphs);
        var text = HtmlUtil.RemoveHtmlTags(subtitle.Paragraphs[0].Text, true);
        Assert.Contains("NARRATOR", text);
        Assert.Contains("illegal ice fishermen", text);
    }

    [Fact]
    public void PreambleAddressCodesProduceLineBreaks()
    {
        // Issue #9803: row-positioning Preamble Address Codes (916e, 92ce, ...) were decoded as
        // stray letters ("n"/"N") instead of line breaks, e.g. "NARRADOR:nAsh y sus amigosN...".
        var subtitle = LoadScc(
            "00:00:25:00\t9420 9420 91d0 91d0 cec1 5252 c1c4 4f52 ba80 916e 916e c173 6820 7920 7375 7320 616d e967 ef73 92ce 92ce e3ef 6ef4 e96e e061 6e20 7375 2076 e961 eae5 942c 942c 942f 942f",
            "00:45:36:04\t942c 942c");

        Assert.Single(subtitle.Paragraphs);
        var lines = HtmlUtil.RemoveHtmlTags(subtitle.Paragraphs[0].Text, true).SplitToLines();
        Assert.Equal(new[] { "NARRADOR:", "Ash y sus amigos", "continúan su viaje" }, lines);
        Assert.DoesNotContain("amigosN", subtitle.Paragraphs[0].Text);
        Assert.DoesNotContain(":n", subtitle.Paragraphs[0].Text);
    }
}
