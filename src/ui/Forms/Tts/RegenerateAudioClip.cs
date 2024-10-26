using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.VideoPlayers;
using System;
using System.Threading;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.Forms.Tts
{
    public sealed partial class RegenerateAudioClip : Form
    {
        public TextToSpeech.FileNameAndSpeedFactor FileNameAndSpeedFactor { get; set; }

        private readonly TextToSpeech _textToSpeech;
        private readonly Subtitle _subtitle;
        private readonly int _index;
        private LibMpvDynamic _libMpv;

        public RegenerateAudioClip(TextToSpeech textToSpeech, Subtitle subtitle, int idx, TextToSpeech.TextToSpeechEngine engine)
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
            labelStability.Text = LanguageSettings.Current.TextToSpeech.Stability;
            labelSimilarity.Text = LanguageSettings.Current.TextToSpeech.Similarity;
            buttonReGenerate.Text = LanguageSettings.Current.TextToSpeech.Regenerate;
            buttonCancel.Text = LanguageSettings.Current.General.Cancel;
            buttonOK.Text = LanguageSettings.Current.General.Ok;
            buttonPlay.Text = LanguageSettings.Current.TextToSpeech.Play;
            UiUtil.FixLargeFonts(this, buttonCancel);

            TextBoxReGenerate.Text = subtitle.Paragraphs[idx].Text;
            textToSpeech.SetCurrentVoices(nikseComboBoxVoice);
            buttonPlay.Enabled = false;
            nikseUpDownStability.Value = (int)Math.Round(Configuration.Settings.Tools.TextToSpeechElevenLabsStability * 100.0);
            nikseUpDownSimilarity.Value = (int)Math.Round(Configuration.Settings.Tools.TextToSpeechElevenLabsSimilarity * 100.0);

            if (engine.Id == TextToSpeech.TextToSpeechEngineId.ElevenLabs)
            {
            }
            else
            {
                labelStability.Visible = false;
                labelSimilarity.Visible = false;
                nikseUpDownStability.Visible = false;
                nikseUpDownSimilarity.Visible = false;
                TextBoxReGenerate.Height = buttonOK.Top - TextBoxReGenerate.Top - 10;
            }
        }

        private void buttonReGenerate_Click(object sender, EventArgs e)
        {
            Configuration.Settings.Tools.TextToSpeechElevenLabsStability = (double)nikseUpDownStability.Value / 100.0;
            Configuration.Settings.Tools.TextToSpeechElevenLabsSimilarity = (double)nikseUpDownSimilarity.Value / 100.0;

            var paragraph = _subtitle.Paragraphs[_index];
            paragraph.Text = TextBoxReGenerate.Text.Trim();

            var next = _subtitle.GetParagraphOrDefault(_index + 1);

            try
            {
                Cursor = Cursors.WaitCursor;
                buttonReGenerate.Enabled = false;
                var fileNameAndSpeedFactor = _textToSpeech.ReGenerateAudio(paragraph, next, nikseComboBoxVoice.Text);
                if (fileNameAndSpeedFactor != null)
                {
                    FileNameAndSpeedFactor = fileNameAndSpeedFactor;
                    buttonPlay.Enabled = true;
                    Cursor = Cursors.Default;
                    buttonPlay_Click(null, null);
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

        private void buttonOK_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
        }

        private void buttonPlay_Click(object sender, EventArgs e)
        {
            if (FileNameAndSpeedFactor == null)
            {
                return;
            }

            var waveFileName = FileNameAndSpeedFactor.Filename;

            if (_libMpv != null)
            {
                _libMpv.Initialize(
                    null,
                    waveFileName,
                    (sender2, args) =>
                    {
                        _libMpv.PlayRate = 1;
                        _libMpv.Play();
                    },
                    null);

                TaskDelayHelper.RunDelayed(TimeSpan.FromMilliseconds(1000), () =>
                {
                    var i = 0;
                    while (i < 100 && !_libMpv.IsPaused)
                    {
                        i++;
                        Thread.Sleep(100);
                        Application.DoEvents();
                    }

                    buttonPlay.Enabled = true;
                });
            }
            else
            {
                using (var soundPlayer = new System.Media.SoundPlayer(waveFileName))
                {
                    soundPlayer.PlaySync();
                }
            }
        }

        private void RegenerateAudioClip_Shown(object sender, EventArgs e)
        {
            if (LibMpvDynamic.IsInstalled)
            {
                _libMpv = new LibMpvDynamic();
            }
        }
    }
}
