using System.Collections.Generic;
using System.IO;
using System.Text;
using Nikse.SubtitleEdit.Core.BluRaySup;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.ContainerFormats.Matroska;

namespace Nikse.SubtitleEdit.Logic;

public class BluRayHelper : IBluRayHelper
{
    public List<BluRaySupParser.PcsData> LoadBluRaySubFromMatroska(MatroskaTrackInfo track, MatroskaFile matroska, out string errorMessage)
    {
        errorMessage = string.Empty;
        if (track.ContentEncodingType == 1)
        {
            errorMessage = "Encrypted content not supported";
            return new List<BluRaySupParser.PcsData>();
        }

        var sub = matroska.GetSubtitle(track.TrackNumber, null);
        var noOfErrors = 0;
        var lastError = string.Empty;
        var subtitle = new Subtitle();
        var subtitles = new List<BluRaySupParser.PcsData>();
        var log = new StringBuilder();
        var clusterStream = new MemoryStream();
        var lastPalettes = new Dictionary<int, List<PaletteInfo>>();
        var lastBitmapObjects = new Dictionary<int, List<BluRaySupParser.OdsData>>();
        foreach (var p in sub)
        {
            var buffer = p.GetData(track);
            if (buffer != null && buffer.Length > 2)
            {
                clusterStream.Write(buffer, 0, buffer.Length);
                if (ContainsBluRayStartSegment(buffer))
                {
                    if (subtitles.Count > 0 && subtitles[subtitles.Count - 1].StartTime ==
                        subtitles[subtitles.Count - 1].EndTime)
                    {
                        subtitles[subtitles.Count - 1].EndTime = (long)((p.Start - 1) * 90.0);
                    }

                    clusterStream.Position = 0;
                    var list = BluRaySupParser.ParseBluRaySup(clusterStream, log, true, lastPalettes,
                        lastBitmapObjects);
                    foreach (var sup in list)
                    {
                        sup.StartTime = (long)((p.Start - 1) * 90.0);
                        sup.EndTime = (long)((p.End - 1) * 90.0);
                        subtitles.Add(sup);

                        // fix overlapping
                        if (subtitles.Count > 1 && sub[subtitles.Count - 2].End > sub[subtitles.Count - 1].Start)
                        {
                            subtitles[subtitles.Count - 2].EndTime = subtitles[subtitles.Count - 1].StartTime - 1;
                        }
                    }

                    clusterStream = new MemoryStream();
                }
            }
            else if (subtitles.Count > 0)
            {
                var lastSub = subtitles[subtitles.Count - 1];
                if (lastSub.StartTime == lastSub.EndTime)
                {
                    lastSub.EndTime = (long)((p.Start - 1) * 90.0);
                    if (lastSub.EndTime - lastSub.StartTime > 1000000)
                    {
                        lastSub.EndTime = lastSub.StartTime;
                    }
                }
            }
        }

        if (noOfErrors > 0)
        {
            errorMessage = string.Format("{0} error(s) occurred during extraction of bdsup\r\n\r\n{1}", noOfErrors, lastError);
        }

        return subtitles;
    }

    private static bool ContainsBluRayStartSegment(byte[] buffer)
    {
        const int epochStart = 0x80;
        var position = 0;
        while (position + 3 <= buffer.Length)
        {
            var segmentType = buffer[position];
            if (segmentType == epochStart)
            {
                return true;
            }

            var length = BluRaySupParser.BigEndianInt16(buffer, position + 1) + 3;
            position += length;
        }

        return false;
    }

    public static bool IsMatroskaFileFast(string fileName)
    {
        using var fs = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        var buffer = new byte[4];
        var bytesRead = fs.Read(buffer, 0, buffer.Length);
        if (bytesRead < 4)
        {
            return false;
        }

        // 1a 45 df a3
        return buffer[0] == 0x1a && buffer[1] == 0x45 && buffer[2] == 0xdf && buffer[3] == 0xa3;
    }
}