using Nikse.SubtitleEdit.Core.AudioToText.PocketSphinx;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.ContainerFormats.Matroska;
using Nikse.SubtitleEdit.Logic;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.Forms
{
    public sealed partial class AudioToText : Form
    {
        public Subtitle Subtitle { get; set; }
        private readonly VideoInfo _videoInfo;
        private readonly string _videoFileName;
        private int _delayInMilliseconds;
        private bool _abort;
        private string _waveFileName;
        private readonly BackgroundWorker _backgroundWorker;
        private bool _showMore = true;
        private const int LogOutput = -256;
        private const int LogInfo = -512;

        public AudioToText(string videoFileName, VideoInfo videoInfo)
        {
            UiUtil.PreInitialize(this);
            InitializeComponent();
            UiUtil.FixFonts(this);
            _videoFileName = videoFileName;
            _videoInfo = videoInfo;
            _backgroundWorker = new BackgroundWorker();
            UiUtil.FixLargeFonts(this, buttonOK);
            labelProgress.Text = string.Empty;
            Text = LanguageSettings.Current.AudioToText.Title;
        }

        public static Process GetCommandLineProcess(string inputVideoFile, int audioTrackNumber, string outWaveFile, string encodeParamters, out string encoderName)
        {
            if (Configuration.Settings.General.UseFFmpegForWaveExtraction && File.Exists(Configuration.Settings.General.FFmpegLocation))
            {
                encoderName = "FFmpeg";
                string audioParameter = string.Empty;
                if (audioTrackNumber > 0)
                {
                    audioParameter = $"-map 0:a:{audioTrackNumber}";
                }

                const string fFmpegWaveTranscodeSettings = "-i \"{0}\" -acodec pcm_s16le -ac 1 -ar 16000 {2} \"{1}\"";
                //-i indicates the input
                //-ac 1 means 1 channel (mono)

                string exeFilePath = Configuration.Settings.General.FFmpegLocation;
                string parameters = string.Format(fFmpegWaveTranscodeSettings, inputVideoFile, outWaveFile, audioParameter);
                return new Process { StartInfo = new ProcessStartInfo(exeFilePath, parameters) { WindowStyle = ProcessWindowStyle.Hidden } };
            }
            else
            {
                encoderName = "VLC";
                string parameters = "\"" + inputVideoFile + "\" -I dummy -vvv --no-random --no-repeat --no-loop --no-sout-video --audio-track=" + audioTrackNumber + " --sout=\"#transcode{acodec=s16l,channels=1,ab=128,samplerate=16000}:std{access=file,mux=wav,dst=" + outWaveFile + "}\" vlc://quit";
                string exeFilePath;
                if (Configuration.IsRunningOnLinux)
                {
                    exeFilePath = "cvlc";
                    parameters = "-vvv --no-random --no-repeat --no-loop --no-sout-video --audio-track=" + audioTrackNumber + " --sout '#transcode{" + encodeParamters + "}:std{mux=wav,access=file,dst=" + outWaveFile + "}' \"" + inputVideoFile + "\" vlc://quit";
                }
                else if (Configuration.IsRunningOnMac)
                {
                    exeFilePath = "VLC.app/Contents/MacOS/VLC";
                }
                else // windows
                {
                    exeFilePath = Logic.VideoPlayers.LibVlcDynamic.GetVlcPath("vlc.exe");
                    if (!File.Exists(exeFilePath))
                    {
                        if (!Configuration.Settings.General.UseFFmpegForWaveExtraction || !File.Exists(Configuration.Settings.General.FFmpegLocation))
                        {
                            throw new DllNotFoundException("NO_VLC");
                        }
                    }
                }
                return new Process { StartInfo = new ProcessStartInfo(exeFilePath, parameters) { WindowStyle = ProcessWindowStyle.Hidden } };
            }
        }

        private void ExtractAudio()
        {
            try
            {
                _waveFileName = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".wav");
                Process process;
                try
                {
                    process = GetCommandLineProcess(_videoFileName, -1, _waveFileName, Configuration.Settings.General.VlcWaveTranscodeSettings, out var waveExtractor);
                    _backgroundWorker.ReportProgress(0, string.Format(LanguageSettings.Current.AudioToText.ExtractingAudioUsingX, waveExtractor));
                }
                catch (DllNotFoundException)
                {
                    if (MessageBox.Show(LanguageSettings.Current.AddWaveform.VlcMediaPlayerNotFound + Environment.NewLine +
                                        Environment.NewLine + LanguageSettings.Current.AddWaveform.GoToVlcMediaPlayerHomePage,
                                        LanguageSettings.Current.AddWaveform.VlcMediaPlayerNotFoundTitle,
                                        MessageBoxButtons.YesNo) == DialogResult.Yes)
                    {
                        UiUtil.OpenUrl("http://www.videolan.org/");
                    }
                    return;
                }

                process.Start();
                while (!process.HasExited && !_abort)
                {
                    Application.DoEvents();
                }

                // check for delay in matroska files
                var mkvAudioTrackNumbers = new Dictionary<int, int>();
                if (_videoFileName.ToLowerInvariant().EndsWith(".mkv", StringComparison.OrdinalIgnoreCase))
                {
                    MatroskaFile matroska = null;
                    try
                    {
                        matroska = new MatroskaFile(_videoFileName);

                        if (matroska.IsValid)
                        {
                            foreach (var track in matroska.GetTracks())
                            {
                                if (track.IsAudio)
                                {
                                    var audioTrackNames = new List<string>();
                                    if (track.CodecId != null && track.Language != null)
                                    {
                                        audioTrackNames.Add("#" + track.TrackNumber + ": " + track.CodecId.Replace("\0", string.Empty) + " - " + track.Language.Replace("\0", string.Empty));
                                    }
                                    else
                                    {
                                        audioTrackNames.Add("#" + track.TrackNumber);
                                    }

                                    mkvAudioTrackNumbers.Add(mkvAudioTrackNumbers.Count, track.TrackNumber);
                                }
                            }
                            if (mkvAudioTrackNumbers.Count > 0)
                            {
                                _delayInMilliseconds = (int)matroska.GetTrackStartTime(mkvAudioTrackNumbers[0]);
                            }
                        }
                    }
                    catch
                    {
                        _delayInMilliseconds = 0;
                    }
                    finally
                    {
                        matroska?.Dispose();
                    }
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);

            }
        }

        private string ExtractTextFromAudio(string targetFile, int delayInMilliseconds)
        {
            var output = new StringBuilder();
            var path = Path.Combine(Configuration.DataDirectory, "pocketsphinx");
            var fileName = Path.Combine(path, "bin", "Release", "Win32", "pocketsphinx_continuous.exe");
            var hmm = Path.Combine(path, "model", "en-us", "en-us");
            var lm = Path.Combine(path, "model", "en-us", "en-us.lm.bin");
            var dict = Path.Combine(path, "model", "en-us", "cmudict-en-us.dict");
            var pocketPhinxParams = $"-infile \"{targetFile}\" -hmm \"{hmm}\" -lm \"{lm}\" -dict \"{dict}\" -time yes"; // > \"{_resultFile}\"";

            var process = new Process { StartInfo = new ProcessStartInfo(fileName, pocketPhinxParams) { CreateNoWindow = true, WindowStyle = ProcessWindowStyle.Hidden } };
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.UseShellExecute = false;

            using (AutoResetEvent outputWaitHandle = new AutoResetEvent(false))
            using (AutoResetEvent errorWaitHandle = new AutoResetEvent(false))
            {
                process.OutputDataReceived += (sender, e) =>
                {
                    if (_abort)
                    {
                        return;
                    }

                    if (e.Data == null)
                    {
                        outputWaitHandle.Set();
                    }
                    else
                    {
                        output.AppendLine(e.Data);
                        var seconds = GetLastTimeStampInSeconds(e.Data);
                        if (seconds > 0)
                        {
                            _backgroundWorker.ReportProgress(seconds);
                        }
                        _backgroundWorker.ReportProgress(LogOutput, e.Data);
                    }
                };
                process.ErrorDataReceived += (sender, e) =>
                {
                    if (_abort)
                    {
                        return;
                    }

                    if (e.Data == null)
                    {
                        errorWaitHandle.Set();
                    }
                    else
                    {
                        _backgroundWorker.ReportProgress(LogInfo, e.Data);
                    }
                };

                process.Start();

                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                var killed = false;
                while (!process.HasExited)
                {
                    Application.DoEvents();
                    Thread.Sleep(50);
                    if (_abort && !killed)
                    {
                        process.Kill();
                        killed = true;
                    }
                }

            }
            return output.ToString();
        }

        private static int GetLastTimeStampInSeconds(string text)
        {
            var lines = text.SplitToLines();

            for (int i = lines.Count - 1; i >= 0; i--)
            {
                string line = lines[i];
                if (line.StartsWith('<') || line.StartsWith('['))
                {
                    continue;
                }

                var words = line.Split();
                if (words.Length != 4)
                {
                    continue;
                }

                if (double.TryParse(words[1], out _) && double.TryParse(words[2], out var end))
                {
                    return (int)Math.Round(end);
                }
            }

            return -1;
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            _abort = true;
            DialogResult = DialogResult.OK;
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            _abort = true;
            _backgroundWorker.CancelAsync();
            DialogResult = DialogResult.Cancel;
        }

        public static Stream GenerateStreamFromString(string s)
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(s);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }

        private Subtitle Start()
        {
            ExtractAudio();

            Subtitle subtitle;
            _backgroundWorker.ReportProgress(0, string.Format(LanguageSettings.Current.AudioToText.ExtractingTextUsingX, "PocketSphinx"));
            var result = ExtractTextFromAudio(_waveFileName, _delayInMilliseconds);
            using (var stream = GenerateStreamFromString(result))
            {
                var reader = new ResultReader(stream);
                var results = reader.Parse();
                var subtitleGenerator = new SubtitleGenerator(results);
                subtitle = subtitleGenerator.Generate("en");
            }

            // cleanup
            try
            {
                File.Delete(_waveFileName);
            }
            catch
            {
                // don't show error about unsuccessful delete
            }

            return subtitle;
        }

        private void AudioToText_Load(object sender, EventArgs e)
        {
            buttonOK.Enabled = false;
            _backgroundWorker.DoWork += _backgroundWorker_DoWork;
            _backgroundWorker.RunWorkerCompleted += _backgroundWorker_RunWorkerCompleted;
            _backgroundWorker.WorkerReportsProgress = true;
            _backgroundWorker.ProgressChanged += _backgroundWorker_ProgressChanged;
            _backgroundWorker.WorkerSupportsCancellation = true;
            _backgroundWorker.RunWorkerAsync();
            progressBar1.Style = ProgressBarStyle.Marquee;
            progressBar1.Visible = true;
        }

        private void _backgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            Subtitle = (Subtitle)e.Result;
            labelProgress.Text = string.Format(LanguageSettings.Current.AudioToText.ProgessViaXy, "PocketSphinx", 100);
            progressBar1.Visible = false;
            buttonOK.Enabled = true;
        }

        private void _backgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            e.Result = Start();
        }

        private void _backgroundWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (e.ProgressPercentage == LogInfo)
            {
                textBoxLog.AppendText(Environment.NewLine + e.UserState);
                return;
            }

            if (e.ProgressPercentage == LogOutput)
            {
                textBoxOutput.AppendText(Environment.NewLine + e.UserState);
                return;
            }

            if (e.UserState is string)
            {
                labelProgress.Text = e.UserState.ToString();
                return;
            }

            var positionInSeconds = e.ProgressPercentage;
            var percentage = (int)Math.Round(positionInSeconds * 100.0 / (_videoInfo.TotalMilliseconds / 1000.0));
            if (percentage > 100)
            {
                percentage = 100;
            }

            if (progressBar1.Style == ProgressBarStyle.Marquee)
            {
                progressBar1.Style = ProgressBarStyle.Blocks;
                progressBar1.Maximum = 100;
            }
            progressBar1.Value = percentage;
            labelProgress.Text = string.Format(LanguageSettings.Current.AudioToText.ProgessViaXy, "PocketSphinx", percentage);
        }

        private void linkLabelShowMoreLess_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            _showMore = !_showMore;
            if (_showMore)
            {
                linkLabelShowMoreLess.Text = LanguageSettings.Current.AudioToText.ShowLess;
                Height = 500;
            }
            else
            {
                linkLabelShowMoreLess.Text = LanguageSettings.Current.AudioToText.ShowMore;
                Height = linkLabelShowMoreLess.Top + linkLabelShowMoreLess.Height + buttonOK.Height + 60;
            }
            labelLog.Visible = _showMore;
            textBoxLog.Visible = _showMore;
            labelOutput.Visible = _showMore;
            textBoxOutput.Visible = _showMore;
        }

        private void AudioToText_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!_abort)
            {
                e.Cancel = true;
            }
        }

        private void AudioToText_Shown(object sender, EventArgs e)
        {
            linkLabelShowMoreLess_LinkClicked(null, null);
        }
    }
}
