using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Features.Main.Layout;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Video.BurnIn;

/// <summary>
/// Pick a position in the video, in the editor look (see <see cref="EditorChrome"/>).
/// </summary>
public class SelectVideoPositionWindow : Window
{
    private readonly SelectVideoPositionViewModel _vm;

    public SelectVideoPositionWindow(SelectVideoPositionViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.General.PickVideoPosition;
        Width = 1280;
        Height = 800;
        MinWidth = 800;
        MinHeight = 500;
        CanResize = true;
        EditorChrome.ApplyWindow(this, EditorWindowStyles.Video);

        _vm = vm;
        vm.Window = this;
        DataContext = vm;

        vm.VideoPlayerControl = InitVideoPlayer.MakeVideoPlayer();
        vm.VideoPlayerControl.Width = double.NaN;
        vm.VideoPlayerControl.Height = double.NaN;
        vm.VideoPlayerControl.HorizontalAlignment = HorizontalAlignment.Stretch;
        vm.VideoPlayerControl.VerticalAlignment = VerticalAlignment.Stretch;
        // Without a command the player's fullscreen button did nothing - maximize this window instead.
        vm.VideoPlayerControl.FullScreenCommand = new RelayCommand(() =>
            WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized);

        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        buttonOk.Classes.Add("primary");
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);
        var panelButtons = EditorChrome.MakeButtonBar(null, buttonCancel, buttonOk);

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
            Child = EditorChrome.MakeVideoFrame(vm.VideoPlayerControl),
            Padding = new Avalonia.Thickness(12),
        }, 0, 0);
        grid.Add(panelButtons, 1, 0);

        Content = grid;

        UiUtil.FocusOnFirstActivation(this, buttonCancel); // initial focus on an input, not an action button - a focused button clicks on bare Space
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        _vm.OnLoaded();
    }

    protected override void OnClosing(WindowClosingEventArgs e)
    {
        base.OnClosing(e);
        _vm.VideoPlayerControl?.CloseAndDisposePlayer();
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        _vm.OnKeyDown(e);
    }
}