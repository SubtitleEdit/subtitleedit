using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.Interfaces;
using Nikse.SubtitleEdit.Logic;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.Forms
{
    public sealed partial class SubtitleViewer : PositionAndSizeForm
    {
        private readonly Subtitle _subtitle;
        private readonly List<IBinaryParagraphWithPosition> _binaryParagraphs;

        public SubtitleViewer(string fileName, Subtitle subtitle, List<IBinaryParagraphWithPosition> binaryParagraphs)
        {
            UiUtil.PreInitialize(this);
            InitializeComponent();
            UiUtil.FixFonts(this);
            buttonOK.Text = LanguageSettings.Current.General.Ok;
            buttonCancel.Text = LanguageSettings.Current.General.Cancel;
            toolStripMenuItemExport.Text = LanguageSettings.Current.Main.Menu.File.Export;
            vobSubToolStripMenuItem.Text = LanguageSettings.Current.Main.Menu.File.ExportVobSub;
            bDNXMLToolStripMenuItem.Text = LanguageSettings.Current.Main.Menu.File.ExportBdnXml;
            bluraySupToolStripMenuItem.Text = LanguageSettings.Current.Main.Menu.File.ExportBluRaySup;
            saveAllImagesWithHtmlIndexViewToolStripMenuItem.Text = LanguageSettings.Current.VobSubOcr.SaveAllSubtitleImagesWithHtml;
            UiUtil.FixLargeFonts(this, buttonOK);

            Text = "View - " + fileName;
            _subtitle = subtitle;
            _binaryParagraphs = binaryParagraphs;
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

        private void listBoxSubtitles_SelectedIndexChanged(object sender, EventArgs e)
        {
            var idx = listBoxSubtitles.SelectedIndex;
            if (idx < 0)
            {
                return;
            }

            pictureBox1.Image?.Dispose();
            var bitmap = _binaryParagraphs[idx].GetBitmap();
            pictureBox1.Image = bitmap;
        }

        private void SubtitleViewer_Shown(object sender, EventArgs e)
        {
            if (_binaryParagraphs != null)
            {
                textBoxRaw.Visible = false;
                listBoxSubtitles.Visible = true;
                listBoxSubtitles.BringToFront();
                for (var index = 0; index < _binaryParagraphs.Count; index++)
                {
                    var paragraph = _binaryParagraphs[index];
                    listBoxSubtitles.Items.Add($"#{index:000}  {paragraph.StartTimeCode} -->  {paragraph.EndTimeCode}");
                }

                if (listBoxSubtitles.Items.Count > 0)
                {
                    listBoxSubtitles.SelectedIndex = 0;
                }

                Text += $" - {_binaryParagraphs.Count} cues";
            }
            else if (_subtitle != null)
            {
                textBoxRaw.Text = _subtitle.ToText(_subtitle.OriginalFormat);
                textBoxRaw.Visible = true;
                textBoxRaw.BringToFront();

                Text += $" - {_subtitle.OriginalFormat.Name} - {_subtitle.Paragraphs.Count} cues";
            }
        }

        private void SubtitleViewer_FormClosing(object sender, FormClosingEventArgs e)
        {
            pictureBox1.Image?.Dispose();
        }
    }
}
