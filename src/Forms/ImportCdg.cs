using Nikse.SubtitleEdit.Core;
using Nikse.SubtitleEdit.Core.CDG;
using Nikse.SubtitleEdit.Core.Interfaces;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Logic;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.Forms
{
    public partial class ImportCdg : Form, IBinaryParagraphList
    {        
        //300x216
        private readonly CdgGraphics _cdgGraphics;
        private readonly Subtitle _subtitle;
        private List<NikseBitmap> _imageList;
        private string _audioFileName;
        private Bitmap _originalBackgroundImage;
        private Bitmap _resizedBackgroundImage;
        private int _durationSeconds;

        public string FileName { get; }

        public ImportCdg(string fileName)
        {
            UiUtil.PreInitialize(this);
            InitializeComponent();
            UiUtil.FixFonts(this);
            _cdgGraphics = CdgGraphicsFile.Load(fileName);
            _subtitle = new Subtitle();
            FileName = fileName;
            labelProgress.Text = string.Empty;
            labelProgress2.Text = string.Empty;
            labelFileName.Text = string.Format("File name: {0}", Path.GetFileName(fileName));
            _durationSeconds = (int) (_cdgGraphics.DurationInMilliseconds / 1000.0 + 0.5);
            labelDuration.Text = string.Format("Duration: {0}", TimeCode.FromSeconds(_cdgGraphics.DurationInMilliseconds / 1000.0).ToDisplayString());
            buttonCancel.Text = Configuration.Settings.Language.General.Ok;

            if (fileName != null && fileName.Length > 3)
            {
                var audioFileName = fileName.Substring(0, fileName.Length - 3) + "mp3";
                if (!File.Exists(audioFileName))
                {
                    audioFileName = fileName.Substring(0, fileName.Length - 3) + "ogg";
                }
                if (File.Exists(audioFileName))
                {
                    _audioFileName = audioFileName;
                    labelAudioFileName.Text = "Audio file name: " + Path.GetFileName(audioFileName);
                }
            }

            if (!string.IsNullOrEmpty(Configuration.Settings.Tools.ExportCdgBackgroundImage) &&
                File.Exists(Configuration.Settings.Tools.ExportCdgBackgroundImage))
            {
                pictureBoxBackgroundImage.Image = new Bitmap(Configuration.Settings.Tools.ExportCdgBackgroundImage);
                labelBackgroundImage.Text = Configuration.Settings.Tools.ExportCdgBackgroundImage;
            }
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void buttonStart_Click(object sender, EventArgs e)
        {
            radioButtonBluRaySup.Enabled = false;
            buttonStart.Enabled = false;

            var cdgToImageList = new CdgToImageList();
            int total = (int)Math.Round(_cdgGraphics.DurationInMilliseconds / CdgGraphics.TimeMsFactor);

            var bw = new BackgroundWorker { WorkerReportsProgress = true };
            bw.DoWork += (o, args) => { _imageList = cdgToImageList.MakeImageList(_cdgGraphics, _subtitle, (number, unique) => { bw.ReportProgress(number, unique); }); };
            bw.RunWorkerCompleted += (o, args) =>
            {
                using (var exportBdnXmlPng = new ExportPngXml())
                {
                    var old = Configuration.Settings.Tools.ExportBluRayRemoveSmallGaps;
                    Configuration.Settings.Tools.ExportBluRayRemoveSmallGaps = false; // Hm, not really sure if a 'true' is needed here - seems to give a small blink once in a while!?
                    exportBdnXmlPng.InitializeFromVobSubOcr(_subtitle, new SubRip(), ExportPngXml.ExportFormats.BluraySup, FileName, this, "Test123");
                    exportBdnXmlPng.ShowDialog(this);
                    Configuration.Settings.Tools.ExportBluRayRemoveSmallGaps = old;
                    var supFileName = exportBdnXmlPng.GetOutputFileName();
                    if (!string.IsNullOrEmpty(supFileName) && File.Exists(supFileName))
                    {

                        var tempFolder = Path.GetTempPath();
                        var tempMkv = Path.Combine(tempFolder, Guid.NewGuid() + ".mkv");
                        var processMakeVideo = GetFFmpegProcess(labelBackgroundImage.Text, _audioFileName, tempMkv);
                        processMakeVideo.Start();
                        processMakeVideo.WaitForExit();

                        var finalMkv = Path.Combine(Path.GetDirectoryName(FileName), Guid.NewGuid() + ".mkv");
                        var processAddSubtitles = GetMkvMergeProcess(tempMkv, supFileName, finalMkv);
                        processAddSubtitles.Start();
                        processAddSubtitles.WaitForExit();
                    }

                    DialogResult = DialogResult.OK;
                }
            };
            bw.ProgressChanged += (o, args) =>
            {
                labelProgress.Text = $"Frame {args.ProgressPercentage:#,###,##0} of {total:#,###,##0}";
                labelProgress.Refresh();
                labelProgress2.Text = $"Unique images {(int)args.UserState:#,###,##0}";
                labelProgress2.Refresh();
            };
            bw.RunWorkerAsync();
        }

        public Bitmap GetSubtitleBitmap(int index, bool crop = true)
        {
            return _imageList[index].GetBitmap();
        }

        public bool GetIsForced(int index)
        {
            return false;
        }

        private void buttonChooseBackgroundImage_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }

            pictureBoxBackgroundImage.Image = new Bitmap(openFileDialog1.FileName);
            labelBackgroundImage.Text = openFileDialog1.FileName;
            Configuration.Settings.Tools.ExportCdgBackgroundImage = openFileDialog1.FileName;
        }

        public Process GetFFmpegProcess(string imageFileName, string audioFileName, string outputFileName)
        {
            var ffmpegLocation = Configuration.Settings.General.FFmpegLocation;
            if (!Configuration.IsRunningOnWindows && (string.IsNullOrEmpty(ffmpegLocation) || !File.Exists(ffmpegLocation)))
            {
                ffmpegLocation = "ffmpeg";
            }

            var process = new Process
            {
                StartInfo =
                {
                    FileName = ffmpegLocation,
                    Arguments = $"-loop 1 -i \"{imageFileName}\" -i \"{audioFileName}\" -c:v libx264 -tune stillimage -shortest \"{outputFileName}\"",
                    //UseShellExecute = false,
                    //RedirectStandardOutput = true,
                    //RedirectStandardError = true,
                    //CreateNoWindow = true
                }
            };
            return process;
        }

        public Process GetMkvMergeProcess(string videoFileName, string subtitleFileName, string outputFileName)
        {
            var location = @"j:\data\Tools\MKVToolNix\mkvmerge.exe";
            if (!Configuration.IsRunningOnWindows && (string.IsNullOrEmpty(location) || !File.Exists(location)))
            {
                location = "mkvmerge";
            }

            var process = new Process
            {
                StartInfo =
                {
                    FileName = location,
                    Arguments = $"-o \"{outputFileName}\" \"{videoFileName}\" \"{subtitleFileName}\"",
                    //UseShellExecute = false,
                    //RedirectStandardOutput = true,
                    //RedirectStandardError = true,
                    //CreateNoWindow = true
                }
            };

            var workingDirectory = Path.GetDirectoryName(location);
            if (!string.IsNullOrEmpty(workingDirectory))
            {
                process.StartInfo.WorkingDirectory = workingDirectory;
            }

            return process;
        }

        private void buttonAudioFileBrowse_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }

            _audioFileName = openFileDialog1.FileName;
            labelAudioFileName.Text = "Audio file name: " + Path.GetFileName(_audioFileName);
        }
    }
}