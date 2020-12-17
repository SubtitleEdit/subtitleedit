using Nikse.SubtitleEdit.Core.Common;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.Forms.BinaryEdit
{
    public partial class BinEditResize : Form
    {
        private readonly Bitmap _bitmap;

        public decimal Factor { get; private set; }
        public bool FixAlignment { get; private set; }
        public ContentAlignment Alignment { get; private set; }

        public BinEditResize(Bitmap bitmap)
        {
            InitializeComponent();

            _bitmap = bitmap;
            trackBarResize_Scroll(null, null);
            Factor = 1.0m;
            FixAlignment = true;
            Alignment = ContentAlignment.BottomCenter;
            checkBoxFixAlignment.Checked = true;
        }

        private void trackBarResize_Scroll(object sender, EventArgs e)
        {
            Factor = trackBarResize.Value / 100.0m;
            labelResize.Text = "Resize in % - " + trackBarResize.Value + "%";
            var bmp = ExportPngXml.ResizeBitmap(_bitmap, (int)Math.Round(_bitmap.Width * Factor), (int)Math.Round(_bitmap.Height * Factor));
            pictureBoxResized.Image?.Dispose();
            pictureBoxResized.Image = bmp;

            comboBoxAlignment.Items.Clear();
            comboBoxAlignment.Items.Add(Configuration.Settings.Language.SubStationAlphaStyles.TopLeft);
            comboBoxAlignment.Items.Add(Configuration.Settings.Language.SubStationAlphaStyles.TopCenter);
            comboBoxAlignment.Items.Add(Configuration.Settings.Language.SubStationAlphaStyles.TopRight);

            comboBoxAlignment.Items.Add(Configuration.Settings.Language.SubStationAlphaStyles.MiddleLeft);
            comboBoxAlignment.Items.Add(Configuration.Settings.Language.SubStationAlphaStyles.MiddleCenter);
            comboBoxAlignment.Items.Add(Configuration.Settings.Language.SubStationAlphaStyles.MiddleRight);

            comboBoxAlignment.Items.Add(Configuration.Settings.Language.SubStationAlphaStyles.BottomLeft);
            comboBoxAlignment.Items.Add(Configuration.Settings.Language.SubStationAlphaStyles.BottomCenter);
            comboBoxAlignment.Items.Add(Configuration.Settings.Language.SubStationAlphaStyles.BottomRight);

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
