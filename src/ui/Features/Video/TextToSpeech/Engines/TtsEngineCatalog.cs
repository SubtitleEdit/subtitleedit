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
        engines.Add(new OpenAiCompatibleSpeech(ttsDownloadService));
        engines.Add(new Murf(ttsDownloadService));
        engines.Add(new GoogleSpeech(ttsDownloadService));
        engines.Add(new KokoroTtsCpp());

        // Supertonic-3 (CrispASR) — ten fixed preset voices and no cloning, so it sits with the
        // fixed-voice engines rather than in the CrispASR cloning group below. 31 languages from
        // one 200 MB GGUF, and by far the fastest local engine here (RTF ~0.09 on an M4).
        engines.Add(new SupertonicCrispAsr());

        // Voxtral 4B TTS (audio.cpp) — 20 preset voices and no cloning, nine languages (English,
        // French, German, Spanish, Italian, Portuguese, Dutch, Hindi, Arabic). ~1.35x real time
        // on an M4 and ~9.5 GB resident; CC BY-NC 4.0, so it shows the licence gate first.
        engines.Add(new VoxtralTtsAudioCpp());

        // Zonos (CrispASR) — ZONOS3-Mini with Zyphra's preset voices. Not a cloning engine: the
        // zonos3 backend cannot make a speaker embedding from a WAV yet. Move it into
        // CreateVoiceCloningEngines when upstream wires its speaker encoder.
        engines.Add(new ZonosTtsCrispAsr());

        engines.AddRange(CreateVoiceCloningEngines());

        return engines;
    }

    /// <summary>
    /// The engines that can clone a speaker from a reference recording, i.e. the ones whose
    /// <see cref="ITtsEngine.SupportsVoiceCloning"/> is true - in display order, and never
    /// including an engine that <see cref="CreateAll"/> hides.
    /// </summary>
    /// <remarks>
    /// Ordered by published cloning quality, best first, regardless of runtime (CrispASR,
    /// audio.cpp, tts.cpp). The figures are each model's own Seed-TTS-eval test-en numbers
    /// (WER / speaker similarity), so they are vendor-reported and not measured in SE. Engines with
    /// both numbers come first, ranked by similarity with WER as the tie-break; then the ones that
    /// publish only WER, then only human-preference results, then nothing comparable. Speed is not
    /// part of the order - dots.tts and Confucius4 are ~7x slower than real time on an M4.
    /// </remarks>
    public static List<ITtsEngine> CreateVoiceCloningEngines()
    {
        return
        [
            // dots.tts SOAR 2B (CrispASR) - 1.30 / 77.1, the best average similarity on
            // Seed-TTS-eval (79.2). Continuous-latent AR model with a flow-matching DiT head and a
            // 48 kHz BigVGAN vocoder, Apache-2.0. Clones from a CAM++ speaker embedding so no
            // reference transcript is needed.
            new DotsTtsCrispAsr(),

            // FireRedTTS3-Base (audio.cpp) - 1.64 / 77.2 (arXiv 2608.17492). Cloning across 24
            // languages; weights are Apache-2.0, so no licence gate. Needs an explicit language
            // pick (no detection; audio.cpp defaults to Chinese).
            new FireRedTts3AudioCpp(),

            // VoxCPM2 (CrispASR) - 1.84 / 75.3. Unlike f5-tts, the voxcpm2-tts backend has
            // Metal/CUDA in CrispASR v0.7.0, so synthesis is fast enough for the
            // TTS-from-subtitles workflow.
            new VoxCPM2CrispAsr(),

            // OmniVoice - 1.60 / 74.1 (arXiv 2604.00688). The CrispASR build runs it as a
            // persistent server, so the model loads once instead of once per line; the standalone
            // omnivoice-tts CLI build follows right after.
            new OmniVoiceCrispAsr(),
            new OmniVoiceTtsCpp(),

            // Qwen3-TTS 1.7B (CrispASR) - 1.24 / 71.4. Qwen3TtsCpp hidden: talker produces
            // scrambled noise on 1.7B - use Qwen3TtsCrispAsr until upstream qwen3-tts.cpp is fixed.
            new Qwen3TtsCrispAsr(),

            // CosyVoice3 0.5B (CrispASR) - 2.02 / 71.8 (arXiv 2505.17589).
            new CosyVoice3CrispAsr(),

            // Confucius4-TTS (CrispASR) - 1.49 / 70.0. NetEase Youdao's two-stage
            // GPT-2/flow-matching model, 22.05 kHz, 14 languages, Apache-2.0. Zero-shot cloning
            // is mandatory (no default voice exists); clones from w2v-BERT + CAM++ conditioning so
            // no reference transcript is needed.
            new Confucius4TtsCrispAsr(),

            // IndexTTS 2.5 (audio.cpp) - the report gives WER and similarity "comparable to
            // IndexTTS 2" (2.23 / 70.6). 5 languages, emotion control and speaking-rate control,
            // with a per-request reference voice so the server is not restarted when the voice
            // changes.
            new IndexTts25AudioCpp(),

            // VibeVoiceCrispAsr - 1.5B, 3.04 / 68.9. It was hidden while its output quality was
            // judged unusable. That was measured on the CrispASR build of the time (v0.8.29);
            // re-checked by ear on v0.8.31 with the same 1.5B q4_k weights and the same request SE
            // sends (speed 1.1), the output is acceptable, so it is back. The old note cited
            // #11223 as the evidence, but that is the speed-slider PR - it set the 1.1 default, it
            // never assessed quality.
            new VibeVoiceCrispAsr(),

            // WER only, no comparable similarity published.

            // Higgs Audio v3 (audio.cpp) - WER 1.11 on its own model card. Boson AI's 4B model,
            // cloning across 100+ languages, the broadest coverage of any engine here. Weights
            // are research / non-commercial licensed (first-run accept window).
            new HiggsTtsAudioCpp(),

            // Fish Audio S2 Pro (audio.cpp) - WER 0.99 in its own report, but a third-party run
            // measured similarity at only 64.6. Cloning from a 10-30 s reference across 80+
            // languages with inline prosody/emotion control. Weights are under the Fish Audio
            // Research License (first-run accept window).
            new FishTtsAudioCpp(),

            // MOSS-TTS v1.5 (CrispASR) - WER 1.68. Qwen3-8B backbone + 1.6B transformer codec at
            // 24 kHz with zero-shot voice cloning, via the moss-tts backend (#12617).
            new MossTtsCrispAsr(),

            // Human-preference results only.

            // Breeze TTS 2 (audio.cpp) - top open-weights model in the Artificial Analysis blind
            // listening arena, but English and Chinese only. Voice design from a free-text
            // description and cloning steered by the same instruction. Weights and output are
            // research / non-commercial (first-run accept window).
            new BreezeTtsAudioCpp(),

            // KugelAudio-0-Open (audio.cpp) - only a German human-preference test is published.
            // Four preset voices plus reference-audio cloning (audio.cpp v0.9.5, with an acoustic
            // encoder SE downloads beside the GGUF), 23 European languages (Danish, Swedish,
            // Norwegian, Finnish, Polish...) from a 7B model. Slow (3-4x real time on an M4) and
            // ~9 GB resident, but MIT-licensed.
            new KugelAudioAudioCpp(),

            // No comparable published numbers.

            // IndexTTS-1.5 (CrispASR) - the generation before IndexTTS 2 / 2.5 above.
            new IndexTtsCrispAsr(),

            // Pocket TTS (CrispASR) - Kyutai's 100M model, the smallest and fastest cloning
            // engine here (one 124-365 MB GGUF per language, RTF ~0.1 on CPU). Six languages;
            // per-request reference resolution, so voice changes need no server restart.
            new PocketTtsCrispAsr(),

            new ChatterboxTtsCpp(),

            // F5-TTS (CrispASR) hidden: CrispASR 0.6.12 has no GPU backend for f5-tts, so
            // synthesis runs the fixed 32-step Euler ODE through a 22-layer DiT + Vocos on
            // CPU only. That's 3-8 minutes per short utterance on Mac CPU - unusable for the
            // typical TTS-from-subtitles workflow. Engine + download service + settings dialog
            // are kept so this is a one-line re-enable when upstream CrispASR adds Metal/CUDA
            // support or exposes an --ode-steps flag.
            //new F5TtsCrispAsr(),
        ];
    }
}
