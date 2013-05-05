using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.OCR;
using System.IO;

namespace Nikse.SubtitleEdit.Forms
{
    public partial class VobSubNOcrCharacterInspect : Form
    {
        private List<ImageSplitterItem> _imageList;
        private List<VobSubOcr.CompareMatch> _matchList;
        private List<NOcrChar> _nocrChars;
        private NOcrChar _nocrChar = null;
        private VobSubOcr _vobSubOcr;
        private Bitmap _bitmap;
        private double _zoomFactor = 3.0;
        private double _unItalicFactor;

        public VobSubNOcrCharacterInspect()
        {
            InitializeComponent();
        }

        private void VobSubNOcrCharacterInspect_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
                DialogResult = DialogResult.Cancel;
        }

        internal void Initialize(Bitmap bitmap, int pixelsIsSpace, bool rightToLeft, List<NOcrChar> nocrChars, VobSubOcr vobSubOcr, double unItalicFactor)
        {
            _bitmap = bitmap;
            NikseBitmap nbmp = new NikseBitmap(bitmap);
            nbmp.ReplaceNonWhiteWithTransparent();
            bitmap = nbmp.GetBitmap();
            _nocrChars = nocrChars;
            _matchList = new List<VobSubOcr.CompareMatch>();
            _vobSubOcr = vobSubOcr;
            _unItalicFactor = unItalicFactor;

            _imageList = ImageSplitter.SplitBitmapToLetters(bitmap, pixelsIsSpace, rightToLeft, Configuration.Settings.VobSubOcr.TopToBottom);
            int index = 0;
            while (index < _imageList.Count)
            {
                ImageSplitterItem item = _imageList[index];
                if (item.Bitmap == null)
                {
                    listBoxInspectItems.Items.Add(item.SpecialCharacter);
                    _matchList.Add(null);
                }
                else
                {
                    nbmp = new NikseBitmap(item.Bitmap);
                    nbmp.ReplaceNonWhiteWithTransparent();
                    item.Y += nbmp.CropTopTransparent(0);
                    nbmp.CropTransparentSidesAndBottom(0, true);
                    nbmp.ReplaceTransparentWith(Color.Black);
                    item.Bitmap = nbmp.GetBitmap();

                    //get nocr matches
                    Nikse.SubtitleEdit.Forms.VobSubOcr.CompareMatch bestGuess;
                    Nikse.SubtitleEdit.Forms.VobSubOcr.CompareMatch match = vobSubOcr.GetNOcrCompareMatch(item, bitmap, out bestGuess, _nocrChars, _unItalicFactor, false, false);
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
                return;

            var img = _imageList[listBoxInspectItems.SelectedIndex];
            if (img.Bitmap != null)
            {
                pictureBoxInspectItem.Width = img.Bitmap.Width;
                pictureBoxInspectItem.Height = img.Bitmap.Height;
                pictureBoxInspectItem.Image = img.Bitmap;
                pictureBoxCharacter.Image = img.Bitmap;
                SizePictureBox();
            }
            else
            {
                pictureBoxInspectItem.Image = null;
                pictureBoxCharacter.Image = null;
            }

            var match = _matchList[listBoxInspectItems.SelectedIndex];
            if (match == null)
            {
                _nocrChar = null;
                pictureBoxCharacter.Invalidate();
                groupBoxCurrentCompareImage.Enabled = false;
                buttonEditDB.Enabled = true;
                buttonAddBetterMatch.Enabled = true;
            }
            else
            {
                textBoxText.Text = match.Text;
                checkBoxItalic.Checked = match.Italic;
                _nocrChar = match.NOcrCharacter;
                pictureBoxCharacter.Invalidate();
                groupBoxCurrentCompareImage.Enabled = true;
            }
        }

        private void pictureBoxCharacter_Paint(object sender, PaintEventArgs e)
        {
            if (_nocrChar == null)
                return;

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
            if (pictureBoxCharacter.Image != null)
            {
                Bitmap bmp = pictureBoxCharacter.Image as Bitmap;
                pictureBoxCharacter.SizeMode = PictureBoxSizeMode.StretchImage;
                pictureBoxCharacter.Width = (int)Math.Round(bmp.Width * _zoomFactor);
                pictureBoxCharacter.Height = (int)Math.Round(bmp.Height * _zoomFactor);
                pictureBoxCharacter.Invalidate();
            }
        }

        private void buttonZoomIn_Click(object sender, EventArgs e)
        {
            if (_zoomFactor < 10)
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
                return;

            var img = _imageList[listBoxInspectItems.SelectedIndex];
            if (img.Bitmap == null)
                return;
            
            var match = _matchList[listBoxInspectItems.SelectedIndex];

            var vobSubOcrNOcrCharacter = new VobSubOcrNOcrCharacter();
            vobSubOcrNOcrCharacter.Initialize(_bitmap, img, new Point(0, 0), false, expandSelectionList.Count > 1, null, null, _vobSubOcr);
            DialogResult result = vobSubOcrNOcrCharacter.ShowDialog(this);
            while (result == DialogResult.OK && (vobSubOcrNOcrCharacter.ShrinkSelection || vobSubOcrNOcrCharacter.ExpandSelection))
            {
                result = vobSubOcrNOcrCharacter.ShowDialog(this);
            //if (result == DialogResult.OK && _vobSubOcrNOcrCharacter.ShrinkSelection)
            //{
            //    shrinkSelection = true;
            //    index--;
            //    if (expandSelectionList.Count > 0)
            //        expandSelectionList.RemoveAt(expandSelectionList.Count - 1);
            //}
            //else if (result == DialogResult.OK && _vobSubOcrNOcrCharacter.ExpandSelection)
            //{
            //    expandSelection = true;
            //}
            }
            if (result == DialogResult.OK)
            {
                _nocrChars.Add(vobSubOcrNOcrCharacter.NOcrChar);
                _vobSubOcr.SaveNOcrWithCurrentLanguage();
                DialogResult = DialogResult.OK;
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
