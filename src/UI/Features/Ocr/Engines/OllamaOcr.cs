using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Logic;
using SkiaSharp;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Ocr.Engines;

public class OllamaOcr
{
    private readonly HttpClient _httpClient;

    public string Error { get; set; }

    public OllamaOcr()
    {
        Error = string.Empty;
        _httpClient = new HttpClient();
        _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/json");
        _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("accept", "application/json");
        _httpClient.Timeout = TimeSpan.FromMinutes(25);
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
            var optionsJson = string.Empty; // "\"options\": { \"temperature\": 0, \"repeat_penalty\": 1.0 },";
            var prompt = string.Format("Act as a precise OCR engine. Transcribe every line of text from this image exactly as it appears. The language is {0}. Maintain the vertical order. Use a single '\\n' to separate each line. Do not skip any text. Output only the transcribed text", language);
            var input = "{ " + modelJson + optionsJson + "  \"messages\": [ { \"role\": \"user\", \"content\": \"" + prompt + "\", \"images\": [ \"" + base64Image + "\"] } ] }";
            var content = new StringContent(input, Encoding.UTF8);
            content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
            var result = await _httpClient.PostAsync(url, content, cancellationToken);
            var bytes = await result.Content.ReadAsByteArrayAsync(cancellationToken);
            var json = Encoding.UTF8.GetString(bytes).Trim();
            if (!result.IsSuccessStatusCode)
            {
                Error = json;
                SeLogger.Error("Error calling Ollama for OCR: Status code=" + result.StatusCode + Environment.NewLine + json);
            }

            result.EnsureSuccessStatusCode();

            var parser = new SeJsonParser();
            var outputTexts = parser.GetAllTagsByNameAsStrings(json, "content");
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
