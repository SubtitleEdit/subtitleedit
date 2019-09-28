using Nikse.SubtitleEdit.Core;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Ocr.Binary;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.Xml;

namespace Nikse.SubtitleEdit.Forms.Ocr
{
    public sealed partial class VobSubEditCharacters : Form
    {
        private readonly XmlDocument _compareDoc = new XmlDocument();
        private readonly string _directoryPath;
        private List<bool> _italics = new List<bool>();
        internal List<VobSubOcr.ImageCompareAddition> Additions { get; }
        private readonly BinaryOcrDb _binOcrDb;

        public XmlDocument ImageCompareDocument => _compareDoc;

        internal VobSubEditCharacters(string databaseFolderName, List<VobSubOcr.ImageCompareAddition> additions, BinaryOcrDb binOcrDb)
        {
            UiUtil.PreInitialize(this);
            InitializeComponent();
            UiUtil.FixFonts(this);

            labelExpandCount.Text = string.Empty;
            _binOcrDb = binOcrDb;
            labelCount.Text = string.Empty;
            if (additions != null)
            {
                Additions = new List<VobSubOcr.ImageCompareAddition>();
                foreach (var a in additions)
                {
                    Additions.Add(a);
                }

                const int makeHigher = 40;
                labelImageCompareFiles.Top = labelImageCompareFiles.Top - makeHigher;
                listBoxFileNames.Top = listBoxFileNames.Top - makeHigher;
                listBoxFileNames.Height = listBoxFileNames.Height + makeHigher;
                groupBoxCurrentCompareImage.Top = groupBoxCurrentCompareImage.Top - makeHigher;
                groupBoxCurrentCompareImage.Height = groupBoxCurrentCompareImage.Height + makeHigher;
            }

            labelImageInfo.Text = string.Empty;
            pictureBox1.SizeMode = PictureBoxSizeMode.AutoSize;

            _directoryPath = Configuration.VobSubCompareDirectory + databaseFolderName + Path.DirectorySeparatorChar;
            if (!File.Exists(_directoryPath + "Images.xml"))
            {
                _compareDoc.LoadXml("<OcrBitmaps></OcrBitmaps>");
            }
            else
            {
                _compareDoc.Load(_directoryPath + "Images.xml");
            }

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
            buttonImport.Text = Configuration.Settings.Language.SubStationAlphaStyles.Import;
            buttonOK.Text = Configuration.Settings.Language.General.Ok;
            buttonCancel.Text = Configuration.Settings.Language.General.Cancel;
            UiUtil.FixLargeFonts(this, buttonOK);
            buttonImport.Visible = binOcrDb != null;
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

            if (_binOcrDb != null)
            {
                foreach (BinaryOcrBitmap bob in _binOcrDb.CompareImages)
                {
                    string name = bob.Key;
                    foreach (VobSubOcr.ImageCompareAddition a in additions)
                    {
                        if (name == a.Name && bob.Text != null)
                        {
                            listBoxFileNames.Items.Add(bob);
                            _italics.Add(bob.Italic);
                        }
                    }
                }
                foreach (BinaryOcrBitmap bob in _binOcrDb.CompareImagesExpanded)
                {
                    string name = bob.Key;
                    foreach (VobSubOcr.ImageCompareAddition a in additions)
                    {
                        if (name == a.Name && bob.Text != null)
                        {
                            listBoxFileNames.Items.Add(bob);
                            _italics.Add(bob.Italic);
                        }
                    }
                }
            }
            else
            {
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
                                listBoxFileNames.Items.Add("[" + text + "] " + node.InnerText);
                                _italics.Add(node.Attributes["Italic"] != null);
                            }
                        }
                    }
                }
            }

            if (listBoxFileNames.Items.Count > 0)
            {
                listBoxFileNames.SelectedIndex = 0;
            }
        }

        private void FillComboWithUniqueAndSortedTexts()
        {
            int count = 0;
            List<string> texts = new List<string>();

            if (_binOcrDb != null)
            {
                foreach (BinaryOcrBitmap bob in _binOcrDb.CompareImages)
                {
                    string text = bob.Text;
                    if (!texts.Contains(text) && text != null)
                    {
                        texts.Add(text);
                    }

                    count++;
                }
                foreach (BinaryOcrBitmap bob in _binOcrDb.CompareImagesExpanded)
                {
                    string text = bob.Text;
                    if (!texts.Contains(text) && text != null)
                    {
                        texts.Add(text);
                    }

                    count++;
                }
            }
            else
            {
                foreach (XmlNode node in _compareDoc.DocumentElement.SelectNodes("Item"))
                {
                    if (node.Attributes.Count >= 1)
                    {
                        string text = node.Attributes["Text"].InnerText;
                        if (!texts.Contains(text))
                        {
                            texts.Add(text);
                        }

                        count++;
                    }
                }
            }
            texts.Sort();
            labelCount.Text = $"{count:#,##0}";

            comboBoxTexts.Items.Clear();
            foreach (string text in texts)
            {
                comboBoxTexts.Items.Add(text);
            }

            if (comboBoxTexts.Items.Count > 0)
            {
                comboBoxTexts.SelectedIndex = 0;
            }
        }

        private void ComboBoxTextsSelectedIndexChanged(object sender, EventArgs e)
        {
            _italics = new List<bool>();
            string target = comboBoxTexts.SelectedItem.ToString();
            textBoxText.Text = target;
            listBoxFileNames.Items.Clear();

            if (_binOcrDb != null)
            {
                foreach (BinaryOcrBitmap bob in _binOcrDb.CompareImages)
                {
                    string text = bob.Text;
                    if (text == target)
                    {
                        listBoxFileNames.Items.Add(bob);
                        _italics.Add(bob.Italic);
                    }
                }
                foreach (BinaryOcrBitmap bob in _binOcrDb.CompareImagesExpanded)
                {
                    string text = bob.Text;
                    if (text == target)
                    {
                        listBoxFileNames.Items.Add(bob);
                        _italics.Add(bob.Italic);
                    }
                }
            }
            else
            {
                foreach (XmlNode node in _compareDoc.DocumentElement.ChildNodes)
                {
                    if (node.Attributes["Text"] != null)
                    {
                        string text = node.Attributes["Text"].InnerText;
                        if (text == target)
                        {
                            listBoxFileNames.Items.Add(node.InnerText);
                            _italics.Add(node.Attributes["Italic"] != null);
                        }
                    }
                }
            }

            if (listBoxFileNames.Items.Count > 0)
            {
                listBoxFileNames.SelectedIndex = 0;
            }
        }

        private string GetSelectedFileName()
        {
            string fileName = listBoxFileNames.SelectedItem.ToString();
            if (fileName.StartsWith('['))
            {
                fileName = fileName.Substring(fileName.IndexOf(']') + 1);
            }

            return fileName.Trim();
        }

        private BinaryOcrBitmap GetSelectedBinOcrBitmap()
        {
            int idx = listBoxFileNames.SelectedIndex;
            if (idx < 0 || _binOcrDb == null)
            {
                return null;
            }

            return listBoxFileNames.Items[idx] as BinaryOcrBitmap;
        }

        private string GetFileName(int index)
        {
            string fileName = listBoxFileNames.Items[index].ToString();
            if (fileName.StartsWith('['))
            {
                fileName = fileName.Substring(fileName.IndexOf(']') + 1);
            }

            return fileName.Trim();
        }

        private void ListBoxFileNamesSelectedIndexChanged(object sender, EventArgs e)
        {
            checkBoxItalic.Checked = _italics[listBoxFileNames.SelectedIndex];
            string name = listBoxFileNames.Items[listBoxFileNames.SelectedIndex].ToString();
            string databaseName = _directoryPath + "Images.db";
            Bitmap bmp = null;
            labelExpandCount.Text = string.Empty;
            labelImageInfo.Text = string.Empty;
            if (_binOcrDb != null)
            {
                var bob = GetSelectedBinOcrBitmap();
                if (bob != null)
                {
                    bmp = bob.ToOldBitmap();
                    labelImageInfo.Text = $"Top:{bob.Y}, {bob.NumberOfColoredPixels} colored pixels of {(bob.Width * bob.Height)}";
                    if (bob.ExpandCount > 0)
                    {
                        labelExpandCount.Text = $"Expand count: {bob.ExpandCount}";
                    }
                }
            }
            else if (File.Exists(databaseName))
            {
                using (var f = new FileStream(databaseName, FileMode.Open))
                {
                    if (name.Contains(']'))
                    {
                        name = name.Substring(name.IndexOf(']') + 1).Trim();
                    }

                    f.Position = Convert.ToInt64(name);
                    bmp = new ManagedBitmap(f).ToOldBitmap();
                }
            }

            if (bmp == null)
            {
                bmp = new Bitmap(1, 1);
                labelImageInfo.Text = Configuration.Settings.Language.VobSubEditCharacters.ImageFileNotFound;
            }

            pictureBox1.Image = bmp;
            pictureBox1.Width = bmp.Width + 2;
            pictureBox1.Height = bmp.Height + 2;
            pictureBox1.SizeMode = PictureBoxSizeMode.CenterImage;

            var bmp2 = VobSubOcr.ResizeBitmap(bmp, bmp.Width * 2, bmp.Height * 2);
            pictureBox2.Image = bmp2;
            pictureBox2.Width = bmp2.Width + 2;
            pictureBox2.Height = bmp2.Height + 2;
            pictureBox2.SizeMode = PictureBoxSizeMode.CenterImage;

            if (Additions != null && Additions.Count > 0)
            {
                if (_binOcrDb != null)
                {
                    var bob = GetSelectedBinOcrBitmap();
                    foreach (var a in Additions)
                    {
                        if (bob?.Text != null && bob.Key == a.Name)
                        {
                            textBoxText.Text = a.Text;
                            checkBoxItalic.Checked = a.Italic;
                            break;
                        }
                    }
                }
                else
                {
                    string target = GetSelectedFileName();
                    foreach (var a in Additions)
                    {
                        if (target.StartsWith(a.Name, StringComparison.Ordinal))
                        {
                            textBoxText.Text = a.Text;
                            break;
                        }
                    }
                }
            }
        }

        private void VobSubEditCharacters_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                DialogResult = DialogResult.Cancel;
            }
        }

        private void ButtonUpdateClick(object sender, EventArgs e)
        {
            if (listBoxFileNames.Items.Count == 0)
            {
                return;
            }

            string target = GetSelectedFileName();
            string newText = textBoxText.Text;
            int oldTextItem = comboBoxTexts.SelectedIndex;
            int oldListBoxFileNamesIndex = listBoxFileNames.SelectedIndex;

            if (_binOcrDb != null)
            {
                var bob = GetSelectedBinOcrBitmap();
                if (bob == null)
                {
                    return;
                }

                string oldText = bob.Text;
                bob.Text = newText;
                bob.Italic = checkBoxItalic.Checked;

                if (Additions != null && Additions.Count > 0)
                {
                    for (int i = Additions.Count - 1; i >= 0; i--)
                    {
                        if (Additions[i].Name == bob.Key)
                        {
                            Additions[i].Italic = bob.Italic;
                            Additions[i].Text = bob.Text;
                            break;
                        }
                    }
                }
                Refill(Additions);

                if (oldText == newText)
                {
                    if (oldTextItem >= 0 && oldTextItem < comboBoxTexts.Items.Count)
                    {
                        comboBoxTexts.SelectedIndex = oldTextItem;
                    }

                    if (oldListBoxFileNamesIndex >= 0 && oldListBoxFileNamesIndex < listBoxFileNames.Items.Count)
                    {
                        listBoxFileNames.SelectedIndex = oldListBoxFileNamesIndex;
                    }
                }
                else
                {
                    int i = 0;
                    foreach (var item in comboBoxTexts.Items)
                    {
                        if (item.ToString() == newText)
                        {
                            comboBoxTexts.SelectedIndexChanged -= ComboBoxTextsSelectedIndexChanged;
                            comboBoxTexts.SelectedIndex = i;
                            ComboBoxTextsSelectedIndexChanged(sender, e);
                            comboBoxTexts.SelectedIndexChanged += ComboBoxTextsSelectedIndexChanged;
                            int k = 0;
                            foreach (var inner in listBoxFileNames.Items)
                            {
                                if ((inner as BinaryOcrBitmap) == bob)
                                {
                                    listBoxFileNames.SelectedIndex = k;
                                    break;
                                }
                                k++;
                            }
                            break;
                        }
                        i++;
                    }
                }
                listBoxFileNames.Focus();
                return;
            }

            XmlNode node = _compareDoc.DocumentElement.SelectSingleNode("Item[.='" + target + "']");
            if (node != null)
            {
                node.Attributes["Text"].InnerText = newText;

                if (Additions != null && Additions.Count > 0)
                {
                    foreach (var a in Additions)
                    {
                        if (target.StartsWith(a.Name, StringComparison.Ordinal))
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
                                if (GetFileName(j).StartsWith(target, StringComparison.Ordinal))
                                {
                                    listBoxFileNames.SelectedIndex = j;
                                }
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
            {
                return;
            }

            int oldComboBoxIndex = comboBoxTexts.SelectedIndex;
            string target = GetSelectedFileName();

            if (_binOcrDb != null)
            {
                BinaryOcrBitmap bob = GetSelectedBinOcrBitmap();
                if (bob != null)
                {
                    if (bob.ExpandCount > 0)
                    {
                        _binOcrDb.CompareImagesExpanded.Remove(bob);
                    }
                    else
                    {
                        _binOcrDb.CompareImages.Remove(bob);
                    }

                    if (Additions != null && Additions.Count > 0)
                    {
                        for (int i = Additions.Count - 1; i >= 0; i--)
                        {
                            if (Additions[i].Name == bob.Key)
                            {
                                Additions.RemoveAt(i);
                                Refill(Additions);
                                break;
                            }
                        }
                    }
                    Refill(Additions);
                }
                if (oldComboBoxIndex >= 0 && oldComboBoxIndex < comboBoxTexts.Items.Count)
                {
                    comboBoxTexts.SelectedIndex = oldComboBoxIndex;
                }

                return;
            }

            XmlNode node = _compareDoc.DocumentElement.SelectSingleNode("Item[.='" + target + "']");
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
                    {
                        comboBoxTexts.SelectedIndex = oldComboBoxIndex;
                    }
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
                        if (_binOcrDb != null)
                        {
                            for (int j = 0; j < listBoxFileNames.Items.Count; j++)
                            {
                                if ((listBoxFileNames.Items[j] as BinaryOcrBitmap).Key == name)
                                {
                                    listBoxFileNames.SelectedIndex = j;
                                }
                            }
                        }
                        else
                        {
                            for (int j = 0; j < listBoxFileNames.Items.Count; j++)
                            {
                                if (GetFileName(j).StartsWith(name, StringComparison.Ordinal))
                                {
                                    listBoxFileNames.SelectedIndex = j;
                                }
                            }
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

        public bool ChangesMade { get; set; }

        private void saveImageAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            saveFileDialog1.Title = Configuration.Settings.Language.VobSubOcr.SaveSubtitleImageAs;
            saveFileDialog1.AddExtension = true;
            saveFileDialog1.FileName = "Image";
            saveFileDialog1.Filter = "PNG image|*.png|BMP image|*.bmp|GIF image|*.gif|TIFF image|*.tiff";
            saveFileDialog1.FilterIndex = 0;

            DialogResult result = saveFileDialog1.ShowDialog(this);
            if (result == DialogResult.OK)
            {
                Bitmap bmp = pictureBox1.Image as Bitmap;
                if (bmp == null)
                {
                    MessageBox.Show("No image!");
                    return;
                }

                try
                {
                    if (saveFileDialog1.FilterIndex == 0)
                    {
                        bmp.Save(saveFileDialog1.FileName, System.Drawing.Imaging.ImageFormat.Png);
                    }
                    else if (saveFileDialog1.FilterIndex == 1)
                    {
                        bmp.Save(saveFileDialog1.FileName);
                    }
                    else if (saveFileDialog1.FilterIndex == 2)
                    {
                        bmp.Save(saveFileDialog1.FileName, System.Drawing.Imaging.ImageFormat.Gif);
                    }
                    else
                    {
                        bmp.Save(saveFileDialog1.FileName, System.Drawing.Imaging.ImageFormat.Tiff);
                    }
                }
                catch (Exception exception)
                {
                    MessageBox.Show(exception.Message);
                }
            }
        }

        private void buttonImport_Click(object sender, EventArgs e)
        {
            using (var form = new VobSubCharactersImport(_binOcrDb))
            {
                form.ShowDialog(this);
                DialogResult = DialogResult.OK;
            }
        }

    }
}
