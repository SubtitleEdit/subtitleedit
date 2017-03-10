using Nikse.SubtitleEdit.Core;
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
using System.Text;
using System.Windows.Forms;
using Nikse.SubtitleEdit.Forms.Ocr;

namespace Nikse.SubtitleEdit.Forms
{
    public sealed partial class BatchConvert : PositionAndSizeForm
    {

        public class ThreadDoWorkParameter
        {
            public bool FixCommonErrors { get; set; }
            public bool MultipleReplaceActive { get; set; }
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
            public ThreadDoWorkParameter(bool fixCommonErrors, bool multipleReplace, bool splitLongLinesActive, bool autoBalance, bool setMinDisplayTimeBetweenSubtitles, ListViewItem item, Subtitle subtitle, SubtitleFormat format, Encoding encoding, string language, string fileName, string toFormat, SubtitleFormat sourceFormat)
            {
                FixCommonErrors = fixCommonErrors;
                MultipleReplaceActive = multipleReplace;
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
        private bool _abort;
        private Ebu.EbuGeneralSubtitleInformation _ebuGeneralInformation;
        public const string BluRaySubtitle = "Blu-ray sup";
        public const string VobSubSubtitle = "VobSub";
        private string _customTextTemplate;

        public BatchConvert(Icon icon)
        {
            InitializeComponent();
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
            groupBoxOffsetTimeCodes.Text = Configuration.Settings.Language.ShowEarlierLater.TitleAll;
            groupBoxSpeed.Text = Configuration.Settings.Language.ChangeSpeedInPercent.TitleShort;
            labelFromFrameRate.Text = Configuration.Settings.Language.ChangeFrameRate.FromFrameRate;
            labelToFrameRate.Text = Configuration.Settings.Language.ChangeFrameRate.ToFrameRate;
            labelHourMinSecMilliSecond.Text = Configuration.Settings.Language.General.HourMinutesSecondsMilliseconds;

            comboBoxFrameRateFrom.Left = labelFromFrameRate.Left + labelFromFrameRate.Width + 3;
            comboBoxFrameRateTo.Left = labelToFrameRate.Left + labelToFrameRate.Width + 3;
            if (comboBoxFrameRateFrom.Left > comboBoxFrameRateTo.Left)
                comboBoxFrameRateTo.Left = comboBoxFrameRateFrom.Left;
            else
                comboBoxFrameRateFrom.Left = comboBoxFrameRateTo.Left;

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

            comboBoxFrameRateTo.Items.Add(23.976);
            comboBoxFrameRateTo.Items.Add(24.0);
            comboBoxFrameRateTo.Items.Add(25.0);
            comboBoxFrameRateTo.Items.Add(29.97);

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
            formatNames.Add(Configuration.Settings.Language.ExportCustomText.Title);
            UiUtil.InitializeSubtitleFormatComboBox(comboBoxSubtitleFormats, formatNames, Configuration.Settings.Tools.BatchConvertFormat);

            UiUtil.InitializeTextEncodingComboBox(comboBoxEncoding);

            if (string.IsNullOrEmpty(Configuration.Settings.Tools.BatchConvertOutputFolder) || !Directory.Exists(Configuration.Settings.Tools.BatchConvertOutputFolder))
                textBoxOutputFolder.Text = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            else
                textBoxOutputFolder.Text = Configuration.Settings.Tools.BatchConvertOutputFolder;
            checkBoxOverwrite.Checked = Configuration.Settings.Tools.BatchConvertOverwriteExisting;
            checkBoxOverwriteOriginalFiles.Checked = Configuration.Settings.Tools.BatchConvertOverwriteOriginal;
            checkBoxFixCasing.Checked = Configuration.Settings.Tools.BatchConvertFixCasing;
            checkBoxFixCommonErrors.Checked = Configuration.Settings.Tools.BatchConvertFixCommonErrors;
            checkBoxMultipleReplace.Checked = Configuration.Settings.Tools.BatchConvertMultipleReplace;
            checkBoxSplitLongLines.Checked = Configuration.Settings.Tools.BatchConvertSplitLongLines;
            checkBoxAutoBalance.Checked = Configuration.Settings.Tools.BatchConvertAutoBalance;
            checkBoxRemoveFormatting.Checked = Configuration.Settings.Tools.BatchConvertRemoveFormatting;
            checkBoxRemoveTextForHI.Checked = Configuration.Settings.Tools.BatchConvertRemoveTextForHI;
            checkBoxSetMinimumDisplayTimeBetweenSubs.Checked = Configuration.Settings.Tools.BatchConvertSetMinDisplayTimeBetweenSubtitles;
            buttonRemoveTextForHiSettings.Text = l.Settings;
            buttonFixCommonErrorSettings.Text = l.Settings;
            buttonMultipleReplaceSettings.Text = l.Settings;
            checkBoxFixCommonErrors.Text = Configuration.Settings.Language.FixCommonErrors.Title;
            checkBoxMultipleReplace.Text = Configuration.Settings.Language.MultipleReplace.Title;
            checkBoxAutoBalance.Text = l.AutoBalance;
            checkBoxSplitLongLines.Text = l.SplitLongLines;
            radioButtonShowEarlier.Text = Configuration.Settings.Language.ShowEarlierLater.ShowEarlier;
            radioButtonShowLater.Text = Configuration.Settings.Language.ShowEarlierLater.ShowLater;
            radioButtonSpeedCustom.Text = Configuration.Settings.Language.ChangeSpeedInPercent.Custom;
            radioButtonSpeedFromDropFrame.Text = Configuration.Settings.Language.ChangeSpeedInPercent.FromDropFrame;
            radioButtonToDropFrame.Text = Configuration.Settings.Language.ChangeSpeedInPercent.ToDropFrame;
            checkBoxSetMinimumDisplayTimeBetweenSubs.Text = l.SetMinMsBetweenSubtitles;

            _removeTextForHearingImpaired = new RemoveTextForHI(new RemoveTextForHISettings());

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
            _customTextTemplate = Configuration.Settings.Tools.BatchConvertExportCustomTextTemplate;
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
                foreach (string fileName in openFileDialog1.FileNames)
                {
                    AddInputFile(fileName);
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
                        return;
                }

                var fi = new FileInfo(fileName);
                var ext = fi.Extension;
                var item = new ListViewItem(fileName);
                item.SubItems.Add(Utilities.FormatBytesToDisplayFileSize(fi.Length));

                SubtitleFormat format = null;
                var sub = new Subtitle();
                if (fi.Length < 1024 * 1024 * 10) // max 10 mb
                {
                    Encoding encoding;
                    format = sub.LoadSubtitle(fileName, out encoding, null);

                    if (format == null)
                    {
                        var ebu = new Ebu();
                        if (ebu.IsMine(null, fileName))
                        {
                            format = ebu;
                        }
                    }
                    if (format == null)
                    {
                        var pac = new Pac();
                        if (pac.IsMine(null, fileName))
                        {
                            format = pac;
                        }
                    }
                    if (format == null)
                    {
                        var cavena890 = new Cavena890();
                        if (cavena890.IsMine(null, fileName))
                        {
                            format = cavena890;
                        }
                    }
                    if (format == null)
                    {
                        var spt = new Spt();
                        if (spt.IsMine(null, fileName))
                        {
                            format = spt;
                        }
                    }
                    if (format == null)
                    {
                        var cheetahCaption = new CheetahCaption();
                        if (cheetahCaption.IsMine(null, fileName))
                        {
                            format = cheetahCaption;
                        }
                    }
                    if (format == null)
                    {
                        var chk = new Chk();
                        if (chk.IsMine(null, fileName))
                        {
                            format = chk;
                        }
                    }
                    if (format == null)
                    {
                        var ayato = new Ayato();
                        if (ayato.IsMine(null, fileName))
                        {
                            format = ayato;
                        }
                    }
                    if (format == null)
                    {
                        var f = new PacUnicode();
                        if (f.IsMine(null, fileName))
                        {
                            format = f;
                        }
                    }
                    if (format == null)
                    {
                        var f = new IaiSub();
                        if (f.IsMine(null, fileName))
                        {
                            format = f;
                        }
                    }
                    if (format == null)
                    {
                        var f = new DlDd();
                        if (f.IsMine(null, fileName))
                        {
                            format = f;
                        }
                    }
                    if (format == null)
                    {
                        var f = new Ted20();
                        if (f.IsMine(null, fileName))
                        {
                            format = f;
                        }
                    }
                    if (format == null)
                    {
                        var capMakerPlus = new CapMakerPlus();
                        if (capMakerPlus.IsMine(null, fileName))
                        {
                            format = capMakerPlus;
                        }
                    }
                    if (format == null)
                    {
                        var captionate = new Captionate();
                        if (captionate.IsMine(null, fileName))
                        {
                            format = captionate;
                        }
                    }
                    if (format == null)
                    {
                        var ultech130 = new Ultech130();
                        if (ultech130.IsMine(null, fileName))
                        {
                            format = ultech130;
                        }
                    }
                    if (format == null)
                    {
                        var nciCaption = new NciCaption();
                        if (nciCaption.IsMine(null, fileName))
                        {
                            format = nciCaption;
                        }
                    }
                    if (format == null)
                    {
                        var avidStl = new AvidStl();
                        if (avidStl.IsMine(null, fileName))
                        {
                            format = avidStl;
                        }
                    }
                    if (format == null)
                    {
                        var asc = new TimeLineAscii();
                        if (asc.IsMine(null, fileName))
                        {
                            format = asc;
                        }
                    }
                    if (format == null)
                    {
                        var asc = new TimeLineFootageAscii();
                        if (asc.IsMine(null, fileName))
                        {
                            format = asc;
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
                        int mkvCount = 0;
                        using (var matroska = new MatroskaFile(fileName))
                        {
                            if (matroska.IsValid)
                            {
                                foreach (var track in matroska.GetTracks(true))
                                {
                                    if (track.CodecId.Equals("S_VOBSUB", StringComparison.OrdinalIgnoreCase))
                                    {
                                        // TODO: Convert from VobSub image based format!
                                    }
                                    else if (track.CodecId.Equals("S_HDMV/PGS", StringComparison.OrdinalIgnoreCase))
                                    {
                                        // TODO: Convert from Blu-ray image based format!
                                    }
                                    else if (track.CodecId.Equals("S_TEXT/UTF8", StringComparison.OrdinalIgnoreCase) || track.CodecId.Equals("S_TEXT/SSA", StringComparison.OrdinalIgnoreCase) || track.CodecId.Equals("S_TEXT/ASS", StringComparison.OrdinalIgnoreCase))
                                    {
                                        mkvCount++;
                                    }
                                }
                            }
                        }
                        if (mkvCount > 0)
                        {
                            item.SubItems.Add("Matroska - " + mkvCount);
                        }
                        else
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

                listViewInputFiles.Items.Add(item);
            }
            catch
            {
            }
        }

        private void listViewInputFiles_DragEnter(object sender, DragEventArgs e)
        {
            if (_converting)
            {
                e.Effect = DragDropEffects.None;
                return;
            }

            if (e.Data.GetDataPresent(DataFormats.FileDrop, false))
                e.Effect = DragDropEffects.All;
        }

        private void listViewInputFiles_DragDrop(object sender, DragEventArgs e)
        {
            if (_converting)
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
            _converting = true;
            buttonConvert.Enabled = false;
            buttonCancel.Enabled = false;
            progressBar1.Style = ProgressBarStyle.Blocks;
            progressBar1.Maximum = listViewInputFiles.Items.Count;
            progressBar1.Value = 0;
            progressBar1.Visible = progressBar1.Maximum > 2;
            string toFormat = comboBoxSubtitleFormats.Text;
            groupBoxOutput.Enabled = false;
            groupBoxConvertOptions.Enabled = false;
            buttonInputBrowse.Enabled = false;
            buttonSearchFolder.Enabled = false;
            comboBoxFilter.Enabled = false;
            textBoxFilter.Enabled = false;
            _count = 0;
            _converted = 0;
            _errors = 0;
            _abort = false;
            var worker1 = new BackgroundWorker();
            var worker2 = new BackgroundWorker();
            var worker3 = new BackgroundWorker();
            worker1.DoWork += DoThreadWork;
            worker1.RunWorkerCompleted += ThreadWorkerCompleted;
            worker2.DoWork += DoThreadWork;
            worker2.RunWorkerCompleted += ThreadWorkerCompleted;
            worker3.DoWork += DoThreadWork;
            worker3.RunWorkerCompleted += ThreadWorkerCompleted;
            listViewInputFiles.BeginUpdate();
            foreach (ListViewItem item in listViewInputFiles.Items)
                item.SubItems[3].Text = "-";
            listViewInputFiles.EndUpdate();
            Refresh();
            int index = 0;
            while (index < listViewInputFiles.Items.Count && _abort == false)
            {
                ListViewItem item = listViewInputFiles.Items[index];
                string fileName = item.Text;
                try
                {
                    SubtitleFormat format = null;
                    var sub = new Subtitle();
                    var fi = new FileInfo(fileName);
                    if (fi.Length < 1024 * 1024) // max 1 mb
                    {
                        Encoding encoding;
                        format = sub.LoadSubtitle(fileName, out encoding, null);
                        if (format == null)
                        {
                            var ebu = new Ebu();
                            if (ebu.IsMine(null, fileName))
                            {
                                ebu.LoadSubtitle(sub, null, fileName);
                                format = ebu;
                            }
                        }
                        if (format == null)
                        {
                            var pac = new Pac();
                            if (pac.IsMine(null, fileName))
                            {
                                pac.BatchMode = true;
                                pac.LoadSubtitle(sub, null, fileName);
                                format = pac;
                            }
                        }
                        if (format == null)
                        {
                            var cavena890 = new Cavena890();
                            if (cavena890.IsMine(null, fileName))
                            {
                                cavena890.LoadSubtitle(sub, null, fileName);
                                format = cavena890;
                            }
                        }
                        if (format == null)
                        {
                            var spt = new Spt();
                            if (spt.IsMine(null, fileName))
                            {
                                spt.LoadSubtitle(sub, null, fileName);
                                format = spt;
                            }
                        }
                        if (format == null)
                        {
                            var cheetahCaption = new CheetahCaption();
                            if (cheetahCaption.IsMine(null, fileName))
                            {
                                cheetahCaption.LoadSubtitle(sub, null, fileName);
                                format = cheetahCaption;
                            }
                        }
                        if (format == null)
                        {
                            var chk = new Chk();
                            if (chk.IsMine(null, fileName))
                            {
                                chk.LoadSubtitle(sub, null, fileName);
                                format = chk;
                            }
                        }
                        if (format == null)
                        {
                            var ayato = new Ayato();
                            if (ayato.IsMine(null, fileName))
                            {
                                ayato.LoadSubtitle(sub, null, fileName);
                                format = ayato;
                            }
                        }
                        if (format == null)
                        {
                            var capMakerPlus = new CapMakerPlus();
                            if (capMakerPlus.IsMine(null, fileName))
                            {
                                capMakerPlus.LoadSubtitle(sub, null, fileName);
                                format = capMakerPlus;
                            }
                        }
                        if (format == null)
                        {
                            var captionate = new Captionate();
                            if (captionate.IsMine(null, fileName))
                            {
                                captionate.LoadSubtitle(sub, null, fileName);
                                format = captionate;
                            }
                        }
                        if (format == null)
                        {
                            var ultech130 = new Ultech130();
                            if (ultech130.IsMine(null, fileName))
                            {
                                ultech130.LoadSubtitle(sub, null, fileName);
                                format = ultech130;
                            }
                        }
                        if (format == null)
                        {
                            var nciCaption = new NciCaption();
                            if (nciCaption.IsMine(null, fileName))
                            {
                                nciCaption.LoadSubtitle(sub, null, fileName);
                                format = nciCaption;
                            }
                        }
                        if (format == null)
                        {
                            var avidStl = new AvidStl();
                            if (avidStl.IsMine(null, fileName))
                            {
                                avidStl.LoadSubtitle(sub, null, fileName);
                                format = avidStl;
                            }
                        }
                        if (format == null)
                        {
                            var elr = new ELRStudioClosedCaption();
                            if (elr.IsMine(null, fileName))
                            {
                                elr.LoadSubtitle(sub, null, fileName);
                                format = elr;
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
                                    sub.Paragraphs.RemoveAt(0);
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
                    else if (format == null && (fileName.EndsWith(".mkv", StringComparison.OrdinalIgnoreCase) || fileName.EndsWith(".mks", StringComparison.OrdinalIgnoreCase)) && item.SubItems[2].Text.StartsWith("Matroska"))
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
                                    foreach (var track in matroska.GetTracks(true))
                                    {
                                        if (track.CodecId.Equals("S_VOBSUB", StringComparison.OrdinalIgnoreCase))
                                        {
                                            // TODO: Convert from VobSub image based format!
                                        }
                                        else if (track.CodecId.Equals("S_HDMV/PGS", StringComparison.OrdinalIgnoreCase))
                                        {
                                            // TODO: Convert from Blu-ray image based format!
                                        }
                                        else if (track.CodecId.Equals("S_TEXT/UTF8", StringComparison.OrdinalIgnoreCase) || track.CodecId.Equals("S_TEXT/SSA", StringComparison.OrdinalIgnoreCase) || track.CodecId.Equals("S_TEXT/ASS", StringComparison.OrdinalIgnoreCase))
                                        {
                                            var mkvSub = matroska.GetSubtitle(track.TrackNumber, null);
                                            Utilities.LoadMatroskaTextSubtitle(track, matroska, mkvSub, sub);
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                        else if (bluRaySubtitles.Count > 0)
                        {
                            item.SubItems[3].Text = Configuration.Settings.Language.BatchConvert.Ocr;
                            using (var vobSubOcr = new VobSubOcr())
                            {
                                vobSubOcr.FileName = Path.GetFileName(fileName);
                                vobSubOcr.InitializeBatch(bluRaySubtitles, Configuration.Settings.VobSubOcr, fileName);
                                sub = vobSubOcr.SubtitleFromOcr;
                            }
                        }
                        else if (isVobSub)
                        {
                            item.SubItems[3].Text = Configuration.Settings.Language.BatchConvert.Ocr;
                            using (var vobSubOcr = new VobSubOcr())
                            {
                                vobSubOcr.InitializeBatch(fileName, Configuration.Settings.VobSubOcr);
                                sub = vobSubOcr.SubtitleFromOcr;
                            }
                        }
                        if (comboBoxSubtitleFormats.Text == AdvancedSubStationAlpha.NameOfFormat && _assStyle != null)
                        {
                            if (!string.IsNullOrWhiteSpace(_assStyle))
                                sub.Header = _assStyle;
                        }
                        else if (comboBoxSubtitleFormats.Text == SubStationAlpha.NameOfFormat && _ssaStyle != null)
                        {
                            if (!string.IsNullOrWhiteSpace(_ssaStyle))
                                sub.Header = _ssaStyle;
                        }

                        bool skip = CheckSkipFilter(fileName, format, sub);
                        if (skip)
                        {
                            item.SubItems[3].Text = Configuration.Settings.Language.BatchConvert.FilterSkipped;
                        }
                        else
                        {
                            Paragraph prev = sub.Paragraphs[0];
                            bool first = true;
                            foreach (Paragraph p in sub.Paragraphs)
                            {
                                if (checkBoxRemoveTextForHI.Checked)
                                {
                                    p.Text = _removeTextForHearingImpaired.RemoveTextFromHearImpaired(p.Text);
                                }
                                if (checkBoxRemoveFormatting.Checked)
                                {
                                    p.Text = HtmlUtil.RemoveHtmlTags(p.Text, true);
                                }

                                if (!numericUpDownPercent.Value.Equals(100))
                                {
                                    double toSpeedPercentage = Convert.ToDouble(numericUpDownPercent.Value) / 100.0;
                                    p.StartTime.TotalMilliseconds = p.StartTime.TotalMilliseconds * toSpeedPercentage;
                                    p.EndTime.TotalMilliseconds = p.EndTime.TotalMilliseconds * toSpeedPercentage;

                                    if (first)
                                    {
                                        first = false;
                                    }
                                    else
                                    {
                                        if (prev.EndTime.TotalMilliseconds >= p.StartTime.TotalMilliseconds)
                                            prev.EndTime.TotalMilliseconds = p.StartTime.TotalMilliseconds - 1;
                                    }
                                }
                                prev = p;
                            }
                            sub.RemoveEmptyLines();
                            if (checkBoxFixCasing.Checked)
                            {
                                _changeCasing.FixCasing(sub, LanguageAutoDetect.AutoDetectGoogleLanguage(sub));
                                _changeCasingNames.Initialize(sub);
                                _changeCasingNames.FixCasing();
                            }
                            double fromFrameRate;
                            double toFrameRate;
                            if (double.TryParse(comboBoxFrameRateFrom.Text.Replace(',', '.').Replace(CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator, "."), NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out fromFrameRate) &&
                            double.TryParse(comboBoxFrameRateTo.Text.Replace(',', '.').Replace(CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator, "."), NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out toFrameRate))
                            {
                                sub.ChangeFrameRate(fromFrameRate, toFrameRate);
                            }
                            if (timeUpDownAdjust.TimeCode.TotalMilliseconds > 0.00001)
                            {
                                var totalMilliseconds = timeUpDownAdjust.TimeCode.TotalMilliseconds;
                                if (radioButtonShowEarlier.Checked)
                                    totalMilliseconds *= -1;
                                sub.AddTimeToAllParagraphs(TimeSpan.FromMilliseconds(totalMilliseconds));
                            }
                            while (worker1.IsBusy && worker2.IsBusy && worker3.IsBusy)
                            {
                                Application.DoEvents();
                                System.Threading.Thread.Sleep(100);
                            }
                            var parameter = new ThreadDoWorkParameter(checkBoxFixCommonErrors.Checked, checkBoxMultipleReplace.Checked, checkBoxSplitLongLines.Checked, checkBoxAutoBalance.Checked, checkBoxSetMinimumDisplayTimeBetweenSubs.Checked, item, sub, GetCurrentSubtitleFormat(), GetCurrentEncoding(), Configuration.Settings.Tools.BatchConvertLanguage, fileName, toFormat, format);
                            if (!worker1.IsBusy)
                                worker1.RunWorkerAsync(parameter);
                            else if (!worker2.IsBusy)
                                worker2.RunWorkerAsync(parameter);
                            else if (!worker3.IsBusy)
                                worker3.RunWorkerAsync(parameter);
                        }
                    }
                }
                catch
                {
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
            _converting = false;
            labelStatus.Text = string.Empty;
            progressBar1.Visible = false;
            TaskbarList.SetProgressState(Handle, TaskbarButtonProgressFlags.NoProgress);
            buttonConvert.Enabled = true;
            buttonCancel.Enabled = true;
            groupBoxOutput.Enabled = true;
            groupBoxConvertOptions.Enabled = true;
            buttonInputBrowse.Enabled = true;
            buttonSearchFolder.Enabled = true;
            comboBoxFilter.Enabled = true;
            textBoxFilter.Enabled = true;
        }

        private bool CheckSkipFilter(string fileName, SubtitleFormat format, Subtitle sub)
        {
            bool skip = false;
            if (comboBoxFilter.SelectedIndex == 1)
            {
                if (format != null && format.GetType() == typeof(SubRip) && FileUtil.HasUtf8Bom(fileName))
                    skip = true;
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
                progressBar1.Value++;
            TaskbarList.SetProgressValue(Handle, progressBar1.Value, progressBar1.Maximum);
            labelStatus.Text = progressBar1.Value + " / " + progressBar1.Maximum;
        }

        private static void DoThreadWork(object sender, DoWorkEventArgs e)
        {
            var p = (ThreadDoWorkParameter)e.Argument;
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
                        paragraph.Text = Utilities.AutoBreakLine(paragraph.Text);
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

            // always re-number
            p.Subtitle.Renumber();

            e.Result = p;
        }

        private void ThreadWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            var p = (ThreadDoWorkParameter)e.Result;
            if (p.Item.Index + 2 < listViewInputFiles.Items.Count)
                listViewInputFiles.EnsureVisible(p.Item.Index + 2);
            else
                listViewInputFiles.EnsureVisible(p.Item.Index);
            if (!string.IsNullOrEmpty(p.Error))
            {
                p.Item.SubItems[3].Text = p.Error;
            }
            else
            {
                if (p.SourceFormat == null)
                    p.SourceFormat = new SubRip();
                if (p.ToFormat == Ebu.NameOfFormat)
                    p.Subtitle.Header = _ebuGeneralInformation.ToString();

                bool success;
                var targetFormat = p.ToFormat;
                if (targetFormat == Configuration.Settings.Language.ExportCustomText.Title)
                    targetFormat = "CustomText:" + _customTextTemplate;
                if (checkBoxOverwriteOriginalFiles.Checked)
                {
                    success = CommandLineConvert.BatchConvertSave(targetFormat, null, GetCurrentEncoding(), Path.GetDirectoryName(p.FileName), _count, ref _converted, ref _errors, _allFormats, p.FileName, p.Subtitle, p.SourceFormat, true, -1, null, null, false, false, false);
                }
                else
                {
                    success = CommandLineConvert.BatchConvertSave(targetFormat, null, GetCurrentEncoding(), textBoxOutputFolder.Text, _count, ref _converted, ref _errors, _allFormats, p.FileName, p.Subtitle, p.SourceFormat, checkBoxOverwrite.Checked, -1, null, null, false, false, false);
                }
                if (success)
                {
                    p.Item.SubItems[3].Text = Configuration.Settings.Language.BatchConvert.Converted;
                }
                else
                {
                    p.Item.SubItems[3].Text = Configuration.Settings.Language.BatchConvert.NotConverted;
                }
                IncrementAndShowProgress();
                if (progressBar1.Value == progressBar1.Maximum)
                    labelStatus.Text = string.Empty;
            }
        }

        private void ComboBoxSubtitleFormatsSelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBoxSubtitleFormats.Text == AdvancedSubStationAlpha.NameOfFormat || comboBoxSubtitleFormats.Text == SubStationAlpha.NameOfFormat)
            {
                buttonStyles.Text = Configuration.Settings.Language.BatchConvert.Style;
                buttonStyles.Visible = true;
                comboBoxEncoding.Enabled = true;
            }
            else if (comboBoxSubtitleFormats.Text == Ebu.NameOfFormat)
            {
                buttonStyles.Text = Configuration.Settings.Language.BatchConvert.Settings;
                buttonStyles.Visible = true;
                if (_ebuGeneralInformation == null)
                    _ebuGeneralInformation = new Ebu.EbuGeneralSubtitleInformation();
                comboBoxEncoding.Enabled = true;
            }
            else if (comboBoxSubtitleFormats.Text == BluRaySubtitle)
            {
                buttonStyles.Text = Configuration.Settings.Language.BatchConvert.Settings;
                buttonStyles.Visible = true;
                comboBoxEncoding.Enabled = false;
            }
            else if (comboBoxSubtitleFormats.Text == VobSubSubtitle)
            {
                buttonStyles.Text = Configuration.Settings.Language.BatchConvert.Settings;
                buttonStyles.Visible = true;
                comboBoxEncoding.Enabled = false;
            }
            else if (comboBoxSubtitleFormats.Text == Configuration.Settings.Language.ExportCustomText.Title)
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
                ShowBluraySettings();
            }
            else if (comboBoxSubtitleFormats.Text == VobSubSubtitle)
            {
                VobSubSettings();
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
                    _customTextTemplate = properties.CurrentFormatName;
            }
        }

        private void ShowBluraySettings()
        {
            using (var properties = new ExportPngXml())
            {
                var s = new Subtitle();
                s.Paragraphs.Add(new Paragraph("Test 123." + Environment.NewLine + "Test 456.", 0, 4000));
                properties.Initialize(s, new SubRip(), "BLURAYSUP", null, null, null);
                properties.DisableSaveButtonAndCheckBoxes();
                properties.ShowDialog(this);
            }
        }

        private void VobSubSettings()
        {
            using (var properties = new ExportPngXml())
            {
                var s = new Subtitle();
                s.Paragraphs.Add(new Paragraph("Test 123." + Environment.NewLine + "Test 456.", 0, 4000));
                properties.Initialize(s, new SubRip(), "VOBSUB", null, null, null);
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
                System.Diagnostics.Process.Start(textBoxOutputFolder.Text);
            else
                MessageBox.Show(string.Format(Configuration.Settings.Language.SplitSubtitle.FolderNotFoundX, textBoxOutputFolder.Text));
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
                return;

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
                RemoveSelectedFiles();
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

            Configuration.Settings.Tools.BatchConvertFixCasing = checkBoxFixCasing.Checked;
            Configuration.Settings.Tools.BatchConvertFixCommonErrors = checkBoxFixCommonErrors.Checked;
            Configuration.Settings.Tools.BatchConvertMultipleReplace = checkBoxMultipleReplace.Checked;
            Configuration.Settings.Tools.BatchConvertSplitLongLines = checkBoxSplitLongLines.Checked;
            Configuration.Settings.Tools.BatchConvertAutoBalance = checkBoxAutoBalance.Checked;
            Configuration.Settings.Tools.BatchConvertRemoveFormatting = checkBoxRemoveFormatting.Checked;
            Configuration.Settings.Tools.BatchConvertRemoveTextForHI = checkBoxRemoveTextForHI.Checked;
            Configuration.Settings.Tools.BatchConvertSetMinDisplayTimeBetweenSubtitles = checkBoxSetMinimumDisplayTimeBetweenSubs.Checked;
            Configuration.Settings.Tools.BatchConvertOutputFolder = textBoxOutputFolder.Text;
            Configuration.Settings.Tools.BatchConvertOverwriteExisting = checkBoxOverwrite.Checked;
            Configuration.Settings.Tools.BatchConvertOverwriteOriginal = checkBoxOverwriteOriginalFiles.Checked;
            Configuration.Settings.Tools.BatchConvertFormat = comboBoxSubtitleFormats.SelectedItem.ToString();
            Configuration.Settings.Tools.BatchConvertAssStyles = _assStyle;
            Configuration.Settings.Tools.BatchConvertSsaStyles = _ssaStyle;
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
                _abort = false;

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
            foreach (string fileName in Directory.GetFiles(path))
            {
                try
                {
                    string ext = Path.GetExtension(fileName).ToLowerInvariant();
                    if (ext != ".png" && ext != ".jpg" && ext != ".dll" && ext != ".exe" && ext != ".zip")
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
                            if (fi.Length < 1024 * 1024) // max 1 mb
                            {
                                Encoding encoding;
                                var sub = new Subtitle();
                                var format = sub.LoadSubtitle(fileName, out encoding, null);
                                if (format == null)
                                {
                                    var ebu = new Ebu();
                                    if (ebu.IsMine(null, fileName))
                                        format = ebu;
                                }
                                if (format == null)
                                {
                                    var pac = new Pac();
                                    if (pac.IsMine(null, fileName))
                                        format = pac;
                                }
                                if (format == null)
                                {
                                    var cavena890 = new Cavena890();
                                    if (cavena890.IsMine(null, fileName))
                                        format = cavena890;
                                }
                                if (format == null)
                                {
                                    var spt = new Spt();
                                    if (spt.IsMine(null, fileName))
                                        format = spt;
                                }
                                if (format == null)
                                {
                                    var cheetahCaption = new CheetahCaption();
                                    if (cheetahCaption.IsMine(null, fileName))
                                        format = cheetahCaption;
                                }
                                if (format == null)
                                {
                                    var chk = new Chk();
                                    if (chk.IsMine(null, fileName))
                                        format = chk;
                                }
                                if (format == null)
                                {
                                    var ayato = new Ayato();
                                    if (ayato.IsMine(null, fileName))
                                        format = ayato;
                                }
                                if (format == null)
                                {
                                    var capMakerPlus = new CapMakerPlus();
                                    if (capMakerPlus.IsMine(null, fileName))
                                        format = capMakerPlus;
                                }
                                if (format == null)
                                {
                                    var captionate = new Captionate();
                                    if (captionate.IsMine(null, fileName))
                                        format = captionate;
                                }
                                if (format == null)
                                {
                                    var ultech130 = new Ultech130();
                                    if (ultech130.IsMine(null, fileName))
                                        format = ultech130;
                                }
                                if (format == null)
                                {
                                    var nciCaption = new NciCaption();
                                    if (nciCaption.IsMine(null, fileName))
                                        format = nciCaption;
                                }
                                if (format == null)
                                {
                                    var avidStl = new AvidStl();
                                    if (avidStl.IsMine(null, fileName))
                                        format = avidStl;
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
                            return;
                    }
                }
                catch
                {
                    // ignored
                }
            }
            if (checkBoxScanFolderRecursive.Checked)
            {
                foreach (string directory in Directory.GetDirectories(path))
                {
                    if (directory != "." && directory != "..")
                        SearchFolder(directory);
                    if (_abort)
                        return;
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
            if (!_converting)
            {
                if (e.KeyCode == Keys.Escape)
                {
                    Close();
                }
                else if (e.KeyData == (Keys.Control | Keys.O)) // Open file/s
                {
                    buttonInputBrowse_Click(null, EventArgs.Empty);
                }
            }
            else if (e.KeyCode == Keys.Escape)
            {
                _abort = true;
                e.SuppressKeyPress = true;
            }
        }

        private void buttonRemoveTextForHiSettings_Click(object sender, EventArgs e)
        {
            using (var form = new FormRemoveTextForHearImpaired(null))
            {
                form.InitializeSettingsOnly();
                form.ShowDialog(this);
            }
        }

        private void comboBoxFilter_SelectedIndexChanged(object sender, EventArgs e)
        {
            textBoxFilter.Visible = comboBoxFilter.SelectedIndex == 3;
        }

    }
}
