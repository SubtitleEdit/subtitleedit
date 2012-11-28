using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Windows.Forms;
using Nikse.SubtitleEdit.Logic;

namespace Nikse.SubtitleEdit.Forms
{

    public sealed partial class GoogleTranslate : Form
    {
        Subtitle _subtitle;
        Subtitle _translatedSubtitle;
        bool _breakTranslation;
        bool _googleTranslate = true;
        MicrosoftTranslationService.SoapService _microsoftTranslationService = null;
        private bool _googleApiNotWorking = false;
        private const string _splitterString  = " == ";
        private const string _newlineString = " __ ";

        private Encoding _screenScrapingEncoding = null;
        public Encoding ScreenScrapingEncoding
        {
            get { return _screenScrapingEncoding; }
        }

        public class ComboBoxItem
        {
            public string Text { get; set; }
            public string Value { get; set; }

            public ComboBoxItem(string text, string value)
            {
                if (text.Length > 1)
                    text = text.Substring(0, 1).ToUpper() + text.Substring(1).ToLower();
                Text = text;

                Value = value;
            }

            public override string ToString()
            {
                return Text;
            }
        }

        public Subtitle TranslatedSubtitle
        {
            get
            {
                return _translatedSubtitle;
            }
        }

        public GoogleTranslate()
        {
            InitializeComponent();

            Text = Configuration.Settings.Language.GoogleTranslate.Title;
            labelFrom.Text = Configuration.Settings.Language.GoogleTranslate.From;
            labelTo.Text = Configuration.Settings.Language.GoogleTranslate.To;
            buttonTranslate.Text = Configuration.Settings.Language.GoogleTranslate.Translate;
            labelPleaseWait.Text = Configuration.Settings.Language.GoogleTranslate.PleaseWait;
            linkLabelPoweredByGoogleTranslate.Text = Configuration.Settings.Language.GoogleTranslate.PoweredByGoogleTranslate;
            buttonOK.Text = Configuration.Settings.Language.General.OK;
            buttonCancel.Text = Configuration.Settings.Language.General.Cancel;

            subtitleListViewFrom.InitializeLanguage(Configuration.Settings.Language.General, Configuration.Settings);
            subtitleListViewTo.InitializeLanguage(Configuration.Settings.Language.General, Configuration.Settings);
            Utilities.InitializeSubtitleFont(subtitleListViewFrom);
            Utilities.InitializeSubtitleFont(subtitleListViewTo);
            subtitleListViewFrom.AutoSizeAllColumns(this);
            subtitleListViewTo.AutoSizeAllColumns(this);
            FixLargeFonts();
        }

        private void FixLargeFonts()
        {
            Graphics graphics = this.CreateGraphics();
            SizeF textSize = graphics.MeasureString(buttonOK.Text, this.Font);
            if (textSize.Height > buttonOK.Height - 4)
            {
                subtitleListViewFrom.InitializeTimeStampColumWidths(this);
                subtitleListViewTo.InitializeTimeStampColumWidths(this);
                int newButtonHeight = (int)(textSize.Height + 7 + 0.5);
                Utilities.SetButtonHeight(this, newButtonHeight, 1);
            }
        }

        internal void Initialize(Subtitle subtitle, string title, bool googleTranslate)
        {
            if (title != null)
                Text = title;

            _googleTranslate = googleTranslate;
            if (!_googleTranslate)
            {
                linkLabelPoweredByGoogleTranslate.Text = Configuration.Settings.Language.GoogleTranslate.PoweredByMicrosoftTranslate;
            }

            labelPleaseWait.Visible = false;
            progressBar1.Visible = false;
            _subtitle = subtitle;
            _translatedSubtitle = new Subtitle(subtitle);

            string defaultFromLanguage = Utilities.AutoDetectGoogleLanguage(subtitle);
            FillComboWithLanguages(comboBoxFrom);
            int i=0;
            foreach (ComboBoxItem item in comboBoxFrom.Items)
            {
                if (item.Value == defaultFromLanguage)
                {
                    comboBoxFrom.SelectedIndex = i;
                    break;
                }
                i++;
            }

            FillComboWithLanguages(comboBoxTo);
            i = 0;
            string uiCultureTL = Configuration.Settings.Tools.GoogleTranslateLastTargetLanguage;
            if (uiCultureTL == defaultFromLanguage)
            {
                foreach (string s in Utilities.GetDictionaryLanguages())
                {
                    string temp = s.Replace("[", string.Empty).Replace("]", string.Empty);
                    if (temp.Length > 4)
                    {
                        temp = temp.Substring(temp.Length - 5,2).ToLower();

                        if (temp != uiCultureTL)
                        {
                            uiCultureTL = temp;
                            break;
                        }
                    }
                }
            }
            comboBoxTo.SelectedIndex = 0;
            foreach (ComboBoxItem item in comboBoxTo.Items)
            {
                if (item.Value == uiCultureTL)
                {
                    comboBoxTo.SelectedIndex = i;
                    break;
                }
                i++;
            }

            subtitleListViewFrom.Fill(subtitle);
            GoogleTranslate_Resize(null, null);

            _googleApiNotWorking = !Configuration.Settings.Tools.UseGooleApiPaidService; // google has closed their free api service :(
        }

        private void buttonTranslate_Click(object sender, EventArgs e)
        {
            if (buttonTranslate.Text == Configuration.Settings.Language.General.Cancel)
            {
                buttonTranslate.Enabled = false;
                _breakTranslation = true;
                buttonOK.Enabled = true;
                buttonCancel.Enabled = true;
                return;
            }

            // empty all texts
            foreach (Paragraph p in _translatedSubtitle.Paragraphs)
                p.Text = string.Empty;

            if (!_googleTranslate)
            {
                string from = (comboBoxFrom.SelectedItem as ComboBoxItem).Value;
                string to = (comboBoxTo.SelectedItem as ComboBoxItem).Value;
                DoMicrosoftTranslate(from, to);
                return;
            }

            buttonOK.Enabled = false;
            buttonCancel.Enabled = false;
            _breakTranslation = false;
            buttonTranslate.Text = Configuration.Settings.Language.General.Cancel;
            const int textMaxSize = 1000;
            Cursor.Current = Cursors.WaitCursor;
            progressBar1.Maximum = _subtitle.Paragraphs.Count;
            progressBar1.Value = 0;
            progressBar1.Visible = true;
            labelPleaseWait.Visible = true;
            int start = 0;
            try
            {
                var sb = new StringBuilder();
                int index = 0;
                foreach (Paragraph p in _subtitle.Paragraphs)
                {
                    string text = string.Format("{1} {0} |", p.Text.Replace("|", _newlineString), _splitterString);
                    if (HttpUtility.UrlEncode(sb.ToString() + text).Length >= textMaxSize)
                    {
                        FillTranslatedText(DoTranslate(sb.ToString()), start, index - 1);
                        sb = new StringBuilder();
                        progressBar1.Refresh();
                        Application.DoEvents();
                        start = index;
                    }
                    sb.Append(text);
                    index++;
                    progressBar1.Value = index;
                    if (_breakTranslation)
                        break;
                }
                if (sb.Length > 0)
                    FillTranslatedText(DoTranslate(sb.ToString()), start, index - 1);
            }
            catch (WebException webException)
            {
                MessageBox.Show(webException.Source + ": " + webException.Message);
            }
            finally
            {
                labelPleaseWait.Visible = false;
                progressBar1.Visible = false;
                Cursor.Current = Cursors.Default;
                buttonTranslate.Text = Configuration.Settings.Language.GoogleTranslate.Translate;
                buttonTranslate.Enabled = true;
                buttonOK.Enabled = true;
                buttonCancel.Enabled = true;

                Configuration.Settings.Tools.GoogleTranslateLastTargetLanguage = (comboBoxTo.SelectedItem as ComboBoxItem).Value;
            }
        }

        private void FillTranslatedText(string translatedText, int start, int end)
        {
            List<string> lines = new List<string>();
            foreach (string s in translatedText.Split(new string[] { "|" }, StringSplitOptions.None))
                lines.Add(s);

            int index = start;
            foreach (string s in  lines)
            {
                if (index < _translatedSubtitle.Paragraphs.Count)
                {
                    string cleanText = s.Replace("</p>", string.Empty).Trim();
                    int indexOfP = cleanText.IndexOf(_splitterString.Trim());
                    if (indexOfP >= 0 && indexOfP < 4)
                        cleanText = cleanText.Remove(0, cleanText.IndexOf(_splitterString.Trim()));
                    cleanText = cleanText.Replace(_splitterString.Trim(), string.Empty).Trim();
                    if (cleanText.Contains("\n") && !cleanText.Contains("\r"))
                        cleanText = cleanText.Replace("\n", Environment.NewLine);
                    cleanText = cleanText.Replace(" ...", "...");
                    cleanText = cleanText.Replace(_newlineString.Trim(), Environment.NewLine);
                    cleanText = cleanText.Replace("<br />", Environment.NewLine);
                    cleanText = cleanText.Replace("<br/>", Environment.NewLine);
                    cleanText = cleanText.Replace("<br />", Environment.NewLine);
                    cleanText = cleanText.Replace(Environment.NewLine + " ", Environment.NewLine);
                    cleanText = cleanText.Replace(" " + Environment.NewLine, Environment.NewLine);
                    _translatedSubtitle.Paragraphs[index].Text = cleanText;
                }
                index++;
            }
            subtitleListViewTo.Fill(_translatedSubtitle);
            subtitleListViewTo.SelectIndexAndEnsureVisible(end);
        }

        private string DoTranslate(string input)
        {
            string languagePair = (comboBoxFrom.SelectedItem as ComboBoxItem).Value + "|" +
                                  (comboBoxTo.SelectedItem as ComboBoxItem).Value;

            input = PreTranslate(input.TrimEnd('|').Trim());

            string result = null;
            if (!_googleApiNotWorking)
            {
                try
                {
                    result = TranslateTextViaApi(input, languagePair);
                }
                catch
                {
                    _googleApiNotWorking = true;
                    result = string.Empty;
                }
            }

            // fallback to screen scraping
            if (string.IsNullOrEmpty(result))
            {
                if (_screenScrapingEncoding == null)
                    _screenScrapingEncoding = GetScreenScrapingEncoding(languagePair);
                result = TranslateTextViaScreenScraping(input, languagePair, _screenScrapingEncoding);
                _googleApiNotWorking = true;
            }

            return PostTranslate(result);
        }

        public static string TranslateTextViaApi(string input, string languagePair)
        {
//            string googleApiKey = "ABQIAAAA4j5cWwa3lDH0RkZceh7PjBTDmNAghl5kWSyuukQ0wtoJG8nFBxRPlalq-gAvbeCXMCkmrysqjXV1Gw";
            string googleApiKey = Configuration.Settings.Tools.GoogleApiKey;

            input = input.Replace(Environment.NewLine, _newlineString);
            input = input.Replace("'", "&apos;");
            // create the web request to the Google Translate REST interface

            //API V 1.0
            string url = "http://ajax.googleapis.com/ajax/services/language/translate?v=1.0&q=" + HttpUtility.UrlEncode(input) + "&langpair=" + languagePair + "&key=" +googleApiKey;

            //API V 2.0 ?
            //string[] arr = languagePair.Split('|');
            //string from = arr[0];
            //string to = arr[1];
            //string url = String.Format("https://www.googleapis.com/language/translate/v2?key={3}&q={0}&source={1}&target={2}", HttpUtility.UrlEncode(input), from, to, googleApiKey);

            WebRequest request = WebRequest.Create(url);
            request.Proxy = Utilities.GetProxy();
            WebResponse response = request.GetResponse();
            StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8);
            string content = reader.ReadToEnd();

            string key = "{\"translatedText\":";
            if (content.Contains(key))
            {
                int start = content.IndexOf(key) + key.Length + 1;
                int end = content.IndexOf("\"}", start);
                string translatedText = content.Substring(start, end - start);
                string test = translatedText.Replace("\\u003c", "<");
                test = test.Replace("\\u003e", ">");
                test = test.Replace("\\u0026#39;", "'");
                test = test.Replace("\\u0026amp;", "&");
                test = test.Replace("\\u0026quot;", "\"");
                test = test.Replace("\\u0026apos;", "'");
                test = test.Replace("\\u0026lt;", "<");
                test = test.Replace("\\u0026gt;", ">");
                test = test.Replace("\\u003d", "=");
                test = test.Replace("\\u200b", string.Empty);
                test = test.Replace("\\\"", "\"");
                test = test.Replace(" <br/>", Environment.NewLine);
                test = test.Replace("<br/>", Environment.NewLine);
                test = RemovePStyleParameters(test);
                return test;
            }
            return string.Empty;
        }

        private static string RemovePStyleParameters(string test)
        {
            string key = "<p style";
            while (test.Contains(key))
            {
                int startPosition = test.IndexOf(key);
                int endPosition = test.IndexOf(">", startPosition + key.Length);
                if (endPosition == -1)
                    return test;
                test = test.Remove(startPosition + 2, endPosition-startPosition-2);

            }
            return test;
        }

        public static Encoding GetScreenScrapingEncoding(string languagePair)
        {
            try
            {
                string url = String.Format("http://translate.google.com/?hl=en&eotf=1&sl={0}&tl={1}&q={2}", languagePair.Substring(0, 2), languagePair.Substring(3), "123 456");
                var webClient = new WebClient();
                webClient.Proxy = Utilities.GetProxy();
                string result = webClient.DownloadString(url).ToLower();
                int idx = result.IndexOf("charset");
                int end = result.IndexOf("\"", idx+8);
                string charset = result.Substring(idx, end-idx).Replace("charset=", string.Empty);
                return Encoding.GetEncoding(charset); // "koi8-r");
            }
            catch
            {
                return System.Text.Encoding.Default;
            }
        }

        /// <summary>
        /// Translate Text using Google Translate API's
        /// Google URL - http://www.google.com/translate_t?hl=en&ie=UTF8&text={0}&langpair={1}
        /// </summary>
        /// <param name="input">Input string</param>
        /// <param name="languagePair">2 letter Language Pair, delimited by "|".
        /// E.g. "ar|en" language pair means to translate from Arabic to English</param>
        /// <returns>Translated to String</returns>
        public static string TranslateTextViaScreenScraping(string input, string languagePair, Encoding encoding)
        {
            input = input.Replace(Environment.NewLine, _newlineString);
            input = input.Replace("'", "&apos;");

            //string url = String.Format("http://www.google.com/translate_t?hl=en&ie=UTF8&text={0}&langpair={1}", HttpUtility.UrlEncode(input), languagePair);
            string url = String.Format("http://translate.google.com/?hl=en&eotf=1&sl={0}&tl={1}&q={2}", languagePair.Substring(0, 2), languagePair.Substring(3), HttpUtility.UrlEncode(input));
            var webClient = new WebClient();
            webClient.Proxy = Utilities.GetProxy();
            webClient.Encoding = encoding;
            string result = webClient.DownloadString(url);
            int startIndex = result.IndexOf("<span id=result_box");
            var sb = new StringBuilder();
            if (startIndex > 0)
            {
                startIndex = result.IndexOf("<span title=", startIndex);
                while (startIndex > 0)
                {
                    startIndex = result.IndexOf(">", startIndex);
                    if (startIndex > 0)
                    {
                        startIndex++;
                        int endIndex = result.IndexOf("</span>", startIndex);
                        string translatedText = result.Substring(startIndex, endIndex - startIndex);
                        string test = HttpUtility.HtmlDecode(translatedText);
                        sb.Append(test);
                        startIndex = result.IndexOf("<span title=", startIndex);
                    }
                }
            }
            string res = sb.ToString();
            res = res.Replace(_newlineString, Environment.NewLine);
            res = res.Replace("<BR>", Environment.NewLine);
            res = res.Replace("<BR />", Environment.NewLine);
            res = res.Replace("<BR/>", Environment.NewLine);
            res = res.Replace("< br />", Environment.NewLine);
            res = res.Replace("< br / >", Environment.NewLine);
            res = res.Replace("<br / >", Environment.NewLine);
            res = res.Replace(" <br/>", Environment.NewLine);
            res = res.Replace(" <br/>", Environment.NewLine);
            res = res.Replace("<br/>", Environment.NewLine);
            res = res.Replace("<br />", Environment.NewLine);
            res = res.Replace("  ", " ").Trim();
            res = res.Replace(Environment.NewLine + " ", Environment.NewLine).Trim();
            res = res.Replace(Environment.NewLine + " ", Environment.NewLine).Trim();
            res = res.Replace(" " + Environment.NewLine, Environment.NewLine).Trim();
            res = res.Replace(" " + Environment.NewLine, Environment.NewLine).Trim();
            int end = res.LastIndexOf("<p>");
            if (end > 0)
                res = res.Substring(0, end);
            return res;
        }

        public void FillComboWithLanguages(ComboBox comboBox)
        {
            if (!_googleTranslate)
            {
                if (comboBox == comboBoxTo)
                {
                    foreach (ComboBoxItem item in comboBoxFrom.Items)
                    {
                        comboBoxTo.Items.Add(new ComboBoxItem(item.Text, item.Value));
                    }
                    return;
                }

               // MicrosoftTranslationService.SoapService client = MsTranslationServiceClient;

                //string[] locales = client.GetLanguagesForTranslate(BingApiId);
                string[] locales = GetMsLocales();

//                string[] names = client.GetLanguageNames(BingApiId, "en", locales);
                string[] names = GetMsNames();

                for (int i = 0; i < locales.Length; i++)
                {
                    if (names.Length > i && locales.Length > i)
                        comboBox.Items.Add(new ComboBoxItem(names[i], locales[i]));
                }

                return;
            }

            FillComboWithGoogleLanguages(comboBox);
        }

        private string[] GetMsLocales()
        {
            string[] locales =  {"ar", "bg", "zh-CHS", "zh-CHT", "cs", "da", "nl", "en", "et", "fi", "fr", "de", "el", "ht", "he", "hu", "id", "it", "ja", "ko", "lv", "lt", "no", "pl", "pt", "ro", "ru", "sk", "sl", "es", "sv", "th", "tr", "uk", "vi" };
            return locales;
        }

        private string[] GetMsNames()
        {
            string[] names = { "Arabic", "Bulgarian", "Chinese Simplified", "Chinese Traditional", "Czech", "Danish", "Dutch", "English", "Estonian", "Finnish", "French", "German", "Greek", "Haitian Creole", "Hebrew", "Hungarian", "Indonesian", "Italian", "Japanese", "Korean", "Latvian", "Lithuanian", "Norwegian", "Polish", "Portuguese", "Romanian", "Russian", "Slovak", "Slovenian", "Spanish", "Swedish", "Thai", "Turkish", "Ukrainian", "Vietnamese" };
            return names;
        }

        public static void FillComboWithGoogleLanguages(ComboBox comboBox)
        {
            comboBox.Items.Add(new ComboBoxItem("AFRIKAANS", "af"));
            comboBox.Items.Add(new ComboBoxItem("ALBANIAN", "sq"));
            //            comboBox.Items.Add(new ComboBoxItem("AMHARIC" , "am"));
            comboBox.Items.Add(new ComboBoxItem("ARABIC", "ar"));
            comboBox.Items.Add(new ComboBoxItem("ARMENIAN", "hy"));
            comboBox.Items.Add(new ComboBoxItem("AZERBAIJANI", "az"));
            comboBox.Items.Add(new ComboBoxItem("BASQUE", "eu"));
            comboBox.Items.Add(new ComboBoxItem("BELARUSIAN", "be"));
            comboBox.Items.Add(new ComboBoxItem("BENGALI" , "bn"));
            //            comboBox.Items.Add(new ComboBoxItem("BIHARI" , "bh"));
            comboBox.Items.Add(new ComboBoxItem("BULGARIAN", "bg"));
            //            comboBox.Items.Add(new ComboBoxItem("BURMESE" , "my"));
            comboBox.Items.Add(new ComboBoxItem("CATALAN", "ca"));
            //            comboBox.Items.Add(new ComboBoxItem("CHEROKEE" , "chr"));
            comboBox.Items.Add(new ComboBoxItem("CHINESE", "zh"));
            comboBox.Items.Add(new ComboBoxItem("CHINESE_SIMPLIFIED", "zh-CN"));
            comboBox.Items.Add(new ComboBoxItem("CHINESE_TRADITIONAL", "zh-TW"));
            comboBox.Items.Add(new ComboBoxItem("CROATIAN", "hr"));
            comboBox.Items.Add(new ComboBoxItem("CZECH", "cs"));
            comboBox.Items.Add(new ComboBoxItem("DANISH", "da"));
            //            comboBox.Items.Add(new ComboBoxItem("DHIVEHI" , "dv"));
            comboBox.Items.Add(new ComboBoxItem("DUTCH", "nl"));
            comboBox.Items.Add(new ComboBoxItem("ENGLISH", "en"));
            comboBox.Items.Add(new ComboBoxItem("ESPERANTO" , "eo"));
            comboBox.Items.Add(new ComboBoxItem("ESTONIAN", "et"));
            comboBox.Items.Add(new ComboBoxItem("FILIPINO", "tl"));
            comboBox.Items.Add(new ComboBoxItem("FINNISH", "fi"));
            comboBox.Items.Add(new ComboBoxItem("FRENCH", "fr"));
            comboBox.Items.Add(new ComboBoxItem("GALICIAN", "gl"));
            comboBox.Items.Add(new ComboBoxItem("GEORGIAN", "ka"));
            comboBox.Items.Add(new ComboBoxItem("GERMAN", "de"));
            comboBox.Items.Add(new ComboBoxItem("GREEK", "el"));
            //            comboBox.Items.Add(new ComboBoxItem("GUARANI" , "gn"));
            comboBox.Items.Add(new ComboBoxItem("GUJARATI" , "gu"));
            comboBox.Items.Add(new ComboBoxItem("HAITIAN CREOLE", "ht"));
            comboBox.Items.Add(new ComboBoxItem("HEBREW", "iw"));
            comboBox.Items.Add(new ComboBoxItem("HINDI", "hi"));
            comboBox.Items.Add(new ComboBoxItem("HUNGARIAN", "hu"));
            comboBox.Items.Add(new ComboBoxItem("ICELANDIC", "is"));
            comboBox.Items.Add(new ComboBoxItem("INDONESIAN", "id"));
            comboBox.Items.Add(new ComboBoxItem("IRISH", "ga"));
            //            comboBox.Items.Add(new ComboBoxItem("INUKTITUT" , "iu"));
            comboBox.Items.Add(new ComboBoxItem("ITALIAN", "it"));
            comboBox.Items.Add(new ComboBoxItem("JAPANESE", "ja"));
            comboBox.Items.Add(new ComboBoxItem("KANNADA" , "kn"));
            //            comboBox.Items.Add(new ComboBoxItem("KAZAKH" , "kk"));
            //            comboBox.Items.Add(new ComboBoxItem("KHMER" , "km"));
            comboBox.Items.Add(new ComboBoxItem("KOREAN", "ko"));
            //            comboBox.Items.Add(new ComboBoxItem("KURDISH", "ku"));
            //            comboBox.Items.Add(new ComboBoxItem("KYRGYZ", "ky"));
            //            comboBox.Items.Add(new ComboBoxItem("LAOTHIAN", "lo"));
            comboBox.Items.Add(new ComboBoxItem("LATIN", "la"));
            comboBox.Items.Add(new ComboBoxItem("LATVIAN", "lv"));
            comboBox.Items.Add(new ComboBoxItem("LITHUANIAN", "lt"));
            comboBox.Items.Add(new ComboBoxItem("MACEDONIAN", "mk"));
            comboBox.Items.Add(new ComboBoxItem("MALAY", "ms"));
            //            comboBox.Items.Add(new ComboBoxItem("MALAYALAM" , "ml"));
            comboBox.Items.Add(new ComboBoxItem("MALTESE", "mt"));
            //            comboBox.Items.Add(new ComboBoxItem("MARATHI" , "mr"));
            //            comboBox.Items.Add(new ComboBoxItem("MONGOLIAN" , "mn"));
            //            comboBox.Items.Add(new ComboBoxItem("NEPALI" , "ne"));
            comboBox.Items.Add(new ComboBoxItem("NORWEGIAN", "no"));
            //            comboBox.Items.Add(new ComboBoxItem("ORIYA" , "or"));
            //            comboBox.Items.Add(new ComboBoxItem("PASHTO" , "ps"));
            comboBox.Items.Add(new ComboBoxItem("PERSIAN", "fa"));
            comboBox.Items.Add(new ComboBoxItem("POLISH", "pl"));
            comboBox.Items.Add(new ComboBoxItem("PORTUGUESE", "pt"));
            //            comboBox.Items.Add(new ComboBoxItem("PUNJABI" , "pa"));
            comboBox.Items.Add(new ComboBoxItem("ROMANIAN", "ro"));
            comboBox.Items.Add(new ComboBoxItem("RUSSIAN", "ru"));
            //            comboBox.Items.Add(new ComboBoxItem("SANSKRIT" , "sa"));
            comboBox.Items.Add(new ComboBoxItem("SERBIAN", "sr"));
            //            comboBox.Items.Add(new ComboBoxItem("SINDHI" , "sd"));
            //            comboBox.Items.Add(new ComboBoxItem("SINHALESE" , "si"));
            comboBox.Items.Add(new ComboBoxItem("SLOVAK", "sk"));
            comboBox.Items.Add(new ComboBoxItem("SLOVENIAN", "sl"));
            comboBox.Items.Add(new ComboBoxItem("SPANISH", "es"));
            comboBox.Items.Add(new ComboBoxItem("SWAHILI", "sw"));
            comboBox.Items.Add(new ComboBoxItem("SWEDISH", "sv"));
            //            comboBox.Items.Add(new ComboBoxItem("TAJIK" , "tg"));
            comboBox.Items.Add(new ComboBoxItem("TAMIL" , "ta"));
            //            comboBox.Items.Add(new ComboBoxItem("TAGALOG" , "tl"));
            comboBox.Items.Add(new ComboBoxItem("TELUGU" , "te"));
            comboBox.Items.Add(new ComboBoxItem("THAI", "th"));
            //            comboBox.Items.Add(new ComboBoxItem("TIBETAN" , "bo"));
            comboBox.Items.Add(new ComboBoxItem("TURKISH", "tr"));
            comboBox.Items.Add(new ComboBoxItem("UKRAINIAN", "uk"));
            comboBox.Items.Add(new ComboBoxItem("URDU", "ur"));
            //            comboBox.Items.Add(new ComboBoxItem("UZBEK" , "uz"));
            //            comboBox.Items.Add(new ComboBoxItem("UIGHUR" , "ug"));
            comboBox.Items.Add(new ComboBoxItem("VIETNAMESE", "vi"));
            comboBox.Items.Add(new ComboBoxItem("WELSH", "cy"));
            comboBox.Items.Add(new ComboBoxItem("YIDDISH", "yi"));
        }

        private void LinkLabel1LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (_googleTranslate)
                System.Diagnostics.Process.Start("http://www.google.com/translate");
            else
                System.Diagnostics.Process.Start("http://www.microsofttranslator.com/");
        }

        private void ButtonOkClick(object sender, EventArgs e)
        {
            if (subtitleListViewTo.Items.Count > 0)
            {
                DialogResult = DialogResult.OK;
            }
            else
            {
                DialogResult = DialogResult.Cancel;
            }
        }

        private string PreTranslate(string s)
        {
            Regex reg;

            if ((comboBoxFrom.SelectedItem as ComboBoxItem).Value == "en")
            {
                reg = new Regex("\\bI'm ");
                s = reg.Replace(s, "I am ");

                reg = new Regex("\\bI've ");
                s = reg.Replace(s, "I have ");

                reg = new Regex("\\bI'll ");
                s = reg.Replace(s, "I will ");

                reg = new Regex("\\bI'd "); // had or would???
                s = reg.Replace(s, "I would ");

                reg = new Regex("\\bIt's ");
                s = reg.Replace(s, "It is ");
                reg = new Regex("\\bit's ");
                s = reg.Replace(s, "it is ");

                reg = new Regex("\\bYou're ");
                s = reg.Replace(s, "You are ");
                reg = new Regex("\\byou're ");
                s = reg.Replace(s, "You are ");

                reg = new Regex("\\bYou've ");
                s = reg.Replace(s, "You have ");
                reg = new Regex("\\byou've ");
                s = reg.Replace(s, "you have ");

                reg = new Regex("\\bYou'll ");
                s = reg.Replace(s, "You will ");
                reg = new Regex("\\byou'll ");
                s = reg.Replace(s, "you will ");

                reg = new Regex("\\bYou'd "); // had or would???
                s = reg.Replace(s, "You would ");
                reg = new Regex("\\byou'd ");
                s = reg.Replace(s, "you would ");

                reg = new Regex("\\bHe's ");
                s = reg.Replace(s, "He is ");
                reg = new Regex("\\bhe's ");
                s = reg.Replace(s, "he is ");

                reg = new Regex("\\bShe's ");
                s = reg.Replace(s, "She is ");
                reg = new Regex("\\bshe's ");
                s = reg.Replace(s, "she is ");

                reg = new Regex("\\bWe're ");
                s = reg.Replace(s, "We are ");
                reg = new Regex("\\bwe're ");
                s = reg.Replace(s, "we are ");

                reg = new Regex("\\bwon't ");
                s = reg.Replace(s, "will not ");

                reg = new Regex("\\bThey're ");
                s = reg.Replace(s, "They are ");
                reg = new Regex("\\bthey're ");
                s = reg.Replace(s, "they are ");

                reg = new Regex("\\bWho's ");
                s = reg.Replace(s, "Who is ");
                reg = new Regex("\\bwho's ");
                s = reg.Replace(s, "who is ");

                reg = new Regex("\\bThat's ");
                s = reg.Replace(s, "That is ");
                reg = new Regex("\\bthat's ");
                s = reg.Replace(s, "that is ");

                reg = new Regex("\\bWhat's ");
                s = reg.Replace(s, "What is ");
                reg = new Regex("\\bwhat's ");
                s = reg.Replace(s, "what is ");

                reg = new Regex("\\bWhere's ");
                s = reg.Replace(s, "Where is ");
                reg = new Regex("\\bwhere's ");
                s = reg.Replace(s, "where is ");

                reg = new Regex("\\bWho's ");
                s = reg.Replace(s, "Who is ");
                reg = new Regex("\\bwho's ");
                s = reg.Replace(s, "who is ");

                reg = new Regex(" 'Cause "); // \b (word boundry) does not workig with '
                s = reg.Replace(s, " Because ");
                reg = new Regex(" 'cause ");
                s = reg.Replace(s, " because ");
                reg = new Regex("\\r\\n'Cause ");
                s = reg.Replace(s, "\r\nBecause ");
                reg = new Regex("\\r\\n'cause ");
                s = reg.Replace(s, "\r\nbecause ");
                reg = new Regex(">'Cause ");
                s = reg.Replace(s, ">Because ");
                reg = new Regex(">'cause ");
                s = reg.Replace(s, ">because ");
            }
            return s;
        }

        private string PostTranslate(string s)
        {
            if ((comboBoxTo.SelectedItem as ComboBoxItem).Value == "da")
            {
                s = s.Replace("Jeg ved.", "Jeg ved det.");
                s = s.Replace(", jeg ved.", ", jeg ved det.");

                s = s.Replace("Jeg er ked af.", "Jeg er ked af det.");
                s = s.Replace(", jeg er ked af.", ", jeg er ked af det.");

                s = s.Replace("Come on.", "Kom nu.");
                s = s.Replace(", come on.", ", kom nu.");
                s = s.Replace("Come on,", "Kom nu,");

                s = s.Replace("Hey ", "Hej ");
                s = s.Replace("Hey,", "Hej,");

                s = s.Replace(" gonna ", " ville ");
                s = s.Replace("Gonna ", "Vil ");

                s = s.Replace("Ked af.", "Undskyld.");
            }
            return s;
        }

        private void FormGoogleTranslate_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape && labelPleaseWait.Visible == false)
                DialogResult = DialogResult.Cancel;
            else if (e.KeyCode == Keys.Escape && labelPleaseWait.Visible)
            {
                _breakTranslation = true;
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
            else if (e.KeyCode == Keys.F1)
                Utilities.ShowHelp("#translation");
            else if (e.Control && e.Shift && e.Alt && e.KeyCode == Keys.L)
            {
                Cursor.Current = Cursors.WaitCursor;
                Configuration.Settings.Language.Save();
                Language.TranslateViaGoogle((comboBoxFrom.SelectedItem as ComboBoxItem).Value + "|" +
                                            (comboBoxTo.SelectedItem as ComboBoxItem).Value);
                Cursor.Current = Cursors.Default;
            }
        }

        private void GoogleTranslate_Resize(object sender, EventArgs e)
        {
            int width = (this.Width / 2) - (subtitleListViewFrom.Left * 3) + 19;
            subtitleListViewFrom.Width = width;
            subtitleListViewTo.Width = width;

            int height = this.Height - (subtitleListViewFrom.Top + buttonTranslate.Height + 60);
            subtitleListViewFrom.Height = height;
            subtitleListViewTo.Height = height;

            comboBoxFrom.Left = subtitleListViewFrom.Left + (subtitleListViewFrom.Width - comboBoxFrom.Width);
            labelFrom.Left = comboBoxFrom.Left - 5 - labelFrom.Width;

            subtitleListViewTo.Left = width + (subtitleListViewFrom.Left * 2);
            labelTo.Left = subtitleListViewTo.Left;
            comboBoxTo.Left = labelTo.Left + labelTo.Width + 5;
            buttonTranslate.Left = comboBoxTo.Left + comboBoxTo.Width + 9;
            labelPleaseWait.Left = buttonTranslate.Left + buttonTranslate.Width + 9;
            progressBar1.Left = labelPleaseWait.Left;
            progressBar1.Width = subtitleListViewTo.Width - (progressBar1.Left - subtitleListViewTo.Left);
        }

        private MicrosoftTranslationService.SoapService MsTranslationServiceClient
        {
            get
            {
                if (_microsoftTranslationService == null)
                {
                    _microsoftTranslationService = new MicrosoftTranslationService.SoapService();
                    _microsoftTranslationService.Proxy = Utilities.GetProxy();
                }
                return _microsoftTranslationService;
            }
        }

        public void DoMicrosoftTranslate(string from, string to)
        {
            MicrosoftTranslationService.SoapService client = MsTranslationServiceClient;

            _breakTranslation = false;
            buttonTranslate.Text = Configuration.Settings.Language.General.Cancel;
            const int textMaxSize = 10000;
            Cursor.Current = Cursors.WaitCursor;
            progressBar1.Maximum = _subtitle.Paragraphs.Count;
            progressBar1.Value = 0;
            progressBar1.Visible = true;
            labelPleaseWait.Visible = true;
            int start = 0;
            bool overQuota = false;
            try
            {
                var sb = new StringBuilder();
                int index = 0;
                foreach (Paragraph p in _subtitle.Paragraphs)
                {
                    string text = string.Format("{1}{0}|", p.Text, _splitterString);
                    if (!overQuota)
                    {
                        if ((HttpUtility.UrlEncode(sb.ToString() + text)).Length >= textMaxSize)
                        {
                            try
                            {
                                FillTranslatedText(client.Translate(Configuration.Settings.Tools.MicrosoftBingApiId, sb.ToString().Replace(Environment.NewLine, "<br />"), from, to, "text/plain", "general"), start, index - 1);
                            }
                            catch (System.Web.Services.Protocols.SoapHeaderException exception)
                            {
                                MessageBox.Show("Sorry, MS is closing their free api: " + exception.Message);
                                overQuota = true;
                            }
                            sb = new StringBuilder();
                            progressBar1.Refresh();
                            Application.DoEvents();
                            start = index;
                        }
                        sb.Append(text);
                    }
                    index++;
                    progressBar1.Value = index;
                    if (_breakTranslation)
                        break;
                }
                if (sb.Length > 0 && !overQuota)
                {
                    try
                    {
                        FillTranslatedText(client.Translate(Configuration.Settings.Tools.MicrosoftBingApiId, sb.ToString().Replace(Environment.NewLine, "<br />"), from, to, "text/plain", "general"), start, index - 1);
                    }
                    catch (System.Web.Services.Protocols.SoapHeaderException exception)
                    {
                        MessageBox.Show("Sorry, MS is closing their free api: " + exception.Message);
                        overQuota = true;
                    }
                }
            }
            finally
            {
                labelPleaseWait.Visible = false;
                progressBar1.Visible = false;
                Cursor.Current = Cursors.Default;
                buttonTranslate.Text = Configuration.Settings.Language.GoogleTranslate.Translate;
                buttonTranslate.Enabled = true;
            }
        }

        private void subtitleListViewFrom_DoubleClick(object sender, EventArgs e)
        {
            if (subtitleListViewFrom.SelectedItems.Count > 0)
            {
                int index = subtitleListViewFrom.SelectedItems[0].Index;
                if (index < subtitleListViewTo.Items.Count)
                {
                    subtitleListViewTo.SelectIndexAndEnsureVisible(index);
                }
            }
        }

        private void subtitleListViewTo_DoubleClick(object sender, EventArgs e)
        {
            if (subtitleListViewTo.SelectedItems.Count > 0)
            {
                int index = subtitleListViewTo.SelectedItems[0].Index;
                if (index < subtitleListViewFrom.Items.Count)
                {
                    subtitleListViewFrom.SelectIndexAndEnsureVisible(index);
                }
            }
        }

    }
}
