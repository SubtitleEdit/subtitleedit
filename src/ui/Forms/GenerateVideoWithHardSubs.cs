using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Logic;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.Forms
{
    public sealed partial class GenerateVideoWithHardSubs : Form
    {
        private bool _abort;
        private bool _loading;
        private readonly Subtitle _assaSubtitle;
        private readonly VideoInfo _videoInfo;
        private readonly string _inputVideoFileName;
        private static readonly Regex FrameFinderRegex = new Regex(@"[Ff]rame=\s*\d+", RegexOptions.Compiled);
        private long _processedFrames;
        private long _startTicks;
        private long _totalFrames;
        private StringBuilder _log;
        private readonly bool _isAssa;
        private readonly FfmpegMediaInfo _mediaInfo;
        private bool _promptFFmpegParameters;
        public string VideoFileName { get; private set; }
        public long MillisecondsEncoding { get; private set; }

        public GenerateVideoWithHardSubs(Subtitle assaSubtitle, string inputVideoFileName, VideoInfo videoInfo, int? fontSize, bool setStartEndCut)
        {
            UiUtil.PreInitialize(this);
            InitializeComponent();
            UiUtil.FixFonts(this);

            _loading = true;
            _videoInfo = videoInfo;
            Text = LanguageSettings.Current.GenerateVideoWithBurnedInSubs.Title;
            _assaSubtitle = new Subtitle(assaSubtitle);
            _inputVideoFileName = inputVideoFileName;
            buttonGenerate.Text = LanguageSettings.Current.Watermark.Generate;
            labelPleaseWait.Text = LanguageSettings.Current.General.PleaseWait;
            labelResolution.Text = LanguageSettings.Current.SubStationAlphaProperties.Resolution;
            labelPreviewPleaseWait.Text = LanguageSettings.Current.General.PleaseWait;
            labelFontSize.Text = LanguageSettings.Current.ExportPngXml.FontSize;
            labelSubtitleFont.Text = LanguageSettings.Current.ExportPngXml.FontFamily;
            buttonCancel.Text = LanguageSettings.Current.General.Cancel;
            labelAudioEnc.Text = LanguageSettings.Current.GenerateVideoWithBurnedInSubs.Encoding;
            labelVideoEncoding.Text = LanguageSettings.Current.GenerateVideoWithBurnedInSubs.Encoding;
            labelAudioBitRate.Text = LanguageSettings.Current.GenerateVideoWithBurnedInSubs.BitRate;
            labelAudioSampleRate.Text = LanguageSettings.Current.GenerateVideoWithBurnedInSubs.SampleRate;
            checkBoxMakeStereo.Text = LanguageSettings.Current.GenerateVideoWithBurnedInSubs.Stereo;
            labelCRF.Text = LanguageSettings.Current.GenerateVideoWithBurnedInSubs.Crf;
            labelPreset.Text = LanguageSettings.Current.GenerateVideoWithBurnedInSubs.Preset;
            labelTune.Text = LanguageSettings.Current.GenerateVideoWithBurnedInSubs.TuneFor;
            buttonPreview.Text = LanguageSettings.Current.General.Preview;
            checkBoxRightToLeft.Text = LanguageSettings.Current.Settings.FixRTLViaUnicodeChars;
            checkBoxAlignRight.Text = LanguageSettings.Current.GenerateVideoWithBurnedInSubs.AlignRight;
            checkBoxBox.Text = LanguageSettings.Current.SubStationAlphaStyles.OpaqueBox;
            progressBar1.Visible = false;
            labelPleaseWait.Visible = false;
            labelPreviewPleaseWait.Visible = false;
            labelProgress.Text = string.Empty;
            labelFileName.Text = string.Empty;
            labelPass.Text = string.Empty;
            comboBoxVideoEncoding.SelectedIndex = 0;
            comboBoxCrf.SelectedIndex = 6;
            comboBoxAudioEnc.SelectedIndex = 0;
            comboBoxAudioSampleRate.SelectedIndex = 1;
            comboBoxTune.SelectedIndex = 0;
            comboBoxAudioBitRate.Text = "128k";
            checkBoxTargetFileSize_CheckedChanged(null, null);
            checkBoxTargetFileSize.Text = LanguageSettings.Current.GenerateVideoWithBurnedInSubs.TargetFileSize;
            labelFileSize.Text = LanguageSettings.Current.GenerateVideoWithBurnedInSubs.FileSizeMb;
            numericUpDownTargetFileSize.Left = labelFileSize.Left + labelFileSize.Width + 5;
            linkLabelHelp.Text = LanguageSettings.Current.Main.Menu.Help.Title;
            groupBoxSettings.Text = LanguageSettings.Current.Settings.Title;
            groupBoxVideo.Text = LanguageSettings.Current.Main.Menu.Video.Title;
            groupBoxAudio.Text = LanguageSettings.Current.GenerateVideoWithBurnedInSubs.Audio;
            promptParameterBeforeGenerateToolStripMenuItem.Text = LanguageSettings.Current.GenerateBlankVideo.GenerateWithFfmpegParametersPrompt;

            comboBoxVideoEncoding.Text = Configuration.Settings.Tools.GenVideoEncoding;
            comboBoxCrf.Text = Configuration.Settings.Tools.GenVideoCrf;
            comboBoxTune.Text = Configuration.Settings.Tools.GenVideoTune;
            comboBoxAudioEnc.Text = Configuration.Settings.Tools.GenVideoAudioEncoding;
            comboBoxAudioSampleRate.Text = Configuration.Settings.Tools.GenVideoAudioSampleRate;
            checkBoxMakeStereo.Checked = Configuration.Settings.Tools.GenVideoAudioForceStereo;
            checkBoxTargetFileSize.Checked = Configuration.Settings.Tools.GenVideoTargetFileSize;
            checkBoxBox.Checked = Configuration.Settings.Tools.GenVideoNonAssaBox;
            checkBoxAlignRight.Checked = Configuration.Settings.Tools.GenVideoNonAssaAlignRight;
            checkBoxRightToLeft.Checked = Configuration.Settings.Tools.GenVideoNonAssaAlignRight;

            labelVideoBitrate.Text = string.Empty;

            numericUpDownWidth.Value = _videoInfo.Width;
            numericUpDownHeight.Value = _videoInfo.Height;

            var left = Math.Max(Math.Max(labelResolution.Left + labelResolution.Width, labelFontSize.Left + labelFontSize.Width), labelSubtitleFont.Left + labelSubtitleFont.Width) + 5;
            numericUpDownFontSize.Left = left;
            comboBoxSubtitleFont.Left = left;
            numericUpDownWidth.Left = left;
            labelX.Left = numericUpDownWidth.Left + numericUpDownWidth.Width + 3;
            numericUpDownHeight.Left = labelX.Left + labelX.Width + 3;
            buttonVideoChooseStandardRes.Left = numericUpDownHeight.Left + numericUpDownHeight.Width + 9;
            labelInfo.Text = LanguageSettings.Current.GenerateVideoWithBurnedInSubs.InfoAssaOff;
            checkBoxRightToLeft.Left = left;
            checkBoxAlignRight.Left = left;
            checkBoxBox.Left = left;

            var audioLeft = Math.Max(Math.Max(labelAudioEnc.Left + labelAudioEnc.Width, labelAudioSampleRate.Left + labelAudioSampleRate.Width), labelAudioBitRate.Left + labelAudioBitRate.Width) + 5;
            comboBoxAudioEnc.Left = audioLeft;
            checkBoxMakeStereo.Left = audioLeft;
            comboBoxAudioSampleRate.Left = audioLeft;
            comboBoxAudioBitRate.Left = audioLeft;

            linkLabelHelp.Left = Width - linkLabelHelp.Width - 30;

            numericUpDownCutFromMinutes.Left = numericUpDownCutFromHours.Right + 1;
            numericUpDownCutFromSeconds.Left = numericUpDownCutFromMinutes.Right + 1;

            numericUpDownCutToMinutes.Left = numericUpDownCutToHours.Right + 1;
            numericUpDownCutToSeconds.Left = numericUpDownCutToMinutes.Right + 1;

            _isAssa = !fontSize.HasValue;
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

                checkBoxRightToLeft.Checked = Configuration.Settings.General.RightToLeftMode &&
                                              LanguageAutoDetect.CouldBeRightToLeftLanguage(_assaSubtitle);
            }
            else
            {
                numericUpDownFontSize.Enabled = false;
                labelFontSize.Enabled = false;
                labelSubtitleFont.Enabled = false;
                comboBoxSubtitleFont.Enabled = false;
                checkBoxRightToLeft.Left = checkBoxTargetFileSize.Left;
                checkBoxAlignRight.Enabled = false;
                checkBoxBox.Enabled = false;
                labelInfo.Text = LanguageSettings.Current.GenerateVideoWithBurnedInSubs.InfoAssaOn;
            }

            var initialFont = Configuration.Settings.Tools.GenVideoFontName;
            if (string.IsNullOrEmpty(initialFont))
            {
                initialFont = Configuration.Settings.Tools.ExportBluRayFontName;
            }
            if (string.IsNullOrEmpty(initialFont))
            {
                initialFont = UiUtil.GetDefaultFont().Name;
            }
            foreach (var x in FontFamily.Families)
            {
                if (x.IsStyleAvailable(FontStyle.Regular) || x.IsStyleAvailable(FontStyle.Bold))
                {
                    comboBoxSubtitleFont.Items.Add(x.Name);
                    if (x.Name.Equals(initialFont, StringComparison.OrdinalIgnoreCase))
                    {
                        comboBoxSubtitleFont.SelectedIndex = comboBoxSubtitleFont.Items.Count - 1;
                    }
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


            _mediaInfo = FfmpegMediaInfo.Parse(inputVideoFileName);

            if (_videoInfo != null && _videoInfo.TotalSeconds > 0)
            {
                var timeSpan = TimeSpan.FromSeconds((long)Math.Round(_videoInfo.TotalSeconds + 0.5));
                numericUpDownCutToHours.Value = timeSpan.Hours;
                numericUpDownCutToMinutes.Value = timeSpan.Minutes;
                numericUpDownCutToSeconds.Value = timeSpan.Seconds;

                if (setStartEndCut && assaSubtitle != null && assaSubtitle.Paragraphs.Count > 0 &&
                    !assaSubtitle.Paragraphs.First().StartTime.IsMaxTime &&
                    !assaSubtitle.Paragraphs.Last().EndTime.IsMaxTime)
                {
                    timeSpan = assaSubtitle.Paragraphs.First().StartTime.TimeSpan;
                    numericUpDownCutFromHours.Value = timeSpan.Hours;
                    numericUpDownCutFromMinutes.Value = timeSpan.Minutes;
                    numericUpDownCutFromSeconds.Value = timeSpan.Seconds;

                    timeSpan = assaSubtitle.Paragraphs.Last().EndTime.TimeSpan;
                    if (timeSpan.Milliseconds > 0)
                    {
                        timeSpan = timeSpan.Add(TimeSpan.FromSeconds(1));
                    }
                    numericUpDownCutToHours.Value = timeSpan.Hours;
                    numericUpDownCutToMinutes.Value = timeSpan.Minutes;
                    numericUpDownCutToSeconds.Value = timeSpan.Seconds;

                    checkBoxCut.Checked = true;
                }

                checkBoxCut_CheckedChanged(null, null);
            }
            else
            {
                groupBoxCut.Visible = false;
            }
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            _abort = true;
            if (buttonGenerate.Enabled)
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

        private void buttonGenerate_Click(object sender, EventArgs e)
        {
            if (checkBoxCut.Checked)
            {
                var cutStart = GetCutStart();
                var cutEnd = GetCutEnd();

                if (cutStart >= cutEnd)
                {
                    MessageBox.Show("Cut end time must be after cut start time");
                    return;
                }

                if (cutStart.TotalSeconds >= _videoInfo.TotalSeconds)
                {
                    MessageBox.Show("cut start time must not be after video end time");
                    return;
                }
            }

            _log = new StringBuilder();
            buttonGenerate.Enabled = false;
            var oldFontSizeEnabled = numericUpDownFontSize.Enabled;
            numericUpDownFontSize.Enabled = false;

            using (var saveDialog = new SaveFileDialog { FileName = SuggestNewVideoFileName(), Filter = "MP4|*.mp4|Matroska|*.mkv|WebM|*.webm", AddExtension = true })
            {
                if (saveDialog.ShowDialog(this) != DialogResult.OK)
                {
                    buttonGenerate.Enabled = true;
                    numericUpDownFontSize.Enabled = true;
                    return;
                }

                VideoFileName = saveDialog.FileName;
            }

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
                    return;
                }
            }

            _totalFrames = (long)_videoInfo.TotalFrames;

            _log = new StringBuilder();
            _log.AppendLine("Target file name: " + VideoFileName);
            _log.AppendLine("Video info width: " + _videoInfo.Width);
            _log.AppendLine("Video info width: " + _videoInfo.Height);
            _log.AppendLine("Video info total frames: " + _videoInfo.TotalFrames);
            _log.AppendLine("Video info total seconds: " + _videoInfo.TotalSeconds);

            labelFileName.Text = string.Format(LanguageSettings.Current.GenerateVideoWithBurnedInSubs.TargetFileName, VideoFileName);
            if (!_isAssa)
            {
                SetStyleForNonAssa(_assaSubtitle);
            }

            FixRightToLeft(_assaSubtitle);

            var format = new AdvancedSubStationAlpha();
            var assaTempFileName = GetAssaFileName(_inputVideoFileName);

            if (checkBoxCut.Checked)
            {
                var cutStart = GetCutStart();
                if (cutStart.TotalMilliseconds > 0.001)
                {
                    var paragraphs = new List<Paragraph>();
                    _assaSubtitle.AddTimeToAllParagraphs(-cutStart);
                    foreach (var assaP in _assaSubtitle.Paragraphs)
                    {
                        if (assaP.StartTime.TotalMilliseconds > 0 && assaP.EndTime.TotalMilliseconds > 0)
                        {
                            paragraphs.Add(assaP);
                        }
                        else if (assaP.EndTime.TotalMilliseconds > 0)
                        {
                            assaP.StartTime.TotalMilliseconds = 0;
                            paragraphs.Add(assaP);
                        }
                    }

                    _assaSubtitle.Paragraphs.Clear();
                    _assaSubtitle.Paragraphs.AddRange(paragraphs);
                }
            }

            FileUtil.WriteAllText(assaTempFileName, format.ToText(_assaSubtitle, null), new TextEncoding(Encoding.UTF8, "UTF8"));

            groupBoxSettings.Enabled = false;
            labelPleaseWait.Visible = true;
            if (_videoInfo.TotalFrames > 0)
            {
                progressBar1.Visible = true;
            }

            if (checkBoxCut.Checked)
            {
                var cutLength = GetCutEnd() - GetCutStart();
                var factor = _videoInfo.TotalMilliseconds / cutLength.TotalMilliseconds;
                _totalFrames = (long)Math.Round(_totalFrames / factor) + 10;
            }

            var stopWatch = Stopwatch.StartNew();
            if (checkBoxTargetFileSize.Checked)
            {
                RunTwoPassEncoding(assaTempFileName);
            }
            else
            {
                RunOnePassEncoding(assaTempFileName);
            }

            progressBar1.Visible = false;
            labelPleaseWait.Visible = false;
            timer1.Stop();
            MillisecondsEncoding = stopWatch.ElapsedMilliseconds;
            labelProgress.Text = string.Empty;
            groupBoxSettings.Enabled = true;

            try
            {
                File.Delete(assaTempFileName);
            }
            catch
            {
                // ignore
            }

            if (_abort)
            {
                DialogResult = DialogResult.Cancel;
                return;
            }

            if (!File.Exists(VideoFileName) || new FileInfo(VideoFileName).Length == 0)
            {
                SeLogger.Error(Environment.NewLine + "Generate hard subbed video failed: " + Environment.NewLine + _log);
                MessageBox.Show("Generate embedded video failed" + Environment.NewLine +
                                "For more info see the error log: " + SeLogger.ErrorFile);
                buttonGenerate.Enabled = true;
                numericUpDownFontSize.Enabled = oldFontSizeEnabled;
                return;
            }

            DialogResult = DialogResult.OK;
        }

        private TimeSpan GetCutEnd()
        {
            return TimeSpan.FromSeconds(
                (double)numericUpDownCutToHours.Value * 60 * 60 +
                (double)numericUpDownCutToMinutes.Value * 60 +
                (double)numericUpDownCutToSeconds.Value);
        }

        private TimeSpan GetCutStart()
        {
            return TimeSpan.FromSeconds(
                (double)numericUpDownCutFromHours.Value * 60 * 60 +
                (double)numericUpDownCutFromMinutes.Value * 60 +
                (double)numericUpDownCutFromSeconds.Value);
        }

        private static string GetAssaFileName(string inputVideoFileName)
        {
            var path = Path.GetDirectoryName(inputVideoFileName);
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
            var fileName = Path.GetFileNameWithoutExtension(_inputVideoFileName);

            fileName += ".burn-in";

            fileName += $".{numericUpDownWidth.Value}x{numericUpDownHeight.Value}";

            if (comboBoxVideoEncoding.Text == "libx265" || comboBoxVideoEncoding.Text == "hevc_nvenc" || comboBoxVideoEncoding.Text == "hevc_amf")
            {
                fileName += ".x265";
            }
            else if (comboBoxVideoEncoding.Text == "libvpx-vp9")
            {
                fileName += ".vp9";
            }
            else
            {
                fileName += ".x264";
            }

            if (checkBoxCut.Enabled && checkBoxCut.Checked)
            {
                fileName += $".{numericUpDownCutFromHours.Text}-{numericUpDownCutFromMinutes.Text}-{numericUpDownCutFromSeconds.Text}_{numericUpDownCutToHours.Text}-{numericUpDownCutToMinutes.Text}-{numericUpDownCutToSeconds.Text}";
            }

            return fileName.Replace(".", "_") + ".mp4";
        }

        private void FixRightToLeft(Subtitle subtitle)
        {
            if (checkBoxRightToLeft.Checked)
            {
                for (var index = 0; index < subtitle.Paragraphs.Count; index++)
                {
                    var paragraph = subtitle.Paragraphs[index];
                    if (LanguageAutoDetect.ContainsRightToLeftLetter(paragraph.Text))
                    {
                        paragraph.Text = Utilities.FixRtlViaUnicodeChars(paragraph.Text);
                    }
                }
            }
        }

        private void RunTwoPassEncoding(string assaTempFileName)
        {
            labelPass.Text = string.Format(LanguageSettings.Current.GenerateVideoWithBurnedInSubs.PassX, "1");

            var bitRate = GetVideoBitRate();
            var videoBitRate = bitRate.ToString(CultureInfo.InvariantCulture) + "k";

            if (bitRate < 10)
            {
                MessageBox.Show($"Bitrate too low: {bitRate}k");
                return;
            }

            var process = GetFfmpegProcess(_inputVideoFileName, VideoFileName, assaTempFileName, 1, videoBitRate);
            _log.AppendLine("ffmpeg arguments pass 1: " + process.StartInfo.Arguments);
            if (!CheckForPromptParameters(process, Text + " - Pass 1"))
            {
                _abort = true;
                return;
            }

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            _startTicks = DateTime.UtcNow.Ticks;
            timer1.Start();

            while (!process.HasExited)
            {
                System.Threading.Thread.Sleep(100);
                WindowsHelper.PreventStandBy();
                Application.DoEvents();
                if (_abort)
                {
                    process.Kill();
                    return;
                }

                var v = (int)_processedFrames;
                SetProgress(v);
            }


            labelPass.Text = string.Format(LanguageSettings.Current.GenerateVideoWithBurnedInSubs.PassX, "2");
            process = GetFfmpegProcess(_inputVideoFileName, VideoFileName, assaTempFileName, 2, videoBitRate);
            _log.AppendLine("ffmpeg arguments pass 2: " + process.StartInfo.Arguments);
            if (!CheckForPromptParameters(process, Text + " - Pass 2"))
            {
                _abort = true;
                return;
            }
            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            _startTicks = DateTime.UtcNow.Ticks;
            timer1.Start();

            while (!process.HasExited)
            {
                System.Threading.Thread.Sleep(100);
                WindowsHelper.PreventStandBy();
                Application.DoEvents();
                if (_abort)
                {
                    process.Kill();
                    return;
                }

                var v = (int)_processedFrames;
                SetProgress(v);
            }
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

        private int GetAudioFileSizeInMb()
        {
            var ffmpegLocation = Configuration.Settings.General.FFmpegLocation;
            if (!Configuration.IsRunningOnWindows && (string.IsNullOrEmpty(ffmpegLocation) || !File.Exists(ffmpegLocation)))
            {
                ffmpegLocation = "ffmpeg";
            }

            var tempFileName = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".aac");
            var process = new Process
            {
                StartInfo =
                {
                    FileName = ffmpegLocation,
                    Arguments = $"-i \"{_inputVideoFileName}\" -vn -acodec copy \"{tempFileName}\"",
                    UseShellExecute = false,
                    CreateNoWindow = true,
                }
            };

            process.Start();
            process.WaitForExit();
            try
            {
                var length = (int)Math.Round(new FileInfo(tempFileName).Length / 1024.0 / 1024);
                try
                {
                    File.Delete(tempFileName);
                }
                catch
                {
                    // ignore
                }

                return length;
            }
            catch
            {
                return 0;
            }
        }

        private void RunOnePassEncoding(string assaTempFileName)
        {
            var process = GetFfmpegProcess(_inputVideoFileName, VideoFileName, assaTempFileName, null);
            _log.AppendLine("ffmpeg arguments: " + process.StartInfo.Arguments);

            if (!CheckForPromptParameters(process, Text))
            {
                _abort = true;
                return;
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
                }

                var v = (int)_processedFrames;
                SetProgress(v);
            }
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

        private Process GetFfmpegProcess(string inputVideoFileName, string outputVideoFileName, string assaTempFileName, int? passNumber = null, string twoPassBitRate = null, bool preview = false)
        {
            var pass = string.Empty;
            if (passNumber.HasValue)
            {
                pass = passNumber.Value.ToString(CultureInfo.InvariantCulture);
            }

            var cutStart = string.Empty;
            var cutEnd = string.Empty;
            if (checkBoxCut.Checked && !preview)
            {
                var start = GetCutStart();
                cutStart = $"-ss {start.Hours:00}:{start.Minutes:00}:{start.Seconds:00}";

                var end = GetCutEnd();
                cutEnd = $"-to {end.Hours:00}:{end.Minutes:00}:{end.Seconds:00}";
            }

            return VideoPreviewGenerator.GenerateHardcodedVideoFile(
                Path.GetFileName(inputVideoFileName),
                assaTempFileName,
                outputVideoFileName,
                (int)numericUpDownWidth.Value,
                (int)numericUpDownHeight.Value,
                comboBoxVideoEncoding.Text,
                comboBoxPreset.Text,
                comboBoxCrf.Text,
                comboBoxAudioEnc.Text,
                checkBoxMakeStereo.Checked,
                comboBoxAudioSampleRate.Text.Replace("Hz", string.Empty).Trim(),
                comboBoxTune.Text,
                comboBoxAudioBitRate.Text,
                pass,
                twoPassBitRate,
                OutputHandler,
                cutStart,
                cutEnd);
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
            var estimatedLeft = ProgressHelper.ToProgressTime(estimatedTotalMs - durationMs);
            labelProgress.Text = estimatedLeft;
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
            checkBoxMakeStereo.Enabled = comboBoxAudioEnc.Text != "copy";
            comboBoxAudioSampleRate.Enabled = comboBoxAudioEnc.Text != "copy";
            labelAudioSampleRate.Enabled = comboBoxAudioEnc.Text != "copy";
            comboBoxAudioBitRate.Enabled = comboBoxAudioEnc.Text != "copy";

            numericUpDownTargetFileSize_ValueChanged(null, null);
        }

        private void GenerateVideoWithHardSubs_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F2)
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
                    textBoxLog.Dock = DockStyle.Fill;

                    if (_log == null)
                    {
                        var log = new StringBuilder();
                        log.AppendLine("Video info width: " + _videoInfo.Width);
                        log.AppendLine("Video info width: " + _videoInfo.Height);
                        log.AppendLine("Video info total frames: " + _videoInfo.TotalFrames);
                        log.AppendLine("Video info total seconds: " + _videoInfo.TotalSeconds);
                        log.AppendLine();
                        log.AppendLine("ffmpeg " + GetFfmpegProcess(_inputVideoFileName, "output.mp4", "input.ass", null).StartInfo.Arguments);
                        textBoxLog.Text = log.ToString();
                    }
                    else
                    {
                        textBoxLog.Text = _log.ToString();
                    }
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
            if (comboBoxVideoEncoding.Text == "libx265")
            {
                UiUtil.OpenUrl("http://trac.ffmpeg.org/wiki/Encode/H.265");
            }
            else if (comboBoxVideoEncoding.Text == "libvpx-vp9")
            {
                UiUtil.OpenUrl("http://trac.ffmpeg.org/wiki/Encode/VP9");
            }
            else if (comboBoxVideoEncoding.Text == "h264_nvenc" || comboBoxVideoEncoding.Text == "hevc_nvenc")
            {
                UiUtil.OpenUrl("https://trac.ffmpeg.org/wiki/HWAccelIntro");
            }
            else
            {
                UiUtil.OpenUrl("http://trac.ffmpeg.org/wiki/Encode/H.264");
            }
        }

        private void GenerateVideoWithHardSubs_FormClosing(object sender, FormClosingEventArgs e)
        {
            Configuration.Settings.Tools.GenVideoFontName = comboBoxSubtitleFont.Text;
            Configuration.Settings.Tools.GenVideoFontSize = (int)numericUpDownFontSize.Value;
            Configuration.Settings.Tools.GenVideoEncoding = comboBoxVideoEncoding.Text;
            Configuration.Settings.Tools.GenVideoPreset = comboBoxPreset.Text;
            Configuration.Settings.Tools.GenVideoCrf = comboBoxCrf.Text;
            Configuration.Settings.Tools.GenVideoTune = comboBoxTune.Text;
            Configuration.Settings.Tools.GenVideoAudioEncoding = comboBoxAudioEnc.Text;
            Configuration.Settings.Tools.GenVideoAudioForceStereo = checkBoxMakeStereo.Checked;
            Configuration.Settings.Tools.GenVideoAudioSampleRate = comboBoxAudioSampleRate.Text;
            Configuration.Settings.Tools.GenVideoTargetFileSize = checkBoxTargetFileSize.Checked;
            Configuration.Settings.Tools.GenVideoNonAssaBox = checkBoxBox.Checked;
            Configuration.Settings.Tools.GenVideoNonAssaAlignRight = checkBoxAlignRight.Checked;
            Configuration.Settings.Tools.GenVideoNonAssaFixRtlUnicode = checkBoxRightToLeft.Checked;

            using (var graphics = CreateGraphics())
            {
                using (var font = new Font(UiUtil.GetDefaultFont().FontFamily, (float)numericUpDownFontSize.Value, FontStyle.Regular))
                {
                    var currentHeight = graphics.MeasureString("HJKLj", font).Height;
                    Configuration.Settings.Tools.GenVideoFontSizePercentOfHeight = (float)(currentHeight / _videoInfo.Height);
                }
            }
        }

        private void comboBoxVideoEncoding_SelectedIndexChanged(object sender, EventArgs e)
        {
            comboBoxCrf.Items.Clear();
            comboBoxCrf.BeginUpdate();
            comboBoxCrf.Items.Add(string.Empty);
            labelTune.Visible = true;
            comboBoxTune.Visible = true;
            labelCRF.Text = "CRF";
            labelCrfHint.Text = string.Empty;

            FillPresets(comboBoxVideoEncoding.Text);
            FillTuneIn(comboBoxVideoEncoding.Text);

            if (comboBoxVideoEncoding.Text == "libx265")
            {
                for (var i = 0; i < 51; i++)
                {
                    comboBoxCrf.Items.Add(i.ToString(CultureInfo.InvariantCulture));
                }

                comboBoxCrf.Text = "28";
            }
            else if (comboBoxVideoEncoding.Text == "libvpx-vp9")
            {
                for (var i = 4; i <= 63; i++)
                {
                    comboBoxCrf.Items.Add(i.ToString(CultureInfo.InvariantCulture));
                }

                comboBoxCrf.Text = "10";
            }
            else if (comboBoxVideoEncoding.Text == "h264_nvenc" ||
                     comboBoxVideoEncoding.Text == "hevc_nvenc")
            {
                comboBoxCrf.Items.Clear();
                comboBoxCrf.Items.Add(string.Empty);
                for (var i = 0; i <= 51; i++)
                {
                    comboBoxCrf.Items.Add(i.ToString(CultureInfo.InvariantCulture));
                }

                labelCRF.Text = "CQ";
                labelCrfHint.Text = "0=best quality, 51=best speed";
                comboBoxCrf.Text = string.Empty;
            }
            else if (comboBoxVideoEncoding.Text == "h264_amf" ||
                     comboBoxVideoEncoding.Text == "hevc_amf")
            {
                comboBoxCrf.Items.Clear();
                comboBoxCrf.Items.Add(string.Empty);
                for (var i = 0; i <= 10; i++)
                {
                    comboBoxCrf.Items.Add(i.ToString(CultureInfo.InvariantCulture));
                }

                labelCRF.Text = "Quality";
                labelCrfHint.Text = "0=best quality, 10=best speed";
                comboBoxCrf.Text = string.Empty;
            }
            else
            {
                for (var i = 17; i <= 28; i++)
                {
                    comboBoxCrf.Items.Add(i.ToString(CultureInfo.InvariantCulture));
                }

                comboBoxCrf.Text = "23";
            }
            comboBoxCrf.EndUpdate();

        }

        private void FillTuneIn(string videoCodec)
        {
            var items = new List<string>
            {
                string.Empty,
                "film",
                "animation",
                "grain",
            };

            string defaultItem = "";
            if (_loading)
            {
                defaultItem = Configuration.Settings.Tools.GenVideoTune;
            }

            if (comboBoxVideoEncoding.Text == "libx265")
            {
                items = new List<string>
                {
                    string.Empty,
                    "psnr",
                    "ssim",
                    "grain",
                    "zerolatency",
                    "fastdecode",
                };
            }
            else if (videoCodec == "libx264")
            {
                items = new List<string>
                {
                    string.Empty,
                    "film",
                    "animation",
                    "grain",
                    "stillimage",
                    "fastdecode",
                    "zerolatency",
                };
            }
            else if (videoCodec == "h264_nvenc")
            {
                items = new List<string>
                {
                    string.Empty,
                    "hq",
                    "ll",
                    "ull",
                    "lossless",
                };
            }
            else if (videoCodec == "hevc_nvenc")
            {
                items = new List<string>
                {
                    string.Empty,
                    "hq",
                    "ll",
                    "ull",
                    "lossless",
                };
            }
            else if (videoCodec == "h264_amf")
            {
                items = new List<string>
                {
                    string.Empty,
                };
            }
            else if (videoCodec == "hevc_amf")
            {
                items = new List<string>
                {
                    string.Empty,
                };
            }
            else if (videoCodec == "libvpx-vp9")
            {
                items = new List<string> { string.Empty };
                labelTune.Visible = false;
                comboBoxTune.Visible = false;
                comboBoxTune.Text = string.Empty;
            }

            comboBoxTune.Items.Clear();
            foreach (var item in items)
            {
                comboBoxTune.Items.Add(item);
            }

            comboBoxTune.Text = items.Contains(defaultItem) ? defaultItem : string.Empty;
        }

        private void FillPresets(string videoCodec)
        {
            var items = new List<string>
            {
               "ultrafast",
               "superfast",
               "veryfast",
               "faster",
               "fast",
               "medium",
               "slow",
               "slower",
               "veryslow",
            };

            string defaultItem = "medium";
            if (_loading)
            {
                defaultItem = Configuration.Settings.Tools.GenVideoPreset;
            }

            if (videoCodec == "h264_nvenc")
            {
                items = new List<string>
                {
                    "default",
                    "slow",
                    "medium",
                    "fast",
                    "hp",
                    "hq",
                    "bd",
                    "ll",
                    "llhq",
                    "llhp",
                    "lossless",
                    "losslesshp",
                    "p1",
                    "p2",
                    "p3",
                    "p4",
                    "p5",
                    "p6",
                    "p7",
                };
            }
            else if (videoCodec == "hevc_nvenc")
            {
                items = new List<string>
                {
                    "default",
                    "slow",
                    "medium",
                    "fast",
                    "hp",
                    "hq",
                    "bd",
                    "ll",
                    "llhq",
                    "llhp",
                    "lossless",
                    "losslesshp",
                    "p1",
                    "p2",
                    "p3",
                    "p4",
                    "p5",
                    "p6",
                    "p7",
                };
            }
            else if (videoCodec == "h264_amf")
            {
                items = new List<string> { string.Empty };
            }
            else if (videoCodec == "hevc_amf")
            {
                items = new List<string> { string.Empty };
            }
            else if (videoCodec == "libvpx-vp9")
            {
                items = new List<string> { string.Empty };
            }

            comboBoxPreset.Items.Clear();
            foreach (var item in items)
            {
                comboBoxPreset.Items.Add(item);
                if (item == defaultItem)
                {
                    comboBoxPreset.SelectedIndex = comboBoxPreset.Items.Count - 1;
                    comboBoxPreset.Text = defaultItem;
                }
            }

            if (comboBoxPreset.SelectedIndex < 0 && comboBoxPreset.Items.Count > 0)
            {
                comboBoxPreset.SelectedIndex = 0;
                comboBoxPreset.Text = string.Empty;
            }
        }

        private void GenerateVideoWithHardSubs_Shown(object sender, EventArgs e)
        {
            if (!File.Exists(_inputVideoFileName))
            {
                MessageBox.Show(string.Format(LanguageSettings.Current.Main.FileNotFound, _inputVideoFileName));
                buttonGenerate.Enabled = false;
                return;
            }

            var targetFileSizeMb = (int)Math.Round(new FileInfo(_inputVideoFileName).Length / 1024.0 / 1024);
            numericUpDownTargetFileSize.Value = Math.Max(targetFileSizeMb, numericUpDownTargetFileSize.Minimum);
            _loading = false;
            UiUtil.FixFonts(groupBoxSettings, 2000);

            buttonGenerate.Focus();
        }

        private void checkBoxTargetFileSize_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxTargetFileSize.Checked)
            {
                comboBoxAudioEnc.Text = "aac";
                labelAudioBitRate.Enabled = true;
                comboBoxAudioBitRate.Enabled = true;
            }
            else
            {
                labelAudioBitRate.Enabled = false;
                comboBoxAudioBitRate.Enabled = false;
            }

            numericUpDownTargetFileSize_ValueChanged(null, null);
        }

        private void buttonPreview_Click(object sender, EventArgs e)
        {
            try
            {
                buttonPreview.Enabled = false;
                labelPreviewPleaseWait.Visible = true;
                Cursor = Cursors.WaitCursor;

                // generate blank video
                var tempVideoFileName = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".mkv");
                var process = VideoPreviewGenerator.GenerateVideoFile(
                               tempVideoFileName,
                               2,
                               (int)numericUpDownWidth.Value,
                               (int)numericUpDownHeight.Value,
                               Color.Black,
                               true,
                               25,
                               null);
                process.Start();
                while (!process.HasExited)
                {
                    System.Threading.Thread.Sleep(100);
                    WindowsHelper.PreventStandBy();
                    Application.DoEvents();
                }

                // make temp assa file with font
                var assaTempFileName = GetAssaFileName(tempVideoFileName);
                var sub = new Subtitle();
                sub.Header = _assaSubtitle.Header;
                sub.Paragraphs.Add(new Paragraph(GetPreviewParagraph()));

                if (!_isAssa)
                {
                    SetStyleForNonAssa(sub);
                }
                FixRightToLeft(sub);
                FileUtil.WriteAllText(assaTempFileName, new AdvancedSubStationAlpha().ToText(sub, string.Empty), new TextEncoding(Encoding.UTF8, "UTF8"));

                // hardcode subtitle
                var outputVideoFileName = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".mp4");
                process = GetFfmpegProcess(tempVideoFileName, outputVideoFileName, assaTempFileName, null, null, true);
                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
                while (!process.HasExited)
                {
                    System.Threading.Thread.Sleep(100);
                    Application.DoEvents();
                }

                string bmpFileName;
                try
                {
                    bmpFileName = VideoPreviewGenerator.GetScreenShot(outputVideoFileName, "00:00:01");
                    using (var bmp = new Bitmap(bmpFileName))
                    {
                        using (var form = new ExportPngXmlPreview(bmp))
                        {
                            form.AllowNext = false;
                            form.AllowPrevious = false;
                            labelPreviewPleaseWait.Visible = false;
                            form.ShowDialog(this);
                        }
                    }
                }
                catch
                {
                    if (comboBoxVideoEncoding.Text.EndsWith("_amf"))
                    {
                        MessageBox.Show("Unable to generate video with AMD hardware acceleration!");
                    }
                    else if (comboBoxVideoEncoding.Text.EndsWith("_nvenc"))
                    {
                        MessageBox.Show("Unable to generate video with Nvidia hardware acceleration!");
                    }
                    else
                    {
                        MessageBox.Show("Unable to generate video!");
                    }

                    Cursor = Cursors.Default;
                    buttonPreview.Enabled = true;
                    labelPreviewPleaseWait.Visible = false;
                    return;
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
                buttonPreview.Enabled = true;
                labelPreviewPleaseWait.Visible = false;
            }
        }

        private void SetStyleForNonAssa(Subtitle sub)
        {
            sub.Header = AdvancedSubStationAlpha.DefaultHeader;
            var style = AdvancedSubStationAlpha.GetSsaStyle("Default", sub.Header);
            style.FontSize = numericUpDownFontSize.Value;
            style.FontName = comboBoxSubtitleFont.Text;
            style.Background = Color.FromArgb(150, 0, 0, 0);

            if (checkBoxAlignRight.Checked)
            {
                style.Alignment = "3";
            }

            if (checkBoxBox.Checked)
            {
                style.BorderStyle = "4"; // box - multi line
                style.ShadowWidth = 5;
            }

            sub.Header = AdvancedSubStationAlpha.GetHeaderAndStylesFromAdvancedSubStationAlpha(sub.Header, new System.Collections.Generic.List<SsaStyle>() { style });
            sub.Header = AdvancedSubStationAlpha.AddTagToHeader("PlayResX", "PlayResX: " + ((int)numericUpDownWidth.Value).ToString(CultureInfo.InvariantCulture), "[Script Info]", sub.Header);
            sub.Header = AdvancedSubStationAlpha.AddTagToHeader("PlayResY", "PlayResY: " + ((int)numericUpDownHeight.Value).ToString(CultureInfo.InvariantCulture), "[Script Info]", sub.Header);
        }

        private Paragraph GetPreviewParagraph()
        {
            _assaSubtitle.Renumber();
            Paragraph longest;
            if (_assaSubtitle.Paragraphs.Count > 2)
            {
                longest = _assaSubtitle.Paragraphs.Where(p => p.Number > 1).OrderByDescending(p => p.Text.Length).FirstOrDefault();
                if (longest != null && longest.Text.Length > 2)
                {
                    return new Paragraph(longest) { StartTime = new TimeCode(0), EndTime = new TimeCode(10000) };
                }
            }

            longest = _assaSubtitle.Paragraphs.OrderByDescending(p => p.Text.Length).FirstOrDefault();
            if (longest != null && longest.Text.Length > 2)
            {
                return new Paragraph(longest) { StartTime = new TimeCode(0), EndTime = new TimeCode(10000) };
            }

            return new Paragraph("Example text", 0, 10000);
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

        private int GetVideoBitRate()
        {
            var audioMb = 0;
            if (comboBoxAudioEnc.Text == "copy")
            {
                audioMb = GetAudioFileSizeInMb();
            }

            // (MiB * 8192 [converts MiB to kBit]) / video seconds = kBit/s total bitrate
            var bitRate = (int)Math.Round(((double)numericUpDownTargetFileSize.Value - audioMb) * 8192.0 / _videoInfo.TotalSeconds);
            if (comboBoxAudioEnc.Text != "copy" && !string.IsNullOrWhiteSpace(comboBoxAudioBitRate.Text))
            {
                var audioBitRate = int.Parse(comboBoxAudioBitRate.Text.RemoveChar('k').TrimEnd());
                bitRate -= audioBitRate;
            }

            return bitRate;
        }

        private void numericUpDownTargetFileSize_ValueChanged(object sender, EventArgs e)
        {
            labelVideoBitrate.Text = string.Empty;
            if (!checkBoxTargetFileSize.Checked)
            {
                return;
            }

            var videoBitRate = GetVideoBitRate();
            var separateAudio = comboBoxAudioEnc.Text != "copy" && !string.IsNullOrWhiteSpace(comboBoxAudioBitRate.Text);
            var audioBitRate = 0;
            if (separateAudio)
            {
                audioBitRate = int.Parse(comboBoxAudioBitRate.Text.RemoveChar('k').TrimEnd());
            }

            labelVideoBitrate.Visible = true;
            if (comboBoxAudioEnc.Text == "copy")
            {
                var audioTrack = _mediaInfo.Tracks.FirstOrDefault(p => p.TrackType == FfmpegTrackType.Audio);
                if (audioTrack?.BitRate > 0)
                {
                    audioBitRate = audioTrack.BitRate / 1024;
                }
                else
                {
                    labelVideoBitrate.Visible = false;
                }
            }

            labelVideoBitrate.Left = numericUpDownTargetFileSize.Right + 5;
            labelVideoBitrate.Text = string.Format(LanguageSettings.Current.GenerateVideoWithBurnedInSubs.TotalBitRateX, $"{(videoBitRate + audioBitRate):#,###,##0}k");
            if (separateAudio)
            {
                labelVideoBitrate.Text += $" ({videoBitRate:#,###,##0}k + {audioBitRate:#,###,##0}k)";
            }
        }

        private void comboBoxAudioBitRate_SelectedValueChanged(object sender, EventArgs e)
        {
            numericUpDownTargetFileSize_ValueChanged(null, null);
        }

        private void checkBoxCut_CheckedChanged(object sender, EventArgs e)
        {
            numericUpDownCutFromHours.Enabled = checkBoxCut.Checked;
            numericUpDownCutFromMinutes.Enabled = checkBoxCut.Checked;
            numericUpDownCutFromSeconds.Enabled = checkBoxCut.Checked;
            buttonCutFrom.Enabled = checkBoxCut.Checked;

            numericUpDownCutToHours.Enabled = checkBoxCut.Checked;
            numericUpDownCutToMinutes.Enabled = checkBoxCut.Checked;
            numericUpDownCutToSeconds.Enabled = checkBoxCut.Checked;
            buttonCutTo.Enabled = checkBoxCut.Checked;
        }

        private void buttonCutFrom_Click(object sender, EventArgs e)
        {
            var timeSpan = new TimeSpan((int)numericUpDownCutFromHours.Value, (int)numericUpDownCutFromMinutes.Value, (int)numericUpDownCutFromSeconds.Value);
            using (var form = new GetVideoPosition(_assaSubtitle, _inputVideoFileName, _videoInfo, timeSpan, LanguageSettings.Current.GenerateVideoWithBurnedInSubs.GetStartPosition))
            {
                if (form.ShowDialog(this) == DialogResult.OK)
                {
                    numericUpDownCutFromHours.Value = form.VideoPosition.Hours;
                    numericUpDownCutFromMinutes.Value = form.VideoPosition.Minutes;
                    numericUpDownCutFromSeconds.Value = form.VideoPosition.Seconds;
                }
            }
        }

        private void buttonCutTo_Click(object sender, EventArgs e)
        {
            var timeSpan = new TimeSpan((int)numericUpDownCutFromHours.Value, (int)numericUpDownCutToMinutes.Value, (int)numericUpDownCutFromSeconds.Value);
            using (var form = new GetVideoPosition(_assaSubtitle, _inputVideoFileName, _videoInfo, timeSpan, LanguageSettings.Current.GenerateVideoWithBurnedInSubs.GetStartPosition))
            {
                if (form.ShowDialog(this) == DialogResult.OK)
                {
                    numericUpDownCutToHours.Value = form.VideoPosition.Hours;
                    numericUpDownCutToMinutes.Value = form.VideoPosition.Minutes;
                    numericUpDownCutToSeconds.Value = form.VideoPosition.Seconds;
                }
            }
        }

        private void promptParameterBeforeGenerateToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _promptFFmpegParameters = true;
            buttonGenerate_Click(null, null);
        }
    }
}
