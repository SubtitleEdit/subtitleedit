using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Styling;
using Nikse.SubtitleEdit.Controls;
using Nikse.SubtitleEdit.Controls.AudioVisualizerControl;
using Nikse.SubtitleEdit.Controls.VideoPlayer;
using Nikse.SubtitleEdit.Features.Main.Layout;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using System;

namespace Nikse.SubtitleEdit.Features.Sync.PointSync.SetSyncPoint;

public class SetSyncPointWindow : Window
{
    /// <summary>Room for the toolbar, the line picker, the time code row and the button bar - nothing else.</summary>
    private const double VideolessHeight = 220;
    private const double VideolessMinHeight = 220;

    public SetSyncPointWindow(SetSyncPointViewModel vm) : this(vm, null)
    {
    }

    /// <param name="videoPlayerControl">Tests pass a player here; null builds the configured one.</param>
    internal SetSyncPointWindow(SetSyncPointViewModel vm, VideoPlayerControl? videoPlayerControl)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.Sync.SetSyncPoint;
        CanResize = true;
        EditorChrome.ApplyWindow(this, EditorWindowStyles.Video);
        AddStyles();
        vm.Window = this;
        DataContext = vm;

        // Built first: with no player library the dialog falls back to its no-video mode (#15787),
        // and the sizes below depend on that.
        vm.SetVideoPlayerControl(videoPlayerControl ?? InitVideoPlayer.MakeVideoPlayer());
        vm.VideoPlayerControl.FullScreenIsVisible = false;

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

        var labelVideoInfo = new TextBlock
        {
            FontSize = 12,
            Foreground = EditorChrome.DimTextBrush,
            VerticalAlignment = VerticalAlignment.Center,
            TextTrimming = TextTrimming.CharacterEllipsis,
        }.WithBindText(vm, nameof(vm.VideoInfo));

        // Entering point sync without a video used to be a dead end - the sync point can be typed,
        // but there was no way to load a video from here (issue #13341).
        var buttonOpenVideo = EditorChrome.MakeActionButton(IconNames.MovieOpenOutline, Se.Language.General.OpenVideoFile, vm.OpenVideoFileCommand);

        var panelVideo = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition(GridLength.Auto),
                new ColumnDefinition(GridLength.Auto),
                new ColumnDefinition(new GridLength(1, GridUnitType.Star)),
            },
            VerticalAlignment = VerticalAlignment.Center,
        };
        panelVideo.Add(buttonOpenVideo, 0, 0);
        panelVideo.Add(EditorChrome.MakeToolbarSeparator(), 0, 1);
        panelVideo.Add(labelVideoInfo, 0, 2);

        var colors = WaveformColors.Current;
        vm.AudioVisualizer = new AudioVisualizer
        {
            Width = double.NaN,
            IsReadOnly = true,
            DrawGridLines = Se.Settings.Waveform.DrawGridLines,
            WaveformColor = colors.Waveform,
            WaveformSelectedColor = colors.Selected,
            WaveformGridColor = colors.Grid,
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
        comboBoxLeft.WithAccessibleName(Se.Language.General.Lines); // no visible label (#12087)
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

        // Playback is the one thing with no meaning when there is no video - the nudge buttons and
        // "Go to sub pos" still move the sync point time code, so they stay.
        var buttonPlayTwoSecondsAndBack = EditorChrome
            .MakeActionButton("fa-solid fa-play", Se.Language.Sync.PlayTwoSecondsAndBack, vm.PlayTwoSecondsAndBackLeftCommand)
            .WithBindIsVisible(nameof(vm.IsVideoVisible));

        var panelLeftButtons = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 2,
            VerticalAlignment = VerticalAlignment.Center,
            Children =
            {
                EditorChrome.CreateToolButton(IconNames.ArrowLeftThick, Se.Language.General.OneSecondBack, vm.LeftOneSecondBackCommand),
                buttonPlayTwoSecondsAndBack,
                EditorChrome.CreateToolButton(IconNames.ArrowRightThick, Se.Language.General.OneSecondForward, vm.LeftOneSecondForwardCommand),
                EditorChrome.MakeToolbarSeparator(),
                EditorChrome.MakeActionButton("fa-solid fa-location-crosshairs", Se.Language.Sync.GoToSubPos, vm.GoToLeftSubtitleCommand),
                EditorChrome.MakeActionButton(IconNames.Find, Se.Language.Sync.FindText, vm.FindTextLeftCommand),
            }
        };

        var panelTimeCode = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition(GridLength.Auto),
                new ColumnDefinition(GridLength.Auto),
                new ColumnDefinition(new GridLength(1, GridUnitType.Star)),
            },
            ColumnSpacing = 10,
        };
        panelTimeCode.Add(EditorChrome.MakeDimLabel(Se.Language.Sync.SyncPointTimeCode, new Thickness(0)), 0, 0);
        panelTimeCode.Add(vm.TimeCodeUpDownSyncPoint, 0, 1);
        panelTimeCode.Add(panelLeftButtons, 0, 2);

        var buttonOk = UiUtil.MakeButton(Se.Language.Sync.SetSyncPoint, vm.OkCommand);
        buttonOk.Classes.Add("primary");
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);
        var buttonPanel = EditorChrome.MakeButtonBar(null, buttonCancel, buttonOk);

        // The waveform used to be pinned at 80 px under the player, too little to pick a precise
        // sync point from; the drag handle between them mirrors the main window (issue #14414).
        var split = new VideoWaveformSplitGrid(vm.VideoPlayerControl, vm.AudioVisualizer, Se.Settings.Synchronization.SetSyncPointWaveformHeight)
        {
            IsVideoVisible = vm.IsVideoVisible,
            IsWaveformVisible = vm.IsAudioVisualizerVisible,
        };

        // A Star row keeps its share of the height even when its child is hidden, so the row itself
        // has to collapse - otherwise the videoless dialog is mostly empty space.
        var rowVideoPlayer = new RowDefinition
        {
            Height = vm.IsVideoVisible || vm.IsAudioVisualizerVisible ? new GridLength(1, GridUnitType.Star) : new GridLength(0),
        };

        vm.VideoPlayerControl.WithBindIsVisible(nameof(vm.IsVideoVisible));

        // The line picker and the sync point time code on the panel color under the player.
        var panelControls = new StackPanel
        {
            Spacing = 10,
            Children =
            {
                comboBoxLeft,
                panelTimeCode,
            },
        };
        var sectionControls = new Border
        {
            Child = panelControls,
            Background = EditorChrome.PanelBrush,
            BorderBrush = EditorChrome.LineBrush,
            BorderThickness = new Thickness(0, 1, 0, 0),
            Padding = new Thickness(14, 12, 14, 14),
        };

        // Without a player or waveform the controls take the freed height, so the button bar
        // stays at the bottom of the window.
        var rowControls = new RowDefinition
        {
            Height = vm.IsVideoVisible || vm.IsAudioVisualizerVisible ? GridLength.Auto : new GridLength(1, GridUnitType.Star),
        };

        // The player in a plain border - no corner radius or clipping, the macOS native video
        // surface disappears inside either.
        var videoArea = new Border
        {
            Child = EditorChrome.MakeVideoFrame(split),
            Padding = new Thickness(12),
        };

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) }, // open video + video info
                rowVideoPlayer,                                                       // video player over waveform
                rowControls,                                                          // combo box, sync point time code, buttons
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) }, // set sync point, cancel
            },
        };

        grid.Add(EditorChrome.MakeToolbar(panelVideo), 0);
        grid.Add(videoArea, 1);
        grid.Add(sectionControls, 2);
        grid.Add(buttonPanel, 3);

        Content = grid;

        // "Open video file..." can bring a video in after the videoless dialog is already up: give
        // the player its row back and grow the window, which is otherwise too short to show it.
        // The waveform lent by the main window arrives a beat after construction and shares the row.
        vm.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName != nameof(vm.IsVideoVisible) && e.PropertyName != nameof(vm.IsAudioVisualizerVisible))
            {
                return;
            }

            split.IsVideoVisible = vm.IsVideoVisible;
            split.IsWaveformVisible = vm.IsAudioVisualizerVisible;
            rowVideoPlayer.Height = vm.IsVideoVisible || vm.IsAudioVisualizerVisible ? new GridLength(1, GridUnitType.Star) : new GridLength(0);
            rowControls.Height = vm.IsVideoVisible || vm.IsAudioVisualizerVisible ? GridLength.Auto : new GridLength(1, GridUnitType.Star);

            if (vm.IsVideoVisible)
            {
                MaxHeight = double.PositiveInfinity;
                MinHeight = 650;
                Height = Math.Max(Height, 800);
            }
            else if (vm.IsAudioVisualizerVisible)
            {
                // No player, but a waveform to show: it fills whatever height the user gives the
                // window, so the videoless cap has to go.
                MaxHeight = double.PositiveInfinity;
                MinHeight = VideolessMinHeight + VideoWaveformSplitGrid.DefaultWaveformHeight;
                Height = Math.Max(Height, VideolessHeight + 2 * VideoWaveformSplitGrid.DefaultWaveformHeight);
            }
        };

        // Focus the time code box, not a button, so the window receives key events without arming
        // any button: a focused button fires OnClick on bare Space, and "Set sync point" used to be
        // focused here - so the first Space a user pressed closed the dialog instead of playing.
        UiUtil.FocusOnFirstActivation(this, () => { vm.FocusTimeCodeUpDown(); });

        AddHandler(KeyDownEvent, vm.OnKeyDownHandler, RoutingStrategies.Tunnel | RoutingStrategies.Bubble, handledEventsToo: false);

        // Avalonia's Button raises OnClick from OnKeyUp on Space whenever it is focused - it does
        // not check that it also saw the KeyDown - so handling Space in OnKeyDownHandler alone is
        // not enough to keep a focused button from clicking on Space release.
        AddHandler(KeyUpEvent, vm.OnKeyUpHandler, RoutingStrategies.Tunnel | RoutingStrategies.Bubble, handledEventsToo: true);
        Loaded += (_, e) => vm.OnLoaded();
        Closing += (_, e) =>
        {
            Se.Settings.Synchronization.SetSyncPointWaveformHeight = split.WaveformHeight;
            vm.OnClosing();
        };
    }

    /// <summary>
    /// A compact time code field, matching the other editor-look dialogs.
    /// </summary>
    private void AddStyles()
    {
        Styles.Add(new Style(x => x.OfType<TimeCodeUpDown>())
        {
            Setters =
            {
                new Setter(TemplatedControl.FontSizeProperty, 12.5),
                new Setter(TemplatedControl.MinHeightProperty, 28.0),
            },
        });
        Styles.Add(new Style(x => x.OfType<TimeCodeUpDown>().Descendant().OfType<TextBox>())
        {
            Setters = { new Setter(TemplatedControl.MinHeightProperty, 26.0) },
        });
        Styles.Add(new Style(x => x.OfType<TimeCodeUpDown>().Descendant().OfType<ButtonSpinner>())
        {
            Setters = { new Setter(TemplatedControl.MinHeightProperty, 26.0) },
        });
    }
}
