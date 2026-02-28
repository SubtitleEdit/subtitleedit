using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Logic;
using SkiaSharp;

namespace Nikse.SubtitleEdit.Features.Ocr;

public class MistralOcr
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    public string Error { get; set; }

    public MistralOcr(string apiKey)
    {
        Error = string.Empty;
        _apiKey = apiKey ?? throw new ArgumentNullException(nameof(apiKey));
        _httpClient = new HttpClient();
        _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/json");
        _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("accept", "application/json");
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
        _httpClient.Timeout = TimeSpan.FromMinutes(25);
    }

    public async Task<string> Ocr(SKBitmap bitmap, string language, CancellationToken cancellationToken)
    {
        try
        {
            // Convert bitmap to base64 with data URI prefix
            var base64Image = "data:image/png;base64," + bitmap.ToBase64String();

            var requestBody = new
            {
                model = "mistral-ocr-latest",
                document = new
                {
                    type = "image_url",
                    image_url = base64Image  // Direct string, not nested object
                },
                include_image_base64 = true
            };

            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8);
            content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");

            var result = await _httpClient.PostAsync("https://api.mistral.ai/v1/ocr", content, cancellationToken);
            var responseBytes = await result.Content.ReadAsByteArrayAsync();
            var responseJson = Encoding.UTF8.GetString(responseBytes).Trim();

            if (!result.IsSuccessStatusCode)
            {
                Error = responseJson;
                SeLogger.Error("Error calling Mistral AI OCR: Status code=" + result.StatusCode + Environment.NewLine + responseJson);
            }

            result.EnsureSuccessStatusCode();

            // Parse JSON response using System.Text.Json
            using var document = JsonDocument.Parse(responseJson);
            var root = document.RootElement;

            // Extract text from the OCR response
            // The OCR endpoint returns pages with markdown content
            if (!root.TryGetProperty("pages", out var pages) || pages.GetArrayLength() == 0)
            {
                Error = "No pages found in OCR response";
                return string.Empty;
            }

            var resultText = new StringBuilder();
            foreach (var page in pages.EnumerateArray())
            {
                if (page.TryGetProperty("markdown", out var markdownElement))
                {
                    var markdown = markdownElement.GetString();
                    if (!string.IsNullOrEmpty(markdown))
                    {
                        resultText.AppendLine(markdown);
                    }
                }
            }

            var finalText = resultText.ToString().Trim();

            // Sanitize the result (keeping your existing logic)
            finalText = finalText.Replace("\\n", Environment.NewLine);
            finalText = finalText.Replace(" ,", ",");
            finalText = finalText.Replace(" .", ".");
            finalText = finalText.Replace(" !", "!");
            finalText = finalText.Replace(" ?", "?");
            finalText = finalText.Replace("( ", "(");
            finalText = finalText.Replace(" )", ")");
            finalText = finalText.Replace("\\\"", "\"");

            if (finalText.EndsWith("!'"))
            {
                finalText = finalText.TrimEnd('\'');
            }

            return finalText.Trim();
        }
        catch (Exception ex)
        {
            SeLogger.Error(ex, "Error calling Mistral AI OCR");
            Error = ex.Message;
            return string.Empty;
        }
    }

    public void Dispose()
    {
        _httpClient?.Dispose();
    }
}