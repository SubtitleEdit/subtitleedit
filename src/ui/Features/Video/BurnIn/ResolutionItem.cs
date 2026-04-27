using System.Collections.Generic;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Video.BurnIn;

public partial class ResolutionItem : ObservableObject
{
    public ResolutionItemType ItemType { get; set; }
    public string Name { get; set; }
    public string DisplayName { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public bool IsSeparator => ItemType == ResolutionItemType.Separator;

    [ObservableProperty] private Color _backgroundColor;

    [ObservableProperty] private Color _textColor;

    public ResolutionItem(string name, ResolutionItemType itemType)
    {
        DisplayName = string.Empty;
        Name = name;
        ItemType = itemType;
        BackgroundColor = Colors.Transparent;
        SetDisplayName(name, itemType);
    }

    private void SetDisplayName(string name, ResolutionItemType itemType)
    {
        DisplayName = itemType == ResolutionItemType.Resolution ? $"{name} - {Width}x{Height}" : name;
    }

    public ResolutionItem(string name, int width, int height)
    {
        DisplayName = string.Empty;
        ItemType = ResolutionItemType.Resolution;
        Name = name;
        Width = width;
        Height = height;
        BackgroundColor = Colors.Transparent;
        SetDisplayName(name, ResolutionItemType.Resolution);
    }


    public static IEnumerable<ResolutionItem> GetResolutions()
    {
        yield return new ResolutionItem( Se.Language.General.UseSourceResolution, ResolutionItemType.UseSource);
        yield return new ResolutionItem( Se.Language.General.PickResolutionFromVideoDotDotDot, ResolutionItemType.PickResolution);
        yield return new ResolutionItem("Landscape modes", ResolutionItemType.Separator);
        yield return new ResolutionItem("4K DCI - Aspect Ratio 16∶9", 4096, 2160);
        yield return new ResolutionItem("4K UHD - Aspect Ratio 16∶9", 3840, 2160);
        yield return new ResolutionItem("2K WQHD - Aspect Ratio 16∶9", 2560, 1440);
        yield return new ResolutionItem("2K DCI - Aspect Ratio 16∶9", 2048, 1080);
        yield return new ResolutionItem("Full HD 1080p - Aspect Ratio 16∶9", 1920, 1080);
        yield return new ResolutionItem("HD 720p - Aspect Ratio 16∶9", 1280, 720);
        yield return new ResolutionItem("540p - Aspect Ratio 16∶9", 960, 540);
        yield return new ResolutionItem("SD PAL - Aspect Ratio 4:3", 720, 576);
        yield return new ResolutionItem("SD NTSC - Aspect Ratio 3:2", 720, 480);
        yield return new ResolutionItem("VGA - Aspect Ratio 4:3", 640, 480);
        yield return new ResolutionItem("360p - Aspect Ratio 16∶9", 640, 360);
        yield return new ResolutionItem("Portrait modes", ResolutionItemType.Separator);
        yield return new ResolutionItem("YouTube shorts/TikTok - Aspect Ratio 9∶16", 1080, 1920);
        yield return new ResolutionItem("YouTube shorts/TikTok - Aspect Ratio 9∶16", 720, 1280);
        yield return new ResolutionItem("1/2 A - Aspect Ratio 9∶16", 540, 960);
        yield return new ResolutionItem("1/2 B - Aspect Ratio 9∶16", 360, 540);
        yield return new ResolutionItem("1/4 A - Aspect Ratio 9∶16", 270, 480);
        yield return new ResolutionItem("1/4 B - Aspect Ratio 9∶16 - (180x270)", 180, 270);
    }
}