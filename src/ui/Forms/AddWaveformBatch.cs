using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.ContainerFormats.Matroska;
using Nikse.SubtitleEdit.Logic;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
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

            Text = LanguageSettings.Current.AddWaveformBatch.Title;
            buttonRipWave.Text = LanguageSettings.Current.AddWaveform.GenerateWaveformData;
            buttonDone.Text = LanguageSettings.Current.General.Ok;

            var l = LanguageSettings.Current.BatchConvert;
            groupBoxInput.Text = l.Input;
            labelChooseInputFiles.Text = l.InputDescription;
            columnHeaderFName.Text = LanguageSettings.Current.JoinSubtitles.FileName;
            columnHeaderFormat.Text = LanguageSettings.Current.Main.Controls.SubtitleFormat;
            columnHeaderSize.Text = LanguageSettings.Current.General.Size;
            columnHeaderStatus.Text = l.Status;
            buttonSearchFolder.Text = l.ScanFolder;
            checkBoxScanFolderRecursive.Text = l.Recursive;
            checkBoxScanFolderRecursive.Left = buttonSearchFolder.Left - checkBoxScanFolderRecursive.Width - 5;
            checkBoxGenerateSceneChanges.Text = LanguageSettings.Current.ImportSceneChanges.GetSceneChangesWithFfmpeg;
            checkBoxGenerateSceneChanges.Left = groupBoxInput.Left + listViewInputFiles.Width - checkBoxGenerateSceneChanges.Width;
            checkBoxGenerateSceneChanges.Visible = !string.IsNullOrWhiteSpace(Configuration.Settings.General.FFmpegLocation) && File.Exists(Configuration.Settings.General.FFmpegLocation);
            removeToolStripMenuItem.Text = LanguageSettings.Current.MultipleReplace.Remove;
            removeAllToolStripMenuItem.Text = LanguageSettings.Current.MultipleReplace.RemoveAll;
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
            if (_converting || listViewInputFiles.SelectedIndices.Count == 0)
            {
                return;
            }

            var idx = listViewInputFiles.SelectedIndices[0];
            listViewInputFiles.BeginUpdate();
            for (int i = listViewInputFiles.SelectedIndices.Count - 1; i >= 0; i--)
            {
                listViewInputFiles.Items.RemoveAt(listViewInputFiles.SelectedIndices[i]);
            }
            listViewInputFiles.EndUpdate();
            if (idx >= 0 && idx < listViewInputFiles.Items.Count)
            {
                listViewInputFiles.Items[idx].Selected = true;
            }
            else if (listViewInputFiles.Items.Count > 0)
            {
                listViewInputFiles.Items[listViewInputFiles.Items.Count - 1].Selected = true;
            }
        }

        private void buttonInputBrowse_Click(object sender, EventArgs e)
        {
            buttonInputBrowse.Enabled = false;
            openFileDialog1.Title = LanguageSettings.Current.General.OpenVideoFile;
            openFileDialog1.FileName = string.Empty;
            openFileDialog1.Filter = UiUtil.GetVideoFileFilter(true);
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

        private void AddInputFile(string fileName)
        {
            try
            {
                var ext = Path.GetExtension(fileName)?.ToLowerInvariant();
                if (string.IsNullOrEmpty(ext) ||
                    !Utilities.VideoFileExtensions.Contains(ext.ToLowerInvariant()) &&
                    !Utilities.AudioFileExtensions.Contains(ext.ToLowerInvariant()))
                {
                    return;
                }

                foreach (ListViewItem lvi in listViewInputFiles.Items)
                {
                    if (lvi.Text.Equals(fileName, StringComparison.OrdinalIgnoreCase))
                    {
                        return;
                    }
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
            if (!string.IsNullOrEmpty(Configuration.Settings.Tools.WaveformBatchLastFolder) &&
                Directory.Exists(Configuration.Settings.Tools.WaveformBatchLastFolder))
            {
                folderBrowserDialog1.SelectedPath = Configuration.Settings.Tools.WaveformBatchLastFolder;
            }

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
                Configuration.Settings.Tools.WaveformBatchLastFolder = folderBrowserDialog1.SelectedPath;
            }
        }

        private void SearchFolder(string path)
        {
            if (checkBoxScanFolderRecursive.Checked)
            {
                ScanFiles(Directory.EnumerateFiles(path, "*", SearchOption.AllDirectories));
            }
            else
            {
                ScanFiles(Directory.EnumerateFiles(path));
            }
        }

        private void ScanFiles(IEnumerable<string> fileNames)
        {
            foreach (string fileName in fileNames)
            {
                try
                {
                    string ext = Path.GetExtension(fileName);
                    if (ext != null && (Utilities.VideoFileExtensions.Contains(ext.ToLowerInvariant()) || Utilities.AudioFileExtensions.Contains(ext.ToLowerInvariant())))
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
                        {
                            return;
                        }
                    }
                }
                catch
                {
                    // ignored
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
                MessageBox.Show(LanguageSettings.Current.BatchConvert.NothingToConvert);
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
            {
                item.SubItems[3].Text = "-";
            }

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
                updateStatus(LanguageSettings.Current.AddWaveformBatch.ExtractingAudio);
                string fileName = item.Text;
                try
                {
                    string targetFile = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".wav");
                    Process process;
                    try
                    {
                        process = AddWaveform.GetCommandLineProcess(fileName, -1, targetFile, Configuration.Settings.General.VlcWaveTranscodeSettings, out var encoderName);
                        labelInfo.Text = encoderName;
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
                    if (fileName.ToLowerInvariant().EndsWith(".mkv", StringComparison.OrdinalIgnoreCase))
                    {
                        try
                        {
                            using (var matroska = new MatroskaFile(fileName))
                            {
                                if (matroska.IsValid)
                                {
                                    foreach (var track in matroska.GetTracks())
                                    {
                                        if (track.IsAudio)
                                        {
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
                                        _delayInMilliseconds = (int)matroska.GetAudioTrackDelayMilliseconds(mkvAudioTrackNumbers[0]);
                                    }
                                }
                            }
                        }
                        catch (Exception exception)
                        {
                            SeLogger.Error(exception, $"Error getting delay from mkv: {fileName}");
                            _delayInMilliseconds = 0;
                        }
                    }

                    updateStatus(LanguageSettings.Current.AddWaveformBatch.Calculating);
                    MakeWaveformAndSpectrogram(fileName, targetFile, _delayInMilliseconds);

                    if (checkBoxGenerateSceneChanges.Visible && checkBoxGenerateSceneChanges.Checked)
                    {
                        GenerateSceneChanges(fileName);
                    }

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

                    updateStatus(LanguageSettings.Current.AddWaveformBatch.Done);
                }
                catch
                {
                    IncrementAndShowProgress();

                    updateStatus(LanguageSettings.Current.AddWaveformBatch.Error);
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

        private void GenerateSceneChanges(string videoFileName)
        {
            var sceneChangesGenerator = new SceneChangesGenerator();
            var threshold = 0.4m;
            if (decimal.TryParse(Configuration.Settings.General.FFmpegSceneThreshold, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var d))
            {
                threshold = d;
            }
            using (var process = sceneChangesGenerator.GetProcess(videoFileName, threshold))
            {
                while (!process.HasExited)
                {
                    Application.DoEvents();
                    System.Threading.Thread.Sleep(100);
                    if (_abort)
                    {
                        DialogResult = DialogResult.Cancel;
                        process.Kill();
                        return;
                    }
                }
            }
            var seconds = SceneChangesGenerator.GetSeconds(sceneChangesGenerator.GetTimeCodesString().SplitToLines().ToArray());
            if (seconds.Count > 0)
            {
                SceneChangeHelper.SaveSceneChanges(videoFileName, seconds);
            }
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
            {
                progressBar1.Value++;
            }

            TaskbarList.SetProgressValue(Owner.Handle, progressBar1.Value, progressBar1.Maximum);
            labelProgress.Text = progressBar1.Value + " / " + progressBar1.Maximum;
        }

        private void AddWaveformBatch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                _abort = true;
            }
        }

        private void AddWaveformBatch_ResizeEnd(object sender, EventArgs e)
        {
            listViewInputFiles.AutoSizeLastColumn();
        }

        private void AddWaveformBatch_Shown(object sender, EventArgs e)
        {
            AddWaveformBatch_ResizeEnd(sender, e);
        }

        private void contextMenuStripFiles_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (listViewInputFiles.Items.Count == 0)
            {
                e.Cancel = true;
            }

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
            {
                e.Effect = DragDropEffects.All;
            }
        }

        private void listViewInputFiles_DragDrop(object sender, DragEventArgs e)
        {
            if (_converting)
            {
                return;
            }

            var fileNames = (string[])e.Data.GetData(DataFormats.FileDrop);
            listViewInputFiles.BeginUpdate();
            foreach (string fileName in fileNames)
            {
                if (File.Exists(fileName))
                {
                    AddInputFile(fileName);
                }
                else if (Directory.Exists(fileName))
                {
                    SearchFolder(fileName);
                }
            }
            listViewInputFiles.EndUpdate();
        }

        private void ListViewInputFiles_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {
                RemoveSelectedFiles();
            }
        }
    }
}