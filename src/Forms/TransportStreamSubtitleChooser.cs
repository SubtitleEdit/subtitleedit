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
            foreach (int id in tsParser.SubtitlePacketIds)
            {
                string s = string.Format(string.Format("Packet id {0}", id));
                listBox1.Items.Add(s);
            }
            listBox1.SelectedIndex = 0;
        }

        public int SelectedIndex
        {
            get
            {
                return listBox1.SelectedIndex;
            }
        }

    }
}
