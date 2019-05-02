using Nikse.SubtitleEdit.Core;
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
using System.Xml;

namespace Nikse.SubtitleEdit.Forms
{
    public partial class ImportSceneChanges : PositionAndSizeForm
    {
        private static StringBuilder _timeCodes;
        public List<double> SceneChangesInSeconds = new List<double>();
        private readonly double _frameRate = 25;
        private readonly string _videoFileName;
        private double _lastSeconds;
        private static readonly Regex TimeRegex = new Regex(@"pts_time:\d+[.,]*\d*", RegexOptions.Compiled);
        private bool _abort;
        private bool _pause;
        private readonly StringBuilder _log;

        public ImportSceneChanges(VideoInfo videoInfo, string videoFileName)
        {
            UiUtil.PreInitialize(this);
            InitializeComponent();
            UiUtil.FixFonts(this);
            if (videoInfo != null && videoInfo.FramesPerSecond > 1)
            {
                _frameRate = videoInfo.FramesPerSecond;
            }

            _videoFileName = videoFileName;

            Text = Configuration.Settings.Language.ImportSceneChanges.Title;
            groupBoxGenerateSceneChanges.Text = Configuration.Settings.Language.ImportSceneChanges.Generate;
            buttonOpenText.Text = Configuration.Settings.Language.ImportSceneChanges.OpenTextFile;
            groupBoxImportText.Text = Configuration.Settings.Language.ImportSceneChanges.Import;
            radioButtonFrames.Text = Configuration.Settings.Language.ImportSceneChanges.Frames;
            radioButtonSeconds.Text = Configuration.Settings.Language.ImportSceneChanges.Seconds;
            radioButtonMilliseconds.Text = Configuration.Settings.Language.ImportSceneChanges.Milliseconds;
            groupBoxTimeCodes.Text = Configuration.Settings.Language.ImportSceneChanges.TimeCodes;
            buttonDownloadFfmpeg.Text = Configuration.Settings.Language.Settings.DownloadFFmpeg;
            buttonImportWithFfmpeg.Text = Configuration.Settings.Language.ImportSceneChanges.GetSceneChangesWithFfmpeg;
            labelFfmpegThreshold.Text = Configuration.Settings.Language.ImportSceneChanges.Sensitivity;
            labelThressholdDescription.Text = Configuration.Settings.Language.ImportSceneChanges.SensitivityDescription;
            buttonOK.Text = Configuration.Settings.Language.General.Ok;
            buttonCancel.Text = Configuration.Settings.Language.General.Cancel;
            UiUtil.FixLargeFonts(this, buttonOK);
            buttonImportWithFfmpeg.Enabled = !string.IsNullOrWhiteSpace(Configuration.Settings.General.FFmpegLocation) && File.Exists(Configuration.Settings.General.FFmpegLocation);
            numericUpDownThreshold.Enabled = !string.IsNullOrWhiteSpace(Configuration.Settings.General.FFmpegLocation) && File.Exists(Configuration.Settings.General.FFmpegLocation);
            var isFfmpegAvailable = !string.IsNullOrEmpty(Configuration.Settings.General.FFmpegLocation) && File.Exists(Configuration.Settings.General.FFmpegLocation);
            buttonDownloadFfmpeg.Visible = !isFfmpegAvailable;
            if (decimal.TryParse(Configuration.Settings.General.FFmpegSceneThreshold, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var thresshold) &&
                thresshold >= numericUpDownThreshold.Minimum &&
                thresshold <= numericUpDownThreshold.Maximum)
            {
                numericUpDownThreshold.Value = thresshold;
            }
            _log = new StringBuilder();
            textBoxLog.Visible = false;
        }

        public sealed override string Text
        {
            get => base.Text;
            set => base.Text = value;
        }

        private void buttonOpenText_Click(object sender, EventArgs e)
        {
            openFileDialog1.Title = buttonOpenText.Text;
            openFileDialog1.Filter = Configuration.Settings.Language.ImportText.TextFiles + "|*.txt;*.scenechanges" +
                                     "|Matroska xml chapter file|*.xml" +
                                     "|EZTitles shotchanges XML file|*.xml" +
                                     "|JSON scene changes file|*.json" +
                                     "|" + Configuration.Settings.Language.General.AllFiles + "|*.*";
            openFileDialog1.FileName = string.Empty;
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                LoadTextFile(openFileDialog1.FileName);
            }
        }

        private void LoadTextFile(string fileName)
        {
            try
            {
                var res = LoadFromMatroskaChapterFile(fileName);
                if (!string.IsNullOrEmpty(res))
                {
                    textBoxIImport.Text = res;
                    radioButtonHHMMSSMS.Checked = true;
                    return;
                }

                res = LoadFromEZTitlesShotchangesFile(fileName);
                if (!string.IsNullOrEmpty(res))
                {
                    textBoxIImport.Text = res;
                    radioButtonFrames.Checked = true;
                    return;
                }

                res = LoadFromJsonShotchangesFile(fileName);
                if (!string.IsNullOrEmpty(res))
                {
                    textBoxIImport.Text = res;
                    radioButtonHHMMSSMS.Checked = true;
                    return;
                }

                var encoding = LanguageAutoDetect.GetEncodingFromFile(fileName);
                string s = File.ReadAllText(fileName, encoding).Trim();
                if (s.Contains('.'))
                {
                    radioButtonSeconds.Checked = true;
                }

                if (s.Contains('.') && s.Contains(':'))
                {
                    radioButtonHHMMSSMS.Checked = true;
                }

                if (!s.Contains(Environment.NewLine) && s.Contains(';'))
                {
                    var sb = new StringBuilder();
                    foreach (string line in s.Split(';'))
                    {
                        if (!string.IsNullOrWhiteSpace(line))
                        {
                            sb.AppendLine(line.Trim());
                        }
                    }
                    textBoxIImport.Text = sb.ToString();
                }
                else
                {
                    textBoxIImport.Text = s;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private string LoadFromMatroskaChapterFile(string fileName)
        {
            try
            {
                var x = new XmlDocument();
                x.Load(fileName);
                var xmlNodeList = x.SelectNodes("//ChapterAtom");
                var sb = new StringBuilder();
                if (xmlNodeList != null)
                {
                    foreach (XmlNode chapter in xmlNodeList)
                    {
                        var start = chapter.SelectSingleNode("ChapterTimeStart");
                        string[] timeParts = start?.InnerText.Split(SplitChars, StringSplitOptions.RemoveEmptyEntries);
                        if (timeParts?.Length == 4)
                        {
                            if (timeParts[3].Length > 3)
                            {
                                timeParts[3] = timeParts[3].Substring(0, 3);
                            }

                            var ts = new TimeSpan(0, Convert.ToInt32(timeParts[0]), Convert.ToInt32(timeParts[1]), Convert.ToInt32(timeParts[2]), Convert.ToInt32(timeParts[3]));
                            sb.AppendLine(new TimeCode(ts).ToShortStringHHMMSSFF());
                        }
                    }
                }
                return sb.ToString();
            }
            catch
            {
                return null;
            }
        }

        private string LoadFromEZTitlesShotchangesFile(string fileName)
        {
            try
            {
                var x = new XmlDocument();
                x.Load(fileName);
                var xmlNodeList = x.SelectNodes("/shotchanges/shotchanges_list/shotchange");
                var sb = new StringBuilder();
                if (xmlNodeList != null)
                {
                    foreach (XmlNode shotChange in xmlNodeList)
                    {
                        sb.AppendLine(shotChange.Attributes["frame"]?.InnerText);
                    }
                }
                return sb.ToString();
            }
            catch
            {
                return null;
            }
        }

        private string LoadFromJsonShotchangesFile(string fileName)
        {
            try
            {
                var text = FileUtil.ReadAllTextShared(fileName, Encoding.UTF8);
                var list = new List<double>();
                foreach (string line in text.Split(','))
                {
                    string s = line.Trim() + "}";
                    string start = Json.ReadTag(s, "frame_time");
                    if (start != null)
                    {
                        double startSeconds;
                        if (double.TryParse(start, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out startSeconds))
                        {
                            list.Add(startSeconds * 1000.0);
                        }
                    }
                }

                var sb = new StringBuilder();
                foreach (double ms in list.OrderBy(p=>p))
                {
                    sb.AppendLine(new TimeCode(ms).ToShortStringHHMMSSFF());
                }
                return sb.ToString();
            }
            catch
            {
                return null;
            }
        }


        private static readonly char[] SplitChars = { ':', '.', ',' };

        private void buttonOK_Click(object sender, EventArgs e)
        {
            SceneChangesInSeconds = new List<double>();
            foreach (string line in string.IsNullOrEmpty(textBoxGenerate.Text) ? textBoxIImport.Lines : textBoxGenerate.Lines)
            {
                if (radioButtonHHMMSSMS.Checked)
                {
                    // Parse string (HH:MM:SS.ms)
                    string[] timeParts = line.Split(SplitChars, StringSplitOptions.RemoveEmptyEntries);
                    try
                    {
                        if (timeParts.Length == 2)
                        {
                            SceneChangesInSeconds.Add(new TimeSpan(0, 0, 0, Convert.ToInt32(timeParts[0]), Convert.ToInt32(timeParts[1])).TotalSeconds);
                        }
                        else if (timeParts.Length == 3)
                        {
                            SceneChangesInSeconds.Add(new TimeSpan(0, 0, Convert.ToInt32(timeParts[0]), Convert.ToInt32(timeParts[1]), Convert.ToInt32(timeParts[2])).TotalSeconds);
                        }
                        else if (timeParts.Length == 4)
                        {
                            SceneChangesInSeconds.Add(new TimeSpan(0, Convert.ToInt32(timeParts[0]), Convert.ToInt32(timeParts[1]), Convert.ToInt32(timeParts[2]), Convert.ToInt32(timeParts[3])).TotalSeconds);
                        }
                    }
                    catch
                    {
                        // ignored
                    }
                }
                else
                {
                    double d;
                    if (double.TryParse(line, out d))
                    {
                        if (radioButtonFrames.Checked)
                        {
                            SceneChangesInSeconds.Add(d / _frameRate);
                        }
                        else if (radioButtonSeconds.Checked)
                        {
                            SceneChangesInSeconds.Add(d);
                        }
                        else if (radioButtonMilliseconds.Checked)
                        {
                            SceneChangesInSeconds.Add(d / TimeCode.BaseUnit);
                        }
                    }
                }
            }
            Configuration.Settings.General.FFmpegSceneThreshold = numericUpDownThreshold.Value.ToString(CultureInfo.InvariantCulture);
            if (SceneChangesInSeconds.Count > 0)
            {
                DialogResult = DialogResult.OK;
            }
            else
            {
                MessageBox.Show(Configuration.Settings.Language.ImportSceneChanges.NoSceneChangesFound);
            }
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            _abort = true;
            DialogResult = DialogResult.Cancel;
        }

        private void ImportSceneChanges_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                DialogResult = DialogResult.Cancel;
            }
            else if (e.KeyCode == Keys.F8)
            {
                if (_pause)
                {
                    _pause = false;
                    textBoxLog.Visible = false;
                    Cursor = Cursors.WaitCursor;
                }
                else
                {
                    Cursor = Cursors.Default;
                    textBoxLog.Text = _log.ToString();
                    _pause = true;
                    textBoxLog.Visible = true;
                    textBoxLog.BringToFront();
                }
            }
        }

        private void buttonImportWithFfmpeg_Click(object sender, EventArgs e)
        {
            groupBoxImportText.Enabled = false;
            buttonOK.Enabled = false;
            progressBar1.Visible = true;
            progressBar1.Style = ProgressBarStyle.Marquee;
            buttonImportWithFfmpeg.Enabled = false;
            numericUpDownThreshold.Enabled = false;
            Cursor = Cursors.WaitCursor;
            textBoxGenerate.Text = string.Empty;
            _timeCodes = new StringBuilder();
            using (var process = new Process())
            {
                process.StartInfo.FileName = Configuration.Settings.General.FFmpegLocation;
                process.StartInfo.Arguments = $"-i \"{_videoFileName}\" -vf \"select=gt(scene\\," + numericUpDownThreshold.Value.ToString(CultureInfo.InvariantCulture) + "),showinfo\" -vsync vfr -f null -";
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;
                process.StartInfo.CreateNoWindow = true;
                process.OutputDataReceived += OutputHandler;
                process.ErrorDataReceived += OutputHandler;
                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
                double lastUpdateSeconds = 0;
                radioButtonHHMMSSMS.Checked = true;
                while (!process.HasExited)
                {
                    Application.DoEvents();
                    System.Threading.Thread.Sleep(100);

                    if (_pause && !_abort)
                    {
                        continue;
                    }

                    if (_lastSeconds > 0.1 && Math.Abs(lastUpdateSeconds - _lastSeconds) > 0 - 001)
                    {
                        lastUpdateSeconds = _lastSeconds;
                        UpdateImportTextBox();
                    }
                    if (_abort)
                    {
                        DialogResult = DialogResult.Cancel;
                        process.Kill();
                        return;
                    }
                }
            }
            Cursor = Cursors.Default;
            UpdateImportTextBox();
            buttonOK.Enabled = true;
            if (!_pause)
            {
                buttonOK_Click(sender, e);
            }
        }

        private void UpdateImportTextBox()
        {
            textBoxGenerate.Text = _timeCodes.ToString();
            textBoxGenerate.SelectionStart = textBoxGenerate.Text.Length;
            textBoxGenerate.ScrollToCaret();
        }

        void OutputHandler(object sendingProcess, DataReceivedEventArgs outLine)
        {
            if (!string.IsNullOrWhiteSpace(outLine.Data))
            {
                _log.AppendLine(outLine.Data);
                var match = TimeRegex.Match(outLine.Data);
                if (match.Success)
                {
                    var timeCode = match.Value.Replace("pts_time:", string.Empty).Replace(",", ".").Replace("٫", ".").Replace("⠨", ".");
                    double seconds;
                    if (double.TryParse(timeCode, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out seconds) && seconds > 0.2)
                    {
                        _timeCodes.AppendLine(TimeCode.FromSeconds(seconds).ToShortString());
                        _lastSeconds = seconds;
                    }
                }
            }
        }

        private void ImportSceneChanges_Shown(object sender, EventArgs e)
        {
            Activate();
        }

        private void buttonDownloadFfmpeg_Click(object sender, EventArgs e)
        {
            using (var form = new DownloadFfmpeg())
            {
                if (form.ShowDialog(this) == DialogResult.OK && !string.IsNullOrEmpty(form.FFmpegPath))
                {
                    Configuration.Settings.General.FFmpegLocation = form.FFmpegPath;
                    buttonDownloadFfmpeg.Visible = false;
                    buttonImportWithFfmpeg.Enabled = true;
                    numericUpDownThreshold.Enabled = true;
                    Configuration.Settings.Save();
                }
            }
        }
    }
}
