using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.OCR;
using Nikse.SubtitleEdit.Logic.VobSub;
using System.Diagnostics;

namespace Nikse.SubtitleEdit.Forms
{
    public sealed partial class VobSubOcr : Form
    {
        private class CompareItem
        {
            public Bitmap Bitmap { get; private set; }
            public string Name { get; private set; }
            public bool Italic { get; set; }
            public int ExpandCount { get; private set; }

            public CompareItem(Bitmap bmp, string name, bool isItalic, int expandCount)
            {
                Bitmap = bmp;
                Name = name;
                Italic = isItalic;
                ExpandCount = expandCount;
            }
        }

        private class CompareMatch
        {
            public string Text { get; set; }
            public bool Italic { get; set; }
            public int ExpandCount { get; set; }
            public CompareMatch(string text, bool italic, int expandCount)
            {
                Text = text;
                Italic = italic;
                ExpandCount = expandCount;
            }
        }

        private class TesseractLanguage
        {
            public string Id { get; set; }
            public string Text { get; set; }
            public override string ToString()
            {
                return Text;
            }
        }

        private class ModiParameter
        {
            public Bitmap Bitmap { get; set; }
            public string Text { get; set; }
            public int Language { get; set; }
        }

        public string FileName { get; set; }
        Subtitle _subtitle = new Subtitle();
        List<CompareItem> _compareBitmaps;
        XmlDocument _compareDoc = new XmlDocument();
        Point _manualOcrDialogPosition = new Point(-1, -1);
        volatile bool _abort;
        int _selectedIndex = -1;
        VobSubOcrSettings _vobSubOcrSettings;
        bool _italicCheckedLast;
        bool _useNewSubIdxCode;

        Type _modiType; 
        Object _modiDoc;
        bool _modiEnabled;

        // DVD rip/vobsub
        List<VobSubMergedPack> _vobSubMergedPackistOriginal;
        List<VobSubMergedPack> _vobSubMergedPackist;
        List<Color> _palette;

        // BluRay sup
        List<Logic.BluRaySup.BluRaySupPicture> _bluRaySubtitlesOriginal;
        List<Logic.BluRaySup.BluRaySupPicture> _bluRaySubtitles;
        Nikse.SubtitleEdit.Logic.BluRaySup.BluRaySupPalette _defaultPaletteInfo;

        // Tesseract OCR
        //tessnet2.Tesseract _tesseractOcrEngine;
//        object _tesseractOcrEngine;
        string _lastLine;
        string _languageId;

        // Dictionaries/spellchecking/fixing
        OcrFixEngine _ocrFixEngine;
        int _tessnetOcrAutoFixes;

        public VobSubOcr()
        {
            InitializeComponent();

            var language = Configuration.Settings.Language.VobSubOcr;
            Text = language.Title;
            groupBoxOcrMethod.Text = language.OcrMethod;
            labelTesseractLanguage.Text = language.Language;
            labelImageDatabase.Text = language.ImageDatabase;
            labelNoOfPixelsIsSpace.Text = language.NoOfPixelsIsSpace;
            buttonNewCharacterDatabase.Text = language.New;
            buttonEditCharacterDatabase.Text = language.Edit;
            buttonStartOcr.Text = language.StartOcr;
            buttonStop.Text = language.Stop;
            labelStartFrom.Text = language.StartOcrFrom;
            labelStatus.Text = language.LoadingVobSubImages;
            groupBoxSubtitleImage.Text = language.SubtitleImage;
            labelSubtitleText.Text = language.SubtitleText;
            buttonOK.Text = Configuration.Settings.Language.General.OK;
            buttonCancel.Text = Configuration.Settings.Language.General.Cancel;
            subtitleListView1.InitializeLanguage(Configuration.Settings.Language.General, Configuration.Settings);
            subtitleListView1.Columns[0].Width = 45;
            subtitleListView1.Columns[1].Width = 90;
            subtitleListView1.Columns[2].Width = 90;
            subtitleListView1.Columns[3].Width = 70;
            subtitleListView1.Columns[4].Width = 150;
            subtitleListView1.InitializeTimeStampColumWidths(this);

            groupBoxImagePalette.Text = language.ImagePalette;
            checkBoxCustomFourColors.Text = language.UseCustomColors;
            checkBoxPatternTransparent.Text = language.Transparent;
            checkBoxEmphasis1Transparent.Text = language.Transparent;
            checkBoxEmphasis2Transparent.Text = language.Transparent;
            checkBoxPromptForUnknownWords.Text = language.PromptForUnknownWords;

            groupBoxOcrAutoFix.Text = language.OcrAutoCorrectionSpellchecking;
            checkBoxGuessUnknownWords.Text = language.TryToGuessUnkownWords;
            checkBoxAutoBreakLines.Text = language.AutoBreakSubtitleIfMoreThanTwoLines;
            tabControlLogs.TabPages[0].Text = language.AllFixes;
            tabControlLogs.TabPages[1].Text = language.GuessesUsed;
            tabControlLogs.TabPages[2].Text = language.UnknownWords;

            numericUpDownPixelsIsSpace.Left = labelNoOfPixelsIsSpace.Left + labelNoOfPixelsIsSpace.Width + 5;
            groupBoxSubtitleImage.Text = string.Empty;
            labelFixesMade.Text = string.Empty;
            labelFixesMade.Left = checkBoxAutoFixCommonErrors.Left + checkBoxAutoFixCommonErrors.Width;
            labelDictionaryLoaded.Text = string.Empty;
            groupBoxImageCompareMethod.Text = language.OcrViaImageCompare;
            groupBoxModiMethod.Text = language.OcrViaModi;
            checkBoxAutoFixCommonErrors.Text = language.FixOcrErrors;
            checkBoxRightToLeft.Text = language.RightToLeft;
            checkBoxRightToLeft.Left = numericUpDownPixelsIsSpace.Left;

            comboBoxOcrMethod.Items.Clear();
            comboBoxOcrMethod.Items.Add(language.OcrViaTesseract);
            comboBoxOcrMethod.Items.Add(language.OcrViaImageCompare);
            comboBoxOcrMethod.Items.Add(language.OcrViaModi);

            checkBoxUseModiInTesseractForUnknownWords.Text = language.TryModiForUnknownWords;
            checkBoxShowOnlyForced.Text = language.ShowOnlyForcedSubtitles;
            checkBoxUseTimeCodesFromIdx.Text = language.UseTimeCodesFromIdx;

            comboBoxTesseractLanguages.Left = labelTesseractLanguage.Left + labelTesseractLanguage.Width;

            FixLargeFonts();
        }

        private void FixLargeFonts()
        {
            Graphics graphics = this.CreateGraphics();
            SizeF textSize = graphics.MeasureString(buttonCancel.Text, this.Font);
            if (textSize.Height > buttonCancel.Height - 4)
            {
                int newButtonHeight = (int)(textSize.Height + 7 + 0.5);
                Utilities.SetButtonHeight(this, newButtonHeight, 1);
            }
        }

        internal bool Initialize(string vobSubFileName, VobSubOcrSettings vobSubOcrSettings, bool useNewSubIdxCode)
        {
            _useNewSubIdxCode = useNewSubIdxCode;
            buttonOK.Enabled = false;
            buttonCancel.Enabled = false;
            buttonStartOcr.Enabled = false;
            buttonStop.Enabled = false;
            buttonNewCharacterDatabase.Enabled = false;
            buttonEditCharacterDatabase.Enabled = false;
            labelStatus.Text = string.Empty;
            progressBar1.Visible = false;
            progressBar1.Maximum = 100;
            progressBar1.Value = 0;
            numericUpDownPixelsIsSpace.Value = vobSubOcrSettings.XOrMorePixelsMakesSpace;
            _vobSubOcrSettings = vobSubOcrSettings;

            InitializeModi();
            InitializeTesseract();
            LoadImageCompareCharacterDatabaseList();

            if (Configuration.Settings.VobSubOcr.LastOcrMethod == "BitmapCompare" && comboBoxOcrMethod.Items.Count > 1)
                comboBoxOcrMethod.SelectedIndex = 1;
            else if (Configuration.Settings.VobSubOcr.LastOcrMethod == "MODI" && comboBoxOcrMethod.Items.Count > 2)
                comboBoxOcrMethod.SelectedIndex = 2;
            else
                comboBoxOcrMethod.SelectedIndex = 0;

            FileName = vobSubFileName;
            Text += " - " + Path.GetFileName(FileName);

            return InitializeSubIdx(vobSubFileName);
        }

        internal void Initialize(List<VobSubMergedPack> vobSubMergedPackist, List<Color> palette, VobSubOcrSettings vobSubOcrSettings, string languageString)
        {
            buttonOK.Enabled = false;
            buttonCancel.Enabled = false;
            buttonStartOcr.Enabled = false;
            buttonStop.Enabled = false;
            buttonNewCharacterDatabase.Enabled = false;
            buttonEditCharacterDatabase.Enabled = false;
            labelStatus.Text = string.Empty;
            progressBar1.Visible = false;
            progressBar1.Maximum = 100;
            progressBar1.Value = 0;
            numericUpDownPixelsIsSpace.Value = vobSubOcrSettings.XOrMorePixelsMakesSpace;
            _vobSubOcrSettings = vobSubOcrSettings;

            InitializeModi();
            InitializeTesseract();
            LoadImageCompareCharacterDatabaseList();

            if (Configuration.Settings.VobSubOcr.LastOcrMethod == "BitmapCompare" && comboBoxOcrMethod.Items.Count > 1)
                comboBoxOcrMethod.SelectedIndex = 1;
            else if (Configuration.Settings.VobSubOcr.LastOcrMethod == "MODI" && comboBoxOcrMethod.Items.Count > 2)
                comboBoxOcrMethod.SelectedIndex = 2;
            else
                comboBoxOcrMethod.SelectedIndex = 0;

            _vobSubMergedPackist = vobSubMergedPackist;
            _palette = palette;

            if (_palette == null)
                checkBoxCustomFourColors.Checked = true;

            SetTesseractLanguageFromLanguageString(languageString);
        }

        internal void Initialize(List<Logic.BluRaySup.BluRaySupPicture> subtitles, VobSubOcrSettings vobSubOcrSettings)
        {
            buttonOK.Enabled = false;
            buttonCancel.Enabled = false;
            buttonStartOcr.Enabled = false;
            buttonStop.Enabled = false;
            buttonNewCharacterDatabase.Enabled = false;
            buttonEditCharacterDatabase.Enabled = false;
            labelStatus.Text = string.Empty;
            progressBar1.Visible = false;
            progressBar1.Maximum = 100;
            progressBar1.Value = 0;
            numericUpDownPixelsIsSpace.Value = 11; // vobSubOcrSettings.XOrMorePixelsMakesSpace;
            _vobSubOcrSettings = vobSubOcrSettings;

            InitializeModi();
            InitializeTesseract();
            LoadImageCompareCharacterDatabaseList();

            if (Configuration.Settings.VobSubOcr.LastOcrMethod == "BitmapCompare" && comboBoxOcrMethod.Items.Count > 1)
                comboBoxOcrMethod.SelectedIndex = 1;
            else if (Configuration.Settings.VobSubOcr.LastOcrMethod == "MODI" && comboBoxOcrMethod.Items.Count > 2)
                comboBoxOcrMethod.SelectedIndex = 2;
            else
                comboBoxOcrMethod.SelectedIndex = 0;

            _bluRaySubtitlesOriginal = subtitles;

            groupBoxImagePalette.Visible = false;

            Text = Configuration.Settings.Language.VobSubOcr.TitleBluRay;
        }



        private void LoadImageCompareCharacterDatabaseList()
        {
            try
            {
                string characterDatabasePath = Configuration.VobSubCompareFolder.TrimEnd(Path.DirectorySeparatorChar);
                if (!Directory.Exists(characterDatabasePath))
                    Directory.CreateDirectory(characterDatabasePath);

                comboBoxCharacterDatabase.Items.Clear();

                foreach (string dir in Directory.GetDirectories(characterDatabasePath))
                    comboBoxCharacterDatabase.Items.Add(Path.GetFileName(dir));

                if (comboBoxCharacterDatabase.Items.Count == 0)
                {
                    Directory.CreateDirectory(characterDatabasePath + Path.DirectorySeparatorChar + _vobSubOcrSettings.LastImageCompareFolder);
                    comboBoxCharacterDatabase.Items.Add(_vobSubOcrSettings.LastImageCompareFolder);
                }

                for (int i = 0; i < comboBoxCharacterDatabase.Items.Count; i++)
                {
                    if (string.Compare(comboBoxCharacterDatabase.Items[i].ToString(), _vobSubOcrSettings.LastImageCompareFolder, true) == 0)
                        comboBoxCharacterDatabase.SelectedIndex = i;
                }
                if (comboBoxCharacterDatabase.SelectedIndex < 0)
                    comboBoxCharacterDatabase.SelectedIndex = 0;

            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format(Configuration.Settings.Language.VobSubOcr.UnableToCreateCharacterDatabaseFolder, ex.Message));
            }            
        }

        private void LoadImageCompareBitmaps()
        {
            _compareBitmaps = new List<CompareItem>();
            string path = Configuration.VobSubCompareFolder + comboBoxCharacterDatabase.SelectedItem + Path.DirectorySeparatorChar;
            if (!File.Exists(path + "CompareDescription.xml"))
                _compareDoc.LoadXml("<OcrBitmaps></OcrBitmaps>");
            else
                _compareDoc.Load(path + "CompareDescription.xml");

            foreach (string bmpFileName in Directory.GetFiles(path, "*.bmp"))
            {
                string name = Path.GetFileNameWithoutExtension(bmpFileName);

                XmlNode node = _compareDoc.DocumentElement.SelectSingleNode("FileName[.='" + name + "']");
                if (node != null)
                {                    
                    bool isItalic = node.Attributes["Italic"] != null;
                    int expandCount = 0;
                    if (node.Attributes["Expand"] != null)
                    {
                        if (!int.TryParse(node.Attributes["Expand"].InnerText, out expandCount))
                            expandCount = 0;
                    }

                    _compareBitmaps.Add(new CompareItem(new Bitmap(bmpFileName), name, isItalic, expandCount));
                }
                else
                {
                    try
                    {
                        File.Delete(bmpFileName);
                    }
                    catch
                    {
                    }
                }
            }
        }

        private bool InitializeSubIdx(string vobSubFileName)
        {
            VobSubParser vobSubParser = new VobSubParser(true);
            string idxFileName = Path.ChangeExtension(vobSubFileName, ".idx");
            vobSubParser.OpenSubIdx(vobSubFileName, idxFileName);
            _vobSubMergedPackist = vobSubParser.MergeVobSubPacks();
            _palette = vobSubParser.IdxPalette;

            List<int> languageStreamIds = new List<int>();
            foreach (var pack in _vobSubMergedPackist)
            {
                if (pack.SubPicture.Delay.TotalMilliseconds > 500 && !languageStreamIds.Contains(pack.StreamId))
                    languageStreamIds.Add(pack.StreamId);
            }
            if (languageStreamIds.Count > 1)
            {
                DvdSubRipChooseLanguage ChooseLanguage = new DvdSubRipChooseLanguage();
                ChooseLanguage.Initialize(_vobSubMergedPackist, _palette, vobSubParser.IdxLanguages, string.Empty);
                if (ChooseLanguage.ShowDialog(this) == System.Windows.Forms.DialogResult.OK)
                {
                    _vobSubMergedPackist = ChooseLanguage.SelectedVobSubMergedPacks;

                    SetTesseractLanguageFromLanguageString(ChooseLanguage.SelectedLanguageString);

                }
                else
                {
                    return false;
                }
            }
            return true;
        }

        private void SetTesseractLanguageFromLanguageString(string languageString)
        {
            // try to match language from vob to tesseract language
            if (comboBoxTesseractLanguages.SelectedIndex >= 0 && comboBoxTesseractLanguages.Items.Count > 1 && languageString != null)
            {
                languageString = languageString.ToLower();
                for (int i = 0; i < comboBoxTesseractLanguages.Items.Count; i++)
                {
                    TesseractLanguage tl = (comboBoxTesseractLanguages.Items[i] as TesseractLanguage);
                    if (tl.Text.StartsWith("Chinese") && (languageString.StartsWith("chinese") || languageString.StartsWith("中文")))
                    {
                        comboBoxTesseractLanguages.SelectedIndex = i;
                        break;
                    }
                    if (tl.Text.StartsWith("Korean") && (languageString.StartsWith("korean") || languageString.StartsWith("한국어")))
                    {
                        comboBoxTesseractLanguages.SelectedIndex = i;
                        break;
                    }
                    else if (tl.Text.StartsWith("Swedish") && languageString.StartsWith("svenska"))
                    {
                        comboBoxTesseractLanguages.SelectedIndex = i;
                        break;
                    }
                    else if (tl.Text.StartsWith("Norwegian") && languageString.StartsWith("norsk"))
                    {
                        comboBoxTesseractLanguages.SelectedIndex = i;
                        break;
                    }
                    else if (tl.Text.StartsWith("Dutch") && languageString.StartsWith("Nederlands"))
                    {
                        comboBoxTesseractLanguages.SelectedIndex = i;
                        break;
                    }
                    else if (tl.Text.StartsWith("Danish") && languageString.StartsWith("dansk"))
                    {
                        comboBoxTesseractLanguages.SelectedIndex = i;
                        break;
                    }
                    else if (tl.Text.StartsWith("English") && languageString.StartsWith("english"))
                    {
                        comboBoxTesseractLanguages.SelectedIndex = i;
                        break;
                    }
                    else if (tl.Text.StartsWith("French") && (languageString.StartsWith("french") || languageString.StartsWith("français")))
                    {
                        comboBoxTesseractLanguages.SelectedIndex = i;
                        break;
                    }
                    else if (tl.Text.StartsWith("Spannish") && (languageString.StartsWith("spannish") || languageString.StartsWith("españo")))
                    {
                        comboBoxTesseractLanguages.SelectedIndex = i;
                        break;
                    }
                    else if (tl.Text.StartsWith("Finnish") && languageString.StartsWith("suomi"))
                    {
                        comboBoxTesseractLanguages.SelectedIndex = i;
                        break;
                    }
                    else if (tl.Text.StartsWith("Italian") && languageString.StartsWith("itali"))
                    {
                        comboBoxTesseractLanguages.SelectedIndex = i;
                        break;
                    }
                    else if (tl.Text.StartsWith("German") && languageString.StartsWith("deutsch"))
                    {
                        comboBoxTesseractLanguages.SelectedIndex = i;
                        break;
                    }
                    else if (tl.Text.StartsWith("Portuguese") && languageString.StartsWith("português"))
                    {
                        comboBoxTesseractLanguages.SelectedIndex = i;
                        break;
                    }
                }
            }
        }

        private void LoadBluRaySup()
        {
            _subtitle = new Subtitle();

            _bluRaySubtitles = new List<Logic.BluRaySup.BluRaySupPicture>();
            int max = _bluRaySubtitlesOriginal.Count;
            for (int i = 0; i < max; i++)
            {
                var x = _bluRaySubtitlesOriginal[i];
                if ((checkBoxShowOnlyForced.Checked && x.IsForced) ||
                    checkBoxShowOnlyForced.Checked == false)
                {
                    _bluRaySubtitles.Add(x);
                    Paragraph p = new Paragraph();
                    p.StartTime = new TimeCode(TimeSpan.FromMilliseconds((x.StartTime + 45) / 90.0));
                    p.EndTime = new TimeCode(TimeSpan.FromMilliseconds((x.EndTime + 45) / 90.0));
                    _subtitle.Paragraphs.Add(p);
                }
            }
            _subtitle.Renumber(1);

            FixShortDisplayTimes(_subtitle);

            subtitleListView1.Fill(_subtitle);
            subtitleListView1.SelectIndexAndEnsureVisible(0);

            numericUpDownStartNumber.Maximum = max;
            if (numericUpDownStartNumber.Maximum > 0 && numericUpDownStartNumber.Minimum <= 1)
                numericUpDownStartNumber.Value = 1;

            buttonOK.Enabled = true;
            buttonCancel.Enabled = true;
            buttonStartOcr.Enabled = true;
            buttonStop.Enabled = false;
            buttonNewCharacterDatabase.Enabled = true;
            buttonEditCharacterDatabase.Enabled = true;
            buttonStartOcr.Focus();
        }

        private void LoadVobRip()
        {
            _subtitle = new Subtitle();
            _vobSubMergedPackist = new List<VobSubMergedPack>();
            int max = _vobSubMergedPackistOriginal.Count;
            for (int i = 0; i < max; i++)
            {
                var x = _vobSubMergedPackistOriginal[i];
                if ((checkBoxShowOnlyForced.Checked && x.SubPicture.Forced) ||
                    checkBoxShowOnlyForced.Checked == false)
                {
                    _vobSubMergedPackist.Add(x);
                    Paragraph p = new Paragraph(string.Empty, x.StartTime.TotalMilliseconds, x.EndTime.TotalMilliseconds);
                    if (checkBoxUseTimeCodesFromIdx.Checked && x.IdxLine != null)
                    {
                        double durationMilliseconds = p.Duration.TotalMilliseconds;
                        p.StartTime = new TimeCode(TimeSpan.FromMilliseconds(x.IdxLine.StartTime.TotalMilliseconds));
                        p.EndTime = new TimeCode(TimeSpan.FromMilliseconds(x.IdxLine.StartTime.TotalMilliseconds + durationMilliseconds));
                    }
                    _subtitle.Paragraphs.Add(p);
                }
            }
            _subtitle.Renumber(1);

            FixShortDisplayTimes(_subtitle);

            subtitleListView1.Fill(_subtitle);
            subtitleListView1.SelectIndexAndEnsureVisible(0);

            numericUpDownStartNumber.Maximum = max;
            if (numericUpDownStartNumber.Maximum > 0 && numericUpDownStartNumber.Minimum <= 1)
                numericUpDownStartNumber.Value = 1;

            buttonOK.Enabled = true;
            buttonCancel.Enabled = true;
            buttonStartOcr.Enabled = true;
            buttonStop.Enabled = false;
            buttonNewCharacterDatabase.Enabled = true;
            buttonEditCharacterDatabase.Enabled = true;
            buttonStartOcr.Focus();
        }

        public void FixShortDisplayTimes(Subtitle subtitle)
        {
            for (int i = 0; i < subtitle.Paragraphs.Count; i++)
            {
                Paragraph p = _subtitle.Paragraphs[i];
                if (p.EndTime.TotalMilliseconds <= p.StartTime.TotalMilliseconds)
                {
                    Paragraph next = _subtitle.GetParagraphOrDefault(i + 1);
                    double newEndTime = p.StartTime.TotalMilliseconds + 2400;
                    if (next == null || (newEndTime < next.StartTime.TotalMilliseconds))
                        p.EndTime.TotalMilliseconds = newEndTime;
                    else if (next != null)
                        p.EndTime.TotalMilliseconds = next.StartTime.TotalMilliseconds -1;
                }
            }
        }

        private Bitmap GetSubtitleBitmap(int index)
        {
            if (_bluRaySubtitlesOriginal != null)
            {
                if (_bluRaySubtitles[index].Palettes.Count == 0 && _defaultPaletteInfo == null)
                {
                    for (int i = 0; i < _bluRaySubtitlesOriginal.Count; i++)
                    {
                        if (_bluRaySubtitlesOriginal[i].Palettes.Count > 0)
                        {
                            _defaultPaletteInfo = _bluRaySubtitlesOriginal[i].DecodePalette(null);
                        }
                    }
                }
                return _bluRaySubtitles[index].DecodeImage(_defaultPaletteInfo);
            }
            else if (checkBoxCustomFourColors.Checked)
            {
                Color pattern = pictureBoxPattern.BackColor;
                Color emphasis1 = pictureBoxEmphasis1.BackColor;
                Color emphasis2 = pictureBoxEmphasis2.BackColor;

                if (checkBoxPatternTransparent.Checked)
                    pattern = Color.Transparent;
                if (checkBoxEmphasis1Transparent.Checked)
                    emphasis1 = Color.Transparent;
                if (checkBoxEmphasis2Transparent.Checked)
                    emphasis2 = Color.Transparent;

                return _vobSubMergedPackist[index].SubPicture.GetBitmap(null, Color.Transparent, pattern, emphasis1, emphasis2);
            }
            return _vobSubMergedPackist[index].SubPicture.GetBitmap(_palette, Color.Transparent, Color.Black, Color.White, Color.Black);
        }

        private long GetSubtitleStartTimeMilliseconds(int index)
        {
            if (_bluRaySubtitlesOriginal != null)
                return (_bluRaySubtitles[index].StartTime + 45) / 90;
            else 
                return (long)_vobSubMergedPackist[index].StartTime.TotalMilliseconds;
        }      
      
        private long GetSubtitleEndTimeMilliseconds(int index)
        {
            if (_bluRaySubtitlesOriginal != null)
                return (_bluRaySubtitles[index].EndTime + 45) / 90;
            else
                return (long)_vobSubMergedPackist[index].EndTime.TotalMilliseconds;
        }

        private int GetSubtitleCount()
        {
            if (_bluRaySubtitlesOriginal != null)
                return _bluRaySubtitles.Count;
            else
                return _vobSubMergedPackist.Count;
        }

        private void ShowSubtitleImage(int index)
        {
            int numberOfImages = GetSubtitleCount();

            if (index < numberOfImages)
            {
                groupBoxSubtitleImage.Text = string.Format(Configuration.Settings.Language.VobSubOcr.SubtitleImageXofY, index + 1, numberOfImages);
                pictureBoxSubtitleImage.Image = GetSubtitleBitmap(index); 
                pictureBoxSubtitleImage.Refresh();
            }
            else
            {
                groupBoxSubtitleImage.Text = Configuration.Settings.Language.VobSubOcr.SubtitleImage;
                pictureBoxSubtitleImage.Image = new Bitmap(1, 1);
            }
        }

        private CompareMatch GetCompareMatch(ImageSplitterItem targetItem, Bitmap parentBitmap)
        {
            int index = 0;
            int smallestDifference = 10000;
            int smallestIndex = -1;
            Bitmap target = targetItem.Bitmap;
            foreach (CompareItem compareItem in _compareBitmaps)
            {
                // check for expand match!
                if (compareItem.ExpandCount > 0)
                {
                    int dif = ImageSplitter.IsBitmapsAlike(compareItem.Bitmap, ImageSplitter.Copy(parentBitmap, new Rectangle(targetItem.X, targetItem.Y, compareItem.Bitmap.Width, compareItem.Bitmap.Height)));
                    if (dif < smallestDifference)
                    {
                        smallestDifference = dif;
                        smallestIndex = index;
                        if (dif == 0)
                            break; // foreach ending
                    }
                }
                index++;
            }

            if (smallestDifference > 0)
            {
                index = 0;
                foreach (CompareItem compareItem in _compareBitmaps)
                {
                    if (compareItem.Bitmap.Width == target.Width && compareItem.Bitmap.Height == target.Height)
                    {
                        int dif = ImageSplitter.IsBitmapsAlike(compareItem.Bitmap, target);
                        if (dif < smallestDifference)
                        {
                            smallestDifference = dif;
                            smallestIndex = index;
                            if (dif == 0)
                                break; // foreach ending
                        }
                    }
                    index++;
                }
            }

            if (smallestIndex >= 0)
            {
                double differencePercentage = smallestDifference * 100.0 / (target.Width * target.Height);
                double maxDiff= _vobSubOcrSettings.AllowDifferenceInPercent; // should be around 1.0 for vob/sub...
                if (_bluRaySubtitlesOriginal != null)
                    maxDiff = 14; // let bluray sup have a 14% diff
                if (differencePercentage < maxDiff) //_vobSubOcrSettings.AllowDifferenceInPercent) // should be around 1.0...
                {
                    XmlNode node = _compareDoc.DocumentElement.SelectSingleNode("FileName[.='" + _compareBitmaps[smallestIndex].Name + "']");
                    if (node != null)
                    {
                        bool isItalic = node.Attributes["Italic"] != null;

                        int expandCount = 0;
                        if (node.Attributes["Expand"] != null)
                        {
                            if (!int.TryParse(node.Attributes["Expand"].InnerText, out expandCount))
                                expandCount = 0;
                        }
                        return new CompareMatch(node.Attributes["Text"].InnerText, isItalic, expandCount);
                    }
                }
            }
            return null;
        }

        private void SaveCompareItem(Bitmap newTarget, string text, bool isItalic, int expandCount)
        {
            string path = Configuration.VobSubCompareFolder + comboBoxCharacterDatabase.SelectedItem + Path.DirectorySeparatorChar;
            string name = Guid.NewGuid().ToString();
            string fileName = path + name + ".bmp";
            newTarget.Save(fileName);

            _compareBitmaps.Add(new CompareItem(newTarget, name, isItalic, expandCount));


            XmlElement element = _compareDoc.CreateElement("FileName");
            XmlAttribute attribute = _compareDoc.CreateAttribute("Text");
            attribute.InnerText = text;
            element.Attributes.Append(attribute);
            if (expandCount > 0)
            {
                XmlAttribute expandSelection = _compareDoc.CreateAttribute("Expand");
                expandSelection.InnerText = expandCount.ToString();
                element.Attributes.Append(expandSelection);
            }
            if (isItalic)
            {
                XmlAttribute italic = _compareDoc.CreateAttribute("Italic");
                italic.InnerText = "true";
                element.Attributes.Append(italic);
            }
            element.InnerText = name;
            _compareDoc.DocumentElement.AppendChild(element);
            _compareDoc.Save(path + "CompareDescription.xml");
        }

        private string SplitAndOcrBitmapNormal(Bitmap bitmap)
        {
            var matches = new List<CompareMatch>();
            List<ImageSplitterItem> list = ImageSplitter.SplitBitmapToLetters(bitmap, (int)numericUpDownPixelsIsSpace.Value);

            if (checkBoxRightToLeft.Checked)
                list.Reverse();

            int index = 0;
            bool expandSelection = false;
            bool shrinkSelection = false;
            var expandSelectionList = new List<ImageSplitterItem>();
            while (index < list.Count)
            {                
                ImageSplitterItem item = list[index];
                if (expandSelection || shrinkSelection)
                {
                    expandSelection = false;
                    if (shrinkSelection && index > 0)
                    {
                        shrinkSelection = false;
                    }
                    else if (index+1 < list.Count && list[index+1].Bitmap != null) // only allow expand to EndOfLine or space
                    {
                        index++; 
                        expandSelectionList.Add(list[index]);
                    }
                    item = GetExpandedSelection(bitmap, expandSelectionList);

                    var vobSubOcrCharacter = new VobSubOcrCharacter();
                    vobSubOcrCharacter.Initialize(bitmap, item, _manualOcrDialogPosition, _italicCheckedLast, expandSelectionList.Count > 1);
                    DialogResult result = vobSubOcrCharacter.ShowDialog(this);
                    if (result == DialogResult.OK && vobSubOcrCharacter.ShrinkSelection)
                    {
                        shrinkSelection = true;
                        index--;
                        if (expandSelectionList.Count > 0)
                            expandSelectionList.RemoveAt(expandSelectionList.Count - 1);
                    }
                    else if (result == DialogResult.OK && vobSubOcrCharacter.ExpandSelection)
                    {
                        expandSelection = true;
                    }
                    else if (result == DialogResult.OK)
                    {
                        string text = vobSubOcrCharacter.ManualRecognizedCharacters;
                        SaveCompareItem(item.Bitmap, text, vobSubOcrCharacter.IsItalic, expandSelectionList.Count);
                        matches.Add(new CompareMatch(text, vobSubOcrCharacter.IsItalic, expandSelectionList.Count));
                        expandSelectionList = new List<ImageSplitterItem>();
                    }
                    else if (result == DialogResult.Abort)
                    {
                        _abort = true;
                    }
                    else
                    {
                        matches.Add(new CompareMatch("*", false, 0));
                    }
                    _manualOcrDialogPosition = vobSubOcrCharacter.FormPosition;
                    _italicCheckedLast = vobSubOcrCharacter.IsItalic;

                }
                else if (item.Bitmap == null)
                {
                    matches.Add(new CompareMatch(item.SpecialCharacter, false, 0));
                }
                else
                {
                    CompareMatch match = GetCompareMatch(item, bitmap);
                    if (match == null)
                    {
                        var vobSubOcrCharacter = new VobSubOcrCharacter();
                        vobSubOcrCharacter.Initialize(bitmap, item, _manualOcrDialogPosition, _italicCheckedLast, false);
                        DialogResult result = vobSubOcrCharacter.ShowDialog(this);
                        if (result == DialogResult.OK && vobSubOcrCharacter.ExpandSelection)
                        {
                            expandSelectionList.Add(item);
                            expandSelection = true;
                        }
                        else if (result == DialogResult.OK)
                        {
                            string text = vobSubOcrCharacter.ManualRecognizedCharacters;
                            SaveCompareItem(item.Bitmap, text, vobSubOcrCharacter.IsItalic, 0);
                            matches.Add(new CompareMatch(text, vobSubOcrCharacter.IsItalic, 0));
                        }
                        else if (result == DialogResult.Abort)
                        {
                            _abort = true;
                        }
                        else
                        {
                            matches.Add(new CompareMatch("*", false, 0));
                        }
                        _manualOcrDialogPosition = vobSubOcrCharacter.FormPosition;
                        _italicCheckedLast = vobSubOcrCharacter.IsItalic;
                    }
                    else // found image match
                    {
                        matches.Add(new CompareMatch(match.Text, match.Italic, 0));
                        if (match.ExpandCount > 0)
                            index += match.ExpandCount - 1;
                    }
                }
                if (_abort)
                    return string.Empty;
                if (!expandSelection && ! shrinkSelection)
                    index++;
                if (shrinkSelection && expandSelectionList.Count < 2)
                {
                    //index--;
                    shrinkSelection = false;
                    expandSelectionList = new List<ImageSplitterItem>();
                }
            }
            string line = GetStringWithItalicTags(matches);
            if (checkBoxAutoFixCommonErrors.Checked)
                line = OcrFixEngine.FixOcrErrorsViaHardcodedRules(line, _lastLine, null); // TODO: add abbreviations list
            return line;
        }

        private static ImageSplitterItem GetExpandedSelection(Bitmap bitmap, List<ImageSplitterItem> expandSelectionList)
        {
            int minimumX = expandSelectionList[0].X;
            int maximumX = expandSelectionList[expandSelectionList.Count - 1].X + expandSelectionList[expandSelectionList.Count - 1].Bitmap.Width;
            int minimumY = expandSelectionList[0].Y;
            int maximumY = expandSelectionList[0].Y + expandSelectionList[0].Bitmap.Height;
            foreach (ImageSplitterItem item in expandSelectionList)
            {
                if (item.Y < minimumY)
                    minimumY = item.Y;
                if (item.Y + item.Bitmap.Height > maximumY)
                    maximumY = item.Y + item.Bitmap.Height;
            }
            Bitmap part = ImageSplitter.Copy(bitmap, new Rectangle(minimumX, minimumY, maximumX - minimumX, maximumY - minimumY));
            return new ImageSplitterItem(minimumX, minimumY, part);
        }

        private static string GetStringWithItalicTags(List<CompareMatch> matches)
        {
            StringBuilder paragraph = new StringBuilder();
            StringBuilder line = new StringBuilder();
            StringBuilder word = new StringBuilder();
            int lettersItalics = 0;
            int lettersNonItalics = 0;
            int wordItalics = 0;
            int wordNonItalics = 0;
            bool isItalic = false;
            bool allItalic = true;
            for (int i = 0; i < matches.Count; i++)
            {
                if (matches[i].Text == " ")
                {
                    ItalicsWord(line, ref word, ref lettersItalics, ref lettersNonItalics, ref wordItalics, ref wordNonItalics, ref isItalic, " ");
                }
                else if (matches[i].Text == Environment.NewLine)
                {
                    ItalicsWord(line, ref word, ref lettersItalics, ref lettersNonItalics, ref wordItalics, ref wordNonItalics, ref isItalic, "");
                    ItalianLine(paragraph, ref line, ref allItalic, ref wordItalics, ref wordNonItalics, ref isItalic, Environment.NewLine);
                }
                else if (matches[i].Italic)
                {
                    word.Append(matches[i].Text);
                    lettersItalics += matches[i].Text.Length;
                }
                else
                {
                    word.Append(matches[i].Text);
                    lettersNonItalics += matches[i].Text.Length;
                }
            }

            if (word.Length > 0)
                ItalicsWord(line, ref word, ref lettersItalics, ref lettersNonItalics, ref wordItalics, ref wordNonItalics, ref isItalic, "");
            if (line.Length > 0)
                ItalianLine(paragraph, ref line, ref allItalic, ref wordItalics, ref wordNonItalics, ref isItalic, "");

            if (allItalic)
            {
                string temp = paragraph.ToString().Replace("<i>", "").Replace("</i>", "");
                paragraph = new StringBuilder();
                paragraph.Append("<i>" + temp + "</i>");

            }

            return paragraph.ToString();
        }

        private static void ItalianLine(StringBuilder paragraph, ref StringBuilder line, ref bool allItalic, ref int wordItalics, ref int wordNonItalics, ref bool isItalic, string appendString)
        {
            if (isItalic)
            {
                line.Append("</i>");
                isItalic = false;
            }

            if (wordItalics > 0 && wordNonItalics == 0)
            {
                string temp = line.ToString().Replace("<i>", "").Replace("</i>", "");
                paragraph.Append("<i>" + temp + "</i>");
                paragraph.Append(appendString);
            }
            else if (wordItalics > 0 && wordNonItalics == 1 && line.ToString().Trim().StartsWith("-"))
            {
                string temp = line.ToString().Replace("<i>", "").Replace("</i>", "");
                paragraph.Append("<i>" + temp + "</i>");
                paragraph.Append(appendString);
            }
            else
            {
                allItalic = false;

                if (wordItalics > 0)
                { 
                    string temp = line.ToString().Replace(" </i>", "</i> ");
                    line = new StringBuilder();
                    line.Append(temp);
                }

                paragraph.Append(line.ToString());
                paragraph.Append(appendString);
            }
            line = new StringBuilder();
            wordItalics = 0;
            wordNonItalics = 0;
        }

        private static void ItalicsWord(StringBuilder line, ref StringBuilder word, ref int lettersItalics, ref int lettersNonItalics, ref int wordItalics, ref int wordNonItalics, ref bool isItalic, string appendString)
        {
            if (lettersItalics >= lettersNonItalics)
            {
                if (!isItalic)
                    line.Append("<i>");
                line.Append(word + appendString);
                wordItalics++;
                isItalic = true;
            }
            else
            {
                line.Append(word.ToString());
                if (isItalic)
                {
                    line.Append("</i>");
                    isItalic = false;
                }
                line.Append(appendString);
                wordNonItalics++;
            }
            word = new StringBuilder();
            lettersItalics = 0;
            lettersNonItalics = 0;
        }

        public Subtitle SubtitleFromOcr
        {
            get
            {
                return _subtitle;
            }
        }

        private void FormVobSubOcr_Shown(object sender, EventArgs e)
        {
            if (_bluRaySubtitlesOriginal != null)
            {
                LoadBluRaySup();
                bool hasForcedSubtitles = false;
                foreach (var x in _bluRaySubtitlesOriginal)
                {
                    if (x.IsForced)
                    {
                        hasForcedSubtitles = true;
                        break;
                    }
                }
                checkBoxShowOnlyForced.Enabled = hasForcedSubtitles;
                checkBoxUseTimeCodesFromIdx.Visible = false;
            }
            else
            {
                _vobSubMergedPackistOriginal = new List<VobSubMergedPack>();
                bool hasIdxTimeCodes = false;
                bool hasForcedSubtitles = false;
                foreach (var x in _vobSubMergedPackist)
                {
                    _vobSubMergedPackistOriginal.Add(x);
                    if (x.IdxLine != null)
                        hasIdxTimeCodes = true;
                    if (x.SubPicture.Forced)
                        hasForcedSubtitles = true;
                }
                checkBoxUseTimeCodesFromIdx.CheckedChanged -= checkBoxUseTimeCodesFromIdx_CheckedChanged;
                checkBoxUseTimeCodesFromIdx.Visible = hasIdxTimeCodes;
                checkBoxUseTimeCodesFromIdx.Checked = hasIdxTimeCodes;
                checkBoxUseTimeCodesFromIdx.CheckedChanged += checkBoxUseTimeCodesFromIdx_CheckedChanged;
                checkBoxShowOnlyForced.Enabled = hasForcedSubtitles;
                LoadVobRip();
            }
        }

        private void ButtonOkClick(object sender, EventArgs e)
        {
            if (Configuration.Settings.VobSubOcr.XOrMorePixelsMakesSpace != (int)numericUpDownPixelsIsSpace.Value && _bluRaySubtitlesOriginal == null)
            {
                Configuration.Settings.VobSubOcr.XOrMorePixelsMakesSpace = (int)numericUpDownPixelsIsSpace.Value;    
                Configuration.Settings.Save();
            }
            DialogResult = DialogResult.OK;
        }

        private void SetButtonsEnabledAfterOcrDone()
        {
            buttonOK.Enabled = true;
            buttonCancel.Enabled = true;
            buttonStartOcr.Enabled = true;
            buttonStop.Enabled = false;
            buttonNewCharacterDatabase.Enabled = true;
            buttonEditCharacterDatabase.Enabled = true;

            labelStatus.Text = string.Empty;
            progressBar1.Visible = false;
        }

        private void ButtonStartOcrClick(object sender, EventArgs e)
        {
            _lastLine = null;
            buttonOK.Enabled = false;
            buttonCancel.Enabled = false;
            buttonStartOcr.Enabled = false;
            buttonStop.Enabled = true;
            buttonNewCharacterDatabase.Enabled = false;
            buttonEditCharacterDatabase.Enabled = false;

            _abort = false;

            int max = GetSubtitleCount();

            progressBar1.Maximum = max;
            progressBar1.Value = 0;
            progressBar1.Visible = true;
            for (int i = (int)numericUpDownStartNumber.Value-1; i < max; i++)
            {
                ShowSubtitleImage(i);

                var startTime = new TimeCode(TimeSpan.FromMilliseconds(GetSubtitleStartTimeMilliseconds(i)));
                var endTime = new TimeCode(TimeSpan.FromMilliseconds(GetSubtitleEndTimeMilliseconds(i)));
                labelStatus.Text = string.Format("{0} / {1}: {2} - {3}", i + 1, max, startTime, endTime);
                progressBar1.Value = i + 1;
                labelStatus.Refresh();
                progressBar1.Refresh();
                Application.DoEvents();
                if (_abort)
                {
                    SetButtonsEnabledAfterOcrDone();
                    return;
                }

                subtitleListView1.SelectIndexAndEnsureVisible(i);
                string text;
                if (comboBoxOcrMethod.SelectedIndex == 0)
                    text = OcrViaTessnet(GetSubtitleBitmap(i), i);
                else if (comboBoxOcrMethod.SelectedIndex == 1)
                    text = SplitAndOcrBitmapNormal(GetSubtitleBitmap(i));
                else 
                    text = CallModi(i);

                _lastLine = text;

                // max allow 2 lines
                if (checkBoxAutoBreakLines.Checked && text.Replace(Environment.NewLine, "*").Length + 2 <= text.Length)
                    text = Utilities.AutoBreakLine(text);               

                Application.DoEvents();
                if (_abort)
                {
                    textBoxCurrentText.Text = text;
                    SetButtonsEnabledAfterOcrDone();
                    return;
                }

                text = text.Trim();
                text = text.Replace("  ", " ");
                text = text.Replace(Environment.NewLine + Environment.NewLine, Environment.NewLine);
                text = text.Replace("  ", " ");
                text = text.Replace(Environment.NewLine + Environment.NewLine, Environment.NewLine);

                Paragraph p = _subtitle.GetParagraphOrDefault(i);
                if (p != null)
                    p.Text = text;
                textBoxCurrentText.Text = text;
            }            
            SetButtonsEnabledAfterOcrDone();
        }

        private Bitmap ResizeBitmap(Bitmap b, int width, int height)
        {
            var result = new Bitmap(width, height);
            using (Graphics g = Graphics.FromImage(result))
                g.DrawImage(b, 0, 0, width, height);
            return result;
        }

        //public void UnItalic(Bitmap bmp)
        //{
        //    double xOffset = 0;
        //    for (int y = 0; y < bmp.Height; y++)
        //    {
        //        int offset = (int)xOffset;
        //        for (int x = bmp.Width - 1; x >= 0; x--)
        //        {
        //            if (x - offset >= 0)
        //                bmp.SetPixel(x, y, bmp.GetPixel(x - offset, y));
        //            else
        //                bmp.SetPixel(x, y, Color.Transparent);
        //        }
        //        //                xOffset += 0.3;
        //        xOffset += 0.05;
        //    }
        //}

        //public void UnItalic(Bitmap bmp, double factor)
        //{
        //    int left = (int)(bmp.Height*factor);
        //    Bitmap unItaliced = new Bitmap(bmp.Width + left, bmp.Height);
        //    double xOffset = 0;
        //    for (int y = 0; y < bmp.Height; y++)
        //    {
        //        int offset = (int)xOffset;
        //        for (int x = bmp.Width - 1; x >= 0; x--)
        //        {
        //            if (x - offset >= 0)
        //                unItaliced.SetPixel(x, y, bmp.GetPixel(x - offset, y));
        //            else
        //                unItaliced.SetPixel(x, y, Color.Transparent);
        //        }
        //        //                xOffset += 0.3;
        //        xOffset += 0.05;
        //    }
        //}

        private string Tesseract3DoOcrViaExe(Bitmap bmp, string language)
        {
            string tempTiffFileName = Path.GetTempPath() + Guid.NewGuid().ToString() + ".tiff";
            bmp.Save(tempTiffFileName, System.Drawing.Imaging.ImageFormat.Tiff);
            string tempTextFileName = Path.GetTempPath() + Guid.NewGuid().ToString();

            Process process = new Process();
            process.StartInfo = new ProcessStartInfo(Configuration.TesseractFolder + "tesseract.exe");
            process.StartInfo.Arguments = "\"" + tempTiffFileName + "\" \"" + tempTextFileName + "\" -l " + language;
            process.StartInfo.WorkingDirectory = (Configuration.TesseractFolder);
            process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            process.Start();
            process.WaitForExit(5000);

            string outputFileName = tempTextFileName + ".txt";
            string result = string.Empty;
            try
            {
                if (File.Exists(outputFileName))
                {
                    result = File.ReadAllText(outputFileName);
                    File.Delete(tempTextFileName + ".txt");
                }
                File.Delete(tempTiffFileName);
            }
            catch
            {
            }
            return result;
        }

        private string OcrViaTessnet(Bitmap bitmap, int index)
        {
            if (_ocrFixEngine == null)
            {
                _languageId = (comboBoxTesseractLanguages.SelectedItem as TesseractLanguage).Id;
                _ocrFixEngine = new OcrFixEngine(_languageId, this);
                if (_ocrFixEngine.IsDictionaryLoaded)
                    labelDictionaryLoaded.Text = string.Format(Configuration.Settings.Language.VobSubOcr.DictionaryX, _ocrFixEngine.DictionaryCulture.NativeName);
                else
                    labelDictionaryLoaded.Text = string.Format(Configuration.Settings.Language.VobSubOcr.DictionaryX, Configuration.Settings.Language.General.None);

                if (_modiEnabled && checkBoxUseModiInTesseractForUnknownWords.Checked)
                {
                    string tesseractLanguageText = (comboBoxTesseractLanguages.SelectedItem as TesseractLanguage).Text;
                    int i = 0;
                    foreach (var modiLanguage in comboBoxModiLanguage.Items)
                    {
                        if ((modiLanguage as ModiLanguage).Text == tesseractLanguageText)
                            comboBoxModiLanguage.SelectedIndex = i;
                        i++;
                    }
                }
                comboBoxModiLanguage.SelectedIndex = -1;
            }

            int badWords = 0;
            string textWithOutFixes = Tesseract3DoOcrViaExe(bitmap, _languageId);

            if (textWithOutFixes.ToString().Trim().Length == 0)
            {
                textWithOutFixes = TesseractResizeAndRetry(bitmap);     
            }

            int numberOfWords = textWithOutFixes.ToString().Split((" " + Environment.NewLine).ToCharArray(), StringSplitOptions.RemoveEmptyEntries).Length;

            string line = textWithOutFixes.ToString().Trim();
            if (_ocrFixEngine.IsDictionaryLoaded)
            {
                if (checkBoxAutoFixCommonErrors.Checked)
                    line = _ocrFixEngine.FixOcrErrors(line, index, _lastLine, true, checkBoxGuessUnknownWords.Checked);
                int correctWords;
                int wordsNotFound = _ocrFixEngine.CountUnknownWordsViaDictionary(line, out correctWords);

                if (wordsNotFound > 0 || correctWords == 0)
                { 
                    string newUnfixedText = TesseractResizeAndRetry(bitmap);
                    string newText = _ocrFixEngine.FixOcrErrors(newUnfixedText, index, _lastLine, true, checkBoxGuessUnknownWords.Checked);
                    int newWordsNotFound = _ocrFixEngine.CountUnknownWordsViaDictionary(newText, out correctWords);
                    if (newWordsNotFound < wordsNotFound)
                    {
                        wordsNotFound = newWordsNotFound;
                        textWithOutFixes = newUnfixedText;
                        line = newText;
                    }
                }

                if (wordsNotFound > 0 || correctWords == 0 || textWithOutFixes.ToString().Replace("~", string.Empty).Trim().Length == 0)
                {                   
                    _ocrFixEngine.AutoGuessesUsed.Clear();
                    _ocrFixEngine.UnknownWordsFound.Clear();
                    if (_modiEnabled && checkBoxUseModiInTesseractForUnknownWords.Checked)
                    {
                        // which is best - modi or tesseract - we find out here
                        string modiText = CallModi(index);

                        if (modiText.Length == 0)
                            modiText = CallModi(index); // retry... strange MODI
                        if (modiText.Length == 0)
                            modiText = CallModi(index); // retry... strange MODI                        

                        if (modiText.Length > 1)
                        {
                            int modiWordsNotFound = _ocrFixEngine.CountUnknownWordsViaDictionary(modiText, out correctWords);
                            if (modiWordsNotFound > 0)
                            {
                                string modiTextOcrFixed = modiText;
                                if (checkBoxAutoFixCommonErrors.Checked)
                                    modiTextOcrFixed = _ocrFixEngine.FixOcrErrors(modiText, index, _lastLine, false, checkBoxGuessUnknownWords.Checked);
                                int modiOcrCorrectedWordsNotFound = _ocrFixEngine.CountUnknownWordsViaDictionary(modiTextOcrFixed, out correctWords);
                                if (modiOcrCorrectedWordsNotFound < modiWordsNotFound)
                                    modiText = modiTextOcrFixed;
                            }

                            if (modiWordsNotFound < wordsNotFound)
                                line = modiText; // use the modi ocr'ed text
                        }

                        // take the best option - before ocr fixing, which we do again to save suggestions and prompt for user input
                        line = _ocrFixEngine.FixUnknownWordsViaGuessOrPrompt(out wordsNotFound, line, index, bitmap, checkBoxAutoFixCommonErrors.Checked, checkBoxPromptForUnknownWords.Checked, true, checkBoxGuessUnknownWords.Checked);
                    }
                    else 
                    { // fix some error manually (modi not available)
                        line = _ocrFixEngine.FixUnknownWordsViaGuessOrPrompt(out wordsNotFound, line, index, bitmap, checkBoxAutoFixCommonErrors.Checked, checkBoxPromptForUnknownWords.Checked, true, checkBoxGuessUnknownWords.Checked);
                    }
                }

                if (_ocrFixEngine.Abort)
                {
                    ButtonStopClick(null, null);
                    _ocrFixEngine.Abort = false;
                    return string.Empty;
                }

                // Log used word guesses (via word replace list)
                foreach (string guess in _ocrFixEngine.AutoGuessesUsed)
                    listBoxLogSuggestions.Items.Add(guess);
                _ocrFixEngine.AutoGuessesUsed.Clear();

                // Log unkown words guess (found via spelling dictionaries)
                foreach (string unknownWord in _ocrFixEngine.UnknownWordsFound)
                    listBoxUnknownWords.Items.Add(unknownWord);
                _ocrFixEngine.UnknownWordsFound.Clear();

                if (wordsNotFound >= 3)
                    subtitleListView1.SetBackgroundColor(index, Color.Red);
                if (wordsNotFound == 2)
                    subtitleListView1.SetBackgroundColor(index, Color.Orange);
                else if (wordsNotFound == 1)
                    subtitleListView1.SetBackgroundColor(index, Color.Yellow);
                else if (line.Trim().Length == 0)
                    subtitleListView1.SetBackgroundColor(index, Color.Orange);
                else
                    subtitleListView1.SetBackgroundColor(index, Color.Green);
            }
            else
            { // no dictionary :(
                if (checkBoxAutoFixCommonErrors.Checked)
                    line = _ocrFixEngine.FixOcrErrors(line, index, _lastLine, true, checkBoxGuessUnknownWords.Checked);

                if (badWords >= numberOfWords) //result.Count)
                    subtitleListView1.SetBackgroundColor(index, Color.Red);
                else if (badWords >= numberOfWords / 2) // result.Count / 2)
                    subtitleListView1.SetBackgroundColor(index, Color.Orange);
                else if (badWords > 0)
                    subtitleListView1.SetBackgroundColor(index, Color.Yellow);
                else if (line.Trim().Length == 0)
                    subtitleListView1.SetBackgroundColor(index, Color.Orange);
                else
                    subtitleListView1.SetBackgroundColor(index, Color.Green);
            }
           
            if (textWithOutFixes.ToString().Trim() != line.Trim())
            {
                _tessnetOcrAutoFixes++;
                labelFixesMade.Text = string.Format(" - {0}", _tessnetOcrAutoFixes);
                LogOcrFix(index, textWithOutFixes.ToString(), line);
            }

            if (_vobSubMergedPackist != null)
                bitmap.Dispose();

            return line;
        }

        private string TesseractResizeAndRetry(Bitmap bitmap)
        {
            string result = Tesseract3DoOcrViaExe(ResizeBitmap(bitmap, bitmap.Width * 3, bitmap.Height * 2), _languageId);
            if (result.ToString().Trim().Length == 0)
                result = Tesseract3DoOcrViaExe(ResizeBitmap(bitmap, bitmap.Width * 4, bitmap.Height * 2), _languageId);
            return result.TrimEnd();
        }

        private void LogOcrFix(int index, string oldLine, string newLine)
        {
            listBoxLog.Items.Add(string.Format("#{0}: {1} -> {2}", index+1, oldLine.Replace(Environment.NewLine, " "), newLine.Replace(Environment.NewLine, " ")));
        }

        private string CallModi(int i)
        {
            var bmp = GetSubtitleBitmap(i).Clone() as Bitmap;
            var mp = new ModiParameter { Bitmap = bmp, Text = "", Language = GetModiLanguage() };

            // We call in a seperate thread... or app will crash sometimes :(
            var modiThread = new System.Threading.Thread(DoWork);
            modiThread.Start(mp);
            modiThread.Join(3000); // wait max 3 seconds     
            modiThread.Abort();

            if (!string.IsNullOrEmpty(mp.Text) && mp.Text.Length > 3 && mp.Text.EndsWith(";0]"))
                mp.Text = mp.Text.Substring(0, mp.Text.Length - 3);

            // Try to avoid blank lines by resizing image
            if (string.IsNullOrEmpty(mp.Text))
            {
                bmp = ResizeBitmap(bmp, (int)(bmp.Width * 1.2), (int)(bmp.Height * 1.2));
                mp = new ModiParameter { Bitmap = bmp, Text = "", Language = GetModiLanguage() };

                // We call in a seperate thread... or app will crash sometimes :(
                modiThread = new System.Threading.Thread(DoWork);
                modiThread.Start(mp);
                modiThread.Join(3000); // wait max 3 seconds     
                modiThread.Abort();
            }
            if (string.IsNullOrEmpty(mp.Text))
            {
                bmp = ResizeBitmap(bmp, (int)(bmp.Width * 1.3), (int)(bmp.Height * 1.4)); // a bit scaling
                mp = new ModiParameter { Bitmap = bmp, Text = "", Language = GetModiLanguage() };

                // We call in a seperate thread... or app will crash sometimes :(
                modiThread = new System.Threading.Thread(DoWork);
                modiThread.Start(mp);
                modiThread.Join(3000); // wait max 3 seconds     
                modiThread.Abort();
            }

            return mp.Text;
        }

        public static void DoWork(object data)
        {
            var paramter = (ModiParameter)data;
            string fileName = Path.GetTempPath() + Path.DirectorySeparatorChar + Guid.NewGuid() + ".bmp";
            Object ocrResult = null;
            try
            {
                paramter.Bitmap.Save(fileName);

                Type modiDocType = Type.GetTypeFromProgID("MODI.Document");
                Object modiDoc = Activator.CreateInstance(modiDocType);
                modiDocType.InvokeMember("Create", BindingFlags.InvokeMethod, null, modiDoc, new Object[] { fileName });

                modiDocType.InvokeMember("OCR", BindingFlags.InvokeMethod, null, modiDoc, new Object[] { paramter.Language, true, true });

                Object images = modiDocType.InvokeMember("Images", BindingFlags.GetProperty, null, modiDoc, new Object[] { });
                Type imagesType = images.GetType();

                Object item = imagesType.InvokeMember("Item", BindingFlags.GetProperty, null, images, new Object[] { "0" });
                Type itemType = item.GetType();

                Object layout = itemType.InvokeMember("Layout", BindingFlags.GetProperty, null, item, new Object[] { });
                Type layoutType = layout.GetType();
                ocrResult = layoutType.InvokeMember("Text", BindingFlags.GetProperty, null, layout, new Object[] { });

                modiDocType.InvokeMember("Close", BindingFlags.InvokeMethod, null, modiDoc, new Object[] { false });
            }
            catch
            {
                paramter.Text = string.Empty;
            }

            try
            {
                File.Delete(fileName);
            }
            catch
            {
            }
            if (ocrResult != null)
                paramter.Text = ocrResult.ToString().Trim();
        }

        private void InitializeModi()
        {
            _modiEnabled = false;
            checkBoxUseModiInTesseractForUnknownWords.Enabled = false;
            comboBoxModiLanguage.Enabled = false;
            try
            {
                InitializeModiLanguages();

                _modiType = Type.GetTypeFromProgID("MODI.Document");
                _modiDoc = Activator.CreateInstance(_modiType);

                _modiEnabled = _modiDoc != null;
                comboBoxModiLanguage.Enabled = _modiEnabled;
                checkBoxUseModiInTesseractForUnknownWords.Enabled = _modiEnabled;
            }
            catch
            {
                _modiEnabled = false;
            }
            if (!_modiEnabled && comboBoxOcrMethod.Items.Count == 3)
                comboBoxOcrMethod.Items.RemoveAt(2);
        }

        private void InitializeTesseract()
        {
            string dir = Configuration.TesseractFolder + "tessdata";
            if (Directory.Exists(dir))
            {
                var list = new List<string>();
                comboBoxTesseractLanguages.Items.Clear();
                foreach (var culture in System.Globalization.CultureInfo.GetCultures(System.Globalization.CultureTypes.NeutralCultures))
                {
                    string tesseractName = culture.ThreeLetterISOLanguageName;
                    if (culture.LCID == 0x4 && !File.Exists(dir + "\\" + tesseractName + ".traineddata"))
                        tesseractName = "chi_sim";
                    string trainDataFileName = dir + "\\" + tesseractName + ".traineddata";
                    if (!list.Contains(culture.ThreeLetterISOLanguageName) && File.Exists(trainDataFileName))
                    {
                        list.Add(culture.ThreeLetterISOLanguageName);
                        comboBoxTesseractLanguages.Items.Add(new TesseractLanguage { Id = tesseractName, Text = culture.EnglishName });
                    }
                }
            }
            if (comboBoxTesseractLanguages.Items.Count > 0)
            {
                for (int i = 0; i < comboBoxTesseractLanguages.Items.Count; i++)
                {
                    if ((comboBoxTesseractLanguages.Items[i] as TesseractLanguage).Id == Configuration.Settings.VobSubOcr.TesseractLastLanguage)
                        comboBoxTesseractLanguages.SelectedIndex = i;
                }

                if (comboBoxTesseractLanguages.SelectedIndex == -1)
                    comboBoxTesseractLanguages.SelectedIndex = 0;
            }
        }

        private void InitializeModiLanguages()
        {
            foreach (ModiLanguage ml in ModiLanguage.AllLanguages)
            {
                comboBoxModiLanguage.Items.Add(ml);
                if (ml.Id == _vobSubOcrSettings.LastModiLanguageId)
                    comboBoxModiLanguage.SelectedIndex = comboBoxModiLanguage.Items.Count - 1;
            }
        }

        private int GetModiLanguage()
        {
            if (comboBoxModiLanguage.SelectedIndex < 0)
                return ModiLanguage.DefaultLanguageId;

            return ((ModiLanguage)comboBoxModiLanguage.SelectedItem).Id;
        }

        private void ButtonStopClick(object sender, EventArgs e)
        {
            _abort = true;
            buttonStop.Enabled = false;
            Application.DoEvents();
            progressBar1.Visible = false;
            labelStatus.Text = string.Empty;
        }

        private void SubtitleListView1SelectedIndexChanged(object sender, EventArgs e)
        {
            if (subtitleListView1.SelectedItems.Count > 0)
            {
                _selectedIndex = subtitleListView1.SelectedItems[0].Index;                
                textBoxCurrentText.Text = _subtitle.Paragraphs[_selectedIndex].Text;
                ShowSubtitleImage(subtitleListView1.SelectedItems[0].Index);
                numericUpDownStartNumber.Value = _selectedIndex + 1;
            }
            else
            {
                _selectedIndex = -1;
                textBoxCurrentText.Text = string.Empty;
            }          
        }

        private void TextBoxCurrentTextTextChanged(object sender, EventArgs e)
        {
            if (_selectedIndex >= 0)
            {
                string text = textBoxCurrentText.Text.TrimEnd();
                _subtitle.Paragraphs[_selectedIndex].Text = text;
                subtitleListView1.SetText(_selectedIndex, text);
            }
        }

        private void ButtonNewCharacterDatabaseClick(object sender, EventArgs e)
        {
            var newFolder = new VobSubOcrNewFolder();
            if (newFolder.ShowDialog(this) == DialogResult.OK)
            {
                _vobSubOcrSettings.LastImageCompareFolder = newFolder.FolderName;
                LoadImageCompareCharacterDatabaseList();
            }
        }

        private void ComboBoxCharacterDatabaseSelectedIndexChanged(object sender, EventArgs e)
        {
            LoadImageCompareBitmaps();
            _vobSubOcrSettings.LastImageCompareFolder = comboBoxCharacterDatabase.SelectedItem.ToString();
        }

        private void ComboBoxModiLanguageSelectedIndexChanged(object sender, EventArgs e)
        {
            _vobSubOcrSettings.LastModiLanguageId = GetModiLanguage();
        }

        private void ButtonEditCharacterDatabaseClick(object sender, EventArgs e)
        {
            var formVobSubEditCharacters = new VobSubEditCharacters(comboBoxCharacterDatabase.SelectedItem.ToString());
            if (formVobSubEditCharacters.ShowDialog() == DialogResult.OK)
            {
                _compareDoc = formVobSubEditCharacters.ImageCompareDocument;
                string path = Configuration.VobSubCompareFolder + comboBoxCharacterDatabase.SelectedItem + Path.DirectorySeparatorChar;
                _compareDoc.Save(path + "CompareDescription.xml");
            }
            LoadImageCompareBitmaps();
        }

        private void VobSubOcr_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F1)
            {
                Utilities.ShowHelp("#importvobsub");
                e.SuppressKeyPress = true;
            }
            else if (e.KeyCode == Keys.Down && e.Modifiers == Keys.Alt)
            {
                int selectedIndex = 0;
                if (subtitleListView1.SelectedItems.Count > 0)
                {
                    selectedIndex = subtitleListView1.SelectedItems[0].Index;
                    selectedIndex++;
                }
                subtitleListView1.SelectIndexAndEnsureVisible(selectedIndex);
            }
            else if (e.KeyCode == Keys.Up && e.Modifiers == Keys.Alt)
            {
                int selectedIndex = 0;
                if (subtitleListView1.SelectedItems.Count > 0)
                {
                    selectedIndex = subtitleListView1.SelectedItems[0].Index;
                    selectedIndex--;
                }
                subtitleListView1.SelectIndexAndEnsureVisible(selectedIndex);
            }
            else if (e.Modifiers == Keys.Control && e.KeyCode == Keys.G)
            {
                var goToLine = new GoToLine();
                goToLine.Initialize(1, subtitleListView1.Items.Count);
                if (goToLine.ShowDialog(this) == DialogResult.OK)
                {
                    subtitleListView1.SelectNone();
                    subtitleListView1.Items[goToLine.LineNumber - 1].Selected = true;
                    subtitleListView1.Items[goToLine.LineNumber - 1].EnsureVisible();
                }
            }
        }

        private void ComboBoxTesseractLanguagesSelectedIndexChanged(object sender, EventArgs e)
        {
            Configuration.Settings.VobSubOcr.TesseractLastLanguage = (comboBoxTesseractLanguages.SelectedItem as TesseractLanguage).Id;
            _ocrFixEngine = null;
        }

        private void ComboBoxOcrMethodSelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBoxOcrMethod.SelectedIndex == 0)
            {
                ShowOcrMethodGroupBox(GroupBoxTesseractMethod);
                Configuration.Settings.VobSubOcr.LastOcrMethod = "Tesseract";
            }
            else if (comboBoxOcrMethod.SelectedIndex == 1)
            {
                ShowOcrMethodGroupBox(groupBoxImageCompareMethod);
                Configuration.Settings.VobSubOcr.LastOcrMethod = "BitmapCompare";
            }
            else if (comboBoxOcrMethod.SelectedIndex == 2)
            {
                ShowOcrMethodGroupBox(groupBoxModiMethod);
                Configuration.Settings.VobSubOcr.LastOcrMethod = "MODI";
            }
        }

        private void ShowOcrMethodGroupBox(GroupBox groupBox)
        {
            GroupBoxTesseractMethod.Visible = false;
            groupBoxImageCompareMethod.Visible = false;
            groupBoxModiMethod.Visible = false;

            groupBox.Visible = true;
            groupBox.BringToFront();
            groupBox.Left = comboBoxOcrMethod.Left;
            groupBox.Top = 50;
        }

        private void ListBoxLogSelectedIndexChanged(object sender, EventArgs e)
        {
            var lb = sender as ListBox;
            if (lb != null &&  lb.SelectedIndex >= 0)
            {
                string text = lb.Items[lb.SelectedIndex].ToString();
                if (text.Contains(":"))
                {
                    string number = text.Substring(1, text.IndexOf(":") - 1);
                    subtitleListView1.SelectIndexAndEnsureVisible(int.Parse(number)-1);
                }
            }
        }

        private void ContextMenuStripListviewOpening(object sender, CancelEventArgs e)
        {
            if (subtitleListView1.SelectedItems.Count == 0)
                e.Cancel = true;

            if (contextMenuStripListview.SourceControl == subtitleListView1)
            {
                normalToolStripMenuItem.Visible = true;
                italicToolStripMenuItem.Visible = true;
                toolStripSeparator1.Visible = true;
                toolStripSeparator1.Visible = subtitleListView1.SelectedItems.Count == 1;
                saveImageAsToolStripMenuItem.Visible = subtitleListView1.SelectedItems.Count == 1;
            }
            else
            {
                normalToolStripMenuItem.Visible = false;
                italicToolStripMenuItem.Visible = false;
                toolStripSeparator1.Visible = false;
                saveImageAsToolStripMenuItem.Visible = true;
            }


        }

        private void SaveImageAsToolStripMenuItemClick(object sender, EventArgs e)
        {
            saveFileDialog1.Title = Configuration.Settings.Language.VobSubOcr.SaveSubtitleImageAs;
            saveFileDialog1.AddExtension = true;                
            saveFileDialog1.FileName = "Image" + _selectedIndex;
            saveFileDialog1.Filter = "PNG image|*.png|BMP image|*.bmp|GIF image|*.gif|TIFF image|*.tiff";
            saveFileDialog1.FilterIndex = 0;

            DialogResult result = saveFileDialog1.ShowDialog(this);
            if (result == DialogResult.OK)
            {
                Bitmap bmp = GetSubtitleBitmap(_selectedIndex);
                if (bmp == null)
                {
                    MessageBox.Show("No image!");
                    return;
                }

                try
                {
                if (saveFileDialog1.FilterIndex == 0)
                    bmp.Save(saveFileDialog1.FileName, System.Drawing.Imaging.ImageFormat.Png);
                else if (saveFileDialog1.FilterIndex == 1)
                    bmp.Save(saveFileDialog1.FileName);
                else if (saveFileDialog1.FilterIndex == 2)
                    bmp.Save(saveFileDialog1.FileName, System.Drawing.Imaging.ImageFormat.Gif);
                else
                    bmp.Save(saveFileDialog1.FileName, System.Drawing.Imaging.ImageFormat.Tiff);
                }
                catch (Exception exception)
                {
                    MessageBox.Show(exception.Message);
                }
            }
        }

        private void NormalToolStripMenuItemClick(object sender, EventArgs e)
        {
            if (_subtitle.Paragraphs.Count > 0 && subtitleListView1.SelectedItems.Count > 0)
            {
                foreach (ListViewItem item in subtitleListView1.SelectedItems)
                {
                    Paragraph p = _subtitle.GetParagraphOrDefault(item.Index);
                    if (p != null)
                    {
                        p.Text = Utilities.RemoveHtmlTags(p.Text);
                        subtitleListView1.SetText(item.Index, p.Text);
                        textBoxCurrentText.Text = p.Text;
                    }
                }
            }
        }

        private void ItalicToolStripMenuItemClick(object sender, EventArgs e)
        {
            const string tag = "i";
            if (_subtitle.Paragraphs.Count > 0 && subtitleListView1.SelectedItems.Count > 0)
            {
                foreach (ListViewItem item in subtitleListView1.SelectedItems)
                {
                    Paragraph p = _subtitle.GetParagraphOrDefault(item.Index);
                    if (p != null)
                    {
                        if (p.Text.Contains("<" + tag + ">"))
                        {
                            p.Text = p.Text.Replace("<" + tag + ">", string.Empty);
                            p.Text = p.Text.Replace("</" + tag + ">", string.Empty);
                        }
                        p.Text = string.Format("<{0}>{1}</{0}>", tag, p.Text);
                        subtitleListView1.SetText(item.Index, p.Text);
                        textBoxCurrentText.Text = p.Text;
                    }
                }
            }
        }

        private void CheckBoxCustomFourColorsCheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxCustomFourColors.Checked)
            {
                pictureBoxPattern.BackColor = Color.White;
                pictureBoxEmphasis1.BackColor = Color.Black;
                pictureBoxEmphasis2.BackColor = Color.Black;
                checkBoxPatternTransparent.Enabled = true;
                checkBoxEmphasis1Transparent.Enabled = true;
                checkBoxEmphasis2Transparent.Enabled = true;
            }
            else
            {
                pictureBoxPattern.BackColor = Color.Gray;
                pictureBoxEmphasis1.BackColor = Color.Gray;
                pictureBoxEmphasis2.BackColor = Color.Gray;
                checkBoxPatternTransparent.Enabled = false;
                checkBoxEmphasis1Transparent.Enabled = false;
                checkBoxEmphasis2Transparent.Enabled = false;
            }
            SubtitleListView1SelectedIndexChanged(null, null);
        }

        private void PictureBoxColorChooserClick(object sender, EventArgs e)
        {
            if (colorDialog1.ShowDialog(this) == DialogResult.OK)
                (sender as PictureBox).BackColor = colorDialog1.Color;
            SubtitleListView1SelectedIndexChanged(null, null);
        }

        private void CheckBoxPatternTransparentCheckedChanged(object sender, EventArgs e)
        {
            SubtitleListView1SelectedIndexChanged(null, null);
        }

        private void CheckBoxEmphasis1TransparentCheckedChanged(object sender, EventArgs e)
        {
            SubtitleListView1SelectedIndexChanged(null, null);
        }

        private void CheckBoxEmphasis2TransparentCheckedChanged(object sender, EventArgs e)
        {
            SubtitleListView1SelectedIndexChanged(null, null);
        }

        private void checkBoxShowOnlyForced_CheckedChanged(object sender, EventArgs e)
        {
            Subtitle oldSubtitle = new Subtitle(_subtitle);
            subtitleListView1.BeginUpdate();
            if (_bluRaySubtitlesOriginal != null)
                LoadBluRaySup();
            else
                LoadVobRip();
            for (int i = 0; i < _subtitle.Paragraphs.Count; i++)
            {
                Paragraph current = _subtitle.Paragraphs[i];
                foreach (Paragraph old in oldSubtitle.Paragraphs)
                {
                    if (current.StartTime.TotalMilliseconds == old.StartTime.TotalMilliseconds &&
                        current.Duration.TotalMilliseconds == old.Duration.TotalMilliseconds)
                    {
                        current.Text = old.Text;
                        break;
                    }

                }
            }
            subtitleListView1.Fill(_subtitle);
            subtitleListView1.EndUpdate();
        }

        private void checkBoxUseTimeCodesFromIdx_CheckedChanged(object sender, EventArgs e)
        {
            Subtitle oldSubtitle = new Subtitle(_subtitle);
            subtitleListView1.BeginUpdate();
            LoadVobRip();
            for (int i = 0; i < _subtitle.Paragraphs.Count; i++)
            {
                Paragraph p = oldSubtitle.GetParagraphOrDefault(i);
                if (p != null && p.Text != string.Empty)
                {
                    _subtitle.Paragraphs[i].Text = p.Text;                    
                }
            }
            subtitleListView1.Fill(_subtitle);
            subtitleListView1.EndUpdate();
        }


    }
}
