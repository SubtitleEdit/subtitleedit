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
    // OCR of a single subtitle frame is short, so cap generation and discourage repetition.
    // Vision models otherwise tend to loop ("- Henry, we're here." x100), which both produces
    // garbage and makes each line take a very long time (#ollama-ocr-looping).
    private const int MaxTokens = 500;
    private const double RepeatPenalty = 1.3;

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
            var prompt = string.Format("Act as a precise OCR engine. Transcribe every line of text from this image exactly as it appears. The language is {0}. Maintain the vertical order. Use a single '\\n' to separate each line. Do not skip any text. Output only the transcribed text", language);
            var input = "{ " + modelJson + optionsJson + "  \"messages\": [ { \"role\": \"user\", \"content\": \"" + prompt + "\", \"images\": [ \"" + base64Image + "\"] } ], \"stream\": false }";
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
            // Defensively collapse runaway repetition the model may still emit within the token cap.
            resultText = CollapseRepetition(resultText);
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
    /// Collapses runaway repetition from a looping vision model. OCR of a single subtitle frame is
    /// short, so when the output is the same short block of lines repeated many times (e.g.
    /// "- Henry, we're here." x100) it is a generation loop, not real text — reduce it to one block.
    /// Conservative: only triggers when the whole output is a cycle of 1-4 lines repeated 3+ times.
    /// </summary>
    internal static string CollapseRepetition(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return text;
        }

        var lines = text.Replace("\r\n", "\n").Replace('\r', '\n').Split('\n')
            .Select(l => l.Trim())
            .Where(l => l.Length > 0)
            .ToList();

        if (lines.Count < 6)
        {
            return text; // too short to be a runaway loop
        }

        // A loop starts by repeating its leading block. The cycle length is the distance to the
        // first reoccurrence of the first line; require the leading line to recur several times so
        // we don't touch genuine (short, varied) OCR output.
        var first = lines[0];
        if (lines.Count(l => l == first) < 3)
        {
            return text;
        }

        var cycle = lines.FindIndex(1, l => l == first);
        if (cycle < 1 || cycle > 4)
        {
            return text;
        }

        var block = lines.Take(cycle).ToList();
        var consecutiveRepeats = 0;
        for (var i = 0; i + cycle <= lines.Count; i += cycle)
        {
            if (Enumerable.Range(0, cycle).All(k => lines[i + k] == block[k]))
            {
                consecutiveRepeats++;
            }
            else
            {
                break;
            }
        }

        if (consecutiveRepeats < 3)
        {
            return text;
        }

        return string.Join(Environment.NewLine, block);
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
