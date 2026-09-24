using Avalonia.Controls;
using Avalonia.Layout;
using Nikse.SubtitleEdit.Features.Main.Layout;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Video.CutVideo;

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
        vm.Window = this;
        DataContext = vm;

        vm.VideoPlayer = InitVideoPlayer.MakeVideoPlayer();
        vm.VideoPlayer.FullScreenIsVisible = false;

        var buttonPanel = UiUtil.MakeButtonBar(UiUtil.MakeButtonOk(vm.OkCommand));

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
            },
            Margin = UiUtil.MakeWindowMargin(),
            HorizontalAlignment = HorizontalAlignment.Stretch,
            RowSpacing = 5,
        };

        grid.Add(UiUtil.MakeBorderForControl(vm.VideoPlayer), 0, 0);
        grid.Add(buttonPanel, 1, 0);

        Content = grid;

        Loaded += (_, _) => vm.OnLoaded();
        Closing += (_, _) => vm.OnClosing();
        Closed += (_, _) => vm.OnClosed();
        KeyDown += (_, e) => vm.OnKeyDown(e);
    }
}
