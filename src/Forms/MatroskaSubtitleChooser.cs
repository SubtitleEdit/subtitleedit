using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Nikse.SubtitleEdit.Logic;
using System.Drawing;

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

        internal void Initialize(List<Logic.Mp4.Boxes.Trak> mp4SubtitleTracks)
        {
            int i = 0;
            foreach (var track in mp4SubtitleTracks)
            {
                i++;
                string s = string.Format("{0}: {1} - {2}", i, track.Mdia.Mdhd.Iso639ThreeLetterCode, track.Mdia.Mdhd.LanguageString); 
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

        private void listBox1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            DialogResult = DialogResult.OK;
        }

    }
}
