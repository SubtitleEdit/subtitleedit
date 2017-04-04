﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Windows.Forms;
using System.Xml;
using Nikse.SubtitleEdit.Core;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Ocr.Binary;

namespace Nikse.SubtitleEdit.Forms.Ocr
{
    public sealed partial class VobSubOcrCharacterInspect : Form
    {

        public XmlDocument ImageCompareDocument { get; private set; }
        private List<VobSubOcr.CompareMatch> _matches;
        private List<Bitmap> _imageSources;
        private string _directoryPath;
        private XmlNode _selectedCompareNode;
        private BinaryOcrBitmap _selectedCompareBinaryOcrBitmap;
        private VobSubOcr.CompareMatch _selectedMatch;
        private BinaryOcrDb _binOcrDb;

        public VobSubOcrCharacterInspect()
        {
            InitializeComponent();

            labelCount.Text = string.Empty;
            labelExpandCount.Text = string.Empty;
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
            buttonOK.Text = Configuration.Settings.Language.General.Ok;
            buttonCancel.Text = Configuration.Settings.Language.General.Cancel;
            UiUtil.FixLargeFonts(this, buttonOK);
        }

        internal void Initialize(string databaseFolderName, List<VobSubOcr.CompareMatch> matches, List<Bitmap> imageSources, BinaryOcrDb binOcrDb)
        {
            _binOcrDb = binOcrDb;
            _matches = matches;
            _imageSources = imageSources;

            if (_binOcrDb == null)
            {
                ImageCompareDocument = new XmlDocument();
                _directoryPath = Configuration.VobSubCompareDirectory + databaseFolderName + Path.DirectorySeparatorChar;
                if (!File.Exists(_directoryPath + "Images.xml"))
                    ImageCompareDocument.LoadXml("<OcrBitmaps></OcrBitmaps>");
                else
                    ImageCompareDocument.Load(_directoryPath + "Images.xml");
            }

            for (int i = 0; i < _matches.Count; i++)
                listBoxInspectItems.Items.Add(_matches[i]);
            if (listBoxInspectItems.Items.Count > 0)
                listBoxInspectItems.SelectedIndex = 0;
            ShowCount();
        }

        private void listBoxInspectItems_SelectedIndexChanged(object sender, EventArgs e)
        {
            labelImageInfo.Text = string.Empty;
            labelExpandCount.Text = string.Empty;

            if (listBoxInspectItems.SelectedIndex < 0)
                return;

            _selectedCompareNode = null;
            _selectedCompareBinaryOcrBitmap = null;

            pictureBoxInspectItem.Image = _imageSources[listBoxInspectItems.SelectedIndex];
            pictureBoxCompareBitmap.Image = null;
            pictureBoxCompareBitmapDouble.Image = null;

            int index = (listBoxInspectItems.SelectedIndex);
            var match = _matches[index];
            _selectedMatch = match;
            if (!string.IsNullOrEmpty(match.Name))
            {
                if (_binOcrDb != null)
                {
                    bool bobFound = false;
                    foreach (BinaryOcrBitmap bob in _binOcrDb.CompareImages)
                    {
                        if (match.Name == bob.Key)
                        {
                            textBoxText.Text = bob.Text;
                            checkBoxItalic.Checked = bob.Italic;
                            _selectedCompareBinaryOcrBitmap = bob;

                            var bitmap = bob.ToOldBitmap();
                            pictureBoxCompareBitmap.Image = bitmap;
                            pictureBoxCompareBitmapDouble.Width = bitmap.Width * 2;
                            pictureBoxCompareBitmapDouble.Height = bitmap.Height * 2;
                            pictureBoxCompareBitmapDouble.Image = bitmap;

                            var matchBob = new BinaryOcrBitmap(new NikseBitmap(_imageSources[listBoxInspectItems.SelectedIndex]));
                            if (matchBob.Hash == bob.Hash && matchBob.Width == bob.Width && matchBob.Height == bob.Height && matchBob.NumberOfColoredPixels == bob.NumberOfColoredPixels)
                            {
                                buttonAddBetterMatch.Enabled = false; // exact match
                            }
                            else
                            {
                                buttonAddBetterMatch.Enabled = true;
                            }

                            bobFound = true;
                            break;
                        }
                    }
                    if (!bobFound)
                    {
                        foreach (BinaryOcrBitmap bob in _binOcrDb.CompareImagesExpanded)
                        {
                            if (match.Name == bob.Key)
                            {
                                textBoxText.Text = bob.Text;
                                checkBoxItalic.Checked = bob.Italic;
                                _selectedCompareBinaryOcrBitmap = bob;

                                var bitmap = bob.ToOldBitmap();
                                pictureBoxCompareBitmap.Image = bitmap;
                                pictureBoxCompareBitmapDouble.Width = bitmap.Width * 2;
                                pictureBoxCompareBitmapDouble.Height = bitmap.Height * 2;
                                pictureBoxCompareBitmapDouble.Image = bitmap;
                                buttonAddBetterMatch.Enabled = false; // exact match
                                labelExpandCount.Text = string.Format("Expand count: {0}", bob.ExpandCount);
                                break;
                            }
                        }
                    }
                }
                else
                {
                    foreach (XmlNode node in ImageCompareDocument.DocumentElement.ChildNodes)
                    {
                        if (node.Attributes["Text"] != null && node.InnerText == match.Name)
                        {
                            string text = node.Attributes["Text"].InnerText;
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
                                    var mbmp = new ManagedBitmap(f);
                                    var bitmap = mbmp.ToOldBitmap();
                                    pictureBoxCompareBitmap.Image = bitmap;
                                    pictureBoxCompareBitmapDouble.Width = bitmap.Width * 2;
                                    pictureBoxCompareBitmapDouble.Height = bitmap.Height * 2;
                                    pictureBoxCompareBitmapDouble.Image = bitmap;
                                    labelImageInfo.Text = string.Format(Configuration.Settings.Language.VobSubEditCharacters.Image + " - {0}x{1}", bitmap.Width, bitmap.Height);
                                }
                                catch (Exception exception)
                                {
                                    labelImageInfo.Text = Configuration.Settings.Language.VobSubEditCharacters.Image;
                                    MessageBox.Show(exception.Message);
                                }
                            }

                            _selectedCompareNode = node;
                            break;
                        }
                    }
                }
            }

            buttonAddBetterMatch.Text = Configuration.Settings.Language.VobSubOcrCharacterInspect.AddBetterMatch;
            if (_selectedMatch.Text == Configuration.Settings.Language.VobSubOcr.NoMatch)
            {
                buttonUpdate.Enabled = false;
                buttonDelete.Enabled = false;
                buttonAddBetterMatch.Enabled = true;
                buttonAddBetterMatch.Text = Configuration.Settings.Language.VobSubOcrCharacterInspect.Add;
                textBoxText.Enabled = true;
                textBoxText.Text = string.Empty;
                checkBoxItalic.Enabled = true;
            }
            else if (_selectedCompareNode == null && _selectedCompareBinaryOcrBitmap == null)
            {
                buttonUpdate.Enabled = false;
                buttonDelete.Enabled = false;
                buttonAddBetterMatch.Enabled = true;
                textBoxText.Enabled = true;
                textBoxText.Text = string.Empty;
                checkBoxItalic.Enabled = false;
            }
            else
            {
                buttonUpdate.Enabled = true;
                buttonDelete.Enabled = true;
                if (_selectedCompareNode != null)
                    buttonAddBetterMatch.Enabled = true;
                textBoxText.Enabled = true;
                checkBoxItalic.Enabled = true;
            }
        }

        private void buttonUpdate_Click(object sender, EventArgs e)
        {
            if (_selectedCompareNode == null && _selectedCompareBinaryOcrBitmap == null)
                return;

            string newText = textBoxText.Text;

            if (_selectedCompareBinaryOcrBitmap != null)
            {
                foreach (var match in _matches)
                {
                    if (match.Name == _selectedCompareBinaryOcrBitmap.Key)
                    {
                        _selectedCompareBinaryOcrBitmap.Text = newText;
                        _selectedCompareBinaryOcrBitmap.Italic = checkBoxItalic.Checked;
                        match.Text = newText;
                        match.Italic = checkBoxItalic.Checked;
                        match.Name = _selectedCompareBinaryOcrBitmap.Key;
                        break;
                    }
                }

                _selectedCompareBinaryOcrBitmap.Text = newText;
                _selectedCompareBinaryOcrBitmap.Italic = checkBoxItalic.Checked;
                listBoxInspectItems.SelectedIndexChanged -= listBoxInspectItems_SelectedIndexChanged;
                if (checkBoxItalic.Checked)
                    listBoxInspectItems.Items[listBoxInspectItems.SelectedIndex] = newText + " (italic)";
                else
                    listBoxInspectItems.Items[listBoxInspectItems.SelectedIndex] = newText;
                listBoxInspectItems.SelectedIndexChanged += listBoxInspectItems_SelectedIndexChanged;
            }
            else
            {
                XmlNode node = _selectedCompareNode;
                listBoxInspectItems.SelectedIndexChanged -= listBoxInspectItems_SelectedIndexChanged;
                listBoxInspectItems.Items[listBoxInspectItems.SelectedIndex] = newText;
                listBoxInspectItems.SelectedIndexChanged += listBoxInspectItems_SelectedIndexChanged;
                node.Attributes["Text"].InnerText = newText;
                SetItalic(node);
            }

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
            if (_selectedCompareNode == null && _selectedCompareBinaryOcrBitmap == null)
                return;

            listBoxInspectItems.Items[listBoxInspectItems.SelectedIndex] = Configuration.Settings.Language.VobSubOcr.NoMatch;
            if (_selectedCompareBinaryOcrBitmap != null)
            {
                if (_selectedCompareBinaryOcrBitmap.ExpandCount > 0)
                    _binOcrDb.CompareImagesExpanded.Remove(_selectedCompareBinaryOcrBitmap);
                else
                    _binOcrDb.CompareImages.Remove(_selectedCompareBinaryOcrBitmap);
                _selectedCompareBinaryOcrBitmap = null;
            }
            else
            {
                ImageCompareDocument.DocumentElement.RemoveChild(_selectedCompareNode);
                _selectedCompareNode = null;
            }
            listBoxInspectItems_SelectedIndexChanged(null, null);
        }

        private void buttonAddBetterMatch_Click(object sender, EventArgs e)
        {
            if (listBoxInspectItems.SelectedIndex < 0)
                return;

            if (listBoxInspectItems.Items[listBoxInspectItems.SelectedIndex].ToString() == textBoxText.Text)
            {
                textBoxText.SelectAll();
                textBoxText.Focus();
                return;
            }

            if (_selectedCompareBinaryOcrBitmap != null || (_binOcrDb != null && (_selectedMatch.Text == Configuration.Settings.Language.VobSubOcr.NoMatch || _selectedMatch.NOcrCharacter == null)))
            {
                var nbmp = new NikseBitmap((pictureBoxInspectItem.Image as Bitmap));
                int x = 0;
                int y = 0;
                if (_selectedMatch != null && _selectedMatch.ImageSplitterItem != null)
                {
                    if (_selectedMatch.ImageSplitterItem != null)
                    {
                        x = _selectedMatch.ImageSplitterItem.X;
                        y = _selectedMatch.ImageSplitterItem.Top;
                    }
                    else
                    {
                        x = _selectedMatch.X;
                        y = _selectedMatch.Y;
                    }
                }
                var bob = new BinaryOcrBitmap(nbmp, checkBoxItalic.Checked, 0, textBoxText.Text, x, y);
                _binOcrDb.Add(bob);

                int index = listBoxInspectItems.SelectedIndex;
                _matches[index].Name = bob.Key;
                _matches[index].ExpandCount = 0;
                _matches[index].Italic = checkBoxItalic.Checked;
                _matches[index].Text = textBoxText.Text;
                listBoxInspectItems.Items.Clear();
                for (int i = 0; i < _matches.Count; i++)
                    listBoxInspectItems.Items.Add(_matches[i].Text);
                listBoxInspectItems.SelectedIndex = index;
                listBoxInspectItems_SelectedIndexChanged(null, null);
                ShowCount();

                // update other letters that are exact matches
                for (int i = 0; i < _matches.Count; i++)
                {
                    if (i != index && i < _imageSources.Count && _matches[i].ExpandCount == 0)
                    {
                        var newMatch = _binOcrDb.FindExactMatch(new BinaryOcrBitmap(new NikseBitmap(_imageSources[i])));
                        if (newMatch >= 0 && _binOcrDb.CompareImages[newMatch].Hash == bob.Hash)
                        {
                            _matches[i].Name = bob.Key;
                            _matches[i].ExpandCount = 0;
                            _matches[i].Italic = checkBoxItalic.Checked;
                            _matches[i].Text = textBoxText.Text;
                            listBoxInspectItems.Items[i] = textBoxText.Text;
                        }
                    }
                }

                return;
            }

            if (_selectedCompareNode != null)
            {
                XmlNode newNode = ImageCompareDocument.CreateElement("Item");
                var text = newNode.OwnerDocument.CreateAttribute("Text");
                text.InnerText = textBoxText.Text;
                newNode.Attributes.Append(text);

                string databaseName = Path.Combine(_directoryPath, "Images.db");
                FileStream f;
                long pos;
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
                ShowCount();
                listBoxInspectItems_SelectedIndexChanged(null, null);
            }
        }

        private void ShowCount()
        {
            labelCount.Text = listBoxInspectItems.Items.Count > 1 ? listBoxInspectItems.Items.Count.ToString(CultureInfo.InvariantCulture) : string.Empty;
        }

        private void VobSubOcrCharacterInspect_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                DialogResult = DialogResult.Cancel;
            }
        }

    }
}
