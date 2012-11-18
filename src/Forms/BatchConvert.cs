using System;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.SubtitleFormats;

namespace Nikse.SubtitleEdit.Forms
{
    public partial class BatchConvert : Form
    {
        string _assStyle;
        string _ssaStyle;
        FormRemoveTextForHearImpaired _removeForHI = new FormRemoveTextForHearImpaired();
        ChangeCasing _changeCasing = new ChangeCasing();
        ChangeCasingNames _changeCasingNames = new ChangeCasingNames();

        public BatchConvert()
        {
            InitializeComponent();
            Text = Configuration.Settings.Language.BatchConvert.Title;
            groupBoxInput.Text = Configuration.Settings.Language.BatchConvert.Input;
            groupBoxOutput.Text = Configuration.Settings.Language.BatchConvert.Output;

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
                            //     ebu.LoadSubtitle(sub, null, fileName);
                            format = ebu;
                        }
                    }
                    if (format == null)
                    {
                        var pac = new Pac();
                        if (pac.IsMine(null, fileName))
                        {
                            //     pac.LoadSubtitle(sub, null, fileName);
                            format = pac;
                        }
                    }
                    if (format == null)
                    {
                        var cavena890 = new Cavena890();
                        if (cavena890.IsMine(null, fileName))
                        {
                            //   cavena890.LoadSubtitle(sub, null, fileName);
                            format = cavena890;
                        }
                    }
                    if (format == null)
                    {
                        var spt = new Spt();
                        if (spt.IsMine(null, fileName))
                        {
                            // spt.LoadSubtitle(sub, null, fileName);
                            format = spt;
                        }
                    }
                    if (format == null)
                    {
                        var cheetahCaption = new CheetahCaption();
                        if (cheetahCaption.IsMine(null, fileName))
                        {
                            //       cheetahCaption.LoadSubtitle(_subtitle, null, fileName);
                            format = cheetahCaption;
                        }
                    }
                    if (format == null)
                    {
                        var capMakerPlus = new CapMakerPlus();
                        if (capMakerPlus.IsMine(null, fileName))
                        {
                            //      capMakerPlus.LoadSubtitle(_subtitle, null, fileName);
                            format = capMakerPlus;
                        }
                    }
                    if (format == null)
                    {
                        var captionate = new Captionate();
                        if (captionate.IsMine(null, fileName))
                        {
                            //           captionate.LoadSubtitle(_subtitle, null, fileName);
                            format = captionate;
                        }
                    }
                    if (format == null)
                    {
                        var ultech130 = new Ultech130();
                        if (ultech130.IsMine(null, fileName))
                        {
                            //                ultech130.LoadSubtitle(_subtitle, null, fileName);
                            format = ultech130;
                        }
                    }
                    if (format == null)
                    {
                        var nciCaption = new NciCaption();
                        if (nciCaption.IsMine(null, fileName))
                        {
                            //                 nciCaption.LoadSubtitle(_subtitle, null, fileName);
                            format = nciCaption;
                        }
                    }

                    if (format == null)
                    {
                        var avidStl = new AvidStl();
                        if (avidStl.IsMine(null, fileName))
                        {
                            //        avidStl.LoadSubtitle(_subtitle, null, fileName);
                            format = avidStl;
                        }
                    }

                }

                if (format == null)
                {
                    item.SubItems.Add("UNKNOWN SUBTITLE FORMAT!");
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
            if (e.Data.GetDataPresent(DataFormats.FileDrop, false))
                e.Effect = DragDropEffects.All;
        }

        private void listViewInputFiles_DragDrop(object sender, DragEventArgs e)
        {
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

        private void buttonConvert_Click(object sender, EventArgs e)
        {
            if (listViewInputFiles.Items.Count == 0)
            {
                MessageBox.Show("Nothing to convert");
                return;
            }
            else if (textBoxOutputFolder.Text.Length < 2)
            {
                MessageBox.Show("Please choose output folder");
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

            string toFormat = comboBoxSubtitleFormats.Text;
            int count = 0;
            int converted = 0;
            int errors = 0;
            var allFormats = SubtitleFormat.AllSubtitleFormats;
            foreach (ListViewItem item in listViewInputFiles.Items)
            {
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

                    if (format == null)
                    {

                    }
                    else
                    {
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
                            if (checkBoxBreakLongLines.Checked)
                            {
                                p.Text = Utilities.AutoBreakLine(p.Text);
                            }
                            if (checkBoxFixItalics.Checked)
                            {
                                p.Text = Utilities.FixInvalidItalicTags(p.Text);
                            }
                        }
                        if (checkBoxFixCasing.Checked)
                        {
                            _changeCasing.FixCasing(sub, Utilities.AutoDetectGoogleLanguage(sub));
                            _changeCasingNames.Initialize(sub);
                            _changeCasingNames.FixCasing();
                            //TODO:ChangeCasingNames!
                        }


                        int oldConverted = converted;
                        Main.BatchConvertSave(toFormat, null, GetCurrentEncoding(), textBoxOutputFolder.Text, count, ref converted, ref errors, allFormats, fileName, sub, format, checkBoxOverwrite.Checked);
                        if (converted == oldConverted + 1)
                        {
                            item.SubItems[3].Text = "Converted";
                        }
                        else if (converted > oldConverted + 1)
                        {
                            item.SubItems[3].Text = string.Format("Converted ({0})", converted - oldConverted);
                        }
                        else
                        {
                            item.SubItems[3].Text = "ERROR";
                        }

                    }
                }
                catch
                {
                }
            }
        }

        private void comboBoxSubtitleFormats_SelectedIndexChanged(object sender, EventArgs e)
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

        private void buttonStyles_Click(object sender, EventArgs e)
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
    }
}

