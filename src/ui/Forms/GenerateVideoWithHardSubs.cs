using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Logic;
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.Forms
{
    public sealed partial class GenerateVideoWithHardSubs : Form
    {
        private bool _abort;
        private readonly Subtitle _assaSubtitle;
        private readonly VideoInfo _videoInfo;
        private readonly string _inputVideoFileName;
        private static readonly Regex FrameFinderRegex = new Regex(@"[Ff]rame=\s*\d+", RegexOptions.Compiled);
        private long _processedFrames;
        private long _startTicks;
        private long _totalFrames;
        private StringBuilder _log;
        public string VideoFileName { get; private set; }

        public GenerateVideoWithHardSubs(Subtitle assaSubtitle, string inputVideoFileName, VideoInfo videoInfo, int? fontSize)
        {
            UiUtil.PreInitialize(this);
            InitializeComponent();
            UiUtil.FixFonts(this);

            _videoInfo = videoInfo;
            Text = LanguageSettings.Current.GenerateVideoWithBurnedInSubs.Title;
            _assaSubtitle = assaSubtitle;
            _inputVideoFileName = inputVideoFileName;
            buttonOK.Text = LanguageSettings.Current.Watermark.Generate;
            labelPleaseWait.Text = LanguageSettings.Current.General.PleaseWait;
            labelFontSize.Text = LanguageSettings.Current.ExportPngXml.FontSize;
            buttonCancel.Text = LanguageSettings.Current.General.Cancel;
            progressBar1.Visible = false;
            labelPleaseWait.Visible = false;
            labelProgress.Text = string.Empty;
            labelFileName.Text = string.Empty;
            comboBoxVideoEncoding.SelectedIndex = 0;
            comboBoxPreset.SelectedIndex = 5;
            comboBoxCrf.SelectedIndex = 6;
            comboBoxAudioEnc.SelectedIndex = 0;
            comboBoxAudioSampleRate.SelectedIndex = 1;
            comboBoxTune.SelectedIndex = 0;

            comboBoxPreset.Text = Configuration.Settings.Tools.GenVideoPreset;
            comboBoxVideoEncoding.Text = Configuration.Settings.Tools.GenVideoEncoding;
            comboBoxCrf.Text = Configuration.Settings.Tools.GenVideoCrf;
            comboBoxTune.Text = Configuration.Settings.Tools.GenVideoTune;
            comboBoxAudioEnc.Text = Configuration.Settings.Tools.GenVideoAudioEncoding;
            comboBoxAudioSampleRate.Text = Configuration.Settings.Tools.GenVideoAudioSampleRate;
            checkBoxMakeStereo.Checked = Configuration.Settings.Tools.GenVideoAudioForceStereo;

            numericUpDownWidth.Value = _videoInfo.Width;
            numericUpDownHeight.Value = _videoInfo.Height;
            if (fontSize.HasValue)
            {
                if (fontSize.Value < numericUpDownFontSize.Minimum)
                {
                    numericUpDownFontSize.Value = numericUpDownFontSize.Minimum;
                }
                else if (fontSize.Value > numericUpDownFontSize.Maximum)
                {
                    numericUpDownFontSize.Value = numericUpDownFontSize.Maximum;
                }
                else
                {
                    numericUpDownFontSize.Value = fontSize.Value;
                }

                var left = Math.Max(labelResolution.Left + labelResolution.Width, labelFontSize.Left + labelFontSize.Width) + 5;
                numericUpDownFontSize.Left = left;
                numericUpDownWidth.Left = left;
                labelX.Left = numericUpDownWidth.Left + numericUpDownWidth.Width + 3;
                numericUpDownHeight.Left = labelX.Left + labelX.Width + 3;
                labelInfo.Text = LanguageSettings.Current.GenerateVideoWithBurnedInSubs.InfoAssaOff;
            }
            else
            {
                numericUpDownFontSize.Visible = false;
                labelFontSize.Visible = false;
                numericUpDownWidth.Enabled = false;
                numericUpDownHeight.Enabled = false;
                labelInfo.Text = LanguageSettings.Current.GenerateVideoWithBurnedInSubs.InfoAssaOn;
            }

            textBoxLog.Visible = false;
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            _abort = true;
            if (buttonOK.Enabled)
            {
                DialogResult = DialogResult.Cancel;
            }
        }

        private void OutputHandler(object sendingProcess, DataReceivedEventArgs outLine)
        {
            if (string.IsNullOrWhiteSpace(outLine.Data))
            {
                return;
            }

            _log.AppendLine(outLine.Data);

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

        private void buttonOK_Click(object sender, EventArgs e)
        {
            buttonOK.Enabled = false;
            numericUpDownFontSize.Enabled = false;
            using (var saveDialog = new SaveFileDialog { FileName = string.Empty, Filter = "MP4|*.mp4|Matroska|*.mkv|WebM|*.webm" })
            {
                if (saveDialog.ShowDialog(this) != DialogResult.OK)
                {
                    buttonOK.Enabled = true;
                    numericUpDownFontSize.Enabled = true;
                    return;
                }

                VideoFileName = saveDialog.FileName;
            }

            _totalFrames = (long)_videoInfo.TotalFrames;

            _log = new StringBuilder();
            _log.AppendLine("Target file name: " + VideoFileName);
            _log.AppendLine("Video info width: " + _videoInfo.Width);
            _log.AppendLine("Video info width: " + _videoInfo.Height);
            _log.AppendLine("Video info total frames: " + _videoInfo.TotalFrames);
            _log.AppendLine("Video info total seconds: " + _videoInfo.TotalSeconds);

            if (File.Exists(VideoFileName))
            {
                File.Delete(VideoFileName);
            }

            labelFileName.Text = string.Format(LanguageSettings.Current.GenerateVideoWithBurnedInSubs.TargetFileName, VideoFileName);
            if (numericUpDownFontSize.Visible)
            {
                var fontSize = (int)numericUpDownFontSize.Value;
                var style = AdvancedSubStationAlpha.GetSsaStyle("Default", _assaSubtitle.Header);
                style.FontSize = fontSize;
                var styleLine = style.ToRawAss();
                _assaSubtitle.Header = AdvancedSubStationAlpha.AddTagToHeader("Style", styleLine, "[V4+ Styles]", _assaSubtitle.Header);
            }

            if (Configuration.Settings.General.RightToLeftMode && LanguageAutoDetect.CouldBeRightToLeftLanguage(_assaSubtitle))
            {
                for (var index = 0; index < _assaSubtitle.Paragraphs.Count; index++)
                {
                    var paragraph = _assaSubtitle.Paragraphs[index];
                    if (LanguageAutoDetect.ContainsRightToLeftLetter(paragraph.Text))
                    {
                        paragraph.Text = Utilities.FixRtlViaUnicodeChars(paragraph.Text);
                    }
                }
            }

            var format = new AdvancedSubStationAlpha();
            var assaTempFileName = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".ass");
            File.WriteAllText(assaTempFileName, format.ToText(_assaSubtitle, null));

            groupBoxSettings.Enabled = false;
            progressBar1.Maximum = (int)_videoInfo.TotalFrames;
            progressBar1.Visible = true;
            labelPleaseWait.Visible = true;
            var process = VideoPreviewGenerator.GenerateHardcodedVideoFile(
                _inputVideoFileName,
                assaTempFileName,
                VideoFileName,
                (int)numericUpDownWidth.Value,
                (int)numericUpDownHeight.Value,
                comboBoxVideoEncoding.Text,
                comboBoxPreset.Text,
                comboBoxCrf.Text,
                comboBoxAudioEnc.Text,
                checkBoxMakeStereo.Checked,
                comboBoxAudioSampleRate.Text.Replace("Hz", string.Empty).Trim(),
                comboBoxTune.Text,
                OutputHandler);

            _log.AppendLine("ffmpeg arguments: " + process.StartInfo.Arguments);

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            _startTicks = DateTime.UtcNow.Ticks;
            timer1.Start();

            while (!process.HasExited)
            {
                System.Threading.Thread.Sleep(100);
                Application.DoEvents();
                if (_abort)
                {
                    process.Kill();
                }

                var v = (int)_processedFrames;
                if (v >= progressBar1.Minimum && v <= progressBar1.Maximum)
                {
                    progressBar1.Value = v;
                }
            }

            progressBar1.Visible = false;
            labelPleaseWait.Visible = false;
            timer1.Stop();
            labelProgress.Text = string.Empty;
            groupBoxSettings.Enabled = true;

            for (int i = 0; i < 10; i++)
            {
                System.Threading.Thread.Sleep(100);
                Application.DoEvents();
            }

            try
            {
                File.Delete(assaTempFileName);
            }
            catch
            {
                // ignore
            }

            DialogResult = _abort ? DialogResult.Cancel : DialogResult.OK;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (_processedFrames <= 0 || _videoInfo.TotalFrames <= 0)
            {
                return;
            }

            var durationMs = (DateTime.UtcNow.Ticks - _startTicks) / 10_000;
            var msPerFrame = (float)durationMs / _processedFrames;
            var estimatedTotalMs = msPerFrame * _totalFrames;
            var estimatedLeft = ToProgressTime(estimatedTotalMs - durationMs);
            labelProgress.Text = estimatedLeft;
        }

        public static string ToProgressTime(float estimatedTotalMs)
        {
            var timeCode = new TimeCode(estimatedTotalMs);
            if (timeCode.TotalSeconds < 60)
            {
                return string.Format(LanguageSettings.Current.GenerateVideoWithBurnedInSubs.TimeRemainingSeconds, (int)Math.Round(timeCode.TotalSeconds));
            }

            if (timeCode.TotalSeconds / 60 > 5)
            {
                return string.Format(LanguageSettings.Current.GenerateVideoWithBurnedInSubs.TimeRemainingMinutes, (int)Math.Round(timeCode.TotalSeconds / 60));
            }

            return string.Format(LanguageSettings.Current.GenerateVideoWithBurnedInSubs.TimeRemainingMinutesAndSeconds, timeCode.Minutes + timeCode.Hours * 60, timeCode.Seconds);
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

        private void comboBoxAudioEnc_SelectedIndexChanged(object sender, EventArgs e)
        {
            checkBoxMakeStereo.Enabled = comboBoxAudioEnc.SelectedIndex > 0;
            comboBoxAudioSampleRate.Enabled = comboBoxAudioEnc.SelectedIndex > 0;
            labelAudioSampleRate.Enabled = comboBoxAudioEnc.SelectedIndex > 0;
        }

        private void GenerateVideoWithHardSubs_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F2 && _log != null)
            {
                if (textBoxLog.Visible)
                {
                    textBoxLog.Visible = false;
                }
                else
                {
                    textBoxLog.Visible = true;
                    textBoxLog.ScrollBars = ScrollBars.Both;
                    textBoxLog.BringToFront();
                    textBoxLog.Text = _log.ToString();
                    textBoxLog.Dock = DockStyle.Fill;
                }
                e.SuppressKeyPress = true;
            }
            else if (e.KeyCode == Keys.Escape && _log == null)
            {
                DialogResult = DialogResult.Cancel;
                e.SuppressKeyPress = true;
            }
            else if (e.KeyCode == Keys.Escape && _log == null)
            {
                DialogResult = DialogResult.Cancel;
                e.SuppressKeyPress = true;
            }
            else if (e.KeyData == UiUtil.HelpKeys)
            {
                linkLabelHelp_LinkClicked(null, null);
            }
        }

        private void linkLabelHelp_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            UiUtil.OpenUrl("http://trac.ffmpeg.org/wiki/Encode/H.264");
        }

        private void GenerateVideoWithHardSubs_FormClosing(object sender, FormClosingEventArgs e)
        {
            Configuration.Settings.Tools.GenVideoEncoding = comboBoxVideoEncoding.Text;
            Configuration.Settings.Tools.GenVideoPreset = comboBoxPreset.Text;
            Configuration.Settings.Tools.GenVideoCrf = comboBoxCrf.Text;
            Configuration.Settings.Tools.GenVideoTune = comboBoxTune.Text;
            Configuration.Settings.Tools.GenVideoAudioEncoding = comboBoxAudioEnc.Text;
            Configuration.Settings.Tools.GenVideoAudioForceStereo = checkBoxMakeStereo.Checked;
            Configuration.Settings.Tools.GenVideoAudioSampleRate = comboBoxAudioSampleRate.Text;
        }
    }
}
