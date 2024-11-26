using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Logic;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using MessageBox = Nikse.SubtitleEdit.Forms.SeMsgBox.MessageBox;

namespace Nikse.SubtitleEdit.Forms
{
    public sealed partial class GenerateTransparentVideoWithSubtitles : Form
    {
        public bool BatchMode { get; set; }
        public string BatchInfo { get; set; }
        public string VideoFileName { get; private set; }
        public long MillisecondsEncoding { get; private set; }

        private const int ListViewBatchSubItemIndexColumnSubtitleSize = 0;
        private const int ListViewBatchSubItemIndexColumnVideoFileName = 2;
        private const int ListViewBatchSubItemIndexColumnResolution = 3;
        private const int ListViewBatchSubItemIndexColumnStatus = 4;

        private bool _abort;
        private readonly Subtitle _assaSubtitle;
        private readonly List<int> _selectedLines;
        private readonly VideoInfo _videoInfo;
        private static readonly Regex FrameFinderRegex = new Regex(@"[Ff]rame=\s*\d+", RegexOptions.Compiled);
        private long _processedFrames;
        private long _startTicks;
        private long _totalFrames;
        private StringBuilder _log;
        private readonly bool _isAssa;
        private bool _promptFFmpegParameters;
        private readonly List<BatchItem> _batchItems;
        private bool _converting;
        private readonly string _inputVideoFileName;
        private bool _useVideoSourceResolution;

        public GenerateTransparentVideoWithSubtitles(Subtitle subtitle, List<int> selectedLines, SubtitleFormat format, string inputVideoFileName, VideoInfo videoInfo)
        {
            UiUtil.PreInitialize(this);
            InitializeComponent();
            textBoxLog.ScrollBars = ScrollBars.Both;
            UiUtil.FixFonts(this);

            _videoInfo = videoInfo;
            if (_videoInfo != null && (_videoInfo.Width <= 0 || _videoInfo.Height <= 0))
            {
                _videoInfo.Width = 1920;
                _videoInfo.Height = 1080;
            }

            _inputVideoFileName = inputVideoFileName;
            _assaSubtitle = new Subtitle(subtitle);
            _selectedLines = selectedLines;
            _batchItems = new List<BatchItem>();
            _useVideoSourceResolution = false;

            if (selectedLines.Count > 1)
            {
                radioButtonSelectedLinesOnly.Checked = true;
            }
            else
            {
                radioButtonAllLines.Checked = true;
            }

            if (format.GetType() == typeof(NetflixImsc11Japanese) && _videoInfo != null)
            {
                _assaSubtitle = new Subtitle();
                var raw = NetflixImsc11JapaneseToAss.Convert(subtitle, _videoInfo.Width, _videoInfo.Height);
                new AdvancedSubStationAlpha().LoadSubtitle(_assaSubtitle, raw.SplitToLines(), null);
            }

            Text = LanguageSettings.Current.Main.Menu.Video.GenerateTransparentVideoWithSubs.Trim('.');
            buttonGenerate.Text = LanguageSettings.Current.Watermark.Generate;
            labelPleaseWait.Text = LanguageSettings.Current.General.PleaseWait;
            labelResolution.Text = LanguageSettings.Current.SubStationAlphaProperties.Resolution;
            labelFontSize.Text = LanguageSettings.Current.ExportPngXml.FontSize;
            nikseLabelOutline.Text = LanguageSettings.Current.SubStationAlphaStyles.Outline;
            checkBoxFontBold.Text = LanguageSettings.Current.General.Bold;
            numericUpDownOutline.Text = LanguageSettings.Current.SubStationAlphaStyles.Outline;
            labelSubtitleFont.Text = LanguageSettings.Current.ExportPngXml.FontFamily;
            buttonOutlineColor.Text = LanguageSettings.Current.GenerateVideoWithBurnedInSubs.OutputSettings;
            buttonCancel.Text = LanguageSettings.Current.General.Cancel;
            checkBoxRightToLeft.Text = LanguageSettings.Current.Settings.FixRTLViaUnicodeChars;
            checkBoxAlignRight.Text = LanguageSettings.Current.GenerateVideoWithBurnedInSubs.AlignRight;
            checkBoxBox.Text = LanguageSettings.Current.SubStationAlphaStyles.OpaqueBox;
            deleteToolStripMenuItem.Text = LanguageSettings.Current.DvdSubRip.Remove;
            clearToolStripMenuItem.Text = LanguageSettings.Current.DvdSubRip.Clear;
            addFilesToolStripMenuItem.Text = LanguageSettings.Current.DvdSubRip.Add;
            labelFrameRate.Text = LanguageSettings.Current.General.FrameRate;
            radioButtonAllLines.Text = LanguageSettings.Current.ShowEarlierLater.AllLines;
            radioButtonSelectedLinesOnly.Text = LanguageSettings.Current.ShowEarlierLater.SelectedLinesOnly;
            radioButtonSelectedLineAndForward.Text = LanguageSettings.Current.ShowEarlierLater.SelectedLinesAndForward;
            nikseLabelBoxType.Text = LanguageSettings.Current.SubStationAlphaStyles.BoxType;
            useSourceResolutionToolStripMenuItem.Text = LanguageSettings.Current.GenerateVideoWithBurnedInSubs.UseSourceResolution;
            nikseComboBoxVideoExtension.Text = LanguageSettings.Current.ExportCustomTextFormat.FileExtension;

            comboBoxOpaqueBoxStyle.Items.Clear();
            comboBoxOpaqueBoxStyle.Items.Add(LanguageSettings.Current.SubStationAlphaStyles.BoxPerLineShort);
            comboBoxOpaqueBoxStyle.Items.Add(LanguageSettings.Current.SubStationAlphaStyles.BoxMultiLineShort);
            comboBoxOpaqueBoxStyle.SelectedIndex = Configuration.Settings.Tools.GenTransparentVideoNonAssaBoxPerLine ? 0 : 1;
            comboBoxOpaqueBoxStyle.Enabled = false;

            nikseComboBoxVideoExtension.Items.Clear();
            nikseComboBoxVideoExtension.Items.Add(".mkv");
            nikseComboBoxVideoExtension.Items.Add(".mp4");
            nikseComboBoxVideoExtension.Items.Add(".webm");
            nikseComboBoxVideoExtension.Items.Add(".mov");
            nikseComboBoxVideoExtension.Text = Configuration.Settings.Tools.GenTransparentVideoExtension;

            progressBar1.Visible = false;
            labelPleaseWait.Visible = false;
            labelProgress.Text = string.Empty;
            labelPass.Text = string.Empty;
            groupBoxSettings.Text = LanguageSettings.Current.Settings.Title;
            groupBoxVideo.Text = LanguageSettings.Current.Main.Menu.Video.Title;
            promptParameterBeforeGenerateToolStripMenuItem.Text = LanguageSettings.Current.GenerateBlankVideo.GenerateWithFfmpegParametersPrompt;

            checkBoxBox.Checked = Configuration.Settings.Tools.GenTransparentVideoNonAssaBox;
            checkBoxAlignRight.Checked = Configuration.Settings.Tools.GenVideoNonAssaAlignRight;
            checkBoxRightToLeft.Checked = Configuration.Settings.Tools.GenVideoNonAssaAlignRight;

            var left = Math.Max(Math.Max(labelResolution.Left + labelResolution.Width, labelFontSize.Left + labelFontSize.Width), labelSubtitleFont.Left + labelSubtitleFont.Width) + 5;
            numericUpDownFontSize.Left = left;
            comboBoxSubtitleFont.Left = left;
            numericUpDownOutline.Left = left;
            numericUpDownWidth.Left = left;
            comboBoxFrameRate.Left = left;
            comboBoxOpaqueBoxStyle.Left = left;
            labelX.Left = numericUpDownWidth.Left + numericUpDownWidth.Width + 3;
            numericUpDownHeight.Left = labelX.Left + labelX.Width + 3;
            buttonVideoChooseStandardRes.Left = numericUpDownHeight.Left + numericUpDownHeight.Width + 9;
            labelInfo.Text = LanguageSettings.Current.GenerateVideoWithBurnedInSubs.InfoAssaOff;
            nikseLabelVideoExtension.Left = comboBoxFrameRate.Right + 9;
            nikseComboBoxVideoExtension.Left = nikseLabelVideoExtension.Right + 3;

            checkBoxFontBold.Left = numericUpDownFontSize.Right + 12;
            checkBoxBox.Left = numericUpDownOutline.Right + 12;

            checkBoxRightToLeft.Left = left;
            checkBoxAlignRight.Left = checkBoxRightToLeft.Right + 12;
            buttonOutlineColor.Left = checkBoxBox.Right + 2;
            buttonOutlineColor.Text = LanguageSettings.Current.AssaSetBackgroundBox.BoxColor;
            panelOutlineColor.Left = buttonOutlineColor.Right + 3;
            buttonForeColor.Left = buttonOutlineColor.Left;
            panelForeColor.Left = panelOutlineColor.Left;
            buttonForeColor.Text = LanguageSettings.Current.Settings.WaveformTextColor;
            buttonColorShadow.Left = buttonOutlineColor.Left;
            panelShadowColor.Left = buttonColorShadow.Right + 3;

            _isAssa = format.GetType() == typeof(AdvancedSubStationAlpha);

            checkBoxFontBold.Checked = Configuration.Settings.Tools.GenVideoFontBold;
            numericUpDownOutline.Value = Configuration.Settings.Tools.GenVideoOutline;

            var initialFont = Configuration.Settings.Tools.GenVideoFontName;
            if (string.IsNullOrEmpty(initialFont))
            {
                initialFont = Configuration.Settings.Tools.ExportBluRayFontName;
            }

            labelInfo.Text = LanguageSettings.Current.GenerateVideoWithBurnedInSubs.InfoAssaOn;
            if (string.IsNullOrEmpty(initialFont))
            {
                initialFont = UiUtil.GetDefaultFont().Name;
            }

            foreach (var x in FontHelper.GetRegularOrBoldCapableFontFamilies())
            {
                comboBoxSubtitleFont.Items.Add(x.Name);
                if (x.Name.Equals(initialFont, StringComparison.OrdinalIgnoreCase))
                {
                    comboBoxSubtitleFont.SelectedIndex = comboBoxSubtitleFont.Items.Count - 1;
                }
            }

            if (comboBoxSubtitleFont.SelectedIndex < 0 && comboBoxSubtitleFont.Items.Count > 0)
            {
                comboBoxSubtitleFont.SelectedIndex = 0;
            }

            if (Configuration.Settings.Tools.GenVideoFontSize >= numericUpDownFontSize.Minimum &&
                Configuration.Settings.Tools.GenVideoFontSize >= numericUpDownFontSize.Minimum)
            {
                numericUpDownFontSize.Value = Configuration.Settings.Tools.GenVideoFontSize;
            }

            checkBoxRightToLeft.Checked = Configuration.Settings.General.RightToLeftMode && LanguageAutoDetect.CouldBeRightToLeftLanguage(_assaSubtitle);
            textBoxLog.Visible = false;

            UiUtil.FixLargeFonts(this, buttonGenerate);
            UiUtil.FixFonts(this, 2000);

            comboBoxFrameRate.Items.Clear();
            comboBoxFrameRate.Items.Add("23.976");
            comboBoxFrameRate.Items.Add("24");
            comboBoxFrameRate.Items.Add("25");
            comboBoxFrameRate.Items.Add("29.97");
            comboBoxFrameRate.Items.Add("30");
            comboBoxFrameRate.Items.Add("50");
            comboBoxFrameRate.Items.Add("59.94");
            comboBoxFrameRate.Items.Add("60");
            comboBoxFrameRate.SelectedIndex = 2;

            FontEnableOrDisable(_isAssa);

            panelOutlineColor.BackColor = Configuration.Settings.Tools.GenVideoNonAssaBoxColor;
            panelForeColor.BackColor = Configuration.Settings.Tools.GenVideoNonAssaTextColor;
            panelShadowColor.BackColor = Configuration.Settings.Tools.GenVideoNonAssaShadowColor;

            var hasSubtitle = subtitle != null && subtitle.Paragraphs.Count > 0;
            BatchMode = hasSubtitle;
            buttonMode_Click(null, null);
            if (!hasSubtitle)
            {
                buttonMode.Enabled = false;
            }
        }

        private void FontEnableOrDisable(bool assa)
        {
            var enabled = !assa;

            numericUpDownFontSize.Enabled = enabled;
            numericUpDownFontSize.Enabled = enabled;
            labelFontSize.Enabled = enabled;
            nikseLabelOutline.Enabled = enabled;
            labelSubtitleFont.Enabled = enabled;
            comboBoxSubtitleFont.Enabled = enabled;
            checkBoxAlignRight.Enabled = enabled;
            checkBoxBox.Enabled = enabled;
            checkBoxFontBold.Enabled = enabled;
            numericUpDownOutline.Enabled = enabled;
            buttonForeColor.Enabled = enabled;
            buttonOutlineColor.Enabled = enabled;
            panelOutlineColor.Enabled = enabled;
            panelForeColor.Enabled = enabled;

            labelInfo.Text = assa ? LanguageSettings.Current.GenerateVideoWithBurnedInSubs.InfoAssaOn : string.Empty;
        }

        private void FixRightToLeft(Subtitle subtitle)
        {
            if (!checkBoxRightToLeft.Checked)
            {
                return;
            }

            for (var index = 0; index < subtitle.Paragraphs.Count; index++)
            {
                var paragraph = subtitle.Paragraphs[index];
                if (LanguageAutoDetect.ContainsRightToLeftLetter(paragraph.Text))
                {
                    paragraph.Text = Utilities.FixRtlViaUnicodeChars(paragraph.Text);
                }
            }
        }

        private void SetStyleForNonAssa(Subtitle sub)
        {
            sub.Header = AdvancedSubStationAlpha.DefaultHeader;
            var style = AdvancedSubStationAlpha.GetSsaStyle("Default", sub.Header);
            style.FontSize = numericUpDownFontSize.Value;
            style.Bold = checkBoxFontBold.Checked;
            style.FontName = comboBoxSubtitleFont.Text;
            style.Background = Color.Cyan; // panelOutlineColor.Color;
            style.Tertiary = panelShadowColor.BackColor;
            style.Primary = panelForeColor.BackColor;
            style.Secondary = Color.Cyan;
            style.Outline = panelOutlineColor.BackColor;
            style.OutlineWidth = numericUpDownOutline.Value;
            style.ShadowWidth = style.OutlineWidth * 0.5m;



            if (comboBoxOpaqueBoxStyle.Enabled == false)
            {
                style.BorderStyle = "0"; // bo box
                style.Background = panelShadowColor.BackColor;
            }
            else if (comboBoxOpaqueBoxStyle.SelectedIndex == 0)
            {
                style.BorderStyle = "3"; // box - per line
                style.Outline = panelShadowColor.BackColor;
                style.Background = panelOutlineColor.BackColor;
            }
            else
            {
                style.Background = panelShadowColor.BackColor;
                style.BorderStyle = "4"; // box - multi line
            }


            if (checkBoxAlignRight.Checked)
            {
                style.Alignment = "3";
            }

            sub.Header = AdvancedSubStationAlpha.GetHeaderAndStylesFromAdvancedSubStationAlpha(sub.Header, new List<SsaStyle> { style });
            sub.Header = AdvancedSubStationAlpha.AddTagToHeader("PlayResX", "PlayResX: " + ((int)numericUpDownWidth.Value).ToString(CultureInfo.InvariantCulture), "[Script Info]", sub.Header);
            sub.Header = AdvancedSubStationAlpha.AddTagToHeader("PlayResY", "PlayResY: " + ((int)numericUpDownHeight.Value).ToString(CultureInfo.InvariantCulture), "[Script Info]", sub.Header);
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            _abort = true;
            if (buttonGenerate.Enabled)
            {
                DialogResult = DialogResult.Cancel;
            }
        }

        private void buttonGenerate_Click(object sender, EventArgs e)
        {
            if (BatchMode && _batchItems.Count == 0)
            {
                MessageBox.Show("No subtitle files added for batch.");
                return;
            }

            _converting = true;
            var selectionSubtitle = GetSubtitleBasedOnSelection();
            var subtitle = GetSubtitleBasedOnCut(selectionSubtitle);

            CalculateTotalFrames(subtitle);

            labelProgress.Text = string.Empty;
            labelProgress.ForeColor = UiUtil.ForeColor;

            _log = new StringBuilder();
            buttonGenerate.Enabled = false;
            buttonPreview.Enabled = false;
            buttonMode.Enabled = false;
            var oldFontSizeEnabled = numericUpDownFontSize.Enabled;
            numericUpDownFontSize.Enabled = false;
            var stopWatch = Stopwatch.StartNew();
            var sbInfo = new StringBuilder();
            sbInfo.AppendLine("Conversion report:");
            sbInfo.AppendLine();
            var okCount = 0;
            var failCount = 0;

            if (BatchMode)
            {
                if (_useVideoSourceResolution)
                {
                    foreach (var batchItem in _batchItems)
                    {
                        if (batchItem.Width <= 0 || batchItem.Height <= 0)
                        {
                            MessageBox.Show("Source resolution requires that all batch items has a video with resolution!" + Environment.NewLine +
                                            "Check the list view context menu.");
                            return;
                        }
                    }
                }

                for (var i = 0; i < listViewBatch.Items.Count; i++)
                {
                    listViewBatch.Items[i].SubItems[ListViewBatchSubItemIndexColumnStatus].Text = string.Empty;
                }

                listViewBatch.SelectedIndices.Clear();
                listViewBatch.Refresh();

                for (var index = 0; index < _batchItems.Count; index++)
                {
                    var batchItem = _batchItems[index];

                    labelPleaseWait.Text = $"{index + 1}/{_batchItems.Count} - {LanguageSettings.Current.General.PleaseWait}";
                    VideoFileName = GetBatchFileName(batchItem);
                    listViewBatch.Items[index].Selected = true;
                    listViewBatch.Items[index].Focused = true;
                    listViewBatch.Items[index].EnsureVisible();
                    Refresh();

                    if (_useVideoSourceResolution)
                    {
                        numericUpDownWidth.Value = batchItem.Width;
                        numericUpDownHeight.Value = batchItem.Height;
                    }

                    var batchSubtitle = Subtitle.Parse(batchItem.SubtitleFileName);
                    CalculateTotalFrames(batchSubtitle);
                    if (ConvertVideo(oldFontSizeEnabled, batchSubtitle))
                    {
                        listViewBatch.Items[index].SubItems[ListViewBatchSubItemIndexColumnStatus].Text = LanguageSettings.Current.BatchConvert.Converted;
                        sbInfo.AppendLine($"{index + 1}: {batchItem.SubtitleFileName} -> {VideoFileName}");
                        okCount++;
                    }
                    else
                    {
                        listViewBatch.Items[index].SubItems[ListViewBatchSubItemIndexColumnStatus].Text = "Error";
                        failCount++;
                    }

                    if (_abort)
                    {
                        break;
                    }
                }

                sbInfo.AppendLine();

                var timeString = $"{stopWatch.Elapsed.Hours + stopWatch.Elapsed.Days * 24:00}:{stopWatch.Elapsed.Minutes:00}:{stopWatch.Elapsed.Seconds:00}";
                if (okCount == 1)
                {
                    sbInfo.AppendLine($"One video file converted in {timeString}");
                }
                else
                {
                    sbInfo.AppendLine($"{okCount} video files converted in {timeString}");
                }

                if (failCount > 0)
                {
                    sbInfo.AppendLine($"{failCount} video file(s) failed!");
                }

                BatchInfo = sbInfo.ToString();
            }
            else
            {
                using (var saveDialog = new SaveFileDialog
                {
                    FileName = SuggestNewVideoFileName(),
                    Filter = "MP4|*.mp4|Matroska|*.mkv|WebM|*.webm|mov|*.mov",
                    AddExtension = true,
                    InitialDirectory = string.IsNullOrEmpty(_assaSubtitle.FileName) ? string.Empty : Path.GetDirectoryName(_assaSubtitle.FileName),
                })
                {
                    if (saveDialog.ShowDialog(this) != DialogResult.OK)
                    {
                        buttonGenerate.Enabled = true;
                        buttonPreview.Enabled = true;
                        buttonMode.Enabled = true;
                        numericUpDownFontSize.Enabled = true;
                        _converting = false;
                        return;
                    }

                    VideoFileName = saveDialog.FileName;
                }

                if (!ConvertVideo(oldFontSizeEnabled, subtitle))
                {
                    buttonGenerate.Enabled = true;
                    buttonPreview.Enabled = true;
                    buttonMode.Enabled = true;
                    numericUpDownFontSize.Enabled = true;
                    progressBar1.Visible = false;
                    labelPleaseWait.Visible = false;
                    timer1.Stop();
                    MillisecondsEncoding = stopWatch.ElapsedMilliseconds;
                    labelProgress.Text = string.Empty;
                    groupBoxSettings.Enabled = true;
                    _converting = false;

                    if (!_abort && _log.ToString().Length > 10)
                    {
                        var title = "Error occurred during encoding";
                        MessageBox.Show($"Encoding with ffmpeg failed: {Environment.NewLine}{_log}", title, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }

                    return;
                }
            }

            progressBar1.Visible = false;
            labelPleaseWait.Visible = false;
            timer1.Stop();
            MillisecondsEncoding = stopWatch.ElapsedMilliseconds;
            labelProgress.Text = string.Empty;
            groupBoxSettings.Enabled = true;
            _converting = false;

            if (_abort)
            {
                DialogResult = DialogResult.Cancel;
                buttonGenerate.Enabled = true;
                buttonPreview.Enabled = true;
                buttonMode.Enabled = true;
                numericUpDownFontSize.Enabled = true;
                return;
            }

            if (BatchMode)
            {
                MessageBox.Show(BatchInfo);
            }
            else if (!File.Exists(VideoFileName) || new FileInfo(VideoFileName).Length == 0)
            {
                SeLogger.Error(Environment.NewLine + "Generate video failed: " + Environment.NewLine + _log);
                MessageBox.Show("Generate embedded video failed" + Environment.NewLine +
                                "For more info see the error log: " + SeLogger.ErrorFile);
                buttonGenerate.Enabled = true;
                buttonPreview.Enabled = true;
                buttonMode.Enabled = true;
                numericUpDownFontSize.Enabled = oldFontSizeEnabled;
                return;
            }
            else
            {
                var encodingTime = new TimeCode(MillisecondsEncoding).ToString();
                using (var f = new ExportPngXmlDialogOpenFolder(string.Format(LanguageSettings.Current.GenerateVideoWithBurnedInSubs.XGeneratedWithBurnedInSubsInX, Path.GetFileName(VideoFileName), encodingTime), Path.GetDirectoryName(VideoFileName), VideoFileName))
                {
                    f.ShowDialog(this);
                }
            }

            buttonGenerate.Enabled = true;
            buttonPreview.Enabled = true;
            buttonMode.Enabled = true;
            numericUpDownFontSize.Enabled = true;
        }

        private string GetBatchFileName(BatchItem batchItem)
        {
            var path = Path.GetDirectoryName(batchItem.SubtitleFileName);
            if (Configuration.Settings.Tools.GenVideoUseOutputFolder &&
                !string.IsNullOrEmpty(Configuration.Settings.Tools.GenVideoOutputFolder) &&
                Directory.Exists(Configuration.Settings.Tools.GenVideoOutputFolder))
            {
                path = Configuration.Settings.Tools.GenVideoOutputFolder;
            }

            var nameNoExt = Path.GetFileNameWithoutExtension(batchItem.SubtitleFileName);

            var ext = Configuration.Settings.Tools.GenTransparentVideoExtension;
            if (string.IsNullOrEmpty(ext) || !ext.StartsWith("."))
            {
                ext = ".mkv";
            }

            var outputFileName = Path.Combine(path, $"{nameNoExt.TrimEnd('.', '.')}{Configuration.Settings.Tools.GenVideoOutputFileSuffix}{ext}");
            return outputFileName;
        }

        private TimeSpan GetCutEnd()
        {
            return TimeSpan.FromSeconds(
                (double)numericUpDownCutToHours.Value * 60 * 60 +
                (double)numericUpDownCutToMinutes.Value * 60 +
                (double)numericUpDownCutToSeconds.Value);
        }

        private TimeSpan GetCutStart()
        {
            return TimeSpan.FromSeconds(
                (double)numericUpDownCutFromHours.Value * 60 * 60 +
                (double)numericUpDownCutFromMinutes.Value * 60 +
                (double)numericUpDownCutFromSeconds.Value);
        }

        private Subtitle GetSubtitleBasedOnCut(Subtitle selectionSubtitle)
        {
            if (!checkBoxCut.Checked)
            {
                return selectionSubtitle;
            }

            var subtitle = new Subtitle();
            var start = GetCutStart();
            var end = GetCutEnd();
            foreach (var p in selectionSubtitle.Paragraphs)
            {
                if (p.StartTime.TotalMilliseconds >= start.TotalMilliseconds && p.EndTime.TotalMilliseconds <= end.TotalMilliseconds)
                {
                    subtitle.Paragraphs.Add(new Paragraph(p));
                }
            }

            subtitle.AddTimeToAllParagraphs(TimeSpan.FromMilliseconds(-start.TotalMilliseconds));

            return subtitle;
        }

        private Subtitle GetSubtitleBasedOnSelection()
        {
            if (BatchMode)
            {
                return _assaSubtitle;
            }

            var subtitle = new Subtitle(_assaSubtitle);
            if (radioButtonSelectedLinesOnly.Checked)
            {
                subtitle.Paragraphs.Clear();
                foreach (var index in _selectedLines)
                {
                    subtitle.Paragraphs.Add(_assaSubtitle.Paragraphs[index]);
                }
            }
            else if (radioButtonSelectedLineAndForward.Checked)
            {
                subtitle.Paragraphs.Clear();
                foreach (var index in _selectedLines)
                {
                    subtitle.Paragraphs.Add(_assaSubtitle.Paragraphs[index]);
                }

                var last = _selectedLines.Max();
                for (var i = last + 1; i < _assaSubtitle.Paragraphs.Count; i++)
                {
                    subtitle.Paragraphs.Add(_assaSubtitle.Paragraphs[i]);
                }
            }

            return subtitle;
        }

        private void CalculateTotalFrames(Subtitle subtitle)
        {
            if (subtitle == null || subtitle.Paragraphs.Count == 0)
            {
                _totalFrames = 0;
                return;
            }

            var seconds = subtitle.Paragraphs.Max(p => p.EndTime.TotalSeconds);
            var frameRate = double.Parse(comboBoxFrameRate.Text, CultureInfo.InvariantCulture);
            _totalFrames = (long)Math.Round(seconds * frameRate, MidpointRounding.AwayFromZero) + 1;
        }

        private static string GetAssaFileName(string inputVideoFileName)
        {
            var path = string.IsNullOrEmpty(inputVideoFileName) ? string.Empty : Path.GetDirectoryName(inputVideoFileName);
            for (var i = 0; i < int.MaxValue; i++)
            {
                var guidLetters = Guid.NewGuid().ToString().RemoveChar('0', '1', '2', '3', '4', '5', '6', '7', '8', '9', '-');
                var fileName = Path.Combine(path, $"{guidLetters}.ass");
                if (!File.Exists(fileName))
                {
                    return fileName;
                }
            }

            return Path.Combine(path, "qwerty12.ass");
        }

        private string SuggestNewVideoFileName()
        {
            var fileName = Path.GetFileNameWithoutExtension(_assaSubtitle.FileName);

            fileName += ".transparent-subs";

            if (numericUpDownWidth.Value > 0 && numericUpDownHeight.Value > 0)
            {
                fileName += $".{numericUpDownWidth.Value}x{numericUpDownHeight.Value}";
            }

            return fileName.Replace(".", "_") + ".mp4";
        }

        private bool ConvertVideo(bool oldFontSizeEnabled, Subtitle subtitle)
        {
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
                    numericUpDownFontSize.Enabled = oldFontSizeEnabled;
                    return false;
                }
            }

            _log = new StringBuilder();

            if (!_isAssa)
            {
                SetStyleForNonAssa(subtitle);
            }

            FixRightToLeft(subtitle);

            var format = new AdvancedSubStationAlpha();
            var assaTempFileName = GetAssaFileName(_assaSubtitle.FileName);

            if (Configuration.Settings.General.CurrentVideoIsSmpte && comboBoxFrameRate.Text.Contains(".", StringComparison.Ordinal))
            {
                foreach (var assaP in subtitle.Paragraphs)
                {
                    assaP.StartTime.TotalMilliseconds *= 1.001;
                    assaP.EndTime.TotalMilliseconds *= 1.001;
                }
            }

            FileUtil.WriteAllText(assaTempFileName, format.ToText(subtitle, null), new TextEncoding(Encoding.UTF8, "UTF8"));

            groupBoxSettings.Enabled = false;
            labelPleaseWait.Visible = true;
            labelPleaseWait.Text = LanguageSettings.Current.General.PleaseWait;

            if (_totalFrames > 0)
            {
                progressBar1.Visible = true;
            }

            var result = RunOnePassEncoding(assaTempFileName, subtitle, VideoFileName);

            try
            {
                File.Delete(assaTempFileName);
            }
            catch
            {
                // ignore
            }

            return result;
        }

        private bool CheckForPromptParameters(Process process, string title)
        {
            if (!_promptFFmpegParameters)
            {
                return true;
            }

            using (var form = new GenerateVideoFFmpegPrompt(title, process.StartInfo.Arguments))
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

        private Process GetFfmpegProcess(string outputVideoFileName, string assaTempFileName, Subtitle subtitle)
        {
            var totalMs = subtitle.Paragraphs.Max(p => p.EndTime.TotalMilliseconds);
            var ts = TimeSpan.FromMilliseconds(totalMs + 2000);
            var timeCode = string.Format($"{ts.Hours:00}\\\\:{ts.Minutes:00}\\\\:{ts.Seconds:00}");

            return VideoPreviewGenerator.GenerateTransparentVideoFile(
                assaTempFileName,
                outputVideoFileName,
                (int)numericUpDownWidth.Value,
                (int)numericUpDownHeight.Value,
                comboBoxFrameRate.Text,
                timeCode,
                OutputHandler);
        }

        private void OutputHandler(object sendingProcess, DataReceivedEventArgs outLine)
        {
            if (string.IsNullOrWhiteSpace(outLine.Data))
            {
                return;
            }

            _log?.AppendLine(outLine.Data);

            var match = FrameFinderRegex.Match(outLine.Data);
            if (!match.Success)
            {
                return;
            }

            var arr = match.Value.Split('=');
            if (arr.Length != 2)
            {
                return;
            }

            if (long.TryParse(arr[1].Trim(), out var f))
            {
                _processedFrames = f;
            }
        }

        private bool RunOnePassEncoding(string assaTempFileName, Subtitle subtitle, string videoFileName)
        {
            var process = GetFfmpegProcess(videoFileName, assaTempFileName, subtitle);
            _log.AppendLine("ffmpeg arguments: " + process.StartInfo.Arguments);

            if (!CheckForPromptParameters(process, Text))
            {
                _abort = true;
                return false;
            }

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            _startTicks = Stopwatch.GetTimestamp();
            timer1.Start();

            while (!process.HasExited)
            {
                Application.DoEvents();
                WindowsHelper.PreventStandBy();
                System.Threading.Thread.Sleep(100);
                if (_abort)
                {
                    process.Kill();
                    return false;
                }

                var v = (int)_processedFrames;
                SetProgress(v);
            }

            if (_abort)
            {
                return false;
            }

            if (process.ExitCode != 0)
            {
                _log.AppendLine("ffmpeg exit code: " + process.ExitCode);
                return false;
            }

            return true;
        }

        private void SetProgress(int v)
        {
            if (_totalFrames == 0)
            {
                progressBar1.Value = progressBar1.Maximum;
            }

            var pct = Math.Min(progressBar1.Maximum, (int)Math.Round(v * 100.0 / _totalFrames, MidpointRounding.AwayFromZero));
            if (pct >= progressBar1.Minimum && pct <= progressBar1.Maximum && _totalFrames > 0)
            {
                progressBar1.Value = pct;
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (_processedFrames <= 0)
            {
                return;
            }

            var durationMs = (Stopwatch.GetTimestamp() - _startTicks) / 10_000;
            var msPerFrame = (float)durationMs / _processedFrames;
            var estimatedTotalMs = msPerFrame * _totalFrames;
            var estimatedLeft = ProgressHelper.ToProgressTime(estimatedTotalMs - durationMs);
            labelProgress.Text = estimatedLeft;
        }

        private void promptParameterBeforeGenerateToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _promptFFmpegParameters = true;
            buttonGenerate_Click(null, null);
        }

        private void ResolutionPickClick(object sender, EventArgs e)
        {
            _useVideoSourceResolution = false;

            labelX.Left = numericUpDownWidth.Left + numericUpDownWidth.Width + 3;
            numericUpDownWidth.Visible = true;
            labelX.Text = "x";
            numericUpDownHeight.Visible = true;

            var text = (sender as ToolStripMenuItem).Text;
            var match = new Regex("\\d+x\\d+").Match(text);
            var parts = match.Value.Split('x');
            numericUpDownWidth.Value = int.Parse(parts[0]);
            numericUpDownHeight.Value = int.Parse(parts[1]);
        }

        private void buttonVideoChooseStandardRes_Click(object sender, EventArgs e)
        {
            var coordinates = buttonVideoChooseStandardRes.PointToClient(Cursor.Position);
            contextMenuStripRes.Show(buttonVideoChooseStandardRes, coordinates);
        }

        private void buttonForeColor_Click(object sender, EventArgs e)
        {
            using (var colorChooser = new ColorChooser { Color = panelForeColor.BackColor })
            {
                if (colorChooser.ShowDialog() == DialogResult.OK)
                {
                    panelForeColor.BackColor = colorChooser.Color;
                }
            }
        }

        private void buttonOutlineColor_Click(object sender, EventArgs e)
        {
            using (var colorChooser = new ColorChooser { Color = panelOutlineColor.BackColor })
            {
                if (colorChooser.ShowDialog() == DialogResult.OK)
                {
                    panelOutlineColor.BackColor = colorChooser.Color;
                }
            }
        }

        private void panelForeColor_Click(object sender, EventArgs e)
        {
            buttonForeColor_Click(null, null);
        }

        private void panelOutlineColor_Click(object sender, EventArgs e)
        {
            buttonOutlineColor_Click(null, null);
        }

        private void GenerateTransparentVideoWithSubtitles_FormClosing(object sender, FormClosingEventArgs e)
        {
            Configuration.Settings.Tools.GenVideoFontName = comboBoxSubtitleFont.Text;
            Configuration.Settings.Tools.GenVideoFontBold = checkBoxFontBold.Checked;
            Configuration.Settings.Tools.GenVideoOutline = (int)numericUpDownOutline.Value;
            Configuration.Settings.Tools.GenVideoFontSize = (int)numericUpDownFontSize.Value;
            Configuration.Settings.Tools.GenTransparentVideoNonAssaBox = checkBoxBox.Checked;
            Configuration.Settings.Tools.GenVideoNonAssaAlignRight = checkBoxAlignRight.Checked;
            Configuration.Settings.Tools.GenVideoNonAssaFixRtlUnicode = checkBoxRightToLeft.Checked;
            Configuration.Settings.Tools.GenVideoNonAssaBoxColor = panelOutlineColor.BackColor;
            Configuration.Settings.Tools.GenVideoNonAssaTextColor = panelForeColor.BackColor;
            Configuration.Settings.Tools.GenVideoNonAssaShadowColor = panelShadowColor.BackColor;
            Configuration.Settings.Tools.GenTransparentVideoNonAssaBoxPerLine = comboBoxOpaqueBoxStyle.SelectedIndex == 0;
            Configuration.Settings.Tools.GenTransparentVideoExtension = nikseComboBoxVideoExtension.Text;
        }

        private void radioButtonAllLines_CheckedChanged(object sender, EventArgs e)
        {
            var lineCount = GetSubtitleBasedOnSelection().Paragraphs.Count;
            groupBoxSelection.Text = string.Format(LanguageSettings.Current.Split.NumberOfLinesX, lineCount);
        }

        private void GenerateTransparentVideoWithSubtitles_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                _abort = true;
                DialogResult = DialogResult.Cancel;
            }
        }

        private void buttonMode_Click(object sender, EventArgs e)
        {
            BatchMode = !BatchMode;

            listViewBatch.Visible = BatchMode;
            labelInfo.Visible = !BatchMode;
            groupBoxSelection.Visible = !BatchMode;
            buttonAddFile.Visible = BatchMode;
            buttonRemoveFile.Visible = BatchMode;
            buttonClear.Visible = BatchMode;
            buttonOutputFileSettings.Visible = BatchMode;
            labelProgress.Text = string.Empty;
            buttonMode.Text = BatchMode
                ? LanguageSettings.Current.Split.Basic
                : LanguageSettings.Current.AudioToText.BatchMode;

            if (!numericUpDownWidth.Visible)
            {
                var item = new ToolStripMenuItem();
                item.Text = (_videoInfo == null || _videoInfo.Width == 0 || _videoInfo.Height == 0)
                    ? "(1920x1080)"
                    : $"({_videoInfo.Width}x{_videoInfo.Height})";

                ResolutionPickClick(item, null);
            }

            Height = BatchMode ? 600 : 425;
            ShowLabelOutput();
            listViewBatch.AutoSizeLastColumn();
        }

        private static string GetSubtitleFilter()
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

            return sb.ToString();
        }


        private void buttonAddFile_Click(object sender, EventArgs e)
        {
            using (var openFileDialog1 = new OpenFileDialog())
            {
                openFileDialog1.Title = LanguageSettings.Current.General.OpenSubtitle;
                openFileDialog1.FileName = string.Empty;
                openFileDialog1.Filter = GetSubtitleFilter();
                openFileDialog1.Multiselect = true;
                if (openFileDialog1.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }

                try
                {
                    Cursor = Cursors.WaitCursor;
                    Refresh();
                    Application.DoEvents();
                    for (var i = 0; i < listViewBatch.Columns.Count; i++)
                    {
                        ListViewSorter.SetSortArrow(listViewBatch.Columns[i], SortOrder.None);
                    }

                    foreach (var fileName in openFileDialog1.FileNames)
                    {
                        Application.DoEvents();
                        AddInputFile(fileName);
                    }
                }
                finally
                {
                    Cursor = Cursors.Default;
                }
            }
        }

        private void AddInputFile(string subtitleFileName)
        {
            if (string.IsNullOrEmpty(subtitleFileName))
            {
                return;
            }

            var ext = Path.GetExtension(subtitleFileName).ToLowerInvariant();
            if (SubtitleFormat.AllSubtitleFormats.Any(p => p.Extension == ext) && File.Exists(subtitleFileName))
            {
                var batchItem = CreateBatchItem(subtitleFileName);
                var listViewItem = new ListViewItem(subtitleFileName) { Tag = batchItem };
                var s = Utilities.FormatBytesToDisplayFileSize(batchItem.SubtitleFileFileSizeInBytes);
                listViewItem.SubItems.Add(s);
                var videoText = Path.GetFileName(batchItem.VideoFileName);
                var resolution = string.Empty;
                if (!string.IsNullOrEmpty(videoText))
                {
                    resolution = $"{batchItem.Width}x{batchItem.Height}";
                }
                listViewItem.SubItems.Add(videoText);
                listViewItem.SubItems.Add(resolution);
                listViewItem.SubItems.Add(string.Empty); // status
                listViewBatch.Items.Add(listViewItem);
                _batchItems.Add(batchItem);
            }
        }

        private static BatchItem CreateBatchItem(string subtitleFileName)
        {
            var fileNameOnlyNoExtension = Path.GetFileNameWithoutExtension(subtitleFileName);
            var fileNameNoExtension = Path.Combine(Path.GetDirectoryName(subtitleFileName) ?? string.Empty, fileNameOnlyNoExtension);
            var videoFileName = TryFindVideoFileName(fileNameNoExtension);
            var width = 0;
            var height = 0;

            if (!string.IsNullOrEmpty(videoFileName))
            {
                var mediaInfo = FfmpegMediaInfo.Parse(videoFileName);
                if (mediaInfo.Dimension.IsValid())
                {
                    width = mediaInfo.Dimension.Width;
                    height = mediaInfo.Dimension.Height;
                }
            }

            var batchVideoAndSub = new BatchItem
            {
                SubtitleFileName = subtitleFileName,
                SubtitleFileFileSizeInBytes = new FileInfo(subtitleFileName).Length,
                VideoFileName = videoFileName,
                Width = width,
                Height = height,
            };

            return batchVideoAndSub;
        }

        private static string TryFindVideoFileName(string fileNameNoExtension)
        {
            if (string.IsNullOrEmpty(fileNameNoExtension))
            {
                return string.Empty;
            }

            foreach (var extension in Utilities.VideoFileExtensions.Concat(Utilities.AudioFileExtensions))
            {
                var fileName = fileNameNoExtension + extension;
                if (File.Exists(fileName))
                {
                    var skipLoad = false;
                    if (extension == ".m2ts" && new FileInfo(fileName).Length < 2000000)
                    {
                        var textSt = new TextST();
                        skipLoad = textSt.IsMine(null, fileName); // don't load TextST files as video/audio file
                    }

                    if (!skipLoad)
                    {
                        return fileName;
                    }
                }
            }

            var index = fileNameNoExtension.LastIndexOf('.');
            if (index > 0 && index > fileNameNoExtension.LastIndexOf(Path.DirectorySeparatorChar))
            {
                return TryFindVideoFileName(fileNameNoExtension.Remove(index));
            }

            return string.Empty;
        }

        public class BatchItem
        {
            public string SubtitleFileName { get; set; }
            public long SubtitleFileFileSizeInBytes { get; set; }
            public string VideoFileName { get; set; }
            public int Width { get; set; }
            public int Height { get; set; }
        }

        private void buttonClear_Click(object sender, EventArgs e)
        {
            ClearBatchItems();
        }

        private void buttonRemoveFile_Click(object sender, EventArgs e)
        {
            RemoveSelectedBatchItems();
        }

        private void ClearBatchItems()
        {
            listViewBatch.Items.Clear();
            _batchItems.Clear();
        }

        private void RemoveSelectedBatchItems()
        {
            var indices = new List<int>();
            foreach (int i in listViewBatch.SelectedIndices)
            {
                indices.Add(i);
            }

            if (indices.Count == 0)
            {
                return;
            }

            indices = indices.OrderByDescending(p => p).ToList();
            var currentIndex = indices.Min();
            foreach (var i in indices)
            {
                listViewBatch.Items.RemoveAt(i);
                _batchItems.RemoveAt(i);
            }

            var newIdx = currentIndex;
            if (currentIndex >= _batchItems.Count - 1)
            {
                newIdx = _batchItems.Count - 1;
            }

            if (listViewBatch.Items.Count > 0)
            {
                listViewBatch.Items[newIdx].Selected = true;
                listViewBatch.Items[newIdx].Focused = true;
            }
        }

        private void addFilesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            buttonAddFile_Click(null, null);
        }

        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            RemoveSelectedBatchItems();
        }

        private void clearToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ClearBatchItems();
        }

        private void buttonPreview_Click(object sender, EventArgs e)
        {
            try
            {
                Cursor = Cursors.WaitCursor;

                var subtitle = new Subtitle(_assaSubtitle);
                _log = new StringBuilder();

                if (!_isAssa)
                {
                    SetStyleForNonAssa(subtitle);
                }

                FixRightToLeft(subtitle);

                var p = subtitle.Paragraphs.FirstOrDefault();
                subtitle.Paragraphs.Clear();
                if (p == null)
                {
                    p = new Paragraph("This is a sample text.", 0, 0);
                }
                subtitle.Paragraphs.Add(p);
                p.StartTime.TotalMilliseconds = 0;
                p.EndTime.TotalMilliseconds = 3_000;
                if (HtmlUtil.RemoveHtmlTags(p.Text, true).Length < 2)
                {
                    p.Text = "This is a sample text.";
                }

                var format = new AdvancedSubStationAlpha();
                var assaTempFileName = GetAssaFileName(subtitle.FileName);

                if (Configuration.Settings.General.CurrentVideoIsSmpte && comboBoxFrameRate.Text.Contains(".", StringComparison.Ordinal))
                {
                    foreach (var assaP in subtitle.Paragraphs)
                    {
                        assaP.StartTime.TotalMilliseconds *= 1.001;
                        assaP.EndTime.TotalMilliseconds *= 1.001;
                    }
                }

                try
                {
                    FileUtil.WriteAllText(assaTempFileName, format.ToText(subtitle, null), new TextEncoding(Encoding.UTF8, "UTF8"));
                }
                catch 
                {
                    // might be a write protected folder, so we try the temp folder
                    assaTempFileName = Path.Combine(Path.GetTempPath(), Path.GetFileName(assaTempFileName));
                    FileUtil.WriteAllText(assaTempFileName, format.ToText(subtitle, null), new TextEncoding(Encoding.UTF8, "UTF8"));
                }

                var videoFileName = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.mp4");
                var result = RunOnePassEncoding(assaTempFileName, subtitle, videoFileName);
                timer1.Stop();
                if (result)
                {
                    try
                    {
                        var bmpFileName = VideoPreviewGenerator.GetScreenShot(videoFileName, "00:00:01");
                        labelPleaseWait.Text = string.Empty;
                        labelProgress.Text = string.Empty;
                        using (var bmp = new Bitmap(bmpFileName))
                        {
                            using (var form = new ExportPngXmlPreview(bmp))
                            {
                                form.AllowNext = false;
                                form.AllowPrevious = false;
                                form.ShowDialog(this);
                            }
                        }

                        try
                        {
                            File.Delete(videoFileName);
                            File.Delete(bmpFileName);
                        }
                        catch
                        {
                            // ignore
                        }
                    }
                    catch
                    {
                        UiUtil.OpenFile(videoFileName);
                    }
                }
            }
            finally
            {
                Cursor = Cursors.Default;
                labelPleaseWait.Text = string.Empty;
                labelProgress.Text = string.Empty;
            }
        }

        private void buttonOutputFileSettings_Click(object sender, EventArgs e)
        {
            using (var form = new GenerateVideoWithHardSubsOutFile())
            {
                form.ShowDialog(this);

                ShowLabelOutput();
            }
        }

        private void ShowLabelOutput()
        {
            if (!BatchMode)
            {
                nikseLabelOutputFileFolder.Visible = false;
                linkLabelSourceFolder.Visible = false;
                return;
            }

            nikseLabelOutputFileFolder.Left = buttonOutputFileSettings.Right + 3;
            linkLabelSourceFolder.Left = buttonOutputFileSettings.Right + 3;

            if (Configuration.Settings.Tools.GenVideoUseOutputFolder)
            {
                linkLabelSourceFolder.Text = Configuration.Settings.Tools.GenVideoOutputFolder;
                linkLabelSourceFolder.Visible = true;
                nikseLabelOutputFileFolder.Visible = false;
            }
            else
            {
                nikseLabelOutputFileFolder.Text = LanguageSettings.Current.BatchConvert.SaveInSourceFolder;
                linkLabelSourceFolder.Visible = false;
                nikseLabelOutputFileFolder.Visible = true;
            }
        }

        private void linkLabelSourceFolder_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            var path = linkLabelSourceFolder.Text;

            if (!Directory.Exists(path))
            {
                try
                {
                    Directory.CreateDirectory(path);
                }
                catch
                {
                    return;
                }
            }

            UiUtil.OpenFolder(linkLabelSourceFolder.Text);
        }

        private void listViewBatch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {
                RemoveSelectedBatchItems();
                e.SuppressKeyPress = true;
            }
        }

        private void listViewBatch_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            if (_converting || listViewBatch.Items.Count == 0)
            {
                return;
            }

            for (var i = 0; i < listViewBatch.Columns.Count; i++)
            {
                ListViewSorter.SetSortArrow(listViewBatch.Columns[i], SortOrder.None);
            }

            var lv = (ListView)sender;
            if (!(lv.ListViewItemSorter is ListViewSorter sorter))
            {
                sorter = new ListViewSorter
                {
                    ColumnNumber = e.Column,
                    IsDisplayFileSize = e.Column == ListViewBatchSubItemIndexColumnSubtitleSize + 1,
                };
                lv.ListViewItemSorter = sorter;
            }

            if (e.Column == sorter.ColumnNumber)
            {
                sorter.Descending = !sorter.Descending; // inverse sort direction
            }
            else
            {
                sorter.ColumnNumber = e.Column;
                sorter.Descending = false;
                sorter.IsDisplayFileSize = e.Column == ListViewBatchSubItemIndexColumnSubtitleSize + 1;
            }

            lv.Sort();

            ListViewSorter.SetSortArrow(listViewBatch.Columns[e.Column], sorter.Descending ? SortOrder.Descending : SortOrder.Ascending);

            _batchItems.Clear();
            foreach (ListViewItem item in listViewBatch.Items)
            {
                _batchItems.Add((BatchItem)item.Tag);
            }
        }

        private void listViewBatch_DragDrop(object sender, DragEventArgs e)
        {
            if (_converting)
            {
                return;
            }

            var fileNames = (string[])e.Data.GetData(DataFormats.FileDrop);
            labelPleaseWait.Visible = true;

            TaskDelayHelper.RunDelayed(TimeSpan.FromMilliseconds(5), () =>
            {
                try
                {
                    Cursor = Cursors.WaitCursor;
                    foreach (var fileName in fileNames)
                    {
                        if (FileUtil.IsDirectory(fileName))
                        {
                            SearchFolder(fileName);
                        }
                        else
                        {
                            Application.DoEvents();
                            AddInputFile(fileName);
                        }
                    }
                }
                finally
                {
                    Cursor = Cursors.Default;
                    labelPleaseWait.Visible = false;
                }
            });
        }

        private void SearchFolder(string path)
        {
            _abort = false;
            foreach (var fileName in Directory.EnumerateFiles(path))
            {
                AddInputFile(fileName);
            }
        }

        private void listViewBatch_DragEnter(object sender, DragEventArgs e)
        {
            if (_converting)
            {
                e.Effect = DragDropEffects.None;
                return;
            }

            if (e.Data.GetDataPresent(DataFormats.FileDrop, false))
            {
                e.Effect = DragDropEffects.All;
            }
        }

        private void buttonCutFrom_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(_inputVideoFileName))
            {
                OpenCutFrom(_inputVideoFileName);
                return;
            }

            using (var openFileDialog1 = new OpenFileDialog())
            {
                openFileDialog1.Title = LanguageSettings.Current.General.OpenVideoFileTitle;
                openFileDialog1.FileName = string.Empty;
                openFileDialog1.Filter = UiUtil.GetVideoFileFilter(true);

                openFileDialog1.FileName = string.Empty;
                if (openFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    OpenCutFrom(openFileDialog1.FileName);
                }
            }
        }

        private void OpenCutFrom(string fileName)
        {
            var timeSpan = new TimeSpan((int)numericUpDownCutFromHours.Value, (int)numericUpDownCutFromMinutes.Value, (int)numericUpDownCutFromSeconds.Value);
            var videoInfo = UiUtil.GetVideoInfo(fileName);
            using (var form = new GetVideoPosition(_assaSubtitle, fileName, videoInfo, timeSpan, LanguageSettings.Current.GenerateVideoWithBurnedInSubs.GetStartPosition))
            {
                if (form.ShowDialog(this) == DialogResult.OK)
                {
                    numericUpDownCutFromHours.Value = form.VideoPosition.Hours;
                    numericUpDownCutFromMinutes.Value = form.VideoPosition.Minutes;
                    numericUpDownCutFromSeconds.Value = form.VideoPosition.Seconds;
                }
            }
        }

        private void buttonCutTo_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(_inputVideoFileName))
            {
                OpenCutTo(_inputVideoFileName);
                return;
            }

            using (var openFileDialog1 = new OpenFileDialog())
            {
                openFileDialog1.Title = LanguageSettings.Current.General.OpenVideoFileTitle;
                openFileDialog1.FileName = string.Empty;
                openFileDialog1.Filter = UiUtil.GetVideoFileFilter(true);

                openFileDialog1.FileName = string.Empty;
                if (openFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    OpenCutTo(openFileDialog1.FileName);
                }
            }
        }

        private void OpenCutTo(string fileName)
        {
            var videoInfo = UiUtil.GetVideoInfo(fileName);
            var timeSpan = new TimeSpan((int)numericUpDownCutFromHours.Value, (int)numericUpDownCutToMinutes.Value, (int)numericUpDownCutFromSeconds.Value);
            using (var form = new GetVideoPosition(_assaSubtitle, fileName, videoInfo, timeSpan, LanguageSettings.Current.GenerateVideoWithBurnedInSubs.GetEndPosition))
            {
                if (form.ShowDialog(this) == DialogResult.OK)
                {
                    numericUpDownCutToHours.Value = form.VideoPosition.Hours;
                    numericUpDownCutToMinutes.Value = form.VideoPosition.Minutes;
                    numericUpDownCutToSeconds.Value = form.VideoPosition.Seconds;
                }
            }
        }

        private void checkBoxBox_CheckedChanged(object sender, EventArgs e)
        {
            comboBoxOpaqueBoxStyle.Enabled = checkBoxBox.Checked;
            if (checkBoxBox.Checked)
            {
                comboBoxOpaqueBoxStyle_SelectedIndexChanged(null, null);
            }
            else
            {
                buttonOutlineColor.Text = LanguageSettings.Current.SubStationAlphaStyles.Outline;
                buttonColorShadow.Text = LanguageSettings.Current.SubStationAlphaStyles.Shadow;
            }
        }

        private void comboBoxOpaqueBoxStyle_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBoxOpaqueBoxStyle.SelectedIndex == 0)
            {
                buttonColorShadow.Text = LanguageSettings.Current.Main.Menu.ContextMenu.Box;
                buttonOutlineColor.Text = LanguageSettings.Current.SubStationAlphaStyles.Shadow;
            }
            else
            {
                buttonOutlineColor.Text = LanguageSettings.Current.SubStationAlphaStyles.Outline;
                buttonColorShadow.Text = LanguageSettings.Current.Main.Menu.ContextMenu.Box;
            }
        }

        private void buttonColorShadow_Click(object sender, EventArgs e)
        {
            using (var colorChooser = new ColorChooser { Color = panelShadowColor.BackColor, ShowAlpha = true })
            {
                if (colorChooser.ShowDialog() == DialogResult.OK)
                {
                    panelShadowColor.BackColor = colorChooser.Color;
                }
            }
        }

        private void panelShadowColor_Click(object sender, EventArgs e)
        {
            buttonColorShadow_Click(null, null);
        }

        private void useSourceResolutionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _useVideoSourceResolution = true;
            numericUpDownWidth.Visible = false;
            numericUpDownHeight.Visible = false;

            labelX.Left = numericUpDownWidth.Left;
            if (BatchMode)
            {
                labelX.Text = LanguageSettings.Current.GenerateVideoWithBurnedInSubs.UseSource;

                numericUpDownWidth.Value = 0;
                numericUpDownHeight.Value = 0;
            }
            else
            {
                labelX.Text = $"{LanguageSettings.Current.GenerateVideoWithBurnedInSubs.UseSource} ({_videoInfo.Width}x{_videoInfo.Height})";

                numericUpDownWidth.Value = _videoInfo.Width;
                numericUpDownHeight.Value = _videoInfo.Height;
            }
        }

        private void contextMenuStripRes_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            useSourceResolutionToolStripMenuItem.Visible =
                BatchMode || (_videoInfo != null && !string.IsNullOrEmpty(_inputVideoFileName));

        }

        private void pickVideoFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (var openFileDialog1 = new OpenFileDialog())
            {
                openFileDialog1.Title = LanguageSettings.Current.General.OpenVideoFile;
                openFileDialog1.FileName = string.Empty;
                openFileDialog1.Filter = UiUtil.GetVideoFileFilter(false);
                openFileDialog1.Multiselect = false;
                if (openFileDialog1.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }

                var idx = listViewBatch.SelectedIndices[0];
                var batchItem = _batchItems[idx];
                batchItem.SubtitleFileName = openFileDialog1.FileName;
                listViewBatch.Items[idx].SubItems[ListViewBatchSubItemIndexColumnVideoFileName].Text = Path.GetFileName(openFileDialog1.FileName);

                var mediaInfo = FfmpegMediaInfo.Parse(openFileDialog1.FileName);
                if (mediaInfo.Dimension.IsValid())
                {
                    batchItem.Width = mediaInfo.Dimension.Width;
                    batchItem.Height = mediaInfo.Dimension.Height;
                    listViewBatch.Items[idx].SubItems[ListViewBatchSubItemIndexColumnResolution].Text = $"{batchItem.Width}x{batchItem.Height}";
                }
            }
        }

        private void buttonCutFromText_Click(object sender, EventArgs e)
        {
            if (!BatchMode && _assaSubtitle.Paragraphs.Count > 0)
            {
                CutFromText(_assaSubtitle);
                return;
            }

            using (var openFileDialog1 = new OpenFileDialog())
            {
                openFileDialog1.Title = LanguageSettings.Current.General.OpenSubtitle;
                openFileDialog1.FileName = string.Empty;
                openFileDialog1.Filter = UiUtil.SubtitleExtensionFilter.Value;

                openFileDialog1.FileName = string.Empty;
                if (openFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    var subtitle = Subtitle.Parse(openFileDialog1.FileName);
                    CutFromText(subtitle);
                }
            }
        }

        private void CutFromText(Subtitle subtitle)
        {
            using (var findSubtitle = new FindSubtitleLine())
            {
                var title = $" \"{labelCutFrom.Text}\"";
                findSubtitle.Initialize(subtitle.Paragraphs, title);
                findSubtitle.ShowDialog();
                if (findSubtitle.SelectedIndex >= 0)
                {
                    var p = subtitle.GetParagraphOrDefault(findSubtitle.SelectedIndex);
                    if (p != null)
                    {
                        numericUpDownCutFromHours.Value = p.StartTime.Hours;
                        numericUpDownCutFromMinutes.Value = p.StartTime.Minutes;
                        numericUpDownCutFromSeconds.Value = p.StartTime.Seconds;
                    }
                }
            }
        }

        private void CutToText(Subtitle subtitle)
        {
            using (var findSubtitle = new FindSubtitleLine())
            {
                var title = $" \"{labelCutTo.Text}\"";
                findSubtitle.Initialize(subtitle.Paragraphs, title);
                findSubtitle.ShowDialog();
                if (findSubtitle.SelectedIndex >= 0)
                {
                    var p = subtitle.GetParagraphOrDefault(findSubtitle.SelectedIndex);
                    if (p != null)
                    {
                        var endP = new Paragraph(p);
                        endP.EndTime.TotalMilliseconds += 1000;
                        numericUpDownCutToHours.Value = endP.EndTime.Hours;
                        numericUpDownCutToMinutes.Value = endP.EndTime.Minutes;
                        numericUpDownCutToSeconds.Value = endP.EndTime.Seconds;
                    }
                }
            }
        }

        private void buttonCutToText_Click(object sender, EventArgs e)
        {
            if (!BatchMode && _assaSubtitle.Paragraphs.Count > 0)
            {
                CutToText(_assaSubtitle);
                return;
            }

            using (var openFileDialog1 = new OpenFileDialog())
            {
                openFileDialog1.Title = LanguageSettings.Current.General.OpenSubtitle;
                openFileDialog1.FileName = string.Empty;
                openFileDialog1.Filter = UiUtil.SubtitleExtensionFilter.Value;

                openFileDialog1.FileName = string.Empty;
                if (openFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    var subtitle = Subtitle.Parse(openFileDialog1.FileName);
                    CutToText(subtitle);
                }
            }
        }

        private void removeVideoFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (int i in listViewBatch.SelectedIndices)
            {
                listViewBatch.Items[i].SubItems[ListViewBatchSubItemIndexColumnVideoFileName].Text = string.Empty;
                listViewBatch.Items[i].SubItems[ListViewBatchSubItemIndexColumnResolution].Text = string.Empty;
                _batchItems[i].VideoFileName = string.Empty;
                _batchItems[i].Width = 0;
                _batchItems[i].Height = 0;
            }
        }

        private void GenerateTransparentVideoWithSubtitles_Shown(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(_inputVideoFileName))
            {
                var mediaInfo = FfmpegMediaInfo.Parse(_inputVideoFileName);
                if (mediaInfo.Dimension.IsValid())
                {
                    numericUpDownWidth.Value = mediaInfo.Dimension.Width;
                    numericUpDownHeight.Value = mediaInfo.Dimension.Height;
                }
            }
        }
    }
}

