namespace Nikse.SubtitleEdit.Logic.Config;

/// <summary>
/// What a "surround with" slot does when fired.
/// </summary>
public enum SurroundWithBehavior
{
    /// <summary>Add the pair, or remove it again when it is already there.</summary>
    Toggle,

    /// <summary>Always add the pair - pressing the shortcut again adds it once more (SE 4 behavior, #15531).</summary>
    Add,

    /// <summary>Only remove the pair; text without it is left alone.</summary>
    Remove,

    /// <summary>Remove one pair per press - the counterpart of <see cref="Add"/> (#15531).</summary>
    RemoveOnce,
}
