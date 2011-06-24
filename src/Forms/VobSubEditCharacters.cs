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
        bool _focusTextBox = false;

        public XmlDocument ImageCompareDocument
        {
            get
            {
                return _compareDoc;
            }
        }

        public VobSubEditCharacters(string databaseFolderName)
        {
            InitializeComponent();
            labelImageInfo.Text = string.Empty;
            pictureBox1.SizeMode = PictureBoxSizeMode.AutoSize;

            _directoryPath = Configuration.VobSubCompareFolder + databaseFolderName + Path.DirectorySeparatorChar;
            if (!File.Exists(_directoryPath + "CompareDescription.xml"))
                _compareDoc.LoadXml("<OcrBitmaps></OcrBitmaps>");
            else
                _compareDoc.Load(_directoryPath + "CompareDescription.xml");

            FillComboWithUniqueAndSortedTexts();

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

        private void ListBoxFileNamesSelectedIndexChanged(object sender, EventArgs e)
        {
            checkBoxItalic.Checked = _italics[listBoxFileNames.SelectedIndex];
            string fileName = _directoryPath + listBoxFileNames.SelectedItem;
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
        }

        private void VobSubEditCharacters_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
                DialogResult = DialogResult.Cancel;
        }

        private void ButtonUpdateClick(object sender, EventArgs e)
        {
            string target = listBoxFileNames.SelectedItem.ToString();
            target = target.Substring(0, target.Length - 4);
            XmlNode node = _compareDoc.DocumentElement.SelectSingleNode("FileName[.='" + target + "']");
            if (node != null)
            {
                string newText = textBoxText.Text;
                node.Attributes["Text"].InnerText = newText;

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

                FillComboWithUniqueAndSortedTexts();
                for (int i = 0; i < comboBoxTexts.Items.Count; i++)
                {
                    if (comboBoxTexts.Items[i].ToString() == newText)
                    {
                        comboBoxTexts.SelectedIndex = i;
                        for (int j = 0; j < listBoxFileNames.Items.Count; j++)
                        {
                            if (listBoxFileNames.Items[j].ToString().StartsWith(target))
                                listBoxFileNames.SelectedIndex = j;
                        }
                        return;
                    }
                }
            }
        }

        private void ButtonDeleteClick(object sender, EventArgs e)
        {
            int oldComboBoxIndex = comboBoxTexts.SelectedIndex;
            string target = listBoxFileNames.SelectedItem.ToString();
            target = target.Substring(0, target.Length - 4);
            XmlNode node = _compareDoc.DocumentElement.SelectSingleNode("FileName[.='" + target + "']");
            if (node != null)
            {
                _compareDoc.DocumentElement.RemoveChild(node);
                FillComboWithUniqueAndSortedTexts();
                if (oldComboBoxIndex < comboBoxTexts.Items.Count)
                    comboBoxTexts.SelectedIndex = oldComboBoxIndex;
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
                            if (listBoxFileNames.Items[j].ToString().StartsWith(name))
                                listBoxFileNames.SelectedIndex = j;
                        }
                        _focusTextBox = true;
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
                DialogResult = DialogResult.OK;
        }
    }
}