using System;

namespace Nikse.SubtitleEdit.Features.Main;

/// <summary>
/// Pure decision logic for the debounced auto-save in <see cref="MainViewModel"/>.
/// Kept side-effect free so it can be unit tested without spinning up the view model.
/// </summary>
public static class AutoSaveDebounce
{
    public enum Action
    {
        /// <summary>Nothing to do this tick (no changes, or still waiting for the idle window).</summary>
        Skip,

        /// <summary>Content just changed - (re)start the idle window and remember the new content hash.</summary>
        Arm,

        /// <summary>Content has been idle long enough - write the file(s) now.</summary>
        Save,
    }

    /// <summary>
    /// Decides what auto-save should do this tick.
    /// </summary>
    /// <param name="mainDirty">Main subtitle has unsaved changes.</param>
    /// <param name="originalDirty">Original/translation subtitle has unsaved changes.</param>
    /// <param name="settleHash">Combined content hash for the current tick.</param>
    /// <param name="previousSettleHash">Content hash recorded when the idle window was last armed.</param>
    /// <param name="lastChangeUtc">When the idle window was last armed.</param>
    /// <param name="nowUtc">Current time.</param>
    /// <param name="idleSeconds">How long content must be unchanged before saving.</param>
    public static Action Decide(
        bool mainDirty,
        bool originalDirty,
        int settleHash,
        int previousSettleHash,
        DateTime lastChangeUtc,
        DateTime nowUtc,
        double idleSeconds)
    {
        if (!mainDirty && !originalDirty)
        {
            return Action.Skip;
        }

        // Content changed since we last looked - restart the idle window.
        if (settleHash != previousSettleHash)
        {
            return Action.Arm;
        }

        // Same content as last tick, but not idle long enough yet.
        if ((nowUtc - lastChangeUtc).TotalSeconds < idleSeconds)
        {
            return Action.Skip;
        }

        return Action.Save;
    }
}
