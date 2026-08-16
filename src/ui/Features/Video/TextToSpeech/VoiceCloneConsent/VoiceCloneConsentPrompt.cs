using Avalonia.Controls;
using Nikse.SubtitleEdit.Features.Shared;
using Nikse.SubtitleEdit.Features.Video.TextToSpeech.Engines;
using Nikse.SubtitleEdit.Logic.Config;
using System;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Video.TextToSpeech.VoiceCloneConsent;

/// <summary>
/// The one place that decides whether the first-clone consent dialog is owed, shows it, and
/// reports whether cloning may go ahead.
/// </summary>
/// <remarks>
/// Shared rather than repeated per entry point: every way of turning a recording into a voice
/// (the voice-settings import button, its drag-drop, the waveform's "Clone voice to") has to ask
/// the same question exactly once, and a second copy of the rule is a second chance to get the
/// "closed the window without accepting" case wrong.
/// </remarks>
public static class VoiceCloneConsentPrompt
{
    /// <summary>
    /// Returns true when <paramref name="engine"/> may clone: either no consent is owed (a
    /// non-cloning engine, or it was accepted earlier and is still on) or the user accepts it now.
    /// </summary>
    /// <param name="engine">The engine about to be handed a reference recording.</param>
    /// <param name="owner">Owner window for the dialog and the declined message.</param>
    /// <param name="showConsentDialogAsync">
    /// Shows <c>VoiceCloneConsentWindow</c> modally over <paramref name="owner"/>. Passed in
    /// rather than opened here so each caller keeps its own dialog plumbing (the main window, for
    /// one, pauses the video and resets held-down shortcut keys around every modal).
    /// </param>
    public static async Task<bool> EnsureAsync(
        ITtsEngine? engine,
        Window owner,
        Func<Task<VoiceCloneConsentViewModel>> showConsentDialogAsync)
    {
        if (!VoiceCloningConsent.RequiresConsent(engine) || VoiceCloningConsent.IsAccepted)
        {
            return true;
        }

        var result = await showConsentDialogAsync();

        // Re-check the stored answer rather than trusting OkPressed alone, matching the IndexTTS
        // 2.5 licence gate: closing the window by any other route must not count as consent.
        if (!result.OkPressed || !VoiceCloningConsent.IsAccepted)
        {
            await MessageBox.Show(
                owner,
                Se.Language.Video.TextToSpeech.VoiceCloneConsentTitle,
                Environment.NewLine + Se.Language.Video.TextToSpeech.VoiceCloneConsentDeclined,
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
            return false;
        }

        return true;
    }
}
