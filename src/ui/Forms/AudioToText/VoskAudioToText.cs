using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Nikse.SubtitleEdit.Controls;
using Nikse.SubtitleEdit.Core.AudioToText;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Logic;
using Vosk;
using MessageBox = Nikse.SubtitleEdit.Forms.SeMsgBox.MessageBox;

namespace Nikse.SubtitleEdit.Forms.AudioToText
{
    public sealed partial class VoskAudioToText : Form
    {
        private readonly string _videoFileName;
        private readonly int _audioTrackNumber;
        private readonly string _voskFolder;
        private bool _cancel;
        private bool _batchMode;
        private int _batchFileNumber;
        private long _startTicks;
        private long _bytesWavTotal;
        private long _bytesWavRead;
        private readonly List<string> _filesToDelete;
        private readonly Form _parentForm;
        private bool _useCenterChannelOnly;
        private Model _model;
        private int _initialWidth = 725;
        private readonly List<string> _outputBatchFileNames = new List<string>();

        public Subtitle TranscribedSubtitle { get; private set; }

        public VoskAudioToText(string videoFileName, int audioTrackNumber, Form parentForm)
        {
            UiUtil.PreInitialize(this);
            InitializeComponent();
            UiUtil.FixFonts(this);
            UiUtil.FixLargeFonts(this, buttonGenerate);
            _videoFileName = videoFileName;
            _audioTrackNumber = audioTrackNumber;
            _parentForm = parentForm;

            Text = LanguageSettings.Current.AudioToText.Title;
            labelInfo.Text = LanguageSettings.Current.AudioToText.Info;
            groupBoxModels.Text = LanguageSettings.Current.AudioToText.Models;
            labelModel.Text = LanguageSettings.Current.AudioToText.ChooseModel;
            linkLabelOpenModelsFolder.Text = LanguageSettings.Current.AudioToText.OpenModelsFolder;
            checkBoxUsePostProcessing.Text = LanguageSettings.Current.AudioToText.UsePostProcessing;
            buttonGenerate.Text = LanguageSettings.Current.Watermark.Generate;
            buttonCancel.Text = LanguageSettings.Current.General.Cancel;
            buttonBatchMode.Text = LanguageSettings.Current.AudioToText.BatchMode;
            groupBoxInputFiles.Text = LanguageSettings.Current.BatchConvert.Input;
            linkLabelVoskWebSite.Text = LanguageSettings.Current.AudioToText.VoskWebsite;

            buttonAddFile.Text = LanguageSettings.Current.DvdSubRip.Add;
            buttonRemoveFile.Text = LanguageSettings.Current.DvdSubRip.Remove;
            buttonClear.Text = LanguageSettings.Current.DvdSubRip.Clear;

            columnHeaderFileName.Text = LanguageSettings.Current.JoinSubtitles.FileName;

            checkBoxUsePostProcessing.Checked = Configuration.Settings.Tools.VoskPostProcessing;
            _voskFolder = VoskModel.ModelFolder;
            FillModels(comboBoxModels, string.Empty);

            textBoxLog.Visible = false;
            textBoxLog.Dock = DockStyle.Fill;
            labelProgress.Text = string.Empty;
            labelTime.Text = string.Empty;
            _filesToDelete = new List<string>();

            if (string.IsNullOrEmpty(videoFileName))
            {
                _batchMode = true;
                buttonBatchMode.Enabled = false;
            }
            else
            {
                listViewInputFiles.Items.Add(videoFileName);
            }

            listViewInputFiles.Visible = false;
            labelFC.Text = string.Empty;
        }

        public static void FillModels(NikseComboBox comboBoxModels, string lastDownloadedModel)
        {
            var voskFolder = Path.Combine(Configuration.DataDirectory, "Vosk");
            var selectName = string.IsNullOrEmpty(lastDownloadedModel) ? Configuration.Settings.Tools.VoskModel : lastDownloadedModel;
            comboBoxModels.Items.Clear();
            foreach (var directory in Directory.GetDirectories(voskFolder))
            {
                var name = Path.GetFileName(directory);
                if (!File.Exists(Path.Combine(directory, "final.mdl")) && !File.Exists(Path.Combine(directory, "am", "final.mdl")))
                {
                    continue;
                }

                comboBoxModels.Items.Add(name);
                if (name == selectName)
                {
                    comboBoxModels.SelectedIndex = comboBoxModels.Items.Count - 1;
                }
            }

            if (comboBoxModels.SelectedIndex < 0 && comboBoxModels.Items.Count > 0)
            {
                comboBoxModels.SelectedIndex = 0;
            }
        }

        private void ButtonGenerate_Click(object sender, EventArgs e)
        {
            if (comboBoxModels.Items.Count == 0)
            {
                buttonDownload_Click(null, null);
                return;
            }

            _useCenterChannelOnly = Configuration.Settings.General.FFmpegUseCenterChannelOnly &&
                                    FfmpegMediaInfo.Parse(_videoFileName).HasFrontCenterAudio(_audioTrackNumber);

            if (_batchMode)
            {
                if (listViewInputFiles.Items.Count == 0)
                {
                    buttonAddFile_Click(null, null);
                    return;
                }

                GenerateBatch();
                TaskbarList.SetProgressState(_parentForm.Handle, TaskbarButtonProgressFlags.NoProgress);
                return;
            }

            ShowProgressBar();
            var modelFileName = Path.Combine(_voskFolder, comboBoxModels.Text);
            buttonGenerate.Enabled = false;
            buttonDownload.Enabled = false;
            buttonBatchMode.Enabled = false;
            comboBoxModels.Enabled = false;
            var waveFileName = GenerateWavFile(_videoFileName, _audioTrackNumber);
            textBoxLog.AppendText(Environment.NewLine);
            progressBar1.Style = ProgressBarStyle.Blocks;
            var transcript = TranscribeViaVosk(waveFileName, modelFileName);
            if (_cancel && (transcript == null || transcript.Count == 0 || MessageBox.Show(LanguageSettings.Current.AudioToText.KeepPartialTranscription, Text, MessageBoxButtons.YesNoCancel) != DialogResult.Yes))
            {
                DialogResult = DialogResult.Cancel;
                return;
            }

            var postProcessor = new AudioToTextPostProcessor(GetLanguage(comboBoxModels.Text))
            {
                ParagraphMaxChars = Configuration.Settings.General.SubtitleLineMaximumLength * 2,
            };
            TranscribedSubtitle = postProcessor.Fix(AudioToTextPostProcessor.Engine.Vosk, transcript, checkBoxUsePostProcessing.Checked, true, true, true, true, false);
            DialogResult = DialogResult.OK;
        }

        private void ShowProgressBar()
        {
            progressBar1.Maximum = 100;
            progressBar1.Value = 0;
            progressBar1.Visible = true;
            progressBar1.Refresh();
            progressBar1.Top = labelProgress.Bottom + 3;
            if (!textBoxLog.Visible)
            {
                progressBar1.BringToFront();
            }
        }

        private void GenerateBatch()
        {
            groupBoxInputFiles.Enabled = false;
            _batchFileNumber = 0;
            var errors = new StringBuilder();
            var errorCount = 0;
            textBoxLog.AppendText("Batch mode" + Environment.NewLine);
            foreach (ListViewItem lvi in listViewInputFiles.Items)
            {
                _batchFileNumber++;
                var videoFileName = lvi.Text;
                listViewInputFiles.SelectedIndices.Clear();
                lvi.Selected = true;
                ShowProgressBar();
                var modelFileName = Path.Combine(_voskFolder, comboBoxModels.Text);
                buttonGenerate.Enabled = false;
                buttonDownload.Enabled = false;
                buttonBatchMode.Enabled = false;
                comboBoxModels.Enabled = false;
                var waveFileName = GenerateWavFile(videoFileName, _audioTrackNumber);
                if (!File.Exists(waveFileName))
                {
                    errors.AppendLine("Unable to extract audio from: " + videoFileName);
                    errorCount++;
                    continue;
                }

                progressBar1.Style = ProgressBarStyle.Blocks;
                var transcript = TranscribeViaVosk(waveFileName, modelFileName);
                if (_cancel)
                {
                    TaskbarList.SetProgressState(_parentForm.Handle, TaskbarButtonProgressFlags.NoProgress);
                    if (!_batchMode)
                    {
                        DialogResult = DialogResult.Cancel;
                    }

                    groupBoxInputFiles.Enabled = true;
                    return;
                }

                var postProcessor = new AudioToTextPostProcessor(GetLanguage(comboBoxModels.Text))
                {
                    ParagraphMaxChars = Configuration.Settings.General.SubtitleLineMaximumLength * 2,
                };
                TranscribedSubtitle = postProcessor.Fix(AudioToTextPostProcessor.Engine.Vosk, transcript, checkBoxUsePostProcessing.Checked, true, true, true, true, false);

                SaveToSourceFolder(videoFileName);
                TaskbarList.SetProgressValue(_parentForm.Handle, _batchFileNumber, listViewInputFiles.Items.Count);
            }

            progressBar1.Visible = false;
            labelTime.Text = string.Empty;

            TaskbarList.StartBlink(_parentForm, 10, 1, 2);

            if (errors.Length > 0)
            {
                MessageBox.Show(this, $"{errorCount} error(s)!{Environment.NewLine}{errors}");
            }

            var fileList = Environment.NewLine + Environment.NewLine + string.Join(Environment.NewLine, _outputBatchFileNames);
            MessageBox.Show(this, string.Format(LanguageSettings.Current.AudioToText.XFilesSavedToVideoSourceFolder, listViewInputFiles.Items.Count - errorCount) + fileList);

            groupBoxInputFiles.Enabled = true;
            buttonGenerate.Enabled = true;
            buttonDownload.Enabled = true;
            buttonBatchMode.Enabled = true;
            DialogResult = DialogResult.Cancel;
        }

        private void SaveToSourceFolder(string videoFileName)
        {
            var format = SubtitleFormat.FromName(Configuration.Settings.General.DefaultSubtitleFormat, new SubRip());
            var text = TranscribedSubtitle.ToText(format);

            var fileName = Path.Combine(Utilities.GetPathAndFileNameWithoutExtension(videoFileName)) + format.Extension;
            if (File.Exists(fileName))
            {
                fileName = $"{Path.Combine(Utilities.GetPathAndFileNameWithoutExtension(videoFileName))}.{Guid.NewGuid().ToString()}.{format.Extension}";
            }

            try
            {
                File.WriteAllText(fileName, text, Encoding.UTF8);
                textBoxLog.AppendText("Subtitle written to : " + fileName + Environment.NewLine);
                _outputBatchFileNames.Add(fileName);
            }
            catch
            {
                var dir = Path.GetDirectoryName(fileName);
                if (!WhisperAudioToText.IsDirectoryWritable(dir))
                {
                    MessageBox.Show($"SE does not have write access to the folder '{dir}'", MessageBoxIcon.Error);
                }

                throw;
            }
        }

        internal static string GetLanguage(string text)
        {
            var languageCodeList = VoskModel.Models.Select(p => p.TwoLetterLanguageCode);
            foreach (var languageCode in languageCodeList)
            {
                if (text.Contains("model-" + languageCode) || text.Contains("model-small-" + languageCode) || text.StartsWith(languageCode, StringComparison.OrdinalIgnoreCase))
                {
                    return languageCode;
                }

                if (languageCode == "jp" && (text.Contains("model-ja") || text.Contains("model-small-ja")))
                {
                    return languageCode;
                }
            }

            return "en";
        }

        public List<ResultText> TranscribeViaVosk(string waveFileName, string modelFileName)
        {
            labelProgress.Text = LanguageSettings.Current.AudioToText.LoadingVoskModel;
            labelProgress.Refresh();
            Application.DoEvents();
            Directory.SetCurrentDirectory(_voskFolder);
            Vosk.Vosk.SetLogLevel(0);
            if (_model == null)
            {
                _model = new Model(modelFileName);
            }

            var rec = new VoskRecognizer(_model, 16000.0f);
            rec.SetMaxAlternatives(0);
            rec.SetWords(true);
            var list = new List<ResultText>();
            labelProgress.Text = LanguageSettings.Current.AudioToText.Transcribing;
            if (_batchMode)
            {
                labelProgress.Text = string.Format(LanguageSettings.Current.AudioToText.TranscribingXOfY, _batchFileNumber, listViewInputFiles.Items.Count);
            }
            else
            {
                TaskbarList.SetProgressValue(_parentForm.Handle, 1, 100);
            }

            labelProgress.Refresh();
            Application.DoEvents();
            var buffer = new byte[4096];
            _bytesWavTotal = new FileInfo(waveFileName).Length;
            _bytesWavRead = 0;
            _startTicks = DateTime.UtcNow.Ticks;
            timer1.Start();
            using (var source = File.OpenRead(waveFileName))
            {
                int bytesRead;
                while ((bytesRead = source.Read(buffer, 0, buffer.Length)) > 0)
                {
                    _bytesWavRead += bytesRead;
                    progressBar1.Value = (int)(_bytesWavRead * 100.0 / _bytesWavTotal);
                    progressBar1.Refresh();
                    Application.DoEvents();
                    if (rec.AcceptWaveform(buffer, bytesRead))
                    {
                        var res = rec.Result();
                        var results = ParseJsonToResult(res);
                        list.AddRange(results);
                    }
                    else
                    {
                        var res = rec.PartialResult();
                        textBoxLog.AppendText(res.RemoveChar('\r', '\n'));
                    }

                    if (!_batchMode)
                    {
                        TaskbarList.SetProgressValue(_parentForm.Handle, Math.Max(1, progressBar1.Value), progressBar1.Maximum);
                    }

                    if (_cancel)
                    {
                        TaskbarList.SetProgressState(_parentForm.Handle, TaskbarButtonProgressFlags.NoProgress);
                        break;
                    }
                }

                if (!_batchMode)
                {
                    TaskbarList.StartBlink(_parentForm, 10, 1, 2);
                }
            }

            var finalResult = rec.FinalResult();
            var finalResults = ParseJsonToResult(finalResult);
            list.AddRange(finalResults);
            timer1.Stop();
            return list;
        }

        public static List<ResultText> ParseJsonToResult(string result)
        {
            var list = new List<ResultText>();
            var jsonParser = new SeJsonParser();
            var root = jsonParser.GetArrayElementsByName(result, "result");
            foreach (var item in root)
            {
                var conf = jsonParser.GetFirstObject(item, "conf");
                var start = jsonParser.GetFirstObject(item, "start");
                var end = jsonParser.GetFirstObject(item, "end");
                var word = jsonParser.GetFirstObject(item, "word");
                if (!string.IsNullOrWhiteSpace(word) &&
                    decimal.TryParse(conf, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var confidence) &&
                    decimal.TryParse(start, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var startSeconds) &&
                    decimal.TryParse(end, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var endSeconds))
                {
                    var rt = new ResultText { Confidence = confidence, Text = word, Start = startSeconds, End = endSeconds };
                    list.Add(rt);
                }
            }

            return list;
        }

        private string GenerateWavFile(string videoFileName, int audioTrackNumber)
        {
            var outWaveFile = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".wav");
            _filesToDelete.Add(outWaveFile);
            var process = GetFfmpegProcess(videoFileName, audioTrackNumber, outWaveFile);
            process.Start();
            ShowProgressBar();
            progressBar1.Style = ProgressBarStyle.Marquee;
            double seconds = 0;
            buttonCancel.Visible = true;
            try
            {
                process.PriorityClass = ProcessPriorityClass.Normal;
            }
            catch
            {
                // ignored
            }

            _cancel = false;
            string targetDriveLetter = null;
            if (Configuration.IsRunningOnWindows)
            {
                var root = Path.GetPathRoot(outWaveFile);
                if (root.Length > 1 && root[1] == ':')
                {
                    targetDriveLetter = root.Remove(1);
                }
            }

            while (!process.HasExited)
            {
                Application.DoEvents();
                System.Threading.Thread.Sleep(100);
                seconds += 0.1;
                if (seconds < 60)
                {
                    labelProgress.Text = string.Format(LanguageSettings.Current.AddWaveform.ExtractingSeconds, seconds);
                }
                else
                {
                    labelProgress.Text = string.Format(LanguageSettings.Current.AddWaveform.ExtractingMinutes, (int)(seconds / 60), (int)(seconds % 60));
                }

                Invalidate();
                if (_cancel)
                {
                    process.Kill();
                    progressBar1.Visible = false;
                    buttonCancel.Visible = false;
                    DialogResult = DialogResult.Cancel;
                    return null;
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
                                labelInfo.Text = LanguageSettings.Current.AddWaveform.LowDiskSpace;
                            }
                        }
                    }
                    catch
                    {
                        // ignored
                    }
                }
            }

            Application.DoEvents();
            System.Threading.Thread.Sleep(100);

            if (!File.Exists(outWaveFile))
            {
                SeLogger.Error("Generated wave file not found: " + outWaveFile + Environment.NewLine +
                               "ffmpeg: " + process.StartInfo.FileName + Environment.NewLine +
                               "Parameters: " + process.StartInfo.Arguments + Environment.NewLine +
                               "OS: " + Environment.OSVersion + Environment.NewLine +
                               "64-bit: " + Environment.Is64BitOperatingSystem);
            }

            return outWaveFile;
        }

        private Process GetFfmpegProcess(string videoFileName, int audioTrackNumber, string outWaveFile)
        {
            if (!File.Exists(Configuration.Settings.General.FFmpegLocation) && Configuration.IsRunningOnWindows)
            {
                return null;
            }

            var audioParameter = string.Empty;
            if (audioTrackNumber > 0)
            {
                audioParameter = $"-map 0:a:{audioTrackNumber}";
            }

            labelFC.Text = string.Empty;
            var fFmpegWaveTranscodeSettings = "-i \"{0}\" -vn -ar 16000 -ac 1 -ab 32k -af volume=1.75 -f wav {2} \"{1}\"";
            if (_useCenterChannelOnly)
            {
                fFmpegWaveTranscodeSettings = "-i \"{0}\" -vn -ar 16000 -ab 32k -af volume=1.75 -af \"pan=mono|c0=FC\" -f wav {2} \"{1}\"";
                labelFC.Text = "FC";
            }

            //-i indicates the input
            //-vn means no video output
            //-ar 44100 indicates the sampling frequency.
            //-ab indicates the bit rate (in this example 160kb/s)
            //-af volume=1.75 will boot volume... 1.0 is normal
            //-ac 2 means 2 channels
            // "-map 0:a:0" is the first audio stream, "-map 0:a:1" is the second audio stream

            var exeFilePath = Configuration.Settings.General.FFmpegLocation;
            if (!Configuration.IsRunningOnWindows)
            {
                exeFilePath = "ffmpeg";
            }

            var parameters = string.Format(fFmpegWaveTranscodeSettings, videoFileName, outWaveFile, audioParameter);
            return new Process { StartInfo = new ProcessStartInfo(exeFilePath, parameters) { WindowStyle = ProcessWindowStyle.Hidden, CreateNoWindow = true } };
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            if (buttonGenerate.Enabled)
            {
                DialogResult = DialogResult.Cancel;
            }
            else
            {
                _cancel = true;
            }
        }

        private void linkLabelVoskWebsite_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            UiUtil.OpenUrl("https://alphacephei.com/vosk/models");
        }

        private void AudioToText_FormClosing(object sender, FormClosingEventArgs e)
        {
            Configuration.Settings.Tools.VoskModel = comboBoxModels.Text;
            Configuration.Settings.Tools.VoskPostProcessing = checkBoxUsePostProcessing.Checked;

            foreach (var fileName in _filesToDelete)
            {
                try
                {
                    if (File.Exists(fileName))
                    {
                        File.Delete(fileName);
                    }
                }
                catch
                {
                    // ignore
                }
            }
        }

        private void AudioToText_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F2)
            {
                if (textBoxLog.Visible)
                {
                    textBoxLog.Visible = false;
                }
                else
                {
                    textBoxLog.Visible = true;
                    textBoxLog.BringToFront();
                }

                e.SuppressKeyPress = true;
            }
            else if (e.KeyCode == Keys.Escape && buttonGenerate.Enabled)
            {
                DialogResult = DialogResult.Cancel;
                e.SuppressKeyPress = true;
            }
            else if (e.KeyData == UiUtil.HelpKeys)
            {
                UiUtil.ShowHelp("#audio_to_text_vosk");
                e.SuppressKeyPress = true;
            }
        }

        private void linkLabelOpenModelFolder_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            UiUtil.OpenFolder(_voskFolder);
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (_bytesWavRead <= 0 || _bytesWavTotal <= 0)
            {
                return;
            }

            var durationMs = (DateTime.UtcNow.Ticks - _startTicks) / 10_000;
            var msPerFrame = (float)durationMs / _bytesWavRead;
            var estimatedTotalMs = msPerFrame * _bytesWavTotal;
            var estimatedLeft = ProgressHelper.ToProgressTime(estimatedTotalMs - durationMs);
            labelTime.Text = estimatedLeft;
        }

        private void buttonDownload_Click(object sender, EventArgs e)
        {
            using (var form = new VoskModelDownload { AutoClose = true })
            {
                form.ShowDialog(this);
                FillModels(comboBoxModels, form.LastDownloadedModel);
            }
        }

        private void buttonAddFile_Click(object sender, EventArgs e)
        {
            using (var openFileDialog1 = new OpenFileDialog())
            {
                openFileDialog1.Title = LanguageSettings.Current.General.OpenVideoFileTitle;
                openFileDialog1.FileName = string.Empty;
                openFileDialog1.Filter = UiUtil.GetVideoFileFilter(true);
                openFileDialog1.Multiselect = true;
                if (openFileDialog1.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }

                foreach (var fileName in openFileDialog1.FileNames)
                {
                    AddInputFile(fileName);
                }
            }
        }

        private void buttonRemoveFile_Click(object sender, EventArgs e)
        {
            for (var i = listViewInputFiles.SelectedIndices.Count - 1; i >= 0; i--)
            {
                listViewInputFiles.Items.RemoveAt(listViewInputFiles.SelectedIndices[i]);
            }
        }

        private void buttonClear_Click(object sender, EventArgs e)
        {
            listViewInputFiles.Items.Clear();
        }

        private void buttonBatchMode_Click(object sender, EventArgs e)
        {
            _batchMode = !_batchMode;
            ShowHideBatchMode();
        }

        private void ShowHideBatchMode()
        {
            if (_batchMode)
            {
                groupBoxInputFiles.Enabled = true;
                Height = checkBoxUsePostProcessing.Bottom + progressBar1.Height + buttonCancel.Height + 450;
                listViewInputFiles.Visible = true;
                buttonBatchMode.Text = LanguageSettings.Current.Split.Basic;
                MinimumSize = new Size(MinimumSize.Width, Height - 75);
                FormBorderStyle = FormBorderStyle.Sizable;
                MaximizeBox = true;
                MinimizeBox = true;
            }
            else
            {
                groupBoxInputFiles.Enabled = false;
                var h = checkBoxUsePostProcessing.Bottom + progressBar1.Height + buttonCancel.Height + 70;
                MinimumSize = new Size(MinimumSize.Width, h - 10);
                Height = h;
                Width = _initialWidth;
                listViewInputFiles.Visible = false;
                buttonBatchMode.Text = LanguageSettings.Current.AudioToText.BatchMode;
                FormBorderStyle = FormBorderStyle.FixedDialog;
                MaximizeBox = false;
                MinimizeBox = true;
            }
        }

        private void AudioToText_Load(object sender, EventArgs e)
        {
            ShowHideBatchMode();
            listViewInputFiles.Columns[0].Width = -2;
        }

        private void comboBoxModels_SelectedIndexChanged(object sender, EventArgs e)
        {
            _model = null;
        }

        private void AudioToText_Shown(object sender, EventArgs e)
        {
            buttonGenerate.Focus();
            _initialWidth = Width;
        }

        private void listViewInputFiles_DragEnter(object sender, DragEventArgs e)
        {
            if (!buttonGenerate.Visible)
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
            var fileNames = (string[])e.Data.GetData(DataFormats.FileDrop);

            TaskDelayHelper.RunDelayed(TimeSpan.FromMilliseconds(25), () =>
            {
                listViewInputFiles.BeginUpdate();
                foreach (var fileName in fileNames.OrderBy(Path.GetFileName))
                {
                    if (File.Exists(fileName))
                    {
                        AddInputFile(fileName);
                    }
                }

                listViewInputFiles.EndUpdate();
            });
        }

        private void AddInputFile(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                return;
            }

            var ext = Path.GetExtension(fileName).ToLowerInvariant();
            if ((Utilities.AudioFileExtensions.Contains(ext) || Utilities.VideoFileExtensions.Contains(ext)) && File.Exists(fileName))
            {
                listViewInputFiles.Items.Add(fileName);
            }
        }

        private void AudioToText_ResizeEnd(object sender, EventArgs e)
        {
            listViewInputFiles.AutoSizeLastColumn();
        }

        private void listViewInputFiles_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.V && e.Modifiers == Keys.Control) //Ctrl+V = Paste from clipboard
            {
                e.SuppressKeyPress = true;
                if (Clipboard.ContainsFileDropList())
                {
                    foreach (var fileName in Clipboard.GetFileDropList())
                    {
                        AddInputFile(fileName);
                    }
                }
                else if (Clipboard.ContainsText())
                {
                    foreach (var fileName in Clipboard.GetText().SplitToLines())
                    {
                        AddInputFile(fileName);
                    }
                }
            }
        }
    }
}
