using Avalonia.Controls;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Shared.BinaryEdit;

public static class InitNativeMacMenuBinaryEdit
{
    public static void Setup(Window window, BinaryEditViewModel vm)
    {
        var root = new NativeMenu();
        var l = Se.Language.Main.Menu;

        // File menu
        var fileMenu = new NativeMenu();
        Add(fileMenu, l.Open, vm.FileOpenCommand);
        var exportMenu = new NativeMenu();
        Add(exportMenu, Se.Language.General.BluRaySup, vm.ExportBluRaySupCommand);
        Add(exportMenu, Se.Language.General.BdnXml, vm.ExportBdnXmlCommand);
        Add(exportMenu, "DOST/png", vm.ExportDostPngCommand);
        Add(exportMenu, "FCP/png", vm.ExportFcpPngCommand);
        Add(exportMenu, Se.Language.General.ImagesWithTimeCode, vm.ExportImagesWithTimeCodeCommand);
        Add(exportMenu, Se.Language.File.Export.TitleExportVobSub, vm.ExportVobSubCommand);
        Add(exportMenu, "WebVTT png", vm.ExportWebVttThumbnailCommand);
        fileMenu.Items.Add(new NativeMenuItem(Clean(l.Export)) { Menu = exportMenu });
        fileMenu.Items.Add(new NativeMenuItemSeparator());
        Add(fileMenu, Se.Language.Tools.ImageBasedEdit.ImportTimeCodes, vm.ImportTimeCodesCommand);
        fileMenu.Items.Add(new NativeMenuItemSeparator());
        Add(fileMenu, l.Exit, vm.CancelCommand);
        root.Items.Add(new NativeMenuItem(Clean(l.File)) { Menu = fileMenu });

        // Tools menu
        var toolsMenu = new NativeMenu();
        Add(toolsMenu, l.AdjustDurations, vm.AdjustDurationsCommand);
        Add(toolsMenu, l.ApplyDurationLimits, vm.ApplyDurationLimitsCommand);
        Add(toolsMenu, Se.Language.General.AlignmentDotDotDot, vm.AlignmentCommand);
        Add(toolsMenu, Se.Language.Tools.ImageBasedEdit.ResizeImagesDotDotDot, vm.ResizeImagesCommand);
        Add(toolsMenu, Se.Language.Tools.ImageBasedEdit.AdjustBrightnessDotDotDot, vm.AdjustBrightnessCommand);
        Add(toolsMenu, Se.Language.Tools.ImageBasedEdit.AdjustAlphaDotDotDot, vm.AdjustAlphaCommand);
        Add(toolsMenu, Se.Language.Tools.ImageBasedEdit.CenterHorizontally, vm.CenterHorizontallyCommand);
        Add(toolsMenu, Se.Language.Tools.ImageBasedEdit.CropImages, vm.CropCommand);
        root.Items.Add(new NativeMenuItem(Clean(l.ToolsSelectedLines)) { Menu = toolsMenu });

        // Synchronization menu
        var syncMenu = new NativeMenu();
        Add(syncMenu, l.AdjustAllTimes, vm.AdjustAllTimesCommand);
        Add(syncMenu, l.ChangeFrameRate, vm.ChangeFrameRateCommand);
        Add(syncMenu, l.ChangeSpeed, vm.ChangeSpeedCommand);
        root.Items.Add(new NativeMenuItem(Clean(l.Synchronization)) { Menu = syncMenu });

        // Video menu
        var videoMenu = new NativeMenu();
        Add(videoMenu, l.OpenVideo, vm.OpenVideoCommand);
        root.Items.Add(new NativeMenuItem(Clean(l.Video)) { Menu = videoMenu });

        // Options menu
        var optionsMenu = new NativeMenu();
        Add(optionsMenu, l.Settings, vm.SettingsCommand);
        root.Items.Add(new NativeMenuItem(Clean(l.Options)) { Menu = optionsMenu });

        NativeMenu.SetMenu(window, root);
    }

    private static void Add(NativeMenu menu, string header, IRelayCommand command)
    {
        var item = new NativeMenuItem(Clean(header));
        item.Click += (_, _) => command.Execute(null);
        menu.Items.Add(item);
    }

    private static string Clean(string? s) => s?.Replace("_", string.Empty) ?? string.Empty;
}
