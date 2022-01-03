using Nikse.SubtitleEdit.Core.AudioToText;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Logic;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Windows.Forms;
using Vosk;

namespace Nikse.SubtitleEdit.Forms
{
    public sealed partial class AudioToText : Form
    {
        private readonly string _videoFileName;
        private readonly string _voskFolder;
        private bool _cancel;
        private long _startTicks;
        private long _bytesWavTotal;
        private long _bytesWavRead;
        public Subtitle TranscribedSubtitle { get; private set; }

        public AudioToText(string videoFileName)
        {
            UiUtil.PreInitialize(this);
            InitializeComponent();
            UiUtil.FixFonts(this);
            UiUtil.FixLargeFonts(this, buttonGenerate);
            _videoFileName = videoFileName;

            Text = LanguageSettings.Current.AudioToText.Title;
            labelInfo.Text = LanguageSettings.Current.AudioToText.Info;
            labelInfo.Text = LanguageSettings.Current.AudioToText.Info;
            groupBoxModels.Text = LanguageSettings.Current.AudioToText.Models;
            labelModel.Text = LanguageSettings.Current.AudioToText.ChooseModel;
            linkLabelOpenModelsFolder.Text = LanguageSettings.Current.AudioToText.OpenModelsFolder;
            checkBoxUsePostProcessing.Text = LanguageSettings.Current.AudioToText.UsePostProcessing;
            buttonGenerate.Text = LanguageSettings.Current.Watermark.Generate;
            buttonCancel.Text = LanguageSettings.Current.General.Cancel;

            checkBoxUsePostProcessing.Checked = Configuration.Settings.Tools.VoskPostProcessing;
            _voskFolder = Path.Combine(Configuration.DataDirectory, "Vosk");
            FillModels();

            textBoxLog.Visible = false;
            textBoxLog.Dock = DockStyle.Fill;
            labelProgress.Text = string.Empty;
            labelTime.Text = string.Empty;
        }

        private void FillModels()
        {
            comboBoxModels.Items.Clear();
            foreach (var directory in Directory.GetDirectories(_voskFolder))
            {
                var name = Path.GetFileName(directory);
                if (!File.Exists(Path.Combine(directory, "final.mdl")) && !File.Exists(Path.Combine(directory, "am", "final.mdl")))
                {
                    continue;
                }

                comboBoxModels.Items.Add(name);
                if (name == Configuration.Settings.Tools.VoskModel)
                {
                    comboBoxModels.SelectedIndex = comboBoxModels.Items.Count - 1;
                }
            }

            if (comboBoxModels.SelectedIndex < 0 && comboBoxModels.Items.Count > 0)
            {
                comboBoxModels.SelectedIndex = 0;
            }
        }

        private void ButtonGenerate_Click(object sender, EventArgs e)
        {
            if (comboBoxModels.Items.Count == 0)
            {
                buttonDownload_Click(null, null);
                return;
            }

            progressBar1.Maximum = 100;
            progressBar1.Value = 0;
            progressBar1.Visible = true;
            var modelFileName = Path.Combine(_voskFolder, comboBoxModels.Text);
            buttonGenerate.Enabled = false;
            buttonDownload.Enabled = false;
            var waveFileName = GenerateWavFile(_videoFileName, 0);
            textBoxLog.AppendText("Wav file name: " + waveFileName);
            textBoxLog.AppendText(Environment.NewLine);
            progressBar1.Style = ProgressBarStyle.Blocks;
            var transcript = TranscribeViaVosk(waveFileName, modelFileName);
            if (_cancel)
            {
                DialogResult = DialogResult.Cancel;
                return;
            }

            var postProcessor = new AudioToTextPostProcessor(GetLanguage(comboBoxModels.Text))
            {
                ParagraphMaxChars = Configuration.Settings.General.SubtitleLineMaximumLength * 2,
            };
            TranscribedSubtitle = postProcessor.Generate(transcript, checkBoxUsePostProcessing.Checked);
            DialogResult = DialogResult.OK;
        }

        internal static string GetLanguage(string text)
        {
            var languageCodeList = new[] { "en", "ru", "cn", "fr", "sv", "de", "es", "fa", "tr", "ca", "uk", "kz", "ph", "ar", "nl", "el", "pt" };
            foreach (var languageCode in languageCodeList)
            {
                if (text.Contains("model-" + languageCode) || text.Contains("model-small-" + languageCode) || text.StartsWith(languageCode, StringComparison.OrdinalIgnoreCase))
                {
                    return languageCode;
                }
            }

            return "en";
        }

        public List<ResultText> TranscribeViaVosk(string waveFileName, string modelFileName)
        {
            labelProgress.Text = LanguageSettings.Current.AudioToText.LoadingVoskModel;
            labelProgress.Refresh();
            Application.DoEvents();
            Directory.SetCurrentDirectory(_voskFolder);
            Vosk.Vosk.SetLogLevel(0);
            var model = new Model(modelFileName);
            var rec = new VoskRecognizer(model, 16000.0f);
            rec.SetMaxAlternatives(0);
            rec.SetWords(true);
            var list = new List<ResultText>();
            labelProgress.Text = LanguageSettings.Current.AudioToText.Transcribing;
            labelProgress.Refresh();
            Application.DoEvents();
            var buffer = new byte[4096];
            _bytesWavTotal = new FileInfo(waveFileName).Length;
            _bytesWavRead = 0;
            _startTicks = DateTime.UtcNow.Ticks;
            timer1.Start();
            using (var source = File.OpenRead(waveFileName))
            {
                int bytesRead;
                while ((bytesRead = source.Read(buffer, 0, buffer.Length)) > 0)
                {
                    _bytesWavRead += bytesRead;
                    progressBar1.Value = (int)(_bytesWavRead * 100.0 / _bytesWavTotal);
                    progressBar1.Refresh();
                    Application.DoEvents();
                    if (rec.AcceptWaveform(buffer, bytesRead))
                    {
                        var res = rec.Result();
                        var results = ParseJsonToResult(res);
                        list.AddRange(results);
                    }
                    else
                    {
                        var res = rec.PartialResult();
                        textBoxLog.AppendText(res.RemoveChar('\r', '\n'));
                    }

                    if (_cancel)
                    {
                        return null;
                    }
                }
            }

            var finalResult = rec.FinalResult();
            var finalResults = ParseJsonToResult(finalResult);
            list.AddRange(finalResults);
            timer1.Stop();
            return list;
        }

        private static List<ResultText> ParseJsonToResult(string result)
        {
            var list = new List<ResultText>();
            var jsonParser = new SeJsonParser();
            var root = jsonParser.GetArrayElementsByName(result, "result");
            foreach (var item in root)
            {
                var conf = jsonParser.GetFirstObject(item, "conf");
                var start = jsonParser.GetFirstObject(item, "start");
                var end = jsonParser.GetFirstObject(item, "end");
                var word = jsonParser.GetFirstObject(item, "word");
                if (!string.IsNullOrWhiteSpace(word) &&
                    decimal.TryParse(conf, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var confidence) &&
                    decimal.TryParse(start, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var startSeconds) &&
                    decimal.TryParse(end, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var endSeconds))
                {
                    var rt = new ResultText { Confidence = confidence, Text = word, Start = startSeconds, End = endSeconds };
                    list.Add(rt);
                }
            }

            return list;
        }

        private string GenerateWavFile(string videoFileName, int audioTrackNumber)
        {
            var outWaveFile = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".wav");
            var process = GetFfmpegProcess(videoFileName, audioTrackNumber, outWaveFile);
            process.Start();
            progressBar1.Style = ProgressBarStyle.Marquee;
            progressBar1.Visible = true;
            double seconds = 0;
            buttonCancel.Visible = true;
            try
            {
                process.PriorityClass = ProcessPriorityClass.Normal;
            }
            catch
            {
                // ignored
            }

            _cancel = false;
            string targetDriveLetter = null;
            if (Configuration.IsRunningOnWindows)
            {
                var root = Path.GetPathRoot(outWaveFile);
                if (root.Length > 1 && root[1] == ':')
                {
                    targetDriveLetter = root.Remove(1);
                }
            }

            while (!process.HasExited)
            {
                Application.DoEvents();
                System.Threading.Thread.Sleep(100);
                seconds += 0.1;
                if (seconds < 60)
                {
                    labelProgress.Text = string.Format(LanguageSettings.Current.AddWaveform.ExtractingSeconds, seconds);
                }
                else
                {
                    labelProgress.Text = string.Format(LanguageSettings.Current.AddWaveform.ExtractingMinutes, (int)(seconds / 60), (int)(seconds % 60));
                }

                Refresh();
                if (_cancel)
                {
                    process.Kill();
                    progressBar1.Visible = false;
                    buttonCancel.Visible = false;
                    DialogResult = DialogResult.Cancel;
                    return null;
                }

                if (targetDriveLetter != null && seconds > 1 && Convert.ToInt32(seconds) % 10 == 0)
                {
                    try
                    {
                        var drive = new DriveInfo(targetDriveLetter);
                        if (drive.IsReady)
                        {
                            if (drive.AvailableFreeSpace < 50 * 1000000) // 50 mb
                            {
                                labelInfo.ForeColor = Color.Red;
                                labelInfo.Text = LanguageSettings.Current.AddWaveform.LowDiskSpace;
                            }
                        }
                    }
                    catch
                    {
                        // ignored
                    }
                }
            }

            return outWaveFile;
        }

        private static Process GetFfmpegProcess(string videoFileName, int audioTrackNumber, string outWaveFile)
        {
            if (!File.Exists(Configuration.Settings.General.FFmpegLocation) && Configuration.IsRunningOnWindows)
            {
                return null;
            }

            var audioParameter = string.Empty;
            if (audioTrackNumber > 0)
            {
                audioParameter = $"-map 0:a:{audioTrackNumber}";
            }

            const string fFmpegWaveTranscodeSettings = "-i \"{0}\" -vn -ar 16000 -ac 1 -ab 128 -vol 448 -f wav {2} \"{1}\"";
            //-i indicates the input
            //-vn means no video output
            //-ar 44100 indicates the sampling frequency.
            //-ab indicates the bit rate (in this example 160kb/s)
            //-vol 448 will boot volume... 256 is normal
            //-ac 2 means 2 channels
            // "-map 0:a:0" is the first audio stream, "-map 0:a:1" is the second audio stream

            var exeFilePath = Configuration.Settings.General.FFmpegLocation;
            if (!Configuration.IsRunningOnWindows)
            {
                exeFilePath = "ffmpeg";
            }

            var parameters = string.Format(fFmpegWaveTranscodeSettings, videoFileName, outWaveFile, audioParameter);
            return new Process { StartInfo = new ProcessStartInfo(exeFilePath, parameters) { WindowStyle = ProcessWindowStyle.Hidden, CreateNoWindow = true } };
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            if (buttonGenerate.Enabled)
            {
                DialogResult = DialogResult.Cancel;
            }
            else
            {
                _cancel = true;
            }
        }

        private void linkLabelVoskWebsite_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            UiUtil.OpenUrl("https://alphacephei.com/vosk/models");
        }

        private void AudioToText_FormClosing(object sender, FormClosingEventArgs e)
        {
            Configuration.Settings.Tools.VoskModel = comboBoxModels.Text;
            Configuration.Settings.Tools.VoskPostProcessing = checkBoxUsePostProcessing.Checked;
        }

        private void AudioToText_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F2)
            {
                if (textBoxLog.Visible)
                {
                    textBoxLog.Visible = true;
                    textBoxLog.BringToFront();
                }
                else
                {
                    textBoxLog.Visible = false;
                }

                e.SuppressKeyPress = true;
            }
            else if (e.KeyCode == UiUtil.HelpKeys)
            {
                linkLabelVoskWebsite_LinkClicked(null, null);
                e.SuppressKeyPress = true;
            }
        }

        private void linkLabelOpenModelFolder_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            UiUtil.OpenFolder(_voskFolder);
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (_bytesWavRead <= 0 || _bytesWavTotal <= 0)
            {
                return;
            }

            var durationMs = (DateTime.UtcNow.Ticks - _startTicks) / 10_000;
            var msPerFrame = (float)durationMs / _bytesWavRead;
            var estimatedTotalMs = msPerFrame * _bytesWavTotal;
            var estimatedLeft = ToProgressTime(estimatedTotalMs - durationMs);
            labelTime.Text = estimatedLeft;
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

        private void buttonDownload_Click(object sender, EventArgs e)
        {
            using (var form = new AudioToTextModelDownload() { AutoClose = true })
            {
                form.ShowDialog(this);
                FillModels();
            }
        }
    }
}
