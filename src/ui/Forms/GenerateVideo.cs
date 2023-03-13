using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Logic;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.Forms
{
    public sealed partial class GenerateVideo : Form
    {
        public string VideoFileName { get; private set; }
        private string _subtitleFileName;
        private bool _abort;
        private long _processedFrames;
        private long _startTicks;
        private long _totalFrames;
        private static readonly Regex FrameFinderRegex = new Regex(@"[Ff]rame=\s*\d+", RegexOptions.Compiled);
        private Bitmap _backgroundImage;
        private bool _promptFFmpegParameters;

        public GenerateVideo(Subtitle subtitle, VideoInfo videoInfo)
        {
            UiUtil.PreInitialize(this);
            InitializeComponent();
            UiUtil.FixFonts(this);

            _subtitleFileName = Utilities.GetFileNameWithoutExtension(subtitle.FileName);
            numericUpDownDurationMinutes.Value = Configuration.Settings.Tools.BlankVideoMinutes;
            
            double? maxTimeP = null;
            if (subtitle?.Paragraphs != null && subtitle.Paragraphs.Count > 0)
            {
                maxTimeP = subtitle?.Paragraphs.Where(p => !p.EndTime.IsMaxTime).Max(p => p.EndTime.TotalMilliseconds);
            }

            if (maxTimeP.HasValue && maxTimeP.Value / 1000 > 120)
            {
                var minutes = (int)maxTimeP.Value / 1000 / 60 + 1;
                if (minutes < 300)
                {
                    numericUpDownDurationMinutes.Value = minutes;
                }
            }

            panelColor.BackColor = Configuration.Settings.Tools.BlankVideoColor;
            if (Configuration.Settings.Tools.BlankVideoUseCheckeredImage)
            {
                radioButtonCheckeredImage.Checked = true;
            }
            else
            {
                radioButtonColor.Checked = true;
            }

            Text = LanguageSettings.Current.GenerateBlankVideo.Title;
            radioButtonCheckeredImage.Text = LanguageSettings.Current.GenerateBlankVideo.CheckeredImage;
            radioButtonColor.Text = LanguageSettings.Current.GenerateBlankVideo.SolidColor;
            labelDuration.Text = LanguageSettings.Current.GenerateBlankVideo.DurationInMinutes;
            labelFrameRate.Text = LanguageSettings.Current.General.FrameRate;
            groupBoxBackground.Text = LanguageSettings.Current.GenerateBlankVideo.Background;
            buttonColor.Text = LanguageSettings.Current.Settings.ChooseColor;
            buttonGenerate.Text = LanguageSettings.Current.Watermark.Generate;
            labelResolution.Text = LanguageSettings.Current.ExportPngXml.VideoResolution;
            labelPleaseWait.Text = LanguageSettings.Current.General.PleaseWait;
            buttonCancel.Text = LanguageSettings.Current.General.Cancel;
            checkBoxAddTimeCode.Text = LanguageSettings.Current.ImportText.GenerateTimeCodes;
            progressBar1.Visible = false;
            labelPleaseWait.Visible = false;
            labelProgress.Text = string.Empty;
            labelImageFileName.Text = string.Empty;

            var left = Math.Max(labelResolution.Left + labelResolution.Width, labelDuration.Left + labelDuration.Width) + 5;
            numericUpDownDurationMinutes.Left = left;
            buttonChooseDuration.Left = numericUpDownDurationMinutes.Left + numericUpDownDurationMinutes.Width + 9;
            numericUpDownWidth.Left = left;
            labelX.Left = numericUpDownWidth.Left + numericUpDownWidth.Width + 3;
            numericUpDownHeight.Left = labelX.Left + labelX.Width + 3;
            buttonVideoChooseStandardRes.Left = numericUpDownHeight.Left + numericUpDownHeight.Width + 9;
            comboBoxFrameRate.Left = left;
            radioButtonImage.Text = LanguageSettings.Current.VobSubEditCharacters.Image;
            buttonChooseImageFile.Left = radioButtonImage.Left + radioButtonImage.Width + 9;

            comboBoxFrameRate.Items.Clear();
            comboBoxFrameRate.Items.Add(23.976.ToString(CultureInfo.CurrentCulture));
            comboBoxFrameRate.Items.Add(24.0.ToString(CultureInfo.CurrentCulture));
            comboBoxFrameRate.Items.Add(25.0.ToString(CultureInfo.CurrentCulture));
            comboBoxFrameRate.Items.Add(29.97.ToString(CultureInfo.CurrentCulture));
            comboBoxFrameRate.Items.Add(30.00.ToString(CultureInfo.CurrentCulture));
            comboBoxFrameRate.Items.Add(50.00.ToString(CultureInfo.CurrentCulture));
            comboBoxFrameRate.Items.Add(59.94.ToString(CultureInfo.CurrentCulture));
            comboBoxFrameRate.Items.Add(60.00.ToString(CultureInfo.CurrentCulture));
            comboBoxFrameRate.SelectedIndex = 0;
            for (var index = 0; index < comboBoxFrameRate.Items.Count; index++)
            {
                var item = comboBoxFrameRate.Items[index];
                var v = Convert.ToDecimal(item.ToString(), CultureInfo.CurrentCulture);
                if (Math.Abs(v - Configuration.Settings.Tools.BlankVideoFrameRate) < 0.01m)
                {
                    comboBoxFrameRate.SelectedIndex = index;
                    break;
                }
            }

            if (videoInfo != null && videoInfo.Success && videoInfo.TotalSeconds > 0)
            {
                numericUpDownDurationMinutes.Value = (decimal)(videoInfo.TotalSeconds / 60.0);
            }
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

        private void buttonGenerate_Click(object sender, EventArgs e)
        {
            EnableDisableControls(false);

            if (string.IsNullOrEmpty(_subtitleFileName) || _subtitleFileName == "Untitled")
            {
                _subtitleFileName = radioButtonColor.Checked ? "blank_video_solid" : (radioButtonCheckeredImage.Checked ? "blank_video_checkered" : "blank_video_image");
            }

            using (var saveDialog = new SaveFileDialog { FileName = _subtitleFileName, Filter = "MP4|*.mp4|Matroska|*.mkv|WebM|*.webm" })
            {
                if (saveDialog.ShowDialog(this) != DialogResult.OK)
                {
                    EnableDisableControls(true);
                    return;
                }

                VideoFileName = saveDialog.FileName;
            }

            if (File.Exists(VideoFileName))
            {
                File.Delete(VideoFileName);
            }

            var addTimeColor = "white";
            if (radioButtonCheckeredImage.Checked)
            {
                addTimeColor = "black";
            }

            progressBar1.Visible = true;
            labelPleaseWait.Visible = true;
            var process = VideoPreviewGenerator.GenerateVideoFile(
                VideoFileName,
                (int)Math.Round(numericUpDownDurationMinutes.Value * 60),
                (int)numericUpDownWidth.Value,
                (int)numericUpDownHeight.Value,
                panelColor.BackColor,
                radioButtonCheckeredImage.Checked,
                decimal.Parse(comboBoxFrameRate.Text),
                radioButtonImage.Checked ? _backgroundImage : null,
                OutputHandler,
                checkBoxAddTimeCode.Checked,
                addTimeColor);

            if (!CheckForPromptParameters(process, Text))
            {
                _abort = true;
                return;
            }

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            _totalFrames = (long)Math.Round(float.Parse(comboBoxFrameRate.Text, CultureInfo.CurrentCulture) * (float)numericUpDownDurationMinutes.Value * 60.0f);
            progressBar1.Maximum = (int)_totalFrames;
            _startTicks = DateTime.UtcNow.Ticks;
            timer1.Start();
            while (!process.HasExited)
            {
                var v = (int)_processedFrames;
                if (v >= progressBar1.Minimum && v <= progressBar1.Maximum)
                {
                    progressBar1.Value = v;
                }

                System.Threading.Thread.Sleep(100);
                Application.DoEvents();
                if (_abort)
                {
                    process.Kill();
                }
            }

            progressBar1.Visible = false;
            labelPleaseWait.Visible = false;
            timer1.Stop();
            labelProgress.Text = string.Empty;
            DialogResult = _abort ? DialogResult.Cancel : DialogResult.OK;
        }

        private bool CheckForPromptParameters(Process process, string title)
        {
            if (!_promptFFmpegParameters)
            {
                return true;
            }

            using (var form = new GenerateVideoFFmpegPrompt(title, process.StartInfo.Arguments))
            {
                if (form.ShowDialog(this) != DialogResult.OK)
                {
                    return false;
                }

                process.StartInfo.Arguments = form.Parameters;
            }

            return true;
        }

        private void EnableDisableControls(bool enable)
        {
            buttonGenerate.Enabled = enable;
            numericUpDownDurationMinutes.Enabled = enable;
            numericUpDownWidth.Enabled = enable;
            numericUpDownHeight.Enabled = enable;
            comboBoxFrameRate.Enabled = enable;
            groupBoxBackground.Enabled = enable;
        }

        private void buttonColor_Click(object sender, EventArgs e)
        {
            using (var colorChooser = new ColorChooser { Color = panelColor.BackColor, ShowAlpha = false })
            {
                if (colorChooser.ShowDialog() == DialogResult.OK)
                {
                    panelColor.BackColor = colorChooser.Color;
                }
            }
        }

        private void panelColor_MouseClick(object sender, MouseEventArgs e)
        {
            buttonColor_Click(null, null);
        }

        private void GenerateVideo_FormClosing(object sender, FormClosingEventArgs e)
        {
            Configuration.Settings.Tools.BlankVideoColor = panelColor.BackColor;
            Configuration.Settings.Tools.BlankVideoUseCheckeredImage = radioButtonCheckeredImage.Checked;
            Configuration.Settings.Tools.BlankVideoMinutes = (int)numericUpDownDurationMinutes.Value;
            Configuration.Settings.Tools.BlankVideoFrameRate = Convert.ToDecimal(comboBoxFrameRate.Text, CultureInfo.CurrentCulture);

            _backgroundImage?.Dispose();
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            _abort = true;
            if (buttonGenerate.Enabled)
            {
                DialogResult = DialogResult.Cancel;
            }
        }

        private void numericUpDownWidth_ValueChanged(object sender, EventArgs e)
        {
            var v = (int)numericUpDownWidth.Value;
            if (v % 2 == 1)
            {
                numericUpDownWidth.Value++;
            }
        }

        private void numericUpDownHeight_ValueChanged(object sender, EventArgs e)
        {
            var v = (int)numericUpDownHeight.Value;
            if (v % 2 == 1)
            {
                numericUpDownHeight.Value++;
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (_processedFrames <= 0)
            {
                return;
            }

            var durationMs = (DateTime.UtcNow.Ticks - _startTicks) / 10_000;
            var msPerFrame = (float)durationMs / _processedFrames;
            var estimatedTotalMs = msPerFrame * _totalFrames;
            var estimatedLeft = ProgressHelper.ToProgressTime(estimatedTotalMs - durationMs);
            labelProgress.Text = estimatedLeft;
        }

        private void ResolutionPickClick(object sender, EventArgs e)
        {
            var text = (sender as ToolStripMenuItem).Text;
            var match = new Regex("\\d+x\\d+").Match(text);
            var parts = match.Value.Split('x');
            numericUpDownWidth.Value = int.Parse(parts[0]);
            numericUpDownHeight.Value = int.Parse(parts[1]);
        }

        private void buttonVideoChooseStandardRes_Click(object sender, EventArgs e)
        {
            var coordinates = buttonVideoChooseStandardRes.PointToClient(Cursor.Position);
            contextMenuStripRes.Show(buttonVideoChooseStandardRes, coordinates);
        }

        private void buttonChooseImageFile_Click(object sender, EventArgs e)
        {
            using (var openFileDialog1 = new OpenFileDialog())
            {
                openFileDialog1.Title = "Open...";
                openFileDialog1.FileName = string.Empty;
                openFileDialog1.Filter = "Images|*.png;*.jpg;*.jpeg;*.gif;*.tiff;*.bmp";
                if (openFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    _backgroundImage?.Dispose();
                    _backgroundImage = (Bitmap)Bitmap.FromFile(openFileDialog1.FileName);
                    labelImageFileName.Text = openFileDialog1.FileName;
                    numericUpDownWidth.Value = _backgroundImage.Width;
                    numericUpDownHeight.Value = _backgroundImage.Height;
                    radioButtonImage.Checked = true;
                }
            }
        }

        private void radioButtonImage_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButtonImage.Checked && _backgroundImage == null)
            {
                buttonChooseImageFile_Click(null, null);
            }
        }

        private void buttonChooseDuration_Click(object sender, EventArgs e)
        {
            using (var openFileDialog1 = new OpenFileDialog())
            {
                openFileDialog1.Title = LanguageSettings.Current.General.OpenVideoFileTitle;
                openFileDialog1.FileName = string.Empty;
                openFileDialog1.Filter = UiUtil.GetVideoFileFilter(true);
                openFileDialog1.FileName = string.Empty;
                if (openFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    var info = UiUtil.GetVideoInfo(openFileDialog1.FileName);
                    if (info != null && info.Success && info.TotalSeconds > 0)
                    {
                        numericUpDownDurationMinutes.Value = (decimal)(info.TotalSeconds / 60.0);
                    }
                }
            }
        }

        private void toolStripMenuItemResBrowse_Click(object sender, EventArgs e)
        {
            using (var openFileDialog1 = new OpenFileDialog())
            {
                openFileDialog1.Title = LanguageSettings.Current.General.OpenVideoFileTitle;
                openFileDialog1.FileName = string.Empty;
                openFileDialog1.Filter = UiUtil.GetVideoFileFilter(true);
                openFileDialog1.FileName = string.Empty;
                if (openFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    var info = UiUtil.GetVideoInfo(openFileDialog1.FileName);
                    if (info != null && info.Success && info.Width > 0 && info.Height > 0)
                    {
                        numericUpDownWidth.Value = info.Width;
                        numericUpDownHeight.Value = info.Height;
                    }
                }
            }
        }

        private void GenerateVideo_Shown(object sender, EventArgs e)
        {
            buttonGenerate.Show();
        }

        private void promptParameterBeforeGenerateToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _promptFFmpegParameters = true;
            buttonGenerate_Click(null, null);

        }
    }
}
