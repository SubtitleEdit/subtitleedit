using System;
using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.UiLogic.Export;

namespace Nikse.SubtitleEdit.Features.Files.ExportImageBased;

public partial class ResolutionItem : ObservableObject
{
    public string Name { get; set; }
    public string DisplayName { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }

    /// <summary>
    /// Sample (pixel) aspect ratio like "64:45" (see <see cref="ExportPixelAspect"/>); empty for
    /// square pixels.
    /// </summary>
    public string PixelAspect { get; set; } = string.Empty;

    public double PixelAspectRatio => ExportPixelAspect.Parse(PixelAspect);

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

    public ResolutionItem(string name, int width, int height, string pixelAspect = "")
    {
        DisplayName = string.Empty;
        Name = name;
        Width = width;
        Height = height;
        PixelAspect = pixelAspect;
        SetDisplayName(name);
    }

    public bool Matches(int width, int height, string? pixelAspect)
    {
        return Width == width && Height == height &&
               Math.Abs(PixelAspectRatio - ExportPixelAspect.Parse(pixelAspect)) < 0.001;
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
        // DVD/SD Blu-ray: stored at 720x576/720x480 with non-square pixels - the player
        // stretches them to 4:3 or 16:9, so the images are pre-squeezed (doom9 p=2052176).
        yield return new ResolutionItem("PAL 16:9 (anamorphic)", 720, 576, ExportPixelAspect.Pal16x9);
        yield return new ResolutionItem("PAL 4:3", 720, 576, ExportPixelAspect.Pal4x3);
        yield return new ResolutionItem("NTSC 16:9 (anamorphic)", 720, 480, ExportPixelAspect.Ntsc16x9);
        yield return new ResolutionItem("NTSC 4:3", 720, 480, ExportPixelAspect.Ntsc4x3);
        yield return new ResolutionItem("SD PAL (square pixels)", 720, 576);
        yield return new ResolutionItem("SD NTSC (square pixels)", 720, 480);
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