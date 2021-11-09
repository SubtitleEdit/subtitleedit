using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Ocr;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.Forms.Ocr
{
    public partial class VobSubNOcrCharacterInspect : Form
    {
        private List<ImageSplitterItem> _imageList;
        private List<VobSubOcr.CompareMatch> _matchList;
        private List<NOcrChar> _nOcrChars;
        private NOcrChar _nOcrChar;
        private NOcrDb _nOcrDb;
        private Bitmap _bitmap;
        private double _zoomFactor = 3.0;
        private Dictionary<int, int> _indexLookup;

        public VobSubNOcrCharacterInspect()
        {
            UiUtil.PreInitialize(this);
            InitializeComponent();
            VobSubEditCharacters.MakeToolStripLetters(contextMenuStripLetters, InsertLanguageCharacter);
            UiUtil.FixFonts(this);
            labelImageSize.Text = string.Empty;
            labelStatus.Text = string.Empty;
            labelExpandCount.Text = string.Empty;

            buttonAddBetterMatch.Text = LanguageSettings.Current.VobSubOcrCharacterInspect.AddBetterMatch;
            buttonOK.Text = LanguageSettings.Current.General.Ok;
            buttonCancel.Text = LanguageSettings.Current.General.Cancel;
        }

        private void InsertLanguageCharacter(object sender, EventArgs e)
        {
            if (sender is ToolStripMenuItem toolStripMenuItem)
            {
                var start = textBoxText.SelectionStart;
                textBoxText.SelectedText = toolStripMenuItem.Text;
                textBoxText.SelectionLength = 0;
                textBoxText.SelectionStart = start + toolStripMenuItem.Text.Length;
            }
        }

        private void VobSubNOcrCharacterInspect_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                DialogResult = DialogResult.Cancel;
            }
        }

        internal void Initialize(Bitmap bitmap, int pixelsIsSpace, bool rightToLeft, NOcrDb nOcrDb, VobSubOcr vobSubOcr, bool italic, int minLineHeight, bool deepSeek)
        {
            _bitmap = bitmap;
            var nikseBitmap = new NikseBitmap(bitmap);
            nikseBitmap.ReplaceNonWhiteWithTransparent();
            _nOcrChars = nOcrDb.OcrCharacters;
            _nOcrDb = nOcrDb;
            _matchList = new List<VobSubOcr.CompareMatch>();

            _imageList = NikseBitmapImageSplitter.SplitBitmapToLettersNew(nikseBitmap, pixelsIsSpace, rightToLeft, Configuration.Settings.VobSubOcr.TopToBottom, minLineHeight, false);

            int index = 0;
            _indexLookup = new Dictionary<int, int>();
            while (index < _imageList.Count)
            {
                var item = _imageList[index];
                if (item.NikseBitmap == null)
                {
                    _indexLookup.Add(listBoxInspectItems.Items.Count, index);
                    listBoxInspectItems.Items.Add(item.SpecialCharacter);
                    _matchList.Add(null);
                }
                else
                {
                    var match = vobSubOcr.GetNOcrCompareMatchNew(item, nikseBitmap, nOcrDb, italic, deepSeek, index, _imageList);
                    if (match == null)
                    {
                        _indexLookup.Add(listBoxInspectItems.Items.Count, index);
                        listBoxInspectItems.Items.Add("?");
                        _matchList.Add(null);
                    }
                    else
                    {
                        _indexLookup.Add(listBoxInspectItems.Items.Count, index);
                        listBoxInspectItems.Items.Add(match.Text);
                        _matchList.Add(match);
                        if (match.ExpandCount > 0)
                        {
                            index += match.ExpandCount - 1;
                        }
                    }
                }
                index++;
            }
        }

        private void listBoxInspectItems_SelectedIndexChanged(object sender, EventArgs e)
        {
            labelImageSize.Text = string.Empty;
            labelExpandCount.Text = string.Empty;
            if (listBoxInspectItems.SelectedIndex < 0)
            {
                return;
            }

            var idx = _indexLookup[listBoxInspectItems.SelectedIndex];
            var img = _imageList[idx];
            var match = _matchList[listBoxInspectItems.SelectedIndex];

            if (img.NikseBitmap != null)
            {
                pictureBoxInspectItem.Width = img.NikseBitmap.Width;
                pictureBoxInspectItem.Height = img.NikseBitmap.Height;
                labelImageSize.Top = pictureBoxInspectItem.Top + img.NikseBitmap.Height + 7;
                labelImageSize.Text = img.NikseBitmap.Width + "x" + img.NikseBitmap.Height;
                if (match != null && match.ExpandCount > 1)
                {
                    var expandList = new List<ImageSplitterItem>();
                    for (int i = idx; i < idx + match.ExpandCount; i++)
                    {
                        expandList.Add(_imageList[i]);
                    }

                    var expandItem = VobSubOcr.GetExpandedSelectionNew(new NikseBitmap(_bitmap), expandList);
                    var old = expandItem.NikseBitmap.GetBitmap();
                    pictureBoxInspectItem.Image = old;
                    pictureBoxCharacter.Image = old;
                    pictureBoxInspectItem.Width = old.Width;
                    pictureBoxInspectItem.Height = old.Height;
                    labelExpandCount.Text = $"Expand count: {match.ExpandCount}";
                }
                else
                {
                    var old = img.NikseBitmap.GetBitmap();
                    pictureBoxInspectItem.Image = old;
                    pictureBoxCharacter.Image = old;
                }

                SizePictureBox();
            }
            else
            {
                pictureBoxInspectItem.Image = null;
                pictureBoxCharacter.Image = null;
            }

            buttonAddBetterMatch.Text = LanguageSettings.Current.VobSubOcrCharacterInspect.AddBetterMatch;
            if (match == null)
            {
                if (img.NikseBitmap != null && img.SpecialCharacter == null)
                {
                    // no match found
                    buttonUpdate.Enabled = false;
                    buttonDelete.Enabled = false;
                    textBoxText.Text = string.Empty;
                    _nOcrChar = null;
                    pictureBoxCharacter.Invalidate();

                    buttonEditDB.Enabled = true;
                    buttonAddBetterMatch.Enabled = true;
                    buttonAddBetterMatch.Text = LanguageSettings.Current.VobSubOcrCharacterInspect.Add;
                    textBoxText.Enabled = true;
                    checkBoxItalic.Enabled = true;
                }
                else
                {
                    // spaces+new lines
                    _nOcrChar = null;
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
            }
            else if (match.NOcrCharacter == null)
            { // no match found
                buttonUpdate.Enabled = false;
                buttonDelete.Enabled = false;
                textBoxText.Text = string.Empty;
                checkBoxItalic.Checked = match.Italic;
                _nOcrChar = null;
                pictureBoxCharacter.Invalidate();

                buttonEditDB.Enabled = true;
                buttonAddBetterMatch.Enabled = true;
                buttonAddBetterMatch.Text = LanguageSettings.Current.VobSubOcrCharacterInspect.Add;
                textBoxText.Enabled = true;
                checkBoxItalic.Enabled = true;
            }
            else
            {
                buttonUpdate.Enabled = true;
                buttonDelete.Enabled = true;
                textBoxText.Text = match.Text;
                checkBoxItalic.Checked = match.Italic;
                _nOcrChar = match.NOcrCharacter;
                pictureBoxCharacter.Invalidate();

                buttonEditDB.Enabled = true;
                buttonAddBetterMatch.Enabled = true;
                textBoxText.Enabled = true;
                checkBoxItalic.Enabled = true;
            }
        }

        private void pictureBoxCharacter_Paint(object sender, PaintEventArgs e)
        {
            if (_nOcrChar == null)
            {
                return;
            }

            var foreground = new Pen(new SolidBrush(Color.Green));
            var background = new Pen(new SolidBrush(Color.Red));
            if (pictureBoxCharacter.Image != null)
            {
                foreach (NOcrPoint op in _nOcrChar.LinesForeground)
                {
                    e.Graphics.DrawLine(foreground, op.GetScaledStart(_nOcrChar, pictureBoxCharacter.Width, pictureBoxCharacter.Height), op.GetScaledEnd(_nOcrChar, pictureBoxCharacter.Width, pictureBoxCharacter.Height));
                }
                foreach (NOcrPoint op in _nOcrChar.LinesBackground)
                {
                    e.Graphics.DrawLine(background, op.GetScaledStart(_nOcrChar, pictureBoxCharacter.Width, pictureBoxCharacter.Height), op.GetScaledEnd(_nOcrChar, pictureBoxCharacter.Width, pictureBoxCharacter.Height));
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
            if (_nOcrChar != null)
            {
                _nOcrChar.Text = textBoxText.Text;
                _nOcrChar.Italic = checkBoxItalic.Checked;
                ShowStatus("Character updated");
            }
        }

        private void ShowStatus(string text)
        {
            labelStatus.Text = text;
            labelStatus.Refresh();
            System.Threading.SynchronizationContext.Current.Post(TimeSpan.FromMilliseconds(1500), () =>
            {
                if (!IsDisposed)
                {
                    labelStatus.Text = string.Empty;
                    labelStatus.Refresh();
                }
            });
        }

        private void buttonDelete_Click(object sender, EventArgs e)
        {
            if (_nOcrChar != null)
            {
                _nOcrDb.Remove(_nOcrChar);
                ShowStatus("Character deleted");
            }
        }

        private void buttonAddBetterMatch_Click(object sender, EventArgs e)
        {
            var expandSelectionList = new List<ImageSplitterItem>();
            if (listBoxInspectItems.SelectedIndex < 0)
            {
                return;
            }

            int index = _indexLookup[listBoxInspectItems.SelectedIndex];
            var img = _imageList[index];
            if (img.NikseBitmap == null)
            {
                return;
            }

            var match = _matchList[listBoxInspectItems.SelectedIndex];
            if (match?.ExpandCount > 1) // expand match
            {
                addBetterMultiMatchToolStripMenuItem_Click(null, null);
                return;
            }

            using (var vobSubOcrNOcrCharacter = new VobSubOcrNOcrCharacter())
            {
                var text = textBoxText.Text;
                vobSubOcrNOcrCharacter.Initialize(_bitmap, img, new Point(-1, -1), checkBoxItalic.Checked, true, expandSelectionList.Count > 1, text);
                DialogResult result = vobSubOcrNOcrCharacter.ShowDialog(this);
                var expandSelection = false;
                var shrinkSelection = false;
                if (result == DialogResult.OK && vobSubOcrNOcrCharacter.ExpandSelection)
                {
                    expandSelection = true;
                    if (img.NikseBitmap != null)
                    {
                        expandSelectionList.Add(img);
                    }
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
                        img = VobSubOcr.GetExpandedSelectionNew(new NikseBitmap(_bitmap), expandSelectionList); // true
                    }

                    vobSubOcrNOcrCharacter.Initialize(_bitmap, img, new Point(0, 0), checkBoxItalic.Checked, true, expandSelectionList.Count > 1, string.Empty);
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
                        if (_imageList[index + 1].NikseBitmap != null)
                        {
                            index++;
                            expandSelectionList.Add(_imageList[index]);
                        }
                    }
                }
                if (result == DialogResult.OK)
                {
                    if (expandSelectionList.Count > 1)
                    {
                        vobSubOcrNOcrCharacter.NOcrChar.ExpandCount = expandSelectionList.Count;
                    }

                    _nOcrChars.Add(vobSubOcrNOcrCharacter.NOcrChar);
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
            var form = new VobSubNOcrEdit(_nOcrDb, pictureBoxInspectItem.Image as Bitmap, null);
            form.ShowDialog(this);
        }

        private void addBetterMultiMatchToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (var form = new AddBetterMultiMatchNOcr())
            {
                var tempImageList = new List<ImageSplitterItem>();
                var idx = _indexLookup[listBoxInspectItems.SelectedIndex];
                for (int i = idx; i < _imageList.Count; i++)
                {
                    tempImageList.Add(_imageList[i]);
                }

                var tempMatchList = new List<VobSubOcr.CompareMatch>();
                idx = listBoxInspectItems.SelectedIndex;
                for (int i = idx; i < _matchList.Count; i++)
                {
                    tempMatchList.Add(_matchList[i]);
                }

                form.Initialize(_bitmap, 0, tempMatchList, tempImageList);
                if (form.ShowDialog(this) == DialogResult.OK)
                {
                    using (var charForm = new VobSubOcrNOcrCharacter())
                    {
                        charForm.Initialize(_bitmap, form.ExpandItem, new Point(0, 0), form.ExpandItalic, false, false, form.ExpandText);
                        if (charForm.ShowDialog(this) == DialogResult.OK)
                        {
                            charForm.NOcrChar.ExpandCount = form.ExpandCount;
                            var expandList = tempImageList.Take(form.ExpandCount).ToList();
                            charForm.NOcrChar.MarginTop = expandList.First().Top - expandList.Min(p => p.Top);
                            _nOcrDb.Add(charForm.NOcrChar);
                            DialogResult = DialogResult.OK;
                            return;
                        }
                    }
                }

                DialogResult = DialogResult.Cancel;
            }
        }

        private void checkBoxItalic_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxItalic.Checked)
            {
                labelTextAssociatedWithImage.Font = new Font(labelTextAssociatedWithImage.Font.FontFamily, labelTextAssociatedWithImage.Font.Size, FontStyle.Italic);
                textBoxText.Font = new Font(textBoxText.Font.FontFamily, textBoxText.Font.Size, FontStyle.Italic | FontStyle.Bold);
            }
            else
            {
                labelTextAssociatedWithImage.Font = new Font(labelTextAssociatedWithImage.Font.FontFamily, labelTextAssociatedWithImage.Font.Size);
                textBoxText.Font = new Font(textBoxText.Font.FontFamily, textBoxText.Font.Size, FontStyle.Bold);
            }
        }
    }
}
