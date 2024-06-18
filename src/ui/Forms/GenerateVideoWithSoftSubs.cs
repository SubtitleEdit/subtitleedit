using Nikse.SubtitleEdit.Core.BluRaySup;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.ContainerFormats.Matroska;
using Nikse.SubtitleEdit.Core.ContainerFormats.Mp4;
using Nikse.SubtitleEdit.Core.ContainerFormats.Mp4.Boxes;
using Nikse.SubtitleEdit.Core.Interfaces;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Logic;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using MessageBox = Nikse.SubtitleEdit.Forms.SeMsgBox.MessageBox;

namespace Nikse.SubtitleEdit.Forms
{
    public sealed partial class GenerateVideoWithSoftSubs : Form
    {
        private bool _abort;
        private readonly Subtitle _subtitle;
        private VideoInfo _videoInfo;
        private string _inputVideoFileName;
        private StringBuilder _log;
        private readonly List<VideoPreviewGeneratorSub> _softSubs = new List<VideoPreviewGeneratorSub>();
        private readonly List<VideoPreviewGeneratorSub> _tracksToDelete = new List<VideoPreviewGeneratorSub>();
        private bool _promptFFmpegParameters;
        private readonly List<string> _cleanUpFolders = new List<string>();

        private const int IndexLanguage = 1;
        private const int IndexDefault = 2;
        private const int IndexForced = 3;

        public string VideoFileName { get; private set; }
        public long MillisecondsEncoding { get; private set; }

        public GenerateVideoWithSoftSubs(Subtitle subtitle, string inputVideoFileName, VideoInfo videoInfo)
        {
            UiUtil.PreInitialize(this);
            InitializeComponent();
            UiUtil.FixFonts(this);

            _videoInfo = videoInfo;
            _subtitle = new Subtitle(subtitle);
            _inputVideoFileName = inputVideoFileName;

            Text = LanguageSettings.Current.GenerateVideoWithEmbeddedSubs.Title;
            labelInputVideoFile.Text = LanguageSettings.Current.GenerateVideoWithEmbeddedSubs.InputVideoFile;
            labelSubtitles.Text = string.Empty;
            buttonAddSubtitles.Text = LanguageSettings.Current.DvdSubRip.Add;
            ButtonRemoveSubtitles.Text = LanguageSettings.Current.SubStationAlphaStyles.Remove;
            buttonClear.Text = LanguageSettings.Current.SubStationAlphaStyles.RemoveAll;
            ButtonMoveSubUp.Text = LanguageSettings.Current.DvdSubRip.MoveUp;
            ButtonMoveSubDown.Text = LanguageSettings.Current.DvdSubRip.MoveDown;
            buttonToggleForced.Text = LanguageSettings.Current.GenerateVideoWithEmbeddedSubs.ToggleForced;
            buttonSetDefault.Text = LanguageSettings.Current.GenerateVideoWithEmbeddedSubs.ToggleDefault;
            buttonSetLanguage.Text = LanguageSettings.Current.GenerateVideoWithEmbeddedSubs.SetLanguage;
            columnHeader1Type.Text = LanguageSettings.Current.Main.Controls.SubtitleFormat;
            columnHeader2Language.Text = LanguageSettings.Current.GenerateVideoWithEmbeddedSubs.LanguageAndTitle;
            columnHeader3Default.Text = LanguageSettings.Current.GenerateVideoWithEmbeddedSubs.Default;
            columnHeader4Forced.Text = LanguageSettings.Current.ExportPngXml.Forced;
            columnHeader5FileName.Text = LanguageSettings.Current.JoinSubtitles.FileName;
            buttonGenerate.Text = LanguageSettings.Current.Watermark.Generate;
            buttonCancel.Text = LanguageSettings.Current.General.Cancel;
            checkBoxDeleteInputVideoAfterGeneration.Text = LanguageSettings.Current.GenerateVideoWithEmbeddedSubs.DeleteInputVideo;
            addToolStripMenuItem.Text = LanguageSettings.Current.DvdSubRip.Add;
            toolStripMenuItemStorageRemove.Text = LanguageSettings.Current.SubStationAlphaStyles.Remove;
            toolStripMenuItemStorageRemoveAll.Text = LanguageSettings.Current.SubStationAlphaStyles.RemoveAll;
            toolStripMenuItemStorageMoveUp.Text = LanguageSettings.Current.DvdSubRip.MoveUp;
            toolStripMenuItemStorageMoveDown.Text = LanguageSettings.Current.DvdSubRip.MoveDown;
            toggleForcedToolStripMenuItem.Text = LanguageSettings.Current.GenerateVideoWithEmbeddedSubs.ToggleForced;
            toggleDefaultToolStripMenuItem.Text = LanguageSettings.Current.GenerateVideoWithEmbeddedSubs.ToggleDefault;
            setLanguageToolStripMenuItem.Text = LanguageSettings.Current.GenerateVideoWithEmbeddedSubs.SetLanguage;
            viewToolStripMenuItem.Text = LanguageSettings.Current.General.ShowPreview;
            setSuffixToolStripMenuItem.Text = LanguageSettings.Current.GenerateVideoWithEmbeddedSubs.OutputFileNameSettings;
            toolStripMenuItemSuffix2.Text = LanguageSettings.Current.GenerateVideoWithEmbeddedSubs.OutputFileNameSettings;

            promptParameterBeforeGenerateToolStripMenuItem.Text = LanguageSettings.Current.GenerateBlankVideo.GenerateWithFfmpegParametersPrompt;
            labelSubtitles.Text = string.Format(LanguageSettings.Current.GenerateVideoWithEmbeddedSubs.SubtitlesX, 0);
            labelNotSupported.Text = string.Empty;
            labelPleaseWait.Text = string.Empty;

            LoadVideo(inputVideoFileName);
            AddCurrentSubtitle();

            checkBoxDeleteInputVideoAfterGeneration.Checked = Configuration.Settings.Tools.GenVideoDeleteInputVideoFile;
            checkBoxDeleteInputVideoAfterGeneration.Left = buttonGenerate.Left - checkBoxDeleteInputVideoAfterGeneration.Width - 10;
        }

        private void AddCurrentSubtitle()
        {
            if (_subtitle != null && _subtitle.Paragraphs.Count > 0)
            {
                var dir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
                Directory.CreateDirectory(dir);
                _cleanUpFolders.Add(dir);

                var targetFormat = _subtitle.OriginalFormat ?? new SubRip();
                var nameOnly = string.IsNullOrEmpty(_subtitle.FileName)
                    ? "Untitled" + targetFormat.Extension
                    : _subtitle.FileName;

                var fileName = Path.Combine(dir, nameOnly);
                File.WriteAllText(fileName, targetFormat.ToText(_subtitle, string.Empty));
                fileName = GetKnownFileNameOrConvertToSrtOrUtf8(fileName, _subtitle);

                AddListViewItem(fileName);

                if (listViewSubtitles.Items.Count > 0)
                {
                    listViewSubtitles.Items[0].Selected = true;
                    listViewSubtitles.Items[0].Focused = true;
                }
            }
        }

        private void LoadVideo(string inputVideoFileName)
        {
            if (string.IsNullOrEmpty(inputVideoFileName) || !File.Exists(inputVideoFileName))
            {
                return;
            }

            var videoInfo = UiUtil.GetVideoInfo(inputVideoFileName);
            if (videoInfo == null || videoInfo.TotalMilliseconds == 0)
            {
                return;
            }

            textBoxInputFileName.Text = inputVideoFileName;
            buttonGenerate.Enabled = true;
            _inputVideoFileName = inputVideoFileName;
            _videoInfo = videoInfo;

            using (var matroska = new MatroskaFile(inputVideoFileName))
            {
                if (matroska.IsValid)
                {
                    listViewSubtitles.BeginUpdate();
                    foreach (var track in matroska.GetTracks().Where(p => p.IsSubtitle))
                    {
                        AddListViewItem(track, matroska);
                    }

                    listViewSubtitles.EndUpdate();
                    return;
                }
            }

            var mp4Parser = new MP4Parser(inputVideoFileName);
            if (mp4Parser.Moov != null && mp4Parser.VideoResolution.X > 0)
            {
                listViewSubtitles.BeginUpdate();
                foreach (var track in mp4Parser.GetSubtitleTracks())
                {
                    AddListViewItem(track);
                }

                listViewSubtitles.EndUpdate();
            }

            _tracksToDelete.Clear();
        }

        private void AddListViewItem(Trak track)
        {
            if (!track.Mdia.IsTextSubtitle && !track.Mdia.IsClosedCaption && !track.Mdia.IsVobSubSubtitle)
            {
                return;
            }

            AddListViewItem(new VideoPreviewGeneratorSub
            {
                Name = track.Mdia.HandlerName,
                Language = track.Mdia.Mdhd.Iso639ThreeLetterCode,
                Format = track.Mdia.HandlerType,
                IsNew = false,
                IsForced = false,
                IsDefault = false,
                Tag = track,
            });
        }

        private void AddListViewItem(VideoPreviewGeneratorSub sub)
        {
            var item = new ListViewItem
            {
                Tag = sub,
                Text = sub.SubtitleFormat ?? sub.Format,
            };

            _softSubs.Add(sub);

            item.SubItems.Add(GetDisplayLanguage(sub));
            item.SubItems.Add(sub.IsDefault.ToString(CultureInfo.InvariantCulture));
            item.SubItems.Add(sub.IsForced.ToString(CultureInfo.InvariantCulture));
            item.SubItems.Add(sub.IsNew ? sub.FileName : string.Empty);
            listViewSubtitles.Items.Add(item);

            labelSubtitles.Text = string.Format(LanguageSettings.Current.GenerateVideoWithEmbeddedSubs.SubtitlesX, listViewSubtitles.Items.Count);
        }

        private static string GetDisplayLanguage(VideoPreviewGeneratorSub sub)
        {
            if (string.IsNullOrWhiteSpace(sub.Language) && string.IsNullOrWhiteSpace(sub.Title))
            {
                return "Undefined";
            }

            if (!string.IsNullOrWhiteSpace(sub.Language) && !string.IsNullOrWhiteSpace(sub.Title))
            {
                return sub.Language + "/" + sub.Title;
            }

            if (!string.IsNullOrWhiteSpace(sub.Language))
            {
                return sub.Language;
            }

            if (!string.IsNullOrWhiteSpace(sub.Title))
            {
                return sub.Title;
            }

            return string.Empty;
        }

        private void AddListViewItem(MatroskaTrackInfo track, MatroskaFile matroska)
        {
            if (track.CodecId.Equals("S_HDMV/PGS", StringComparison.OrdinalIgnoreCase))
            {
                AddListViewItem(new VideoPreviewGeneratorSub
                {
                    Name = track.CodecId,
                    Language = track.Language,
                    Title = track.Name,
                    IsNew = false,
                    IsForced = track.IsForced,
                    IsDefault = track.IsDefault,
                    Tag = track,
                    SubtitleFormat = track.CodecId,
                });

                return;
            }

            if (track.CodecId.Equals("S_VOBSUB", StringComparison.OrdinalIgnoreCase) ||
                track.CodecId.Equals("S_HDMV/TEXTST", StringComparison.OrdinalIgnoreCase) ||
                track.CodecId.Equals("S_DVBSUB", StringComparison.OrdinalIgnoreCase))
            {
                if (string.IsNullOrEmpty(labelNotSupported.Text))
                {
                    labelNotSupported.Text = $"Not supported: {track.CodecId}";
                }
                else
                {
                    labelNotSupported.Text += $", {track.CodecId}";
                }

                return; // not supported
            }

            var subtitle = new Subtitle();
            var sub = matroska.GetSubtitle(track.TrackNumber, null);
            var format = Utilities.LoadMatroskaTextSubtitle(track, matroska, sub, subtitle);

            var fileName = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            File.WriteAllText(fileName, subtitle.ToText(format), Encoding.UTF8);

            AddListViewItem(new VideoPreviewGeneratorSub
            {
                Name = track.CodecId,
                Language = track.Language,
                Title = track.Name,
                IsNew = false,
                IsForced = track.IsForced,
                IsDefault = track.IsDefault,
                Tag = track,
                SubtitleFormat = format.GetType().Name,
                FileName = fileName,
            });
        }

        private void AddListViewItem(string fileName)
        {
            if (string.IsNullOrEmpty(fileName) || !File.Exists(fileName))
            {
                return;
            }

            if (fileName.EndsWith(".sup", StringComparison.OrdinalIgnoreCase) &&
                FileUtil.IsBluRaySup(fileName))
            {
                AddListViewItem(new VideoPreviewGeneratorSub
                {
                    Name = Path.GetFileName(fileName),
                    Language = GetLanguageFromFileName(fileName),
                    Format = "S_HDMV/PGS",
                    SubtitleFormat = null,
                    IsNew = true,
                    IsForced = false,
                    IsDefault = false,
                    FileName = fileName,
                });

                return;
            }

            var subtitle = Subtitle.Parse(fileName);
            if (subtitle == null || subtitle.Paragraphs.Count == 0)
            {
                return;
            }

            fileName = GetKnownFileNameOrConvertToSrtOrUtf8(fileName, subtitle);
            if (fileName != subtitle.FileName)
            {
                subtitle = Subtitle.Parse(fileName);
            }

            var language = string.Empty;
            var title = string.Empty;
            var twoLetterCode = LanguageAutoDetect.AutoDetectGoogleLanguage(subtitle);
            if (twoLetterCode != null && twoLetterCode.Length == 2)
            {
                var iso = Iso639Dash2LanguageCode.List.FirstOrDefault(p => p.TwoLetterCode == twoLetterCode);
                if (iso != null)
                {
                    language = iso.ThreeLetterCode;
                    title = iso.EnglishName;
                }
            }

            AddListViewItem(new VideoPreviewGeneratorSub
            {
                Name = Path.GetFileName(fileName),
                Language = language,
                Title = title,
                Format = subtitle.OriginalFormat.FriendlyName,
                SubtitleFormat = subtitle.OriginalFormat.GetType().Name,
                IsNew = true,
                IsForced = false,
                IsDefault = false,
                FileName = fileName,
            });
        }

        private static string GetLanguageFromFileName(string fileName)
        {
            var defaultLanguage = "eng";

            if (string.IsNullOrEmpty(fileName))
            {
                return defaultLanguage;
            }

            var fileNameNoExt = Path.GetFileNameWithoutExtension(fileName);
            var split = fileNameNoExt.Split('.', '_', '_');
            var last = split.LastOrDefault();

            if (last == null)
            {
                return defaultLanguage;
            }

            if (last.Length == 3)
            {
                return last;
            }

            if (last.Length == 2)
            {
                var threeLetterCode = Iso639Dash2LanguageCode.GetThreeLetterCodeFromTwoLetterCode(last);
                if (threeLetterCode.Length == 3)
                {
                    return threeLetterCode;
                }
            }

            return defaultLanguage;
        }

        private string GetKnownFileNameOrConvertToSrtOrUtf8(string fileName, Subtitle subtitle)
        {
            SubtitleFormat targetFormat = new SubRip();

            if (subtitle.OriginalFormat != null && (
                subtitle.OriginalFormat.Name == new SubRip().Name ||
                subtitle.OriginalFormat.Name == new AdvancedSubStationAlpha().Name ||
                subtitle.OriginalFormat.Name == new SubStationAlpha().Name ||
                subtitle.OriginalFormat.Name == new WebVTT().Name ||
                subtitle.OriginalFormat.Name == new WebVTTFileWithLineNumber().Name))
            {
                if (subtitle.OriginalEncoding?.EncodingName == Encoding.UTF8.EncodingName)
                {
                    return fileName;
                }

                targetFormat = subtitle.OriginalFormat;
            }

            var dir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(dir);
            _cleanUpFolders.Add(dir);

            var nameOnly = string.IsNullOrEmpty(subtitle.FileName)
                ? "Untitled"
                : Path.GetFileNameWithoutExtension(subtitle.FileName);

            fileName = Path.Combine(dir, nameOnly.Trim() + targetFormat.Extension);
            File.WriteAllText(fileName, targetFormat.ToText(subtitle, string.Empty));
            return fileName;
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            _abort = true;
            if (buttonGenerate.Enabled)
            {
                DialogResult = DialogResult.Cancel;
            }
        }

        private void OutputHandler(object sendingProcess, DataReceivedEventArgs outLine)
        {
            if (string.IsNullOrWhiteSpace(outLine.Data))
            {
                return;
            }

            _log?.AppendLine(outLine.Data);
        }

        private void buttonGenerate_Click(object sender, EventArgs e)
        {
            if (_softSubs.Count == 0)
            {
                var res = MessageBox.Show("Generate video without any embedded subs?", "", MessageBoxButtons.YesNoCancel);
                if (res != DialogResult.Yes)
                {
                    return;
                }
            }

            _log = new StringBuilder();
            buttonGenerate.Enabled = false;

            using (var saveDialog = new SaveFileDialog
            {
                FileName = SuggestNewVideoFileName(),
                Filter = GetTargetVideoFilter(),
                AddExtension = true
            })
            {
                if (saveDialog.ShowDialog(this) != DialogResult.OK)
                {
                    buttonGenerate.Enabled = true;
                    return;
                }

                VideoFileName = saveDialog.FileName;
            }

            if (VideoFileName.Equals(_inputVideoFileName, StringComparison.OrdinalIgnoreCase))
            {
                MessageBox.Show("Input video file name and output file name cannot be the same.", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var isMp4 = VideoFileName.EndsWith(".mp4", StringComparison.OrdinalIgnoreCase);
            if (isMp4 && _softSubs.Any(p => p.SubtitleFormat == "S_HDMV/PGS"))
            {
                MessageBox.Show("Cannot embed S_HDMV/PGS in MP4", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (File.Exists(VideoFileName))
            {
                try
                {
                    File.Delete(VideoFileName);
                }
                catch
                {
                    MessageBox.Show($"Cannot overwrite video file {VideoFileName} - probably in use!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    buttonGenerate.Enabled = true;
                    return;
                }
            }

            _log = new StringBuilder();
            _log.AppendLine("Target file name: " + VideoFileName);

            groupBoxSettings.Enabled = false;
            buttonGenerate.Enabled = false;

            var stopWatch = Stopwatch.StartNew();

            RunEmbedding();

            MillisecondsEncoding = stopWatch.ElapsedMilliseconds;
            groupBoxSettings.Enabled = true;

            if (_abort)
            {
                DialogResult = DialogResult.Cancel;
                return;
            }

            if (!File.Exists(VideoFileName) || new FileInfo(VideoFileName).Length == 0)
            {
                SeLogger.Error(Environment.NewLine + "Generate embedded video failed: " + Environment.NewLine + _log);
                MessageBox.Show("Generate embedded video failed" + Environment.NewLine +
                                "For more info see the error log: " + SeLogger.ErrorFile, MessageBoxButtons.OK, MessageBoxIcon.Error);
                buttonGenerate.Enabled = true;
                groupBoxSettings.Enabled = true;
                return;
            }

            var inputFileInfo = new FileInfo(_inputVideoFileName);
            var outputFileInfo = new FileInfo(VideoFileName);
            if (inputFileInfo.Length > 5_000_000 && outputFileInfo.Length < 9_000)
            {
                SeLogger.Error(Environment.NewLine + "Generate embedded video file very small: " + Environment.NewLine + _log);
                MessageBox.Show("Generate embedded video seems very small - it probably failed!" + Environment.NewLine +
                                "For more info see the error log: " + SeLogger.ErrorFile, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                buttonGenerate.Enabled = true;
                groupBoxSettings.Enabled = true;
                return;
            }

            if (checkBoxDeleteInputVideoAfterGeneration.Checked && File.Exists(_inputVideoFileName))
            {
                try
                {
                    File.Delete(_inputVideoFileName);
                }
                catch
                {
                    MessageBox.Show($"Cannot delete input video file {_inputVideoFileName} - probably in use!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

            DialogResult = DialogResult.OK;
        }

        private static string GetTargetVideoFilter()
        {
            if (Configuration.Settings.Tools.GenVideoEmbedOutputExt == ".mp4")
            {
                return "MP4|*.mp4|Matroska|*.mkv|WebM|*.webm";
            }

            if (Configuration.Settings.Tools.GenVideoEmbedOutputExt == ".webm")
            {
                return "WebM|*.webm|Matroska|*.mkv|MP4|*.mp4";
            }

            return "Matroska|*.mkv|WebM|*.webm|MP4|*.mp4";
        }

        private string SuggestNewVideoFileName()
        {
            var fileName = Path.GetFileNameWithoutExtension(_inputVideoFileName);

            var suffixesToRemove = new List<string> { Configuration.Settings.Tools.GenVideoEmbedOutputSuffix };
            if (Configuration.Settings.Tools.GenVideoEmbedOutputReplace != null)
            {
                suffixesToRemove.AddRange(Configuration.Settings.Tools.GenVideoEmbedOutputReplace.SplitToLines());
            }

            foreach (var suffix in suffixesToRemove.Where(p => p.Length > 0).OrderByDescending(p => p.Length))
            {
                fileName = fileName.Replace("." + suffix, string.Empty);
                fileName = fileName.Replace("_" + suffix, string.Empty);
                fileName = fileName.Replace("-" + suffix, string.Empty);
                fileName = fileName.Replace(suffix, string.Empty);
            }

            if (!string.IsNullOrWhiteSpace(Configuration.Settings.Tools.GenVideoEmbedOutputSuffix))
            {
                fileName = fileName.TrimEnd('_', '-', '.') + "-" + Configuration.Settings.Tools.GenVideoEmbedOutputSuffix;
            }

            fileName = fileName.Replace('.', '-');

            fileName += Configuration.Settings.Tools.GenVideoEmbedOutputExt == ".mp4" ? ".mp4" : ".mkv";

            return fileName;
        }

        private void RunEmbedding()
        {
            var process = GetFfmpegProcess(_inputVideoFileName, VideoFileName);
            _log.AppendLine("ffmpeg arguments: " + process.StartInfo.Arguments);
            if (!CheckForPromptParameters(process))
            {
                _abort = true;
                return;
            }

            try
            {
                labelNotSupported.Text = string.Empty;
                labelPleaseWait.Text = LanguageSettings.Current.General.PleaseWait;
                Cursor = Cursors.WaitCursor;
                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                while (!process.HasExited)
                {
                    Application.DoEvents();
                    WindowsHelper.PreventStandBy();
                    System.Threading.Thread.Sleep(100);
                    if (_abort)
                    {
                        process.Kill();
                    }
                }
            }
            finally
            {
                Cursor = Cursors.Default;
                labelPleaseWait.Text = string.Empty;
            }
        }

        private bool CheckForPromptParameters(Process process)
        {
            if (!_promptFFmpegParameters)
            {
                return true;
            }

            using (var form = new GenerateVideoFFmpegPrompt(Text, process.StartInfo.Arguments))
            {
                if (form.ShowDialog(this) != DialogResult.OK)
                {
                    return false;
                }

                _log.AppendLine("ffmpeg arguments custom: " + process.StartInfo.Arguments);
                process.StartInfo.Arguments = form.Parameters;
            }

            return true;
        }

        private Process GetFfmpegProcess(string inputVideoFileName, string outputVideoFileName)
        {
            return VideoPreviewGenerator.GenerateSoftCodedVideoFile(
                inputVideoFileName,
                _softSubs,
                _tracksToDelete,
                outputVideoFileName,
                OutputHandler);
        }

        private void GenerateVideoWithSoftSubs_KeyDown(object sender, KeyEventArgs e)
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
                    textBoxLog.ScrollBars = ScrollBars.Both;
                    textBoxLog.BringToFront();
                    textBoxLog.Dock = DockStyle.Fill;

                    if (_log == null)
                    {
                        var log = new StringBuilder();
                        log.AppendLine("Video info width: " + _videoInfo.Width);
                        log.AppendLine("Video info width: " + _videoInfo.Height);
                        log.AppendLine("Video info total frames: " + _videoInfo.TotalFrames);
                        log.AppendLine("Video info total seconds: " + _videoInfo.TotalSeconds);
                        log.AppendLine();
                        log.AppendLine("MP4: ffmpeg " + GetFfmpegProcess(_inputVideoFileName, "output.mp4").StartInfo.Arguments);
                        log.AppendLine();
                        log.AppendLine("MKV: ffmpeg " + GetFfmpegProcess(_inputVideoFileName, "output.mkv").StartInfo.Arguments);
                        textBoxLog.Text = log.ToString();
                    }
                    else
                    {
                        textBoxLog.Text = _log.ToString();
                    }
                }
                e.SuppressKeyPress = true;
            }
            else if (e.KeyCode == Keys.Escape && _log == null)
            {
                DialogResult = DialogResult.Cancel;
                e.SuppressKeyPress = true;
            }
            else if (e.KeyCode == Keys.Escape && _log == null)
            {
                DialogResult = DialogResult.Cancel;
                e.SuppressKeyPress = true;
            }
        }

        private void GenerateVideoWithHardSubs_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!string.IsNullOrEmpty(VideoFileName))
            {
                Configuration.Settings.Tools.GenVideoEmbedOutputExt = Path.GetExtension(VideoFileName).ToLowerInvariant();
            }

            Configuration.Settings.Tools.GenVideoDeleteInputVideoFile = checkBoxDeleteInputVideoAfterGeneration.Checked;

            foreach (var cleanUpFolder in _cleanUpFolders)
            {
                try
                {
                    Directory.Delete(cleanUpFolder, true);
                }
                catch
                {
                    // ignore
                }
            }
        }

        private void GenerateVideoWithHardSubs_Shown(object sender, EventArgs e)
        {
            listViewSubtitles.AutoSizeLastColumn();
            listViewSubtitles_SelectedIndexChanged(null, null);

            if (!File.Exists(_inputVideoFileName))
            {
                buttonGenerate.Enabled = false;
            }
            else
            {
                buttonGenerate.Focus();
            }

            UiUtil.FixFonts(groupBoxSettings, 2000);
        }

        private void buttonOpenVideoFile_Click(object sender, EventArgs e)
        {
            using (var openFileDialog1 = new OpenFileDialog())
            {
                openFileDialog1.Title = LanguageSettings.Current.General.OpenVideoFile;
                openFileDialog1.FileName = string.Empty;
                openFileDialog1.Filter = UiUtil.GetVideoFileFilter(false);
                openFileDialog1.FileName = string.Empty;
                if (openFileDialog1.ShowDialog() != DialogResult.OK)
                {
                    return;
                }

                buttonClear_Click(null, null);
                LoadVideo(openFileDialog1.FileName);
            }

            listViewSubtitles_SelectedIndexChanged(null, null);
        }

        private void buttonAddSubtitles_Click(object sender, EventArgs e)
        {
            using (var openFileDialog1 = new OpenFileDialog())
            {
                openFileDialog1.Title = LanguageSettings.Current.General.OpenSubtitle;
                openFileDialog1.FileName = string.Empty;
                openFileDialog1.Filter = GetOpenSubtitleDialogFilter();
                openFileDialog1.FileName = string.Empty;
                openFileDialog1.Multiselect = true;
                if (openFileDialog1.ShowDialog() != DialogResult.OK)
                {
                    return;
                }

                foreach (var fileName in openFileDialog1.FileNames)
                {
                    AddListViewItem(fileName);
                }
            }

            listViewSubtitles_SelectedIndexChanged(null, null);
        }

        private static string GetOpenSubtitleDialogFilter()
        {
            var sb = new StringBuilder();
            sb.Append(LanguageSettings.Current.General.SubtitleFiles + "|");
            foreach (var s in SubtitleFormat.AllSubtitleFormats.Concat(SubtitleFormat.GetTextOtherFormats()))
            {
                UiUtil.AddExtension(sb, s.Extension);
                foreach (var ext in s.AlternateExtensions)
                {
                    UiUtil.AddExtension(sb, ext);
                }
            }

            UiUtil.AddExtension(sb, new Pac().Extension);
            UiUtil.AddExtension(sb, new Cavena890().Extension);
            UiUtil.AddExtension(sb, new Spt().Extension);
            UiUtil.AddExtension(sb, new Sptx().Extension);
            UiUtil.AddExtension(sb, new Wsb().Extension);
            UiUtil.AddExtension(sb, new CheetahCaption().Extension);
            UiUtil.AddExtension(sb, ".chk");
            UiUtil.AddExtension(sb, new CaptionsInc().Extension);
            UiUtil.AddExtension(sb, new Ultech130().Extension);
            UiUtil.AddExtension(sb, new ELRStudioClosedCaption().Extension);
            UiUtil.AddExtension(sb, ".uld"); // Ultech drop frame
            UiUtil.AddExtension(sb, new SonicScenaristBitmaps().Extension);
            UiUtil.AddExtension(sb, ".mks");
            UiUtil.AddExtension(sb, ".mxf");
            UiUtil.AddExtension(sb, ".sup");
            UiUtil.AddExtension(sb, new FinalDraftTemplate2().Extension);
            UiUtil.AddExtension(sb, new Ayato().Extension);
            UiUtil.AddExtension(sb, new PacUnicode().Extension);
            UiUtil.AddExtension(sb, new WinCaps32().Extension);
            UiUtil.AddExtension(sb, new IsmtDfxp().Extension);
            UiUtil.AddExtension(sb, new PlayCaptionsFreeEditor().Extension);
            UiUtil.AddExtension(sb, ".cdg"); // karaoke
            UiUtil.AddExtension(sb, ".pns"); // karaoke

            sb.Append('|');
            sb.Append(LanguageSettings.Current.General.AllFiles);
            sb.Append("|*.*");
            return sb.ToString();
        }

        private void listViewSubtitles_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop, false))
            {
                e.Effect = DragDropEffects.All;
            }
        }

        private void listViewSubtitles_DragDrop(object sender, DragEventArgs e)
        {
            var fileNames = (string[])e.Data.GetData(DataFormats.FileDrop);
            foreach (var fileName in fileNames)
            {
                if (!FileUtil.IsDirectory(fileName))
                {
                    AddListViewItem(fileName);
                }
            }
        }

        private void ButtonRemoveSubtitles_Click(object sender, EventArgs e)
        {
            if (listViewSubtitles.Items.Count <= 0)
            {
                return;
            }

            var list = new List<int>();
            foreach (int index in listViewSubtitles.SelectedIndices)
            {
                list.Add(index);
            }

            foreach (var index in list.OrderByDescending(p => p))
            {
                _tracksToDelete.Add(_softSubs[index]);
                _softSubs.RemoveAt(index);
                listViewSubtitles.Items.RemoveAt(index);
            }

            var newIndex = list.Min(p => p);
            if (newIndex < listViewSubtitles.Items.Count)
            {
                listViewSubtitles.Items[newIndex].Selected = true;
            }
            else
            {
                newIndex--;
                if (newIndex >= 0 && newIndex < listViewSubtitles.Items.Count)
                {
                    listViewSubtitles.Items[newIndex].Selected = true;
                }
            }

            labelSubtitles.Text = string.Format(LanguageSettings.Current.GenerateVideoWithEmbeddedSubs.SubtitlesX, listViewSubtitles.Items.Count);
        }

        private void buttonClear_Click(object sender, EventArgs e)
        {
            listViewSubtitles.Items.Clear();
            _tracksToDelete.AddRange(_softSubs);
            _softSubs.Clear();
            labelSubtitles.Text = string.Format(LanguageSettings.Current.GenerateVideoWithEmbeddedSubs.SubtitlesX, listViewSubtitles.Items.Count);
        }

        private void MoveUp(ListView listView)
        {
            if (listView.SelectedItems.Count != 1)
            {
                return;
            }

            var idx = listView.SelectedItems[0].Index;
            if (idx == 0)
            {
                return;
            }

            var item = listView.SelectedItems[0];
            listView.Items.RemoveAt(idx);
            var style = _softSubs[idx];
            _softSubs.RemoveAt(idx);
            _softSubs.Insert(idx - 1, style);

            idx--;
            listView.Items.Insert(idx, item);
        }

        private void MoveDown(ListView listView)
        {
            if (listView.SelectedItems.Count != 1)
            {
                return;
            }

            var idx = listView.SelectedItems[0].Index;
            if (idx >= listView.Items.Count - 1)
            {
                return;
            }

            listView.BeginUpdate();
            var item = listView.SelectedItems[0];
            listView.Items.RemoveAt(idx);

            var style = _softSubs[idx];
            _softSubs.RemoveAt(idx);
            _softSubs.Insert(idx + 1, style);

            idx++;
            listView.Items.Insert(idx, item);
            listView.EndUpdate();
        }

        private void MoveToTop(ListView listView)
        {
            if (listView.SelectedItems.Count != 1)
            {
                return;
            }

            var idx = listView.SelectedItems[0].Index;
            if (idx == 0)
            {
                return;
            }

            listView.BeginUpdate();
            var item = listView.SelectedItems[0];
            listView.Items.RemoveAt(idx);

            var style = _softSubs[idx];
            _softSubs.RemoveAt(idx);
            _softSubs.Insert(0, style);

            idx = 0;
            listView.Items.Insert(idx, item);
            listView.EndUpdate();
        }

        private void ButtonMoveSubUp_Click(object sender, EventArgs e)
        {
            MoveUp(listViewSubtitles);
        }

        private void ButtonMoveSubDown_Click(object sender, EventArgs e)
        {
            MoveDown(listViewSubtitles);
        }

        private void GenerateVideoWithSoftSubs_ResizeEnd(object sender, EventArgs e)
        {
            listViewSubtitles.AutoSizeLastColumn();
        }

        private void buttonToggleForced_Click(object sender, EventArgs e)
        {
            if (listViewSubtitles.SelectedIndices.Count < 1)
            {
                return;
            }

            foreach (int index in listViewSubtitles.SelectedIndices)
            {
                _softSubs[index].IsForced = !_softSubs[index].IsForced;
                listViewSubtitles.Items[index].SubItems[IndexForced].Text = _softSubs[index].IsForced.ToString(CultureInfo.InvariantCulture);
            }
        }

        private void buttonSetDefault_Click(object sender, EventArgs e)
        {
            if (listViewSubtitles.SelectedIndices.Count != 1)
            {
                return;
            }

            var selectedIndex = listViewSubtitles.SelectedItems[0].Index;
            if (_softSubs[selectedIndex].IsDefault)
            {
                _softSubs[selectedIndex].IsDefault = false;
                listViewSubtitles.Items[selectedIndex].SubItems[IndexDefault].Text = _softSubs[selectedIndex].IsDefault.ToString(CultureInfo.InvariantCulture);
                return;
            }

            for (var index = 0; index < listViewSubtitles.Items.Count; index++)
            {
                _softSubs[index].IsDefault = index == selectedIndex;
                listViewSubtitles.Items[index].SubItems[IndexDefault].Text = _softSubs[index].IsDefault.ToString(CultureInfo.InvariantCulture);
            }

            MoveToTop(listViewSubtitles);
        }

        private void toggleDefaultToolStripMenuItem_DropDownOpening(object sender, EventArgs e)
        {
            toggleDefaultToolStripMenuItem.Visible = listViewSubtitles.SelectedIndices.Count == 1;
        }

        private void buttonSetLanguage_Click(object sender, EventArgs e)
        {
            if (listViewSubtitles.SelectedIndices.Count < 1)
            {
                return;
            }

            var first = _softSubs[listViewSubtitles.SelectedIndices[0]];

            using (var cl = new GenerateVideoWithSoftSubsLanguage(first.Language, first.Title))
            {
                if (cl.ShowDialog(this) == DialogResult.OK)
                {
                    foreach (int index in listViewSubtitles.SelectedIndices)
                    {
                        var ss = _softSubs[index];
                        ss.Language = cl.Result.ThreeLetterLanguageCode;
                        ss.Title = cl.Result.Title;
                        listViewSubtitles.Items[index].SubItems[IndexLanguage].Text = ss.Language + "/" + ss.Title;
                    }
                }
            }
        }

        private void listViewSubtitles_SelectedIndexChanged(object sender, EventArgs e)
        {
            var count = listViewSubtitles.SelectedItems.Count;

            ButtonRemoveSubtitles.Enabled = count > 0;
            buttonClear.Enabled = listViewSubtitles.Items.Count > 0;
            ButtonMoveSubUp.Enabled = count == 1;
            ButtonMoveSubDown.Enabled = count == 1;
            buttonToggleForced.Enabled = count > 0;
            buttonSetDefault.Enabled = count == 1;
            buttonSetLanguage.Enabled = count > 0;
        }

        private void viewToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var selected = listViewSubtitles.SelectedItems;
            if (selected.Count != 1)
            {
                return;
            }

            var softSub = _softSubs[selected[0].Index];
            Subtitle subtitle = null;
            List<IBinaryParagraphWithPosition> binSubtitles = null;
            if (softSub.SubtitleFormat != null)
            {
                subtitle = Subtitle.Parse(softSub.FileName);
            }
            else if (softSub.FileName.EndsWith(".sup", StringComparison.OrdinalIgnoreCase) && softSub.Format == "Blu-ray sup")
            {
                var bluRaySubtitles = BluRaySupParser.ParseBluRaySup(softSub.FileName, new StringBuilder());
                binSubtitles = new List<IBinaryParagraphWithPosition>(bluRaySubtitles);
            }

            using (var cl = new SubtitleViewer(softSub.FileName, subtitle, binSubtitles))
            {
                cl.ShowDialog(this);
            }
        }

        private void listViewSubtitles_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            viewToolStripMenuItem_Click(sender, e);
        }

        private void promptParameterBeforeGenerateToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _promptFFmpegParameters = true;
            buttonGenerate_Click(null, null);
        }

        private void toolStripMenuItemStorageRemoveAll_Click(object sender, EventArgs e)
        {
            buttonClear_Click(null, null);
        }

        private void setSuffixToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (var form = new GenerateVideoWithSoftSubsOutFileName(Configuration.Settings.Tools.GenVideoEmbedOutputSuffix, Configuration.Settings.Tools.GenVideoEmbedOutputReplace))
            {
                if (form.ShowDialog(this) == DialogResult.OK)
                {
                    Configuration.Settings.Tools.GenVideoEmbedOutputSuffix = form.Suffix;
                    Configuration.Settings.Tools.GenVideoEmbedOutputReplace = form.ReplaceList;
                }
            }
        }
    }
}
