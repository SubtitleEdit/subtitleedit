using Nikse.SubtitleEdit.Core;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Ocr;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.Forms.Ocr
{
    public partial class VobSubNOcrCharacterInspect : Form
    {
        private List<ImageSplitterItem> _imageList;
        private List<VobSubOcr.CompareMatch> _matchList;
        private List<NOcrChar> _nocrChars;
        private NOcrChar _nocrChar;
        private VobSubOcr _vobSubOcr;
        private Bitmap _bitmap;
        private Bitmap _bitmap2;
        private double _zoomFactor = 3.0;

        public VobSubNOcrCharacterInspect()
        {
            UiUtil.PreInitialize(this);
            InitializeComponent();
            UiUtil.FixFonts(this);
        }

        private void VobSubNOcrCharacterInspect_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                DialogResult = DialogResult.Cancel;
            }
        }

        internal void Initialize(Bitmap bitmap, int pixelsIsSpace, bool rightToLeft, NOcrDb nOcrDb, VobSubOcr vobSubOcr)
        {
            _bitmap = bitmap;
            var nbmp = new NikseBitmap(bitmap);
            nbmp.ReplaceNonWhiteWithTransparent();
            bitmap = nbmp.GetBitmap();
            _bitmap2 = bitmap;
            _nocrChars = nOcrDb.OcrCharacters;
            _matchList = new List<VobSubOcr.CompareMatch>();
            _vobSubOcr = vobSubOcr;

            const int minLineHeight = 6;
            _imageList = NikseBitmapImageSplitter.SplitBitmapToLettersNew(nbmp, pixelsIsSpace, rightToLeft, Configuration.Settings.VobSubOcr.TopToBottom, minLineHeight);

            int index = 0;
            while (index < _imageList.Count)
            {
                ImageSplitterItem item = _imageList[index];
                if (item.NikseBitmap == null)
                {
                    listBoxInspectItems.Items.Add(item.SpecialCharacter);
                    _matchList.Add(null);
                }
                else
                {
                    nbmp = item.NikseBitmap;
                    nbmp.ReplaceNonWhiteWithTransparent();
                    item.Y += nbmp.CropTopTransparent(0);
                    nbmp.CropTransparentSidesAndBottom(0, true);
                    nbmp.ReplaceTransparentWith(Color.Black);

                    //get nocr matches
                    var match = vobSubOcr.GetNOcrCompareMatchNew(item, nbmp, nOcrDb, false, false);
                    if (match == null)
                    {
                        listBoxInspectItems.Items.Add("?");
                        _matchList.Add(null);
                    }
                    else
                    {
                        listBoxInspectItems.Items.Add(match.Text);
                        _matchList.Add(match);
                    }
                }
                index++;
            }
        }

        private void listBoxInspectItems_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBoxInspectItems.SelectedIndex < 0)
            {
                return;
            }

            var img = _imageList[listBoxInspectItems.SelectedIndex];
            if (img.NikseBitmap != null)
            {
                pictureBoxInspectItem.Width = img.NikseBitmap.Width;
                pictureBoxInspectItem.Height = img.NikseBitmap.Height;
                var old = img.NikseBitmap.GetBitmap();
                pictureBoxInspectItem.Image = old;
                pictureBoxCharacter.Image = old;
                SizePictureBox();
            }
            else
            {
                pictureBoxInspectItem.Image = null;
                pictureBoxCharacter.Image = null;
            }

            var match = _matchList[listBoxInspectItems.SelectedIndex];
            if (match == null)
            { // spaces+new lines
                _nocrChar = null;
                pictureBoxCharacter.Invalidate();

                buttonUpdate.Enabled = false;
                buttonDelete.Enabled = false;
                buttonEditDB.Enabled = false;
                buttonAddBetterMatch.Enabled = false;
                textBoxText.Text = string.Empty;
                textBoxText.Enabled = false;
                checkBoxItalic.Checked = false;
                checkBoxItalic.Enabled = false;
            }
            else if (match.NOcrCharacter == null)
            { // no match found
                buttonUpdate.Enabled = false;
                buttonDelete.Enabled = false;
                textBoxText.Text = string.Empty;
                checkBoxItalic.Checked = match.Italic;
                _nocrChar = null;
                pictureBoxCharacter.Invalidate();

                buttonEditDB.Enabled = true;
                buttonAddBetterMatch.Enabled = true;
                textBoxText.Enabled = false;
                checkBoxItalic.Enabled = false;
            }
            else
            {
                buttonUpdate.Enabled = true;
                buttonDelete.Enabled = true;
                textBoxText.Text = match.Text;
                checkBoxItalic.Checked = match.Italic;
                _nocrChar = match.NOcrCharacter;
                pictureBoxCharacter.Invalidate();

                buttonEditDB.Enabled = true;
                buttonAddBetterMatch.Enabled = true;
                textBoxText.Enabled = true;
                checkBoxItalic.Enabled = true;
            }
        }

        private void pictureBoxCharacter_Paint(object sender, PaintEventArgs e)
        {
            if (_nocrChar == null)
            {
                return;
            }

            var foreground = new Pen(new SolidBrush(Color.Green));
            var background = new Pen(new SolidBrush(Color.Red));
            if (pictureBoxCharacter.Image != null)
            {
                foreach (NOcrPoint op in _nocrChar.LinesForeground)
                {
                    e.Graphics.DrawLine(foreground, op.GetScaledStart(_nocrChar, pictureBoxCharacter.Width, pictureBoxCharacter.Height), op.GetScaledEnd(_nocrChar, pictureBoxCharacter.Width, pictureBoxCharacter.Height));
                }
                foreach (NOcrPoint op in _nocrChar.LinesBackground)
                {
                    e.Graphics.DrawLine(background, op.GetScaledStart(_nocrChar, pictureBoxCharacter.Width, pictureBoxCharacter.Height), op.GetScaledEnd(_nocrChar, pictureBoxCharacter.Width, pictureBoxCharacter.Height));
                }
            }
        }

        private void SizePictureBox()
        {
            if (pictureBoxCharacter.Image is Bitmap bmp)
            {
                pictureBoxCharacter.SizeMode = PictureBoxSizeMode.StretchImage;
                pictureBoxCharacter.Width = (int)Math.Round(bmp.Width * _zoomFactor);
                pictureBoxCharacter.Height = (int)Math.Round(bmp.Height * _zoomFactor);
                pictureBoxCharacter.Invalidate();
            }
        }

        private void buttonZoomIn_Click(object sender, EventArgs e)
        {
            if (_zoomFactor < 20)
            {
                _zoomFactor++;
                SizePictureBox();
            }
        }

        private void buttonZoomOut_Click(object sender, EventArgs e)
        {
            if (_zoomFactor > 1)
            {
                _zoomFactor--;
                SizePictureBox();
            }
        }

        private void buttonUpdate_Click(object sender, EventArgs e)
        {
            if (_nocrChar != null)
            {
                _nocrChar.Text = textBoxText.Text;
                _nocrChar.Italic = checkBoxItalic.Checked;
                _vobSubOcr.SaveNOcrWithCurrentLanguage();
                MessageBox.Show("nOCR saved!");
            }
        }

        private void buttonDelete_Click(object sender, EventArgs e)
        {
            if (_nocrChar != null)
            {
                _nocrChars.Remove(_nocrChar);
                _vobSubOcr.SaveNOcrWithCurrentLanguage();
                MessageBox.Show("nOCR saved!");
            }
        }

        private void buttonAddBetterMatch_Click(object sender, EventArgs e)
        {
            var expandSelectionList = new List<ImageSplitterItem>();
            if (listBoxInspectItems.SelectedIndex < 0)
            {
                return;
            }

            int index = listBoxInspectItems.SelectedIndex;
            var img = _imageList[index];
            if (img.NikseBitmap == null)
            {
                return;
            }

            using (var vobSubOcrNOcrCharacter = new VobSubOcrNOcrCharacter())
            {
                vobSubOcrNOcrCharacter.Initialize(_bitmap, img, new Point(0, 0), false, expandSelectionList.Count > 1);
                DialogResult result = vobSubOcrNOcrCharacter.ShowDialog(this);
                bool expandSelection = false;
                bool shrinkSelection = false;
                if (result == DialogResult.OK && vobSubOcrNOcrCharacter.ExpandSelection)
                {
                    expandSelection = true;
                    expandSelectionList.Add(img);
                }
                while (result == DialogResult.OK && (vobSubOcrNOcrCharacter.ShrinkSelection || vobSubOcrNOcrCharacter.ExpandSelection))
                {
                    if (expandSelection || shrinkSelection)
                    {
                        expandSelection = false;
                        if (shrinkSelection && index > 0)
                        {
                            shrinkSelection = false;
                        }
                        else if (index + 1 < _imageList.Count && _imageList[index + 1].NikseBitmap != null) // only allow expand to EndOfLine or space
                        {
                            index++;
                            expandSelectionList.Add(_imageList[index]);
                        }
                        img = VobSubOcr.GetExpandedSelection(new NikseBitmap(_bitmap), expandSelectionList, false); // true
                    }

                    vobSubOcrNOcrCharacter.Initialize(_bitmap2, img, new Point(0, 0), false, expandSelectionList.Count > 1);
                    result = vobSubOcrNOcrCharacter.ShowDialog(this);

                    if (result == DialogResult.OK && vobSubOcrNOcrCharacter.ShrinkSelection)
                    {
                        shrinkSelection = true;
                        index--;
                        if (expandSelectionList.Count > 0)
                        {
                            expandSelectionList.RemoveAt(expandSelectionList.Count - 1);
                        }
                    }
                    else if (result == DialogResult.OK && vobSubOcrNOcrCharacter.ExpandSelection)
                    {
                        expandSelection = true;
                        index++;
                        expandSelectionList.Add(_imageList[index]);
                    }
                }
                if (result == DialogResult.OK)
                {
                    if (expandSelectionList.Count > 1)
                    {
                        vobSubOcrNOcrCharacter.NOcrChar.ExpandCount = expandSelectionList.Count;
                    }

                    _nocrChars.Add(vobSubOcrNOcrCharacter.NOcrChar);
                    _vobSubOcr.SaveNOcrWithCurrentLanguage();
                    DialogResult = DialogResult.OK;
                }
            }
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
        }

        private void buttonEditDB_Click(object sender, EventArgs e)
        {
            var form = new VobSubNOcrEdit(_nocrChars, pictureBoxInspectItem.Image as Bitmap);
            form.ShowDialog(this);
        }
    }
}
