using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Layout;
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
        var panelVideo = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Children =
            {
                labelVideoInfo
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
        gridLeft.Add(panelLeftButtons, 3);

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

        Activated += delegate { buttonOk.Focus(); }; // hack to make OnKeyDown work

        AddHandler(KeyDownEvent, vm.OnKeyDownHandler, RoutingStrategies.Tunnel | RoutingStrategies.Bubble, handledEventsToo: false);
        Loaded += (_, e) => vm.OnLoaded();
        Closing += (_, e) => vm.OnClosing();
    }
}
