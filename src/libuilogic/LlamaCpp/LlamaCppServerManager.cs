using System.Diagnostics;
using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.UiLogic.Ocr;

namespace Nikse.SubtitleEdit.UiLogic.LlamaCpp;

/// <summary>
/// A curated llama.cpp model that can be downloaded and served. Optional <paramref name="MmprojFileName"/>
/// / <paramref name="MmprojUrl"/> are set for multimodal (vision) models that need a separate vision
/// projector. <paramref name="ChatTemplate"/> and <paramref name="NoJinja"/> override the llama-server
/// launch flags when the bundled chat template needs replacing (TranslateGemma ships a non-standard
/// Jinja template).
/// </summary>
public sealed record LlamaCppModel(
    string DisplayName,
    string FileName,
    string Size,
    string Url,
    string? MmprojFileName = null,
    string? MmprojUrl = null,
    string? ChatTemplate = null,
    bool NoJinja = false,
    // Translation prompt this model was trained on ({0} = source language English name,
    // {1} = target language English name, optional {2} = the text to translate); null = the
    // user's generic llama.cpp prompt. Needed for Hy-MT2, which answers in Chinese when given
    // the generic prompt, and for MiLMMT-46, whose completion format embeds the text between
    // a "{0}: " prefix and a trailing "{1}:" cue.
    string? PromptTemplate = null,
    // Model-recommended sampling; -1 = leave the server default.
    double Temperature = -1,
    double TopP = -1,
    int TopK = -1,
    double RepeatPenalty = -1,
    // Raw-completion translation model with no instruction training (MiLMMT-46): it can only
    // continue its trained PromptTemplate. Excluded from the advanced engine's model list -
    // its JSON batch protocol gets back well-formed JSON whose values are the untranslated
    // source lines, which would be written to the grid as a "successful" batch.
    bool CompletionOnly = false,
    // Launches the server with "--reasoning off" for models that think by default (Gemma 4).
    // Thinking is not just slow here, it loses the answer: the thoughts go to
    // "message.reasoning_content" and "message.content" - the only field the engines read -
    // stays empty until the token budget runs out, so the line comes back untranslated. The
    // Qwen families avoid this through their chat-template override instead (chatml +
    // --no-jinja bypasses the embedded template's thinking logic).
    bool NoThinking = false,
    // OCR models only: how the subtitle image is framed before it is sent (see AiOcrPadding).
    AiOcrPadding OcrPadding = AiOcrPadding.Square);

/// <summary>
/// Manages the local <c>llama-server</c> process used by the llama.cpp auto-translate and OCR
/// engines: folder/executable paths, the curated model lists, and the server lifecycle (start,
/// health probe, stop, kill on app exit). Modeled on the CrispASR/Chatterbox server handling.
/// </summary>
public static class LlamaCppServerManager
{
    // Tencent's official Hy-MT2 prompt - the model is trained on exactly this phrasing and
    // expects the TARGET language's English name ({1}); {0} (source) is unused by design.
    // The line-break sentence is our addition to Tencent's official wording: the 7B honors it
    // reliably, the 1.8B only sometimes - lost breaks are restored by SE's auto-break like for
    // the other LLM engines.
    private const string HyMt2PromptTemplate =
        "Translate the following text into {1}. Keep line breaks exactly the same. Note that you should only output the translated result without any additional explanation:";

    // MiLMMT-46's trained raw-completion format (with language English names). The text sits
    // inside the template ({2}) and the trailing "{1}:" cue is mandatory - without it the model
    // does not switch language and just echo-loops the source. Its GGUF chat template is a pure
    // passthrough ("{{ message.content }}", no role markers), so the chat endpoint delivers
    // this verbatim and no ChatTemplate/NoJinja override is wanted.
    private const string MiLmMt46PromptTemplate =
        "Translate this from {0} to {1}:\n{0}: {2}\n{1}:";

    public static readonly IReadOnlyList<LlamaCppModel> TranslateModels = new[]
    {
        new LlamaCppModel("TranslateGemma 4B (Q4_K_M)", "translategemma-4b_Q4_K_M.gguf", "2.5 GB",
            "https://huggingface.co/SandLogicTechnologies/translategemma-4b-it-GGUF/resolve/main/translategemma-4b_Q4_K_M.gguf",
            ChatTemplate: "gemma", NoJinja: true),
        new LlamaCppModel("TranslateGemma 4B (Q5_K_M)", "translategemma-4b_Q5_K_M.gguf", "2.8 GB",
            "https://huggingface.co/SandLogicTechnologies/translategemma-4b-it-GGUF/resolve/main/translategemma-4b_Q5_K_M.gguf",
            ChatTemplate: "gemma", NoJinja: true),
        new LlamaCppModel("TranslateGemma 4B (Q8_0)", "translategemma-4b-it-q8_0.gguf", "4.1 GB",
            "https://huggingface.co/NikolayKozloff/translategemma-4b-it-Q8_0-GGUF/resolve/main/translategemma-4b-it-q8_0.gguf",
            ChatTemplate: "gemma", NoJinja: true),
        new LlamaCppModel("TranslateGemma 12B (Q4_K_M)", "translategemma-12b-it-q4_k_m.gguf", "7.3 GB",
            "https://huggingface.co/NikolayKozloff/translategemma-12b-it-Q4_K_M-GGUF/resolve/main/translategemma-12b-it-q4_k_m.gguf",
            ChatTemplate: "gemma", NoJinja: true),
        new LlamaCppModel("TranslateGemma 12B (Q5_K_M)", "translategemma-12b-it-q5_k_m.gguf", "8.5 GB",
            "https://huggingface.co/NikolayKozloff/translategemma-12b-it-Q5_K_M-GGUF/resolve/main/translategemma-12b-it-q5_k_m.gguf",
            ChatTemplate: "gemma", NoJinja: true),

        // MiLMMT-46 v1.0 (Xiaomi, 2026) - Gemma3-based translation-specialized models, 46
        // languages including Danish/Norwegian/Swedish (the gap in Hy-MT2's coverage); the paper
        // reports it ahead of TranslateGemma and Hy-MT 1.5. Temperature 0 matches the model
        // card's greedy-decoding usage. CompletionOnly: see the record field - regular engine only.
        new LlamaCppModel("MiLMMT-46 4B (Q4_K_M) - 46 languages incl. Nordic", "MiLMMT-46-4B-v1.0.Q4_K_M.gguf", "2.5 GB",
            "https://huggingface.co/mradermacher/MiLMMT-46-4B-v1.0-GGUF/resolve/main/MiLMMT-46-4B-v1.0.Q4_K_M.gguf",
            PromptTemplate: MiLmMt46PromptTemplate, Temperature: 0, CompletionOnly: true),
        new LlamaCppModel("MiLMMT-46 4B (Q8_0) - 46 languages incl. Nordic", "MiLMMT-46-4B-v1.0.Q8_0.gguf", "4.1 GB",
            "https://huggingface.co/mradermacher/MiLMMT-46-4B-v1.0-GGUF/resolve/main/MiLMMT-46-4B-v1.0.Q8_0.gguf",
            PromptTemplate: MiLmMt46PromptTemplate, Temperature: 0, CompletionOnly: true),
        new LlamaCppModel("MiLMMT-46 12B (Q4_K_M) - 46 languages incl. Nordic", "MiLMMT-46-12B-v1.0.Q4_K_M.gguf", "7.3 GB",
            "https://huggingface.co/mradermacher/MiLMMT-46-12B-v1.0-GGUF/resolve/main/MiLMMT-46-12B-v1.0.Q4_K_M.gguf",
            PromptTemplate: MiLmMt46PromptTemplate, Temperature: 0, CompletionOnly: true),
        new LlamaCppModel("MiLMMT-46 12B (Q5_K_M) - 46 languages incl. Nordic", "MiLMMT-46-12B-v1.0.Q5_K_M.gguf", "8.4 GB",
            "https://huggingface.co/mradermacher/MiLMMT-46-12B-v1.0-GGUF/resolve/main/MiLMMT-46-12B-v1.0.Q5_K_M.gguf",
            PromptTemplate: MiLmMt46PromptTemplate, Temperature: 0, CompletionOnly: true),

        // Gemma 4 (Google, 2026) - 140+ languages, the strongest general model here for translation
        // into non-English targets. NOTE: unlike Gemma 2/3 this must use its own embedded Jinja
        // template - Gemma 4 replaced the <start_of_turn> scheme with <|turn>role ... <turn|>, so
        // llama.cpp's built-in "gemma" template does NOT apply and forcing it produces garbage.
        // That template turns thinking ON by default, which for subtitle-sized requests means no
        // translation at all: 7 of 16 English->Danish lines and 11 of 16 English->German lines came
        // back empty at ~10-12 s/line, each burning the whole max_tokens budget inside
        // reasoning_content. Hence NoThinking on every Gemma 4 entry - with it, 0 of 16 empty at
        // ~1 s/line.
        new LlamaCppModel("Gemma 4 E4B it (Q4_K_M)", "google_gemma-4-E4B-it-Q4_K_M.gguf", "5.4 GB",
            "https://huggingface.co/bartowski/google_gemma-4-E4B-it-GGUF/resolve/main/google_gemma-4-E4B-it-Q4_K_M.gguf",
            NoThinking: true),
        new LlamaCppModel("Gemma 4 E4B it (Q8_0)", "google_gemma-4-E4B-it-Q8_0.gguf", "8.0 GB",
            "https://huggingface.co/bartowski/google_gemma-4-E4B-it-GGUF/resolve/main/google_gemma-4-E4B-it-Q8_0.gguf",
            NoThinking: true),
        // The 12B repo (and its file names) drop the "google_" prefix the E4B repo uses.
        new LlamaCppModel("Gemma 4 12B it (Q4_K_M)", "gemma-4-12B-it-Q4_K_M.gguf", "7.6 GB",
            "https://huggingface.co/bartowski/gemma-4-12B-it-GGUF/resolve/main/gemma-4-12B-it-Q4_K_M.gguf",
            NoThinking: true),

        // Alternative model family. Qwen 3 is the strongest open model for CJK
        // (Chinese/Japanese/Korean) and competitive elsewhere — useful fallback
        // when Gemma's quirks bite (occasional refusals, formatting drift, etc).
        // NoThinking (--reasoning off) suppresses the hybrid Qwen3 template's thinking mode so
        // output is clean translation, not <think>...</think> reasoning blocks - same mechanism
        // as Gemma 4 above. This used to be "--no-jinja --chat-template chatml" instead (forcing
        // the template also bypasses enable_thinking), but that proved unreliable: a controlled
        // A/B (seconv EN->DA, 10 reps x 20 lines on the Q8_0 9B, 200 lines/variant) measured
        // chatml+no-jinja leaking raw <think> text - which runs the model out of its token budget
        // before it ever reaches the translation - into 2.5% of lines (5/200), while NoThinking
        // alone stayed at 0% (0/200) across every rep on both the 9B and the 4B Q4_K_M (0/60).
        // Stacking both overrides is worse, not better (6.5% on 9B, 16.7% on 4B): --reasoning off
        // does not reliably suppress thinking once --chat-template chatml has replaced the
        // template it hooks into, so the two must never be combined.
        new LlamaCppModel("Qwen 3 4B Instruct (Q4_K_M)", "Qwen_Qwen3-4B-Instruct-2507-Q4_K_M.gguf", "2.5 GB",
            "https://huggingface.co/bartowski/Qwen_Qwen3-4B-Instruct-2507-GGUF/resolve/main/Qwen_Qwen3-4B-Instruct-2507-Q4_K_M.gguf",
            NoThinking: true),
        new LlamaCppModel("Qwen 3 8B (Q4_K_M)", "Qwen_Qwen3-8B-Q4_K_M.gguf", "4.7 GB",
            "https://huggingface.co/bartowski/Qwen_Qwen3-8B-GGUF/resolve/main/Qwen_Qwen3-8B-Q4_K_M.gguf",
            NoThinking: true),

        // Qwen 3.5 - newer Qwen generation. Same NoThinking handling as Qwen 3 above. Kept to <= 8 GB.
        new LlamaCppModel("Qwen 3.5 4B (Q4_K_M)", "Qwen_Qwen3.5-4B-Q4_K_M.gguf", "2.8 GB",
            "https://huggingface.co/bartowski/Qwen_Qwen3.5-4B-GGUF/resolve/main/Qwen_Qwen3.5-4B-Q4_K_M.gguf",
            NoThinking: true),
        new LlamaCppModel("Qwen 3.5 4B (Q8_0)", "Qwen_Qwen3.5-4B-Q8_0.gguf", "4.3 GB",
            "https://huggingface.co/bartowski/Qwen_Qwen3.5-4B-GGUF/resolve/main/Qwen_Qwen3.5-4B-Q8_0.gguf",
            NoThinking: true),
        new LlamaCppModel("Qwen 3.5 9B (Q4_K_M)", "Qwen_Qwen3.5-9B-Q4_K_M.gguf", "5.7 GB",
            "https://huggingface.co/bartowski/Qwen_Qwen3.5-9B-GGUF/resolve/main/Qwen_Qwen3.5-9B-Q4_K_M.gguf",
            NoThinking: true),
        new LlamaCppModel("Qwen 3.5 9B (Q8_0)", "Qwen_Qwen3.5-9B-Q8_0.gguf", "9.8 GB",
            "https://huggingface.co/bartowski/Qwen_Qwen3.5-9B-GGUF/resolve/main/Qwen_Qwen3.5-9B-Q8_0.gguf",
            NoThinking: true),

        // Qwen 3.6 35B-A3B - a mixture-of-experts model: 35B total but only ~3B active per token, so
        // it generates fast even fully on CPU. That makes it the option for machines with plenty of
        // RAM but no usable GPU. The catch is the quant: UD-IQ2_M is 2-bit, and low-bit hurts MoE
        // more than dense models (few active params means little redundancy to absorb the error), so
        // translation quality may well trail the much smaller Qwen 3.5 9B Q4_K_M - hence the note in
        // the display name. IQ2_M is nonetheless the smallest Qwen 3.6 build available; every other
        // quant of this model is larger, not smaller. Its GGUF reports architecture "qwen35moe" (the
        // Qwen 3.5 MoE arch), which the pinned engine already supports.
        new LlamaCppModel("Qwen 3.6 35B-A3B (IQ2_M) - fast on CPU, 2-bit quality", "Qwen3.6-35B-A3B-UD-IQ2_M.gguf", "11.5 GB",
            "https://huggingface.co/unsloth/Qwen3.6-35B-A3B-GGUF/resolve/main/Qwen3.6-35B-A3B-UD-IQ2_M.gguf",
            NoThinking: true),

        // Hy-MT2 (Tencent Hunyuan-MT 2, 2026) - translation-specialized, official GGUFs, Apache-2.0.
        // Excellent for its 33+5 supported languages (CJK, major European/Asian) but has NO Nordic
        // languages (Danish/Swedish/Norwegian/Finnish), no Greek/Romanian/Hungarian - generation in
        // unsupported languages produces garbage, hence the coverage note in the display name.
        // Trained on a fixed prompt (PromptTemplate below, with language NAMES - the generic prompt
        // makes it answer in Chinese) and Tencent-recommended sampling. Embedded chat template works
        // as-is, so no ChatTemplate/NoJinja overrides.
        new LlamaCppModel("Hy-MT2 7B (Q4_K_M) - 33 languages, no Nordic", "Hy-MT2-7B-Q4_K_M.gguf", "4.6 GB",
            "https://huggingface.co/tencent/Hy-MT2-7B-GGUF/resolve/main/Hy-MT2-7B-Q4_K_M.gguf",
            PromptTemplate: HyMt2PromptTemplate, Temperature: 0.7, TopP: 0.6, TopK: 20, RepeatPenalty: 1.05),
        new LlamaCppModel("Hy-MT2 7B (Q8_0) - 33 languages, no Nordic", "HY-MT2-7B-Q8_0.gguf", "8.0 GB",
            "https://huggingface.co/tencent/Hy-MT2-7B-GGUF/resolve/main/HY-MT2-7B-Q8_0.gguf",
            PromptTemplate: HyMt2PromptTemplate, Temperature: 0.7, TopP: 0.6, TopK: 20, RepeatPenalty: 1.05),
        new LlamaCppModel("Hy-MT2 1.8B (Q8_0) - 33 languages, no Nordic", "Hy-MT2-1.8B-Q8_0.gguf", "1.9 GB",
            "https://huggingface.co/tencent/Hy-MT2-1.8B-GGUF/resolve/main/Hy-MT2-1.8B-Q8_0.gguf",
            PromptTemplate: HyMt2PromptTemplate, Temperature: 0.7, TopP: 0.6, TopK: 20, RepeatPenalty: 1.05),

        // TranslatePsy-AfriSLM 4B (Tether/QVAC, 2026) - a Qwen 3.5 4B fine-tune for English <-> 19
        // Sub-Saharan African languages (Afrikaans, Amharic, Hausa, Igbo, Kinyarwanda, Lingala,
        // Luganda, Malagasy, Nyanja, Oromo, Shona, Somali, Southern Sotho, Swahili, Tswana, Wolof,
        // Xhosa, Yoruba, Zulu), official GGUFs, Apache-2.0. Fills a real gap: the base Qwen 3.5 4B
        // writes fluent-looking nonsense in Zulu/Yoruba (16 subtitle lines, back-translated: 1-2 of
        // 16 recognizable vs 15-16 of 16 for this model). Verified 2026-09-21 on b10840:
        // - NoThinking is mandatory - it inherits Qwen 3.5's thinking template and without
        //   "--reasoning off" 12 of 16 lines came back empty (answer in reasoning_content).
        // - No PromptTemplate on purpose: the model card's prompt ("Please translate the following
        //   {0} text into {1}: {2}.\n\nTranslation:") drops line breaks and, translating INTO
        //   English, pads short lines with invented sentences ("Yeah." -> "Yes. Yes, that's
        //   right."); SE's generic prompt has neither problem. The advanced engine's JSON batch
        //   protocol works as well (all lines translated, all line breaks kept).
        new LlamaCppModel("TranslatePsy-AfriSLM 4B (Q4_K_M) - English <-> 19 African languages", "TranslatePsy-AfriSLM-4B-Q4_K_M-imat.gguf", "3.1 GB",
            "https://huggingface.co/qvac/TranslatePsy-AfriSLM-4B-Q4-GGUF/resolve/main/TranslatePsy-AfriSLM-4B-Q4_K_M-imat.gguf",
            NoThinking: true),
        new LlamaCppModel("TranslatePsy-AfriSLM 4B (Q8_0) - English <-> 19 African languages", "TranslatePsy-AfriSLM-4B-Q8_0-imat.gguf", "5.2 GB",
            "https://huggingface.co/qvac/TranslatePsy-AfriSLM-4B-Q8-GGUF/resolve/main/TranslatePsy-AfriSLM-4B-Q8_0-imat.gguf",
            NoThinking: true),

        // Index-Translate (bilibili, 2026) - a Qwen 3.5 fine-tune for translation, 150 languages,
        // Apache-2.0 (#15518). Temperature 0 = bilibili's greedy default. NoThinking because it keeps
        // Qwen 3.5's thinking template: without "--reasoning off" every line still came back, but at
        // ~4x the time (34 s vs 8 s for 24 lines EN->DA on the 2B). No PromptTemplate - its trained
        // prompt ("Translate the following text into {1}. Output the translation directly...") is
        // already what SE's generic prompt asks for. Downloads come from IndexTeam's official GGUF
        // repos (published 2026-10-02); the local file names are kept from the earlier community
        // quants so existing downloads and saved selections still match. Compared 2026-10-01 (seconv, 24 dialog lines
        // EN->DA/DE/JA, b10840): the 9B was on par with TranslateGemma 12B (best in Japanese, never
        // left a line untranslated) and ahead of Gemma 4 E4B; the 2B is roughly TranslateGemma 4B
        // level at twice its speed. The 2B Q4_K_M is not offered - it made clearly worse slips than
        // the Q8_0 (e.g. "sweetie" -> the Danish insult "kærling"). Index-Homura (same family) is
        // left out: it needs a target syllable count per line, which the engines cannot supply.
        new LlamaCppModel("Index-Translate 2B (Q8_0) - 150 languages", "Index-Translate-2B.Q8_0.gguf", "2.1 GB",
            "https://huggingface.co/IndexTeam/Index-Translate-2B-GGUF/resolve/main/Index-Translate-2B.Q8_0.gguf",
            Temperature: 0, NoThinking: true),
        new LlamaCppModel("Index-Translate 9B (Q4_K_M) - 150 languages", "index-translate-9b-q4_k_m.gguf", "5.8 GB",
            "https://huggingface.co/IndexTeam/Index-Translate-9B-GGUF/resolve/main/Index-Translate-9B.Q4_K_M.gguf",
            Temperature: 0, NoThinking: true),

        // Aya Expanse 8B (Cohere) - a dedicated multilingual model (23 languages), a good translation
        // alternative to the Gemma/Qwen families. Uses its own embedded (Cohere) chat template, so we
        // leave ChatTemplate/NoJinja at their defaults instead of forcing gemma/chatml. Kept to <= 8 GB.
        new LlamaCppModel("Aya Expanse 8B (Q4_K_M)", "aya-expanse-8b-Q4_K_M.gguf", "4.7 GB",
            "https://huggingface.co/bartowski/aya-expanse-8b-GGUF/resolve/main/aya-expanse-8b-Q4_K_M.gguf"),
        new LlamaCppModel("Aya Expanse 8B (Q5_K_M)", "aya-expanse-8b-Q5_K_M.gguf", "5.4 GB",
            "https://huggingface.co/bartowski/aya-expanse-8b-GGUF/resolve/main/aya-expanse-8b-Q5_K_M.gguf"),
        new LlamaCppModel("Aya Expanse 8B (Q8_0)", "aya-expanse-8b-Q8_0.gguf", "7.9 GB",
            "https://huggingface.co/bartowski/aya-expanse-8b-GGUF/resolve/main/aya-expanse-8b-Q8_0.gguf"),
    };

    // Models for the AI review tool (proofreading). Translation-tuned models (TranslateGemma,
    // Aya) are deliberately absent - proofreading needs general instruction-following and strict
    // JSON output, where the plain instruct models are much stronger. Kept to ~12 GB or below.
    public static readonly IReadOnlyList<LlamaCppModel> ReviewModels = new[]
    {
        // NoThinking instead of chatml+no-jinja - see the note in TranslateModels for the A/B data.
        new LlamaCppModel("Qwen 3.5 4B (Q4_K_M)", "Qwen_Qwen3.5-4B-Q4_K_M.gguf", "2.8 GB",
            "https://huggingface.co/bartowski/Qwen_Qwen3.5-4B-GGUF/resolve/main/Qwen_Qwen3.5-4B-Q4_K_M.gguf",
            NoThinking: true),
        new LlamaCppModel("Qwen 3.5 4B (Q8_0)", "Qwen_Qwen3.5-4B-Q8_0.gguf", "4.3 GB",
            "https://huggingface.co/bartowski/Qwen_Qwen3.5-4B-GGUF/resolve/main/Qwen_Qwen3.5-4B-Q8_0.gguf",
            NoThinking: true),
        new LlamaCppModel("Qwen 3.5 9B (Q4_K_M)", "Qwen_Qwen3.5-9B-Q4_K_M.gguf", "5.7 GB",
            "https://huggingface.co/bartowski/Qwen_Qwen3.5-9B-GGUF/resolve/main/Qwen_Qwen3.5-9B-Q4_K_M.gguf",
            NoThinking: true),
        new LlamaCppModel("Qwen 3.5 9B (Q8_0)", "Qwen_Qwen3.5-9B-Q8_0.gguf", "9.8 GB",
            "https://huggingface.co/bartowski/Qwen_Qwen3.5-9B-GGUF/resolve/main/Qwen_Qwen3.5-9B-Q8_0.gguf",
            NoThinking: true),
        // MoE, ~3B active - fast on CPU; see the note in TranslateModels for the 2-bit caveat.
        new LlamaCppModel("Qwen 3.6 35B-A3B (IQ2_M) - fast on CPU, 2-bit quality", "Qwen3.6-35B-A3B-UD-IQ2_M.gguf", "11.5 GB",
            "https://huggingface.co/unsloth/Qwen3.6-35B-A3B-GGUF/resolve/main/Qwen3.6-35B-A3B-UD-IQ2_M.gguf",
            NoThinking: true),
        new LlamaCppModel("Gemma 3 4B it (Q4_K_M)", "google_gemma-3-4b-it-Q4_K_M.gguf", "2.5 GB",
            "https://huggingface.co/bartowski/google_gemma-3-4b-it-GGUF/resolve/main/google_gemma-3-4b-it-Q4_K_M.gguf",
            ChatTemplate: "gemma", NoJinja: true),
        new LlamaCppModel("Gemma 3 12B it (Q4_K_M)", "google_gemma-3-12b-it-Q4_K_M.gguf", "7.3 GB",
            "https://huggingface.co/bartowski/google_gemma-3-12b-it-GGUF/resolve/main/google_gemma-3-12b-it-Q4_K_M.gguf",
            ChatTemplate: "gemma", NoJinja: true),
        // Gemma 4 uses its own embedded Jinja template - see the note in TranslateModels; the
        // built-in "gemma" template above is the Gemma 2/3 format and must not be forced here.
        // NoThinking for the same reason as there: the review client reads message.content too,
        // so a model that answers in reasoning_content returns an empty review.
        // E2B is the smallest option in this list - for laptops/iGPUs where even the 4B models
        // are a stretch.
        new LlamaCppModel("Gemma 4 E2B it (Q4_K_M)", "google_gemma-4-E2B-it-Q4_K_M.gguf", "3.5 GB",
            "https://huggingface.co/bartowski/google_gemma-4-E2B-it-GGUF/resolve/main/google_gemma-4-E2B-it-Q4_K_M.gguf",
            NoThinking: true),
        new LlamaCppModel("Gemma 4 E4B it (Q4_K_M)", "google_gemma-4-E4B-it-Q4_K_M.gguf", "5.4 GB",
            "https://huggingface.co/bartowski/google_gemma-4-E4B-it-GGUF/resolve/main/google_gemma-4-E4B-it-Q4_K_M.gguf",
            NoThinking: true),
        new LlamaCppModel("Gemma 4 E4B it (Q8_0)", "google_gemma-4-E4B-it-Q8_0.gguf", "8.0 GB",
            "https://huggingface.co/bartowski/google_gemma-4-E4B-it-GGUF/resolve/main/google_gemma-4-E4B-it-Q8_0.gguf",
            NoThinking: true),
        new LlamaCppModel("Gemma 4 12B it (Q4_K_M)", "gemma-4-12B-it-Q4_K_M.gguf", "7.6 GB",
            "https://huggingface.co/bartowski/gemma-4-12B-it-GGUF/resolve/main/gemma-4-12B-it-Q4_K_M.gguf",
            NoThinking: true),

        // Different families for second opinions. Llama 3.1 is the strongest English
        // proofreader of its size; Phi-4 mini is the small/fast option.
        // All use their embedded chat templates.
        new LlamaCppModel("Llama 3.1 8B Instruct (Q4_K_M)", "Meta-Llama-3.1-8B-Instruct-Q4_K_M.gguf", "4.9 GB",
            "https://huggingface.co/bartowski/Meta-Llama-3.1-8B-Instruct-GGUF/resolve/main/Meta-Llama-3.1-8B-Instruct-Q4_K_M.gguf"),
        new LlamaCppModel("Phi-4 mini 3.8B (Q4_K_M)", "microsoft_Phi-4-mini-instruct-Q4_K_M.gguf", "2.5 GB",
            "https://huggingface.co/bartowski/microsoft_Phi-4-mini-instruct-GGUF/resolve/main/microsoft_Phi-4-mini-instruct-Q4_K_M.gguf"),

        // PrismML Bonsai 27B, 1-bit (Q1_0, upstream llama.cpp - the ternary Q2_0/PQ2_0/PTQ1_0 files
        // need PrismML's fork). English only: tested 2026-10-11 on b11425 with the review protocol it
        // found 8/8 planted English errors with no false edits (Qwen 3.5 4B: 7/8), but only 2/6 in
        // Danish, and its translations were poor - hence review only, not in TranslateModels.
        // NoThinking is required: with thinking on a 15-line batch took ~400 s. ~12 tok/s on an M4.
        new LlamaCppModel("Bonsai 27B 1-bit (Q1_0) - English only", "Bonsai-27B-Q1_0.gguf", "3.8 GB",
            "https://huggingface.co/prism-ml/Bonsai-27B-gguf/resolve/main/Bonsai-27B-Q1_0.gguf",
            NoThinking: true),

        // EuroLLM 2512 - all 24 official EU languages (incl. Danish/Swedish/Norwegian/Finnish, which
        // the Hy-MT2 family lacks entirely), so these are the best picks for proofreading European
        // subtitles. Replaces the older EuroLLM-9B-Instruct entry; anyone who already downloaded that
        // one still sees it, as a custom entry from the models folder. The 9B quant comes from
        // mradermacher (no bartowski build exists), hence the dotted file name. The 22B is the largest
        // model in this list: IQ4_XS is the biggest quant that still fits ~12 GB - Q4_K_M is 13.7 GB.
        new LlamaCppModel("EuroLLM 9B Instruct 2512 (Q4_K_M)", "EuroLLM-9B-Instruct-2512.Q4_K_M.gguf", "5.6 GB",
            "https://huggingface.co/mradermacher/EuroLLM-9B-Instruct-2512-GGUF/resolve/main/EuroLLM-9B-Instruct-2512.Q4_K_M.gguf"),
        new LlamaCppModel("EuroLLM 22B Instruct 2512 (IQ4_XS)", "utter-project_EuroLLM-22B-Instruct-2512-IQ4_XS.gguf", "12.3 GB",
            "https://huggingface.co/bartowski/utter-project_EuroLLM-22B-Instruct-2512-GGUF/resolve/main/utter-project_EuroLLM-22B-Instruct-2512-IQ4_XS.gguf"),

        // Granite 4.1 (IBM, 2026) - Apache-2.0, dense, 128k context, 12 languages, and tuned for
        // structured output, which is what the review protocol asks for (strict JSON). Downloaded
        // straight from IBM's own GGUF repo rather than a third-party quanter. Embedded chat template.
        new LlamaCppModel("Granite 4.1 8B (Q4_K_M)", "granite-4.1-8b-Q4_K_M.gguf", "5.3 GB",
            "https://huggingface.co/ibm-granite/granite-4.1-8b-GGUF/resolve/main/granite-4.1-8b-Q4_K_M.gguf"),
    };

    /// <summary>
    /// The curated OCR vision models, <b>ordered best-first on subtitle images</b> - the order is
    /// not cosmetic. The first entry is what the OCR/Video OCR/batch-convert dropdowns preselect
    /// when nothing is saved, and the headless callers (seconv, batch convert) fall back to the
    /// first *installed* entry, so a weaker model placed early wins over a better one that happens
    /// to sit later in the list. Keep new models in measured rank, not in the order they were added.
    /// Ranked 2026-08-25 on llama.cpp b10625 with SE's own flags, prompt and square-pad
    /// preprocessing over a 14-image EN/DE/FR/ES/IT/RU/ZH/JA corpus (music cues, SDH hash cues,
    /// italics, video-frame burn-ins, small/low-res), scoring recognition separately from line-break
    /// preservation: GLM-OCR 13/14 exact at 2.5 s/image and the only one that keeps every line break
    /// and every music note; PaddleOCR-VL 12/14 recognized (0.32% char error) but merges two-line
    /// subtitles; HunyuanOCR 12/14 recognized (0.6%) with the same merging; LightOnOCR 9/14
    /// recognized (2.53%) at 18.5 s/image - weakest and ~7x slower, hence last.
    /// LightOnOCR-3 (2026-10-15, b11553): measured on 390 real Blu-ray subtitle images - 150 Dutch
    /// from #16031 plus 60 each German/Danish/French/English, reference hand-checked against the
    /// image wherever any model disagreed - exact lines: GLM-OCR 385, LightOnOCR-3 4B 384,
    /// LightOnOCR-3 0.8B 379 - the 4B ranks second, the 0.8B took LightOnOCR 1B's last place.
    /// </summary>
    // DeepSeek-OCR-2 was evaluated for this list and rejected (2026-08-26): through
    // mainline llama.cpp with the OCR prompt it captions the image ("The image displays a
    // scene from...") instead of extracting text - 2/24 subtitle lines exact at 11 s/image
    // on a burned-in video clip where GLM-OCR scored 21/24 at 4 s. Its strong results come
    // from CrispEmbed's fork, which drives the model's own crop/prompt modes, so that is
    // where SE offers it.
    public static readonly IReadOnlyList<LlamaCppModel> OcrModels = new[]
    {
        new LlamaCppModel("GLM-OCR 0.9B (Q8_0)", "GLM-OCR-Q8_0.gguf", "1.4 GB",
            "https://huggingface.co/ggml-org/GLM-OCR-GGUF/resolve/main/GLM-OCR-Q8_0.gguf",
            MmprojFileName: "mmproj-GLM-OCR-Q8_0.gguf",
            MmprojUrl: "https://huggingface.co/ggml-org/GLM-OCR-GGUF/resolve/main/mmproj-GLM-OCR-Q8_0.gguf"),
        // LFM3-VL 2B (Liquid AI, LFM 1.0 license), a general vision model rather than an OCR
        // specialist; needs llama.cpp b11960 or newer. Measured 2026-10-29 on b11968 against 220
        // real Blu-ray subtitle images (Dutch 60, German/Danish/French/English 40 each, reference
        // hand-checked wherever any model disagreed): 214 exact against GLM-OCR's 213 at 3.7 s vs
        // 4.9 s per image, and 18/19 on the synthetic corpus (GLM-OCR 16/19). Its misses are fluent
        // word swaps ("mistake" for "mistanke", "just" for "juist", "Yea" for "Yeah"), which are
        // harder to catch when proofreading than GLM-OCR's l/I slips - hence second, not first.
        // Thinking stays on: with it off the model merged 25 of 220 two-line subtitles into one
        // line and wrote the ♩ and ♫ notes for ♪; with it on, neither (~570 reasoning characters
        // per image, which the engines ignore - they read content only). Q4_K_M: the Q8_0 scored
        // the same (192/220 with thinking off, 17/19 synthetic) at 1.7x the size. Replaces
        // LFM2.5-VL 3B, which split lines into one word per line on real discs (171/220) and
        // was a 3.5 GB download. Same prompt as LFM2.5: under the shared prompt it answers
        // "There are two lines of text in the image..." instead of the text.
        new LlamaCppModel("LFM3-VL 2B (Q4_K_M)", "LFM3-VL-2B-Q4_K_M.gguf", "1.3 GB",
            "https://huggingface.co/LiquidAI/LFM3-VL-2B-GGUF/resolve/main/LFM3-VL-2B-Q4_K_M.gguf",
            MmprojFileName: "mmproj-LFM3-VL-2B-Q8_0.gguf",
            MmprojUrl: "https://huggingface.co/LiquidAI/LFM3-VL-2B-GGUF/resolve/main/mmproj-LFM3-VL-2B-Q8_0.gguf",
            PromptTemplate: SeOcrDefaults.LlamaCppOcrPromptLfmVl),
        // LightOnOCR-3 4B (LightOn, Apache-2.0, Qwen 3.5 VL) as mradermacher's Q8_0 - no official
        // GGUF exists. As accurate as GLM-OCR on the 390 real images (384 vs 385 exact) and ~2.5x
        // faster (1.3-1.8 s vs 3.5-5 s per image), and right where GLM-OCR fails: it keeps the line
        // break in two-line ♪ lyrics and reads capital I where GLM-OCR writes "lst"/"ledereen". Its
        // misses: two misread words, a closing quote added to a quotation that continues in the next
        // cue, and "- Hi!" for "-Hi!" on English dialogue dashes. Behind GLM-OCR and LFM3-VL because
        // it is a 5.2 GB download against their 1.4 GB and 1.3 GB. Q8_0 only: the i1-Q4_K_M was no better than
        // the 0.8B (143/150 Dutch vs 147/150 for Q8_0).
        // NoThinking: the Qwen 3.5 template thinks by default and the answer then lands in
        // reasoning_content, leaving content empty on most images. Border: the square cost it
        // accuracy on every corpus (synthetic 16/19 vs 17/19) and ~2.5x the time.
        new LlamaCppModel("LightOnOCR-3 4B (Q8_0)", "LightOnOCR-3-4B.Q8_0.gguf", "5.2 GB",
            "https://huggingface.co/mradermacher/LightOnOCR-3-4B-GGUF/resolve/main/LightOnOCR-3-4B.Q8_0.gguf",
            MmprojFileName: "mmproj-LightOnOCR-3-4B.Q8_0.gguf",
            MmprojUrl: "https://huggingface.co/mradermacher/LightOnOCR-3-4B-GGUF/resolve/main/LightOnOCR-3-4B.mmproj-f16.gguf",
            NoThinking: true,
            OcrPadding: AiOcrPadding.Border),
        // PaddlePaddle's official llama.cpp package - 109 languages (NaViT + ERNIE-4.5).
        new LlamaCppModel("PaddleOCR-VL 1.6", "PaddleOCR-VL-1.6-GGUF.gguf", "1.8 GB",
            "https://huggingface.co/PaddlePaddle/PaddleOCR-VL-1.6-GGUF/resolve/main/PaddleOCR-VL-1.6-GGUF.gguf",
            MmprojFileName: "PaddleOCR-VL-1.6-GGUF-mmproj.gguf",
            MmprojUrl: "https://huggingface.co/PaddlePaddle/PaddleOCR-VL-1.6-GGUF/resolve/main/PaddleOCR-VL-1.6-GGUF-mmproj.gguf"),
        // HunyuanOCR 1.5 (Tencent, ~1B). Was the fastest of the list when added (~1.6x GLM-OCR per
        // image on b10310); on b10625 that lead is gone - re-measured 2026-08-25 at 3.4 s/image
        // against GLM-OCR's 2.5 s, and it merges two-line subtitles into one line on 4 of 14
        // images. Verified 2026-08-15 on the pinned b10310 build, 9-image
        // EN/DE/FR/ES/IT/ZH/JA/RU subtitle corpus: recognition itself exact in every script,
        // but two formatting quirks keep it from being the default: it sporadically prefixes a
        // markdown "# " heading (1/9 images; immune to prompt wording, identical at bf16, and
        // unstrippable because genuine SDH "# lyrics" hash cues - which it preserves verbatim -
        // look the same), and it silently drops ♪ note marks. GLM-OCR gets both right.
        // Q8_0 only: bf16 measured identical on the same corpus while twice the size.
        // Border, not the square (2026-10-15, b10840, 60 English + 121 Arabic images): English
        // character error 1.4% vs 8.6% in the square, Arabic 8.1% vs 7.7%, and Video OCR crops
        // take 1.5 s instead of 16-21 s because the square triples the image tokens.
        new LlamaCppModel("HunyuanOCR 1.5 (Q8_0)", "HunyuanOCR-Q8_0.gguf", "1.3 GB",
            "https://huggingface.co/ggml-org/HunyuanOCR-GGUF/resolve/main/HunyuanOCR-Q8_0.gguf",
            MmprojFileName: "mmproj-HunyuanOCR-Q8_0.gguf",
            MmprojUrl: "https://huggingface.co/ggml-org/HunyuanOCR-GGUF/resolve/main/mmproj-HunyuanOCR-Q8_0.gguf",
            OcrPadding: AiOcrPadding.Border),
        // QARI-OCR 0.2.2.1 (NAMAA-Space, Apache-2.0): Qwen2-VL-2B fine-tuned for Arabic, here as
        // mradermacher's merged GGUF. The pick for Arabic, not a general default - see #16043.
        // Measured 2026-10-14 on b10840 with SE's flags, shared prompt and square pad over 61
        // Arabic images (20 HD bitmaps, 20 SD bitmaps, 20 burned-in 720p bottom-third crops, one
        // real anime frame), exact / character error / median s per image:
        //   QARI 0.2.2.1 29/61, 5.2% (HD bitmaps 0.6%), 4.5 s - no loops, no empty answers
        //   HunyuanOCR 26/61, 9.3%, 1.5 s; LFM2.5-VL 10/61, 10.1%; PaddleOCR-VL 4/61, 12.7%
        //   (writes أ/إ as ا and ى as ي); GLM-OCR 1/61, 56% (fluent invented Arabic);
        //   LightOnOCR 1/61 (loops); plain Qwen2-VL-2B 0/61 (chats about the image).
        // Rejected alongside it: QARI 0.3 (13-17/61, emits HTML tags - a document-structure
        // model), QARI 0.4 4B (25/61 with its own prompt, 0/61 under the shared prompt) and
        // Arabic-GLM-OCR-v2 (18/61 with 16 repetition loops and 7 empty answers).
        // It follows the shared prompt as-is and reads Latin text correctly, but it joins every
        // two-line subtitle into one line (all 12 two-line Arabic cases, and English too) and
        // turns ♪ into لَ, so it ranks behind the general models for non-Arabic subtitles.
        // Border, not the square (2026-10-15, 121 Arabic images): 7.3% character error vs 7.7%,
        // the same line breaks, and 2.6 s instead of 31 s per Video OCR crop. The square's only
        // edge is on HD bitmaps, and there only in how often a mixed Arabic/Latin line comes back
        // with its runs swapped (1 vs 3 of 40) - the words themselves are read equally well.
        new LlamaCppModel("QARI-OCR 0.2.2.1 Arabic 2B (Q8_0)", "Qari-OCR-0.2.2.1-VL-2B-Instruct-merged.Q8_0.gguf", "2.4 GB",
            "https://huggingface.co/mradermacher/Qari-OCR-0.2.2.1-VL-2B-Instruct-merged-GGUF/resolve/main/Qari-OCR-0.2.2.1-VL-2B-Instruct-merged.Q8_0.gguf",
            MmprojFileName: "mmproj-Qari-OCR-0.2.2.1-VL-2B-Instruct-merged.Q8_0.gguf",
            MmprojUrl: "https://huggingface.co/mradermacher/Qari-OCR-0.2.2.1-VL-2B-Instruct-merged-GGUF/resolve/main/Qari-OCR-0.2.2.1-VL-2B-Instruct-merged.mmproj-Q8_0.gguf",
            OcrPadding: AiOcrPadding.Border),
        // LightOnOCR-3 0.8B (LightOn, Apache-2.0, Qwen 3.5 VL) as Prathat's Q8_0 - no official GGUF.
        // Replaces LightOnOCR 1B (18.5 s/image, last in every measurement); anyone who already
        // downloaded that one still sees it, as a custom entry from the models folder. The fast
        // pick: 0.35-0.85 s per real image, 6-12x GLM-OCR, at about 1.0 GB. Last because it is the
        // least accurate of the general models on real data (379/390 vs GLM-OCR's 385): five
        // fluent misreads (four of them Dutch), a closing quote added to a quotation that continues
        // in the next cue, and "- " after dialogue dashes. Best of all on the synthetic corpus
        // (18/19), so the misses are language-prior guesses rather than unreadable images.
        // Q8_0 and f16 gave identical output. NoThinking and Border for the same reasons as the 4B.
        new LlamaCppModel("LightOnOCR-3 0.8B (Q8_0)", "LightOnOCR-3-0.8B-Q8_0.gguf", "1.0 GB",
            "https://huggingface.co/Prathat/LightOnOCR-3-0.8B-GGUF/resolve/main/LightOnOCR-3-0.8B-Q8_0.gguf",
            MmprojFileName: "mmproj-LightOnOCR-3-0.8B-Q8_0.gguf",
            MmprojUrl: "https://huggingface.co/Prathat/LightOnOCR-3-0.8B-GGUF/resolve/main/LightOnOCR-3-0.8B-mmproj-f16.gguf",
            NoThinking: true,
            OcrPadding: AiOcrPadding.Border),
    };

    /// <summary>
    /// The image framing for a curated OCR model given by its file name without extension (what
    /// the engines send as the model name). Unknown and self-supplied models get the square.
    /// </summary>
    public static AiOcrPadding GetOcrPadding(string? modelName)
    {
        if (string.IsNullOrWhiteSpace(modelName))
        {
            return AiOcrPadding.Square;
        }

        var model = OcrModels.FirstOrDefault(m =>
            Path.GetFileNameWithoutExtension(m.FileName).Equals(modelName.Trim(), StringComparison.OrdinalIgnoreCase));
        return model?.OcrPadding ?? AiOcrPadding.Square;
    }

    /// <summary>
    /// The OCR prompt to send for <paramref name="model"/>: the model's own
    /// <see cref="LlamaCppModel.PromptTemplate"/> when it has one and the user has left the shared
    /// OCR prompt at its default, otherwise the user's prompt. A user who has edited the shared
    /// prompt has opted into driving every model with it, so their wording wins even over a model
    /// that ships a tuned prompt. Callers pass the model as the user picked it; a null model (a
    /// user-managed server with an unknown model name) always gets the user's prompt.
    /// </summary>
    public static string ResolveOcrPrompt(LlamaCppModel? model, string? userPrompt)
    {
        var prompt = string.IsNullOrWhiteSpace(userPrompt) ? SeOcrDefaults.LlamaCppOcrPrompt : userPrompt;
        if (model?.PromptTemplate is { Length: > 0 } modelPrompt &&
            prompt.Trim() == SeOcrDefaults.LlamaCppOcrPrompt)
        {
            return modelPrompt;
        }

        return prompt;
    }

    /// <summary>
    /// Root folder holding the llama-server executable and the <c>models</c> subfolder.
    /// Defaults to <c>&lt;data folder&gt;/llama.cpp</c> via <see cref="Configuration.DataDirectory"/>
    /// (which the UI's Se config syncs at startup). Headless hosts (seconv) that resolve the
    /// folder differently set this before first use.
    /// </summary>
    public static string? FolderOverride { get; set; }

    /// <summary>
    /// Optional info-level log sink. The UI wires this to the tools log
    /// (<c>Se.WriteToolsLog</c>); seconv wires it to --verbose console output.
    /// </summary>
    public static Action<string>? LogAction { get; set; }

    /// <summary>
    /// Full path to the llama-server executable when it lives outside
    /// <see cref="GetAndCreateFolder"/> - e.g. seconv falling back to a llama.cpp
    /// install on the system PATH (brew/winget/apt). Models still resolve against
    /// the folder.
    /// </summary>
    public static string? ExecutableOverride { get; set; }

    private static readonly HttpClient HttpClient = new() { Timeout = TimeSpan.FromMinutes(5) };
    private static readonly SemaphoreSlim ServerLock = new(1, 1);
    private static Process? _serverProcess;
    private static int _serverPort;
    private static string? _serverModelPath;
    private static int _serverContextSize;
    private static string _serverExtraArguments = string.Empty;
    private static bool _serverExtraArgumentsOnly;
    private static bool _processExitHooked;
    private static readonly StringBuilder _serverLog = new();

    public static bool IsServerRunning => _serverProcess is { HasExited: false } && _serverPort != 0;

    public static string? RunningModelPath => IsServerRunning ? _serverModelPath : null;

    /// <summary>Context size the running server was started with, or 0 when nothing is running.</summary>
    public static int RunningContextSize => IsServerRunning ? _serverContextSize : 0;

    public static string ApiUrl => $"http://127.0.0.1:{_serverPort}/v1/chat/completions";

    public static string GetAndCreateFolder()
    {
        var folder = FolderOverride;
        if (string.IsNullOrEmpty(folder))
        {
            var dataFolder = string.IsNullOrEmpty(Configuration.DataDirectory)
                ? AppContext.BaseDirectory
                : Configuration.DataDirectory;
            folder = Path.Combine(dataFolder, "llama.cpp");
        }

        if (!Directory.Exists(folder))
        {
            Directory.CreateDirectory(folder);
        }

        return folder;
    }

    public static string GetAndCreateModelsFolder()
    {
        var folder = Path.Combine(GetAndCreateFolder(), "models");
        if (!Directory.Exists(folder))
        {
            Directory.CreateDirectory(folder);
        }

        return folder;
    }

    public static string GetExecutable()
    {
        if (!string.IsNullOrEmpty(ExecutableOverride))
        {
            return ExecutableOverride;
        }

        return Path.Combine(GetAndCreateFolder(), OperatingSystem.IsWindows() ? "llama-server.exe" : "llama-server");
    }

    public static bool IsEngineInstalled()
    {
        return File.Exists(GetExecutable());
    }

    public static string GetModelPath(string fileName)
    {
        return Path.Combine(GetAndCreateModelsFolder(), fileName);
    }

    public static bool IsModelInstalled(string fileName)
    {
        var path = GetModelPath(fileName);
        return File.Exists(path) && new FileInfo(path).Length > 10_000_000;
    }

    /// <summary>
    /// Picks the llama-server chat-template flags for a <c>.gguf</c> we do not curate (a file the user
    /// downloaded themselves, e.g. a TranslateGemma quant or size we do not offer). A curated entry with
    /// the same file name wins; otherwise the family is guessed from the file name, because getting this
    /// wrong is not cosmetic: every Gemma (2/3) we ship needs <c>gemma</c> + <c>--no-jinja</c>
    /// (TranslateGemma's embedded Jinja template is non-standard), and every Qwen needs
    /// <c>--reasoning off</c> (<c>NoThinking</c>) to suppress the hybrid template's thinking mode, which
    /// otherwise emits &lt;think&gt; blocks instead of a translation - see the note on the curated Qwen
    /// entries in <see cref="TranslateModels"/> for why this is <c>NoThinking</c> and not a
    /// <c>chatml</c>/<c>--no-jinja</c> template override. Families with a usable embedded template (Aya,
    /// Llama, EuroLLM, Phi) fall through to the default of no override. Gemma 4 also keeps its embedded
    /// template and needs <c>--reasoning off</c>, for the same reason documented on
    /// <see cref="LlamaCppModel.NoThinking"/>.
    /// </summary>
    public static (string? ChatTemplate, bool NoJinja, bool NoThinking) InferChatTemplate(string fileName)
    {
        var curated = TranslateModels.Concat(ReviewModels).Concat(OcrModels)
            .FirstOrDefault(m => m.FileName.Equals(fileName, StringComparison.OrdinalIgnoreCase));
        if (curated != null)
        {
            return (curated.ChatTemplate, curated.NoJinja, curated.NoThinking);
        }

        // Gemma 4 dropped the <start_of_turn> scheme for <|turn>role ... <turn|>, so the built-in
        // "gemma" template does not apply - fall through and let its embedded Jinja template win,
        // with thinking turned off so the translation lands in message.content.
        if (IsGemma4FileName(fileName))
        {
            return (null, false, true);
        }

        // Matches "translategemma-27b-it.Q4_K_M.gguf", "google_gemma-3-27b-it-Q4_K_M.gguf", etc.
        if (fileName.Contains("gemma", StringComparison.OrdinalIgnoreCase))
        {
            return ("gemma", true, false);
        }

        // "afrislm" / "index-translate": TranslatePsy-AfriSLM and bilibili's Index-Translate are
        // Qwen 3.5 fine-tunes that think by default just like their base, but no file of those
        // families carries "qwen" in its name.
        if (fileName.Contains("qwen", StringComparison.OrdinalIgnoreCase) ||
            fileName.Contains("afrislm", StringComparison.OrdinalIgnoreCase) ||
            fileName.Contains("index-translate", StringComparison.OrdinalIgnoreCase))
        {
            return (null, false, true);
        }

        return (null, false, false);
    }

    /// <summary>
    /// True when the file name names the Gemma <b>4</b> family, as opposed to a <b>4B</b> model of
    /// another Gemma family. Plain "contains gemma-4" is not enough: "translategemma-4b-it-q8_0.gguf"
    /// contains it too, and treating that TranslateGemma 4B quant as a Gemma 4 would drop the
    /// <c>gemma</c> chat template it needs. The version digit is therefore only accepted when the
    /// next character is not a letter - "gemma-4-12B", "gemma-4-E4B" and "gemma4_sub" are the family,
    /// "gemma-4b" is a size.
    /// </summary>
    internal static bool IsGemma4FileName(string fileName)
    {
        foreach (var marker in new[] { "gemma-4", "gemma4" })
        {
            var i = fileName.IndexOf(marker, StringComparison.OrdinalIgnoreCase);
            while (i >= 0)
            {
                var after = i + marker.Length;
                if (after >= fileName.Length || !char.IsLetter(fileName[after]))
                {
                    return true;
                }

                i = fileName.IndexOf(marker, after, StringComparison.OrdinalIgnoreCase);
            }
        }

        return false;
    }

    /// <summary>
    /// Builds the <see cref="LlamaCppModel"/> for a non-curated <c>*.gguf</c> (dropped into the
    /// models folder, or passed to seconv by path), inferring the same per-family settings the
    /// curated entries carry: chat-template flags via <see cref="InferChatTemplate"/>, and for
    /// MiLMMT quants the trained completion prompt, greedy sampling and the regular-engine-only
    /// restriction - a self-supplied MiLMMT quant echo-loops the source under any other prompt.
    /// <paramref name="fileNameOrPath"/> may be a bare file name or a full path (seconv).
    /// </summary>
    public static LlamaCppModel CreateCustomModel(string displayName, string fileNameOrPath, string size)
    {
        var name = Path.GetFileName(fileNameOrPath);
        var (chatTemplate, noJinja, noThinking) = InferChatTemplate(name);
        var isMiLmMt = name.Contains("milmmt", StringComparison.OrdinalIgnoreCase);
        return new LlamaCppModel(displayName, fileNameOrPath, size, Url: string.Empty,
            ChatTemplate: chatTemplate, NoJinja: noJinja,
            PromptTemplate: isMiLmMt ? MiLmMt46PromptTemplate : null,
            Temperature: isMiLmMt ? 0 : -1,
            CompletionOnly: isMiLmMt,
            NoThinking: noThinking);
    }

    /// <summary>
    /// Builds the <see cref="LlamaCppModel"/> for a non-curated vision <c>*.gguf</c> used for OCR:
    /// the model plus the <paramref name="mmprojFileNameOrPath"/> vision projector it is served with
    /// (<c>--mmproj</c>) - without one llama-server loads the model blind and every image comes back
    /// as a hallucination. Unlike <see cref="CreateCustomModel"/> this never overrides the chat
    /// template: a multimodal GGUF's embedded template is what encodes the image placeholder, so
    /// forcing e.g. <c>gemma</c> + <c>--no-jinja</c> on a Gemma-named vision model would drop the
    /// image from the prompt entirely. Only the thinking switch is inferred, for the same reason as
    /// on the translate side (a thinking model answers into <c>reasoning_content</c> and leaves
    /// <c>content</c> - the only field the OCR engines read - empty).
    /// </summary>
    public static LlamaCppModel CreateCustomOcrModel(string displayName, string fileNameOrPath, string size, string mmprojFileNameOrPath)
    {
        var fileName = Path.GetFileName(fileNameOrPath);
        var (_, _, noThinking) = InferChatTemplate(fileName);
        return new LlamaCppModel(displayName, fileNameOrPath, size, Url: string.Empty,
            MmprojFileName: mmprojFileNameOrPath,
            NoThinking: noThinking,
            PromptTemplate: IsLfmVisionModel(fileName) ? SeOcrDefaults.LlamaCppOcrPromptLfmVl : null);
    }

    /// <summary>
    /// An LFM vision model (LFM2.5-VL, LFM3-VL, any quant) answers the shared OCR prompt's line count
    /// literally, so a self-supplied one gets the LFM prompt too. This also keeps LFM2.5-VL working
    /// for users who downloaded it while it was curated: it now shows up as a custom model.
    /// </summary>
    private static bool IsLfmVisionModel(string fileName)
    {
        return fileName.StartsWith("LFM", StringComparison.OrdinalIgnoreCase) &&
               fileName.Contains("-VL", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// The vision projector sitting next to <paramref name="modelPath"/>, or null when there is none.
    /// Covers both curated sidecar conventions: <c>mmproj-&lt;file&gt;</c> (GLM-OCR, LightOnOCR,
    /// HunyuanOCR) and <c>&lt;stem&gt;-mmproj.gguf</c> (PaddleOCR-VL) - the two names HuggingFace
    /// GGUF repos publish vision projectors under, so a self-supplied vision model downloaded from
    /// one of them is recognised as-is.
    /// </summary>
    public static string? FindMmprojSidecar(string modelPath)
    {
        var dir = Path.GetDirectoryName(modelPath);
        if (string.IsNullOrEmpty(dir))
        {
            return null;
        }

        var candidates = new[]
        {
            Path.Combine(dir, "mmproj-" + Path.GetFileName(modelPath)),
            Path.Combine(dir, Path.GetFileNameWithoutExtension(modelPath) + "-mmproj.gguf"),
        };
        return candidates.FirstOrDefault(File.Exists);
    }

    /// <summary>True when the file name is a vision projector rather than a model in its own right.</summary>
    private static bool IsMmprojFileName(string fileName)
    {
        return fileName.StartsWith("mmproj-", StringComparison.OrdinalIgnoreCase) ||
               fileName.EndsWith("-mmproj.gguf", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Points the regular llama.cpp translate engine's per-model settings (trained-in prompt and
    /// recommended sampling) at the given curated/custom model, or resets them for null (remote
    /// server / unknown model). Must be called wherever a local translate run picks its model
    /// (Auto-translate window, batch convert, seconv) - the values persist in settings, so a
    /// stale prompt from a previously used model would otherwise leak into the next run, and for
    /// completion-only models (MiLMMT-46) the wrong prompt does not just degrade output, it makes
    /// the model echo the untranslated source.
    /// </summary>
    public static void ApplyTranslatePromptSettings(LlamaCppModel? model)
    {
        Configuration.Settings.Tools.LlamaCppModelPrompt = model?.PromptTemplate ?? string.Empty;
        Configuration.Settings.Tools.LlamaCppModelTemperature = model?.Temperature ?? -1;
        Configuration.Settings.Tools.LlamaCppModelTopP = model?.TopP ?? -1;
        Configuration.Settings.Tools.LlamaCppModelTopK = model?.TopK ?? -1;
        Configuration.Settings.Tools.LlamaCppModelRepeatPenalty = model?.RepeatPenalty ?? -1;
    }

    /// <summary>
    /// Returns the curated <see cref="TranslateModels"/> plus any other <c>*.gguf</c> the user has
    /// dropped into the llama.cpp models folder. Custom entries are emitted with an empty <c>Url</c>
    /// (no download needed - already on disk), the file name as <c>DisplayName</c>, a
    /// human-readable file size, and chat-template flags from <see cref="InferChatTemplate"/> so a
    /// self-supplied TranslateGemma/Qwen quant is served with the same flags as the curated ones.
    /// Vision projectors (<c>mmproj-*.gguf</c> and <c>*-mmproj.gguf</c>) are skipped because they're
    /// not standalone translation models.
    /// </summary>
    public static IReadOnlyList<LlamaCppModel> GetAllTranslateModels()
    {
        return GetCuratedPlusCustomModels(TranslateModels);
    }

    public static IReadOnlyList<LlamaCppModel> GetAllReviewModels()
    {
        return GetCuratedPlusCustomModels(ReviewModels);
    }

    /// <summary>
    /// Returns the curated <see cref="OcrModels"/> plus any self-supplied vision <c>*.gguf</c> in the
    /// llama.cpp models folder. Only files that have a vision projector next to them
    /// (<see cref="FindMmprojSidecar"/>) qualify: a text-only model served to the OCR engines cannot
    /// see the image at all, and the sidecar is what tells the two apart without opening the GGUF.
    /// </summary>
    public static IReadOnlyList<LlamaCppModel> GetAllOcrModels()
    {
        return GetCuratedPlusCustomModels(OcrModels, requireVisionProjector: true);
    }

    private static IReadOnlyList<LlamaCppModel> GetCuratedPlusCustomModels(IReadOnlyList<LlamaCppModel> curated, bool requireVisionProjector = false)
    {
        var folder = GetAndCreateModelsFolder();
        if (!Directory.Exists(folder))
        {
            return curated;
        }

        // "known" spans all curated lists so e.g. a downloaded review model does not show up
        // as a custom entry in the translate list (and vice versa).
        var knownNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var m in TranslateModels.Concat(ReviewModels).Concat(OcrModels))
        {
            knownNames.Add(m.FileName);
            if (!string.IsNullOrEmpty(m.MmprojFileName))
            {
                knownNames.Add(m.MmprojFileName);
            }
        }

        var custom = new List<LlamaCppModel>();
        try
        {
            foreach (var path in Directory.EnumerateFiles(folder, "*.gguf", SearchOption.TopDirectoryOnly))
            {
                var name = Path.GetFileName(path);
                if (knownNames.Contains(name))
                {
                    continue;
                }
                if (IsMmprojFileName(name))
                {
                    continue;
                }

                var mmproj = requireVisionProjector ? FindMmprojSidecar(path) : null;
                if (requireVisionProjector && mmproj == null)
                {
                    continue;
                }

                var size = FormatFileSize(new FileInfo(path).Length);
                custom.Add(mmproj == null
                    ? CreateCustomModel(name, name, size)
                    : CreateCustomOcrModel(name, name, size, Path.GetFileName(mmproj)));
            }
        }
        catch
        {
            // ignore - if the folder can't be scanned (locked / IO error) just fall back to curated only.
            return curated;
        }

        if (custom.Count == 0)
        {
            return curated;
        }

        custom.Sort((a, b) => string.Compare(a.FileName, b.FileName, StringComparison.OrdinalIgnoreCase));
        return curated.Concat(custom).ToList();
    }

    private static string FormatFileSize(long bytes)
    {
        const double gb = 1024d * 1024d * 1024d;
        const double mb = 1024d * 1024d;
        if (bytes >= gb)
        {
            return (bytes / gb).ToString("0.#", CultureInfo.InvariantCulture) + " GB";
        }
        if (bytes >= mb)
        {
            return (bytes / mb).ToString("0", CultureInfo.InvariantCulture) + " MB";
        }
        return (bytes / 1024d).ToString("0", CultureInfo.InvariantCulture) + " KB";
    }

    public static bool IsModelInstalled(LlamaCppModel model)
    {
        if (!IsModelInstalled(model.FileName))
        {
            return false;
        }

        if (model.MmprojFileName == null)
        {
            return true;
        }

        return IsModelInstalled(model.MmprojFileName);
    }

    /// <summary>
    /// Starts (or reuses) a llama-server for the given model and points
    /// <see cref="Core.Settings.ToolsSettings.LlamaCppApiUrl"/> at it. Throws on failure.
    /// </summary>
    public const int DefaultContextSize = 8192;

    /// <param name="extraArgumentsOnly">
    /// Launches llama-server with <paramref name="extraArguments"/> instead of SE's curated flags
    /// (-ngl/-c/-np/--swa-full/--cache-reuse and the chat-template pair), for users who want full
    /// control over the server configuration. The model, host and port are always passed - SE has
    /// to know which model it is talking to and where. (#13865)
    /// </param>
    public static async Task EnsureServerRunningAsync(LlamaCppModel model, CancellationToken cancellationToken, int contextSize = DefaultContextSize, string? extraArguments = null, bool extraArgumentsOnly = false)
    {
        var extraArgs = extraArguments?.Trim() ?? string.Empty;
        var argsOnly = extraArgumentsOnly && extraArgs.Length > 0;
        var modelPath = GetModelPath(model.FileName);
        if (IsRunningWith(modelPath, contextSize, extraArgs, argsOnly))
        {
            Configuration.Settings.Tools.LlamaCppApiUrl = ApiUrl;
            return;
        }

        await ServerLock.WaitAsync(cancellationToken);
        try
        {
            if (IsRunningWith(modelPath, contextSize, extraArgs, argsOnly))
            {
                Configuration.Settings.Tools.LlamaCppApiUrl = ApiUrl;
                return;
            }

            // Server not running, or running with a different model - (re)start.
            if (_serverProcess != null)
            {
                StopServerInternal();
            }

            var exe = GetExecutable();
            if (!File.Exists(exe))
            {
                throw new FileNotFoundException("llama-server executable not found - please download llama.cpp first.", exe);
            }

            if (!File.Exists(modelPath))
            {
                throw new FileNotFoundException("llama.cpp model not found - please download a model first.", modelPath);
            }

            string? mmprojPath = null;
            if (model.MmprojFileName != null)
            {
                mmprojPath = GetModelPath(model.MmprojFileName);
                if (!File.Exists(mmprojPath))
                {
                    throw new FileNotFoundException("llama.cpp vision projector not found - please download the model first.", mmprojPath);
                }
            }

            var port = FindFreeLoopbackPort();
            var psi = new ProcessStartInfo
            {
                WorkingDirectory = Path.GetDirectoryName(exe) ?? GetAndCreateFolder(),
                FileName = exe,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
            };
            foreach (var arg in BuildServerArguments(model, modelPath, mmprojPath, port, contextSize, extraArgs, argsOnly))
            {
                psi.ArgumentList.Add(arg);
            }

            var process = Process.Start(psi)
                ?? throw new InvalidOperationException("Failed to start llama-server");

            LogAction?.Invoke($"llama-server starting - PID: {process.Id}, Cmd: {FormatLaunchCommand(exe, psi.ArgumentList)}");

            lock (_serverLog)
            {
                _serverLog.Clear();
            }

            process.ErrorDataReceived += (_, e) =>
            {
                if (e.Data != null)
                {
                    lock (_serverLog) _serverLog.AppendLine(e.Data);
                }
            };
            process.OutputDataReceived += (_, e) =>
            {
                if (e.Data != null)
                {
                    lock (_serverLog) _serverLog.AppendLine(e.Data);
                }
            };
            process.BeginErrorReadLine();
            process.BeginOutputReadLine();

            _serverProcess = process;
            _serverPort = port;
            _serverModelPath = modelPath;
            _serverContextSize = contextSize;
            _serverExtraArguments = extraArgs;
            _serverExtraArgumentsOnly = argsOnly;
            HookProcessExitOnce();

            var deadline = DateTime.UtcNow.AddMinutes(5);
            while (DateTime.UtcNow < deadline)
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (process.HasExited)
                {
                    var tail = SnapshotServerLog();
                    _serverProcess = null;
                    _serverPort = 0;
                    _serverModelPath = null;
                    throw new InvalidOperationException(
                        $"llama-server exited during startup (code {process.ExitCode}). Output: {tail}");
                }

                if (await ProbeHealthAsync(port, TimeSpan.FromSeconds(2), cancellationToken))
                {
                    Configuration.Settings.Tools.LlamaCppApiUrl = ApiUrl;
                    return;
                }

                await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
            }

            var lastOutput = SnapshotServerLog();
            StopServerInternal();
            throw new TimeoutException(
                $"llama-server did not report healthy within 5 minutes. Last output: {lastOutput}");
        }
        finally
        {
            ServerLock.Release();
        }
    }

    /// <summary>
    /// The llama-server command line for one launch. The model, host and port are always ours -
    /// SE has to know which model it is talking to and where - and the user's own arguments always
    /// come last, so a repeated flag (e.g. -ngl, -c) overrides SE's value: llama-server applies
    /// later arguments over earlier ones. <paramref name="argsOnly"/> drops SE's curated tuning
    /// altogether, for users who want full control; without it a bare switch such as --swa-full
    /// cannot be turned off at all, since there is nothing to repeat with a different value (#13865).
    /// </summary>
    internal static List<string> BuildServerArguments(
        LlamaCppModel model,
        string modelPath,
        string? mmprojPath,
        int port,
        int contextSize,
        string extraArgs,
        bool argsOnly)
    {
        var args = new List<string> { "-m", modelPath };
        if (mmprojPath != null)
        {
            args.Add("--mmproj");
            args.Add(mmprojPath);
        }

        args.Add("--host");
        args.Add("127.0.0.1");
        args.Add("--port");
        args.Add(port.ToString(CultureInfo.InvariantCulture));

        if (!argsOnly)
        {
            // Offload all layers to the GPU when a GPU build is in use; ignored by the CPU build.
            args.Add("-ngl");
            args.Add("99");
            args.Add("-c");
            args.Add(contextSize.ToString(CultureInfo.InvariantCulture));
            // SE is the server's only client, but llama-server defaults to 4 parallel slots,
            // which silently splits -c four ways (8192 became 2048 usable tokens per request).
            args.Add("-np");
            args.Add("1");
            // Keep the full KV cache for sliding-window-attention models (Gemma, Qwen 3.5);
            // without this their prompt cache only works on byte-identical requests and
            // cache_prompt reuse is lost entirely. Costs some KV memory at these context sizes.
            args.Add("--swa-full");
            if (mmprojPath == null)
            {
                // Chunk-level KV-cache reuse after the first diverging token; together with the
                // clients' cache_prompt this keeps repeated prompt prefixes (system prompt,
                // rolling context) from being re-ingested every request. Auto-disables with a
                // warning on models whose context cannot shift. Not combined with multimodal -
                // vision chunks cannot be shifted.
                args.Add("--cache-reuse");
                args.Add("256");
            }

            if (model.NoThinking)
            {
                args.Add("--reasoning");
                args.Add("off");
            }

            if (model.NoJinja)
            {
                args.Add("--no-jinja");
            }

            if (model.ChatTemplate != null)
            {
                args.Add("--chat-template");
                args.Add(model.ChatTemplate);
            }
        }

        args.AddRange(SplitCommandLineArguments(extraArgs));
        return args;
    }

    private static bool IsRunningWith(string modelPath, int contextSize, string extraArgs, bool argsOnly)
    {
        return IsServerRunning &&
               _serverModelPath == modelPath &&
               _serverExtraArguments == extraArgs &&
               _serverExtraArgumentsOnly == argsOnly &&
               // With SE's flags suppressed the context size comes from the user's own arguments
               // (or the server default), so the requested value says nothing about the running one.
               (argsOnly || _serverContextSize == contextSize);
    }

    public static void StopServer()
    {
        ServerLock.Wait();
        try
        {
            StopServerInternal();
        }
        finally
        {
            ServerLock.Release();
        }
    }

    private static void StopServerInternal()
    {
        var p = _serverProcess;
        _serverProcess = null;
        _serverPort = 0;
        _serverModelPath = null;
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

    public static string SnapshotServerLog()
    {
        lock (_serverLog)
        {
            var s = _serverLog.ToString().TrimEnd();
            return s.Length > 2000 ? s[^2000..] : s;
        }
    }

    private static async Task<bool> ProbeHealthAsync(int port, TimeSpan timeout, CancellationToken cancellationToken)
    {
        try
        {
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(timeout);
            using var resp = await HttpClient.GetAsync($"http://127.0.0.1:{port}/health", cts.Token);
            return resp.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Splits a user-entered argument string on whitespace, honoring single/double quotes so
    /// values with spaces survive (e.g. <c>--override-kv "key=str:some value"</c>).
    /// </summary>
    internal static List<string> SplitCommandLineArguments(string arguments)
    {
        var result = new List<string>();
        if (string.IsNullOrWhiteSpace(arguments))
        {
            return result;
        }

        var current = new StringBuilder();
        var quote = '\0';
        foreach (var ch in arguments)
        {
            if (quote != '\0')
            {
                if (ch == quote)
                {
                    quote = '\0';
                }
                else
                {
                    current.Append(ch);
                }
            }
            else if (ch == '"' || ch == '\'')
            {
                quote = ch;
            }
            else if (char.IsWhiteSpace(ch))
            {
                if (current.Length > 0)
                {
                    result.Add(current.ToString());
                    current.Clear();
                }
            }
            else
            {
                current.Append(ch);
            }
        }

        if (current.Length > 0)
        {
            result.Add(current.ToString());
        }

        return result;
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
}
