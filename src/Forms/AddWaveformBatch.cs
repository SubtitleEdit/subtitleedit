using Nikse.SubtitleEdit.Core;
using Nikse.SubtitleEdit.Core.ContainerFormats.Matroska;
using Nikse.SubtitleEdit.Logic;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.Forms
{
    public sealed partial class AddWaveformBatch : PositionAndSizeForm
    {

        private int _delayInMilliseconds;
        private bool _converting;
        private bool _abort;

        public AddWaveformBatch()
        {
            UiUtil.PreInitialize(this);
            InitializeComponent();
            UiUtil.FixFonts(this);
            labelProgress.Text = string.Empty;
            labelInfo.Text = string.Empty;
            progressBar1.Visible = false;

            Text = Configuration.Settings.Language.AddWaveformBatch.Title;
            buttonRipWave.Text = Configuration.Settings.Language.AddWaveform.GenerateWaveformData;
            buttonDone.Text = Configuration.Settings.Language.General.Ok;

            var l = Configuration.Settings.Language.BatchConvert;
            groupBoxInput.Text = l.Input;
            labelChooseInputFiles.Text = l.InputDescription;
            columnHeaderFName.Text = Configuration.Settings.Language.JoinSubtitles.FileName;
            columnHeaderFormat.Text = Configuration.Settings.Language.Main.Controls.SubtitleFormat;
            columnHeaderSize.Text = Configuration.Settings.Language.General.Size;
            columnHeaderStatus.Text = l.Status;
            buttonSearchFolder.Text = l.ScanFolder;
            checkBoxScanFolderRecursive.Text = l.Recursive;
            checkBoxScanFolderRecursive.Left = buttonSearchFolder.Left - checkBoxScanFolderRecursive.Width - 5;
            UiUtil.FixLargeFonts(this, buttonDone);
        }

        private void removeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            RemoveSelectedFiles();
        }

        private void removeAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            listViewInputFiles.Items.Clear();
        }

        private void RemoveSelectedFiles()
        {
            if (_converting)
                return;

            for (int i = listViewInputFiles.SelectedIndices.Count - 1; i >= 0; i--)
            {
                listViewInputFiles.Items.RemoveAt(listViewInputFiles.SelectedIndices[i]);
            }
        }

        private void buttonInputBrowse_Click(object sender, EventArgs e)
        {
            buttonInputBrowse.Enabled = false;
            openFileDialog1.Title = Configuration.Settings.Language.General.OpenVideoFile;
            openFileDialog1.FileName = string.Empty;
            openFileDialog1.Filter = Utilities.GetVideoFileFilter(true);
            openFileDialog1.Multiselect = true;
            if (openFileDialog1.ShowDialog(this) == DialogResult.OK)
            {
                foreach (string fileName in openFileDialog1.FileNames)
                {
                    AddInputFile(fileName);
                }
            }
            buttonInputBrowse.Enabled = true;
        }

        private static readonly ICollection<string> ExcludedExtensions = new List<string> { ".srt", ".txt", ".exe", ".ass", ".sub", ".jpg", ".png", ".zip", ".rar" };
        private void AddInputFile(string fileName)
        {
            try
            {
                var ext = Path.GetExtension(fileName).ToLowerInvariant();
                if (string.IsNullOrEmpty(ext) || ExcludedExtensions.Contains(ext))
                {
                    return;
                }

                foreach (ListViewItem lvi in listViewInputFiles.Items)
                {
                    if (lvi.Text.Equals(fileName, StringComparison.OrdinalIgnoreCase))
                        return;
                }

                var fi = new FileInfo(fileName);
                var item = new ListViewItem(fileName);
                item.SubItems.Add(Utilities.FormatBytesToDisplayFileSize(fi.Length));

                item.SubItems.Add(Path.GetExtension(fileName));
                item.SubItems.Add("-");

                listViewInputFiles.Items.Add(item);
            }
            catch
            {
                // Ignore errors
            }
        }

        private void buttonSearchFolder_Click(object sender, EventArgs e)
        {
            folderBrowserDialog1.ShowNewFolderButton = false;
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                listViewInputFiles.BeginUpdate();
                buttonRipWave.Enabled = false;
                progressBar1.Style = ProgressBarStyle.Marquee;
                progressBar1.Visible = true;
                buttonInputBrowse.Enabled = false;
                buttonSearchFolder.Enabled = false;
                _abort = false;

                SearchFolder(folderBrowserDialog1.SelectedPath);

                buttonRipWave.Enabled = true;
                progressBar1.Style = ProgressBarStyle.Continuous;
                progressBar1.Visible = true;
                buttonInputBrowse.Enabled = true;
                buttonSearchFolder.Enabled = true;
                listViewInputFiles.EndUpdate();
            }
        }

        private void SearchFolder(string path)
        {
            foreach (string fileName in Directory.GetFiles(path))
            {
                try
                {
                    string ext = Path.GetExtension(fileName).ToLower();
                    if (Utilities.VideoFileExtensions.Contains(ext))
                    {
                        var fi = new FileInfo(fileName);
                        if (ext == ".mkv" && FileUtil.IsVobSub(fileName))
                        {
                            AddFromSearch(fileName, fi, "Matroska");
                        }
                        else
                        {
                            AddFromSearch(fileName, fi, ext.Remove(0, 1));
                        }
                        progressBar1.Refresh();
                        Application.DoEvents();
                        if (_abort)
                            return;
                    }
                }
                catch
                {
                }
            }
            if (checkBoxScanFolderRecursive.Checked)
            {
                foreach (string directory in Directory.GetDirectories(path))
                {
                    if (directory != "." && directory != "..")
                        SearchFolder(directory);
                    if (_abort)
                        return;
                }
            }
        }

        private void AddFromSearch(string fileName, FileInfo fi, string nameOfFormat)
        {
            var item = new ListViewItem(fileName);
            item.SubItems.Add(Utilities.FormatBytesToDisplayFileSize(fi.Length));
            item.SubItems.Add(nameOfFormat);
            item.SubItems.Add("-");
            listViewInputFiles.Items.Add(item);
        }

        private void buttonDoneClick(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void buttonRipWave_Click(object sender, EventArgs e)
        {
            if (listViewInputFiles.Items.Count == 0)
            {
                MessageBox.Show(Configuration.Settings.Language.BatchConvert.NothingToConvert);
                return;
            }
            _converting = true;
            buttonRipWave.Enabled = false;
            progressBar1.Style = ProgressBarStyle.Blocks;
            progressBar1.Maximum = listViewInputFiles.Items.Count;
            progressBar1.Value = 0;
            progressBar1.Visible = progressBar1.Maximum > 2;
            buttonInputBrowse.Enabled = false;
            buttonSearchFolder.Enabled = false;
            _abort = false;
            listViewInputFiles.BeginUpdate();
            foreach (ListViewItem item in listViewInputFiles.Items)
                item.SubItems[3].Text = "-";
            listViewInputFiles.EndUpdate();
            Refresh();
            int index = 0;
            while (index < listViewInputFiles.Items.Count && _abort == false)
            {
                var item = listViewInputFiles.Items[index];
                Action<string> updateStatus = status =>
                {
                    item.SubItems[3].Text = status;
                    Refresh();
                };
                updateStatus(Configuration.Settings.Language.AddWaveformBatch.ExtractingAudio);
                string fileName = item.Text;
                try
                {
                    string targetFile = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".wav");
                    Process process;
                    try
                    {
                        string encoderName;
                        process = AddWaveform.GetCommandLineProcess(fileName, -1, targetFile, Configuration.Settings.General.VlcWaveTranscodeSettings, out encoderName);
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
                    while (!process.HasExited && !_abort)
                    {
                        Application.DoEvents();
                    }

                    // check for delay in matroska files
                    var audioTrackNames = new List<string>();
                    var mkvAudioTrackNumbers = new Dictionary<int, int>();
                    if (fileName.ToLower().EndsWith(".mkv", StringComparison.OrdinalIgnoreCase))
                    {
                        MatroskaFile matroska = null;
                        try
                        {
                            matroska = new MatroskaFile(fileName);

                            if (matroska.IsValid)
                            {
                                foreach (var track in matroska.GetTracks())
                                {
                                    if (track.IsAudio)
                                    {
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
                            if (matroska != null)
                            {
                                matroska.Dispose();
                            }
                        }
                    }

                    updateStatus(Configuration.Settings.Language.AddWaveformBatch.Calculating);
                    MakeWaveformAndSpectrogram(fileName, targetFile, _delayInMilliseconds);

                    // cleanup
                    try
                    {
                        File.Delete(targetFile);
                    }
                    catch
                    {
                        // don't show error about unsuccessful delete
                    }

                    IncrementAndShowProgress();

                    updateStatus(Configuration.Settings.Language.AddWaveformBatch.Done);
                }
                catch
                {
                    IncrementAndShowProgress();

                    updateStatus(Configuration.Settings.Language.AddWaveformBatch.Error);
                }
                index++;
            }
            _converting = false;
            labelProgress.Text = string.Empty;
            labelInfo.Text = string.Empty;
            progressBar1.Visible = false;
            TaskbarList.SetProgressState(Owner.Handle, TaskbarButtonProgressFlags.NoProgress);
            buttonRipWave.Enabled = true;
            buttonInputBrowse.Enabled = true;
            buttonSearchFolder.Enabled = true;
        }

        private void MakeWaveformAndSpectrogram(string videoFileName, string targetFile, int delayInMilliseconds)
        {
            using (var waveFile = new WavePeakGenerator(targetFile))
            {
                waveFile.GeneratePeaks(delayInMilliseconds, WavePeakGenerator.GetPeakWaveFileName(videoFileName));

                if (Configuration.Settings.VideoControls.GenerateSpectrogram)
                {
                    waveFile.GenerateSpectrogram(delayInMilliseconds, WavePeakGenerator.SpectrogramDrawer.GetSpectrogramFolder(videoFileName));
                }
            }
        }

        private void IncrementAndShowProgress()
        {
            if (progressBar1.Value < progressBar1.Maximum)
                progressBar1.Value++;
            TaskbarList.SetProgressValue(Owner.Handle, progressBar1.Value, progressBar1.Maximum);
            labelProgress.Text = progressBar1.Value + " / " + progressBar1.Maximum;
        }

        private void AddWaveformBatch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
                _abort = true;
        }

        private void contextMenuStripFiles_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (listViewInputFiles.Items.Count == 0)
                e.Cancel = true;
            removeToolStripMenuItem.Visible = listViewInputFiles.SelectedItems.Count > 0;
        }

        private void listViewInputFiles_DragEnter(object sender, DragEventArgs e)
        {
            if (_converting)
            {
                e.Effect = DragDropEffects.None;
                return;
            }

            if (e.Data.GetDataPresent(DataFormats.FileDrop, false))
                e.Effect = DragDropEffects.All;
        }

        private void listViewInputFiles_DragDrop(object sender, DragEventArgs e)
        {
            if (_converting)
            {
                return;
            }

            var fileNames = (string[])e.Data.GetData(DataFormats.FileDrop);
            foreach (string fileName in fileNames)
            {
                AddInputFile(fileName);
            }
        }

    }
}
