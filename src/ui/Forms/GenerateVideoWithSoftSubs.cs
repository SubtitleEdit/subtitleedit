using Nikse.SubtitleEdit.Core.BluRaySup;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.ContainerFormats.Matroska;
using Nikse.SubtitleEdit.Core.ContainerFormats.Mp4;
using Nikse.SubtitleEdit.Core.ContainerFormats.Mp4.Boxes;
using Nikse.SubtitleEdit.Core.Interfaces;
using Nikse.SubtitleEdit.Logic;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

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
        private bool _promptFFmpegParameters;
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
            columnHeader2Language.Text = LanguageSettings.Current.ChooseLanguage.Language;
            columnHeader3Default.Text = LanguageSettings.Current.GenerateVideoWithEmbeddedSubs.Default;
            columnHeader4Forced.Text = LanguageSettings.Current.ExportPngXml.Forced;
            columnHeader5FileName.Text = LanguageSettings.Current.JoinSubtitles.FileName;
            buttonGenerate.Text = LanguageSettings.Current.Watermark.Generate;
            buttonCancel.Text = LanguageSettings.Current.General.Cancel;
            promptParameterBeforeGenerateToolStripMenuItem.Text = LanguageSettings.Current.GenerateBlankVideo.GenerateWithFfmpegParametersPrompt;
            labelSubtitles.Text = string.Format(LanguageSettings.Current.GenerateVideoWithEmbeddedSubs.SubtitlesX, 0);

            LoadVideo(inputVideoFileName);
            AddCurrentSubtitle();
        }

        private void AddCurrentSubtitle()
        {
            if (_subtitle != null && _subtitle.Paragraphs.Count > 0)
            {
                var dir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
                Directory.CreateDirectory(dir);

                var nameOnly = string.IsNullOrEmpty(_subtitle.FileName)
                    ? "Untitled.srt"
                    : _subtitle.FileName;

                var fileName = Path.Combine(dir, nameOnly);
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
                    foreach (var track in matroska.GetTracks().Where(p => p.IsSubtitle))
                    {
                        AddListViewItem(track, matroska);
                    }

                    return;
                }
            }

            var mp4Parser = new MP4Parser(inputVideoFileName);
            if (mp4Parser.Moov != null && mp4Parser.VideoResolution.X > 0)
            {
                foreach (var track in mp4Parser.GetSubtitleTracks())
                {
                    AddListViewItem(track);
                }
            }
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
                Text = sub.SubtitleFormat != null ? sub.SubtitleFormat.Name : sub.Format,
            };
            item.SubItems.Add(GetDisplayLanguage(sub.Language));
            item.SubItems.Add(sub.IsDefault.ToString(CultureInfo.InvariantCulture));
            item.SubItems.Add(sub.IsForced.ToString(CultureInfo.InvariantCulture));
            item.SubItems.Add(sub.IsNew ? sub.FileName : string.Empty);
            listViewSubtitles.Items.Add(item);

            _softSubs.Add(sub);

            labelSubtitles.Text = string.Format(LanguageSettings.Current.GenerateVideoWithEmbeddedSubs.SubtitlesX, listViewSubtitles.Items.Count);
        }

        private static string GetDisplayLanguage(string language)
        {
            if (string.IsNullOrWhiteSpace(language))
            {
                return "Undefined";
            }

            var threeLetterCode = Iso639Dash2LanguageCode.GetThreeLetterCodeFromTwoLetterCode(language);
            if (language.Length == 3)
            {
                threeLetterCode = language;
            }

            return Iso639Dash2LanguageCode.List.FirstOrDefault(p => p.ThreeLetterCode == threeLetterCode)?.EnglishName;
        }

        private void AddListViewItem(MatroskaTrackInfo track, MatroskaFile matroska)
        {
            if (track.CodecId.Equals("S_VOBSUB", StringComparison.OrdinalIgnoreCase) ||
                track.CodecId.Equals("S_HDMV/PGS", StringComparison.OrdinalIgnoreCase) ||
                track.CodecId.Equals("S_HDMV/TEXTST", StringComparison.OrdinalIgnoreCase) ||
                track.CodecId.Equals("S_DVBSUB", StringComparison.OrdinalIgnoreCase))
            {
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
                IsNew = false,
                IsForced = track.IsForced,
                IsDefault = track.IsDefault,
                Tag = track,
                SubtitleFormat = format,
                FileName = fileName,
            });
        }

        private void AddListViewItem(string fileName)
        {
            listViewSubtitles.BeginUpdate();
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
                    Language = "eng", //TODO: get from file name or sup
                    Format = "Blu-ray sup",
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

            AddListViewItem(new VideoPreviewGeneratorSub
            {
                Name = Path.GetFileName(fileName),
                Language = LanguageAutoDetect.AutoDetectGoogleLanguage(subtitle),
                Format = subtitle.OriginalFormat.FriendlyName,
                SubtitleFormat = subtitle.OriginalFormat,
                IsNew = true,
                IsForced = false,
                IsDefault = false,
                FileName = fileName,
            });
            listViewSubtitles.EndUpdate();
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
                if (res != DialogResult.OK)
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

            if (File.Exists(VideoFileName))
            {
                try
                {
                    File.Delete(VideoFileName);
                }
                catch
                {
                    MessageBox.Show($"Cannot overwrite video file {VideoFileName} - probably in use!");
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
                                "For more info see the error log: " + SeLogger.ErrorFile);
                buttonGenerate.Enabled = true;
                groupBoxSettings.Enabled = true;
                return;
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
            fileName += ".embed";
            fileName = fileName.Replace(".", "_");
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

                LoadVideo(openFileDialog1.FileName);
            }
        }

        private void buttonAddSubtitles_Click(object sender, EventArgs e)
        {
            using (var openFileDialog1 = new OpenFileDialog())
            {
                openFileDialog1.Title = LanguageSettings.Current.General.OpenSubtitle;
                openFileDialog1.FileName = string.Empty;
                openFileDialog1.Filter = UiUtil.SubtitleExtensionFilter.Value;
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
            listViewSubtitles.BeginUpdate();
            listViewSubtitles.Items.Clear();
            listViewSubtitles.EndUpdate();
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
                listViewSubtitles.Items[index].SubItems[3].Text = _softSubs[index].IsForced.ToString(CultureInfo.InvariantCulture);
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
                listViewSubtitles.Items[selectedIndex].SubItems[2].Text = _softSubs[selectedIndex].IsDefault.ToString(CultureInfo.InvariantCulture);
                return;
            }

            for (var index = 0; index < listViewSubtitles.Items.Count; index++)
            {
                _softSubs[index].IsDefault = index == selectedIndex;
                listViewSubtitles.Items[index].SubItems[2].Text = _softSubs[index].IsDefault.ToString(CultureInfo.InvariantCulture);
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

            using (var cl = new Options.ChooseIsoLanguage())
            {
                if (cl.ShowDialog(this) == DialogResult.OK)
                {
                    foreach (int index in listViewSubtitles.SelectedIndices)
                    {
                        _softSubs[index].Language = cl.CultureName;
                        listViewSubtitles.Items[index].SubItems[1].Text = GetDisplayLanguage(cl.CultureName);
                    }
                }
            }
        }

        private void listViewSubtitles_SelectedIndexChanged(object sender, EventArgs e)
        {
            var count = listViewSubtitles.SelectedItems.Count;

            ButtonRemoveSubtitles.Enabled = count > 0;
            buttonClear.Enabled = count > 0;
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
    }
}
