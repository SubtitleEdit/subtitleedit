using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Ocr.Engines;

public class OllamaOcr
{
    // glm-ocr and similar are "thinking" models that, left unchecked, emit reasoning, markdown
    // fences and the same line over and over. We disable thinking, use a strict prompt, cap the
    // tokens (a subtitle frame is short) and discourage repetition. Tuned against local glm-ocr.
    private const int MaxTokens = 96;
    private const double RepeatPenalty = 1.1;

    private readonly HttpClient _httpClient;

    public string Error { get; set; }

    public OllamaOcr(int timeoutMinutes = 5)
    {
        Error = string.Empty;
        _httpClient = new HttpClient();
        _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("accept", "application/json");
        _httpClient.Timeout = TimeSpan.FromMinutes(Math.Max(1, timeoutMinutes));
    }

    public async Task<string> Ocr(SKBitmap bitmap, string url, string model, string language, CancellationToken cancellationToken)
    {
        try
        {
            // Pad to square to improve OCR accuracy
            using var paddedBitmap = PreprocessImage(bitmap);
            string base64Image = paddedBitmap.ToBase64String();

            //var pngBytes = paddedBitmap.ToPngArray();
            //System.IO.File.WriteAllBytes("c:\\temp\\ollama-ocr-image.png", pngBytes);

            var modelJson = "\"model\": \"" + model + "\",";
            var optionsJson = "\"options\": { \"temperature\": 0, \"repeat_penalty\": " +
                              RepeatPenalty.ToString(System.Globalization.CultureInfo.InvariantCulture) +
                              ", \"num_predict\": " + MaxTokens + " }, ";
            var prompt = string.Format("You are an OCR engine. The language is {0}. Output only the exact text visible in the image, nothing else. Separate two lines with a single newline.", language);
            // "think": false disables the model's chain-of-thought (glm-ocr is a thinking model),
            // which is the main source of the "way too much text" output.
            var input = "{ " + modelJson + optionsJson + " \"think\": false, \"messages\": [ { \"role\": \"user\", \"content\": \"" + prompt + "\", \"images\": [ \"" + base64Image + "\"] } ], \"stream\": false }";
            var content = new StringContent(input, Encoding.UTF8);
            content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");

            // Trailing slash forces a needless 307 redirect on every request (Ollama's route is /api/chat).
            var requestUrl = string.IsNullOrWhiteSpace(url) ? url : url.TrimEnd('/');

            Se.WriteToolsLog($"Ollama OCR: POST {requestUrl} model={model} image={paddedBitmap.Width}x{paddedBitmap.Height}");
            var stopwatch = Stopwatch.StartNew();
            var result = await _httpClient.PostAsync(requestUrl, content, cancellationToken);
            var bytes = await result.Content.ReadAsByteArrayAsync(cancellationToken);
            stopwatch.Stop();
            var json = Encoding.UTF8.GetString(bytes).Trim();
            if (!result.IsSuccessStatusCode)
            {
                Error = json;
                Se.WriteToolsLog($"Ollama OCR: FAILED status={(int)result.StatusCode} in {stopwatch.ElapsedMilliseconds} ms");
                SeLogger.Error("Error calling Ollama for OCR: Status code=" + result.StatusCode + Environment.NewLine + json);
            }
            else
            {
                Se.WriteToolsLog($"Ollama OCR: OK in {stopwatch.ElapsedMilliseconds} ms ({bytes.Length} bytes)");
            }

            result.EnsureSuccessStatusCode();

            var parser = new SeJsonParser();
            var outputTexts = parser.GetAllTagsByNameAsStrings(json, "content");
            var resultText = string.Join(string.Empty, outputTexts).Trim();

            // sanitize
            resultText = resultText.Trim();
            resultText = resultText.Replace("\\n", Environment.NewLine);
            // Strip any trailing garbage the model still appends after the real text (repeated
            // lines, ``` fences, an echo of the prompt) — the transcription is always first.
            resultText = CleanOcrText(resultText, prompt);
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
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            SeLogger.Error(ex, "Error calling Ollama for OCR");
            return string.Empty;
        }
    }

    /// <summary>
    /// Keeps only the real transcription. glm-ocr (and similar) reliably emit the correct text
    /// first and then append garbage — repeated lines, a ``` markdown fence, or an echo of the
    /// prompt. A subtitle frame is at most a few short lines, so stop at the first line that is a
    /// duplicate, a code fence, or the start of the prompt, and never keep more than 4 lines.
    /// </summary>
    internal static string CleanOcrText(string text, string prompt)
    {
        if (string.IsNullOrEmpty(text))
        {
            return text;
        }

        var promptPrefix = prompt != null && prompt.Length >= 15 ? prompt.Substring(0, 15) : prompt ?? string.Empty;
        var kept = new List<string>();
        var seen = new HashSet<string>(StringComparer.Ordinal);

        foreach (var raw in text.Replace("\r\n", "\n").Replace('\r', '\n').Split('\n'))
        {
            var line = raw.Trim();
            if (line.Length == 0)
            {
                continue;
            }

            if (line.StartsWith("```", StringComparison.Ordinal) ||
                (promptPrefix.Length > 0 && line.StartsWith(promptPrefix, StringComparison.OrdinalIgnoreCase)) ||
                seen.Contains(line))
            {
                break;
            }

            seen.Add(line);
            kept.Add(line);
            if (kept.Count >= 4)
            {
                break;
            }
        }

        return kept.Count == 0 ? text : string.Join(Environment.NewLine, kept);
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
}
