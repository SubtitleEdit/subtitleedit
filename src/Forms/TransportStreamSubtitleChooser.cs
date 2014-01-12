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

        internal void Initialize(Logic.TransportStream.TransportStreamParser tsParser, string fileName)
        {
            _tsParser = tsParser;
            if (string.IsNullOrEmpty(Configuration.Settings.Language.TransportStreamSubtitleChooser.Title))
                Text = string.Format("Transport stream subtitle chooser - {0}", fileName);
            else
                Text = string.Format(Configuration.Settings.Language.TransportStreamSubtitleChooser.Title, fileName);
            foreach (int id in tsParser.SubtitlePacketIds)
            {
                string s = string.Format(string.Format("Transport Packet Identifier (PID) = {0}, number of subtitles = {1}", id, tsParser.GetDvbSubtitles(id).Count));
                if (!string.IsNullOrEmpty(Configuration.Settings.Language.TransportStreamSubtitleChooser.PidLine))
                    s = string.Format(string.Format(Configuration.Settings.Language.TransportStreamSubtitleChooser.PidLine, id, tsParser.GetDvbSubtitles(id).Count));
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
                var start = new TimeCode(TimeSpan.FromMilliseconds(sub.StartMilliseconds));
                var end = new TimeCode(TimeSpan.FromMilliseconds(sub.EndMilliseconds));
                if (string.IsNullOrEmpty(Configuration.Settings.Language.TransportStreamSubtitleChooser.SubLine))
                    listBoxSubtitles.Items.Add(string.Format("{0}:  {1} --> {2},  {3} image(s)", i, start.ToString(), end.ToString(), sub.NumberOfImages));
                else
                    listBoxSubtitles.Items.Add(string.Format(Configuration.Settings.Language.TransportStreamSubtitleChooser.SubLine, i, start.ToString(), end.ToString(), sub.NumberOfImages));
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

            var dvbBmp = list[idx].GetActiveImage();
            var nDvbBmp = new NikseBitmap(dvbBmp);
            nDvbBmp.CropTopTransparent(2);
            nDvbBmp.CropTransparentSidesAndBottom(2, true);
            dvbBmp.Dispose();
            var oldImage = pictureBox1.Image;
            pictureBox1.Image = nDvbBmp.GetBitmap();
            if (oldImage != null)
                oldImage.Dispose();
        }

    }
}
