using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SpellCheck;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.SpellCheck;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.Forms.Ocr
{
    public partial class WordSplitDictionaryGenerator : Form
    {
        private List<Subtitle> _subtitleList;
        private Hunspell _hunspell;

        public WordSplitDictionaryGenerator()
        {
            UiUtil.PreInitialize(this);
            InitializeComponent();
            UiUtil.FixFonts(this);
            FillSpellCheckDictionaries();
            _subtitleList = new List<Subtitle>();
            comboBoxMinOccurrences.SelectedIndex = 13;
            comboBoxMinOccurrencesLongWords.SelectedIndex = 5;
            listViewInputFiles.AutoSizeLastColumn();
            labelStatus.Text = string.Empty;
        }

        private void FillSpellCheckDictionaries()
        {
            comboBoxDictionaries.Items.Clear();
            foreach (string name in Utilities.GetDictionaryLanguages())
            {
                comboBoxDictionaries.Items.Add(name);
            }

            if (comboBoxDictionaries.Items.Count > 0)
            {
                comboBoxDictionaries.SelectedIndex = 0;
            }
        }

        private void buttonInputBrowse_Click(object sender, EventArgs e)
        {
            buttonInputBrowse.Enabled = false;
            using (var openFileDialog1 = new OpenFileDialog
            {
                Title = LanguageSettings.Current.General.OpenSubtitle,
                FileName = string.Empty,
                Filter = UiUtil.SubtitleExtensionFilter.Value,
                Multiselect = true
            })
            {
                if (openFileDialog1.ShowDialog(this) == DialogResult.OK)
                {
                    try
                    {
                        Cursor = Cursors.WaitCursor;
                        labelStatus.Text = LanguageSettings.Current.General.PleaseWait;
                        listViewInputFiles.BeginUpdate();
                        foreach (string fileName in openFileDialog1.FileNames)
                        {
                            AddInputFile(fileName);
                            Application.DoEvents();
                        }
                    }
                    finally
                    {
                        listViewInputFiles.EndUpdate();
                        Cursor = Cursors.Default;
                        labelStatus.Text = string.Empty;
                    }
                }

                buttonInputBrowse.Enabled = true;
            }

            labelStatus.Text = $"{listViewInputFiles.Items.Count} input files";
            ButtonGenerate.Enabled = listViewInputFiles.Items.Count > 0;
        }

        private void AddInputFile(string fileName)
        {
            foreach (ListViewItem lvi in listViewInputFiles.Items)
            {
                if (lvi.Text.Equals(fileName, StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }
            }

            var fi = new FileInfo(fileName);
            var ext = fi.Extension.ToLowerInvariant();
            var item = new ListViewItem(fileName);
            item.SubItems.Add(Utilities.FormatBytesToDisplayFileSize(fi.Length));
            var sub = new Subtitle();
            if (fi.Length < 500_000)
            {
                if (!FileUtil.IsBluRaySup(fileName) && !FileUtil.IsVobSub(fileName) &&
                    !((ext == ".mkv" || ext == ".mks") && FileUtil.IsMatroskaFile(fileName)))
                {
                    SubtitleFormat format = sub.LoadSubtitle(fileName, out _, null);

                    if (format == null)
                    {
                        foreach (var f in SubtitleFormat.GetBinaryFormats(true))
                        {
                            if (f.IsMine(null, fileName))
                            {
                                f.LoadSubtitle(sub, null, fileName);
                                format = f;
                                break;
                            }
                        }
                    }

                    if (format == null)
                    {
                        var encoding = LanguageAutoDetect.GetEncodingFromFile(fileName);
                        var lines = FileUtil.ReadAllTextShared(fileName, encoding).SplitToLines();
                        foreach (var f in SubtitleFormat.GetTextOtherFormats())
                        {
                            if (f.IsMine(lines, fileName))
                            {
                                f.LoadSubtitle(sub, lines, fileName);
                                format = f;
                                break;
                            }
                        }
                    }

                    if (format != null)
                    {
                        item.SubItems.Add(format.Name);
                        listViewInputFiles.Items.Add(item);
                        _subtitleList.Add(sub);
                    }
                }
            }
        }

        private void okButton_Click(object sender, EventArgs e)
        {
            LoadHunspell();
            var wordDictionary = new Dictionary<string, int>();
            foreach (var subtitle in _subtitleList)
            {
                foreach (var p in subtitle.Paragraphs)
                {
                    var words = SpellCheckWordLists.Split(HtmlUtil.RemoveHtmlTags(p.Text, true));
                    foreach (var word in words)
                    {
                        if (!_hunspell.Spell(word.Text) || Utilities.IsNumber(word.Text))
                        {
                            continue;
                        }

                        if (wordDictionary.ContainsKey(word.Text))
                        {
                            wordDictionary[word.Text]++;
                        }
                        else
                        {
                            wordDictionary.Add(word.Text, 1);
                        }
                    }
                }

                labelStatus.Text = $"{wordDictionary.Count:#,###,##0} words...";
                labelStatus.Refresh();
                Application.DoEvents();
            }

            SaveFile(wordDictionary);
        }

        private void SaveFile(Dictionary<string, int> wordDictionary)
        {
            int minUseCountSmall = int.Parse(comboBoxMinOccurrences.Text);
            int minUseCountLarge = int.Parse(comboBoxMinOccurrencesLongWords.Text);
            using (var saveFileDialog = new SaveFileDialog
            {
                Title = LanguageSettings.Current.General.OpenSubtitle,
                FileName = GetThreeLetterLanguageCode() + "_WordSplitList",
                Filter = "Text|*.txt",
                InitialDirectory = Configuration.DictionariesDirectory,
            })
            {
                if (saveFileDialog.ShowDialog(this) == DialogResult.OK)
                {
                    var list = new List<string>();
                    foreach (var word in wordDictionary)
                    {
                        if (word.Key.Length < 5 && word.Value >= minUseCountSmall ||
                            word.Key.Length >= 5 && word.Value >= minUseCountLarge)
                        {
                            list.Add(word.Key);
                        }
                    }

                    var sb = new StringBuilder();
                    foreach (var word in list.OrderByDescending(prop => prop.Length))
                    {
                        sb.AppendLine(word);
                    }

                    File.WriteAllText(saveFileDialog.FileName, sb.ToString());

                    var info = $"{list.Count:#,###,##0} words saved in {saveFileDialog.FileName}";
                    labelStatus.Text = info;
                    using (var f = new ExportPngXmlDialogOpenFolder(info + Environment.NewLine + "File is created with longest words first - do check the bottom of the file regarding valid one/two/three letter words!", Path.GetDirectoryName(saveFileDialog.FileName), saveFileDialog.FileName))
                    {
                        f.ShowDialog(this);
                    }
                }
            }
        }

        private string GetLanguageCode()
        {
            var languageString = comboBoxDictionaries.Text;
            if (languageString.IndexOf('[') > 0 && languageString.IndexOf('[') < languageString.IndexOf(']'))
            {
                languageString = languageString.Substring(languageString.IndexOf('[') + 1);
                languageString = languageString.Substring(0, languageString.IndexOf(']'));
            }

            return languageString;
        }

        private string GetThreeLetterLanguageCode()
        {
            var languageString = GetLanguageCode().Split('_', '-').First();
            return Iso639Dash2LanguageCode.GetThreeLetterCodeFromTwoLetterCode(languageString);
        }

        private void LoadHunspell()
        {
            var dictionary = Utilities.DictionaryFolder + GetLanguageCode();
            _hunspell?.Dispose();
            _hunspell = null;
            _hunspell = Hunspell.GetHunspell(dictionary);
        }

        private void WordSplitDictionaryGenerator_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                DialogResult = DialogResult.Cancel;
            }
        }

        private void WordSplitDictionaryGenerator_ResizeEnd(object sender, EventArgs e)
        {
            listViewInputFiles.AutoSizeLastColumn();
        }
    }
}
