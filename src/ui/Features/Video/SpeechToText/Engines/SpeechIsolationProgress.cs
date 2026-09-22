using Nikse.SubtitleEdit.Logic.Media;
using System;
using System.IO;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Features.Video.SpeechToText.Engines;

/// <summary>
/// Progress of a CrispASR "--separate" run, read off its stderr (#15176).
/// </summary>
/// <remarks>
/// The separator says nothing about how far it is, but on the CPU path - the slow one, minutes per
/// minute of audio - it prints "mel_band_roformer: separating (...)" once per chunk and
/// "mel_band_roformer: layer i/n" for each transformer layer of that chunk. The chunk count is
/// only printed at the very end ("segmented N samples into M chunks"), so it is computed up front
/// from the audio's length the same way the separator does: the input is resampled to 44.1 kHz
/// and cut into 8 s windows with 25% overlap. The GPU path runs one fused graph per chunk and
/// prints nothing per chunk, so there the progress simply stays at zero - it is also the fast one.
/// </remarks>
public sealed partial class SpeechIsolationProgress
{
    /// <summary>The separator resamples every input to the model's rate before chunking.</summary>
    public const int SampleRate = 44100;

    /// <summary>The model's trained window: 8 s at 44.1 kHz.</summary>
    public const int ChunkSamples = 352800;

    /// <summary>Windows advance by 75% of their length (25% overlap).</summary>
    public const int StrideSamples = 264600;

    [GeneratedRegex(@"^mel_band_roformer: separating \(", RegexOptions.CultureInvariant)]
    private static partial Regex ChunkStartRegex();

    [GeneratedRegex(@"^mel_band_roformer: layer (\d+)/(\d+)", RegexOptions.CultureInvariant)]
    private static partial Regex LayerRegex();

    [GeneratedRegex(@"^mel_band_roformer: segmented \d+ samples into (\d+) chunks", RegexOptions.CultureInvariant)]
    private static partial Regex SegmentedRegex();

    private readonly object _lock = new();
    private int _layersDone;
    private int _layerCount;

    public SpeechIsolationProgress(int chunkCount)
    {
        ChunkCount = Math.Max(0, chunkCount);
    }

    /// <summary>How many chunks the separator will process, or 0 when the audio's length is unknown.</summary>
    public int ChunkCount { get; private set; }

    /// <summary>The chunk being processed (1-based), 0 before the first one starts.</summary>
    public int CurrentChunk { get; private set; }

    /// <summary>0..1 of the separation done, or null when no progress can be told yet.</summary>
    public double? Fraction
    {
        get
        {
            lock (_lock)
            {
                if (ChunkCount <= 0 || CurrentChunk <= 0)
                {
                    return null;
                }

                var layerPart = _layerCount > 0 ? (double)_layersDone / _layerCount : 0.0;
                return Math.Clamp((CurrentChunk - 1 + layerPart) / ChunkCount, 0.0, 1.0);
            }
        }
    }

    /// <summary>Whole percent of <see cref="Fraction"/>, or null when there is none yet.</summary>
    public int? Percent => Fraction is { } f ? (int)Math.Floor(f * 100.0) : null;

    /// <summary>How many chunks the separator cuts <paramref name="durationSeconds"/> of audio into.</summary>
    public static int GetChunkCount(double durationSeconds)
    {
        if (double.IsNaN(durationSeconds) || durationSeconds <= 0)
        {
            return 0;
        }

        var samples = (long)Math.Round(durationSeconds * SampleRate);
        if (samples <= ChunkSamples)
        {
            return 1; // short audio is run whole, no chunking
        }

        return (int)((samples + StrideSamples - 1) / StrideSamples);
    }

    /// <summary>
    /// The chunk count for the wav the separator is about to read, or 0 when its header cannot be
    /// read - progress is then reported without a percentage.
    /// </summary>
    public static int GetChunkCountFromWaveFile(string waveFileName)
    {
        try
        {
            using var stream = File.OpenRead(waveFileName);
            var header = new WaveHeader2(stream);
            return GetChunkCount(header.LengthInSeconds);
        }
        catch
        {
            return 0;
        }
    }

    /// <summary>
    /// Feeds one line of the separator's output. Returns true when it moved the progress, so a
    /// caller can update its status only then.
    /// </summary>
    public bool TryUpdate(string? line)
    {
        if (string.IsNullOrEmpty(line))
        {
            return false;
        }

        lock (_lock)
        {
            if (ChunkStartRegex().IsMatch(line))
            {
                CurrentChunk++;
                _layersDone = 0;
                _layerCount = 0;
                if (ChunkCount > 0 && CurrentChunk > ChunkCount)
                {
                    ChunkCount = CurrentChunk; // the estimate was short - never report more than 100%
                }

                return true;
            }

            var layer = LayerRegex().Match(line);
            if (layer.Success)
            {
                if (CurrentChunk == 0)
                {
                    CurrentChunk = 1; // the chunk line was lost - still count the layers
                }

                _layersDone = int.Parse(layer.Groups[1].Value);
                _layerCount = Math.Max(_layersDone, int.Parse(layer.Groups[2].Value));
                return true;
            }

            var segmented = SegmentedRegex().Match(line);
            if (segmented.Success)
            {
                // Printed after the last chunk: the definitive count, so the bar ends on 100%.
                ChunkCount = int.Parse(segmented.Groups[1].Value);
                CurrentChunk = ChunkCount;
                _layersDone = _layerCount = 1;
                return true;
            }
        }

        return false;
    }
}
