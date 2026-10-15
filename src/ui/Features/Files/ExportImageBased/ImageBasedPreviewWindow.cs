using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Data;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Files.ExportImageBased;

/// <summary>
/// The exported image of one line, full size in its frame (see <see cref="ImageBasedPreviewViewModel.Initialize"/>),
/// on the editor window color (see <see cref="EditorChrome"/>).
/// </summary>
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
        Background = EditorChrome.WindowBrush; // around the frame; the frame itself is drawn on a checkerboard
        EditorChrome.AddEditorStyles(Styles);
        vm.Window = this;
        DataContext = vm;

        var image = new Image
        {
            [!Image.SourceProperty] = new Binding(nameof(vm.BitmapPreview)) { Mode = BindingMode.OneWay },
        };
        AutomationProperties.SetName(image, Se.Language.General.Preview);
        vm.ImagePreview = image;

        Content = image;

        UiUtil.FocusOnFirstActivation(this, () => { Focus(); }); // hack to make OnKeyDown work
        KeyDown += (_, e) => vm.OnKeyDown(e);
    }
}