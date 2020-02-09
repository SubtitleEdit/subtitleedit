using Nikse.SubtitleEdit.Core;
using Nikse.SubtitleEdit.Core.Dictionaries;
using Nikse.SubtitleEdit.Core.Forms.FixCommonErrors;
using Nikse.SubtitleEdit.Core.Interfaces;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Ocr;
using Nikse.SubtitleEdit.Logic.SpellCheck;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.Forms
{
    public sealed partial class FixCommonErrors : Form, IFixCallbacks
    {
        private const int IndexRemoveEmptyLines = 0;
        private const int IndexOverlappingDisplayTime = 1;
        private const int IndexTooShortDisplayTime = 2;
        private const int IndexTooLongDisplayTime = 3;
        private const int IndexTooShortGap = 4;
        private const int IndexInvalidItalicTags = 5;
        private const int IndexUnneededSpaces = 6;
        private const int IndexUnneededPeriods = 7;
        private const int IndexMissingSpaces = 8;
        private const int IndexBreakLongLines = 9;
        private const int IndexMergeShortLines = 10;
        private const int IndexMergeShortLinesAll = 11;
        private const int IndexDoubleApostropheToQuote = 12;
        private const int IndexFixMusicNotation = 13;
        private const int IndexAddPeriodAfterParagraph = 14;
        private const int IndexStartWithUppercaseLetterAfterParagraph = 15;
        private const int IndexStartWithUppercaseLetterAfterPeriodInsideParagraph = 16;
        private const int IndexStartWithUppercaseLetterAfterColon = 17;
        private const int IndexAddMissingQuotes = 18;
        private const int IndexFixHyphensAdd = 19;
        private const int IndexFixHyphens = 20;
        private const int IndexFix3PlusLines = 21;
        private const int IndexFixDoubleDash = 22;
        private const int IndexFixDoubleGreaterThan = 23;
        private const int IndexFixEllipsesStart = 24;
        private const int IndexFixMissingOpenBracket = 25;
        private const int IndexFixOcrErrorsViaReplaceList = 26;
        private const int IndexUppercaseIInsideLowercaseWord = 27;
        private const int IndexRemoveSpaceBetweenNumbers = 28;
        private const int IndexDialogsOnOneLine = 29;
        private int _indexAloneLowercaseIToUppercaseIEnglish = -1;
        private int _turkishAnsiIndex = -1;
        private int _danishLetterIIndex = -1;
        private int _spanishInvertedQuestionAndExclamationMarksIndex = -1;

        private readonly LanguageStructure.FixCommonErrors _language;
        private readonly LanguageStructure.General _languageGeneral;
        private bool _hasFixesBeenMade;

        private readonly Keys _goToLine = UiUtil.GetKeys(Configuration.Settings.Shortcuts.MainEditGoToLineNumber);
        private readonly Keys _preview = UiUtil.GetKeys(Configuration.Settings.Shortcuts.MainToolsFixCommonErrorsPreview);
        private readonly Keys _mainGeneralGoToNextSubtitle = UiUtil.GetKeys(Configuration.Settings.Shortcuts.GeneralGoToNextSubtitle);
        private readonly Keys _mainGeneralGoToPrevSubtitle = UiUtil.GetKeys(Configuration.Settings.Shortcuts.GeneralGoToPrevSubtitle);
        private readonly Keys _mainListViewGoToNextError = UiUtil.GetKeys(Configuration.Settings.Shortcuts.MainListViewGoToNextError);

        private class FixItem
        {
            public string Name { get; }
            public string Example { get; }
            public Action Action { get; }
            public bool DefaultChecked { get; }

            public FixItem(string name, string example, Action action, bool selected)
            {
                Name = name;
                Example = example;
                Action = action;
                DefaultChecked = selected;
            }
        }

        public class LanguageItem
        {
            public CultureInfo Code { get; }
            public string Name { get; }

            public LanguageItem(CultureInfo code, string name)
            {
                Code = code;
                Name = name;
            }

            public override string ToString()
            {
                return Name;
            }
        }

        private SubtitleFormat _format;
        private int _totalFixes;
        private int _totalErrors;
        private List<FixItem> _fixActions;
        private int _subtitleListViewIndex = -1;
        private bool _onlyListFixes = true;
        private string _autoDetectGoogleLanguage;
        private HashSet<string> _nameList;
        private HashSet<string> _abbreviationList;
        private readonly StringBuilder _newLog = new StringBuilder();
        private readonly StringBuilder _appliedLog = new StringBuilder();
        private int _numberOfImportantLogMessages;
        private HashSet<string> _allowedFixes;
        private bool _linesDeletedOrMerged;

        public SubtitleFormat Format => _format;

        public Encoding Encoding { get; private set; }
        public Subtitle Subtitle { get; private set; }
        public Subtitle FixedSubtitle { get; private set; }

        public void AddToTotalErrors(int count)
        {
            _totalErrors += count;
        }

        public void AddToDeleteIndices(int index)
        {
        }

        private void InitializeLanguageNames(LanguageItem firstItem = null)
        {
            comboBoxLanguage.BeginUpdate();
            comboBoxLanguage.Items.Clear();
            if (firstItem != null)
            {
                comboBoxLanguage.Items.Add(firstItem);
            }
            foreach (var ci in Utilities.GetSubtitleLanguageCultures())
            {
                comboBoxLanguage.Items.Add(new LanguageItem(ci, ci.EnglishName));
            }
            comboBoxLanguage.Sorted = true;
            comboBoxLanguage.EndUpdate();
        }

        public void RunBatchSettings(Subtitle subtitle, SubtitleFormat format, TextEncoding encoding, string language)
        {
            _autoDetectGoogleLanguage = language;
            var ci = CultureInfo.GetCultureInfo(_autoDetectGoogleLanguage);
            string threeLetterIsoLanguageName = ci.ThreeLetterISOLanguageName;

            InitializeLanguageNames(new LanguageItem(null, "-Auto-"));
            int languageIndex = 0;
            int j = 0;
            foreach (var x in comboBoxLanguage.Items)
            {
                if (x is LanguageItem xci)
                {
                    if (xci.Code != null && xci.Code.TwoLetterISOLanguageName == ci.TwoLetterISOLanguageName)
                    {
                        languageIndex = j;
                        break;
                    }
                    if (xci.Code != null && xci.Code.TwoLetterISOLanguageName == "en")
                    {
                        languageIndex = j;
                    }
                }
                j++;
            }
            if (string.IsNullOrEmpty(language))
            {
                languageIndex = 0;
            }

            comboBoxLanguage.SelectedIndex = languageIndex;
            AddFixActions(threeLetterIsoLanguageName);
            FixedSubtitle = new Subtitle(subtitle, false); // copy constructor
            Subtitle = new Subtitle(subtitle, false); // copy constructor
            _format = format;
            Encoding = encoding.Encoding;
            _onlyListFixes = false;
            InitUserInterface();
            groupBoxStep1.Text = string.Empty;
            buttonBack.Visible = false;
            buttonNextFinish.Visible = false;
            buttonCancel.Text = _languageGeneral.Ok;
        }

        public string Language
        {
            get
            {
                var ci = comboBoxLanguage.SelectedItem as LanguageItem;
                return ci?.Code == null ? string.Empty : ci.Code.TwoLetterISOLanguageName;
            }
            set
            {
                for (int index = 0; index < comboBoxLanguage.Items.Count; index++)
                {
                    var item = comboBoxLanguage.Items[index];
                    if (item.ToString() == value)
                    {
                        comboBoxLanguage.SelectedIndex = index;
                    }
                }
            }
        }

        public bool BatchMode { get; set; }

        public void RunBatch(Subtitle subtitle, SubtitleFormat format, Encoding encoding, string language)
        {
            _autoDetectGoogleLanguage = language;
            var ci = CultureInfo.GetCultureInfo(_autoDetectGoogleLanguage);
            string threeLetterIsoLanguageName = ci.ThreeLetterISOLanguageName;
            InitializeLanguageNames();
            int languageIndex = 0;
            int j = 0;
            foreach (var x in comboBoxLanguage.Items)
            {
                var xci = (LanguageItem)x;
                if (xci.Code.TwoLetterISOLanguageName == ci.TwoLetterISOLanguageName)
                {
                    languageIndex = j;
                    break;
                }
                if (xci.Code.TwoLetterISOLanguageName == "en")
                {
                    languageIndex = j;
                }
                j++;
            }
            comboBoxLanguage.SelectedIndex = languageIndex;

            AddFixActions(threeLetterIsoLanguageName);

            FixedSubtitle = new Subtitle(subtitle, false); // copy constructor
            Subtitle = new Subtitle(subtitle, false); // copy constructor
            _format = format;
            Encoding = encoding;
            _onlyListFixes = true;
            _hasFixesBeenMade = true;
            _numberOfImportantLogMessages = 0;
            _onlyListFixes = false;
            _totalFixes = 0;
            _totalErrors = 0;
            RunSelectedActions();
            FixedSubtitle = Subtitle;
        }

        public void Initialize(Subtitle subtitle, SubtitleFormat format, Encoding encoding)
        {
            _autoDetectGoogleLanguage = LanguageAutoDetect.AutoDetectGoogleLanguage(encoding); // Guess language via encoding
            if (string.IsNullOrEmpty(_autoDetectGoogleLanguage))
            {
                _autoDetectGoogleLanguage = LanguageAutoDetect.AutoDetectGoogleLanguage(subtitle); // Guess language based on subtitle contents
            }

            if (_autoDetectGoogleLanguage.Equals("zh", StringComparison.OrdinalIgnoreCase))
            {
                _autoDetectGoogleLanguage = "zh-CHS"; // Note that "zh-CHS" (Simplified Chinese) and "zh-CHT" (Traditional Chinese) are neutral cultures
            }

            CultureInfo ci = CultureInfo.GetCultureInfo(_autoDetectGoogleLanguage);
            string threeLetterIsoLanguageName = ci.ThreeLetterISOLanguageName;
            InitializeLanguageNames();
            int languageIndex = 0;
            int j = 0;
            foreach (var x in comboBoxLanguage.Items)
            {
                var xci = (LanguageItem)x;
                if (xci.Code.TwoLetterISOLanguageName == ci.TwoLetterISOLanguageName)
                {
                    languageIndex = j;
                    break;
                }
                if (xci.Code.TwoLetterISOLanguageName == "en")
                {
                    languageIndex = j;
                }
                j++;
            }
            comboBoxLanguage.SelectedIndex = languageIndex;

            AddFixActions(threeLetterIsoLanguageName);

            FixedSubtitle = new Subtitle(subtitle, false); // copy constructor
            Subtitle = new Subtitle(subtitle, false); // copy constructor
            _format = format;
            Encoding = encoding;
            InitUserInterface();
        }

        private void InitUserInterface()
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

            UiUtil.InitializeSubtitleFont(textBoxListViewText);
            UiUtil.InitializeSubtitleFont(subtitleListView1);
            listViewFixes.ListViewItemSorter = new ListViewSorter { ColumnNumber = 1, IsNumber = true };

            if (!string.IsNullOrEmpty(Configuration.Settings.CommonErrors.StartSize))
            {
                StartPosition = FormStartPosition.Manual;
                var arr = Configuration.Settings.CommonErrors.StartSize.Split(';');
                if (arr.Length == 2 && int.TryParse(arr[0], out var x) && int.TryParse(arr[1], out var y))
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
                var arr = Configuration.Settings.CommonErrors.StartPosition.Split(';');
                if (arr.Length == 2 && int.TryParse(arr[0], out var x) && int.TryParse(arr[1], out var y))
                {
                    var screen = Screen.FromPoint(Cursor.Position);
                    if (x > 0 && x < screen.WorkingArea.Width && y > 0 && y < screen.WorkingArea.Height)
                    {
                        Left = x;
                        Top = y;
                    }
                    else
                    {
                        StartPosition = FormStartPosition.CenterParent;
                    }
                }
            }

            if (Screen.PrimaryScreen.WorkingArea.Width <= 124)
            {
                Width = MinimumSize.Width;
                Height = MinimumSize.Height;
            }
            if (Configuration.Settings.Tools.FixCommonErrorsSkipStepOne && !BatchMode)
            {
                Next();
            }
            Activate();
        }

        private void AddFixActions(string threeLetterIsoLanguageName)
        {
            _turkishAnsiIndex = -1;
            _danishLetterIIndex = -1;
            _spanishInvertedQuestionAndExclamationMarksIndex = -1;
            _indexAloneLowercaseIToUppercaseIEnglish = -1;

            FixCommonErrorsSettings ce = Configuration.Settings.CommonErrors;
            _fixActions = new List<FixItem>
            {
                new FixItem(_language.RemovedEmptyLinesUnsedLineBreaks, string.Empty, () => new FixEmptyLines().Fix(Subtitle, this), ce.EmptyLinesTicked),
                new FixItem(_language.FixOverlappingDisplayTimes, string.Empty, () => new FixOverlappingDisplayTimes().Fix(Subtitle, this), ce.OverlappingDisplayTimeTicked),
                new FixItem(_language.FixShortDisplayTimes, string.Empty, () => new FixShortDisplayTimes().Fix(Subtitle, this), ce.TooShortDisplayTimeTicked),
                new FixItem(_language.FixLongDisplayTimes, string.Empty, () => new FixLongDisplayTimes().Fix(Subtitle, this), ce.TooLongDisplayTimeTicked),
                new FixItem(_language.FixShortGaps, string.Empty, () => new FixShortGaps().Fix(Subtitle, this), ce.TooShortGapTicked),
                new FixItem(_language.FixInvalidItalicTags, _language.FixInvalidItalicTagsExample, () => new FixInvalidItalicTags().Fix(Subtitle, this), ce.InvalidItalicTagsTicked),
                new FixItem(_language.RemoveUnneededSpaces, _language.RemoveUnneededSpacesExample, () => new FixUnneededSpaces().Fix(Subtitle, this), ce.UnneededSpacesTicked),
                new FixItem(_language.RemoveUnneededPeriods, _language.RemoveUnneededPeriodsExample, () => new FixUnneededPeriods().Fix(Subtitle, this), ce.UnneededPeriodsTicked),
                new FixItem(_language.FixMissingSpaces, _language.FixMissingSpacesExample, () => new FixMissingSpaces().Fix(Subtitle, this), ce.MissingSpacesTicked),
                new FixItem(_language.BreakLongLines, string.Empty, () => new FixLongLines().Fix(Subtitle, this), ce.BreakLongLinesTicked),
                new FixItem(_language.RemoveLineBreaks, string.Empty, () => new FixShortLines().Fix(Subtitle, this), ce.MergeShortLinesTicked),
                new FixItem(_language.RemoveLineBreaksAll, string.Empty, () => new FixShortLinesAll().Fix(Subtitle, this), ce.MergeShortLinesAllTicked),
                new FixItem(_language.FixDoubleApostrophes, string.Empty, () => new FixDoubleApostrophes().Fix(Subtitle, this), ce.DoubleApostropheToQuoteTicked),
                new FixItem(_language.FixMusicNotation, _language.FixMusicNotationExample, () => new FixMusicNotation().Fix(Subtitle, this), ce.FixMusicNotationTicked),
                new FixItem(_language.AddPeriods, string.Empty, () => new FixMissingPeriodsAtEndOfLine().Fix(Subtitle, this), ce.AddPeriodAfterParagraphTicked),
                new FixItem(_language.StartWithUppercaseLetterAfterParagraph, string.Empty, () => new FixStartWithUppercaseLetterAfterParagraph().Fix(Subtitle, this) , ce.StartWithUppercaseLetterAfterParagraphTicked),
                new FixItem(_language.StartWithUppercaseLetterAfterPeriodInsideParagraph, string.Empty, () => new FixStartWithUppercaseLetterAfterPeriodInsideParagraph().Fix(Subtitle, this) , ce.StartWithUppercaseLetterAfterPeriodInsideParagraphTicked),
                new FixItem(_language.StartWithUppercaseLetterAfterColon, string.Empty, () => new FixStartWithUppercaseLetterAfterColon().Fix(Subtitle, this), ce.StartWithUppercaseLetterAfterColonTicked),
                new FixItem(_language.AddMissingQuotes, _language.AddMissingQuotesExample, () => new AddMissingQuotes().Fix(Subtitle, this), ce.AddMissingQuotesTicked),
                new FixItem(_language.FixHyphensAdd, string.Empty, () => new FixHyphensAdd().Fix(Subtitle, this), ce.FixHyphensAddTicked),
                new FixItem(_language.FixHyphens, string.Empty, () => new FixHyphensRemove().Fix(Subtitle, this), ce.FixHyphensTicked),
                new FixItem(_language.Fix3PlusLines, string.Empty, () => new Fix3PlusLines().Fix(Subtitle, this), ce.Fix3PlusLinesTicked),
                new FixItem(_language.FixDoubleDash, _language.FixDoubleDashExample, () => new FixDoubleDash().Fix(Subtitle, this), ce.FixDoubleDashTicked),
                new FixItem(_language.FixDoubleGreaterThan, _language.FixDoubleGreaterThanExample, () => new FixDoubleGreaterThan().Fix(Subtitle, this), ce.FixDoubleGreaterThanTicked),
                new FixItem(_language.FixEllipsesStart, _language.FixEllipsesStartExample, () => new FixEllipsesStart().Fix(Subtitle, this), ce.FixEllipsesStartTicked),
                new FixItem(_language.FixMissingOpenBracket, _language.FixMissingOpenBracketExample, () => new FixMissingOpenBracket().Fix(Subtitle, this), ce.FixMissingOpenBracketTicked),
                new FixItem(_language.FixCommonOcrErrors, _language.FixOcrErrorExample, () => FixOcrErrorsViaReplaceList(threeLetterIsoLanguageName), ce.FixOcrErrorsViaReplaceListTicked),
                new FixItem(_language.FixUppercaseIInsindeLowercaseWords, _language.FixUppercaseIInsindeLowercaseWordsExample, () => new FixUppercaseIInsideWords().Fix(Subtitle, this), ce.UppercaseIInsideLowercaseWordTicked),
                new FixItem(_language.RemoveSpaceBetweenNumber, _language.FixSpaceBetweenNumbersExample, () => new RemoveSpaceBetweenNumbers().Fix(Subtitle, this), ce.RemoveSpaceBetweenNumberTicked),
                new FixItem(_language.FixDialogsOnOneLine, _language.FixDialogsOneLineExample, () => new FixDialogsOnOneLine().Fix(Subtitle, this), ce.FixDialogsOnOneLineTicked)
            };

            if (Language == "en")
            {
                _indexAloneLowercaseIToUppercaseIEnglish = _fixActions.Count;
                _fixActions.Add(new FixItem(_language.FixLowercaseIToUppercaseI, _language.FixLowercaseIToUppercaseIExample, () => new FixAloneLowercaseIToUppercaseI().Fix(Subtitle, this), ce.AloneLowercaseIToUppercaseIEnglishTicked));
            }
            if (Language == "tr")
            {
                _turkishAnsiIndex = _fixActions.Count;
                _fixActions.Add(new FixItem(_language.FixTurkishAnsi, "Ý > İ, Ð > Ğ, Þ > Ş, ý > ı, ð > ğ, þ > ş", () => new FixTurkishAnsiToUnicode().Fix(Subtitle, this), ce.TurkishAnsiTicked));
            }

            if (Language == "da")
            {
                _danishLetterIIndex = _fixActions.Count;
                _fixActions.Add(new FixItem(_language.FixDanishLetterI, "Jeg synes i er søde. -> Jeg synes I er søde.", () => new FixDanishLetterI().Fix(Subtitle, this), ce.DanishLetterITicked));
            }

            if (Language == "es")
            {
                _spanishInvertedQuestionAndExclamationMarksIndex = _fixActions.Count;
                _fixActions.Add(new FixItem(_language.FixSpanishInvertedQuestionAndExclamationMarks, "Hablas bien castellano? -> ¿Hablas bien castellano?", () => new FixSpanishInvertedQuestionAndExclamationMarks().Fix(Subtitle, this), ce.SpanishInvertedQuestionAndExclamationMarksTicked));
            }

            listView1.Items.Clear();
            foreach (var fi in _fixActions)
            {
                AddFixActionItemToListView(fi);
            }
        }

        public FixCommonErrors()
        {
            UiUtil.PreInitialize(this);
            InitializeComponent();
            UiUtil.FixFonts(this);

            labelStartTimeWarning.Text = string.Empty;
            labelDurationWarning.Text = string.Empty;
            labelNumberOfImportantLogMessages.Text = string.Empty;
            Encoding = Encoding.UTF8;
            _language = Configuration.Settings.Language.FixCommonErrors;
            _languageGeneral = Configuration.Settings.Language.General;
            Text = _language.Title;
            groupBoxStep1.Text = _language.Step1;
            groupBox2.Text = _language.Step2;
            listView1.Columns[0].Text = _languageGeneral.Apply;
            listView1.Columns[1].Text = _language.WhatToFix;
            listView1.Columns[2].Text = _language.Example;
            buttonSelectAll.Text = _language.SelectAll;
            buttonInverseSelection.Text = _language.InverseSelection;
            tabControl1.TabPages[0].Text = _language.Fixes;
            tabControl1.TabPages[1].Text = _language.Log;
            listViewFixes.Columns[0].Text = _languageGeneral.Apply;
            listViewFixes.Columns[1].Text = _languageGeneral.LineNumber;
            listViewFixes.Columns[2].Text = _language.Function;
            listViewFixes.Columns[3].Text = _languageGeneral.Before;
            listViewFixes.Columns[4].Text = _languageGeneral.After;
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
            buttonSplitLine.Text = _languageGeneral.SplitLine;
            subtitleListView1.InitializeLanguage(_languageGeneral, Configuration.Settings);
            labelLanguage.Text = Configuration.Settings.Language.ChooseLanguage.Language;
            toolStripMenuItemDelete.Text = Configuration.Settings.Language.Main.Menu.ContextMenu.Delete;
            mergeSelectedLinesToolStripMenuItem.Text = Configuration.Settings.Language.Main.Menu.ContextMenu.MergeSelectedLines;
            buttonResetDefault.Text = _language.SelectDefault;

            splitContainerStep2.Panel1MinSize = 110;
            splitContainerStep2.Panel2MinSize = 160;

            numericUpDownDuration.Left = timeUpDownStartTime.Left + timeUpDownStartTime.Width;
            labelDuration.Left = timeUpDownStartTime.Left + timeUpDownStartTime.Width - 3;
            FixLargeFonts();
            listView1.Select();
        }

        private void FixLargeFonts()
        {
            using (var graphics = CreateGraphics())
            {
                var textSize = graphics.MeasureString(buttonCancel.Text, Font);
                if (textSize.Height > buttonCancel.Height - 4)
                {
                    subtitleListView1.InitializeTimestampColumnWidths(this);
                    var newButtonHeight = (int)(textSize.Height + 7 + 0.5);
                    UiUtil.SetButtonHeight(this, newButtonHeight, 1);
                }
            }
        }

        private void AddFixActionItemToListView(FixItem fi)
        {
            var item = new ListViewItem(string.Empty) { Tag = fi, Checked = fi.DefaultChecked };
            item.SubItems.Add(fi.Name);
            item.SubItems.Add(fi.Example);
            listView1.Items.Add(item);
        }

        public void AddFixToListView(Paragraph p, string action, string before, string after)
        {
            if (_onlyListFixes)
            {
                var item = new ListViewItem(string.Empty) { Checked = true, Tag = p };
                item.SubItems.Add(p.Number.ToString(CultureInfo.InvariantCulture));
                item.SubItems.Add(action);
                item.SubItems.Add(UiUtil.GetListViewTextFromString(before));
                item.SubItems.Add(UiUtil.GetListViewTextFromString(after));
                listViewFixes.Items.Add(item);
            }
        }

        public bool AllowFix(Paragraph p, string action)
        {
            if (_onlyListFixes || BatchMode)
            {
                return true;
            }

            return _allowedFixes.Contains(p.Number.ToString(CultureInfo.InvariantCulture) + "|" + action);
        }

        public void ShowStatus(string message)
        {
            message = message.Replace(Environment.NewLine, "  ");
            if (message.Length > 103)
            {
                message = message.Substring(0, 100) + "...";
            }

            labelStatus.Text = message;
            labelStatus.Refresh();
        }

        public void LogStatus(string sender, string message, bool isImportant)
        {
            if (isImportant)
            {
                _numberOfImportantLogMessages++;
            }

            LogStatus(sender, message);
        }

        public void LogStatus(string sender, string message)
        {
            if (!string.IsNullOrEmpty(message))
            {
                message += Environment.NewLine;
                if (_onlyListFixes)
                {
                    _newLog.AppendLine(" +  " + sender + ": " + message);
                }
                else
                {
                    _appliedLog.AppendLine(string.Format(_language.FixedOkXY, sender, message));
                }
            }
        }

        public bool IsName(string candidate)
        {
            MakeSureNameListIsLoaded();
            return _nameList.Contains(candidate); // O(1)
        }

        private void MakeSureNameListIsLoaded()
        {
            if (_nameList == null)
            {
                string languageTwoLetterCode = LanguageAutoDetect.AutoDetectGoogleLanguage(Subtitle);
                // Will contains both one word names and multi names
                var namesList = new NameList(Configuration.DictionariesDirectory, languageTwoLetterCode, Configuration.Settings.WordLists.UseOnlineNames, Configuration.Settings.WordLists.NamesUrl);
                _nameList = namesList.GetNames();
                // Multi word names.
                foreach (var name in namesList.GetMultiNames())
                {
                    _nameList.Add(name);
                }
            }
        }

        public HashSet<string> GetAbbreviations()
        {
            if (_abbreviationList != null)
            {
                return _abbreviationList;
            }

            MakeSureNameListIsLoaded();
            _abbreviationList = new HashSet<string>();
            foreach (string name in _nameList)
            {
                if (name.EndsWith('.'))
                {
                    _abbreviationList.Add(name);
                }
            }
            return _abbreviationList;
        }

        private OcrFixEngine _ocrFixEngine;
        private string _ocrFixEngineLanguage;

        public void FixOcrErrorsViaReplaceList(string threeLetterIsoLanguageName)
        {
            if (_ocrFixEngine == null || _ocrFixEngineLanguage != threeLetterIsoLanguageName)
            {
                _ocrFixEngine?.Dispose();
                _ocrFixEngineLanguage = threeLetterIsoLanguageName;
                _ocrFixEngine = new OcrFixEngine(_ocrFixEngineLanguage, null, this);
            }

            string fixAction = _language.FixCommonOcrErrors;
            int noOfFixes = 0;
            string lastLine = string.Empty;
            for (int i = 0; i < Subtitle.Paragraphs.Count; i++)
            {
                var p = Subtitle.Paragraphs[i];
                string text = _ocrFixEngine.FixOcrErrors(p.Text, i, lastLine, false, OcrFixEngine.AutoGuessLevel.Cautious);
                lastLine = text;
                if (AllowFix(p, fixAction) && p.Text != text)
                {
                    string oldText = p.Text;
                    p.Text = text;
                    noOfFixes++;
                    AddFixToListView(p, fixAction, oldText, p.Text);
                }
                Application.DoEvents();
            }
            if (noOfFixes > 0)
            {
                _totalFixes += noOfFixes;
                LogStatus(_language.FixCommonOcrErrors, string.Format(_language.CommonOcrErrorsFixed, noOfFixes));
            }
        }

        public void UpdateFixStatus(int fixes, string message, string xMessage)
        {
            if (fixes > 0)
            {
                _totalFixes += fixes;
                LogStatus(message, string.Format(xMessage, fixes));
            }
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
                Cursor = Cursors.WaitCursor;
                Next();
                ShowAvailableFixesStatus(false);
            }
            Cursor = Cursors.Default;
        }

        private void Next()
        {
            RunSelectedActions();

            buttonBack.Enabled = true;
            buttonNextFinish.Text = _languageGeneral.Ok;
            buttonNextFinish.Enabled = _hasFixesBeenMade || _linesDeletedOrMerged;
            groupBoxStep1.Visible = false;
            groupBox2.Visible = true;
            listViewFixes.Sort();
            subtitleListView1.Fill(FixedSubtitle);
            if (listViewFixes.Items.Count > 0)
            {
                listViewFixes.Items[0].Selected = true;
            }
            else
            {
                subtitleListView1.SelectIndexAndEnsureVisible(0);
            }
        }

        private void RunSelectedActions()
        {
            subtitleListView1.BeginUpdate();
            _newLog.Clear();

            Subtitle = new Subtitle(FixedSubtitle, false);
            if (listView1.Items[IndexFixOcrErrorsViaReplaceList].Checked)
            {
                var fixItem = (FixItem)listView1.Items[IndexFixOcrErrorsViaReplaceList].Tag;
                fixItem.Action.Invoke();
            }
            foreach (ListViewItem item in listView1.Items)
            {
                if (item.Checked && item.Index != IndexRemoveEmptyLines)
                {
                    var fixItem = (FixItem)item.Tag;
                    fixItem.Action.Invoke();
                }
            }
            if (listView1.Items[IndexInvalidItalicTags].Checked)
            {
                var fixItem = (FixItem)listView1.Items[IndexInvalidItalicTags].Tag;
                fixItem.Action.Invoke();
            }
            if (listView1.Items[IndexRemoveEmptyLines].Checked)
            {
                var fixItem = (FixItem)listView1.Items[IndexRemoveEmptyLines].Tag;
                fixItem.Action.Invoke();
            }

            // build log
            textBoxFixedIssues.Text = string.Empty;
            if (_newLog.Length >= 0)
            {
                textBoxFixedIssues.AppendText(_newLog + Environment.NewLine);
            }

            textBoxFixedIssues.AppendText(_appliedLog.ToString());
            subtitleListView1.EndUpdate();
        }

        private void FormFixKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                DialogResult = DialogResult.Cancel;
            }
            else if (e.KeyCode == UiUtil.HelpKeys)
            {
                Utilities.ShowHelp("#fixcommonerrors");
            }
            else if (e.KeyCode == Keys.Enter && buttonNextFinish.Text == _language.Next)
            {
                ButtonFixClick(null, null);
            }
            else if (subtitleListView1.Visible && subtitleListView1.Items.Count > 0 && e.KeyData == _goToLine)
            {
                GoToLineNumber();
            }
            else if (e.KeyData == _preview && listViewFixes.Items.Count > 0)
            {
                GenerateDiff();
            }
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
                GoToNextSyntaxError();
                e.SuppressKeyPress = true;
            }
        }

        private void GoToNextSyntaxError()
        {
            int idx = 0;
            try
            {
                if (listViewFixes.SelectedItems.Count > 0)
                {
                    idx = listViewFixes.SelectedItems[0].Index;
                }

                idx++;
                if (listViewFixes.Items.Count > idx)
                {
                    listViewFixes.Items[idx].Selected = true;
                    listViewFixes.Items[idx].EnsureVisible();
                    if (idx > 0)
                    {
                        listViewFixes.Items[idx - 1].Selected = false;
                    }
                }
            }
            catch
            {
                // ignored
            }
        }

        private void GoToLineNumber()
        {
            using (var goToLine = new GoToLine())
            {
                goToLine.Initialize(1, subtitleListView1.Items.Count);
                if (goToLine.ShowDialog(this) == DialogResult.OK)
                {
                    subtitleListView1.SelectNone();
                    subtitleListView1.Items[goToLine.LineNumber - 1].Selected = true;
                    subtitleListView1.Items[goToLine.LineNumber - 1].EnsureVisible();
                    subtitleListView1.Items[goToLine.LineNumber - 1].Focused = true;
                }
            }
        }

        private void GenerateDiff()
        {
            string htmlFileName = FileUtil.GetTempFileName(".html");
            var sb = new StringBuilder();
            sb.Append("<html><head><meta charset='utf-8'><title>Subtitle Edit - Fix common errors preview</title><style>body,p,td {font-size:90%; font-family:Tahoma;} td {border:1px solid black;padding:5px} table {border-collapse: collapse;}</style></head><body><table><tbody>");
            sb.AppendLine($"<tr><td style='font-weight:bold'>{_languageGeneral.LineNumber}</td><td style='font-weight:bold'>{_language.Function}</td><td style='font-weight:bold'>{_languageGeneral.Before}</td><td style='font-weight:bold'>{_languageGeneral.After}</td></tr>");
            foreach (ListViewItem item in listViewFixes.Items)
            {
                if (item.Checked)
                {
                    var p = (Paragraph)item.Tag;
                    string what = item.SubItems[2].Text;
                    string before = item.SubItems[3].Text;
                    string after = item.SubItems[4].Text;
                    var arr = MakeDiffHtml(before, after);
                    sb.AppendLine($"<tr><td>{p.Number}</td><td>{what}</td><td><pre>{arr[0]}</pre></td><td><pre>{arr[1]}</pre></td></tr>");
                }
            }
            sb.Append("</table></body></html>");
            File.WriteAllText(htmlFileName, sb.ToString());
            UiUtil.OpenFile(htmlFileName);
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
                        {
                            beforeBackgroundColors.Add(i, Color.Red);
                        }
                        else
                        {
                            beforeColors.Add(i, Color.Red);
                        }

                        if (char.IsWhiteSpace(after[i]))
                        {
                            afterBackgroundColors.Add(i, Color.Red);
                        }
                        else
                        {
                            afterColors.Add(i, Color.Red);
                        }
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
                    {
                        beforeBackgroundColors.Add(i, Color.Red);
                    }
                    else
                    {
                        beforeColors.Add(i, Color.Red);
                    }
                }
                if (i < after.Length)
                {
                    if (char.IsWhiteSpace(after[i]))
                    {
                        afterBackgroundColors.Add(i, Color.Red);
                    }
                    else
                    {
                        afterColors.Add(i, Color.Red);
                    }
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
                    {
                        beforeColors.Remove(bLength);
                    }

                    if (beforeBackgroundColors.ContainsKey(bLength))
                    {
                        beforeBackgroundColors.Remove(bLength);
                    }

                    if (afterColors.ContainsKey(aLength))
                    {
                        afterColors.Remove(aLength);
                    }

                    if (afterBackgroundColors.ContainsKey(aLength))
                    {
                        afterBackgroundColors.Remove(aLength);
                    }
                }
                else
                {
                    break;
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

            return new[] { sb.ToString(), sb2.ToString() };
        }

        private void SaveConfiguration()
        {
            var ce = Configuration.Settings.CommonErrors;

            ce.EmptyLinesTicked = listView1.Items[IndexRemoveEmptyLines].Checked;
            ce.OverlappingDisplayTimeTicked = listView1.Items[IndexOverlappingDisplayTime].Checked;
            ce.TooShortDisplayTimeTicked = listView1.Items[IndexTooShortDisplayTime].Checked;
            ce.TooLongDisplayTimeTicked = listView1.Items[IndexTooLongDisplayTime].Checked;
            ce.TooShortGapTicked = listView1.Items[IndexTooShortGap].Checked;
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
            if (_indexAloneLowercaseIToUppercaseIEnglish >= 0)
            {
                ce.AloneLowercaseIToUppercaseIEnglishTicked = listView1.Items[_indexAloneLowercaseIToUppercaseIEnglish].Checked;
            }

            ce.FixOcrErrorsViaReplaceListTicked = listView1.Items[IndexFixOcrErrorsViaReplaceList].Checked;
            ce.RemoveSpaceBetweenNumberTicked = listView1.Items[IndexRemoveSpaceBetweenNumbers].Checked;
            ce.FixDialogsOnOneLineTicked = listView1.Items[IndexDialogsOnOneLine].Checked;
            if (_danishLetterIIndex >= 0)
            {
                ce.DanishLetterITicked = listView1.Items[_danishLetterIIndex].Checked;
            }

            if (_turkishAnsiIndex >= 0)
            {
                ce.TurkishAnsiTicked = listView1.Items[_turkishAnsiIndex].Checked;
            }

            if (_spanishInvertedQuestionAndExclamationMarksIndex >= 0)
            {
                ce.SpanishInvertedQuestionAndExclamationMarksTicked = listView1.Items[_spanishInvertedQuestionAndExclamationMarksIndex].Checked;
            }

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
            var sorter = (ListViewSorter)listViewFixes.ListViewItemSorter;

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
            {
                item.Checked = true;
            }
        }

        private void ButtonInverseSelectionClick(object sender, EventArgs e)
        {
            foreach (ListViewItem item in listView1.Items)
            {
                item.Checked = !item.Checked;
            }
        }

        private void ListViewFixesSelectedIndexChanged(object sender, EventArgs e)
        {
            if (listViewFixes.SelectedItems.Count > 0)
            {
                var p = (Paragraph)listViewFixes.SelectedItems[0].Tag;

                foreach (ListViewItem lvi in subtitleListView1.Items)
                {
                    if (lvi.Tag is Paragraph p2 && p.Id == p2.Id)
                    {
                        var index = lvi.Index;
                        if (index - 1 > 0)
                        {
                            subtitleListView1.EnsureVisible(index - 1);
                        }

                        if (index + 1 < subtitleListView1.Items.Count)
                        {
                            subtitleListView1.EnsureVisible(index + 1);
                        }

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
            if (FixedSubtitle.Paragraphs.Count > 0)
            {
                int firstSelectedIndex = 0;
                if (subtitleListView1.SelectedItems.Count > 0)
                {
                    firstSelectedIndex = subtitleListView1.SelectedItems[0].Index;
                }

                Paragraph p = FixedSubtitle.GetParagraphOrDefault(firstSelectedIndex);
                if (p != null)
                {
                    InitializeListViewEditBox(p);
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
                FixedSubtitle.Paragraphs[_subtitleListViewIndex].Text = text;
                subtitleListView1.SetText(_subtitleListViewIndex, text);

                EnableOkButton();
                UpdateListSyntaxColoring();
            }
        }

        private void EnableOkButton()
        {
            if (!_hasFixesBeenMade)
            {
                _hasFixesBeenMade = true;
                buttonNextFinish.Enabled = true;
            }
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
            numericUpDownDuration.Value = (decimal)(p.Duration.TotalMilliseconds / TimeCode.BaseUnit);
            numericUpDownDuration.ValueChanged += NumericUpDownDurationValueChanged;
        }

        private void NumericUpDownDurationValueChanged(object sender, EventArgs e)
        {
            if (FixedSubtitle.Paragraphs.Count > 0 && subtitleListView1.SelectedItems.Count > 0)
            {
                int firstSelectedIndex = subtitleListView1.SelectedItems[0].Index;

                Paragraph currentParagraph = FixedSubtitle.GetParagraphOrDefault(firstSelectedIndex);
                if (currentParagraph != null)
                {
                    UpdateOverlapErrors();

                    // update _subtitle + listview
                    currentParagraph.EndTime.TotalMilliseconds = currentParagraph.StartTime.TotalMilliseconds + ((double)numericUpDownDuration.Value * TimeCode.BaseUnit);
                    subtitleListView1.SetDuration(firstSelectedIndex, currentParagraph, FixedSubtitle.GetParagraphOrDefault(firstSelectedIndex + 1));
                }
            }
        }

        private void UpdateOverlapErrors()
        {
            labelStartTimeWarning.Text = string.Empty;
            labelDurationWarning.Text = string.Empty;

            TimeCode startTime = timeUpDownStartTime.TimeCode;
            if (FixedSubtitle.Paragraphs.Count > 0 && subtitleListView1.SelectedItems.Count > 0 && startTime != null)
            {
                int firstSelectedIndex = subtitleListView1.SelectedItems[0].Index;

                Paragraph prevParagraph = FixedSubtitle.GetParagraphOrDefault(firstSelectedIndex - 1);
                if (prevParagraph != null && prevParagraph.EndTime.TotalMilliseconds > startTime.TotalMilliseconds)
                {
                    labelStartTimeWarning.Text = string.Format(_languageGeneral.OverlapPreviousLineX, (prevParagraph.EndTime.TotalMilliseconds - startTime.TotalMilliseconds) / TimeCode.BaseUnit);
                }

                Paragraph nextParagraph = FixedSubtitle.GetParagraphOrDefault(firstSelectedIndex + 1);
                if (nextParagraph != null)
                {
                    double durationMilliseconds = (double)numericUpDownDuration.Value * TimeCode.BaseUnit;
                    if (startTime.TotalMilliseconds + durationMilliseconds > nextParagraph.StartTime.TotalMilliseconds)
                    {
                        labelDurationWarning.Text = string.Format(_languageGeneral.OverlapNextX, ((startTime.TotalMilliseconds + durationMilliseconds) - nextParagraph.StartTime.TotalMilliseconds) / TimeCode.BaseUnit);
                    }

                    if (labelStartTimeWarning.Text.Length == 0 &&
                        startTime.TotalMilliseconds > nextParagraph.StartTime.TotalMilliseconds)
                    {
                        double di = (startTime.TotalMilliseconds - nextParagraph.StartTime.TotalMilliseconds) / TimeCode.BaseUnit;
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
            if (FixedSubtitle == null || FixedSubtitle.Paragraphs.Count == 0 || _subtitleListViewIndex < 0 || _subtitleListViewIndex >= Subtitle.Paragraphs.Count)
            {
                return;
            }

            subtitleListView1.SyntaxColorLine(FixedSubtitle.Paragraphs, _subtitleListViewIndex, FixedSubtitle.Paragraphs[_subtitleListViewIndex]);
            Paragraph next = FixedSubtitle.GetParagraphOrDefault(_subtitleListViewIndex + 1);
            if (next != null)
            {
                subtitleListView1.SyntaxColorLine(FixedSubtitle.Paragraphs, _subtitleListViewIndex + 1, FixedSubtitle.Paragraphs[_subtitleListViewIndex + 1]);
            }
        }

        private void MaskedTextBox_TextChanged(object sender, EventArgs e)
        {
            if (_subtitleListViewIndex >= 0 &&
                timeUpDownStartTime.TimeCode != null &&
                FixedSubtitle.Paragraphs.Count > 0 &&
                subtitleListView1.SelectedItems.Count > 0)
            {
                TimeCode startTime = timeUpDownStartTime.TimeCode;
                labelStartTimeWarning.Text = string.Empty;
                labelDurationWarning.Text = string.Empty;

                UpdateOverlapErrors();

                // update _subtitle + listview
                FixedSubtitle.Paragraphs[_subtitleListViewIndex].EndTime.TotalMilliseconds +=
                    (startTime.TotalMilliseconds - FixedSubtitle.Paragraphs[_subtitleListViewIndex].StartTime.TotalMilliseconds);
                FixedSubtitle.Paragraphs[_subtitleListViewIndex].StartTime = startTime;
                subtitleListView1.SetStartTimeAndDuration(_subtitleListViewIndex, FixedSubtitle.Paragraphs[_subtitleListViewIndex], FixedSubtitle.GetParagraphOrDefault(_subtitleListViewIndex + 1), FixedSubtitle.GetParagraphOrDefault(_subtitleListViewIndex - 1));
            }
        }

        private void UpdateListViewTextInfo(string text)
        {
            labelTextLineLengths.Text = _languageGeneral.SingleLineLengths;
            labelSingleLine.Left = labelTextLineLengths.Left + labelTextLineLengths.Width - 6;
            text = HtmlUtil.RemoveHtmlTags(text, true);
            UiUtil.GetLineLengths(labelSingleLine, text);

            string s = text.Replace(Environment.NewLine, " ");
            labelTextLineTotal.ForeColor = Color.Black;
            buttonSplitLine.Visible = false;
            var abl = Utilities.AutoBreakLine(s, _autoDetectGoogleLanguage).SplitToLines();
            if (abl.Count > Configuration.Settings.General.MaxNumberOfLines || abl.Any(li => li.Length > Configuration.Settings.General.SubtitleLineMaximumLength))
            {
                buttonSplitLine.Visible = true;
                labelTextLineTotal.ForeColor = Color.Red;
            }
            labelTextLineTotal.Text = string.Format(_languageGeneral.TotalLengthX, s.Length);
        }

        private void ButtonFixesSelectAllClick(object sender, EventArgs e)
        {
            foreach (ListViewItem item in listViewFixes.Items)
            {
                item.Checked = true;
            }
        }

        private void ButtonFixesInverseClick(object sender, EventArgs e)
        {
            foreach (ListViewItem item in listViewFixes.Items)
            {
                item.Checked = !item.Checked;
            }
        }

        private void ButtonFixesApplyClick(object sender, EventArgs e)
        {
            buttonFixesApply.Enabled = false;
            _hasFixesBeenMade = true;
            Cursor = Cursors.WaitCursor;
            ShowStatus(_language.Analysing);

            _subtitleListViewIndex = -1;
            int firstSelectedIndex = 0;
            if (subtitleListView1.SelectedItems.Count > 0)
            {
                firstSelectedIndex = subtitleListView1.SelectedItems[0].Index;
            }

            _allowedFixes = new HashSet<string>();
            foreach (ListViewItem item in listViewFixes.Items)
            {
                if (item.Checked)
                {
                    string key = item.SubItems[1].Text + "|" + item.SubItems[2].Text;
                    if (!_allowedFixes.Contains(key))
                    {
                        _allowedFixes.Add(key);
                    }
                }
            }

            _numberOfImportantLogMessages = 0;
            _onlyListFixes = false;
            _totalFixes = 0;
            _totalErrors = 0;
            RunSelectedActions();
            FixedSubtitle = new Subtitle(Subtitle, false);
            subtitleListView1.Fill(FixedSubtitle);
            ShowAvailableFixesStatus(true);
            RefreshFixes();
            if (listViewFixes.Items.Count == 0)
            {
                subtitleListView1.SelectIndexAndEnsureVisible(firstSelectedIndex);
            }

            Cursor = Cursors.Default;
            if (_numberOfImportantLogMessages == 0)
            {
                labelNumberOfImportantLogMessages.Text = string.Empty;
            }
            else
            {
                labelNumberOfImportantLogMessages.Text = string.Format(_language.NumberOfImportantLogMessages, _numberOfImportantLogMessages);
            }

            buttonFixesApply.Enabled = true;
        }

        private void ButtonRefreshFixesClick(object sender, EventArgs e)
        {
            Cursor = Cursors.WaitCursor;
            ShowStatus(_language.Analysing);
            _totalFixes = 0;
            RefreshFixes();
            ShowAvailableFixesStatus(false);
            Cursor = Cursors.Default;
        }

        private void ShowAvailableFixesStatus(bool applied)
        {
            labelStatus.ForeColor = DefaultForeColor;
            if (_totalFixes == 0 && _totalErrors == 0)
            {
                ShowStatus(_language.NothingToFix);
                if (subtitleListView1.SelectedItems.Count == 0)
                {
                    subtitleListView1.SelectIndexAndEnsureVisible(0);
                }
            }
            else if (_totalFixes > 0)
            {
                if (_totalErrors > 0)
                {
                    labelStatus.ForeColor = Color.Red;
                    ShowStatus(string.Format(applied ? _language.XFixedBut : _language.XCouldBeFixedBut, _totalFixes));
                }
                else
                {
                    ShowStatus(string.Format(applied ? _language.XFixesApplied : _language.FixesFoundX, _totalFixes));
                }
            }
            else if (_totalErrors > 0)
            {
                labelStatus.ForeColor = Color.Red;
                ShowStatus(_language.NothingFixableBut);
            }

            TopMost = true;
            BringToFront();
            TopMost = false;
        }

        private void RefreshFixes()
        {
            listViewFixes.BeginUpdate();

            // save de-selected fixes
            var deSelectedFixes = new List<string>();
            foreach (ListViewItem item in listViewFixes.Items)
            {
                if (!item.Checked)
                {
                    deSelectedFixes.Add(item.SubItems[1].Text + item.SubItems[2].Text + item.SubItems[3].Text);
                }
            }

            listViewFixes.Items.Clear();
            _onlyListFixes = true;
            Next();

            // restore de-selected fixes
            foreach (ListViewItem item in listViewFixes.Items)
            {
                if (deSelectedFixes.Contains(item.SubItems[1].Text + item.SubItems[2].Text + item.SubItems[3].Text))
                {
                    item.Checked = false;
                }
            }

            listViewFixes.EndUpdate();
        }

        private void ButtonAutoBreakClick(object sender, EventArgs e)
        {
            if (textBoxListViewText.Text.Length > 0)
            {
                string oldText = textBoxListViewText.Text;
                textBoxListViewText.Text = Utilities.AutoBreakLine(textBoxListViewText.Text, Language);
                if (oldText != textBoxListViewText.Text)
                {
                    EnableOkButton();
                }
            }
        }

        private void ButtonUnBreakClick(object sender, EventArgs e)
        {
            string oldText = textBoxListViewText.Text;
            textBoxListViewText.Text = Utilities.UnbreakLine(textBoxListViewText.Text);
            if (oldText != textBoxListViewText.Text)
            {
                EnableOkButton();
            }
        }

        private void ToolStripMenuItemDeleteClick(object sender, EventArgs e)
        {
            if (FixedSubtitle.Paragraphs.Count <= 0 || subtitleListView1.SelectedItems.Count <= 0)
            {
                return;
            }

            var askText = subtitleListView1.SelectedItems.Count > 1 ? string.Format(Configuration.Settings.Language.Main.DeleteXLinesPrompt, subtitleListView1.SelectedItems.Count) : Configuration.Settings.Language.Main.DeleteOneLinePrompt;
            if (Configuration.Settings.General.PromptDeleteLines && MessageBox.Show(askText, Configuration.Settings.Language.General.Title, MessageBoxButtons.YesNoCancel) != DialogResult.Yes)
            {
                return;
            }

            _linesDeletedOrMerged = true;
            _subtitleListViewIndex = -1;
            var indexes = new List<int>();
            foreach (ListViewItem item in subtitleListView1.SelectedItems)
            {
                indexes.Add(item.Index);
            }

            int firstIndex = subtitleListView1.SelectedItems[0].Index;

            // save de-selected fixes
            var deSelectedFixes = new List<string>();
            foreach (ListViewItem item in listViewFixes.Items)
            {
                if (!item.Checked)
                {
                    int number = Convert.ToInt32(item.SubItems[1].Text);
                    if (number > firstIndex)
                    {
                        number -= subtitleListView1.SelectedItems.Count;
                    }

                    if (number >= 0)
                    {
                        deSelectedFixes.Add(number + item.SubItems[2].Text + item.SubItems[3].Text);
                    }
                }
            }

            FixedSubtitle.RemoveParagraphsByIndices(indexes);
            FixedSubtitle.Renumber();
            subtitleListView1.Fill(FixedSubtitle);

            // refresh fixes
            listViewFixes.Items.Clear();
            _onlyListFixes = true;
            Next();

            // restore de-selected fixes
            if (deSelectedFixes.Count > 0)
            {
                foreach (ListViewItem item in listViewFixes.Items)
                {
                    if (deSelectedFixes.Contains(item.SubItems[1].Text + item.SubItems[2].Text + item.SubItems[3].Text))
                    {
                        item.Checked = false;
                    }
                }
            }

            if (subtitleListView1.Items.Count > firstIndex)
            {
                subtitleListView1.SelectIndexAndEnsureVisible(firstIndex, true);
            }
            else if (subtitleListView1.Items.Count > 0)
            {
                subtitleListView1.SelectIndexAndEnsureVisible(subtitleListView1.Items.Count - 1, true);
            }
        }

        private void MergeSelectedLinesToolStripMenuItemClick(object sender, EventArgs e)
        {
            if (FixedSubtitle.Paragraphs.Count > 0 && subtitleListView1.SelectedItems.Count > 0)
            {
                _linesDeletedOrMerged = true;
                int startNumber = FixedSubtitle.Paragraphs[0].Number;
                int firstSelectedIndex = subtitleListView1.SelectedItems[0].Index;

                // save de-selected fixes
                var deSelectedFixes = new List<string>();
                foreach (ListViewItem item in listViewFixes.Items)
                {
                    if (!item.Checked)
                    {
                        int firstSelectedNumber = subtitleListView1.GetSelectedParagraph(FixedSubtitle).Number;
                        int number = Convert.ToInt32(item.SubItems[1].Text);
                        if (number > firstSelectedNumber)
                        {
                            number--;
                        }

                        deSelectedFixes.Add(number + item.SubItems[2].Text + item.SubItems[3].Text);
                    }
                }

                var currentParagraph = FixedSubtitle.GetParagraphOrDefault(firstSelectedIndex);
                var nextParagraph = FixedSubtitle.GetParagraphOrDefault(firstSelectedIndex + 1);

                if (nextParagraph != null && currentParagraph != null)
                {
                    subtitleListView1.SelectedIndexChanged -= SubtitleListView1SelectedIndexChanged;

                    currentParagraph.Text = currentParagraph.Text.Replace(Environment.NewLine, " ");
                    currentParagraph.Text += Environment.NewLine + nextParagraph.Text.Replace(Environment.NewLine, " ");
                    currentParagraph.EndTime = nextParagraph.EndTime;

                    FixedSubtitle.Paragraphs.Remove(nextParagraph);

                    FixedSubtitle.Renumber(startNumber);
                    subtitleListView1.Fill(FixedSubtitle);
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
                    {
                        item.Checked = false;
                    }
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
            using (var graphics = CreateGraphics())
            {
                var timestampSizeF = graphics.MeasureString(listViewFixes.Columns[0].Text, Font); // Apply
                var width = (int)(timestampSizeF.Width + 12);
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
            }
        }

        private void FixCommonErrorsShown(object sender, EventArgs e)
        {
            FixCommonErrorsResize(null, null);
        }

        private void SplitSelectedParagraph(double? splitSeconds)
        {
            if (FixedSubtitle.Paragraphs.Count > 0 && subtitleListView1.SelectedItems.Count > 0)
            {
                subtitleListView1.SelectedIndexChanged -= SubtitleListView1SelectedIndexChanged;
                int firstSelectedIndex = subtitleListView1.SelectedItems[0].Index;

                // save de-selected fixes
                var deSelectedFixes = new List<string>();
                foreach (ListViewItem item in listViewFixes.Items)
                {
                    if (!item.Checked)
                    {
                        int number = Convert.ToInt32(item.SubItems[1].Text);
                        if (number > firstSelectedIndex)
                        {
                            number++;
                        }

                        deSelectedFixes.Add(number + item.SubItems[2].Text + item.SubItems[3].Text);
                    }
                }

                Paragraph currentParagraph = FixedSubtitle.GetParagraphOrDefault(firstSelectedIndex);
                var newParagraph = new Paragraph();

                string oldText = currentParagraph.Text;
                var lines = currentParagraph.Text.SplitToLines();
                if (lines.Count == 2 && (lines[0].EndsWith('.') || lines[0].EndsWith('!') || lines[0].EndsWith('?')))
                {
                    currentParagraph.Text = Utilities.AutoBreakLine(lines[0], Language);
                    newParagraph.Text = Utilities.AutoBreakLine(lines[1], Language);
                }
                else
                {
                    string s = Utilities.AutoBreakLine(currentParagraph.Text, 5, Configuration.Settings.General.MergeLinesShorterThan, Language);
                    lines = s.SplitToLines();
                    if (lines.Count == 2)
                    {
                        currentParagraph.Text = Utilities.AutoBreakLine(lines[0], Language);
                        newParagraph.Text = Utilities.AutoBreakLine(lines[1], Language);
                    }
                    else if (lines.Count > 2)
                    {
                        var half = lines.Count / 2;
                        var sb1 = new StringBuilder();
                        for (int i = 0; i < half; i++)
                        {
                            sb1.AppendLine(lines[i]);
                        }

                        currentParagraph.Text = Utilities.AutoBreakLine(sb1.ToString(), _autoDetectGoogleLanguage);
                        sb1 = new StringBuilder();
                        for (int i = half; i < lines.Count; i++)
                        {
                            sb1.AppendLine(lines[i]);
                        }

                        newParagraph.Text = Utilities.AutoBreakLine(sb1.ToString(), _autoDetectGoogleLanguage);
                    }
                }

                double startFactor = (double)HtmlUtil.RemoveHtmlTags(currentParagraph.Text).Length / HtmlUtil.RemoveHtmlTags(oldText).Length;
                if (startFactor < 0.20)
                {
                    startFactor = 0.20;
                }

                if (startFactor > 0.80)
                {
                    startFactor = 0.80;
                }

                double middle = currentParagraph.StartTime.TotalMilliseconds + (currentParagraph.Duration.TotalMilliseconds * startFactor);
                if (splitSeconds.HasValue && splitSeconds.Value > (currentParagraph.StartTime.TotalSeconds + 0.2) && splitSeconds.Value < (currentParagraph.EndTime.TotalSeconds - 0.2))
                {
                    middle = splitSeconds.Value * TimeCode.BaseUnit;
                }

                newParagraph.EndTime.TotalMilliseconds = currentParagraph.EndTime.TotalMilliseconds;
                currentParagraph.EndTime.TotalMilliseconds = middle;
                newParagraph.StartTime.TotalMilliseconds = currentParagraph.EndTime.TotalMilliseconds + 1;

                FixedSubtitle.Paragraphs.Insert(firstSelectedIndex + 1, newParagraph);
                FixedSubtitle.Renumber();
                subtitleListView1.Fill(FixedSubtitle);
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
                    {
                        item.Checked = false;
                    }
                }
            }
        }

        private void ButtonSplitLineClick(object sender, EventArgs e)
        {
            SplitSelectedParagraph(null);
        }

        private void TextBoxListViewTextKeyDown(object sender, KeyEventArgs e)
        {
            UiUtil.CheckAutoWrap(textBoxListViewText, e, Utilities.GetNumberOfLines(textBoxListViewText.Text));
        }

        private void FixCommonErrorsFormClosing(object sender, FormClosingEventArgs e)
        {
            if (_ocrFixEngine != null)
            {
                _ocrFixEngine.Dispose();
                _ocrFixEngine = null;
            }
            Owner = null;
        }

        private void comboBoxLanguage_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (Subtitle != null)
            {
                if (comboBoxLanguage.SelectedItem is LanguageItem ci)
                {
                    _autoDetectGoogleLanguage = ci.Code.TwoLetterISOLanguageName;
                    AddFixActions(ci.Code.ThreeLetterISOLanguageName);
                }
            }
        }

        private void comboBoxLanguage_Enter(object sender, EventArgs e)
        {
            SaveConfiguration();
        }

        private void buttonResetDefault_Click(object sender, EventArgs e)
        {
            Configuration.Settings.CommonErrors.SetDefaultFixes();
            AddFixActions(CultureInfo.GetCultureInfo(_autoDetectGoogleLanguage).ThreeLetterISOLanguageName);
        }

        private void subtitleListView1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.A && e.Modifiers == Keys.Control)
            {
                subtitleListView1.SelectAll();
                e.SuppressKeyPress = true;
            }
            else if (e.KeyCode == Keys.D && e.Modifiers == Keys.Control)
            {
                subtitleListView1.SelectFirstSelectedItemOnly();
                e.SuppressKeyPress = true;
            }
            else if (e.KeyCode == Keys.I && e.Modifiers == (Keys.Control | Keys.Shift)) //InverseSelection
            {
                subtitleListView1.InverseSelection();
                e.SuppressKeyPress = true;
            }
            else if (e.KeyCode == Keys.Delete)
            {
                ToolStripMenuItemDeleteClick(null, null);
            }
        }

        private Hunspell _hunspell;

        public bool DoSpell(string word)
        {
            if (_hunspell == null && Language != null)
            {
                var fileMatches = Directory.GetFiles(Utilities.DictionaryFolder, Language + "*.dic");
                if (fileMatches.Length > 0)
                {
                    var dictionary = fileMatches[0].Substring(0, fileMatches[0].Length - 4);
                    try
                    {
                        _hunspell = Hunspell.GetHunspell(dictionary);
                    }
                    catch
                    {
                        _hunspell = null;
                    }
                }
            }

            if (_hunspell == null)
            {
                return false;
            }

            return _hunspell.Spell(word);
        }
    }
}
