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

    /// <summary>
    /// The logo dialog edits its instance live (dragging writes X/Y, the sliders write Alpha and
    /// Size), so it has to be handed a copy - otherwise Cancel keeps every change.
    /// </summary>
    public BurnInLogo Clone()
    {
        return new BurnInLogo
        {
            LogoFileName = LogoFileName,
            X = X,
            Y = Y,
            Alpha = Alpha,
            Size = Size,
        };
    }
}