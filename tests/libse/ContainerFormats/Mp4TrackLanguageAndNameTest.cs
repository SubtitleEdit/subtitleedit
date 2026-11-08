using Nikse.SubtitleEdit.Core.ContainerFormats.Mp4.Boxes;
using System.Text;

namespace LibSETests.ContainerFormats;

/// <summary>
/// #16730: streaming-service MP4s leave mdhd at "und" and only name their subtitle tracks
/// in udta/name ("English", "Simplified Chinese"), so the MP4 track picker showed eleven
/// identical "SubtitleHandler" rows.
/// </summary>
public class Mp4TrackLanguageAndNameTest
{
    private static byte[] MakeBox(string type, params byte[][] payloads)
    {
        var payload = payloads.SelectMany(p => p).ToArray();
        var size = payload.Length + 8;
        var box = new byte[size];
        box[0] = (byte)(size >> 24);
        box[1] = (byte)(size >> 16);
        box[2] = (byte)(size >> 8);
        box[3] = (byte)size;
        Encoding.Latin1.GetBytes(type).CopyTo(box, 4);
        payload.CopyTo(box, 8);
        return box;
    }

    private static byte[] Mdhd(string language)
    {
        var payload = new byte[24]; // version 0: flags, times, timescale, duration, language, quality
        payload[15] = 0xE8; // timescale 1000
        payload[14] = 0x03;
        var packed = ((language[0] - 0x60) << 10) | ((language[1] - 0x60) << 5) | (language[2] - 0x60);
        payload[20] = (byte)(packed >> 8);
        payload[21] = (byte)packed;
        return MakeBox("mdhd", payload);
    }

    private static byte[] Hdlr(string handlerType, string name)
    {
        var payload = new byte[24 + name.Length + 1];
        Encoding.ASCII.GetBytes(handlerType).CopyTo(payload, 8);
        Encoding.ASCII.GetBytes(name).CopyTo(payload, 24);
        return MakeBox("hdlr", payload);
    }

    private static byte[] Elng(string tag) => MakeBox("elng", new byte[4], Encoding.ASCII.GetBytes(tag + "\0"));

    private static Trak ParseTrak(byte[] trakPayload)
    {
        using var ms = new MemoryStream(trakPayload);
        return new Trak(ms, (ulong)trakPayload.Length);
    }

    private static byte[] Mdia(params byte[][] children) => MakeBox("mdia", children);

    [Fact]
    public void UndLanguageWithUdtaName_ReadsTrackName()
    {
        var trak = ParseTrak(Concat(
            Mdia(Mdhd("und"), Hdlr("sbtl", "SubtitleHandler")),
            MakeBox("udta", MakeBox("name", Encoding.UTF8.GetBytes("Simplified Chinese")))));

        Assert.Equal("Simplified Chinese", trak.TrackName);
        Assert.Equal("sbtl", trak.Mdia.HandlerType);
        Assert.Equal(string.Empty, trak.Mdia.LanguageDisplayName);
        Assert.Equal("Chinese", trak.LanguageDisplayName);
    }

    [Theory]
    [InlineData("English", "English")]
    [InlineData("spanish", "Spanish")]
    [InlineData("Bahasa Malaysia", "Malay")]
    [InlineData("Bahasa Indonesia", "Indonesian")]
    [InlineData("Traditional Chinese", "Chinese")]
    [InlineData("English (SDH)", "English")]
    [InlineData("Portuguese [Brazil]", "Portuguese")]
    [InlineData("Commentary", "")]
    [InlineData("SubtitleHandler", "")]
    [InlineData("", "")]
    public void LanguageFromTrackName(string trackName, string expected)
    {
        Assert.Equal(expected, Trak.LanguageFromTrackName(trackName));
    }

    [Fact]
    public void MdhdLanguageWinsOverTrackName()
    {
        var trak = ParseTrak(Concat(
            Mdia(Mdhd("jpn"), Hdlr("sbtl", "SubtitleHandler")),
            MakeBox("udta", MakeBox("name", Encoding.UTF8.GetBytes("English")))));

        Assert.Equal("Japanese", trak.LanguageDisplayName);
    }

    [Fact]
    public void MdhdLanguage_GivesEnglishName()
    {
        var trak = ParseTrak(Mdia(Mdhd("kor"), Hdlr("sbtl", "SubtitleHandler")));

        Assert.Equal("Korean", trak.Mdia.LanguageDisplayName);
        Assert.Equal(string.Empty, trak.TrackName);
    }

    [Fact]
    public void ElngOverridesMdhd()
    {
        var trak = ParseTrak(Mdia(Mdhd("und"), Hdlr("sbtl", "SubtitleHandler"), Elng("zh-Hant")));

        Assert.Equal("zh-Hant", trak.Mdia.ExtendedLanguage);
        Assert.Equal("Chinese (zh-Hant)", trak.Mdia.LanguageDisplayName);
        Assert.Equal("Chinese (zh-Hant)", trak.LanguageDisplayName);
    }

    [Fact]
    public void QuickTimeNameAtom_IsRead()
    {
        var text = Encoding.UTF8.GetBytes("Magyar");
        var qtString = Concat(new[] { (byte)0, (byte)text.Length, (byte)0x15, (byte)0xC7 }, text);
        var nam = MakeBox("xnam", qtString);
        nam[4] = 0xA9;

        var trak = ParseTrak(Concat(Mdia(Mdhd("hun"), Hdlr("sbtl", "")), MakeBox("udta", nam)));

        Assert.Equal("Magyar", trak.TrackName);
        Assert.Equal("Hungarian", trak.Mdia.LanguageDisplayName);
    }

    [Fact]
    public void UdtaBeforeMdia_StillParsesMdia()
    {
        var trak = ParseTrak(Concat(
            MakeBox("udta", MakeBox("name", Encoding.UTF8.GetBytes("Thai\0"))),
            Mdia(Mdhd("tha"), Hdlr("sbtl", "SubtitleHandler"))));

        Assert.Equal("Thai", trak.TrackName);
        Assert.Equal("Thai", trak.Mdia.LanguageDisplayName);
    }

    private static byte[] Concat(params byte[][] parts) => parts.SelectMany(p => p).ToArray();
}
