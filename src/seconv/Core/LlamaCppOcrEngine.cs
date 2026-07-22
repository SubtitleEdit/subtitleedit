using Nikse.SubtitleEdit.Core.Common;
using SkiaSharp;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using Nikse.SubtitleEdit.UiLogic.LlamaCpp;

namespace SeConv.Core;

/// <summary>
/// OCR via llama.cpp with a vision model (GLM-OCR, LightOnOCR, PaddleOCR-VL). With
/// <c>--ocr-url</c> the engine posts to an already-running llama-server; otherwise it finds
/// the llama-server binary (Subtitle Edit's data folder next to seconv, the installed SE data
/// folder, then the system PATH) and an installed OCR model, starts the server on a free
/// loopback port on first use, and lets <see cref="LlamaCppServerManager"/> kill it at process
/// exit. seconv never downloads engines or models (consistent with its Tesseract/Paddle
/// policy) — missing pieces fail fast with instructions instead.
/// </summary>
internal sealed class LlamaCppOcrEngine : IOcrEngine
{
    public string Name => "llama.cpp";

    // Same default prompt as the UI's llama.cpp OCR (SeOcr.LlamaCppOcrPrompt).
    private const string PromptTemplate = "Extract all text exactly as written. The language is {language}. Preserve line breaks.";

    private readonly HttpClient _httpClient;
    private readonly string _language;
    private readonly string _modelName;
    private readonly string? _url;          // non-null = user-managed server via --ocr-url
    private readonly LlamaCppModel? _model; // non-null = local server mode; started on first Recognize
    private readonly bool _quiet;

    private LlamaCppOcrEngine(string? url, LlamaCppModel? model, string modelName, string? language, bool quiet)
    {
        _url = url;
        _model = model;
        _modelName = modelName;
        _language = string.IsNullOrWhiteSpace(language) ? "English" : language;
        _quiet = quiet;
        _httpClient = new HttpClient
        {
            Timeout = TimeSpan.FromMinutes(25),
        };
        _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("accept", "application/json");
    }

    /// <summary>
    /// Validates the setup up front so a broken install fails before any image is OCR'ed:
    /// either a user-managed server URL, or a resolved local llama-server + installed OCR
    /// model. Throws <see cref="InvalidOperationException"/> with an actionable message.
    /// </summary>
    public static LlamaCppOcrEngine Create(ConversionOptions options)
    {
        if (options.Verbose)
        {
            LlamaCppServerManager.LogAction = m => Console.WriteLine("  " + m);
        }

        var url = options.OcrUrl?.Trim();
        if (!string.IsNullOrEmpty(url))
        {
            // User-managed server: accept a bare host:port and complete it to the
            // chat/completions endpoint (same convention as --translate-url).
            var fullUrl = url.Contains("/v1/", StringComparison.OrdinalIgnoreCase)
                ? url
                : url.TrimEnd('/') + "/v1/chat/completions";
            var modelName = string.IsNullOrWhiteSpace(options.OcrModel)
                ? "glmocr"
                : Path.GetFileNameWithoutExtension(options.OcrModel.Trim());
            return new LlamaCppOcrEngine(fullUrl, null, modelName, options.OcrLanguage, options.Quiet);
        }

        LlamaCppLocal.EnsureServerBinary("the OCR window > llama.cpp", "--ocr-url");
        var model = ResolveOcrModel(options.OcrModel);
        return new LlamaCppOcrEngine(null, model, Path.GetFileNameWithoutExtension(model.FileName), options.OcrLanguage, options.Quiet);
    }

    public string Recognize(SKBitmap bitmap)
    {
        if (bitmap is null || bitmap.Width == 0 || bitmap.Height == 0)
        {
            return string.Empty;
        }

        var url = _url;
        if (_model != null)
        {
            // Reuses the already-running server across files in the same run (the server
            // manager is a no-op when the model matches).
            if (!LlamaCppServerManager.IsServerRunning && !_quiet)
            {
                Console.WriteLine($"  Starting llama-server with model {Path.GetFileName(_model.FileName)} (stops at exit)...");
            }

            LlamaCppServerManager.EnsureServerRunningAsync(_model, CancellationToken.None).GetAwaiter().GetResult();
            url = LlamaCppServerManager.ApiUrl;
        }

        using var padded = PadToSquare(bitmap);
        using var image = SKImage.FromBitmap(padded);
        using var data = image.Encode(SKEncodedImageFormat.Png, 90);
        var base64 = Convert.ToBase64String(data.ToArray());

        var prompt = PromptTemplate.Replace("{language}", _language);
        var body = "{ \"model\": \"" + Escape(_modelName) + "\", \"temperature\": 0, \"messages\": [ { \"role\": \"user\", \"content\": [ " +
                   "{ \"type\": \"text\", \"text\": \"" + Escape(prompt) + "\" }, " +
                   "{ \"type\": \"image_url\", \"image_url\": { \"url\": \"data:image/png;base64," + base64 + "\" } } " +
                   "] } ] }";

        using var content = new StringContent(body, Encoding.UTF8);
        content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");

        var resp = _httpClient.PostAsync(url, content).GetAwaiter().GetResult();
        var bodyBytes = resp.Content.ReadAsByteArrayAsync().GetAwaiter().GetResult();
        var json = Encoding.UTF8.GetString(bodyBytes).Trim();
        if (!resp.IsSuccessStatusCode)
        {
            throw new InvalidOperationException($"llama.cpp returned {(int)resp.StatusCode}: {json}");
        }

        var parser = new SeJsonParser();
        var contents = parser.GetAllTagsByNameAsStrings(json, "content");
        var text = string.Join(string.Empty, contents).Trim();
        text = text.Replace("\\n", Environment.NewLine).Replace("\\\"", "\"");
        return text.Trim();
    }

    /// <summary>
    /// Resolves <c>--ocr-model</c> to an installed model: a full <c>.gguf</c> path (needs its
    /// mmproj vision-projector sidecar next to it), a curated OCR model by file/display name,
    /// or - when omitted - the first installed curated OCR model.
    /// </summary>
    internal static LlamaCppModel ResolveOcrModel(string? requestedModel)
    {
        var curatedNames = string.Join(", ", LlamaCppServerManager.OcrModels.Select(m => m.FileName));

        if (!string.IsNullOrWhiteSpace(requestedModel))
        {
            var name = requestedModel.Trim();

            // Full path to a .gguf: use it directly, but require the mmproj sidecar - a vision
            // model served without its projector can't see the image at all.
            if (Path.IsPathRooted(name) || name.Contains(Path.DirectorySeparatorChar) || name.Contains(Path.AltDirectorySeparatorChar))
            {
                if (!File.Exists(name))
                {
                    throw new InvalidOperationException($"OCR model file not found: {name}");
                }

                var fullPath = Path.GetFullPath(name);
                var mmproj = FindMmprojSidecar(fullPath);
                if (mmproj == null)
                {
                    var fileName = Path.GetFileName(fullPath);
                    var stem = Path.GetFileNameWithoutExtension(fullPath);
                    throw new InvalidOperationException(
                        $"No vision projector found next to {fullPath}. llama.cpp OCR models need their mmproj sidecar; " +
                        $"expected 'mmproj-{fileName}' or '{stem}-mmproj.gguf' in the same folder.");
                }

                return new LlamaCppModel(Path.GetFileName(fullPath), fullPath, string.Empty, Url: string.Empty,
                    MmprojFileName: mmproj);
            }

            // Name: match the curated OCR models (with or without .gguf).
            var model = LlamaCppServerManager.OcrModels.FirstOrDefault(m => m.FileName.Equals(name, StringComparison.OrdinalIgnoreCase))
                        ?? LlamaCppServerManager.OcrModels.FirstOrDefault(m => m.FileName.Equals(name + ".gguf", StringComparison.OrdinalIgnoreCase))
                        ?? LlamaCppServerManager.OcrModels.FirstOrDefault(m => m.DisplayName.Equals(name, StringComparison.OrdinalIgnoreCase));
            if (model == null || !LlamaCppServerManager.IsModelInstalled(model))
            {
                throw new InvalidOperationException(
                    $"OCR model '{name}' not found in {LlamaCppServerManager.GetAndCreateModelsFolder()}. " +
                    "Download one in Subtitle Edit (OCR window > llama.cpp) or pass a full path via --ocr-model. " +
                    $"Curated models: {curatedNames}.");
            }

            return model;
        }

        // No model given: pick the first installed curated OCR model.
        var installed = LlamaCppServerManager.OcrModels.FirstOrDefault(LlamaCppServerManager.IsModelInstalled);
        if (installed == null)
        {
            throw new InvalidOperationException(
                $"No llama.cpp OCR model found in {LlamaCppServerManager.GetAndCreateModelsFolder()}. " +
                "Download one in Subtitle Edit (OCR window > llama.cpp) or pass --ocr-model:<path.gguf>. " +
                $"Curated models: {curatedNames}.");
        }

        return installed;
    }

    private static string? FindMmprojSidecar(string modelPath)
    {
        // Both curated sidecar conventions: "mmproj-<file>" (GLM-OCR, LightOnOCR) and
        // "<stem>-mmproj.gguf" (PaddleOCR-VL).
        var dir = Path.GetDirectoryName(modelPath)!;
        var candidates = new[]
        {
            Path.Combine(dir, "mmproj-" + Path.GetFileName(modelPath)),
            Path.Combine(dir, Path.GetFileNameWithoutExtension(modelPath) + "-mmproj.gguf"),
        };
        return candidates.FirstOrDefault(File.Exists);
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
