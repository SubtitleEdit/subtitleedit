using Nikse.SubtitleEdit.Logic;
using System;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.Forms
{
    public sealed partial class BatchConvertMkvEnding : Form
    {
        public string LanguageCodeStyle { get; set; }

        public BatchConvertMkvEnding()
        {
            UiUtil.PreInitialize(this);
            InitializeComponent();
            UiUtil.FixFonts(this);
            buttonCancel.Text = LanguageSettings.Current.General.Cancel;
            buttonOK.Text = LanguageSettings.Current.General.Ok;
            radioButton2Letter.Text = LanguageSettings.Current.BatchConvert.MkvLanguageStyleTwoLetter;
            radioButton3Letter.Text = LanguageSettings.Current.BatchConvert.MkvLanguageStyleThreeLetter;
            radioButtonNone.Text = LanguageSettings.Current.BatchConvert.MkvLanguageStyleEmpty;
            labelFileNameEnding.Text = LanguageSettings.Current.BatchConvert.MkvLanguageInOutputFileName;
            UiUtil.FixLargeFonts(this, buttonOK);
        }

        private void SetMkvLanguageTitle()
        {
            var styleName = LanguageSettings.Current.BatchConvert.MkvLanguageStyleThreeLetter;
            if (radioButton2Letter.Checked)
            {
                styleName = LanguageSettings.Current.BatchConvert.MkvLanguageStyleTwoLetter;
            }
            else if (radioButtonNone.Checked)
            {
                styleName = LanguageSettings.Current.BatchConvert.MkvLanguageStyleEmpty;
            }

            Text = string.Format(LanguageSettings.Current.BatchConvert.MkvLanguageInOutputFileNameX, styleName);
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
                labelFileNameExample.Text = string.Format(LanguageSettings.Current.General.ExampleX, "Video.en.srt");
                LanguageCodeStyle = "2";
            }
            else if (radioButtonNone.Checked)
            {
                labelFileNameExample.Text = string.Format(LanguageSettings.Current.General.ExampleX, "Video.srt");
                LanguageCodeStyle = "0";
            }
            else
            {
                labelFileNameExample.Text = string.Format(LanguageSettings.Current.General.ExampleX, "Video.eng.srt");
                LanguageCodeStyle = "3";
            }

            SetMkvLanguageTitle();
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
            SetMkvLanguageTitle();
        }
    }
}
