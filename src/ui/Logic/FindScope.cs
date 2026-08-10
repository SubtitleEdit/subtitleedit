namespace Nikse.SubtitleEdit.Logic;

public partial class FindService
{
    /// <summary>
    /// Which columns a find or replace covers while an original subtitle is loaded (translator
    /// mode). Ignored when there is no original text - everything then targets the text column.
    /// </summary>
    public enum FindScope
    {
        TextAndOriginal,
        TextOnly,
        OriginalOnly,
    }
}
