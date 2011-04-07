using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Nikse.SubtitleEdit.Logic;

namespace Nikse.SubtitleEdit.Forms
{
    public partial class TranscriptImporter : Form
    {
        Main _main;
        Subtitle _subtitle;

        public TranscriptImporter()
        {
            InitializeComponent();
            FixLargeFonts();
        }

        private void FixLargeFonts()
        {
            Graphics graphics = this.CreateGraphics();
            SizeF textSize = graphics.MeasureString("OK", this.Font);
            if (textSize.Height > buttonInsert.Height - 4)
            {
                int newButtonHeight = (int)(textSize.Height + 7 + 0.5);
                Utilities.SetButtonHeight(this, newButtonHeight, 1);
            }
        }

        public void Initialize(Subtitle subtitle, Main main)
        {
            _subtitle = subtitle;
            _main = main;
            if (subtitle.Paragraphs.Count > 0)
                subtitle.Renumber(subtitle.Paragraphs[0].Number);

//            Text = Configuration.Settings.Language.SplitLongLines.Title;

            //buttonOK.Text = Configuration.Settings.Language.General.OK;
            //buttonCancel.Text = Configuration.Settings.Language.General.Cancel;
            SubtitleListview1.InitializeLanguage(Configuration.Settings.Language.General, Configuration.Settings);
            Utilities.InitializeSubtitleFont(SubtitleListview1);
            SubtitleListview1.AutoSizeAllColumns(this);
            SubtitleListview1.Fill(subtitle);
            SubtitleListview1.SelectIndexAndEnsureVisible(0);
        }

        private void buttonInsert_Click(object sender, EventArgs e)
        {
            if (SubtitleListview1.SelectedIndices.Count == 1)
            {
                int index = SubtitleListview1.SelectedIndices[0];
                var p = _subtitle.Paragraphs[index];
                _main.InsertViaEndPosition(p.Text, p.Duration.TotalMilliseconds, false);
                SubtitleListview1.Items[index].Selected = false;
                SubtitleListview1.SelectIndexAndEnsureVisible(index + 1);
            }
        }

        private void buttonPlayPause_Click(object sender, EventArgs e)
        {
            _main.PlayPause();
        }

        private void buttonStartHalfASecondBack_Click(object sender, EventArgs e)
        {
            _main.GoBackSeconds(0.5);
        }

        private void buttonStartThreeSecondsBack_Click(object sender, EventArgs e)
        {
            _main.GoBackSeconds(3.0);
        }

        private void SubtitleListview1_SelectedIndexChanged(object sender, EventArgs e)
        {
            Paragraph p = SubtitleListview1.GetSelectedParagraph(_subtitle);
            if (p != null)
            {

            }
            else
            { 
            }            
        }

    }
}
