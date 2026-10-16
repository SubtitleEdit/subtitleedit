using Avalonia.Controls;
using Avalonia.Layout;
using Nikse.SubtitleEdit.Features.Main.Layout;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Video.CutVideo;

/// <summary>
/// Preview a cut video transition, in the editor look (see <see cref="EditorChrome"/>).
/// </summary>
public class CutVideoPreviewWindow : Window
{
    public CutVideoPreviewWindow(CutVideoPreviewViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.Video.CutVideoPreviewTransitionTitle;
        CanResize = true;
        Width = 720;
        Height = 500;
        MinWidth = 400;
        MinHeight = 300;
        EditorChrome.ApplyWindow(this, EditorWindowStyles.Video);
        vm.Window = this;
        DataContext = vm;

        vm.VideoPlayer = InitVideoPlayer.MakeVideoPlayer();
        vm.VideoPlayer.FullScreenIsVisible = false;

        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        buttonOk.Classes.Add("primary");
        var buttonPanel = EditorChrome.MakeButtonBar(null, buttonOk);

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
            },
        };

        // The player in a plain border - no corner radius or clipping, the macOS native video
        // surface disappears inside either
        grid.Add(new Border
        {
            Child = EditorChrome.MakeVideoFrame(vm.VideoPlayer),
            Padding = new Avalonia.Thickness(12),
        }, 0, 0);
        grid.Add(buttonPanel, 1, 0);

        Content = grid;

        Loaded += (_, _) => vm.OnLoaded();
        Closing += (_, _) => vm.OnClosing();
        KeyDown += (_, e) => vm.OnKeyDown(e);
    }
}
