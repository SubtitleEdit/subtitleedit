﻿using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.Settings;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Core.Translate;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Core.AutoTranslate
{
    public class ChatGptTranslate : IAutoTranslator, IDisposable
    {
        private static readonly Regex UnicodeRegex = new Regex(@"\\u([0-9a-fA-F]{4})", RegexOptions.Compiled);

        private HttpClient _httpClient;

        public static string StaticName { get; set; } = "ChatGPT";
        public override string ToString() => StaticName;
        public string Name => StaticName;
        public string Url => "https://chat.openai.com/";
        public string Error { get; set; }
        public int MaxCharacters => 1500;
        public static string[] Models => new[]
        {
            "gpt-5-mini", "gpt-5-nano",  "gpt-5", "gpt-4.1-mini", "gpt-4.1-nano", "gpt-4.1", "gpt-oss-120b",
        };

        public static string RemovePreamble(string original, string input)
        {
            if (original.Contains(":") && input.IndexOf("<think>") < 0)
            {
                return input;
            }

            var translation = input;
            var indexOfStartThink = translation.IndexOf("<think>");
            var indexOfEndThink = translation.IndexOf("</think>");
            if (indexOfStartThink >= 0 && indexOfEndThink > indexOfStartThink)
            {
                translation = translation.Remove(indexOfStartThink, indexOfEndThink - indexOfStartThink + 8).Trim();
            }

            var regex = new Regex(@"^(Here is|Here's) [a-zA-Z ,]+:");
            var match = regex.Match(translation);
            if (match.Success)
            {
                var result = translation.Remove(match.Index, match.Value.Length);
                return result.Trim();
            }

            return translation;
        }

        public void Initialize()
        {
            _httpClient?.Dispose();
            _httpClient = HttpClientFactoryWithProxy.CreateHttpClientWithProxy();
            _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/json");
            _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("accept", "application/json");
            _httpClient.BaseAddress = new Uri(Configuration.Settings.Tools.ChatGptUrl.TrimEnd('/'));
            _httpClient.Timeout = TimeSpan.FromMinutes(15);

            if (!string.IsNullOrEmpty(Configuration.Settings.Tools.ChatGptApiKey))
            {
                _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", "Bearer " + Configuration.Settings.Tools.ChatGptApiKey);
            }
        }

        public List<TranslationPair> GetSupportedSourceLanguages()
        {
            return ListLanguages();
        }

        public List<TranslationPair> GetSupportedTargetLanguages()
        {
            return ListLanguages();
        }

        public async Task<string> Translate(string text, string sourceLanguageCode, string targetLanguageCode, CancellationToken cancellationToken)
        {
            var model = Configuration.Settings.Tools.ChatGptModel;
            if (string.IsNullOrEmpty(model))
            {
                model = Models[0];
                Configuration.Settings.Tools.ChatGptModel = model;
            }

            if (string.IsNullOrEmpty(Configuration.Settings.Tools.ChatGptPrompt))
            {
                Configuration.Settings.Tools.ChatGptPrompt = new ToolsSettings().ChatGptPrompt;
            }
            var prompt = string.Format(Configuration.Settings.Tools.ChatGptPrompt, sourceLanguageCode, targetLanguageCode);
            var input = "{\"model\": \"" + model + "\",\"messages\": [{ \"role\": \"user\", \"content\": \"" + Json.EncodeJsonText(prompt) + "\\n\\n" + Json.EncodeJsonText(text.Trim()) + "\" }]}";
            var content = new StringContent(input, Encoding.UTF8);
            content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
            var result = await _httpClient.PostAsync(string.Empty, content, cancellationToken);
            var bytes = await result.Content.ReadAsByteArrayAsync();
            var json = Encoding.UTF8.GetString(bytes).Trim();
            if (!result.IsSuccessStatusCode)
            {
                Error = json;
                SeLogger.Error("ChatGptTranslate failed calling API: Status code=" + result.StatusCode + Environment.NewLine + json);
            }

            result.EnsureSuccessStatusCode();

            var parser = new SeJsonParser();
            var resultText = parser.GetFirstObject(json, "content");
            if (resultText == null)
            {
                return string.Empty;
            }

            var outputText = Json.DecodeJsonText(resultText).Trim();
            if (outputText.StartsWith('"') && outputText.EndsWith('"') && !text.StartsWith('"'))
            {
                outputText = outputText.Trim('"').Trim();
            }

            outputText = FixNewLines(outputText);
            outputText = RemovePreamble(text, outputText);
            outputText = DecodeUnicodeEscapes(outputText);
            return outputText.Trim();
        }

        public static List<TranslationPair> ListLanguages()
        {
            return new List<TranslationPair>
            {
               MakePair("Albanian","sq"),
               MakePair("Arabic","ar"),
               MakePair("Armenian","hy"),
               MakePair("Awadhi","ay"),
               MakePair("Azerbaijani","az"),
               MakePair("Bashkir","ba"),
               MakePair("Basque","eu"),
               MakePair("Belarusian","be"),
               MakePair("Bengali","bn"),
               MakePair("Bhojpuri",""),
               MakePair("Bosnian","bs"),
               MakePair("Brazilian Portuguese","br"),
               MakePair("Bulgarian","bg"),
               MakePair("Cantonese","zh"),
               MakePair("Catalan","ca"),
               MakePair("Chhattisgarhi",""),
               MakePair("Chinese (Simplified)", "zho-Hans"),
               MakePair("Chinese (Traditional)","zh-Hant"),
               MakePair("Croatian","hr"),
               MakePair("Czech","cs"),
               MakePair("Danish","da"),
               MakePair("Dogri",""),
               MakePair("Dutch","nl"),
               MakePair("English","en"),
               MakePair("Estonian","et"),
               MakePair("Faroese","fo"),
               MakePair("Finnish","fi"),
               MakePair("French","fr"),
               MakePair("Galician","gl"),
               MakePair("Georgian","ka"),
               MakePair("German","de"),
               MakePair("Greek","el"),
               MakePair("Gujarati","gu"),
               MakePair("Haryanvi",""),
               MakePair("Hebrew","iw"),
               MakePair("Hindi","hi"),
               MakePair("Hungarian","hu"),
               MakePair("Icelandic","is"),
               MakePair("Indonesian","id"),
               MakePair("Irish","ga"),
               MakePair("Italian","it"),
               MakePair("Japanese","ja"),
               MakePair("Javanese","jv"),
               MakePair("Kannada","kn"),
               MakePair("Kashmiri","ks"),
               MakePair("Kazakh","kk"),
               MakePair("Konkani",""),
               MakePair("Korean","ko"),
               MakePair("Kyrgyz","ky"),
               MakePair("Latvian","lv"),
               MakePair("Lithuanian","lt"),
               MakePair("Macedonian","mk"),
               MakePair("Maithili",""),
               MakePair("Malay","ms"),
               MakePair("Maltese","mt"),
               MakePair("Mandarin","zh"),
               MakePair("Mandarin Chinese","zh"),
               MakePair("Marathi","mr"),
               MakePair("Marwari",""),
               MakePair("Min Nan",""),
               MakePair("Mongolian","mn"),
               MakePair("Montenegrin",""),
               MakePair("Myanmar(Burmese)", "my"),
               MakePair("Nepali","ne"),
               MakePair("Norwegian","no"),
               MakePair("Oriya","or"),
               MakePair("Pashto","ps"),
               MakePair("Persian","fa"),
               MakePair("Polish","pl"),
               MakePair("Portuguese","pt"),
               MakePair("Punjabi","pa"),
               MakePair("Rajasthani",""),
               MakePair("Romanian","ro"),
               MakePair("Russian","ru"),
               MakePair("Sanskrit","sa"),
               MakePair("Santali",""),
               MakePair("Serbian","sr"),
               MakePair("Sindhi","sd"),
               MakePair("Sinhala","si"),
               MakePair("Slovak","sk"),
               MakePair("Slovene","sl"),
               MakePair("Slovenian","sl"),
               MakePair("Spanish","es"),
               MakePair("Spanish (Latin America)","es-419"),
               MakePair("Swedish","sv"),
               MakePair("Tatar","tt"),
               MakePair("Thai","th"),
               MakePair("Turkish","tr"),
               MakePair("Ukrainian","uk"),
               MakePair("Uyghur","ug"),
               MakePair("Urdu","ur"),
               MakePair("Uzbek","uz"),
               MakePair("Vietnamese","vi"),
               MakePair("Welsh","cy"),
               MakePair("Wu",""),
            };
        }

        private static TranslationPair MakePair(string nameCode, string twoLetter)
        {
            return new TranslationPair(nameCode, nameCode, twoLetter);
        }

        internal static string DecodeUnicodeEscapes(string input)
        {
            try
            {
                return UnicodeRegex.Replace(input, match =>
                 char.ConvertFromUtf32(int.Parse(match.Groups[1].Value, System.Globalization.NumberStyles.HexNumber)));
            }
            catch
            {
                return input;
            }
        }

        internal static string FixNewLines(string outputText)
        {
            outputText = outputText.Replace("<br/>", Environment.NewLine);
            outputText = outputText.Replace("<br />", Environment.NewLine);
            outputText = outputText.Replace("<br  />", Environment.NewLine);
            outputText = outputText.Replace(Environment.NewLine + Environment.NewLine, Environment.NewLine);
            return outputText.Trim();
        }

        public void Dispose()
        {
            _httpClient?.Dispose();
        }
    }
}
