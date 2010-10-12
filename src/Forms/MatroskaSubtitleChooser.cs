using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Nikse.SubtitleEdit.Logic;

namespace Nikse.SubtitleEdit.Forms
{
    public sealed partial class MatroskaSubtitleChooser : Form
    {
        public MatroskaSubtitleChooser()
        {
            InitializeComponent();

            Text = Configuration.Settings.Language.MatroskaSubtitleChooser.Title;
            labelChoose.Text = Configuration.Settings.Language.MatroskaSubtitleChooser.PleaseChoose;
            buttonOK.Text = Configuration.Settings.Language.General.OK;
            buttonCancel.Text = Configuration.Settings.Language.General.Cancel;
        }

        public int SelectedIndex
        {
            get
            {
                return listBox1.SelectedIndex;
            }
        }

        public void Initialize(List<MatroskaSubtitleInfo> subtitleInfoList)
        { 
            foreach (MatroskaSubtitleInfo info in subtitleInfoList)
            {
                string s = string.Format(Configuration.Settings.Language.MatroskaSubtitleChooser.TrackXLanguageYTypeZ, info.TrackNumber, info.Name, info.Language, info.CodecId);
                listBox1.Items.Add(s);
            }
            listBox1.SelectedIndex = 0;
        }

        private void FormMatroskaSubtitleChooser_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
                DialogResult = DialogResult.Cancel;
        }

        private void ButtonOkClick(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
        }
    }
}
