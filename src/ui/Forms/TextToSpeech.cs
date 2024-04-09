using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Logic;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Speech.Synthesis;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.Forms
{
    public partial class TextToSpeech : Form
    {
        private readonly Subtitle _subtitle;
        private readonly string _videoFileName;
        private readonly VideoInfo _videoInfo;
        private string _waveFolder;

        public TextToSpeech(Subtitle subtitle, string videoFileName, VideoInfo videoInfo)
        {
            UiUtil.PreInitialize(this);
            InitializeComponent();
            UiUtil.FixFonts(this);

            _subtitle = subtitle;
            _videoFileName = videoFileName;
            _videoInfo = videoInfo;

            buttonOK.Text = LanguageSettings.Current.General.Ok;
            UiUtil.FixLargeFonts(this, buttonOK);

            progressBar1.Visible = false;
            labelProgress.Text = string.Empty;

            nikseComboBoxEngine.DropDownStyle = ComboBoxStyle.DropDownList;
            nikseComboBoxEngine.Items.Clear();
            nikseComboBoxEngine.Items.Add("Microsoft SpeechSynthesizer (fast/robotic)");
            nikseComboBoxEngine.Items.Add("Tortoise TTS (very slow/very good)");
            nikseComboBoxEngine.Items.Add("Mimic3");
            nikseComboBoxEngine.SelectedIndex = 0;
        }

        private void ButtonGenerateTtsClick(object sender, EventArgs e)
        {
            _waveFolder = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(_waveFolder);

            if (nikseComboBoxEngine.SelectedIndex == 0)
            {
                GenerateParagraphAudioMs();
            }
            else if (nikseComboBoxEngine.SelectedIndex == 1)
            {
                GenerateParagraphAudioTortoiseTts();
            }
            else if (nikseComboBoxEngine.SelectedIndex == 2)
            {
                GenerateParagraphAudioMimic3();
            }

            var fileNames = FixParagraphAudioSpeed();

            // rename result file
            var tempAudioFile = MergeAudioParagraphs(fileNames);
            var resultAudioFileName = Path.Combine(Path.GetDirectoryName(tempAudioFile), Path.GetFileNameWithoutExtension(_videoFileName) + ".wav");
            File.Move(tempAudioFile, resultAudioFileName);
            
            Cleanup(_waveFolder, resultAudioFileName);

            if (checkBoxAddToVideoFile.Checked)
            {
                AddAudioToVideoFile(resultAudioFileName);
            }

            UiUtil.OpenFolder(_waveFolder);
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
            Process addAudioProcess = VideoPreviewGenerator.AddAudioTrack(_videoFileName, audioFileName, outputFileName);
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

        private void GenerateParagraphAudioMs()
        {
            progressBar1.Value = 0;
            progressBar1.Maximum = _subtitle.Paragraphs.Count;
            progressBar1.Visible = true;

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

                for (var index = 0; index < _subtitle.Paragraphs.Count; index++)
                {
                    progressBar1.Value = index + 1;
                    labelProgress.Text = $"Generating audio texts: {index + 1} / {_subtitle.Paragraphs.Count}...";
                    var p = _subtitle.Paragraphs[index];
                    synthesizer.SetOutputToWaveFile(Path.Combine(_waveFolder, index + ".wav"));
                    var builder = new PromptBuilder();
                    if (voiceInfo != null)
                    {
                        builder.StartVoice(voiceInfo);
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

        private bool GenerateParagraphAudioTortoiseTts()
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
            progressBar1.Maximum = _subtitle.Paragraphs.Count;
            progressBar1.Visible = true;


            for (var index = 0; index < _subtitle.Paragraphs.Count; index++)
            {
                progressBar1.Value = index + 1;
                labelProgress.Text = $"Generating audio texts: {index + 1} / {_subtitle.Paragraphs.Count}...";
                var p = _subtitle.Paragraphs[index];
                var outputFileName = Path.Combine(_waveFolder, index + ".wav");

                var processTortoiseTts = new Process
                {
                    StartInfo =
                    {
                        WorkingDirectory = Path.GetDirectoryName(files[0]),
                        FileName = pythonExe,
                        Arguments = $"do_tts.py --output_path \"{_waveFolder}\" --preset ultra_fast --voice {voice} --text \"{p.Text.RemoveChar('"')}\"",
                        UseShellExecute = false,
                        CreateNoWindow = true,
                    }
                };

                processTortoiseTts.Start();
                processTortoiseTts.WaitForExit();

                var inputFile = Path.Combine(_waveFolder, $"{voice}_0_2.wav");
                File.Move(inputFile, outputFileName);

                progressBar1.Refresh();
                labelProgress.Refresh();
                Application.DoEvents();
            }

            progressBar1.Visible = false;
            labelProgress.Text = string.Empty;

            return true;
        }

        private void GenerateParagraphAudioMimic3()
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

            if (nikseComboBoxEngine.SelectedIndex == 1)
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
        }
    }
}