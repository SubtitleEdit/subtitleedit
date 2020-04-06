using System;
using System.Windows.Forms;
using Nikse.SubtitleEdit.Core;
using Nikse.SubtitleEdit.Logic;

namespace Nikse.SubtitleEdit.Forms
{
    public partial class BatchConvertMkvEnding : Form
    {
        public string LanguageCodeStyle { get; set; }

        public BatchConvertMkvEnding()
        {
            UiUtil.PreInitialize(this);
            InitializeComponent();
            UiUtil.FixFonts(this);
            buttonCancel.Text = Configuration.Settings.Language.General.Cancel;
            buttonOK.Text = Configuration.Settings.Language.General.Ok;
            radioButton2Letter.Text = Configuration.Settings.Language.BatchConvert.MkvLanguageStyleTwoLetter;
            radioButton3Letter.Text = Configuration.Settings.Language.BatchConvert.MkvLanguageStyleThreeLetter;
            radioButtonNone.Text = Configuration.Settings.Language.BatchConvert.MkvLanguageStyleEmpty;
            UiUtil.FixLargeFonts(this, buttonOK);
        }

        private void BatchConvertMkvEnding_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                DialogResult = DialogResult.Cancel;
            }
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void LanguageCodeChanged(object sender, EventArgs e)
        {
            if (radioButton2Letter.Checked)
            {
                labelFileNameExample.Text = string.Format(Configuration.Settings.Language.General.ExampleX, "Video.en.srt");
                LanguageCodeStyle = "2";
            }
            else if (radioButtonNone.Checked)
            {
                labelFileNameExample.Text = string.Format(Configuration.Settings.Language.General.ExampleX, "Video.srt");
                LanguageCodeStyle = "0";
            }
            else
            {
                labelFileNameExample.Text = string.Format(Configuration.Settings.Language.General.ExampleX, "Video.eng.srt");
                LanguageCodeStyle = "3";
            }
        }

        private void BatchConvertMkvEnding_Load(object sender, EventArgs e)
        {
            if (LanguageCodeStyle == "2")
            {
                radioButton2Letter.Checked = true;
            }
            else if (LanguageCodeStyle == "0")
            {
                radioButtonNone.Checked = true;
            }
            else
            {
                radioButton3Letter.Checked = true;
            }
        }
    }
}
