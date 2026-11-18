using Nikse.SubtitleEdit.Core.SubtitleFormats;

namespace Nikse.SubtitleEdit.UiLogic.AutoTranslate
{
    /// <summary>
    /// Builds the user message the local-LLM translate engines send, in either of the two prompt
    /// shapes a model can need.
    /// <para>
    /// Chat models take an instruction and the text after it, which is the historical wire format:
    /// the prompt, a real blank line, then the text - line breaks inside either part encoded as the
    /// "&lt;br /&gt;" placeholder the engines decode again on the way back.
    /// </para>
    /// <para>
    /// Completion-format models are trained on one block of text with the source embedded in it and
    /// a trailing target-language cue, and they only translate when they get exactly that. A prompt
    /// carrying <c>{2}</c> is such a template: it is filled in and sent verbatim, with real line
    /// breaks (under "&lt;br /&gt;" encoding MiLMMT-46 starts mirroring placeholder fragments into
    /// its output). Issue #13803 - this used to work only with the built-in llama.cpp engine, so the
    /// same model behind LM Studio, KoboldCpp, Ollama or any OpenAI-compatible server could not be
    /// prompted correctly.
    /// </para>
    /// </summary>
    public static class LlmTranslatePrompt
    {
        /// <summary>
        /// True when the template embeds the text itself (<c>{2}</c>) rather than expecting it
        /// appended after the prompt.
        /// </summary>
        public static bool IsCompletionTemplate(string template)
        {
            return template != null && template.Contains("{2}");
        }

        /// <summary>
        /// Fills a completion-format template: {0} = source language English name, {1} = target
        /// language English name, {2} = the text to translate, {3} / {4} = source / target language
        /// code (see <see cref="ToLanguageCode"/>; PLaMo 3 Translate is cued with a code, not a
        /// name). Placeholders are replaced rather than string.Format'ed, so braces in a user-edited
        /// prompt cannot throw, and the text goes in last so brace sequences in subtitle text (ASSA
        /// override tags) can never hit one.
        /// </summary>
        public static string FillCompletionTemplate(string template, string sourceLanguage, string targetLanguage, string text)
        {
            var filled = template
                .Replace("{0}", sourceLanguage)
                .Replace("{1}", targetLanguage);
            if (filled.Contains("{3}"))
            {
                filled = filled.Replace("{3}", ToLanguageCode(sourceLanguage));
            }

            if (filled.Contains("{4}"))
            {
                filled = filled.Replace("{4}", ToLanguageCode(targetLanguage));
            }

            return filled.Replace("{2}", text.Trim());
        }

        /// <summary>
        /// The lowercase language code for an English language name from the LLM engines' language
        /// list ("Japanese" -> "ja"). Simplified and Traditional Chinese use the script subtags
        /// ("zh-hans" / "zh-hant") rather than a region. A value that is not a listed name - a code
        /// seconv was given, say - comes back lowercased as it is.
        /// </summary>
        public static string ToLanguageCode(string language)
        {
            var trimmed = (language ?? string.Empty).Trim();
            var pair = ChatGptTranslate.ListLanguages()
                .FirstOrDefault(p => p.Name.Equals(trimmed, StringComparison.OrdinalIgnoreCase));
            var code = (pair?.TwoLetterIsoLanguageName ?? trimmed).ToLowerInvariant();
            return code switch
            {
                "zh-cn" or "zh" => "zh-hans",
                "zh-tw" => "zh-hant",
                _ => code,
            };
        }

        /// <summary>
        /// The JSON-encoded content of the user message, ready to drop into a request body.
        /// </summary>
        public static string BuildEncodedUserMessage(string template, string sourceLanguage, string targetLanguage, string text)
        {
            if (IsCompletionTemplate(template))
            {
                return Json.EncodeJsonText(FillCompletionTemplate(template, sourceLanguage, targetLanguage, text), "\\n");
            }

            var prompt = template
                .Replace("{0}", sourceLanguage)
                .Replace("{1}", targetLanguage);

            return Json.EncodeJsonText(prompt) + "\\n\\n" + Json.EncodeJsonText(text.Trim());
        }

        /// <summary>
        /// The user message as readable text with <c>&lt;text&gt;</c> where the subtitle text goes:
        /// what <see cref="BuildEncodedUserMessage"/> sends, before the JSON encoding. For seconv's
        /// --verbose output.
        /// </summary>
        public static string DescribeUserMessage(string template, string sourceLanguage, string targetLanguage)
        {
            const string placeholder = "<text>";
            if (IsCompletionTemplate(template))
            {
                return FillCompletionTemplate(template, sourceLanguage, targetLanguage, placeholder);
            }

            return template
                .Replace("{0}", sourceLanguage)
                .Replace("{1}", targetLanguage) + "\n\n" + placeholder;
        }
    }
}
