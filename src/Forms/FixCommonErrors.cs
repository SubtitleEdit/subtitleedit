using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Ocr;
using Nikse.SubtitleEdit.Logic.SubtitleFormats;
using Nikse.SubtitleEdit.Logic.Forms;

namespace Nikse.SubtitleEdit.Forms
{
    public sealed partial class FixCommonErrors : Form
    {
        private const int IndexRemoveEmptyLines = 0;
        private const int IndexOverlappingDisplayTime = 1;
        private const int IndexTooShortDisplayTime = 2;
        private const int IndexTooLongDisplayTime = 3;
        private const int IndexInvalidItalicTags = 4;
        private const int IndexUnneededSpaces = 5;
        private const int IndexUnneededPeriods = 6;
        private const int IndexMissingSpaces = 7;
        private const int IndexBreakLongLines = 8;
        private const int IndexMergeShortLines = 9;
        private const int IndexMergeShortLinesAll = 10;
        private const int IndexDoubleApostropheToQuote = 11;
        private const int IndexFixMusicNotation = 12;
        private const int IndexAddPeriodAfterParagraph = 13;
        private const int IndexStartWithUppercaseLetterAfterParagraph = 14;
        private const int IndexStartWithUppercaseLetterAfterPeriodInsideParagraph = 15;
        private const int IndexStartWithUppercaseLetterAfterColon = 16;
        private const int IndexAddMissingQuotes = 17;
        private const int IndexFixHyphens = 18;
        private const int IndexFixHyphensAdd = 19;
        private const int IndexFix3PlusLines = 20;
        private const int IndexFixDoubleDash = 21;
        private const int IndexFixDoubleGreaterThan = 22;
        private const int IndexFixEllipsesStart = 23;
        private const int IndexFixMissingOpenBracket = 24;
        private const int IndexFixOcrErrorsViaReplaceList = 25;
        private const int IndexUppercaseIInsideLowercaseWord = 26;
        private const int IndexAloneLowercaseIToUppercaseIEnglish = 27;
        private const int IndexRemoveSpaceBetweenNumbers = 28;
        private const int IndexDialogsOnOneLine = 29;
        private const int IndexDanishLetterI = 30;
        private const int IndexFixSpanishInvertedQuestionAndExclamationMarks = 31;
        private int _turkishAnsiIndex = -1;
        private int _danishLetterIIndex = -1;
        private int _spanishInvertedQuestionAndExclamationMarksIndex = -1;

        private readonly LanguageStructure.FixCommonErrors _language;
        private readonly LanguageStructure.General _languageGeneral;
        private bool _hasFixesBeenMade;

        private static readonly Regex FixMissingSpacesReComma = new Regex(@"[^\s\d],[^\s]", RegexOptions.Compiled);
        private static readonly Regex FixMissingSpacesRePeriod = new Regex(@"[a-z][a-z][.][a-zA-Z]", RegexOptions.Compiled);
        private static readonly Regex FixMissingSpacesReQuestionMark = new Regex(@"[^\s\d]\?[a-zA-Z]", RegexOptions.Compiled);
        private static readonly Regex FixMissingSpacesReExclamation = new Regex(@"[^\s\d]\![a-zA-Z]", RegexOptions.Compiled);
        private static readonly Regex FixMissingSpacesReColon = new Regex(@"[^\s\d]\:[a-zA-Z]", RegexOptions.Compiled);
        private static readonly Regex UrlCom = new Regex(@"\w\.com\b", RegexOptions.Compiled);
        private static readonly Regex UrlNet = new Regex(@"\w\.net\b", RegexOptions.Compiled);
        private static readonly Regex UrlOrg = new Regex(@"\w\.org\b", RegexOptions.Compiled);

        private static readonly Regex ReAfterLowercaseLetter = new Regex(@"[a-zæøåäöéùáàìéóúñüéíóúñü]I", RegexOptions.Compiled);
        private static readonly Regex ReBeforeLowercaseLetter = new Regex(@"I[a-zæøåäöéùàìéóúñüéíóúñü]", RegexOptions.Compiled);

        private static readonly Regex RemoveSpaceBetweenNumbersRegEx = new Regex(@"\d \d", RegexOptions.Compiled);

        public static readonly Regex FixAloneLowercaseIToUppercaseIRE = new Regex(@"\bi\b", RegexOptions.Compiled);

        private Keys _goToLine = Utilities.GetKeys(Configuration.Settings.Shortcuts.MainEditGoToLineNumber);
        private Keys _preview = Utilities.GetKeys(Configuration.Settings.Shortcuts.MainToolsFixCommonErrorsPreview);
        private Keys _mainGeneralGoToNextSubtitle = Utilities.GetKeys(Configuration.Settings.Shortcuts.GeneralGoToNextSubtitle);
        private Keys _mainGeneralGoToPrevSubtitle = Utilities.GetKeys(Configuration.Settings.Shortcuts.GeneralGoToPrevSubtitle);
        private Keys _mainListViewGoToNextError = Utilities.GetKeys(Configuration.Settings.Shortcuts.MainListViewGoToNextError);

        private class FixItem
        {
            public string Name { get; set; }
            public string Example { get; set; }
            public EventHandler Action { get; set; }
            public bool DefaultChecked { get; set; }

            public FixItem(string name, string example, EventHandler action, bool selected)
            {
                Name = name;
                Example = example;
                Action = action;
                DefaultChecked = selected;
            }
        }

        private class ListViewSorter : System.Collections.IComparer
        {
            public int Compare(object o1, object o2)
            {
                var lvi1 = o1 as ListViewItem;
                var lvi2 = o2 as ListViewItem;
                if (lvi1 == null || lvi2 == null)
                    return 0;

                if (Descending)
                {
                    ListViewItem temp = lvi1;
                    lvi1 = lvi2;
                    lvi2 = temp;
                }

                if (IsNumber)
                {
                    int i1 = int.Parse(lvi1.SubItems[ColumnNumber].Text);
                    int i2 = int.Parse(lvi2.SubItems[ColumnNumber].Text);

                    if (i1 > i2)
                        return 1;
                    if (i1 == i2)
                        return 0;
                    return -1;
                }
                return string.Compare(lvi2.SubItems[ColumnNumber].Text, lvi1.SubItems[ColumnNumber].Text, StringComparison.Ordinal);
            }
            public int ColumnNumber { get; set; }
            public bool IsNumber { get; set; }
            public bool Descending { get; set; }
        }

        public Subtitle _subtitle;
        private SubtitleFormat _format;
        private Encoding _encoding = Encoding.UTF8;
        private Subtitle _originalSubtitle;
        private int _totalFixes;
        private int _totalErrors;
        private List<FixItem> _fixActions;
        private int _subtitleListViewIndex = -1;
        private bool _onlyListFixes = true;
        private bool _batchMode = false;
        private string _autoDetectGoogleLanguage;
        private List<string> _namesEtcList;
        private List<string> _abbreviationList;
        private StringBuilder _newLog = new StringBuilder();
        private StringBuilder _appliedLog = new StringBuilder();
        private int _numberOfImportantLogMessages = 0;
        private List<int> _deleteIndices = new List<int>();

        public Subtitle FixedSubtitle
        {
            get { return _originalSubtitle; }
        }

        public void RunBatchSettings(Subtitle subtitle, SubtitleFormat format, Encoding encoding, string language)
        {
            _autoDetectGoogleLanguage = language;
            var ci = CultureInfo.GetCultureInfo(_autoDetectGoogleLanguage);
            string threeLetterISOLanguageName = ci.ThreeLetterISOLanguageName;

            comboBoxLanguage.Items.Clear();
            foreach (CultureInfo x in CultureInfo.GetCultures(CultureTypes.NeutralCultures))
                comboBoxLanguage.Items.Add(x);
            comboBoxLanguage.Sorted = true;
            int languageIndex = 0;
            int j = 0;
            foreach (var x in comboBoxLanguage.Items)
            {
                var xci = (CultureInfo)x;
                if (xci.TwoLetterISOLanguageName == ci.TwoLetterISOLanguageName)
                {
                    languageIndex = j;
                    break;
                }
                else if (xci.TwoLetterISOLanguageName == "en")
                {
                    languageIndex = j;
                }
                j++;
            }
            comboBoxLanguage.SelectedIndex = languageIndex;
            AddFixActions(threeLetterISOLanguageName);
            _originalSubtitle = new Subtitle(subtitle); // copy constructor
            _subtitle = new Subtitle(subtitle); // copy constructor
            _format = format;
            _encoding = encoding;
            _onlyListFixes = false;
            InitUI();
            groupBoxStep1.Text = string.Empty;
            buttonBack.Visible = false;
            buttonNextFinish.Visible = false;
            buttonCancel.Text = Configuration.Settings.Language.General.Ok;
        }

        public string Language
        {
            get
            {
                var ci = (CultureInfo)comboBoxLanguage.SelectedItem;
                if (ci == null)
                    return "en";
                return ci.TwoLetterISOLanguageName;
            }
        }

        public void RunBatch(Subtitle subtitle, SubtitleFormat format, Encoding encoding, string language)
        {
            _autoDetectGoogleLanguage = language;
            var ci = CultureInfo.GetCultureInfo(_autoDetectGoogleLanguage);
            string threeLetterISOLanguageName = ci.ThreeLetterISOLanguageName;

            comboBoxLanguage.Items.Clear();
            foreach (CultureInfo x in CultureInfo.GetCultures(CultureTypes.NeutralCultures))
                comboBoxLanguage.Items.Add(x);
            comboBoxLanguage.Sorted = true;
            int languageIndex = 0;
            int j = 0;
            foreach (var x in comboBoxLanguage.Items)
            {
                var xci = (CultureInfo)x;
                if (xci.TwoLetterISOLanguageName == ci.TwoLetterISOLanguageName)
                {
                    languageIndex = j;
                    break;
                }
                else if (xci.TwoLetterISOLanguageName == "en")
                {
                    languageIndex = j;
                }
                j++;
            }
            comboBoxLanguage.SelectedIndex = languageIndex;

            AddFixActions(threeLetterISOLanguageName);

            _originalSubtitle = new Subtitle(subtitle); // copy constructor
            _subtitle = new Subtitle(subtitle); // copy constructor
            _format = format;
            _encoding = encoding;
            _onlyListFixes = true;
            _hasFixesBeenMade = true;
            _numberOfImportantLogMessages = 0;
            _onlyListFixes = false;
            _totalFixes = 0;
            _totalErrors = 0;
            _batchMode = true;
            RunSelectedActions();
            _originalSubtitle = _subtitle;
        }

        public void Initialize(Subtitle subtitle, SubtitleFormat format, Encoding encoding)
        {
            _autoDetectGoogleLanguage = Utilities.AutoDetectGoogleLanguage(encoding); // Guess language via encoding
            if (string.IsNullOrEmpty(_autoDetectGoogleLanguage))
                _autoDetectGoogleLanguage = Utilities.AutoDetectGoogleLanguage(subtitle); // Guess language based on subtitle contents
            if (_autoDetectGoogleLanguage.Equals("zh", StringComparison.OrdinalIgnoreCase))
                _autoDetectGoogleLanguage = "zh-CHS"; // Note that "zh-CHS" (Simplified Chinese) and "zh-CHT" (Traditional Chinese) are neutral cultures
            CultureInfo ci = CultureInfo.GetCultureInfo(_autoDetectGoogleLanguage);
            string threeLetterISOLanguageName = ci.ThreeLetterISOLanguageName;

            comboBoxLanguage.Items.Clear();
            foreach (CultureInfo x in CultureInfo.GetCultures(CultureTypes.NeutralCultures))
                comboBoxLanguage.Items.Add(x);
            comboBoxLanguage.Sorted = true;
            int languageIndex = 0;
            int j = 0;
            foreach (var x in comboBoxLanguage.Items)
            {
                var xci = (CultureInfo)x;
                if (xci.TwoLetterISOLanguageName == ci.TwoLetterISOLanguageName)
                {
                    languageIndex = j;
                    break;
                }
                else if (xci.TwoLetterISOLanguageName == "en")
                {
                    languageIndex = j;
                }
                j++;
            }
            comboBoxLanguage.SelectedIndex = languageIndex;

            AddFixActions(threeLetterISOLanguageName);

            _originalSubtitle = new Subtitle(subtitle); // copy constructor
            _subtitle = new Subtitle(subtitle); // copy constructor
            _format = format;
            _encoding = encoding;
            InitUI();
        }

        private void InitUI()
        {
            labelStatus.Text = string.Empty;
            labelTextLineLengths.Text = string.Empty;
            labelTextLineTotal.Text = string.Empty;
            groupBoxStep1.BringToFront();
            groupBox2.Visible = false;
            groupBoxStep1.Visible = true;
            listView1.Columns[0].Width = 50;
            listView1.Columns[1].Width = 310;
            listView1.Columns[2].Width = 400;

            Utilities.InitializeSubtitleFont(textBoxListViewText);
            Utilities.InitializeSubtitleFont(subtitleListView1);
            listViewFixes.ListViewItemSorter = new ListViewSorter { ColumnNumber = 1, IsNumber = true };

            if (!string.IsNullOrEmpty(Configuration.Settings.CommonErrors.StartSize))
            {
                StartPosition = FormStartPosition.Manual;
                string[] arr = Configuration.Settings.CommonErrors.StartSize.Split(';');
                int x, y;
                if (arr.Length == 2 && int.TryParse(arr[0], out x) && int.TryParse(arr[1], out y))
                {
                    if (x > 10 && x < 10000 && y > 10 && y < 10000)
                    {
                        Width = x;
                        Height = y;
                    }
                }
            }
            if (!string.IsNullOrEmpty(Configuration.Settings.CommonErrors.StartPosition))
            {
                StartPosition = FormStartPosition.Manual;
                string[] arr = Configuration.Settings.CommonErrors.StartPosition.Split(';');
                int x, y;
                if (arr.Length == 2 && int.TryParse(arr[0], out x) && int.TryParse(arr[1], out y))
                {
                    if (x > 0 && x < Screen.PrimaryScreen.WorkingArea.Width && y > 0 && y < Screen.PrimaryScreen.WorkingArea.Height)
                    {
                        Left = x;
                        Top = y;
                    }
                }
            }

            if (Screen.PrimaryScreen.WorkingArea.Width <= 124)
            {
                Width = MinimumSize.Width;
                Height = MinimumSize.Height;
            }
            Activate();
        }

        private void AddFixActions(string threeLetterISOLanguageName)
        {
            _turkishAnsiIndex = -1;
            _danishLetterIIndex = -1;
            _spanishInvertedQuestionAndExclamationMarksIndex = -1;

            FixCommonErrorsSettings ce = Configuration.Settings.CommonErrors;
            _fixActions = new List<FixItem>();
            _fixActions.Add(new FixItem(_language.RemovedEmptyLinesUnsedLineBreaks, string.Empty, delegate { FixEmptyLines(); }, ce.EmptyLinesTicked));
            _fixActions.Add(new FixItem(_language.FixOverlappingDisplayTimes, string.Empty, delegate { FixOverlappingDisplayTimes(); }, ce.OverlappingDisplayTimeTicked));
            _fixActions.Add(new FixItem(_language.FixShortDisplayTimes, string.Empty, delegate { FixShortDisplayTimes(); }, ce.TooShortDisplayTimeTicked));
            _fixActions.Add(new FixItem(_language.FixLongDisplayTimes, string.Empty, delegate { FixLongDisplayTimes(); }, ce.TooLongDisplayTimeTicked));
            _fixActions.Add(new FixItem(_language.FixInvalidItalicTags, _language.FixInvalidItalicTagsExample, delegate { FixInvalidItalicTags(); }, ce.InvalidItalicTagsTicked));
            _fixActions.Add(new FixItem(_language.RemoveUnneededSpaces, _language.RemoveUnneededSpacesExample, delegate { FixUnneededSpaces(); }, ce.UnneededSpacesTicked));
            _fixActions.Add(new FixItem(_language.RemoveUnneededPeriods, _language.RemoveUnneededPeriodsExample, delegate { FixUnneededPeriods(); }, ce.UnneededPeriodsTicked));
            _fixActions.Add(new FixItem(_language.FixMissingSpaces, _language.FixMissingSpacesExample, delegate { FixMissingSpaces(); }, ce.MissingSpacesTicked));
            _fixActions.Add(new FixItem(_language.BreakLongLines, string.Empty, delegate { FixLongLines(); }, ce.BreakLongLinesTicked));
            _fixActions.Add(new FixItem(_language.RemoveLineBreaks, string.Empty, delegate { FixShortLines(); }, ce.MergeShortLinesTicked));
            _fixActions.Add(new FixItem(_language.RemoveLineBreaksAll, string.Empty, delegate { FixShortLinesAll(); }, ce.MergeShortLinesAllTicked));
            _fixActions.Add(new FixItem(_language.FixDoubleApostrophes, string.Empty, delegate { FixDoubleApostrophes(); }, ce.DoubleApostropheToQuoteTicked));
            _fixActions.Add(new FixItem(_language.FixMusicNotation, _language.FixMusicNotationExample, delegate { FixMusicNotation(); }, ce.FixMusicNotationTicked));
            _fixActions.Add(new FixItem(_language.AddPeriods, string.Empty, delegate { FixMissingPeriodsAtEndOfLine(); }, ce.AddPeriodAfterParagraphTicked));
            _fixActions.Add(new FixItem(_language.StartWithUppercaseLetterAfterParagraph, string.Empty, delegate { FixStartWithUppercaseLetterAfterParagraph(); }, ce.StartWithUppercaseLetterAfterParagraphTicked));
            _fixActions.Add(new FixItem(_language.StartWithUppercaseLetterAfterPeriodInsideParagraph, string.Empty, delegate { FixStartWithUppercaseLetterAfterPeriodInsideParagraph(); }, ce.StartWithUppercaseLetterAfterPeriodInsideParagraphTicked));
            _fixActions.Add(new FixItem(_language.StartWithUppercaseLetterAfterColon, string.Empty, delegate { FixStartWithUppercaseLetterAfterColon(); }, ce.StartWithUppercaseLetterAfterColonTicked));
            _fixActions.Add(new FixItem(_language.AddMissingQuotes, _language.AddMissingQuotesExample, delegate { AddMissingQuotes(); }, ce.AddMissingQuotesTicked));
            _fixActions.Add(new FixItem(_language.FixHyphens, string.Empty, delegate { FixHyphens(); }, ce.FixHyphensTicked));
            _fixActions.Add(new FixItem(_language.FixHyphensAdd, string.Empty, delegate { FixHyphensAdd(); }, ce.FixHyphensAddTicked));
            _fixActions.Add(new FixItem(_language.Fix3PlusLines, string.Empty, delegate { Fix3PlusLines(); }, ce.Fix3PlusLinesTicked));
            _fixActions.Add(new FixItem(_language.FixDoubleDash, _language.FixDoubleDashExample, delegate { FixDoubleDash(); }, ce.FixDoubleDashTicked));
            _fixActions.Add(new FixItem(_language.FixDoubleGreaterThan, _language.FixDoubleGreaterThanExample, delegate { FixDoubleGreaterThan(); }, ce.FixDoubleGreaterThanTicked));
            _fixActions.Add(new FixItem(_language.FixEllipsesStart, _language.FixEllipsesStartExample, delegate { FixEllipsesStart(); }, ce.FixEllipsesStartTicked));
            _fixActions.Add(new FixItem(_language.FixMissingOpenBracket, _language.FixMissingOpenBracketExample, delegate { FixMissingOpenBracket(); }, ce.FixMissingOpenBracketTicked));

            if (string.IsNullOrEmpty(_language.FixOcrErrorExample))
                _fixActions.Add(new FixItem(_language.FixCommonOcrErrors, "D0n't -> Don't", delegate { FixOcrErrorsViaReplaceList(threeLetterISOLanguageName); }, ce.FixOcrErrorsViaReplaceListTicked));
            else
                _fixActions.Add(new FixItem(_language.FixCommonOcrErrors, _language.FixOcrErrorExample, delegate { FixOcrErrorsViaReplaceList(threeLetterISOLanguageName); }, ce.FixOcrErrorsViaReplaceListTicked));

            _fixActions.Add(new FixItem(_language.FixUppercaseIInsindeLowercaseWords, _language.FixUppercaseIInsindeLowercaseWordsExample, delegate { FixUppercaseIInsideWords(); }, ce.UppercaseIInsideLowercaseWordTicked));
            _fixActions.Add(new FixItem(_language.FixLowercaseIToUppercaseI, _language.FixLowercaseIToUppercaseIExample, delegate { FixAloneLowercaseIToUppercaseI(); }, ce.AloneLowercaseIToUppercaseIEnglishTicked));

            if (string.IsNullOrEmpty(_language.FixSpaceBetweenNumbersExample))
                _fixActions.Add(new FixItem(_language.RemoveSpaceBetweenNumber, "1 100 -> 1100", delegate { RemoveSpaceBetweenNumbers(); }, ce.RemoveSpaceBetweenNumberTicked));
            else
                _fixActions.Add(new FixItem(_language.RemoveSpaceBetweenNumber, _language.FixSpaceBetweenNumbersExample, delegate { RemoveSpaceBetweenNumbers(); }, ce.RemoveSpaceBetweenNumberTicked));

            if (string.IsNullOrEmpty(_language.FixSpaceBetweenNumbersExample))
                _fixActions.Add(new FixItem(_language.FixDialogsOnOneLine, "Hi John! - Hi Ida! -> Hi John!" + Configuration.Settings.General.ListViewLineSeparatorString + "- Hi Ida!", delegate { DialogsOnOneLine(); }, ce.FixDialogsOnOneLineTicked));
            else
                _fixActions.Add(new FixItem(_language.FixDialogsOnOneLine, _language.FixDialogsOneLineExample, delegate { DialogsOnOneLine(); }, ce.FixDialogsOnOneLineTicked));

            if (Language == "tr")
            {
                _turkishAnsiIndex = _fixActions.Count;
                _fixActions.Add(new FixItem(_language.FixTurkishAnsi, "Ý > İ, Ð > Ğ, Þ > Ş, ý > ı, ð > ğ, þ > ş", delegate { TurkishAnsiToUnicode(); }, ce.TurkishAnsiTicked));
            }

            if (Language == "da")
            {
                _danishLetterIIndex = _fixActions.Count;
                _fixActions.Add(new FixItem(_language.FixDanishLetterI, "Jeg synes i er søde. -> Jeg synes I er søde.", delegate { FixDanishLetterI(); }, ce.DanishLetterITicked));
            }

            if (Language == "es")
            {
                _spanishInvertedQuestionAndExclamationMarksIndex = _fixActions.Count;
                _fixActions.Add(new FixItem(_language.FixSpanishInvertedQuestionAndExclamationMarks, "Hablas bien castellano? -> ¿Hablas bien castellano?", delegate { FixSpanishInvertedQuestionAndExclamationMarks(); }, ce.SpanishInvertedQuestionAndExclamationMarksTicked));
            }

            listView1.Items.Clear();
            foreach (FixItem fi in _fixActions)
                AddFixActionItemToListView(fi);
        }

        public FixCommonErrors()
        {
            InitializeComponent();

            labelStartTimeWarning.Text = string.Empty;
            labelDurationWarning.Text = string.Empty;
            labelNumberOfImportantLogMessages.Text = string.Empty;

            _language = Configuration.Settings.Language.FixCommonErrors;
            _languageGeneral = Configuration.Settings.Language.General;
            Text = _language.Title;
            groupBoxStep1.Text = _language.Step1;
            groupBox2.Text = _language.Step2;
            listView1.Columns[0].Text = Configuration.Settings.Language.General.Apply;
            listView1.Columns[1].Text = _language.WhatToFix;
            listView1.Columns[2].Text = _language.Example;
            buttonSelectAll.Text = _language.SelectAll;
            buttonInverseSelection.Text = _language.InverseSelection;
            tabControl1.TabPages[0].Text = _language.Fixes;
            tabControl1.TabPages[1].Text = _language.Log;
            listViewFixes.Columns[0].Text = Configuration.Settings.Language.General.Apply;
            listViewFixes.Columns[1].Text = Configuration.Settings.Language.General.LineNumber;
            listViewFixes.Columns[2].Text = _language.Function;
            listViewFixes.Columns[3].Text = Configuration.Settings.Language.General.Before;
            listViewFixes.Columns[4].Text = Configuration.Settings.Language.General.After;
            buttonNextFinish.Text = _language.Next;
            buttonBack.Text = _language.Back;
            buttonCancel.Text = _languageGeneral.Cancel;
            buttonFixesSelectAll.Text = _language.SelectAll;
            buttonFixesInverse.Text = _language.InverseSelection;
            buttonRefreshFixes.Text = _language.RefreshFixes;
            buttonFixesApply.Text = _language.ApplyFixes;
            labelStartTime.Text = _languageGeneral.StartTime;
            labelDuration.Text = _languageGeneral.Duration;
            buttonAutoBreak.Text = _language.AutoBreak;
            buttonUnBreak.Text = _language.Unbreak;
            subtitleListView1.InitializeLanguage(_languageGeneral, Configuration.Settings);
            labelLanguage.Text = Configuration.Settings.Language.ChooseLanguage.Language;
            toolStripMenuItemDelete.Text = Configuration.Settings.Language.Main.Menu.ContextMenu.Delete;
            mergeSelectedLinesToolStripMenuItem.Text = Configuration.Settings.Language.Main.Menu.ContextMenu.MergeSelectedLines;

            splitContainerStep2.Panel1MinSize = 110;
            splitContainerStep2.Panel2MinSize = 160;

            numericUpDownDuration.Left = timeUpDownStartTime.Left + timeUpDownStartTime.Width;
            labelDuration.Left = timeUpDownStartTime.Left + timeUpDownStartTime.Width - 3;

            FixLargeFonts();
        }

        private void FixLargeFonts()
        {
            Graphics graphics = this.CreateGraphics();
            SizeF textSize = graphics.MeasureString(buttonCancel.Text, this.Font);
            if (textSize.Height > buttonCancel.Height - 4)
            {
                subtitleListView1.InitializeTimestampColumnWidths(this);
                int newButtonHeight = (int)(textSize.Height + 7 + 0.5);
                Utilities.SetButtonHeight(this, newButtonHeight, 1);
            }
        }

        private void AddFixActionItemToListView(FixItem fi)
        {
            var item = new ListViewItem(string.Empty);
            item.Tag = fi;
            item.Checked = fi.DefaultChecked;

            var subItem = new ListViewItem.ListViewSubItem(item, fi.Name);
            item.SubItems.Add(subItem);
            subItem = new ListViewItem.ListViewSubItem(item, fi.Example);
            item.SubItems.Add(subItem);

            listView1.Items.Add(item);
        }

        private void AddFixToListView(Paragraph p, string action, string before, string after)
        {
            if (_onlyListFixes)
            {
                var item = new ListViewItem(string.Empty) { Checked = true };

                var subItem = new ListViewItem.ListViewSubItem(item, p.Number.ToString());
                item.SubItems.Add(subItem);
                subItem = new ListViewItem.ListViewSubItem(item, action);
                item.SubItems.Add(subItem);
                subItem = new ListViewItem.ListViewSubItem(item, before.Replace(Environment.NewLine, Configuration.Settings.General.ListViewLineSeparatorString));
                item.SubItems.Add(subItem);
                subItem = new ListViewItem.ListViewSubItem(item, after.Replace(Environment.NewLine, Configuration.Settings.General.ListViewLineSeparatorString));
                item.SubItems.Add(subItem);

                item.Tag = p; // save paragraph in Tag

                listViewFixes.Items.Add(item);
            }
        }

        public bool AllowFix(Paragraph p, string action)
        {
            //if (!buttonBack.Enabled)
            if (_onlyListFixes || _batchMode)
                return true;

            string ln = p.Number.ToString();
            foreach (ListViewItem item in listViewFixes.Items)
            {
                if (item.SubItems[1].Text == ln && item.SubItems[2].Text == action)
                    return item.Checked;
            }
            return false;
        }

        public void ShowStatus(string message)
        {
            message = message.Replace(Environment.NewLine, "  ");
            if (message.Length > 83)
                message = message.Substring(0, 80) + "...";
            labelStatus.Text = message;
            labelStatus.Refresh();
        }

        public void LogStatus(string sender, string message, bool isImportant)
        {
            if (isImportant)
                _numberOfImportantLogMessages++;
            LogStatus(sender, message);
        }

        public void LogStatus(string sender, string message)
        {
            if (!string.IsNullOrEmpty(message))
            {
                message += Environment.NewLine;
                if (_onlyListFixes)
                    _newLog.AppendLine(" +  " + sender + ": " + message);
                else
                    _appliedLog.AppendLine(string.Format(_language.FixedOkXY, sender, message));
            }
        }

        private void FixEmptyLines()
        {
            string fixAction0 = _language.RemovedEmptyLine;
            string fixAction1 = _language.RemovedEmptyLineAtTop;
            string fixAction2 = _language.RemovedEmptyLineAtBottom;

            if (_subtitle.Paragraphs.Count == 0)
                return;

            int emptyLinesRemoved = 0;

            int firstNumber = _subtitle.Paragraphs[0].Number;
            listViewFixes.BeginUpdate();
            for (int i = _subtitle.Paragraphs.Count - 1; i >= 0; i--)
            {
                Paragraph p = _subtitle.Paragraphs[i];
                if (!string.IsNullOrEmpty(p.Text))
                {
                    string text = p.Text.Trim(' ');
                    if (text.StartsWith(Environment.NewLine))
                    {
                        if (AllowFix(p, fixAction1))
                        {
                            p.Text = text.TrimStart(Utilities.NewLineChars);
                            emptyLinesRemoved++;
                            AddFixToListView(p, fixAction1, text, p.Text);
                        }
                    }
                    if (text.EndsWith(Environment.NewLine, StringComparison.Ordinal))
                    {
                        if (AllowFix(p, fixAction2))
                        {
                            p.Text = text.TrimEnd(Utilities.NewLineChars);
                            emptyLinesRemoved++;
                            AddFixToListView(p, fixAction2, text, p.Text);
                        }
                    }
                }
            }

            // this must be the very last action done, or line numbers will be messed up!!!
            for (int i = _subtitle.Paragraphs.Count - 1; i >= 0; i--)
            {
                Paragraph p = _subtitle.Paragraphs[i];
                if (string.IsNullOrEmpty(p.Text))
                {
                    if (AllowFix(p, fixAction0))
                    {
                        _subtitle.Paragraphs.RemoveAt(i);
                        emptyLinesRemoved++;
                        AddFixToListView(p, fixAction0, p.Text, string.Format("[{0}]", _language.RemovedEmptyLine));
                        _deleteIndices.Add(i);
                    }
                }
            }

            listViewFixes.EndUpdate();
            if (emptyLinesRemoved > 0)
            {
                LogStatus(_language.RemovedEmptyLinesUnsedLineBreaks, string.Format(_language.EmptyLinesRemovedX, emptyLinesRemoved));
                _totalFixes += emptyLinesRemoved;
                _subtitle.Renumber(firstNumber);
            }
        }

        public void FixOverlappingDisplayTimes()
        {
            // negative display time
            string fixAction = _language.FixOverlappingDisplayTime;
            int noOfOverlappingDisplayTimesFixed = 0;
            for (int i = 0; i < _subtitle.Paragraphs.Count; i++)
            {
                Paragraph p = _subtitle.Paragraphs[i];
                Paragraph oldP = new Paragraph(p);
                if (p.Duration.TotalMilliseconds < 0) // negative display time...
                {
                    bool isFixed = false;
                    string status = string.Format(_language.StartTimeLaterThanEndTime,
                                                    i + 1, p.StartTime, p.EndTime, p.Text, Environment.NewLine);

                    Paragraph prev = _subtitle.GetParagraphOrDefault(i - 1);
                    Paragraph next = _subtitle.GetParagraphOrDefault(i + 1);

                    double wantedDisplayTime = Utilities.GetOptimalDisplayMilliseconds(p.Text) * 0.9;

                    if (next == null || next.StartTime.TotalMilliseconds > p.StartTime.TotalMilliseconds + wantedDisplayTime)
                    {
                        if (AllowFix(p, fixAction))
                        {
                            p.EndTime.TotalMilliseconds = p.StartTime.TotalMilliseconds + wantedDisplayTime;
                            isFixed = true;
                        }
                    }
                    else if (next.StartTime.TotalMilliseconds > p.StartTime.TotalMilliseconds + 500.0)
                    {
                        if (AllowFix(p, fixAction))
                        {
                            p.EndTime.TotalMilliseconds = p.StartTime.TotalMilliseconds + 500.0;
                            isFixed = true;
                        }
                    }
                    else if (prev == null || next.StartTime.TotalMilliseconds - wantedDisplayTime > prev.EndTime.TotalMilliseconds)
                    {
                        if (AllowFix(p, fixAction))
                        {
                            p.StartTime.TotalMilliseconds = next.StartTime.TotalMilliseconds - wantedDisplayTime;
                            p.EndTime.TotalMilliseconds = next.StartTime.TotalMilliseconds - 1;
                            isFixed = true;
                        }
                    }
                    else
                    {
                        LogStatus(_language.FixOverlappingDisplayTimes, string.Format(_language.UnableToFixStartTimeLaterThanEndTime,
                                                    i + 1, p), true);
                        _totalErrors++;
                    }

                    if (isFixed)
                    {
                        _totalFixes++;
                        noOfOverlappingDisplayTimesFixed++;
                        status = string.Format(_language.XFixedToYZ, status, Environment.NewLine, p);
                        LogStatus(_language.FixOverlappingDisplayTimes, status);
                        AddFixToListView(p, fixAction, oldP.ToString(), p.ToString());
                    }
                }
            }

            // overlapping display time
            for (int i = 1; i < _subtitle.Paragraphs.Count; i++)
            {
                Paragraph p = _subtitle.Paragraphs[i];
                Paragraph prev = _subtitle.GetParagraphOrDefault(i - 1);
                Paragraph target = prev;
                string oldCurrent = p.ToString();
                string oldPrevious = prev.ToString();
                double prevWantedDisplayTime = Utilities.GetOptimalDisplayMilliseconds(prev.Text, Configuration.Settings.General.SubtitleMaximumCharactersPerSeconds);
                double currentWantedDisplayTime = Utilities.GetOptimalDisplayMilliseconds(p.Text, Configuration.Settings.General.SubtitleMaximumCharactersPerSeconds);
                double prevOptimalDisplayTime = Utilities.GetOptimalDisplayMilliseconds(prev.Text);
                double currentOptimalDisplayTime = Utilities.GetOptimalDisplayMilliseconds(p.Text);
                bool canBeEqual = _format != null && (_format.GetType() == typeof(AdvancedSubStationAlpha) || _format.GetType() == typeof(SubStationAlpha));
                if (!canBeEqual)
                    canBeEqual = Configuration.Settings.Tools.FixCommonErrorsFixOverlapAllowEqualEndStart;

                double diff = prev.EndTime.TotalMilliseconds - p.StartTime.TotalMilliseconds;
                if (!prev.StartTime.IsMaxTime && !p.StartTime.IsMaxTime && diff >= 0 && !(canBeEqual && diff == 0))
                {

                    int diffHalf = (int)(diff / 2);
                    if (!Configuration.Settings.Tools.FixCommonErrorsFixOverlapAllowEqualEndStart && p.StartTime.TotalMilliseconds == prev.EndTime.TotalMilliseconds &&
                        prev.Duration.TotalMilliseconds > 100)
                    {
                        if (AllowFix(target, fixAction))
                        {
                            if (!canBeEqual)
                            {
                                bool okEqual = true;
                                if (prev.Duration.TotalMilliseconds > Configuration.Settings.General.SubtitleMinimumDisplayMilliseconds)
                                    prev.EndTime.TotalMilliseconds--;
                                else if (p.Duration.TotalMilliseconds > Configuration.Settings.General.SubtitleMinimumDisplayMilliseconds)
                                    p.StartTime.TotalMilliseconds++;
                                else
                                    okEqual = false;
                                if (okEqual)
                                {
                                    _totalFixes++;
                                    noOfOverlappingDisplayTimesFixed++;
                                    AddFixToListView(target, fixAction, oldPrevious, prev.ToString());
                                }
                            }
                        }
                        //                        prev.EndTime.TotalMilliseconds--;
                    }
                    else if (prevOptimalDisplayTime <= (p.StartTime.TotalMilliseconds - prev.StartTime.TotalMilliseconds))
                    {
                        if (AllowFix(target, fixAction))
                        {
                            prev.EndTime.TotalMilliseconds = p.StartTime.TotalMilliseconds - 1;
                            if (canBeEqual)
                                prev.EndTime.TotalMilliseconds++;
                            _totalFixes++;
                            noOfOverlappingDisplayTimesFixed++;
                            AddFixToListView(target, fixAction, oldPrevious, prev.ToString());
                        }
                    }
                    else if (diff > 0 && currentOptimalDisplayTime <= p.Duration.TotalMilliseconds - diffHalf &&
                             prevOptimalDisplayTime <= prev.Duration.TotalMilliseconds - diffHalf)
                    {
                        if (AllowFix(p, fixAction))
                        {
                            prev.EndTime.TotalMilliseconds -= diffHalf;
                            p.StartTime.TotalMilliseconds = prev.EndTime.TotalMilliseconds + 1;
                            _totalFixes++;
                            noOfOverlappingDisplayTimesFixed++;
                            AddFixToListView(p, fixAction, oldCurrent, p.ToString());
                        }
                    }
                    else if (currentOptimalDisplayTime <= p.EndTime.TotalMilliseconds - prev.EndTime.TotalMilliseconds)
                    {
                        if (AllowFix(p, fixAction))
                        {
                            p.StartTime.TotalMilliseconds = prev.EndTime.TotalMilliseconds + 1;
                            if (canBeEqual)
                                p.StartTime.TotalMilliseconds = prev.EndTime.TotalMilliseconds;

                            _totalFixes++;
                            noOfOverlappingDisplayTimesFixed++;
                            AddFixToListView(p, fixAction, oldCurrent, p.ToString());
                        }
                    }
                    else if (diff > 0 && currentWantedDisplayTime <= p.Duration.TotalMilliseconds - diffHalf &&
                             prevWantedDisplayTime <= prev.Duration.TotalMilliseconds - diffHalf)
                    {
                        if (AllowFix(p, fixAction))
                        {
                            prev.EndTime.TotalMilliseconds -= diffHalf;
                            p.StartTime.TotalMilliseconds = prev.EndTime.TotalMilliseconds + 1;
                            _totalFixes++;
                            noOfOverlappingDisplayTimesFixed++;
                            AddFixToListView(p, fixAction, oldCurrent, p.ToString());
                        }
                    }
                    else if (prevWantedDisplayTime <= (p.StartTime.TotalMilliseconds - prev.StartTime.TotalMilliseconds))
                    {
                        if (AllowFix(target, fixAction))
                        {
                            prev.EndTime.TotalMilliseconds = p.StartTime.TotalMilliseconds - 1;
                            if (canBeEqual)
                                prev.EndTime.TotalMilliseconds++;
                            _totalFixes++;
                            noOfOverlappingDisplayTimesFixed++;
                            AddFixToListView(target, fixAction, oldPrevious, prev.ToString());
                        }
                    }
                    else if (currentWantedDisplayTime <= p.EndTime.TotalMilliseconds - prev.EndTime.TotalMilliseconds)
                    {
                        if (AllowFix(p, fixAction))
                        {
                            p.StartTime.TotalMilliseconds = prev.EndTime.TotalMilliseconds + 1;
                            if (canBeEqual)
                                p.StartTime.TotalMilliseconds = prev.EndTime.TotalMilliseconds;

                            _totalFixes++;
                            noOfOverlappingDisplayTimesFixed++;
                            AddFixToListView(p, fixAction, oldCurrent, p.ToString());
                        }
                    }
                    else if (Math.Abs(p.StartTime.TotalMilliseconds - prev.EndTime.TotalMilliseconds) < 10 && p.Duration.TotalMilliseconds > 1)
                    {
                        if (AllowFix(p, fixAction))
                        {
                            prev.EndTime.TotalMilliseconds -= 2;
                            p.StartTime.TotalMilliseconds = prev.EndTime.TotalMilliseconds + 1;
                            if (canBeEqual)
                                p.StartTime.TotalMilliseconds = prev.EndTime.TotalMilliseconds;
                            _totalFixes++;
                            noOfOverlappingDisplayTimesFixed++;
                            AddFixToListView(p, fixAction, oldCurrent, p.ToString());
                        }
                    }
                    else if (Math.Abs(p.StartTime.TotalMilliseconds - prev.StartTime.TotalMilliseconds) < 10 && Math.Abs(p.EndTime.TotalMilliseconds - prev.EndTime.TotalMilliseconds) < 10)
                    { // merge lines with same time codes
                        if (AllowFix(target, fixAction))
                        {
                            prev.Text = prev.Text.Replace(Environment.NewLine, " ");
                            p.Text = p.Text.Replace(Environment.NewLine, " ");

                            string stripped = Utilities.RemoveHtmlTags(prev.Text).TrimStart();
                            if (!stripped.StartsWith("- "))
                                prev.Text = "- " + prev.Text.TrimStart();

                            stripped = Utilities.RemoveHtmlTags(p.Text).TrimStart();
                            if (!stripped.StartsWith("- "))
                                p.Text = "- " + p.Text.TrimStart();

                            prev.Text = prev.Text.Trim() + Environment.NewLine + p.Text;
                            p.Text = string.Empty;
                            _totalFixes++;
                            noOfOverlappingDisplayTimesFixed++;
                            AddFixToListView(target, fixAction, oldCurrent, p.ToString());

                            p.StartTime.TotalMilliseconds = prev.EndTime.TotalMilliseconds + 1;
                            p.EndTime.TotalMilliseconds = p.StartTime.TotalMilliseconds + 1;
                            if (canBeEqual)
                            {
                                p.StartTime.TotalMilliseconds = prev.EndTime.TotalMilliseconds;
                                p.EndTime.TotalMilliseconds = p.StartTime.TotalMilliseconds;
                            }
                        }
                    }
                    else
                    {
                        if (AllowFix(p, fixAction))
                        {
                            LogStatus(_language.FixOverlappingDisplayTimes, string.Format(_language.UnableToFixTextXY, i + 1, Environment.NewLine + prev.Number + "  " + prev + Environment.NewLine + p.Number + "  " + p), true);
                            _totalErrors++;
                        }
                    }
                }
            }

            if (noOfOverlappingDisplayTimesFixed > 0)
                LogStatus(fixAction, string.Format(_language.XOverlappingTimestampsFixed, noOfOverlappingDisplayTimesFixed));
        }

        public void FixShortDisplayTimes()
        {
            string fixAction = _language.FixShortDisplayTime;
            int noOfShortDisplayTimes = 0;
            for (int i = 0; i < _subtitle.Paragraphs.Count; i++)
            {
                Paragraph p = _subtitle.Paragraphs[i];
                var skip = p.StartTime.IsMaxTime || p.EndTime.IsMaxTime;
                double displayTime = p.Duration.TotalMilliseconds;
                if (!skip && displayTime < Configuration.Settings.General.SubtitleMinimumDisplayMilliseconds)
                {
                    Paragraph next = _subtitle.GetParagraphOrDefault(i + 1);
                    Paragraph prev = _subtitle.GetParagraphOrDefault(i - 1);
                    if (next == null || (p.StartTime.TotalMilliseconds + Configuration.Settings.General.SubtitleMinimumDisplayMilliseconds) < next.StartTime.TotalMilliseconds)
                    {
                        Paragraph temp = new Paragraph(p);
                        temp.EndTime.TotalMilliseconds = p.StartTime.TotalMilliseconds + Configuration.Settings.General.SubtitleMinimumDisplayMilliseconds;
                        if (Utilities.GetCharactersPerSecond(temp) <= Configuration.Settings.General.SubtitleMaximumCharactersPerSeconds)
                        {
                            if (AllowFix(p, fixAction))
                            {
                                string oldCurrent = p.ToString();
                                p.EndTime.TotalMilliseconds = p.StartTime.TotalMilliseconds + Configuration.Settings.General.SubtitleMinimumDisplayMilliseconds;
                                _totalFixes++;
                                noOfShortDisplayTimes++;
                                AddFixToListView(p, fixAction, oldCurrent, p.ToString());
                            }
                        }
                    }
                    else if (Configuration.Settings.Tools.FixShortDisplayTimesAllowMoveStartTime && p.StartTime.TotalMilliseconds > Configuration.Settings.General.SubtitleMinimumDisplayMilliseconds &&
                             (prev == null || prev.EndTime.TotalMilliseconds < p.EndTime.TotalMilliseconds - Configuration.Settings.General.SubtitleMinimumDisplayMilliseconds))
                    {
                        if (AllowFix(p, fixAction))
                        {
                            string oldCurrent = p.ToString();
                            if (next.StartTime.TotalMilliseconds - Configuration.Settings.General.MininumMillisecondsBetweenLines > p.EndTime.TotalMilliseconds)
                                p.EndTime.TotalMilliseconds = next.StartTime.TotalMilliseconds - Configuration.Settings.General.MininumMillisecondsBetweenLines;
                            p.StartTime.TotalMilliseconds = p.EndTime.TotalMilliseconds - Configuration.Settings.General.SubtitleMinimumDisplayMilliseconds;

                            _totalFixes++;
                            noOfShortDisplayTimes++;
                            AddFixToListView(p, fixAction, oldCurrent, p.ToString());
                        }
                    }
                    else
                    {
                        LogStatus(_language.FixShortDisplayTimes, string.Format(_language.UnableToFixTextXY, i + 1, p));
                        _totalErrors++;
                        skip = true;
                    }
                }

                double charactersPerSecond = Utilities.GetCharactersPerSecond(p);
                if (!skip && charactersPerSecond > Configuration.Settings.General.SubtitleMaximumCharactersPerSeconds)
                {
                    Paragraph temp = new Paragraph(p);
                    while (Utilities.GetCharactersPerSecond(temp) > Configuration.Settings.General.SubtitleMaximumCharactersPerSeconds)
                    {
                        temp.EndTime.TotalMilliseconds++;
                    }
                    Paragraph next = _subtitle.GetParagraphOrDefault(i + 1);
                    Paragraph nextNext = _subtitle.GetParagraphOrDefault(i + 2);
                    Paragraph prev = _subtitle.GetParagraphOrDefault(i - 1);
                    double diffMs = temp.Duration.TotalMilliseconds - p.Duration.TotalMilliseconds;

                    // Normal - just make current subtitle duration longer
                    if (next == null || (temp.EndTime.TotalMilliseconds) < next.StartTime.TotalMilliseconds)
                    {
                        if (AllowFix(p, fixAction))
                        {
                            string oldCurrent = p.ToString();
                            p.EndTime.TotalMilliseconds = temp.EndTime.TotalMilliseconds;
                            _totalFixes++;
                            noOfShortDisplayTimes++;
                            AddFixToListView(p, fixAction, oldCurrent, p.ToString());
                        }
                    }

                    // Start current subtitle earlier (max 50 ms)
                    else if (Configuration.Settings.Tools.FixShortDisplayTimesAllowMoveStartTime && p.StartTime.TotalMilliseconds > Configuration.Settings.General.SubtitleMinimumDisplayMilliseconds &&
                             diffMs < 50 && (prev == null || prev.EndTime.TotalMilliseconds < p.EndTime.TotalMilliseconds - temp.Duration.TotalMilliseconds - Configuration.Settings.General.MininumMillisecondsBetweenLines))
                    {
                        noOfShortDisplayTimes = MoveStartTime(fixAction, noOfShortDisplayTimes, p, temp, next);
                    }

                    // Make current subtitle duration longer + move next subtitle
                    else if (diffMs < 1000 &&
                             Configuration.Settings.Tools.FixShortDisplayTimesAllowMoveStartTime &&
                             p.StartTime.TotalMilliseconds > Configuration.Settings.General.SubtitleMinimumDisplayMilliseconds &&
                             (nextNext == null || next.EndTime.TotalMilliseconds + diffMs + Configuration.Settings.General.MininumMillisecondsBetweenLines * 2 < nextNext.StartTime.TotalMilliseconds))
                    {
                        if (AllowFix(p, fixAction))
                        {
                            string oldCurrent = p.ToString();
                            p.EndTime.TotalMilliseconds = p.StartTime.TotalMilliseconds + temp.Duration.TotalMilliseconds;
                            var nextDurationMs = next.Duration.TotalMilliseconds;
                            next.StartTime.TotalMilliseconds = p.EndTime.TotalMilliseconds + Configuration.Settings.General.MininumMillisecondsBetweenLines;
                            next.EndTime.TotalMilliseconds = next.StartTime.TotalMilliseconds + nextDurationMs;

                            _totalFixes++;
                            noOfShortDisplayTimes++;
                            AddFixToListView(p, fixAction, oldCurrent, p.ToString());
                        }
                    }

                    // Make next subtitle duration shorter +  make current subtitle duration longer
                    else if (diffMs < 1000 &&
                             Configuration.Settings.Tools.FixShortDisplayTimesAllowMoveStartTime && Utilities.GetCharactersPerSecond(new Paragraph(next.Text, p.StartTime.TotalMilliseconds + temp.Duration.TotalMilliseconds + Configuration.Settings.General.MininumMillisecondsBetweenLines, next.EndTime.TotalMilliseconds)) < Configuration.Settings.General.SubtitleMaximumCharactersPerSeconds)
                    {
                        if (AllowFix(p, fixAction))
                        {
                            string oldCurrent = p.ToString();
                            next.StartTime.TotalMilliseconds = p.StartTime.TotalMilliseconds + temp.Duration.TotalMilliseconds + Configuration.Settings.General.MininumMillisecondsBetweenLines;
                            p.EndTime.TotalMilliseconds = next.StartTime.TotalMilliseconds - Configuration.Settings.General.MininumMillisecondsBetweenLines;

                            _totalFixes++;
                            noOfShortDisplayTimes++;
                            AddFixToListView(p, fixAction, oldCurrent, p.ToString());
                        }
                    }

                    // Make next-next subtitle duration shorter + move next + make current subtitle duration longer
                    else if (diffMs < 500 &&
                             Configuration.Settings.Tools.FixShortDisplayTimesAllowMoveStartTime && nextNext != null &&
                             Utilities.GetCharactersPerSecond(new Paragraph(nextNext.Text, nextNext.StartTime.TotalMilliseconds + diffMs + Configuration.Settings.General.MininumMillisecondsBetweenLines, nextNext.EndTime.TotalMilliseconds - (diffMs))) < Configuration.Settings.General.SubtitleMaximumCharactersPerSeconds)
                    {
                        if (AllowFix(p, fixAction))
                        {
                            string oldCurrent = p.ToString();
                            p.EndTime.TotalMilliseconds += diffMs;
                            next.StartTime.TotalMilliseconds += diffMs;
                            next.EndTime.TotalMilliseconds += diffMs;
                            nextNext.StartTime.TotalMilliseconds += diffMs;

                            _totalFixes++;
                            noOfShortDisplayTimes++;
                            AddFixToListView(p, fixAction, oldCurrent, p.ToString());
                        }
                    }

                    // Start current subtitle earlier (max 200 ms)
                    else if (Configuration.Settings.Tools.FixShortDisplayTimesAllowMoveStartTime && p.StartTime.TotalMilliseconds > Configuration.Settings.General.SubtitleMinimumDisplayMilliseconds &&
                             diffMs < 200 && (prev == null || prev.EndTime.TotalMilliseconds < p.EndTime.TotalMilliseconds - temp.Duration.TotalMilliseconds - Configuration.Settings.General.MininumMillisecondsBetweenLines))
                    {
                        noOfShortDisplayTimes = MoveStartTime(fixAction, noOfShortDisplayTimes, p, temp, next);
                    }
                    else
                    {
                        LogStatus(_language.FixShortDisplayTimes, string.Format(_language.UnableToFixTextXY, i + 1, p));
                        _totalErrors++;
                    }
                }
                skip = false;
            }
            if (noOfShortDisplayTimes > 0)
                LogStatus(fixAction, string.Format(_language.XDisplayTimesProlonged, noOfShortDisplayTimes));
        }

        private int MoveStartTime(string fixAction, int noOfShortDisplayTimes, Paragraph p, Paragraph temp, Paragraph next)
        {
            if (AllowFix(p, fixAction))
            {
                string oldCurrent = p.ToString();
                if (next != null && next.StartTime.TotalMilliseconds - Configuration.Settings.General.MininumMillisecondsBetweenLines > p.EndTime.TotalMilliseconds)
                    p.EndTime.TotalMilliseconds = next.StartTime.TotalMilliseconds - Configuration.Settings.General.MininumMillisecondsBetweenLines;
                p.StartTime.TotalMilliseconds = p.EndTime.TotalMilliseconds - temp.Duration.TotalMilliseconds;

                _totalFixes++;
                noOfShortDisplayTimes++;
                AddFixToListView(p, fixAction, oldCurrent, p.ToString());
            }
            return noOfShortDisplayTimes;
        }

        public void FixInvalidItalicTags()
        {
            const string beginTag = "<i>";
            const string endTag = "</i>";
            string fixAction = _language.FixInvalidItalicTag;
            int noOfInvalidHtmlTags = 0;
            listViewFixes.BeginUpdate();
            for (int i = 0; i < _subtitle.Paragraphs.Count; i++)
            {
                var text = _subtitle.Paragraphs[i].Text;
                if (text.Contains('<'))
                {
                    text = text.Replace(beginTag.ToUpper(), beginTag).Replace(endTag.ToUpper(), endTag);
                    string oldText = text;

                    text = Utilities.FixInvalidItalicTags(text);
                    if (text != oldText)
                    {
                        if (AllowFix(_subtitle.Paragraphs[i], fixAction))
                        {
                            _subtitle.Paragraphs[i].Text = text;
                            _totalFixes++;
                            noOfInvalidHtmlTags++;
                            AddFixToListView(_subtitle.Paragraphs[i], fixAction, oldText, text);
                        }
                    }
                }
            }
            listViewFixes.EndUpdate();
            listViewFixes.Refresh();
            if (noOfInvalidHtmlTags > 0)
                LogStatus(_language.FixInvalidItalicTags, string.Format(_language.XInvalidHtmlTagsFixed, noOfInvalidHtmlTags));
        }

        public void FixLongDisplayTimes()
        {
            string fixAction = _language.FixLongDisplayTime;
            int noOfLongDisplayTimes = 0;
            for (int i = 0; i < _subtitle.Paragraphs.Count; i++)
            {
                Paragraph p = _subtitle.Paragraphs[i];
                double maxDisplayTime = Utilities.GetOptimalDisplayMilliseconds(p.Text) * 8.0;
                if (maxDisplayTime > Configuration.Settings.General.SubtitleMaximumDisplayMilliseconds)
                    maxDisplayTime = Configuration.Settings.General.SubtitleMaximumDisplayMilliseconds;
                double displayTime = p.Duration.TotalMilliseconds;
                if (displayTime > Configuration.Settings.General.SubtitleMaximumDisplayMilliseconds)
                {
                    if (AllowFix(p, fixAction))
                    {
                        string oldCurrent = p.ToString();
                        p.EndTime.TotalMilliseconds = p.StartTime.TotalMilliseconds + Configuration.Settings.General.SubtitleMaximumDisplayMilliseconds;
                        _totalFixes++;
                        noOfLongDisplayTimes++;
                        AddFixToListView(p, fixAction, oldCurrent, p.ToString());
                    }
                }
                else if (maxDisplayTime < displayTime)
                {
                    if (AllowFix(p, fixAction))
                    {
                        string oldCurrent = p.ToString();
                        displayTime = Utilities.GetOptimalDisplayMilliseconds(p.Text);
                        p.EndTime.TotalMilliseconds = p.StartTime.TotalMilliseconds + displayTime;
                        _totalFixes++;
                        noOfLongDisplayTimes++;
                        AddFixToListView(p, fixAction, oldCurrent, p.ToString());
                    }
                }
            }
            if (noOfLongDisplayTimes > 0)
                LogStatus(_language.FixLongDisplayTimes, string.Format(_language.XDisplayTimesShortned, noOfLongDisplayTimes));
        }

        public void FixLongLines()
        {
            string fixAction = _language.BreakLongLine;
            int noOfLongLines = 0;
            for (int i = 0; i < _subtitle.Paragraphs.Count; i++)
            {
                Paragraph p = _subtitle.Paragraphs[i];
                string[] lines = p.Text.Split(Utilities.NewLineChars, StringSplitOptions.RemoveEmptyEntries);
                bool tooLong = false;
                foreach (string line in lines)
                {
                    if (Utilities.RemoveHtmlTags(line).Length > Configuration.Settings.General.SubtitleLineMaximumLength)
                    {
                        tooLong = true;
                    }
                }
                if (tooLong)
                {
                    if (AllowFix(p, fixAction))
                    {
                        string oldText = p.Text;
                        p.Text = Utilities.AutoBreakLine(p.Text, Language);
                        if (oldText != p.Text)
                        {
                            _totalFixes++;
                            noOfLongLines++;
                            AddFixToListView(p, fixAction, oldText, p.Text);
                        }
                        else
                        {
                            LogStatus(fixAction, string.Format(_language.UnableToFixTextXY, i + 1, p));
                            _totalErrors++;
                        }
                    }
                }
            }
            if (noOfLongLines > 0)
                LogStatus(_language.BreakLongLines, string.Format(_language.XLineBreaksAdded, noOfLongLines));
        }

        public void FixShortLines()
        {
            string fixAction = _language.MergeShortLine;
            int noOfShortLines = 0;
            for (int i = 0; i < _subtitle.Paragraphs.Count; i++)
            {
                Paragraph p = _subtitle.Paragraphs[i];

                string s = Utilities.RemoveHtmlTags(p.Text);
                if (s.Replace(Environment.NewLine, " ").Replace("  ", " ").Length < Configuration.Settings.Tools.MergeLinesShorterThan && p.Text.Contains(Environment.NewLine))
                {
                    s = s.TrimEnd().TrimEnd('.', '?', '!', ':', ';');
                    s = s.TrimStart('-');
                    if (!s.Contains('.') &&
                        !s.Contains('?') &&
                        !s.Contains('!') &&
                        !s.Contains(':') &&
                        !s.Contains(';') &&
                        !s.Contains('-') &&
                        p.Text != p.Text.ToUpper())
                    {
                        if (AllowFix(p, fixAction))
                        {
                            s = p.Text.Replace(Environment.NewLine, " ");
                            s = s.Replace("  ", " ");

                            string oldCurrent = p.Text;
                            p.Text = s;
                            _totalFixes++;
                            noOfShortLines++;
                            AddFixToListView(p, fixAction, oldCurrent, p.Text);
                        }
                    }
                }
            }
            if (noOfShortLines > 0)
                LogStatus(_language.RemoveLineBreaks, string.Format(_language.XLinesUnbreaked, noOfShortLines));
        }

        public void FixShortLinesAll()
        {
            string fixAction = _language.MergeShortLineAll;
            int noOfShortLines = 0;
            for (int i = 0; i < _subtitle.Paragraphs.Count; i++)
            {
                Paragraph p = _subtitle.Paragraphs[i];

                string s = Utilities.RemoveHtmlTags(p.Text);
                if (s.Replace(Environment.NewLine, " ").Replace("  ", " ").Length < Configuration.Settings.Tools.MergeLinesShorterThan && p.Text.Contains(Environment.NewLine))
                {
                    s = Utilities.AutoBreakLine(p.Text, Language);
                    if (s != p.Text)
                    {
                        if (AllowFix(p, fixAction))
                        {
                            string oldCurrent = p.Text;
                            p.Text = s;
                            _totalFixes++;
                            noOfShortLines++;
                            AddFixToListView(p, fixAction, oldCurrent, p.Text);
                        }
                    }
                }
            }
            if (noOfShortLines > 0)
                LogStatus(_language.RemoveLineBreaks, string.Format(_language.XLinesUnbreaked, noOfShortLines));
        }

        public void FixUnneededSpaces()
        {
            const string char160 = " "; // Convert.ToChar(160).ToString()

            string fixAction = _language.UnneededSpace;
            int doubleSpaces = 0;
            for (int i = 0; i < _subtitle.Paragraphs.Count; i++)
            {
                Paragraph p = _subtitle.Paragraphs[i];
                string oldText = p.Text;

                p.Text = Utilities.RemoveUnneededSpaces(p.Text, Language);

                if (p.Text.Length != oldText.Length && Utilities.CountTagInText(p.Text, " ") != Utilities.CountTagInText(oldText, " ") + Utilities.CountTagInText(oldText, char160))
                {
                    if (AllowFix(p, fixAction))
                    {
                        doubleSpaces++;
                        _totalFixes++;
                        AddFixToListView(p, fixAction, oldText, p.Text);
                    }
                    else
                    {
                        p.Text = oldText;
                    }
                }
                else
                {
                    p.Text = oldText;
                }

            }
            if (doubleSpaces > 0)
                LogStatus(_language.RemoveUnneededSpaces, string.Format(_language.XUnneededSpacesRemoved, doubleSpaces));
        }

        public void FixUnneededPeriods()
        {
            string fixAction = _language.UnneededPeriod;
            int unneededPeriods = 0;
            for (int i = 0; i < _subtitle.Paragraphs.Count; i++)
            {
                Paragraph p = _subtitle.Paragraphs[i];
                if (p.Text.Contains("!." + Environment.NewLine))
                {
                    if (AllowFix(p, fixAction))
                    {
                        string oldText = p.Text;
                        p.Text = p.Text.Replace("!." + Environment.NewLine, "!" + Environment.NewLine);
                        unneededPeriods++;
                        _totalFixes++;
                        AddFixToListView(p, fixAction, oldText, p.Text);
                    }
                }
                if (p.Text.Contains("?." + Environment.NewLine))
                {
                    if (AllowFix(p, fixAction))
                    {
                        string oldText = p.Text;
                        p.Text = p.Text.Replace("?." + Environment.NewLine, "?" + Environment.NewLine);
                        unneededPeriods++;
                        _totalFixes++;
                        AddFixToListView(p, fixAction, oldText, p.Text);
                    }
                }
                if (p.Text.EndsWith("!.", StringComparison.Ordinal))
                {
                    if (AllowFix(p, fixAction))
                    {
                        string oldText = p.Text;
                        p.Text = p.Text.TrimEnd('.');
                        unneededPeriods++;
                        _totalFixes++;
                        AddFixToListView(p, fixAction, oldText, p.Text);
                    }
                }
                if (p.Text.EndsWith("?.", StringComparison.Ordinal))
                {
                    if (AllowFix(p, fixAction))
                    {
                        string oldText = p.Text;
                        p.Text = p.Text.TrimEnd('.');
                        unneededPeriods++;
                        _totalFixes++;
                        AddFixToListView(p, fixAction, oldText, p.Text);
                    }
                }

                if (p.Text.Contains("!. "))
                {
                    if (AllowFix(p, fixAction))
                    {
                        string oldText = p.Text;
                        p.Text = p.Text.Replace("!. ", "! ");
                        unneededPeriods++;
                        _totalFixes++;
                        AddFixToListView(p, fixAction, oldText, p.Text);
                    }
                }
                if (p.Text.Contains("?. "))
                {
                    if (AllowFix(p, fixAction))
                    {
                        string oldText = p.Text;
                        p.Text = p.Text.Replace("?. ", "? ");
                        unneededPeriods++;
                        _totalFixes++;
                        AddFixToListView(p, fixAction, oldText, p.Text);
                    }
                }

            }
            if (unneededPeriods > 0)
                LogStatus(_language.RemoveUnneededPeriods, string.Format(_language.XUnneededPeriodsRemoved, unneededPeriods));
        }

        public void FixMissingSpaces()
        {
            string fixAction = _language.FixMissingSpace;
            int missingSpaces = 0;
            for (int i = 0; i < _subtitle.Paragraphs.Count; i++)
            {
                Paragraph p = _subtitle.Paragraphs[i];

                // missing space after comma ","
                Match match = FixMissingSpacesReComma.Match(p.Text);
                if (match.Success)
                {
                    while (match.Success)
                    {
                        if (!@"""”<.".Contains(p.Text[match.Index + 2]))
                        {
                            if (AllowFix(p, fixAction))
                            {
                                _totalFixes++;
                                missingSpaces++;

                                string oldText = p.Text;
                                p.Text = p.Text.Replace(match.Value, match.Value[0] + ", " + match.Value[match.Value.Length - 1]);
                                AddFixToListView(p, fixAction, oldText, p.Text);
                            }
                        }
                        match = match.NextMatch();
                    }
                }

                // missing space after "?"
                match = FixMissingSpacesReQuestionMark.Match(p.Text);
                if (match.Success)
                {
                    while (match.Success)
                    {
                        if (!@"""<".Contains(p.Text[match.Index + 2]))
                        {
                            if (AllowFix(p, fixAction))
                            {
                                _totalFixes++;
                                missingSpaces++;

                                string oldText = p.Text;
                                p.Text = p.Text.Replace(match.Value, match.Value[0] + "? " + match.Value[match.Value.Length - 1]);
                                AddFixToListView(p, fixAction, oldText, p.Text);
                            }
                        }
                        match = FixMissingSpacesReQuestionMark.Match(p.Text, match.Index + 1);
                    }
                }

                // missing space after "!"
                match = FixMissingSpacesReExclamation.Match(p.Text);
                if (match.Success)
                {
                    while (match.Success)
                    {
                        if (!@"""<".Contains(p.Text[match.Index + 2]))
                        {
                            if (AllowFix(p, fixAction))
                            {
                                _totalFixes++;
                                missingSpaces++;

                                string oldText = p.Text;
                                p.Text = p.Text.Replace(match.Value, match.Value[0] + "! " + match.Value[match.Value.Length - 1]);
                                AddFixToListView(p, fixAction, oldText, p.Text);
                            }
                        }
                        match = FixMissingSpacesReExclamation.Match(p.Text, match.Index + 1);
                    }
                }

                // missing space after ":"
                match = FixMissingSpacesReColon.Match(p.Text);
                if (match.Success)
                {
                    while (match.Success)
                    {
                        int start = match.Index;
                        start -= 4;
                        if (start < 0)
                            start = 0;
                        int indexOfStartCodeTag = p.Text.IndexOf('{', start);
                        int indexOfEndCodeTag = p.Text.IndexOf('}', start);
                        if (indexOfStartCodeTag >= 0 && indexOfEndCodeTag >= 0 && indexOfStartCodeTag < match.Index)
                        {
                            // we are inside a tag: like indexOfEndCodeTag "{y:i}Is this italic?"
                        }
                        else if (!@"""<".Contains(p.Text[match.Index + 2]))
                        {
                            if (AllowFix(p, fixAction))
                            {
                                _totalFixes++;
                                missingSpaces++;

                                string oldText = p.Text;
                                p.Text = p.Text.Replace(match.Value, match.Value[0] + ": " + match.Value[match.Value.Length - 1]);
                                AddFixToListView(p, fixAction, oldText, p.Text);
                            }
                        }
                        match = FixMissingSpacesReColon.Match(p.Text, match.Index + 1);
                    }
                }

                // missing space after period "."
                match = FixMissingSpacesRePeriod.Match(p.Text);
                if (match.Success)
                {
                    while (match.Success)
                    {
                        if (!p.Text.Contains("www.", StringComparison.OrdinalIgnoreCase) &&
                            !p.Text.Contains("http://", StringComparison.OrdinalIgnoreCase) &&
                            !UrlCom.IsMatch(p.Text) &&
                            !UrlNet.IsMatch(p.Text) &&
                            !UrlOrg.IsMatch(p.Text)) // urls are skipped
                        {
                            bool isMatchAbbreviation = false;

                            string word = GetWordFromIndex(p.Text, match.Index);
                            if (Utilities.CountTagInText(word, '.') > 1)
                                isMatchAbbreviation = true;

                            if (!isMatchAbbreviation && word.Contains('@')) // skip emails
                                isMatchAbbreviation = true;

                            if (match.Value.Equals("h.d", StringComparison.OrdinalIgnoreCase) && match.Index > 0 && p.Text.Substring(match.Index - 1, 4).Equals("ph.d", StringComparison.OrdinalIgnoreCase))
                                isMatchAbbreviation = true;

                            if (!isMatchAbbreviation && AllowFix(p, fixAction))
                            {
                                _totalFixes++;
                                missingSpaces++;

                                string oldText = p.Text;
                                p.Text = p.Text.Replace(match.Value, match.Value.Replace(".", ". "));
                                AddFixToListView(p, fixAction, oldText, p.Text);
                            }
                        }
                        match = match.NextMatch();
                    }
                }

                if (!p.Text.StartsWith("--"))
                {
                    string[] arr = p.Text.Split(Utilities.NewLineChars, StringSplitOptions.RemoveEmptyEntries);
                    if (arr.Length == 2 && arr[0].Length > 1 && arr[1].Length > 1)
                    {
                        if (arr[0][0] == '-' && arr[0][1] != ' ')
                            arr[0] = arr[0].Insert(1, " ");
                        if (arr[0].Length > 6 && arr[0].StartsWith("<i>-") && arr[0][4] != ' ')
                            arr[0] = arr[0].Insert(4, " ");
                        if (arr[1][0] == '-' && arr[1][1] != ' ' && arr[1][1] != '-')
                            arr[1] = arr[1].Insert(1, " ");
                        if (arr[1].Length > 6 && arr[1].StartsWith("<i>-") && arr[1][4] != ' ')
                            arr[1] = arr[1].Insert(4, " ");
                        string newText = arr[0] + Environment.NewLine + arr[1];
                        if (newText != p.Text && AllowFix(p, fixAction))
                        {
                            _totalFixes++;
                            missingSpaces++;

                            string oldText = p.Text;
                            p.Text = newText;
                            AddFixToListView(p, fixAction, oldText, p.Text);
                        }
                    }
                }

                //fix missing spaces before/after quotes - Get a"get out of jail free"card. -> Get a "get out of jail free" card.
                if (Utilities.CountTagInText(p.Text, "\"") == 2)
                {
                    int start = p.Text.IndexOf('"');
                    int end = p.Text.LastIndexOf('"');
                    string quote = p.Text.Substring(start, end - start + 1);
                    if (!quote.Contains(Environment.NewLine))
                    {
                        string newText = p.Text;
                        int indexOfFontTag = newText.ToLower().IndexOf("<font ", StringComparison.Ordinal);
                        if (start > 0 && !(Environment.NewLine + @" >[(♪♫¿").Contains(p.Text[start - 1]))
                        {
                            if (indexOfFontTag == -1 || start > newText.IndexOf('>', indexOfFontTag)) // font tags can contain "
                            {
                                newText = newText.Insert(start, " ");
                                end++;
                            }
                        }
                        if (end < newText.Length - 2 && !(Environment.NewLine + @" <,.!?:;])♪♫¿").Contains(p.Text[end + 1]))
                        {
                            if (indexOfFontTag == -1 || end > newText.IndexOf('>', indexOfFontTag)) // font tags can contain "
                            {
                                newText = newText.Insert(end + 1, " ");
                            }
                        }
                        if (newText != p.Text && AllowFix(p, fixAction))
                        {
                            _totalFixes++;
                            missingSpaces++;

                            string oldText = p.Text;
                            p.Text = newText;
                            AddFixToListView(p, fixAction, oldText, p.Text);
                        }
                    }
                }

                //fix missing spaces before/after music quotes - #He's so happy# -> #He's so happy#
                if ((p.Text.Contains('#') || p.Text.Contains('♪') || p.Text.Contains('♫')) && p.Text.Length > 5)
                {
                    string newText = p.Text;
                    if (@"#♪♫".Contains(newText[0]) && !@" <".Contains(newText[1]) && !newText.Substring(1).StartsWith(Environment.NewLine) &&
                        !newText.Substring(1).StartsWith('♪') && !newText.Substring(1).StartsWith('♫'))
                        newText = newText.Insert(1, " ");
                    if (@"#♪♫".Contains(newText[newText.Length - 1]) && !@" >".Contains(newText[newText.Length - 2]) &&
                        !newText.Substring(0, newText.Length - 1).EndsWith(Environment.NewLine, StringComparison.Ordinal) && !newText.Substring(0, newText.Length - 1).EndsWith('♪') &&
                        !newText.Substring(0, newText.Length - 1).EndsWith('♫'))
                        newText = newText.Insert(newText.Length - 1, " ");
                    if (newText != p.Text && AllowFix(p, fixAction))
                    {
                        _totalFixes++;
                        missingSpaces++;

                        string oldText = p.Text;
                        p.Text = newText;
                        AddFixToListView(p, fixAction, oldText, p.Text);
                    }
                }

                //fix missing spaces in "Hey...move it!" to "Hey... move it!"
                int index = p.Text.IndexOf("...", StringComparison.Ordinal);
                if (index >= 0 && p.Text.Length > 5)
                {
                    string newText = p.Text;
                    while (index != -1)
                    {
                        if (newText.Length > index + 4 && index > 1)
                        {
                            if (Utilities.AllLettersAndNumbers.Contains(newText[index + 3]) &&
                                Utilities.AllLettersAndNumbers.Contains(newText[index - 1]))
                                newText = newText.Insert(index + 3, " ");
                        }
                        index = newText.IndexOf("...", index + 2, StringComparison.Ordinal);
                    }
                    if (newText != p.Text && AllowFix(p, fixAction))
                    {
                        _totalFixes++;
                        missingSpaces++;

                        string oldText = p.Text;
                        p.Text = newText;
                        AddFixToListView(p, fixAction, oldText, p.Text);
                    }
                }

                //fix missing spaces in "The<i>Bombshell</i> will gone." to "The <i>Bombshell</i> will gone."
                index = p.Text.IndexOf("<i>", StringComparison.Ordinal);
                if (index >= 0 && p.Text.Length > 5)
                {
                    string newText = p.Text;
                    while (index != -1)
                    {
                        if (newText.Length > index + 6 && index > 1)
                        {
                            if (Utilities.AllLettersAndNumbers.Contains(newText[index + 3]) &&
                                Utilities.AllLettersAndNumbers.Contains(newText[index - 1]))
                                newText = newText.Insert(index, " ");
                        }
                        index = newText.IndexOf("<i>", index + 3, StringComparison.Ordinal);
                    }
                    if (newText != p.Text && AllowFix(p, fixAction))
                    {
                        _totalFixes++;
                        missingSpaces++;

                        string oldText = p.Text;
                        p.Text = newText;
                        AddFixToListView(p, fixAction, oldText, p.Text);
                    }
                }

                //fix missing spaces in "The <i>Bombshell</i>will gone." to "The <i>Bombshell</i> will gone."
                index = p.Text.IndexOf("</i>", StringComparison.Ordinal);
                if (index >= 0 && p.Text.Length > 5)
                {
                    string newText = p.Text;
                    while (index != -1)
                    {
                        if (newText.Length > index + 6 && index > 1)
                        {
                            if (Utilities.AllLettersAndNumbers.Contains(newText[index + 4]) &&
                                Utilities.AllLettersAndNumbers.Contains(newText[index - 1]))
                                newText = newText.Insert(index + 4, " ");
                        }
                        index = newText.IndexOf("</i>", index + 4, StringComparison.Ordinal);
                    }
                    if (newText != p.Text && AllowFix(p, fixAction))
                    {
                        _totalFixes++;
                        missingSpaces++;

                        string oldText = p.Text;
                        p.Text = newText;
                        AddFixToListView(p, fixAction, oldText, p.Text);
                    }
                }

                if (Language == "fr") // special rules for French
                {
                    string newText = p.Text;
                    int j = 1;
                    while (j < newText.Length)
                    {
                        if (@"!?:;".Contains(newText[j]))
                        {
                            if (Utilities.AllLetters.Contains(newText[j - 1]))
                            {
                                newText = newText.Insert(j, " ");
                                j++;
                            }
                        }
                        j++;
                    }
                    if (newText != p.Text && AllowFix(p, fixAction))
                    {
                        _totalFixes++;
                        missingSpaces++;

                        string oldText = p.Text;
                        p.Text = newText;
                        AddFixToListView(p, fixAction, oldText, p.Text);
                    }
                }

            }
            if (missingSpaces > 0)
                LogStatus(_language.FixMissingSpaces, string.Format(_language.XMissingSpacesAdded, missingSpaces));
        }

        private static string GetWordFromIndex(string text, int index)
        {
            if (string.IsNullOrEmpty(text) || index < 0 || index >= text.Length)
                return string.Empty;

            int endIndex = index;
            for (int i = index; i < text.Length; i++)
            {
                if ((@" " + Environment.NewLine).Contains(text[i]))
                    break;
                endIndex = i;
            }

            int startIndex = index;
            for (int i = index; i >= 0; i--)
            {
                if ((@" " + Environment.NewLine).Contains(text[i]))
                    break;
                startIndex = i;
            }

            return text.Substring(startIndex, endIndex - startIndex + 1);
        }

        public void AddMissingQuotes()
        {
            string fixAction = _language.AddMissingQuote;
            int noOfFixes = 0;
            for (int i = 0; i < _subtitle.Paragraphs.Count; i++)
            {
                Paragraph p = _subtitle.Paragraphs[i];

                if (Utilities.CountTagInText(p.Text, "\"") == 1)
                {
                    Paragraph next = _subtitle.GetParagraphOrDefault(i + 1);
                    if (next != null)
                    {
                        double betweenMilliseconds = next.StartTime.TotalMilliseconds - p.EndTime.TotalMilliseconds;
                        if (betweenMilliseconds > 1500)
                            next = null; // cannot be quote spanning several lines of more than 1.5 seconds between lines!
                        else if (next.Text.Replace("<i>", string.Empty).TrimStart().TrimStart('-').TrimStart().StartsWith('"') &&
                                 next.Text.Replace("</i>", string.Empty).TrimEnd().EndsWith('"') &&
                                 Utilities.CountTagInText(next.Text, "\"") == 2)
                            next = null; // seems to have valid quotes, so no spanning
                    }

                    Paragraph prev = _subtitle.GetParagraphOrDefault(i - 1);
                    if (prev != null)
                    {
                        double betweenMilliseconds = p.StartTime.TotalMilliseconds - prev.EndTime.TotalMilliseconds;
                        if (betweenMilliseconds > 1500)
                            prev = null; // cannot be quote spanning several lines of more than 1.5 seconds between lines!
                        else if (prev.Text.Replace("<i>", string.Empty).TrimStart().TrimStart('-').TrimStart().StartsWith('"') &&
                                 prev.Text.Replace("</i>", string.Empty).TrimEnd().EndsWith('"') &&
                                 Utilities.CountTagInText(prev.Text, "\"") == 2)
                            prev = null; // seems to have valid quotes, so no spanning
                    }

                    string oldText = p.Text;
                    string[] lines = Utilities.RemoveHtmlTags(p.Text).Split(Utilities.NewLineChars, StringSplitOptions.RemoveEmptyEntries);
                    if (lines.Length == 2 && lines[0].TrimStart().StartsWith('-') && lines[1].TrimStart().StartsWith('-'))
                    { // dialog
                        lines = p.Text.Split(Utilities.NewLineChars, StringSplitOptions.RemoveEmptyEntries);
                        string line = lines[0].Trim();

                        if (line.Length > 5 && line.TrimStart().StartsWith("- \"") && (line.EndsWith('.') || line.EndsWith('!') || line.EndsWith('?')))
                        {
                            p.Text = p.Text.Trim().Replace(" " + Environment.NewLine, Environment.NewLine);
                            p.Text = p.Text.Replace(Environment.NewLine, "\"" + Environment.NewLine);
                        }
                        else if (line.Length > 5 && line.EndsWith('"') && line.Contains("- ") && line.IndexOf("- ", StringComparison.Ordinal) < 4)
                        {
                            p.Text = p.Text.Insert(line.IndexOf("- ", StringComparison.Ordinal) + 2, "\"");
                        }
                        else if (line.Contains('"') && line.IndexOf('"') > 2 && line.IndexOf('"') < line.Length - 3)
                        {
                            int index = line.IndexOf('"');
                            if (line[index - 1] == ' ')
                            {
                                p.Text = p.Text.Trim().Replace(" " + Environment.NewLine, Environment.NewLine);
                                p.Text = p.Text.Replace(Environment.NewLine, "\"" + Environment.NewLine);
                            }
                            else if (line[index + 1] == ' ')
                            {
                                if (line.Length > 5 && line.Contains("- ") && line.IndexOf("- ", StringComparison.Ordinal) < 4)
                                    p.Text = p.Text.Insert(line.IndexOf("- ", StringComparison.Ordinal) + 2, "\"");
                            }
                        }
                        else if (lines[1].Contains('"'))
                        {
                            line = lines[1].Trim();
                            if (line.Length > 5 && line.TrimStart().StartsWith("- \"") && (line.EndsWith('.') || line.EndsWith('!') || line.EndsWith('?')))
                            {
                                p.Text = p.Text.Trim() + "\"";
                            }
                            else if (line.Length > 5 && line.EndsWith('"') && p.Text.Contains(Environment.NewLine + "- "))
                            {
                                p.Text = p.Text.Insert(p.Text.IndexOf(Environment.NewLine + "- ", StringComparison.Ordinal) + Environment.NewLine.Length + 2, "\"");
                            }
                            else if (line.Contains('"') && line.IndexOf('"') > 2 && line.IndexOf('"') < line.Length - 3)
                            {
                                int index = line.IndexOf('"');
                                if (line[index - 1] == ' ')
                                {
                                    p.Text = p.Text.Trim() + "\"";
                                }
                                else if (line[index + 1] == ' ')
                                {
                                    if (line.Length > 5 && p.Text.Contains(Environment.NewLine + "- "))
                                        p.Text = p.Text.Insert(p.Text.IndexOf(Environment.NewLine + "- ", StringComparison.Ordinal) + Environment.NewLine.Length + 2, "\"");
                                }
                            }
                        }
                    }
                    else
                    { // not dialog
                        if (p.Text.StartsWith('"'))
                        {
                            if (next == null || !next.Text.Contains('"'))
                                p.Text += "\"";
                        }
                        else if (p.Text.StartsWith("<i>\"") && p.Text.EndsWith("</i>", StringComparison.Ordinal) && Utilities.CountTagInText(p.Text, "</i>") == 1)
                        {
                            if (next == null || !next.Text.Contains('"'))
                                p.Text = p.Text.Replace("</i>", "\"</i>");
                        }
                        else if (p.Text.EndsWith('"'))
                        {
                            if (prev == null || !prev.Text.Contains('"'))
                                p.Text = "\"" + p.Text;
                        }
                        else if (p.Text.Contains(Environment.NewLine + "\"") && Utilities.CountTagInText(p.Text, Environment.NewLine) == 1)
                        {
                            if (next == null || !next.Text.Contains('"'))
                                p.Text = p.Text + "\"";
                        }
                        else if ((p.Text.Contains(Environment.NewLine + "\"") || p.Text.Contains(Environment.NewLine + "-\"") || p.Text.Contains(Environment.NewLine + "- \"")) &&
                                 Utilities.CountTagInText(p.Text, Environment.NewLine) == 1 && p.Text.Length > 3)
                        {
                            if (next == null || !next.Text.Contains('"'))
                            {
                                if (p.Text.StartsWith("<i>") && p.Text.EndsWith("</i>", StringComparison.Ordinal) && Utilities.CountTagInText(p.Text, "</i>") == 1)
                                    p.Text = p.Text.Replace("</i>", "\"</i>");
                                else
                                    p.Text = p.Text + "\"";
                            }
                        }
                        else if (p.Text.StartsWith("<i>") && p.Text.EndsWith("</i>", StringComparison.Ordinal) && Utilities.CountTagInText(p.Text, "<i>") == 1)
                        {
                            if (prev == null || !prev.Text.Contains('"'))
                                p.Text = p.Text.Replace("<i>", "<i>\"");
                        }
                        else if (p.Text.Contains('"'))
                        {
                            string text = p.Text;
                            int indexOfQuote = p.Text.IndexOf('"');
                            if (text.Contains('"') && indexOfQuote > 2 && indexOfQuote < text.Length - 3)
                            {
                                int index = text.IndexOf('"');
                                if (text[index - 1] == ' ')
                                {
                                    if (p.Text.EndsWith(','))
                                        p.Text = p.Text.Insert(p.Text.Length - 1, "\"").Trim();
                                    else
                                        p.Text = p.Text.Trim() + "\"";
                                }
                                else if (text[index + 1] == ' ')
                                    p.Text = "\"" + p.Text;
                            }
                        }
                    }

                    if (oldText != p.Text)
                    {
                        if (AllowFix(p, fixAction))
                        {
                            _totalFixes++;
                            noOfFixes++;
                            AddFixToListView(p, fixAction, oldText, p.Text);
                        }
                        else
                        {
                            p.Text = oldText;
                        }
                    }

                }
            }
            if (noOfFixes > 0)
                LogStatus(fixAction, string.Format(_language.XMissingQuotesAdded, noOfFixes));
        }

        private static string GetWholeWord(string text, int index)
        {
            int start = index;
            while (start > 0 && !(Environment.NewLine + @" ,.!?""'=()/-").Contains(text[start - 1]))
                start--;

            int end = index;
            while (end + 1 < text.Length && !(Environment.NewLine + @" ,.!?""'=()/-").Contains(text[end + 1]))
                end++;

            return text.Substring(start, end - start + 1);
        }

        public void FixUppercaseIInsideWords()
        {
            string fixAction = _language.FixUppercaseIInsideLowercaseWord;
            int uppercaseIsInsideLowercaseWords = 0;
            //            bool isLineContinuation = false;
            for (int i = 0; i < _subtitle.Paragraphs.Count; i++)
            {
                Paragraph p = _subtitle.Paragraphs[i];
                string oldText = p.Text;

                Match match = ReAfterLowercaseLetter.Match(p.Text);
                while (match.Success)
                {
                    if (!(match.Index > 1 && p.Text.Substring(match.Index - 1, 2) == "Mc") // irish names, McDonalds etc.
                        && p.Text[match.Index + 1] == 'I'
                        && AllowFix(p, fixAction))
                    {
                        p.Text = p.Text.Substring(0, match.Index + 1) + "l";
                        if (match.Index + 2 < oldText.Length)
                            p.Text += oldText.Substring(match.Index + 2);

                        uppercaseIsInsideLowercaseWords++;
                        _totalFixes++;
                        AddFixToListView(p, fixAction, oldText, p.Text);
                    }
                    match = match.NextMatch();
                }

                StripableText st = new StripableText(p.Text);
                match = ReBeforeLowercaseLetter.Match(st.StrippedText);
                while (match.Success)
                {
                    string word = GetWholeWord(st.StrippedText, match.Index);
                    if (!IsName(word))
                    {
                        if (AllowFix(p, fixAction))
                        {
                            if (word.Equals("internal", StringComparison.OrdinalIgnoreCase) ||
                                word.Equals("island", StringComparison.OrdinalIgnoreCase) ||
                                word.Equals("islands", StringComparison.OrdinalIgnoreCase))
                            {
                            }
                            else if (match.Index == 0)
                            {  // first letter in paragraph

                                //too risky! - perhaps if periods is fixed at the same time... or too complicated!?
                                //if (isLineContinuation)
                                //{
                                //    st.StrippedText = st.StrippedText.Remove(match.Index, 1).Insert(match.Index, "l");
                                //    p.Text = st.MergedString;
                                //    uppercaseIsInsideLowercaseWords++;
                                //    _totalFixes++;
                                //    AddFixToListView(p, fixAction, oldText, p.Text);
                                //}
                            }
                            else
                            {
                                if (match.Index > 2 && st.StrippedText[match.Index - 1] == ' ')
                                {
                                    if ((Utilities.AllLettersAndNumbers + @",").Contains(st.StrippedText[match.Index - 2])
                                        && match.Length >= 2 && Utilities.LowercaseVowels.Contains(char.ToLower(match.Value[1])))
                                    {
                                        st.StrippedText = st.StrippedText.Remove(match.Index, 1).Insert(match.Index, "l");
                                        p.Text = st.MergedString;
                                        uppercaseIsInsideLowercaseWords++;
                                        _totalFixes++;
                                        AddFixToListView(p, fixAction, oldText, p.Text);
                                    }
                                }
                                else if (match.Index > Environment.NewLine.Length + 1 && Environment.NewLine.Contains(st.StrippedText[match.Index - 1]))
                                {
                                    if ((Utilities.AllLettersAndNumbers + @",").Contains(st.StrippedText[match.Index - Environment.NewLine.Length + 1])
                                        && match.Length >= 2 && Utilities.LowercaseVowels.Contains(match.Value[1]))
                                    {
                                        st.StrippedText = st.StrippedText.Remove(match.Index, 1).Insert(match.Index, "l");
                                        p.Text = st.MergedString;
                                        uppercaseIsInsideLowercaseWords++;
                                        _totalFixes++;
                                        AddFixToListView(p, fixAction, oldText, p.Text);
                                    }
                                }
                                else if (match.Index > 1 && ((st.StrippedText[match.Index - 1] == '\"') || (st.StrippedText[match.Index - 1] == '\'') ||
                                                             (st.StrippedText[match.Index - 1] == '>') || (st.StrippedText[match.Index - 1] == '-')))
                                {
                                }
                                else
                                {
                                    var before = '\0';
                                    var after = '\0';
                                    if (match.Index > 0)
                                        before = st.StrippedText[match.Index - 1];
                                    if (match.Index < st.StrippedText.Length - 2)
                                        after = st.StrippedText[match.Index + 1];
                                    if (before != '\0' && char.IsUpper(before) && after != '\0' && char.IsLower(after) &&
                                        !Utilities.LowercaseVowels.Contains(char.ToLower(before)) && !Utilities.LowercaseVowels.Contains(after))
                                    {
                                        st.StrippedText = st.StrippedText.Remove(match.Index, 1).Insert(match.Index, "i");
                                        p.Text = st.MergedString;
                                        uppercaseIsInsideLowercaseWords++;
                                        _totalFixes++;
                                        AddFixToListView(p, fixAction, oldText, p.Text);
                                    }
                                    else if (@"‘’¡¿„“()[]♪'. ".Contains(before) && !Utilities.LowercaseVowels.Contains(char.ToLower(after)))
                                    {
                                    }
                                    else
                                    {
                                        st.StrippedText = st.StrippedText.Remove(match.Index, 1).Insert(match.Index, "l");
                                        p.Text = st.MergedString;
                                        uppercaseIsInsideLowercaseWords++;
                                        _totalFixes++;
                                        AddFixToListView(p, fixAction, oldText, p.Text);
                                    }
                                }
                            }

                        }
                    }
                    match = match.NextMatch();
                }

                //isLineContinuation = p.Text.Length > 0 && Utilities.GetLetters(true, true, false).Contains(p.Text[p.Text.Length - 1].ToString());
            }
            if (uppercaseIsInsideLowercaseWords > 0)
                LogStatus(_language.FixUppercaseIInsindeLowercaseWords, string.Format(_language.XUppercaseIsFoundInsideLowercaseWords, uppercaseIsInsideLowercaseWords));
        }

        public void FixDoubleApostrophes()
        {
            string fixAction = _language.FixDoubleApostrophes;
            int fixCount = 0;
            for (int i = 0; i < _subtitle.Paragraphs.Count; i++)
            {
                Paragraph p = _subtitle.Paragraphs[i];

                if (p.Text.Contains("''"))
                {
                    if (AllowFix(p, fixAction))
                    {
                        string oldText = p.Text;
                        p.Text = p.Text.Replace("''", "\"");
                        fixCount++;
                        _totalFixes++;
                        AddFixToListView(p, fixAction, oldText, p.Text);
                    }
                }
            }
            if (fixCount > 0)
                LogStatus(_language.FixDoubleApostrophes, string.Format(_language.XDoubleApostrophesFixed, fixCount));
        }

        public void FixMissingPeriodsAtEndOfLine()
        {
            string fixAction = _language.FixMissingPeriodAtEndOfLine;
            int missigPeriodsAtEndOfLine = 0;
            for (int i = 0; i < _subtitle.Paragraphs.Count; i++)
            {
                Paragraph p = _subtitle.Paragraphs[i];
                Paragraph next = _subtitle.GetParagraphOrDefault(i + 1);
                string nextText = string.Empty;
                if (next != null)
                    nextText = Utilities.RemoveHtmlTags(next.Text).TrimStart('-', '"', '„').TrimStart();
                string tempNoHtml = Utilities.RemoveHtmlTags(p.Text).TrimEnd();

                if (IsOneLineUrl(p.Text) || p.Text.Contains('♪') || p.Text.Contains('♫') || p.Text.EndsWith('\''))
                {
                    // ignore urls
                }
                else if (!string.IsNullOrEmpty(nextText) && next != null &&
                    next.Text.Length > 0 &&
                    Utilities.UppercaseLetters.Contains(nextText[0]) &&
                    tempNoHtml.Length > 0 &&
                    !@",.!?:;>-])♪♫…".Contains(tempNoHtml[tempNoHtml.Length - 1]))
                {
                    string tempTrimmed = tempNoHtml.TrimEnd().TrimEnd('\'', '"', '“', '”').TrimEnd();
                    if (tempTrimmed.Length > 0 && !@")]*#¶.!?".Contains(tempTrimmed[tempTrimmed.Length - 1]))
                    {
                        if (p.Text != p.Text.ToUpper())
                        {
                            //don't end the sentence if the next word is an I word as they're always capped.
                            if (!next.Text.StartsWith("I ") && !next.Text.StartsWith("I'"))
                            {
                                //test to see if the first word of the next line is a name
                                if (!IsName(next.Text.Split(new[] { ' ', '.', ',', '-', '?', '!', ':', ';', '"', '(', ')', '[', ']', '{', '}', '|', '<', '>', '/', '+', '\r', '\n' })[0]))
                                {
                                    if (AllowFix(p, fixAction))
                                    {
                                        string oldText = p.Text;
                                        if (p.Text.EndsWith('>'))
                                        {
                                            int lastLT = p.Text.LastIndexOf('<');
                                            if (lastLT > 0)
                                                p.Text = p.Text.Insert(lastLT, ".");
                                        }
                                        else
                                        {
                                            if (p.Text.EndsWith('“') && tempNoHtml.StartsWith('„'))
                                                p.Text = p.Text.TrimEnd('“') + ".“";
                                            else if (p.Text.EndsWith('"') && tempNoHtml.StartsWith('"'))
                                                p.Text = p.Text.TrimEnd('"') + ".\"";
                                            else
                                                p.Text += ".";
                                        }
                                        if (p.Text != oldText)
                                        {
                                            _totalFixes++;
                                            missigPeriodsAtEndOfLine++;
                                            AddFixToListView(p, fixAction, oldText, p.Text);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                else if (next != null && !string.IsNullOrEmpty(p.Text) && Utilities.AllLettersAndNumbers.Contains(p.Text[p.Text.Length - 1]))
                {
                    if (p.Text != p.Text.ToUpper())
                    {
                        StripableText st = new StripableText(next.Text);
                        if (st.StrippedText.Length > 0 && st.StrippedText != st.StrippedText.ToUpper() &&
                            Utilities.UppercaseLetters.Contains(st.StrippedText[0]))
                        {
                            if (AllowFix(p, fixAction))
                            {
                                int j = p.Text.Length - 1;
                                while (j >= 0 && !@".!?¿¡".Contains(p.Text[j]))
                                    j--;
                                string endSign = ".";
                                if (j >= 0 && p.Text[j] == '¿')
                                    endSign = "?";
                                if (j >= 0 && p.Text[j] == '¡')
                                    endSign = "!";

                                string oldText = p.Text;
                                _totalFixes++;
                                missigPeriodsAtEndOfLine++;
                                p.Text += endSign;
                                AddFixToListView(p, fixAction, oldText, p.Text);
                            }
                        }
                    }
                }

                if (p.Text.Length > 4)
                {
                    int indexOfNewLine = p.Text.IndexOf(Environment.NewLine + " -", 3, StringComparison.Ordinal);
                    if (indexOfNewLine == -1)
                        indexOfNewLine = p.Text.IndexOf(Environment.NewLine + "-", 3, StringComparison.Ordinal);
                    if (indexOfNewLine == -1)
                        indexOfNewLine = p.Text.IndexOf(Environment.NewLine + "<i>-", 3, StringComparison.Ordinal);
                    if (indexOfNewLine == -1)
                        indexOfNewLine = p.Text.IndexOf(Environment.NewLine + "<i> -", 3, StringComparison.Ordinal);
                    if (indexOfNewLine > 0)
                    {
                        if (Configuration.Settings.General.UppercaseLetters.Contains(char.ToUpper(p.Text[indexOfNewLine - 1])))
                        {
                            if (AllowFix(p, fixAction))
                            {
                                string oldText = p.Text;

                                string text = p.Text.Substring(0, indexOfNewLine);
                                StripableText st = new StripableText(text);
                                if (st.Pre.TrimEnd().EndsWith('¿')) // Spanish ¿
                                    p.Text = p.Text.Insert(indexOfNewLine, "?");
                                else if (st.Pre.TrimEnd().EndsWith('¡')) // Spanish ¡
                                    p.Text = p.Text.Insert(indexOfNewLine, "!");
                                else
                                    p.Text = p.Text.Insert(indexOfNewLine, ".");

                                _totalFixes++;
                                missigPeriodsAtEndOfLine++;
                                AddFixToListView(p, fixAction, oldText, p.Text);
                            }
                        }
                    }
                }
            }

            if (missigPeriodsAtEndOfLine > 0)
                LogStatus(_language.AddPeriods, string.Format(_language.XPeriodsAdded, missigPeriodsAtEndOfLine));
        }

        private static bool IsOneLineUrl(string s)
        {
            if (s.Contains(' ') || s.Contains(Environment.NewLine))
                return false;

            if (s.StartsWith("http://", StringComparison.OrdinalIgnoreCase))
                return true;

            if (s.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                return true;

            if (s.StartsWith("www.", StringComparison.OrdinalIgnoreCase))
                return true;

            string[] parts = s.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 3 && parts[2].Length > 1 && parts[2].Length < 7)
                return true;

            return false;
        }

        private bool IsName(string candidate)
        {
            MakeSureNamesListIsLoaded();
            return _namesEtcList.Contains(candidate);
        }

        private void MakeSureNamesListIsLoaded()
        {
            if (_namesEtcList == null)
            {
                _namesEtcList = new List<string>();
                string languageTwoLetterCode = Utilities.AutoDetectGoogleLanguage(_subtitle);

                // Will contains both one word names and multi names
                Utilities.LoadNamesEtcWordLists(_namesEtcList, _namesEtcList, languageTwoLetterCode);
            }
        }

        private List<string> GetAbbreviations()
        {
            if (_abbreviationList != null)
                return _abbreviationList;

            MakeSureNamesListIsLoaded();
            _abbreviationList = new List<string>();
            foreach (string name in _namesEtcList)
            {
                if (name.EndsWith('.'))
                    _abbreviationList.Add(name);
            }
            return _abbreviationList;
        }

        public void FixStartWithUppercaseLetterAfterParagraph()
        {
            listViewFixes.BeginUpdate();
            string fixAction = _language.FixFirstLetterToUppercaseAfterParagraph;
            int fixedStartWithUppercaseLetterAfterParagraphTicked = 0;
            for (int i = 0; i < _subtitle.Paragraphs.Count; i++)
            {
                Paragraph p = _subtitle.Paragraphs[i];
                Paragraph prev = _subtitle.GetParagraphOrDefault(i - 1);

                string oldText = p.Text;
                string fixedText = FixStartWithUppercaseLetterAfterParagraph(p, prev, _encoding, Language);

                if (oldText != p.Text)
                {
                    if (AllowFix(p, fixAction))
                    {
                        _totalFixes++;
                        fixedStartWithUppercaseLetterAfterParagraphTicked++;
                        AddFixToListView(p, fixAction, oldText, p.Text);
                    }
                    else
                    {
                        p.Text = oldText;
                    }
                }
            }
            listViewFixes.EndUpdate();
            if (fixedStartWithUppercaseLetterAfterParagraphTicked > 0)
                LogStatus(_language.StartWithUppercaseLetterAfterParagraph, fixedStartWithUppercaseLetterAfterParagraphTicked.ToString());
        }

        public static string FixStartWithUppercaseLetterAfterParagraph(Paragraph p, Paragraph prev, Encoding encoding, string language)
        {
            if (p.Text != null && p.Text.Length > 1)
            {
                string text = p.Text;
                string pre = string.Empty;
                if (text.Length > 4 && text.StartsWith("<i> "))
                {
                    pre = "<i> ";
                    text = text.Substring(4);
                }
                if (text.Length > 3 && text.StartsWith("<i>"))
                {
                    pre = "<i>";
                    text = text.Substring(3);
                }
                if (text.Length > 4 && text.StartsWith("<I> "))
                {
                    pre = "<I> ";
                    text = text.Substring(4);
                }
                if (text.Length > 3 && text.StartsWith("<I>"))
                {
                    pre = "<I>";
                    text = text.Substring(3);
                }
                if (text.Length > 2 && text.StartsWith('♪'))
                {
                    pre = pre + "♪";
                    text = text.Substring(1);
                }
                if (text.Length > 2 && text.StartsWith(' '))
                {
                    pre = pre + " ";
                    text = text.Substring(1);
                }
                if (text.Length > 2 && text.StartsWith('♫'))
                {
                    pre = pre + "♫";
                    text = text.Substring(1);
                }
                if (text.Length > 2 && text.StartsWith(' '))
                {
                    pre = pre + " ";
                    text = text.Substring(1);
                }

                var firstLetter = text[0];

                string prevText = " .";
                if (prev != null)
                    prevText = Utilities.RemoveHtmlTags(prev.Text);

                bool isPrevEndOfLine = IsPrevoiusTextEndOfParagraph(prevText);
                if (prevText == " .")
                    isPrevEndOfLine = true;
                if ((!text.StartsWith("www.") && !text.StartsWith("http:") && !text.StartsWith("https:")) &&
                    (char.IsLower(firstLetter) || IsTurkishLittleI(firstLetter, encoding, language)) &&
                    !char.IsDigit(firstLetter) &&
                    isPrevEndOfLine)
                {
                    bool isMatchInKnowAbbreviations = language == "en" &&
                        (prevText.EndsWith(" o.r.", StringComparison.Ordinal) ||
                         prevText.EndsWith(" a.m.", StringComparison.Ordinal) ||
                         prevText.EndsWith(" p.m.", StringComparison.Ordinal));

                    if (!isMatchInKnowAbbreviations)
                    {
                        if (IsTurkishLittleI(firstLetter, encoding, language))
                            p.Text = pre + GetTurkishUppercaseLetter(firstLetter, encoding) + text.Substring(1);
                        else if (language == "en" && (text.StartsWith("l ") || text.StartsWith("l-I") || text.StartsWith("ls ") || text.StartsWith("lnterested") ||
                                                      text.StartsWith("lsn't ") || text.StartsWith("ldiot") || text.StartsWith("ln") || text.StartsWith("lm") ||
                                                      text.StartsWith("ls") || text.StartsWith("lt") || text.StartsWith("lf ") || text.StartsWith("lc") || text.StartsWith("l'm ")) || text.StartsWith("l am ")) // l > I
                            p.Text = pre + "I" + text.Substring(1);
                        else
                            p.Text = pre + char.ToUpper(firstLetter) + text.Substring(1);
                    }
                }
            }

            if (p.Text != null && p.Text.Contains(Environment.NewLine))
            {
                string[] arr = p.Text.Replace("\r\n", "\n").Replace("\r", "\n").Split('\n');
                if (arr.Length == 2 && arr[1].Length > 1)
                {
                    string text = arr[1];
                    string pre = string.Empty;
                    if (text.Length > 4 && text.StartsWith("<i> "))
                    {
                        pre = "<i> ";
                        text = text.Substring(4);
                    }
                    if (text.Length > 3 && text.StartsWith("<i>"))
                    {
                        pre = "<i>";
                        text = text.Substring(3);
                    }
                    if (text.Length > 4 && text.StartsWith("<I> "))
                    {
                        pre = "<I> ";
                        text = text.Substring(4);
                    }
                    if (text.Length > 3 && text.StartsWith("<I>"))
                    {
                        pre = "<I>";
                        text = text.Substring(3);
                    }
                    if (text.Length > 2 && text.StartsWith('♪'))
                    {
                        pre = pre + "♪";
                        text = text.Substring(1);
                    }
                    if (text.Length > 2 && text.StartsWith(' '))
                    {
                        pre = pre + " ";
                        text = text.Substring(1);
                    }
                    if (text.Length > 2 && text.StartsWith('♫'))
                    {
                        pre = pre + "♫";
                        text = text.Substring(1);
                    }
                    if (text.Length > 2 && text.StartsWith(' '))
                    {
                        pre = pre + " ";
                        text = text.Substring(1);
                    }

                    char firstLetter = text[0];
                    string prevText = Utilities.RemoveHtmlTags(arr[0]);
                    bool isPrevEndOfLine = IsPrevoiusTextEndOfParagraph(prevText);
                    if ((!text.StartsWith("www.") && !text.StartsWith("http:") && !text.StartsWith("https:")) &&
                        (char.IsLower(firstLetter) || IsTurkishLittleI(firstLetter, encoding, language)) &&
                        !prevText.EndsWith("...", StringComparison.Ordinal) &&
                        isPrevEndOfLine)
                    {
                        bool isMatchInKnowAbbreviations = language == "en" &&
                            (prevText.EndsWith(" o.r.", StringComparison.Ordinal) ||
                             prevText.EndsWith(" a.m.", StringComparison.Ordinal) ||
                             prevText.EndsWith(" p.m.", StringComparison.Ordinal));

                        if (!isMatchInKnowAbbreviations)
                        {
                            if (IsTurkishLittleI(firstLetter, encoding, language))
                                text = pre + GetTurkishUppercaseLetter(firstLetter, encoding) + text.Substring(1);
                            else if (language == "en" && (text.StartsWith("l ") || text.StartsWith("l-I") || text.StartsWith("ls ") || text.StartsWith("lnterested") ||
                                                     text.StartsWith("lsn't ") || text.StartsWith("ldiot") || text.StartsWith("ln") || text.StartsWith("lm") ||
                                                     text.StartsWith("ls") || text.StartsWith("lt") || text.StartsWith("lf ") || text.StartsWith("lc") || text.StartsWith("l'm ")) || text.StartsWith("l am ")) // l > I
                                text = pre + "I" + text.Substring(1);
                            else
                                text = pre + char.ToUpper(firstLetter) + text.Substring(1);
                            p.Text = arr[0] + Environment.NewLine + text;
                        }
                    }

                    arr = p.Text.Replace("\r\n", "\n").Replace("\r", "\n").Split('\n');
                    if ((arr[0].StartsWith('-') || arr[0].StartsWith("<i>-")) &&
                        (arr[1].StartsWith('-') || arr[1].StartsWith("<i>-")) &&
                        !arr[0].StartsWith("--") && !arr[0].StartsWith("<i>--") &&
                        !arr[1].StartsWith("--") && !arr[1].StartsWith("<i>--"))
                    {
                        if (isPrevEndOfLine && arr[1].StartsWith("<i>- ") && arr[1].Length > 6)
                        {
                            p.Text = arr[0] + Environment.NewLine + "<i>- " + char.ToUpper(arr[1][5]) + arr[1].Remove(0, 6);
                        }
                        else if (isPrevEndOfLine && arr[1].StartsWith("- ") && arr[1].Length > 3)
                        {
                            p.Text = arr[0] + Environment.NewLine + "- " + char.ToUpper(arr[1][2]) + arr[1].Remove(0, 3);
                        }
                        arr = p.Text.Replace("\r\n", "\n").Replace("\r", "\n").Split('\n');

                        prevText = " .";
                        if (prev != null && p.StartTime.TotalMilliseconds - 10000 < prev.EndTime.TotalMilliseconds)
                            prevText = Utilities.RemoveHtmlTags(prev.Text);
                        bool isPrevLineEndOfLine = IsPrevoiusTextEndOfParagraph(prevText);
                        if (prevText == " .")
                            isPrevEndOfLine = true;
                        if (isPrevLineEndOfLine && arr[0].StartsWith("<i>- ") && arr[0].Length > 6)
                        {
                            p.Text = "<i>- " + char.ToUpper(arr[0][5]) + arr[0].Remove(0, 6) + Environment.NewLine + arr[1];
                        }
                        else if (isPrevLineEndOfLine && arr[0].StartsWith("- ") && arr[0].Length > 3)
                        {
                            p.Text = "- " + char.ToUpper(arr[0][2]) + arr[0].Remove(0, 3) + Environment.NewLine + arr[1];
                        }

                    }
                }
            }

            if (p.Text.Length > 4)
            {
                int len = 0;
                int indexOfNewLine = p.Text.IndexOf(Environment.NewLine + " -", 1, StringComparison.Ordinal);
                if (indexOfNewLine == -1)
                {
                    indexOfNewLine = p.Text.IndexOf(Environment.NewLine + "- <i> ♪", 1, StringComparison.Ordinal);
                    len = "- <i> ♪".Length;
                }
                if (indexOfNewLine == -1)
                {
                    indexOfNewLine = p.Text.IndexOf(Environment.NewLine + "-", 1, StringComparison.Ordinal);
                    len = "-".Length;
                }
                if (indexOfNewLine == -1)
                {
                    indexOfNewLine = p.Text.IndexOf(Environment.NewLine + "<i>-", 1, StringComparison.Ordinal);
                    len = "<i>-".Length;
                }
                if (indexOfNewLine == -1)
                {
                    indexOfNewLine = p.Text.IndexOf(Environment.NewLine + "<i> -", 1, StringComparison.Ordinal);
                    len = "<i> -".Length;
                }
                if (indexOfNewLine == -1)
                {
                    indexOfNewLine = p.Text.IndexOf(Environment.NewLine + "♪ -", 1, StringComparison.Ordinal);
                    len = "♪ -".Length;
                }
                if (indexOfNewLine == -1)
                {
                    indexOfNewLine = p.Text.IndexOf(Environment.NewLine + "♪ <i> -", 1, StringComparison.Ordinal);
                    len = "♪ <i> -".Length;
                }
                if (indexOfNewLine == -1)
                {
                    indexOfNewLine = p.Text.IndexOf(Environment.NewLine + "♪ <i>-", 1, StringComparison.Ordinal);
                    len = "♪ <i>-".Length;
                }

                if (indexOfNewLine > 0)
                {
                    string text = p.Text.Substring(indexOfNewLine + len);
                    StripableText st = new StripableText(text);

                    if (st.StrippedText.Length > 0 && IsTurkishLittleI(st.StrippedText[0], encoding, language) && !st.Pre.EndsWith('[') && !st.Pre.Contains("..."))
                    {
                        text = st.Pre + GetTurkishUppercaseLetter(st.StrippedText[0], encoding) + st.StrippedText.Substring(1) + st.Post;
                        p.Text = p.Text.Remove(indexOfNewLine + len).Insert(indexOfNewLine + len, text);
                    }
                    else if (st.StrippedText.Length > 0 && st.StrippedText[0] != char.ToUpper(st.StrippedText[0]) && !st.Pre.EndsWith('[') && !st.Pre.Contains("..."))
                    {
                        text = st.Pre + char.ToUpper(st.StrippedText[0]) + st.StrippedText.Substring(1) + st.Post;
                        p.Text = p.Text.Remove(indexOfNewLine + len).Insert(indexOfNewLine + len, text);
                    }
                }
            }
            return p.Text;
        }

        private static bool IsTurkishLittleI(char firstLetter, Encoding encoding, string language)
        {
            if (language != "tr")
            {
                return false;
            }

            return encoding.Equals(Encoding.UTF8)
                ? firstLetter == 'ı' || firstLetter == 'i'
                : firstLetter == 'ý' || firstLetter == 'i';
        }

        private static char GetTurkishUppercaseLetter(char letter, Encoding encoding)
        {
            if (encoding.Equals(Encoding.UTF8))
            {
                if (letter == 'ı')
                    return 'I';
                if (letter == 'i')
                    return 'İ';
            }
            else
            {
                if (letter == 'i')
                    return 'Ý';
                if (letter == 'ý')
                    return 'I';
            }
            return letter;
        }

        private static bool IsPrevoiusTextEndOfParagraph(string prevText)
        {
            if (string.IsNullOrEmpty(prevText) || prevText.Length < 3)
                return true;

            prevText = prevText.Replace("♪", string.Empty).Replace("♫", string.Empty).Trim();
            bool isPrevEndOfLine = prevText.Length > 1 &&
                                   !prevText.EndsWith("...", StringComparison.Ordinal) &&
                                   (prevText.EndsWith('.') ||
                                    prevText.EndsWith('!') ||
                                    prevText.EndsWith('?'));

            if (isPrevEndOfLine && prevText.Length > 5 && prevText.EndsWith('.') &&
                prevText[prevText.Length - 3] == '.' &&
                Utilities.AllLetters.Contains(prevText[prevText.Length - 2]))
                isPrevEndOfLine = false;
            return isPrevEndOfLine;
        }

        private void FixStartWithUppercaseLetterAfterPeriodInsideParagraph()
        {
            string fixAction = _language.StartWithUppercaseLetterAfterPeriodInsideParagraph;
            int noOfFixes = 0;
            for (int i = 0; i < _subtitle.Paragraphs.Count; i++)
            {
                Paragraph p = _subtitle.Paragraphs[i];
                string oldText = p.Text;
                StripableText st = new StripableText(p.Text);
                if (p.Text.Length > 3)
                {
                    string text = st.StrippedText.Replace("  ", " ");
                    int start = text.IndexOfAny(new[] { '.', '!', '?' });
                    while (start != -1 && start < text.Length)
                    {
                        if (start > 0 && char.IsDigit(text[start - 1]))
                        {
                            // ignore periods after a number
                        }
                        else if (start + 4 < text.Length && text[start + 1] == ' ')
                        {
                            if (!IsAbbreviation(text, start))
                            {
                                StripableText subText = new StripableText(text.Substring(start + 2));
                                if (subText.StrippedText.Length > 0 && IsTurkishLittleI(subText.StrippedText[0], _encoding, Language))
                                {
                                    if (subText.StrippedText.Length > 1 && !(subText.Pre.Contains('\'') && subText.StrippedText.StartsWith('s')))
                                    {
                                        text = text.Substring(0, start + 2) + subText.Pre + GetTurkishUppercaseLetter(subText.StrippedText[0], _encoding) + subText.StrippedText.Substring(1) + subText.Post;
                                        if (AllowFix(p, fixAction))
                                        {
                                            p.Text = st.Pre + text + st.Post;
                                        }
                                    }
                                }
                                else if (subText.StrippedText.Length > 0 && Configuration.Settings.General.UppercaseLetters.Contains(char.ToUpper(subText.StrippedText[0])))
                                {
                                    if (subText.StrippedText.Length > 1 && !(subText.Pre.Contains('\'') && subText.StrippedText.StartsWith('s')))
                                    {
                                        text = text.Substring(0, start + 2) + subText.Pre + char.ToUpper(subText.StrippedText[0]) + subText.StrippedText.Substring(1) + subText.Post;
                                        if (AllowFix(p, fixAction))
                                        {
                                            p.Text = st.Pre + text + st.Post;
                                        }
                                    }
                                }
                            }
                        }
                        start += 4;
                        if (start < text.Length)
                            start = text.IndexOfAny(new[] { '.', '!', '?' }, start);
                    }
                }

                if (oldText != p.Text)
                {
                    noOfFixes++;
                    _totalFixes++;
                    AddFixToListView(p, fixAction, oldText, p.Text);
                }
            }
            if (noOfFixes > 0)
                LogStatus(_language.StartWithUppercaseLetterAfterPeriodInsideParagraph, noOfFixes.ToString());
        }

        private void FixStartWithUppercaseLetterAfterColon()
        {
            string fixAction = _language.StartWithUppercaseLetterAfterColon;
            int noOfFixes = 0;
            listViewFixes.BeginUpdate();
            for (int i = 0; i < _subtitle.Paragraphs.Count; i++)
            {
                Paragraph p = _subtitle.Paragraphs[i];
                Paragraph last = _subtitle.GetParagraphOrDefault(i - 1);
                string oldText = p.Text;
                int skipCount = 0;

                if (last != null)
                {
                    string lastText = Utilities.RemoveHtmlTags(last.Text);
                    if (lastText.EndsWith(':') || lastText.EndsWith(';'))
                    {
                        var st = new StripableText(p.Text);
                        if (st.StrippedText.Length > 0 && st.StrippedText[0] != char.ToUpper(st.StrippedText[0]))
                            p.Text = st.Pre + char.ToUpper(st.StrippedText[0]) + st.StrippedText.Substring(1) + st.Post;
                    }
                }

                if (oldText.Contains(':') || oldText.Contains(';'))
                {
                    bool lastWasColon = false;
                    for (int j = 0; j < p.Text.Length; j++)
                    {
                        var s = p.Text[j];
                        if (s == ':' || s == ';')
                        {
                            lastWasColon = true;
                        }
                        else if (lastWasColon)
                        {
                            if (skipCount > 0)
                                skipCount--;
                            else if (p.Text.Substring(j).StartsWith("<i>"))
                                skipCount = 2;
                            else if (p.Text.Substring(j).StartsWith("<b>"))
                                skipCount = 2;
                            else if (p.Text.Substring(j).StartsWith("<u>"))
                                skipCount = 2;
                            else if (p.Text.Substring(j).StartsWith("<font ") && p.Text.Substring(j).Contains('>'))
                                skipCount = p.Text.Substring(j).IndexOf('>') - p.Text.Substring(j).IndexOf("<font ", StringComparison.Ordinal);
                            else if (IsTurkishLittleI(s, _encoding, Language))
                            {
                                p.Text = p.Text.Remove(j, 1).Insert(j, GetTurkishUppercaseLetter(s, _encoding).ToString());
                                lastWasColon = false;
                            }
                            else if (char.IsLower(s))
                            {
                                p.Text = p.Text.Remove(j, 1).Insert(j, char.ToUpper(s).ToString());
                                lastWasColon = false;
                            }
                            else if (!(" " + Environment.NewLine).Contains(s))
                                lastWasColon = false;
                        }
                    }
                }

                if (oldText != p.Text)
                {
                    noOfFixes++;
                    _totalFixes++;
                    AddFixToListView(p, fixAction, oldText, p.Text);
                }
            }
            listViewFixes.EndUpdate();
            if (noOfFixes > 0)
                LogStatus(_language.StartWithUppercaseLetterAfterColon, noOfFixes.ToString());
        }

        private bool IsAbbreviation(string text, int index)
        {
            if (text[index] != '.' && text[index] != '!' && text[index] != '?')
                return false;

            if (index - 3 > 0 && Utilities.AllLettersAndNumbers.Contains(text[index - 1]) && text[index - 2] == '.') // e.g: O.R.
                return true;

            string word = string.Empty;
            int i = index - 1;
            while (i >= 0 && Utilities.AllLetters.Contains(text[i]))
            {
                word = text[i] + word;
                i--;
            }

            List<string> abbreviations = GetAbbreviations();
            return abbreviations.Contains(word + ".");
        }

        public void FixOcrErrorsViaReplaceList(string threeLetterISOLanguageName)
        {
            var ocrFixEngine = new OcrFixEngine(threeLetterISOLanguageName, null, this);
            string fixAction = _language.FixCommonOcrErrors;
            int noOfFixes = 0;
            string lastLine = string.Empty;
            for (int i = 0; i < _subtitle.Paragraphs.Count; i++)
            {
                var p = _subtitle.Paragraphs[i];
                string text = ocrFixEngine.FixOcrErrors(p.Text, i, lastLine, false, OcrFixEngine.AutoGuessLevel.Cautious);
                lastLine = text;
                if (p.Text != text)
                {
                    if (AllowFix(p, fixAction))
                    {
                        string oldText = p.Text;
                        p.Text = text;
                        noOfFixes++;
                        _totalFixes++;
                        AddFixToListView(p, fixAction, oldText, p.Text);
                    }
                    Application.DoEvents();
                }
            }
            if (noOfFixes > 0)
                LogStatus(_language.FixCommonOcrErrors, string.Format(_language.CommonOcrErrorsFixed, noOfFixes));
        }

        private void RemoveSpaceBetweenNumbers()
        {
            string fixAction = _language.RemoveSpaceBetweenNumber;
            int noOfFixes = 0;
            for (int i = 0; i < _subtitle.Paragraphs.Count; i++)
            {
                Paragraph p = _subtitle.Paragraphs[i];
                string text = p.Text;
                Match match = RemoveSpaceBetweenNumbersRegEx.Match(text);
                int counter = 0;
                while (match.Success && counter < 100 && text.Length > match.Index + 1)
                {
                    string temp = text.Substring(match.Index + 2);
                    if (temp != "1/2" &&
                        !temp.StartsWith("1/2 ") &&
                        !temp.StartsWith("1/2.") &&
                        !temp.StartsWith("1/2!") &&
                        !temp.StartsWith("1/2?") &&
                        !temp.StartsWith("1/2<"))
                    {
                        text = text.Remove(match.Index + 1, 1);
                    }
                    if (text.Length > match.Index + 1)
                        match = RemoveSpaceBetweenNumbersRegEx.Match(text, match.Index + 2);
                    counter++;
                }
                if (p.Text != text)
                {
                    if (AllowFix(p, fixAction))
                    {
                        string oldText = p.Text;
                        p.Text = text;
                        noOfFixes++;
                        _totalFixes++;
                        AddFixToListView(p, fixAction, oldText, p.Text);
                    }
                }
            }
            if (noOfFixes > 0)
                LogStatus(_language.FixCommonOcrErrors, string.Format(_language.RemoveSpaceBetweenNumbersFixed, noOfFixes));
        }

        private void DialogsOnOneLine()
        {
            string fixAction = _language.FixDialogsOnOneLine;
            int noOfFixes = 0;
            for (int i = 0; i < _subtitle.Paragraphs.Count; i++)
            {
                Paragraph p = _subtitle.Paragraphs[i];
                string text = p.Text;
                string oldText = text;
                if (text.Contains(" - ") && !text.Contains(Environment.NewLine))
                {
                    string[] parts = text.Replace(" - ", Environment.NewLine).Split(Utilities.NewLineChars, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length == 2)
                    {
                        string part0 = Utilities.RemoveHtmlTags(parts[0]).Trim();
                        string part1 = Utilities.RemoveHtmlTags(parts[1]).Trim();
                        if (part0.Length > 1 && @"!?.".Contains(part0[part0.Length - 1]) &&
                            part1.Length > 1 && ("'" + Utilities.UppercaseLetters).Contains(part1[0]))
                        {
                            text = text.Replace(" - ", Environment.NewLine + "- ");
                            if (Utilities.AllLettersAndNumbers.Contains(part0[0]))
                            {
                                if (text.StartsWith("<i>"))
                                    text = "<i>- " + text;
                                else
                                    text = "- " + text;
                            }
                        }
                    }
                }

                if ((text.Contains(". -") || text.Contains("! -") || text.Contains("? -")) && Utilities.CountTagInText(text, Environment.NewLine) == 1)
                {
                    string temp = Utilities.AutoBreakLine(text, Language);
                    var arr = text.Split(Utilities.NewLineChars, StringSplitOptions.RemoveEmptyEntries);
                    var arrTemp = temp.Split(Utilities.NewLineChars, StringSplitOptions.RemoveEmptyEntries);
                    if (arr.Length == 2 && arrTemp.Length == 2 && !arr[1].TrimStart().StartsWith('-') && arrTemp[1].TrimStart().StartsWith('-'))
                        text = temp;
                    else if (arr.Length == 2 && arrTemp.Length == 2 && !arr[1].TrimStart().StartsWith("<i>-") && arrTemp[1].TrimStart().StartsWith("<i>-"))
                        text = temp;
                }
                else if ((text.Contains(". -") || text.Contains("! -") || text.Contains("? -")) && !text.Contains(Environment.NewLine))
                {
                    string temp = Utilities.AutoBreakLine(text, Language);
                    var arrTemp = temp.Split(Utilities.NewLineChars, StringSplitOptions.RemoveEmptyEntries);
                    if (arrTemp.Length == 2)
                    {
                        if (arrTemp[1].TrimStart().StartsWith('-') || arrTemp[1].TrimStart().StartsWith("<i>-"))
                            text = temp;
                    }
                    else
                    {
                        int index = text.IndexOf(". -", StringComparison.Ordinal);
                        if (index < 0)
                            index = text.IndexOf("! -", StringComparison.Ordinal);
                        if (index < 0)
                            index = text.IndexOf("? -", StringComparison.Ordinal);
                        if (index > 0)
                        {
                            text = text.Remove(index + 1, 1).Insert(index + 1, Environment.NewLine);
                            text = text.Replace(Environment.NewLine + " ", Environment.NewLine);
                            text = text.Replace(" " + Environment.NewLine, Environment.NewLine);
                        }
                    }
                }

                if (oldText != text)
                {
                    if (AllowFix(p, fixAction))
                    {
                        p.Text = text;
                        noOfFixes++;
                        _totalFixes++;
                        AddFixToListView(p, fixAction, oldText, p.Text);
                    }
                }

            }
            if (noOfFixes > 0)
                LogStatus(_language.FixCommonOcrErrors, string.Format(_language.RemoveSpaceBetweenNumbersFixed, noOfFixes));
        }

        private void TurkishAnsiToUnicode()
        {
            string fixAction = _language.FixTurkishAnsi;
            int noOfFixes = 0;
            for (int i = 0; i < _subtitle.Paragraphs.Count; i++)
            {
                Paragraph p = _subtitle.Paragraphs[i];
                string text = p.Text;
                string oldText = text;
                text = text.Replace("Ý", "İ");
                text = text.Replace("Ð", "Ğ");
                text = text.Replace("Þ", "Ş");
                text = text.Replace("ý", "ı");
                text = text.Replace("ð", "ğ");
                text = text.Replace("þ", "ş");
                if (oldText != text)
                {
                    if (AllowFix(p, fixAction))
                    {
                        p.Text = text;
                        noOfFixes++;
                        _totalFixes++;
                        AddFixToListView(p, fixAction, oldText, p.Text);
                    }
                }
            }
            if (noOfFixes > 0)
                LogStatus(_language.FixCommonOcrErrors, string.Format(_language.FixTurkishAnsi, noOfFixes));
        }

        private void FixAloneLowercaseIToUppercaseI()
        {
            string fixAction = _language.FixLowercaseIToUppercaseI;
            int iFixes = 0;
            for (int i = 0; i < _subtitle.Paragraphs.Count; i++)
            {
                Paragraph p = _subtitle.Paragraphs[i];

                string oldText = p.Text;
                string s = p.Text;
                if (s.Contains('i'))
                {
                    s = FixAloneLowercaseIToUppercaseLine(FixAloneLowercaseIToUppercaseIRE, oldText, s, 'i');

                    if (s != oldText && AllowFix(p, fixAction))
                    {
                        p.Text = s;
                        iFixes++;
                        _totalFixes++;
                        AddFixToListView(p, fixAction, oldText, p.Text);
                    }

                }
            }
            if (iFixes > 0)
                LogStatus(_language.FixLowercaseIToUppercaseI, string.Format(_language.XIsChangedToUppercase, iFixes));
        }

        public static string FixAloneLowercaseIToUppercaseLine(Regex re, string oldText, string s, char target)
        {
            //html tags
            if (s.Contains(">" + target + "</"))
                s = s.Replace(">" + target + "</", ">I</");
            if (s.Contains(">" + target + " "))
                s = s.Replace(">" + target + " ", ">I ");
            if (s.Contains(">" + target + "" + Environment.NewLine))
                s = s.Replace(">" + target + "" + Environment.NewLine, ">I" + Environment.NewLine);

            // reg-ex
            Match match = re.Match(s);
            if (match.Success)
            {
                while (match.Success)
                {
                    if (s[match.Index] == target)
                    {
                        var prev = '\0';
                        var next = '\0';
                        if (match.Index > 0)
                            prev = s[match.Index - 1];
                        if (match.Index + 1 < s.Length)
                            next = s[match.Index + 1];

                        string wholePrev = string.Empty;
                        if (match.Index > 1)
                            wholePrev = s.Substring(0, match.Index - 1);

                        if (prev != '>' && next != '>' && next != '}' && !wholePrev.TrimEnd().EndsWith("...", StringComparison.Ordinal))
                        {
                            bool fix = true;

                            if (prev == '.' || prev == '\'')
                                fix = false;

                            if (prev == ' ' && next == '.')
                                fix = false;

                            if (prev == '-' && match.Index > 2)
                                fix = false;

                            if (fix && next == '-' && match.Index < s.Length - 5 && s[match.Index + 2] == 'l' && !(Environment.NewLine + @" <>!.?:;,").Contains(s[match.Index + 3]))
                                fix = false;

                            if (fix)
                            {
                                string temp = s.Substring(0, match.Index) + "I";
                                if (match.Index + 1 < oldText.Length)
                                    temp += s.Substring(match.Index + 1);
                                s = temp;
                            }
                        }
                    }
                    match = match.NextMatch();
                }
            }
            return s;
        }

        public void FixHyphens()
        {
            string fixAction = _language.FixHyphen;
            int iFixes = 0;
            for (int i = 0; i < _subtitle.Paragraphs.Count; i++)
            {
                Paragraph p = _subtitle.Paragraphs[i];
                string text = p.Text;

                if (text.TrimStart().StartsWith('-') ||
                    text.TrimStart().StartsWith("<i>-") ||
                    text.TrimStart().StartsWith("<i> -") ||
                    text.TrimStart().StartsWith("<I>-") ||
                    text.TrimStart().StartsWith("<I> -") ||
                    text.Contains(Environment.NewLine + '-') ||
                    text.Contains(Environment.NewLine + " -") ||
                    text.Contains(Environment.NewLine + "<i>-") ||
                    text.Contains(Environment.NewLine + "<i> -") ||
                    text.Contains(Environment.NewLine + "<I>-") ||
                    text.Contains(Environment.NewLine + "<I> -"))
                {
                    Paragraph prev = _subtitle.GetParagraphOrDefault(i - 1);

                    if (prev == null || !Utilities.RemoveHtmlTags(prev.Text).TrimEnd().EndsWith('-'))
                    {
                        var lines = Utilities.RemoveHtmlTags(p.Text).Split(Utilities.NewLineChars, StringSplitOptions.RemoveEmptyEntries);
                        int startHyphenCount = 0;
                        foreach (string line in lines)
                        {
                            if (line.TrimStart().StartsWith('-'))
                                startHyphenCount++;
                        }
                        if (startHyphenCount == 1)
                        {
                            string oldText = p.Text;

                            bool remove = true;

                            string[] parts = Utilities.RemoveHtmlTags(text).Split(Utilities.NewLineChars, StringSplitOptions.RemoveEmptyEntries);
                            if (parts.Length == 2)
                            {
                                if (parts[0].TrimStart().StartsWith('-') && parts[1].Contains(": "))
                                    remove = false;
                                if (parts[1].TrimStart().StartsWith('-') && parts[0].Contains(": "))
                                    remove = false;
                            }

                            if (remove)
                            {
                                int idx = text.IndexOf('-');
                                var st = new StripableText(text);
                                if (idx < 5 && st.Pre.Length >= idx)
                                {
                                    text = text.Remove(idx, 1).TrimStart();
                                    idx = text.IndexOf('-');
                                    st = new StripableText(text);
                                    if (idx < 5 && idx >= 0 && st.Pre.Length >= idx)
                                    {
                                        text = text.Remove(idx, 1).TrimStart();
                                        st = new StripableText(text);
                                    }
                                    idx = text.IndexOf('-');
                                    if (idx < 5 && idx >= 0 && st.Pre.Length >= idx)
                                        text = text.Remove(idx, 1).TrimStart();

                                    text = text.Replace("  ", " ");
                                    text = text.Replace("> ", ">");
                                    text = text.Replace(" <", "<");
                                }
                                else
                                {
                                    int idxNL = text.IndexOf(Environment.NewLine, StringComparison.Ordinal);
                                    if (idxNL > 0)
                                    {
                                        idx = text.IndexOf('-', idxNL);
                                        if (idx >= 0 && idxNL + 5 > idxNL)
                                        {
                                            text = text.Remove(idx, 1).TrimStart().Replace(Environment.NewLine + " ", Environment.NewLine);

                                            idx = text.IndexOf('-', idxNL);
                                            if (idx >= 0 && idxNL + 5 > idxNL)
                                            {
                                                text = text.Remove(idx, 1).TrimStart();

                                                text = text.Replace("  ", " ");
                                                text = text.Replace(Environment.NewLine + " ", Environment.NewLine);
                                                text = text.Replace("> ", ">");
                                                text = text.Replace(" <", "<");
                                            }
                                        }
                                    }
                                }

                                if (text != oldText)
                                {
                                    if (AllowFix(p, fixAction))
                                    {
                                        p.Text = text;
                                        iFixes++;
                                        _totalFixes++;
                                        AddFixToListView(p, fixAction, oldText, p.Text);
                                    }
                                }
                            }
                        }
                    }
                }
                else if (text.StartsWith("<font ", StringComparison.Ordinal))
                {
                    Paragraph prev = _subtitle.GetParagraphOrDefault(i - 1);
                    if (prev == null || !Utilities.RemoveHtmlTags(prev.Text).TrimEnd().EndsWith('-'))
                    {
                        string oldText = p.Text;
                        var st = new StripableText(text);
                        if (st.Pre.EndsWith('-') || st.Pre.EndsWith("- ", StringComparison.Ordinal))
                        {
                            text = st.Pre.TrimEnd().TrimEnd('-').TrimEnd() + st.StrippedText + st.Post;
                        }
                        if (text != oldText)
                        {
                            if (AllowFix(p, fixAction))
                            {
                                p.Text = text;
                                iFixes++;
                                _totalFixes++;
                                AddFixToListView(p, fixAction, oldText, p.Text);
                            }
                        }
                    }
                }
            }
            if (iFixes > 0)
                LogStatus(_language.FixHyphens, string.Format(_language.XHyphensFixed, iFixes));
        }

        public void FixHyphensAdd()
        {
            string fixAction = _language.FixHyphen;
            int iFixes = 0;
            for (int i = 0; i < _subtitle.Paragraphs.Count; i++)
            {
                Paragraph p = _subtitle.Paragraphs[i];
                string text = p.Text;

                if (text.TrimStart().StartsWith('-') ||
                    text.TrimStart().StartsWith("<i>-") ||
                    text.TrimStart().StartsWith("<i> -") ||
                    text.TrimStart().StartsWith("<I>-") ||
                    text.TrimStart().StartsWith("<I> -") ||
                    text.Contains(Environment.NewLine + "-") ||
                    text.Contains(Environment.NewLine + " -") ||
                    text.Contains(Environment.NewLine + "<i>-") ||
                    text.Contains(Environment.NewLine + "<i> -") ||
                    text.Contains(Environment.NewLine + "<I>-") ||
                    text.Contains(Environment.NewLine + "<I> -"))
                {
                    Paragraph prev = _subtitle.GetParagraphOrDefault(i - 1);

                    if (prev == null || !Utilities.RemoveHtmlTags(prev.Text).TrimEnd().EndsWith('-'))
                    {
                        var lines = Utilities.RemoveHtmlTags(p.Text).Split(Utilities.NewLineChars, StringSplitOptions.RemoveEmptyEntries);
                        int startHyphenCount = 0;
                        foreach (string line in lines)
                        {
                            if (line.TrimStart().StartsWith('-'))
                                startHyphenCount++;
                        }
                        if (startHyphenCount == 1)
                        {
                            string oldText = p.Text;

                            string[] parts = Utilities.RemoveHtmlTags(text).Split(Utilities.NewLineChars, StringSplitOptions.RemoveEmptyEntries);
                            if (parts.Length == 2)
                            {
                                bool doAdd = parts[0].TrimEnd().EndsWith('.') || parts[0].TrimEnd().EndsWith('!') || parts[0].TrimEnd().EndsWith('?') || Language == "ko";

                                if (parts[0].TrimStart().StartsWith('-') && parts[1].Contains(':'))
                                    doAdd = false;
                                if (parts[1].TrimStart().StartsWith('-') && parts[0].Contains(':'))
                                    doAdd = false;

                                if (doAdd)
                                {
                                    int idx = text.IndexOf('-');
                                    bool addFirstLine = idx < 5;
                                    if (addFirstLine && idx > 0 && Utilities.AllLetters.Contains(text[idx - 1]))
                                        addFirstLine = false;
                                    if (addFirstLine)
                                    {
                                        // add dash in second line.
                                        if (text.Contains(Environment.NewLine + "<i>"))
                                            text = text.Replace(Environment.NewLine + "<i>", Environment.NewLine + "<i>- ");
                                        else
                                            text = text.Replace(Environment.NewLine, Environment.NewLine + "- ").Replace(Environment.NewLine + "-  ", Environment.NewLine + "- ");
                                    }
                                    else
                                    {
                                        // add dash in first line.
                                        if (text.StartsWith("<i>"))
                                            text = "<i>- " + text.Remove(0, 3).Trim();
                                        else if (text.StartsWith("{\\an") && text.Length > 6 && text[5] == '}')
                                            text = text.Insert(6, "- ");
                                        else
                                            text = "- " + text.Trim();
                                    }

                                    if (text != oldText)
                                    {
                                        if (AllowFix(p, fixAction))
                                        {
                                            p.Text = text;
                                            iFixes++;
                                            _totalFixes++;
                                            AddFixToListView(p, fixAction, oldText, p.Text);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

            }
            if (iFixes > 0)
                LogStatus(_language.FixHyphens, string.Format(_language.XHyphensFixed, iFixes));
        }

        private void Fix3PlusLines()
        {
            string fixAction = _language.Fix3PlusLine;
            int iFixes = 0;
            for (int i = 0; i < _subtitle.Paragraphs.Count; i++)
            {
                Paragraph p = _subtitle.Paragraphs[i];
                string text = p.Text;

                if (Utilities.CountTagInText(text, Environment.NewLine) > 1)
                {
                    string oldText = p.Text;
                    text = Utilities.AutoBreakLine(text);

                    if (AllowFix(p, fixAction))
                    {
                        p.Text = text;
                        iFixes++;
                        _totalFixes++;
                        AddFixToListView(p, fixAction, oldText, p.Text);
                    }
                }
            }
            if (iFixes > 0)
                LogStatus(_language.Fix3PlusLines, string.Format(_language.X3PlusLinesFixed, iFixes));
        }

        public void FixMusicNotation()
        {
            string fixAction = _language.FixMusicNotation;
            int fixCount = 0;
            for (int i = 0; i < _subtitle.Paragraphs.Count; i++)
            {
                Paragraph p = _subtitle.Paragraphs[i];

                string[] musicSymbols = Configuration.Settings.Tools.MusicSymbolToReplace.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                string oldText = p.Text;

                string newText = p.Text;
                string noTagsText = Utilities.RemoveHtmlTags(newText);
                foreach (string musicSymbol in musicSymbols)
                {
                    newText = newText.Replace(musicSymbol, Configuration.Settings.Tools.MusicSymbol);
                    newText = newText.Replace(musicSymbol.ToUpper(), Configuration.Settings.Tools.MusicSymbol);
                    noTagsText = noTagsText.Replace(musicSymbol, Configuration.Settings.Tools.MusicSymbol);
                    noTagsText = noTagsText.Replace(musicSymbol.ToUpper(), Configuration.Settings.Tools.MusicSymbol);
                }

                if (!newText.Equals(oldText))
                {
                    if (!noTagsText.Equals(Utilities.RemoveHtmlTags(oldText)))
                    {
                        if (AllowFix(p, fixAction))
                        {
                            p.Text = newText;
                            fixCount++;
                            _totalFixes++;
                            AddFixToListView(p, fixAction, oldText, p.Text);
                        }
                    }
                }
            }
            if (fixCount > 0)
                LogStatus(_language.FixMusicNotation, string.Format(_language.XFixMusicNotation, fixCount));
        }

        public void FixDoubleDash()
        {
            string fixAction = _language.FixDoubleDash;
            int fixCount = 0;
            for (int i = 0; i < _subtitle.Paragraphs.Count; i++)
            {
                Paragraph p = _subtitle.Paragraphs[i];
                string text = p.Text;
                string oldText = p.Text;

                while (text.Contains("---"))
                {
                    text = text.Replace("---", "--");
                }

                if (text.Contains("--"))
                {
                    text = text.Replace("--", "... ");
                    text = text.Replace("...  ", "... ");
                    text = text.Replace(" ...", "...");
                    text = text.TrimEnd();
                    text = text.Replace("... " + Environment.NewLine, "..." + Environment.NewLine);
                    text = text.Replace("... </i>", "...</i>");
                    text = text.Replace("... ?", "...?");
                    text = text.Replace("... !", "...!");
                    if (text.StartsWith("... "))
                        text = text.Remove(3, 1);
                    if (text.StartsWith("<i>... "))
                        text = text.Remove(6, 1);
                }
                //if (text.EndsWith('-'))
                //{
                //    text = text.Substring(0, text.Length - 1) + "...";
                //    text = text.Replace(" ...", "...");
                //}
                //if (text.EndsWith("-</i>"))
                //{
                //    text = text.Replace("-</i>", "...</i>");
                //    text = text.Replace(" ...", "...");
                //}

                if (p.Text.StartsWith('—'))
                {
                    text = text.Remove(0, 1);
                    text = text.Insert(0, "...");
                }
                if (p.Text.EndsWith('—'))
                {
                    text = text.Substring(0, text.Length - 1) + "...";
                    text = text.Replace(" ...", "...");
                }

                if (text != oldText && AllowFix(p, fixAction))
                {
                    p.Text = text;
                    fixCount++;
                    _totalFixes++;
                    AddFixToListView(p, fixAction, oldText, p.Text);
                }

            }
            if (fixCount > 0)
                LogStatus(_language.FixDoubleDash, string.Format(_language.XFixDoubleDash, fixCount));
        }

        public void FixDoubleGreaterThan()
        {
            string fixAction = _language.FixDoubleGreaterThan;
            int fixCount = 0;
            for (int i = 0; i < _subtitle.Paragraphs.Count; i++)
            {
                Paragraph p = _subtitle.Paragraphs[i];

                if (p.Text.StartsWith(">> "))
                {
                    if (AllowFix(p, fixAction))
                    {
                        string oldText = p.Text;
                        p.Text = p.Text.Substring(3, p.Text.Length - 3);
                        fixCount++;
                        _totalFixes++;
                        AddFixToListView(p, fixAction, oldText, p.Text);
                    }
                }
                if (p.Text.StartsWith(">>"))
                {
                    if (AllowFix(p, fixAction))
                    {
                        string oldText = p.Text;
                        p.Text = p.Text.Substring(2, p.Text.Length - 2);
                        fixCount++;
                        _totalFixes++;
                        AddFixToListView(p, fixAction, oldText, p.Text);
                    }
                }
            }
            if (fixCount > 0)
                LogStatus(_language.FixDoubleGreaterThan, string.Format(_language.XFixDoubleGreaterThan, fixCount));
        }

        public void FixEllipsesStart()
        {
            string fixAction = _language.FixEllipsesStart;
            int fixCount = 0;
            listViewFixes.BeginUpdate();
            for (int i = 0; i < _subtitle.Paragraphs.Count; i++)
            {
                Paragraph p = _subtitle.Paragraphs[i];
                var text = p.Text;
                if (text.Contains("..") && AllowFix(p, fixAction))
                {
                    var oldText = text;
                    if (!text.Contains(Environment.NewLine))
                    {
                        text = FixCommonErrorsHelper.FixEllipsesStartHelper(text);
                        if (oldText != text)
                        {
                            p.Text = text;
                            fixCount++;
                            _totalErrors++;
                            AddFixToListView(p, fixAction, oldText, text);
                        }
                    }
                    else
                    {
                        var lines = text.Split(Utilities.NewLineChars, StringSplitOptions.RemoveEmptyEntries);
                        var fixedParagraph = string.Empty;
                        for (int k = 0; k < lines.Length; k++)
                        {
                            var line = lines[k];
                            fixedParagraph += Environment.NewLine + FixCommonErrorsHelper.FixEllipsesStartHelper(line);
                            fixedParagraph = fixedParagraph.Trim();
                        }

                        if (fixedParagraph != text)
                        {
                            p.Text = fixedParagraph;
                            fixCount++;
                            _totalErrors++;
                            AddFixToListView(p, fixAction, oldText, fixedParagraph);
                        }
                    }
                }
            }
            listViewFixes.EndUpdate();
            listViewFixes.Refresh();
            if (fixCount > 0)
                LogStatus(_language.FixEllipsesStart, string.Format(_language.XFixEllipsesStart, fixCount));
        }

        private static string FixMissingOpenBracket(string text, string openB)
        {
            string pre = string.Empty;
            string closeB = openB == "(" ? ")" : "]";

            if (text.Contains(" " + closeB))
                openB = openB + " ";

            do
            {
                if (text.Length > 1 && text.StartsWith('-'))
                {
                    pre += "- ";
                    if (text[1] == ' ')
                        text = text.Substring(2);
                    else
                        text = text.Substring(1);
                }
                if (text.Length > 3 && text.StartsWith("<i>"))
                {
                    pre += "<i>";
                    if (text[3] == ' ')
                        text = text.Substring(4);
                    else
                        text = text.Substring(3);
                }
                if (text.Length > 1 && (text[0] == ' ' || text[0] == '.'))
                {
                    pre += text[0] == '.' ? '.' : ' ';
                    text = text.Substring(1);
                    while (text.Length > 0 && text[0] == '.')
                    {
                        pre += ".";
                        text = text.Substring(1);
                    }
                    text = text.TrimStart(' ');
                }
            } while (text.StartsWith("<i>") || text.StartsWith('-'));

            text = pre + openB + text;
            return text;
        }

        public void FixMissingOpenBracket()
        {
            string fixAction = _language.FixMissingOpenBracket;
            int fixCount = 0;
            for (int i = 0; i < _subtitle.Paragraphs.Count; i++)
            {
                Paragraph p = _subtitle.Paragraphs[i];

                if (AllowFix(p, fixAction))
                {
                    var hit = false;
                    string oldText = p.Text;
                    var openIdx = p.Text.IndexOf('(');
                    var closeIdx = p.Text.IndexOf(')');
                    if ((closeIdx > -1 && closeIdx < openIdx) || (closeIdx > -1 && openIdx == -1))
                    {
                        p.Text = FixMissingOpenBracket(p.Text, "(");
                        hit = true;
                    }

                    openIdx = p.Text.IndexOf('[');
                    closeIdx = p.Text.IndexOf(']');
                    if ((closeIdx > -1 && closeIdx < openIdx) || (closeIdx > -1 && openIdx == -1))
                    {
                        p.Text = FixMissingOpenBracket(p.Text, "[");
                        hit = true;
                    }

                    if (hit)
                    {
                        fixCount++;
                        _totalFixes++;
                        AddFixToListView(p, fixAction, oldText, p.Text);
                    }
                }
            }

            if (fixCount > 0)
                LogStatus(_language.FixMissingOpenBracket, string.Format(_language.XFixMissingOpenBracket, fixCount));
        }

        private static Regex MyRegEx(string inputRegex)
        {
            return new Regex(inputRegex.Replace(" ", "[ \r\n]+"), RegexOptions.Compiled);
        }

        private void FixDanishLetterI()
        {
            const string fixAction = "Fix danish letter 'i'";
            int fixCount = 0;

            var littleIRegex = new Regex(@"\bi\b", RegexOptions.Compiled);

            var iList = new List<Regex>
                             { // not a complete list, more phrases will come
                                 MyRegEx(@", i ved nok\b"),
                                 MyRegEx(@", i ved, "),
                                 MyRegEx(@", i ved."),
                                 MyRegEx(@", i ikke blev\b"),
                                 MyRegEx(@"\b i føler at\b"),
                                 MyRegEx(@"\badvarede i os\b"),
                                 MyRegEx(@"\badvarede i dem\b"),
                                 MyRegEx(@"\bat i aldrig\b"),
                                 MyRegEx(@"\bat i alle bliver\b"),
                                 MyRegEx(@"\bat i alle er\b"),
                                 MyRegEx(@"\bat i alle forventer\b"),
                                 MyRegEx(@"\bat i alle gør\b"),
                                 MyRegEx(@"\bat i alle har\b"),
                                 MyRegEx(@"\bat i alle ved\b"),
                                 MyRegEx(@"\bat i alle vil\b"),
                                 MyRegEx(@"\bat i bare\b"),
                                 MyRegEx(@"\bat i bager\b"),
                                 MyRegEx(@"\bat i bruger\b"),
                                 MyRegEx(@"\bat i dræber\b"),
                                 MyRegEx(@"\bat i dræbte\b"),
                                 MyRegEx(@"\bat i fandt\b"),
                                 MyRegEx(@"\bat i fik\b"),
                                 MyRegEx(@"\bat i finder\b"),
                                 MyRegEx(@"\bat i forstår\b"),
                                 MyRegEx(@"\bat i får\b"),
                                 MyRegEx(@"\b[Aa]t i hver især\b"),
                                 MyRegEx(@"\bAt i ikke\b"),
                                 MyRegEx(@"\bat i ikke\b"),
                                 MyRegEx(@"\bat i kom\b"),
                                 MyRegEx(@"\bat i kommer\b"),
                                 MyRegEx(@"\bat i næsten er\b"),
                                 MyRegEx(@"\bat i næsten fik\b"),
                                 MyRegEx(@"\bat i næsten har\b"),
                                 MyRegEx(@"\bat i næsten skulle\b"),
                                 MyRegEx(@"\bat i næsten var\b"),
                                 MyRegEx(@"\bat i også får\b"),
                                 MyRegEx(@"\bat i også gør\b"),
                                 MyRegEx(@"\bat i også mener\b"),
                                 MyRegEx(@"\bat i også siger\b"),
                                 MyRegEx(@"\bat i også tror\b"),
                                 MyRegEx(@"\bat i rev\b"),
                                 MyRegEx(@"\bat i river\b"),
                                 MyRegEx(@"\bat i samarbejder\b"),
                                 MyRegEx(@"\bat i snakkede\b"),
                                 MyRegEx(@"\bat i scorer\b"),
                                 MyRegEx(@"\bat i siger\b"),
                                 MyRegEx(@"\bat i skal\b"),
                                 MyRegEx(@"\bat i skulle\b"),
                                 MyRegEx(@"\bat i to ikke\b"),
                                 MyRegEx(@"\bat i to siger\b"),
                                 MyRegEx(@"\bat i to har\b"),
                                 MyRegEx(@"\bat i to er\b"),
                                 MyRegEx(@"\bat i to bager\b"),
                                 MyRegEx(@"\bat i to skal\b"),
                                 MyRegEx(@"\bat i to gør\b"),
                                 MyRegEx(@"\bat i to får\b"),
                                 MyRegEx(@"\bat i udnyttede\b"),
                                 MyRegEx(@"\bat i udnytter\b"),
                                 MyRegEx(@"\bat i vil\b"),
                                 MyRegEx(@"\bat i ville\b"),
                                 MyRegEx(@"\bBehandler i mig\b"),
                                 MyRegEx(@"\bbehandler i mig\b"),
                                 MyRegEx(@"\bbliver i rige\b"),
                                 MyRegEx(@"\bbliver i ikke\b"),
                                 MyRegEx(@"\bbliver i indkvarteret\b"),
                                 MyRegEx(@"\bbliver i indlogeret\b"),
                                 MyRegEx(@"\bburde i gøre\b"),
                                 MyRegEx(@"\bburde i ikke\b"),
                                 MyRegEx(@"\bburde i købe\b"),
                                 MyRegEx(@"\bburde i løbe\b"),
                                 MyRegEx(@"\bburde i se\b"),
                                 MyRegEx(@"\bburde i sige\b"),
                                 MyRegEx(@"\bburde i tage\b"),
                                 MyRegEx(@"\bDa i ankom\b"),
                                 MyRegEx(@"\bda i ankom\b"),
                                 MyRegEx(@"\bda i forlod\b"),
                                 MyRegEx(@"\bDa i forlod\b"),
                                 MyRegEx(@"\bda i fik\b"),
                                 MyRegEx(@"\bDa i fik\b"),
                                 MyRegEx(@"\bDa i gik\b"),
                                 MyRegEx(@"\bda i gik\b"),
                                 MyRegEx(@"\bda i kom\b"),
                                 MyRegEx(@"\bDa i kom\b"),
                                 MyRegEx(@"\bda i så "),
                                 MyRegEx(@"\bDa i så "),
                                 MyRegEx(@"\bdet får i\b"),
                                 MyRegEx(@"\bDet får i\b"),
                                 MyRegEx(@"\bDet har i\b"),
                                 MyRegEx(@"\bdet har i\b"),
                                 MyRegEx(@"\bDet må i "),
                                 MyRegEx(@"\bdet må i "),
                                 MyRegEx(@"\b[Dd]et Det kan i sgu"),
                                 MyRegEx(@"\bend i aner\b"),
                                 MyRegEx(@"\bend i tror\b"),
                                 MyRegEx(@"\bend i ved\b"),
                                 MyRegEx(@"\b, er i alle\b"),
                                 MyRegEx(@"\bellers får i "),
                                 MyRegEx(@"\bEr i alle\b"),
                                 MyRegEx(@"\ber i allerede\b"),
                                 MyRegEx(@"\bEr i allerede\b"),
                                 MyRegEx(@"\ber i allesammen\b"),
                                 MyRegEx(@"\bEr i allesammen\b"),
                                 MyRegEx(@"\ber i der\b"),
                                 MyRegEx(@"\bEr i der\b"),
                                 MyRegEx(@"\bEr i fra\b"),
                                 MyRegEx(@"\bEr i gennem\b"),
                                 MyRegEx(@"\ber i gennem\b"),
                                 MyRegEx(@"\ber i glade\b"),
                                 MyRegEx(@"\bEr i glade\b"),
                                 MyRegEx(@"\bEr i gået\b"),
                                 MyRegEx(@"\ber i gået\b"),
                                 MyRegEx(@"\ber i her\b"),
                                 MyRegEx(@"\bEr i her\b"),
                                 MyRegEx(@"\ber i imod\b"),
                                 MyRegEx(@"\bEr i imod\b"),
                                 MyRegEx(@"\ber i klar\b"),
                                 MyRegEx(@"\bEr i klar\b"),
                                 MyRegEx(@"\bEr i mætte\b"),
                                 MyRegEx(@"\ber i mætte\b"),
                                 MyRegEx(@"\bEr i med\b"),
                                 MyRegEx(@"\ber i med\b"),
                                 MyRegEx(@"\ber i mod\b"),
                                 MyRegEx(@"\bEr i mod\b"),
                                 MyRegEx(@"\ber i okay\b"),
                                 MyRegEx(@"\bEr i okay\b"),
                                 MyRegEx(@"\ber i på\b"),
                                 MyRegEx(@"\bEr i på\b"),
                                 MyRegEx(@"\bEr i parate\b"),
                                 MyRegEx(@"\ber i parate\b"),
                                 MyRegEx(@"\ber i sikker\b"),
                                 MyRegEx(@"\bEr i sikker\b"),
                                 MyRegEx(@"\bEr i sikre\b"),
                                 MyRegEx(@"\ber i sikre\b"),
                                 MyRegEx(@"\ber i skøre\b"),
                                 MyRegEx(@"\bEr i skøre\b"),
                                 MyRegEx(@"\ber i stadig\b"),
                                 MyRegEx(@"\bEr i stadig\b"),
                                 MyRegEx(@"\bEr i sultne\b"),
                                 MyRegEx(@"\ber i sultne\b"),
                                 MyRegEx(@"\bEr i tilfredse\b"),
                                 MyRegEx(@"\ber i tilfredse\b"),
                                 MyRegEx(@"\bEr i to\b"),
                                 MyRegEx(@"\ber i ved at\b"),
                                 MyRegEx(@"\ber i virkelig\b"),
                                 MyRegEx(@"\bEr i virkelig\b"),
                                 MyRegEx(@"\bEr i vågne\b"),
                                 MyRegEx(@"\ber i vågne\b"),
                                 MyRegEx(@"\bfanden vil i?"),
                                 MyRegEx(@"\bfor ser i\b"),
                                 MyRegEx(@"\bFor ser i\b"),
                                 MyRegEx(@"\bFordi i ventede\b"),
                                 MyRegEx(@"\bfordi i ventede\b"),
                                 MyRegEx(@"\bFordi i deltog\b"),
                                 MyRegEx(@"\bfordi i deltog\b"),
                                 MyRegEx(@"\bforhandler i stadig\b"),
                                 MyRegEx(@"\bForhandler i stadig\b"),
                                 MyRegEx(@"\bforstår i\b"),
                                 MyRegEx(@"\bForstår i\b"),
                                 MyRegEx(@"\bFør i får\b"),
                                 MyRegEx(@"\bfør i får\b"),
                                 MyRegEx(@"\bFør i kommer\b"),
                                 MyRegEx(@"\bfør i kommer\b"),
                                 MyRegEx(@"\bFør i tager\b"),
                                 MyRegEx(@"\bfør i tager\b"),
                                 MyRegEx(@"\bfår i alle\b"),
                                 MyRegEx(@"\bfår i fratrukket\b"),
                                 MyRegEx(@"\bfår i ikke\b"),
                                 MyRegEx(@"\bfår i klø\b"),
                                 MyRegEx(@"\bfår i point\b"),
                                 MyRegEx(@"\bgider i at\b"),
                                 MyRegEx(@"\bGider i at\b"),
                                 MyRegEx(@"\bGider i ikke\b"),
                                 MyRegEx(@"\bgider i ikke\b"),
                                 MyRegEx(@"\bgider i lige\b"),
                                 MyRegEx(@"\bGider i lige\b"),
                                 MyRegEx(@"\b[Gg]ik i lige\b"),
                                 MyRegEx(@"\b[Gg]ik i hjem\b"),
                                 MyRegEx(@"\b[Gg]ik i over\b"),
                                 MyRegEx(@"\b[Gg]ik i forbi\b"),
                                 MyRegEx(@"\b[Gg]ik i ind\b"),
                                 MyRegEx(@"\b[Gg]ik i uden\b"),
                                 MyRegEx(@"\bGjorde i det\b"),
                                 MyRegEx(@"\bGjorde i det\b"),
                                 MyRegEx(@"\bgjorde i ikke\b"),
                                 MyRegEx(@"\bGider i godt\b"),
                                 MyRegEx(@"\bgider i godt\b"),
                                 MyRegEx(@"\bGider i ikke\b"),
                                 MyRegEx(@"\bgider i ikke\b"),
                                 MyRegEx(@"\b[Gg]iver i mig\b"),
                                 MyRegEx(@"\bglor i på\b"),
                                 MyRegEx(@"\bGlor i på\b"),
                                 MyRegEx(@"\b[Gg]lor i allesammen på\b"),
                                 MyRegEx(@"\b[Gg]lor i alle på\b"),
                                 MyRegEx(@"\bGår i ind\b"),
                                 MyRegEx(@"\bgår i ind\b"),
                                 MyRegEx(@"\b[Gg]å i bare\b"),
                                 MyRegEx(@"\bHørte i det\b"),
                                 MyRegEx(@"\bhørte i det\b"),
                                 MyRegEx(@"\bHar i \b"),
                                 MyRegEx(@"\bhar i ødelagt\b"),
                                 MyRegEx(@"\bhar i fået\b"),
                                 MyRegEx(@"\bHar i fået\b"),
                                 MyRegEx(@"\bHar i det\b"),
                                 MyRegEx(@"\bhar i det\b"),
                                 MyRegEx(@"\bhar i gjort\b"),
                                 MyRegEx(@"\bhar i ikke\b"),
                                 MyRegEx(@"\bHar i nogen\b"),
                                 MyRegEx(@"\bhar i nogen\b"),
                                 MyRegEx(@"\bHar i nok\b"),
                                 MyRegEx(@"\bhar i nok\b"),
                                 MyRegEx(@"\bhar i ordnet\b"),
                                 MyRegEx(@"\bHar i ordnet\b"),
                                 MyRegEx(@"\bhar i spist\b"),
                                 MyRegEx(@"\bHar i spist\b"),
                                 MyRegEx(@"\bhar i tænkt\b"),
                                 MyRegEx(@"\bhar i tabt\b"),
                                 MyRegEx(@"\bhelvede vil i?"),
                                 MyRegEx(@"\bHer har i\b"),
                                 MyRegEx(@"\bher har i\b"),
                                 MyRegEx(@"\b[Hh]older i fast\b"),
                                 MyRegEx(@"\b[Hh]older i godt fast\b"),
                                 MyRegEx(@"\bHvad fanden har i\b"),
                                 MyRegEx(@"\bhvad fanden har i\b"),
                                 MyRegEx(@"\bHvad fanden tror i\b"),
                                 MyRegEx(@"\bhvad fanden tror i\b"),
                                 MyRegEx(@"\bhvad fanden vil i\b"),
                                 MyRegEx(@"\bHvad fanden vil i\b"),
                                 MyRegEx(@"\bHvad gør i\b"),
                                 MyRegEx(@"\bhvad gør i\b"),
                                 MyRegEx(@"\bhvad har i\b"),
                                 MyRegEx(@"\bHvad har i\b"),
                                 MyRegEx(@"\bHvad i ikke\b"),
                                 MyRegEx(@"\bhvad i ikke\b"),
                                 MyRegEx(@"\b[Hh]vad laver i\b"),
                                 MyRegEx(@"\b[Hh]vad lavede i\b"),
                                 MyRegEx(@"\b[Hh]vad mener i\b"),
                                 MyRegEx(@"\b[Hh]vad siger i\b"),
                                 MyRegEx(@"\b[Hh]vad skal i\b"),
                                 MyRegEx(@"\b[Hh]vad snakker i\b"),
                                 MyRegEx(@"\b[Hh]vad sløver i\b"),
                                 MyRegEx(@"\b[Hh]vad synes i\b"),
                                 MyRegEx(@"\b[Hh]vad vil i\b"),
                                 MyRegEx(@"\b[Hh]vem er i\b"),
                                 MyRegEx(@"\b[Hh]vem fanden tror i\b"),
                                 MyRegEx(@"\b[Hh]vem tror i\b"),
                                 MyRegEx(@"\b[Hh]vilken slags mennesker er i?"),
                                 MyRegEx(@"\b[Hh]vilken slags folk er i?"),
                                 MyRegEx(@"\b[Hh]vis i altså\b"),
                                 MyRegEx(@"\b[Hh]vis i bare\b"),
                                 MyRegEx(@"\b[Hh]vis i forstår\b"),
                                 MyRegEx(@"\b[Hh]vis i får\b"),
                                 MyRegEx(@"\b[Hh]vis i går\b"),
                                 MyRegEx(@"\b[Hh]vis i ikke\b"),
                                 MyRegEx(@"\b[Hh]vis i lovede\b"),
                                 MyRegEx(@"\b[Hh]vis i lover\b"),
                                 MyRegEx(@"\b[Hh]vis i overholder\b"),
                                 MyRegEx(@"\b[Hh]vis i overtræder\b"),
                                 MyRegEx(@"\b[Hh]vis i slipper\b"),
                                 MyRegEx(@"\b[Hh]vis i taber\b"),
                                 MyRegEx(@"\b[Hh]vis i vandt\b"),
                                 MyRegEx(@"\b[Hh]vis i vinder\b"),
                                 MyRegEx(@"\b[Hh]vor er i\b"),
                                 MyRegEx(@"\b[Hh]vor får i\b"),
                                 MyRegEx(@"\b[Hh]vor gamle er i\b"),
                                 MyRegEx(@"\b[Hh]vor i begyndte\b"),
                                 MyRegEx(@"\b[Hh]vor i startede\b"),
                                 MyRegEx(@"\b[Hh]vor skal i\b"),
                                 MyRegEx(@"\b[Hh]vor var i\b"),
                                 MyRegEx(@"\b[Hh]vordan har i\b"),
                                 MyRegEx(@"\b[Hh]vordan hørte i\b"),
                                 MyRegEx(@"\b[Hh]vordan i når\b"),
                                 MyRegEx(@"\b[Hh]vordan i nåede\b"),
                                 MyRegEx(@"\b[Hh]vordan kunne i\b"),
                                 MyRegEx(@"\b[Hh]vorfor afleverer i det\b"),
                                 MyRegEx(@"\b[Hh]vorfor gør i "),
                                 MyRegEx(@"\b[Hh]vorfor gjorde i "),
                                 MyRegEx(@"\b[Hh]vorfor græder i "),
                                 MyRegEx(@"\b[Hh]vorfor har i "),
                                 MyRegEx(@"\b[Hh]vorfor kom i "),
                                 MyRegEx(@"\b[Hh]vorfor kommer i "),
                                 MyRegEx(@"\b[Hh]vorfor løb i "),
                                 MyRegEx(@"\b[Hh]vorfor lover i "),
                                 MyRegEx(@"\b[Hh]vorfor lovede i "),
                                 MyRegEx(@"\b[Hh]vorfor skal i\b"),
                                 MyRegEx(@"\b[Hh]vorfor skulle i\b"),
                                 MyRegEx(@"\b[Hh]vorfor sagde i\b"),
                                 MyRegEx(@"\b[Hh]vorfor synes i\b"),
                                 MyRegEx(@"\b[Hh]vornår gør i "),
                                 MyRegEx(@"\bHvornår kom i\b"),
                                 MyRegEx(@"\b[Hh]vornår ville i "),
                                 MyRegEx(@"\b[Hh]vornår giver i "),
                                 MyRegEx(@"\b[Hh]vornår gav i "),
                                 MyRegEx(@"\b[Hh]vornår rejser i\b"),
                                 MyRegEx(@"\b[Hh]vornår rejste i\b"),
                                 MyRegEx(@"\b[Hh]vornår skal i "),
                                 MyRegEx(@"\b[Hh]vornår skulle i "),
                                 MyRegEx(@"\b[Hh]ører i på\b"),
                                 MyRegEx(@"\b[Hh]ørte i på\b"),
                                 MyRegEx(@"\b[Hh]ører i,\b"),
                                 MyRegEx(@"\b[Hh]ører i ikke\b"),
                                 MyRegEx(@"\bi altid\b"),
                                 MyRegEx(@"\bi ankomme\b"),
                                 MyRegEx(@"\bi ankommer\b"),
                                 MyRegEx(@"\bi bare kunne\b"),
                                 MyRegEx(@"\bi bare havde\b"),
                                 MyRegEx(@"\bi bare gjorde\b"),
                                 MyRegEx(@"\bi begge er\b"),
                                 MyRegEx(@"\bi begge gør\b"),
                                 MyRegEx(@"\bi begge har\b"),
                                 MyRegEx(@"\bi begge var\b"),
                                 MyRegEx(@"\bi begge vil\b"),
                                 MyRegEx(@"\bi behøver ikke gemme\b"),
                                 MyRegEx(@"\bi behøver ikke prøve\b"),
                                 MyRegEx(@"\bi behøver ikke skjule\b"),
                                 MyRegEx(@"\bi behandlede\b"),
                                 MyRegEx(@"\bi behandler\b"),
                                 MyRegEx(@"\bi beskidte dyr\b"),
                                 MyRegEx(@"\bi blev\b"),
                                 MyRegEx(@"\bi blive\b"),
                                 MyRegEx(@"\bi bliver\b"),
                                 MyRegEx(@"\bi burde\b"),
                                 MyRegEx(@"\bi er\b"),
                                 MyRegEx(@"\bi fyrer af\b"),
                                 MyRegEx(@"\bi gør\b"),
                                 MyRegEx(@"\bi gav\b"),
                                 MyRegEx(@"\bi gerne "),
                                 MyRegEx(@"\bi giver\b"),
                                 MyRegEx(@"\bi gjorde\b"),
                                 MyRegEx(@"\bi hører\b"),
                                 MyRegEx(@"\bi hørte\b"),
                                 MyRegEx(@"\bi har\b"),
                                 MyRegEx(@"\bi havde\b"),
                                 MyRegEx(@"\bi igen bliver\b"),
                                 MyRegEx(@"\bi igen burde\b"),
                                 MyRegEx(@"\bi igen finder\b"),
                                 MyRegEx(@"\bi igen gør\b"),
                                 MyRegEx(@"\bi igen kommer\b"),
                                 MyRegEx(@"\bi igen prøver\b"),
                                 MyRegEx(@"\bi igen siger\b"),
                                 MyRegEx(@"\bi igen skal\b"),
                                 MyRegEx(@"\bi igen vil\b"),
                                 MyRegEx(@"\bi ikke gerne\b"),
                                 MyRegEx(@"\bi ikke kan\b"),
                                 MyRegEx(@"\bi ikke kommer\b"),
                                 MyRegEx(@"\bi ikke vil\b"),
                                 MyRegEx(@"\bi kan\b"),
                                 MyRegEx(@"\bi kender\b"),
                                 MyRegEx(@"\bi kom\b"),
                                 MyRegEx(@"\bi komme\b"),
                                 MyRegEx(@"\bi kommer\b"),
                                 MyRegEx(@"\bi kunne\b"),
                                 MyRegEx(@"\bi morer jer\b"),
                                 MyRegEx(@"\bi må gerne\b"),
                                 MyRegEx(@"\bi må give\b"),
                                 MyRegEx(@"\bi må da\b"),
                                 MyRegEx(@"\bi nåede\b"),
                                 MyRegEx(@"\bi når\b"),
                                 MyRegEx(@"\bi prøve\b"),
                                 MyRegEx(@"\bi prøvede\b"),
                                 MyRegEx(@"\bi prøver\b"),
                                 MyRegEx(@"\bi sagde\b"),
                                 MyRegEx(@"\bi scorede\b"),
                                 MyRegEx(@"\bi ser\b"),
                                 MyRegEx(@"\bi set\b"),
                                 MyRegEx(@"\bi siger\b"),
                                 MyRegEx(@"\bi sikkert alle\b"),
                                 MyRegEx(@"\bi sikkert ikke gør\b"),
                                 MyRegEx(@"\bi sikkert ikke kan\b"),
                                 MyRegEx(@"\bi sikkert ikke vil\b"),
                                 MyRegEx(@"\bi skal\b"),
                                 MyRegEx(@"\bi skulle\b"),
                                 MyRegEx(@"\bi små stakler\b"),
                                 MyRegEx(@"\bi stopper\b"),
                                 MyRegEx(@"\bi synes\b"),
                                 MyRegEx(@"\bi troede\b"),
                                 MyRegEx(@"\bi tror\b"),
                                 MyRegEx(@"\bi var\b"),
                                 MyRegEx(@"\bi vel ikke\b"),
                                 MyRegEx(@"\bi vil\b"),
                                 MyRegEx(@"\bi ville\b"),
                                 MyRegEx(@"\b[Kk]an i lugte\b"),
                                 MyRegEx(@"\b[Kk]an i overleve\b"),
                                 MyRegEx(@"\b[Kk]an i spise\b"),
                                 MyRegEx(@"\b[Kk]an i se\b"),
                                 MyRegEx(@"\b[Kk]an i smage\b"),
                                 MyRegEx(@"\b[Kk]an i forstå\b"),
                                 MyRegEx(@"\b[Kk]ørte i hele\b"),
                                 MyRegEx(@"\b[Kk]ørte i ikke\b"),
                                 MyRegEx(@"\b[Kk]an i godt\b"),
                                 MyRegEx(@"\b[Kk]an i gøre\b"),
                                 MyRegEx(@"\b[Kk]an i huske\b"),
                                 MyRegEx(@"\b[Kk]an i ikke\b"),
                                 MyRegEx(@"\b[Kk]an i lide\b"),
                                 MyRegEx(@"\b[Kk]an i leve\b"),
                                 MyRegEx(@"\b[Kk]an i love\b"),
                                 MyRegEx(@"\b[Kk]an i måske\b"),
                                 MyRegEx(@"\b[Kk]an i nok\b"),
                                 MyRegEx(@"\b[Kk]an i se\b"),
                                 MyRegEx(@"\b[Kk]an i sige\b"),
                                 MyRegEx(@"\b[Kk]an i tilgive\b"),
                                 MyRegEx(@"\b[Kk]an i tygge\b"),
                                 MyRegEx(@"\b[Kk]an i to ikke\b"),
                                 MyRegEx(@"\b[Kk]an i tro\b"),
                                 MyRegEx(@"\bKender i "),
                                 MyRegEx(@"\b[Kk]ender i hinanden\b"),
                                 MyRegEx(@"\b[Kk]ender i to hinanden\b"),
                                 MyRegEx(@"\bKendte i \b"),
                                 MyRegEx(@"\b[Kk]endte i hinanden\b"),
                                 MyRegEx(@"\b[Kk]iggede i på\b"),
                                 MyRegEx(@"\b[Kk]igger i på\b"),
                                 MyRegEx(@"\b[Kk]ommer i her\b"),
                                 MyRegEx(@"\b[Kk]ommer i ofte\b"),
                                 MyRegEx(@"\b[Kk]ommer i sammen\b"),
                                 MyRegEx(@"\b[Kk]ommer i tit\b"),
                                 MyRegEx(@"\b[Kk]unne i fortælle\b"),
                                 MyRegEx(@"\b[Kk]unne i give\b"),
                                 MyRegEx(@"\b[Kk]unne i gøre\b"),
                                 MyRegEx(@"\b[Kk]unne i ikke\b"),
                                 MyRegEx(@"\b[Kk]unne i lide\b"),
                                 MyRegEx(@"\b[Kk]unne i mødes\b"),
                                 MyRegEx(@"\b[Kk]unne i se\b"),
                                 MyRegEx(@"\b[Ll]eder i efter\b"),
                                 MyRegEx(@"\b[Ll]aver i ikke\b"),
                                 MyRegEx(@"\blaver i her\b"),
                                 MyRegEx(@"\b[Ll]igner i far\b"),
                                 MyRegEx(@"\b[Ll]igner i hinanden\b"),
                                 MyRegEx(@"\b[Ll]igner i mor\b"),
                                 MyRegEx(@"\bLover i\b"),
                                 MyRegEx(@"\b[Ll]ykkes i med\b"),
                                 MyRegEx(@"\b[Ll]ykkedes i med\b"),
                                 MyRegEx(@"\b[Ll]øb i hellere\b"),
                                 MyRegEx(@"\b[Mm]ødte i "),
                                 MyRegEx(@"\b[Mm]angler i en\b"),
                                 MyRegEx(@"\b[Mm]en i gutter\b"),
                                 MyRegEx(@"\b[Mm]en i drenge\b"),
                                 MyRegEx(@"\b[Mm]en i fyre\b"),
                                 MyRegEx(@"\b[Mm]en i står\b"),
                                 MyRegEx(@"\b[Mm]ener i at\b"),
                                 MyRegEx(@"\b[Mm]ener i det\b"),
                                 MyRegEx(@"\b[Mm]ener i virkelig\b"),
                                 MyRegEx(@"\b[Mm]ens i sov\b"),
                                 MyRegEx(@"\b[Mm]ens i stadig\b"),
                                 MyRegEx(@"\b[Mm]ens i lå\b"),
                                 MyRegEx(@"\b[Mm]ister i point\b"),
                                 MyRegEx(@"\b[Mm]orer i jer\b"),
                                 MyRegEx(@"\b[Mm]å i alle"),
                                 MyRegEx(@"\b[Mm]å i gerne"),
                                 MyRegEx(@"\b[Mm]å i godt\b"),
                                 MyRegEx(@"\b[Mm]å i vide\b"),
                                 MyRegEx(@"\b[Mm]å i ikke"),
                                 MyRegEx(@"\b[Nn]u løber i\b"),
                                 MyRegEx(@"\b[Nn]u siger i\b"),
                                 MyRegEx(@"\b[Nn]u skal i\b"),
                                 MyRegEx(@"\b[Nn]år i\b"),
                                 MyRegEx(@"\b[Oo]m i ikke\b"),
                                 MyRegEx(@"\b[Oo]pgiver i\b"),
                                 MyRegEx(@"\b[Oo]vergiver i jer\b"),
                                 MyRegEx(@"\bpersoner i lukker\b"),
                                 MyRegEx(@"\b[Pp]as på i ikke\b"),
                                 MyRegEx(@"\b[Pp]as på i ikke\b"),
                                 MyRegEx(@"\b[Pp]å i ikke\b"),
                                 MyRegEx(@"\b[Pp]å at i ikke\b"),
                                 MyRegEx(@"\b[Ss]agde i ikke\b"),
                                 MyRegEx(@"\b[Ss]amlede i ham\b"),
                                 MyRegEx(@"\bSer i\b"),
                                 MyRegEx(@"\bSiger i\b"),
                                 MyRegEx(@"\b[Ss]ikker på i ikke\b"),
                                 MyRegEx(@"\b[Ss]ikre på i ikke\b"),
                                 MyRegEx(@"\b[Ss]kal i alle\b"),
                                 MyRegEx(@"\b[Ss]kal i allesammen\b"),
                                 MyRegEx(@"\b[Ss]kal i begge dø\b"),
                                 MyRegEx(@"\b[Ss]kal i bare\b"),
                                 MyRegEx(@"\b[Ss]kal i dele\b"),
                                 MyRegEx(@"\b[Ss]kal i dø\b"),
                                 MyRegEx(@"\b[Ss]kal i fordele\b"),
                                 MyRegEx(@"\b[Ss]kal i fordeles\b"),
                                 MyRegEx(@"\b[Ss]kal i fortælle\b"),
                                 MyRegEx(@"\b[Ss]kal i gøre\b"),
                                 MyRegEx(@"\b[Ss]kal i have\b"),
                                 MyRegEx(@"\b[Ss]kal i ikke\b"),
                                 MyRegEx(@"\b[Ss]kal i klare\b"),
                                 MyRegEx(@"\b[Ss]kal i klatre\b"),
                                 MyRegEx(@"\b[Ss]kal i larme\b"),
                                 MyRegEx(@"\b[Ss]kal i lave\b"),
                                 MyRegEx(@"\b[Ss]kal i løfte\b"),
                                 MyRegEx(@"\b[Ss]kal i med\b"),
                                 MyRegEx(@"\b[Ss]kal i på\b"),
                                 MyRegEx(@"\b[Ss]kal i til\b"),
                                 MyRegEx(@"\b[Ss]kal i ud\b"),
                                 MyRegEx(@"\b[Ss]lap i ud\b"),
                                 MyRegEx(@"\b[Ss]lap i væk\b"),
                                 MyRegEx(@"\b[Ss]nart er i\b"),
                                 MyRegEx(@"\b[Ss]om i måske\b"),
                                 MyRegEx(@"\b[Ss]om i nok\b"),
                                 MyRegEx(@"\b[Ss]om i ved\b"),
                                 MyRegEx(@"\b[Ss]pis i bare\b"),
                                 MyRegEx(@"\b[Ss]pis i dem\b"),
                                 MyRegEx(@"\b[Ss]ynes i at\b"),
                                 MyRegEx(@"\b[Ss]ynes i det\b"),
                                 MyRegEx(@"\b[Ss]ynes i,"),
                                 MyRegEx(@"\b[Ss]ætter i en\b"),
                                 MyRegEx(@"\bSå i at\b"),
                                 MyRegEx(@"\bSå i det\b"),
                                 MyRegEx(@"\bSå i noget\b"),
                                 MyRegEx(@"\b[Ss]å tager i\b"),
                                 MyRegEx(@"\bTænder i på\b"),
                                 MyRegEx(@"\btænder i på\b"),
                                 MyRegEx(@"\btog i bilen\b"),
                                 MyRegEx(@"\bTog i bilen\b"),
                                 MyRegEx(@"\btog i liften\b"),
                                 MyRegEx(@"\bTog i liften\b"),
                                 MyRegEx(@"\btog i toget\b"),
                                 MyRegEx(@"\bTog i toget\b"),
                                 MyRegEx(@"\btræder i frem\b"),
                                 MyRegEx(@"\bTræder i frem\b"),
                                 MyRegEx(@"\bTror i at\b"),
                                 MyRegEx(@"\btror i at\b"),
                                 MyRegEx(@"\btror i det\b"),
                                 MyRegEx(@"\bTror i det\b"),
                                 MyRegEx(@"\bTror i jeg\b"),
                                 MyRegEx(@"\btror i jeg\b"),
                                 MyRegEx(@"\bTror i på\b"),
                                 MyRegEx(@"\b[Tr]ror i på\b"),
                                 MyRegEx(@"\b[Tr]ror i, "),
                                 MyRegEx(@"\b[Vv]ar i blevet\b"),
                                 MyRegEx(@"\b[Vv]ed i alle\b"),
                                 MyRegEx(@"\b[Vv]ed i allesammen\b"),
                                 MyRegEx(@"\b[Vv]ed i er\b"),
                                 MyRegEx(@"\b[Vv]ed i ikke\b"),
                                 MyRegEx(@"\b[Vv]ed i hvad\b"),
                                 MyRegEx(@"\b[Vv]ed i hvem\b"),
                                 MyRegEx(@"\b[Vv]ed i hvor\b"),
                                 MyRegEx(@"\b[Vv]ed i hvorfor\b"),
                                 MyRegEx(@"\b[Vv]ed i hvordan\b"),
                                 MyRegEx(@"\b[Vv]ed i var\b"),
                                 MyRegEx(@"\b[Vv]ed i ville\b"),
                                 MyRegEx(@"\b[Vv]ed i har\b"),
                                 MyRegEx(@"\b[Vv]ed i havde\b"),
                                 MyRegEx(@"\b[Vv]ed i hvem\b"),
                                 MyRegEx(@"\b[Vv]ed i hvad\b"),
                                 MyRegEx(@"\b[Vv]ed i hvor\b"),
                                 MyRegEx(@"\b[Vv]ed i mente\b"),
                                 MyRegEx(@"\b[Vv]ed i tror\b"),
                                 MyRegEx(@"\b[Vv]enter i på\b"),
                                 MyRegEx(@"\b[Vv]il i besegle\b"),
                                 MyRegEx(@"\b[Vv]il i dræbe\b"),
                                 MyRegEx(@"\b[Vv]il i fjerne\b"),
                                 MyRegEx(@"\b[Vv]il i fortryde\b"),
                                 MyRegEx(@"\b[Vv]il i gerne\b"),
                                 MyRegEx(@"\b[Vv]il i godt\b"),
                                 MyRegEx(@"\b[Vv]il i have\b"),
                                 MyRegEx(@"\b[Vv]il i høre\b"),
                                 MyRegEx(@"\b[Vv]il i ikke\b"),
                                 MyRegEx(@"\b[Vv]il i købe\b"),
                                 MyRegEx(@"\b[Vv]il i kaste\b"),
                                 MyRegEx(@"\b[Vv]il i møde\b"),
                                 MyRegEx(@"\b[Vv]il i måske\b"),
                                 MyRegEx(@"\bvil i savne\b"),
                                 MyRegEx(@"\bVil i savne\b"),
                                 MyRegEx(@"\bvil i se\b"),
                                 MyRegEx(@"\bVil i se\b"),
                                 MyRegEx(@"\bvil i sikkert\b"),
                                 MyRegEx(@"\bvil i smage\b"),
                                 MyRegEx(@"\bVil i smage\b"),
                                 MyRegEx(@"\b[Vv]il i virkelig\b"),
                                 MyRegEx(@"\b[Vv]il i virkeligt\b"),
                                 MyRegEx(@"\bVil i være\b"),
                                 MyRegEx(@"\bvil i være\b"),
                                 MyRegEx(@"\bVille i blive\b"),
                                 MyRegEx(@"\bville i blive\b"),
                                 MyRegEx(@"\bville i dræbe\b"),
                                 MyRegEx(@"\bville i få\b"),
                                 MyRegEx(@"\bville i få\b"),
                                 MyRegEx(@"\bville i gøre\b"),
                                 MyRegEx(@"\bville i høre\b"),
                                 MyRegEx(@"\bville i ikke\b"),
                                 MyRegEx(@"\bville i kaste\b"),
                                 MyRegEx(@"\bville i komme\b"),
                                 MyRegEx(@"\bville i mene\b"),
                                 MyRegEx(@"\bville i nå\b"),
                                 MyRegEx(@"\bville i savne\b"),
                                 MyRegEx(@"\bVille i se\b"),
                                 MyRegEx(@"\bville i se\b"),
                                 MyRegEx(@"\bville i sikkert\b"),
                                 MyRegEx(@"\bville i synes\b"),
                                 MyRegEx(@"\bville i tage\b"),
                                 MyRegEx(@"\bville i tro\b"),
                                 MyRegEx(@"\bville i være\b"),
                                 MyRegEx(@"\bville i være\b"),
                                 MyRegEx(@"\b[Vv]iste i, at\b"),
                                 MyRegEx(@"\b[Vv]iste i at\b"),
                                 MyRegEx(@"\bvover i\b"),

                             };

            Regex regExIDag = new Regex(@"\bidag\b", RegexOptions.Compiled);
            Regex regExIGaar = new Regex(@"\bigår\b", RegexOptions.Compiled);
            Regex regExIMorgen = new Regex(@"\bimorgen\b", RegexOptions.Compiled);
            Regex regExIAlt = new Regex(@"\bialt\b", RegexOptions.Compiled);
            Regex regExIGang = new Regex(@"\bigang\b", RegexOptions.Compiled);
            Regex regExIStand = new Regex(@"\bistand\b", RegexOptions.Compiled);
            Regex regExIOevrigt = new Regex(@"\biøvrigt\b", RegexOptions.Compiled);

            for (int i = 0; i < _subtitle.Paragraphs.Count; i++)
            {
                string text = _subtitle.Paragraphs[i].Text;
                string oldText = text;

                if (littleIRegex.IsMatch(text))
                {
                    foreach (Regex regex in iList)
                    {
                        Match match = regex.Match(text);
                        while (match.Success)
                        {
                            Match iMatch = littleIRegex.Match(match.Value);
                            if (iMatch.Success)
                            {
                                string temp = match.Value.Remove(iMatch.Index, 1).Insert(iMatch.Index, "I");

                                int index = match.Index;
                                if (index + match.Value.Length >= text.Length)
                                    text = text.Substring(0, index) + temp;
                                else
                                    text = text.Substring(0, index) + temp + text.Substring(index + match.Value.Length);
                            }
                            match = match.NextMatch();
                        }
                    }
                }

                if (regExIDag.IsMatch(text))
                    text = regExIDag.Replace(text, "i dag");

                if (regExIGaar.IsMatch(text))
                    text = regExIGaar.Replace(text, "i går");

                if (regExIMorgen.IsMatch(text))
                    text = regExIMorgen.Replace(text, "i morgen");

                if (regExIAlt.IsMatch(text))
                    text = regExIAlt.Replace(text, "i alt");

                if (regExIGang.IsMatch(text))
                    text = regExIGang.Replace(text, "i gang");

                if (regExIStand.IsMatch(text))
                    text = regExIStand.Replace(text, "i stand");

                if (regExIOevrigt.IsMatch(text))
                    text = regExIOevrigt.Replace(text, "i øvrigt");

                if (text != oldText)
                {
                    _subtitle.Paragraphs[i].Text = text;
                    fixCount++;
                    _totalFixes++;
                    AddFixToListView(_subtitle.Paragraphs[i], fixAction, oldText, text);
                }
            }
            if (fixCount > 0)
                LogStatus(_language.FixDanishLetterI, string.Format(_language.XIsChangedToUppercase, fixCount));
        }

        /// <summary>
        /// Will try to fix issues with Spanish special letters ¿? and ¡!.
        /// Sentences ending with "?" must start with "¿".
        /// Sentences ending with "!" must start with "¡".
        /// </summary>
        public void FixSpanishInvertedQuestionAndExclamationMarks()
        {
            string fixAction = _language.FixSpanishInvertedQuestionAndExclamationMarks;
            int fixCount = 0;
            for (int i = 0; i < _subtitle.Paragraphs.Count; i++)
            {
                Paragraph p = _subtitle.Paragraphs[i];
                Paragraph last = _subtitle.GetParagraphOrDefault(i - 1);

                bool wasLastLineClosed = last == null || last.Text.EndsWith('?') || last.Text.EndsWith('!') || last.Text.EndsWith('.') ||
                                         last.Text.EndsWith(':') || last.Text.EndsWith(')') || last.Text.EndsWith(']');
                string trimmedStart = p.Text.TrimStart('-', ' ');
                if (last != null && last.Text.EndsWith("...", StringComparison.Ordinal) && trimmedStart.Length > 0 && char.IsLower(trimmedStart[0]))
                    wasLastLineClosed = false;
                if (!wasLastLineClosed && last.Text == last.Text.ToUpper())
                    wasLastLineClosed = true;

                string oldText = p.Text;

                FixSpanishInvertedLetter('?', "¿", p, last, ref wasLastLineClosed, fixAction, ref fixCount);
                FixSpanishInvertedLetter('!', "¡", p, last, ref wasLastLineClosed, fixAction, ref fixCount);

                if (p.Text != oldText)
                {
                    fixCount++;
                    _totalFixes++;
                    AddFixToListView(p, fixAction, oldText, p.Text);
                }

            }
            if (fixCount > 0)
                LogStatus(_language.FixSpanishInvertedQuestionAndExclamationMarks, fixCount.ToString());
        }

        private void FixSpanishInvertedLetter(char mark, string inverseMark, Paragraph p, Paragraph last, ref bool wasLastLineClosed, string fixAction, ref int fixCount)
        {
            if (p.Text.Contains(mark))
            {
                bool skip = false;
                if (last != null && p.Text.Contains(mark) && !p.Text.Contains(inverseMark) && last.Text.Contains(inverseMark) && !last.Text.Contains(mark))
                    skip = true;

                if (!skip && Utilities.CountTagInText(p.Text, mark) == Utilities.CountTagInText(p.Text, inverseMark) &&
                    Utilities.RemoveHtmlTags(p.Text).TrimStart(inverseMark[0]).Contains(inverseMark) == false &&
                    Utilities.RemoveHtmlTags(p.Text).TrimEnd(mark).Contains(mark) == false)
                {
                    skip = true;
                }

                if (!skip)
                {
                    int startIndex = 0;
                    int markIndex = p.Text.IndexOf(mark);
                    if (!wasLastLineClosed && ((p.Text.IndexOf('!') > 0 && p.Text.IndexOf('!') < markIndex) ||
                                               (p.Text.IndexOf('?') > 0 && p.Text.IndexOf('?') < markIndex) ||
                                               (p.Text.IndexOf('.') > 0 && p.Text.IndexOf('.') < markIndex)))
                        wasLastLineClosed = true;
                    while (markIndex > 0 && startIndex < p.Text.Length)
                    {
                        int inverseMarkIndex = p.Text.IndexOf(inverseMark, startIndex, StringComparison.Ordinal);
                        if (wasLastLineClosed && (inverseMarkIndex == -1 || inverseMarkIndex > markIndex))
                        {
                            if (AllowFix(p, fixAction))
                            {
                                int j = markIndex - 1;

                                while (j > startIndex && (p.Text[j] == '.' || p.Text[j] == '!' || p.Text[j] == '?'))
                                    j--;

                                while (j > startIndex &&
                                       (p.Text[j] != '.' || IsSpanishAbbreviation(p.Text, j)) &&
                                       p.Text[j] != '!' &&
                                       p.Text[j] != '?' &&
                                       !(j > 3 && p.Text.Substring(j - 3, 3) == Environment.NewLine + "-") &&
                                       !(j > 4 && p.Text.Substring(j - 4, 4) == Environment.NewLine + " -") &&
                                       !(j > 6 && p.Text.Substring(j - 6, 6) == Environment.NewLine + "<i>-"))
                                    j--;

                                if (@".!?".Contains(p.Text[j]))
                                {
                                    j++;
                                }
                                if (j + 3 < p.Text.Length && p.Text.Substring(j + 1, 2) == Environment.NewLine)
                                {
                                    j += 3;
                                }
                                else if (j + 2 < p.Text.Length && p.Text.Substring(j, 2) == Environment.NewLine)
                                {
                                    j += 2;
                                }
                                if (j >= startIndex)
                                {
                                    string part = p.Text.Substring(j, markIndex - j + 1);

                                    string speaker = string.Empty;
                                    int speakerEnd = part.IndexOf(')');
                                    if (part.StartsWith('(') && speakerEnd > 0 && speakerEnd < part.IndexOf(mark))
                                    {
                                        while (Environment.NewLine.Contains(part[speakerEnd + 1]))
                                            speakerEnd++;
                                        speaker = part.Substring(0, speakerEnd + 1);
                                        part = part.Substring(speakerEnd + 1);
                                    }
                                    speakerEnd = part.IndexOf(']');
                                    if (part.StartsWith('[') && speakerEnd > 0 && speakerEnd < part.IndexOf(mark))
                                    {
                                        while (Environment.NewLine.Contains(part[speakerEnd + 1]))
                                            speakerEnd++;
                                        speaker = part.Substring(0, speakerEnd + 1);
                                        part = part.Substring(speakerEnd + 1);
                                    }

                                    var st = new StripableText(part);
                                    if (j == 0 && mark == '!' && st.Pre == "¿" && Utilities.CountTagInText(p.Text, mark) == 1 && Utilities.RemoveHtmlTags(p.Text).EndsWith(mark))
                                    {
                                        p.Text = inverseMark + p.Text;
                                    }
                                    else if (j == 0 && mark == '?' && st.Pre == "¡" && Utilities.CountTagInText(p.Text, mark) == 1 && Utilities.RemoveHtmlTags(p.Text).EndsWith(mark))
                                    {
                                        p.Text = inverseMark + p.Text;
                                    }
                                    else
                                    {
                                        string temp = inverseMark;
                                        int addToIndex = 0;
                                        while (p.Text.Length > markIndex + 1 && p.Text[markIndex + 1] == mark &&
                                            Utilities.CountTagInText(p.Text, mark) > Utilities.CountTagInText(p.Text + temp, inverseMark))
                                        {
                                            temp += inverseMark;
                                            st.Post += mark;
                                            markIndex++;
                                            addToIndex++;
                                        }

                                        p.Text = p.Text.Remove(j, markIndex - j + 1).Insert(j, speaker + st.Pre + temp + st.StrippedText + st.Post);
                                        markIndex += addToIndex;
                                    }

                                }
                            }
                        }
                        else if (last != null && !wasLastLineClosed && inverseMarkIndex == p.Text.IndexOf(mark) && !last.Text.Contains(inverseMark))
                        {
                            string lastOldtext = last.Text;
                            int idx = last.Text.Length - 2;
                            while (idx > 0 && (last.Text.Substring(idx, 2) != ". ") && (last.Text.Substring(idx, 2) != "! ") && (last.Text.Substring(idx, 2) != "? "))
                                idx--;

                            last.Text = last.Text.Insert(idx, inverseMark);
                            fixCount++;
                            _totalFixes++;
                            AddFixToListView(last, fixAction, lastOldtext, last.Text);
                        }

                        startIndex = markIndex + 2;
                        if (startIndex < p.Text.Length)
                            markIndex = p.Text.IndexOf(mark, startIndex);
                        else
                            markIndex = -1;
                        wasLastLineClosed = true;
                    }
                }
                if (p.Text.EndsWith(mark + "...", StringComparison.Ordinal) && p.Text.Length > 4)
                {
                    p.Text = p.Text.Remove(p.Text.Length - 4, 4) + "..." + mark;
                }
            }
            else if (Utilities.CountTagInText(p.Text, inverseMark) == 1)
            {
                int idx = p.Text.IndexOf(inverseMark, StringComparison.Ordinal);
                while (idx < p.Text.Length && !@".!?".Contains(p.Text[idx]))
                {
                    idx++;
                }
                if (idx < p.Text.Length)
                {
                    p.Text = p.Text.Insert(idx, mark.ToString());
                    if (p.Text.Contains("¡¿") && p.Text.Contains("!?"))
                        p.Text = p.Text.Replace("!?", "?!");
                    if (p.Text.Contains("¿¡") && p.Text.Contains("?!"))
                        p.Text = p.Text.Replace("?!", "!?");
                }
            }
        }

        private bool IsSpanishAbbreviation(string text, int index)
        {
            if (text[index] != '.')
                return false;

            if (index + 3 < text.Length && text[index + 2] == '.') //  X
                return true;                                    // O.R.

            if (index - 3 > 0 && text[index - 1] != '.' && text[index - 2] == '.') //    X
                return true;                          // O.R.

            string word = string.Empty;
            int i = index - 1;
            while (i >= 0 && Utilities.AllLetters.Contains(text[i]))
            {
                word = text[i] + word;
                i--;
            }

            //Common Spanish abbreviations
            //Dr. (same as English)
            //Sr. (same as Mr.)
            //Sra. (same as Mrs.)
            //Ud.
            //Uds.
            if (word.Equals("dr", StringComparison.OrdinalIgnoreCase) ||
                word.Equals("sr", StringComparison.OrdinalIgnoreCase) ||
                word.Equals("sra", StringComparison.OrdinalIgnoreCase) ||
                word.Equals("ud", StringComparison.OrdinalIgnoreCase) ||
                word.Equals("uds", StringComparison.OrdinalIgnoreCase))
                return true;

            List<string> abbreviations = GetAbbreviations();
            return abbreviations.Contains(word + ".");
        }

        private void ButtonFixClick(object sender, EventArgs e)
        {
            if (buttonBack.Enabled)
            {
                Cursor = Cursors.WaitCursor;
                SaveConfiguration();
                Cursor = Cursors.Default;
                DialogResult = DialogResult.OK;
            }
            else
            {
                if (listView1.Items[IndexAloneLowercaseIToUppercaseIEnglish].Checked && Language != "en")
                {
                    if (MessageBox.Show(_language.FixLowercaseIToUppercaseICheckedButCurrentLanguageIsNotEnglish + Environment.NewLine +
                                                      Environment.NewLine +
                                                      _language.ContinueAnyway, _language.Continue, MessageBoxButtons.YesNo) == DialogResult.No)
                    {
                        listView1.Items[IndexAloneLowercaseIToUppercaseIEnglish].Checked = false;
                        ShowStatus(_language.UncheckedFixLowercaseIToUppercaseI);
                        return;
                    }
                }
                Cursor = Cursors.WaitCursor;
                Next();
                ShowAvailableFixesStatus();
            }
            Cursor = Cursors.Default;
        }

        private void Next()
        {

            RunSelectedActions();

            buttonBack.Enabled = true;
            buttonNextFinish.Text = _languageGeneral.Ok;
            buttonNextFinish.Enabled = _hasFixesBeenMade;
            groupBoxStep1.Visible = false;
            groupBox2.Visible = true;
            listViewFixes.Sort();
            subtitleListView1.Fill(_originalSubtitle);
            if (listViewFixes.Items.Count > 0)
                listViewFixes.Items[0].Selected = true;
            else
                subtitleListView1.SelectIndexAndEnsureVisible(0);
        }

        private void RunSelectedActions()
        {
            subtitleListView1.BeginUpdate();
            _newLog = new StringBuilder();
            _deleteIndices = new List<int>();

            _subtitle = new Subtitle(_originalSubtitle);
            foreach (ListViewItem item in listView1.Items)
            {
                if (item.Checked && item.Index != IndexRemoveEmptyLines)
                {
                    var fixItem = (FixItem)item.Tag;
                    fixItem.Action.Invoke(null, null);
                }
            }
            if (listView1.Items[IndexInvalidItalicTags].Checked)
            {
                var fixItem = (FixItem)listView1.Items[IndexInvalidItalicTags].Tag;
                fixItem.Action.Invoke(null, null);
            }
            if (listView1.Items[IndexRemoveEmptyLines].Checked)
            {
                var fixItem = (FixItem)listView1.Items[IndexRemoveEmptyLines].Tag;
                fixItem.Action.Invoke(null, null);
            }

            // build log
            textBoxFixedIssues.Text = string.Empty;
            if (_newLog.Length >= 0)
                textBoxFixedIssues.AppendText(_newLog + Environment.NewLine);
            textBoxFixedIssues.AppendText(_appliedLog.ToString());
            subtitleListView1.EndUpdate();
        }

        private void FormFixKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
                DialogResult = DialogResult.Cancel;
            else if (e.KeyCode == Keys.F1)
                Utilities.ShowHelp("#fixcommonerrors");
            else if (e.KeyCode == Keys.Enter && buttonNextFinish.Text == _language.Next)
                ButtonFixClick(null, null);
            else if (subtitleListView1.Visible && subtitleListView1.Items.Count > 0 && e.KeyData == _goToLine)
                GoToLineNumber();
            else if (e.KeyData == _preview && listViewFixes.Items.Count > 0)
                GenerateDiff();
            else if (_mainGeneralGoToNextSubtitle == e.KeyData || (e.KeyCode == Keys.Down && e.Modifiers == Keys.Alt))
            {
                int selectedIndex = 0;
                if (subtitleListView1.SelectedItems.Count > 0)
                {
                    selectedIndex = subtitleListView1.SelectedItems[0].Index;
                    selectedIndex++;
                }
                subtitleListView1.SelectIndexAndEnsureVisible(selectedIndex);
            }
            else if (_mainGeneralGoToPrevSubtitle == e.KeyData || (e.KeyCode == Keys.Up && e.Modifiers == Keys.Alt))
            {
                int selectedIndex = 0;
                if (subtitleListView1.SelectedItems.Count > 0)
                {
                    selectedIndex = subtitleListView1.SelectedItems[0].Index;
                    selectedIndex--;
                }
                subtitleListView1.SelectIndexAndEnsureVisible(selectedIndex);
            }
            else if (_mainListViewGoToNextError == e.KeyData)
            {
                GoToNextSynaxError();
                e.SuppressKeyPress = true;
            }

        }

        private void GoToNextSynaxError()
        {
            int idx = 0;
            try
            {
                if (listViewFixes.SelectedItems.Count > 0)
                    idx = listViewFixes.SelectedItems[0].Index;
                idx++;
                if (listViewFixes.Items.Count > idx)
                {
                    listViewFixes.Items[idx].Selected = true;
                    listViewFixes.Items[idx].EnsureVisible();
                    if (idx > 0)
                        listViewFixes.Items[idx - 1].Selected = false;
                }
            }
            catch
            {
            }
        }

        private void GoToLineNumber()
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

        private void GenerateDiff()
        {
            string htmlFileName = Path.GetTempFileName() + ".html";
            var sb = new StringBuilder();
            sb.Append("<html><head><meta charset='utf-8'><title>Subtitle Edit - Fix common errors preview</title><style>body,p,td {font-size:90%; font-family:Tahoma;} td {border:1px solid black;padding:5px} table {border-collapse: collapse;}</style></head><body><table><tbody>");
            sb.AppendLine(string.Format("<tr><td style='font-weight:bold'>{0}</td><td style='font-weight:bold'>{1}</td><td style='font-weight:bold'>{2}</td><td style='font-weight:bold'>{3}</td></tr>", Configuration.Settings.Language.General.LineNumber, _language.Function, Configuration.Settings.Language.General.Before, Configuration.Settings.Language.General.After));
            foreach (ListViewItem item in listViewFixes.Items)
            {
                if (item.Checked)
                {
                    var p = (Paragraph)item.Tag;
                    string what = item.SubItems[2].Text;
                    string before = item.SubItems[3].Text;
                    string after = item.SubItems[4].Text;
                    var arr = MakeDiffHtml(before, after);
                    sb.AppendLine(string.Format("<tr><td>{0}</td><td>{1}</td><td><pre>{2}</pre></td><td><pre>{3}</pre></td></tr>", p.Number, what, arr[0], arr[1]));
                }
            }
            sb.Append("</table></body></html>");
            File.WriteAllText(htmlFileName, sb.ToString());
            System.Diagnostics.Process.Start(htmlFileName);
        }

        private static string[] MakeDiffHtml(string before, string after)
        {

            before = before.Replace("<br />", "↲");
            after = after.Replace("<br />", "↲");
            before = before.Replace(Environment.NewLine, "↲");
            after = after.Replace(Environment.NewLine, "↲");

            var beforeColors = new Dictionary<int, Color>();
            var beforeBackgroundColors = new Dictionary<int, Color>();
            var afterColors = new Dictionary<int, Color>();
            var afterBackgroundColors = new Dictionary<int, Color>();

            // from start
            int minLength = Math.Min(before.Length, after.Length);
            int startCharactersOk = 0;
            for (int i = 0; i < minLength; i++)
            {
                if (before[i] == after[i])
                {
                    startCharactersOk++;
                }
                else
                {
                    if (before.Length > i + 4 && after.Length > i + 4 &&
                        before[i + 1] == after[i + 1] &&
                        before[i + 2] == after[i + 2] &&
                        before[i + 3] == after[i + 3] &&
                        before[i + 4] == after[i + 4])
                    {
                        startCharactersOk++;

                        if (char.IsWhiteSpace(before[i]))
                            beforeBackgroundColors.Add(i, Color.Red);
                        else
                            beforeColors.Add(i, Color.Red);

                        if (char.IsWhiteSpace(after[i]))
                            afterBackgroundColors.Add(i, Color.Red);
                        else
                            afterColors.Add(i, Color.Red);
                    }
                    else
                    {
                        break;
                    }
                }
            }

            int maxLength = Math.Max(before.Length, after.Length);
            for (int i = startCharactersOk; i <= maxLength; i++)
            {
                if (i < before.Length)
                {
                    if (char.IsWhiteSpace(before[i]))
                        beforeBackgroundColors.Add(i, Color.Red);
                    else
                        beforeColors.Add(i, Color.Red);
                }
                if (i < after.Length)
                {
                    if (char.IsWhiteSpace(after[i]))
                        afterBackgroundColors.Add(i, Color.Red);
                    else
                        afterColors.Add(i, Color.Red);
                }
            }

            // from end
            for (int i = 1; i < minLength; i++)
            {
                int bLength = before.Length - i;
                int aLength = after.Length - i;
                if (before[bLength] == after[aLength])
                {
                    if (beforeColors.ContainsKey(bLength))
                        beforeColors.Remove(bLength);
                    if (beforeBackgroundColors.ContainsKey(bLength))
                        beforeBackgroundColors.Remove(bLength);

                    if (afterColors.ContainsKey(aLength))
                        afterColors.Remove(aLength);
                    if (afterBackgroundColors.ContainsKey(aLength))
                        afterBackgroundColors.Remove(aLength);
                }
                else
                {
                    if (i == 0 && i > 4 &&
                        before[before.Length - (i + 1)] == after[after.Length - (i + 1)] &&
                        before[before.Length - (i + 2)] == after[after.Length - (i + 2)] &&
                        before[before.Length - (i + 3)] == after[after.Length - (i + 3)] &&
                        before[before.Length - (i + 4)] == after[after.Length - (i + 4)])
                    {
                        continue;
                    }
                    else
                    {
                        break;
                    }
                }
            }

            var sb = new StringBuilder();
            for (int i = 0; i < before.Length; i++)
            {
                var s = before[i];
                if (beforeColors.ContainsKey(i) && beforeBackgroundColors.ContainsKey(i))
                {
                    sb.AppendFormat("<span style=\"color:{0}; background-color: {1}\">{2}</span>", ColorTranslator.ToHtml(beforeColors[i]), ColorTranslator.ToHtml(beforeBackgroundColors[i]), s);
                }
                else if (beforeColors.ContainsKey(i))
                {
                    sb.AppendFormat("<span style=\"color:{0}; \">{1}</span>", ColorTranslator.ToHtml(beforeColors[i]), s);
                }
                else if (beforeBackgroundColors.ContainsKey(i))
                {
                    sb.AppendFormat("<span style=\"background-color: {0}\">{1}</span>", ColorTranslator.ToHtml(beforeBackgroundColors[i]), s);
                }
                else
                {
                    sb.Append(s);
                }
            }
            var sb2 = new StringBuilder();
            for (int i = 0; i < after.Length; i++)
            {
                var s = after[i];
                if (afterColors.ContainsKey(i) && afterBackgroundColors.ContainsKey(i))
                {
                    sb2.AppendFormat("<span style=\"color:{0}; background-color: {1}\">{2}</span>", ColorTranslator.ToHtml(afterColors[i]), ColorTranslator.ToHtml(afterBackgroundColors[i]), s);
                }
                else if (afterColors.ContainsKey(i))
                {
                    sb2.AppendFormat("<span style=\"color:{0}; \">{1}</span>", ColorTranslator.ToHtml(afterColors[i]), s);
                }
                else if (afterBackgroundColors.ContainsKey(i))
                {
                    sb2.AppendFormat("<span style=\"background-color: {0}\">{1}</span>", ColorTranslator.ToHtml(afterBackgroundColors[i]), s);
                }
                else
                {
                    sb2.Append(s);
                }
            }

            return new string[] { sb.ToString(), sb2.ToString() };
        }

        private void SaveConfiguration()
        {
            FixCommonErrorsSettings ce = Configuration.Settings.CommonErrors;

            ce.EmptyLinesTicked = listView1.Items[IndexRemoveEmptyLines].Checked;
            ce.OverlappingDisplayTimeTicked = listView1.Items[IndexOverlappingDisplayTime].Checked;
            ce.TooShortDisplayTimeTicked = listView1.Items[IndexTooShortDisplayTime].Checked;
            ce.TooLongDisplayTimeTicked = listView1.Items[IndexTooLongDisplayTime].Checked;
            ce.InvalidItalicTagsTicked = listView1.Items[IndexInvalidItalicTags].Checked;
            ce.UnneededSpacesTicked = listView1.Items[IndexUnneededSpaces].Checked;
            ce.UnneededPeriodsTicked = listView1.Items[IndexUnneededPeriods].Checked;
            ce.MissingSpacesTicked = listView1.Items[IndexMissingSpaces].Checked;
            ce.BreakLongLinesTicked = listView1.Items[IndexBreakLongLines].Checked;
            ce.MergeShortLinesTicked = listView1.Items[IndexMergeShortLines].Checked;
            ce.MergeShortLinesAllTicked = listView1.Items[IndexMergeShortLinesAll].Checked;

            ce.UppercaseIInsideLowercaseWordTicked = listView1.Items[IndexUppercaseIInsideLowercaseWord].Checked;
            ce.DoubleApostropheToQuoteTicked = listView1.Items[IndexDoubleApostropheToQuote].Checked;
            ce.FixMusicNotationTicked = listView1.Items[IndexFixMusicNotation].Checked;
            ce.AddPeriodAfterParagraphTicked = listView1.Items[IndexAddPeriodAfterParagraph].Checked;
            ce.StartWithUppercaseLetterAfterParagraphTicked = listView1.Items[IndexStartWithUppercaseLetterAfterParagraph].Checked;
            ce.StartWithUppercaseLetterAfterPeriodInsideParagraphTicked = listView1.Items[IndexStartWithUppercaseLetterAfterPeriodInsideParagraph].Checked;
            ce.StartWithUppercaseLetterAfterColonTicked = listView1.Items[IndexStartWithUppercaseLetterAfterColon].Checked;
            ce.AddMissingQuotesTicked = listView1.Items[IndexAddMissingQuotes].Checked;
            ce.FixHyphensTicked = listView1.Items[IndexFixHyphens].Checked;
            ce.FixHyphensAddTicked = listView1.Items[IndexFixHyphensAdd].Checked;
            ce.Fix3PlusLinesTicked = listView1.Items[IndexFix3PlusLines].Checked;
            ce.FixDoubleDashTicked = listView1.Items[IndexFixDoubleDash].Checked;
            ce.FixDoubleGreaterThanTicked = listView1.Items[IndexFixDoubleGreaterThan].Checked;
            ce.FixEllipsesStartTicked = listView1.Items[IndexFixEllipsesStart].Checked;
            ce.FixMissingOpenBracketTicked = listView1.Items[IndexFixMissingOpenBracket].Checked;
            ce.AloneLowercaseIToUppercaseIEnglishTicked = listView1.Items[IndexAloneLowercaseIToUppercaseIEnglish].Checked;
            ce.FixOcrErrorsViaReplaceListTicked = listView1.Items[IndexFixOcrErrorsViaReplaceList].Checked;
            ce.RemoveSpaceBetweenNumberTicked = listView1.Items[IndexRemoveSpaceBetweenNumbers].Checked;
            ce.FixDialogsOnOneLineTicked = listView1.Items[IndexDialogsOnOneLine].Checked;
            if (_danishLetterIIndex > -1)
                ce.DanishLetterITicked = listView1.Items[_danishLetterIIndex].Checked;
            if (_turkishAnsiIndex > -1)
                ce.TurkishAnsiTicked = listView1.Items[_turkishAnsiIndex].Checked;
            if (_spanishInvertedQuestionAndExclamationMarksIndex > -1)
                ce.SpanishInvertedQuestionAndExclamationMarksTicked = listView1.Items[_spanishInvertedQuestionAndExclamationMarksIndex].Checked;

            Configuration.Settings.Save();
        }

        private void ButtonBackClick(object sender, EventArgs e)
        {
            buttonNextFinish.Enabled = true;
            _totalFixes = 0;
            _onlyListFixes = true;
            buttonBack.Enabled = false;
            buttonNextFinish.Text = _language.Next;
            groupBox2.Visible = false;
            groupBoxStep1.Visible = true;
            ShowStatus(string.Empty);
            listViewFixes.Items.Clear();
        }

        private void ButtonCancelClick(object sender, EventArgs e)
        {
            SaveConfiguration();
        }

        private void ListViewFixesColumnClick(object sender, ColumnClickEventArgs e)
        {
            ListViewSorter sorter = (ListViewSorter)listViewFixes.ListViewItemSorter;

            if (e.Column == sorter.ColumnNumber)
            {
                sorter.Descending = !sorter.Descending; // inverse sort direction
            }
            else
            {
                sorter.ColumnNumber = e.Column;
                sorter.Descending = false;
                sorter.IsNumber = e.Column == 1; // only index 1 is numeric
            }
            listViewFixes.Sort();
        }

        private void ButtonSelectAllClick(object sender, EventArgs e)
        {
            foreach (ListViewItem item in listView1.Items)
                item.Checked = true;
        }

        private void ButtonInverseSelectionClick(object sender, EventArgs e)
        {
            foreach (ListViewItem item in listView1.Items)
                item.Checked = !item.Checked;
        }

        private void ListViewFixesSelectedIndexChanged(object sender, EventArgs e)
        {
            if (listViewFixes.SelectedItems.Count > 0)
            {
                var p = (Paragraph)listViewFixes.SelectedItems[0].Tag;

                foreach (ListViewItem lvi in subtitleListView1.Items)
                {
                    Paragraph p2 = lvi.Tag as Paragraph;
                    if (p.ID == p2.ID)
                    {
                        var index = lvi.Index;
                        if (index - 1 > 0)
                            subtitleListView1.EnsureVisible(index - 1);
                        if (index + 1 < subtitleListView1.Items.Count)
                            subtitleListView1.EnsureVisible(index + 1);
                        subtitleListView1.SelectedIndexChanged -= SubtitleListView1SelectedIndexChanged;
                        subtitleListView1.SelectNone();
                        subtitleListView1.SelectedIndexChanged += SubtitleListView1SelectedIndexChanged;
                        subtitleListView1.Items[index].Selected = true;
                        subtitleListView1.EnsureVisible(index);
                        return;
                    }
                }
            }
        }

        private void SubtitleListView1SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_originalSubtitle.Paragraphs.Count > 0)
            {
                int firstSelectedIndex = 0;
                if (subtitleListView1.SelectedItems.Count > 0)
                    firstSelectedIndex = subtitleListView1.SelectedItems[0].Index;

                Paragraph p = GetParagraphOrDefault(firstSelectedIndex);
                if (p != null)
                {
                    textBoxListViewText.TextChanged -= TextBoxListViewTextTextChanged;
                    InitializeListViewEditBox(p);
                    textBoxListViewText.TextChanged += TextBoxListViewTextTextChanged;

                    _subtitleListViewIndex = firstSelectedIndex;
                    UpdateOverlapErrors();
                    UpdateListViewTextInfo(p.Text);
                }
            }
        }

        private void TextBoxListViewTextTextChanged(object sender, EventArgs e)
        {
            if (_subtitleListViewIndex >= 0)
            {
                string text = textBoxListViewText.Text.TrimEnd();
                UpdateListViewTextInfo(text);

                // update _subtitle + listview
                _originalSubtitle.Paragraphs[_subtitleListViewIndex].Text = text;
                subtitleListView1.SetText(_subtitleListViewIndex, text);

                EnableOKButton();
                UpdateListSyntaxColoring();
            }
        }

        private void EnableOKButton()
        {
            if (!_hasFixesBeenMade)
            {
                _hasFixesBeenMade = true;
                buttonNextFinish.Enabled = true;
            }
        }

        private Paragraph GetParagraphOrDefault(int index)
        {
            if (_originalSubtitle.Paragraphs == null || _originalSubtitle.Paragraphs.Count <= index || index < 0)
                return null;

            return _originalSubtitle.Paragraphs[index];
        }

        private void InitializeListViewEditBox(Paragraph p)
        {
            textBoxListViewText.TextChanged -= TextBoxListViewTextTextChanged;
            textBoxListViewText.Text = p.Text;
            textBoxListViewText.TextChanged += TextBoxListViewTextTextChanged;

            timeUpDownStartTime.MaskedTextBox.TextChanged -= MaskedTextBox_TextChanged;
            timeUpDownStartTime.TimeCode = p.StartTime;
            timeUpDownStartTime.MaskedTextBox.TextChanged += MaskedTextBox_TextChanged;

            numericUpDownDuration.ValueChanged -= NumericUpDownDurationValueChanged;
            numericUpDownDuration.Value = (decimal)(p.Duration.TotalMilliseconds / 1000.0);
            numericUpDownDuration.ValueChanged += NumericUpDownDurationValueChanged;
        }

        private void NumericUpDownDurationValueChanged(object sender, EventArgs e)
        {
            if (_originalSubtitle.Paragraphs.Count > 0 && subtitleListView1.SelectedItems.Count > 0)
            {
                int firstSelectedIndex = subtitleListView1.SelectedItems[0].Index;

                Paragraph currentParagraph = GetParagraphOrDefault(firstSelectedIndex);
                if (currentParagraph != null)
                {
                    UpdateOverlapErrors();

                    // update _subtitle + listview
                    currentParagraph.EndTime.TotalMilliseconds = currentParagraph.StartTime.TotalMilliseconds + ((double)numericUpDownDuration.Value * 1000.0);
                    subtitleListView1.SetDuration(firstSelectedIndex, currentParagraph);
                }
            }
        }

        private void UpdateOverlapErrors()
        {
            labelStartTimeWarning.Text = string.Empty;
            labelDurationWarning.Text = string.Empty;

            TimeCode startTime = timeUpDownStartTime.TimeCode;
            if (_originalSubtitle.Paragraphs.Count > 0 && subtitleListView1.SelectedItems.Count > 0 && startTime != null)
            {
                int firstSelectedIndex = subtitleListView1.SelectedItems[0].Index;

                Paragraph prevParagraph = GetParagraphOrDefault(firstSelectedIndex - 1);
                if (prevParagraph != null && prevParagraph.EndTime.TotalMilliseconds > startTime.TotalMilliseconds)
                    labelStartTimeWarning.Text = string.Format(_languageGeneral.OverlapPreviousLineX, (prevParagraph.EndTime.TotalMilliseconds - startTime.TotalMilliseconds) / 1000.0);

                Paragraph nextParagraph = GetParagraphOrDefault(firstSelectedIndex + 1);
                if (nextParagraph != null)
                {
                    double durationMilliSeconds = (double)numericUpDownDuration.Value * 1000.0;
                    if (startTime.TotalMilliseconds + durationMilliSeconds > nextParagraph.StartTime.TotalMilliseconds)
                    {
                        labelDurationWarning.Text = string.Format(_languageGeneral.OverlapNextX, ((startTime.TotalMilliseconds + durationMilliSeconds) - nextParagraph.StartTime.TotalMilliseconds) / 1000.0);
                    }

                    if (labelStartTimeWarning.Text.Length == 0 &&
                        startTime.TotalMilliseconds > nextParagraph.StartTime.TotalMilliseconds)
                    {
                        double di = (startTime.TotalMilliseconds - nextParagraph.StartTime.TotalMilliseconds) / 1000.0;
                        labelStartTimeWarning.Text = string.Format(_languageGeneral.OverlapNextX, di);
                    }
                    else if (numericUpDownDuration.Value < 0)
                    {
                        labelDurationWarning.Text = _languageGeneral.Negative;
                    }
                }
            }
            UpdateListSyntaxColoring();
        }

        private void UpdateListSyntaxColoring()
        {
            if (_subtitle == null || _subtitle.Paragraphs.Count == 0 || _subtitleListViewIndex < 0 || _subtitleListViewIndex >= _subtitle.Paragraphs.Count)
                return;

            subtitleListView1.SyntaxColorLine(_subtitle.Paragraphs, _subtitleListViewIndex, _subtitle.Paragraphs[_subtitleListViewIndex]);
            Paragraph next = _subtitle.GetParagraphOrDefault(_subtitleListViewIndex + 1);
            if (next != null)
                subtitleListView1.SyntaxColorLine(_subtitle.Paragraphs, _subtitleListViewIndex + 1, _subtitle.Paragraphs[_subtitleListViewIndex + 1]);
        }

        private void MaskedTextBox_TextChanged(object sender, EventArgs e)
        {
            if (_subtitleListViewIndex >= 0 &&
                timeUpDownStartTime.TimeCode != null &&
                _originalSubtitle.Paragraphs.Count > 0 &&
                subtitleListView1.SelectedItems.Count > 0)
            {
                TimeCode startTime = timeUpDownStartTime.TimeCode;
                labelStartTimeWarning.Text = string.Empty;
                labelDurationWarning.Text = string.Empty;

                UpdateOverlapErrors();

                // update _subtitle + listview
                _originalSubtitle.Paragraphs[_subtitleListViewIndex].EndTime.TotalMilliseconds +=
                    (startTime.TotalMilliseconds - _originalSubtitle.Paragraphs[_subtitleListViewIndex].StartTime.TotalMilliseconds);
                _originalSubtitle.Paragraphs[_subtitleListViewIndex].StartTime = startTime;
                subtitleListView1.SetStartTime(_subtitleListViewIndex, _originalSubtitle.Paragraphs[_subtitleListViewIndex]);
            }
        }

        private void UpdateListViewTextInfo(string text)
        {
            labelTextLineTotal.Text = string.Empty;

            labelTextLineLengths.Text = _languageGeneral.SingleLineLengths;
            labelSingleLine.Left = labelTextLineLengths.Left + labelTextLineLengths.Width - 6;
            Utilities.GetLineLengths(labelSingleLine, text);

            string s = Utilities.RemoveHtmlTags(text).Replace(Environment.NewLine, " ");
            buttonSplitLine.Visible = false;
            if (s.Length < Configuration.Settings.General.SubtitleLineMaximumLength * 1.9)
            {
                labelTextLineTotal.ForeColor = Color.Black;
                labelTextLineTotal.Text = string.Format(_languageGeneral.TotalLengthX, s.Length);
            }
            else if (s.Length < Configuration.Settings.General.SubtitleLineMaximumLength * 2.1)
            {
                labelTextLineTotal.ForeColor = Color.Orange;
                labelTextLineTotal.Text = string.Format(_languageGeneral.TotalLengthX, s.Length);
            }
            else
            {
                labelTextLineTotal.ForeColor = Color.Red;
                labelTextLineTotal.Text = string.Format(_languageGeneral.TotalLengthX, s.Length);
                buttonSplitLine.Visible = true;
            }
        }

        private void ButtonFixesSelectAllClick(object sender, EventArgs e)
        {
            foreach (ListViewItem item in listViewFixes.Items)
                item.Checked = true;
        }

        private void ButtonFixesInverseClick(object sender, EventArgs e)
        {
            foreach (ListViewItem item in listViewFixes.Items)
                item.Checked = !item.Checked;
        }

        private void ButtonFixesApplyClick(object sender, EventArgs e)
        {
            _hasFixesBeenMade = true;
            Cursor = Cursors.WaitCursor;
            ShowStatus(_language.Analysing);

            _subtitleListViewIndex = -1;
            int firstSelectedIndex = 0;
            if (subtitleListView1.SelectedItems.Count > 0)
                firstSelectedIndex = subtitleListView1.SelectedItems[0].Index;

            _numberOfImportantLogMessages = 0;
            _onlyListFixes = false;
            _totalFixes = 0;
            _totalErrors = 0;
            RunSelectedActions();
            _originalSubtitle = new Subtitle(_subtitle);
            subtitleListView1.Fill(_originalSubtitle);
            RefreshFixes();
            if (listViewFixes.Items.Count == 0)
            {
                subtitleListView1.SelectIndexAndEnsureVisible(firstSelectedIndex);
            }
            if (_totalFixes == 0 && _totalErrors == 0)
                ShowStatus(_language.NothingToFix);
            else if (_totalFixes > 0)
                ShowStatus(string.Format(_language.XFixesApplied, _totalFixes));
            else if (_totalErrors > 0)
                ShowStatus(_language.NothingToFixBut);

            Cursor = Cursors.Default;
            if (_numberOfImportantLogMessages == 0)
                labelNumberOfImportantLogMessages.Text = string.Empty;
            else
                labelNumberOfImportantLogMessages.Text = string.Format(_language.NumberOfImportantLogMessages, _numberOfImportantLogMessages);
        }

        private void ButtonRefreshFixesClick(object sender, EventArgs e)
        {
            Cursor = Cursors.WaitCursor;
            ShowStatus(_language.Analysing);
            _totalFixes = 0;
            RefreshFixes();

            ShowAvailableFixesStatus();

            Cursor = Cursors.Default;
        }

        private void ShowAvailableFixesStatus()
        {
            if (_totalFixes == 0 && _totalErrors == 0)
            {
                ShowStatus(_language.NothingToFix);
                if (subtitleListView1.SelectedItems.Count == 0)
                    subtitleListView1.SelectIndexAndEnsureVisible(0);
            }
            else if (_totalFixes > 0)
                ShowStatus(string.Format(_language.FixesFoundX, _totalFixes));
            else if (_totalErrors > 0)
                ShowStatus(_language.NothingToFixBut);

            TopMost = true;
            BringToFront();
            TopMost = false;
        }

        private void RefreshFixes()
        {
            // save de-seleced fixes
            var deSelectedFixes = new List<string>();
            foreach (ListViewItem item in listViewFixes.Items)
            {
                if (!item.Checked)
                    deSelectedFixes.Add(item.SubItems[1].Text + item.SubItems[2].Text + item.SubItems[3].Text);
            }

            listViewFixes.Items.Clear();
            _onlyListFixes = true;
            Next();

            // restore de-selected fixes
            foreach (ListViewItem item in listViewFixes.Items)
            {
                if (deSelectedFixes.Contains(item.SubItems[1].Text + item.SubItems[2].Text + item.SubItems[3].Text))
                    item.Checked = false;
            }
        }

        private void ButtonAutoBreakClick(object sender, EventArgs e)
        {
            if (textBoxListViewText.Text.Length > 0)
            {
                string oldText = textBoxListViewText.Text;
                textBoxListViewText.Text = Utilities.AutoBreakLine(textBoxListViewText.Text, Language);
                if (oldText != textBoxListViewText.Text)
                    EnableOKButton();
            }
        }

        private void ButtonUnBreakClick(object sender, EventArgs e)
        {
            string oldText = textBoxListViewText.Text;
            textBoxListViewText.Text = Utilities.UnbreakLine(textBoxListViewText.Text);
            if (oldText != textBoxListViewText.Text)
                EnableOKButton();
        }

        private void ToolStripMenuItemDeleteClick(object sender, EventArgs e)
        {
            if (_originalSubtitle.Paragraphs.Count > 0 && subtitleListView1.SelectedItems.Count > 0)
            {
                _subtitleListViewIndex = -1;

                var indexes = new List<int>();
                foreach (ListViewItem item in subtitleListView1.SelectedItems)
                    indexes.Add(item.Index);
                int firstIndex = subtitleListView1.SelectedItems[0].Index;

                int startNumber = _originalSubtitle.Paragraphs[0].Number;
                if (startNumber == 2)
                    startNumber = 1;

                // save de-seleced fixes
                List<string> deSelectedFixes = new List<string>();
                foreach (ListViewItem item in listViewFixes.Items)
                {
                    if (!item.Checked)
                    {
                        int number = Convert.ToInt32(item.SubItems[1].Text);
                        if (number > firstIndex)
                            number -= subtitleListView1.SelectedItems.Count;
                        deSelectedFixes.Add(number + item.SubItems[2].Text + item.SubItems[3].Text);
                    }
                }

                indexes.Reverse();
                foreach (int i in indexes)
                {
                    _originalSubtitle.Paragraphs.RemoveAt(i);
                }
                _originalSubtitle.Renumber(startNumber);
                subtitleListView1.Fill(_originalSubtitle);
                if (subtitleListView1.Items.Count > firstIndex)
                {
                    subtitleListView1.Items[firstIndex].Selected = true;
                }
                else if (subtitleListView1.Items.Count > 0)
                {
                    subtitleListView1.Items[subtitleListView1.Items.Count - 1].Selected = true;
                }

                // refresh fixes
                listViewFixes.Items.Clear();
                _onlyListFixes = true;
                Next();

                // restore de-selected fixes
                foreach (ListViewItem item in listViewFixes.Items)
                {
                    if (deSelectedFixes.Contains(item.SubItems[1].Text + item.SubItems[2].Text + item.SubItems[3].Text))
                        item.Checked = false;
                }

            }
        }

        private void MergeSelectedLinesToolStripMenuItemClick(object sender, EventArgs e)
        {
            if (_originalSubtitle.Paragraphs.Count > 0 && subtitleListView1.SelectedItems.Count > 0)
            {
                int startNumber = _originalSubtitle.Paragraphs[0].Number;
                int firstSelectedIndex = subtitleListView1.SelectedItems[0].Index;

                // save de-seleced fixes
                List<string> deSelectedFixes = new List<string>();
                foreach (ListViewItem item in listViewFixes.Items)
                {
                    if (!item.Checked)
                    {
                        int firstSelectedNumber = subtitleListView1.GetSelectedParagraph(_originalSubtitle).Number;
                        int number = Convert.ToInt32(item.SubItems[1].Text);
                        if (number > firstSelectedNumber)
                            number--;
                        deSelectedFixes.Add(number + item.SubItems[2].Text + item.SubItems[3].Text);
                    }
                }

                Paragraph currentParagraph = _originalSubtitle.GetParagraphOrDefault(firstSelectedIndex);
                Paragraph nextParagraph = _originalSubtitle.GetParagraphOrDefault(firstSelectedIndex + 1);

                if (nextParagraph != null && currentParagraph != null)
                {
                    subtitleListView1.SelectedIndexChanged -= SubtitleListView1SelectedIndexChanged;

                    currentParagraph.Text = currentParagraph.Text.Replace(Environment.NewLine, " ");
                    currentParagraph.Text += Environment.NewLine + nextParagraph.Text.Replace(Environment.NewLine, " ");
                    currentParagraph.EndTime = nextParagraph.EndTime;

                    _originalSubtitle.Paragraphs.Remove(nextParagraph);

                    _originalSubtitle.Renumber(startNumber);
                    subtitleListView1.Fill(_originalSubtitle);
                    subtitleListView1.SelectIndexAndEnsureVisible(firstSelectedIndex);
                    subtitleListView1.SelectedIndexChanged += SubtitleListView1SelectedIndexChanged;
                    _subtitleListViewIndex = -1;
                    SubtitleListView1SelectedIndexChanged(null, null);
                }

                // refresh fixes
                listViewFixes.Items.Clear();
                _onlyListFixes = true;
                Next();

                // restore de-selected fixes
                foreach (ListViewItem item in listViewFixes.Items)
                {
                    if (deSelectedFixes.Contains(item.SubItems[1].Text + item.SubItems[2].Text + item.SubItems[3].Text))
                        item.Checked = false;
                }
            }
        }

        private void ContextMenuStripListviewOpening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (subtitleListView1.SelectedItems.Count == 0)
            {
                e.Cancel = true;
            }
            else if (subtitleListView1.SelectedItems.Count == 2 &&
                     subtitleListView1.SelectedItems[0].Index == subtitleListView1.SelectedItems[1].Index - 1)
            {

                mergeSelectedLinesToolStripMenuItem.Visible = true;
                toolStripSeparator1.Visible = true;
            }
            else
            {
                mergeSelectedLinesToolStripMenuItem.Visible = false;
                toolStripSeparator1.Visible = false;
            }
        }

        private void FixCommonErrorsResize(object sender, EventArgs e)
        {
            groupBox2.Width = Width - (groupBox2.Left * 2 + 15);
            groupBoxStep1.Width = Width - (groupBoxStep1.Left * 2 + 15);
            buttonCancel.Left = Width - (buttonCancel.Width + 26);
            buttonNextFinish.Left = buttonCancel.Left - (buttonNextFinish.Width + 6);
            buttonBack.Left = buttonNextFinish.Left - (buttonBack.Width + 6);
            tabControl1.Width = groupBox2.Width - (tabControl1.Left * 2);
            listView1.Width = groupBoxStep1.Width - (listView1.Left * 2);

            ListViewFixesAutoSizeAllColumns();
            subtitleListView1.AutoSizeAllColumns(this);
        }

        public void ListViewFixesAutoSizeAllColumns()
        {
            Graphics graphics = CreateGraphics();

            SizeF timestampSizeF = graphics.MeasureString(listViewFixes.Columns[0].Text, Font); // Apply
            int width = (int)(timestampSizeF.Width + 12);
            listViewFixes.Columns[0].Width = width;

            timestampSizeF = graphics.MeasureString(listViewFixes.Columns[1].Text, Font); // line#
            width = (int)(timestampSizeF.Width + 12);
            listViewFixes.Columns[1].Width = width;

            timestampSizeF = graphics.MeasureString("Auto break all lines and even more stuff", Font); // Function
            width = (int)(timestampSizeF.Width + 12);
            listViewFixes.Columns[2].Width = width;

            int length = listViewFixes.Columns[0].Width + listViewFixes.Columns[1].Width + listViewFixes.Columns[2].Width;
            int lengthAvailable = Width - length;
            width = (lengthAvailable - 10) / 2;
            listViewFixes.Columns[3].Width = width; // before
            listViewFixes.Columns[4].Width = width; // after
            graphics.Dispose();
        }

        private void FixCommonErrorsShown(object sender, EventArgs e)
        {
            FixCommonErrorsResize(null, null);
        }

        private void SplitSelectedParagraph(double? splitSeconds)
        {
            if (_originalSubtitle.Paragraphs.Count > 0 && subtitleListView1.SelectedItems.Count > 0)
            {
                subtitleListView1.SelectedIndexChanged -= SubtitleListView1SelectedIndexChanged;
                int firstSelectedIndex = subtitleListView1.SelectedItems[0].Index;

                // save de-seleced fixes
                List<string> deSelectedFixes = new List<string>();
                foreach (ListViewItem item in listViewFixes.Items)
                {
                    if (!item.Checked)
                    {
                        int number = Convert.ToInt32(item.SubItems[1].Text);
                        if (number > firstSelectedIndex)
                            number++;
                        deSelectedFixes.Add(number + item.SubItems[2].Text + item.SubItems[3].Text);
                    }
                }

                Paragraph currentParagraph = _originalSubtitle.GetParagraphOrDefault(firstSelectedIndex);
                var newParagraph = new Paragraph();

                string oldText = currentParagraph.Text;
                string[] lines = currentParagraph.Text.Split(Utilities.NewLineChars, StringSplitOptions.RemoveEmptyEntries);
                if (lines.Length == 2 && (lines[0].EndsWith('.') || lines[0].EndsWith('!') || lines[0].EndsWith('?')))
                {
                    currentParagraph.Text = Utilities.AutoBreakLine(lines[0], Language);
                    newParagraph.Text = Utilities.AutoBreakLine(lines[1], Language);
                }
                else
                {
                    string s = Utilities.AutoBreakLine(currentParagraph.Text, 5, Configuration.Settings.Tools.MergeLinesShorterThan, Language);
                    lines = s.Split(Utilities.NewLineChars, StringSplitOptions.RemoveEmptyEntries);
                    if (lines.Length == 2)
                    {
                        currentParagraph.Text = Utilities.AutoBreakLine(lines[0], Language);
                        newParagraph.Text = Utilities.AutoBreakLine(lines[1], Language);
                    }
                }

                double startFactor = (double)Utilities.RemoveHtmlTags(currentParagraph.Text).Length / Utilities.RemoveHtmlTags(oldText).Length;
                if (startFactor < 0.20)
                    startFactor = 0.20;
                if (startFactor > 0.80)
                    startFactor = 0.80;

                double middle = currentParagraph.StartTime.TotalMilliseconds + (currentParagraph.Duration.TotalMilliseconds * startFactor);
                if (splitSeconds.HasValue && splitSeconds.Value > (currentParagraph.StartTime.TotalSeconds + 0.2) && splitSeconds.Value < (currentParagraph.EndTime.TotalSeconds - 0.2))
                    middle = splitSeconds.Value * 1000.0;
                newParagraph.EndTime.TotalMilliseconds = currentParagraph.EndTime.TotalMilliseconds;
                currentParagraph.EndTime.TotalMilliseconds = middle;
                newParagraph.StartTime.TotalMilliseconds = currentParagraph.EndTime.TotalMilliseconds + 1;

                _originalSubtitle.Paragraphs.Insert(firstSelectedIndex + 1, newParagraph);
                _originalSubtitle.Renumber(1);
                subtitleListView1.Fill(_originalSubtitle);
                textBoxListViewText.Text = currentParagraph.Text;

                subtitleListView1.SelectIndexAndEnsureVisible(firstSelectedIndex);
                subtitleListView1.SelectedIndexChanged += SubtitleListView1SelectedIndexChanged;

                // restore de-selected fixes
                listViewFixes.Items.Clear();
                _onlyListFixes = true;
                Next();
                foreach (ListViewItem item in listViewFixes.Items)
                {
                    if (deSelectedFixes.Contains(item.SubItems[1].Text + item.SubItems[2].Text + item.SubItems[3].Text))
                        item.Checked = false;
                }
            }
        }

        private void ButtonSplitLineClick(object sender, EventArgs e)
        {
            SplitSelectedParagraph(null);
        }

        private void TextBoxListViewTextKeyDown(object sender, KeyEventArgs e)
        {
            int numberOfNewLines = textBoxListViewText.Text.Length - textBoxListViewText.Text.Replace(Environment.NewLine, " ").Length;
            Utilities.CheckAutoWrap(textBoxListViewText, e, numberOfNewLines);
        }

        private void FixCommonErrorsFormClosing(object sender, FormClosingEventArgs e)
        {
            Owner = null;
        }

        private void comboBoxLanguage_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_subtitle != null)
            {
                var ci = (CultureInfo)comboBoxLanguage.SelectedItem;
                _autoDetectGoogleLanguage = ci.TwoLetterISOLanguageName;
                AddFixActions(ci.ThreeLetterISOLanguageName);
            }
        }

        private void comboBoxLanguage_Enter(object sender, EventArgs e)
        {
            SaveConfiguration();
        }

    }
}