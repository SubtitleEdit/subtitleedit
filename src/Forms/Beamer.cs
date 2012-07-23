using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Text;
using System.Windows.Forms;
using Nikse.SubtitleEdit.Logic;

namespace Nikse.SubtitleEdit.Forms
{
    public partial class Beamer : Form
    {
        private Subtitle _subtitle;
        private int _index;
        private bool _fullscreen;
        private Color _subtitleColor = Color.White;
        private string _subtitleFontName = "Verdana";
        private float _subtitleFontSize = 75.0f;
        private Color _borderColor = Color.Black;
        private float _borderWidth = 2.0f;
        private bool _isLoading = true;
        private int _marginLeft;
        private int _marginBottom = 25;
        private int _showIndex = -2;
        private double _millisecondsFactor = 1.0;
        private Main _main;
        private bool _noTimerAction;
        private long _videoStartTick;

        public Beamer(Main main, Subtitle subtitle, int index)
        {
            InitializeComponent();
            _main = main;
            _subtitle = subtitle;
            _index = index;

            groupBoxImageSettings.Text = Configuration.Settings.Language.ExportPngXml.ImageSettings;
            labelSubtitleFont.Text = Configuration.Settings.Language.ExportPngXml.FontFamily;
            labelSubtitleFontSize.Text = Configuration.Settings.Language.ExportPngXml.FontSize;
            buttonColor.Text = Configuration.Settings.Language.ExportPngXml.FontColor;
            checkBoxAntiAlias.Text = Configuration.Settings.Language.ExportPngXml.AntiAlias;
            buttonBorderColor.Text = Configuration.Settings.Language.ExportPngXml.BorderColor;
            labelBorderWidth.Text = Configuration.Settings.Language.ExportPngXml.BorderWidth;

            _subtitleFontName = Configuration.Settings.SubtitleBeaming.FontName;
            _subtitleFontSize = Configuration.Settings.SubtitleBeaming.FontSize;
            if (_subtitleFontSize > 100 || _subtitleFontSize < 10)
                _subtitleFontSize = 60;
            _subtitleColor = Configuration.Settings.SubtitleBeaming.FontColor;
            _borderColor = Configuration.Settings.SubtitleBeaming.BorderColor;
            _borderWidth = Configuration.Settings.SubtitleBeaming.BorderWidth;

            panelColor.BackColor = _subtitleColor;
            panelBorderColor.BackColor = _borderColor;

            if (Configuration.Settings.SubtitleBeaming.BorderWidth > 0 && Configuration.Settings.SubtitleBeaming.BorderWidth < 5)
                comboBoxBorderWidth.SelectedIndex = (int)_borderWidth;
            else
                comboBoxBorderWidth.SelectedIndex = 2;
            comboBoxHAlign.SelectedIndex = 1;

            foreach (var x in System.Drawing.FontFamily.Families)
            {
                comboBoxSubtitleFont.Items.Add(x.Name);
                if (string.Compare(x.Name, _subtitleFontName, true) == 0)
                    comboBoxSubtitleFont.SelectedIndex = comboBoxSubtitleFont.Items.Count - 1;
            }
            if (_subtitleFontSize > 10 && _subtitleFontSize < 100)
                comboBoxSubtitleFontSize.SelectedIndex = (int)(_subtitleFontSize -10);
            else
                comboBoxSubtitleFontSize.SelectedIndex = 40;
            _isLoading = false;
            ShowCurrent();
        }

        private void ButtonColorClick(object sender, EventArgs e)
        {
            colorDialog1.Color = panelColor.BackColor;
            if (colorDialog1.ShowDialog() == DialogResult.OK)
            {
                panelColor.BackColor = colorDialog1.Color;
                ShowCurrent();
            }
        }

        private void ButtonBorderColorClick(object sender, EventArgs e)
        {
            colorDialog1.Color = panelBorderColor.BackColor;
            if (colorDialog1.ShowDialog() == DialogResult.OK)
            {
                panelBorderColor.BackColor = colorDialog1.Color;
                ShowCurrent();
            }
        }

        private void ComboBoxSubtitleFontSelectedValueChanged(object sender, EventArgs e)
        {
            ShowCurrent();
        }

        private void ComboBoxSubtitleFontSizeSelectedIndexChanged(object sender, EventArgs e)
        {
            ShowCurrent();
        }

        private void ComboBoxBorderWidthSelectedIndexChanged(object sender, EventArgs e)
        {
            ShowCurrent();
        }

        private void CheckBoxAntiAliasCheckedChanged(object sender, EventArgs e)
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
                    _main.FocusParagraph(_index);
                }
                else
                {
                    pictureBox1.Image = null;
                }
            }
            else
            {
                string text = "Testing 123" + Environment.NewLine + "Subtitle Edit";
                if (_index >= 0 && _index < _subtitle.Paragraphs.Count && _subtitle.Paragraphs[_index].Text.Length > 1)
                {
                    text = _subtitle.Paragraphs[_index].Text;
                    _main.FocusParagraph(_index);
                }
                var bmp = GenerateImageFromTextWithStyle(text);
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

            Configuration.Settings.SubtitleBeaming.FontName = _subtitleFontName;
            Configuration.Settings.SubtitleBeaming.FontSize = (int)_subtitleFontSize;
            Configuration.Settings.SubtitleBeaming.FontColor = _subtitleColor;
            Configuration.Settings.SubtitleBeaming.BorderColor = _borderColor;
            Configuration.Settings.SubtitleBeaming.BorderWidth = (int)_borderWidth;
        }

        private Bitmap GenerateImageFromTextWithStyle(string text)
        {
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
            text = Utilities.RemoveHtmlFontTag(text);

            Font font;
            try
            {
                font = new Font(_subtitleFontName, _subtitleFontSize);
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
                font = new Font("Verdana", _subtitleFontSize);
            }
            var bmp = new Bitmap(400, 200);
            var g = Graphics.FromImage(bmp);

            SizeF textSize = g.MeasureString("Hj!", font);
            var lineHeight = (textSize.Height * 0.64f);
            float italicSpacing = textSize.Width / 8;

            textSize = g.MeasureString(text, font);
            g.Dispose();
            bmp.Dispose();
            bmp = new Bitmap((int)(textSize.Width * 0.8), (int)(textSize.Height * 0.7) + 10);
            g = Graphics.FromImage(bmp);

            var lefts = new List<float>();
            foreach (string line in text.Replace("<i>", "i").Replace("</i>", "i").Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries))
            {
                if (comboBoxHAlign.SelectedIndex == 0) // left
                    lefts.Add(5);
                else
                    lefts.Add((float)(bmp.Width - g.MeasureString(line, font).Width * 0.8) / 2);
            }

            if (checkBoxAntiAlias.Checked)
            {
                g.TextRenderingHint = TextRenderingHint.AntiAlias;
                g.SmoothingMode = SmoothingMode.AntiAlias;
            }
            var sf = new StringFormat {Alignment = StringAlignment.Near, LineAlignment = StringAlignment.Near};
            // draw the text to a path
            var path = new GraphicsPath();

            // display italic
            var sb = new StringBuilder();
            int i = 0;
            bool isItalic = false;
            float left = 5;
            if (lefts.Count >= 0)
                left = lefts[0];
            float top = 5;
            bool newLine = false;
            int lineNumber = 0;
            float leftMargin = left;
            bool italicFromStart = false;
            int pathPointsStart = -1;
            while (i < text.Length)
            {
                if (text.Substring(i).ToLower().StartsWith("<i>"))
                {
                    italicFromStart = i == 0;
                    if (sb.Length > 0)
                    {
                        TextDraw.DrawText(font, sf, path, sb, isItalic, false, false, left, top, ref newLine, leftMargin, ref pathPointsStart);
                    }
                    isItalic = true;
                    i += 2;
                }
                else if (text.Substring(i).ToLower().StartsWith("</i>") && isItalic)
                {
                    TextDraw.DrawText(font, sf, path, sb, isItalic, false, false, left, top, ref newLine, leftMargin, ref pathPointsStart);
                    isItalic = false;
                    i += 3;
                }
                else if (text.Substring(i).StartsWith(Environment.NewLine))
                {
                    TextDraw.DrawText(font, sf, path, sb, isItalic, false, false, left, top, ref newLine, leftMargin, ref pathPointsStart);

                    top += lineHeight;
                    newLine = true;
                    i += Environment.NewLine.Length - 1;
                    lineNumber++;
                    if (lineNumber < lefts.Count)
                    {
                        leftMargin = lefts[lineNumber];
                        left = leftMargin;
                    }
                    if (isItalic)
                        italicFromStart = true;
                }
                else
                {
                    sb.Append(text.Substring(i, 1));
                }
                i++;
            }
            if (sb.Length > 0)
            {
                TextDraw.DrawText(font, sf, path, sb, isItalic, false, false, left, top, ref newLine, leftMargin, ref pathPointsStart);
            }

            if (_borderWidth > 0)
                g.DrawPath(new Pen(_borderColor, _borderWidth), path);
            g.FillPath(new SolidBrush(_subtitleColor), path);
            g.Dispose();
            return bmp;
        }

        private static string RemoveSubStationAlphaFormatting(string s)
        {
            int indexOfBegin = s.IndexOf("{", StringComparison.Ordinal);
            while (indexOfBegin >= 0 && s.IndexOf("}", StringComparison.Ordinal) > indexOfBegin)
            {
                int indexOfEnd = s.IndexOf("}", StringComparison.Ordinal);
                s = s.Remove(indexOfBegin, (indexOfEnd - indexOfBegin) + 1);
                indexOfBegin = s.IndexOf("{", StringComparison.Ordinal);
            }
            return s;
        }

        private void Timer1Tick(object sender, EventArgs e)
        {
            if (_noTimerAction)
                return;

            double positionInMilliseconds = (DateTime.Now.Ticks - _videoStartTick) / 10000; // 10,000 ticks = 1 millisecond
            positionInMilliseconds *= _millisecondsFactor;
            int index = 0;
            foreach (Paragraph p in _subtitle.Paragraphs)
            {
                if (p.StartTime.TotalMilliseconds <= positionInMilliseconds &&
                    p.EndTime.TotalMilliseconds > positionInMilliseconds)
                {
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

        private void ButtonStartClick(object sender, EventArgs e)
        {
            if (_index >= _subtitle.Paragraphs.Count - 1)
            {
                _index = -1;
                _videoStartTick = DateTime.Now.Ticks;
            }
            else if (_index >= 0)
            {
                _videoStartTick = DateTime.Now.Ticks- ((long) (_subtitle.Paragraphs[_index].StartTime.TotalMilliseconds) * 10000); //10,000 ticks = 1 millisecond
            }

            groupBoxImageSettings.Hide();
            buttonStart.Hide();
            FormBorderStyle = FormBorderStyle.None;
            BackColor = Color.Black;
            WindowState = FormWindowState.Maximized;
            _fullscreen = true;
            pictureBox1.Image = null;
            Cursor.Hide();
            _marginBottom = Height - 200;
            timer1.Start();
        }

        private void BeamerKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Modifiers == Keys.Control && e.KeyCode == Keys.Home)
            {
                _index = 0;
                ShowCurrent();
                e.Handled = true;
                return;
            }
            if (e.Modifiers == Keys.None && e.KeyCode == Keys.End)
            {
                _index = _subtitle.Paragraphs.Count-1;
                ShowCurrent();
                e.Handled = true;
                return;
            }

            if (!_fullscreen)
            {
                if (e.KeyCode == Keys.Escape)
                {
                    DialogResult = DialogResult.Cancel;
                }
                else if (e.KeyCode == Keys.Space || (e.KeyCode == Keys.Down && e.Modifiers == Keys.Alt))
                {
                    if (_index < _subtitle.Paragraphs.Count - 1)
                        _index++;
                    ShowCurrent();
                    e.Handled = true;
                }
                else if (e.KeyCode == Keys.Up && e.Modifiers == Keys.Alt)
                {
                    if (_index > 0)
                        _index--;
                    ShowCurrent();
                    e.Handled = true;
                }
                else if (e.Modifiers == Keys.None && e.KeyCode == Keys.PageDown)
                {
                    if (_index < _subtitle.Paragraphs.Count - 21)
                        _index += 20;
                    else
                        _index = _subtitle.Paragraphs.Count-1;
                    ShowCurrent();
                    e.Handled = true;
                }
                else if (e.Modifiers == Keys.None && e.KeyCode == Keys.PageUp)
                {
                    if (_index > 20)
                        _index -= 20;
                    else
                        _index = 0;
                    ShowCurrent();
                    e.Handled = true;
                }
                return;
            }

            if (e.KeyCode == Keys.Escape)
            {
                groupBoxImageSettings.Show();
                buttonStart.Show();
                timer1.Stop();
                Cursor.Show();
                FormBorderStyle = FormBorderStyle.FixedDialog;
                BackColor = Control.DefaultBackColor;
                WindowState = FormWindowState.Normal;
                _showIndex = -2;
                _fullscreen = false;
                ShowCurrent();
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.Pause)
            {
                timer1.Stop();
                timer1.Enabled = false;
            }
            else if (e.KeyCode == Keys.Space || (e.KeyCode == Keys.Down && e.Modifiers == Keys.Alt))
            {
                bool timer1Enabled = timer1.Enabled;
                timer1.Enabled = false;
                System.Threading.Thread.Sleep(100);
                if (_index < _subtitle.Paragraphs.Count - 1)
                    _index++;

                _videoStartTick = DateTime.Now.Ticks - ((long)(_subtitle.Paragraphs[_index].StartTime.TotalMilliseconds) * 10000); //10,000 ticks = 1 millisecond

                ShowCurrent();
                _noTimerAction = false;
                if (timer1Enabled || _fullscreen)
                    timer1.Start();

                e.Handled = true;
            }
            else if (e.KeyCode == Keys.Up && e.Modifiers == Keys.Alt)
            {
                bool timer1Enabled = timer1.Enabled;
                timer1.Enabled = false;
                System.Threading.Thread.Sleep(100);
                if (_index > 0)
                    _index--;
                _videoStartTick = DateTime.Now.Ticks - ((long)(_subtitle.Paragraphs[_index].StartTime.TotalMilliseconds) * 10000); //10,000 ticks = 1 millisecond
                ShowCurrent();
                if (timer1Enabled)
                    timer1.Start();
            }
            else if (e.Modifiers == Keys.None && e.KeyCode == Keys.PageDown)
            {
                if (_index < _subtitle.Paragraphs.Count - 21)
                    _index += 20;
                else
                    _index = _subtitle.Paragraphs.Count - 1;
                ShowCurrent();
                e.Handled = true;
            }
            else if (e.Modifiers == Keys.None && e.KeyCode == Keys.PageUp)
            {
                if (_index > 20)
                    _index -= 20;
                else
                    _index = 0;
                ShowCurrent();
                e.Handled = true;
            }
            else if (e.Modifiers == Keys.None && e.KeyCode == Keys.Add)
            {
                if (comboBoxSubtitleFontSize.SelectedIndex < comboBoxSubtitleFontSize.Items.Count - 1)
                {
                    comboBoxSubtitleFontSize.SelectedIndex = comboBoxSubtitleFontSize.SelectedIndex + 1;
                    ShowCurrent();
                }
            }
            else if (e.Modifiers == Keys.None && e.KeyCode == Keys.Subtract)
            {
                if (comboBoxSubtitleFontSize.SelectedIndex > 0)
                {
                    comboBoxSubtitleFontSize.SelectedIndex = comboBoxSubtitleFontSize.SelectedIndex - 1;
                    ShowCurrent();
                }
            }
            else if (e.Modifiers == Keys.Control && e.KeyCode == Keys.Add)
            {
                _millisecondsFactor += 0.001;
            }
            else if (e.Modifiers == Keys.Control && e.KeyCode == Keys.Subtract)
            {
                _millisecondsFactor -= 0.001;
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

        private void BeamerFormClosing(object sender, FormClosingEventArgs e)
        {
            Cursor.Show();
        }

        private void ComboBoxHAlignSelectedIndexChanged(object sender, EventArgs e)
        {
            ShowCurrent();
        }

    }
}
