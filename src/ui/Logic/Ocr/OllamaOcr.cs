using Nikse.SubtitleEdit.Core.Common;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Logic.Ocr
{
    public class OllamaOcr : IDisposable
    {
        private readonly HttpClient _httpClient;
        private readonly SeJsonParser _parser;

        public string Error { get; set; }

        public OllamaOcr()
        {
            Error = string.Empty;
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/json");
            _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("accept", "application/json");
            _httpClient.Timeout = TimeSpan.FromMinutes(25);
            _parser = new SeJsonParser();
        }

        public async Task<IEnumerable<string>> GetModels()
        {
            try
            {
                using (var response = await _httpClient.GetAsync("http://localhost:11434/api/tags").ConfigureAwait(false))
                {
                    if (!response.IsSuccessStatusCode)
                    {
                        return Enumerable.Empty<string>();
                    }

                    var json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                    return _parser.GetAllTagsByNameAsStrings(json, "model");
                }
            }
            catch (Exception ex)
            {
                SeLogger.Error(ex, "Error getting models from Ollama");
                return Enumerable.Empty<string>();
            }
        }

        public async Task<string> Ocr(Bitmap bitmap, string model, string language, CancellationToken cancellationToken)
        {
            try
            {
                var modelJson = "\"model\": \"" + model + "\",";

                var prompt = $"Get the text (use '\\n' for new line) from this image in {language}. Return only the text - no commnts or notes. For new line, use '\\n'.";
                var input = "{ " + modelJson + "  \"messages\": [ { \"role\": \"user\", \"content\": \"" + prompt + "\", \"images\": [ \"" + Utilities.PngToBase64String(bitmap) + "\"] } ] }";
                var content = new StringContent(input, Encoding.UTF8);
                content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
                var result = await _httpClient.PostAsync("http://localhost:11434/api/chat", content, cancellationToken).ConfigureAwait(false);
                var bytes = await result.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
                content.Dispose();
                var json = Encoding.UTF8.GetString(bytes).Trim();
                if (!result.IsSuccessStatusCode)
                {
                    Error = json;
                    SeLogger.Error("Error calling Ollama for OCR: Status code=" + result.StatusCode + Environment.NewLine + json);
                }

                result.EnsureSuccessStatusCode();

                var outputTexts = _parser.GetAllTagsByNameAsStrings(json, "content");
                var resultText = string.Join(string.Empty, outputTexts).Trim();

                // sanitize
                resultText = resultText.Trim();
                resultText = resultText.Replace("\\n", Environment.NewLine);
                resultText = resultText.Replace(" ,", ",");
                resultText = resultText.Replace(" .", ".");
                resultText = resultText.Replace(" !", "!");
                resultText = resultText.Replace(" ?", "?");
                resultText = resultText.Replace("( ", "(");
                resultText = resultText.Replace(" )", ")");
                resultText = resultText.Replace("\\\"", "\"");
                if (resultText.EndsWith("!'"))
                {
                    resultText = resultText.TrimEnd('\'');
                }

                return resultText.Trim();
            }
            catch (Exception ex)
            {
                SeLogger.Error(ex, "Error calling Ollama for OCR");
                return string.Empty;
            }
        }

        public void Dispose() => _httpClient?.Dispose();
    }
}