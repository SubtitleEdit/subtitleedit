using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Core.TextToSpeech;
using Nikse.SubtitleEdit.Logic;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Nikse.SubtitleEdit.Controls;
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
        private readonly List<ElevenLabModel> _elevenLabVoices;
        private bool _actorsOn;

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

        public enum TextToSpeechEngineId
        {
            Piper,
            Tortoise,
            Coqui,
            MsSpeechSynthesizer,
            ElevenLabs,
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
            _elevenLabVoices = new List<ElevenLabModel>();
            _actors = _subtitle.Paragraphs
                .Where(p => !string.IsNullOrEmpty(p.Actor))
                .Select(p => p.Actor)
                .Distinct()
                .ToList();

            Text = LanguageSettings.Current.TextToSpeech.Title;
            labelVoice.Text = LanguageSettings.Current.TextToSpeech.Voice;
            labelApiKey.Text = LanguageSettings.Current.VobSubOcr.ApiKey;
            buttonTestVoice.Text = LanguageSettings.Current.TextToSpeech.TestVoice;
            labelActors.Text = LanguageSettings.Current.TextToSpeech.ActorInfo;
            checkBoxAddToVideoFile.Text = LanguageSettings.Current.TextToSpeech.AddAudioToVideo;
            buttonGenerateTTS.Text = LanguageSettings.Current.TextToSpeech.GenerateSpeech;
            buttonOK.Text = LanguageSettings.Current.General.Ok;
            buttonCancel.Text = LanguageSettings.Current.General.Cancel;
            UiUtil.FixLargeFonts(this, buttonOK);

            progressBar1.Visible = false;
            labelProgress.Text = string.Empty;

            _engines = new List<TextToSpeechEngine>();
            _engines.Add(new TextToSpeechEngine(TextToSpeechEngineId.Piper, "Piper (fast/good)", _engines.Count));
            _engines.Add(new TextToSpeechEngine(TextToSpeechEngineId.Tortoise, "Tortoise TTS (very slow/good)", _engines.Count));
            _engines.Add(new TextToSpeechEngine(TextToSpeechEngineId.Coqui, "Coqui AI TTS (only one voice)", _engines.Count));
            if (Configuration.IsRunningOnWindows)
            {
                _engines.Add(new TextToSpeechEngine(TextToSpeechEngineId.MsSpeechSynthesizer, "Microsoft SpeechSynthesizer (very fast/robotic)", _engines.Count));
            }
            _engines.Add(new TextToSpeechEngine(TextToSpeechEngineId.ElevenLabs, "ElevenLabs TTS (online/pay/good)", _engines.Count));

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
            nikseComboBoxEngine_SelectedIndexChanged(null, null);

            if (!SubtitleFormatHasActors() || !_actors.Any())
            {
                var w = groupBoxMsSettings.Width + 100;
                var right = buttonOK.Right;
                groupBoxMsSettings.Left = progressBar1.Left;
                groupBoxMsSettings.Width = right - progressBar1.Left;
                groupBoxMsSettings.Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right | AnchorStyles.Bottom;

                Width = w;
                MinimumSize = new Size(w, MinimumSize.Height);
                Width = w;
            }

            nikseComboBoxVoice.Text = Configuration.Settings.Tools.TextToSpeechLastVoice;
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

        private async void ButtonGenerateTtsClick(object sender, EventArgs e)
        {
            if (buttonGenerateTTS.Text == LanguageSettings.Current.General.Cancel)
            {
                buttonGenerateTTS.Enabled = false;
                _abort = true;
                Application.DoEvents();
                return;
            }

            buttonGenerateTTS.Text = LanguageSettings.Current.General.Cancel;

            _waveFolder = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(_waveFolder);

            await GenerateParagraphAudio(_subtitle, true, null);
            if (_abort)
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
            var resultAudioFileName = Path.Combine(Path.GetDirectoryName(tempAudioFile), Path.GetFileNameWithoutExtension(_videoFileName) + ".wav");
            File.Move(tempAudioFile, resultAudioFileName);

            Cleanup(_waveFolder, resultAudioFileName);

            if (checkBoxAddToVideoFile.Checked)
            {
                AddAudioToVideoFile(resultAudioFileName);
                if (_abort)
                {
                    HandleAbort();
                    return;
                }
            }

            HandleAbort();
            UiUtil.OpenFolder(_waveFolder);
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
        }

        private async Task<bool> GenerateParagraphAudio(Subtitle subtitle, bool showProgressBar, string overrideFileName)
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
                var result = await GenerateParagraphAudioCoqui(subtitle, showProgressBar, overrideFileName);
                return result;
            }

            if (engine.Id == TextToSpeechEngineId.ElevenLabs)
            {
                var result = await GenerateParagraphAudioElevenLabs(subtitle, showProgressBar, overrideFileName);
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
                    File.Delete(fileName);
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

                if (!File.Exists(pFileName))
                {
                    pFileName = Path.Combine(_waveFolder, index + ".mp3");
                }

                var outputFileName1 = Path.Combine(_waveFolder, index + "_u.wav");
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
                var outputFileName2 = Path.Combine(_waveFolder, index + "_t.wav");
                if (!string.IsNullOrEmpty(overrideFileName) && File.Exists(Path.Combine(_waveFolder, overrideFileName)))
                {
                    outputFileName2 = Path.Combine(_waveFolder, Path.GetFileNameWithoutExtension(overrideFileName) + "_t.wav");
                }

                fileNames.Add(new FileNameAndSpeedFactor { Filename = outputFileName2, Factor = factor });
                var mergeProcess = VideoPreviewGenerator.ChangeSpeed(outputFileName1, outputFileName2, (float)factor);
                mergeProcess.Start();

                while (!mergeProcess.HasExited)
                {
                    Application.DoEvents();
                    if (_abort)
                    {
                        return new List<FileNameAndSpeedFactor>();
                    }
                }
            }

            return fileNames;
        }

        private string MergeAudioParagraphs(List<FileNameAndSpeedFactor> fileNames)
        {
            labelProgress.Text = string.Empty;
            labelProgress.Refresh();
            Application.DoEvents();
            var silenceFileName = Path.Combine(_waveFolder, "silence.wav");
            var silenceProcess = VideoPreviewGenerator.GenerateEmptyAudio(silenceFileName, (float)_videoInfo.TotalSeconds);
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
                if (!File.Exists(pFileName.Filename))
                {
                    SeLogger.Error($"TextToSpeech: File not found (skipping): {pFileName.Filename}");
                    continue;
                }

                outputFileName = Path.Combine(_waveFolder, $"silence{index}.wav");
                var mergeProcess = VideoPreviewGenerator.MergeAudioTracks(inputFileName, pFileName.Filename, outputFileName, (float)p.StartTime.TotalSeconds);
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

                    builder.AppendText(p.Text);
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
            var voices = PiperModels.GetVoices();

            for (var index = 0; index < subtitle.Paragraphs.Count; index++)
            {
                if (showProgressBar)
                {
                    progressBar1.Value = index + 1;
                    labelProgress.Text = string.Format(LanguageSettings.Current.TextToSpeech.GeneratingSpeechFromTextXOfY, index + 1, subtitle.Paragraphs.Count);
                }

                var p = subtitle.Paragraphs[index];
                var outputFileName = Path.Combine(_waveFolder, string.IsNullOrEmpty(overrideFileName) ? index + ".wav" : overrideFileName);
                if (File.Exists(outputFileName))
                {
                    try
                    {
                        File.Delete(outputFileName);
                    }
                    catch
                    {
                        // ignore
                    }
                }

                var voice = voices.First(x => x.ToString() == nikseComboBoxVoice.Text);
                if (_actorAndVoices.Count > 0 && !string.IsNullOrEmpty(p.Actor))
                {
                    var f = _actorAndVoices.FirstOrDefault(x => x.Actor == p.Actor);
                    if (f != null && !string.IsNullOrEmpty(f.Voice))
                    {
                        voice = voices[f.VoiceIndex];
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
                var streamWriter = processPiper.StandardInput;
                streamWriter.Write(p.Text);
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

                var processTortoiseTts = new Process
                {
                    StartInfo =
                    {
                        WorkingDirectory = Path.GetDirectoryName(files[0]),
                        FileName = pythonExe,
                        Arguments = $"do_tts.py --text \"{p.Text.RemoveChar('"')}\" --output_path \"{_waveFolder.TrimEnd(Path.DirectorySeparatorChar)}\" --preset ultra_fast --voice {v}",
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

        private async Task<bool> GenerateParagraphAudioCoqui(Subtitle subtitle, bool showProgressBar, string overrideFileName)
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
                var outputFileName = Path.Combine(_waveFolder, string.IsNullOrEmpty(overrideFileName) ? index + ".wav" : overrideFileName);

                var result = await httpClient.GetAsync("?text=" + Utilities.UrlEncode(p.Text));
                var bytes = await result.Content.ReadAsByteArrayAsync();

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

        private async Task<bool> GenerateParagraphAudioElevenLabs(Subtitle subtitle, bool showProgressBar, string overrideFileName)
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
                var data = "{ \"text\": \"" + Json.EncodeJsonText(p.Text) + "\", \"model_id\": \"eleven_monolingual_v1\", \"voice_settings\": { \"stability\": 0.5, \"similarity_boost\": 0.5 } }";
                var content = new StringContent(data, Encoding.UTF8);
                content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
                var result = await httpClient.PostAsync(url, content, CancellationToken.None);
                var bytes = await result.Content.ReadAsByteArrayAsync();

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

        private void buttonOK_Click(object sender, EventArgs e)
        {
            EditedSubtitle = _subtitle;
            DialogResult = DialogResult.OK;
        }

        private void nikseComboBoxEngine_SelectedIndexChanged(object sender, EventArgs e)
        {
            nikseComboBoxVoice.Items.Clear();

            labelApiKey.Visible = false;
            nikseTextBoxApiKey.Visible = false;

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
                foreach (var voice in PiperModels.GetVoices())
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

            if (engine.Id == TextToSpeechEngineId.ElevenLabs)
            {
                nikseTextBoxApiKey.Text = Configuration.Settings.Tools.TextToSpeechElevenLabsApiKey;

                labelApiKey.Visible = true;
                nikseTextBoxApiKey.Visible = true;

                if (_elevenLabVoices.Count == 0)
                {
                    _elevenLabVoices.AddRange(GetElevelLabVoices());
                }

                foreach (var voice in _elevenLabVoices)
                {
                    nikseComboBoxVoice.Items.Add(voice.ToString());
                }
            }

            if (nikseComboBoxVoice.Items.Count > 0)
            {
                nikseComboBoxVoice.SelectedIndex = 0;
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
                        var voices = PiperModels.GetVoices();
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

                                DarkTheme.SetDarkTheme(parent);
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

                                DarkTheme.SetDarkTheme(parent);
                            }
                        }
                    }
                    else
                    {
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
                            contextMenuStripActors.Items.Add(tsi);
                        }
                    }

                    labelActors.Visible = true;
                    listViewActors.Visible = true;
                    _actorsOn = true;
                }
            }
        }

        private List<ElevenLabModel> GetElevelLabVoices()
        {
            var ttsPath = Path.Combine(Configuration.DataDirectory, "TextToSpeech");
            if (!Directory.Exists(ttsPath))
            {
                Directory.CreateDirectory(ttsPath);
            }

            var elevelLabsPath = Path.Combine(ttsPath, "ElevenLabs");
            if (!Directory.Exists(elevelLabsPath))
            {
                Directory.CreateDirectory(elevelLabsPath);
            }

            var result = new List<ElevenLabModel>();

            var jsonFileName = Path.Combine(elevelLabsPath, "eleven-labs-voices.json");

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
                                var path = Path.Combine(elevelLabsPath, name.Replace('/', Path.DirectorySeparatorChar));
                                zip.ExtractFile(entry, path);
                            }
                        }
                    }
                }
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
        }

        private void TextToSpeech_Load(object sender, EventArgs e)
        {
            listViewActors.AutoSizeLastColumn();
        }

        private void TextToSpeech_SizeChanged(object sender, EventArgs e)
        {
            listViewActors.AutoSizeLastColumn();
        }

        public async Task<FileNameAndSpeedFactor> ReGenerateAudio(Paragraph p, string voice)
        {
            nikseComboBoxVoice.Text = voice;
            var sub = new Subtitle();
            sub.Paragraphs.Add(p);
            var waveFileNameOnly = Guid.NewGuid() + ".wav";
            var ok = await GenerateParagraphAudio(sub, false, waveFileNameOnly);
            if (!ok)
            {
                return null;
            }

            var fileNameAndSpeedFactors = FixParagraphAudioSpeed(sub, waveFileNameOnly);

            return fileNameAndSpeedFactors.First();
        }

        private async void buttonTestVoice_Click(object sender, EventArgs e)
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
                var ok = await GenerateParagraphAudio(sub, false, waveFileNameOnly);
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
                    try
                    {
                        File.Delete(waveFileName);
                    }
                    catch
                    {
                        // ignore
                    }
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

            Configuration.Settings.Tools.TextToSpeechEngine = engine.Id.ToString();
            Configuration.Settings.Tools.TextToSpeechLastVoice = nikseComboBoxVoice.Text;
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
            if (_actorsOn)
            {
                var engine = _engines.First(p => p.Index == nikseComboBoxEngine.SelectedIndex);

                if (engine.Id == TextToSpeechEngineId.Piper)
                {
                    var voices = PiperModels.GetVoices();
                    var voice = voices.First(x => x.ToString() == nikseComboBoxVoice.Text);
                    if (_actorAndVoices.Count > 0 && !string.IsNullOrEmpty(paragraph.Actor))
                    {
                        var f = _actorAndVoices.FirstOrDefault(x => x.Actor == paragraph.Actor);
                        if (f != null && !string.IsNullOrEmpty(f.Voice))
                        {
                            return voices[f.VoiceIndex].Voice;
                        }
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
            DialogResult = DialogResult.Cancel;
        }
    }
}