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

namespace Nikse.SubtitleEdit.Features.Sync.PointSync.SetSyncPoint;

public class SetSyncPointWindow : Window
{
    public SetSyncPointWindow(SetSyncPointViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.Sync.SetSyncPoint;
        CanResize = true;
        Width = 1000;
        Height = 800;
        MinWidth = 800;
        MinHeight = 650;
        vm.Window = this;
        DataContext = vm;

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
        AutomationProperties.SetName(vm.TimeCodeUpDownSyncPoint, Se.Language.General.VideoPosition);

        var panelTimeCode = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 10,
            Children =
            {
                UiUtil.MakeLabel(Se.Language.General.VideoPosition),
                vm.TimeCodeUpDownSyncPoint,
            }
        };

        var panelLeftButtons = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Children =
            {
                UiUtil.MakeButton(vm.LeftOneSecondBackCommand, IconNames.ArrowLeftThick, Se.Language.General.OneSecondBack),
                UiUtil.MakeButton(Se.Language.Sync.PlayTwoSecondsAndBack, vm.PlayTwoSecondsAndBackLeftCommand),
                UiUtil.MakeButton(vm.LeftOneSecondForwardCommand, IconNames.ArrowRightThick, Se.Language.General.OneSecondForward),
                UiUtil.MakeButton(Se.Language.Sync.GoToSubPos, vm.GoToLeftSubtitleCommand),
                UiUtil.MakeButton(Se.Language.Sync.FindText, vm.FindTextLeftCommand),
            }
        };

        var buttonOk = UiUtil.MakeButton(Se.Language.Sync.SetSyncPoint, vm.OkCommand);
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);
        var buttonPanel = UiUtil.MakeButtonBar(buttonOk, buttonCancel);

        var gridLeft = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) }, // video player
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
