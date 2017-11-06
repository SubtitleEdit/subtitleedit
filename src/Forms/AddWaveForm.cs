﻿using Nikse.SubtitleEdit.Core;
using Nikse.SubtitleEdit.Core.ContainerFormats.Matroska;
using Nikse.SubtitleEdit.Core.ContainerFormats.Mp4;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Windows.Forms;
using Nikse.SubtitleEdit.Logic;

namespace Nikse.SubtitleEdit.Forms
{
    public sealed partial class AddWaveform : Form
    {
        public string SourceVideoFileName { get; private set; }
        private bool _cancel;
        private string _peakWaveFileName;
        private string _wavFileName;
        private string _spectrogramDirectory;
        public WavePeakData Peaks { get; private set; }
        public SpectrogramData Spectrogram { get; private set; }
        private string _encodeParamters;
        private const string RetryEncodeParameters = "acodec=s16l";
        private int _audioTrackNumber = -1;
        private int _delayInMilliseconds;

        public AddWaveform()
        {
            UiUtil.PreInitialize(this);
            InitializeComponent();
            UiUtil.FixFonts(this);
            labelProgress.Text = string.Empty;
            buttonCancel.Visible = false;
            labelInfo.Text = string.Empty;
            UiUtil.FixLargeFonts(this, buttonCancel);
        }

        public void Initialize(string videoFile, string peakWaveFileName, string spectrogramDirectory, int audioTrackNumber)
        {
            _peakWaveFileName = peakWaveFileName;
            _audioTrackNumber = audioTrackNumber;
            if (_audioTrackNumber < 0)
                _audioTrackNumber = 0;
            Text = Configuration.Settings.Language.AddWaveform.Title;
            buttonRipWave.Text = Configuration.Settings.Language.AddWaveform.GenerateWaveformData;
            labelPleaseWait.Text = Configuration.Settings.Language.AddWaveform.PleaseWait;
            labelVideoFileName.Text = videoFile;
            buttonCancel.Text = Configuration.Settings.Language.General.Cancel;
            labelSourcevideoFile.Text = Configuration.Settings.Language.AddWaveform.SourceVideoFile;
            _spectrogramDirectory = spectrogramDirectory;
            _encodeParamters = Configuration.Settings.General.VlcWaveTranscodeSettings;
        }

        public static Process GetCommandLineProcess(string inputVideoFile, int audioTrackNumber, string outWaveFile, string encodeParamters, out string encoderName)
        {
            encoderName = "VLC";
            string parameters = "\"" + inputVideoFile + "\" -I dummy -vvv --no-random --no-repeat --no-loop --no-sout-video --audio-track=" + audioTrackNumber + " --sout=\"#transcode{acodec=s16l,channels=1,ab=128}:std{access=file,mux=wav,dst=" + outWaveFile + "}\" vlc://quit";
            string exeFilePath;
            if (Configuration.IsRunningOnLinux())
            {
                exeFilePath = "cvlc";
                parameters = "-vvv --no-random --no-repeat --no-loop --no-sout-video --audio-track=" + audioTrackNumber + " --sout '#transcode{" + encodeParamters + "}:std{mux=wav,access=file,dst=" + outWaveFile + "}' \"" + inputVideoFile + "\" vlc://quit";
            }
            else if (Configuration.IsRunningOnMac())
            {
                exeFilePath = "VLC.app/Contents/MacOS/VLC";
            }
            else // windows
            {
                exeFilePath = Logic.VideoPlayers.LibVlcDynamic.GetVlcPath("vlc.exe");
                if (!File.Exists(exeFilePath))
                {
                    if (Configuration.Settings.General.UseFFmpegForWaveExtraction && File.Exists(Configuration.Settings.General.FFmpegLocation))
                    {
                        // We will run FFmpeg
                    }
                    else
                    {
                        throw new DllNotFoundException("NO_VLC");
                    }
                }
            }

            if (Configuration.Settings.General.UseFFmpegForWaveExtraction && File.Exists(Configuration.Settings.General.FFmpegLocation))
            {
                encoderName = "FFmpeg";
                string audioParameter = string.Empty;
                if (audioTrackNumber > 0)
                    audioParameter = string.Format("-map 0:a:{0}", audioTrackNumber);

                const string fFmpegWaveTranscodeSettings = "-i \"{0}\" -vn -ar 24000 -ac 2 -ab 128 -vol 448 -f wav {2} \"{1}\"";
                //-i indicates the input
                //-vn means no video ouput
                //-ar 44100 indicates the sampling frequency.
                //-ab indicates the bit rate (in this example 160kb/s)
                //-vol 448 will boot volume... 256 is normal
                //-ac 2 means 2 channels

                // "-map 0:a:0" is the first audio stream, "-map 0:a:1" is the second audio stream

                exeFilePath = Configuration.Settings.General.FFmpegLocation;
                parameters = string.Format(fFmpegWaveTranscodeSettings, inputVideoFile, outWaveFile, audioParameter);
            }
            return new Process { StartInfo = new ProcessStartInfo(exeFilePath, parameters) { WindowStyle = ProcessWindowStyle.Hidden } };
        }

        private void buttonRipWave_Click(object sender, EventArgs e)
        {
            buttonRipWave.Enabled = false;
            _cancel = false;
            SourceVideoFileName = labelVideoFileName.Text;
            string targetFile = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".wav");
            string targetDriveLetter = null;
            if (!(Configuration.IsRunningOnLinux() || Configuration.IsRunningOnMac()))
            {
                var root = Path.GetPathRoot(targetFile);
                if (root.Length > 1 && root[1] == ':')
                {
                    targetDriveLetter = root.Remove(1);
                }
            }

            labelPleaseWait.Visible = true;
            string encoderName;
            Process process;
            try
            {
                process = GetCommandLineProcess(SourceVideoFileName, _audioTrackNumber, targetFile, _encodeParamters, out encoderName);
                labelInfo.Text = encoderName;
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
                buttonRipWave.Enabled = true;
                return;
            }

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
            }
            while (!process.HasExited)
            {
                Application.DoEvents();
                System.Threading.Thread.Sleep(100);
                seconds += 0.1;
                if (seconds < 60)
                    labelProgress.Text = string.Format(Configuration.Settings.Language.AddWaveform.ExtractingSeconds, seconds);
                else
                    labelProgress.Text = string.Format(Configuration.Settings.Language.AddWaveform.ExtractingMinutes, (int)(seconds / 60), (int)(seconds % 60));
                Refresh();
                if (_cancel)
                {
                    process.Kill();
                    progressBar1.Visible = false;
                    labelPleaseWait.Visible = false;
                    buttonRipWave.Enabled = true;
                    buttonCancel.Visible = false;
                    DialogResult = DialogResult.Cancel;
                    return;
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
                                labelInfo.Text = Configuration.Settings.Language.AddWaveform.LowDiskSpace;
                            }
                            else if (labelInfo.ForeColor == Color.Red)
                            {
                                labelInfo.Text = string.Format(Configuration.Settings.Language.AddWaveform.FreeDiskSpace, Utilities.FormatBytesToDisplayFileSize(drive.AvailableFreeSpace));
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
            process.Dispose();

            var targetFileInfo = new FileInfo(targetFile);
            if (!targetFileInfo.Exists)
            {
                if (_encodeParamters != RetryEncodeParameters)
                {
                    _encodeParamters = RetryEncodeParameters;
                    buttonRipWave_Click(null, null);
                    return;
                }

                MessageBox.Show(string.Format(Configuration.Settings.Language.AddWaveform.WaveFileNotFound, IntPtr.Size * 8, process.StartInfo.FileName, process.StartInfo.Arguments));

                labelPleaseWait.Visible = false;
                labelProgress.Text = string.Empty;
                buttonRipWave.Enabled = true;
                return;
            }

            if (targetFileInfo.Length <= 200)
            {
                MessageBox.Show(string.Format(Configuration.Settings.Language.AddWaveform.WaveFileMalformed, encoderName, process.StartInfo.FileName, process.StartInfo.Arguments));

                labelPleaseWait.Visible = false;
                labelProgress.Text = string.Empty;
                buttonRipWave.Enabled = true;
                return;
            }

            ReadWaveFile(targetFile, _delayInMilliseconds);
            labelProgress.Text = string.Empty;
            File.Delete(targetFile);
            DialogResult = DialogResult.OK;
        }

        private void ReadWaveFile(string targetFile, int delayInMilliseconds)
        {
            labelProgress.Text = Configuration.Settings.Language.AddWaveform.GeneratingPeakFile;
            Refresh();

            using (var waveFile = new WavePeakGenerator(targetFile))
            {
                Peaks = waveFile.GeneratePeaks(delayInMilliseconds, _peakWaveFileName);

                if (Configuration.Settings.VideoControls.GenerateSpectrogram)
                {
                    labelProgress.Text = Configuration.Settings.Language.AddWaveform.GeneratingSpectrogram;
                    Refresh();
                    Spectrogram = waveFile.GenerateSpectrogram(delayInMilliseconds, _spectrogramDirectory);
                }
            }

            labelPleaseWait.Visible = false;
        }

        private void AddWaveform_Shown(object sender, EventArgs e)
        {
            Refresh();
            var audioTrackNames = new List<string>();
            var mkvAudioTrackNumbers = new Dictionary<int, int>();
            int numberOfAudioTracks = 0;
            if (labelVideoFileName.Text.Length > 1 && File.Exists(labelVideoFileName.Text))
            {
                if (labelVideoFileName.Text.EndsWith(".mkv", StringComparison.OrdinalIgnoreCase))
                { // Choose for number of audio tracks in matroska files
                    MatroskaFile matroska = null;
                    try
                    {
                        matroska = new MatroskaFile(labelVideoFileName.Text);
                        if (matroska.IsValid)
                        {
                            foreach (var track in matroska.GetTracks())
                            {
                                if (track.IsAudio)
                                {
                                    numberOfAudioTracks++;
                                    if (track.CodecId != null && track.Language != null)
                                        audioTrackNames.Add("#" + track.TrackNumber + ": " + track.CodecId.Replace("\0", string.Empty) + " - " + track.Language.Replace("\0", string.Empty));
                                    else
                                        audioTrackNames.Add("#" + track.TrackNumber);
                                    mkvAudioTrackNumbers.Add(mkvAudioTrackNumbers.Count, track.TrackNumber);
                                }
                            }
                        }
                    }
                    finally
                    {
                        if (matroska != null)
                        {
                            matroska.Dispose();
                        }
                    }
                }
                else if (labelVideoFileName.Text.EndsWith(".mp4", StringComparison.OrdinalIgnoreCase) || labelVideoFileName.Text.EndsWith(".m4v", StringComparison.OrdinalIgnoreCase))
                { // Choose for number of audio tracks in mp4 files
                    try
                    {
                        var mp4 = new MP4Parser(labelVideoFileName.Text);
                        var tracks = mp4.GetAudioTracks();
                        int i = 0;
                        foreach (var track in tracks)
                        {
                            i++;
                            if (track.Name != null && track.Mdia != null && track.Mdia.Mdhd != null && track.Mdia.Mdhd.LanguageString != null)
                                audioTrackNames.Add(i + ":  " + track.Name + " - " + track.Mdia.Mdhd.LanguageString);
                            else if (track.Name != null)
                                audioTrackNames.Add(i + ":  " + track.Name);
                            else
                                audioTrackNames.Add(i.ToString(CultureInfo.InvariantCulture));
                        }
                        numberOfAudioTracks = tracks.Count;
                    }
                    catch
                    {
                    }
                }

                // Choose audio track
                if (numberOfAudioTracks > 1)
                {
                    using (var form = new ChooseAudioTrack(audioTrackNames, _audioTrackNumber))
                    {
                        if (form.ShowDialog(this) == DialogResult.OK)
                        {
                            _audioTrackNumber = form.SelectedTrack;
                        }
                        else
                        {
                            DialogResult = DialogResult.Cancel;
                            return;
                        }
                    }
                }

                // check for delay in matroska files
                if (labelVideoFileName.Text.EndsWith(".mkv", StringComparison.OrdinalIgnoreCase))
                {
                    MatroskaFile matroska = null;
                    try
                    {
                        matroska = new MatroskaFile(labelVideoFileName.Text);
                        if (matroska.IsValid)
                        {
                            _delayInMilliseconds = (int)matroska.GetTrackStartTime(mkvAudioTrackNumbers[_audioTrackNumber]);
                        }
                    }
                    catch
                    {
                        _delayInMilliseconds = 0;
                    }
                    finally
                    {
                        if (matroska != null)
                        {
                            matroska.Dispose();
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

        private void AddWaveform_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
                DialogResult = DialogResult.Cancel;
            else if (e.KeyCode == UiUtil.HelpKeys)
            {
                Utilities.ShowHelp("#waveform");
                e.SuppressKeyPress = true;
            }
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            _cancel = true;
        }

        internal void InitializeViaWaveFile(string fileName, string peakWaveFileName, string spectrogramFolder)
        {
            _peakWaveFileName = peakWaveFileName;
            _wavFileName = fileName;
            _spectrogramDirectory = spectrogramFolder;
        }

        private void FixWaveOnly()
        {
            Text = Configuration.Settings.Language.AddWaveform.Title;
            buttonRipWave.Text = Configuration.Settings.Language.AddWaveform.GenerateWaveformData;
            labelPleaseWait.Text = Configuration.Settings.Language.AddWaveform.PleaseWait;
            labelVideoFileName.Text = string.Empty;
            buttonCancel.Text = Configuration.Settings.Language.General.Cancel;
            buttonRipWave.Enabled = false;
            _cancel = false;
            buttonCancel.Visible = false;
            progressBar1.Visible = false;
            progressBar1.Style = ProgressBarStyle.Blocks;

            labelProgress.Text = Configuration.Settings.Language.AddWaveform.GeneratingPeakFile;
            Refresh();
            labelPleaseWait.Visible = false;
            try
            {
                ReadWaveFile(_wavFileName, _delayInMilliseconds);
                labelProgress.Text = string.Empty;
                DialogResult = DialogResult.OK;
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message + Environment.NewLine + exception.StackTrace);
                DialogResult = DialogResult.Cancel;
            }
        }

    }
}
