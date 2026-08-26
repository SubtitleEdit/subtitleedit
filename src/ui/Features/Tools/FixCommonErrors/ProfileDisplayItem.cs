using System.Collections.Generic;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Nikse.SubtitleEdit.Features.Tools.FixCommonErrors;

public partial class ProfileDisplayItem : ObservableObject
{
    [ObservableProperty] private string _name;
    
    public ObservableCollection<FixRuleDisplayItem> FixRules { get; set; }

    /// <summary>
    /// Every rule of the profile, independent of the search filter applied to
    /// <see cref="FixRules"/> (the grid's collection). Same item instances, so selection
    /// state is shared. Apply/save must read this list - the filtered one loses rules.
    /// </summary>
    public List<FixRuleDisplayItem> AllFixRules { get; set; }

    public ProfileDisplayItem()
    {
        Name = string.Empty;
        FixRules = new ObservableCollection<FixRuleDisplayItem>();
        AllFixRules = new List<FixRuleDisplayItem>();
    }
    public override string ToString()
    {
        return Name;
    }
}
