using Avalonia.Media;
using System;

namespace Nikse.SubtitleEdit.Features.Video.TextToSpeech;

/// <summary>
/// The status dots and labels shared by the local TTS engine settings dialogs.
/// </summary>
public static class TtsSettingsStatus
{
    // A SolidColorBrush set as Shape.Fill is parented into that shape's visual tree, so the same
    // instance can't be shared across bound Ellipses (only one dot would render). Fresh brush per
    // assignment instead.
    public static IBrush Green() => new SolidColorBrush(Color.FromRgb(0x4C, 0xAF, 0x50));
    public static IBrush Amber() => new SolidColorBrush(Color.FromRgb(0xFF, 0x98, 0x00));
    public static IBrush Red() => new SolidColorBrush(Color.FromRgb(0xF4, 0x43, 0x36));
    public static IBrush Grey() => new SolidColorBrush(Color.FromRgb(0x9E, 0x9E, 0x9E));

    /// <summary>
    /// Status of a model file for an engine that downloads it on first use, when it runs on CrispASR.
    /// </summary>
    public static void ApplyModelStatus(bool fileInstalled, bool engineInstalled, Action<string> setLabel, Action<IBrush> setBrush)
    {
        if (fileInstalled)
        {
            setLabel("Installed");
            setBrush(Green());
            return;
        }

        if (!engineInstalled)
        {
            // No CrispASR runtime means there is nothing to auto-download into, so don't promise
            // a download that can't happen until the user installs CrispASR.
            setLabel("CrispASR required");
            setBrush(Grey());
            return;
        }

        setLabel("Auto-download on first use");
        setBrush(Grey());
    }

    /// <summary>
    /// Status of a model file that is downloaded explicitly.
    /// </summary>
    public static void ApplyModelStatus(bool installed, Action<string> setLabel, Action<IBrush> setBrush)
    {
        if (installed)
        {
            setLabel("Installed");
            setBrush(Green());
            return;
        }

        setLabel("Not downloaded");
        setBrush(Grey());
    }
}
