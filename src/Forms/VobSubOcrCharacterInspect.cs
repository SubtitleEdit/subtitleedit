using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Windows.Forms;
using System.Xml;
using Nikse.SubtitleEdit.Logic;

namespace Nikse.SubtitleEdit.Forms
{
    public sealed partial class VobSubOcrCharacterInspect : Form
    {

        public XmlDocument ImageCompareDocument { get; private set; }
        private List<VobSubOcr.CompareMatch> _matches;
        private List<Bitmap> _imageSources;
        private string _directoryPath;
        private XmlNode _selectedCompareNode = null;

        public VobSubOcrCharacterInspect()
        {
            InitializeComponent();

            Text = Configuration.Settings.Language.VobSubOcrCharacterInspect.Title;
            groupBoxInspectItems.Text = Configuration.Settings.Language.VobSubOcrCharacterInspect.InspectItems;
            labelImageInfo.Text = string.Empty;
            groupBoxCurrentCompareImage.Text = Configuration.Settings.Language.VobSubEditCharacters.CurrentCompareImage;
            labelTextAssociatedWithImage.Text = Configuration.Settings.Language.VobSubEditCharacters.TextAssociatedWithImage;
            checkBoxItalic.Text = Configuration.Settings.Language.VobSubEditCharacters.IsItalic;
            buttonUpdate.Text = Configuration.Settings.Language.VobSubEditCharacters.Update;
            buttonDelete.Text = Configuration.Settings.Language.VobSubEditCharacters.Delete;
            buttonAddBetterMatch.Text = Configuration.Settings.Language.VobSubOcrCharacterInspect.AddBetterMatch;
            labelDoubleSize.Text = Configuration.Settings.Language.VobSubEditCharacters.ImageDoubleSize;
            buttonOK.Text = Configuration.Settings.Language.General.OK;
            buttonCancel.Text = Configuration.Settings.Language.General.Cancel;
            FixLargeFonts();
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

        internal void Initialize(string databaseFolderName, List<VobSubOcr.CompareMatch> matches, List<Bitmap> imageSources)
        {
            _matches = matches;
            _imageSources = imageSources;

            ImageCompareDocument = new XmlDocument();
            _directoryPath = Configuration.VobSubCompareFolder + databaseFolderName + Path.DirectorySeparatorChar;
            if (!File.Exists(_directoryPath + "Images.xml"))
                ImageCompareDocument.LoadXml("<OcrBitmaps></OcrBitmaps>");
            else
                ImageCompareDocument.Load(_directoryPath + "Images.xml");

            for (int i = 0; i < _matches.Count; i++)
                listBoxInspectItems.Items.Add(_matches[i].Text);
            if (listBoxInspectItems.Items.Count > 0)
                listBoxInspectItems.SelectedIndex = 0;
        }

        private void listBoxInspectItems_SelectedIndexChanged(object sender, EventArgs e)
        {
            labelImageInfo.Text = string.Empty;

            if (listBoxInspectItems.SelectedIndex < 0)
                return;

            _selectedCompareNode = null;
            pictureBoxInspectItem.Image = _imageSources[listBoxInspectItems.SelectedIndex];
            pictureBoxCompareBitmap.Image = null;
            pictureBoxCompareBitmapDouble.Image = null;

            int index = (listBoxInspectItems.SelectedIndex);
            var match = _matches[index];
            if (!string.IsNullOrEmpty(match.Name))
            {
                Bitmap bitmap = new Bitmap(1,1);
                foreach (XmlNode node in ImageCompareDocument.DocumentElement.ChildNodes)
                {
                    if (node.Attributes["Text"] != null && node.InnerText == match.Name)
                    {
                        string text = node.Attributes["Text"].InnerText;
                        string imageFileName = node.InnerText;
                        imageFileName = Path.Combine(_directoryPath, imageFileName);
                        textBoxText.Text = text;
                        checkBoxItalic.Checked = node.Attributes["Italic"] != null;

                        string databaseName = Path.Combine(_directoryPath, "Images.db");
                        using (var f = new FileStream(databaseName, FileMode.Open))
                        {
                            try
                            {
                                string name = node.InnerText;
                                int pos = Convert.ToInt32(name);
                                f.Position = pos;
                                ManagedBitmap mbmp = new ManagedBitmap(f);
                                bitmap = mbmp.ToOldBitmap();
                                pictureBoxCompareBitmap.Image = bitmap;
                                pictureBoxCompareBitmapDouble.Width = bitmap.Width * 2;
                                pictureBoxCompareBitmapDouble.Height = bitmap.Height * 2;
                                pictureBoxCompareBitmapDouble.Image = bitmap;
                            }
                            catch (Exception exception)
                            {
                                MessageBox.Show(exception.Message);
                            }
                        }


                        try
                        {
                            labelImageInfo.Text = string.Format(Configuration.Settings.Language.VobSubEditCharacters.Image + " - {0}x{1}", bitmap.Width, bitmap.Height);
                        }
                        catch
                        {
                        }

                        _selectedCompareNode = node;
                        break;
                    }
                }
            }

            if (_selectedCompareNode == null)
            {
                buttonUpdate.Enabled = false;
                buttonDelete.Enabled = false;
                buttonAddBetterMatch.Enabled = false;
                textBoxText.Enabled = false;
                textBoxText.Text = string.Empty;
                checkBoxItalic.Enabled = false;
            }
            else
            {
                buttonUpdate.Enabled = true;
                buttonDelete.Enabled = true;
                buttonAddBetterMatch.Enabled = true;
                textBoxText.Enabled = true;
                checkBoxItalic.Enabled = true;
            }
        }

        private void buttonUpdate_Click(object sender, EventArgs e)
        {
            if (_selectedCompareNode == null)
                return;

            XmlNode node = _selectedCompareNode;
            string newText = textBoxText.Text;
            listBoxInspectItems.SelectedIndexChanged -= listBoxInspectItems_SelectedIndexChanged;
            listBoxInspectItems.Items[listBoxInspectItems.SelectedIndex] = newText;
            listBoxInspectItems.SelectedIndexChanged += listBoxInspectItems_SelectedIndexChanged;
            node.Attributes["Text"].InnerText = newText;

            SetItalic(node);
            listBoxInspectItems_SelectedIndexChanged(null, null);
        }

        private void SetItalic(XmlNode node)
        {
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
        }

        private void buttonDelete_Click(object sender, EventArgs e)
        {
            if (_selectedCompareNode == null)
                return;
            listBoxInspectItems.Items[listBoxInspectItems.SelectedIndex] = Configuration.Settings.Language.VobSubOcr.NoMatch;
            ImageCompareDocument.DocumentElement.RemoveChild(_selectedCompareNode);
            _selectedCompareNode = null;
            listBoxInspectItems_SelectedIndexChanged(null, null);
        }

        private void buttonAddBetterMatch_Click(object sender, EventArgs e)
        {
            if (_selectedCompareNode != null)
            {
                if (listBoxInspectItems.SelectedIndex < 0)
                    return;

                if (listBoxInspectItems.Items[listBoxInspectItems.SelectedIndex].ToString() == textBoxText.Text)
                {
                    textBoxText.SelectAll();
                    textBoxText.Focus();
                    return;
                }

                XmlNode newNode = ImageCompareDocument.CreateElement("Item");
                XmlAttribute text = newNode.OwnerDocument.CreateAttribute("Text");
                text.InnerText = textBoxText.Text;
                newNode.Attributes.Append(text);


                string databaseName = Path.Combine(_directoryPath, "Images.db");
                FileStream f;
                long pos = 0;
                if (!File.Exists(databaseName))
                {
                    using (f = new FileStream(databaseName, FileMode.Create))
                    {
                        pos = f.Position;
                        new ManagedBitmap(pictureBoxInspectItem.Image as Bitmap).AppendToStream(f);
                    }
                }
                else
                {
                    using (f = new FileStream(databaseName, FileMode.Append))
                    {
                        pos = f.Position;
                        new ManagedBitmap(pictureBoxInspectItem.Image as Bitmap).AppendToStream(f);
                    }
                }
                string name = pos.ToString(CultureInfo.InvariantCulture);
                newNode.InnerText = name;


                SetItalic(newNode);
                ImageCompareDocument.DocumentElement.AppendChild(newNode);

                int index = listBoxInspectItems.SelectedIndex;
                _matches[index].Name = name;
                _matches[index].ExpandCount = 0;
                _matches[index].Italic = checkBoxItalic.Checked;
                _matches[index].Text = textBoxText.Text;
                listBoxInspectItems.Items.Clear();
                for (int i = 0; i < _matches.Count; i++)
                    listBoxInspectItems.Items.Add(_matches[i].Text);
                listBoxInspectItems.SelectedIndex = index;

                listBoxInspectItems_SelectedIndexChanged(null, null);
            }
        }

    }
}
