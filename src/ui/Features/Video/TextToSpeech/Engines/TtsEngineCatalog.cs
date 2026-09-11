using Nikse.SubtitleEdit.Logic.Download;
using System;
using System.Collections.Generic;

namespace Nikse.SubtitleEdit.Features.Video.TextToSpeech.Engines;

/// <summary>
/// The text-to-speech engines SE offers, in the order they are shown.
/// </summary>
/// <remarks>
/// One list, because the engines are now reached from more than one place: the TTS window's engine
/// combo takes all of them, while the waveform's "Clone voice to" menu takes only the cloning ones.
/// A second hand-written list would quietly keep offering an engine that was disabled here (see the
/// commented-out entries below - those are decisions, not leftovers).
/// </remarks>
public static class TtsEngineCatalog
{
    /// <summary>
    /// Every engine, in display order: Piper first where it exists, then the cloud/local engines
    /// with fixed voices, then the cloning engines.
    /// </summary>
    public static List<ITtsEngine> CreateAll(ITtsDownloadService ttsDownloadService)
    {
        var engines = new List<ITtsEngine>();

        if (!OperatingSystem.IsMacOS())
        {
            engines.Add(new Piper(ttsDownloadService));
        }

        engines.Add(new EdgeTts());
        engines.Add(new AllTalk(ttsDownloadService));
        engines.Add(new ElevenLabs(ttsDownloadService));
        engines.Add(new AzureSpeech(ttsDownloadService));
        engines.Add(new MistralSpeech(ttsDownloadService));
        engines.Add(new Murf(ttsDownloadService));
        engines.Add(new GoogleSpeech(ttsDownloadService));
        engines.Add(new KokoroTtsCpp());

        engines.AddRange(CreateVoiceCloningEngines());

        return engines;
    }

    /// <summary>
    /// The engines that can clone a speaker from a reference recording, i.e. the ones whose
    /// <see cref="ITtsEngine.SupportsVoiceCloning"/> is true - in display order, and never
    /// including an engine that <see cref="CreateAll"/> hides.
    /// </summary>
    public static List<ITtsEngine> CreateVoiceCloningEngines()
    {
        return
        [
            new OmniVoiceTtsCpp(),

            // CrispASR-based engines grouped at the bottom: both share the same heavy CrispASR
            // runtime download (~hundreds of MB) and are typically picked together, so we group
            // them last so the lighter cloud/local engines surface first in the list.
            // Qwen3TtsCpp hidden: talker produces scrambled noise on 1.7B —
            // use Qwen3TtsCrispAsr until upstream qwen3-tts.cpp is fixed.
            new Qwen3TtsCrispAsr(),

            // VibeVoiceCrispAsr was hidden while its output quality was judged unusable. That
            // was measured on the CrispASR build of the time (v0.8.29); re-checked by ear on
            // v0.8.31 with the same 1.5B q4_k weights and the same request SE sends (speed 1.1),
            // the output is acceptable, so it is back. The old note cited #11223 as the evidence,
            // but that is the speed-slider PR - it set the 1.1 default, it never assessed quality.
            new VibeVoiceCrispAsr(),

            new IndexTtsCrispAsr(),

            // dots.tts SOAR 2B (CrispASR) — continuous-latent AR model with a flow-matching DiT
            // head and a 48 kHz BigVGAN vocoder, Apache-2.0. Grouped with the other CrispASR
            // engines; clones from a CAM++ speaker embedding so no reference transcript is needed.
            new DotsTtsCrispAsr(),

            // Confucius4-TTS (CrispASR) — NetEase Youdao's two-stage GPT-2/flow-matching model,
            // 22.05 kHz, 14 languages, Apache-2.0. Zero-shot cloning is mandatory (no default
            // voice exists); clones from w2v-BERT + CAM++ conditioning so no reference
            // transcript is needed.
            new Confucius4TtsCrispAsr(),

            // CosyVoice3 (CrispASR) sits immediately after IndexTtsCrispAsr to keep the CrispASR
            // engines grouped visually in the engine combo.
            new CosyVoice3CrispAsr(),

            // Pocket TTS (CrispASR) — Kyutai's 100M model, the smallest and fastest cloning
            // engine here (one 124-365 MB GGUF per language, RTF ~0.1 on CPU). Six languages;
            // per-request reference resolution, so voice changes need no server restart.
            new PocketTtsCrispAsr(),

            // IndexTTS 2.5 on the audio.cpp runtime (not CrispASR): 5 languages, emotion
            // control and speaking-rate control, with a per-request reference voice so the
            // server is not restarted when the voice changes.
            new IndexTts25AudioCpp(),

            // Higgs Audio v3 (audio.cpp) — Boson AI's 4B model, cloning across 100+ languages,
            // the broadest coverage of any engine here. Shares the audio.cpp runtime install
            // with IndexTTS 2.5, so it is grouped right after it. Weights are research /
            // non-commercial licensed (first-run accept window, like IndexTTS 2.5's).
            new HiggsTtsAudioCpp(),

            // Fish Audio S2 Pro (audio.cpp) — cloning from a 10-30 s reference across 80+
            // languages with inline prosody/emotion control. Same shared runtime; weights are
            // under the Fish Audio Research License (first-run accept window).
            new FishTtsAudioCpp(),

            // FireRedTTS3-Base (audio.cpp) — cloning across 24 languages with the best published
            // speaker similarity of the open models (arXiv 2608.17492). Same shared runtime;
            // weights are Apache-2.0, so no licence gate. Needs an explicit language pick
            // (no detection; audio.cpp defaults to Chinese).
            new FireRedTts3AudioCpp(),

            // F5-TTS (CrispASR) hidden: CrispASR 0.6.12 has no GPU backend for f5-tts, so
            // synthesis runs the fixed 32-step Euler ODE through a 22-layer DiT + Vocos on
            // CPU only. That's 3-8 minutes per short utterance on Mac CPU — unusable for the
            // typical TTS-from-subtitles workflow. Engine + download service + settings dialog
            // are kept so this is a one-line re-enable when upstream CrispASR adds Metal/CUDA
            // support or exposes an --ode-steps flag.
            //new F5TtsCrispAsr(),

            // VoxCPM2 (CrispASR) — unlike f5-tts, the voxcpm2-tts backend has Metal/CUDA in
            // CrispASR v0.7.0, so synthesis is fast enough for the TTS-from-subtitles workflow.
            new VoxCPM2CrispAsr(),

            // MOSS-TTS (CrispASR) — Qwen3-8B backbone + 1.6B transformer codec at 24 kHz with
            // zero-shot voice cloning, via the moss-tts backend (#12617).
            new MossTtsCrispAsr(),

            new ZonosTtsCrispAsr(),

            // OmniVoice (CrispASR) — the same model family as the standalone OmniVoice TTS
            // above, but on the shared CrispASR runtime and as a persistent server, so the
            // model loads once instead of once per line.
            new OmniVoiceCrispAsr(),

            new ChatterboxTtsCpp(),
        ];
    }
}
