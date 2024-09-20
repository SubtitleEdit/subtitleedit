using Nikse.SubtitleEdit.Controls;
using Nikse.SubtitleEdit.Core.AutoTranslate;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.Translate;
using Nikse.SubtitleEdit.Forms.Options;
using Nikse.SubtitleEdit.Logic;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using MessageBox = Nikse.SubtitleEdit.Forms.SeMsgBox.MessageBox;
using Timer = System.Windows.Forms.Timer;

namespace Nikse.SubtitleEdit.Forms.Translate
{
    public sealed partial class AutoTranslate : Form
    {
        public Subtitle TranslatedSubtitle { get; }
        public string TwoLetterIsoSource { get; internal set; }
        public string TwoLetterIsoTarget { get; internal set; }

        private readonly Subtitle _subtitle;
        private readonly Encoding _encoding;
        private IAutoTranslator _autoTranslator;
        private List<IAutoTranslator> _autoTranslatorEngines;
        private int _translationProgressIndex = -1;
        private bool _translationProgressDirty = true;
        private bool _breakTranslation;
        private bool _translationInProgress;
        private bool _singleLineMode;
        private CancellationTokenSource _cancellationTokenSource;

        public AutoTranslate(Subtitle subtitle, Subtitle targetLines, string title, Encoding encoding)
        {
            UiUtil.PreInitialize(this);
            InitializeComponent();
            UiUtil.FixFonts(this);

            Text = LanguageSettings.Current.Main.VideoControls.AutoTranslate;
            buttonTranslate.Text = LanguageSettings.Current.GoogleTranslate.Translate;
            labelPleaseWait.Text = LanguageSettings.Current.GoogleTranslate.PleaseWait;
            buttonStrategy.Text = LanguageSettings.Current.General.Advanced;
            buttonOK.Text = LanguageSettings.Current.General.Ok;
            buttonCancel.Text = LanguageSettings.Current.General.Cancel;
            labelUrl.Text = LanguageSettings.Current.Main.Url;
            labelApiKey.Text = LanguageSettings.Current.Settings.GoogleTranslateApiKey;
            nikseComboBoxUrl.Left = labelUrl.Right + 5;
            startLibreTranslateServerToolStripMenuItem.Text = string.Format(LanguageSettings.Current.GoogleTranslate.StartWebServerX, new LibreTranslate().Name);
            startNLLBServeServerToolStripMenuItem.Text = string.Format(LanguageSettings.Current.GoogleTranslate.StartWebServerX, new NoLanguageLeftBehindServe().Name);
            startNLLBAPIServerToolStripMenuItem.Text = string.Format(LanguageSettings.Current.GoogleTranslate.StartWebServerX, new NoLanguageLeftBehindApi().Name);
            labelSource.Text = LanguageSettings.Current.GoogleTranslate.From;
            labelTarget.Text = LanguageSettings.Current.GoogleTranslate.To;
            toolStripMenuItemStartLibre.Text = string.Format(LanguageSettings.Current.GoogleTranslate.StartWebServerX, new LibreTranslate().Name);
            toolStripMenuItemStartNLLBServe.Text = string.Format(LanguageSettings.Current.GoogleTranslate.StartWebServerX, new NoLanguageLeftBehindServe().Name);
            toolStripMenuItemStartNLLBApi.Text = string.Format(LanguageSettings.Current.GoogleTranslate.StartWebServerX, new NoLanguageLeftBehindApi().Name);
            labelFormality.Text = LanguageSettings.Current.GoogleTranslate.Formality;
            comboBoxFormality.Left = labelFormality.Right + 3;

            subtitleListViewSource.InitializeLanguage(LanguageSettings.Current.General, Configuration.Settings);
            subtitleListViewTarget.InitializeLanguage(LanguageSettings.Current.General, Configuration.Settings);
            subtitleListViewSource.HideColumn(SubtitleListView.SubtitleColumn.CharactersPerSeconds);
            subtitleListViewSource.HideColumn(SubtitleListView.SubtitleColumn.WordsPerMinute);
            subtitleListViewTarget.HideColumn(SubtitleListView.SubtitleColumn.CharactersPerSeconds);
            subtitleListViewTarget.HideColumn(SubtitleListView.SubtitleColumn.WordsPerMinute);
            UiUtil.InitializeSubtitleFont(subtitleListViewSource);
            UiUtil.InitializeSubtitleFont(subtitleListViewTarget);
            subtitleListViewSource.HideColumn(SubtitleListView.SubtitleColumn.End);
            subtitleListViewSource.HideColumn(SubtitleListView.SubtitleColumn.Gap);
            subtitleListViewTarget.HideColumn(SubtitleListView.SubtitleColumn.End);
            subtitleListViewTarget.HideColumn(SubtitleListView.SubtitleColumn.Gap);
            subtitleListViewSource.AutoSizeColumns();
            subtitleListViewSource.AutoSizeColumns();
            UiUtil.FixLargeFonts(this, buttonOK);
            ActiveControl = buttonTranslate;

            if (!string.IsNullOrEmpty(title))
            {
                Text = title;
            }

            _subtitle = new Subtitle(subtitle, false);
            _encoding = encoding;

            InitializeAutoTranslatorEngines();

            nikseComboBoxUrl.UsePopupWindow = true;

            labelPleaseWait.Visible = false;
            progressBar1.Visible = false;

            if (targetLines != null)
            {
                TranslatedSubtitle = new Subtitle(targetLines, false);
                TranslatedSubtitle.Renumber();
                subtitleListViewTarget.Fill(TranslatedSubtitle);
            }
            else
            {
                TranslatedSubtitle = new Subtitle(_subtitle, false);
                foreach (var paragraph in TranslatedSubtitle.Paragraphs)
                {
                    paragraph.Text = string.Empty;
                }
            }

            subtitleListViewSource.Fill(_subtitle);
            AutoTranslate_Resize(null, null);
            UpdateTranslation();
            MergeAndSplitHelper.MergeSplitProblems = false;
        }

        private void InitializeAutoTranslatorEngines()
        {
            _autoTranslatorEngines = new List<IAutoTranslator>
            {
                new GoogleTranslateV1(),
                new GoogleTranslateV2(),
                new MicrosoftTranslator(),
                new DeepLTranslate(),
                new LibreTranslate(),
                new MyMemoryApi(),
                new ChatGptTranslate(),
                new LmStudioTranslate(),
                new OllamaTranslate(),
                new AnthropicTranslate(),
                new GroqTranslate(),
                new OpenRouterTranslate(),
                new GeminiTranslate(),
                new PapagoTranslate(),
                new NoLanguageLeftBehindServe(),
                new NoLanguageLeftBehindApi(),
            };

            nikseComboBoxEngine.Items.Clear();
            nikseComboBoxEngine.Items.AddRange(_autoTranslatorEngines.Select(p => p.Name).ToArray<object>());

            if (!string.IsNullOrEmpty(Configuration.Settings.Tools.AutoTranslateLastName))
            {
                var lastEngine = _autoTranslatorEngines.FirstOrDefault(p => p.Name == Configuration.Settings.Tools.AutoTranslateLastName);
                if (lastEngine != null)
                {
                    _autoTranslator = lastEngine;
                    nikseComboBoxEngine.SelectedIndex = _autoTranslatorEngines.IndexOf(lastEngine);
                }
            }

            if (nikseComboBoxEngine.SelectedIndex < 0)
            {
                _autoTranslator = _autoTranslatorEngines[0];
                nikseComboBoxEngine.SelectedIndex = 0;
            }

            if (!string.IsNullOrEmpty(Configuration.Settings.Tools.AutoTranslateLastUrl))
            {
                nikseComboBoxUrl.Text = Configuration.Settings.Tools.AutoTranslateLastUrl;
            }
        }

        private void SetAutoTranslatorEngine()
        {
            _autoTranslator = GetCurrentEngine();
            linkLabelPoweredBy.Text = string.Format(LanguageSettings.Current.GoogleTranslate.PoweredByX, _autoTranslator.Name);
            nikseTextBoxApiKey.Visible = false;
            labelUrl.Visible = false;
            labelApiKey.Visible = false;
            nikseComboBoxUrl.Visible = false;
            nikseTextBoxApiKey.Top = nikseComboBoxUrl.Top;
            labelApiKey.Text = LanguageSettings.Current.Settings.GoogleTranslateApiKey;
            labelFormality.Visible = false;
            comboBoxFormality.ContextMenuStrip = null;
            comboBoxFormality.Visible = false;
            var engineType = _autoTranslator.GetType();

            if (engineType == typeof(GoogleTranslateV1))
            {
                return;
            }

            if (engineType == typeof(GoogleTranslateV2))
            {
                labelApiKey.Left = labelUrl.Left;
                nikseTextBoxApiKey.Text = Configuration.Settings.Tools.GoogleApiV2Key;
                nikseTextBoxApiKey.Left = labelApiKey.Right + 3;
                labelApiKey.Visible = true;
                nikseTextBoxApiKey.Visible = true;
                return;
            }

            if (engineType == typeof(MicrosoftTranslator))
            {
                labelApiKey.Left = labelUrl.Left;
                nikseTextBoxApiKey.Text = Configuration.Settings.Tools.MicrosoftTranslatorApiKey;
                nikseTextBoxApiKey.Left = labelApiKey.Right + 3;
                labelApiKey.Visible = true;
                nikseTextBoxApiKey.Visible = true;
                return;
            }

            if (engineType == typeof(DeepLTranslate))
            {
                labelFormality.Visible = true;
                labelFormality.Text = LanguageSettings.Current.GoogleTranslate.Formality;
                comboBoxFormality.Left = labelFormality.Right + 3;
                comboBoxFormality.Visible = true;
                comboBoxFormality.DropDownStyle = ComboBoxStyle.DropDownList;

                FillUrls(new List<string>
                {
                    Configuration.Settings.Tools.AutoTranslateDeepLUrl,
                    Configuration.Settings.Tools.AutoTranslateDeepLUrl.Contains("api-free.deepl.com") ? "https://api.deepl.com/" : "https://api-free.deepl.com/",
                });

                labelApiKey.Left = nikseComboBoxUrl.Right + 12;
                nikseTextBoxApiKey.Text = Configuration.Settings.Tools.AutoTranslateDeepLApiKey;
                nikseTextBoxApiKey.Left = labelApiKey.Right + 3;
                labelApiKey.Visible = true;
                nikseTextBoxApiKey.Visible = true;

                SelectFormality();

                return;
            }

            if (engineType == typeof(NoLanguageLeftBehindServe))
            {
                FillUrls(new List<string>
                {
                    Configuration.Settings.Tools.AutoTranslateNllbServeUrl,
                    "http://127.0.0.1:6060/",
                    "http://192.168.8.127:6060/",
                });

                return;
            }

            if (engineType == typeof(NoLanguageLeftBehindApi))
            {
                FillUrls(new List<string>
                {
                    Configuration.Settings.Tools.AutoTranslateNllbApiUrl,
                    "http://localhost:7860/api/v2/",
                    "https://winstxnhdw-nllb-api.hf.space/api/v2/",
                });

                return;
            }

            if (engineType == typeof(LibreTranslate))
            {
                FillUrls(new List<string>
                {
                    Configuration.Settings.Tools.AutoTranslateLibreUrl,
                    "http://localhost:5000/",
                    "https://libretranslate.com/",
                    "https://translate.argosopentech.com/",
                    "https://translate.terraprint.co/",
                });

                return;
            }

            if (engineType == typeof(PapagoTranslate))
            {
                nikseComboBoxUrl.Items.Clear();
                nikseComboBoxUrl.Items.Add(Configuration.Settings.Tools.AutoTranslatePapagoApiKeyId);
                nikseComboBoxUrl.SelectedIndex = 0;
                nikseComboBoxUrl.Visible = true;
                labelUrl.Visible = true;
                labelUrl.Text = "Client ID";
                nikseComboBoxUrl.Left = labelUrl.Right + 3;

                labelApiKey.Left = nikseComboBoxUrl.Right + 12;
                labelApiKey.Text = "Client secret";
                nikseTextBoxApiKey.Text = Configuration.Settings.Tools.AutoTranslatePapagoApiKey;
                nikseTextBoxApiKey.Left = labelApiKey.Right + 3;
                labelApiKey.Visible = true;
                nikseTextBoxApiKey.Visible = true;

                return;
            }


            if (engineType == typeof(MyMemoryApi))
            {
                labelApiKey.Left = labelUrl.Left;
                nikseTextBoxApiKey.Text = Configuration.Settings.Tools.AutoTranslateMyMemoryApiKey;
                nikseTextBoxApiKey.Left = labelApiKey.Right + 3;
                labelApiKey.Visible = true;
                nikseTextBoxApiKey.Visible = true;

                return;
            }

            if (engineType == typeof(ChatGptTranslate))
            {
                if (Configuration.Settings.Tools.ChatGptUrl == null)
                {
                    Configuration.Settings.Tools.ChatGptUrl = "https://api.openai.com/v1/chat/completions";
                }

                FillUrls(new List<string>
                {
                    Configuration.Settings.Tools.ChatGptUrl.TrimEnd('/'),
                    Configuration.Settings.Tools.ChatGptUrl.StartsWith("http://localhost:1234/v1/chat/completions", StringComparison.OrdinalIgnoreCase) ? "https://api.openai.com/v1/chat/completions" : "http://localhost:1234/v1/chat/completions"
                });

                labelFormality.Text = LanguageSettings.Current.AudioToText.Model;
                labelFormality.Enabled = true;
                labelFormality.Visible = true;

                comboBoxFormality.DropDownStyle = ComboBoxStyle.DropDown;
                comboBoxFormality.Items.Clear();
                comboBoxFormality.Enabled = true;
                comboBoxFormality.Left = labelFormality.Right + 3;
                comboBoxFormality.Visible = true;
                comboBoxFormality.Items.AddRange(ChatGptTranslate.Models);
                comboBoxFormality.Text = Configuration.Settings.Tools.ChatGptModel;

                labelApiKey.Left = nikseComboBoxUrl.Right + 12;
                nikseTextBoxApiKey.Text = Configuration.Settings.Tools.ChatGptApiKey;
                nikseTextBoxApiKey.Left = labelApiKey.Right + 3;
                labelApiKey.Visible = true;
                nikseTextBoxApiKey.Visible = true;
                return;
            }

            if (engineType == typeof(LmStudioTranslate))
            {
                if (string.IsNullOrEmpty(Configuration.Settings.Tools.LmStudioApiUrl))
                {
                    Configuration.Settings.Tools.LmStudioApiUrl = "http://localhost:1234/v1/chat/completions";
                }

                FillUrls(new List<string>
                {
                    Configuration.Settings.Tools.LmStudioApiUrl.TrimEnd('/'),
                });

                return;
            }

            if (engineType == typeof(OllamaTranslate))
            {
                if (Configuration.Settings.Tools.OllamaApiUrl == null)
                {
                    Configuration.Settings.Tools.OllamaApiUrl = "http://localhost:11434/api/generate";
                }

                FillUrls(new List<string>
                {
                    Configuration.Settings.Tools.OllamaApiUrl.TrimEnd('/'),
                });

                var models = Configuration.Settings.Tools.OllamaModels.Split(',').ToList();

                labelFormality.Text = LanguageSettings.Current.AudioToText.Model;
                labelFormality.Enabled = true;
                labelFormality.Visible = true;

                comboBoxFormality.DropDownStyle = ComboBoxStyle.DropDown;
                comboBoxFormality.Items.Clear();
                comboBoxFormality.Enabled = true;
                comboBoxFormality.Left = labelFormality.Right + 3;
                comboBoxFormality.Visible = true;
                foreach (var model in models)
                {
                    comboBoxFormality.Items.Add(model);
                }
                comboBoxFormality.Text = Configuration.Settings.Tools.OllamaModel;

                comboBoxFormality.ContextMenuStrip = contextMenuStripOlamaModels;

                return;
            }

            if (engineType == typeof(AnthropicTranslate))
            {
                FillUrls(new List<string>
                {
                    Configuration.Settings.Tools.AnthropicApiUrl,
                });

                labelApiKey.Left = nikseComboBoxUrl.Right + 12;
                nikseTextBoxApiKey.Text = Configuration.Settings.Tools.AnthropicApiKey;
                nikseTextBoxApiKey.Left = labelApiKey.Right + 3;
                labelApiKey.Visible = true;
                nikseTextBoxApiKey.Visible = true;

                labelFormality.Text = LanguageSettings.Current.AudioToText.Model;
                labelFormality.Visible = true;
                comboBoxFormality.Left = labelFormality.Right + 3;
                comboBoxFormality.Visible = true;
                comboBoxFormality.DropDownStyle = ComboBoxStyle.DropDown;
                comboBoxFormality.Items.Clear();
                comboBoxFormality.Items.AddRange(AnthropicTranslate.Models);
                comboBoxFormality.Text = Configuration.Settings.Tools.AnthropicApiModel;

                return;
            }

            if (engineType == typeof(GroqTranslate))
            {
                FillUrls(new List<string>
                {
                    Configuration.Settings.Tools.GroqUrl,
                });

                labelApiKey.Left = nikseComboBoxUrl.Right + 12;
                nikseTextBoxApiKey.Text = Configuration.Settings.Tools.GroqApiKey;
                nikseTextBoxApiKey.Left = labelApiKey.Right + 3;
                labelApiKey.Visible = true;
                nikseTextBoxApiKey.Visible = true;

                labelFormality.Text = LanguageSettings.Current.AudioToText.Model;
                labelFormality.Visible = true;
                comboBoxFormality.Left = labelFormality.Right + 3;
                comboBoxFormality.Visible = true;
                comboBoxFormality.DropDownStyle = ComboBoxStyle.DropDown;
                comboBoxFormality.Items.Clear();
                comboBoxFormality.Items.AddRange(GroqTranslate.Models);
                comboBoxFormality.Text = Configuration.Settings.Tools.GroqModel;

                return;
            }


            if (engineType == typeof(OpenRouterTranslate))
            {
                FillUrls(new List<string>
                {
                    Configuration.Settings.Tools.OpenRouterUrl,
                });

                labelApiKey.Left = nikseComboBoxUrl.Right + 12;
                nikseTextBoxApiKey.Text = Configuration.Settings.Tools.OpenRouterApiKey;
                nikseTextBoxApiKey.Left = labelApiKey.Right + 3;
                labelApiKey.Visible = true;
                nikseTextBoxApiKey.Visible = true;

                labelFormality.Text = LanguageSettings.Current.AudioToText.Model;
                labelFormality.Visible = true;
                comboBoxFormality.Left = labelFormality.Right + 3;
                comboBoxFormality.Visible = true;
                comboBoxFormality.DropDownStyle = ComboBoxStyle.DropDown;
                comboBoxFormality.Items.Clear();
                comboBoxFormality.Items.AddRange(OpenRouterTranslate.Models);
                comboBoxFormality.Text = Configuration.Settings.Tools.OpenRouterModel;

                return;
            }


            if (engineType == typeof(GeminiTranslate))
            {
                nikseComboBoxUrl.Visible = false;
                labelUrl.Visible = false;

                labelApiKey.Left = labelUrl.Left;
                nikseTextBoxApiKey.Text = Configuration.Settings.Tools.GeminiProApiKey;
                nikseTextBoxApiKey.Left = labelApiKey.Right + 3;
                labelApiKey.Visible = true;
                nikseTextBoxApiKey.Visible = true;
                return;
            }

            throw new Exception($"Engine {_autoTranslator.Name} not handled!");
        }

        private void SelectFormality()
        {
            comboBoxFormality.SelectedIndex = 0;
            for (var i = 0; i < comboBoxFormality.Items.Count; i++)
            {
                if (comboBoxFormality.Items[i].ToString() == Configuration.Settings.Tools.AutoTranslateDeepLFormality)
                {
                    comboBoxFormality.SelectedIndex = i;
                    break;
                }
            }
        }

        private void FillUrls(List<string> list)
        {
            nikseComboBoxUrl.Items.Clear();
            foreach (var url in list.Distinct())
            {
                if (!string.IsNullOrEmpty(url))
                {
                    nikseComboBoxUrl.Items.Add(url.TrimEnd('/') + "/");
                }
            }

            labelUrl.Text = LanguageSettings.Current.Main.Url;
            nikseComboBoxUrl.Left = labelUrl.Right + 3;
            if (nikseComboBoxUrl.Items.Count > 0)
            {
                nikseComboBoxUrl.SelectedIndex = 0;
            }

            nikseComboBoxUrl.Visible = true;
            labelUrl.Visible = true;
        }

        private void SetAutoTranslatorUrl(string url)
        {
            var engine = GetCurrentEngine();
            var engineType = engine.GetType();

            if (engineType == typeof(NoLanguageLeftBehindApi))
            {
                Configuration.Settings.Tools.AutoTranslateNllbApiUrl = url;
                return;
            }

            if (engineType == typeof(NoLanguageLeftBehindServe))
            {
                Configuration.Settings.Tools.AutoTranslateNllbServeUrl = url;
                return;
            }

            if (engineType == typeof(LibreTranslate))
            {
                Configuration.Settings.Tools.AutoTranslateLibreUrl = url;

                if (url.Contains("https://libretranslate.com", StringComparison.OrdinalIgnoreCase))
                {
                    labelApiKey.Left = nikseComboBoxUrl.Right + 9;
                    nikseTextBoxApiKey.Left = labelApiKey.Right + 3;
                    nikseTextBoxApiKey.Text = Configuration.Settings.Tools.AutoTranslateLibreApiKey;
                    labelApiKey.Visible = true;
                    nikseTextBoxApiKey.Visible = true;
                }
                else
                {
                    labelApiKey.Visible = false;
                    nikseTextBoxApiKey.Visible = false;
                }
            }
        }

        private void SetupLanguageSettings()
        {
            var sourceLanguages = _autoTranslator.GetSupportedSourceLanguages();
            FillComboWithLanguages(comboBoxSource, sourceLanguages);
            var sourceLanguageIsoCode = EvaluateDefaultSourceLanguageCode(_encoding, _subtitle, sourceLanguages);
            SelectLanguageCode(comboBoxSource, sourceLanguageIsoCode);

            FillComboWithLanguages(comboBoxTarget, _autoTranslator.GetSupportedTargetLanguages());
            var targetLanguageIsoCode = EvaluateDefaultTargetLanguageCode(sourceLanguageIsoCode);
            SelectLanguageCode(comboBoxTarget, targetLanguageIsoCode);
        }

        public static void SelectLanguageCode(NikseComboBox comboBox, string languageIsoCode)
        {
            var i = 0;
            var threeLetterLanguageCode = Iso639Dash2LanguageCode.GetThreeLetterCodeFromTwoLetterCode(languageIsoCode);

            foreach (var comboBoxItem in comboBox.Items)
            {
                var item = comboBoxItem as TranslationPair;
                if (item == null)
                {
                    continue;
                }

                if (!string.IsNullOrEmpty(item.TwoLetterIsoLanguageName) && item.TwoLetterIsoLanguageName == languageIsoCode)
                {
                    comboBox.SelectedIndex = i;
                    return;
                }

                if (item.Code.Contains('-'))
                {
                    var arr = item.Code.ToLowerInvariant().Split('-');
                    if (arr[0].Length == 2 && arr[0] == languageIsoCode)
                    {
                        comboBox.SelectedIndex = i;
                        return;
                    }

                    if (arr[0].Length == 3 && arr[0] == languageIsoCode)
                    {
                        comboBox.SelectedIndex = i;
                        return;
                    }

                    if (arr[1].Length == 2 && arr[1] == languageIsoCode)
                    {
                        comboBox.SelectedIndex = i;
                        return;
                    }

                    if (arr[1].Length == 3 && arr[1] == languageIsoCode)
                    {
                        comboBox.SelectedIndex = i;
                        return;
                    }
                }

                if (languageIsoCode.Length == 2 && item.Code == languageIsoCode)
                {
                    comboBox.SelectedIndex = i;
                    return;
                }

                if (!string.IsNullOrEmpty(threeLetterLanguageCode) && item.Code.StartsWith(threeLetterLanguageCode) || item.Code == languageIsoCode)
                {
                    comboBox.SelectedIndex = i;
                    return;
                }

                i++;
            }

            if (comboBox.SelectedIndex < 0 && comboBox.Items.Count > 0)
            {
                comboBox.SelectedIndex = 0;
            }
        }

        public static void FillComboWithLanguages(NikseComboBox comboBox, List<TranslationPair> languages)
        {
            comboBox.Items.Clear();
            var languagesFilled = false;

            if (!string.IsNullOrEmpty(Configuration.Settings.General.DefaultLanguages))
            {
                var favorites = Utilities.GetSubtitleLanguageCultures(true).ToList();
                var languagesToAdd = new List<TranslationPair>();

                foreach (var language in languages)
                {
                    if (favorites.Any(p0 => p0.TwoLetterISOLanguageName == language.Code) ||
                        favorites.Any(p1 => p1.ThreeLetterISOLanguageName == language.Code) ||
                        (!string.IsNullOrWhiteSpace(language.Code) && favorites.Any(p2 => p2.TwoLetterISOLanguageName.StartsWith(language.Code, StringComparison.OrdinalIgnoreCase))) ||
                        favorites.Any(p3 => p3.EnglishName.Contains(language.Name, StringComparison.OrdinalIgnoreCase)) ||
                        favorites.Any(p4 => language.Name.Contains(p4.EnglishName, StringComparison.OrdinalIgnoreCase)))
                    {
                        languagesFilled = true;
                        languagesToAdd.Add(language);
                    }
                }

                comboBox.Items.AddRange(languagesToAdd.OrderBy(p => p.Name).ToArray<object>());
            }

            if (!languagesFilled)
            {
                comboBox.Items.AddRange(languages.OrderBy(p => p.Name).ToArray<object>());
            }

            comboBox.Items.Add(LanguageSettings.Current.General.ChangeLanguageFilter);
        }

        public static string EvaluateDefaultSourceLanguageCode(Encoding encoding, Subtitle subtitle, List<TranslationPair> sourceLanguages)
        {
            var defaultSourceLanguageCode = LanguageAutoDetect.AutoDetectGoogleLanguage(encoding); // Guess language via encoding
            if (string.IsNullOrEmpty(defaultSourceLanguageCode))
            {
                defaultSourceLanguageCode = LanguageAutoDetect.AutoDetectGoogleLanguage(subtitle); // Guess language based on subtitle contents
            }

            if (!string.IsNullOrEmpty(Configuration.Settings.Tools.GoogleTranslateLastSourceLanguage) &&
                Configuration.Settings.Tools.GoogleTranslateLastTargetLanguage.StartsWith(defaultSourceLanguageCode) &&
                sourceLanguages.Any(p => p.Code == Configuration.Settings.Tools.GoogleTranslateLastSourceLanguage))
            {
                return Configuration.Settings.Tools.GoogleTranslateLastSourceLanguage;
            }

            return defaultSourceLanguageCode;
        }

        public static string EvaluateDefaultTargetLanguageCode(string defaultSourceLanguage)
        {
            var installedLanguages = new List<string>();
            foreach (InputLanguage language in InputLanguage.InstalledInputLanguages)
            {
                var layoutName = language.LayoutName;
                // related to https://github.com/SubtitleEdit/subtitleedit/issues/8084
                if (string.IsNullOrEmpty(layoutName))
                {
                    continue;
                }
                var iso639 = Iso639Dash2LanguageCode.GetTwoLetterCodeFromEnglishName(layoutName);
                if (!string.IsNullOrEmpty(iso639) && !installedLanguages.Contains(iso639))
                {
                    installedLanguages.Add(iso639.ToLowerInvariant());
                }
            }

            var uiCultureTargetLanguage = Configuration.Settings.Tools.GoogleTranslateLastTargetLanguage;
            if (uiCultureTargetLanguage == defaultSourceLanguage)
            {
                foreach (var s in Utilities.GetDictionaryLanguages())
                {
                    var temp = s.Replace("[", string.Empty).Replace("]", string.Empty);
                    if (temp.Length > 4)
                    {
                        temp = temp.Substring(temp.Length - 5, 2).ToLowerInvariant();
                        if (temp != defaultSourceLanguage && installedLanguages.Any(p => p.Contains(temp)))
                        {
                            uiCultureTargetLanguage = temp;
                            break;
                        }
                    }
                }
            }

            if (uiCultureTargetLanguage == defaultSourceLanguage)
            {
                foreach (var language in installedLanguages)
                {
                    if (language != defaultSourceLanguage)
                    {
                        uiCultureTargetLanguage = language;
                        break;
                    }
                }
            }

            if (uiCultureTargetLanguage == defaultSourceLanguage)
            {
                var name = CultureInfo.CurrentCulture.Name;
                if (name.Length > 2)
                {
                    name = name.Remove(0, name.Length - 2);
                }
                var iso = IsoCountryCodes.ThreeToTwoLetterLookup.FirstOrDefault(p => p.Value == name);
                if (!iso.Equals(default(KeyValuePair<string, string>)))
                {
                    var iso639 = Iso639Dash2LanguageCode.GetTwoLetterCodeFromThreeLetterCode(iso.Key);
                    if (!string.IsNullOrEmpty(iso639))
                    {
                        uiCultureTargetLanguage = iso639;
                    }
                }
            }

            // Set target language to something different than source language
            if (uiCultureTargetLanguage == defaultSourceLanguage && defaultSourceLanguage == "en")
            {
                uiCultureTargetLanguage = "es";
            }
            else if (uiCultureTargetLanguage == defaultSourceLanguage)
            {
                uiCultureTargetLanguage = "en";
            }

            return uiCultureTargetLanguage;
        }

        private void AutoTranslate_Resize(object sender, EventArgs e)
        {
            var width = (Width / 2) - (subtitleListViewSource.Left * 3) + 19;
            subtitleListViewSource.Width = width;
            subtitleListViewTarget.Width = width;

            var height = Height - (subtitleListViewSource.Top + buttonTranslate.Height + 95);
            subtitleListViewSource.Height = height;
            subtitleListViewTarget.Height = height;

            comboBoxSource.Left = subtitleListViewSource.Left + (subtitleListViewSource.Width - comboBoxSource.Width);
            labelSource.Left = comboBoxSource.Left - 5 - labelSource.Width;

            subtitleListViewTarget.Left = width + (subtitleListViewSource.Left * 2);
            subtitleListViewTarget.Width = Width - subtitleListViewTarget.Left - 32;
            labelTarget.Left = subtitleListViewTarget.Left;
            comboBoxTarget.Left = labelTarget.Left + labelTarget.Width + 5;
            buttonTranslate.Left = comboBoxTarget.Left + comboBoxTarget.Width + 9;
            labelPleaseWait.Left = buttonTranslate.Left + buttonTranslate.Width + 9;
            progressBar1.Left = labelPleaseWait.Left;
            progressBar1.Width = subtitleListViewTarget.Width - (progressBar1.Left - subtitleListViewTarget.Left);
        }

        private async void buttonTranslate_Click(object sender, EventArgs e)
        {
            if (_translationInProgress)
            {
                _translationInProgress = false;
                _cancellationTokenSource.Cancel();
                _breakTranslation = true;
                buttonTranslate.Enabled = false;
                buttonOK.Enabled = true;
                buttonCancel.Enabled = true;
                Application.DoEvents();
                buttonOK.Refresh();
                _singleLineMode = false;
                return;
            }

            _translationInProgress = true;
            _cancellationTokenSource = new CancellationTokenSource();
            _autoTranslator = GetCurrentEngine();
            var engineType = _autoTranslator.GetType();

            if (_autoTranslator.Name == DeepLTranslate.StaticName && string.IsNullOrWhiteSpace(nikseTextBoxApiKey.Text))
            {
                MessageBox.Show(this, string.Format(LanguageSettings.Current.GoogleTranslate.XRequiresAnApiKey, _autoTranslator.Name), Text, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                _translationInProgress = false;
                _singleLineMode = false;
                return;
            }

            if (_autoTranslator.Name == DeepLTranslate.StaticName && string.IsNullOrWhiteSpace(nikseComboBoxUrl.Text))
            {
                MessageBox.Show(this, string.Format("{0} requires an url", _autoTranslator.Name), Text, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                _translationInProgress = false;
                _singleLineMode = false;
                return;
            }

            SaveSettings(engineType);

            buttonOK.Enabled = false;
            buttonCancel.Enabled = false;
            _breakTranslation = false;
            buttonTranslate.Text = LanguageSettings.Current.General.Cancel;
            progressBar1.Minimum = 0;
            progressBar1.Value = 0;
            progressBar1.Maximum = TranslatedSubtitle.Paragraphs.Count;
            progressBar1.Visible = true;
            labelPleaseWait.Visible = true;

            _autoTranslator.Initialize();

            var timerUpdate = new Timer();
            timerUpdate.Interval = 1500;
            timerUpdate.Tick += TimerUpdate_Tick;
            timerUpdate.Start();
            var linesTranslated = 0;

            // Max merge attempts(always=-1,never=0), fallback to forceSingleLineMode after exceeding the number of attempts
            int maxMergeErrorCount = Configuration.Settings.Tools.AutoTranslateMaxMerges;

            var forceSingleLineMode = Configuration.Settings.Tools.AutoTranslateStrategy == TranslateStrategy.TranslateEachLineSeparately.ToString() ||
                                      _autoTranslator.Name == NoLanguageLeftBehindApi.StaticName ||  // NLLB seems to miss some text...
                                      _autoTranslator.Name == NoLanguageLeftBehindServe.StaticName ||
                                      _singleLineMode ||
                                      maxMergeErrorCount == 0;

            var delaySeconds = Configuration.Settings.Tools.AutoTranslateDelaySeconds;

            if (comboBoxSource.SelectedItem is TranslationPair source && comboBoxTarget.SelectedItem is TranslationPair target)
            {
                SetFormality(target);
                TwoLetterIsoSource = source.TwoLetterIsoLanguageName;
                TwoLetterIsoTarget = target.TwoLetterIsoLanguageName;
                var mergeErrorCount = 0;

                Configuration.Settings.Tools.GoogleTranslateLastSourceLanguage = source.TwoLetterIsoLanguageName ?? source.Code;
                Configuration.Settings.Tools.GoogleTranslateLastTargetLanguage = target.TwoLetterIsoLanguageName ?? target.Code;
                try
                {
                    var start = subtitleListViewTarget.SelectedIndex >= 0 ? subtitleListViewTarget.SelectedIndex : 0;
                    var index = start;
                    while (index < _subtitle.Paragraphs.Count)
                    {
                        Delay(delaySeconds, start, index);

                        if (_breakTranslation)
                        {
                            break;
                        }

                        var linesMergedAndTranslated = await MergeAndSplitHelper.MergeAndTranslateIfPossible(_subtitle, TranslatedSubtitle, source, target, index, _autoTranslator, forceSingleLineMode, _cancellationTokenSource.Token);
                        Application.DoEvents();

                        if (_breakTranslation)
                        {
                            break;
                        }

                        if (linesMergedAndTranslated > 0)
                        {
                            index += linesMergedAndTranslated;
                            linesTranslated += linesMergedAndTranslated;
                            _translationProgressIndex = index - 1;
                            continue;
                        }

                        if (!forceSingleLineMode)
                        {
                            if (maxMergeErrorCount < 0)
                            {
                                continue;                        
                            } 

                            if (mergeErrorCount >= maxMergeErrorCount - 1)
                            {
                                forceSingleLineMode = true;                        
                            }

                            mergeErrorCount++;
                        }

                        var p = _subtitle.Paragraphs[index];
                        var f = new Formatting();
                        var unformattedText = f.SetTagsAndReturnTrimmed(p.Text, source.Code);

                        var translation = await _autoTranslator.Translate(unformattedText, source.Code, target.Code, _cancellationTokenSource.Token);

                        if (_breakTranslation)
                        {
                            break;
                        }

                        translation = translation
                            .Replace("<br />", Environment.NewLine)
                            .Replace("<br/>", Environment.NewLine);

                        var reFormattedText = f.ReAddFormatting(translation);
                        if (reFormattedText.StartsWith("- ", StringComparison.Ordinal) && !p.Text.Contains('-'))
                        {
                            reFormattedText = reFormattedText.TrimStart('-').Trim();
                        }

                        TranslatedSubtitle.Paragraphs[index].Text = Utilities.AutoBreakLine(reFormattedText);

                        _translationProgressIndex = index;
                        _translationProgressDirty = true;
                        progressBar1.Value = index;

                        // Proceed to the next line only after successfully obtaining the translation to avoid intermittent translation results
                        if (!string.IsNullOrWhiteSpace(translation))
                        {   
                            linesTranslated++;   
                            index++;
                        }

                        Application.DoEvents();
                        if (_breakTranslation || _singleLineMode)
                        {
                            break;
                        }
                    }
                }
                catch (TaskCanceledException exception)
                {
                    SeLogger.Error(exception);
                    // ignore
                }
                catch (Exception exception)
                {
                    HandleError(exception, linesTranslated, engineType);
                }
                finally
                {
                    _singleLineMode = false;
                }
            }

            timerUpdate.Stop();

            progressBar1.Visible = false;
            labelPleaseWait.Visible = false;
            buttonOK.Enabled = true;
            buttonCancel.Enabled = true;
            _breakTranslation = false;
            buttonTranslate.Enabled = true;
            buttonTranslate.Text = LanguageSettings.Current.GoogleTranslate.Translate;
            _translationInProgress = false;

            timerUpdate.Dispose();
            _translationProgressDirty = true;
            UpdateTranslation();
            buttonOK.Focus();
        }

        private void Delay(int delaySeconds, int start, int index)
        {
            if (index > start && delaySeconds > 0)
            {
                for (var i = delaySeconds; i > 0; i--)
                {
                    labelPleaseWait.Text = LanguageSettings.Current.GoogleTranslate.PleaseWait + $" ({i})";
                    labelPleaseWait.Refresh();
                    Application.DoEvents();
                    Thread.Sleep(1000);
                    if (_breakTranslation)
                    {
                        break;
                    }
                }

                labelPleaseWait.Text = LanguageSettings.Current.GoogleTranslate.PleaseWait;
                Application.DoEvents();
            }
        }

        private void HandleError(Exception exception, int linesTranslate, Type engineType)
        {
            SeLogger.Error(exception);

            if (nikseTextBoxApiKey.Visible &&
                string.IsNullOrWhiteSpace(nikseTextBoxApiKey.Text) &&
                engineType != typeof(MyMemoryApi))
            {
                MessageBox.Show(this, string.Format(LanguageSettings.Current.GoogleTranslate.XRequiresAnApiKey, _autoTranslator.Name), Text, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                nikseTextBoxApiKey.Focus();
            }

            var count = 0;
            while (count < 10 && exception.InnerException != null)
            {
                exception = exception.InnerException;
                count++;
            }

            if (linesTranslate == 0 && engineType == typeof(LibreTranslate) && nikseComboBoxUrl.Text.Contains("https://libretranslate.com", StringComparison.OrdinalIgnoreCase))
            {
                var dr = MessageBox.Show(
                    this, string.Format(LanguageSettings.Current.GoogleTranslate.XRequiresAnApiKey, nikseComboBoxUrl.Text) + Environment.NewLine +
                        Environment.NewLine +
                        LanguageSettings.Current.GoogleTranslate.ReadMore + Environment.NewLine +
                        Environment.NewLine +
                        _autoTranslator.Error,
                        Text,
                        MessageBoxButtons.YesNoCancel,
                        MessageBoxIcon.Error);

                if (dr == DialogResult.Yes)
                {
                    UiUtil.ShowHelp("#translation");
                }
            }
            else if (linesTranslate == 0 && engineType == typeof(DeepLTranslate) && _autoTranslator.Error.Contains("Wrong endpoint. Use https://api.deepl.com"))
            {
                nikseComboBoxUrl.Text = "https://api.deepl.com/";

                MessageBox.Show(
                    this, "You might be using a wrong url - switching to https://api.deepl.com/ - please retry." + Environment.NewLine +
                        Environment.NewLine +
                        _autoTranslator.Error,
                        Text,
                        MessageBoxButtons.OKCancel,
                        MessageBoxIcon.Error);
            }
            else if (linesTranslate == 0 && engineType == typeof(DeepLTranslate) && _autoTranslator.Error.Contains("Wrong endpoint. Use https://api-free.deepl.com"))
            {
                nikseComboBoxUrl.Text = "https://api-free.deepl.com/";

                MessageBox.Show(
                    this, "You might be using a wrong url - switching to https://api-free.deepl.com/ - please retry." + Environment.NewLine +
                        Environment.NewLine +
                        _autoTranslator.Error,
                        Text,
                        MessageBoxButtons.OKCancel,
                        MessageBoxIcon.Error);
            }
            else if (linesTranslate == 0 && engineType == typeof(PapagoTranslate))
            {
                var dr = MessageBox.Show(
                    this, string.Format(LanguageSettings.Current.GoogleTranslate.XRequiresAnApiKey, PapagoTranslate.StaticName) + Environment.NewLine +
                        Environment.NewLine +
                        LanguageSettings.Current.GoogleTranslate.ReadMore + Environment.NewLine +
                        Environment.NewLine +
                        "Client ID and Client secret are required" + Environment.NewLine +
                        Environment.NewLine +
                        _autoTranslator.Error,
                        Text,
                        MessageBoxButtons.YesNoCancel,
                        MessageBoxIcon.Error);

                if (dr == DialogResult.Yes)
                {
                    UiUtil.ShowHelp("#translation");
                }
            }
            else if (linesTranslate == 0 &&
                     (nikseComboBoxUrl.Text.Contains("//192.", StringComparison.OrdinalIgnoreCase) ||
                      nikseComboBoxUrl.Text.Contains("//127.", StringComparison.OrdinalIgnoreCase) ||
                      nikseComboBoxUrl.Text.Contains("//localhost", StringComparison.OrdinalIgnoreCase)))
            {
                if (engineType == typeof(NoLanguageLeftBehindApi) ||
                    engineType == typeof(NoLanguageLeftBehindServe) ||
                    engineType == typeof(LibreTranslate) ||
                    engineType == typeof(LmStudioTranslate) ||
                    engineType == typeof(OllamaTranslate))
                {
                    var err = string.IsNullOrEmpty(_autoTranslator.Error) ? string.Empty : _autoTranslator.Error + Environment.NewLine;
                    var dr = MessageBox.Show(
                        string.Format(LanguageSettings.Current.GoogleTranslate.XRequiresALocalWebServer, _autoTranslator.Name)
                        + Environment.NewLine + err
                        + Environment.NewLine + LanguageSettings.Current.GoogleTranslate.ReadMore + Environment.NewLine,
                        MessageBoxButtons.YesNoCancel, MessageBoxIcon.Error);

                    if (dr == DialogResult.Yes)
                    {
                        UiUtil.ShowHelp("#translation");
                    }
                }
                else
                {
                    MessageBox.Show(exception.Message + Environment.NewLine + exception.StackTrace +
                        Environment.NewLine +
                        _autoTranslator.Error, MessageBoxIcon.Error);
                }
            }
            else
            {
                MessageBox.Show(exception.Message + Environment.NewLine + exception.StackTrace +
                        Environment.NewLine +
                        _autoTranslator.Error, MessageBoxIcon.Error);
            }
        }

        private void SaveSettings(Type engineType)
        {
            if (engineType == typeof(MicrosoftTranslator) && !string.IsNullOrWhiteSpace(nikseTextBoxApiKey.Text))
            {
                Configuration.Settings.Tools.MicrosoftTranslatorApiKey = nikseTextBoxApiKey.Text.Trim();
            }

            if (engineType == typeof(DeepLTranslate) && !string.IsNullOrWhiteSpace(nikseTextBoxApiKey.Text))
            {
                Configuration.Settings.Tools.AutoTranslateDeepLUrl = nikseComboBoxUrl.Text.Trim();
                Configuration.Settings.Tools.AutoTranslateDeepLApiKey = nikseTextBoxApiKey.Text.Trim();
            }

            if (engineType == typeof(LibreTranslate) && nikseTextBoxApiKey.Visible && !string.IsNullOrWhiteSpace(nikseTextBoxApiKey.Text))
            {
                Configuration.Settings.Tools.AutoTranslateLibreApiKey = nikseTextBoxApiKey.Text.Trim();
            }

            if (engineType == typeof(MyMemoryApi) && nikseTextBoxApiKey.Visible && !string.IsNullOrWhiteSpace(nikseTextBoxApiKey.Text))
            {
                Configuration.Settings.Tools.AutoTranslateMyMemoryApiKey = nikseTextBoxApiKey.Text.Trim();
            }

            if (engineType == typeof(ChatGptTranslate))
            {
                Configuration.Settings.Tools.ChatGptApiKey = nikseTextBoxApiKey.Text.Trim();
                Configuration.Settings.Tools.ChatGptUrl = nikseComboBoxUrl.Text.Trim();
                Configuration.Settings.Tools.ChatGptModel = comboBoxFormality.Text.Trim();
            }

            if (engineType == typeof(LmStudioTranslate))
            {
                Configuration.Settings.Tools.LmStudioApiUrl = nikseComboBoxUrl.Text.Trim();
                Configuration.Settings.Tools.LmStudioModel = comboBoxFormality.Text.Trim();
            }

            if (engineType == typeof(OllamaTranslate))
            {
                Configuration.Settings.Tools.OllamaApiUrl = nikseComboBoxUrl.Text.Trim();
                Configuration.Settings.Tools.OllamaModel = comboBoxFormality.Text.Trim();
            }

            if (engineType == typeof(AnthropicTranslate) && !string.IsNullOrWhiteSpace(nikseTextBoxApiKey.Text))
            {
                Configuration.Settings.Tools.AnthropicApiKey = nikseTextBoxApiKey.Text.Trim();
                Configuration.Settings.Tools.AnthropicApiModel = comboBoxFormality.Text.Trim();
            }

            if (engineType == typeof(GroqTranslate) && !string.IsNullOrWhiteSpace(nikseTextBoxApiKey.Text))
            {
                Configuration.Settings.Tools.GroqApiKey = nikseTextBoxApiKey.Text.Trim();
                Configuration.Settings.Tools.GroqModel = comboBoxFormality.Text.Trim();
            }

            if (engineType == typeof(OpenRouterTranslate) && !string.IsNullOrWhiteSpace(nikseTextBoxApiKey.Text))
            {
                Configuration.Settings.Tools.OpenRouterApiKey = nikseTextBoxApiKey.Text.Trim();
                Configuration.Settings.Tools.OpenRouterModel = comboBoxFormality.Text.Trim();
            }

            if (engineType == typeof(GeminiTranslate) && !string.IsNullOrWhiteSpace(nikseTextBoxApiKey.Text))
            {
                Configuration.Settings.Tools.GeminiProApiKey = nikseTextBoxApiKey.Text.Trim();
            }

            if (engineType == typeof(PapagoTranslate) && !string.IsNullOrWhiteSpace(nikseTextBoxApiKey.Text))
            {
                Configuration.Settings.Tools.AutoTranslatePapagoApiKeyId = nikseComboBoxUrl.Text.Trim();
                Configuration.Settings.Tools.AutoTranslatePapagoApiKey = nikseTextBoxApiKey.Text.Trim();
            }
        }

        private static void StartNoLanguageLeftBehindServe()
        {
            var modelName = Configuration.Settings.Tools.AutoTranslateNllbServeModel;
            var arguments = string.IsNullOrEmpty(modelName) ? string.Empty : $"-mi {modelName}";
            var process = new Process
            {
                StartInfo = new ProcessStartInfo("nllb-serve", arguments)
                {
                    UseShellExecute = false,
                }
            };

            process.StartInfo.EnvironmentVariables["PYTHONIOENCODING"] = "utf-8";
            process.StartInfo.EnvironmentVariables["PYTHONLEGACYWINDOWSSTDIO"] = "utf-8";
            process.Start();
        }

        private static void StartLibreTranslate()
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo("libretranslate", string.Empty)
                {
                    UseShellExecute = false,
                }
            };

            process.Start();
        }

        private void TimerUpdate_Tick(object sender, EventArgs e)
        {
            UpdateTranslation();
        }

        private void UpdateTranslation()
        {
            if (!_translationProgressDirty)
            {
                return;
            }

            subtitleListViewTarget.BeginUpdate();
            subtitleListViewTarget.Fill(TranslatedSubtitle);
            _translationProgressDirty = true;
            subtitleListViewTarget.SelectIndexAndEnsureVisible(_translationProgressIndex < 0 ? 0 : _translationProgressIndex);
            subtitleListViewTarget.EndUpdate();
            subtitleListViewSource.SelectIndexAndEnsureVisible(_translationProgressIndex < 0 ? 0 : _translationProgressIndex);

            SyncListViews(subtitleListViewTarget, subtitleListViewSource);

            subtitleListViewSource.Refresh();
            subtitleListViewTarget.Refresh();
            Application.DoEvents();
        }

        private void AutoTranslate_ResizeEnd(object sender, EventArgs e)
        {
            AutoTranslate_Resize(null, null);
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            SaveSettings(GetCurrentEngine().GetType());
            var isEmpty = TranslatedSubtitle == null || TranslatedSubtitle.Paragraphs.All(p => string.IsNullOrEmpty(p.Text));
            DialogResult = isEmpty ? DialogResult.Cancel : DialogResult.OK;
        }

        private IAutoTranslator GetCurrentEngine()
        {
            return _autoTranslatorEngines.First(p => p.Name == nikseComboBoxEngine.Text);
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private async void nikseComboBoxEngine_SelectedIndexChanged(object sender, EventArgs e)
        {
            SetAutoTranslatorEngine();
            SetupLanguageSettings();

            if (GetCurrentEngine().GetType() == typeof(OllamaTranslate))
            {
                await DownloadOllamaModelsAsync();
            }
        }

        private void nikseComboBoxUrl_SelectedIndexChanged(object sender, EventArgs e)
        {
            SetAutoTranslatorUrl(nikseComboBoxUrl.Text);
        }

        private void linkLabelPoweredBy_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            var engine = _autoTranslatorEngines.First(p => p.Name == nikseComboBoxEngine.Text);
            UiUtil.OpenUrl(engine.Url);
        }

        private void AutoTranslate_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                DialogResult = DialogResult.Cancel;
            }
            else if (e.KeyData == UiUtil.HelpKeys)
            {
                UiUtil.ShowHelp("#translation");
                e.SuppressKeyPress = true;
            }
        }

        private void AutoTranslate_FormClosing(object sender, FormClosingEventArgs e)
        {
            var engine = GetCurrentEngine();
            Configuration.Settings.Tools.AutoTranslateLastName = engine.Name;
            Configuration.Settings.Tools.AutoTranslateLastUrl = nikseComboBoxUrl.Text;
        }

        private void StartLibreTranslateServerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            StartLibreTranslate();
        }

        private void StartNllbServeServerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            StartNoLanguageLeftBehindServe();
        }

        private void contextMenuStrip1_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (_translationInProgress)
            {
                e.Cancel = true;
                return;
            }

            startLibreTranslateServerToolStripMenuItem.Visible = false;
            startNLLBServeServerToolStripMenuItem.Visible = false;
            startNLLBAPIServerToolStripMenuItem.Visible = false;
            toolStripSeparator2.Visible = false;

            var engineType = _autoTranslator.GetType();
            if (engineType == typeof(NoLanguageLeftBehindServe))
            {
                startNLLBServeServerToolStripMenuItem.Visible = true;
                toolStripSeparator2.Visible = true;
            }
            else if (engineType == typeof(LibreTranslate))
            {
                startLibreTranslateServerToolStripMenuItem.Visible = true;
                toolStripSeparator2.Visible = true;
            }

            var idx = subtitleListViewTarget.SelectedIndex;
            if (idx >= 0 && !string.IsNullOrWhiteSpace(TranslatedSubtitle.Paragraphs[idx].Text))
            {
                translateCurrentLineToolStripMenuItem1.Text = LanguageSettings.Current.GoogleTranslate.ReTranslateCurrentLine;
            }
            else
            {
                translateCurrentLineToolStripMenuItem1.Text = LanguageSettings.Current.GoogleTranslate.TranslateCurrentLine;
            }
            translateCurrentLineToolStripMenuItem.Text = translateCurrentLineToolStripMenuItem1.Text;
        }

        private void contextMenuStrip2_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (_translationInProgress)
            {
                e.Cancel = true;
                return;
            }

            toolStripMenuItemStartLibre.Visible = false;
            toolStripMenuItemStartNLLBServe.Visible = false;
            toolStripMenuItemStartNLLBApi.Visible = false;
            toolStripSeparator1.Visible = false;

            var engineType = _autoTranslator.GetType();
            if (engineType == typeof(NoLanguageLeftBehindServe))
            {
                toolStripMenuItemStartNLLBServe.Visible = true;
                toolStripSeparator1.Visible = true;
            }
            else if (engineType == typeof(LibreTranslate))
            {
                toolStripMenuItemStartLibre.Visible = true;
                toolStripSeparator1.Visible = true;
            }

            var idx = subtitleListViewTarget.SelectedIndex;
            if (idx >= 0 && !string.IsNullOrWhiteSpace(TranslatedSubtitle.Paragraphs[idx].Text))
            {
                translateCurrentLineToolStripMenuItem1.Text = "Re-translate current line";
            }
            else
            {
                translateCurrentLineToolStripMenuItem1.Text = "translate only current line";
            }
            translateCurrentLineToolStripMenuItem.Text = translateCurrentLineToolStripMenuItem1.Text;
        }

        private void subtitleListViewTarget_Click(object sender, EventArgs e)
        {
            SyncListViews(subtitleListViewTarget, subtitleListViewSource);
        }

        private void subtitleListViewTarget_DoubleClick(object sender, EventArgs e)
        {
            SyncListViews(subtitleListViewTarget, subtitleListViewSource);
        }

        private static void SyncListViews(ListView listViewSelected, SubtitleListView listViewOther)
        {
            if (listViewSelected.SelectedItems.Count > 0)
            {
                var first = listViewSelected.TopItem.Index;
                var index = listViewSelected.SelectedItems[0].Index;
                if (index < listViewOther.Items.Count)
                {
                    listViewOther.SelectIndexAndEnsureVisible(index, false);
                    if (first >= 0)
                    {
                        listViewOther.TopItem = listViewOther.Items[first];
                    }
                }
            }
        }

        private void subtitleListViewSource_DoubleClick(object sender, EventArgs e)
        {
            SyncListViews(subtitleListViewSource, subtitleListViewTarget);
        }

        private void subtitleListViewSource_Click(object sender, EventArgs e)
        {
            SyncListViews(subtitleListViewSource, subtitleListViewTarget);
        }

        private void SetFormality(TranslationPair target)
        {
            if (comboBoxFormality.Visible && target?.HasFormality == true && comboBoxFormality.SelectedIndex > 0)
            {
                if (_autoTranslator.Name == DeepLTranslate.StaticName)
                {
                    var f = string.Empty;
                    if (comboBoxFormality.SelectedIndex == 0)
                    {
                        f = string.Empty;
                    }
                    else if (comboBoxFormality.SelectedIndex == 1)
                    {
                        f = "more";
                    }
                    else if (comboBoxFormality.SelectedIndex == 2)
                    {
                        f = "less";
                    }
                    else if (comboBoxFormality.SelectedIndex == 3)
                    {
                        f = "prefer_more";
                    }
                    else if (comboBoxFormality.SelectedIndex == 4)
                    {
                        f = "prefer_less";
                    }

                    Configuration.Settings.Tools.AutoTranslateDeepLFormality = f;
                }
            }
        }

        private void comboBoxTarget_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBoxTarget.SelectedIndex > 0 && comboBoxTarget.Text == LanguageSettings.Current.General.ChangeLanguageFilter)
            {
                using (var form = new DefaultLanguagesChooser(Configuration.Settings.General.DefaultLanguages))
                {
                    if (form.ShowDialog(this) == DialogResult.OK)
                    {
                        Configuration.Settings.General.DefaultLanguages = form.DefaultLanguages;
                    }
                }

                SetupLanguageSettings();
                return;
            }

            if (_autoTranslator.Name == DeepLTranslate.StaticName && comboBoxTarget.SelectedItem is TranslationPair target)
            {
                if (target.HasFormality == null || target.HasFormality == false)
                {
                    labelFormality.Enabled = false;
                    comboBoxFormality.Enabled = false;
                    return;
                }

                labelFormality.Enabled = true;
                comboBoxFormality.Enabled = true;

                if (target.TwoLetterIsoLanguageName == "ja" && comboBoxFormality.Items.Count != 3)
                {
                    comboBoxFormality.Items.Clear();
                    comboBoxFormality.Items.Add("default");
                    comboBoxFormality.Items.Add("more");
                    comboBoxFormality.Items.Add("less");

                    SelectFormality();
                }
                else if (comboBoxFormality.Items.Count != 3)
                {
                    comboBoxFormality.Items.Clear();
                    comboBoxFormality.Items.Add("default");
                    comboBoxFormality.Items.Add("more");
                    comboBoxFormality.Items.Add("less");
                    comboBoxFormality.Items.Add("prefer_more");
                    comboBoxFormality.Items.Add("prefer_less");

                    SelectFormality();
                }
            }
        }

        private void buttonStrategy_Click(object sender, EventArgs e)
        {
            using (var form = new AutoTranslateSettings(_autoTranslator.GetType(), _autoTranslator.Name))
            {
                form.ShowDialog(this);
            }
        }

        private void translateCurrentLineToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            _singleLineMode = true;
            buttonTranslate_Click(null, null);
        }

        private void translateCurrentLineToolStripMenuItem_Click(object sender, EventArgs e)
        {
            translateCurrentLineToolStripMenuItem1_Click(null, null);
        }

        private void comboBoxSource_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBoxSource.SelectedIndex <= 0 || comboBoxSource.Text != LanguageSettings.Current.General.ChangeLanguageFilter)
            {
                return;
            }

            using (var form = new DefaultLanguagesChooser(Configuration.Settings.General.DefaultLanguages))
            {
                if (form.ShowDialog(this) == DialogResult.OK)
                {
                    Configuration.Settings.General.DefaultLanguages = form.DefaultLanguages;
                }

                SetupLanguageSettings();
            }
        }

        private async void UpdateLocalModelsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            await DownloadOllamaModelsAsync(shouldNotifyOnError: true);
        }

        private async Task DownloadOllamaModelsAsync(bool shouldNotifyOnError = false)
        {
            try
            {
                var models = await GetModelsAsync(nikseComboBoxUrl.Text.Replace("generate", "tags")).ConfigureAwait(true);
                if (models.Count > 0)
                {
                    FillOllamaModels(models.ToArray());
                }
            }
            catch (Exception exception)
            {
                if (shouldNotifyOnError)
                {
                    MessageBox.Show("Unable to get ollama models - is ollama running?" + Environment.NewLine + exception.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    SeLogger.Error(exception, "Unable to get ollama models");
                }
            }

            async Task<List<string>> GetModelsAsync(string url)
            {
                var result = await GetOllamaClient().GetAsync(new Uri(url)).ConfigureAwait(false);
                var bytes = await result.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
                if (!result.IsSuccessStatusCode)
                {
                    return new List<string>();
                }

                var parser = new SeJsonParser();
                var resultJson = Encoding.UTF8.GetString(bytes);
                var names = parser.GetAllTagsByNameAsStrings(resultJson, "name");
                var models = Configuration.Settings.Tools.OllamaModels.Split(',').ToList();
                foreach (var name in names.OrderByDescending(name => name))
                {
                    if (!models.Contains(name))
                    {
                        models.Insert(0, name);
                    }
                }

                Configuration.Settings.Tools.OllamaModels = string.Join(",", models);
                return models;
            }
        }

        private HttpClient _httpClient;

        private HttpClient GetOllamaClient() => _httpClient ?? (_httpClient = new HttpClient());

        private void FillOllamaModels(string[] models)
        {
            if (!(GetCurrentEngine() is OllamaTranslate))
            {
                return;
            }

            comboBoxFormality.BeginUpdate();
            var v = comboBoxFormality.Text;
            comboBoxFormality.Items.Clear();
            comboBoxFormality.Items.AddRange(models);
            comboBoxFormality.Text = v;
            comboBoxFormality.EndUpdate();
        }

        private void findModelsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            UiUtil.OpenUrl("https://ollama.com/library");
        }
    }
}
