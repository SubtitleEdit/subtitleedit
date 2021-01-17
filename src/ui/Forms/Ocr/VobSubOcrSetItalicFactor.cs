using Nikse.SubtitleEdit.Logic;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.Forms.Ocr
{
    public sealed partial class VobSubOcrSetItalicFactor : Form
    {
        private readonly Bitmap _bmp;
        private double _factor;

        public VobSubOcrSetItalicFactor(Bitmap bmp, double factor)
        {
            UiUtil.PreInitialize(this);
            InitializeComponent();
            UiUtil.FixFonts(this);

            _bmp = bmp;
            _factor = factor;
            numericUpDownItalicFactor.Value = (decimal)factor;

            Text = LanguageSettings.Current.VobSubOcrSetItalicAngle.Title;
            labelDescription.Text = LanguageSettings.Current.VobSubOcrSetItalicAngle.Description;
            saveImageAsToolStripMenuItem.Text = LanguageSettings.Current.VobSubOcr.SaveSubtitleImageAs;
            buttonOK.Text = LanguageSettings.Current.General.Ok;
            buttonCancel.Text = LanguageSettings.Current.General.Cancel;
        }

        private void numericUpDownItalicFactor_ValueChanged(object sender, EventArgs e)
        {
            pictureBoxSubtitleImage.Image = VobSubOcr.UnItalic(_bmp, (double)numericUpDownItalicFactor.Value);
        }

        internal double GetUnItalicFactor()
        {
            return _factor;
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            _factor = (double)numericUpDownItalicFactor.Value;
            DialogResult = DialogResult.OK;
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void VobSubOcrSetItalicFactor_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                DialogResult = DialogResult.Cancel;
            }
        }

        private void saveImageAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            saveFileDialog1.Title = LanguageSettings.Current.VobSubOcr.SaveSubtitleImageAs;
            saveFileDialog1.AddExtension = true;
            saveFileDialog1.FileName = "ImageUnItalic";
            saveFileDialog1.Filter = "PNG image|*.png|BMP image|*.bmp|GIF image|*.gif|TIFF image|*.tiff";
            saveFileDialog1.FilterIndex = 0;

            DialogResult result = saveFileDialog1.ShowDialog(this);
            if (result == DialogResult.OK)
            {
                var bmp = (Bitmap)pictureBoxSubtitleImage.Image;
                if (bmp == null)
                {
                    MessageBox.Show("No image!");
                    return;
                }

                try
                {
                    if (saveFileDialog1.FilterIndex == 0)
                    {
                        bmp.Save(saveFileDialog1.FileName, System.Drawing.Imaging.ImageFormat.Png);
                    }
                    else if (saveFileDialog1.FilterIndex == 1)
                    {
                        bmp.Save(saveFileDialog1.FileName);
                    }
                    else if (saveFileDialog1.FilterIndex == 2)
                    {
                        bmp.Save(saveFileDialog1.FileName, System.Drawing.Imaging.ImageFormat.Gif);
                    }
                    else
                    {
                        bmp.Save(saveFileDialog1.FileName, System.Drawing.Imaging.ImageFormat.Tiff);
                    }
                }
                catch (Exception exception)
                {
                    MessageBox.Show(exception.Message);
                }
            }
        }

        private void VobSubOcrSetItalicFactor_Shown(object sender, EventArgs e)
        {
            numericUpDownItalicFactor_ValueChanged(null, null);
        }
    }
}
