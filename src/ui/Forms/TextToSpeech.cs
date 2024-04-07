using Nikse.SubtitleEdit.Logic;
using System;
using System.Speech.Synthesis;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.Forms
{
    public partial class TextToSpeech : Form
    {
        public TextToSpeech(Core.Common.Subtitle subtitle, string videoFileName)
        {
            UiUtil.PreInitialize(this);
            InitializeComponent();
            UiUtil.FixFonts(this);

            buttonOK.Text = LanguageSettings.Current.General.Ok;
            UiUtil.FixLargeFonts(this, buttonOK);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var text = textBox1.Text;

            using (SpeechSynthesizer synth = new SpeechSynthesizer())
            {
                synth.SetOutputToWaveFile(@"C:\data\Sample.wav");
                synth.SelectVoiceByHints(VoiceGender.Male, VoiceAge.Adult);
                var builder = new PromptBuilder();
                builder.AppendText(text);
                synth.Speak(builder);
            }
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
        }
    }
}
