using System.Collections.Generic;
using System.IO;
using System.Text;
using Nikse.SubtitleEdit.Core.BluRaySup;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.ContainerFormats.Matroska;

namespace Nikse.SubtitleEdit.Logic;

public class BluRayHelper : IBluRayHelper
{
    public List<BluRaySupParser.PcsData> LoadBluRaySubFromMatroska(MatroskaTrackInfo track, MatroskaFile matroska, out string errorMessage, MatroskaFile.LoadMatroskaCallback? progressCallback = null)
    {
        errorMessage = string.Empty;
        if (track.ContentEncodingType == 1)
        {
            errorMessage = "Encrypted content not supported";
            return new List<BluRaySupParser.PcsData>();
        }

        return BluRaySupParser.ParseBluRaySupFromMatroska(track, matroska, progressCallback);
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