using CommunityToolkit.Mvvm.ComponentModel;

namespace Nikse.SubtitleEdit.Features.Video.BurnIn;

public partial class BurnInLogo : ObservableObject
{
    [ObservableProperty] private string _logoFileName;
    [ObservableProperty] private int _x;
    [ObservableProperty] private int _y;
    [ObservableProperty] private int _alpha;
    [ObservableProperty] private int _size;

    /// <summary>
    /// The video resolution X/Y/Size were picked against. Batch jobs (and a resolution changed
    /// after placing the logo) can render at another size, so the overlay is scaled to it -
    /// fixed pixels put a top-right logo outside a smaller video.
    /// </summary>
    public int ReferenceWidth { get; set; }
    public int ReferenceHeight { get; set; }

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
            ReferenceWidth = ReferenceWidth,
            ReferenceHeight = ReferenceHeight,
        };
    }
}