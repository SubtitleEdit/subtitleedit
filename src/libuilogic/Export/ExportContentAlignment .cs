namespace Nikse.SubtitleEdit.UiLogic.Export;

public enum ExportContentAlignment
{
    Left,
    Center,
    Right,

    /// <summary>
    /// Justify each line the way the subtitle is placed in the frame - a "{\an1}" line is
    /// left justified, an "{\an3}" line right justified, everything else centered. Added last
    /// so the values already saved in profiles keep their meaning.
    /// </summary>
    FromAlignment,
}