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
    public partial class AudioToText : Form
    {
        private readonly string _videoFileName;
        private readonly string _voskFolder;
        private bool _cancel;
        public Subtitle TranscribedSubtitle { get; private set; }

        public AudioToText(string videoFileName)
        {
            UiUtil.PreInitialize(this);
            InitializeComponent();
            UiUtil.FixFonts(this);
            UiUtil.FixLargeFonts(this, buttonGenerate);
            _videoFileName = videoFileName;

            _voskFolder = Path.Combine(Configuration.DataDirectory, "Vosk");
            if (!Directory.Exists(_voskFolder))
            {
                Directory.CreateDirectory(_voskFolder);
            }

            comboBoxModels.Items.Clear();
            foreach (var directory in Directory.GetDirectories(_voskFolder))
            {
                var name = Path.GetFileName(directory);
                if (!Directory.Exists(Path.Combine(directory, "conf")))
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

            textBoxLog.Visible = false;
            labelProgress.Text = string.Empty;
        }

        private void ButtonGenerate_Click(object sender, EventArgs e)
        {
            if (comboBoxModels.Items.Count == 0)
            {
                MessageBox.Show(string.Format("Please download an audio-to-text model from the Vosk website and unpack to {0}", _voskFolder));
                return;
            }

            progressBar1.Maximum = 100;
            progressBar1.Value = 0;
            progressBar1.Visible = true;
            var modelFileName = Path.Combine(_voskFolder, comboBoxModels.Text);
            buttonGenerate.Enabled = false;
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

            var subtitleGenerator = new Core.AudioToText.Vosk.SubtitleGenerator(transcript);
            TranscribedSubtitle = subtitleGenerator.Generate("en");
            DialogResult = DialogResult.OK;
        }

        public List<ResultText> TranscribeViaVosk(string waveFileName, string modelFileName)
        {
            labelProgress.Text = "Loading Vosk speach model...";
            labelProgress.Refresh();
            Application.DoEvents();
            Vosk.Vosk.SetLogLevel(0);
            var model = new Model(modelFileName);
            var rec = new VoskRecognizer(model, 16000.0f);
            rec.SetMaxAlternatives(0);
            rec.SetWords(true);
            var list = new List<ResultText>();
            labelProgress.Text = "Transcribing audio to text...";
            labelProgress.Refresh();
            Application.DoEvents();
            var totalRead = 0;
            var buffer = new byte[4096];
            var totalLength = new FileInfo(waveFileName).Length;
            using (var source = File.OpenRead(waveFileName))
            {
                int bytesRead;
                while ((bytesRead = source.Read(buffer, 0, buffer.Length)) > 0)
                {
                    totalRead += bytesRead;
                    progressBar1.Value = (int)(totalRead * 100.0 / totalLength);
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
                }

                if (_cancel)
                {
                    return null;
                }
            }

            var finalResult = rec.FinalResult();
            var finalResults = ParseJsonToResult(finalResult);
            list.AddRange(finalResults);
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

            string audioParameter = string.Empty;
            if (audioTrackNumber > 0)
            {
                audioParameter = $"-map 0:a:{audioTrackNumber}";
            }

            const string fFmpegWaveTranscodeSettings = "-i \"{0}\" -vn -ar 16000 -ac 1 -ab 128 -vol 448 -f wav {2} \"{1}\"";
            //-i indicates the input
            //-vn means no video ouput
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
            DialogResult = DialogResult.Cancel;
        }

        private void linkLabelVoskWebsite_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            UiUtil.OpenUrl("https://alphacephei.com/vosk/models");
        }

        private void AudioToText_FormClosing(object sender, FormClosingEventArgs e)
        {
            Configuration.Settings.Tools.VoskModel = comboBoxModels.Text;
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
        }

        private void linkLabelOpenModelFolder_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            UiUtil.OpenFolder(_voskFolder);
        }
    }
}
