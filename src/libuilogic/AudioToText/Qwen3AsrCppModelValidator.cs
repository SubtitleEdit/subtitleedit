namespace Nikse.SubtitleEdit.UiLogic.AudioToText;

/// <summary>
/// Tells whether a user-supplied GGUF file is one the qwen3-asr.cpp engine ("Qwen3 ASR CPP")
/// can load. Several unrelated projects publish "Qwen3-ASR GGUF" files and only one layout
/// works here (issue #16232):
/// <list type="bullet">
/// <item>qwen3-asr.cpp: ONE file with the audio encoder ("audio.encoder.*" tensors,
/// "qwen3-asr.audio.encoder.*" keys) and the LLM, architecture "qwen3-asr". Compatible.</item>
/// <item>llama.cpp multimodal (ggml-org): decoder-only LLM GGUF plus a separate "mmproj" file
/// with "clip.audio.*" keys. Not compatible.</item>
/// <item>transcribe.cpp (handy-computer): architecture "qwen3_asr" with "stt.*" keys. Not compatible.</item>
/// <item>CapsWriter-Offline: ONNX encoder files plus a decoder-only "qwen3_asr_llm.gguf". Not compatible.</item>
/// </list>
/// qwen3-asr.cpp itself does not reject a foreign file - it falls back to default hyperparameters
/// and crashes or outputs garbage - so the check has to happen here, before the file is copied.
/// </summary>
public static class Qwen3AsrCppModelValidator
{
    public const string RequiredArchitecture = "qwen3-asr";
    public const string AudioEncoderTensorPrefix = "audio.encoder.";
    public const string AudioEncoderKeyPrefix = "qwen3-asr.audio.encoder.";

    public const string CompatibleSourcesHint =
        "Qwen3 ASR CPP needs a single GGUF file converted with qwen3-asr.cpp, which contains both the audio encoder and the language model. " +
        "Compatible downloads: huggingface.co/OpenVoiceOS (qwen3-asr-0.6b-f16 / q8_0 / q5_k_m / q4_k_m) and huggingface.co/FlippyDora/qwen3-asr-1.7b-GGUF.";

    /// <summary>
    /// Returns null when the file is a qwen3-asr.cpp model, otherwise a user-facing message
    /// explaining what the file is and where a compatible one can be found.
    /// </summary>
    public static string? GetIncompatibilityReason(string fileName)
    {
        GgufHeader header;
        try
        {
            header = GgufHeader.Read(fileName);
        }
        catch (InvalidDataException ex)
        {
            return $"'{Path.GetFileName(fileName)}' is not a GGUF model file: {ex.Message}";
        }

        return GetIncompatibilityReason(header, Path.GetFileName(fileName));
    }

    public static string? GetIncompatibilityReason(GgufHeader header, string displayName)
    {
        var architecture = header.Architecture ?? string.Empty;
        var hasAudioEncoder = HasAudioEncoder(header);

        if (architecture == RequiredArchitecture && hasAudioEncoder)
        {
            return null;
        }

        string what;
        if (architecture == "qwen3_asr" || header.Keys.Any(k => k.StartsWith("stt.", StringComparison.Ordinal)))
        {
            what = "a transcribe.cpp model, which is a different GGUF layout";
        }
        else if (header.Keys.Any(k => k.StartsWith("clip.", StringComparison.Ordinal)))
        {
            what = "a llama.cpp multimodal projector ('mmproj') file, not a complete model";
        }
        else if (!hasAudioEncoder)
        {
            what = "a text-decoder-only GGUF without the audio encoder (the layout used by llama.cpp and CapsWriter-Offline, where the encoder is a separate ONNX file)";
        }
        else
        {
            what = $"a '{architecture}' model, not a 'qwen3-asr' model";
        }

        return $"'{displayName}' is {what}. {CompatibleSourcesHint}";
    }

    private static bool HasAudioEncoder(GgufHeader header)
    {
        return header.TensorNames.Any(n => n.Contains(AudioEncoderTensorPrefix, StringComparison.Ordinal)) ||
               header.Keys.Any(k => k.StartsWith(AudioEncoderKeyPrefix, StringComparison.Ordinal));
    }
}
