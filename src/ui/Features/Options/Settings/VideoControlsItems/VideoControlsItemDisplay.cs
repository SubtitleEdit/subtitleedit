using CommunityToolkit.Mvvm.ComponentModel;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Options.Settings.VideoControlsItems;

public partial class VideoControlsItemDisplay : ObservableObject
{
    [ObservableProperty] private bool _isVisible;

    public string Name { get; }
    public string IconName { get; }
    public SeVideoControlsItemType Type { get; }

    public VideoControlsItemDisplay(SeVideoControlsItemType type, bool isVisible)
    {
        Type = type;
        Name = GetDisplayName(type);
        IconName = GetIconName(type);
        _isVisible = isVisible;
    }

    private static string GetDisplayName(SeVideoControlsItemType type)
    {
        var l = Se.Language.Options.Settings;
        return type switch
        {
            SeVideoControlsItemType.Play => Se.Language.General.Play,
            SeVideoControlsItemType.Stop => Se.Language.General.Stop,
            SeVideoControlsItemType.FullScreen => Se.Language.General.FullScreen,
            SeVideoControlsItemType.PositionSlider => Se.Language.General.VideoPosition,
            SeVideoControlsItemType.Volume => Se.Language.General.Volume,
            SeVideoControlsItemType.PositionText => l.VideoControlsPositionText,
            SeVideoControlsItemType.VideoFileName => l.VideoControlsVideoFileName,
            SeVideoControlsItemType.PlayerName => l.VideoControlsPlayerName,
            _ => type.ToString(),
        };
    }

    // Play, stop, full screen and volume use the same icons as the video controls themselves.
    private static string GetIconName(SeVideoControlsItemType type)
    {
        return type switch
        {
            SeVideoControlsItemType.Play => "fa-solid fa-play",
            SeVideoControlsItemType.Stop => "fa-solid fa-stop",
            SeVideoControlsItemType.FullScreen => "fa-solid fa-expand",
            SeVideoControlsItemType.PositionSlider => "fa-solid fa-sliders",
            SeVideoControlsItemType.Volume => "fa-solid fa-volume-up",
            SeVideoControlsItemType.PositionText => "fa-solid fa-clock",
            SeVideoControlsItemType.VideoFileName => "fa-solid fa-file-video",
            SeVideoControlsItemType.PlayerName => "fa-solid fa-circle-info",
            _ => "fa-solid fa-circle",
        };
    }

    // The list box item's screen-reader name falls back to ToString() (#12087).
    public override string ToString() => Name;
}
