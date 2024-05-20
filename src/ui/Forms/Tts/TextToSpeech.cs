using Nikse.SubtitleEdit.Controls;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Core.TextToSpeech;
using Nikse.SubtitleEdit.Logic;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using MessageBox = Nikse.SubtitleEdit.Forms.SeMsgBox.MessageBox;

namespace Nikse.SubtitleEdit.Forms.Tts
{
    public sealed partial class TextToSpeech : Form
    {
        public Subtitle EditedSubtitle { get; set; }

        private readonly Subtitle _subtitle;
        private readonly string _videoFileName;
        private readonly VideoInfo _videoInfo;
        private string _waveFolder;
        private readonly List<ActorAndVoice> _actorAndVoices;
        private readonly SubtitleFormat _subtitleFormat;
        private bool _abort;
        private readonly List<string> _actors;
        private readonly List<TextToSpeechEngine> _engines;
        private readonly List<PiperModel> _piperVoices;
        private readonly List<ElevenLabModel> _elevenLabVoices;
        private readonly List<AzureVoiceModel> _azureVoices;
        private bool _actorsOn;
        private bool _converting;

        public class ActorAndVoice
        {
            public string Actor { get; set; }
            public int UseCount { get; set; }
            public string Voice { get; set; }
            public int VoiceIndex { get; set; }
        }

        public class FileNameAndSpeedFactor
        {
            public string Filename { get; set; }
            public decimal Factor { get; set; }
        }

        public class TextToSpeechEngine
        {
            public TextToSpeechEngineId Id { get; set; }
            public string Name { get; set; }
            public int Index { get; set; }

            public TextToSpeechEngine(TextToSpeechEngineId id, string name, int index)
            {
                Id = id;
                Name = name;
                Index = index;
            }
        }

        public class AzureVoiceModel
        {
            public string DisplayName { get; set; }
            public string LocalName { get; set; }
            public string ShortName { get; set; }
            public string Gender { get; set; }
            public string Locale { get; set; }

            public override string ToString()
            {
                return $"{Locale} - {DisplayName} ({Gender})";
            }
        }

        public enum TextToSpeechEngineId
        {
            Piper,
            Tortoise,
            Coqui,
            MsSpeechSynthesizer,
            ElevenLabs,
            AzureTextToSpeech,
            AllTalk,
        }

        public TextToSpeech(Subtitle subtitle, SubtitleFormat subtitleFormat, string videoFileName, VideoInfo videoInfo)
        {
            UiUtil.PreInitialize(this);
            InitializeComponent();
            UiUtil.FixFonts(this);

            _subtitle = new Subtitle(subtitle, false);
            _subtitleFormat = subtitleFormat;
            _videoFileName = videoFileName;
            _videoInfo = videoInfo;
            _piperVoices = new List<PiperModel>();
            _elevenLabVoices = new List<ElevenLabModel>();
            _azureVoices = new List<AzureVoiceModel>();
            _actors = _subtitle.Paragraphs
                .Where(p => !string.IsNullOrEmpty(p.Actor))
                .Select(p => p.Actor)
                .Distinct()
                .ToList();

            Text = LanguageSettings.Current.TextToSpeech.Title;
            labelEngine.Text = LanguageSettings.Current.AudioToText.Engine;
            groupBoxSettings.Text = LanguageSettings.Current.Settings.Title;
            labelVoice.Text = LanguageSettings.Current.TextToSpeech.Voice;
            labelApiKey.Text = LanguageSettings.Current.VobSubOcr.ApiKey;
            buttonTestVoice.Text = LanguageSettings.Current.TextToSpeech.TestVoice;
            labelActors.Text = LanguageSettings.Current.TextToSpeech.ActorInfo;
            checkBoxAddToVideoFile.Text = LanguageSettings.Current.TextToSpeech.AddAudioToVideo;
            buttonGenerateTTS.Text = LanguageSettings.Current.TextToSpeech.GenerateSpeech;
            labelRegion.Text = LanguageSettings.Current.General.Region;
            checkBoxShowPreview.Text = LanguageSettings.Current.TextToSpeech.ReviewAudioClips;
            buttonOK.Text = LanguageSettings.Current.General.Ok;
            buttonCancel.Text = LanguageSettings.Current.General.Cancel;
            UiUtil.FixLargeFonts(this, buttonOK);

            progressBar1.Visible = false;
            labelProgress.Text = string.Empty;
            labelVoiceCount.Text = string.Empty;

            _engines = new List<TextToSpeechEngine>();
            _engines.Add(new TextToSpeechEngine(TextToSpeechEngineId.Piper, "Piper (fast/good)", _engines.Count));
            _engines.Add(new TextToSpeechEngine(TextToSpeechEngineId.AllTalk, "All Talk TTS (Coqui based)", _engines.Count));
            _engines.Add(new TextToSpeechEngine(TextToSpeechEngineId.Coqui, "Coqui AI TTS (only one voice)", _engines.Count));
            if (Configuration.IsRunningOnWindows)
            {
                _engines.Add(new TextToSpeechEngine(TextToSpeechEngineId.MsSpeechSynthesizer, "Microsoft SpeechSynthesizer (very fast/robotic)", _engines.Count));
            }
            _engines.Add(new TextToSpeechEngine(TextToSpeechEngineId.ElevenLabs, "ElevenLabs TTS (online/pay/good)", _engines.Count));
            _engines.Add(new TextToSpeechEngine(TextToSpeechEngineId.AzureTextToSpeech, "Microsoft Azure TTS (online/pay/good)", _engines.Count));
            _engines.Add(new TextToSpeechEngine(TextToSpeechEngineId.Tortoise, "Tortoise TTS (slow/good)", _engines.Count));

            _actorAndVoices = new List<ActorAndVoice>();
            nikseComboBoxEngine.DropDownStyle = ComboBoxStyle.DropDownList;
            nikseComboBoxEngine.Items.Clear();
            foreach (var engine in _engines)
            {
                nikseComboBoxEngine.Items.Add(engine.Name);
                if (Configuration.Settings.Tools.TextToSpeechEngine == engine.Id.ToString())
                {
                    nikseComboBoxEngine.SelectedIndex = nikseComboBoxEngine.Items.Count - 1;
                }
            }

            if (nikseComboBoxEngine.SelectedIndex < 0)
            {
                nikseComboBoxEngine.SelectedIndex = 0;
            }

            labelActors.Visible = false;
            listViewActors.Visible = false;

            if (!SubtitleFormatHasActors() || !_actors.Any())
            {
                var w = groupBoxSettings.Width + 100;
                var right = buttonCancel.Right;
                groupBoxSettings.Left = progressBar1.Left;
                groupBoxSettings.Width = right - progressBar1.Left;
                groupBoxSettings.Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right | AnchorStyles.Bottom;

                Width = w;
                MinimumSize = new Size(w, MinimumSize.Height);
                Width = w;
            }

            nikseComboBoxEngine_SelectedIndexChanged(null, null);
            nikseComboBoxEngine.SelectedIndexChanged += nikseComboBoxEngine_SelectedIndexChanged;
            nikseComboBoxVoice.Text = Configuration.Settings.Tools.TextToSpeechLastVoice;
            checkBoxAddToVideoFile.Checked = Configuration.Settings.Tools.TextToSpeechAddToVideoFile;
            checkBoxShowPreview.Checked = Configuration.Settings.Tools.TextToSpeechPreview;

            checkBoxAddToVideoFile.Enabled = _videoFileName != null;
        }

        private void SetActor(ActorAndVoice actor)
        {
            foreach (int index in listViewActors.SelectedIndices)
            {
                ListViewItem item = listViewActors.Items[index];
                var itemActor = (ActorAndVoice)item.Tag;
                itemActor.Voice = actor.Voice;
                itemActor.VoiceIndex = actor.VoiceIndex;
                item.SubItems[1].Text = actor.Voice;
            }
        }

        private void FillActorListView()
        {
            listViewActors.BeginUpdate();
            listViewActors.Items.Clear();
            foreach (var actor in _actorAndVoices)
            {
                var lvi = new ListViewItem
                {
                    Tag = actor,
                    Text = actor.Actor,
                };
                lvi.SubItems.Add(actor.Voice);
                listViewActors.Items.Add(lvi);
            }

            listViewActors.EndUpdate();
        }

        private void ButtonGenerateTtsClick(object sender, EventArgs e)
        {
            if (buttonGenerateTTS.Text == LanguageSettings.Current.General.Cancel)
            {
                buttonGenerateTTS.Enabled = false;
                _abort = true;
                Application.DoEvents();
                return;
            }

            try
            {
                buttonGenerateTTS.Text = LanguageSettings.Current.General.Cancel;
                _converting = true;
                buttonOK.Enabled = false;
                _waveFolder = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
                Directory.CreateDirectory(_waveFolder);

                var generateOk = GenerateParagraphAudio(_subtitle, true, null);
                if (_abort || !generateOk)
                {
                    HandleAbort();
                    return;
                }

                var fileNameAndSpeedFactors = FixParagraphAudioSpeed(_subtitle, null);
                if (_abort)
                {
                    HandleAbort();
                    return;
                }

                if (checkBoxShowPreview.Checked)
                {
                    using (var form = new ReviewAudioClips(this, _subtitle, fileNameAndSpeedFactors))
                    {
                        var dr = form.ShowDialog(this);
                        if (dr != DialogResult.OK)
                        {
                            _abort = true;
                            HandleAbort();
                            return;
                        }

                        foreach (var idx in form.SkipIndices)
                        {
                            fileNameAndSpeedFactors[idx] = null; // skip these files
                        }
                    }
                }

                var tempAudioFile = MergeAudioParagraphs(fileNameAndSpeedFactors);
                if (_abort)
                {
                    HandleAbort();
                    return;
                }

                // rename result file
                var resultAudioFileName = string.IsNullOrEmpty(_videoFileName) ? _subtitle.FileName : _videoFileName;
                if (!string.IsNullOrEmpty(resultAudioFileName))
                {
                    resultAudioFileName = Path.ChangeExtension(resultAudioFileName, ".wav");
                    resultAudioFileName = Path.Combine(_waveFolder, Path.GetFileName(resultAudioFileName));
                }
                else
                {
                    resultAudioFileName = Path.Combine(_waveFolder, "Untitled.wav");
                }

                if (File.Exists(resultAudioFileName))
                {
                    resultAudioFileName = tempAudioFile;
                }
                else
                {
                    File.Move(tempAudioFile, resultAudioFileName);
                }

                Cleanup(_waveFolder, resultAudioFileName);

                if (checkBoxAddToVideoFile.Checked)
                {
                    if (_abort)
                    {
                        HandleAbort();
                        return;
                    }

                    if (_videoFileName != null)
                    {
                        AddAudioToVideoFile(resultAudioFileName);
                    }

                }

                UiUtil.OpenFolder(_waveFolder);
                HandleAbort();
            }
            catch (Exception exception)
            {
                MessageBox.Show("Ups: " + exception.Message + Environment.NewLine + exception.Message);
                SeLogger.Error(exception, $"{Text}: Error running engine {nikseComboBoxEngine.Text} with video {_videoFileName}");
            }
        }

        private void HandleAbort()
        {
            buttonGenerateTTS.Enabled = false;
            progressBar1.Visible = false;
            labelProgress.Text = string.Empty;
            if (_abort)
            {
                Cleanup(_waveFolder, string.Empty);
            }

            buttonGenerateTTS.Text = "Generate text to speech";
            buttonGenerateTTS.Enabled = true;
            _abort = false;
            _converting = false;
            buttonOK.Enabled = true;
        }

        private bool GenerateParagraphAudio(Subtitle subtitle, bool showProgressBar, string overrideFileName)
        {
            var engine = _engines.First(p => p.Index == nikseComboBoxEngine.SelectedIndex);
            if (engine.Id == TextToSpeechEngineId.MsSpeechSynthesizer)
            {
                GenerateParagraphAudioMs(subtitle, showProgressBar, overrideFileName);
                return true;
            }

            if (engine.Id == TextToSpeechEngineId.Piper)
            {
                return GenerateParagraphAudioPiperTts(subtitle, showProgressBar, overrideFileName);
            }

            if (engine.Id == TextToSpeechEngineId.Tortoise)
            {
                return GenerateParagraphAudioTortoiseTts(subtitle, showProgressBar, overrideFileName);
            }

            if (engine.Id == TextToSpeechEngineId.Coqui)
            {
                var result = GenerateParagraphAudioCoqui(subtitle, showProgressBar, overrideFileName);
                return result;
            }

            if (engine.Id == TextToSpeechEngineId.AllTalk)
            {
                var result = GenerateParagraphAudioAllTalk(subtitle, showProgressBar, overrideFileName);
                return result;
            }

            if (engine.Id == TextToSpeechEngineId.ElevenLabs)
            {
                var result = GenerateParagraphAudioElevenLabs(subtitle, showProgressBar, overrideFileName);
                return result;
            }

            if (engine.Id == TextToSpeechEngineId.AzureTextToSpeech)
            {
                var result = GenerateParagraphAudioAzure(subtitle, showProgressBar, overrideFileName);
                return result;
            }

            return false;
        }

        private void AddAudioToVideoFile(string audioFileName)
        {
            var videoExt = ".mkv";
            if (_videoFileName.EndsWith(".mp4", StringComparison.OrdinalIgnoreCase))
            {
                videoExt = ".mp4";
            }

            labelProgress.Text = "Add audio to video file...";
            var outputFileName = Path.Combine(_waveFolder, Path.GetFileNameWithoutExtension(audioFileName) + videoExt);
            var addAudioProcess = VideoPreviewGenerator.AddAudioTrack(_videoFileName, audioFileName, outputFileName);
            addAudioProcess.Start();
            while (!addAudioProcess.HasExited)
            {
                Application.DoEvents();
                if (_abort)
                {
                    break;
                }
            }

            labelProgress.Text = string.Empty;
        }

        private static void Cleanup(string waveFolder, string resultAudioFile)
        {
            foreach (var fileName in Directory.GetFiles(waveFolder, "*.wav"))
            {
                if (!fileName.Equals(resultAudioFile, StringComparison.OrdinalIgnoreCase))
                {
                    SafeFileDelete(fileName);
                }
            }

            foreach (var fileName in Directory.GetFiles(waveFolder, "*.mp3"))
            {
                if (!fileName.Equals(resultAudioFile, StringComparison.OrdinalIgnoreCase))
                {
                    SafeFileDelete(fileName);
                }
            }
        }

        private List<FileNameAndSpeedFactor> FixParagraphAudioSpeed(Subtitle subtitle, string overrideFileName)
        {
            var fileNames = new List<FileNameAndSpeedFactor>(subtitle.Paragraphs.Count);

            labelProgress.Text = string.Empty;
            labelProgress.Refresh();
            Application.DoEvents();

            progressBar1.Value = 0;
            progressBar1.Maximum = subtitle.Paragraphs.Count;
            progressBar1.Visible = true;
            var ext = ".wav";

            for (var index = 0; index < subtitle.Paragraphs.Count; index++)
            {
                progressBar1.Value = index + 1;
                labelProgress.Text = string.Format(LanguageSettings.Current.TextToSpeech.AdjustingSpeedXOfY, index + 1, subtitle.Paragraphs.Count);
                var p = subtitle.Paragraphs[index];

                var next = subtitle.GetParagraphOrDefault(index + 1);
                var pFileName = Path.Combine(_waveFolder, index + ".wav");
                if (!string.IsNullOrEmpty(overrideFileName) && File.Exists(Path.Combine(_waveFolder, overrideFileName)))
                {
                    pFileName = Path.Combine(_waveFolder, overrideFileName);
                }

                if (!string.IsNullOrEmpty(overrideFileName) && File.Exists(Path.Combine(_waveFolder, Path.ChangeExtension(overrideFileName, ".mp3"))))
                {
                    pFileName = Path.Combine(_waveFolder, Path.ChangeExtension(overrideFileName, ".mp3"));
                }

                if (!File.Exists(pFileName))
                {
                    pFileName = Path.Combine(_waveFolder, index + ".mp3");
                }

                if (!File.Exists(pFileName) || string.IsNullOrWhiteSpace(p.Text))
                {
                    fileNames.Add(new FileNameAndSpeedFactor { Filename = pFileName, Factor = 1 });
                    continue;
                }

                var outputFileName1 = Path.Combine(_waveFolder, $"{index}_{Guid.NewGuid()}{ext}");
                if (!string.IsNullOrEmpty(overrideFileName) && File.Exists(Path.Combine(_waveFolder, overrideFileName)))
                {
                    outputFileName1 = Path.Combine(_waveFolder, Path.GetFileNameWithoutExtension(overrideFileName) + "_u.wav");
                }

                var trimProcess = VideoPreviewGenerator.TrimSilenceStartAndEnd(pFileName, outputFileName1);
                trimProcess.Start();
                while (!trimProcess.HasExited)
                {
                    if (_abort)
                    {
                        return new List<FileNameAndSpeedFactor>();
                    }
                }

                var addDuration = 0d;
                if (next != null && p.EndTime.TotalMilliseconds < next.StartTime.TotalMilliseconds)
                {
                    var diff = next.StartTime.TotalMilliseconds - p.EndTime.TotalMilliseconds;
                    addDuration = Math.Min(1000, diff);
                    if (addDuration < 0)
                    {
                        addDuration = 0;
                    }
                }

                if (_abort)
                {
                    return new List<FileNameAndSpeedFactor>();
                }

                var waveInfo = UiUtil.GetVideoInfo(outputFileName1);
                if (waveInfo.TotalMilliseconds <= p.DurationTotalMilliseconds + addDuration)
                {
                    fileNames.Add(new FileNameAndSpeedFactor { Filename = outputFileName1, Factor = 1 });
                    continue;
                }

                var factor = (decimal)waveInfo.TotalMilliseconds / (decimal)(p.DurationTotalMilliseconds + addDuration);
                var outputFileName2 = Path.Combine(_waveFolder, $"{index}_{Guid.NewGuid()}{ext}");
                if (!string.IsNullOrEmpty(overrideFileName) && File.Exists(Path.Combine(_waveFolder, overrideFileName)))
                {
                    outputFileName2 = Path.Combine(_waveFolder, $"{Path.GetFileNameWithoutExtension(overrideFileName)}_{Guid.NewGuid()}{ext}");
                }

                fileNames.Add(new FileNameAndSpeedFactor { Filename = outputFileName2, Factor = factor });
                var mergeProcess = VideoPreviewGenerator.ChangeSpeed(outputFileName1, outputFileName2, (float)factor);
                mergeProcess.Start();

                SafeFileDelete(pFileName);

                while (!mergeProcess.HasExited)
                {
                    Application.DoEvents();
                    if (_abort)
                    {
                        return new List<FileNameAndSpeedFactor>();
                    }
                }

                SafeFileDelete(outputFileName1);
            }

            return fileNames;
        }

        private static void SafeFileDelete(string pFileName)
        {
            try
            {
                File.Delete(pFileName);
            }
            catch
            {
                // ignore
            }
        }

        private string MergeAudioParagraphs(List<FileNameAndSpeedFactor> fileNames)
        {
            labelProgress.Text = string.Empty;
            labelProgress.Refresh();
            Application.DoEvents();
            var silenceFileName = Path.Combine(_waveFolder, "silence.wav");

            var durationInSeconds = 10f;
            if (_videoInfo != null)
            {
                durationInSeconds = (float)_videoInfo.TotalSeconds;
            }
            else if (_subtitle.Paragraphs.Count > 0)
            {
                durationInSeconds = (float)_subtitle.Paragraphs.Max(p => p.EndTime.TotalSeconds);
            }

            var silenceProcess = VideoPreviewGenerator.GenerateEmptyAudio(silenceFileName, durationInSeconds);
            silenceProcess.Start();
            silenceProcess.WaitForExit();

            progressBar1.Value = 0;
            progressBar1.Maximum = _subtitle.Paragraphs.Count;
            progressBar1.Visible = true;
            var inputFileName = silenceFileName;
            var outputFileName = string.Empty;
            for (var index = 0; index < fileNames.Count; index++)
            {
                progressBar1.Value = index + 1;
                labelProgress.Text = string.Format(LanguageSettings.Current.TextToSpeech.MergingAudioTrackXOfY, index + 1, _subtitle.Paragraphs.Count);
                var p = _subtitle.Paragraphs[index];
                var pFileName = fileNames[index];
                if (pFileName == null || !File.Exists(pFileName.Filename))
                {
                    SeLogger.Error($"TextToSpeech: File not found (skipping): {pFileName?.Filename}");
                    continue;
                }

                outputFileName = Path.Combine(_waveFolder, $"silence{index}.wav");
                var mergeProcess = VideoPreviewGenerator.MergeAudioTracks(inputFileName, pFileName.Filename, outputFileName, (float)p.StartTime.TotalSeconds);
                var deleteTempFileName = inputFileName;
                inputFileName = outputFileName;
                mergeProcess.Start();

                while (!mergeProcess.HasExited)
                {
                    Application.DoEvents();
                    if (_abort)
                    {
                        return string.Empty;
                    }
                }

                SafeFileDelete(deleteTempFileName);
            }

            progressBar1.Visible = false;
            labelProgress.Text = string.Empty;

            return outputFileName;
        }

        private void GenerateParagraphAudioMs(Subtitle subtitle, bool showProgressBar, string overrideFileName)
        {

            progressBar1.Value = 0;
            progressBar1.Maximum = subtitle.Paragraphs.Count;
            progressBar1.Visible = showProgressBar;

            using (var synthesizer = new System.Speech.Synthesis.SpeechSynthesizer())
            {
                System.Speech.Synthesis.VoiceInfo voiceInfo = null;
                var vs = synthesizer.GetInstalledVoices().Where(p => p.Enabled).ToList();
                for (var index = 0; index < vs.Count; index++)
                {
                    var v = vs[index];
                    if (index == nikseComboBoxVoice.SelectedIndex)
                    {
                        synthesizer.SelectVoice(v.VoiceInfo.Name);
                        voiceInfo = v.VoiceInfo;
                    }
                }

                for (var index = 0; index < subtitle.Paragraphs.Count; index++)
                {
                    if (showProgressBar)
                    {
                        progressBar1.Value = index + 1;
                        labelProgress.Text = string.Format(LanguageSettings.Current.TextToSpeech.GeneratingSpeechFromTextXOfY, index + 1, subtitle.Paragraphs.Count);
                    }

                    var p = subtitle.Paragraphs[index];
                    if (string.IsNullOrWhiteSpace(p.Text))
                    {
                        continue;
                    }

                    var wavFileName = Path.Combine(_waveFolder, string.IsNullOrEmpty(overrideFileName) ? index + ".wav" : overrideFileName);
                    synthesizer.SetOutputToWaveFile(wavFileName);
                    var builder = new System.Speech.Synthesis.PromptBuilder();
                    if (voiceInfo != null)
                    {
                        var v = voiceInfo;
                        if (_actorAndVoices.Count > 0 && !string.IsNullOrEmpty(p.Actor))
                        {
                            var f = _actorAndVoices.FirstOrDefault(x => x.Actor == p.Actor);
                            if (f != null && !string.IsNullOrEmpty(f.Voice))
                            {
                                var item = vs[f.VoiceIndex];
                                v = item.VoiceInfo;
                            }
                        }

                        builder.StartVoice(v);
                    }

                    var text = Utilities.UnbreakLine(p.Text);
                    builder.AppendText(text);
                    if (voiceInfo != null)
                    {
                        builder.EndVoice();
                    }

                    synthesizer.Speak(builder);

                    progressBar1.Refresh();
                    labelProgress.Refresh();
                    Application.DoEvents();
                }
            }

            progressBar1.Visible = false;
            labelProgress.Text = string.Empty;
        }

        private bool GenerateParagraphAudioPiperTts(Subtitle subtitle, bool showProgressBar, string overrideFileName)
        {
            var ttsPath = Path.Combine(Configuration.DataDirectory, "TextToSpeech");
            if (!Directory.Exists(ttsPath))
            {
                Directory.CreateDirectory(ttsPath);
            }

            var piperPath = Path.Combine(ttsPath, "Piper");
            if (!Directory.Exists(piperPath))
            {
                Directory.CreateDirectory(piperPath);
            }

            var piperExe = Path.Combine(piperPath, "piper.exe");

            if (Configuration.IsRunningOnWindows && !File.Exists(piperExe))
            {
                if (MessageBox.Show(string.Format(LanguageSettings.Current.Settings.DownloadX, "Piper Text To Speech"), "Subtitle Edit", MessageBoxButtons.YesNoCancel) != DialogResult.Yes)
                {
                    return false;
                }

                using (var form = new PiperDownload("Piper TextToSpeech") { AutoClose = true, PiperPath = piperPath })
                {
                    if (form.ShowDialog(this) != DialogResult.OK)
                    {
                        return false;
                    }
                }
            }

            progressBar1.Value = 0;
            progressBar1.Maximum = subtitle.Paragraphs.Count;
            progressBar1.Visible = showProgressBar;

            for (var index = 0; index < subtitle.Paragraphs.Count; index++)
            {
                if (showProgressBar)
                {
                    progressBar1.Value = index + 1;
                    labelProgress.Text = string.Format(LanguageSettings.Current.TextToSpeech.GeneratingSpeechFromTextXOfY, index + 1, subtitle.Paragraphs.Count);
                }

                var p = subtitle.Paragraphs[index];
                if (string.IsNullOrWhiteSpace(p.Text))
                {
                    continue;
                }

                var outputFileName = Path.Combine(_waveFolder, string.IsNullOrEmpty(overrideFileName) ? index + ".wav" : overrideFileName);
                if (File.Exists(outputFileName))
                {
                    SafeFileDelete(outputFileName);
                }

                var voice = _piperVoices.First(x => x.ToString() == nikseComboBoxVoice.Text);
                if (_actorAndVoices.Count > 0 && !string.IsNullOrEmpty(p.Actor))
                {
                    var f = _actorAndVoices.FirstOrDefault(x => x.Actor == p.Actor);
                    if (f != null && !string.IsNullOrEmpty(f.Voice))
                    {
                        voice = _piperVoices[f.VoiceIndex];
                    }
                }

                var modelFileName = Path.Combine(piperPath, voice.ModelShort);
                if (!File.Exists(modelFileName))
                {
                    using (var form = new PiperDownload("Piper TextToSpeech voice: " + voice.Voice) { AutoClose = true, ModelUrl = voice.Model, ModelFileName = modelFileName, PiperPath = piperPath })
                    {
                        if (form.ShowDialog(this) != DialogResult.OK)
                        {
                            return false;
                        }
                    }
                }

                var configFileName = Path.Combine(piperPath, voice.ConfigShort);
                if (!File.Exists(configFileName))
                {
                    using (var form = new PiperDownload("Piper TextToSpeech voice config: " + voice.Voice) { AutoClose = true, ModelUrl = voice.Config, ModelFileName = configFileName, PiperPath = piperPath })
                    {
                        if (form.ShowDialog(this) != DialogResult.OK)
                        {
                            return false;
                        }
                    }
                }

                var processPiper = new Process
                {
                    StartInfo =
                    {
                        WorkingDirectory = piperPath,
                        FileName = Configuration.IsRunningOnWindows ? piperExe : "piper",
                        Arguments = $"-m \"{voice.ModelShort}\" -c \"{voice.ConfigShort}\" -f out.wav",
                        UseShellExecute = false,
                        CreateNoWindow = true,
                        RedirectStandardInput = true,
                    }
                };

                processPiper.Start();
                var streamWriter = new StreamWriter(processPiper.StandardInput.BaseStream, Encoding.UTF8);
                var text = Utilities.UnbreakLine(p.Text);
                streamWriter.Write(text);
                streamWriter.Flush();
                streamWriter.Close();

                while (!processPiper.HasExited)
                {
                    Application.DoEvents();
                    if (_abort)
                    {
                        progressBar1.Visible = false;
                        labelProgress.Text = string.Empty;
                        return false;
                    }
                }

                var inputFile = Path.Combine(piperPath, "out.wav");
                File.Move(inputFile, outputFileName);

                progressBar1.Refresh();
                labelProgress.Refresh();
                Application.DoEvents();
                streamWriter.Dispose();
            }

            progressBar1.Visible = false;
            labelProgress.Text = string.Empty;

            return true;
        }

        private bool GenerateParagraphAudioTortoiseTts(Subtitle subtitle, bool showProgressBar, string overrideFileName)
        {
            var pythonFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                "AppData",
                "Local",
                "Programs",
                "Python");

            var pyFileName = "do_tts.py";
            var voice = nikseComboBoxVoice.Text;
            var files = Directory.EnumerateFiles(pythonFolder, pyFileName, SearchOption.AllDirectories).ToList();
            if (files.Count == 0)
            {
                MessageBox.Show(this, $"{pyFileName} not found under {pythonFolder}");
                return false;
            }

            var pythonFolderVersionFolder = files[0].Substring(pythonFolder.Length).Split(new char[] { Path.DirectorySeparatorChar }, StringSplitOptions.RemoveEmptyEntries);
            var pythonExe = Path.Combine(pythonFolder, pythonFolderVersionFolder[0], "python.exe");
            if (!File.Exists(pythonExe))
            {
                pythonExe = Configuration.IsRunningOnWindows ? "python.exe" : "python";
            }

            progressBar1.Value = 0;
            progressBar1.Maximum = subtitle.Paragraphs.Count;
            progressBar1.Visible = showProgressBar;

            for (var index = 0; index < subtitle.Paragraphs.Count; index++)
            {
                if (showProgressBar)
                {
                    progressBar1.Value = index + 1;
                    labelProgress.Text = string.Format(LanguageSettings.Current.TextToSpeech.GeneratingSpeechFromTextXOfY, index + 1, subtitle.Paragraphs.Count);
                }

                var p = subtitle.Paragraphs[index];
                if (string.IsNullOrWhiteSpace(p.Text))
                {
                    continue;
                }

                var outputFileName = Path.Combine(_waveFolder, string.IsNullOrEmpty(overrideFileName) ? index + ".wav" : overrideFileName);

                var v = voice;
                if (_actorAndVoices.Count > 0 && !string.IsNullOrEmpty(p.Actor))
                {
                    var f = _actorAndVoices.FirstOrDefault(x => x.Actor == p.Actor);
                    if (f != null && !string.IsNullOrEmpty(f.Voice))
                    {
                        v = f.Voice;
                    }
                }

                var text = Utilities.UnbreakLine(p.Text);
                var processTortoiseTts = new Process
                {
                    StartInfo =
                    {
                        WorkingDirectory = Path.GetDirectoryName(files[0]),
                        FileName = pythonExe,
                        Arguments = $"do_tts.py --text \"{text.RemoveChar('"')}\" --output_path \"{_waveFolder.TrimEnd(Path.DirectorySeparatorChar)}\" --preset ultra_fast --voice {v}",
                        UseShellExecute = false,
                        CreateNoWindow = true,
                    }
                };

                processTortoiseTts.Start();
                while (!processTortoiseTts.HasExited)
                {
                    Application.DoEvents();
                    if (_abort)
                    {
                        progressBar1.Visible = false;
                        labelProgress.Text = string.Empty;
                        return false;
                    }
                }

                var inputFile = Path.Combine(_waveFolder, $"{v}_0_2.wav");
                if (!File.Exists(inputFile))
                {
                    throw new Exception("No file generated by Tortoise TTS?" + Environment.NewLine + inputFile + " not found!");
                }

                File.Move(inputFile, outputFileName);

                progressBar1.Refresh();
                labelProgress.Refresh();
                Application.DoEvents();
            }

            progressBar1.Visible = false;
            labelProgress.Text = string.Empty;

            return true;
        }

        private bool GenerateParagraphAudioCoqui(Subtitle subtitle, bool showProgressBar, string overrideFileName)
        {
            var httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri(nikseComboBoxVoice.Text.StartsWith("http", StringComparison.OrdinalIgnoreCase) ? nikseComboBoxVoice.Text : "http://localhost:5002/api/tts");

            progressBar1.Value = 0;
            progressBar1.Maximum = subtitle.Paragraphs.Count;
            progressBar1.Visible = showProgressBar;

            for (var index = 0; index < subtitle.Paragraphs.Count; index++)
            {
                if (showProgressBar)
                {
                    progressBar1.Value = index + 1;
                    labelProgress.Text = string.Format(LanguageSettings.Current.TextToSpeech.GeneratingSpeechFromTextXOfY, index + 1, subtitle.Paragraphs.Count);
                }

                var p = subtitle.Paragraphs[index];
                if (string.IsNullOrWhiteSpace(p.Text))
                {
                    continue;
                }

                var outputFileName = Path.Combine(_waveFolder, string.IsNullOrEmpty(overrideFileName) ? index + ".wav" : overrideFileName);

                var text = Utilities.UnbreakLine(p.Text);
                var result = httpClient.GetAsync("?text=" + Utilities.UrlEncode(text)).Result;
                var bytes = result.Content.ReadAsByteArrayAsync().Result;

                if (!result.IsSuccessStatusCode)
                {
                    SeLogger.Error($"coqui TTS failed calling API as base address {httpClient.BaseAddress} : Status code={result.StatusCode}");
                }

                File.WriteAllBytes(outputFileName, bytes);

                progressBar1.Refresh();
                labelProgress.Refresh();
                Application.DoEvents();
            }

            progressBar1.Visible = false;
            labelProgress.Text = string.Empty;

            return true;
        }

        private bool GenerateParagraphAudioAllTalk(Subtitle subtitle, bool showProgressBar, string overrideFileName)
        {
            var httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri("http://127.0.0.1:7851");

            progressBar1.Value = 0;
            progressBar1.Maximum = subtitle.Paragraphs.Count;
            progressBar1.Visible = showProgressBar;

            for (var index = 0; index < subtitle.Paragraphs.Count; index++)
            {
                if (showProgressBar)
                {
                    progressBar1.Value = index + 1;
                    labelProgress.Text = string.Format(LanguageSettings.Current.TextToSpeech.GeneratingSpeechFromTextXOfY, index + 1, subtitle.Paragraphs.Count);
                }

                var voice = nikseComboBoxVoice.Text;
                var p = subtitle.Paragraphs[index];
                if (string.IsNullOrWhiteSpace(p.Text))
                {
                    continue;
                }

                if (_actorAndVoices.Count > 0 && !string.IsNullOrEmpty(p.Actor))
                {
                    var f = _actorAndVoices.FirstOrDefault(x => x.Actor == p.Actor);
                    if (f != null && !string.IsNullOrEmpty(f.Voice))
                    {
                        voice = f.Voice;
                    }
                }

                var outputFileName = Path.Combine(_waveFolder, string.IsNullOrEmpty(overrideFileName) ? index + ".wav" : overrideFileName);

                var multipartContent = new MultipartFormDataContent();
                var text = Utilities.UnbreakLine(p.Text);
                multipartContent.Add(new StringContent(Json.EncodeJsonText(text)), "text_input");
                multipartContent.Add(new StringContent("standard"), "text_filtering");
                multipartContent.Add(new StringContent(voice), "character_voice_gen");
                multipartContent.Add(new StringContent("false"), "narrator_enabled");
                multipartContent.Add(new StringContent(voice), "narrator_voice_gen");
                multipartContent.Add(new StringContent("character"), "text_not_inside");
                multipartContent.Add(new StringContent(nikseComboBoxRegion.Text), "language");
                multipartContent.Add(new StringContent("output"), "output_file_name");
                multipartContent.Add(new StringContent("false"), "output_file_timestamp");
                multipartContent.Add(new StringContent("false"), "autoplay");
                multipartContent.Add(new StringContent("1.0"), "autoplay_volume");
                var result = httpClient.PostAsync("/api/tts-generate", multipartContent).Result;
                var bytes = result.Content.ReadAsByteArrayAsync().Result;
                var resultJson = Encoding.UTF8.GetString(bytes);

                if (!result.IsSuccessStatusCode)
                {
                    SeLogger.Error($"All Talk TTS failed calling API as base address {httpClient.BaseAddress} : Status code={result.StatusCode}" + Environment.NewLine + resultJson);
                }

                var jsonParser = new SeJsonParser();
                var allTalkOutput = jsonParser.GetFirstObject(resultJson, "output_file_path");
                File.Copy(allTalkOutput, outputFileName);
                SafeFileDelete(allTalkOutput);
                progressBar1.Refresh();
                labelProgress.Refresh();
                Application.DoEvents();

                if (_abort)
                {
                    return false;
                }
            }

            progressBar1.Visible = false;
            labelProgress.Text = string.Empty;

            return true;
        }

        private bool GenerateParagraphAudioElevenLabs(Subtitle subtitle, bool showProgressBar, string overrideFileName)
        {
            if (string.IsNullOrWhiteSpace(nikseTextBoxApiKey.Text))
            {
                MessageBox.Show("Please add API key");
                nikseTextBoxApiKey.Focus();
                return false;
            }

            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/json");
            httpClient.DefaultRequestHeaders.TryAddWithoutValidation("accept", "audio/mpeg");
            httpClient.DefaultRequestHeaders.TryAddWithoutValidation("xi-api-key", nikseTextBoxApiKey.Text.Trim());

            progressBar1.Value = 0;
            progressBar1.Maximum = subtitle.Paragraphs.Count;
            progressBar1.Visible = showProgressBar;

            var voices = _elevenLabVoices;
            var v = nikseComboBoxVoice.Text;

            for (var index = 0; index < subtitle.Paragraphs.Count; index++)
            {
                if (showProgressBar)
                {
                    progressBar1.Value = index + 1;
                    labelProgress.Text = string.Format(LanguageSettings.Current.TextToSpeech.GeneratingSpeechFromTextXOfY, index + 1, subtitle.Paragraphs.Count);
                }

                var p = subtitle.Paragraphs[index];
                if (string.IsNullOrWhiteSpace(p.Text))
                {
                    continue;
                }

                var outputFileName = Path.Combine(_waveFolder, string.IsNullOrEmpty(overrideFileName) ? index + ".mp3" : overrideFileName.Replace(".wav", ".mp3"));

                if (_actorAndVoices.Count > 0 && !string.IsNullOrEmpty(p.Actor))
                {
                    var f = _actorAndVoices.FirstOrDefault(x => x.Actor == p.Actor);
                    if (f != null && !string.IsNullOrEmpty(f.Voice))
                    {
                        v = f.Voice;
                    }
                }

                var voice = voices.First(x => x.ToString() == v);

                var url = "https://api.elevenlabs.io/v1/text-to-speech/" + voice.Model;
                var text = Utilities.UnbreakLine(p.Text);
                var data = "{ \"text\": \"" + Json.EncodeJsonText(text) + "\", \"model_id\": \"eleven_multilingual_v2\", \"voice_settings\": { \"stability\": 0.8, \"similarity_boost\": 1.0 } }";
                var content = new StringContent(data, Encoding.UTF8);
                content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
                var result = httpClient.PostAsync(url, content, CancellationToken.None).Result;
                var bytes = result.Content.ReadAsByteArrayAsync().Result;

                if (!result.IsSuccessStatusCode)
                {
                    var error = Encoding.UTF8.GetString(bytes).Trim();
                    SeLogger.Error($"ElevenLabs TTS failed calling API as base address {httpClient.BaseAddress} : Status code={result.StatusCode} {error}" + Environment.NewLine + "Data=" + data);
                    MessageBox.Show(this, "Calling url: " + url + Environment.NewLine + "With: " + data + Environment.NewLine + Environment.NewLine + "Error: " + error);
                    return false;
                }

                File.WriteAllBytes(outputFileName, bytes);

                progressBar1.Refresh();
                labelProgress.Refresh();
                Application.DoEvents();
            }

            progressBar1.Visible = false;
            labelProgress.Text = string.Empty;

            return true;
        }

        private List<AzureVoiceModel> GetAzureVoices(bool useCache)
        {
            var ttsPath = Path.Combine(Configuration.DataDirectory, "TextToSpeech");
            if (!Directory.Exists(ttsPath))
            {
                Directory.CreateDirectory(ttsPath);
            }

            var azurePath = Path.Combine(ttsPath, "Azure");
            if (!Directory.Exists(azurePath))
            {
                Directory.CreateDirectory(azurePath);
            }

            var list = new List<AzureVoiceModel>();
            var jsonFileName = Path.Combine(azurePath, "AzureVoices.json");
            if (!File.Exists(jsonFileName))
            {
                var asm = System.Reflection.Assembly.GetExecutingAssembly();
                var stream = asm.GetManifestResourceStream("Nikse.SubtitleEdit.Resources.AzureVoices.zip");
                if (stream != null)
                {
                    using (var zip = ZipExtractor.Open(stream))
                    {
                        var dir = zip.ReadCentralDir();
                        foreach (var entry in dir)
                        {
                            var fileName = Path.GetFileName(entry.FilenameInZip);
                            if (!string.IsNullOrEmpty(fileName))
                            {
                                var name = entry.FilenameInZip;
                                var path = Path.Combine(azurePath, name.Replace('/', Path.DirectorySeparatorChar));
                                zip.ExtractFile(entry, path);
                            }
                        }
                    }
                }
            }

            if (!useCache)
            {
                using (var httpClient = new HttpClient())
                {
                    httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Ocp-Apim-Subscription-Key", nikseTextBoxApiKey.Text.Trim());
                    var url = $"https://{nikseComboBoxRegion.Text.Trim()}.tts.speech.microsoft.com/cognitiveservices/voices/list";
                    var result = httpClient.GetAsync(new Uri(url)).Result;
                    var bytes = result.Content.ReadAsByteArrayAsync().Result;

                    if (!result.IsSuccessStatusCode)
                    {
                        Cursor = Cursors.Default;
                        var error = Encoding.UTF8.GetString(bytes).Trim();
                        SeLogger.Error($"Failed getting voices form Azure via url \"{url}\" : Status code={result.StatusCode} {error}");
                        MessageBox.Show(this, "Calling url: " + url + Environment.NewLine + "Got error: " + error);
                        return new List<AzureVoiceModel>();
                    }

                    File.WriteAllBytes(jsonFileName, bytes);
                }
            }

            var json = File.ReadAllText(jsonFileName);
            var parser = new SeJsonParser();
            var arr = parser.GetArrayElements(json);
            foreach (var item in arr)
            {
                var displayName = parser.GetFirstObject(item, "DisplayName");
                var localName = parser.GetFirstObject(item, "LocalName");
                var shortName = parser.GetFirstObject(item, "ShortName");
                var gender = parser.GetFirstObject(item, "Gender");
                var locale = parser.GetFirstObject(item, "Locale");

                list.Add(new AzureVoiceModel
                {
                    DisplayName = displayName,
                    LocalName = localName,
                    ShortName = shortName,
                    Gender = gender,
                    Locale = locale,
                });
            }

            return list;
        }

        private bool GenerateParagraphAudioAzure(Subtitle subtitle, bool showProgressBar, string overrideFileName)
        {
            if (string.IsNullOrWhiteSpace(nikseTextBoxApiKey.Text))
            {
                MessageBox.Show("Please add API key");
                nikseTextBoxApiKey.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(nikseComboBoxRegion.Text))
            {
                MessageBox.Show("Please add region");
                nikseComboBoxRegion.Focus();
                return false;
            }

            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "ssml+xml");
            httpClient.DefaultRequestHeaders.TryAddWithoutValidation("accept", "audio/mpeg");
            httpClient.DefaultRequestHeaders.TryAddWithoutValidation("X-Microsoft-OutputFormat", "audio-16khz-32kbitrate-mono-mp3");
            httpClient.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "SubtitleEdit");
            httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Ocp-Apim-Subscription-Key", nikseTextBoxApiKey.Text.Trim());

            progressBar1.Value = 0;
            progressBar1.Maximum = subtitle.Paragraphs.Count;
            progressBar1.Visible = showProgressBar;

            var voices = _azureVoices;
            var v = nikseComboBoxVoice.Text;

            for (var index = 0; index < subtitle.Paragraphs.Count; index++)
            {
                if (showProgressBar)
                {
                    progressBar1.Value = index + 1;
                    labelProgress.Text = string.Format(LanguageSettings.Current.TextToSpeech.GeneratingSpeechFromTextXOfY, index + 1, subtitle.Paragraphs.Count);
                }

                var p = subtitle.Paragraphs[index];
                if (string.IsNullOrWhiteSpace(p.Text))
                {
                    continue;
                }

                var outputFileName = Path.Combine(_waveFolder, string.IsNullOrEmpty(overrideFileName) ? index + ".mp3" : overrideFileName.Replace(".wav", ".mp3"));

                if (_actorAndVoices.Count > 0 && !string.IsNullOrEmpty(p.Actor))
                {
                    var f = _actorAndVoices.FirstOrDefault(x => x.Actor == p.Actor);
                    if (f != null && !string.IsNullOrEmpty(f.Voice))
                    {
                        v = f.Voice;
                    }
                }

                var voice = voices.First(x => x.ToString() == v);

                var url = $"https://{nikseComboBoxRegion.Text.Trim()}.tts.speech.microsoft.com/cognitiveservices/v1";
                var text = Utilities.UnbreakLine(p.Text);
                var data = $"<speak version='1.0' xml:lang='en-US'><voice xml:lang='en-US' xml:gender='{voice.Gender}' name='{voice.ShortName}'>{System.Net.WebUtility.HtmlEncode(text)}</voice></speak>";
                var content = new StringContent(data, Encoding.UTF8);
                var result = httpClient.PostAsync(url, content, CancellationToken.None).Result;
                var bytes = result.Content.ReadAsByteArrayAsync().Result;

                if (!result.IsSuccessStatusCode)
                {
                    var error = Encoding.UTF8.GetString(bytes).Trim();
                    SeLogger.Error($"Azure TTS failed calling API on address {url} : Status code={result.StatusCode} {error}" + Environment.NewLine + "Data=" + data);
                    MessageBox.Show(this, "Calling url: " + url + Environment.NewLine + "With: " + data + Environment.NewLine + Environment.NewLine + "Error: " + error + result);
                    return false;
                }

                File.WriteAllBytes(outputFileName, bytes);

                progressBar1.Refresh();
                labelProgress.Refresh();
                Application.DoEvents();
            }

            progressBar1.Visible = false;
            labelProgress.Text = string.Empty;

            return true;
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            EditedSubtitle = _subtitle;
            DialogResult = DialogResult.OK;
        }

        private void nikseComboBoxEngine_SelectedIndexChanged(object sender, EventArgs e)
        {
            labelVoiceCount.Text = string.Empty;
            nikseComboBoxVoice.Items.Clear();

            labelApiKey.Visible = false;
            nikseTextBoxApiKey.Visible = false;
            labelRegion.Visible = false;
            nikseComboBoxRegion.Visible = false;

            labelRegion.Text = LanguageSettings.Current.General.Region;
            labelVoice.Text = LanguageSettings.Current.TextToSpeech.Voice;
            if (SubtitleFormatHasActors() && _actors.Any())
            {
                labelVoice.Text = LanguageSettings.Current.TextToSpeech.DefaultVoice;
            }

            var engine = _engines.First(p => p.Index == nikseComboBoxEngine.SelectedIndex);
            if (engine.Id == TextToSpeechEngineId.MsSpeechSynthesizer)
            {
                using (var synthesizer = new System.Speech.Synthesis.SpeechSynthesizer())
                {
                    foreach (var v in synthesizer.GetInstalledVoices())
                    {
                        if (v.Enabled)
                        {
                            nikseComboBoxVoice.Items.Add(v.VoiceInfo.Name);
                        }
                    }
                }
            }

            if (engine.Id == TextToSpeechEngineId.Piper)
            {
                if (_piperVoices.Count == 0)
                {
                    _piperVoices.AddRange(GetPiperVoices(true));
                }

                foreach (var voice in _piperVoices)
                {
                    nikseComboBoxVoice.Items.Add(voice.ToString());
                }
            }

            if (engine.Id == TextToSpeechEngineId.Tortoise)
            {
                nikseComboBoxVoice.Items.Add("angie");
                nikseComboBoxVoice.Items.Add("applejack");
                nikseComboBoxVoice.Items.Add("daniel");
                nikseComboBoxVoice.Items.Add("deniro");
                nikseComboBoxVoice.Items.Add("emma");
                nikseComboBoxVoice.Items.Add("freeman");
                nikseComboBoxVoice.Items.Add("geralt");
                nikseComboBoxVoice.Items.Add("halle");
                nikseComboBoxVoice.Items.Add("jlaw");
                nikseComboBoxVoice.Items.Add("lj");
                nikseComboBoxVoice.Items.Add("mol");
                nikseComboBoxVoice.Items.Add("myself");
                nikseComboBoxVoice.Items.Add("pat");
                nikseComboBoxVoice.Items.Add("pat2");
                nikseComboBoxVoice.Items.Add("rainbow");
                nikseComboBoxVoice.Items.Add("Update rainbow");
                nikseComboBoxVoice.Items.Add("snakes");
                nikseComboBoxVoice.Items.Add("tim_reynolds");
                nikseComboBoxVoice.Items.Add("tom");
                nikseComboBoxVoice.Items.Add("weaver");
                nikseComboBoxVoice.Items.Add("william");
            }

            if (engine.Id == TextToSpeechEngineId.Coqui)
            {
                labelVoice.Text = LanguageSettings.Current.General.WebServiceUrl;
                nikseComboBoxVoice.Items.Add("http://localhost:5002/api/tts");
            }

            if (engine.Id == TextToSpeechEngineId.AllTalk)
            {
                nikseComboBoxVoice.Items.AddRange(GetAllTalkVoices(true).ToArray());

                labelRegion.Text = LanguageSettings.Current.ChooseLanguage.Language;
                nikseComboBoxRegion.Items.Clear();
                nikseComboBoxRegion.Items.Add("ar");
                nikseComboBoxRegion.Items.Add("zh");
                nikseComboBoxRegion.Items.Add("cs");
                nikseComboBoxRegion.Items.Add("nl");
                nikseComboBoxRegion.Items.Add("en");
                nikseComboBoxRegion.Items.Add("fr");
                nikseComboBoxRegion.Items.Add("de");
                nikseComboBoxRegion.Items.Add("hi");
                nikseComboBoxRegion.Items.Add("hu");
                nikseComboBoxRegion.Items.Add("it");
                nikseComboBoxRegion.Items.Add("ja");
                nikseComboBoxRegion.Items.Add("ko");
                nikseComboBoxRegion.Items.Add("pl");
                nikseComboBoxRegion.Items.Add("pt");
                nikseComboBoxRegion.Items.Add("ru");
                nikseComboBoxRegion.Items.Add("es");
                nikseComboBoxRegion.Items.Add("tr");

                labelRegion.Visible = true;
                nikseComboBoxRegion.Visible = true;
                nikseComboBoxRegion.Text = "en";
            }

            if (engine.Id == TextToSpeechEngineId.ElevenLabs)
            {
                nikseTextBoxApiKey.Text = Configuration.Settings.Tools.TextToSpeechElevenLabsApiKey;

                labelApiKey.Visible = true;
                nikseTextBoxApiKey.Visible = true;

                if (_elevenLabVoices.Count == 0)
                {
                    _elevenLabVoices.AddRange(GetElevenLabVoices(true));
                }

                foreach (var voice in _elevenLabVoices)
                {
                    nikseComboBoxVoice.Items.Add(voice.ToString());
                }
            }

            if (engine.Id == TextToSpeechEngineId.AzureTextToSpeech)
            {
                labelApiKey.Visible = true;
                nikseTextBoxApiKey.Visible = true;

                var azureVoices = GetAzureVoices(true);
                _azureVoices.AddRange(azureVoices);
                nikseComboBoxVoice.Items.AddRange(_azureVoices.Select(p => p.ToString()).ToArray());

                labelRegion.Visible = true;
                nikseComboBoxRegion.Visible = true;

                nikseComboBoxRegion.Items.Clear();
                nikseComboBoxRegion.Items.Add("australiaeast");
                nikseComboBoxRegion.Items.Add("brazilsouth");
                nikseComboBoxRegion.Items.Add("canadacentral");
                nikseComboBoxRegion.Items.Add("centralus");
                nikseComboBoxRegion.Items.Add("eastasia");
                nikseComboBoxRegion.Items.Add("eastus");
                nikseComboBoxRegion.Items.Add("eastus2");
                nikseComboBoxRegion.Items.Add("francecentral");
                nikseComboBoxRegion.Items.Add("germanywestcentral");
                nikseComboBoxRegion.Items.Add("centralindia");
                nikseComboBoxRegion.Items.Add("japaneast");
                nikseComboBoxRegion.Items.Add("japanwest");
                nikseComboBoxRegion.Items.Add("jioindiawest");
                nikseComboBoxRegion.Items.Add("koreacentral");
                nikseComboBoxRegion.Items.Add("northcentralus");
                nikseComboBoxRegion.Items.Add("northeurope");
                nikseComboBoxRegion.Items.Add("norwayeast");
                nikseComboBoxRegion.Items.Add("southcentralus");
                nikseComboBoxRegion.Items.Add("southeastasia");
                nikseComboBoxRegion.Items.Add("swedencentral");
                nikseComboBoxRegion.Items.Add("switzerlandnorth");
                nikseComboBoxRegion.Items.Add("switzerlandwest");
                nikseComboBoxRegion.Items.Add("uaenorth");
                nikseComboBoxRegion.Items.Add("usgovarizona");
                nikseComboBoxRegion.Items.Add("usgovvirginia");
                nikseComboBoxRegion.Items.Add("uksouth");
                nikseComboBoxRegion.Items.Add("westcentralus");
                nikseComboBoxRegion.Items.Add("westeurope");
                nikseComboBoxRegion.Items.Add("westus");
                nikseComboBoxRegion.Items.Add("westus2");
                nikseComboBoxRegion.Items.Add("westus3");

                nikseTextBoxApiKey.Text = Configuration.Settings.Tools.TextToSpeechAzureApiKey;
                nikseComboBoxRegion.Text = Configuration.Settings.Tools.TextToSpeechAzureRegion;
            }

            if (nikseComboBoxVoice.Items.Count > 0 && nikseComboBoxVoice.SelectedIndex < 0)
            {
                SetFirstLanguageHitAsVoice();
            }

            if (nikseComboBoxVoice.Items.Count > 1)
            {
                labelVoiceCount.Text = nikseComboBoxVoice.Items.Count.ToString(CultureInfo.InvariantCulture);
            }

            _actorAndVoices.Clear();

            if (SubtitleFormatHasActors())
            {
                if (_actors.Any())
                {
                    foreach (var actor in _actors)
                    {
                        var actorAndVoice = new ActorAndVoice
                        {
                            Actor = actor,
                            UseCount = _subtitle.Paragraphs.Count(p => p.Actor == actor),
                        };

                        _actorAndVoices.Add(actorAndVoice);
                    }

                    FillActorListView();

                    contextMenuStripActors.Items.Clear();

                    if (engine.Id == TextToSpeechEngineId.Piper)
                    {
                        var voices = _piperVoices;
                        foreach (var voiceLanguage in voices
                                     .GroupBy(p => p.Language)
                                     .OrderBy(p => p.Key))
                        {
                            if (voiceLanguage.Count() == 1)
                            {
                                var voice = voiceLanguage.First();
                                var tsi = new ToolStripMenuItem();
                                tsi.Tag = new ActorAndVoice { Voice = voice.Voice, VoiceIndex = voices.IndexOf(voice) };
                                tsi.Text = voice.ToString();
                                tsi.Click += (x, args) =>
                                {
                                    var a = (ActorAndVoice)(x as ToolStripItem).Tag;
                                    SetActor(a);
                                };
                                contextMenuStripActors.Items.Add(tsi);
                            }
                            else
                            {
                                var parent = new ToolStripMenuItem();
                                parent.Text = voiceLanguage.Key;
                                contextMenuStripActors.Items.Add(parent);

                                foreach (var voice in voiceLanguage.OrderBy(p => p.Voice).ToList())
                                {
                                    var tsi = new ToolStripMenuItem();
                                    tsi.Tag = new ActorAndVoice { Voice = voice.Voice, VoiceIndex = voices.IndexOf(voice) };
                                    tsi.Text = voice.Voice + " (" + voice.Quality + ")";
                                    tsi.Click += (x, args) =>
                                    {
                                        var a = (ActorAndVoice)(x as ToolStripItem).Tag;
                                        SetActor(a);
                                    };
                                    parent.DropDownItems.Add(tsi);
                                }

                                if (Configuration.Settings.General.UseDarkTheme)
                                {
                                    DarkTheme.SetDarkTheme(parent);
                                }
                            }
                        }
                    }
                    if (engine.Id == TextToSpeechEngineId.AzureTextToSpeech)
                    {
                        var voices = _azureVoices;
                        foreach (var voiceLanguage in voices
                                     .GroupBy(p => p.Locale.Substring(0, 2))
                                     .OrderBy(p => p.Key))
                        {
                            if (voiceLanguage.Count() == 1)
                            {
                                var voice = voiceLanguage.First();
                                var tsi = new ToolStripMenuItem();
                                tsi.Tag = new ActorAndVoice { Voice = voice.ToString(), VoiceIndex = voices.IndexOf(voice) };
                                tsi.Text = voice.ToString();
                                tsi.Click += (x, args) =>
                                {
                                    var a = (ActorAndVoice)(x as ToolStripItem).Tag;
                                    SetActor(a);
                                };
                                contextMenuStripActors.Items.Add(tsi);
                            }
                            else
                            {
                                if (voiceLanguage.Count() < 30)
                                {
                                    var parent = new ToolStripMenuItem();
                                    parent.Text = voiceLanguage.Key;
                                    contextMenuStripActors.Items.Add(parent);
                                    var tsiList = new List<ToolStripItem>(nikseComboBoxVoice.Items.Count);
                                    foreach (var voice in voiceLanguage.OrderBy(p => p.ToString()).ToList())
                                    {
                                        var tsi = new ToolStripMenuItem();
                                        tsi.Tag = new ActorAndVoice { Voice = voice.ToString(), VoiceIndex = voices.IndexOf(voice) };
                                        tsi.Text = voice.ToString();
                                        tsi.Click += (x, args) =>
                                        {
                                            var a = (ActorAndVoice)(x as ToolStripItem).Tag;
                                            SetActor(a);
                                        };
                                        tsiList.Add(tsi);
                                    }
                                    parent.DropDownItems.AddRange(tsiList.ToArray());

                                    if (Configuration.Settings.General.UseDarkTheme)
                                    {
                                        DarkTheme.SetDarkTheme(parent);
                                    }
                                }
                                else
                                {
                                    var parent = new ToolStripMenuItem();
                                    parent.Text = voiceLanguage.Key;
                                    contextMenuStripActors.Items.Add(parent);
                                    var subGroup = voiceLanguage.GroupBy(p => p.Locale);
                                    foreach (var subGroupElement in subGroup)
                                    {
                                        var groupParent = new ToolStripMenuItem();
                                        groupParent.Text = subGroupElement.Key;
                                        parent.DropDownItems.Add(groupParent);
                                        var tsiList = new List<ToolStripItem>(subGroupElement.Count());
                                        foreach (var voice in subGroupElement.OrderBy(p => p.DisplayName).ToList())
                                        {
                                            var tsi = new ToolStripMenuItem();
                                            tsi.Tag = new ActorAndVoice { Voice = voice.ToString(), VoiceIndex = voices.IndexOf(voice) };
                                            tsi.Text = voice.ToString();
                                            tsi.Click += (x, args) =>
                                            {
                                                var a = (ActorAndVoice)(x as ToolStripItem).Tag;
                                                SetActor(a);
                                            };
                                            tsiList.Add(tsi);
                                        }

                                        groupParent.DropDownItems.AddRange(tsiList.ToArray());

                                        if (Configuration.Settings.General.UseDarkTheme)
                                        {
                                            DarkTheme.SetDarkTheme(groupParent);
                                        }

                                    }
                                    if (Configuration.Settings.General.UseDarkTheme)
                                    {
                                        DarkTheme.SetDarkTheme(parent);
                                    }
                                }
                            }
                        }
                    }
                    else if (engine.Id == TextToSpeechEngineId.ElevenLabs)
                    {
                        var voices = _elevenLabVoices;
                        foreach (var voiceLanguage in voices
                                     .GroupBy(p => p.Language)
                                     .OrderBy(p => p.Key))
                        {
                            if (voiceLanguage.Count() == 1)
                            {
                                var voice = voiceLanguage.First();
                                var tsi = new ToolStripMenuItem();
                                tsi.Tag = new ActorAndVoice { Voice = voice.Voice, VoiceIndex = voices.IndexOf(voice) };
                                tsi.Text = voice.ToString();
                                tsi.Click += (x, args) =>
                                {
                                    var a = (ActorAndVoice)(x as ToolStripItem).Tag;
                                    SetActor(a);
                                };
                                contextMenuStripActors.Items.Add(tsi);
                            }
                            else
                            {
                                var parent = new ToolStripMenuItem();
                                parent.Text = voiceLanguage.Key;
                                contextMenuStripActors.Items.Add(parent);

                                foreach (var voice in voiceLanguage.OrderBy(p => p.Voice).ToList())
                                {
                                    var tsi = new ToolStripMenuItem();
                                    tsi.Tag = new ActorAndVoice { Voice = voice.Voice, VoiceIndex = voices.IndexOf(voice) };
                                    tsi.Text = voice.Voice + " (" + voice.Gender + ")";
                                    tsi.Click += (x, args) =>
                                    {
                                        var a = (ActorAndVoice)(x as ToolStripItem).Tag;
                                        SetActor(a);
                                    };
                                    parent.DropDownItems.Add(tsi);
                                }

                                if (Configuration.Settings.General.UseDarkTheme)
                                {
                                    DarkTheme.SetDarkTheme(parent);
                                }
                            }
                        }
                    }
                    else if (engine.Id == TextToSpeechEngineId.AllTalk)
                    {
                        var tsiList = new List<ToolStripItem>(nikseComboBoxVoice.Items.Count);
                        for (var index = 0; index < nikseComboBoxVoice.Items.Count; index++)
                        {
                            var item = nikseComboBoxVoice.Items[index];

                            var tsi = new ToolStripMenuItem();
                            tsi.Tag = new ActorAndVoice { Voice = item.ToString(), VoiceIndex = index };
                            tsi.Text = item.ToString();
                            tsi.Click += (x, args) =>
                            {
                                var a = (ActorAndVoice)(x as ToolStripItem).Tag;
                                SetActor(a);
                            };
                            tsiList.Add(tsi);
                        }

                        contextMenuStripActors.Items.AddRange(tsiList.ToArray());
                    }
                    else
                    {
                        var tsiList = new List<ToolStripItem>(nikseComboBoxVoice.Items.Count);
                        for (var index = 0; index < nikseComboBoxVoice.Items.Count; index++)
                        {
                            var item = nikseComboBoxVoice.Items[index];

                            var tsi = new ToolStripMenuItem();
                            tsi.Tag = new ActorAndVoice { Voice = item.ToString(), VoiceIndex = index };
                            tsi.Text = item.ToString();
                            tsi.Click += (x, args) =>
                            {
                                var a = (ActorAndVoice)(x as ToolStripItem).Tag;
                                SetActor(a);
                            };
                            tsiList.Add(tsi);
                        }

                        contextMenuStripActors.Items.AddRange(tsiList.ToArray());
                    }

                    labelActors.Visible = true;
                    listViewActors.Visible = true;
                    _actorsOn = true;
                }
            }
        }

        private void SetFirstLanguageHitAsVoice()
        {
            nikseComboBoxVoice.Text = Configuration.Settings.Tools.TextToSpeechLastVoice;
            if (nikseComboBoxVoice.Text == Configuration.Settings.Tools.TextToSpeechLastVoice &&
                !string.IsNullOrEmpty(Configuration.Settings.Tools.TextToSpeechLastVoice))
            {
                return;
            }

            var language = LanguageAutoDetect.AutoDetectGoogleLanguage(_subtitle);
            for (var index = 0; index < nikseComboBoxVoice.Items.Count; index++)
            {
                var item = (string)nikseComboBoxVoice.Items[index];
                if (item.StartsWith(language, StringComparison.OrdinalIgnoreCase))
                {
                    nikseComboBoxVoice.SelectedIndex = index;
                    return;
                }
            }

            nikseComboBoxVoice.SelectedIndex = 0;
        }

        private List<PiperModel> GetPiperVoices(bool useCache)
        {
            var ttsPath = Path.Combine(Configuration.DataDirectory, "TextToSpeech");
            if (!Directory.Exists(ttsPath))
            {
                Directory.CreateDirectory(ttsPath);
            }

            var elevenLabsPath = Path.Combine(ttsPath, "Piper");
            if (!Directory.Exists(elevenLabsPath))
            {
                Directory.CreateDirectory(elevenLabsPath);
            }

            var result = new List<PiperModel>();

            var jsonFileName = Path.Combine(elevenLabsPath, "voices.json");

            if (!File.Exists(jsonFileName))
            {
                var asm = System.Reflection.Assembly.GetExecutingAssembly();
                var stream = asm.GetManifestResourceStream("Nikse.SubtitleEdit.Resources.PiperVoices.zip");
                if (stream != null)
                {
                    using (var zip = ZipExtractor.Open(stream))
                    {
                        var dir = zip.ReadCentralDir();
                        foreach (var entry in dir)
                        {
                            var fileName = Path.GetFileName(entry.FilenameInZip);
                            if (!string.IsNullOrEmpty(fileName))
                            {
                                var name = entry.FilenameInZip;
                                var path = Path.Combine(elevenLabsPath, name.Replace('/', Path.DirectorySeparatorChar));
                                zip.ExtractFile(entry, path);
                            }
                        }
                    }
                }
            }

            if (!useCache)
            {
                var httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/json");
                httpClient.DefaultRequestHeaders.TryAddWithoutValidation("accept", "application/json");
                var url = "https://huggingface.co/rhasspy/piper-voices/resolve/main/voices.json?download=true";
                var res = httpClient.GetAsync(new Uri(url), CancellationToken.None).Result;
                var bytes = res.Content.ReadAsByteArrayAsync().Result;

                if (!res.IsSuccessStatusCode)
                {
                    var error = Encoding.UTF8.GetString(bytes).Trim();
                    SeLogger.Error($"Failed getting voices form Piper via url \"{url}\" : Status code={res.StatusCode} {error}");
                    MessageBox.Show(this, "Calling url: " + url + Environment.NewLine + "Got error: " + error + " " + result);
                    return result;
                }

                File.WriteAllBytes(jsonFileName, bytes);
            }

            if (File.Exists(jsonFileName))
            {
                var json = File.ReadAllText(jsonFileName);
                var parser = new SeJsonParser();
                var arr = parser.GetRootElements(json);

                foreach (var element in arr)
                {
                    var elements = parser.GetRootElements(element.Json);
                    var name = elements.FirstOrDefault(p => p.Name == "name");
                    var quality = elements.FirstOrDefault(p => p.Name == "quality");
                    var language = elements.FirstOrDefault(p => p.Name == "language");
                    var files = elements.FirstOrDefault(p => p.Name == "files");

                    if (name != null && quality != null && language != null && files != null)
                    {
                        var languageDisplay = parser.GetFirstObject(language.Json, "name_english");
                        var languageFamily = parser.GetFirstObject(language.Json, "family");
                        var languageCode = parser.GetFirstObject(language.Json, "code");

                        var filesElements = parser.GetRootElements(files.Json);
                        var model = filesElements.FirstOrDefault(p => p.Name.EndsWith(".onnx"));
                        var config = filesElements.FirstOrDefault(p => p.Name.EndsWith("onnx.json"));
                        if (model != null && config != null)
                        {
                            var modelUrl = "https://huggingface.co/rhasspy/piper-voices/resolve/v1.0.0/" + model.Name;
                            var configUrl = "https://huggingface.co/rhasspy/piper-voices/resolve/v1.0.0/" + config.Name;
                            result.Add(new PiperModel(name.Json, languageDisplay, quality.Json, modelUrl, configUrl));
                        }
                    }
                }
            }

            return result;
        }

        private List<ElevenLabModel> GetElevenLabVoices(bool useCache)
        {
            var ttsPath = Path.Combine(Configuration.DataDirectory, "TextToSpeech");
            if (!Directory.Exists(ttsPath))
            {
                Directory.CreateDirectory(ttsPath);
            }

            var elevenLabsPath = Path.Combine(ttsPath, "ElevenLabs");
            if (!Directory.Exists(elevenLabsPath))
            {
                Directory.CreateDirectory(elevenLabsPath);
            }

            var result = new List<ElevenLabModel>();

            var jsonFileName = Path.Combine(elevenLabsPath, "eleven-labs-voices.json");

            if (!File.Exists(jsonFileName))
            {
                var asm = System.Reflection.Assembly.GetExecutingAssembly();
                var stream = asm.GetManifestResourceStream("Nikse.SubtitleEdit.Resources.eleven-labs-voices.zip");
                if (stream != null)
                {
                    using (var zip = ZipExtractor.Open(stream))
                    {
                        var dir = zip.ReadCentralDir();
                        foreach (var entry in dir)
                        {
                            var fileName = Path.GetFileName(entry.FilenameInZip);
                            if (!string.IsNullOrEmpty(fileName))
                            {
                                var name = entry.FilenameInZip;
                                var path = Path.Combine(elevenLabsPath, name.Replace('/', Path.DirectorySeparatorChar));
                                zip.ExtractFile(entry, path);
                            }
                        }
                    }
                }
            }

            if (!useCache)
            {
                var httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/json");
                httpClient.DefaultRequestHeaders.TryAddWithoutValidation("accept", "application/json");

                if (!string.IsNullOrWhiteSpace(nikseTextBoxApiKey.Text))
                {
                    httpClient.DefaultRequestHeaders.TryAddWithoutValidation("xi-api-key", nikseTextBoxApiKey.Text.Trim());
                }

                var url = "https://api.elevenlabs.io/v1/voices";
                var res = httpClient.GetAsync(new Uri(url), CancellationToken.None).Result;
                var bytes = res.Content.ReadAsByteArrayAsync().Result;

                if (!res.IsSuccessStatusCode)
                {
                    Cursor = Cursors.Default;
                    var error = Encoding.UTF8.GetString(bytes).Trim();
                    SeLogger.Error($"Failed getting voices form ElevenLabs via url \"{url}\" : Status code={res.StatusCode} {error}");
                    MessageBox.Show(this, "Calling url: " + url + Environment.NewLine + "Got error: " + error);
                    return new List<ElevenLabModel>();
                }

                File.WriteAllBytes(jsonFileName, bytes);
            }

            if (File.Exists(jsonFileName))
            {
                var json = File.ReadAllText(jsonFileName);
                var parser = new SeJsonParser();
                var voices = parser.GetArrayElementsByName(json, "voices");
                foreach (var voice in voices)
                {
                    var name = parser.GetFirstObject(voice, "name");
                    var voiceId = parser.GetFirstObject(voice, "voice_id");
                    var gender = parser.GetFirstObject(voice, "gender");
                    var description = parser.GetFirstObject(voice, "description");
                    var accent = parser.GetFirstObject(voice, "accent");
                    var useCase = parser.GetFirstObject(voice, "use case");
                    result.Add(new ElevenLabModel(string.Empty, name, gender, description, useCase, accent, voiceId));
                }
            }

            return result;
        }

        private List<string> GetAllTalkVoices(bool useCache)
        {
            var ttsPath = Path.Combine(Configuration.DataDirectory, "TextToSpeech");
            if (!Directory.Exists(ttsPath))
            {
                Directory.CreateDirectory(ttsPath);
            }

            var allTalkPath = Path.Combine(ttsPath, "AllTalk");
            if (!Directory.Exists(allTalkPath))
            {
                Directory.CreateDirectory(allTalkPath);
            }

            var result = new List<string>();

            var jsonFileName = Path.Combine(allTalkPath, "AllTalkVoices.json");

            if (!File.Exists(jsonFileName))
            {
                var asm = System.Reflection.Assembly.GetExecutingAssembly();
                var stream = asm.GetManifestResourceStream("Nikse.SubtitleEdit.Resources.AllTalkVoices.zip");
                if (stream != null)
                {
                    using (var zip = ZipExtractor.Open(stream))
                    {
                        var dir = zip.ReadCentralDir();
                        foreach (var entry in dir)
                        {
                            var fileName = Path.GetFileName(entry.FilenameInZip);
                            if (!string.IsNullOrEmpty(fileName))
                            {
                                var name = entry.FilenameInZip;
                                var path = Path.Combine(allTalkPath, name.Replace('/', Path.DirectorySeparatorChar));
                                zip.ExtractFile(entry, path);
                            }
                        }
                    }
                }
            }

            if (!useCache)
            {
                var httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/json");
                httpClient.DefaultRequestHeaders.TryAddWithoutValidation("accept", "application/json");

                var url = "http://127.0.0.1:7851/api/voices";
                var res = httpClient.GetAsync(new Uri(url), CancellationToken.None).Result;
                var bytes = res.Content.ReadAsByteArrayAsync().Result;

                if (!res.IsSuccessStatusCode)
                {
                    Cursor = Cursors.Default;
                    var error = Encoding.UTF8.GetString(bytes).Trim();
                    SeLogger.Error($"Failed getting voices form AllTalk via url \"{url}\" : Status code={res.StatusCode} {error}");
                    MessageBox.Show(this, "Calling url: " + url + Environment.NewLine + "Got error: " + error);
                    return new List<string>();
                }

                File.WriteAllBytes(jsonFileName, bytes);
            }

            if (File.Exists(jsonFileName))
            {
                var json = File.ReadAllText(jsonFileName);
                var parser = new SeJsonParser();
                var voices = parser.GetArrayElementsByName(json, "voices");
                foreach (var voice in voices)
                {
                    result.Add(voice);
                }
            }

            return result;
        }

        private bool SubtitleFormatHasActors()
        {
            var formatType = _subtitleFormat.GetType();
            return formatType == typeof(AdvancedSubStationAlpha) ||
                   formatType == typeof(SubStationAlpha) ||
                   formatType == typeof(WebVTTFileWithLineNumber) ||
                   formatType == typeof(WebVTT);
        }

        private void TextToSpeech_ResizeEnd(object sender, EventArgs e)
        {
            listViewActors.AutoSizeLastColumn();

            nikseComboBoxEngine.DropDownWidth = 0;
            nikseComboBoxVoice.DropDownWidth = 0;
        }

        private void TextToSpeech_Load(object sender, EventArgs e)
        {
            TextToSpeech_ResizeEnd(null, null);
        }

        private void TextToSpeech_SizeChanged(object sender, EventArgs e)
        {
            listViewActors.AutoSizeLastColumn();
        }

        public FileNameAndSpeedFactor ReGenerateAudio(Paragraph p, Paragraph next, string voice)
        {
            nikseComboBoxVoice.Text = voice;
            var sub = new Subtitle();
            sub.Paragraphs.Add(p);

            if (next != null)
            {
                var nextEmpty = new Paragraph(next) { Text = string.Empty };
                sub.Paragraphs.Add(nextEmpty);
            }

            var waveFileNameOnly = Guid.NewGuid() + GetEngineAudioExtension();
            var ok = GenerateParagraphAudio(sub, false, waveFileNameOnly);
            if (!ok)
            {
                return null;
            }

            var fileNameAndSpeedFactors = FixParagraphAudioSpeed(sub, waveFileNameOnly);

            return fileNameAndSpeedFactors.First();
        }

        private string GetEngineAudioExtension()
        {
            var engine = _engines.First(p => p.Index == nikseComboBoxEngine.SelectedIndex);
            if (engine.Id == TextToSpeechEngineId.ElevenLabs || engine.Id == TextToSpeechEngineId.AzureTextToSpeech)
            {
                return ".mp3";
            }

            return ".wav";
        }

        private void buttonTestVoice_Click(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(TextBoxTest.Text))
                {
                    return;
                }

                buttonTestVoice.Enabled = false;
                _waveFolder = Path.GetTempPath();
                var text = TextBoxTest.Text;
                var sub = new Subtitle();
                sub.Paragraphs.Add(new Paragraph(text, 0, 2500));
                var waveFileNameOnly = Guid.NewGuid() + ".wav";
                var ok = GenerateParagraphAudio(sub, false, waveFileNameOnly);
                if (!ok)
                {
                    MessageBox.Show(this, "Ups, voice generation failed!");
                    return;
                }

                var waveFileName = Path.Combine(_waveFolder, waveFileNameOnly);
                if (!File.Exists(waveFileName))
                {
                    var mp3FileName = waveFileName.Replace(".wav", ".mp3");
                    if (File.Exists(mp3FileName))
                    {
                        var process = VideoPreviewGenerator.ConvertFormat(mp3FileName, waveFileName);
                        process.Start();
                        process.WaitForExit();
                    }
                }

                using (var soundPlayer = new System.Media.SoundPlayer(waveFileName))
                {
                    soundPlayer.Play();
                }

                TaskDelayHelper.RunDelayed(TimeSpan.FromSeconds(30), () =>
                {
                    SafeFileDelete(waveFileName);
                });
            }
            catch (Exception ex)
            {
                HandleError(ex);
            }
            finally
            {
                TaskDelayHelper.RunDelayed(TimeSpan.FromSeconds(1), () => buttonTestVoice.Enabled = true);
            }
        }

        private void HandleError(Exception ex)
        {
            var engine = _engines.First(p => p.Index == nikseComboBoxEngine.SelectedIndex);
            var msg = "An error occurred while trying to generate speech.";
            if (engine.Id == TextToSpeechEngineId.Coqui)
            {
                msg += Environment.NewLine + "Make sure you have started the Coqui-ai TTS web server locally";
            }

            MessageBox.Show(this, msg + Environment.NewLine + Environment.NewLine + ex.Message + Environment.NewLine + ex.StackTrace);
        }

        private void TextToSpeech_FormClosing(object sender, FormClosingEventArgs e)
        {
            var engine = _engines.First(p => p.Index == nikseComboBoxEngine.SelectedIndex);

            if (engine.Id == TextToSpeechEngineId.ElevenLabs)
            {
                Configuration.Settings.Tools.TextToSpeechElevenLabsApiKey = nikseTextBoxApiKey.Text;
            }
            else if (engine.Id == TextToSpeechEngineId.AzureTextToSpeech)
            {
                Configuration.Settings.Tools.TextToSpeechAzureApiKey = nikseTextBoxApiKey.Text;
                Configuration.Settings.Tools.TextToSpeechAzureRegion = nikseComboBoxRegion.Text;
            }

            Configuration.Settings.Tools.TextToSpeechEngine = engine.Id.ToString();
            Configuration.Settings.Tools.TextToSpeechLastVoice = nikseComboBoxVoice.Text;
            Configuration.Settings.Tools.TextToSpeechAddToVideoFile = checkBoxAddToVideoFile.Checked;
            Configuration.Settings.Tools.TextToSpeechPreview = checkBoxShowPreview.Checked;
        }

        private void TextToSpeech_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyData == UiUtil.HelpKeys)
            {
                UiUtil.ShowHelp("#text_to_speech");
                e.SuppressKeyPress = true;
            }
        }

        private void TextBoxTest_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                buttonTestVoice_Click(null, null);
            }
        }

        public string GetParagraphAudio(Paragraph paragraph)
        {
            if (_actorsOn && _actorAndVoices.Count > 0 && !string.IsNullOrEmpty(paragraph.Actor))
            {
                var f = _actorAndVoices.FirstOrDefault(x => x.Actor == paragraph.Actor);
                if (f != null && !string.IsNullOrEmpty(f.Voice))
                {
                    var engine = _engines.First(p => p.Index == nikseComboBoxEngine.SelectedIndex);

                    if (engine.Id == TextToSpeechEngineId.Piper)
                    {
                        return _piperVoices[f.VoiceIndex].ToString();
                    }

                    if (engine.Id == TextToSpeechEngineId.AzureTextToSpeech)
                    {
                        return _azureVoices[f.VoiceIndex].ToString();
                    }

                    if (engine.Id == TextToSpeechEngineId.ElevenLabs)
                    {
                        return _elevenLabVoices[f.VoiceIndex].ToString();
                    }
                }
            }

            return nikseComboBoxVoice.Text;
        }

        public void SetCurrentVoices(NikseComboBox nikseComboBox)
        {
            nikseComboBox.Items.Clear();
            foreach (var voice in nikseComboBoxVoice.Items)
            {
                nikseComboBox.Items.Add(voice.ToString());
            }

            nikseComboBox.SelectedIndex = nikseComboBoxVoice.SelectedIndex;
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            if (_converting)
            {
                _abort = true;
                return;
            }

            DialogResult = DialogResult.Cancel;
        }

        private void TextToSpeech_Shown(object sender, EventArgs e)
        {
            nikseComboBoxEngine.DropDownWidth = nikseComboBoxEngine.Width;
            nikseComboBoxVoice.DropDownWidth = nikseComboBoxVoice.Width;
        }

        private bool RefreshVoices()
        {
            if (nikseTextBoxApiKey.Visible && string.IsNullOrWhiteSpace(nikseTextBoxApiKey.Text))
            {
                Cursor = Cursors.Default;
                MessageBox.Show("Please add API key");
                nikseTextBoxApiKey.Focus();
                return false;
            }

            var engine = _engines.First(p => p.Index == nikseComboBoxEngine.SelectedIndex);
            if (engine.Id == TextToSpeechEngineId.AzureTextToSpeech)
            {
                if (string.IsNullOrWhiteSpace(nikseComboBoxRegion.Text))
                {
                    Cursor = Cursors.Default;
                    MessageBox.Show("Please add region");
                    nikseComboBoxRegion.Focus();
                    return false;
                }

                var _ = GetAzureVoices(false);
                nikseComboBoxEngine_SelectedIndexChanged(null, null);
            }
            else if (engine.Id == TextToSpeechEngineId.ElevenLabs)
            {
                GetElevenLabVoices(false);
                nikseComboBoxEngine_SelectedIndexChanged(null, null);
            }
            else if (engine.Id == TextToSpeechEngineId.AllTalk)
            {
                GetAllTalkVoices(false);
                nikseComboBoxEngine_SelectedIndexChanged(null, null);
            }

            return true;
        }

        private void contextMenuStripVoices_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            var engine = _engines.First(p => p.Index == nikseComboBoxEngine.SelectedIndex);
            if (engine.Id == TextToSpeechEngineId.AzureTextToSpeech ||
                engine.Id == TextToSpeechEngineId.ElevenLabs ||
                engine.Id == TextToSpeechEngineId.Piper ||
                engine.Id == TextToSpeechEngineId.AllTalk)
            {
                return;
            }

            e.Cancel = true;
        }

        private void refreshVoicesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var dr = MessageBox.Show(this, "Download updated voice list?", "Update voices", MessageBoxButtons.YesNoCancel);
            if (dr != DialogResult.Yes)
            {
                return;
            }

            try
            {
                Cursor = Cursors.WaitCursor;
                var ok = RefreshVoices();
                Cursor = Cursors.Default;
                if (ok)
                {
                    MessageBox.Show(this, "Voice list updated :)");
                }
            }
            catch (Exception ex)
            {
                Cursor = Cursors.Default;
                MessageBox.Show(this, "Voice list download failed!" + Environment.NewLine +
                                      Environment.NewLine +
                                      ex.Message);
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }
    }
}