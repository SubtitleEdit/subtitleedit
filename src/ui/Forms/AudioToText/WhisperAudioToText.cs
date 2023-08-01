using Nikse.SubtitleEdit.Core.AudioToText;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.ContainerFormats.Matroska;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
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
        private List<ResultText> _resultList;
        private string _languageCode;
        private ConcurrentBag<string> _outputText = new ConcurrentBag<string>();
        private long _startTicks;
        private double _endSeconds;
        private double _showProgressPct = -1;
        private double _lastEstimatedMs = double.MaxValue;
        private VideoInfo _videoInfo;
        private readonly WavePeakData _wavePeaks;
        public bool UnknownArgument { get; set; }
        public bool IncompleteModel { get; set; }
        public string IncompleteModelName { get; set; }

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
            labelAdvanced.Text = Configuration.Settings.Tools.WhisperExtraSettings;

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

            Init();
            InitializeWhisperEngines(comboBoxWhisperEngine);
        }

        public static void InitializeWhisperEngines(ComboBox cb)
        {
            cb.Items.Clear();
            var engines = new List<string>();
            engines.Add(WhisperChoice.OpenAI);
            engines.Add(WhisperChoice.Cpp);
            if (Configuration.IsRunningOnWindows)
            {
                engines.Add(WhisperChoice.ConstMe);
                engines.Add(WhisperChoice.PurfviewFasterWhisper);
            }
            engines.Add(WhisperChoice.CTranslate2);
          //  engines.Add(WhisperChoice.StableTs);
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
            comboBoxLanguages.Items.Clear();
            comboBoxLanguages.Items.AddRange(WhisperLanguage.Languages.OrderBy(p => p.Name).ToArray<object>());
            var lang = WhisperLanguage.Languages.FirstOrDefault(p => p.Code == Configuration.Settings.Tools.WhisperLanguageCode);
            comboBoxLanguages.Text = lang != null ? lang.ToString() : "English";

            FillModels(comboBoxModels, string.Empty);

            labelFC.Text = string.Empty;

            removeTemporaryFilesToolStripMenuItem.Checked = Configuration.Settings.Tools.WhisperDeleteTempFiles;
            ContextMenuStrip = contextMenuStripWhisperAdvanced;
        }

        public static void FillModels(ComboBox comboBoxModels, string lastDownloadedModel)
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
                Configuration.Settings.Tools.WhisperChoice == WhisperChoice.PurfviewFasterWhisper)
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
                            comboBoxModels.SelectedIndex = comboBoxModels.Items.Count - 1;
                        }
                    }
                }

                if (comboBoxModels.SelectedIndex < 0 && comboBoxModels.Items.Count > 0)
                {
                    comboBoxModels.SelectedIndex = 0;
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

            _languageCode = GetLanguage(comboBoxLanguages.Text);
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
            comboBoxLanguages.Enabled = false;
            comboBoxModels.Enabled = false;
            var waveFileName = GenerateWavFile(_videoFileName, _audioTrackNumber);
            progressBar1.Style = ProgressBarStyle.Blocks;
            timer1.Start();
            var transcript = TranscribeViaWhisper(waveFileName);
            timer1.Stop();
            if (_cancel && (transcript == null || transcript.Paragraphs.Count == 0 || MessageBox.Show(LanguageSettings.Current.AudioToText.KeepPartialTranscription, Text, MessageBoxButtons.YesNoCancel) != DialogResult.Yes))
            {
                DialogResult = DialogResult.Cancel;
                return;
            }

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
                transcript = WhisperTimingFixer.ShortenLongTexts(transcript);
                transcript = WhisperTimingFixer.ShortenViaWavePeaks(transcript, wavePeaks);
            }

            TranscribedSubtitle = postProcessor.Fix(AudioToTextPostProcessor.Engine.Whisper, transcript, checkBoxUsePostProcessing.Checked, true, true, true, true, true);

            if (transcript == null || transcript.Paragraphs.Count == 0)
            {
                UpdateLog();
                SeLogger.Error(textBoxLog.Text);
                IncompleteModelName = comboBoxModels.Text;
            }
            else
            {
                //TODO: remove at some point
                UpdateLog();
                SeLogger.Error(textBoxLog.Text);
            }

            DialogResult = DialogResult.OK;
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
                var transcript = TranscribeViaWhisper(waveFileName);
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
                    transcript = WhisperTimingFixer.ShortenLongTexts(transcript);
                    transcript = WhisperTimingFixer.ShortenViaWavePeaks(transcript, wavePeaks);
                }

                var postProcessor = new AudioToTextPostProcessor(_languageCode)
                {
                    ParagraphMaxChars = Configuration.Settings.General.SubtitleLineMaximumLength * 2,
                };
                TranscribedSubtitle = postProcessor.Fix(AudioToTextPostProcessor.Engine.Whisper, transcript, checkBoxUsePostProcessing.Checked, true, true, true, true, true);

                SaveToSourceFolder(videoFileName);
                TaskbarList.SetProgressValue(_parentForm.Handle, _batchFileNumber, listViewInputFiles.Items.Count);
            }

            progressBar1.Visible = false;
            labelTime.Text = string.Empty;

            TaskbarList.StartBlink(_parentForm, 10, 1, 2);

            if (errors.Length > 0)
            {
                MessageBox.Show($"{errorCount} error(s)!{Environment.NewLine}{errors}");
            }

            MessageBox.Show(string.Format(LanguageSettings.Current.AudioToText.XFilesSavedToVideoSourceFolder, listViewInputFiles.Items.Count - errorCount));

            groupBoxInputFiles.Enabled = true;
            buttonGenerate.Enabled = true;
            buttonDownload.Enabled = true;
            buttonBatchMode.Enabled = true;
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
            var text = TranscribedSubtitle.ToText(format);

            var fileName = Path.Combine(Utilities.GetPathAndFileNameWithoutExtension(videoFileName)) + format.Extension;
            if (File.Exists(fileName))
            {
                fileName = $"{Path.Combine(Utilities.GetPathAndFileNameWithoutExtension(videoFileName))}.{Guid.NewGuid().ToByteArray()}.{format.Extension}";
            }

            File.WriteAllText(fileName, text, Encoding.UTF8);
            _outputText.Add("Subtitle written to : " + fileName);
        }

        internal static string GetLanguage(string name)
        {
            var language = WhisperLanguage.Languages.FirstOrDefault(l => l.Name == name);
            return language != null ? language.Code : "en";
        }

        public Subtitle TranscribeViaWhisper(string waveFileName)
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

            labelProgress.Refresh();
            Application.DoEvents();
            _resultList = new List<ResultText>();
            var process = GetWhisperProcess(waveFileName, model.Name, _languageCode, checkBoxTranslateToEnglish.Checked, OutputHandler);
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
                    process.Kill();
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

            if (GetResultFromSrt(waveFileName, out var resultTexts, _outputText, _filesToDelete))
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

        public static bool GetResultFromSrt(string waveFileName, out List<ResultText> resultTexts, ConcurrentBag<string> outputText, List<string> filesToDelete)
        {
            var srtFileName = waveFileName + ".srt";
            if (!File.Exists(srtFileName) && waveFileName.EndsWith(".wav"))
            {
                srtFileName = waveFileName.Remove(waveFileName.Length - 4) + ".srt";
            }

            var whisperFolder = WhisperHelper.GetWhisperFolder() ?? string.Empty;
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
                new SubRip().LoadSubtitle(sub, FileUtil.ReadAllLinesShared(srtFileName, Encoding.UTF8), srtFileName);
                outputText?.Add($"Loading result from {srtFileName}{Environment.NewLine}");
            }
            else
            {
                new WebVTT().LoadSubtitle(sub, FileUtil.ReadAllLinesShared(vttFileName, Encoding.UTF8), srtFileName);
                outputText?.Add($"Loading result from {vttFileName}{Environment.NewLine}");
            }

            sub.RemoveEmptyLines();

            var results = new List<ResultText>();
            foreach (var p in sub.Paragraphs)
            {
                results.Add(new ResultText
                {
                    Start = (int)Math.Round(p.StartTime.TotalSeconds, MidpointRounding.AwayFromZero),
                    End = (int)Math.Round(p.EndTime.TotalSeconds, MidpointRounding.AwayFromZero),
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
                else if (_pctWhisper.IsMatch(line))
                {
                    var arr = line.Split('%');
                    if (arr.Length > 1 && double.TryParse(arr[0], NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var pct))
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
            var outWaveFile = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".wav");
            _filesToDelete.Add(outWaveFile);
            var process = GetFfmpegProcess(videoFileName, audioTrackNumber, outWaveFile);
            process.Start();
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

        public static Process GetWhisperProcess(string waveFileName, string model, string language, bool translate, DataReceivedEventHandler dataReceivedHandler = null)
        {
            // whisper --model tiny.en --language English --fp16 False a.wav

            var translateToEnglish = translate ? WhisperHelper.GetWhisperTranslateParameter() : string.Empty;
            if (language.ToLowerInvariant() == "english" || language.ToLowerInvariant() == "en")
            {
                language = "en";
                translateToEnglish = string.Empty;
            }

            if (Configuration.Settings.Tools.WhisperChoice == WhisperChoice.Cpp)
            {
                if (!Configuration.Settings.Tools.WhisperExtraSettings.Contains("--print-progress"))
                {
                    translateToEnglish += "--print-progress ";
                }
            }

            var outputSrt = string.Empty;
            var postParams = string.Empty;
            if (Configuration.Settings.Tools.WhisperChoice == WhisperChoice.Cpp ||
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

            SeLogger.Error($"{w} {parameters}");

            var process = new Process { StartInfo = new ProcessStartInfo(w, parameters) { WindowStyle = ProcessWindowStyle.Hidden, CreateNoWindow = true } };

            if (!string.IsNullOrEmpty(Configuration.Settings.General.FFmpegLocation) && process.StartInfo.EnvironmentVariables["Path"] != null)
            {
                process.StartInfo.EnvironmentVariables["Path"] = process.StartInfo.EnvironmentVariables["Path"].TrimEnd(';') + ";" + Path.GetDirectoryName(Configuration.Settings.General.FFmpegLocation);
            }

            var whisperFolder = WhisperHelper.GetWhisperFolder();
            if (!string.IsNullOrEmpty(whisperFolder))
            {
                process.StartInfo.WorkingDirectory = whisperFolder;
            }

            if (!string.IsNullOrEmpty(whisperFolder) && process.StartInfo.EnvironmentVariables["Path"] != null)
            {
                process.StartInfo.EnvironmentVariables["Path"] = process.StartInfo.EnvironmentVariables["Path"].TrimEnd(';') + ";" + whisperFolder;
            }

            if (Configuration.Settings.Tools.WhisperChoice != WhisperChoice.Cpp &&
                Configuration.Settings.Tools.WhisperChoice != WhisperChoice.ConstMe)
            {
                process.StartInfo.EnvironmentVariables["PYTHONIOENCODING"] = "utf-8";
                process.StartInfo.EnvironmentVariables["PYTHONLEGACYWINDOWSSTDIO"] = "utf-8";
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

            if (Configuration.Settings.Tools.WhisperChoice == WhisperChoice.ConstMe)
            {
                Configuration.Settings.Tools.WhisperChoice = WhisperChoice.Cpp;
                whisperConstMeToolStripMenuItem_Click(null, null);
                if (Configuration.Settings.Tools.WhisperChoice == WhisperChoice.Cpp)
                {
                    Init();
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

            System.Threading.SynchronizationContext.Current.Post(TimeSpan.FromMilliseconds(25), () =>
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
            checkBoxTranslateToEnglish.Enabled = comboBoxLanguages.Text.ToLowerInvariant() != "english";
        }

        private void WhisperPhpOriginalChoose()
        {
            Configuration.Settings.Tools.WhisperChoice = WhisperChoice.OpenAI;

            if (Configuration.IsRunningOnWindows)
            {
                var path = WhisperHelper.GetWhisperFolder();
                if (string.IsNullOrEmpty(path))
                {
                    using (var openFileDialog1 = new OpenFileDialog())
                    {
                        openFileDialog1.Title = "Locate whisper.exe (Python version)";
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
            if (comboBoxWhisperEngine.Text == Configuration.Settings.Tools.WhisperChoice)
            {
                return;
            }

            if (comboBoxWhisperEngine.Text == WhisperChoice.OpenAI)
            {
                WhisperPhpOriginalChoose();
            }
            else if (comboBoxWhisperEngine.Text == WhisperChoice.Cpp)
            {
                Configuration.Settings.Tools.WhisperChoice = WhisperChoice.Cpp;
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
                setCPPConstmeModelsFolderToolStripMenuItem.Text = $"{LanguageSettings.Current.AudioToText.SetCppConstMeFolder} [{Configuration.Settings.Tools.WhisperCppModelLocation}]";
            }
            else
            {
                setCPPConstmeModelsFolderToolStripMenuItem.Text = LanguageSettings.Current.AudioToText.SetCppConstMeFolder;
            }
        }

        private void runOnlyPostProcessingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
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
                    _subtitle = WhisperTimingFixer.ShortenLongTexts(_subtitle);
                    _subtitle = WhisperTimingFixer.ShortenViaWavePeaks(_subtitle, wavePeaks);
                }
                else if (!checkBoxUsePostProcessing.Checked)
                {
                    return;
                }

                TranscribedSubtitle = postProcessor.Fix(AudioToTextPostProcessor.Engine.Whisper, _subtitle, checkBoxUsePostProcessing.Checked, true, true, true, true, true);
                DialogResult = DialogResult.OK;
            }
            finally
            {
                buttonGenerate.Enabled = true;
                buttonDownload.Enabled = true;
                buttonBatchMode.Enabled = true;
                comboBoxLanguages.Enabled = true;
                comboBoxModels.Enabled = true;
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
    }
}
