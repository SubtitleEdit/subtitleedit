using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Nikse.SubtitleEdit.Features.Tools.FixCommonErrors;

public partial class ProfileDisplayItem : ObservableObject
{
    [ObservableProperty] private string _name;
    
    public ObservableCollection<FixRuleDisplayItem> FixRules { get; set; }
    
    public ProfileDisplayItem()
    {
        Name = string.Empty;
        FixRules = new ObservableCollection<FixRuleDisplayItem>();
    }
    override public string ToString()
    {
        return Name;
    }
}
