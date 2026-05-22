using Nikse.SubtitleEdit.Core.SubtitleFormats;

namespace Nikse.SubtitleEdit.Logic.Config;

public class MsOrFramesValue
{
    public int Milliseconds { get; set; }
    public int Frames { get; set; }

    public int GetMilliseconds()
    {
        return Se.Settings.General.UseFrameMode
            ? SubtitleFormat.FramesToMilliseconds(Frames)
            : Milliseconds;
    }
}
