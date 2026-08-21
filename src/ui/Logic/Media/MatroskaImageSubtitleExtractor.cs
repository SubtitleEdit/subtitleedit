using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.ContainerFormats.Matroska;
using Nikse.SubtitleEdit.Core.ContainerFormats.TransportStream;
using Nikse.SubtitleEdit.Core.VobSub;
using Nikse.SubtitleEdit.Logic.Config;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Nikse.SubtitleEdit.Logic.Media;

/// <summary>
/// Builds image based subtitle data (VobSub/DVB) from a Matroska track. Shared by the
/// track picker preview, the binary edit import, and batch convert, which used to carry
/// their own copies of this parsing.
/// </summary>
public static class MatroskaImageSubtitleExtractor
{
    public static List<VobSubMergedPack> ExtractVobSub(MatroskaTrackInfo track, MatroskaFile matroska, out Idx? idx,
        MatroskaFile.LoadMatroskaCallback? progressCallback = null)
    {
        if (track.ContentEncodingType == 1) // encrypted
        {
            idx = null;
            return new List<VobSubMergedPack>();
        }

        var sub = matroska.GetSubtitle(track.TrackNumber, progressCallback);
        return ExtractVobSub(track, sub, out idx);
    }

    public static List<VobSubMergedPack> ExtractVobSub(MatroskaTrackInfo track, List<MatroskaSubtitle> sub, out Idx? idx)
    {
        var mergedVobSubPacks = new List<VobSubMergedPack>();
        if (track.ContentEncodingType == 1) // encrypted
        {
            idx = null;
            return mergedVobSubPacks;
        }

        idx = new Idx(track.GetCodecPrivate().SplitToLines());
        foreach (var p in sub)
        {
            mergedVobSubPacks.Add(new VobSubMergedPack(p.GetData(track), TimeSpan.FromMilliseconds(p.Start), 32, null));
            mergedVobSubPacks[mergedVobSubPacks.Count - 1].EndTime = TimeSpan.FromMilliseconds(p.End);

            // fix overlapping (some versions of Handbrake make overlapping time codes - thx Hawke)
            if (mergedVobSubPacks.Count > 1 &&
                mergedVobSubPacks[mergedVobSubPacks.Count - 2].EndTime > mergedVobSubPacks[mergedVobSubPacks.Count - 1].StartTime)
            {
                mergedVobSubPacks[mergedVobSubPacks.Count - 2].EndTime =
                    TimeSpan.FromMilliseconds(mergedVobSubPacks[mergedVobSubPacks.Count - 1].StartTime.TotalMilliseconds - 1);
            }
        }

        // remove bad packs
        for (var i = mergedVobSubPacks.Count - 1; i >= 0; i--)
        {
            if (mergedVobSubPacks[i].SubPicture.SubPictureDateSize <= 2)
            {
                mergedVobSubPacks.RemoveAt(i);
            }
            else if (mergedVobSubPacks[i].SubPicture.SubPictureDateSize <= 67 &&
                     mergedVobSubPacks[i].SubPicture.Delay.TotalMilliseconds < 35)
            {
                mergedVobSubPacks.RemoveAt(i);
            }
        }

        return mergedVobSubPacks;
    }

    public static (Subtitle subtitle, List<DvbSubPes> subtitleImages) ExtractDvb(MatroskaTrackInfo track, MatroskaFile matroska,
        MatroskaFile.LoadMatroskaCallback? progressCallback = null)
    {
        var sub = matroska.GetSubtitle(track.TrackNumber, progressCallback);
        return ExtractDvb(track, sub);
    }

    public static (Subtitle subtitle, List<DvbSubPes> subtitleImages) ExtractDvb(MatroskaTrackInfo track, List<MatroskaSubtitle> sub)
    {
        var subtitleImages = new List<DvbSubPes>();
        var subtitle = new Subtitle();

        for (var index = 0; index < sub.Count; index++)
        {
            try
            {
                var msub = sub[index];
                DvbSubPes? pes = null;
                var data = msub.GetData(track);
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
                        last.EndTime.TotalMilliseconds = msub.Start;
                        if (last.DurationTotalMilliseconds > Se.Settings.General.SubtitleMaximumDisplayMilliseconds)
                        {
                            last.EndTime.TotalMilliseconds = last.StartTime.TotalMilliseconds + 3000;
                        }
                    }
                }

                if (pes != null && pes.PageCompositions != null && pes.PageCompositions.Any(p => p.Regions.Count > 0))
                {
                    subtitleImages.Add(pes);
                    subtitle.Paragraphs.Add(new Paragraph(string.Empty, msub.Start, msub.End));
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

        return (subtitle, subtitleImages);
    }
}
