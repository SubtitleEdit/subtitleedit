using CommunityToolkit.Mvvm.ComponentModel;
using Nikse.SubtitleEdit.Logic.NetflixQualityCheck;

namespace Nikse.SubtitleEdit.Features.Tools.FixNetflixErrors;

public partial class NetflixCheckDisplayItem : ObservableObject
{
    [ObservableProperty] private string _name = string.Empty;
    [ObservableProperty] private bool _isSelected;

    public INetflixQualityChecker Checker { get; }

    public NetflixCheckDisplayItem(INetflixQualityChecker checker, string name, bool isSelected = true)
    {
        Checker = checker;
        Name = name;
        IsSelected = isSelected;
    }
}
