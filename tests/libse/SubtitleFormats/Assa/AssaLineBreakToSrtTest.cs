using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;

namespace LibSETests.SubtitleFormats.Assa
{
    /// <summary>
    /// Regression tests for issue #11338: converting ASSA to SubRip must turn the ASSA
    /// line-break tags "\N" (hard) and "\n" (soft) into real line breaks *inside* a single
    /// subtitle - not split each line into its own subtitle sharing the same time code.
    /// See the ASS spec: http://www.tcax.org/docs/ass-specs.htm
    /// </summary>
    public class AssaLineBreakToSrtTest
    {
        private static string BuildAssa(string dialogueText) =>
            "[Script Info]\r\n" +
            "ScriptType: v4.00+\r\n" +
            "\r\n" +
            "[V4+ Styles]\r\n" +
            "Format: Name, Fontname, Fontsize\r\n" +
            "Style: Default,Arial,20\r\n" +
            "\r\n" +
            "[Events]\r\n" +
            "Format: Layer, Start, End, Style, Name, MarginL, MarginR, MarginV, Effect, Text\r\n" +
            "Dialogue: 0,0:00:01.00,0:00:02.00,Default,,0,0,0,," + dialogueText + "\r\n";

        [Theory]
        [InlineData("First line\\NSecond line")] // hard break
        [InlineData("First line\\nSecond line")] // soft break
        public void AssaLineBreakBecomesSingleMultiLineSubtitle(string dialogueText)
        {
            var assa = new AdvancedSubStationAlpha();
            var lines = BuildAssa(dialogueText).SplitToLines();
            Assert.True(assa.IsMine(lines, null));

            var subtitle = new Subtitle();
            assa.LoadSubtitle(subtitle, lines, null);

            // The two visual lines must stay as ONE subtitle, not two.
            Assert.Single(subtitle.Paragraphs);
            Assert.Equal("First line" + System.Environment.NewLine + "Second line", subtitle.Paragraphs[0].Text);
        }

        [Fact]
        public void ConvertAssaToSubRipKeepsLineBreakWithinOneCue()
        {
            var assa = new AdvancedSubStationAlpha();
            var lines = BuildAssa("First line\\NSecond line").SplitToLines();

            var subtitle = new Subtitle();
            assa.LoadSubtitle(subtitle, lines, null);
            subtitle.OriginalFormat = assa;

            var srt = new SubRip().ToText(subtitle, "test").Replace("\r\n", "\n");

            // Exactly one SubRip cue (index "1") with both lines under the same time code,
            // and no second cue is created for the wrapped line.
            Assert.Contains("1\n00:00:01,000 --> 00:00:02,000\nFirst line\nSecond line", srt);
            Assert.DoesNotContain("\n2\n", srt);
        }
    }
}
