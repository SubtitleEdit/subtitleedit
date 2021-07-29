using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.VideoPlayers;
using System;
using System.IO;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.Forms
{
    public sealed partial class SetSyncPoint : Form
    {
        private double _lastPosition;
        private TimeSpan _guess;
        private double _goBackPosition;
        private double _stopPosition = -1.0;
        private Subtitle _subtitle;
        private int _audioTrackNumber = -1;
        private readonly Keys _mainGeneralGoToNextSubtitle = UiUtil.GetKeys(Configuration.Settings.Shortcuts.GeneralGoToNextSubtitle);
        private readonly Keys _mainGeneralGoToNextSubtitlePlayTranslate = UiUtil.GetKeys(Configuration.Settings.Shortcuts.GeneralGoToNextSubtitlePlayTranslate);
        private readonly Keys _mainGeneralGoToPrevSubtitle = UiUtil.GetKeys(Configuration.Settings.Shortcuts.GeneralGoToPrevSubtitle);
        private readonly Keys _mainGeneralGoToPrevSubtitlePlayTranslate = UiUtil.GetKeys(Configuration.Settings.Shortcuts.GeneralGoToPrevSubtitlePlayTranslate);
        private string _subtitleFileName;
        public string VideoFileName { get; private set; }

        public SetSyncPoint()
        {
            UiUtil.PreInitialize(this);
            InitializeComponent();
            UiUtil.FixFonts(this);

            groupBoxSyncPointTimeCode.Text = LanguageSettings.Current.SetSyncPoint.SyncPointTimeCode;
            buttonThreeSecondsBack.Text = LanguageSettings.Current.SetSyncPoint.ThreeSecondsBack;
            buttonHalfASecondBack.Text = LanguageSettings.Current.SetSyncPoint.HalfASecondBack;
            buttonVerify.Text = string.Format(LanguageSettings.Current.VisualSync.PlayXSecondsAndBack, Configuration.Settings.Tools.VerifyPlaySeconds);
            buttonHalfASecondAhead.Text = LanguageSettings.Current.SetSyncPoint.HalfASecondForward;
            buttonThreeSecondsAhead.Text = LanguageSettings.Current.SetSyncPoint.ThreeSecondsForward;
            buttonOpenMovie.Text = LanguageSettings.Current.General.OpenVideoFile;
            buttonSetSyncPoint.Text = LanguageSettings.Current.PointSync.SetSyncPoint;
            buttonCancel.Text = LanguageSettings.Current.General.Cancel;
            subtitleListView1.InitializeLanguage(LanguageSettings.Current.General, Configuration.Settings);
            UiUtil.InitializeSubtitleFont(subtitleListView1);
            subtitleListView1.AutoSizeAllColumns(this);
            buttonFindTextEnd.Text = LanguageSettings.Current.VisualSync.FindText;
            UiUtil.FixLargeFonts(this, buttonSetSyncPoint);
        }

        public TimeSpan SynchronizationPoint => timeUpDownLine.TimeCode.TimeSpan;

        public void Initialize(Subtitle subtitle, string subtitleFileName, int index, string videoFileName, int audioTrackNumber)
        {
            _subtitleFileName = subtitleFileName;
            _subtitle = subtitle;
            _audioTrackNumber = audioTrackNumber;
            subtitleListView1.Fill(subtitle);
            _guess = subtitle.Paragraphs[index].StartTime.TimeSpan;
            subtitleListView1.Items[index].Selected = true;
            Text = string.Format(LanguageSettings.Current.SetSyncPoint.Title, subtitle.Paragraphs[index].Number + ": " + subtitle.Paragraphs[index]);
            labelSubtitle.Text = string.Empty;
            labelVideoFileName.Text = LanguageSettings.Current.General.NoVideoLoaded;

            timeUpDownLine.TimeCode = subtitle.Paragraphs[index].StartTime;

            if (!string.IsNullOrEmpty(videoFileName) && File.Exists(videoFileName))
            {
                OpenVideo(videoFileName);
            }
            else if (!string.IsNullOrEmpty(subtitleFileName))
            {
                TryToFindAndOpenVideoFile(Path.GetDirectoryName(subtitleFileName) + Path.DirectorySeparatorChar +
                                          Path.GetFileNameWithoutExtension(subtitleFileName));
            }
        }

        private void TryToFindAndOpenVideoFile(string fileNameNoExtension)
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
                TryToFindAndOpenVideoFile(fileNameNoExtension);
            }
        }

        private void buttonOpenMovie_Click(object sender, EventArgs e)
        {
            openFileDialog1.Title = LanguageSettings.Current.General.OpenVideoFileTitle;
            openFileDialog1.FileName = string.Empty;
            openFileDialog1.Filter = UiUtil.GetVideoFileFilter(false);
            if (File.Exists(_subtitleFileName))
            {
                var videoFileExt = UiUtil.GetVideoFileFilter(false);
                var folder = Path.GetDirectoryName(_subtitleFileName);
                if (folder != null)
                {
                    foreach (var file in Directory.GetFiles(folder))
                    {
                        if (file.EndsWith(".nfo", StringComparison.OrdinalIgnoreCase)
                            || file.EndsWith(".srt", StringComparison.OrdinalIgnoreCase)
                            || file.EndsWith(".sub", StringComparison.OrdinalIgnoreCase))
                        {
                            continue;
                        }

                        if (videoFileExt.Contains(Path.GetExtension(file)))
                        {
                            openFileDialog1.InitialDirectory = Path.GetDirectoryName(file);
                            break;
                        }
                    }
                }
            }
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                _audioTrackNumber = -1;
                openFileDialog1.InitialDirectory = Path.GetDirectoryName(openFileDialog1.FileName);
                OpenVideo(openFileDialog1.FileName);
            }
        }

        private void OpenVideo(string fileName)
        {
            if (File.Exists(fileName))
            {
                var fi = new FileInfo(fileName);
                if (fi.Length < 1000)
                {
                    return;
                }

                labelVideoFileName.Text = fileName;
                VideoFileName = fileName;
                if (videoPlayerContainer1.VideoPlayer != null)
                {
                    videoPlayerContainer1.Pause();
                    videoPlayerContainer1.VideoPlayer.DisposeVideoPlayer();
                }

                VideoInfo videoInfo = UiUtil.GetVideoInfo(fileName);

                UiUtil.InitializeVideoPlayerAndContainer(fileName, videoInfo, videoPlayerContainer1, VideoStartLoaded, VideoStartEnded);
            }
        }

        private void VideoStartEnded(object sender, EventArgs e)
        {
            videoPlayerContainer1.Pause();
        }

        private void VideoStartLoaded(object sender, EventArgs e)
        {
            timer1.Start();

            videoPlayerContainer1.Pause();

            if (_guess.TotalMilliseconds > 0 && _guess.TotalMilliseconds / TimeCode.BaseUnit < videoPlayerContainer1.VideoPlayer.Duration)
            {
                videoPlayerContainer1.VideoPlayer.CurrentPosition = _guess.TotalMilliseconds / TimeCode.BaseUnit;
                videoPlayerContainer1.RefreshProgressBar();
            }

            if (_audioTrackNumber >= 0 && videoPlayerContainer1.VideoPlayer is LibVlcDynamic libVlc)
            {
                libVlc.AudioTrackNumber = _audioTrackNumber;
            }
            else if (_audioTrackNumber >= 0 && videoPlayerContainer1.VideoPlayer is LibMpvDynamic libMpv)
            {
                libMpv.AudioTrackNumber = _audioTrackNumber;
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (videoPlayerContainer1 != null)
            {
                double pos;

                if (_stopPosition >= 0 && videoPlayerContainer1.CurrentPosition > _stopPosition)
                {
                    videoPlayerContainer1.Pause();
                    videoPlayerContainer1.CurrentPosition = _goBackPosition;
                    _stopPosition = -1;
                }

                if (!videoPlayerContainer1.IsPaused)
                {
                    videoPlayerContainer1.RefreshProgressBar();
                    pos = videoPlayerContainer1.CurrentPosition;
                }
                else
                {
                    pos = videoPlayerContainer1.CurrentPosition;
                }
                if (Math.Abs(pos - _lastPosition) > 0.01)
                {
                    UiUtil.ShowSubtitle(_subtitle, videoPlayerContainer1);
                    timeUpDownLine.TimeCode = TimeCode.FromSeconds(pos);
                    _lastPosition = pos;
                }
            }
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void GetTime_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                DialogResult = DialogResult.Cancel;
            }
            else if (e.KeyCode == Keys.S && e.Modifiers == Keys.Control)
            {
                videoPlayerContainer1.Pause();
                e.SuppressKeyPress = true;
            }
            else if (e.KeyCode == Keys.P && e.Control)
            {
                videoPlayerContainer1.VideoPlayer.Pause();
                e.SuppressKeyPress = true;
            }
            else if (e.Modifiers == Keys.Alt && e.KeyCode == Keys.Left)
            {
                GoBackSeconds(0.5, videoPlayerContainer1.VideoPlayer);
                e.SuppressKeyPress = true;
            }
            else if (e.Modifiers == Keys.Alt && e.KeyCode == Keys.Right)
            {
                GoBackSeconds(-0.5, videoPlayerContainer1.VideoPlayer);
                e.SuppressKeyPress = true;
            }
            else if (e.Modifiers == Keys.Control && e.KeyCode == Keys.Left)
            {
                GoBackSeconds(0.1, videoPlayerContainer1.VideoPlayer);
                e.SuppressKeyPress = true;
            }
            else if (e.Modifiers == Keys.Control && e.KeyCode == Keys.Right)
            {
                GoBackSeconds(-0.1, videoPlayerContainer1.VideoPlayer);
                e.SuppressKeyPress = true;
            }
            else if (e.KeyData == UiUtil.HelpKeys)
            {
                UiUtil.ShowHelp("#sync");
                e.SuppressKeyPress = true;
            }
            else if (_mainGeneralGoToNextSubtitle == e.KeyData || _mainGeneralGoToNextSubtitlePlayTranslate == e.KeyData)
            {
                int selectedIndex = 0;
                if (subtitleListView1.SelectedItems.Count > 0)
                {
                    selectedIndex = subtitleListView1.SelectedItems[0].Index;
                    selectedIndex++;
                }
                subtitleListView1.SelectIndexAndEnsureVisible(selectedIndex);
                e.SuppressKeyPress = true;
            }
            else if (_mainGeneralGoToPrevSubtitle == e.KeyData || _mainGeneralGoToPrevSubtitlePlayTranslate == e.KeyData)
            {
                int selectedIndex = 0;
                if (subtitleListView1.SelectedItems.Count > 0)
                {
                    selectedIndex = subtitleListView1.SelectedItems[0].Index;
                    selectedIndex--;
                }
                subtitleListView1.SelectIndexAndEnsureVisible(selectedIndex);
                e.SuppressKeyPress = true;
            }
        }

        private void GetTime_FormClosing(object sender, FormClosingEventArgs e)
        {
            timer1.Stop();
            videoPlayerContainer1?.Pause();
        }

        private void GetTime_FormClosed(object sender, FormClosedEventArgs e)
        {
            videoPlayerContainer1.VideoPlayer?.DisposeVideoPlayer();
        }

        private void GoBackSeconds(double seconds, VideoPlayer mediaPlayer)
        {
            if (mediaPlayer != null)
            {
                if (mediaPlayer.CurrentPosition > seconds)
                {
                    mediaPlayer.CurrentPosition -= seconds;
                }
                else
                {
                    mediaPlayer.CurrentPosition = 0;
                }

                videoPlayerContainer1.RefreshProgressBar();
            }
        }

        private void buttonStartHalfASecondBack_Click(object sender, EventArgs e)
        {
            GoBackSeconds(0.5, videoPlayerContainer1.VideoPlayer);
        }

        private void buttonStartThreeSecondsBack_Click(object sender, EventArgs e)
        {
            GoBackSeconds(3, videoPlayerContainer1.VideoPlayer);
        }

        private void buttonStartThreeSecondsAhead_Click(object sender, EventArgs e)
        {
            GoBackSeconds(-3.0, videoPlayerContainer1.VideoPlayer);
        }

        private void buttonStartHalfASecondAhead_Click(object sender, EventArgs e)
        {
            GoBackSeconds(-0.5, videoPlayerContainer1.VideoPlayer);
        }

        private void buttonStartVerify_Click(object sender, EventArgs e)
        {
            if (videoPlayerContainer1?.VideoPlayer != null)
            {
                _goBackPosition = videoPlayerContainer1.CurrentPosition;
                _stopPosition = _goBackPosition + Configuration.Settings.Tools.VerifyPlaySeconds;
                videoPlayerContainer1.Play();
            }
        }

        private void GetTimeLoad(object sender, EventArgs e)
        {
            if (subtitleListView1.SelectedItems.Count == 1)
            {
                subtitleListView1.SelectIndexAndEnsureVisible(subtitleListView1.SelectedItems[0].Index);
            }
        }

        private void SubtitleListView1MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (subtitleListView1.SelectedItems.Count == 1)
            {
                int index = subtitleListView1.SelectedItems[0].Index;

                videoPlayerContainer1.Pause();
                videoPlayerContainer1.CurrentPosition = _subtitle.Paragraphs[index].StartTime.TotalMilliseconds / TimeCode.BaseUnit;
            }
        }

        private void ButtonFindTextEndClick(object sender, EventArgs e)
        {
            using (var findSubtitle = new FindSubtitleLine())
            {
                findSubtitle.Initialize(_subtitle.Paragraphs, string.Empty);
                findSubtitle.ShowDialog();
                if (findSubtitle.SelectedIndex >= 0)
                {
                    subtitleListView1.SelectIndexAndEnsureVisible(findSubtitle.SelectedIndex);
                }
            }
        }

        private void timeUpDownLine_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                buttonOK_Click(sender, e);
            }
        }
    }
}
