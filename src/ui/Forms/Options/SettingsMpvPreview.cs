using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Logic;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.Forms.Options
{
    public partial class SettingsMpvPreview : Form
    {
        private bool _loading = true;
        private bool _backgroundImageDark;

        public SettingsMpvPreview()
        {
            UiUtil.PreInitialize(this);
            InitializeComponent();
            UiUtil.FixFonts(this);
            InitializeLanguage();

            var gs = Configuration.Settings.General;
            _backgroundImageDark = gs.UseDarkTheme;

            panelPrimaryColor.BackColor = Color.FromArgb(gs.MpvPreviewTextPrimaryColor);
            panelOutlineColor.BackColor = Color.FromArgb(gs.MpvPreviewTextOutlineColor);
            panelBackColor.BackColor = Color.FromArgb(gs.MpvPreviewTextBackColor);

            if (gs.MpvPreviewTextOutlineWidth >= numericUpDownSsaOutline.Minimum && gs.MpvPreviewTextOutlineWidth <= numericUpDownSsaOutline.Maximum)
            {
                numericUpDownSsaOutline.Value = gs.MpvPreviewTextOutlineWidth;
            }

            if (gs.MpvPreviewTextShadowWidth >= numericUpDownSsaShadow.Minimum && gs.MpvPreviewTextShadowWidth <= numericUpDownSsaShadow.Maximum)
            {
                numericUpDownSsaShadow.Value = gs.MpvPreviewTextShadowWidth;
            }

            checkBoxSsaOpaqueBox.Checked = gs.MpvPreviewTextOpaqueBox;
            numericUpDownSsaMarginLeft.Value = gs.MpvPreviewTextMarginLeft;
            numericUpDownSsaMarginRight.Value = gs.MpvPreviewTextMarginRight;
            numericUpDownSsaMarginVertical.Value = gs.MpvPreviewTextMarginVertical;

            _loading = false;
            UpdateSsaExample();
        }

        private void InitializeLanguage()
        {
            var l = LanguageSettings.Current.SubStationAlphaStyles;
            groupBoxSsaStyle.Text = l.TitleSubstationAlpha;
            groupBoxSsaColors.Text = l.Colors;
            buttonPrimaryColor.Text = l.Primary;
            buttonOutlineColor.Text = l.Outline;
            buttonBackColor.Text = l.Shadow;
            groupBoxSsaBorder.Text = l.Border;
            labelSsaOutline.Text = l.Outline;
            labelSsaShadow.Text = l.Shadow;
            checkBoxSsaOpaqueBox.Text = l.OpaqueBox;
            groupBoxMargins.Text = l.Margins;
            labelMarginLeft.Text = l.MarginLeft;
            labelMarginRight.Text = l.MarginRight;
            labelMarginVertical.Text = l.MarginVertical;
            groupBoxPreview.Text = LanguageSettings.Current.General.Preview;
            buttonOK.Text = LanguageSettings.Current.General.Ok;
            buttonCancel.Text = LanguageSettings.Current.General.Cancel;
        }

        private void UpdateSsaExample()
        {
            GeneratePreviewReal();
        }

        private void GeneratePreviewReal()
        {
            if (_loading)
            {
                return;
            }

            pictureBoxPreview.Image?.Dispose();
            var backgroundImage = TextDesigner.MakeBackgroundImage(pictureBoxPreview.Width, pictureBoxPreview.Height, 9, _backgroundImageDark);
            var outlineWidth = (float)numericUpDownSsaOutline.Value;
            var shadowWidth = (float)numericUpDownSsaShadow.Value;

            Font font;
            try
            {
                font = new Font(Configuration.Settings.General.VideoPlayerPreviewFontName, Configuration.Settings.General.VideoPlayerPreviewFontSize * 1.1f, Configuration.Settings.General.VideoPlayerPreviewFontBold ? FontStyle.Bold : FontStyle.Regular);
            }
            catch
            {
                font = new Font(Font, FontStyle.Regular);
            }

            var measureBmp = TextDesigner.MakeTextBitmapAssa(
                Configuration.Settings.General.PreviewAssaText,
                0,
                0,
                font,
                pictureBoxPreview.Width,
                pictureBoxPreview.Height,
                outlineWidth,
                shadowWidth,
                null,
                panelPrimaryColor.BackColor,
                panelOutlineColor.BackColor,
                panelBackColor.BackColor,
                checkBoxSsaOpaqueBox.Checked);
            var nBmp = new NikseBitmap(measureBmp);
            var measuredWidth = nBmp.GetNonTransparentWidth();
            var measuredHeight = nBmp.GetNonTransparentHeight();

            float left = (pictureBoxPreview.Width - measuredWidth) / 2.0f;
            float top = pictureBoxPreview.Height - measuredHeight - (int)numericUpDownSsaMarginVertical.Value;
            var designedText = TextDesigner.MakeTextBitmapAssa(
                Configuration.Settings.General.PreviewAssaText,
                (int)Math.Round(left),
                (int)Math.Round(top),
                font,
                pictureBoxPreview.Width,
                pictureBoxPreview.Height,
                outlineWidth,
                shadowWidth,
                backgroundImage,
                panelPrimaryColor.BackColor,
                panelOutlineColor.BackColor,
                panelBackColor.BackColor,
                checkBoxSsaOpaqueBox.Checked);

            pictureBoxPreview.Image?.Dispose();
            pictureBoxPreview.Image = designedText;
            font.Dispose();
        }

        private void pictureBoxPreview_Click(object sender, EventArgs e)
        {
            _backgroundImageDark = !_backgroundImageDark;
            UpdateSsaExample();
        }       
        
        private void numericUpDownSsaMarginVertical_ValueChanged(object sender, EventArgs e)
        {
            UpdateSsaExample();
        }

        private void checkBoxSsaOpaqueBox_CheckedChanged(object sender, EventArgs e)
        {
            numericUpDownSsaOutline.Enabled = !checkBoxSsaOpaqueBox.Checked;
            numericUpDownSsaShadow.Enabled = !checkBoxSsaOpaqueBox.Checked;
            UpdateSsaExample();
        }

        private void numericUpDownSsaOutline_ValueChanged(object sender, EventArgs e)
        {
            UpdateSsaExample();
        }

        private void numericUpDownSsaShadow_ValueChanged(object sender, EventArgs e)
        {
            UpdateSsaExample();
        }

        private void SetPanelColor(Panel colorPanel)
        {
            colorDialogSSAStyle.Color = colorPanel.BackColor;
            if (colorDialogSSAStyle.ShowDialog() == DialogResult.OK)
            {
                colorPanel.BackColor = colorDialogSSAStyle.Color;
                UpdateSsaExample();
            }
        }

        private void buttonPrimaryColor_Click(object sender, EventArgs e)
        {
            SetPanelColor(panelPrimaryColor);
        }

        private void buttonOutlineColor_Click(object sender, EventArgs e)
        {
            SetPanelColor(panelOutlineColor);
        }

        private void buttonShadowColor_Click(object sender, EventArgs e)
        {
            SetPanelColor(panelBackColor);
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            var gs = Configuration.Settings.General;
            gs.MpvPreviewTextPrimaryColor = panelPrimaryColor.BackColor.ToArgb();
            gs.MpvPreviewTextOutlineColor = panelOutlineColor.BackColor.ToArgb();
            gs.MpvPreviewTextBackColor = panelBackColor.BackColor.ToArgb();
            gs.MpvPreviewTextOutlineWidth = numericUpDownSsaOutline.Value;
            gs.MpvPreviewTextShadowWidth = numericUpDownSsaShadow.Value;
            gs.MpvPreviewTextMarginLeft = (int)numericUpDownSsaMarginLeft.Value;
            gs.MpvPreviewTextMarginRight = (int)numericUpDownSsaMarginRight.Value;
            gs.MpvPreviewTextMarginVertical = (int)numericUpDownSsaMarginVertical.Value;

            DialogResult = DialogResult.OK;
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void SettingsMpvPreview_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyData == Keys.Escape)
            {
                DialogResult = DialogResult.Cancel;
            }
        }
    }
}
