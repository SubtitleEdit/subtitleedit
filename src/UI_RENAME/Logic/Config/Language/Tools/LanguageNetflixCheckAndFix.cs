namespace Nikse.SubtitleEdit.Logic.Config.Language;

public class LanguageNetflixCheckAndFix
{
    public string Title { get; set; }
    public string GenerateReport { get; set; }
    public string NothingToReport { get; set; }
    public string SaveNetflixQualityReport { get; set; }
    public string NetflixReportSaved { get; set; }
    public string NetFlixQualityReportSavedToX { get; set; }
    public string DialogHyphenSpace { get; set; }
    public string EllipsesNotThreeDots { get; set; }
    public string OnlyAllowedGlyphs { get; set; }
    public string Italics { get; set; }
    public string MaxCharsSec { get; set; }
    public string MaxDuration { get; set; }
    public string MaxLineLength { get; set; }
    public string MinDuration { get; set; }
    public string MaxNumberOfLines { get; set; }
    public string OneToTenSpellOut { get; set; }
    public string ShotChanges { get; set; }
    public string StartNumberSpellOut { get; set; }
    public string TextforHiUseBrackets { get; set; }
    public string FrameRate { get; set; }
    public string TwoFrameGrap { get; set; }
    public string WhiteSpace { get; set; }

    // Checker comment strings
    public string GlyphCheckReport { get; set; }
    public string WhiteSpaceCheckForXReport { get; set; }
    public string WhiteSpaceBeforePunctuation { get; set; }
    public string WhiteSpaceLineEnding { get; set; }
    public string WhiteSpaceCheckConsecutive { get; set; }
    public string TwoLinesMaximum { get; set; }
    public string TextCanFitOnOneLine { get; set; }
    public string SingleVerticalLineLengthMax11 { get; set; }
    public string SingleHorizontalLineLengthMax13 { get; set; }
    public string SingleLineLengthExceedsX { get; set; }
    public string NumbersOneToTenSpellOut { get; set; }
    public string UseSquareBracketsForHiAndSoundEffects { get; set; }
    public string ShotChangeInCueWithinXFramesAfterSnap { get; set; }
    public string ShotChangeInCue1ToXFramesBeforeSnap { get; set; }
    public string ShotChangeInCueXToYFramesBeforePull { get; set; }
    public string ShotChangeOutCueWithinXFramesAfterChange { get; set; }
    public string ShotChangeOutCueWithinXFramesOfChange { get; set; }
    public string ShotChangeOutCueOnShotChange { get; set; }
    public string DualSpeakersHyphenWithoutSpace { get; set; }
    public string DualSpeakersHyphenWithSpace { get; set; }
    public string DualSpeakersHyphenWithSpaceSecondOnly { get; set; }
    public string DualSpeakersHyphenWithoutSpaceSecondOnly { get; set; }
    public string MinimumTwoFramesGapOverlapping { get; set; }
    public string MinimumTwoFramesGap { get; set; }
    public string FrameRateIsInvalidX { get; set; }
    public string FrameRateIsInvalid { get; set; }
    public string MinimumDurationJapanese { get; set; }
    public string MinimumDuration { get; set; }
    public string EllipsesUseSmartCharacter { get; set; }
    public string MaximumDuration { get; set; }
    public string ItalicsFixed { get; set; }
    public string ItalicsNotAllowed { get; set; }
    public string BridgeGapsXToYFrames { get; set; }
    public string StartNumberSpellOutComment { get; set; }
    public string MaximumXCharactersPerSecond { get; set; }

    public LanguageNetflixCheckAndFix()
    {
        Title = "Netflix check and fix errors";
        GenerateReport = "Generate report";
        NothingToReport = "No issues found.";
        SaveNetflixQualityReport = "Save Netflix quality report";
        NetflixReportSaved = "Netflix quality report saved";
        NetFlixQualityReportSavedToX = "Netflix quality report saved to:\n {0}";
        DialogHyphenSpace = "Dialog hyphen space";
        EllipsesNotThreeDots = "Use elipses (not three dots)";
        OnlyAllowedGlyphs = "Only allowed glyphs";
        Italics = "Italics";
        MaxCharsSec = "Max chars/sec";
        MaxDuration = "Max duration";
        MaxLineLength = "Max line length";
        MinDuration = "Min duration";
        MaxNumberOfLines = "Max number of lines";
        OneToTenSpellOut = "One to ten spell out";
        ShotChanges = "Shot changes";
        StartNumberSpellOut = "Start number spell out";
        TextforHiUseBrackets = "Text for HI, use brackets";
        FrameRate = "Frame rate";
        TwoFrameGrap = "Two frame gap";
        WhiteSpace = "White space";

        // Checker comment strings
        GlyphCheckReport = "Invalid character {0} found at column {1}";
        WhiteSpaceCheckForXReport = "White space issue ({0}) found at column {1}.";
        WhiteSpaceBeforePunctuation = "missing before punctuation";
        WhiteSpaceLineEnding = "line ending";
        WhiteSpaceCheckConsecutive = "2+ consecutive";
        TwoLinesMaximum = "Two lines maximum";
        TextCanFitOnOneLine = "Text can fit on one line";
        SingleVerticalLineLengthMax11 = "Single vertical line length > 11";
        SingleHorizontalLineLengthMax13 = "Single horizontal line length > 13";
        SingleLineLengthExceedsX = "Single line length > {0}";
        NumbersOneToTenSpellOut = "From 1 to 10, numbers should be written out: one, two, three, etc";
        UseSquareBracketsForHiAndSoundEffects = "Use brackets [ ] to enclose speaker IDs or sound effects";
        ShotChangeInCueWithinXFramesAfterSnap = "The in-cue is within {0} frames after the shot change, snap the in-cue to the shot-change";
        ShotChangeInCue1ToXFramesBeforeSnap = "The in-cue is 1-{0} frames before the shot change, snap the in-cue to the shot change";
        ShotChangeInCueXToYFramesBeforePull = "The in-cue is {0}-{1} frames before the shot change, pull the in-cue to half a second ({2} frames) before the shot-change";
        ShotChangeOutCueWithinXFramesAfterChange = "The out-cue is within {0} frames after the shot change";
        ShotChangeOutCueWithinXFramesOfChange = "The out-cue is within {0} frames of the shot change";
        ShotChangeOutCueOnShotChange = "The out-cue is on the shot change, respect the two-frame gap";
        DualSpeakersHyphenWithoutSpace = "Dual Speakers: Use a hyphen without a space";
        DualSpeakersHyphenWithSpace = "Dual Speakers: Use a hyphen with a space";
        DualSpeakersHyphenWithSpaceSecondOnly = "Dual Speakers: Use a hyphen with a space to denote the second speaker only";
        DualSpeakersHyphenWithoutSpaceSecondOnly = "Dual Speakers: Use a hyphen without a space to denote the second speaker only";
        MinimumTwoFramesGapOverlapping = "Minimum two frames gap (Overlapping)";
        MinimumTwoFramesGap = "Minimum two frames gap";
        FrameRateIsInvalidX = "Frame rate is invalid: '{0}'";
        FrameRateIsInvalid = "Frame rate is invalid";
        MinimumDurationJapanese = "Minimum duration: 0.5 second";
        MinimumDuration = "Minimum duration: 5/6 second (833 ms)";
        EllipsesUseSmartCharacter = "Use the single smart character (U+2026) as opposed to three dots/periods in a row";
        MaximumDuration = "Maximum duration: 7 seconds per subtitle event";
        ItalicsFixed = "Fixed italics";
        ItalicsNotAllowed = "Italics not allowed";
        BridgeGapsXToYFrames = "3-{0} frames gap => 2 frames gap";
        StartNumberSpellOutComment = "When a number begins a sentence, it should always be spelled out";
        MaximumXCharactersPerSecond = "Maximum {0} characters per second";
    }
}