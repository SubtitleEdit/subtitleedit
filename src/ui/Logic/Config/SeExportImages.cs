using System.Collections.Generic;

namespace Nikse.SubtitleEdit.Logic.Config;

public class SeExportImages
{
    public List<SeExportImagesProfile> Profiles { get; set; }
    public string LastProfileName { get; set; }

    /// <summary>
    /// Draw ASSA/SSA lines with their own style (font, colours, outline, alignment, margins)
    /// instead of the export window's settings.
    /// </summary>
    public bool UseAssaStyles { get; set; }

    public SeExportImages()
    {
        Profiles = new List<SeExportImagesProfile>
        {
            new SeExportImagesProfile
            {
                ProfileName = "Default",
            }
        };

        LastProfileName = "Default";
        UseAssaStyles = true;
    }
}