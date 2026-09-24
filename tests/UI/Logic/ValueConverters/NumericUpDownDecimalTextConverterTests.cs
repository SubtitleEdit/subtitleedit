using Nikse.SubtitleEdit.Logic.ValueConverters;

namespace UITests.Logic.ValueConverters;

/// <summary>
/// Fractional number boxes parsed with the current culture, where the other decimal separator is
/// the thousands separator: typing "0.5" on a Danish system gave 5 (and "0,5" on an English one).
/// </summary>
public class NumericUpDownDecimalTextConverterTests
{
    [Theory]
    [InlineData("0.5", 0.5)]
    [InlineData("0,5", 0.5)]
    [InlineData(".5", 0.5)]
    [InlineData(" 12,25 ", 12.25)]
    [InlineData("1.234,5", 1234.5)]
    [InlineData("1,234.5", 1234.5)]
    [InlineData("-0,75", -0.75)]
    [InlineData("3", 3)]
    public void Parse_TakesBothDecimalSeparators(string text, double expected)
    {
        Assert.Equal((decimal)expected, NumericUpDownDecimalTextConverter.Parse(text));
    }

    [Theory]
    [InlineData("")]
    [InlineData("abc")]
    [InlineData(null)]
    public void Parse_NotANumber_IsNull(string? text)
    {
        Assert.Null(NumericUpDownDecimalTextConverter.Parse(text));
    }
}
