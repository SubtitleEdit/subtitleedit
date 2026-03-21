using Avalonia;
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
using Projektanker.Icons.Avalonia;
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

        public bool IsFullScreen
        {
            get => _isFullScreen;
            set
            {
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
            }
        }

        public bool IsPlaying => _videoPlayerInstance.IsPlaying;

        public IVideoPlayerInstance VideoPlayerInstance => _videoPlayerInstance;
        public bool VideoPlayerDisplayTimeLeft { get; set; }

        double _positionIgnore = -1;
        double _volumeIgnore = -1;
        private readonly Button _buttonPlay;
        private readonly Button _buttonFullScreen;
        private readonly Button _buttonFullScreenCollapse;
        private readonly Icon _iconVolume;
        private DispatcherTimer? _positionTimer;
        private IVideoPlayerInstance _videoPlayerInstance;
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

        public VideoPlayerControl(IVideoPlayerInstance videoPlayerInstance)
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

            stackPanel.Children.Add(_buttonPlay);

            // Stop
            var buttonStop = new Button
            {
                Margin = new Thickness(0, 0, 3, 0),
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
            };
            _buttonFullScreen.Bind(IsVisibleProperty, new Binding
            {
                Path = nameof(FullScreenIsVisible),
                Source = this
            });
            Attached.SetIcon(_buttonFullScreen, "fa-solid fa-expand");
            _buttonFullScreen.Click += (_, _) => FullscreenRequested?.Invoke();
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
            };
            Attached.SetIcon(_buttonFullScreenCollapse, "fa-solid fa-compress");
            _buttonFullScreenCollapse.Click += (_, _) => FullscreenCollapseRequested?.Invoke();
            stackPanel.Children.Add(_buttonFullScreenCollapse);

            _gridProgress.Children.Add(stackPanel);
            Grid.SetColumn(stackPanel, 0);

            var sliderPosition = new Slider
            {
                Minimum = 0,
                Margin = new Thickness(2, 0, 0, 0),
            };
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

            // For any direct value changes
            sliderPosition.ValueChanged += (s, e) => { NotifyPositionChanged(e.NewValue); };

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
                VerticalAlignment = VerticalAlignment.Center
            };
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
            };
            progressText.Bind(TextBlock.TextProperty, this.GetObservable(ProgressTextProperty));
            _gridProgress.Children.Add(progressText);
            Grid.SetColumn(progressText, 1);
            ProgressText = string.Empty;
            progressText.PointerPressed += (_, _) => ToggleDisplayProgressTextModeRequested?.Invoke();

            _textBlockPlayerName = new TextBlock
            {
                VerticalAlignment = VerticalAlignment.Top,
                HorizontalAlignment = HorizontalAlignment.Right,
                FontSize = 8,
                FontWeight = FontWeight.Bold,
                Opacity = 0.4,
            };
            _gridProgress.Children.Add(_textBlockPlayerName);
            Grid.SetColumn(_textBlockPlayerName, 3);

            _textBlockVideoFileName = new TextBlock
            {
                VerticalAlignment = VerticalAlignment.Bottom,
                HorizontalAlignment = HorizontalAlignment.Right,
                FontSize = 8,
                FontWeight = FontWeight.Bold,
                Opacity = 0.4,
                TextAlignment = TextAlignment.Right,
            };
            _gridProgress.Add(_textBlockVideoFileName, 0, 1, 1, 3);
            _textBlockVideoFileName.PointerPressed += (_, e) => { VideoFileNamePointerPressed?.Invoke(e); };

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

        private void OnMainGridPointerPressed(object? sender, PointerPressedEventArgs e)
        {
            // Only act on left button presses
            var props = e.GetCurrentPoint(this).Properties;
            if (!props.IsLeftButtonPressed)
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

        public event Action? PlayPauseRequested;
        public event Action? StopRequested;
        public event Action? FullscreenRequested;
        public event Action? FullscreenCollapseRequested;
        public event Action<double>? PositionChanged;
        public event Action<double>? VolumeChanged;
        public event Action? ToggleDisplayProgressTextModeRequested;
        public event Action<PointerPressedEventArgs>? VideoFileNamePointerPressed;

        public void SetPlayPauseIcon(bool isPlaying)
        {
            if (isPlaying)
            {
                Attached.SetIcon(_buttonPlay, "fa-solid fa-pause");
            }
            else
            {
                Attached.SetIcon(_buttonPlay, "fa-solid fa-play");
            }
        }

        public void SetVolumeIcon(bool isMuted)
        {
            Dispatcher.UIThread.Invoke(() => { _iconVolume.Value = isMuted ? "fa-solid fa-volume-xmark" : "fa-solid fa-volume-up"; });
        }

        internal async Task Open(string videoFileName)
        {
            await _videoPlayerInstance.LoadFile(videoFileName);
            _videoPlayerInstance.Volume = Volume;
            _positionTimer?.Stop();
            StartPositionTimer();
            _videoPlayerInstance.Pause();
            _textBlockPlayerName.Text = _videoPlayerInstance.Name;
            _videoFileName = videoFileName;

            var shortName = System.IO.Path.GetFileName(videoFileName);
            if (shortName.Length > 55)
            {
                shortName = "..." + shortName[^50..];
            }
            _textBlockVideoFileName.Text = shortName;
        }

        internal void Close()
        {
            _positionTimer?.Stop();
            StopAutoHideControls();
            _videoPlayerInstance.CloseFile();
            ProgressText = string.Empty;
            _videoFileName = string.Empty;
            _textBlockVideoFileName.Text = string.Empty;
        }

        internal async Task WaitForPlayersReadyAsync(int timeoutMs = 2500)
        {
            var end = DateTime.UtcNow.AddMilliseconds(timeoutMs);
            while (DateTime.UtcNow < end)
            {
                // Consider player ready when Duration is known (> 0)
                var ready = VideoPlayerInstance.Duration > 0.001;

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
                var postFix = IsSmpteTimingEnabled ? " (SMPTE)" : string.Empty;
                var pos = _videoPlayerInstance.Position;
                if (IsSmpteTimingEnabled)
                {
                    pos = pos * 1000.0 / 1001.0; // SMPTE timing adjustment 
                }

                SetPositionDisplayOnly(pos);

                if (VideoPlayerDisplayTimeLeft)
                {
                    var left = Duration - pos;

                    if (left > 0.001)
                    {
                        ProgressText =
                            $"-{TimeCode.FromSeconds(left).ToShortDisplayString()}{postFix}";
                    }
                    else
                    {
                        ProgressText =
                            $"{TimeCode.FromSeconds(0).ToShortDisplayString()}{postFix}";
                    }
                }
                else
                {
                    ProgressText =
                        $"{TimeCode.FromSeconds(pos + Se.Settings.General.CurrentVideoOffsetInMs / 1000.0).ToShortDisplayString()} / {TimeCode.FromSeconds(Duration + Se.Settings.General.CurrentVideoOffsetInMs / 1000.0).ToShortDisplayString()}{postFix}";
                }

                //TODO: move to a slower timer or events
                Duration = _videoPlayerInstance.Duration;
                var isPlaying = _videoPlayerInstance.IsPlaying;
                SetPlayPauseIcon(isPlaying);
            };
            _positionTimer.Start();
        }

        private void StartAutoHideControls()
        {
            _lastActivityTime = DateTime.UtcNow;

            // Show controls initially when entering full screen
            ShowControls();

            // Timer to hide controls after 3 seconds of inactivity
            _autoHideTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(3) };
            _autoHideTimer.Tick += (s, e) =>
            {
                if (IsFullScreen && (DateTime.UtcNow - _lastActivityTime).TotalSeconds >= 3)
                {
                    HideControls();
                }
            };
            _autoHideTimer.Start();
        }

        private void StopAutoHideControls()
        {
            _autoHideTimer?.Stop();
            _autoHideTimer = null;
        }

        private void OnUserActivity()
        {
            _lastActivityTime = DateTime.UtcNow;
            if (IsFullScreen)
            {
                ShowControls();
                _autoHideTimer?.Start();
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
            Dispatcher.UIThread.Post(async void () =>
            {
                try
                {
                    Task.Delay(100).Wait();
                    await Open(videoFileName);
                    Task.Delay(100).Wait();
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

