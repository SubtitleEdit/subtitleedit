using Avalonia.Controls;
using Avalonia.Data;
using Nikse.SubtitleEdit.Logic;

namespace Nikse.SubtitleEdit.Features.Files.ExportImageBased;

public class ImageBasedPreviewWindow : Window
{
    public ImageBasedPreviewWindow(ImageBasedPreviewViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Bind(Window.TitleProperty, new Binding(nameof(vm.Title)) { Mode = BindingMode.TwoWay });
        Width = 1060;
        Height = 700;
        CanResize = true;
        MinWidth = 720;
        MinHeight = 480;
        vm.Window = this;
        DataContext = vm;

        var image = new Image
        {
            [!Image.SourceProperty] = new Binding(nameof(vm.BitmapPreview)) { Mode = BindingMode.TwoWay },
        };
        vm.ImagePreview = image;

        Content = image;

        Activated += delegate { Focus(); }; // hack to make OnKeyDown work
        KeyDown += (_, e) => vm.OnKeyDown(e);
    }
}