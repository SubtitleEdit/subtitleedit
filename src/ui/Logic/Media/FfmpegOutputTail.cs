using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Nikse.SubtitleEdit.Logic.Media;

/// <summary>
/// Keeps the last lines ffmpeg wrote, for the error log when a run fails. Pass
/// <see cref="Handler"/> to <see cref="FfmpegGenerator.GetProcess"/>.
/// </summary>
/// <remarks>
/// Only the tail: the reason for a failure is in ffmpeg's last lines, and a long run writes a
/// progress line every half second.
/// </remarks>
internal sealed class FfmpegOutputTail
{
    private const int MaxLines = 15;

    private readonly Queue<string> _lines = new();
    private readonly object _lock = new();

    /// <summary>Called from the process' output reader threads.</summary>
    public void Handler(object sender, DataReceivedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(e.Data))
        {
            return;
        }

        Add(e.Data);
    }

    internal void Add(string line)
    {
        lock (_lock)
        {
            _lines.Enqueue(line);
            while (_lines.Count > MaxLines)
            {
                _lines.Dequeue();
            }
        }
    }

    public override string ToString()
    {
        lock (_lock)
        {
            return string.Join(Environment.NewLine, _lines);
        }
    }
}
