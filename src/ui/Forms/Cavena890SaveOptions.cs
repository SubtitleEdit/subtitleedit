using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Logic;
using System;
using System.IO;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.Forms
{
    public partial class Cavena890SaveOptions : Form
    {
        public Cavena890SaveOptions(Subtitle subtitle, string subtitleFileName)
        {
            UiUtil.PreInitialize(this);
            InitializeComponent();
            UiUtil.FixFonts(this);

            buttonCancel.Text = LanguageSettings.Current.General.Cancel;
            buttonOK.Text = LanguageSettings.Current.General.Ok;

            timeUpDownStartTime.ForceHHMMSSFF();
            timeUpDownStartTime.TimeCode = new TimeCode(TimeCode.ParseHHMMSSFFToMilliseconds(Configuration.Settings.SubtitleSettings.Cavena890StartOfMessage));
            textBoxTranslatedTitle.Text = Configuration.Settings.SubtitleSettings.CurrentCavena89Title;
            textBoxOriginalTitle.Text = Configuration.Settings.SubtitleSettings.CurrentCavena890riginalTitle;
            textBoxTranslator.Text = Configuration.Settings.SubtitleSettings.CurrentCavena890Translator;
            textBoxComment.Text = Configuration.Settings.SubtitleSettings.CurrentCavena89Comment;
            if (string.IsNullOrWhiteSpace(textBoxComment.Text))
            {
                textBoxComment.Text = "Made with Subtitle Edit";
            }

            if (string.IsNullOrWhiteSpace(Configuration.Settings.SubtitleSettings.CurrentCavena89Title))
            {
                string title = Path.GetFileNameWithoutExtension(subtitleFileName) ?? string.Empty;
                if (title.Length > 28)
                {
                    title = title.Substring(0, 28);
                }

                textBoxTranslatedTitle.Text = title;
            }

            var language = LanguageAutoDetect.AutoDetectGoogleLanguage(subtitle);
            switch (language)
            {
                case "he":
                    Configuration.Settings.SubtitleSettings.CurrentCavena89LanguageId = Cavena890.LanguageIdHebrew;
                    comboBoxLanguage.SelectedIndex = 5;
                    break;
                case "ru":
                    Configuration.Settings.SubtitleSettings.CurrentCavena89LanguageId = Cavena890.LanguageIdRussian;
                    comboBoxLanguage.SelectedIndex = 6;
                    break;
                case "ro":
                    Configuration.Settings.SubtitleSettings.CurrentCavena89LanguageId = Cavena890.LanguageIdRomanian;
                    comboBoxLanguage.SelectedIndex = 7;
                    break;
                case "zh":
                    Configuration.Settings.SubtitleSettings.CurrentCavena89LanguageId = Cavena890.LanguageIdChineseSimplified;
                    comboBoxLanguage.SelectedIndex = 2;
                    break;
                case "da":
                    Configuration.Settings.SubtitleSettings.CurrentCavena89LanguageId = Cavena890.LanguageIdDanish;
                    comboBoxLanguage.SelectedIndex = 1;
                    break;
                default:
                    Configuration.Settings.SubtitleSettings.CurrentCavena89LanguageId = Cavena890.LanguageIdEnglish;
                    comboBoxLanguage.SelectedIndex = 4;
                    break;
            }
        }

        private void Cavena890SaveOptions_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                DialogResult = DialogResult.Cancel;
            }
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            Configuration.Settings.SubtitleSettings.Cavena890StartOfMessage = timeUpDownStartTime.TimeCode.ToHHMMSSFF();
            Configuration.Settings.SubtitleSettings.CurrentCavena89Title = textBoxTranslatedTitle.Text;
            Configuration.Settings.SubtitleSettings.CurrentCavena890riginalTitle = textBoxOriginalTitle.Text;
            Configuration.Settings.SubtitleSettings.CurrentCavena890Translator = textBoxTranslator.Text;
            Configuration.Settings.SubtitleSettings.CurrentCavena89Comment = textBoxComment.Text;

            switch (comboBoxLanguage.SelectedIndex)
            {
                case 0:
                    Configuration.Settings.SubtitleSettings.CurrentCavena89LanguageId = Cavena890.LanguageIdArabic;
                    break;
                case 1:
                    Configuration.Settings.SubtitleSettings.CurrentCavena89LanguageId = Cavena890.LanguageIdDanish;
                    break;
                case 2:
                    Configuration.Settings.SubtitleSettings.CurrentCavena89LanguageId = Cavena890.LanguageIdChineseSimplified;
                    break;
                case 3:
                    Configuration.Settings.SubtitleSettings.CurrentCavena89LanguageId = Cavena890.LanguageIdChineseTraditional;
                    break;
                case 5:
                    Configuration.Settings.SubtitleSettings.CurrentCavena89LanguageId = Cavena890.LanguageIdHebrew;
                    break;
                case 6:
                    Configuration.Settings.SubtitleSettings.CurrentCavena89LanguageId = Cavena890.LanguageIdRussian;
                    break;
                default:
                    Configuration.Settings.SubtitleSettings.CurrentCavena89LanguageId = Cavena890.LanguageIdEnglish;
                    break;
            }

            DialogResult = DialogResult.OK;
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void buttonImport_Click(object sender, EventArgs e)
        {
            openFileDialog1.Filter = "Cavena 890 (*.890)|*.890";
            openFileDialog1.FileName = string.Empty;
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                var buffer = FileUtil.ReadAllBytesShared(openFileDialog1.FileName);
                if (buffer.Length > 270)
                {
                    var s = System.Text.Encoding.ASCII.GetString(buffer, 40, 28);
                    textBoxTranslatedTitle.Text = s.Replace("\0", string.Empty);

                    s = System.Text.Encoding.ASCII.GetString(buffer, 218, 28);
                    textBoxOriginalTitle.Text = s.Replace("\0", string.Empty);

                    s = System.Text.Encoding.ASCII.GetString(buffer, 68, 28);
                    textBoxTranslator.Text = s.Replace("\0", string.Empty);

                    s = System.Text.Encoding.ASCII.GetString(buffer, 148, 24);
                    textBoxComment.Text = s.Replace("\0", string.Empty);

                    s = System.Text.Encoding.ASCII.GetString(buffer, 256, 11);
                    timeUpDownStartTime.TimeCode = new TimeCode(TimeCode.ParseHHMMSSFFToMilliseconds(s));

                    switch (buffer[146])
                    {
                        case Cavena890.LanguageIdHebrew:
                            Configuration.Settings.SubtitleSettings.CurrentCavena89LanguageId = Cavena890.LanguageIdHebrew;
                            comboBoxLanguage.SelectedIndex = 5;
                            break;
                        case Cavena890.LanguageIdRussian:
                            Configuration.Settings.SubtitleSettings.CurrentCavena89LanguageId = Cavena890.LanguageIdRussian;
                            comboBoxLanguage.SelectedIndex = 6;
                            break;
                        case Cavena890.LanguageIdChineseSimplified:
                            Configuration.Settings.SubtitleSettings.CurrentCavena89LanguageId = Cavena890.LanguageIdChineseSimplified;
                            comboBoxLanguage.SelectedIndex = 2;
                            break;
                        case Cavena890.LanguageIdChineseTraditional:
                            Configuration.Settings.SubtitleSettings.CurrentCavena89LanguageId = Cavena890.LanguageIdChineseSimplified;
                            comboBoxLanguage.SelectedIndex = 3;
                            break;
                        case Cavena890.LanguageIdDanish:
                            Configuration.Settings.SubtitleSettings.CurrentCavena89LanguageId = Cavena890.LanguageIdDanish;
                            comboBoxLanguage.SelectedIndex = 1;
                            break;
                        case Cavena890.LanguageIdRomanian:
                            Configuration.Settings.SubtitleSettings.CurrentCavena89LanguageId = Cavena890.LanguageIdRomanian;
                            comboBoxLanguage.SelectedIndex = 7;
                            break;
                        default:
                            Configuration.Settings.SubtitleSettings.CurrentCavena89LanguageId = Cavena890.LanguageIdEnglish;
                            comboBoxLanguage.SelectedIndex = 4;
                            break;
                    }
                }
            }
        }

    }
}
