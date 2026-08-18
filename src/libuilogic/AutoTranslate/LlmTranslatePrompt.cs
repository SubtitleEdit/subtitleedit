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
        /// language English name, {2} = the text to translate. Placeholders are replaced rather
        /// than string.Format'ed, so braces in a user-edited prompt cannot throw, and the text goes
        /// in last so brace sequences in subtitle text (ASSA override tags) can never hit one.
        /// </summary>
        public static string FillCompletionTemplate(string template, string sourceLanguage, string targetLanguage, string text)
        {
            return template
                .Replace("{0}", sourceLanguage)
                .Replace("{1}", targetLanguage)
                .Replace("{2}", text.Trim());
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
    }
}
