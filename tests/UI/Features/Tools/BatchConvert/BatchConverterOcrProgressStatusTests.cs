using Nikse.SubtitleEdit.Features.Tools.BatchConvert;
using Nikse.SubtitleEdit.Logic.Config;

namespace UITests.Features.Tools.BatchConvert;

/// <summary>
/// After OCR finishes, batch convert clears the "OCR: {0}%" progress off the row so it does not sit
/// on a finished-looking "OCR: 100%" while the convert functions and the save still run (#13706).
/// That clearing keys off <see cref="BatchConverter.IsOcrProgressStatus"/>, which must not mistake a
/// terminal status an OCR runner deliberately left behind for progress - swallowing a "Cancelled" or
/// an error message would be a good deal worse than the cosmetic issue it fixes.
/// </summary>
public class BatchConverterOcrProgressStatusTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(7)]
    [InlineData(100)]
    public void ProgressValues_AreRecognized(int percent)
    {
        Assert.True(BatchConverter.IsOcrProgressStatus(string.Format(Se.Language.General.OcrPercentX, percent)));
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("-")]
    [InlineData("Converted")]
    [InlineData("Cancelled")]
    [InlineData("Error: engine not reachable")]
    [InlineData("Preparing OCR...")]
    [InlineData("OCR: %")]
    [InlineData("OCR: abc%")]
    [InlineData("OCR: 50")]
    [InlineData("50%")]
    public void NonProgressValues_AreNotRecognized(string? status)
    {
        Assert.False(BatchConverter.IsOcrProgressStatus(status));
    }

    [Fact]
    public void WorkingStatus_IsNotRecognized()
    {
        // The status the row is reset back to must not itself look like progress, or a second pass
        // over it would be ambiguous.
        Assert.False(BatchConverter.IsOcrProgressStatus(Se.Language.General.OcrDotDotDot));
    }
}
