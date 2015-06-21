using System;
using System.Windows.Forms;
using Nikse.SubtitleEdit.Logic;

namespace Nikse.SubtitleEdit.Forms
{
    public partial class ChooseResolution : Form
    {
        public int VideoWidth { get; set; }
        public int VideoHeight { get; set; }

        public ChooseResolution()
        {
            InitializeComponent();
            buttonOK.Text = Configuration.Settings.Language.General.Ok;
            buttonCancel.Text = Configuration.Settings.Language.General.Cancel;
            Text = Configuration.Settings.Language.ExportPngXml.VideoResolution;
            labelVideoResolution.Text = Configuration.Settings.Language.ExportPngXml.VideoResolution;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            openFileDialog1.Title = Configuration.Settings.Language.General.OpenVideoFileTitle;
            openFileDialog1.FileName = string.Empty;
            openFileDialog1.Filter = Utilities.GetVideoFileFilter(false);
            openFileDialog1.FileName = string.Empty;
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                VideoInfo info = Utilities.GetVideoInfo(openFileDialog1.FileName);
                if (info != null && info.Success)
                {
                    numericUpDownVideoWidth.Value = info.Width;
                    numericUpDownVideoHeight.Value = info.Height;
                }
            }
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            VideoWidth = (int)numericUpDownVideoWidth.Value;
            VideoHeight = (int)numericUpDownVideoHeight.Value;
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void ChooseResolution_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
                DialogResult = DialogResult.Cancel;
        }

    }
}