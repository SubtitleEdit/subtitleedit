using System.Collections.Generic;

namespace Nikse.SubtitleEdit.Logic.Config;

public class SeFixCommonErrors
{
    public string LastLanguageCode { get; set; }
    public List<SeFixCommonErrorsProfile> Profiles { get; set; }
    public string LastProfileName { get; set; }
    public bool SkipStep1 { get; set; }

    public SeFixCommonErrors()
    {
        LastLanguageCode = string.Empty;

        Profiles = new List<SeFixCommonErrorsProfile>
        {
            new SeFixCommonErrorsProfile
            {
                ProfileName = "Default",
                SelectedRules = SeFixCommonErrorsProfile.DefaultFixes
            }
        };

        LastProfileName = "Default";
    }
}