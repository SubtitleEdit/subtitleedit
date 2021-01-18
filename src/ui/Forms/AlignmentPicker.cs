using Nikse.SubtitleEdit.Logic;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.Forms
{
    public sealed partial class AlignmentPicker : Form
    {
        public ContentAlignment Alignment { get; private set; }
        private readonly string _text;

        public AlignmentPicker(string text)
        {
            UiUtil.PreInitialize(this);
            InitializeComponent();
            UiUtil.FixFonts(this);
            Text = LanguageSettings.Current.SubStationAlphaStyles.Alignment;

            button1.Text = LanguageSettings.Current.SubStationAlphaStyles.TopLeft;
            button2.Text = LanguageSettings.Current.SubStationAlphaStyles.TopCenter;
            button3.Text = LanguageSettings.Current.SubStationAlphaStyles.TopRight;

            button4.Text = LanguageSettings.Current.SubStationAlphaStyles.MiddleLeft;
            button5.Text = LanguageSettings.Current.SubStationAlphaStyles.MiddleCenter;
            button6.Text = LanguageSettings.Current.SubStationAlphaStyles.MiddleRight;

            button7.Text = LanguageSettings.Current.SubStationAlphaStyles.BottomLeft;
            button8.Text = LanguageSettings.Current.SubStationAlphaStyles.BottomCenter;
            button9.Text = LanguageSettings.Current.SubStationAlphaStyles.BottomRight;

            UiUtil.FixLargeFonts(this, button1);
            _text = text;
        }

        public void Done()
        {
            DialogResult = DialogResult.OK;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Alignment = ContentAlignment.TopLeft;
            Done();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Alignment = ContentAlignment.TopCenter;
            Done();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            Alignment = ContentAlignment.TopRight;
            Done();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            Alignment = ContentAlignment.MiddleLeft;
            Done();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            Alignment = ContentAlignment.MiddleCenter;
            Done();
        }

        private void button6_Click(object sender, EventArgs e)
        {
            Alignment = ContentAlignment.MiddleRight;
            Done();
        }

        private void button7_Click(object sender, EventArgs e)
        {
            Alignment = ContentAlignment.BottomLeft;
            Done();
        }

        private void button8_Click(object sender, EventArgs e)
        {
            Alignment = ContentAlignment.BottomCenter;
            Done();
        }

        private void button9_Click(object sender, EventArgs e)
        {
            Alignment = ContentAlignment.BottomRight;
            Done();
        }

        private void AlignmentPicker_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                DialogResult = DialogResult.Cancel;
            }
        }

        private void AlignmentPicker_Shown(object sender, EventArgs e)
        {
            if (_text.StartsWith("{\\an7}", StringComparison.Ordinal))
            {
                button1.Focus();
            }
            else if (_text.StartsWith("{\\an8}", StringComparison.Ordinal))
            {
                button2.Focus();
            }
            else if (_text.StartsWith("{\\an9}", StringComparison.Ordinal))
            {
                button3.Focus();
            }
            else if (_text.StartsWith("{\\an4}", StringComparison.Ordinal))
            {
                button4.Focus();
            }
            else if (_text.StartsWith("{\\an5}", StringComparison.Ordinal))
            {
                button5.Focus();
            }
            else if (_text.StartsWith("{\\an6}", StringComparison.Ordinal))
            {
                button6.Focus();
            }
            else if (_text.StartsWith("{\\an1}", StringComparison.Ordinal))
            {
                button7.Focus();
            }
            else if (_text.StartsWith("{\\an3}", StringComparison.Ordinal))
            {
                button9.Focus();
            }
            else
            {
                button8.Focus();
            }
        }
    }
}
