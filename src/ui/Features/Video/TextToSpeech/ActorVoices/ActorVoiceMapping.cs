namespace Nikse.SubtitleEdit.Features.Video.TextToSpeech.ActorVoices;

// One row in the actor/voice-to-TTS-voice cast. Persisted as part of SubtitleEditTts.json
// (round-trips with the audio export) and also kept in SeVideoTextToSpeech.LastActorVoiceMappings
// so the next session can pre-fill the same cast even before the user has exported anything.
public class ActorVoiceMapping
{
    // The ASSA Actor field or the WebVTT <v ...> voice name this row maps from.
    public string Actor { get; set; }

    // The TTS engine name (matches ITtsEngine.Name). Empty means "use the global engine".
    public string EngineName { get; set; }

    // The voice name within the engine (matches Voice.Name). Empty means "use the global voice".
    public string VoiceName { get; set; }

    // Optional engine-specific model key (e.g. "0.6B" for Qwen3, "eleven_v3" for ElevenLabs).
    // Empty falls back to the engine's default / the main window's selected model.
    public string Model { get; set; }

    // Optional free-text instruction for engines that support voice design (Qwen3, OmniVoice).
    public string Instruction { get; set; }

    public ActorVoiceMapping()
    {
        Actor = string.Empty;
        EngineName = string.Empty;
        VoiceName = string.Empty;
        Model = string.Empty;
        Instruction = string.Empty;
    }
}
