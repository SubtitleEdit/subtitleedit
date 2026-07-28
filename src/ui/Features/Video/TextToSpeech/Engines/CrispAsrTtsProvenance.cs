using Nikse.SubtitleEdit.Logic.Config;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

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
///   without the attestation flag, and only understands the attestation flag from v0.8.22, so the
///   set is passed only when the binary's own <c>--help</c> lists all three.</item>
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
    /// The flags SE passes to opt out of provenance marking. All three have to be understood by the
    /// binary before any of them is passed — <c>--accept-marking-responsibility</c> is what the
    /// server demands before it will honour either opt-out, and it only exists from v0.8.22.
    /// </summary>
    private static readonly string[] MarkingOptOutFlags =
    {
        "--no-watermark",
        "--no-c2pa",
        "--accept-marking-responsibility",
    };

    /// <summary>
    /// Appends the provenance opt-out flags to a <c>crispasr --server</c> argument list.
    /// </summary>
    /// <remarks>
    /// Skipped unless the binary's own <c>--help</c> lists all of them: crispasr treats an unknown
    /// argument as fatal ("error: unknown argument", usage dump, exit 0), so passing them to an
    /// older install breaks TTS outright. v0.8.20 is the trap — it has <c>--no-watermark</c> but
    /// neither <c>--no-c2pa</c> nor <c>--accept-marking-responsibility</c>.
    /// </remarks>
    public static void AddServerMarkingArgs(Collection<string> args, string crispAsrExecutable)
    {
        if (!IsAccepted || !SupportsMarkingOptOut(crispAsrExecutable))
        {
            return;
        }

        foreach (var flag in MarkingOptOutFlags)
        {
            args.Add(flag);
        }
    }

    /// <summary>
    /// True when the crispasr binary's <c>--help</c> documents every flag in
    /// <see cref="MarkingOptOutFlags"/>.
    /// </summary>
    /// <remarks>
    /// Asks the binary rather than inferring from its hash. A locally built crispasr — or any
    /// release SE never tracked — has an unrecognised hash, and guessing "probably new enough"
    /// there is what breaks server startup; guessing "probably too old" would silently drop the
    /// opt-out on a perfectly capable build. <c>--help</c> costs a few milliseconds and answers
    /// the actual question, and the result is cached per (path, size, write time) so a
    /// re-download re-probes. Anything unexpected — cannot start, times out, no help text — fails
    /// closed, i.e. CrispASR keeps marking its output, which is the safe direction.
    /// </remarks>
    public static bool SupportsMarkingOptOut(string crispAsrExecutable)
    {
        if (string.IsNullOrEmpty(crispAsrExecutable) || !File.Exists(crispAsrExecutable))
        {
            return false;
        }

        string cacheKey;
        try
        {
            var info = new FileInfo(crispAsrExecutable);
            cacheKey = $"{info.FullName}|{info.Length}|{info.LastWriteTimeUtc.Ticks}";
        }
        catch
        {
            return false;
        }

        lock (SupportCacheLock)
        {
            if (SupportCache.TryGetValue(cacheKey, out var cached))
            {
                return cached;
            }
        }

        var help = HelpTextProvider(crispAsrExecutable);
        var supported = help != null
            && MarkingOptOutFlags.All(flag => help.Contains(flag, StringComparison.Ordinal));

        lock (SupportCacheLock)
        {
            SupportCache[cacheKey] = supported;
        }

        return supported;
    }

    private static readonly Dictionary<string, bool> SupportCache = new();
    private static readonly Lock SupportCacheLock = new();

    /// <summary>
    /// How <see cref="SupportsMarkingOptOut"/> gets the binary's help text. Swappable so the flag
    /// decision can be tested without spawning a stub executable, which is not portable between
    /// the dev machines and the Windows CI.
    /// </summary>
    internal static Func<string, string?> HelpTextProvider { get; set; } = ReadHelpText;

    internal static void ClearSupportCache()
    {
        lock (SupportCacheLock)
        {
            SupportCache.Clear();
        }
    }

    private static string? ReadHelpText(string crispAsrExecutable)
    {
        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = crispAsrExecutable,
                WorkingDirectory = Path.GetDirectoryName(crispAsrExecutable) ?? string.Empty,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
            };
            psi.ArgumentList.Add("--help");

            using var process = Process.Start(psi);
            if (process == null)
            {
                return null;
            }

            // Drain both pipes concurrently. crispasr writes its whole ~25 KB usage dump to
            // stderr and nothing at all to stdout, so reading stdout to EOF first deadlocked on
            // Windows (#12878): the child blocks once the 4 KB stderr pipe is full, stdout never
            // reaches EOF because the child cannot exit, and WaitForExit below is never even
            // reached — the app froze on the UI thread for good. Unix escaped it only because its
            // pipe buffer is 64 KB. Starting both reads before waiting also means the timeout
            // actually governs the probe.
            var stdout = process.StandardOutput.ReadToEndAsync();
            var stderr = process.StandardError.ReadToEndAsync();
            if (!process.WaitForExit(HelpProbeTimeoutMs))
            {
                TryKill(process);
                return null;
            }

            // The process has exited, so both pipes are at EOF and these are already complete.
            // Bounded anyway so a wedged read cannot outlive the probe.
            if (!Task.WaitAll(new Task[] { stdout, stderr }, HelpProbeTimeoutMs))
            {
                return null;
            }

            return stdout.Result + stderr.Result;
        }
        catch (Exception exception)
        {
            Se.LogError(exception, "CrispASR marking opt-out probe failed for " + crispAsrExecutable);
            return null;
        }
    }

    private const int HelpProbeTimeoutMs = 15_000;

    private static void TryKill(Process process)
    {
        try
        {
            process.Kill(entireProcessTree: true);
        }
        catch
        {
            // best effort — the probe already failed closed
        }
    }
}
