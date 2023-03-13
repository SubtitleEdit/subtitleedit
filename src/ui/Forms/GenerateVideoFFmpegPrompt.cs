using Nikse.SubtitleEdit.Logic;
using System;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.Forms
{
    public sealed partial class GenerateVideoFFmpegPrompt : Form
    {
        public string Parameters { get; private set; }

        public GenerateVideoFFmpegPrompt(string title, string parameters)
        {
            UiUtil.PreInitialize(this);
            InitializeComponent();
            UiUtil.FixFonts(this);

            Text = title;
            textBox1.Text = parameters.Trim();

          //TODO:  labelFFmpeg.Text = LanguageSettings.Current.GenerateBlankVideo.DurationInMinutes;
            buttonOK.Text = LanguageSettings.Current.Watermark.Generate;
            buttonCancel.Text = LanguageSettings.Current.General.Cancel;
        }
        
        private void buttonOK_Click(object sender, EventArgs e)
        {
            Parameters = textBox1.Text;
            DialogResult = DialogResult.OK;
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void GenerateVideo_Shown(object sender, EventArgs e)
        {
            buttonOK.Show();
        }
    }
}
