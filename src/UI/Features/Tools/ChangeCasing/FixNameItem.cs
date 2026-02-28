using CommunityToolkit.Mvvm.ComponentModel;

namespace Nikse.SubtitleEdit.Features.Tools.ChangeCasing;

public partial class FixNameItem : ObservableObject
{
    [ObservableProperty] private bool _isChecked;
    public string Name { get; set; }

    public FixNameItem(string name, bool isChecked)
    {
        Name = name;
        IsChecked = isChecked;
    }

    public override string ToString()
    {
        return Name;
    }
}
