using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Features.Files.ImportPlainText;
using Nikse.SubtitleEdit.Features.Video.SpeechToText.Engines;
using Nikse.SubtitleEdit.Logic.Config;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Tools.ImproveTimeCodes;

/// <summary>
/// Transcribes a whole audio file with a Crisp ASR speech-to-text model and returns every
/// word it heard with its time - one cue per word, from <c>-ml 1 -sow</c>.
/// </summary>
public sealed class CrispAsrWordTranscriber
{
    private readonly string _executable;
    private readonly string _backend;
    private readonly string _model;
    private readonly string? _vadModel;

    public CrispAsrWordTranscriber(string executable, string backend, string model, string? vadModel)
    {
        _executable = executable;
        _backend = backend;
        _model = model;
        _vadModel = vadModel;
    }

    /// <param name="languageCode">Two-letter code of the spoken language, or "auto".</param>
    /// <param name="progress">Percent done, 0-100, as crispasr reports it.</param>
    public async Task<List<SpeechToTextCheck.HeardWord>> TranscribeAsync(
        string audioFileName,
        string languageCode,
        IProgress<double>? progress,
        CancellationToken cancellationToken)
    {
        var outputBase = Path.Combine(
            Path.GetDirectoryName(audioFileName) ?? Path.GetTempPath(),
            Path.GetFileNameWithoutExtension(audioFileName) + "-words");
        var outputFileName = outputBase + ".srt";

        var vadPart = _vadModel != null && (_vadModel == CrispAsrVadModel.WebRtc || File.Exists(_vadModel))
            ? " " + CrispAsrVadModel.BuildArguments(_vadModel)
            : string.Empty;
        var arguments =
            $"--backend {_backend} -l {languageCode} -m \"{_model}\"{vadPart} -f \"{audioFileName}\" " +
            $"--output-srt -of \"{outputBase}\" -ml 1 -sow --print-progress";
        Se.WriteToolsLog($"{_executable} {arguments}");

        using var process = new Process
        {
            StartInfo = new ProcessStartInfo(_executable, arguments)
            {
                WindowStyle = ProcessWindowStyle.Hidden,
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                WorkingDirectory = Path.GetDirectoryName(_executable),
            },
        };

        var errors = new StringBuilder();
        DataReceivedEventHandler onLine = (_, e) =>
        {
            if (e.Data == null)
            {
                return;
            }

            if (TryParseProgress(e.Data, out var percent))
            {
                progress?.Report(percent);
            }
            else if (!e.Data.StartsWith('['))
            {
                // Everything but the transcript itself, which also goes to the .srt.
                lock (errors)
                {
                    errors.AppendLine(e.Data);
                }
            }
        };
        process.OutputDataReceived += onLine;
        process.ErrorDataReceived += onLine;

        process.Start();
        process.BeginErrorReadLine();
        process.BeginOutputReadLine();

        try
        {
            await process.WaitForExitAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            try
            {
                if (!process.HasExited)
                {
                    process.Kill(true);
                }
            }
            catch
            {
                // It exited on its own in the meantime.
            }

            throw;
        }

        if (process.ExitCode != 0 || !File.Exists(outputFileName))
        {
            string detail;
            lock (errors)
            {
                detail = errors.ToString().Trim();
            }

            throw new ForcedAlignerException($"Speech-to-text failed (exit code {process.ExitCode}).", detail);
        }

        var lines = await File.ReadAllLinesAsync(outputFileName, cancellationToken).ConfigureAwait(false);
        return ParseWords(lines.ToList());
    }

    /// <summary>
    /// The newest Silero VAD model in the Crisp ASR folder, or null. With it, only speech is
    /// transcribed, which is quicker and keeps a word from being cut in two at a slice edge.
    /// </summary>
    public static string? FindVadModel(string crispAsrFolder)
    {
        return CrispAsrVadModel.FindSilero(crispAsrFolder);
    }

    /// <summary>
    /// The VAD chosen in speech-to-text (#15563) when its model is on disk, else Silero - no
    /// download prompt in the middle of improving time codes.
    /// </summary>
    public static string? FindVadModel(ISpeechToTextEngine engine, string? vadChoice)
    {
        var option = CrispAsrVadModel.GetEffective(vadChoice, engine);
        return CrispAsrVadModel.GetModelPath(option, engine) ?? FindVadModel(engine.GetAndCreateWhisperFolder());
    }

    /// <summary>
    /// The words of a one-word-per-cue SRT. A cue that still holds several words has its time
    /// shared out by length, as the engine gave no finer times for them.
    /// </summary>
    internal static List<SpeechToTextCheck.HeardWord> ParseWords(List<string> srtLines)
    {
        var subtitle = new Subtitle();
        new SubRip().LoadSubtitle(subtitle, srtLines, string.Empty);

        var words = new List<SpeechToTextCheck.HeardWord>();
        foreach (var paragraph in subtitle.Paragraphs)
        {
            var parts = HtmlUtil.RemoveHtmlTags(paragraph.Text, true)
                .Split(new[] { ' ', '\t', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0)
            {
                continue;
            }

            var start = paragraph.StartTime.TotalSeconds;
            var duration = Math.Max(0, paragraph.EndTime.TotalSeconds - start);
            var totalChars = parts.Sum(p => p.Length);
            foreach (var part in parts)
            {
                var length = totalChars > 0 ? duration * part.Length / totalChars : duration / parts.Length;
                words.Add(new SpeechToTextCheck.HeardWord(part, start, start + length));
                start += length;
            }
        }

        return words;
    }

    /// <summary>"crispasr: progress =  14% (1/7 slices)"</summary>
    internal static bool TryParseProgress(string line, out double percent)
    {
        percent = 0;
        if (!line.StartsWith("crispasr: progress =", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        var value = line[(line.IndexOf('=') + 1)..].TrimStart();
        var end = value.IndexOf('%');
        return end > 0 && double.TryParse(value[..end], NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out percent);
    }
}
