using Nikse.SubtitleEdit.Core.Cea608;

namespace LibSETests.Cea608;

public class SerializedScreenTextTest
{
    private static string Decode(params (int a, int b)[] pairs)
    {
        var result = new List<string>();
        var parser = new CcDataC608Parser();
        parser.DisplayScreen += data => result.Add(SerializedScreenText.GetText(data.Screen));
        var t = 0;
        foreach (var (a, b) in pairs)
        {
            parser.AddData(t, new[] { a, b });
            t += 100;
        }

        return Assert.Single(result);
    }

    private static (int, int)[] PopOn(params (int, int)[] body)
    {
        var pairs = new List<(int, int)> { (0x14, 0x20), (0x14, 0x2E) }; // RCL, ENM
        pairs.AddRange(body);
        pairs.Add((0x14, 0x2F)); // EOC
        pairs.Add((0x14, 0x2C)); // EDM
        return pairs.ToArray();
    }

    [Fact]
    public void PlainTextHasNoTags()
    {
        Assert.Equal("Hi", Decode(PopOn((0x14, 0x70), (0x48, 0x69))));
    }

    [Fact]
    public void ItalicPacWrapsRowInItalicTags()
    {
        // PAC row 15, white italics
        Assert.Equal("<i>Hi</i>", Decode(PopOn((0x14, 0x6E), (0x48, 0x69))));
    }

    [Fact]
    public void MidRowItalicsTagOnlyThePartAfterTheCode()
    {
        // "Hi " + mid-row italics + "yo " + mid-row white + "ok"; the closing tag goes before the space
        var text = Decode(PopOn((0x14, 0x70), (0x48, 0x69), (0x20, 0x00), (0x11, 0x2E), (0x79, 0x6F), (0x20, 0x00), (0x11, 0x20), (0x6F, 0x6B)));
        Assert.Equal("Hi <i>yo</i> ok", text);
    }

    [Fact]
    public void EachRowGetsItsOwnTagsAndNoPadding()
    {
        // row 14 italics "Hi", row 15 plain "yo"
        var text = Decode(PopOn((0x14, 0x4E), (0x48, 0x69), (0x14, 0x70), (0x79, 0x6F)));
        Assert.Equal("<i>Hi</i>" + Environment.NewLine + "yo", text);
    }
}
