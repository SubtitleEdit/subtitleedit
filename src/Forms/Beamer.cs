using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Nikse.SubtitleEdit.Logic;
using System.Drawing.Text;
using System.Drawing.Drawing2D;

namespace Nikse.SubtitleEdit.Forms
{
    public partial class Beamer : Form
    {
        private Subtitle _subtitle;
        private int _index = 0;
        bool _fullscreen = false;
        Color _subtitleColor = Color.White;
        string _subtitleFontName = "Verdana";
        float _subtitleFontSize = 75.0f;
        Color _borderColor = Color.Black;
        float _borderWidth = 2.0f;
        bool _isLoading = true;
        int _marginLeft = 0;
        int _marginBottom = 25;
        int _showIndex = -2;
        double _seconds = 0.0;

        public Beamer(Subtitle subtitle, int index)
        {
            InitializeComponent();
            _subtitle = subtitle;
            _index = index;

            groupBoxImageSettings.Text = Configuration.Settings.Language.ExportPngXml.ImageSettings;
            labelSubtitleFont.Text = Configuration.Settings.Language.ExportPngXml.FontFamily;
            labelSubtitleFontSize.Text = Configuration.Settings.Language.ExportPngXml.FontSize;
            buttonColor.Text = Configuration.Settings.Language.ExportPngXml.FontColor;
            checkBoxAntiAlias.Text = Configuration.Settings.Language.ExportPngXml.AntiAlias;
            buttonBorderColor.Text = Configuration.Settings.Language.ExportPngXml.BorderColor;
            labelBorderWidth.Text = Configuration.Settings.Language.ExportPngXml.BorderWidth;

            panelColor.BackColor = _subtitleColor;
            panelBorderColor.BackColor = _borderColor;
            comboBoxBorderWidth.SelectedIndex = 2;

            foreach (var x in System.Drawing.FontFamily.Families)
            {
                comboBoxSubtitleFont.Items.Add(x.Name);
                if (string.Compare(x.Name, _subtitleFontName, true) == 0)
                    comboBoxSubtitleFont.SelectedIndex = comboBoxSubtitleFont.Items.Count - 1;
            }
            comboBoxSubtitleFontSize.SelectedIndex = 40;
            _isLoading = false;
            ShowCurrent();
        }

        private void buttonColor_Click(object sender, EventArgs e)
        {
            colorDialog1.Color = panelColor.BackColor;
            if (colorDialog1.ShowDialog() == DialogResult.OK)
            {
                panelColor.BackColor = colorDialog1.Color;
                ShowCurrent();
            }
        }

        private void panelColor_MouseClick(object sender, MouseEventArgs e)
        {
            buttonColor_Click(null, null);
        }

        private void buttonBorderColor_Click(object sender, EventArgs e)
        {
            colorDialog1.Color = panelBorderColor.BackColor;
            if (colorDialog1.ShowDialog() == DialogResult.OK)
            {
                panelBorderColor.BackColor = colorDialog1.Color;
                ShowCurrent();
            }
        }

        private void panelBorderColor_MouseClick(object sender, MouseEventArgs e)
        {
            buttonBorderColor_Click(null, null);
        }

        private void comboBoxSubtitleFont_SelectedValueChanged(object sender, EventArgs e)
        {
            ShowCurrent();
        }

        private void comboBoxSubtitleFontSize_SelectedIndexChanged(object sender, EventArgs e)
        {
            ShowCurrent();
        }

        private void comboBoxBorderWidth_SelectedIndexChanged(object sender, EventArgs e)
        {
            ShowCurrent();
        }

        private void checkBoxAntiAlias_CheckedChanged(object sender, EventArgs e)
        {
            ShowCurrent();
        }

        private void ShowCurrent()
        {
            SetupImageParameters();
            if (_fullscreen)
            {
                if (_index > 0 && _index < _subtitle.Paragraphs.Count)
                {
                    var bmp = GenerateImageFromTextWithStyle(_subtitle.Paragraphs[_index].Text);
                    pictureBox1.Image = bmp;
                    pictureBox1.Height = bmp.Height;
                    pictureBox1.Width = bmp.Width;
                    pictureBox1.Left = (Width - bmp.Width) / 2 + _marginLeft;
                    pictureBox1.Top = Height - (pictureBox1.Height + _marginBottom);
                    _showIndex = _index;
                }
                else
                {
                    pictureBox1.Image = null;
                }
            }
            else
            {
                var bmp = GenerateImageFromTextWithStyle("Testing 123" + Environment.NewLine + "Subtitle Edit");
                pictureBox1.Top = groupBoxImageSettings.Top + groupBoxImageSettings.Height + 5;
                pictureBox1.Left = 5;
                pictureBox1.Image = bmp;
                pictureBox1.Height = bmp.Height;
                pictureBox1.Width = bmp.Width;
                _showIndex = -2;
            }
        }

        private void SetupImageParameters()
        {
            if (_isLoading)
                return;

            _subtitleColor = panelColor.BackColor;
            _borderColor = panelBorderColor.BackColor;
            _subtitleFontName = comboBoxSubtitleFont.SelectedItem.ToString();
            _subtitleFontSize = float.Parse(comboBoxSubtitleFontSize.SelectedItem.ToString());
            _borderWidth = float.Parse(comboBoxBorderWidth.SelectedItem.ToString());
        }

        private Bitmap GenerateImageFromTextWithStyle(string text)
        {
            if (string.IsNullOrEmpty(text))
                text = " ";

            // remove styles for display text (except italic)
            text = RemoveSubStationAlphaFormatting(text);
            text = text.Replace("<b>", string.Empty);
            text = text.Replace("</b>", string.Empty);
            text = text.Replace("<B>", string.Empty);
            text = text.Replace("</B>", string.Empty);
            text = text.Replace("<u>", string.Empty);
            text = text.Replace("</u>", string.Empty);
            text = text.Replace("<U>", string.Empty);
            text = text.Replace("</U>", string.Empty);
            text = Logic.Utilities.RemoveHtmlFontTag(text);

            Font font = new System.Drawing.Font(_subtitleFontName, _subtitleFontSize);
            Bitmap bmp = new Bitmap(400, 200);
            Graphics g = Graphics.FromImage(bmp);


            SizeF textSize = g.MeasureString("Hj!", font);
            var lineHeight = (textSize.Height * 0.64f);

            textSize = g.MeasureString(text, font);
            g.Dispose();
            bmp.Dispose();
            bmp = new Bitmap((int)(textSize.Width * 0.8), (int)(textSize.Height * 0.7) + 10);
            g = Graphics.FromImage(bmp);

            if (checkBoxAntiAlias.Checked)
            {
                g.TextRenderingHint = TextRenderingHint.AntiAlias;
                g.SmoothingMode = SmoothingMode.AntiAlias;
            }
            StringFormat sf = new StringFormat();
            sf.Alignment = StringAlignment.Near;
            sf.LineAlignment = StringAlignment.Near;// draw the text to a path            
            System.Drawing.Drawing2D.GraphicsPath path = new System.Drawing.Drawing2D.GraphicsPath();

            // display italic
            StringBuilder sb = new StringBuilder();
            int i = 0;
            bool isItalic = false;
            int left = 5;
            float top = 5;
            bool newLine = false;
            while (i < text.Length)
            {
                if (text.Substring(i).ToLower().StartsWith("<i>"))
                {
                    if (sb.Length > 0)
                        DrawText(font, sf, path, sb, isItalic, left, top, ref newLine);
                    isItalic = true;
                    i += 2;
                }
                else if (text.Substring(i).ToLower().StartsWith("</i>") && isItalic)
                {
                    DrawText(font, sf, path, sb, isItalic, left, top, ref newLine);
                    isItalic = false;
                    i += 3;
                }
                else if (text.Substring(i).StartsWith(Environment.NewLine))
                {
                    DrawText(font, sf, path, sb, isItalic, left, top, ref newLine);
                    top += lineHeight;
                    newLine = true;
                    i += Environment.NewLine.Length - 1;
                }
                else
                {
                    sb.Append(text.Substring(i, 1));
                }
                i++;
            }
            if (sb.Length > 0)
            {
                DrawText(font, sf, path, sb, isItalic, left, top, ref newLine);
            }

            if (_borderWidth > 0)
                g.DrawPath(new Pen(_borderColor, _borderWidth), path);
            g.FillPath(new SolidBrush(_subtitleColor), path);
            g.Dispose();
            return bmp;
        }

        private static void DrawText(Font font, StringFormat sf, System.Drawing.Drawing2D.GraphicsPath path, StringBuilder sb, bool isItalic, int left, float top, ref bool newLine)
        {
            PointF next = new PointF(left, top);
            if (path.PointCount > 0)
                next.X = path.GetLastPoint().X;
            if (newLine)
            {
                next.X = 5;
                newLine = false;
            }

            if (isItalic)
                path.AddString(sb.ToString(), font.FontFamily, (int)System.Drawing.FontStyle.Italic, font.Size, next, sf);
            else
                path.AddString(sb.ToString(), font.FontFamily, 0, font.Size, next, sf);

            sb.Length = 0;
        }

        private static string RemoveSubStationAlphaFormatting(string s)
        {
            int indexOfBegin = s.IndexOf("{");
            while (indexOfBegin >= 0 && s.IndexOf("}") > indexOfBegin)
            {
                int indexOfEnd = s.IndexOf("}");
                s = s.Remove(indexOfBegin, (indexOfEnd - indexOfBegin) + 1);
                indexOfBegin = s.IndexOf("{");
            }
            return s;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            _seconds += 0.025;
            double positionInMilliseconds = (_seconds * 1000.0) + 15.0;
            string text = string.Empty;
            int index = 0;
            foreach (Paragraph p in _subtitle.Paragraphs)
            {
                if (p.StartTime.TotalMilliseconds <= positionInMilliseconds &&
                    p.EndTime.TotalMilliseconds > positionInMilliseconds)
                {
                    text = p.Text.Replace("|", Environment.NewLine);
                    break;
                }
                index++;
            }
            if (index == _subtitle.Paragraphs.Count)
                index = -1;

            if (index == -1)
            {
                pictureBox1.Image = null;
            }
            else if (index != _showIndex)
            {
                _index = index;
                ShowCurrent();
            }
        }

        private void buttonStart_Click(object sender, EventArgs e)
        {
            _index = -1;
            groupBoxImageSettings.Hide();
            buttonStart.Hide();
            _seconds = 0.0;
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            BackColor = Color.Black;
            WindowState = FormWindowState.Maximized;
            _fullscreen = true;
            pictureBox1.Image = null;
            Cursor.Hide();
            _marginBottom = Height - 200;
            timer1.Start();
        }

        private void Beamer_KeyDown(object sender, KeyEventArgs e)
        {
            if (!_fullscreen)
                return;

            if (e.KeyCode == Keys.Escape)
            {
                groupBoxImageSettings.Show();
                buttonStart.Show();
                timer1.Stop();
                Cursor.Show();
                FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
                BackColor = Control.DefaultBackColor;
                WindowState = FormWindowState.Normal;
                _showIndex = -2;
                _fullscreen = false;
                ShowCurrent();
            }
            else if (e.KeyCode == Keys.Pause)
            {
                timer1.Stop();
            }
            else if (e.KeyCode == Keys.Space || (e.KeyCode == Keys.Down && e.Modifiers == Keys.Alt))
            {
                timer1.Stop();
                if (_index < _subtitle.Paragraphs.Count - 1)
                    _index++;
                _seconds = _subtitle.Paragraphs[_index].StartTime.TotalSeconds;
                ShowCurrent();
                timer1.Start();
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.Up && e.Modifiers == Keys.Alt)
            {
                if (_index > 0)
                    _index--;
                _seconds = _subtitle.Paragraphs[_index].StartTime.TotalSeconds;
                ShowCurrent();
            }
            else if (e.KeyCode == Keys.Add)
            {
                if (comboBoxSubtitleFontSize.SelectedIndex < comboBoxSubtitleFontSize.Items.Count -1)
                {
                    comboBoxSubtitleFontSize.SelectedIndex = comboBoxSubtitleFontSize.SelectedIndex + 1;
                    ShowCurrent();
                }
            }
            else if (e.KeyCode == Keys.Subtract)
            {
                if (comboBoxSubtitleFontSize.SelectedIndex > 0)
                {
                    comboBoxSubtitleFontSize.SelectedIndex = comboBoxSubtitleFontSize.SelectedIndex - 1;                    
                    ShowCurrent();
                }
            }
            else if (e.KeyCode == Keys.Up)
            {
                _marginBottom++;
                ShowCurrent();
            }
            else if (e.KeyCode == Keys.Down)
            {
                _marginBottom--;
                ShowCurrent();
            }
            else if (e.KeyCode == Keys.Left)
            {
                _marginLeft--;
                ShowCurrent();
            }
            else if (e.KeyCode == Keys.Right)
            {
                _marginLeft++;
                ShowCurrent();
            }
            else if (e.KeyCode == Keys.Back || e.KeyCode ==  Keys.Delete)
            {
                timer1.Stop();
                int temp = _index;
                _index = -1;
                ShowCurrent();
                _index = temp;
            }
        }

        private void Beamer_FormClosing(object sender, FormClosingEventArgs e)
        {
            Cursor.Show();
        }

    }
}
