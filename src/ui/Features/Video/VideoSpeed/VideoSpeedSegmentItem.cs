using System;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Nikse.SubtitleEdit.Features.Video.VideoSpeed;

public enum SpeedMode
{
    Multiplier,
    TargetDuration,
}

public enum AudioSpeedMode
{
    KeepPitch,
    ShiftPitch,
    Mute,
}

public partial class VideoSpeedSegmentItem : ObservableObject
{
    [ObservableProperty] private TimeSpan _startTime;
    [ObservableProperty] private TimeSpan _endTime;
    [ObservableProperty] private SpeedMode _mode = SpeedMode.TargetDuration;
    [ObservableProperty] private double _targetDurationSeconds = 5.0;
    [ObservableProperty] private double _speedMultiplier = 1.0;
    [ObservableProperty] private AudioSpeedMode _audioMode = AudioSpeedMode.KeepPitch;

    public TimeSpan OriginalDuration => EndTime > StartTime ? EndTime - StartTime : TimeSpan.Zero;

    public bool IsReverse => false;

    public double EffectiveSpeedRatio
    {
        get
        {
            if (Mode == SpeedMode.TargetDuration)
            {
                var origSec = OriginalDuration.TotalSeconds;
                if (origSec <= 0 || TargetDurationSeconds <= 0)
                {
                    return 1.0;
                }
                return origSec / TargetDurationSeconds;
            }
            return SpeedMultiplier > 0.001 ? SpeedMultiplier : 1.0;
        }
    }

    public TimeSpan OutputDuration
    {
        get
        {
            if (Mode == SpeedMode.TargetDuration)
            {
                return TimeSpan.FromSeconds(Math.Max(0.1, TargetDurationSeconds));
            }
            var ratio = EffectiveSpeedRatio;
            return ratio > 0 ? TimeSpan.FromSeconds(OriginalDuration.TotalSeconds / ratio) : OriginalDuration;
        }
    }

    public string SpeedDisplay => $"{EffectiveSpeedRatio:0.00}x";
    public string OriginalDurationDisplay => OriginalDuration.ToString(@"hh\:mm\:ss\.fff");
    public string OutputDurationDisplay => OutputDuration.ToString(@"hh\:mm\:ss\.fff");
}
