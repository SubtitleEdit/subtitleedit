using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.Xml;
using Nikse.SubtitleEdit.Logic;
using System.Globalization;

namespace Nikse.SubtitleEdit.Forms
{
    public partial class AddToOcrReplaceList : Form
    {
        private string _threeLetterISOLanguageName;

        public AddToOcrReplaceList()
        {
            InitializeComponent();
            Text = Configuration.Settings.Language.AddToOcrReplaceList.Title;
            labelDescription.Text = Configuration.Settings.Language.AddToOcrReplaceList.Description;
            buttonCancel.Text = Configuration.Settings.Language.General.Cancel;
            buttonOK.Text = Configuration.Settings.Language.General.OK;
            FixLargeFonts();
        }

        private void FixLargeFonts()
        {
            Graphics graphics = this.CreateGraphics();
            SizeF textSize = graphics.MeasureString(buttonOK.Text, this.Font);
            if (textSize.Height > buttonOK.Height - 4)
            {
                int newButtonHeight = (int)(textSize.Height + 7 + 0.5);
                Utilities.SetButtonHeight(this, newButtonHeight, 1);
            }
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            string key = textBoxOcrFixKey.Text.Trim();
            string value = textBoxOcrFixValue.Text.Trim();
            if (key.Length == 0 || value.Length == 0)
                return;
            if (key == value)
                return;

            var ocrFixWords = new Dictionary<string, string>();
            var ocrFixPartialLines = new Dictionary<string, string>();

            try
            {
                var ci = new CultureInfo(LanguageString.Replace("_", "-"));
                _threeLetterISOLanguageName = ci.ThreeLetterISOLanguageName;
            }
            catch
            {
            }

            string replaceListXmlFileName = Utilities.DictionaryFolder + _threeLetterISOLanguageName + "_OCRFixReplaceList.xml";
            if (File.Exists(replaceListXmlFileName))
            {
                var xml = new XmlDocument();
                xml.Load(replaceListXmlFileName);
                ocrFixWords = Logic.OCR.OcrFixEngine.LoadReplaceList(xml, "WholeWords");
                ocrFixPartialLines = Logic.OCR.OcrFixEngine.LoadReplaceList(xml, "PartialLines");
            }
            Dictionary<string, string> dictionary = ocrFixWords;
            string elementName = "Word";
            string parentName = "WholeWords";

            if (key.Contains(" "))
            {
                dictionary = ocrFixPartialLines;
                elementName = "LinePart";
                parentName = "PartialLines";
            }

            if (dictionary.ContainsKey(key))
            {
                MessageBox.Show(Configuration.Settings.Language.Settings.WordAlreadyExists);
                return;
            }

            dictionary.Add(key, value);

            //Sort
            var sortedDictionary = new SortedDictionary<string, string>();
            foreach (var pair in dictionary)
            {
                if (!sortedDictionary.ContainsKey(pair.Key))
                    sortedDictionary.Add(pair.Key, pair.Value);
            }

            var doc = new XmlDocument();
            if (File.Exists(replaceListXmlFileName))
                doc.Load(replaceListXmlFileName);
            else
                doc.LoadXml("<OCRFixReplaceList><WholeWords/><PartialWords/><PartialLines/><BeginLines/><EndLines/><WholeLines/></OCRFixReplaceList>");

            XmlNode wholeWords = doc.DocumentElement.SelectSingleNode(parentName);
            wholeWords.RemoveAll();
            foreach (var pair in sortedDictionary)
            {
                XmlNode node = doc.CreateElement(elementName);

                XmlAttribute wordFrom = doc.CreateAttribute("from");
                wordFrom.InnerText = pair.Key;
                node.Attributes.Append(wordFrom);

                XmlAttribute wordTo = doc.CreateAttribute("to");
                wordTo.InnerText = pair.Value;
                node.Attributes.Append(wordTo);

                wholeWords.AppendChild(node);
            }
            doc.Save(replaceListXmlFileName);
            DialogResult = DialogResult.OK;
            NewSource = key;
            NewTarget = value;
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void AddToOcrReplaceList_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
                DialogResult = DialogResult.Cancel;
        }

        public string NewSource { get; set; }
        public string NewTarget { get; set; }

        internal void Initialize(Subtitle subtitle, string languageId, string hunspellName, string source)
        {
            if (!string.IsNullOrEmpty(source))
                textBoxOcrFixKey.Text = source;

            comboBoxDictionaries.Items.Clear();
            foreach (string name in Utilities.GetDictionaryLanguages())
            {
                comboBoxDictionaries.Items.Add(name);
                if (hunspellName != null && name.ToLower() == hunspellName.ToLower())
                    comboBoxDictionaries.SelectedIndex = comboBoxDictionaries.Items.Count - 1;
            }
            _threeLetterISOLanguageName = languageId;
        }

        public string LanguageString
        {
            get
            {
                string name = comboBoxDictionaries.SelectedItem.ToString();
                int start = name.LastIndexOf("[");
                int end = name.LastIndexOf("]");
                if (start >= 0 && end > start)
                {
                    start++;
                    name = name.Substring(start, end - start);
                    return name;
                }
                return null;
            }
        }
    }
}
