using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.SubtitleFormats;
using Nikse.SubtitleEdit.Logic.BluRaySup;

namespace Nikse.SubtitleEdit.Forms
{
    public partial class BatchConvert : Form
    {
        public class ThreadDoWorkParameter
        {
            public bool FixCommonErrors { get; set; }
            public bool MultipleReplaceActive { get; set; }
            public ListViewItem Item { get; set; }
            public Subtitle Subtitle { get; set; }
            public SubtitleFormat Format { get; set; }
            public Encoding Encoding { get; set; }
            public string Language { get; set; }
            public string Error { get; set; }
            public string FileName { get; set; }
            public string ToFormat { get; set; }
            public SubtitleFormat SourceFormat { get; set; } 

            public ThreadDoWorkParameter(bool fixCommonErrors, bool multipleReplace, ListViewItem item, Subtitle subtitle, SubtitleFormat format, Encoding encoding, string language, string fileName, string toFormat, SubtitleFormat sourceFormat)
            {
                FixCommonErrors = fixCommonErrors;
                MultipleReplaceActive = multipleReplace;
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

        string _assStyle;
        string _ssaStyle;
        FormRemoveTextForHearImpaired _removeForHI = new FormRemoveTextForHearImpaired();
        ChangeCasing _changeCasing = new ChangeCasing();
        ChangeCasingNames _changeCasingNames = new ChangeCasingNames();        
        bool _converting = false;
        int _count = 0;
        int _converted = 0;
        int _errors = 0;
        IList<SubtitleFormat> _allFormats = SubtitleFormat.AllSubtitleFormats;
        bool _abort = false;

        public BatchConvert(Icon icon)
        {
            InitializeComponent();
            this.Icon = (Icon)icon.Clone();

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
            checkBoxFixCasing.Text = l.ReDoCasing;
            checkBoxRemoveTextForHI.Text = l.RemoveTextForHI;
            columnHeaderFName.Text = Configuration.Settings.Language.JoinSubtitles.FileName;
            columnHeaderFormat.Text = Configuration.Settings.Language.Main.Controls.SubtitleFormat;
            columnHeaderSize.Text = Configuration.Settings.Language.General.Size;
            columnHeaderStatus.Text = l.Status;
            linkLabelOpenOutputFolder.Text = Configuration.Settings.Language.Main.Menu.File.Open;
            buttonConvert.Text = l.Convert;
            buttonCancel.Text = Configuration.Settings.Language.General.OK;

            groupBoxChangeFrameRate.Text = Configuration.Settings.Language.ChangeFrameRate.Title;
            groupBoxOffsetTimeCodes.Text = Configuration.Settings.Language.ShowEarlierLater.TitleAll;
            labelFromFrameRate.Text = Configuration.Settings.Language.ChangeFrameRate.FromFrameRate;
            labelToFrameRate.Text = Configuration.Settings.Language.ChangeFrameRate.ToFrameRate;
            labelHoursMinSecsMilliSecs.Text = Configuration.Settings.Language.General.HourMinutesSecondsMilliseconds;

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

            comboBoxFrameRateFrom.Items.Add((23.976).ToString());
            comboBoxFrameRateFrom.Items.Add((24.0).ToString());
            comboBoxFrameRateFrom.Items.Add((25.0).ToString());
            comboBoxFrameRateFrom.Items.Add((29.97).ToString());

            comboBoxFrameRateTo.Items.Add((23.976).ToString());
            comboBoxFrameRateTo.Items.Add((24.0).ToString());
            comboBoxFrameRateTo.Items.Add((25.0).ToString());
            comboBoxFrameRateTo.Items.Add((29.97).ToString());


            FixLargeFonts();

            foreach (SubtitleFormat f in SubtitleFormat.AllSubtitleFormats)
            {
                if (!f.IsVobSubIndexFile)
                    comboBoxSubtitleFormats.Items.Add(f.Name);

            }
            comboBoxSubtitleFormats.SelectedIndex = 0;

            comboBoxEncoding.Items.Clear();
            int encodingSelectedIndex = 0;
            comboBoxEncoding.Items.Add(Encoding.UTF8.EncodingName);
            foreach (EncodingInfo ei in Encoding.GetEncodings())
            {
                if (ei.Name != Encoding.UTF8.BodyName && ei.CodePage >= 949 && !ei.DisplayName.Contains("EBCDIC") && ei.CodePage != 1047)
                {
                    comboBoxEncoding.Items.Add(ei.CodePage + ": " + ei.DisplayName);
                    if (ei.Name == Configuration.Settings.General.DefaultEncoding)
                        encodingSelectedIndex = comboBoxEncoding.Items.Count - 1;
                }
            }
            comboBoxEncoding.SelectedIndex = encodingSelectedIndex;

            if (string.IsNullOrEmpty(Configuration.Settings.Tools.BatchConvertOutputFolder) || !System.IO.Directory.Exists(Configuration.Settings.Tools.BatchConvertOutputFolder))
                textBoxOutputFolder.Text = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            else
                textBoxOutputFolder.Text = Configuration.Settings.Tools.BatchConvertOutputFolder;
            checkBoxOverwrite.Checked = Configuration.Settings.Tools.BatchConvertOverwrite;
            checkBoxFixCasing.Checked = Configuration.Settings.Tools.BatchConvertFixCasing;
            checkBoxFixCommonErrors.Checked = Configuration.Settings.Tools.BatchConvertFixCommonErrors;
            checkBoxMultipleReplace.Checked = Configuration.Settings.Tools.BatchConvertMultipleReplace;
            checkBoxRemoveFormatting.Checked = Configuration.Settings.Tools.BatchConvertRemoveFormatting;
            checkBoxRemoveTextForHI.Checked = Configuration.Settings.Tools.BatchConvertRemoveTextForHI;
            if (!string.IsNullOrEmpty(Configuration.Settings.Language.BatchConvert.Settings))
                buttonFixCommonErrorSettings.Text = Configuration.Settings.Language.BatchConvert.Settings;
            checkBoxFixCommonErrors.Text = Configuration.Settings.Language.FixCommonErrors.Title;
            checkBoxMultipleReplace.Text = Configuration.Settings.Language.MultipleReplace.Title;
            radioButtonShowEarlier.Text = Configuration.Settings.Language.ShowEarlierLater.ShowEarlier;
            radioButtonShowLater.Text = Configuration.Settings.Language.ShowEarlierLater.ShowLater;
        }

        private void FixLargeFonts()
        {
            Graphics graphics = this.CreateGraphics();
            SizeF textSize = graphics.MeasureString(buttonCancel.Text, this.Font);
            if (textSize.Height > buttonCancel.Height - 4)
            {
                int newButtonHeight = (int)(textSize.Height + 7 + 0.5);
                Utilities.SetButtonHeight(this, newButtonHeight, 1);
            }
        }

        private void buttonChooseFolder_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                textBoxOutputFolder.Text = folderBrowserDialog1.SelectedPath;
            }
        }

        private void buttonOpenOutputFolder_Click(object sender, EventArgs e)
        {
            if (System.IO.Directory.Exists(textBoxOutputFolder.Text))
                System.Diagnostics.Process.Start(textBoxOutputFolder.Text);
            else
                MessageBox.Show(string.Format(Configuration.Settings.Language.SplitSubtitle.FolderNotFoundX, textBoxOutputFolder.Text));
        }

        private void buttonInputBrowse_Click(object sender, EventArgs e)
        {
            buttonInputBrowse.Enabled = false;
            openFileDialog1.Title = Configuration.Settings.Language.General.OpenSubtitle;
            openFileDialog1.FileName = string.Empty;
            openFileDialog1.Filter = Utilities.GetOpenDialogFilter();
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
                FileInfo fi = new FileInfo(fileName);
                var item = new ListViewItem(fileName);
                item.SubItems.Add(Utilities.FormatBytesToDisplayFileSize(fi.Length));

                SubtitleFormat format = null;
                Encoding encoding;
                var sub = new Subtitle();
                var _subtitle = new Subtitle();
                if (fi.Length < 1024 * 1024) // max 1 mb
                {
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

                }

                if (format == null)
                {
                    if (Main.IsBluRaySupFile(fileName))
                        item.SubItems.Add("Blu-ray");
                    else if (Main.HasVobSubHeader(fileName))
                        item.SubItems.Add("VobSub");
                    else
                        item.SubItems.Add(Configuration.Settings.Language.UnknownSubtitle.Title);
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

            string[] fileNames = (string[])e.Data.GetData(DataFormats.FileDrop);
            foreach (string fileName in fileNames)
            {
                AddInputFile(fileName);
            }
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            Configuration.Settings.Tools.BatchConvertOutputFolder = textBoxOutputFolder.Text;
            Configuration.Settings.Tools.BatchConvertOverwrite = checkBoxOverwrite.Checked;
            DialogResult = DialogResult.Cancel;
        }

        private Encoding GetCurrentEncoding()
        {
            if (comboBoxEncoding.Text == Encoding.UTF8.BodyName || comboBoxEncoding.Text == Encoding.UTF8.EncodingName || comboBoxEncoding.Text == "utf-8")
            {
                return Encoding.UTF8;
            }

            foreach (EncodingInfo ei in Encoding.GetEncodings())
            {
                if (ei.CodePage + ": " + ei.DisplayName == comboBoxEncoding.Text)
                    return ei.GetEncoding();
            }

            return Encoding.UTF8;
        }

        private SubtitleFormat GetCurrentSubtitleFormat()
        {
            return Utilities.GetSubtitleFormatByFriendlyName(comboBoxSubtitleFormats.SelectedItem.ToString());
        }

        private void buttonConvert_Click(object sender, EventArgs e)
        {
            if (listViewInputFiles.Items.Count == 0)
            {
                MessageBox.Show(Configuration.Settings.Language.BatchConvert.NothingToConvert);
                return;
            }
            else if (textBoxOutputFolder.Text.Length < 2)
            {
                MessageBox.Show(Configuration.Settings.Language.BatchConvert.PleaseChooseOutputFolder);
                return;
            }
            else if (!Directory.Exists(textBoxOutputFolder.Text))
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

            _converting = true;
            buttonConvert.Enabled = false;
            buttonCancel.Enabled = false;
            progressBar1.Maximum = listViewInputFiles.Items.Count;
            progressBar1.Value = 0;
            progressBar1.Visible = progressBar1.Maximum > 2;
            string toFormat = comboBoxSubtitleFormats.Text;
            groupBoxOutput.Enabled = false;
            groupBoxConvertOptions.Enabled = false;
            buttonInputBrowse.Enabled = false;
            _count = 0;
            _converted = 0;
            _errors = 0;
            _abort = false;

            BackgroundWorker worker1 = new BackgroundWorker();
            BackgroundWorker worker2 = new BackgroundWorker();
            BackgroundWorker worker3 = new BackgroundWorker();
            worker1.DoWork += new DoWorkEventHandler(DoThreadWork);
            worker1.RunWorkerCompleted += new RunWorkerCompletedEventHandler(ThreadWorkerCompleted);
            worker2.DoWork += new DoWorkEventHandler(DoThreadWork);
            worker2.RunWorkerCompleted += new RunWorkerCompletedEventHandler(ThreadWorkerCompleted);
            worker3.DoWork += new DoWorkEventHandler(DoThreadWork);
            worker3.RunWorkerCompleted += new RunWorkerCompletedEventHandler(ThreadWorkerCompleted);

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
                string friendlyName = item.SubItems[1].Text;

                try
                {
                    SubtitleFormat format = null;
                    Encoding encoding;
                    var sub = new Subtitle();
                    var fi = new FileInfo(fileName);
                    if (fi.Length < 1024 * 1024) // max 1 mb
                    {
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

                    }

                    List<Nikse.SubtitleEdit.Logic.BluRaySup.BluRaySupParser.PcsData> bluRaySubtitles = new List<Nikse.SubtitleEdit.Logic.BluRaySup.BluRaySupParser.PcsData>();
                    bool isVobSub = false;
                    if (format == null && fileName.ToLower().EndsWith(".sup") && Main.IsBluRaySupFile(fileName))
                    {
                        var log = new StringBuilder();
                        bluRaySubtitles = BluRaySupParser.ParseBluRaySup(fileName, log);
                    }
                    else if (format == null && fileName.ToLower().EndsWith(".sub") && Main.HasVobSubHeader(fileName))
                    {
                        isVobSub = true;
                    }



                    if (format == null && bluRaySubtitles.Count == 0 && !isVobSub)
                    {
                        if (progressBar1.Value < progressBar1.Maximum)
                            progressBar1.Value++;
                        labelStatus.Text = progressBar1.Value + " / " + progressBar1.Maximum;
                    }
                    else
                    {
                        if (bluRaySubtitles.Count > 0)
                        {
                            item.SubItems[3].Text = "OCR...";
                            var vobSubOcr = new VobSubOcr();
                            vobSubOcr.FileName = Path.GetFileName(fileName);
                            vobSubOcr.InitializeBatch(bluRaySubtitles, Configuration.Settings.VobSubOcr, fileName);
                            sub = vobSubOcr.SubtitleFromOcr;
                        }
                        else if (isVobSub)
                        {
                            item.SubItems[3].Text = "OCR...";
                            var vobSubOcr = new VobSubOcr();
                            vobSubOcr.InitializeBatch(fileName, Configuration.Settings.VobSubOcr, true);
                            sub = vobSubOcr.SubtitleFromOcr;
                        }

                        if (comboBoxSubtitleFormats.Text == new AdvancedSubStationAlpha().Name && _assStyle != null)
                        {
                            sub.Header = _assStyle;
                        }
                        else if (comboBoxSubtitleFormats.Text == new SubStationAlpha().Name && _ssaStyle != null)
                        {
                            sub.Header = _ssaStyle;
                        }

                        foreach (Paragraph p in sub.Paragraphs)
                        {
                            if (checkBoxRemoveTextForHI.Checked)
                            {
                                p.Text = _removeForHI.RemoveTextFromHearImpaired(p.Text);
                            }
                            if (checkBoxRemoveFormatting.Checked)
                            {
                                p.Text = Utilities.RemoveHtmlTags(p.Text);
                                if (p.Text.StartsWith("{") && p.Text.Length > 6 && p.Text[5] == '}')
                                    p.Text = p.Text.Remove(0, 6);
                                if (p.Text.StartsWith("{") && p.Text.Length > 6 && p.Text[4] == '}')
                                    p.Text = p.Text.Remove(0, 5);
                            }
                        }
                        if (checkBoxFixCasing.Checked)
                        {
                            _changeCasing.FixCasing(sub, Utilities.AutoDetectGoogleLanguage(sub));
                            _changeCasingNames.Initialize(sub);
                            _changeCasingNames.FixCasing();
                        }

                        double fromFrameRate;
                        double toFrameRate;
                        if (double.TryParse(comboBoxFrameRateFrom.Text, out fromFrameRate) &&
                            double.TryParse(comboBoxFrameRateTo.Text, out toFrameRate))
                        {
                            sub.ChangeFramerate(fromFrameRate, toFrameRate);
                        }

                        if (timeUpDownAdjust.TimeCode.Milliseconds > 0.00001)
                        {
                            var totalMilliseconds = timeUpDownAdjust.TimeCode.Milliseconds;
                            if (radioButtonShowEarlier.Checked)
                                totalMilliseconds *= -1;
                            sub.AddTimeToAllParagraphs(TimeSpan.FromMilliseconds(totalMilliseconds));
                        }

                        while (worker1.IsBusy && worker2.IsBusy && worker3.IsBusy)
                        {
                            Application.DoEvents();
                            System.Threading.Thread.Sleep(100);
                        }

                        ThreadDoWorkParameter parameter = new ThreadDoWorkParameter(checkBoxFixCommonErrors.Checked, checkBoxMultipleReplace.Checked, item, sub, GetCurrentSubtitleFormat(), GetCurrentEncoding(), Configuration.Settings.Tools.BatchConvertLanguage, fileName, toFormat, format);
                        if (!worker1.IsBusy)
                            worker1.RunWorkerAsync(parameter);
                        else if (!worker2.IsBusy)
                            worker2.RunWorkerAsync(parameter);
                        else if (!worker3.IsBusy)
                            worker3.RunWorkerAsync(parameter);
                    }
                    
                }
                catch
                {
                    if (progressBar1.Value < progressBar1.Maximum)
                        progressBar1.Value++;
                    labelStatus.Text = progressBar1.Value + " / " + progressBar1.Maximum;
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
                }
                System.Threading.Thread.Sleep(100);
            }
            _converting = false;
            labelStatus.Text = string.Empty;
            progressBar1.Visible = false;
            buttonConvert.Enabled = true;
            buttonCancel.Enabled = true;
            groupBoxOutput.Enabled = true;
            groupBoxConvertOptions.Enabled = true;
            buttonInputBrowse.Enabled = true;
        }

        void DoThreadWork(object sender, DoWorkEventArgs e)
        {
            ThreadDoWorkParameter p = (ThreadDoWorkParameter)e.Argument;
            if (p.FixCommonErrors)
            {
                try
                {
                    FixCommonErrors fixCommonErrors = new FixCommonErrors();
                    fixCommonErrors.RunBatch(p.Subtitle, p.Format, p.Encoding, Configuration.Settings.Tools.BatchConvertLanguage);
                    p.Subtitle = fixCommonErrors.FixedSubtitle;
                }
                catch (Exception exception)
                {
                    p.Error = "FCE ERROR: " + exception.Message;
                }
            }
            if (p.MultipleReplaceActive)
            {
                try
                {
                    var form = new MultipleReplace();
                    form.Initialize(p.Subtitle);
                    p.Subtitle = form.FixedSubtitle;
                }
                catch (Exception exception)
                {
                    p.Error = "MultipleReplace error: " + exception.Message;
                }
            }
            e.Result = p;
        }

        void ThreadWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            ThreadDoWorkParameter p = (ThreadDoWorkParameter)e.Result;
            if (!string.IsNullOrEmpty(p.Error))
            {
                p.Item.SubItems[3].Text = p.Error;
            }
            else
            {
                if (p.SourceFormat == null)
                    p.SourceFormat = new SubRip();

                bool success = Main.BatchConvertSave(p.ToFormat, null, GetCurrentEncoding(), textBoxOutputFolder.Text, _count, ref _converted, ref _errors, _allFormats, p.FileName, p.Subtitle, p.SourceFormat, checkBoxOverwrite.Checked);
                if (success)
                {
                    p.Item.SubItems[3].Text = Configuration.Settings.Language.BatchConvert.Converted;
                }
                else
                {
                    p.Item.SubItems[3].Text = "ERROR";
                }
                if (progressBar1.Value < progressBar1.Maximum)
                    progressBar1.Value++;
                labelStatus.Text = progressBar1.Value + " / " + progressBar1.Maximum;
                if (progressBar1.Value == progressBar1.Maximum)
                    labelStatus.Text = string.Empty;                
            }
        }

        private void ComboBoxSubtitleFormatsSelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBoxSubtitleFormats.Text == new AdvancedSubStationAlpha().Name || comboBoxSubtitleFormats.Text == new SubStationAlpha().Name)
            {
                buttonStyles.Visible = true;
            }
            else
            {
                buttonStyles.Visible = false;
            }
            _assStyle = null;
            _ssaStyle = null;
        }

        private void ButtonStylesClick(object sender, EventArgs e)
        {
            if (comboBoxSubtitleFormats.Text == new AdvancedSubStationAlpha().Name)
            {
                var sub = new Subtitle();
                var form = new SubStationAlphaStyles(sub, new AdvancedSubStationAlpha());
                form.MakeOnlyOneStyle();
                if (form.ShowDialog(this) == DialogResult.OK)
                {
                    _assStyle = form.Header;
                }
            }
            else if (comboBoxSubtitleFormats.Text == new SubStationAlpha().Name)
            {
                var sub = new Subtitle();
                var form = new SubStationAlphaStyles(sub, new SubStationAlpha());
                if (form.ShowDialog(this) == DialogResult.OK)
                {
                    _ssaStyle = form.Header;
                }
            }
        }

        private void LinkLabelOpenOutputFolderLinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (System.IO.Directory.Exists(textBoxOutputFolder.Text))
                System.Diagnostics.Process.Start(textBoxOutputFolder.Text);
            else
                MessageBox.Show(string.Format(Configuration.Settings.Language.SplitSubtitle.FolderNotFoundX, textBoxOutputFolder.Text));
        }

        private void ContextMenuStripFilesOpening(object sender, System.ComponentModel.CancelEventArgs e)
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

            for (int i = listViewInputFiles.SelectedIndices.Count-1; i>=0; i--)
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
            if (e.KeyCode == Keys.Escape)
                _abort = true;
        }

        private void buttonFixCommonErrorSettings_Click(object sender, EventArgs e)
        {
            var form = new FixCommonErrors();
            form.RunBatchSettings(new Subtitle(), GetCurrentSubtitleFormat(), GetCurrentEncoding(), Configuration.Settings.Tools.BatchConvertLanguage);
            form.ShowDialog(this);
            Configuration.Settings.Tools.BatchConvertLanguage = form.Language;
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
            Configuration.Settings.Tools.BatchConvertRemoveFormatting = checkBoxRemoveFormatting.Checked;
            Configuration.Settings.Tools.BatchConvertRemoveTextForHI = checkBoxRemoveTextForHI.Checked;
        }

        private void buttonMultipleReplaceSettings_Click(object sender, EventArgs e)
        {
            var form = new MultipleReplace();
            form.Initialize(new Subtitle());
            form.ShowDialog(this);
        }

    }
}

