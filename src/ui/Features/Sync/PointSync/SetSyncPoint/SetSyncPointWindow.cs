using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Nikse.SubtitleEdit.Controls;
using Nikse.SubtitleEdit.Controls.AudioVisualizerControl;
using Nikse.SubtitleEdit.Features.Main.Layout;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using System;

namespace Nikse.SubtitleEdit.Features.Sync.PointSync.SetSyncPoint;

public class SetSyncPointWindow : Window
{
    /// <summary>Room for the line picker, the time code and the two button rows - nothing else.</summary>
    private const double VideolessHeight = 340;
    private const double VideolessMinHeight = 340;

    public SetSyncPointWindow(SetSyncPointViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.Sync.SetSyncPoint;
        CanResize = true;
        vm.Window = this;
        DataContext = vm;

        // The view model is initialized before this constructor runs, so the dialog knows here
        // whether it has a video - and without one there is no player or waveform to make room
        // for, just the line picker and the sync point time code (issue #13341).
        Width = 1000;
        MinWidth = 800;
        Height = vm.IsVideoVisible ? 800 : VideolessHeight;
        MinHeight = vm.IsVideoVisible ? 650 : VideolessMinHeight;
        if (!vm.IsVideoVisible)
        {
            // Both modes save under the same window name, so RestoreWindowPosition would bring
            // back the with-video height and leave the videoless dialog mostly blank. The content
            // is fixed-height in this mode - cap the window instead of special-casing the restore.
            MaxHeight = VideolessHeight;
        }

        var labelVideoInfo = UiUtil.MakeLabel(string.Empty).WithBindText(vm, nameof(vm.VideoInfo));

        // Entering point sync without a video used to be a dead end - the sync point can be typed,
        // but there was no way to load a video from here (issue #13341).
        var buttonOpenVideo = UiUtil.MakeButton(Se.Language.General.OpenVideoFile, vm.OpenVideoFileCommand);

        var panelVideo = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 10,
            VerticalAlignment = VerticalAlignment.Center,
            Children =
            {
                buttonOpenVideo,
                labelVideoInfo,
            }
        };

        vm.VideoPlayerControl = InitVideoPlayer.MakeVideoPlayer();
        vm.VideoPlayerControl.FullScreenIsVisible = false;

        vm.AudioVisualizer = new AudioVisualizer
        {
            Height = 80,
            Width = double.NaN,
            IsReadOnly = true,
            DrawGridLines = Se.Settings.Waveform.DrawGridLines,
            WaveformColor = Se.Settings.Waveform.WaveformColor.FromHexToColor(),
            WaveformSelectedColor = Se.Settings.Waveform.WaveformSelectedColor.FromHexToColor(),
            InvertMouseWheel = Se.Settings.Waveform.InvertMouseWheel,
        };
        vm.AudioVisualizer.OnVideoPositionChanged += vm.AudioVisualizerLeftPositionChanged;
        vm.AudioVisualizer.OnPrimarySingleClicked += vm.AudioVisualizerOnPrimarySingleClicked;

        // IsAudioVisualizerVisible was set but never bound, so the waveform box was drawn as a
        // grey slab whether or not the main window had any peaks to lend it.
        vm.AudioVisualizer.WithBindIsVisible(nameof(vm.IsAudioVisualizerVisible));

        var comboBoxLeft = UiUtil.MakeComboBoxBindText(vm.Paragraphs, vm, nameof(SubtitleDisplayItem.Text), nameof(vm.SelectedParagraphIndex));
        comboBoxLeft.Width = double.NaN;
        comboBoxLeft.MinHeight = 50;
        comboBoxLeft.HorizontalAlignment = HorizontalAlignment.Stretch;
        vm.ComboBoxSubtitle = comboBoxLeft;

        vm.TimeCodeUpDownSyncPoint = new TimeCodeUpDown
        {
            VerticalAlignment = VerticalAlignment.Center,
            [!TimeCodeUpDown.ValueProperty] = new Binding(nameof(vm.SyncPointTimeCode))
            {
                Mode = BindingMode.TwoWay,
            },
        };
        // Not "Video position": this box is the sync point itself, and it is the only input when
        // the dialog has no video at all (issue #13341). SE4 labelled it the same way.
        AutomationProperties.SetName(vm.TimeCodeUpDownSyncPoint, Se.Language.Sync.SyncPointTimeCode);

        var panelTimeCode = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 10,
            Children =
            {
                UiUtil.MakeLabel(Se.Language.Sync.SyncPointTimeCode),
                vm.TimeCodeUpDownSyncPoint,
            }
        };

        // Playback is the one thing with no meaning when there is no video - the nudge buttons and
        // "Go to sub pos" still move the sync point time code, so they stay.
        var buttonPlayTwoSecondsAndBack = UiUtil
            .MakeButton(Se.Language.Sync.PlayTwoSecondsAndBack, vm.PlayTwoSecondsAndBackLeftCommand)
            .WithBindIsVisible(nameof(vm.IsVideoVisible));

        var panelLeftButtons = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Children =
            {
                UiUtil.MakeButton(vm.LeftOneSecondBackCommand, IconNames.ArrowLeftThick, Se.Language.General.OneSecondBack),
                buttonPlayTwoSecondsAndBack,
                UiUtil.MakeButton(vm.LeftOneSecondForwardCommand, IconNames.ArrowRightThick, Se.Language.General.OneSecondForward),
                UiUtil.MakeButton(Se.Language.Sync.GoToSubPos, vm.GoToLeftSubtitleCommand),
                UiUtil.MakeButton(Se.Language.Sync.FindText, vm.FindTextLeftCommand),
            }
        };

        var buttonOk = UiUtil.MakeButton(Se.Language.Sync.SetSyncPoint, vm.OkCommand);
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);
        var buttonPanel = UiUtil.MakeButtonBar(buttonOk, buttonCancel);

        // A Star row keeps its share of the height even when its child is hidden, so the row itself
        // has to collapse - otherwise the videoless dialog is mostly empty space.
        var rowVideoPlayer = new RowDefinition
        {
            Height = vm.IsVideoVisible ? new GridLength(1, GridUnitType.Star) : new GridLength(0),
        };

        vm.VideoPlayerControl.WithBindIsVisible(nameof(vm.IsVideoVisible));

        var gridLeft = new Grid
        {
            RowDefinitions =
            {
                rowVideoPlayer,                                                       // video player
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) }, // audio visualizer
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) }, // combo box
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) }, // sync point time code
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) }, // buttons
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
            ColumnSpacing = 10,
            RowSpacing = 10,
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        gridLeft.Add(vm.VideoPlayerControl, 0);
        gridLeft.Add(vm.AudioVisualizer, 1);
        gridLeft.Add(comboBoxLeft, 2);
        gridLeft.Add(panelTimeCode, 3);
        gridLeft.Add(panelLeftButtons, 4);

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) }, // video info
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) }, // video player etc. for left/right
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) }, // sync, ok, cancel buttons
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
            Margin = UiUtil.MakeWindowMargin(),
            ColumnSpacing = 10,
            RowSpacing = 10,
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        grid.Add(panelVideo, 0, 0, 1, 2);
        grid.Add(UiUtil.MakeBorderForControl(gridLeft), 1);
        grid.Add(buttonPanel, 2, 0, 1, 2);

        Content = grid;

        // "Open video file..." can bring a video in after the videoless dialog is already up: give
        // the player its row back and grow the window, which is otherwise too short to show it.
        vm.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName != nameof(vm.IsVideoVisible) || !vm.IsVideoVisible)
            {
                return;
            }

            rowVideoPlayer.Height = new GridLength(1, GridUnitType.Star);
            MaxHeight = double.PositiveInfinity;
            MinHeight = 650;
            Height = Math.Max(Height, 800);
        };

        // Focus the time code box, not a button, so the window receives key events without arming
        // any button: a focused button fires OnClick on bare Space, and "Set sync point" used to be
        // focused here - so the first Space a user pressed closed the dialog instead of playing.
        Activated += delegate { vm.FocusTimeCodeUpDown(); };

        AddHandler(KeyDownEvent, vm.OnKeyDownHandler, RoutingStrategies.Tunnel | RoutingStrategies.Bubble, handledEventsToo: false);

        // Avalonia's Button raises OnClick from OnKeyUp on Space whenever it is focused - it does
        // not check that it also saw the KeyDown - so handling Space in OnKeyDownHandler alone is
        // not enough to keep a focused button from clicking on Space release.
        AddHandler(KeyUpEvent, vm.OnKeyUpHandler, RoutingStrategies.Tunnel | RoutingStrategies.Bubble, handledEventsToo: true);
        Loaded += (_, e) => vm.OnLoaded();
        Closing += (_, e) => vm.OnClosing();
    }
}
