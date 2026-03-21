using Nikse.SubtitleEdit.Core.BluRaySup;
using Nikse.SubtitleEdit.Core.ContainerFormats.Matroska;
using System.Collections.Generic;

namespace Nikse.SubtitleEdit.Logic;

public interface IBluRayHelper
{
    List<BluRaySupParser.PcsData> LoadBluRaySubFromMatroska(MatroskaTrackInfo track, MatroskaFile matroska, out string errorMessage);
}