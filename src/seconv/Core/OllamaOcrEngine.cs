using Nikse.SubtitleEdit.Core.Common;
using SkiaSharp;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace SeConv.Core;

/// <summary>
/// OCR via a local Ollama server with a vision-capable model (e.g. <c>llama3.2-vision</c>,
/// <c>qwen2.5vl</c>, <c>gemma3</c>). Requires Ollama running locally; configurable via
/// <c>--ollama-url</c> (default <c>http://localhost:11434/api/chat</c>) and
/// <c>--ollama-model</c> (default <c>llama3.2-vision</c>).
/// </summary>
internal sealed class OllamaOcrEngine : IOcrEngine
{
    public string Name => "ollama";

    private readonly HttpClient _httpClient;
    private readonly string _url;
    private readonly string _model;
    private readonly string _language;

    public OllamaOcrEngine(string url, string model, string language)
    {
        _url = string.IsNullOrWhiteSpace(url) ? "http://localhost:11434/api/chat" : url;
        _model = string.IsNullOrWhiteSpace(model) ? "llama3.2-vision" : model;
        _language = string.IsNullOrWhiteSpace(language) ? "English" : language;

        _httpClient = new HttpClient
        {
            Timeout = TimeSpan.FromMinutes(25),
        };
        _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("accept", "application/json");
    }

    public string Recognize(SKBitmap bitmap)
    {
        if (bitmap is null || bitmap.Width == 0 || bitmap.Height == 0)
        {
            return string.Empty;
        }

        using var padded = PadToSquare(bitmap);
        using var image = SKImage.FromBitmap(padded);
        using var data = image.Encode(SKEncodedImageFormat.Png, 90);
        var pngBytes = data.ToArray();
        var base64 = Convert.ToBase64String(pngBytes);

        var prompt = $"Act as a precise OCR engine. Transcribe every line of text from this image exactly as it appears. The language is {_language}. Maintain the vertical order. Use a single '\\n' to separate each line. Do not skip any text. Output only the transcribed text";
        var body = "{ \"model\": \"" + Escape(_model) + "\", " +
                   "\"messages\": [ { \"role\": \"user\", \"content\": \"" + Escape(prompt) + "\", " +
                   "\"images\": [ \"" + base64 + "\" ] } ], " +
                   "\"stream\": false }";

        using var content = new StringContent(body, Encoding.UTF8);
        content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");

        var resp = _httpClient.PostAsync(_url, content).GetAwaiter().GetResult();
        var bodyBytes = resp.Content.ReadAsByteArrayAsync().GetAwaiter().GetResult();
        var json = Encoding.UTF8.GetString(bodyBytes).Trim();
        if (!resp.IsSuccessStatusCode)
        {
            throw new InvalidOperationException($"Ollama returned {(int)resp.StatusCode}: {json}");
        }

        var parser = new SeJsonParser();
        var contents = parser.GetAllTagsByNameAsStrings(json, "content");
        var text = string.Join(string.Empty, contents).Trim();
        text = text.Replace("\\n", Environment.NewLine).Replace("\\\"", "\"");
        return text.Trim();
    }

    private static string Escape(string s) =>
        s.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\n", "\\n").Replace("\r", "\\r");

    private static SKBitmap PadToSquare(SKBitmap source)
    {
        var margin = (int)(Math.Max(source.Width, source.Height) * 0.2);
        var side = Math.Max(source.Width, source.Height) + margin;
        var info = new SKImageInfo(side, side, SKColorType.Rgba8888, SKAlphaType.Opaque);
        var square = new SKBitmap(info);
        using var canvas = new SKCanvas(square);
        canvas.Clear(SKColors.Black);
        var left = (side - source.Width) / 2f;
        var top = (side - source.Height) / 2f;
        canvas.DrawBitmap(source, new SKRect(left, top, left + source.Width, top + source.Height));
        canvas.Flush();
        return square;
    }

    public void Dispose() => _httpClient.Dispose();
}
