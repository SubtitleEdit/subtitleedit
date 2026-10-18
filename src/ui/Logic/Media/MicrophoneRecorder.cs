using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Logic.Config;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Logic.Media;

/// <summary>
/// Records the default microphone to a 16 kHz mono wav through ffmpeg's capture devices
/// (avfoundation on macOS, dshow on Windows, pulse/alsa on Linux), so the "record speech to
/// text box" button needs no audio library of its own - ffmpeg is already a requirement of
/// every speech-to-text engine the recording is handed to.
/// <para>
/// The recording is stopped by writing "q" to ffmpeg's stdin, the same key an interactive
/// ffmpeg takes: it then flushes and finalizes the wav header. Killing the process instead would
/// leave a wav whose header says 0 bytes of data.
/// </para>
/// </summary>
public sealed class MicrophoneRecorder : IDisposable
{
    public const int SampleRate = 16000;

    private static readonly Regex DshowAudioDeviceRegex = new("\"(?<name>[^\"]+)\"\\s*\\(audio\\)", RegexOptions.Compiled);

    private Process? _process;
    private readonly FfmpegOutputTail _output = new();

    public string OutputFileName { get; } = Path.Combine(Path.GetTempPath(), $"se_microphone_{Guid.NewGuid()}.wav");

    public bool IsRecording => _process is { HasExited: false };

    /// <summary>The last lines ffmpeg wrote, for the error status/log when a recording fails.</summary>
    public string FfmpegOutput => _output.ToString();

    /// <summary>
    /// Starts recording. Returns null on success, else a short reason (no capture device found,
    /// ffmpeg could not start, or ffmpeg quit right away - typically a denied microphone permission).
    /// </summary>
    public async Task<string?> StartAsync()
    {
        var arguments = BuildCaptureArguments(OutputFileName);
        if (arguments == null)
        {
            return Se.Language.Main.NoMicrophoneFound;
        }

        var process = new Process
        {
            StartInfo =
            {
                FileName = FfmpegHelper.GetFfmpegLocation(),
                Arguments = arguments,
                UseShellExecute = false,
                RedirectStandardInput = true,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                CreateNoWindow = true,
            }
        };
        process.ErrorDataReceived += _output.Handler;
        process.OutputDataReceived += _output.Handler;

        try
        {
            process.Start();
            process.BeginErrorReadLine();
            process.BeginOutputReadLine();
        }
        catch (Exception exception)
        {
            Se.LogError(exception, "MicrophoneRecorder: could not start ffmpeg");
            process.Dispose();
            return string.Format(Se.Language.Main.RecordingFailedX, exception.Message);
        }

        // A capture that cannot open the device fails within a few hundred ms (wrong device
        // name, permission denied, no PulseAudio) - catch that here so the button does not sit
        // in its "recording" state over a dead process.
        await Task.Delay(600);
        if (process.HasExited)
        {
            var reason = _output.ToString();
            Se.LogError($"MicrophoneRecorder: ffmpeg exited with {process.ExitCode}{Environment.NewLine}" +
                        $"ffmpeg {arguments}{Environment.NewLine}{reason}");
            process.Dispose();
            return string.Format(Se.Language.Main.RecordingFailedX, LastLine(reason));
        }

        _process = process;
        return null;
    }

    /// <summary>
    /// Stops the recording and waits for ffmpeg to finalize the file. Returns true when a wav
    /// with audio data was written.
    /// </summary>
    public async Task<bool> StopAsync()
    {
        var process = _process;
        _process = null;
        if (process == null)
        {
            return false;
        }

        try
        {
            if (!process.HasExited)
            {
                try
                {
                    await process.StandardInput.WriteAsync("q");
                    await process.StandardInput.FlushAsync();
                }
                catch (Exception)
                {
                    // stdin already closed - fall through to the wait/kill below
                }

                using var cts = new System.Threading.CancellationTokenSource(TimeSpan.FromSeconds(5));
                try
                {
                    await process.WaitForExitAsync(cts.Token);
                }
                catch (OperationCanceledException)
                {
                    process.Kill(true);
                    await process.WaitForExitAsync();
                }
            }
        }
        finally
        {
            process.Dispose();
        }

        // 44 bytes is a wav header with no samples: nothing was captured (or the header was
        // never finalized), so there is nothing to transcribe.
        var info = new FileInfo(OutputFileName);
        return info.Exists && info.Length > 44;
    }

    public void Dispose()
    {
        var process = _process;
        _process = null;
        if (process != null)
        {
            try
            {
                if (!process.HasExited)
                {
                    process.Kill(true);
                }
            }
            catch (Exception)
            {
                // already gone
            }

            process.Dispose();
        }

        try
        {
            File.Delete(OutputFileName);
        }
        catch (Exception)
        {
            // temp file - best effort
        }
    }

    /// <summary>
    /// The ffmpeg arguments that capture the default microphone of the running OS to
    /// <paramref name="outputFileName"/>, or null when no capture input is available.
    /// </summary>
    public static string? BuildCaptureArguments(string outputFileName)
    {
        if (OperatingSystem.IsMacOS())
        {
            return BuildCaptureArguments("avfoundation", ":default", outputFileName);
        }

        if (OperatingSystem.IsWindows())
        {
            var device = GetFirstDshowAudioDevice();
            return device == null ? null : BuildWindowsCaptureArguments(device, outputFileName);
        }

        // Linux: PulseAudio/PipeWire is the normal desktop case; alsa is the fallback for a
        // bare ffmpeg build or a system without a sound server.
        var inputFormat = HasInputDevice("pulse") ? "pulse" : "alsa";
        return BuildCaptureArguments(inputFormat, "default", outputFileName);
    }

    public static string BuildWindowsCaptureArguments(string deviceName, string outputFileName)
    {
        return BuildCaptureArguments("dshow", "audio=\"" + deviceName + "\"", outputFileName);
    }

    private static string BuildCaptureArguments(string inputFormat, string input, string outputFileName)
    {
        // -nostdin is deliberately absent: stdin is how the recording is stopped.
        return $"-hide_banner -y -f {inputFormat} -i {input} -ac 1 -ar {SampleRate} -c:a pcm_s16le \"{outputFileName}\"";
    }

    /// <summary>
    /// The audio device names from "ffmpeg -list_devices true -f dshow -i dummy" output, in the
    /// order ffmpeg lists them (the first is Windows' default capture device in practice).
    /// </summary>
    public static List<string> ParseDshowAudioDevices(string ffmpegOutput)
    {
        var result = new List<string>();
        foreach (Match match in DshowAudioDeviceRegex.Matches(ffmpegOutput))
        {
            var name = match.Groups["name"].Value;
            if (!result.Contains(name))
            {
                result.Add(name);
            }
        }

        return result;
    }

    private static string? GetFirstDshowAudioDevice()
    {
        var output = RunFfmpeg("-hide_banner -list_devices true -f dshow -i dummy");
        var devices = ParseDshowAudioDevices(output);
        return devices.Count > 0 ? devices[0] : null;
    }

    private static bool HasInputDevice(string name)
    {
        var output = RunFfmpeg("-hide_banner -devices");
        foreach (var line in output.SplitToLines())
        {
            // " D  pulse           Pulse audio input"
            var trimmed = line.Trim();
            if (trimmed.StartsWith("D", StringComparison.Ordinal) && trimmed.Length > 2)
            {
                var rest = trimmed.Substring(1).TrimStart('.', 'E', ' ');
                if (rest.StartsWith(name + " ", StringComparison.Ordinal) || rest == name)
                {
                    return true;
                }
            }
        }

        return false;
    }

    /// <summary>Runs ffmpeg with <paramref name="arguments"/> and returns stdout+stderr (ffmpeg lists to stderr).</summary>
    private static string RunFfmpeg(string arguments)
    {
        try
        {
            using var process = new Process
            {
                StartInfo =
                {
                    FileName = FfmpegHelper.GetFfmpegLocation(),
                    Arguments = "-nostdin " + arguments,
                    UseShellExecute = false,
                    RedirectStandardError = true,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true,
                }
            };
            process.Start();
            var stderrTask = process.StandardError.ReadToEndAsync();
            var stdout = process.StandardOutput.ReadToEnd();
            if (!process.WaitForExit(10_000))
            {
                process.Kill(true);
            }

            return stdout + Environment.NewLine + stderrTask.Result;
        }
        catch (Exception exception)
        {
            Se.LogError(exception, "MicrophoneRecorder: ffmpeg " + arguments);
            return string.Empty;
        }
    }

    private static string LastLine(string text)
    {
        var lines = text.SplitToLines();
        for (var i = lines.Count - 1; i >= 0; i--)
        {
            if (!string.IsNullOrWhiteSpace(lines[i]))
            {
                return lines[i].Trim();
            }
        }

        return string.Empty;
    }
}
