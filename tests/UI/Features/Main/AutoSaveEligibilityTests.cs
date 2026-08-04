using Nikse.SubtitleEdit.Features.Main;

namespace UITests.Features.Main;

public class AutoSaveEligibilityTests
{
    [Fact]
    public void NamedFileSameFormat_CanWriteBack()
    {
        // The healthy case: a normal, named file open in the format it was loaded in.
        var eligible = AutoSaveEligibility.CanWriteBack(
            isEmpty: false, hasFileName: true, converted: false,
            lastOpenSaveFormatName: "SubRip", currentFormatName: "SubRip");

        Assert.True(eligible);
    }

    [Fact]
    public void ReopenedNormalFile_AfterEarlierImport_CanWriteBack()
    {
        // Regression guard for issue #12753: an import (OCR, Matroska, Transport Stream, transcription, ...)
        // sets _converted = true, and it used to be cleared only by Save-as - so ResetSubtitle leaked the flag
        // and a normal file opened afterwards stayed "converted", silently disabling auto-save. ResetSubtitle
        // now clears _converted, so the reopened file presents as not-converted and must be eligible here.
        var eligible = AutoSaveEligibility.CanWriteBack(
            isEmpty: false, hasFileName: true, converted: false,
            lastOpenSaveFormatName: "SubRip", currentFormatName: "SubRip");

        Assert.True(eligible);
    }

    [Fact]
    public void Converted_CannotWriteBack()
    {
        // A converted/imported document has no committed save target; saving it needs "Save as".
        var eligible = AutoSaveEligibility.CanWriteBack(
            isEmpty: false, hasFileName: true, converted: true,
            lastOpenSaveFormatName: "SubRip", currentFormatName: "SubRip");

        Assert.False(eligible);
    }

    [Fact]
    public void Empty_CannotWriteBack()
    {
        var eligible = AutoSaveEligibility.CanWriteBack(
            isEmpty: true, hasFileName: true, converted: false,
            lastOpenSaveFormatName: "SubRip", currentFormatName: "SubRip");

        Assert.False(eligible);
    }

    [Fact]
    public void NoFileName_CannotWriteBack()
    {
        // Untitled: no file to write back to yet.
        var eligible = AutoSaveEligibility.CanWriteBack(
            isEmpty: false, hasFileName: false, converted: false,
            lastOpenSaveFormatName: "SubRip", currentFormatName: "SubRip");

        Assert.False(eligible);
    }

    [Fact]
    public void NeverOpenedOrSaved_CannotWriteBack()
    {
        // lastOpenSaveFormatName == null: the file was never opened/saved in a known format.
        var eligible = AutoSaveEligibility.CanWriteBack(
            isEmpty: false, hasFileName: true, converted: false,
            lastOpenSaveFormatName: null, currentFormatName: "SubRip");

        Assert.False(eligible);
    }

    [Fact]
    public void FormatChanged_CannotWriteBack()
    {
        // Changing the format turns a save into a "Save as"; auto-save must not trigger that.
        var eligible = AutoSaveEligibility.CanWriteBack(
            isEmpty: false, hasFileName: true, converted: false,
            lastOpenSaveFormatName: "SubRip", currentFormatName: "Advanced Sub Station Alpha");

        Assert.False(eligible);
    }
}
