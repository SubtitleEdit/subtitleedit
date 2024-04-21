using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Logic;
using System;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.Forms.Tts
{
    public partial class RegenerateAudioClip : Form
    {
        public string NewAudioFileName { get; set; }

        private readonly TextToSpeech _textToSpeech;
        private readonly Subtitle _subtitle;
        private readonly int _index;

        public RegenerateAudioClip(TextToSpeech textToSpeech, Subtitle subtitle, int idx)
        {
            UiUtil.PreInitialize(this);
            InitializeComponent();
            UiUtil.FixFonts(this);

            _subtitle = subtitle;
            _textToSpeech = textToSpeech;
            _index = idx;

            buttonCancel.Text = LanguageSettings.Current.General.Cancel;
            UiUtil.FixLargeFonts(this, buttonCancel);

            TextBoxReGenerate.Text = subtitle.Paragraphs[idx].Text;
            textToSpeech.SetCurrentVoices(nikseComboBoxVoice);
        }

        private async void buttonReGenerate_Click(object sender, EventArgs e)
        {
            var paragraph = _subtitle.Paragraphs[_index];
            paragraph.Text = TextBoxReGenerate.Text.Trim();

            try
            {
                Cursor = Cursors.WaitCursor;
                buttonReGenerate.Enabled = false;
                var fileName = await _textToSpeech.GenerateAudio(paragraph, nikseComboBoxVoice.Text);
                if (!string.IsNullOrEmpty(fileName))
                {
                    NewAudioFileName = fileName;
                    Cursor = Cursors.Default;
                    DialogResult = DialogResult.OK;
                }
            }
            finally
            {
                Cursor = Cursors.Default;
                buttonReGenerate.Enabled = true;
            }
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }
    }
}
