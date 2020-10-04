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
using System.Globalization;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.Forms
{
    public partial class ImportCdg : Form, IBinaryParagraphList
    {
        private readonly CdgGraphics _cdgGraphics;
        private readonly Subtitle _subtitle;
        private List<NikseBitmap> _imageList;

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
            labelDuration.Text = string.Format("Duration: {0}", TimeCode.FromSeconds(_cdgGraphics.DurationInMilliseconds / 1000.0).ToDisplayString());
            buttonCancel.Text = Configuration.Settings.Language.General.Ok;
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
                        var audioFileName = @"C:\Users\WinX\Desktop\auto-br\CDG\samples\FOR HE'S A JOLLY GOOD FELLOW.mp3";
                        var processMakeVideo = GetFFmpegProcess(labelBackgroundImage.Text,)
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
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
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
                    Arguments = $"-o \"{videoFileName}\" \"{outputFileName}\" \"{subtitleFileName}\"",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                }
            };

            var workingDirectory = Path.GetDirectoryName(location);
            if (!string.IsNullOrEmpty(workingDirectory))
            {
                process.StartInfo.WorkingDirectory = workingDirectory;
            }

            return process;
        }
    }
}