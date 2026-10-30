namespace Nikse.SubtitleEdit.Logic.Config;

/// <summary>
/// What to do when the open subtitle file was changed by another program (checked when the main
/// window is activated again, see MainViewModel.ExternalFileChange).
/// </summary>
public enum FileChangedOutsideActionType
{
    /// <summary>Reload right away when there are no unsaved changes, otherwise ask.</summary>
    ReloadWhenUnmodified,

    /// <summary>Always ask before reloading, also when there are no unsaved changes.</summary>
    AlwaysAsk,

    /// <summary>Leave the subtitle alone (saving over the outside change still asks).</summary>
    Never,
}
