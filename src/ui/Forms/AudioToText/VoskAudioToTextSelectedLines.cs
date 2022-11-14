using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Nikse.SubtitleEdit.Core.AudioToText;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Logic;
using Vosk;

namespace Nikse.SubtitleEdit.Forms.AudioToText
{
    public sealed partial class VoskAudioToTextSelectedLines : Form
    {
        private readonly string _voskFolder;
        private bool _cancel;
        private int _batchFileNumber;
        private long _startTicks;
        private long _bytesWavTotal;
        private long _bytesWavRead;
        private readonly List<AudioClipsGet.AudioClip> _audioClips;
        private readonly Form _parentForm;
        private Model _model;

        public Subtitle TranscribedSubtitle { get; private set; }

        public VoskAudioToTextSelectedLines(List<AudioClipsGet.AudioClip> audioClips, Form parentForm)
        {
            UiUtil.PreInitialize(this);
            InitializeComponent();
            UiUtil.FixFonts(this);
            UiUtil.FixLargeFonts(this, buttonGenerate);
            _parentForm = parentForm;

            Text = LanguageSettings.Current.AudioToText.Title;
            labelInfo.Text = LanguageSettings.Current.AudioToText.Info;
            groupBoxModels.Text = LanguageSettings.Current.AudioToText.Models;
            labelModel.Text = LanguageSettings.Current.AudioToText.ChooseModel;
            linkLabelOpenModelsFolder.Text = LanguageSettings.Current.AudioToText.OpenModelsFolder;
            checkBoxUsePostProcessing.Text = LanguageSettings.Current.AudioToText.UsePostProcessing;
            buttonGenerate.Text = LanguageSettings.Current.Watermark.Generate;
            buttonCancel.Text = LanguageSettings.Current.General.Cancel;
            groupBoxInputFiles.Text = LanguageSettings.Current.BatchConvert.Input;

            columnHeaderFileName.Text = LanguageSettings.Current.JoinSubtitles.FileName;

            checkBoxUsePostProcessing.Checked = Configuration.Settings.Tools.VoskPostProcessing;
            _voskFolder = Path.Combine(Configuration.DataDirectory, "Vosk");
            VoskAudioToText.FillModels(comboBoxModels, string.Empty);

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
            groupBoxInputFiles.Enabled = false;
            _batchFileNumber = 0;
            textBoxLog.AppendText("Batch mode" + Environment.NewLine);
            var postProcessor = new AudioToTextPostProcessor(GetLanguage(comboBoxModels.Text))
            {
                ParagraphMaxChars = Configuration.Settings.General.SubtitleLineMaximumLength * 2,
            };

            ShowProgressBar();
            foreach (ListViewItem lvi in listViewInputFiles.Items)
            {
                _batchFileNumber++;
                var videoFileName = lvi.Text;
                listViewInputFiles.SelectedIndices.Clear();
                lvi.Selected = true;
                var modelFileName = Path.Combine(_voskFolder, comboBoxModels.Text);
                buttonGenerate.Enabled = false;
                buttonDownload.Enabled = false;
                comboBoxModels.Enabled = false;
                var waveFileName = videoFileName;
                textBoxLog.AppendText("Wav file name: " + waveFileName + Environment.NewLine);
                var transcript = TranscribeViaVosk(waveFileName, modelFileName);
                if (_cancel)
                {
                    TaskbarList.SetProgressState(_parentForm.Handle, TaskbarButtonProgressFlags.NoProgress);
                    if (!textBoxLog.Visible)
                    {
                        DialogResult = DialogResult.Cancel;
                        progressBar1.Hide();
                    }

                    return;
                }

                TranscribedSubtitle = postProcessor.Generate(AudioToTextPostProcessor.Engine.Vosk, transcript, checkBoxUsePostProcessing.Checked, false, true, false, false, false);

                progressBar1.Value = (int)Math.Round(_batchFileNumber * 100.0 / _audioClips.Count, MidpointRounding.AwayFromZero);
                progressBar1.Refresh();
                Application.DoEvents();

                SaveToAudioClip(_batchFileNumber - 1);

                TaskbarList.SetProgressValue(_parentForm.Handle, _batchFileNumber, listViewInputFiles.Items.Count);
            }

            progressBar1.Value = 100;
            labelTime.Text = string.Empty;
            PostFix(postProcessor);

            DialogResult = DialogResult.OK;
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
            Directory.SetCurrentDirectory(_voskFolder);
            Vosk.Vosk.SetLogLevel(0);
            if (_model == null)
            {
                labelProgress.Text = LanguageSettings.Current.AudioToText.LoadingVoskModel;
                labelProgress.Refresh();
                Application.DoEvents();
                _model = new Model(modelFileName);
            }
            var rec = new VoskRecognizer(_model, 16000.0f);
            rec.SetMaxAlternatives(0);
            rec.SetWords(true);
            var list = new List<ResultText>();
            labelProgress.Text = LanguageSettings.Current.AudioToText.Transcribing;
            labelProgress.Text = string.Format(LanguageSettings.Current.AudioToText.TranscribingXOfY, _batchFileNumber, listViewInputFiles.Items.Count);
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
                    if (rec.AcceptWaveform(buffer, bytesRead))
                    {
                        var res = rec.Result();
                        var results = VoskAudioToText.ParseJsonToResult(res);
                        list.AddRange(results);
                    }
                    else
                    {
                        var res = rec.PartialResult();
                        textBoxLog.AppendText(res.RemoveChar('\r', '\n'));
                    }

                    if (_cancel)
                    {
                        TaskbarList.SetProgressState(_parentForm.Handle, TaskbarButtonProgressFlags.NoProgress);
                        return null;
                    }
                }
            }

            var finalResult = rec.FinalResult();
            var finalResults = VoskAudioToText.ParseJsonToResult(finalResult);
            list.AddRange(finalResults);
            timer1.Stop();
            return list;
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
                linkLabelVoskWebsite_LinkClicked(null, null);
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
            var estimatedLeft = ToProgressTime(estimatedTotalMs - durationMs);
            labelTime.Text = estimatedLeft;
        }

        public static string ToProgressTime(float estimatedTotalMs)
        {
            var timeCode = new TimeCode(estimatedTotalMs);
            if (timeCode.TotalSeconds < 60)
            {
                return string.Format(LanguageSettings.Current.GenerateVideoWithBurnedInSubs.TimeRemainingSeconds, (int)Math.Round(timeCode.TotalSeconds));
            }

            if (timeCode.TotalSeconds / 60 > 5)
            {
                return string.Format(LanguageSettings.Current.GenerateVideoWithBurnedInSubs.TimeRemainingMinutes, (int)Math.Round(timeCode.TotalSeconds / 60));
            }

            return string.Format(LanguageSettings.Current.GenerateVideoWithBurnedInSubs.TimeRemainingMinutesAndSeconds, timeCode.Minutes + timeCode.Hours * 60, timeCode.Seconds);
        }

        private void buttonDownload_Click(object sender, EventArgs e)
        {
            using (var form = new VoskModelDownload { AutoClose = true })
            {
                form.ShowDialog(this);
                VoskAudioToText.FillModels(comboBoxModels, form.LastDownloadedModel);
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

        private void comboBoxModels_SelectedIndexChanged(object sender, EventArgs e)
        {
            _model = null;
        }

        private void AudioToTextSelectedLines_Shown(object sender, EventArgs e)
        {
            buttonGenerate.Focus();
        }

        private void AudioToTextSelectedLines_ResizeEnd(object sender, EventArgs e)
        {
            listViewInputFiles.AutoSizeLastColumn();
        }
    }
}
