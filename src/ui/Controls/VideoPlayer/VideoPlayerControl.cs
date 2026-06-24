using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.VideoPlayers;
using Nikse.SubtitleEdit.Logic.VideoPlayers.LibMpvDynamic;
using Optris.Icons.Avalonia;
using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Nikse.SubtitleEdit.Controls.VideoPlayer
{
    public class VideoPlayerControl : UserControl
    {
        public static readonly StyledProperty<Control?> PlayerContentProperty =
            AvaloniaProperty.Register<VideoPlayerControl, Control?>(nameof(PlayerContent));

        public static readonly StyledProperty<double> VolumeProperty =
            AvaloniaProperty.Register<VideoPlayerControl, double>(nameof(Volume), 100);

        /// <summary>
        /// Video position in seconds.
        /// </summary>
        public static readonly StyledProperty<double> PositionProperty =
            AvaloniaProperty.Register<VideoPlayerControl, double>(nameof(Position));

        public static readonly StyledProperty<double> DurationProperty =
            AvaloniaProperty.Register<VideoPlayerControl, double>(nameof(Duration));

        public static readonly StyledProperty<string> ProgressTextProperty =
            AvaloniaProperty.Register<VideoPlayerControl, string>(nameof(ProgressText), default!);

        public static readonly StyledProperty<ICommand> PlayCommandProperty =
            AvaloniaProperty.Register<VideoPlayerControl, ICommand>(nameof(PlayCommand));

        public static readonly StyledProperty<ICommand> StopCommandProperty =
            AvaloniaProperty.Register<VideoPlayerControl, ICommand>(nameof(StopCommand));

        public static readonly StyledProperty<ICommand> FullScreenCommandProperty =
            AvaloniaProperty.Register<VideoPlayerControl, ICommand>(nameof(FullScreenCommand));

        public static readonly StyledProperty<bool> StopIsVisibleProperty =
            AvaloniaProperty.Register<VideoPlayerControl, bool>(nameof(StopIsVisible));

        public static readonly StyledProperty<bool> FullScreenIsVisibleProperty =
            AvaloniaProperty.Register<VideoPlayerControl, bool>(nameof(FullScreenIsVisible));

        public Control? PlayerContent
        {
            get => GetValue(PlayerContentProperty);
            set => SetValue(PlayerContentProperty, value);
        }

        public double Volume
        {
            get => GetValue(VolumeProperty);
            set
            {
                if (value < 0)
                {
                    value = 0;
                }
                else if (value > _videoPlayerInstance.VolumeMaximum)
                {
                    value = _videoPlayerInstance.VolumeMaximum;
                }

                SetValue(VolumeProperty, value);
                _videoPlayerInstance.Volume = value;
            }
        }

        /// <summary>
        /// Video position in seconds.
        /// </summary>
        public double Position
        {
            get => GetValue(PositionProperty);
            set => SetValue(PositionProperty, value);
        }

        public double Duration
        {
            get => GetValue(DurationProperty);
            set => SetValue(DurationProperty, value);
        }

        public string ProgressText
        {
            get => GetValue(ProgressTextProperty);
            set => SetValue(ProgressTextProperty, value);
        }

        private readonly TextBlock _textBlockVideoFileName;
        private readonly TextBlock _textBlockPlayerName;
        private readonly TextBlock _textBlockProgress;

        public ICommand PlayCommand
        {
            get => GetValue(PlayCommandProperty);
            set => SetValue(PlayCommandProperty, value);
        }

        public ICommand StopCommand
        {
            get => GetValue(StopCommandProperty);
            set => SetValue(StopCommandProperty, value);
        }

        public ICommand FullScreenCommand
        {
            get => GetValue(FullScreenCommandProperty);
            set => SetValue(FullScreenCommandProperty, value);
        }

        public bool StopIsVisible
        {
            get => GetValue(StopIsVisibleProperty);
            set => SetValue(StopIsVisibleProperty, value);
        }

        public bool FullScreenIsVisible
        {
            get => GetValue(FullScreenIsVisibleProperty);
            set => SetValue(FullScreenIsVisibleProperty, value);
        }

        private bool _isFullScreen = false;

        public event Action<bool>? IsFullScreenChanged;

        public bool IsFullScreen
        {
            get => _isFullScreen;
            set
            {
                if (_isFullScreen == value)
                {
                    return;
                }

                _buttonFullScreenCollapse.IsVisible = value;
                _buttonFullScreen.IsVisible = !value;
                _isFullScreen = value;

                // Start or stop the auto-hide mechanism based on full screen state
                if (value)
                {
                    StartAutoHideControls();
                }
                else
                {
                    StopAutoHideControls();
                    ShowControls();
                }

                IsFullScreenChanged?.Invoke(value);
            }
        }

        public bool IsPlaying => _videoPlayerInstance.IsPlaying;

        public IVideoPlayer VideoPlayer => _videoPlayerInstance;
        public bool VideoPlayerDisplayTimeLeft { get; set; }

        double _positionIgnore = -1;
        double _volumeIgnore = -1;
        private readonly Button _buttonPlay;
        private readonly Button _buttonFullScreen;
        private readonly Button _buttonFullScreenCollapse;
        private readonly Icon _iconVolume;
        private DispatcherTimer? _positionTimer;
        private int _slowPollCounter;
        private IVideoPlayer _videoPlayerInstance;
        private string _videoFileName;
        private readonly Grid _gridProgress; // Reference to the controls grid
        private DispatcherTimer? _autoHideTimer;
        private DateTime _lastActivityTime;
        private ContentPresenter? _contentPresenter;

        private void NotifyPositionChanged(double newPosition)
        {
            if (Math.Abs(_positionIgnore - newPosition) < 0.001)
            {
                return;
            }

            // First update our property
            Position = newPosition;

            _videoPlayerInstance.Position = newPosition;

            // Then notify listeners like the ViewModel
            PositionChanged?.Invoke(newPosition);
        }

        public void SetPosition(double seconds)
        {
            Position = seconds;
        }

        public void SetPositionDisplayOnly(double seconds)
        {
            _positionIgnore = seconds;
            Position = seconds;
        }

        public int ContentWidth => _contentPresenter?.Bounds.Width > 0 ? (int)_contentPresenter.Bounds.Width : 0;
        public int ContentHeight => _contentPresenter?.Bounds.Height > 0 ? (int)_contentPresenter.Bounds.Height : 0;

        public VideoPlayerControl(IVideoPlayer videoPlayerInstance)
        {
            _videoPlayerInstance = videoPlayerInstance;
            _videoFileName = string.Empty;
            _lastActivityTime = DateTime.UtcNow;

            var mainGrid = new Grid
            {
                RowDefinitions = new RowDefinitions("*,Auto"), // video + controls
                Background = Brushes.Transparent // Enable hit testing for pointer events
            };

            // PlayerContent
            var contentPresenter = new ContentPresenter
            {
                [!ContentPresenter.ContentProperty] = this[!PlayerContentProperty],
                Background = new SolidColorBrush(Colors.Black),
            };
            _contentPresenter = contentPresenter;
            mainGrid.Children.Add(contentPresenter);
            Grid.SetRow(contentPresenter, 0);

            // Row with buttons + position slider + volume slider
            _gridProgress = new Grid
            {
                ColumnDefinitions = new ColumnDefinitions("Auto,*,Auto,Auto"),
                Margin = new Thickness(10, 4)
            };
            Grid.SetRow(_gridProgress, 1);
            mainGrid.Children.Add(_gridProgress);

            // Attach a tunnel handler so we see clicks even if child handles them.
            mainGrid.AddHandler(InputElement.PointerPressedEvent, OnMainGridPointerPressed, RoutingStrategies.Tunnel, handledEventsToo: true);
            // Release handler is on `this` (not mainGrid) so it still fires when the pointer
            // is captured to this control — routing wouldn't reach mainGrid in that case.
            this.AddHandler(InputElement.PointerReleasedEvent, OnMainGridPointerReleased, RoutingStrategies.Tunnel | RoutingStrategies.Bubble, handledEventsToo: true);
            // Scrub the video by scrolling the mouse wheel over the video surface (issue #11080).
            this.AddHandler(InputElement.PointerWheelChangedEvent, OnVideoWheelChanged, RoutingStrategies.Bubble, handledEventsToo: true);

            // Buttons
            var stackPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Center
            };

            // Play
            _buttonPlay = new Button
            {
                Margin = new Thickness(0, 0, 3, 0),
                [AutomationProperties.NameProperty] = Se.Language.General.Play,
            };
            Attached.SetIcon(_buttonPlay, "fa-solid fa-play");
            _buttonPlay.Click += (_, _) =>
            {
                _videoPlayerInstance.PlayOrPause();
                PlayPauseRequested?.Invoke();
            };
            _buttonPlay.Bind(Button.CommandProperty, new Binding
            {
                Path = nameof(PlayCommand),
                Source = this
            });
            if (Se.Settings.Appearance.ShowHints)
            {
                ToolTip.SetTip(_buttonPlay, Se.Language.General.Play);
            }

            stackPanel.Children.Add(_buttonPlay);

            // Stop
            var buttonStop = new Button
            {
                Margin = new Thickness(0, 0, 3, 0),
                [AutomationProperties.NameProperty] = Se.Language.General.Stop,
            };
            buttonStop.Bind(Button.IsVisibleProperty, new Binding
            {
                Path = nameof(StopIsVisible),
                Source = this
            });
            Attached.SetIcon(buttonStop, "fa-solid fa-stop");
            buttonStop.Click += (_, _) =>
            {
                _videoPlayerInstance.Stop();
                StopRequested?.Invoke();
            };
            if (Se.Settings.Appearance.ShowHints)
            {
                ToolTip.SetTip(buttonStop, Se.Language.General.Stop);
            }
            stackPanel.Children.Add(buttonStop);
            buttonStop.Bind(Button.CommandProperty, new Binding
            {
                Path = nameof(StopCommand),
                Source = this
            });

            // Fullscreen
            _buttonFullScreen = new Button
            {
                Margin = new Thickness(0, 0, 3, 0),
                [AutomationProperties.NameProperty] = Se.Language.General.FullScreen,
            };
            _buttonFullScreen.Bind(IsVisibleProperty, new Binding
            {
                Path = nameof(FullScreenIsVisible),
                Source = this
            });
            Attached.SetIcon(_buttonFullScreen, "fa-solid fa-expand");
            _buttonFullScreen.Click += (_, _) => FullscreenRequested?.Invoke();
            if (Se.Settings.Appearance.ShowHints)
            {
                ToolTip.SetTip(_buttonFullScreen, Se.Language.General.FullScreen);
            }
            stackPanel.Children.Add(_buttonFullScreen);
            _buttonFullScreen.Bind(Button.CommandProperty, new Binding
            {
                Path = nameof(FullScreenCommand),
                Source = this
            });


            _buttonFullScreenCollapse = new Button()
            {
                Margin = new Thickness(0, 0, 3, 0),
                IsVisible = false,
                [AutomationProperties.NameProperty] = Se.Language.General.ExitFullScreen,
            };
            Attached.SetIcon(_buttonFullScreenCollapse, "fa-solid fa-compress");
            _buttonFullScreenCollapse.Click += (_, _) => FullscreenCollapseRequested?.Invoke();
            if (Se.Settings.Appearance.ShowHints)
            {
                ToolTip.SetTip(_buttonFullScreenCollapse, Se.Language.General.ExitFullScreen);
            }
            stackPanel.Children.Add(_buttonFullScreenCollapse);

            _gridProgress.Children.Add(stackPanel);
            Grid.SetColumn(stackPanel, 0);

            var sliderPosition = new Slider
            {
                Minimum = 0,
                Margin = new Thickness(2, 0, 0, 0),
                [AutomationProperties.NameProperty] = Se.Language.General.VideoPosition,
            };
            if (Se.Settings.Appearance.ShowHints)
            {
                ToolTip.SetTip(sliderPosition, Se.Language.General.VideoPosition);

                // Show the hovered timestamp in the tooltip (frame/HH:MM:SS:FF vs ms format
                // is already handled by ToDisplayString via UseTimeFormatHHMMSSFF).
                // Avalonia's Slider centers the thumb on the value point, so the effective
                // value-range track is narrower than the slider by one thumb width — we have
                // to match that mapping or the hint reads later than the actual click target.
                const double thumbWidth = 14.0;
                sliderPosition.AddHandler(PointerMovedEvent, (_, e) =>
                {
                    var available = sliderPosition.Bounds.Width - thumbWidth;
                    if (available <= 0 || Duration <= 0)
                    {
                        return;
                    }

                    var x = e.GetPosition(sliderPosition).X - thumbWidth / 2;
                    var ratio = Math.Clamp(x / available, 0.0, 1.0);
                    var hovered = sliderPosition.Minimum + ratio * (sliderPosition.Maximum - sliderPosition.Minimum);
                    var offsetSec = Se.Settings.General.CurrentVideoOffsetInMs / 1000.0;
                    ToolTip.SetTip(sliderPosition, TimeCode.FromSeconds(hovered + offsetSec).ToDisplayString());
                });
            }
            sliderPosition.TemplateApplied += (s, e) =>
            {
                if (e.NameScope.Find<Thumb>("thumb") is Thumb thumb)
                {
                    thumb.Width = 14;
                    thumb.Height = 14;
                }
            };

            sliderPosition.Bind(RangeBase.MaximumProperty, this.GetObservable(DurationProperty));
            sliderPosition.Bind(RangeBase.ValueProperty, this.GetObservable(PositionProperty));

            // Also ensure the control can receive keyboard focus
            sliderPosition.Focusable = true;

            var sliderPositionUserMoving = false;
            sliderPosition.AddHandler(PointerPressedEvent, (_, _) => sliderPositionUserMoving = true, RoutingStrategies.Tunnel);
            sliderPosition.AddHandler(PointerReleasedEvent, (_, _) => sliderPositionUserMoving = false, RoutingStrategies.Tunnel);
            sliderPosition.AddHandler(PointerCaptureLostEvent, (_, _) => sliderPositionUserMoving = false, RoutingStrategies.Tunnel);
            sliderPosition.AddHandler(KeyDownEvent, (_, e) =>
            {
                if (e.Key is Key.Left or Key.Right or Key.Up or Key.Down or Key.Home or Key.End or Key.PageUp or Key.PageDown)
                {
                    sliderPositionUserMoving = true;
                }
            }, RoutingStrategies.Tunnel);
            sliderPosition.AddHandler(KeyUpEvent, (_, _) => sliderPositionUserMoving = false, RoutingStrategies.Tunnel);

            // For any direct value changes
            sliderPosition.ValueChanged += (s, e) =>
            {
                NotifyPositionChanged(e.NewValue);
                if (sliderPositionUserMoving)
                {
                    UserSeeked?.Invoke(e.NewValue);
                }
            };

            _gridProgress.Children.Add(sliderPosition);
            Grid.SetColumn(sliderPosition, 1);

            _iconVolume = new Icon
            {
                Value = "fa-solid fa-volume-up",
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(10, 0, 4, 0)
            };
            _gridProgress.Children.Add(_iconVolume);
            Grid.SetColumn(_iconVolume, 2);

            var sliderVolume = new Slider
            {
                Minimum = 0,
                Maximum = videoPlayerInstance.VolumeMaximum,
                Width = 80,
                VerticalAlignment = VerticalAlignment.Center,
                Focusable = true,
                [AutomationProperties.NameProperty] = Se.Language.General.Volume,
            };
            if (Se.Settings.Appearance.ShowHints)
            {
                ToolTip.SetTip(sliderVolume, Se.Language.General.Volume);
            }
            sliderVolume.TemplateApplied += (s, e) =>
            {
                if (e.NameScope.Find<Thumb>("thumb") is Thumb thumb)
                {
                    thumb.Width = 14;
                    thumb.Height = 14;
                }
            };
            sliderVolume.Bind(RangeBase.ValueProperty, this.GetObservable(VolumeProperty));

            sliderVolume.ValueChanged += (s, e) =>
            {
                if (_volumeIgnore == e.NewValue)
                {
                    return;
                }

                Volume = e.NewValue;
                _videoPlayerInstance.Volume = e.NewValue;
                VolumeChanged?.Invoke(e.NewValue);
                SetVolumeIcon(e.NewValue < 0.0001);

                ToolTip.SetTip(sliderVolume, $"{Se.Language.General.Volume} {sliderVolume.Value:0}%");
            };

            _gridProgress.Children.Add(sliderVolume);
            Grid.SetColumn(sliderVolume, 3);


            // ProgressText
            var progressText = new TextBlock
            {
                VerticalAlignment = VerticalAlignment.Bottom,
                HorizontalAlignment = HorizontalAlignment.Center,
                FontSize = 12,
                FontWeight = FontWeight.Bold,
                FontFeatures = FontFeatureCollection.Parse("tnum"),
            };
            _textBlockProgress = progressText;
            progressText.Bind(TextBlock.TextProperty, this.GetObservable(ProgressTextProperty));
            _gridProgress.Children.Add(progressText);
            Grid.SetColumn(progressText, 1);
            ProgressText = string.Empty;
            progressText.PointerPressed += (_, _) => ToggleDisplayProgressTextModeRequested?.Invoke();

            _textBlockPlayerName = new TextBlock
            {
                VerticalAlignment = VerticalAlignment.Top,
                HorizontalAlignment = HorizontalAlignment.Right,
                FontSize = 9,
                FontWeight = FontWeight.Bold,
                Opacity = 0.6,
            };
            _gridProgress.Children.Add(_textBlockPlayerName);
            Grid.SetColumn(_textBlockPlayerName, 3);

            _textBlockVideoFileName = new TextBlock
            {
                VerticalAlignment = VerticalAlignment.Bottom,
                HorizontalAlignment = HorizontalAlignment.Right,
                FontSize = 9,
                FontWeight = FontWeight.Bold,
                Opacity = 0.6,
                TextAlignment = TextAlignment.Right,
                // Trim from the start so the file extension stays visible (e.g. "…movie.mkv").
                TextTrimming = TextTrimming.PrefixCharacterEllipsis,
                MaxLines = 1,
            };
            _gridProgress.Add(_textBlockVideoFileName, 0, 1, 1, 3);
            _textBlockVideoFileName.PointerPressed += (_, e) => { VideoFileNamePointerPressed?.Invoke(e); };

            // Resize the file-name label with the window: cap its width to the space to the
            // right of the centered position/duration text so it fills what's available
            // without overlapping that text. Recompute both when the controls grow/shrink
            // (window resize) and when the progress text changes size — the latter covers
            // startup, where the progress text is still empty when the grid is first laid out.
            _gridProgress.SizeChanged += (_, _) => UpdateVideoFileNameMaxWidth();
            _textBlockProgress.SizeChanged += (_, _) => UpdateVideoFileNameMaxWidth();

            Content = mainGrid;

            sliderPosition.Maximum = 1;
            sliderPosition.Value = 0;

            sliderVolume.Maximum = LibMpvDynamicPlayer.MaxVolume;
            sliderVolume.Value = 50;

            // Attach keyboard event handler to detect keyboard activity
            this.KeyDown += OnKeyDown;
        }

        // Raised when the user clicks the video surface (row 0), not the controls row.
        public event EventHandler<PointerPressedEventArgs>? SurfacePointerPressed;

        // Enable/disable click-to-toggle behavior (default on)
        public bool ClickToTogglePlay { get; set; } = true;
        public bool IsSmpteTimingEnabled { get; set; }
        private bool _surfaceLeftButtonDown;

        private void OnMainGridPointerPressed(object? sender, PointerPressedEventArgs e)
        {
            var props = e.GetCurrentPoint(this).Properties;
            if (props.PointerUpdateKind != PointerUpdateKind.LeftButtonPressed || _surfaceLeftButtonDown)
            {
                return;
            }

            // If the click is inside the controls row (_gridProgress), ignore it
            var inControls = false;
            try
            {
                var ptInControls = e.GetPosition(_gridProgress);
                inControls =
                    ptInControls.X >= 0 && ptInControls.Y >= 0 &&
                    ptInControls.X <= _gridProgress.Bounds.Width &&
                    ptInControls.Y <= _gridProgress.Bounds.Height;
            }
            catch
            {
                // ignore
            }

            if (inControls)
            {
                return;
            }

            _surfaceLeftButtonDown = true;
            e.Pointer.Capture(this);

            // This is a click on the video surface
            SurfacePointerPressed?.Invoke(this, e);

            if (ClickToTogglePlay)
            {
                _videoPlayerInstance.PlayOrPause();
                PlayPauseRequested?.Invoke();
                e.Handled = true;
            }

            if (IsFullScreen)
            {
                // Consider this user activity for the auto-hide logic
                OnUserActivity();
            }
        }

        private void OnMainGridPointerReleased(object? sender, PointerReleasedEventArgs e)
        {
            if (!_surfaceLeftButtonDown)
            {
                return;
            }

            var props = e.GetCurrentPoint(this).Properties;
            if (!props.IsLeftButtonPressed)
            {
                _surfaceLeftButtonDown = false;
                e.Pointer.Capture(null);
            }
        }

        private void OnVideoWheelChanged(object? sender, PointerWheelEventArgs e)
        {
            // Ignore wheel events over the controls row (sliders, buttons).
            try
            {
                if (_gridProgress.IsVisible)
                {
                    var ptInControls = e.GetPosition(_gridProgress);
                    if (ptInControls.X >= 0 && ptInControls.Y >= 0 &&
                        ptInControls.X <= _gridProgress.Bounds.Width &&
                        ptInControls.Y <= _gridProgress.Bounds.Height)
                    {
                        return;
                    }
                }
            }
            catch
            {
                // ignore
            }

            var delta = e.Delta.Y;
            if (Math.Abs(delta) < 0.1 && Math.Abs(e.Delta.X) > 0.1)
            {
                delta = e.Delta.X;
            }

            if (Math.Abs(delta) < 0.0001)
            {
                return;
            }

            if (Se.Settings.Waveform.InvertMouseWheel)
            {
                delta = -delta;
            }

            const double stepSeconds = 0.5;
            var newPosition = Position + delta * stepSeconds;

            var duration = Duration;
            if (newPosition < 0)
            {
                newPosition = 0;
            }
            else if (duration > 0 && newPosition > duration)
            {
                newPosition = duration;
            }

            NotifyPositionChanged(newPosition);
            UserSeeked?.Invoke(newPosition);

            if (IsFullScreen)
            {
                OnUserActivity();
            }

            e.Handled = true;
        }

        public event Action? PlayPauseRequested;
        public event Action? StopRequested;
        public event Action? FullscreenRequested;
        public event Action? FullscreenCollapseRequested;
        public event Action<double>? PositionChanged;
        public event Action<double>? UserSeeked;
        public event Action<double>? VolumeChanged;
        public event Action? ToggleDisplayProgressTextModeRequested;
        public event Action<PointerPressedEventArgs>? VideoFileNamePointerPressed;

        public void SetPlayPauseIcon(bool isPlaying)
        {
            if (isPlaying)
            {
                Attached.SetIcon(_buttonPlay, "fa-solid fa-pause");
                AutomationProperties.SetName(_buttonPlay, Se.Language.General.Pause);
                if (Se.Settings.Appearance.ShowHints)
                {
                    ToolTip.SetTip(_buttonPlay, Se.Language.General.Pause);
                }
            }
            else
            {
                Attached.SetIcon(_buttonPlay, "fa-solid fa-play");
                AutomationProperties.SetName(_buttonPlay, Se.Language.General.Play);
                if (Se.Settings.Appearance.ShowHints)
                {
                    ToolTip.SetTip(_buttonPlay, Se.Language.General.Play);
                }
            }
        }

        public void SetVolumeIcon(bool isMuted)
        {
            Dispatcher.UIThread.Invoke(() => { _iconVolume.Value = isMuted ? "fa-solid fa-volume-xmark" : "fa-solid fa-volume-up"; });
        }

        internal async Task Open(string videoFileName)
        {
            // Reset slider state before LoadFile. Otherwise, when the new file's
            // Duration arrives on the next timer tick, the slider's Maximum drops
            // and a stale Value (left over from the previous file) gets clamped to
            // the new Maximum — firing ValueChanged and seeking mpv to EOF.
            SetPositionDisplayOnly(0);
            Duration = 0;

            await _videoPlayerInstance.LoadFile(videoFileName);
            _videoPlayerInstance.Volume = Volume;
            _positionTimer?.Stop();
            _slowPollCounter = 4; // force Duration+icon update on the very first tick
            StartPositionTimer();
            _videoPlayerInstance.Pause();
            _textBlockPlayerName.Text = _videoPlayerInstance.Name;
            _videoFileName = videoFileName;

            // Re-arm fullscreen auto-hide. A preceding Close() (e.g. via Ctrl+N
            // before opening a new file) stops _autoHideTimer, and the IsFullScreen
            // setter doesn't run a fresh true→true transition — so without this
            // the controls would stay visible until the user moves the cursor on
            // the fullscreen monitor.
            if (IsFullScreen)
            {
                StartAutoHideControls();
            }

            _textBlockVideoFileName.Text = System.IO.Path.GetFileName(videoFileName);
            UpdateVideoFileNameMaxWidth();
        }

        // Cap the file-name label to the width available to the right of the centered
        // position/duration text. The label is right-aligned, so this lets it fill the
        // free space and grow/shrink with the window while its PrefixCharacterEllipsis
        // trims the start when the name is too long to fit.
        private void UpdateVideoFileNameMaxWidth()
        {
            var gridWidth = _gridProgress.Bounds.Width;
            if (gridWidth <= 0)
            {
                return;
            }

            // Right edge of the centered progress text (falls back to the grid center
            // before that text has been laid out).
            var progressRight = _textBlockProgress.Bounds.Width > 0
                ? _textBlockProgress.Bounds.Right
                : gridWidth / 2;

            const double gap = 8;
            var available = gridWidth - progressRight - gap;
            _textBlockVideoFileName.MaxWidth = available > 20 ? available : 20;
        }

        internal void Close()
        {
            _positionTimer?.Stop();
            StopAutoHideControls();
            _videoPlayerInstance.CloseFile();
            ProgressText = string.Empty;
            _videoFileName = string.Empty;
            _textBlockVideoFileName.Text = string.Empty;
            SetPositionDisplayOnly(0);
            Duration = 0;
        }

        internal async Task WaitForPlayersReadyAsync(int timeoutMs = 2500)
        {
            var end = DateTime.UtcNow.AddMilliseconds(timeoutMs);
            while (DateTime.UtcNow < end)
            {
                // Consider player ready when Duration is known (> 0)
                var ready = VideoPlayer.Duration > 0.001;

                if (ready)
                {
                    break;
                }

                await Task.Delay(100);
            }

            // Small extra delay to ensure seeking is reliable
            await Task.Delay(200);
        }

        internal void TogglePlayPause()
        {
            _videoPlayerInstance.PlayOrPause();
        }

        internal AudioTrackInfo? ToggleAudioTrack()
        {
            return _videoPlayerInstance.ToggleAudioTrack();
        }

        private void StartPositionTimer()
        {
            _positionTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(50) };
            _positionTimer.Tick += (s, e) =>
            {
                // Duration and IsPlaying change infrequently — poll every 5th tick (~250 ms)
                // instead of every 50 ms to reduce P/Invoke overhead on the UI thread.
                // Polled first so that ProgressText below always uses the current Duration.
                _slowPollCounter++;
                if (_slowPollCounter >= 5)
                {
                    _slowPollCounter = 0;
                    Duration = _videoPlayerInstance.Duration;
                    SetPlayPauseIcon(_videoPlayerInstance.IsPlaying);
                }

                var postFix = IsSmpteTimingEnabled ? " (SMPTE)" : string.Empty;
                var pos = _videoPlayerInstance.Position;
                if (IsSmpteTimingEnabled)
                {
                    pos = pos * 1000.0 / 1001.0; // SMPTE timing adjustment
                }

                SetPositionDisplayOnly(pos);

                var fullDuration = TimeCode.FromSeconds(Duration + Se.Settings.General.CurrentVideoOffsetInMs / 1000.0).ToDisplayString();
                if (VideoPlayerDisplayTimeLeft)
                {
                    var left = Duration - pos;

                    if (left > 0.001)
                    {
                        ProgressText =
                            $"-{TimeCode.FromSeconds(left).ToDisplayString()} / {fullDuration}{postFix}";
                    }
                    else
                    {
                        ProgressText =
                            $"{TimeCode.FromSeconds(0).ToDisplayString()} / {fullDuration}{postFix}";
                    }
                }
                else
                {
                    ProgressText =
                        $" {TimeCode.FromSeconds(pos + Se.Settings.General.CurrentVideoOffsetInMs / 1000.0).ToDisplayString()} / {fullDuration}{postFix}";
                }
            };
            _positionTimer.Start();
        }

        private void StartAutoHideControls()
        {
            _lastActivityTime = DateTime.UtcNow;

            // When the user opts to hide controls in full-screen, never show them —
            // not even briefly on entry — and don't bother arming the auto-hide timer.
            if (Se.Settings.Video.FullscreenHideControls)
            {
                HideControls();
                _autoHideTimer?.Stop();
                return;
            }

            // Show controls initially when entering full screen
            ShowControls();

            // Single one-shot timer reset on each user activity. Stops itself on tick so
            // Stop()+Start() reliably reschedules a fresh 3-second wait from "now" — a
            // free-running periodic timer can drift out of phase with _lastActivityTime
            // and fail to hide after the user re-shows controls during playback.
            if (_autoHideTimer == null)
            {
                _autoHideTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(3) };
                _autoHideTimer.Tick += (s, e) =>
                {
                    _autoHideTimer?.Stop();
                    if (IsFullScreen)
                    {
                        HideControls();
                    }
                };
            }

            _autoHideTimer.Stop();
            _autoHideTimer.Start();
        }

        private void StopAutoHideControls()
        {
            _autoHideTimer?.Stop();
        }

        private void OnUserActivity()
        {
            _lastActivityTime = DateTime.UtcNow;
            if (IsFullScreen)
            {
                // If the user opted to hide controls in full-screen, don't reveal them on activity.
                if (Se.Settings.Video.FullscreenHideControls)
                {
                    return;
                }

                ShowControls();
                if (_autoHideTimer != null)
                {
                    _autoHideTimer.Stop();
                    _autoHideTimer.Start();
                }
            }
        }

        public void NotifyUserActivity()
        {
            OnUserActivity();
        }

        private void OnKeyDown(object? sender, KeyEventArgs e)
        {
            if (IsFullScreen)
            {
                OnUserActivity();
            }
        }

        public void Reload()
        {
            var videoFileName = _videoFileName;
            var position = Position;
            Close();
            Dispatcher.UIThread.Post(async () =>
            {
                try
                {
                    await Task.Delay(100);
                    await Open(videoFileName);
                    await Task.Delay(100);
                    Position = position;
                }
                catch (Exception e)
                {
                    Se.LogError(e, "Failed to reload video");
                }
            });
        }

        private void ShowControls()
        {
            Dispatcher.UIThread.Post(() => { _gridProgress.IsVisible = true; });
        }

        private void HideControls()
        {
            Dispatcher.UIThread.Post(() => { _gridProgress.IsVisible = false; });
        }

        internal void SetSpeed(double speed)
        {
            _videoPlayerInstance.Speed = speed;
        }

        public void HideVideoControls()
        {
            _gridProgress.IsVisible = false;
        }
    }
}

