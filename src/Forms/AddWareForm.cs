using System;
using System.Diagnostics;
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

        public AddWareForm()
        {
            InitializeComponent();
            labelProgress.Text = string.Empty;
            buttonCancel.Visible = false;
        }

        public WavePeakGenerator WavePeak { get; private set; }

        public void Initialize(string videoFile)
        {
            Text = Configuration.Settings.Language.AddWaveForm.Title;
            buttonRipWave.Text = Configuration.Settings.Language.AddWaveForm.GenerateWaveFormData;
            labelPleaseWait.Text = Configuration.Settings.Language.AddWaveForm.PleaseWait;
            labelVideoFileName.Text = videoFile;
            buttonCancel.Text = Configuration.Settings.Language.General.Cancel;
            labelSourcevideoFile.Text = Configuration.Settings.Language.AddWaveForm.SourceVideoFile;
        }

        private void buttonRipWave_Click(object sender, EventArgs e)
        {
            buttonRipWave.Enabled = false;
            _cancel = false;

            string vlcPath;
            if (Utilities.IsRunningOnLinux() || Utilities.IsRunningOnMac())
            {
                vlcPath = "vlc";
            }
            else // windows
            {
                vlcPath = Nikse.SubtitleEdit.Logic.VideoPlayers.LibVlc11xDynamic.GetVlcPath("vlc.exe");
                if (!System.IO.File.Exists(vlcPath))
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

            labelPleaseWait.Visible = true;
            SourceVideoFileName = labelVideoFileName.Text;
            string targetFile = Path.GetTempFileName() + ".wav";
            string parameters = "-I dummy -vvv \"" + SourceVideoFileName + "\" --sout=#transcode{vcodec=none,acodec=s16l}:file{dst=\"" + targetFile + "\"}  vlc://quit";
            //string parameters = "-I dummy -vvv \"" + _wavFileName + "\" --sout=#transcode{vcodec=none,acodec=s24l}:file{dst=\"" + _targetFile + "\"}  vlc://quit";
            //string parameters = "-I dummy -vvv \"" + _wavFileName + "\" --sout=#transcode{vcodec=none,acodec=s32l}:file{dst=\"" + _targetFile + "\"}  vlc://quit";

            Process process = new Process();
            process.StartInfo = new ProcessStartInfo(vlcPath, parameters);
            process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            process.Start();
            progressBar1.Style = ProgressBarStyle.Marquee;
            progressBar1.Visible = true;
            double seconds = 0;
            buttonCancel.Visible = true;
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
            }
            buttonCancel.Visible = false;
            progressBar1.Visible = false;
            progressBar1.Style = ProgressBarStyle.Blocks;

            if (!File.Exists(targetFile))
            {
                MessageBox.Show("Could not find extracted wave file! This feature requires VLC media player 1.1.x or newer." + Environment.NewLine
                                + Environment.NewLine +
                                "Command line: " + vlcPath + " " + parameters);

                labelPleaseWait.Visible = false;
                labelProgress.Text = string.Empty;
                buttonRipWave.Enabled = true;
                return;
            }
            labelProgress.Text = Configuration.Settings.Language.AddWaveForm.GeneratingPeakFile;
            this.Refresh();
            labelPleaseWait.Visible = false;
            ReadWaveFile(targetFile);
            labelProgress.Text = string.Empty;
            File.Delete(targetFile);
            this.DialogResult = DialogResult.OK;
        }

        private void ReadWaveFile(string targetFile)
        {
            WavePeakGenerator waveFile = new WavePeakGenerator(targetFile);

            int sampleRate = 126;
            while (!(waveFile.Header.SampleRate % sampleRate == 0) && sampleRate < 1000)
                sampleRate++; // old sample-rate / new sample-rate must have rest = 0
            waveFile.GeneratePeakSamples(sampleRate); // samples per second - SampleRate 
                                               
            WavePeak = waveFile;
            waveFile.Close();
        }

        private void AddWareForm_Shown(object sender, EventArgs e)
        {
            if (labelVideoFileName.Text.Length > 1 && File.Exists(labelVideoFileName.Text))
                buttonRipWave_Click(null, null);
            else if (_wavFileName != null)
                FixWaveOnly();
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
                ReadWaveFile(_wavFileName);
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
