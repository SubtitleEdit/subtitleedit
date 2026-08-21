using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Media;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Video.TextToSpeech.AutoCast;

/// <summary>
/// Builds one cloning reference per speaker out of the lines that speaker says in the video.
/// </summary>
/// <remarks>
/// Not simply "their longest line": a cloning model hears a speaker far better in fifteen seconds
/// than in two, so several of their lines are cut and joined. Only their own lines are used, so
/// nothing another speaker says can leak into the reference.
/// </remarks>
public static class SpeakerReferenceBuilder
{
    /// <summary>Roughly how much audio to gather per speaker. More adds little and costs time.</summary>
    internal const double TargetSeconds = 15.0;

    /// <summary>Lines shorter than this are mostly attack and silence - poor cloning material.</summary>
    internal const double MinimumLineSeconds = 1.0;

    /// <summary>Cap on how many pieces are joined, so a speaker of many one-word lines stays sane.</summary>
    internal const int MaxParts = 8;

    /// <summary>
    /// Picks which of a speaker's lines to clone from: the longest ones (they carry the most
    /// voice), until <see cref="TargetSeconds"/> is reached - returned in time order, because the
    /// joined audio and its transcript have to say the same thing in the same order.
    /// </summary>
    public static List<Paragraph> PickReferenceLines(IReadOnlyList<Paragraph> speakerLines)
    {
        var usable = speakerLines
            .Where(p => p.Duration.TotalSeconds >= MinimumLineSeconds && !string.IsNullOrWhiteSpace(p.Text))
            .ToList();

        // Nothing long enough: fall back to the longest line there is, however short. A poor
        // reference still clones something; no reference clones nothing.
        if (usable.Count == 0)
        {
            usable = speakerLines.OrderByDescending(p => p.Duration.TotalSeconds).Take(1).ToList();
        }

        var picked = new List<Paragraph>();
        var seconds = 0.0;
        foreach (var line in usable.OrderByDescending(p => p.Duration.TotalSeconds))
        {
            picked.Add(line);
            seconds += line.Duration.TotalSeconds;
            if (seconds >= TargetSeconds || picked.Count >= MaxParts)
            {
                break;
            }
        }

        return picked.OrderBy(p => p.StartTime.TotalMilliseconds).ToList();
    }

    /// <summary>
    /// Cuts the picked lines out of the video, joins them into one WAV and writes the matching
    /// transcript beside it. Returns the WAV, or null when nothing could be cut.
    /// </summary>
    public static async Task<string?> BuildAsync(
        string videoFileName,
        IReadOnlyList<Paragraph> speakerLines,
        string speakerName,
        string outputFolder,
        int audioTrackFfIndex,
        CancellationToken cancellationToken)
    {
        var picked = PickReferenceLines(speakerLines);
        if (picked.Count == 0)
        {
            return null;
        }

        Directory.CreateDirectory(outputFolder);
        var safeName = MakeSafeFileName(speakerName);
        var partsFolder = Path.Combine(outputFolder, safeName + "-parts");
        Directory.CreateDirectory(partsFolder);

        var parts = new List<string>();
        for (var i = 0; i < picked.Count; i++)
        {
            // Exactly the line, with no growing into the gaps around it: these lines are far
            // apart in the video, and the gap next to one of them is where another speaker is.
            var partFileName = Path.Combine(partsFolder, $"part-{i + 1:000}.wav");
            var cut = await PerLineVoiceClone.CutClipAsync(
                videoFileName,
                picked[i].StartTime.TotalSeconds,
                picked[i].Duration.TotalSeconds,
                partFileName,
                audioTrackFfIndex,
                cancellationToken);
            if (cut)
            {
                parts.Add(partFileName);
            }
        }

        if (parts.Count == 0)
        {
            return null;
        }

        var referenceFileName = Path.Combine(outputFolder, safeName + ".wav");
        if (parts.Count == 1)
        {
            File.Move(parts[0], referenceFileName, overwrite: true);
        }
        else if (!await ConcatAsync(parts, referenceFileName, partsFolder, cancellationToken))
        {
            // Joining failed - one good part is still a usable reference.
            File.Move(parts[0], referenceFileName, overwrite: true);
        }

        // The transcript of what the joined audio says, in the same order.
        await File.WriteAllTextAsync(
            Path.ChangeExtension(referenceFileName, ".txt"),
            string.Join(' ', picked.Select(p => Utilities.UnbreakLine(HtmlUtil.RemoveHtmlTags(p.Text ?? string.Empty, alsoSsaTags: true))).Where(t => t.Length > 0)),
            cancellationToken);

        TryDeleteFolder(partsFolder);
        return referenceFileName;
    }

    private static async Task<bool> ConcatAsync(
        IReadOnlyList<string> parts,
        string outputFileName,
        string workFolder,
        CancellationToken cancellationToken)
    {
        try
        {
            // ffmpeg's concat demuxer reads the parts from a list file; single quotes in a path
            // are the one thing that breaks its syntax, so they are escaped as it expects.
            var listFileName = Path.Combine(workFolder, "parts.txt");
            var list = new StringBuilder();
            foreach (var part in parts)
            {
                list.AppendLine($"file '{part.Replace("'", @"'\''")}'");
            }

            await File.WriteAllTextAsync(listFileName, list.ToString(), cancellationToken);

            using var process = FfmpegGenerator.GetProcess(
                FfmpegGenerator.ConcatAudioClipsParameters(listFileName, outputFileName),
                (_, _) => { });
            await process.StartAndWaitAsync(cancellationToken);

            return process.ExitCode == 0 && File.Exists(outputFileName);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception exception)
        {
            Se.LogError(exception, "Auto cast: joining the reference parts failed");
            return false;
        }
    }

    private static string MakeSafeFileName(string name)
    {
        var safe = new string(name.Trim().Select(c => Path.GetInvalidFileNameChars().Contains(c) ? '_' : c).ToArray());
        return string.IsNullOrWhiteSpace(safe) ? "speaker" : safe;
    }

    private static void TryDeleteFolder(string folder)
    {
        try
        {
            if (Directory.Exists(folder))
            {
                Directory.Delete(folder, true);
            }
        }
        catch
        {
            // Leftovers in a temp folder are not worth reporting.
        }
    }
}
