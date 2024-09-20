using System;
using System.Windows.Forms;
using Nikse.SubtitleEdit.Core.AutoTranslate;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.Settings;
using Nikse.SubtitleEdit.Logic;

namespace Nikse.SubtitleEdit.Forms.Translate
{
    public sealed partial class AutoTranslateSettings : Form
    {
        private readonly Type _engineType;

        public AutoTranslateSettings(Type engineType, string engineName)
        {
            UiUtil.PreInitialize(this);
            InitializeComponent();
            UiUtil.FixFonts(this);

            _engineType = engineType;

            Text = LanguageSettings.Current.General.Advanced;
            labelDelay.Text = LanguageSettings.Current.GoogleTranslate.Delay;
            labelMaxBytes.Text = LanguageSettings.Current.GoogleTranslate.MaxBytes;
            labelMaxMerges.Text = LanguageSettings.Current.GoogleTranslate.MaxMerges;
            labelParagraphHandling.Text = LanguageSettings.Current.GoogleTranslate.LineMergeHandling;
            labelPrompt.Text = string.Format(LanguageSettings.Current.GoogleTranslate.PromptX, engineName);
            buttonOk.Text = LanguageSettings.Current.General.Ok;
            buttonCancel.Text = LanguageSettings.Current.General.Cancel;

            nikseUpDownDelay.Value = Configuration.Settings.Tools.AutoTranslateDelaySeconds;

            if (Configuration.Settings.Tools.AutoTranslateMaxBytes <= 0)
            {
                Configuration.Settings.Tools.AutoTranslateMaxBytes = new ToolsSettings().AutoTranslateMaxBytes;
            }
            nikseUpDownMaxBytes.Value = Configuration.Settings.Tools.AutoTranslateMaxBytes;

            nikseUpDownMaxMerges.Value = Configuration.Settings.Tools.AutoTranslateMaxMerges;

            comboBoxParagraphHandling.Left = labelParagraphHandling.Right + 4;
            nikseUpDownDelay.Left = labelDelay.Right + 4;
            nikseUpDownMaxBytes.Left = labelMaxBytes.Right + 4;
            nikseUpDownMaxMerges.Left = labelMaxMerges.Right + 4;

            if (_engineType == typeof(ChatGptTranslate))
            {
                nikseTextBoxPrompt.Text = Configuration.Settings.Tools.ChatGptPrompt;
                if (string.IsNullOrWhiteSpace(nikseTextBoxPrompt.Text))
                {
                    nikseTextBoxPrompt.Text = new ToolsSettings().ChatGptPrompt;
                }
            }
            else if (_engineType == typeof(OllamaTranslate))
            {
                nikseTextBoxPrompt.Text = Configuration.Settings.Tools.OllamaPrompt;
                if (string.IsNullOrWhiteSpace(nikseTextBoxPrompt.Text))
                {
                    nikseTextBoxPrompt.Text = new ToolsSettings().OllamaPrompt;
                }
            }
            else if (_engineType == typeof(LmStudioTranslate))
            {
                nikseTextBoxPrompt.Text = Configuration.Settings.Tools.LmStudioPrompt;
                if (string.IsNullOrWhiteSpace(nikseTextBoxPrompt.Text))
                {
                    nikseTextBoxPrompt.Text = new ToolsSettings().LmStudioPrompt;
                }
            }
            else if (_engineType == typeof(AnthropicTranslate))
            {
                nikseTextBoxPrompt.Text = Configuration.Settings.Tools.AnthropicPrompt;
                if (string.IsNullOrWhiteSpace(nikseTextBoxPrompt.Text))
                {
                    nikseTextBoxPrompt.Text = new ToolsSettings().AnthropicPrompt;
                }
            }
            else if (_engineType == typeof(GroqTranslate))
            {
                nikseTextBoxPrompt.Text = Configuration.Settings.Tools.GroqPrompt;
                if (string.IsNullOrWhiteSpace(nikseTextBoxPrompt.Text))
                {
                    nikseTextBoxPrompt.Text = new ToolsSettings().GroqPrompt;
                }
            }
            else if (_engineType == typeof(OpenRouterTranslate))
            {
                nikseTextBoxPrompt.Text = Configuration.Settings.Tools.OpenRouterPrompt;
                if (string.IsNullOrWhiteSpace(nikseTextBoxPrompt.Text))
                {
                    nikseTextBoxPrompt.Text = new ToolsSettings().OpenRouterPrompt;
                }
            }
            else
            {
                labelPrompt.Visible = false;
                nikseTextBoxPrompt.Visible = false;
            }

            comboBoxParagraphHandling.Items.Clear();
            comboBoxParagraphHandling.Items.Add(LanguageSettings.Current.GenerateVideoWithEmbeddedSubs.Default);
            comboBoxParagraphHandling.Items.Add(LanguageSettings.Current.GoogleTranslate.TranslateLinesSeparately);
            comboBoxParagraphHandling.SelectedIndex = 0;
            if (Enum.TryParse<TranslateStrategy>(Configuration.Settings.Tools.AutoTranslateStrategy, out var ts) &&
                ts == TranslateStrategy.TranslateEachLineSeparately)
            {
                comboBoxParagraphHandling.SelectedIndex = 1;
            }
        }

        private void buttonOk_Click(object sender, EventArgs e)
        {
            if (nikseTextBoxPrompt.Text.Replace("{0}", string.Empty).Replace("{1}", string.Empty).Contains('{'))
            {
                MessageBox.Show("Character not allowed in prompt: '{' (besides '{0}' and '{1}')");
                return;
            }

            if (nikseTextBoxPrompt.Text.Replace("{0}", string.Empty).Replace("{1}", string.Empty).Contains('}'))
            {
                MessageBox.Show("Character not allowed in prompt: '}' (besides '{0}' and '{1}')");
                return;
            }

            if (nikseTextBoxPrompt.Text.Length > 1000)
            {
                MessageBox.Show("Too many characters in prompt");
                return;
            }

            Configuration.Settings.Tools.AutoTranslateDelaySeconds = (int)nikseUpDownDelay.Value;
            Configuration.Settings.Tools.AutoTranslateMaxBytes = (int)nikseUpDownMaxBytes.Value;
            Configuration.Settings.Tools.AutoTranslateMaxMerges = (int)nikseUpDownMaxMerges.Value;

            if (_engineType == typeof(ChatGptTranslate))
            {
                Configuration.Settings.Tools.ChatGptPrompt = nikseTextBoxPrompt.Text;
            }
            else if (_engineType == typeof(OllamaTranslate))
            {
                Configuration.Settings.Tools.OllamaPrompt = nikseTextBoxPrompt.Text;
            }
            else if (_engineType == typeof(LmStudioTranslate))
            {
                Configuration.Settings.Tools.LmStudioPrompt = nikseTextBoxPrompt.Text;
            }
            else if (_engineType == typeof(AnthropicTranslate))
            {
                Configuration.Settings.Tools.AnthropicPrompt = nikseTextBoxPrompt.Text;
            }
            else if (_engineType == typeof(GroqTranslate))
            {
                Configuration.Settings.Tools.GroqPrompt = nikseTextBoxPrompt.Text;
            }
            else if (_engineType == typeof(OpenRouterTranslate))
            {
                Configuration.Settings.Tools.OpenRouterPrompt = nikseTextBoxPrompt.Text;
            }

            if (comboBoxParagraphHandling.SelectedIndex == 1)
            {
                Configuration.Settings.Tools.AutoTranslateStrategy = TranslateStrategy.TranslateEachLineSeparately.ToString();
            }
            else
            {
                Configuration.Settings.Tools.AutoTranslateStrategy = TranslateStrategy.Default.ToString();
            }

            DialogResult = DialogResult.OK;
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void buttonCancel_KeyDown(object sender, KeyEventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void AutoTranslateSettings_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                DialogResult = DialogResult.Cancel;
            }
            else if (e.KeyData == UiUtil.HelpKeys)
            {
                UiUtil.ShowHelp("#translation");
            }
        }
    }
}
