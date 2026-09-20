using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Logic;
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

    public async Task<string> Ocr(SKBitmap bitmap, string url, string model, string language, string promptTemplate, CancellationToken cancellationToken)
    {
        try
        {
            // Pad to square to improve OCR accuracy
            using var paddedBitmap = PreprocessImage(bitmap);
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

    public SKBitmap PreprocessImage(SKBitmap source)
    {
        int margin = (int)(Math.Max(source.Width, source.Height) * 0.2);
        int side = Math.Max(source.Width, source.Height) + margin;
        var info = new SKImageInfo(side, side, SKColorType.Rgba8888, SKAlphaType.Opaque);
        var squareBitmap = new SKBitmap(info);

        using (var canvas = new SKCanvas(squareBitmap))
        {
            canvas.Clear(SKColors.Black);
            using (var image = SKImage.FromBitmap(source))
            {
                float left = (side - source.Width) / 2f;
                float top = (side - source.Height) / 2f;
                var destRect = new SKRect(left, top, left + source.Width, top + source.Height);
                var sampling = new SKSamplingOptions(SKFilterMode.Linear, SKMipmapMode.None);
                canvas.DrawImage(image, destRect, sampling, null);
            }

            canvas.Flush();
        }

        return squareBitmap;
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
