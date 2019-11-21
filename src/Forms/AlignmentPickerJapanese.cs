using System;
using System.Windows.Forms;
using Nikse.SubtitleEdit.Core;
using Nikse.SubtitleEdit.Logic;

namespace Nikse.SubtitleEdit.Forms
{
    public sealed partial class AlignmentPickerJapanese : Form
    {
        public string ContentAlignment { get; set; }
        private readonly string _text;

        public AlignmentPickerJapanese(string text)
        {
            UiUtil.PreInitialize(this);
            InitializeComponent();
            UiUtil.FixFonts(this);
            Text = Configuration.Settings.Language.SubStationAlphaStyles.Alignment;
            UiUtil.FixLargeFonts(this, button2);
            _text = text;
        }

        private void AlignmentPickerJapanese_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                DialogResult = DialogResult.Cancel;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            ContentAlignment = string.Empty;
            DialogResult = DialogResult.OK;
        }

        private void button5_Click(object sender, EventArgs e)
        {
            ContentAlignment = @"{\an5}";
            DialogResult = DialogResult.OK;
        }

        private void button8_Click(object sender, EventArgs e)
        {
            ContentAlignment = @"{\an8}";
            DialogResult = DialogResult.OK;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            ContentAlignment = @"{\an7}";
            DialogResult = DialogResult.OK;
        }

        private void button6_Click(object sender, EventArgs e)
        {
            ContentAlignment = @"{\an9}";
            DialogResult = DialogResult.OK;
        }

        private void AlignmentPickerJapanese_Shown(object sender, EventArgs e)
        {
            if (_text.StartsWith(@"{\an8}", StringComparison.Ordinal))
            {
                button8.Focus();
            }
            else if (_text.StartsWith(@"{\an5}", StringComparison.Ordinal))
            {
                button5.Focus();
            }
            else if (_text.StartsWith(@"{\an7", StringComparison.Ordinal))
            {
                button4.Focus();
            }
            else if (_text.StartsWith(@"{\an9", StringComparison.Ordinal))
            {
                button6.Focus();
            }
            else
            {
                button2.Focus();
            }
        }
    }
}
