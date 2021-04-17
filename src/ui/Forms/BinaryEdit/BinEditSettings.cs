using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Logic;
using System;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.Forms.BinaryEdit
{
    public sealed partial class BinEditSettings : Form
    {
        public BinEditSettings()
        {
            UiUtil.PreInitialize(this);
            InitializeComponent();
            UiUtil.FixFonts(this);

            panelBackgroundColor.BackColor = Configuration.Settings.Tools.BinEditBackgroundColor;
            panelImageBackgroundColor.BackColor = Configuration.Settings.Tools.BinEditImageBackgroundColor;

            numericUpDownMarginLeft.Value = Configuration.Settings.Tools.BinEditLeftMargin;
            numericUpDownMarginRight.Value = Configuration.Settings.Tools.BinEditRightMargin;
            numericUpDownMarginVertical.Value = Configuration.Settings.Tools.BinEditVerticalMargin;
            UiUtil.FixLargeFonts(this, buttonOK);

            Text = LanguageSettings.Current.Settings.Title;
            buttonOK.Text = LanguageSettings.Current.General.Ok;
            buttonCancel.Text = LanguageSettings.Current.General.Cancel;
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            Configuration.Settings.Tools.BinEditBackgroundColor = panelBackgroundColor.BackColor;
            Configuration.Settings.Tools.BinEditImageBackgroundColor = panelImageBackgroundColor.BackColor;

            Configuration.Settings.Tools.BinEditLeftMargin = (int)numericUpDownMarginLeft.Value;
            Configuration.Settings.Tools.BinEditRightMargin = (int)numericUpDownMarginRight.Value;
            Configuration.Settings.Tools.BinEditVerticalMargin = (int)numericUpDownMarginVertical.Value;

            DialogResult = DialogResult.OK;
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void buttonBackgroundColor_Click(object sender, EventArgs e)
        {
            using (var colorChooser = new ColorChooser { Color = panelBackgroundColor.BackColor, ShowAlpha = false })
            {
                if (colorChooser.ShowDialog() == DialogResult.OK)
                {
                    panelBackgroundColor.BackColor = colorChooser.Color;
                }
            }
        }

        private void buttonImageBackgroundColor_Click(object sender, EventArgs e)
        {
            using (var colorChooser = new ColorChooser { Color = panelImageBackgroundColor.BackColor, ShowAlpha = false })
            {
                if (colorChooser.ShowDialog() == DialogResult.OK)
                {
                    panelImageBackgroundColor.BackColor = colorChooser.Color;
                }
            }
        }

        private void BinEditSettings_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                DialogResult = DialogResult.Cancel;
            }
        }
    }
}
