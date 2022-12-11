using Nikse.SubtitleEdit.Core.AudioToText;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Logic;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

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
            buttonGenerate.Text = LanguageSettings.Current.Watermark.Generate;
            buttonCancel.Text = LanguageSettings.Current.General.Cancel;
            groupBoxInputFiles.Text = LanguageSettings.Current.BatchConvert.Input;
            linkLabeWhisperWebSite.Text = LanguageSettings.Current.AudioToText.WhisperWebsite;

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
            labelCpp.Visible = Configuration.Settings.Tools.WhisperUseCpp;
            foreach (var audioClip in audioClips)
            {
                listViewInputFiles.Items.Add(audioClip.AudioFileName);
            }
        }

        private void Init()
        {
            comboBoxLanguages.Items.Clear();
            comboBoxLanguages.Items.AddRange(WhisperLanguage.Languages.OrderBy(p => p.Name).ToArray<object>());
            var lang = WhisperLanguage.Languages.FirstOrDefault(p => p.Code == Configuration.Settings.Tools.WhisperLanguageCode);
            comboBoxLanguages.Text = lang != null ? lang.ToString() : "English";
            WhisperAudioToText.FillModels(comboBoxModels, string.Empty);

            whisperCppCToolStripMenuItem.Checked = Configuration.Settings.Tools.WhisperUseCpp;
            whisperPhpOriginalToolStripMenuItem.Checked = !Configuration.Settings.Tools.WhisperUseCpp;
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
                comboBoxLanguages.Enabled = false;
                var waveFileName = videoFileName;

                _outputText.Add(string.Empty);
                var transcript = TranscribeViaWhisper(waveFileName);
                if (_cancel)
                {
                    TaskbarList.SetProgressState(_parentForm.Handle, TaskbarButtonProgressFlags.NoProgress);
                    groupBoxInputFiles.Enabled = true;
                    return;
                }

                TranscribedSubtitle = postProcessor.Generate(AudioToTextPostProcessor.Engine.Whisper, transcript, checkBoxUsePostProcessing.Checked, true, true, true, true, false);

                SaveToAudioClip(_batchFileNumber - 1);
                TaskbarList.SetProgressValue(_parentForm.Handle, _batchFileNumber, listViewInputFiles.Items.Count);
            }

            timer1.Stop();
            progressBar1.Value = 100;
            labelTime.Text = string.Empty;
            PostFix(postProcessor);

            DialogResult = DialogResult.OK;
        }

        public List<ResultText> TranscribeViaWhisper(string waveFileName)
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
            _outputText.Add($"Calling whisper{(Configuration.Settings.Tools.WhisperUseCpp ? "-CPP" : string.Empty)} with : whisper {process.StartInfo.Arguments}");
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

                Invalidate();
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

            _outputText.Add($"Calling whisper{(Configuration.Settings.Tools.WhisperUseCpp ? "-CPP" : string.Empty)} done in {sw.Elapsed}{Environment.NewLine}");

            for (var i = 0; i < 10; i++)
            {
                Application.DoEvents();
                System.Threading.Thread.Sleep(50);
            }

            if (WhisperAudioToText.GetResultFromSrt(waveFileName, out var resultTexts, _outputText, null))
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

            var postSubFixed = postProcessor.Generate(postSub, checkBoxUsePostProcessing.Checked, true, false, true, false, false);
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
                linkLabelWhisperWebsite_LinkClicked(null, null);
                e.SuppressKeyPress = true;
            }
            else if (e.KeyData == UiUtil.HelpKeys)
            {
                UiUtil.ShowHelp("#audio_to_text");
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
            checkBoxTranslateToEnglish.Enabled = comboBoxLanguages.Text.ToLowerInvariant() != "english";
        }

        private void whisperPhpOriginalToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Configuration.Settings.Tools.WhisperUseCpp = false;

            if (Configuration.IsRunningOnWindows)
            {
                var path = WhisperHelper.GetWhisperFolder();
                if (string.IsNullOrEmpty(path))
                {
                    using (var openFileDialog1 = new OpenFileDialog())
                    {
                        openFileDialog1.Title = "Locate whisper.exe (OpenAI php version)";
                        openFileDialog1.FileName = string.Empty;
                        openFileDialog1.Filter = "whisper.exe|whisper.exe";

                        if (openFileDialog1.ShowDialog() != DialogResult.OK || !openFileDialog1.FileName.EndsWith("whisper.exe", StringComparison.OrdinalIgnoreCase))
                        {
                            Configuration.Settings.Tools.WhisperUseCpp = true;
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

        private void whisperCppCToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Configuration.Settings.Tools.WhisperUseCpp = true;
            Init();
        }

        private void removeTemporaryFilesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Configuration.Settings.Tools.WhisperDeleteTempFiles = !Configuration.Settings.Tools.WhisperDeleteTempFiles;
            removeTemporaryFilesToolStripMenuItem.Checked = Configuration.Settings.Tools.WhisperDeleteTempFiles;
        }
    }
}
