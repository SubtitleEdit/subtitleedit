using Nikse.SubtitleEdit.Core;
using Nikse.SubtitleEdit.Core.BluRaySup;
using Nikse.SubtitleEdit.Core.CDG;
using Nikse.SubtitleEdit.Core.Interfaces;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Logic;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
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

        public string FileName { get; }

        public ImportCdg(string fileName)
        {
            UiUtil.PreInitialize(this);
            InitializeComponent();
            UiUtil.FixFonts(this);
            _cdgGraphics = CdgGraphicsFile.Load(fileName);
            _subtitle = new Subtitle();
            FileName = fileName;
            labelStatus.Text = string.Empty;
            labelFileName.Text = string.Format("File name: {0}", Path.GetFileName(fileName));
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
                    labelAudioFileName.Text = Path.GetFileName(audioFileName);
                }
            }

            textBoxFFmpegPath.Text = Configuration.Settings.General.MkvMergeLocation;

            comboBoxRes.SelectedIndex = 0;

            for (int i = 0; i <= 1000; i++)
            {
                comboBoxLeftRightMargin.Items.Add(i);
                comboBoxBottomMargin.Items.Add(i);
            }

            comboBoxLeftRightMargin.SelectedIndex = Configuration.Settings.Tools.ExportCdgMarginLeft;
            comboBoxBottomMargin.SelectedIndex = Configuration.Settings.Tools.ExportCdgMarginBottom;

            if (!string.IsNullOrEmpty(Configuration.Settings.Tools.ExportCdgBackgroundImage) &&
                File.Exists(Configuration.Settings.Tools.ExportCdgBackgroundImage))
            {
                SetBackgroundImage(Configuration.Settings.Tools.ExportCdgBackgroundImage);
            }

            radioButtonBluRaySup_CheckedChanged(null, null);
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void buttonStart_Click(object sender, EventArgs e)
        {
            if (radioButtonVideo.Checked)
            {
                if (string.IsNullOrEmpty(_audioFileName) || !File.Exists(_audioFileName))
                {
                    MessageBox.Show("No audio file!");
                    return;
                }

                if (_originalBackgroundImage == null)
                {
                    MessageBox.Show("No background image file!");
                    return;
                }

                if (string.IsNullOrEmpty(textBoxFFmpegPath.Text) || !File.Exists(textBoxFFmpegPath.Text))
                {
                    MessageBox.Show("mkvmerge.exe not found!");
                    return;
                }

                if (string.IsNullOrEmpty(Configuration.Settings.General.FFmpegLocation) || !File.Exists(Configuration.Settings.General.FFmpegLocation))
                {
                    MessageBox.Show("ffmpeg not configured!");
                    return;
                }
            }

            radioButtonBluRaySup.Enabled = false;
            radioButtonVideo.Enabled = false;
            buttonStart.Enabled = false;

            var cdgToImageList = new CdgToImageList();
            int total = (int)Math.Round(_cdgGraphics.DurationInMilliseconds / CdgGraphics.TimeMsFactor);

            var bw = new BackgroundWorker { WorkerReportsProgress = true };
            bw.DoWork += (o, args) => { _imageList = cdgToImageList.MakeImageList(_cdgGraphics, _subtitle, (number, unique) => { bw.ReportProgress(number, unique); }); };
            bw.RunWorkerCompleted += (o, args) =>
            {
                if (radioButtonBluRaySup.Checked)
                {
                    using (var exportBdnXmlPng = new ExportPngXml())
                    {
                        exportBdnXmlPng.InitializeFromVobSubOcr(_subtitle, new SubRip(), ExportPngXml.ExportFormats.BluraySup, FileName, this, "Test123");
                        exportBdnXmlPng.ShowDialog(this);
                    }
                    DialogResult = DialogResult.OK;
                }
                else
                {
                    var tempFolder = Path.GetTempPath();
                    var supFileName = Path.Combine(tempFolder, Guid.NewGuid() + ".sup");
                    using (var binarySubtitleFile = new FileStream(supFileName, FileMode.Create))
                    {
                        labelStatus.Text = "Generating Blu-ray sup file...";
                        labelStatus.Refresh();

                        var bottomMargin = comboBoxBottomMargin.SelectedIndex;
                        var leftMargin = comboBoxLeftRightMargin.SelectedIndex;
                        for (var index = 0; index < _subtitle.Paragraphs.Count; index++)
                        {
                            var p = _subtitle.Paragraphs[index];
                            var image = _imageList[index].GetBitmap();
                            var brSub = new BluRaySupPicture
                            {
                                StartTime = (long)p.StartTime.TotalMilliseconds,
                                EndTime = (long)p.EndTime.TotalMilliseconds,
                                Width = 620,
                                Height = 350,
                                CompositionNumber = p.Number * 2
                            };

                            var buffer = BluRaySupPicture.CreateSupFrame(brSub, image, 25, bottomMargin, leftMargin, ContentAlignment.BottomLeft);
                            binarySubtitleFile.Write(buffer, 0, buffer.Length);
                        }
                    }

                    if (!string.IsNullOrEmpty(supFileName) && File.Exists(supFileName))
                    {
                        saveFileDialog1.Filter = "Matroska (*.mkv)|*.mkv";
                        saveFileDialog1.FileName = FileName.Substring(0, FileName.Length - 3) + "mkv";
                        if (saveFileDialog1.ShowDialog(this) != DialogResult.OK)
                        {
                            return;
                        }

                        var finalMkv = saveFileDialog1.FileName;

                        labelStatus.Text = "Generating video...";
                        labelStatus.Refresh();
                        var tempImageFileName = Path.Combine(tempFolder, Guid.NewGuid() + ".png");
                        _resizedBackgroundImage.Save(tempImageFileName, ImageFormat.Png);
                        var tempMkv = Path.Combine(tempFolder, Guid.NewGuid() + ".mkv");
                        var processMakeVideo = GetFFmpegProcess(tempImageFileName, _audioFileName, tempMkv);
                        processMakeVideo.Start();
                        processMakeVideo.WaitForExit();

                        labelStatus.Text = "Adding subtitles to video...";
                        labelStatus.Refresh();
                        var processAddSubtitles = GetMkvMergeProcess(tempMkv, supFileName, finalMkv);
                        processAddSubtitles.Start();
                        processAddSubtitles.WaitForExit();
                        labelStatus.Text = string.Empty;

                        UiUtil.OpenFolderFromFileName(finalMkv);
                    }

                    DialogResult = DialogResult.OK;
                }
            };
            bw.ProgressChanged += (o, args) =>
            {
                labelStatus.Text = $"Frame {args.ProgressPercentage:#,###,##0} of {total:#,###,##0}, unique images {(int)args.UserState:#,###,##0}";
                labelStatus.Refresh();
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

            SetBackgroundImage(openFileDialog1.FileName);
        }

        private void SetBackgroundImage(string fileName)
        {
            _originalBackgroundImage = new Bitmap(fileName);
            SetBackgroundImage(_originalBackgroundImage);
            labelBackgroundImage.Text = fileName;
            Configuration.Settings.Tools.ExportCdgBackgroundImage = fileName;
        }

        private void SetBackgroundImage(Bitmap originalBackgroundImage)
        {
            _resizedBackgroundImage = ExportPngXml.ResizeBitmap(originalBackgroundImage, 620, 350);
            var bmp = new Bitmap(_resizedBackgroundImage);
            using (var g = Graphics.FromImage(bmp))
            {
                g.DrawRectangle(Pens.Green, Convert.ToInt32(comboBoxLeftRightMargin.SelectedIndex), 350 - CdgGraphics.FullHeight - Convert.ToInt32(comboBoxBottomMargin.SelectedIndex), CdgGraphics.FullWidth, CdgGraphics.FullHeight);
            }

            var oldBitmap = pictureBoxBackgroundImage.Image as Bitmap;
            pictureBoxBackgroundImage.Image = bmp;
            oldBitmap?.Dispose();
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
                    Arguments = $"-loop 1 -i \"{imageFileName}\" -i \"{audioFileName}\" -c:v libx264 -tune stillimage -shortest -s 620x350 \"{outputFileName}\"",
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };
            return process;
        }

        public Process GetMkvMergeProcess(string videoFileName, string subtitleFileName, string outputFileName)
        {
            var location = Configuration.Settings.General.MkvMergeLocation;
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
                    UseShellExecute = false,
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

        private void buttonAudioFileBrowse_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }

            _audioFileName = openFileDialog1.FileName;
            labelAudioFileName.Text = Path.GetFileName(_audioFileName);
        }

        private void comboBoxBottomMargin_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_originalBackgroundImage == null)
            {
                return;
            }

            SetBackgroundImage(_originalBackgroundImage);
            Configuration.Settings.Tools.ExportCdgMarginBottom = comboBoxBottomMargin.SelectedIndex;
        }

        private void comboBoxLeftRightMargin_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_originalBackgroundImage == null)
            {
                return;
            }

            SetBackgroundImage(_originalBackgroundImage);
            Configuration.Settings.Tools.ExportCdgMarginLeft = comboBoxLeftRightMargin.SelectedIndex;
        }

        private void buttonBrowseToFFmpeg_Click(object sender, EventArgs e)
        {
            openFileDialog1.FileName = string.Empty;
            openFileDialog1.Filter = "MKVToolNix mkvmerge (mkvmerge.exe)|mkvmerge.exe";
            if (openFileDialog1.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }
            textBoxFFmpegPath.Text = openFileDialog1.FileName;
            Configuration.Settings.General.MkvMergeLocation = openFileDialog1.FileName;
        }

        private void buttonDownloadFfmpeg_Click(object sender, EventArgs e)
        {
            UiUtil.OpenURL("https://mkvtoolnix.download/downloads.html");
        }

        private void radioButtonBluRaySup_CheckedChanged(object sender, EventArgs e)
        {
            groupBoxVideoExportSettings.Enabled = !radioButtonBluRaySup.Checked;
            groupBoxMkvMerge.Enabled = !radioButtonBluRaySup.Checked;
        }
    }
}