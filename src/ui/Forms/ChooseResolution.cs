using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Logic;
using System;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.Forms
{
    public sealed partial class ChooseResolution : Form
    {
        public int VideoWidth { get; set; }
        public int VideoHeight { get; set; }

        public ChooseResolution()
        {
            UiUtil.PreInitialize(this);
            InitializeComponent();
            UiUtil.FixFonts(this);
            buttonOK.Text = LanguageSettings.Current.General.Ok;
            buttonCancel.Text = LanguageSettings.Current.General.Cancel;
            Text = LanguageSettings.Current.ExportPngXml.VideoResolution;
            labelVideoResolution.Text = LanguageSettings.Current.ExportPngXml.VideoResolution;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            openFileDialog1.Title = LanguageSettings.Current.General.OpenVideoFileTitle;
            openFileDialog1.FileName = string.Empty;
            openFileDialog1.Filter = UiUtil.GetVideoFileFilter(false);
            openFileDialog1.FileName = string.Empty;
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                VideoInfo info = UiUtil.GetVideoInfo(openFileDialog1.FileName);
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
            {
                DialogResult = DialogResult.Cancel;
            }
        }

    }
}
