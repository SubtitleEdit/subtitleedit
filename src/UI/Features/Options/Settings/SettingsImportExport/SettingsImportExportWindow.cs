using Avalonia.Controls;
using Nikse.SubtitleEdit.Logic;

namespace Nikse.SubtitleEdit.Features.Options.Settings.SettingsImportExport;

public class SettingsImportExportWindow : Window
{
    public SettingsImportExportWindow(SettingsImportExportViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        CanResize = false;
        SizeToContent = SizeToContent.WidthAndHeight;
        vm.Window = this;
        DataContext = vm;
        Content = new SettingsImportExportPage(vm);
        Title = vm.TitleText;
        Loaded += vm.OnLoaded;
        KeyDown += vm.KeyDown;
    }
}
