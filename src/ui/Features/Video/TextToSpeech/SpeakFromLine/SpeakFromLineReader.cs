using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Video.TextToSpeech.Engines;
using Nikse.SubtitleEdit.Features.Video.TextToSpeech.Voices;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.VideoPlayers.LibMpvDynamic;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Video.TextToSpeech.SpeakFromLine;

/// <summary>
/// Reads subtitle lines aloud one after another, ignoring their time codes. The next line is
/// generated while the current one plays, so a fast engine speaks without gaps.
/// </summary>
public sealed class SpeakFromLineReader
{
    private readonly ITtsEngine _engine;
    private readonly Voice _voice;
    private readonly TtsLanguage? _language;

    public SpeakFromLineReader(ITtsEngine engine, Voice voice, TtsLanguage? language)
    {
        _engine = engine;
        _voice = voice;
        _language = language;
    }

    /// <summary>
    /// Speaks <paramref name="texts"/> in order until the end, or until
    /// <paramref name="cancellationToken"/> is cancelled.
    /// </summary>
    /// <param name="onLineStarting">Called just before line i is heard - the caller selects the row.</param>
    /// <param name="keepGoing">Polled while speaking; false stops the reading (the user moved on).</param>
    /// <param name="getSpeed">Polled while speaking: the playback speed, so a change is heard at once.</param>
    public async Task RunAsync(IReadOnlyList<string> texts, Action<int> onLineStarting, Func<bool> keepGoing, Func<double> getSpeed, CancellationToken cancellationToken)
    {
        var folder = Path.Combine(Path.GetTempPath(), "se-speak-" + Guid.NewGuid());
        Directory.CreateDirectory(folder);
        LibMpvDynamicPlayer? player = null;
        Task<string?>? pending = null;
        try
        {
            player = CreatePlayer();
            pending = GenerateAsync(texts[0], folder, cancellationToken);
            for (var i = 0; i < texts.Count; i++)
            {
                var fileName = await pending;
                pending = i + 1 < texts.Count ? GenerateAsync(texts[i + 1], folder, cancellationToken) : null;
                cancellationToken.ThrowIfCancellationRequested();
                if (fileName == null)
                {
                    continue; // nothing to say on this line (empty or only tags)
                }

                if (!keepGoing())
                {
                    return;
                }

                onLineStarting(i);
                var finished = await PlayAndWaitAsync(player, fileName, keepGoing, getSpeed, cancellationToken);
                TryDelete(fileName);
                if (!finished)
                {
                    return;
                }
            }
        }
        finally
        {
            if (pending != null)
            {
                // Let an in-flight generation finish before the folder goes, and never let its
                // failure (or cancellation) escape from here.
                try
                {
                    await pending;
                }
                catch
                {
                    // ignore
                }
            }

            DisposePlayer(player);
            try
            {
                Directory.Delete(folder, true);
            }
            catch
            {
                // ignore
            }
        }
    }

    public static string GetSpeakableText(string text)
    {
        return Utilities.UnbreakLine(HtmlUtil.RemoveHtmlTags(text, alsoSsaTags: true)).Trim();
    }

    private Task<string?> GenerateAsync(string text, string folder, CancellationToken cancellationToken) =>
        GenerateClipAsync(_engine, _voice, _language, text, folder, cancellationToken);

    /// <summary>Generates one line's clip; null when there is nothing to say.</summary>
    internal static Task<string?> GenerateClipAsync(ITtsEngine engine, Voice voice, TtsLanguage? language, string text, string folder, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return Task.FromResult<string?>(null);
        }

        // Off the UI thread: the local engines start their server synchronously on first use.
        return Task.Run(async () =>
        {
            var result = await engine.Speak(text, folder, voice, language, null, null, cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();
            if (result.Error || string.IsNullOrEmpty(result.FileName) || !File.Exists(result.FileName))
            {
                throw new InvalidOperationException(string.IsNullOrEmpty(result.ErrorMessage)
                    ? $"{engine.Name} did not produce any audio."
                    : result.ErrorMessage);
            }

            return (string?)result.FileName;
        }, cancellationToken);
    }

    internal static LibMpvDynamicPlayer CreatePlayer()
    {
        var player = new LibMpvDynamicPlayer();
        player.LoadLib();
        var err = player.Initialize();
        if (err < 0)
        {
            var message = player.GetErrorString(err);
            player.Dispose();
            throw new InvalidOperationException($"Failed to initialize mpv: {message}");
        }

        return player;
    }

    /// <returns>True when the clip played to its end, false when <paramref name="keepGoing"/> said stop.</returns>
    private static async Task<bool> PlayAndWaitAsync(LibMpvDynamicPlayer player, string fileName, Func<bool> keepGoing, Func<double> getSpeed, CancellationToken cancellationToken)
    {
        await player.LoadAudio(fileName);
        var speed = getSpeed();
        player.Speed = speed;

        // LoadAudio keeps the file open at its end, which pauses mpv - and the pause carries over
        // to the next file, so un-pause explicitly.
        player.Play();

        // mpv pauses itself when the clip ends ("keep-open"). The grace period skips the moment
        // right after loadfile, before the new clip has reported that it is playing. A clip mpv
        // could not load never pauses - mpv just goes idle - so a position that stops moving
        // also ends the wait instead of hanging the reading forever.
        var stopwatch = Stopwatch.StartNew();
        var stallDetector = new PlaybackStallDetector(StallTimeoutMilliseconds);
        while (true)
        {
            await Task.Delay(50, cancellationToken);
            if (!keepGoing())
            {
                return false;
            }

            var newSpeed = getSpeed();
            if (Math.Abs(newSpeed - speed) > 0.001)
            {
                speed = newSpeed;
                player.Speed = speed;
            }

            if (stopwatch.ElapsedMilliseconds > 300 && player.IsPaused)
            {
                return true;
            }

            if (stallDetector.IsStalled(player.Position, stopwatch.ElapsedMilliseconds))
            {
                Se.LogError(new InvalidOperationException($"Playback did not progress for {StallTimeoutMilliseconds} ms"),
                    $"Speak from current line: skipping {Path.GetFileName(fileName)}");
                return true;
            }
        }
    }

    internal const int StallTimeoutMilliseconds = 5000;

    /// <summary>
    /// Reports when the playback position has not moved for a while - mpv goes idle without
    /// pausing when it cannot load a clip, so the end-of-clip pause never comes.
    /// </summary>
    internal sealed class PlaybackStallDetector
    {
        private readonly long _timeoutMilliseconds;
        private double _lastPosition = double.NaN;
        private long _lastChangeMilliseconds;

        public PlaybackStallDetector(long timeoutMilliseconds)
        {
            _timeoutMilliseconds = timeoutMilliseconds;
        }

        public bool IsStalled(double position, long elapsedMilliseconds)
        {
            if (double.IsNaN(_lastPosition) || Math.Abs(position - _lastPosition) > 0.0001)
            {
                _lastPosition = position;
                _lastChangeMilliseconds = elapsedMilliseconds;
                return false;
            }

            return elapsedMilliseconds - _lastChangeMilliseconds >= _timeoutMilliseconds;
        }
    }

    internal static void DisposePlayer(LibMpvDynamicPlayer? player)
    {
        if (player == null)
        {
            return;
        }

        // Pause now so cancelling is heard at once, then destroy the core off the UI thread -
        // mpv_terminate_destroy blocks until its workers exit (see TextToSpeechViewModel.DisposePreviewPlayer).
        try
        {
            player.Pause();
        }
        catch
        {
            // ignore
        }

        Task.Run(() =>
        {
            try
            {
                player.Dispose();
            }
            catch (Exception ex)
            {
                Se.LogError(ex, "Speak from current line: disposing the audio player failed");
            }
        });
    }

    internal static void TryDelete(string fileName)
    {
        try
        {
            File.Delete(fileName);
        }
        catch
        {
            // ignore
        }
    }
}
