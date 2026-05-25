using System;
using System.Threading;
using System.Threading.Tasks;
using Nikse.SubtitleEdit.Features.Video.TextToSpeech.Engines;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Video.TextToSpeech;

// Engines read voice-design instruction text out of Se.Settings during Speak(). To temporarily
// substitute a different instruction (per-line in Generate, per-row in the Cast Test/Regenerate
// buttons, per-line in Review Regenerate) we have to swap the global field, run the engine call,
// then put the original back.
//
// Three places used to keep private copies of this swap. The duplication was harmless on its own
// but the swap is GLOBAL: if two callers ran concurrently (e.g. user clicks Test in the Cast
// dialog while a Generate loop is mid-paragraph) their swap/restore pairs would interleave and
// one would restore the *other's* swap as if it were the original — leaving stale instruction
// text in Se.Settings.
//
// The semaphore here serialises the entire swap → speak → restore critical section so concurrent
// callers wait their turn. Side-effect: parallel Speak() calls that share the swap field are
// effectively serialised, which is acceptable given how rarely both happen at once.
public static class TtsInstructionSwap
{
    private static readonly SemaphoreSlim Gate = new(1, 1);

    // Runs `speak` with the given instruction temporarily applied to Se.Settings. Returns
    // whatever `speak` returns. Settings are always restored, even when `speak` throws.
    public static async Task<T> RunAsync<T>(ITtsEngine? engine, string? instruction, Func<Task<T>> speak)
    {
        await Gate.WaitAsync();
        var previous = Swap(engine, instruction);
        try
        {
            return await speak();
        }
        finally
        {
            Restore(engine, previous);
            Gate.Release();
        }
    }

    private static string? Swap(ITtsEngine? engine, string? instruction)
    {
        var s = Se.Settings.Video.TextToSpeech;
        switch (engine)
        {
            case Qwen3TtsCpp:
            case Qwen3TtsCrispAsr:
                var prevQwen = s.Qwen3TtsCppInstruction;
                s.Qwen3TtsCppInstruction = instruction ?? string.Empty;
                return prevQwen;
            case OmniVoiceTtsCpp:
                var prevOmni = s.OmniVoiceTtsCppInstruction;
                s.OmniVoiceTtsCppInstruction = instruction ?? string.Empty;
                return prevOmni;
            default:
                return null;
        }
    }

    private static void Restore(ITtsEngine? engine, string? previous)
    {
        if (previous == null)
        {
            return;
        }

        var s = Se.Settings.Video.TextToSpeech;
        switch (engine)
        {
            case Qwen3TtsCpp:
            case Qwen3TtsCrispAsr:
                s.Qwen3TtsCppInstruction = previous;
                break;
            case OmniVoiceTtsCpp:
                s.OmniVoiceTtsCppInstruction = previous;
                break;
        }
    }
}
