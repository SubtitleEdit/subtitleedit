using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Forms.Options;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.VideoPlayers;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.Forms.Assa
{
    public sealed partial class AssSetBackground : Form
    {
        public Subtitle UpdatedSubtitle { get; private set; }
        private readonly Subtitle _subtitle;
        private readonly Subtitle _subtitleWithNewHeader;
        private readonly int[] _selectedIndices;
        private LibMpvDynamic _mpv;
        private string _mpvTextFileName;
        private bool _closing;
        private bool _videoLoaded;
        private bool _updatePreview = true;
        private readonly string _videoFileName;
        private readonly VideoInfo _videoInfo;
        private bool _loading = true;
        private static readonly Regex FrameFinderRegex = new Regex(@"[Ff]rame=\s*\d+", RegexOptions.Compiled);
        private long _processedFrames;
        private string _assaBox;

        private int _top;
        private int _bottom;
        private int _left;
        private int _right;
        private Color _boxColor;
        private Color _boxShadowColor;
        private Color _boxOutlineColor;


        public AssSetBackground(Subtitle subtitle, int[] selectedIndices, string videoFileName, VideoInfo videoInfo)
        {
            UiUtil.PreInitialize(this);
            InitializeComponent();
            UiUtil.FixFonts(this);

            _subtitle = subtitle;
            _videoFileName = videoFileName;
            _videoInfo = videoInfo;

            _subtitleWithNewHeader = new Subtitle(_subtitle, false);
            if (_subtitleWithNewHeader.Header == null)
            {
                _subtitleWithNewHeader.Header = AdvancedSubStationAlpha.DefaultHeader;
            }

            _selectedIndices = selectedIndices;
           // Text = LanguageSettings.Current.AssaSetPosition.SetPosition;
            groupBoxPreview.Text = LanguageSettings.Current.General.Preview;
            
            comboBoxBoxStyle.Items.Clear();
            comboBoxBoxStyle.Items.Add(LanguageSettings.Current.AssaProgressBarGenerator.SquareCorners);
            comboBoxBoxStyle.Items.Add(LanguageSettings.Current.AssaProgressBarGenerator.RoundedCorners);

            buttonOK.Text = LanguageSettings.Current.General.Ok;
            buttonCancel.Text = LanguageSettings.Current.General.Cancel;

            comboBoxBoxStyle.SelectedIndex = 0;
            
            panelPrimaryColor.BackColor = Color.Black;
            _boxColor = panelPrimaryColor.BackColor;
            panelOutlineColor.BackColor = Color.Gray;
            _boxOutlineColor = panelOutlineColor.BackColor;
            panelShadowColor.BackColor = Color.Black;
            _boxShadowColor = panelShadowColor.BackColor;

            UiUtil.FixLargeFonts(this, buttonOK);

            if (_videoInfo == null)
            {
                _videoInfo = UiUtil.GetVideoInfo(_videoFileName);
            }

            SetPosition_ResizeEnd(null, null);
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
            DialogResult = DialogResult.OK;
        }

        private int[] GetIndices()
        {
            return _selectedIndices;
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void GeneratePreviewViaMpv()
        {
            var fileName = _videoFileName;
            if (!File.Exists(fileName))
            {
                var isFfmpegAvailable = !Configuration.IsRunningOnWindows || !string.IsNullOrEmpty(Configuration.Settings.General.FFmpegLocation) && File.Exists(Configuration.Settings.General.FFmpegLocation);
                if (!isFfmpegAvailable)
                {
                    return;
                }

                using (var p = GetFFmpegProcess(fileName))
                {
                    p.Start();
                    p.WaitForExit();
                }
            }

            if (!LibMpvDynamic.IsInstalled)
            {
                if (MessageBox.Show("Download and use \"mpv\" as video player?", "Subtitle Edit", MessageBoxButtons.YesNoCancel) == DialogResult.Yes)
                {
                    using (var form = new SettingsMpv(!LibMpvDynamic.IsInstalled))
                    {
                        if (form.ShowDialog(this) != DialogResult.OK)
                        {
                            return;
                        }

                        Configuration.Settings.General.VideoPlayer = "MPV";
                    }
                }
            }

            if (!LibMpvDynamic.IsInstalled)
            {
                return;
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
            var indices = GetIndices();
            var styleToApply = string.Empty; // $"{{\\pos({_x},{_y})}}";

            var p = indices.Length > 0 ?
                new Paragraph(_subtitleWithNewHeader.Paragraphs[indices[0]]) :
                new Paragraph(Configuration.Settings.General.PreviewAssaText, 0, 1000);

            // remove fade tags 
            p.Text = Regex.Replace(p.Text, @"{\\fad\([\d\.,]*\)}", string.Empty);
            p.Text = Regex.Replace(p.Text, @"\\fad\([\d\.,]*\)", string.Empty);
            p.Text = Regex.Replace(p.Text, @"{\\fade\([\d\.,]*\)}", string.Empty);
            p.Text = Regex.Replace(p.Text, @"\\fade\([\d\.,]*\)", string.Empty);

            p.Text = styleToApply + p.Text;
            subtitle.Paragraphs.Add(p);

            // box
            //Build box + gen preview via mpv
            _assaBox = GenerateBackgroundBox(
                _left - (int)numericUpDownMarginLeft.Value,
                _top - (int)numericUpDownMarginTop.Value,
                _right + (int)numericUpDownMarginRight.Value,
                _bottom + (int)numericUpDownMarginBottom.Value);

            var p2 = new Paragraph(_assaBox?? string.Empty, 0, 1000);
            p2.StartTime.TotalMilliseconds = p.StartTime.TotalMilliseconds;
            p2.EndTime.TotalMilliseconds = p.EndTime.TotalMilliseconds;
            p2.Layer = (int)numericUpDownBoxLayer.Value;
            p2.Style = "SE-progress-bar-bg";
            p2.Extra = "SE-progress-bar-bg";
            subtitle.Paragraphs.Add(p2);

            subtitle.Header = _subtitleWithNewHeader.Header ?? AdvancedSubStationAlpha.DefaultHeader;
            var style = new SsaStyle
            {
                Alignment = "7",
                Name = "SE-progress-bar-bg",
                MarginLeft = 0,
                MarginRight = 0,
                MarginVertical = 0,
                Primary = _boxColor,
                Secondary = _boxShadowColor,
                Tertiary = _boxShadowColor,
                Background = _boxShadowColor,
                Outline = _boxOutlineColor,
                ShadowWidth = numericUpDownShadowWidth.Value,
                OutlineWidth = numericUpDownOutlineWidth.Value,
            };
            subtitle.Header = AdvancedSubStationAlpha.UpdateOrAddStyle(subtitle.Header, style);

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
            _mpv?.Dispose();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (_mpv == null || _closing)
            {
                return;
            }

            if (_updatePreview)
            {
                VideoLoaded(null, null);
                _updatePreview = false;
            }
        }

        private void SetPosition_Shown(object sender, EventArgs e)
        {
            var playResX = AdvancedSubStationAlpha.GetTagFromHeader("PlayResX", "[Script Info]", _subtitleWithNewHeader.Header);
            var playResY = AdvancedSubStationAlpha.GetTagFromHeader("PlayResY", "[Script Info]", _subtitleWithNewHeader.Header);
            if (string.IsNullOrEmpty(playResX) || string.IsNullOrEmpty(playResY))
            {
                var dialogResult = MessageBox.Show(LanguageSettings.Current.AssaSetPosition.ResolutionMissing, "Subtitle Edit", MessageBoxButtons.YesNoCancel);
                if (dialogResult == DialogResult.OK || dialogResult == DialogResult.Yes)
                {
                    if (string.IsNullOrEmpty(_subtitleWithNewHeader.Header))
                    {
                        _subtitleWithNewHeader.Header = AdvancedSubStationAlpha.DefaultHeader;
                    }

                    _subtitleWithNewHeader.Header = AdvancedSubStationAlpha.AddTagToHeader("PlayResX", "PlayResX: " + _videoInfo.Width.ToString(CultureInfo.InvariantCulture), "[Script Info]", _subtitleWithNewHeader.Header);
                    _subtitleWithNewHeader.Header = AdvancedSubStationAlpha.AddTagToHeader("PlayResY", "PlayResY: " + _videoInfo.Height.ToString(CultureInfo.InvariantCulture), "[Script Info]", _subtitleWithNewHeader.Header);
                }
            }

            buttonPreview_Click(null, null);
            GeneratePreviewViaMpv();
            _loading = false;
        }

        private void pictureBoxPreview_Click(object sender, EventArgs e)
        {
            VideoLoaded(null, null);
        }

        private void SetPosition_ResizeEnd(object sender, EventArgs e)
        {
            var aspectRatio = (double)_videoInfo.Width / _videoInfo.Height;
            var newWidth = pictureBoxPreview.Height * aspectRatio;
            Width += (int)(newWidth - pictureBoxPreview.Width);
        }

        private void numericUpDownRotateX_ValueChanged(object sender, EventArgs e)
        {
            if (_loading)
            {
                return;
            }

            _updatePreview = true;
            VideoLoaded(null, null);
        }

        private void buttonPreview_Click(object sender, EventArgs e)
        {
            try
            {
                Cursor = Cursors.WaitCursor;

                // generate blank video
                var tempVideoFileName = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".mkv");
                var process = VideoPreviewGenerator.GenerateVideoFile(
                               tempVideoFileName,
                               2,
                               _videoInfo.Width,
                               _videoInfo.Height,
                               Color.Cyan,
                               false,
                               25,
                               null);
                process.Start();
                while (!process.HasExited)
                {
                    System.Threading.Thread.Sleep(100);
                    Application.DoEvents();
                }

                // make temp assa file with font
                var assaTempFileName = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".ass");
                var sub = new Subtitle();
                sub.Header = _subtitle.Header;
                sub.Paragraphs.Add(new Paragraph(GetPreviewParagraph()));
                File.WriteAllText(assaTempFileName, new AdvancedSubStationAlpha().ToText(sub, string.Empty));

                // hard code subtitle
                var outputVideoFileName = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".mp4");
                process = GetFfmpegProcess(tempVideoFileName, outputVideoFileName, assaTempFileName);
                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
                while (!process.HasExited)
                {
                    System.Threading.Thread.Sleep(100);
                    Application.DoEvents();
                }

                Cursor = Cursors.Default;
                var bmpFileName = VideoPreviewGenerator.GetScreenShot(outputVideoFileName, "00:00:01");
                using (var bmp = new Bitmap(bmpFileName))
                {
                    var nBmp = new NikseBitmap(bmp);
                    _top = nBmp.CalcTopCropping(Color.Cyan);
                    _bottom = nBmp.Height - nBmp.CalcBottomCropping(Color.Cyan);
                    _left = nBmp.CalcLeftCropping(Color.Cyan);
                    _right = nBmp.Width - nBmp.CalcRightCropping(Color.Cyan);

                    _updatePreview = true;
                }

                try
                {
                    File.Delete(tempVideoFileName);
                    File.Delete(assaTempFileName);
                    File.Delete(outputVideoFileName);
                    File.Delete(bmpFileName);
                }
                catch
                {
                    // ignore
                }
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private string GenerateBackgroundBox(int x, int y, int right, int bottom)
        {
            if (comboBoxBoxStyle.SelectedIndex == 1) // rounded corners
            {
                //var barEnd = x + width;
                //return $@"{{m {x} {y} b {x} {y} {x} {height} {height} {height} l {barEnd} {height} b {width} {height} {width} {y} {barEnd} {y} l  {barEnd} {y} {height} {y}{{\p0}}";
                //m top-left - x top - right - x LINE top-right - y maxX height x left height
            }

            return $"{{\\p1}}m {x} {y} l {right} {y} {right} {bottom} {x} {bottom}{{\\p0}}";
        }

        private Paragraph GetPreviewParagraph()
        {
            var idx = _selectedIndices.Min();
            var first = _subtitle.Paragraphs[idx];
            if (string.IsNullOrWhiteSpace(first.Text))
            {
                return new Paragraph("Example text", 0, 10000);
            }

            return new Paragraph(first) { StartTime = new TimeCode(0), EndTime = new TimeCode(10000) };
        }

        private Process GetFfmpegProcess(string inputVideoFileName, string outputVideoFileName, string assaTempFileName, int? passNumber = null, string twoPassBitRate = null)
        {
            var pass = string.Empty;
            if (passNumber.HasValue)
            {
                pass = passNumber.Value.ToString(CultureInfo.InvariantCulture);
            }

            return VideoPreviewGenerator.GenerateHardcodedVideoFile(
                inputVideoFileName,
                assaTempFileName,
                outputVideoFileName,
                _videoInfo.Width,
                _videoInfo.Height,
                "libx264",
                null,
                null,
                null,
                false,
                null,
                null,
                null,
                pass,
                twoPassBitRate,
                OutputHandler);
        }

        private void OutputHandler(object sendingProcess, DataReceivedEventArgs outLine)
        {
            if (string.IsNullOrWhiteSpace(outLine.Data))
            {
                return;
            }

            var match = FrameFinderRegex.Match(outLine.Data);
            if (!match.Success)
            {
                return;
            }

            var arr = match.Value.Split('=');
            if (arr.Length != 2)
            {
                return;
            }

            if (long.TryParse(arr[1].Trim(), out var f))
            {
                _processedFrames = f;
            }
        }

        private void PreviewValueChanged(object sender, EventArgs e)
        {
            _updatePreview = true;
        }

        private void AssSetBackground_Load(object sender, EventArgs e)
        {
            
        }

        private void buttonPrimaryColor_Click(object sender, EventArgs e)
        {
            var colorDialog = new ColorChooser();
            colorDialog.Color = panelPrimaryColor.BackColor;
            if (colorDialog.ShowDialog() == DialogResult.OK)
            {
                _boxColor = colorDialog.Color;
                _updatePreview = true;
                panelPrimaryColor.BackColor = colorDialog.Color;
            }
        }

        private void panelPrimaryColor_MouseClick(object sender, MouseEventArgs e)
        {
            buttonPrimaryColor_Click(null, null);
        }

        private void buttonShadowColor_Click(object sender, EventArgs e)
        {
            var colorDialog = new ColorChooser();
            colorDialog.Color = panelShadowColor.BackColor;
            if (colorDialog.ShowDialog() == DialogResult.OK)
            {
                _boxShadowColor = colorDialog.Color;
                _updatePreview = true;
                panelShadowColor.BackColor = colorDialog.Color;
            }
        }

        private void panelShadowColor_MouseClick(object sender, MouseEventArgs e)
        {
            buttonShadowColor_Click(null, null);
        }

        private void buttonOutlineColor_Click(object sender, EventArgs e)
        {
            var colorDialog = new ColorChooser();
            colorDialog.Color = panelOutlineColor.BackColor;
            if (colorDialog.ShowDialog() == DialogResult.OK)
            {
                _boxOutlineColor = colorDialog.Color;
                _updatePreview = true;
                panelOutlineColor.BackColor = colorDialog.Color;
            }
        }

        private void panelOutlineColor_MouseClick(object sender, MouseEventArgs e)
        {
            buttonOutlineColor_Click(null, null);
        }
    }
}
