using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.Xml;
using Nikse.SubtitleEdit.Logic;

namespace Nikse.SubtitleEdit.Forms
{
    public sealed partial class VobSubEditCharacters : Form
    {
        XmlDocument _compareDoc = new XmlDocument();
        string _directoryPath;
        List<bool> _italics = new List<bool>();
        internal List<VobSubOcr.ImageCompareAddition> Additions { get; private set; }

        public XmlDocument ImageCompareDocument
        {
            get
            {
                return _compareDoc;
            }
        }

        internal VobSubEditCharacters(string databaseFolderName, List<VobSubOcr.ImageCompareAddition> additions)
        {
            InitializeComponent();
            if (additions != null)
            {
                Additions = new List<VobSubOcr.ImageCompareAddition>();
                foreach (var a in additions)
                    Additions.Add(a);

                int makeHigher = 40;
                labelImageCompareFiles.Top = labelImageCompareFiles.Top - makeHigher;
                listBoxFileNames.Top = listBoxFileNames.Top - makeHigher;
                listBoxFileNames.Height = listBoxFileNames.Height + makeHigher;
                groupBoxCurrentCompareImage.Top = groupBoxCurrentCompareImage.Top - makeHigher;
                groupBoxCurrentCompareImage.Height = groupBoxCurrentCompareImage.Height + makeHigher;
            }

            labelImageInfo.Text = string.Empty;
            pictureBox1.SizeMode = PictureBoxSizeMode.AutoSize;

            _directoryPath = Configuration.VobSubCompareFolder + databaseFolderName + Path.DirectorySeparatorChar;
            if (!File.Exists(_directoryPath + "CompareDescription.xml"))
                _compareDoc.LoadXml("<OcrBitmaps></OcrBitmaps>");
            else
                _compareDoc.Load(_directoryPath + "CompareDescription.xml");

            Refill(Additions);

            Text = Configuration.Settings.Language.VobSubEditCharacters.Title;
            labelChooseCharacters.Text = Configuration.Settings.Language.VobSubEditCharacters.ChooseCharacter;
            labelImageCompareFiles.Text = Configuration.Settings.Language.VobSubEditCharacters.ImageCompareFiles;
            groupBoxCurrentCompareImage.Text = Configuration.Settings.Language.VobSubEditCharacters.CurrentCompareImage;
            labelTextAssociatedWithImage.Text = Configuration.Settings.Language.VobSubEditCharacters.TextAssociatedWithImage;
            checkBoxItalic.Text = Configuration.Settings.Language.VobSubEditCharacters.IsItalic;
            buttonUpdate.Text = Configuration.Settings.Language.VobSubEditCharacters.Update;
            buttonDelete.Text = Configuration.Settings.Language.VobSubEditCharacters.Delete;
            labelDoubleSize.Text = Configuration.Settings.Language.VobSubEditCharacters.ImageDoubleSize;
            buttonOK.Text = Configuration.Settings.Language.General.OK;
            buttonCancel.Text = Configuration.Settings.Language.General.Cancel;
            FixLargeFonts();
        }

        private void Refill(List<VobSubOcr.ImageCompareAddition> additions)
        {
            if (additions != null && additions.Count > 0)
            {
                labelChooseCharacters.Visible = false;
                comboBoxTexts.Visible = false;
                FillLastAdditions(additions);
            }
            else
            {
                FillComboWithUniqueAndSortedTexts();
            }
        }

        private void FillLastAdditions(List<VobSubOcr.ImageCompareAddition> additions)
        {
            _italics = new List<bool>();
            listBoxFileNames.Items.Clear();
            foreach (XmlNode node in _compareDoc.DocumentElement.ChildNodes)
            {
                if (node.Attributes["Text"] != null)
                {
                    string text = node.Attributes["Text"].InnerText;
                    string name = node.InnerText;
                    foreach (VobSubOcr.ImageCompareAddition a in additions)
                    {
                        if (name == a.Name)
                        {
                            listBoxFileNames.Items.Add("[" + text +"] " + node.InnerText + ".bmp");
                            _italics.Add(node.Attributes["Italic"] != null);
                        }

                    }
                }
            }

            if (listBoxFileNames.Items.Count > 0)
                listBoxFileNames.SelectedIndex = 0;
        }

        private void FixLargeFonts()
        {
            Graphics graphics = this.CreateGraphics();
            SizeF textSize = graphics.MeasureString(buttonOK.Text, this.Font);
            if (textSize.Height > buttonOK.Height - 4)
            {
                int newButtonHeight = (int)(textSize.Height + 7 + 0.5);
                Utilities.SetButtonHeight(this, newButtonHeight, 1);
            }
        }

        private void FillComboWithUniqueAndSortedTexts()
        {
            List<string> texts = new List<string>();
            foreach (XmlNode node in _compareDoc.DocumentElement.SelectNodes("FileName"))
            {
                if (node.Attributes.Count >= 1)
                {
                    string text = node.Attributes["Text"].InnerText;
                    if (!texts.Contains(text))
                        texts.Add(text);
                }
            }
            texts.Sort();

            comboBoxTexts.Items.Clear();
            foreach (string text in texts)
            {
                comboBoxTexts.Items.Add(text);
            }

            if (comboBoxTexts.Items.Count > 0)
                comboBoxTexts.SelectedIndex = 0;
        }

        private void ComboBoxTextsSelectedIndexChanged(object sender, EventArgs e)
        {
            _italics = new List<bool>();
            string target = comboBoxTexts.SelectedItem.ToString();
            textBoxText.Text = target;
            listBoxFileNames.Items.Clear();
            foreach (XmlNode node in _compareDoc.DocumentElement.ChildNodes)
            {
                if (node.Attributes["Text"] != null)
                {
                    string text = node.Attributes["Text"].InnerText;
                    if (text == target)
                    {
                        listBoxFileNames.Items.Add(node.InnerText + ".bmp");
                        _italics.Add(node.Attributes["Italic"] != null);
                    }
                }
            }

            if (listBoxFileNames.Items.Count > 0)
                listBoxFileNames.SelectedIndex = 0;
        }

        private string GetSelectedFileName()
        {
            string fileName = listBoxFileNames.SelectedItem.ToString();
            if (fileName.StartsWith("["))
                fileName = fileName.Substring(fileName.IndexOf("]") + 1);
            return fileName.Trim();
        }

        private string GetFileName(int index)
        {
            string fileName = listBoxFileNames.Items[index].ToString();
            if (fileName.StartsWith("["))
                fileName = fileName.Substring(fileName.IndexOf("]") + 1);
            return fileName.Trim();
        }

        private void ListBoxFileNamesSelectedIndexChanged(object sender, EventArgs e)
        {
            checkBoxItalic.Checked = _italics[listBoxFileNames.SelectedIndex];
            string fileName = _directoryPath + GetSelectedFileName();
            Bitmap bmp;
            if (File.Exists(fileName))
            {
                Bitmap tmp = new Bitmap(fileName);
                try
                {
                    labelImageInfo.Text = string.Format(Configuration.Settings.Language.VobSubEditCharacters.Image + " - {0}x{1}", tmp.Width, tmp.Height);
                }
                catch
                {
                }
                bmp = new Bitmap(tmp);
                tmp.Dispose();
            }
            else
            {
                bmp = new Bitmap(1,1);
                labelImageInfo.Text = Configuration.Settings.Language.VobSubEditCharacters.ImageFileNotFound;
            }
            pictureBox1.Image = bmp;
            pictureBox2.Width = bmp.Width * 2;
            pictureBox2.Height = bmp.Height * 2;
            pictureBox2.Image = bmp;

            if (Additions != null && Additions.Count > 0)
            {
                string target = GetSelectedFileName();
                foreach (var a in Additions)
                {
                    if (target.StartsWith(a.Name))
                    {
                        textBoxText.Text = a.Text;
                        break;
                    }
                }
            }
        }

        private void VobSubEditCharacters_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
                DialogResult = DialogResult.Cancel;
        }

        private void ButtonUpdateClick(object sender, EventArgs e)
        {
            if (listBoxFileNames.Items.Count == 0)
                return;

            string target = GetSelectedFileName();
            target = target.Substring(0, target.Length - 4);
            XmlNode node = _compareDoc.DocumentElement.SelectSingleNode("FileName[.='" + target + "']");
            if (node != null)
            {
                string newText = textBoxText.Text;
                node.Attributes["Text"].InnerText = newText;

                if (Additions != null && Additions.Count > 0)
                {
                    foreach (var a in Additions)
                    {
                        if (target.StartsWith(a.Name))
                        {
                            a.Text = newText;
                            a.Italic = checkBoxItalic.Checked;
                            break;
                        }
                    }
                }

                if (checkBoxItalic.Checked)
                {
                    if (node.Attributes["Italic"] == null)
                    {
                        XmlAttribute italic = node.OwnerDocument.CreateAttribute("Italic");
                        italic.InnerText = "true";
                        node.Attributes.Append(italic);
                    }
                }
                else
                {
                    if (node.Attributes["Italic"] != null)
                    {
                        node.Attributes.RemoveNamedItem("Italic");
                    }
                }

                Refill(Additions);
                if (Additions == null || Additions.Count == 0)
                {
                    for (int i = 0; i < comboBoxTexts.Items.Count; i++)
                    {
                        if (comboBoxTexts.Items[i].ToString() == newText)
                        {
                            comboBoxTexts.SelectedIndex = i;
                            for (int j = 0; j < listBoxFileNames.Items.Count; j++)
                            {
                                if (GetFileName(j).StartsWith(target))
                                    listBoxFileNames.SelectedIndex = j;
                            }
                            return;
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < listBoxFileNames.Items.Count; i++)
                    {
                        if (listBoxFileNames.Items[i].ToString().Contains(target))
                        {
                            listBoxFileNames.SelectedIndex = i;
                            return;
                        }
                    }
                }
            }
        }

        private void ButtonDeleteClick(object sender, EventArgs e)
        {
            if (listBoxFileNames.Items.Count == 0)
                return;

            int oldComboBoxIndex = comboBoxTexts.SelectedIndex;
            string target = GetSelectedFileName();
            target = target.Substring(0, target.Length - 4);
            XmlNode node = _compareDoc.DocumentElement.SelectSingleNode("FileName[.='" + target + "']");
            if (node != null)
            {
                _compareDoc.DocumentElement.RemoveChild(node);
                if (Additions != null && Additions.Count > 0)
                {
                    for (int i = Additions.Count - 1; i >= 0; i--)
                    {
                        if (Additions[i].Name == target)
                        {
                            Additions.RemoveAt(i);
                            Refill(Additions);
                            break;
                        }
                    }
                }

                Refill(Additions);
                if (Additions == null || Additions.Count == 0)
                {
                    if (oldComboBoxIndex < comboBoxTexts.Items.Count)
                        comboBoxTexts.SelectedIndex = oldComboBoxIndex;
                }
            }
        }

        internal void Initialize(string name, string text)
        {
            if (name != null && text != null)
            {
                for (int i = 0; i < comboBoxTexts.Items.Count; i++)
                {
                    if (comboBoxTexts.Items[i].ToString() == text)
                    {
                        comboBoxTexts.SelectedIndex = i;
                        for (int j = 0; j < listBoxFileNames.Items.Count; j++)
                        {
                            if (GetFileName(j).StartsWith(name))
                                listBoxFileNames.SelectedIndex = j;
                        }
                        return;
                    }
                }

            }
        }

        private void VobSubEditCharacters_Shown(object sender, EventArgs e)
        {
            textBoxText.Focus();
        }

        private void textBoxText_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                ButtonUpdateClick(null, null);
                DialogResult = DialogResult.OK;
            }
        }

    }
}