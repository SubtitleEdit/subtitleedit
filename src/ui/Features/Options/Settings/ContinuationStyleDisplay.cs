using Nikse.SubtitleEdit.Logic.Config;
using System.Collections.Generic;

namespace Nikse.SubtitleEdit.Features.Options.Settings;

public class ContinuationStyleDisplay
{
    public string Name { get; set; }
    public string Code { get; set; }

    public ContinuationStyleDisplay()
    {
        Name = string.Empty;
        Code = string.Empty;
    }
    public ContinuationStyleDisplay(ContinuationStyleDisplay other)
    {
        Name = other.Name;
        Code = other.Code;
    }

    public override string ToString()
    {
        return Name;
    }

    public static List<ContinuationStyleDisplay> List()
    {
        return
        [
            new() 
            { 
                Name = Se.Language.General.None, 
                Code = Core.Enums.ContinuationStyle.None.ToString()  
            },
            new() 
            { 
                Name = Se.Language.Options.Settings.ContinuationStyleNoneTrailingDots, 
                Code = Core.Enums.ContinuationStyle.NoneTrailingDots.ToString()  
            },
            new() 
            { 
                Name = Se.Language.Options.Settings.ContinuationStyleNoneLeadingTrailingDots, 
                Code = Core.Enums.ContinuationStyle.NoneLeadingTrailingDots.ToString()  
            },
            new() 
            { 
                Name = Se.Language.Options.Settings.ContinuationStyleNoneTrailingEllipsis, 
                Code = Core.Enums.ContinuationStyle.NoneTrailingEllipsis.ToString()  
            },
            new() 
            { 
                Name = Se.Language.Options.Settings.ContinuationStyleNoneLeadingTrailingEllipsis, 
                Code = Core.Enums.ContinuationStyle.NoneLeadingTrailingEllipsis.ToString()  
            },
            new() 
            { 
                Name = Se.Language.Options.Settings.ContinuationStyleOnlyTrailingDots, 
                Code = Core.Enums.ContinuationStyle.OnlyTrailingDots.ToString()  
            },
            new() 
            { 
                Name = Se.Language.Options.Settings.ContinuationStyleLeadingTrailingDots, 
                Code = Core.Enums.ContinuationStyle.LeadingTrailingDots.ToString()  
            },
            new() 
            { 
                Name = Se.Language.Options.Settings.ContinuationStyleOnlyTrailingEllipsis, 
                Code = Core.Enums.ContinuationStyle.OnlyTrailingEllipsis.ToString()  
            },
            new() 
            { 
                Name = Se.Language.Options.Settings.ContinuationStyleLeadingTrailingEllipsis, 
                Code = Core.Enums.ContinuationStyle.LeadingTrailingEllipsis.ToString()  
            },
            new() 
            { 
                Name = Se.Language.Options.Settings.ContinuationStyleLeadingTrailingDash, 
                Code = Core.Enums.ContinuationStyle.LeadingTrailingDash.ToString()  
            },
            new() 
            { 
                Name = Se.Language.Options.Settings.ContinuationStyleLeadingTrailingDashDots, 
                Code = Core.Enums.ContinuationStyle.LeadingTrailingDashDots.ToString()  
            },
            new() 
            { 
                Name = Se.Language.Options.Settings.ContinuationStyleCustom, 
                Code = Core.Enums.ContinuationStyle.Custom.ToString()  
            },
        ];
    }
}
