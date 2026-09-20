using Nikse.SubtitleEdit.Core.SubtitleFormats;

namespace LibSETests.SubtitleFormats;

/// <summary>
/// The video preview turns the position a format carries - TTML regions, EBU STL teletext rows,
/// PAC percentages - into ASSA alignment and margins, and asks the format whether it still carries
/// any (discussion #13857). The header and the Region/Effect/MarginV fields of the file a subtitle
/// was read from survive a format change in the toolbar, so the flag is what stops a subtitle now
/// shown as SubRip from keeping the layout of the format it has left.
/// </summary>
public class SubtitleFormatPositionSupportTest
{
    [Theory]
    [InlineData(typeof(Ebu))]
    [InlineData(typeof(Pac))]
    [InlineData(typeof(PacUnicode))]
    [InlineData(typeof(TimedText10))]
    [InlineData(typeof(DfxpBasic))]
    [InlineData(typeof(ItunesTimedText))]
    [InlineData(typeof(NetflixTimedText))]
    [InlineData(typeof(SmpteTt2052))]
    [InlineData(typeof(TimedTextImsc11))]
    [InlineData(typeof(TimedTextImscRosetta))]
    [InlineData(typeof(NetflixImsc11Japanese))]
    [InlineData(typeof(ClqttJson))]
    [InlineData(typeof(WebVTT))]
    [InlineData(typeof(WebVTTFileWithLineNumber))]
    public void AFormatThatCarriesPositionsSaysSo(Type formatType)
    {
        var format = (SubtitleFormat)Activator.CreateInstance(formatType)!;

        Assert.True(format.HasPositionSupport);
    }

    // Nothing in a SubRip or an ASSA file says where a line sits that this turns into a position:
    // ASSA margins are already in script units, and SubRip has none at all.
    [Theory]
    [InlineData(typeof(SubRip))]
    [InlineData(typeof(AdvancedSubStationAlpha))]
    [InlineData(typeof(SubStationAlpha))]
    [InlineData(typeof(MicroDvd))]
    [InlineData(typeof(EbuTtD))]
    public void AFormatWithoutPositionsSaysSo(Type formatType)
    {
        var format = (SubtitleFormat)Activator.CreateInstance(formatType)!;

        Assert.False(format.HasPositionSupport);
    }
}
