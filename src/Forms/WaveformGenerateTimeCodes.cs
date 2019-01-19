using Nikse.SubtitleEdit.Core;
using Nikse.SubtitleEdit.Logic;
using System;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.Forms
{
    public sealed partial class WaveformGenerateTimeCodes : Form
    {
        public bool StartFromVideoPosition { get; set; }
        public bool DeleteAll { get; set; }
        public bool DeleteForward { get; set; }
        public int BlockSize { get; set; }
        public int VolumeMinimum { get; set; }
        public int VolumeMaximum { get; set; }
        public int DefaultMilliseconds { get; set; }

        public WaveformGenerateTimeCodes()
        {
            UiUtil.PreInitialize(this);
            InitializeComponent();
            UiUtil.FixFonts(this);

            var l = Configuration.Settings.Language.WaveformGenerateTimeCodes;
            Text = l.Title;
            groupBoxStartFrom.Text = l.StartFrom;
            radioButtonStartFromPos.Text = l.CurrentVideoPosition;
            radioButtonStartFromStart.Text = l.Beginning;
            groupBoxDeleteLines.Text = l.DeleteLines;
            groupBoxDetectOptions.Text = l.DetectOptions;
            labelScanBlocksMs.Text = l.ScanBlocksOfMs;
            labelAbove1.Text = l.BlockAverageVolMin1;
            labelAbove2.Text = l.BlockAverageVolMin2;
            labelBelow1.Text = l.BlockAverageVolMax1;
            labelBelow2.Text = l.BlockAverageVolMax2;
            groupBoxOther.Text = l.Other;
            labelSplit1.Text = l.SplitLongLinesAt1;
            labelSplit2.Text = l.SplitLongLinesAt2;
            radioButtonDeleteAll.Text = Configuration.Settings.Language.General.All;
            radioButtonDeleteNone.Text = Configuration.Settings.Language.General.None;
            radioButtonForward.Text = l.FromCurrentVideoPosition;

            numericUpDownBlockSize.Left = labelScanBlocksMs.Left + labelScanBlocksMs.Width + 3;
            numericUpDownMinVol.Left = labelAbove1.Left + labelAbove1.Width + 3;
            labelAbove2.Left = numericUpDownMinVol.Left + numericUpDownMinVol.Width + 3;
            numericUpDownMaxVol.Left = labelBelow1.Left + labelBelow1.Width + 3;
            labelBelow2.Left = numericUpDownMaxVol.Left + numericUpDownMaxVol.Width + 3;
            numericUpDownDefaultMilliseconds.Left = labelSplit1.Left + labelSplit1.Width + 3;
            labelSplit2.Left = numericUpDownDefaultMilliseconds.Left + numericUpDownDefaultMilliseconds.Width + 3;

            buttonOK.Text = Configuration.Settings.Language.General.Ok;
            buttonCancel.Text = Configuration.Settings.Language.General.Cancel;
            UiUtil.FixLargeFonts(this, buttonOK);
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            StartFromVideoPosition = radioButtonStartFromPos.Checked;
            DeleteAll = radioButtonDeleteAll.Checked;
            DeleteForward = radioButtonForward.Checked;
            BlockSize = (int)numericUpDownBlockSize.Value;
            VolumeMinimum = (int)numericUpDownMinVol.Value;
            VolumeMaximum = (int)numericUpDownMaxVol.Value;
            DefaultMilliseconds = (int)numericUpDownDefaultMilliseconds.Value;
            DialogResult = DialogResult.OK;
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

    }
}
