using Nikse.SubtitleEdit.Logic;
using System;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.Forms
{
    public partial class TransportStreamSubtitleChooser : PositionAndSizeForm
    {
        private Logic.TransportStream.TransportStreamParser _tsParser;

        public TransportStreamSubtitleChooser()
        {
            InitializeComponent();
            labelChoose.Text = Configuration.Settings.Language.MatroskaSubtitleChooser.PleaseChoose;
            buttonOK.Text = Configuration.Settings.Language.General.Ok;
            buttonCancel.Text = Configuration.Settings.Language.General.Cancel;
            Utilities.FixLargeFonts(this, buttonOK);
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
            Text = string.Format(Configuration.Settings.Language.TransportStreamSubtitleChooser.Title, fileName);

            foreach (int id in tsParser.SubtitlePacketIds)
            {
                listBoxTracks.Items.Add(string.Format(Configuration.Settings.Language.TransportStreamSubtitleChooser.PidLine, id, tsParser.GetDvbSubtitles(id).Count));
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
                var start = new TimeCode(sub.StartMilliseconds);
                var end = new TimeCode(sub.EndMilliseconds);
                listBoxSubtitles.Items.Add(string.Format(Configuration.Settings.Language.TransportStreamSubtitleChooser.SubLine, i, start, end, sub.NumberOfImages));
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
