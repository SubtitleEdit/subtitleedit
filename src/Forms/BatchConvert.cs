﻿using Nikse.SubtitleEdit.Core;
using Nikse.SubtitleEdit.Core.BluRaySup;
using Nikse.SubtitleEdit.Core.ContainerFormats.Matroska;
using Nikse.SubtitleEdit.Core.Forms;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Forms.Styles;
using Nikse.SubtitleEdit.Logic;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Nikse.SubtitleEdit.Core.VobSub;
using Nikse.SubtitleEdit.Forms.Ocr;
using Idx = Nikse.SubtitleEdit.Core.VobSub.Idx;
using Nikse.SubtitleEdit.Core.Interfaces;

namespace Nikse.SubtitleEdit.Forms
{
    public sealed partial class BatchConvert : PositionAndSizeForm
    {

        public class ThreadDoWorkParameter
        {
            public bool FixCommonErrors { get; set; }
            public bool MultipleReplaceActive { get; set; }
            public bool FixRtl { get; set; }
            public bool SplitLongLinesActive { get; set; }
            public bool AutoBalanceActive { get; set; }
            public bool SetMinDisplayTimeBetweenSubtitles { get; set; }
            public ListViewItem Item { get; set; }
            public Subtitle Subtitle { get; set; }
            public SubtitleFormat Format { get; set; }
            public Encoding Encoding { get; set; }
            public string Language { get; set; }
            public string Error { get; set; }
            public string FileName { get; set; }
            public string ToFormat { get; set; }
            public SubtitleFormat SourceFormat { get; set; }
            public ThreadDoWorkParameter(bool fixCommonErrors, bool multipleReplace, bool fixRtl, bool splitLongLinesActive, bool autoBalance, bool setMinDisplayTimeBetweenSubtitles, ListViewItem item, Subtitle subtitle, SubtitleFormat format, Encoding encoding, string language, string fileName, string toFormat, SubtitleFormat sourceFormat)
            {
                FixCommonErrors = fixCommonErrors;
                MultipleReplaceActive = multipleReplace;
                FixRtl = fixRtl;
                SplitLongLinesActive = splitLongLinesActive;
                AutoBalanceActive = autoBalance;
                SetMinDisplayTimeBetweenSubtitles = setMinDisplayTimeBetweenSubtitles;
                Item = item;
                Subtitle = subtitle;
                Format = format;
                Encoding = encoding;
                Language = language;
                FileName = fileName;
                ToFormat = toFormat;
                SourceFormat = sourceFormat;
            }
        }

        private string _assStyle;
        private string _ssaStyle;
        private readonly RemoveTextForHI _removeTextForHearingImpaired;
        private readonly ChangeCasing _changeCasing = new ChangeCasing();
        private readonly ChangeCasingNames _changeCasingNames = new ChangeCasingNames();
        private bool _converting;
        private int _count;
        private int _converted;
        private int _errors;
        private readonly IList<SubtitleFormat> _allFormats;
        private bool _searching;
        private bool _abort;
        private Ebu.EbuGeneralSubtitleInformation _ebuGeneralInformation;
        public const string BluRaySubtitle = "Blu-ray sup";
        public const string VobSubSubtitle = "VobSub";
        public const string DostImageSubtitle = "Dost-image";
        public const string BdnXmlSubtitle = "BDN-XML";
        public const string FcpImageSubtitle = "FCP-image";
        private string _customTextTemplate;
        private readonly DurationsBridgeGaps _bridgeGaps;
        private const int ConvertMaxFileSize = 1024 * 1024 * 10; // 10 MB

        public BatchConvert(Icon icon)
        {
            UiUtil.PreInitialize(this);
            InitializeComponent();
            UiUtil.FixFonts(this);
            Icon = (Icon)icon.Clone();

            progressBar1.Visible = false;
            labelStatus.Text = string.Empty;
            var l = Configuration.Settings.Language.BatchConvert;
            Text = l.Title;
            groupBoxInput.Text = l.Input;
            labelChooseInputFiles.Text = l.InputDescription;
            groupBoxOutput.Text = l.Output;
            labelChooseOutputFolder.Text = l.ChooseOutputFolder;
            checkBoxOverwrite.Text = l.OverwriteExistingFiles;
            labelOutputFormat.Text = Configuration.Settings.Language.Main.Controls.SubtitleFormat;
            labelEncoding.Text = Configuration.Settings.Language.Main.Controls.FileEncoding;
            buttonStyles.Text = l.Style;
            checkBoxUseStyleFromSource.Text = l.UseStyleFromSource;
            groupBoxConvertOptions.Text = l.ConvertOptions;
            checkBoxRemoveFormatting.Text = l.RemoveFormatting;
            checkBoxFixCasing.Text = l.RedoCasing;
            checkBoxRemoveTextForHI.Text = l.RemoveTextForHI;
            checkBoxOverwriteOriginalFiles.Text = l.OverwriteOriginalFiles;
            columnHeaderFName.Text = Configuration.Settings.Language.JoinSubtitles.FileName;
            columnHeaderFormat.Text = Configuration.Settings.Language.Main.Controls.SubtitleFormat;
            columnHeaderSize.Text = Configuration.Settings.Language.General.Size;
            columnHeaderStatus.Text = l.Status;
            linkLabelOpenOutputFolder.Text = Configuration.Settings.Language.Main.Menu.File.Open;
            buttonSearchFolder.Text = l.ScanFolder;
            buttonConvert.Text = l.Convert;
            buttonCancel.Text = Configuration.Settings.Language.General.Ok;
            checkBoxScanFolderRecursive.Text = l.Recursive;
            checkBoxScanFolderRecursive.Left = buttonSearchFolder.Left - checkBoxScanFolderRecursive.Width - 5;

            groupBoxChangeFrameRate.Text = Configuration.Settings.Language.ChangeFrameRate.Title;
            groupBoxOffsetTimeCodes.Text = l.OffsetTimeCodes;
            groupBoxSpeed.Text = Configuration.Settings.Language.ChangeSpeedInPercent.TitleShort;
            labelFromFrameRate.Text = Configuration.Settings.Language.ChangeFrameRate.FromFrameRate;
            labelToFrameRate.Text = Configuration.Settings.Language.ChangeFrameRate.ToFrameRate;
            labelHourMinSecMilliSecond.Text = Configuration.Settings.General.UseTimeFormatHHMMSSFF ? Configuration.Settings.Language.General.HourMinutesSecondsFrames : Configuration.Settings.Language.General.HourMinutesSecondsMilliseconds;

            comboBoxFrameRateFrom.Left = labelFromFrameRate.Left + labelFromFrameRate.Width + 3;
            comboBoxFrameRateTo.Left = labelToFrameRate.Left + labelToFrameRate.Width + 3;
            if (comboBoxFrameRateFrom.Left > comboBoxFrameRateTo.Left)
            {
                comboBoxFrameRateTo.Left = comboBoxFrameRateFrom.Left;
            }
            else
            {
                comboBoxFrameRateFrom.Left = comboBoxFrameRateTo.Left;
            }

            comboBoxSubtitleFormats.Left = labelOutputFormat.Left + labelOutputFormat.Width + 3;
            comboBoxEncoding.Left = labelEncoding.Left + labelEncoding.Width + 3;
            if (comboBoxSubtitleFormats.Left > comboBoxEncoding.Left)
            {
                comboBoxEncoding.Left = comboBoxSubtitleFormats.Left;
            }
            else
            {
                comboBoxSubtitleFormats.Left = comboBoxEncoding.Left;
            }
            buttonStyles.Left = comboBoxSubtitleFormats.Left + comboBoxSubtitleFormats.Width + 5;

            timeUpDownAdjust.MaskedTextBox.Text = "000000000";

            comboBoxFrameRateFrom.Items.Add(23.976);
            comboBoxFrameRateFrom.Items.Add(24.0);
            comboBoxFrameRateFrom.Items.Add(25.0);
            comboBoxFrameRateFrom.Items.Add(29.97);
            comboBoxFrameRateFrom.Items.Add(30.0);

            comboBoxFrameRateTo.Items.Add(23.976);
            comboBoxFrameRateTo.Items.Add(24.0);
            comboBoxFrameRateTo.Items.Add(25.0);
            comboBoxFrameRateTo.Items.Add(29.97);
            comboBoxFrameRateTo.Items.Add(30.0);

            UiUtil.FixLargeFonts(this, buttonCancel);

            _allFormats = new List<SubtitleFormat> { new Pac() };
            var formatNames = new List<string>();
            foreach (var format in SubtitleFormat.AllSubtitleFormats)
            {
                if (!format.IsVobSubIndexFile)
                {
                    formatNames.Add(format.Name);
                    _allFormats.Add(format);
                }
            }
            formatNames.Add("PAC");
            formatNames.Add(new Ayato().Name);
            formatNames.Add(l.PlainText);
            formatNames.Add(BluRaySubtitle);
            formatNames.Add(VobSubSubtitle);
            formatNames.Add(DostImageSubtitle);
            formatNames.Add(BdnXmlSubtitle);
            formatNames.Add(FcpImageSubtitle);
            formatNames.Add(Configuration.Settings.Language.ExportCustomText.Title);
            UiUtil.InitializeSubtitleFormatComboBox(comboBoxSubtitleFormats, formatNames, Configuration.Settings.Tools.BatchConvertFormat);

            UiUtil.InitializeTextEncodingComboBox(comboBoxEncoding);

            if (string.IsNullOrEmpty(Configuration.Settings.Tools.BatchConvertOutputFolder) || !Directory.Exists(Configuration.Settings.Tools.BatchConvertOutputFolder))
            {
                textBoxOutputFolder.Text = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            }
            else
            {
                textBoxOutputFolder.Text = Configuration.Settings.Tools.BatchConvertOutputFolder;
            }

            checkBoxOverwrite.Checked = Configuration.Settings.Tools.BatchConvertOverwriteExisting;
            checkBoxOverwriteOriginalFiles.Checked = Configuration.Settings.Tools.BatchConvertOverwriteOriginal;
            checkBoxFixCasing.Checked = Configuration.Settings.Tools.BatchConvertFixCasing;
            checkBoxFixCommonErrors.Checked = Configuration.Settings.Tools.BatchConvertFixCommonErrors;
            checkBoxMultipleReplace.Checked = Configuration.Settings.Tools.BatchConvertMultipleReplace;
            checkBoxFixRtl.Checked = Configuration.Settings.Tools.BatchConvertFixRtl;
            checkBoxSplitLongLines.Checked = Configuration.Settings.Tools.BatchConvertSplitLongLines;
            checkBoxAutoBalance.Checked = Configuration.Settings.Tools.BatchConvertAutoBalance;
            checkBoxRemoveFormatting.Checked = Configuration.Settings.Tools.BatchConvertRemoveFormatting;
            checkBoxRemoveTextForHI.Checked = Configuration.Settings.Tools.BatchConvertRemoveTextForHI;
            checkBoxBridgeGaps.Checked = Configuration.Settings.Tools.BatchConvertBridgeGaps;
            checkBoxSetMinimumDisplayTimeBetweenSubs.Checked = Configuration.Settings.Tools.BatchConvertSetMinDisplayTimeBetweenSubtitles;
            buttonRemoveTextForHiSettings.Text = l.Settings;
            buttonBridgeGapsSettings.Text = l.Settings;
            buttonFixCommonErrorSettings.Text = l.Settings;
            buttonMultipleReplaceSettings.Text = l.Settings;
            buttonFixRtlSettings.Text = l.Settings;
            checkBoxFixCommonErrors.Text = Configuration.Settings.Language.FixCommonErrors.Title;
            checkBoxMultipleReplace.Text = Configuration.Settings.Language.MultipleReplace.Title;
            checkBoxAutoBalance.Text = l.AutoBalance;
            checkBoxFixRtl.Text = l.FixRtl;
            checkBoxSplitLongLines.Text = l.SplitLongLines;
            radioButtonShowEarlier.Text = Configuration.Settings.Language.ShowEarlierLater.ShowEarlier;
            radioButtonShowLater.Text = Configuration.Settings.Language.ShowEarlierLater.ShowLater;
            radioButtonSpeedCustom.Text = Configuration.Settings.Language.ChangeSpeedInPercent.Custom;
            radioButtonSpeedFromDropFrame.Text = Configuration.Settings.Language.ChangeSpeedInPercent.FromDropFrame;
            radioButtonToDropFrame.Text = Configuration.Settings.Language.ChangeSpeedInPercent.ToDropFrame;
            checkBoxSetMinimumDisplayTimeBetweenSubs.Text = l.SetMinMsBetweenSubtitles;
            checkBoxBridgeGaps.Text = l.BridgeGaps;

            _removeTextForHearingImpaired = new RemoveTextForHI(new RemoveTextForHISettings(new Subtitle()));
            _removeTextForHiSettings = _removeTextForHearingImpaired.Settings;

            labelFilter.Text = l.Filter;
            comboBoxFilter.Items[0] = Configuration.Settings.Language.General.AllFiles;
            comboBoxFilter.Items[1] = l.FilterSrtNoUtf8BOM;
            comboBoxFilter.Items[2] = l.FilterMoreThanTwoLines;
            comboBoxFilter.Items[3] = l.FilterContains;
            comboBoxFilter.SelectedIndex = 0;
            comboBoxFilter.Left = labelFilter.Left + labelFilter.Width + 4;
            textBoxFilter.Left = comboBoxFilter.Left + comboBoxFilter.Width + 4;

            _assStyle = Configuration.Settings.Tools.BatchConvertAssStyles;
            _ssaStyle = Configuration.Settings.Tools.BatchConvertSsaStyles;
            checkBoxUseStyleFromSource.Checked = Configuration.Settings.Tools.BatchConvertUseStyleFromSource;
            _customTextTemplate = Configuration.Settings.Tools.BatchConvertExportCustomTextTemplate;

            comboBoxSubtitleFormats.AutoCompleteSource = AutoCompleteSource.ListItems;
            comboBoxSubtitleFormats.AutoCompleteMode = AutoCompleteMode.Append;

            _bridgeGaps = new DurationsBridgeGaps(null);
            _bridgeGaps.InitializeSettingsOnly();
        }

        private void buttonChooseFolder_Click(object sender, EventArgs e)
        {
            folderBrowserDialog1.ShowNewFolderButton = true;
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                textBoxOutputFolder.Text = folderBrowserDialog1.SelectedPath;
            }
        }

        private void buttonInputBrowse_Click(object sender, EventArgs e)
        {
            buttonInputBrowse.Enabled = false;
            openFileDialog1.Title = Configuration.Settings.Language.General.OpenSubtitle;
            openFileDialog1.FileName = string.Empty;
            openFileDialog1.Filter = UiUtil.SubtitleExtensionFilter.Value;
            openFileDialog1.Multiselect = true;
            if (openFileDialog1.ShowDialog(this) == DialogResult.OK)
            {
                try
                {
                    Cursor = Cursors.WaitCursor;
                    foreach (string fileName in openFileDialog1.FileNames)
                    {
                        AddInputFile(fileName);
                    }
                }
                finally
                {
                    Cursor = Cursors.Default;
                }
            }

            buttonInputBrowse.Enabled = true;
        }

        private void AddInputFile(string fileName)
        {
            try
            {
                foreach (ListViewItem lvi in listViewInputFiles.Items)
                {
                    if (lvi.Text.Equals(fileName, StringComparison.OrdinalIgnoreCase))
                    {
                        return;
                    }
                }

                var fi = new FileInfo(fileName);
                var ext = fi.Extension;
                var item = new ListViewItem(fileName);
                item.SubItems.Add(Utilities.FormatBytesToDisplayFileSize(fi.Length));
                var isMkv = false;
                var mkvPgs = new List<string>();
                var mkvVobSub = new List<string>();
                var mkvSrt = new List<string>();
                var mkvSsa = new List<string>();
                var mkvAss = new List<string>();
                int mkvCount = 0;

                SubtitleFormat format = null;
                var sub = new Subtitle();
                if (fi.Length < ConvertMaxFileSize)
                {
                    if (!FileUtil.IsBluRaySup(fileName) && !FileUtil.IsVobSub(fileName))
                    {
                        format = sub.LoadSubtitle(fileName, out _, null);

                        if (format == null)
                        {
                            foreach (var f in SubtitleFormat.GetBinaryFormats(true))
                            {
                                if (f.IsMine(null, fileName))
                                {
                                    f.LoadSubtitle(sub, null, fileName);
                                    format = f;
                                    break;
                                }
                            }
                        }

                        if (format == null)
                        {
                            var encoding = LanguageAutoDetect.GetEncodingFromFile(fileName);
                            var lines = FileUtil.ReadAllTextShared(fileName, encoding).SplitToLines();
                            foreach (var f in SubtitleFormat.GetTextOtherFormats())
                            {
                                if (f.IsMine(lines, fileName))
                                {
                                    f.LoadSubtitle(sub, lines, fileName);
                                    format = f;
                                    break;
                                }
                            }
                        }
                    }
                }

                if (format == null)
                {
                    if (FileUtil.IsBluRaySup(fileName))
                    {
                        item.SubItems.Add("Blu-ray");
                    }
                    else if (FileUtil.IsVobSub(fileName))
                    {
                        item.SubItems.Add("VobSub");
                    }
                    else if (ext.Equals(".mkv", StringComparison.OrdinalIgnoreCase) || ext.Equals(".mks", StringComparison.OrdinalIgnoreCase))
                    {
                        isMkv = true;
                        using (var matroska = new MatroskaFile(fileName))
                        {
                            if (matroska.IsValid)
                            {
                                foreach (var track in matroska.GetTracks(true))
                                {
                                    if (track.CodecId.Equals("S_VOBSUB", StringComparison.OrdinalIgnoreCase))
                                    {
                                        mkvVobSub.Add((track.Language ?? "undefined") + " #" + track.TrackNumber);
                                    }
                                    else if (track.CodecId.Equals("S_HDMV/PGS", StringComparison.OrdinalIgnoreCase))
                                    {
                                        mkvPgs.Add((track.Language ?? "undefined") + " #" + track.TrackNumber);
                                    }
                                    else if (track.CodecId.Equals("S_TEXT/UTF8", StringComparison.OrdinalIgnoreCase))
                                    {
                                        mkvSrt.Add((track.Language ?? "undefined") + " #" + track.TrackNumber);
                                    }
                                    else if (track.CodecId.Equals("S_TEXT/SSA", StringComparison.OrdinalIgnoreCase))
                                    {
                                        mkvSsa.Add((track.Language ?? "undefined") + " #" + track.TrackNumber);
                                    }
                                    else if (track.CodecId.Equals("S_TEXT/ASS", StringComparison.OrdinalIgnoreCase))
                                    {
                                        mkvAss.Add((track.Language ?? "undefined") + " #" + track.TrackNumber);
                                    }
                                }
                            }
                        }
                        if (mkvVobSub.Count + mkvPgs.Count + mkvSrt.Count + mkvSsa.Count + mkvAss.Count <= 0)
                        {
                            item.SubItems.Add(Configuration.Settings.Language.UnknownSubtitle.Title);
                        }
                    }
                    else
                    {
                        item.SubItems.Add(Configuration.Settings.Language.UnknownSubtitle.Title);
                    }
                }
                else
                {
                    item.SubItems.Add(format.Name);
                }
                item.SubItems.Add("-");

                if (isMkv)
                {
                    if (mkvCount > 0)
                    {
                        listViewInputFiles.Items.Add(item);
                    }

                    foreach (var lang in mkvPgs)
                    {
                        item = new ListViewItem(fileName);
                        item.SubItems.Add(Utilities.FormatBytesToDisplayFileSize(fi.Length));
                        listViewInputFiles.Items.Add(item);
                        item.SubItems.Add("Matroska/PGS - " + lang);
                        item.SubItems.Add("-");
                    }
                    foreach (var lang in mkvVobSub)
                    {
                        item = new ListViewItem(fileName);
                        item.SubItems.Add(Utilities.FormatBytesToDisplayFileSize(fi.Length));
                        listViewInputFiles.Items.Add(item);
                        item.SubItems.Add("Matroska/VobSub - " + lang);
                        item.SubItems.Add("-");
                    }
                    foreach (var lang in mkvSrt)
                    {
                        item = new ListViewItem(fileName);
                        item.SubItems.Add(Utilities.FormatBytesToDisplayFileSize(fi.Length));
                        listViewInputFiles.Items.Add(item);
                        item.SubItems.Add("Matroska/SRT - " + lang);
                        item.SubItems.Add("-");
                    }
                    foreach (var lang in mkvSsa)
                    {
                        item = new ListViewItem(fileName);
                        item.SubItems.Add(Utilities.FormatBytesToDisplayFileSize(fi.Length));
                        listViewInputFiles.Items.Add(item);
                        item.SubItems.Add("Matroska/SSA - " + lang);
                        item.SubItems.Add("-");
                    }
                    foreach (var lang in mkvAss)
                    {
                        item = new ListViewItem(fileName);
                        item.SubItems.Add(Utilities.FormatBytesToDisplayFileSize(fi.Length));
                        listViewInputFiles.Items.Add(item);
                        item.SubItems.Add("Matroska/ASS - " + lang);
                        item.SubItems.Add("-");
                    }
                }
                else
                {
                    listViewInputFiles.Items.Add(item);
                }
            }
            catch
            {
                // ignored
            }
        }

        private void listViewInputFiles_DragEnter(object sender, DragEventArgs e)
        {
            if (_converting || _searching)
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
            if (_converting || _searching)
            {
                return;
            }

            var fileNames = (string[])e.Data.GetData(DataFormats.FileDrop);
            foreach (string fileName in fileNames)
            {
                if (FileUtil.IsDirectory(fileName))
                {
                    SearchFolder(fileName);
                }
                else
                {
                    AddInputFile(fileName);
                }
            }
        }

        private void radioButtonSpeedCustom_CheckedChanged(object sender, EventArgs e)
        {
            numericUpDownPercent.Enabled = true;
        }

        private void radioButtonSpeedFromDropFrame_CheckedChanged(object sender, EventArgs e)
        {
            numericUpDownPercent.Value = Convert.ToDecimal(099.98887);
            numericUpDownPercent.Enabled = false;
        }

        private void radioButtonToDropFrame_CheckedChanged(object sender, EventArgs e)
        {
            numericUpDownPercent.Value = Convert.ToDecimal(100.1001001);
            numericUpDownPercent.Enabled = false;
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private Encoding GetCurrentEncoding()
        {
            return UiUtil.GetTextEncodingComboBoxCurrentEncoding(comboBoxEncoding);
        }

        private SubtitleFormat GetCurrentSubtitleFormat()
        {
            var format = Utilities.GetSubtitleFormatByFriendlyName(comboBoxSubtitleFormats.SelectedItem.ToString());
            return format ?? new SubRip();
        }

        private void buttonConvert_Click(object sender, EventArgs e)
        {
            if (listViewInputFiles.Items.Count == 0)
            {
                MessageBox.Show(Configuration.Settings.Language.BatchConvert.NothingToConvert);
                return;
            }
            if (!checkBoxOverwriteOriginalFiles.Checked)
            {
                if (textBoxOutputFolder.Text.Length < 2)
                {
                    MessageBox.Show(Configuration.Settings.Language.BatchConvert.PleaseChooseOutputFolder);
                    return;
                }
                if (!Directory.Exists(textBoxOutputFolder.Text))
                {
                    try
                    {
                        Directory.CreateDirectory(textBoxOutputFolder.Text);
                    }
                    catch (Exception exception)
                    {
                        MessageBox.Show(exception.Message);
                        return;
                    }
                }
            }

            string toFormat = comboBoxSubtitleFormats.Text;
            ChangeControlStates(true);
            _count = 0;
            _converted = 0;
            _errors = 0;
            _abort = false;

            // create workers
            BackgroundWorker worker1 = CreateWorker();
            BackgroundWorker worker2 = CreateWorker();
            BackgroundWorker worker3 = CreateWorker();

            listViewInputFiles.BeginUpdate();
            foreach (ListViewItem item in listViewInputFiles.Items)
            {
                item.SubItems[3].Text = "-";
            }
            listViewInputFiles.EndUpdate();

            var mkvFileNames = new List<string>();
            Refresh();
            int index = 0;
            while (index < listViewInputFiles.Items.Count && !_abort)
            {
                ListViewItem item = listViewInputFiles.Items[index];
                string fileName = item.Text;
                try
                {
                    SubtitleFormat format = null;
                    var sub = new Subtitle();
                    var fi = new FileInfo(fileName);
                    if (fi.Length < ConvertMaxFileSize && !FileUtil.IsBluRaySup(fileName) && !FileUtil.IsVobSub(fileName))
                    {
                        format = sub.LoadSubtitle(fileName, out _, null);

                        if (format == null)
                        {
                            foreach (var f in SubtitleFormat.GetBinaryFormats(true))
                            {
                                if (f.IsMine(null, fileName))
                                {
                                    f.LoadSubtitle(sub, null, fileName);
                                    format = f;
                                    break;
                                }
                            }
                        }

                        if (format == null)
                        {
                            var encoding = LanguageAutoDetect.GetEncodingFromFile(fileName);
                            var lines = FileUtil.ReadAllTextShared(fileName, encoding).SplitToLines();
                            foreach (var f in SubtitleFormat.GetTextOtherFormats())
                            {
                                if (f.IsMine(lines, fileName))
                                {
                                    f.LoadSubtitle(sub, lines, fileName);
                                    format = f;
                                    break;
                                }
                            }
                        }

                        if (format == null)
                        {
                            var enc = LanguageAutoDetect.GetEncodingFromFile(fileName);
                            var s = File.ReadAllText(fileName, enc);

                            // check for RTF file
                            if (fileName.EndsWith(".rtf", StringComparison.OrdinalIgnoreCase) && s.TrimStart().StartsWith("{\\rtf", StringComparison.Ordinal))
                            {
                                using (var rtb = new RichTextBox { Rtf = s })
                                {
                                    s = rtb.Text;
                                }
                            }
                            var uknownFormatImporter = new UknownFormatImporter { UseFrames = true };
                            var genericParseSubtitle = uknownFormatImporter.AutoGuessImport(s.SplitToLines());
                            if (genericParseSubtitle.Paragraphs.Count > 1)
                            {
                                sub = genericParseSubtitle;
                                format = new SubRip();
                            }
                        }

                        if (format != null && format.GetType() == typeof(MicroDvd))
                        {
                            if (sub != null && sub.Paragraphs.Count > 0 && sub.Paragraphs[0].Duration.TotalMilliseconds < 1001)
                            {
                                if (sub.Paragraphs[0].Text.StartsWith("29.", StringComparison.Ordinal) || sub.Paragraphs[0].Text.StartsWith("23.", StringComparison.Ordinal) ||
                                sub.Paragraphs[0].Text.StartsWith("29,", StringComparison.Ordinal) || sub.Paragraphs[0].Text.StartsWith("23,", StringComparison.Ordinal) ||
                                sub.Paragraphs[0].Text == "24" || sub.Paragraphs[0].Text == "25" ||
                                sub.Paragraphs[0].Text == "30" || sub.Paragraphs[0].Text == "60")
                                {
                                    sub.Paragraphs.RemoveAt(0);
                                }
                            }
                        }
                    }
                    var bluRaySubtitles = new List<BluRaySupParser.PcsData>();
                    bool isVobSub = false;
                    bool isMatroska = false;
                    if (format == null && fileName.EndsWith(".sup", StringComparison.OrdinalIgnoreCase) && FileUtil.IsBluRaySup(fileName))
                    {
                        var log = new StringBuilder();
                        bluRaySubtitles = BluRaySupParser.ParseBluRaySup(fileName, log);
                    }
                    else if (format == null && fileName.EndsWith(".sub", StringComparison.OrdinalIgnoreCase) && FileUtil.IsVobSub(fileName))
                    {
                        isVobSub = true;
                    }
                    else if (format == null && (fileName.EndsWith(".mkv", StringComparison.OrdinalIgnoreCase) || fileName.EndsWith(".mks", StringComparison.OrdinalIgnoreCase)) && item.SubItems[2].Text.StartsWith("Matroska", StringComparison.Ordinal))
                    {
                        isMatroska = true;
                    }
                    if (format == null && bluRaySubtitles.Count == 0 && !isVobSub && !isMatroska)
                    {
                        IncrementAndShowProgress();
                    }
                    else
                    {
                        if (isMatroska && (Path.GetExtension(fileName).Equals(".mkv", StringComparison.OrdinalIgnoreCase) || Path.GetExtension(fileName).Equals(".mks", StringComparison.OrdinalIgnoreCase)))
                        {
                            using (var matroska = new MatroskaFile(fileName))
                            {
                                if (matroska.IsValid)
                                {
                                    var trackId = item.SubItems[2].Text;
                                    if (trackId.Contains("#"))
                                    {
                                        trackId = trackId.Remove(0, trackId.IndexOf("#", StringComparison.Ordinal) + 1);
                                    }

                                    foreach (var track in matroska.GetTracks(true))
                                    {
                                        if (track.CodecId.Equals("S_VOBSUB", StringComparison.OrdinalIgnoreCase))
                                        {
                                            if (trackId == track.TrackNumber.ToString(CultureInfo.InvariantCulture))
                                            {
                                                var vobSubs = LoadVobSubFromMatroska(track, matroska, out var idx);
                                                if (vobSubs.Count > 0)
                                                {
                                                    item.SubItems[3].Text = Configuration.Settings.Language.BatchConvert.Ocr;
                                                    using (var vobSubOcr = new VobSubOcr())
                                                    {
                                                        vobSubOcr.FileName = Path.GetFileName(fileName);
                                                        vobSubOcr.InitializeBatch(vobSubs, idx.Palette, Configuration.Settings.VobSubOcr, fileName, false, track.Language);
                                                        sub = vobSubOcr.SubtitleFromOcr;
                                                    }
                                                }
                                                fileName = fileName.Substring(0, fileName.LastIndexOf('.')) + ".#" + trackId + "." + track.Language + ".mkv";
                                                break;
                                            }
                                        }
                                        else if (track.CodecId.Equals("S_HDMV/PGS", StringComparison.OrdinalIgnoreCase))
                                        {
                                            if (trackId == track.TrackNumber.ToString(CultureInfo.InvariantCulture))
                                            {
                                                bluRaySubtitles = LoadBluRaySupFromMatroska(track, matroska, Handle);
                                                if (bluRaySubtitles.Count > 0)
                                                {
                                                    item.SubItems[3].Text = Configuration.Settings.Language.BatchConvert.Ocr;
                                                    using (var vobSubOcr = new VobSubOcr())
                                                    {
                                                        vobSubOcr.FileName = Path.GetFileName(fileName);
                                                        vobSubOcr.InitializeBatch(bluRaySubtitles, Configuration.Settings.VobSubOcr, fileName, false, track.Language);
                                                        sub = vobSubOcr.SubtitleFromOcr;
                                                    }
                                                }
                                                fileName = fileName.Substring(0, fileName.LastIndexOf('.')) + ".#" + trackId + "." + track.Language + ".mkv";
                                                break;
                                            }
                                        }
                                        else if (track.CodecId.Equals("S_TEXT/UTF8", StringComparison.OrdinalIgnoreCase) || track.CodecId.Equals("S_TEXT/SSA", StringComparison.OrdinalIgnoreCase) || track.CodecId.Equals("S_TEXT/ASS", StringComparison.OrdinalIgnoreCase))
                                        {
                                            if (trackId == track.TrackNumber.ToString(CultureInfo.InvariantCulture))
                                            {
                                                var mkvSub = matroska.GetSubtitle(track.TrackNumber, null);
                                                Utilities.LoadMatroskaTextSubtitle(track, matroska, mkvSub, sub);
                                                fileName = fileName.Substring(0, fileName.LastIndexOf('.')) + "." + track.Language + ".mkv";
                                                if (mkvFileNames.Contains(fileName))
                                                {
                                                    fileName = fileName.Substring(0, fileName.LastIndexOf('.')) + ".#" + trackId + "." + track.Language + ".mkv";
                                                }
                                                mkvFileNames.Add(fileName);
                                                break;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        else if (bluRaySubtitles.Count > 0)
                        {
                            if ((toFormat == BdnXmlSubtitle || toFormat == BluRaySubtitle ||
                                toFormat == VobSubSubtitle || toFormat == DostImageSubtitle) &&
                                AllowImageToImage())
                            {
                                foreach (var b in bluRaySubtitles)
                                {
                                    sub.Paragraphs.Add(new Paragraph(b.StartTimeCode, b.EndTimeCode, string.Empty));
                                }
                            }
                            else
                            {
                                item.SubItems[3].Text = Configuration.Settings.Language.BatchConvert.Ocr;
                                using (var vobSubOcr = new VobSubOcr())
                                {
                                    vobSubOcr.FileName = Path.GetFileName(fileName);
                                    vobSubOcr.InitializeBatch(bluRaySubtitles, Configuration.Settings.VobSubOcr, fileName, false);
                                    sub = vobSubOcr.SubtitleFromOcr;
                                }
                            }
                        }
                        else if (isVobSub)
                        {
                            item.SubItems[3].Text = Configuration.Settings.Language.BatchConvert.Ocr;
                            using (var vobSubOcr = new VobSubOcr())
                            {
                                vobSubOcr.InitializeBatch(fileName, Configuration.Settings.VobSubOcr, false);
                                sub = vobSubOcr.SubtitleFromOcr;
                            }
                        }
                        if (comboBoxSubtitleFormats.Text == AdvancedSubStationAlpha.NameOfFormat && _assStyle != null)
                        {
                            if (!string.IsNullOrWhiteSpace(_assStyle) && !checkBoxUseStyleFromSource.Checked)
                            {
                                sub.Header = _assStyle;
                            }
                        }
                        else if (comboBoxSubtitleFormats.Text == SubStationAlpha.NameOfFormat && _ssaStyle != null)
                        {
                            if (!string.IsNullOrWhiteSpace(_ssaStyle) && !checkBoxUseStyleFromSource.Checked)
                            {
                                sub.Header = _ssaStyle;
                            }
                        }

                        bool skip = CheckSkipFilter(fileName, format, sub);
                        if (skip)
                        {
                            item.SubItems[3].Text = Configuration.Settings.Language.BatchConvert.FilterSkipped;
                        }
                        else
                        {
                            if (checkBoxBridgeGaps.Checked)
                            {
                                Core.Forms.DurationsBridgeGaps.BridgeGaps(sub, _bridgeGaps.MinMsBetweenLines, !_bridgeGaps.PreviousSubtitleTakesAllTime, Configuration.Settings.Tools.BridgeGapMilliseconds, null, null);
                            }

                            // this variable is used to check whether one of the action is guaranteed to run
                            bool updateWarranty = checkBoxRemoveTextForHI.Checked || checkBoxRemoveFormatting.Checked ||
                                !numericUpDownPercent.Value.Equals(100);

                            if (updateWarranty)
                            {
                                Paragraph prev = default;
                                bool shouldRemoveHearingImpaired = checkBoxRemoveTextForHI.Checked;
                                bool shouldRemoveFormatting = checkBoxRemoveFormatting.Checked;
                                double percentage = (double)numericUpDownPercent.Value / 100.0;
                                bool shouldUpdateTime = percentage != 100;
                                _removeTextForHearingImpaired.Settings = _removeTextForHiSettings;
                                foreach (Paragraph p in sub.Paragraphs)
                                {
                                    if (shouldRemoveHearingImpaired)
                                    {
                                        p.Text = _removeTextForHearingImpaired.RemoveTextFromHearImpaired(p.Text);
                                    }
                                    if (shouldRemoveFormatting)
                                    {
                                        p.Text = HtmlUtil.RemoveHtmlTags(p.Text, true);
                                    }
                                    if (shouldUpdateTime)
                                    {
                                        p.StartTime.TotalMilliseconds *= percentage;
                                        p.EndTime.TotalMilliseconds *= percentage;

                                        // fix overlapping times
                                        if (prev?.EndTime.TotalMilliseconds >= p.StartTime.TotalMilliseconds)
                                        {
                                            prev.EndTime.TotalMilliseconds = p.StartTime.TotalMilliseconds - 1;
                                        }
                                        prev = p;
                                    }
                                }
                            }

                            if (bluRaySubtitles == null || bluRaySubtitles.Count == 0)
                            {
                                sub.RemoveEmptyLines(); //TODO: only for image export?
                            }
                            if (checkBoxFixCasing.Checked)
                            {
                                _changeCasing.FixCasing(sub, LanguageAutoDetect.AutoDetectGoogleLanguage(sub));
                                _changeCasingNames.Initialize(sub);
                                _changeCasingNames.FixCasing();
                            }

                            if (double.TryParse(comboBoxFrameRateFrom.Text.Replace(',', '.').Replace(CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator, "."), NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var fromFrameRate) &&
                            double.TryParse(comboBoxFrameRateTo.Text.Replace(',', '.').Replace(CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator, "."), NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var toFrameRate))
                            {
                                sub.ChangeFrameRate(fromFrameRate, toFrameRate);
                            }
                            if (timeUpDownAdjust.TimeCode.TotalMilliseconds > 0.00001)
                            {
                                var totalMilliseconds = timeUpDownAdjust.TimeCode.TotalMilliseconds;
                                if (radioButtonShowEarlier.Checked)
                                {
                                    totalMilliseconds *= -1;
                                }

                                sub.AddTimeToAllParagraphs(TimeSpan.FromMilliseconds(totalMilliseconds));
                            }
                            while (worker1.IsBusy && worker2.IsBusy && worker3.IsBusy)
                            {
                                Application.DoEvents();
                                System.Threading.Thread.Sleep(100);
                            }
                            var parameter = new ThreadDoWorkParameter(checkBoxFixCommonErrors.Checked, checkBoxMultipleReplace.Checked, checkBoxFixRtl.Checked, checkBoxSplitLongLines.Checked, checkBoxAutoBalance.Checked, checkBoxSetMinimumDisplayTimeBetweenSubs.Checked, item, sub, GetCurrentSubtitleFormat(), GetCurrentEncoding(), Configuration.Settings.Tools.BatchConvertLanguage, fileName, toFormat, format);
                            if (!worker1.IsBusy)
                            {
                                worker1.RunWorkerAsync(parameter);
                            }
                            else if (!worker2.IsBusy)
                            {
                                worker2.RunWorkerAsync(parameter);
                            }
                            else if (!worker3.IsBusy)
                            {
                                worker3.RunWorkerAsync(parameter);
                            }
                        }
                    }
                }
                catch (Exception exception)
                {
                    System.Diagnostics.Debug.WriteLine(exception);
                    IncrementAndShowProgress();
                }
                index++;
            }
            while (worker1.IsBusy || worker2.IsBusy || worker3.IsBusy)
            {
                try
                {
                    Application.DoEvents();
                }
                catch
                {
                    // ignored
                }
                System.Threading.Thread.Sleep(100);
            }
            ChangeControlStates(false);
        }

        private BackgroundWorker CreateWorker()
        {
            var worker = new BackgroundWorker();
            worker.RunWorkerCompleted += ThreadWorkerCompleted;
            worker.DoWork += DoThreadWork;
            return worker;
        }

        private void ChangeControlStates(bool converting)
        {
            _converting = converting;

            if (converting)
            {
                progressBar1.Style = ProgressBarStyle.Blocks;
                progressBar1.Maximum = listViewInputFiles.Items.Count;
                progressBar1.Value = 0;
                // no need for progress bar when there are only two subtitle
                progressBar1.Visible = progressBar1.Maximum > 2;
            }
            else
            {
                labelStatus.Text = string.Empty;
                TaskbarList.SetProgressState(Handle, TaskbarButtonProgressFlags.NoProgress);
                progressBar1.Visible = converting;
            }

            buttonConvert.Enabled = !converting;
            buttonCancel.Enabled = !converting;
            groupBoxOutput.Enabled = !converting;
            groupBoxConvertOptions.Enabled = !converting;
            buttonInputBrowse.Enabled = !converting;
            buttonSearchFolder.Enabled = !converting;
            comboBoxFilter.Enabled = !converting;
            textBoxFilter.Enabled = !converting;
        }

        /// <summary>
        /// Text based functions retuires text, so no image to image convert
        /// </summary>
        /// <returns></returns>
        private bool AllowImageToImage()
        {
            return !checkBoxAutoBalance.Checked &&
                   !checkBoxFixCasing.Checked &&
                   !checkBoxFixCommonErrors.Checked &&
                   !checkBoxFixRtl.Checked &&
                   !checkBoxMultipleReplace.Checked &&
                   !checkBoxRemoveFormatting.Checked &&
                   !checkBoxSplitLongLines.Checked &&
                   !checkBoxRemoveTextForHI.Checked;
        }

        internal static List<VobSubMergedPack> LoadVobSubFromMatroska(MatroskaTrackInfo matroskaSubtitleInfo, MatroskaFile matroska, out Idx idx)
        {
            var mergedVobSubPacks = new List<VobSubMergedPack>();
            if (matroskaSubtitleInfo.ContentEncodingType == 1)
            {
                idx = null;
                return mergedVobSubPacks;
            }

            var sub = matroska.GetSubtitle(matroskaSubtitleInfo.TrackNumber, null);
            idx = new Idx(matroskaSubtitleInfo.GetCodecPrivate().SplitToLines());
            foreach (var p in sub)
            {
                mergedVobSubPacks.Add(new VobSubMergedPack(p.GetData(matroskaSubtitleInfo), TimeSpan.FromMilliseconds(p.Start), 32, null));
                if (mergedVobSubPacks.Count > 0)
                {
                    mergedVobSubPacks[mergedVobSubPacks.Count - 1].EndTime = TimeSpan.FromMilliseconds(p.End);
                }

                // fix overlapping (some versions of Handbrake makes overlapping time codes - thx Hawke)
                if (mergedVobSubPacks.Count > 1 && mergedVobSubPacks[mergedVobSubPacks.Count - 2].EndTime > mergedVobSubPacks[mergedVobSubPacks.Count - 1].StartTime)
                {
                    mergedVobSubPacks[mergedVobSubPacks.Count - 2].EndTime = TimeSpan.FromMilliseconds(mergedVobSubPacks[mergedVobSubPacks.Count - 1].StartTime.TotalMilliseconds - 1);
                }
            }
            return mergedVobSubPacks;
        }

        internal static List<BluRaySupParser.PcsData> LoadBluRaySupFromMatroska(MatroskaTrackInfo track, MatroskaFile matroska, IntPtr handle)
        {
            if (track.ContentEncodingType == 1)
            {
                return new List<BluRaySupParser.PcsData>();
            }

            var sub = matroska.GetSubtitle(track.TrackNumber, null);
            TaskbarList.SetProgressState(handle, TaskbarButtonProgressFlags.NoProgress);
            var subtitles = new List<BluRaySupParser.PcsData>();
            var log = new StringBuilder();
            var clusterStream = new MemoryStream();
            foreach (var p in sub)
            {
                byte[] buffer = p.GetData(track);
                if (buffer != null && buffer.Length > 2)
                {
                    clusterStream.Write(buffer, 0, buffer.Length);
                    if (ContainsBluRayStartSegment(buffer))
                    {
                        if (subtitles.Count > 0 && subtitles[subtitles.Count - 1].StartTime == subtitles[subtitles.Count - 1].EndTime)
                        {
                            subtitles[subtitles.Count - 1].EndTime = (long)((p.Start - 1) * 90.0);
                        }
                        clusterStream.Position = 0;
                        var list = BluRaySupParser.ParseBluRaySup(clusterStream, log, true);
                        foreach (var sup in list)
                        {
                            sup.StartTime = (long)((p.Start - 1) * 90.0);
                            sup.EndTime = (long)((p.End - 1) * 90.0);
                            subtitles.Add(sup);

                            // fix overlapping
                            if (subtitles.Count > 1 && sub[subtitles.Count - 2].End > sub[subtitles.Count - 1].Start)
                            {
                                subtitles[subtitles.Count - 2].EndTime = subtitles[subtitles.Count - 1].StartTime - 1;
                            }
                        }
                        clusterStream = new MemoryStream();
                    }
                }
                else if (subtitles.Count > 0)
                {
                    var lastSub = subtitles[subtitles.Count - 1];
                    if (lastSub.StartTime == lastSub.EndTime)
                    {
                        lastSub.EndTime = (long)((p.Start - 1) * 90.0);
                        if (lastSub.EndTime - lastSub.StartTime > 1000000)
                        {
                            lastSub.EndTime = lastSub.StartTime;
                        }
                    }
                }
            }
            return subtitles;
        }

        private static bool ContainsBluRayStartSegment(byte[] buffer)
        {
            const int epochStart = 0x80;
            var position = 0;
            while (position + 3 <= buffer.Length)
            {
                var segmentType = buffer[position];
                if (segmentType == epochStart)
                {
                    return true;
                }

                int length = BluRaySupParser.BigEndianInt16(buffer, position + 1) + 3;
                position += length;
            }
            return false;
        }

        private bool CheckSkipFilter(string fileName, SubtitleFormat format, Subtitle sub)
        {
            bool skip = false;
            if (comboBoxFilter.SelectedIndex == 1)
            {
                if (format != null && format.GetType() == typeof(SubRip) && FileUtil.HasUtf8Bom(fileName))
                {
                    skip = true;
                }
            }
            else if (comboBoxFilter.SelectedIndex == 2)
            {
                skip = true;
                foreach (Paragraph p in sub.Paragraphs)
                {
                    if (p.Text != null && Utilities.GetNumberOfLines(p.Text) > 2)
                    {
                        skip = false;
                        break;
                    }
                }
            }
            else if (comboBoxFilter.SelectedIndex == 3 && !string.IsNullOrWhiteSpace(textBoxFilter.Text))
            {
                skip = true;
                foreach (Paragraph p in sub.Paragraphs)
                {
                    if (p.Text != null && p.Text.Contains(textBoxFilter.Text, StringComparison.Ordinal))
                    {
                        skip = false;
                        break;
                    }
                }
            }
            return skip;
        }

        private void IncrementAndShowProgress()
        {
            if (progressBar1.Value < progressBar1.Maximum)
            {
                progressBar1.Value++;
            }

            TaskbarList.SetProgressValue(Handle, progressBar1.Value, progressBar1.Maximum);
            labelStatus.Text = progressBar1.Value + " / " + progressBar1.Maximum;
        }

        private static void DoThreadWork(object sender, DoWorkEventArgs e)
        {
            var p = (ThreadDoWorkParameter)e.Argument;
            var mode = Configuration.Settings.Tools.BatchConvertFixRtlMode;

            if (p.FixRtl && mode == BatchConvertFixRtl.RemoveUnicode)
            {
                for (int i = 0; i < p.Subtitle.Paragraphs.Count; i++)
                {
                    var paragraph = p.Subtitle.Paragraphs[i];
                    paragraph.Text = paragraph.Text.Replace("\u200E", string.Empty);
                    paragraph.Text = paragraph.Text.Replace("\u200F", string.Empty);
                    paragraph.Text = paragraph.Text.Replace("\u202A", string.Empty);
                    paragraph.Text = paragraph.Text.Replace("\u202B", string.Empty);
                    paragraph.Text = paragraph.Text.Replace("\u202C", string.Empty);
                    paragraph.Text = paragraph.Text.Replace("\u202D", string.Empty);
                    paragraph.Text = paragraph.Text.Replace("\u202E", string.Empty);
                    paragraph.Text = paragraph.Text.Replace("\u00C2", " "); // no break space
                    paragraph.Text = paragraph.Text.Replace("\u00A0", " "); // no break space
                }
            }

            if (p.FixCommonErrors)
            {
                try
                {
                    using (var fixCommonErrors = new FixCommonErrors { BatchMode = true })
                    {
                        for (int i = 0; i < 3; i++)
                        {
                            fixCommonErrors.RunBatch(p.Subtitle, p.Format, p.Encoding, Configuration.Settings.Tools.BatchConvertLanguage);
                            p.Subtitle = fixCommonErrors.FixedSubtitle;
                        }
                    }
                }
                catch (Exception exception)
                {
                    p.Error = string.Format(Configuration.Settings.Language.BatchConvert.FixCommonErrorsErrorX, exception.Message);
                }
            }
            if (p.MultipleReplaceActive)
            {
                try
                {
                    using (var form = new MultipleReplace())
                    {
                        form.RunFromBatch(p.Subtitle);
                        p.Subtitle = form.FixedSubtitle;
                        p.Subtitle.RemoveParagraphsByIndices(form.DeleteIndices);
                    }
                }
                catch (Exception exception)
                {
                    p.Error = string.Format(Configuration.Settings.Language.BatchConvert.MultipleReplaceErrorX, exception.Message);
                }
            }
            if (p.SplitLongLinesActive)
            {
                try
                {
                    p.Subtitle = SplitLongLinesHelper.SplitLongLinesInSubtitle(p.Subtitle, Configuration.Settings.General.SubtitleLineMaximumLength * 2, Configuration.Settings.General.SubtitleLineMaximumLength);
                }
                catch (Exception exception)
                {
                    p.Error = string.Format(Configuration.Settings.Language.BatchConvert.AutoBalanceErrorX, exception.Message);
                }
            }
            if (p.AutoBalanceActive)
            {
                try
                {
                    foreach (var paragraph in p.Subtitle.Paragraphs)
                    {
                        paragraph.Text = Utilities.AutoBreakLine(paragraph.Text);
                    }
                }
                catch (Exception exception)
                {
                    p.Error = string.Format(Configuration.Settings.Language.BatchConvert.AutoBalanceErrorX, exception.Message);
                }
            }
            if (p.SetMinDisplayTimeBetweenSubtitles)
            {
                double minumumMillisecondsBetweenLines = Configuration.Settings.General.MinimumMillisecondsBetweenLines;
                for (int i = 0; i < p.Subtitle.Paragraphs.Count - 1; i++)
                {
                    Paragraph current = p.Subtitle.GetParagraphOrDefault(i);
                    Paragraph next = p.Subtitle.GetParagraphOrDefault(i + 1);
                    var gapsBetween = next.StartTime.TotalMilliseconds - current.EndTime.TotalMilliseconds;
                    if (gapsBetween < minumumMillisecondsBetweenLines && current.Duration.TotalMilliseconds > minumumMillisecondsBetweenLines)
                    {
                        current.EndTime.TotalMilliseconds -= (minumumMillisecondsBetweenLines - gapsBetween);
                    }
                }
            }

            if (p.FixRtl && mode == BatchConvertFixRtl.ReverseStartEnd)
            {
                for (int i = 0; i < p.Subtitle.Paragraphs.Count; i++)
                {
                    var paragraph = p.Subtitle.Paragraphs[i];
                    paragraph.Text = Utilities.ReverseStartAndEndingForRightToLeft(paragraph.Text);
                }
            }
            else if (p.FixRtl && mode == BatchConvertFixRtl.AddUnicode) // fix with unicode char
            {
                for (int i = 0; i < p.Subtitle.Paragraphs.Count; i++)
                {
                    var paragraph = p.Subtitle.Paragraphs[i];
                    paragraph.Text = Utilities.FixRtlViaUnicodeChars(paragraph.Text);
                }
            }

            // always re-number
            p.Subtitle.Renumber();

            e.Result = p;
        }

        private void ThreadWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            var p = (ThreadDoWorkParameter)e.Result;
            if (p.Item.Index + 2 < listViewInputFiles.Items.Count)
            {
                listViewInputFiles.EnsureVisible(p.Item.Index + 2);
            }
            else
            {
                listViewInputFiles.EnsureVisible(p.Item.Index);
            }

            if (!string.IsNullOrEmpty(p.Error))
            {
                p.Item.SubItems[3].Text = p.Error;
            }
            else
            {
                if (p.SourceFormat == null)
                {
                    p.SourceFormat = new SubRip();
                }

                if (p.ToFormat == Ebu.NameOfFormat)
                {
                    p.Subtitle.Header = _ebuGeneralInformation.ToString();
                }

                var targetFormat = p.ToFormat;
                if (targetFormat == Configuration.Settings.Language.ExportCustomText.Title)
                {
                    targetFormat = "CustomText:" + _customTextTemplate;
                }

                try
                {
                    var binaryParagraphs = new List<IBinaryParagraph>();
                    if (p.FileName != null && p.FileName.EndsWith(".sup", StringComparison.OrdinalIgnoreCase) && FileUtil.IsBluRaySup(p.FileName) && AllowImageToImage())
                    {
                        binaryParagraphs = BluRaySupParser.ParseBluRaySup(p.FileName, new StringBuilder()).Cast<IBinaryParagraph>().ToList();
                    }
                    var dir = textBoxOutputFolder.Text;
                    var overwrite = checkBoxOverwrite.Checked;
                    if (checkBoxOverwriteOriginalFiles.Checked)
                    {
                        dir = Path.GetDirectoryName(p.FileName);
                        overwrite = true;
                    }
                    var success = CommandLineConvert.BatchConvertSave(targetFormat, TimeSpan.Zero, GetCurrentEncoding(), dir, _count, ref _converted, ref _errors, _allFormats, p.FileName, p.Subtitle, p.SourceFormat, binaryParagraphs, overwrite, -1, null, null);
                    p.Item.SubItems[3].Text = success ? Configuration.Settings.Language.BatchConvert.Converted : Configuration.Settings.Language.BatchConvert.NotConverted;
                }
                catch (Exception exception)
                {
                    p.Error = $"Save: {exception.InnerException?.Message ?? exception.Message}";
                    p.Item.SubItems[3].Text = p.Error;
                }

                IncrementAndShowProgress();
                if (progressBar1.Value == progressBar1.Maximum)
                {
                    labelStatus.Text = string.Empty;
                }
            }
        }

        private void ComboBoxSubtitleFormatsSelectedIndexChanged(object sender, EventArgs e)
        {
            checkBoxUseStyleFromSource.Visible = false;
            if (comboBoxSubtitleFormats.Text == AdvancedSubStationAlpha.NameOfFormat || comboBoxSubtitleFormats.Text == SubStationAlpha.NameOfFormat)
            {
                buttonStyles.Text = Configuration.Settings.Language.BatchConvert.Style;
                buttonStyles.Visible = true;
                comboBoxEncoding.Enabled = true;
                checkBoxUseStyleFromSource.Visible = true;
                checkBoxUseStyleFromSource.Left = buttonStyles.Left + buttonStyles.Width - checkBoxUseStyleFromSource.Width;
            }
            else if (comboBoxSubtitleFormats.Text == Ebu.NameOfFormat)
            {
                buttonStyles.Text = Configuration.Settings.Language.BatchConvert.Settings;
                buttonStyles.Visible = true;
                if (_ebuGeneralInformation == null)
                {
                    _ebuGeneralInformation = new Ebu.EbuGeneralSubtitleInformation();
                }

                comboBoxEncoding.Enabled = true;
            }
            else if (comboBoxSubtitleFormats.Text == BluRaySubtitle ||
                     comboBoxSubtitleFormats.Text == VobSubSubtitle ||
                     comboBoxSubtitleFormats.Text == DostImageSubtitle ||
                     comboBoxSubtitleFormats.Text == BdnXmlSubtitle ||
                     comboBoxSubtitleFormats.Text == FcpImageSubtitle ||
                     comboBoxSubtitleFormats.Text == Configuration.Settings.Language.ExportCustomText.Title)
            {
                buttonStyles.Text = Configuration.Settings.Language.BatchConvert.Settings;
                buttonStyles.Visible = true;
                comboBoxEncoding.Enabled = false;
            }
            else if (comboBoxSubtitleFormats.Text == Configuration.Settings.Language.BatchConvert.PlainText)
            {
                buttonStyles.Text = Configuration.Settings.Language.BatchConvert.Settings;
                buttonStyles.Visible = true;
                comboBoxEncoding.Enabled = true;
            }
            else
            {
                buttonStyles.Visible = false;
                comboBoxEncoding.Enabled = true;
            }
        }

        private void ButtonStylesClick(object sender, EventArgs e)
        {
            if (comboBoxSubtitleFormats.Text == AdvancedSubStationAlpha.NameOfFormat || comboBoxSubtitleFormats.Text == SubStationAlpha.NameOfFormat)
            {
                ShowAssSsaStyles();
            }
            else if (comboBoxSubtitleFormats.Text == Ebu.NameOfFormat)
            {
                ShowEbuSettings();
            }
            else if (comboBoxSubtitleFormats.Text == BluRaySubtitle)
            {
                ImageExportSettings(ExportPngXml.ExportFormats.BluraySup);
            }
            else if (comboBoxSubtitleFormats.Text == VobSubSubtitle)
            {
                ImageExportSettings(ExportPngXml.ExportFormats.VobSub);
            }
            else if (comboBoxSubtitleFormats.Text == DostImageSubtitle)
            {
                ImageExportSettings(ExportPngXml.ExportFormats.Dost);
            }
            else if (comboBoxSubtitleFormats.Text == BdnXmlSubtitle)
            {
                ImageExportSettings(ExportPngXml.ExportFormats.BdnXml);
            }
            else if (comboBoxSubtitleFormats.Text == FcpImageSubtitle)
            {
                ImageExportSettings(ExportPngXml.ExportFormats.Fcp);
            }
            else if (comboBoxSubtitleFormats.Text == Configuration.Settings.Language.BatchConvert.PlainText)
            {
                using (var form = new ExportText())
                {
                    var s = new Subtitle();
                    s.Paragraphs.Add(new Paragraph("Test 123." + Environment.NewLine + "Test 456.", 0, 4000));
                    s.Paragraphs.Add(new Paragraph("Test 789.", 5000, 9000));
                    form.Initialize(s, null);
                    form.PrepareForBatchSettings();
                    form.ShowDialog(this);
                }
            }
            else if (comboBoxSubtitleFormats.Text == Configuration.Settings.Language.ExportCustomText.Title)
            {
                ShowExportCustomTextSettings();
            }
        }

        private void ShowExportCustomTextSettings()
        {
            var s = new Subtitle();
            s.Paragraphs.Add(new Paragraph("Test 123." + Environment.NewLine + "Test 456.", 0, 4000));
            s.Paragraphs.Add(new Paragraph("Test 777." + Environment.NewLine + "Test 888.", 0, 4000));
            using (var properties = new ExportCustomText(s, null, "Test"))
            {
                properties.InitializeForBatchConvert(_customTextTemplate);
                if (properties.ShowDialog(this) == DialogResult.OK)
                {
                    _customTextTemplate = properties.CurrentFormatName;
                }
            }
        }

        private void ImageExportSettings(string format)
        {
            using (var properties = new ExportPngXml())
            {
                var s = new Subtitle();
                s.Paragraphs.Add(new Paragraph("Test 123." + Environment.NewLine + "Test 456.", 0, 4000));
                properties.Initialize(s, new SubRip(), format, null, null, null);
                properties.DisableSaveButtonAndCheckBoxes();
                properties.ShowDialog(this);
            }
        }

        private void ShowEbuSettings()
        {
            using (var properties = new EbuSaveOptions())
            {
                properties.Initialize(_ebuGeneralInformation, 0, null, null);
                properties.ShowDialog(this);
            }
        }

        private void ShowAssSsaStyles()
        {
            SubStationAlphaStylesBatchConvert form = null;
            try
            {
                var assa = new AdvancedSubStationAlpha();
                var sub = new Subtitle();
                if (comboBoxSubtitleFormats.Text == assa.Name)
                {
                    if (!string.IsNullOrEmpty(_assStyle))
                    {
                        sub.Header = _assStyle;
                    }
                    form = new SubStationAlphaStylesBatchConvert(sub, assa);
                    if (form.ShowDialog(this) == DialogResult.OK)
                    {
                        _assStyle = form.Header;
                    }
                }
                else
                {
                    if (!string.IsNullOrEmpty(_ssaStyle))
                    {
                        sub.Header = _ssaStyle;
                    }
                    var ssa = new SubStationAlpha();
                    if (comboBoxSubtitleFormats.Text == ssa.Name)
                    {
                        form = new SubStationAlphaStylesBatchConvert(sub, ssa);
                        if (form.ShowDialog(this) == DialogResult.OK)
                        {
                            _ssaStyle = form.Header;
                        }
                    }
                }
            }
            finally
            {
                form?.Dispose();
            }
        }

        private void LinkLabelOpenOutputFolderLinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (Directory.Exists(textBoxOutputFolder.Text))
            {
                System.Diagnostics.Process.Start(textBoxOutputFolder.Text);
            }
            else
            {
                MessageBox.Show(string.Format(Configuration.Settings.Language.SplitSubtitle.FolderNotFoundX, textBoxOutputFolder.Text));
            }
        }

        private void ContextMenuStripFilesOpening(object sender, CancelEventArgs e)
        {
            if (listViewInputFiles.Items.Count == 0 || _converting)
            {
                e.Cancel = true;
                return;
            }
            removeToolStripMenuItem.Visible = listViewInputFiles.SelectedItems.Count > 0;
        }

        private void RemoveAllToolStripMenuItemClick(object sender, EventArgs e)
        {
            listViewInputFiles.Items.Clear();
        }

        private void RemoveSelectedFiles()
        {
            if (_converting)
            {
                return;
            }

            for (int i = listViewInputFiles.SelectedIndices.Count - 1; i >= 0; i--)
            {
                listViewInputFiles.Items.RemoveAt(listViewInputFiles.SelectedIndices[i]);
            }
        }

        private void RemoveToolStripMenuItemClick(object sender, EventArgs e)
        {
            RemoveSelectedFiles();
        }

        private void ListViewInputFilesKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {
                RemoveSelectedFiles();
            }
            else if (e.KeyCode == Keys.A && e.Modifiers == Keys.Control)
            {
                listViewInputFiles.SelectAll();
                e.SuppressKeyPress = true;
            }
            else if (e.KeyCode == Keys.D && e.Modifiers == Keys.Control)
            {
                listViewInputFiles.SelectFirstSelectedItemOnly();
                e.SuppressKeyPress = true;
            }
            else if (e.KeyCode == Keys.I && e.Modifiers == (Keys.Control | Keys.Shift)) //InverseSelection
            {
                listViewInputFiles.InverseSelection();
                e.SuppressKeyPress = true;
            }
        }

        private void buttonFixCommonErrorSettings_Click(object sender, EventArgs e)
        {
            using (var form = new FixCommonErrors { BatchMode = true })
            {
                form.RunBatchSettings(new Subtitle(), GetCurrentSubtitleFormat(), GetCurrentEncoding(), Configuration.Settings.Tools.BatchConvertLanguage);
                form.ShowDialog(this);
                Configuration.Settings.Tools.BatchConvertLanguage = form.Language;
            }
        }

        private void BatchConvert_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (_converting)
            {
                e.Cancel = true;
                return;
            }
            if (_searching)
            {
                _abort = true;
            }

            Configuration.Settings.Tools.BatchConvertFixCasing = checkBoxFixCasing.Checked;
            Configuration.Settings.Tools.BatchConvertFixCommonErrors = checkBoxFixCommonErrors.Checked;
            Configuration.Settings.Tools.BatchConvertMultipleReplace = checkBoxMultipleReplace.Checked;
            Configuration.Settings.Tools.BatchConvertFixRtl = checkBoxFixRtl.Checked;
            Configuration.Settings.Tools.BatchConvertSplitLongLines = checkBoxSplitLongLines.Checked;
            Configuration.Settings.Tools.BatchConvertAutoBalance = checkBoxAutoBalance.Checked;
            Configuration.Settings.Tools.BatchConvertRemoveFormatting = checkBoxRemoveFormatting.Checked;
            Configuration.Settings.Tools.BatchConvertBridgeGaps = checkBoxBridgeGaps.Checked;
            Configuration.Settings.Tools.BatchConvertRemoveTextForHI = checkBoxRemoveTextForHI.Checked;
            Configuration.Settings.Tools.BatchConvertSetMinDisplayTimeBetweenSubtitles = checkBoxSetMinimumDisplayTimeBetweenSubs.Checked;
            Configuration.Settings.Tools.BatchConvertOutputFolder = textBoxOutputFolder.Text;
            Configuration.Settings.Tools.BatchConvertOverwriteExisting = checkBoxOverwrite.Checked;
            Configuration.Settings.Tools.BatchConvertOverwriteOriginal = checkBoxOverwriteOriginalFiles.Checked;
            Configuration.Settings.Tools.BatchConvertFormat = comboBoxSubtitleFormats.SelectedItem.ToString();
            Configuration.Settings.Tools.BatchConvertAssStyles = _assStyle;
            Configuration.Settings.Tools.BatchConvertSsaStyles = _ssaStyle;
            Configuration.Settings.Tools.BatchConvertUseStyleFromSource = checkBoxUseStyleFromSource.Checked;
            Configuration.Settings.Tools.BatchConvertExportCustomTextTemplate = _customTextTemplate;
        }

        private void buttonMultipleReplaceSettings_Click(object sender, EventArgs e)
        {
            using (var form = new MultipleReplace())
            {
                form.Initialize(new Subtitle());
                form.ShowDialog(this);
            }
        }

        private void checkBoxOverwriteOriginalFiles_CheckedChanged(object sender, EventArgs e)
        {
            labelChooseOutputFolder.Enabled = !checkBoxOverwriteOriginalFiles.Checked;
            textBoxOutputFolder.Enabled = !checkBoxOverwriteOriginalFiles.Checked;
            checkBoxOverwrite.Enabled = !checkBoxOverwriteOriginalFiles.Checked;
            buttonChooseFolder.Enabled = !checkBoxOverwriteOriginalFiles.Checked;
        }

        private void buttonSearchFolder_Click(object sender, EventArgs e)
        {
            folderBrowserDialog1.ShowNewFolderButton = false;
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                listViewInputFiles.BeginUpdate();
                buttonConvert.Enabled = false;
                buttonCancel.Enabled = false;
                progressBar1.Style = ProgressBarStyle.Marquee;
                progressBar1.Visible = true;
                groupBoxOutput.Enabled = false;
                groupBoxConvertOptions.Enabled = false;
                buttonInputBrowse.Enabled = false;
                buttonSearchFolder.Enabled = false;
                labelStatus.Text = string.Format(Configuration.Settings.Language.BatchConvert.ScanningFolder, folderBrowserDialog1.SelectedPath);

                SearchFolder(folderBrowserDialog1.SelectedPath);

                labelStatus.Text = string.Empty;
                buttonConvert.Enabled = true;
                buttonCancel.Enabled = true;
                progressBar1.Style = ProgressBarStyle.Continuous;
                progressBar1.Visible = true;
                groupBoxOutput.Enabled = true;
                groupBoxConvertOptions.Enabled = true;
                buttonInputBrowse.Enabled = true;
                buttonSearchFolder.Enabled = true;
                listViewInputFiles.EndUpdate();
            }
        }

        private void SearchFolder(string path)
        {
            _abort = false;
            _searching = true;
            try
            {
                SearchFolderRecurse(path);
            }
            finally
            {
                _searching = false;
            }
        }

        private void SearchFolderRecurse(string path)
        {
            foreach (string fileName in Directory.EnumerateFiles(path))
            {
                try
                {
                    string ext = Path.GetExtension(fileName).ToLowerInvariant();
                    if (ext != "" &&
                        ext != ".png" &&
                        ext != ".jpg" &&
                        ext != ".docx" &&
                        ext != ".pptx" &&
                        ext != ".xlsx" &&
                        ext != ".pdf" &&
                        ext != ".dll" &&
                        ext != ".exe" &&
                        ext != ".rar" &&
                        ext != ".7z" &&
                        ext != ".zip")
                    {
                        var fi = new FileInfo(fileName);
                        if (ext == ".sub" && FileUtil.IsVobSub(fileName))
                        {
                            AddFromSearch(fileName, fi, "VobSub");
                        }
                        else if (ext == ".sup" && FileUtil.IsBluRaySup(fileName))
                        {
                            AddFromSearch(fileName, fi, "Blu-ray");
                        }
                        else
                        {
                            if (fi.Length < ConvertMaxFileSize)
                            {
                                var sub = new Subtitle();
                                var format = sub.LoadSubtitle(fileName, out _, null);

                                if (format == null)
                                {
                                    foreach (var f in SubtitleFormat.GetBinaryFormats(true))
                                    {
                                        if (f.IsMine(null, fileName))
                                        {
                                            f.LoadSubtitle(sub, null, fileName);
                                            format = f;
                                            break;
                                        }
                                    }
                                }

                                if (format == null)
                                {
                                    var encoding = LanguageAutoDetect.GetEncodingFromFile(fileName);
                                    var lines = FileUtil.ReadAllTextShared(fileName, encoding).SplitToLines();
                                    foreach (var f in SubtitleFormat.GetTextOtherFormats())
                                    {
                                        if (f.IsMine(lines, fileName))
                                        {
                                            f.LoadSubtitle(sub, lines, fileName);
                                            format = f;
                                            break;
                                        }
                                    }
                                }

                                if (format != null)
                                {
                                    AddFromSearch(fileName, fi, format.Name);
                                }
                            }
                        }
                        progressBar1.Refresh();
                        Application.DoEvents();
                        if (_abort)
                        {
                            return;
                        }
                    }
                }
                catch
                {
                    // ignored
                }
            }
            if (checkBoxScanFolderRecursive.Checked)
            {
                foreach (string directory in Directory.EnumerateDirectories(path))
                {
                    if (directory != "." && directory != "..")
                    {
                        SearchFolderRecurse(directory);
                    }

                    if (_abort)
                    {
                        return;
                    }
                }
            }
        }

        private void AddFromSearch(string fileName, FileInfo fi, string nameOfFormat)
        {
            var item = new ListViewItem(fileName);
            item.SubItems.Add(Utilities.FormatBytesToDisplayFileSize(fi.Length));
            item.SubItems.Add(nameOfFormat);
            item.SubItems.Add("-");
            listViewInputFiles.Items.Add(item);
        }

        private void BatchConvert_KeyDown(object sender, KeyEventArgs e)
        {
            if (_converting || _searching)
            {
                if (e.KeyCode == Keys.Escape)
                {
                    _abort = true;
                    e.SuppressKeyPress = true;
                }
            }
            else if (e.KeyCode == Keys.Escape)
            {
                Close();
            }
            else if (e.KeyData == (Keys.Control | Keys.O)) // Open file/s
            {
                buttonInputBrowse_Click(null, EventArgs.Empty);
            }
        }

        RemoveTextForHISettings _removeTextForHiSettings;
        private void buttonRemoveTextForHiSettings_Click(object sender, EventArgs e)
        {
            using (var form = new FormRemoveTextForHearImpaired(null, new Subtitle()))
            {
                form.InitializeSettingsOnly();
                form.ShowDialog(this);
                _removeTextForHiSettings = form.GetSettings(new Subtitle());
            }
        }

        private void comboBoxFilter_SelectedIndexChanged(object sender, EventArgs e)
        {
            textBoxFilter.Visible = comboBoxFilter.SelectedIndex == 3;
        }

        private void buttonBridgeGapsSettings_Click(object sender, EventArgs e)
        {
            _bridgeGaps.ShowDialog(this);
        }

        private void buttonFixRtlSettings_Click(object sender, EventArgs e)
        {
            using (var form = new BatchConvertFixRtl())
            {
                form.ShowDialog(this);
            }
        }
    }
}
