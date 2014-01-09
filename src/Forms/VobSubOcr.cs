using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Xml;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.OCR;
using Nikse.SubtitleEdit.Logic.SubtitleFormats;
using Nikse.SubtitleEdit.Logic.VobSub;

namespace Nikse.SubtitleEdit.Forms
{
    public sealed partial class VobSubOcr : Form
    {
        internal class CompareItem
        {
            public ManagedBitmap Bitmap { get; private set; }
            public string Name { get; private set; }
            public bool Italic { get; set; }
            public int ExpandCount { get; private set; }
            public int NumberOfForegroundColors { get; set; }
            public string Text { get; set; }

            public CompareItem(ManagedBitmap bmp, string name, bool isItalic, int expandCount, string text)
            {
                Bitmap = bmp;
                Name = name;
                Italic = isItalic;
                ExpandCount = expandCount;
                NumberOfForegroundColors = -1;
                Text = text;
            }
        }

        internal class SubPicturesWithSeparateTimeCodes
        {
            public SubPicture Picture { get; private set; }
            public TimeSpan Start { get; private set; }
            public TimeSpan End { get; private set; }

            public SubPicturesWithSeparateTimeCodes(SubPicture subPicture, TimeSpan start, TimeSpan end)
            {
                Picture = subPicture;
                Start = start;
                End = end;
            }
        }

        internal class NOcrThreadParameter
        {
            public Bitmap Picture { get; set; }
            public int Index { get; set; }
            public int Increment { get; set; }
            public string Result { get; set; }
            public List<NOcrChar> NOcrChars { get; set; }
            public BackgroundWorker Self { get; set; }
            public double UnItalicFactor { get; set; }
            public bool AdvancedItalicDetection { get; set; }
            public int NOcrLastLowercaseHeight;
            public int NOcrLastUppercaseHeight;
            public int NumberOfPixelsIsSpace;
            public bool RightToLeft;

            public NOcrThreadParameter(Bitmap picture, int index, List<NOcrChar> nOcrChars, BackgroundWorker self, int increment, double unItalicFactor, bool advancedItalicDetection, int numberOfPixelsIsSpace, bool rightToLeft)
            {
                Self = self;
                Picture = picture;
                Index = index;
                NOcrChars = new List<NOcrChar>();
                foreach (NOcrChar c in nOcrChars)
                {
                    NOcrChars.Add(new NOcrChar(c));
                }
                Increment = increment;
                UnItalicFactor = unItalicFactor;
                AdvancedItalicDetection = advancedItalicDetection;
                NOcrLastLowercaseHeight = -1;
                NOcrLastUppercaseHeight = -1;
                NumberOfPixelsIsSpace = numberOfPixelsIsSpace;
                RightToLeft = rightToLeft;
            }
        }

        internal class ImageCompareThreadParameter
        {
            public Bitmap Picture { get; set; }
            public int Index { get; set; }
            public int Increment { get; set; }
            public string Result { get; set; }
            public List<CompareItem> CompareBitmaps { get; set; }
            public BackgroundWorker Self { get; set; }
            public int NumberOfPixelsIsSpace;
            public bool RightToLeft;
            public float MaxErrorPercent;
            public XmlDocument CompareDoc { get; set; }

            public ImageCompareThreadParameter(Bitmap picture, int index, List<CompareItem> compareBitmaps, BackgroundWorker self, int increment, int numberOfPixelsIsSpace, bool rightToLeft, float maxErrorPercent, XmlDocument compareDoc)
            {
                Self = self;
                Picture = picture;
                Index = index;
                CompareBitmaps = new List<CompareItem>();
                foreach (CompareItem c in compareBitmaps)
                {
                    CompareBitmaps.Add(c);
                }
                Increment = increment;
                NumberOfPixelsIsSpace = numberOfPixelsIsSpace;
                RightToLeft = rightToLeft;
                MaxErrorPercent = maxErrorPercent;
                CompareDoc = compareDoc;
            }
        }

        internal class CompareMatch
        {
            public string Text { get; set; }
            public bool Italic { get; set; }
            public int ExpandCount { get; set; }
            public string Name { get; set; }
            public NOcrChar NOcrCharacter { get; set; }
            public CompareMatch(string text, bool italic, int expandCount, string name)
            {
                Text = text;
                Italic = italic;
                ExpandCount = expandCount;
                Name = name;
            }
            public CompareMatch(string text, bool italic, int expandCount, string name, NOcrChar character)
            {
                Text = text;
                Italic = italic;
                ExpandCount = expandCount;
                Name = name;
                NOcrCharacter = character;
            }
        }

        internal class ImageCompareAddition
        {
            public string Name { get; set; }
            public string Text { get; set; }
            public NikseBitmap Image { get; set; }
            public bool Italic { get; set; }
            public int Index { get; set; }

            public ImageCompareAddition(string name, string text, NikseBitmap image, bool italic, int index)
            {
                Name = name;
                Text = text;
                Image = image;
                Text = text;
                Italic = italic;
                Index = index;
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

        private Nikse.SubtitleEdit.Forms.Main _main;
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
        double _unItalicFactor = 0.33;

        Timer _mainOcrTimer;
        int _mainOcrTimerMax = 0;
        int _mainOcrIndex = 0;
        bool _mainOcrRunning = false;
        Bitmap _mainOcrBitmap= null;

        Type _modiType;
        Object _modiDoc;
        bool _modiEnabled;

        // DVD rip/vobsub
        List<VobSubMergedPack> _vobSubMergedPackistOriginal;
        List<VobSubMergedPack> _vobSubMergedPackist;
        List<Color> _palette;

        // BluRay sup
        List<Nikse.SubtitleEdit.Logic.BluRaySup.BluRaySupParser.PcsData> _bluRaySubtitlesOriginal;
        List<Nikse.SubtitleEdit.Logic.BluRaySup.BluRaySupParser.PcsData> _bluRaySubtitles;

        // SP list
        List<SpHeader> _spList;

        // SP vobsub list (mp4)
        List<SubPicturesWithSeparateTimeCodes> _mp4List;

        // XSub (DivX)
        List<XSub> _xSubList;

        // DVB (from transport stream)
        List<Logic.TransportStream.DvbSubtitle> _dvbSubtitles;
        Color _dvbSubColor = Color.Transparent;

        string _lastLine;
        string _languageId;
        string _importLanguageString;

        // Dictionaries/spellchecking/fixing
        OcrFixEngine _ocrFixEngine;
        int _tesseractOcrAutoFixes;

        Subtitle _bdnXmlOriginal;
        Subtitle _bdnXmlSubtitle;
        string _bdnFileName;
        bool _isSon = false;

        List<ImageCompareAddition> _lastAdditions = new List<ImageCompareAddition>();
        VobSubOcrCharacter _vobSubOcrCharacter = new VobSubOcrCharacter();

        List<NOcrChar> _nocrChars = null;
        VobSubOcrNOcrCharacter _vobSubOcrNOcrCharacter = new VobSubOcrNOcrCharacter();
        int _nocrLastLowercaseHeight = -1;
        int _nocrLastUppercaseHeight = -1;
        List<BackgroundWorker> _nocrThreads = null;
        bool _nocrThreadsStop = false;
        string[] _nocrThreadResults = null;
        public const int NocrMinColor = 300;

        List<BackgroundWorker> _icThreads = null;
        bool _icThreadsStop = false;
        string[] _icThreadResults = null;

        Keys _italicShortcut = Utilities.GetKeys(Configuration.Settings.Shortcuts.MainTextBoxItalic);
        Keys _mainGeneralGoToNextSubtitle = Utilities.GetKeys(Configuration.Settings.Shortcuts.GeneralGoToNextSubtitle);
        Keys _mainGeneralGoToPrevSubtitle = Utilities.GetKeys(Configuration.Settings.Shortcuts.GeneralGoToPrevSubtitle);

        private string[] _tesseractAsyncStrings = null;
        private int _tesseractAsyncIndex = 0;
        private BackgroundWorker _tesseractThread = null;

        public static void SetDoubleBuffered(System.Windows.Forms.Control c)
        {
            //Taxes: Remote Desktop Connection and painting http://blogs.msdn.com/oldnewthing/archive/2006/01/03/508694.aspx
            if (System.Windows.Forms.SystemInformation.TerminalServerSession)
                return;

            System.Reflection.PropertyInfo aProp = typeof(System.Windows.Forms.Control).GetProperty("DoubleBuffered", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            aProp.SetValue(c, true, null);
        }

        public VobSubOcr()
        {
            InitializeComponent();
            //this.DoubleBuffered = true;
            SetDoubleBuffered(subtitleListView1);

            var language = Configuration.Settings.Language.VobSubOcr;
            Text = language.Title;
            groupBoxOcrMethod.Text = language.OcrMethod;
            labelTesseractLanguage.Text = language.Language;
            labelImageDatabase.Text = language.ImageDatabase;
            labelNoOfPixelsIsSpace.Text = language.NoOfPixelsIsSpace;
            if (!string.IsNullOrEmpty(language.MaxErrorPercent)) //TODO: Remove in SE 3.4
                labelMaxErrorPercent.Text = language.MaxErrorPercent;
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
            checkBoxBackgroundTransparent.Text = language.Transparent;
            checkBoxPatternTransparent.Text = language.Transparent;
            checkBoxEmphasis1Transparent.Text = language.Transparent;
            checkBoxEmphasis2Transparent.Text = language.Transparent;
            checkBoxAutoTransparentBackground.Text = language.AutoTransparentBackground;
            checkBoxAutoTransparentBackground.Left = groupBoxSubtitleImage.Width - checkBoxAutoTransparentBackground.Width - 2;
            checkBoxPromptForUnknownWords.Text = language.PromptForUnknownWords;
            checkBoxPromptForUnknownWords.Checked = Configuration.Settings.VobSubOcr.PromptForUnknownWords;
            checkBoxGuessUnknownWords.Checked = Configuration.Settings.VobSubOcr.GuessUnknownWords;

            groupBoxOcrAutoFix.Text = language.OcrAutoCorrectionSpellchecking;
            checkBoxGuessUnknownWords.Text = language.TryToGuessUnkownWords;
            checkBoxAutoBreakLines.Text = language.AutoBreakSubtitleIfMoreThanTwoLines;
            checkBoxAutoBreakLines.Checked = Configuration.Settings.VobSubOcr.AutoBreakSubtitleIfMoreThanTwoLines;
            tabControlLogs.TabPages[0].Text = language.AllFixes;
            tabControlLogs.TabPages[1].Text = language.GuessesUsed;
            tabControlLogs.TabPages[2].Text = language.UnknownWords;

            buttonUknownToNames.Text = Configuration.Settings.Language.SpellCheck.AddToNamesAndIgnoreList;
            buttonUknownToUserDic.Text = Configuration.Settings.Language.SpellCheck.AddToUserDictionary;
            buttonAddToOcrReplaceList.Text = Configuration.Settings.Language.SpellCheck.AddToOcrReplaceList;
            buttonGoogleIt.Text = Configuration.Settings.Language.Main.VideoControls.GoogleIt;

            numericUpDownPixelsIsSpace.Left = labelNoOfPixelsIsSpace.Left + labelNoOfPixelsIsSpace.Width + 5;
            numericUpDownMaxErrorPct.Left = numericUpDownPixelsIsSpace.Left;
            groupBoxSubtitleImage.Text = string.Empty;
            labelFixesMade.Text = string.Empty;
            labelFixesMade.Left = checkBoxAutoFixCommonErrors.Left + checkBoxAutoFixCommonErrors.Width;

            labelDictionaryLoaded.Text = string.Format(Configuration.Settings.Language.VobSubOcr.DictionaryX, string.Empty);
            comboBoxDictionaries.Left = labelDictionaryLoaded.Left + labelDictionaryLoaded.Width;

            groupBoxImageCompareMethod.Text = string.Empty; // language.OcrViaImageCompare;
            groupBoxModiMethod.Text = string.Empty; //language.OcrViaModi;
            GroupBoxTesseractMethod.Text = string.Empty;

            checkBoxAutoFixCommonErrors.Text = language.FixOcrErrors;
            checkBoxRightToLeft.Text = language.RightToLeft;
            checkBoxRightToLeft.Left = numericUpDownPixelsIsSpace.Left;
            groupBoxOCRControls.Text = string.Empty;

            FillSpellCheckDictionaries();

            comboBoxOcrMethod.Items.Clear();
            comboBoxOcrMethod.Items.Add(language.OcrViaTesseract);
            comboBoxOcrMethod.Items.Add(language.OcrViaImageCompare);
            comboBoxOcrMethod.Items.Add(language.OcrViaModi);
            if (!string.IsNullOrEmpty(language.OcrViaNOCR) && Configuration.Settings.General.ShowBetaStuff)
                comboBoxOcrMethod.Items.Add(language.OcrViaNOCR);

            checkBoxUseModiInTesseractForUnknownWords.Text = language.TryModiForUnknownWords;
            checkBoxTesseractItalicsOn.Checked = Configuration.Settings.VobSubOcr.UseItalicsInTesseract;
            checkBoxTesseractItalicsOn.Text = Configuration.Settings.Language.General.Italic;

            checkBoxTesseractMusicOn.Checked = Configuration.Settings.VobSubOcr.UseMusicSymbolsInTesseract;
            checkBoxTesseractMusicOn.Text = Configuration.Settings.Language.Settings.MusicSymbol;
            checkBoxTesseractMusicOn.Left = checkBoxTesseractItalicsOn.Left + checkBoxTesseractItalicsOn.Width + 15;

            if (Configuration.Settings.VobSubOcr.ItalicFactor >= 0.1 && Configuration.Settings.VobSubOcr.ItalicFactor < 1)
                _unItalicFactor = Configuration.Settings.VobSubOcr.ItalicFactor;
            checkBoxShowOnlyForced.Text = language.ShowOnlyForcedSubtitles;
            checkBoxUseTimeCodesFromIdx.Text = language.UseTimeCodesFromIdx;

            normalToolStripMenuItem.Text = Configuration.Settings.Language.Main.Menu.ContextMenu.Normal;
            italicToolStripMenuItem.Text = Configuration.Settings.Language.General.Italic;
            importTextWithMatchingTimeCodesToolStripMenuItem.Text = language.ImportTextWithMatchingTimeCodes;
            saveImageAsToolStripMenuItem.Text = language.SaveSubtitleImageAs;
            saveAllImagesWithHtmlIndexViewToolStripMenuItem.Text = language.SaveAllSubtitleImagesWithHtml;
            inspectImageCompareMatchesForCurrentImageToolStripMenuItem.Text = language.InspectCompareMatchesForCurrentImage;
            EditLastAdditionsToolStripMenuItem.Text = language.EditLastAdditions;
            checkBoxRightToLeft.Checked = Configuration.Settings.VobSubOcr.RightToLeft;
            deleteToolStripMenuItem.Text = Configuration.Settings.Language.Main.Menu.ContextMenu.Delete;

            toolStripMenuItemExport.Text = Configuration.Settings.Language.Main.Menu.File.Export;
            vobSubToolStripMenuItem.Text = Configuration.Settings.Language.Main.Menu.File.ExportVobSub;
            bDNXMLToolStripMenuItem.Text = Configuration.Settings.Language.Main.Menu.File.ExportBdnXml;
            bluraySupToolStripMenuItem.Text = Configuration.Settings.Language.Main.Menu.File.ExportBluRaySup;

            toolStripMenuItemClearFixes.Text = Configuration.Settings.Language.DvdSubrip.Clear;
            toolStripMenuItemClearGuesses.Text = Configuration.Settings.Language.DvdSubrip.Clear;
            clearToolStripMenuItem.Text = Configuration.Settings.Language.DvdSubrip.Clear;

            checkBoxNOcrCorrect.Checked = Configuration.Settings.VobSubOcr.LineOcrDraw;
            checkBoxNOcrItalic.Checked = Configuration.Settings.VobSubOcr.LineOcrAdvancedItalic;

            comboBoxTesseractLanguages.Left = labelTesseractLanguage.Left + labelTesseractLanguage.Width;
            buttonGetTesseractDictionaries.Left = comboBoxTesseractLanguages.Left + comboBoxTesseractLanguages.Width + 5;

            Utilities.InitializeSubtitleFont(subtitleListView1);
            subtitleListView1.AutoSizeAllColumns(this);

            Utilities.InitializeSubtitleFont(textBoxCurrentText);

            italicToolStripMenuItem.ShortcutKeys = Utilities.GetKeys(Configuration.Settings.Shortcuts.MainListViewItalic);

            comboBoxTesseractLanguages.Left = labelTesseractLanguage.Left + labelTesseractLanguage.Width + 3;
            comboBoxModiLanguage.Left = label1.Left + label1.Width + 3;

            comboBoxCharacterDatabase.Left = labelImageDatabase.Left + labelImageDatabase.Width + 3;
            buttonNewCharacterDatabase.Left = comboBoxCharacterDatabase.Left + comboBoxCharacterDatabase.Width + 3;
            buttonEditCharacterDatabase.Left = buttonNewCharacterDatabase.Left;
            numericUpDownPixelsIsSpace.Left = labelNoOfPixelsIsSpace.Left + labelNoOfPixelsIsSpace.Width + 3;
            checkBoxRightToLeft.Left = numericUpDownPixelsIsSpace.Left;

            FixLargeFonts();
            buttonEditCharacterDatabase.Top = buttonNewCharacterDatabase.Top + buttonNewCharacterDatabase.Height + 3;

            splitContainerBottom.Panel1MinSize = 400;
            splitContainerBottom.Panel2MinSize = 250;

            pictureBoxBackground.Left = checkBoxCustomFourColors.Left + checkBoxCustomFourColors.Width + 8;
            checkBoxBackgroundTransparent.Left = pictureBoxBackground.Left + pictureBoxBackground.Width + 3;
            pictureBoxPattern.Left = checkBoxBackgroundTransparent.Left + checkBoxBackgroundTransparent.Width + 8;
            checkBoxPatternTransparent.Left = pictureBoxPattern.Left + pictureBoxPattern.Width + 3;
            pictureBoxEmphasis1.Left = checkBoxPatternTransparent.Left + checkBoxPatternTransparent.Width + 8;
            checkBoxEmphasis1Transparent.Left = pictureBoxEmphasis1.Left + pictureBoxEmphasis1.Width + 3;
            pictureBoxEmphasis2.Left = checkBoxEmphasis1Transparent.Left + checkBoxEmphasis1Transparent.Width + 8;
            checkBoxEmphasis2Transparent.Left = pictureBoxEmphasis2.Left + pictureBoxEmphasis2.Width + 3;

            buttonGetTesseractDictionaries.Visible = !string.IsNullOrEmpty(Configuration.Settings.Language.GetTesseractDictionaries.Title);

            try
            {
                numericUpDownMaxErrorPct.Value = (decimal)Configuration.Settings.VobSubOcr.AllowDifferenceInPercent;
            }
            catch
            {
                numericUpDownMaxErrorPct.Value = 1.1m;
            }
        }

        private void FillSpellCheckDictionaries()
        {
            comboBoxDictionaries.SelectedIndexChanged -= comboBoxDictionaries_SelectedIndexChanged;
            comboBoxDictionaries.Items.Clear();
            comboBoxDictionaries.Items.Add(Configuration.Settings.Language.General.None);
            foreach (string name in Utilities.GetDictionaryLanguages())
            {
                comboBoxDictionaries.Items.Add(name);
            }
            comboBoxDictionaries.SelectedIndexChanged += comboBoxDictionaries_SelectedIndexChanged;
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

        internal void InitializeBatch(string vobSubFileName, VobSubOcrSettings vobSubOcrSettings, bool useNewSubIdxCode)
        {
            Initialize(vobSubFileName, vobSubOcrSettings, useNewSubIdxCode, null);
            FormVobSubOcr_Shown(null, null);

            int max = GetSubtitleCount();
            if (comboBoxOcrMethod.SelectedIndex == 0 && _tesseractAsyncStrings == null)
            {
                _tesseractAsyncStrings = new string[max];
                _tesseractAsyncIndex = (int)numericUpDownStartNumber.Value + 5;
                _tesseractThread = new BackgroundWorker();
                _tesseractThread.DoWork += TesseractThreadDoWork;
                _tesseractThread.RunWorkerCompleted += TesseractThreadRunWorkerCompleted;
                _tesseractThread.WorkerSupportsCancellation = true;
                if (_tesseractAsyncIndex >= 0 && _tesseractAsyncIndex < max)
                    _tesseractThread.RunWorkerAsync(GetSubtitleBitmap(_tesseractAsyncIndex));
            }
            System.Threading.Thread.Sleep(1000);
            subtitleListView1.SelectedIndexChanged -= SubtitleListView1SelectedIndexChanged;
            for (int i = 0; i < max; i++)
            {
                var startTime = new TimeCode(TimeSpan.FromMilliseconds(GetSubtitleStartTimeMilliseconds(i)));
                var endTime = new TimeCode(TimeSpan.FromMilliseconds(GetSubtitleEndTimeMilliseconds(i)));
                Application.DoEvents();
                if (_abort)
                {
                    SetButtonsEnabledAfterOcrDone();
                    return;
                }

                subtitleListView1.SelectIndexAndEnsureVisible(i);
                string text = OcrViaTesseract(GetSubtitleBitmap(i), i);

                _lastLine = text;

                text = text.Replace("<i>-</i>", "-");
                text = text.Replace("<i>a</i>", "a");
                text = text.Replace("  ", " ");
                text = text.Trim();

                text = text.Replace(" " + Environment.NewLine, Environment.NewLine);
                text = text.Replace(Environment.NewLine + " ", Environment.NewLine);

                // max allow 2 lines
                if (checkBoxAutoBreakLines.Checked && text.Replace(Environment.NewLine, "*").Length + 2 <= text.Length)
                {
                    text = text.Replace(" " + Environment.NewLine, Environment.NewLine);
                    text = text.Replace(Environment.NewLine + " ", Environment.NewLine);
                    while (text.Contains(Environment.NewLine + Environment.NewLine))
                        text = text.Replace(Environment.NewLine + Environment.NewLine, Environment.NewLine);

                    if (text.Replace(Environment.NewLine, "*").Length + 2 <= text.Length)
                        text = Utilities.AutoBreakLine(text);
                }

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
                if (subtitleListView1.SelectedItems.Count == 1 && subtitleListView1.SelectedItems[0].Index == i)
                    textBoxCurrentText.Text = text;
                else
                    subtitleListView1.SetText(i, text);
            }
            SetButtonsEnabledAfterOcrDone();
        }

        internal bool Initialize(string vobSubFileName, VobSubOcrSettings vobSubOcrSettings, bool useNewSubIdxCode, Nikse.SubtitleEdit.Forms.Main main)
        {
            _main = main;
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
            numericUpDownNumberOfPixelsIsSpaceNOCR.Value = vobSubOcrSettings.XOrMorePixelsMakesSpace;
            _vobSubOcrSettings = vobSubOcrSettings;

            InitializeModi();
            InitializeTesseract();
            LoadImageCompareCharacterDatabaseList();

            SetOcrMethod();

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
            numericUpDownNumberOfPixelsIsSpaceNOCR.Value = vobSubOcrSettings.XOrMorePixelsMakesSpace;
            _vobSubOcrSettings = vobSubOcrSettings;

            InitializeModi();
            InitializeTesseract();
            LoadImageCompareCharacterDatabaseList();

            SetOcrMethod();

            _vobSubMergedPackist = vobSubMergedPackist;
            _palette = palette;

            if (_palette == null)
                checkBoxCustomFourColors.Checked = true;

            SetTesseractLanguageFromLanguageString(languageString);
            _importLanguageString = languageString;
        }

        internal void InitializeQuick(List<VobSubMergedPack> vobSubMergedPackist, List<Color> palette, VobSubOcrSettings vobSubOcrSettings, string languageString)
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
            numericUpDownNumberOfPixelsIsSpaceNOCR.Value = vobSubOcrSettings.XOrMorePixelsMakesSpace;
            _vobSubOcrSettings = vobSubOcrSettings;
            _vobSubMergedPackist = vobSubMergedPackist;
            _palette = palette;

            if (_palette == null)
                checkBoxCustomFourColors.Checked = true;

            _importLanguageString = languageString;
            if (_importLanguageString.Contains("(") && !_importLanguageString.StartsWith("("))
                _importLanguageString = _importLanguageString.Substring(0, languageString.IndexOf("(") - 1).Trim();
        }

        internal void InitializeBatch(List<Nikse.SubtitleEdit.Logic.BluRaySup.BluRaySupParser.PcsData> subtitles, VobSubOcrSettings vobSubOcrSettings, string fileName)
        {
            Initialize(subtitles, vobSubOcrSettings, fileName);
            FormVobSubOcr_Shown(null, null);
            checkBoxPromptForUnknownWords.Checked = false;

            int max = GetSubtitleCount();
            if (comboBoxOcrMethod.SelectedIndex == 0 && _tesseractAsyncStrings == null)
            {
                _tesseractAsyncStrings = new string[max];
                _tesseractAsyncIndex = (int)numericUpDownStartNumber.Value + 5;
                _tesseractThread = new BackgroundWorker();
                _tesseractThread.DoWork += TesseractThreadDoWork;
                _tesseractThread.RunWorkerCompleted += TesseractThreadRunWorkerCompleted;
                _tesseractThread.WorkerSupportsCancellation = true;
                if (_tesseractAsyncIndex >= 0 && _tesseractAsyncIndex < max)
                    _tesseractThread.RunWorkerAsync(GetSubtitleBitmap(_tesseractAsyncIndex));
            }
            System.Threading.Thread.Sleep(1000);
            subtitleListView1.SelectedIndexChanged -= SubtitleListView1SelectedIndexChanged;
            for (int i = 0; i < max; i++)
            {
                var startTime = new TimeCode(TimeSpan.FromMilliseconds(GetSubtitleStartTimeMilliseconds(i)));
                var endTime = new TimeCode(TimeSpan.FromMilliseconds(GetSubtitleEndTimeMilliseconds(i)));
                Application.DoEvents();
                if (_abort)
                {
                    SetButtonsEnabledAfterOcrDone();
                    return;
                }

                subtitleListView1.SelectIndexAndEnsureVisible(i);
                string text = OcrViaTesseract(GetSubtitleBitmap(i), i);

                _lastLine = text;

                text = text.Replace("<i>-</i>", "-");
                text = text.Replace("<i>a</i>", "a");
                text = text.Replace("  ", " ");
                text = text.Trim();

                text = text.Replace(" " + Environment.NewLine, Environment.NewLine);
                text = text.Replace(Environment.NewLine + " ", Environment.NewLine);

                // max allow 2 lines
                if (checkBoxAutoBreakLines.Checked && text.Replace(Environment.NewLine, "*").Length + 2 <= text.Length)
                {
                    text = text.Replace(" " + Environment.NewLine, Environment.NewLine);
                    text = text.Replace(Environment.NewLine + " ", Environment.NewLine);
                    while (text.Contains(Environment.NewLine + Environment.NewLine))
                        text = text.Replace(Environment.NewLine + Environment.NewLine, Environment.NewLine);

                    if (text.Replace(Environment.NewLine, "*").Length + 2 <= text.Length)
                        text = Utilities.AutoBreakLine(text);
                }

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
                if (subtitleListView1.SelectedItems.Count == 1 && subtitleListView1.SelectedItems[0].Index == i)
                    textBoxCurrentText.Text = text;
                else
                    subtitleListView1.SetText(i, text);
            }
            SetButtonsEnabledAfterOcrDone();
        }

        internal void Initialize(List<Nikse.SubtitleEdit.Logic.BluRaySup.BluRaySupParser.PcsData> subtitles, VobSubOcrSettings vobSubOcrSettings, string fileName)
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
            numericUpDownNumberOfPixelsIsSpaceNOCR.Value = 11;
            _vobSubOcrSettings = vobSubOcrSettings;

            InitializeModi();
            InitializeTesseract();
            LoadImageCompareCharacterDatabaseList();

            SetOcrMethod();

            _bluRaySubtitlesOriginal = subtitles;

            groupBoxImagePalette.Visible = false;

            Text = Configuration.Settings.Language.VobSubOcr.TitleBluRay;
            if (!string.IsNullOrEmpty(fileName))
            {
                if (fileName.Length > 40)
                    fileName = Path.GetFileName(fileName);
                Text += " - " + fileName;
            }
            checkBoxAutoTransparentBackground.Checked = false;
            checkBoxAutoTransparentBackground.Visible = false;
        }

        private void LoadImageCompareCharacterDatabaseList()
        {
            try
            {
                comboBoxCharacterDatabase.SelectedIndexChanged -= ComboBoxCharacterDatabaseSelectedIndexChanged;
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
                comboBoxCharacterDatabase.SelectedIndexChanged += ComboBoxCharacterDatabaseSelectedIndexChanged;

            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format(Configuration.Settings.Language.VobSubOcr.UnableToCreateCharacterDatabaseFolder, ex.Message));
            }
        }

        private void LoadImageCompareBitmaps()
        {
            DisposeImageCompareBitmaps();

            _compareBitmaps = new List<CompareItem>();
            string path = Configuration.VobSubCompareFolder + comboBoxCharacterDatabase.SelectedItem + Path.DirectorySeparatorChar;
            if (!File.Exists(path + "CompareDescription.xml"))
                _compareDoc.LoadXml("<OcrBitmaps></OcrBitmaps>");
            else
                _compareDoc.Load(path + "CompareDescription.xml");

            string databaseName = path + "Images.db";
            if (!File.Exists(databaseName))
            {
                labelStatus.Text = Configuration.Settings.Language.VobSubOcr.LoadingImageCompareDatabase;
                labelStatus.Refresh();
                using (var f = new FileStream(databaseName, FileMode.Create))
                {
                    foreach (string bmpFileName in Directory.GetFiles(path, "*.bmp"))
                    {
                        string name = Path.GetFileNameWithoutExtension(bmpFileName);

                        XmlNode node = _compareDoc.DocumentElement.SelectSingleNode("FileName[.='" + name + "']");
                        if (node != null)
                        {
                            node.InnerText = f.Position.ToString(CultureInfo.InvariantCulture);
                            var b = new Bitmap(bmpFileName);
                            var m = new ManagedBitmap(b);
                            b.Dispose();
                            m.AppendToStream(f);
                        }
                    }
                    f.Close();
                }
                _compareDoc.Save(path + "Images.xml");
                string text = File.ReadAllText(path + "Images.xml");
                File.WriteAllText(path + "Images.xml", text.Replace("<FileName", "<Item").Replace("</FileName>", "</Item>"));
                labelStatus.Text = string.Empty;
            }

            if (File.Exists(databaseName))
            {
                labelStatus.Text = Configuration.Settings.Language.VobSubOcr.LoadingImageCompareDatabase;
                labelStatus.Refresh();
                _compareDoc.Load(path + "Images.xml");
                using (var f = new FileStream(databaseName, FileMode.Open))
                {
                    foreach (XmlNode node in _compareDoc.DocumentElement.SelectNodes("Item"))
                    {
                        try
                        {
                            string name = node.InnerText;
                            int pos = Convert.ToInt32(name);
                            bool isItalic = node.Attributes["Italic"] != null;
                            string text = node.Attributes["Text"].InnerText;
                            int expandCount = 0;
                            if (node.Attributes["Expand"] != null)
                            {
                                if (!int.TryParse(node.Attributes["Expand"].InnerText, out expandCount))
                                    expandCount = 0;
                            }
                            f.Position = pos;
                            var mbmp = new ManagedBitmap(f);
                            _compareBitmaps.Add(new CompareItem(mbmp, name, isItalic, expandCount, text));
                        }
                        catch
                        {
                            //MessageBox.Show(node.OuterXml);
                        }
                    }
                }
                labelStatus.Text = string.Empty;
            }
        }

        private void DisposeImageCompareBitmaps()
        {
            _compareBitmaps = null;
        }

        private bool InitializeSubIdx(string vobSubFileName)
        {
            VobSubParser vobSubParser = new VobSubParser(true);
            string idxFileName = Path.ChangeExtension(vobSubFileName, ".idx");
            vobSubParser.OpenSubIdx(vobSubFileName, idxFileName);
            _vobSubMergedPackist = vobSubParser.MergeVobSubPacks();
            _palette = vobSubParser.IdxPalette;
            vobSubParser.VobSubPacks.Clear();

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
                Form form = _main;
                if (form == null)
                    form = this;
                ChooseLanguage.Activate();
                if (ChooseLanguage.ShowDialog(form) == System.Windows.Forms.DialogResult.OK)
                {
                    _vobSubMergedPackist = ChooseLanguage.SelectedVobSubMergedPacks;
                    SetTesseractLanguageFromLanguageString(ChooseLanguage.SelectedLanguageString);
                    _importLanguageString = ChooseLanguage.SelectedLanguageString;
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

        private void LoadBdnXml()
        {
            _subtitle = new Subtitle();

            _bdnXmlSubtitle = new Subtitle();
            int max = _bdnXmlOriginal.Paragraphs.Count;
            for (int i = 0; i < max; i++)
            {
                var x = _bdnXmlOriginal.Paragraphs[i];
                if ((checkBoxShowOnlyForced.Checked && x.Forced) ||
                    checkBoxShowOnlyForced.Checked == false)
                {
                    _bdnXmlSubtitle.Paragraphs.Add(new Paragraph(x));
                    Paragraph p = new Paragraph(x);
                    p.Text = string.Empty;
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

        private void LoadBluRaySup()
        {
            _subtitle = new Subtitle();

            _bluRaySubtitles = new List<Nikse.SubtitleEdit.Logic.BluRaySup.BluRaySupParser.PcsData>();
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
                    double newEndTime = p.StartTime.TotalMilliseconds + Configuration.Settings.VobSubOcr.DefaultMillisecondsForUnknownDurations;
                    if (next == null || (newEndTime < next.StartTime.TotalMilliseconds))
                        p.EndTime.TotalMilliseconds = newEndTime;
                    else if (next != null)
                        p.EndTime.TotalMilliseconds = next.StartTime.TotalMilliseconds - 1;
                }
            }
        }

        public Bitmap GetSubtitleBitmap(int index)
        {
            Color background;
            Color pattern;
            Color emphasis1;
            Color emphasis2;

            if (_mp4List != null)
            {
                Bitmap mp4Bmp;
                if (checkBoxCustomFourColors.Checked)
                {
                    GetCustomColors(out background, out pattern, out emphasis1, out emphasis2);

                    mp4Bmp = _mp4List[index].Picture.GetBitmap(null, background, pattern, emphasis1, emphasis2, true);
                    if (checkBoxAutoTransparentBackground.Checked)
                        mp4Bmp.MakeTransparent();
                    return mp4Bmp;
                }

                mp4Bmp = _mp4List[index].Picture.GetBitmap(null, Color.Transparent, Color.Black, Color.White, Color.Black, false);
                if (checkBoxAutoTransparentBackground.Checked)
                    mp4Bmp.MakeTransparent();
                return mp4Bmp;

            }
            else if (_spList != null)
            {
                Bitmap spBmp;
                if (checkBoxCustomFourColors.Checked)
                {
                    GetCustomColors(out background, out pattern, out emphasis1, out emphasis2);

                    spBmp = _spList[index].Picture.GetBitmap(null, background, pattern, emphasis1, emphasis2, true);
                    if (checkBoxAutoTransparentBackground.Checked)
                        spBmp.MakeTransparent();
                    return spBmp;
                }

                spBmp = _spList[index].Picture.GetBitmap(null, Color.Transparent, Color.Black, Color.White, Color.Black, false);
                if (checkBoxAutoTransparentBackground.Checked)
                    spBmp.MakeTransparent();
                return spBmp;
            }
            else if (_bdnXmlSubtitle != null)
            {
                if (index >= 0 && index < _bdnXmlSubtitle.Paragraphs.Count)
                {
                    string[] fileNames = _bdnXmlSubtitle.Paragraphs[index].Text.Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                    var bitmaps = new List<Bitmap>();
                    int maxWidth = 0;
                    int totalHeight = 0;

                    foreach (string fn in fileNames)
                    {
                        string fullFileName = Path.Combine(Path.GetDirectoryName(_bdnFileName), fn);
                        if (File.Exists(fullFileName))
                        {
                            try
                            {
                                var temp = new Bitmap(fullFileName);
                                if (temp.Width > maxWidth)
                                    maxWidth = temp.Width;
                                totalHeight += temp.Height;
                                bitmaps.Add(temp);
                            }
                            catch
                            {
                                return null;
                            }
                        }
                    }

                    Bitmap b = null;
                    if (bitmaps.Count > 1)
                    {
                        var merged = new Bitmap(maxWidth, totalHeight + 7 * bitmaps.Count);
                        int y = 0;
                        for (int k = 0; k < bitmaps.Count; k++)
                        {
                            Bitmap part = bitmaps[k];
                            if (checkBoxAutoTransparentBackground.Checked)
                                part.MakeTransparent();
                            using (var g = Graphics.FromImage(merged))
                                g.DrawImage(part, 0, y);
                            y += part.Height + 7;
                            part.Dispose();
                        }
                        b = merged;
                    }
                    else if (bitmaps.Count == 1)
                    {
                        b = bitmaps[0];
                    }

                    if (b != null)
                    {
                        if (_isSon && checkBoxCustomFourColors.Checked)
                        {
                            GetCustomColors(out background, out pattern, out emphasis1, out emphasis2);

                            FastBitmap fbmp = new FastBitmap(b);
                            fbmp.LockImage();
                            for (int x = 0; x < fbmp.Width; x++)
                            {
                                for (int y = 0; y < fbmp.Height; y++)
                                {
                                    Color c = fbmp.GetPixel(x, y);
                                    if (c.R == Color.Red.R && c.G == Color.Red.G && c.B == Color.Red.B) // normally anti-alias
                                        fbmp.SetPixel(x, y, emphasis2);
                                    else if (c.R == Color.Blue.R && c.G == Color.Blue.G && c.B == Color.Blue.B) // normally text?
                                        fbmp.SetPixel(x, y, pattern);
                                    else if (c.R == Color.White.R && c.G == Color.White.G && c.B == Color.White.B) // normally background
                                        fbmp.SetPixel(x, y, background);
                                    else if (c.R == Color.Black.R && c.G == Color.Black.G && c.B == Color.Black.B) // outline/border
                                        fbmp.SetPixel(x, y, emphasis1);
                                    else
                                        fbmp.SetPixel(x, y, c);
                                }
                            }
                            fbmp.UnlockImage();
                        }
                        if (checkBoxAutoTransparentBackground.Checked)
                            b.MakeTransparent();
                        return b;
                    }
                }
                return null;
            }
            else if (_xSubList != null)
            {
                if (checkBoxCustomFourColors.Checked)
                {
                    GetCustomColors(out background, out pattern, out emphasis1, out emphasis2);
                    return _xSubList[index].GetImage(background, pattern, emphasis1, emphasis2);
                }
                else
                {
                    return _xSubList[index].GetImage();
                }
            }
            else if (_dvbSubtitles != null)
            {
                var dvbBmp = _dvbSubtitles[index].GetActiveImage();
                var nDvbBmp = new NikseBitmap(dvbBmp);
                nDvbBmp.CropTopTransparent(2);
                nDvbBmp.CropTransparentSidesAndBottom(2, true);
                if (checkBoxTransportStreamGetColorAndSplit.Checked)
                    _dvbSubColor = nDvbBmp.GetBrightestColor();
                if (checkBoxTransportStreamGrayscale.Checked)
                    nDvbBmp.GrayScale();
                dvbBmp.Dispose();                
                return nDvbBmp.GetBitmap();
            }

            if (_bluRaySubtitlesOriginal != null)
            {
                return _bluRaySubtitles[index].GetBitmap();
            }
            else if (checkBoxCustomFourColors.Checked)
            {
                GetCustomColors(out background, out pattern, out emphasis1, out emphasis2);

                Bitmap bm = _vobSubMergedPackist[index].SubPicture.GetBitmap(null, background, pattern, emphasis1, emphasis2, true);
                if (checkBoxAutoTransparentBackground.Checked)
                    bm.MakeTransparent();
                return bm;
            }

            Bitmap bmp = _vobSubMergedPackist[index].SubPicture.GetBitmap(_palette, Color.Transparent, Color.Black, Color.White, Color.Black, false);
            if (checkBoxAutoTransparentBackground.Checked)
                bmp.MakeTransparent();
            return bmp;
        }

        private void GetCustomColors(out Color background, out Color pattern, out Color emphasis1, out Color emphasis2)
        {
            background = pictureBoxBackground.BackColor;
            pattern = pictureBoxPattern.BackColor;
            emphasis1 = pictureBoxEmphasis1.BackColor;
            emphasis2 = pictureBoxEmphasis2.BackColor;

            if (checkBoxBackgroundTransparent.Checked)
                background = Color.Transparent;
            if (checkBoxPatternTransparent.Checked)
                pattern = Color.Transparent;
            if (checkBoxEmphasis1Transparent.Checked)
                emphasis1 = Color.Transparent;
            if (checkBoxEmphasis2Transparent.Checked)
                emphasis2 = Color.Transparent;
        }

        private long GetSubtitleStartTimeMilliseconds(int index)
        {
            if (_mp4List != null)
                return (long)(_mp4List[index].Start.TotalMilliseconds);
            else if (_spList != null)
                return (long)(_spList[index].StartTime.TotalMilliseconds);
            else if (_bdnXmlSubtitle != null)
                return (long)_bdnXmlSubtitle.Paragraphs[index].StartTime.TotalMilliseconds;
            else if (_bluRaySubtitlesOriginal != null)
                return (_bluRaySubtitles[index].StartTime + 45) / 90;
            else if (_xSubList != null)
                return (long)_xSubList[index].Start.TotalMilliseconds;
            else if (_dvbSubtitles != null)
                return (long)_dvbSubtitles[index].StartMilliseconds;
            else
                return (long)_vobSubMergedPackist[index].StartTime.TotalMilliseconds;
        }

        private long GetSubtitleEndTimeMilliseconds(int index)
        {
            if (_mp4List != null)
                return (long)(_mp4List[index].End.TotalMilliseconds);
            else if (_spList != null)
                return (long)(_spList[index].StartTime.TotalMilliseconds + _spList[index].Picture.Delay.TotalMilliseconds);
            else if (_bdnXmlSubtitle != null)
                return (long)_bdnXmlSubtitle.Paragraphs[index].EndTime.TotalMilliseconds;
            else if (_bluRaySubtitlesOriginal != null)
                return (_bluRaySubtitles[index].EndTime + 45) / 90;
            else if (_xSubList != null)
                return (long)_xSubList[index].End.TotalMilliseconds;
            else if (_dvbSubtitles != null)
                return (long)_dvbSubtitles[index].EndMilliseconds;
            else
                return (long)_vobSubMergedPackist[index].EndTime.TotalMilliseconds;
        }

        private int GetSubtitleCount()
        {
            if (_mp4List != null)
                return _mp4List.Count;
            else if (_spList != null)
                return _spList.Count;
            else if (_bdnXmlSubtitle != null)
                return _bdnXmlSubtitle.Paragraphs.Count;
            else if (_bluRaySubtitlesOriginal != null)
                return _bluRaySubtitles.Count;
            else if (_xSubList != null)
                return _xSubList.Count;
            else if (_dvbSubtitles != null)
                return _dvbSubtitles.Count;
            else
                return _vobSubMergedPackist.Count;
        }

        private Bitmap ShowSubtitleImage(int index)
        {
            int numberOfImages = GetSubtitleCount();
            Bitmap bmp;
            if (index < numberOfImages)
            {
                bmp = GetSubtitleBitmap(index);
                if (bmp == null)
                    bmp = new Bitmap(1, 1);
                groupBoxSubtitleImage.Text = string.Format(Configuration.Settings.Language.VobSubOcr.SubtitleImageXofY, index + 1, numberOfImages) + "   " + bmp.Width + "x" + bmp.Height;
            }
            else
            {
                groupBoxSubtitleImage.Text = Configuration.Settings.Language.VobSubOcr.SubtitleImage;
                bmp = new Bitmap(1, 1);
            }
            Bitmap old = pictureBoxSubtitleImage.Image as Bitmap;
            pictureBoxSubtitleImage.Image = bmp.Clone() as Bitmap;
            pictureBoxSubtitleImage.Invalidate();
            if (old != null)
                old.Dispose();
            return bmp;
        }

        private void ShowSubtitleImage(int index, Bitmap bmp)
        {
            try
            {
                int numberOfImages = GetSubtitleCount();
                if (index < numberOfImages)
                {
                    groupBoxSubtitleImage.Text = string.Format(Configuration.Settings.Language.VobSubOcr.SubtitleImageXofY, index + 1, numberOfImages) + "   " + bmp.Width + "x" + bmp.Height;
                }
                else
                {
                    groupBoxSubtitleImage.Text = Configuration.Settings.Language.VobSubOcr.SubtitleImage;
                }
                Bitmap old = pictureBoxSubtitleImage.Image as Bitmap;
                pictureBoxSubtitleImage.Image = bmp.Clone() as Bitmap;
                pictureBoxSubtitleImage.Invalidate();
                if (old != null)
                    old.Dispose();
            }
            catch
            {
                // can crash is user is clicking around...
            }
        }

        private static Point MakePointItalic(Point p, int height, int moveLeftPixels, double unItalicFactor)
        {
            return new Point((int)Math.Round(p.X + (height - p.Y) * unItalicFactor - moveLeftPixels), p.Y);
        }

        private static NOcrChar NOcrFindExpandedMatch(Bitmap parentBitmap, ImageSplitterItem targetItem, int topMargin, List<NOcrChar> nOcrChars)
        {
            var nbmp = new NikseBitmap(parentBitmap);
            int w = targetItem.NikseBitmap.Width;
            int index = 0;
            foreach (NOcrChar oc in nOcrChars)
            {
                    if (oc.ExpandCount > 1 && oc.Width > w && targetItem.X + oc.Width < nbmp.Width)
                    {
                        bool ok = true;
                        index = 0;
                        while (index < oc.LinesForeground.Count && ok)
                        {
                            NOcrPoint op = oc.LinesForeground[index];
                            foreach (Point point in op.GetPoints(oc.Width, oc.Height))
                            {
                                Point p = new Point(point.X + targetItem.X, point.Y + targetItem.Y);
                                if (p.X >= 0 && p.Y >= 0 && p.X < nbmp.Width && p.Y < nbmp.Height)
                                {
                                    Color c = nbmp.GetPixel(p.X, p.Y);
                                    if (c.A > 150 && c.R + c.G + c.B > NocrMinColor)
                                    {
                                    }
                                    else
                                    {
                                        ok = false;
                                        break;
                                    }
                                }
                            }
                            index++;
                        }
                        index = 0;
                        while (index < oc.LinesBackground.Count && ok)
                        {
                            NOcrPoint op = oc.LinesBackground[index];
                            foreach (Point point in op.GetPoints(oc.Width, oc.Height))
                            {
                                Point p = new Point(point.X + targetItem.X, point.Y + targetItem.Y);
                                if (p.X >= 0 && p.Y >= 0 && p.X < nbmp.Width && p.Y < nbmp.Height)
                                {
                                    Color c = nbmp.GetPixel(p.X, p.Y);
                                    if (c.A > 150 && c.R + c.G + c.B > NocrMinColor)
                                    {
                                        ok = false;
                                        break;
                                    }
                                }
                            }
                            index++;
                        }
                        if (ok)
                            return oc;


                        ok = true;
                        index = 0;
                        while (index < oc.LinesForeground.Count && ok)
                        {
                            NOcrPoint op = oc.LinesForeground[index];
                            foreach (Point point in op.ScaledGetPoints(oc, oc.Width, oc.Height - 1))
                            {
                                Point p = new Point(point.X + targetItem.X, point.Y + targetItem.Y);
                                if (p.X >= 0 && p.Y >= 0 && p.X < nbmp.Width && p.Y < nbmp.Height)
                                {
                                    Color c = nbmp.GetPixel(p.X, p.Y);
                                    if (c.A > 150 && c.R + c.G + c.B > NocrMinColor)
                                    {
                                    }
                                    else
                                    {
                                        ok = false;
                                        break;
                                    }
                                }
                            }
                            index++;
                        }
                        index = 0;
                        while (index < oc.LinesBackground.Count && ok)
                        {
                            NOcrPoint op = oc.LinesBackground[index];
                            foreach (Point point in op.ScaledGetPoints(oc, oc.Width, oc.Height - 1))
                            {
                                Point p = new Point(point.X + targetItem.X, point.Y + targetItem.Y);
                                if (p.X >= 0 && p.Y >= 0 && p.X < nbmp.Width && p.Y < nbmp.Height)
                                {
                                    Color c = nbmp.GetPixel(p.X, p.Y);
                                    if (c.A > 150 && c.R + c.G + c.B > NocrMinColor)
                                    {
                                        ok = false;
                                        break;
                                    }
                                }
                            }
                            index++;
                        }
                        if (ok)
                            return oc;
                }
            }
            return null;
        }

        private static NOcrChar NOcrFindBestMatch(Bitmap parentBitmap, ImageSplitterItem targetItem, int topMargin, out bool italic, List<NOcrChar> nOcrChars, double unItalicFactor, bool tryItalicScaling, bool deepSeek)
        {
            italic = false;
            var nbmp = targetItem.NikseBitmap;
            int index = 0;
            foreach (NOcrChar oc in nOcrChars)
            {
                if (Math.Abs(oc.Width - nbmp.Width) < 3 && Math.Abs(oc.Height - nbmp.Height) < 3 && Math.Abs(oc.MarginTop - topMargin) < 3)
                { // only very accurate matches

                    bool ok = true;
                    index = 0;
                    while (index < oc.LinesForeground.Count && ok)
                    {
                        NOcrPoint op = oc.LinesForeground[index];
                        foreach (Point point in op.ScaledGetPoints(oc, nbmp.Width, nbmp.Height))
                        {
                            if (point.X >= 0 && point.Y >= 0 && point.X < nbmp.Width && point.Y < nbmp.Height)
                            {
                                Color c = nbmp.GetPixel(point.X, point.Y);
                                if (c.A > 150 && c.R + c.G + c.B > NocrMinColor)
                                {
                                }
                                else
                                {
                                    Point p = new Point(point.X - 1, point.Y);
                                    if (p.X < 0)
                                        p.X = 1;
                                    c = nbmp.GetPixel(p.X, p.Y);
                                    if (nbmp.Width > 20 && c.A > 150 && c.R + c.G + c.B > NocrMinColor)
                                    {
                                    }
                                    else
                                    {
                                        ok = false;
                                        break;
                                    }
                                }
                            }
                        }
                        index++;
                    }
                    index = 0;
                    while (index < oc.LinesBackground.Count && ok)
                    {
                        NOcrPoint op = oc.LinesBackground[index];
                        foreach (Point point in op.ScaledGetPoints(oc, nbmp.Width, nbmp.Height))
                        {
                            if (point.X >= 0 && point.Y >= 0 && point.X < nbmp.Width && point.Y < nbmp.Height)
                            {
                                Color c = nbmp.GetPixel(point.X, point.Y);
                                if (c.A > 150 && c.R + c.G + c.B > NocrMinColor)
                                {
                                    Point p = new Point(point.X, point.Y);
                                    if (oc.Width > 19 && point.X > 0)
                                        p.X = p.X - 1;
                                    c = nbmp.GetPixel(p.X, p.Y);
                                    if (c.A > 150 && c.R + c.G + c.B > NocrMinColor)
                                    {
                                        ok = false;
                                        break;
                                    }
                                }
                            }
                        }
                        index++;
                    }
                    if (ok)
                        return oc;
                }
            }


            foreach (NOcrChar oc in nOcrChars)
            {
                int marginTopDiff = Math.Abs(oc.MarginTop - topMargin);
                if (Math.Abs(oc.Width - nbmp.Width) < 4 && Math.Abs(oc.Height - nbmp.Height) < 4 && marginTopDiff > 4 && marginTopDiff < 9)
                { // only very accurate matches - but not for margin top

                    bool ok = true;
                    index = 0;
                    while (index < oc.LinesForeground.Count && ok)
                    {
                        NOcrPoint op = oc.LinesForeground[index];
                        foreach (Point point in op.ScaledGetPoints(oc, nbmp.Width, nbmp.Height))
                        {
                            if (point.X >= 0 && point.Y >= 0 && point.X < nbmp.Width && point.Y < nbmp.Height)
                            {
                                Color c = nbmp.GetPixel(point.X, point.Y);
                                if (c.A > 150 && c.R + c.G + c.B > NocrMinColor)
                                {
                                }
                                else
                                {
                                    ok = false;
                                    break;
                                }
                            }
                        }
                        index++;
                    }
                    index = 0;
                    while (index < oc.LinesBackground.Count && ok)
                    {
                        NOcrPoint op = oc.LinesBackground[index];
                        foreach (Point point in op.ScaledGetPoints(oc, nbmp.Width, nbmp.Height))
                        {
                            if (point.X >= 0 && point.Y >= 0 && point.X < nbmp.Width && point.Y < nbmp.Height)
                            {
                                Color c = nbmp.GetPixel(point.X, point.Y);
                                if (c.A > 150 && c.R + c.G + c.B > NocrMinColor)
                                {
                                    ok = false;
                                    break;
                                }
                            }
                        }
                        index++;
                    }
                    if (ok)
                        return oc;
                }
            }

            // try some resize if aspect ratio is about the same
            double widthPercent = nbmp.Height * 100.0 / nbmp.Width;
            foreach (NOcrChar oc in nOcrChars)
            {
                if (!oc.IsSensitive)
                {
                    if (Math.Abs(oc.WidthPercent - widthPercent) < 15 && oc.Width > 12 && oc.Height > 19 && nbmp.Width > 19 && nbmp.Height > 12 && Math.Abs(oc.MarginTop - topMargin) < nbmp.Height / 4)
                    {
                        bool ok = true;
                        index = 0;
                        while (index < oc.LinesForeground.Count && ok)
                        {
                            NOcrPoint op = oc.LinesForeground[index];
                            foreach (Point point in op.ScaledGetPoints(oc, nbmp.Width, nbmp.Height))
                            {
                                if (point.X >= 0 && point.Y >= 0 && point.X < nbmp.Width && point.Y < nbmp.Height)
                                {
                                    Color c = nbmp.GetPixel(point.X, point.Y);
                                    if (c.A > 150 && c.R + c.G + c.B > NocrMinColor)
                                    {
                                    }
                                    else
                                    {
                                        ok = false;
                                        break;
                                    }
                                }
                            }
                            index++;
                        }
                        index = 0;
                        while (index < oc.LinesBackground.Count && ok)
                        {
                            NOcrPoint op = oc.LinesBackground[index];
                            foreach (Point point in op.ScaledGetPoints(oc, nbmp.Width, nbmp.Height))
                            {
                                if (point.X >= 0 && point.Y >= 0 && point.X < nbmp.Width && point.Y < nbmp.Height)
                                {
                                    Color c = nbmp.GetPixel(point.X, point.Y);
                                    if (c.A > 150 && c.R + c.G + c.B > NocrMinColor)
                                    {
                                        ok = false;
                                        break;
                                    }
                                }
                            }
                            index++;
                        }
                        if (ok)
                            return oc;
                    }
                }
            }

            // matches 2 or 3 pixels to the left
            foreach (NOcrChar oc in nOcrChars)
            {
                if (!oc.IsSensitive)
                {
                    if (Math.Abs(oc.WidthPercent - widthPercent) < 15 && oc.Width > 14 && oc.Height > 19 && nbmp.Width > 20 && nbmp.Height > 14 && Math.Abs(oc.MarginTop - topMargin) < nbmp.Height / 4)
                    {
                        bool ok = true;
                        index = 0;
                        while (index < oc.LinesForeground.Count && ok)
                        {
                            NOcrPoint op = oc.LinesForeground[index];
                            foreach (Point point in op.ScaledGetPoints(oc, nbmp.Width, nbmp.Height))
                            {
                                Point p = new Point(point.X - 2, point.Y);
                                Point p1 = new Point(point.X - 1, point.Y);
                                if (p.X >= 0 && p.Y >= 0 && p.X < nbmp.Width && p.Y < nbmp.Height && p1.X >= 0)
                                {
                                    Color c = nbmp.GetPixel(p.X, p.Y);
                                    if (c.A > 150 && c.R + c.G + c.B > NocrMinColor)
                                    {
                                    }
                                    else
                                    {
                                        c = nbmp.GetPixel(p1.X, p1.Y);
                                        if (c.A > 150 && c.R + c.G + c.B > NocrMinColor)
                                        {
                                        }
                                        else
                                        {
                                            ok = false;
                                            break;
                                        }
                                    }
                                }
                            }
                            index++;
                        }
                        index = 0;
                        while (index < oc.LinesBackground.Count && ok)
                        {
                            NOcrPoint op = oc.LinesBackground[index];
                            foreach (Point point in op.ScaledGetPoints(oc, nbmp.Width, nbmp.Height))
                            {
                                Point p = new Point(point.X - 2, point.Y);
                                Point p1 = new Point(point.X - 1, point.Y);
                                if (p.X >= 0 && p.Y >= 0 && p.X < nbmp.Width && point.Y < nbmp.Height && p1.X >= 0)
                                {
                                    Color c = nbmp.GetPixel(p.X, p.Y);
                                    if (c.A > 150 && c.R + c.G + c.B > NocrMinColor)
                                    {
                                        c = nbmp.GetPixel(p1.X, p1.Y);
                                        if (c.A > 150 && c.R + c.G + c.B > NocrMinColor)
                                        {
                                            ok = false;
                                            break;
                                        }
                                    }
                                }
                            }
                            index++;
                        }
                        if (ok)
                            return oc;
                    }
                }
            }

            // matches 5 pixels lower
            int yLower = 5;
            widthPercent = (nbmp.Height - yLower) * 100.0 / nbmp.Width;
            foreach (NOcrChar oc in nOcrChars)
            {
                if (!oc.IsSensitive)
                {
                    if (Math.Abs(oc.WidthPercent - widthPercent) < 20 && oc.Width > 12 && oc.Height > 19 && nbmp.Width > 19 && nbmp.Height > 12 && Math.Abs(oc.MarginTop - topMargin) < 15)
                    {
                        bool ok = true;
                        index = 0;
                        while (index < oc.LinesForeground.Count && ok)
                        {
                            NOcrPoint op = oc.LinesForeground[index];
                            foreach (Point point in op.ScaledGetPoints(oc, nbmp.Width, nbmp.Height - yLower))
                            {
                                if (point.X >= 0 && point.Y + yLower >= 0 && point.X < nbmp.Width && point.Y + yLower < nbmp.Height)
                                {
                                    Color c = nbmp.GetPixel(point.X, point.Y + yLower);
                                    if (c.A > 150 && c.R + c.G + c.B > NocrMinColor)
                                    {
                                    }
                                    else
                                    {
                                        ok = false;
                                        break;
                                    }
                                }
                            }
                            index++;
                        }
                        index = 0;
                        while (index < oc.LinesBackground.Count && ok)
                        {
                            NOcrPoint op = oc.LinesBackground[index];
                            foreach (Point point in op.ScaledGetPoints(oc, nbmp.Width, nbmp.Height - yLower))
                            {
                                if (point.X >= 0 && point.Y + yLower >= 0 && point.X < nbmp.Width && point.Y + yLower < nbmp.Height)
                                {
                                    Color c = nbmp.GetPixel(point.X, point.Y + yLower);
                                    if (c.A > 150 && c.R + c.G + c.B > NocrMinColor)
                                    {
                                        ok = false;
                                        break;
                                    }
                                }
                            }
                            index++;
                        }
                        if (ok)
                            return oc;
                    }
                }
            }


            if (deepSeek) // if we do now draw then just try anything...
            {
                widthPercent = nbmp.Height * 100.0 / nbmp.Width;

                foreach (NOcrChar oc in nOcrChars)
                {
                    if (Math.Abs(oc.WidthPercent - widthPercent) < 40 && oc.Height > 12 && oc.Width > 16 && nbmp.Width > 16 && nbmp.Height > 12 && Math.Abs(oc.MarginTop - topMargin) < 15)
                    {
                        bool ok = true;
                        foreach (NOcrPoint op in oc.LinesForeground)
                        {
                            foreach (Point point in op.ScaledGetPoints(oc, nbmp.Width, nbmp.Height))
                            {
                                if (point.X >= 0 && point.Y >= 0 && point.X < nbmp.Width && point.Y < nbmp.Height)
                                {
                                    Color c = nbmp.GetPixel(point.X, point.Y);
                                    if (c.A > 150 && c.R + c.G + c.B > NocrMinColor)
                                    {
                                    }
                                    else
                                    {
                                        ok = false;
                                        break;
                                    }
                                }
                            }
                        }
                        foreach (NOcrPoint op in oc.LinesBackground)
                        {
                            foreach (Point point in op.ScaledGetPoints(oc, nbmp.Width, nbmp.Height))
                            {
                                if (point.X >= 0 && point.Y >= 0 && point.X < nbmp.Width && point.Y < nbmp.Height)
                                {
                                    Color c = nbmp.GetPixel(point.X, point.Y);
                                    if (c.A > 150 && c.R + c.G + c.B > NocrMinColor)
                                    {
                                        ok = false;
                                        break;
                                    }
                                }
                            }
                        }
                        if (ok)
                            return oc;
                    }
                }

                foreach (NOcrChar oc in nOcrChars)
                {
                    if (Math.Abs(oc.WidthPercent - widthPercent) < 40 && oc.Height > 12 && oc.Width > 19 && nbmp.Width > 19 && nbmp.Height > 12 && Math.Abs(oc.MarginTop - topMargin) < 15)
                    {
                        bool ok = true;
                        foreach (NOcrPoint op in oc.LinesForeground)
                        {
                            foreach (Point point in op.ScaledGetPoints(oc, nbmp.Width - 3, nbmp.Height))
                            {
                                if (point.X >= 0 && point.Y >= 0 && point.X < nbmp.Width && point.Y < nbmp.Height)
                                {
                                    Color c = nbmp.GetPixel(point.X, point.Y);
                                    if (c.A > 150 && c.R + c.G + c.B > NocrMinColor)
                                    {
                                    }
                                    else
                                    {
                                        ok = false;
                                        break;
                                    }
                                }
                            }
                        }
                        foreach (NOcrPoint op in oc.LinesBackground)
                        {
                            foreach (Point point in op.ScaledGetPoints(oc, nbmp.Width - 3, nbmp.Height))
                            {
                                if (point.X >= 0 && point.Y >= 0 && point.X < nbmp.Width && point.Y < nbmp.Height)
                                {
                                    Color c = nbmp.GetPixel(point.X, point.Y);
                                    if (c.A > 150 && c.R + c.G + c.B > NocrMinColor)
                                    {
                                        ok = false;
                                        break;
                                    }
                                }
                            }
                        }
                        if (ok)
                            return oc;
                    }
                }

                foreach (NOcrChar oc in nOcrChars)
                {
                    if (Math.Abs(oc.WidthPercent - widthPercent) < 40 && oc.Height > 12 && oc.Width > 19 && nbmp.Width > 19 && nbmp.Height > 12 && Math.Abs(oc.MarginTop - topMargin) < 15)
                    {
                        bool ok = true;
                        foreach (NOcrPoint op in oc.LinesForeground)
                        {
                            foreach (Point point in op.ScaledGetPoints(oc, nbmp.Width, nbmp.Height - 4))
                            {
                                if (point.X >= 0 && point.Y + 4 >= 0 && point.X < nbmp.Width && point.Y + 4 < nbmp.Height)
                                {
                                    Color c = nbmp.GetPixel(point.X, point.Y + 4);
                                    if (c.A > 150 && c.R + c.G + c.B > NocrMinColor)
                                    {
                                    }
                                    else
                                    {
                                        ok = false;
                                        break;
                                    }
                                }
                            }
                        }
                        foreach (NOcrPoint op in oc.LinesBackground)
                        {
                            foreach (Point point in op.ScaledGetPoints(oc, nbmp.Width, nbmp.Height - 4))
                            {
                                if (point.X >= 0 && point.Y + 4 >= 0 && point.X < nbmp.Width && point.Y + 4 < nbmp.Height)
                                {
                                    Color c = nbmp.GetPixel(point.X, point.Y + 4);
                                    if (c.A > 150 && c.R + c.G + c.B > NocrMinColor)
                                    {
                                        ok = false;
                                        break;
                                    }
                                }
                            }
                        }
                        if (ok)
                            return oc;
                    }
                }

            }

            if (tryItalicScaling)
            {
              //  int left = targetItem.X;
              //  int width = targetItem.Bitmap.Width;
              //  //if (left > 3)
              //  //{
              //  //    left -= 3;
              //  //    width += 3;
              //  //}
              //  var temp = ImageSplitter.Copy(parentBitmap, new Rectangle(left, targetItem.Y, width , targetItem.Bitmap.Height));
              //  var bitmap2 = UnItalic(temp, unItalicFactor);
              //  //var nbmpUnItalic = new NikseBitmap(unItalicedBmp);
              //  //nbmpUnItalic.ReplaceNonWhiteWithTransparent();
              //  //Bitmap bitmap2 = nbmpUnItalic.GetBitmap();
              ////  bitmap2.Save(@"D:\Download\__" + Guid.NewGuid().ToString() + ".bmp");
              //  var list = ImageSplitter.SplitBitmapToLetters(bitmap2, 10, false, false);
              //  var matches = new List<NOcrChar>();
              //  bool unitalicOk = true;
              //  foreach (var spi in list)
              //  {
              //      var m = NOcrFindBestMatch(parentBitmap, spi, topMargin, out italic, nOcrChars, unItalicFactor, false, true);
              //      if (m == null)
              //      {
              //          if (spi.Bitmap.Width > 2)
              //          {
              //              unitalicOk = false;
              //              break;
              //          }
              //      }
              //      else
              //      {
              //          matches.Add(m);
              //      }
              //  }

              //  if (unitalicOk && matches.Count > 0)
              //  {
              //      italic = true;
              //      if (matches.Count == 1)
              //      {
              //          return matches[0];
              //      }
              //      else if (matches.Count > 1)
              //      {
              //          NOcrChar c = new NOcrChar(matches[0]);
              //          c.LinesBackground.Clear();
              //          c.LinesForeground.Clear();
              //          c.Text = string.Empty;
              //          foreach (var m in matches)
              //              c.Text += m.Text;
              //          return c;
              //      }
              //  }



                int maxMoveLeft = 9;
                if (nbmp.Width < 20)
                    maxMoveLeft = 7;
                if (nbmp.Width < 16)
                    maxMoveLeft = 4;
                for (int movePixelsLeft = 0; movePixelsLeft < maxMoveLeft; movePixelsLeft++)
                {
                    foreach (NOcrChar oc in nOcrChars)
                    {
                        if (Math.Abs(oc.WidthPercent - widthPercent) < 99 && oc.Width > 10 && nbmp.Width > 10)
                        {
                            bool ok = true;
                            var o = MakeItalicNOcrChar(oc, movePixelsLeft, unItalicFactor);
                            index = 0;
                            while (index < o.LinesForeground.Count && ok)
                            {
                                NOcrPoint op = o.LinesForeground[index];
                                foreach (Point p in op.ScaledGetPoints(o, nbmp.Width, nbmp.Height))
                                {
                                    if (p.X >= 2 && p.Y >= 2 && p.X < nbmp.Width && p.Y < nbmp.Height)
                                    {
                                        Color c = nbmp.GetPixel(p.X, p.Y);
                                        if (c.A > 150 && c.R + c.G + c.B > NocrMinColor)
                                        {
                                        }
                                        else
                                        {
                                            ok = false;
                                            break;
                                        }
                                    }
                                }
                                index++;
                            }

                            index = 0;
                            while (index < o.LinesBackground.Count && ok)
                            {
                                NOcrPoint op = o.LinesBackground[index];
                                foreach (Point p in op.ScaledGetPoints(o, nbmp.Width, nbmp.Height))
                                {
                                    if (p.X >= 0 && p.Y >= 0 && p.X < nbmp.Width && p.Y < nbmp.Height)
                                    {
                                        Color c = nbmp.GetPixel(p.X, p.Y);
                                        if (c.A > 150 && c.R + c.G + c.B > NocrMinColor)
                                        {
                                            ok = false;
                                            break;
                                        }
                                    }
                                }
                                index++;
                            }
                            if (ok)
                            {
                                italic = true;
                                return o;
                            }
                        }
                    }
                }
                for (int movePixelsLeft = 0; movePixelsLeft < maxMoveLeft; movePixelsLeft++)
                {
                    foreach (NOcrChar oc in nOcrChars)
                    {
                        if (Math.Abs(oc.WidthPercent - widthPercent) < 99 && oc.Width > 10 && nbmp.Width > 10)
                        {
                            bool ok = true;
                            var o = MakeItalicNOcrChar(oc, movePixelsLeft, unItalicFactor);
                            index = 0;
                            while (index < o.LinesForeground.Count && ok)
                            {
                                NOcrPoint op = o.LinesForeground[index];
                                foreach (Point p in op.ScaledGetPoints(o, nbmp.Width - 4, nbmp.Height))
                                {
                                    if (p.X >= 2 && p.Y >= 2 && p.X < nbmp.Width && p.Y < nbmp.Height)
                                    {
                                        Color c = nbmp.GetPixel(p.X, p.Y);
                                        if (c.A > 150 && c.R + c.G + c.B > NocrMinColor)
                                        {
                                        }
                                        else
                                        {
                                            ok = false;
                                            break;
                                        }
                                    }
                                }
                                index++;
                            }
                            index = 0;
                            while (index < o.LinesBackground.Count && ok)
                            {
                                NOcrPoint op = o.LinesBackground[index];
                                foreach (Point p in op.ScaledGetPoints(o, nbmp.Width-4, nbmp.Height))
                                {
                                    if (p.X >= 0 && p.Y >= 0 && p.X < nbmp.Width && p.Y < nbmp.Height)
                                    {
                                        Color c = nbmp.GetPixel(p.X, p.Y);
                                        if (c.A > 150 && c.R + c.G + c.B > NocrMinColor)
                                        {
                                            ok = false;
                                            break;
                                        }
                                    }
                                }
                                index++;
                            }
                            if (ok)
                            {
                                italic = true;
                                return o;
                            }
                        }
                    }
                }
                for (int movePixelsLeft = 0; movePixelsLeft < maxMoveLeft; movePixelsLeft++)
                {
                    foreach (NOcrChar oc in nOcrChars)
                    {
                        if (Math.Abs(oc.WidthPercent - widthPercent) < 99 && oc.Width > 10 && nbmp.Width > 10)
                        {
                            bool ok = true;
                            var o = MakeItalicNOcrChar(oc, movePixelsLeft, unItalicFactor);
                            index = 0;
                            while (index < o.LinesForeground.Count && ok)
                            {
                                NOcrPoint op = o.LinesForeground[index];
                                foreach (Point p in op.ScaledGetPoints(o, nbmp.Width - 6, nbmp.Height))
                                {
                                    if (p.X >= 2 && p.Y >= 2 && p.X < nbmp.Width && p.Y < nbmp.Height)
                                    {
                                        Color c = nbmp.GetPixel(p.X, p.Y);
                                        if (c.A > 150 && c.R + c.G + c.B > NocrMinColor)
                                        {
                                        }
                                        else
                                        {
                                            ok = false;
                                            break;
                                        }
                                    }
                                }
                                index++;
                            }
                            index = 0;
                            while (index < o.LinesBackground.Count && ok)
                            {
                                NOcrPoint op = o.LinesBackground[index];
                                foreach (Point p in op.ScaledGetPoints(o, nbmp.Width - 6, nbmp.Height))
                                {
                                    if (p.X >= 0 && p.Y >= 0 && p.X < nbmp.Width && p.Y < nbmp.Height)
                                    {
                                        Color c = nbmp.GetPixel(p.X, p.Y);
                                        if (c.A > 150 && c.R + c.G + c.B > NocrMinColor)
                                        {
                                            ok = false;
                                            break;
                                        }
                                    }
                                }
                                index++;
                            }
                            if (ok)
                            {
                                italic = true;
                                return o;
                            }
                        }
                    }
                }
            }

            return null;
        }

        private static NOcrChar MakeItalicNOcrChar(NOcrChar oldChar, int movePixelsLeft, double unItalicFactor)
        {
            var c = new NOcrChar();
            foreach (NOcrPoint op in oldChar.LinesForeground)
            {
                c.LinesForeground.Add(new NOcrPoint(MakePointItalic(op.Start, oldChar.Height, movePixelsLeft, unItalicFactor), MakePointItalic(op.End, oldChar.Height, movePixelsLeft, unItalicFactor)));
            }
            foreach (NOcrPoint op in oldChar.LinesBackground)
            {
                c.LinesBackground.Add(new NOcrPoint(MakePointItalic(op.Start, oldChar.Height, movePixelsLeft, unItalicFactor), MakePointItalic(op.End, oldChar.Height, movePixelsLeft, unItalicFactor)));
            }
            c.Text = oldChar.Text;
            c.Width = oldChar.Width;
            c.Height = oldChar.Height;
            c.MarginTop = oldChar.MarginTop;
            c.Italic = true;
            return c;
        }

        internal CompareMatch GetNOcrCompareMatch(ImageSplitterItem targetItem, Bitmap parentBitmap, List<NOcrChar> nOcrChars, double unItalicFactor, bool tryItalicScaling, bool deepSeek)
        {
            bool italic;
            var expandedResult = NOcrFindExpandedMatch(parentBitmap, targetItem, targetItem.Y - targetItem.ParentY, nOcrChars);
            if (expandedResult != null)
                return new CompareMatch(expandedResult.Text, expandedResult.Italic, expandedResult.ExpandCount, null, expandedResult);

            var result = NOcrFindBestMatch(parentBitmap, targetItem, targetItem.Y - targetItem.ParentY, out italic, nOcrChars, unItalicFactor, tryItalicScaling, deepSeek);
            if (result == null)
            {
                if (checkBoxNOcrCorrect.Checked)
                    return null;
                else
                    return new CompareMatch("*", false, 0, null);
            }

            // Fix uppercase/lowercase issues (not I/l)
            if (result.Text == "e")
                _nocrLastLowercaseHeight = targetItem.NikseBitmap.Height;
            else if (_nocrLastLowercaseHeight == -1 && result.Text == "a")
                _nocrLastLowercaseHeight = targetItem.NikseBitmap.Height;

            if (result.Text == "E" || result.Text == "H" || result.Text == "R" || result.Text == "D" || result.Text == "T")
                _nocrLastUppercaseHeight = targetItem.NikseBitmap.Height;
            else if (_nocrLastUppercaseHeight == -1 && result.Text == "M")
                _nocrLastUppercaseHeight = targetItem.NikseBitmap.Height;

            if (result.Text == "V" || result.Text == "W" || result.Text == "U" || result.Text == "S" || result.Text == "Z" || result.Text == "O" || result.Text == "X" || result.Text == "Ø" || result.Text == "C")
            {
                if (_nocrLastLowercaseHeight > 3 && targetItem.NikseBitmap.Height - _nocrLastLowercaseHeight < 2)
                    result.Text = result.Text.ToLower();
            }
            else if (result.Text == "v" || result.Text == "w" || result.Text == "u" || result.Text == "s" || result.Text == "z" || result.Text == "o" || result.Text == "x" || result.Text == "ø" || result.Text == "c")
            {
                if (_nocrLastUppercaseHeight > 3 && _nocrLastUppercaseHeight - targetItem.NikseBitmap.Height < 2)
                    result.Text = result.Text.ToUpper();
            }

            if (italic)
                return new CompareMatch(result.Text, true, 0, null, result);
            else
                return new CompareMatch(result.Text, result.Italic, 0, null, result);
        }

        internal static CompareMatch GetNOcrCompareMatch(ImageSplitterItem targetItem, Bitmap parentBitmap, NOcrThreadParameter p)
        {
            bool italic;
            var expandedResult = NOcrFindExpandedMatch(parentBitmap, targetItem, targetItem.Y - targetItem.ParentY, p.NOcrChars);
            if (expandedResult != null)
                return new CompareMatch(expandedResult.Text, expandedResult.Italic, expandedResult.ExpandCount, null, expandedResult);

            var result = NOcrFindBestMatch(parentBitmap, targetItem, targetItem.Y - targetItem.ParentY, out italic, p.NOcrChars, p.UnItalicFactor, p.AdvancedItalicDetection, true);
            if (result == null)
                return null;

            // Fix uppercase/lowercase issues (not I/l)
            if (result.Text == "e")
                p.NOcrLastLowercaseHeight = targetItem.NikseBitmap.Height;
            else if (p.NOcrLastLowercaseHeight == -1 && result.Text == "a")
                p.NOcrLastLowercaseHeight = targetItem.NikseBitmap.Height;

            if (result.Text == "E" || result.Text == "H" || result.Text == "R" || result.Text == "D" || result.Text == "T")
                p.NOcrLastUppercaseHeight = targetItem.NikseBitmap.Height;
            else if (p.NOcrLastUppercaseHeight == -1 && result.Text == "M")
                p.NOcrLastUppercaseHeight = targetItem.NikseBitmap.Height;

            if (result.Text == "V" || result.Text == "W" || result.Text == "U" || result.Text == "S" || result.Text == "Z" || result.Text == "O" || result.Text == "X" || result.Text == "Ø" || result.Text == "C")
            {
                if (p.NOcrLastLowercaseHeight > 3 && targetItem.NikseBitmap.Height - p.NOcrLastLowercaseHeight < 2)
                    result.Text = result.Text.ToLower();
            }
            else if (result.Text == "v" || result.Text == "w" || result.Text == "u" || result.Text == "s" || result.Text == "z" || result.Text == "o" || result.Text == "x" || result.Text == "ø" || result.Text == "c")
            {
                if (p.NOcrLastUppercaseHeight > 3 && p.NOcrLastUppercaseHeight - targetItem.NikseBitmap.Height < 2)
                    result.Text = result.Text.ToUpper();
            }

            if (italic)
                return new CompareMatch(result.Text, true, 0, null, result);
            else
                return new CompareMatch(result.Text, result.Italic, 0, null, result);
        }

        private CompareMatch GetCompareMatch(ImageSplitterItem targetItem, NikseBitmap parentBitmap, out CompareMatch secondBestGuess)
        {
            secondBestGuess = null;
            int index = 0;
            int smallestDifference = 10000;
            int smallestIndex = -1;
            NikseBitmap target = targetItem.NikseBitmap;
            if (_compareBitmaps == null)
            {
                secondBestGuess = null;
                return null;
            }

            foreach (CompareItem compareItem in _compareBitmaps)
            {
                // check for expand match!
                if (compareItem.ExpandCount > 0 && compareItem.Bitmap.Width > target.Width &&
                    parentBitmap.Width >= compareItem.Bitmap.Width + targetItem.X &&
                    parentBitmap.Height >= compareItem.Bitmap.Height + targetItem.Y)
                {
                    var cutBitmap = parentBitmap.CopyRectangle(new Rectangle(targetItem.X, targetItem.Y, compareItem.Bitmap.Width, compareItem.Bitmap.Height));
                    int dif = NikseBitmapImageSplitter.IsBitmapsAlike(compareItem.Bitmap, cutBitmap);
                    if (dif < smallestDifference)
                    {
                        bool allow = true;
                        if (Math.Abs(target.Height - compareItem.Bitmap.Height) > 5 && compareItem.Text == "\"")
                            allow = false;
                        if (allow)
                        {
                            smallestDifference = dif;
                            smallestIndex = index;
                            if (dif == 0)
                                break; // foreach ending
                        }
                    }
                }
                index++;
            }

            // Search images with minor location changes
            FindBestMatch(ref index, ref smallestDifference, ref smallestIndex, target, _compareBitmaps);

            if (smallestDifference * 100.0 / (target.Width * target.Height) > _vobSubOcrSettings.AllowDifferenceInPercent && target.Width < 70)
            {
                if (smallestDifference > 2 && target.Width > 25)
                {
                    var cutBitmap = target.CopyRectangle(new Rectangle(4, 0, target.Width - 4, target.Height));
                    FindBestMatch(ref index, ref smallestDifference, ref smallestIndex, cutBitmap, _compareBitmaps);
                    double differencePercentage = smallestDifference * 100.0 / (target.Width * target.Height);
                }

                if (smallestDifference > 2 && target.Width > 12)
                {
                    var cutBitmap = target.CopyRectangle(new Rectangle(1, 0, target.Width - 2, target.Height));
                    FindBestMatch(ref index, ref smallestDifference, ref smallestIndex, cutBitmap, _compareBitmaps);
                }

                if (smallestDifference > 2 && target.Width > 12)
                {
                    var cutBitmap = target.CopyRectangle(new Rectangle(0, 0, target.Width - 2, target.Height));
                    FindBestMatch(ref index, ref smallestDifference, ref smallestIndex, cutBitmap, _compareBitmaps);
                }

                if (smallestDifference > 2 && target.Width > 12)
                {
                    var cutBitmap = target.CopyRectangle(new Rectangle(1, 0, target.Width - 2, target.Height));
                    int topCrop = 0;
                    var cutBitmap2 = NikseBitmapImageSplitter.CropTopAndBottom(cutBitmap, out topCrop, 2);
                    if (cutBitmap2.Height != target.Height)
                        FindBestMatch(ref index, ref smallestDifference, ref smallestIndex, cutBitmap2, _compareBitmaps);
                }

                if (smallestDifference > 2 && target.Width > 15)
                {
                    var cutBitmap = target.CopyRectangle(new Rectangle(1, 0, target.Width - 2, target.Height));
                    int topCrop = 0;
                    var cutBitmap2 = NikseBitmapImageSplitter.CropTopAndBottom(cutBitmap, out topCrop);
                    if (cutBitmap2.Height != target.Height)
                        FindBestMatch(ref index, ref smallestDifference, ref smallestIndex, cutBitmap2, _compareBitmaps);
                }

                if (smallestDifference > 2 && target.Width > 15)
                {
                    var cutBitmap = target.CopyRectangle(new Rectangle(1, 0, target.Width - 2, target.Height));
                    int topCrop = 0;
                    var cutBitmap2 = NikseBitmapImageSplitter.CropTopAndBottom(cutBitmap, out topCrop);
                    if (cutBitmap2.Height != target.Height)
                        FindBestMatch(ref index, ref smallestDifference, ref smallestIndex, cutBitmap2, _compareBitmaps);
                }
            }

            if (smallestIndex >= 0)
            {
                double differencePercentage = smallestDifference * 100.0 / (target.Width * target.Height);
                double maxDiff = (double)numericUpDownMaxErrorPct.Value;
                if (differencePercentage <= maxDiff)
                {
                    var hit = _compareBitmaps[smallestIndex];
                    return new CompareMatch(hit.Text, hit.Italic, hit.ExpandCount, hit.Name);
                }

                var guess = _compareBitmaps[smallestIndex];
                secondBestGuess = new CompareMatch(guess.Text, guess.Italic, guess.ExpandCount, guess.Name);
            }

            return null;
        }

        static public Bitmap CopyBitmapSection(Bitmap srcBitmap, Rectangle section)
        {
            Bitmap bmp = new Bitmap(section.Width, section.Height);
            Graphics g = Graphics.FromImage(bmp);
            g.DrawImage(srcBitmap, 0, 0, section, GraphicsUnit.Pixel);
            g.Dispose();
            return bmp;
        }

        private static void FindBestMatch(ref int index, ref int smallestDifference, ref int smallestIndex, NikseBitmap target, List<CompareItem> compareBitmaps)
        {
            int numberOfForegroundColors = CalculateNumberOfForegroundColors(target);
            int minForeColorMatch = 90;

            index = 0;
            foreach (CompareItem compareItem in compareBitmaps)
            {
                if (compareItem.Bitmap.Width == target.Width && compareItem.Bitmap.Height == target.Height) // precise math in size
                {
                    if (compareItem.NumberOfForegroundColors < 1)
                        compareItem.NumberOfForegroundColors = CalculateNumberOfForegroundColors(compareItem.Bitmap);

                    if (Math.Abs(compareItem.NumberOfForegroundColors - numberOfForegroundColors) < 3)
                    {
                        int dif = NikseBitmapImageSplitter.IsBitmapsAlike(compareItem.Bitmap, target);
                        if (dif < smallestDifference)
                        {
                            smallestDifference = dif;
                            smallestIndex = index;
                            if (dif < 3)
                            {
                                break; // foreach ending
                            }
                        }
                    }
                }
                index++;
            }

            if (smallestDifference > 1)
            {
                index = 0;
                foreach (CompareItem compareItem in compareBitmaps)
                {
                    if (compareItem.Bitmap.Width == target.Width && compareItem.Bitmap.Height == target.Height) // precise math in size
                    {
                        if (compareItem.NumberOfForegroundColors < 1)
                            compareItem.NumberOfForegroundColors = CalculateNumberOfForegroundColors(compareItem.Bitmap);

                        if (Math.Abs(compareItem.NumberOfForegroundColors - numberOfForegroundColors) < 40)
                        {
                            int dif = NikseBitmapImageSplitter.IsBitmapsAlike(compareItem.Bitmap, target);
                            if (dif < smallestDifference)
                            {
                                smallestDifference = dif;
                                smallestIndex = index;
                                if (dif == 0)
                                {
                                    break; // foreach ending
                                }
                            }
                        }
                    }
                    index++;
                }
            }

            if (target.Width > 5 && smallestDifference > 2) // for other than very narrow letter (like 'i' and 'l' and 'I'), try more sizes
            {
                index = 0;
                foreach (CompareItem compareItem in compareBitmaps)
                {
                    if (compareItem.Bitmap.Width == target.Width && compareItem.Bitmap.Height == target.Height - 1)
                    {
                        if (compareItem.NumberOfForegroundColors == -1)
                            compareItem.NumberOfForegroundColors = CalculateNumberOfForegroundColors(compareItem.Bitmap);

                        if (Math.Abs(compareItem.NumberOfForegroundColors - numberOfForegroundColors) < minForeColorMatch)
                        {
                            int dif = NikseBitmapImageSplitter.IsBitmapsAlike(compareItem.Bitmap, target);
                            if (dif < smallestDifference)
                            {
                                smallestDifference = dif;
                                smallestIndex = index;
                                if (dif == 0)
                                    break; // foreach ending
                            }
                        }
                    }
                    index++;
                }

                if (smallestDifference > 2)
                {
                    index = 0;
                    foreach (CompareItem compareItem in compareBitmaps)
                    {
                        if (compareItem.Bitmap.Width == target.Width && compareItem.Bitmap.Height == target.Height + 1)
                        {
                            if (compareItem.NumberOfForegroundColors == -1)
                                compareItem.NumberOfForegroundColors = CalculateNumberOfForegroundColors(compareItem.Bitmap);

                            if (Math.Abs(compareItem.NumberOfForegroundColors - numberOfForegroundColors) < minForeColorMatch)
                            {
                                int dif = NikseBitmapImageSplitter.IsBitmapsAlike(target, compareItem.Bitmap);
                                if (dif < smallestDifference)
                                {
                                    smallestDifference = dif;
                                    smallestIndex = index;
                                    if (dif == 0)
                                        break; // foreach ending
                                }
                            }
                        }
                        index++;
                    }
                }

                if (smallestDifference > 3)
                {
                    index = 0;
                    foreach (CompareItem compareItem in compareBitmaps)
                    {
                        if (compareItem.Bitmap.Width == target.Width + 1 && compareItem.Bitmap.Height == target.Height + 1)
                        {
                            if (compareItem.NumberOfForegroundColors == -1)
                                compareItem.NumberOfForegroundColors = CalculateNumberOfForegroundColors(compareItem.Bitmap);

                            if (Math.Abs(compareItem.NumberOfForegroundColors - numberOfForegroundColors) < minForeColorMatch)
                            {
                                int dif = NikseBitmapImageSplitter.IsBitmapsAlike(target, compareItem.Bitmap);
                                if (dif < smallestDifference)
                                {
                                    smallestDifference = dif;
                                    smallestIndex = index;
                                    if (dif == 0)
                                        break; // foreach ending
                                }
                            }
                        }
                        index++;
                    }
                }

                if (smallestDifference > 5)
                {
                    index = 0;
                    foreach (CompareItem compareItem in compareBitmaps)
                    {
                        if (compareItem.Bitmap.Width == target.Width - 1 && compareItem.Bitmap.Height == target.Height)
                        {
                            if (compareItem.NumberOfForegroundColors == -1)
                                compareItem.NumberOfForegroundColors = CalculateNumberOfForegroundColors(compareItem.Bitmap);

                            if (Math.Abs(compareItem.NumberOfForegroundColors - numberOfForegroundColors) < minForeColorMatch)
                            {
                                int dif = NikseBitmapImageSplitter.IsBitmapsAlike(compareItem.Bitmap, target);
                                if (dif < smallestDifference)
                                {
                                    smallestDifference = dif;
                                    smallestIndex = index;
                                    if (dif == 0)
                                        break; // foreach ending
                                }
                            }
                        }
                        index++;
                    }
                }

                if (smallestDifference > 5)
                {
                    index = 0;
                    foreach (CompareItem compareItem in compareBitmaps)
                    {
                        if (compareItem.Bitmap.Width == target.Width - 1 && compareItem.Bitmap.Height == target.Height - 1)
                        {
                            if (compareItem.NumberOfForegroundColors == -1)
                                compareItem.NumberOfForegroundColors = CalculateNumberOfForegroundColors(compareItem.Bitmap);

                            if (Math.Abs(compareItem.NumberOfForegroundColors - numberOfForegroundColors) < minForeColorMatch)
                            {
                                int dif = NikseBitmapImageSplitter.IsBitmapsAlike(compareItem.Bitmap, target);
                                if (dif < smallestDifference)
                                {
                                    smallestDifference = dif;
                                    smallestIndex = index;
                                    if (dif == 0)
                                        break; // foreach ending
                                }
                            }
                        }
                        index++;
                    }
                }

                if (smallestDifference > 5)
                {
                    index = 0;
                    foreach (CompareItem compareItem in compareBitmaps)
                    {
                        if (compareItem.Bitmap.Width - 1 == target.Width && compareItem.Bitmap.Height == target.Height)
                        {
                            if (compareItem.NumberOfForegroundColors == -1)
                                compareItem.NumberOfForegroundColors = CalculateNumberOfForegroundColors(compareItem.Bitmap);

                            if (Math.Abs(compareItem.NumberOfForegroundColors - numberOfForegroundColors) < minForeColorMatch)
                            {
                                int dif = NikseBitmapImageSplitter.IsBitmapsAlike(target, compareItem.Bitmap);
                                if (dif < smallestDifference)
                                {
                                    smallestDifference = dif;
                                    smallestIndex = index;
                                    if (dif == 0)
                                        break; // foreach ending
                                }
                            }
                        }
                        index++;
                    }
                }

                if (smallestDifference > 9 && target.Width > 10)
                {
                    index = 0;
                    foreach (CompareItem compareItem in compareBitmaps)
                    {
                        if (compareItem.Bitmap.Width == target.Width - 2 && compareItem.Bitmap.Height == target.Height)
                        {
                            if (compareItem.NumberOfForegroundColors == -1)
                                compareItem.NumberOfForegroundColors = CalculateNumberOfForegroundColors(compareItem.Bitmap);

                            if (Math.Abs(compareItem.NumberOfForegroundColors - numberOfForegroundColors) < minForeColorMatch)
                            {
                                int dif = NikseBitmapImageSplitter.IsBitmapsAlike(compareItem.Bitmap, target);
                                if (dif < smallestDifference)
                                {
                                    smallestDifference = dif;
                                    smallestIndex = index;
                                    if (dif == 0)
                                        break; // foreach ending
                                }
                            }
                        }
                        index++;
                    }
                }

                if (smallestDifference > 9 && target.Width > 12)
                {
                    index = 0;
                    foreach (CompareItem compareItem in compareBitmaps)
                    {
                        if (compareItem.Bitmap.Width == target.Width - 3 && compareItem.Bitmap.Height == target.Height)
                        {
                            if (compareItem.NumberOfForegroundColors == -1)
                                compareItem.NumberOfForegroundColors = CalculateNumberOfForegroundColors(compareItem.Bitmap);

                            if (Math.Abs(compareItem.NumberOfForegroundColors - numberOfForegroundColors) < minForeColorMatch)
                            {
                                int dif = NikseBitmapImageSplitter.IsBitmapsAlike(compareItem.Bitmap, target);
                                if (dif < smallestDifference)
                                {
                                    smallestDifference = dif;
                                    smallestIndex = index;
                                    if (dif == 0)
                                        break; // foreach ending
                                }
                            }
                        }
                        index++;
                    }
                }

                if (smallestDifference > 9 && target.Width > 12)
                {
                    index = 0;
                    foreach (CompareItem compareItem in compareBitmaps)
                    {
                        if (compareItem.Bitmap.Width == target.Width && compareItem.Bitmap.Height == target.Height - 3)
                        {
                            if (compareItem.NumberOfForegroundColors == -1)
                                compareItem.NumberOfForegroundColors = CalculateNumberOfForegroundColors(compareItem.Bitmap);

                            if (Math.Abs(compareItem.NumberOfForegroundColors - numberOfForegroundColors) < minForeColorMatch)
                            {
                                int dif = NikseBitmapImageSplitter.IsBitmapsAlike(compareItem.Bitmap, target);
                                if (dif < smallestDifference)
                                {
                                    smallestDifference = dif;
                                    smallestIndex = index;
                                    if (dif == 0)
                                        break; // foreach ending
                                }
                            }
                        }
                        index++;
                    }
                }

                if (smallestDifference > 9)
                {
                    index = 0;
                    foreach (CompareItem compareItem in compareBitmaps)
                    {
                        if (compareItem.Bitmap.Width - 2 == target.Width && compareItem.Bitmap.Height == target.Height)
                        {
                            if (compareItem.NumberOfForegroundColors == -1)
                                compareItem.NumberOfForegroundColors = CalculateNumberOfForegroundColors(compareItem.Bitmap);

                            if (Math.Abs(compareItem.NumberOfForegroundColors - numberOfForegroundColors) < minForeColorMatch)
                            {
                                int dif = NikseBitmapImageSplitter.IsBitmapsAlike(target, compareItem.Bitmap);
                                if (dif < smallestDifference)
                                {
                                    smallestDifference = dif;
                                    smallestIndex = index;
                                    if (dif == 0)
                                        break; // foreach ending
                                }
                            }
                        }
                        index++;
                    }
                }
            }

            if (smallestDifference == 0)
            {
                if (smallestIndex > 200)
                {
                    CompareItem hit = compareBitmaps[smallestIndex];
                    compareBitmaps.RemoveAt(smallestIndex);
                    compareBitmaps.Insert(0, hit);
                    smallestIndex = 0;
                    index = 0;
                }
            }
        }

        private unsafe static int CalculateNumberOfForegroundColors(NikseBitmap nikseBitmap)
        {
            int count = 0;
            for (int y = 0; y < nikseBitmap.Height; y++)
            {
                for (int x = 0; x < nikseBitmap.Width; x++)
                {
                    Color c = nikseBitmap.GetPixel(x, y);
                    if (c.A > 100 && c.R + c.G + c.B > 200)
                        count++;
                }
            }
            return count;
        }

        private static int CalculateNumberOfForegroundColors(ManagedBitmap managedBitmap)
        {
            int count = 0;
            for (int y = 0; y < managedBitmap.Height; y++)
            {
                for (int x = 0; x < managedBitmap.Width; x++)
                {
                    Color c = managedBitmap.GetPixel(x, y);
                    if (c.A > 100 && c.R + c.G + c.B > 200)
                        count++;
                }
            }
            return count;
        }

        private string SaveCompareItem(NikseBitmap newTarget, string text, bool isItalic, int expandCount)
        {
            string path = Configuration.VobSubCompareFolder + comboBoxCharacterDatabase.SelectedItem + Path.DirectorySeparatorChar;
            string databaseName = path + "Images.db";
            FileStream f;
            long pos = 0;
            if (!File.Exists(databaseName))
            {
                using (f = new FileStream(databaseName, FileMode.Create))
                {
                    pos = f.Position;
                    new ManagedBitmap(newTarget).AppendToStream(f);
                }
            }
            else
            {
                using (f = new FileStream(databaseName, FileMode.Append))
                {
                    pos = f.Position;
                    new ManagedBitmap(newTarget).AppendToStream(f);
                }
            }
            string name = pos.ToString(CultureInfo.InvariantCulture);

            _compareBitmaps.Add(new CompareItem(new ManagedBitmap(newTarget), name, isItalic, expandCount, text));

            XmlElement element = _compareDoc.CreateElement("Item");
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
            element.InnerText = pos.ToString(CultureInfo.InvariantCulture);
            _compareDoc.DocumentElement.AppendChild(element);
            _compareDoc.Save(path + "Images.xml");
            return name;
        }

        /// <summary>
        /// Ocr via image compare
        /// </summary>
        private string SplitAndOcrBitmapNormal(Bitmap bitmap, int listViewIndex)
        {
            if (_ocrFixEngine == null)
                LoadOcrFixEngine(null, LanguageString);

            string threadText = null;
            if (_icThreadResults != null && !string.IsNullOrEmpty(_icThreadResults[listViewIndex]))
                threadText = _icThreadResults[listViewIndex];

            string line = string.Empty;
            if (threadText == null)
            {
                var matches = new List<CompareMatch>();
                NikseBitmap parentBitmap = new NikseBitmap(bitmap);
                List<ImageSplitterItem> list = NikseBitmapImageSplitter.SplitBitmapToLetters(parentBitmap, (int)numericUpDownPixelsIsSpace.Value, checkBoxRightToLeft.Checked, Configuration.Settings.VobSubOcr.TopToBottom);
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
                        else if (index + 1 < list.Count && list[index + 1].NikseBitmap != null) // only allow expand to EndOfLine or space
                        {

                            index++;
                            expandSelectionList.Add(list[index]);
                        }
                        item = GetExpandedSelection(parentBitmap, expandSelectionList, checkBoxRightToLeft.Checked);

                        _vobSubOcrCharacter.Initialize(bitmap, item, _manualOcrDialogPosition, _italicCheckedLast, expandSelectionList.Count > 1, null, _lastAdditions, this);
                        DialogResult result = _vobSubOcrCharacter.ShowDialog(this);
                        _manualOcrDialogPosition = _vobSubOcrCharacter.FormPosition;
                        if (result == DialogResult.OK && _vobSubOcrCharacter.ShrinkSelection)
                        {
                            shrinkSelection = true;
                            index--;
                            if (expandSelectionList.Count > 0)
                                expandSelectionList.RemoveAt(expandSelectionList.Count - 1);
                        }
                        else if (result == DialogResult.OK && _vobSubOcrCharacter.ExpandSelection)
                        {
                            expandSelection = true;
                        }
                        else if (result == DialogResult.OK)
                        {
                            string text = _vobSubOcrCharacter.ManualRecognizedCharacters;
                            string name = SaveCompareItem(item.NikseBitmap, text, _vobSubOcrCharacter.IsItalic, expandSelectionList.Count);
                            var addition = new ImageCompareAddition(name, text, item.NikseBitmap, _vobSubOcrCharacter.IsItalic, listViewIndex);
                            _lastAdditions.Add(addition);
                            matches.Add(new CompareMatch(text, _vobSubOcrCharacter.IsItalic, expandSelectionList.Count, null));
                            expandSelectionList = new List<ImageSplitterItem>();
                        }
                        else if (result == DialogResult.Abort)
                        {
                            _abort = true;
                        }
                        else
                        {
                            matches.Add(new CompareMatch("*", false, 0, null));
                        }
                        _italicCheckedLast = _vobSubOcrCharacter.IsItalic;

                    }
                    else if (item.NikseBitmap == null)
                    {
                        matches.Add(new CompareMatch(item.SpecialCharacter, false, 0, null));
                    }
                    else
                    {
                        CompareMatch bestGuess;
                        CompareMatch match = GetCompareMatch(item, parentBitmap, out bestGuess);
                        if (match == null)
                        {
                            _vobSubOcrCharacter.Initialize(bitmap, item, _manualOcrDialogPosition, _italicCheckedLast, false, bestGuess, _lastAdditions, this);
                            DialogResult result = _vobSubOcrCharacter.ShowDialog(this);
                            _manualOcrDialogPosition = _vobSubOcrCharacter.FormPosition;
                            if (result == DialogResult.OK && _vobSubOcrCharacter.ExpandSelection)
                            {
                                expandSelectionList.Add(item);
                                expandSelection = true;
                            }
                            else if (result == DialogResult.OK)
                            {
                                string text = _vobSubOcrCharacter.ManualRecognizedCharacters;
                                string name = SaveCompareItem(item.NikseBitmap, text, _vobSubOcrCharacter.IsItalic, 0);
                                var addition = new ImageCompareAddition(name, text, item.NikseBitmap, _vobSubOcrCharacter.IsItalic, listViewIndex);
                                _lastAdditions.Add(addition);
                                matches.Add(new CompareMatch(text, _vobSubOcrCharacter.IsItalic, 0, null));
                            }
                            else if (result == DialogResult.Abort)
                            {
                                _abort = true;
                            }
                            else
                            {
                                matches.Add(new CompareMatch("*", false, 0, null));
                            }
                            _italicCheckedLast = _vobSubOcrCharacter.IsItalic;
                        }
                        else // found image match
                        {
                            matches.Add(new CompareMatch(match.Text, match.Italic, 0, null));
                            if (match.ExpandCount > 0)
                                index += match.ExpandCount - 1;
                        }
                    }
                    if (_abort)
                        return string.Empty;
                    if (!expandSelection && !shrinkSelection)
                        index++;
                    if (shrinkSelection && expandSelectionList.Count < 2)
                    {
                        shrinkSelection = false;
                        expandSelectionList = new List<ImageSplitterItem>();
                    }
                }

                line = GetStringWithItalicTags(matches);
            }
            else
            {
                line = threadText;
            }
            if (checkBoxAutoFixCommonErrors.Checked)
                line = OcrFixEngine.FixOcrErrorsViaHardcodedRules(line, _lastLine, null); // TODO: add abbreviations list

            if (checkBoxRightToLeft.Checked)
                line = ReverseNumberStrings(line);


            //ocr fix engine
            string textWithOutFixes = line;
            if (_ocrFixEngine.IsDictionaryLoaded)
            {
                if (checkBoxAutoFixCommonErrors.Checked)
                    line = _ocrFixEngine.FixOcrErrors(line, listViewIndex, _lastLine, true, checkBoxGuessUnknownWords.Checked);
                int correctWords;
                int wordsNotFound = _ocrFixEngine.CountUnknownWordsViaDictionary(line, out correctWords);

                if (wordsNotFound > 0 || correctWords == 0 || textWithOutFixes != null && textWithOutFixes.ToString().Replace("~", string.Empty).Trim().Length == 0)
                {
                    _ocrFixEngine.AutoGuessesUsed.Clear();
                    _ocrFixEngine.UnknownWordsFound.Clear();
                    line = _ocrFixEngine.FixUnknownWordsViaGuessOrPrompt(out wordsNotFound, line, listViewIndex, bitmap, checkBoxAutoFixCommonErrors.Checked, checkBoxPromptForUnknownWords.Checked, true, checkBoxGuessUnknownWords.Checked);
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
                LogUnknownWords();

                if (wordsNotFound >= 3)
                    subtitleListView1.SetBackgroundColor(listViewIndex, Color.Red);
                if (wordsNotFound == 2)
                    subtitleListView1.SetBackgroundColor(listViewIndex, Color.Orange);
                else if (wordsNotFound == 1)
                    subtitleListView1.SetBackgroundColor(listViewIndex, Color.Yellow);
                else if (line.Trim().Length == 0)
                    subtitleListView1.SetBackgroundColor(listViewIndex, Color.Orange);
                else
                    subtitleListView1.SetBackgroundColor(listViewIndex, Color.LightGreen);
            }

            if (textWithOutFixes.Trim() != line.Trim())
            {
                _tesseractOcrAutoFixes++;
                labelFixesMade.Text = string.Format(" - {0}", _tesseractOcrAutoFixes);
                LogOcrFix(listViewIndex, textWithOutFixes.ToString(), line);
            }

            return line;
        }

        private void SaveNOcr(string fileName)
        {
            try
            {
                var textWriter = new XmlTextWriter(fileName, null) { Formatting = Formatting.Indented };
                textWriter.WriteStartDocument();
                textWriter.WriteStartElement("NOCR", "");
                textWriter.WriteAttributeString("Language", "en");
                foreach (NOcrChar oc in _nocrChars)
                {
                    textWriter.WriteStartElement("Char", "");
                    textWriter.WriteAttributeString("Text", oc.Text);
                    textWriter.WriteAttributeString("Width", oc.Width.ToString(CultureInfo.InvariantCulture));
                    textWriter.WriteAttributeString("Height", oc.Height.ToString(CultureInfo.InvariantCulture));
                    textWriter.WriteAttributeString("MarginTop", oc.MarginTop.ToString(CultureInfo.InvariantCulture));
                    if (oc.Italic)
                        textWriter.WriteAttributeString("Italic", oc.Italic.ToString(CultureInfo.InvariantCulture));
                    if (oc.ExpandCount > 1)
                        textWriter.WriteAttributeString("ExpandCount", oc.ExpandCount.ToString(CultureInfo.InvariantCulture));
                    foreach (NOcrPoint op in oc.LinesForeground)
                    {
                        textWriter.WriteStartElement("Point", "");
                        textWriter.WriteAttributeString("On", true.ToString());
                        textWriter.WriteAttributeString("Start", op.Start.X.ToString(CultureInfo.InvariantCulture) + "," + op.Start.Y.ToString(CultureInfo.InvariantCulture));
                        textWriter.WriteAttributeString("End", op.End.X.ToString(CultureInfo.InvariantCulture) + "," + op.End.Y.ToString(CultureInfo.InvariantCulture));
                        textWriter.WriteEndElement();
                    }
                    foreach (NOcrPoint op in oc.LinesBackground)
                    {
                        textWriter.WriteStartElement("Point", "");
                        textWriter.WriteAttributeString("Start", op.Start.X.ToString(CultureInfo.InvariantCulture) + "," + op.Start.Y.ToString(CultureInfo.InvariantCulture));
                        textWriter.WriteAttributeString("End", op.End.X.ToString(CultureInfo.InvariantCulture) + "," + op.End.Y.ToString(CultureInfo.InvariantCulture));
                        textWriter.WriteEndElement();
                    }
                    textWriter.WriteEndElement();
                }
                textWriter.WriteEndElement();
                textWriter.WriteEndDocument();
                textWriter.Close();
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
            }
        }


        private void LoadNOcr(string fileName, bool clear)
        {
            if (clear || _nocrChars == null)
                _nocrChars = new List<NOcrChar>();

            if (File.Exists(fileName))
            {
                try
                {
                    foreach (NOcrChar c in LoadNOcr(fileName))
                        _nocrChars.Add(c);
                }
                catch (Exception exception)
                {
                    MessageBox.Show(exception.Message);
                }
            }
        }

        public static List<NOcrChar> LoadNOcr(string fileName)
        {
            var nocrChars = new List<NOcrChar>();
            if (File.Exists(fileName))
            {
                try
                {
                    var doc = new XmlDocument();
                    doc.Load(fileName);
                    foreach (XmlNode node in doc.DocumentElement.SelectNodes("Char"))
                    {
                        var oc = new NOcrChar(node.Attributes["Text"].Value);
                        oc.Width = Convert.ToInt32(node.Attributes["Width"].Value, CultureInfo.InvariantCulture);
                        oc.Height = Convert.ToInt32(node.Attributes["Height"].Value, CultureInfo.InvariantCulture);
                        oc.MarginTop = Convert.ToInt32(node.Attributes["MarginTop"].Value, CultureInfo.InvariantCulture);
                        if (node.Attributes["Italic"] != null)
                            oc.Italic = Convert.ToBoolean(node.Attributes["Italic"].Value, CultureInfo.InvariantCulture);
                        if (node.Attributes["ExpandCount"] != null)
                            oc.ExpandCount = Convert.ToInt32(node.Attributes["ExpandCount"].Value, CultureInfo.InvariantCulture);
                        foreach (XmlNode pointNode in node.SelectNodes("Point"))
                        {
                            var op = new NOcrPoint(DecodePoint(pointNode.Attributes["Start"].Value), DecodePoint(pointNode.Attributes["End"].Value));
                            XmlAttribute a = pointNode.Attributes["On"];
                            if (a != null && Convert.ToBoolean(a.Value))
                                oc.LinesForeground.Add(op);
                            else
                                oc.LinesBackground.Add(op);
                        }
                        nocrChars.Add(oc);
                    }
                }
                catch (Exception exception)
                {
                    MessageBox.Show(exception.Message);
                }
            }
            return nocrChars;
        }

        public static List<NOcrChar> LoadNOcrForTesseract(string xmlRessourceName)
        {
            var nocrChars = new List<NOcrChar>();
            System.Reflection.Assembly asm = System.Reflection.Assembly.GetExecutingAssembly();
            Stream strm = asm.GetManifestResourceStream(xmlRessourceName);
            if (strm != null)
            {
                XmlDocument doc = new XmlDocument();
                var rdr = new StreamReader(strm);
                using (var zip = new System.IO.Compression.GZipStream(rdr.BaseStream, System.IO.Compression.CompressionMode.Decompress))
                {
                    byte[] data = new byte[175000];
                    zip.Read(data, 0, 175000);
                    doc.LoadXml(System.Text.Encoding.UTF8.GetString(data));
                }
                rdr.Close();

                try
                {
                    foreach (XmlNode node in doc.DocumentElement.SelectNodes("Char"))
                    {
                        var oc = new NOcrChar(node.Attributes["Text"].Value);
                        oc.Width = Convert.ToInt32(node.Attributes["Width"].Value, CultureInfo.InvariantCulture);
                        oc.Height = Convert.ToInt32(node.Attributes["Height"].Value, CultureInfo.InvariantCulture);
                        oc.MarginTop = Convert.ToInt32(node.Attributes["MarginTop"].Value, CultureInfo.InvariantCulture);
                        if (node.Attributes["Italic"] != null)
                            oc.Italic = Convert.ToBoolean(node.Attributes["Italic"].Value, CultureInfo.InvariantCulture);
                        if (node.Attributes["ExpandCount"] != null)
                            oc.ExpandCount = Convert.ToInt32(node.Attributes["ExpandCount"].Value, CultureInfo.InvariantCulture);
                        foreach (XmlNode pointNode in node.SelectNodes("Point"))
                        {
                            var op = new NOcrPoint(DecodePoint(pointNode.Attributes["Start"].Value), DecodePoint(pointNode.Attributes["End"].Value));
                            XmlAttribute a = pointNode.Attributes["On"];
                            if (a != null && Convert.ToBoolean(a.Value))
                                oc.LinesForeground.Add(op);
                            else
                                oc.LinesBackground.Add(op);
                        }
                        nocrChars.Add(oc);
                    }
                }
                catch (Exception exception)
                {
                    MessageBox.Show(exception.Message);
                }
            }
            return nocrChars;
        }

        private static Point DecodePoint(string text)
        {
            var arr = text.Split(',');
            return new Point(Convert.ToInt32(arr[0], CultureInfo.InvariantCulture), Convert.ToInt32(arr[1], CultureInfo.InvariantCulture));
        }

        private void NOCRIntialize(Bitmap bitmap)
        {
            var matches = new List<CompareMatch>();
            List<ImageSplitterItem> list = NikseBitmapImageSplitter.SplitBitmapToLetters(new NikseBitmap(bitmap), (int)numericUpDownNumberOfPixelsIsSpaceNOCR.Value, checkBoxRightToLeft.Checked, Configuration.Settings.VobSubOcr.TopToBottom);

            foreach (ImageSplitterItem item in list)
            {
                if (item.NikseBitmap != null)
                {
                    var nbmp = item.NikseBitmap;
                    nbmp.ReplaceNonWhiteWithTransparent();
                    item.Y += nbmp.CropTopTransparent(0);
                    nbmp.CropTransparentSidesAndBottom(0, true);
                    nbmp.ReplaceTransparentWith(Color.Black);
                    GetNOcrCompareMatch(item, bitmap, _nocrChars, _unItalicFactor, false, false);
                }
            }
        }

        private string OcrViaNOCR(Bitmap bitmap, int listViewIndex)
        {
            if (_ocrFixEngine == null)
                comboBoxDictionaries_SelectedIndexChanged(null, null);

            string line = string.Empty;
            if (_nocrThreadResults != null)
                line = _nocrThreadResults[listViewIndex];
            if (string.IsNullOrEmpty(line))
            {
                var nbmpInput = new NikseBitmap(bitmap);
                nbmpInput.ReplaceNonWhiteWithTransparent();

                var matches = new List<CompareMatch>();
                List<ImageSplitterItem> list = NikseBitmapImageSplitter.SplitBitmapToLetters(nbmpInput, (int)numericUpDownNumberOfPixelsIsSpaceNOCR.Value, checkBoxRightToLeft.Checked, Configuration.Settings.VobSubOcr.TopToBottom);

                foreach (ImageSplitterItem item in list)
                {
                    if (item.NikseBitmap != null)
                    {
                        item.NikseBitmap.ReplaceNonWhiteWithTransparent();
                        item.Y += item.NikseBitmap.CropTopTransparent(0);
                        item.NikseBitmap.CropTransparentSidesAndBottom(0, true);
                        item.NikseBitmap.ReplaceTransparentWith(Color.Black);
                    }
                }
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
                        else if (index + 1 < list.Count && list[index + 1].NikseBitmap != null) // only allow expand to EndOfLine or space
                        {

                            index++;
                            expandSelectionList.Add(list[index]);
                        }
                        item = GetExpandedSelection(nbmpInput, expandSelectionList, checkBoxRightToLeft.Checked);
                        if (item.NikseBitmap != null)
                        {
                            item.NikseBitmap.ReplaceNonWhiteWithTransparent();
                            item.Y += item.NikseBitmap.CropTopTransparent(0);
                            item.NikseBitmap.CropTransparentSidesAndBottom(0, true);
                            item.NikseBitmap.ReplaceTransparentWith(Color.Black);
                        }

                        _vobSubOcrNOcrCharacter.Initialize(bitmap, item, _manualOcrDialogPosition, _italicCheckedLast, expandSelectionList.Count > 1, null, _lastAdditions, this);
                        DialogResult result = _vobSubOcrNOcrCharacter.ShowDialog(this);
                        _manualOcrDialogPosition = _vobSubOcrNOcrCharacter.FormPosition;
                        if (result == DialogResult.OK && _vobSubOcrNOcrCharacter.ShrinkSelection)
                        {
                            shrinkSelection = true;
                            index--;
                            if (expandSelectionList.Count > 0)
                                expandSelectionList.RemoveAt(expandSelectionList.Count - 1);
                        }
                        else if (result == DialogResult.OK && _vobSubOcrNOcrCharacter.ExpandSelection)
                        {
                            expandSelection = true;
                        }
                        else if (result == DialogResult.OK)
                        {
                            var c = _vobSubOcrNOcrCharacter.NOcrChar;
                            if (expandSelectionList.Count > 1)
                                c.ExpandCount = expandSelectionList.Count;
                            _nocrChars.Add(c);
                            SaveNOcrWithCurrentLanguage();
                            string text = _vobSubOcrNOcrCharacter.NOcrChar.Text;
                            string name = SaveCompareItem(item.NikseBitmap, text, _vobSubOcrNOcrCharacter.IsItalic, expandSelectionList.Count);
                            var addition = new ImageCompareAddition(name, text, item.NikseBitmap, _vobSubOcrNOcrCharacter.IsItalic, listViewIndex);
                            _lastAdditions.Add(addition);
                            matches.Add(new CompareMatch(text, _vobSubOcrNOcrCharacter.IsItalic, expandSelectionList.Count, null));
                            expandSelectionList = new List<ImageSplitterItem>();
                        }
                        else if (result == DialogResult.Abort)
                        {
                            _abort = true;
                        }
                        else
                        {
                            matches.Add(new CompareMatch("*", false, 0, null));
                        }
                        _italicCheckedLast = _vobSubOcrNOcrCharacter.IsItalic;

                    }
                    else if (item.NikseBitmap == null)
                    {
                        matches.Add(new CompareMatch(item.SpecialCharacter, false, 0, null));
                    }
                    else
                    {
                        CompareMatch match = GetNOcrCompareMatch(item, bitmap, _nocrChars, _unItalicFactor, checkBoxNOcrItalic.Checked, !checkBoxNOcrCorrect.Checked);
                        if (match == null)
                        {
                            _vobSubOcrNOcrCharacter.Initialize(bitmap, item, _manualOcrDialogPosition, _italicCheckedLast, false, null, _lastAdditions, this);
                            DialogResult result = _vobSubOcrNOcrCharacter.ShowDialog(this);
                            _manualOcrDialogPosition = _vobSubOcrNOcrCharacter.FormPosition;
                            if (result == DialogResult.OK && _vobSubOcrNOcrCharacter.ExpandSelection)
                            {
                                expandSelectionList.Add(item);
                                expandSelection = true;
                            }
                            else if (result == DialogResult.OK)
                            {
                                _nocrChars.Add(_vobSubOcrNOcrCharacter.NOcrChar);
                                SaveNOcrWithCurrentLanguage();
                                string text = _vobSubOcrNOcrCharacter.NOcrChar.Text;
                                string name = SaveCompareItem(item.NikseBitmap, text, _vobSubOcrNOcrCharacter.IsItalic, 0);
                                var addition = new ImageCompareAddition(name, text, item.NikseBitmap, _vobSubOcrNOcrCharacter.IsItalic, listViewIndex);
                                _lastAdditions.Add(addition);
                                matches.Add(new CompareMatch(text, _vobSubOcrNOcrCharacter.IsItalic, 0, null));
                            }
                            else if (result == DialogResult.Abort)
                            {
                                _abort = true;
                            }
                            else
                            {
                                matches.Add(new CompareMatch("*", false, 0, null));
                            }
                            _italicCheckedLast = _vobSubOcrNOcrCharacter.IsItalic;
                        }
                        else // found image match
                        {
                            matches.Add(new CompareMatch(match.Text, match.Italic, 0, null));
                            if (match.ExpandCount > 0)
                                index += match.ExpandCount - 1;
                        }
                    }
                    if (_abort)
                        return string.Empty;
                    if (!expandSelection && !shrinkSelection)
                        index++;
                    if (shrinkSelection && expandSelectionList.Count < 2)
                    {
                        shrinkSelection = false;
                        expandSelectionList = new List<ImageSplitterItem>();
                    }
                }
                line = GetStringWithItalicTags(matches);
            }

            line = FixNocrHardcodedStuff(line);

            //ocr fix engine
            string textWithOutFixes = line;
            if (_ocrFixEngine.IsDictionaryLoaded)
            {
                if (checkBoxAutoFixCommonErrors.Checked)
                    line = _ocrFixEngine.FixOcrErrors(line, listViewIndex, _lastLine, true, checkBoxGuessUnknownWords.Checked);
                int correctWords;
                int wordsNotFound = _ocrFixEngine.CountUnknownWordsViaDictionary(line, out correctWords);

                if (wordsNotFound > 0 || correctWords == 0 || textWithOutFixes != null && textWithOutFixes.ToString().Replace("~", string.Empty).Trim().Length == 0)
                {
                    _ocrFixEngine.AutoGuessesUsed.Clear();
                    _ocrFixEngine.UnknownWordsFound.Clear();
                    line = _ocrFixEngine.FixUnknownWordsViaGuessOrPrompt(out wordsNotFound, line, listViewIndex, bitmap, checkBoxAutoFixCommonErrors.Checked, checkBoxPromptForUnknownWords.Checked, true, checkBoxGuessUnknownWords.Checked);
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
                LogUnknownWords();

                if (wordsNotFound >= 3)
                    subtitleListView1.SetBackgroundColor(listViewIndex, Color.Red);
                if (wordsNotFound == 2)
                    subtitleListView1.SetBackgroundColor(listViewIndex, Color.Orange);
                else if (wordsNotFound == 1)
                    subtitleListView1.SetBackgroundColor(listViewIndex, Color.Yellow);
                else if (line.Trim().Length == 0)
                    subtitleListView1.SetBackgroundColor(listViewIndex, Color.Orange);
                else
                    subtitleListView1.SetBackgroundColor(listViewIndex, Color.LightGreen);
            }

            if (textWithOutFixes.Trim() != line.Trim())
            {
                _tesseractOcrAutoFixes++;
                labelFixesMade.Text = string.Format(" - {0}", _tesseractOcrAutoFixes);
                LogOcrFix(listViewIndex, textWithOutFixes.ToString(), line);
            }

            return line;
        }

        private string FixNocrHardcodedStuff(string line)
        {
            // fix I/l
            int start = line.IndexOf("I");
            while (start > 0)
            {
                if (start > 0 && line[start - 1].ToString() != line[start - 1].ToString().ToUpper())
                    line = line.Remove(start, 1).Insert(start, "l");
                else if (start < line.Length - 1 && line[start + 1].ToString() != line[start + 1].ToString().ToUpper())
                    line = line.Remove(start, 1).Insert(start, "l");
                start++;
                start = line.IndexOf("I", start);
            }
            start = line.IndexOf("l");
            while (start > 0)
            {
                if (start < line.Length - 1 && line[start + 1].ToString() != line[start + 1].ToString().ToLower())
                    line = line.Remove(start, 1).Insert(start, "I");
                start++;
                start = line.IndexOf("l", start);
            }
            if (line.Contains("l"))
            {
                if (line.StartsWith("l"))
                    line = line.Remove(0, 1).Insert(0, "I");
                if (line.StartsWith("<i>l"))
                    line = line.Remove(3, 1).Insert(3, "I");
                if (line.StartsWith("- l"))
                    line = line.Remove(2, 1).Insert(2, "I");
                if (line.StartsWith("-l"))
                    line = line.Remove(1, 1).Insert(1, "I");
                line = line.Replace(". l", ". I");
                line = line.Replace("? l", "? I");
                line = line.Replace("! l", "! I");
                line = line.Replace(": l", ": I");
                line = line.Replace("." + Environment.NewLine + "l", "." + Environment.NewLine + "I");
                line = line.Replace("?" + Environment.NewLine + "l", "?" + Environment.NewLine + "I");
                line = line.Replace("!" + Environment.NewLine + "l", "!" + Environment.NewLine + "I");
                line = line.Replace("." + Environment.NewLine + "- l", "." + Environment.NewLine + "- I");
                line = line.Replace("?" + Environment.NewLine + "- l", "?" + Environment.NewLine + "- I");
                line = line.Replace("!" + Environment.NewLine + "- l", "!" + Environment.NewLine + "- I");
                line = line.Replace("." + Environment.NewLine + "-l", "." + Environment.NewLine + "-I");
                line = line.Replace("?" + Environment.NewLine + "-l", "?" + Environment.NewLine + "-I");
                line = line.Replace("!" + Environment.NewLine + "-l", "!" + Environment.NewLine + "-I");
                line = line.Replace(" lq", " Iq");
                line = line.Replace(" lw", " Iw");
                line = line.Replace(" lr", " Ir");
                line = line.Replace(" lt", " It");
                line = line.Replace(" lp", " Ip");
                line = line.Replace(" ls", " Is");
                line = line.Replace(" ld", " Id");
                line = line.Replace(" lf", " If");
                line = line.Replace(" lg", " Ig");
                line = line.Replace(" lh", " Ih");
                line = line.Replace(" lj", " Ij");
                line = line.Replace(" lk", " Ik");
                line = line.Replace(" ll", " Il");
                line = line.Replace(" lz", " Iz");
                line = line.Replace(" lx", " Ix");
                line = line.Replace(" lc", " Ic");
                line = line.Replace(" lv", " Iv");
                line = line.Replace(" lb", " Ib");
                line = line.Replace(" ln", " In");
                line = line.Replace(" lm", " Im");
            }
            if (line.Contains("I"))
            {
                line = line.Replace("II", "ll");
            }

            // fix periods with space between
            line = line.Replace(".   .", "..");
            line = line.Replace(".  .", "..");
            line = line.Replace(". .", "..");
            line = line.Replace(". .", "..");
            line = line.Replace(" ." + Environment.NewLine, "." + Environment.NewLine);
            if (line.EndsWith(" ."))
                line = line.Remove(line.Length - 2, 1);

            // fix no space before comma
            line = line.Replace(" ,", ",");

            // fix O => 0
            if (line.Contains("O"))
            {
                line = line.Replace(", OOO", ",000");
                line = line.Replace(",OOO", ",000");
                line = line.Replace(". OOO", ".000");
                line = line.Replace(".OOO", ".000");

                line = line.Replace("1O", "10");
                line = line.Replace("2O", "20");
                line = line.Replace("3O", "30");
                line = line.Replace("4O", "40");
                line = line.Replace("5O", "50");
                line = line.Replace("6O", "60");
                line = line.Replace("7O", "70");
                line = line.Replace("8O", "80");
                line = line.Replace("9O", "90");

                line = line.Replace("O1", "01");
                line = line.Replace("O2", "02");
                line = line.Replace("O3", "03");
                line = line.Replace("O4", "04");
                line = line.Replace("O5", "05");
                line = line.Replace("O6", "06");
                line = line.Replace("O7", "07");
                line = line.Replace("O8", "08");
                line = line.Replace("O9", "09");

                line = line.Replace("O-O", "0-0");
                line = line.Replace("O-1", "0-1");
                line = line.Replace("O-2", "0-2");
                line = line.Replace("O-3", "0-3");
                line = line.Replace("O-4", "0-4");
                line = line.Replace("O-5", "0-5");
                line = line.Replace("O-6", "0-6");
                line = line.Replace("O-7", "0-7");
                line = line.Replace("O-8", "0-8");
                line = line.Replace("O-9", "0-9");
                line = line.Replace("1-O", "1-0");
                line = line.Replace("2-O", "2-0");
                line = line.Replace("3-O", "3-0");
                line = line.Replace("4-O", "4-0");
                line = line.Replace("5-O", "5-0");
                line = line.Replace("6-O", "6-0");
                line = line.Replace("7-O", "7-0");
                line = line.Replace("8-O", "8-0");
                line = line.Replace("9-O", "9-0");
            }

            if (checkBoxAutoFixCommonErrors.Checked)
                line = OcrFixEngine.FixOcrErrorsViaHardcodedRules(line, _lastLine, null); // TODO: add abbreviations list

            if (checkBoxRightToLeft.Checked)
                line = ReverseNumberStrings(line);

            return line;
        }

        private string ReverseNumberStrings(string line)
        {
            Regex regex = new Regex(@"\b\d+\b");
            var matches = regex.Matches(line);
            foreach (Match match in matches)
            {
                if (match.Length > 1)
                {
                    string number = string.Empty;
                    for (int i = match.Index; i < match.Index + match.Length; i++)
                    {
                        number = line[i] + number;
                    }
                    line = line.Remove(match.Index, match.Length).Insert(match.Index, number);
                }
            }
            return line;
        }

        internal static ImageSplitterItem GetExpandedSelection(NikseBitmap bitmap, List<ImageSplitterItem> expandSelectionList, bool rightToLeft)
        {
            if (rightToLeft)
            {
                int minimumX = expandSelectionList[expandSelectionList.Count - 1].X - expandSelectionList[expandSelectionList.Count - 1].NikseBitmap.Width;
                int maximumX = expandSelectionList[0].X;
                int minimumY = expandSelectionList[0].Y;
                int maximumY = expandSelectionList[0].Y + expandSelectionList[0].NikseBitmap.Height;
                foreach (ImageSplitterItem item in expandSelectionList)
                {
                    if (item.Y < minimumY)
                        minimumY = item.Y;
                    if (item.Y + item.NikseBitmap.Height > maximumY)
                        maximumY = item.Y + item.NikseBitmap.Height;
                }
                var part = bitmap.CopyRectangle(new Rectangle(minimumX, minimumY, maximumX - minimumX, maximumY - minimumY));
                return new ImageSplitterItem(minimumX, minimumY, part);
            }
            else
            {
                int minimumX = expandSelectionList[0].X;
                int maximumX = expandSelectionList[expandSelectionList.Count - 1].X + expandSelectionList[expandSelectionList.Count - 1].NikseBitmap.Width;
                int minimumY = expandSelectionList[0].Y;
                int maximumY = expandSelectionList[0].Y + expandSelectionList[0].NikseBitmap.Height;
                foreach (ImageSplitterItem item in expandSelectionList)
                {
                    if (item.Y < minimumY)
                        minimumY = item.Y;
                    if (item.Y + item.NikseBitmap.Height > maximumY)
                        maximumY = item.Y + item.NikseBitmap.Height;
                }
                var part = bitmap.CopyRectangle(new Rectangle(minimumX, minimumY, maximumX - minimumX, maximumY - minimumY));
                return new ImageSplitterItem(minimumX, minimumY, part);
            }
        }

        private static string GetStringWithItalicTags(List<CompareMatch> matches)
        {
            StringBuilder paragraph = new StringBuilder();
            StringBuilder line = new StringBuilder();
            StringBuilder word = new StringBuilder();
            int lettersItalics = 0;
            int lettersNonItalics = 0;
            int lineLettersNonItalics = 0;
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
                    ItalianLine(paragraph, ref line, ref allItalic, ref wordItalics, ref wordNonItalics, ref isItalic, Environment.NewLine, lineLettersNonItalics);
                    lineLettersNonItalics = 0;
                }
                else if (matches[i].Italic)
                {
                    word.Append(matches[i].Text);
                    lettersItalics += matches[i].Text.Length;
                    lineLettersNonItalics += matches[i].Text.Length;
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
                ItalianLine(paragraph, ref line, ref allItalic, ref wordItalics, ref wordNonItalics, ref isItalic, "", lineLettersNonItalics);

            if (allItalic && matches.Count > 0)
            {
                string temp = paragraph.ToString().Replace("<i>", "").Replace("</i>", "");
                paragraph = new StringBuilder();
                paragraph.Append("<i>" + temp + "</i>");

            }

            return paragraph.ToString();
        }

        private static void ItalianLine(StringBuilder paragraph, ref StringBuilder line, ref bool allItalic, ref int wordItalics, ref int wordNonItalics, ref bool isItalic, string appendString, int lineLettersNonItalics)
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
            else if (wordItalics > 0 && wordNonItalics < 2 && lineLettersNonItalics < 3 && line.ToString().Trim().StartsWith("-"))
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
            if (lettersItalics >= lettersNonItalics && lettersItalics > 0)
            {
                if (!isItalic)
                    line.Append("<i>");
                line.Append(word + appendString);
                wordItalics++;
                isItalic = true;
            }
            else
            {
                if (isItalic)
                {
                    line.Append("</i>");
                    isItalic = false;
                }
                line.Append(word.ToString());
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
            checkBoxUseModiInTesseractForUnknownWords.Checked = Configuration.Settings.VobSubOcr.UseModiInTesseractForUnknownWords;
            if (_mp4List != null)
            {
                checkBoxShowOnlyForced.Visible = false;
                checkBoxUseTimeCodesFromIdx.Visible = false;

                buttonOK.Enabled = true;
                buttonCancel.Enabled = true;
                buttonStartOcr.Enabled = true;
                buttonStop.Enabled = false;
                buttonNewCharacterDatabase.Enabled = true;
                buttonEditCharacterDatabase.Enabled = true;
                buttonStartOcr.Focus();
            }
            else if (_spList != null)
            {
                checkBoxShowOnlyForced.Visible = false;
                checkBoxUseTimeCodesFromIdx.Visible = false;

                buttonOK.Enabled = true;
                buttonCancel.Enabled = true;
                buttonStartOcr.Enabled = true;
                buttonStop.Enabled = false;
                buttonNewCharacterDatabase.Enabled = true;
                buttonEditCharacterDatabase.Enabled = true;
                buttonStartOcr.Focus();
            }
            else if (_dvbSubtitles != null)
            {
                checkBoxShowOnlyForced.Visible = false;
                checkBoxUseTimeCodesFromIdx.Visible = false;

                buttonOK.Enabled = true;
                buttonCancel.Enabled = true;
                buttonStartOcr.Enabled = true;
                buttonStop.Enabled = false;
                buttonNewCharacterDatabase.Enabled = true;
                buttonEditCharacterDatabase.Enabled = true;
                buttonStartOcr.Focus();
            }
            else if (_bdnXmlOriginal != null)
            {
                LoadBdnXml();
                bool hasForcedSubtitles = false;
                foreach (var x in _bdnXmlOriginal.Paragraphs)
                {
                    if (x.Forced)
                    {
                        hasForcedSubtitles = true;
                        break;
                    }
                }
                checkBoxShowOnlyForced.Enabled = hasForcedSubtitles;
                checkBoxUseTimeCodesFromIdx.Visible = false;
            }
            else if (_bluRaySubtitlesOriginal != null)
            {
                numericUpDownMaxErrorPct.Value = (decimal)Configuration.Settings.VobSubOcr.BlurayAllowDifferenceInPercent;
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
            else if (_xSubList != null)
            {
                checkBoxShowOnlyForced.Visible = false;
                checkBoxUseTimeCodesFromIdx.Visible = false;

                buttonOK.Enabled = true;
                buttonCancel.Enabled = true;
                buttonStartOcr.Enabled = true;
                buttonStop.Enabled = false;
                buttonNewCharacterDatabase.Enabled = true;
                buttonEditCharacterDatabase.Enabled = true;
                buttonStartOcr.Focus();
            }
            else
            {
                ReadyVobSubRip();
            }
            VobSubOcr_Resize(null, null);
        }

        public void DoHide()
        {
            base.SetVisibleCore(false);
        }

        public Subtitle ReadyVobSubRip()
        {
            _vobSubMergedPackistOriginal = new List<VobSubMergedPack>();
            bool hasIdxTimeCodes = false;
            bool hasForcedSubtitles = false;
            if (_vobSubMergedPackist == null)
                return null;
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
            return _subtitle;
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
            _mainOcrRunning = false;
            labelStatus.Text = string.Empty;
            progressBar1.Visible = false;
        }

        static void ImageCompareThreadDoWork(object sender, DoWorkEventArgs e)
        {
            var p = (ImageCompareThreadParameter)e.Argument;
            e.Result = p;
            Bitmap bitmap = p.Picture;
            var matches = new List<CompareMatch>();
            List<ImageSplitterItem> lines = NikseBitmapImageSplitter.SplitVertical(bitmap);
            List<ImageSplitterItem> list = NikseBitmapImageSplitter.SplitBitmapToLetters(lines, p.NumberOfPixelsIsSpace, p.RightToLeft, Configuration.Settings.VobSubOcr.TopToBottom);

            int index = 0;
            int outerIndex = 0;
            while (outerIndex < list.Count)
            {
                ImageSplitterItem item = list[outerIndex];
                if (item.NikseBitmap == null)
                {
                    matches.Add(new CompareMatch(item.SpecialCharacter, false, 0, null));
                }
                else
                {
                    var target = item.NikseBitmap;
                    int numberOfForegroundColors = CalculateNumberOfForegroundColors(target);

                    int smallestDifference = 10000;
                    int smallestIndex = -1;

                    if (smallestDifference > 0)
                    {
                        index = 0;
                        foreach (CompareItem compareItem in p.CompareBitmaps)
                        {
                            if (compareItem.Bitmap.Width == target.Width && compareItem.Bitmap.Height == target.Height)
                            {
                                if (compareItem.NumberOfForegroundColors < 1)
                                    compareItem.NumberOfForegroundColors = CalculateNumberOfForegroundColors(compareItem.Bitmap);

                                if (Math.Abs(compareItem.NumberOfForegroundColors - numberOfForegroundColors) < 30)
                                {
                                    int dif = NikseBitmapImageSplitter.IsBitmapsAlike(compareItem.Bitmap, target);
                                    if (dif < smallestDifference)
                                    {
                                        smallestDifference = dif;
                                        smallestIndex = index;
                                        if (dif < 0.2)
                                            break; // foreach ending
                                    }
                                }
                            }
                            index++;
                        }
                    }

                    if (smallestDifference > 1 && target.Width < 55 && target.Width > 5)
                    {
                        index = 0;
                        foreach (CompareItem compareItem in p.CompareBitmaps)
                        {
                            if (compareItem.Bitmap.Width == target.Width && compareItem.Bitmap.Height == target.Height + 1)
                            {
                                if (compareItem.NumberOfForegroundColors == -1)
                                    compareItem.NumberOfForegroundColors = CalculateNumberOfForegroundColors(compareItem.Bitmap);

                                if (Math.Abs(compareItem.NumberOfForegroundColors - numberOfForegroundColors) < 50)
                                {
                                    int dif = NikseBitmapImageSplitter.IsBitmapsAlike(target, compareItem.Bitmap);
                                    if (dif < smallestDifference)
                                    {
                                        smallestDifference = dif;
                                        smallestIndex = index;
                                        if (dif < 0.5)
                                            break; // foreach ending
                                    }
                                }
                            }
                            index++;
                        }
                    }

                    if (smallestDifference > 1 && target.Width < 55 && target.Width > 5)
                    {
                        index = 0;
                        foreach (CompareItem compareItem in p.CompareBitmaps)
                        {
                            if (compareItem.Bitmap.Width == target.Width - 1 && compareItem.Bitmap.Height == target.Height ||
                                compareItem.Bitmap.Width == target.Width - 1 && compareItem.Bitmap.Height == target.Height - 1 ||
                                compareItem.Bitmap.Width == target.Width && compareItem.Bitmap.Height == target.Height - 1)
                            {
                                 if (compareItem.NumberOfForegroundColors < 1)
                                    compareItem.NumberOfForegroundColors = CalculateNumberOfForegroundColors(compareItem.Bitmap);

                                 if (Math.Abs(compareItem.NumberOfForegroundColors - numberOfForegroundColors) < 55)
                                 {
                                     int dif = NikseBitmapImageSplitter.IsBitmapsAlike(compareItem.Bitmap, target);
                                     if (dif < smallestDifference)
                                     {
                                         smallestDifference = dif;
                                         smallestIndex = index;
                                         if (dif < 0.5)
                                             break; // foreach ending
                                     }
                                 }
                            }
                            index++;
                        }
                    }

                    CompareMatch match = null;
                    double differencePercentage = smallestDifference * 100.0 / (item.NikseBitmap.Width * item.NikseBitmap.Height);
                    double maxDiff = (double)p.MaxErrorPercent;
                    if (differencePercentage <= maxDiff && smallestIndex >= 0)
                    {
                        var hit = p.CompareBitmaps[smallestIndex];
                        match = new CompareMatch(hit.Text, hit.Italic, hit.ExpandCount, hit.Name);
                        matches.Add(new CompareMatch(match.Text, match.Italic, 0, null));
                        if (match.ExpandCount > 0)
                            outerIndex += match.ExpandCount - 1;
                    }
                    else
                    {
                        p.Result = string.Empty;
                        return;
                    }
                }
                outerIndex++;
            }
            bitmap.Dispose();
            p.Result = GetStringWithItalicTags(matches);

        }

        void ImageCompareThreadRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            var p = (ImageCompareThreadParameter)e.Result;
            if (!_icThreadsStop)
            {
                if (string.IsNullOrEmpty(_icThreadResults[p.Index]))
                    _icThreadResults[p.Index] = p.Result;
                p.Index += p.Increment;
                while (p.Index <= _mainOcrIndex)
                    p.Index += p.Increment;
                p.Picture.Dispose();
                if (p.Index < _subtitle.Paragraphs.Count)
                {
                    p.Result = string.Empty;
                    p.Picture = GetSubtitleBitmap(p.Index);
                    p.Self.RunWorkerAsync(p);
                }
            }
            else
            {
                _mainOcrRunning = false;
            }
        }

        static string NocrThreadDoItalicWork(NOcrThreadParameter p)
        {
            var unItalicedBmp = UnItalic(p.Picture, p.UnItalicFactor);
            var nbmp = new NikseBitmap(unItalicedBmp);
            nbmp.ReplaceNonWhiteWithTransparent();
            Bitmap bitmap = nbmp.GetBitmap();
            unItalicedBmp.Dispose();

            var matches = new List<CompareMatch>();
            int minLineHeight = p.NOcrLastLowercaseHeight;
            if (minLineHeight < 10)
                minLineHeight = 22;
            int maxLineHeight = p.NOcrLastUppercaseHeight;
            if (maxLineHeight < 10)
                minLineHeight = 80;

            List<ImageSplitterItem> lines = NikseBitmapImageSplitter.SplitVertical(nbmp, minLineHeight);
            List<ImageSplitterItem> list = NikseBitmapImageSplitter.SplitBitmapToLetters(lines, p.NumberOfPixelsIsSpace, p.RightToLeft, Configuration.Settings.VobSubOcr.TopToBottom);

            foreach (ImageSplitterItem item in list)
            {
                if (item.NikseBitmap != null)
                {
                    item.NikseBitmap.ReplaceNonWhiteWithTransparent();
                    item.Y += item.NikseBitmap.CropTopTransparent(0);
                    item.NikseBitmap.CropTransparentSidesAndBottom(0, true);
                    item.NikseBitmap.ReplaceTransparentWith(Color.Black);
                }
            }

            int index = 0;
            while (index < list.Count)
            {
                ImageSplitterItem item = list[index];
                if (item.NikseBitmap == null)
                {
                    matches.Add(new CompareMatch(item.SpecialCharacter, false, 0, null));
                }
                else
                {
                    bool old = p.AdvancedItalicDetection;
                    p.AdvancedItalicDetection = false;
                    CompareMatch match = GetNOcrCompareMatch(item, bitmap, p);
                    p.AdvancedItalicDetection = old;
                    if (match == null)
                    {
                       return string.Empty;
                    }
                    else // found image match
                    {
                        matches.Add(new CompareMatch(match.Text, match.Italic, 0, null));
                        if (match.ExpandCount > 0)
                            index += match.ExpandCount - 1;
                    }
                }
                index++;
            }
            return "<i>" + Utilities.RemoveHtmlTags(GetStringWithItalicTags(matches)) + "</i>";
        }

        public string NocrFastCheck(Bitmap bitmap)
        {
            var nbmpInput = new NikseBitmap(bitmap);
            nbmpInput.ReplaceNonWhiteWithTransparent();

            var matches = new List<CompareMatch>();
            List<ImageSplitterItem> list = NikseBitmapImageSplitter.SplitBitmapToLetters(nbmpInput, (int)numericUpDownNumberOfPixelsIsSpaceNOCR.Value, checkBoxRightToLeft.Checked, Configuration.Settings.VobSubOcr.TopToBottom);

            foreach (ImageSplitterItem item in list)
            {
                if (item.NikseBitmap != null)
                {
                    item.NikseBitmap.ReplaceNonWhiteWithTransparent();
                    item.Y += item.NikseBitmap.CropTopTransparent(0);
                    item.NikseBitmap.CropTransparentSidesAndBottom(0, true);
                    item.NikseBitmap.ReplaceTransparentWith(Color.Black);
                }
            }
            int index = 0;
            var expandSelectionList = new List<ImageSplitterItem>();
            while (index < list.Count)
            {
                ImageSplitterItem item = list[index];
                if (item.NikseBitmap == null)
                {
                    matches.Add(new CompareMatch(item.SpecialCharacter, false, 0, null));
                }
                else
                {
                    CompareMatch match = null;

                    var nbmp = item.NikseBitmap;
                    int index2 = 0;
                    int topMargin = item.Y - item.ParentY;
                    foreach (NOcrChar oc in _nocrChars)
                    {
                        if (Math.Abs(oc.Width - nbmp.Width) < 3 && Math.Abs(oc.Height - nbmp.Height) < 4 && Math.Abs(oc.MarginTop - topMargin) < 4)
                        { // only very accurate matches

                            bool ok = true;
                            index2 = 0;
                            while (index2 < oc.LinesForeground.Count && ok)
                            {
                                NOcrPoint op = oc.LinesForeground[index2];
                                foreach (Point point in op.ScaledGetPoints(oc, nbmp.Width, nbmp.Height))
                                {
                                    if (point.X >= 0 && point.Y >= 0 && point.X < nbmp.Width && point.Y < nbmp.Height)
                                    {
                                        Color c = nbmp.GetPixel(point.X, point.Y);
                                        if (c.A > 150 && c.R + c.G + c.B > NocrMinColor)
                                        {
                                        }
                                        else
                                        {
                                            Point p = new Point(point.X - 1, point.Y);
                                            if (p.X < 0)
                                                p.X = 1;
                                            c = nbmp.GetPixel(p.X, p.Y);
                                            if (nbmp.Width > 20 && c.A > 150 && c.R + c.G + c.B > NocrMinColor)
                                            {
                                            }
                                            else
                                            {
                                                ok = false;
                                                break;
                                            }
                                        }
                                    }
                                }
                                index2++;
                            }
                            index2 = 0;
                            while (index2 < oc.LinesBackground.Count && ok)
                            {
                                NOcrPoint op = oc.LinesBackground[index2];
                                foreach (Point point in op.ScaledGetPoints(oc, nbmp.Width, nbmp.Height))
                                {
                                    if (point.X >= 0 && point.Y >= 0 && point.X < nbmp.Width && point.Y < nbmp.Height)
                                    {
                                        Color c = nbmp.GetPixel(point.X, point.Y);
                                        if (c.A > 150 && c.R + c.G + c.B > NocrMinColor)
                                        {
                                            Point p = new Point(point.X, point.Y);
                                            if (oc.Width > 19 && point.X > 0)
                                                p.X = p.X - 1;
                                            c = nbmp.GetPixel(p.X, p.Y);
                                            if (c.A > 150 && c.R + c.G + c.B > NocrMinColor)
                                            {
                                                ok = false;
                                                break;
                                            }
                                        }
                                    }
                                }
                                index2++;
                            }
                            if (ok)
                                match = new CompareMatch(oc.Text, oc.Italic, 0, null);
                        }
                    }


                    if (match == null)
                    {
                        matches.Add(new CompareMatch("*", false, 0, null));
                    }
                    else // found image match
                    {
                        matches.Add(new CompareMatch(match.Text, match.Italic, 0, null));
                        if (match.ExpandCount > 0)
                            index += match.ExpandCount - 1;
                    }
                }
                index++;
            }
            return GetStringWithItalicTags(matches);
        }

        static void NOcrThreadDoWork(object sender, DoWorkEventArgs e)
        {
            var p = (NOcrThreadParameter)e.Argument;
            e.Result = p;
            var nbmpInput = new NikseBitmap(p.Picture);
            nbmpInput.ReplaceNonWhiteWithTransparent();

            var matches = new List<CompareMatch>();
            int minLineHeight = p.NOcrLastLowercaseHeight;
            if (minLineHeight < 10)
                minLineHeight = 22;
            int maxLineHeight = p.NOcrLastUppercaseHeight;
            if (maxLineHeight < 10)
                minLineHeight = 80;

            List<ImageSplitterItem> lines = NikseBitmapImageSplitter.SplitVertical(nbmpInput, minLineHeight);
            List<ImageSplitterItem> list = NikseBitmapImageSplitter.SplitBitmapToLetters(lines, p.NumberOfPixelsIsSpace, p.RightToLeft, Configuration.Settings.VobSubOcr.TopToBottom);

            foreach (ImageSplitterItem item in list)
            {
                if (item.NikseBitmap != null)
                {
                    var nbmp = item.NikseBitmap;
                    nbmp.ReplaceNonWhiteWithTransparent();
                    item.Y += nbmp.CropTopTransparent(0);
                    nbmp.CropTransparentSidesAndBottom(0, true);
                    nbmp.ReplaceTransparentWith(Color.Black);
                }
            }
            int index = 0;
            while (index < list.Count)
            {
                ImageSplitterItem item = list[index];
                if (item.NikseBitmap == null)
                {
                    matches.Add(new CompareMatch(item.SpecialCharacter, false, 0, null));
                }
                else
                {
                    CompareMatch match = GetNOcrCompareMatch(item, nbmpInput.GetBitmap(), p);
                    if (match == null)
                    {
                        p.Result = string.Empty;
                        return;
                    }
                    else // found image match
                    {
                        matches.Add(new CompareMatch(match.Text, match.Italic, 0, null));
                        if (match.ExpandCount > 0)
                            index += match.ExpandCount - 1;
                    }
                }
                index++;
            }
            p.Result = GetStringWithItalicTags(matches);
        }

        void NOcrThreadRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            var p = (NOcrThreadParameter)e.Result;
            if (!_nocrThreadsStop)
            {
                if (string.IsNullOrEmpty(_nocrThreadResults[p.Index]))
                    _nocrThreadResults[p.Index] = p.Result;
                p.Index += p.Increment;
                p.Picture.Dispose();
                if (p.Index < _subtitle.Paragraphs.Count)
                {
                    p.Result = string.Empty;
                    p.Picture = GetSubtitleBitmap(p.Index);
                    p.Self.RunWorkerAsync(p);
                }
            }
        }

        void TesseractThreadDoWork(object sender, DoWorkEventArgs e)
        {
            var bitmap = (Bitmap)e.Argument;
            if (_tesseractAsyncIndex >= 0 && _tesseractAsyncIndex < _tesseractAsyncStrings.Length)
            {
                if (string.IsNullOrEmpty(_tesseractAsyncStrings[_tesseractAsyncIndex]) && bitmap != null)
                    _tesseractAsyncStrings[_tesseractAsyncIndex] = Tesseract3DoOcrViaExe(bitmap, _languageId, "-psm 6"); // 6 = Assume a single uniform block of text.);
            }
        }

        void TesseractThreadRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (!e.Cancelled)
            {
                _tesseractAsyncIndex++;
                if (_tesseractAsyncIndex >= 0 && _tesseractAsyncIndex < _tesseractAsyncStrings.Length)
                    _tesseractThread.RunWorkerAsync(GetSubtitleBitmap(_tesseractAsyncIndex));
            }
        }

        private void ButtonStartOcrClick(object sender, EventArgs e)
        {
            Configuration.Settings.VobSubOcr.RightToLeft = checkBoxRightToLeft.Checked;
            _lastLine = null;
            buttonOK.Enabled = false;
            buttonCancel.Enabled = false;
            buttonStartOcr.Enabled = false;
            buttonStop.Enabled = true;
            buttonNewCharacterDatabase.Enabled = false;
            buttonEditCharacterDatabase.Enabled = false;

            _abort = false;

            int max = GetSubtitleCount();

            if (comboBoxOcrMethod.SelectedIndex == 0 && _tesseractAsyncStrings == null)
            {
                _nocrChars = null;
                _tesseractAsyncStrings = new string[max];
                _tesseractAsyncIndex = (int)numericUpDownStartNumber.Value + 5;
                _tesseractThread = new BackgroundWorker();
                _tesseractThread.DoWork += TesseractThreadDoWork;
                _tesseractThread.RunWorkerCompleted += TesseractThreadRunWorkerCompleted;
                _tesseractThread.WorkerSupportsCancellation = true;
                if (_tesseractAsyncIndex >= 0 && _tesseractAsyncIndex < max)
                    _tesseractThread.RunWorkerAsync(GetSubtitleBitmap(_tesseractAsyncIndex));
            }
            else if (comboBoxOcrMethod.SelectedIndex == 1)
            {
                if (_compareBitmaps == null)
                    LoadImageCompareBitmaps();
            }
            else if (comboBoxOcrMethod.SelectedIndex == 3)
            {
                if (_nocrChars == null)
                    LoadNOcrWithCurrentLanguage();

                if (_nocrChars == null)
                {
                    MessageBox.Show("Fatal - No NOCR dictionary loaded!");
                    SetButtonsEnabledAfterOcrDone();
                    return;
                }

                _nocrThreadsStop = false;
                _nocrThreads = new List<BackgroundWorker>();
                _nocrThreadResults = new string[_subtitle.Paragraphs.Count];
                int noOfThreads = Environment.ProcessorCount - 1;
                if (noOfThreads >= max)
                    noOfThreads = max - 1;
                int start = (int)numericUpDownStartNumber.Value + 5;
                if (noOfThreads >= 1 && max > 5)
                {
                    // finder letter size (uppercase/lowercase)
                    int testIndex = 0;
                    while (testIndex < 6 && (_nocrLastLowercaseHeight == -1 || _nocrLastUppercaseHeight == -1))
                    {
                        NOCRIntialize(GetSubtitleBitmap(testIndex));
                        testIndex++;
                    }

                    for (int i = 0; i < noOfThreads; i++)
                    {
                        if (start + i < max)
                        {
                            var bw = new BackgroundWorker();
                            var p = new NOcrThreadParameter(GetSubtitleBitmap(start + i), start + i, _nocrChars, bw, noOfThreads, _unItalicFactor, checkBoxNOcrItalic.Checked, (int)numericUpDownNumberOfPixelsIsSpaceNOCR.Value, checkBoxRightToLeft.Checked);
                            p.NOcrLastLowercaseHeight = _nocrLastLowercaseHeight;
                            p.NOcrLastUppercaseHeight = _nocrLastUppercaseHeight;
                            bw.DoWork += NOcrThreadDoWork;
                            bw.RunWorkerCompleted += NOcrThreadRunWorkerCompleted;
                            bw.RunWorkerAsync(p);
                        }
                    }
                }
            }

            progressBar1.Maximum = max;
            progressBar1.Value = 0;
            progressBar1.Visible = true;

            _mainOcrTimerMax = max;
            _mainOcrIndex = (int)numericUpDownStartNumber.Value - 1;
            _mainOcrTimer = new Timer();
            _mainOcrTimer.Tick += mainOcrTimer_Tick;
            _mainOcrTimer.Interval = 5;
            _mainOcrRunning = true;
            mainOcrTimer_Tick(null, null);

            if (comboBoxOcrMethod.SelectedIndex == 1)
            {
                _icThreadsStop = false;
                _icThreads = new List<BackgroundWorker>();
                _icThreadResults = new string[_subtitle.Paragraphs.Count];
                int noOfThreads = Environment.ProcessorCount - 2; // -1 or -2?
                if (noOfThreads >= max)
                    noOfThreads = max - 1;
                int start = (int)numericUpDownStartNumber.Value + 5;
                if (noOfThreads > 2)
                noOfThreads = 2; // Threading is not really good - subtitle picture creation should probably be threaded also/instead
                for (int i = 0; i < noOfThreads; i++)
                {
                    if (start + i < max)
                    {
                        Application.DoEvents();
                        var bw = new BackgroundWorker();
                        var p = new ImageCompareThreadParameter(GetSubtitleBitmap(start + i), start + i, _compareBitmaps, bw, noOfThreads, (int)numericUpDownPixelsIsSpace.Value, checkBoxRightToLeft.Checked, (float)numericUpDownMaxErrorPct.Value, _compareDoc);
                        bw.DoWork += ImageCompareThreadDoWork;
                        bw.RunWorkerCompleted += ImageCompareThreadRunWorkerCompleted;
                        bw.RunWorkerAsync(p);
                    }
                }
            }
        }

        private bool MainLoop(int max, int i)
        {
            if (i >= max)
            {
                SetButtonsEnabledAfterOcrDone();
                _mainOcrRunning = false;
                return true;
            }

            var bmp = ShowSubtitleImage(i);
            var startTime = new TimeCode(TimeSpan.FromMilliseconds(GetSubtitleStartTimeMilliseconds(i)));
            var endTime = new TimeCode(TimeSpan.FromMilliseconds(GetSubtitleEndTimeMilliseconds(i)));
            labelStatus.Text = string.Format("{0} / {1}: {2} - {3}", i + 1, max, startTime, endTime);
            progressBar1.Value = i + 1;
            labelStatus.Refresh();
            progressBar1.Refresh();
            if (_abort)
            {
                SetButtonsEnabledAfterOcrDone();
                _mainOcrRunning = false;
                return true;
            }

            _mainOcrBitmap = bmp;
            subtitleListView1.SelectIndexAndEnsureVisible(i);
            string text;
            if (comboBoxOcrMethod.SelectedIndex == 0)
                text = OcrViaTesseract(bmp, i);
            else if (comboBoxOcrMethod.SelectedIndex == 1)
                text = SplitAndOcrBitmapNormal(bmp, i);
            else if (comboBoxOcrMethod.SelectedIndex == 2)
                text = CallModi(i);
            else
                text = OcrViaNOCR(bmp, i);

            _lastLine = text;

            text = text.Replace("<i>-</i>", "-");
            text = text.Replace("<i>a</i>", "a");
            text = text.Replace("  ", " ");
            text = text.Trim();

            text = text.Replace(" " + Environment.NewLine, Environment.NewLine);
            text = text.Replace(Environment.NewLine + " ", Environment.NewLine);

            // max allow 2 lines
            if (checkBoxAutoBreakLines.Checked && text.Replace(Environment.NewLine, "*").Length + 2 <= text.Length)
            {
                text = text.Replace(" " + Environment.NewLine, Environment.NewLine);
                text = text.Replace(Environment.NewLine + " ", Environment.NewLine);
                while (text.Contains(Environment.NewLine + Environment.NewLine))
                    text = text.Replace(Environment.NewLine + Environment.NewLine, Environment.NewLine);

                if (text.Replace(Environment.NewLine, "*").Length + 2 <= text.Length)
                    text = Utilities.AutoBreakLine(text);
            }

            if (_dvbSubtitles != null && checkBoxTransportStreamGetColorAndSplit.Checked)
            {
                text = Utilities.UnbreakLine(text);
                if (_dvbSubColor != Color.Transparent)
                    text = "<font color=\"" + ColorTranslator.ToHtml(_dvbSubColor) + "\">" + text + "</font>";
            }

            if (_abort)
            {
                textBoxCurrentText.Text = text;
                _mainOcrRunning = false;
                SetButtonsEnabledAfterOcrDone();
                _nocrThreadsStop = true;
                _icThreadsStop = true;
                return true;
            }

            text = text.Trim();
            text = text.Replace("  ", " ");
            text = text.Replace(Environment.NewLine + Environment.NewLine, Environment.NewLine);
            text = text.Replace("  ", " ");
            text = text.Replace(Environment.NewLine + Environment.NewLine, Environment.NewLine);

            Paragraph p = _subtitle.GetParagraphOrDefault(i);
            if (p != null)
                p.Text = text;
            if (subtitleListView1.SelectedItems.Count == 1 && subtitleListView1.SelectedItems[0].Index == i)
                textBoxCurrentText.Text = text;
            else
                subtitleListView1.SetText(i, text);

            return false;
        }

        void mainOcrTimer_Tick(object sender, EventArgs e)
        {
            _mainOcrTimer.Stop();
            bool done = MainLoop(_mainOcrTimerMax, _mainOcrIndex);
            if (done || _abort)
            {
                SetButtonsEnabledAfterOcrDone();
            }
            else
            {
                _mainOcrIndex++;
                _mainOcrTimer.Start();
            }

        }

        private void LoadNOcrWithCurrentLanguage()
        {
            string fileName = GetNOcrLanguageFileName();
            if (!string.IsNullOrEmpty(fileName))
            {
                LoadNOcr(fileName, true);
                fileName = fileName.Insert(fileName.Length - 4, "_User");
                LoadNOcr(fileName, false);
            }
        }

        internal void SaveNOcrWithCurrentLanguage()
        {
            SaveNOcr(GetNOcrLanguageFileName());
        }

        private Bitmap ResizeBitmap(Bitmap b, int width, int height)
        {
            var result = new Bitmap(width, height);
            using (Graphics g = Graphics.FromImage(result))
                g.DrawImage(b, 0, 0, width, height);
            return result;
        }

        public static Bitmap UnItalic(Bitmap bmp, double factor)
        {
            int left = (int)(bmp.Height * factor);
            Bitmap unItaliced = new Bitmap(bmp.Width + left + 4, bmp.Height);

            Point[] destinationPoints = {
                new Point(0, 0),  // destination for upper-left point of  original
                new Point(bmp.Width, 0), // destination for upper-right point of original
                new Point(left, bmp.Height)   // destination for lower-left point of original
            };

            using (var g = Graphics.FromImage(unItaliced))
            {
                g.DrawImage(bmp, destinationPoints);
            }
            return unItaliced;
        }

        private string Tesseract3DoOcrViaExe(Bitmap bmp, string language, string psmMode)
        {
            // change yellow color to white - easier for Tesseract
            var nbmp = new NikseBitmap(bmp);
            nbmp.ReplaceYellowWithWhite(); // optimized replace
            bool useHocr = true;

            string tempTiffFileName = Path.GetTempPath() + Guid.NewGuid().ToString() + ".png";
            var b = nbmp.GetBitmap();
            b.Save(tempTiffFileName, System.Drawing.Imaging.ImageFormat.Png);
            string tempTextFileName = Path.GetTempPath() + Guid.NewGuid().ToString();
            b.Dispose();

            var process = new Process();
            process.StartInfo = new ProcessStartInfo(Configuration.TesseractFolder + "tesseract.exe");
            process.StartInfo.UseShellExecute = true;
            process.StartInfo.Arguments = "\"" + tempTiffFileName + "\" \"" + tempTextFileName + "\" -l " + language;

            if (checkBoxTesseractMusicOn.Checked)
                process.StartInfo.Arguments += "+music";

            if (!string.IsNullOrEmpty(psmMode))
                process.StartInfo.Arguments += " " + psmMode.Trim();

            if (useHocr)
                process.StartInfo.Arguments += " hocr";
            process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;

            if (Utilities.IsRunningOnLinux() || Utilities.IsRunningOnMac())
            {
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardError = true;
                process.StartInfo.FileName = "tesseract";
            }
            else
            {
                process.StartInfo.WorkingDirectory = (Configuration.TesseractFolder);
            }

            try
            {
                process.Start();
            }
            catch
            {
                MessageBox.Show("Unable to start 'tesseract' - make sure tesseract-ocr 3.x is installed!");
                throw;
            }
            process.WaitForExit(5000);

            string result = string.Empty;
            if (useHocr)
            {
                string outputFileName = tempTextFileName + ".html";
                try
                {
                    if (File.Exists(outputFileName))
                    {
                        result = File.ReadAllText(outputFileName);
                        result = ParseHocr(result);
                        File.Delete(outputFileName);
                    }
                    File.Delete(tempTiffFileName);
                }
                catch
                {
                }
            }
            else
            {
                string outputFileName = tempTextFileName + ".txt";
                try
                {
                    if (File.Exists(outputFileName))
                    {
                        result = File.ReadAllText(outputFileName);
                        File.Delete(outputFileName);
                    }
                    File.Delete(tempTiffFileName);
                }
                catch
                {
                }
            }
            return result;
        }

        private string ParseHocr(string html)
        {
            string s = html.Replace("<em>", "@001_____").Replace("</em>", "@002_____");

            int first = s.IndexOf("<");
            while (first >= 0)
            {
                int last = s.IndexOf(">", first);
                if (last > 0)
                {
                    s = s.Remove(first, last - first + 1);
                    first = s.IndexOf("<");
                }
                else
                {
                    first = -1;
                }
            }

            s = s.Trim();
            s = s.Replace("@001_____", "<i>").Replace("@002_____", "</i>");
            while (s.Contains("  "))
                s = s.Replace("  ", " ");
            s = s.Replace("</i> <i>", " ");

            // html escape decoding
            s = s.Replace("&amp;", "&");
            s = s.Replace("&lt;", "<");
            s = s.Replace("&gt;", ">");
            s = s.Replace("&quot;", "\"");
            s = s.Replace("&#39;", "'");
            s = s.Replace("&apos;", "'");

            while (s.Contains("\n\n"))
                s = s.Replace("\n\n", "\n");
            s = s.Replace("</i>\n<i>", "\n");
            s = s.Replace("\n", Environment.NewLine);

            return s;
        }

        private bool HasSingleLetters(string line)
        {
            if (!_ocrFixEngine.IsDictionaryLoaded || !_ocrFixEngine.SpellCheckDictionaryName.StartsWith("en_"))
                return false;

            if (line.Contains("[") && line.Contains("]"))
                line = line.Replace("[", string.Empty).Replace("]", string.Empty);

            int count = 0;
            var arr = line.Replace("<i>", string.Empty).Replace("</i>", string.Empty).Replace("a.m", string.Empty).Replace("p.m", string.Empty).
                           Replace("o.r", string.Empty).Replace("e.g", string.Empty).Replace("Ph.D", string.Empty).Replace("d.t.s", string.Empty).
                           Split(" .?!()\r\n\t".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            foreach (string s in arr)
            {
                if (s.Length == 1 && !"♪♫-:'”1234567890&aAI\"".Contains(s))
                    count++;
            }
            if (count > 0)
                return true;
            return false;
        }

        private string OcrViaTesseract(Bitmap bitmap, int index)
        {
            if (bitmap == null)
                return string.Empty;

            if (_ocrFixEngine == null)
                comboBoxDictionaries_SelectedIndexChanged(null, null);

            int badWords = 0;
            string textWithOutFixes;
            if (!string.IsNullOrEmpty(_tesseractAsyncStrings[index]))
            {
                textWithOutFixes = _tesseractAsyncStrings[index];
            }
            else
            {
                if (_tesseractAsyncIndex <= index)
                    _tesseractAsyncIndex = index + 10;
                textWithOutFixes = Tesseract3DoOcrViaExe(bitmap, _languageId, "-psm 6"); // 6 = Assume a single uniform block of text.
            }

            if ((!textWithOutFixes.Contains(Environment.NewLine) || Utilities.CountTagInText("\n", textWithOutFixes) > 2)
                && (textWithOutFixes.Length < 17 || bitmap.Height < 50))
            {
                string psm = Tesseract3DoOcrViaExe(bitmap, _languageId, "-psm 7"); // 7 = Treat the image as a single text line.
                if (textWithOutFixes != psm)
                {
                    if (textWithOutFixes.Trim().Length == 0)
                    {
                        textWithOutFixes = psm;
                    }
                    else if (psm.Length > textWithOutFixes.Length)
                    {
                        if (!psm.Contains("9") && textWithOutFixes.Contains("9") ||
                            !psm.Contains("6") && textWithOutFixes.Contains("6") ||
                            !psm.Contains("5") && textWithOutFixes.Contains("5") ||
                            !psm.Contains("3") && textWithOutFixes.Contains("3") ||
                            !psm.Contains("1") && textWithOutFixes.Contains("1") ||
                            !psm.Contains("$") && textWithOutFixes.Contains("$") ||
                            !psm.Contains("•") && textWithOutFixes.Contains("•") ||
                            !psm.Contains("Y") && textWithOutFixes.Contains("Y") ||
                            !psm.Contains("'") && textWithOutFixes.Contains("'") ||
                            !psm.Contains("€") && textWithOutFixes.Contains("€"))

                            textWithOutFixes = psm;
                    }
                    else if (psm.Length == textWithOutFixes.Length &&
                             (!psm.Contains("0") && textWithOutFixes.Contains("0") ||  // these chars are often mistaken
                              !psm.Contains("9") && textWithOutFixes.Contains("9") ||
                              !psm.Contains("8") && textWithOutFixes.Contains("8") ||
                              !psm.Contains("5") && textWithOutFixes.Contains("5") ||
                              !psm.Contains("3") && textWithOutFixes.Contains("3") ||
                              !psm.Contains("1") && textWithOutFixes.Contains("1") ||
                              !psm.Contains("$") && textWithOutFixes.Contains("$") ||
                              !psm.Contains("€") && textWithOutFixes.Contains("€") ||
                              !psm.Contains("•") && textWithOutFixes.Contains("•") ||
                              !psm.Contains("Y") && textWithOutFixes.Contains("Y") ||
                              !psm.Contains("'") && textWithOutFixes.Contains("'") ||
                              !psm.Contains("/") && textWithOutFixes.Contains("/") ||
                              !psm.Contains("(") && textWithOutFixes.Contains("(") ||
                              !psm.Contains(")") && textWithOutFixes.Contains(")") ||
                              !psm.Contains("_") && textWithOutFixes.Contains("_")))
                    {
                        textWithOutFixes = psm;
                    }
                    else if (psm.Length == textWithOutFixes.Length && psm.EndsWith(".") && !textWithOutFixes.EndsWith("."))
                    {
                        textWithOutFixes = psm;
                    }
                }
            }
            if (!checkBoxTesseractItalicsOn.Checked)
                textWithOutFixes = textWithOutFixes.Replace("<i>", string.Empty).Replace("</i>", string.Empty);

            // Sometimes Tesseract has problems with small fonts - it helps to make the image larger
            if (textWithOutFixes.Replace("<i>", string.Empty).Replace("</i>", string.Empty).Replace("@", string.Empty).Replace("%", string.Empty).Replace("|", string.Empty).Trim().Length < 3 ||
                Utilities.CountTagInText("\n", textWithOutFixes) > 2)
            {
                string rs = TesseractResizeAndRetry(bitmap);
                textWithOutFixes = rs;
                if (!checkBoxTesseractItalicsOn.Checked)
                    textWithOutFixes = textWithOutFixes.Replace("<i>", string.Empty).Replace("</i>", string.Empty);
            }

            // fix italics
            textWithOutFixes = FixItalics(textWithOutFixes);

            int numberOfWords = textWithOutFixes.ToString().Split((" " + Environment.NewLine).ToCharArray(), StringSplitOptions.RemoveEmptyEntries).Length;

            string line = textWithOutFixes.ToString().Trim();
            if (_ocrFixEngine.IsDictionaryLoaded)
            {
                if (checkBoxAutoFixCommonErrors.Checked)
                    line = _ocrFixEngine.FixOcrErrors(line, index, _lastLine, true, checkBoxGuessUnknownWords.Checked);
                int correctWords;
                int wordsNotFound = _ocrFixEngine.CountUnknownWordsViaDictionary(line, out correctWords);
                int oldCorrectWords = correctWords;

                if (wordsNotFound > 0 || correctWords == 0)
                {
                    List<string> oldUnkownWords = new List<string>();
                    oldUnkownWords.AddRange(_ocrFixEngine.UnknownWordsFound);
                    _ocrFixEngine.UnknownWordsFound.Clear();

                    string newUnfixedText = TesseractResizeAndRetry(bitmap);
                    string newText = _ocrFixEngine.FixOcrErrors(newUnfixedText, index, _lastLine, true, checkBoxGuessUnknownWords.Checked);
                    int newWordsNotFound = _ocrFixEngine.CountUnknownWordsViaDictionary(newText, out correctWords);

                    if (wordsNotFound == 1 && newWordsNotFound == 1 && newUnfixedText.EndsWith("!!") && textWithOutFixes.EndsWith("u") && newText.Length > 1)
                    {
                        _ocrFixEngine.UnknownWordsFound.Clear();
                        newText = textWithOutFixes.Substring(0, textWithOutFixes.Length - 1) + "!!";
                        newWordsNotFound = _ocrFixEngine.CountUnknownWordsViaDictionary(newText, out correctWords);
                    }
                    else if ((!newText.Contains("9") || textWithOutFixes.Contains("9")) &&
                             (!newText.Replace("</i>", string.Empty).Contains("/") || textWithOutFixes.Replace("</i>", string.Empty).Contains("/")) &&
                             newUnfixedText.Trim().Length > 0 &&
                             newWordsNotFound < wordsNotFound || (newWordsNotFound == wordsNotFound && newText.EndsWith("!") && textWithOutFixes.EndsWith("l")))
                    {
                        wordsNotFound = newWordsNotFound;
                        if (textWithOutFixes.Length > 3 && textWithOutFixes.EndsWith("...") && !newText.EndsWith(".") && !newText.EndsWith(",") && !newText.EndsWith("!") &&
                            !newText.EndsWith("?") && !newText.EndsWith("</i>"))
                            newText = newText.TrimEnd() + "...";
                        else if (textWithOutFixes.Length > 0 && textWithOutFixes.EndsWith(".") && !newText.EndsWith(".") && !newText.EndsWith(",") && !newText.EndsWith("!") &&
                            !newText.EndsWith("?") && !newText.EndsWith("</i>"))
                            newText = newText.TrimEnd() + ".";
                        else if (textWithOutFixes.Length > 0 && textWithOutFixes.EndsWith("?") && !newText.EndsWith(".") && !newText.EndsWith(",") && !newText.EndsWith("!") &&
                            !newText.EndsWith("?") && !newText.EndsWith("</i>"))
                            newText = newText.TrimEnd() + "?";

                        textWithOutFixes = newUnfixedText;
                        line = FixItalics(newText);
                    }
                    else if (correctWords > oldCorrectWords + 1 || (correctWords > oldCorrectWords && !textWithOutFixes.Contains(" ")))
                    {
                        wordsNotFound = newWordsNotFound;
                        textWithOutFixes = newUnfixedText;
                        line = newText;
                    }
                    else
                    {
                        _ocrFixEngine.UnknownWordsFound.Clear();
                        _ocrFixEngine.UnknownWordsFound.AddRange(oldUnkownWords);
                    }
                }


                if (wordsNotFound > 0 || correctWords == 0 || textWithOutFixes != null && textWithOutFixes.ToString().Replace("~", string.Empty).Trim().Length < 2)
                {
                    if (_bluRaySubtitles != null && !line.Contains("<i>"))
                    {
                        _ocrFixEngine.AutoGuessesUsed.Clear();
                        _ocrFixEngine.UnknownWordsFound.Clear();

                        // which is best - normal image or one color image?
                        var nbmp = new NikseBitmap(bitmap);
                        nbmp.MakeOneColor(Color.White);
                        Bitmap oneColorBitmap = nbmp.GetBitmap();
                        string oneColorText = Tesseract3DoOcrViaExe(oneColorBitmap, _languageId, "-psm 6"); // 6 = Assume a single uniform block of text.
                        oneColorBitmap.Dispose();
                        nbmp = null;

                        if (oneColorText.Length > 1 &&
                            !oneColorText.Contains("CD") &&
                            (!oneColorText.Contains("0") || line.Contains("0")) &&
                            (!oneColorText.Contains("2") || line.Contains("2")) &&
                            (!oneColorText.Contains("3") || line.Contains("4")) &&
                            (!oneColorText.Contains("5") || line.Contains("5")) &&
                            (!oneColorText.Contains("9") || line.Contains("9")) &&
                            (!oneColorText.Contains("•") || line.Contains("•")) &&
                            (!oneColorText.Contains(")") || line.Contains(")")) &&
                            Utilities.CountTagInText(oneColorText, "(") < 2 && Utilities.CountTagInText(oneColorText, ")") < 2 &&
                            Utilities.CountTagInText(oneColorText, Environment.NewLine) < 3)
                        {
                            int modiCorrectWords;
                            int modiWordsNotFound = _ocrFixEngine.CountUnknownWordsViaDictionary(oneColorText, out modiCorrectWords);
                            string modiTextOcrFixed = oneColorText;
                            if (checkBoxAutoFixCommonErrors.Checked)
                                modiTextOcrFixed = _ocrFixEngine.FixOcrErrors(oneColorText, index, _lastLine, false, checkBoxGuessUnknownWords.Checked);
                            int modiOcrCorrectedCorrectWords;
                            int modiOcrCorrectedWordsNotFound = _ocrFixEngine.CountUnknownWordsViaDictionary(modiTextOcrFixed, out modiOcrCorrectedCorrectWords);
                            if (modiOcrCorrectedWordsNotFound <= modiWordsNotFound)
                            {
                                oneColorText = modiTextOcrFixed;
                                modiWordsNotFound = modiOcrCorrectedWordsNotFound;
                                modiCorrectWords = modiOcrCorrectedCorrectWords;
                            }

                            if (modiWordsNotFound < wordsNotFound || (textWithOutFixes.Length == 1 && modiWordsNotFound == 0))
                            {
                                line = FixItalics(oneColorText); // use one-color text
                                wordsNotFound = modiWordsNotFound;
                                correctWords = modiCorrectWords;
                                if (checkBoxAutoFixCommonErrors.Checked)
                                    line = _ocrFixEngine.FixOcrErrors(line, index, _lastLine, true, checkBoxGuessUnknownWords.Checked);
                            }
                            else if (wordsNotFound == modiWordsNotFound && oneColorText.EndsWith("!") && (line.EndsWith("l") || line.EndsWith("ﬂ")))
                            {
                                line = FixItalics(oneColorText);
                                wordsNotFound = modiWordsNotFound;
                                correctWords = modiCorrectWords;
                                if (checkBoxAutoFixCommonErrors.Checked)
                                    line = _ocrFixEngine.FixOcrErrors(line, index, _lastLine, true, checkBoxGuessUnknownWords.Checked);
                            }
                        }
                    }
                }

                if (checkBoxTesseractItalicsOn.Checked)
                {
                    if (line.Contains("<i>") || wordsNotFound > 0 || correctWords == 0 || textWithOutFixes != null && textWithOutFixes.ToString().Replace("~", string.Empty).Trim().Length < 2)
                    {
                        _ocrFixEngine.AutoGuessesUsed.Clear();
                        _ocrFixEngine.UnknownWordsFound.Clear();

                        // which is best - normal image or de-italic'ed? We find out here
                        var unItalicedBmp = UnItalic(bitmap, _unItalicFactor);
                        string unItalicText = Tesseract3DoOcrViaExe(unItalicedBmp, _languageId, "-psm 6"); // 6 = Assume a single uniform block of text.
                        unItalicedBmp.Dispose();

                        if (unItalicText.Length > 1)
                        {
                            int modiCorrectWords;
                            int modiWordsNotFound = _ocrFixEngine.CountUnknownWordsViaDictionary(unItalicText, out modiCorrectWords);
                            string modiTextOcrFixed = unItalicText;
                            if (checkBoxAutoFixCommonErrors.Checked)
                                modiTextOcrFixed = _ocrFixEngine.FixOcrErrors(unItalicText, index, _lastLine, false, checkBoxGuessUnknownWords.Checked);
                            int modiOcrCorrectedCorrectWords;
                            int modiOcrCorrectedWordsNotFound = _ocrFixEngine.CountUnknownWordsViaDictionary(modiTextOcrFixed, out modiOcrCorrectedCorrectWords);
                            if (modiOcrCorrectedWordsNotFound <= modiWordsNotFound)
                            {
                                unItalicText = modiTextOcrFixed;
                                modiWordsNotFound = modiOcrCorrectedWordsNotFound;
                                modiCorrectWords = modiOcrCorrectedCorrectWords;
                            }

                            bool ok = modiWordsNotFound < wordsNotFound || (textWithOutFixes.Length == 1 && modiWordsNotFound == 0);

                            if (!ok)
                                ok = wordsNotFound == modiWordsNotFound && unItalicText.EndsWith("!") && (line.EndsWith("l") || line.EndsWith("ﬂ"));

                            if (!ok)
                                ok = wordsNotFound == modiWordsNotFound && line.StartsWith("<i>") && line.EndsWith("</i>");

                            if (ok && Utilities.CountTagInText(unItalicText, "/") > Utilities.CountTagInText(line, "/") + 1)
                                ok = false;
                            if (ok && Utilities.CountTagInText(unItalicText, "\\") > Utilities.CountTagInText(line, "\\"))
                                ok = false;
                            if (ok && Utilities.CountTagInText(unItalicText, ")") > Utilities.CountTagInText(line, ")") + 1)
                                ok = false;
                            if (ok && Utilities.CountTagInText(unItalicText, "(") > Utilities.CountTagInText(line, "(") + 1)
                                ok = false;
                            if (ok && Utilities.CountTagInText(unItalicText, "$") > Utilities.CountTagInText(line, "$") + 1)
                                ok = false;
                            if (ok && Utilities.CountTagInText(unItalicText, "€") > Utilities.CountTagInText(line, "€") + 1)
                                ok = false;
                            if (ok && Utilities.CountTagInText(unItalicText, "•") > Utilities.CountTagInText(line, "•"))
                                ok = false;

                            if (ok)
                            {
                                wordsNotFound = modiWordsNotFound;
                                correctWords = modiCorrectWords;

                                line = line.Replace("<i>", string.Empty).Replace("</i>", string.Empty).Trim();

                                if (line.Length > 7 && unItalicText.Length > 7 && unItalicText.StartsWith("I ") &&
                                    line.StartsWith(unItalicText.Remove(0, 2).Substring(0, 4)))
                                    unItalicText = unItalicText.Remove(0, 2);

                                if (checkBoxTesseractMusicOn.Checked)
                                {
                                    if ((line.StartsWith("J' ") || line.StartsWith("J“ ") || line.StartsWith("J* ") || line.StartsWith("♪ ")) && unItalicText.Length > 3 && unItalicText.Replace("<i>", string.Empty).Replace("</i>", string.Empty).Substring(1, 2) == "' ")
                                    {
                                        unItalicText = "♪ " + unItalicText.Remove(0, 2).TrimStart();
                                    }
                                    if ((line.StartsWith("J' ") || line.StartsWith("J“ ") || line.StartsWith("J* ") || line.StartsWith("♪ ")) && unItalicText.Length > 3 && unItalicText.Replace("<i>", string.Empty).Replace("</i>", string.Empty).Substring(1, 1) == " ")
                                    {
                                        bool ita = unItalicText.StartsWith("<i>") && unItalicText.EndsWith("</i>");
                                        unItalicText = Utilities.RemoveHtmlTags(unItalicText);
                                        unItalicText = "♪ " + unItalicText.Remove(0, 2).TrimStart();
                                        if (ita)
                                            unItalicText = "<i>" + unItalicText + "</i>";
                                    }
                                    if ((line.StartsWith("J' ") || line.StartsWith("J“ ") || line.StartsWith("J* ") || line.StartsWith("♪ ")) && unItalicText.Length > 3 && unItalicText.Replace("<i>", string.Empty).Replace("</i>", string.Empty).Substring(2, 1) == " ")
                                    {
                                        bool ita = unItalicText.StartsWith("<i>") && unItalicText.EndsWith("</i>");
                                        unItalicText = Utilities.RemoveHtmlTags(unItalicText);
                                        unItalicText = "♪ " + unItalicText.Remove(0, 2).TrimStart();
                                        if (ita)
                                            unItalicText = "<i>" + unItalicText + "</i>";
                                    }
                                    if (unItalicText.StartsWith("J'") && (line.StartsWith("♪") || textWithOutFixes.StartsWith("♪") || textWithOutFixes.StartsWith("<i>♪") || unItalicText.EndsWith("♪")))
                                    {
                                        bool ita = unItalicText.StartsWith("<i>") && unItalicText.EndsWith("</i>");
                                        unItalicText = Utilities.RemoveHtmlTags(unItalicText);
                                        unItalicText = "♪ " + unItalicText.Remove(0, 2).TrimStart();
                                        if (ita)
                                            unItalicText = "<i>" + unItalicText + "</i>";
                                    }
                                    if ((line.StartsWith("J` ") || line.StartsWith("J“ ") || line.StartsWith("J' ") || line.StartsWith("J* ")) && unItalicText.StartsWith("S "))
                                    {
                                        bool ita = unItalicText.StartsWith("<i>") && unItalicText.EndsWith("</i>");
                                        unItalicText = Utilities.RemoveHtmlTags(unItalicText);
                                        unItalicText = "♪ " + unItalicText.Remove(0, 2).TrimStart();
                                        if (ita)
                                            unItalicText = "<i>" + unItalicText + "</i>";
                                    }
                                    if ((line.StartsWith("J` ") || line.StartsWith("J“ ") || line.StartsWith("J' ") || line.StartsWith("J* ")) && unItalicText.StartsWith("<i>S</i> "))
                                    {
                                        bool ita = unItalicText.StartsWith("<i>") && unItalicText.EndsWith("</i>");
                                        unItalicText = Utilities.RemoveHtmlTags(unItalicText);
                                        unItalicText = "♪ " + unItalicText.Remove(0, 8).TrimStart();
                                        if (ita)
                                            unItalicText = "<i>" + unItalicText + "</i>";
                                    }
                                    if (unItalicText.StartsWith(";'") && (line.StartsWith("♪") || textWithOutFixes.StartsWith("♪") || textWithOutFixes.StartsWith("<i>♪") || unItalicText.EndsWith("♪")))
                                    {
                                        bool ita = unItalicText.StartsWith("<i>") && unItalicText.EndsWith("</i>");
                                        unItalicText = Utilities.RemoveHtmlTags(unItalicText);
                                        unItalicText = "♪ " + unItalicText.Remove(0, 2).TrimStart();
                                        if (ita)
                                            unItalicText = "<i>" + unItalicText + "</i>";
                                    }
                                    if (unItalicText.StartsWith(",{*") && (line.StartsWith("♪") || textWithOutFixes.StartsWith("♪") || textWithOutFixes.StartsWith("<i>♪") || unItalicText.EndsWith("♪")))
                                    {
                                        bool ita = unItalicText.StartsWith("<i>") && unItalicText.EndsWith("</i>");
                                        unItalicText = Utilities.RemoveHtmlTags(unItalicText);
                                        unItalicText = "♪ " + unItalicText.Remove(0, 3).TrimStart();
                                        if (ita)
                                            unItalicText = "<i>" + unItalicText + "</i>";
                                    }

                                    if (unItalicText.EndsWith("J'") && (line.EndsWith("♪") || textWithOutFixes.EndsWith("♪") || textWithOutFixes.EndsWith("♪</i>") || unItalicText.StartsWith("♪")))
                                    {
                                        bool ita = unItalicText.StartsWith("<i>") && unItalicText.EndsWith("</i>");
                                        unItalicText = Utilities.RemoveHtmlTags(unItalicText);
                                        unItalicText = unItalicText.Remove(unItalicText.Length - 3, 2).TrimEnd() + " ♪";
                                        if (ita)
                                            unItalicText = "<i>" + unItalicText + "</i>";
                                    }
                                }

                                if (unItalicText.StartsWith("[") && !line.StartsWith("["))
                                {
                                    unItalicText = unItalicText.Remove(0, 1);
                                    if (unItalicText.EndsWith("]"))
                                        unItalicText = unItalicText.TrimEnd(']');
                                }
                                if (unItalicText.StartsWith("{") && !line.StartsWith("{"))
                                {
                                    unItalicText = unItalicText.Remove(0, 1);
                                    if (unItalicText.EndsWith("}"))
                                        unItalicText = unItalicText.TrimEnd('}');
                                }
                                if (unItalicText.EndsWith("}") && !line.EndsWith("}"))
                                    unItalicText = unItalicText.TrimEnd('}');

                                if (line.EndsWith("...") && unItalicText.EndsWith("”!"))
                                    unItalicText = unItalicText.TrimEnd('!').TrimEnd('”') + ".";
                                if (line.EndsWith("...") && unItalicText.EndsWith("\"!"))
                                    unItalicText = unItalicText.TrimEnd('!').TrimEnd('"') + ".";

                                if (line.EndsWith(".") && !unItalicText.EndsWith(".") && !unItalicText.EndsWith(".</i>"))
                                {
                                    string post = string.Empty;
                                    if (unItalicText.EndsWith("</i>"))
                                    {
                                        post = "</i>";
                                        unItalicText = unItalicText.Remove(unItalicText.Length - 4);
                                    }
                                    if (unItalicText.EndsWith("'") && !line.EndsWith("'."))
                                        unItalicText = unItalicText.TrimEnd('\'');
                                    unItalicText += "." + post;
                                }
                                if (unItalicText.EndsWith(".") && !unItalicText.EndsWith("...") && !unItalicText.EndsWith("...</i>") && line.EndsWith("..."))
                                {
                                    string post = string.Empty;
                                    if (unItalicText.EndsWith("</i>"))
                                    {
                                        post = "</i>";
                                        unItalicText = unItalicText.Remove(unItalicText.Length - 4);
                                    }
                                    unItalicText += ".." + post;
                                }
                                if (unItalicText.EndsWith("..") && !unItalicText.EndsWith("...") && !unItalicText.EndsWith("...</i>") && line.EndsWith("..."))
                                {
                                    string post = string.Empty;
                                    if (unItalicText.EndsWith("</i>"))
                                    {
                                        post = "</i>";
                                        unItalicText = unItalicText.Remove(unItalicText.Length - 4);
                                    }
                                    unItalicText += "." + post;
                                }

                                if (line.EndsWith("!") && !unItalicText.EndsWith("!") && !unItalicText.EndsWith("!</i>"))
                                {
                                    if (unItalicText.EndsWith("!'"))
                                    {
                                        unItalicText = unItalicText.TrimEnd('\'');
                                    }
                                    else
                                    {
                                        if (unItalicText.EndsWith("l</i>") && _ocrFixEngine != null)
                                        {
                                            string w = unItalicText.Substring(0, unItalicText.Length - 4);
                                            int wIdx = w.Length - 1;
                                            while (wIdx >= 0 && !(" .,!?<>:;'-$@£()[]<>/\"".Contains(w[wIdx].ToString())))
                                            {
                                                wIdx--;
                                            }
                                            if (wIdx + 1 < w.Length && unItalicText.Length > 5)
                                            {
                                                w = w.Substring(wIdx + 1);
                                                if (!_ocrFixEngine.DoSpell(w))
                                                    unItalicText = unItalicText.Remove(unItalicText.Length - 5, 1);
                                            }
                                            unItalicText = unItalicText.Insert(unItalicText.Length - 4, "!");
                                        }
                                        else if (unItalicText.EndsWith("l") && _ocrFixEngine != null)
                                        {
                                            string w = unItalicText;
                                            int wIdx = w.Length - 1;
                                            while (wIdx >= 0 && !(" .,!?<>:;'-$@£()[]<>/\"".Contains(w[wIdx].ToString())))
                                            {
                                                wIdx--;
                                            }
                                            if (wIdx + 1 < w.Length && unItalicText.Length > 5)
                                            {
                                                w = w.Substring(wIdx + 1);
                                                if (!_ocrFixEngine.DoSpell(w))
                                                    unItalicText = unItalicText.Remove(unItalicText.Length - 1, 1);
                                            }
                                            unItalicText += "!";
                                        }
                                        else
                                        {
                                            unItalicText += "!";
                                        }

                                    }
                                }
                                if (line.EndsWith("?") && !unItalicText.EndsWith("?") && !unItalicText.EndsWith("?</i>"))
                                {
                                    if (unItalicText.EndsWith("?'"))
                                        unItalicText = unItalicText.TrimEnd('\'');
                                    else
                                        unItalicText += "?";
                                }

                                line = unItalicText.Replace("<i>", string.Empty).Replace("</i>", string.Empty);
                                if (checkBoxAutoFixCommonErrors.Checked)
                                {
                                    if (line.Contains("'.") && !textWithOutFixes.Contains("'.") && textWithOutFixes.Contains(":") && !line.EndsWith("'.") && Configuration.Settings.Tools.OcrFixUseHardcodedRules)
                                    {
                                        line = line.Replace("'.", ":");
                                    }
                                    line = _ocrFixEngine.FixOcrErrors(line, index, _lastLine, true, checkBoxGuessUnknownWords.Checked);
                                }
                                line = "<i>" + line + "</i>";
                            }
                            else
                            {
                                unItalicText = unItalicText.Replace("</i>", string.Empty);
                                if (line.EndsWith("</i>") && unItalicText.EndsWith("."))
                                {
                                    line = line.Remove(line.Length - 4, 4);
                                    if (line.EndsWith("-"))
                                        line = line.TrimEnd('-') + ".";
                                    if (Utilities.AllLetters.Contains(line.Substring(line.Length - 1)))
                                        line += ".";
                                    line += "</i>";
                                }
                            }
                        }
                    }
                }

                if (checkBoxTesseractMusicOn.Checked)
                {
                    if (line == "[J'J'J~]" || line == "[J'J'J']")
                        line = "[ ♪ ♪ ♪ ]";

                    line = line.Replace(" J' ", " ♪ ");

                    if (line.StartsWith("J'"))
                    {
                        line = "♪ " + line.Remove(0, 2).TrimStart();
                    }
                    if (line.StartsWith("<i>J'"))
                    {
                        line = "<i>♪ " + line.Remove(0, 5).TrimStart();
                    }
                    if (line.StartsWith("[J'"))
                    {
                        line = "[♪ " + line.Remove(0, 3).TrimStart();
                    }
                    if (line.StartsWith("<i>[J'"))
                    {
                        line = "<i>[♪ " + line.Remove(0, 6).TrimStart();
                    }
                    if (line.EndsWith("J'"))
                    {
                        line = line.Remove(line.Length - 2, 2).TrimEnd() + " ♪";
                    }
                    if (line.EndsWith("J'</i>"))
                    {
                        line = line.Remove(line.Length - 6, 6).TrimEnd() + " ♪</i>";
                    }
                    if (line.Contains(Environment.NewLine + "J'"))
                    {
                        line = line.Replace(Environment.NewLine + "J'", Environment.NewLine + "♪ ");
                        line = line.Replace("  ", " ");
                    }
                    if (line.Contains("J'" + Environment.NewLine))
                    {
                        line = line.Replace("J'" + Environment.NewLine, " ♪" + Environment.NewLine);
                        line = line.Replace("  ", " ");
                    }
                }


                if (wordsNotFound > 0 || correctWords == 0 || textWithOutFixes != null && textWithOutFixes.ToString().Replace("~", string.Empty).Trim().Length < 2)
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

                        if (modiText.Length > 1 &&
                            !modiText.Contains("CD") &&
                            (!modiText.Contains("0") || line.Contains("0")) &&
                            (!modiText.Contains("2") || line.Contains("2")) &&
                            (!modiText.Contains("3") || line.Contains("4")) &&
                            (!modiText.Contains("5") || line.Contains("5")) &&
                            (!modiText.Contains("9") || line.Contains("9")) &&
                            (!modiText.Contains("•") || line.Contains("•")) &&
                            (!modiText.Contains(")") || line.Contains(")")) &&
                            Utilities.CountTagInText(modiText, "(") < 2 && Utilities.CountTagInText(modiText, ")") < 2 &&
                            Utilities.CountTagInText(modiText, Environment.NewLine) < 3)
                        {
                            int modiWordsNotFound = _ocrFixEngine.CountUnknownWordsViaDictionary(modiText, out correctWords);
                            //if (modiWordsNotFound > 0)
                            {
                                string modiTextOcrFixed = modiText;
                                if (checkBoxAutoFixCommonErrors.Checked)
                                    modiTextOcrFixed = _ocrFixEngine.FixOcrErrors(modiText, index, _lastLine, false, checkBoxGuessUnknownWords.Checked);
                                int modiOcrCorrectedWordsNotFound = _ocrFixEngine.CountUnknownWordsViaDictionary(modiTextOcrFixed, out correctWords);
                                if (modiOcrCorrectedWordsNotFound <= modiWordsNotFound)
                                    modiText = modiTextOcrFixed;
                            }

                            if (modiWordsNotFound < wordsNotFound || (textWithOutFixes.Length == 1 && modiWordsNotFound == 0))
                                line = modiText; // use the modi ocr'ed text
                            else if (wordsNotFound == modiWordsNotFound && modiText.EndsWith("!") && (line.EndsWith("l") || line.EndsWith("ﬂ")))
                                line = modiText;
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

                //check tesseract... find some otherway to do this...
                //string tmp = Utilities.RemoveHtmlTags(line).Trim();
                //if (!tmp.Trim().EndsWith("..."))
                //{
                //    tmp = tmp.TrimEnd('.').TrimEnd();
                //    if (tmp.Length > 2 && Utilities.LowercaseLetters.Contains(tmp.Substring(tmp.Length - 1, 1)))
                //    {
                //        if (_nocrChars == null)
                //            _nocrChars = LoadNOcrForTesseract("Nikse.SubtitleEdit.Resources.nOCR_TesseractHelper.xml.zip");
                //        string text = Utilities.RemoveHtmlTags(NocrFastCheck(bitmap).TrimEnd());
                //        string post = string.Empty;
                //        if (line.EndsWith("</i>"))
                //        {
                //            post = "</i>";
                //            line = line.Remove(line.Length - 4, 4).Trim();
                //        }
                //        if (text.EndsWith("."))
                //        {
                //            line = line.TrimEnd('.').Trim();
                //            while (text.EndsWith(".") || text.EndsWith(" "))
                //            {
                //                line += text.Substring(text.Length - 1).Trim();
                //                text = text.Remove(text.Length - 1, 1);
                //            }
                //        }
                //        else if (text.EndsWith("l") && text.EndsWith("!") && !text.EndsWith("l!"))
                //        {
                //            line = line.Remove(line.Length - 1, 1) + "!";
                //        }
                //        line += post;
                //    }
                //}

                // Log used word guesses (via word replace list)
                foreach (string guess in _ocrFixEngine.AutoGuessesUsed)
                    listBoxLogSuggestions.Items.Add(guess);
                _ocrFixEngine.AutoGuessesUsed.Clear();

                // Log unkown words guess (found via spelling dictionaries)
                LogUnknownWords();

                if (wordsNotFound >= 3)
                    subtitleListView1.SetBackgroundColor(index, Color.Red);
                if (wordsNotFound == 2)
                    subtitleListView1.SetBackgroundColor(index, Color.Orange);
                else if (wordsNotFound == 1 || line.Length == 1 || line.Contains("_") || HasSingleLetters(line))
                    subtitleListView1.SetBackgroundColor(index, Color.Yellow);
                else if (line.Trim().Length == 0)
                    subtitleListView1.SetBackgroundColor(index, Color.Orange);
                else
                    subtitleListView1.SetBackgroundColor(index, Color.LightGreen);
            }
            else
            { // no dictionary :(
                if (checkBoxAutoFixCommonErrors.Checked)
                    line = _ocrFixEngine.FixOcrErrors(line, index, _lastLine, true, checkBoxGuessUnknownWords.Checked);

                if (badWords >= numberOfWords)
                    subtitleListView1.SetBackgroundColor(index, Color.Red);
                else if (badWords >= numberOfWords / 2)
                    subtitleListView1.SetBackgroundColor(index, Color.Orange);
                else if (badWords > 0 || line.Contains("_") || HasSingleLetters(line))
                    subtitleListView1.SetBackgroundColor(index, Color.Yellow);
                else if (line.Replace("<i>", string.Empty).Replace("</i>", string.Empty).Trim().Length == 0)
                    subtitleListView1.SetBackgroundColor(index, Color.Orange);
                else
                    subtitleListView1.SetBackgroundColor(index, Color.LightGreen);
            }

            if (textWithOutFixes.ToString().Trim() != line.Trim())
            {
                _tesseractOcrAutoFixes++;
                labelFixesMade.Text = string.Format(" - {0}", _tesseractOcrAutoFixes);
                LogOcrFix(index, textWithOutFixes.ToString(), line);
            }

            if (_vobSubMergedPackist != null)
                bitmap.Dispose();

            return line;
        }

        private static string FixItalics(string s)
        {
            int italicStartCount = Utilities.CountTagInText(s, "<i>");

            s = s.Replace(Environment.NewLine + " ", Environment.NewLine);
            s = s.Replace(Environment.NewLine + " ", Environment.NewLine);
            s = s.Replace(" " + Environment.NewLine, Environment.NewLine);
            s = s.Replace(" " + Environment.NewLine, Environment.NewLine);
            s = s.Replace(Environment.NewLine + Environment.NewLine, Environment.NewLine);
            s = s.Replace(Environment.NewLine + Environment.NewLine, Environment.NewLine);

            if (italicStartCount == 1 && s.Contains("<i>-</i>"))
            {
                s = s.Replace("<i>-</i>", "-");
                s = s.Replace("  ", " ");
            }

            if (s.Contains("</i> / <i>"))
                s = s.Replace("</i> / <i>", " I ").Replace("  ", " ");
            if (s.StartsWith("/ <i>"))
                s = ("<i>I " + s.Remove(0, 5)).Replace("  ", " ");
            if (s.StartsWith("I <i>"))
                s = ("<i>I " + s.Remove(0, 5)).Replace("  ", " ");
            else if (italicStartCount == 1 && s.Length > 20 &&
                     s.IndexOf("<i>") > 1 && s.IndexOf("<i>") < 10 && s.EndsWith("</i>"))
                s = "<i>" + s.Replace("<i>", string.Empty).Replace("</i>", string.Empty) + "</i>";
            s = s.Replace("</i>" + Environment.NewLine + "<i>", Environment.NewLine);
            return s;
        }

        private void LogUnknownWords()
        {
            foreach (string unknownWord in _ocrFixEngine.UnknownWordsFound)
                listBoxUnknownWords.Items.Add(unknownWord);
            _ocrFixEngine.UnknownWordsFound.Clear();
        }

        private string TesseractResizeAndRetry(Bitmap bitmap)
        {
            string result = Tesseract3DoOcrViaExe(ResizeBitmap(bitmap, bitmap.Width * 3, bitmap.Height * 2), _languageId, null);
            if (result.ToString().Trim().Length == 0)
                result = Tesseract3DoOcrViaExe(ResizeBitmap(bitmap, bitmap.Width * 4, bitmap.Height * 2), _languageId, "-psm 7");
            return result.TrimEnd();
        }

        private void LogOcrFix(int index, string oldLine, string newLine)
        {
            listBoxLog.Items.Add(string.Format("#{0}: {1} -> {2}", index + 1, oldLine.Replace(Environment.NewLine, " "), newLine.Replace(Environment.NewLine, " ")));
        }

        private string CallModi(int i)
        {
            Bitmap bmp;
            try
            {
                var tmp = GetSubtitleBitmap(i);
                if (tmp == null)
                    return string.Empty;
                bmp = tmp.Clone() as Bitmap;
            }
            catch
            {
                return string.Empty;
            }
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
            if (!Directory.Exists(Configuration.TesseractFolder))
            {
                Directory.CreateDirectory(Configuration.TesseractFolder);
                if (!Utilities.IsRunningOnLinux() && !Utilities.IsRunningOnMac())
                {
                    System.Diagnostics.Process process = new System.Diagnostics.Process();
                    System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
                    startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                    startInfo.FileName = "xcopy";
                    startInfo.Arguments = "\"" + Path.Combine(Configuration.TesseractOriginalFolder, "*.*") + "\" \"" + Configuration.TesseractFolder + "\" /s";
                    MessageBox.Show(startInfo.Arguments);
                    process.StartInfo = startInfo;
                    process.Start();
                    process.WaitForExit();
                }
            }

            string dir = Path.Combine(Configuration.TesseractFolder, "tessdata");
            if (Directory.Exists(dir))
            {
                var list = new List<string>();
                comboBoxTesseractLanguages.Items.Clear();
                foreach (var culture in System.Globalization.CultureInfo.GetCultures(System.Globalization.CultureTypes.NeutralCultures))
                {
                    string tesseractName = culture.ThreeLetterISOLanguageName;
                    if (culture.LCID == 0x4 && !File.Exists(dir + Path.DirectorySeparatorChar + tesseractName + ".traineddata"))
                        tesseractName = "chi_sim";
                    if (culture.Name == "zh-CHT" && !File.Exists(dir + Path.DirectorySeparatorChar + tesseractName + ".traineddata"))
                        tesseractName = "chi_tra";
                    string trainDataFileName = dir + Path.DirectorySeparatorChar + tesseractName + ".traineddata";
                    if (!list.Contains(culture.ThreeLetterISOLanguageName) && File.Exists(trainDataFileName))
                    {
                        if (culture.ThreeLetterISOLanguageName != "zho")
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
            if (_mainOcrTimer != null)
                _mainOcrTimer.Stop();
            _abort = true;
            _nocrThreadsStop = true;
            _icThreadsStop = true;
            buttonStop.Enabled = false;
            progressBar1.Visible = false;
            labelStatus.Text = string.Empty;
            SetButtonsEnabledAfterOcrDone();
        }

        private void SubtitleListView1SelectedIndexChanged(object sender, EventArgs e)
        {
            if (subtitleListView1.SelectedItems.Count > 0)
            {
                try
                {
                    _selectedIndex = subtitleListView1.SelectedItems[0].Index;
                }
                catch
                {
                    return;
                }
                textBoxCurrentText.Text = _subtitle.Paragraphs[_selectedIndex].Text;
                if (_mainOcrRunning && _mainOcrBitmap != null)
                    ShowSubtitleImage(_selectedIndex, _mainOcrBitmap);
                else
                    ShowSubtitleImage(_selectedIndex);
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
            FixVerticalScrollBars(textBoxCurrentText);
        }

        private void FixVerticalScrollBars(TextBox tb)
        {
            var lineCount = Utilities.CountTagInText(tb.Text, Environment.NewLine) + 1;
            if (lineCount > 5)
                tb.ScrollBars = ScrollBars.Vertical;
            else
                tb.ScrollBars = ScrollBars.None;
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
            EditImageCompareCharacters(null, null);
        }

        public DialogResult EditImageCompareCharacters(string name, string text)
        {
            var formVobSubEditCharacters = new VobSubEditCharacters(comboBoxCharacterDatabase.SelectedItem.ToString(), null);
            formVobSubEditCharacters.Initialize(name, text);

            DialogResult result = formVobSubEditCharacters.ShowDialog();
            if (result == DialogResult.OK)
            {
                _compareDoc = formVobSubEditCharacters.ImageCompareDocument;
                string path = Configuration.VobSubCompareFolder + comboBoxCharacterDatabase.SelectedItem + Path.DirectorySeparatorChar;
                _compareDoc.Save(path + "Images.xml");
            }
            Cursor = Cursors.WaitCursor;
            LoadImageCompareBitmaps();
            Cursor = Cursors.Default;
            return result;
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
                    subtitleListView1.Items[goToLine.LineNumber - 1].Focused = true;
                }
            }
            else if (_mainGeneralGoToNextSubtitle == e.KeyData)
            {
                int selectedIndex = 0;
                if (subtitleListView1.SelectedItems.Count > 0)
                {
                    selectedIndex = subtitleListView1.SelectedItems[0].Index;
                    selectedIndex++;
                }
                subtitleListView1.SelectIndexAndEnsureVisible(selectedIndex);
                e.SuppressKeyPress = true;
            }
            else if (_mainGeneralGoToPrevSubtitle == e.KeyData)
            {
                int selectedIndex = 0;
                if (subtitleListView1.SelectedItems.Count > 0)
                {
                    selectedIndex = subtitleListView1.SelectedItems[0].Index;
                    selectedIndex--;
                }
                subtitleListView1.SelectIndexAndEnsureVisible(selectedIndex);
                e.SuppressKeyPress = true;
            }
        }

        private void ComboBoxTesseractLanguagesSelectedIndexChanged(object sender, EventArgs e)
        {
            Configuration.Settings.VobSubOcr.TesseractLastLanguage = (comboBoxTesseractLanguages.SelectedItem as TesseractLanguage).Id;
            _ocrFixEngine = null;
            LoadOcrFixEngine(null, null);
        }

        private void LoadOcrFixEngine(string threeLetterISOLanguageName, string hunspellName)
        {
            if (string.IsNullOrEmpty(threeLetterISOLanguageName) && comboBoxTesseractLanguages.SelectedItem != null)
            {
                _languageId = (comboBoxTesseractLanguages.SelectedItem as TesseractLanguage).Id;
                threeLetterISOLanguageName = _languageId;
            }

            _ocrFixEngine = new OcrFixEngine(threeLetterISOLanguageName, hunspellName, this);
            if (_ocrFixEngine.IsDictionaryLoaded)
            {
                string loadedDictionaryName = _ocrFixEngine.SpellCheckDictionaryName;
                int i = 0;
                comboBoxDictionaries.SelectedIndexChanged -= comboBoxDictionaries_SelectedIndexChanged;
                foreach (string item in comboBoxDictionaries.Items)
                {
                    if (item.Contains("[" + loadedDictionaryName + "]"))
                        comboBoxDictionaries.SelectedIndex = i;
                    i++;
                }
                comboBoxDictionaries.SelectedIndexChanged += comboBoxDictionaries_SelectedIndexChanged;
                comboBoxDictionaries.Left = labelDictionaryLoaded.Left + labelDictionaryLoaded.Width;
                comboBoxDictionaries.Width = groupBoxOcrAutoFix.Width - (comboBoxDictionaries.Left + 10 + buttonSpellCheckDownload.Width);
            }
            else
            {
                comboBoxDictionaries.SelectedIndex = 0;
            }

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
                checkBoxPromptForUnknownWords.Checked = false;
            }
            else if (comboBoxOcrMethod.SelectedIndex == 2)
            {
                ShowOcrMethodGroupBox(groupBoxModiMethod);
                Configuration.Settings.VobSubOcr.LastOcrMethod = "MODI";
            }
            else if (comboBoxOcrMethod.SelectedIndex == 3)
            {
                ShowOcrMethodGroupBox(groupBoxNOCR);
                Configuration.Settings.VobSubOcr.LastOcrMethod = "nOCR";
                SetSpellCheckLanguage(Configuration.Settings.VobSubOcr.LineOcrLastSpellcheck);

                comboBoxNOcrLanguage.Items.Clear();
                int index = 0;
                int selIndex = 0;
                foreach (string fileName in Directory.GetFiles(Configuration.DictionariesFolder, "nOCR_*.xml"))
                {
                    string s = Path.GetFileNameWithoutExtension(fileName);
                    s = s.Remove(0, 5);
                    if (s.Length > 0 && !s.ToLower().EndsWith("_user"))
                    {
                        if (s == Configuration.Settings.VobSubOcr.LineOcrLastLanguages)
                            selIndex = index;
                        comboBoxNOcrLanguage.Items.Add(s);
                        index++;
                    }
                }
                if (comboBoxNOcrLanguage.Items.Count > 0)
                    comboBoxNOcrLanguage.SelectedIndex = selIndex;
            }
        }

        private string GetNOcrLanguageFileName()
        {
            if (comboBoxNOcrLanguage.SelectedIndex < 0)
                return null;

            return Configuration.DictionariesFolder + "nOCR_" + comboBoxNOcrLanguage.Items[comboBoxNOcrLanguage.SelectedIndex] + ".xml";
        }


        private void ShowOcrMethodGroupBox(GroupBox groupBox)
        {
            GroupBoxTesseractMethod.Visible = false;
            groupBoxImageCompareMethod.Visible = false;
            groupBoxModiMethod.Visible = false;
            groupBoxNOCR.Visible = false;

            groupBox.Visible = true;
            groupBox.BringToFront();
            groupBox.Left = comboBoxOcrMethod.Left;
            groupBox.Top = 50;
        }

        private void ListBoxLogSelectedIndexChanged(object sender, EventArgs e)
        {
            var lb = sender as ListBox;
            if (lb != null && lb.SelectedIndex >= 0)
            {
                string text = lb.Items[lb.SelectedIndex].ToString();
                if (text.Contains(":"))
                {
                    string number = text.Substring(1, text.IndexOf(":") - 1);
                    subtitleListView1.SelectIndexAndEnsureVisible(int.Parse(number) - 1);
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

            if (comboBoxOcrMethod.SelectedIndex == 1) // image compare
            {
                toolStripSeparatorImageCompare.Visible = true;
                inspectImageCompareMatchesForCurrentImageToolStripMenuItem.Visible = true;
                EditLastAdditionsToolStripMenuItem.Visible = _lastAdditions != null && _lastAdditions.Count > 0;
            }
            else
            {
                toolStripSeparatorImageCompare.Visible = false;
                inspectImageCompareMatchesForCurrentImageToolStripMenuItem.Visible = false;
                EditLastAdditionsToolStripMenuItem.Visible = false;
            }

            if (comboBoxOcrMethod.SelectedIndex == 3) // nocr compare
            {
                toolStripMenuItemInspectNOcrMatches.Visible = true;
                toolStripSeparatorImageCompare.Visible = true;
            }
            else
            {
                toolStripMenuItemInspectNOcrMatches.Visible = false;
                toolStripSeparatorImageCompare.Visible = false;
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
                        if (item.Index == _selectedIndex)
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
                        if (item.Index == _selectedIndex)
                            textBoxCurrentText.Text = p.Text;
                    }
                }
            }
        }

        private void CheckBoxCustomFourColorsCheckedChanged(object sender, EventArgs e)
        {
            ResetTesseractThread();
            if (checkBoxCustomFourColors.Checked)
            {
                pictureBoxPattern.BackColor = Color.White;
                pictureBoxEmphasis1.BackColor = Color.Black;
                pictureBoxEmphasis2.BackColor = Color.Black;
                checkBoxBackgroundTransparent.Enabled = true;
                checkBoxPatternTransparent.Enabled = true;
                checkBoxEmphasis1Transparent.Enabled = true;
                checkBoxEmphasis2Transparent.Enabled = true;
            }
            else
            {
                pictureBoxPattern.BackColor = Color.Gray;
                pictureBoxEmphasis1.BackColor = Color.Gray;
                pictureBoxEmphasis2.BackColor = Color.Gray;
                checkBoxBackgroundTransparent.Enabled = false;
                checkBoxPatternTransparent.Enabled = false;
                checkBoxEmphasis1Transparent.Enabled = false;
                checkBoxEmphasis2Transparent.Enabled = false;
            }
            SubtitleListView1SelectedIndexChanged(null, null);
        }

        private void ResetTesseractThread()
        {
            if (_tesseractThread != null)
            {
                _tesseractThread.CancelAsync();
                for (int i = 0; i < _tesseractAsyncStrings.Length; i++)
                    _tesseractAsyncStrings[i] = string.Empty;
                _tesseractAsyncIndex = 0;
            }
        }

        private void PictureBoxColorChooserClick(object sender, EventArgs e)
        {
            if (colorDialog1.ShowDialog(this) == DialogResult.OK)
                (sender as PictureBox).BackColor = colorDialog1.Color;
            SubtitleListView1SelectedIndexChanged(null, null);
            ResetTesseractThread();
        }

        private void CheckBoxPatternTransparentCheckedChanged(object sender, EventArgs e)
        {
            SubtitleListView1SelectedIndexChanged(null, null);
            ResetTesseractThread();
        }

        private void CheckBoxEmphasis1TransparentCheckedChanged(object sender, EventArgs e)
        {
            SubtitleListView1SelectedIndexChanged(null, null);
            ResetTesseractThread();
        }

        private void CheckBoxEmphasis2TransparentCheckedChanged(object sender, EventArgs e)
        {
            SubtitleListView1SelectedIndexChanged(null, null);
            ResetTesseractThread();
        }

        private void checkBoxShowOnlyForced_CheckedChanged(object sender, EventArgs e)
        {
            if (_tesseractThread != null)
            {
                _tesseractThread.CancelAsync();
                int i = 0;
                while (i < 10 && _tesseractThread.IsBusy)
                {
                    System.Threading.Thread.Sleep(100);
                    i++;
                }
                _tesseractAsyncStrings = null;
            }

            Subtitle oldSubtitle = new Subtitle(_subtitle);
            subtitleListView1.BeginUpdate();
            if (_bdnXmlOriginal != null)
                LoadBdnXml();
            else if (_bluRaySubtitlesOriginal != null)
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
                if (p != null && p.Text.Length > 0)
                {
                    _subtitle.Paragraphs[i].Text = p.Text;
                }
            }
            subtitleListView1.Fill(_subtitle);
            subtitleListView1.EndUpdate();
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

        private void SetSpellCheckLanguage(string languageString)
        {
            if (string.IsNullOrEmpty(languageString))
                return;

            int i = 0;
            foreach (string item in comboBoxDictionaries.Items)
            {
                if (item.Contains("[" + languageString + "]"))
                    comboBoxDictionaries.SelectedIndex = i;
                i++;
            }
        }

        private void comboBoxDictionaries_SelectedIndexChanged(object sender, EventArgs e)
        {
            Configuration.Settings.General.SpellCheckLanguage = LanguageString;
            string threeLetterISOLanguageName = string.Empty;
            try
            {
                _ocrFixEngine = null;
                var ci = new CultureInfo(LanguageString.Replace("_", "-"));
                threeLetterISOLanguageName = ci.ThreeLetterISOLanguageName;
            }
            catch
            {
            }
            LoadOcrFixEngine(threeLetterISOLanguageName, LanguageString);
        }

        internal void Initialize(Subtitle bdnSubtitle, VobSubOcrSettings vobSubOcrSettings, bool isSon)
        {
            _bdnXmlOriginal = bdnSubtitle;
            _bdnFileName = bdnSubtitle.FileName;
            _isSon = isSon;
            if (_isSon)
            {
                checkBoxCustomFourColors.Checked = true;
                pictureBoxBackground.BackColor = Color.Transparent;
                pictureBoxPattern.BackColor = Color.DarkGray;
                pictureBoxEmphasis1.BackColor = Color.Black;
                pictureBoxEmphasis2.BackColor = Color.White;
            }

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
            numericUpDownPixelsIsSpace.Value = 11;
            _vobSubOcrSettings = vobSubOcrSettings;

            InitializeModi();
            InitializeTesseract();
            LoadImageCompareCharacterDatabaseList();

            SetOcrMethod();

            groupBoxImagePalette.Visible = isSon;

            Text = Configuration.Settings.Language.VobSubOcr.TitleBluRay;
            Text += " - " + Path.GetFileName(_bdnFileName);

            checkBoxAutoTransparentBackground.Checked = true;
        }

        private void SetOcrMethod()
        {
            if (Configuration.Settings.VobSubOcr.LastOcrMethod == "BitmapCompare" && comboBoxOcrMethod.Items.Count > 1)
                comboBoxOcrMethod.SelectedIndex = 1;
            else if (Configuration.Settings.VobSubOcr.LastOcrMethod == "MODI" && comboBoxOcrMethod.Items.Count > 2)
                comboBoxOcrMethod.SelectedIndex = 2;
            else if (Configuration.Settings.VobSubOcr.LastOcrMethod == "nOCR" && comboBoxOcrMethod.Items.Count > 3)
                comboBoxOcrMethod.SelectedIndex = 3;
            else
                comboBoxOcrMethod.SelectedIndex = 0;
        }

        internal void StartOcrFromDelayed()
        {
            if (_lastAdditions.Count > 0)
            {
                var last = _lastAdditions[_lastAdditions.Count - 1];
                numericUpDownStartNumber.Value = last.Index + 1;
                Timer t = new Timer();
                t.Interval = 200;
                t.Tick += new EventHandler(t_Tick);
                t.Start();
            }
        }

        void t_Tick(object sender, EventArgs e)
        {
            (sender as Timer).Stop();
            ButtonStartOcrClick(null, null);
        }

        private void VobSubOcr_Resize(object sender, EventArgs e)
        {
            int originalTopHeight = 105;

            int adjustPercent = (int)(Height * 0.15);
            groupBoxSubtitleImage.Height = originalTopHeight + adjustPercent;
            groupBoxOcrMethod.Height = groupBoxSubtitleImage.Height;

            splitContainerBottom.Top = groupBoxSubtitleImage.Top + groupBoxSubtitleImage.Height + 5;
            splitContainerBottom.Height = progressBar1.Top - (splitContainerBottom.Top + 20);
            checkBoxUseTimeCodesFromIdx.Left = groupBoxOCRControls.Left + 1;

            listBoxUnknownWords.Top = listBoxLog.Top;
            listBoxUnknownWords.Left = listBoxLog.Left;
        }

        private void importTextWithMatchingTimeCodesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            openFileDialog1.Title = Configuration.Settings.Language.General.OpenSubtitle;
            openFileDialog1.FileName = string.Empty;
            openFileDialog1.Filter = Utilities.GetOpenDialogFilter();
            if (openFileDialog1.ShowDialog(this) == DialogResult.OK)
            {
                string fileName = openFileDialog1.FileName;
                if (!File.Exists(fileName))
                    return;

                var fi = new FileInfo(fileName);
                if (fi.Length > 1024 * 1024 * 10) // max 10 mb
                {
                    if (MessageBox.Show(string.Format(Configuration.Settings.Language.Main.FileXIsLargerThan10Mb + Environment.NewLine +
                                                      Environment.NewLine +
                                                      Configuration.Settings.Language.Main.ContinueAnyway,
                                                      fileName), Text, MessageBoxButtons.YesNoCancel) != DialogResult.Yes)
                        return;
                }

                Subtitle sub = new Subtitle();
                Encoding encoding = null;
                SubtitleFormat format = sub.LoadSubtitle(fileName, out encoding, encoding);
                if (format == null || sub == null)
                    return;

                int index = 0;
                foreach (Paragraph p in sub.Paragraphs)
                {
                    foreach (Paragraph currentP in _subtitle.Paragraphs)
                    {
                        if (string.IsNullOrEmpty(currentP.Text) && Math.Abs(p.StartTime.TotalMilliseconds - currentP.StartTime.TotalMilliseconds) <= 40)
                        {
                            currentP.Text = p.Text;
                            subtitleListView1.SetText(index, p.Text);
                            break;
                        }
                    }
                    index++;
                }

            }
        }

        private void saveAllImagesWithHtmlIndexViewToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialog1.ShowDialog(this) == DialogResult.OK)
            {
                progressBar1.Maximum = _subtitle.Paragraphs.Count - 1;
                progressBar1.Value = 0;
                progressBar1.Visible = true;
                int imagesSavedCount = 0;
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("<html>");
                sb.AppendLine("<head><title>Subtitle images</title></head>");
                sb.AppendLine("<body>");
                for (int i = 0; i < _subtitle.Paragraphs.Count; i++)
                {
                    progressBar1.Value = i;
                    Bitmap bmp = GetSubtitleBitmap(i);
                    string numberString = string.Format("{0:0000}", i + 1);
                    if (bmp != null)
                    {
                        string fileName = Path.Combine(folderBrowserDialog1.SelectedPath, numberString + ".png");
                        bmp.Save(fileName, System.Drawing.Imaging.ImageFormat.Png);
                        imagesSavedCount++;
                        Paragraph p = _subtitle.Paragraphs[i];
                        string text = string.Empty;
                        if (!string.IsNullOrEmpty(p.Text))
                        {
                            string backgroundColor = System.Drawing.ColorTranslator.ToHtml(subtitleListView1.GetBackgroundColor(i));
                            text = "<br /><div style='font-size:22px; background-color:" + backgroundColor + "'>" + Utilities.HtmlEncode(p.Text.Replace("<i>", "@1__").Replace("</i>", "@2__")).Replace("@1__", "<i>").Replace("@2__", "</i>").Replace(Environment.NewLine, "<br />") + "</div>";
                        }
                        sb.AppendLine(string.Format("#{3}:{0}->{1}<div style='text-align:center'><img src='{2}.png' />" + text + "</div><br /><hr />", p.StartTime.ToShortString(), p.EndTime.ToShortString(), numberString, i + 1));
                        bmp.Dispose();
                    }
                }
                sb.AppendLine("</body>");
                sb.AppendLine("</html>");
                string htmlFileName = Path.Combine(folderBrowserDialog1.SelectedPath, "index.html");
                File.WriteAllText(htmlFileName, sb.ToString());
                progressBar1.Visible = false;
                MessageBox.Show(string.Format("{0} images saved in {1}", imagesSavedCount, folderBrowserDialog1.SelectedPath));
                Process.Start(htmlFileName);
            }
        }

        private void inspectImageCompareMatchesForCurrentImageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (subtitleListView1.SelectedItems.Count != 1)
                return;

            if (_compareBitmaps == null)
                LoadImageCompareBitmaps();

            Cursor = Cursors.WaitCursor;
            Bitmap bitmap = GetSubtitleBitmap(subtitleListView1.SelectedItems[0].Index);
            NikseBitmap parentBitmap = new NikseBitmap(bitmap);
            var matches = new List<CompareMatch>();
            List<ImageSplitterItem> list = NikseBitmapImageSplitter.SplitBitmapToLetters(parentBitmap, (int)numericUpDownPixelsIsSpace.Value, checkBoxRightToLeft.Checked, Configuration.Settings.VobSubOcr.TopToBottom);
            int index = 0;
            var imageSources = new List<Bitmap>();
            while (index < list.Count)
            {
                ImageSplitterItem item = list[index];
                if (item.NikseBitmap == null)
                {
                    matches.Add(new CompareMatch(item.SpecialCharacter, false, 0, null));
                    imageSources.Add(null);
                }
                else
                {
                    CompareMatch bestGuess;
                    CompareMatch match = GetCompareMatch(item, parentBitmap, out bestGuess);
                    if (match == null)
                    {
                        matches.Add(new CompareMatch(Configuration.Settings.Language.VobSubOcr.NoMatch, false, 0, null));
                        imageSources.Add(item.NikseBitmap.GetBitmap());
                    }
                    else // found image match
                    {
                        matches.Add(new CompareMatch(match.Text, match.Italic, 0, match.Name));
                        imageSources.Add(item.NikseBitmap.GetBitmap());
                        if (match.ExpandCount > 0)
                            index += match.ExpandCount - 1;
                    }
                }
                index++;
            }
            Cursor = Cursors.Default;
            VobSubOcrCharacterInspect inspect = new VobSubOcrCharacterInspect();
            inspect.Initialize(comboBoxCharacterDatabase.SelectedItem.ToString(), matches, imageSources);
            if (inspect.ShowDialog(this) == DialogResult.OK)
            {
                Cursor = Cursors.WaitCursor;
                _compareDoc = inspect.ImageCompareDocument;
                string path = Configuration.VobSubCompareFolder + comboBoxCharacterDatabase.SelectedItem + Path.DirectorySeparatorChar;
                _compareDoc.Save(path + "Images.xml");
                LoadImageCompareBitmaps();
                Cursor = Cursors.Default;
            }
        }

        private void inspectLastAdditionsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            VobSubEditCharacters formVobSubEditCharacters = new VobSubEditCharacters(comboBoxCharacterDatabase.SelectedItem.ToString(), _lastAdditions);
            if (formVobSubEditCharacters.ShowDialog(this) == DialogResult.OK)
            {
                _lastAdditions = formVobSubEditCharacters.Additions;
                _compareDoc = formVobSubEditCharacters.ImageCompareDocument;
                string path = Configuration.VobSubCompareFolder + comboBoxCharacterDatabase.SelectedItem + Path.DirectorySeparatorChar;
                _compareDoc.Save(path + "Images.xml");
            }
        }

        private void checkBoxAutoTransparentBackground_CheckedChanged(object sender, EventArgs e)
        {
            ResetTesseractThread();
            SubtitleListView1SelectedIndexChanged(null, null);
        }

        internal void Initialize(string fileName, List<Color> palette, VobSubOcrSettings vobSubOcrSettings, List<SpHeader> spList)
        {
            _spList = spList;
            _useNewSubIdxCode = false;
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

            _palette = palette;

            if (_palette == null)
                checkBoxCustomFourColors.Checked = true;

            SetOcrMethod();

            FileName = fileName;
            Text += " - " + Path.GetFileName(FileName);

            foreach (SpHeader header in _spList)
            {
                Paragraph p = new Paragraph(string.Empty, header.StartTime.TotalMilliseconds, header.StartTime.TotalMilliseconds + header.Picture.Delay.TotalMilliseconds);
                _subtitle.Paragraphs.Add(p);
            }
            _subtitle.Renumber(1);
            subtitleListView1.Fill(_subtitle);
            subtitleListView1.SelectIndexAndEnsureVisible(0);
        }

        private void textBoxCurrentText_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyData == _italicShortcut) // Ctrl+i (or cusstom) = italic
            {
                TextBox tb = textBoxCurrentText;
                string text = tb.SelectedText;
                int selectionStart = tb.SelectionStart;
                string tag = "i";
                if (text.Contains("<" + tag + ">"))
                {
                    text = text.Replace("<" + tag + ">", string.Empty);
                    text = text.Replace("</" + tag + ">", string.Empty);
                }
                else
                {
                    text = string.Format("<{0}>{1}</{0}>", tag, text);
                }
                tb.SelectedText = text;
                tb.SelectionStart = selectionStart;
                tb.SelectionLength = text.Length;
                e.SuppressKeyPress = true;
            }
        }

        internal void Initialize(List<SubPicturesWithSeparateTimeCodes> subPicturesWithTimeCodes, VobSubOcrSettings vobSubOcrSettings, string fileName)
        {
            _mp4List = subPicturesWithTimeCodes;

            _useNewSubIdxCode = false;
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

            if (_palette == null)
                checkBoxCustomFourColors.Checked = true;

            SetOcrMethod();

            FileName = fileName;
            Text += " - " + Path.GetFileName(FileName);

            foreach (SubPicturesWithSeparateTimeCodes subItem in _mp4List)
            {
                var p = new Paragraph(string.Empty, subItem.Start.TotalMilliseconds, subItem.End.TotalMilliseconds);
                _subtitle.Paragraphs.Add(p);
            }
            subtitleListView1.Fill(_subtitle);
            subtitleListView1.SelectIndexAndEnsureVisible(0);
        }

        internal void Initialize(List<XSub> subPictures, VobSubOcrSettings vobSubOcrSettings, string fileName)
        {
            _xSubList = subPictures;

            _useNewSubIdxCode = false;
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

            checkBoxCustomFourColors.Enabled = true;
            checkBoxCustomFourColors.Checked = true;
            checkBoxAutoTransparentBackground.Enabled = true;
            checkBoxAutoTransparentBackground.Enabled = false;

            SetOcrMethod();

            FileName = fileName;
            Text += " - " + Path.GetFileName(FileName);

            foreach (XSub subItem in _xSubList)
            {
                var p = new Paragraph(string.Empty, subItem.Start.TotalMilliseconds, subItem.End.TotalMilliseconds);
                _subtitle.Paragraphs.Add(p);
            }
            _subtitle.Renumber(1);
            subtitleListView1.Fill(_subtitle);
            subtitleListView1.SelectIndexAndEnsureVisible(0);
        }


        private void VobSubOcr_FormClosing(object sender, FormClosingEventArgs e)
        {
            _abort = true;
            if (_mainOcrTimer != null)
                _mainOcrTimer.Stop();

            if (_tesseractThread != null)
                _tesseractThread.CancelAsync();
            _tesseractAsyncIndex = 10000;

            System.Threading.Thread.Sleep(100);
            DisposeImageCompareBitmaps();

            Configuration.Settings.VobSubOcr.UseItalicsInTesseract = checkBoxTesseractItalicsOn.Checked;
            Configuration.Settings.VobSubOcr.ItalicFactor = _unItalicFactor;
            Configuration.Settings.VobSubOcr.UseModiInTesseractForUnknownWords = checkBoxUseModiInTesseractForUnknownWords.Checked;
            Configuration.Settings.VobSubOcr.PromptForUnknownWords = checkBoxPromptForUnknownWords.Checked;
            Configuration.Settings.VobSubOcr.GuessUnknownWords = checkBoxGuessUnknownWords.Checked;
            Configuration.Settings.VobSubOcr.AutoBreakSubtitleIfMoreThanTwoLines = checkBoxAutoBreakLines.Checked;
            Configuration.Settings.VobSubOcr.LineOcrDraw = checkBoxNOcrCorrect.Checked;
            Configuration.Settings.VobSubOcr.LineOcrAdvancedItalic = checkBoxNOcrItalic.Checked;

            if (_bluRaySubtitlesOriginal != null)
                Configuration.Settings.VobSubOcr.BlurayAllowDifferenceInPercent = (double)numericUpDownMaxErrorPct.Value;
            else
                Configuration.Settings.VobSubOcr.AllowDifferenceInPercent = (double)numericUpDownMaxErrorPct.Value;

            if (comboBoxOcrMethod.SelectedIndex == 3) // line ocr
            {
                Configuration.Settings.VobSubOcr.LineOcrLastSpellcheck = LanguageString;
                if (comboBoxNOcrLanguage.Items.Count > 0 && comboBoxNOcrLanguage.SelectedIndex >= 0)
                    Configuration.Settings.VobSubOcr.LineOcrLastLanguages = comboBoxNOcrLanguage.Items[comboBoxNOcrLanguage.SelectedIndex].ToString();
            }
        }

        private void subtitleListView1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Modifiers == Keys.Control && e.KeyCode == Keys.A)
            {
                subtitleListView1.SelectedIndexChanged -= SubtitleListView1SelectedIndexChanged;
                subtitleListView1.BeginUpdate();
                for (int i = 0; i < subtitleListView1.Items.Count; i++)
                    subtitleListView1.Items[i].Selected = true;
                subtitleListView1.EndUpdate();
                subtitleListView1.SelectedIndexChanged += SubtitleListView1SelectedIndexChanged;
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
            if (e.Modifiers == Keys.Control && e.KeyCode == Keys.D)
            {
                subtitleListView1.SelectedIndexChanged -= SubtitleListView1SelectedIndexChanged;
                subtitleListView1.BeginUpdate();
                for (int i = 0; i < subtitleListView1.Items.Count; i++)
                    subtitleListView1.Items[i].Selected = false;
                subtitleListView1.EndUpdate();
                subtitleListView1.SelectedIndexChanged += SubtitleListView1SelectedIndexChanged;
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
            else if (e.Modifiers == Keys.None && e.KeyCode == Keys.Delete)
            {
                DeleteToolStripMenuItemClick(sender, e);
                e.Handled = true;
                e.SuppressKeyPress = true;
                subtitleListView1.Focus();
            }
        }

        private void DeleteToolStripMenuItemClick(object sender, EventArgs e)
        {
            if (subtitleListView1.SelectedItems.Count == 0)
                return;

            string askText;
            if (subtitleListView1.SelectedItems.Count > 1)
                askText = string.Format(Configuration.Settings.Language.Main.DeleteXLinesPrompt, subtitleListView1.SelectedItems.Count);
            else
                askText = Configuration.Settings.Language.Main.DeleteOneLinePrompt;
            if (Configuration.Settings.General.PromptDeleteLines && MessageBox.Show(askText, "", MessageBoxButtons.YesNo) == DialogResult.No)
                return;

            ResetTesseractThread();

            int selIdx = subtitleListView1.SelectedItems[0].Index;
            List<int> indices = new List<int>();
            foreach (int idx in subtitleListView1.SelectedIndices)
                indices.Add(idx);
            indices.Reverse();

            if (_mp4List != null)
            {
                foreach (int idx in indices)
                    _mp4List.RemoveAt(idx);
            }
            else if (_spList != null)
            {
                foreach (int idx in indices)
                    _spList.RemoveAt(idx);
            }
            else if (_dvbSubtitles != null)
            {
                foreach (int idx in indices)
                    _dvbSubtitles.RemoveAt(idx);
            }
            else if (_bdnXmlSubtitle != null)
            {
                foreach (int idx in indices)
                    _bdnXmlSubtitle.Paragraphs.RemoveAt(idx);
            }
            else if (_xSubList != null)
            {
                foreach (int idx in indices)
                    _xSubList.RemoveAt(idx);
            }
            else if (_bluRaySubtitlesOriginal != null)
            {
                foreach (int idx in indices)
                {
                    var x1 = _bluRaySubtitles[idx];
                    int i = 0;
                    while (i < _bluRaySubtitlesOriginal.Count)
                    {
                        var x2 = _bluRaySubtitlesOriginal[i];
                        if (x2.StartTime == x1.StartTime)
                        {
                            _bluRaySubtitlesOriginal.Remove(x2);
                            break;
                        }
                        i++;
                    }
                    _bluRaySubtitles.RemoveAt(idx);
                }
            }
            else
            {
                foreach (int idx in indices)
                {
                    var x1 = _vobSubMergedPackist[idx];
                    int i = 0;
                    while (i < _vobSubMergedPackistOriginal.Count)
                    {
                        var x2 = _vobSubMergedPackistOriginal[i];
                        if (x2.StartTime.TotalMilliseconds == x1.StartTime.TotalMilliseconds)
                        {
                            _vobSubMergedPackistOriginal.Remove(x2);
                            break;
                        }
                        i++;
                    }
                    _vobSubMergedPackist.RemoveAt(idx);
                }
            }

            foreach (int idx in indices)
                _subtitle.Paragraphs.RemoveAt(idx);
            subtitleListView1.Fill(_subtitle);

            if (selIdx < subtitleListView1.Items.Count)
                subtitleListView1.SelectIndexAndEnsureVisible(selIdx, true);
            else
                subtitleListView1.SelectIndexAndEnsureVisible(subtitleListView1.Items.Count - 1, true);
        }

        private void buttonUknownToNames_Click(object sender, EventArgs e)
        {
            if (listBoxUnknownWords.Items.Count > 0 && listBoxUnknownWords.SelectedItems.Count > 0)
            {
                string text = listBoxUnknownWords.SelectedItems[0].ToString();
                if (text.Contains(":"))
                {
                    if (_ocrFixEngine == null)
                        comboBoxDictionaries_SelectedIndexChanged(null, null);

                    text = text.Substring(text.IndexOf(":") + 1).Trim();
                    var form = new AddToNamesList();
                    form.Initialize(_subtitle, comboBoxDictionaries.Text, text);
                    if (form.ShowDialog(this) == DialogResult.OK)
                    {
                        comboBoxDictionaries_SelectedIndexChanged(null, null);
                        ShowStatus(string.Format(Configuration.Settings.Language.Main.NameXAddedToNamesEtcList, form.NewName));
                    }
                    else if (!string.IsNullOrEmpty(form.NewName))
                    {
                        MessageBox.Show(string.Format(Configuration.Settings.Language.Main.NameXNotAddedToNamesEtcList, form.NewName));
                    }
                }
            }
        }

        private void buttonUknownToUserDic_Click(object sender, EventArgs e)
        {
            if (listBoxUnknownWords.Items.Count > 0 && listBoxUnknownWords.SelectedItems.Count > 0)
            {
                string text = listBoxUnknownWords.SelectedItems[0].ToString();
                if (text.Contains(":"))
                {
                    text = text.Substring(text.IndexOf(":") + 1).Trim().ToLower();
                    var form = new AddToUserDic();
                    form.Initialize(_subtitle, comboBoxDictionaries.Text, text);
                    if (form.ShowDialog(this) == DialogResult.OK)
                    {
                        comboBoxDictionaries_SelectedIndexChanged(null, null);
                        ShowStatus(string.Format(Configuration.Settings.Language.Main.WordXAddedToUserDic, form.NewWord));
                    }
                    else if (!string.IsNullOrEmpty(form.NewWord))
                    {
                        MessageBox.Show(string.Format(Configuration.Settings.Language.Main.WordXNotAddedToUserDic, form.NewWord));
                    }
                }
            }
        }

        private void buttonAddToOcrReplaceList_Click(object sender, EventArgs e)
        {
            if (listBoxUnknownWords.Items.Count > 0 && listBoxUnknownWords.SelectedItems.Count > 0)
            {
                string text = listBoxUnknownWords.SelectedItems[0].ToString();
                if (text.Contains(":"))
                {
                    text = text.Substring(text.IndexOf(":") + 1).Trim().ToLower();
                    var form = new AddToOcrReplaceList();
                    form.Initialize(_subtitle, _languageId, comboBoxDictionaries.Text, text);
                    if (form.ShowDialog(this) == DialogResult.OK)
                    {
                        comboBoxDictionaries_SelectedIndexChanged(null, null);
                        ShowStatus(string.Format(Configuration.Settings.Language.Main.OcrReplacePairXAdded, form.NewSource, form.NewTarget));
                    }
                    else
                    {
                        MessageBox.Show(string.Format(Configuration.Settings.Language.Main.OcrReplacePairXNotAdded, form.NewSource, form.NewTarget));
                    }
                }
            }
        }

        private void buttonGoogleIt_Click(object sender, EventArgs e)
        {
            if (listBoxUnknownWords.Items.Count > 0 && listBoxUnknownWords.SelectedItems.Count > 0)
            {
                string text = listBoxUnknownWords.SelectedItems[0].ToString();
                if (text.Contains(":"))
                {
                    text = text.Substring(text.IndexOf(":") + 1).Trim();
                    System.Diagnostics.Process.Start("http://www.google.com/search?q=" + Utilities.UrlEncode(text));
                }
            }
        }

        private void listBoxCopyToClipboard_KeyDown(object sender, KeyEventArgs e)
        {
            ListBox lb = sender as ListBox;
            if (lb != null && lb.Items.Count > 0 && lb.SelectedItems.Count > 0)
            {
                string text = lb.SelectedItems[0].ToString();
                Clipboard.SetText(text);
            }
        }

        private void toolStripMenuItemSetUnItalicFactor_Click(object sender, EventArgs e)
        {
            var form = new VobSubOcrSetItalicFactor(GetSubtitleBitmap(_selectedIndex), _unItalicFactor);
            form.ShowDialog(this);
            _unItalicFactor = form.GetUnItalicFactor();
        }

        private void vobSubToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var exportBdnXmlPng = new ExportPngXml();
            exportBdnXmlPng.InitializeFromVobSubOcr(_subtitle, new SubRip(), "VOBSUB", FileName, this, _importLanguageString);
            exportBdnXmlPng.ShowDialog(this);
        }

        private void bluraySupToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var exportBdnXmlPng = new ExportPngXml();
            exportBdnXmlPng.InitializeFromVobSubOcr(_subtitle, new SubRip(), "BLURAYSUP", FileName, this, _importLanguageString);
            exportBdnXmlPng.ShowDialog(this);
        }

        private void bDNXMLToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var exportBdnXmlPng = new ExportPngXml();
            exportBdnXmlPng.InitializeFromVobSubOcr(_subtitle, new SubRip(), "BDNXML", FileName, this, _importLanguageString);
            exportBdnXmlPng.ShowDialog(this);
        }

        private void clearToolStripMenuItem_Click(object sender, EventArgs e)
        {
            listBoxUnknownWords.Items.Clear();
        }

        private void toolStripMenuItemClearFixes_Click(object sender, EventArgs e)
        {
            listBoxLog.Items.Clear();
        }

        private void toolStripMenuItemClearGuesses_Click(object sender, EventArgs e)
        {
            listBoxLogSuggestions.Items.Clear();
        }

        private void buttonGetTesseractDictionaries_Click(object sender, EventArgs e)
        {
            var form = new GetTesseractDictionaries();
            form.ShowDialog(this);
            InitializeTesseract();
        }

        private void toolStripMenuItemInspectNOcrMatches_Click(object sender, EventArgs e)
        {
            if (subtitleListView1.SelectedItems.Count != 1)
                return;

            if (_nocrChars == null)
                LoadNOcrWithCurrentLanguage();

            Cursor = Cursors.WaitCursor;
            Bitmap bitmap = GetSubtitleBitmap(subtitleListView1.SelectedItems[0].Index);
            bool oldPrompt = checkBoxPromptForUnknownWords.Checked;
            string result = OcrViaNOCR(bitmap, subtitleListView1.SelectedItems[0].Index);
            checkBoxPromptForUnknownWords.Checked = oldPrompt;
            Cursor = Cursors.Default;
            var inspect = new VobSubNOcrCharacterInspect();
            bool oldCorrect = checkBoxNOcrCorrect.Checked;
            checkBoxNOcrCorrect.Checked = false;
            inspect.Initialize(bitmap, (int)numericUpDownNumberOfPixelsIsSpaceNOCR.Value, checkBoxRightToLeft.Checked, _nocrChars, this, _unItalicFactor);
            if (inspect.ShowDialog(this) == DialogResult.OK)
            {
                Cursor = Cursors.WaitCursor;
                SaveNOcrWithCurrentLanguage();
                Cursor = Cursors.Default;
            }
            checkBoxNOcrCorrect.Checked = oldCorrect;
        }

        private void buttonLineOcrEditLanguage_Click(object sender, EventArgs e)
        {
            if (_nocrChars == null)
            {
                if (string.IsNullOrEmpty(GetNOcrLanguageFileName()))
                {
                    MessageBox.Show("No line ocr language loaded - please re-install");
                    return;
                }
                LoadNOcrWithCurrentLanguage();
            }

            var form = new VobSubNOcrEdit(_nocrChars, null);
            if (form.ShowDialog(this) == DialogResult.OK)
            {
                SaveNOcrWithCurrentLanguage();
            }
            else
            {
                LoadNOcrWithCurrentLanguage();
            }
        }

        private void buttonLineOcrNewLanguage_Click(object sender, EventArgs e)
        {
            var newFolder = new VobSubOcrNewFolder();
            if (newFolder.ShowDialog(this) == DialogResult.OK)
            {
                string s = newFolder.FolderName;
                if (string.IsNullOrEmpty(s))
                    return;
                s = s.Replace("?", string.Empty).Replace("/", string.Empty).Replace("*", string.Empty).Replace("\\", string.Empty);
                if (string.IsNullOrEmpty(s))
                    return;
                if (File.Exists(Configuration.DictionariesFolder + "nOCR_" + newFolder.FolderName + ".xml"))
                {
                    MessageBox.Show("Line ocr language file already exists!");
                    return;
                }

                _nocrChars = new List<NOcrChar>();
                comboBoxNOcrLanguage.Items.Add(s);
                comboBoxNOcrLanguage.SelectedIndex = comboBoxNOcrLanguage.Items.Count - 1;
            }
        }

        private void comboBoxNOcrLanguage_SelectedIndexChanged(object sender, EventArgs e)
        {
            _nocrChars = null;
            _nocrThreads = null;
        }

        private void buttonSpellCheckDownload_Click(object sender, EventArgs e)
        {
            new GetDictionaries().ShowDialog(this);
            FillSpellCheckDictionaries();
            if (comboBoxDictionaries.Items.Count > 0)
                comboBoxDictionaries.SelectedIndex = 0;
        }

        private void ShowStatus(string msg)
        {
            labelStatus.Text = msg;
            timerHideStatus.Start();
        }

        private void timerHideStatus_Tick(object sender, EventArgs e)
        {
            timerHideStatus.Stop();
            labelStatus.Text = string.Empty;
        }

        private void dOSTToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var exportBdnXmlPng = new ExportPngXml();
            exportBdnXmlPng.InitializeFromVobSubOcr(_subtitle, new SubRip(), "DOST", FileName, this, _importLanguageString);
            exportBdnXmlPng.ShowDialog(this);
        }

        // TODO: Get language from ts file
        internal void Initialize(List<Logic.TransportStream.DvbSubtitle> subtitles, VobSubOcrSettings vobSubOcrSettings, string fileName)
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
            numericUpDownNumberOfPixelsIsSpaceNOCR.Value = vobSubOcrSettings.XOrMorePixelsMakesSpace;
            _vobSubOcrSettings = vobSubOcrSettings;

            InitializeModi();
            InitializeTesseract();
            LoadImageCompareCharacterDatabaseList();

            SetOcrMethod();

            _dvbSubtitles = subtitles;

            ShowDvbSubs();

            FileName = fileName;
            Text += " - " + Path.GetFileName(fileName);

            groupBoxImagePalette.Visible = false;
            groupBoxTransportStream.Left = groupBoxImagePalette.Left;
            groupBoxTransportStream.Top = groupBoxImagePalette.Top;
            groupBoxTransportStream.Visible = true;
            //SetTesseractLanguageFromLanguageString(languageString);
            //_importLanguageString = languageString;
        }

        private void checkBoxTransportStreamGrayscale_CheckedChanged(object sender, EventArgs e)
        {
            SubtitleListView1SelectedIndexChanged(null, null);
        }

        private void checkBoxTransportStreamGetColorAndSplit_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxTransportStreamGetColorAndSplit.Checked)
            {
                SplitDvbForEachSubImage();
            }
            else
            {
                MergeDvbForEachSubImage();
            }
            SubtitleListView1SelectedIndexChanged(null, null);
        }

        private void MergeDvbForEachSubImage()
        {
            for (int i = 0; i < _dvbSubtitles.Count; i++)
            {
                var dvbSub = _dvbSubtitles[i];
                if (dvbSub.ActiveImageIndex.HasValue && dvbSub.ActiveImageIndex > 0)
                    _dvbSubtitles.RemoveAt(i);
                else
                    dvbSub.ActiveImageIndex = null;
            }
            _tesseractAsyncStrings = null;
            ShowDvbSubs();
        }

        private void SplitDvbForEachSubImage()
        {
            var list = new List<Nikse.SubtitleEdit.Logic.TransportStream.DvbSubtitle>();
            foreach (var dvbSub in _dvbSubtitles)
            {
                if (dvbSub.ActiveImageIndex == null)
                {
                    for (int i = 0; i < dvbSub.Pes.ObjectDataList.Count; i++)
                    {
                        var newDbvSub = new Nikse.SubtitleEdit.Logic.TransportStream.DvbSubtitle();
                        newDbvSub.Pes = dvbSub.Pes;
                        newDbvSub.ActiveImageIndex = i;
                        newDbvSub.StartMilliseconds = dvbSub.StartMilliseconds;
                        newDbvSub.EndMilliseconds = dvbSub.EndMilliseconds;
                        list.Add(newDbvSub);
                    }
                }
                else
                {
                    list.Add(dvbSub);
                }
            }
            _dvbSubtitles = list;
            _tesseractAsyncStrings = null;
            ShowDvbSubs();
        }

        private void ShowDvbSubs()
        {
            _subtitle.Paragraphs.Clear();
            foreach (var sub in _dvbSubtitles)
            {
                _subtitle.Paragraphs.Add(new Paragraph(string.Empty, sub.StartMilliseconds, sub.EndMilliseconds));
            }
            _subtitle.Renumber(1);
            subtitleListView1.Fill(_subtitle);
            subtitleListView1.SelectIndexAndEnsureVisible(0);
        }

    }
}
