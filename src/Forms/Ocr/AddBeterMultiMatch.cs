﻿using Nikse.SubtitleEdit.Core;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Ocr.Binary;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.Forms.Ocr
{
    public partial class AddBeterMultiMatch : Form
    {
        public AddBeterMultiMatch()
        {
            UiUtil.PreInitialize(this);
            InitializeComponent();
            UiUtil.FixFonts(this);
        }

        public BinaryOcrBitmap ExpandedMatch { get; set; }

        private List<VobSubOcr.CompareMatch> _matches;
        private List<ImageSplitterItem> _splitterItems;
        private int _startIndex;

        internal void Initialize(int selectedIndex, List<VobSubOcr.CompareMatch> matches, List<ImageSplitterItem> splitterItems)
        {
            _startIndex = selectedIndex;
            _matches = matches;
            _splitterItems = splitterItems;
            int count = 0;
            for (int i = selectedIndex; i < _splitterItems.Count; i++)
            {
                var m = _matches[i];
                if (m.Text != Configuration.Settings.Language.VobSubOcr.NoMatch && (m.ImageSplitterItem?.NikseBitmap == null || !string.IsNullOrWhiteSpace(m.ImageSplitterItem.SpecialCharacter)))
                    break;
                count++;
                listBoxInspectItems.Items.Add(m);
                if (count < 3)
                    listBoxInspectItems.SetSelected(listBoxInspectItems.Items.Count - 1, true);
            }
            numericUpDownExpandCount.Maximum = listBoxInspectItems.Items.Count;
            MakeExpandImage();
        }

        private void NumericUpDownExpandCountValueChanged(object sender, System.EventArgs e)
        {
            for (int i = 0; i < listBoxInspectItems.Items.Count; i++)
            {
                listBoxInspectItems.SetSelected(listBoxInspectItems.Items.Count - 1, i < numericUpDownExpandCount.Value);
            }
            MakeExpandImage();
        }

        private void MakeExpandImage()
        {
            var splitterItem = _splitterItems[_startIndex];
            if (splitterItem.NikseBitmap == null)
                return;
            ExpandedMatch = new BinaryOcrBitmap(new NikseBitmap(splitterItem.NikseBitmap), false, (int)numericUpDownExpandCount.Value, string.Empty, splitterItem.X, splitterItem.Y) { ExpandedList = new List<BinaryOcrBitmap>() };
            for (int i = 1; i < listBoxInspectItems.Items.Count; i++)
            {
                if (i < numericUpDownExpandCount.Value)
                {
                    splitterItem = _splitterItems[_startIndex + i];
                    if (splitterItem.NikseBitmap == null)
                        break;
                    ExpandedMatch.ExpandedList.Add(new BinaryOcrBitmap(splitterItem.NikseBitmap, false, 0, null, splitterItem.X, splitterItem.Y));
                }
            }
            pictureBoxInspectItem.Image = ExpandedMatch.ToOldBitmap();
        }

        private void buttonOK_Click(object sender, System.EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(textBoxText.Text))
                return;

            ExpandedMatch.Italic = checkBoxItalic.Checked;
            ExpandedMatch.Text = textBoxText.Text;
            DialogResult = DialogResult.OK;
        }

        private void buttonCancel_Click(object sender, System.EventArgs e)
        {
            ExpandedMatch = null;
            DialogResult = DialogResult.Cancel;
        }

        private void AddBeterMultiMatch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
                DialogResult = DialogResult.Cancel;
        }
    }
}
