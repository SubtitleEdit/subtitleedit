using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.ContainerFormats.Matroska;
using Nikse.SubtitleEdit.Core.ContainerFormats.TransportStream;
using Nikse.SubtitleEdit.Core.VobSub;
using Nikse.SubtitleEdit.Logic.Config;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Nikse.SubtitleEdit.Logic;

/// <summary>
/// Turns the blocks of a DVB subtitle track in a Matroska file into images + timings - shared by
/// the main window's import (which OCRs them) and "Point sync via other subtitle" (#15911).
/// </summary>
public static class MatroskaDvbHelper
{
    /// <summary>
    /// One image per paragraph of <paramref name="subtitle"/>, which is filled with the timings.
    /// </summary>
    public static List<DvbSubPes> ParseImages(MatroskaTrackInfo track, List<MatroskaSubtitle> blocks, Subtitle subtitle)
    {
        var subtitleImages = new List<DvbSubPes>();
        foreach (var block in blocks)
        {
            try
            {
                DvbSubPes? pes = null;
                var data = block.GetData(track);
                if (data != null && data.Length > 9 && data[0] == 15 &&
                    data[1] >= SubtitleSegment.PageCompositionSegment &&
                    data[1] <= SubtitleSegment.DisplayDefinitionSegment) // sync byte + segment id
                {
                    var buffer = new byte[data.Length + 3];
                    Buffer.BlockCopy(data, 0, buffer, 2, data.Length);
                    buffer[0] = 32;
                    buffer[1] = 0;
                    buffer[buffer.Length - 1] = 255;
                    pes = new DvbSubPes(0, buffer);
                }
                else if (VobSubParser.IsMpeg2PackHeader(data))
                {
                    pes = new DvbSubPes(data, Mpeg2Header.Length);
                }
                else if (VobSubParser.IsPrivateStream1(data, 0))
                {
                    pes = new DvbSubPes(data, 0);
                }
                else if (data!.Length > 9 && data[0] == 32 && data[1] == 0 && data[2] == 14 && data[3] == 16)
                {
                    pes = new DvbSubPes(0, data);
                }

                if (pes == null && subtitle.Paragraphs.Count > 0)
                {
                    var last = subtitle.Paragraphs[subtitle.Paragraphs.Count - 1];
                    if (last.DurationTotalMilliseconds < 100)
                    {
                        last.EndTime.TotalMilliseconds = block.Start;
                        if (last.DurationTotalMilliseconds > Se.Settings.General.SubtitleMaximumDisplayMilliseconds)
                        {
                            last.EndTime.TotalMilliseconds = last.StartTime.TotalMilliseconds + 3000;
                        }
                    }
                }

                if (pes != null && pes.PageCompositions != null && pes.PageCompositions.Any(p => p.Regions.Count > 0))
                {
                    subtitleImages.Add(pes);
                    subtitle.Paragraphs.Add(new Paragraph(string.Empty, block.Start, block.End));
                }
            }
            catch
            {
                // continue
            }
        }

        for (var index = 0; index < subtitle.Paragraphs.Count; index++)
        {
            var p = subtitle.Paragraphs[index];
            if (p.DurationTotalMilliseconds < 200)
            {
                p.EndTime.TotalMilliseconds = p.StartTime.TotalMilliseconds + 3000;
            }

            var next = subtitle.GetParagraphOrDefault(index + 1);
            if (next != null && next.StartTime.TotalMilliseconds < p.EndTime.TotalMilliseconds)
            {
                p.EndTime.TotalMilliseconds = next.StartTime.TotalMilliseconds -
                                              Se.Settings.General.MinimumBetweenLines.GetMilliseconds();
            }
        }

        return subtitleImages;
    }
}
