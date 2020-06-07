using Nikse.SubtitleEdit.Core;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Ocr;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.Forms.Ocr
{
    public partial class AddBetterMultiMatchNOcr : Form
    {
        public NOcrChar NOcrChar { get; set; }
        
        private NikseBitmap _wholeImage;
        private List<VobSubOcr.CompareMatch> _matches;
        private List<ImageSplitterItem> _splitterItems;
        private ImageSplitterItem _expandItem;
        private int _startIndex;
        int _extraCount;

        public AddBetterMultiMatchNOcr()
        {
            UiUtil.PreInitialize(this);
            InitializeComponent();
            UiUtil.FixFonts(this);

            labelText.Text = Configuration.Settings.Language.General.Text;
            labelImageInfo.Text = Configuration.Settings.Language.General.Preview;
            buttonCancel.Text = Configuration.Settings.Language.General.Cancel;
            buttonOK.Text = Configuration.Settings.Language.General.Ok;
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

                var m = _matches[i];
                if (m == null || m.Extra?.Count > 0)
                {
                    break;
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

            _expandItem = VobSubOcr.GetExpandedSelectionNew(_wholeImage,expandList);
            var newBmp = _expandItem.NikseBitmap.GetBitmap();
            pictureBoxInspectItem.Image = newBmp;
            pictureBoxInspectItem.Width = newBmp.Width;
            pictureBoxInspectItem.Height = newBmp.Height;
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            using (var form = new VobSubOcrNOcrCharacter())
            {
                form.Initialize(_expandItem.NikseBitmap.GetBitmap(), _expandItem, new Point(0, 0), checkBoxItalic.Checked, false, false, textBoxText.Text);
                var result = form.ShowDialog(this);
                NOcrChar = form.NOcrChar;
                NOcrChar.ExpandCount = (int)numericUpDownExpandCount.Value;
                DialogResult = result;
            }
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
