using Nikse.SubtitleEdit.Controls;
using Nikse.SubtitleEdit.Core.AudioToText;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.ContainerFormats.Matroska;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Forms.Options;
using Nikse.SubtitleEdit.Logic;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using MessageBox = Nikse.SubtitleEdit.Forms.SeMsgBox.MessageBox;

namespace Nikse.SubtitleEdit.Forms.AudioToText
{
    public sealed partial class WhisperAudioToText : Form
    {
        private readonly string _videoFileName;
        private Subtitle _subtitle;
        private readonly int _audioTrackNumber;
        private bool _cancel;
        private bool _batchMode;
        private int _batchFileNumber;
        private readonly List<string> _filesToDelete;
        private readonly Form _parentForm;
        private bool _useCenterChannelOnly;
        private int _initialWidth = 725;
        private readonly Regex _timeRegexShort = new Regex(@"^\[\d\d:\d\d[\.,]\d\d\d --> \d\d:\d\d[\.,]\d\d\d\]", RegexOptions.Compiled);
        private readonly Regex _timeRegexLong = new Regex(@"^\[\d\d:\d\d:\d\d[\.,]\d\d\d --> \d\d:\d\d:\d\d[\.,]\d\d\d]", RegexOptions.Compiled);
        private readonly Regex _pctWhisper = new Regex(@"^\d+%\|", RegexOptions.Compiled);
        private readonly Regex _pctWhisperFaster = new Regex(@"^\s*\d+%\s*\|", RegexOptions.Compiled);
        private List<ResultText> _resultList;
        private string _languageCode;
        private ConcurrentBag<string> _outputText = new ConcurrentBag<string>();
        private long _startTicks;
        private double _endSeconds;
        private double _showProgressPct = -1;
        private double _lastEstimatedMs = double.MaxValue;
        private VideoInfo _videoInfo;
        private readonly WavePeakData _wavePeaks;
        private readonly List<string> _outputBatchFileNames = new List<string>();

        public bool UnknownArgument { get; set; }
        public bool RunningOnCuda { get; set; }
        public bool IncompleteModel { get; set; }
        public string IncompleteModelName { get; set; }

        private static bool? CudaSomeDevice { get; set; }

        public Subtitle TranscribedSubtitle { get; private set; }

        public WhisperAudioToText(string videoFileName, Subtitle subtitle, int audioTrackNumber, Form parentForm, WavePeakData wavePeaks)
        {
            UiUtil.PreInitialize(this);
            InitializeComponent();
            UiUtil.FixFonts(this);
            UiUtil.FixLargeFonts(this, buttonGenerate);
            _videoFileName = videoFileName;
            _subtitle = subtitle;
            _audioTrackNumber = audioTrackNumber;
            _parentForm = parentForm;
            _wavePeaks = wavePeaks;

            Text = LanguageSettings.Current.AudioToText.Title;
            labelInfo.Text = LanguageSettings.Current.AudioToText.WhisperInfo;
            groupBoxModels.Text = LanguageSettings.Current.AudioToText.LanguagesAndModels;
            labelModel.Text = LanguageSettings.Current.AudioToText.ChooseModel;
            labelChooseLanguage.Text = LanguageSettings.Current.AudioToText.ChooseLanguage;
            linkLabelOpenModelsFolder.Text = LanguageSettings.Current.AudioToText.OpenModelsFolder;
            checkBoxTranslateToEnglish.Text = LanguageSettings.Current.AudioToText.TranslateToEnglish;
            checkBoxUsePostProcessing.Text = LanguageSettings.Current.AudioToText.UsePostProcessing;
            linkLabelPostProcessingConfigure.Left = checkBoxUsePostProcessing.Right + 1;
            linkLabelPostProcessingConfigure.Text = LanguageSettings.Current.Settings.Title;
            checkBoxAutoAdjustTimings.Text = LanguageSettings.Current.AudioToText.AutoAdjustTimings;
            buttonGenerate.Text = LanguageSettings.Current.Watermark.Generate;
            buttonCancel.Text = LanguageSettings.Current.General.Cancel;
            buttonBatchMode.Text = LanguageSettings.Current.AudioToText.BatchMode;
            groupBoxInputFiles.Text = LanguageSettings.Current.BatchConvert.Input;
            linkLabeWhisperWebSite.Text = LanguageSettings.Current.AudioToText.WhisperWebsite;
            buttonAddFile.Text = LanguageSettings.Current.DvdSubRip.Add;
            buttonRemoveFile.Text = LanguageSettings.Current.DvdSubRip.Remove;
            buttonClear.Text = LanguageSettings.Current.DvdSubRip.Clear;
            runOnlyPostProcessingToolStripMenuItem.Text = LanguageSettings.Current.AudioToText.OnlyRunPostProcessing;
            setCPPConstmeModelsFolderToolStripMenuItem.Text = LanguageSettings.Current.AudioToText.SetCppConstMeFolder;
            removeTemporaryFilesToolStripMenuItem.Text = LanguageSettings.Current.AudioToText.RemoveTemporaryFiles;
            buttonAdvanced.Text = LanguageSettings.Current.General.Advanced;
            SetAdvancedLabel();
            downloadCUDAForPurfviewsWhisperFasterToolStripMenuItem.Text = LanguageSettings.Current.AudioToText.DownloadFasterWhisperCuda;

            columnHeaderFileName.Text = LanguageSettings.Current.JoinSubtitles.FileName;

            checkBoxUsePostProcessing.Checked = Configuration.Settings.Tools.VoskPostProcessing;
            checkBoxAutoAdjustTimings.Checked = Configuration.Settings.Tools.WhisperAutoAdjustTimings;

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

            if (_subtitle == null || _subtitle.Paragraphs.Count == 0)
            {
                runOnlyPostProcessingToolStripMenuItem.Visible = false;
                toolStripSeparatorRunOnlyPostprocessing.Visible = false;
            }
            else
            {
                runOnlyPostProcessingToolStripMenuItem.Visible = true;
                toolStripSeparatorRunOnlyPostprocessing.Visible = true;
            }

            textBoxLog.Visible = false;
            textBoxLog.Dock = DockStyle.Fill;
            labelProgress.Text = string.Empty;
            labelTime.Text = string.Empty;
            listViewInputFiles.Visible = false;
            labelElapsed.Text = string.Empty;
            labelEngine.Text = LanguageSettings.Current.AudioToText.Engine;
            labelEngine.Left = comboBoxWhisperEngine.Left - labelEngine.Width - 5;

            if ((Configuration.Settings.Tools.WhisperChoice == WhisperChoice.PurfviewFasterWhisper ||
                 Configuration.Settings.Tools.WhisperChoice == WhisperChoice.PurfviewFasterWhisperXXL)
                && !string.IsNullOrEmpty(Configuration.Settings.Tools.WhisperPurfviewFasterWhisperDefaultCmd))
            {
                Configuration.Settings.Tools.WhisperExtraSettings = Configuration.Settings.Tools.WhisperPurfviewFasterWhisperDefaultCmd;
            }

            Init();
            InitializeWhisperEngines(comboBoxWhisperEngine);
            FixPurfviewWhisperStandardArgument(labelAdvanced, comboBoxWhisperEngine.Text);
        }

        public static void InitializeWhisperEngines(NikseComboBox cb)
        {
            cb.Items.Clear();

            var is64BitOs = IntPtr.Size * 8 == 64;
            if (!is64BitOs)
            {
                cb.Items.Add(WhisperChoice.Cpp);
                cb.SelectedIndex = 0;
                return;
            }

            var engines = new List<string> { WhisperChoice.OpenAi };
            if (Configuration.IsRunningOnWindows)
            {
                engines.Add(WhisperChoice.PurfviewFasterWhisper);
                engines.Add(WhisperChoice.PurfviewFasterWhisperXXL);
                engines.Add(WhisperChoice.Cpp);
                engines.Add(WhisperChoice.CppCuBlas);
                engines.Add(WhisperChoice.ConstMe);
            }
            else
            {
                engines.Add(WhisperChoice.Cpp);
            }
            engines.Add(WhisperChoice.CTranslate2);
            engines.Add(WhisperChoice.StableTs);
            engines.Add(WhisperChoice.WhisperX);

            foreach (var engine in engines)
            {
                cb.Items.Add(engine);
                if (engine == Configuration.Settings.Tools.WhisperChoice)
                {
                    cb.SelectedIndex = cb.Items.Count - 1;
                }
            }

            if (cb.SelectedIndex < 0)
            {
                cb.SelectedIndex = 0;
            }
        }

        private void Init()
        {
            InitializeLanguageNames(comboBoxLanguages);

            FillModels(comboBoxModels, string.Empty);

            labelFC.Text = string.Empty;

            removeTemporaryFilesToolStripMenuItem.Checked = Configuration.Settings.Tools.WhisperDeleteTempFiles;
            ContextMenuStrip = contextMenuStripWhisperAdvanced;
        }

        public static void FillModels(NikseComboBox comboBoxModels, string lastDownloadedModel)
        {
            var whisperModel = WhisperHelper.GetWhisperModel();
            var modelsFolder = whisperModel.ModelFolder;
            var selectName = string.IsNullOrEmpty(lastDownloadedModel) ? Configuration.Settings.Tools.WhisperModel : lastDownloadedModel;

            if (!Directory.Exists(modelsFolder))
            {
                whisperModel.CreateModelFolder();
            }

            comboBoxModels.Items.Clear();

            if (Configuration.Settings.Tools.WhisperChoice == WhisperChoice.CTranslate2 ||
                Configuration.Settings.Tools.WhisperChoice == WhisperChoice.PurfviewFasterWhisper ||
                Configuration.Settings.Tools.WhisperChoice == WhisperChoice.PurfviewFasterWhisperXXL)
            {
                foreach (var model in whisperModel.Models)
                {
                    var path = modelsFolder;
                    var parts = model.Folder.Split('/', '\\').ToList();
                    path = Path.Combine(path, parts[0]);

                    if (Directory.Exists(path))
                    {
                        comboBoxModels.Items.Add(model);
                        if (model.Name == selectName)
                        {
                            try
                            {
                                comboBoxModels.SelectedIndex = comboBoxModels.Items.Count - 1;
                            }
                            catch
                            {
                                // ignore
                            }
                        }
                    }
                }

                // look for custom models 
                var modelSubFolders = Directory.GetDirectories(modelsFolder, "faster-whisper-*");
                foreach (var modelSubFolder in modelSubFolders)
                {
                    var folderNameOnly = Path.GetFileName(modelSubFolder);
                    var x = whisperModel.Models.Where(p => p.Folder.Equals(folderNameOnly, StringComparison.OrdinalIgnoreCase)).ToList();
                    if (!x.Any())
                    {
                        long fileSize = 0;
                        var files = Directory.GetFiles(modelSubFolder, "*" + WhisperHelper.ModelExtension()).ToList();
                        foreach (var file in files)
                        {
                            var fileInfo = new FileInfo(file);
                            fileSize += fileInfo.Length;
                        }

                        var model = new WhisperModel
                        {
                            Name = folderNameOnly.Remove(0, "faster-whisper-".Length),
                            AlreadyDownloaded = false,
                            Folder = Path.Combine(modelsFolder, modelSubFolder),
                            Rename = false,
                            Urls = Array.Empty<string>(),
                            Dynamic = true,
                            Size = Utilities.FormatBytesToDisplayFileSize(fileSize),
                        };

                        comboBoxModels.Items.Add(model);

                        if (model.Name == selectName)
                        {
                            try
                            {
                                comboBoxModels.SelectedIndex = comboBoxModels.Items.Count - 1;
                            }
                            catch
                            {
                                // ignore
                            }
                        }
                    }
                }

                if (comboBoxModels.SelectedIndex < 0 && comboBoxModels.Items.Count > 0)
                {
                    try
                    {
                        comboBoxModels.SelectedIndex = 0;
                    }
                    catch
                    {
                        // ignore
                    }
                }

                return;
            }

            var models = new List<WhisperModel>();
            foreach (var fileName in Directory.GetFiles(modelsFolder))
            {
                var name = Path.GetFileName(fileName);
                var model = whisperModel.Models.FirstOrDefault(p => p.Name + WhisperHelper.ModelExtension() == name);
                if (model == null)
                {
                    continue;
                }

                var fileInfo = new FileInfo(fileName);
                if (fileInfo.Length < 10_000_000)
                {
                    continue;
                }

                model.Bytes = fileInfo.Length;
                models.Add(model);
            }

            foreach (var model in models.OrderBy(m => m.Bytes))
            {
                comboBoxModels.Items.Add(model);
                if (model.Name == selectName)
                {
                    try
                    {
                        comboBoxModels.SelectedIndex = comboBoxModels.Items.Count - 1;
                    }
                    catch
                    {
                        // ignore
                    }
                }
            }

            if (comboBoxModels.SelectedIndex < 0 && comboBoxModels.Items.Count > 0)
            {
                try
                {
                    comboBoxModels.SelectedIndex = 0;
                }
                catch
                {
                    // ignore
                }
            }
        }

        private void ButtonGenerate_Click(object sender, EventArgs e)
        {
            _cancel = false;

            // Check if chosen whisper implementation is installed
            if (comboBoxWhisperEngine.Text == WhisperChoice.Cpp)
            {
                var fileName = WhisperHelper.GetWhisperPathAndFileName(WhisperChoice.Cpp);
                if (!File.Exists(fileName))
                {
                    if (MessageBox.Show(string.Format(LanguageSettings.Current.Settings.DownloadX, "Whisper CPP"), "Subtitle Edit", MessageBoxButtons.YesNoCancel) == DialogResult.Yes)
                    {
                        using (var downloadForm = new WhisperDownload(WhisperChoice.Cpp))
                        {
                            if (downloadForm.ShowDialog(this) != DialogResult.OK)
                            {
                                return;
                            }
                        }
                    }
                    else
                    {
                        return;
                    }
                }
            }

            if (comboBoxWhisperEngine.Text == WhisperChoice.CppCuBlas)
            {
                var fileName = WhisperHelper.GetWhisperPathAndFileName(WhisperChoice.CppCuBlas);
                if (!File.Exists(fileName))
                {
                    if (MessageBox.Show(string.Format(LanguageSettings.Current.Settings.DownloadX, "Whisper " + WhisperChoice.CppCuBlas), "Subtitle Edit", MessageBoxButtons.YesNoCancel) == DialogResult.Yes)
                    {
                        using (var downloadForm = new WhisperDownload(WhisperChoice.CppCuBlas))
                        {
                            if (downloadForm.ShowDialog(this) != DialogResult.OK)
                            {
                                return;
                            }
                        }
                    }
                    else
                    {
                        return;
                    }
                }
            }

            if (comboBoxWhisperEngine.Text == WhisperChoice.PurfviewFasterWhisper ||
                comboBoxWhisperEngine.Text == WhisperChoice.PurfviewFasterWhisperXXL)
            {
                var fileName = WhisperHelper.GetWhisperPathAndFileName(WhisperChoice.PurfviewFasterWhisper);
                if (!File.Exists(fileName))
                {
                    if (MessageBox.Show(string.Format(LanguageSettings.Current.Settings.DownloadX, comboBoxWhisperEngine.Text), "Subtitle Edit", MessageBoxButtons.YesNoCancel) == DialogResult.Yes)
                    {
                        using (var downloadForm = new WhisperDownload(comboBoxWhisperEngine.Text))
                        {
                            if (downloadForm.ShowDialog(this) != DialogResult.OK)
                            {
                                return;
                            }
                        }

                        if (comboBoxWhisperEngine.Text == WhisperChoice.PurfviewFasterWhisper &&
                            !IsFasterWhisperCudaInstalled() && 
                            IsFasterWhisperCudaSupported())
                        {
                            DownloadCudaForWhisperFaster(this);
                        }
                    }
                    else
                    {
                        return;
                    }
                }
            }

            // Model must be installed
            if (comboBoxModels.Items.Count == 0)
            {
                buttonDownload_Click(null, null);
                return;
            }

            _languageCode = GetLanguage(comboBoxLanguages.Text);

            if (comboBoxModels.Items[comboBoxModels.SelectedIndex] is WhisperModel model &&
                _languageCode != "en" && IsModelEnglishOnly(model))
            {
                var result = MessageBox.Show("English model should only be used with English language." + Environment.NewLine +
                "Continue anyway?", Text, MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning);
                if (result != DialogResult.Yes)
                {
                    return;
                }
            }

            if (comboBoxModels.Items[comboBoxModels.SelectedIndex] is WhisperModel model2 &&
                _languageCode != "no" && _languageCode != "nb" && IsModelNorwegianOnly(model2))
            {
                var result = MessageBox.Show("Norwegian model should only be used with Norwegian language." + Environment.NewLine +
                                             "Continue anyway?", Text, MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning);
                if (result != DialogResult.Yes)
                {
                    return;
                }
            }

            try
            {
                var f = SeLogger.GetWhisperLogFilePath();
                if (File.Exists(f) && new FileInfo(f).Length > 100_000)
                {
                    File.Delete(f);
                }
            }
            catch
            {
                // ignore
            }

            _useCenterChannelOnly = Configuration.Settings.General.FFmpegUseCenterChannelOnly &&
                                    FfmpegMediaInfo.Parse(_videoFileName).HasFrontCenterAudio(_audioTrackNumber);

            IncompleteModel = false;
            ShowProgressBar();

            if (_batchMode)
            {
                if (listViewInputFiles.Items.Count == 0)
                {
                    buttonAddFile_Click(null, null);
                    return;
                }

                timer1.Start();
                GenerateBatch();
                TaskbarList.SetProgressState(_parentForm.Handle, TaskbarButtonProgressFlags.NoProgress);
                timer1.Stop();
                return;
            }

            buttonGenerate.Enabled = false;
            buttonDownload.Enabled = false;
            buttonBatchMode.Enabled = false;
            buttonAdvanced.Enabled = false;
            comboBoxLanguages.Enabled = false;
            comboBoxModels.Enabled = false;
            linkLabelPostProcessingConfigure.Enabled = false;
            var waveFileName = GenerateWavFile(_videoFileName, _audioTrackNumber);
            if (_cancel)
            {
                return;
            }

            progressBar1.Style = ProgressBarStyle.Blocks;
            timer1.Start();
            var transcript = TranscribeViaWhisper(waveFileName, _videoFileName);
            timer1.Stop();
            if (_cancel && (transcript == null || transcript.Paragraphs.Count == 0 || MessageBox.Show(LanguageSettings.Current.AudioToText.KeepPartialTranscription, Text, MessageBoxButtons.YesNoCancel) != DialogResult.Yes))
            {
                DialogResult = DialogResult.Cancel;
                return;
            }

            timer1.Start();
            if (_showProgressPct > 0 && progressBar1.Style == ProgressBarStyle.Blocks)
            {
                _showProgressPct = 100;
                progressBar1.Value = progressBar1.Maximum;
            }

            if (checkBoxAutoAdjustTimings.Checked || checkBoxUsePostProcessing.Checked)
            {
                labelProgress.Text = LanguageSettings.Current.AudioToText.PostProcessing;
            }

            labelTime.Text = string.Empty;
            labelProgress.Refresh();
            Application.DoEvents();

            var postProcessor = new AudioToTextPostProcessor(checkBoxTranslateToEnglish.Checked ? "en" : _languageCode)
            {
                ParagraphMaxChars = Configuration.Settings.General.SubtitleLineMaximumLength * 2,
            };

            WavePeakData wavePeaks = null;
            if (checkBoxAutoAdjustTimings.Checked)
            {
                wavePeaks = _wavePeaks ?? MakeWavePeaks();
            }

            if (checkBoxAutoAdjustTimings.Checked && wavePeaks != null)
            {
                transcript = WhisperTimingFixer.ShortenLongDuration(transcript);
                transcript = WhisperTimingFixer.ShortenViaWavePeaks(transcript, wavePeaks);
            }

            TranscribedSubtitle = postProcessor.Fix(
                AudioToTextPostProcessor.Engine.Whisper,
                transcript,
                checkBoxUsePostProcessing.Checked,
                Configuration.Settings.Tools.WhisperPostProcessingAddPeriods,
                Configuration.Settings.Tools.WhisperPostProcessingMergeLines,
                Configuration.Settings.Tools.WhisperPostProcessingFixCasing,
                Configuration.Settings.Tools.WhisperPostProcessingFixShortDuration,
                Configuration.Settings.Tools.WhisperPostProcessingSplitLines);

            if (transcript == null || transcript.Paragraphs.Count == 0)
            {
                UpdateLog();
                SeLogger.WhisperInfo(textBoxLog.Text);
                IncompleteModelName = comboBoxModels.Text;
            }
            else
            {
                UpdateLog();
                SeLogger.WhisperInfo(textBoxLog.Text);
            }

            timer1.Stop();

            DialogResult = DialogResult.OK;
        }

        private static bool IsModelEnglishOnly(WhisperModel model)
        {
            return model.Name.EndsWith(".en", StringComparison.InvariantCulture) ||
                   model.Name == "distil-large-v2" ||
                   model.Name == "distil-large-v3";
        }

        private static bool IsModelNorwegianOnly(WhisperModel model)
        {
            return model.ToString().Contains("Norwegian", StringComparison.OrdinalIgnoreCase);
        }

        private void ShowProgressBar()
        {
            progressBar1.Maximum = 100;
            progressBar1.Value = 0;
            progressBar1.Visible = true;
            progressBar1.Refresh();
            progressBar1.Top = labelProgress.Bottom + 3;
            labelElapsed.Top = progressBar1.Top - labelElapsed.Height - 3;
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
            _outputText.Add("Batch mode");
            foreach (ListViewItem lvi in listViewInputFiles.Items)
            {
                _batchFileNumber++;
                var videoFileName = lvi.Text;
                listViewInputFiles.SelectedIndices.Clear();
                lvi.Selected = true;
                buttonGenerate.Enabled = false;
                buttonDownload.Enabled = false;
                buttonBatchMode.Enabled = false;
                buttonAdvanced.Enabled = false;
                comboBoxModels.Enabled = false;
                comboBoxLanguages.Enabled = false;
                var waveFileName = GenerateWavFile(videoFileName, _audioTrackNumber);
                if (!File.Exists(waveFileName))
                {
                    errors.AppendLine("Unable to extract audio from: " + videoFileName);
                    errorCount++;
                    continue;
                }

                _outputText.Add(string.Empty);
                progressBar1.Style = ProgressBarStyle.Blocks;
                var transcript = TranscribeViaWhisper(waveFileName, videoFileName);
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

                WavePeakData wavePeaks = null;
                if (checkBoxAutoAdjustTimings.Checked)
                {
                    wavePeaks = _wavePeaks ?? MakeWavePeaks();
                }

                if (checkBoxAutoAdjustTimings.Checked && wavePeaks != null)
                {
                    transcript = WhisperTimingFixer.ShortenLongDuration(transcript);
                    transcript = WhisperTimingFixer.ShortenViaWavePeaks(transcript, wavePeaks);
                }

                var postProcessor = new AudioToTextPostProcessor(_languageCode)
                {
                    ParagraphMaxChars = Configuration.Settings.General.SubtitleLineMaximumLength * 2,
                };
                TranscribedSubtitle = postProcessor.Fix(
                    AudioToTextPostProcessor.Engine.Whisper,
                    transcript,
                    checkBoxUsePostProcessing.Checked,
                    Configuration.Settings.Tools.WhisperPostProcessingAddPeriods,
                    Configuration.Settings.Tools.WhisperPostProcessingMergeLines,
                    Configuration.Settings.Tools.WhisperPostProcessingFixCasing,
                    Configuration.Settings.Tools.WhisperPostProcessingFixShortDuration,
                    Configuration.Settings.Tools.WhisperPostProcessingSplitLines);


                SaveToSourceFolder(videoFileName);
                TaskbarList.SetProgressValue(_parentForm.Handle, _batchFileNumber, listViewInputFiles.Items.Count);
            }

            progressBar1.Visible = false;
            labelTime.Text = string.Empty;

            TaskbarList.StartBlink(_parentForm, 10, 1, 2);

            Activate();
            Focus();
            Application.DoEvents();

            if (errors.Length > 0)
            {
                MessageBox.Show(this, $"{errorCount} error(s)!{Environment.NewLine}{errors}", Text, MessageBoxButtons.OK);
            }

            var fileList = Environment.NewLine + Environment.NewLine + string.Join(Environment.NewLine, _outputBatchFileNames);
            MessageBox.Show(this, string.Format(LanguageSettings.Current.AudioToText.XFilesSavedToVideoSourceFolder, listViewInputFiles.Items.Count - errorCount) + fileList, Text, MessageBoxButtons.OK);

            groupBoxInputFiles.Enabled = true;
            buttonGenerate.Enabled = true;
            buttonDownload.Enabled = true;
            buttonBatchMode.Enabled = true;
            buttonAdvanced.Enabled = true;
            DialogResult = DialogResult.Cancel;
        }

        private WavePeakData MakeWavePeaks()
        {
            if (string.IsNullOrEmpty(_videoFileName) || !File.Exists(_videoFileName))
            {
                return null;
            }

            var targetFile = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".wav");
            try
            {
                var process = AddWaveform.GetCommandLineProcess(_videoFileName, -1, targetFile, Configuration.Settings.General.VlcWaveTranscodeSettings, out var encoderName);
                process.Start();
                while (!process.HasExited)
                {
                    Application.DoEvents();
                }

                // check for delay in matroska files
                var delayInMilliseconds = 0;
                var audioTrackNames = new List<string>();
                var mkvAudioTrackNumbers = new Dictionary<int, int>();
                if (_videoFileName.ToLowerInvariant().EndsWith(".mkv", StringComparison.OrdinalIgnoreCase))
                {
                    try
                    {
                        using (var matroska = new MatroskaFile(_videoFileName))
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
                                    delayInMilliseconds = (int)matroska.GetAudioTrackDelayMilliseconds(mkvAudioTrackNumbers[0]);
                                }
                            }
                        }
                    }
                    catch (Exception exception)
                    {
                        SeLogger.Error(exception, $"Error getting delay from mkv: {_videoFileName}");
                    }
                }

                if (File.Exists(targetFile))
                {
                    using (var waveFile = new WavePeakGenerator(targetFile))
                    {
                        if (!string.IsNullOrEmpty(_videoFileName) && File.Exists(_videoFileName))
                        {
                            return waveFile.GeneratePeaks(delayInMilliseconds, WavePeakGenerator.GetPeakWaveFileName(_videoFileName));
                        }
                    }
                }
            }
            catch
            {
                // ignore
            }

            return null;
        }

        private void SaveToSourceFolder(string videoFileName)
        {
            var format = SubtitleFormat.FromName(Configuration.Settings.General.DefaultSubtitleFormat, new SubRip());
            if (format.GetType() == typeof(AdvancedSubStationAlpha))
            {
                try
                {
                    var info = FfmpegMediaInfo.Parse(videoFileName);
                    if (info.Dimension.Width > 0)
                    {
                        if (string.IsNullOrEmpty(TranscribedSubtitle.Header))
                        {
                            TranscribedSubtitle.Header = AdvancedSubStationAlpha.DefaultHeader;
                        }

                        TranscribedSubtitle.Header = AdvancedSubStationAlpha.AddTagToHeader("PlayResX", "PlayResX: " + info.Dimension.Width.ToString(CultureInfo.InvariantCulture), "[Script Info]", TranscribedSubtitle.Header);
                        TranscribedSubtitle.Header = AdvancedSubStationAlpha.AddTagToHeader("PlayResY", "PlayResY: " + info.Dimension.Height.ToString(CultureInfo.InvariantCulture), "[Script Info]", TranscribedSubtitle.Header);
                    }
                }
                catch
                {
                    // ignore
                }
            }

            var text = TranscribedSubtitle.ToText(format);

            var fileName = Path.Combine(Utilities.GetPathAndFileNameWithoutExtension(videoFileName)) + format.Extension;
            if (File.Exists(fileName))
            {
                fileName = $"{Path.Combine(Utilities.GetPathAndFileNameWithoutExtension(videoFileName))}.{Guid.NewGuid().ToString()}{format.Extension}";
            }

            try
            {
                File.WriteAllText(fileName, text, Encoding.UTF8);
                _outputText.Add("Subtitle written to : " + fileName);
                _outputBatchFileNames.Add(fileName);
            }
            catch
            {
                var dir = Path.GetDirectoryName(fileName);
                if (!IsDirectoryWritable(dir))
                {
                    MessageBox.Show($"SE does not have write access to the folder '{dir}'", MessageBoxIcon.Error);
                }

                throw;
            }
        }

        public static bool IsDirectoryWritable(string dirPath)
        {
            try
            {
                using (FileStream fs = File.Create(Path.Combine(dirPath, Path.GetRandomFileName()), 1, FileOptions.DeleteOnClose))
                {
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        internal static string GetLanguage(string name)
        {
            var language = WhisperLanguage.Languages.FirstOrDefault(l => l.Name == name);
            return language != null ? language.Code : "en";
        }

        public Subtitle TranscribeViaWhisper(string waveFileName, string videoFileName)
        {
            _showProgressPct = -1;
            var model = comboBoxModels.Items[comboBoxModels.SelectedIndex] as WhisperModel;
            if (model == null)
            {
                return new Subtitle();
            }

            labelProgress.Text = LanguageSettings.Current.AudioToText.Transcribing;
            if (_batchMode)
            {
                labelProgress.Text = string.Format(LanguageSettings.Current.AudioToText.TranscribingXOfY, _batchFileNumber, listViewInputFiles.Items.Count);
            }
            else
            {
                TaskbarList.SetProgressValue(_parentForm.Handle, 1, 100);
            }

            //Delete invalid preprocessor_config.json file
            if (Configuration.Settings.Tools.WhisperChoice == WhisperChoice.PurfviewFasterWhisper ||
                Configuration.Settings.Tools.WhisperChoice == WhisperChoice.PurfviewFasterWhisperCuda ||
                Configuration.Settings.Tools.WhisperChoice == WhisperChoice.PurfviewFasterWhisperXXL)
            {
                var dir = Path.Combine(WhisperHelper.GetWhisperFolder(), "_models", model.Folder);
                if (Directory.Exists(dir))
                {
                    try
                    {
                        var jsonFileName = Path.Combine(dir, "preprocessor_config.json");
                        if (File.Exists(jsonFileName))
                        {
                            var text = FileUtil.ReadAllTextShared(jsonFileName, Encoding.UTF8);
                            if (text.StartsWith("Entry not found", StringComparison.OrdinalIgnoreCase))
                            {
                                File.Delete(jsonFileName);
                            }
                        }

                        jsonFileName = Path.Combine(dir, "vocabulary.json");
                        if (File.Exists(jsonFileName))
                        {
                            var text = FileUtil.ReadAllTextShared(jsonFileName, Encoding.UTF8);
                            if (text.StartsWith("Entry not found", StringComparison.OrdinalIgnoreCase))
                            {
                                File.Delete(jsonFileName);
                            }
                        }
                    }
                    catch
                    {
                        // ignore
                    }
                }
            }

            labelProgress.Refresh();
            Application.DoEvents();
            _resultList = new List<ResultText>();

            var inputFile = waveFileName;
            if (!_useCenterChannelOnly &&
                (comboBoxWhisperEngine.Text == WhisperChoice.PurfviewFasterWhisper ||
                 comboBoxWhisperEngine.Text == WhisperChoice.PurfviewFasterWhisperXXL) &&
                _audioTrackNumber <= 0)
            {
                inputFile = videoFileName;
            }

            var process = GetWhisperProcess(inputFile, model.Name, _languageCode, checkBoxTranslateToEnglish.Checked, OutputHandler);
            var sw = Stopwatch.StartNew();
            _outputText.Add($"Calling whisper ({Configuration.Settings.Tools.WhisperChoice}) with : {process.StartInfo.FileName} {process.StartInfo.Arguments}{Environment.NewLine}");
            _startTicks = DateTime.UtcNow.Ticks;
            _videoInfo = UiUtil.GetVideoInfo(waveFileName);
            timer1.Start();
            if (!_batchMode)
            {
                ShowProgressBar();
                progressBar1.Style = ProgressBarStyle.Marquee;
            }

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

            labelProgress.Text = LanguageSettings.Current.AudioToText.Transcribing;
            while (!process.HasExited)
            {
                Application.DoEvents();
                System.Threading.Thread.Sleep(100);
                WindowsHelper.PreventStandBy();

                if (_cancel)
                {
                    if ((comboBoxWhisperEngine.Text == WhisperChoice.PurfviewFasterWhisper ||
                         comboBoxWhisperEngine.Text == WhisperChoice.PurfviewFasterWhisperXXL) && 
                        KillProcessHelper.AttachConsole((uint)process.Id))
                    {
                        KillProcessHelper.TryToKillProcessViaCtrlC(process);
                    }
                    else
                    {
                        process.Kill();
                    }

                    progressBar1.Visible = false;
                    buttonCancel.Visible = false;
                    DialogResult = DialogResult.Cancel;

                    var partialSub = new Subtitle();
                    partialSub.Paragraphs.AddRange(_resultList.OrderBy(p => p.Start).Select(p => new Paragraph(p.Text, (double)p.Start * 1000.0, (double)p.End * 1000.0)).ToList());
                    if (partialSub.Paragraphs.Count > 0)
                    {
                        return partialSub;
                    }

                    return null;
                }
            }

            _outputText.Add($"Calling whisper {Configuration.Settings.Tools.WhisperChoice} done in {sw.Elapsed}{Environment.NewLine}");

            for (var i = 0; i < 10; i++)
            {
                Application.DoEvents();
                System.Threading.Thread.Sleep(50);
            }

            process.Dispose();

            if (GetResultFromSrt(waveFileName, videoFileName, out var resultTexts, _outputText, _filesToDelete))
            {
                var subtitle = new Subtitle();
                subtitle.Paragraphs.AddRange(resultTexts.Select(p => new Paragraph(p.Text, (double)p.Start * 1000.0, (double)p.End * 1000.0)).ToList());
                return subtitle;
            }

            _outputText?.Add("Loading result from STDOUT" + Environment.NewLine);

            var sub = new Subtitle();
            sub.Paragraphs.AddRange(_resultList.OrderBy(p => p.Start).Select(p => new Paragraph(p.Text, (double)p.Start * 1000.0, (double)p.End * 1000.0)).ToList());
            return sub;
        }

        public static bool GetResultFromSrt(string waveFileName, string videoFileName, out List<ResultText> resultTexts, ConcurrentBag<string> outputText, List<string> filesToDelete)
        {
            var srtFileName = waveFileName + ".srt";
            if (!File.Exists(srtFileName) && waveFileName.EndsWith(".wav"))
            {
                srtFileName = waveFileName.Remove(waveFileName.Length - 4) + ".srt";
            }

            var whisperFolder = WhisperHelper.GetWhisperFolder() ?? string.Empty;
            if (!string.IsNullOrEmpty(whisperFolder) && !File.Exists(srtFileName) && !string.IsNullOrEmpty(videoFileName))
            {
                srtFileName = Path.Combine(whisperFolder, Path.GetFileNameWithoutExtension(videoFileName)) + ".srt";
            }

            if (!File.Exists(srtFileName))
            {
                srtFileName = Path.Combine(whisperFolder, Path.GetFileNameWithoutExtension(waveFileName)) + ".srt";
            }

            var vttFileName = Path.Combine(whisperFolder, Path.GetFileName(waveFileName) + ".vtt");
            if (!File.Exists(vttFileName))
            {
                vttFileName = Path.Combine(whisperFolder, Path.GetFileNameWithoutExtension(waveFileName)) + ".vtt";
            }

            if (!File.Exists(srtFileName) && !File.Exists(vttFileName))
            {
                resultTexts = new List<ResultText>();
                return false;
            }

            var sub = new Subtitle();
            if (File.Exists(srtFileName))
            {
                var rawText = FileUtil.ReadAllLinesShared(srtFileName, Encoding.UTF8);
                new SubRip().LoadSubtitle(sub, rawText, srtFileName);
                outputText?.Add($"Loading result from {srtFileName}{Environment.NewLine}");
            }
            else
            {
                var rawText = FileUtil.ReadAllLinesShared(srtFileName, Encoding.UTF8);
                new WebVTT().LoadSubtitle(sub, rawText, srtFileName);
                outputText?.Add($"Loading result from {vttFileName}{Environment.NewLine}");
            }

            sub.RemoveEmptyLines();

            var results = new List<ResultText>();
            foreach (var p in sub.Paragraphs)
            {
                results.Add(new ResultText
                {
                    Start = (decimal)p.StartTime.TotalSeconds,
                    End = (decimal)p.EndTime.TotalSeconds,
                    Text = p.Text
                });
            }

            resultTexts = results;

            if (File.Exists(srtFileName))
            {
                filesToDelete?.Add(srtFileName);
            }

            if (File.Exists(vttFileName))
            {
                filesToDelete?.Add(vttFileName);
            }

            return true;
        }

        private void OutputHandler(object sendingProcess, DataReceivedEventArgs outLine)
        {
            if (string.IsNullOrWhiteSpace(outLine.Data))
            {
                return;
            }

            if (outLine.Data.Contains("not all tensors loaded from model file"))
            {
                IncompleteModel = true;
            }

            if (outLine.Data.Contains("error: unknown argument: ", StringComparison.OrdinalIgnoreCase))
            {
                UnknownArgument = true;
            }
            else if (outLine.Data.Contains("error: unrecognized argument: ", StringComparison.OrdinalIgnoreCase))
            {
                UnknownArgument = true;
            }

            if (outLine.Data.Contains("running on: CUDA", StringComparison.OrdinalIgnoreCase))
            {
                RunningOnCuda = true;
            }

            _outputText.Add(outLine.Data.Trim() + Environment.NewLine);

            foreach (var line in outLine.Data.SplitToLines())
            {
                if (_timeRegexShort.IsMatch(line))
                {
                    var start = line.Substring(1, 10);
                    var end = line.Substring(14, 10);
                    var text = line.Remove(0, 25).Trim();
                    var rt = new ResultText
                    {
                        Start = GetSeconds(start),
                        End = GetSeconds(end),
                        Text = Utilities.AutoBreakLine(text, _languageCode),
                    };

                    if (_showProgressPct < 0)
                    {
                        _endSeconds = (double)rt.End;
                    }

                    _resultList.Add(rt);
                }
                else if (_timeRegexLong.IsMatch(line))
                {
                    var start = line.Substring(1, 12);
                    var end = line.Substring(18, 12);
                    var text = line.Remove(0, 31).Trim();
                    var rt = new ResultText
                    {
                        Start = GetSeconds(start),
                        End = GetSeconds(end),
                        Text = Utilities.AutoBreakLine(text, _languageCode),
                    };

                    if (_showProgressPct < 0)
                    {
                        _endSeconds = (double)rt.End;
                    }

                    _resultList.Add(rt);
                }
                else if (line.StartsWith("whisper_full: progress =", StringComparison.OrdinalIgnoreCase))
                {
                    var arr = line.Split('=');
                    if (arr.Length == 2)
                    {
                        var pctString = arr[1].Trim().TrimEnd('%').TrimEnd();
                        if (double.TryParse(pctString, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var pct))
                        {
                            _endSeconds = _videoInfo.TotalSeconds * pct / 100.0;
                            _showProgressPct = pct;
                        }
                    }
                }
                else if (_pctWhisper.IsMatch(line.TrimStart()))
                {
                    var arr = line.Split('%');
                    if (arr.Length > 1 && double.TryParse(arr[0], NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var pct))
                    {
                        _endSeconds = _videoInfo.TotalSeconds * pct / 100.0;
                        _showProgressPct = pct;
                    }
                }
                else if (_pctWhisperFaster.IsMatch(line))
                {
                    var arr = line.Split('%');
                    if (arr.Length > 1 && double.TryParse(arr[0].Trim(), NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var pct))
                    {
                        _endSeconds = _videoInfo.TotalSeconds * pct / 100.0;
                        _showProgressPct = pct;
                    }
                }
            }
        }

        private static decimal GetSeconds(string timeCode)
        {
            return (decimal)(TimeCode.ParseToMilliseconds(timeCode) / 1000.0);
        }

        private string GenerateWavFile(string videoFileName, int audioTrackNumber)
        {
            if (videoFileName.EndsWith(".wav"))
            {
                try
                {
                    using (var waveFile = new WavePeakGenerator(videoFileName))
                    {
                        if (waveFile.Header != null && waveFile.Header.SampleRate == 16000)
                        {
                            return videoFileName;
                        }
                    }
                }
                catch
                {
                    // ignore
                }
            }

            var ffmpegLog = new StringBuilder();
            var outWaveFile = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".wav");
            _filesToDelete.Add(outWaveFile);
            var process = GetFfmpegProcess(videoFileName, audioTrackNumber, outWaveFile);

            process.ErrorDataReceived += (sender, args) =>
            {
                ffmpegLog.AppendLine(args.Data);
            };

            process.StartInfo.RedirectStandardError = true;
            process.Start();
            process.BeginErrorReadLine();

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
                SeLogger.WhisperInfo("Generated wave file not found: " + outWaveFile + Environment.NewLine +
                               "ffmpeg: " + process.StartInfo.FileName + Environment.NewLine +
                               "Parameters: " + process.StartInfo.Arguments + Environment.NewLine +
                               "OS: " + Environment.OSVersion + Environment.NewLine +
                               "64-bit: " + Environment.Is64BitOperatingSystem + Environment.NewLine +
                               "ffmpeg exit code: " + process.ExitCode + Environment.NewLine +
                               "ffmpeg log: " + ffmpegLog);
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
            return new Process
            {
                StartInfo = new ProcessStartInfo(exeFilePath, parameters)
                {
                    WindowStyle = ProcessWindowStyle.Hidden,
                    CreateNoWindow = true,
                    UseShellExecute = false,
                }
            };
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

        public static Process GetWhisperProcess(string waveFileName, string model, string language, bool translate, DataReceivedEventHandler dataReceivedHandler = null)
        {
            // whisper --model tiny.en --language English --fp16 False a.wav

            var translateToEnglish = translate ? WhisperHelper.GetWhisperTranslateParameter() : string.Empty;
            if (language.ToLowerInvariant() == "english" || language.ToLowerInvariant() == "en")
            {
                language = "en";
                translateToEnglish = string.Empty;
            }

            if (Configuration.Settings.Tools.WhisperChoice == WhisperChoice.Cpp || Configuration.Settings.Tools.WhisperChoice == WhisperChoice.CppCuBlas)
            {
                if (!Configuration.Settings.Tools.WhisperExtraSettings.Contains("--print-progress"))
                {
                    translateToEnglish += "--print-progress ";
                }
            }

            var outputSrt = string.Empty;
            var postParams = string.Empty;
            if (Configuration.Settings.Tools.WhisperChoice == WhisperChoice.Cpp ||
                Configuration.Settings.Tools.WhisperChoice == WhisperChoice.CppCuBlas ||
                Configuration.Settings.Tools.WhisperChoice == WhisperChoice.ConstMe)
            {
                outputSrt = "--output-srt ";
            }
            else if (Configuration.Settings.Tools.WhisperChoice == WhisperChoice.StableTs)
            {
                var srtFileName = Path.GetFileNameWithoutExtension(waveFileName);
                postParams = $" -o {srtFileName}.srt";
            }

            var w = WhisperHelper.GetWhisperPathAndFileName();
            var m = WhisperHelper.GetWhisperModelForCmdLine(model);
            var parameters = $"--language {language} --model \"{m}\" {outputSrt}{translateToEnglish}{Configuration.Settings.Tools.WhisperExtraSettings} \"{waveFileName}\"{postParams}";

            SeLogger.WhisperInfo($"{w} {parameters}");

            var process = new Process { StartInfo = new ProcessStartInfo(w, parameters) { WindowStyle = ProcessWindowStyle.Hidden, CreateNoWindow = true } };

            if (!string.IsNullOrEmpty(Configuration.Settings.General.FFmpegLocation) && process.StartInfo.EnvironmentVariables["Path"] != null)
            {
                process.StartInfo.EnvironmentVariables["Path"] = process.StartInfo.EnvironmentVariables["Path"].TrimEnd(';') + ";" + Path.GetDirectoryName(Configuration.Settings.General.FFmpegLocation);
            }

            var whisperFolder = WhisperHelper.GetWhisperFolder();
            if (!string.IsNullOrEmpty(whisperFolder))
            {
                if (File.Exists(whisperFolder))
                {
                    whisperFolder = Path.GetDirectoryName(whisperFolder);
                }

                if (whisperFolder != null)
                {
                    process.StartInfo.WorkingDirectory = whisperFolder;
                }
            }

            if (!string.IsNullOrEmpty(whisperFolder) && process.StartInfo.EnvironmentVariables["Path"] != null)
            {
                process.StartInfo.EnvironmentVariables["Path"] = process.StartInfo.EnvironmentVariables["Path"].TrimEnd(';') + ";" + whisperFolder;
            }

            if (Configuration.Settings.Tools.WhisperChoice != WhisperChoice.Cpp &&
                Configuration.Settings.Tools.WhisperChoice != WhisperChoice.CppCuBlas &&
                 Configuration.Settings.Tools.WhisperChoice != WhisperChoice.ConstMe)
            {
                process.StartInfo.EnvironmentVariables["PYTHONIOENCODING"] = "utf-8";
                process.StartInfo.EnvironmentVariables["PYTHONUTF8"] = "1";
                //process.StartInfo.EnvironmentVariables["PYTHONLEGACYWINDOWSSTDIO"] = "utf-8";
            }

            if (dataReceivedHandler != null)
            {
                process.StartInfo.StandardOutputEncoding = Encoding.UTF8;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;
                process.OutputDataReceived += dataReceivedHandler;
                process.ErrorDataReceived += dataReceivedHandler;
            }

            process.Start();

            if (dataReceivedHandler != null)
            {
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
            }

            return process;
        }

        private void linkLabelWhisperWebsite_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            UiUtil.OpenUrl(WhisperHelper.GetWebSiteUrl());
        }

        private void AudioToText_FormClosing(object sender, FormClosingEventArgs e)
        {
            TaskbarList.SetProgressState(_parentForm.Handle, TaskbarButtonProgressFlags.NoProgress);

            if (comboBoxModels.SelectedItem is WhisperModel model)
            {
                Configuration.Settings.Tools.WhisperModel = model.Name;
            }

            if (comboBoxLanguages.SelectedItem is WhisperLanguage language)
            {
                Configuration.Settings.Tools.WhisperLanguageCode = language.Code;
            }

            Configuration.Settings.Tools.VoskPostProcessing = checkBoxUsePostProcessing.Checked;
            Configuration.Settings.Tools.WhisperAutoAdjustTimings = checkBoxAutoAdjustTimings.Checked;

            DeleteTemporaryFiles(_filesToDelete);
        }

        public static void DeleteTemporaryFiles(List<string> filesToDelete)
        {
            if (!Configuration.Settings.Tools.WhisperDeleteTempFiles)
            {
                return;
            }

            foreach (var fileName in filesToDelete)
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
                    UpdateLog();
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
                UiUtil.ShowHelp("#audio_to_text_whisper");
                e.SuppressKeyPress = true;
            }
        }

        private void linkLabelOpenModelFolder_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            UiUtil.OpenFolder(WhisperHelper.GetWhisperModel().ModelFolder);
        }

        private void UpdateLog()
        {
            if (_outputText.IsEmpty)
            {
                return;
            }

            textBoxLog.AppendText(string.Join(Environment.NewLine, _outputText));
            _outputText = new ConcurrentBag<string>();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            UpdateLog();

            if (_batchMode)
            {
                var pct = _batchFileNumber * 100.0 / listViewInputFiles.Items.Count;
                SetProgressBarPct(pct);
                return;
            }

            var durationMs = (DateTime.UtcNow.Ticks - _startTicks) / 10_000;

            labelElapsed.Text = new TimeCode(durationMs).ToShortDisplayString();
            if (_endSeconds <= 0 || _videoInfo == null)
            {
                if (_showProgressPct > 0)
                {
                    if (progressBar1.Style != ProgressBarStyle.Blocks)
                    {
                        progressBar1.Style = ProgressBarStyle.Blocks;
                        progressBar1.Maximum = 100;
                    }

                    SetProgressBarPct(_showProgressPct);
                }

                return;
            }

            if (progressBar1.Style != ProgressBarStyle.Blocks)
            {
                progressBar1.Style = ProgressBarStyle.Blocks;
                progressBar1.Maximum = 100;
            }

            _videoInfo.TotalSeconds = Math.Max(_endSeconds, _videoInfo.TotalSeconds);
            var msPerFrame = durationMs / (_endSeconds * 1000.0);
            var estimatedTotalMs = msPerFrame * _videoInfo.TotalMilliseconds;
            var msEstimatedLeft = estimatedTotalMs - durationMs;
            if (msEstimatedLeft > _lastEstimatedMs)
            {
                msEstimatedLeft = _lastEstimatedMs;
            }
            else
            {
                _lastEstimatedMs = msEstimatedLeft;
            }

            if (_showProgressPct > 0)
            {
                SetProgressBarPct(_showProgressPct);
            }
            else
            {
                SetProgressBarPct(_endSeconds * 100.0 / _videoInfo.TotalSeconds);
            }

            labelTime.Text = ProgressHelper.ToProgressTime(msEstimatedLeft);
            labelTime.Refresh();
            BringToFront();
        }

        private void SetProgressBarPct(double pct)
        {
            var p = (int)Math.Round(pct, MidpointRounding.AwayFromZero);

            if (p > progressBar1.Maximum)
            {
                p = progressBar1.Maximum;
            }

            if (p < progressBar1.Minimum)
            {
                p = progressBar1.Minimum;
            }

            progressBar1.Value = p;
            TaskbarList.SetProgressValue(_parentForm.Handle, p, 100);
        }

        private void buttonDownload_Click(object sender, EventArgs e)
        {
            using (var form = new WhisperModelDownload { AutoClose = true })
            {
                form.ShowDialog(this);
                FillModels(comboBoxModels, form.LastDownloadedModel != null ? form.LastDownloadedModel.Name : string.Empty);
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

        private void AudioToText_Shown(object sender, EventArgs e)
        {
            buttonGenerate.Focus();
            _initialWidth = Width;

            TaskDelayHelper.RunDelayed(TimeSpan.FromMilliseconds(25), () =>
            {
                CheckIfInstalledAndVersion(Configuration.Settings.Tools.WhisperChoice);
            });
        }

        private bool _checkedInstalledAndVersion;
        private void CheckIfInstalledAndVersion(string whisperChoice)
        {
            if (_checkedInstalledAndVersion)
            {
                return;
            }

            _checkedInstalledAndVersion = true;

            if (whisperChoice == WhisperChoice.Cpp)
            {
                var targetFile = WhisperHelper.GetWhisperPathAndFileName(whisperChoice);
                if (File.Exists(targetFile))
                {
                    if (!Configuration.Settings.Tools.WhisperIgnoreVersion &&
                        WhisperDownload.IsOldVersion(targetFile, whisperChoice))
                    {
                        if (MessageBox.Show(string.Format(LanguageSettings.Current.Settings.DownloadX, "Whisper CPP (Update)"), "Subtitle Edit", MessageBoxButtons.YesNoCancel) == DialogResult.Yes)
                        {
                            using (var downloadForm = new WhisperDownload(whisperChoice))
                            {
                                if (downloadForm.ShowDialog(this) != DialogResult.OK)
                                {
                                    Configuration.Settings.Tools.WhisperIgnoreVersion = true;
                                    return;
                                }
                            }
                        }
                    }
                }
                else
                {
                    if (MessageBox.Show(string.Format(LanguageSettings.Current.Settings.DownloadX, "Whisper CPP"), "Subtitle Edit", MessageBoxButtons.YesNoCancel) == DialogResult.Yes)
                    {
                        using (var downloadForm = new WhisperDownload(whisperChoice))
                        {
                            if (downloadForm.ShowDialog(this) != DialogResult.OK)
                            {
                                return;
                            }
                        }
                    }
                }
            }
            else if (whisperChoice == WhisperChoice.CppCuBlas)
            {
                var targetFile = WhisperHelper.GetWhisperPathAndFileName(whisperChoice);
                if (File.Exists(targetFile))
                {
                    if (!Configuration.Settings.Tools.WhisperIgnoreVersion &&
                        WhisperDownload.IsOldVersion(targetFile, whisperChoice))
                    {
                        if (MessageBox.Show(string.Format(LanguageSettings.Current.Settings.DownloadX, "Whisper CPP cuBLASS (Update)"), "Subtitle Edit", MessageBoxButtons.YesNoCancel) == DialogResult.Yes)
                        {
                            using (var downloadForm = new WhisperDownload(whisperChoice))
                            {
                                if (downloadForm.ShowDialog(this) != DialogResult.OK)
                                {
                                    Configuration.Settings.Tools.WhisperIgnoreVersion = true;
                                    return;
                                }
                            }
                        }
                    }
                }
                else
                {
                    if (MessageBox.Show(string.Format(LanguageSettings.Current.Settings.DownloadX, "Whisper CPP cuBLASS"), "Subtitle Edit", MessageBoxButtons.YesNoCancel) == DialogResult.Yes)
                    {
                        using (var downloadForm = new WhisperDownload(whisperChoice))
                        {
                            if (downloadForm.ShowDialog(this) != DialogResult.OK)
                            {
                                return;
                            }
                        }
                    }
                }
            }
            else if (whisperChoice == WhisperChoice.PurfviewFasterWhisper ||
                     whisperChoice == WhisperChoice.PurfviewFasterWhisperXXL)
            {
                var targetFile = WhisperHelper.GetWhisperPathAndFileName(whisperChoice);
                if (File.Exists(targetFile))
                {
                    if (!Configuration.Settings.Tools.WhisperIgnoreVersion &&
                        WhisperDownload.IsOldVersion(targetFile, whisperChoice))
                    {
                        if (MessageBox.Show(string.Format(LanguageSettings.Current.Settings.DownloadX, whisperChoice + " (Update)"), "Subtitle Edit", MessageBoxButtons.YesNoCancel) == DialogResult.Yes)
                        {
                            using (var downloadForm = new WhisperDownload(whisperChoice))
                            {
                                if (downloadForm.ShowDialog(this) != DialogResult.OK)
                                {
                                    Configuration.Settings.Tools.WhisperIgnoreVersion = true;
                                    return;
                                }
                            }
                        }
                    }
                }
                else
                {
                    if (MessageBox.Show(string.Format(LanguageSettings.Current.Settings.DownloadX, whisperChoice), "Subtitle Edit", MessageBoxButtons.YesNoCancel) == DialogResult.Yes)
                    {
                        using (var downloadForm = new WhisperDownload(whisperChoice))
                        {
                            if (downloadForm.ShowDialog(this) != DialogResult.OK)
                            {
                                return;
                            }
                        }

                        if (whisperChoice == WhisperChoice.PurfviewFasterWhisper &&
                            !IsFasterWhisperCudaInstalled() && IsFasterWhisperCudaSupported())
                        {
                            DownloadCudaForWhisperFaster(this);
                        }
                    }
                }
            }
            else if (whisperChoice == WhisperChoice.ConstMe)
            {
                var targetFile = WhisperHelper.GetWhisperPathAndFileName(whisperChoice);
                if (File.Exists(targetFile))
                {
                    if (!Configuration.Settings.Tools.WhisperIgnoreVersion &&
                        WhisperDownload.IsOldVersion(targetFile, whisperChoice))
                    {
                        if (MessageBox.Show(string.Format(LanguageSettings.Current.Settings.DownloadX, "Whisper Const-me (Update)"), "Subtitle Edit", MessageBoxButtons.YesNoCancel) == DialogResult.Yes)
                        {
                            using (var downloadForm = new WhisperDownload(whisperChoice))
                            {
                                if (downloadForm.ShowDialog(this) != DialogResult.OK)
                                {
                                    Configuration.Settings.Tools.WhisperIgnoreVersion = true;
                                    return;
                                }
                            }
                        }
                    }
                }
                else
                {
                    if (MessageBox.Show(string.Format(LanguageSettings.Current.Settings.DownloadX, "Whisper Const-me"), "Subtitle Edit", MessageBoxButtons.YesNoCancel) == DialogResult.Yes)
                    {
                        using (var downloadForm = new WhisperDownload(whisperChoice))
                        {
                            if (downloadForm.ShowDialog(this) != DialogResult.OK)
                            {
                                return;
                            }
                        }
                    }
                }
            }
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

        private void comboBoxLanguages_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBoxLanguages.SelectedIndex > 0 && comboBoxLanguages.Text == LanguageSettings.Current.General.ChangeLanguageFilter)
            {
                using (var form = new DefaultLanguagesChooser(Configuration.Settings.General.DefaultLanguages))
                {
                    if (form.ShowDialog(this) == DialogResult.OK)
                    {
                        Configuration.Settings.General.DefaultLanguages = form.DefaultLanguages;
                    }
                }

                InitializeLanguageNames(comboBoxLanguages);
                return;
            }

            checkBoxTranslateToEnglish.Enabled = comboBoxLanguages.Text.ToLowerInvariant() != "english";
        }

        internal static void InitializeLanguageNames(NikseComboBox comboBox)
        {
            comboBox.Items.Clear();

            var languagesFilled = false;

            if (!string.IsNullOrEmpty(Configuration.Settings.General.DefaultLanguages))
            {
                var favorites = Utilities.GetSubtitleLanguageCultures(true).ToList();
                var languages = WhisperLanguage.Languages;
                var languagesToAdd = new List<WhisperLanguage>();

                foreach (var whisperLanguage in languages)
                {
                    if (favorites.Any(p => p.TwoLetterISOLanguageName == whisperLanguage.Code) ||
                        favorites.Any(p2 => p2.EnglishName.Contains(whisperLanguage.Name, StringComparison.OrdinalIgnoreCase)) ||
                        favorites.Any(p3 => whisperLanguage.Name.Contains(p3.EnglishName, StringComparison.OrdinalIgnoreCase)))
                    {
                        languagesFilled = true;
                        languagesToAdd.Add(whisperLanguage);
                    }
                }

                comboBox.Items.AddRange(languagesToAdd.OrderBy(p => p.Name).ToArray<object>());

                var lang = languages.FirstOrDefault(p => p.Code == Configuration.Settings.Tools.WhisperLanguageCode);
                comboBox.Text = lang != null ? lang.ToString() : "English";
            }

            if (!languagesFilled)
            {
                comboBox.Items.AddRange(WhisperLanguage.Languages.OrderBy(p => p.Name).ToArray<object>());
                var lang = WhisperLanguage.Languages.FirstOrDefault(p => p.Code == Configuration.Settings.Tools.WhisperLanguageCode);
                comboBox.Text = lang != null ? lang.ToString() : "English";
            }

            comboBox.Items.Add(LanguageSettings.Current.General.ChangeLanguageFilter);

            if (string.IsNullOrEmpty(comboBox.Text) && comboBox.Items.Count > 0)
            {
                comboBox.SelectedIndex = 0;
            }
        }

        private void WhisperPhpOriginalChoose()
        {
            Configuration.Settings.Tools.WhisperChoice = WhisperChoice.OpenAi;

            if (Configuration.IsRunningOnWindows)
            {
                var path = WhisperHelper.GetWhisperFolder();
                if (string.IsNullOrEmpty(path))
                {
                    using (var openFileDialog1 = new OpenFileDialog())
                    {
                        openFileDialog1.Title = "Locate whisper.exe (OpenAI Python version)";
                        openFileDialog1.FileName = string.Empty;
                        openFileDialog1.Filter = "whisper.exe|whisper.exe";

                        if (openFileDialog1.ShowDialog() != DialogResult.OK || !openFileDialog1.FileName.EndsWith("whisper.exe", StringComparison.OrdinalIgnoreCase))
                        {
                            Configuration.Settings.Tools.WhisperChoice = WhisperChoice.Cpp;
                            comboBoxWhisperEngine.Text = WhisperChoice.Cpp;
                        }
                        else
                        {
                            Configuration.Settings.Tools.WhisperLocation = openFileDialog1.FileName;
                        }
                    }
                }
            }

            Init();
        }

        private void removeTemporaryFilesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Configuration.Settings.Tools.WhisperDeleteTempFiles = !Configuration.Settings.Tools.WhisperDeleteTempFiles;
            removeTemporaryFilesToolStripMenuItem.Checked = Configuration.Settings.Tools.WhisperDeleteTempFiles;
        }

        private void whisperConstMeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var oldChoice = Configuration.Settings.Tools.WhisperChoice;
            Configuration.Settings.Tools.WhisperChoice = WhisperChoice.ConstMe;
            var fileName = WhisperHelper.GetWhisperPathAndFileName();
            if (!File.Exists(fileName) ||
                WhisperDownload.IsOld(fileName, WhisperChoice.ConstMe))
            {
                Configuration.Settings.Tools.WhisperChoice = oldChoice;
                if (MessageBox.Show(string.Format(LanguageSettings.Current.Settings.DownloadX, "whisper ConstMe (GPU)"), "Subtitle Edit", MessageBoxButtons.YesNoCancel) == DialogResult.Yes)
                {
                    using (var downloadForm = new WhisperDownload(WhisperChoice.ConstMe))
                    {
                        if (downloadForm.ShowDialog(this) == DialogResult.OK)
                        {
                            Configuration.Settings.Tools.WhisperChoice = WhisperChoice.ConstMe;
                        }
                        else
                        {
                            return;
                        }
                    }
                }
                else
                {
                    return;
                }
            }

            Init();
        }

        private void WhisperEngineCTranslate2()
        {
            Configuration.Settings.Tools.WhisperChoice = WhisperChoice.CTranslate2;

            if (Configuration.IsRunningOnWindows)
            {
                var path = WhisperHelper.GetWhisperFolder();
                if (string.IsNullOrEmpty(path))
                {
                    using (var openFileDialog1 = new OpenFileDialog())
                    {
                        openFileDialog1.Title = "Locate whisper-ctranslate2.exe (Python version)";
                        openFileDialog1.FileName = string.Empty;
                        openFileDialog1.Filter = "whisper-ctranslate2.exe|whisper-ctranslate2.exe";

                        if (openFileDialog1.ShowDialog() != DialogResult.OK || !openFileDialog1.FileName.EndsWith("whisper-ctranslate2.exe", StringComparison.OrdinalIgnoreCase))
                        {
                            Configuration.Settings.Tools.WhisperChoice = WhisperChoice.Cpp;
                            comboBoxWhisperEngine.Text = WhisperChoice.Cpp;
                        }
                        else
                        {
                            Configuration.Settings.Tools.WhisperCtranslate2Location = openFileDialog1.FileName;
                        }
                    }
                }
            }

            Init();
        }

        private void WhisperEnginePurfviewFasterWhisper(string whisperChoice)
        {
            var oldChoice = Configuration.Settings.Tools.WhisperChoice;
            Configuration.Settings.Tools.WhisperChoice = whisperChoice;
            var fileName = WhisperHelper.GetWhisperPathAndFileName();
            if (!File.Exists(fileName) || WhisperDownload.IsOld(fileName, WhisperChoice.PurfviewFasterWhisper))
            {
                Configuration.Settings.Tools.WhisperChoice = oldChoice;
                if (MessageBox.Show(string.Format(LanguageSettings.Current.Settings.DownloadX, whisperChoice), "Subtitle Edit", MessageBoxButtons.YesNoCancel) == DialogResult.Yes)
                {
                    using (var downloadForm = new WhisperDownload(WhisperChoice.PurfviewFasterWhisper))
                    {
                        if (downloadForm.ShowDialog(this) == DialogResult.OK)
                        {
                            Configuration.Settings.Tools.WhisperChoice = WhisperChoice.PurfviewFasterWhisper;
                        }
                        else
                        {
                            return;
                        }
                    }

                    if (!IsFasterWhisperCudaInstalled() && IsFasterWhisperCudaSupported())
                    {
                        DownloadCudaForWhisperFaster(this);
                    }
                }
                else
                {
                    return;
                }
            }

            Configuration.Settings.Tools.WhisperChoice = whisperChoice;
            Init();
        }

        private void WhisperEngineWhisperX()
        {
            Configuration.Settings.Tools.WhisperChoice = WhisperChoice.WhisperX;

            if (Configuration.IsRunningOnWindows)
            {
                var path = WhisperHelper.GetWhisperFolder();
                if (string.IsNullOrEmpty(path))
                {
                    using (var openFileDialog1 = new OpenFileDialog())
                    {
                        openFileDialog1.Title = "Locate whisperx.exe (Python version)";
                        openFileDialog1.FileName = string.Empty;
                        openFileDialog1.Filter = "whisperx.exe|whisperx.exe";

                        if (openFileDialog1.ShowDialog() != DialogResult.OK || !openFileDialog1.FileName.EndsWith("whisperx.exe", StringComparison.OrdinalIgnoreCase))
                        {
                            Configuration.Settings.Tools.WhisperChoice = WhisperChoice.Cpp;
                            comboBoxWhisperEngine.Text = WhisperChoice.Cpp;
                        }
                        else
                        {
                            Configuration.Settings.Tools.WhisperXLocation = openFileDialog1.FileName;
                        }
                    }
                }
            }

            Init();
        }

        private void WhisperEngineStableTs()
        {
            Configuration.Settings.Tools.WhisperChoice = WhisperChoice.StableTs;

            if (Configuration.IsRunningOnWindows)
            {
                var path = WhisperHelper.GetWhisperFolder();
                if (string.IsNullOrEmpty(path))
                {
                    using (var openFileDialog1 = new OpenFileDialog())
                    {
                        openFileDialog1.Title = "Locate stable-ts.exe (Python version)";
                        openFileDialog1.FileName = string.Empty;
                        openFileDialog1.Filter = "stable-ts.exe|stable-ts.exe";

                        if (openFileDialog1.ShowDialog() != DialogResult.OK
                            || !openFileDialog1.FileName.EndsWith("stable-ts.exe", StringComparison.OrdinalIgnoreCase))
                        {
                            Configuration.Settings.Tools.WhisperChoice = WhisperChoice.Cpp;
                            comboBoxWhisperEngine.Text = WhisperChoice.Cpp;
                        }
                        else
                        {
                            Configuration.Settings.Tools.WhisperStableTsLocation = openFileDialog1.FileName;
                        }
                    }
                }
            }

            Init();
        }

        private void comboBoxWhisperEngine_SelectedIndexChanged(object sender, EventArgs e)
        {
            FixPurfviewWhisperStandardArgument(labelAdvanced, comboBoxWhisperEngine.Text);

            if (comboBoxWhisperEngine.Text == Configuration.Settings.Tools.WhisperChoice)
            {
                return;
            }

            if (comboBoxWhisperEngine.Text == WhisperChoice.OpenAi)
            {
                WhisperPhpOriginalChoose();
            }
            else if (comboBoxWhisperEngine.Text == WhisperChoice.Cpp)
            {
                Configuration.Settings.Tools.WhisperChoice = WhisperChoice.Cpp;
                var fileName = WhisperHelper.GetWhisperPathAndFileName();
                if (!File.Exists(fileName) || WhisperDownload.IsOld(fileName, WhisperChoice.Cpp))
                {
                    if (MessageBox.Show(string.Format(LanguageSettings.Current.Settings.DownloadX, "Whisper CPP"), "Subtitle Edit", MessageBoxButtons.YesNoCancel) == DialogResult.Yes)
                    {
                        using (var downloadForm = new WhisperDownload(WhisperChoice.Cpp))
                        {
                            if (downloadForm.ShowDialog(this) != DialogResult.OK)
                            {
                                return;
                            }
                        }
                    }
                }

                Init();
            }
            else if (comboBoxWhisperEngine.Text == WhisperChoice.CppCuBlas)
            {
                Configuration.Settings.Tools.WhisperChoice = WhisperChoice.CppCuBlas;
                var fileName = WhisperHelper.GetWhisperPathAndFileName();
                if (!File.Exists(fileName) || WhisperDownload.IsOld(fileName, WhisperChoice.CppCuBlas))
                {
                    if (MessageBox.Show(string.Format(LanguageSettings.Current.Settings.DownloadX, "Whisper " + WhisperChoice.CppCuBlas), "Subtitle Edit", MessageBoxButtons.YesNoCancel) == DialogResult.Yes)
                    {
                        using (var downloadForm = new WhisperDownload(WhisperChoice.CppCuBlas))
                        {
                            if (downloadForm.ShowDialog(this) != DialogResult.OK)
                            {
                                return;
                            }
                        }
                    }
                }

                Init();
            }
            else if (comboBoxWhisperEngine.Text == WhisperChoice.ConstMe)
            {
                whisperConstMeToolStripMenuItem_Click(null, null);
            }
            else if (comboBoxWhisperEngine.Text == WhisperChoice.CTranslate2)
            {
                WhisperEngineCTranslate2();
            }
            else if (comboBoxWhisperEngine.Text == WhisperChoice.PurfviewFasterWhisper ||
                     comboBoxWhisperEngine.Text == WhisperChoice.PurfviewFasterWhisperXXL)
            {
                WhisperEnginePurfviewFasterWhisper(comboBoxWhisperEngine.Text);
            }
            else if (comboBoxWhisperEngine.Text == WhisperChoice.WhisperX)
            {
                WhisperEngineWhisperX();
            }
            else if (comboBoxWhisperEngine.Text == WhisperChoice.StableTs)
            {
                WhisperEngineStableTs();
            }
        }

        internal static void FixPurfviewWhisperStandardArgument(Label label, string engine)
        {
            if (engine != WhisperChoice.PurfviewFasterWhisper && Configuration.Settings.Tools.WhisperExtraSettings.Contains("--standard", StringComparison.Ordinal))
            {
                Configuration.Settings.Tools.WhisperExtraSettings = Configuration.Settings.Tools.WhisperExtraSettings.Replace("--standard", string.Empty).Trim();
            }
            else if (engine == WhisperChoice.PurfviewFasterWhisper &&
                     !Configuration.Settings.Tools.WhisperExtraSettings.Contains("--standard", StringComparison.Ordinal) &&
                     Configuration.Settings.Tools.WhisperPurfviewFasterWhisperDefaultCmd == "--standard")
            {
                Configuration.Settings.Tools.WhisperExtraSettings = Configuration.Settings.Tools.WhisperPurfviewFasterWhisperDefaultCmd;
            }

            label.Text = Configuration.Settings.Tools.WhisperExtraSettings;
        }

        private void setCPPConstMeModelsFolderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (var folderBrowserDialog1 = new FolderBrowserDialog())
            {
                if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
                {
                    Configuration.Settings.Tools.WhisperCppModelLocation = folderBrowserDialog1.SelectedPath;
                }
            }
        }

        private void contextMenuStripWhisperAdvanced_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            runOnlyPostProcessingToolStripMenuItem.Visible = buttonGenerate.Enabled;
            toolStripSeparatorRunOnlyPostprocessing.Visible = buttonGenerate.Enabled;

            if (Configuration.Settings.Tools.WhisperChoice == WhisperChoice.Cpp ||
                Configuration.Settings.Tools.WhisperChoice == WhisperChoice.ConstMe)
            {
                if (!string.IsNullOrEmpty(Configuration.Settings.Tools.WhisperCppModelLocation) &&
                    Directory.Exists(Configuration.Settings.Tools.WhisperCppModelLocation))
                {
                    setCPPConstmeModelsFolderToolStripMenuItem.Text = $"{LanguageSettings.Current.AudioToText.SetCppConstMeFolder} [{Configuration.Settings.Tools.WhisperCppModelLocation}]";
                }
                else
                {
                    setCPPConstmeModelsFolderToolStripMenuItem.Text = LanguageSettings.Current.AudioToText.SetCppConstMeFolder;
                }
                setCPPConstmeModelsFolderToolStripMenuItem.Visible = true;
            }
            else
            {
                setCPPConstmeModelsFolderToolStripMenuItem.Visible = false;
            }

            downloadCUDAForPurfviewsWhisperFasterToolStripMenuItem.Visible =
                buttonGenerate.Enabled && Configuration.Settings.Tools.WhisperChoice == WhisperChoice.PurfviewFasterWhisper;

            downloadNvidiaCudaForCPPCuBLASToolStripMenuItem.Visible =
                buttonGenerate.Enabled && Configuration.Settings.Tools.WhisperChoice == WhisperChoice.CppCuBlas;

            var whisperLogFile = SeLogger.GetWhisperLogFilePath();
            if (File.Exists(whisperLogFile))
            {
                showWhisperlogtxtToolStripMenuItem.Visible = true;
                showWhisperlogtxtToolStripMenuItem.Text = string.Format(LanguageSettings.Current.General.ViewX, $"\"{Path.GetFileName(whisperLogFile)}\"");
            }
            else
            {
                showWhisperlogtxtToolStripMenuItem.Visible = false;
            }
        }

        private void runOnlyPostProcessingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                labelProgress.Text = LanguageSettings.Current.AudioToText.PostProcessing;

                _languageCode = LanguageAutoDetect.AutoDetectGoogleLanguage(_subtitle);
                var postProcessor = new AudioToTextPostProcessor(_languageCode)
                {
                    ParagraphMaxChars = Configuration.Settings.General.SubtitleLineMaximumLength * 2,
                };

                WavePeakData wavePeaks = null;
                if (checkBoxAutoAdjustTimings.Checked)
                {
                    wavePeaks = _wavePeaks ?? MakeWavePeaks();
                }

                if (checkBoxAutoAdjustTimings.Checked && wavePeaks != null)
                {
                    _subtitle = WhisperTimingFixer.ShortenLongDuration(_subtitle);
                    _subtitle = WhisperTimingFixer.ShortenViaWavePeaks(_subtitle, wavePeaks);
                }
                else if (!checkBoxUsePostProcessing.Checked)
                {
                    return;
                }

                TranscribedSubtitle = postProcessor.Fix(AudioToTextPostProcessor.Engine.Whisper,
                    _subtitle,
                    checkBoxUsePostProcessing.Checked,
                    Configuration.Settings.Tools.WhisperPostProcessingAddPeriods,
                    Configuration.Settings.Tools.WhisperPostProcessingMergeLines,
                    Configuration.Settings.Tools.WhisperPostProcessingFixCasing,
                    Configuration.Settings.Tools.WhisperPostProcessingFixShortDuration,
                    Configuration.Settings.Tools.WhisperPostProcessingSplitLines);
                DialogResult = DialogResult.OK;
            }
            finally
            {
                buttonGenerate.Enabled = true;
                buttonDownload.Enabled = true;
                buttonBatchMode.Enabled = true;
                buttonAdvanced.Enabled = true;
                comboBoxLanguages.Enabled = true;
                comboBoxModels.Enabled = true;
                linkLabelPostProcessingConfigure.Enabled = true;
            }
        }

        private void buttonAdvanced_Click(object sender, EventArgs e)
        {
            using (var form = new WhisperAdvanced(comboBoxWhisperEngine.Text))
            {
                var res = form.ShowDialog(this);
                SetAdvancedLabel();
            }
        }

        private void SetAdvancedLabel()
        {
            labelAdvanced.Left = buttonAdvanced.Left;
            labelAdvanced.Text = Configuration.Settings.Tools.WhisperExtraSettings;

            if (labelAdvanced.Right > buttonAdvanced.Right + 5)
            {
                labelAdvanced.Left = buttonAdvanced.Right + 5 - labelAdvanced.Width;
            }
        }

        private void downloadCUDAForPurfviewsWhisperFasterToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DownloadCudaForWhisperFaster(this);
        }

        public static void DownloadCudaForWhisperFaster(IWin32Window owner)
        {
            if (MessageBox.Show(string.Format(LanguageSettings.Current.Settings.DownloadX, "cuBLAS and cuDNN libs"), "Subtitle Edit", MessageBoxButtons.YesNoCancel) != DialogResult.Yes)
            {
                return;
            }

            var alreadyInstalled = IsFasterWhisperCudaInstalled();
            if (alreadyInstalled)
            {
                if (MessageBox.Show("CUDA is probably already installed - reinstall?", "Subtitle Edit", MessageBoxButtons.YesNoCancel) != DialogResult.Yes)
                {
                    return;
                }
            }

            var downloadTarget = WhisperChoice.PurfviewFasterWhisperCuda;
            if (Configuration.Settings.Tools.WhisperChoice == WhisperChoice.CppCuBlas)
            {
                downloadTarget = WhisperChoice.CppCuBlasLib;
            }

            using (var downloadForm = new WhisperDownload(downloadTarget))
            {
                if (downloadForm.ShowDialog(owner) == DialogResult.OK)
                {
                }
            }
        }

        public static bool IsFasterWhisperCudaSupported()
        {
            var w = WhisperHelper.GetWhisperPathAndFileName(WhisperChoice.PurfviewFasterWhisper);
            var process = new Process { StartInfo = new ProcessStartInfo(w, "--checkcuda") { WindowStyle = ProcessWindowStyle.Hidden, CreateNoWindow = true } };
            var whisperFolder = WhisperHelper.GetWhisperFolder(WhisperChoice.PurfviewFasterWhisper);
            if (!string.IsNullOrEmpty(whisperFolder))
            {
                process.StartInfo.WorkingDirectory = whisperFolder;
            }

            process.StartInfo.StandardOutputEncoding = Encoding.UTF8;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.OutputDataReceived += OutputHandlerCheckCuda;
            process.ErrorDataReceived += OutputHandlerCheckCuda;

            process.Start();

            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            while (!process.HasExited)
            {
                Application.DoEvents();
            }

            for (var i = 0; i < 100; i++)
            {
                Application.DoEvents();
                System.Threading.Thread.Sleep(2);
            }

            process.Dispose();

            return CudaSomeDevice == true;
        }

        private static void OutputHandlerCheckCuda(object sendingProcess, DataReceivedEventArgs outLine)
        {
            if (string.IsNullOrEmpty(outLine.Data))
            {
                return;
            }

            SeLogger.WhisperInfo("CUDA check reports: " + outLine.Data);

            if (!outLine.Data.Contains("CUDA device: 0") && outLine.Data.Contains("CUDA device:"))
            {
                CudaSomeDevice = true;
            }
        }

        public static bool IsFasterWhisperCudaInstalled()
        {
            var folder = Path.Combine(Configuration.DataDirectory, "Whisper", "Purfview-Whisper-Faster");
            if (Configuration.Settings.Tools.WhisperChoice == WhisperChoice.CppCuBlas)
            {
                folder = Path.Combine(Configuration.DataDirectory, "Whisper", WhisperChoice.CppCuBlas);
            }

            if (!Directory.Exists(folder))
            {
                return false;
            }

            var cudaFiles = Directory.GetFiles(folder, "cu*.dll");
            var alreadyInstalled = cudaFiles.Length > 2;
            return alreadyInstalled;
        }

        private void ShowWhisperLogFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            UiUtil.OpenFile(SeLogger.GetWhisperLogFilePath());
        }

        private void downloadNvidiaCudaForCPPCuBLASToolStripMenuItem_Click(object sender, EventArgs e)
        {
            UiUtil.OpenUrl("https://developer.nvidia.com/cuda-downloads");
        }

        private void linkLabelPostProcessingConfigure_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            ShowPostProcessingSettings(this);
        }

        public static void ShowPostProcessingSettings(Form owner)
        {
            using (var form = new PostProcessingSettings()
            {
                AddPeriods = Configuration.Settings.Tools.WhisperPostProcessingAddPeriods,
                MergeLines = Configuration.Settings.Tools.WhisperPostProcessingMergeLines,
                SplitLines = Configuration.Settings.Tools.WhisperPostProcessingSplitLines,
                FixCasing = Configuration.Settings.Tools.WhisperPostProcessingFixCasing,
                FixShortDuration = Configuration.Settings.Tools.WhisperPostProcessingFixShortDuration,
            })
            {
                if (form.ShowDialog(owner) == DialogResult.OK)
                {
                    Configuration.Settings.Tools.WhisperPostProcessingAddPeriods = form.AddPeriods;
                    Configuration.Settings.Tools.WhisperPostProcessingMergeLines = form.MergeLines;
                    Configuration.Settings.Tools.WhisperPostProcessingSplitLines = form.SplitLines;
                    Configuration.Settings.Tools.WhisperPostProcessingFixCasing = form.FixCasing;
                    Configuration.Settings.Tools.WhisperPostProcessingFixShortDuration = form.FixShortDuration;
                }
            }
        }
    }
}
