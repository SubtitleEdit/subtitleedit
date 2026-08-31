using System;
using System.Collections.ObjectModel;
using System.Linq;
using Avalonia.Headless.XUnit;
using Nikse.SubtitleEdit.Features.Files.ExportImageBased;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Media;
using Nikse.SubtitleEdit.UiLogic.Export;

namespace UITests.Features.Files;

/// <summary>
/// The export dialog and the headless exporters (seconv, batch convert) share ExportTextTags,
/// whose "\pos" coordinates and "\bord"/"\shad" widths are in the script's own PlayResX/PlayResY
/// resolution. The dialog is handed the subtitle's header so it can scale them to the export
/// canvas like the other callers do - without it a 288-line script's {\bord2} rendered a fifth
/// of its intended width on a 1080-line canvas.
/// </summary>
public class ExportImageBasedScriptResolutionTests
{
    private const string Header288 = "[Script Info]\r\nPlayResX: 384\r\nPlayResY: 288\r\n";

    private sealed class NullServiceProvider : IServiceProvider
    {
        public object? GetService(Type serviceType) => null;
    }

    private static ExportImageBasedViewModel BuildViewModel(string? subtitleHeader)
    {
        var vm = new ExportImageBasedViewModel(
            new FileHelper(),
            new FolderHelper(),
            new WindowService(new NullServiceProvider()));

        var subtitles = new ObservableCollection<SubtitleLineViewModel>
        {
            new SubtitleLineViewModel
            {
                Number = 1,
                Text = "{\\bord2}Hello world",
                StartTime = TimeSpan.FromSeconds(1),
                EndTime = TimeSpan.FromSeconds(3),
            },
        };

        vm.Initialize(new ExportHandlerBluRaySup(), subtitles, null, null, subtitleHeader);
        vm.SelectedResolution = vm.Resolutions.First(r => r is { Width: 1920, Height: 1080 });
        return vm;
    }

    [AvaloniaFact]
    public void WithScriptHeader_BordWidthIsScaledToTheExportCanvas()
    {
        var vm = BuildViewModel(Header288);

        // 288 line script on a 1080 line canvas: 2 * 1080/288 = 7.5
        Assert.Equal(7.5, vm.GetImageParameter(0).OutlineWidth);
    }

    [AvaloniaFact]
    public void WithoutHeader_TagValuesAreUsedUnscaled()
    {
        var vm = BuildViewModel(null);

        Assert.Equal(2.0, vm.GetImageParameter(0).OutlineWidth);
    }
}
