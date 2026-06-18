using Nikse.SubtitleEdit.Features.Tools.BatchConvert;

namespace UITests.Features.Tools.BatchConvert;

public class BatchConverterMp4FormatTests
{
    // Regression test for issue #11693: Batch Convert wrote empty 7-byte files for MP4 files
    // with embedded text subtitles. The scanner stores the picked track in the item's format
    // string (e.g. "MP4/WebVTT - eng"), but the converter guarded on the bare string "MP4",
    // which never matched - so no subtitle was ever extracted. These cases assert the converter
    // recognizes exactly the format strings the scanner produces.

    [Theory]
    // "MP4/WebVTT - " + VttcLanguage  (see AddFile in BatchConvertViewModel)
    [InlineData("MP4/WebVTT - eng")]
    [InlineData("MP4/WebVTT - en-us")]
    // $"MP4/#{index} {HandlerType} - {language}"
    [InlineData("MP4/#0 text - eng")]
    [InlineData("MP4/#1 sbtl - und")]
    public void IsMp4SubtitleFormat_ScannerProducedTrackStrings_AreRecognized(string format)
    {
        Assert.True(BatchConverter.IsMp4SubtitleFormat(format));
    }

    [Theory]
    [InlineData("MP4")]                       // the old buggy guard value - never produced by the scanner
    [InlineData("Matroska/SubRip - eng #3")]  // a different container's track string
    [InlineData("SubRip")]
    [InlineData("")]
    [InlineData(null)]
    public void IsMp4SubtitleFormat_NonMp4TrackStrings_AreRejected(string? format)
    {
        Assert.False(BatchConverter.IsMp4SubtitleFormat(format));
    }
}
