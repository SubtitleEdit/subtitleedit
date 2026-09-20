using Nikse.SubtitleEdit.Logic.Media;
using System;
using System.Collections.Generic;
using System.IO;

namespace Nikse.SubtitleEdit.Features.Files.ImportPlainText;

/// <summary>
/// How loud the isolated speech is over time, used to find where a line stops being spoken.
///
/// A forced aligner is good at starts and hopeless at ends - it ends each line where the next
/// one begins - which is why ends are otherwise laid out by reading time. With music and sound
/// effects removed the audio itself can answer: the line ends where the speech goes quiet.
/// On audio that still has its music this does not work at all, because it never goes quiet.
/// </summary>
public sealed class SpeechEnvelope
{
    public const double FrameSeconds = 0.02;

    /// <summary>Quiet is measured against the loud passages: this far below them counts as no speech.</summary>
    private const double QuietDecibels = -30.0;

    /// <summary>A pause shorter than this is a breath or a stop consonant, not the end of the line.</summary>
    private const double QuietSeconds = 0.25;

    /// <summary>Nothing ends sooner than this after its start - the aligner's start is not frame exact.</summary>
    private const double MinimumSpeechSeconds = 0.25;

    private readonly double[] _rms;
    private readonly double _quietThreshold;

    public SpeechEnvelope(double[] rmsPerFrame)
    {
        _rms = rmsPerFrame;

        // Against the 99th percentile rather than the maximum, so one click or plosive does not
        // set the scale for the whole recording.
        var sorted = (double[])rmsPerFrame.Clone();
        Array.Sort(sorted);
        var loud = sorted.Length > 0 ? sorted[(int)((sorted.Length - 1) * 0.99)] : 0;
        _quietThreshold = loud * Math.Pow(10, QuietDecibels / 20.0);
    }

    /// <summary>
    /// Where the speech that starts at <paramref name="startSeconds"/> goes quiet, or null when
    /// it does not do so before <paramref name="limitSeconds"/>.
    /// </summary>
    public double? FindSpeechEnd(double startSeconds, double limitSeconds)
    {
        if (_quietThreshold <= 0)
        {
            return null;
        }

        var quietFramesNeeded = (int)Math.Round(QuietSeconds / FrameSeconds);
        var first = Math.Max(0, (int)(startSeconds / FrameSeconds));
        var earliestEnd = (int)((startSeconds + MinimumSpeechSeconds) / FrameSeconds);
        var last = Math.Min(_rms.Length, (int)(limitSeconds / FrameSeconds));
        var quietFrames = 0;
        var speechHeard = false;
        for (var i = first; i < last; i++)
        {
            if (_rms[i] >= _quietThreshold)
            {
                speechHeard = true;
                quietFrames = 0;
                continue;
            }

            // Quiet is only the end of the line once the line has been heard. A start that is
            // a little early, or a closing line the aligner could only place by estimate, sits
            // in silence - and "it went quiet" right away would cut it to the minimum duration.
            quietFrames++;
            if (speechHeard && i >= earliestEnd && quietFrames >= quietFramesNeeded)
            {
                return (i - quietFrames + 1) * FrameSeconds;
            }
        }

        return null;
    }

    /// <summary>
    /// Reads a 16-bit PCM wav of any sample rate and channel count, streaming - the speech stem
    /// of a feature film is over a gigabyte. Returns null for anything else.
    /// </summary>
    public static SpeechEnvelope? FromWaveFile(string waveFileName)
    {
        using var stream = new FileStream(waveFileName, FileMode.Open, FileAccess.Read, FileShare.Read, 1024 * 1024);
        var header = new WaveHeader2(stream);
        if (header.AudioFormat != WaveHeader2.AudioFormatPcm || header.BitsPerSample != 16 ||
            header.NumberOfChannels < 1 || header.SampleRate <= 0)
        {
            return null;
        }

        stream.Position = header.DataStartPosition;
        var channels = header.NumberOfChannels;
        var samplesPerFrame = (int)(header.SampleRate * FrameSeconds) * channels;
        var buffer = new byte[samplesPerFrame * 2];
        var rms = new List<double>();
        var remaining = (long)header.DataChunkSize;
        while (remaining >= buffer.Length)
        {
            var read = ReadFully(stream, buffer);
            if (read < buffer.Length)
            {
                break;
            }

            remaining -= read;
            double sum = 0;
            for (var i = 0; i < buffer.Length; i += 2)
            {
                var sample = (short)(buffer[i] | (buffer[i + 1] << 8));
                sum += (double)sample * sample;
            }

            rms.Add(Math.Sqrt(sum / samplesPerFrame));
        }

        return new SpeechEnvelope(rms.ToArray());
    }

    private static int ReadFully(Stream stream, byte[] buffer)
    {
        var total = 0;
        while (total < buffer.Length)
        {
            var read = stream.Read(buffer, total, buffer.Length - total);
            if (read == 0)
            {
                break;
            }

            total += read;
        }

        return total;
    }
}
