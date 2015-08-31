using Nikse.SubtitleEdit.Core;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.Forms
{
    public partial class ChooseAudioTrack : Form
    {
        public ChooseAudioTrack(List<string> tracks, int defaultTrack)
        {
            InitializeComponent();
            foreach (string track in tracks)
            {
                listBoxTracks.Items.Add(track);
                if (listBoxTracks.Items.Count == defaultTrack)
                    listBoxTracks.SelectedIndex = listBoxTracks.Items.Count - 1;
            }
            if (listBoxTracks.SelectedIndex == -1 && listBoxTracks.Items.Count > 0)
                listBoxTracks.SelectedIndex = 0;

            Text = Configuration.Settings.Language.ChooseAudioTrack.Title;
            labelDescr.Text = Configuration.Settings.Language.ChooseAudioTrack.Title;
        }

        public int SelectedTrack { get; set; }

        private void ChooseAudioTrack_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
                DialogResult = DialogResult.Cancel;
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
