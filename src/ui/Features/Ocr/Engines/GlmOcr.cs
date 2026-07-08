using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using SkiaSharp;
using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Ocr.Engines;

/// <summary>
/// OCR via GLM vision models (Z.ai / bigmodel.cn) through the OpenAI-compatible
/// chat completions API - e.g. glm-4.5v or glm-4.6v with an image + prompt.
/// </summary>
public class GlmOcr
{
    public const string DefaultUrl = "https://api.z.ai/api/paas/v4/chat/completions";
    public const string DefaultModel = "glm-4.5v";

    private readonly HttpClient _httpClient;

    public string Error { get; set; }

    public GlmOcr(string apiKey, int timeoutMinutes = 5)
    {
        Error = string.Empty;
        _httpClient = new HttpClient();
        _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("accept", "application/json");
        if (!string.IsNullOrWhiteSpace(apiKey))
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey.Trim());
        }

        _httpClient.Timeout = TimeSpan.FromMinutes(Math.Max(1, timeoutMinutes));
    }

    public async Task<string> Ocr(SKBitmap bitmap, string url, string model, string language, CancellationToken cancellationToken)
    {
        return await Ocr(bitmap.ToBase64String(), "image/png", url, model, language, cancellationToken);
    }

    /// <summary>OCR an image file as-is - no decode/re-encode roundtrip.</summary>
    public async Task<string> Ocr(string imageFileName, string url, string model, string language, CancellationToken cancellationToken)
    {
        var bytes = await File.ReadAllBytesAsync(imageFileName, cancellationToken);
        var mimeType = imageFileName.EndsWith(".png", StringComparison.OrdinalIgnoreCase) ? "image/png" : "image/jpeg";
        return await Ocr(Convert.ToBase64String(bytes), mimeType, url, model, language, cancellationToken);
    }

    private async Task<string> Ocr(string base64Image, string mimeType, string url, string model, string language, CancellationToken cancellationToken)
    {
        try
        {
            var prompt = string.Format("You are an OCR engine. The language is {0}. Output only the exact text visible in the image, nothing else. If the image contains no text, output nothing. Separate two lines with a single newline.", language);
            var modelName = string.IsNullOrWhiteSpace(model) ? DefaultModel : model.Trim();

            // "thinking" disabled: GLM vision models are thinking models by default, and
            // reasoning output would drown the short transcription we want.
            var input =
                "{ \"model\": \"" + EscapeJson(modelName) + "\", \"temperature\": 0, " +
                "\"thinking\": { \"type\": \"disabled\" }, " +
                "\"messages\": [ { \"role\": \"user\", \"content\": [ " +
                "{ \"type\": \"text\", \"text\": \"" + EscapeJson(prompt) + "\" }, " +
                "{ \"type\": \"image_url\", \"image_url\": { \"url\": \"data:" + mimeType + ";base64," + base64Image + "\" } } " +
                "] } ] }";

            var content = new StringContent(input, Encoding.UTF8);
            content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");

            var requestUrl = string.IsNullOrWhiteSpace(url) ? DefaultUrl : url.Trim();
            var result = await _httpClient.PostAsync(requestUrl, content, cancellationToken);
            var bytes = await result.Content.ReadAsByteArrayAsync(cancellationToken);
            var json = Encoding.UTF8.GetString(bytes).Trim();
            if (!result.IsSuccessStatusCode)
            {
                Error = json;
                SeLogger.Error("Error calling GLM for OCR: Status code=" + result.StatusCode + Environment.NewLine + json);
            }

            result.EnsureSuccessStatusCode();

            var parser = new SeJsonParser();
            var outputTexts = parser.GetAllTagsByNameAsStrings(json, "content");
            var resultText = string.Join(string.Empty, outputTexts).Trim();

            resultText = resultText.Replace("\\n", Environment.NewLine);
            resultText = OllamaOcr.CleanOcrText(resultText, prompt);
            resultText = resultText.Replace("\\\"", "\"");

            return resultText.Trim();
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (OperationCanceledException ex)
        {
            Error = "Request timed out";
            SeLogger.Error(ex, "GLM OCR request timed out");
            return string.Empty;
        }
        catch (Exception ex)
        {
            Error = ex.Message;
            SeLogger.Error(ex, "Error calling GLM for OCR");
            return string.Empty;
        }
    }

    private static string EscapeJson(string text)
    {
        return text
            .Replace("\\", "\\\\")
            .Replace("\"", "\\\"")
            .Replace("\r", " ")
            .Replace("\n", " ");
    }
}
