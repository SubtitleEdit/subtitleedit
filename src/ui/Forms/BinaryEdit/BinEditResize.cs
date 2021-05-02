using Nikse.SubtitleEdit.Logic;
using System;
using System.Drawing;
using System.Windows.Forms;
using Nikse.SubtitleEdit.Core.Common;

namespace Nikse.SubtitleEdit.Forms.BinaryEdit
{
    public sealed partial class BinEditResize : Form
    {
        private readonly Bitmap _bitmap;

        public decimal Factor { get; private set; }
        public bool FixAlignment { get; private set; }
        public ContentAlignment Alignment { get; private set; }

        public BinEditResize(Bitmap bitmap)
        {
            UiUtil.PreInitialize(this);
            InitializeComponent();
            UiUtil.FixFonts(this);

            var nikseBitmap = new NikseBitmap(bitmap);
            nikseBitmap.CropTransparentSidesAndBottom(99999, true);
            nikseBitmap.CropTopTransparent(2);
            _bitmap = nikseBitmap.GetBitmap();

            trackBarResize_Scroll(null, null);
            Factor = 1.0m;
            FixAlignment = true;
            Alignment = ContentAlignment.BottomCenter;
            checkBoxFixAlignment.Checked = true;

            Text = LanguageSettings.Current.BinEdit.ResizeTitle;
            labelResize.Text = string.Format(LanguageSettings.Current.BinEdit.ResizeX, trackBarResize.Value);
            buttonOK.Text = LanguageSettings.Current.General.Ok;
            buttonCancel.Text = LanguageSettings.Current.General.Cancel;
            UiUtil.FixLargeFonts(this, buttonOK);
        }

        private void trackBarResize_Scroll(object sender, EventArgs e)
        {
            Factor = trackBarResize.Value / 100.0m;
            labelResize.Text = string.Format(LanguageSettings.Current.BinEdit.ResizeX, trackBarResize.Value);
            var bmp = ExportPngXml.ResizeBitmap(_bitmap, (int)Math.Round(_bitmap.Width * Factor), (int)Math.Round(_bitmap.Height * Factor));
            pictureBoxResized.Image?.Dispose();
            pictureBoxResized.Image = bmp;

            comboBoxAlignment.Items.Clear();
            comboBoxAlignment.Items.Add(LanguageSettings.Current.SubStationAlphaStyles.TopLeft);
            comboBoxAlignment.Items.Add(LanguageSettings.Current.SubStationAlphaStyles.TopCenter);
            comboBoxAlignment.Items.Add(LanguageSettings.Current.SubStationAlphaStyles.TopRight);

            comboBoxAlignment.Items.Add(LanguageSettings.Current.SubStationAlphaStyles.MiddleLeft);
            comboBoxAlignment.Items.Add(LanguageSettings.Current.SubStationAlphaStyles.MiddleCenter);
            comboBoxAlignment.Items.Add(LanguageSettings.Current.SubStationAlphaStyles.MiddleRight);

            comboBoxAlignment.Items.Add(LanguageSettings.Current.SubStationAlphaStyles.BottomLeft);
            comboBoxAlignment.Items.Add(LanguageSettings.Current.SubStationAlphaStyles.BottomCenter);
            comboBoxAlignment.Items.Add(LanguageSettings.Current.SubStationAlphaStyles.BottomRight);

            comboBoxAlignment.SelectedIndex = 7;
        }

        private void BinEditResize_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                DialogResult = DialogResult.Cancel;
            }
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void checkBoxFixAlignment_CheckedChanged(object sender, EventArgs e)
        {
            comboBoxAlignment.Enabled = checkBoxFixAlignment.Checked;
            FixAlignment = checkBoxFixAlignment.Checked;
        }

        private void comboBoxAlignment_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBoxAlignment.SelectedIndex == 0)
            {
                Alignment = ContentAlignment.TopLeft;
            }
            else if (comboBoxAlignment.SelectedIndex == 1)
            {
                Alignment = ContentAlignment.TopCenter;
            }
            else if (comboBoxAlignment.SelectedIndex == 2)
            {
                Alignment = ContentAlignment.TopRight;
            }

            else if (comboBoxAlignment.SelectedIndex == 3)
            {
                Alignment = ContentAlignment.MiddleLeft;
            }
            else if (comboBoxAlignment.SelectedIndex == 4)
            {
                Alignment = ContentAlignment.MiddleCenter;
            }
            else if (comboBoxAlignment.SelectedIndex == 5)
            {
                Alignment = ContentAlignment.MiddleRight;
            }

            else if (comboBoxAlignment.SelectedIndex == 6)
            {
                Alignment = ContentAlignment.BottomLeft;
            }
            else if (comboBoxAlignment.SelectedIndex == 8)
            {
                Alignment = ContentAlignment.BottomRight;
            }
            else
            {
                Alignment = ContentAlignment.BottomCenter;
            }
        }
    }
}
