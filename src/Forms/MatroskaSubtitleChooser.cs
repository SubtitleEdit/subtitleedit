using Nikse.SubtitleEdit.Core;
using Nikse.SubtitleEdit.Core.ContainerFormats.Matroska;
using Nikse.SubtitleEdit.Core.ContainerFormats.Mp4.Boxes;
using Nikse.SubtitleEdit.Logic;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.Forms
{
    public sealed partial class MatroskaSubtitleChooser : Form
    {
        public MatroskaSubtitleChooser(string fileType)
        {
            UiUtil.PreInitialize(this);
            InitializeComponent();
            UiUtil.FixFonts(this);

            Text = fileType.Equals("mp4", StringComparison.Ordinal) ? Configuration.Settings.Language.MatroskaSubtitleChooser.TitleMp4 : Configuration.Settings.Language.MatroskaSubtitleChooser.Title;
            labelChoose.Text = Configuration.Settings.Language.MatroskaSubtitleChooser.PleaseChoose;
            buttonOK.Text = Configuration.Settings.Language.General.Ok;
            buttonCancel.Text = Configuration.Settings.Language.General.Cancel;
            UiUtil.FixLargeFonts(this, buttonOK);
        }

        public int SelectedIndex => listBox1.SelectedIndex;

        internal void Initialize(List<MatroskaTrackInfo> subtitleInfoList)
        {
            var format = Configuration.Settings.Language.MatroskaSubtitleChooser.TrackXLanguageYTypeZ;
            foreach (var info in subtitleInfoList)
            {
                var track = string.Format((!string.IsNullOrWhiteSpace(info.Name) ? "{0} - {1}" : "{0}"), info.TrackNumber, info.Name);
                listBox1.Items.Add(string.Format(format, track, info.Language, info.CodecId));
            }
            listBox1.SelectedIndex = 0;
        }

        internal void Initialize(List<Trak> mp4SubtitleTracks)
        {
            int i = 0;
            foreach (var track in mp4SubtitleTracks)
            {
                i++;
                string handler = (track.Mdia.HandlerType + " " + track.Mdia.HandlerName).Trim();
                if (handler.Length > 1)
                {
                    handler = " - " + handler;
                }

                string s = $"{i}: {track.Mdia.Mdhd.Iso639ThreeLetterCode} - {track.Mdia.Mdhd.LanguageString}{handler}";
                listBox1.Items.Add(s);
            }
            listBox1.SelectedIndex = 0;
        }

        private void FormMatroskaSubtitleChooser_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                DialogResult = DialogResult.Cancel;
            }
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
