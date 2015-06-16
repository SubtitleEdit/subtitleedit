using System;
using System.Collections.Generic;
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

            labelPreview.Text = paragraph.Text;
            labelTotalMilliseconds.Text = string.Format("{0:#,##0.000}", paragraph.Duration.TotalMilliseconds / 1000);
            numericUpDownDelay.Maximum = (decimal)((paragraph.Duration.TotalMilliseconds - 500) / 1000);
            numericUpDownDelay.Minimum = 0;

            numericUpDownDelay.Left = labelEndDelay.Left + labelEndDelay.Width + 5;
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
            while (i < _paragraph.Text.Length)
            {
                if (tagOn)
                {
                    if (_paragraph.Text[i] == '>')
                        tagOn = false;
                    tag += _paragraph.Text[i];
                }
                else if (_paragraph.Text[i] == '<')
                {
                    tagOn = true;
                    tag += _paragraph.Text[i];
                }
                else
                {
                    text += tag + _paragraph.Text[i];
                    tag = string.Empty;

                    //end tag
                    if (i + 2 < _paragraph.Text.Length &&
                        _paragraph.Text[i + 1] == '<' &&
                        _paragraph.Text[i + 2] == '/')
                    {
                        while (i < _paragraph.Text.Length && _paragraph.Text[i] != '>')
                        {
                            tag += _paragraph.Text[i];
                            i++;
                        }
                        text += tag;
                    }

                    startMilliseconds = index * stepsLength;
                    startMilliseconds += _paragraph.StartTime.TotalMilliseconds;
                    endMilliseconds = ((index + 1) * stepsLength) - 1;
                    endMilliseconds += _paragraph.StartTime.TotalMilliseconds;
                    start = new TimeCode(startMilliseconds);
                    end = new TimeCode(endMilliseconds);
                    _animation.Add(new Paragraph(start, end, text));
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
            labelPreview.Text = s;
            labelPreview.Refresh();

            if (_timerCount > _paragraph.EndTime.TotalMilliseconds)
            {
                timer1.Stop();
                System.Threading.Thread.Sleep(200);
                labelPreview.Text = _paragraph.Text;
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
