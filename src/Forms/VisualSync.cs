using Nikse.SubtitleEdit.Controls;
using Nikse.SubtitleEdit.Core;
using Nikse.SubtitleEdit.Core.Forms.FixCommonErrors;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.VideoPlayers;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.Forms
{
    public sealed partial class VisualSync : PositionAndSizeForm
    {
        private List<Paragraph> _paragraphs;
        private List<Paragraph> _paragraphsAlternate;
        private VideoInfo _videoInfo;
        private string _subtitleFileName;
        private Subtitle _inputSubtitle;
        private Subtitle _inputAlternateSubtitle;
        private double _oldFrameRate;
        private bool _isStartSceneActive;
        private double _startGoBackPosition;
        private double _startStopPosition = -1.0;
        private double _endGoBackPosition;
        private double _endStopPosition = -1.0;
        private readonly LanguageStructure.VisualSync _language;
        private readonly LanguageStructure.General _languageGeneral;
        private readonly Timer _timerHideSyncLabel = new Timer();
        private string _adjustInfo = string.Empty;

        public string VideoFileName { get; set; }
        public int AudioTrackNumber { get; set; }

        public bool OkPressed { get; set; }

        public bool FrameRateChanged { get; private set; }

        public double FrameRate => _videoInfo?.FramesPerSecond ?? 0;

        public List<Paragraph> Paragraphs => _paragraphs;

        public List<Paragraph> ParagraphsAlternate => _paragraphsAlternate;

        public VisualSync()
        {
            UiUtil.PreInitialize(this);
            InitializeComponent();
            UiUtil.FixFonts(this);

            openFileDialog1.InitialDirectory = string.Empty;

            MediaPlayerStart.InitializeVolume(Configuration.Settings.General.VideoPlayerDefaultVolume);
            MediaPlayerEnd.InitializeVolume(Configuration.Settings.General.VideoPlayerDefaultVolume);

            labelSyncDone.Text = string.Empty;
            _language = Configuration.Settings.Language.VisualSync;
            _languageGeneral = Configuration.Settings.Language.General;
            Text = _language.Title;
            buttonOpenMovie.Text = _languageGeneral.OpenVideoFile;
            groupBoxMovieInfo.Text = _languageGeneral.VideoInformation;
            labelVideoInfo.Text = _languageGeneral.NoVideoLoaded;
            groupBoxStartScene.Text = _language.StartScene;
            groupBoxEndScene.Text = _language.EndScene;
            buttonStartThreeSecondsBack.Text = _language.ThreeSecondsBack;
            buttonThreeSecondsBack.Text = _language.ThreeSecondsBack;
            buttonStartHalfASecondBack.Text = _language.HalfASecondBack;
            buttonEndHalfASecondBack.Text = _language.HalfASecondBack;
            buttonStartVerify.Text = string.Format(_language.PlayXSecondsAndBack, Configuration.Settings.Tools.VerifyPlaySeconds);
            buttonEndVerify.Text = buttonStartVerify.Text;
            buttonGotoStartSubtitlePosition.Text = _language.GoToSubPosition;
            buttonGotoEndSubtitlePosition.Text = _language.GoToSubPosition;
            buttonFindTextStart.Text = _language.FindText;
            buttonFindTextEnd.Text = _language.FindText;
            buttonSync.Text = _language.Synchronize;
            buttonOK.Text = _languageGeneral.Ok;
            buttonCancel.Text = _languageGeneral.Cancel;
            labelTip.Text = _language.Tip;
            UiUtil.FixLargeFonts(this, buttonCancel);
            _timerHideSyncLabel.Tick += timerHideSyncLabel_Tick;
            _timerHideSyncLabel.Interval = 1000;
        }

        private void timerHideSyncLabel_Tick(object sender, EventArgs e)
        {
            labelSyncDone.Text = _adjustInfo;
        }

        private void GotoSubtitlePosition(VideoPlayerContainer mediaPlayer)
        {
            int index;
            if (mediaPlayer == MediaPlayerStart)
            {
                index = comboBoxStartTexts.SelectedIndex;
            }
            else
            {
                index = comboBoxEndTexts.SelectedIndex;
            }

            mediaPlayer.Pause();
            if (index != -1)
            {
                double indexPositionInSeconds = _paragraphs[index].StartTime.TotalMilliseconds / TimeCode.BaseUnit;

                if (indexPositionInSeconds > mediaPlayer.Duration)
                {
                    indexPositionInSeconds = mediaPlayer.Duration - (2 * 60);
                }

                if (indexPositionInSeconds < 0)
                {
                    indexPositionInSeconds = 0;
                }

                mediaPlayer.CurrentPosition = indexPositionInSeconds;
                mediaPlayer.RefreshProgressBar();
            }
        }

        private void OpenVideo(string fileName)
        {
            if (File.Exists(fileName))
            {
                timer1.Stop();
                timerProgressBarRefresh.Stop();

                VideoFileName = fileName;

                var fi = new FileInfo(fileName);
                if (fi.Length < 1000)
                {
                    return;
                }

                if (MediaPlayerStart.VideoPlayer != null)
                {
                    MediaPlayerStart.Pause();
                    MediaPlayerStart.VideoPlayer.DisposeVideoPlayer();
                }
                if (MediaPlayerEnd.VideoPlayer != null)
                {
                    MediaPlayerEnd.Pause();
                    MediaPlayerEnd.VideoPlayer.DisposeVideoPlayer();
                }

                VideoInfo videoInfo = ShowVideoInfo(fileName);
                UiUtil.InitializeVideoPlayerAndContainer(fileName, videoInfo, MediaPlayerStart, VideoStartLoaded, VideoStartEnded);
            }
        }

        private void VideoStartEnded(object sender, EventArgs e)
        {
            MediaPlayerStart.Pause();
        }

        private void VideoStartLoaded(object sender, EventArgs e)
        {
            MediaPlayerStart.Pause();
            GotoSubtitlePosition(MediaPlayerStart);

            _startGoBackPosition = MediaPlayerStart.CurrentPosition;
            _startStopPosition = _startGoBackPosition + 0.1;
            MediaPlayerStart.Play();

            if (MediaPlayerStart.VideoPlayer.GetType() == typeof(LibVlcDynamic))
            {
                MediaPlayerEnd.VideoPlayer = (MediaPlayerStart.VideoPlayer as LibVlcDynamic).MakeSecondMediaPlayer(MediaPlayerEnd.PanelPlayer, VideoFileName, VideoEndLoaded, VideoEndEnded);
            }
            else
            {
                UiUtil.InitializeVideoPlayerAndContainer(VideoFileName, _videoInfo, MediaPlayerEnd, VideoEndLoaded, VideoEndEnded);
            }
            timer1.Start();
            timerProgressBarRefresh.Start();

            if (AudioTrackNumber >= 0 && MediaPlayerStart.VideoPlayer is LibVlcDynamic)
            {
                var libVlc = (LibVlcDynamic)MediaPlayerStart.VideoPlayer;
                libVlc.AudioTrackNumber = AudioTrackNumber;
            }
            else if (AudioTrackNumber >= 0 && MediaPlayerStart.VideoPlayer is LibMpvDynamic)
            {
                var libMpv = (LibMpvDynamic)MediaPlayerStart.VideoPlayer;
                libMpv.AudioTrackNumber = AudioTrackNumber;
            }
        }

        private void VideoEndEnded(object sender, EventArgs e)
        {
            MediaPlayerEnd.Pause();
        }

        private void VideoEndLoaded(object sender, EventArgs e)
        {
            MediaPlayerEnd.Pause();
            GotoSubtitlePosition(MediaPlayerEnd);

            _endGoBackPosition = MediaPlayerEnd.CurrentPosition;
            _endStopPosition = _endGoBackPosition + 0.1;
            MediaPlayerEnd.Play();

            if (AudioTrackNumber >= 0 && MediaPlayerEnd.VideoPlayer is LibVlcDynamic)
            {
                var libVlc = (LibVlcDynamic)MediaPlayerEnd.VideoPlayer;
                libVlc.AudioTrackNumber = AudioTrackNumber;
            }
            else if (AudioTrackNumber >= 0 && MediaPlayerEnd.VideoPlayer is LibMpvDynamic)
            {
                var libMpv = (LibMpvDynamic)MediaPlayerEnd.VideoPlayer;
                libMpv.AudioTrackNumber = AudioTrackNumber;
            }
        }

        private VideoInfo ShowVideoInfo(string fileName)
        {
            _videoInfo = UiUtil.GetVideoInfo(fileName);
            var info = new FileInfo(fileName);
            long fileSizeInBytes = info.Length;

            labelVideoInfo.Text = string.Format(_languageGeneral.FileNameXAndSize, fileName, Utilities.FormatBytesToDisplayFileSize(fileSizeInBytes)) + Environment.NewLine +
                                  string.Format(_languageGeneral.ResolutionX, +_videoInfo.Width + "x" + _videoInfo.Height) + "    ";
            if (_videoInfo.FramesPerSecond > 5 && _videoInfo.FramesPerSecond < 200)
            {
                labelVideoInfo.Text += string.Format(_languageGeneral.FrameRateX + "        ", _videoInfo.FramesPerSecond);
            }

            if (_videoInfo.TotalFrames > 10)
            {
                labelVideoInfo.Text += string.Format(_languageGeneral.TotalFramesX + "         ", (int)_videoInfo.TotalFrames);
            }

            if (!string.IsNullOrEmpty(_videoInfo.VideoCodec))
            {
                labelVideoInfo.Text += string.Format(_languageGeneral.VideoEncodingX, _videoInfo.VideoCodec) + "        ";
            }

            return _videoInfo;
        }

        private void Timer1Tick(object sender, EventArgs e)
        {
            if (MediaPlayerStart != null)
            {
                if (!MediaPlayerStart.IsPaused)
                {
                    MediaPlayerStart.RefreshProgressBar();
                    if (_startStopPosition >= 0 && MediaPlayerStart.CurrentPosition > _startStopPosition)
                    {
                        MediaPlayerStart.Pause();
                        MediaPlayerStart.CurrentPosition = _startGoBackPosition;
                        _startStopPosition = -1;
                    }
                    UiUtil.ShowSubtitle(new Subtitle(_paragraphs), MediaPlayerStart);
                }
                if (!MediaPlayerEnd.IsPaused)
                {
                    MediaPlayerEnd.RefreshProgressBar();
                    if (_endStopPosition >= 0 && MediaPlayerEnd.CurrentPosition > _endStopPosition)
                    {
                        MediaPlayerEnd.Pause();
                        MediaPlayerEnd.CurrentPosition = _endGoBackPosition;
                        _endStopPosition = -1;
                    }
                    UiUtil.ShowSubtitle(new Subtitle(_paragraphs), MediaPlayerEnd);
                }
            }
        }

        private void FormVisualSync_FormClosing(object sender, FormClosingEventArgs e)
        {
            _timerHideSyncLabel.Stop();
            labelSyncDone.Text = string.Empty;
            timer1.Stop();
            timerProgressBarRefresh.Stop();
            MediaPlayerStart?.Pause();
            MediaPlayerEnd?.Pause();

            bool change = false;
            for (int i = 0; i < _paragraphs.Count; i++)
            {
                if (_paragraphs[i].ToString() != _inputSubtitle.Paragraphs[i].ToString())
                {
                    change = true;
                    break;
                }
            }

            if (!change)
            {
                DialogResult = DialogResult.Cancel;
                return;
            }

            DialogResult dr;
            if (DialogResult == DialogResult.OK)
            {
                dr = DialogResult.Yes;
            }
            else
            {
                dr = MessageBox.Show(_language.KeepChangesMessage, _language.KeepChangesTitle, MessageBoxButtons.YesNoCancel);
            }

            if (dr == DialogResult.Cancel)
            {
                e.Cancel = true;
                timer1.Start();
                timerProgressBarRefresh.Start();
            }
            else if (dr == DialogResult.Yes)
            {
                DialogResult = DialogResult.OK;
            }
            else
            {
                DialogResult = DialogResult.Cancel;
            }
        }

        internal void Initialize(Bitmap bitmap, Subtitle subtitle, Subtitle alternate, string fileName, string title, double frameRate)
        {
            if (bitmap != null)
            {
                IntPtr iconHandle = bitmap.GetHicon();
                Icon = Icon.FromHandle(iconHandle);
            }

            _inputSubtitle = subtitle;
            _inputAlternateSubtitle = alternate;
            _oldFrameRate = frameRate;
            _subtitleFileName = fileName;
            Text = title;
        }

        private void LoadAndShowOriginalSubtitle()
        {
            _paragraphs = new List<Paragraph>();
            foreach (Paragraph p in _inputSubtitle.Paragraphs)
            {
                _paragraphs.Add(new Paragraph(p));
            }

            if (_inputAlternateSubtitle != null)
            {
                _paragraphsAlternate = new List<Paragraph>();
                foreach (Paragraph p in _inputAlternateSubtitle.Paragraphs)
                {
                    _paragraphsAlternate.Add(new Paragraph(p));
                }
            }

            FillStartAndEndTexts();

            if (comboBoxStartTexts.Items.Count > Configuration.Settings.Tools.StartSceneIndex)
            {
                comboBoxStartTexts.SelectedIndex = Configuration.Settings.Tools.StartSceneIndex;
            }
            else
            {
                comboBoxStartTexts.SelectedIndex = 0;
            }

            if (comboBoxEndTexts.Items.Count > Configuration.Settings.Tools.EndSceneIndex)
            {
                comboBoxEndTexts.SelectedIndex = comboBoxEndTexts.Items.Count - (Configuration.Settings.Tools.EndSceneIndex + 1);
            }
            else
            {
                comboBoxEndTexts.SelectedIndex = comboBoxEndTexts.Items.Count - 1;
            }
        }

        private void FillStartAndEndTexts()
        {
            comboBoxStartTexts.BeginUpdate();
            comboBoxEndTexts.BeginUpdate();
            comboBoxStartTexts.Items.Clear();
            comboBoxEndTexts.Items.Clear();
            foreach (Paragraph p in _paragraphs)
            {
                string s = p.StartTime + " - " + UiUtil.GetListViewTextFromString(p.Text);
                comboBoxStartTexts.Items.Add(s);
                comboBoxEndTexts.Items.Add(s);
            }
            comboBoxStartTexts.EndUpdate();
            comboBoxEndTexts.EndUpdate();
        }

        private void TryToFindAndOpenMovieFile(string fileNameNoExtension)
        {
            string movieFileName = null;

            foreach (string extension in Utilities.VideoFileExtensions)
            {
                movieFileName = fileNameNoExtension + extension;
                if (File.Exists(movieFileName))
                {
                    break;
                }
            }

            if (movieFileName != null && File.Exists(movieFileName))
            {
                OpenVideo(movieFileName);
            }
            else if (fileNameNoExtension.Contains('.'))
            {
                fileNameNoExtension = fileNameNoExtension.Substring(0, fileNameNoExtension.LastIndexOf('.'));
                TryToFindAndOpenMovieFile(fileNameNoExtension);
            }
        }

        private void ButtonGotoStartSubtitlePositionClick(object sender, EventArgs e)
        {
            GotoSubtitlePosition(MediaPlayerStart);
        }

        private void ButtonGotoEndSubtitlePositionClick(object sender, EventArgs e)
        {
            GotoSubtitlePosition(MediaPlayerEnd);
        }

        private void ButtonSyncClick(object sender, EventArgs e)
        {
            // Video player current start and end position.
            double videoPlayerCurrentStartPos = MediaPlayerStart.CurrentPosition;
            double videoPlayerCurrentEndPos = MediaPlayerEnd.CurrentPosition;

            // Subtitle start and end time in seconds.
            double subStart = _paragraphs[comboBoxStartTexts.SelectedIndex].StartTime.TotalMilliseconds / TimeCode.BaseUnit;
            double subEnd = _paragraphs[comboBoxEndTexts.SelectedIndex].StartTime.TotalMilliseconds / TimeCode.BaseUnit;

            // Both end times must be greater than start time.
            if (!(videoPlayerCurrentEndPos > videoPlayerCurrentStartPos && subEnd > videoPlayerCurrentStartPos))
            {
                MessageBox.Show(_language.StartSceneMustComeBeforeEndScene, "", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            SetSyncFactorLabel(videoPlayerCurrentStartPos, videoPlayerCurrentEndPos);

            double subDiff = subEnd - subStart;
            double realDiff = videoPlayerCurrentEndPos - videoPlayerCurrentStartPos;

            // speed factor
            double factor = realDiff / subDiff;

            // adjust to starting position
            double adjust = videoPlayerCurrentStartPos - subStart * factor;

            foreach (Paragraph p in _paragraphs)
            {
                p.Adjust(factor, adjust);
            }
            if (_inputAlternateSubtitle != null)
            {
                foreach (Paragraph p in _paragraphsAlternate)
                {
                    p.Adjust(factor, adjust);
                }
            }

            // fix overlapping time codes
            var tmpSubtitle = new Subtitle();
            foreach (Paragraph p in _paragraphs)
            {
                tmpSubtitle.Paragraphs.Add(new Paragraph(p));
            }

            new FixOverlappingDisplayTimes().Fix(tmpSubtitle, new EmptyFixCallback());
            _paragraphs = tmpSubtitle.Paragraphs;

            // fix overlapping time codes for alternate subtitle (translation)
            if (_inputAlternateSubtitle != null)
            {
                tmpSubtitle = new Subtitle();
                foreach (Paragraph p in _paragraphsAlternate)
                {
                    tmpSubtitle.Paragraphs.Add(new Paragraph(p));
                }

                new FixOverlappingDisplayTimes().Fix(tmpSubtitle, new EmptyFixCallback());
                _paragraphsAlternate = tmpSubtitle.Paragraphs;
            }

            // update comboboxes
            int startSaveIdx = comboBoxStartTexts.SelectedIndex;
            int endSaveIdx = comboBoxEndTexts.SelectedIndex;
            FillStartAndEndTexts();
            comboBoxStartTexts.SelectedIndex = startSaveIdx;
            comboBoxEndTexts.SelectedIndex = endSaveIdx;

            labelSyncDone.Text = _language.SynchronizationDone;
            _timerHideSyncLabel.Start();
        }

        private void GoBackSeconds(double seconds, VideoPlayerContainer mediaPlayer)
        {
            if (mediaPlayer.CurrentPosition > seconds)
            {
                mediaPlayer.CurrentPosition -= seconds;
            }
            else
            {
                mediaPlayer.CurrentPosition = 0;
            }

            UiUtil.ShowSubtitle(new Subtitle(_paragraphs), mediaPlayer);
        }

        private void ButtonStartHalfASecondBackClick(object sender, EventArgs e)
        {
            GoBackSeconds(0.5, MediaPlayerStart);
        }

        private void ButtonStartThreeSecondsBackClick(object sender, EventArgs e)
        {
            GoBackSeconds(3.0, MediaPlayerStart);
        }

        private void ButtonEndHalfASecondBackClick(object sender, EventArgs e)
        {
            GoBackSeconds(0.5, MediaPlayerEnd);
        }

        private void ButtonThreeSecondsBackClick(object sender, EventArgs e)
        {
            GoBackSeconds(3.0, MediaPlayerEnd);
        }

        private void ButtonOpenMovieClick(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(openFileDialog1.InitialDirectory) && !string.IsNullOrEmpty(_subtitleFileName))
            {
                openFileDialog1.InitialDirectory = Path.GetDirectoryName(_subtitleFileName);
            }

            openFileDialog1.Title = _languageGeneral.OpenVideoFileTitle;
            openFileDialog1.FileName = string.Empty;
            openFileDialog1.Filter = Utilities.GetVideoFileFilter(true);
            openFileDialog1.FileName = string.Empty;
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                AudioTrackNumber = -1;
                openFileDialog1.InitialDirectory = Path.GetDirectoryName(openFileDialog1.FileName);
                OpenVideo(openFileDialog1.FileName);
            }
        }

        private void SizeWmp()
        {
            MediaPlayerStart.Height = panelControlsStart.Top - (MediaPlayerStart.Top + 2);
            MediaPlayerEnd.Height = MediaPlayerStart.Height;
            MediaPlayerEnd.RefreshProgressBar();
        }

        private void FormVisualSync_Resize(object sender, EventArgs e)
        {
            int halfWidth = Width / 2;
            groupBoxStartScene.Width = halfWidth - 18;
            MediaPlayerStart.Width = groupBoxStartScene.Width - 12;
            panelControlsStart.Width = MediaPlayerStart.Width;
            groupBoxEndScene.Left = halfWidth + 3;
            groupBoxEndScene.Width = halfWidth - 18;
            MediaPlayerEnd.Width = groupBoxEndScene.Width - 12;
            SizeWmp();
            panelControlsEnd.Width = MediaPlayerEnd.Width;
            groupBoxStartScene.Height = Height - groupBoxEndScene.Top - 90;
            groupBoxEndScene.Height = Height - groupBoxEndScene.Top - 90;
            SizeWmp();
        }

        private void ButtonFindTextStartClick(object sender, EventArgs e)
        {
            using (var findSubtitle = new FindSubtitleLine())
            {
                findSubtitle.Initialize(_paragraphs, " " + "(" + _language.StartScene.ToLowerInvariant() + ")");
                findSubtitle.ShowDialog();
                if (findSubtitle.SelectedIndex >= 0)
                {
                    comboBoxStartTexts.SelectedIndex = findSubtitle.SelectedIndex;
                }
            }
        }

        private void ButtonFindTextEndClick(object sender, EventArgs e)
        {
            using (var findSubtitle = new FindSubtitleLine())
            {
                findSubtitle.Initialize(_paragraphs, " " + "(" + _language.EndScene.ToLowerInvariant() + ")");
                findSubtitle.ShowDialog();
                if (findSubtitle.SelectedIndex >= 0)
                {
                    comboBoxEndTexts.SelectedIndex = findSubtitle.SelectedIndex;
                }
            }
        }

        private void HighlightStartScene()
        {
            _isStartSceneActive = true;
            panelControlsStart.BorderStyle = BorderStyle.FixedSingle;
            panelControlsEnd.BorderStyle = BorderStyle.None;
        }

        private void HighlightEndScene()
        {
            _isStartSceneActive = false;
            panelControlsEnd.BorderStyle = BorderStyle.FixedSingle;
            panelControlsStart.BorderStyle = BorderStyle.None;
        }

        private void GroupBoxStartSceneEnter(object sender, EventArgs e)
        {
            HighlightStartScene();
        }

        private void GroupBoxEndSceneEnter(object sender, EventArgs e)
        {
            HighlightEndScene();
        }

        private void VisualSync_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                DialogResult = DialogResult.Cancel;
            }
            else if (e.KeyCode == UiUtil.HelpKeys)
            {
                Utilities.ShowHelp("#visual_sync");
            }
            else if (e.Modifiers == Keys.Control && e.KeyCode == Keys.O)
            {
                ButtonOpenMovieClick(null, null);
            }
            else if (e.Modifiers == Keys.Control && e.KeyCode == Keys.F)
            {
                if (_isStartSceneActive)
                {
                    ButtonFindTextStartClick(null, null);
                }
                else
                {
                    ButtonFindTextEndClick(null, null);
                }
            }
            else if (MediaPlayerStart != null && MediaPlayerEnd != null)
            {
                if (e.Modifiers == Keys.Control && e.KeyCode == Keys.S)
                {
                    if (_isStartSceneActive)
                    {
                        _startStopPosition = -1;
                        if (!MediaPlayerStart.IsPaused)
                        {
                            MediaPlayerStart.Pause();
                        }
                    }
                    else
                    {
                        _endStopPosition = -1;
                        if (!MediaPlayerEnd.IsPaused)
                        {
                            MediaPlayerEnd.Pause();
                        }
                    }
                }
                else if (e.Modifiers == Keys.Control && e.KeyCode == Keys.P)
                {
                    if (_isStartSceneActive)
                    {
                        _startStopPosition = -1;
                        MediaPlayerStart.TogglePlayPause();
                    }
                    else
                    {
                        _endStopPosition = -1;
                        MediaPlayerStart.TogglePlayPause();
                    }
                }
                else if (e.Modifiers == Keys.Alt && e.KeyCode == Keys.Left)
                {
                    if (_isStartSceneActive)
                    {
                        GoBackSeconds(0.5, MediaPlayerStart);
                    }
                    else
                    {
                        GoBackSeconds(0.5, MediaPlayerEnd);
                    }

                    e.SuppressKeyPress = true;
                }
                else if (e.Modifiers == Keys.Alt && e.KeyCode == Keys.Right)
                {
                    if (_isStartSceneActive)
                    {
                        GoBackSeconds(-0.5, MediaPlayerStart);
                    }
                    else
                    {
                        GoBackSeconds(-0.5, MediaPlayerEnd);
                    }

                    e.SuppressKeyPress = true;
                }
                else if (e.Modifiers == Keys.Control && e.KeyCode == Keys.Left)
                {
                    if (_isStartSceneActive)
                    {
                        GoBackSeconds(0.1, MediaPlayerStart);
                    }
                    else
                    {
                        GoBackSeconds(0.1, MediaPlayerEnd);
                    }

                    e.SuppressKeyPress = true;
                }
                else if (e.Modifiers == Keys.Control && e.KeyCode == Keys.Right)
                {
                    if (_isStartSceneActive)
                    {
                        GoBackSeconds(-0.1, MediaPlayerStart);
                    }
                    else
                    {
                        GoBackSeconds(-0.1, MediaPlayerEnd);
                    }

                    e.SuppressKeyPress = true;
                }
                else if (e.Modifiers == (Keys.Control | Keys.Shift) && e.KeyCode == Keys.Right)
                {
                    if (_isStartSceneActive)
                    {
                        GoBackSeconds(1.0, MediaPlayerStart);
                    }
                    else
                    {
                        GoBackSeconds(1.0, MediaPlayerEnd);
                    }

                    e.SuppressKeyPress = true;
                }
                else if (e.Modifiers == (Keys.Control | Keys.Shift) && e.KeyCode == Keys.Left)
                {
                    if (_isStartSceneActive)
                    {
                        GoBackSeconds(-1.0, MediaPlayerStart);
                    }
                    else
                    {
                        GoBackSeconds(-1.0, MediaPlayerEnd);
                    }

                    e.SuppressKeyPress = true;
                }
                else if (e.Modifiers == Keys.None && e.KeyCode == Keys.Space)
                {
                    if (_isStartSceneActive)
                    {
                        MediaPlayerStart.TogglePlayPause();
                    }
                    else
                    {
                        MediaPlayerEnd.TogglePlayPause();
                    }

                    e.SuppressKeyPress = true;
                }
            }
        }

        private void VisualSync_Shown(object sender, EventArgs e)
        {
            comboBoxStartTexts.Focus();
        }

        private void ButtonStartVerifyClick(object sender, EventArgs e)
        {
            if (MediaPlayerStart != null && MediaPlayerStart.VideoPlayer != null)
            {
                _startGoBackPosition = MediaPlayerStart.CurrentPosition;
                _startStopPosition = _startGoBackPosition + Configuration.Settings.Tools.VerifyPlaySeconds;
                MediaPlayerStart.Play();
            }
        }

        private void ButtonEndVerifyClick(object sender, EventArgs e)
        {
            if (MediaPlayerEnd != null && MediaPlayerEnd.VideoPlayer != null)
            {
                _endGoBackPosition = MediaPlayerEnd.CurrentPosition;
                _endStopPosition = _endGoBackPosition + Configuration.Settings.Tools.VerifyPlaySeconds;
                MediaPlayerEnd.Play();
            }
        }

        private void VisualSync_Load(object sender, EventArgs e)
        {
            LoadAndShowOriginalSubtitle();
            if (!string.IsNullOrEmpty(VideoFileName) && File.Exists(VideoFileName))
            {
                OpenVideo(VideoFileName);
            }
            else if (!string.IsNullOrEmpty(_subtitleFileName))
            {
                TryToFindAndOpenMovieFile(Path.GetDirectoryName(_subtitleFileName) + Path.DirectorySeparatorChar + Path.GetFileNameWithoutExtension(_subtitleFileName));
            }

            FormVisualSync_Resize(null, null);
        }

        private void MediaPlayerStart_OnButtonClicked(object sender, EventArgs e)
        {
            if (!_isStartSceneActive)
            {
                HighlightStartScene();
            }
        }

        private void MediaPlayerEnd_OnButtonClicked(object sender, EventArgs e)
        {
            if (_isStartSceneActive)
            {
                HighlightEndScene();
            }
        }

        private void VisualSync_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (MediaPlayerStart.VideoPlayer != null) // && MediaPlayerStart.VideoPlayer.GetType() == typeof(QuartsPlayer))
            {
                MediaPlayerStart.VideoPlayer.Pause();
                MediaPlayerStart.VideoPlayer.DisposeVideoPlayer();
            }
            if (MediaPlayerEnd.VideoPlayer != null) // && MediaPlayerEnd.VideoPlayer.GetType() == typeof(QuartsPlayer))
            {
                MediaPlayerEnd.VideoPlayer.Pause();
                MediaPlayerEnd.VideoPlayer.DisposeVideoPlayer();
            }
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            OkPressed = true;
        }

        private void timerProgressBarRefresh_Tick(object sender, EventArgs e)
        {
            timerProgressBarRefresh.Stop();
            if (MediaPlayerStart.VideoPlayer != null) // && MediaPlayerStart.VideoPlayer.GetType() == typeof(QuartsPlayer))
            {
                MediaPlayerStart.RefreshProgressBar();
            }
            if (MediaPlayerEnd.VideoPlayer != null) // && MediaPlayerEnd.VideoPlayer.GetType() == typeof(QuartsPlayer))
            {
                MediaPlayerEnd.RefreshProgressBar();
            }
            timerProgressBarRefresh.Start();
        }

        private void SetSyncFactorLabel(double videoPlayerCurrentStartPos, double videoPlayerCurrentEndPos)
        {
            if (string.IsNullOrWhiteSpace(VideoFileName))
            {
                return;
            }

            _adjustInfo = string.Empty;
            if (videoPlayerCurrentEndPos > videoPlayerCurrentStartPos)
            {
                double subStart = _paragraphs[comboBoxStartTexts.SelectedIndex].StartTime.TotalMilliseconds / TimeCode.BaseUnit;
                double subEnd = _paragraphs[comboBoxEndTexts.SelectedIndex].StartTime.TotalMilliseconds / TimeCode.BaseUnit;

                double subDiff = subEnd - subStart;
                double realDiff = videoPlayerCurrentEndPos - videoPlayerCurrentStartPos;

                // speed factor
                double factor = realDiff / subDiff;

                // adjust to starting position
                double adjust = videoPlayerCurrentStartPos - subStart * factor;

                if (Math.Abs(adjust) > 0.001 || (Math.Abs(1 - factor)) > 0.001)
                {
                    _adjustInfo = string.Format("*{0:0.000}, {1:+0.000;-0.000}", factor, adjust);
                }
            }
        }

    }
}
