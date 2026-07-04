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
    }
}