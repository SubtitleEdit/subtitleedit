using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.Common.TextEffect;
using Nikse.SubtitleEdit.Logic;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.Forms
{
    public sealed partial class EffectKaraoke : Form
    {
        private Paragraph _paragraph;
        private List<Paragraph> _animation;
        private int _timerCount;

        public EffectKaraoke()
        {
            UiUtil.PreInitialize(this);
            InitializeComponent();
            UiUtil.FixFonts(this);

            Text = LanguageSettings.Current.EffectKaraoke.Title;
            labelChooseColor.Text = LanguageSettings.Current.EffectKaraoke.ChooseColor;
            labelTM.Text = LanguageSettings.Current.EffectKaraoke.TotalSeconds;
            labelEndDelay.Text = LanguageSettings.Current.EffectKaraoke.EndDelayInSeconds;
            buttonPreview.Text = LanguageSettings.Current.General.Preview;
            buttonOK.Text = LanguageSettings.Current.General.Ok;
            buttonCancel.Text = LanguageSettings.Current.General.Cancel;
            radioButtonByWordEffect.Text = LanguageSettings.Current.EffectKaraoke.WordEffect;
            radioButtonByCharEffect.Text = LanguageSettings.Current.EffectKaraoke.CharacterEffect;

            UiUtil.FixLargeFonts(this, buttonOK);
        }

        private void FormEffectKaraoke_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                DialogResult = DialogResult.Cancel;
            }
        }

        internal void Initialize(Paragraph paragraph)
        {
            _paragraph = new Paragraph(paragraph);
            if (_paragraph.Text.Length > 3)
            {
                _paragraph.Text = HtmlUtil.FixUpperTags(_paragraph.Text);
            }
            _animation = new List<Paragraph>();

            panelColor.BackColor = Color.Red;
            labelColor.Text = Utilities.ColorToHex(panelColor.BackColor);

            AddToPreview(richTextBoxPreview, paragraph.Text);
            RefreshPreview();
            labelTotalMilliseconds.Text = $"{paragraph.DurationTotalMilliseconds / TimeCode.BaseUnit:#,##0.000}";
            numericUpDownDelay.Maximum = (decimal)((paragraph.DurationTotalMilliseconds - 500) / TimeCode.BaseUnit);
            numericUpDownDelay.Minimum = 0;

            numericUpDownDelay.Left = labelEndDelay.Left + labelEndDelay.Width + 5;
        }

        private static void AddToPreview(RichTextBox rtb, string text)
        {
            SetRtbHtml.SetText(rtb, text.Replace("\r\n", "\n"));
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
            using (var colorChooser = new ColorChooser { Color = panelColor.BackColor, ShowAlpha = false })
            {
                if (colorChooser.ShowDialog() == DialogResult.OK)
                {
                    labelColor.Text = Utilities.ColorToHex(colorChooser.Color);
                    panelColor.BackColor = colorChooser.Color;
                }
            }
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

        public List<Paragraph> MakeAnimation(Paragraph paragraph)
        {
            _paragraph = paragraph;
            MakeAnimation();
            return _animation;
        }

        private void MakeAnimation()
        {
            _animation = new List<Paragraph>();
            if (HtmlUtil.RemoveHtmlTags(_paragraph.Text, true).Length == 0 || _paragraph.DurationTotalMilliseconds < 0.001)
            {
                _animation.Add(new Paragraph(_paragraph));
                return;
            }

            var delaySeconds = (double)numericUpDownDelay.Value;
            var karaokeEffect = new KaraokeEffect(SelectStrategy());
            _animation.AddRange(karaokeEffect.Transform(_paragraph, panelColor.BackColor, delaySeconds * TimeCode.BaseUnit));
        }

        private TextEffectBase SelectStrategy()
        {
            if (radioButtonByCharEffect.Checked)
            {
                return new KaraokeCharTransform();
            }

            return new KaraokeWordTransform();
        }

        private void Timer1Tick(object sender, EventArgs e)
        {
            _timerCount += timer1.Interval;
            var s = GetText(_timerCount, _animation);
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
            foreach (var p in animation)
            {
                if (p.StartTime.TotalMilliseconds <= milliseconds && p.EndTime.TotalMilliseconds >= milliseconds)
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
