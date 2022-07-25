using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Logic;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.Forms
{
    public partial class AudioClipsGet : Form
    {
        public class AudioClip
        {
            public string AudioFileName { get; set; }
            public Paragraph Paragraph { get; set; }
        }

        private bool _abort = false;
        private readonly List<Paragraph> _paragraphs;
        public List<AudioClip> AudioClips { get; set; }
        private readonly string _videoFileName ;
        private readonly int _audioTrackNumber;

        public AudioClipsGet(List<Paragraph> paragraphs, string videoFileName, int audioTrackNumber)
        {
            InitializeComponent();

            _paragraphs = paragraphs;
            _videoFileName = videoFileName;
            _audioTrackNumber = audioTrackNumber;
        }

        private void AudioClipsGet_Shown(object sender, EventArgs e)
        {
            var index = 0;
            progressBar1.Maximum = _paragraphs.Count;
            progressBar1.Value = 0;
            var targetFolder = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(targetFolder);
            AudioClips = new List<AudioClip>();
            while (index < _paragraphs.Count && _abort == false)
            {
                var item = _paragraphs[index];
                progressBar1.Value++;
                try
                {
                    var targetFile = Path.Combine(targetFolder, (index+1)+ ".wav");
                    var audioParameter = string.Empty;
                    if (_audioTrackNumber > 0)
                    {
                        audioParameter = $"-map 0:a:{_audioTrackNumber}";
                    }

                    var start = $"{item.StartTime.TotalSeconds:0.000}".Replace(",", ".");
                    var duration = $"{item.Duration.TotalSeconds:0.000}".Replace(",", ".");
                    var fFmpegWaveTranscodeSettings = "-ss " + start + " -t " + duration + " -i \"{0}\" -vn -ar 24000 -ac 2 -ab 128 -vol 448 -f wav {2} \"{1}\"";
                    //-ss = start time
                    //-t = duration
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
                    var parameters = string.Format(fFmpegWaveTranscodeSettings, _videoFileName, targetFile, audioParameter);
                    var process = new Process { StartInfo = new ProcessStartInfo(exeFilePath, parameters) { WindowStyle = ProcessWindowStyle.Hidden, CreateNoWindow = true } };
                    process.Start();
                    while (!process.HasExited && !_abort)
                    {
                        Application.DoEvents();
                    }

                    UpdateStatus(LanguageSettings.Current.AddWaveformBatch.Calculating);

                    AudioClips.Add(new AudioClip
                    {
                        Paragraph = item,
                        AudioFileName = targetFile,
                    });

                    UpdateStatus(LanguageSettings.Current.AddWaveformBatch.Done);
                }
                catch
                {
                    UpdateStatus(LanguageSettings.Current.AddWaveformBatch.Error);
                }
                index++;
            }
            labelProgress.Text = string.Empty;
            DialogResult = DialogResult.OK;
        }

        private void UpdateStatus(string status)
        {
            labelProgress.Text = status;
            Refresh();
        }
    }
}
