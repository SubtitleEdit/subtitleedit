namespace Nikse.SubtitleEdit.Features.Video.TextToSpeech;

public class TtsImportExportItem
{
    public string Text { get; set; }
    public long StartMs { get; set; }
    public long EndMs { get; set; }
    public string AudioFileName { get; set; }
    public float SpeedFactor { get; set; }
    public string EngineName { get; set; }
    public string VoiceName { get; set; }
    public bool Include { get; set; }

    public TtsImportExportItem()
    {
        Text = string.Empty;
        StartMs = 0;
        EndMs = 0;
        AudioFileName = string.Empty;
        SpeedFactor = 1.0f;
        EngineName = string.Empty;
        VoiceName = string.Empty;
        Include = true;
    }
}
