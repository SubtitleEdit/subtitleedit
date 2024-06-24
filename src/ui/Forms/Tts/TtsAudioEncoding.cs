using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Logic;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.Forms.Tts
{
    public sealed partial class TtsAudioEncoding : Form
    {
        public TtsAudioEncoding()
        {
            UiUtil.PreInitialize(this);
            InitializeComponent();
            UiUtil.FixFonts(this);
            Text = LanguageSettings.Current.GenerateVideoWithBurnedInSubs.Audio;
            buttonCancel.Text = LanguageSettings.Current.General.Cancel;
            buttonOK.Text = LanguageSettings.Current.General.Ok;
            UiUtil.FixLargeFonts(this, buttonOK);

            if (string.IsNullOrWhiteSpace(Configuration.Settings.Tools.TextToSpeechCustomAudioEncoding))
            {
                Configuration.Settings.Tools.TextToSpeechCustomAudioEncoding = "copy";
            }

            checkBoxMakeStereo.Checked = Configuration.Settings.Tools.TextToSpeechCustomAudioStereo;
            comboBoxAudioEnc.Text = Configuration.Settings.Tools.TextToSpeechCustomAudioEncoding;
        }

        private void buttonOK_Click(object sender, System.EventArgs e)
        {
            Configuration.Settings.Tools.TextToSpeechCustomAudioStereo = checkBoxMakeStereo.Checked;
            Configuration.Settings.Tools.TextToSpeechCustomAudioEncoding = comboBoxAudioEnc.Text;

            DialogResult = DialogResult.OK;
        }

        private void buttonCancel_Click(object sender, System.EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }
    }
}
