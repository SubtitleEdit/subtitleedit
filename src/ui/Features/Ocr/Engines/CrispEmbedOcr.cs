using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using SkiaSharp;
using System;
using System.Collections.Generic;
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
    private readonly int _timeoutMinutes;
    private Process? _serverProcess;
    private int _port;

    private string _cliExecutable = string.Empty;
    private string _cliRecognizerModel = string.Empty;
    private string _cliDetectorModel = string.Empty;
    private bool _isCliPipeline;

    private const int MaxCapturedOutputLines = 12;
    private readonly Queue<string> _lastOutputLines = new();
    private readonly Lock _outputLock = new();

    public string Error { get; set; }

    public CrispEmbedOcr(int timeoutMinutes = 5)
    {
        Error = string.Empty;
        _timeoutMinutes = Math.Max(1, timeoutMinutes);
        _httpClient = new HttpClient();
        _httpClient.Timeout = TimeSpan.FromMinutes(_timeoutMinutes);
    }

    /// <summary>
    /// Prepares the PP-OCRv6 detector+recognizer pipeline, which runs as one crispembed CLI
    /// invocation per image instead of through crispembed-server. The server only enables its
    /// OCR orchestrator when it is also given a primary embedding model via -m, and dies on
    /// startup if that model is not a real embedding GGUF - so there is no server-side route to
    /// the pipeline without downloading an unrelated model. The CLI reloads the pair per image,
    /// which costs little: the two GGUFs are a few MB and a full invocation is well under a
    /// second, i.e. no slower than one already-loaded VLM request.
    /// </summary>
    public bool StartCliPipeline(string cliExecutable, string recognizerFileName, string detectorFileName)
    {
        if (!File.Exists(cliExecutable))
        {
            Error = $"CrispEmbed CLI not found: {cliExecutable}";
            return false;
        }

        if (!File.Exists(recognizerFileName))
        {
            Error = $"CrispEmbed model not found: {recognizerFileName}";
            return false;
        }

        if (!File.Exists(detectorFileName))
        {
            Error = $"CrispEmbed text detector not found: {detectorFileName}";
            return false;
        }

        _cliExecutable = cliExecutable;
        _cliRecognizerModel = recognizerFileName;
        _cliDetectorModel = detectorFileName;
        _isCliPipeline = true;
        return true;
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

            // DeepSeek-OCR-2's dynamic crop mode (default since CrispEmbed v0.17.5) tiles the
            // image into multiple views, which helps full-page documents but is 3-6x slower on
            // subtitle-sized bitmaps with byte-identical output (measured 2026-08-05, EN/ZH/JA/RU
            // plus a 1920px two-liner). Only the DeepSeek engine reads this variable.
            _serverProcess.StartInfo.Environment["DS2_CROP_MODE"] = "0";

            _serverProcess.OutputDataReceived += (_, e) => CaptureServerOutput(e.Data);
            _serverProcess.ErrorDataReceived += (_, e) => CaptureServerOutput(e.Data);
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
                Error = await BuildStartupFailureMessage(_serverProcess);
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

            if (_isCliPipeline)
            {
                return await OcrViaCliPipeline(tempFileName, cancellationToken);
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

            return NormalizeServerText(resultText);
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
    /// Normalizes a VLM OCR result to platform newlines with per-line trimming. The document
    /// models reproduce a centered second line's indentation as literal leading spaces, which
    /// are never meaningful in a subtitle.
    /// </summary>
    internal static string NormalizeServerText(string text)
    {
        var lines = text.Replace("\r\n", "\n").Split('\n');
        for (var i = 0; i < lines.Length; i++)
        {
            lines[i] = lines[i].Trim();
        }

        return string.Join(Environment.NewLine, lines).Trim();
    }

    /// <summary>
    /// Runs one crispembed CLI invocation for the PP-OCRv6 pipeline and returns the recognized
    /// text. --json keeps the result on a single line ("full_text" with "\n" between detected
    /// regions), which is unambiguous next to the progress lines the CLI writes to stdout.
    /// </summary>
    private async Task<string> OcrViaCliPipeline(string imageFileName, CancellationToken cancellationToken)
    {
        var arguments = $"--ocr-pipeline \"{imageFileName}\" --ocr-engine ppocrv6 " +
                        $"--ocr-det \"{_cliDetectorModel}\" --ocr-rec \"{_cliRecognizerModel}\" -t 4 --json";

        using var process = new Process
        {
            StartInfo = new ProcessStartInfo(_cliExecutable, arguments)
            {
                WindowStyle = ProcessWindowStyle.Hidden,
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                WorkingDirectory = Path.GetDirectoryName(_cliExecutable),
            },
        };

        // v0.17.7's n_threads audit made the PP-OCRv6 detector honor the CLI's -t 1 default where
        // it previously ran at ggml's 4-thread default, costing ~18% wall clock on the scalar
        // (Metal/CPU) path; "-t 4" above restores it and stays correct after the upstream fix,
        // since an explicit -t always wins over the fixed min(4, cores) default. The env gate is
        // a separate, thread-independent win on the recognizer's scalar convs; upstream flips it
        // default-on for the recognizer after this report, and the env keeps precedence, so
        // setting it stays harmless. Both diagnosed in CrispStrobe/CrispEmbed#45 (2026-08-09);
        // output is byte-identical in every arm. Drop both once the pin moves past v0.17.7.
        process.StartInfo.Environment["CRISPEMBED_CONV2D_MK"] = "1";

        process.Start();

        var stdOutTask = process.StandardOutput.ReadToEndAsync(cancellationToken);
        var stdErrTask = process.StandardError.ReadToEndAsync(cancellationToken);

        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeout.CancelAfter(TimeSpan.FromMinutes(_timeoutMinutes));

        try
        {
            await process.WaitForExitAsync(timeout.Token);
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            KillQuietly(process);
            Error = "CrispEmbed OCR timed out";
            SeLogger.Error("CrispEmbed CLI pipeline timed out: " + arguments);
            return string.Empty;
        }
        catch (OperationCanceledException)
        {
            KillQuietly(process);
            throw;
        }

        var output = await stdOutTask;
        var errorOutput = await stdErrTask;
        if (!string.IsNullOrWhiteSpace(errorOutput))
        {
            LogServerOutput("crispembed: " + errorOutput.Trim());
        }

        if (process.ExitCode != 0)
        {
            Error = string.IsNullOrWhiteSpace(errorOutput)
                ? $"CrispEmbed exited with code {process.ExitCode}"
                : errorOutput.Trim();
            SeLogger.Error("CrispEmbed CLI pipeline failed: " + Error);
            return string.Empty;
        }

        return ParseCliPipelineOutput(output);
    }

    /// <summary>
    /// Picks the JSON object out of the CLI's stdout - the detector prints progress lines such as
    /// "ppocrv6-det: persistent GGML detector graph ready" before it - and returns "full_text"
    /// with the platform's newlines.
    /// </summary>
    internal static string ParseCliPipelineOutput(string? output)
    {
        if (string.IsNullOrWhiteSpace(output))
        {
            return string.Empty;
        }

        foreach (var line in output.Split('\n'))
        {
            var trimmed = line.Trim();
            if (!trimmed.StartsWith('{'))
            {
                continue;
            }

            try
            {
                using var document = JsonDocument.Parse(trimmed);
                if (document.RootElement.TryGetProperty("full_text", out var fullText))
                {
                    return (fullText.GetString() ?? string.Empty)
                        .Replace("\r\n", "\n")
                        .Replace("\n", Environment.NewLine)
                        .Trim();
                }
            }
            catch (JsonException)
            {
                // not the result object - keep looking
            }
        }

        return string.Empty;
    }

    private static void KillQuietly(Process process)
    {
        try
        {
            if (!process.HasExited)
            {
                process.Kill(entireProcessTree: true);
            }
        }
        catch
        {
            // ignore
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

    /// <summary>
    /// Mirrors the server's output to the tools log and keeps the last few lines in memory, so a
    /// process that dies during startup can report what it actually said instead of only an exit
    /// code. Raised on threadpool threads by the async readers, hence the lock.
    /// </summary>
    private void CaptureServerOutput(string? line)
    {
        LogServerOutput(line);

        if (string.IsNullOrWhiteSpace(line))
        {
            return;
        }

        lock (_outputLock)
        {
            _lastOutputLines.Enqueue(line.Trim());
            while (_lastOutputLines.Count > MaxCapturedOutputLines)
            {
                _lastOutputLines.Dequeue();
            }
        }
    }

    /// <summary>
    /// Builds the message shown when the server exits before it starts listening. The useful part
    /// is almost always on the process's own stderr - a missing shared library, an unreadable
    /// model - so wait for the async readers to drain and append what they saw. Without this the
    /// user only ever sees "exited with code N" while the real cause sits in the tools log.
    /// </summary>
    private async Task<string> BuildStartupFailureMessage(Process process)
    {
        var exitCode = process.ExitCode;

        // The process has already exited; this only lets the redirected streams finish. It is
        // capped because a surviving grandchild can hold the pipe open indefinitely.
        try
        {
            using var drain = new CancellationTokenSource(TimeSpan.FromSeconds(2));
            await process.WaitForExitAsync(drain.Token);
        }
        catch (OperationCanceledException)
        {
            // report whatever was captured before the deadline
        }

        var message = $"CrispEmbed server exited with code {exitCode}";

        var hint = GetExitCodeHint(exitCode);
        if (!string.IsNullOrEmpty(hint))
        {
            message += Environment.NewLine + Environment.NewLine + hint;
        }

        string output;
        lock (_outputLock)
        {
            output = string.Join(Environment.NewLine, _lastOutputLines);
        }

        if (!string.IsNullOrWhiteSpace(output))
        {
            message += Environment.NewLine + Environment.NewLine + output;
        }

        return message;
    }

    /// <summary>
    /// Plain-language help for the exit codes the dynamic loader and the shell use on Unix, which
    /// a user cannot be expected to decode. 127 means a shared library could not be loaded.
    /// The original cause (issue #13205) was an unshipped OpenBLAS dependency, fixed upstream in
    /// CrispEmbed v0.17.5; what remains is typically a glibc too old for the pinned build, so the
    /// hint no longer names a specific package to install.
    /// </summary>
    internal static string GetExitCodeHint(int exitCode)
    {
        if (OperatingSystem.IsWindows())
        {
            return string.Empty;
        }

        return exitCode switch
        {
            127 => "Exit code 127 means a shared library the CrispEmbed binaries depend on could not be loaded." +
                   Environment.NewLine +
                   "This is usually a system C library (glibc) older than the one the CrispEmbed build needs;" +
                   Environment.NewLine +
                   "the CUDA builds in particular require a newer glibc than the CPU builds." +
                   Environment.NewLine +
                   "Run \"ldd\" on the CrispEmbed binary to see which library is missing.",
            126 => "Exit code 126 means the CrispEmbed binary could not be executed - it may be missing the" +
                   Environment.NewLine +
                   "executable permission, or be blocked by the system.",
            _ => string.Empty,
        };
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
