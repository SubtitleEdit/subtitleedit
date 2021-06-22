using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.VideoPlayers;
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.Forms.Assa
{
    public sealed partial class SetPosition : Form
    {
        public Subtitle UpdatedSubtitle { get; private set; }
        private readonly Subtitle _subtitle;
        private readonly Subtitle _subtitleWithNewHeader;
        private readonly int[] _selectedIndices;
        private LibMpvDynamic _mpv;
        private string _mpvTextFileName;
        private bool _closing;
        private bool _videoLoaded;
        private int _x = -1;
        private int _y = -1;
        private int _tempX;
        private int _tempY;
        private bool _updatePos = true;
        private readonly string _videoFileName;
        private readonly VideoInfo _videoInfo;
        private bool _positionChanged;

        public SetPosition(Subtitle subtitle, int[] selectedIndices, string videoFileName, VideoInfo videoInfo)
        {
            UiUtil.PreInitialize(this);
            InitializeComponent();
            UiUtil.FixFonts(this);

            _subtitle = subtitle;
            _videoFileName = videoFileName;
            _videoInfo = videoInfo;
            _subtitleWithNewHeader = new Subtitle(_subtitle, false);
            _selectedIndices = selectedIndices;
            radioButtonSelectedLines.Checked = true;
            Text = LanguageSettings.Current.AssaSetPosition.SetPosition;
            radioButtonSelectedLines.Text = string.Format(LanguageSettings.Current.AssaOverrideTags.SelectedLinesX, _selectedIndices.Length);
            radioButtonClipboard.Text = LanguageSettings.Current.AssaSetPosition.Clipboard;
            labelInfo.Text = LanguageSettings.Current.AssaSetPosition.SetPosInfo;
            groupBoxPreview.Text = LanguageSettings.Current.General.Preview;
            buttonOK.Text = LanguageSettings.Current.General.Ok;
            buttonCancel.Text = LanguageSettings.Current.General.Cancel;
            UiUtil.FixLargeFonts(this, buttonOK);

            if (_videoInfo == null)
            {
                _videoInfo = UiUtil.GetVideoInfo(_videoFileName);
            }

            labelVideoResolution.Text = string.Format(LanguageSettings.Current.AssaSetPosition.VideoResolutionX, $"{_videoInfo.Width}x{_videoInfo.Height}");
            SetPosition_ResizeEnd(null, null);

            if (Configuration.Settings.Tools.AssaSetPositionTarget == "Clipboard")
            {
                radioButtonClipboard.Checked = true;
            }
            else
            {
                radioButtonSelectedLines.Checked = true;
            }
        }

        private void ApplyCustomStyles_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                DialogResult = DialogResult.Cancel;
            }
            else if (e.KeyCode == Keys.Enter)
            {
                buttonOK_Click(null, null);
            }
            else if (e.KeyData == UiUtil.HelpKeys)
            {
                UiUtil.OpenUrl("https://www.nikse.dk/SubtitleEdit/AssaOverrideTags#pos");
                e.SuppressKeyPress = true;
            }
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            _subtitle.Header = _subtitleWithNewHeader.Header;
            if (_positionChanged)
            {
                ApplyOverrideTags(_subtitle);
            }

            DialogResult = DialogResult.OK;
        }

        private void ApplyOverrideTags(Subtitle subtitle)
        {
            var styleToApply = $"{{\\pos({_x},{_y})}}";

            if (radioButtonClipboard.Checked)
            {
                Clipboard.SetText(styleToApply);
                return;
            }

            UpdatedSubtitle = new Subtitle(subtitle, false);
            var indices = GetIndices(subtitle);

            for (int i = 0; i < UpdatedSubtitle.Paragraphs.Count; i++)
            {
                if (!indices.Contains(i))
                {
                    continue;
                }

                var p = UpdatedSubtitle.Paragraphs[i];

                // remove old position tags
                p.Text = Regex.Replace(p.Text, @"{\\pos(.*)}", string.Empty);
                p.Text = Regex.Replace(p.Text, @"\\pos(.*)", string.Empty);

                if (p.Text.StartsWith("{\\", StringComparison.Ordinal) && styleToApply.EndsWith('}'))
                {
                    p.Text = styleToApply.TrimEnd('}') + p.Text.Remove(0, 1);
                }
                else
                {
                    p.Text = styleToApply + p.Text;
                }
            }
        }

        private int[] GetIndices(Subtitle subtitle)
        {
            return _selectedIndices;
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private bool GeneratePreviewViaMpv()
        {
            var fileName = _videoFileName; // Path.Combine(Configuration.DataDirectory, "preview.mp4");
            if (!File.Exists(fileName))
            {
                var isFfmpegAvailable = !Configuration.IsRunningOnWindows || !string.IsNullOrEmpty(Configuration.Settings.General.FFmpegLocation) && File.Exists(Configuration.Settings.General.FFmpegLocation);
                if (!isFfmpegAvailable)
                {
                    return false;
                }

                using (var p = GetFFmpegProcess(fileName))
                {
                    p.Start();
                    p.WaitForExit();
                }
            }

            if (!LibMpvDynamic.IsInstalled)
            {
                return false;
            }

            if (_mpv == null)
            {
                _mpv = new LibMpvDynamic();
                _mpv.Initialize(pictureBoxPreview, fileName, VideoLoaded, null);
            }
            else
            {
                VideoLoaded(null, null);
            }

            return true;
        }

        public static Process GetFFmpegProcess(string outputFileName)
        {
            var ffmpegLocation = Configuration.Settings.General.FFmpegLocation;
            if (!Configuration.IsRunningOnWindows && (string.IsNullOrEmpty(ffmpegLocation) || !File.Exists(ffmpegLocation)))
            {
                ffmpegLocation = "ffmpeg";
            }

            return new Process
            {
                StartInfo =
                {
                    FileName = ffmpegLocation,
                    Arguments = $"-t 1 -f lavfi -i color=c=blue:s=720x480 -c:v libx264 -tune stillimage -pix_fmt yuv420p \"{outputFileName}\"",
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };
        }

        private void VideoLoaded(object sender, EventArgs e)
        {
            var format = new AdvancedSubStationAlpha();
            var subtitle = new Subtitle();
            var indices = GetIndices(subtitle);
            var styleToApply = $"{{\\pos({_x},{_y})}}";
            var p = indices.Length > 0 ? 
                new Paragraph(_subtitleWithNewHeader.Paragraphs[indices[0]]) : 
                new Paragraph(Configuration.Settings.General.PreviewAssaText, 0, 1000);

            // remove old position tags
            p.Text = Regex.Replace(p.Text, @"{\\pos(.*)}", string.Empty);
            p.Text = Regex.Replace(p.Text, @"\\pos(.*)", string.Empty);

            // remove fade tags 
            p.Text = Regex.Replace(p.Text, @"{\\fad(.*)}", string.Empty);
            p.Text = Regex.Replace(p.Text, @"\\fade(.*)", string.Empty);

            p.Text = styleToApply + p.Text;
            subtitle.Paragraphs.Add(p);

            subtitle.Header = _subtitleWithNewHeader.Header ?? AdvancedSubStationAlpha.DefaultHeader;
            var text = subtitle.ToText(format);
            _mpvTextFileName = FileUtil.GetTempFileName(format.Extension);
            File.WriteAllText(_mpvTextFileName, text);
            _mpv.LoadSubtitle(_mpvTextFileName);

            if (!_videoLoaded)
            {
                Application.DoEvents();
                _mpv.Pause();
                _mpv.CurrentPosition = p.StartTime.TotalSeconds + 0.05;
                Application.DoEvents();
                _videoLoaded = true;
                timer1.Start();
            }
        }

        private void ApplyCustomStyles_FormClosing(object sender, FormClosingEventArgs e)
        {
            _closing = true;
            Application.DoEvents();
            Configuration.Settings.Tools.AssaSetPositionTarget = radioButtonClipboard.Checked ? "Clipboard" : "SelectedLines";
            _mpv?.Dispose();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (_mpv == null || _closing)
            {
                return;
            }


            if (_updatePos && (_x != _tempX || _y != _tempY))
            {
                _x = _tempX;
                _y = _tempY;
                VideoLoaded(null, null);
            }
        }

        private void SetPosition_Shown(object sender, EventArgs e)
        {
            ShowStyleAlignment();
            ShowCurrentPosition();

            var playResX = AdvancedSubStationAlpha.GetTagFromHeader("PlayResX", "[Script Info]", _subtitleWithNewHeader.Header);
            var playResY = AdvancedSubStationAlpha.GetTagFromHeader("PlayResY", "[Script Info]", _subtitleWithNewHeader.Header);
            if (string.IsNullOrEmpty(playResX) || string.IsNullOrEmpty(playResY))
            {
                var dialogResult = MessageBox.Show("PlayResX/PlayResY are not set - set the resolution now?", "Subtitle Edit", MessageBoxButtons.YesNoCancel);
                if (dialogResult == DialogResult.OK || dialogResult == DialogResult.Yes)
                {
                    _subtitleWithNewHeader.Header = AdvancedSubStationAlpha.AddTagToHeader("PlayResX", "PlayResX: " + _videoInfo.Width.ToString(CultureInfo.InvariantCulture), "[Script Info]", _subtitleWithNewHeader.Header);
                    _subtitleWithNewHeader.Header = AdvancedSubStationAlpha.AddTagToHeader("PlayResY", "PlayResY: " + _videoInfo.Height.ToString(CultureInfo.InvariantCulture), "[Script Info]", _subtitleWithNewHeader.Header);
                }
            }

            GeneratePreviewViaMpv();
        }

        private void ShowCurrentPosition()
        {
            var indices = GetIndices(_subtitle);
            if (indices.Length == 0)
            {
                return;
            }

            var p = _subtitleWithNewHeader.Paragraphs[indices[0]];
            var match = Regex.Match(p.Text, @"\\pos(.*)");
            if (match.Success)
            {
                var arr = match.Value.Split('(', ')', ',');
                if (arr.Length > 2)
                {
                    if (int.TryParse(arr[1], out var x) && int.TryParse(arr[2], out var y))
                    {
                        _x = x;
                        _y = y;
                        _updatePos = false;
                        labelCurrentTextPosition.Text = string.Format(LanguageSettings.Current.AssaSetPosition.CurrentTextPositionX, $"{_x},{_y}");
                    }
                }
            }
        }

        private void ShowStyleAlignment()
        {
            var indices = GetIndices(_subtitle);
            if (indices.Length == 0)
            {
                labelStyleAlignment.Text = string.Format(LanguageSettings.Current.AssaSetPosition.StyleAlignmentX, "{\\an2}");

                return;
            }

            var p = _subtitleWithNewHeader.Paragraphs[indices[0]];
            var style = AdvancedSubStationAlpha.GetSsaStyle(p.Extra, _subtitleWithNewHeader.Header);
            labelStyleAlignment.Text = string.Format(LanguageSettings.Current.AssaSetPosition.StyleAlignmentX, "{\\an" + style.Alignment + "} (" + p.Extra + ")");
        }

        private void pictureBoxPreview_Click(object sender, EventArgs e)
        {
            _x = _tempX;
            _y = _tempY;
            labelCurrentTextPosition.Text = string.Format(LanguageSettings.Current.AssaSetPosition.CurrentTextPositionX, $"{_x},{_y}");
            _updatePos = !_updatePos;
            VideoLoaded(null, null);
            _positionChanged = true;
        }

        private void pictureBoxPreview_MouseMove(object sender, MouseEventArgs e)
        {
            var xAspectRatio = (double)_videoInfo.Width / pictureBoxPreview.Width;
            _tempX = (int)Math.Round(e.Location.X * xAspectRatio);

            var yAspectRatio = (double)_videoInfo.Height / pictureBoxPreview.Height;
            _tempY = (int)Math.Round(e.Location.Y * yAspectRatio);

            labelCurrentPosition.Text = string.Format(LanguageSettings.Current.AssaSetPosition.CurrentMousePositionX, $"{_tempX},{_tempY}");
        }

        private void SetPosition_ResizeEnd(object sender, EventArgs e)
        {
            var aspectRatio = (double)_videoInfo.Width / _videoInfo.Height;
            var newWidth = pictureBoxPreview.Height * aspectRatio;
            Width += (int)(newWidth - pictureBoxPreview.Width);
        }
    }
}
