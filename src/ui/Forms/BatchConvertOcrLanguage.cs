using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Ocr;
using System;
using System.Windows.Forms;
using Nikse.SubtitleEdit.Core.Common;
using System.IO;
using System.Linq;

namespace Nikse.SubtitleEdit.Forms
{
    public sealed partial class BatchConvertOcrLanguage : Form
    {

        private class OcrLanguageItem
        {
            public string Id { get; set; }
            public string Text { get; set; }

            public override string ToString()
            {
                return Text;
            }
        }

        public string OcrEngine { get; set; }
        public string OcrLanguage { get; set; }

        public BatchConvertOcrLanguage(string ocrEngine, string ocrLanguage)
        {
            UiUtil.PreInitialize(this);
            InitializeComponent();
            UiUtil.FixFonts(this);

            Text = LanguageSettings.Current.VobSubOcr.OcrMethod;
            buttonCancel.Text = LanguageSettings.Current.General.Cancel;
            buttonOK.Text = LanguageSettings.Current.General.Ok;
            UiUtil.FixLargeFonts(this, buttonOK);

            OcrEngine = ocrEngine;
            OcrLanguage = ocrLanguage;
        }

        private void BatchConvertMkvEnding_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                DialogResult = DialogResult.Cancel;
            }
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            OcrLanguage = (comboBoxLanguage.SelectedItem as OcrLanguageItem).Id;

            DialogResult = DialogResult.OK;
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void BatchConvertMkvEnding_Load(object sender, EventArgs e)
        {
            if (OcrEngine.Equals("nOcr", StringComparison.OrdinalIgnoreCase))
            {
                comboBoxOcrMethod.SelectedIndex = 1;
            }
            else
            {
                comboBoxOcrMethod.SelectedIndex = 0;
            }
        }

        private void InitLanguage()
        {
            comboBoxLanguage.Items.Clear();

            if (OcrEngine.Equals("nOcr", StringComparison.OrdinalIgnoreCase))
            {
                foreach (var s in NOcrDb.GetDatabases())
                {
                    comboBoxLanguage.Items.Add(new OcrLanguageItem { Id = s, Text = s });
                    if (s == Configuration.Settings.VobSubOcr.LineOcrLastLanguages)
                    {
                        comboBoxLanguage.SelectedIndex = comboBoxLanguage.Items.Count - 1;
                    }
                }
            }
            else
            {
                var dir = Configuration.TesseractDataDirectory; // is T5 default?
                if (Directory.Exists(dir))
                {
                    var cultures = Iso639Dash2LanguageCode.List;
                    foreach (var fileName in Directory.GetFiles(dir, "*.traineddata"))
                    {
                        string tesseractName = Path.GetFileNameWithoutExtension(fileName);
                        if (tesseractName != "osd" && tesseractName != "music" && !tesseractName.EndsWith("-frak", StringComparison.Ordinal))
                        {
                            string cultureName = tesseractName;
                            var match = cultures.FirstOrDefault(p => p.ThreeLetterCode == tesseractName);
                            if (match != null)
                            {
                                cultureName = match.EnglishName;
                            }
                            else if (tesseractName == "chi_sim")
                            {
                                cultureName = "Chinese simplified";
                            }
                            else if (tesseractName == "chi_tra")
                            {
                                cultureName = "Chinese traditional";
                            }
                            else if (tesseractName == "per")
                            {
                                cultureName = "Farsi";
                            }
                            else if (tesseractName == "nor")
                            {
                                cultureName = "Norwegian";
                            }

                            comboBoxLanguage.Items.Add(new OcrLanguageItem { Id = tesseractName, Text = cultureName });
                        }
                    }
                }
            }

            if (comboBoxLanguage.Items.Count > 0 && comboBoxLanguage.SelectedIndex < 0)
            {
                comboBoxLanguage.SelectedIndex = 0;
            }
        }

        private void comboBoxOcrMethod_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBoxOcrMethod.SelectedIndex == 1)
            {
                OcrEngine = "nOCR";
            }
            else
            {
                OcrEngine = "Tesseract";
            }

            InitLanguage();
        }
    }
}
