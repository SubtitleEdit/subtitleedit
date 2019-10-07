using System;
using System.Windows.Forms;
using Nikse.SubtitleEdit.Core;
using Nikse.SubtitleEdit.Logic;

namespace Nikse.SubtitleEdit.Forms
{
    public sealed partial class BatchConvertTsSettings : Form
    {
        public BatchConvertTsSettings()
        {
            UiUtil.PreInitialize(this);
            InitializeComponent();
            UiUtil.FixFonts(this);
            Text = Configuration.Settings.Language.BatchConvert.TransportStreamSettings;
            checkBoxOverrideOriginalXPosition.Text = Configuration.Settings.Language.BatchConvert.TransportStreamOverrideXPosition;
            checkBoxOverrideOriginalYPosition.Text = Configuration.Settings.Language.BatchConvert.TransportStreamOverrideYPosition;
            checkBoxOverrideVideoSize.Text = Configuration.Settings.Language.BatchConvert.TransportStreamOverrideVideoSize;
            labelBottomMargin.Text = Configuration.Settings.Language.ExportPngXml.BottomMargin;
            labelXMargin.Text = Configuration.Settings.Language.ExportPngXml.LeftRightMargin;
            labelXAlignment.Text = Configuration.Settings.Language.ExportPngXml.Align;
            labelWidth.Text = Configuration.Settings.Language.General.Width;
            labelHeight.Text = Configuration.Settings.Language.General.Height;
            buttonCancel.Text = Configuration.Settings.Language.General.Cancel;
            buttonOK.Text = Configuration.Settings.Language.General.Ok;
            UiUtil.FixLargeFonts(this, buttonOK);

            numericUpDownXMargin.Left = labelXMargin.Left + labelXMargin.Width + 5;
            comboBoxHAlign.Left = labelXAlignment.Left + labelXAlignment.Width + 5;
            numericUpDownBottomMargin.Left = labelBottomMargin.Left + labelBottomMargin.Width + 5;
            var widthAndHeightLeft = Math.Max(labelWidth.Left + labelWidth.Width, labelHeight.Left + labelHeight.Width) + 5;
            numericUpDownWidth.Left = widthAndHeightLeft;
            numericUpDownHeight.Left = widthAndHeightLeft;

            checkBoxOverrideOriginalYPosition.Checked = Configuration.Settings.Tools.BatchConvertTsOverridePosition;
            numericUpDownBottomMargin.Value = Configuration.Settings.Tools.BatchConvertTsOverrideBottomMargin;
            checkBoxOverrideVideoSize.Checked = Configuration.Settings.Tools.BatchConvertTsOverrideScreenSize;
            numericUpDownWidth.Value = Configuration.Settings.Tools.BatchConvertTsScreenWidth;
            numericUpDownHeight.Value = Configuration.Settings.Tools.BatchConvertTsScreenHeight;

            checkBoxOverrideOriginalXPosition_CheckedChanged(null, null);
            CheckBoxOverrideOriginalYPosition_CheckedChanged(null, null);
            CheckBoxOverrideVideoSize_CheckedChanged(null, null);
        }

        private void CheckBoxOverrideOriginalYPosition_CheckedChanged(object sender, EventArgs e)
        {
            labelBottomMargin.Enabled = checkBoxOverrideOriginalYPosition.Checked;
            numericUpDownBottomMargin.Enabled = checkBoxOverrideOriginalYPosition.Checked;
        }

        private void CheckBoxOverrideVideoSize_CheckedChanged(object sender, EventArgs e)
        {
            labelWidth.Enabled = checkBoxOverrideVideoSize.Checked;
            numericUpDownWidth.Enabled = checkBoxOverrideVideoSize.Checked;
            labelHeight.Enabled = checkBoxOverrideVideoSize.Checked;
            numericUpDownHeight.Enabled = checkBoxOverrideVideoSize.Checked;
        }

        private void BatchConvertTsSettings_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                DialogResult = DialogResult.Cancel;
            }
        }

        private void ButtonOK_Click(object sender, EventArgs e)
        {
            Configuration.Settings.Tools.BatchConvertTsOverridePosition = checkBoxOverrideOriginalYPosition.Checked;
            Configuration.Settings.Tools.BatchConvertTsOverrideBottomMargin = (int)numericUpDownBottomMargin.Value;
            Configuration.Settings.Tools.BatchConvertTsOverrideScreenSize = checkBoxOverrideVideoSize.Checked;
            Configuration.Settings.Tools.BatchConvertTsScreenWidth = (int)numericUpDownWidth.Value;
            Configuration.Settings.Tools.BatchConvertTsScreenHeight = (int)numericUpDownHeight.Value;
            DialogResult = DialogResult.OK;
        }

        private void checkBoxOverrideOriginalXPosition_CheckedChanged(object sender, EventArgs e)
        {
            labelXMargin.Enabled = checkBoxOverrideOriginalXPosition.Checked;
            numericUpDownXMargin.Enabled = checkBoxOverrideOriginalXPosition.Checked;
            labelXAlignment.Enabled = checkBoxOverrideOriginalXPosition.Checked;
            comboBoxHAlign.Enabled = checkBoxOverrideOriginalXPosition.Checked;
        }
    }
}
