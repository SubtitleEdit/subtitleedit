using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Nikse.SubtitleEdit.Controls.AudioVisualizerControl;
using Nikse.SubtitleEdit.Features.Main.Layout;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Sync.VisualSync;

public class VisualSyncWindow : Window
{
    public VisualSyncWindow(VisualSyncViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.Sync.VisualSync;
        CanResize = true;
        Width = 1100;
        Height = 700;
        MinWidth = 900;
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

        vm.VideoPlayerControlLeft = InitVideoPlayer.MakeVideoPlayer();
        vm.VideoPlayerControlLeft.FullScreenIsVisible = false;

        vm.VideoPlayerControlRight = InitVideoPlayer.MakeVideoPlayer();
        vm.VideoPlayerControlRight.FullScreenIsVisible = false;

        vm.AudioVisualizerLeft = new AudioVisualizer
        {
            Height = 80,
            Width = double.NaN,
            IsReadOnly = true,
            DrawGridLines = Se.Settings.Waveform.DrawGridLines,
            WaveformColor = Se.Settings.Waveform.WaveformColor.FromHexToColor(),
            WaveformSelectedColor = Se.Settings.Waveform.WaveformSelectedColor.FromHexToColor(),
            InvertMouseWheel = Se.Settings.Waveform.InvertMouseWheel,
        };
        vm.AudioVisualizerLeft.OnVideoPositionChanged += vm.AudioVisualizerLeftPositionChanged;
        vm.AudioVisualizerLeft.OnPrimarySingleClicked += vm.AudioVisualizerLeft_OnPrimarySingleClicked;

        vm.AudioVisualizerRight = new AudioVisualizer
        {
            Height = 80,
            Width = double.NaN,
            IsReadOnly = true,
            DrawGridLines = Se.Settings.Waveform.DrawGridLines,
            WaveformColor = Se.Settings.Waveform.WaveformColor.FromHexToColor(),
            WaveformSelectedColor = Se.Settings.Waveform.WaveformSelectedColor.FromHexToColor(),
            InvertMouseWheel = Se.Settings.Waveform.InvertMouseWheel,
        };
        vm.AudioVisualizerRight.OnVideoPositionChanged += vm.AudioVisualizerRightPositionChanged;
        vm.AudioVisualizerRight.OnPrimarySingleClicked += vm.AudioVisualizerRight_OnPrimarySingleClicked;

        var comboBoxLeft = UiUtil.MakeComboBoxBindText(vm.Paragraphs, vm, nameof(SubtitleDisplayItem.Text), nameof(vm.SelectedParagraphLeftIndex));
        comboBoxLeft.Width = double.NaN;
        comboBoxLeft.MinHeight = 50;
        comboBoxLeft.HorizontalAlignment = HorizontalAlignment.Stretch;
        vm.ComboBoxLeft = comboBoxLeft;

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

        var comboBoxRight = UiUtil.MakeComboBoxBindText(vm.Paragraphs, vm, nameof(SubtitleDisplayItem.Text), nameof(vm.SelectedParagraphRightIndex));
        comboBoxRight.Width = double.NaN;
        comboBoxRight.MinHeight = 50;
        comboBoxRight.HorizontalAlignment = HorizontalAlignment.Stretch;
        vm.ComboBoxRight = comboBoxRight;

        var panelRightButtons = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Children =
            {
                UiUtil.MakeButton(vm.RightOneSecondBackCommand, IconNames.ArrowLeftThick, Se.Language.General.OneSecondBack),
                UiUtil.MakeButton(Se.Language.Sync.PlayTwoSecondsAndBack, vm.PlayTwoSecondsAndBackRightCommand),
                UiUtil.MakeButton(vm.RightOneSecondForwardCommand, IconNames.ArrowRightThick, Se.Language.General.OneSecondForward),
                UiUtil.MakeButton(Se.Language.Sync.GoToSubPos, vm.GoToRightSubtitleCommand),
                UiUtil.MakeButton(Se.Language.Sync.FindText, vm.FindTextRightCommand),
            }
        };

        var labelInfo = UiUtil.MakeLabel(string.Empty).WithBindText(vm, nameof(vm.AdjustInfo));
        var buttonSync = UiUtil.MakeButton(Se.Language.Sync.Sync, vm.SyncCommand);
        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);
        var buttonPanel = UiUtil.MakeButtonBar(labelInfo, buttonSync, buttonOk, buttonCancel);

        var gridLeft = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) }, // label
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

        gridLeft.Add(UiUtil.MakeLabel(Se.Language.Sync.StartScene), 0);
        gridLeft.Add(vm.VideoPlayerControlLeft, 1);
        gridLeft.Add(vm.AudioVisualizerLeft, 2);
        gridLeft.Add(comboBoxLeft, 3);
        gridLeft.Add(panelLeftButtons, 4);

        var gridRight = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) }, // label
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

        gridRight.Add(UiUtil.MakeLabel(Se.Language.Sync.EndScene), 0);
        gridRight.Add(vm.VideoPlayerControlRight, 1);
        gridRight.Add(vm.AudioVisualizerRight, 2);
        gridRight.Add(comboBoxRight, 3);
        gridRight.Add(panelRightButtons, 4);

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
        grid.Add(UiUtil.MakeBorderForControl(gridRight), 1, 1);
        grid.Add(buttonPanel, 2, 0, 1, 2);

        Content = grid;

        Activated += delegate { buttonOk.Focus(); }; // hack to make OnKeyDown work

        AddHandler(KeyDownEvent, vm.OnKeyDownHandler, RoutingStrategies.Tunnel | RoutingStrategies.Bubble, handledEventsToo: false);
        Loaded += (_, _) => vm.OnLoaded();
        Closing += (_, e) => vm.OnClosing();
    }
}
