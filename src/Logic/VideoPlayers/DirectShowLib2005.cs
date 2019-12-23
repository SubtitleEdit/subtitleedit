//using System;
//using System.ComponentModel;
//using System.Runtime.InteropServices;
//using System.Windows.Forms;
//using DirectShowLib;

//namespace Nikse.SubtitleEdit.Logic.VideoPlayers
//{
//    public class DirectShowLib2005 : VideoPlayer, IDisposable
//    {
//        public override event EventHandler OnVideoLoaded;
//        public override event EventHandler OnVideoEnded;

//        private enum PlayState
//        {
//            Stopped,
//            Paused,
//            Running,
//            Init
//        }

//        private enum MediaType
//        {
//            Audio,
//            Video
//        }

//        private const int WmGraphNotify = 0x0400 + 13;
//        private const int VolumeFull = 0;
//        private const int VolumeSilence = -10000;

//        private IGraphBuilder _graphBuilder;
//        private IMediaControl _mediaControl;
//        private IMediaEventEx _mediaEventEx;
//        private IVideoWindow _videoWindow;
//        private IBasicAudio _basicAudio;
//        private IBasicVideo _basicVideo;
//        private IMediaSeeking _mediaSeeking;
//        private IMediaPosition _mediaPosition;
//        private IVideoFrameStep _frameStep;

//        private Control _ownerControl;
//        private string _fileName;
//        private bool _isAudioOnly;
//        private bool _isFullScreen;
//        private int _currentVolume = VolumeFull;
//        private PlayState _currentState = PlayState.Stopped;
//        private double _currentPlaybackRate = 1.0;
//        private Timer _videoEndTimer;
//        private BackgroundWorker _videoLoader;
//        private IntPtr _hDrain = IntPtr.Zero;

//        private void OpenClip(Control ownerControl, string fileName, EventHandler onVideoLoaded, EventHandler onVideoEnded)
//        {
//            try
//            {
//                if (string.IsNullOrEmpty(fileName))
//                {
//                    return;
//                }

//                _currentState = PlayState.Stopped;
//                _currentVolume = VolumeFull;

//                PlayMovieInWindow(_fileName, ownerControl);

//                ownerControl.Resize += OwnerControlResize;
//                if (onVideoLoaded != null)
//                {
//                    _videoLoader = new BackgroundWorker();
//                    _videoLoader.RunWorkerCompleted += VideoLoaderRunWorkerCompleted;
//                    _videoLoader.DoWork += VideoLoaderDoWork;
//                    _videoLoader.RunWorkerAsync();
//                }

//                OwnerControlResize(this, null);
//                _videoEndTimer = new Timer { Interval = 500 };
//                _videoEndTimer.Tick += VideoEndTimerTick;
//                _videoEndTimer.Start();
//            }
//            catch
//            {
//                CloseClip();
//            }
//        }

//        private void PlayMovieInWindow(string fileName, Control ownerControl)
//        {
//            if (string.IsNullOrEmpty(fileName))
//            {
//                return;
//            }

//            // Have the graph builder construct its the appropriate graph automatically
//            _graphBuilder = (IGraphBuilder)new FilterGraph();
//            var hr = _graphBuilder.RenderFile(fileName, null);
//            DsError.ThrowExceptionForHR(hr);

//            // QueryInterface for DirectShow interfaces
//            _mediaControl = (IMediaControl)_graphBuilder;
//            _mediaEventEx = (IMediaEventEx)_graphBuilder;
//            _mediaSeeking = (IMediaSeeking)_graphBuilder;
//            _mediaPosition = (IMediaPosition)_graphBuilder;

//            // Query for video interfaces, which may not be relevant for audio files
//            _videoWindow = _graphBuilder as IVideoWindow;
//            _basicVideo = _graphBuilder as IBasicVideo;

//            // Query for audio interfaces, which may not be relevant for video-only files
//            _basicAudio = _graphBuilder as IBasicAudio;

//            // Is this an audio-only file (no video component)?
//            CheckVisibility();

//            // Have the graph signal event via window callbacks for performance
//            //hr = _mediaEventEx.SetNotifyWindow(ownerControl.Handle, WmGraphNotify, IntPtr.Zero);
//            //DsError.ThrowExceptionForHR(hr);

//            if (!_isAudioOnly)
//            {
//                MakeVideoWindow(ownerControl);
//            }

//            // Complete window initialization
//            _isFullScreen = false;
//            _currentPlaybackRate = 1.0;


//            // Run the graph to play the media file
//            hr = _mediaControl.Run();
//            DsError.ThrowExceptionForHR(hr);

//            _currentState = PlayState.Running;
//        }

//        private void MakeVideoWindow(Control ownerControl)
//        {
//            var hr = _videoWindow.put_Owner(ownerControl.Handle);
//            DsError.ThrowExceptionForHR(hr);

//            hr = _videoWindow.put_WindowStyle(WindowStyle.Child | WindowStyle.ClipSiblings | WindowStyle.ClipChildren);
//            DsError.ThrowExceptionForHR(hr);

//            OwnerControlResize(null, null);

//            GetFrameStepInterface();
//        }

//        private void CloseClip()
//        {
//            int hr = 0;

//            // Stop media playback
//            if (_mediaControl != null)
//                hr = _mediaControl.Stop();

//            // Clear global flags
//            _currentState = PlayState.Stopped;
//            _isAudioOnly = true;
//            _isFullScreen = false;

//            // Free DirectShow interfaces
//            CloseInterfaces();

//            // Clear file name to allow selection of new file with open dialog
//            _fileName = string.Empty;

//            // No current media state
//            _currentState = PlayState.Init;

//        }

//        private int InitVideoWindow(int nMultiplier, int nDivider)
//        {
//            var hr = 0;
//            int lHeight, lWidth;

//            if (_basicVideo == null)
//                return 0;

//            // Read the default video size
//            hr = _basicVideo.GetVideoSize(out lWidth, out lHeight);
//            if (hr == DsResults.E_NoInterface)
//                return 0;


//            // Account for requests of normal, half, or double size
//            lWidth = lWidth * nMultiplier / nDivider;
//            lHeight = lHeight * nMultiplier / nDivider;

//            //TODO:  ClientSize = new Size(lWidth, lHeight);
//            Application.DoEvents();

//            //hr = _videoWindow.SetWindowPosition(0, 0, _ownerControl.Width, _ownerControl.Height);

//            return hr;
//        }

//        private void CheckVisibility()
//        {
//            int hr = 0;
//            OABool lVisible;

//            if ((_videoWindow == null) || (_basicVideo == null))
//            {
//                // Audio-only files have no video interfaces.  This might also
//                // be a file whose video component uses an unknown video codec.
//                _isAudioOnly = true;
//                return;
//            }
//            else
//            {
//                // Clear the global flag
//                _isAudioOnly = false;
//            }

//            hr = _videoWindow.get_Visible(out lVisible);
//            if (hr < 0)
//            {
//                // If this is an audio-only clip, get_Visible() won't work.
//                //
//                // Also, if this video is encoded with an unsupported codec,
//                // we won't see any video, although the audio will work if it is
//                // of a supported format.
//                if (hr == unchecked((int)0x80004002)) //E_NOINTERFACE
//                {
//                    _isAudioOnly = true;
//                }
//                else
//                    DsError.ThrowExceptionForHR(hr);
//            }
//        }

//        //
//        // Some video renderers support stepping media frame by frame with the
//        // IVideoFrameStep interface.  See the interface documentation for more
//        // details on frame stepping.
//        //
//        private bool GetFrameStepInterface()
//        {
//            int hr = 0;

//            IVideoFrameStep frameStepTest = null;

//            // Get the frame step interface, if supported
//            frameStepTest = (IVideoFrameStep)_graphBuilder;

//            // Check if this decoder can step
//            hr = frameStepTest.CanStep(0, null);
//            if (hr == 0)
//            {
//                _frameStep = frameStepTest;
//                return true;
//            }
//            else
//            {
//                // BUG 1560263 found by husakm (thanks)...
//                // Marshal.ReleaseComObject(frameStepTest);
//                _frameStep = null;
//                return false;
//            }
//        }

//        private void CloseInterfaces()
//        {
//            int hr = 0;

//            try
//            {
//                lock (this)
//                {
//                    // Relinquish ownership (IMPORTANT!) after hiding video window
//                    if (!_isAudioOnly)
//                    {
//                        hr = _videoWindow.put_Visible(OABool.False);
//                        DsError.ThrowExceptionForHR(hr);
//                        hr = _videoWindow.put_Owner(IntPtr.Zero);
//                        DsError.ThrowExceptionForHR(hr);
//                    }

//                    if (_mediaEventEx != null)
//                    {
//                        hr = _mediaEventEx.SetNotifyWindow(IntPtr.Zero, 0, IntPtr.Zero);
//                        DsError.ThrowExceptionForHR(hr);
//                    }
//                    // Release and zero DirectShow interfaces
//                    if (_mediaEventEx != null)
//                        _mediaEventEx = null;
//                    if (_mediaSeeking != null)
//                        _mediaSeeking = null;
//                    if (_mediaPosition != null)
//                        _mediaPosition = null;
//                    if (_mediaControl != null)
//                        _mediaControl = null;
//                    if (_basicAudio != null)
//                        _basicAudio = null;
//                    if (_basicVideo != null)
//                        _basicVideo = null;
//                    if (_videoWindow != null)
//                        _videoWindow = null;
//                    if (_frameStep != null)
//                        _frameStep = null;
//                    if (_graphBuilder != null)
//                        Marshal.ReleaseComObject(_graphBuilder); _graphBuilder = null;

//                    GC.Collect();
//                }
//            }
//            catch
//            {
//                // ignore
//            }
//        }

//        /*
//         * Media Related methods
//         */

//        private void PauseClip()
//        {
//            if (_mediaControl == null)
//                return;

//            // Toggle play/pause behavior
//            if ((_currentState == PlayState.Paused) || (_currentState == PlayState.Stopped))
//            {
//                if (_mediaControl.Run() >= 0)
//                    _currentState = PlayState.Running;
//            }
//            else
//            {
//                if (_mediaControl.Pause() >= 0)
//                    _currentState = PlayState.Paused;
//            }
//        }

//        private void StopClip()
//        {
//            int hr = 0;
//            DsLong pos = new DsLong(0);

//            if ((_mediaControl == null) || (_mediaSeeking == null))
//                return;

//            // Stop and reset postion to beginning
//            if ((_currentState == PlayState.Paused) || (_currentState == PlayState.Running))
//            {
//                hr = _mediaControl.Stop();
//                _currentState = PlayState.Stopped;

//                // Seek to the beginning
//                hr = _mediaSeeking.SetPositions(pos, AMSeekingSeekingFlags.AbsolutePositioning, null, AMSeekingSeekingFlags.NoPositioning);

//                // Display the first frame to indicate the reset condition
//                hr = _mediaControl.Pause();
//            }
//        }

//        private int ToggleMute()
//        {
//            int hr = 0;

//            if ((_graphBuilder == null) || (_basicAudio == null))
//                return 0;

//            // Read current volume
//            hr = _basicAudio.get_Volume(out _currentVolume);
//            if (hr == -1) //E_NOTIMPL
//            {
//                // Fail quietly if this is a video-only media file
//                return 0;
//            }
//            else if (hr < 0)
//            {
//                return hr;
//            }

//            // Switch volume levels
//            if (_currentVolume == VolumeFull)
//                _currentVolume = VolumeSilence;
//            else
//                _currentVolume = VolumeFull;

//            // Set new volume
//            hr = _basicAudio.put_Volume(_currentVolume);

//            return hr;
//        }

//        private int ToggleFullScreen(Control ownerControl)
//        {
//            int hr = 0;
//            OABool lMode;

//            // Don't bother with full-screen for audio-only files
//            if ((_isAudioOnly) || (_videoWindow == null))
//                return 0;

//            // Read current state
//            hr = _videoWindow.get_FullScreenMode(out lMode);
//            DsError.ThrowExceptionForHR(hr);

//            if (lMode == OABool.False)
//            {
//                // Save current message drain
//                hr = _videoWindow.get_MessageDrain(out _hDrain);
//                DsError.ThrowExceptionForHR(hr);

//                // Set message drain to application main window
//                hr = _videoWindow.put_MessageDrain(ownerControl.Handle);
//                DsError.ThrowExceptionForHR(hr);

//                // Switch to full-screen mode
//                lMode = OABool.True;
//                hr = _videoWindow.put_FullScreenMode(lMode);
//                DsError.ThrowExceptionForHR(hr);
//                _isFullScreen = true;
//            }
//            else
//            {
//                // Switch back to windowed mode
//                lMode = OABool.False;
//                hr = _videoWindow.put_FullScreenMode(lMode);
//                DsError.ThrowExceptionForHR(hr);

//                // Undo change of message drain
//                hr = _videoWindow.put_MessageDrain(_hDrain);
//                DsError.ThrowExceptionForHR(hr);

//                // Reset video window
//                hr = _videoWindow.SetWindowForeground(OABool.True);
//                DsError.ThrowExceptionForHR(hr);

//                // Reclaim keyboard focus for player application
//                //Focus();
//                _isFullScreen = false;
//            }

//            return hr;
//        }

//        private int StepOneFrame()
//        {
//            int hr = 0;

//            // If the Frame Stepping interface exists, use it to step one frame
//            if (_frameStep != null)
//            {
//                // The graph must be paused for frame stepping to work
//                if (_currentState != PlayState.Paused)
//                    PauseClip();

//                // Step the requested number of frames, if supported
//                hr = _frameStep.Step(1, null);
//            }

//            return hr;
//        }

//        private int StepFrames(int nFramesToStep)
//        {
//            int hr = 0;

//            // If the Frame Stepping interface exists, use it to step frames
//            if (_frameStep != null)
//            {
//                // The renderer may not support frame stepping for more than one
//                // frame at a time, so check for support.  S_OK indicates that the
//                // renderer can step nFramesToStep successfully.
//                hr = _frameStep.CanStep(nFramesToStep, null);
//                if (hr == 0)
//                {
//                    // The graph must be paused for frame stepping to work
//                    if (_currentState != PlayState.Paused)
//                        PauseClip();

//                    // Step the requested number of frames, if supported
//                    hr = _frameStep.Step(nFramesToStep, null);
//                }
//            }

//            return hr;
//        }

//        private int ModifyRate(double dRateAdjust)
//        {
//            int hr = 0;
//            double dRate;

//            // If the IMediaPosition interface exists, use it to set rate
//            if ((_mediaPosition != null) && (dRateAdjust != 0.0))
//            {
//                hr = _mediaPosition.get_Rate(out dRate);
//                if (hr == 0)
//                {
//                    // Add current rate to adjustment value
//                    double dNewRate = dRate + dRateAdjust;
//                    hr = _mediaPosition.put_Rate(dNewRate);

//                    // Save global rate
//                    if (hr == 0)
//                    {
//                        _currentPlaybackRate = dNewRate;
//                    }
//                }
//            }

//            return hr;
//        }

//        private int SetRate(double rate)
//        {
//            int hr = 0;

//            // If the IMediaPosition interface exists, use it to set rate
//            if (_mediaPosition != null)
//            {
//                hr = _mediaPosition.put_Rate(rate);
//                if (hr >= 0)
//                {
//                    _currentPlaybackRate = rate;
//                }
//            }

//            return hr;
//        }

//        private void HandleGraphEvent()
//        {
//            int hr = 0;
//            EventCode evCode;
//            IntPtr evParam1, evParam2;

//            // Make sure that we don't access the media event interface
//            // after it has already been released.
//            if (_mediaEventEx == null)
//                return;

//            // Process all queued events
//            while (_mediaEventEx.GetEvent(out evCode, out evParam1, out evParam2, 0) == 0)
//            {
//                // Free memory associated with callback, since we're not using it
//                hr = _mediaEventEx.FreeEventParams(evCode, evParam1, evParam2);

//                // If this is the end of the clip, reset to beginning
//                if (evCode == EventCode.Complete)
//                {
//                    DsLong pos = new DsLong(0);
//                    // Reset to first frame of movie
//                    hr = _mediaSeeking.SetPositions(pos, AMSeekingSeekingFlags.AbsolutePositioning,
//                      null, AMSeekingSeekingFlags.NoPositioning);
//                    if (hr < 0)
//                    {
//                        // Some custom filters (like the Windows CE MIDI filter)
//                        // may not implement seeking interfaces (IMediaSeeking)
//                        // to allow seeking to the start.  In that case, just stop
//                        // and restart for the same effect.  This should not be
//                        // necessary in most cases.
//                        hr = _mediaControl.Stop();
//                        hr = _mediaControl.Run();
//                    }
//                }
//            }
//        }

//        /*
//         * WinForm Related methods
//         */

//        //protected override void WndProc(ref Message m)
//        //{
//        //    switch (m.Msg)
//        //    {
//        //        case WMGraphNotify:
//        //            {
//        //                HandleGraphEvent();
//        //                break;
//        //            }
//        //    }

//        //    // Pass this message to the video window for notification of system changes
//        //    if (videoWindow != null)
//        //        videoWindow.NotifyOwnerMessage(m.HWnd, m.Msg, m.WParam, m.LParam);

//        //    base.WndProc(ref m);
//        //}




//        // Pause the capture graph.
//        public override string PlayerName => "DirectShowLib";

//        /// <summary>
//        /// In DirectX -10000 is silent and 0 is full volume.
//        /// Also, -3500 to 0 seems to be all you can hear! Not much use for -3500 to -9999...
//        /// </summary>
//        public override int Volume
//        {
//            get
//            {
//                if (_basicAudio == null)
//                    return 0;
//                try
//                {
//                    _basicAudio.get_Volume(out var v);
//                    return v / 35 + 100;
//                }
//                catch
//                {
//                    return 0;
//                }
//            }
//            set
//            {
//                try
//                {
//                    if (value == 0)
//                    {
//                        _basicAudio.put_Volume(-10000);
//                    }
//                    else
//                    {
//                        _basicAudio.put_Volume((value - 100) * 35);
//                    }
//                }
//                catch
//                {
//                    // ignored
//                }
//            }
//        }


//        public override double Duration
//        {
//            get
//            {
//                if (_mediaPosition == null)
//                    return 0;
//                _mediaPosition.get_Duration(out var dur);
//                return dur;
//            }
//        }

//        public override double CurrentPosition
//        {
//            get
//            {
//                if (_mediaPosition == null)
//                    return 0;
//                _mediaPosition.get_CurrentPosition(out var pos);
//                return pos;
//            }
//            set => _mediaPosition?.put_CurrentPosition(value);
//        }

//        public override void Play()
//        {
//            if (_currentState == PlayState.Running)
//            {
//                int hr = _mediaControl.Pause();
//                DsError.ThrowExceptionForHR(hr);
//                _currentState = PlayState.Paused;
//            }
//            else if (_currentState == PlayState.Paused || _currentState == PlayState.Stopped)
//            {
//                int hr = _mediaControl.Run();
//                DsError.ThrowExceptionForHR(hr);
//                _currentState = PlayState.Running;
//            }
//        }

//        public override void Pause()
//        {
//            //PauseClip();

//            // If we are playing
//            if (_currentState == PlayState.Running)
//            {
//                int hr = _mediaControl.Pause();
//                DsError.ThrowExceptionForHR(hr);
//                _currentState = PlayState.Paused;
//            }
//        }
//        // Pause the capture graph.
//        public override void Stop()
//        {
//            //StopClip();
//            //return;

//            // Can only Stop when playing or paused
//            if (_currentState == PlayState.Running || _currentState == PlayState.Paused)
//            {
//                int hr = _mediaControl.Stop();
//                DsError.ThrowExceptionForHR(hr);

//                _currentState = PlayState.Stopped;
//                Rewind();
//            }
//        }

//        public override bool IsPaused => !IsPlaying;

//        public override bool IsPlaying => _currentState == PlayState.Running;

//        public override void Initialize(Control ownerControl, string videoFileName, EventHandler onVideoLoaded, EventHandler onVideoEnded)
//        {
//            OnVideoLoaded = onVideoLoaded;
//            OnVideoEnded = onVideoEnded;
//            _fileName = videoFileName;
//            _ownerControl = ownerControl;
//            OpenClip(ownerControl, videoFileName, onVideoLoaded, onVideoEnded);
//        }

//        public override void DisposeVideoPlayer()
//        {
//            // Dispose();
//        }

//        // Reset the clip back to the beginning
//        public void Rewind()
//        {
//            _mediaPosition.put_CurrentPosition(0);
//        }

//        public void Dispose()
//        {

//        }

//        private void VideoLoaderDoWork(object sender, DoWorkEventArgs e)
//        {
//            int i = 0;
//            while (_currentState == PlayState.Init && i < 100)
//            {
//                Application.DoEvents();
//                i++;
//            }
//        }

//        private void VideoLoaderRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
//        {
//            if (OnVideoLoaded != null)
//            {
//                try
//                {
//                    OnVideoLoaded.Invoke(_graphBuilder, new EventArgs());
//                }
//                catch
//                {
//                    // ignored
//                }
//            }
//            _videoEndTimer = null;
//        }

//        private void VideoEndTimerTick(object sender, EventArgs e)
//        {
//            if (IsPaused == false && _graphBuilder != null && CurrentPosition >= Duration)
//            {
//                Pause();
//                if (OnVideoEnded != null && _graphBuilder != null)
//                {
//                    OnVideoEnded.Invoke(_graphBuilder, new EventArgs());
//                }
//            }
//        }

//        private void OwnerControlResize(object sender, EventArgs e)
//        {
//            if (_basicVideo == null)
//            {
//                return;
//            }

//            try
//            {
//                _basicVideo.get_SourceWidth(out var w);
//                _basicVideo.get_SourceHeight(out var h);

//                // calc new scaled size with correct aspect ratio
//                float factorX = _ownerControl.Width / (float)w;
//                float factorY = _ownerControl.Height / (float)h;

//                int newHeight;
//                int newWidth;
//                if (factorX > factorY)
//                {
//                    newWidth = (int)(w * factorY);
//                    newHeight = (int)(h * factorY);
//                }
//                else
//                {
//                    newWidth = (int)(w * factorX);
//                    newHeight = (int)(h * factorX);
//                }
//                _videoWindow.put_Width(newWidth);
//                _videoWindow.put_Height(newHeight);

//                if (newWidth - 2 < _ownerControl.Width)
//                {
//                    _videoWindow.put_Left((_ownerControl.Width - newWidth) / 2);
//                }
//                if (newHeight - 2 < _ownerControl.Height)
//                {
//                    _videoWindow.put_Top((_ownerControl.Height - newHeight) / 2);
//                }
//            }
//            catch
//            {
//                // ignored
//            }
//        }
//    }
//}
