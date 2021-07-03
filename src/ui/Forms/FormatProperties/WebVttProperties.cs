using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Logic;
using System;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.Forms.FormatProperties
{
    public sealed partial class WebVttProperties : Form
    {
        public WebVttProperties()
        {
            UiUtil.PreInitialize(this);
            InitializeComponent();
            UiUtil.FixFonts(this);
            UiUtil.FixLargeFonts(this, buttonOK);

            Text = string.Format(LanguageSettings.Current.Main.Menu.File.FormatXProperties, new WebVTT().FriendlyName).Replace("...", string.Empty);
            groupBoxAlignment.Text = LanguageSettings.Current.Settings.Alignment;

            labelAn7.Text = LanguageSettings.Current.SubStationAlphaStyles.TopLeft;
            labelAn8.Text = LanguageSettings.Current.SubStationAlphaStyles.TopCenter;
            labelAn9.Text = LanguageSettings.Current.SubStationAlphaStyles.TopRight;
            labelAn4.Text = LanguageSettings.Current.SubStationAlphaStyles.MiddleLeft;
            labelAn5.Text = LanguageSettings.Current.SubStationAlphaStyles.MiddleCenter;
            labelAn6.Text = LanguageSettings.Current.SubStationAlphaStyles.MiddleRight;
            labelAn1.Text = LanguageSettings.Current.SubStationAlphaStyles.BottomLeft;
            labelAn2.Text = LanguageSettings.Current.SubStationAlphaStyles.BottomCenter;
            labelAn3.Text = LanguageSettings.Current.SubStationAlphaStyles.BottomRight;

            textBoxAn1.Text = Configuration.Settings.SubtitleSettings.WebVttCueAn1;
            textBoxAn2.Text = Configuration.Settings.SubtitleSettings.WebVttCueAn2;
            textBoxAn3.Text = Configuration.Settings.SubtitleSettings.WebVttCueAn3;
            textBoxAn4.Text = Configuration.Settings.SubtitleSettings.WebVttCueAn4;
            textBoxAn5.Text = Configuration.Settings.SubtitleSettings.WebVttCueAn5;
            textBoxAn6.Text = Configuration.Settings.SubtitleSettings.WebVttCueAn6;
            textBoxAn7.Text = Configuration.Settings.SubtitleSettings.WebVttCueAn7;
            textBoxAn8.Text = Configuration.Settings.SubtitleSettings.WebVttCueAn8;
            textBoxAn9.Text = Configuration.Settings.SubtitleSettings.WebVttCueAn9;

            checkBoxUseXTimestampMap.Checked = Configuration.Settings.SubtitleSettings.WebVttUseXTimestampMap;
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            Configuration.Settings.SubtitleSettings.WebVttCueAn1 = textBoxAn1.Text;
            Configuration.Settings.SubtitleSettings.WebVttCueAn2 = textBoxAn2.Text;
            Configuration.Settings.SubtitleSettings.WebVttCueAn3 = textBoxAn3.Text;
            Configuration.Settings.SubtitleSettings.WebVttCueAn4 = textBoxAn4.Text;
            Configuration.Settings.SubtitleSettings.WebVttCueAn5 = textBoxAn5.Text;
            Configuration.Settings.SubtitleSettings.WebVttCueAn6 = textBoxAn6.Text;
            Configuration.Settings.SubtitleSettings.WebVttCueAn7 = textBoxAn7.Text;
            Configuration.Settings.SubtitleSettings.WebVttCueAn8 = textBoxAn8.Text;
            Configuration.Settings.SubtitleSettings.WebVttCueAn9 = textBoxAn9.Text;

            Configuration.Settings.SubtitleSettings.WebVttUseXTimestampMap = checkBoxUseXTimestampMap.Checked;

            DialogResult = DialogResult.OK;
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void WebVttProperties_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                DialogResult = DialogResult.Cancel;
            }
        }
    }
}
