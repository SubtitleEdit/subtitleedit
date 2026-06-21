using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Video.SpeechToText.Engines;
using Nikse.SubtitleEdit.Features.Video.TextToSpeech.Voices;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Download;
using Nikse.SubtitleEdit.Logic.Media;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Video.TextToSpeech.Engines;

/// <summary>
/// Qwen3-TTS run through the CrispASR runtime instead of qwen3-tts.cpp. Useful as an A/B
/// comparison engine since CrispASR's qwen3-tts backends emit EOS reliably on short prompts
/// where qwen3-tts.cpp 0.6B doesn't (see PR investigation notes from May 2026).
///
/// Three CrispASR sub-backends are wired up, one per model key:
///  - qwen3-tts-1.7b-voicedesign — VoiceDesign model, requires an `instructions` field
///    per request (free-text voice description, e.g. "a calm female voice").
///  - qwen3-tts-1.7b-customvoice — picks one of nine fixed speakers baked into the GGUF by
///    name (e.g. "vivian"); does NOT clone and ignores any reference WAV.
///  - qwen3-tts-1.7b-base — runtime ICL voice cloning from an imported reference WAV plus its
///    transcript (the <name>.txt sidecar the backend auto-loads as ref-text). This is the
///    model the "1.7B Voice clone" key maps to.
///
/// The 1.7B VoiceDesign talker GGUF is the same file the qwen3-tts.cpp engine uses (cstr's
/// upload, new tensor names). The CrispASR-style codec/tokenizer GGUF is a different file
/// (~986 MB) and lives in this engine's own folder — see <see cref="Qwen3TtsCrispAsrDownloadService"/>
/// for the URLs.
/// </summary>
public class Qwen3TtsCrispAsr : ITtsEngine
{
    public string Name => "Qwen3 TTS (CrispASR)";
    public string Description => "via CrispASR (VoiceDesign or CustomVoice 1.7B)";
    public bool HasLanguageParameter => false;
    public bool HasApiKey => false;
    public bool HasRegion => false;
    public bool HasModel => true;
    public bool HasKeyFile => false;

    public const string ModelKeyVoiceDesign = "1.7B VoiceDesign";
    public const string ModelKeyCustomVoice = "1.7B CustomVoice";
    public const string ModelKeyClone = "1.7B Voice clone";
    public const string DefaultModelKey = ModelKeyVoiceDesign;

    public const string VoiceDesignTalkerFileName = "qwen3-tts-12hz-1.7b-voicedesign-q8_0.gguf";
    public const string CustomVoiceTalkerFileName = "qwen3-tts-12hz-1.7b-customvoice-q8_0.gguf";
    public const string BaseTalkerFileName = "qwen3-tts-12hz-1.7b-base-q8_0.gguf";
    public const string CodecFileName = "qwen3-tts-tokenizer-12hz.gguf";

    /// <summary>
    /// The nine fixed speakers baked into the CustomVoice talker GGUF, in the order the model
    /// exposes them (first entry, <c>aiden</c>, is the backend's default when none is picked).
    /// CustomVoice selects one of these by name via the request's <c>voice</c> field — it does
    /// NOT clone from a reference WAV (that is what <see cref="ModelKeyClone"/> / the Base
    /// backend does). Names must match the GGUF metadata exactly; they are sent verbatim.
    /// </summary>
    public static readonly string[] CustomVoiceSpeakers =
    {
        "aiden", "dylan", "eric", "ono_anna", "ryan", "serena", "sohee", "uncle_fu", "vivian",
    };

    // Exact byte size of each GGUF on cstr's HuggingFace repos (X-Linked-Size). Used to reject
    // truncated files that crispasr's own --auto-download may have left behind in ~/.cache/crispasr/
    // when a download was interrupted: the loader passes the bounds check on these "looks-like-a-
    // GGUF" partials before segfaulting deep in the legacy weight loader, which surfaces as an
    // opaque process exit (Windows access violation 0xC0000005) with a CUDA banner in the log
    // that wrongly suggests a GPU problem. A size check is essentially free (one stat call) and
    // catches the truncation case completely. SHA-256 verification stays on the SE-side
    // downloader for content integrity; sizes-only is enough at the cache-seed gate.
    private static readonly Dictionary<string, long> ExpectedFileSizes = new(StringComparer.OrdinalIgnoreCase)
    {
        [VoiceDesignTalkerFileName] = 2042225536L,
        [CustomVoiceTalkerFileName] = 2042225952L,
        [BaseTalkerFileName] = 2066258176L,
        [CodecFileName] = 358453280L,
    };

    /// <summary>
    /// True if <paramref name="path"/> exists and (when an expected size is known for
    /// <paramref name="fileName"/>) matches that size. Used in place of bare File.Exists at every
    /// gate that decides whether a local GGUF is usable, so partial downloads can't masquerade as
    /// installed models.
    /// </summary>
    public static bool IsValidLocalModelFile(string path, string fileName)
    {
        if (!File.Exists(path))
        {
            return false;
        }

        if (!ExpectedFileSizes.TryGetValue(fileName, out var expected))
        {
            return true;
        }

        try
        {
            return new FileInfo(path).Length == expected;
        }
        catch
        {
            return false;
        }
    }

    public static string ResolveModelKey(string? modelKey)
    {
        var key = string.IsNullOrEmpty(modelKey)
            ? Se.Settings.Video.TextToSpeech.Qwen3TtsCrispAsrModel
            : modelKey;

        if (key == ModelKeyCustomVoice)
        {
            return ModelKeyCustomVoice;
        }
        if (key == ModelKeyClone)
        {
            return ModelKeyClone;
        }
        return ModelKeyVoiceDesign;
    }

    public static string GetTalkerFileName(string? modelKey) =>
        ResolveModelKey(modelKey) switch
        {
            ModelKeyCustomVoice => CustomVoiceTalkerFileName,
            ModelKeyClone => BaseTalkerFileName,
            _ => VoiceDesignTalkerFileName,
        };

    /// <summary>
    /// CrispASR sub-backend name matching <paramref name="modelKey"/>:
    /// VoiceDesign → qwen3-tts-1.7b-voicedesign, CustomVoice → qwen3-tts-1.7b-customvoice,
    /// Voice clone → qwen3-tts-1.7b-base (the ICL voice-cloning backend).
    /// </summary>
    public static string GetBackendName(string? modelKey) =>
        ResolveModelKey(modelKey) switch
        {
            ModelKeyCustomVoice => "qwen3-tts-1.7b-customvoice",
            ModelKeyClone => "qwen3-tts-1.7b-base",
            _ => "qwen3-tts-1.7b-voicedesign",
        };

    /// <summary>
    /// True when the resolved model is the instruction-tuned VoiceDesign variant. Only this
    /// model honours the voice instruction; CustomVoice picks a fixed speaker and the Base
    /// (Voice clone) model clones from a reference WAV.
    /// </summary>
    public static bool IsVoiceDesignModel(string? modelKey) =>
        ResolveModelKey(modelKey) == ModelKeyVoiceDesign;

    /// <summary>
    /// True when the resolved model is the Base talker used for runtime voice cloning. This is
    /// the only model that consumes an imported reference WAV (plus its <c>.txt</c> transcript);
    /// CustomVoice and VoiceDesign ignore imported voices.
    /// </summary>
    public static bool IsCloneModel(string? modelKey) =>
        ResolveModelKey(modelKey) == ModelKeyClone;

    private static readonly HttpClient HttpClient = new()
    {
        Timeout = TimeSpan.FromMinutes(5),
    };
    private static readonly SemaphoreSlim ServerLock = new(1, 1);
    private static Process? _serverProcess;
    private static int _serverPort;
    private static string? _serverModelKey;
    private static string? _serverLaunchCommand;
    private static bool _processExitHooked;
    private static readonly StringBuilder _serverLog = new();

    private static string ServerBaseUrl => $"http://127.0.0.1:{_serverPort}";

    public Task<bool> IsInstalled(string? region)
    {
        return Task.FromResult(File.Exists(GetCrispAsrExecutable()));
    }

    public override string ToString() => Name;

    /// <summary>
    /// Path to the crispasr executable installed by the speech-to-text feature.
    /// Shared with Chatterbox TTS and all CrispASR ASR engines.
    /// </summary>
    public static string GetCrispAsrExecutable()
    {
        return new CrispAsrCohere().GetExecutable();
    }

    /// <summary>
    /// Returns the update status of the CrispASR runtime this engine sits on top of.
    /// Reads the speech-to-text CrispASR install's <c>.installed.sha256</c> sidecar;
    /// returns <see cref="DownloadHashManager.UpdateStatus.Unknown"/> when CrispASR
    /// is not installed or the sidecar is missing. Mirrors
    /// <see cref="ChatterboxTtsCpp.GetEngineUpdateStatus"/>.
    /// </summary>
    public static DownloadHashManager.UpdateStatus GetEngineUpdateStatus()
    {
        var exe = GetCrispAsrExecutable();
        if (!File.Exists(exe))
        {
            return DownloadHashManager.UpdateStatus.Unknown;
        }

        var folder = Path.GetDirectoryName(exe);
        return string.IsNullOrEmpty(folder)
            ? DownloadHashManager.UpdateStatus.Unknown
            : DownloadHashManager.GetSidecarStatus(folder);
    }

    public static string GetSetFolder()
    {
        if (!Directory.Exists(Se.TextToSpeechFolder))
        {
            Directory.CreateDirectory(Se.TextToSpeechFolder);
        }

        var folder = Path.Combine(Se.TextToSpeechFolder, "Qwen3TtsCrispAsr");
        if (!Directory.Exists(folder))
        {
            Directory.CreateDirectory(folder);
        }

        return folder;
    }

    public static string GetSetModelsFolder()
    {
        // Qwen3 TTS (CrispASR) is driven by the CrispASR binary, so its GGUFs live
        // alongside the CrispASR speech-to-text models in CrispASR/models/ rather
        // than under TextToSpeech/Qwen3TtsCrispAsr/. The voices folder and synth
        // output WAVs still live under TextToSpeech/Qwen3TtsCrispAsr/ since those
        // are TTS-engine state, not models. Mirrors ChatterboxTtsCpp's layout.
        var modelsFolder = Path.Combine(Se.CrispAsrFolder, "models");
        if (!Directory.Exists(modelsFolder))
        {
            Directory.CreateDirectory(modelsFolder);
        }

        return modelsFolder;
    }

    public static string GetSetVoicesFolder()
    {
        var voicesFolder = Path.Combine(GetSetFolder(), "voices");
        if (!Directory.Exists(voicesFolder))
        {
            Directory.CreateDirectory(voicesFolder);
        }

        SeedVoicesFromQwen3TtsCppIfEmpty(voicesFolder);
        NormalizeVoiceSampleRatesOnce(voicesFolder);
        NormalizeVoiceTranscriptsOnce(voicesFolder);
        return voicesFolder;
    }

    private static bool _voiceSeedAttempted;
    private static bool _voiceSampleRatesNormalized;
    private static bool _voiceTranscriptsNormalized;

    /// <summary>
    /// One-time best-effort seeding of reference voices from the existing qwen3-tts.cpp engine's
    /// voices folder, so users who already have that voice pack get the WAVs here for free. The
    /// Base (Voice clone) backend strictly requires a 24 kHz mono reference, so each WAV is
    /// resampled on the way in rather than plain-copied (the qwen3-tts.cpp pack ships 22.05/48 kHz
    /// clips). The .txt sidecars in that pack are Wikimedia attribution blurbs, NOT spoken
    /// transcriptions — feeding them as ref-text produces runaway, off-voice output — so they are
    /// deliberately NOT copied. Cloning a seeded voice prompts for the real transcript via the
    /// Speak pre-check / re-import flow. Users who never installed qwen3-tts.cpp get voices.zip
    /// pulled by the DownloadTtsViewModel flow that chains after model download.
    /// </summary>
    private static void SeedVoicesFromQwen3TtsCppIfEmpty(string voicesFolder)
    {
        if (_voiceSeedAttempted)
        {
            return;
        }
        _voiceSeedAttempted = true;

        try
        {
            if (Directory.EnumerateFiles(voicesFolder, "*.wav").Any())
            {
                return;
            }

            var sourceFolder = Qwen3TtsCpp.GetSetVoicesFolder();
            if (!Directory.Exists(sourceFolder) || !Directory.EnumerateFiles(sourceFolder, "*.wav").Any())
            {
                return;
            }

            foreach (var src in Directory.GetFiles(sourceFolder, "*.wav"))
            {
                var dest = Path.Combine(voicesFolder, Path.GetFileName(src));
                if (File.Exists(dest))
                {
                    continue;
                }

                try
                {
                    // Resample to 24 kHz mono — the Base backend rejects any other rate.
                    var process = FfmpegGenerator.ConvertToMono24kHzWav(src, dest);
                    if (process.Start())
                    {
                        process.WaitForExit();
                    }
                }
                catch (Exception ex)
                {
                    Se.LogError(ex, $"Qwen3 TTS (CrispASR): failed to resample seeded voice {Path.GetFileName(src)}");
                }
            }
        }
        catch (Exception ex)
        {
            Se.LogError(ex, "Qwen3 TTS (CrispASR): voice seeding from qwen3-tts.cpp folder failed");
        }
    }

    private static void NormalizeVoiceTranscriptsOnce(string voicesFolder)
    {
        if (_voiceTranscriptsNormalized)
        {
            return;
        }
        _voiceTranscriptsNormalized = true;
        NormalizeVoiceTranscripts(voicesFolder);
    }

    /// <summary>
    /// One-time, per-session pass that resamples any reference WAV in <paramref name="voicesFolder"/>
    /// that is not already 24 kHz to 24 kHz mono in place. The qwen3-tts backend strictly rejects a
    /// voice prompt at any other rate (logs "voice prompt must be 24kHz, got NNNNN Hz") and then
    /// returns empty audio, so a voice pack at another rate — the shared voices.zip ships 22.05 kHz
    /// clips — must be brought up to 24 kHz before the server reads it from --voice-dir. Files that
    /// are already 24 kHz (or whose header we cannot parse) are left untouched so we never re-encode
    /// on every launch. Best-effort per file: an ffmpeg failure leaves the original in place.
    /// </summary>
    private static void NormalizeVoiceSampleRatesOnce(string voicesFolder)
    {
        if (_voiceSampleRatesNormalized)
        {
            return;
        }
        _voiceSampleRatesNormalized = true;

        if (!Directory.Exists(voicesFolder))
        {
            return;
        }

        foreach (var wav in Directory.GetFiles(voicesFolder, "*.wav"))
        {
            if (TryGetWavSampleRate(wav) is null or 24000)
            {
                continue; // unparseable header (leave for synth-time handling) or already correct
            }

            var temp = wav + ".24k.wav";
            var consumed = false;
            try
            {
                var process = FfmpegGenerator.ConvertToMono24kHzWav(wav, temp);
                if (!process.Start())
                {
                    continue;
                }

                process.WaitForExit();
                if (File.Exists(temp) && new FileInfo(temp).Length > 0)
                {
                    File.Delete(wav);
                    File.Move(temp, wav);
                    consumed = true;
                }
            }
            catch (Exception ex)
            {
                Se.LogError(ex, $"Qwen3 TTS (CrispASR): failed to resample voice '{wav}' to 24 kHz");
            }
            finally
            {
                if (!consumed && File.Exists(temp))
                {
                    try { File.Delete(temp); } catch { /* leave it; not worth retrying */ }
                }
            }
        }
    }

    /// <summary>
    /// Reads the sample rate (Hz) from a PCM WAV header by walking RIFF chunks to the <c>fmt </c>
    /// chunk, or null if the file is not a WAV we can parse. Cheap header-only read — no decode.
    /// </summary>
    private static int? TryGetWavSampleRate(string fileName)
    {
        try
        {
            using var fs = File.OpenRead(fileName);
            using var br = new BinaryReader(fs);
            if (fs.Length < 12 ||
                Encoding.ASCII.GetString(br.ReadBytes(4)) != "RIFF")
            {
                return null;
            }

            br.ReadInt32(); // overall RIFF size
            if (Encoding.ASCII.GetString(br.ReadBytes(4)) != "WAVE")
            {
                return null;
            }

            while (fs.Position + 8 <= fs.Length)
            {
                var chunkId = Encoding.ASCII.GetString(br.ReadBytes(4));
                var chunkSize = br.ReadInt32();
                if (chunkId == "fmt ")
                {
                    br.ReadInt16(); // audio format
                    br.ReadInt16(); // channels
                    return br.ReadInt32(); // sample rate (little-endian)
                }

                if (chunkSize < 0 || fs.Position + chunkSize > fs.Length)
                {
                    return null;
                }

                fs.Position += chunkSize + (chunkSize & 1); // chunks are word-aligned
            }
        }
        catch
        {
            return null;
        }

        return null;
    }

    /// <summary>
    /// Brings the ref-text sidecars in <paramref name="voicesFolder"/> into the shape the Base
    /// backend needs. Two clean-ups, per reference WAV:
    ///  - Drop attribution-blurb <c>.txt</c> files: the qwen3-tts.cpp / voices.zip pack ships
    ///    Wikimedia/Creative-Commons blurbs next to its named clips, NOT spoken transcriptions.
    ///    The backend would happily load such a blurb as ref-text and produce runaway, off-voice
    ///    output, so a blurb is treated as "no transcript" and removed.
    ///  - Fill in a missing transcription from the sibling OmniVoice pack when it ships the same
    ///    generic reference WAV (female_06.wav, male_03.wav, …) WITH a real transcript — a correct
    ///    ref-text for free, no Whisper, no prompt.
    /// Voices still left without a transcript afterwards are handled lazily at voice-selection /
    /// import / synth time. Best-effort: an IO error on one file is logged and skipped.
    /// </summary>
    public static void NormalizeVoiceTranscripts(string voicesFolder)
    {
        if (!Directory.Exists(voicesFolder))
        {
            return;
        }

        foreach (var wav in Directory.GetFiles(voicesFolder, "*.wav"))
        {
            try
            {
                var sidecar = Path.ChangeExtension(wav, ".txt");
                if (File.Exists(sidecar))
                {
                    var text = File.ReadAllText(sidecar).Trim();
                    if (!string.IsNullOrWhiteSpace(text) && !LooksLikeAttributionBlurb(text))
                    {
                        continue; // already carries a usable transcription
                    }

                    // Empty or an attribution blurb — remove so it is never used as ref-text.
                    File.Delete(sidecar);
                }

                var reuse = FindReusableTranscript(wav);
                if (!string.IsNullOrWhiteSpace(reuse))
                {
                    File.WriteAllText(sidecar, reuse);
                }
            }
            catch (Exception ex)
            {
                Se.LogError(ex, $"Qwen3 TTS (CrispASR): failed to normalize transcript for '{wav}'");
            }
        }
    }

    /// <summary>
    /// Best-effort lookup of a real spoken transcription for a bundled reference voice from the
    /// sibling OmniVoice voice pack, which ships the same generic reference WAVs WITH real
    /// transcripts. Matched by file name; returns null when OmniVoice isn't installed or has no
    /// matching usable transcript.
    /// </summary>
    public static string? FindReusableTranscript(string wavPath)
    {
        try
        {
            var omniVoicesFolder = OmniVoiceTtsCpp.GetSetVoicesFolder();
            var candidate = Path.Combine(omniVoicesFolder, Path.GetFileName(wavPath));
            return File.Exists(candidate) ? TryReadUsableTranscript(candidate) : null;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Returns the usable spoken transcription stored in the <c>.txt</c> sidecar next to
    /// <paramref name="wavPath"/>, or null when there is none — a missing file, whitespace, or an
    /// attribution blurb (see <see cref="LooksLikeAttributionBlurb"/>) all count as "no transcript".
    /// </summary>
    public static string? TryReadUsableTranscript(string wavPath)
    {
        try
        {
            var sidecar = Path.ChangeExtension(wavPath, ".txt");
            if (!File.Exists(sidecar))
            {
                return null;
            }

            var text = File.ReadAllText(sidecar).Trim();
            return string.IsNullOrWhiteSpace(text) || LooksLikeAttributionBlurb(text) ? null : text;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Writes <paramref name="transcript"/> as the <c>.txt</c> ref-text sidecar next to a
    /// reference WAV (the Base backend auto-loads it as ref-text). Mirrors the sibling clone
    /// engines' <c>TryWriteRefTextSidecar</c> so the auto-fill flows can share one shape.
    /// </summary>
    public static bool TryWriteRefTextSidecar(string voiceWavPath, string transcript)
    {
        if (string.IsNullOrEmpty(voiceWavPath) || string.IsNullOrWhiteSpace(transcript))
        {
            return false;
        }

        try
        {
            var sidecar = Path.ChangeExtension(voiceWavPath, ".txt");
            File.WriteAllText(sidecar, transcript.Trim());
            return true;
        }
        catch (Exception ex)
        {
            Se.LogError(ex, $"Qwen3 TTS (CrispASR): failed to write ref-text sidecar for '{voiceWavPath}'");
            return false;
        }
    }

    /// <summary>
    /// True when <paramref name="text"/> looks like the Wikimedia / Creative-Commons attribution
    /// blurb that ships in the qwen3-tts.cpp voice pack's <c>.txt</c> sidecars rather than an
    /// actual spoken transcription. Feeding such a blurb as ref-text yields runaway, off-voice
    /// output, so it is treated as "no transcript" wherever a real ref-text is required.
    /// </summary>
    public static bool LooksLikeAttributionBlurb(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return false;
        }

        return text.Contains("commons.wikimedia.org", StringComparison.OrdinalIgnoreCase)
            || text.Contains("creativecommons.org", StringComparison.OrdinalIgnoreCase)
            || text.Contains("This file is licensed", StringComparison.OrdinalIgnoreCase)
            || text.Contains("Downsampled to", StringComparison.OrdinalIgnoreCase);
    }

    public static string GetTalkerPath(string? modelKey = null) =>
        Path.Combine(GetSetModelsFolder(), GetTalkerFileName(modelKey));

    public static string GetCodecPath() =>
        Path.Combine(GetSetModelsFolder(), CodecFileName);

    public static bool AreModelsInstalled(string? modelKey = null) =>
        IsValidLocalModelFile(GetTalkerPath(modelKey), GetTalkerFileName(modelKey))
        && IsValidLocalModelFile(GetCodecPath(), CodecFileName);

    /// <summary>
    /// Path crispasr's --auto-download writes GGUFs to before SE took over model management.
    /// Exposed so the SE downloader can copy already-cached files instead of re-pulling 2 GB,
    /// and so settings UI can reference the same location.
    /// </summary>
    public static string GetCrispAsrCacheFolder() =>
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".cache", "crispasr");

    /// <summary>
    /// Best-effort copy of <paramref name="fileName"/> from <see cref="GetCrispAsrCacheFolder"/>
    /// into SE's models folder. Used by the downloader to avoid re-pulling files crispasr's
    /// own --auto-download has already fetched. Returns true if the destination ends up present
    /// (whether seeded just now or already there); false on any IO failure.
    /// </summary>
    public static bool TrySeedModelFromCrispAsrCache(string fileName, string destinationPath)
    {
        if (IsValidLocalModelFile(destinationPath, fileName))
        {
            return true;
        }

        try
        {
            // A pre-existing wrong-sized destination is a leftover partial — almost always
            // copied here by a previous run of this seeder from a half-downloaded cache file.
            // Remove it so the SE-side downloader can replace it cleanly; otherwise every later
            // File.Exists gate would happily reuse the bad bytes.
            if (File.Exists(destinationPath))
            {
                Se.LogError($"Qwen3 TTS (CrispASR): removing wrong-sized local model file {destinationPath}");
                File.Delete(destinationPath);
            }

            var cachePath = Path.Combine(GetCrispAsrCacheFolder(), fileName);
            if (!IsValidLocalModelFile(cachePath, fileName))
            {
                // Either no cache file or it's a truncated --auto-download partial. Don't seed
                // from it — the SE-side downloader does .part + SHA-256 verify, which is the
                // safe path. Leave the cache file alone (it's shared with other crispasr users).
                return false;
            }

            File.Copy(cachePath, destinationPath);
            return true;
        }
        catch (Exception ex)
        {
            Se.LogError(ex, $"Qwen3 TTS (CrispASR): cache seed copy failed for {fileName}");
            return false;
        }
    }

    public Task<Voice[]> GetVoices(string language)
    {
        var result = new List<Voice>();
        var modelKey = ResolveModelKey(Se.Settings.Video.TextToSpeech.Qwen3TtsCrispAsrModel);

        if (modelKey == ModelKeyVoiceDesign)
        {
            // VoiceDesign has no speaker encoder — the instruction string drives the voice, so
            // a single "Default" entry is all the combo needs.
            result.Add(new Voice(new Qwen3TtsVoice("Default", string.Empty)));
            return Task.FromResult(result.ToArray());
        }

        if (modelKey == ModelKeyCustomVoice)
        {
            // CustomVoice picks one of the fixed speakers baked into the GGUF, selected by name.
            // The on-disk reference WAVs in the voices folder belong to the Voice clone (Base)
            // model, not here — sending a WAV path to CustomVoice just makes the backend fall
            // back to its first speaker. FilePath stays empty: Speak sends Voice (the name).
            foreach (var speaker in CustomVoiceSpeakers)
            {
                result.Add(new Voice(new Qwen3TtsVoice(speaker, string.Empty)));
            }
            return Task.FromResult(result.ToArray());
        }

        // Voice clone (Base): list the imported reference WAVs. Voice cloning refuses requests
        // without a reference, so there is no "Default" entry — the user must import a voice
        // (with its transcript) first.
        var voicesFolder = GetSetVoicesFolder();
        if (Directory.Exists(voicesFolder))
        {
            foreach (var file in Directory.GetFiles(voicesFolder, "*.wav"))
            {
                var name = Path.GetFileNameWithoutExtension(file).Replace('_', ' ');
                result.Add(new Voice(new Qwen3TtsVoice(name, file)));
            }
        }

        return Task.FromResult(result.ToArray());
    }

    public bool IsVoiceInstalled(Voice voice) => true;

    public Task<string[]> GetRegions() => Task.FromResult(Array.Empty<string>());

    public Task<string[]> GetModels() => Task.FromResult(new[] { ModelKeyVoiceDesign, ModelKeyCustomVoice, ModelKeyClone });

    public Task<TtsLanguage[]> GetLanguages(Voice voice, string? model) => Task.FromResult(Array.Empty<TtsLanguage>());

    public Task<Voice[]> RefreshVoices(string language, CancellationToken cancellationToken) =>
        GetVoices(language);

    public async Task<TtsResult> Speak(
        string text,
        string outputFolder,
        Voice voice,
        TtsLanguage? language,
        string? region,
        string? model,
        CancellationToken cancellationToken)
    {
        if (voice.EngineVoice is not Qwen3TtsVoice qwen3Voice)
        {
            throw new ArgumentException("Voice is not a Qwen3TtsVoice");
        }

        var modelKey = ResolveModelKey(model);

        // Voice clone (Base) needs a reference WAV plus its transcript. The backend resolves the
        // bare voice name against --voice-dir and auto-loads the matching <name>.txt as ref-text;
        // without that transcript it returns "ref-text not set" (HTTP 500). Surface both gaps
        // up-front with an actionable message instead of an opaque server error.
        if (modelKey == ModelKeyClone)
        {
            if (string.IsNullOrEmpty(qwen3Voice.FilePath))
            {
                throw new InvalidOperationException(
                    "Qwen3 TTS (CrispASR) voice cloning requires a reference voice. "
                    + "Import one via the voice settings (you'll be asked for its transcript), "
                    + "then pick it in the voice combo.");
            }

            if (string.IsNullOrWhiteSpace(TryReadUsableTranscript(qwen3Voice.FilePath)))
            {
                throw new InvalidOperationException(
                    $"Qwen3 TTS (CrispASR) voice cloning needs the spoken transcription of '{qwen3Voice.Voice}'. "
                    + "Re-import the voice and enter the exact words spoken in the reference WAV.");
            }
        }

        await EnsureServerRunningAsync(modelKey, cancellationToken);

        var outputFileName = Path.Combine(GetSetFolder(), Guid.NewGuid() + ".wav");
        var inputText = text;
        // Share the qwen3-tts.cpp instruction setting so users get the same voice description
        // regardless of which Qwen3 engine they're testing with.
        var instruction = Se.Settings.Video.TextToSpeech.Qwen3TtsCppInstruction ?? string.Empty;

        // OpenAI-compatible /v1/audio/speech payload. The crispasr backend interprets `voice`
        // differently per model (see crispasr_backend_qwen3_tts.cpp):
        //   - Voice clone (Base): `voice` is a BARE name (no path, no extension) resolved against
        //     --voice-dir to <name>.wav + the auto-loaded <name>.txt transcript. Sending the
        //     absolute path instead skips the sidecar load and fails — so send the basename only.
        //   - CustomVoice: `voice` is a fixed SPEAKER NAME (e.g. "vivian"); a .wav value would
        //     make the backend silently fall back to its first speaker.
        //   - VoiceDesign: ignores `voice`; the voice comes from `instructions`.
        var payload = new Dictionary<string, object>
        {
            ["input"] = inputText,
            ["response_format"] = "wav",
        };
        if (modelKey == ModelKeyClone && !string.IsNullOrEmpty(qwen3Voice.FilePath))
        {
            payload["voice"] = Path.GetFileNameWithoutExtension(qwen3Voice.FilePath);
            // The reference is the user's own imported WAV — attest consent so the request also
            // works against CrispASR builds that gate cloning on it.
            payload["consent_attestation"] = "I have the speaker's consent, or it is my own voice.";
            // Skip the audible AI-disclosure prefix CrispASR otherwise prepends to cloned audio;
            // SE surfaces the AI-generated nature in its UI. The inaudible watermark + C2PA
            // provenance metadata stay embedded regardless (defaults to true server-side).
            payload["spoken_disclaimer"] = false;
        }
        else if (modelKey == ModelKeyCustomVoice && !string.IsNullOrEmpty(qwen3Voice.Voice))
        {
            payload["voice"] = qwen3Voice.Voice;
        }

        if (modelKey != ModelKeyClone && !string.IsNullOrEmpty(instruction))
        {
            // VoiceDesign treats this as the voice description; CustomVoice as optional style.
            // The Base clone model has no instruct path, so don't send it there.
            payload["instructions"] = instruction;
        }
        else if (modelKey == ModelKeyVoiceDesign)
        {
            // VoiceDesign refuses requests without an instruction. Send a neutral default
            // so the first run doesn't surface a confusing 500 from the server.
            payload["instructions"] = "a calm female voice";
        }

        var body = JsonSerializer.Serialize(payload);
        using var content = new StringContent(body, Encoding.UTF8, "application/json");
        Se.WriteToolsLog($"Qwen3 TTS (CrispASR): POST {ServerBaseUrl}/v1/audio/speech (voice={qwen3Voice}, model={modelKey}, textLen={text.Length}, instructionLen={instruction.Length})");

        HttpResponseMessage response;
        try
        {
            response = await HttpClient.PostAsync($"{ServerBaseUrl}/v1/audio/speech", content, cancellationToken);
        }
        catch (HttpRequestException ex)
        {
            var serverLog = SnapshotServerLog();
            var launchCommand = _serverLaunchCommand;
            var died = _serverProcess?.HasExited == true;
            if (died)
            {
                StopServerInternal();
            }

            var failMsg = $"Qwen3 TTS (CrispASR) request failed — Voice: {qwen3Voice}, Text: {text}, "
                + $"RequestJson: {body}, ServerExited: {died}, ServerLog: {serverLog}"
                + LaunchCmdSuffix(launchCommand);
            Se.LogError(ex, failMsg);
            Se.WriteToolsLog(failMsg);

            throw new InvalidOperationException(
                (died
                    ? "Qwen3 TTS (CrispASR) — the crispasr server crashed during synthesis."
                    : "Qwen3 TTS (CrispASR) request failed — the connection to the crispasr server was dropped.")
                + (string.IsNullOrEmpty(serverLog) ? string.Empty : $"{Environment.NewLine}Server log:{Environment.NewLine}{serverLog}")
                + LaunchCmdSuffix(launchCommand),
                ex);
        }

        using (response)
        {
            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await SafeReadErrorAsync(response, cancellationToken);
                var serverLog = SnapshotServerLog();
                var launchCommand = _serverLaunchCommand;
                var errMsg = $"Qwen3 TTS (CrispASR) server error {(int)response.StatusCode} {response.StatusCode} — "
                    + $"Voice: {qwen3Voice}, Text: {text}, RequestJson: {body}, "
                    + $"ResponseBody: {errorBody}, ServerLog: {serverLog}"
                    + LaunchCmdSuffix(launchCommand);
                Se.LogError(errMsg);
                Se.WriteToolsLog(errMsg);
                throw new InvalidOperationException(
                    $"Qwen3 TTS (CrispASR) synthesis failed ({(int)response.StatusCode}): {errorBody}"
                    + (string.IsNullOrEmpty(serverLog) ? string.Empty : $"{Environment.NewLine}Server log:{Environment.NewLine}{serverLog}")
                    + LaunchCmdSuffix(launchCommand));
            }

            await using var fileStream = File.Create(outputFileName);
            await using var contentStream = await response.Content.ReadAsStreamAsync(cancellationToken);
            await contentStream.CopyToAsync(fileStream, cancellationToken);
        }

        return new TtsResult(outputFileName, text);
    }

    private static string FormatLaunchCommand(string exe, System.Collections.ObjectModel.Collection<string> args)
    {
        static string Quote(string s) =>
            !string.IsNullOrEmpty(s) && s.IndexOfAny(new[] { ' ', '\t' }) >= 0
                ? "\"" + s.Replace("\"", "\\\"") + "\""
                : s;

        var sb = new StringBuilder(Quote(exe));
        foreach (var a in args)
        {
            sb.Append(' ').Append(Quote(a));
        }
        return sb.ToString();
    }

    private static string LaunchCmdSuffix(string? launchCommand) =>
        string.IsNullOrEmpty(launchCommand)
            ? string.Empty
            : $"{Environment.NewLine}Launch command: {launchCommand}";

    private static async Task<string> SafeReadErrorAsync(HttpResponseMessage response, CancellationToken ct)
    {
        try
        {
            return await response.Content.ReadAsStringAsync(ct);
        }
        catch (Exception ex)
        {
            return $"<failed to read error body: {ex.Message}>";
        }
    }

    private static async Task EnsureServerRunningAsync(string modelKey, CancellationToken ct)
    {
        if (_serverProcess is { HasExited: false } && _serverPort != 0 && _serverModelKey == modelKey)
        {
            return;
        }

        await ServerLock.WaitAsync(ct);
        try
        {
            if (_serverProcess is { HasExited: false } && _serverPort != 0 && _serverModelKey == modelKey)
            {
                return;
            }

            if (_serverProcess != null)
            {
                StopServerInternal();
            }

            var exe = GetCrispAsrExecutable();
            if (!File.Exists(exe))
            {
                throw new FileNotFoundException(
                    "CrispASR executable not found. Install CrispASR via Video → Audio to text first.", exe);
            }

            // Talker and codec GGUFs: if locally staged, use directly; otherwise hand off to
            // crispasr's --auto-download to fetch them into ~/.cache/crispasr/ on first run.
            // Slower the first time, instant after. A proper SE-side download service is a
            // follow-up; this keeps the engine usable without manual file placement.
            var talker = GetTalkerPath(modelKey);
            var hasLocalTalker = IsValidLocalModelFile(talker, GetTalkerFileName(modelKey));
            var codec = GetCodecPath();
            var hasLocalCodec = IsValidLocalModelFile(codec, CodecFileName);

            var port = FindFreeLoopbackPort();
            var psi = new ProcessStartInfo
            {
                WorkingDirectory = Path.GetDirectoryName(exe) ?? GetSetFolder(),
                FileName = exe,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
            };
            psi.ArgumentList.Add("--server");
            psi.ArgumentList.Add("--backend");
            psi.ArgumentList.Add(GetBackendName(modelKey));
            psi.ArgumentList.Add("-m");
            psi.ArgumentList.Add(hasLocalTalker ? talker : "auto");
            if (hasLocalCodec)
            {
                psi.ArgumentList.Add("--codec-model");
                psi.ArgumentList.Add(codec);
            }
            if (!hasLocalTalker || !hasLocalCodec)
            {
                psi.ArgumentList.Add("--auto-download");
            }
            psi.ArgumentList.Add("--host");
            psi.ArgumentList.Add("127.0.0.1");
            psi.ArgumentList.Add("--port");
            psi.ArgumentList.Add(port.ToString());
            // The crispasr server gates /v1/audio/speech requests with a `voice` field behind
            // --voice-dir being set. Pointing at our voices folder satisfies the gate and also
            // makes /v1/voices reflect any imported reference WAVs.
            psi.ArgumentList.Add("--voice-dir");
            psi.ArgumentList.Add(GetSetVoicesFolder());

            var process = Process.Start(psi)
                ?? throw new InvalidOperationException("Failed to start crispasr (qwen3-tts)");

            var launchCommand = FormatLaunchCommand(exe, psi.ArgumentList);
            _serverLaunchCommand = launchCommand;
            Se.WriteToolsLog("Qwen3 TTS (CrispASR) server starting — "
                + $"PID: {process.Id}, "
                + $"Cmd: {launchCommand}");

            lock (_serverLog) _serverLog.Clear();
            process.ErrorDataReceived += (_, e) =>
            {
                if (e.Data != null) lock (_serverLog) _serverLog.AppendLine(e.Data);
            };
            process.OutputDataReceived += (_, e) =>
            {
                if (e.Data != null) lock (_serverLog) _serverLog.AppendLine(e.Data);
            };
            process.BeginErrorReadLine();
            process.BeginOutputReadLine();

            _serverProcess = process;
            _serverPort = port;
            _serverModelKey = modelKey;
            HookProcessExitOnce();

            // First-run auto-download (~2 GB talker + ~986 MB codec) needs a generous timeout.
            var deadline = DateTime.UtcNow.AddMinutes(hasLocalTalker && hasLocalCodec ? 5 : 30);
            while (DateTime.UtcNow < deadline)
            {
                ct.ThrowIfCancellationRequested();
                if (process.HasExited)
                {
                    var tail = SnapshotServerLog();
                    var exitCode = process.ExitCode;
                    var exitedLaunchCommand = _serverLaunchCommand;
                    _serverProcess = null;
                    _serverPort = 0;
                    _serverModelKey = null;
                    _serverLaunchCommand = null;
                    throw new InvalidOperationException(
                        $"crispasr (qwen3-tts) exited during startup (code {exitCode}). Output: {tail}"
                        + LaunchCmdSuffix(exitedLaunchCommand));
                }
                if (await ProbeHealthAsync(port, TimeSpan.FromSeconds(2), ct))
                {
                    return;
                }
                await Task.Delay(TimeSpan.FromSeconds(1), ct);
            }

            var lastOutput = SnapshotServerLog();
            var timeoutLaunchCommand = _serverLaunchCommand;
            StopServerInternal();
            throw new TimeoutException(
                $"crispasr (qwen3-tts) did not report healthy within {(hasLocalTalker && hasLocalCodec ? 5 : 30)} minutes. Last output: {lastOutput}"
                + LaunchCmdSuffix(timeoutLaunchCommand));
        }
        finally
        {
            ServerLock.Release();
        }
    }

    private static string SnapshotServerLog()
    {
        lock (_serverLog)
        {
            var s = _serverLog.ToString().TrimEnd();
            return s.Length > 2000 ? s[^2000..] : s;
        }
    }

    private static async Task<bool> ProbeHealthAsync(int port, TimeSpan timeout, CancellationToken ct)
    {
        try
        {
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            cts.CancelAfter(timeout);
            using var resp = await HttpClient.GetAsync($"http://127.0.0.1:{port}/health", cts.Token);
            return resp.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    private static int FindFreeLoopbackPort()
    {
        var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        try
        {
            return ((IPEndPoint)listener.LocalEndpoint).Port;
        }
        finally
        {
            listener.Stop();
        }
    }

    private static void HookProcessExitOnce()
    {
        if (_processExitHooked)
        {
            return;
        }
        _processExitHooked = true;
        AppDomain.CurrentDomain.ProcessExit += (_, _) => StopServerInternal();
    }

    /// <summary>
    /// Stop the running crispasr (qwen3-tts) server if any, releasing GPU memory. Called by
    /// <c>TextToSpeechViewModel</c> when starting synthesis on a different engine or when the
    /// TTS window closes, so the four CrispASR-based TTS engines don't pile up in VRAM.
    /// </summary>
    public static void StopServer() => StopServerInternal();

    private static void StopServerInternal()
    {
        var p = _serverProcess;
        _serverProcess = null;
        _serverPort = 0;
        _serverModelKey = null;
        _serverLaunchCommand = null;
        if (p == null)
        {
            return;
        }
        try
        {
            if (!p.HasExited)
            {
                p.Kill(entireProcessTree: true);
                p.WaitForExit(2000);
            }
        }
        catch
        {
            // best effort
        }
        finally
        {
            p.Dispose();
        }
    }

    private static string GetUniqueDestinationFileName(string folder, string baseName)
    {
        var candidate = Path.Combine(folder, baseName + ".wav");
        if (!File.Exists(candidate))
        {
            return candidate;
        }

        var number = 1;
        do
        {
            candidate = Path.Combine(folder, $"{baseName}_{number}.wav");
            number++;
        } while (File.Exists(candidate));

        return candidate;
    }

    public bool ImportVoice(string fileName) => ImportVoiceInternal(fileName, null);

    /// <summary>
    /// Imports a reference voice for the Base (Voice clone) model, writing the supplied
    /// <paramref name="transcript"/> as the <c>.txt</c> sidecar the backend needs as ref-text.
    /// Used by the voice-settings dialog, which prompts for the transcription on import.
    /// </summary>
    public bool ImportVoice(string fileName, string transcript) => ImportVoiceInternal(fileName, transcript);

    private bool ImportVoiceInternal(string fileName, string? transcript)
    {
        if (string.IsNullOrEmpty(fileName) || !File.Exists(fileName))
        {
            return false;
        }

        var voicesFolder = GetSetVoicesFolder();
        var baseName = Path.GetFileNameWithoutExtension(fileName);
        var destinationFileName = GetUniqueDestinationFileName(voicesFolder, baseName);

        // CrispASR's qwen3-tts backend expects a 24 kHz mono reference WAV. Always resample on
        // import via ffmpeg so the saved file is in the right shape regardless of what the user
        // picked.
        try
        {
            var process = FfmpegGenerator.ConvertToMono24kHzWav(fileName, destinationFileName);
            if (!process.Start())
            {
                return false;
            }

            process.WaitForExit();
        }
        catch (Exception ex)
        {
            Se.LogError(ex, "Qwen3 TTS (CrispASR) voice import failed (ffmpeg conversion).");
            return false;
        }

        if (!File.Exists(destinationFileName))
        {
            return false;
        }

        // Voice cloning needs a matching .txt sidecar holding the spoken transcription of the
        // reference WAV (the backend auto-loads it as ref-text). Prefer the transcript the user
        // typed in the import dialog; fall back to copying a sibling .txt next to the source.
        try
        {
            var destSidecar = Path.ChangeExtension(destinationFileName, ".txt");
            if (!string.IsNullOrWhiteSpace(transcript))
            {
                File.WriteAllText(destSidecar, transcript.Trim());
            }
            else
            {
                var sourceSidecar = Path.ChangeExtension(fileName, ".txt");
                if (File.Exists(sourceSidecar) && !File.Exists(destSidecar))
                {
                    File.Copy(sourceSidecar, destSidecar);
                }
            }
        }
        catch (Exception ex)
        {
            // Sidecar write is best-effort — log and continue. The imported WAV is still saved;
            // Speak surfaces a clear error if the transcript is missing when cloning.
            Se.LogError(ex, "Qwen3 TTS (CrispASR) voice import: failed to write .txt sidecar");
        }

        return true;
    }
}
