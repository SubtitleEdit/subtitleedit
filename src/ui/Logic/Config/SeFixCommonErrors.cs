using System.Collections.Generic;

namespace Nikse.SubtitleEdit.Logic.Config;

public class SeFixCommonErrors
{
    public string LastLanguageCode { get; set; }
    public List<SeFixCommonErrorsProfile> Profiles { get; set; }
    public string LastProfileName { get; set; }
    public bool SkipStep1 { get; set; }

    /// <summary>
    /// Let "Fix common OCR errors" guess at unknown words (which also splits words it thinks are
    /// two words run together). Off by default: on a normal, non-OCR'd subtitle the guessing breaks
    /// far more words than it fixes (issue #12441). The OCR window keeps its own separate setting.
    /// </summary>
    public bool TryToGuessUnknownWords { get; set; }

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