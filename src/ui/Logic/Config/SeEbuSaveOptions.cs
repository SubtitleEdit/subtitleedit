namespace Nikse.SubtitleEdit.Logic.Config;

/// <summary>
/// EBU STL save options that live outside the 1024-character GSI header stored on the subtitle:
/// the justification code is written per TTI block, and the layout values are read from libse's
/// Configuration singleton by <c>Ebu.Save</c>. Persisting them here is what makes the choices in
/// the save options dialog survive both reopening the dialog and restarting the application.
/// </summary>
public class SeEbuSaveOptions
{
    /// <summary>0=unchanged, 1=left, 2=centered, 3=right - centered is the broadcast default.</summary>
    public int JustificationCode { get; set; } = 2;

    public int MarginTop { get; set; } = 0;
    public int MarginBottom { get; set; } = 2;
    public int NewLineRows { get; set; } = 2;
    public bool TeletextUseBox { get; set; } = true;
    public bool TeletextUseDoubleHeight { get; set; } = true;

    /// <summary>
    /// Font family the video preview draws EBU STL subtitles in, empty for the normal preview font.
    /// Preview only - no font is written to an STL file, which carries a character table and leaves
    /// the typeface to the decoder. Lets someone with a teletext face installed see the real thing.
    /// </summary>
    public string PreviewFontName { get; set; } = string.Empty;
}
