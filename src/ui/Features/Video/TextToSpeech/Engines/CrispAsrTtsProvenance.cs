using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Download;
using Nikse.SubtitleEdit.UiLogic;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;

namespace Nikse.SubtitleEdit.Features.Video.TextToSpeech.Engines;

/// <summary>
/// The consent / AI-marking attestations every CrispASR TTS engine has to send, in one place.
///
/// CrispASR gates voice cloning behind two request fields and provenance marking behind three
/// launch flags:
///
/// <list type="bullet">
///   <item><c>consent_attestation</c> — required (HTTP 400 <c>consent_required</c> otherwise) when
///   the request's <c>voice</c> names a <c>.wav</c> reference, i.e. actual cloning.</item>
///   <item><c>marking_attestation</c> — required from CrispASR v0.8.22 (HTTP 400
///   <c>marking_attestation_required</c>) whenever such a clone also sets
///   <c>spoken_disclaimer: false</c>, which SE does on every engine. Chatterbox is the engine that
///   hits this today (it is the only one whose <c>voice</c> keeps the <c>.wav</c> extension); the
///   others clone from the server's startup <c>--voice</c> and are not classified as clones by the
///   server, but they send the field too so a widened upstream check cannot break them.</item>
///   <item><c>--no-watermark --no-c2pa --accept-marking-responsibility</c> — SE opts out of the
///   inaudible AudioSeal watermark and the C2PA manifest chunk, both of which alter the rendered
///   audio/container that then gets muxed into the user's video. The server refuses either opt-out
///   without the attestation flag.</item>
/// </list>
///
/// All of it is behind <see cref="SeVideoTextToSpeech.AcceptVoiceCloning"/> (default on): with the
/// setting off SE sends no attestation, keeps the audible AI disclaimer, and lets CrispASR
/// watermark and sign its output — cloning a reference WAV then fails at the server, which is the
/// point.
/// </summary>
public static class CrispAsrTtsProvenance
{
    internal const string ConsentAttestation = "I have the speaker's consent, or it is my own voice.";
    internal const string MarkingAttestation = "I will disclose that this audio is AI-generated.";

    public static bool IsAccepted => Se.Settings.Video.TextToSpeech.AcceptVoiceCloning;

    /// <summary>
    /// Adds the cloning attestations plus <c>spoken_disclaimer: false</c> to a
    /// <c>/v1/audio/speech</c> payload. No-op when the user has not accepted voice cloning, so the
    /// server applies its own defaults (audible disclaimer on, cloning refused).
    /// </summary>
    public static void AddSpeechAttestations(IDictionary<string, object> payload)
    {
        if (!IsAccepted)
        {
            return;
        }

        payload["consent_attestation"] = ConsentAttestation;
        payload["marking_attestation"] = MarkingAttestation;

        // Skip the audible AI-disclosure prefix CrispASR otherwise prepends to cloned audio; SE
        // surfaces the AI-generated nature in its UI, and a spoken disclaimer in front of every
        // dubbed line would be unusable.
        payload["spoken_disclaimer"] = false;
    }

    /// <summary>
    /// Appends the provenance opt-out flags to a <c>crispasr --server</c> argument list.
    /// </summary>
    /// <remarks>
    /// Skipped unless the installed binary is known to understand them (v0.8.22+): crispasr aborts
    /// on an unknown argument, so passing them to an older install the user declined to update
    /// would break TTS outright.
    /// </remarks>
    public static void AddServerMarkingArgs(Collection<string> args, string crispAsrExecutable)
    {
        if (!IsAccepted || !SupportsMarkingOptOut(crispAsrExecutable))
        {
            return;
        }

        args.Add("--no-watermark");
        args.Add("--no-c2pa");
        args.Add("--accept-marking-responsibility");
    }

    /// <summary>
    /// True when the installed crispasr executable is a release that has the provenance opt-out
    /// flags. A hash matching a known *older* release says "definitely too old"; an unrecognised
    /// hash (custom local build) is given the benefit of the doubt, as elsewhere in SE.
    /// </summary>
    public static bool SupportsMarkingOptOut(string crispAsrExecutable)
    {
        if (string.IsNullOrEmpty(crispAsrExecutable) || !File.Exists(crispAsrExecutable))
        {
            return false;
        }

        var folder = Path.GetDirectoryName(crispAsrExecutable);
        string? variant = null;
        if (folder != null)
        {
            variant = OperatingSystem.IsWindows()
                ? DownloadHashManager.DetectCrispAsrWindowsVariant(folder)
                : OperatingSystem.IsLinux()
                    ? DownloadHashManager.DetectCrispAsrLinuxVariant(folder)
                    : null;
        }

        var key = DownloadHashManager.ResolveCrispAsrExecutableKey(variant);
        if (key == null)
        {
            return true;
        }

        var hash = Sha256Util.ComputeSha256(crispAsrExecutable);
        if (hash == null)
        {
            return true;
        }

        return DownloadHashManager.GetStatus(key, hash) != DownloadHashManager.UpdateStatus.UpdateAvailable;
    }
}
