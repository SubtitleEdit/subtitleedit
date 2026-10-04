using Nikse.SubtitleEdit.Logic.Config;
using System.Collections.Generic;

namespace Nikse.SubtitleEdit.Features.Options.Settings;

public enum TimeCodeMode
{
    Time,
    Frames,
    FrameNumbers,
}

public class TimeCodeModeDisplay
{
    public string Name { get; set; } = string.Empty;
    public TimeCodeMode Mode { get; set; }

    public override string ToString()
    {
        return Name;
    }

    public static List<TimeCodeModeDisplay> List()
    {
        return
        [
            new() { Name = Se.Language.Options.Settings.TimeCodeModeTime, Mode = TimeCodeMode.Time },
            new() { Name = Se.Language.Options.Settings.TimeCodeModeFrames, Mode = TimeCodeMode.Frames },
            new() { Name = Se.Language.Options.Settings.TimeCodeModeFrameNumbers, Mode = TimeCodeMode.FrameNumbers },
        ];
    }
}
