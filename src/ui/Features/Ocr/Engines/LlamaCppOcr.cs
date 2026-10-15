using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.UiLogic.AutoTranslate;
using Nikse.SubtitleEdit.UiLogic.LlamaCpp;
using Nikse.SubtitleEdit.UiLogic.Ocr;
using SkiaSharp;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Ocr.Engines;

public class LlamaCppOcr : IDisposable
{
    private readonly HttpClient _httpClient;

    public string Error { get; set; }

    public LlamaCppOcr(int timeoutMinutes = 5)
    {
        Error = string.Empty;
        _httpClient = new HttpClient();
        _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/json");
        _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("accept", "application/json");
        _httpClient.Timeout = TimeSpan.FromMinutes(Math.Max(1, timeoutMinutes));
    }

    /// <summary>
    /// Completes a remote llama-server URL typed as a bare origin ("http://host:8080") or
    /// "host:8080" to the chat/completions endpoint, like auto-translate and seconv's --ocr-url.
    /// </summary>
    public static string CompleteRemoteUrl(string? url)
    {
        return AutoTranslateUrl.Complete(url, LlamaCppTranslate.DefaultUrl);
    }

    public async Task<string> Ocr(SKBitmap bitmap, string url, string model, string language, string promptTemplate, CancellationToken cancellationToken)
    {
        try
        {
            // Square for GLM-OCR and unknown models, a thin border for models that read wide strips better
            using var paddedBitmap = AiOcrImagePadding.Pad(bitmap, LlamaCppServerManager.GetOcrPadding(model));
            string base64Image = paddedBitmap.ToBase64String();

            //var pngBytes = paddedBitmap.ToPngArray();
            //System.IO.File.WriteAllBytes("c:\\temp\\ollama-ocr-image.png", pngBytes);

            var template = string.IsNullOrWhiteSpace(promptTemplate)
                ? SeOcrDefaults.LlamaCppOcrPrompt
                : promptTemplate;
            var prompt = template
                .Replace("{language}", language)
                .Replace("\"", "\\\"")
                .Replace("\r", " ")
                .Replace("\n", " ");
            var modelName = string.IsNullOrEmpty(model) ? "glmocr" : model;

            var input =
                "{ \"model\": \"" + modelName + "\", \"temperature\": 0, \"messages\": [ { \"role\": \"user\", \"content\": [ " +
                "{ \"type\": \"text\", \"text\": \"" + prompt + "\" }, " +
                "{ \"type\": \"image_url\", \"image_url\": { \"url\": \"data:image/png;base64," + base64Image + "\" } } " +
                "] } ] }";

            var content = new StringContent(input, Encoding.UTF8);
            content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
            var result = await _httpClient.PostAsync(url, content, cancellationToken);
            var bytes = await result.Content.ReadAsByteArrayAsync(cancellationToken);
            var json = Encoding.UTF8.GetString(bytes).Trim();
            if (!result.IsSuccessStatusCode)
            {
                Error = json;
                SeLogger.Error("Error calling llama.cpp for OCR: Status code=" + result.StatusCode + Environment.NewLine + json);
            }

            result.EnsureSuccessStatusCode();

            var parser = new SeJsonParser();
            var outputTexts = parser.GetAllTagsByNameAsStrings(json, "content");
            var resultText = string.Join(string.Empty, outputTexts).Trim();

            // sanitize
            resultText = resultText.Trim();
            resultText = resultText.Replace("\\n", Environment.NewLine);
            resultText = OcrHelper.TrimAiOcrLines(resultText);
            resultText = OcrHelper.FixAiOcrPunctuationSpaces(resultText, language);

            return resultText.Trim();
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (OperationCanceledException ex)
        {
            Error = "Request timed out";
            SeLogger.Error(ex, "llama.cpp OCR request timed out");
            return string.Empty;
        }
        catch (Exception ex)
        {
            // Record it so the caller can tell a real failure apart from a textless image: the
            // OCR loops fail fast on a non-empty Error and otherwise grind through the whole job
            // only to report "no text found". A non-success HTTP status has already stored the
            // (more informative) response body in Error.
            if (string.IsNullOrEmpty(Error))
            {
                Error = ex.Message;
            }

            SeLogger.Error(ex, "Error calling llama.cpp for OCR");
            return string.Empty;
        }
    }

    /// <summary>
    /// A new engine (and so a new HttpClient, handler and connection pool) is built per OCR run
    /// from several call sites; without disposal those pile up and eventually exhaust sockets.
    /// </summary>
    public void Dispose()
    {
        _httpClient.Dispose();
        GC.SuppressFinalize(this);
    }
}
