using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Logic;
using System;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.Forms.Tts
{
    public sealed partial class RegenerateAudioClip : Form
    {
        public TextToSpeech.FileNameAndSpeedFactor FileNameAndSpeedFactor { get; set; }

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

            Text = LanguageSettings.Current.ExportCustomText.Edit;
            labelText.Text = LanguageSettings.Current.General.Text;
            labelVoice.Text = LanguageSettings.Current.TextToSpeech.Voice;
            buttonReGenerate.Text = LanguageSettings.Current.TextToSpeech.Regenerate;
            buttonCancel.Text = LanguageSettings.Current.General.Cancel;
            UiUtil.FixLargeFonts(this, buttonCancel);

            TextBoxReGenerate.Text = subtitle.Paragraphs[idx].Text;
            textToSpeech.SetCurrentVoices(nikseComboBoxVoice);
        }

        private void buttonReGenerate_Click(object sender, EventArgs e)
        {
            var paragraph = _subtitle.Paragraphs[_index];
            paragraph.Text = TextBoxReGenerate.Text.Trim();

            var next = _subtitle.GetParagraphOrDefault(_index + 1);

            try
            {
                Cursor = Cursors.WaitCursor;
                buttonReGenerate.Enabled = false;
                var fileNameAndSpeedFactor =  _textToSpeech.ReGenerateAudio(paragraph, next, nikseComboBoxVoice.Text);
                if (fileNameAndSpeedFactor != null)
                {
                    FileNameAndSpeedFactor = fileNameAndSpeedFactor;
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
