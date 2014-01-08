using Nikse.SubtitleEdit.Logic;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.Forms
{
    public partial class TransportStreamSubtitleChooser : Form
    {
        Logic.TransportStream.TransportStreamParser _tsParser;

        public TransportStreamSubtitleChooser()
        {
            InitializeComponent();
            //Text = Configuration.Settings.Language.MatroskaSubtitleChooser.Title;
            labelChoose.Text = Configuration.Settings.Language.MatroskaSubtitleChooser.PleaseChoose;
            buttonOK.Text = Configuration.Settings.Language.General.OK;
            buttonCancel.Text = Configuration.Settings.Language.General.Cancel;
            FixLargeFonts();
        }

        private void FixLargeFonts()
        {
            Graphics graphics = this.CreateGraphics();
            SizeF textSize = graphics.MeasureString(buttonOK.Text, this.Font);
            if (textSize.Height > buttonOK.Height - 4)
            {
                int newButtonHeight = (int)(textSize.Height + 7 + 0.5);
                Utilities.SetButtonHeight(this, newButtonHeight, 1);
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

        private void TransportStreamSubtitleChooser_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
                DialogResult = DialogResult.Cancel;
        }

        internal void Initialize(Logic.TransportStream.TransportStreamParser tsParser)
        {
            _tsParser = tsParser;
            foreach (int id in tsParser.SubtitlePacketIds)
            {
                string s = string.Format(string.Format("Transport Packet Identifier (PID) = {0}, number of subtitles = {1}", id, tsParser.GetDvbSubtitles(id).Count));
                listBoxTracks.Items.Add(s);
            }
            listBoxTracks.SelectedIndex = 0;
        }

        public int SelectedIndex
        {
            get
            {
                return listBoxTracks.SelectedIndex;
            }
        }

        private void listBoxTracks_SelectedIndexChanged(object sender, EventArgs e)
        {
            int idx = listBoxTracks.SelectedIndex;
            if (idx < 0)
                return;

            listBoxSubtitles.Items.Clear();
            int pid = _tsParser.SubtitlePacketIds[idx];
            var list = _tsParser.GetDvbSubtitles(pid);
            int i = 0;
            foreach (var sub in list)
            {
                i++;
                var tc = new TimeCode(TimeSpan.FromMilliseconds(sub.StartMilliseconds));
                listBoxSubtitles.Items.Add(string.Format("{0}: {1}, {2} images", i, tc.ToString(), sub.Pes.ObjectDataList.Count));
            }
            if (list.Count > 0)
                listBoxSubtitles.SelectedIndex = 0;
        }

        private void listBoxSubtitles_SelectedIndexChanged(object sender, EventArgs e)
        {
            int idx = listBoxSubtitles.SelectedIndex;
            if (idx < 0)
                return;

            int pid = _tsParser.SubtitlePacketIds[listBoxTracks.SelectedIndex];
            var list = _tsParser.GetDvbSubtitles(pid);

            var dvbBmp = list[idx].Pes.GetImageFull();
            NikseBitmap nDvbBmp = new NikseBitmap(dvbBmp);
            nDvbBmp.CropTopTransparent(2);
            nDvbBmp.CropSidesAndBottom(2, Color.Transparent, true);
            dvbBmp.Dispose();
            var oldImage = pictureBox1.Image;
            pictureBox1.Image = nDvbBmp.GetBitmap();
            if (oldImage != null)
                oldImage.Dispose();
        }

    }
}
