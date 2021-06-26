using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.ContainerFormats.TransportStream;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Forms.Ocr;
using Nikse.SubtitleEdit.Logic;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.Forms
{
    public partial class TransportStreamSubtitleChooser : PositionAndSizeForm
    {
        public int SelectedIndex => listBoxTracks.SelectedIndex;
        public bool IsTeletext { get; private set; }
        public string Srt { get; private set; }

        public class StreamTrackItem
        {
            public string Text { get; set; }
            public bool IsTeletext { get; set; }
            public string Srt { get; set; }
            public int Pid { get; set; }
            public int PageNumber { get; set; }
            public string Language { get; set; }

            public override string ToString()
            {
                return Text;
            }
        }

        private TransportStreamParser _tsParser;
        private string _fileName;
        private ProgramMapTableParser _programMapTableParser;

        public TransportStreamSubtitleChooser()
        {
            UiUtil.PreInitialize(this);
            InitializeComponent();
            UiUtil.FixFonts(this);
            labelChoose.Text = LanguageSettings.Current.MatroskaSubtitleChooser.PleaseChoose;
            buttonOK.Text = LanguageSettings.Current.General.Ok;
            buttonCancel.Text = LanguageSettings.Current.General.Cancel;
            toolStripMenuItemExport.Text = LanguageSettings.Current.Main.Menu.File.Export;
            vobSubToolStripMenuItem.Text = LanguageSettings.Current.Main.Menu.File.ExportVobSub;
            bDNXMLToolStripMenuItem.Text = LanguageSettings.Current.Main.Menu.File.ExportBdnXml;
            bluraySupToolStripMenuItem.Text = LanguageSettings.Current.Main.Menu.File.ExportBluRaySup;
            saveAllImagesWithHtmlIndexViewToolStripMenuItem.Text = LanguageSettings.Current.VobSubOcr.SaveAllSubtitleImagesWithHtml;
            UiUtil.FixLargeFonts(this, buttonOK);
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
            {
                DialogResult = DialogResult.Cancel;
            }
        }

        internal void Initialize(TransportStreamParser tsParser, string fileName)
        {
            _programMapTableParser = new ProgramMapTableParser();
            _programMapTableParser.Parse(fileName); // get languages
            _tsParser = tsParser;
            _fileName = fileName;
            Text = string.Format(LanguageSettings.Current.TransportStreamSubtitleChooser.Title, fileName);

            foreach (int id in tsParser.SubtitlePacketIds)
            {
                var language = _programMapTableParser.GetSubtitleLanguage(id);
                if (string.IsNullOrEmpty(language))
                {
                    language = "unknown";
                }

                listBoxTracks.Items.Add(new StreamTrackItem
                {
                    Text = string.Format(LanguageSettings.Current.TransportStreamSubtitleChooser.PidLineImage, id, language, tsParser.GetDvbSubtitles(id).Count),
                    IsTeletext = false,
                    Pid = id,
                    Language = language
                });
            }

            foreach (var program in tsParser.TeletextSubtitlesLookup)
            {
                var language = _programMapTableParser.GetSubtitleLanguage(program.Key);
                if (string.IsNullOrEmpty(language))
                {
                    language = "unknown";
                }

                foreach (var kvp in program.Value)
                {
                    var subtitle = new Subtitle(kvp.Value);
                    subtitle.Renumber();
                    listBoxTracks.Items.Add(new StreamTrackItem
                    {
                        Text = string.Format(LanguageSettings.Current.TransportStreamSubtitleChooser.PidLineTeletext, kvp.Key, program.Key, language, kvp.Value.Count),
                        IsTeletext = true,
                        Pid = program.Key,
                        PageNumber = kvp.Key,
                        Srt = new SubRip().ToText(subtitle, null)
                    });
                }
            }

            listBoxTracks.SelectedIndex = 0;
        }

        private void listBoxTracks_SelectedIndexChanged(object sender, EventArgs e)
        {
            int idx = listBoxTracks.SelectedIndex;
            if (idx < 0)
            {
                return;
            }

            var item = (StreamTrackItem)listBoxTracks.SelectedItem;
            if (item.IsTeletext)
            {
                textBoxTeletext.Visible = true;
                textBoxTeletext.Text = item.Srt;
                IsTeletext = true;
                Srt = item.Srt;
                return;
            }

            IsTeletext = false;
            Srt = string.Empty;
            textBoxTeletext.Visible = false;
            listBoxSubtitles.Items.Clear();
            int pid = _tsParser.SubtitlePacketIds[idx];
            var list = _tsParser.GetDvbSubtitles(pid);
            int i = 0;
            foreach (var sub in list)
            {
                i++;
                var start = new TimeCode(sub.StartMilliseconds);
                var end = new TimeCode(sub.EndMilliseconds);
                listBoxSubtitles.Items.Add(string.Format(LanguageSettings.Current.TransportStreamSubtitleChooser.SubLine, i, start, end, sub.NumberOfImages));
            }
            if (list.Count > 0)
            {
                listBoxSubtitles.SelectedIndex = 0;
            }
        }

        private void listBoxSubtitles_SelectedIndexChanged(object sender, EventArgs e)
        {
            int idx = listBoxSubtitles.SelectedIndex;
            if (idx < 0)
            {
                return;
            }

            int pid = _tsParser.SubtitlePacketIds[listBoxTracks.SelectedIndex];
            var list = _tsParser.GetDvbSubtitles(pid);

            var dvbBmp = list[idx].GetBitmap();
            var nDvbBmp = new NikseBitmap(dvbBmp);
            nDvbBmp.CropTopTransparent(2);
            nDvbBmp.CropTransparentSidesAndBottom(2, true);
            dvbBmp.Dispose();
            var oldImage = pictureBox1.Image;
            pictureBox1.Image = nDvbBmp.GetBitmap();
            oldImage?.Dispose();
        }

        private List<TransportStreamSubtitle> GetSelectedSubtitles()
        {
            int idx = listBoxSubtitles.SelectedIndex;
            if (idx < 0)
            {
                return null;
            }

            int pid = _tsParser.SubtitlePacketIds[listBoxTracks.SelectedIndex];
            return _tsParser.GetDvbSubtitles(pid);
        }

        private string GetSelectedLanguage()
        {
            int idx = listBoxSubtitles.SelectedIndex;
            if (idx < 0)
            {
                return null;
            }

            int pid = _tsParser.SubtitlePacketIds[listBoxTracks.SelectedIndex];
            return _programMapTableParser.GetSubtitleLanguage(pid);
        }

        private void BluraySupToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ExportTo(ExportPngXml.ExportFormats.BluraySup);
        }

        private void BDNXMLToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ExportTo(ExportPngXml.ExportFormats.BdnXml);
        }

        private void VobSubToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ExportTo(ExportPngXml.ExportFormats.VobSub);
        }

        private void DOSTToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ExportTo(ExportPngXml.ExportFormats.Dost);
        }

        private Point GetResolution()
        {
            int idx = listBoxTracks.SelectedIndex;
            if (idx < 0)
            {
                return new Point(DvbSubPes.DefaultScreenWidth, DvbSubPes.DefaultScreenHeight);
            }

            int pid = _tsParser.SubtitlePacketIds[idx];
            var list = _tsParser.GetDvbSubtitles(pid);
            if (list.Count > 0)
            {
                using (var bmp = list[0].GetBitmap())
                {
                    return new Point(bmp.Width, bmp.Height);
                }
            }
            return new Point(DvbSubPes.DefaultScreenWidth, DvbSubPes.DefaultScreenHeight);
        }

        private void ExportTo(string exportType)
        {
            var subtitles = GetSelectedSubtitles();
            if (subtitles == null)
            {
                return;
            }

            using (var formSubOcr = new VobSubOcr())
            {
                formSubOcr.Initialize(subtitles, Configuration.Settings.VobSubOcr, _fileName, GetSelectedLanguage(), true);
                using (var exportBdnXmlPng = new ExportPngXml())
                {
                    exportBdnXmlPng.InitializeFromVobSubOcr(formSubOcr.SubtitleFromOcr, new SubRip(), exportType, _fileName, formSubOcr, GetSelectedLanguage());
                    exportBdnXmlPng.SetResolution(GetResolution());
                    exportBdnXmlPng.ShowDialog(this);
                }
            }
        }

        private void SaveAllImagesWithHtmlIndexViewToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var subtitles = GetSelectedSubtitles();
            if (subtitles == null)
            {
                return;
            }

            using (var formSubOcr = new VobSubOcr())
            {
                formSubOcr.Initialize(subtitles, Configuration.Settings.VobSubOcr, _fileName, GetSelectedLanguage());
                formSubOcr.SaveAllImagesWithHtmlIndexViewToolStripMenuItem_Click(sender, e);
            }
        }

        private void TransportStreamSubtitleChooser_Shown(object sender, EventArgs e)
        {
            BringToFront();
        }

        private void contextMenuStripListview_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            int idx = listBoxTracks.SelectedIndex;
            if (idx < 0)
            {
                e.Cancel = true;
                return;
            }

            var item = (StreamTrackItem)listBoxTracks.SelectedItem;
            if (item.IsTeletext)
            {
                toolStripMenuItemExport.Visible = false;
                saveAllImagesWithHtmlIndexViewToolStripMenuItem.Visible = false;
                saveSubtitleAsToolStripMenuItem.Visible = true;
            }
            else
            {
                toolStripMenuItemExport.Visible = true;
                saveAllImagesWithHtmlIndexViewToolStripMenuItem.Visible = true;
                saveSubtitleAsToolStripMenuItem.Visible = false;
            }
        }

        private void saveSubtitleAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int idx = listBoxTracks.SelectedIndex;
            if (idx < 0)
            {
                return;
            }

            var item = (StreamTrackItem)listBoxTracks.SelectedItem;
            if (item.IsTeletext)
            {
                saveFileDialog1.Title = LanguageSettings.Current.ExportCustomText.SaveSubtitleAs;
                var fileName = Utilities.GetPathAndFileNameWithoutExtension(_fileName);
                if (item.PageNumber > 0)
                {
                    fileName += "." + item.PageNumber.ToString(CultureInfo.InvariantCulture);
                }
                if (!string.IsNullOrEmpty(item.Language))
                {
                    fileName += "." + _programMapTableParser.GetSubtitleLanguageTwoLetter(item.Pid);
                }
                saveFileDialog1.InitialDirectory = Path.GetDirectoryName(fileName);
                saveFileDialog1.FileName = Path.GetFileName(fileName) + ".srt";

                if (saveFileDialog1.ShowDialog(this) == DialogResult.OK)
                {
                    File.WriteAllText(saveFileDialog1.FileName, item.Srt);
                }
            }
        }

    }
}
