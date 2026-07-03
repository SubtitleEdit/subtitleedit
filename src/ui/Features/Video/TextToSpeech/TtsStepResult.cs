using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Video.TextToSpeech.Voices;

namespace Nikse.SubtitleEdit.Features.Video.TextToSpeech;

public class TtsStepResult
{
    public Paragraph Paragraph { get; set; }
    public string Text { get; set; }
    public string CurrentFileName { get; set; }
    public float SpeedFactor { get; set; }
    public Voice? Voice { get; set; }

    // Recorded so the review window can show which engine/model/instruction produced each line
    // and restore them to the left panel when the user clicks the row. Engine name is used (not
    // the engine instance) so it survives serialization to/from SubtitleEditTts.json. May be
    // empty for lines that didn't go through the per-actor cast.
    public string EngineName { get; set; }
    public string Model { get; set; }
    public string Instruction { get; set; }

    // Whether the review window's Include checkbox is/was ticked for this line. Defaults to
    // true; carried through import so an exported session's unchecked rows stay unchecked.
    public bool Include { get; set; }

    public TtsStepResult()
    {
        Paragraph = new Paragraph();
        Text = string.Empty;
        CurrentFileName = string.Empty;
        SpeedFactor = 1.0f;
        EngineName = string.Empty;
        Model = string.Empty;
        Instruction = string.Empty;
        Include = true;
    }
}