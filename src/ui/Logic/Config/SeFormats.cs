namespace Nikse.SubtitleEdit.Logic.Config;

public class SeFormats
{
    public string RosettaLanguage { get; set; }
    public bool RosettaLanguageAutoDetect { get; set; }    
    public string RosettaFontSize { get; set; }
    public string RosettaLineHeight { get; set; }

    public string TmpegEncXmlFontName { get; set; }
    public decimal TmpegEncXmlFontHeight { get; set; }
    public decimal TmpegEncXmlOffsetX { get; set; }
    public decimal TmpegEncXmlOffsetY { get; set; }
    public bool TmpegEncXmlFontBold { get; set; }

    // WebVTT "X-TIMESTAMP-MAP" header offsets every cue on load (e.g. MPEGTS:900000 = +10s).
    // On by default to match the spec; can be turned off for files where the offset is unwanted.
    public bool WebVttUseXTimestampMap { get; set; }
    public bool WebVttUseMultipleXTimestampMap { get; set; }
    public bool WebVttMergeLinesWithSameText { get; set; }
    public bool WebVttDoNoMergeTags { get; set; }

    // Cue settings written for each ASSA alignment tag ({\an1} - {\an9}) when saving WebVTT,
    // e.g. "{\an8}" -> "line:20%". Free text, so any valid cue setting can be used.
    public string WebVttCueAn1 { get; set; }
    public string WebVttCueAn2 { get; set; }
    public string WebVttCueAn3 { get; set; }
    public string WebVttCueAn4 { get; set; }
    public string WebVttCueAn5 { get; set; }
    public string WebVttCueAn6 { get; set; }
    public string WebVttCueAn7 { get; set; }
    public string WebVttCueAn8 { get; set; }
    public string WebVttCueAn9 { get; set; }

    // Timed Text 1.0 / iTunes Timed Text / IMSC 1.1 file properties. These used to be written
    // straight onto the libse Configuration singleton, which SE5 never persists - so every one of
    // them was back at its built-in default after a restart.
    public string TimedText10TimeCodeFormat { get; set; }
    public string TimedText10FileExtension { get; set; }
    public string TimedTextItunesTopOrigin { get; set; }
    public string TimedTextItunesTopExtent { get; set; }
    public string TimedTextItunesBottomOrigin { get; set; }
    public string TimedTextItunesBottomExtent { get; set; }
    public string TimedTextItunesTimeCodeFormat { get; set; }
    public string TimedTextItunesStyleAttribute { get; set; }
    public string TimedTextItunesLanguage { get; set; }
    public string TimedTextImsc11TimeCodeFormat { get; set; }
    public string TimedTextImsc11FileExtension { get; set; }

    public SeFormats()
    {
        RosettaLanguage = "en";
        RosettaLanguageAutoDetect = true;
        RosettaFontSize = "100%";
        RosettaLineHeight = "125%";

        TmpegEncXmlFontName = "Tahoma";
        TmpegEncXmlFontHeight = 0.067m;
        TmpegEncXmlFontBold = false;
        TmpegEncXmlOffsetX = 0.001m;
        TmpegEncXmlOffsetY = 0.001m;

        WebVttUseXTimestampMap = true;
        WebVttUseMultipleXTimestampMap = true;
        WebVttMergeLinesWithSameText = false;
        WebVttDoNoMergeTags = false;

        WebVttCueAn1 = "position:20%";
        WebVttCueAn2 = string.Empty;
        WebVttCueAn3 = "position:80%";
        WebVttCueAn4 = "position:20% line:50%";
        WebVttCueAn5 = "line:50%";
        WebVttCueAn6 = "position:80% line:50%";
        WebVttCueAn7 = "position:20% line:20%";
        WebVttCueAn8 = "line:20%";
        WebVttCueAn9 = "position:80% line:20%";

        TimedText10TimeCodeFormat = "Source";
        TimedText10FileExtension = ".xml";
        TimedTextItunesTopOrigin = "0% 0%";
        TimedTextItunesTopExtent = "100% 15%";
        TimedTextItunesBottomOrigin = "0% 85%";
        TimedTextItunesBottomExtent = "100% 15%";
        TimedTextItunesTimeCodeFormat = "Frames";
        TimedTextItunesStyleAttribute = "tts:fontStyle";
        TimedTextItunesLanguage = string.Empty;
        TimedTextImsc11TimeCodeFormat = "hh:mm:ss.ms";
        TimedTextImsc11FileExtension = ".xml";
    }
}