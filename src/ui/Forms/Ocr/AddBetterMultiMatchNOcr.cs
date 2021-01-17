using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Logic;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.Forms.Ocr
{
    public partial class AddBetterMultiMatchNOcr : Form
    {
        public ImageSplitterItem ExpandItem { get; set; }
        public int ExpandCount { get; set; }
        public bool ExpandItalic { get; set; }
        public string ExpandText { get; set; }

        private NikseBitmap _wholeImage;
        private List<VobSubOcr.CompareMatch> _matches;
        private List<ImageSplitterItem> _splitterItems;
        private int _startIndex;
        private int _extraCount;

        public AddBetterMultiMatchNOcr()
        {
            UiUtil.PreInitialize(this);
            InitializeComponent();
            UiUtil.FixFonts(this);

            labelText.Text = LanguageSettings.Current.General.Text;
            labelImageInfo.Text = LanguageSettings.Current.General.Preview;
            buttonCancel.Text = LanguageSettings.Current.General.Cancel;
            buttonOK.Text = LanguageSettings.Current.General.Ok;
            UiUtil.FixLargeFonts(this, buttonOK);
        }

        internal void Initialize(Bitmap bitmap, int selectedIndex, List<VobSubOcr.CompareMatch> matches, List<ImageSplitterItem> splitterItems)
        {
            _wholeImage = new NikseBitmap(bitmap);
            _startIndex = selectedIndex;
            for (int i = 0; i < selectedIndex; i++)
            {
                if (matches[i] != null && matches[i].Extra != null && matches[i].Extra.Count > 0)
                {
                    _extraCount += matches[i].Extra.Count - 1;
                }
            }

            _matches = matches;
            _splitterItems = splitterItems;
            int count = 0;
            for (int i = _startIndex; i < _splitterItems.Count - _extraCount; i++)
            {
                if (i >= _matches.Count)
                {
                    break;
                }

                var item = _splitterItems[i];
                if (item.SpecialCharacter != null)
                {
                    break;
                }

                var m = _matches[i];
                if (m != null && m.Extra?.Count > 0)
                {
                    break;
                }

                if (m == null)
                {
                    m = new VobSubOcr.CompareMatch(string.Empty, false, 0, null);
                }

                count++;
                listBoxInspectItems.Items.Add(m);
                if (count < 3)
                {
                    listBoxInspectItems.SetSelected(listBoxInspectItems.Items.Count - 1, true);
                }
            }
            numericUpDownExpandCount.Maximum = listBoxInspectItems.Items.Count;
            MakeExpandImage();
        }

        private void NumericUpDownExpandCountValueChanged(object sender, EventArgs e)
        {
            for (int i = 0; i < listBoxInspectItems.Items.Count; i++)
            {
                listBoxInspectItems.SetSelected(i, i < numericUpDownExpandCount.Value);
            }
            MakeExpandImage();
        }

        private void MakeExpandImage()
        {
            var splitterItem = _splitterItems[_startIndex];
            if (splitterItem.NikseBitmap == null)
            {
                return;
            }

            var expandList = new List<ImageSplitterItem> { splitterItem };
            for (int i = 1; i < listBoxInspectItems.Items.Count; i++)
            {
                if (i < numericUpDownExpandCount.Value)
                {
                    splitterItem = _splitterItems[_startIndex + i + _extraCount];
                    if (splitterItem.NikseBitmap == null)
                    {
                        break;
                    }

                    expandList.Add(splitterItem);
                }
            }

            ExpandItem = VobSubOcr.GetExpandedSelectionNew(_wholeImage, expandList);
            var newBmp = ExpandItem.NikseBitmap.GetBitmap();
            pictureBoxInspectItem.Image = newBmp;
            pictureBoxInspectItem.Width = newBmp.Width;
            pictureBoxInspectItem.Height = newBmp.Height;
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            ExpandCount = (int)numericUpDownExpandCount.Value;
            ExpandItalic = checkBoxItalic.Checked;
            ExpandText = textBoxText.Text;
            DialogResult = DialogResult.OK;
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void AddBetterMultiMatch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                DialogResult = DialogResult.Cancel;
            }
        }
    }
}
