﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Core;

namespace Nikse.SubtitleEdit.Forms
{
    public sealed partial class EffectTypewriter : Form
    {
        private Paragraph _paragraph;
        private List<Paragraph> _animation;
        private int _timerCount;

        public EffectTypewriter()
        {
            InitializeComponent();

            Text = Configuration.Settings.Language.EffectTypewriter.Title;
            labelTM.Text = Configuration.Settings.Language.EffectKaraoke.TotalMilliseconds;
            labelEndDelay.Text = Configuration.Settings.Language.EffectKaraoke.EndDelayInMilliseconds;
            buttonPreview.Text = Configuration.Settings.Language.General.Preview;
            buttonOK.Text = Configuration.Settings.Language.General.Ok;
            buttonCancel.Text = Configuration.Settings.Language.General.Cancel;
            Utilities.FixLargeFonts(this, buttonOK);
        }

        public List<Paragraph> TypewriterParagraphs
        {
            get
            {
                return _animation;
            }
        }

        private void FormEffectTypewriter_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
                DialogResult = DialogResult.Cancel;
        }

        internal void Initialize(Paragraph paragraph)
        {
            _paragraph = paragraph;

            AddToPreview(richTextBoxPreview, paragraph.Text);
            RefreshPreview();

            labelTotalMilliseconds.Text = string.Format("{0:#,##0.000}", paragraph.Duration.TotalMilliseconds / TimeCode.BaseUnit);
            numericUpDownDelay.Maximum = (decimal)((paragraph.Duration.TotalMilliseconds - 500) / TimeCode.BaseUnit);
            numericUpDownDelay.Minimum = 0;

            numericUpDownDelay.Left = labelEndDelay.Left + labelEndDelay.Width + 5;
        }

        private List<EffectKaraoke.ColorEntry> _colorList;
        private List<EffectKaraoke.FontEntry> _fontList;

        private void AddToPreview(RichTextBox rtb, string text)
        {
            richTextBoxPreview.ForeColor = Color.White;
            _colorList = new List<EffectKaraoke.ColorEntry>();
            _fontList = new List<EffectKaraoke.FontEntry>();

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
                        else if (tag.StartsWith("<font ", StringComparison.Ordinal))
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

                _colorList.Add(new EffectKaraoke.ColorEntry { Start = length, Length = text.Length, Color = string.IsNullOrWhiteSpace(color) ? Color.White : ColorTranslator.FromHtml(color) });

                var fontStyle = new FontStyle();
                if (underline)
                    fontStyle |= FontStyle.Underline;
                if (italic)
                    fontStyle |= FontStyle.Italic;
                if (bold)
                    fontStyle |= FontStyle.Bold;
                _fontList.Add(new EffectKaraoke.FontEntry { Start = length, Length = text.Length, Font = new Font(rtb.Font.FontFamily, rtb.Font.Size, fontStyle) });
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

        private void ButtonPreviewClick(object sender, EventArgs e)
        {
            MakeAnimation();
            PlayAnimation();
        }

        private void PlayAnimation()
        {
            _timerCount = (int)_paragraph.StartTime.TotalMilliseconds;
            timer1.Start();
        }

        private static double CalculateStepLength(string text, double duration)
        {
            text = HtmlUtil.RemoveHtmlTags(text);
            return duration / text.Length;
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
            string beforeEndTag = string.Empty;
            string alignment = string.Empty;
            while (i < _paragraph.Text.Length)
            {
                if (i == 0 && _paragraph.Text.StartsWith("{\\", StringComparison.Ordinal) && _paragraph.Text.IndexOf('}') > 2)
                {
                    int idx = _paragraph.Text.IndexOf('}');
                    alignment = _paragraph.Text.Substring(0, idx + 1);
                    i = idx;
                }
                else if (tagOn)
                {
                    tag += _paragraph.Text[i];
                    if (_paragraph.Text[i] == '>')
                    {
                        tagOn = false;
                        if (tag.StartsWith("<font ", StringComparison.InvariantCultureIgnoreCase))
                        {
                            beforeEndTag = "</font>";
                        }
                        else if (tag == "<i>")
                        {
                            beforeEndTag = "</i>";
                        }
                        else if (tag == "<b>")
                        {
                            beforeEndTag = "</b>";
                        }
                        else if (tag == "<u>")
                        {
                            beforeEndTag = "</u>";
                        }
                    }
                }
                else if (_paragraph.Text[i] == '<')
                {
                    tagOn = true;
                    tag += _paragraph.Text[i];
                    beforeEndTag = string.Empty;
                }
                else
                {
                    text += tag + _paragraph.Text[i];
                    tag = string.Empty;

                    startMilliseconds = index * stepsLength;
                    startMilliseconds += _paragraph.StartTime.TotalMilliseconds;
                    endMilliseconds = ((index + 1) * stepsLength) - 1;
                    endMilliseconds += _paragraph.StartTime.TotalMilliseconds;
                    start = new TimeCode(startMilliseconds);
                    end = new TimeCode(endMilliseconds);
                    _animation.Add(new Paragraph(start, end, alignment + text + beforeEndTag));
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
                _animation.Add(new Paragraph(start, end, _paragraph.Text));
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
