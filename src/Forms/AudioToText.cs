using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Nikse.SubtitleEdit.Core;
using Nikse.SubtitleEdit.Core.AudioToText.PhocketSphinx;
using Nikse.SubtitleEdit.Core.ContainerFormats.Matroska;

namespace Nikse.SubtitleEdit.Forms
{
    public partial class AudioToText : Form
    {
        public Subtitle Subtitle { get; set; }
        private readonly string _videoFileName;
        private int _delayInMilliseconds;
        private bool _abort;
        private string _waveFileName;
        private readonly BackgroundWorker _backgroundWorker;

        private static readonly StringBuilder Output = new StringBuilder();
        private static readonly StringBuilder Error = new StringBuilder();


        public AudioToText(string videoFileName)
        {
            InitializeComponent();
            _videoFileName = videoFileName;
            _backgroundWorker = new BackgroundWorker();
        }

        public static Process GetCommandLineProcess(string inputVideoFile, int audioTrackNumber, string outWaveFile, string encodeParamters, out string encoderName)
        {
            encoderName = "FFmpeg";
            string audioParameter = string.Empty;
            if (audioTrackNumber > 0)
                audioParameter = $"-map 0:a:{audioTrackNumber}";

            const string fFmpegWaveTranscodeSettings = "-i \"{0}\" -acodec pcm_s16le -ac 1 -ar 16000 {2} \"{1}\"";
            //-i indicates the input
            //-ac 1 means 1 channel (mono)


            string exeFilePath = Configuration.Settings.General.FFmpegLocation;
            string parameters = string.Format(fFmpegWaveTranscodeSettings, inputVideoFile, outWaveFile, audioParameter);
            return new Process { StartInfo = new ProcessStartInfo(exeFilePath, parameters) { WindowStyle = ProcessWindowStyle.Hidden } };
        }


        private void ExtractAudio()
        {
            try
            {
                _waveFileName = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".wav");
                Process process;
                try
                {
                    process = GetCommandLineProcess(_videoFileName, -1, _waveFileName, Configuration.Settings.General.VlcWaveTranscodeSettings, out _);
                }
                catch (DllNotFoundException)
                {
                    if (MessageBox.Show(Configuration.Settings.Language.AddWaveform.VlcMediaPlayerNotFound + Environment.NewLine +
                                        Environment.NewLine + Configuration.Settings.Language.AddWaveform.GoToVlcMediaPlayerHomePage,
                                        Configuration.Settings.Language.AddWaveform.VlcMediaPlayerNotFoundTitle,
                                        MessageBoxButtons.YesNo) == DialogResult.Yes)
                    {
                        Process.Start("http://www.videolan.org/");
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
                if (_videoFileName.ToLower().EndsWith(".mkv", StringComparison.OrdinalIgnoreCase))
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
                                        audioTrackNames.Add("#" + track.TrackNumber + ": " + track.CodecId.Replace("\0", string.Empty) + " - " + track.Language.Replace("\0", string.Empty));
                                    else
                                        audioTrackNames.Add("#" + track.TrackNumber);
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
            var path = Path.Combine(Configuration.DataDirectory, "pocketsphinx");
            var fileName = Path.Combine(path, "bin", "Release", "Win32", "pocketsphinx_continuous.exe");
            //            var fileName = Path.Combine(path, "bin", "Release", "x64", "pocketsphinx_continuous.exe");
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
                    if (e.Data == null)
                    {
                        outputWaitHandle.Set();
                    }
                    else
                    {
                        Output.AppendLine(e.Data);
                    }
                };
                process.ErrorDataReceived += (sender, e) =>
                {
                    if (e.Data == null)
                    {
                        errorWaitHandle.Set();
                    }
                    else
                    {
                        Error.AppendLine(e.Data);
                    }
                };

                process.Start();

                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
                var timeout = 1000 * 60 * 60; //60 min timeout

                if (process.WaitForExit(timeout) &&
                    outputWaitHandle.WaitOne(timeout) &&
                    errorWaitHandle.WaitOne(timeout))
                {
                    //Console.WriteLine("Done");
                    // Process completed. Check process.ExitCode here.
                }
                else
                {
                    // Console.WriteLine("Timeout");
                    // Timed out.
                }
            }
            return Output.ToString();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var fileName = @"E:\PocketSphinx\pocketsphinx\a.txt";
            using (var s = new FileStream(fileName, FileMode.Open))
            {
                var reader = new ResultReader(s);
                var results = reader.Parse();
                var subtitleGenerator = new SubtitleGenerator(results);
                Subtitle = subtitleGenerator.Generate();
            }
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
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

        private Subtitle Start(string videoFileName)
        {
            //labelStatus.Text = "Extracting audio from video...";
            ExtractAudio();
            Subtitle subtitle = new Subtitle();

            var result = ExtractTextFromAudio(_waveFileName, _delayInMilliseconds);
            using (var stream = GenerateStreamFromString(result))
            {
                var reader = new ResultReader(stream);
                var results = reader.Parse();
                var subtitleGenerator = new SubtitleGenerator(results);
                subtitle = subtitleGenerator.Generate();
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
            _backgroundWorker.DoWork += _backgroundWorker_DoWork;
            _backgroundWorker.RunWorkerCompleted += _backgroundWorker_RunWorkerCompleted;
            _backgroundWorker.RunWorkerAsync(_videoFileName);
            progressBar1.Style = ProgressBarStyle.Marquee;
            progressBar1.Visible = true;
            labelStatus.Text = "Extracting text from video - this will take a while...";
        }

        private void _backgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            Subtitle = (Subtitle)e.Result;
            labelStatus.Text = "Done extracing text from video.";
            textBoxOutput.Text = Output.ToString();
            textBoxLog.Text = Error.ToString();
            progressBar1.Visible = false;
        }

        private void _backgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            e.Result = Start((string)e.Argument);
        }
    }
}
