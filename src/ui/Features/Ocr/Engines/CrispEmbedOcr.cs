using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using SkiaSharp;
using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Ocr.Engines;

/// <summary>
/// Runs OCR through a local crispembed-server instance so the GGUF model is loaded once per
/// OCR session instead of once per subtitle image. The server only accepts image file paths,
/// so each bitmap is written to a temp PNG (flattened onto an opaque background - subtitle
/// bitmaps are typically white text on transparency) and posted to /ocr/model.
/// </summary>
public class CrispEmbedOcr : IDisposable
{
    private readonly HttpClient _httpClient;
    private Process? _serverProcess;
    private int _port;

    public string Error { get; set; }

    public CrispEmbedOcr(int timeoutMinutes = 5)
    {
        Error = string.Empty;
        _httpClient = new HttpClient();
        _httpClient.Timeout = TimeSpan.FromMinutes(Math.Max(1, timeoutMinutes));
    }

    public async Task<bool> StartServerAsync(string serverExecutable, string modelFileName, CancellationToken cancellationToken)
    {
        if (!File.Exists(serverExecutable))
        {
            Error = $"CrispEmbed server not found: {serverExecutable}";
            return false;
        }

        if (!File.Exists(modelFileName))
        {
            Error = $"CrispEmbed model not found: {modelFileName}";
            return false;
        }

        _port = GetFreePort();
        var arguments = $"--ocr \"{modelFileName}\" --host 127.0.0.1 --port {_port}";
        Se.WriteToolsLog($"{serverExecutable} {arguments}");

        try
        {
            _serverProcess = new Process
            {
                StartInfo = new ProcessStartInfo(serverExecutable, arguments)
                {
                    WindowStyle = ProcessWindowStyle.Hidden,
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    WorkingDirectory = Path.GetDirectoryName(serverExecutable),
                },
            };

            _serverProcess.OutputDataReceived += (_, e) => LogServerOutput(e.Data);
            _serverProcess.ErrorDataReceived += (_, e) => LogServerOutput(e.Data);
            _serverProcess.Start();
            _serverProcess.BeginOutputReadLine();
            _serverProcess.BeginErrorReadLine();
        }
        catch (Exception ex)
        {
            // e.g. Win32Exception when the binary is missing the executable bit, quarantined,
            // or the wrong architecture - surface it instead of dying as an unobserved task.
            Error = ex.Message;
            SeLogger.Error(ex, "Failed to start CrispEmbed server");
            return false;
        }

        // Model load happens before the server starts listening, so poll /health until it
        // answers. Large models can take a while to load from a cold disk cache.
        var healthUrl = $"http://127.0.0.1:{_port}/health";
        var timeout = DateTime.UtcNow.AddMinutes(5);
        while (DateTime.UtcNow < timeout)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (_serverProcess.HasExited)
            {
                Error = $"CrispEmbed server exited with code {_serverProcess.ExitCode}";
                SeLogger.Error("CrispEmbed server exited during startup: " + Error);
                return false;
            }

            try
            {
                using var response = await _httpClient.GetAsync(healthUrl, cancellationToken);
                if (response.IsSuccessStatusCode)
                {
                    return true;
                }
            }
            catch (HttpRequestException)
            {
                // not listening yet
            }
            catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested)
            {
                // request timeout while the model loads - keep polling
            }

            await Task.Delay(250, cancellationToken);
        }

        Error = "CrispEmbed server did not start in time";
        return false;
    }

    public async Task<string> Ocr(SKBitmap bitmap, CancellationToken cancellationToken)
    {
        var tempFileName = Path.Combine(Path.GetTempPath(), $"se-crispembed-{Guid.NewGuid()}.png");
        try
        {
            using (var flattened = FlattenImage(bitmap))
            {
                await File.WriteAllBytesAsync(tempFileName, flattened.ToPngArray(), cancellationToken);
            }

            // The server reads the image from disk, so only the path goes over the wire. The
            // server's request parser does not un-escape JSON strings, so send forward slashes
            // (fine with Windows file APIs) to keep the serialized path escape-free.
            var input = JsonSerializer.Serialize(new { image = tempFileName.Replace('\\', '/') });
            var content = new StringContent(input, Encoding.UTF8);
            content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
            var result = await _httpClient.PostAsync($"http://127.0.0.1:{_port}/ocr/model", content, cancellationToken);
            var bytes = await result.Content.ReadAsByteArrayAsync(cancellationToken);
            var json = Encoding.UTF8.GetString(bytes).Trim();
            if (!result.IsSuccessStatusCode)
            {
                Error = json;
                SeLogger.Error("Error calling CrispEmbed for OCR: Status code=" + result.StatusCode + Environment.NewLine + json);
                return string.Empty;
            }

            // The recognized text is returned under the legacy "latex" key for all engines
            // (math and document alike); accept "text" too in case upstream renames it.
            using var document = JsonDocument.Parse(json);
            var resultText = document.RootElement.TryGetProperty("latex", out var latexElement)
                ? latexElement.GetString() ?? string.Empty
                : document.RootElement.TryGetProperty("text", out var textElement)
                    ? textElement.GetString() ?? string.Empty
                    : string.Empty;

            return resultText.Replace("\r\n", "\n").Replace("\n", Environment.NewLine).Trim();
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (OperationCanceledException ex)
        {
            Error = "Request timed out";
            SeLogger.Error(ex, "CrispEmbed OCR request timed out");
            return string.Empty;
        }
        catch (Exception ex)
        {
            SeLogger.Error(ex, "Error calling CrispEmbed for OCR");
            return string.Empty;
        }
        finally
        {
            try
            {
                File.Delete(tempFileName);
            }
            catch
            {
                // ignore
            }
        }
    }

    /// <summary>
    /// Draws the bitmap onto an opaque black background with a small margin. Subtitle bitmaps
    /// are usually white/coloured glyphs on full transparency, which would otherwise be
    /// flattened to an arbitrary background by the server's image loader.
    /// </summary>
    private static SKBitmap FlattenImage(SKBitmap source)
    {
        var margin = Math.Max(8, (int)(Math.Max(source.Width, source.Height) * 0.05));
        var info = new SKImageInfo(source.Width + margin * 2, source.Height + margin * 2, SKColorType.Rgba8888, SKAlphaType.Opaque);
        var flattened = new SKBitmap(info);
        using var canvas = new SKCanvas(flattened);
        canvas.Clear(SKColors.Black);
        using (var image = SKImage.FromBitmap(source))
        {
            var destRect = new SKRect(margin, margin, margin + source.Width, margin + source.Height);
            var sampling = new SKSamplingOptions(SKFilterMode.Linear, SKMipmapMode.None);
            canvas.DrawImage(image, destRect, sampling, null);
        }

        canvas.Flush();
        return flattened;
    }

    private static void LogServerOutput(string? line)
    {
        if (!string.IsNullOrWhiteSpace(line))
        {
            Se.WriteToolsLog("crispembed-server: " + line);
        }
    }

    private static int GetFreePort()
    {
        using var listener = new TcpListener(System.Net.IPAddress.Loopback, 0);
        listener.Start();
        var port = ((System.Net.IPEndPoint)listener.LocalEndpoint).Port;
        listener.Stop();
        return port;
    }

    public void Dispose()
    {
        try
        {
            if (_serverProcess is { HasExited: false })
            {
                _serverProcess.Kill(entireProcessTree: true);
            }

            _serverProcess?.Dispose();
        }
        catch
        {
            // ignore
        }

        _httpClient.Dispose();
    }
}
