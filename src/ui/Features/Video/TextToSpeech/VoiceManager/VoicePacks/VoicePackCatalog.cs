using System.Collections.Generic;

namespace Nikse.SubtitleEdit.Features.Video.TextToSpeech.VoiceManager.VoicePacks;

/// <summary>
/// The voice packs offered by "Download voice packs...". All hosted on SubtitleEdit's
/// support-files releases; every pack is a flat zip of <c>Name.wav</c> + <c>Name.txt</c>
/// (spoken transcript) pairs plus an <c>ATTRIBUTION.txt</c>.
/// </summary>
/// <remarks>
/// Adding a pack: build it with the same layout (22.05 kHz mono 16-bit PCM WAV, 6-11 s of clean
/// speech per voice, real transcripts - no attribution blurbs, those are rejected as ref-text),
/// upload it to a support-files release, and add a line here with its byte size and SHA-256.
/// The non-English packs were cut from Multilingual LibriSpeech (LibriVox readers, CC BY 4.0);
/// the English pack is the reference set shared with the qwen3-tts.cpp / OmniVoice downloads.
/// </remarks>
public static class VoicePackCatalog
{
    private const string PacksUrlBase = "https://github.com/SubtitleEdit/support-files/releases/download/tts-voice-packs-2026-09/";

    public static IReadOnlyList<VoicePack> All { get; } = new List<VoicePack>
    {
        new("en-standard", "English - standard voices", "English", "en",
            "18 English reference voices (male/female, plus a few well-known narrators) with transcripts.",
            "https://github.com/SubtitleEdit/support-files/releases/download/qwen3-tts-cpp-2026-5/voices.zip",
            18, 7560781, "Wikimedia Commons / CC",
            "8935dcb18c71fe261e95c0e7c8e4f6cbca153c76109bb946ba4dfe1f115fcada"),

        new("de", "German voices", "German", "de",
            "8 German audiobook readers (4 female, 4 male) with transcripts.",
            PacksUrlBase + "voices-de.zip", 8, 3127159, "CC BY 4.0 (Multilingual LibriSpeech)",
            "c541c76ab960ab31165d753385016012622bd36e43f0fdb3e53e96b31741b88d"),

        new("es", "Spanish voices", "Spanish", "es",
            "8 Spanish audiobook readers (4 female, 4 male) with transcripts.",
            PacksUrlBase + "voices-es.zip", 8, 3105013, "CC BY 4.0 (Multilingual LibriSpeech)",
            "dcd84cf4286152c2dd23436ce36d42427a587af5f9213162f4f61adc6510ec84"),

        new("fr", "French voices", "French", "fr",
            "8 French audiobook readers (4 female, 4 male) with transcripts.",
            PacksUrlBase + "voices-fr.zip", 8, 3159607, "CC BY 4.0 (Multilingual LibriSpeech)",
            "f0a81f8558d9c3c42de55915b5721a98ef0d1db4f4da52d7e14d848349d7e71f"),

        new("it", "Italian voices", "Italian", "it",
            "7 Italian audiobook readers with transcripts.",
            PacksUrlBase + "voices-it.zip", 7, 2775499, "CC BY 4.0 (Multilingual LibriSpeech)",
            "ad082e544bc515a54d52736d882b90f00e7a88f07ce5f69e66eb18ec823640f2"),

        new("nl", "Dutch voices", "Dutch", "nl",
            "3 Dutch audiobook readers with transcripts.",
            PacksUrlBase + "voices-nl.zip", 3, 1131560, "CC BY 4.0 (Multilingual LibriSpeech)",
            "260a8f4900242b9fd655d4bde7a9d5b2d9e4b5ddfd24f159e78ff1af08541d2a"),

        new("pl", "Polish voices", "Polish", "pl",
            "4 Polish audiobook readers with transcripts.",
            PacksUrlBase + "voices-pl.zip", 4, 1560246, "CC BY 4.0 (Multilingual LibriSpeech)",
            "d2f43bdf5d9d955f1523d65dfe91de502892fca1708322510a9bab7d3efae55a"),

        new("pt", "Portuguese voices", "Portuguese", "pt",
            "8 Portuguese audiobook readers (4 female, 4 male) with transcripts.",
            PacksUrlBase + "voices-pt.zip", 8, 3166381, "CC BY 4.0 (Multilingual LibriSpeech)",
            "7ef61545300c4cfbd0c837acb80625835f3f4e673461ac4eae3b444eb33a8685"),
    };
}
