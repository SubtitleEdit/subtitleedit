using System;
using System.Windows.Forms;
using Nikse.SubtitleEdit.Core;
using Nikse.SubtitleEdit.Logic;

namespace Nikse.SubtitleEdit.Forms
{
    public sealed partial class BatchConvertFixRtl : Form
    {
        public const string AddUnicode = "ADD_UNICODE";
        public const string RemoveUnicode = "REMOVE_UNICODE";
        public const string ReverseStartEnd = "REVERSE_START_END";

        public BatchConvertFixRtl()
        {
            UiUtil.PreInitialize(this);
            InitializeComponent();
            UiUtil.FixFonts(this);
            Text = Configuration.Settings.Language.BatchConvert.FixRtl;
            groupBox1.Text = Configuration.Settings.Language.BatchConvert.Settings;
            buttonCancel.Text = Configuration.Settings.Language.General.Cancel;
            buttonOK.Text = Configuration.Settings.Language.General.Ok;
            radioButtonAddUnicode.Text = Configuration.Settings.Language.BatchConvert.FixRtlAddUnicode;
            radioButtonRemoveUnicode.Text = Configuration.Settings.Language.BatchConvert.FixRtlRemoveUnicode;
            radioButtonReverseStartEnd.Text = Configuration.Settings.Language.BatchConvert.FixRtlReverseStartEnd;
            UiUtil.FixLargeFonts(this, buttonOK);

            var mode = Configuration.Settings.Tools.BatchConvertFixRtlMode;
            switch (mode)
            {
                case AddUnicode:
                    radioButtonRemoveUnicode.Checked = true;
                    break;
                case ReverseStartEnd:
                    radioButtonReverseStartEnd.Checked = true;
                    break;
                default:
                    radioButtonAddUnicode.Checked = true;
                    break;
            }
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void BatchConvertFixRtl_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                DialogResult = DialogResult.Cancel;
            }
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            if (radioButtonRemoveUnicode.Checked)
            {
                Configuration.Settings.Tools.BatchConvertFixRtlMode = RemoveUnicode;
            }
            else if (radioButtonReverseStartEnd.Checked)
            {
                Configuration.Settings.Tools.BatchConvertFixRtlMode = ReverseStartEnd;
            }
            else
            {
                Configuration.Settings.Tools.BatchConvertFixRtlMode = AddUnicode;
            }
            DialogResult = DialogResult.OK;
        }
    }
}
