using CommunityToolkit.Mvvm.ComponentModel;

namespace Nikse.SubtitleEdit.Features.Video.BurnIn;

public partial class BurnInLogo : ObservableObject
{
    [ObservableProperty] private string _logoFileName;
    [ObservableProperty] private int _x;
    [ObservableProperty] private int _y;
    [ObservableProperty] private int _alpha;
    [ObservableProperty] private int _size;

    public BurnInLogo()
    {
        LogoFileName = string.Empty;
        Alpha = 100;
        Size = 100;
    }
}