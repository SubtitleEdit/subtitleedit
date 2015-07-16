﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Nikse.SubtitleEdit.Core;
using Nikse.SubtitleEdit.Logic;

namespace Nikse.SubtitleEdit.Forms
{
    public sealed partial class EffectKaraoke : Form
    {
        private Paragraph _paragraph;
        private List<Paragraph> _animation;
        private int _timerCount;

        public EffectKaraoke()
        {
            InitializeComponent();

            Text = Configuration.Settings.Language.EffectKaraoke.Title;
            labelChooseColor.Text = Configuration.Settings.Language.EffectKaraoke.ChooseColor;
            labelTM.Text = Configuration.Settings.Language.EffectKaraoke.TotalMilliseconds;
            labelEndDelay.Text = Configuration.Settings.Language.EffectKaraoke.EndDelayInMilliseconds;
            buttonPreview.Text = Configuration.Settings.Language.General.Preview;
            buttonOK.Text = Configuration.Settings.Language.General.Ok;
            buttonCancel.Text = Configuration.Settings.Language.General.Cancel;
            Utilities.FixLargeFonts(this, buttonOK);
        }

        private void FormEffectkaraoke_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
                DialogResult = DialogResult.Cancel;
        }

        internal void Initialize(Paragraph paragraph)
        {
            _paragraph = paragraph;
            _animation = new List<Paragraph>();

            colorDialog1.Color = Color.Red;
            labelColor.Text = Utilities.ColorToHex(colorDialog1.Color);
            panelColor.BackColor = colorDialog1.Color;

            AddToPreview(richTextBoxPreview, paragraph.Text);
            RefreshPreview();
            labelTotalMilliseconds.Text = string.Format("{0:#,##0.000}", paragraph.Duration.TotalMilliseconds / 1000);
            numericUpDownDelay.Maximum = (int)((paragraph.Duration.TotalMilliseconds - 500) / 1000);
            numericUpDownDelay.Minimum = 0;

            numericUpDownDelay.Left = labelEndDelay.Left + labelEndDelay.Width + 5;
        }

        internal class ColorEntry
        {
            public int Start { get; set; }
            public int Length { get; set; }
            public Color Color { get; set; }
        }

        internal class FontEntry
        {
            public int Start { get; set; }
            public int Length { get; set; }
            public Font Font { get; set; }
        }

        private List<ColorEntry> _colorList;
        private List<FontEntry> _fontList;

        private void AddToPreview(RichTextBox rtb, string text)
        {
            richTextBoxPreview.ForeColor = Color.White;
            _colorList = new List<ColorEntry>();
            _fontList = new List<FontEntry>();

            int bold = 0;
            int underline = 0;
            int italic = 0;
            var fontColors = new Stack<string>();
            string currentColor = string.Empty;

            var sb = new StringBuilder();
            int i = 0;
            while (i < text.Length)
            {
                if (text[i] == '<')
                {
                    AddTextToRichTextBox(rtb, bold > 0, italic > 0, underline > 0, currentColor, sb.ToString());
                    sb.Clear();
                    string tag = GetTag(text.Substring(i).ToLower());
                    if (i + 1 < text.Length && text[i + 1] == '/')
                    {
                        if (tag == "</i>" && italic > 0)
                            italic--;
                        else if (tag == "</b>" && bold > 0)
                            bold--;
                        else if (tag == "<u>" && underline > 0)
                            underline--;
                        else if (tag == "</font>")
                            currentColor = fontColors.Count > 0 ? fontColors.Pop() : string.Empty;
                    }
                    else
                    {
                        if (tag == "<i>")
                            italic++;
                        else if (tag == "<b>")
                            bold++;
                        else if (tag == "<u>")
                            underline++;
                        else if (tag.StartsWith("<font "))
                        {
                            const string colorTag = " color=";
                            if (tag.Contains(colorTag))
                            {
                                string tempColor = string.Empty;
                                var start = tag.IndexOf(colorTag, StringComparison.Ordinal);
                                int j = start + colorTag.Length;
                                if (@"""'".Contains(tag[j]))
                                    j++;
                                while (j < tag.Length && (@"#" + Utilities.LowercaseLettersWithNumbers).Contains(tag[j]))
                                {
                                    tempColor += tag[j];
                                    j++;
                                }
                                if (!string.IsNullOrEmpty(currentColor))
                                    fontColors.Push(currentColor);
                                currentColor = tempColor;
                            }
                        }
                    }
                    i += tag.Length;
                }
                else
                {
                    sb.Append(text[i]);
                    i++;
                }
            }
            if (sb.Length > 0)
                AddTextToRichTextBox(rtb, bold > 0, italic > 0, underline > 0, currentColor, sb.ToString());

            foreach (var fontEntry in _fontList)
            {
                rtb.SelectionStart = fontEntry.Start;
                rtb.SelectionLength = fontEntry.Length;
                rtb.SelectionFont = fontEntry.Font;
                rtb.DeselectAll();
            }

            foreach (var colorEntry in _colorList)
            {
                rtb.SelectionStart = colorEntry.Start;
                rtb.SelectionLength = colorEntry.Length;
                rtb.SelectionColor = colorEntry.Color;
                rtb.DeselectAll();
            }
        }

        private void AddTextToRichTextBox(RichTextBox rtb, bool bold, bool italic, bool underline, string color, string text)
        {
            if (text.Length > 0)
            {
                int length = rtb.Text.Length;
                richTextBoxPreview.Text += text;

                _colorList.Add(new ColorEntry { Start = length, Length = text.Length, Color = string.IsNullOrWhiteSpace(color) ? Color.White : ColorTranslator.FromHtml(color) });
 
                var fontStyle = new FontStyle();
                if (underline)
                    fontStyle = fontStyle | FontStyle.Underline;
                if (italic)
                    fontStyle = fontStyle | FontStyle.Italic;
                if (bold)
                    fontStyle = fontStyle | FontStyle.Bold;
                _fontList.Add(new FontEntry { Start = length, Length = text.Length, Font = new Font(rtb.Font.FontFamily, rtb.Font.Size, fontStyle) });
            }
        }

        private static string GetTag(string text)
        {
            var sb = new StringBuilder();
            for (int i = 0; i < text.Length; i++)
            {
                sb.Append(text[i]);
                if (text[i] == '>')
                    return sb.ToString();
            }
            return sb.ToString();
        }

        private void ClearPreview()
        {
            richTextBoxPreview.Text = string.Empty;
        }

        private void RefreshPreview()
        {
            richTextBoxPreview.SelectAll();
            richTextBoxPreview.SelectionAlignment = HorizontalAlignment.Center;
            richTextBoxPreview.Refresh();
        }

        private void ButtonChooseColorClick(object sender, EventArgs e)
        {
            if (colorDialog1.ShowDialog() == DialogResult.OK)
            {
                labelColor.Text = Utilities.ColorToHex(colorDialog1.Color);
                panelColor.BackColor = colorDialog1.Color;
            }
        }

        private void ButtonPreviewClick(object sender, EventArgs e)
        {
            MakeAnimation();
            PlayAnimation();
        }

        private void PlayAnimation()
        {
            _colorList = new List<ColorEntry>();
            _fontList = new List<FontEntry>();
            _timerCount = (int)_paragraph.StartTime.TotalMilliseconds;
            timer1.Start();
        }

        private static double CalculateStepLength(string text, double duration)
        {
            text = HtmlUtil.RemoveHtmlTags(text);
            return duration / text.Length;
        }

        public List<Paragraph> MakeAnimation(Paragraph paragraph)
        {
            _paragraph = paragraph;
            MakeAnimation();
            return _animation;
        }

        private void MakeAnimation()
        {
            _animation = new List<Paragraph>();
            double duration = _paragraph.Duration.TotalMilliseconds - ((double)numericUpDownDelay.Value * TimeCode.BaseUnit);
            double stepsLength = CalculateStepLength(_paragraph.Text, duration);

            double startMilliseconds;
            double endMilliseconds;
            TimeCode start;
            TimeCode end;
            int index = 0;
            string text = string.Empty;
            bool tagOn = false;
            string tag = string.Empty;
            int i = 0;
            string startFontTag = string.Format("<font color=\"{0}\">", Utilities.ColorToHex(panelColor.BackColor));
            const string endFontTag = "</font>";
            string afterCurrentTag = string.Empty;
            string beforeEndTag = string.Empty;
            string alignment = string.Empty;
            while (i < _paragraph.Text.Length)
            {
                var c = _paragraph.Text[i];
                if (i == 0 && _paragraph.Text.StartsWith("{\\", StringComparison.Ordinal) && _paragraph.Text.IndexOf('}') > 2)
                {
                    int idx = _paragraph.Text.IndexOf('}');
                    alignment = _paragraph.Text.Substring(0, idx + 1);
                    i = idx;
                }
                else if (tagOn)
                {
                    tag += c;
                    if (_paragraph.Text[i] == '>')
                    {
                        tagOn = false;
                        if (tag.StartsWith("<font ", StringComparison.InvariantCultureIgnoreCase))
                        {
                            afterCurrentTag = tag;
                            tag = string.Empty;
                        }
                        else if (tag == "<i>")
                        {
                            afterCurrentTag = "<i>";
                            beforeEndTag = "</i>";
                        }
                        else if (tag == "<b>")
                        {
                            afterCurrentTag = "<b>";
                            beforeEndTag = "</b>";
                        }
                        else if (tag == "<u>")
                        {
                            afterCurrentTag = "<u>";
                            beforeEndTag = "</u>";
                        }
                    }
                }
                else if (_paragraph.Text[i] == '<')
                {
                    afterCurrentTag = string.Empty;
                    tagOn = true;
                    tag += c;
                    beforeEndTag = string.Empty;
                }
                else
                {
                    text += tag + c;
                    tag = string.Empty;
                    string afterText = string.Empty;
                    if (i + 1 < _paragraph.Text.Length)
                        afterText = _paragraph.Text.Substring(i + 1);
                    if (string.Compare(afterText, endFontTag, StringComparison.InvariantCultureIgnoreCase) == 0 && afterCurrentTag.StartsWith("<font", StringComparison.InvariantCultureIgnoreCase))
                    {
                        afterText = string.Empty;
                        afterCurrentTag = string.Empty;
                    }
                    string tempText = alignment + startFontTag + text + beforeEndTag + endFontTag + afterCurrentTag + afterText;
                    tempText = tempText.Replace("<i></i>", string.Empty);
                    tempText = tempText.Replace("<u></u>", string.Empty);
                    tempText = tempText.Replace("<b></b>", string.Empty);

                    startMilliseconds = index * stepsLength;
                    startMilliseconds += _paragraph.StartTime.TotalMilliseconds;
                    endMilliseconds = ((index + 1) * stepsLength) - 1;
                    endMilliseconds += _paragraph.StartTime.TotalMilliseconds;
                    start = new TimeCode(startMilliseconds);
                    end = new TimeCode(endMilliseconds);
                    _animation.Add(new Paragraph(start, end, tempText));
                    index++;
                }
                i++;
            }

            if (numericUpDownDelay.Value > 0)
            {
                startMilliseconds = index * stepsLength;
                startMilliseconds += _paragraph.StartTime.TotalMilliseconds;
                endMilliseconds = _paragraph.EndTime.TotalMilliseconds;
                start = new TimeCode(startMilliseconds);
                end = new TimeCode(endMilliseconds);
                _animation.Add(new Paragraph(start, end, startFontTag + _paragraph.Text + endFontTag));
            }
            else if (_animation.Count > 0)
            {
                _animation[_animation.Count - 1].EndTime.TotalMilliseconds = _paragraph.EndTime.TotalMilliseconds;
            }
        }

        private void Timer1Tick(object sender, EventArgs e)
        {
            _timerCount += timer1.Interval;

            string s = GetText(_timerCount, _animation);
            ClearPreview();
            AddToPreview(richTextBoxPreview, s);
            RefreshPreview();

            if (_timerCount > _paragraph.EndTime.TotalMilliseconds)
            {
                timer1.Stop();
                System.Threading.Thread.Sleep(200);
                ClearPreview();
                AddToPreview(richTextBoxPreview, _paragraph.Text);
            }
        }

        private static string GetText(int milliseconds, IEnumerable<Paragraph> animation)
        {
            foreach (Paragraph p in animation)
            {
                if (p.StartTime.TotalMilliseconds <= milliseconds &&
                    p.EndTime.TotalMilliseconds >= milliseconds)
                {
                    return p.Text;
                }
            }
            return string.Empty;
        }

        private void ButtonOkClick(object sender, EventArgs e)
        {
            MakeAnimation();
            DialogResult = DialogResult.OK;
        }

    }
}
