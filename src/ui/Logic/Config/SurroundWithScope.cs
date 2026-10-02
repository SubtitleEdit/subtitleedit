namespace Nikse.SubtitleEdit.Logic.Config;

/// <summary>
/// What text a "surround with" slot works on.
/// </summary>
public enum SurroundWithScope
{
    /// <summary>The selected part of the text box, else the whole text of each selected subtitle.</summary>
    SelectionOrText,

    /// <summary>Each line of each selected subtitle on its own - "[Hello]" / "[Bye]" instead of "[Hello" / "Bye]".</summary>
    EachLine,
}
