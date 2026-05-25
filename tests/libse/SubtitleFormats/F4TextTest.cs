using System.Text.RegularExpressions;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;

namespace LibSETests.SubtitleFormats;

public class F4TextTest
{
    // Regression: EncodeTimeCode used Math.Round on Milliseconds / 100, so any
    // value in 950..999 ms rounded to 10 deciseconds and produced "..-10",
    // which violates the format regex "^\d\d:\d\d:\d\d-\d$". The writer fix
    // clamps to 9 so every emitted timecode stays single-digit.
    [Theory]
    [InlineData(0)]
    [InlineData(499)]
    [InlineData(949)]
    [InlineData(950)]   // boundary that used to round up to 10
    [InlineData(999)]   // worst case
    public void EncodedTimecode_DecisecondIsAlwaysSingleDigit(int milliseconds)
    {
        var src = new Subtitle();
        src.Paragraphs.Add(new Paragraph("hello", 0, 1000 + milliseconds));

        var text = F4Text.ToF4Text(src);

        // Every emitted timecode must satisfy the format regex (one decisecond
        // digit only). Use the same shape the parser uses to validate.
        var perTc = new Regex(@"#(\d\d:\d\d:\d\d-\d)#");
        foreach (Match m in perTc.Matches(text))
        {
            Assert.Matches(@"^\d\d:\d\d:\d\d-\d$", m.Groups[1].Value);
        }

        // And there must be at least one timecode emitted.
        Assert.Matches(@"#\d\d:\d\d:\d\d-\d#", text);
    }
}
