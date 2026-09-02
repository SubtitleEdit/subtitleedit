using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Logic;
using Xunit;

namespace UITests.Logic;

/// <summary>
/// Regressions for two silent-corruption bugs: toggle casing moved an ASSA line break to the
/// end of the line, and UUEncode always wrote 4 characters for the final group so every
/// embedded attachment grew by 1-2 bytes.
/// </summary>
public class CasingAndUuEncodingRegressionTests
{
    [Fact]
    public void CasingKeepsAssaLineBreakInPlace()
    {
        var toggler = new CasingToggler();
        var format = new AdvancedSubStationAlpha();
        Assert.Equal(@"{\an8}HELLO\NWORLD", toggler.ToggleCasing(@"{\an8}Hello\NWorld", format));
        Assert.Equal(@"HELLO\NWORLD", toggler.ToggleCasing(@"Hello\NWorld", format));
    }

    [Fact]
    public void UuEncodeRoundTripsExactLength()
    {
        foreach (var len in new[] { 1, 2, 3, 4, 5, 6, 7 })
        {
            var input = new byte[len];
            for (var i = 0; i < len; i++) { input[i] = (byte)(i + 1); }
            var decoded = Nikse.SubtitleEdit.Core.Common.UUEncoding.UUDecode(
                Nikse.SubtitleEdit.Core.Common.UUEncoding.UUEncode(input));
            Assert.Equal(len, decoded.Length);
            Assert.Equal(input, decoded);
        }
    }
}
