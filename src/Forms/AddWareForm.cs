using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Nikse.SubtitleEdit.Logic;

namespace Nikse.SubtitleEdit.Forms
{
    public sealed partial class AddWareForm : Form
    {
        public string SourceVideoFileName { get; private set; }
        private bool _cancel = false;
        private string _wavFileName = null;
        private string _spectrogramDirectory;
        public List<Bitmap> SpectrogramBitmaps { get; private set; }
        private string _encodeParamters;
        private const string RetryEncodeParameters = "acodec=s16l";
        private int _audioTrackNumber = -1;
        private int _delayInMilliseconds = 0;

        public AddWareForm()
        {
            InitializeComponent();
            labelProgress.Text = string.Empty;
            buttonCancel.Visible = false;
            labelInfo.Text = string.Empty;
        }

        public WavePeakGenerator WavePeak { get; private set; }

        public void Initialize(string videoFile, string spectrogramDirectory, int audioTrackNumber)
        {
            _audioTrackNumber = audioTrackNumber;
            if (_audioTrackNumber < 0)
                _audioTrackNumber = 0;
            Text = Configuration.Settings.Language.AddWaveForm.Title;
            buttonRipWave.Text = Configuration.Settings.Language.AddWaveForm.GenerateWaveFormData;
            labelPleaseWait.Text = Configuration.Settings.Language.AddWaveForm.PleaseWait;
            labelVideoFileName.Text = videoFile;
            buttonCancel.Text = Configuration.Settings.Language.General.Cancel;
            labelSourcevideoFile.Text = Configuration.Settings.Language.AddWaveForm.SourceVideoFile;
            _spectrogramDirectory = spectrogramDirectory;
            _encodeParamters = Configuration.Settings.General.VlcWaveTranscodeSettings;
        }

        private void buttonRipWave_Click(object sender, EventArgs e)
        {
            buttonRipWave.Enabled = false;
            _cancel = false;
            bool runningOnWindows = false;

            SourceVideoFileName = labelVideoFileName.Text;
            string targetFile = Path.GetTempFileName() + ".wav";
//            string parameters = "-I dummy -vvv \"" + SourceVideoFileName + "\" --sout=#transcode{vcodec=none,acodec=s16l}:file{dst=\"" + targetFile + "\"}  vlc://quit";
            string parameters = "-I dummy -vvv --no-sout-video --audio-track=" + _audioTrackNumber.ToString() + " --sout #transcode{" + _encodeParamters + "}:std{mux=wav,access=file,dst=\"" + targetFile + "\"} \"" + SourceVideoFileName + "\" vlc://quit";
            string exeFilePath;
            if (Utilities.IsRunningOnLinux() || Utilities.IsRunningOnMac())
            {
                exeFilePath = "cvlc";
                parameters = "-vvv --no-sout-video --audio-track=" + _audioTrackNumber.ToString() + " --sout '#transcode{" + _encodeParamters + "}:std{mux=wav,access=file,dst=" + targetFile + "}' \"" + SourceVideoFileName + "\" vlc://quit";
            }
            else // windows
            {
                runningOnWindows = true;
                exeFilePath = Nikse.SubtitleEdit.Logic.VideoPlayers.LibVlc11xDynamic.GetVlcPath("vlc.exe");
                if (!System.IO.File.Exists(exeFilePath))
                {
                    if (MessageBox.Show(Configuration.Settings.Language.AddWaveForm.VlcMediaPlayerNotFound + Environment.NewLine +
                                        Environment.NewLine +
                                        Configuration.Settings.Language.AddWaveForm.GoToVlcMediaPlayerHomePage,
                                       Configuration.Settings.Language.AddWaveForm.VlcMediaPlayerNotFoundTitle, MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes)
                    {
                        System.Diagnostics.Process.Start("http://www.videolan.org/");
                    }
                    buttonRipWave.Enabled = true;
                    return;
                }
            }

            labelInfo.Text = "VCL";
            if (Configuration.Settings.General.UseFFMPEGForWaveExtraction && File.Exists(Configuration.Settings.General.FFMPEGLocation) && !string.IsNullOrEmpty(Configuration.Settings.General.FFMPEGWaveTranscodeSettings))
            {
                exeFilePath = Configuration.Settings.General.FFMPEGLocation;
                parameters = string.Format(Configuration.Settings.General.FFMPEGWaveTranscodeSettings, SourceVideoFileName, targetFile);
                //-i indicates the input
                //-ab indicates the bit rate (in this example 160kb/sec)
                //-vn means no video ouput
                //-ac 2 means 2 channels
                //-ar 44100 indicates the sampling frequency.
                labelInfo.Text = "FFMPEG";
            } 
            
            labelPleaseWait.Visible = true;
            Process process = new Process();
            process.StartInfo = new ProcessStartInfo(exeFilePath, parameters);
            process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            process.Start();
            progressBar1.Style = ProgressBarStyle.Marquee;
            progressBar1.Visible = true;
            double seconds = 0;
            buttonCancel.Visible = true;
            try
            {
                process.PriorityClass = ProcessPriorityClass.BelowNormal;
            }
            catch
            {
            }
            while (!process.HasExited)
            {
                Application.DoEvents();
                System.Threading.Thread.Sleep(100);
                seconds += 0.1;
                if (seconds < 60)
                    labelProgress.Text = string.Format(Configuration.Settings.Language.AddWaveForm.ExtractingSeconds, seconds);
                else
                    labelProgress.Text = string.Format(Configuration.Settings.Language.AddWaveForm.ExtractingMinutes, (int)(seconds / 60), (int)(seconds % 60));
                this.Refresh();
                if (_cancel)
                {
                    process.Kill();
                    progressBar1.Visible = false;
                    labelPleaseWait.Visible = false;
                    buttonRipWave.Enabled = true;
                    targetFile = null;
                    buttonCancel.Visible = false;
                    DialogResult = DialogResult.Cancel;
                    return;
                }

                if (seconds > 1 && Convert.ToInt32(seconds) % 60 == 0 && runningOnWindows)
                {
                    try
                    {
                        var drive = new DriveInfo("c");
                        if (drive.IsReady)
                        {
                            if (drive.AvailableFreeSpace < 50 * 1000000) // 50 mb
                            {
                                labelInfo.ForeColor = Color.Red;
                                labelInfo.Text = "LOW DISC SPACE!";
                            }
                            else if (labelInfo.ForeColor == Color.Red)
                            {
                                labelInfo.Text = Utilities.FormatBytesToDisplayFileSize(drive.AvailableFreeSpace) + " free";
                            }
                        }
                    }
                    catch
                    { 
                    }
                }

            }
            buttonCancel.Visible = false;
            progressBar1.Visible = false;
            progressBar1.Style = ProgressBarStyle.Blocks;

            if (!File.Exists(targetFile))
            {
                if (_encodeParamters != RetryEncodeParameters)
                {
                    _encodeParamters = RetryEncodeParameters;
                    buttonRipWave_Click(null, null);
                    return;
                }

                MessageBox.Show("Could not find extracted wave file! This feature requires VLC media player 1.1.x or newer (32-bit)." + Environment.NewLine
                                + Environment.NewLine +
                                "Command line: " + exeFilePath + " " + parameters);

                labelPleaseWait.Visible = false;
                labelProgress.Text = string.Empty;
                buttonRipWave.Enabled = true;
                return;
            }

            FileInfo fi = new FileInfo(targetFile);
            if (fi.Length <= 200)
            {
                MessageBox.Show("Sorry! VLC was unable to extract audio to wave file via this command line:" + Environment.NewLine
                                + Environment.NewLine +
                                "Command line: " + exeFilePath + " " + parameters);

                labelPleaseWait.Visible = false;
                labelProgress.Text = string.Empty;
                buttonRipWave.Enabled = true;
                return;
            }

            ReadWaveFile(targetFile, _delayInMilliseconds);
            labelProgress.Text = string.Empty;
            File.Delete(targetFile);
            this.DialogResult = DialogResult.OK;
        }

        private void ReadWaveFile(string targetFile, int delayInMilliseconds)
        {
            WavePeakGenerator waveFile = new WavePeakGenerator(targetFile);

            int sampleRate = Configuration.Settings.VideoControls.WaveFormMininumSampleRate; // Normally 128
            while (!(waveFile.Header.SampleRate % sampleRate == 0) && sampleRate < 5000)
                sampleRate++; // old sample-rate / new sample-rate must have rest = 0

            labelProgress.Text = Configuration.Settings.Language.AddWaveForm.GeneratingPeakFile;
            this.Refresh();
            waveFile.GeneratePeakSamples(sampleRate, delayInMilliseconds); // samples per second - SampleRate

            if (Configuration.Settings.VideoControls.GenerateSpectrogram)
            {
                labelProgress.Text = Configuration.Settings.Language.AddWaveForm.GeneratingSpectrogram;
                this.Refresh();
                System.IO.Directory.CreateDirectory(_spectrogramDirectory);
                SpectrogramBitmaps = waveFile.GenerateFourierData(256, _spectrogramDirectory);
            }
            labelPleaseWait.Visible = false;

            WavePeak = waveFile;
            waveFile.Close();
        }

        private void AddWareForm_Shown(object sender, EventArgs e)
        {
            if (labelVideoFileName.Text.Length > 1 && File.Exists(labelVideoFileName.Text))
            {
                if (labelVideoFileName.Text.ToLower().EndsWith(".mkv"))
                {
                    var mkv = new Matroska(labelVideoFileName.Text);
                    if (mkv.IsValid)
                    {
                        var trackInfo = mkv.GetTrackInfo();
                        int numberOfAudioTracks = 0;
                        int trackNumber = 2;
                        foreach (var ti in trackInfo)
                        {
                            if (ti.IsAudio)
                            {
                                numberOfAudioTracks++;
                                trackNumber = ti.TrackNumber;
                                break;
                            }
                        }
                        if (numberOfAudioTracks == 1)
                        {
                            _delayInMilliseconds = (int)mkv.GetTrackStartTime(trackNumber);
                        }
                    }                    
                }

                buttonRipWave_Click(null, null);
            }
            else if (_wavFileName != null)
            {
                FixWaveOnly();
            }
        }

        private void AddWareForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
                DialogResult = DialogResult.Cancel;
            else if (e.KeyCode == Keys.F1)
            {
                Utilities.ShowHelp("#waveform");
                e.SuppressKeyPress = true;
            }
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            _cancel = true;
        }

        internal void InitializeViaWaveFile(string fileName)
        {
            _wavFileName = fileName;
        }

        private void FixWaveOnly()
        {
            Text = Configuration.Settings.Language.AddWaveForm.Title;
            buttonRipWave.Text = Configuration.Settings.Language.AddWaveForm.GenerateWaveFormData;
            labelPleaseWait.Text = Configuration.Settings.Language.AddWaveForm.PleaseWait;
            labelVideoFileName.Text = string.Empty;
            buttonCancel.Text = Configuration.Settings.Language.General.Cancel;
            buttonRipWave.Enabled = false;
            _cancel = false;
            buttonCancel.Visible = false;
            progressBar1.Visible = false;
            progressBar1.Style = ProgressBarStyle.Blocks;

            labelProgress.Text = Configuration.Settings.Language.AddWaveForm.GeneratingPeakFile;
            this.Refresh();
            labelPleaseWait.Visible = false;
            try
            {
                ReadWaveFile(_wavFileName, _delayInMilliseconds);
                labelProgress.Text = string.Empty;
                this.DialogResult = DialogResult.OK;
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message + Environment.NewLine + exception.StackTrace);
                this.DialogResult = DialogResult.Cancel;
            }
        }
    }
}
