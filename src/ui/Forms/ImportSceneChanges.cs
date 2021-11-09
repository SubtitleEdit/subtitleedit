using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Logic;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;

namespace Nikse.SubtitleEdit.Forms
{
    public partial class ImportSceneChanges : PositionAndSizeForm
    {
        public List<double> SceneChangesInSeconds = new List<double>();
        private readonly double _frameRate = 25;
        private readonly string _videoFileName;
        private bool _abort;
        private bool _pause;
        private readonly StringBuilder _log;
        private SceneChangesGenerator _sceneChangesGenerator;

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

            Text = LanguageSettings.Current.ImportSceneChanges.Title;
            groupBoxGenerateSceneChanges.Text = LanguageSettings.Current.ImportSceneChanges.Generate;
            buttonOpenText.Text = LanguageSettings.Current.ImportSceneChanges.OpenTextFile;
            groupBoxImportText.Text = LanguageSettings.Current.ImportSceneChanges.Import;
            radioButtonFrames.Text = LanguageSettings.Current.ImportSceneChanges.Frames;
            radioButtonSeconds.Text = LanguageSettings.Current.ImportSceneChanges.Seconds;
            radioButtonMilliseconds.Text = LanguageSettings.Current.ImportSceneChanges.Milliseconds;
            groupBoxTimeCodes.Text = LanguageSettings.Current.ImportSceneChanges.TimeCodes;
            buttonDownloadFfmpeg.Text = LanguageSettings.Current.Settings.DownloadFFmpeg;
            buttonImportWithFfmpeg.Text = LanguageSettings.Current.ImportSceneChanges.GetSceneChangesWithFfmpeg;
            labelFfmpegThreshold.Text = LanguageSettings.Current.ImportSceneChanges.Sensitivity;
            labelThresholdDescription.Text = LanguageSettings.Current.ImportSceneChanges.SensitivityDescription;
            buttonOK.Text = LanguageSettings.Current.General.Ok;
            buttonCancel.Text = LanguageSettings.Current.General.Cancel;
            UiUtil.FixLargeFonts(this, buttonOK);
            buttonImportWithFfmpeg.Enabled = !string.IsNullOrWhiteSpace(Configuration.Settings.General.FFmpegLocation) && File.Exists(Configuration.Settings.General.FFmpegLocation);
            numericUpDownThreshold.Enabled = !string.IsNullOrWhiteSpace(Configuration.Settings.General.FFmpegLocation) && File.Exists(Configuration.Settings.General.FFmpegLocation);
            var isFfmpegAvailable = !string.IsNullOrEmpty(Configuration.Settings.General.FFmpegLocation) && File.Exists(Configuration.Settings.General.FFmpegLocation);
            buttonDownloadFfmpeg.Visible = !isFfmpegAvailable;
            if (decimal.TryParse(Configuration.Settings.General.FFmpegSceneThreshold, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var threshold) &&
                threshold >= numericUpDownThreshold.Minimum &&
                threshold <= numericUpDownThreshold.Maximum)
            {
                numericUpDownThreshold.Value = threshold;
            }
            _log = new StringBuilder();
            textBoxLog.Visible = false;
            if (!Configuration.IsRunningOnWindows)
            {
                buttonDownloadFfmpeg.Visible = false;
                buttonImportWithFfmpeg.Enabled = true;
                numericUpDownThreshold.Enabled = true;
            }

            numericUpDownThreshold.Left = labelFfmpegThreshold.Left + labelFfmpegThreshold.Width + 4;
        }

        public sealed override string Text
        {
            get => base.Text;
            set => base.Text = value;
        }

        private void buttonOpenText_Click(object sender, EventArgs e)
        {
            openFileDialog1.Title = buttonOpenText.Text;
            openFileDialog1.Filter = LanguageSettings.Current.ImportText.TextFiles + "|*.txt;*.scenechanges;*.xml;*.json" +
                                     "|Matroska xml chapter file|*.xml" +
                                     "|EZTitles shotchanges XML file|*.xml" +
                                     "|JSON scene changes file|*.json" +
                                     "|" + LanguageSettings.Current.General.AllFiles + "|*.*";
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
                        if (double.TryParse(start, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var startSeconds))
                        {
                            list.Add(startSeconds * 1000.0);
                        }
                    }
                }

                var sb = new StringBuilder();
                foreach (double ms in list.OrderBy(p => p))
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
            var lines = string.IsNullOrEmpty(textBoxGenerate.Text) ? textBoxIImport.Lines : textBoxGenerate.Lines;
            if (radioButtonHHMMSSMS.Checked)
            {
                SceneChangesInSeconds = SceneChangesGenerator.GetSeconds(lines);
            }
            else
            {
                SceneChangesInSeconds = new List<double>();
                foreach (var line in lines)
                {
                    if (double.TryParse(line, out var d))
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
                MessageBox.Show(LanguageSettings.Current.ImportSceneChanges.NoSceneChangesFound);
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
            _sceneChangesGenerator = new SceneChangesGenerator();
            groupBoxImportText.Enabled = false;
            buttonOK.Enabled = false;
            progressBar1.Visible = true;
            progressBar1.Style = ProgressBarStyle.Marquee;
            buttonImportWithFfmpeg.Enabled = false;
            numericUpDownThreshold.Enabled = false;
            Cursor = Cursors.WaitCursor;
            textBoxGenerate.Text = string.Empty;
            using (var process = _sceneChangesGenerator.GetProcess(_videoFileName, numericUpDownThreshold.Value))
            {
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

                    if (_sceneChangesGenerator.LastSeconds > 0.1 && Math.Abs(lastUpdateSeconds - _sceneChangesGenerator.LastSeconds) > 0 - 001)
                    {
                        lastUpdateSeconds = _sceneChangesGenerator.LastSeconds;
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
            textBoxGenerate.Text = _sceneChangesGenerator.GetTimeCodesString();
            textBoxGenerate.SelectionStart = textBoxGenerate.Text.Length;
            textBoxGenerate.ScrollToCaret();
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
