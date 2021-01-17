using Nikse.SubtitleEdit.Logic;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.Forms
{
    public sealed partial class ChooseAudioTrack : Form
    {
        public int SelectedTrack { get; set; }

        public ChooseAudioTrack(List<string> tracks, int selectedTrack)
        {
            UiUtil.PreInitialize(this);
            InitializeComponent();
            UiUtil.FixFonts(this);
            foreach (string track in tracks)
            {
                listBoxTracks.Items.Add(track);
            }
            listBoxTracks.SelectedIndex = selectedTrack;

            if (listBoxTracks.SelectedIndex == -1 && listBoxTracks.Items.Count > 0)
            {
                listBoxTracks.SelectedIndex = 0;
            }

            Text = LanguageSettings.Current.ChooseAudioTrack.Title;
            labelDescr.Text = LanguageSettings.Current.ChooseAudioTrack.Title;
            UiUtil.FixLargeFonts(this, buttonOK);
        }

        private void ChooseAudioTrack_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                DialogResult = DialogResult.Cancel;
            }
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            SelectedTrack = listBoxTracks.SelectedIndex;
            DialogResult = DialogResult.OK;
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }
    }
}
