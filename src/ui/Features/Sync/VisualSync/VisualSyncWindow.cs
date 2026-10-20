using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Nikse.SubtitleEdit.Controls;
using Nikse.SubtitleEdit.Controls.AudioVisualizerControl;
using Nikse.SubtitleEdit.Features.Main.Layout;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Sync.VisualSync;

/// <summary>
/// Visual sync, in the editor look (see <see cref="EditorChrome"/>): the video on top, the start
/// and end scene side by side (video over waveform, line picker, step buttons), the sync result,
/// Sync, Cancel and OK at the bottom.
/// </summary>
public class VisualSyncWindow : Window
{
    public VisualSyncWindow(VisualSyncViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = UiUtil.MakeWindowTitle(Se.Language.Sync.VisualSync);
        CanResize = true;
        Width = 1100;
        Height = 700;
        MinWidth = 900;
        MinHeight = 650;
        Background = EditorChrome.WindowBrush;
        EditorChrome.AddEditorStyles(Styles);
        vm.Window = this;
        DataContext = vm;

        vm.VideoPlayerControlLeft = InitVideoPlayer.MakeVideoPlayer();
        vm.VideoPlayerControlLeft.FullScreenIsVisible = false;
        AutomationProperties.SetName(vm.VideoPlayerControlLeft, Se.Language.Sync.StartScene);

        vm.VideoPlayerControlRight = InitVideoPlayer.MakeVideoPlayer();
        vm.VideoPlayerControlRight.FullScreenIsVisible = false;
        AutomationProperties.SetName(vm.VideoPlayerControlRight, Se.Language.Sync.EndScene);

        vm.AudioVisualizerLeft = MakeAudioVisualizer();
        vm.AudioVisualizerLeft.OnVideoPositionChanged += vm.AudioVisualizerLeftPositionChanged;
        vm.AudioVisualizerLeft.OnPrimarySingleClicked += vm.AudioVisualizerLeft_OnPrimarySingleClicked;

        // IsAudioVisualizerVisible was set but never bound, so the waveform box was drawn as a
        // grey slab whether or not the main window had any peaks to lend it.
        vm.AudioVisualizerLeft.WithBindIsVisible(nameof(vm.IsAudioVisualizerVisible));

        vm.AudioVisualizerRight = MakeAudioVisualizer();
        vm.AudioVisualizerRight.OnVideoPositionChanged += vm.AudioVisualizerRightPositionChanged;
        vm.AudioVisualizerRight.OnPrimarySingleClicked += vm.AudioVisualizerRight_OnPrimarySingleClicked;
        vm.AudioVisualizerRight.WithBindIsVisible(nameof(vm.IsAudioVisualizerVisible));

        var comboBoxLeft = MakeLineComboBox(vm, nameof(vm.SelectedParagraphLeftIndex), Se.Language.Sync.StartScene);
        vm.ComboBoxLeft = comboBoxLeft;

        var comboBoxRight = MakeLineComboBox(vm, nameof(vm.SelectedParagraphRightIndex), Se.Language.Sync.EndScene);
        vm.ComboBoxRight = comboBoxRight;

        // The waveforms used to be pinned at 80 px under the players, too little to pick a precise
        // scene from; the drag handles mirror the main window and move together (issue #14414).
        var splitLeft = new VideoWaveformSplitGrid(vm.VideoPlayerControlLeft, vm.AudioVisualizerLeft, Se.Settings.Synchronization.VisualSyncWaveformHeight)
        {
            IsWaveformVisible = vm.IsAudioVisualizerVisible,
        };
        var splitRight = new VideoWaveformSplitGrid(vm.VideoPlayerControlRight, vm.AudioVisualizerRight, Se.Settings.Synchronization.VisualSyncWaveformHeight)
        {
            IsWaveformVisible = vm.IsAudioVisualizerVisible,
        };
        splitLeft.WaveformHeightChanged += splitRight.SetWaveformHeight;
        splitRight.WaveformHeightChanged += splitLeft.SetWaveformHeight;

        var gridLeft = MakePane(Se.Language.Sync.StartScene, splitLeft, comboBoxLeft, MakeStepButtons(
            vm.LeftOneSecondBackCommand,
            vm.PlayTwoSecondsAndBackLeftCommand,
            vm.LeftOneSecondForwardCommand,
            vm.GoToLeftSubtitleCommand,
            vm.FindTextLeftCommand));
        var gridRight = MakePane(Se.Language.Sync.EndScene, splitRight, comboBoxRight, MakeStepButtons(
            vm.RightOneSecondBackCommand,
            vm.PlayTwoSecondsAndBackRightCommand,
            vm.RightOneSecondForwardCommand,
            vm.GoToRightSubtitleCommand,
            vm.FindTextRightCommand));

        var panes = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
        };
        panes.Add(gridLeft, 0, 0);
        panes.Add(new Border
        {
            Child = gridRight,
            BorderBrush = EditorChrome.LineBrush,
            BorderThickness = new Thickness(1, 0, 0, 0),
        }, 0, 1);

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) }, // open video, video info
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) }, // video player etc. for left/right
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) }, // sync info, sync, ok, cancel buttons
            },
        };

        grid.Add(MakeToolbar(vm), 0, 0);
        grid.Add(panes, 1, 0);
        grid.Add(MakeButtonBar(vm), 2, 0);

        // Seek/play keys follow the pane last clicked or focused once focus leaves both (#15789).
        vm.PaneLeft = gridLeft;
        vm.PaneRight = gridRight;
        gridLeft.AddHandler(PointerPressedEvent, (_, _) => vm.IsRightPaneActive = false, RoutingStrategies.Tunnel, handledEventsToo: true);
        gridRight.AddHandler(PointerPressedEvent, (_, _) => vm.IsRightPaneActive = true, RoutingStrategies.Tunnel, handledEventsToo: true);
        gridLeft.GotFocus += (_, _) => vm.IsRightPaneActive = false;
        gridRight.GotFocus += (_, _) => vm.IsRightPaneActive = true;

        Content = grid;

        // The lent waveform arrives a beat after construction, and goes away again when a
        // different video is opened from the dialog.
        vm.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(vm.IsAudioVisualizerVisible))
            {
                splitLeft.IsWaveformVisible = vm.IsAudioVisualizerVisible;
                splitRight.IsWaveformVisible = vm.IsAudioVisualizerVisible;
            }
        };

        UiUtil.FocusOnFirstActivation(this, comboBoxLeft); // initial focus on an input, not an action button - a focused button clicks on bare Space

        AddHandler(KeyDownEvent, vm.OnKeyDownHandler, RoutingStrategies.Tunnel | RoutingStrategies.Bubble, handledEventsToo: false);
        Loaded += (_, _) => vm.OnLoaded();
        Closing += (_, e) =>
        {
            Se.Settings.Synchronization.VisualSyncWaveformHeight = splitLeft.WaveformHeight;
            vm.OnClosing();
        };
    }

    private static AudioVisualizer MakeAudioVisualizer()
    {
        var audioVisualizer = new AudioVisualizer
        {
            Width = double.NaN,
            IsReadOnly = true,
            DrawGridLines = Se.Settings.Waveform.DrawGridLines,
            WaveformColor = Se.Settings.Waveform.WaveformColor.FromHexToColor(),
            WaveformSelectedColor = Se.Settings.Waveform.WaveformSelectedColor.FromHexToColor(),
            WaveformGridColor = Se.Settings.Waveform.WaveformGridColor.FromHexToColor(),
            InvertMouseWheel = Se.Settings.Waveform.InvertMouseWheel,
        };
        AutomationProperties.SetName(audioVisualizer, Se.Language.General.Waveform);
        return audioVisualizer;
    }

    private static ComboBox MakeLineComboBox(VisualSyncViewModel vm, string selectedIndexProperty, string name)
    {
        var comboBox = UiUtil.MakeComboBoxBindText(vm.Paragraphs, vm, nameof(SubtitleDisplayItem.Text), selectedIndexProperty);
        comboBox.Width = double.NaN;
        comboBox.MinHeight = 50;
        comboBox.FontSize = 12.5;
        comboBox.CornerRadius = new CornerRadius(6);
        comboBox.HorizontalAlignment = HorizontalAlignment.Stretch;
        AutomationProperties.SetName(comboBox, name);
        return comboBox;
    }

    private static WrapPanel MakeStepButtons(
        System.Windows.Input.ICommand oneSecondBack,
        System.Windows.Input.ICommand playTwoSecondsAndBack,
        System.Windows.Input.ICommand oneSecondForward,
        System.Windows.Input.ICommand goToSubtitle,
        System.Windows.Input.ICommand findText)
    {
        // Wraps rather than clipping when the pane is narrow or the texts are long (translations)
        var panel = new WrapPanel { Orientation = Orientation.Horizontal, ItemSpacing = 2, LineSpacing = 2 };
        panel.Children.Add(EditorChrome.CreateToolButton("fa-solid fa-arrow-left", Se.Language.General.OneSecondBack, oneSecondBack, 28));
        panel.Children.Add(EditorChrome.MakeTextToolButton("fa-solid fa-play", Se.Language.Sync.PlayTwoSecondsAndBack, playTwoSecondsAndBack));
        panel.Children.Add(EditorChrome.CreateToolButton("fa-solid fa-arrow-right", Se.Language.General.OneSecondForward, oneSecondForward, 28));
        var separator = EditorChrome.MakeToolbarSeparator();
        separator.Margin = new Thickness(4, 0);
        panel.Children.Add(separator);
        panel.Children.Add(EditorChrome.MakeTextToolButton("fa-solid fa-location-crosshairs", Se.Language.Sync.GoToSubPos, goToSubtitle));
        panel.Children.Add(EditorChrome.MakeTextToolButton("fa-solid fa-magnifying-glass", Se.Language.Sync.FindText, findText));
        return panel;
    }

    /// <summary>
    /// One scene: header, video over waveform, the line picker and the step buttons.
    /// </summary>
    private static Grid MakePane(string title, Control videoAndWaveform, ComboBox comboBox, Control buttons)
    {
        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) }, // header
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) }, // video player over waveform
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) }, // combo box
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) }, // buttons
            },
            RowSpacing = 10,
            Margin = new Thickness(14, 12, 14, 12),
            // Hit-testable over its whole area, so a click anywhere in the pane makes it the active one
            Background = Brushes.Transparent,
        };

        // A plain border: the native video surface on macOS disappears under a rounded, clipping one
        var videoBorder = new Border
        {
            Child = videoAndWaveform,
            BorderBrush = EditorChrome.LineBrush,
            BorderThickness = new Thickness(1),
        };

        grid.Add(EditorChrome.MakeSectionHeader(title), 0, 0);
        grid.Add(videoBorder, 1, 0);
        grid.Add(comboBox, 2, 0);
        grid.Add(buttons, 3, 0);
        return grid;
    }

    /// <summary>
    /// Open a video and the video info.
    /// </summary>
    private static Border MakeToolbar(VisualSyncViewModel vm)
    {
        // Entering visual sync without a video used to be a dead end - two blank players and no
        // way to load one from here. SE4 had the same button.
        var buttonOpenVideo = EditorChrome.MakeTextToolButton("fa-solid fa-film", Se.Language.General.OpenVideoFile, vm.OpenVideoFileCommand);

        var labelVideoInfo = EditorChrome.MakeDimLabel(string.Empty, new Thickness(0)).WithBindText(vm, nameof(vm.VideoInfo));
        labelVideoInfo.TextTrimming = TextTrimming.CharacterEllipsis;

        var panel = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 2, VerticalAlignment = VerticalAlignment.Center };
        panel.Children.Add(buttonOpenVideo);
        panel.Children.Add(EditorChrome.MakeToolbarSeparator());
        panel.Children.Add(labelVideoInfo);

        return new Border
        {
            Child = panel,
            Background = EditorChrome.PanelBrush,
            BorderBrush = EditorChrome.LineBrush,
            BorderThickness = new Thickness(0, 0, 0, 1),
            Padding = new Thickness(10, 6),
        };
    }

    /// <summary>
    /// The result of the last sync on the left; Sync (with manual sync), Cancel and OK on the right.
    /// </summary>
    private static Border MakeButtonBar(VisualSyncViewModel vm)
    {
        var labelInfo = EditorChrome.MakeDimLabel(string.Empty, new Thickness(0)).WithBindText(vm, nameof(vm.AdjustInfo));
        labelInfo.TextTrimming = TextTrimming.CharacterEllipsis;

        var buttonSync = new SeSplitButton
        {
            Content = Se.Language.General.Sync,
            Command = vm.SyncCommand,
            Flyout = new MenuFlyout
            {
                Items =
                {
                    new MenuItem
                    {
                        Header = Se.Language.General.ManualSync,
                        Command = vm.ManualSyncCommand,
                    },
                },
            },
        };
        AutomationProperties.SetName(buttonSync, Se.Language.General.Sync);
        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        buttonOk.Classes.Add("primary");
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);

        var buttons = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 10, VerticalAlignment = VerticalAlignment.Center };
        buttons.Children.Add(buttonSync);
        buttons.Children.Add(EditorChrome.MakeToolbarSeparator());
        buttons.Children.Add(buttonCancel);
        buttons.Children.Add(buttonOk);

        var bar = new DockPanel();
        DockPanel.SetDock(buttons, Dock.Right);
        bar.Children.Add(buttons);
        bar.Children.Add(labelInfo);

        return EditorChrome.WrapButtonBar(bar);
    }
}
