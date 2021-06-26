using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Logic;
using System;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.Forms
{
    public sealed partial class GenerateVideo : Form
    {

        public string VideoFileName { get; private set; }
        private bool _abort;

        public GenerateVideo(Subtitle subtitle)
        {
            UiUtil.PreInitialize(this);
            InitializeComponent();
            UiUtil.FixFonts(this);

            numericUpDownDurationMinutes.Value = Configuration.Settings.Tools.BlankVideoMinutes;
            panelColor.BackColor = Configuration.Settings.Tools.BlankVideoColor;
            if (Configuration.Settings.Tools.BlankVideoUseCheckeredImage)
            {
                radioButtonCheckeredImage.Checked = true;
            }
            else
            {
                radioButtonColor.Checked = true;
            }

            //Text = LanguageSettings.Current.AssaSetPosition.SetPosition;
            //buttonOK.Text = LanguageSettings.Current.General.ge;
            labelPleaseWait.Text = LanguageSettings.Current.General.PleaseWait;
            buttonCancel.Text = LanguageSettings.Current.General.Cancel;
            progressBar1.Visible = false;
            labelPleaseWait.Visible = false;
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            buttonOK.Enabled = false;
            var fileName = radioButtonColor.Checked ? "blank_video_solid" : "blank_video_checkered";
            using (var saveDialog = new SaveFileDialog { FileName = fileName, Filter = "Matroska|*.mkv" })
            {
                if (saveDialog.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }

                VideoFileName = saveDialog.FileName;
            }

            progressBar1.Style = ProgressBarStyle.Marquee;
            progressBar1.Visible = true;
            labelPleaseWait.Visible = true;
            var process = VideoPreviewGenerator.GenerateVideoFile(
                VideoFileName,
                (int)Math.Round(numericUpDownDurationMinutes.Value * 60),
                (int)numericUpDownWidth.Value,
                (int)numericUpDownHeight.Value,
                panelColor.BackColor,
                radioButtonCheckeredImage.Checked);

            process.Start();
            while (!process.HasExited)
            {
                System.Threading.Thread.Sleep(100);
                Application.DoEvents();
                if (_abort)
                {
                    process.Kill();
                }
            }

            progressBar1.Visible = false;
            labelPleaseWait.Visible = false;
            DialogResult = _abort ? DialogResult.Cancel : DialogResult.OK;
        }

        private void buttonColor_Click(object sender, EventArgs e)
        {
            using (var colorChooser = new ColorChooser { Color = panelColor.BackColor, ShowAlpha = false })
            {
                if (colorChooser.ShowDialog() == DialogResult.OK)
                {
                    panelColor.BackColor = colorChooser.Color;
                }
            }
        }

        private void panelColor_MouseClick(object sender, MouseEventArgs e)
        {
            buttonColor_Click(null, null);
        }

        private void GenerateVideo_FormClosing(object sender, FormClosingEventArgs e)
        {
            Configuration.Settings.Tools.BlankVideoColor = panelColor.BackColor;
            Configuration.Settings.Tools.BlankVideoUseCheckeredImage = radioButtonCheckeredImage.Checked;
            Configuration.Settings.Tools.BlankVideoMinutes = (int)numericUpDownDurationMinutes.Value;
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            _abort = true;
            if (buttonOK.Enabled)
            {
                DialogResult = DialogResult.Cancel;
            }
        }
    }
}
