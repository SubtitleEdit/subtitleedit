using Nikse.SubtitleEdit.Core.AudioToText;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Forms.Options;
using Nikse.SubtitleEdit.Logic;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using MessageBox = Nikse.SubtitleEdit.Forms.SeMsgBox.MessageBox;

namespace Nikse.SubtitleEdit.Forms.AudioToText
{
    public sealed partial class WhisperAudioToTextSelectedLines : Form
    {
        private bool _cancel;
        private int _batchFileNumber;
        private readonly List<AudioClipsGet.AudioClip> _audioClips;
        private readonly Form _parentForm;
        private readonly List<string> _filesToDelete;
        private readonly Regex _timeRegex = new Regex(@"^\[\d\d:\d\d[\.,]\d\d\d --> \d\d:\d\d[\.,]\d\d\d\]", RegexOptions.Compiled);
        private List<ResultText> _resultList;
        private string _languageCode;
        private ConcurrentBag<string> _outputText = new ConcurrentBag<string>();

        public Subtitle TranscribedSubtitle { get; private set; }

        public bool UnknownArgument { get; set; }
        public bool RunningOnCuda { get; set; }
        public bool IncompleteModel { get; set; }
        public string IncompleteModelName { get; set; }


        public WhisperAudioToTextSelectedLines(List<AudioClipsGet.AudioClip> audioClips, Form parentForm)
        {
            UiUtil.PreInitialize(this);
            InitializeComponent();
            UiUtil.FixFonts(this);
            UiUtil.FixLargeFonts(this, buttonGenerate);
            _parentForm = parentForm;
            _filesToDelete = new List<string>();

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
            buttonGenerate.Text = LanguageSettings.Current.Watermark.Generate;
            buttonCancel.Text = LanguageSettings.Current.General.Cancel;
            groupBoxInputFiles.Text = LanguageSettings.Current.BatchConvert.Input;
            linkLabeWhisperWebSite.Text = LanguageSettings.Current.AudioToText.WhisperWebsite;
            buttonAdvanced.Text = LanguageSettings.Current.General.Advanced;
            labelAdvanced.Text = Configuration.Settings.Tools.WhisperExtraSettings;
            columnHeaderFileName.Text = LanguageSettings.Current.JoinSubtitles.FileName;
            checkBoxUsePostProcessing.Checked = Configuration.Settings.Tools.VoskPostProcessing;

            Init();

            textBoxLog.Visible = false;
            textBoxLog.Dock = DockStyle.Fill;
            labelProgress.Text = string.Empty;
            labelTime.Text = string.Empty;
            listViewInputFiles.Visible = true;
            _audioClips = audioClips;
            progressBar1.Maximum = 100;
            foreach (var audioClip in audioClips)
            {
                listViewInputFiles.Items.Add(audioClip.AudioFileName);
            }

            WhisperAudioToText.InitializeWhisperEngines(comboBoxWhisperEngine);
            WhisperAudioToText.FixPurfviewWhisperStandardArgument(labelAdvanced, comboBoxWhisperEngine.Text);
        }

        private void Init()
        {
            WhisperAudioToText.InitializeLanguageNames(comboBoxLanguages);

            WhisperAudioToText.FillModels(comboBoxModels, string.Empty);

            removeTemporaryFilesToolStripMenuItem.Checked = Configuration.Settings.Tools.WhisperDeleteTempFiles;

            ContextMenuStrip = contextMenuStripWhisperAdvanced;
        }

        private void ButtonGenerate_Click(object sender, EventArgs e)
        {
            if (comboBoxModels.Items.Count == 0)
            {
                buttonDownload_Click(null, null);
                return;
            }

            if (listViewInputFiles.Items.Count == 0)
            {
                return;
            }

            GenerateBatch();
            TaskbarList.SetProgressState(_parentForm.Handle, TaskbarButtonProgressFlags.NoProgress);
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
            _languageCode = WhisperAudioToText.GetLanguage(comboBoxLanguages.Text);
            groupBoxInputFiles.Enabled = false;
            comboBoxLanguages.Enabled = false;
            comboBoxModels.Enabled = false;
            linkLabelPostProcessingConfigure.Enabled = false;
            _batchFileNumber = 0;
            var postProcessor = new AudioToTextPostProcessor(_languageCode)
            {
                ParagraphMaxChars = Configuration.Settings.General.SubtitleLineMaximumLength * 2,
            };
            _outputText.Add("Batch mode");
            timer1.Start();
            ShowProgressBar();
            foreach (ListViewItem lvi in listViewInputFiles.Items)
            {
                var pct = _batchFileNumber * 100.0 / listViewInputFiles.Items.Count;
                progressBar1.Value = (int)Math.Round(pct, MidpointRounding.AwayFromZero);
                progressBar1.Refresh();
                _batchFileNumber++;
                var videoFileName = lvi.Text;
                listViewInputFiles.SelectedIndices.Clear();
                lvi.Selected = true;
                buttonGenerate.Enabled = false;
                buttonDownload.Enabled = false;
                comboBoxModels.Enabled = false;
                linkLabelPostProcessingConfigure.Enabled = false;
                comboBoxLanguages.Enabled = false;
                var waveFileName = videoFileName;

                _outputText.Add(string.Empty);
                var transcript = TranscribeViaWhisper(waveFileName, videoFileName);
                if (_cancel)
                {
                    TaskbarList.SetProgressState(_parentForm.Handle, TaskbarButtonProgressFlags.NoProgress);
                    groupBoxInputFiles.Enabled = true;
                    return;
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

                SaveToAudioClip(_batchFileNumber - 1);
                TaskbarList.SetProgressValue(_parentForm.Handle, _batchFileNumber, listViewInputFiles.Items.Count);
            }

            timer1.Stop();
            progressBar1.Value = 100;
            labelTime.Text = string.Empty;
            PostFix(postProcessor);

            DialogResult = DialogResult.OK;
        }

        public List<ResultText> TranscribeViaWhisper(string waveFileName, string videoFileName)
        {
            var model = comboBoxModels.Items[comboBoxModels.SelectedIndex] as WhisperModel;
            if (model == null)
            {
                return new List<ResultText>();
            }

            labelProgress.Text = string.Format(LanguageSettings.Current.AudioToText.TranscribingXOfY, _batchFileNumber, listViewInputFiles.Items.Count);
            labelProgress.Refresh();
            Application.DoEvents();
            _resultList = new List<ResultText>();
            var process = WhisperAudioToText.GetWhisperProcess(waveFileName, model.Name, _languageCode, checkBoxTranslateToEnglish.Checked, OutputHandler);
            var sw = Stopwatch.StartNew();
            _outputText.Add($"Calling whisper ({Configuration.Settings.Tools.WhisperChoice}) with : whisper {process.StartInfo.Arguments}{Environment.NewLine}");
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

            while (!process.HasExited)
            {
                Application.DoEvents();
                System.Threading.Thread.Sleep(100);
                WindowsHelper.PreventStandBy();

                if (_cancel)
                {
                    process.Kill();
                    progressBar1.Visible = false;
                    buttonCancel.Visible = false;

                    if (!textBoxLog.Visible)
                    {
                        DialogResult = DialogResult.Cancel;
                        progressBar1.Hide();
                    }

                    return null;
                }
            }

            _outputText.Add($"Calling whisper ({Configuration.Settings.Tools.WhisperChoice} done in {sw.Elapsed}{Environment.NewLine}");

            for (var i = 0; i < 10; i++)
            {
                Application.DoEvents();
                System.Threading.Thread.Sleep(50);
            }

            if (WhisperAudioToText.GetResultFromSrt(waveFileName, videoFileName, out var resultTexts, _outputText, null))
            {
                return resultTexts;
            }

            return _resultList;
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
                if (_timeRegex.IsMatch(line))
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

                    _resultList.Add(rt);
                }
            }
        }

        private static decimal GetSeconds(string timeCode)
        {
            return (decimal)(TimeCode.ParseToMilliseconds(timeCode) / 1000.0);
        }

        private void PostFix(AudioToTextPostProcessor postProcessor)
        {
            var postSub = new Subtitle();
            foreach (var audioClip in _audioClips)
            {
                postSub.Paragraphs.Add(audioClip.Paragraph);
            }

            var postSubFixed = postProcessor.Fix(
                postSub,
                checkBoxUsePostProcessing.Checked,
                Configuration.Settings.Tools.WhisperPostProcessingAddPeriods,
                Configuration.Settings.Tools.WhisperPostProcessingMergeLines,
                Configuration.Settings.Tools.WhisperPostProcessingFixCasing,
                Configuration.Settings.Tools.WhisperPostProcessingFixShortDuration,
                Configuration.Settings.Tools.WhisperPostProcessingSplitLines,
                AudioToTextPostProcessor.Engine.Whisper);

            for (var index = 0; index < _audioClips.Count; index++)
            {
                var audioClip = _audioClips[index];
                if (index < postSubFixed.Paragraphs.Count)
                {
                    audioClip.Paragraph.Text = postSubFixed.Paragraphs[index].Text;
                }
            }
        }

        private void SaveToAudioClip(int index)
        {
            var audioClip = _audioClips[index];

            var sb = new StringBuilder();
            foreach (var p in TranscribedSubtitle.Paragraphs)
            {
                sb.AppendLine(p.Text);
            }

            audioClip.Paragraph.Text = sb.ToString().Trim();

            try
            {
                File.Delete(audioClip.AudioFileName);
            }
            catch
            {
                // ignore
            }
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

        private void linkLabelWhisperWebsite_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            UiUtil.OpenUrl(WhisperHelper.GetWebSiteUrl());
        }

        private void AudioToText_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (comboBoxModels.SelectedItem is WhisperModel model)
            {
                Configuration.Settings.Tools.WhisperModel = model.Name;
            }

            if (comboBoxLanguages.SelectedItem is WhisperLanguage language)
            {
                Configuration.Settings.Tools.WhisperLanguageCode = language.Code;
            }

            WhisperAudioToText.DeleteTemporaryFiles(_filesToDelete);
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

        private void UpdateLog()
        {
            if (_outputText.IsEmpty)
            {
                return;
            }

            textBoxLog.AppendText(string.Join(Environment.NewLine, _outputText) + Environment.NewLine);
            _outputText = new ConcurrentBag<string>();
        }

        private void linkLabelOpenModelFolder_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            UiUtil.OpenFolder(WhisperHelper.GetWhisperModel().ModelFolder);
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            UpdateLog();
        }

        private void buttonDownload_Click(object sender, EventArgs e)
        {
            using (var form = new WhisperModelDownload { AutoClose = true })
            {
                form.ShowDialog(this);
                WhisperAudioToText.FillModels(comboBoxModels, form.LastDownloadedModel.Name);
            }
        }

        private void ShowHideBatchMode()
        {
            Height = checkBoxUsePostProcessing.Bottom + progressBar1.Height + buttonCancel.Height + 450;
            listViewInputFiles.Visible = true;
        }

        private void AudioToText_Load(object sender, EventArgs e)
        {
            ShowHideBatchMode();
            listViewInputFiles.Columns[0].Width = -2;
        }

        private void AudioToTextSelectedLines_Shown(object sender, EventArgs e)
        {
            buttonGenerate.Focus();
        }

        private void AudioToTextSelectedLines_ResizeEnd(object sender, EventArgs e)
        {
            listViewInputFiles.AutoSizeLastColumn();
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

                WhisperAudioToText.InitializeLanguageNames(comboBoxLanguages);
                return;
            }

            checkBoxTranslateToEnglish.Enabled = comboBoxLanguages.Text.ToLowerInvariant() != "english";
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

        private void WhisperEnginePurfviewFasterWhisper()
        {
            var oldChoice = Configuration.Settings.Tools.WhisperChoice;
            Configuration.Settings.Tools.WhisperChoice = WhisperChoice.PurfviewFasterWhisper;
            var fileName = WhisperHelper.GetWhisperPathAndFileName();
            if (!File.Exists(fileName) || WhisperDownload.IsOld(fileName, WhisperChoice.PurfviewFasterWhisper))
            {
                Configuration.Settings.Tools.WhisperChoice = oldChoice;
                if (MessageBox.Show(string.Format(LanguageSettings.Current.Settings.DownloadX, "Purfview Faster-Whisper"), "Subtitle Edit", MessageBoxButtons.YesNoCancel) == DialogResult.Yes)
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
                }
                else
                {
                    return;
                }
            }

            Configuration.Settings.Tools.WhisperChoice = WhisperChoice.PurfviewFasterWhisper;
            Init();
        }

        private void comboBoxWhisperEngine_SelectedIndexChanged(object sender, EventArgs e)
        {
            WhisperAudioToText.FixPurfviewWhisperStandardArgument(labelAdvanced, comboBoxWhisperEngine.Text);

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
            else if (comboBoxWhisperEngine.Text == WhisperChoice.PurfviewFasterWhisper)
            {
                WhisperEnginePurfviewFasterWhisper();
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
            if (!string.IsNullOrEmpty(Configuration.Settings.Tools.WhisperCppModelLocation) &&
                Directory.Exists(Configuration.Settings.Tools.WhisperCppModelLocation))
            {
                setCPPConstmeModelsFolderToolStripMenuItem.Text = $"Set CPP/Const-me models folder... [{Configuration.Settings.Tools.WhisperCppModelLocation}]";
            }
            else
            {
                setCPPConstmeModelsFolderToolStripMenuItem.Text = "Set CPP/Const-me models folder...";
            }
        }

        private void buttonAdvanced_Click(object sender, EventArgs e)
        {
            using (var form = new WhisperAdvanced(comboBoxWhisperEngine.Text))
            {
                var res = form.ShowDialog(this);
                labelAdvanced.Text = Configuration.Settings.Tools.WhisperExtraSettings;
            }
        }

        private void downloadCUDAForPurfviewsWhisperFasterToolStripMenuItem_Click(object sender, EventArgs e)
        {
            WhisperAudioToText.DownloadCudaForWhisperFaster(this);
        }

        private void linkLabelPostProcessingConfigure_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            WhisperAudioToText.ShowPostProcessingSettings(this);
        }
    }
}
