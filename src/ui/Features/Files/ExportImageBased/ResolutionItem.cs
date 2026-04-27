using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Files.ExportImageBased;

public partial class ResolutionItem : ObservableObject
{
    public string Name { get; set; }
    public string DisplayName { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }


    public ResolutionItem(string name)
    {
        DisplayName = string.Empty;
        Name = name;

        SetDisplayName(name);
    }

    public override string ToString()
    {
        return DisplayName;
    }

    private void SetDisplayName(string name)
    {
        if (Width > 0 && Height > 0)
        {
            if (string.IsNullOrEmpty(name))
            {
                DisplayName = $"{Width}x{Height}";
                return;
            }

            DisplayName = $"{name} - {Width}x{Height}";
            return;
        }
        
        DisplayName = name; 
    }

    public ResolutionItem(string name, int width, int height)
    {
        DisplayName = string.Empty;
        Name = name;
        Width = width;
        Height = height;
        SetDisplayName(name);
    }

    public static IEnumerable<ResolutionItem> GetResolutions()
    {
        yield return new ResolutionItem(Se.Language.General.PickResolutionFromVideoDotDotDot, 0, 0);
        
        yield return new ResolutionItem("4K DCI", 4096, 2160);
        yield return new ResolutionItem("4K UHD", 3840, 2160);
        yield return new ResolutionItem("2K WQHD", 2560, 1440);
        yield return new ResolutionItem("2K DCI", 2048, 1080);
        yield return new ResolutionItem("Full HD", 1920, 1080);
        yield return new ResolutionItem("HD 720p", 1280, 720);
        yield return new ResolutionItem("540p", 960, 540);
        yield return new ResolutionItem("SD PAL (4:3)", 720, 576);
        yield return new ResolutionItem("SD NTSC (3:2)", 720, 480);
        yield return new ResolutionItem("VGA (4:3)", 640, 480);
        yield return new ResolutionItem("360p", 640, 360);

        yield return new ResolutionItem("YouTube shorts", 1080, 1920);
        yield return new ResolutionItem("YouTube shorts", 720, 1280);
        yield return new ResolutionItem("1/2 A (9∶16)", 540, 960);
        yield return new ResolutionItem("1/2 B (9∶16)", 360, 540);
        yield return new ResolutionItem("1/4 A (9∶16)", 270, 480);
        yield return new ResolutionItem("1/4 B (9∶16)", 180, 270);
    }
}