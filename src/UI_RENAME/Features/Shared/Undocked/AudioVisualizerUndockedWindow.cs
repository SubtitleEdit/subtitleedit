using Avalonia.Controls;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Shared.Undocked;

public class AudioVisualizerUndockedWindow : Window
{
    public AudioVisualizerUndockedWindow(AudioVisualizerUndockedViewModel vm)
    {
        vm.Window = this;
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.General.AudioVisualizer; 
        MinWidth = 400;
        MinHeight = 100;
        Width = 800;
        Height = 200;
        CanResize = true;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        DataContext = vm;

        Loaded += vm.Onloaded;
        KeyDown += vm.OnKeyDown;
        KeyUp += vm.OnKeyUp;
        Closing += vm.OnClosing;
    }
}
