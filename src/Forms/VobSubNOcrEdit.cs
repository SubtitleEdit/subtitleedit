using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.OCR;

namespace Nikse.SubtitleEdit.Forms
{
    public partial class VobSubNOcrEdit : Form
    {

        private List<NOcrChar> _nocrChars;
        private NOcrChar _nocrChar = null;
        private double _zoomFactor = 3.0;
        private Bitmap bitmap;        

        public VobSubNOcrEdit(List<NOcrChar> nocrChars, Bitmap bitmap)
        {
            InitializeComponent();

            this._nocrChars = nocrChars;
            this.bitmap = bitmap;

            List<string> list = new List<string>();
            foreach (NOcrChar c in _nocrChars)
            {
                if (!list.Contains(c.Text))
                    list.Add(c.Text);
            }
            list.Sort();
            comboBoxTexts.Items.Clear();
            foreach (string s in list)
            {
                comboBoxTexts.Items.Add(s);
            }

            if (bitmap != null)
            {
                pictureBoxCharacter.Image = bitmap;
                SizePictureBox();                
            }
        }

        private void VobSubNOcrEdit_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                DialogResult = DialogResult.Cancel;
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

        private bool IsMatch()
        {
            NikseBitmap nbmp = new NikseBitmap(pictureBoxCharacter.Image as Bitmap);
            var bmp = pictureBoxCharacter.Image as Bitmap;
            foreach (NOcrPoint op in _nocrChar.LinesForeground)
            {
                foreach (Point point in op.ScaledGetPoints(_nocrChar, nbmp.Width, nbmp.Height))
                {
                    if (point.X >= 0 && point.Y >= 0 && point.X < nbmp.Width && point.Y < nbmp.Height)
                    {
                        Color c = nbmp.GetPixel(point.X, point.Y);
                        if (c.A > 150 && c.R > 100 && c.G > 100 && c.B > 100)
                        {
                        }
                        else
                        {
                            return false;
                        }
                    }
                }
            }
            foreach (NOcrPoint op in _nocrChar.LinesBackground)
            {
                foreach (Point point in op.ScaledGetPoints(_nocrChar, nbmp.Width, nbmp.Height))
                {
                    if (point.X >= 0 && point.Y >= 0 && point.X < nbmp.Width && point.Y < nbmp.Height)
                    {
                        Color c = nbmp.GetPixel(point.X, point.Y);
                        if (c.A > 150 && c.R > 100 && c.G > 100 && c.B > 100)
                        {
                            return false;
                        }
                    }
                }
            }
            return true;
        }

        private void listBoxFileNames_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBoxFileNames.SelectedIndex < 0)
                return;

            _nocrChar = listBoxFileNames.Items[listBoxFileNames.SelectedIndex] as NOcrChar;
            if (_nocrChar == null)
            {
                pictureBoxCharacter.Invalidate();
                groupBoxCurrentCompareImage.Enabled = false;
            }
            else
            {
                textBoxText.Text = _nocrChar.Text;
                checkBoxItalic.Checked = _nocrChar.Italic;
                pictureBoxCharacter.Invalidate();
                groupBoxCurrentCompareImage.Enabled = true;

                if (pictureBoxCharacter.Image != null)
                {
                    if (IsMatch())
                    {
                        groupBoxCurrentCompareImage.BackColor = Color.LightGreen;
                    }
                    else
                    {
                        groupBoxCurrentCompareImage.BackColor = Control.DefaultBackColor;
                    }
                }
                
            }
        }

        private void comboBoxTexts_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBoxTexts.SelectedIndex < 0)
                return;

            listBoxFileNames.Items.Clear();
            string text = comboBoxTexts.Items[comboBoxTexts.SelectedIndex].ToString();
            foreach (NOcrChar c in _nocrChars)
            {
                if (c.Text == text)
                {
                    listBoxFileNames.Items.Add(c);
                }
            }
            if (listBoxFileNames.Items.Count > 0)
                listBoxFileNames.SelectedIndex = 0;
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

        private Point MakePointItalic(Point p, int height)
        {
            int _italicMoveRight = 2;
            double _unItalicFactor = 0.2;            
            return new Point((int)Math.Round(p.X + (height - p.Y) * _unItalicFactor - _italicMoveRight), p.Y);
        }

        private void buttonMakeItalic_Click(object sender, EventArgs e)
        {
            var c = new NOcrChar();

            foreach (NOcrPoint op in _nocrChar.LinesForeground)
            {
                c.LinesForeground.Add(new NOcrPoint(MakePointItalic(op.Start, _nocrChar.Height), MakePointItalic(op.End, _nocrChar.Height)));
            }
            foreach (NOcrPoint op in _nocrChar.LinesBackground)
            {
                c.LinesBackground.Add(new NOcrPoint(MakePointItalic(op.Start, _nocrChar.Height), MakePointItalic(op.End, _nocrChar.Height)));
            }
            c.Text = _nocrChar.Text;
            c.Width = _nocrChar.Width;
            c.Height = _nocrChar.Height;
            c.MarginTop = _nocrChar.MarginTop;
            _nocrChar = c;
            pictureBoxCharacter.Invalidate();

            if (IsMatch())
            {
                groupBoxCurrentCompareImage.BackColor = Color.LightGreen;
            }
            else
            {
                groupBoxCurrentCompareImage.BackColor = Control.DefaultBackColor;
            }

        }

    }
}
