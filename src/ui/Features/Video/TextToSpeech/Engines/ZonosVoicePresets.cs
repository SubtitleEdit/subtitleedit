using System;
using System.Linq;

namespace Nikse.SubtitleEdit.Features.Video.TextToSpeech.Engines;

/// <summary>
/// The ZONOS3-Mini preset speakers Zyphra publishes in <c>gguf/voices/</c> of
/// <c>Zyphra/ZONOS3-Mini</c>: one 192-d speaker embedding per voice (<c>*.embedding.npy</c>,
/// 896 bytes each). CrispASR's zonos3 backend takes a voice as such an embedding only - it
/// cannot clone from a WAV yet (v0.8.55: <c>spk-encoder.gguf</c> is not wired) - so these are
/// the voices the engine offers.
///
/// The model infers the language from the text and has no language flag, but the speaker
/// embedding carries an accent: Danish text in the English Imogen voice came out as
/// English-accented gibberish (84% WER in an ASR round-trip, 2026-11-22). So the display name
/// leads with the voice's language, and languages without a preset use
/// <see cref="ZonosTtsCrispAsr.DefaultVoiceName"/> (no speaker embedding).
/// </summary>
internal static class ZonosVoicePresets
{
    public sealed record Preset(string Language, string Name, string Description, string FileStem, string Sha256)
    {
        /// <summary>The file in the voices folder; the server is sent <see cref="FileStem"/>.</summary>
        public string FileName => FileStem + ".embedding.npy";

        /// <summary>Folder in the HuggingFace repo (Zyphra groups the voices by language).</summary>
        public string RepoFolder => Language == "Chinese" ? "Mandarin Chinese" : Language;

        public string DisplayName => $"{Language} - {Name} ({Description})";
    }

    public const long FileSize = 896;

    // Hashes are the HF LFS oids (= SHA-256) at the pinned revision in ZonosTtsCrispAsr.
    public static readonly Preset[] All =
    {
        new("English", "Imogen", "British, female", "Imogen_en-British_F_1", "683424b91faeade48c8a92e05e1c3679366abbba443ef62eccba8dc40de48e9e"),
        new("English", "Rupert", "British, male", "Rupert_en-British_M_1", "0c68e699376dbafdc1ab0fadc133f48e7df9cb33b3a7b80b05f42c5c0aad8fa7"),
        new("English", "Savannah", "American, female", "Savannah_en-American_F_1", "dd81587e66f96fb62da47ef13d5d867de464ca386a83f01b4403cdd19629ca6c"),
        new("English", "Wyatt", "American, male", "Wyatt_en-American_M_1", "b39d377b653ebaca6bbd4ecd13c382fe16d81f0cfae27217d9b626e5589a0511"),
        new("English", "Cillian", "Irish, male", "Cillian_en-Irish_M_1", "f3125fae2f679fb5c8128d308ddd02986fd7d914975d17cc6dadf9eb130b1061"),
        new("English", "Jett", "energetic, male", "Jett_en-energetic_M_1", "624b749d220dac7fdcda479b1964cb8f7de58c838c1357f2f0161dac06e34f48"),
        new("English", "Thaddeus", "announcer, male", "Thaddeus_en-announcervoice_M_1", "fc8e21356730988aa20da70123991404345e0bdbfe3afe31e6371d8d93095461"),
        new("English", "Willow", "ASMR, female", "Willow_en-asrm_F_1", "6ebb84309f21f1dcbe6371e2355dc98f8ee3d0aee5850c305d06a73fe0a7c222"),
        new("English", "Morgana", "witch, female", "Morgana_en-witch_F_1", "a817387697c43138639aa1e52a9e42d9f3a629655e0faea5ac8565eb8e57c091"),
        new("English", "Sakura", "anime, female", "Sakura_en-anime_F_1", "daf3e82d2e6f2ce2fe90c73473a312a721295ae7f85763f99c87ee2f861063a9"),
        new("French", "Gaspard", "male", "Gaspard_fr_M_1", "6e19a9a545af6a263c00b997368d8396ff8abc761b53dce2a6d4df735f0766e9"),
        new("German", "Liesel", "female", "Liesel_de_F_1", "2d44e40f8c1f43fcabaabf301e14bfc48325bf4596cb04e076d662d8b2724c5d"),
        new("German", "Wolfgang", "male", "Wolfgang_de_M_1", "3a0e305b2789f79b843d96628b275b5d8f3f877f89c80e87fe57bf6078901467"),
        new("Hindi", "Ananya", "female", "Ananya_hi_F_1", "f7acf756273d86131de701d51b25a7e0b3323d724b7e98a0c890fcf176f85a14"),
        new("Hindi", "Arjun", "male", "Arjun_hi_M_1", "9cb0eb4cc81b79acadedb173e1fbbc0203bf71ea9ae66e56bd76393386f25815"),
        new("Italian", "Ginevra", "female", "Ginevra_it_F_1", "950ecf44f57fd053effd83c65f7f35059df0dcd1bb99dbc197b6649507171621"),
        new("Italian", "Lorenzo", "male", "Lorenzo_it_M_1", "a355bb33b907130985b74eae3fa05e4aa19d7fe7233f26c0416afad0726c2367"),
        new("Korean", "Seoyeon", "female", "Seoyeon_ko_F_1", "dddb6aa68c3febe83ca656c959f4d78246c72e011cde72c3745d1a46a37d8345"),
        new("Korean", "Minjun", "male", "Minjun_ko_M_1", "ab3d9c60c3f36080fd7e9eebd20c8ece42059236dd83f19980e5e4dbca94fc4e"),
        new("Chinese", "Meilin", "energetic, female", "Meilin_zh-energetic_F_1", "8430a2852291d874f327666d05bd15ee854a009cc54b9d3cdba9005d6bc7f59b"),
        new("Chinese", "Haoran", "energetic, male", "Haoran_zh-energetic_M_1", "b056f91bcbf4ffda432459f8c6a80b37cdc98cd82fd296e29cf2584325809c6c"),
        new("Portuguese", "Leonor", "female", "Leonor_pt_F_1", "c4074805d713ac20444253f1b98f814b7f27c43b6ab29bb2f0feeaaae8f7e4b2"),
        new("Portuguese", "Thiago", "male", "Thiago_pt_M_1", "fd1eee487091ef60636fe08cbafc15133c34a3f697629e1057e749c5b9893dc0"),
        new("Spanish", "Ximena", "female", "Ximena_es_F_1", "b9cc1284c52003705619b37f2c25c59e170d2c70650af39e5692b2fe91782bd6"),
        new("Spanish", "Rodrigo", "male", "Rodrigo_es_M_1", "0305b7ab40d5d4a1df07ed7aa1c164f64ede1ac4b6250fa56383b4c3ae03e1e1"),
        new("Telugu", "Sravya", "female", "Sravya_te_F_1", "a62e330ccfe900dbc4caac3e57d661adf284da8c967072dbe91bab154df79c0f"),
        new("Telugu", "Chaitanya", "male", "Chaitanya_te_M_1", "ca818012cf5656d12eb3bccdfad4086831a6a892f8e7bf2277ca11bf921e5eb7"),
    };

    public static Preset? FindByStem(string? stem) =>
        All.FirstOrDefault(p => string.Equals(p.FileStem, stem, StringComparison.Ordinal));
}
