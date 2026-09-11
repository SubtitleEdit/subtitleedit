using System.Collections.Generic;
using Nikse.SubtitleEdit.Features.Video.TextToSpeech.ActorVoices;

namespace Nikse.SubtitleEdit.Logic.Config;

public class SeVideoTextToSpeech
{
    public string Engine { get; set; }
    public string Voice { get; set; }
    public bool ReviewAudioClips { get; set; }
    public bool CustomAudio { get; set; }
    public bool CustomAudioStereo { get; set; }
    public string CustomAudioEncoding { get; set; }
    public bool GenerateVideoFile { get; set; }
    public string VoiceTestText { get; set; }
    public string AllTalkUrl { get; set; }
    public string AzureApiKey { get; set; }
    public string AzureRegion { get; set; }
    public string ElevenLabsApiKey { get; set; }
    public string ElevenLabsModel { get; set; }
    public string ElevenLabsLanguage { get; set; }
    public double ElevenLabsStability { get; set; }
    public double ElevenLabsSimilarity { get; set; }
    public double ElevenLabsSpeakerBoost { get; set; }
    public double ElevenLabsSpeed { get; set; }
    public double ElevenLabsStyleeExaggeration { get; set; }
    public string MurfApiKey { get; set; }
    public string MurfStyle { get; set; }
    public string MistralApiKey { get; set; }
    public string MistralModel { get; set; }
    public string Qwen3TtsCppModel { get; set; }
    public string Qwen3TtsCppVulkanPath { get; set; }
    public string Qwen3TtsCppInstruction { get; set; }
    public string Qwen3TtsCrispAsrModel { get; set; }
    // Display name of the picked Qwen3-TTS output language ("Auto" = let the model infer it).
    public string Qwen3TtsCrispAsrLanguage { get; set; }
    public string VibeVoiceCrispAsrModel { get; set; }
    public double VibeVoiceCrispAsrSpeed { get; set; }
    public string IndexTtsCrispAsrModel { get; set; }
    public double IndexTtsCrispAsrSpeed { get; set; }
    public string PocketTtsCrispAsrModel { get; set; }
    public double PocketTtsCrispAsrSpeed { get; set; }
    public string DotsTtsCrispAsrModel { get; set; }
    // Flow-matching Euler steps for dots.tts (8-32, default 16). Higher is better and slower;
    // there is no CLI flag for it, so the engine passes it as CRISPASR_DOTS_ODE_STEPS.
    public int DotsTtsCrispAsrOdeSteps { get; set; }
    public string Confucius4TtsCrispAsrModel { get; set; }
    // Display name of the picked Confucius4-TTS target language (empty = English, the backend's
    // implicit default — there is no auto-detection).
    public string Confucius4TtsCrispAsrLanguage { get; set; }
    // Flow-matching Euler steps for the Confucius4-TTS S2A stage (10-40, default 20). Higher is
    // better and slower; passed as --tts-steps.
    public int Confucius4TtsCrispAsrOdeSteps { get; set; }
    public string IndexTts25AudioCppModel { get; set; }
    // Licence version the user accepted for the IndexTTS-2.5 weights (bilibili Model Use
    // License, not OSI-approved). Empty until accepted; a version bump re-prompts.
    public string IndexTts25AudioCppLicenseAccepted { get; set; }
    // ggml backend of the installed audio.cpp archive: metal, cuda, vulkan or cpu.
    public string IndexTts25AudioCppBackend { get; set; }
    // The model's own duration control (>1 slower, <1 faster) rather than a resample, so
    // pitch is unaffected. Valid range 0.5-2.0.
    public double IndexTts25AudioCppDurationFactor { get; set; }
    // One of IndexTts25AudioCpp.EmotionNames, or empty/"none" for no emotion conditioning.
    public string IndexTts25AudioCppEmotion { get; set; }
    public double IndexTts25AudioCppEmotionAlpha { get; set; }

    // Higgs Audio v3 (audio.cpp). The runtime backend is shared with IndexTTS 2.5 —
    // see AudioCppRuntime.GetBackend — so only model choice and licence live here.
    public string HiggsTtsAudioCppModel { get; set; }
    public string HiggsTtsAudioCppLicenseAccepted { get; set; }

    // Fish Audio S2 Pro (audio.cpp). Same sharing as above.
    public string FishTtsAudioCppModel { get; set; }
    public string FishTtsAudioCppLicenseAccepted { get; set; }
    public string FireRedTts3AudioCppModel { get; set; }
    public string FireRedTts3AudioCppLanguage { get; set; }
    public string CosyVoice3CrispAsrModel { get; set; }
    public double CosyVoice3CrispAsrSpeed { get; set; }
    // Display name of the picked CosyVoice3 target language ("Auto" = plain zero-shot cloning).
    public string CosyVoice3CrispAsrLanguage { get; set; }
    // ISO code of the language spoken in imported reference WAVs (empty = let the server detect).
    // Sent as `source_lang` so cross-lingual cloning engages for Latin-script references too.
    public string CosyVoice3CrispAsrSourceLanguage { get; set; }
    public string F5TtsCrispAsrModel { get; set; }
    public double F5TtsCrispAsrSpeed { get; set; }
    public string OmniVoiceCrispAsrModel { get; set; }
    public double OmniVoiceCrispAsrSpeed { get; set; }
    // Display name of the picked OmniVoice target language ("Auto" = let the model decide).
    public string OmniVoiceCrispAsrLanguage { get; set; }
    public string VoxCPM2CrispAsrModel { get; set; }
    public double VoxCPM2CrispAsrSpeed { get; set; }
    public string MossTtsCrispAsrModel { get; set; }
    public double MossTtsCrispAsrSpeed { get; set; }
    // Display name of the picked MOSS-TTS target language ("Auto" = let the model infer it).
    public string MossTtsCrispAsrLanguage { get; set; }
    public string OmniVoiceTtsCppVulkanPath { get; set; }
    public string OmniVoiceTtsCppInstruction { get; set; }
    public string ChatterboxModel { get; set; }
    public string ChatterboxCrispAsrLanguage { get; set; }
    public string ChatterboxCrispAsrSourceLanguage { get; set; }
    // Display name of the picked Zonos output language (empty = backend default, English US).
    public string ZonosTtsCrispAsrLanguage { get; set; }
    public string KokoroVoice { get; set; }
    public string GoogleApiKey { get; set; }
    public string GoogleKeyFile { get; set; }

    // CrispASR voice cloning: the user accepts responsibility for having the speaker's consent and
    // for disclosing that the result is AI-generated. Drives the `consent_attestation` /
    // `marking_attestation` fields the crispasr server requires before it will clone a voice, and
    // the provenance opt-out flags (--no-watermark / --no-c2pa) SE passes at server start.
    // Turn it off and CrispASR keeps its audible AI disclaimer, inaudible watermark and C2PA
    // manifest, and refuses to clone a reference WAV at all.
    public bool AcceptVoiceCloning { get; set; }

    // Version stamp of the voice-cloning terms the user accepted in the first-clone dialog, empty
    // until then. Separate from AcceptVoiceCloning (which defaults on and predates the dialog) so
    // the prompt can be shown once, and shown again if the terms change.
    // See Features.Video.TextToSpeech.Engines.VoiceCloningConsent.
    public string VoiceCloningConsent { get; set; }

    // Pro audio post-processing
    public bool ProAudioChainEnabled { get; set; }

    // Audio ducking (mix original audio at reduced volume)
    public bool AudioDuckingEnabled { get; set; }
    public int AudioDuckingOriginalVolume { get; set; }

    // Edge-TTS prosody parameters
    public string EdgeTtsRate { get; set; }
    public string EdgeTtsPitch { get; set; }
    public string EdgeTtsVolume { get; set; }

    // Optional override for the edge-tts executable path
    public string EdgeTtsPath { get; set; }

    // VAD-based internal silence compression (shorten pauses between words before time-stretching)
    public bool VadSilenceCompressionEnabled { get; set; }
    public double VadMaxSilenceSeconds { get; set; }

    // High-quality time-stretching using rubberband (WSOLA) instead of atempo
    public bool HighQualityTimeStretchEnabled { get; set; }

    // Silence padding between segments (ms)
    public int SilencePaddingMs { get; set; }

    // Output sample rate (0 = default)
    public int OutputSampleRate { get; set; }

    // Base folder the generation clips are written into while a run is in progress. Empty = the
    // system temp folder. Each run still gets its own "se-tts-<guid>" subfolder inside it, so
    // pointing this at the subtitle folder keeps the clips next to the work instead of buried in
    // %TMP% (#13332).
    public string GenerationFolder { get; set; }

    // Remove the run's generation folder when the Text to speech window closes. Turn it off to
    // keep the per-line clips around for inspection - nothing else sweeps them.
    public bool DeleteTempFiles { get; set; }

    // Remembered actor/voice mappings. Pre-fills the cast dialog so users don't have to
    // re-assign the same character voices every time they open a new subtitle. Keyed by actor
    // name; matches happen case-insensitively.
    public List<ActorVoiceMapping> LastActorVoiceMappings { get; set; }

    public SeVideoTextToSpeech()
    {
        Engine = "Piper";
        Voice = string.Empty;
        ElevenLabsApiKey = string.Empty;
        AzureApiKey = string.Empty;
        AzureRegion = string.Empty;
        ElevenLabsModel = "eleven_turbo_v2_5";
        ElevenLabsLanguage = string.Empty;
        ElevenLabsStability = 0.5;
        ElevenLabsSimilarity = 0.5;
        ElevenLabsSpeakerBoost = 0;
        ElevenLabsStyleeExaggeration = 0;
        ElevenLabsSpeed = 1.0;
        ReviewAudioClips = true;
        CustomAudio = false;
        CustomAudioStereo = true;
        CustomAudioEncoding = string.Empty;
        GenerateVideoFile = true;
        VoiceTestText = "Hello, how are you doing?";
        AllTalkUrl = "http://127.0.0.1:7851";
        MurfApiKey = string.Empty;
        MurfStyle = "Conversational";
        MistralApiKey = string.Empty;
        MistralModel = "voxtral-mini-tts-2603";
        Qwen3TtsCppModel = "0.6B";
        Qwen3TtsCppVulkanPath = string.Empty;
        Qwen3TtsCppInstruction = string.Empty;
        Qwen3TtsCrispAsrModel = "1.7B VoiceDesign";
        Qwen3TtsCrispAsrLanguage = string.Empty;
        VibeVoiceCrispAsrModel = "Q8_0 (~2.8 GB)";
        VibeVoiceCrispAsrSpeed = 1.1;
        IndexTtsCrispAsrModel = "Q8_0 (~870 MB)";
        IndexTtsCrispAsrSpeed = 1.0;
        PocketTtsCrispAsrModel = "English F16 (~219 MB)";
        PocketTtsCrispAsrSpeed = 1.0;
        DotsTtsCrispAsrModel = "Q8_0 (~3.5 GB)";
        DotsTtsCrispAsrOdeSteps = 16;
        Confucius4TtsCrispAsrModel = "Q8_0 (~1.9 GB)";
        Confucius4TtsCrispAsrLanguage = string.Empty;
        Confucius4TtsCrispAsrOdeSteps = 20;
        IndexTts25AudioCppModel = "Q8_0 (~3.3 GB)";
        IndexTts25AudioCppLicenseAccepted = string.Empty;
        IndexTts25AudioCppBackend = string.Empty;
        IndexTts25AudioCppDurationFactor = 1.0;
        IndexTts25AudioCppEmotion = string.Empty;
        IndexTts25AudioCppEmotionAlpha = 0.8;
        HiggsTtsAudioCppModel = "Q8_0 (~4.7 GB)";
        HiggsTtsAudioCppLicenseAccepted = string.Empty;
        FishTtsAudioCppModel = "Q8_0 (~5.9 GB)";
        FishTtsAudioCppLicenseAccepted = string.Empty;
        FireRedTts3AudioCppModel = "Q8_0 (~3.9 GB)";
        FireRedTts3AudioCppLanguage = string.Empty;
        CosyVoice3CrispAsrModel = "Q4_K (~1.6 GB total)";
        CosyVoice3CrispAsrSpeed = 1.0;
        CosyVoice3CrispAsrLanguage = string.Empty;
        CosyVoice3CrispAsrSourceLanguage = string.Empty;
        F5TtsCrispAsrModel = "F16 (~953 MB)";
        F5TtsCrispAsrSpeed = 1.0;
        OmniVoiceCrispAsrModel = "Q4_K (~1 GB)";
        OmniVoiceCrispAsrSpeed = 1.0;
        OmniVoiceCrispAsrLanguage = string.Empty;
        VoxCPM2CrispAsrModel = "Q4_K (~1.7 GB)";
        VoxCPM2CrispAsrSpeed = 1.0;
        MossTtsCrispAsrModel = "Q4_K (~10.5 GB incl. codec)";
        MossTtsCrispAsrSpeed = 1.0;
        MossTtsCrispAsrLanguage = string.Empty;
        OmniVoiceTtsCppVulkanPath = string.Empty;
        OmniVoiceTtsCppInstruction = string.Empty;
        ChatterboxModel = "Base";
        ChatterboxCrispAsrLanguage = string.Empty;
        ChatterboxCrispAsrSourceLanguage = string.Empty;
        ZonosTtsCrispAsrLanguage = string.Empty;
        KokoroVoice = "af_maple";
        GoogleApiKey = string.Empty;
        GoogleKeyFile = string.Empty;
        AcceptVoiceCloning = true;
        VoiceCloningConsent = string.Empty;
        ProAudioChainEnabled = false;
        AudioDuckingEnabled = false;
        AudioDuckingOriginalVolume = 15;
        EdgeTtsRate = string.Empty;
        EdgeTtsPitch = string.Empty;
        EdgeTtsVolume = string.Empty;
        EdgeTtsPath = string.Empty;
        VadSilenceCompressionEnabled = false;
        VadMaxSilenceSeconds = 0.15;
        HighQualityTimeStretchEnabled = false;
        SilencePaddingMs = 0;
        OutputSampleRate = 0;
        GenerationFolder = string.Empty;
        DeleteTempFiles = true;
        LastActorVoiceMappings = new List<ActorVoiceMapping>();
    }
}