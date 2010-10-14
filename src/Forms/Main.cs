using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Windows.Forms;
using System.Xml;
using Nikse.SubtitleEdit.Controls;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Enums;
using Nikse.SubtitleEdit.Logic.SubtitleFormats;
using Nikse.SubtitleEdit.Logic.VideoPlayers;
using Nikse.SubtitleEdit.Logic.VobSub;

namespace Nikse.SubtitleEdit.Forms
{
    public sealed partial class Main : Form
    {
        private class ComboBoxZoomItem
        {
            public string Text { get; set; }
            public double ZoomFactor { get; set; }
            public override string ToString()
            {
                return Text;
            }
        }

        const int TabControlListView = 0;
        const int TabControlSourceView = 1;

        Subtitle _subtitle = new Subtitle();
        Subtitle _subtitleAlternate = new Subtitle();
        string _fileName;
        string _videoFileName;

        public string VideoFileName
        {
            get { return _videoFileName; }
            set { _videoFileName = value; }
        }
        DateTime _fileDateTime;
        string _title;
        FindReplaceDialogHelper _findHelper;
        int _replaceStartLineIndex = 0;
        bool _sourceViewChange;
        bool _change;
        int _subtitleListViewIndex = -1;
        Paragraph _oldSelectedParagraph;
        bool _converted;
        SubtitleFormat _oldSubtitleFormat;
        List<int> _selectedIndexes;
        LanguageStructure.Main _language;
        LanguageStructure.General _languageGeneral;
        SpellCheck _spellCheckForm;
        PositionsAndSizes _formPositionsAndSizes = new PositionsAndSizes();

        VideoInfo _videoInfo;
        int _repeatCount = -1;
        double _endSeconds = -1;
        int _autoContinueDelayCount = -1;
        long _lastTextKeyDownTicks = 0;
        System.Windows.Forms.Timer _timerAddHistoryWhenDone = new Timer();
        string _timerAddHistoryWhenDoneText;

        private System.Threading.Mutex _mutex;

        private bool AutoRepeatContinueOn
        {
            get
            {
                return tabControlButtons.SelectedIndex == 0;
            }
        }

        public string Title
        {
            get
            {
                if (_title == null)
                {
                    string[] versionInfo = Utilities.AssemblyVersion.Split('.');
                    _title = String.Format("{0} {1}.{2}", _languageGeneral.Title, versionInfo[0], versionInfo[1]);
                    if (versionInfo.Length >= 3 && versionInfo[2] != "0")
                        _title += "." + versionInfo[2];

                    _title = "Subtitle Edit 3.0 RC2";

                }
                return _title;
            }
        }

        public void SetCurrentFormat(SubtitleFormat format)
        {
            if (format.IsVobSubIndexFile)
            {
                comboBoxSubtitleFormats.Items.Clear();
                comboBoxSubtitleFormats.Items.Add(format.FriendlyName);

                SubtitleListview1.HideNonVobSubColumns();
            }
            else if (comboBoxSubtitleFormats.Items.Count == 1)
            {
                SetFormatToSubRip();
                SubtitleListview1.ShowAllColumns();
            }

            int i = 0;
            foreach (object obj in comboBoxSubtitleFormats.Items)
            {
                if (obj.ToString() == format.FriendlyName)
                    comboBoxSubtitleFormats.SelectedIndex = i;
                i++;
            }
        }

        public void SetCurrentFormat(string subtitleFormatFriendlyName)
        {

            foreach (SubtitleFormat format in SubtitleFormat.AllSubtitleFormats)
            {
                if (format.FriendlyName == subtitleFormatFriendlyName)
                {
                    SetCurrentFormat(format);
                    break;
                }
            }
        }

        public Main()
        {
            InitializeComponent();

            SetLanguage(Configuration.Settings.General.Language);

            labelTextLineLengths.Text = string.Empty;
            labelTextLineTotal.Text = string.Empty;
            labelStartTimeWarning.Text = string.Empty;
            labelDurationWarning.Text = string.Empty;
            labelVideoInfo.Text = string.Empty;
            Text = Title;

            SetFormatToSubRip();

            if (Configuration.Settings.General.DefaultEncoding == "ANSI")
            {
                comboBoxEncoding.SelectedIndex = 0;
                comboBoxEncoding.Items[0] = "ANSI - " + Encoding.Default.CodePage.ToString();
            }
            else
            {
                comboBoxEncoding.Text = Configuration.Settings.General.DefaultEncoding;
            }

            toolStripComboBoxFrameRate.Items.Add((23.976).ToString());
            toolStripComboBoxFrameRate.Items.Add((24.0).ToString());
            toolStripComboBoxFrameRate.Items.Add((25.0).ToString());
            toolStripComboBoxFrameRate.Items.Add((29.97).ToString());
            toolStripComboBoxFrameRate.Text = Configuration.Settings.General.DefaultFrameRate.ToString();

            UpdateRecentFilesUI();
            InitializeToolbar();
            InitializeSubtitleFont();

            tabControlSubtitle.SelectTab(TabControlSourceView); // AC
            ShowSourceLineNumber();                             // AC
            tabControlSubtitle.SelectTab(TabControlListView);   // AC
            if (Configuration.Settings.General.StartInSourceView)
                tabControlSubtitle.SelectTab(TabControlSourceView);

            AudioWaveForm.Visible = Configuration.Settings.General.ShowWaveForm;
            panelWaveFormControls.Visible = Configuration.Settings.General.ShowWaveForm;
            trackBarWaveFormPosition.Visible = Configuration.Settings.General.ShowWaveForm;
            toolStripButtonToogleWaveForm.Checked = Configuration.Settings.General.ShowWaveForm;
            toolStripButtonToogleVideo.Checked = Configuration.Settings.General.ShowVideoPlayer;

            string fileName = string.Empty;
            string[] args = Environment.GetCommandLineArgs();
            if (args.Length >= 2)
                fileName = args[1];

            if (fileName.Length > 0 && File.Exists(fileName))
            {
                OpenSubtitle(fileName, null);
            }
            else if (Configuration.Settings.General.StartLoadLastFile)
            {
                if (Configuration.Settings.RecentFiles.FileNames.Count > 0)
                {
                    fileName = Configuration.Settings.RecentFiles.FileNames[0];
                    if (File.Exists(fileName))
                        OpenSubtitle(fileName, null);
                }
            }

            timeUpDownStartTime.MaskedTextBox.TextChanged += MaskedTextBox_TextChanged;
            labelAutoDuration.Visible = false;
            labelSubtitle.Text = string.Empty;
            comboBoxAutoRepeat.SelectedIndex = 2;
            comboBoxAutoContinue.SelectedIndex = 2;
            timeUpDownVideoPosition.TimeCode = new TimeCode(0, 0, 0, 0);
            timeUpDownVideoPositionAdjust.TimeCode = new TimeCode(0, 0, 0, 0);
            timeUpDownVideoPosition.TimeCodeChanged += VideoPositionChanged;
            timeUpDownVideoPositionAdjust.TimeCodeChanged += VideoPositionChanged;
            timeUpDownVideoPosition.Enabled = false;
            timeUpDownVideoPositionAdjust.Enabled = false;

            switch (Configuration.Settings.VideoControls.LastActiveTab)
            {
                case "Translate":
                    tabControlButtons.SelectedIndex = 0;
                    break;
                case "Create":
                    tabControlButtons.SelectedIndex = 1;
                    break;
                case "Adjust":
                    tabControlButtons.SelectedIndex = 2;
                    break;
            }
            tabControl1_SelectedIndexChanged(null, null);

            buttonCustomUrl.Text = Configuration.Settings.VideoControls.CustomSearchText;
            buttonCustomUrl.Enabled = Configuration.Settings.VideoControls.CustomSearchUrl.Length > 1;
            
            // Initialize events etc. for audio wave form
            AudioWaveForm.OnPositionSelected += AudioWaveForm_OnPositionSelected;
            AudioWaveForm.OnTimeChanged += AudioWaveForm_OnTimeChanged;
            AudioWaveForm.OnNewSelectionRightClicked += AudioWaveForm_OnNewSelectionRightClicked;
            AudioWaveForm.OnParagraphRightClicked += AudioWaveForm_OnParagraphRightClicked;
            AudioWaveForm.OnTooglePlay += AudioWaveForm_OnTooglePlay;
            AudioWaveForm.OnPause += AudioWaveForm_OnPause;
            AudioWaveForm.OnTimeChangedAndOffsetRest += AudioWaveForm_OnTimeChangedAndOffsetRest;
            AudioWaveForm.OnZoomedChanged += AudioWaveForm_OnZoomedChanged;
            AudioWaveForm.DrawGridLines = Configuration.Settings.VideoControls.WaveFormDrawGrid;
            AudioWaveForm.GridColor = Configuration.Settings.VideoControls.WaveFormGridColor;
            AudioWaveForm.SelectedColor = Configuration.Settings.VideoControls.WaveFormSelectedColor;
            AudioWaveForm.Color = Configuration.Settings.VideoControls.WaveFormColor;
            AudioWaveForm.BackgroundColor = Configuration.Settings.VideoControls.WaveFormBackgroundColor;
            AudioWaveForm.TextColor = Configuration.Settings.VideoControls.WaveFormTextColor;
            
            for (double zoomCounter = WaveForm.ZoomMininum; zoomCounter <= WaveForm.ZoomMaxinum + (0.001); zoomCounter += 0.1)
            {
                int percent = (int)Math.Round((zoomCounter * 100));
                ComboBoxZoomItem item = new ComboBoxZoomItem() { Text = percent.ToString() + "%", ZoomFactor = zoomCounter };
                toolStripComboBoxWaveForm.Items.Add(item);
                if (percent == 100)
                    toolStripComboBoxWaveForm.SelectedIndex = toolStripComboBoxWaveForm.Items.Count - 1;
            }
            toolStripComboBoxWaveForm.SelectedIndexChanged += toolStripComboBoxWaveForm_SelectedIndexChanged;

            _timerAddHistoryWhenDone.Interval = 500;
            _timerAddHistoryWhenDone.Tick += new EventHandler(timerAddHistoryWhenDone_Tick);

            try
            {
                _mutex = System.Threading.Mutex.OpenExisting("Subtitle_Edit_Mutex");
            }
            catch 
            {
                _mutex = new System.Threading.Mutex(true, "Subtitle_Edit_Mutex");
            }
        }

        void timerAddHistoryWhenDone_Tick(object sender, EventArgs e)
        {
            _timerAddHistoryWhenDone.Stop();
            _subtitle.MakeHistoryForUndo(_timerAddHistoryWhenDoneText, GetCurrentSubtitleFormat(), _fileDateTime);
        }

        void AudioWaveForm_OnZoomedChanged(object sender, EventArgs e)
        {
            SelectZoomTextInComboBox();
        }

        void AudioWaveForm_OnTimeChangedAndOffsetRest(double seconds, Paragraph paragraph)
        {
            int index = _subtitle.GetIndex(paragraph);
            if (mediaPlayer.VideoPlayer != null && index >= 0)
            {
                SubtitleListview1.SelectIndexAndEnsureVisible(index);
                mediaPlayer.CurrentPosition = seconds;
                ButtonSetStartAndOffsetRestClick(null, null);
                AudioWaveForm.Invalidate();
            }   
        }

        void AudioWaveForm_OnPause(object sender, EventArgs e)
        {
            if (mediaPlayer.VideoPlayer != null)
                mediaPlayer.Pause();
        }

        void AudioWaveForm_OnTooglePlay(object sender, EventArgs e)
        {
            if (mediaPlayer.VideoPlayer != null)
                mediaPlayer.TooglePlayPause();
        }

        void AudioWaveForm_OnParagraphRightClicked(double seconds, Paragraph paragraph)
        {
            SubtitleListview1.SelectIndexAndEnsureVisible(_subtitle.GetIndex(paragraph));

            addParagraphHereToolStripMenuItem.Visible = false;
            deleteParagraphToolStripMenuItem.Visible = true;
            splitToolStripMenuItem1.Visible = true;
            mergeWithPreviousToolStripMenuItem.Visible = true;
            mergeWithNextToolStripMenuItem.Visible = true;
            contextMenuStripWaveForm.Show(MousePosition.X, MousePosition.Y);
        }

        void AudioWaveForm_OnNewSelectionRightClicked(Paragraph paragraph)
        {
            SubtitleListview1.SelectIndexAndEnsureVisible(_subtitle.GetIndex(paragraph));

            addParagraphHereToolStripMenuItem.Visible = true;
            deleteParagraphToolStripMenuItem.Visible = false;
            splitToolStripMenuItem1.Visible = false;
            mergeWithPreviousToolStripMenuItem.Visible = false;
            mergeWithNextToolStripMenuItem.Visible = false;

            contextMenuStripWaveForm.Show(MousePosition.X, MousePosition.Y);
        }

        void AudioWaveForm_OnTimeChanged(double seconds, Paragraph paragraph)
        {
            _change = true;
            MakeHistoryForUndoWhenNoMoreChanges(string.Format(_language.VideoControls.BeforeChangingTimeInWaveFormX, "#" + paragraph.Number + " " + paragraph.Text));

            int index = _subtitle.GetIndex(paragraph);
            if (index == _subtitleListViewIndex)
            {
                timeUpDownStartTime.TimeCode = paragraph.StartTime;
                decimal durationInSeconds = (decimal) (paragraph.Duration.TotalSeconds);
                if (durationInSeconds >= numericUpDownDuration.Minimum && durationInSeconds <= numericUpDownDuration.Maximum)
                    numericUpDownDuration.Value = durationInSeconds;
            }
            else
            {
                SubtitleListview1.SetStartTime(index, paragraph);
                SubtitleListview1.SetDuration(index, paragraph);
            }
        }

        void AudioWaveForm_OnPositionSelected(double seconds, Paragraph paragraph)
        {
            mediaPlayer.CurrentPosition = seconds;
            if (paragraph != null)
                SubtitleListview1.SelectIndexAndEnsureVisible(_subtitle.GetIndex(paragraph));
        }


        private void VideoPositionChanged(object sender, EventArgs e)
        {
            TimeUpDown tud = (TimeUpDown)sender;
            if (tud.Enabled)
            {
                mediaPlayer.CurrentPosition = tud.TimeCode.TotalSeconds;
            }
        }

        private void Main_Load(object sender, EventArgs e)
        {
            if (Configuration.Settings.General.StartRememberPositionAndSize &&
                !string.IsNullOrEmpty(Configuration.Settings.General.StartPosition))
            {
                string[] parts = Configuration.Settings.General.StartPosition.Split(';');
                if (parts.Length == 2)
                {
                    int x;
                    int y;
                    if (int.TryParse(parts[0], out x) && int.TryParse(parts[1], out y))
                    {
                        if (x > -100 || y > -100)
                        {
                            Left = x;
                            Top = y;
                        }
                    }
                }

                if (Configuration.Settings.General.StartSize == "Maximized")
                {
                    CenterFormOnCurrentScreen();

                    WindowState = FormWindowState.Maximized;
                    return;
                }

                parts = Configuration.Settings.General.StartSize.Split(';');
                if (parts.Length == 2)
                {
                    int x;
                    int y;
                    if (int.TryParse(parts[0], out x) && int.TryParse(parts[1], out y))
                    {
                        Width = x;
                        Height = y;
                    }
                }

                Screen screen = Screen.FromControl(this);

                if (screen.Bounds.Width < Width)
                    Width = screen.Bounds.Width;
                if (screen.Bounds.Height < Height)
                    Height = screen.Bounds.Height;

                if (screen.Bounds.X + screen.Bounds.Width - 200 < Left)
                    Left = screen.Bounds.X + screen.Bounds.Width - Width;
                if (screen.Bounds.Y + screen.Bounds.Height - 100 < Top)
                    Top = screen.Bounds.Y + screen.Bounds.Height - Height;
            }
            else
            {
                CenterFormOnCurrentScreen();
            }

            if (Environment.OSVersion.Version.Major < 6) // 6 == Vista/Win2008Server/Win7
            {
                string unicodeFontName = "Lucida Sans Unicode";
                Configuration.Settings.General.SubtitleFontName = unicodeFontName;
                float fontSize = toolStripMenuItemSingleNote.Font.Size;
                toolStripMenuItemSingleNote.Font = new System.Drawing.Font(unicodeFontName, fontSize);
                toolStripMenuItemDoubleNote.Font = new System.Drawing.Font(unicodeFontName, fontSize);
                toolStripMenuItemSmiley.Font = new System.Drawing.Font(unicodeFontName, fontSize);
                toolStripMenuItemLove.Font = new System.Drawing.Font(unicodeFontName, fontSize);
                textBoxSource.Font = new System.Drawing.Font(unicodeFontName, fontSize);
                textBoxListViewText.Font = new System.Drawing.Font(unicodeFontName, fontSize);
                SubtitleListview1.Font = new System.Drawing.Font(unicodeFontName, fontSize);
            }
        }

        private void InitializeLanguage()
        {
            fileToolStripMenuItem.Text = _language.Menu.File.Title;
            newToolStripMenuItem.Text = _language.Menu.File.New;
            openToolStripMenuItem.Text = _language.Menu.File.Open;
            reopenToolStripMenuItem.Text = _language.Menu.File.Reopen;
            saveToolStripMenuItem.Text = _language.Menu.File.Save;
            saveAsToolStripMenuItem.Text = _language.Menu.File.SaveAs;
            toolStripMenuItemOpenContainingFolder.Text = _language.Menu.File.OpenContainingFolder;
            toolStripMenuItemCompare.Text = _language.Menu.File.Compare;
            toolStripMenuItemImportDvdSubtitles.Text = _language.Menu.File.ImportOcrFromDvd;
            toolStripMenuItemSubIdx.Text = _language.Menu.File.ImportOcrVobSubSubtitle;
            matroskaImportStripMenuItem.Text = _language.Menu.File.ImportSubtitleFromMatroskaFile;
            toolStripMenuItemManualAnsi.Text = _language.Menu.File.ImportSubtitleWithManualChosenEncoding;
            toolStripMenuItemImportText.Text = _language.Menu.File.ImportText;
            toolStripMenuItemImportTimeCodes.Text = _language.Menu.File.ImportTimecodes;
            exitToolStripMenuItem.Text = _language.Menu.File.Exit;

            editToolStripMenuItem.Text = _language.Menu.Edit.Title;
            showHistoryforUndoToolStripMenuItem.Text = _language.Menu.Edit.ShowUndoHistory;
            toolStripMenuItemTranslationMode.Text = _language.Menu.Edit.ShowOriginalText;

            toolStripMenuItemInsertUnicodeCharacter.Text = _language.Menu.Edit.InsertUnicodeSymbol;

            findToolStripMenuItem.Text = _language.Menu.Edit.Find;
            findNextToolStripMenuItem.Text = _language.Menu.Edit.FindNext;
            replaceToolStripMenuItem.Text = _language.Menu.Edit.Replace;
            multipleReplaceToolStripMenuItem.Text = _language.Menu.Edit.MultipleReplace;
            gotoLineNumberToolStripMenuItem.Text = _language.Menu.Edit.GoToSubtitleNumber;

            toolsToolStripMenuItem.Text = _language.Menu.Tools.Title;
            adjustDisplayTimeToolStripMenuItem.Text = _language.Menu.Tools.AdjustDisplayDuration;
            fixToolStripMenuItem.Text = _language.Menu.Tools.FixCommonErrors;
            startNumberingFromToolStripMenuItem.Text = _language.Menu.Tools.StartNumberingFrom;
            removeTextForHearImparedToolStripMenuItem.Text = _language.Menu.Tools.RemoveTextForHearingImpaired;
            ChangeCasingToolStripMenuItem.Text = _language.Menu.Tools.ChangeCasing;
            toolStripMenuItemChangeFramerate.Text = _language.Menu.Tools.ChangeFrameRate;
            toolStripMenuItemAutoMergeShortLines.Text = _language.Menu.Tools.MergeShortLines;
            setMinimumDisplayTimeBetweenParagraphsToolStripMenuItem.Text = _language.Menu.Tools.MinimumDisplayTimeBetweenParagraphs;
            toolStripMenuItem1.Text = _language.Menu.Tools.SortBy;
            sortNumberToolStripMenuItem.Text = _languageGeneral.Number;
            sortStartTimeToolStripMenuItem.Text = _languageGeneral.StartTime;
            sortEndTimeToolStripMenuItem.Text = _languageGeneral.EndTime;
            sortDisplayTimeToolStripMenuItem.Text = _languageGeneral.Duration;
            sortTextAlphabeticallytoolStripMenuItem.Text = _language.Menu.Tools.TextAlphabetically;
            sortTextMaxLineLengthToolStripMenuItem.Text = _language.Menu.Tools.TextSingleLineMaximumLength;
            sortTextTotalLengthToolStripMenuItem.Text = _language.Menu.Tools.TextTotalLength;
            sortTextNumberOfLinesToolStripMenuItem.Text = _language.Menu.Tools.TextNumberOfLines;
            splitToolStripMenuItem.Text = _language.Menu.Tools.SplitSubtitle;
            appendTextVisuallyToolStripMenuItem.Text = _language.Menu.Tools.AppendSubtitle;

            toolStripMenuItemVideo.Text = _language.Menu.Video.Title;
            openVideoToolStripMenuItem.Text = _language.Menu.Video.OpenVideo;
            showhideWaveFormToolStripMenuItem.Text = _language.Menu.Video.ShowHideWaveForm;
            showhideVideoToolStripMenuItem.Text = _language.Menu.Video.ShowHideVideo;

            toolStripMenuItemSpellCheckMain.Text = _language.Menu.SpellCheck.Title;
            spellCheckToolStripMenuItem.Text = _language.Menu.SpellCheck.SpellCheck;
            findDoubleWordsToolStripMenuItem.Text = _language.Menu.SpellCheck.FindDoubleWords;
            GetDictionariesToolStripMenuItem.Text = _language.Menu.SpellCheck.GetDictionaries;
            addWordToNamesetcListToolStripMenuItem.Text = _language.Menu.SpellCheck.AddToNamesEtcList;

            toolStripMenuItemSyncronization.Text = _language.Menu.Synchronization.Title;
            toolStripMenuItemAdjustAllTimes.Text = _language.Menu.Synchronization.AdjustAllTimes;
            visualSyncToolStripMenuItem.Text = _language.Menu.Synchronization.VisualSync;
            toolStripMenuItemPointSync.Text = _language.Menu.Synchronization.PointSync;

            toolStripMenuItemAutoTranslate.Text = _language.Menu.AutoTranslate.Title;
            translateByGoogleToolStripMenuItem.Text = _language.Menu.AutoTranslate.TranslatePoweredByGoogle;
            translatepoweredByMicrosoftToolStripMenuItem.Text = _language.Menu.AutoTranslate.TranslatePoweredByMicrosoft;
            translateFromSwedishToDanishToolStripMenuItem.Text = _language.Menu.AutoTranslate.TranslateFromSwedishToDanish;

            optionsToolStripMenuItem.Text = _language.Menu.Options.Title;
            settingsToolStripMenuItem.Text = _language.Menu.Options.Settings;
            changeLanguageToolStripMenuItem.Text = _language.Menu.Options.ChooseLanguage;
            try
            {
                var ci = new System.Globalization.CultureInfo(_languageGeneral.CultureName);
                changeLanguageToolStripMenuItem.Text += " [" + ci.NativeName + "]";
            }
            catch
            { 
            }

            helpToolStripMenuItem.Text = _language.Menu.Help.Title;
            helpToolStripMenuItem1.Text = _language.Menu.Help.Help;
            aboutToolStripMenuItem.Text = _language.Menu.Help.About;

            toolStripButtonFileNew.ToolTipText = _language.Menu.ToolBar.New;
            toolStripButtonFileOpen.ToolTipText = _language.Menu.ToolBar.Open;
            toolStripButtonSave.ToolTipText = _language.Menu.ToolBar.Save;
            toolStripButtonSaveAs.ToolTipText = _language.Menu.ToolBar.SaveAs;
            toolStripButtonFind.ToolTipText = _language.Menu.ToolBar.Find;
            toolStripButtonReplace.ToolTipText = _language.Menu.ToolBar.Replace;
            toolStripButtonVisualSync.ToolTipText = _language.Menu.ToolBar.VisualSync;
            toolStripButtonSpellCheck.ToolTipText = _language.Menu.ToolBar.SpellCheck;
            toolStripButtonSettings.ToolTipText = _language.Menu.ToolBar.Settings;
            toolStripButtonHelp.ToolTipText = _language.Menu.ToolBar.Help;
            toolStripButtonToogleWaveForm.ToolTipText = _language.Menu.ToolBar.ShowHideWaveForm;
            toolStripButtonToogleVideo.ToolTipText = _language.Menu.ToolBar.ShowHideVideo;
            

            toolStripMenuItemDelete.Text = _language.Menu.ContextMenu.Delete;
            toolStripMenuItemInsertBefore.Text = _language.Menu.ContextMenu.InsertBefore;
            toolStripMenuItemInsertAfter.Text = _language.Menu.ContextMenu.InsertAfter;
            splitLineToolStripMenuItem.Text = _language.Menu.ContextMenu.Split;
            toolStripMenuItemMergeLines.Text = _language.Menu.ContextMenu.MergeSelectedLines;
            mergeBeforeToolStripMenuItem.Text = _language.Menu.ContextMenu.MergeWithLineBefore;
            mergeAfterToolStripMenuItem.Text = _language.Menu.ContextMenu.MergeWithLineAfter;
            normalToolStripMenuItem.Text = _language.Menu.ContextMenu.Normal;
            boldToolStripMenuItem.Text = _languageGeneral.Bold;
            underlineToolStripMenuItem.Text = _language.Menu.ContextMenu.Underline;
            italicToolStripMenuItem.Text = _languageGeneral.Italic;
            colorToolStripMenuItem.Text = _language.Menu.ContextMenu.Color;
            toolStripMenuItemFont.Text = _language.Menu.ContextMenu.FontName;
            toolStripMenuItemAutoBreakLines.Text = _language.Menu.ContextMenu.AutoBalanceSelectedLines;
            toolStripMenuItemUnbreakLines.Text = _language.Menu.ContextMenu.RemoveLineBreaksFromSelectedLines;
            typeEffectToolStripMenuItem.Text = _language.Menu.ContextMenu.TypewriterEffect;
            karokeeEffectToolStripMenuItem.Text = _language.Menu.ContextMenu.KaraokeEffect;
            showSelectedLinesEarlierlaterToolStripMenuItem.Text = _language.Menu.ContextMenu.ShowSelectedLinesEarlierLater;
            visualSyncSelectedLinesToolStripMenuItem.Text = _language.Menu.ContextMenu.VisualSyncSelectedLines;
            googleTranslateSelectedLinesToolStripMenuItem.Text = _language.Menu.ContextMenu.GoogleTranslateSelectedLines;
            adjustDisplayTimeForSelectedLinesToolStripMenuItem.Text = _language.Menu.ContextMenu.AdjustDisplayDurationForSelectedLines;
            fixCommonErrorsInSelectedLinesToolStripMenuItem.Text = _language.Menu.ContextMenu.FixCommonErrorsInSelectedLines;
            changeCasingForSelectedLinesToolStripMenuItem.Text = _language.Menu.ContextMenu.ChangeCasingForSelectedLines;

            // main controls
            SubtitleListview1.InitializeLanguage(_languageGeneral, Configuration.Settings);
            toolStripLabelSubtitleFormat.Text = _language.Controls.SubtitleFormat;
            toolStripLabelEncoding.Text = _language.Controls.FileEncoding;            
            tabControlSubtitle.TabPages[0].Text = _language.Controls.ListView;
            tabControlSubtitle.TabPages[1].Text = _language.Controls.SourceView;
            labelDuration.Text = _languageGeneral.Duration;
            labelStartTime.Text = _languageGeneral.StartTime;
            labelText.Text = _languageGeneral.Text;
            toolStripLabelFrameRate.Text = _languageGeneral.FrameRate;
            buttonUndoListViewChanges.Text = _language.Controls.UndoChangesInEditPanel;
            buttonPrevious.Text = _language.Controls.Previous;
            buttonNext.Text = _language.Controls.Next;
            buttonAutoBreak.Text = _language.Controls.AutoBreak;
            buttonUnBreak.Text = _language.Controls.Unbreak;
            ShowSourceLineNumber();

            // Video controls
            tabPageTranslate.Text = _language.VideoControls.Translate;
            tabPageCreate.Text = _language.VideoControls.Create;
            tabPageAdjust.Text = _language.VideoControls.Adjust;
            checkBoxSyncListViewWithVideWhilePlaying.Text = _language.VideoControls.SelectCurrentElementWhilePlaying;
            if (_videoFileName == null)
                labelVideoInfo.Text = Configuration.Settings.Language.General.NoVideoLoaded;

            groupBoxAutoRepeat.Text = _language.VideoControls.AutoRepeat;
            checkBoxAutoRepeatOn.Text = _language.VideoControls.AutoRepeatOn;
            labelAutoRepeatCount.Text = _language.VideoControls.AutoRepeatCount;
            groupBoxAutoContinue.Text = _language.VideoControls.AutoContinue;
            checkBoxAutoContinue.Text = _language.VideoControls.AutoContinueOn;
            labelAutoContinueDelay.Text = _language.VideoControls.DelayInSeconds;
            buttonPlayPrevious.Text = _language.VideoControls.Previous;
            buttonPlayCurrent.Text = _language.VideoControls.PlayCurrent;
            buttonPlayNext.Text = _language.VideoControls.Next;
            buttonStop.Text = _language.VideoControls.Pause;
            groupBoxTranslateSearch.Text = _language.VideoControls.SearchTextOnline;
            buttonGoogleIt.Text = _language.VideoControls.GoogleIt;
            buttonGoogleTranslateIt.Text = _language.VideoControls.GoogleTranslate;
            labelTranslateTip.Text = _language.VideoControls.TranslateTip;

            buttonInsertNewText.Text = _language.VideoControls.InsertNewSubtitleAtVideoPosition;
            buttonBeforeText.Text = _language.VideoControls.PlayFromJustBeforeText;
            buttonGotoSub.Text = _language.VideoControls.GoToSubtitlePositionAndPause;
            buttonSetStartTime.Text = _language.VideoControls.SetStartTime;
            buttonSetEnd.Text = _language.VideoControls.SetEndTime;
            buttonSecBack1.Text = _language.VideoControls.SecondsBackShort;
            buttonSecBack2.Text = _language.VideoControls.SecondsBackShort;
            buttonForward1.Text = _language.VideoControls.SecondsForwardShort;
            buttonForward2.Text = _language.VideoControls.SecondsForwardShort;
            labelVideoPosition.Text = _language.VideoControls.VideoPosition;
            labelCreateTip.Text = _language.VideoControls.CreateTip;

            buttonSetStartAndOffsetRest.Text = _language.VideoControls.SetstartTimeAndOffsetOfRest;
            buttonSetEndAndGoToNext.Text = _language.VideoControls.SetEndTimeAndGoToNext;
            buttonAdjustSetStartTime.Text = _language.VideoControls.SetStartTime;
            buttonAdjustSetEndTime.Text = _language.VideoControls.SetEndTime;
            buttonAdjustPlayBefore.Text = _language.VideoControls.PlayFromJustBeforeText;
            buttonAdjustGoToPosAndPause.Text = _language.VideoControls.GoToSubtitlePositionAndPause;
            buttonAdjustSecBack1.Text = _language.VideoControls.SecondsBackShort;
            buttonAdjustSecBack2.Text = _language.VideoControls.SecondsBackShort;
            buttonAdjustSecForward1.Text = _language.VideoControls.SecondsForwardShort;
            buttonAdjustSecForward2.Text = _language.VideoControls.SecondsForwardShort;
            labelAdjustTip.Text = _language.VideoControls.CreateTip;

            //waveform
            addParagraphHereToolStripMenuItem.Text = Configuration.Settings.Language.WaveForm.AddParagraphHere;
            deleteParagraphToolStripMenuItem.Text = Configuration.Settings.Language.WaveForm.DeleteParagraph;
            splitToolStripMenuItem1.Text = Configuration.Settings.Language.WaveForm.Split;
            mergeWithPreviousToolStripMenuItem.Text = Configuration.Settings.Language.WaveForm.MergeWithPrevious;
            mergeWithNextToolStripMenuItem.Text = Configuration.Settings.Language.WaveForm.MergeWithNext;
            toolStripMenuItemWaveFormPlaySelection.Text = Configuration.Settings.Language.WaveForm.PlaySelection;

            toolStripButtonWaveFormZoomOut.ToolTipText = Configuration.Settings.Language.WaveForm.ZoomOut;
            toolStripButtonWaveFormZoomIn.ToolTipText = Configuration.Settings.Language.WaveForm.ZoomIn;

            AudioWaveForm.WaveFormNotLoadedText = Configuration.Settings.Language.WaveForm.ClickToAddWaveForm;
        }

        private void SetFormatToSubRip()
        {
            comboBoxSubtitleFormats.SelectedIndexChanged -= ComboBoxSubtitleFormatsSelectedIndexChanged;
            foreach (SubtitleFormat format in SubtitleFormat.AllSubtitleFormats)
            {
                if (!format.IsVobSubIndexFile)
                    comboBoxSubtitleFormats.Items.Add(format.FriendlyName);
            }
            comboBoxSubtitleFormats.SelectedIndex = 0;
            comboBoxSubtitleFormats.SelectedIndexChanged += ComboBoxSubtitleFormatsSelectedIndexChanged;
        }

        private bool ContinueNewOrExit()
        {
            if (_change)
            {
                string promptText = _language.SaveChangesToUntitled;
                if (!string.IsNullOrEmpty(_fileName))
                    promptText = string.Format(_language.SaveChangesToX, _fileName);

                DialogResult dr = MessageBox.Show(promptText, Title, MessageBoxButtons.YesNoCancel, MessageBoxIcon.Exclamation);

                if (dr == DialogResult.Cancel)
                    return false;

                if (dr == DialogResult.No)
                    return true;

                if (string.IsNullOrEmpty(_fileName))
                {
                    saveFileDialog1.Title = _language.SaveSubtitleAs;
                    if (saveFileDialog1.ShowDialog(this) == DialogResult.OK)
                    {
                        _fileName = saveFileDialog1.FileName;
                        Text = Title + " - " + _fileName;
                        Configuration.Settings.RecentFiles.Add(_fileName);
                        Configuration.Settings.Save();

                        if (SaveSubtitle(GetCurrentSubtitleFormat()) == DialogResult.OK)
                            return true;
                    }
                    return false;
                }
                if (SaveSubtitle(GetCurrentSubtitleFormat()) == DialogResult.OK)
                    return true;
                return false;
            }
            return true;
        }

        private void ExitToolStripMenuItemClick(object sender, EventArgs e)
        {
            ReloadFromSourceView();
            Application.Exit();
        }

        private void AboutToolStripMenuItemClick(object sender, EventArgs e)
        {
            ReloadFromSourceView();
            new About().ShowDialog(this);
        }

        private void VisualSyncToolStripMenuItemClick(object sender, EventArgs e)
        {
            ReloadFromSourceView();
            ShowVisualSync(false);
        }

        public void MakeHistoryForUndo(string description)
        {
            _subtitle.MakeHistoryForUndo(description, GetCurrentSubtitleFormat(), _fileDateTime);
        }

        /// <summary>
        /// Add undo history - but only if nothing happens for half a second
        /// </summary>
        /// <param name="description">Undo description</param>
        public void MakeHistoryForUndoWhenNoMoreChanges(string description)
        {
            _timerAddHistoryWhenDone.Stop();
            _timerAddHistoryWhenDoneText = description;
            _timerAddHistoryWhenDone.Start();
        }

        private void ShowVisualSync(bool onlySelectedLines)
        {
            if (_subtitle != null && _subtitle.Paragraphs.Count > 1)
            {
                var visualSync = new VisualSync();
                _formPositionsAndSizes.SetPositionAndSize(visualSync);
                visualSync.VideoFileName = _videoFileName;

                SaveSubtitleListviewIndexes();
                if (onlySelectedLines)
                {
                    var selectedLines = new Subtitle { WasLoadedWithFrameNumbers = _subtitle.WasLoadedWithFrameNumbers };
                    foreach (int index in SubtitleListview1.SelectedIndices)
                        selectedLines.Paragraphs.Add(_subtitle.Paragraphs[index]);
                    visualSync.Initialize(this.Icon, selectedLines, _fileName, _language.VisualSyncSelectedLines, CurrentFrameRate);
                }
                else
                {
                    visualSync.Initialize(this.Icon, _subtitle, _fileName, _language.VisualSyncTitle, CurrentFrameRate);
                }

                if (visualSync.ShowDialog(this) == DialogResult.OK)
                {
                    MakeHistoryForUndo(_language.BeforeVisualSync);

                    if (onlySelectedLines)
                    { // we only update selected lines
                        int i = 0;
                        foreach (int index in SubtitleListview1.SelectedIndices)
                        {
                            _subtitle.Paragraphs[index] = visualSync.Paragraphs[i];
                            i++;
                        }
                        ShowStatus(_language.VisualSyncPerformedOnSelectedLines);
                    }
                    else
                    {
                        _subtitle.Paragraphs.Clear();
                        foreach (Paragraph p in visualSync.Paragraphs)
                            _subtitle.Paragraphs.Add(new Paragraph(p));
                        ShowStatus(_language.VisualSyncPerformed);
                    }
                    if (visualSync.FrameRateChanged)
                        toolStripComboBoxFrameRate.Text = visualSync.FrameRate.ToString();
                    if (IsFramesRelevant && visualSync.FrameRate > 0)
                        _subtitle.CalculateFrameNumbersFromTimeCodesNoCheck(CurrentFrameRate);
                    ShowSource();
                    SubtitleListview1.Fill(_subtitle, _subtitleAlternate);
                    RestoreSubtitleListviewIndexes();
                    if (onlySelectedLines && SubtitleListview1.SelectedItems.Count > 0)
                    {
                        SubtitleListview1.EnsureVisible(SubtitleListview1.SelectedItems[SubtitleListview1.SelectedItems.Count - 1].Index);
                    }
                }
                _videoFileName = visualSync.VideoFileName;
                _formPositionsAndSizes.SavePositionAndSize(visualSync);
                visualSync.Dispose();
            }
            else
            {
                MessageBox.Show(_language.NoSubtitleLoaded, Title, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

        }

        private void OpenToolStripMenuItemClick(object sender, EventArgs e)
        {
            ReloadFromSourceView();
            OpenNewFile();
        }

        private void OpenNewFile()
        {
            openFileDialog1.Title = _languageGeneral.OpenSubtitle;
            openFileDialog1.FileName = string.Empty;
            openFileDialog1.Filter = Utilities.GetOpenDialogFiler();
            if (openFileDialog1.ShowDialog(this) == DialogResult.OK)
            {
                OpenSubtitle(openFileDialog1.FileName, null);
            }
        }

        public double CurrentFrameRate
        {
            get
            {
                double f;
                if (double.TryParse(toolStripComboBoxFrameRate.Text, out f))
                    return f;
                return Configuration.Settings.General.DefaultFrameRate;
            }
        }

        private void OpenSubtitle(string fileName, Encoding encoding)
        {
            if (File.Exists(fileName))
            {
                openFileDialog1.InitialDirectory = Path.GetDirectoryName(fileName);

                if (Path.GetExtension(fileName).ToLower() == ".sub" && IsVobSubFile(fileName, false))
                {
                    if (MessageBox.Show(_language.ImportThisVobSubSubtitle, _title, MessageBoxButtons.YesNo) == DialogResult.Yes)
                    {
                        ImportAndOcrVobSubSubtitleNew(fileName);
                    }
                    return;
                }

                var fi = new FileInfo(fileName);
                if (fi.Length > 1024 * 1024 * 10) // max 10 mb
                {
                    if (MessageBox.Show(string.Format(_language.FileXIsLargerThan10Mb + Environment.NewLine +
                                                      Environment.NewLine +
                                                      _language.ContinueAnyway,
                                                      fileName), Title, MessageBoxButtons.YesNoCancel) != DialogResult.Yes)
                        return;
                }

                MakeHistoryForUndo(string.Format(_language.BeforeLoadOf, Path.GetFileName(fileName)));

                SubtitleFormat format = _subtitle.LoadSubtitle(fileName, out encoding, encoding);

                bool justConverted = false;
                if (format == null)
                {
                    Ebu ebu = new Ebu();
                    if (ebu.IsMine(null, fileName))
                    {
                        ebu.LoadSubtitle(_subtitle, null, fileName);
                        _oldSubtitleFormat = ebu;
                        SetFormatToSubRip();
                        justConverted = true;
                    }
                    format = GetCurrentSubtitleFormat();
                }

                _fileDateTime = File.GetLastWriteTime(fileName);

                if (GetCurrentSubtitleFormat().IsFrameBased)
                    _subtitle.CalculateTimeCodesFromFrameNumbers(CurrentFrameRate);
                else
                    _subtitle.CalculateFrameNumbersFromTimeCodes(CurrentFrameRate);

                if (format != null)
                {
                    if (Configuration.Settings.General.RemoveBlankLinesWhenOpening)
                    {
                        _subtitle.RemoveEmptyLines();
                    }

                    _subtitleListViewIndex = -1;

                    if (format.FriendlyName == new Sami().FriendlyName)
                        encoding = Encoding.Default;

                    SetCurrentFormat(format);

                    textBoxSource.Text = _subtitle.ToText(format);
                    SubtitleListview1.Fill(_subtitle, _subtitleAlternate);
                    if (SubtitleListview1.Items.Count > 0)
                        SubtitleListview1.Items[0].Selected = true;
                    _findHelper = null;
                    _spellCheckForm = null;
                    _videoFileName = null;
                    labelVideoInfo.Text = Configuration.Settings.Language.General.NoVideoLoaded;
                    AudioWaveForm.WavePeaks = null;
                    AudioWaveForm.Invalidate();

                    if (Configuration.Settings.RecentFiles.FileNames.Count > 0 &&
                        Configuration.Settings.RecentFiles.FileNames[0] == fileName)
                    {
                    }
                    else
                    {
                        Configuration.Settings.RecentFiles.Add(fileName);
                        Configuration.Settings.Save();
                        UpdateRecentFilesUI();
                    }
                    _fileName = fileName;
                    Text = Title + " - " + _fileName;
                    ShowStatus(string.Format(_language.LoadedSubtitleX, _fileName));
                    _sourceViewChange = false;
                    _change = false;
                    _converted = false;

                    if (justConverted)
                    {
                        _converted = true;
                        ShowStatus(string.Format(_language.LoadedSubtitleX, _fileName) + " - " + string.Format(_language.ConvertedToX, format.FriendlyName));                        
                    }

                    if (encoding == Encoding.UTF7)
                        comboBoxEncoding.Text = "UTF-7";
                    else if (encoding == Encoding.UTF8)
                        comboBoxEncoding.Text = "UTF-8";
                    else if (encoding == System.Text.Encoding.Unicode)
                        comboBoxEncoding.Text = "Unicode";
                    else if (encoding == System.Text.Encoding.BigEndianUnicode)
                        comboBoxEncoding.Text = "Unicode (big endian)";
                    else
                    {
                        comboBoxEncoding.Items[0] = "ANSI - " + encoding.CodePage.ToString();
                        comboBoxEncoding.SelectedIndex = 0;
                    }
                }
                else
                {
                    var info = new FileInfo(fileName);
                    if (info.Length < 50)
                    {
                        _findHelper = null;
                        _spellCheckForm = null;
                        _videoFileName = null;
                        labelVideoInfo.Text = Configuration.Settings.Language.General.NoVideoLoaded;
                        AudioWaveForm.WavePeaks = null;
                        AudioWaveForm.Invalidate();

                        Configuration.Settings.RecentFiles.Add(fileName);
                        Configuration.Settings.Save();
                        UpdateRecentFilesUI();
                        _fileName = fileName;
                        Text = Title + " - " + _fileName;
                        ShowStatus(string.Format(_language.LoadedEmptyOrShort, _fileName));
                        _sourceViewChange = false;
                        _change = false;
                        _converted = false;

                        MessageBox.Show(_language.FileIsEmptyOrShort);
                    }
                    else
                        ShowUnknownSubtitle();
                }
                if (!string.IsNullOrEmpty(_fileName) && (toolStripButtonToogleVideo.Checked || toolStripButtonToogleWaveForm.Checked))
                {
                    TryToFindAndOpenVideoFile(Path.Combine(Path.GetDirectoryName(_fileName), Path.GetFileNameWithoutExtension(_fileName)));
                }
                else
                {
                    if (mediaPlayer.VideoPlayer != null)
                    {
                        mediaPlayer.VideoPlayer.DisposeVideoPlayer();
                        mediaPlayer.VideoPlayer = null;
                        timer1.Stop();
                    }
                }
            }
            else
            {
                MessageBox.Show(string.Format(_language.FileNotFound, fileName));
            }
        }

        private void ShowUnknownSubtitle()
        {
            var unknownSubtitle = new UnknownSubtitle();
            unknownSubtitle.Initialize(Title);
            unknownSubtitle.ShowDialog(this);
        }

        private void UpdateRecentFilesUI()
        {
            reopenToolStripMenuItem.DropDownItems.Clear();
            if (Configuration.Settings.General.ShowRecentFiles &&
                Configuration.Settings.RecentFiles.FileNames.Count > 0)
            {
                reopenToolStripMenuItem.Visible = true;
                foreach (string fileName in Configuration.Settings.RecentFiles.FileNames)
                {
                    if (File.Exists(fileName))
                        reopenToolStripMenuItem.DropDownItems.Add(fileName, null, ReopenSubtitleToolStripMenuItemClick);
                }
            }
            else
            {
                Configuration.Settings.RecentFiles.FileNames.Clear();
                reopenToolStripMenuItem.Visible = false;
            }
        }

        private void ReopenSubtitleToolStripMenuItemClick(object sender, EventArgs e)
        {
            ReloadFromSourceView();
            var item = sender as ToolStripItem;

            if (ContinueNewOrExit())
                OpenSubtitle(item.Text, null);
        }

        private void SaveToolStripMenuItemClick(object sender, EventArgs e)
        {
            ReloadFromSourceView();
            SaveSubtitle(GetCurrentSubtitleFormat());
        }

        private void SaveAsToolStripMenuItemClick(object sender, EventArgs e)
        {
            ReloadFromSourceView();
            FileSaveAs();
        }

        private DialogResult FileSaveAs()
        {
            SubtitleFormat currentFormat = GetCurrentSubtitleFormat();
            Utilities.SetSaveDialogFilter(saveFileDialog1, currentFormat);
            saveFileDialog1.Title = _language.SaveSubtitleAs;
            saveFileDialog1.DefaultExt = "*" + currentFormat.Extension;
            saveFileDialog1.AddExtension = true;               
 
            if (!string.IsNullOrEmpty(_videoFileName))
                saveFileDialog1.FileName = Path.GetFileNameWithoutExtension(_videoFileName);
            else
                saveFileDialog1.FileName = Path.GetFileNameWithoutExtension(_fileName);

            DialogResult result = saveFileDialog1.ShowDialog(this);
            if (result == DialogResult.OK)
            {
                _converted = false;
                _fileName = saveFileDialog1.FileName;

                // only allow current extension or ".txt"
                string ext = Path.GetExtension(_fileName).ToLower();
                bool extOk = ext == currentFormat.Extension.ToLower() || currentFormat.AlternateExtensions.Contains(ext) || ext == ".txt";
                if (!extOk)
                {
                    if (_fileName.EndsWith("."))
                        _fileName = _fileName.Substring(0, _fileName.Length - 1);
                    _fileName += currentFormat.Extension;
                }

                _fileDateTime = File.GetLastWriteTime(_fileName);
                Text = Title + " - " + _fileName;
                Configuration.Settings.RecentFiles.Add(_fileName);
                Configuration.Settings.Save();

                int index = 0;
                foreach (SubtitleFormat format in SubtitleFormat.AllSubtitleFormats)
                {
                    if (saveFileDialog1.FilterIndex == index +1)
                        SaveSubtitle(format);
                    index++;
                }
            }
            return result;
        }

        private Encoding GetCurrentEncoding()
        {
            if (comboBoxEncoding.Text == "UTF-7")
                return System.Text.Encoding.UTF7;
            if (comboBoxEncoding.Text == "UTF-8")
                return System.Text.Encoding.UTF8;
            else if (comboBoxEncoding.Text == "Unicode")
                return System.Text.Encoding.Unicode;
            else if (comboBoxEncoding.Text == "Unicode (big endian)")
                return System.Text.Encoding.BigEndianUnicode;
            else 
            {
                if (comboBoxEncoding.Text.StartsWith("ANSI - "))
                {
                    string codePage = comboBoxEncoding.Text.Substring(6).Trim();
                    int codePageNumber = 0;
                    if (int.TryParse(codePage, out codePageNumber))
                    {
                        return Encoding.GetEncoding(codePageNumber);
                    }
                }
                return System.Text.Encoding.Default;
            }
        }

        private DialogResult SaveSubtitle(SubtitleFormat format)
        {           
            if (string.IsNullOrEmpty(_fileName) || _converted)
                return FileSaveAs();

            try
            {
                string allText = _subtitle.ToText(format).Trim();
                var currentEncoding = GetCurrentEncoding();
                if (currentEncoding == Encoding.Default && (allText.Contains("♪") || allText.Contains("♫") | allText.Contains("♥"))) // ANSI & music/unicode symbols
                {
                    if (MessageBox.Show(string.Format(_language.UnicodeMusicSymbolsAnsiWarning), Title, MessageBoxButtons.YesNo) == DialogResult.No)
                        return DialogResult.No;
                }


                bool containsNegativeTime = false;
                foreach (var p in _subtitle.Paragraphs)
                {
                    if (p.StartTime.TotalMilliseconds < 0 || p.EndTime.TotalMilliseconds < 0)
                    {
                        containsNegativeTime = true;
                        break;
                    }
                }
                if (containsNegativeTime && !string.IsNullOrEmpty(_language.NegativeTimeWarning))
                {
                    if (MessageBox.Show(_language.NegativeTimeWarning, Title, MessageBoxButtons.YesNo) == DialogResult.No)
                        return DialogResult.No;
                }

                if (File.Exists(_fileName))
                {
                    DateTime fileOnDisk = File.GetLastWriteTime(_fileName);
                    if (_fileDateTime != fileOnDisk && _fileDateTime != new DateTime())
                    {
                        if (MessageBox.Show(string.Format(_language.OverwriteModifiedFile,
                                                          _fileName, fileOnDisk.ToShortDateString(), fileOnDisk.ToString("HH:mm:ss"),
                                                          Environment.NewLine, _fileDateTime.ToShortDateString(), _fileDateTime.ToString("HH:mm:ss")),
                                             Title + " - " + _language.FileOnDiskModified, MessageBoxButtons.YesNo) == DialogResult.No)
                            return DialogResult.No;
                    }
                    File.Delete(_fileName);
                }

                File.WriteAllText(_fileName, allText, currentEncoding);
                _fileDateTime = File.GetLastWriteTime(_fileName);
                ShowStatus(string.Format(_language.SavedSubtitleX, _fileName));
                _change = false;
                return DialogResult.OK;
            }
            catch
            {
                MessageBox.Show(string.Format(_language.UnableToSaveSubtitleX, _fileName));
                return DialogResult.Cancel;
            }
        }

        private void NewToolStripMenuItemClick(object sender, EventArgs e)
        {
            ReloadFromSourceView();
            FileNew();
        }

        private void ResetSubtitle()
        {
            SetCurrentFormat(new SubRip());
            _subtitle = new Subtitle(_subtitle.HistoryItems);
            _subtitleAlternate = new Subtitle();
            textBoxSource.Text = string.Empty;
            SubtitleListview1.Items.Clear();
            _fileName = string.Empty;
            _fileDateTime = new DateTime();
            Text = Title;
            _oldSubtitleFormat = null;
            

            comboBoxEncoding.Items[0] = "ANSI - " + Encoding.Default.CodePage.ToString();
            if (Configuration.Settings.General.DefaultEncoding == "ANSI")
                comboBoxEncoding.SelectedIndex = 0;
            else
                comboBoxEncoding.Text = Configuration.Settings.General.DefaultEncoding;

            toolStripComboBoxFrameRate.Text = Configuration.Settings.General.DefaultFrameRate.ToString();
            _findHelper = null;
            _spellCheckForm = null;
            _videoFileName = null;
            labelVideoInfo.Text = Configuration.Settings.Language.General.NoVideoLoaded;
            AudioWaveForm.WavePeaks = null;
            AudioWaveForm.Invalidate();

            ShowStatus(_language.New);
            _sourceViewChange = false;

            _subtitleListViewIndex = -1;
            textBoxListViewText.Text = string.Empty;
            textBoxListViewText.Enabled = false;
            labelTextLineLengths.Text = string.Empty;
            labelTextLineTotal.Text = string.Empty;

            if (mediaPlayer.VideoPlayer != null)
            {
                mediaPlayer.VideoPlayer.DisposeVideoPlayer();
                mediaPlayer.VideoPlayer = null;
            }

            _change = false;
            _converted = false;

        }

        private void FileNew()
        {
            if (ContinueNewOrExit())
            {
                MakeHistoryForUndo(_language.BeforeNew);
                ResetSubtitle();
            }
        }

        private void ComboBoxSubtitleFormatsSelectedIndexChanged(object sender, EventArgs e)
        {            
            _converted = true;
            if (_oldSubtitleFormat == null)
            {
                MakeHistoryForUndo(string.Format(_language.BeforeConvertingToX, GetCurrentSubtitleFormat().FriendlyName));
            }
            else
            {
                _subtitle.MakeHistoryForUndo(string.Format(_language.BeforeConvertingToX, GetCurrentSubtitleFormat().FriendlyName), _oldSubtitleFormat, _fileDateTime);

                _oldSubtitleFormat.RemoveNativeFormatting(_subtitle);

                SaveSubtitleListviewIndexes();
                SubtitleListview1.Fill(_subtitle, _subtitleAlternate);
                RestoreSubtitleListviewIndexes();
            }
            ShowSource();

            SubtitleFormat format = GetCurrentSubtitleFormat();
            if (format != null)
            {
                ShowStatus(string.Format(_language.ConvertedToX, format.FriendlyName));
                _oldSubtitleFormat = format;
            }
        }

        private void ComboBoxSubtitleFormatsEnter(object sender, EventArgs e)
        {
            SubtitleFormat format = GetCurrentSubtitleFormat();
            if (format != null)
                _oldSubtitleFormat = format;
        }

        private SubtitleFormat GetCurrentSubtitleFormat()
        {
            return Utilities.GetSubtitleFormatByFriendlyName(comboBoxSubtitleFormats.SelectedItem.ToString());
        }

        private void ShowSource()
        {
            if (_subtitle != null && _subtitle.Paragraphs.Count > 0)
            {
                SubtitleFormat format = GetCurrentSubtitleFormat();
                if (format != null)
                {
                    if (GetCurrentSubtitleFormat().IsFrameBased)
                        _subtitle.CalculateTimeCodesFromFrameNumbers(CurrentFrameRate);
                    else
                        _subtitle.CalculateFrameNumbersFromTimeCodes(CurrentFrameRate);

                    textBoxSource.TextChanged -= TextBoxSourceTextChanged;
                    textBoxSource.Text = _subtitle.ToText(format);
                    textBoxSource.TextChanged += TextBoxSourceTextChanged;
                    return;
                }
            }
            textBoxSource.Text = string.Empty;
        }

        private void SettingsToolStripMenuItemClick(object sender, EventArgs e)
        {
            ShowSettings();
        }

        private void ShowSettings()
        {
            string oldListViewLineSeparatorString = Configuration.Settings.General.ListViewLineSeparatorString;
            string oldSubtitleFontSettings = Configuration.Settings.General.SubtitleFontName + 
                                          Configuration.Settings.General.SubtitleFontBold +
                                          Configuration.Settings.General.SubtitleFontSize;


            var settings = new Settings();
            settings.Initialize(this.Icon, toolStripButtonFileNew.Image, toolStripButtonFileOpen.Image, toolStripButtonSave.Image, toolStripButtonSaveAs.Image, 
                                toolStripButtonFind.Image, toolStripButtonReplace.Image, toolStripButtonVisualSync.Image, toolStripButtonSpellCheck.Image, toolStripButtonSettings.Image, toolStripButtonHelp.Image);
            _formPositionsAndSizes.SetPositionAndSize(settings);
            settings.ShowDialog(this);
            _formPositionsAndSizes.SavePositionAndSize(settings);
            settings.Dispose();

            InitializeToolbar();
            UpdateRecentFilesUI();
            InitializeSubtitleFont();
            buttonCustomUrl.Text = Configuration.Settings.VideoControls.CustomSearchText;
            buttonCustomUrl.Enabled = Configuration.Settings.VideoControls.CustomSearchUrl.Length > 1;

            AudioWaveForm.DrawGridLines = Configuration.Settings.VideoControls.WaveFormDrawGrid;
            AudioWaveForm.GridColor = Configuration.Settings.VideoControls.WaveFormGridColor;
            AudioWaveForm.SelectedColor = Configuration.Settings.VideoControls.WaveFormSelectedColor;
            AudioWaveForm.Color = Configuration.Settings.VideoControls.WaveFormColor;
            AudioWaveForm.BackgroundColor = Configuration.Settings.VideoControls.WaveFormBackgroundColor;
            AudioWaveForm.TextColor =  Configuration.Settings.VideoControls.WaveFormTextColor; 

            if (oldSubtitleFontSettings != Configuration.Settings.General.SubtitleFontName + 
                                          Configuration.Settings.General.SubtitleFontBold +
                                          Configuration.Settings.General.SubtitleFontSize)
            {
                Utilities.InitializeSubtitleFont(textBoxListViewText);
                SubtitleListview1.SubtitleFontName = Configuration.Settings.General.SubtitleFontName;
                SubtitleListview1.SubtitleFontBold = Configuration.Settings.General.SubtitleFontBold;
                SubtitleListview1.SubtitleFontSize = Configuration.Settings.General.SubtitleFontSize;
                SaveSubtitleListviewIndexes();
                SubtitleListview1.Fill(_subtitle, _subtitleAlternate);
                RestoreSubtitleListviewIndexes();
            }

            if (oldListViewLineSeparatorString != Configuration.Settings.General.ListViewLineSeparatorString)
            {
                SubtitleListview1.InitializeLanguage(_languageGeneral, Configuration.Settings);
                SaveSubtitleListviewIndexes();
                SubtitleListview1.Fill(_subtitle, _subtitleAlternate);
                RestoreSubtitleListviewIndexes();
            }
        }

        private void InitializeSubtitleFont()
        {
            var gs = Configuration.Settings.General;

            if (string.IsNullOrEmpty(gs.SubtitleFontName))
                gs.SubtitleFontName = "Tahoma";

            if (gs.SubtitleFontBold)
            {
                textBoxSource.Font = new System.Drawing.Font(gs.SubtitleFontName, gs.SubtitleFontSize, System.Drawing.FontStyle.Bold);
                textBoxListViewText.Font = new System.Drawing.Font(gs.SubtitleFontName, gs.SubtitleFontSize, System.Drawing.FontStyle.Bold);
            }
            else
            {
                textBoxSource.Font = new System.Drawing.Font(gs.SubtitleFontName, gs.SubtitleFontSize);
                textBoxListViewText.Font = new System.Drawing.Font(gs.SubtitleFontName, gs.SubtitleFontSize);
            }
        }

        private void TryLoadIcon(ToolStripButton button, string iconName)
        { 
            string fullPath = Configuration.BaseDirectory + @"Icons\" + iconName + ".png";
            if (File.Exists(fullPath))
                button.Image = new Bitmap(fullPath);
        }

        private void InitializeToolbar()
        {
            GeneralSettings gs = Configuration.Settings.General;          

            TryLoadIcon(toolStripButtonFileNew, "New");
            TryLoadIcon(toolStripButtonFileOpen, "Open");
            TryLoadIcon(toolStripButtonSave, "Save");
            TryLoadIcon(toolStripButtonSaveAs, "SaveAs");
            TryLoadIcon(toolStripButtonFind, "Find");
            TryLoadIcon(toolStripButtonReplace, "Replace");
            TryLoadIcon(toolStripButtonVisualSync, "VisualSync");
            TryLoadIcon(toolStripButtonSettings, "Settings");
            TryLoadIcon(toolStripButtonSpellCheck, "SpellCheck");
            TryLoadIcon(toolStripButtonHelp, "Help");

            TryLoadIcon(toolStripButtonToogleVideo, "VideoToogle");
            TryLoadIcon(toolStripButtonToogleWaveForm, "WaveFormToogle");

            toolStripButtonFileNew.Visible = gs.ShowToolbarNew;
            toolStripButtonFileOpen.Visible = gs.ShowToolbarOpen;
            toolStripButtonSave.Visible = gs.ShowToolbarSave;
            toolStripButtonSaveAs.Visible = gs.ShowToolbarSaveAs;
            toolStripButtonFind.Visible = gs.ShowToolbarFind;
            toolStripButtonReplace.Visible = gs.ShowToolbarReplace;
            toolStripButtonVisualSync.Visible = gs.ShowToolbarVisualSync;
            toolStripButtonSettings.Visible = gs.ShowToolbarSettings;
            toolStripButtonSpellCheck.Visible = gs.ShowToolbarSpellCheck;
            toolStripButtonHelp.Visible = gs.ShowToolbarHelp;

            toolStripSeparatorFrameRate.Visible = gs.ShowFrameRate;
            toolStripLabelFrameRate.Visible = gs.ShowFrameRate;
            toolStripComboBoxFrameRate.Visible = gs.ShowFrameRate;
            toolStripButtonGetFrameRate.Visible = gs.ShowFrameRate;

            toolStripSeparatorFindReplace.Visible = gs.ShowToolbarFind || gs.ShowToolbarReplace;
            toolStripSeparatorHelp.Visible = gs.ShowToolbarHelp;

        }

        private void ToolStripButtonFileNewClick(object sender, EventArgs e)
        {
            ReloadFromSourceView();
            FileNew();
        }

        private void ToolStripButtonFileOpenClick(object sender, EventArgs e)
        {
            ReloadFromSourceView();
            OpenNewFile();
        }

        private void ToolStripButtonSaveClick(object sender, EventArgs e)
        {
            ReloadFromSourceView();
            SaveSubtitle(GetCurrentSubtitleFormat());
        }

        private void ToolStripButtonSaveAsClick(object sender, EventArgs e)
        {
            ReloadFromSourceView();
            FileSaveAs();
        }

        private void ToolStripButtonFindClick(object sender, EventArgs e)
        {
            ReloadFromSourceView();
            Find();
        }

        private void ToolStripButtonVisualSyncClick(object sender, EventArgs e)
        {
            ReloadFromSourceView();
            ShowVisualSync(false);
        }

        private void ToolStripButtonSettingsClick(object sender, EventArgs e)
        {
            ReloadFromSourceView();
            ShowSettings();
        }
        
        private void TextBoxSourceClick(object sender, EventArgs e)
        {
            ShowSourceLineNumber();
        }

        private void TextBoxSourceKeyDown(object sender, KeyEventArgs e)
        {            
            ShowSourceLineNumber();
            e.Handled = false;

            if (e.Modifiers == Keys.Control && e.KeyCode == Keys.A)
            {
                textBoxSource.SelectAll();
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
            if (e.Modifiers == Keys.Control && e.KeyCode == Keys.D)
            {
                textBoxSource.SelectionLength = 0;
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
        }

        private void TextBoxSourceTextChanged(object sender, EventArgs e)
        {
            ShowSourceLineNumber();
            _sourceViewChange = true;
            _change = true;
        }


        private void ShowSourceLineNumber()
        {
            if (tabControlSubtitle.SelectedIndex == TabControlSourceView)
            {
                string number = textBoxSource.GetLineFromCharIndex(textBoxSource.SelectionStart).ToString();
                if (number.Length > 0)
                    toolStripSelected.Text = string.Format(_language.LineNumberX, int.Parse(number) + 1);
                else
                    toolStripSelected.Text = string.Empty;
            }
        }

        private void ButtonGetFrameRateClick(object sender, EventArgs e)
        {
            openFileDialog1.Title = _language.OpenVideoFile;
            openFileDialog1.FileName = string.Empty;
            openFileDialog1.Filter = Utilities.GetVideoFileFilter(); 
            if (openFileDialog1.ShowDialog(this) == DialogResult.OK)
            {
                _videoFileName = openFileDialog1.FileName;
                VideoInfo info = Utilities.GetVideoInfo(openFileDialog1.FileName, delegate { Application.DoEvents(); });
                if (info != null && info.Success)
                {
                    string oldFrameRate = toolStripComboBoxFrameRate.Text;
                    toolStripComboBoxFrameRate.Text = info.FramesPerSecond.ToString();

                    if (oldFrameRate != toolStripComboBoxFrameRate.Text)
                    {
                        ShowSource();
                        SubtitleListview1.Fill(_subtitle, _subtitleAlternate);

                        SubtitleFormat format = Utilities.GetSubtitleFormatByFriendlyName(comboBoxSubtitleFormats.SelectedItem.ToString());
                        if (_subtitle.WasLoadedWithFrameNumbers && format.IsTimeBased)
                        {
                            MessageBox.Show(string.Format(_language.NewFrameRateUsedToCalculateTimeCodes, info.FramesPerSecond));
                        }
                        else if (!_subtitle.WasLoadedWithFrameNumbers && format.IsFrameBased)
                        {
                            MessageBox.Show(string.Format(_language.NewFrameRateUsedToCalculateFrameNumbers, info.FramesPerSecond));
                        }
                    }
                }
            }
        }

        private void FindToolStripMenuItemClick(object sender, EventArgs e)
        {
            ReloadFromSourceView();
            Find();
        }

        private void Find()
        {
            string selectedText;
            if (tabControlSubtitle.SelectedIndex == TabControlSourceView)                
                selectedText = textBoxSource.SelectedText;
            else
                selectedText = textBoxListViewText.SelectedText;

            if (selectedText.Length == 0 && _findHelper != null)
                selectedText = _findHelper.FindText;

            var findDialog = new FindDialog();
            findDialog.Initialize(selectedText, _findHelper);
            if (findDialog.ShowDialog(this) == DialogResult.OK)
            {
                _findHelper = findDialog.GetFindDialogHelper(_subtitleListViewIndex);
                if (tabControlSubtitle.SelectedIndex == TabControlListView)
                {
                    int selectedIndex = -1;
                    //set the starting selectedIndex if a row is highlighted
                    if (SubtitleListview1.SelectedItems.Count > 0)
                        selectedIndex = SubtitleListview1.SelectedItems[0].Index;

                    //if we fail to find the text, we might want to start searching from the top of the file.
                    bool foundIt = false;
                    if (_findHelper.Find(_subtitle, selectedIndex))
                    {
                        foundIt = true;
                    }
                    else if (_findHelper.StartLineIndex >= 1)
                    {
                        if (MessageBox.Show(_language.FindContinue, _language.FindContinueTitle, MessageBoxButtons.YesNo) == DialogResult.Yes)
                        {
                            selectedIndex = -1;
                            if (_findHelper.Find(_subtitle, selectedIndex))
                                foundIt = true;
                        }
                    }

                    if (foundIt)
                    {
                        SubtitleListview1.SelectIndexAndEnsureVisible(_findHelper.SelectedIndex);
                        textBoxListViewText.Focus();
                        textBoxListViewText.SelectionStart = _findHelper.SelectedPosition;
                        textBoxListViewText.SelectionLength = _findHelper.FindTextLength;
                        ShowStatus(string.Format(_language.XFoundAtLineNumberY, _findHelper.FindText, _findHelper.SelectedIndex + 1));
                        _findHelper.SelectedPosition++;
                    }
                    else
                    {
                        ShowStatus(string.Format(_language.XNotFound, _findHelper.FindText));
                    }
                }
                else if (tabControlSubtitle.SelectedIndex == TabControlSourceView)
                {
                    if (_findHelper.Find(textBoxSource, textBoxSource.SelectionStart))
                    {
                        textBoxSource.SelectionStart = _findHelper.SelectedIndex;
                        textBoxSource.SelectionLength = _findHelper.FindTextLength;
                        textBoxSource.ScrollToCaret();
                        ShowStatus(string.Format(_language.XFoundAtLineNumberY, _findHelper.FindText, textBoxSource.GetLineFromCharIndex(textBoxSource.SelectionStart)));
                    }
                    else
                    {
                        ShowStatus(string.Format(_language.XNotFound, _findHelper.FindText));
                    }
                }
            }
            findDialog.Dispose();
        }

        private void FindNextToolStripMenuItemClick(object sender, EventArgs e)
        {
            ReloadFromSourceView();
            FindNext();
        }

        private void FindNext()
        {
            if (_findHelper != null)
            {
                if (tabControlSubtitle.SelectedIndex == TabControlListView)
                {
                    int selectedIndex = -1;
                    if (SubtitleListview1.SelectedItems.Count > 0)
                        selectedIndex = SubtitleListview1.SelectedItems[0].Index;
                    if (_findHelper.FindNext(_subtitle, selectedIndex, _findHelper.SelectedPosition))
                    {
                        SubtitleListview1.SelectIndexAndEnsureVisible(_findHelper.SelectedIndex);
                        ShowStatus(string.Format(_language.XFoundAtLineNumberY, _findHelper.FindText, _findHelper.SelectedIndex+1));
                        textBoxListViewText.Focus();
                        textBoxListViewText.SelectionStart = _findHelper.SelectedPosition;
                        textBoxListViewText.SelectionLength = _findHelper.FindTextLength;
                        _findHelper.SelectedPosition++;
                    }
                    else
                    {
                        if (_findHelper.StartLineIndex >= 1)
                        {
                            if (MessageBox.Show(_language.FindContinue, _language.FindContinueTitle, MessageBoxButtons.YesNo) == DialogResult.Yes)
                            {
                                _findHelper.StartLineIndex = 0;
                                if (_findHelper.Find(_subtitle, 0))
                                {
                                    SubtitleListview1.SelectIndexAndEnsureVisible(_findHelper.SelectedIndex);
                                    textBoxListViewText.Focus();
                                    textBoxListViewText.SelectionStart = _findHelper.SelectedPosition;
                                    textBoxListViewText.SelectionLength = _findHelper.FindTextLength;
                                    ShowStatus(string.Format(_language.XFoundAtLineNumberY, _findHelper.FindText, _findHelper.SelectedIndex + 1));
                                    _findHelper.SelectedPosition++;
                                    return;
                                }
                            }
                        }
                        ShowStatus(string.Format(_language.XNotFound, _findHelper.FindText));                            
                    }
                }
                else if (tabControlSubtitle.SelectedIndex == TabControlSourceView)
                {
                    if (_findHelper.FindNext(textBoxSource, textBoxSource.SelectionStart))
                    {
                        textBoxSource.SelectionStart = _findHelper.SelectedIndex;
                        textBoxSource.SelectionLength = _findHelper.FindTextLength;
                        textBoxSource.ScrollToCaret();
                        ShowStatus(string.Format(_language.XFoundAtLineNumberY, _findHelper.FindText, textBoxSource.GetLineFromCharIndex(textBoxSource.SelectionStart)));
                    }
                    else
                    {
                        ShowStatus(string.Format(_language.XNotFound, _findHelper.FindText));
                    }
                }
            }

        }

        private void ToolStripButtonReplaceClick(object sender, EventArgs e)
        {
            ReloadFromSourceView();
            Replace(null);
        }

        private void ReplaceToolStripMenuItemClick(object sender, EventArgs e)
        {
            ReloadFromSourceView();
            Replace(null);
        }

        private void ReplaceSourceView(ReplaceDialog replaceDialog)
        {
            bool isFirst = true;
            string selectedText = textBoxSource.SelectedText;
            if (selectedText.Length == 0 && _findHelper != null)
                selectedText = _findHelper.FindText;

            if (replaceDialog == null)
            {
                replaceDialog = new ReplaceDialog();
                if (_findHelper == null)
                {
                    _findHelper = replaceDialog.GetFindDialogHelper(_subtitleListViewIndex);
                    _findHelper.WindowPositionLeft = Left + (Width / 2) - (replaceDialog.Width / 2);
                    _findHelper.WindowPositionTop = Top + (Height / 2) - (replaceDialog.Height / 2);
                }
            }
            else
                isFirst = false;

            replaceDialog.Initialize(selectedText, _findHelper);
            if (replaceDialog.ShowDialog(this) == DialogResult.OK)
            {
                _findHelper = replaceDialog.GetFindDialogHelper(_subtitleListViewIndex);
                int replaceCount = 0;
                bool searchStringFound = true;
                while (searchStringFound)
                {
                    searchStringFound = false;
                    int start = textBoxSource.SelectionStart;
                    if (isFirst)
                    {
                        MakeHistoryForUndo(string.Format(_language.BeforeReplace, _findHelper.FindText));
                        isFirst = false;
                        if (start >= 0)
                            start--;
                    }
                    if (_findHelper.FindNext(textBoxSource, start))
                    {
                        textBoxSource.SelectionStart = _findHelper.SelectedIndex;
                        textBoxSource.SelectionLength = _findHelper.FindTextLength;
                        if (!replaceDialog.FindOnly)
                            textBoxSource.SelectedText = _findHelper.ReplaceText;
                        textBoxSource.ScrollToCaret();

                        replaceCount++;
                        searchStringFound = true;
                    }
                    if (replaceDialog.FindOnly)
                    {
                        if (searchStringFound)
                            ShowStatus(string.Format(_language.MatchFoundX, _findHelper.FindText));
                        else
                            ShowStatus(string.Format(_language.NoMatchFoundX, _findHelper.FindText));

                        Replace(replaceDialog);
                        return;
                    }
                    if (!replaceDialog.ReplaceAll)
                    {
                        break; // out of while loop
                    }
                }
                ReloadFromSourceView();
                if (replaceCount == 0)
                    ShowStatus(_language.FoundNothingToReplace);
                else
                    ShowStatus(string.Format(_language.ReplaceCountX, replaceCount));
            }
            replaceDialog.Dispose();
        }

        private void ReplaceListView(ReplaceDialog replaceDialog)
        {
            bool isFirst = true;
            string selectedText = textBoxListViewText.SelectedText;
            if (selectedText.Length == 0 && _findHelper != null)
                selectedText = _findHelper.FindText;

            if (replaceDialog == null)
            {
                replaceDialog = new ReplaceDialog();
                if (_findHelper == null)
                {
                    _findHelper = replaceDialog.GetFindDialogHelper(_subtitleListViewIndex);
                    _findHelper.WindowPositionLeft = Left + (Width / 2) - (replaceDialog.Width / 2);
                    _findHelper.WindowPositionTop = Top + (Height / 2) - (replaceDialog.Height / 2);
                }
                int index = 0;

                if (SubtitleListview1.SelectedItems.Count > 0)
                    index = SubtitleListview1.SelectedItems[0].Index;

                _findHelper.SelectedIndex = index;
                _findHelper.SelectedPosition = index;
                _replaceStartLineIndex = index;
            }
            else
            {
                isFirst = false;
                if (_findHelper != null)
                    selectedText = _findHelper.FindText;
            }
            replaceDialog.Initialize(selectedText, _findHelper);
            if (replaceDialog.ShowDialog(this) == DialogResult.OK)
            {
                if (_findHelper == null)
                {
                    _findHelper = replaceDialog.GetFindDialogHelper(_subtitleListViewIndex);
                }
                else
                {
                    int line = _findHelper.SelectedIndex;
                    int pos = _findHelper.SelectedPosition;
                    _findHelper = replaceDialog.GetFindDialogHelper(_subtitleListViewIndex);
                    _findHelper.SelectedIndex = line;
                    _findHelper.SelectedPosition = pos;
                }
                int replaceCount = 0;
                bool searchStringFound = true;
                while (searchStringFound)
                {
                    searchStringFound = false;
                    if (isFirst)
                    {
                        MakeHistoryForUndo(string.Format(_language.BeforeReplace, _findHelper.FindText));
                        isFirst = false;
                    }

                    if (replaceDialog.ReplaceAll)
                    {
                        if (_findHelper.FindNext(_subtitle, _findHelper.SelectedIndex, _findHelper.SelectedPosition))
                        {
                            SubtitleListview1.SelectIndexAndEnsureVisible(_findHelper.SelectedIndex);
                            textBoxListViewText.SelectionStart = _findHelper.SelectedPosition;
                            textBoxListViewText.SelectionLength = _findHelper.FindTextLength;
                            textBoxListViewText.SelectedText = _findHelper.ReplaceText;
                            _findHelper.SelectedPosition += _findHelper.ReplaceText.Length;
                            searchStringFound = true;
                            replaceCount++;
                        }
                        else
                        {
                            ShowStatus(string.Format(_language.NoMatchFoundX, _findHelper.FindText));

                            if (_replaceStartLineIndex >= 1) // Prompt for start over
                            {
                                _replaceStartLineIndex = 0;
                                if (MessageBox.Show(_language.FindContinue, _language.FindContinueTitle, MessageBoxButtons.YesNo) == DialogResult.Yes)
                                {
                                    SubtitleListview1.SelectIndexAndEnsureVisible(0);
                                    _findHelper.StartLineIndex = 0;
                                    _findHelper.SelectedIndex = 0;
                                    _findHelper.SelectedPosition = 0;

                                    if (_findHelper.FindNext(_subtitle, _findHelper.SelectedIndex, _findHelper.SelectedPosition))
                                    {
                                        SubtitleListview1.SelectIndexAndEnsureVisible(_findHelper.SelectedIndex);
                                        textBoxListViewText.SelectionStart = _findHelper.SelectedPosition;
                                        textBoxListViewText.SelectionLength = _findHelper.FindTextLength;
                                        textBoxListViewText.SelectedText = _findHelper.ReplaceText;
                                        _findHelper.SelectedPosition += _findHelper.ReplaceText.Length;
                                        searchStringFound = true;
                                        replaceCount++;
                                    }
                                }
                            }
                        }
                    }
                    else if (replaceDialog.FindOnly)
                    {
                        if (_findHelper.FindNext(_subtitle, _findHelper.SelectedIndex, _findHelper.SelectedPosition))
                        {
                            SubtitleListview1.SelectIndexAndEnsureVisible(_findHelper.SelectedIndex);
                            textBoxListViewText.Focus();
                            textBoxListViewText.SelectionStart = _findHelper.SelectedPosition;
                            textBoxListViewText.SelectionLength = _findHelper.FindTextLength;
                            _findHelper.SelectedPosition += _findHelper.FindTextLength;
                            ShowStatus(string.Format(_language.NoXFoundAtLineY, _findHelper.SelectedIndex + 1, _findHelper.FindText));
                            Replace(replaceDialog);
                            return;
                        }
                        else if (_replaceStartLineIndex >= 1) // Prompt for start over
                        {
                            _replaceStartLineIndex = 0;
                            if (MessageBox.Show(_language.FindContinue, _language.FindContinueTitle, MessageBoxButtons.YesNo) == DialogResult.Yes)
                            {
                                SubtitleListview1.SelectIndexAndEnsureVisible(0);
                                _findHelper.StartLineIndex = 0;
                                _findHelper.SelectedIndex = 0;
                                _findHelper.SelectedPosition = 0;
                                if (_findHelper.FindNext(_subtitle, _findHelper.SelectedIndex, _findHelper.SelectedPosition))
                                {
                                    SubtitleListview1.SelectIndexAndEnsureVisible(_findHelper.SelectedIndex);
                                    textBoxListViewText.Focus();
                                    textBoxListViewText.SelectionStart = _findHelper.SelectedPosition;
                                    textBoxListViewText.SelectionLength = _findHelper.FindTextLength;
                                    _findHelper.SelectedPosition += _findHelper.FindTextLength;
                                    ShowStatus(string.Format(_language.NoXFoundAtLineY, _findHelper.SelectedIndex + 1, _findHelper.FindText));
                                    Replace(replaceDialog);
                                    return;
                                }
                            }
                            else
                            {
                                return;
                            }
                        }
                        ShowStatus(string.Format(_language.NoMatchFoundX, _findHelper.FindText));
                    }
                    else if (!replaceDialog.FindOnly) // replace once only
                    {
                        string msg = string.Empty;
                        if (textBoxListViewText.SelectionLength == _findHelper.FindTextLength)
                        {
                            textBoxListViewText.SelectedText = _findHelper.ReplaceText;
                            msg = _language.OneReplacementMade + " ";
                        }

                        if (_findHelper.FindNext(_subtitle, _findHelper.SelectedIndex, _findHelper.SelectedPosition))
                        {
                            SubtitleListview1.SelectIndexAndEnsureVisible(_findHelper.SelectedIndex);
                            textBoxListViewText.Focus();
                            textBoxListViewText.SelectionStart = _findHelper.SelectedPosition;
                            textBoxListViewText.SelectionLength = _findHelper.FindTextLength;
                            _findHelper.SelectedPosition += _findHelper.ReplaceText.Length;
                            ShowStatus(string.Format(msg + _language.XFoundAtLineNumberY + _language.XFoundAtLineNumberY, _findHelper.SelectedIndex + 1, _findHelper.FindText));
                        }
                        else
                        {
                            ShowStatus(msg + string.Format(_language.XNotFound, _findHelper.FindText));

                            // Prompt for start over
                            if (_replaceStartLineIndex >= 1)
                            {
                                _replaceStartLineIndex = 0;
                                if (MessageBox.Show(_language.FindContinue, _language.FindContinueTitle, MessageBoxButtons.YesNo) == DialogResult.Yes)
                                {
                                    SubtitleListview1.SelectIndexAndEnsureVisible(0);
                                    _findHelper.StartLineIndex = 0;
                                    _findHelper.SelectedIndex = 0;
                                    _findHelper.SelectedPosition = 0;


                                    if (_findHelper.FindNext(_subtitle, _findHelper.SelectedIndex, _findHelper.SelectedPosition))
                                    {
                                        SubtitleListview1.SelectIndexAndEnsureVisible(_findHelper.SelectedIndex);
                                        textBoxListViewText.Focus();
                                        textBoxListViewText.SelectionStart = _findHelper.SelectedPosition;
                                        textBoxListViewText.SelectionLength = _findHelper.FindTextLength;
                                        _findHelper.SelectedPosition += _findHelper.ReplaceText.Length;
                                        ShowStatus(string.Format(msg + _language.XFoundAtLineNumberY + _language.XFoundAtLineNumberY, _findHelper.SelectedIndex + 1, _findHelper.FindText));
                                    }

                                }
                                else
                                {
                                    return;
                                }
                            }

                        }
                        Replace(replaceDialog);
                        return;
                    }
                }

                ShowSource();
                if (replaceCount == 0)
                    ShowStatus(_language.FoundNothingToReplace);
                else
                    ShowStatus(string.Format(_language.ReplaceCountX, replaceCount));
            }
            replaceDialog.Dispose();
        }

        private void Replace(ReplaceDialog replaceDialog)
        {
            if (tabControlSubtitle.SelectedIndex == TabControlSourceView)
            {
                ReplaceSourceView(replaceDialog);
            }
            else
            {
                ReplaceListView(replaceDialog);
            }
        }

        public void ShowStatus(string message)
        {
            labelStatus.Text = message;
            statusStrip1.Refresh();
        }

        private void ReloadFromSourceView()
        {
            if (_sourceViewChange)
            {
                MakeHistoryForUndo(_language.BeforeChangesMadeInSourceView);
                SaveSubtitleListviewIndexes();
                if (textBoxSource.Text.Trim().Length > 0)
                {
                    SubtitleFormat format = _subtitle.ReloadLoadSubtitle(new List<string>(textBoxSource.Lines), null);
                    _sourceViewChange = false;
                    if (format == null)
                    {
                        MessageBox.Show(_language.UnableToParseSourceView);
                    }
                    else
                    {
                        int index = 0;
                        foreach (object obj in comboBoxSubtitleFormats.Items)
                        {
                            if (obj.ToString() == format.FriendlyName)
                                comboBoxSubtitleFormats.SelectedIndex = index;
                            index++;
                        }
                    }
                }
                else
                {
                    _sourceViewChange = false;
                    _subtitle.Paragraphs.Clear();
                }
                _subtitleListViewIndex = -1;
                SubtitleListview1.Fill(_subtitle, _subtitleAlternate);
                RestoreSubtitleListviewIndexes();
            }
        }

        private void HelpToolStripMenuItem1Click(object sender, EventArgs e)
        {
            ReloadFromSourceView();
            Utilities.ShowHelp(string.Empty);
        }

        private void ToolStripButtonHelpClick(object sender, EventArgs e)
        {
            ReloadFromSourceView();
            Utilities.ShowHelp(string.Empty);
        }

        private void GotoLineNumberToolStripMenuItemClick(object sender, EventArgs e)
        {
            ReloadFromSourceView();
            if (_subtitle.Paragraphs.Count < 1 || textBoxSource.Lines.Length < 1)
            {
                MessageBox.Show(_language.NoSubtitleLoaded, Title, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var goToLine = new GoToLine();
            if (tabControlSubtitle.SelectedIndex == TabControlListView)
            {
                goToLine.Initialize(1, SubtitleListview1.Items.Count);
            }
            else if (tabControlSubtitle.SelectedIndex == TabControlSourceView)
            {
                goToLine.Initialize(1, textBoxSource.Lines.Length);
            }
            if (goToLine.ShowDialog(this) == DialogResult.OK)
            {
                if (tabControlSubtitle.SelectedIndex == TabControlListView)
                {
                    SubtitleListview1.SelectNone();

                    SubtitleListview1.Items[goToLine.LineNumber - 1].Selected = true;
                    SubtitleListview1.Items[goToLine.LineNumber - 1].EnsureVisible();
                    ShowStatus(string.Format(_language.GoToLineNumberX, goToLine.LineNumber));
                }
                else if (tabControlSubtitle.SelectedIndex == TabControlSourceView)
                {
                    // binary search
                    int start = 0;
                    int end = textBoxSource.Text.Length;
                    while (end - start > 10)
                    {
                        int middle = (end - start) / 2;
                        if (goToLine.LineNumber - 1 >= textBoxSource.GetLineFromCharIndex(start + middle))
                            start += middle;
                        else
                            end = start + middle;
                    }

                    // go before line, so we can find first char on line
                    start -= 100;
                    if (start < 0)
                        start = 0;

                    for (int i = start; i <= end; i++)
                    {
                        if (textBoxSource.GetLineFromCharIndex(i) == goToLine.LineNumber - 1)
                        {
                            // select line, scroll to line, and focus...
                            textBoxSource.SelectionStart = i;
                            textBoxSource.SelectionLength = textBoxSource.Lines[goToLine.LineNumber - 1].Length;
                            textBoxSource.ScrollToCaret();
                            ShowStatus(string.Format(_language.GoToLineNumberX, goToLine.LineNumber));
                            if (textBoxSource.CanFocus)
                                textBoxSource.Focus();
                            break;
                        }
                    }

                    ShowSourceLineNumber();
                }
            }
            goToLine.Dispose();
        }

        private void TextBoxSourceLeave(object sender, EventArgs e)
        {
            ReloadFromSourceView();
        }

        private void AdjustDisplayTimeToolStripMenuItemClick(object sender, EventArgs e)
        {
            AdjustDisplayTime(false);
        }

        private void AdjustDisplayTime(bool onlySelectedLines)
        {
            if (_subtitle != null && _subtitle.Paragraphs.Count > 1)
            {
                ReloadFromSourceView();
                var adjustDisplayTime = new AdjustDisplayDuration();
                _formPositionsAndSizes.SetPositionAndSize(adjustDisplayTime);

                ListView.SelectedIndexCollection selectedIndexes = null;
                if (onlySelectedLines)
                {
                    adjustDisplayTime.Text += " - " + _language.SelectedLines;
                    selectedIndexes = SubtitleListview1.SelectedIndices;
                }

                if (adjustDisplayTime.ShowDialog(this) == DialogResult.OK)
                {
                    MakeHistoryForUndo(_language.BeforeDisplayTimeAdjustment);
                    if (adjustDisplayTime.AdjustUsingPercent)
                    {
                        double percent = double.Parse(adjustDisplayTime.AdjustValue);
                        _subtitle.AdjustDisplayTimeUsingPercent(percent, selectedIndexes);
                    }
                    else
                    {
                        double seconds = double.Parse(adjustDisplayTime.AdjustValue);
                        _subtitle.AdjustDisplayTimeUsingSeconds(seconds, selectedIndexes);
                    }
                    ShowStatus(string.Format(_language.DisplayTimeAdjustedX, adjustDisplayTime.AdjustValue));
                    SaveSubtitleListviewIndexes();
                    if (IsFramesRelevant)
                        _subtitle.CalculateFrameNumbersFromTimeCodesNoCheck(CurrentFrameRate);
                    ShowSource();
                    SubtitleListview1.Fill(_subtitle, _subtitleAlternate);
                    RestoreSubtitleListviewIndexes();
                    _change = true;
                }
                _formPositionsAndSizes.SavePositionAndSize(adjustDisplayTime);
            }
            else
            {
                MessageBox.Show(_language.NoSubtitleLoaded, Title, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private bool IsFramesRelevant
        {
            get
            {
                return _subtitle.WasLoadedWithFrameNumbers || GetCurrentSubtitleFormat().IsFrameBased;
            }
        }

        private void FixToolStripMenuItemClick(object sender, EventArgs e)
        {
            FixCommonErrors(false);
        }

        private void FixCommonErrors(bool onlySelectedLines)
        {
            if (_subtitle != null && _subtitle.Paragraphs.Count > 1)
            {
                ReloadFromSourceView();
                SaveSubtitleListviewIndexes();
                var fixErrors = new FixCommonErrors();
                _formPositionsAndSizes.SetPositionAndSize(fixErrors);

                if (onlySelectedLines)
                {
                    var selectedLines = new Subtitle { WasLoadedWithFrameNumbers = _subtitle.WasLoadedWithFrameNumbers };
                    foreach (int index in SubtitleListview1.SelectedIndices)
                        selectedLines.Paragraphs.Add(_subtitle.Paragraphs[index]);
                    fixErrors.Initialize(selectedLines);
                }
                else
                {
                    fixErrors.Initialize(_subtitle);
                }

                if (fixErrors.ShowDialog(this) == DialogResult.OK)
                {
                    MakeHistoryForUndo(_language.BeforeCommonErrorFixes);

                    if (onlySelectedLines)
                    { // we only update selected lines
                        int i = 0;
                        foreach (int index in SubtitleListview1.SelectedIndices)
                        {
                            _subtitle.Paragraphs[index] = fixErrors.FixedSubtitle.Paragraphs[i];
                            i++;
                        }
                        ShowStatus(_language.CommonErrorsFixedInSelectedLines);
                    }
                    else
                    {
                        _subtitle.Paragraphs.Clear();
                        foreach (Paragraph p in fixErrors.FixedSubtitle.Paragraphs)
                            _subtitle.Paragraphs.Add(p);
                        ShowStatus(_language.CommonErrorsFixed);
                    }
                    ShowSource();
                    SubtitleListview1.Fill(_subtitle, _subtitleAlternate);
                    RestoreSubtitleListviewIndexes();
                    _change = true;
                    _formPositionsAndSizes.SavePositionAndSize(fixErrors);
                }
            }
            else
            {
                MessageBox.Show(_language.NoSubtitleLoaded, Title, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void StartNumberingFromToolStripMenuItemClick(object sender, EventArgs e)
        {
            if (_subtitle != null && _subtitle.Paragraphs.Count > 1)
            {
                ReloadFromSourceView();
                var startNumberingFrom = new StartNumberingFrom();
                _formPositionsAndSizes.SetPositionAndSize(startNumberingFrom);
                if (startNumberingFrom.ShowDialog(this) == DialogResult.OK)
                {
                    SaveSubtitleListviewIndexes();
                    MakeHistoryForUndo(_language.BeforeRenumbering);
                    ShowStatus(string.Format(_language.RenumberedStartingFromX, startNumberingFrom.StartFromNumber));
                    _subtitle.Renumber(startNumberingFrom.StartFromNumber);
                    ShowSource();
                    SubtitleListview1.Fill(_subtitle, _subtitleAlternate);
                    RestoreSubtitleListviewIndexes();
                    _change = true;
                }
                _formPositionsAndSizes.SavePositionAndSize(startNumberingFrom);
            }
            else
            {
                MessageBox.Show(_language.NoSubtitleLoaded, Title, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void Renumber()
        { 
            if (_subtitle != null && _subtitle.Paragraphs != null && _subtitle.Paragraphs.Count > 0)
                _subtitle.Renumber(_subtitle.Paragraphs[0].Number);
        }

        private void RemoveTextForHearImparedToolStripMenuItemClick(object sender, EventArgs e)
        {
            if (_subtitle != null && _subtitle.Paragraphs.Count > 1)
            {
                ReloadFromSourceView();
                var removeTextFromHearImpaired = new FormRemoveTextForHearImpaired();
                _formPositionsAndSizes.SetPositionAndSize(removeTextFromHearImpaired);
                removeTextFromHearImpaired.Initialize(_subtitle);
                if (removeTextFromHearImpaired.ShowDialog(this) == DialogResult.OK)
                {
                    MakeHistoryForUndo(_language.BeforeRemovalOfTextingForHearingImpaired);
                    int count = removeTextFromHearImpaired.RemoveTextFromHearImpaired();
                    if (count > 0)
                    {
                        if (count == 1)
                            ShowStatus(_language.TextingForHearingImpairedRemovedOneLine);
                        else
                            ShowStatus(string.Format(_language.TextingForHearingImpairedRemovedXLines, count));
                        _subtitleListViewIndex = -1;
                        Renumber();
                        ShowSource();
                        SubtitleListview1.Fill(_subtitle, _subtitleAlternate);
                        _change = true;
                        if (_subtitle.Paragraphs.Count > 0)
                            SubtitleListview1.SelectIndexAndEnsureVisible(0);
                    }
                }
                _formPositionsAndSizes.SavePositionAndSize(removeTextFromHearImpaired);
            }
            else
            {
                MessageBox.Show(_language.NoSubtitleLoaded, Title, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void SplitToolStripMenuItemClick(object sender, EventArgs e)
        {
            if (_subtitle != null && _subtitle.Paragraphs.Count > 1)
            {
                ReloadFromSourceView();
                var splitSubtitle = new SplitSubtitle();
                double lengthInSeconds = 0;
                if (mediaPlayer.VideoPlayer != null)
                    lengthInSeconds = mediaPlayer.Duration;
                splitSubtitle.Initialize(_subtitle, _fileName , GetCurrentSubtitleFormat(), GetCurrentEncoding(), lengthInSeconds);
                if (splitSubtitle.ShowDialog(this) == DialogResult.OK)
                {
                    ShowStatus(_language.SubtitleSplitted);
                }
            }
            else
            {
                MessageBox.Show(_language.NoSubtitleLoaded, Title, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void AppendTextVisuallyToolStripMenuItemClick(object sender, EventArgs e)
        {
            if (_subtitle != null && _subtitle.Paragraphs.Count > 1)
            {
                ReloadFromSourceView();

                if (MessageBox.Show(_language.SubtitleAppendPrompt, _language.SubtitleAppendPromptTitle, MessageBoxButtons.YesNoCancel) == DialogResult.Yes)
                {
                    openFileDialog1.Title = _language.OpenSubtitleToAppend;
                    openFileDialog1.FileName = string.Empty;
                    openFileDialog1.Filter = Utilities.GetOpenDialogFiler();
                    if (openFileDialog1.ShowDialog(this) == DialogResult.OK)
                    {
                        bool success = false;
                        string fileName = openFileDialog1.FileName;
                        if (File.Exists(fileName))
                        {
                            var subtitleToAppend = new Subtitle();
                            Encoding encoding;
                            SubtitleFormat format = subtitleToAppend.LoadSubtitle(fileName, out encoding, null);
                            if (GetCurrentSubtitleFormat().IsFrameBased)
                                subtitleToAppend.CalculateTimeCodesFromFrameNumbers(CurrentFrameRate);
                            else
                                subtitleToAppend.CalculateFrameNumbersFromTimeCodes(CurrentFrameRate);

                            if (format != null)
                            {
                                if (subtitleToAppend != null && subtitleToAppend.Paragraphs.Count > 1)
                                {
                                    VisualSync visualSync = new VisualSync();

                                    visualSync.Initialize(this.Icon, subtitleToAppend, _fileName, _language.AppendViaVisualSyncTitle, CurrentFrameRate);

                                    visualSync.ShowDialog(this);
                                    if (visualSync.OKPressed)
                                    {
                                        if (MessageBox.Show(_language.AppendSynchronizedSubtitlePrompt, _language.SubtitleAppendPromptTitle, MessageBoxButtons.YesNo) == DialogResult.Yes)
                                        {
                                            int start = _subtitle.Paragraphs.Count +1;

                                            MakeHistoryForUndo(_language.BeforeAppend);
                                            foreach (Paragraph p in visualSync.Paragraphs)
                                                _subtitle.Paragraphs.Add(new Paragraph(p));
                                            _subtitle.Renumber(1);
                                            ShowSource();
                                            SubtitleListview1.Fill(_subtitle, _subtitleAlternate);

                                            // select appended lines
                                            for (int i = start; i < _subtitle.Paragraphs.Count; i++)
                                                SubtitleListview1.Items[i].Selected = true;
                                            SubtitleListview1.EnsureVisible(start);

                                            ShowStatus(string.Format(_language.SubtitleAppendedX, fileName));
                                            success = true;
                                        }
                                    }
                                    visualSync.Dispose();
                                }
                            }
                        }
                        if (!success)
                            ShowStatus(_language.SubtitleNotAppended);
                    }
                }
            }
            else
            {
                MessageBox.Show(_language.NoSubtitleLoaded, Title, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void TranslateByGoogleToolStripMenuItemClick(object sender, EventArgs e)
        {
            TranslateViaGoogle(false, true);
        }

        private void TranslateViaGoogle(bool onlySelectedLines, bool useGoogle)
        {
            if (_subtitle != null && _subtitle.Paragraphs.Count >= 1)
            {
                ReloadFromSourceView();
                var googleTranslate = new GoogleTranslate();
                _formPositionsAndSizes.SetPositionAndSize(googleTranslate);
                SaveSubtitleListviewIndexes();
                string title = _language.GoogleTranslate;
                if (!useGoogle)
                    title = _language.MicrosoftTranslate;
                if (onlySelectedLines)
                {
                    var selectedLines = new Subtitle { WasLoadedWithFrameNumbers = _subtitle.WasLoadedWithFrameNumbers };
                    foreach (int index in SubtitleListview1.SelectedIndices)
                        selectedLines.Paragraphs.Add(_subtitle.Paragraphs[index]);
                    title += " - " + _language.SelectedLines;
                    googleTranslate.Initialize(selectedLines, title, useGoogle);
                }
                else
                {
                    googleTranslate.Initialize(_subtitle, title, useGoogle);
                }
                if (googleTranslate.ShowDialog(this) == DialogResult.OK)
                {
                    _subtitleListViewIndex = -1;

                    _subtitleAlternate = new Subtitle(_subtitle);
                    MakeHistoryForUndo(_language.BeforeGoogleTranslation);

                    if (onlySelectedLines)
                    { // we only update selected lines
                        int i = 0;
                        foreach (int index in SubtitleListview1.SelectedIndices)
                        {
                            _subtitle.Paragraphs[index] = googleTranslate.TranslatedSubtitle.Paragraphs[i];
                            i++;
                        }
                        ShowStatus(_language.SelectedLinesTranslated);
                    }
                    else
                    {

                        _subtitle.Paragraphs.Clear();
                        foreach (Paragraph p in googleTranslate.TranslatedSubtitle.Paragraphs)
                            _subtitle.Paragraphs.Add(new Paragraph(p));
                        ShowStatus(_language.SubtitleTranslated);
                    }
                    ShowSource();

                    SubtitleListview1.AddAlternateTextColumn(Configuration.Settings.Language.General.OriginalText);
                    SubtitleListview1.Fill(_subtitle, _subtitleAlternate);

                    RestoreSubtitleListviewIndexes();
                    _change = true;
                    _converted = true;
                }
                _formPositionsAndSizes.SavePositionAndSize(googleTranslate);
            }
            else
            {
                MessageBox.Show(_language.NoSubtitleLoaded, Title, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }


        private static string GetTranslateStringFromNikseDk(string input)
        {
//          string url = String.Format("http://localhost:2782/mt/Translate.aspx?text={0}&langpair={1}", HttpUtility.UrlEncode(input), "svda");
            string url = String.Format("http://www.nikse.dk/mt/Translate.aspx?text={0}&langpair={1}", HttpUtility.UrlEncode(input), "svda");
            var webClient = new WebClient {Proxy = Utilities.GetProxy(), Encoding = System.Text.Encoding.UTF8};
            return webClient.DownloadString(url);
        }

        private void TranslateFromSwedishToDanishToolStripMenuItemClick(object sender, EventArgs e)
        {
            if (_subtitle != null && _subtitle.Paragraphs.Count > 1)
            {
                bool isSwedish = Utilities.AutoDetectGoogleLanguage(_subtitle) == "sv";
                string promptText = _language.TranslateSwedishToDanish;
                if (!isSwedish)
                    promptText = _language.TranslateSwedishToDanishWarning;

                if (MessageBox.Show(promptText, Title, MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    try
                    {
                        _subtitleAlternate = new Subtitle(_subtitle);

                        int firstSelectedIndex = 0;
                        if (SubtitleListview1.SelectedItems.Count > 0)
                            firstSelectedIndex = SubtitleListview1.SelectedItems[0].Index;
                        _subtitleListViewIndex = -1;

                        Cursor.Current = Cursors.WaitCursor;
                        ShowStatus(_language.TranslatingViaNikseDkMt);
                        var sb = new StringBuilder();
                        var output = new StringBuilder();
                        foreach (Paragraph p in _subtitle.Paragraphs)
                        {
                            string s = p.Text;
                            s = s.Replace(Environment.NewLine, "<br/>");
                            s = "<p>" + s + "</p>";
                            sb.Append(s);

                            if (sb.Length > 9000)
                            {
                                output.Append(GetTranslateStringFromNikseDk(sb.ToString()));
                                sb = new StringBuilder();
                            }
                        }
                        if (sb.Length > 0)
                            output.Append(GetTranslateStringFromNikseDk(sb.ToString()));

                        MakeHistoryForUndo(_language.BeforeSwedishToDanishTranslation);
                        string result = output.ToString();
                        const string key = "<div id=\"translatedText\">";
                        int startIndex = result.IndexOf(key);
                        if (startIndex > 0)
                        {
                            int index = 0;
                            while (startIndex > 0)
                            {
                                startIndex += key.Length;
                                int endIndex = result.IndexOf("</div>", startIndex);
                                string translatedText = result.Substring(startIndex, endIndex - startIndex);

                                foreach (string s in translatedText.Split(new string[] { "<p>", "</p>" }, StringSplitOptions.RemoveEmptyEntries))
                                {
                                    if (index < _subtitle.Paragraphs.Count)
                                        _subtitle.Paragraphs[index].Text = s;
                                    index++;
                                }
                                startIndex = result.IndexOf(key, startIndex + key.Length);
                            }
                            ShowSource();
                            SubtitleListview1.AddAlternateTextColumn(Configuration.Settings.Language.General.OriginalText);
                            SubtitleListview1.Fill(_subtitle, _subtitleAlternate);
                            ShowStatus(_language.TranslationFromSwedishToDanishComplete);
                            SubtitleListview1.SelectIndexAndEnsureVisible(firstSelectedIndex);
                            _change = true;
                            _converted = true;
                        }                        
                    }
                    catch
                    {
                        ShowStatus(_language.TranslationFromSwedishToDanishFailed);
                    }
                    Cursor.Current = Cursors.Default;
                }
            }
            else
            {
                MessageBox.Show(_language.NoSubtitleLoaded, Title, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void ShowHistoryforUndoToolStripMenuItemClick(object sender, EventArgs e)
        {
            if (_subtitle != null && _subtitle.CanUndo)
            {
                ReloadFromSourceView();
                var showHistory = new ShowHistory();
                showHistory.Initialize(_subtitle);
                if (showHistory.ShowDialog(this) == DialogResult.OK)
                {
                    _subtitleListViewIndex = -1;
                    textBoxListViewText.Text = string.Empty;
                    MakeHistoryForUndo(_language.BeforeUndo);
                    string subtitleFormatFriendlyName;
                    _fileName = _subtitle.UndoHistory(showHistory.SelectedIndex, out subtitleFormatFriendlyName, out _fileDateTime);
                    Text = Title + " - " + _fileName;
                    ShowStatus(_language.UndoPerformed);

                    comboBoxSubtitleFormats.SelectedIndexChanged -= ComboBoxSubtitleFormatsSelectedIndexChanged;
                    SetCurrentFormat(subtitleFormatFriendlyName);
                    comboBoxSubtitleFormats.SelectedIndexChanged += ComboBoxSubtitleFormatsSelectedIndexChanged;

                    ShowSource();
                    SubtitleListview1.Fill(_subtitle, _subtitleAlternate);
                    _change = true;

                    if (SubtitleListview1.Items.Count > 0)
                        SubtitleListview1.Items[0].Selected = true;
                }
            }
            else
            {
                MessageBox.Show(_language.NothingToUndo, Title, MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void ToolStripButtonSpellCheckClick(object sender, EventArgs e)
        {
            SpellCheck(true);
        }

        private void SpellCheckToolStripMenuItemClick(object sender, EventArgs e)
        {
            SpellCheck(true);
        }

        private void SpellCheck(bool autoDetect)
        {
            try
            {
                string dictionaryFolder = Utilities.DictionaryFolder;
                if (!Directory.Exists(dictionaryFolder) || Directory.GetFiles(dictionaryFolder, "*.dic").Length == 0)
                {
                    ShowGetDictionaries();
                    return;
                }

                if (_subtitle != null && _subtitle.Paragraphs.Count > 0)
                {
                    if (_spellCheckForm != null)
                    {
                        DialogResult result = MessageBox.Show(_language.ContinueWithCurrentSpellCheck, Title, MessageBoxButtons.YesNoCancel);
                        if (result == System.Windows.Forms.DialogResult.Cancel)
                            return;

                        if (result == System.Windows.Forms.DialogResult.No)
                        {
                            _spellCheckForm = new SpellCheck();
                            _spellCheckForm.DoSpellCheck(autoDetect, _subtitle, dictionaryFolder, this);
                        }
                        else
                        {
                            _spellCheckForm.ContinueSpellcheck(_subtitle);
                        }
                    }
                    else
                    {
                        _spellCheckForm = new SpellCheck();
                        _spellCheckForm.DoSpellCheck(autoDetect, _subtitle, dictionaryFolder, this);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("{0}{1}{2}{3}{4}", ex.Source, Environment.NewLine, ex.Message, Environment.NewLine, ex.StackTrace), _title, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public void ChangeWholeTextMainPart(ref int noOfChangedWords, ref bool firstChange, int i, Paragraph p)
        {
            SubtitleListview1.SetText(i, p.Text);
            _change = true;
            noOfChangedWords++;
            if (firstChange)
            {
                MakeHistoryForUndo(_language.BeforeSpellCheck);
                firstChange = false;
            }
            if (tabControlSubtitle.SelectedIndex == TabControlSourceView)
                ShowSource();
            else
                RefreshSelectedParagraph();
        }     

        public void FocusParagraph(int index)
        {
            if (tabControlSubtitle.SelectedIndex == TabControlSourceView)
            {
                tabControlSubtitle.SelectedIndex = TabControlListView;
            }

            if (tabControlSubtitle.SelectedIndex == TabControlListView)
            {
               SubtitleListview1.SelectIndexAndEnsureVisible(index);
            }
        }

        private void RefreshSelectedParagraph()
        {
            _subtitleListViewIndex = -1;
            SubtitleListview1_SelectedIndexChanged(null, null);
        }

        public void CorrectWord(string changeWord, Paragraph p, string oldWord, ref bool firstChange)
        {
            if (oldWord != changeWord)
            {
                if (firstChange)
                {
                    MakeHistoryForUndo(_language.BeforeSpellCheck);
                    firstChange = false;
                }
                var regEx = new Regex("\\b" + oldWord + "\\b");
                if (regEx.IsMatch(p.Text))
                {
                    p.Text = regEx.Replace(p.Text, changeWord);
                }
                else
                {                    
                    int startIndex = p.Text.IndexOf(oldWord);
                    while (startIndex >= 0 && startIndex < p.Text.Length && p.Text.Substring(startIndex).Contains(oldWord))
                    {
                        bool startOk = (startIndex == 0) || (p.Text[startIndex - 1] == ' ') ||
                                       (Environment.NewLine.EndsWith(p.Text[startIndex - 1].ToString()));

                        if (startOk)
                        {
                            int end = startIndex + oldWord.Length;
                            if (end <= p.Text.Length)
                            {
                                if ((end == p.Text.Length) || ((" ,.!?:;')" + Environment.NewLine).Contains(p.Text[end].ToString())))
                                    p.Text = p.Text.Remove(startIndex, oldWord.Length).Insert(startIndex, changeWord);
                            }
                        }
                        startIndex = p.Text.IndexOf(oldWord, startIndex + 2);
                    }

                }
                ShowStatus(string.Format(_language.SpellCheckChangedXToY, oldWord, changeWord));
                SubtitleListview1.SetText(_subtitle.GetIndex(p), p.Text);
                _change = true;
                if (tabControlSubtitle.SelectedIndex == TabControlSourceView)
                {
                    ShowSource();
                }
                else
                {
                    RefreshSelectedParagraph();
                }
            }
        }

        private void GetDictionariesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ShowGetDictionaries();
        }

        private void ShowGetDictionaries()
        {
            new GetDictionaries().ShowDialog(this);
        }

        private void ContextMenuStripListviewOpening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (SubtitleListview1.SelectedItems.Count == 0)
            {
                e.Cancel = true;
            }
            else
            {
                toolStripMenuItemInsertBefore.Visible = true;
                toolStripMenuItemInsertAfter.Visible = true;
                toolStripMenuItemMergeLines.Visible = true;
                mergeAfterToolStripMenuItem.Visible = true;
                mergeBeforeToolStripMenuItem.Visible = true;
                splitLineToolStripMenuItem.Visible = true;
                typeEffectToolStripMenuItem.Visible = true;
                toolStripSeparator7.Visible = true;
                karokeeEffectToolStripMenuItem.Visible = true;
                toolStripSeparatorAdvancedFunctions.Visible = true;
                showSelectedLinesEarlierlaterToolStripMenuItem.Visible = true;
                visualSyncSelectedLinesToolStripMenuItem.Visible = true;
                googleTranslateSelectedLinesToolStripMenuItem.Visible = true;
                adjustDisplayTimeForSelectedLinesToolStripMenuItem.Visible = true;
                toolStripMenuItemUnbreakLines.Visible = true;
                toolStripMenuItemAutoBreakLines.Visible = true;
                toolStripSeparatorBreakLines.Visible = true;

                if (SubtitleListview1.SelectedItems.Count == 1)
                {
                    toolStripMenuItemMergeLines.Visible = false;
                    visualSyncSelectedLinesToolStripMenuItem.Visible = false;
                    toolStripMenuItemUnbreakLines.Visible = false;
                    toolStripMenuItemAutoBreakLines.Visible = false;
                    toolStripSeparatorBreakLines.Visible = false;
                }
                else if (SubtitleListview1.SelectedItems.Count == 2)
                {
                    toolStripMenuItemInsertBefore.Visible = false;
                    toolStripMenuItemInsertAfter.Visible = false;
                    mergeAfterToolStripMenuItem.Visible = false;
                    mergeBeforeToolStripMenuItem.Visible = false;
                    splitLineToolStripMenuItem.Visible = false;
                    typeEffectToolStripMenuItem.Visible = false;
                }
                else if (SubtitleListview1.SelectedItems.Count >= 2)
                {
                    toolStripMenuItemInsertBefore.Visible = false;
                    toolStripMenuItemInsertAfter.Visible = false;
                    splitLineToolStripMenuItem.Visible = false;
                    toolStripMenuItemMergeLines.Visible = false;
                    mergeAfterToolStripMenuItem.Visible = false;
                    mergeBeforeToolStripMenuItem.Visible = false;
                    toolStripMenuItemMergeLines.Visible = false;
                    typeEffectToolStripMenuItem.Visible = false;
                    toolStripSeparator7.Visible = false;
                }

                if (GetCurrentSubtitleFormat().GetType() != typeof(SubRip))
                {
                    karokeeEffectToolStripMenuItem.Visible = false;
                    toolStripSeparatorAdvancedFunctions.Visible = SubtitleListview1.SelectedItems.Count == 1;
                }
            }
        }

        private void BoldToolStripMenuItemClick(object sender, EventArgs e)
        {
            ListViewToogleTag("b");
        }

        private void ItalicToolStripMenuItemClick(object sender, EventArgs e)
        {
            ListViewToogleTag("i");
        }

        private void UnderlineToolStripMenuItemClick(object sender, EventArgs e)
        {
            ListViewToogleTag("u");
        }

        private void ListViewToogleTag(string tag)
        {
            if (_subtitle.Paragraphs.Count > 0 && SubtitleListview1.SelectedItems.Count > 0)
            {
                SubtitleListview1.SelectedIndexChanged -= SubtitleListview1_SelectedIndexChanged;
                MakeHistoryForUndo(string.Format(_language.BeforeAddingTagX, tag));

                var indexes = new List<int>();
                foreach (ListViewItem item in SubtitleListview1.SelectedItems)
                    indexes.Add(item.Index);

                SubtitleListview1.BeginUpdate();
                foreach (int i in indexes)
                {
                    if (_subtitle.Paragraphs[i].Text.Contains("<" + tag + ">"))
                    {
                        _subtitle.Paragraphs[i].Text = _subtitle.Paragraphs[i].Text.Replace("<" + tag + ">", string.Empty);
                        _subtitle.Paragraphs[i].Text = _subtitle.Paragraphs[i].Text.Replace("</" + tag + ">", string.Empty);
                    }
                    else
                    {
                        _subtitle.Paragraphs[i].Text = string.Format("<{0}>{1}</{0}>", tag, _subtitle.Paragraphs[i].Text);
                    }
                    SubtitleListview1.SetText(i, _subtitle.Paragraphs[i].Text);
                }
                SubtitleListview1.EndUpdate();

                ShowStatus(string.Format(_language.TagXAdded, tag));
                ShowSource();
                _change = true;
                RefreshSelectedParagraph();
                SubtitleListview1.SelectedIndexChanged += SubtitleListview1_SelectedIndexChanged;
            }
        }

        private void ToolStripMenuItemDeleteClick(object sender, EventArgs e)
        {
            if (_subtitle.Paragraphs.Count > 0 && SubtitleListview1.SelectedItems.Count > 0)
            {
                string statusText;
                string historyText;
                string askText;

                if (SubtitleListview1.SelectedItems.Count > 1)
                {
                    statusText = string.Format(_language.XLinesDeleted, SubtitleListview1.SelectedItems.Count);
                    historyText = string.Format(_language.BeforeDeletingXLines, SubtitleListview1.SelectedItems.Count);
                    askText = string.Format(_language.DeleteXLinesPrompt, SubtitleListview1.SelectedItems.Count);
                }
                else
                {
                    statusText = _language.OneLineDeleted;
                    historyText = _language.BeforeDeletingOneLine;
                    askText = _language.DeleteOneLinePrompt;
                }

                if (MessageBox.Show(askText, Title, MessageBoxButtons.YesNo) == DialogResult.No)
                    return;

                MakeHistoryForUndo(historyText);
                _subtitleListViewIndex = -1;

                var indexes = new List<int>();
                foreach (ListViewItem item in SubtitleListview1.SelectedItems)
                    indexes.Add(item.Index);
                int firstIndex = SubtitleListview1.SelectedItems[0].Index;

                int startNumber = _subtitle.Paragraphs[0].Number;
                indexes.Reverse();
                foreach (int i in indexes)
                {
                    _subtitle.Paragraphs.RemoveAt(i);
                }
                _subtitle.Renumber(startNumber);
                SubtitleListview1.Fill(_subtitle, _subtitleAlternate);
                if (SubtitleListview1.FirstVisibleIndex == 0)
                    SubtitleListview1.FirstVisibleIndex =  - 1;
                if (SubtitleListview1.Items.Count > firstIndex)
                {
                    SubtitleListview1.SelectIndexAndEnsureVisible(firstIndex);
                }
                else if (SubtitleListview1.Items.Count > 0)
                {
                    SubtitleListview1.SelectIndexAndEnsureVisible(SubtitleListview1.Items.Count-1);
                }                
                ShowStatus(statusText);
                ShowSource();
                _change = true;
            }
        }

        private void ToolStripMenuItemInsertBeforeClick(object sender, EventArgs e)
        {
            if (_subtitle.Paragraphs.Count > 0)
            {
                InsertBefore();
            }
        }

        private void InsertBefore()
        {
            MakeHistoryForUndo(_language.BeforeInsertLine);

            int startNumber = 1;
            if (_subtitle.Paragraphs.Count > 0)
                startNumber = _subtitle.Paragraphs[0].Number;
            int firstSelectedIndex = 0;
            if (SubtitleListview1.SelectedItems.Count > 0)
                firstSelectedIndex = SubtitleListview1.SelectedItems[0].Index;

            var newParagraph = new Paragraph();
            Paragraph prev = _subtitle.GetParagraphOrDefault(firstSelectedIndex - 1);
            Paragraph next = _subtitle.GetParagraphOrDefault(firstSelectedIndex);
            if (prev != null)
            {
                newParagraph.StartTime.TotalMilliseconds = prev.EndTime.TotalMilliseconds + 200;
                newParagraph.EndTime.TotalMilliseconds = newParagraph.StartTime.TotalMilliseconds + 1200;
                if (next != null && newParagraph.EndTime.TotalMilliseconds > next.StartTime.TotalMilliseconds)
                    newParagraph.EndTime.TotalMilliseconds = next.StartTime.TotalMilliseconds - 1;
                if (newParagraph.StartTime.TotalMilliseconds > newParagraph.EndTime.TotalMilliseconds)
                    newParagraph.StartTime.TotalMilliseconds = prev.EndTime.TotalMilliseconds + 1;
            }
            else if (next != null)
            {
                newParagraph.StartTime.TotalMilliseconds = next.StartTime.TotalMilliseconds - 1200;
                newParagraph.EndTime.TotalMilliseconds = next.StartTime.TotalMilliseconds - 1;
            }
            else
            {
                newParagraph.StartTime.TotalMilliseconds = 1000;
                newParagraph.EndTime.TotalMilliseconds = 3000;
            }

            _subtitle.Paragraphs.Insert(firstSelectedIndex, newParagraph);

            _subtitleListViewIndex = -1;
            _subtitle.Renumber(startNumber);
            SubtitleListview1.Fill(_subtitle, _subtitleAlternate);
            SubtitleListview1.SelectIndexAndEnsureVisible(firstSelectedIndex);
            ShowSource();
            ShowStatus(_language.LineInserted);
            _change = true;
        }

        private void ToolStripMenuItemInsertAfterClick(object sender, EventArgs e)
        {
            if (_subtitle.Paragraphs.Count > 0)
            {
                InsertAfter();
            }
        }

        private void InsertAfter()
        {
            MakeHistoryForUndo(_language.BeforeInsertLine);

            int startNumber = 1;
            if (_subtitle.Paragraphs.Count > 0)
                startNumber = _subtitle.Paragraphs[0].Number;
            
            int firstSelectedIndex = 0;
            if (SubtitleListview1.SelectedItems.Count > 0)
                firstSelectedIndex = SubtitleListview1.SelectedItems[0].Index + 1;

            var newParagraph = new Paragraph();
            Paragraph prev = _subtitle.GetParagraphOrDefault(firstSelectedIndex - 1);
            Paragraph next = _subtitle.GetParagraphOrDefault(firstSelectedIndex);
            if (prev != null)
            {
                newParagraph.StartTime.TotalMilliseconds = prev.EndTime.TotalMilliseconds + 200;
                newParagraph.EndTime.TotalMilliseconds = newParagraph.StartTime.TotalMilliseconds + 1200;
                if (next != null && newParagraph.EndTime.TotalMilliseconds > next.StartTime.TotalMilliseconds)
                    newParagraph.EndTime.TotalMilliseconds = next.StartTime.TotalMilliseconds - 1;
                if (newParagraph.StartTime.TotalMilliseconds > newParagraph.EndTime.TotalMilliseconds)
                    newParagraph.StartTime.TotalMilliseconds = prev.EndTime.TotalMilliseconds + 1;
            }
            else if (next != null)
            {
                newParagraph.StartTime.TotalMilliseconds = next.StartTime.TotalMilliseconds - 1200;
                newParagraph.EndTime.TotalMilliseconds = next.StartTime.TotalMilliseconds - 1;
            }
            else
            {
                newParagraph.StartTime.TotalMilliseconds = 1000;
                newParagraph.EndTime.TotalMilliseconds = 3000;
            }

            _subtitle.Paragraphs.Insert(firstSelectedIndex, newParagraph);

            _subtitle.Renumber(startNumber);
            SubtitleListview1.Fill(_subtitle, _subtitleAlternate);
            SubtitleListview1.SelectIndexAndEnsureVisible(firstSelectedIndex);
            ShowSource();
            ShowStatus(_language.LineInserted);
            _change = true;
        }

        private void SubtitleListView1SelectedIndexChange()
        {
            StopAutoDuration();
            ShowLineInformationListView();            
            if (_subtitle.Paragraphs.Count > 0)
            {
                int firstSelectedIndex = 0;
                if (SubtitleListview1.SelectedItems.Count > 0)
                    firstSelectedIndex = SubtitleListview1.SelectedItems[0].Index;

                if (_subtitleListViewIndex >= 0)
                {
                    if (_subtitleListViewIndex == firstSelectedIndex)
                        return;

                    bool showSource = false;

                    Paragraph last = _subtitle.GetParagraphOrDefault(_subtitleListViewIndex);
                    if (textBoxListViewText.Text != last.Text)
                    {
                        last.Text = textBoxListViewText.Text.TrimEnd();
                        SubtitleListview1.SetText(_subtitleListViewIndex, last.Text);
                        showSource = true;
                    }

                    TimeCode startTime = timeUpDownStartTime.TimeCode;
                    if (startTime != null)
                    {
                        if (last.StartTime.TotalMilliseconds != startTime.TotalMilliseconds)
                        {
                            double dur = last.Duration.TotalMilliseconds;
                            last.StartTime.TotalMilliseconds = startTime.TotalMilliseconds;
                            last.EndTime.TotalMilliseconds = startTime.TotalMilliseconds + dur;
                            SubtitleListview1.SetStartTime(_subtitleListViewIndex, last);
                            showSource = true;
                        }
                    }

                    double duration = (double)numericUpDownDuration.Value * 1000.0;
                    if (duration > 0 && duration < 100000 && duration != last.Duration.TotalMilliseconds)
                    {
                        last.EndTime.TotalMilliseconds = last.StartTime.TotalMilliseconds + duration;
                        SubtitleListview1.SetDuration(_subtitleListViewIndex, last);
                        showSource = true;
                    }

                    if (showSource)
                    {
                        MakeHistoryForUndo(_language.BeforeLineUpdatedInListView);
                        ShowSource();
                    }
                }

                Paragraph p = _subtitle.GetParagraphOrDefault(firstSelectedIndex);
                if (p != null)
                {
                    textBoxListViewText.TextChanged -= TextBoxListViewTextTextChanged;
                    InitializeListViewEditBox(p);
                    textBoxListViewText.TextChanged += TextBoxListViewTextTextChanged;

                    _subtitleListViewIndex = firstSelectedIndex;
                    _oldSelectedParagraph = new Paragraph(p);

                    UpdateListViewTextInfo(p.Text);
                }
            }
        }


        private void SubtitleListview1_SelectedIndexChanged(object sender, EventArgs e)
        {
            SubtitleListView1SelectedIndexChange();
        }

        private void ShowLineInformationListView()
        {
            if (SubtitleListview1.SelectedItems.Count == 1)
                toolStripSelected.Text = string.Format("{0}/{1}", SubtitleListview1.SelectedItems[0].Index + 1, SubtitleListview1.Items.Count);
            else
                toolStripSelected.Text = string.Format(_language.XLinesSelected, SubtitleListview1.SelectedItems.Count);
        }

        private void UpdateListViewTextInfo(string text)
        {
            labelTextLineLengths.Text = _languageGeneral.SingleLineLengths;
            panelSingleLine.Left = labelTextLineLengths.Left + labelTextLineLengths.Width - 6;
            Utilities.DisplayLineLengths(panelSingleLine, text);

            string s = Utilities.RemoveHtmlTags(text).Replace(Environment.NewLine, " ");
            if (s.Length < Configuration.Settings.General.SubtitleLineMaximumLength * 1.9)
            {
                labelTextLineTotal.ForeColor = System.Drawing.Color.Black;
                labelTextLineTotal.Text = string.Format(_languageGeneral.TotalLengthX, s.Length);
            }
            else if (s.Length < Configuration.Settings.General.SubtitleLineMaximumLength * 2.1)
            {
                labelTextLineTotal.ForeColor = System.Drawing.Color.Orange;
                labelTextLineTotal.Text = string.Format(_languageGeneral.TotalLengthX, s.Length);
            }
            else
            {
                labelTextLineTotal.ForeColor = System.Drawing.Color.Red;
                labelTextLineTotal.Text = string.Format(_languageGeneral.TotalLengthXSplitLine, s.Length);
            }
        }

        private void ButtonNextClick(object sender, EventArgs e)
        {
            if (_subtitle.Paragraphs.Count > 0)
            {
                int firstSelectedIndex = 0;
                if (SubtitleListview1.SelectedItems.Count > 0)
                    firstSelectedIndex = SubtitleListview1.SelectedItems[0].Index;

                firstSelectedIndex++;
                Paragraph p = _subtitle.GetParagraphOrDefault(firstSelectedIndex);
                if (p != null)
                { 
                    SubtitleListview1.SelectNone();
                    SubtitleListview1.Items[firstSelectedIndex].Selected = true;
                    SubtitleListview1.EnsureVisible(firstSelectedIndex);
                }
            }
        }

        private void ButtonPreviousClick(object sender, EventArgs e)
        {
            if (_subtitle.Paragraphs.Count > 0)
            {
                int firstSelectedIndex = 1;
                if (SubtitleListview1.SelectedItems.Count > 0)
                    firstSelectedIndex = SubtitleListview1.SelectedItems[0].Index;

                firstSelectedIndex--;
                Paragraph p = _subtitle.GetParagraphOrDefault(firstSelectedIndex);
                if (p != null)
                {
                    SubtitleListview1.SelectNone();
                    SubtitleListview1.Items[firstSelectedIndex].Selected = true;
                    SubtitleListview1.EnsureVisible(firstSelectedIndex);
                }
            }
        }

        private void NormalToolStripMenuItemClick(object sender, EventArgs e)
        {
            if (_subtitle.Paragraphs.Count > 0 && SubtitleListview1.SelectedItems.Count > 0)
            {
                MakeHistoryForUndo(_language.BeforeSettingFontToNormal);
                foreach (ListViewItem item in SubtitleListview1.SelectedItems)
                {
                    Paragraph p = _subtitle.GetParagraphOrDefault(item.Index);
                    if (p != null)
                    {
                        p.Text = Utilities.RemoveHtmlTags(p.Text);
                        SubtitleListview1.SetText(item.Index, p.Text);
                    }
                }
                ShowSource();
                _change = true;
                RefreshSelectedParagraph();
            }
        }

        private void ButtonAutoBreakClick(object sender, EventArgs e)
        {
            if (textBoxListViewText.Text.Length > 0)
                textBoxListViewText.Text = Utilities.AutoBreakLine(textBoxListViewText.Text);
        }

        private void TextBoxListViewTextTextChanged(object sender, EventArgs e)
        {
            if (_subtitleListViewIndex >= 0)
            {
                string text = textBoxListViewText.Text.TrimEnd();
                UpdateListViewTextInfo(text);

                // update _subtitle + listview
                _subtitle.Paragraphs[_subtitleListViewIndex].Text = text;
                SubtitleListview1.SetText(_subtitleListViewIndex, text);
                _change = true;
            }
        }

        private void TextBoxListViewTextKeyDown(object sender, KeyEventArgs e)
        {
            int numberOfNewLines = textBoxListViewText.Text.Length - textBoxListViewText.Text.Replace(Environment.NewLine, " ").Length;
            if (e.KeyCode == Keys.Enter && numberOfNewLines > 1)
            {
                e.SuppressKeyPress = true;
            }

            // last key down in text
            _lastTextKeyDownTicks = DateTime.Now.Ticks;
        }

        private void SplitLineToolStripMenuItemClick(object sender, EventArgs e)
        {
            if (_subtitle.Paragraphs.Count > 0 && SubtitleListview1.SelectedItems.Count > 0)
            {
                SubtitleListview1.SelectedIndexChanged -= SubtitleListview1_SelectedIndexChanged;
                MakeHistoryForUndo(_language.BeforeSplitLine);

                int startNumber = _subtitle.Paragraphs[0].Number;
                int firstSelectedIndex = SubtitleListview1.SelectedItems[0].Index;

                Paragraph currentParagraph = _subtitle.GetParagraphOrDefault(firstSelectedIndex);
                var newParagraph = new Paragraph();

                double middle = currentParagraph.StartTime.TotalMilliseconds + (currentParagraph.Duration.TotalMilliseconds / 2.0);
                newParagraph.EndTime.TotalMilliseconds = currentParagraph.EndTime.TotalMilliseconds;
                currentParagraph.EndTime.TotalMilliseconds = middle;
                newParagraph.StartTime.TotalMilliseconds = currentParagraph.EndTime.TotalMilliseconds + 1;

                _subtitle.Paragraphs.Insert(firstSelectedIndex+1, newParagraph);


                string[] lines = currentParagraph.Text.Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                if (lines.Length == 2)
                {
                    currentParagraph.Text = Utilities.AutoBreakLine(lines[0]);
                    newParagraph.Text = Utilities.AutoBreakLine(lines[1]);
                }
                else
                {
                    string s = Utilities.AutoBreakLine(currentParagraph.Text);
                    lines = s.Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                    if (lines.Length == 2)
                    {
                        currentParagraph.Text = Utilities.AutoBreakLine(lines[0]);
                        newParagraph.Text = Utilities.AutoBreakLine(lines[1]);
                    }
                }

                _subtitle.Renumber(startNumber);
                SubtitleListview1.Fill(_subtitle, _subtitleAlternate);
                SubtitleListview1.SelectIndexAndEnsureVisible(firstSelectedIndex);
                ShowSource();
                ShowStatus(_language.LineSplitted);
                SubtitleListview1.SelectedIndexChanged += SubtitleListview1_SelectedIndexChanged;
                RefreshSelectedParagraph();
            }
        }

        private void MergeBeforeToolStripMenuItemClick(object sender, EventArgs e)
        {
            if (_subtitle.Paragraphs.Count > 0 && SubtitleListview1.SelectedItems.Count > 0)
            {
                int startNumber = _subtitle.Paragraphs[0].Number;
                int firstSelectedIndex = SubtitleListview1.SelectedItems[0].Index;

                Paragraph prevParagraph = _subtitle.GetParagraphOrDefault(firstSelectedIndex-1);
                Paragraph currentParagraph = _subtitle.GetParagraphOrDefault(firstSelectedIndex);

                if (prevParagraph != null && currentParagraph != null)
                {
                    SubtitleListview1.SelectedIndexChanged -= SubtitleListview1_SelectedIndexChanged;
                    MakeHistoryForUndo(_language.BeforeMergeLines);

                    prevParagraph.Text = prevParagraph.Text.Replace(Environment.NewLine, " ");
                    prevParagraph.Text += Environment.NewLine + currentParagraph.Text.Replace(Environment.NewLine, " ");
                    prevParagraph.Text = Utilities.AutoBreakLine(prevParagraph.Text);
                    prevParagraph.EndTime.TotalMilliseconds = prevParagraph.EndTime.TotalMilliseconds + currentParagraph.Duration.TotalMilliseconds; // currentParagraph.EndTime;

                    _subtitle.Paragraphs.Remove(currentParagraph);

                    _subtitle.Renumber(startNumber);
                    SubtitleListview1.Fill(_subtitle, _subtitleAlternate);
                    SubtitleListview1.Items[firstSelectedIndex-1].Selected = true;
                    SubtitleListview1.SelectIndexAndEnsureVisible(firstSelectedIndex-1);
                    ShowSource();
                    ShowStatus(_language.LinesMerged);
                    SubtitleListview1.SelectedIndexChanged += SubtitleListview1_SelectedIndexChanged;
                    RefreshSelectedParagraph();
                    _change = true;
                }
            }
        }

        private void MergeAfterToolStripMenuItemClick(object sender, EventArgs e)
        {
            if (_subtitle.Paragraphs.Count > 0 && SubtitleListview1.SelectedItems.Count > 0)
            {
                int startNumber = _subtitle.Paragraphs[0].Number;
                int firstSelectedIndex = SubtitleListview1.SelectedItems[0].Index;

                Paragraph currentParagraph = _subtitle.GetParagraphOrDefault(firstSelectedIndex);
                Paragraph nextParagraph = _subtitle.GetParagraphOrDefault(firstSelectedIndex + 1);

                if (nextParagraph != null && currentParagraph != null)
                {
                    SubtitleListview1.SelectedIndexChanged -= SubtitleListview1_SelectedIndexChanged;
                    MakeHistoryForUndo(_language.BeforeMergeLines);

                    currentParagraph.Text = currentParagraph.Text.Replace(Environment.NewLine, " ");
                    currentParagraph.Text += Environment.NewLine + nextParagraph.Text.Replace(Environment.NewLine, " ");
                    currentParagraph.Text = Utilities.AutoBreakLine(currentParagraph.Text);
                    currentParagraph.EndTime.TotalMilliseconds = currentParagraph.EndTime.TotalMilliseconds + nextParagraph.Duration.TotalMilliseconds; //nextParagraph.EndTime;

                    _subtitle.Paragraphs.Remove(nextParagraph);

                    _subtitle.Renumber(startNumber);
                    SubtitleListview1.Fill(_subtitle, _subtitleAlternate);
                    SubtitleListview1.SelectIndexAndEnsureVisible(firstSelectedIndex);
                    ShowSource();
                    ShowStatus(_language.LinesMerged);
                    SubtitleListview1.SelectedIndexChanged += SubtitleListview1_SelectedIndexChanged;
                    RefreshSelectedParagraph();                    
                    _change = true;
                }
            }
        }

        private void UpdateStartTimeInfo(TimeCode startTime)
        {
            if (_subtitle.Paragraphs.Count > 0 && SubtitleListview1.SelectedItems.Count > 0 && startTime != null)
            {
                UpdateOverlapErrors(startTime);

                // update _subtitle + listview
                Paragraph p = _subtitle.Paragraphs[_subtitleListViewIndex];
                p.EndTime.TotalMilliseconds += (startTime.TotalMilliseconds - p.StartTime.TotalMilliseconds);
                p.StartTime = startTime;
                SubtitleListview1.SetStartTime(_subtitleListViewIndex, p);
            }
        }

        private void UpdateOverlapErrors(TimeCode startTime)
        {
            labelStartTimeWarning.Text = string.Empty;
            labelDurationWarning.Text = string.Empty;

            if (_subtitle.Paragraphs.Count > 0 && SubtitleListview1.SelectedItems.Count > 0 && startTime != null)
            {

                int firstSelectedIndex = SubtitleListview1.SelectedItems[0].Index;

                Paragraph prevParagraph = _subtitle.GetParagraphOrDefault(firstSelectedIndex - 1);
                if (prevParagraph != null && prevParagraph.EndTime.TotalMilliseconds > startTime.TotalMilliseconds)
                    labelStartTimeWarning.Text = string.Format(_languageGeneral.OverlapPreviousLineX, prevParagraph.EndTime.TotalSeconds - startTime.TotalSeconds);

                Paragraph nextParagraph = _subtitle.GetParagraphOrDefault(firstSelectedIndex + 1);
                if (nextParagraph != null)
                {
                    double durationMilliSeconds = (double)numericUpDownDuration.Value * 1000.0;
                    if (startTime.TotalMilliseconds + durationMilliSeconds > nextParagraph.StartTime.TotalMilliseconds)
                    {
                        labelDurationWarning.Text = string.Format(_languageGeneral.OverlapX, ((startTime.TotalMilliseconds + durationMilliSeconds) - nextParagraph.StartTime.TotalMilliseconds) / 1000.0);
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
        }
       
        private void NumericUpDownDurationValueChanged(object sender, EventArgs e)
        {
            if (_subtitle.Paragraphs.Count > 0 && SubtitleListview1.SelectedItems.Count > 0)
            {
                int firstSelectedIndex = SubtitleListview1.SelectedItems[0].Index;

                Paragraph currentParagraph = _subtitle.GetParagraphOrDefault(firstSelectedIndex);
                if (currentParagraph != null)
                {
                    UpdateOverlapErrors(timeUpDownStartTime.TimeCode);

                    // update _subtitle + listview
                    currentParagraph.EndTime.TotalMilliseconds = currentParagraph.StartTime.TotalMilliseconds + ((double)numericUpDownDuration.Value * 1000.0);
                    SubtitleListview1.SetDuration(firstSelectedIndex, currentParagraph);
                    _change = true;
                }
            }
        }

        private void ButtonUndoListViewChangesClick(object sender, EventArgs e)
        {
            if (_subtitleListViewIndex >= 0 && _oldSelectedParagraph != null)
            {
                var p = new Paragraph(_oldSelectedParagraph);
                _subtitle.Paragraphs[_subtitleListViewIndex] = p;

                SubtitleListview1.SetText(_subtitleListViewIndex, p.Text);
                SubtitleListview1.SetStartTime(_subtitleListViewIndex, p);
                SubtitleListview1.SetDuration(_subtitleListViewIndex, p);

                InitializeListViewEditBox(p);
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
            numericUpDownDuration.Value = (decimal)(p.Duration.TotalSeconds);
            numericUpDownDuration.ValueChanged += NumericUpDownDurationValueChanged;

            UpdateOverlapErrors(timeUpDownStartTime.TimeCode);
            if (_subtitle != null && _subtitle.Paragraphs.Count > 0)
                textBoxListViewText.Enabled = true;
        }

        void MaskedTextBox_TextChanged(object sender, EventArgs e)
        {
            if (_subtitleListViewIndex >= 0)
            {
                UpdateStartTimeInfo(timeUpDownStartTime.TimeCode);
                _change = true;
            }
        }

        private void FormMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            ReloadFromSourceView();
            if (!ContinueNewOrExit())
            {
                e.Cancel = true;
            }
            else
            {
                if (Configuration.Settings.General.StartRememberPositionAndSize && WindowState != FormWindowState.Minimized)
                {
                    Configuration.Settings.General.StartPosition = Left + ";" + Top;
                    if (WindowState == FormWindowState.Maximized)
                        Configuration.Settings.General.StartSize = "Maximized";
                    else
                        Configuration.Settings.General.StartSize = Width + ";" + Height;

                    //TODO: save in adjust all lines ...Configuration.Settings.General.DefaultAdjustMilliseconds = (int)timeUpDownAdjust.TimeCode.TotalMilliseconds;

                    Configuration.Settings.Save();
                }
            }

            if (_mutex != null)
            {
                _mutex.WaitOne();
                _mutex.ReleaseMutex();
            }
        }

        private void ButtonUnBreakClick(object sender, EventArgs e)
        {
            textBoxListViewText.Text = Utilities.UnbreakLine(textBoxListViewText.Text);
        }

        private void TabControlSubtitleSelectedIndexChanged(object sender, EventArgs e)
        {
            if (tabControlSubtitle.SelectedIndex == TabControlSourceView)
            {
                ShowSource();
                ShowSourceLineNumber();
                if (textBoxSource.CanFocus)
                    textBoxSource.Focus();
            }
            else
            {
                ReloadFromSourceView();
                ShowLineInformationListView();
                if (SubtitleListview1.CanFocus)
                    SubtitleListview1.Focus();
            }            
        }

        private void ColorToolStripMenuItemClick(object sender, EventArgs e)
        {
            if (_subtitle.Paragraphs.Count > 0 && SubtitleListview1.SelectedItems.Count > 0)
            {
                if (colorDialog1.ShowDialog(this) == DialogResult.OK)
                { 
                    string color = Utilities.ColorToHex(colorDialog1.Color);

                    MakeHistoryForUndo(_language.BeforeSettingColor);

                    foreach (ListViewItem item in SubtitleListview1.SelectedItems)
                    {
                        Paragraph p = _subtitle.GetParagraphOrDefault(item.Index);
                        if (p != null)
                        {
                            bool done = false;

                            string s = p.Text;
                            if (s.StartsWith("<font "))
                            {
                                int start = s.IndexOf("<font ");
                                if (start >= 0)
                                {
                                    int end = s.IndexOf(">", start);
                                    if (end > 0)
                                    {
                                        string f = s.Substring(start, end - start);
                                        if (f.Contains(" face=") && !f.Contains(" color="))
                                        {
                                            start = s.IndexOf(" face=", start);
                                            s = s.Insert(start, string.Format(" color=\"{0}\"", color));
                                            p.Text = s;
                                            done = true;
                                        }
                                        else if (f.Contains(" color="))
                                        {
                                            int colorStart = f.IndexOf(" color=");
                                            if (s.IndexOf("\"", colorStart + " color=".Length + 1) > 0)
                                                end = s.IndexOf("\"", colorStart + " color=".Length + 1);
                                            s = s.Substring(0, colorStart) + string.Format(" color=\"{0}", color) + s.Substring(end);
                                            p.Text = s;
                                            done = true;
                                        }
                                    }
                                }
                            }


                            if (!done)
                                p.Text = string.Format("<font color=\"{0}\">{1}</font>", color, p.Text);
                            SubtitleListview1.SetText(item.Index, p.Text);
                        }
                    }
                    _change = true;
                    RefreshSelectedParagraph();
                }
            }
        }

        private void toolStripMenuItemFont_Click(object sender, EventArgs e)
        {
            if (_subtitle.Paragraphs.Count > 0 && SubtitleListview1.SelectedItems.Count > 0)
            {
                if (fontDialog1.ShowDialog(this) == DialogResult.OK)
                {                   
                    MakeHistoryForUndo(_language.BeforeSettingFontName); 

                    foreach (ListViewItem item in SubtitleListview1.SelectedItems)
                    {
                        Paragraph p = _subtitle.GetParagraphOrDefault(item.Index);
                        if (p != null)
                        {
                            bool done = false;

                            string s = p.Text;
                            if (s.StartsWith("<font "))
                            {
                                int start = s.IndexOf("<font ");
                                if (start >= 0)
                                {
                                    int end = s.IndexOf(">", start);
                                    if (end > 0)
                                    {
                                        string f = s.Substring(start, end - start);
                                        if (f.Contains(" color=") && !f.Contains(" face="))
                                        {
                                            start = s.IndexOf(" color=", start);
                                            s = s.Insert(start, string.Format(" face=\"{0}\"", fontDialog1.Font.Name));
                                            p.Text = s;
                                            done = true;
                                        }
                                        else if (f.Contains(" face="))
                                        { 
                                            int faceStart = f.IndexOf(" face=");
                                            if (s.IndexOf("\"", faceStart + " face=".Length + 1) > 0)
                                                end = s.IndexOf("\"", faceStart + " face=".Length + 1);
                                            s = s.Substring(0, faceStart) + string.Format(" face=\"{0}", fontDialog1.Font.Name) + s.Substring(end);
                                            p.Text = s;
                                            done = true;
                                        }
                                    }
                                }
                            }


                            if (!done)
                                p.Text = string.Format("<font face=\"{0}\">{1}</font>", fontDialog1.Font.Name, s);
                            SubtitleListview1.SetText(item.Index, p.Text);
                        }
                    }
                    _change = true;
                    RefreshSelectedParagraph();
                }
            }
        }

        private void TypeEffectToolStripMenuItemClick(object sender, EventArgs e)
        {
            if (SubtitleListview1.SelectedItems.Count > 0)
            {
                var typewriter = new EffectTypewriter();

                typewriter.Initialize(SubtitleListview1.GetSelectedParagraph(_subtitle));

                if (typewriter.ShowDialog(this) == DialogResult.OK)
                {
                    MakeHistoryForUndo(_language.BeforeTypeWriterEffect);
                    int firstNumber = _subtitle.Paragraphs[0].Number;
                    int lastSelectedIndex = SubtitleListview1.SelectedItems[0].Index;
                    int index = lastSelectedIndex;
                    _subtitle.Paragraphs.RemoveAt(index);
                    foreach (Paragraph p in typewriter.TypewriterParagraphs)
                    {
                        _subtitle.Paragraphs.Insert(index, p);
                        index++;
                    }
                    _subtitle.Renumber(firstNumber);
                    _subtitleListViewIndex = -1;
                    ShowSource();
                    SubtitleListview1.Fill(_subtitle, _subtitleAlternate);
                    SubtitleListview1.SelectIndexAndEnsureVisible(lastSelectedIndex);
                }
                typewriter.Dispose();
            }
        }

        private void KarokeeEffectToolStripMenuItemClick(object sender, EventArgs e)
        {
            if (SubtitleListview1.SelectedItems.Count > 0)
            {
                var karaoke = new EffectKaraoke();

                karaoke.Initialize(SubtitleListview1.GetSelectedParagraph(_subtitle));

                if (karaoke.ShowDialog(this) == DialogResult.OK)
                {
                    MakeHistoryForUndo(_language.BeforeKaraokeEffect);
                    int firstNumber = _subtitle.Paragraphs[0].Number;
                    int lastSelectedIndex = SubtitleListview1.SelectedItems[0].Index;

                    int i = SubtitleListview1.SelectedItems.Count - 1;
                    while (i >= 0)
                    {
                        ListViewItem item = SubtitleListview1.SelectedItems[i];
                        Paragraph p = _subtitle.GetParagraphOrDefault(item.Index);
                        if (p != null)
                        {
                            int index = item.Index;
                            _subtitle.Paragraphs.RemoveAt(index);
                            foreach (Paragraph kp in karaoke.MakeAnimation(p))
                            {
                                _subtitle.Paragraphs.Insert(index, kp);
                                index++;
                            }
                        }
                        i--;
                    }

                    _subtitle.Renumber(firstNumber);
                    _subtitleListViewIndex = -1;
                    ShowSource();
                    SubtitleListview1.Fill(_subtitle, _subtitleAlternate);
                    SubtitleListview1.SelectIndexAndEnsureVisible(lastSelectedIndex);
                }
                karaoke.Dispose();
            }
        }

        private void MatroskaImportStripMenuItemClick(object sender, EventArgs e)
        {
            openFileDialog1.Title = _language.OpenMatroskaFile;
            openFileDialog1.FileName = string.Empty;
            openFileDialog1.Filter = _language.MatroskaFiles + "|*.mkv;*.mks|" + _languageGeneral.AllFiles + "|*.*";
            if (openFileDialog1.ShowDialog(this) == DialogResult.OK)
            {
                bool isValid;
                var matroska = new Matroska();
                var subtitleList = matroska.GetMatroskaSubtitleTracks(openFileDialog1.FileName, out isValid);
                if (isValid)
                {
                    if (subtitleList.Count == 0)
                    {
                        MessageBox.Show(_language.NoSubtitlesFound);
                    }
                    else
                    {
                        if (ContinueNewOrExit())
                        {
                            if (subtitleList.Count > 1)
                            {
                                MatroskaSubtitleChooser subtitleChooser = new MatroskaSubtitleChooser();
                                subtitleChooser.Initialize(subtitleList);
                                if (subtitleChooser.ShowDialog(this) == DialogResult.OK)
                                {
                                    LoadMatroskaSubtitle(subtitleList[subtitleChooser.SelectedIndex], openFileDialog1.FileName);
                                }
                            }
                            else
                            {
                                LoadMatroskaSubtitle(subtitleList[0], openFileDialog1.FileName);
                            }
                        }
                    }
                }
                else
                {
                    MessageBox.Show(string.Format(_language.NotAValidMatroskaFileX, openFileDialog1.FileName));
                }
            }
        }

        private void LoadMatroskaSubtitle(MatroskaSubtitleInfo matroskaSubtitleInfo, string fileName)
        {
            bool isValid;
            bool isSsa = false;
            var matroska = new Matroska();

            SubtitleFormat format;

            if (matroskaSubtitleInfo.CodecPrivate.ToLower().Contains("[script info]"))
            {
                format = new SubStationAlpha();
                isSsa = true;
            }
            else
            {
                format = new SubRip();
            }

            comboBoxSubtitleFormats.SelectedIndexChanged -= ComboBoxSubtitleFormatsSelectedIndexChanged;
            SetCurrentFormat(format);
            comboBoxSubtitleFormats.SelectedIndexChanged += ComboBoxSubtitleFormatsSelectedIndexChanged;
            
            ShowStatus(_language.ParsingMatroskaFile);
            Refresh();
            Cursor.Current = Cursors.WaitCursor;
            Subtitle sub = matroska.GetMatroskaSubtitle(fileName, (int)matroskaSubtitleInfo.TrackNumber, out isValid);
            Cursor.Current = Cursors.Default;
            if (isValid)
            {
                MakeHistoryForUndo(_language.BeforeImportFromMatroskaFile);
                _subtitleListViewIndex = -1;

                _subtitle.Paragraphs.Clear();

                if (isSsa)
                {
                    int commaCount = 100;

                    foreach (Paragraph p in sub.Paragraphs)
                    {
                        string s1 = p.Text;
                        if (s1.Contains(@"{\"))
                            s1 = s1.Substring(0, s1.IndexOf(@"{\"));
                        int temp = s1.Split(',').Length;
                        if (temp < commaCount)
                            commaCount = temp;
                    }

                    foreach (Paragraph p in sub.Paragraphs)
                    {
                        string s = string.Empty;
                        string[] arr = p.Text.Split(',');
                        if (arr.Length >= commaCount)
                        {
                            for (int i = commaCount; i <= arr.Length; i++)
                            {
                                if (s.Length > 0)
                                    s += ",";
                                s += arr[i-1];
                            }
                        }
                        p.Text = s;
                        _subtitle.Paragraphs.Add(p);
                    }
                }
                else
                {
                    foreach (Paragraph p in sub.Paragraphs)
                    {
                        _subtitle.Paragraphs.Add(p);
                    }
                }

                comboBoxEncoding.Text = "UTF-8";
                ShowStatus(_language.SubtitleImportedFromMatroskaFile);
                _subtitle.Renumber(1);
                _subtitle.WasLoadedWithFrameNumbers = false;
                _fileName = string.Empty;
                _fileDateTime = new DateTime();
                Text = Title;
                _converted = false;

                SubtitleListview1.Fill(_subtitle, _subtitleAlternate);
                if (_subtitle.Paragraphs.Count > 0)
                    SubtitleListview1.SelectIndexAndEnsureVisible(0);

                if (format.FriendlyName == new SubStationAlpha().FriendlyName)
                    _subtitle.Header = matroskaSubtitleInfo.CodecPrivate;
                ShowSource();
            }
        }

        private void SubtitleListview1_DragEnter(object sender, DragEventArgs e)
        {
            // make sure they're actually dropping files (not text or anything else)
            if (e.Data.GetDataPresent(DataFormats.FileDrop, false))
                e.Effect = DragDropEffects.All;
        }

        private void SubtitleListview1_DragDrop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (files.Length == 1)
            {
                if (ContinueNewOrExit())
                {
                    string fileName = files[0];

                    var fi = new FileInfo(fileName);
                    string ext = Path.GetExtension(fileName).ToLower();
                    if (fi.Length < 1024 * 1024 * 2) // max 2 mb
                    {
                        OpenSubtitle(fileName, null);
                    }
                    else if (fi.Length < 50000000 && ext == ".sub" && IsVobSubFile(fileName, true)) // max 50 mb
                    {
                        OpenSubtitle(fileName, null);
                    }
                    else
                    {
                        MessageBox.Show(string.Format(_language.DropFileXNotAccepted, fileName));
                    }
                }
            }
            else
            {
                MessageBox.Show(_language.DropOnlyOneFile);
            }
        }

        private void TextBoxSourceDragEnter(object sender, DragEventArgs e)
        {
            // make sure they're actually dropping files (not text or anything else)
            if (e.Data.GetDataPresent(DataFormats.FileDrop, false))
                e.Effect = DragDropEffects.All;
        }

        private void TextBoxSourceDragDrop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (files.Length == 1)
            {
                if (ContinueNewOrExit())
                {
                    OpenSubtitle(files[0], null);
                }
            }
            else
            {
                MessageBox.Show(_language.DropOnlyOneFile);
            }
        }

        private void ToolStripMenuItemManualAnsiClick(object sender, EventArgs e)
        {
            ReloadFromSourceView();
            openFileDialog1.Title = _language.OpenAnsiSubtitle;
            openFileDialog1.FileName = string.Empty;
            openFileDialog1.Filter = Utilities.GetOpenDialogFiler();
            if (openFileDialog1.ShowDialog(this) == DialogResult.OK)
            {
                var chooseEncoding = new ChooseEncoding();
                chooseEncoding.Initialize(openFileDialog1.FileName);
                if (chooseEncoding.ShowDialog(this) == DialogResult.OK)
                {
                    Encoding encoding = chooseEncoding.GetEncoding();
                    comboBoxEncoding.Text = "UTF-8";
                    OpenSubtitle(openFileDialog1.FileName, encoding);
                    _converted = true;
                }
            }

        }

        private void ChangeCasingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ChangeCasing(false);
        }

        private void ChangeCasing(bool onlySelectedLines)
        {
            if (_subtitle != null && _subtitle.Paragraphs.Count > 1)
            {
                SaveSubtitleListviewIndexes();
                var changeCasing = new ChangeCasing();
                _formPositionsAndSizes.SetPositionAndSize(changeCasing);
                if (onlySelectedLines)
                    changeCasing.Text += " - " + _language.SelectedLines;
                ReloadFromSourceView();
                if (changeCasing.ShowDialog(this) == DialogResult.OK)
                {
                    MakeHistoryForUndo(_language.BeforeChangeCasing);

                    Cursor.Current = Cursors.WaitCursor;
                    var selectedLines = new Subtitle();
                    selectedLines.WasLoadedWithFrameNumbers = _subtitle.WasLoadedWithFrameNumbers;
                    if (onlySelectedLines)
                    {
                        foreach (int index in SubtitleListview1.SelectedIndices)
                            selectedLines.Paragraphs.Add(new Paragraph(_subtitle.Paragraphs[index]));
                    }
                    else
                    {
                        foreach (Paragraph p in _subtitle.Paragraphs)
                            selectedLines.Paragraphs.Add(new Paragraph(p));
                    }

                    bool saveChangeCaseChanges = true;
                    changeCasing.FixCasing(selectedLines, Utilities.AutoDetectGoogleLanguage(_subtitle));
                    var changeCasingNames = new ChangeCasingNames();
                    if (changeCasing.ChangeNamesToo)
                    {
                        changeCasingNames.Initialize(selectedLines);
                        if (changeCasingNames.ShowDialog(this) == DialogResult.OK)
                        {
                            changeCasingNames.FixCasing();

                            if (changeCasing.LinesChanged == 0)
                                ShowStatus(string.Format(_language.CasingCompleteMessageOnlyNames, changeCasingNames.LinesChanged, _subtitle.Paragraphs.Count));
                            else
                                ShowStatus(string.Format(_language.CasingCompleteMessage, changeCasing.LinesChanged, _subtitle.Paragraphs.Count, changeCasingNames.LinesChanged));
                        }
                        else
                        {
                            saveChangeCaseChanges = false;
                        }
                    }
                    else
                    {
                        ShowStatus(string.Format(_language.CasingCompleteMessageNoNames, changeCasing.LinesChanged, _subtitle.Paragraphs.Count));
                    }

                    if (saveChangeCaseChanges)
                    {
                        if (onlySelectedLines)
                        {
                            int i = 0;
                            foreach (int index in SubtitleListview1.SelectedIndices)
                            {
                                _subtitle.Paragraphs[index].Text = selectedLines.Paragraphs[i].Text;
                                i++;
                            }
                        }
                        else
                        {
                            for (int i = 0; i < _subtitle.Paragraphs.Count; i++)
                            {
                                _subtitle.Paragraphs[i].Text = selectedLines.Paragraphs[i].Text;
                            }
                        }
                        ShowSource();
                        SubtitleListview1.Fill(_subtitle, _subtitleAlternate);
                        if (changeCasing.LinesChanged > 0 || changeCasingNames.LinesChanged > 0)
                        {
                            _change = true;
                            _subtitleListViewIndex = -1;
                            RestoreSubtitleListviewIndexes();
                        }
                    }
                    Cursor.Current = Cursors.Default;
                }
                _formPositionsAndSizes.SavePositionAndSize(changeCasing);
            }
            else
            {
                MessageBox.Show(_language.NoSubtitleLoaded, Title, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void ToolStripMenuItemChangeFramerateClick(object sender, EventArgs e)
        {
            if (_subtitle != null && _subtitle.Paragraphs.Count > 1)
            {
                int lastSelectedIndex = 0;
                if (SubtitleListview1.SelectedItems.Count > 0)
                    lastSelectedIndex = SubtitleListview1.SelectedItems[0].Index;

                ReloadFromSourceView();
                var changeFramerate = new ChangeFrameRate();
                _formPositionsAndSizes.SetPositionAndSize(changeFramerate);
                changeFramerate.Initialize(CurrentFrameRate.ToString());
                if (changeFramerate.ShowDialog(this) == DialogResult.OK)
                {
                    MakeHistoryForUndo(_language.BeforeChangeFrameRate);

                    double oldFramerate = changeFramerate.OldFrameRate;
                    double newFramerate = changeFramerate.NewFrameRate;
                    _subtitle.ChangeFramerate(oldFramerate, newFramerate);

                    ShowStatus(string.Format(_language.FrameRateChangedFromXToY, oldFramerate, newFramerate));
                    toolStripComboBoxFrameRate.Text = newFramerate.ToString();

                    ShowSource();
                    SubtitleListview1.Fill(_subtitle, _subtitleAlternate);
                    _change = true;
                    _subtitleListViewIndex = -1;
                    SubtitleListview1.SelectIndexAndEnsureVisible(lastSelectedIndex);
                }
                _formPositionsAndSizes.SavePositionAndSize(changeFramerate);
            }
            else
            {
                MessageBox.Show(_language.NoSubtitleLoaded, Title, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private bool IsVobSubFile(string subFileName, bool verbose)
        {
            try
            {
                var buffer = new byte[4];
                var fs = new FileStream(subFileName, FileMode.Open, FileAccess.Read, FileShare.Read) {Position = 0};
                fs.Read(buffer, 0, 4);
                bool isHeaderOk = VobSubParser.IsMpeg2PackHeader(buffer) || VobSubParser.IsPrivateStream1(buffer, 0);
                fs.Close();
                if (isHeaderOk) 
                {
                    if (!verbose)
                        return true;

                    string idxFileName = Path.Combine(Path.GetDirectoryName(subFileName), Path.GetFileNameWithoutExtension(subFileName) + ".idx");
                    if (File.Exists(idxFileName))
                        return true;
                    return (MessageBox.Show(string.Format(_language.IdxFileNotFoundWarning, idxFileName ), _title, MessageBoxButtons.YesNo) ==  DialogResult.Yes);
                }
                if (verbose)
                    MessageBox.Show(string.Format(_language.InvalidVobSubHeader,  subFileName));
            }
            catch (Exception ex)
            {
                if (verbose)
                    MessageBox.Show(ex.Message);
            }
            return false;
        }

        private void ImportAndOcrVobSubSubtitleNew(string fileName)
        {
            if (IsVobSubFile(fileName, true))
            {
                var vobSubOcr = new VobSubOcr();
                if (vobSubOcr.Initialize(fileName, Configuration.Settings.VobSubOcr, true))
                {
                    if (vobSubOcr.ShowDialog(this) == DialogResult.OK)
                    {
                        MakeHistoryForUndo(_language.BeforeImportingVobSubFile);

                        _subtitle.Paragraphs.Clear();
                        SetCurrentFormat(new SubRip().FriendlyName);
                        _subtitle.WasLoadedWithFrameNumbers = false;
                        _subtitle.CalculateFrameNumbersFromTimeCodes(CurrentFrameRate);
                        foreach (Paragraph p in vobSubOcr.SubtitleFromOcr.Paragraphs)
                        {
                            _subtitle.Paragraphs.Add(p);
                        }

                        ShowSource();
                        SubtitleListview1.Fill(_subtitle, _subtitleAlternate);
                        _change = true;
                        _subtitleListViewIndex = -1;
                        SubtitleListview1.FirstVisibleIndex = -1;
                        SubtitleListview1.SelectIndexAndEnsureVisible(0);

                        _fileName = Path.ChangeExtension(vobSubOcr.FileName, ".srt");
                        Text = Title + " - " + _fileName;

                        Configuration.Settings.Save();
                    }
                }
            }
        }

        private void ToolStripMenuItemMergeLinesClick(object sender, EventArgs e)
        {
            if (_subtitle.Paragraphs.Count > 0 && SubtitleListview1.SelectedItems.Count >= 1 && SubtitleListview1.SelectedItems.Count <= 2)
                MergeAfterToolStripMenuItemClick(null, null);
        }

        private void VisualSyncSelectedLinesToolStripMenuItemClick(object sender, EventArgs e)
        {
            ShowVisualSync(true);           
        }

        private void GoogleTranslateSelectedLinesToolStripMenuItemClick(object sender, EventArgs e)
        {
            TranslateViaGoogle(true, true);
        }

        private void SaveSubtitleListviewIndexes()
        {
            _selectedIndexes = new List<int>();
            foreach (int index in SubtitleListview1.SelectedIndices)
                _selectedIndexes.Add(index);
        }

        private void RestoreSubtitleListviewIndexes()
        {
            _subtitleListViewIndex = -1;
            if (_selectedIndexes != null)
            {
                SubtitleListview1.SelectNone();
                int i = 0;
                foreach (int index in _selectedIndexes)
                {
                    if (index >= 0 && index < SubtitleListview1.Items.Count)
                    { 
                        SubtitleListview1.Items[index].Selected = true;
                        if (i == 0)
                            SubtitleListview1.Items[index].EnsureVisible();
                    }
                    i++;
                }
            }
        }

        private void ShowSelectedLinesEarlierlaterToolStripMenuItemClick(object sender, EventArgs e)
        {
            if (_subtitle != null && _subtitle.Paragraphs.Count > 1)
            {
                var showEarlierOrLater = new ShowEarlierLater();
                SaveSubtitleListviewIndexes();

                var selectedLines = new Subtitle { WasLoadedWithFrameNumbers = _subtitle.WasLoadedWithFrameNumbers };
                foreach (int index in SubtitleListview1.SelectedIndices)
                    selectedLines.Paragraphs.Add(_subtitle.Paragraphs[index]);
                showEarlierOrLater.Initialize(selectedLines, _videoFileName);

                if (showEarlierOrLater.ShowDialog(this) == DialogResult.OK)
                {
                    MakeHistoryForUndo(_language.BeforeShowSelectedLinesEarlierLater);

                    // we only update selected lines
                    int i = 0;
                    double frameRate = CurrentFrameRate;
                    foreach (int index in SubtitleListview1.SelectedIndices)
                    {
                        _subtitle.Paragraphs[index] = new Paragraph(showEarlierOrLater.Paragraphs[i]);
                        if (_subtitle.WasLoadedWithFrameNumbers)
                            _subtitle.Paragraphs[index].CalculateFrameNumbersFromTimeCodes(frameRate);
                        i++;
                    }
                    ShowStatus(_language.ShowSelectedLinesEarlierLaterPerformed);
                    ShowSource();
                    SubtitleListview1.Fill(_subtitle, _subtitleAlternate);
                    RestoreSubtitleListviewIndexes();
                }
                showEarlierOrLater.Dispose();
            }
            else
            {
                MessageBox.Show(_language.NoSubtitleLoaded, Title, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void Main_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Right && e.Modifiers == Keys.Control)
            {
                if (!textBoxListViewText.Focused)
                {
                    mediaPlayer.CurrentPosition += 0.10;
                    e.SuppressKeyPress = true;
                }
            }
            else if (e.KeyCode == Keys.Left && e.Modifiers == Keys.Control)
            {
                if (!textBoxListViewText.Focused)
                {
                    mediaPlayer.CurrentPosition -= 0.10;
                    e.SuppressKeyPress = true;
                }
            }
            else if (e.KeyCode == Keys.Right && e.Modifiers == Keys.Alt)
            {
                if (mediaPlayer.VideoPlayer != null)
                {
                    mediaPlayer.CurrentPosition += 0.5;
                    e.SuppressKeyPress = true;
                }
            }
            else if (e.KeyCode == Keys.Left && e.Modifiers == Keys.Alt)
            {
                if (mediaPlayer.VideoPlayer != null)
                {
                    mediaPlayer.CurrentPosition -= 0.5;
                    e.SuppressKeyPress = true;
                }
            }
            else if (e.KeyCode == Keys.Down && e.Modifiers == Keys.Alt)
            {
                if (AutoRepeatContinueOn)
                    Next();
                else
                    ButtonNextClick(null, null);
            }
            else if (e.KeyCode == Keys.Up && e.Modifiers == Keys.Alt)
            {
                if (AutoRepeatContinueOn)
                    PlayPrevious();
                else
                    ButtonPreviousClick(null, null);
            }
            else if (e.KeyCode == Keys.Home && e.Modifiers == Keys.Alt)
            {
                SubtitleListview1.FirstVisibleIndex = -1;
                SubtitleListview1.SelectIndexAndEnsureVisible(0);
                e.SuppressKeyPress = true;
            }
            else if (e.KeyCode == Keys.End && e.Modifiers == Keys.Alt)
            {
                SubtitleListview1.SelectIndexAndEnsureVisible(SubtitleListview1.Items.Count - 1);
                e.SuppressKeyPress = true;
            }
            else if (e.Modifiers == Keys.Control && e.KeyCode == Keys.L) //Locate first selected line in subtitle listview
            {
                if (SubtitleListview1.SelectedItems.Count > 0)
                    SubtitleListview1.SelectedItems[0].EnsureVisible();
                e.SuppressKeyPress = true;
            }
            else if (e.Modifiers == (Keys.Control | Keys.Alt | Keys.Shift) && e.KeyCode == Keys.R) // reload "Language.xml"
            {
                if (File.Exists(Configuration.BaseDirectory + "Language.xml"))
                    SetLanguage("Language.xml");
            }
            else if (e.Modifiers == (Keys.Control | Keys.Shift) && e.KeyCode == Keys.M)
            {
                if (_subtitle.Paragraphs.Count > 0 && SubtitleListview1.SelectedItems.Count >= 1 && SubtitleListview1.SelectedItems.Count <= 2)
                {
                    e.SuppressKeyPress = true;
                    MergeAfterToolStripMenuItemClick(null, null);
                }
            }
            else if (e.Modifiers == Keys.Control && e.KeyCode == Keys.U)
            { // toogle translator mode
                EditToolStripMenuItemDropDownOpening(null, null);
                toolStripMenuItemTranslationMode_Click(null, null);
            }
            else if (e.Modifiers == Keys.Control && e.KeyCode == Keys.P)
            {
                if (mediaPlayer.VideoPlayer != null)
                {
                    mediaPlayer.TooglePlayPause();
                    e.SuppressKeyPress = true;
                }
            }
            else if (e.Modifiers == (Keys.Control | Keys.Shift) && e.KeyCode == Keys.Right)
            {
                if (!textBoxListViewText.Focused)
                {
                    mediaPlayer.CurrentPosition += 1.0;
                    e.SuppressKeyPress = true;
                }
            }
            else if (e.Modifiers == (Keys.Control | Keys.Shift) && e.KeyCode == Keys.Left)
            {
                if (!textBoxListViewText.Focused)
                {
                    mediaPlayer.CurrentPosition -= 1.0;
                    e.SuppressKeyPress = true;
                }
            }
            else if (e.Modifiers == Keys.None && e.KeyCode == Keys.Space)
            {
                if (!textBoxListViewText.Focused && mediaPlayer.VideoPlayer != null)
                {
                    mediaPlayer.TooglePlayPause();
                    e.SuppressKeyPress = true;
                }
            }
            else if (e.Modifiers == Keys.None && e.KeyCode == Keys.F7)
            {
                if (mediaPlayer.VideoPlayer != null)
                {
                    GoBackSeconds(3, labelSubtitle, mediaPlayer.VideoPlayer);
                }
            }
            else if (e.Modifiers == Keys.None && e.KeyCode == Keys.F8)
            {
                if (mediaPlayer.VideoPlayer != null)
                {
                    mediaPlayer.TooglePlayPause();
                    e.SuppressKeyPress = true;
                }
            }
            else if (tabControlButtons.SelectedTab == tabPageAdjust && mediaPlayer.VideoPlayer != null)
            {
                if ((e.Modifiers == Keys.Control && e.KeyCode == Keys.Space))
                {
                    buttonSetStartAndOffsetRest_Click(null, null);
                }
                else if (e.Modifiers == Keys.Shift && e.KeyCode == Keys.Space)
                {
                    buttonSetEndAndGoToNext_Click(null, null);
                }
                else if (e.Modifiers == Keys.None && e.KeyCode == Keys.F9)
                {
                    buttonSetStartAndOffsetRest_Click(null, null);
                    e.SuppressKeyPress = true;
                }
                else if (e.Modifiers == Keys.None && e.KeyCode == Keys.F10)
                {
                    buttonSetEndAndGoToNext_Click(null, null);
                    e.SuppressKeyPress = true;
                }
                else if (e.Modifiers == Keys.None && e.KeyCode == Keys.F11)
                {
                    buttonSetStartTime_Click(null, null);
                    e.SuppressKeyPress = true;
                }
                else if (e.Modifiers == Keys.None && e.KeyCode == Keys.F12)
                {
                    buttonSetEnd_Click(null, null);
                    e.SuppressKeyPress = true;
                }
            }
            else if (tabControlButtons.SelectedTab == tabPageCreate && mediaPlayer.VideoPlayer != null)
            {
                if (e.Modifiers == Keys.None && e.KeyCode == Keys.F9)
                {
                    buttonInsertNewText_Click(null, null);
                    e.SuppressKeyPress = true;
                }
                else if (e.Modifiers == Keys.None && e.KeyCode == Keys.F10)
                {
                    buttonBeforeText_Click(null, null); 
                    e.SuppressKeyPress = true;
                }
                else if (e.Modifiers == Keys.None && e.KeyCode == Keys.F11)
                {
                    buttonSetStartTime_Click(null, null);
                    e.SuppressKeyPress = true;
                }
                else if (e.Modifiers == Keys.None && e.KeyCode == Keys.F12)
                {
                    buttonSetEnd_Click(null, null);
                    e.SuppressKeyPress = true;
                }

            }
        }

        private void SubtitleListview1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.A && e.Control) //SelectAll
            { 
                foreach (ListViewItem item in SubtitleListview1.Items)
                    item.Selected = true;
                e.SuppressKeyPress = true;
            }
            else if (e.KeyCode == Keys.D && e.Control) //SelectFirstSelectedItemOnly
            {
                if (SubtitleListview1.SelectedItems.Count > 0)
                {
                    bool skipFirst = true;
                    foreach (ListViewItem item in SubtitleListview1.SelectedItems)
                    {
                        if (skipFirst)
                            skipFirst = false;
                        else
                            item.Selected = false;
                    }
                    e.SuppressKeyPress = true;
                }
            }
            else if (e.KeyCode == Keys.Delete && SubtitleListview1.SelectedItems.Count > 0) //Delete
            {
                ToolStripMenuItemDeleteClick(null, null);
            }
            else if (e.Shift && e.KeyCode == Keys.Insert)
            {
                InsertBefore();
                e.SuppressKeyPress = true;
            }
            else if (e.KeyCode == Keys.Insert)
            {
                InsertAfter();
                e.SuppressKeyPress = true;
            }
            else if (e.Shift && e.Control && e.KeyCode == Keys.I) //InverseSelection
            {
                foreach (ListViewItem item in SubtitleListview1.Items)
                    item.Selected = !item.Selected;
                e.SuppressKeyPress = true;
            }
            else if (e.Modifiers == Keys.Control && e.KeyCode == Keys.Home)
            {
                SubtitleListview1.FirstVisibleIndex = -1;
                SubtitleListview1.SelectIndexAndEnsureVisible(0);
                e.SuppressKeyPress = true;
            }
            else if (e.Modifiers == Keys.Control && e.KeyCode == Keys.End)
            {
                SubtitleListview1.SelectIndexAndEnsureVisible(SubtitleListview1.Items.Count-1);
                e.SuppressKeyPress = true;
            }
            else if (e.Modifiers == Keys.None && e.KeyCode == Keys.Enter)
            {
                SubtitleListview1_MouseDoubleClick(null, null);
            }
        }

        private void AdjustDisplayTimeForSelectedLinesToolStripMenuItemClick(object sender, EventArgs e)
        {
            AdjustDisplayTime(true);
        }

        private void FixCommonErrorsInSelectedLinesToolStripMenuItemClick(object sender, EventArgs e)
        {
            FixCommonErrors(true);
        }

        private void FindDoubleWordsToolStripMenuItemClick(object sender, EventArgs e)
        {
            var regex = new Regex(@"\b([\w]+)[ \r\n]+\1[ ,.!?]");
            _findHelper = new FindReplaceDialogHelper(FindType.RegEx, string.Format(_language.DoubleWordsViaRegEx, regex), regex, string.Empty, 0, 0, _subtitleListViewIndex);

            ReloadFromSourceView();
            FindNext();
        }

        private void ChangeCasingForSelectedLinesToolStripMenuItemClick(object sender, EventArgs e)
        {
            ChangeCasing(true);
        }

        private void CenterFormOnCurrentScreen()
        {
            Screen screen = Screen.FromControl(this);
            Left = screen.Bounds.X + ((screen.Bounds.Width - Width) / 2);
            Top = screen.Bounds.Y + ((screen.Bounds.Height - Height) / 2);
        }

        private void SortSubtitle(SubtitleSortCriteria subtitleSortCriteria, string description)
        {
            Paragraph firstSelectedParagraph = null;
            if (SubtitleListview1.SelectedItems.Count > 0)
                firstSelectedParagraph = _subtitle.Paragraphs[SubtitleListview1.SelectedItems[0].Index];

            _subtitleListViewIndex = -1;
            MakeHistoryForUndo(string.Format(_language.BeforeSortX, description));
            _subtitle.Sort(subtitleSortCriteria);
            ShowSource();
            SubtitleListview1.Fill(_subtitle, _subtitleAlternate);
            SubtitleListview1.SelectIndexAndEnsureVisible(firstSelectedParagraph);
            ShowStatus(string.Format(_language.SortedByX, description));
        }

        private void SortNumberToolStripMenuItemClick(object sender, EventArgs e)
        {
            SortSubtitle(SubtitleSortCriteria.Number, (sender as ToolStripItem).Text);
        }

        private void SortStartTimeToolStripMenuItemClick(object sender, EventArgs e)
        {
            SortSubtitle(SubtitleSortCriteria.StartTime, (sender as ToolStripItem).Text);
        }

        private void SortEndTimeToolStripMenuItemClick(object sender, EventArgs e)
        {
            SortSubtitle(SubtitleSortCriteria.EndTime, (sender as ToolStripItem).Text);
        }

        private void SortDisplayTimeToolStripMenuItemClick(object sender, EventArgs e)
        {
            SortSubtitle(SubtitleSortCriteria.Duration, (sender as ToolStripItem).Text);
        }

        private void SortTextMaxLineLengthToolStripMenuItemClick(object sender, EventArgs e)
        {
            SortSubtitle(SubtitleSortCriteria.TextMaxLineLength, (sender as ToolStripItem).Text);
        }

        private void SortTextTotalLengthToolStripMenuItemClick(object sender, EventArgs e)
        {
            SortSubtitle(SubtitleSortCriteria.TextTotalLength, (sender as ToolStripItem).Text);
        }

        private void SortTextNumberOfLinesToolStripMenuItemClick(object sender, EventArgs e)
        {
            SortSubtitle(SubtitleSortCriteria.TextNumberOfLines, (sender as ToolStripItem).Text);
        }

        private void SortTextAlphabeticallytoolStripMenuItemClick(object sender, EventArgs e)
        {
            SortSubtitle(SubtitleSortCriteria.Text, (sender as ToolStripItem).Text);
        }

        private void ChangeLanguageToolStripMenuItemClick(object sender, EventArgs e)
        {
            var cl = new ChooseLanguage();
            _formPositionsAndSizes.SetPositionAndSize(cl);
            if (cl.ShowDialog(this) == DialogResult.OK)
            {
                SetLanguage(cl.CultureName);
                Configuration.Settings.Save();
            }
            _formPositionsAndSizes.SavePositionAndSize(cl);
         }

        private void SetLanguage(string cultureName)
        {
            try
            {
                if (string.IsNullOrEmpty(cultureName) || cultureName == "en-US")
                {
                    Configuration.Settings.Language = new Language(); // default is en-US
                }
                else if (cultureName == "Language.xml")
                {
                    var reader = new StreamReader(Configuration.BaseDirectory + cultureName);
                    Configuration.Settings.Language = Language.Load(reader);
                    reader.Close();
                }
                else
                {
                    System.Reflection.Assembly asm = System.Reflection.Assembly.GetExecutingAssembly();

                    Stream strm = asm.GetManifestResourceStream("Nikse.SubtitleEdit.Resources." + cultureName + ".xml.zip");
                    if (strm != null)
                    {
                        var rdr = new StreamReader(strm);
                        Configuration.Settings.Language = Language.LoadAndDecompress(rdr);
                        rdr.Close();
                    }
                    else
                    {
                        strm = asm.GetManifestResourceStream("Nikse.SubtitleEdit.Resources." + cultureName + ".xml");
                        if (strm != null)
                        {
                            var reader = new StreamReader(strm);
                            Configuration.Settings.Language = Language.Load(reader);
                            reader.Close();
                        }
                    }
                }
                Configuration.Settings.General.Language = cultureName;
                _languageGeneral = Configuration.Settings.Language.General;
                _language = Configuration.Settings.Language.Main;
                InitializeLanguage();
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message + Environment.NewLine +
                                Environment.NewLine +
                                exception.StackTrace, "Error loading language file");
                Configuration.Settings.Language = new Language(); // default is en-US
                _languageGeneral = Configuration.Settings.Language.General;
                _language = Configuration.Settings.Language.Main;
                InitializeLanguage();
                Configuration.Settings.General.Language = null;
            }
        }

        private void ToolStripMenuItemCompareClick(object sender, EventArgs e)
        {
            var compareForm = new Compare();
            compareForm.Initialize(_subtitle, _fileName, Configuration.Settings.Language.General.CurrentSubtitle);
            compareForm.Show();
        }

        private void ToolStripMenuItemAutoBreakLinesClick(object sender, EventArgs e)
        {
            if (_subtitle != null && _subtitle.Paragraphs.Count > 1)
            {
                ReloadFromSourceView();
                var autoBreakUnbreakLines = new AutoBreakUnbreakLines();
                var selectedLines = new Subtitle();
                foreach (int index in SubtitleListview1.SelectedIndices)
                    selectedLines.Paragraphs.Add(_subtitle.Paragraphs[index]);
                autoBreakUnbreakLines.Initialize(selectedLines, true);

                if (autoBreakUnbreakLines.ShowDialog() == DialogResult.OK && autoBreakUnbreakLines.FixedParagraphs.Count > 0)
                {
                    MakeHistoryForUndo(_language.BeforeAutoBalanceSelectedLines);

                    SubtitleListview1.BeginUpdate();
                    foreach (int index in SubtitleListview1.SelectedIndices)
                    {
                        Paragraph p = _subtitle.GetParagraphOrDefault(index);

                        int indexFixed = autoBreakUnbreakLines.FixedParagraphs.IndexOf(p);
                        if (indexFixed >= 0)
                        {
                            p.Text = Utilities.AutoBreakLine(p.Text);
                            SubtitleListview1.SetText(index, p.Text);
                        }
                    }
                    SubtitleListview1.EndUpdate();
                    _change = true;
                    RefreshSelectedParagraph();
                    ShowStatus(string.Format(_language.NumberOfLinesAutoBalancedX, autoBreakUnbreakLines.FixedParagraphs.Count));
                }
            }
            else
            {
                MessageBox.Show(_language.NoSubtitleLoaded, Title, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void ToolStripMenuItemUnbreakLinesClick(object sender, EventArgs e)
        {
            if (_subtitle != null && _subtitle.Paragraphs.Count > 1)
            {
                ReloadFromSourceView();
                var autoBreakUnbreakLines = new AutoBreakUnbreakLines();
                var selectedLines = new Subtitle();
                foreach (int index in SubtitleListview1.SelectedIndices)
                    selectedLines.Paragraphs.Add(_subtitle.Paragraphs[index]);
                autoBreakUnbreakLines.Initialize(selectedLines, false);

                if (autoBreakUnbreakLines.ShowDialog() == DialogResult.OK && autoBreakUnbreakLines.FixedParagraphs.Count > 0)
                {
                    MakeHistoryForUndo(_language.BeforeRemoveLineBreaksInSelectedLines);

                    SubtitleListview1.BeginUpdate();
                    foreach (int index in SubtitleListview1.SelectedIndices)
                    {
                        Paragraph p = _subtitle.GetParagraphOrDefault(index);

                        int indexFixed = autoBreakUnbreakLines.FixedParagraphs.IndexOf(p);
                        if (indexFixed >= 0)
                        {
                            p.Text = Utilities.UnbreakLine(p.Text);
                            SubtitleListview1.SetText(index, p.Text);
                        }
                    }
                    SubtitleListview1.EndUpdate();
                    _change = true;
                    RefreshSelectedParagraph();
                    ShowStatus(string.Format(_language.NumberOfWithRemovedLineBreakX, autoBreakUnbreakLines.FixedParagraphs.Count));
                }
            }
            else
            {
                MessageBox.Show(_language.NoSubtitleLoaded, Title, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void MultipleReplaceToolStripMenuItemClick(object sender, EventArgs e)
        {
            var multipleReplace = new MultipleReplace();
            multipleReplace.Initialize(_subtitle);
            if (multipleReplace.ShowDialog(this) == DialogResult.OK)
            {
                MakeHistoryForUndo(_language.BeforeMultipleReplace);
                SaveSubtitleListviewIndexes();

                for (int i = 0; i < _subtitle.Paragraphs.Count; i++)
                {
                    _subtitle.Paragraphs[i].Text = multipleReplace.FixedSubtitle.Paragraphs[i].Text;
                }

                SubtitleListview1.Fill(_subtitle, _subtitleAlternate);
                RestoreSubtitleListviewIndexes();
                RefreshSelectedParagraph();
                ShowSource();
                if (multipleReplace.FixCount > 0)
                    _change = true;
                ShowStatus(string.Format(_language.NumberOfLinesReplacedX , multipleReplace.FixCount));
            }
        }

        private void ToolStripMenuItemImportDvdSubtitlesClick(object sender, EventArgs e)
        {
            if (ContinueNewOrExit())
            {
                var formSubRip = new DvdSubRip();
                if (formSubRip.ShowDialog(this) == DialogResult.OK)
                {
                    var showSubtitles = new DvdSubRipChooseLanguage();
                    showSubtitles.Initialize(formSubRip.MergedVobSubPacks, formSubRip.Palette, formSubRip.Languages, formSubRip.SelectedLanguage);
                    if (showSubtitles.ShowDialog(this) == DialogResult.OK)
                    {
                        var formSubOcr = new VobSubOcr();
                        formSubOcr.Initialize(showSubtitles.SelectedVobSubMergedPacks, formSubRip.Palette, Configuration.Settings.VobSubOcr);
                        if (formSubOcr.ShowDialog(this) == DialogResult.OK)
                        {
                            MakeHistoryForUndo(_language.BeforeImportingDvdSubtitle);

                            _subtitle.Paragraphs.Clear();
                            SetCurrentFormat(new SubRip().FriendlyName);
                            _subtitle.WasLoadedWithFrameNumbers = false;
                            _subtitle.CalculateFrameNumbersFromTimeCodes(CurrentFrameRate);
                            foreach (Paragraph p in formSubOcr.SubtitleFromOcr.Paragraphs)
                            {
                                _subtitle.Paragraphs.Add(p);
                            }

                            ShowSource();
                            SubtitleListview1.Fill(_subtitle, _subtitleAlternate);
                            _change = true;
                            _subtitleListViewIndex = -1;
                            SubtitleListview1.FirstVisibleIndex = -1;
                            SubtitleListview1.SelectIndexAndEnsureVisible(0);

                            _fileName = string.Empty;
                            Text = Title;

                            Configuration.Settings.Save();
                        }
                    }
                }
            }
        }

        private void ToolStripMenuItemSubIdxClick1(object sender, EventArgs e)
        {
            if (ContinueNewOrExit())
            {
                openFileDialog1.Title = _language.OpenVobSubFile;
                openFileDialog1.FileName = string.Empty;
                openFileDialog1.Filter = _language.VobSubFiles + "|*.sub";
                if (openFileDialog1.ShowDialog(this) == DialogResult.OK)
                {
                    ImportAndOcrVobSubSubtitleNew(openFileDialog1.FileName);
                }
            }
        }

        private void SubtitleListview1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (Configuration.Settings.General.ListViewDoubleClickAction == 1)
            {
                GotoSubPositionAndPause();
            }
            else if (Configuration.Settings.General.ListViewDoubleClickAction == 2)
            {
                if (AutoRepeatContinueOn)
                    PlayCurrent();
                else
                    buttonBeforeText_Click(null, null);
            }
            else if (Configuration.Settings.General.ListViewDoubleClickAction == 3)
            {
                textBoxListViewText.Focus();
            }
        }

        private void AddWordToNamesetcListToolStripMenuItemClick(object sender, EventArgs e)
        {
            var addToNamesList = new AddToNamesList();
            _formPositionsAndSizes.SetPositionAndSize(addToNamesList);
            addToNamesList.Initialize(_subtitle, textBoxListViewText.SelectedText);
            if (addToNamesList.ShowDialog(this) == DialogResult.OK)
                ShowStatus(string.Format(_language.NameXAddedToNamesEtcList, addToNamesList.NewName));
            else if (!string.IsNullOrEmpty(addToNamesList.NewName))
                ShowStatus(string.Format(_language.NameXNotAddedToNamesEtcList, addToNamesList.NewName));
            _formPositionsAndSizes.SavePositionAndSize(addToNamesList);
        }

        private void EditToolStripMenuItemDropDownOpening(object sender, EventArgs e)
        {
            if (GetCurrentEncoding() == Encoding.Default || _subtitleListViewIndex == -1)
            {
                toolStripMenuItemInsertUnicodeCharacter.Visible = false;
                toolStripSeparatorInsertUnicodeCharacter.Visible = false;
            }
            else
            {
                toolStripMenuItemInsertUnicodeCharacter.Visible = true;
                toolStripSeparatorInsertUnicodeCharacter.Visible = true;                
            }

            if (SubtitleListview1.IsColumTextAlternateActive)
            {
                toolStripMenuItemTranslationMode.ShortcutKeys = Keys.None;
                toolStripMenuItemTranslationMode.ShortcutKeyDisplayString = string.Empty;
                toolStripMenuItemTranslationMode.Text = _language.Menu.Edit.HideOriginalText;
            }
            else
            {
                toolStripMenuItemTranslationMode.ShortcutKeys = Keys.Control & Keys.Alt & Keys.O;
                toolStripMenuItemTranslationMode.ShortcutKeyDisplayString = "Ctrl+Alt+O";
                toolStripMenuItemTranslationMode.Text = _language.Menu.Edit.ShowOriginalText;
            }
        }

        private void InsertUnicodeSymbol(object sender, EventArgs e)
        {
            if (tabControlSubtitle.SelectedIndex == TabControlSourceView)
            {

                textBoxSource.Text = textBoxSource.Text.Insert(textBoxSource.SelectionStart, (sender as ToolStripMenuItem).Text);
            }
            else
            {
                textBoxListViewText.Text = textBoxListViewText.Text.Insert(textBoxListViewText.SelectionStart, (sender as ToolStripMenuItem).Text);
            }
        }

        private void ToolStripMenuItemAutoMergeShortLinesClick(object sender, EventArgs e)
        {
            if (_subtitle != null && _subtitle.Paragraphs.Count > 1)
            {
                ReloadFromSourceView();
                var formMergeShortLines = new MergeShortLines();
                _formPositionsAndSizes.SetPositionAndSize(formMergeShortLines);
                formMergeShortLines.Initialize(_subtitle);
                if (formMergeShortLines.ShowDialog(this) == DialogResult.OK)
                {
                    MakeHistoryForUndo(_language.BeforeMergeShortLines);
                    _subtitle = formMergeShortLines.MergedSubtitle;
                    ShowStatus(string.Format(_language.MergedShortLinesX, formMergeShortLines.NumberOfMerges));
                    SaveSubtitleListviewIndexes();
                    ShowSource();
                    SubtitleListview1.Fill(_subtitle, _subtitleAlternate);
                    RestoreSubtitleListviewIndexes();
                    _change = true;
                }
                _formPositionsAndSizes.SavePositionAndSize(formMergeShortLines);
            }
            else
            {
                MessageBox.Show(_language.NoSubtitleLoaded, Title, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void setMinimalDisplayTimeDifferenceToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SetMinimumDisplayTimeBetweenParagraphs setMinDisplayDiff = new SetMinimumDisplayTimeBetweenParagraphs();
            _formPositionsAndSizes.SetPositionAndSize(setMinDisplayDiff);
            setMinDisplayDiff.Initialize(_subtitle);
            if (setMinDisplayDiff.ShowDialog() == System.Windows.Forms.DialogResult.OK && setMinDisplayDiff.FixCount > 0)
            {
                MakeHistoryForUndo(_language.BeforeSetMinimumDisplayTimeBetweenParagraphs);                
                _subtitle = setMinDisplayDiff.FixedSubtitle;
                _subtitle.CalculateFrameNumbersFromTimeCodesNoCheck(CurrentFrameRate);
                ShowStatus(string.Format(_language.XMinimumDisplayTimeBetweenParagraphsChanged, setMinDisplayDiff.FixCount));
                SaveSubtitleListviewIndexes();
                ShowSource();
                SubtitleListview1.Fill(_subtitle, _subtitleAlternate);
                RestoreSubtitleListviewIndexes();
                _change = true;
            }
            _formPositionsAndSizes.SavePositionAndSize(setMinDisplayDiff);
        }

        private void toolStripMenuItemImportText_Click(object sender, EventArgs e)
        {
            if (ContinueNewOrExit())
            {
                ImportText importText = new ImportText();
                if (importText.ShowDialog(this) == System.Windows.Forms.DialogResult.OK)
                {
                    SyncPointsSync syncPointSync = new SyncPointsSync();
                    syncPointSync.Initialize(importText.FixedSubtitle, _fileName, importText.VideoFileName);
                    if (syncPointSync.ShowDialog(this) == System.Windows.Forms.DialogResult.OK)
                    {
                        ResetSubtitle();

                        _subtitleListViewIndex = -1;
                        MakeHistoryForUndo(_language.BeforeImportText); 
                        _subtitle = importText.FixedSubtitle;
                        _subtitle.CalculateFrameNumbersFromTimeCodesNoCheck(CurrentFrameRate);
                        ShowStatus(_language.TextImported);
                        ShowSource();
                        SubtitleListview1.Fill(_subtitle, _subtitleAlternate);
                        _change = true;
                    }
                    _videoFileName = syncPointSync.VideoFileName;
                }
            }
        }

        private void toolStripMenuItemPointSync_Click(object sender, EventArgs e)
        {
            SyncPointsSync pointSync = new SyncPointsSync();
            _formPositionsAndSizes.SetPositionAndSize(pointSync);

            pointSync.Initialize(_subtitle, _fileName,  _videoFileName);
            if (pointSync.ShowDialog(this) == DialogResult.OK)
            {
                _subtitleListViewIndex = -1;
                MakeHistoryForUndo(_language.BeforePointSynchronization);
                _subtitle = pointSync.FixedSubtitle;
                _subtitle.CalculateFrameNumbersFromTimeCodesNoCheck(CurrentFrameRate);
                ShowStatus(_language.PointSynchronizationDone);
                ShowSource();
                SubtitleListview1.Fill(_subtitle, _subtitleAlternate);
                _change = true;              
            }
            _videoFileName = pointSync.VideoFileName;
            _formPositionsAndSizes.SavePositionAndSize(pointSync);
        }

        private void toolStripMenuItemImportTimeCodes_Click(object sender, EventArgs e)
        {
            if (_subtitle.Paragraphs.Count < 1)
            {
                MessageBox.Show(_language.NoSubtitleLoaded, Title, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            openFileDialog1.Title = _languageGeneral.OpenSubtitle;
            openFileDialog1.FileName = string.Empty;
            openFileDialog1.Filter = Utilities.GetOpenDialogFiler();
            if (openFileDialog1.ShowDialog(this) == DialogResult.OK)
            {
                Encoding encoding = null;
                Subtitle timeCodeSubtitle = new Subtitle();
                SubtitleFormat format = timeCodeSubtitle.LoadSubtitle(openFileDialog1.FileName, out encoding, encoding);
                if (format == null)
                {
                    ShowUnknownSubtitle();
                    return;
                }

                MakeHistoryForUndo(_language.BeforeTimeCodeImport);

                if (GetCurrentSubtitleFormat().IsFrameBased)
                    timeCodeSubtitle.CalculateTimeCodesFromFrameNumbers(CurrentFrameRate);
                else
                    timeCodeSubtitle.CalculateFrameNumbersFromTimeCodes(CurrentFrameRate);

                int count = 0;
                for (int i = 0; i < timeCodeSubtitle.Paragraphs.Count; i++)
                {
                    Paragraph existing = _subtitle.GetParagraphOrDefault(i);
                    Paragraph newTimeCode = timeCodeSubtitle.GetParagraphOrDefault(i);
                    if (existing == null || newTimeCode == null)
                        break;
                    existing.StartTime.TotalMilliseconds = newTimeCode.StartTime.TotalMilliseconds;
                    existing.EndTime.TotalMilliseconds = newTimeCode.EndTime.TotalMilliseconds;
                    existing.StartFrame = newTimeCode.StartFrame;
                    existing.EndFrame = newTimeCode.EndFrame;
                    count++;

                }
                ShowStatus(string.Format(_language.TimeCodeImportedFromXY, Path.GetFileName(openFileDialog1.FileName), count));
                SaveSubtitleListviewIndexes();
                ShowSource();
                SubtitleListview1.Fill(_subtitle, _subtitleAlternate);
                RestoreSubtitleListviewIndexes();
                _change = true;
            }

        }

        private void addWordsFromWordlistToNamesetcToolStripMenuItem_Click(object sender, EventArgs e)
        {
            openFileDialog1.Title = "Open names_etc.xml file to import into en_US_names_etc.xml";
            openFileDialog1.FileName = string.Empty;
            openFileDialog1.Filter = "Xml files|*.xml";
            if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                int wordsAdded = 0;
                XmlDocument doc = new XmlDocument();
                doc.Load(openFileDialog1.FileName);

                string language = "en_US";

                var namesEtc = new List<string>();
                Utilities.LoadGlobalNamesEtc(namesEtc, namesEtc);

                var localNamesEtc = new List<string>();
                string userNamesEtcXmlFileName = Utilities.LoadLocalNamesEtc(localNamesEtc, localNamesEtc, language);
                localNamesEtc.Sort();

                foreach (XmlNode node in doc.DocumentElement.SelectNodes("name"))
                {

                    string word = node.InnerText.Trim();
                    if (!localNamesEtc.Contains(word) && !namesEtc.Contains(word))
                    {
                        wordsAdded++;
                        localNamesEtc.Add(word);
                        localNamesEtc.Sort();
                    }
                }

                if (MessageBox.Show("Add new words: " + wordsAdded.ToString(), "", MessageBoxButtons.OKCancel) == System.Windows.Forms.DialogResult.OK) 
                {
                    var namesEtcDoc = new XmlDocument();
                    if (File.Exists(userNamesEtcXmlFileName))
                        namesEtcDoc.Load(userNamesEtcXmlFileName);
                    else
                        namesEtcDoc.LoadXml("<ignore_words />");

                    XmlNode de = namesEtcDoc.DocumentElement;
                    if (de != null)
                    {
                        de.RemoveAll();
                        foreach (var name in localNamesEtc)
                        {
                            XmlNode node = namesEtcDoc.CreateElement("name");
                            node.InnerText = name;
                            de.AppendChild(node);
                        }
                        namesEtcDoc.Save(userNamesEtcXmlFileName);
                    }

                }
            }
        }

        private void addWordsFromWordlistTonamesetcToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            openFileDialog1.Title = "Open names_etc.xml file to import into names_etc.xml";
            openFileDialog1.FileName = string.Empty;
            openFileDialog1.Filter = "Xml files|*.xml";
            if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                int wordsAdded = 0;
                XmlDocument doc = new XmlDocument();
                doc.Load(openFileDialog1.FileName);

                string language = "en_US";

                string globalNamesEtcFileName = Utilities.DictionaryFolder + "names_etc.xml";

                var namesEtc = new List<string>();
                Utilities.LoadGlobalNamesEtc(namesEtc, namesEtc);
                namesEtc.Sort();

                var localNamesEtc = new List<string>();
                string userNamesEtcXmlFileName = Utilities.LoadLocalNamesEtc(localNamesEtc, localNamesEtc, language);

                foreach (XmlNode node in doc.DocumentElement.SelectNodes("name"))
                {

                    string word = node.InnerText.Trim();
                    if (!localNamesEtc.Contains(word) && !namesEtc.Contains(word))
                    {
                        wordsAdded++;
                        namesEtc.Add(word);
                    }
                }

                if (MessageBox.Show("Add new words: " + wordsAdded.ToString(), "", MessageBoxButtons.OKCancel) == System.Windows.Forms.DialogResult.OK) 
                {
                    namesEtc.Sort();
                    var namesEtcDoc = new XmlDocument();
                    if (File.Exists(globalNamesEtcFileName))
                        namesEtcDoc.Load(globalNamesEtcFileName);
                    else
                        namesEtcDoc.LoadXml("<ignore_words />");

                    XmlNode de = namesEtcDoc.DocumentElement;
                    if (de != null)
                    {
                        de.RemoveAll();
                        foreach (var name in namesEtc)
                        {
                            XmlNode node = namesEtcDoc.CreateElement("name");
                            node.InnerText = name;
                            de.AppendChild(node);
                        }
                        namesEtcDoc.Save(globalNamesEtcFileName);
                    }

                }
            }
        }

        private void toolStripMenuItemTranslationMode_Click(object sender, EventArgs e)
        {
            if (SubtitleListview1.IsColumTextAlternateActive)
            {
                SubtitleListview1.RemoveAlternateTextColumn();
                _subtitleAlternate = new Subtitle();
            }
            else
            {
                OpenAlternateSubtitle();
            }
        }

        private void OpenAlternateSubtitle()
        {
            openFileDialog1.Title = Configuration.Settings.Language.General.OpenOriginalSubtitleFile;
            openFileDialog1.FileName = string.Empty;
            openFileDialog1.Filter = Utilities.GetOpenDialogFiler();
            if (!(openFileDialog1.ShowDialog(this) == DialogResult.OK))
                return;

            string fileName = openFileDialog1.FileName;
            if (!File.Exists(fileName))
                return;

            if (Path.GetExtension(fileName).ToLower() == ".sub" && IsVobSubFile(fileName, false))
                return;

            var fi = new FileInfo(fileName);
            if (fi.Length > 1024 * 1024 * 10) // max 10 mb
            {
                if (MessageBox.Show(string.Format(_language.FileXIsLargerThan10Mb + Environment.NewLine +
                                                    Environment.NewLine +
                                                    _language.ContinueAnyway,
                                                    fileName), Title, MessageBoxButtons.YesNoCancel) != DialogResult.Yes)
                    return;
            }


            Encoding encoding;
            _subtitleAlternate = new Subtitle();
            SubtitleFormat format = _subtitleAlternate.LoadSubtitle(fileName, out encoding, null);
            if (format == null)
                return;

            if (format.IsFrameBased)
                _subtitleAlternate.CalculateTimeCodesFromFrameNumbers(CurrentFrameRate);
            else
                _subtitleAlternate.CalculateFrameNumbersFromTimeCodes(CurrentFrameRate);


            SubtitleListview1.AddAlternateTextColumn(Configuration.Settings.Language.General.OriginalText);
            SubtitleListview1.Fill(_subtitle, _subtitleAlternate);
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


        private void OpenVideo(string fileName)
        {            
            if (File.Exists(fileName))
            {
                FileInfo fi = new FileInfo(fileName);
                if (fi.Length < 1000)
                    return;

                VideoFileName = fileName;
                if (mediaPlayer.VideoPlayer != null)
                {
                    mediaPlayer.Pause();
                    mediaPlayer.VideoPlayer.DisposeVideoPlayer();
                }

                VideoInfo videoInfo = ShowVideoInfo(fileName);
                toolStripComboBoxFrameRate.Text = videoInfo.FramesPerSecond.ToString();

                Utilities.InitializeVideoPlayerAndContainer(fileName, videoInfo, mediaPlayer, VideoLoaded, VideoEnded);
                labelVideoInfo.Text = Path.GetFileName(fileName) + " " + videoInfo.Width + "x" + videoInfo.Height + " " + videoInfo.VideoCodec;
                                
                string peakWaveFileName = GetPeakWaveFileName(fileName);
                if (File.Exists(peakWaveFileName))
                {
                    AudioWaveForm.WavePeaks = new WavePeakGenerator(peakWaveFileName);
                    AudioWaveForm.WavePeaks.GenerateAllSamples();
                    AudioWaveForm.WavePeaks.Close();

                    Paragraph p = new Paragraph();
                    if (_subtitle.Paragraphs.Count > 0)
                        p = _subtitle.Paragraphs[0];
                    AudioWaveForm.SetPosition(0, _subtitle, 0, 0);
                    timerWaveForm.Start();
                }
            }
        }

        void VideoLoaded(object sender, EventArgs e)
        {
            mediaPlayer.Stop();
            timer1.Start();

            trackBarWaveFormPosition.Maximum = (int)mediaPlayer.Duration;
        }

        void VideoEnded(object sender, EventArgs e)
        {
            mediaPlayer.Pause();
        }

        private VideoInfo ShowVideoInfo(string fileName)
        {
            _videoInfo = Utilities.GetVideoInfo(fileName, delegate { Application.DoEvents(); });
            var info = new FileInfo(fileName);
            long fileSizeInBytes = info.Length;

            //labelVideoInfo.Text = string.Format(_languageGeneral.FileNameXAndSize, fileName, Utilities.FormatBytesToDisplayFileSize(fileSizeInBytes)) + Environment.NewLine +
            //                      string.Format(_languageGeneral.ResolutionX, +_videoInfo.Width + "x" + _videoInfo.Height) + "    ";
            //if (_videoInfo.FramesPerSecond > 5 && _videoInfo.FramesPerSecond < 200)
            //    labelVideoInfo.Text += string.Format(_languageGeneral.FrameRateX + "        ", _videoInfo.FramesPerSecond);
            //if (_videoInfo.TotalFrames > 10)
            //    labelVideoInfo.Text += string.Format(_languageGeneral.TotalFramesX + "         ", (int)_videoInfo.TotalFrames);
            //if (!string.IsNullOrEmpty(_videoInfo.VideoCodec))
            //    labelVideoInfo.Text += string.Format(_languageGeneral.VideoEncodingX, _videoInfo.VideoCodec) + "        ";

            //TimeSpan span = TimeSpan.FromMilliseconds(_videoInfo.TotalMilliseconds);
            //_totalPositionString = " / " + string.Format("{0:00}:{1:00}:{2:00},{3:000}", span.Hours, span.Minutes, span.Seconds, span.Milliseconds);

            return _videoInfo;
        }

        private void TryToFindAndOpenVideoFile(string fileNameNoExtension)
        {
            string movieFileName = null;

            foreach (string extension in Utilities.GetMovieFileExtensions())
            {
                movieFileName = fileNameNoExtension + extension;
                if (File.Exists(movieFileName))
                    break;
            }

            if (movieFileName != null && File.Exists(movieFileName))
            {
                OpenVideo(movieFileName);
            }
            else if (fileNameNoExtension.Contains("."))
            {
                fileNameNoExtension = fileNameNoExtension.Substring(0, fileNameNoExtension.LastIndexOf("."));
                TryToFindAndOpenVideoFile(fileNameNoExtension);
            }
        }

        private void GoBackSeconds(double seconds,
                                   Label labelGoBackSubtitleStatus,
                                   VideoPlayer mediaPlayer)
        {
            if (mediaPlayer != null)
            {
                if (mediaPlayer.CurrentPosition > seconds)
                    mediaPlayer.CurrentPosition -= seconds;
                else
                    mediaPlayer.CurrentPosition = 0;
                Utilities.ShowSubtitle(_subtitle.Paragraphs, labelGoBackSubtitleStatus, mediaPlayer);
                //                ShowPosition(labelPosition, mediaPlayer);
            }
        }

        private void ButtonStartHalfASecondBackClick(object sender, EventArgs e)
        {
            GoBackSeconds(0.5, labelSubtitle, mediaPlayer.VideoPlayer);
        }

        private void ButtonStartThreeSecondsBackClick(object sender, EventArgs e)
        {
            GoBackSeconds(3.0, labelSubtitle, mediaPlayer.VideoPlayer);
        }

        private void ButtonStartOneMinuteBackClick(object sender, EventArgs e)
        {
            GoBackSeconds(60, labelSubtitle, mediaPlayer.VideoPlayer);
        }

        private void ButtonStartHalfASecondAheadClick(object sender, EventArgs e)
        {
            GoBackSeconds(-0.5, labelSubtitle, mediaPlayer.VideoPlayer);
        }

        private void ButtonStartThreeSecondsAheadClick(object sender, EventArgs e)
        {
            GoBackSeconds(-3, labelSubtitle, mediaPlayer.VideoPlayer);
        }

        private void ButtonStartOneMinuteAheadClick(object sender, EventArgs e)
        {
            GoBackSeconds(-60, labelSubtitle, mediaPlayer.VideoPlayer);
        }

        private void videoTimer_Tick(object sender, EventArgs e)
        {
            if (mediaPlayer != null)
            {
                if (!mediaPlayer.IsPaused)
                {
                    mediaPlayer.RefreshProgressBar();
                    Utilities.ShowSubtitle(_subtitle.Paragraphs, labelSubtitle, mediaPlayer.VideoPlayer);
                }
            }
        }

        private void videoModeHiddenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            HideVideoPlayer();
        }

        private void createadjustLinesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ShowVideoPlayer();
        }

        private void HideVideoPlayer()
        {
            if (mediaPlayer != null)
                mediaPlayer.Pause();

            groupBoxVideo.Visible = false;
            tabControlSubtitle.Height = this.Height - (tabControlSubtitle.Top + statusStrip1.Height + 38);
        }

        private void ShowVideoPlayer()
        {            
            groupBoxVideo.Visible = true;
            groupBoxVideo.Top = statusStrip1.Top - (groupBoxVideo.Height + 5);
            tabControlSubtitle.Height = groupBoxVideo.Top - (tabControlSubtitle.Top);
            labelVideoInfo.TextAlign = ContentAlignment.TopRight;
            labelVideoInfo.Left = groupBoxVideo.Width - (labelVideoInfo.Width + 10);
            if (toolStripButtonToogleVideo.Checked)
            {
                if (AudioWaveForm.Visible)
                {
                    AudioWaveForm.Left = tabControlButtons.Left + tabControlButtons.Width + 5;
                    panelVideoPlayer.Width = 360;
                    panelVideoPlayer.Left = this.Width - (360 + 34);
                    AudioWaveForm.Width = panelVideoPlayer.Left - (AudioWaveForm.Left + 5);
                }
                else
                {
                    panelVideoPlayer.Left = tabControlButtons.Left + tabControlButtons.Width + 5;
                    int w = this.Width - (panelVideoPlayer.Left + 34);
                    if (w > 500)
                        w = 500;
                    panelVideoPlayer.Width = w; // this.Width - (panelVideoPlayer.Left + 34);

                    labelVideoInfo.TextAlign = ContentAlignment.TopLeft;
                    labelVideoInfo.Left = checkBoxSyncListViewWithVideWhilePlaying.Left + checkBoxSyncListViewWithVideWhilePlaying.Width + 10;
                }
            }
            else if (AudioWaveForm.Visible)
            {
                AudioWaveForm.Left = tabControlButtons.Left + tabControlButtons.Width + 5;
                AudioWaveForm.Width = this.Width - (AudioWaveForm.Left + 34);
            }
            checkBoxSyncListViewWithVideWhilePlaying.Left = tabControlButtons.Left + tabControlButtons.Width + 5;
            panelWaveFormControls.Left = AudioWaveForm.Left;
            trackBarWaveFormPosition.Left = panelWaveFormControls.Left + panelWaveFormControls.Width + 5;
            trackBarWaveFormPosition.Width = AudioWaveForm.Left + AudioWaveForm.Width - trackBarWaveFormPosition.Left + 5;

            if (mediaPlayer.VideoPlayer == null && !string.IsNullOrEmpty(_fileName))
                TryToFindAndOpenVideoFile(Path.Combine(Path.GetDirectoryName(_fileName), Path.GetFileNameWithoutExtension(_fileName)));
        }

        private void Main_Resize(object sender, EventArgs e)
        {
            tabControlSubtitle.Width = this.Width - tabControlSubtitle.Left - 22;
            if (groupBoxVideo.Visible)
                ShowVideoPlayer();
            else
                HideVideoPlayer();
            AudioWaveForm.Invalidate();
        }

        private void PlayCurrent()
        {
            if (_subtitleListViewIndex >= 0)
            {
                GotoSubtitleIndex(_subtitleListViewIndex);
                textBoxListViewText.Focus();
                ReadyAutoRepeat();
                PlayPart(_subtitle.Paragraphs[_subtitleListViewIndex]);
            }
        }

        private void ReadyAutoRepeat()
        {
            if (checkBoxAutoRepeatOn.Checked)
            {
                _repeatCount = int.Parse(comboBoxAutoRepeat.Text);
            }
            else
            {
                _repeatCount = -1;
            }
            labelStatus.Text = _language.VideoControls.Playing;
        }

        private void Next()
        {
            int newIndex = _subtitleListViewIndex + 1;
            if (newIndex < _subtitle.Paragraphs.Count)
            {
                foreach (ListViewItem item in SubtitleListview1.SelectedItems)
                    item.Selected = false;
                SubtitleListview1.Items[newIndex].Selected = true;
                SubtitleListview1.Items[newIndex].EnsureVisible();
                textBoxListViewText.Focus();
                textBoxListViewText.SelectAll();
                _subtitleListViewIndex = newIndex;
                GotoSubtitleIndex(newIndex);
                Utilities.ShowSubtitle(_subtitle.Paragraphs, labelSubtitle, mediaPlayer.VideoPlayer);
                PlayCurrent();
            }
        }

        private void PlayPrevious()
        {
            if (_subtitleListViewIndex > 0)
            {
                int newIndex = _subtitleListViewIndex - 1;
                foreach (ListViewItem item in SubtitleListview1.SelectedItems)
                    item.Selected = false;
                SubtitleListview1.Items[newIndex].Selected = true;
                SubtitleListview1.Items[newIndex].EnsureVisible();
                textBoxListViewText.Focus();
                textBoxListViewText.SelectAll();
                GotoSubtitleIndex(newIndex);
                Utilities.ShowSubtitle(_subtitle.Paragraphs, labelSubtitle, mediaPlayer.VideoPlayer);
                _subtitleListViewIndex = newIndex;
                PlayCurrent();
            }
        }

        private void GotoSubtitleIndex(int index)
        {
            if (mediaPlayer != null && mediaPlayer.VideoPlayer != null && mediaPlayer.Duration > 0)
            {
                mediaPlayer.CurrentPosition = _subtitle.Paragraphs[index].StartTime.TotalSeconds;
            }
        }

        private void buttonPlayCurrent_Click(object sender, EventArgs e)
        {
            PlayCurrent();
        }

        private void buttonPlayPrevious_Click(object sender, EventArgs e)
        {
            PlayPrevious();
        }

        private void PlayPart(Paragraph paragraph)
        {
            if (mediaPlayer != null && mediaPlayer.VideoPlayer != null)
            {
                double startSeconds = paragraph.StartTime.TotalSeconds;
                if (startSeconds > 0.2)
                    startSeconds -= 0.2; // go a little back

                _endSeconds = paragraph.EndTime.TotalSeconds;
                if (mediaPlayer.Duration > _endSeconds + 0.2)
                    _endSeconds += 0.2; // go a little forward

                mediaPlayer.CurrentPosition = startSeconds;
                Utilities.ShowSubtitle(_subtitle.Paragraphs, labelSubtitle, mediaPlayer.VideoPlayer);
                mediaPlayer.Play();
            }
        }

        private void buttonSetStartTime_Click(object sender, EventArgs e)
        {
            if (SubtitleListview1.SelectedItems.Count == 1)
            {
                timeUpDownStartTime.MaskedTextBox.TextChanged -= MaskedTextBox_TextChanged;
                int index = SubtitleListview1.SelectedItems[0].Index;
                double videoPosition = mediaPlayer.CurrentPosition;

                timeUpDownStartTime.TimeCode = new TimeCode(TimeSpan.FromSeconds(videoPosition));

                var duration = _subtitle.Paragraphs[index].Duration.TotalMilliseconds;

                _subtitle.Paragraphs[index].StartTime.TotalMilliseconds = TimeSpan.FromSeconds(videoPosition).TotalMilliseconds;
                _subtitle.Paragraphs[index].EndTime.TotalMilliseconds = _subtitle.Paragraphs[index].StartTime.TotalMilliseconds + duration;
                SubtitleListview1.SetStartTime(index, _subtitle.Paragraphs[index]);
                SubtitleListview1.SetDuration(index, _subtitle.Paragraphs[index]);
                timeUpDownStartTime.TimeCode = _subtitle.Paragraphs[index].StartTime;
                timeUpDownStartTime.MaskedTextBox.TextChanged += MaskedTextBox_TextChanged;

            }
        }

        private void buttonSetEndTime_Click(object sender, EventArgs e)
        {
            if (SubtitleListview1.SelectedItems.Count == 1)
            {
                int index = SubtitleListview1.SelectedItems[0].Index;
                double videoPosition = mediaPlayer.CurrentPosition;

                _subtitle.Paragraphs[index].EndTime = new TimeCode(TimeSpan.FromSeconds(videoPosition));
                SubtitleListview1.SetStartTime(index, _subtitle.Paragraphs[index]);
                SubtitleListview1.SetDuration(index, _subtitle.Paragraphs[index]);

                if (index + 1 < _subtitle.Paragraphs.Count)
                {
                    SubtitleListview1.SelectedItems[0].Selected = false;
                    SubtitleListview1.Items[index + 1].Selected = true;
                    _subtitle.Paragraphs[index + 1].StartTime = new TimeCode(TimeSpan.FromSeconds(videoPosition));
                    SubtitleListview1.AutoScrollOffset.Offset(0, index * 16);
                    SubtitleListview1.EnsureVisible(Math.Min(SubtitleListview1.Items.Count - 1, index + 5));
                }
                else
                {
                    numericUpDownDuration.Value = (decimal)(_subtitle.Paragraphs[index].Duration.TotalSeconds);
                }
            }
        }

        private void buttonSetStartAndOffsetRest_Click(object sender, EventArgs e)
        {
            if (SubtitleListview1.SelectedItems.Count == 1)
            {
                int index = SubtitleListview1.SelectedItems[0].Index;
                double videoPosition = mediaPlayer.CurrentPosition;
                var tc = new TimeCode(TimeSpan.FromSeconds(videoPosition));
                double offset = _subtitle.Paragraphs[index].StartTime.TotalMilliseconds - tc.TotalMilliseconds;
                for (int i = index; i < SubtitleListview1.Items.Count; i++)
                {
                    _subtitle.Paragraphs[i].StartTime = new TimeCode(TimeSpan.FromMilliseconds(_subtitle.Paragraphs[i].StartTime.TotalMilliseconds - offset));
                    _subtitle.Paragraphs[i].EndTime = new TimeCode(TimeSpan.FromMilliseconds(_subtitle.Paragraphs[i].EndTime.TotalMilliseconds - offset));
                    SubtitleListview1.SetStartTime(i, _subtitle.Paragraphs[i]);
                }
            }
        }

        private void buttonSetEnd_Click(object sender, EventArgs e)
        {
            if (SubtitleListview1.SelectedItems.Count == 1)
            {
                int index = SubtitleListview1.SelectedItems[0].Index;
                double videoPosition = mediaPlayer.CurrentPosition;

                _subtitle.Paragraphs[index].EndTime = new TimeCode(TimeSpan.FromSeconds(videoPosition));
                SubtitleListview1.SetStartTime(index, _subtitle.Paragraphs[index]);
                SubtitleListview1.SetDuration(index, _subtitle.Paragraphs[index]);

                numericUpDownDuration.Value = (decimal)(_subtitle.Paragraphs[index].Duration.TotalSeconds);
            }
        }

        private void buttonInsertNewText_Click(object sender, EventArgs e)
        {
            mediaPlayer.Pause();

            int startNumber = 1;
            if (_subtitle.Paragraphs.Count > 0)
                startNumber = _subtitle.Paragraphs[0].Number;

            // current movie pos
            double totalMilliseconds = mediaPlayer.CurrentPosition * 1000.0;

            TimeCode tc = new TimeCode(TimeSpan.FromMilliseconds(totalMilliseconds));
            MakeHistoryForUndo(_language.BeforeInsertSubtitleAtVideoPosition + "  " + tc.ToString());


            // find index where to insert
            int index = 0;
            foreach (Paragraph p in _subtitle.Paragraphs)
            {
                if (p.StartTime.TotalMilliseconds > totalMilliseconds)
                    break;
                index++;
            }

            // create and insert
            var newPararaph = new Paragraph("", totalMilliseconds, totalMilliseconds + 2000);
            _subtitle.Paragraphs.Insert(index, newPararaph);

            _subtitleListViewIndex = -1;
            _subtitle.Renumber(startNumber);
            SubtitleListview1.Fill(_subtitle.Paragraphs);
            SubtitleListview1.SelectIndexAndEnsureVisible(index);

            textBoxListViewText.Focus();
            timerAutoDuration.Start();
            
            ShowStatus(string.Format(_language.VideoControls.NewTextInsertAtX,  newPararaph.StartTime.ToShortString()));
        }

        private void timerAutoDuration_Tick(object sender, EventArgs e)
        {
            labelAutoDuration.Visible = !labelAutoDuration.Visible;

            double duration = Utilities.GetDisplayMillisecondsFromText(textBoxListViewText.Text) * 1.4;
            numericUpDownDuration.Value = (decimal)(duration / 1000.0);

            // update _subtitle + listview
            if (SubtitleListview1.SelectedItems.Count > 0)
            {
                int firstSelectedIndex = SubtitleListview1.SelectedItems[0].Index;
                Paragraph currentParagraph = _subtitle.GetParagraphOrDefault(firstSelectedIndex);
                currentParagraph.EndTime.TotalMilliseconds = currentParagraph.StartTime.TotalMilliseconds + duration;
                SubtitleListview1.SetDuration(firstSelectedIndex, currentParagraph);
            }
        }

        private void buttonBeforeText_Click(object sender, EventArgs e)
        {
            if (SubtitleListview1.SelectedItems.Count > 0)
            {
                int index = SubtitleListview1.SelectedItems[0].Index;

                mediaPlayer.Pause();
                double pos = _subtitle.Paragraphs[index].StartTime.TotalSeconds;
                if (pos > 1)
                    mediaPlayer.CurrentPosition = (_subtitle.Paragraphs[index].StartTime.TotalSeconds) - 0.5;
                else
                    mediaPlayer.CurrentPosition = _subtitle.Paragraphs[index].StartTime.TotalSeconds;
                mediaPlayer.Play();
            }

        }


        private void GotoSubPositionAndPause()
        {
            if (SubtitleListview1.SelectedItems.Count > 0)
            {
                int index = SubtitleListview1.SelectedItems[0].Index;

                mediaPlayer.Pause();
                mediaPlayer.CurrentPosition = _subtitle.Paragraphs[index].StartTime.TotalSeconds;
                Utilities.ShowSubtitle(_subtitle.Paragraphs, labelSubtitle, mediaPlayer.VideoPlayer);

                double startPos = mediaPlayer.CurrentPosition - 1;
                if (startPos < 0)
                    startPos = 0;

                AudioWaveForm.SetPosition(startPos, _subtitle, mediaPlayer.CurrentPosition, index);
            }
        }

        private void buttonGotoSub_Click(object sender, EventArgs e)
        {
            GotoSubPositionAndPause();
        }

        private void buttonStop_Click(object sender, EventArgs e)
        {
            timerAutoContinue.Stop();

            if (mediaPlayer != null)
                mediaPlayer.Pause();

            labelStatus.Text = string.Empty;
        }

        private void buttonPlayNext_Click(object sender, EventArgs e)
        {
            Next();
        }

        private void buttonOpenVideo_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(openFileDialog1.InitialDirectory) && !string.IsNullOrEmpty(_fileName))
                openFileDialog1.InitialDirectory = Path.GetDirectoryName(_fileName);
            openFileDialog1.Title = Configuration.Settings.Language.General.OpenVideoFile;
            openFileDialog1.FileName = string.Empty;
            openFileDialog1.Filter = Utilities.GetVideoFileFilter();
            openFileDialog1.FileName = string.Empty;
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                openFileDialog1.InitialDirectory = Path.GetDirectoryName(openFileDialog1.FileName);
                OpenVideo(openFileDialog1.FileName);
            }
        }

        private void toolStripButtonToogleVideo_Click(object sender, EventArgs e)
        {
            toolStripButtonToogleVideo.Checked = !toolStripButtonToogleVideo.Checked;
            panelVideoPlayer.Visible = toolStripButtonToogleVideo.Checked;
            mediaPlayer.BringToFront();
            labelSubtitle.BringToFront();
            if (!toolStripButtonToogleVideo.Checked && !toolStripButtonToogleWaveForm.Checked)
            {
                HideVideoPlayer();
            }
            else
            {
                ShowVideoPlayer();
            }
            Configuration.Settings.General.ShowVideoPlayer = toolStripButtonToogleVideo.Checked;
            Refresh();
        }

        private void toolStripButtonToogleWaveForm_Click(object sender, EventArgs e)
        {
            toolStripButtonToogleWaveForm.Checked = !toolStripButtonToogleWaveForm.Checked;
            AudioWaveForm.Visible = toolStripButtonToogleWaveForm.Checked;
            trackBarWaveFormPosition.Visible = toolStripButtonToogleWaveForm.Checked;
            panelWaveFormControls.Visible = toolStripButtonToogleWaveForm.Checked;
            if (!toolStripButtonToogleWaveForm.Checked && !toolStripButtonToogleVideo.Checked)
            {
                HideVideoPlayer();
            }
            else
            {
                ShowVideoPlayer();
            }
            Configuration.Settings.General.ShowWaveForm = toolStripButtonToogleWaveForm.Checked;
            Refresh();
        }

        private void toolStripMenuItemAdjustAllTimes_Click(object sender, EventArgs e)
        {
            if (_subtitle != null && _subtitle.Paragraphs.Count > 1)
            {
                mediaPlayer.Pause();
                var showEarlierOrLater = new ShowEarlierLater();
                SaveSubtitleListviewIndexes();

                showEarlierOrLater.Initialize(_subtitle, _videoFileName);

                if (showEarlierOrLater.ShowDialog(this) == DialogResult.OK)
                {
                    MakeHistoryForUndo(_language.BeforeShowSelectedLinesEarlierLater);

                    // we only update selected lines
                    int i = 0;
                    double frameRate = CurrentFrameRate;
                    for (int index = 0; i < _subtitle.Paragraphs.Count; index++)
                    {
                        _subtitle.Paragraphs[index] = new Paragraph(showEarlierOrLater.Paragraphs[i]);
                        if (_subtitle.WasLoadedWithFrameNumbers)
                            _subtitle.Paragraphs[index].CalculateFrameNumbersFromTimeCodes(frameRate);
                        i++;
                    }
                    ShowStatus(_language.ShowSelectedLinesEarlierLaterPerformed);
                    ShowSource();
                    SubtitleListview1.Fill(_subtitle, _subtitleAlternate);
                    RestoreSubtitleListviewIndexes();
                }
                showEarlierOrLater.Dispose();
            }
            else
            {
                MessageBox.Show(_language.NoSubtitleLoaded, Title, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (mediaPlayer != null && mediaPlayer.VideoPlayer != null)
            {
                if (!mediaPlayer.IsPaused)
                {
                    timeUpDownVideoPosition.Enabled = false;
                    timeUpDownVideoPositionAdjust.Enabled = false;

                    if (_endSeconds >= 0 && mediaPlayer.CurrentPosition > _endSeconds && !AutoRepeatContinueOn)
                    {
                        mediaPlayer.Pause();
                        _endSeconds = -1;
                    }

                    // mediaPlayer.RefreshProgressBar();
                    // Utilities.ShowSubtitle(_subtitle.Paragraphs, labelSubtitle, mediaPlayer.VideoPlayer);
                    if (AutoRepeatContinueOn && !checkBoxSyncListViewWithVideWhilePlaying.Checked)
                    {
                        if (_endSeconds >= 0 && mediaPlayer.CurrentPosition > _endSeconds && checkBoxAutoRepeatOn.Checked)
                        {
                            mediaPlayer.Pause();
                            _endSeconds = -1;

                            if (checkBoxAutoRepeatOn.Checked && _repeatCount > 0)
                            {
                                if (_repeatCount == 1)
                                    labelStatus.Text = _language.VideoControls.RepeatingLastTime;
                                else
                                    labelStatus.Text = string.Format(Configuration.Settings.Language.Main.VideoControls.RepeatingXTimesLeft, _repeatCount);

                                _repeatCount--;
                                PlayPart(_subtitle.Paragraphs[_subtitleListViewIndex]);
                            }
                            else if (checkBoxAutoContinue.Checked)
                            {
                                _autoContinueDelayCount = int.Parse(comboBoxAutoContinue.Text);
                                if (_repeatCount == 1)
                                    labelStatus.Text = _language.VideoControls.AutoContinueInOneSecond;
                                else
                                    labelStatus.Text = string.Format(Configuration.Settings.Language.Main.VideoControls.AutoContinueInXSeconds, _autoContinueDelayCount);
                                timerAutoContinue.Start();
                            }
                        }
                    }
                }
                else
                {
                    timeUpDownVideoPosition.Enabled = true;
                    timeUpDownVideoPositionAdjust.Enabled = true;
                }
                timeUpDownVideoPosition.TimeCode = new TimeCode(TimeSpan.FromMilliseconds(mediaPlayer.CurrentPosition * 1000.0));
                timeUpDownVideoPositionAdjust.TimeCode = new TimeCode(TimeSpan.FromMilliseconds(mediaPlayer.CurrentPosition * 1000.0));
                mediaPlayer.RefreshProgressBar();
                int index = Utilities.ShowSubtitle(_subtitle.Paragraphs, labelSubtitle, mediaPlayer.VideoPlayer);
                if (index != -1 && checkBoxSyncListViewWithVideWhilePlaying.Checked)
                {
                    if ((DateTime.Now.Ticks - _lastTextKeyDownTicks) > 10000 * 700) // only if last typed char was entered > 700 milliseconds 
                    {
                        SubtitleListview1.BeginUpdate();
                        if (index + 2 < SubtitleListview1.Items.Count)
                            SubtitleListview1.EnsureVisible(index + 2);
                        SubtitleListview1.SelectIndexAndEnsureVisible(index);
                        SubtitleListview1.EndUpdate();
                    }
                }

                trackBarWaveFormPosition.ValueChanged -= trackBarWaveFormPosition_ValueChanged;
                trackBarWaveFormPosition.Value = (int)mediaPlayer.CurrentPosition;
                trackBarWaveFormPosition.ValueChanged += trackBarWaveFormPosition_ValueChanged;
            }
        }

        private void StopAutoDuration()
        {
            timerAutoDuration.Stop();
            labelAutoDuration.Visible = false;
        }

        private void textBoxListViewText_Leave(object sender, EventArgs e)
        {
            StopAutoDuration();
        }

        private void timerAutoContinue_Tick(object sender, EventArgs e)
        {
            _autoContinueDelayCount--;

            if (_autoContinueDelayCount == 0)
            {
                timerAutoContinue.Stop();

                if (timerStillTyping.Enabled)
                {
                    labelStatus.Text = _language.VideoControls.StillTypingAutoContinueStopped;
                }
                else
                {
                    labelStatus.Text = string.Empty;
                    Next();
                }
            }
            else
            {
                if (_repeatCount == 1)
                    labelStatus.Text = _language.VideoControls.AutoContinueInOneSecond;
                else
                    labelStatus.Text = string.Format(Configuration.Settings.Language.Main.VideoControls.AutoContinueInXSeconds, _autoContinueDelayCount);
            }
        }

        private void timerStillTyping_Tick(object sender, EventArgs e)
        {
            timerStillTyping.Stop();
        }

        private void textBoxListViewText_MouseMove(object sender, MouseEventArgs e)
        {
            if (AutoRepeatContinueOn && !textBoxSearchWord.Focused)
            { 
                string selectedText = textBoxListViewText.SelectedText;
                if (!string.IsNullOrEmpty(selectedText))
                {
                    selectedText = selectedText.Trim();
                    selectedText = selectedText.TrimEnd('.');
                    selectedText = selectedText.TrimEnd(',');
                    selectedText = selectedText.TrimEnd('!');
                    selectedText = selectedText.TrimEnd('?');
                    selectedText = selectedText.Trim();
                    if (!string.IsNullOrEmpty(selectedText) && selectedText != textBoxSearchWord.Text)
                    {
                        textBoxSearchWord.Text = Utilities.RemoveHtmlTags(selectedText);
                    }
                }
            }
        }

        private void textBoxListViewText_KeyUp(object sender, KeyEventArgs e)
        {
            textBoxListViewText_MouseMove(sender, null);
        }

        private void buttonGoogleIt_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("http://www.google.dk/#q=" + HttpUtility.UrlEncode(textBoxSearchWord.Text));
        }

        private void buttonGoogleTranslateIt_Click(object sender, EventArgs e)
        {
            string languageId = Utilities.AutoDetectGoogleLanguage(_subtitle);
            System.Diagnostics.Process.Start("http://translate.google.com/#auto|" + languageId + "|" + HttpUtility.UrlEncode(textBoxSearchWord.Text));
        }

        private void ButtonPlayCurrentClick(object sender, EventArgs e)
        {
            PlayCurrent();
        }

        private void buttonPlayNext_Click_1(object sender, EventArgs e)
        {
            Next();
        }

        private void buttonPlayPrevious_Click_1(object sender, EventArgs e)
        {
            PlayPrevious();
        }

        private void buttonStop_Click_1(object sender, EventArgs e)
        {
            timerAutoContinue.Stop();
            mediaPlayer.Pause();
            labelStatus.Text = string.Empty;
        }

        private void fileToolStripMenuItem_DropDownOpening(object sender, EventArgs e)
        {
            toolStripMenuItemOpenContainingFolder.Visible = !string.IsNullOrEmpty(_fileName) && File.Exists(_fileName);
        }

        private void toolStripMenuItemOpenContainingFolder_Click(object sender, EventArgs e)
        {
            string folderName = Path.GetDirectoryName(_fileName);
            if (Utilities.IsRunningOnMono())
            {
                System.Diagnostics.Process.Start(folderName);
            }
            else 
            {
                string argument = @"/select, " + _fileName;
                System.Diagnostics.Process.Start("explorer.exe", argument);
            }
        }

        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {           
            if (tabControlButtons.SelectedIndex == 0)
            {
                tabControlButtons.Width = groupBoxTranslateSearch.Left + groupBoxTranslateSearch.Width + 10;
                Configuration.Settings.VideoControls.LastActiveTab = "Translate";
            }
            else if (tabControlButtons.SelectedIndex == 1)
            {
                tabControlButtons.Width = buttonInsertNewText.Left + buttonInsertNewText.Width + 35;
                Configuration.Settings.VideoControls.LastActiveTab = "Create";
            }
            else if (tabControlButtons.SelectedIndex == 2)
            {
                tabControlButtons.Width = buttonInsertNewText.Left + buttonInsertNewText.Width + 35;
                Configuration.Settings.VideoControls.LastActiveTab = "Adjust";
            }

            if (toolStripButtonToogleVideo.Checked && toolStripButtonToogleWaveForm.Checked )
            {
                AudioWaveForm.Left = tabControlButtons.Left + tabControlButtons.Width + 5;
                panelVideoPlayer.Left = AudioWaveForm.Left + AudioWaveForm.Width + 5;
            }
            else if (toolStripButtonToogleVideo.Checked)
            {
                panelVideoPlayer.Left = tabControlButtons.Left + tabControlButtons.Width + 5;
            }
            else
            {
                AudioWaveForm.Left = tabControlButtons.Left + tabControlButtons.Width + 5;                
            }
            panelWaveFormControls.Left = AudioWaveForm.Left;
            this.Main_Resize(null, null);


            labelVideoInfo.TextAlign = ContentAlignment.TopRight;
            labelVideoInfo.Left = groupBoxVideo.Width - (labelVideoInfo.Width + 10);
            if (toolStripButtonToogleVideo.Checked && !AudioWaveForm.Visible)
            {
                labelVideoInfo.TextAlign = ContentAlignment.TopLeft;
                labelVideoInfo.Left = checkBoxSyncListViewWithVideWhilePlaying.Left + checkBoxSyncListViewWithVideWhilePlaying.Width + 10;
            }            

            Refresh();
        }

        private void buttonSecBack1_Click(object sender, EventArgs e)
        {
            GoBackSeconds((double)numericUpDownSec1.Value , labelSubtitle, mediaPlayer.VideoPlayer);
        }

        private void buttonForward1_Click(object sender, EventArgs e)
        {
            GoBackSeconds(-(double)numericUpDownSec1.Value, labelSubtitle, mediaPlayer.VideoPlayer);
        }

        private void ButtonSetStartAndOffsetRestClick(object sender, EventArgs e)
        {
            if (SubtitleListview1.SelectedItems.Count == 1)
            {

                timeUpDownStartTime.MaskedTextBox.TextChanged -= MaskedTextBox_TextChanged;
                int index = SubtitleListview1.SelectedItems[0].Index;
                double videoPosition = mediaPlayer.CurrentPosition;
                var tc = new TimeCode(TimeSpan.FromSeconds(videoPosition));
                timeUpDownStartTime.TimeCode = tc;

                MakeHistoryForUndo(_language.BeforeSetStartTimeAndOffsetTheRest + "  " +_subtitle.Paragraphs[index].Number.ToString() + " - " + tc.ToString());


                double offset = _subtitle.Paragraphs[index].StartTime.TotalMilliseconds - tc.TotalMilliseconds;
                for (int i = index; i < SubtitleListview1.Items.Count; i++)
                {
                    _subtitle.Paragraphs[i].StartTime = new TimeCode(TimeSpan.FromMilliseconds(_subtitle.Paragraphs[i].StartTime.TotalMilliseconds - offset));
                    _subtitle.Paragraphs[i].EndTime = new TimeCode(TimeSpan.FromMilliseconds(_subtitle.Paragraphs[i].EndTime.TotalMilliseconds - offset));
                    SubtitleListview1.SetStartTime(i, _subtitle.Paragraphs[i]);
                }
                timeUpDownStartTime.MaskedTextBox.TextChanged += MaskedTextBox_TextChanged;
            }
        }

        private void buttonSetEndAndGoToNext_Click(object sender, EventArgs e)
        {
            if (SubtitleListview1.SelectedItems.Count == 1)
            {
                int index = SubtitleListview1.SelectedItems[0].Index;
                double videoPosition = mediaPlayer.CurrentPosition;

                _subtitle.Paragraphs[index].EndTime = new TimeCode(TimeSpan.FromSeconds(videoPosition));
                SubtitleListview1.SetDuration(index, _subtitle.Paragraphs[index]);
                numericUpDownDuration.Value = (decimal)(_subtitle.Paragraphs[index].Duration.TotalSeconds);

                if (index + 1 < _subtitle.Paragraphs.Count)
                {
                    SubtitleListview1.Items[index].Selected = false;
                    SubtitleListview1.Items[index + 1].Selected = true;
                    _subtitle.Paragraphs[index + 1].StartTime = new TimeCode(TimeSpan.FromSeconds(videoPosition+0.001));
                    SubtitleListview1.SetStartTime(index + 1, _subtitle.Paragraphs[index + 1]);
                    SubtitleListview1.AutoScrollOffset.Offset(0, index * 16);
                    SubtitleListview1.EnsureVisible(Math.Min(SubtitleListview1.Items.Count - 1, index + 5));
                }
            }
        }

        private void buttonAdjustSecBack_Click(object sender, EventArgs e)
        {
            GoBackSeconds((double)numericUpDownSecAdjust1.Value, labelSubtitle, mediaPlayer.VideoPlayer);
        }

        private void buttonAdjustSecForward_Click(object sender, EventArgs e)
        {
            GoBackSeconds(-(double)numericUpDownSecAdjust1.Value, labelSubtitle, mediaPlayer.VideoPlayer);
        }

        private void Main_Shown(object sender, EventArgs e)
        {
            toolStripButtonToogleVideo.Checked = !Configuration.Settings.General.ShowVideoPlayer;
            toolStripButtonToogleVideo_Click(null, null);
        }

        private void mediaPlayer_DragDrop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (files.Length == 1)
            {
                string fileName = files[0];

                var fi = new FileInfo(fileName);
                string ext = Path.GetExtension(fileName).ToLower();
                
                if (Utilities.GetVideoFileFilter().Contains(ext))
                {
                    OpenVideo(fileName);
                }
                else
                {
                    MessageBox.Show(string.Format(_language.DropFileXNotAccepted, fileName));
                }
            }
            else
            {
                MessageBox.Show(_language.DropOnlyOneFile);
            }
        }

        private void mediaPlayer_DragEnter(object sender, DragEventArgs e)
        {
            // make sure they're actually dropping files (not text or anything else)
            if (e.Data.GetDataPresent(DataFormats.FileDrop, false))
                e.Effect = DragDropEffects.All;
        }

        private void buttonSecBack2_Click(object sender, EventArgs e)
        {
            GoBackSeconds((double)numericUpDownSec2.Value , labelSubtitle, mediaPlayer.VideoPlayer);
        }

        private void buttonForward2_Click(object sender, EventArgs e)
        {
            GoBackSeconds(-(double)numericUpDownSec2.Value, labelSubtitle, mediaPlayer.VideoPlayer);
        }

        private void buttonAdjustSecBack2_Click(object sender, EventArgs e)
        {
            GoBackSeconds((double)numericUpDownSecAdjust2.Value, labelSubtitle, mediaPlayer.VideoPlayer);
        }

        private void buttonAdjustSecForward2_Click(object sender, EventArgs e)
        {
            GoBackSeconds(-(double)numericUpDownSecAdjust2.Value, labelSubtitle, mediaPlayer.VideoPlayer);
        }

        private void translatepoweredByMicrosoftToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TranslateViaGoogle(false, false);
        }

        public static string Sha256Hash(string value)
        {
            System.Security.Cryptography.SHA256Managed hasher = new System.Security.Cryptography.SHA256Managed();
            byte[] bytes = Encoding.UTF8.GetBytes(value);
            byte[] hash = hasher.ComputeHash(bytes);
            return Convert.ToBase64String(hash, 0, hash.Length);
        }

        private string GetPeakWaveFileName(string videoFileName)
        {
            string dir = Configuration.BaseDirectory + "WaveForms";
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            FileInfo fi = new FileInfo(videoFileName);
            string wavePeakName = Sha256Hash(Path.GetFileName(videoFileName) + fi.Length.ToString() + fi.CreationTimeUtc.ToShortDateString()) + ".wav";
            wavePeakName = wavePeakName.Replace("=", string.Empty).Replace("/", string.Empty).Replace(",", string.Empty).Replace("?", string.Empty).Replace("*", string.Empty).Replace("+", string.Empty).Replace("\\", string.Empty);
            wavePeakName = Path.Combine(dir, wavePeakName);
            return wavePeakName;            
        }

        private void AudioWaveForm_Click(object sender, EventArgs e)
        {
            if (AudioWaveForm.WavePeaks == null)
            {
                if (string.IsNullOrEmpty(_videoFileName))
                {
                    buttonOpenVideo_Click(sender, e);
                    if (string.IsNullOrEmpty(_videoFileName))
                        return;
                }
                
                AddWareForm addWaveForm = new AddWareForm();
                addWaveForm.Initialize(_videoFileName);
                if (addWaveForm.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    string peakWaveFileName = GetPeakWaveFileName(addWaveForm.SourceVideoFileName);
                    addWaveForm.WavePeak.WritePeakSamples(peakWaveFileName);
                    var audioPeakWave = new WavePeakGenerator(peakWaveFileName);
                    audioPeakWave.GenerateAllSamples();
                    AudioWaveForm.WavePeaks = audioPeakWave;
                    timerWaveForm.Start();
                }
            }
        }

        private void timerWaveForm_Tick(object sender, EventArgs e)
        {
            if (AudioWaveForm.Visible && mediaPlayer.VideoPlayer != null && AudioWaveForm.WavePeaks != null)
            {
                int index = -1;
                if (SubtitleListview1.SelectedItems.Count > 0)
                    index = SubtitleListview1.SelectedItems[0].Index;
                if (mediaPlayer.CurrentPosition > AudioWaveForm.EndPositionSeconds || mediaPlayer.CurrentPosition < AudioWaveForm.StartPositionSeconds)
                {
                    double startPos = mediaPlayer.CurrentPosition - 0.01;
                    if (startPos < 0)
                        startPos = 0;
                    AudioWaveForm.ClearSelection();
                    AudioWaveForm.SetPosition(startPos, _subtitle, mediaPlayer.CurrentPosition, index);
                }
                else
                {
                    AudioWaveForm.SetPosition(AudioWaveForm.StartPositionSeconds, _subtitle, mediaPlayer.CurrentPosition, index);
                }

                bool paused = mediaPlayer.IsPaused;
                toolStripButtonWaveFormPause.Visible = !paused;
                toolStripButtonWaveFormPlay.Visible = paused;
            }
            else
            {
                toolStripButtonWaveFormPlay.Visible = true;
                toolStripButtonWaveFormPause.Visible = false;
            }
        }

        private void addParagraphHereToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AudioWaveForm.ClearSelection();
            Paragraph newParagraph = new Paragraph(AudioWaveForm.NewSelectionParagraph);
            if (newParagraph == null)
                return;

            mediaPlayer.Pause();

            int startNumber = 1;
            if (_subtitle.Paragraphs.Count > 0)
                startNumber = _subtitle.Paragraphs[0].Number;

            // find index where to insert
            int index = 0;
            foreach (Paragraph p in _subtitle.Paragraphs)
            {
                if (p.StartTime.TotalMilliseconds > newParagraph.StartTime.TotalMilliseconds)
                    break;
                index++;
            }

            // create and insert
            _subtitle.Paragraphs.Insert(index, newParagraph);

            _subtitleListViewIndex = -1;
            _subtitle.Renumber(startNumber);
            SubtitleListview1.Fill(_subtitle.Paragraphs);
            SubtitleListview1.SelectIndexAndEnsureVisible(index);

            textBoxListViewText.Focus();
            AudioWaveForm.NewSelectionParagraph = null;

            ShowStatus(string.Format(_language.VideoControls.NewTextInsertAtX, newParagraph.StartTime.ToShortString()));
        }

        private void mergeWithPreviousToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int index = _subtitle.GetIndex(AudioWaveForm.RightClickedParagraph);
            if (index >= 0)
            {
                SubtitleListview1.SelectIndexAndEnsureVisible(index);
                MergeBeforeToolStripMenuItemClick(null, null); 
            }
        }

        private void deleteParagraphToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int index = _subtitle.GetIndex(AudioWaveForm.RightClickedParagraph);
            if (index >= 0)
            {
                SubtitleListview1.SelectIndexAndEnsureVisible(index);
                ToolStripMenuItemDeleteClick(null, null);
            }
        }

        private void splitToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            int index = _subtitle.GetIndex(AudioWaveForm.RightClickedParagraph);
            if (index >= 0)
            {
                SubtitleListview1.SelectIndexAndEnsureVisible(index);
                SplitLineToolStripMenuItemClick(null, null);
            }
        }

        private void mergeWithNextToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int index = _subtitle.GetIndex(AudioWaveForm.RightClickedParagraph);
            if (index >= 0)
            {
                SubtitleListview1.SelectIndexAndEnsureVisible(index);
                MergeAfterToolStripMenuItemClick(null, null);
            }            
        }

        private void buttonWaveFormZoomIn_Click(object sender, EventArgs e)
        {
            if (AudioWaveForm.WavePeaks != null && AudioWaveForm.Visible)
            {
                AudioWaveForm.ZoomFactor += 0.1;
            }
        }

        private void buttonWaveFormZoomOut_Click(object sender, EventArgs e)
        {
            if (AudioWaveForm.WavePeaks != null && AudioWaveForm.Visible)
            {
                AudioWaveForm.ZoomFactor -= 0.1;
            }
        }

        private void buttonWaveFormZoomReset_Click(object sender, EventArgs e)
        {
            if (AudioWaveForm.WavePeaks != null && AudioWaveForm.Visible)
            {
                AudioWaveForm.ZoomFactor = 1.0;
            }
        }

        private void toolStripMenuItemWaveFormPlaySelection_Click(object sender, EventArgs e)
        {
            if (mediaPlayer != null && mediaPlayer.VideoPlayer != null)
            {
                Paragraph p = AudioWaveForm.NewSelectionParagraph;
                if (p == null)
                    p = AudioWaveForm.RightClickedParagraph;

                if (p != null)
                {
                    mediaPlayer.CurrentPosition = p.StartTime.TotalSeconds;
                    Utilities.ShowSubtitle(_subtitle.Paragraphs, labelSubtitle, mediaPlayer.VideoPlayer);
                    mediaPlayer.Play();
                    _endSeconds = p.EndTime.TotalSeconds;
                }
            }
        }

        private void toolStripButtonWaveFormZoomIn_Click(object sender, EventArgs e)
        {
            if (AudioWaveForm.WavePeaks != null && AudioWaveForm.Visible)
            {
                AudioWaveForm.ZoomFactor += 0.1;
                SelectZoomTextInComboBox();
            }
        }

        private void toolStripButtonWaveFormZoomOut_Click(object sender, EventArgs e)
        {
            if (AudioWaveForm.WavePeaks != null && AudioWaveForm.Visible)
            {
                AudioWaveForm.ZoomFactor -= 0.1;
                SelectZoomTextInComboBox();
            }
        }

        private void toolStripComboBoxWaveForm_SelectedIndexChanged(object sender, EventArgs e)
        {
            ComboBoxZoomItem item = toolStripComboBoxWaveForm.SelectedItem as ComboBoxZoomItem;
            if (item != null)
            {
                AudioWaveForm.ZoomFactor = item.ZoomFactor;
            }
        }

        private void SelectZoomTextInComboBox()
        {
            int i = 0;
            foreach (object obj in toolStripComboBoxWaveForm.Items)
            {
                ComboBoxZoomItem item = obj as ComboBoxZoomItem;
                if (Math.Abs(AudioWaveForm.ZoomFactor - item.ZoomFactor) < 0.001)
                {
                    toolStripComboBoxWaveForm.SelectedIndex = i;
                    return;
                }
                i++;
            }
        }

        private void toolStripButtonWaveFormPause_Click(object sender, EventArgs e)
        {
            mediaPlayer.Pause();
        }

        private void toolStripButtonWaveFormPlay_Click(object sender, EventArgs e)
        {
            mediaPlayer.Play();
        }

        private void trackBarWaveFormPosition_ValueChanged(object sender, EventArgs e)
        {
            mediaPlayer.CurrentPosition = trackBarWaveFormPosition.Value;
        }

        private void buttonCustomUrl_Click(object sender, EventArgs e)
        {
            string url = Configuration.Settings.VideoControls.CustomSearchUrl;
            if (!string.IsNullOrEmpty(url))
            {
                if (url.Contains("{0}"))
                {
                    url = string.Format(url, HttpUtility.UrlEncode(textBoxSearchWord.Text));
                }
                System.Diagnostics.Process.Start(url);
            }
        }

        private void showhideWaveFormToolStripMenuItem_Click(object sender, EventArgs e)
        {
            toolStripButtonToogleWaveForm_Click(null, null);
        }

        private void exportNewWordsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            openFileDialog1.Title = "Open names_etc.xml file to import into names_etc.xml";
            openFileDialog1.FileName = string.Empty;
            openFileDialog1.Filter = "Xml files|*.xml";
            if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                int wordsAdded = 0;
                XmlDocument doc = new XmlDocument();
                doc.Load(openFileDialog1.FileName);

                string language = "en_US";

                string globalNamesEtcFileName = Utilities.DictionaryFolder + "names_etc.xml";

                var namesEtc = new List<string>();
                Utilities.LoadGlobalNamesEtc(namesEtc, namesEtc);
                namesEtc.Sort();

                var localNamesEtc = new List<string>();
                var newNames = new List<string>();
                string userNamesEtcXmlFileName = Utilities.LoadLocalNamesEtc(localNamesEtc, localNamesEtc, language);

                foreach (XmlNode node in doc.DocumentElement.SelectNodes("name"))
                {

                    string word = node.InnerText.Trim();
                    if (!localNamesEtc.Contains(word) && !namesEtc.Contains(word))
                    {
                        wordsAdded++;
                        newNames.Add(word);
                        newNames.Sort();
                    }
                }

                saveFileDialog1.Filter = "Xml files|*.xml";
                saveFileDialog1.FileName = "NewNames.xml";

                if (saveFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    if (MessageBox.Show("Save new words: " + newNames.Count.ToString(), "", MessageBoxButtons.OKCancel) == System.Windows.Forms.DialogResult.OK)
                    {
                        var namesEtcDoc = new XmlDocument();
                        namesEtcDoc.LoadXml("<ignore_words />");

                        XmlNode de = namesEtcDoc.DocumentElement;
                        if (de != null)
                        {
                            foreach (var name in newNames)
                            {
                                XmlNode node = namesEtcDoc.CreateElement("name");
                                node.InnerText = name;
                                de.AppendChild(node);
                            }
                            namesEtcDoc.Save(saveFileDialog1.FileName);
                        }
                    }
                }
            }
        }

    }
}