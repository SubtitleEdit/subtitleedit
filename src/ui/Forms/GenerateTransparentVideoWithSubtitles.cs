using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Logic;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.Forms
{
    public sealed partial class GenerateTransparentVideoWithSubtitles : Form
    {
        private bool _abort;
        private readonly Subtitle _assaSubtitle;
        private readonly VideoInfo _videoInfo;
        private static readonly Regex FrameFinderRegex = new Regex(@"[Ff]rame=\s*\d+", RegexOptions.Compiled);
        private long _processedFrames;
        private long _startTicks;
        private long _totalFrames;
        private StringBuilder _log;
        private readonly bool _isAssa;
        private bool _promptFFmpegParameters;
        public string VideoFileName { get; private set; }
        public long MillisecondsEncoding { get; private set; }

        public GenerateTransparentVideoWithSubtitles(Subtitle subtitle, SubtitleFormat format, VideoInfo videoInfo)
        {
            UiUtil.PreInitialize(this);
            InitializeComponent();
            textBoxLog.ScrollBars = ScrollBars.Both;
            UiUtil.FixFonts(this);

            _videoInfo = videoInfo;
            _assaSubtitle = new Subtitle(subtitle);

            if (format.GetType() == typeof(NetflixImsc11Japanese) && _videoInfo != null)
            {
                _assaSubtitle = new Subtitle();
                var raw = NetflixImsc11JapaneseToAss.Convert(subtitle, _videoInfo.Width, _videoInfo.Height);
                new AdvancedSubStationAlpha().LoadSubtitle(_assaSubtitle, raw.SplitToLines(), null);
            }

            Text = LanguageSettings.Current.Main.Menu.Video.GenerateTransparentVideoWithSubs.Trim('.');
            buttonGenerate.Text = LanguageSettings.Current.Watermark.Generate;
            labelPleaseWait.Text = LanguageSettings.Current.General.PleaseWait;
            labelResolution.Text = LanguageSettings.Current.SubStationAlphaProperties.Resolution;
            labelFontSize.Text = LanguageSettings.Current.ExportPngXml.FontSize;
            nikseLabelOutline.Text = LanguageSettings.Current.SubStationAlphaStyles.Outline;
            checkBoxFontBold.Text = LanguageSettings.Current.General.Bold;
            numericUpDownOutline.Text = LanguageSettings.Current.SubStationAlphaStyles.Outline;
            labelSubtitleFont.Text = LanguageSettings.Current.ExportPngXml.FontFamily;
            buttonOutlineColor.Text = LanguageSettings.Current.GenerateVideoWithBurnedInSubs.OutputSettings;
            buttonCancel.Text = LanguageSettings.Current.General.Cancel;
            checkBoxRightToLeft.Text = LanguageSettings.Current.Settings.FixRTLViaUnicodeChars;
            checkBoxAlignRight.Text = LanguageSettings.Current.GenerateVideoWithBurnedInSubs.AlignRight;
            checkBoxBox.Text = LanguageSettings.Current.SubStationAlphaStyles.OpaqueBox;
            deleteToolStripMenuItem.Text = LanguageSettings.Current.DvdSubRip.Remove;
            clearToolStripMenuItem.Text = LanguageSettings.Current.DvdSubRip.Clear;
            addFilesToolStripMenuItem.Text = LanguageSettings.Current.DvdSubRip.Add;
            labelFrameRate.Text = LanguageSettings.Current.General.FrameRate;

            progressBar1.Visible = false;
            labelPleaseWait.Visible = false;
            labelProgress.Text = string.Empty;
            labelPass.Text = string.Empty;
            groupBoxSettings.Text = LanguageSettings.Current.Settings.Title;
            groupBoxVideo.Text = LanguageSettings.Current.Main.Menu.Video.Title;
            promptParameterBeforeGenerateToolStripMenuItem.Text = LanguageSettings.Current.GenerateBlankVideo.GenerateWithFfmpegParametersPrompt;

            checkBoxBox.Checked = Configuration.Settings.Tools.GenTransparentVideoNonAssaBox;
            checkBoxAlignRight.Checked = Configuration.Settings.Tools.GenVideoNonAssaAlignRight;
            checkBoxRightToLeft.Checked = Configuration.Settings.Tools.GenVideoNonAssaAlignRight;

            if (_videoInfo != null)
            {
                numericUpDownWidth.Value = _videoInfo.Width;
                numericUpDownHeight.Value = _videoInfo.Height;
            }

            var left = Math.Max(Math.Max(labelResolution.Left + labelResolution.Width, labelFontSize.Left + labelFontSize.Width), labelSubtitleFont.Left + labelSubtitleFont.Width) + 5;
            numericUpDownFontSize.Left = left;
            comboBoxSubtitleFont.Left = left;
            numericUpDownOutline.Left = left;
            numericUpDownWidth.Left = left;
            comboBoxFrameRate.Left = left;
            labelX.Left = numericUpDownWidth.Left + numericUpDownWidth.Width + 3;
            numericUpDownHeight.Left = labelX.Left + labelX.Width + 3;
            buttonVideoChooseStandardRes.Left = numericUpDownHeight.Left + numericUpDownHeight.Width + 9;
            labelInfo.Text = LanguageSettings.Current.GenerateVideoWithBurnedInSubs.InfoAssaOff;

            checkBoxFontBold.Left = numericUpDownFontSize.Right + 12;
            checkBoxBox.Left = numericUpDownOutline.Right + 12;

            checkBoxRightToLeft.Left = left;
            checkBoxAlignRight.Left = checkBoxRightToLeft.Right + 12;
            buttonOutlineColor.Left = checkBoxBox.Right + 2;
            buttonOutlineColor.Text = LanguageSettings.Current.AssaSetBackgroundBox.BoxColor;
            panelOutlineColor.Left = buttonOutlineColor.Right + 3;
            buttonForeColor.Left = buttonOutlineColor.Left;
            panelForeColor.Left = panelOutlineColor.Left;
            buttonForeColor.Text = LanguageSettings.Current.Settings.WaveformTextColor;

            _isAssa = format.GetType() == typeof(AdvancedSubStationAlpha);

            checkBoxFontBold.Checked = Configuration.Settings.Tools.GenVideoFontBold;
            numericUpDownOutline.Value = Configuration.Settings.Tools.GenVideoOutline;

            var initialFont = Configuration.Settings.Tools.GenVideoFontName;
            if (string.IsNullOrEmpty(initialFont))
            {
                initialFont = Configuration.Settings.Tools.ExportBluRayFontName;
            }
            labelInfo.Text = LanguageSettings.Current.GenerateVideoWithBurnedInSubs.InfoAssaOn;
            if (string.IsNullOrEmpty(initialFont))
            {
                initialFont = UiUtil.GetDefaultFont().Name;
            }

            foreach (var x in FontHelper.GetRegularOrBoldCapableFontFamilies())
            {
                comboBoxSubtitleFont.Items.Add(x.Name);
                if (x.Name.Equals(initialFont, StringComparison.OrdinalIgnoreCase))
                {
                    comboBoxSubtitleFont.SelectedIndex = comboBoxSubtitleFont.Items.Count - 1;
                }
            }
            if (comboBoxSubtitleFont.SelectedIndex < 0 && comboBoxSubtitleFont.Items.Count > 0)
            {
                comboBoxSubtitleFont.SelectedIndex = 0;
            }

            if (Configuration.Settings.Tools.GenVideoFontSize >= numericUpDownFontSize.Minimum &&
                Configuration.Settings.Tools.GenVideoFontSize >= numericUpDownFontSize.Minimum)
            {
                numericUpDownFontSize.Value = Configuration.Settings.Tools.GenVideoFontSize;
            }

            checkBoxRightToLeft.Checked = Configuration.Settings.General.RightToLeftMode && LanguageAutoDetect.CouldBeRightToLeftLanguage(_assaSubtitle);
            textBoxLog.Visible = false;

            UiUtil.FixLargeFonts(this, buttonGenerate);
            UiUtil.FixFonts(this, 2000);

            comboBoxFrameRate.Items.Clear();
            comboBoxFrameRate.Items.Add("23.976");
            comboBoxFrameRate.Items.Add("24");
            comboBoxFrameRate.Items.Add("25");
            comboBoxFrameRate.Items.Add("29.97");
            comboBoxFrameRate.Items.Add("30");
            comboBoxFrameRate.Items.Add("50");
            comboBoxFrameRate.Items.Add("59.94");
            comboBoxFrameRate.Items.Add("60");
            comboBoxFrameRate.SelectedIndex = 2;

            FontEnableOrDisable(_isAssa);

            panelOutlineColor.BackColor = Configuration.Settings.Tools.GenVideoNonAssaBoxColor;
            panelForeColor.BackColor = Configuration.Settings.Tools.GenVideoNonAssaTextColor;
        }

        private void FontEnableOrDisable(bool assa)
        {
            var enabled = !assa;

            numericUpDownFontSize.Enabled = enabled;
            numericUpDownFontSize.Enabled = enabled;
            labelFontSize.Enabled = enabled;
            nikseLabelOutline.Enabled = enabled;
            labelSubtitleFont.Enabled = enabled;
            comboBoxSubtitleFont.Enabled = enabled;
            checkBoxAlignRight.Enabled = enabled;
            checkBoxBox.Enabled = enabled;
            checkBoxFontBold.Enabled = enabled;
            numericUpDownOutline.Enabled = enabled;
            buttonForeColor.Enabled = enabled;
            buttonOutlineColor.Enabled = enabled;
            panelOutlineColor.Enabled = enabled;
            panelForeColor.Enabled = enabled;

            labelInfo.Text = assa ? LanguageSettings.Current.GenerateVideoWithBurnedInSubs.InfoAssaOn : string.Empty;
        }

        private void FixRightToLeft(Subtitle subtitle)
        {
            if (!checkBoxRightToLeft.Checked)
            {
                return;
            }

            for (var index = 0; index < subtitle.Paragraphs.Count; index++)
            {
                var paragraph = subtitle.Paragraphs[index];
                if (LanguageAutoDetect.ContainsRightToLeftLetter(paragraph.Text))
                {
                    paragraph.Text = Utilities.FixRtlViaUnicodeChars(paragraph.Text);
                }
            }
        }

        private void SetStyleForNonAssa(Subtitle sub)
        {
            sub.Header = AdvancedSubStationAlpha.DefaultHeader;
            var style = AdvancedSubStationAlpha.GetSsaStyle("Default", sub.Header);
            style.FontSize = numericUpDownFontSize.Value;
            style.Bold = checkBoxFontBold.Checked;
            style.FontName = comboBoxSubtitleFont.Text;
            style.Background = panelOutlineColor.BackColor;
            style.Primary = panelForeColor.BackColor;
            style.OutlineWidth = numericUpDownOutline.Value;
            style.ShadowWidth = style.OutlineWidth * 0.5m;

            if (checkBoxAlignRight.Checked)
            {
                style.Alignment = "3";
            }

            if (checkBoxBox.Checked)
            {
                style.BorderStyle = "4"; // box - multi line
                style.ShadowWidth = 0;
                style.OutlineWidth = numericUpDownOutline.Value;
            }
            else
            {
                style.Outline = panelOutlineColor.BackColor;
            }

            sub.Header = AdvancedSubStationAlpha.GetHeaderAndStylesFromAdvancedSubStationAlpha(sub.Header, new List<SsaStyle> { style });
            sub.Header = AdvancedSubStationAlpha.AddTagToHeader("PlayResX", "PlayResX: " + ((int)numericUpDownWidth.Value).ToString(CultureInfo.InvariantCulture), "[Script Info]", sub.Header);
            sub.Header = AdvancedSubStationAlpha.AddTagToHeader("PlayResY", "PlayResY: " + ((int)numericUpDownHeight.Value).ToString(CultureInfo.InvariantCulture), "[Script Info]", sub.Header);
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void buttonGenerate_Click(object sender, EventArgs e)
        {
            CalculateTotalFrames();

            labelProgress.Text = string.Empty;
            labelProgress.ForeColor = UiUtil.ForeColor;

            _log = new StringBuilder();
            buttonGenerate.Enabled = false;
            var oldFontSizeEnabled = numericUpDownFontSize.Enabled;
            numericUpDownFontSize.Enabled = false;

            using (var saveDialog = new SaveFileDialog
                   {
                       FileName = SuggestNewVideoFileName(),
                       Filter = "MP4|*.mp4|Matroska|*.mkv|WebM|*.webm|mov|*.mov",
                       AddExtension = true,
                       InitialDirectory = string.IsNullOrEmpty(_assaSubtitle.FileName) ? string.Empty : Path.GetDirectoryName(_assaSubtitle.FileName),
                   })
            {
                if (saveDialog.ShowDialog(this) != DialogResult.OK)
                {
                    buttonGenerate.Enabled = true;
                    numericUpDownFontSize.Enabled = true;
                    return;
                }

                VideoFileName = saveDialog.FileName;
            }

            var stopWatch = Stopwatch.StartNew();
            if (!ConvertVideo(oldFontSizeEnabled, _assaSubtitle))
            {
                buttonGenerate.Enabled = true;
                numericUpDownFontSize.Enabled = true;
                progressBar1.Visible = false;
                labelPleaseWait.Visible = false;
                timer1.Stop();
                MillisecondsEncoding = stopWatch.ElapsedMilliseconds;
                labelProgress.Text = string.Empty;
                groupBoxSettings.Enabled = true;

                if (!_abort && _log.ToString().Length > 10)
                {
                    var title = "Error occurred during encoding";
                    MessageBox.Show($"Encoding with ffmpeg failed: {Environment.NewLine}{_log}", title, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

                return;
            }

            progressBar1.Visible = false;
            labelPleaseWait.Visible = false;
            timer1.Stop();
            MillisecondsEncoding = stopWatch.ElapsedMilliseconds;
            labelProgress.Text = string.Empty;
            groupBoxSettings.Enabled = true;

            if (_abort)
            {
                DialogResult = DialogResult.Cancel;
                buttonGenerate.Enabled = true;
                numericUpDownFontSize.Enabled = true;
                return;
            }

            if (!File.Exists(VideoFileName) || new FileInfo(VideoFileName).Length == 0)
            {
                SeLogger.Error(Environment.NewLine + "Generate video failed: " + Environment.NewLine + _log);
                MessageBox.Show("Generate embedded video failed" + Environment.NewLine +
                                "For more info see the error log: " + SeLogger.ErrorFile);
                buttonGenerate.Enabled = true;
                numericUpDownFontSize.Enabled = oldFontSizeEnabled;
                return;
            }

            var encodingTime = new TimeCode(MillisecondsEncoding).ToString();
            using (var f = new ExportPngXmlDialogOpenFolder(string.Format(LanguageSettings.Current.GenerateVideoWithBurnedInSubs.XGeneratedWithBurnedInSubsInX, Path.GetFileName(VideoFileName), encodingTime), Path.GetDirectoryName(VideoFileName), VideoFileName))
            {
                f.ShowDialog(this);
            }

            buttonGenerate.Enabled = true;
            numericUpDownFontSize.Enabled = true;
        }

        private void CalculateTotalFrames()
        {
            var seconds = _assaSubtitle.Paragraphs.Max(p => p.EndTime.TotalSeconds);    
            var frameRate = double.Parse(comboBoxFrameRate.Text, CultureInfo.InvariantCulture);
            _totalFrames = (long)Math.Round(seconds * frameRate, MidpointRounding.AwayFromZero) + 1;
        }

        private static string GetAssaFileName(string inputVideoFileName)
        {
            var path = string.IsNullOrEmpty(inputVideoFileName) ? string.Empty : Path.GetDirectoryName(inputVideoFileName);
            for (var i = 0; i < int.MaxValue; i++)
            {
                var guidLetters = Guid.NewGuid().ToString().RemoveChar('0', '1', '2', '3', '4', '5', '6', '7', '8', '9', '-');
                var fileName = Path.Combine(path, $"{guidLetters}.ass");
                if (!File.Exists(fileName))
                {
                    return fileName;
                }
            }

            return Path.Combine(path, "qwerty12.ass");
        }

        private string SuggestNewVideoFileName()
        {
            var fileName = Path.GetFileNameWithoutExtension(_assaSubtitle.FileName);

            fileName += ".transparent-subs";

            if (numericUpDownWidth.Value > 0 && numericUpDownHeight.Value > 0)
            {
                fileName += $".{numericUpDownWidth.Value}x{numericUpDownHeight.Value}";
            }

            return fileName.Replace(".", "_") + ".mp4";
        }

        private bool ConvertVideo(bool oldFontSizeEnabled, Subtitle subtitle)
        {
            if (File.Exists(VideoFileName))
            {
                try
                {
                    File.Delete(VideoFileName);
                }
                catch
                {
                    MessageBox.Show($"Cannot overwrite video file {VideoFileName} - probably in use!");
                    buttonGenerate.Enabled = true;
                    numericUpDownFontSize.Enabled = oldFontSizeEnabled;
                    return false;
                }
            }

            _log = new StringBuilder();

            if (_videoInfo != null)
            {
                _log.AppendLine("Video info width: " + _videoInfo.Width);
                _log.AppendLine("Video info width: " + _videoInfo.Height);
                _log.AppendLine("Video info total frames: " + _videoInfo.TotalFrames);
                _log.AppendLine("Video info total seconds: " + _videoInfo.TotalSeconds);
            }

            if (!_isAssa)
            {
                SetStyleForNonAssa(subtitle);
            }

            FixRightToLeft(subtitle);

            var format = new AdvancedSubStationAlpha();
            var assaTempFileName = GetAssaFileName(_assaSubtitle.FileName);

            if (Configuration.Settings.General.CurrentVideoIsSmpte && comboBoxFrameRate.Text.Contains(".", StringComparison.Ordinal))
            {
                foreach (var assaP in subtitle.Paragraphs)
                {
                    assaP.StartTime.TotalMilliseconds *= 1.001;
                    assaP.EndTime.TotalMilliseconds *= 1.001;
                }
            }

            FileUtil.WriteAllText(assaTempFileName, format.ToText(subtitle, null), new TextEncoding(Encoding.UTF8, "UTF8"));

            groupBoxSettings.Enabled = false;
            labelPleaseWait.Visible = true;
            labelPleaseWait.Text = LanguageSettings.Current.General.PleaseWait;

            if (_totalFrames > 0)
            {
                progressBar1.Visible = true;
            }

            var result = RunOnePassEncoding(assaTempFileName);

            try
            {
                File.Delete(assaTempFileName);
            }
            catch
            {
                // ignore
            }

            return result;
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

                _log.AppendLine("ffmpeg arguments custom: " + process.StartInfo.Arguments);
                process.StartInfo.Arguments = form.Parameters;
            }

            return true;
        }

        private Process GetFfmpegProcess(string outputVideoFileName, string assaTempFileName)
        {
            var totalMs = _assaSubtitle.Paragraphs.Max(p => p.EndTime.TotalMilliseconds);
            var ts = TimeSpan.FromMilliseconds(totalMs + 2000);
            var timeCode = string.Format($"{ts.Hours:00}\\\\:{ts.Minutes:00}\\\\:{ts.Seconds:00}");

            return VideoPreviewGenerator.GenerateTransparentVideoFile(
                assaTempFileName,
                outputVideoFileName,
                (int)numericUpDownWidth.Value,
                (int)numericUpDownHeight.Value,
                comboBoxFrameRate.Text,
                timeCode,
                OutputHandler);
        }

        private void OutputHandler(object sendingProcess, DataReceivedEventArgs outLine)
        {
            if (string.IsNullOrWhiteSpace(outLine.Data))
            {
                return;
            }

            _log?.AppendLine(outLine.Data);

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

        private bool RunOnePassEncoding(string assaTempFileName)
        {
            var process = GetFfmpegProcess(VideoFileName, assaTempFileName);
            _log.AppendLine("ffmpeg arguments: " + process.StartInfo.Arguments);

            if (!CheckForPromptParameters(process, Text))
            {
                _abort = true;
                return false;
            }

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            _startTicks = DateTime.UtcNow.Ticks;
            timer1.Start();

            while (!process.HasExited)
            {
                Application.DoEvents();
                WindowsHelper.PreventStandBy();
                System.Threading.Thread.Sleep(100);
                if (_abort)
                {
                    process.Kill();
                    return false;
                }

                var v = (int)_processedFrames;
                SetProgress(v);
            }

            if (_abort)
            {
                return false;
            }

            if (process.ExitCode != 0)
            {
                _log.AppendLine("ffmpeg exit code: " + process.ExitCode);
                return false;
            }

            return true;
        }

        private void SetProgress(int v)
        {
            if (_totalFrames == 0)
            {
                progressBar1.Value = progressBar1.Maximum;
            }

            var pct = Math.Min(progressBar1.Maximum, (int)Math.Round(v * 100.0 / _totalFrames, MidpointRounding.AwayFromZero));
            if (pct >= progressBar1.Minimum && pct <= progressBar1.Maximum && _totalFrames > 0)
            {
                progressBar1.Value = pct;
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

        private void promptParameterBeforeGenerateToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _promptFFmpegParameters = true;
            buttonGenerate_Click(null, null);
        }

        private void ResolutionPickClick(object sender, EventArgs e)
        {
            labelX.Left = numericUpDownWidth.Left + numericUpDownWidth.Width + 3;
            numericUpDownWidth.Visible = true;
            labelX.Text = "x";
            numericUpDownHeight.Visible = true;

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

        private void buttonForeColor_Click(object sender, EventArgs e)
        {
            using (var colorChooser = new ColorChooser { Color = panelForeColor.BackColor })
            {
                if (colorChooser.ShowDialog() == DialogResult.OK)
                {
                    panelForeColor.BackColor = colorChooser.Color;
                }
            }
        }

        private void buttonOutlineColor_Click(object sender, EventArgs e)
        {
            using (var colorChooser = new ColorChooser { Color = panelOutlineColor.BackColor })
            {
                if (colorChooser.ShowDialog() == DialogResult.OK)
                {
                    panelOutlineColor.BackColor = colorChooser.Color;
                }
            }
        }

        private void panelForeColor_Click(object sender, EventArgs e)
        {
            buttonForeColor_Click(null, null);
        }

        private void panelOutlineColor_Click(object sender, EventArgs e)
        {
            buttonOutlineColor_Click(null, null);
        }

        private void GenerateTransparentVideoWithSubtitles_FormClosing(object sender, FormClosingEventArgs e)
        {
            Configuration.Settings.Tools.GenVideoFontName = comboBoxSubtitleFont.Text;
            Configuration.Settings.Tools.GenVideoFontBold = checkBoxFontBold.Checked;
            Configuration.Settings.Tools.GenVideoOutline = (int)numericUpDownOutline.Value;
            Configuration.Settings.Tools.GenVideoFontSize = (int)numericUpDownFontSize.Value;
            Configuration.Settings.Tools.GenTransparentVideoNonAssaBox = checkBoxBox.Checked;
            Configuration.Settings.Tools.GenVideoNonAssaAlignRight = checkBoxAlignRight.Checked;
            Configuration.Settings.Tools.GenVideoNonAssaFixRtlUnicode = checkBoxRightToLeft.Checked;
            Configuration.Settings.Tools.GenVideoNonAssaBoxColor = panelOutlineColor.BackColor;
            Configuration.Settings.Tools.GenVideoNonAssaTextColor = panelForeColor.BackColor;
        }
    }
}
