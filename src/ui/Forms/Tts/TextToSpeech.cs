using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Speech.Synthesis;
using System.Windows.Forms;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Core.TextToSpeech;
using Nikse.SubtitleEdit.Logic;
using MessageBox = Nikse.SubtitleEdit.Forms.SeMsgBox.MessageBox;

namespace Nikse.SubtitleEdit.Forms.Tts
{
    public partial class TextToSpeech : Form
    {
        private readonly Subtitle _subtitle;
        private readonly string _videoFileName;
        private readonly VideoInfo _videoInfo;
        private string _waveFolder;
        private readonly List<ActorAndVoice> _actorAndVoices;
        private readonly SubtitleFormat _subtitleFormat;

        public class ActorAndVoice
        {
            public string Actor { get; set; }
            public int UseCount { get; set; }
            public string Voice { get; set; }
            public int VoiceIndex { get; set; }
        }

        public TextToSpeech(Subtitle subtitle, SubtitleFormat subtitleFormat, string videoFileName, VideoInfo videoInfo)
        {
            UiUtil.PreInitialize(this);
            InitializeComponent();
            UiUtil.FixFonts(this);

            _subtitle = subtitle;
            _subtitleFormat = subtitleFormat;
            _videoFileName = videoFileName;
            _videoInfo = videoInfo;

            buttonOK.Text = LanguageSettings.Current.General.Ok;
            UiUtil.FixLargeFonts(this, buttonOK);

            progressBar1.Visible = false;
            labelProgress.Text = string.Empty;

            _actorAndVoices = new List<ActorAndVoice>();
            nikseComboBoxEngine.DropDownStyle = ComboBoxStyle.DropDownList;
            nikseComboBoxEngine.Items.Clear();
            nikseComboBoxEngine.Items.Add("Microsoft SpeechSynthesizer (very fast/robotic)");
            nikseComboBoxEngine.Items.Add("Piper (fast/good)");
            nikseComboBoxEngine.Items.Add("Tortoise TTS (very slow/very good)");
            //nikseComboBoxEngine.Items.Add("Mimic3");
            nikseComboBoxEngine.SelectedIndex = 0;

            listView1.Visible = false;
            nikseComboBoxEngine_SelectedIndexChanged(null, null);
        }

        private void SetActor(ActorAndVoice actor)
        {
            foreach (int index in listView1.SelectedIndices)
            {
                ListViewItem item = listView1.Items[index];
                var itemActor = (ActorAndVoice)item.Tag;
                itemActor.Voice = actor.Voice;
                itemActor.VoiceIndex = actor.VoiceIndex;
                item.SubItems[1].Text = actor.Voice;
            }
        }

        private void FillActorListView()
        {
            listView1.BeginUpdate();
            listView1.Items.Clear();
            foreach (var actor in _actorAndVoices)
            {
                var lvi = new ListViewItem
                {
                    Tag = actor,
                    Text = actor.Actor,
                };
                lvi.SubItems.Add(actor.Voice);
                listView1.Items.Add(lvi);
            }

            listView1.EndUpdate();
        }

        private void ButtonGenerateTtsClick(object sender, EventArgs e)
        {
            _waveFolder = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(_waveFolder);

            GenerateParagraphAudio(_subtitle, true);

            var fileNames = FixParagraphAudioSpeed();

            var tempAudioFile = MergeAudioParagraphs(fileNames);

            // rename result file
            var resultAudioFileName = Path.Combine(Path.GetDirectoryName(tempAudioFile), Path.GetFileNameWithoutExtension(_videoFileName) + ".wav");
            File.Move(tempAudioFile, resultAudioFileName);

            Cleanup(_waveFolder, resultAudioFileName);

            if (checkBoxAddToVideoFile.Checked)
            {
                AddAudioToVideoFile(resultAudioFileName);
            }

            UiUtil.OpenFolder(_waveFolder);
        }

        private void GenerateParagraphAudio(Subtitle subtitle, bool showProgressBar)
        {
            if (nikseComboBoxEngine.SelectedIndex == 0)
            {
                GenerateParagraphAudioMs(subtitle, showProgressBar);
            }
            else if (nikseComboBoxEngine.SelectedIndex == 1)
            {
                GenerateParagraphAudioPiperTts(subtitle, showProgressBar);
            }
            else if (nikseComboBoxEngine.SelectedIndex == 2)
            {
                GenerateParagraphAudioTortoiseTts(subtitle, showProgressBar);
            }
            else if (nikseComboBoxEngine.SelectedIndex == 3)
            {
                GenerateParagraphAudioMimic3(subtitle, showProgressBar);
            }
        }

        private void AddAudioToVideoFile(string audioFileName)
        {
            var videoExt = ".mkv";
            if (_videoFileName.EndsWith(".mp4", StringComparison.OrdinalIgnoreCase))
            {
                videoExt = ".mp4";
            }

            labelProgress.Text = "Add audtio to video file...";
            var outputFileName = Path.Combine(_waveFolder, Path.GetFileNameWithoutExtension(audioFileName) + videoExt);
            var addAudioProcess = VideoPreviewGenerator.AddAudioTrack(_videoFileName, audioFileName, outputFileName);
            addAudioProcess.Start();
            addAudioProcess.WaitForExit();

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

        private List<string> FixParagraphAudioSpeed()
        {
            var fileNames = new List<string>(_subtitle.Paragraphs.Count);

            labelProgress.Text = "Adjusting speed...";
            labelProgress.Refresh();
            Application.DoEvents();

            progressBar1.Value = 0;
            progressBar1.Maximum = _subtitle.Paragraphs.Count;
            progressBar1.Visible = true;
            for (var index = 0; index < _subtitle.Paragraphs.Count; index++)
            {
                progressBar1.Value = index + 1;
                labelProgress.Text = $"Adjusting speed: {index + 1} / {_subtitle.Paragraphs.Count}...";
                var p = _subtitle.Paragraphs[index];
                var next = _subtitle.GetParagraphOrDefault(index + 1);
                var pFileName = Path.Combine(_waveFolder, index + ".wav");

                //TODO: analyse audio and remove silence at start and end (ffmpeg -af silenceremove=1:0:-50dB:1:1:-50dB)
                var outputFileName1 = Path.Combine(_waveFolder, index + "_u.wav");
                var trimProcess = VideoPreviewGenerator.TrimSilenceStartAndEnd(pFileName, outputFileName1);
                trimProcess.Start();
                trimProcess.WaitForExit();

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

                var waveInfo = UiUtil.GetVideoInfo(outputFileName1);
                if (waveInfo.TotalMilliseconds <= p.DurationTotalMilliseconds + addDuration)
                {
                    fileNames.Add(outputFileName1);
                    continue;
                }

                var factor = waveInfo.TotalMilliseconds / (p.DurationTotalMilliseconds + addDuration);
                var outputFileName2 = Path.Combine(_waveFolder, index + "_t.wav");
                fileNames.Add(outputFileName2);
                var mergeProcess = VideoPreviewGenerator.ChangeSpeed(outputFileName1, outputFileName2, (float)factor);
                mergeProcess.Start();
                mergeProcess.WaitForExit();
            }

            return fileNames;
        }

        private string MergeAudioParagraphs(List<string> fileNames)
        {
            labelProgress.Text = "Merging audio track...";
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
                labelProgress.Text = $"Merging audio track: {index + 1} / {_subtitle.Paragraphs.Count}...";
                var p = _subtitle.Paragraphs[index];
                var pFileName = fileNames[index];
                if (!File.Exists(pFileName))
                {
                    SeLogger.Error($"TextToSpeech: File not found (skipping): {pFileName}");
                    continue;
                }

                outputFileName = Path.Combine(_waveFolder, $"silence{index}.wav");
                var mergeProcess = VideoPreviewGenerator.MergeAudioTracks(inputFileName, pFileName, outputFileName, (float)p.StartTime.TotalSeconds);
                inputFileName = outputFileName;
                mergeProcess.Start();
                mergeProcess.WaitForExit();
            }

            progressBar1.Visible = false;
            labelProgress.Text = string.Empty;

            return outputFileName;
        }

        private void GenerateParagraphAudioMs(Subtitle subtitle, bool showProgressBar)
        {
            progressBar1.Value = 0;
            progressBar1.Maximum = subtitle.Paragraphs.Count;
            progressBar1.Visible = showProgressBar;

            using (var synthesizer = new SpeechSynthesizer())
            {
                VoiceInfo voiceInfo = null;
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
                        labelProgress.Text = $"Generating audio texts: {index + 1} / {subtitle.Paragraphs.Count}...";
                    }

                    var p = subtitle.Paragraphs[index];
                    synthesizer.SetOutputToWaveFile(Path.Combine(_waveFolder, index + ".wav"));
                    var builder = new PromptBuilder();
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

        private bool GenerateParagraphAudioPiperTts(Subtitle subtitle, bool showProgressBar)
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

            if (!File.Exists(piperExe))
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
                    labelProgress.Text = $"Generating audio texts: {index + 1} / {subtitle.Paragraphs.Count}...";
                }

                var p = subtitle.Paragraphs[index];
                var outputFileName = Path.Combine(_waveFolder, index + ".wav");
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
                    using (var form = new PiperDownload("Piper TextToSpeech Voice") { AutoClose = true, ModelUrl = voice.Model, ModelFileName = modelFileName, PiperPath = piperPath })
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
                    using (var form = new PiperDownload("Piper TextToSpeech Voice") { AutoClose = true, ModelUrl = voice.Config, ModelFileName = configFileName, PiperPath = piperPath })
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
                        FileName = piperExe,
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
                processPiper.WaitForExit();

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

        private bool GenerateParagraphAudioTortoiseTts(Subtitle subtitle, bool showProgressBar)
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
                MessageBox.Show($"{pyFileName} not found under {pythonFolder}");
                return false;
            }

            var pythonFolderVersionFolder = files[0].Substring(pythonFolder.Length).Split(new char[] { Path.DirectorySeparatorChar }, StringSplitOptions.RemoveEmptyEntries);
            var pythonExe = Path.Combine(pythonFolder, pythonFolderVersionFolder[0], "python.exe");
            if (!File.Exists(pythonExe))
            {
                pythonExe = "python.exe";
            }

            progressBar1.Value = 0;
            progressBar1.Maximum = subtitle.Paragraphs.Count;
            progressBar1.Visible = showProgressBar;

            for (var index = 0; index < subtitle.Paragraphs.Count; index++)
            {
                if (showProgressBar)
                {
                    progressBar1.Value = index + 1;
                    labelProgress.Text = $"Generating audio texts: {index + 1} / {subtitle.Paragraphs.Count}...";
                }

                var p = subtitle.Paragraphs[index];
                var outputFileName = Path.Combine(_waveFolder, index + ".wav");

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
                        Arguments = $"do_tts.py --output_path \"{_waveFolder}\" --preset ultra_fast --voice {v} --text \"{p.Text.RemoveChar('"')}\"",
                        UseShellExecute = false,
                        CreateNoWindow = true,
                    }
                };

                processTortoiseTts.Start();
                processTortoiseTts.WaitForExit();

                var inputFile = Path.Combine(_waveFolder, $"{v}_0_2.wav");
                File.Move(inputFile, outputFileName);

                progressBar1.Refresh();
                labelProgress.Refresh();
                Application.DoEvents();
            }

            progressBar1.Visible = false;
            labelProgress.Text = string.Empty;

            return true;
        }

        private void GenerateParagraphAudioMimic3(Subtitle subtitle, bool showProgressBar)
        {
            throw new NotImplementedException();
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
        }

        private void nikseComboBoxEngine_SelectedIndexChanged(object sender, EventArgs e)
        {
            nikseComboBoxVoice.Items.Clear();

            if (nikseComboBoxEngine.SelectedIndex == 0)
            {
                using (var synthesizer = new SpeechSynthesizer())
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

            if (nikseComboBoxEngine.SelectedIndex == 1) // Piper
            {
                foreach (var voice in PiperModels.GetVoices())
                {
                    nikseComboBoxVoice.Items.Add(voice.ToString());
                }
            }

            if (nikseComboBoxEngine.SelectedIndex == 2) // Tortoise TTS
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

            if (nikseComboBoxVoice.Items.Count > 0)
            {
                nikseComboBoxVoice.SelectedIndex = 0;
            }

            _actorAndVoices.Clear();

            if (_subtitleFormat.GetType() == typeof(AdvancedSubStationAlpha))
            {
                var actors = _subtitle.Paragraphs
                    .Where(p => !string.IsNullOrEmpty(p.Actor))
                    .Select(p => p.Actor)
                    .Distinct()
                    .ToList();
                if (actors.Any())
                {
                    foreach (var actor in actors)
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

                        listView1.Visible = true;
                    }
                }
            }
        }

        private void TextToSpeech_ResizeEnd(object sender, EventArgs e)
        {
            listView1.AutoSizeLastColumn();
        }

        private void TextToSpeech_Load(object sender, EventArgs e)
        {
            listView1.AutoSizeLastColumn();
        }

        private void TextToSpeech_SizeChanged(object sender, EventArgs e)
        {
            listView1.AutoSizeLastColumn();
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
                GenerateParagraphAudio(sub, false);
                var waveFileName = Path.Combine(_waveFolder, "0.wav");
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
                MessageBox.Show(ex.Message + Environment.NewLine + ex.StackTrace);
            }
            finally
            {
                buttonTestVoice.Enabled = true;
            }
        }
    }
}