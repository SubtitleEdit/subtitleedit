using System;
using System.Drawing;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.Forms
{
    public partial class VobSubOcrSetItalicFactor : Form
    {
        Bitmap _bmp;
        double _factor;

        public VobSubOcrSetItalicFactor(Bitmap bmp, double factor)
        {
            InitializeComponent();

            _bmp = bmp;
            _factor = factor;
            numericUpDown1.Value = (decimal)factor;
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            pictureBoxSubtitleImage.Image = VobSubOcr.UnItalic(_bmp, (double)numericUpDown1.Value);
        }       

        internal double GetUnItalicFactor()
        {
            return _factor;
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            _factor = (double)numericUpDown1.Value;
            DialogResult = DialogResult.OK;
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void VobSubOcrSetItalicFactor_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
                DialogResult = DialogResult.Cancel;
        }
    }
}
