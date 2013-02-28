using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.OCR;

namespace Nikse.SubtitleEdit.Forms
{
    public partial class VobSubOcrNOcrCharacter : Form
    {

        VobSubOcr _vobSubForm;
        List<VobSubOcr.ImageCompareAddition> _additions;
        NOcrChar _nocrChar = null;
        bool _drawLineOn;
        bool _startDone;
        Point _start;
        Point _end;
        double _zoomFactor = 1.0;
        int _imageWidth;
        int _imageHeight;
        int _mx;
        int _my;

        public VobSubOcrNOcrCharacter()
        {
            InitializeComponent();
            FixLargeFonts();
        }

        private void FixLargeFonts()
        {
            Graphics graphics = this.CreateGraphics();
            SizeF textSize = graphics.MeasureString(buttonCancel.Text, this.Font);
            if (textSize.Height > buttonCancel.Height - 4)
            {
                int newButtonHeight = (int)(textSize.Height + 7 + 0.5);
                Utilities.SetButtonHeight(this, newButtonHeight, 1);
            }
        }

        public NOcrChar NOcrChar
        {
            get
            {
                return _nocrChar;
            }
        }

        public Point FormPosition
        {
            get
            {
                return new Point(Left, Top);
            }
        }

        public bool ExpandSelection { get; private set; }

        public bool ShrinkSelection { get; private set; }

        internal void Initialize(Bitmap vobSubImage, ImageSplitterItem character, Point position, bool showShrink, VobSubOcr.CompareMatch bestGuess, List<VobSubOcr.ImageCompareAddition> additions, VobSubOcr vobSubForm)
        {
            radioButtonHot.Checked = true;
            ShrinkSelection = false;
            ExpandSelection = false;

            textBoxCharacters.Text = string.Empty;
            _vobSubForm = vobSubForm;
            _additions = additions;
            _nocrChar = new NOcrChar();
            _imageWidth = character.Bitmap.Width;
            _imageHeight = character.Bitmap.Height;

            buttonShrinkSelection.Visible = showShrink;

            if (position.X != -1 && position.Y != -1)
            {
                StartPosition = FormStartPosition.Manual;
                Left = position.X;
                Top = position.Y;
            }

            pictureBoxSubtitleImage.Image = vobSubImage;
            pictureBoxCharacter.Image = character.Bitmap;

            Bitmap org = (Bitmap)vobSubImage.Clone();
            Bitmap bm = new Bitmap(org.Width, org.Height);
            Graphics g = Graphics.FromImage(bm);
            g.DrawImage(org, 0, 0, org.Width, org.Height);
            g.DrawRectangle(Pens.Red, character.X, character.Y, character.Bitmap.Width, character.Bitmap.Height - 1);
            g.Dispose();
            pictureBoxSubtitleImage.Image = bm;

            pictureBoxCharacter.Top = labelCharacters.Top + 16;
            SizePictureBox();
        }

        private void buttonExpandSelection_Click(object sender, EventArgs e)
        {
            ExpandSelection = true;
            DialogResult = DialogResult.OK;
        }

        private void buttonShrinkSelection_Click(object sender, EventArgs e)
        {
            ShrinkSelection = true;
            DialogResult = DialogResult.OK;
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            if (textBoxCharacters.Text.Length == 0)
            {
                MessageBox.Show("Text is empty!");
                return;
            }
            _nocrChar.Text = textBoxCharacters.Text;
            DialogResult = DialogResult.OK;
        }

        public bool IsItalic { get; set; }

        private void pictureBoxCharacter_Paint(object sender, PaintEventArgs e)
        {
            var foreground = new Pen(new SolidBrush(Color.Green));
            var background = new Pen(new SolidBrush(Color.Red));
            if (pictureBoxCharacter.Image != null)
            {
                foreach (NOcrPoint op in _nocrChar.LinesForeground)
                {
                    e.Graphics.DrawLine(foreground, op.GetStart(pictureBoxCharacter.Width, pictureBoxCharacter.Height), op.GetEnd(pictureBoxCharacter.Width, pictureBoxCharacter.Height));
                }
                foreach (NOcrPoint op in _nocrChar.LinesBackground)
                {
                    e.Graphics.DrawLine(background, op.GetStart(pictureBoxCharacter.Width, pictureBoxCharacter.Height), op.GetEnd(pictureBoxCharacter.Width, pictureBoxCharacter.Height));
                }
            }

            if (_drawLineOn)
            {
                if (_startDone)
                {
                    var p = foreground;
                    if (radioButtonCold.Checked)
                        p = background;
                    e.Graphics.DrawLine(p, new Point((int)Math.Round(_start.X * _zoomFactor), (int)Math.Round(_start.Y * _zoomFactor)), new Point(_mx, _my));
                }
            }
        }

        private void SizePictureBox()
        {
            pictureBoxCharacter.SizeMode = PictureBoxSizeMode.StretchImage;
            pictureBoxCharacter.Width = (int)Math.Round(_imageWidth * _zoomFactor);
            pictureBoxCharacter.Height = (int)Math.Round(_imageHeight * _zoomFactor);
            pictureBoxCharacter.Invalidate();
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

        private void ShowOcrPoints()
        {
            listBoxLinesForeground.Items.Clear();
            foreach (NOcrPoint op in _nocrChar.LinesForeground)
            {
                listBoxLinesForeground.Items.Add(op);
            }
            listBoxlinesBackground.Items.Clear();
            foreach (NOcrPoint op in _nocrChar.LinesBackground)
            {
                listBoxlinesBackground.Items.Add(op);
            }
            pictureBoxCharacter.Invalidate();
        }

        private void pictureBoxCharacter_MouseClick(object sender, MouseEventArgs e)
        {
            if (_drawLineOn)
            {
                if (_startDone)
                {
                    _end = new Point((int)Math.Round(e.Location.X / _zoomFactor), (int)Math.Round(e.Location.Y / _zoomFactor));
                    _nocrChar.Width = pictureBoxCharacter.Image.Height * 100.0 / pictureBoxCharacter.Image.Width;
                    if (radioButtonHot.Checked)
                        _nocrChar.LinesForeground.Add(new NOcrPoint(_start, _end, _imageWidth, _imageHeight));
                    else
                        _nocrChar.LinesBackground.Add(new NOcrPoint(_start, _end, _imageWidth, _imageHeight));
                    _drawLineOn = false;
                    pictureBoxCharacter.Invalidate();
                    ShowOcrPoints();
                }
                else
                {
                    _start = new Point((int)Math.Round(e.Location.X / _zoomFactor), (int)Math.Round(e.Location.Y / _zoomFactor));
                    _startDone = true;
                    pictureBoxCharacter.Invalidate();
                }
            }
            else
            {
                _startDone = false;
                _drawLineOn = true;
                _start = new Point((int)Math.Round(e.Location.X / _zoomFactor), (int)Math.Round(e.Location.Y / _zoomFactor));
                _startDone = true;
                pictureBoxCharacter.Invalidate();
            }
        }

        private void pictureBoxCharacter_MouseMove(object sender, MouseEventArgs e)
        {
            if (_drawLineOn)
            {
                _mx = e.X;
                _my = e.Y;
                pictureBoxCharacter.Invalidate();
            }
        }

        private void VobSubOcrNOcrCharacter_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                _drawLineOn = false;
                _startDone = false;
                pictureBoxCharacter.Invalidate();
            }
        }

        private void removeForegroundToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listBoxlinesBackground.SelectedItems.Count == 1)
            {
                var op = listBoxlinesBackground.Items[listBoxlinesBackground.SelectedIndex] as NOcrPoint;
                _nocrChar.LinesBackground.Remove(op);
            }
            ShowOcrPoints();
        }

        private void removeForegroundToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            if (listBoxLinesForeground.SelectedItems.Count == 1)
            {
                var op = listBoxLinesForeground.Items[listBoxLinesForeground.SelectedIndex] as NOcrPoint;
                _nocrChar.LinesForeground.Remove(op);
            }
            ShowOcrPoints();
        }

    }
}
