using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Logic;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using System.Windows.Forms;
using Nikse.SubtitleEdit.Core.AudioToText;
using Vosk;

namespace Nikse.SubtitleEdit.Forms
{
    public partial class AudioToText : Form
    {
        private readonly string _videoFileName;
        private readonly Subtitle _subtitle;
        private bool _cancel;

        public AudioToText(Subtitle subtitle, string videoFileName, VideoInfo videoInfo)
        {
            UiUtil.PreInitialize(this);
            InitializeComponent();
            UiUtil.FixFonts(this);
            UiUtil.FixLargeFonts(this, buttonOK);
            _subtitle = subtitle;
            _videoFileName = videoFileName;
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            var waveFileName = GenerateWavFile(_videoFileName, 0);
            textBoxLog.AppendText("Wav file name: " + waveFileName);
            textBoxLog.AppendText(Environment.NewLine);
            textBoxLog.AppendText(Environment.NewLine);

            var transcript = GenerateTranscript(waveFileName);

            _subtitle.Paragraphs.Clear();
            foreach (var resultText in transcript)
            {
                _subtitle.Paragraphs.Add(new Paragraph(resultText.Text, (double)resultText.Start * 1000.0, (double)resultText.End * 1000.0));
            }

            //textBoxLog.AppendText("Transcript result:" + Environment.NewLine);
            DialogResult = DialogResult.OK;
        }

        private List<ResultText> GenerateTranscript(string waveFileName)
        {
            // You can set to -1 to disable logging messages
            Vosk.Vosk.SetLogLevel(0);
            var model = new Model(Path.Combine(Configuration.DataDirectory, "Vosk", "eng"));
            var rec = new VoskRecognizer(model, 16000.0f);
            rec.SetMaxAlternatives(0);
            rec.SetWords(true);
            var sb = new StringBuilder();
            var totalLength = new FileInfo(waveFileName).Length;
            var totalRead = 0;
            progressBar1.Maximum = 100;
            progressBar1.Value = 0;
            progressBar1.Visible = true;
            progressBar1.Style = ProgressBarStyle.Blocks;
            var list = new List<ResultText>();
            using (Stream source = File.OpenRead(waveFileName))
            {
                byte[] buffer = new byte[4096];
                int bytesRead;
                while ((bytesRead = source.Read(buffer, 0, buffer.Length)) > 0)
                {
                    totalRead += bytesRead;
                    progressBar1.Value = (int)(totalRead * 100.0 / totalLength);
                    progressBar1.Refresh();
                    Application.DoEvents();

                    if (rec.AcceptWaveform(buffer, bytesRead))
                    {
                        Console.WriteLine(rec.Result());
                        sb.AppendLine(rec.Result());
                        textBoxLog.AppendText("Result: " + rec.Result().RemoveChar('\n').RemoveChar('\r') + Environment.NewLine);
                        var result = ParseJsonToResult(rec.Result());
                        list.AddRange(result);
                    }
                    else
                    {
                        var s = rec.PartialResult();
                        var s2 = rec.Result();
                        Console.WriteLine(s);
                        sb.AppendLine(s);
                        if (s.Length > 20)
                        {
                            textBoxLog.AppendText("Partial: " + rec.PartialResult().RemoveChar('\n').RemoveChar('\r') + Environment.NewLine);
                            var result = ParseJsonToResult(s2);
                            list.AddRange(result);
                        }
                    }
                }
            }

            return list;
        }

        private List<ResultText> ParseJsonToResult(string result)
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
                                //labelInfo.ForeColor = Color.Red;
                                //labelInfo.Text = LanguageSettings.Current.AddWaveform.LowDiskSpace;
                            }
                            //else if (labelInfo.ForeColor == Color.Red)
                            //{
                            //    labelInfo.Text = string.Format(LanguageSettings.Current.AddWaveform.FreeDiskSpace, Utilities.FormatBytesToDisplayFileSize(drive.AvailableFreeSpace));
                            //}
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

        private Process GetFfmpegProcess(string videoFileName, int audioTrackNumber, string outWaveFile)
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
    }
}
