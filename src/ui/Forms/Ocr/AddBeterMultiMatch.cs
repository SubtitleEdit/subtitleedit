using Nikse.SubtitleEdit.Core.Common;
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

            labelText.Text = LanguageSettings.Current.General.Text;
            labelImageInfo.Text = LanguageSettings.Current.General.Preview;
            buttonCancel.Text = LanguageSettings.Current.General.Cancel;
            buttonOK.Text = LanguageSettings.Current.General.Ok;
            UiUtil.FixLargeFonts(this, buttonOK);
        }

        public BinaryOcrBitmap ExpandedMatch { get; set; }

        private List<VobSubOcr.CompareMatch> _matches;
        private List<ImageSplitterItem> _splitterItems;
        private int _startIndex;
        int _extraCount;

        internal void Initialize(int selectedIndex, List<VobSubOcr.CompareMatch> matches, List<ImageSplitterItem> splitterItems)
        {
            _startIndex = selectedIndex;
            for (int i = 0; i < selectedIndex; i++)
            {
                if (matches[i].Extra != null && matches[i].Extra.Count > 0)
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
                if (m.Extra?.Count > 0)
                {
                    break;
                }

                if (m.Text != LanguageSettings.Current.VobSubOcr.NoMatch && (m.ImageSplitterItem?.NikseBitmap == null || !string.IsNullOrWhiteSpace(m.ImageSplitterItem.SpecialCharacter)))
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

        private void NumericUpDownExpandCountValueChanged(object sender, System.EventArgs e)
        {
            for (int i = 0; i < listBoxInspectItems.Items.Count; i++)
            {
                listBoxInspectItems.SetSelected(i, i < numericUpDownExpandCount.Value);
            }
            MakeExpandImage();
        }

        private void MakeExpandImage()
        {
            var splitterItem = _splitterItems[_startIndex + _extraCount];
            if (splitterItem.NikseBitmap == null)
            {
                return;
            }

            ExpandedMatch = new BinaryOcrBitmap(new NikseBitmap(splitterItem.NikseBitmap), false, (int)numericUpDownExpandCount.Value, string.Empty, splitterItem.X, splitterItem.Y) { ExpandedList = new List<BinaryOcrBitmap>() };
            for (int i = 1; i < listBoxInspectItems.Items.Count; i++)
            {
                if (i < numericUpDownExpandCount.Value)
                {
                    splitterItem = _splitterItems[_startIndex + i + _extraCount];
                    if (splitterItem.NikseBitmap == null)
                    {
                        break;
                    }

                    ExpandedMatch.ExpandedList.Add(new BinaryOcrBitmap(splitterItem.NikseBitmap, false, 0, null, splitterItem.X, splitterItem.Y));
                }
            }

            var newBmp = ExpandedMatch.ToOldBitmap();
            pictureBoxInspectItem.Image = newBmp;
            pictureBoxInspectItem.Width = newBmp.Width;
            pictureBoxInspectItem.Height = newBmp.Height;
        }

        private void buttonOK_Click(object sender, System.EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(textBoxText.Text))
            {
                return;
            }

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
            {
                DialogResult = DialogResult.Cancel;
            }
        }
    }
}
