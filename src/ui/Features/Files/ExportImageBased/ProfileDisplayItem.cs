using CommunityToolkit.Mvvm.ComponentModel;

namespace Nikse.SubtitleEdit.Features.Files.ExportImageBased;

public partial class ProfileDisplayItem : ObservableObject
{
    [ObservableProperty] private string _name;
    [ObservableProperty] private bool _isSelected;
    public object Profile { get; set; }

    public ProfileDisplayItem(string displayName, bool isSelected, object profile)
    {
        Name = displayName;
        IsSelected = isSelected;
        Profile = profile;
    }

    public override string ToString()
    {
        return Name;
    }
}