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

    // Per-line engine snapshot — needed so the review window's "click-to-sync left panel" can
    // restore the exact settings a line was generated with after a round-trip through
    // SubtitleEditTts.json. Without these the lookup falls back to the global selection and the
    // user loses their per-actor model/instruction. May be empty for legacy exports.
    public string Model { get; set; }
    public string Instruction { get; set; }

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
        Model = string.Empty;
        Instruction = string.Empty;
        Include = true;
    }
}
