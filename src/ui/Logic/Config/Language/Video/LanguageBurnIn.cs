namespace Nikse.SubtitleEdit.Logic.Config.Language;

public class LanguageBurnIn
{
    public string Title { get; set; }
    public string TargetFileSize { get; set; }
    public string FileSizeMb { get; set; }
    public string MatchSourceVideoSize { get; set; }
    public string BitRate { get; set; }
    public string TotalBitRateX { get; set; }
    public string SampleRate { get; set; }
    public string Audio { get; set; }
    public string Preset { get; set; }
    public string PixelFormat { get; set; }
    public string Crf { get; set; }
    public string UseSource { get; set; }
    public string FontSizeFactor { get; set; }
    public string BoxType { get; set; }
    public string FromTime { get; set; }
    public string ToTime { get; set; }
    public string AudioEncoding { get; set; }
    public string OutputProperties { get; set; }
    public string VideoFileSize { get; set; }
    public string OneBox { get; set; }
    public string LogoInfo { get; set; }

    public LanguageBurnIn()
    {
        Title = "Generate video with burned-in subtitles";
        TargetFileSize = "Target file size (requires 2 pass encoding)";
        FileSizeMb = "File size in MB";
        MatchSourceVideoSize = "Match source video size";
        BitRate = "Bit rate";
        TotalBitRateX = "Total bit rate: {0}";
        SampleRate = "Sample rate";
        Audio = "Audio";
        Preset = "Preset";
        PixelFormat = "Pixel format";
        Crf = "CRF";
        UseSource = "Use source";
        FontSizeFactor = "Font size factor";
        BoxType = "Box type";
        FromTime = "From time";
        ToTime = "To time";
        AudioEncoding = "Audio encoding";
        OutputProperties = "Output properties...";
        VideoFileSize = "Video file size";
        OneBox = "One box";
        LogoInfo = "Pick a PNG image and drag it to position it on the video.";
    }
}