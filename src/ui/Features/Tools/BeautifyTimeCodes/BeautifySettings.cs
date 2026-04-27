using CommunityToolkit.Mvvm.ComponentModel;

namespace Nikse.SubtitleEdit.Features.Tools.BeautifyTimeCodes;

public partial class BeautifySettings : ObservableObject
{
    [ObservableProperty]
    private bool _snapToFrames = true;

    [ObservableProperty]
    private int _frameGap = 2;

    [ObservableProperty]
    private int _shotChangeThresholdMs = 250;

    [ObservableProperty]
    private int _shotChangeOffsetFrames = 2;

    [ObservableProperty]
    private double _minDurationMs = 800;

    public Core.Common.BeautifySettings ToCore()
    {
        return new Core.Common.BeautifySettings
        {
            SnapToFrames = SnapToFrames,
            FrameGap = FrameGap,
            ShotChangeThresholdMs = ShotChangeThresholdMs,
            ShotChangeOffsetFrames = ShotChangeOffsetFrames,
            MinDurationMs = MinDurationMs
        };
    }
}
