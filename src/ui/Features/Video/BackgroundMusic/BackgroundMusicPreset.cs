using Nikse.SubtitleEdit.Logic.Config;
using System;
using System.Collections.Generic;

namespace Nikse.SubtitleEdit.Features.Video.BackgroundMusic;

/// <summary>
/// A starting point for the prompt. Prompts stay English — they are model input, not UI text — and
/// all end with <see cref="PromptSuffix"/>, which keeps the music under someone talking.
/// </summary>
public sealed class BackgroundMusicPreset
{
    public const string CustomKey = "custom";

    public const string PromptSuffix = ", unobtrusive underscore for someone talking, instrumental, no vocals, steady tempo";

    public string Key { get; }
    public string DisplayName { get; }
    public string Prompt { get; }
    public int Bpm { get; }

    public BackgroundMusicPreset(string key, string displayName, string prompt, int bpm)
    {
        Key = key;
        DisplayName = displayName;
        Prompt = prompt;
        Bpm = bpm;
    }

    public bool IsCustom => Key == CustomKey;

    public override string ToString() => DisplayName;

    /// <summary>Tried on ACE-Step 1.5 Turbo Q8_0 (seed 42): no vocals, and each loops cleanly.</summary>
    public static List<BackgroundMusicPreset> GetAll()
    {
        var l = Se.Language.Video.BackgroundMusic;
        var presets = new List<BackgroundMusicPreset>
        {
            new("cooking", l.PresetCooking,
                "light cheerful background music for a baking and cooking video, warm acoustic guitar, ukulele, soft piano, gentle hand claps, light glockenspiel, relaxed and friendly" + PromptSuffix,
                120),
            new("tech", l.PresetTech,
                "modern clean electronic background music for a tech and gadget review video, soft synth plucks, light electronic beat, warm sub bass, crisp hi-hats, focused and optimistic" + PromptSuffix,
                110),
            new("forest", l.PresetForest,
                "peaceful nature background music for a forest walk video, fingerpicked acoustic guitar, soft wooden flute, gentle strings, light harp, airy and calm" + PromptSuffix,
                84),
            new("piano", l.PresetRelaxingPiano,
                "relaxing solo piano background music, soft felt piano, slow gentle melody, warm reverb, calm and reflective" + PromptSuffix,
                72),
            new("lofi", l.PresetLofiVlog,
                "chill lo-fi hip hop background music for a vlog, mellow rhodes piano, soft boom bap drums, warm bass, subtle vinyl crackle, laid-back" + PromptSuffix,
                85),
            new("corporate", l.PresetCorporate,
                "inspiring positive corporate background music for an explainer or presentation video, bright piano, light plucked strings, soft pulsing synth, gentle drums, uplifting" + PromptSuffix,
                100),
            new(CustomKey, Se.Language.General.Custom, string.Empty, 100),
        };

        // Alphabetical in the UI language.
        presets.Sort((x, y) => string.Compare(x.DisplayName, y.DisplayName, StringComparison.CurrentCultureIgnoreCase));
        return presets;
    }
}
