using System.Collections.Generic;

namespace Nikse.SubtitleEdit.Logic.Config;

public class SeExportImages
{
    public List<SeExportImagesProfile> Profiles { get; set; }
    public string LastProfileName { get; set; }

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
    }
}