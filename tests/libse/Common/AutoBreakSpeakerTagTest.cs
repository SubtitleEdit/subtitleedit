using Nikse.SubtitleEdit.Core.Common;

namespace LibSETests.Common;

/// <summary>
/// Translation models often hand a two-speaker cue back on one line with both speaker tags kept,
/// and the break by length then put the second tag mid-line. A break before a speaker tag counts
/// as a dialog break now, like one before a dash: it wins over the balanced break and may leave a
/// line over the maximum, as dash dialogs do today.
/// </summary>
public class AutoBreakSpeakerTagTest : IDisposable
{
    private readonly bool _dashEarly = Configuration.Settings.Tools.AutoBreakDashEarly;

    public AutoBreakSpeakerTagTest()
    {
        Configuration.Settings.Tools.AutoBreakDashEarly = true; // the default; the dialog break depends on it
    }

    public void Dispose()
    {
        Configuration.Settings.Tools.AutoBreakDashEarly = _dashEarly;
    }

    private static string Break(string text)
    {
        return Utilities.AutoBreakLine(text, 43, 33, "en").Replace(Environment.NewLine, "|");
    }

    /// <summary>The same break with the dialog preference off: what the length rule alone gives.</summary>
    private static string BreakByLengthOnly(string text)
    {
        Configuration.Settings.Tools.AutoBreakDashEarly = false;
        try
        {
            return Break(text);
        }
        finally
        {
            Configuration.Settings.Tools.AutoBreakDashEarly = true;
        }
    }

    [Theory]
    [InlineData("Inspector: I can hear it quite clearly, the wall is thin. Erik: Don't open that door.",
                "Inspector: I can hear it quite clearly, the wall is thin.|Erik: Don't open that door.")]
    [InlineData("【Anna】I was at home the whole night. 【Inspector】But the neighbours heard you leaving.",
                "【Anna】I was at home the whole night.|【Inspector】But the neighbours heard you leaving.")]
    [InlineData("[Anna] I was at home the whole night. [Inspector] But the neighbours heard you leaving.",
                "[Anna] I was at home the whole night.|[Inspector] But the neighbours heard you leaving.")]
    [InlineData("-That's not Erik's car. -Then whose is it?",
                "-That's not Erik's car.|-Then whose is it?")]
    [InlineData("The door opened again. Inspector: Sit down, both of you, this will take a while.",
                "The door opened again.|Inspector: Sit down, both of you, this will take a while.")]
    [InlineData("Inspector: Where were you last night? Erik: At the office.",
                "Inspector: Where were you last night?|Erik: At the office.")]
    [InlineData("Inspector： Where were you last night? Erik： At the office.",
                "Inspector： Where were you last night?|Erik： At the office.")]
    [InlineData("Dr. Jones: The results came back this morning. Mrs. Meier: And what do they say?",
                "Dr. Jones: The results came back this morning.|Mrs. Meier: And what do they say?")]
    public void BreaksBeforeTheSecondSpeakerTag(string text, string expected)
    {
        Assert.Equal(expected, Break(text));
    }

    // Without a speaker tag the dialog rule changes nothing: a time, a colon mid-sentence or a URL is
    // not a tag, and a wrapped sentence still breaks where the length rule puts it.
    [Theory]
    [InlineData("The meeting is at 10:30 and the lab report arrived this morning, and the results matched what I expected.", "10:30")]
    [InlineData("Here is what the footage shows: the car arrived at ten and left at half past eleven.", "shows:")]
    [InlineData("Open the link http://example.com/report now. It has the lab results for this morning.", "http:")]
    [InlineData("The lab report arrived this morning, and the results matched exactly what I had expected to see.", "and")]
    public void WithoutASpeakerTagTheLengthRuleDecides(string text, string notAtLineStart)
    {
        var broken = Break(text);

        Assert.Equal(BreakByLengthOnly(text), broken);
        var lines = broken.Split('|');
        Assert.Equal(2, lines.Length);
        Assert.False(lines[1].StartsWith(notAtLineStart, StringComparison.Ordinal), lines[1]);
    }
}
