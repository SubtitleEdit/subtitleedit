using Nikse.SubtitleEdit.Logic.Config;
using System.Collections.Generic;

namespace Nikse.SubtitleEdit.Features.Options.Settings;

public class WindowsRenderingModeDisplay
{
    public string Name { get; set; } = string.Empty;
    public WindowsRenderingModeType Mode { get; set; }

    public override string ToString()
    {
        return Name;
    }

    public static List<WindowsRenderingModeDisplay> List()
    {
        return
        [
            new() { Name = Se.Language.Options.Settings.WindowsRenderingModeDefault, Mode = WindowsRenderingModeType.Default },
            new() { Name = Se.Language.Options.Settings.WindowsRenderingModeRedirectionSurface, Mode = WindowsRenderingModeType.RedirectionSurface },
            new() { Name = Se.Language.Options.Settings.WindowsRenderingModeLowLatencySwapChain, Mode = WindowsRenderingModeType.LowLatencySwapChain },
            new() { Name = Se.Language.Options.Settings.WindowsRenderingModeSoftware, Mode = WindowsRenderingModeType.Software },
        ];
    }
}
