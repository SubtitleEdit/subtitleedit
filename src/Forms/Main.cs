using Nikse.SubtitleEdit.Controls;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.BluRaySup;
using Nikse.SubtitleEdit.Logic.Enums;
using Nikse.SubtitleEdit.Logic.Networking;
using Nikse.SubtitleEdit.Logic.SubtitleFormats;
using Nikse.SubtitleEdit.Logic.TransportStream;
using Nikse.SubtitleEdit.Logic.VideoFormats;
using Nikse.SubtitleEdit.Logic.VideoPlayers;
using Nikse.SubtitleEdit.Logic.VobSub;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

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

        private const int TabControlListView = 0;
        private const int TabControlSourceView = 1;

        private Subtitle _subtitle = new Subtitle();

        private int _undoIndex = -1;
        private string _listViewTextUndoLast = null;
        private int _listViewTextUndoIndex = -1;
        private long _listViewTextTicks = -1;
        private string _listViewAlternateTextUndoLast = null;
        private long _listViewAlternateTextTicks = -1;

        private Subtitle _subtitleAlternate = new Subtitle();
        private string _subtitleAlternateFileName;
        private string _fileName;
        private string _videoFileName;
        private int _videoAudioTrackNumber = -1;

        public string VideoFileName
        {
            get { return _videoFileName; }
            set { _videoFileName = value; }
        }
        private DateTime _fileDateTime;
        private string _title;
        private FindReplaceDialogHelper _findHelper;
        private int _replaceStartLineIndex = 0;
        private bool _sourceViewChange;
        private string _changeSubtitleToString = string.Empty;
        private string _changeAlternateSubtitleToString = string.Empty;
        private int _subtitleListViewIndex = -1;
        private Paragraph _oldSelectedParagraph;
        private bool _converted;
        private SubtitleFormat _oldSubtitleFormat;
        private List<int> _selectedIndexes;
        private LanguageStructure.Main _language;
        private LanguageStructure.General _languageGeneral;
        private SpellCheck _spellCheckForm;
        private PositionsAndSizes _formPositionsAndSizes = new PositionsAndSizes();
        private bool _loading = true;
        private int _repeatCount = -1;
        private double _endSeconds = -1;
        private const double EndDelay = 0.05;
        private int _autoContinueDelayCount = -1;
        private long _lastTextKeyDownTicks = 0;
        private long _lastHistoryTicks = 0;
        private double? _audioWaveformRightClickSeconds = null;
        private Timer _timerDoSyntaxColoring = new Timer();
        private Timer _timerAutoSave = new Timer();
        private Timer _timerClearStatus = new Timer();
        private string _textAutoSave;
        private string _textAutoSaveOriginal;
        private StringBuilder _statusLog = new StringBuilder();
        private bool _makeHistoryPaused = false;

        private Nikse.SubtitleEdit.Logic.Forms.CheckForUpdatesHelper _checkForUpdatesHelper;
        private Timer _timerCheckForUpdates;

        private NikseWebServiceSession _networkSession;
        private NetworkChat _networkChat = null;

        private ShowEarlierLater _showEarlierOrLater = null;

        private bool _isVideoControlsUndocked = false;
        private VideoPlayerUndocked _videoPlayerUndocked = null;
        private WaveformUndocked _waveformUndocked = null;
        private VideoControlsUndocked _videoControlsUndocked = null;

        private GoogleOrMicrosoftTranslate _googleOrMicrosoftTranslate = null;

        private bool _cancelWordSpellCheck = true;

        private Keys _mainGeneralGoToFirstSelectedLine = Keys.None;
        private Keys _mainGeneralGoToFirstEmptyLine = Keys.None;
        private Keys _mainGeneralMergeSelectedLines = Keys.None;
        private Keys _mainGeneralMergeSelectedLinesOnlyFirstText = Keys.None;
        private Keys _mainGeneralToggleTranslationMode = Keys.None;
        private Keys _mainGeneralSwitchTranslationAndOriginal = Keys.None;
        private Keys _mainGeneralMergeTranslationAndOriginal = Keys.None;
        private Keys _mainGeneralGoToNextSubtitle = Keys.None;
        private Keys _mainGeneralGoToPrevSubtitle = Keys.None;
        private Keys _mainGeneralGoToStartOfCurrentSubtitle = Keys.None;
        private Keys _mainGeneralGoToEndOfCurrentSubtitle = Keys.None;
        private Keys _mainGeneralFileSaveAll = Keys.None;
        private Keys _mainToolsAutoDuration = Keys.None;
        private Keys _mainToolsBeamer = Keys.None;
        private Keys _toggleVideoDockUndock = Keys.None;
        private Keys _videoPause = Keys.None;
        private Keys _videoPlayPauseToggle = Keys.None;
        private Keys _video1FrameLeft = Keys.None;
        private Keys _video1FrameRight = Keys.None;
        private Keys _video100MsLeft = Keys.None;
        private Keys _video100MsRight = Keys.None;
        private Keys _video500MsLeft = Keys.None;
        private Keys _video500MsRight = Keys.None;
        private Keys _video1000MsLeft = Keys.None;
        private Keys _video1000MsRight = Keys.None;
        private Keys _videoPlayFirstSelected = Keys.None;
        private Keys _mainVideoFullscreen = Keys.None;
        private Keys _mainTextBoxSplitAtCursor = Keys.None;
        private Keys _mainTextBoxMoveLastWordDown = Keys.None;
        private Keys _mainTextBoxMoveFirstWordFromNextUp = Keys.None;
        private Keys _mainTextBoxSelectionToLower = Keys.None;
        private Keys _mainTextBoxSelectionToUpper = Keys.None;
        private Keys _mainCreateInsertSubAtVideoPos = Keys.None;
        private Keys _mainCreatePlayFromJustBefore = Keys.None;
        private Keys _mainCreateSetStart = Keys.None;
        private Keys _mainCreateSetEnd = Keys.None;
        private Keys _mainCreateStartDownEndUp = Keys.None;
        private Keys _mainCreateSetEndAddNewAndGoToNew = Keys.None;
        private Keys _mainAdjustSetStartAndOffsetTheRest = Keys.None;
        private Keys _mainAdjustSetEndAndOffsetTheRest = Keys.None;
        private Keys _mainAdjustSetEndAndOffsetTheRestAndGoToNext = Keys.None;
        private Keys _mainAdjustSetEndAndGotoNext = Keys.None;
        private Keys _mainAdjustInsertViaEndAutoStartAndGoToNext = Keys.None;
        private Keys _mainAdjustSetStartAutoDurationAndGoToNext = Keys.None;
        private Keys _mainAdjustSetEndNextStartAndGoToNext = Keys.None;
        private Keys _mainAdjustStartDownEndUpAndGoToNext = Keys.None;
        private Keys _mainAdjustSetStart = Keys.None;
        private Keys _mainAdjustSetStartKeepDuration = Keys.None;
        private Keys _mainAdjustSetEnd = Keys.None;
        private Keys _mainAdjustSelected100MsForward = Keys.None;
        private Keys _mainAdjustSelected100MsBack = Keys.None;
        private Keys _mainInsertAfter = Keys.None;
        private Keys _mainInsertBefore = Keys.None;
        private Keys _mainTextBoxInsertAfter = Keys.None;
        private Keys _mainTextBoxAutoBreak = Keys.None;
        private Keys _mainTextBoxUnbreak = Keys.None;
        private Keys _mainMergeDialog = Keys.None;
        private Keys _mainToggleFocus = Keys.None;
        private Keys _mainListViewToggleDashes = Keys.None;
        private Keys _mainListViewAutoDuration = Keys.None;
        private Keys _mainListViewFocusWaveform = Keys.None;
        private Keys _mainListViewGoToNextError = Keys.None;
        private Keys _mainListViewCopyText = Keys.None;
        private Keys _mainEditReverseStartAndEndingForRTL = Keys.None;
        private Keys _waveformVerticalZoom = Keys.None;
        private Keys _waveformVerticalZoomOut = Keys.None;
        private Keys _waveformZoomIn = Keys.None;
        private Keys _waveformZoomOut = Keys.None;
        private Keys _waveformPlaySelection = Keys.None;
        private Keys _waveformSearchSilenceForward = Keys.None;
        private Keys _waveformSearchSilenceBack = Keys.None;
        private Keys _waveformAddTextAtHere = Keys.None;
        private Keys _waveformFocusListView = Keys.None;
        private Keys _mainTranslateCustomSearch1 = Keys.None;
        private Keys _mainTranslateCustomSearch2 = Keys.None;
        private Keys _mainTranslateCustomSearch3 = Keys.None;
        private Keys _mainTranslateCustomSearch4 = Keys.None;
        private Keys _mainTranslateCustomSearch5 = Keys.None;
        private Keys _mainTranslateCustomSearch6 = Keys.None;
        private bool _videoLoadedGoToSubPosAndPause = false;
        private string _cutText = string.Empty;
        private Paragraph _mainCreateStartDownEndUpParagraph;
        private Paragraph _mainAdjustStartDownEndUpAndGoToNextParagraph;
        private string _lastDoNotPrompt = string.Empty;
        private VideoInfo _videoInfo = null;
        private bool _splitDualSami = false;
        private bool _openFileDialogOn = false;
        private bool _resetVideo = true;
        private static object _syncUndo = new object();
        private string[] _dragAndDropFiles = null;
        private Timer _dragAndDropTimer = new Timer(); // to prevent locking windows explorer

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
                    _title = String.Format("{0} {1}.{2}.{3}", _languageGeneral.Title, versionInfo[0], versionInfo[1], versionInfo[2]);
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
                {
                    comboBoxSubtitleFormats.SelectedIndex = i;
                    return;
                }
                i++;
            }
        }

        public void SetCurrentFormat(string subtitleFormatName)
        {
            foreach (SubtitleFormat format in SubtitleFormat.AllSubtitleFormats)
            {
                if (format.Name.Trim().Equals(subtitleFormatName.Trim(), StringComparison.OrdinalIgnoreCase) ||
                    format.FriendlyName.Trim().Equals(subtitleFormatName.Trim(), StringComparison.OrdinalIgnoreCase))
                {
                    SetCurrentFormat(format);
                    return;
                }
            }
            SetCurrentFormat(new SubRip());
        }

        public Main()
        {
            try
            {
                InitializeComponent();

                textBoxListViewTextAlternate.Visible = false;
                labelAlternateText.Visible = false;
                labelAlternateCharactersPerSecond.Visible = false;
                labelTextAlternateLineLengths.Visible = false;
                labelAlternateSingleLine.Visible = false;
                labelTextAlternateLineTotal.Visible = false;

                SetLanguage(Configuration.Settings.General.Language);
                toolStripStatusNetworking.Visible = false;
                labelTextLineLengths.Text = string.Empty;
                labelCharactersPerSecond.Text = string.Empty;
                labelTextLineTotal.Text = string.Empty;
                labelStartTimeWarning.Text = string.Empty;
                labelDurationWarning.Text = string.Empty;
                labelVideoInfo.Text = string.Empty;
                labelSingleLine.Text = string.Empty;
                Text = Title;
                timeUpDownStartTime.TimeCode = new TimeCode(0, 0, 0, 0);
                checkBoxAutoRepeatOn.Checked = Configuration.Settings.General.AutoRepeatOn;
                comboBoxAutoRepeat.SelectedIndex = Configuration.Settings.General.AutoRepeatCount;
                checkBoxAutoContinue.Checked = Configuration.Settings.General.AutoContinueOn;
                checkBoxSyncListViewWithVideoWhilePlaying.Checked = Configuration.Settings.General.SyncListViewWithVideoWhilePlaying;

                SetFormatToSubRip();

                if (Configuration.Settings.General.DefaultSubtitleFormat != "SubRip")
                    SetCurrentFormat(Configuration.Settings.General.DefaultSubtitleFormat);

                comboBoxEncoding.Items.Clear();
                comboBoxEncoding.Items.Add(Encoding.UTF8.EncodingName);
                foreach (EncodingInfo ei in Encoding.GetEncodings())
                {
                    if (ei.Name != Encoding.UTF8.BodyName && ei.CodePage >= 949 && !ei.DisplayName.Contains("EBCDIC") && ei.CodePage != 1047) //Configuration.Settings.General.EncodingMininumCodePage)
                        comboBoxEncoding.Items.Add(ei.CodePage + ": " + ei.DisplayName);
                }
                SetEncoding(Configuration.Settings.General.DefaultEncoding);

                toolStripComboBoxFrameRate.Items.Add((23.976).ToString());
                toolStripComboBoxFrameRate.Items.Add((24.0).ToString());
                toolStripComboBoxFrameRate.Items.Add((25.0).ToString());
                toolStripComboBoxFrameRate.Items.Add((29.97).ToString());
                toolStripComboBoxFrameRate.Items.Add((30).ToString());
                toolStripComboBoxFrameRate.Text = Configuration.Settings.General.DefaultFrameRate.ToString();

                UpdateRecentFilesUI();
                InitializeToolbar();
                Utilities.InitializeSubtitleFont(textBoxSource);
                Utilities.InitializeSubtitleFont(textBoxListViewText);
                Utilities.InitializeSubtitleFont(textBoxListViewTextAlternate);
                Utilities.InitializeSubtitleFont(SubtitleListview1);

                if (Configuration.Settings.General.CenterSubtitleInTextBox)
                {
                    textBoxListViewText.TextAlign = HorizontalAlignment.Center;
                    textBoxListViewTextAlternate.TextAlign = HorizontalAlignment.Center;
                }

                tabControlSubtitle.SelectTab(TabControlSourceView); // AC
                ShowSourceLineNumber();                             // AC
                tabControlSubtitle.SelectTab(TabControlListView);   // AC
                if (Configuration.Settings.General.StartInSourceView)
                    tabControlSubtitle.SelectTab(TabControlSourceView);

                audioVisualizer.Visible = Configuration.Settings.General.ShowAudioVisualizer;
                audioVisualizer.ShowWaveform = Configuration.Settings.General.ShowWaveform;
                audioVisualizer.ShowSpectrogram = Configuration.Settings.General.ShowSpectrogram;
                panelWaveformControls.Visible = Configuration.Settings.General.ShowAudioVisualizer;
                trackBarWaveformPosition.Visible = Configuration.Settings.General.ShowAudioVisualizer;
                toolStripButtonToggleWaveform.Checked = Configuration.Settings.General.ShowAudioVisualizer;
                toolStripButtonToggleVideo.Checked = Configuration.Settings.General.ShowVideoPlayer;

                if (Configuration.Settings.General.UseTimeFormatHHMMSSFF)
                {
                    numericUpDownDuration.DecimalPlaces = 2;
                    numericUpDownDuration.Increment = (decimal)(0.01);
                    toolStripSeparatorFrameRate.Visible = true;
                    toolStripLabelFrameRate.Visible = true;
                    toolStripComboBoxFrameRate.Visible = true;
                    toolStripButtonGetFrameRate.Visible = true;
                }

                _timerClearStatus.Interval = Configuration.Settings.General.ClearStatusBarAfterSeconds * 1000;
                _timerClearStatus.Tick += TimerClearStatus_Tick;

                string fileName = string.Empty;
                string[] args = Environment.GetCommandLineArgs();
                int srcLineNumber = -1;
                if (args.Length >= 2 && args[1].Equals("/convert", StringComparison.OrdinalIgnoreCase))
                {
                    BatchConvert(args);
                    return;
                }
                else if (args.Length >= 2)
                {
                    fileName = args[1];
                    if (args.Length > 2 && args[2].StartsWith("/srcline:", StringComparison.OrdinalIgnoreCase))
                    {
                        string srcLine = args[2].Remove(0, 9);
                        if (!int.TryParse(srcLine, out srcLineNumber))
                            srcLineNumber = -1;
                    }
                }

                if (fileName.Length > 0 && File.Exists(fileName))
                {
                    OpenSubtitle(fileName, null);
                    if (srcLineNumber >= 0 && GetCurrentSubtitleFormat().GetType() == typeof(SubRip) && srcLineNumber < textBoxSource.Lines.Length)
                    {
                        int pos = 0;
                        for (int i = 0; i < srcLineNumber; i++)
                        {
                            pos += textBoxSource.Lines[i].Length;
                        }
                        if (pos + 35 < textBoxSource.TextLength)
                            pos += 35;
                        string s = textBoxSource.Text.Substring(0, pos);
                        int lastTimeCode = s.LastIndexOf(" --> ", StringComparison.Ordinal); // 00:02:26,407 --> 00:02:31,356
                        if (lastTimeCode > 14 && lastTimeCode + 16 >= s.Length)
                        {
                            s = s.Substring(0, lastTimeCode - 5);
                            lastTimeCode = s.LastIndexOf(" --> ", StringComparison.Ordinal);
                        }

                        if (lastTimeCode > 14 && lastTimeCode + 16 < s.Length)
                        {
                            string tc = s.Substring(lastTimeCode - 13, 30).Trim();
                            int index = 0;
                            foreach (Paragraph p in _subtitle.Paragraphs)
                            {
                                if (tc == p.StartTime + " --> " + p.EndTime)
                                {
                                    SubtitleListview1.SelectNone();
                                    SubtitleListview1.Items[0].Selected = false;
                                    SubtitleListview1.SelectIndexAndEnsureVisible(index, true);
                                    break;
                                }
                                index++;
                            }
                        }
                    }
                }
                else if (Configuration.Settings.General.StartLoadLastFile)
                {
                    if (Configuration.Settings.RecentFiles.Files.Count > 0)
                    {
                        fileName = Configuration.Settings.RecentFiles.Files[0].FileName;
                        if (File.Exists(fileName))
                        {
                            OpenSubtitle(fileName, null, Configuration.Settings.RecentFiles.Files[0].VideoFileName, Configuration.Settings.RecentFiles.Files[0].OriginalFileName);
                            SetRecentIndecies(fileName);
                            GotoSubPosAndPause();
                        }
                    }
                }

                labelAutoDuration.Visible = false;
                mediaPlayer.SubtitleText = string.Empty;
                //                comboBoxAutoRepeat.SelectedIndex = 2;
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

                buttonCustomUrl1.Text = Configuration.Settings.VideoControls.CustomSearchText1;
                buttonCustomUrl1.Visible = Configuration.Settings.VideoControls.CustomSearchUrl1.Length > 1;

                buttonCustomUrl2.Text = Configuration.Settings.VideoControls.CustomSearchText2;
                buttonCustomUrl2.Visible = Configuration.Settings.VideoControls.CustomSearchUrl2.Length > 1;

                // Initialize events etc. for audio waveform
                audioVisualizer.OnDoubleClickNonParagraph += AudioWaveform_OnDoubleClickNonParagraph;
                audioVisualizer.OnPositionSelected += AudioWaveform_OnPositionSelected;
                audioVisualizer.OnTimeChanged += AudioWaveform_OnTimeChanged; // start and/or end position of paragraph changed
                audioVisualizer.OnNewSelectionRightClicked += AudioWaveform_OnNewSelectionRightClicked;
                audioVisualizer.OnParagraphRightClicked += AudioWaveform_OnParagraphRightClicked;
                audioVisualizer.OnNonParagraphRightClicked += AudioWaveform_OnNonParagraphRightClicked;
                audioVisualizer.OnSingleClick += AudioWaveform_OnSingleClick;
                audioVisualizer.OnPause += AudioWaveform_OnPause;
                audioVisualizer.OnTimeChangedAndOffsetRest += AudioWaveform_OnTimeChangedAndOffsetRest;
                audioVisualizer.OnZoomedChanged += AudioWaveform_OnZoomedChanged;
                audioVisualizer.InsertAtVideoPosition += audioVisualizer_InsertAtVideoPosition;
                audioVisualizer.DrawGridLines = Configuration.Settings.VideoControls.WaveformDrawGrid;
                audioVisualizer.GridColor = Configuration.Settings.VideoControls.WaveformGridColor;
                audioVisualizer.SelectedColor = Configuration.Settings.VideoControls.WaveformSelectedColor;
                audioVisualizer.Color = Configuration.Settings.VideoControls.WaveformColor;
                audioVisualizer.BackgroundColor = Configuration.Settings.VideoControls.WaveformBackgroundColor;
                audioVisualizer.TextColor = Configuration.Settings.VideoControls.WaveformTextColor;
                audioVisualizer.MouseWheelScrollUpIsForward = Configuration.Settings.VideoControls.WaveformMouseWheelScrollUpIsForward;
                audioVisualizer.AllowOverlap = Configuration.Settings.VideoControls.WaveformAllowOverlap;
                audioVisualizer.ClosenessForBorderSelection = Configuration.Settings.VideoControls.WaveformBorderHitMs;

                for (double zoomCounter = AudioVisualizer.ZoomMininum; zoomCounter <= AudioVisualizer.ZoomMaxinum + (0.001); zoomCounter += 0.1)
                {
                    int percent = (int)Math.Round((zoomCounter * 100));
                    ComboBoxZoomItem item = new ComboBoxZoomItem() { Text = percent + "%", ZoomFactor = zoomCounter };
                    toolStripComboBoxWaveform.Items.Add(item);
                    if (percent == 100)
                        toolStripComboBoxWaveform.SelectedIndex = toolStripComboBoxWaveform.Items.Count - 1;
                }
                toolStripComboBoxWaveform.SelectedIndexChanged += toolStripComboBoxWaveform_SelectedIndexChanged;

                FixLargeFonts();

                if (Configuration.Settings.General.RightToLeftMode)
                    ToolStripMenuItemRightToLeftModeClick(null, null);
            }
            catch (Exception exception)
            {
                Cursor = Cursors.Default;
                MessageBox.Show(exception.Message + Environment.NewLine + exception.StackTrace);
            }
        }

        private void audioVisualizer_InsertAtVideoPosition(object sender, EventArgs e)
        {
            InsertNewTextAtVideoPosition();
        }

        private void TimerClearStatus_Tick(object sender, EventArgs e)
        {
            ShowStatus(string.Empty);
        }

        private void SetEncoding(Encoding encoding)
        {
            if (encoding.BodyName == Encoding.UTF8.BodyName)
            {
                comboBoxEncoding.SelectedIndex = 0;
                return;
            }

            int i = 0;
            foreach (string s in comboBoxEncoding.Items)
            {
                if (s == encoding.CodePage + ": " + encoding.EncodingName)
                {
                    comboBoxEncoding.SelectedIndex = i;
                    return;
                }
                i++;
            }
            comboBoxEncoding.SelectedIndex = 0;
        }

        private void SetEncoding(string encodingName)
        {
            if (encodingName == Encoding.UTF8.BodyName || encodingName == Encoding.UTF8.EncodingName || encodingName == "utf-8")
            {
                comboBoxEncoding.SelectedIndex = 0;
                return;
            }

            int i = 0;
            foreach (string s in comboBoxEncoding.Items)
            {
                if (s == encodingName || s.StartsWith(encodingName + ":"))
                {
                    comboBoxEncoding.SelectedIndex = i;
                    return;
                }
                i++;
            }
            comboBoxEncoding.SelectedIndex = 0;
        }

        private Encoding GetCurrentEncoding()
        {
            if (comboBoxEncoding.Text == Encoding.UTF8.BodyName || comboBoxEncoding.Text == Encoding.UTF8.EncodingName || comboBoxEncoding.Text == "utf-8")
            {
                return Encoding.UTF8;
            }

            foreach (EncodingInfo ei in Encoding.GetEncodings())
            {
                if (ei.CodePage + ": " + ei.DisplayName == comboBoxEncoding.Text)
                    return ei.GetEncoding();
            }

            return Encoding.UTF8;
        }

        private void BatchConvert(string[] args) // E.g.: /convert *.txt SubRip
        {
            const int ATTACH_PARENT_PROCESS = -1;
            if (!Configuration.IsRunningOnMac() && !Configuration.IsRunningOnLinux())
                NativeMethods.AttachConsole(ATTACH_PARENT_PROCESS);

            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine(Title + " - Batch converter");
            Console.WriteLine();
            Console.WriteLine("- Syntax: SubtitleEdit /convert <pattern> <name-of-format-without-spaces> [/offset:hh:mm:ss:ms] [/encoding:<encoding name>] [/fps:<frame rate>] [/inputfolder:<input folder>] [/outputfolder:<output folder>] [/pac-codepage:<code page>]");
            Console.WriteLine();
            Console.WriteLine("    example: SubtitleEdit /convert *.srt sami");
            Console.WriteLine("    list available formats: SubtitleEdit /convert /list");
            Console.WriteLine();

            string currentDir = Directory.GetCurrentDirectory();

            if (args.Length < 4)
            {
                if (args.Length == 3 && (args[2].Equals("/list", StringComparison.OrdinalIgnoreCase) || args[2].Equals("-list", StringComparison.OrdinalIgnoreCase)))
                {
                    Console.WriteLine("- Supported formats (input/output):");
                    foreach (SubtitleFormat format in SubtitleFormat.AllSubtitleFormats)
                    {
                        Console.WriteLine("    " + format.Name.Replace(" ", string.Empty));
                    }
                    Console.WriteLine();
                    Console.WriteLine("- Supported formats (input only):");
                    Console.WriteLine("    " + new CapMakerPlus().FriendlyName);
                    Console.WriteLine("    " + new Captionate().FriendlyName);
                    Console.WriteLine("    " + new Cavena890().FriendlyName);
                    Console.WriteLine("    " + new CheetahCaption().FriendlyName);
                    Console.WriteLine("    " + new Chk().FriendlyName);
                    //                    Console.WriteLine("    " + new Ebu().FriendlyName);
                    Console.WriteLine("    Matroska (.mkv)");
                    Console.WriteLine("    Matroska subtitle (.mks)");
                    Console.WriteLine("    " + new NciCaption().FriendlyName);
                    Console.WriteLine("    " + new AvidStl().FriendlyName);
                    Console.WriteLine("    " + new Pac().FriendlyName);
                    Console.WriteLine("    " + new Spt().FriendlyName);
                    Console.WriteLine("    " + new Ultech130().FriendlyName);
                }

                Console.WriteLine();
                Console.Write(currentDir + ">");
                if (!Configuration.IsRunningOnMac() && !Configuration.IsRunningOnLinux())
                    NativeMethods.FreeConsole();
                Environment.Exit(1);
            }

            int count = 0;
            int converted = 0;
            int errors = 0;
            string inputDirectory = string.Empty;
            try
            {
                int max = args.Length;

                string pattern = args[2];
                string toFormat = args[3];
                string offset = string.Empty;
                for (int idx = 4; idx < max; idx++)
                    if (args.Length > idx && args[idx].StartsWith("/offset:", StringComparison.OrdinalIgnoreCase))
                        offset = args[idx].ToLower();

                string fps = string.Empty;
                for (int idx = 4; idx < max; idx++)
                    if (args.Length > idx && args[idx].StartsWith("/fps:", StringComparison.OrdinalIgnoreCase))
                        fps = args[idx].ToLower();
                if (fps.Length > 6)
                {
                    fps = fps.Remove(0, 5).Replace(",", ".").Trim();
                    double d;
                    if (double.TryParse(fps, System.Globalization.NumberStyles.AllowDecimalPoint, System.Globalization.CultureInfo.InvariantCulture, out d))
                    {
                        toolStripComboBoxFrameRate.Text = d.ToString(System.Globalization.CultureInfo.InvariantCulture);
                        Configuration.Settings.General.CurrentFrameRate = d;
                    }
                }

                string targetEncodingName = string.Empty;
                for (int idx = 4; idx < max; idx++)
                    if (args.Length > idx && args[idx].StartsWith("/encoding:", StringComparison.OrdinalIgnoreCase))
                        targetEncodingName = args[idx].ToLower();
                Encoding targetEncoding = Encoding.UTF8;
                try
                {
                    if (!string.IsNullOrEmpty(targetEncodingName))
                    {
                        targetEncodingName = targetEncodingName.Substring(10);
                        if (!string.IsNullOrEmpty(targetEncodingName))
                            targetEncoding = Encoding.GetEncoding(targetEncodingName);
                    }
                }
                catch (Exception exception)
                {
                    Console.WriteLine("Unable to set encoding (" + exception.Message + ") - using UTF-8");
                    targetEncoding = Encoding.UTF8;
                }

                string outputFolder = string.Empty;
                for (int idx = 4; idx < max; idx++)
                    if (args.Length > idx && args[idx].StartsWith("/outputfolder:", StringComparison.OrdinalIgnoreCase))
                        outputFolder = args[idx].ToLower();
                if (outputFolder.Length > "/outputFolder:".Length)
                {
                    outputFolder = outputFolder.Remove(0, "/outputFolder:".Length);
                    if (!Directory.Exists(outputFolder))
                        outputFolder = string.Empty;
                }

                string inputFolder = Directory.GetCurrentDirectory();
                for (int idx = 4; idx < max; idx++)
                    if (args.Length > idx && args[idx].StartsWith("/inputFolder:", StringComparison.OrdinalIgnoreCase))
                        inputFolder = args[idx].ToLower();
                if (inputFolder.Length > "/inputFolder:".Length)
                {
                    inputFolder = inputFolder.Remove(0, "/inputFolder:".Length);
                    if (!Directory.Exists(inputFolder))
                        inputFolder = Directory.GetCurrentDirectory();
                }

                string pacCodePage = string.Empty;
                for (int idx = 4; idx < max; idx++)
                    if (args.Length > idx && args[idx].StartsWith("/pac-codepage:", StringComparison.OrdinalIgnoreCase))
                        pacCodePage = args[idx].ToLower();
                if (pacCodePage.Length > "/pac-codepage:".Length)
                    pacCodePage = pacCodePage.Remove(0, "/pac-codepage:".Length);

                bool overwrite = false;
                for (int idx = 4; idx < max; idx++)
                    if (args.Length > idx && args[idx].Equals("/overwrite", StringComparison.OrdinalIgnoreCase))
                        overwrite = true;

                string[] files;
                inputDirectory = Directory.GetCurrentDirectory();
                if (!string.IsNullOrEmpty(inputFolder))
                    inputDirectory = inputFolder;

                if (pattern.Contains(',') && !File.Exists(pattern))
                {
                    files = pattern.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    for (int k = 0; k < files.Length; k++)
                        files[k] = files[k].Trim();
                }
                else
                {
                    int indexOfDirectorySeparatorChar = pattern.LastIndexOf(Path.DirectorySeparatorChar);
                    if (indexOfDirectorySeparatorChar > 0 && indexOfDirectorySeparatorChar < pattern.Length)
                    {
                        pattern = pattern.Substring(indexOfDirectorySeparatorChar + 1);
                        inputDirectory = args[2].Substring(0, indexOfDirectorySeparatorChar);
                    }
                    files = Directory.GetFiles(inputDirectory, pattern);
                }

                var formats = SubtitleFormat.AllSubtitleFormats;
                foreach (string fName in files)
                {
                    string fileName = fName;
                    count++;

                    if (!string.IsNullOrEmpty(inputFolder) && File.Exists(Path.Combine(inputFolder, fileName)))
                        fileName = Path.Combine(inputFolder, fileName);

                    if (File.Exists(fileName))
                    {
                        var sub = new Subtitle();
                        SubtitleFormat format = null;
                        bool done = false;

                        if (Path.GetExtension(fileName).Equals(".mkv", StringComparison.OrdinalIgnoreCase) || Path.GetExtension(fileName).Equals(".mks", StringComparison.OrdinalIgnoreCase))
                        {
                            Matroska mkv = new Matroska();
                            bool isValid = false;
                            bool hasConstantFrameRate = false;
                            double frameRate = 0;
                            int width = 0;
                            int height = 0;
                            double milliseconds = 0;
                            string videoCodec = string.Empty;
                            mkv.GetMatroskaInfo(fileName, ref isValid, ref hasConstantFrameRate, ref frameRate, ref width, ref height, ref milliseconds, ref videoCodec);
                            if (isValid)
                            {
                                var subtitleList = mkv.GetMatroskaSubtitleTracks(fileName, out isValid);
                                if (subtitleList.Count > 0)
                                {
                                    foreach (MatroskaSubtitleInfo x in subtitleList)
                                    {
                                        if (x.CodecId.Equals("S_VOBSUB", StringComparison.OrdinalIgnoreCase))
                                        {
                                            Console.WriteLine("{0}: {1} - Cannot convert from VobSub image based format!", fileName, toFormat);
                                        }
                                        else if (x.CodecId.Equals("S_HDMV/PGS", StringComparison.OrdinalIgnoreCase))
                                        {
                                            Console.WriteLine("{0}: {1} - Cannot convert from Blu-ray image based format!", fileName, toFormat);
                                        }
                                        else
                                        {
                                            LoadMatroskaSubtitle(x, fileName, true);
                                            sub = _subtitle;
                                            format = GetCurrentSubtitleFormat();
                                            string newFileName = fileName;
                                            if (subtitleList.Count > 1)
                                                newFileName = fileName.Insert(fileName.Length - 4, "_" + x.TrackNumber + "_" + x.Language.Replace("?", string.Empty).Replace("!", string.Empty).Replace("*", string.Empty).Replace(",", string.Empty).Replace("/", string.Empty).Trim());

                                            if (format.GetType() == typeof(AdvancedSubStationAlpha) || format.GetType() == typeof(SubStationAlpha))
                                            {
                                                if (toFormat.ToLower() != new AdvancedSubStationAlpha().Name.ToLower().Replace(" ", string.Empty) &&
                                                    toFormat.ToLower() != new SubStationAlpha().Name.ToLower().Replace(" ", string.Empty))
                                                {

                                                    foreach (SubtitleFormat sf in formats)
                                                    {
                                                        if (sf.Name.Replace(" ", string.Empty).Equals(toFormat, StringComparison.OrdinalIgnoreCase) || sf.Name.Replace(" ", string.Empty).Equals(toFormat.Replace(" ", string.Empty), StringComparison.OrdinalIgnoreCase))
                                                        {
                                                            format.RemoveNativeFormatting(sub, sf);
                                                            break;
                                                        }
                                                    }

                                                }
                                            }

                                            BatchConvertSave(toFormat, offset, targetEncoding, outputFolder, count, ref converted, ref errors, formats, newFileName, sub, format, overwrite, pacCodePage);
                                            done = true;
                                        }
                                    }
                                }
                            }
                        }

                        var fi = new FileInfo(fileName);
                        if (fi.Length < 10 * 1024 * 1024 && !done) // max 10 mb
                        {
                            Encoding encoding;
                            format = sub.LoadSubtitle(fileName, out encoding, null, true);

                            if (format == null || format.GetType() == typeof(Ebu))
                            {
                                var ebu = new Ebu();
                                if (ebu.IsMine(null, fileName))
                                {
                                    ebu.LoadSubtitle(sub, null, fileName);
                                    format = ebu;
                                }
                            }
                            if (format == null)
                            {
                                var pac = new Pac();
                                if (pac.IsMine(null, fileName))
                                {
                                    pac.BatchMode = true;
                                    pac.LoadSubtitle(sub, null, fileName);
                                    format = pac;
                                }
                            }
                            if (format == null)
                            {
                                var cavena890 = new Cavena890();
                                if (cavena890.IsMine(null, fileName))
                                {
                                    cavena890.LoadSubtitle(sub, null, fileName);
                                    format = cavena890;
                                }
                            }
                            if (format == null)
                            {
                                var spt = new Spt();
                                if (spt.IsMine(null, fileName))
                                {
                                    spt.LoadSubtitle(sub, null, fileName);
                                    format = spt;
                                }
                            }
                            if (format == null)
                            {
                                var cheetahCaption = new CheetahCaption();
                                if (cheetahCaption.IsMine(null, fileName))
                                {
                                    cheetahCaption.LoadSubtitle(sub, null, fileName);
                                    format = cheetahCaption;
                                }
                            }
                            if (format == null)
                            {
                                var chk = new Chk();
                                if (chk.IsMine(null, fileName))
                                {
                                    chk.LoadSubtitle(sub, null, fileName);
                                    format = chk;
                                }
                            }
                            if (format == null)
                            {
                                var capMakerPlus = new CapMakerPlus();
                                if (capMakerPlus.IsMine(null, fileName))
                                {
                                    capMakerPlus.LoadSubtitle(sub, null, fileName);
                                    format = capMakerPlus;
                                }
                            }
                            if (format == null)
                            {
                                var captionate = new Captionate();
                                if (captionate.IsMine(null, fileName))
                                {
                                    captionate.LoadSubtitle(sub, null, fileName);
                                    format = captionate;
                                }
                            }
                            if (format == null)
                            {
                                var ultech130 = new Ultech130();
                                if (ultech130.IsMine(null, fileName))
                                {
                                    ultech130.LoadSubtitle(sub, null, fileName);
                                    format = ultech130;
                                }
                            }
                            if (format == null)
                            {
                                var nciCaption = new NciCaption();
                                if (nciCaption.IsMine(null, fileName))
                                {
                                    nciCaption.LoadSubtitle(sub, null, fileName);
                                    format = nciCaption;
                                }
                            }
                            if (format == null)
                            {
                                var tsb4 = new TSB4();
                                if (tsb4.IsMine(null, fileName))
                                {
                                    tsb4.LoadSubtitle(sub, null, fileName);
                                    format = tsb4;
                                }
                            }
                            if (format == null)
                            {
                                var avidStl = new AvidStl();
                                if (avidStl.IsMine(null, fileName))
                                {
                                    avidStl.LoadSubtitle(sub, null, fileName);
                                    format = avidStl;
                                }
                            }
                            if (format == null)
                            {
                                var elr = new ELRStudioClosedCaption();
                                if (elr.IsMine(null, fileName))
                                {
                                    elr.LoadSubtitle(sub, null, fileName);
                                    format = elr;
                                }
                            }
                        }

                        if (format == null)
                        {
                            if (fi.Length < 1024 * 1024) // max 1 mb
                                Console.WriteLine("{0}: {1} - input file format unknown!", fileName, toFormat);
                            else
                                Console.WriteLine("{0}: {1} - input file too large!", fileName, toFormat);
                        }
                        else if (!done)
                        {
                            BatchConvertSave(toFormat, offset, targetEncoding, outputFolder, count, ref converted, ref errors, formats, fileName, sub, format, overwrite, pacCodePage);
                        }
                    }
                    else
                    {
                        Console.WriteLine("{0}: {1} - file not found!", count, fileName);
                        errors++;
                    }
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine();
                Console.WriteLine("Ups - an error occured: " + exception.Message);
                Console.WriteLine();
            }

            Console.WriteLine();
            Console.WriteLine("{0} file(s) converted", converted);
            Console.WriteLine();
            Console.Write(currentDir + ">");

            if (!Configuration.IsRunningOnMac() && !Configuration.IsRunningOnLinux())
                NativeMethods.FreeConsole();

            if (count == converted && errors == 0)
                Environment.Exit(0);
            else
                Environment.Exit(1);
        }

        internal static bool BatchConvertSave(string toFormat, string offset, Encoding targetEncoding, string outputFolder, int count, ref int converted, ref int errors, IList<SubtitleFormat> formats, string fileName, Subtitle sub, SubtitleFormat format, bool overwrite, string pacCodePage)
        {
            // adjust offset
            if (!string.IsNullOrEmpty(offset) && (offset.StartsWith("/offset:") || offset.StartsWith("offset:")))
            {
                string[] parts = offset.Split(new[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length == 5)
                {
                    try
                    {
                        TimeSpan ts = new TimeSpan(0, int.Parse(parts[1].TrimStart('-')), int.Parse(parts[2]), int.Parse(parts[3]), int.Parse(parts[4]));
                        if (parts[1].StartsWith('-'))
                            sub.AddTimeToAllParagraphs(ts.Negate());
                        else
                            sub.AddTimeToAllParagraphs(ts);
                    }
                    catch
                    {
                        Console.Write(" (unable to read offset " + offset + ")");
                    }
                }
            }

            bool targetFormatFound = false;
            string outputFileName;
            foreach (SubtitleFormat sf in formats)
            {
                if (sf.IsTextBased && (sf.Name.Replace(" ", string.Empty).Equals(toFormat, StringComparison.OrdinalIgnoreCase) || sf.Name.Replace(" ", string.Empty).Equals(toFormat.Replace(" ", string.Empty), StringComparison.OrdinalIgnoreCase)))
                {
                    targetFormatFound = true;
                    sf.BatchMode = true;
                    outputFileName = FormatOutputFileNameForBatchConvert(fileName, sf.Extension, outputFolder, overwrite);
                    Console.Write("{0}: {1} -> {2}...", count, Path.GetFileName(fileName), outputFileName);
                    if (sf.IsFrameBased && !sub.WasLoadedWithFrameNumbers)
                        sub.CalculateFrameNumbersFromTimeCodesNoCheck(Configuration.Settings.General.CurrentFrameRate);
                    else if (sf.IsTimeBased && sub.WasLoadedWithFrameNumbers)
                        sub.CalculateTimeCodesFromFrameNumbers(Configuration.Settings.General.CurrentFrameRate);

                    if (sf.GetType() == typeof(ItunesTimedText) || sf.GetType() == typeof(ScenaristClosedCaptions) || sf.GetType() == typeof(ScenaristClosedCaptionsDropFrame))
                    {
                        Encoding outputEnc = new UTF8Encoding(false); // create encoding with no BOM
                        TextWriter file = new StreamWriter(outputFileName, false, outputEnc); // open file with encoding
                        file.Write(sub.ToText(sf));
                        file.Close(); // save and close it
                    }
                    else if (targetEncoding == Encoding.UTF8 && (format.GetType() == typeof(TmpegEncAW5) || format.GetType() == typeof(TmpegEncXml)))
                    {
                        Encoding outputEnc = new UTF8Encoding(false); // create encoding with no BOM
                        TextWriter file = new StreamWriter(outputFileName, false, outputEnc); // open file with encoding
                        file.Write(sub.ToText(sf));
                        file.Close(); // save and close it
                    }
                    else
                    {
                        File.WriteAllText(outputFileName, sub.ToText(sf), targetEncoding);
                    }

                    if (format.GetType() == typeof(Sami) || format.GetType() == typeof(SamiModern))
                    {
                        var sami = (Sami)format;
                        foreach (string className in Sami.GetStylesFromHeader(sub.Header))
                        {
                            var newSub = new Subtitle();
                            foreach (Paragraph p in sub.Paragraphs)
                            {
                                if (p.Extra != null && p.Extra.Trim().Equals(className.Trim(), StringComparison.OrdinalIgnoreCase))
                                    newSub.Paragraphs.Add(p);
                            }
                            if (newSub.Paragraphs.Count > 0 && newSub.Paragraphs.Count < sub.Paragraphs.Count)
                            {
                                string s = fileName;
                                if (s.LastIndexOf('.') > 0)
                                    s = s.Insert(s.LastIndexOf('.'), "_" + className);
                                else
                                    s += "_" + className + format.Extension;
                                outputFileName = FormatOutputFileNameForBatchConvert(s, sf.Extension, outputFolder, overwrite);
                                File.WriteAllText(outputFileName, newSub.ToText(sf), targetEncoding);
                            }
                        }
                    }
                    Console.WriteLine(" done.");
                }
            }
            if (!targetFormatFound)
            {
                var ebu = new Ebu();
                if (ebu.Name.Replace(" ", string.Empty).Equals(toFormat.Replace(" ", string.Empty), StringComparison.OrdinalIgnoreCase))
                {
                    targetFormatFound = true;
                    outputFileName = FormatOutputFileNameForBatchConvert(fileName, ebu.Extension, outputFolder, overwrite);
                    Console.Write("{0}: {1} -> {2}...", count, Path.GetFileName(fileName), outputFileName);
                    Ebu.Save(outputFileName, sub, true);
                    Console.WriteLine(" done.");
                }
            }
            if (!targetFormatFound)
            {
                var pac = new Pac();
                if (pac.Name.Replace(" ", string.Empty).Equals(toFormat, StringComparison.OrdinalIgnoreCase) || toFormat.Equals("pac", StringComparison.OrdinalIgnoreCase) || toFormat.Equals(".pac", StringComparison.OrdinalIgnoreCase))
                {
                    pac.BatchMode = true;
                    int codePage;
                    if (!string.IsNullOrEmpty(pacCodePage) && int.TryParse(pacCodePage, out codePage))
                        pac.CodePage = codePage;
                    targetFormatFound = true;
                    outputFileName = FormatOutputFileNameForBatchConvert(fileName, pac.Extension, outputFolder, overwrite);
                    Console.Write("{0}: {1} -> {2}...", count, Path.GetFileName(fileName), outputFileName);
                    pac.Save(outputFileName, sub);
                    Console.WriteLine(" done.");
                }
            }
            if (!targetFormatFound)
            {
                var cavena890 = new Cavena890();
                if (cavena890.Name.Replace(" ", string.Empty).Equals(toFormat, StringComparison.OrdinalIgnoreCase))
                {
                    targetFormatFound = true;
                    outputFileName = FormatOutputFileNameForBatchConvert(fileName, cavena890.Extension, outputFolder, overwrite);
                    Console.Write("{0}: {1} -> {2}...", count, Path.GetFileName(fileName), outputFileName);
                    cavena890.Save(outputFileName, sub);
                    Console.WriteLine(" done.");
                }
            }
            if (!targetFormatFound)
            {
                var cheetahCaption = new CheetahCaption();
                if (cheetahCaption.Name.Replace(" ", string.Empty).Equals(toFormat, StringComparison.OrdinalIgnoreCase))
                {
                    targetFormatFound = true;
                    outputFileName = FormatOutputFileNameForBatchConvert(fileName, cheetahCaption.Extension, outputFolder, overwrite);
                    Console.Write("{0}: {1} -> {2}...", count, Path.GetFileName(fileName), outputFileName);
                    CheetahCaption.Save(outputFileName, sub);
                    Console.WriteLine(" done.");
                }
            }
            if (!targetFormatFound)
            {
                var capMakerPlus = new CapMakerPlus();
                if (capMakerPlus.Name.Replace(" ", string.Empty).Equals(toFormat, StringComparison.OrdinalIgnoreCase))
                {
                    targetFormatFound = true;
                    outputFileName = FormatOutputFileNameForBatchConvert(fileName, capMakerPlus.Extension, outputFolder, overwrite);
                    Console.Write("{0}: {1} -> {2}...", count, Path.GetFileName(fileName), outputFileName);
                    CapMakerPlus.Save(outputFileName, sub);
                    Console.WriteLine(" done.");
                }
            }
            if (!targetFormatFound)
            {
                if (Configuration.Settings.Language.BatchConvert.PlainText == toFormat || Configuration.Settings.Language.BatchConvert.PlainText.Replace(" ", string.Empty).Equals(toFormat.Replace(" ", string.Empty), StringComparison.OrdinalIgnoreCase))
                {
                    targetFormatFound = true;
                    outputFileName = FormatOutputFileNameForBatchConvert(fileName, ".txt", outputFolder, overwrite);
                    Console.Write("{0}: {1} -> {2}...", count, Path.GetFileName(fileName), outputFileName);
                    File.WriteAllText(outputFileName, ExportText.GeneratePlainText(sub, false, false, false, false, false, false, string.Empty, true, false, true, true, false), targetEncoding);
                    Console.WriteLine(" done.");
                }
            }
            if (!targetFormatFound)
            {
                Console.WriteLine("{0}: {1} - target format '{2}' not found!", count, fileName, toFormat);
                errors++;
                return false;
            }
            else
            {
                converted++;
                return true;
            }
        }

        private static string FormatOutputFileNameForBatchConvert(string fileName, string extension, string outputFolder, bool overwrite)
        {
            string outputFileName = Path.ChangeExtension(fileName, extension);
            if (!string.IsNullOrEmpty(outputFolder))
                outputFileName = Path.Combine(outputFolder, Path.GetFileName(outputFileName));
            if (File.Exists(outputFileName) && !overwrite)
                outputFileName = Path.ChangeExtension(outputFileName, Guid.NewGuid() + extension);
            return outputFileName;
        }

        private void AudioWaveform_OnNonParagraphRightClicked(object sender, Nikse.SubtitleEdit.Controls.AudioVisualizer.ParagraphEventArgs e)
        {
            addParagraphHereToolStripMenuItem.Visible = false;
            addParagraphAndPasteToolStripMenuItem.Visible = false;
            deleteParagraphToolStripMenuItem.Visible = false;
            toolStripMenuItemFocusTextbox.Visible = !string.IsNullOrEmpty(Configuration.Settings.Language.Waveform.FocusTextBox); //TODO: Remove in 3.4
            splitToolStripMenuItem1.Visible = false;
            mergeWithPreviousToolStripMenuItem.Visible = false;
            mergeWithNextToolStripMenuItem.Visible = false;
            toolStripSeparator11.Visible = false;
            toolStripMenuItemWaveformPlaySelection.Visible = false;
            toolStripSeparator24.Visible = false;
            contextMenuStripWaveform.Show(MousePosition.X, MousePosition.Y);
        }

        private void AudioWaveform_OnDoubleClickNonParagraph(object sender, Nikse.SubtitleEdit.Controls.AudioVisualizer.ParagraphEventArgs e)
        {
            if (mediaPlayer.VideoPlayer != null)
            {
                _endSeconds = -1;
                if (e.Paragraph == null)
                {
                    if (Configuration.Settings.VideoControls.WaveformDoubleClickOnNonParagraphAction == "PlayPause")
                        mediaPlayer.TogglePlayPause();
                }
                else
                {
                    SubtitleListview1.SelectIndexAndEnsureVisible(_subtitle.GetIndex(e.Paragraph));
                }
            }
        }

        private void AudioWaveform_OnZoomedChanged(object sender, EventArgs e)
        {
            SelectZoomTextInComboBox();
        }

        private void AudioWaveform_OnTimeChangedAndOffsetRest(object sender, Nikse.SubtitleEdit.Controls.AudioVisualizer.ParagraphEventArgs e)
        {
            if (mediaPlayer.VideoPlayer == null)
                return;

            int index = _subtitle.Paragraphs.IndexOf(e.Paragraph);
            if (index < 0)
            {
                if (_subtitleAlternate != null && SubtitleListview1.IsAlternateTextColumnVisible && Configuration.Settings.General.ShowOriginalAsPreviewIfAvailable)
                {
                    index = _subtitleAlternate.GetIndex(e.Paragraph);
                    if (index >= 0)
                    {
                        var current = Utilities.GetOriginalParagraph(index, e.Paragraph, _subtitle.Paragraphs);
                        if (current != null)
                        {
                            index = _subtitle.Paragraphs.IndexOf(current);
                        }
                    }
                }
                else if (_subtitleAlternate != null && SubtitleListview1.IsAlternateTextColumnVisible)
                {
                    index = _subtitle.GetIndex(e.Paragraph);
                }
            }
            if (index >= 0)
            {
                SubtitleListview1.SelectIndexAndEnsureVisible(index);
                mediaPlayer.CurrentPosition = e.Seconds;
                ButtonSetStartAndOffsetRestClick(null, null);
            }
            audioVisualizer.Invalidate();
        }

        private void AudioWaveform_OnPause(object sender, EventArgs e)
        {
            _endSeconds = -1;
            if (mediaPlayer.VideoPlayer != null)
                mediaPlayer.Pause();
        }

        private void AudioWaveform_OnSingleClick(object sender, Nikse.SubtitleEdit.Controls.AudioVisualizer.ParagraphEventArgs e)
        {
            timerWaveform.Stop();
            _endSeconds = -1;
            if (mediaPlayer.VideoPlayer != null)
                mediaPlayer.Pause();

            mediaPlayer.CurrentPosition = e.Seconds;

            int index = -1;
            if (SubtitleListview1.SelectedItems.Count > 0)
                index = SubtitleListview1.SelectedItems[0].Index;
            SetWaveformPosition(audioVisualizer.StartPositionSeconds, e.Seconds, index);
            timerWaveform.Start();
        }

        private void AudioWaveform_OnParagraphRightClicked(object sender, Nikse.SubtitleEdit.Controls.AudioVisualizer.ParagraphEventArgs e)
        {
            SubtitleListview1.SelectIndexAndEnsureVisible(_subtitle.GetIndex(e.Paragraph));

            addParagraphHereToolStripMenuItem.Visible = false;
            addParagraphAndPasteToolStripMenuItem.Visible = false;
            deleteParagraphToolStripMenuItem.Visible = true;
            toolStripMenuItemFocusTextbox.Visible = true;
            splitToolStripMenuItem1.Visible = true;
            mergeWithPreviousToolStripMenuItem.Visible = true;
            mergeWithNextToolStripMenuItem.Visible = true;
            toolStripSeparator11.Visible = true;
            toolStripMenuItemWaveformPlaySelection.Visible = true;
            toolStripSeparator24.Visible = true;

            _audioWaveformRightClickSeconds = e.Seconds;
            contextMenuStripWaveform.Show(MousePosition.X, MousePosition.Y);
        }

        private void AudioWaveform_OnNewSelectionRightClicked(object sender, Nikse.SubtitleEdit.Controls.AudioVisualizer.ParagraphEventArgs e)
        {
            SubtitleListview1.SelectIndexAndEnsureVisible(_subtitle.GetIndex(e.Paragraph));

            addParagraphHereToolStripMenuItem.Visible = true;
            addParagraphAndPasteToolStripMenuItem.Visible = Clipboard.ContainsText();

            deleteParagraphToolStripMenuItem.Visible = false;
            toolStripMenuItemFocusTextbox.Visible = false;
            splitToolStripMenuItem1.Visible = false;
            mergeWithPreviousToolStripMenuItem.Visible = false;
            mergeWithNextToolStripMenuItem.Visible = false;

            contextMenuStripWaveform.Show(MousePosition.X, MousePosition.Y);
        }

        private void AudioWaveform_OnTimeChanged(object sender, Nikse.SubtitleEdit.Controls.AudioVisualizer.ParagraphEventArgs e)
        {
            var paragraph = e.Paragraph;
            var beforeParagraph = e.BeforeParagraph;
            if (beforeParagraph == null)
                beforeParagraph = paragraph;

            if (beforeParagraph.StartTime.TotalMilliseconds == paragraph.StartTime.TotalMilliseconds &&
                beforeParagraph.EndTime.TotalMilliseconds == paragraph.EndTime.TotalMilliseconds)
                _makeHistoryPaused = true;

            int selectedIndex = FirstSelectedIndex;
            int index = _subtitle.Paragraphs.IndexOf(paragraph);
            if (index == _subtitleListViewIndex)
            {
                // Make history item for rollback (change paragraph back for history + change again)
                _subtitle.Paragraphs[index] = new Paragraph(beforeParagraph);
                MakeHistoryForUndoOnlyIfNotResent(string.Format(_language.VideoControls.BeforeChangingTimeInWaveformX, "#" + paragraph.Number + " " + paragraph.Text));
                _subtitle.Paragraphs[index] = paragraph;
                _makeHistoryPaused = true;

                Paragraph original = null;
                if (_subtitleAlternate != null && SubtitleListview1.IsAlternateTextColumnVisible)
                    original = Utilities.GetOriginalParagraph(index, beforeParagraph, _subtitleAlternate.Paragraphs);

                if (Configuration.Settings.General.UseTimeFormatHHMMSSFF)
                { // so we don't get weird rounds we'll use whole frames when moving start time
                    double fr = 1000.0 / Configuration.Settings.General.CurrentFrameRate;
                    if (e.BeforeParagraph != null && e.BeforeParagraph.StartTime.TotalMilliseconds != e.Paragraph.StartTime.TotalMilliseconds &&
                                                     e.BeforeParagraph.Duration.TotalMilliseconds == e.Paragraph.Duration.TotalMilliseconds)
                    {
                        // move paragraph
                        paragraph.StartTime.TotalMilliseconds = ((int)Math.Round(paragraph.StartTime.TotalMilliseconds / fr)) * fr;
                        paragraph.EndTime.TotalMilliseconds = paragraph.StartTime.TotalMilliseconds + e.BeforeParagraph.Duration.TotalMilliseconds;
                    }
                    else if (e.BeforeParagraph != null && e.BeforeParagraph.EndTime.TotalMilliseconds == e.Paragraph.EndTime.TotalMilliseconds)
                    {
                        paragraph.EndTime.TotalMilliseconds = ((int)Math.Round(paragraph.EndTime.TotalMilliseconds / fr)) * fr;
                        int end = SubtitleFormat.MillisecondsToFrames(paragraph.EndTime.TotalMilliseconds);
                        int dur = SubtitleFormat.MillisecondsToFrames(paragraph.Duration.TotalMilliseconds);
                        paragraph.StartTime.TotalMilliseconds = SubtitleFormat.FramesToMilliseconds(end - dur);
                    }
                }

                timeUpDownStartTime.TimeCode = paragraph.StartTime;
                decimal durationInSeconds = (decimal)(paragraph.Duration.TotalSeconds);
                if (durationInSeconds >= numericUpDownDuration.Minimum && durationInSeconds <= numericUpDownDuration.Maximum)
                    SetDurationInSeconds((double)durationInSeconds);

                MovePrevNext(e, beforeParagraph, index);

                if (original != null)
                {
                    original.StartTime.TotalMilliseconds = paragraph.StartTime.TotalMilliseconds;
                    original.EndTime.TotalMilliseconds = paragraph.EndTime.TotalMilliseconds;
                }
            }
            else
            {
                if (_subtitleAlternate != null && SubtitleListview1.IsAlternateTextColumnVisible && Configuration.Settings.General.ShowOriginalAsPreviewIfAvailable)
                {
                    index = _subtitleAlternate.GetIndex(paragraph);
                    if (index >= 0)
                    {
                        // Make history item for rollback (change paragraph back for history + change again)
                        _subtitleAlternate.Paragraphs[index] = new Paragraph(beforeParagraph);
                        MakeHistoryForUndoOnlyIfNotResent(string.Format(_language.VideoControls.BeforeChangingTimeInWaveformX, "#" + paragraph.Number + " " + paragraph.Text));
                        _subtitleAlternate.Paragraphs[index] = paragraph;
                        _makeHistoryPaused = true;

                        var current = Utilities.GetOriginalParagraph(index, beforeParagraph, _subtitle.Paragraphs);
                        if (current != null)
                        {
                            current.StartTime.TotalMilliseconds = paragraph.StartTime.TotalMilliseconds;
                            current.EndTime.TotalMilliseconds = paragraph.EndTime.TotalMilliseconds;

                            index = _subtitle.GetIndex(current);

                            SubtitleListview1.SetStartTimeAndDuration(index, paragraph);

                            if (index == selectedIndex)
                            {
                                timeUpDownStartTime.TimeCode = paragraph.StartTime;
                                decimal durationInSeconds = (decimal)(paragraph.Duration.TotalSeconds);
                                if (durationInSeconds >= numericUpDownDuration.Minimum && durationInSeconds <= numericUpDownDuration.Maximum)
                                    SetDurationInSeconds((double)durationInSeconds);
                            }
                        }
                    }
                }
                else if (_subtitleAlternate != null && SubtitleListview1.IsAlternateTextColumnVisible)
                {
                    index = _subtitle.GetIndex(paragraph);
                    if (index >= 0)
                    {
                        // Make history item for rollback (change paragraph back for history + change again)
                        _subtitle.Paragraphs[index] = new Paragraph(beforeParagraph);
                        MakeHistoryForUndoOnlyIfNotResent(string.Format(_language.VideoControls.BeforeChangingTimeInWaveformX, "#" + paragraph.Number + " " + paragraph.Text));
                        _subtitle.Paragraphs[index] = paragraph;
                        _makeHistoryPaused = true;

                        var original = Utilities.GetOriginalParagraph(index, beforeParagraph, _subtitleAlternate.Paragraphs);
                        if (original != null)
                        {
                            original.StartTime.TotalMilliseconds = paragraph.StartTime.TotalMilliseconds;
                            original.EndTime.TotalMilliseconds = paragraph.EndTime.TotalMilliseconds;
                        }
                        SubtitleListview1.SetStartTimeAndDuration(index, paragraph);
                    }
                }
                else
                {
                    if (index >= 0)
                    {
                        // Make history item for rollback (change paragraph back for history + change again)
                        _subtitle.Paragraphs[index] = new Paragraph(beforeParagraph);
                        MakeHistoryForUndoOnlyIfNotResent(string.Format(_language.VideoControls.BeforeChangingTimeInWaveformX, "#" + paragraph.Number + " " + paragraph.Text));
                        _subtitle.Paragraphs[index] = paragraph;
                        _makeHistoryPaused = true;

                        MovePrevNext(e, beforeParagraph, index);
                    }

                    SubtitleListview1.SetStartTimeAndDuration(index, paragraph);
                }
            }
            beforeParagraph.StartTime.TotalMilliseconds = paragraph.StartTime.TotalMilliseconds;
            beforeParagraph.EndTime.TotalMilliseconds = paragraph.EndTime.TotalMilliseconds;
            _makeHistoryPaused = false;
        }

        private void MovePrevNext(Nikse.SubtitleEdit.Controls.AudioVisualizer.ParagraphEventArgs e, Paragraph beforeParagraph, int index)
        {
            if (e.MovePreviousOrNext)
            {
                if (e.MouseDownParagraphType == AudioVisualizer.MouseDownParagraphType.Start)
                {
                    var prev = _subtitle.GetParagraphOrDefault(index - 1);
                    if (prev != null)
                    {
                        prev.EndTime.TotalMilliseconds = prev.EndTime.TotalMilliseconds + (e.Paragraph.StartTime.TotalMilliseconds - beforeParagraph.StartTime.TotalMilliseconds);
                        SubtitleListview1.SetStartTimeAndDuration(index - 1, prev);
                        audioVisualizer.Invalidate();
                    }
                }
                else if (e.MouseDownParagraphType == AudioVisualizer.MouseDownParagraphType.End)
                {
                    var next = _subtitle.GetParagraphOrDefault(index + 1);
                    if (next != null)
                    {
                        next.StartTime.TotalMilliseconds = next.StartTime.TotalMilliseconds + (e.Paragraph.EndTime.TotalMilliseconds - beforeParagraph.EndTime.TotalMilliseconds);
                        SubtitleListview1.SetStartTimeAndDuration(index + 1, next);
                        audioVisualizer.Invalidate();
                    }
                }
            }
        }

        private void AudioWaveform_OnPositionSelected(object sender, Nikse.SubtitleEdit.Controls.AudioVisualizer.ParagraphEventArgs e)
        {
            mediaPlayer.CurrentPosition = e.Seconds;
            if (e.Paragraph != null)
                SubtitleListview1.SelectIndexAndEnsureVisible(_subtitle.GetIndex(e.Paragraph));
        }

        private void VideoPositionChanged(object sender, EventArgs e)
        {
            var tud = (TimeUpDown)sender;
            if (tud.Enabled)
            {
                mediaPlayer.CurrentPosition = tud.TimeCode.TotalSeconds;
            }
        }

        private void Main_Load(object sender, EventArgs e)
        {
            splitContainer1.Panel1MinSize = 525;
            splitContainer1.Panel2MinSize = 250;
            splitContainerMain.Panel1MinSize = 200;
            splitContainerMain.Panel2MinSize = 220;

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

            if (Environment.OSVersion.Version.Major < 6 && Configuration.Settings.General.SubtitleFontName == Utilities.WinXP2KUnicodeFontName) // 6 == Vista/Win2008Server/Win7
            {
                //const string unicodeFontName = Utilities.WinXp2kUnicodeFontName;
                //Configuration.Settings.General.SubtitleFontName = unicodeFontName;
                //float fontSize = toolStripMenuItemInsertUnicodeSymbol.Font.Size;
                //textBoxSource.Font = new Font(unicodeFontName, fontSize);
                //textBoxListViewText.Font = new Font(unicodeFontName, fontSize);
                //SubtitleListview1.Font = new Font(unicodeFontName, fontSize);
                //toolStripWaveControls.RenderMode = ToolStripRenderMode.System;
                //toolStripMenuItemSurroundWithMusicSymbols.Font = new Font(unicodeFontName, fontSize);
                Utilities.InitializeSubtitleFont(SubtitleListview1);
                Refresh();
            }

        }

        private void InitializeLanguage()
        {
            fileToolStripMenuItem.Text = _language.Menu.File.Title;
            newToolStripMenuItem.Text = _language.Menu.File.New;
            openToolStripMenuItem.Text = _language.Menu.File.Open;
            toolStripMenuItemOpenKeepVideo.Text = _language.Menu.File.OpenKeepVideo;
            reopenToolStripMenuItem.Text = _language.Menu.File.Reopen;
            saveToolStripMenuItem.Text = _language.Menu.File.Save;
            saveAsToolStripMenuItem.Text = _language.Menu.File.SaveAs;
            toolStripMenuItemRestoreAutoBackup.Text = _language.Menu.File.RestoreAutoBackup;
            openOriginalToolStripMenuItem.Text = _language.Menu.File.OpenOriginal;
            saveOriginalToolStripMenuItem.Text = _language.Menu.File.SaveOriginal;
            saveOriginalAstoolStripMenuItem.Text = _language.SaveOriginalSubtitleAs;
            removeOriginalToolStripMenuItem.Text = _language.Menu.File.CloseOriginal;

            toolStripMenuItemOpenContainingFolder.Text = _language.Menu.File.OpenContainingFolder;
            toolStripMenuItemCompare.Text = _language.Menu.File.Compare;
            toolStripMenuItemStatistics.Text = _language.Menu.File.Statistics;
            toolStripMenuItemPlugins.Text = _language.Menu.File.Plugins;
            toolStripMenuItemImportDvdSubtitles.Text = _language.Menu.File.ImportOcrFromDvd;
            toolStripMenuItemSubIdx.Text = _language.Menu.File.ImportOcrVobSubSubtitle;
            toolStripButtonGetFrameRate.ToolTipText = _language.GetFrameRateFromVideoFile;
            toolStripMenuItemImportBluRaySup.Text = _language.Menu.File.ImportBluRaySupFile;
            if (!string.IsNullOrEmpty(_language.Menu.File.ImportXSub)) //TODO: Fix in 3.4
                toolStripMenuItemImportXSub.Text = _language.Menu.File.ImportXSub;
            matroskaImportStripMenuItem.Text = _language.Menu.File.ImportSubtitleFromMatroskaFile;
            toolStripMenuItemManualAnsi.Text = _language.Menu.File.ImportSubtitleWithManualChosenEncoding;
            toolStripMenuItemImportText.Text = _language.Menu.File.ImportText;
            if (!string.IsNullOrEmpty(_language.Menu.File.ImportImages))
                toolStripMenuItemImportImages.Text = _language.Menu.File.ImportImages;
            else
                toolStripMenuItemImportImages.Visible = false;
            toolStripMenuItemImportTimeCodes.Text = _language.Menu.File.ImportTimecodes;
            toolStripMenuItemExport.Text = _language.Menu.File.Export;
            toolStripMenuItemExportPngXml.Text = _language.Menu.File.ExportBdnXml;
            bluraySupToolStripMenuItem.Text = _language.Menu.File.ExportBluRaySup;
            adobeEncoreFABImageScriptToolStripMenuItem.Text = _language.Menu.File.ExportAdobeEncoreFabImageScript;
            toolStripMenuItemTextTimeCodePair.Text = _language.Menu.File.ExportKoreanAtsFilePair;
            vobSubsubidxToolStripMenuItem.Text = _language.Menu.File.ExportVobSub;
            toolStripMenuItemCavena890.Text = _language.Menu.File.ExportCavena890;
            eBUSTLToolStripMenuItem.Text = _language.Menu.File.ExportEbu;
            pACScreenElectronicsToolStripMenuItem.Text = _language.Menu.File.ExportPac;
            plainTextToolStripMenuItem.Text = _language.Menu.File.ExportPlainText;
            toolStripMenuItemAvidStl.Text = _language.Menu.File.ExportAvidStl;
            toolStripMenuItemExportCapMakerPlus.Text = _language.Menu.File.ExportCapMakerPlus;
            toolStripMenuItemExportCaptionInc.Text = _language.Menu.File.ExportCaptionsInc;
            toolStripMenuItemExportCheetahCap.Text = _language.Menu.File.ExportCheetahCap;
            toolStripMenuItemExportUltech130.Text = _language.Menu.File.ExportUltech130;
            if (!string.IsNullOrEmpty(_language.Menu.File.ExportCustomTextFormat))
                exportCustomTextFormatToolStripMenuItem.Text = _language.Menu.File.ExportCustomTextFormat; //TODO: Fix in 3.4
            exitToolStripMenuItem.Text = _language.Menu.File.Exit;

            editToolStripMenuItem.Text = _language.Menu.Edit.Title;
            showHistoryforUndoToolStripMenuItem.Text = _language.Menu.Edit.ShowUndoHistory;
            toolStripMenuItemUndo.Text = _language.Menu.Edit.Undo;
            toolStripMenuItemRedo.Text = _language.Menu.Edit.Redo;

            toolStripMenuItemInsertUnicodeCharacter.Text = _language.Menu.Edit.InsertUnicodeSymbol;

            findToolStripMenuItem.Text = _language.Menu.Edit.Find;
            findNextToolStripMenuItem.Text = _language.Menu.Edit.FindNext;
            replaceToolStripMenuItem.Text = _language.Menu.Edit.Replace;
            multipleReplaceToolStripMenuItem.Text = _language.Menu.Edit.MultipleReplace;
            gotoLineNumberToolStripMenuItem.Text = _language.Menu.Edit.GoToSubtitleNumber;
            toolStripMenuItemRightToLeftMode.Text = _language.Menu.Edit.RightToLeftMode;

            if (!string.IsNullOrEmpty(_language.Menu.Edit.FixTrlViaUnicodeControlCharacters)) // TODO: Fix in 3.4
                toolStripMenuItemRtlUnicodeControlChars.Text = _language.Menu.Edit.FixTrlViaUnicodeControlCharacters;

            toolStripMenuItemReverseRightToLeftStartEnd.Text = _language.Menu.Edit.ReverseRightToLeftStartEnd;
            if (!string.IsNullOrEmpty(_language.Menu.Edit.ModifySelection))
                toolStripMenuItemModifySelection.Text = _language.Menu.Edit.ModifySelection;
            if (!string.IsNullOrEmpty(_language.Menu.Edit.InverseSelection))
                toolStripMenuItemInverseSelection.Text = _language.Menu.Edit.InverseSelection;
            editSelectAllToolStripMenuItem.Text = _language.Menu.ContextMenu.SelectAll;

            toolsToolStripMenuItem.Text = _language.Menu.Tools.Title;
            adjustDisplayTimeToolStripMenuItem.Text = _language.Menu.Tools.AdjustDisplayDuration;
            toolStripMenuItemApplyDurationLimits.Text = _language.Menu.Tools.ApplyDurationLimits;
            toolStripMenuItemDurationBridgeGaps.Text = _language.Menu.Tools.DurationsBridgeGap;
            toolStripMenuItemDurationBridgeGaps.Visible = !string.IsNullOrEmpty(_language.Menu.Tools.DurationsBridgeGap); //TODO: Remove in SE 3.4
            fixToolStripMenuItem.Text = _language.Menu.Tools.FixCommonErrors;
            startNumberingFromToolStripMenuItem.Text = _language.Menu.Tools.StartNumberingFrom;
            removeTextForHearImparedToolStripMenuItem.Text = _language.Menu.Tools.RemoveTextForHearingImpaired;
            ChangeCasingToolStripMenuItem.Text = _language.Menu.Tools.ChangeCasing;
            toolStripMenuItemChangeFrameRate2.Text = _language.Menu.Tools.ChangeFrameRate;
            changeSpeedInPercentToolStripMenuItem.Text = _language.Menu.Tools.ChangeSpeedInPercent;
            toolStripMenuItemAutoMergeShortLines.Text = _language.Menu.Tools.MergeShortLines;
            toolStripMenuItemMergeDuplicateText.Text = _language.Menu.Tools.MergeDuplicateText;
            toolStripMenuItemMergeLinesWithSameTimeCodes.Text = _language.Menu.Tools.MergeSameTimeCodes;
            if (string.IsNullOrEmpty(_language.Menu.Tools.MergeSameTimeCodes))
                toolStripMenuItemMergeLinesWithSameTimeCodes.Visible = false;
            toolStripMenuItemMergeDuplicateText.Visible = !string.IsNullOrEmpty(_language.Menu.Tools.MergeDuplicateText);
            toolStripMenuItemAutoSplitLongLines.Text = _language.Menu.Tools.SplitLongLines;
            setMinimumDisplayTimeBetweenParagraphsToolStripMenuItem.Text = _language.Menu.Tools.MinimumDisplayTimeBetweenParagraphs;
            toolStripMenuItem1.Text = _language.Menu.Tools.SortBy;

            if (!string.IsNullOrEmpty(_language.Menu.Tools.Number))
                sortNumberToolStripMenuItem.Text = _language.Menu.Tools.Number;
            else
                sortNumberToolStripMenuItem.Text = _languageGeneral.Number;

            if (!string.IsNullOrEmpty(_language.Menu.Tools.StartTime))
                sortStartTimeToolStripMenuItem.Text = _language.Menu.Tools.StartTime;
            else
                sortStartTimeToolStripMenuItem.Text = _languageGeneral.StartTime;

            if (!string.IsNullOrEmpty(_language.Menu.Tools.EndTime))
                sortEndTimeToolStripMenuItem.Text = _language.Menu.Tools.EndTime;
            else
                sortEndTimeToolStripMenuItem.Text = _languageGeneral.EndTime;

            if (!string.IsNullOrEmpty(_language.Menu.Tools.Duration))
                sortDisplayTimeToolStripMenuItem.Text = _language.Menu.Tools.Duration;
            else
                sortDisplayTimeToolStripMenuItem.Text = _languageGeneral.Duration;

            if (string.IsNullOrEmpty(_language.Menu.Tools.Duration))
            {
                toolStripSeparatorAscOrDesc.Visible = false;
                descendingToolStripMenuItem.Visible = false;
                AscendingToolStripMenuItem.Visible = false;
            }
            else
            {
                descendingToolStripMenuItem.Text = _language.Menu.Tools.Descending;
                AscendingToolStripMenuItem.Text = _language.Menu.Tools.Ascending;
            }

            sortTextAlphabeticallytoolStripMenuItem.Text = _language.Menu.Tools.TextAlphabetically;
            sortTextMaxLineLengthToolStripMenuItem.Text = _language.Menu.Tools.TextSingleLineMaximumLength;
            sortTextTotalLengthToolStripMenuItem.Text = _language.Menu.Tools.TextTotalLength;
            sortTextNumberOfLinesToolStripMenuItem.Text = _language.Menu.Tools.TextNumberOfLines;
            textCharssecToolStripMenuItem.Text = _language.Menu.Tools.TextNumberOfCharactersPerSeconds;
            textWordsPerMinutewpmToolStripMenuItem.Text = _language.Menu.Tools.WordsPerMinute;
            styleToolStripMenuItem.Text = _language.Menu.Tools.Style;
            if (string.IsNullOrEmpty(styleToolStripMenuItem.Text))
                styleToolStripMenuItem.Text = "Style"; //TODO: Fix in SE 3.4

            toolStripMenuItemShowOriginalInPreview.Text = _language.Menu.Edit.ShowOriginalTextInAudioAndVideoPreview;
            toolStripMenuItemMakeEmptyFromCurrent.Text = _language.Menu.Tools.MakeNewEmptyTranslationFromCurrentSubtitle;
            toolStripMenuItemBatchConvert.Text = _language.Menu.Tools.BatchConvert;
            if (!string.IsNullOrEmpty(_language.Menu.Tools.GenerateTimeAsText)) //TODO: Fix in SE 3.4
                generateDatetimeInfoFromVideoToolStripMenuItem.Text = _language.Menu.Tools.GenerateTimeAsText;
            if (!string.IsNullOrEmpty(_language.Menu.Tools.MeasurementConverter)) //TODO: Fix in SE 3.4
                toolStripMenuItemMeasurementConverter.Text = _language.Menu.Tools.MeasurementConverter;
            splitToolStripMenuItem.Text = _language.Menu.Tools.SplitSubtitle;
            appendTextVisuallyToolStripMenuItem.Text = _language.Menu.Tools.AppendSubtitle;
            joinSubtitlesToolStripMenuItem.Text = _language.Menu.Tools.JoinSubtitles;

            toolStripMenuItemVideo.Text = _language.Menu.Video.Title;
            openVideoToolStripMenuItem.Text = _language.Menu.Video.OpenVideo;
            if (!string.IsNullOrEmpty(_language.Menu.Video.OpenDvd))
                toolStripMenuItemOpenDvd.Text = _language.Menu.Video.OpenDvd; //TODO: Remove in SE 3.4
            toolStripMenuItemSetAudioTrack.Text = _language.Menu.Video.ChooseAudioTrack;
            closeVideoToolStripMenuItem.Text = _language.Menu.Video.CloseVideo;

            if (!string.IsNullOrEmpty(_language.Menu.Video.ImportSceneChanges)) //TODO: Remove in SE 3.4
                toolStripMenuItemImportSceneChanges.Text = _language.Menu.Video.ImportSceneChanges;

            if (Configuration.Settings.VideoControls.GenerateSpectrogram)
                showhideWaveformToolStripMenuItem.Text = _language.Menu.Video.ShowHideWaveformAndSpectrogram;
            else
                showhideWaveformToolStripMenuItem.Text = _language.Menu.Video.ShowHideWaveform;

            showhideVideoToolStripMenuItem.Text = _language.Menu.Video.ShowHideVideo;
            undockVideoControlsToolStripMenuItem.Text = _language.Menu.Video.UnDockVideoControls;
            redockVideoControlsToolStripMenuItem.Text = _language.Menu.Video.ReDockVideoControls;

            toolStripMenuItemSpellCheckMain.Text = _language.Menu.SpellCheck.Title;
            spellCheckToolStripMenuItem.Text = _language.Menu.SpellCheck.SpellCheck;
            if (!string.IsNullOrEmpty(_language.Menu.SpellCheck.SpellCheckFromCurrentLine))
                toolStripMenuItemSpellCheckFromCurrentLine.Text = _language.Menu.SpellCheck.SpellCheckFromCurrentLine;
            findDoubleWordsToolStripMenuItem.Text = _language.Menu.SpellCheck.FindDoubleWords;
            FindDoubleLinesToolStripMenuItem.Text = _language.Menu.SpellCheck.FindDoubleLines;
            GetDictionariesToolStripMenuItem.Text = _language.Menu.SpellCheck.GetDictionaries;
            addWordToNamesetcListToolStripMenuItem.Text = _language.Menu.SpellCheck.AddToNamesEtcList;

            toolStripMenuItemSynchronization.Text = _language.Menu.Synchronization.Title;
            toolStripMenuItemAdjustAllTimes.Text = _language.Menu.Synchronization.AdjustAllTimes;
            visualSyncToolStripMenuItem.Text = _language.Menu.Synchronization.VisualSync;
            toolStripMenuItemPointSync.Text = _language.Menu.Synchronization.PointSync;
            pointSyncViaOtherSubtitleToolStripMenuItem.Text = _language.Menu.Synchronization.PointSyncViaOtherSubtitle;

            toolStripMenuItemAutoTranslate.Text = _language.Menu.AutoTranslate.Title;
            translateByGoogleToolStripMenuItem.Text = _language.Menu.AutoTranslate.TranslatePoweredByGoogle;
            translatepoweredByMicrosoftToolStripMenuItem.Text = _language.Menu.AutoTranslate.TranslatePoweredByMicrosoft;
            translatepoweredByMicrosoftToolStripMenuItem.Visible = Configuration.Settings.Tools.MicrosoftBingApiId != "C2C2E9A508E6748F0494D68DFD92FAA1FF9B0BA4";
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

            toolStripMenuItemNetworking.Text = _language.Menu.Networking.Title;
            startServerToolStripMenuItem.Text = _language.Menu.Networking.StartNewSession;
            joinSessionToolStripMenuItem.Text = _language.Menu.Networking.JoinSession;
            showSessionKeyLogToolStripMenuItem.Text = _language.Menu.Networking.ShowSessionInfoAndLog;
            chatToolStripMenuItem.Text = _language.Menu.Networking.Chat;
            leaveSessionToolStripMenuItem.Text = _language.Menu.Networking.LeaveSession;

            checkForUpdatesToolStripMenuItem.Text = _language.Menu.Help.CheckForUpdates;
            helpToolStripMenuItem.Text = _language.Menu.Help.Title;
            helpToolStripMenuItem1.Text = _language.Menu.Help.Help;
            aboutToolStripMenuItem.Text = _language.Menu.Help.About;

            toolStripButtonFileNew.ToolTipText = _language.Menu.ToolBar.New;
            toolStripButtonFileOpen.ToolTipText = _language.Menu.ToolBar.Open;
            toolStripButtonSave.ToolTipText = _language.Menu.ToolBar.Save;
            toolStripButtonSaveAs.ToolTipText = _language.Menu.ToolBar.SaveAs;
            toolStripButtonFind.ToolTipText = _language.Menu.ToolBar.Find;
            toolStripButtonReplace.ToolTipText = _language.Menu.ToolBar.Replace;
            toolStripButtonFixCommonErrors.ToolTipText = Configuration.Settings.Language.Settings.FixCommonerrors; // TODO: 3.4 Use Toolbar translation tag
            toolStripButtonVisualSync.ToolTipText = _language.Menu.ToolBar.VisualSync;
            toolStripButtonSpellCheck.ToolTipText = _language.Menu.ToolBar.SpellCheck;
            toolStripButtonSettings.ToolTipText = _language.Menu.ToolBar.Settings;
            toolStripButtonHelp.ToolTipText = _language.Menu.ToolBar.Help;
            toolStripButtonToggleWaveform.ToolTipText = _language.Menu.ToolBar.ShowHideWaveform;
            toolStripButtonToggleVideo.ToolTipText = _language.Menu.ToolBar.ShowHideVideo;

            toolStripMenuItemAssStyles.Text = _language.Menu.ContextMenu.SubStationAlphaStyles;
            setStylesForSelectedLinesToolStripMenuItem.Text = _language.Menu.ContextMenu.SubStationAlphaSetStyle;

            toolStripMenuItemDelete.Text = _language.Menu.ContextMenu.Delete;
            insertLineToolStripMenuItem.Text = _language.Menu.ContextMenu.InsertFirstLine;
            toolStripMenuItemInsertBefore.Text = _language.Menu.ContextMenu.InsertBefore;
            toolStripMenuItemInsertAfter.Text = _language.Menu.ContextMenu.InsertAfter;
            toolStripMenuItemInsertSubtitle.Text = _language.Menu.ContextMenu.InsertSubtitleAfter;

            toolStripMenuItemCopySourceText.Text = _language.Menu.ContextMenu.CopyToClipboard;

            toolStripMenuItemColumn.Text = _language.Menu.ContextMenu.Column;
            columnDeleteTextOnlyToolStripMenuItem.Text = _language.Menu.ContextMenu.ColumnDeleteText;
            toolStripMenuItemColumnDeleteText.Text = _language.Menu.ContextMenu.ColumnDeleteTextAndShiftCellsUp;
            ShiftTextCellsDownToolStripMenuItem.Text = _language.Menu.ContextMenu.ColumnInsertEmptyTextAndShiftCellsDown;
            toolStripMenuItemInsertTextFromSub.Text = _language.Menu.ContextMenu.ColumnInsertTextFromSubtitle;
            toolStripMenuItemColumnImportText.Text = _language.Menu.ContextMenu.ColumnImportTextAndShiftCellsDown;
            toolStripMenuItemPasteSpecial.Text = _language.Menu.ContextMenu.ColumnPasteFromClipboard;
            copyOriginalTextToCurrentToolStripMenuItem.Text = _language.Menu.ContextMenu.ColumnCopyOriginalTextToCurrent;

            splitLineToolStripMenuItem.Text = _language.Menu.ContextMenu.Split;
            toolStripMenuItemMergeLines.Text = _language.Menu.ContextMenu.MergeSelectedLines;
            toolStripMenuItemMergeDialog.Text = _language.Menu.ContextMenu.MergeSelectedLinesAsDialog;
            mergeBeforeToolStripMenuItem.Text = _language.Menu.ContextMenu.MergeWithLineBefore;
            mergeAfterToolStripMenuItem.Text = _language.Menu.ContextMenu.MergeWithLineAfter;
            normalToolStripMenuItem.Text = _language.Menu.ContextMenu.Normal;
            boldToolStripMenuItem.Text = _languageGeneral.Bold;
            underlineToolStripMenuItem.Text = _language.Menu.ContextMenu.Underline;
            italicToolStripMenuItem.Text = _languageGeneral.Italic;
            colorToolStripMenuItem.Text = _language.Menu.ContextMenu.Color;
            toolStripMenuItemFont.Text = _language.Menu.ContextMenu.FontName;
            toolStripMenuItemAlignment.Text = _language.Menu.ContextMenu.Alignment;
            toolStripMenuItemAutoBreakLines.Text = _language.Menu.ContextMenu.AutoBalanceSelectedLines;
            toolStripMenuItemUnbreakLines.Text = _language.Menu.ContextMenu.RemoveLineBreaksFromSelectedLines;
            typeEffectToolStripMenuItem.Text = _language.Menu.ContextMenu.TypewriterEffect;
            karokeeEffectToolStripMenuItem.Text = _language.Menu.ContextMenu.KaraokeEffect;
            showSelectedLinesEarlierlaterToolStripMenuItem.Text = _language.Menu.ContextMenu.ShowSelectedLinesEarlierLater;
            visualSyncSelectedLinesToolStripMenuItem.Text = _language.Menu.ContextMenu.VisualSyncSelectedLines;
            toolStripMenuItemGoogleMicrosoftTranslateSelLine.Text = _language.Menu.ContextMenu.GoogleAndMicrosoftTranslateSelectedLine;
            googleTranslateSelectedLinesToolStripMenuItem.Text = _language.Menu.ContextMenu.GoogleTranslateSelectedLines;
            adjustDisplayTimeForSelectedLinesToolStripMenuItem.Text = _language.Menu.ContextMenu.AdjustDisplayDurationForSelectedLines;
            fixCommonErrorsInSelectedLinesToolStripMenuItem.Text = _language.Menu.ContextMenu.FixCommonErrorsInSelectedLines;
            changeCasingForSelectedLinesToolStripMenuItem.Text = _language.Menu.ContextMenu.ChangeCasingForSelectedLines;
            toolStripMenuItemSaveSelectedLines.Text = _language.Menu.ContextMenu.SaveSelectedLines;

            // textbox context menu
            cutToolStripMenuItem.Text = _language.Menu.ContextMenu.Cut;
            copyToolStripMenuItem.Text = _language.Menu.ContextMenu.Copy;
            pasteToolStripMenuItem.Text = _language.Menu.ContextMenu.Paste;
            deleteToolStripMenuItem.Text = _language.Menu.ContextMenu.Delete;
            toolStripMenuItemSplitTextAtCursor.Text = _language.Menu.ContextMenu.SplitLineAtCursorPosition;
            selectAllToolStripMenuItem.Text = _language.Menu.ContextMenu.SelectAll;
            normalToolStripMenuItem1.Text = _language.Menu.ContextMenu.Normal;
            boldToolStripMenuItem1.Text = _languageGeneral.Bold;
            italicToolStripMenuItem1.Text = _languageGeneral.Italic;
            underlineToolStripMenuItem1.Text = _language.Menu.ContextMenu.Underline;
            colorToolStripMenuItem1.Text = _language.Menu.ContextMenu.Color;
            fontNameToolStripMenuItem.Text = _language.Menu.ContextMenu.FontName;
            toolStripMenuItemInsertUnicodeSymbol.Text = _language.Menu.Edit.InsertUnicodeSymbol;

            // main controls
            SubtitleListview1.InitializeLanguage(_languageGeneral, Configuration.Settings);
            toolStripLabelSubtitleFormat.Text = _language.Controls.SubtitleFormat;
            toolStripLabelEncoding.Text = _language.Controls.FileEncoding;
            tabControlSubtitle.TabPages[0].Text = _language.Controls.ListView;
            tabControlSubtitle.TabPages[1].Text = _language.Controls.SourceView;
            labelDuration.Text = _languageGeneral.Duration;
            labelStartTime.Text = _languageGeneral.StartTime;
            labelText.Text = _languageGeneral.Text;
            labelAlternateText.Text = Configuration.Settings.Language.General.OriginalText;
            toolStripLabelFrameRate.Text = _languageGeneral.FrameRate;
            buttonPrevious.Text = _language.Controls.Previous;
            buttonNext.Text = _language.Controls.Next;
            buttonAutoBreak.Text = _language.Controls.AutoBreak;
            buttonUnBreak.Text = _language.Controls.Unbreak;
            buttonSplitLine.Text = _languageGeneral.SplitLine;
            ShowSourceLineNumber();

            // Video controls
            tabPageTranslate.Text = _language.VideoControls.Translate + "  ";
            tabPageCreate.Text = _language.VideoControls.Create + "  ";
            tabPageAdjust.Text = _language.VideoControls.Adjust + "  ";
            checkBoxSyncListViewWithVideoWhilePlaying.Text = _language.VideoControls.SelectCurrentElementWhilePlaying;
            if (_videoFileName == null)
                labelVideoInfo.Text = Configuration.Settings.Language.General.NoVideoLoaded;
            toolStripButtonLockCenter.Text = _language.VideoControls.Center;
            toolStripSplitButtonPlayRate.Text = _language.VideoControls.PlayRate;
            toolStripMenuItemPlayRateSlow.Text = _language.VideoControls.Slow;
            toolStripMenuItemPlayRateNormal.Text = _language.VideoControls.Normal;
            toolStripMenuItemPlayRateFast.Text = _language.VideoControls.Fast;
            toolStripMenuItemPlayRateVeryFast.Text = _language.VideoControls.VeryFast;

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
            labelVideoPosition2.Text = _language.VideoControls.VideoPosition;
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
            addParagraphHereToolStripMenuItem.Text = Configuration.Settings.Language.Waveform.AddParagraphHere;
            addParagraphAndPasteToolStripMenuItem.Text = Configuration.Settings.Language.Waveform.AddParagraphHereAndPasteText;
            deleteParagraphToolStripMenuItem.Text = Configuration.Settings.Language.Waveform.DeleteParagraph;
            toolStripMenuItemFocusTextbox.Text = Configuration.Settings.Language.Waveform.FocusTextBox;

            splitToolStripMenuItem1.Text = Configuration.Settings.Language.Waveform.Split;
            mergeWithPreviousToolStripMenuItem.Text = Configuration.Settings.Language.Waveform.MergeWithPrevious;
            mergeWithNextToolStripMenuItem.Text = Configuration.Settings.Language.Waveform.MergeWithNext;
            toolStripMenuItemWaveformPlaySelection.Text = Configuration.Settings.Language.Waveform.PlaySelection;
            showWaveformAndSpectrogramToolStripMenuItem.Text = Configuration.Settings.Language.Waveform.ShowWaveformAndSpectrogram;
            showOnlyWaveformToolStripMenuItem.Text = Configuration.Settings.Language.Waveform.ShowWaveformOnly;
            showOnlySpectrogramToolStripMenuItem.Text = Configuration.Settings.Language.Waveform.ShowSpectrogramOnly;
            seekSilenceToolStripMenuItem.Text = Configuration.Settings.Language.Waveform.SeekSilence;
            guessTimeCodesToolStripMenuItem.Text = Configuration.Settings.Language.Waveform.GuessTimeCodes;

            toolStripButtonWaveformZoomOut.ToolTipText = Configuration.Settings.Language.Waveform.ZoomOut;
            toolStripButtonWaveformZoomIn.ToolTipText = Configuration.Settings.Language.Waveform.ZoomIn;

            if (Configuration.Settings.VideoControls.GenerateSpectrogram)
                audioVisualizer.WaveformNotLoadedText = Configuration.Settings.Language.Waveform.ClickToAddWaveformAndSpectrogram;
            else
                audioVisualizer.WaveformNotLoadedText = Configuration.Settings.Language.Waveform.ClickToAddWaveform;
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

        private int FirstSelectedIndex
        {
            get
            {
                if (SubtitleListview1.SelectedItems.Count == 0)
                    return -1;
                return SubtitleListview1.SelectedItems[0].Index;
            }
        }

        private int FirstVisibleIndex
        {
            get
            {
                if (SubtitleListview1.Items.Count == 0 || SubtitleListview1.TopItem == null)
                    return -1;
                return SubtitleListview1.TopItem.Index;
            }
        }

        private bool ContinueNewOrExit()
        {
            if (_changeSubtitleToString != SerializeSubtitle(_subtitle))
            {
                if (_lastDoNotPrompt != SerializeSubtitle(_subtitle))
                {

                    string promptText = _language.SaveChangesToUntitled;
                    if (!string.IsNullOrEmpty(_fileName))
                        promptText = string.Format(_language.SaveChangesToX, _fileName);

                    DialogResult dr = MessageBox.Show(this, promptText, Title, MessageBoxButtons.YesNoCancel, MessageBoxIcon.Exclamation);

                    if (dr == DialogResult.Cancel)
                        return false;

                    if (dr == DialogResult.Yes)
                    {
                        if (string.IsNullOrEmpty(_fileName))
                        {
                            if (!string.IsNullOrEmpty(openFileDialog1.InitialDirectory))
                                saveFileDialog1.InitialDirectory = openFileDialog1.InitialDirectory;
                            saveFileDialog1.Title = _language.SaveSubtitleAs;
                            if (saveFileDialog1.ShowDialog(this) == DialogResult.OK)
                            {
                                openFileDialog1.InitialDirectory = saveFileDialog1.InitialDirectory;
                                _fileName = saveFileDialog1.FileName;
                                SetTitle();
                                Configuration.Settings.RecentFiles.Add(_fileName, FirstVisibleIndex, FirstSelectedIndex, _videoFileName, _subtitleAlternateFileName);
                                Configuration.Settings.Save();

                            }
                            else
                            {
                                return false;
                            }
                        }
                        if (SaveSubtitle(GetCurrentSubtitleFormat()) != DialogResult.OK)
                            return false;
                    }
                }
            }

            return ContinueNewOrExitAlternate();
        }

        private bool ContinueNewOrExitAlternate()
        {
            if (Configuration.Settings.General.AllowEditOfOriginalSubtitle && _subtitleAlternate != null && _subtitleAlternate.Paragraphs.Count > 0 && _changeAlternateSubtitleToString != _subtitleAlternate.ToText(new SubRip()).Trim())
            {
                string promptText = _language.SaveChangesToUntitledOriginal;
                if (!string.IsNullOrEmpty(_subtitleAlternateFileName))
                    promptText = string.Format(_language.SaveChangesToOriginalX, _subtitleAlternateFileName);

                DialogResult dr = MessageBox.Show(this, promptText, Title, MessageBoxButtons.YesNoCancel, MessageBoxIcon.Exclamation);

                if (dr == DialogResult.Cancel)
                    return false;

                if (dr == DialogResult.Yes)
                {
                    if (string.IsNullOrEmpty(_subtitleAlternateFileName))
                    {
                        if (!string.IsNullOrEmpty(openFileDialog1.InitialDirectory))
                            saveFileDialog1.InitialDirectory = openFileDialog1.InitialDirectory;
                        saveFileDialog1.Title = _language.SaveOriginalSubtitleAs;
                        if (saveFileDialog1.ShowDialog(this) == DialogResult.OK)
                        {
                            openFileDialog1.InitialDirectory = saveFileDialog1.InitialDirectory;
                            _subtitleAlternateFileName = saveFileDialog1.FileName;
                        }
                        else
                        {
                            return false;
                        }
                    }
                    if (SaveOriginalSubtitle(GetCurrentSubtitleFormat()) != DialogResult.OK)
                        return false;
                }
            }
            _lastDoNotPrompt = SerializeSubtitle(_subtitle);
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
            var about = new About();
            _formPositionsAndSizes.SetPositionAndSize(about);
            about.Initialize();
            about.ShowDialog(this);
            _formPositionsAndSizes.SavePositionAndSize(about);
        }

        private void VisualSyncToolStripMenuItemClick(object sender, EventArgs e)
        {
            ReloadFromSourceView();
            ShowVisualSync(false);
        }

        public void MakeHistoryForUndo(string description, bool resetTextUndo)
        {
            if (_makeHistoryPaused)
                return;

            if (resetTextUndo)
            {
                _listViewTextUndoLast = null;
                _listViewAlternateTextUndoLast = null;
            }

            if (_undoIndex == -1)
            {
                _subtitle.HistoryItems.Clear();
            }
            else
            {
                // remove items for redo
                while (_subtitle.HistoryItems.Count > _undoIndex + 1)
                    _subtitle.HistoryItems.RemoveAt(_subtitle.HistoryItems.Count - 1);
            }

            _subtitle.FileName = _fileName;
            _subtitle.MakeHistoryForUndo(description, GetCurrentSubtitleFormat(), _fileDateTime, _subtitleAlternate, _subtitleAlternateFileName, _subtitleListViewIndex, textBoxListViewText.SelectionStart, textBoxListViewTextAlternate.SelectionStart);
            _undoIndex++;

            if (_undoIndex > Subtitle.MaximumHistoryItems)
                _undoIndex--;
        }

        public void MakeHistoryForUndo(string description)
        {
            MakeHistoryForUndo(description, true);
        }

        /// <summary>
        /// Add undo history - but only if last entry is older than 500 ms
        /// </summary>
        /// <param name="description">Undo description</param>
        public void MakeHistoryForUndoOnlyIfNotResent(string description)
        {
            if (_makeHistoryPaused)
                return;

            if ((DateTime.Now.Ticks - _lastHistoryTicks) > 10000 * 500) // only if last change was longer ago than 500 milliseconds
            {
                MakeHistoryForUndo(description);
                _lastHistoryTicks = DateTime.Now.Ticks;
            }
        }

        private bool IsSubtitleLoaded
        {
            get
            {
                if (_subtitle == null || _subtitle.Paragraphs.Count == 0)
                    return false;
                if (_subtitle.Paragraphs.Count == 1 && string.IsNullOrEmpty(_subtitle.Paragraphs[0].Text))
                    return false;
                return true;
            }
        }

        private void ShowVisualSync(bool onlySelectedLines)
        {
            if (IsSubtitleLoaded)
            {
                var visualSync = new VisualSync();
                _formPositionsAndSizes.SetPositionAndSize(visualSync);
                visualSync.VideoFileName = _videoFileName;
                visualSync.AudioTrackNumber = _videoAudioTrackNumber;

                SaveSubtitleListviewIndexes();
                if (onlySelectedLines)
                {
                    var selectedLines = new Subtitle { WasLoadedWithFrameNumbers = _subtitle.WasLoadedWithFrameNumbers };
                    foreach (int index in SubtitleListview1.SelectedIndices)
                        selectedLines.Paragraphs.Add(_subtitle.Paragraphs[index]);
                    visualSync.Initialize(toolStripButtonVisualSync.Image as Bitmap, selectedLines, _fileName, _language.VisualSyncSelectedLines, CurrentFrameRate);
                }
                else
                {
                    visualSync.Initialize(toolStripButtonVisualSync.Image as Bitmap, _subtitle, _fileName, _language.VisualSyncTitle, CurrentFrameRate);
                }

                _endSeconds = -1;
                mediaPlayer.Pause();
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
                        toolStripComboBoxFrameRate.Text = string.Format("{0:0.###}", visualSync.FrameRate);
                    if (IsFramesRelevant && CurrentFrameRate > 0)
                    {
                        _subtitle.CalculateFrameNumbersFromTimeCodesNoCheck(CurrentFrameRate);
                        if (tabControlSubtitle.SelectedIndex == TabControlSourceView)
                            ShowSource();
                    }
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
                MessageBox.Show(this, _language.NoSubtitleLoaded, Title, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

        }

        private void OpenToolStripMenuItemClick(object sender, EventArgs e)
        {
            openToolStripMenuItem.Enabled = false;
            ReloadFromSourceView();
            OpenNewFile();
            openToolStripMenuItem.Enabled = true;
        }

        private void OpenNewFile()
        {
            if (_openFileDialogOn)
                return;
            _openFileDialogOn = true;
            _lastDoNotPrompt = string.Empty;
            if (!ContinueNewOrExit())
            {
                _openFileDialogOn = false;
                return;
            }
            openFileDialog1.Title = _languageGeneral.OpenSubtitle;
            openFileDialog1.FileName = string.Empty;
            openFileDialog1.Filter = Utilities.GetOpenDialogFilter();
            if (openFileDialog1.ShowDialog(this) == DialogResult.OK)
            {
                RemoveAlternate(true);
                OpenSubtitle(openFileDialog1.FileName, null);
            }
            _openFileDialogOn = false;
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
            OpenSubtitle(fileName, encoding, null, null);
        }

        private void ResetHistory()
        {
            _undoIndex = -1;
            _subtitle.HistoryItems.Clear();
        }

        private void OpenSubtitle(string fileName, Encoding encoding, string videoFileName, string originalFileName)
        {
            if (File.Exists(fileName))
            {
                bool videoFileLoaded = false;
                string ext = Path.GetExtension(fileName).ToLower();

                // save last first visible index + first selected index from listview
                if (!string.IsNullOrEmpty(_fileName))
                    Configuration.Settings.RecentFiles.Add(_fileName, FirstVisibleIndex, FirstSelectedIndex, _videoFileName, originalFileName);

                openFileDialog1.InitialDirectory = Path.GetDirectoryName(fileName);

                if (ext == ".sub" && IsVobSubFile(fileName, false))
                {
                    if (MessageBox.Show(this, _language.ImportThisVobSubSubtitle, _title, MessageBoxButtons.YesNo) == DialogResult.Yes)
                    {
                        ImportAndOcrVobSubSubtitleNew(fileName, _loading);
                    }
                    return;
                }

                if (ext == ".sup")
                {
                    if (FileUtils.IsBluRaySup(fileName))
                    {
                        ImportAndOcrBluRaySup(fileName, _loading);
                        return;
                    }
                    else if (FileUtils.IsSpDvdSup(fileName))
                    {
                        ImportAndOcrSpDvdSup(fileName, _loading);
                        return;
                    }
                }

                if (ext == ".mkv" || ext == ".mks")
                {
                    var mkv = new Matroska();
                    bool isValid = false;
                    bool hasConstantFrameRate = false;
                    double frameRate = 0;
                    int width = 0;
                    int height = 0;
                    double milliseconds = 0;
                    string videoCodec = string.Empty;
                    mkv.GetMatroskaInfo(fileName, ref isValid, ref hasConstantFrameRate, ref frameRate, ref width, ref height, ref milliseconds, ref videoCodec);
                    if (isValid)
                    {
                        ImportSubtitleFromMatroskaFile(fileName);
                        return;
                    }
                }

                if (ext == ".divx" || ext == ".avi")
                {
                    if (ImportSubtitleFromDivX(fileName))
                        return;
                }

                var fi = new FileInfo(fileName);

                if ((ext == ".ts" || ext == ".rec" || ext == ".mpeg" || ext == ".mpg") && fi.Length > 10000 && FileUtils.IsTransportStream(fileName))
                {
                    ImportSubtitleFromTransportStream(fileName);
                    return;
                }

                if ((ext == ".m2ts") && fi.Length > 10000 && FileUtils.IsM2TransportStream(fileName))
                {
                    ImportSubtitleFromTransportStream(fileName);
                    return;
                }

                if ((ext == ".mp4" || ext == ".m4v" || ext == ".3gp")
                    && fi.Length > 10000)
                {
                    if (ImportSubtitleFromMp4(fileName))
                        OpenVideo(fileName);
                    return;
                }

                if (fi.Length > 1024 * 1024 * 10) // max 10 mb
                {

                    // retry bluray sup (file with wrong extension)
                    if (FileUtils.IsBluRaySup(fileName))
                    {
                        ImportAndOcrBluRaySup(fileName, _loading);
                        return;
                    }

                    // retry vobsub (file with wrong extension)
                    if (IsVobSubFile(fileName, false))
                    {
                        if (MessageBox.Show(this, _language.ImportThisVobSubSubtitle, _title, MessageBoxButtons.YesNo) == DialogResult.Yes)
                        {
                            ImportAndOcrVobSubSubtitleNew(fileName, _loading);
                        }
                        return;
                    }

                    if (MessageBox.Show(this, string.Format(_language.FileXIsLargerThan10MB + Environment.NewLine +
                                                      Environment.NewLine +
                                                      _language.ContinueAnyway,
                                                      fileName), Title, MessageBoxButtons.YesNoCancel) != DialogResult.Yes)
                        return;
                }

                if (_subtitle.HistoryItems.Count > 0 || _subtitle.Paragraphs.Count > 0)
                    MakeHistoryForUndo(string.Format(_language.BeforeLoadOf, Path.GetFileName(fileName)));

                bool change = _changeSubtitleToString != SerializeSubtitle(_subtitle);
                if (change)
                    change = _lastDoNotPrompt != SerializeSubtitle(_subtitle);

                SubtitleFormat format = _subtitle.LoadSubtitle(fileName, out encoding, encoding);
                if (!change)
                    _changeSubtitleToString = SerializeSubtitle(_subtitle);

                ShowHideTextBasedFeatures(format);

                bool justConverted = false;
                if (format == null)
                {
                    var ebu = new Ebu();
                    if (ebu.IsMine(null, fileName))
                    {
                        ebu.LoadSubtitle(_subtitle, null, fileName);
                        _oldSubtitleFormat = ebu;
                        SetCurrentFormat(Configuration.Settings.General.DefaultSubtitleFormat);
                        SetEncoding(Configuration.Settings.General.DefaultEncoding);
                        encoding = GetCurrentEncoding();
                        justConverted = true;
                        format = GetCurrentSubtitleFormat();
                    }
                }

                if (format == null)
                {
                    var pac = new Pac();
                    if (pac.IsMine(null, fileName))
                    {
                        pac.LoadSubtitle(_subtitle, null, fileName);
                        _oldSubtitleFormat = pac;
                        SetCurrentFormat(Configuration.Settings.General.DefaultSubtitleFormat);
                        SetEncoding(Configuration.Settings.General.DefaultEncoding);
                        encoding = GetCurrentEncoding();
                        justConverted = true;
                        format = GetCurrentSubtitleFormat();
                    }
                }

                if (format == null)
                {
                    var cavena890 = new Cavena890();
                    if (cavena890.IsMine(null, fileName))
                    {
                        cavena890.LoadSubtitle(_subtitle, null, fileName);
                        _oldSubtitleFormat = cavena890;
                        SetCurrentFormat(Configuration.Settings.General.DefaultSubtitleFormat);
                        SetEncoding(Configuration.Settings.General.DefaultEncoding);
                        encoding = GetCurrentEncoding();
                        justConverted = true;
                        format = GetCurrentSubtitleFormat();
                    }
                }

                if (format == null)
                {
                    var spt = new Spt();
                    if (spt.IsMine(null, fileName))
                    {
                        spt.LoadSubtitle(_subtitle, null, fileName);
                        _oldSubtitleFormat = spt;
                        SetCurrentFormat(Configuration.Settings.General.DefaultSubtitleFormat);
                        SetEncoding(Configuration.Settings.General.DefaultEncoding);
                        encoding = GetCurrentEncoding();
                        justConverted = true;
                        format = GetCurrentSubtitleFormat();
                    }
                }

                if (format == null && ext == ".wsb")
                {
                    string[] arr = File.ReadAllLines(fileName, Utilities.GetEncodingFromFile(fileName));
                    var list = new List<string>();
                    foreach (string l in arr)
                        list.Add(l);
                    var wsb = new Wsb();
                    if (wsb.IsMine(list, fileName))
                    {
                        wsb.LoadSubtitle(_subtitle, list, fileName);
                        _oldSubtitleFormat = wsb;
                        SetCurrentFormat(Configuration.Settings.General.DefaultSubtitleFormat);
                        SetEncoding(Configuration.Settings.General.DefaultEncoding);
                        encoding = GetCurrentEncoding();
                        justConverted = true;
                        format = GetCurrentSubtitleFormat();
                    }
                }

                if (format == null)
                {
                    var cheetahCaption = new CheetahCaption();
                    if (cheetahCaption.IsMine(null, fileName))
                    {
                        cheetahCaption.LoadSubtitle(_subtitle, null, fileName);
                        _oldSubtitleFormat = cheetahCaption;
                        SetCurrentFormat(Configuration.Settings.General.DefaultSubtitleFormat);
                        SetEncoding(Configuration.Settings.General.DefaultEncoding);
                        encoding = GetCurrentEncoding();
                        justConverted = true;
                        format = GetCurrentSubtitleFormat();
                    }
                }

                if (format == null)
                {
                    var capMakerPlus = new CapMakerPlus();
                    if (capMakerPlus.IsMine(null, fileName))
                    {
                        capMakerPlus.LoadSubtitle(_subtitle, null, fileName);
                        _oldSubtitleFormat = capMakerPlus;
                        SetCurrentFormat(Configuration.Settings.General.DefaultSubtitleFormat);
                        SetEncoding(Configuration.Settings.General.DefaultEncoding);
                        encoding = GetCurrentEncoding();
                        justConverted = true;
                        format = GetCurrentSubtitleFormat();
                    }
                }

                if (format == null)
                {
                    var captionsInc = new CaptionsInc();
                    if (captionsInc.IsMine(null, fileName))
                    {
                        captionsInc.LoadSubtitle(_subtitle, null, fileName);
                        _oldSubtitleFormat = captionsInc;
                        SetCurrentFormat(Configuration.Settings.General.DefaultSubtitleFormat);
                        SetEncoding(Configuration.Settings.General.DefaultEncoding);
                        encoding = GetCurrentEncoding();
                        justConverted = true;
                        format = GetCurrentSubtitleFormat();
                    }
                }

                if (format == null)
                {
                    var ultech130 = new Ultech130();
                    if (ultech130.IsMine(null, fileName))
                    {
                        ultech130.LoadSubtitle(_subtitle, null, fileName);
                        _oldSubtitleFormat = ultech130;
                        SetCurrentFormat(Configuration.Settings.General.DefaultSubtitleFormat);
                        SetEncoding(Configuration.Settings.General.DefaultEncoding);
                        encoding = GetCurrentEncoding();
                        justConverted = true;
                        format = GetCurrentSubtitleFormat();
                    }
                }

                if (format == null)
                {
                    var nciCaption = new NciCaption();
                    if (nciCaption.IsMine(null, fileName))
                    {
                        nciCaption.LoadSubtitle(_subtitle, null, fileName);
                        _oldSubtitleFormat = nciCaption;
                        SetCurrentFormat(Configuration.Settings.General.DefaultSubtitleFormat);
                        SetEncoding(Configuration.Settings.General.DefaultEncoding);
                        encoding = GetCurrentEncoding();
                        justConverted = true;
                        format = GetCurrentSubtitleFormat();
                    }
                }

                if (format == null)
                {
                    var tsb4 = new TSB4();
                    if (tsb4.IsMine(null, fileName))
                    {
                        tsb4.LoadSubtitle(this._subtitle, null, fileName);
                        _oldSubtitleFormat = tsb4;
                        this.SetCurrentFormat(Configuration.Settings.General.DefaultSubtitleFormat);
                        this.SetEncoding(Configuration.Settings.General.DefaultEncoding);
                        encoding = GetCurrentEncoding();
                        justConverted = true;
                        format = GetCurrentSubtitleFormat();
                    }
                }

                if (format == null)
                {
                    var avidStl = new AvidStl();
                    if (avidStl.IsMine(null, fileName))
                    {
                        avidStl.LoadSubtitle(_subtitle, null, fileName);
                        _oldSubtitleFormat = avidStl;
                        SetCurrentFormat(Configuration.Settings.General.DefaultSubtitleFormat);
                        SetEncoding(Configuration.Settings.General.DefaultEncoding);
                        encoding = GetCurrentEncoding();
                        justConverted = true;
                        format = GetCurrentSubtitleFormat();
                    }
                }

                if (format == null)
                {
                    var chk = new Chk();
                    if (chk.IsMine(null, fileName))
                    {
                        chk.LoadSubtitle(_subtitle, null, fileName);
                        _oldSubtitleFormat = chk;
                        SetCurrentFormat(Configuration.Settings.General.DefaultSubtitleFormat);
                        SetEncoding(Configuration.Settings.General.DefaultEncoding);
                        encoding = GetCurrentEncoding();
                        justConverted = true;
                        format = GetCurrentSubtitleFormat();
                    }
                }

                if (format == null)
                {
                    try
                    {
                        var bdnXml = new BdnXml();
                        string[] arr = File.ReadAllLines(fileName, Utilities.GetEncodingFromFile(fileName));
                        var list = new List<string>();
                        foreach (string l in arr)
                            list.Add(l);
                        if (bdnXml.IsMine(list, fileName))
                        {
                            if (ContinueNewOrExit())
                            {
                                ImportAndOcrBdnXml(fileName, bdnXml, list);
                            }
                            return;
                        }
                    }
                    catch
                    {
                        format = null;
                    }
                }

                if (format == null)
                {
                    try
                    {
                        var fcpImage = new FinalCutProImage();
                        string[] arr = File.ReadAllLines(fileName, Utilities.GetEncodingFromFile(fileName));
                        var list = new List<string>();
                        foreach (string l in arr)
                            list.Add(l);
                        if (fcpImage.IsMine(list, fileName))
                        {
                            if (ContinueNewOrExit())
                            {
                                ImportAndOcrDost(fileName, fcpImage, list);
                            }
                            return;
                        }
                    }
                    catch
                    {
                        format = null;
                    }
                }

                if (format == null)
                {
                    var elr = new ELRStudioClosedCaption();
                    if (elr.IsMine(null, fileName))
                    {
                        elr.LoadSubtitle(_subtitle, null, fileName);
                        _oldSubtitleFormat = elr;
                        SetCurrentFormat(Configuration.Settings.General.DefaultSubtitleFormat);
                        SetEncoding(Configuration.Settings.General.DefaultEncoding);
                        encoding = GetCurrentEncoding();
                        justConverted = true;
                        format = GetCurrentSubtitleFormat();
                    }
                }

                if (fileName.EndsWith(".dost", StringComparison.OrdinalIgnoreCase))
                {
                    try
                    {
                        var dost = new Dost();
                        string[] arr = File.ReadAllLines(fileName, Utilities.GetEncodingFromFile(fileName));
                        var list = new List<string>();
                        foreach (string l in arr)
                            list.Add(l);
                        if (dost.IsMine(list, fileName))
                        {
                            if (ContinueNewOrExit())
                                ImportAndOcrDost(fileName, dost, list);
                            return;
                        }
                    }
                    catch
                    {
                        format = null;
                    }
                }

                if (format == null || format.Name == new Scenarist().Name)
                {
                    try
                    {
                        var son = new Son();
                        string[] arr = File.ReadAllLines(fileName, Utilities.GetEncodingFromFile(fileName));
                        var list = new List<string>();
                        foreach (string l in arr)
                            list.Add(l);
                        if (son.IsMine(list, fileName))
                        {
                            if (ContinueNewOrExit())
                                ImportAndOcrSon(fileName, son, list);
                            return;
                        }
                    }
                    catch
                    {
                        format = null;
                    }
                }

                if (format == null || format.Name == new SubRip().Name)
                {
                    if (_subtitle.Paragraphs.Count > 1)
                    {
                        int imageCount = 0;
                        foreach (Paragraph p in _subtitle.Paragraphs)
                        {
                            string s = p.Text.ToLower();
                            if (s.EndsWith(".bmp", StringComparison.Ordinal) || s.EndsWith(".png", StringComparison.Ordinal) || s.EndsWith(".jpg", StringComparison.Ordinal) || s.EndsWith(".tif", StringComparison.Ordinal))
                            {
                                imageCount++;
                            }
                        }
                        if (imageCount > 2 && imageCount >= _subtitle.Paragraphs.Count - 2)
                        {
                            if (ContinueNewOrExit())
                                ImportAndOcrSrt(_subtitle);
                            return;

                        }
                    }
                }

                if (format == null)
                {
                    try
                    {
                        var satBoxPng = new SatBoxPng();
                        string[] arr = File.ReadAllLines(fileName, Utilities.GetEncodingFromFile(fileName));
                        var list = new List<string>();
                        foreach (string l in arr)
                            list.Add(l);
                        if (satBoxPng.IsMine(list, fileName))
                        {
                            var subtitle = new Subtitle();
                            satBoxPng.LoadSubtitle(subtitle, list, fileName);
                            if (ContinueNewOrExit())
                                ImportAndOcrSrt(subtitle);
                            return;
                        }
                    }
                    catch
                    {
                        format = null;
                    }
                }

                if (format == null || format.Name == new Scenarist().Name)
                {
                    try
                    {
                        var sst = new SonicScenaristBitmaps();
                        string[] arr = File.ReadAllLines(fileName, Utilities.GetEncodingFromFile(fileName));
                        var list = new List<string>();
                        foreach (string l in arr)
                            list.Add(l);
                        if (sst.IsMine(list, fileName))
                        {
                            if (ContinueNewOrExit())
                                ImportAndOcrSst(fileName, sst, list);
                            return;
                        }
                    }
                    catch
                    {
                        format = null;
                    }
                }

                if (format == null)
                {
                    try
                    {
                        var htmlSamiArray = new HtmlSamiArray();
                        string[] arr = File.ReadAllLines(fileName, Utilities.GetEncodingFromFile(fileName));
                        var list = new List<string>();
                        foreach (string l in arr)
                            list.Add(l);
                        if (htmlSamiArray.IsMine(list, fileName))
                        {
                            htmlSamiArray.LoadSubtitle(_subtitle, list, fileName);
                            _oldSubtitleFormat = htmlSamiArray;
                            SetCurrentFormat(Configuration.Settings.General.DefaultSubtitleFormat);
                            SetEncoding(Configuration.Settings.General.DefaultEncoding);
                            encoding = GetCurrentEncoding();
                            justConverted = true;
                            format = GetCurrentSubtitleFormat();
                        }
                    }
                    catch
                    {
                        format = null;
                    }
                }

                // retry vobsub (file with wrong extension)
                if (format == null && fi.Length > 500 && IsVobSubFile(fileName, false))
                {
                    if (MessageBox.Show(this, _language.ImportThisVobSubSubtitle, _title, MessageBoxButtons.YesNo) == DialogResult.Yes)
                    {
                        ImportAndOcrVobSubSubtitleNew(fileName, _loading);
                    }
                    return;
                }

                // retry bluray (file with wrong extension)
                if (format == null && fi.Length > 500 && FileUtils.IsBluRaySup(fileName))
                {
                    ImportAndOcrBluRaySup(fileName, _loading);
                    return;
                }

                // retry SP DVD (file with wrong extension)
                if (format == null && fi.Length > 500 && FileUtils.IsSpDvdSup(fileName))
                {
                    ImportAndOcrSpDvdSup(fileName, _loading);
                    return;
                }

                // check for idx file
                if (format == null && fi.Length > 100 && ext == ".idx")
                {
                    if (string.IsNullOrEmpty(_language.ErrorLoadIdx))
                        MessageBox.Show("Cannot read/edit .idx files. Idx files are a part of an idx/sub file pair (also called VobSub), and Subtitle Edit can open the .sub file.");
                    else
                        MessageBox.Show(_language.ErrorLoadIdx);
                    return;
                }

                // check for .rar file
                if (format == null && fi.Length > 100 && FileUtils.IsRar(fileName))
                {
                    if (string.IsNullOrEmpty(_language.ErrorLoadRar))
                        MessageBox.Show("This file seems to be a compressed .rar file. Subtitle Edit cannot open compressed files.");
                    else
                        MessageBox.Show(_language.ErrorLoadRar);
                    return;
                }

                // check for .zip file
                if (format == null && fi.Length > 100 && FileUtils.IsZip(fileName))
                {
                    if (string.IsNullOrEmpty(_language.ErrorLoadZip))
                        MessageBox.Show("This file seems to be a compressed .zip file. Subtitle Edit cannot open compressed files.");
                    else
                        MessageBox.Show(_language.ErrorLoadZip);
                    return;
                }

                if (format == null && fi.Length < 100 * 1000000 && TransportStreamParser.IsDvbSup(fileName))
                {
                    ImportSubtitleFromDvbSupFile(fileName);
                    return;
                }

                if (format == null && fi.Length < 500000)
                { // Try to use a generic subtitle format parser (guessing subtitle format)
                    try
                    {
                        Encoding enc = Utilities.GetEncodingFromFile(fileName);
                        string s = File.ReadAllText(fileName, enc);

                        // check for RTF file
                        if (fileName.EndsWith(".rtf", StringComparison.OrdinalIgnoreCase) && !s.TrimStart().StartsWith("{\\rtf", StringComparison.Ordinal))
                        {
                            var rtBox = new RichTextBox();
                            rtBox.Rtf = s;
                            s = rtBox.Text;
                        }
                        var uknownFormatImporter = new UknownFormatImporter();
                        uknownFormatImporter.UseFrames = true;
                        var genericParseSubtitle = uknownFormatImporter.AutoGuessImport(s.Replace(Environment.NewLine, "\n").Replace("\r", "\n").Split('\n'));
                        if (genericParseSubtitle.Paragraphs.Count > 1)
                        {
                            _subtitle = genericParseSubtitle;
                            SetCurrentFormat(Configuration.Settings.General.DefaultSubtitleFormat);
                            SetEncoding(Configuration.Settings.General.DefaultEncoding);
                            encoding = GetCurrentEncoding();
                            justConverted = true;
                            format = GetCurrentSubtitleFormat();
                            ShowStatus("Guessed subtitle format via generic subtitle parser!");
                        }
                    }
                    catch
                    {
                    }
                }

                _fileDateTime = File.GetLastWriteTime(fileName);

                if (format != null && format.IsFrameBased)
                    _subtitle.CalculateTimeCodesFromFrameNumbers(CurrentFrameRate);
                else
                    _subtitle.CalculateFrameNumbersFromTimeCodes(CurrentFrameRate);

                if (format != null)
                {
                    if (Configuration.Settings.General.RemoveBlankLinesWhenOpening)
                    {
                        _subtitle.RemoveEmptyLines();
                    }

                    foreach (Paragraph p in _subtitle.Paragraphs)
                    {
                        p.Text = p.Text.Replace("<і>", "<i>").Replace("</і>", "</i>");  // different unicode chars
                    }

                    _subtitleListViewIndex = -1;
                    SetCurrentFormat(format);
                    _subtitleAlternateFileName = null;
                    if (LoadAlternateSubtitleFile(originalFileName))
                        _subtitleAlternateFileName = originalFileName;

                    // Seungki begin
                    _splitDualSami = false;
                    if (Configuration.Settings.SubtitleSettings.SamiDisplayTwoClassesAsTwoSubtitles && format.GetType() == typeof(Sami) && Sami.GetStylesFromHeader(_subtitle.Header).Count == 2)
                    {
                        List<string> classes = Sami.GetStylesFromHeader(_subtitle.Header);
                        var s1 = new Subtitle(_subtitle);
                        var s2 = new Subtitle(_subtitle);
                        s1.Paragraphs.Clear();
                        s2.Paragraphs.Clear();
                        foreach (Paragraph p in _subtitle.Paragraphs)
                        {
                            if (p.Extra != null && p.Extra.Equals(classes[0], StringComparison.OrdinalIgnoreCase))
                                s1.Paragraphs.Add(p);
                            else
                                s2.Paragraphs.Add(p);
                        }
                        if (s1.Paragraphs.Count == 0 || s2.Paragraphs.Count == 0)
                            return;

                        _subtitle = s1;
                        _subtitleAlternate = s2;
                        _subtitleAlternateFileName = _fileName;
                        SubtitleListview1.HideExtraColumn();
                        SubtitleListview1.ShowAlternateTextColumn(classes[1]);
                        _splitDualSami = true;
                    }
                    // Seungki end

                    textBoxSource.Text = _subtitle.ToText(format);
                    SubtitleListview1.Fill(_subtitle, _subtitleAlternate);
                    if (SubtitleListview1.Items.Count > 0)
                        SubtitleListview1.Items[0].Selected = true;
                    _findHelper = null;
                    _spellCheckForm = null;

                    if (_resetVideo)
                    {
                        _videoFileName = null;
                        _videoInfo = null;
                        _videoAudioTrackNumber = -1;
                        labelVideoInfo.Text = Configuration.Settings.Language.General.NoVideoLoaded;
                        audioVisualizer.WavePeaks = null;
                        audioVisualizer.ResetSpectrogram();
                        audioVisualizer.Invalidate();
                    }

                    if (Configuration.Settings.General.ShowVideoPlayer || Configuration.Settings.General.ShowAudioVisualizer)
                    {
                        if (!Configuration.Settings.General.DisableVideoAutoLoading)
                        {
                            if (!string.IsNullOrEmpty(videoFileName) && File.Exists(videoFileName))
                            {
                                OpenVideo(videoFileName);
                            }
                            else if (!string.IsNullOrEmpty(fileName) && (toolStripButtonToggleVideo.Checked || toolStripButtonToggleWaveform.Checked))
                            {
                                TryToFindAndOpenVideoFile(Path.Combine(Path.GetDirectoryName(fileName), Path.GetFileNameWithoutExtension(fileName)));
                            }
                        }
                    }
                    videoFileLoaded = _videoFileName != null;

                    if (Configuration.Settings.RecentFiles.Files.Count > 0 &&
                        Configuration.Settings.RecentFiles.Files[0].FileName == fileName)
                    {
                    }
                    else
                    {
                        Configuration.Settings.RecentFiles.Add(fileName, _videoFileName, _subtitleAlternateFileName);
                        Configuration.Settings.Save();
                        UpdateRecentFilesUI();
                    }
                    _fileName = fileName;
                    SetTitle();
                    ShowStatus(string.Format(_language.LoadedSubtitleX, _fileName));
                    _sourceViewChange = false;
                    _changeSubtitleToString = SerializeSubtitle(_subtitle);
                    _converted = false;
                    ResetHistory();

                    SetUndockedWindowsTitle();

                    if (justConverted)
                    {
                        _converted = true;
                        ShowStatus(string.Format(_language.LoadedSubtitleX, _fileName) + " - " + string.Format(_language.ConvertedToX, format.FriendlyName));
                    }
                    if (Configuration.Settings.General.AutoConvertToUtf8)
                        encoding = Encoding.UTF8;
                    SetEncoding(encoding);

                    if (format.GetType() == typeof(SubStationAlpha))
                    {
                        string errors = AdvancedSubStationAlpha.CheckForErrors(_subtitle.Header);
                        if (!string.IsNullOrEmpty(errors))
                            MessageBox.Show(this, errors, Title, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        errors = (format as SubStationAlpha).Errors;
                        if (!string.IsNullOrEmpty(errors))
                            MessageBox.Show(this, errors, Title, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                    else if (format.GetType() == typeof(AdvancedSubStationAlpha))
                    {
                        string errors = AdvancedSubStationAlpha.CheckForErrors(_subtitle.Header);
                        if (!string.IsNullOrEmpty(errors))
                            MessageBox.Show(this, errors, Title, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        errors = (format as AdvancedSubStationAlpha).Errors;
                        if (!string.IsNullOrEmpty(errors))
                            MessageBox.Show(errors, Title, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                    else if (format.GetType() == typeof(SubRip))
                    {
                        string errors = (format as SubRip).Errors;
                        if (!string.IsNullOrEmpty(errors))
                            MessageBox.Show(this, errors, Title, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                    else if (format.GetType() == typeof(MicroDvd))
                    {
                        string errors = (format as MicroDvd).Errors;
                        if (!string.IsNullOrEmpty(errors))
                            MessageBox.Show(this, errors, Title, MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
                        _videoInfo = null;
                        _videoAudioTrackNumber = -1;
                        labelVideoInfo.Text = Configuration.Settings.Language.General.NoVideoLoaded;
                        audioVisualizer.WavePeaks = null;
                        audioVisualizer.ResetSpectrogram();
                        audioVisualizer.Invalidate();

                        Configuration.Settings.RecentFiles.Add(fileName, FirstVisibleIndex, FirstSelectedIndex, _videoFileName, _subtitleAlternateFileName);
                        Configuration.Settings.Save();
                        UpdateRecentFilesUI();
                        _fileName = fileName;
                        SetTitle();
                        ShowStatus(string.Format(_language.LoadedEmptyOrShort, _fileName));
                        _sourceViewChange = false;
                        _converted = false;

                        MessageBox.Show(_language.FileIsEmptyOrShort);
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(fileName) && fileName.EndsWith(".xml", StringComparison.OrdinalIgnoreCase))
                        {
                            string[] arr = File.ReadAllLines(fileName, Utilities.GetEncodingFromFile(fileName));
                            var sb = new StringBuilder();
                            foreach (string l in arr)
                                sb.AppendLine(l);
                            string xmlAsString = sb.ToString().Trim();
                            if (xmlAsString.Contains("http://www.w3.org/ns/ttml") && xmlAsString.Contains("<?xml version="))
                            {
                                var xml = new System.Xml.XmlDocument();
                                try
                                {
                                    xml.LoadXml(xmlAsString);
                                }
                                catch (Exception ex)
                                {
                                    MessageBox.Show("Timed text is not valid: " + ex.Message);
                                    return;
                                }
                            }

                            if (xmlAsString.Contains("http://www.w3.org/") &&
                                xmlAsString.Contains("/ttaf1"))
                            {
                                var xml = new System.Xml.XmlDocument();
                                try
                                {
                                    xml.LoadXml(xmlAsString);
                                }
                                catch (Exception ex)
                                {
                                    MessageBox.Show("Timed text is not valid: " + ex.Message);
                                    return;
                                }
                            }
                        }

                        ShowUnknownSubtitle();
                        return;
                    }
                }

                if (!videoFileLoaded && mediaPlayer.VideoPlayer != null)
                {
                    mediaPlayer.VideoPlayer.DisposeVideoPlayer();
                    mediaPlayer.VideoPlayer = null;
                    timer1.Stop();
                }
            }
            else
            {
                MessageBox.Show(string.Format(_language.FileNotFound, fileName));
            }
        }

        private void ShowHideTextBasedFeatures(SubtitleFormat format)
        {
            if (format != null && !format.IsTextBased)
            {
                textBoxSource.Enabled = false;
            }
            else
            {
                textBoxSource.Enabled = true;
            }
        }

        private void SetUndockedWindowsTitle()
        {
            string title = Configuration.Settings.Language.General.NoVideoLoaded;
            if (!string.IsNullOrEmpty(_videoFileName))
                title = Path.GetFileNameWithoutExtension(_videoFileName);

            if (_videoControlsUndocked != null && !_videoControlsUndocked.IsDisposed)
                _videoControlsUndocked.Text = string.Format(Configuration.Settings.Language.General.ControlsWindowTitle, title);

            if (_videoPlayerUndocked != null && !_videoPlayerUndocked.IsDisposed)
                _videoPlayerUndocked.Text = string.Format(Configuration.Settings.Language.General.VideoWindowTitle, title);

            if (_waveformUndocked != null && !_waveformUndocked.IsDisposed)
                _waveformUndocked.Text = string.Format(Configuration.Settings.Language.General.AudioWindowTitle, title);
        }

        private void ImportAndOcrBdnXml(string fileName, BdnXml bdnXml, List<string> list)
        {
            Subtitle bdnSubtitle = new Subtitle();
            bdnXml.LoadSubtitle(bdnSubtitle, list, fileName);
            bdnSubtitle.FileName = fileName;
            var formSubOcr = new VobSubOcr();
            _formPositionsAndSizes.SetPositionAndSize(formSubOcr);
            formSubOcr.Initialize(bdnSubtitle, Configuration.Settings.VobSubOcr, false);
            if (formSubOcr.ShowDialog(this) == DialogResult.OK)
            {
                MakeHistoryForUndo(_language.BeforeImportingBdnXml);
                FileNew();
                _subtitle.Paragraphs.Clear();
                SetCurrentFormat(Configuration.Settings.General.DefaultSubtitleFormat);
                _subtitle.WasLoadedWithFrameNumbers = false;
                _subtitle.CalculateFrameNumbersFromTimeCodes(CurrentFrameRate);
                foreach (Paragraph p in formSubOcr.SubtitleFromOcr.Paragraphs)
                {
                    _subtitle.Paragraphs.Add(p);
                }

                ShowSource();
                SubtitleListview1.Fill(_subtitle, _subtitleAlternate);
                _subtitleListViewIndex = -1;
                SubtitleListview1.FirstVisibleIndex = -1;
                SubtitleListview1.SelectIndexAndEnsureVisible(0);

                _fileName = Path.ChangeExtension(formSubOcr.FileName, ".srt");
                SetTitle();
                _converted = true;
            }
            _formPositionsAndSizes.SavePositionAndSize(formSubOcr);
        }

        private void ImportAndOcrSon(string fileName, Son format, List<string> list)
        {
            Subtitle sub = new Subtitle();
            format.LoadSubtitle(sub, list, fileName);
            sub.FileName = fileName;
            var formSubOcr = new VobSubOcr();
            _formPositionsAndSizes.SetPositionAndSize(formSubOcr);
            formSubOcr.Initialize(sub, Configuration.Settings.VobSubOcr, true);
            if (formSubOcr.ShowDialog(this) == DialogResult.OK)
            {
                MakeHistoryForUndo(_language.BeforeImportingBdnXml);
                FileNew();
                _subtitle.Paragraphs.Clear();
                SetCurrentFormat(Configuration.Settings.General.DefaultSubtitleFormat);
                _subtitle.WasLoadedWithFrameNumbers = false;
                _subtitle.CalculateFrameNumbersFromTimeCodes(CurrentFrameRate);
                foreach (Paragraph p in formSubOcr.SubtitleFromOcr.Paragraphs)
                {
                    _subtitle.Paragraphs.Add(p);
                }

                ShowSource();
                SubtitleListview1.Fill(_subtitle, _subtitleAlternate);
                _subtitleListViewIndex = -1;
                SubtitleListview1.FirstVisibleIndex = -1;
                SubtitleListview1.SelectIndexAndEnsureVisible(0);

                _fileName = Path.ChangeExtension(formSubOcr.FileName, ".srt");
                SetTitle();
                _converted = true;
            }
            _formPositionsAndSizes.SavePositionAndSize(formSubOcr);
        }

        private void ImportAndOcrDost(string fileName, SubtitleFormat format, List<string> list)
        {
            var sub = new Subtitle();
            format.LoadSubtitle(sub, list, fileName);
            sub.FileName = fileName;
            var formSubOcr = new VobSubOcr();
            _formPositionsAndSizes.SetPositionAndSize(formSubOcr);
            formSubOcr.Initialize(sub, Configuration.Settings.VobSubOcr, false);
            if (formSubOcr.ShowDialog(this) == DialogResult.OK)
            {
                MakeHistoryForUndo(_language.BeforeImportingBdnXml);
                FileNew();
                _subtitle.Paragraphs.Clear();
                SetCurrentFormat(Configuration.Settings.General.DefaultSubtitleFormat);
                _subtitle.WasLoadedWithFrameNumbers = false;
                _subtitle.CalculateFrameNumbersFromTimeCodes(CurrentFrameRate);
                foreach (Paragraph p in formSubOcr.SubtitleFromOcr.Paragraphs)
                {
                    _subtitle.Paragraphs.Add(p);
                }

                ShowSource();
                SubtitleListview1.Fill(_subtitle, _subtitleAlternate);
                _subtitleListViewIndex = -1;
                SubtitleListview1.FirstVisibleIndex = -1;
                SubtitleListview1.SelectIndexAndEnsureVisible(0);

                _fileName = Path.ChangeExtension(formSubOcr.FileName, ".srt");
                SetTitle();
                _converted = true;
            }
            _formPositionsAndSizes.SavePositionAndSize(formSubOcr);
        }

        private void ImportAndOcrSst(string fileName, SonicScenaristBitmaps format, List<string> list)
        {
            Subtitle sub = new Subtitle();
            format.LoadSubtitle(sub, list, fileName);
            sub.FileName = fileName;
            var formSubOcr = new VobSubOcr();
            _formPositionsAndSizes.SetPositionAndSize(formSubOcr);
            formSubOcr.Initialize(sub, Configuration.Settings.VobSubOcr, true);
            if (formSubOcr.ShowDialog(this) == DialogResult.OK)
            {
                MakeHistoryForUndo(_language.BeforeImportingBdnXml);
                FileNew();
                _subtitle.Paragraphs.Clear();
                SetCurrentFormat(Configuration.Settings.General.DefaultSubtitleFormat);
                _subtitle.WasLoadedWithFrameNumbers = false;
                _subtitle.CalculateFrameNumbersFromTimeCodes(CurrentFrameRate);
                foreach (Paragraph p in formSubOcr.SubtitleFromOcr.Paragraphs)
                {
                    _subtitle.Paragraphs.Add(p);
                }

                ShowSource();
                SubtitleListview1.Fill(_subtitle, _subtitleAlternate);
                _subtitleListViewIndex = -1;
                SubtitleListview1.FirstVisibleIndex = -1;
                SubtitleListview1.SelectIndexAndEnsureVisible(0);

                _fileName = Path.ChangeExtension(formSubOcr.FileName, ".srt");
                SetTitle();
                _converted = true;
            }
            _formPositionsAndSizes.SavePositionAndSize(formSubOcr);
        }

        private void ImportAndOcrSrt(Subtitle subtitle)
        {
            var formSubOcr = new VobSubOcr();
            _formPositionsAndSizes.SetPositionAndSize(formSubOcr);
            formSubOcr.Initialize(subtitle, Configuration.Settings.VobSubOcr, false);
            if (formSubOcr.ShowDialog(this) == DialogResult.OK)
            {
                MakeHistoryForUndo(_language.BeforeImportingBdnXml);
                FileNew();
                _subtitle.Paragraphs.Clear();
                SetCurrentFormat(Configuration.Settings.General.DefaultSubtitleFormat);
                _subtitle.WasLoadedWithFrameNumbers = false;
                _subtitle.CalculateFrameNumbersFromTimeCodes(CurrentFrameRate);
                foreach (Paragraph p in formSubOcr.SubtitleFromOcr.Paragraphs)
                {
                    _subtitle.Paragraphs.Add(p);
                }

                ShowSource();
                SubtitleListview1.Fill(_subtitle, _subtitleAlternate);
                _subtitleListViewIndex = -1;
                SubtitleListview1.FirstVisibleIndex = -1;
                SubtitleListview1.SelectIndexAndEnsureVisible(0);

                _fileName = Path.ChangeExtension(formSubOcr.FileName, ".srt");
                SetTitle();
                _converted = true;
            }
            _formPositionsAndSizes.SavePositionAndSize(formSubOcr);
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
                Configuration.Settings.RecentFiles.Files.Count > 0)
            {
                reopenToolStripMenuItem.Visible = true;
                foreach (var file in Configuration.Settings.RecentFiles.Files)
                {
                    if (File.Exists(file.FileName))
                        reopenToolStripMenuItem.DropDownItems.Add(file.FileName, null, ReopenSubtitleToolStripMenuItemClick);
                }
            }
            else
            {
                Configuration.Settings.RecentFiles.Files.Clear();
                reopenToolStripMenuItem.Visible = false;
            }
        }

        private void ReopenSubtitleToolStripMenuItemClick(object sender, EventArgs e)
        {
            ReloadFromSourceView();
            var item = sender as ToolStripItem;

            if (ContinueNewOrExit())
            {
                RecentFileEntry rfe = null;
                foreach (var file in Configuration.Settings.RecentFiles.Files)
                {
                    if (file.FileName == item.Text)
                        rfe = file;
                }

                if (rfe == null)
                    OpenSubtitle(item.Text, null);
                else
                    OpenSubtitle(rfe.FileName, null, rfe.VideoFileName, rfe.OriginalFileName);
                SetRecentIndecies(item.Text);
                GotoSubPosAndPause();
            }
        }

        private void GotoSubPosAndPause()
        {
            if (!string.IsNullOrEmpty(_videoFileName))
            {
                _videoLoadedGoToSubPosAndPause = true;
            }
            else
            {
                mediaPlayer.SubtitleText = string.Empty;
            }
        }

        private void SetRecentIndecies(string fileName)
        {
            if (!Configuration.Settings.General.RememberSelectedLine)
                return;

            foreach (var x in Configuration.Settings.RecentFiles.Files)
            {
                if (fileName.Equals(x.FileName, StringComparison.OrdinalIgnoreCase))
                {
                    int sIndex = x.FirstSelectedIndex;
                    if (sIndex >= 0 && sIndex < SubtitleListview1.Items.Count)
                    {
                        SubtitleListview1.SelectedIndexChanged -= SubtitleListview1_SelectedIndexChanged;
                        for (int i = 0; i < SubtitleListview1.Items.Count; i++)
                            SubtitleListview1.Items[i].Selected = i == sIndex;
                        _subtitleListViewIndex = -1;
                        SubtitleListview1.EnsureVisible(sIndex);
                        SubtitleListview1.SelectedIndexChanged += SubtitleListview1_SelectedIndexChanged;
                        SubtitleListview1.Items[sIndex].Focused = true;
                    }

                    int topIndex = x.FirstVisibleIndex;
                    if (topIndex >= 0 && topIndex < SubtitleListview1.Items.Count)
                    {
                        // to fix bug in .net framework we have to set topitem 3 times... wtf!?
                        SubtitleListview1.TopItem = SubtitleListview1.Items[topIndex];
                        SubtitleListview1.TopItem = SubtitleListview1.Items[topIndex];
                        SubtitleListview1.TopItem = SubtitleListview1.Items[topIndex];
                    }

                    RefreshSelectedParagraph();
                    break;
                }
            }
        }

        private void SaveToolStripMenuItemClick(object sender, EventArgs e)
        {
            ReloadFromSourceView();
            SaveSubtitle(GetCurrentSubtitleFormat());
        }

        private void SaveAsToolStripMenuItemClick(object sender, EventArgs e)
        {
            ReloadFromSourceView();
            FileSaveAs(true);
        }

        private DialogResult FileSaveAs(bool allowUsingLastSaveAsFormat)
        {
            SubtitleFormat currentFormat = null;
            if (allowUsingLastSaveAsFormat && !string.IsNullOrEmpty(Configuration.Settings.General.LastSaveAsFormat))
                currentFormat = Utilities.GetSubtitleFormatByFriendlyName(Configuration.Settings.General.LastSaveAsFormat);
            if (currentFormat == null)
                currentFormat = GetCurrentSubtitleFormat();

            Utilities.SetSaveDialogFilter(saveFileDialog1, currentFormat);

            saveFileDialog1.Title = _language.SaveSubtitleAs;
            saveFileDialog1.DefaultExt = "*" + currentFormat.Extension;
            saveFileDialog1.AddExtension = true;

            if (!string.IsNullOrEmpty(_videoFileName))
                saveFileDialog1.FileName = Path.GetFileNameWithoutExtension(_videoFileName);
            else
                saveFileDialog1.FileName = Path.GetFileNameWithoutExtension(_fileName);

            if (!string.IsNullOrEmpty(openFileDialog1.InitialDirectory))
                saveFileDialog1.InitialDirectory = openFileDialog1.InitialDirectory;

            DialogResult result = saveFileDialog1.ShowDialog(this);
            if (result == DialogResult.OK)
            {
                openFileDialog1.InitialDirectory = saveFileDialog1.InitialDirectory;
                _converted = false;
                _fileName = saveFileDialog1.FileName;
                _fileDateTime = File.GetLastWriteTime(_fileName);
                SetTitle();
                MakeHistoryForUndo(_language.Menu.File.SaveAs);
                Configuration.Settings.RecentFiles.Add(_fileName, FirstVisibleIndex, FirstSelectedIndex, _videoFileName, _subtitleAlternateFileName);
                Configuration.Settings.Save();

                int index = 0;
                foreach (SubtitleFormat format in SubtitleFormat.AllSubtitleFormats)
                {
                    if (saveFileDialog1.FilterIndex == index + 1)
                    {
                        // only allow current extension or ".txt"
                        string ext = Path.GetExtension(_fileName).ToLower();
                        bool extOk = ext.Equals(format.Extension, StringComparison.OrdinalIgnoreCase) || format.AlternateExtensions.Contains(ext) || ext == ".txt";
                        if (!extOk)
                        {
                            if (_fileName.EndsWith('.'))
                                _fileName = _fileName.Substring(0, _fileName.Length - 1);
                            _fileName += format.Extension;
                        }

                        if (SaveSubtitle(format) == DialogResult.OK)
                        {
                            Configuration.Settings.General.LastSaveAsFormat = format.Name;
                            SetCurrentFormat(format);
                        }
                    }
                    index++;
                }
            }
            return result;
        }

        private DialogResult SaveSubtitle(SubtitleFormat format)
        {
            if (string.IsNullOrEmpty(_fileName) || _converted)
                return FileSaveAs(false);

            try
            {
                if (format != null && !format.IsTextBased)
                {
                    if (format.GetType() == typeof(Ebu))
                    {
                        Ebu.Save(_fileName, _subtitle);
                    }
                    return DialogResult.OK;
                }

                string allText = _subtitle.ToText(format);

                // Seungki begin
                if (_splitDualSami && _subtitleAlternate != null)
                {
                    var s = new Subtitle(_subtitle);
                    foreach (Paragraph p in _subtitleAlternate.Paragraphs)
                        s.Paragraphs.Add(p);
                    allText = s.ToText(format);
                }
                // Seungki end

                var currentEncoding = GetCurrentEncoding();
                bool isUnicode = currentEncoding == Encoding.Unicode || currentEncoding == Encoding.UTF32 || currentEncoding == Encoding.UTF7 || currentEncoding == Encoding.UTF8;
                if (!isUnicode && (allText.Contains('♪') || allText.Contains('♫') || allText.Contains('♥') || allText.Contains('—') || allText.Contains('―') || allText.Contains('…'))) // ANSI & music/unicode symbols
                {
                    if (MessageBox.Show(string.Format(_language.UnicodeMusicSymbolsAnsiWarning), Title, MessageBoxButtons.YesNo) == DialogResult.No)
                        return DialogResult.No;
                }

                if (!isUnicode)
                {
                    allText = allText.Replace("—", "-"); // mdash, code 8212
                    allText = allText.Replace("―", "-"); // mdash, code 8213
                    allText = allText.Replace("…", "...");
                    allText = allText.Replace("♪", "#");
                    allText = allText.Replace("♫", "#");
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
                    FileInfo fileInfo = new FileInfo(_fileName);
                    DateTime fileOnDisk = fileInfo.LastWriteTime;
                    if (_fileDateTime != fileOnDisk && _fileDateTime != new DateTime())
                    {
                        if (MessageBox.Show(string.Format(_language.OverwriteModifiedFile,
                                                          _fileName, fileOnDisk.ToShortDateString(), fileOnDisk.ToString("HH:mm:ss"),
                                                          Environment.NewLine, _fileDateTime.ToShortDateString(), _fileDateTime.ToString("HH:mm:ss")),
                                             Title + " - " + _language.FileOnDiskModified, MessageBoxButtons.YesNo) == DialogResult.No)
                            return DialogResult.No;
                    }
                    if (fileInfo.IsReadOnly)
                    {
                        if (string.IsNullOrEmpty(_language.FileXIsReadOnly))
                            MessageBox.Show("Cannot save " + _fileName + Environment.NewLine + "File is read-only!");
                        else
                            MessageBox.Show(string.Format(_language.FileXIsReadOnly, _fileName));
                        return DialogResult.No;
                    }
                }

                if (Control.ModifierKeys == (Keys.Control | Keys.Shift))
                    allText = allText.Replace("\r\n", "\n");

                if (format.GetType() == typeof(ItunesTimedText) || format.GetType() == typeof(ScenaristClosedCaptions) || format.GetType() == typeof(ScenaristClosedCaptionsDropFrame))
                {
                    Encoding outputEnc = new UTF8Encoding(false); // create encoding with no BOM
                    TextWriter file = new StreamWriter(_fileName, false, outputEnc); // open file with encoding
                    file.Write(allText);
                    file.Close(); // save and close it
                }
                else if (currentEncoding == Encoding.UTF8 && (format.GetType() == typeof(TmpegEncAW5) || format.GetType() == typeof(TmpegEncXml)))
                {
                    Encoding outputEnc = new UTF8Encoding(false); // create encoding with no BOM
                    TextWriter file = new StreamWriter(_fileName, false, outputEnc); // open file with encoding
                    file.Write(allText);
                    file.Close(); // save and close it
                }
                else
                {
                    if (string.IsNullOrWhiteSpace(allText))
                    {
                        MessageBox.Show(string.Format(_language.UnableToSaveSubtitleX, _fileName), String.Empty, MessageBoxButtons.OK, MessageBoxIcon.Stop);
                        return DialogResult.Cancel;
                    }

                    FileStream fs = null;
                    try
                    {
                        fs = File.Open(_fileName, FileMode.Create, FileAccess.Write, FileShare.Read);
                        using (StreamWriter sw = new StreamWriter(fs, currentEncoding))
                        {
                            fs = null;
                            sw.Write(allText);
                        }
                    }
                    finally
                    {
                        if (fs != null)
                            fs.Dispose();
                    }
                }

                _fileDateTime = File.GetLastWriteTime(_fileName);
                ShowStatus(string.Format(_language.SavedSubtitleX, _fileName));
                _changeSubtitleToString = SerializeSubtitle(_subtitle);
                return DialogResult.OK;
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
                return DialogResult.Cancel;
            }
        }

        private DialogResult SaveOriginalSubtitle(SubtitleFormat format)
        {
            try
            {
                string allText = _subtitleAlternate.ToText(format).Trim();
                var currentEncoding = GetCurrentEncoding();
                bool isUnicode = currentEncoding == Encoding.Unicode || currentEncoding == Encoding.UTF32 || currentEncoding == Encoding.UTF7 || currentEncoding == Encoding.UTF8;
                if (!isUnicode && (allText.Contains('♪') || allText.Contains('♫') || allText.Contains('♥') || allText.Contains('—') || allText.Contains('―') || allText.Contains('…'))) // ANSI & music/unicode symbols
                {
                    if (MessageBox.Show(string.Format(_language.UnicodeMusicSymbolsAnsiWarning), Title, MessageBoxButtons.YesNo) == DialogResult.No)
                        return DialogResult.No;
                }

                if (!isUnicode)
                {
                    allText = allText.Replace("—", "-"); // mdash, code 8212
                    allText = allText.Replace("―", "-"); // mdash, code 8213
                    allText = allText.Replace("…", "...");
                    allText = allText.Replace("♪", "#");
                    allText = allText.Replace("♫", "#");
                }

                bool containsNegativeTime = false;
                foreach (var p in _subtitleAlternate.Paragraphs)
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

                File.WriteAllText(_subtitleAlternateFileName, allText, currentEncoding);
                ShowStatus(string.Format(_language.SavedOriginalSubtitleX, _subtitleAlternateFileName));
                _changeAlternateSubtitleToString = _subtitleAlternate.ToText(new SubRip()).Trim();
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
            SetCurrentFormat(Configuration.Settings.General.DefaultSubtitleFormat);

            _subtitle = new Subtitle(_subtitle.HistoryItems);
            _changeAlternateSubtitleToString = string.Empty;
            _changeSubtitleToString = string.Empty;
            _subtitleAlternateFileName = null;
            textBoxSource.Text = string.Empty;
            SubtitleListview1.Items.Clear();
            _fileName = string.Empty;
            _fileDateTime = new DateTime();
            Text = Title;
            _oldSubtitleFormat = null;
            labelSingleLine.Text = string.Empty;
            RemoveAlternate(true);
            _splitDualSami = false;

            SubtitleListview1.HideExtraColumn();
            SubtitleListview1.DisplayExtraFromExtra = false;

            ComboBoxSubtitleFormatsSelectedIndexChanged(null, null);

            toolStripComboBoxFrameRate.Text = Configuration.Settings.General.DefaultFrameRate.ToString();

            SetEncoding(Configuration.Settings.General.DefaultEncoding);

            toolStripComboBoxFrameRate.Text = Configuration.Settings.General.DefaultFrameRate.ToString();
            _findHelper = null;
            _spellCheckForm = null;
            _videoFileName = null;
            _videoInfo = null;
            _videoAudioTrackNumber = -1;
            labelVideoInfo.Text = Configuration.Settings.Language.General.NoVideoLoaded;
            audioVisualizer.WavePeaks = null;
            audioVisualizer.ResetSpectrogram();
            audioVisualizer.Invalidate();

            _sourceViewChange = false;

            _subtitleListViewIndex = -1;
            textBoxListViewText.Text = string.Empty;
            textBoxListViewTextAlternate.Text = string.Empty;
            textBoxListViewText.Enabled = false;
            labelTextLineLengths.Text = string.Empty;
            labelCharactersPerSecond.Text = string.Empty;
            labelTextLineTotal.Text = string.Empty;

            _listViewTextUndoLast = null;
            _listViewAlternateTextUndoLast = null;
            _listViewTextUndoIndex = -1;

            if (mediaPlayer.VideoPlayer != null)
            {
                mediaPlayer.VideoPlayer.DisposeVideoPlayer();
                mediaPlayer.VideoPlayer = null;
            }

            _changeSubtitleToString = SerializeSubtitle(_subtitle);
            _converted = false;

            SetUndockedWindowsTitle();
            mediaPlayer.SubtitleText = string.Empty;
            ShowStatus(_language.New);
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
                if (!_loading)
                    MakeHistoryForUndo(string.Format(_language.BeforeConvertingToX, GetCurrentSubtitleFormat().FriendlyName));
            }
            else
            {
                _subtitle.MakeHistoryForUndo(string.Format(_language.BeforeConvertingToX, GetCurrentSubtitleFormat().FriendlyName), _oldSubtitleFormat, _fileDateTime, _subtitleAlternate, _subtitleAlternateFileName, _subtitleListViewIndex, textBoxListViewText.SelectionStart, textBoxListViewTextAlternate.SelectionStart);
                _oldSubtitleFormat.RemoveNativeFormatting(_subtitle, GetCurrentSubtitleFormat());
                SaveSubtitleListviewIndexes();
                SubtitleListview1.Fill(_subtitle, _subtitleAlternate);
                RestoreSubtitleListviewIndexes();

                if (_oldSubtitleFormat.HasStyleSupport && _networkSession == null)
                {
                    SubtitleListview1.HideExtraColumn();
                }
            }
            SubtitleFormat format = GetCurrentSubtitleFormat();
            if (_oldSubtitleFormat != null && !_oldSubtitleFormat.IsFrameBased && format.IsFrameBased)
                _subtitle.CalculateFrameNumbersFromTimeCodesNoCheck(CurrentFrameRate);
            else if (_oldSubtitleFormat != null && _oldSubtitleFormat.IsFrameBased && !format.IsFrameBased)
                _subtitle.CalculateTimeCodesFromFrameNumbers(CurrentFrameRate);
            ShowSource();
            SubtitleListview1.DisplayExtraFromExtra = false;
            if (format != null)
            {
                ShowStatus(string.Format(_language.ConvertedToX, format.FriendlyName));
                _oldSubtitleFormat = format;

                if (format.HasStyleSupport && _networkSession == null)
                {
                    List<string> styles = new List<string>();
                    if (format.GetType() == typeof(AdvancedSubStationAlpha) || format.GetType() == typeof(SubStationAlpha))
                        styles = AdvancedSubStationAlpha.GetStylesFromHeader(_subtitle.Header);
                    else if (format.GetType() == typeof(TimedText10) || format.GetType() == typeof(ItunesTimedText))
                        styles = TimedText10.GetStylesFromHeader(_subtitle.Header);
                    else if (format.GetType() == typeof(Sami) || format.GetType() == typeof(SamiModern))
                        styles = Sami.GetStylesFromHeader(_subtitle.Header);
                    else if (format.Name == "Nuendo")
                        styles = GetNuendoStyles();

                    foreach (Paragraph p in _subtitle.Paragraphs)
                    {
                        if (string.IsNullOrEmpty(p.Extra) && styles.Count > 0)
                            p.Extra = styles[0];
                    }

                    if (format.GetType() == typeof(Sami) || format.GetType() == typeof(SamiModern))
                        SubtitleListview1.ShowExtraColumn(Configuration.Settings.Language.General.Class);
                    else if (format.GetType() == typeof(TimedText10) || format.GetType() == typeof(ItunesTimedText))
                        SubtitleListview1.ShowExtraColumn("Style / Language");
                    else if (format.Name == "Nuendo")
                        SubtitleListview1.ShowExtraColumn("Character"); //TODO: Put in language xml file
                    else
                        SubtitleListview1.ShowExtraColumn(Configuration.Settings.Language.General.Style);

                    SubtitleListview1.DisplayExtraFromExtra = true;
                    SubtitleListview1.Fill(_subtitle, _subtitleAlternate);
                }
            }
            ShowHideTextBasedFeatures(format);
        }

        private static List<string> GetNuendoStyles()
        {
            if (!string.IsNullOrEmpty(Configuration.Settings.SubtitleSettings.NuendoCharacterListFile) && File.Exists(Configuration.Settings.SubtitleSettings.NuendoCharacterListFile))
            {
                return NuendoProperties.LoadCharacters(Configuration.Settings.SubtitleSettings.NuendoCharacterListFile);
            }
            return new List<string>();
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
            textBoxSource.TextChanged -= TextBoxSourceTextChanged;
            textBoxSource.Text = string.Empty;
            textBoxSource.TextChanged += TextBoxSourceTextChanged;
        }

        private void SettingsToolStripMenuItemClick(object sender, EventArgs e)
        {
            ShowSettings();
        }

        private void ShowSettings()
        {
            string oldVideoPlayer = Configuration.Settings.General.VideoPlayer;
            string oldListViewLineSeparatorString = Configuration.Settings.General.ListViewLineSeparatorString;
            string oldSubtitleFontSettings = Configuration.Settings.General.SubtitleFontName +
                                          Configuration.Settings.General.SubtitleFontBold +
                                          Configuration.Settings.General.CenterSubtitleInTextBox +
                                          Configuration.Settings.General.SubtitleFontSize +
                                          Configuration.Settings.General.SubtitleFontColor.ToArgb() +
                                          Configuration.Settings.General.SubtitleBackgroundColor.ToArgb();
            bool oldUseTimeFormatHHMMSSFF = Configuration.Settings.General.UseTimeFormatHHMMSSFF;

            string oldSyntaxColoring = Configuration.Settings.Tools.ListViewSyntaxColorDurationSmall.ToString() +
                                       Configuration.Settings.Tools.ListViewSyntaxColorDurationBig +
                                       Configuration.Settings.Tools.ListViewSyntaxColorLongLines +
                                       Configuration.Settings.Tools.ListViewSyntaxColorOverlap +
                                       Configuration.Settings.Tools.ListViewSyntaxMoreThanXLines +
                                       Configuration.Settings.Tools.ListViewSyntaxMoreThanXLinesX +
                                       Configuration.Settings.Tools.ListViewSyntaxErrorColor.ToArgb();

            var oldAllowEditOfOriginalSubtitle = Configuration.Settings.General.AllowEditOfOriginalSubtitle;
            using (var settings = new Settings())
            {
                settings.Initialize(this.Icon, toolStripButtonFileNew.Image, toolStripButtonFileOpen.Image, toolStripButtonSave.Image, toolStripButtonSaveAs.Image,
                    toolStripButtonFind.Image, toolStripButtonReplace.Image, toolStripButtonFixCommonErrors.Image, toolStripButtonVisualSync.Image, toolStripButtonSpellCheck.Image, toolStripButtonSettings.Image, toolStripButtonHelp.Image);
                _formPositionsAndSizes.SetPositionAndSize(settings);
                settings.ShowDialog(this);
                _formPositionsAndSizes.SavePositionAndSize(settings);
            }

            try
            { // can have some problems with fonts...
                Utilities.InitializeSubtitleFont(textBoxSource);
                Utilities.InitializeSubtitleFont(textBoxListViewText);
                Utilities.InitializeSubtitleFont(textBoxListViewTextAlternate);
                Utilities.InitializeSubtitleFont(SubtitleListview1);
                InitializeToolbar();
            }
            catch (Exception exception)
            {
                System.Diagnostics.Debug.WriteLine(exception.Message + Environment.NewLine + exception.StackTrace);
            }
            UpdateRecentFilesUI();
            buttonCustomUrl1.Text = Configuration.Settings.VideoControls.CustomSearchText1;
            buttonCustomUrl1.Visible = Configuration.Settings.VideoControls.CustomSearchUrl1.Length > 1;
            buttonCustomUrl2.Text = Configuration.Settings.VideoControls.CustomSearchText2;
            buttonCustomUrl2.Visible = Configuration.Settings.VideoControls.CustomSearchUrl2.Length > 1;

            audioVisualizer.DrawGridLines = Configuration.Settings.VideoControls.WaveformDrawGrid;
            audioVisualizer.GridColor = Configuration.Settings.VideoControls.WaveformGridColor;
            audioVisualizer.SelectedColor = Configuration.Settings.VideoControls.WaveformSelectedColor;
            audioVisualizer.Color = Configuration.Settings.VideoControls.WaveformColor;
            audioVisualizer.BackgroundColor = Configuration.Settings.VideoControls.WaveformBackgroundColor;
            audioVisualizer.TextColor = Configuration.Settings.VideoControls.WaveformTextColor;
            audioVisualizer.MouseWheelScrollUpIsForward = Configuration.Settings.VideoControls.WaveformMouseWheelScrollUpIsForward;
            audioVisualizer.AllowOverlap = Configuration.Settings.VideoControls.WaveformAllowOverlap;
            audioVisualizer.ClosenessForBorderSelection = Configuration.Settings.VideoControls.WaveformBorderHitMs;

            string newSyntaxColoring = Configuration.Settings.Tools.ListViewSyntaxColorDurationSmall.ToString() +
                           Configuration.Settings.Tools.ListViewSyntaxColorDurationBig +
                           Configuration.Settings.Tools.ListViewSyntaxColorLongLines +
                           Configuration.Settings.Tools.ListViewSyntaxColorOverlap +
                           Configuration.Settings.Tools.ListViewSyntaxMoreThanXLines +
                           Configuration.Settings.Tools.ListViewSyntaxMoreThanXLinesX +
                           Configuration.Settings.Tools.ListViewSyntaxErrorColor.ToArgb();

            if (oldSubtitleFontSettings != Configuration.Settings.General.SubtitleFontName +
                                          Configuration.Settings.General.SubtitleFontBold +
                                          Configuration.Settings.General.CenterSubtitleInTextBox +
                                          Configuration.Settings.General.SubtitleFontSize +
                                          Configuration.Settings.General.SubtitleFontColor.ToArgb() +
                                          Configuration.Settings.General.SubtitleBackgroundColor.ToArgb() ||
                                          oldSyntaxColoring != newSyntaxColoring)
            {
                try
                { // can have some problems with fonts...
                    Utilities.InitializeSubtitleFont(textBoxListViewText);
                    Utilities.InitializeSubtitleFont(textBoxListViewTextAlternate);
                    Utilities.InitializeSubtitleFont(textBoxSource);
                    SubtitleListview1.SubtitleFontName = Configuration.Settings.General.SubtitleFontName;
                    SubtitleListview1.SubtitleFontBold = Configuration.Settings.General.SubtitleFontBold;
                    SubtitleListview1.SubtitleFontSize = Configuration.Settings.General.SubtitleFontSize;
                }
                catch (Exception exception)
                {
                    System.Diagnostics.Debug.WriteLine(exception.Message + Environment.NewLine + exception.StackTrace);
                }

                SubtitleListview1.ForeColor = Configuration.Settings.General.SubtitleFontColor;
                SubtitleListview1.BackColor = Configuration.Settings.General.SubtitleBackgroundColor;
                if (Configuration.Settings.General.CenterSubtitleInTextBox)
                {
                    textBoxListViewText.TextAlign = HorizontalAlignment.Center;
                    textBoxListViewTextAlternate.TextAlign = HorizontalAlignment.Center;
                }
                else if (textBoxListViewText.TextAlign == HorizontalAlignment.Center)
                {
                    textBoxListViewText.TextAlign = HorizontalAlignment.Left;
                    textBoxListViewTextAlternate.TextAlign = HorizontalAlignment.Left;
                }

                SaveSubtitleListviewIndexes();
                Utilities.InitializeSubtitleFont(SubtitleListview1);
                SubtitleListview1.AutoSizeAllColumns(this);
                SubtitleListview1.Fill(_subtitle, _subtitleAlternate);
                RestoreSubtitleListviewIndexes();
                mediaPlayer.SetSubtitleFont();
                ShowSubtitle();
            }
            mediaPlayer.SetSubtitleFont();
            mediaPlayer.ShowStopButton = Configuration.Settings.General.VideoPlayerShowStopButton;
            mediaPlayer.ShowMuteButton = Configuration.Settings.General.VideoPlayerShowMuteButton;
            mediaPlayer.ShowFullscreenButton = Configuration.Settings.General.VideoPlayerShowFullscreenButton;

            if (oldListViewLineSeparatorString != Configuration.Settings.General.ListViewLineSeparatorString)
            {
                SubtitleListview1.InitializeLanguage(_languageGeneral, Configuration.Settings);
                SaveSubtitleListviewIndexes();
                SubtitleListview1.Fill(_subtitle, _subtitleAlternate);
                RestoreSubtitleListviewIndexes();
            }

            if (oldAllowEditOfOriginalSubtitle != Configuration.Settings.General.AllowEditOfOriginalSubtitle && _subtitleAlternate != null && _subtitleAlternate.Paragraphs.Count > 0)
            {
                if (Configuration.Settings.General.AllowEditOfOriginalSubtitle)
                {
                    AddAlternate();
                }
                else
                {
                    RemoveAlternate(false);
                }
                Main_Resize(null, null);
            }
            textBoxListViewTextAlternate.Enabled = Configuration.Settings.General.AllowEditOfOriginalSubtitle && _subtitleListViewIndex >= 0;

            SetShortcuts();

            _timerAutoSave.Stop();
            if (!string.IsNullOrEmpty(_videoFileName) && oldVideoPlayer != Configuration.Settings.General.VideoPlayer && mediaPlayer.VideoPlayer != null)
            {
                string vfn = _videoFileName;
                CloseVideoToolStripMenuItemClick(null, null);
                OpenVideo(vfn);
            }

            if (Configuration.Settings.General.AutoBackupSeconds > 0)
            {
                _timerAutoSave.Interval = 1000 * Configuration.Settings.General.AutoBackupSeconds; // take backup every x second if changes were made
                _timerAutoSave.Start();
            }
            SetTitle();
            if (Configuration.Settings.VideoControls.GenerateSpectrogram)
            {
                audioVisualizer.WaveformNotLoadedText = Configuration.Settings.Language.Waveform.ClickToAddWaveformAndSpectrogram;
                showhideWaveformToolStripMenuItem.Text = _language.Menu.Video.ShowHideWaveformAndSpectrogram;
            }
            else
            {
                audioVisualizer.WaveformNotLoadedText = Configuration.Settings.Language.Waveform.ClickToAddWaveform;
                showhideWaveformToolStripMenuItem.Text = _language.Menu.Video.ShowHideWaveform;
            }
            audioVisualizer.Invalidate();

            if (oldUseTimeFormatHHMMSSFF != Configuration.Settings.General.UseTimeFormatHHMMSSFF)
                RefreshTimeCodeMode();
        }

        private void AddAlternate()
        {
            buttonUnBreak.Visible = false;
            buttonAutoBreak.Visible = false;
            buttonSplitLine.Visible = false;
            textBoxListViewTextAlternate.Visible = true;
            labelAlternateText.Visible = true;
            labelAlternateCharactersPerSecond.Visible = true;
            labelTextAlternateLineLengths.Visible = true;
            labelAlternateSingleLine.Visible = true;
            labelTextAlternateLineTotal.Visible = true;
        }

        private int ShowSubtitle()
        {
            if (_splitDualSami)
                return Utilities.ShowSubtitle(_subtitle.Paragraphs, _subtitleAlternate, mediaPlayer);
            if (SubtitleListview1.IsAlternateTextColumnVisible && Configuration.Settings.General.ShowOriginalAsPreviewIfAvailable)
                return Utilities.ShowSubtitle(_subtitleAlternate.Paragraphs, mediaPlayer);
            return Utilities.ShowSubtitle(_subtitle.Paragraphs, mediaPlayer);
        }

        private static void TryLoadIcon(ToolStripButton button, string iconName)
        {
            string fullPath = Configuration.IconsFolder + iconName + ".png";
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

            TryLoadIcon(toolStripButtonToggleVideo, "VideoToggle");
            TryLoadIcon(toolStripButtonToggleWaveform, "WaveformToggle");

            toolStripButtonFileNew.Visible = gs.ShowToolbarNew;
            toolStripButtonFileOpen.Visible = gs.ShowToolbarOpen;
            toolStripButtonSave.Visible = gs.ShowToolbarSave;
            toolStripButtonSaveAs.Visible = gs.ShowToolbarSaveAs;
            toolStripButtonFind.Visible = gs.ShowToolbarFind;
            toolStripButtonReplace.Visible = gs.ShowToolbarReplace;
            toolStripButtonVisualSync.Visible = gs.ShowToolbarVisualSync;
            toolStripButtonFixCommonErrors.Visible = gs.ShowToolbarFixCommonErrors;
            toolStripButtonSettings.Visible = gs.ShowToolbarSettings;
            toolStripButtonSpellCheck.Visible = gs.ShowToolbarSpellCheck;
            toolStripButtonHelp.Visible = gs.ShowToolbarHelp;

            toolStripSeparatorFrameRate.Visible = gs.ShowFrameRate;
            toolStripLabelFrameRate.Visible = gs.ShowFrameRate;
            toolStripComboBoxFrameRate.Visible = gs.ShowFrameRate;
            toolStripButtonGetFrameRate.Visible = gs.ShowFrameRate;

            toolStripSeparatorFindReplace.Visible = gs.ShowToolbarFind || gs.ShowToolbarReplace;
            toolStripSeparatorHelp.Visible = gs.ShowToolbarHelp;

            toolStrip1.Visible = gs.ShowToolbarNew || gs.ShowToolbarOpen || gs.ShowToolbarSave || gs.ShowToolbarSaveAs || gs.ShowToolbarFind || gs.ShowToolbarReplace ||
                                 gs.ShowToolbarVisualSync || gs.ShowToolbarSettings || gs.ShowToolbarSpellCheck || gs.ShowToolbarHelp;
        }

        private void ToolStripButtonFileNewClick(object sender, EventArgs e)
        {
            _lastDoNotPrompt = string.Empty;
            ReloadFromSourceView();
            FileNew();
            ShowHideTextBasedFeatures(GetCurrentSubtitleFormat());
        }

        private void ToolStripButtonFileOpenClick(object sender, EventArgs e)
        {
            toolStripButtonFileOpen.Enabled = false;
            ReloadFromSourceView();
            OpenNewFile();
            toolStripButtonFileOpen.Enabled = true;
        }

        private void ToolStripButtonSaveClick(object sender, EventArgs e)
        {
            ReloadFromSourceView();
            bool oldChange = _changeSubtitleToString != SerializeSubtitle(_subtitle);
            SaveSubtitle(GetCurrentSubtitleFormat());

            if (_subtitleAlternate != null && _changeAlternateSubtitleToString != _subtitleAlternate.ToText(new SubRip()).Trim() && Configuration.Settings.General.AllowEditOfOriginalSubtitle && _subtitleAlternate.Paragraphs.Count > 0)
            {
                SaveOriginalToolStripMenuItemClick(null, null);
                if (oldChange && _changeSubtitleToString == SerializeSubtitle(_subtitle) && _changeAlternateSubtitleToString == _subtitleAlternate.ToText(new SubRip()).Trim())
                    ShowStatus(string.Format(_language.SavedSubtitleX, Path.GetFileName(_fileName)) + " + " +
                        string.Format(_language.SavedOriginalSubtitleX, Path.GetFileName(_subtitleAlternateFileName)));
            }
        }

        private void ToolStripButtonSaveAsClick(object sender, EventArgs e)
        {
            ReloadFromSourceView();
            FileSaveAs(true);
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
            labelStatus.Text = string.Empty;
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
            openFileDialog1.Filter = Utilities.GetVideoFileFilter(false);
            if (openFileDialog1.ShowDialog(this) == DialogResult.OK)
            {
                _videoFileName = openFileDialog1.FileName;
                VideoInfo info = Utilities.GetVideoInfo(openFileDialog1.FileName);
                if (info != null && info.Success)
                {
                    string oldFrameRate = toolStripComboBoxFrameRate.Text;
                    toolStripComboBoxFrameRate.Text = string.Format("{0:0.###}", info.FramesPerSecond);

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

            using (var findDialog = new FindDialog())
            {
                findDialog.SetIcon(toolStripButtonFind.Image as Bitmap);
                findDialog.Initialize(selectedText, _findHelper);
                if (findDialog.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }

                _findHelper = findDialog.GetFindDialogHelper(_subtitleListViewIndex);
                _findHelper.AddHistory(_findHelper.FindText);
                ShowStatus(string.Format(_language.SearchingForXFromLineY, _findHelper.FindText, _subtitleListViewIndex + 1));
                if (tabControlSubtitle.SelectedIndex == TabControlListView)
                {
                    int selectedIndex = -1;
                    //set the starting selectedIndex if a row is highlighted
                    if (SubtitleListview1.SelectedItems.Count > 0)
                        selectedIndex = SubtitleListview1.SelectedItems[0].Index;

                    //if we fail to find the text, we might want to start searching from the top of the file.
                    bool foundIt = false;
                    if (_findHelper.Find(_subtitle, _subtitleAlternate, selectedIndex))
                    {
                        foundIt = true;
                    }
                    else if (_findHelper.StartLineIndex >= 1)
                    {
                        if (MessageBox.Show(_language.FindContinue, _language.FindContinueTitle, MessageBoxButtons.YesNo) == DialogResult.Yes)
                        {
                            selectedIndex = -1;
                            if (_findHelper.Find(_subtitle, _subtitleAlternate, selectedIndex))
                                foundIt = true;
                        }
                    }

                    if (foundIt)
                    {
                        SubtitleListview1.SelectIndexAndEnsureVisible(_findHelper.SelectedIndex);
                        TextBox tb;
                        if (_findHelper.MatchInOriginal)
                            tb = textBoxListViewTextAlternate;
                        else
                            tb = textBoxListViewText;
                        tb.Focus();
                        tb.SelectionStart = _findHelper.SelectedPosition;
                        tb.SelectionLength = _findHelper.FindTextLength;
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
                    if (_findHelper.FindNext(_subtitle, _subtitleAlternate, selectedIndex, _findHelper.SelectedPosition, Configuration.Settings.General.AllowEditOfOriginalSubtitle))
                    {
                        SubtitleListview1.SelectIndexAndEnsureVisible(_findHelper.SelectedIndex);
                        ShowStatus(string.Format(_language.XFoundAtLineNumberY, _findHelper.FindText, _findHelper.SelectedIndex + 1));
                        TextBox tb;
                        if (_findHelper.MatchInOriginal)
                            tb = textBoxListViewTextAlternate;
                        else
                            tb = textBoxListViewText;
                        tb.Focus();
                        tb.SelectionStart = _findHelper.SelectedPosition;
                        tb.SelectionLength = _findHelper.FindTextLength;
                        _findHelper.SelectedPosition++;
                    }
                    else
                    {
                        if (_findHelper.StartLineIndex >= 1)
                        {
                            if (MessageBox.Show(_language.FindContinue, _language.FindContinueTitle, MessageBoxButtons.YesNo) == DialogResult.Yes)
                            {
                                _findHelper.StartLineIndex = 0;
                                if (_findHelper.Find(_subtitle, _subtitleAlternate, 0))
                                {
                                    SubtitleListview1.SelectIndexAndEnsureVisible(_findHelper.SelectedIndex);
                                    TextBox tb;
                                    if (_findHelper.MatchInOriginal)
                                        tb = textBoxListViewTextAlternate;
                                    else
                                        tb = textBoxListViewText;
                                    tb.Focus();
                                    tb.SelectionStart = _findHelper.SelectedPosition;
                                    tb.SelectionLength = _findHelper.FindTextLength;
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
            else
            {
                Find();
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
                replaceDialog.SetIcon(toolStripButtonReplace.Image as Bitmap);
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
                ShowStatus(string.Format(_language.SearchingForXFromLineY, _findHelper.FindText, _subtitleListViewIndex + 1));
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
                        _makeHistoryPaused = true;
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
            if (_makeHistoryPaused)
                RestartHistory();
            replaceDialog.Dispose();
        }

        private void ReplaceListView(ReplaceDialog replaceDialog)
        {
            SaveSubtitleListviewIndexes();
            int firstIndex = FirstSelectedIndex;
            bool isFirst = true;
            string selectedText = textBoxListViewText.SelectedText;
            if (selectedText.Length == 0 && _findHelper != null)
                selectedText = _findHelper.FindText;

            if (replaceDialog == null)
            {
                replaceDialog = new ReplaceDialog();
                replaceDialog.SetIcon(toolStripButtonReplace.Image as Bitmap);
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
                    bool success = _findHelper.Success;
                    _findHelper = replaceDialog.GetFindDialogHelper(_subtitleListViewIndex);
                    _findHelper.SelectedIndex = line;
                    _findHelper.SelectedPosition = pos;
                    _findHelper.Success = success;
                }
                ShowStatus(string.Format(_language.SearchingForXFromLineY, _findHelper.FindText, _subtitleListViewIndex + 1));
                int replaceCount = 0;
                bool searchStringFound = true;
                while (searchStringFound)
                {
                    searchStringFound = false;
                    if (isFirst)
                    {
                        MakeHistoryForUndo(string.Format(_language.BeforeReplace, _findHelper.FindText));
                        isFirst = false;
                        _makeHistoryPaused = true;
                    }

                    if (replaceDialog.ReplaceAll)
                    {
                        if (_findHelper.FindNext(_subtitle, _subtitleAlternate, _findHelper.SelectedIndex, _findHelper.SelectedPosition, Configuration.Settings.General.AllowEditOfOriginalSubtitle))
                        {
                            textBoxListViewText.Visible = false;
                            SetTextForFindAndReplace(true);
                            _findHelper.SelectedPosition += _findHelper.ReplaceText.Length;
                            searchStringFound = true;
                            replaceCount++;
                        }
                        else
                        {
                            textBoxListViewText.Visible = true;
                            _subtitleListViewIndex = -1;
                            if (firstIndex >= 0 && firstIndex < SubtitleListview1.Items.Count)
                            {
                                SubtitleListview1.Items[firstIndex].Selected = true;
                                SubtitleListview1.Items[firstIndex].Focused = true;
                                SubtitleListview1.Focus();
                                textBoxListViewText.Text = _subtitle.Paragraphs[firstIndex].Text;
                                if (_subtitleAlternate != null && textBoxListViewTextAlternate.Visible)
                                {
                                    var orginial = Utilities.GetOriginalParagraph(_findHelper.SelectedIndex, _subtitle.Paragraphs[_findHelper.SelectedIndex], _subtitleAlternate.Paragraphs);
                                    if (orginial != null)
                                        textBoxListViewTextAlternate.Text = orginial.Text;
                                }
                            }
                            else
                            {
                                SubtitleListview1.SelectIndexAndEnsureVisible(0, true);
                            }
                            ShowStatus(string.Format(_language.NoMatchFoundX, _findHelper.FindText));

                            if (_replaceStartLineIndex >= 1) // Prompt for start over
                            {
                                _replaceStartLineIndex = 0;
                                string msgText = _language.ReplaceContinueNotFound;
                                if (replaceCount > 0)
                                    msgText = string.Format(_language.ReplaceXContinue, replaceCount);
                                if (MessageBox.Show(msgText, _language.ReplaceContinueTitle, MessageBoxButtons.YesNo) == DialogResult.Yes)
                                {
                                    _findHelper.StartLineIndex = 0;
                                    _findHelper.SelectedIndex = 0;
                                    _findHelper.SelectedPosition = 0;
                                    SetTextForFindAndReplace(false);

                                    if (_findHelper.FindNext(_subtitle, _subtitleAlternate, _findHelper.SelectedIndex, _findHelper.SelectedPosition, Configuration.Settings.General.AllowEditOfOriginalSubtitle))
                                    {
                                        SetTextForFindAndReplace(true);
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
                        if (_findHelper.FindNext(_subtitle, _subtitleAlternate, _findHelper.SelectedIndex, _findHelper.SelectedPosition, Configuration.Settings.General.AllowEditOfOriginalSubtitle))
                        {
                            SubtitleListview1.SelectIndexAndEnsureVisible(_findHelper.SelectedIndex, true);
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
                                if (_findHelper.FindNext(_subtitle, _subtitleAlternate, _findHelper.SelectedIndex, _findHelper.SelectedPosition, Configuration.Settings.General.AllowEditOfOriginalSubtitle))
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
                        if (_findHelper.FindType == FindType.RegEx && _findHelper.Success)
                        {
                            if (_findHelper.FindType == FindType.RegEx)
                            {
                                var r = new Regex(_findHelper.FindText, RegexOptions.Multiline);
                                string result = r.Replace(textBoxListViewText.SelectedText, _findHelper.ReplaceText);
                                if (result != textBoxListViewText.SelectedText)
                                {
                                    textBoxListViewText.SelectedText = result;
                                }
                            }
                            else
                            {
                                textBoxListViewText.SelectedText = _findHelper.ReplaceText;
                            }
                            msg = _language.OneReplacementMade + " ";
                        }
                        else if (textBoxListViewText.SelectionLength == _findHelper.FindTextLength)
                        {
                            textBoxListViewText.SelectedText = _findHelper.ReplaceText;
                            msg = _language.OneReplacementMade + " ";
                        }

                        if (_findHelper.FindNext(_subtitle, _subtitleAlternate, _findHelper.SelectedIndex, _findHelper.SelectedPosition, Configuration.Settings.General.AllowEditOfOriginalSubtitle))
                        {
                            SubtitleListview1.SelectIndexAndEnsureVisible(_findHelper.SelectedIndex);
                            if (_findHelper.MatchInOriginal)
                            {
                                textBoxListViewTextAlternate.Focus();
                                textBoxListViewTextAlternate.SelectionStart = _findHelper.SelectedPosition;
                                textBoxListViewTextAlternate.SelectionLength = _findHelper.FindTextLength;
                            }
                            else
                            {
                                textBoxListViewText.Focus();
                                textBoxListViewText.SelectionStart = _findHelper.SelectedPosition;
                                textBoxListViewText.SelectionLength = _findHelper.FindTextLength;
                            }
                            _findHelper.SelectedPosition += _findHelper.ReplaceText.Length;
                            ShowStatus(string.Format(msg + _language.XFoundAtLineNumberY, _findHelper.FindText, _findHelper.SelectedIndex + 1));
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

                                    if (_findHelper.FindNext(_subtitle, _subtitleAlternate, _findHelper.SelectedIndex, _findHelper.SelectedPosition, Configuration.Settings.General.AllowEditOfOriginalSubtitle))
                                    {
                                        SubtitleListview1.SelectIndexAndEnsureVisible(_findHelper.SelectedIndex, true);
                                        textBoxListViewText.Focus();
                                        textBoxListViewText.SelectionStart = _findHelper.SelectedPosition;
                                        textBoxListViewText.SelectionLength = _findHelper.FindTextLength;
                                        _findHelper.SelectedPosition += _findHelper.ReplaceText.Length;
                                        ShowStatus(string.Format(msg + _language.XFoundAtLineNumberY, _findHelper.FindText, _findHelper.SelectedIndex + 1));
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
            RestoreSubtitleListviewIndexes();
            if (_makeHistoryPaused)
                RestartHistory();
            replaceDialog.Dispose();
        }

        private void SetTextForFindAndReplace(bool replace)
        {
            _subtitleListViewIndex = _findHelper.SelectedIndex;
            textBoxListViewText.Text = _subtitle.Paragraphs[_findHelper.SelectedIndex].Text;
            if (_subtitleAlternate != null && textBoxListViewTextAlternate.Visible)
            {
                var orginial = Utilities.GetOriginalParagraph(_findHelper.SelectedIndex, _subtitle.Paragraphs[_findHelper.SelectedIndex], _subtitleAlternate.Paragraphs);
                if (orginial != null)
                    textBoxListViewTextAlternate.Text = orginial.Text;
            }

            if (replace)
            {
                if (_findHelper.MatchInOriginal)
                {
                    textBoxListViewTextAlternate.SelectionStart = _findHelper.SelectedPosition;
                    textBoxListViewTextAlternate.SelectionLength = _findHelper.FindTextLength;
                    textBoxListViewTextAlternate.SelectedText = _findHelper.ReplaceText;
                }
                else
                {
                    textBoxListViewText.SelectionStart = _findHelper.SelectedPosition;
                    textBoxListViewText.SelectionLength = _findHelper.FindTextLength;
                    if (_findHelper.FindType == FindType.RegEx)
                    {
                        var r = new Regex(_findHelper.FindText, RegexOptions.Multiline);
                        string result = r.Replace(textBoxListViewText.SelectedText, _findHelper.ReplaceText);
                        if (result != textBoxListViewText.SelectedText)
                        {
                            textBoxListViewText.SelectedText = result;
                        }
                    }
                    else
                    {
                        textBoxListViewText.SelectedText = _findHelper.ReplaceText;
                    }
                }
            }
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
            if (!string.IsNullOrEmpty(message))
            {
                _timerClearStatus.Stop();
                _statusLog.AppendLine(string.Format("{0:0000}-{1:00}-{2:00} {3}: {4}", DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.ToLongTimeString(), message));
                _timerClearStatus.Start();
            }
            ShowSourceLineNumber();
            ShowLineInformationListView();
        }

        private void ReloadFromSourceView()
        {
            if (_sourceViewChange)
            {
                SaveSubtitleListviewIndexes();
                if (!string.IsNullOrWhiteSpace(textBoxSource.Text))
                {
                    var temp = new Subtitle(_subtitle);
                    SubtitleFormat format = GetCurrentSubtitleFormat();
                    var list = new List<string>(textBoxSource.Lines);
                    if (format != null && format.IsMine(list, null))
                        format.LoadSubtitle(temp, list, null);
                    else
                        format = temp.ReloadLoadSubtitle(new List<string>(textBoxSource.Lines), null);

                    if (format == null)
                    {
                        MessageBox.Show(_language.UnableToParseSourceView);
                        return;
                    }
                    else
                    {
                        _sourceViewChange = false;
                        MakeHistoryForUndo(_language.BeforeChangesMadeInSourceView);
                        _subtitle.ReloadLoadSubtitle(new List<string>(textBoxSource.Lines), null);
                        if (format.IsFrameBased)
                            _subtitle.CalculateTimeCodesFromFrameNumbers(CurrentFrameRate);
                        int index = 0;
                        foreach (object obj in comboBoxSubtitleFormats.Items)
                        {
                            if (obj.ToString() == format.FriendlyName)
                                comboBoxSubtitleFormats.SelectedIndex = index;
                            index++;
                        }

                        if (format.GetType() == typeof(AdvancedSubStationAlpha) || format.GetType() == typeof(SubStationAlpha))
                        {
                            string errors = AdvancedSubStationAlpha.CheckForErrors(_subtitle.Header);
                            if (!string.IsNullOrEmpty(errors))
                                MessageBox.Show(errors, Title, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                        else if (format.GetType() == typeof(SubRip))
                        {
                            string errors = (format as SubRip).Errors;
                            if (!string.IsNullOrEmpty(errors))
                                MessageBox.Show(errors, Title, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                        else if (format.GetType() == typeof(MicroDvd))
                        {
                            string errors = (format as MicroDvd).Errors;
                            if (!string.IsNullOrEmpty(errors))
                                MessageBox.Show(errors, Title, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                    }
                }
                else
                {
                    _sourceViewChange = false;
                    MakeHistoryForUndo(_language.BeforeChangesMadeInSourceView);
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
            if (!IsSubtitleLoaded)
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
                    SubtitleListview1.Items[goToLine.LineNumber - 1].Focused = true;
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
            if (IsSubtitleLoaded)
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
                    else if (adjustDisplayTime.AdjustUsingSeconds)
                    {
                        double seconds = double.Parse(adjustDisplayTime.AdjustValue);
                        _subtitle.AdjustDisplayTimeUsingSeconds(seconds, selectedIndexes);
                    }
                    else
                    { // recalculate durations!!!
                        double maxCharSeconds = (double)(adjustDisplayTime.MaxCharactersPerSecond);
                        _subtitle.RecalculateDisplayTimes(maxCharSeconds, selectedIndexes);
                    }
                    ShowStatus(string.Format(_language.DisplayTimesAdjustedX, adjustDisplayTime.AdjustValue));
                    SaveSubtitleListviewIndexes();
                    if (IsFramesRelevant && CurrentFrameRate > 0)
                    {
                        _subtitle.CalculateFrameNumbersFromTimeCodesNoCheck(CurrentFrameRate);
                        if (tabControlSubtitle.SelectedIndex == TabControlSourceView)
                            ShowSource();
                    }
                    ShowSource();
                    SubtitleListview1.Fill(_subtitle, _subtitleAlternate);
                    RestoreSubtitleListviewIndexes();
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
            if (_networkSession == null)
                FixCommonErrors(false);
        }

        private void FixCommonErrors(bool onlySelectedLines)
        {
            if (IsSubtitleLoaded)
            {
                ReloadFromSourceView();
                SaveSubtitleListviewIndexes();
                var fixErrors = new FixCommonErrors();
                if (onlySelectedLines)
                {
                    var selectedLines = new Subtitle { WasLoadedWithFrameNumbers = _subtitle.WasLoadedWithFrameNumbers };
                    foreach (int index in SubtitleListview1.SelectedIndices)
                        selectedLines.Paragraphs.Add(_subtitle.Paragraphs[index]);
                    fixErrors.Initialize(selectedLines, GetCurrentSubtitleFormat(), GetCurrentEncoding());
                }
                else
                {
                    fixErrors.Initialize(_subtitle, GetCurrentSubtitleFormat(), GetCurrentEncoding());
                }

                if (fixErrors.ShowDialog(this) == DialogResult.OK)
                {
                    MakeHistoryForUndo(_language.BeforeCommonErrorFixes);
                    _subtitle.Renumber(1);
                    if (onlySelectedLines)
                    { // we only update selected lines
                        int i = 0;
                        List<int> deletes = new List<int>();
                        if (_networkSession != null)
                        {
                            _networkSession.TimerStop();
                            foreach (int index in SubtitleListview1.SelectedIndices)
                            {
                                var pOld = _subtitle.Paragraphs[index];
                                var p = fixErrors.FixedSubtitle.GetParagraphOrDefaultById(pOld.ID);
                                if (p == null)
                                {
                                    deletes.Add(index);
                                }
                                else
                                {
                                    _subtitle.Paragraphs[index] = p;
                                    SubtitleListview1.SetTimeAndText(index, p);
                                }
                                i++;
                            }
                            NetworkGetSendUpdates(deletes, 0, null);
                        }
                        else
                        {
                            foreach (int index in SubtitleListview1.SelectedIndices)
                            {
                                var pOld = _subtitle.Paragraphs[index];
                                var p = fixErrors.FixedSubtitle.GetParagraphOrDefaultById(pOld.ID);
                                if (p == null)
                                {
                                    deletes.Add(index);
                                }
                                else
                                {
                                    _subtitle.Paragraphs[index] = p;
                                }
                                i++;
                            }
                            deletes.Reverse();
                            foreach (int index in deletes)
                            {
                                _subtitle.Paragraphs.RemoveAt(index);
                            }
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
                    _subtitle.Renumber(1);
                    ShowSource();
                    SubtitleListview1.Fill(_subtitle, _subtitleAlternate);
                    RestoreSubtitleListviewIndexes();
                }
                Configuration.Settings.CommonErrors.StartSize = fixErrors.Width + ";" + fixErrors.Height;
                Configuration.Settings.CommonErrors.StartPosition = fixErrors.Left + ";" + fixErrors.Top;
            }
            else
            {
                MessageBox.Show(_language.NoSubtitleLoaded, Title, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            ShowInTaskbar = true;
        }

        private void StartNumberingFromToolStripMenuItemClick(object sender, EventArgs e)
        {
            if (IsSubtitleLoaded)
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
                _subtitle.Renumber(1);
        }

        private void RemoveTextForHearImparedToolStripMenuItemClick(object sender, EventArgs e)
        {
            if (IsSubtitleLoaded)
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
            if (IsSubtitleLoaded)
            {
                ReloadFromSourceView();

                if (Configuration.Settings.Tools.SplitAdvanced)
                {
                    var split = new Split();
                    double lengthInSeconds = 0;
                    if (mediaPlayer.VideoPlayer != null)
                        lengthInSeconds = mediaPlayer.Duration;
                    split.Initialize(_subtitle, _fileName, GetCurrentSubtitleFormat());
                    if (split.ShowDialog(this) == DialogResult.OK)
                    {
                        ShowStatus(_language.SubtitleSplitted);
                    }
                    else if (split.ShowBasic)
                    {
                        Configuration.Settings.Tools.SplitAdvanced = false;
                        SplitToolStripMenuItemClick(null, null);
                    }
                }
                else
                {
                    var splitSubtitle = new SplitSubtitle();
                    double lengthInSeconds = 0;
                    if (mediaPlayer.VideoPlayer != null)
                        lengthInSeconds = mediaPlayer.Duration;
                    splitSubtitle.Initialize(_subtitle, _fileName, GetCurrentSubtitleFormat(), GetCurrentEncoding(), lengthInSeconds);
                    if (splitSubtitle.ShowDialog(this) == DialogResult.OK)
                    {
                        ShowStatus(_language.SubtitleSplitted);
                    }
                    else if (splitSubtitle.ShowAdvanced)
                    {
                        Configuration.Settings.Tools.SplitAdvanced = true;
                        SplitToolStripMenuItemClick(null, null);
                    }
                }
            }
            else
            {
                MessageBox.Show(_language.NoSubtitleLoaded, Title, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void AppendTextVisuallyToolStripMenuItemClick(object sender, EventArgs e)
        {
            if (IsSubtitleLoaded)
            {
                ReloadFromSourceView();

                if (MessageBox.Show(_language.SubtitleAppendPrompt, _language.SubtitleAppendPromptTitle, MessageBoxButtons.YesNoCancel) == DialogResult.Yes)
                {
                    openFileDialog1.Title = _language.OpenSubtitleToAppend;
                    openFileDialog1.FileName = string.Empty;
                    openFileDialog1.Filter = Utilities.GetOpenDialogFilter();
                    if (openFileDialog1.ShowDialog(this) == DialogResult.OK)
                    {
                        bool success = false;
                        string fileName = openFileDialog1.FileName;
                        if (File.Exists(fileName))
                        {
                            var subtitleToAppend = new Subtitle();
                            SubtitleFormat format = null;

                            // do not allow blu-ray/vobsub
                            string extension = Path.GetExtension(fileName).ToLower();
                            if (extension == ".sub" && (IsVobSubFile(fileName, false) || FileUtils.IsSpDvdSup(fileName)))
                            {
                                format = null;
                            }
                            else if (extension == ".sup" && FileUtils.IsBluRaySup(fileName))
                            {
                                format = null;
                            }
                            else
                            {
                                Encoding encoding;
                                format = subtitleToAppend.LoadSubtitle(fileName, out encoding, null);
                                if (GetCurrentSubtitleFormat().IsFrameBased)
                                    subtitleToAppend.CalculateTimeCodesFromFrameNumbers(CurrentFrameRate);
                                else
                                    subtitleToAppend.CalculateFrameNumbersFromTimeCodes(CurrentFrameRate);
                            }

                            if (format != null)
                            {
                                if (subtitleToAppend.Paragraphs.Count > 1)
                                {
                                    var visualSync = new VisualSync();
                                    visualSync.Initialize(toolStripButtonVisualSync.Image as Bitmap, subtitleToAppend, _fileName, _language.AppendViaVisualSyncTitle, CurrentFrameRate);
                                    visualSync.ShowDialog(this);
                                    if (visualSync.OkPressed)
                                    {
                                        if (MessageBox.Show(_language.AppendSynchronizedSubtitlePrompt, _language.SubtitleAppendPromptTitle, MessageBoxButtons.YesNo) == DialogResult.Yes)
                                        {
                                            int start = _subtitle.Paragraphs.Count + 1;
                                            var fr = CurrentFrameRate;
                                            MakeHistoryForUndo(_language.BeforeAppend);
                                            foreach (Paragraph p in visualSync.Paragraphs)
                                            {
                                                if (format.IsFrameBased)
                                                    p.CalculateFrameNumbersFromTimeCodes(fr);
                                                _subtitle.Paragraphs.Add(new Paragraph(p));
                                            }
                                            if (format.GetType() == typeof(AdvancedSubStationAlpha) && GetCurrentSubtitleFormat().GetType() == typeof(AdvancedSubStationAlpha))
                                            {
                                                List<string> currentStyles = new List<string>();
                                                if (_subtitle.Header != null)
                                                    currentStyles = AdvancedSubStationAlpha.GetStylesFromHeader(_subtitle.Header);
                                                foreach (string styleName in AdvancedSubStationAlpha.GetStylesFromHeader(subtitleToAppend.Header))
                                                {
                                                    bool alreadyExists = false;
                                                    foreach (string currentStyleName in currentStyles)
                                                    {
                                                        if (currentStyleName.Trim().Equals(styleName.Trim(), StringComparison.OrdinalIgnoreCase))
                                                            alreadyExists = true;
                                                    }
                                                    if (!alreadyExists)
                                                    {
                                                        var newStyle = AdvancedSubStationAlpha.GetSsaStyle(styleName, subtitleToAppend.Header);
                                                        _subtitle.Header = AdvancedSubStationAlpha.AddSsaStyle(newStyle, _subtitle.Header);
                                                    }
                                                }
                                            }

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
            if (!IsSubtitleLoaded)
            {
                MessageBox.Show(_language.NoSubtitleLoaded, Title, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            bool isAlternateVisible = SubtitleListview1.IsAlternateTextColumnVisible;
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
                googleTranslate.Initialize(selectedLines, title, useGoogle, GetCurrentEncoding());
            }
            else
            {
                googleTranslate.Initialize(_subtitle, title, useGoogle, GetCurrentEncoding());
            }
            if (googleTranslate.ShowDialog(this) == DialogResult.OK)
            {
                _subtitleListViewIndex = -1;

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
                    _subtitleAlternate = new Subtitle(_subtitle);
                    _subtitleAlternateFileName = _fileName;
                    _fileName = null;
                    _subtitle.Paragraphs.Clear();
                    foreach (Paragraph p in googleTranslate.TranslatedSubtitle.Paragraphs)
                        _subtitle.Paragraphs.Add(new Paragraph(p));
                    ShowStatus(_language.SubtitleTranslated);
                }
                ShowSource();

                if (!onlySelectedLines)
                {
                    SubtitleListview1.ShowAlternateTextColumn(Configuration.Settings.Language.General.OriginalText);
                    SubtitleListview1.AutoSizeAllColumns(this);
                    SetupAlternateEdit();
                }
                SubtitleListview1.Fill(_subtitle, _subtitleAlternate);
                ResetHistory();
                RestoreSubtitleListviewIndexes();
                _converted = true;
                SetTitle();
                //if (googleTranslate.ScreenScrapingEncoding != null)
                //    SetEncoding(googleTranslate.ScreenScrapingEncoding);
                SetEncoding(Encoding.UTF8);
                if (!isAlternateVisible)
                {
                    toolStripMenuItemShowOriginalInPreview.Checked = false;
                    Configuration.Settings.General.ShowOriginalAsPreviewIfAvailable = false;
                    audioVisualizer.Invalidate();
                }
            }
            _formPositionsAndSizes.SavePositionAndSize(googleTranslate);
        }

        private static string GetTranslateStringFromNikseDk(string input)
        {
            WebRequest.DefaultWebProxy = Utilities.GetProxy();
            //            WebRequest request = WebRequest.Create("http://localhost:54942/MultiTranslator/TranslateForSubtitleEdit");
            WebRequest request = WebRequest.Create("http://www.nikse.dk/MultiTranslator/TranslateForSubtitleEdit");
            request.Method = "POST";
            string postData = String.Format("languagePair={1}&text={0}", Utilities.UrlEncode(input), "svda");
            byte[] byteArray = Encoding.UTF8.GetBytes(postData);
            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = byteArray.Length;
            Stream dataStream = request.GetRequestStream();
            dataStream.Write(byteArray, 0, byteArray.Length);
            dataStream.Close();
            WebResponse response = request.GetResponse();
            dataStream = response.GetResponseStream();
            StreamReader reader = new StreamReader(dataStream);
            string responseFromServer = reader.ReadToEnd();
            string result = responseFromServer;
            reader.Close();
            response.Close();
            return result;
        }

        private void TranslateFromSwedishToDanishToolStripMenuItemClick(object sender, EventArgs e)
        {
            if (!IsSubtitleLoaded)
            {
                MessageBox.Show(_language.NoSubtitleLoaded, Title, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            bool isSwedish = Utilities.AutoDetectGoogleLanguage(_subtitle) == "sv";
            string promptText = _language.TranslateSwedishToDanish;
            if (!isSwedish)
                promptText = _language.TranslateSwedishToDanishWarning;

            if (MessageBox.Show(promptText, Title, MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                try
                {
                    _subtitleAlternate = new Subtitle(_subtitle);
                    _subtitleAlternateFileName = null;
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
                    if (result.Length > 0)
                    {
                        int index = 0;
                        foreach (string s in result.Split(new string[] { "<p>", "</p>" }, StringSplitOptions.RemoveEmptyEntries))
                        {
                            if (index < _subtitle.Paragraphs.Count)
                                _subtitle.Paragraphs[index].Text = s;
                            index++;
                        }
                        ShowSource();
                        SubtitleListview1.ShowAlternateTextColumn(Configuration.Settings.Language.General.OriginalText);
                        SubtitleListview1.AutoSizeAllColumns(this);
                        SubtitleListview1.Fill(_subtitle, _subtitleAlternate);
                        ShowStatus(_language.TranslationFromSwedishToDanishComplete);
                        SubtitleListview1.SelectIndexAndEnsureVisible(firstSelectedIndex);
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

        private void UndoLastAction()
        {
            if (_subtitle != null && _subtitle.CanUndo && _undoIndex >= 0)
                UndoToIndex(true);
        }

        /// <summary>
        /// Undo or Redo
        /// </summary>
        /// <param name="undo">True equals undo, false triggers redo</param>
        private void UndoToIndex(bool undo)
        {
            if (_networkSession != null)
                return;

            lock (_syncUndo)
            {
                if (!undo && _undoIndex >= _subtitle.HistoryItems.Count - 1)
                    return;
                if (undo && !_subtitle.CanUndo && _undoIndex < 0)
                    return;

                // Add latest changes if any (also stop changes from being added while redoing/undoing)
                timerTextUndo.Stop();
                timerAlternateTextUndo.Stop();
                _listViewTextTicks = 0;
                _listViewAlternateTextTicks = 0;
                TimerTextUndoTick(null, null);
                TimerAlternateTextUndoTick(null, null);

                try
                {
                    int selectedIndex = FirstSelectedIndex;
                    string text = string.Empty;
                    if (undo)
                    {
                        _subtitle.HistoryItems[_undoIndex].RedoParagraphs = new List<Paragraph>();
                        _subtitle.HistoryItems[_undoIndex].RedoParagraphsAlternate = new List<Paragraph>();

                        foreach (Paragraph p in _subtitle.Paragraphs)
                            _subtitle.HistoryItems[_undoIndex].RedoParagraphs.Add(new Paragraph(p));
                        if (Configuration.Settings.General.AllowEditOfOriginalSubtitle && _subtitleAlternate != null)
                        {
                            foreach (Paragraph p in _subtitleAlternate.Paragraphs)
                                _subtitle.HistoryItems[_undoIndex].RedoParagraphsAlternate.Add(new Paragraph(p));
                        }
                        _subtitle.HistoryItems[_undoIndex].RedoFileName = _fileName;
                        _subtitle.HistoryItems[_undoIndex].RedoFileModified = _fileDateTime;
                        _subtitle.HistoryItems[_undoIndex].RedoOriginalFileName = _subtitleAlternateFileName;

                        if (selectedIndex >= 0)
                        {
                            _subtitle.HistoryItems[_undoIndex].RedoParagraphs[selectedIndex].Text =
                                textBoxListViewText.Text;
                            if (Configuration.Settings.General.AllowEditOfOriginalSubtitle && _subtitleAlternate != null &&
                                selectedIndex < _subtitle.HistoryItems[_undoIndex].RedoParagraphsAlternate.Count)
                                _subtitle.HistoryItems[_undoIndex].RedoParagraphsAlternate[selectedIndex].Text =
                                    textBoxListViewTextAlternate.Text;
                            _subtitle.HistoryItems[_undoIndex].RedoLineIndex = selectedIndex;
                            _subtitle.HistoryItems[_undoIndex].RedoLinePosition = textBoxListViewText.SelectionStart;
                            _subtitle.HistoryItems[_undoIndex].RedoLinePositionAlternate = textBoxListViewTextAlternate.SelectionStart;
                        }
                        else
                        {
                            _subtitle.HistoryItems[_undoIndex].RedoLineIndex = -1;
                            _subtitle.HistoryItems[_undoIndex].RedoLinePosition = -1;
                        }
                    }
                    else
                    {
                        _undoIndex++;
                    }
                    text = _subtitle.HistoryItems[_undoIndex].Description;

                    _subtitleListViewIndex = -1;
                    textBoxListViewText.Text = string.Empty;
                    textBoxListViewTextAlternate.Text = string.Empty;
                    string subtitleFormatFriendlyName;

                    string oldFileName = _fileName;
                    DateTime oldFileDateTime = _fileDateTime;

                    string oldAlternameFileName = _subtitleAlternateFileName;
                    _fileName = _subtitle.UndoHistory(_undoIndex, out subtitleFormatFriendlyName, out _fileDateTime, out _subtitleAlternate, out _subtitleAlternateFileName);
                    if (string.IsNullOrEmpty(oldAlternameFileName) && !string.IsNullOrEmpty(_subtitleAlternateFileName))
                    {
                        SubtitleListview1.ShowAlternateTextColumn(Configuration.Settings.Language.General.OriginalText);
                        SubtitleListview1.AutoSizeAllColumns(this);
                    }
                    else if (SubtitleListview1.IsAlternateTextColumnVisible && _subtitleAlternate != null && _subtitleAlternate.Paragraphs.Count == 0)
                    {
                        RemoveAlternate(true);
                    }

                    if (!undo)
                    {
                        if (_subtitle.HistoryItems[_undoIndex].RedoParagraphs != null)
                        //TODO: sometimes redo paragraphs can be null - how?
                        {
                            _subtitle.Paragraphs.Clear();
                            if (Configuration.Settings.General.AllowEditOfOriginalSubtitle && _subtitleAlternate != null)
                                _subtitleAlternate.Paragraphs.Clear();
                            foreach (Paragraph p in _subtitle.HistoryItems[_undoIndex].RedoParagraphs)
                                _subtitle.Paragraphs.Add(new Paragraph(p));
                            if (Configuration.Settings.General.AllowEditOfOriginalSubtitle && _subtitleAlternate != null)
                            {
                                foreach (Paragraph p in _subtitle.HistoryItems[_undoIndex].RedoParagraphsAlternate)
                                    _subtitleAlternate.Paragraphs.Add(new Paragraph(p));
                            }
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine("Undo failed at undo index: " + _undoIndex);
                        }
                        _subtitle.HistoryItems[_undoIndex].RedoParagraphs = null;
                        _subtitle.HistoryItems[_undoIndex].RedoParagraphsAlternate = null;
                        if (SubtitleListview1.IsAlternateTextColumnVisible && _subtitleAlternate != null && _subtitleAlternate.Paragraphs.Count == 0)
                        {
                            RemoveAlternate(true);
                        }
                    }

                    if (oldFileName.Equals(_fileName, StringComparison.OrdinalIgnoreCase))
                        _fileDateTime = oldFileDateTime; // undo will not give overwrite-newer-file warning

                    comboBoxSubtitleFormats.SelectedIndexChanged -= ComboBoxSubtitleFormatsSelectedIndexChanged;
                    SetCurrentFormat(subtitleFormatFriendlyName);
                    comboBoxSubtitleFormats.SelectedIndexChanged += ComboBoxSubtitleFormatsSelectedIndexChanged;

                    ShowSource();
                    SubtitleListview1.Fill(_subtitle, _subtitleAlternate);

                    if (selectedIndex >= _subtitle.Paragraphs.Count)
                        SubtitleListview1.SelectIndexAndEnsureVisible(_subtitle.Paragraphs.Count - 1, true);
                    else if (selectedIndex >= 0 && selectedIndex < _subtitle.Paragraphs.Count)
                        SubtitleListview1.SelectIndexAndEnsureVisible(selectedIndex, true);
                    else
                        SubtitleListview1.SelectIndexAndEnsureVisible(0, true);

                    audioVisualizer.Invalidate();
                    if (undo)
                    {
                        if (_subtitle.HistoryItems[_undoIndex].LineIndex == FirstSelectedIndex)
                        {
                            textBoxListViewText.SelectionStart = _subtitle.HistoryItems[_undoIndex].LinePosition;
                            if (_subtitleAlternate != null)
                                textBoxListViewTextAlternate.SelectionStart =
                                    _subtitle.HistoryItems[_undoIndex].LinePositionAlternate;
                        }
                        ShowStatus(_language.UndoPerformed + ": " + text.Replace(Environment.NewLine, "  "));
                        _undoIndex--;
                    }
                    else
                    {
                        if (_subtitle.HistoryItems[_undoIndex].RedoLineIndex >= 0 &&
                            _subtitle.HistoryItems[_undoIndex].RedoLineIndex == FirstSelectedIndex)
                            textBoxListViewText.SelectionStart = _subtitle.HistoryItems[_undoIndex].RedoLinePosition;
                        if (_subtitleAlternate != null && _subtitle.HistoryItems[_undoIndex].RedoLineIndex >= 0 &&
                            _subtitle.HistoryItems[_undoIndex].RedoLineIndex == FirstSelectedIndex)
                            textBoxListViewTextAlternate.SelectionStart =
                                _subtitle.HistoryItems[_undoIndex].RedoLinePositionAlternate;
                        if (_subtitle.HistoryItems[_undoIndex].RedoFileName.Equals(_fileName, StringComparison.OrdinalIgnoreCase))
                            _fileDateTime = _subtitle.HistoryItems[_undoIndex].RedoFileModified;
                        _fileName = _subtitle.HistoryItems[_undoIndex].RedoFileName;
                        _subtitleAlternateFileName = _subtitle.HistoryItems[_undoIndex].RedoFileName;
                        ShowStatus(_language.UndoPerformed);
                    }

                }
                catch (Exception exception)
                {
                    System.Diagnostics.Debug.WriteLine(exception.Message);
                }

                timerTextUndo.Start();
                timerAlternateTextUndo.Start();
                SetTitle();
            }
        }

        private void RedoLastAction()
        {
            if (_undoIndex < _subtitle.HistoryItems.Count - 1)
                UndoToIndex(false);
        }

        private void ShowHistoryforUndoToolStripMenuItemClick(object sender, EventArgs e)
        {
            if (_subtitle != null && _subtitle.CanUndo)
            {
                ReloadFromSourceView();
                var showHistory = new ShowHistory();
                showHistory.Initialize(_subtitle, _undoIndex);
                if (showHistory.ShowDialog(this) == DialogResult.OK)
                {
                    int selectedIndex = FirstSelectedIndex;
                    _subtitleListViewIndex = -1;
                    textBoxListViewText.Text = string.Empty;
                    textBoxListViewTextAlternate.Text = string.Empty;
                    MakeHistoryForUndo(_language.BeforeUndo);
                    string subtitleFormatFriendlyName;

                    string oldFileName = _fileName;
                    DateTime oldFileDateTime = _fileDateTime;

                    _fileName = _subtitle.UndoHistory(showHistory.SelectedIndex, out subtitleFormatFriendlyName, out _fileDateTime, out _subtitleAlternate, out _subtitleAlternateFileName);

                    if (oldFileName.Equals(_fileName, StringComparison.OrdinalIgnoreCase))
                        _fileDateTime = oldFileDateTime; // undo will not give overwrite-newer-file warning

                    SetTitle();
                    ShowStatus(_language.UndoPerformed);

                    comboBoxSubtitleFormats.SelectedIndexChanged -= ComboBoxSubtitleFormatsSelectedIndexChanged;
                    SetCurrentFormat(subtitleFormatFriendlyName);
                    comboBoxSubtitleFormats.SelectedIndexChanged += ComboBoxSubtitleFormatsSelectedIndexChanged;

                    ShowSource();
                    SubtitleListview1.Fill(_subtitle, _subtitleAlternate);

                    if (selectedIndex >= 0 && selectedIndex < _subtitle.Paragraphs.Count)
                        SubtitleListview1.SelectIndexAndEnsureVisible(selectedIndex, true);
                    else
                        SubtitleListview1.SelectIndexAndEnsureVisible(0, true);

                    audioVisualizer.Invalidate();
                }
            }
            else
            {
                MessageBox.Show(_language.NothingToUndo, Title, MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void ToolStripButtonSpellCheckClick(object sender, EventArgs e)
        {
            SpellCheck(true, 0);
        }

        private void SpellCheckToolStripMenuItemClick(object sender, EventArgs e)
        {
            SpellCheck(true, 0);
        }

        private void SpellCheckViaWord()
        {
            if (_subtitle == null | _subtitle.Paragraphs.Count == 0)
                return;

            WordSpellChecker wordSpellChecker = null;
            int totalLinesChanged = 0;
            try
            {
                wordSpellChecker = new WordSpellChecker(this, Utilities.AutoDetectGoogleLanguage(_subtitle));
                wordSpellChecker.NewDocument();
                Application.DoEvents();
            }
            catch
            {
                MessageBox.Show(_language.UnableToStartWord);
                return;
            }
            string version = wordSpellChecker.Version;

            int index = FirstSelectedIndex;
            if (index < 0)
                index = 0;

            _cancelWordSpellCheck = false;
            for (; index < _subtitle.Paragraphs.Count; index++)
            {
                Paragraph p = _subtitle.Paragraphs[index];
                int errorsBefore;
                int errorsAfter;
                ShowStatus(string.Format(_language.SpellChekingViaWordXLineYOfX, version, index + 1, _subtitle.Paragraphs.Count));
                SubtitleListview1.SelectIndexAndEnsureVisible(index);
                string newText = wordSpellChecker.CheckSpelling(p.Text, out errorsBefore, out errorsAfter);
                if (errorsAfter > 0)
                {
                    wordSpellChecker.CloseDocument();
                    wordSpellChecker.Quit();
                    ShowStatus(string.Format(_language.SpellCheckAbortedXCorrections, totalLinesChanged));
                    Cursor = Cursors.Default;
                    return;
                }
                else if (errorsBefore != errorsAfter)
                {
                    if (textBoxListViewText.Text != newText)
                    {
                        textBoxListViewText.Text = newText;
                        totalLinesChanged++;
                    }
                }

                Application.DoEvents();
                if (_cancelWordSpellCheck)
                    break;
            }
            wordSpellChecker.CloseDocument();
            wordSpellChecker.Quit();
            ShowStatus(string.Format(_language.SpellCheckCompletedXCorrections, totalLinesChanged));
            Cursor = Cursors.Default;
            _cancelWordSpellCheck = true;
        }

        private void SpellCheck(bool autoDetect, int startFromLine)
        {
            //if (Configuration.Settings.General.SpellChecker.Contains("word", StringComparison.OrdinalIgnoreCase))
            //{
            //    SpellCheckViaWord();
            //    return;
            //}

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
                        if (result == DialogResult.Cancel)
                            return;

                        if (result == DialogResult.No)
                        {
                            _spellCheckForm = new SpellCheck();
                            _spellCheckForm.DoSpellCheck(autoDetect, _subtitle, dictionaryFolder, this, startFromLine);
                        }
                        else
                        {
                            _spellCheckForm.ContinueSpellCheck(_subtitle);
                        }
                    }
                    else
                    {
                        _spellCheckForm = new SpellCheck();
                        _spellCheckForm.DoSpellCheck(autoDetect, _subtitle, dictionaryFolder, this, startFromLine);
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
                    int startIndex = p.Text.IndexOf(oldWord, StringComparison.Ordinal);
                    while (startIndex >= 0 && startIndex < p.Text.Length && p.Text.Substring(startIndex).Contains(oldWord))
                    {
                        bool startOk = startIndex == 0 || p.Text[startIndex - 1] == ' ' || startIndex == p.Text.Length - oldWord.Length ||
                                       Environment.NewLine.EndsWith(p.Text[startIndex - 1]);

                        if (startOk)
                        {
                            int end = startIndex + oldWord.Length;
                            if (end <= p.Text.Length)
                            {
                                if (end == p.Text.Length || (@" ,.!?:;')" + Environment.NewLine).Contains(p.Text[end]))
                                    p.Text = p.Text.Remove(startIndex, oldWord.Length).Insert(startIndex, changeWord);
                            }
                        }
                        if (startIndex + 2 >= p.Text.Length)
                            startIndex = -1;
                        else
                            startIndex = p.Text.IndexOf(oldWord, startIndex + 2, StringComparison.Ordinal);
                    }

                }
                ShowStatus(string.Format(_language.SpellCheckChangedXToY, oldWord, changeWord));
                SubtitleListview1.SetText(_subtitle.GetIndex(p), p.Text);
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
            new GetDictionaries().ShowDialog(this); // backup plan..
        }

        private void ContextMenuStripListviewOpening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            var format = GetCurrentSubtitleFormat();
            var formatType = format.GetType();
            toolStripMenuItemSetLanguage.Visible = false;
            if ((formatType == typeof(AdvancedSubStationAlpha) || formatType == typeof(SubStationAlpha)) && SubtitleListview1.SelectedItems.Count > 0)
            {
                toolStripMenuItemWebVTT.Visible = false;
                var styles = AdvancedSubStationAlpha.GetStylesFromHeader(_subtitle.Header);
                setStylesForSelectedLinesToolStripMenuItem.DropDownItems.Clear();
                foreach (string style in styles)
                {
                    setStylesForSelectedLinesToolStripMenuItem.DropDownItems.Add(style, null, tsi_Click);
                }
                setStylesForSelectedLinesToolStripMenuItem.Visible = styles.Count > 1;
                toolStripMenuItemAssStyles.Visible = true;
                if (formatType == typeof(AdvancedSubStationAlpha))
                {
                    toolStripMenuItemAssStyles.Text = _language.Menu.ContextMenu.AdvancedSubStationAlphaStyles;
                    setStylesForSelectedLinesToolStripMenuItem.Text = _language.Menu.ContextMenu.AdvancedSubStationAlphaSetStyle;
                }
                else
                {
                    toolStripMenuItemAssStyles.Text = _language.Menu.ContextMenu.SubStationAlphaStyles;
                    setStylesForSelectedLinesToolStripMenuItem.Text = _language.Menu.ContextMenu.SubStationAlphaSetStyle;
                }
            }
            else if ((formatType == typeof(TimedText10) || formatType == typeof(ItunesTimedText)) && SubtitleListview1.SelectedItems.Count > 0)
            {
                toolStripMenuItemWebVTT.Visible = false;
                toolStripMenuItemAssStyles.Text = _language.Menu.ContextMenu.TimedTextStyles;
                var styles = TimedText10.GetStylesFromHeader(_subtitle.Header);
                setStylesForSelectedLinesToolStripMenuItem.DropDownItems.Clear();
                foreach (string style in styles)
                {
                    setStylesForSelectedLinesToolStripMenuItem.DropDownItems.Add(style, null, tsi_Click);
                }
                setStylesForSelectedLinesToolStripMenuItem.Visible = styles.Count >= 1;
                toolStripMenuItemAssStyles.Visible = true;
                setStylesForSelectedLinesToolStripMenuItem.Text = _language.Menu.ContextMenu.TimedTextSetStyle;

                // languages
                var languages = TimedText10.GetUsedLanguages(_subtitle);
                toolStripMenuItemSetLanguage.DropDownItems.Clear();
                if (!string.IsNullOrEmpty(_language.Menu.ContextMenu.TimedTextSetLanguage)) // TODO: remove if in 3.4
                    toolStripMenuItemSetLanguage.Text = _language.Menu.ContextMenu.TimedTextSetLanguage;
                toolStripMenuItemSetLanguage.Visible = true;
                if (languages.Count > 0)
                {
                    foreach (string language in languages)
                    {
                        toolStripMenuItemSetLanguage.DropDownItems.Add(language, null, AddLanguageClick);
                    }
                    toolStripMenuItemSetLanguage.DropDownItems.Add("-");
                }

                toolStripMenuItemSetLanguage.DropDownItems.Add("New");
                var newItem = (ToolStripMenuItem)toolStripMenuItemSetLanguage.DropDownItems[toolStripMenuItemSetLanguage.DropDownItems.Count - 1];
                var moreLanguages = new List<string>();
                foreach (System.Globalization.CultureInfo x in System.Globalization.CultureInfo.GetCultures(System.Globalization.CultureTypes.NeutralCultures))
                {
                    if (!languages.Contains(x.TwoLetterISOLanguageName.ToLower()) && !languages.Contains(x.TwoLetterISOLanguageName.ToLower()))
                        moreLanguages.Add(x.TwoLetterISOLanguageName.ToLower());
                }
                moreLanguages.Sort();
                foreach (string language in moreLanguages)
                {
                    newItem.DropDownItems.Add(language, null, AddLanguageClick);
                }
            }
            else if ((formatType == typeof(Sami) || formatType == typeof(SamiModern)) && SubtitleListview1.SelectedItems.Count > 0)
            {
                toolStripMenuItemWebVTT.Visible = false;
                toolStripMenuItemAssStyles.Text = _language.Menu.ContextMenu.TimedTextStyles;
                var styles = Sami.GetStylesFromHeader(_subtitle.Header);
                setStylesForSelectedLinesToolStripMenuItem.DropDownItems.Clear();
                foreach (string style in styles)
                {
                    setStylesForSelectedLinesToolStripMenuItem.DropDownItems.Add(style, null, tsi_Click);
                }
                setStylesForSelectedLinesToolStripMenuItem.Visible = styles.Count > 1;
                toolStripMenuItemAssStyles.Visible = false;
                setStylesForSelectedLinesToolStripMenuItem.Text = _language.Menu.ContextMenu.SamiSetStyle;
            }
            else if ((formatType == typeof(WebVTT) && SubtitleListview1.SelectedItems.Count > 0))
            {
                setStylesForSelectedLinesToolStripMenuItem.Visible = false;
                toolStripMenuItemAssStyles.Visible = false;
                toolStripMenuItemWebVTT.Visible = true;
                var voices = WebVTT.GetVoices(_subtitle);
                toolStripMenuItemWebVTT.DropDownItems.Clear();
                foreach (string style in voices)
                {
                    toolStripMenuItemWebVTT.DropDownItems.Add(style, null, WebVTTSetVoice);
                }
                if (string.IsNullOrEmpty(_language.Menu.ContextMenu.WebVTTSetNewVoice)) //TODO: In 3.4 remove if
                    toolStripMenuItemWebVTT.DropDownItems.Add("Set new voice...", null, WebVTTSetNewVoice);
                else
                    toolStripMenuItemWebVTT.DropDownItems.Add(_language.Menu.ContextMenu.WebVTTSetNewVoice, null, WebVTTSetNewVoice);
                if (voices.Count > 0)
                {
                    if (string.IsNullOrEmpty(_language.Menu.ContextMenu.WebVTTRemoveVoices)) //TODO: In 3.4 remove if
                        toolStripMenuItemWebVTT.DropDownItems.Add("Remove voices", null, WebVTTRemoveVoices);
                    else
                        toolStripMenuItemWebVTT.DropDownItems.Add(_language.Menu.ContextMenu.WebVTTRemoveVoices, null, WebVTTRemoveVoices);
                }
            }
            else if ((format.Name == "Nuendo" && SubtitleListview1.SelectedItems.Count > 0))
            {
                toolStripMenuItemWebVTT.Visible = false;
                var styles = GetNuendoStyles();
                setStylesForSelectedLinesToolStripMenuItem.DropDownItems.Clear();
                foreach (string style in styles)
                {
                    setStylesForSelectedLinesToolStripMenuItem.DropDownItems.Add(style, null, NuendoSetStyle);
                }
                setStylesForSelectedLinesToolStripMenuItem.Visible = styles.Count > 1;
                toolStripMenuItemAssStyles.Visible = false;
                setStylesForSelectedLinesToolStripMenuItem.Text = "Set character"; //TODO: Set language _language.Menu.ContextMenu.TimedTextStyles;
            }
            else
            {
                setStylesForSelectedLinesToolStripMenuItem.Visible = false;
                toolStripMenuItemAssStyles.Visible = false;
                toolStripMenuItemWebVTT.Visible = false;
            }

            toolStripMenuItemGoogleMicrosoftTranslateSelLine.Visible = false;
            if (SubtitleListview1.SelectedItems.Count == 0)
            {
                contextMenuStripEmpty.Show(MousePosition.X, MousePosition.Y);
                e.Cancel = true;
            }
            else
            {
                bool noNetWorkSession = _networkSession == null;

                toolStripMenuItemSaveSelectedLines.Visible = false;
                toolStripMenuItemInsertBefore.Visible = true;
                toolStripMenuItemInsertAfter.Visible = true;
                toolStripMenuItemInsertSubtitle.Visible = noNetWorkSession;
                toolStripMenuItemMergeLines.Visible = true;
                mergeAfterToolStripMenuItem.Visible = true;
                mergeBeforeToolStripMenuItem.Visible = true;
                splitLineToolStripMenuItem.Visible = true;
                toolStripSeparator7.Visible = true;
                typeEffectToolStripMenuItem.Visible = noNetWorkSession;
                karokeeEffectToolStripMenuItem.Visible = noNetWorkSession;
                toolStripSeparatorAdvancedFunctions.Visible = noNetWorkSession;
                fixCommonErrorsInSelectedLinesToolStripMenuItem.Visible = true;
                adjustDisplayTimeForSelectedLinesToolStripMenuItem.Visible = true;
                showSelectedLinesEarlierlaterToolStripMenuItem.Visible = true;
                visualSyncSelectedLinesToolStripMenuItem.Visible = true;
                googleTranslateSelectedLinesToolStripMenuItem.Visible = true;
                toolStripMenuItemGoogleMicrosoftTranslateSelLine.Visible = false;
                toolStripMenuItemUnbreakLines.Visible = true;
                toolStripMenuItemAutoBreakLines.Visible = true;
                toolStripSeparatorBreakLines.Visible = true;
                toolStripMenuItemSurroundWithMusicSymbols.Visible = IsUnicode;

                if (SubtitleListview1.SelectedItems.Count == 1)
                {
                    toolStripMenuItemMergeLines.Visible = false;
                    visualSyncSelectedLinesToolStripMenuItem.Visible = false;
                    toolStripMenuItemUnbreakLines.Visible = false;
                    toolStripMenuItemAutoBreakLines.Visible = false;
                    toolStripSeparatorBreakLines.Visible = false;
                    if (_subtitleAlternate != null && noNetWorkSession)
                        toolStripMenuItemGoogleMicrosoftTranslateSelLine.Visible = true;
                    toolStripMenuItemMergeDialog.Visible = false;
                }
                else if (SubtitleListview1.SelectedItems.Count == 2)
                {
                    toolStripMenuItemInsertBefore.Visible = false;
                    toolStripMenuItemInsertAfter.Visible = false;
                    toolStripMenuItemInsertSubtitle.Visible = false;
                    mergeAfterToolStripMenuItem.Visible = false;
                    mergeBeforeToolStripMenuItem.Visible = false;
                    splitLineToolStripMenuItem.Visible = false;
                    typeEffectToolStripMenuItem.Visible = false;
                    toolStripMenuItemMergeDialog.Visible = true;
                }
                else if (SubtitleListview1.SelectedItems.Count >= 2)
                {
                    toolStripMenuItemSaveSelectedLines.Visible = true;
                    toolStripMenuItemInsertBefore.Visible = false;
                    toolStripMenuItemInsertAfter.Visible = false;
                    toolStripMenuItemInsertSubtitle.Visible = false;
                    splitLineToolStripMenuItem.Visible = false;
                    mergeAfterToolStripMenuItem.Visible = false;
                    mergeBeforeToolStripMenuItem.Visible = false;
                    typeEffectToolStripMenuItem.Visible = false;
                    toolStripSeparator7.Visible = false;

                    if (SubtitleListview1.SelectedItems.Count > 25)
                    {
                        toolStripMenuItemMergeLines.Visible = false;
                    }
                    else if (SubtitleListview1.SelectedItems.Count > 2)
                    { // only allow merge is text is not way too long
                        try
                        {
                            int totalLength = 0;
                            foreach (int index in SubtitleListview1.SelectedIndices)
                            {
                                totalLength += _subtitle.Paragraphs[index].Text.Length;
                            }
                            if (totalLength > Configuration.Settings.General.SubtitleLineMaximumLength * 2.5)
                            {
                                toolStripMenuItemMergeLines.Visible = false;
                            }
                        }
                        catch
                        {
                        }
                    }

                    toolStripMenuItemMergeDialog.Visible = false;
                }

                if (formatType != typeof(SubRip))
                {
                    karokeeEffectToolStripMenuItem.Visible = false;
                    toolStripSeparatorAdvancedFunctions.Visible = SubtitleListview1.SelectedItems.Count == 1 && noNetWorkSession;
                }
            }
            toolStripMenuItemPasteSpecial.Visible = Clipboard.ContainsText();
        }

        private void tsi_Click(object sender, EventArgs e)
        {
            string style = (sender as ToolStripItem).Text;
            if (!string.IsNullOrEmpty(style))
            {
                MakeHistoryForUndo("Set style: " + style);

                var format = GetCurrentSubtitleFormat();
                var formatType = format.GetType();
                if ((formatType == typeof(TimedText10) || formatType == typeof(ItunesTimedText)))
                {
                    foreach (int index in SubtitleListview1.SelectedIndices)
                    {
                        _subtitle.Paragraphs[index].Style = style;
                        _subtitle.Paragraphs[index].Extra = TimedText10.SetExtra(_subtitle.Paragraphs[index]);
                        SubtitleListview1.SetExtraText(index, _subtitle.Paragraphs[index].Extra, SubtitleListview1.ForeColor);
                    }
                }
                else
                {
                    foreach (int index in SubtitleListview1.SelectedIndices)
                    {
                        _subtitle.Paragraphs[index].Extra = style;
                        SubtitleListview1.SetExtraText(index, style, SubtitleListview1.ForeColor);
                    }
                }
            }
        }

        private void AddLanguageClick(object sender, EventArgs e)
        {
            string lang = (sender as ToolStripItem).Text;
            if (!string.IsNullOrEmpty(lang))
            {
                MakeHistoryForUndo("Set language: " + lang);
                foreach (int index in SubtitleListview1.SelectedIndices)
                {
                    _subtitle.Paragraphs[index].Language = lang;
                    _subtitle.Paragraphs[index].Extra = TimedText10.SetExtra(_subtitle.Paragraphs[index]);
                    SubtitleListview1.SetExtraText(index, _subtitle.Paragraphs[index].Extra, SubtitleListview1.ForeColor);
                }
            }
        }

        private void NuendoSetStyle(object sender, EventArgs e)
        {
            string style = (sender as ToolStripItem).Text;
            if (!string.IsNullOrEmpty(style))
            {
                int indexOfComment = style.IndexOf('[');
                if (indexOfComment > 0)
                    style = style.Substring(0, indexOfComment).Trim();
                MakeHistoryForUndo("Set style: " + style);
                foreach (int index in SubtitleListview1.SelectedIndices)
                {
                    _subtitle.Paragraphs[index].Extra = style;
                    _subtitle.Paragraphs[index].Actor = style;
                    SubtitleListview1.SetExtraText(index, style, SubtitleListview1.ForeColor);
                }
            }
        }

        private void WebVTTSetVoice(object sender, EventArgs e)
        {
            string voice = (sender as ToolStripItem).Text;
            if (!string.IsNullOrEmpty(voice))
            {
                MakeHistoryForUndo("Set voice: " + voice);
                foreach (int index in SubtitleListview1.SelectedIndices)
                {
                    _subtitle.Paragraphs[index].Text = WebVTT.RemoveTag("v", _subtitle.Paragraphs[index].Text);
                    _subtitle.Paragraphs[index].Text = string.Format("<v {0}>{1}", voice, _subtitle.Paragraphs[index].Text);
                    SubtitleListview1.SetText(index, _subtitle.Paragraphs[index].Text);
                }
                RefreshSelectedParagraph();
            }
        }

        private void WebVTTSetNewVoice(object sender, EventArgs e)
        {
            var form = new WebVttNewVoice();
            if (form.ShowDialog(this) == DialogResult.OK)
            {
                string voice = form.VoiceName;
                if (!string.IsNullOrEmpty(voice))
                {
                    foreach (int index in SubtitleListview1.SelectedIndices)
                    {
                        _subtitle.Paragraphs[index].Text = WebVTT.RemoveTag("v", _subtitle.Paragraphs[index].Text);
                        _subtitle.Paragraphs[index].Text = string.Format("<v {0}>{1}", voice, _subtitle.Paragraphs[index].Text);
                        SubtitleListview1.SetText(index, _subtitle.Paragraphs[index].Text);
                    }
                    RefreshSelectedParagraph();
                }
            }
        }

        private void WebVTTRemoveVoices(object sender, EventArgs e)
        {
            foreach (int index in SubtitleListview1.SelectedIndices)
            {
                _subtitle.Paragraphs[index].Text = WebVTT.RemoveTag("v", _subtitle.Paragraphs[index].Text);
                SubtitleListview1.SetText(index, _subtitle.Paragraphs[index].Text);
            }
            RefreshSelectedParagraph();
        }

        private void WebVTTSetVoiceTextBox(object sender, EventArgs e)
        {
            string voice = (sender as ToolStripItem).Text;
            if (!string.IsNullOrEmpty(voice))
            {
                TextBox tb = textBoxListViewText;
                if (textBoxListViewTextAlternate.Focused)
                    tb = textBoxListViewTextAlternate;

                if (tb.SelectionLength > 0)
                {
                    string s = tb.SelectedText;
                    s = WebVTT.RemoveTag("v", s);
                    if (tb.SelectedText == tb.Text)
                        s = string.Format("<v {0}>{1}", voice, s);
                    else
                        s = string.Format("<v {0}>{1}</v>", voice, s);
                    tb.SelectedText = s;
                }
            }
        }

        private void WebVTTSetNewVoiceTextBox(object sender, EventArgs e)
        {
            var form = new WebVttNewVoice();
            if (form.ShowDialog(this) == DialogResult.OK)
            {
                string voice = form.VoiceName;
                if (!string.IsNullOrEmpty(voice))
                {
                    TextBox tb = textBoxListViewText;
                    if (textBoxListViewTextAlternate.Focused)
                        tb = textBoxListViewTextAlternate;

                    if (tb.SelectionLength > 0)
                    {
                        string s = tb.SelectedText;
                        s = WebVTT.RemoveTag("v", s);
                        if (tb.SelectedText == tb.Text)
                            s = string.Format("<v {0}>{1}", voice, s);
                        else
                            s = string.Format("<v {0}>{1}</v>", voice, s);
                        tb.SelectedText = s;
                    }
                }
            }
        }

        private void BoldToolStripMenuItemClick(object sender, EventArgs e)
        {
            ListViewToggleTag("b");
        }

        private void ItalicToolStripMenuItemClick(object sender, EventArgs e)
        {
            ListViewToggleTag("i");
        }

        private void UnderlineToolStripMenuItemClick(object sender, EventArgs e)
        {
            ListViewToggleTag("u");
        }

        private void ListViewToggleTag(string tag)
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
                    if (_subtitleAlternate != null && Configuration.Settings.General.AllowEditOfOriginalSubtitle)
                    {
                        Paragraph original = Utilities.GetOriginalParagraph(i, _subtitle.Paragraphs[i], _subtitleAlternate.Paragraphs);
                        if (original != null)
                        {
                            if (original.Text.Contains("<" + tag + ">"))
                            {
                                original.Text = original.Text.Replace("<" + tag + ">", string.Empty);
                                original.Text = original.Text.Replace("</" + tag + ">", string.Empty);
                            }
                            else
                            {
                                int indexOfEndBracket = original.Text.IndexOf('}');
                                if (original.Text.StartsWith("{\\") && indexOfEndBracket > 1 && indexOfEndBracket < 6)
                                    original.Text = string.Format("{2}<{0}>{1}</{0}>", tag, original.Text.Remove(0, indexOfEndBracket + 1), original.Text.Substring(0, indexOfEndBracket + 1));
                                else
                                    original.Text = string.Format("<{0}>{1}</{0}>", tag, original.Text);
                            }
                            SubtitleListview1.SetAlternateText(i, original.Text);
                        }
                    }

                    if (_subtitle.Paragraphs[i].Text.Contains("<" + tag + ">"))
                    {
                        _subtitle.Paragraphs[i].Text = _subtitle.Paragraphs[i].Text.Replace("<" + tag + ">", string.Empty);
                        _subtitle.Paragraphs[i].Text = _subtitle.Paragraphs[i].Text.Replace("</" + tag + ">", string.Empty);
                    }
                    else
                    {
                        int indexOfEndBracket = _subtitle.Paragraphs[i].Text.IndexOf('}');
                        if (_subtitle.Paragraphs[i].Text.StartsWith("{\\") && indexOfEndBracket > 1 && indexOfEndBracket < 6)
                            _subtitle.Paragraphs[i].Text = string.Format("{2}<{0}>{1}</{0}>", tag, _subtitle.Paragraphs[i].Text.Remove(0, indexOfEndBracket + 1), _subtitle.Paragraphs[i].Text.Substring(0, indexOfEndBracket + 1));
                        else
                            _subtitle.Paragraphs[i].Text = string.Format("<{0}>{1}</{0}>", tag, _subtitle.Paragraphs[i].Text);
                    }
                    SubtitleListview1.SetText(i, _subtitle.Paragraphs[i].Text);
                }
                SubtitleListview1.EndUpdate();

                ShowStatus(string.Format(_language.TagXAdded, tag));
                ShowSource();
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

                if (Configuration.Settings.General.PromptDeleteLines && MessageBox.Show(askText, Title, MessageBoxButtons.YesNo) == DialogResult.No)
                {
                    _cutText = string.Empty;
                    return;
                }

                if (!string.IsNullOrEmpty(_cutText))
                {
                    Clipboard.SetText(_cutText);
                    _cutText = string.Empty;
                }

                MakeHistoryForUndo(historyText);
                _subtitleListViewIndex = -1;

                if (Configuration.Settings.General.AllowEditOfOriginalSubtitle && _subtitleAlternate != null && _subtitleAlternate.Paragraphs.Count > 0)
                {
                    var alternateIndexes = new List<int>();
                    foreach (ListViewItem item in SubtitleListview1.SelectedItems)
                    {
                        Paragraph p = _subtitle.GetParagraphOrDefault(item.Index);
                        if (p != null)
                        {
                            Paragraph original = Utilities.GetOriginalParagraph(item.Index, p, _subtitleAlternate.Paragraphs);
                            if (original != null)
                                alternateIndexes.Add(_subtitleAlternate.GetIndex(original));
                        }
                    }

                    alternateIndexes.Reverse();
                    foreach (int i in alternateIndexes)
                    {
                        if (i < _subtitleAlternate.Paragraphs.Count)
                            _subtitleAlternate.Paragraphs.RemoveAt(i);
                    }
                    _subtitleAlternate.Renumber(1);
                }

                var indexes = new List<int>();
                foreach (ListViewItem item in SubtitleListview1.SelectedItems)
                    indexes.Add(item.Index);
                int firstIndex = SubtitleListview1.SelectedItems[0].Index;

                if (_networkSession != null)
                {
                    _networkSession.TimerStop();
                    NetworkGetSendUpdates(indexes, 0, null);
                }
                else
                {
                    indexes.Reverse();
                    foreach (int i in indexes)
                    {
                        _subtitle.Paragraphs.RemoveAt(i);
                        if (_networkSession != null && _networkSession.LastSubtitle != null && i < _networkSession.LastSubtitle.Paragraphs.Count)
                            _networkSession.LastSubtitle.Paragraphs.RemoveAt(i);
                    }
                    _subtitle.Renumber(1);
                    SubtitleListview1.Fill(_subtitle, _subtitleAlternate);
                    if (SubtitleListview1.FirstVisibleIndex == 0)
                        SubtitleListview1.FirstVisibleIndex = -1;
                    if (SubtitleListview1.Items.Count > firstIndex)
                    {
                        SubtitleListview1.SelectIndexAndEnsureVisible(firstIndex, true);
                    }
                    else if (SubtitleListview1.Items.Count > 0)
                    {
                        SubtitleListview1.SelectIndexAndEnsureVisible(SubtitleListview1.Items.Count - 1, true);
                    }
                }

                ShowStatus(statusText);
                ShowSource();
            }
        }

        private void ToolStripMenuItemInsertBeforeClick(object sender, EventArgs e)
        {
            if (_subtitle.Paragraphs.Count > 0)
                InsertBefore();
            textBoxListViewText.Focus();
        }

        private void InsertBefore()
        {
            var format = GetCurrentSubtitleFormat();
            bool useExtraForStyle = format.HasStyleSupport;
            List<string> styles = new List<string>();
            if (format.GetType() == typeof(AdvancedSubStationAlpha) || format.GetType() == typeof(SubStationAlpha))
                styles = AdvancedSubStationAlpha.GetStylesFromHeader(_subtitle.Header);
            else if (format.GetType() == typeof(TimedText10) || format.GetType() == typeof(ItunesTimedText))
                styles = TimedText10.GetStylesFromHeader(_subtitle.Header);
            else if (format.GetType() == typeof(Sami) || format.GetType() == typeof(SamiModern))
                styles = Sami.GetStylesFromHeader(_subtitle.Header);
            string style = "Default";
            if (styles.Count > 0)
                style = styles[0];

            MakeHistoryForUndo(_language.BeforeInsertLine);

            int startNumber = 1;
            if (_subtitle.Paragraphs.Count > 0)
                startNumber = _subtitle.Paragraphs[0].Number;
            int firstSelectedIndex = 0;
            if (SubtitleListview1.SelectedItems.Count > 0)
                firstSelectedIndex = SubtitleListview1.SelectedItems[0].Index;

            int addMilliseconds = Configuration.Settings.General.MininumMillisecondsBetweenLines + 1;
            if (addMilliseconds < 1)
                addMilliseconds = 1;

            var newParagraph = new Paragraph();
            if (useExtraForStyle)
            {
                newParagraph.Extra = style;
                if (format.GetType() == typeof(TimedText10) || format.GetType() == typeof(ItunesTimedText))
                {
                    if (styles.Count > 0)
                        newParagraph.Style = style;
                    newParagraph.Extra = TimedText10.SetExtra(newParagraph);
                }
            }

            Paragraph prev = _subtitle.GetParagraphOrDefault(firstSelectedIndex - 1);
            Paragraph next = _subtitle.GetParagraphOrDefault(firstSelectedIndex);
            if (prev != null && next != null)
            {
                newParagraph.EndTime.TotalMilliseconds = next.StartTime.TotalMilliseconds - addMilliseconds;
                newParagraph.StartTime.TotalMilliseconds = newParagraph.EndTime.TotalMilliseconds - 2000;
                if (newParagraph.StartTime.TotalMilliseconds <= prev.EndTime.TotalMilliseconds)
                    newParagraph.StartTime.TotalMilliseconds = prev.EndTime.TotalMilliseconds + 1;
                if (newParagraph.Duration.TotalMilliseconds < 100)
                    newParagraph.EndTime.TotalMilliseconds += 100;
            }
            else if (prev != null)
            {
                newParagraph.StartTime.TotalMilliseconds = prev.EndTime.TotalMilliseconds + addMilliseconds;
                newParagraph.EndTime.TotalMilliseconds = newParagraph.StartTime.TotalMilliseconds + Configuration.Settings.General.NewEmptyDefaultMs;
                if (newParagraph.StartTime.TotalMilliseconds > newParagraph.EndTime.TotalMilliseconds)
                    newParagraph.StartTime.TotalMilliseconds = prev.EndTime.TotalMilliseconds + 1;
            }
            else if (next != null)
            {
                newParagraph.StartTime.TotalMilliseconds = next.StartTime.TotalMilliseconds - 2001;
                newParagraph.EndTime.TotalMilliseconds = next.StartTime.TotalMilliseconds - 1;
            }
            else
            {
                newParagraph.StartTime.TotalMilliseconds = 1000;
                newParagraph.EndTime.TotalMilliseconds = 3000;
            }
            if (GetCurrentSubtitleFormat().IsFrameBased)
            {
                newParagraph.CalculateFrameNumbersFromTimeCodes(CurrentFrameRate);
                newParagraph.CalculateTimeCodesFromFrameNumbers(CurrentFrameRate);
            }

            if (Configuration.Settings.General.AllowEditOfOriginalSubtitle && _subtitleAlternate != null && _subtitleAlternate.Paragraphs.Count > 0)
            {
                var currentOriginal = Utilities.GetOriginalParagraph(firstSelectedIndex, _subtitle.Paragraphs[firstSelectedIndex], _subtitleAlternate.Paragraphs);
                if (currentOriginal != null)
                {
                    _subtitleAlternate.Paragraphs.Insert(_subtitleAlternate.Paragraphs.IndexOf(currentOriginal), new Paragraph(newParagraph));
                }
                else
                {
                    _subtitleAlternate.InsertParagraphInCorrectTimeOrder(new Paragraph(newParagraph));
                }
                _subtitleAlternate.Renumber(1);
            }

            if (_networkSession != null)
            {
                _networkSession.TimerStop();
                NetworkGetSendUpdates(new List<int>(), firstSelectedIndex, newParagraph);
            }
            else
            {
                _subtitle.Paragraphs.Insert(firstSelectedIndex, newParagraph);
                _subtitleListViewIndex = -1;
                _subtitle.Renumber(1);
                SubtitleListview1.Fill(_subtitle, _subtitleAlternate);
            }
            SubtitleListview1.SelectIndexAndEnsureVisible(firstSelectedIndex);
            ShowSource();
            ShowStatus(_language.LineInserted);
        }

        private void ToolStripMenuItemInsertAfterClick(object sender, EventArgs e)
        {
            if (_subtitle.Paragraphs.Count > 0)
            {
                InsertAfter();
                textBoxListViewText.Focus();
            }
        }

        private void InsertAfter()
        {
            var format = GetCurrentSubtitleFormat();
            bool useExtraForStyle = format.HasStyleSupport;
            var styles = new List<string>();
            if (format.GetType() == typeof(AdvancedSubStationAlpha) || format.GetType() == typeof(SubStationAlpha))
                styles = AdvancedSubStationAlpha.GetStylesFromHeader(_subtitle.Header);
            else if (format.GetType() == typeof(TimedText10) || format.GetType() == typeof(ItunesTimedText))
                styles = TimedText10.GetStylesFromHeader(_subtitle.Header);
            else if (format.GetType() == typeof(Sami) || format.GetType() == typeof(SamiModern))
                styles = Sami.GetStylesFromHeader(_subtitle.Header);
            string style = "Default";
            if (styles.Count > 0)
                style = styles[0];

            MakeHistoryForUndo(_language.BeforeInsertLine);

            int firstSelectedIndex = 0;
            if (SubtitleListview1.SelectedItems.Count > 0)
                firstSelectedIndex = SubtitleListview1.SelectedItems[0].Index + 1;

            var newParagraph = new Paragraph();
            if (useExtraForStyle)
            {
                newParagraph.Extra = style;
                if (format.GetType() == typeof(TimedText10) || format.GetType() == typeof(ItunesTimedText))
                {
                    if (styles.Count > 0)
                        newParagraph.Style = style;
                    newParagraph.Extra = TimedText10.SetExtra(newParagraph);
                }
            }

            Paragraph prev = _subtitle.GetParagraphOrDefault(firstSelectedIndex - 1);
            Paragraph next = _subtitle.GetParagraphOrDefault(firstSelectedIndex);
            if (prev != null)
            {
                int addMilliseconds = Configuration.Settings.General.MininumMillisecondsBetweenLines;
                if (addMilliseconds < 1)
                    addMilliseconds = 1;

                newParagraph.StartTime.TotalMilliseconds = prev.EndTime.TotalMilliseconds + addMilliseconds;
                newParagraph.EndTime.TotalMilliseconds = newParagraph.StartTime.TotalMilliseconds + Configuration.Settings.General.NewEmptyDefaultMs;
                if (next != null && newParagraph.EndTime.TotalMilliseconds > next.StartTime.TotalMilliseconds)
                    newParagraph.EndTime.TotalMilliseconds = next.StartTime.TotalMilliseconds - 1;
                if (newParagraph.StartTime.TotalMilliseconds > newParagraph.EndTime.TotalMilliseconds)
                    newParagraph.StartTime.TotalMilliseconds = prev.EndTime.TotalMilliseconds + 1;
            }
            else if (next != null)
            {
                newParagraph.StartTime.TotalMilliseconds = next.StartTime.TotalMilliseconds - 2000;
                newParagraph.EndTime.TotalMilliseconds = next.StartTime.TotalMilliseconds - 1;
            }
            else
            {
                newParagraph.StartTime.TotalMilliseconds = 1000;
                newParagraph.EndTime.TotalMilliseconds = 3000;
            }
            if (GetCurrentSubtitleFormat().IsFrameBased)
            {
                newParagraph.CalculateFrameNumbersFromTimeCodes(CurrentFrameRate);
                newParagraph.CalculateTimeCodesFromFrameNumbers(CurrentFrameRate);
            }

            if (Configuration.Settings.General.AllowEditOfOriginalSubtitle && _subtitleAlternate != null && _subtitleAlternate.Paragraphs.Count > 0)
            {
                var currentOriginal = Utilities.GetOriginalParagraph(firstSelectedIndex - 1, _subtitle.Paragraphs[firstSelectedIndex - 1], _subtitleAlternate.Paragraphs);
                if (currentOriginal != null)
                {
                    _subtitleAlternate.Paragraphs.Insert(_subtitleAlternate.Paragraphs.IndexOf(currentOriginal) + 1, new Paragraph(newParagraph));
                }
                else
                {
                    _subtitleAlternate.InsertParagraphInCorrectTimeOrder(new Paragraph(newParagraph));
                }
                _subtitleAlternate.Renumber(1);
            }

            if (_networkSession != null)
            {
                _networkSession.TimerStop();
                NetworkGetSendUpdates(new List<int>(), firstSelectedIndex, newParagraph);
            }
            else
            {
                _subtitle.Paragraphs.Insert(firstSelectedIndex, newParagraph);
                _subtitle.Renumber(1);
                SubtitleListview1.Fill(_subtitle, _subtitleAlternate);
            }
            SubtitleListview1.SelectIndexAndEnsureVisible(firstSelectedIndex);
            ShowSource();
            ShowStatus(_language.LineInserted);
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

                    double duration = GetDurationInMilliseconds();
                    if (duration > 0 && duration < 100000 && duration != last.Duration.TotalMilliseconds)
                    {
                        last.EndTime.TotalMilliseconds = last.StartTime.TotalMilliseconds + duration;
                        SubtitleListview1.SetDuration(_subtitleListViewIndex, last);
                        showSource = true;
                    }

                    if (showSource)
                    {
                        ShowSource();
                    }
                }

                Paragraph p = _subtitle.GetParagraphOrDefault(firstSelectedIndex);
                if (p != null)
                {
                    InitializeListViewEditBox(p);
                    _subtitleListViewIndex = firstSelectedIndex;
                    _oldSelectedParagraph = new Paragraph(p);
                    UpdateListViewTextInfo(labelTextLineLengths, labelSingleLine, labelTextLineTotal, labelCharactersPerSecond, p, textBoxListViewText);

                    if (Configuration.Settings.General.AllowEditOfOriginalSubtitle && _subtitleAlternate != null && _subtitleAlternate.Paragraphs.Count > 0)
                    {
                        InitializeListViewEditBoxAlternate(p, firstSelectedIndex);
                        labelAlternateCharactersPerSecond.Left = textBoxListViewTextAlternate.Left + (textBoxListViewTextAlternate.Width - labelAlternateCharactersPerSecond.Width);
                        labelTextAlternateLineTotal.Left = textBoxListViewTextAlternate.Left + (textBoxListViewTextAlternate.Width - labelTextAlternateLineTotal.Width);
                    }
                }
            }
        }

        private void SubtitleListview1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!_makeHistoryPaused)
            {
                if (_listViewTextUndoLast != null && _listViewTextUndoIndex >= 0 && _subtitle.Paragraphs.Count > _listViewTextUndoIndex &&
                    _subtitle.Paragraphs[_listViewTextUndoIndex].Text.TrimEnd() != _listViewTextUndoLast.TrimEnd())
                {
                    MakeHistoryForUndo(Configuration.Settings.Language.General.Text + ": " + _listViewTextUndoLast.TrimEnd() + " -> " + _subtitle.Paragraphs[_listViewTextUndoIndex].Text.TrimEnd(), false);
                    _subtitle.HistoryItems[_subtitle.HistoryItems.Count - 1].Subtitle.Paragraphs[_listViewTextUndoIndex].Text = _listViewTextUndoLast;
                    if (Configuration.Settings.General.AllowEditOfOriginalSubtitle && _subtitleAlternate != null)
                    {
                        var original = Utilities.GetOriginalParagraph(_listViewTextUndoIndex, _subtitle.Paragraphs[_listViewTextUndoIndex], _subtitleAlternate.Paragraphs);
                        var idx = _subtitleAlternate.GetIndex(original);
                        if (idx >= 0)
                            _subtitle.HistoryItems[_subtitle.HistoryItems.Count - 1].OriginalSubtitle.Paragraphs[idx].Text = _listViewAlternateTextUndoLast;
                    }
                }
                else if (Configuration.Settings.General.AllowEditOfOriginalSubtitle && _subtitleAlternate != null && _listViewAlternateTextUndoLast != null &&
                        _subtitle.Paragraphs.Count > _listViewTextUndoIndex && _listViewTextUndoIndex >= 0)
                {
                    var original = Utilities.GetOriginalParagraph(_listViewTextUndoIndex, _subtitle.Paragraphs[_listViewTextUndoIndex], _subtitleAlternate.Paragraphs);
                    if (original != null && original.Text.TrimEnd() != _listViewAlternateTextUndoLast.TrimEnd())
                    {
                        var idx = _subtitleAlternate.GetIndex(original);
                        if (idx >= 0)
                        {
                            MakeHistoryForUndo(Configuration.Settings.Language.General.Text + ": " + _listViewAlternateTextUndoLast.TrimEnd() + " -> " + original.Text.TrimEnd(), false);
                            _subtitle.HistoryItems[_subtitle.HistoryItems.Count - 1].OriginalSubtitle.Paragraphs[idx].Text = _listViewAlternateTextUndoLast;
                        }
                    }
                }
            }

            _listViewTextUndoIndex = -1;
            SubtitleListView1SelectedIndexChange();
        }

        private void ShowLineInformationListView()
        {
            if (SubtitleListview1.SelectedItems.Count == 1)
                toolStripSelected.Text = string.Format("{0}/{1}", SubtitleListview1.SelectedItems[0].Index + 1, SubtitleListview1.Items.Count);
            else
                toolStripSelected.Text = string.Format(_language.XLinesSelected, SubtitleListview1.SelectedItems.Count);
        }

        private void UpdateListViewTextCharactersPerSeconds(Label charsPerSecond, Paragraph paragraph)
        {
            if (paragraph.Duration.TotalSeconds > 0)
            {
                double charactersPerSecond = Utilities.GetCharactersPerSecond(paragraph);
                if (charactersPerSecond > Configuration.Settings.General.SubtitleMaximumCharactersPerSeconds + 7)
                    charsPerSecond.ForeColor = Color.Red;
                else if (charactersPerSecond > Configuration.Settings.General.SubtitleMaximumCharactersPerSeconds)
                    charsPerSecond.ForeColor = Color.Orange;
                else
                    charsPerSecond.ForeColor = Color.Black;
                charsPerSecond.Text = string.Format(_language.CharactersPerSecond, charactersPerSecond);
            }
            else
            {
                charsPerSecond.ForeColor = Color.Red;
                charsPerSecond.Text = string.Format(_language.CharactersPerSecond, _languageGeneral.NotAvailable);
            }
        }

        private void UpdateListViewTextInfo(Label lineLengths, Label singleLine, Label lineTotal, Label charactersPerSecond, Paragraph paragraph, TextBox textBox)
        {
            if (paragraph == null)
                return;
            bool textBoxHasFocus = textBox.Focused;
            string text = paragraph.Text;
            lineLengths.Text = _languageGeneral.SingleLineLengths.Trim();
            singleLine.Left = lineLengths.Left + lineLengths.Width - 3;
            Utilities.GetLineLengths(singleLine, text);

            buttonSplitLine.Visible = false;
            string s = Utilities.RemoveHtmlTags(text, true).Replace(Environment.NewLine, string.Empty); // we don't count new line in total length... correct?

            // remove unicode control characters
            s = s.Replace(Convert.ToChar(8207).ToString(), string.Empty).
                Replace(Convert.ToChar(8206).ToString(), string.Empty).
                Replace(Convert.ToChar(0x202A).ToString(), string.Empty).
                Replace(Convert.ToChar(0x202B).ToString(), string.Empty).
                Replace(Convert.ToChar(0x202D).ToString(), string.Empty).
                Replace(Convert.ToChar(0x202E).ToString(), string.Empty).
                Replace(Convert.ToChar(0x202B).ToString(), string.Empty);

            int numberOfLines = 1 + (text.Trim().Length - text.Trim().Replace(Environment.NewLine, string.Empty).Length) / Environment.NewLine.Length;
            int maxLines = int.MaxValue;
            if (Configuration.Settings.Tools.ListViewSyntaxMoreThanXLines)
                maxLines = Configuration.Settings.Tools.ListViewSyntaxMoreThanXLinesX;
            if (numberOfLines <= maxLines)
            {
                if (s.Length <= Configuration.Settings.General.SubtitleLineMaximumLength * Math.Max(numberOfLines, 2))
                {
                    lineTotal.ForeColor = Color.Black;
                    if (!textBoxHasFocus)
                        lineTotal.Text = string.Format(_languageGeneral.TotalLengthX, s.Length);
                }
                else
                {
                    lineTotal.ForeColor = Color.Red;
                    if (!textBoxHasFocus)
                        lineTotal.Text = string.Format(_languageGeneral.TotalLengthXSplitLine, s.Length);
                    if (buttonUnBreak.Visible)
                    {
                        if (!textBoxHasFocus)
                            lineTotal.Text = string.Format(_languageGeneral.TotalLengthX, s.Length);
                        buttonSplitLine.Visible = true;
                    }
                }
            }
            UpdateListViewTextCharactersPerSeconds(charactersPerSecond, paragraph);
            charactersPerSecond.Left = textBox.Left + (textBox.Width - labelCharactersPerSecond.Width);
            lineTotal.Left = textBox.Left + (textBox.Width - lineTotal.Width);
            FixVerticalScrollBars(textBox);
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
                    SubtitleListview1.SelectIndexAndEnsureVisible(firstSelectedIndex);
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
                    SubtitleListview1.SelectIndexAndEnsureVisible(firstSelectedIndex);
            }
        }

        private static string RemoveSsaStyle(string text)
        {
            int indexOfBegin = text.IndexOf('{');
            while (indexOfBegin >= 0 && text.IndexOf('}') > indexOfBegin)
            {
                int indexOfEnd = text.IndexOf('}');
                text = text.Remove(indexOfBegin, (indexOfEnd - indexOfBegin) + 1);
                indexOfBegin = text.IndexOf('{');
            }
            return text;
        }

        private void NormalToolStripMenuItemClick(object sender, EventArgs e)
        {
            if (_subtitle.Paragraphs.Count > 0 && SubtitleListview1.SelectedItems.Count > 0)
            {
                MakeHistoryForUndo(_language.BeforeSettingFontToNormal);

                bool isSsa = GetCurrentSubtitleFormat().FriendlyName == new SubStationAlpha().FriendlyName ||
                             GetCurrentSubtitleFormat().FriendlyName == new AdvancedSubStationAlpha().FriendlyName;

                foreach (ListViewItem item in SubtitleListview1.SelectedItems)
                {
                    Paragraph p = _subtitle.GetParagraphOrDefault(item.Index);
                    if (p != null)
                    {
                        int indexOfEndBracket = p.Text.IndexOf('}');
                        if (p.Text.StartsWith("{\\") && indexOfEndBracket > 1 && indexOfEndBracket < 6)
                            p.Text = p.Text.Remove(0, indexOfEndBracket + 1);
                        p.Text = Utilities.RemoveHtmlTags(p.Text);
                        p.Text = p.Text.Replace("♪", string.Empty);
                        if (isSsa)
                            p.Text = RemoveSsaStyle(p.Text);
                        SubtitleListview1.SetText(item.Index, p.Text);

                        if (_subtitleAlternate != null && Configuration.Settings.General.AllowEditOfOriginalSubtitle)
                        {
                            Paragraph original = Utilities.GetOriginalParagraph(item.Index, p, _subtitleAlternate.Paragraphs);
                            if (original != null)
                            {
                                original.Text = Utilities.RemoveHtmlTags(original.Text);
                                original.Text = original.Text.Replace("♪", string.Empty);
                                if (isSsa)
                                    original.Text = RemoveSsaStyle(original.Text);
                                SubtitleListview1.SetAlternateText(item.Index, original.Text);
                            }
                        }
                    }
                }
                ShowSource();
                RefreshSelectedParagraph();
            }
        }

        private void ButtonAutoBreakClick(object sender, EventArgs e)
        {
            string language = Utilities.AutoDetectGoogleLanguage(_subtitle);
            string languageOriginal = string.Empty;
            if (_subtitleAlternate != null)
                Utilities.AutoDetectGoogleLanguage(_subtitleAlternate);

            if (SubtitleListview1.SelectedItems.Count > 1)
            {
                MakeHistoryForUndo(_language.BeforeRemoveLineBreaksInSelectedLines);
                SubtitleListview1.BeginUpdate();
                foreach (int index in SubtitleListview1.SelectedIndices)
                {
                    Paragraph p = _subtitle.GetParagraphOrDefault(index);
                    if (p != null)
                    {
                        p.Text = Utilities.AutoBreakLine(p.Text, language);
                        SubtitleListview1.SetText(index, p.Text);
                    }

                    if (_subtitleAlternate != null && SubtitleListview1.IsAlternateTextColumnVisible && Configuration.Settings.General.AllowEditOfOriginalSubtitle)
                    {
                        var original = Utilities.GetOriginalParagraph(index, p, _subtitleAlternate.Paragraphs);
                        if (original != null)
                        {
                            original.Text = Utilities.AutoBreakLine(p.Text, languageOriginal);
                            SubtitleListview1.SetAlternateText(index, original.Text);
                        }
                    }
                    SubtitleListview1.SyntaxColorLine(_subtitle.Paragraphs, index, p);
                }
                SubtitleListview1.EndUpdate();
                RefreshSelectedParagraph();
            }
            else
            {
                textBoxListViewText.Text = Utilities.AutoBreakLine(textBoxListViewText.Text, language);
                if (_subtitleAlternate != null && Configuration.Settings.General.AllowEditOfOriginalSubtitle)
                    textBoxListViewTextAlternate.Text = Utilities.AutoBreakLine(textBoxListViewTextAlternate.Text, languageOriginal);
            }
        }

        private static void FixVerticalScrollBars(TextBox tb)
        {
            var lineCount = Utilities.CountTagInText(tb.Text, Environment.NewLine) + 1;
            if (lineCount > 3)
                tb.ScrollBars = ScrollBars.Vertical;
            else
                tb.ScrollBars = ScrollBars.None;
        }

        private void TextBoxListViewTextTextChanged(object sender, EventArgs e)
        {
            if (_subtitleListViewIndex >= 0)
            {
                int numberOfNewLines = textBoxListViewText.Text.Length - textBoxListViewText.Text.Replace(Environment.NewLine, " ").Length;
                Utilities.CheckAutoWrap(textBoxListViewText, new KeyEventArgs(Keys.None), numberOfNewLines);

                // update _subtitle + listview
                string text = textBoxListViewText.Text.TrimEnd();
                _subtitle.Paragraphs[_subtitleListViewIndex].Text = text;
                UpdateListViewTextInfo(labelTextLineLengths, labelSingleLine, labelTextLineTotal, labelCharactersPerSecond, _subtitle.Paragraphs[_subtitleListViewIndex], textBoxListViewText);
                SubtitleListview1.SetText(_subtitleListViewIndex, text);

                _listViewTextUndoIndex = _subtitleListViewIndex;
                labelStatus.Text = string.Empty;

                StartUpdateListSyntaxColoring();
                FixVerticalScrollBars(textBoxListViewText);
            }
        }

        private void TextBoxListViewTextAlternateTextChanged(object sender, EventArgs e)
        {
            if (_subtitleListViewIndex >= 0)
            {
                Paragraph p = _subtitle.GetParagraphOrDefault(_subtitleListViewIndex);
                if (p == null)
                    return;

                Paragraph original = Utilities.GetOriginalParagraph(_subtitleListViewIndex, p, _subtitleAlternate.Paragraphs);
                if (original != null)
                {
                    int numberOfNewLines = textBoxListViewTextAlternate.Text.Length - textBoxListViewTextAlternate.Text.Replace(Environment.NewLine, " ").Length;
                    Utilities.CheckAutoWrap(textBoxListViewTextAlternate, new KeyEventArgs(Keys.None), numberOfNewLines);

                    // update _subtitle + listview
                    string text = textBoxListViewTextAlternate.Text.TrimEnd();
                    original.Text = text;
                    UpdateListViewTextInfo(labelTextAlternateLineLengths, labelAlternateSingleLine, labelTextAlternateLineTotal, labelAlternateCharactersPerSecond, original, textBoxListViewTextAlternate);
                    SubtitleListview1.SetAlternateText(_subtitleListViewIndex, text);
                    _listViewTextUndoIndex = _subtitleListViewIndex;
                }
                labelStatus.Text = string.Empty;

                StartUpdateListSyntaxColoring();
                FixVerticalScrollBars(textBoxListViewTextAlternate);
            }

        }

        private void TextBoxListViewTextKeyDown(object sender, KeyEventArgs e)
        {
            _listViewTextTicks = DateTime.Now.Ticks;
            if (e.Modifiers == Keys.Shift && e.KeyCode == Keys.ShiftKey)
                return;
            if (e.Modifiers == Keys.Control && e.KeyCode == (Keys.LButton | Keys.ShiftKey))
                return;

            int numberOfNewLines = textBoxListViewText.Text.Length - textBoxListViewText.Text.Replace(Environment.NewLine, " ").Length;

            //Utilities.CheckAutoWrap(textBoxListViewText, e, numberOfNewLines);

            if (e.KeyCode == Keys.Enter && e.Modifiers == Keys.None && numberOfNewLines > 1)
            {
                e.SuppressKeyPress = true;
            }
            else if (e.Modifiers == Keys.Control && e.KeyCode == Keys.A)
            {
                textBoxListViewText.SelectAll();
                e.SuppressKeyPress = true;
            }
            else if (e.KeyData == _mainTextBoxAutoBreak)
            {
                ButtonAutoBreakClick(null, null);
                e.SuppressKeyPress = true;
            }
            else if (e.KeyData == _mainTextBoxUnbreak)
            {
                ButtonUnBreakClick(null, null);
                e.SuppressKeyPress = true;
            }
            else if (e.Modifiers == Keys.Alt && e.KeyCode == Keys.I)
            {
                if (textBoxListViewText.SelectionLength == 0)
                {
                    if (textBoxListViewText.Text.Contains("<i>"))
                    {
                        textBoxListViewText.Text = HtmlUtils.RemoveOpenCloseTags(textBoxListViewText.Text, HtmlUtils.TagItalic);
                    }
                    else
                    {
                        textBoxListViewText.Text = string.Format("<i>{0}</i>", textBoxListViewText.Text);
                    }
                }
                else
                {
                    TextBoxListViewToggleTag("i");
                }
            }
            if (e.Modifiers == Keys.Control && e.KeyCode == Keys.D)
            {
                textBoxListViewText.SelectionLength = 0;
                e.SuppressKeyPress = true;
            }
            else if (_mainTextBoxSplitAtCursor == e.KeyData)
            {
                ToolStripMenuItemSplitTextAtCursorClick(null, null);
                e.SuppressKeyPress = true;
            }
            else if (e.KeyData == _mainTextBoxInsertAfter)
            {
                InsertAfter();
                e.SuppressKeyPress = true;
            }
            else if (e.KeyData == _mainListViewGoToNextError)
            {
                GoToNextSynaxError();
                e.SuppressKeyPress = true;
            }
            else if (_mainTextBoxSelectionToLower == e.KeyData) // selection to lowercase
            {
                if (textBoxListViewText.SelectionLength > 0)
                {
                    int start = textBoxListViewText.SelectionStart;
                    int length = textBoxListViewText.SelectionLength;
                    textBoxListViewText.SelectedText = textBoxListViewText.SelectedText.ToLower();
                    textBoxListViewText.SelectionStart = start;
                    textBoxListViewText.SelectionLength = length;
                    e.SuppressKeyPress = true;
                }
            }
            else if (_mainTextBoxSelectionToUpper == e.KeyData) // selection to uppercase
            {
                if (textBoxListViewText.SelectionLength > 0)
                {
                    int start = textBoxListViewText.SelectionStart;
                    int length = textBoxListViewText.SelectionLength;
                    textBoxListViewText.SelectedText = textBoxListViewText.SelectedText.ToUpper();
                    textBoxListViewText.SelectionStart = start;
                    textBoxListViewText.SelectionLength = length;
                    e.SuppressKeyPress = true;
                }
            }

            // last key down in text
            _lastTextKeyDownTicks = DateTime.Now.Ticks;

            UpdatePositionAndTotalLength(labelTextLineTotal, textBoxListViewText);
        }

        private void MoveFirstWordInNextUp()
        {
            int firstIndex = FirstSelectedIndex;
            if (firstIndex >= 0)
            {
                var p = _subtitle.GetParagraphOrDefault(firstIndex);
                var next = _subtitle.GetParagraphOrDefault(firstIndex + 1);
                if (p != null && next != null)
                {
                    string s = next.Text.Trim();
                    // Find the first space.
                    int idx = s.IndexOf(' ');
                    // If the first space is after a "-", even if there is "{\an8}<i>" before, find the second space.
                    if (idx > 0 && s.Substring(idx - 1, 2) == "- ")
                        idx = idx + 1 + s.Substring(idx + 1).IndexOf(' ');

                    if (idx > 0 || s.Length > 0)
                    // A first word was found or next subtitle is not empty (has one word).
                    {
                        // Undo
                        MakeHistoryForUndo(_language.BeforeLineUpdatedInListView);

                        // Define firstWord. If idx > 0, there is a first word.
                        // If not, firstWord is the whole text of the next subtitle.
                        string firstWord = (idx > 0 ? s.Substring(0, idx).Trim() : next.Text);

                        // If firstWord contains a line break, it has two words.
                        if (firstWord.Contains(Environment.NewLine))
                        {
                            // Redefine firstWord and idx.
                            firstWord = firstWord.Remove(firstWord.IndexOf(Environment.NewLine, StringComparison.Ordinal));
                            idx = s.IndexOf(Environment.NewLine, StringComparison.Ordinal);
                        }

                        // Remove first word from the next subtitle.
                        // If there is only one word, 'next' will be empty.
                        next.Text = (idx > 0 ? s.Substring(idx + 1).Trim() : string.Empty);

                        // If the first subtitle ends with a tag (</i>):
                        String endTag = "";
                        if (p.Text.EndsWith('>') && p.Text.Contains('<'))
                        {
                            // Save the end tag.
                            endTag = p.Text.Substring(p.Text.LastIndexOf('<'), p.Text.Length - p.Text.LastIndexOf('<'));
                            // Remove the endTag from first subtitle.
                            p.Text = p.Text.Remove(p.Text.LastIndexOf('<'));
                        }

                        // If the first subtitle ends with "...":
                        Boolean firstSubtitleEndsWithEllipsis = p.Text.EndsWith("...");
                        if (firstSubtitleEndsWithEllipsis)
                        {
                            // Remove "..." from first subtitle.
                            p.Text = p.Text.TrimEnd('.');
                        }

                        // If the second subtitle (next) starts with a position tag, like {\an8}:
                        String positionTag = "";
                        if (firstWord.StartsWith('{') && firstWord.Contains('}'))
                        {
                            // Save the start tag.
                            positionTag = firstWord.Substring(firstWord.IndexOf('{'), firstWord.IndexOf('}') + 1);
                            // Remove the position tag from the first word.
                            firstWord = firstWord.Remove(0, firstWord.IndexOf('}') + 1);
                        }

                        // If the second subtitle (next) starts with a tag:
                        String startTag = "";
                        if (firstWord.StartsWith('<') && firstWord.Contains('>'))
                        {
                            // Save the start tag.
                            startTag = firstWord.Substring(firstWord.IndexOf('<'), firstWord.IndexOf('>') + 1);
                            // Remove the start tag from the first word.
                            firstWord = firstWord.Remove(0, firstWord.IndexOf('>') + 1);
                        }

                        // If the second subtitle ends with a tag and there's only one word in it:
                        if (next.Text.EndsWith('>') && next.Text.Contains('<') && next.Text.IndexOf(' ') < 0)
                        {
                            // Remove the end tag.
                            next.Text = next.Text.Remove(next.Text.LastIndexOf('<'));
                        }

                        // If the second subtitle (next) starts with a dialog ("-"):
                        String dialogMarker = "";
                        if (firstWord.StartsWith('-'))
                        {
                            // Save the dialog marker ("-" or "- ").
                            dialogMarker = (firstWord.StartsWith("- ") ? "- " : "-");
                            // Remove the dialog marker from the first word.
                            firstWord = firstWord.Remove(0, dialogMarker.Length);
                        }

                        // If the second subtitle starts with "...":
                        Boolean nextSubtitleStartsWithEllipsis = firstWord.StartsWith("...");
                        if (nextSubtitleStartsWithEllipsis)
                        {
                            // Remove "..." from the beginning of first word.
                            firstWord = firstWord.TrimStart('.');
                        }

                        // Add positionTag + startTag + dialogMarker + "..." + text to 'next'.
                        if (idx > 0)
                            next.Text = positionTag + startTag + dialogMarker + (nextSubtitleStartsWithEllipsis ? "..." : "") + next.Text.Trim();

                        // Add text + firstWord + "..." + endTag to First line.
                        p.Text = (idx == 0 ? startTag : "") + p.Text.Trim() + " " + firstWord.Trim() + (idx > 0 && firstSubtitleEndsWithEllipsis ? "..." : "") + endTag;

                        // Now, idx will hold the position of the last line break, if any.
                        idx = p.Text.LastIndexOf(Environment.NewLine, StringComparison.Ordinal);

                        // Check if the last line of the subtitle (the one that now contains the moved word) is longer than SubtitleLineMaximumLength.
                        if (Utilities.RemoveHtmlTags(p.Text.Substring((idx > 0 ? idx + Environment.NewLine.Length : 0))).Length > Configuration.Settings.General.SubtitleLineMaximumLength)
                            p.Text = Utilities.AutoBreakLine(p.Text);
                    }

                    SubtitleListview1.SetText(firstIndex, p.Text);
                    SubtitleListview1.SetText(firstIndex + 1, next.Text);
                    textBoxListViewText.Text = p.Text;
                }
            }
        }

        private void MoveLastWordDown()
        {
            int firstIndex = FirstSelectedIndex;
            if (firstIndex >= 0)
            {
                var p = _subtitle.GetParagraphOrDefault(firstIndex);
                var next = _subtitle.GetParagraphOrDefault(firstIndex + 1);
                if (p != null && next != null)
                {
                    string s = p.Text.Trim();
                    int idx = s.LastIndexOf(' ');
                    if (idx > 0 || s.Length > 0)
                    // A last word was found or the first subtitle is not empty (has one word).
                    {
                        // Undo
                        MakeHistoryForUndo(_language.BeforeLineUpdatedInListView);

                        // Define lastWord. If idx > 0, there is a last word.
                        // If not, lastWord is the whole text of the first subtitle.
                        string lastWord = (idx > 0 ? s.Substring(idx).Trim() : p.Text);

                        // If lastWord contains a line break, it has two words.
                        if (lastWord.Contains(Environment.NewLine))
                        {
                            // Redefine lastWord and idx.
                            lastWord = lastWord.Substring(lastWord.LastIndexOf(Environment.NewLine, StringComparison.Ordinal));
                            idx = s.LastIndexOf(Environment.NewLine, StringComparison.Ordinal);
                        }

                        // Remove last word from the first subtitle.
                        p.Text = (idx > 0 ? s.Substring(0, idx).Trim() : string.Empty);

                        // If the first subtitle ends with a tag (</i>):
                        String endTag = "";
                        if (lastWord.EndsWith('>') && lastWord.Contains('<'))
                        {
                            // Save the end tag.
                            endTag = lastWord.Substring(lastWord.LastIndexOf('<'), lastWord.Length - lastWord.LastIndexOf('<'));
                            // Remove the end tag from the last word.
                            lastWord = lastWord.Remove(lastWord.LastIndexOf('<'));
                        }

                        // If the first subtitle ends with "...":
                        Boolean firstSubtitleEndsWithEllipsis = lastWord.EndsWith("...");
                        if (firstSubtitleEndsWithEllipsis)
                        {
                            // Remove "..." from the last word.
                            lastWord = lastWord.TrimEnd('.');
                        }

                        // If the second subtitle (next) starts with a position tag, like {\an8}:
                        String positionTag = "";
                        if (next.Text.StartsWith('{') && next.Text.Contains('}'))
                        {
                            // Save the start tag.
                            positionTag = next.Text.Substring(next.Text.IndexOf('{'), next.Text.IndexOf('}') + 1);
                            // Remove the position tag from next subtitle.
                            next.Text = next.Text.Remove(0, next.Text.IndexOf('}') + 1);
                        }

                        // If the second subtitle (next) starts with a tag:
                        String startTag = "";
                        if (next.Text.StartsWith('<') && next.Text.Contains('>'))
                        {
                            // Save the start tag.
                            startTag = next.Text.Substring(next.Text.IndexOf('<'), next.Text.IndexOf('>') + 1);
                            // Remove the start tag from next subtitle.
                            next.Text = next.Text.Remove(0, next.Text.IndexOf('>') + 1);
                        }

                        // If the second subtitle (next) starts with a dialog ("-"):
                        String dialogMarker = "";
                        if (next.Text.StartsWith('-'))
                        {
                            // Save the dialog marker ("-" or "- ").
                            dialogMarker = (next.Text.StartsWith("- ") ? "- " : "-");
                            // Remove the dialog marker from the next subtitle.
                            next.Text = next.Text.Remove(0, dialogMarker.Length);
                        }

                        // If the second subtitle starts with "...":
                        Boolean nextSubtitleStartsWithEllipsis = next.Text.StartsWith("...");
                        if (nextSubtitleStartsWithEllipsis)
                        {
                            // Remove "..." from the beginning of 'next'.
                            next.Text = next.Text.TrimStart('.');
                        }

                        // Add text + "..." + endTag to first subtitle.
                        if (idx > 0)
                            p.Text = p.Text + (firstSubtitleEndsWithEllipsis ? "..." : "") + endTag;

                        // Add positionTag + startTag + dialogMarker + "..." + lastWord to 'next'.
                        next.Text = (idx > 0 ? positionTag : "") + (idx > 0 ? startTag : "") + dialogMarker + (nextSubtitleStartsWithEllipsis && idx > 0 ? "..." : "") + lastWord.Trim() + " " + next.Text.Trim();

                        // Now, idx will hold the position of the first line break, if any.
                        idx = next.Text.IndexOf(Environment.NewLine, StringComparison.Ordinal);

                        // Check if the first line of the next subtitle (the one that now contains the moved word) is longer than SubtitleLineMaximumLength.
                        if (Utilities.RemoveHtmlTags(next.Text.Substring(0, (idx > 0 ? idx : next.Text.Length))).Length > Configuration.Settings.General.SubtitleLineMaximumLength)
                            next.Text = Utilities.AutoBreakLine(next.Text);
                    }

                    SubtitleListview1.SetText(firstIndex, p.Text);
                    SubtitleListview1.SetText(firstIndex + 1, next.Text);
                    textBoxListViewText.Text = p.Text;
                }
            }
        }

        private void MakeAutoDurationSelectedLines()
        {
            if (_subtitle.Paragraphs.Count == 0)
                return;

            if (SubtitleListview1.SelectedItems.Count == 1)
            {
                MakeAutoDuration();
                return;
            }

            if (SubtitleListview1.SelectedItems.Count > 1)
            {
                MakeHistoryForUndo(_language.BeforeAutoDuration);
                foreach (int index in SubtitleListview1.SelectedIndices)
                {
                    Paragraph p = _subtitle.GetParagraphOrDefault(index);
                    if (p == null)
                        return;

                    double duration = Utilities.GetOptimalDisplayMilliseconds(textBoxListViewText.Text);
                    Paragraph next = _subtitle.GetParagraphOrDefault(index + 1);
                    if (next != null && p.StartTime.TotalMilliseconds + duration + Configuration.Settings.General.MininumMillisecondsBetweenLines > next.StartTime.TotalMilliseconds)
                    {
                        duration = next.StartTime.TotalMilliseconds - p.StartTime.TotalMilliseconds - Configuration.Settings.General.MininumMillisecondsBetweenLines;
                    }
                    if (duration > 500)
                    {
                        p.EndTime.TotalMilliseconds = p.StartTime.TotalMilliseconds + duration;
                    }
                }
                SaveSubtitleListviewIndexes();
                SubtitleListview1.Fill(_subtitle, _subtitleAlternate);
                RestoreSubtitleListviewIndexes();
                RefreshSelectedParagraph();
            }
        }

        private void MakeAutoDuration()
        {
            int i = _subtitleListViewIndex;
            Paragraph p = _subtitle.GetParagraphOrDefault(i);
            if (p == null)
                return;

            double duration = Utilities.GetOptimalDisplayMilliseconds(textBoxListViewText.Text);
            Paragraph next = _subtitle.GetParagraphOrDefault(i + 1);
            if (next != null && p.StartTime.TotalMilliseconds + duration + Configuration.Settings.General.MininumMillisecondsBetweenLines > next.StartTime.TotalMilliseconds)
            {
                duration = next.StartTime.TotalMilliseconds - p.StartTime.TotalMilliseconds - Configuration.Settings.General.MininumMillisecondsBetweenLines;
                if (duration < 400)
                    return;
            }
            SetDurationInSeconds(duration / 1000.0);

            p.EndTime.TotalMilliseconds = p.StartTime.TotalMilliseconds + duration;
            SubtitleListview1.SetDuration(i, p);
        }

        private void SplitLineToolStripMenuItemClick(object sender, EventArgs e)
        {
            SplitSelectedParagraph(null, null);
        }

        private void SplitSelectedParagraph(double? splitSeconds, int? textIndex)
        {
            string language = Utilities.AutoDetectGoogleLanguage(_subtitle);

            int? alternateTextIndex = null;
            if (textBoxListViewTextAlternate.Focused)
            {
                alternateTextIndex = textIndex;
                textIndex = null;
            }

            if (_subtitle.Paragraphs.Count > 0 && SubtitleListview1.SelectedItems.Count > 0)
            {
                SubtitleListview1.SelectedIndexChanged -= SubtitleListview1_SelectedIndexChanged;
                MakeHistoryForUndo(_language.BeforeSplitLine);

                int firstSelectedIndex = SubtitleListview1.SelectedItems[0].Index;

                Paragraph currentParagraph = _subtitle.GetParagraphOrDefault(firstSelectedIndex);
                var newParagraph = new Paragraph(currentParagraph);

                currentParagraph.Text = currentParagraph.Text.Replace("< /i>", "</i>");
                currentParagraph.Text = currentParagraph.Text.Replace("< i>", "<i>");
                string oldText = currentParagraph.Text;
                string[] lines = currentParagraph.Text.Split(Utilities.NewLineChars, StringSplitOptions.RemoveEmptyEntries);
                if (textIndex != null && textIndex.Value > 2 && textIndex.Value < oldText.Length - 2)
                {
                    string a = oldText.Substring(0, textIndex.Value).Trim();
                    string b = oldText.Substring(textIndex.Value).Trim();
                    if (oldText.TrimStart().StartsWith("<i>") && oldText.TrimEnd().EndsWith("</i>") &&
                        Utilities.CountTagInText(oldText, "<i>") == 1 && Utilities.CountTagInText(oldText, "</i>") == 1)
                    {
                        a = a + "</i>";
                        b = "<i>" + b;
                    }
                    else if (oldText.TrimStart().StartsWith("<b>") && oldText.TrimEnd().EndsWith("</b>") &&
                        Utilities.CountTagInText(oldText, "<b>") == 1 && Utilities.CountTagInText(oldText, "</b>") == 1)
                    {
                        a = a + "</b>";
                        b = "<b>" + b;
                    }
                    else if (a.StartsWith('-') && (a.EndsWith('.') || a.EndsWith('!') || a.EndsWith('?')) &&
                        b.StartsWith('-') && (b.EndsWith('.') || b.EndsWith('!') || b.EndsWith('?')))
                    {
                        a = a.TrimStart('-').TrimStart();
                        b = b.TrimStart('-').TrimStart();
                    }
                    else if (a.StartsWith("<i>-") && (a.EndsWith(".</i>") || a.EndsWith("!</i>") || a.EndsWith("?</i>")) &&
                        b.StartsWith("<i>-") && (b.EndsWith(".</i>") || b.EndsWith("!</i>") || b.EndsWith("?</i>")))
                    {
                        a = a.Remove(3, 1).Replace("  ", " ");
                        b = b.Remove(3, 1).Replace("  ", " ");
                    }
                    else if (a.StartsWith('-') && (a.EndsWith('.') || a.EndsWith('!') || a.EndsWith('?')) &&
                        b.StartsWith("<i>-") && (b.EndsWith(".</i>") || b.EndsWith("!</i>") || b.EndsWith("?</i>")))
                    {
                        a = a.TrimStart('-').TrimStart();
                        b = b.Remove(3, 1).Replace("  ", " ").Trim();
                    }
                    else if (a.StartsWith("<i>-") && (a.EndsWith(".</i>") || a.EndsWith("!</i>") || a.EndsWith("?</i>")) &&
                        b.StartsWith('-') && (b.EndsWith('.') || b.EndsWith('!') || b.EndsWith('?')))
                    {
                        a = a.Remove(3, 1).Replace("  ", " ").Trim();
                        b = b.TrimStart('-').TrimStart();
                    }

                    currentParagraph.Text = Utilities.AutoBreakLine(a, language);
                    newParagraph.Text = Utilities.AutoBreakLine(b, language);
                }
                else
                {
                    if (lines.Length == 2 && (lines[0].EndsWith('.') || lines[0].EndsWith('!') || lines[0].EndsWith('?')))
                    {
                        currentParagraph.Text = Utilities.AutoBreakLine(lines[0], language);
                        newParagraph.Text = Utilities.AutoBreakLine(lines[1], language);
                        if (lines[0].Length > 2 && lines[0][0] == '-' && lines[0][1] != '-' &&
                            lines[1].Length > 2 && lines[1][0] == '-' && lines[1][1] != '-')
                        {
                            currentParagraph.Text = currentParagraph.Text.TrimStart('-').Trim();
                            newParagraph.Text = newParagraph.Text.TrimStart('-').Trim();
                        }
                        if (currentParagraph.Text.StartsWith("<i>") && !currentParagraph.Text.Contains("</i>") &&
                           newParagraph.Text.EndsWith("</i>") && !newParagraph.Text.Contains("<i>"))
                        {
                            currentParagraph.Text = currentParagraph.Text + "</i>";
                            newParagraph.Text = "<i>" + newParagraph.Text;
                        }
                        if (currentParagraph.Text.StartsWith("<b>") && !currentParagraph.Text.Contains("</b>") &&
                           newParagraph.Text.EndsWith("</b>") && !newParagraph.Text.Contains("<b>"))
                        {
                            currentParagraph.Text = currentParagraph.Text + "</b>";
                            newParagraph.Text = "<b>" + newParagraph.Text;
                        }
                        if (currentParagraph.Text.StartsWith("<i>-") && (currentParagraph.Text.EndsWith(".</i>") || currentParagraph.Text.EndsWith("!</i>")) &&
                            newParagraph.Text.StartsWith("<i>-") && (newParagraph.Text.EndsWith(".</i>") || newParagraph.Text.EndsWith("!</i>")))
                        {
                            currentParagraph.Text = currentParagraph.Text.Remove(3, 1);
                            newParagraph.Text = newParagraph.Text.Remove(3, 1);
                        }
                        else if (lines[0].StartsWith('-') && (lines[0].EndsWith('.') || lines[0].EndsWith('!') || lines[0].EndsWith('?')) &&
                                                                      lines[1].StartsWith("<i>-") && (lines[1].EndsWith(".</i>") || lines[1].EndsWith("!</i>") || lines[1].EndsWith("?</i>")))
                        {
                            currentParagraph.Text = lines[0].TrimStart('-').TrimStart();
                            newParagraph.Text = lines[1].Remove(3, 1).Replace("  ", " ").Trim();
                        }
                    }
                    else if (lines.Length == 2 && (lines[0].EndsWith(".</i>") || lines[0].EndsWith("!</i>") || lines[0].EndsWith("?</i>")))
                    {
                        currentParagraph.Text = Utilities.AutoBreakLine(lines[0], language);
                        newParagraph.Text = Utilities.AutoBreakLine(lines[1], language);
                        if (lines[0].Length > 5 && lines[0].StartsWith("<i>-") && lines[0][4] != '-' &&
                            lines[1].Length > 5 && lines[1].StartsWith("<i>-") && lines[1][4] != '-')
                        {
                            currentParagraph.Text = currentParagraph.Text.Remove(3, 1);
                            if (currentParagraph.Text[3] == ' ')
                                currentParagraph.Text = currentParagraph.Text.Remove(3, 1);

                            newParagraph.Text = newParagraph.Text.Remove(3, 1);
                            if (newParagraph.Text[3] == ' ')
                                newParagraph.Text = newParagraph.Text.Remove(3, 1);
                        }
                        else if (lines[0].StartsWith('-') && (lines[0].EndsWith('.') || lines[0].EndsWith('!') || lines[0].EndsWith('?')) &&
                                                lines[1].StartsWith("<i>-") && (lines[1].EndsWith(".</i>") || lines[1].EndsWith("!</i>") || lines[1].EndsWith("?</i>")))
                        {
                            currentParagraph.Text = lines[0].TrimStart('-').TrimStart();
                            newParagraph.Text = lines[1].Remove(3, 1).Replace("  ", " ").Trim();
                        }
                        else if (lines[0].StartsWith("<i>-") && (lines[0].EndsWith(".</i>") || lines[0].EndsWith("!</i>") || lines[0].EndsWith("?</i>")) &&
                            lines[1].StartsWith('-') && (lines[1].EndsWith('.') || lines[1].EndsWith('!') || lines[1].EndsWith('?')))
                        {
                            currentParagraph.Text = lines[0].Remove(3, 1).Replace("  ", " ").Trim();
                            newParagraph.Text = lines[1].TrimStart('-').TrimStart();
                        }

                    }
                    else
                    {
                        string s = currentParagraph.Text;
                        var arr = Utilities.RemoveHtmlTags(s, true).Replace(Environment.NewLine, "\n").Split('\n');
                        if (arr.Length != 2 || arr[0].Length > Configuration.Settings.General.SubtitleLineMaximumLength || arr[1].Length > Configuration.Settings.General.SubtitleLineMaximumLength)
                            s = Utilities.AutoBreakLine(currentParagraph.Text, 5, Configuration.Settings.Tools.MergeLinesShorterThan, language);

                        lines = s.Split(Utilities.NewLineChars, StringSplitOptions.RemoveEmptyEntries);
                        if (lines.Length == 1)
                        {
                            s = Utilities.AutoBreakLine(currentParagraph.Text, 3, 20, language);
                            lines = s.Split(Utilities.NewLineChars, StringSplitOptions.RemoveEmptyEntries);
                        }
                        if (lines.Length == 1)
                        {
                            s = Utilities.AutoBreakLine(currentParagraph.Text, 3, 18, language);
                            lines = s.Split(Utilities.NewLineChars, StringSplitOptions.RemoveEmptyEntries);
                        }
                        if (lines.Length == 1)
                        {
                            s = Utilities.AutoBreakLine(currentParagraph.Text, 3, 15, language);
                            lines = s.Split(Utilities.NewLineChars, StringSplitOptions.RemoveEmptyEntries);
                        }

                        if (lines.Length == 2)
                        {
                            if (Utilities.CountTagInText(s, "<i>") == 1 && lines[0].StartsWith("<i>") && lines[1].EndsWith("</i>"))
                            {
                                lines[0] += "</i>";
                                lines[1] = "<i>" + lines[1];
                            }
                            currentParagraph.Text = Utilities.AutoBreakLine(lines[0], language);
                            newParagraph.Text = Utilities.AutoBreakLine(lines[1], language);
                        }
                        else if (lines.Length == 1)
                        {
                            currentParagraph.Text = Utilities.AutoBreakLine(lines[0], language);
                            newParagraph.Text = string.Empty;
                        }

                        if (currentParagraph.Text.StartsWith("<i>") && !currentParagraph.Text.Contains("</i>") &&
                         newParagraph.Text.EndsWith("</i>") && !newParagraph.Text.Contains("<i>"))
                        {
                            currentParagraph.Text = currentParagraph.Text + "</i>";
                            newParagraph.Text = "<i>" + newParagraph.Text;
                        }
                        if (currentParagraph.Text.StartsWith("<b>") && !currentParagraph.Text.Contains("</b>") &&
                           newParagraph.Text.EndsWith("</b>") && !newParagraph.Text.Contains("<b>"))
                        {
                            currentParagraph.Text = currentParagraph.Text + "</b>";
                            newParagraph.Text = "<b>" + newParagraph.Text;
                        }
                        if (currentParagraph.Text.StartsWith("<i>-") && (currentParagraph.Text.EndsWith(".</i>") || currentParagraph.Text.EndsWith("!</i>")) &&
                            newParagraph.Text.StartsWith("<i>-") && (newParagraph.Text.EndsWith(".</i>") || newParagraph.Text.EndsWith("!</i>")))
                        {
                            currentParagraph.Text = currentParagraph.Text.Remove(3, 1);
                            newParagraph.Text = newParagraph.Text.Remove(3, 1);
                        }
                    }
                }
                if (currentParagraph.Text.StartsWith("<i> "))
                    currentParagraph.Text = currentParagraph.Text.Remove(3, 1);
                if (newParagraph.Text.StartsWith("<i> "))
                    newParagraph.Text = newParagraph.Text.Remove(3, 1);

                double middle = currentParagraph.StartTime.TotalMilliseconds + (currentParagraph.Duration.TotalMilliseconds / 2);
                if (!string.IsNullOrWhiteSpace(Utilities.RemoveHtmlTags(oldText)))
                {
                    var startFactor = (double)Utilities.RemoveHtmlTags(currentParagraph.Text).Length / Utilities.RemoveHtmlTags(oldText).Length;
                    if (startFactor < 0.25)
                        startFactor = 0.25;
                    if (startFactor > 0.75)
                        startFactor = 0.75;
                    middle = currentParagraph.StartTime.TotalMilliseconds + (currentParagraph.Duration.TotalMilliseconds * startFactor);
                }

                if (splitSeconds.HasValue && splitSeconds.Value > (currentParagraph.StartTime.TotalSeconds + 0.2) && splitSeconds.Value < (currentParagraph.EndTime.TotalSeconds - 0.2))
                    middle = splitSeconds.Value * 1000.0;
                newParagraph.EndTime.TotalMilliseconds = currentParagraph.EndTime.TotalMilliseconds;
                currentParagraph.EndTime.TotalMilliseconds = middle;
                newParagraph.StartTime.TotalMilliseconds = currentParagraph.EndTime.TotalMilliseconds + 1;
                if (Configuration.Settings.General.MininumMillisecondsBetweenLines > 0)
                {
                    Paragraph next = _subtitle.GetParagraphOrDefault(firstSelectedIndex + 1);
                    if (next == null || next.StartTime.TotalMilliseconds > newParagraph.EndTime.TotalMilliseconds + Configuration.Settings.General.MininumMillisecondsBetweenLines + Configuration.Settings.General.MininumMillisecondsBetweenLines)
                    {
                        newParagraph.StartTime.TotalMilliseconds += Configuration.Settings.General.MininumMillisecondsBetweenLines;
                        newParagraph.EndTime.TotalMilliseconds += Configuration.Settings.General.MininumMillisecondsBetweenLines;
                    }
                    else
                    {
                        newParagraph.StartTime.TotalMilliseconds += Configuration.Settings.General.MininumMillisecondsBetweenLines;
                    }
                }

                if (Configuration.Settings.General.AllowEditOfOriginalSubtitle && _subtitleAlternate != null && _subtitleAlternate.Paragraphs.Count > 0)
                {
                    Paragraph originalCurrent = Utilities.GetOriginalParagraph(firstSelectedIndex, currentParagraph, _subtitleAlternate.Paragraphs);
                    if (originalCurrent != null)
                    {
                        string languageOriginal = Utilities.AutoDetectGoogleLanguage(_subtitleAlternate);

                        originalCurrent.EndTime.TotalMilliseconds = currentParagraph.EndTime.TotalMilliseconds;
                        Paragraph originalNew = new Paragraph(newParagraph);

                        lines = originalCurrent.Text.Split(Utilities.NewLineChars, StringSplitOptions.RemoveEmptyEntries);

                        oldText = originalCurrent.Text;
                        if (alternateTextIndex != null && alternateTextIndex.Value > 2 && alternateTextIndex.Value < oldText.Length - 2)
                        {
                            originalCurrent.Text = Utilities.AutoBreakLine(oldText.Substring(0, alternateTextIndex.Value).Trim(), language);
                            originalNew.Text = Utilities.AutoBreakLine(oldText.Substring(alternateTextIndex.Value).Trim(), language);
                            if (originalCurrent.Text.StartsWith("<i>") && !originalCurrent.Text.Contains("</i>") &&
                                originalNew.Text.EndsWith("</i>") && !originalNew.Text.Contains("<i>"))
                            {
                                originalCurrent.Text = originalCurrent.Text + "</i>";
                                originalNew.Text = "<i>" + originalNew.Text;
                            }
                            if (originalCurrent.Text.StartsWith("<b>") && !originalCurrent.Text.Contains("</b>") &&
                                originalNew.Text.EndsWith("</b>") && !originalNew.Text.Contains("<b>"))
                            {
                                originalCurrent.Text = originalCurrent.Text + "</b>";
                                originalNew.Text = "<b>" + originalNew.Text;
                            }
                            if (originalCurrent.Text.StartsWith('-') && (originalCurrent.Text.EndsWith('.') || originalCurrent.Text.EndsWith('!')) &&
                                originalNew.Text.StartsWith('-') && (originalNew.Text.EndsWith('.') || originalNew.Text.EndsWith('!')))
                            {
                                originalCurrent.Text = originalCurrent.Text.Remove(0, 1).Trim();
                                originalNew.Text = originalNew.Text.Remove(0, 1).Trim();
                            }
                            if (originalCurrent.Text.StartsWith("<i>-") && (originalCurrent.Text.EndsWith(".</i>") || originalCurrent.Text.EndsWith("!</i>")) &&
                                originalNew.Text.StartsWith("<i>-") && (originalNew.Text.EndsWith(".</i>") || originalNew.Text.EndsWith("!</i>")))
                            {
                                originalCurrent.Text = originalCurrent.Text.Remove(3, 1);
                                originalNew.Text = originalNew.Text.Remove(3, 1);
                            }
                            if (originalCurrent.Text.StartsWith("<b>-") && (originalCurrent.Text.EndsWith(".</b>") || originalCurrent.Text.EndsWith("!</b>")) &&
                                originalNew.Text.StartsWith("<b>-") && (originalNew.Text.EndsWith(".</b>") || originalNew.Text.EndsWith("!</b>")))
                            {
                                originalCurrent.Text = originalCurrent.Text.Remove(3, 1);
                                originalNew.Text = originalNew.Text.Remove(3, 1);
                            }
                            lines = new string[0];
                        }
                        else if (lines.Length == 2 && (lines[0].EndsWith('.') || lines[0].EndsWith('!') || lines[0].EndsWith('?')))
                        {
                            string a = lines[0].Trim();
                            string b = lines[1].Trim();
                            if (oldText.TrimStart().StartsWith("<i>") && oldText.TrimEnd().EndsWith("</i>") &&
                                Utilities.CountTagInText(oldText, "<i>") == 1 && Utilities.CountTagInText(oldText, "</i>") == 1)
                            {
                                a = a + "</i>";
                                b = "<i>" + b;
                            }
                            if (oldText.TrimStart().StartsWith("<b>") && oldText.TrimEnd().EndsWith("</b>") &&
                                Utilities.CountTagInText(oldText, "<b>") == 1 && Utilities.CountTagInText(oldText, "</b>") == 1)
                            {
                                a = a + "</b>";
                                b = "<b>" + b;
                            }
                            if (a.StartsWith('-') && (a.EndsWith('.') || a.EndsWith('!') || a.EndsWith('?')) &&
                                b.StartsWith('-') && (b.EndsWith('.') || b.EndsWith('!') || b.EndsWith('?')))
                            {
                                a = a.TrimStart('-').TrimStart();
                                b = b.TrimStart('-').TrimStart();
                            }
                            if (a.StartsWith("<i>-") && (a.EndsWith(".</i>") || a.EndsWith("!</i>") || a.EndsWith("?</i>")) &&
                                b.StartsWith("<i>-") && (b.EndsWith(".</i>") || b.EndsWith("!</i>") || b.EndsWith("?</i>")))
                            {
                                a = a.Remove(3, 1).Replace("  ", " ");
                                b = b.Remove(3, 1).Replace("  ", " ");
                            }

                            lines[0] = a;
                            lines[1] = b;
                            originalCurrent.Text = Utilities.AutoBreakLine(a);
                            originalNew.Text = Utilities.AutoBreakLine(b);
                        }
                        else
                        {
                            string s = Utilities.AutoBreakLine(originalCurrent.Text, 5, Configuration.Settings.Tools.MergeLinesShorterThan, languageOriginal);
                            lines = s.Split(Utilities.NewLineChars, StringSplitOptions.RemoveEmptyEntries);
                        }

                        if (lines.Length == 1)
                        {
                            string s = Utilities.AutoBreakLine(lines[0], 3, 20, languageOriginal);
                            lines = s.Split(Utilities.NewLineChars, StringSplitOptions.RemoveEmptyEntries);
                        }
                        if (lines.Length == 1)
                        {
                            string s = Utilities.AutoBreakLine(lines[0], 3, 18, languageOriginal);
                            lines = s.Split(Utilities.NewLineChars, StringSplitOptions.RemoveEmptyEntries);
                        }
                        if (lines.Length == 1)
                        {
                            string s = Utilities.AutoBreakLine(lines[0], 3, 15, languageOriginal);
                            lines = s.Split(Utilities.NewLineChars, StringSplitOptions.RemoveEmptyEntries);
                        }
                        if (lines.Length == 2)
                        {
                            string a = lines[0].Trim();
                            string b = lines[1].Trim();
                            if (oldText.TrimStart().StartsWith("<i>") && oldText.TrimEnd().EndsWith("</i>") &&
                                Utilities.CountTagInText(oldText, "<i>") == 1 && Utilities.CountTagInText(oldText, "</i>") == 1)
                            {
                                a = a + "</i>";
                                b = "<i>" + b;
                            }
                            if (oldText.TrimStart().StartsWith("<b>") && oldText.TrimEnd().EndsWith("</b>") &&
                                Utilities.CountTagInText(oldText, "<b>") == 1 && Utilities.CountTagInText(oldText, "</b>") == 1)
                            {
                                a = a + "</b>";
                                b = "<b>" + b;
                            }
                            if (a.StartsWith('-') && (a.EndsWith('.') || a.EndsWith('!') || a.EndsWith('?')) &&
                                b.StartsWith('-') && (b.EndsWith('.') || b.EndsWith('!') || b.EndsWith('?')))
                            {
                                a = a.TrimStart('-').TrimStart();
                                b = b.TrimStart('-').TrimStart();
                            }
                            if (a.StartsWith("<i>-") && (a.EndsWith(".</i>") || a.EndsWith("!</i>") || a.EndsWith("?</i>")) &&
                                b.StartsWith("<i>-") && (b.EndsWith(".</i>") || b.EndsWith("!</i>") || b.EndsWith("?</i>")))
                            {
                                a = a.Remove(3, 1).Replace("  ", " ");
                                b = b.Remove(3, 1).Replace("  ", " ");
                            }

                            lines[0] = a;
                            lines[1] = b;

                            originalCurrent.Text = Utilities.AutoBreakLine(lines[0]);
                            originalNew.Text = Utilities.AutoBreakLine(lines[1]);
                        }
                        else if (lines.Length == 1)
                        {
                            originalNew.Text = string.Empty;
                        }
                        if (originalCurrent != null && originalNew != null)
                        {
                            if (originalCurrent.Text.StartsWith("<i> "))
                                originalCurrent.Text = originalCurrent.Text.Remove(3, 1);
                            if (originalNew.Text.StartsWith("<i> "))
                                originalCurrent.Text = originalCurrent.Text.Remove(3, 1);
                        }
                        _subtitleAlternate.InsertParagraphInCorrectTimeOrder(originalNew);
                        _subtitleAlternate.Renumber(1);
                    }
                }

                if (_networkSession != null)
                {
                    _networkSession.TimerStop();
                    SetDurationInSeconds(currentParagraph.Duration.TotalSeconds);
                    _networkSession.UpdateLine(_subtitle.GetIndex(currentParagraph), currentParagraph);
                    NetworkGetSendUpdates(new List<int>(), firstSelectedIndex + 1, newParagraph);
                }
                else
                {
                    if (GetCurrentSubtitleFormat().IsFrameBased)
                    {
                        if (currentParagraph != null)
                        {
                            currentParagraph.CalculateFrameNumbersFromTimeCodes(CurrentFrameRate);
                            currentParagraph.CalculateTimeCodesFromFrameNumbers(CurrentFrameRate);
                        }
                        newParagraph.CalculateFrameNumbersFromTimeCodes(CurrentFrameRate);
                        newParagraph.CalculateTimeCodesFromFrameNumbers(CurrentFrameRate);
                    }
                    _subtitle.Paragraphs.Insert(firstSelectedIndex + 1, newParagraph);
                    _subtitle.Renumber(1);
                    SubtitleListview1.Fill(_subtitle, _subtitleAlternate);
                }
                SubtitleListview1.SelectIndexAndEnsureVisible(firstSelectedIndex);
                ShowSource();
                ShowStatus(_language.LineSplitted);
                SubtitleListview1.SelectedIndexChanged += SubtitleListview1_SelectedIndexChanged;
                RefreshSelectedParagraph();
                SubtitleListview1.SelectIndexAndEnsureVisible(firstSelectedIndex, true);
            }
        }

        private void MergeBeforeToolStripMenuItemClick(object sender, EventArgs e)
        {
            string language = Utilities.AutoDetectGoogleLanguage(_subtitle);
            if (_subtitle.Paragraphs.Count > 0 && SubtitleListview1.SelectedItems.Count > 0)
            {
                int firstSelectedIndex = SubtitleListview1.SelectedItems[0].Index;

                Paragraph prevParagraph = _subtitle.GetParagraphOrDefault(firstSelectedIndex - 1);
                Paragraph currentParagraph = _subtitle.GetParagraphOrDefault(firstSelectedIndex);

                if (prevParagraph != null && currentParagraph != null)
                {
                    SubtitleListview1.SelectedIndexChanged -= SubtitleListview1_SelectedIndexChanged;
                    MakeHistoryForUndo(_language.BeforeMergeLines);

                    if (_subtitleAlternate != null)
                    {
                        Paragraph prevOriginal = Utilities.GetOriginalParagraph(firstSelectedIndex, prevParagraph, _subtitleAlternate.Paragraphs);
                        Paragraph currentOriginal = Utilities.GetOriginalParagraph(firstSelectedIndex + 1, currentParagraph, _subtitleAlternate.Paragraphs);

                        if (currentOriginal != null)
                        {
                            if (prevOriginal == null)
                            {
                                currentOriginal.StartTime = prevParagraph.StartTime;
                                currentOriginal.EndTime = currentParagraph.EndTime;
                            }
                            else
                            {
                                prevOriginal.Text = prevOriginal.Text.Replace(Environment.NewLine, " ");
                                prevOriginal.Text += Environment.NewLine + currentOriginal.Text.Replace(Environment.NewLine, " ");
                                prevOriginal.Text = ChangeAllLinesItalictoSingleItalic(prevOriginal.Text);
                                prevOriginal.Text = Utilities.AutoBreakLine(prevOriginal.Text);
                                prevOriginal.EndTime = currentOriginal.EndTime;
                                _subtitleAlternate.Paragraphs.Remove(currentOriginal);
                            }
                            _subtitleAlternate.Renumber(1);
                        }
                    }

                    prevParagraph.Text = prevParagraph.Text.Replace(Environment.NewLine, " ");
                    prevParagraph.Text += Environment.NewLine + currentParagraph.Text.Replace(Environment.NewLine, " ");
                    prevParagraph.Text = Utilities.AutoBreakLine(prevParagraph.Text, language);

                    //                    prevParagraph.EndTime.TotalMilliseconds = prevParagraph.EndTime.TotalMilliseconds + currentParagraph.Duration.TotalMilliseconds;
                    prevParagraph.EndTime.TotalMilliseconds = currentParagraph.EndTime.TotalMilliseconds;

                    if (_networkSession != null)
                    {
                        _networkSession.TimerStop();
                        List<int> deleteIndices = new List<int>();
                        deleteIndices.Add(_subtitle.GetIndex(currentParagraph));
                        NetworkGetSendUpdates(deleteIndices, 0, null);
                    }
                    else
                    {
                        _subtitle.Paragraphs.Remove(currentParagraph);
                        _subtitle.Renumber(1);
                        SubtitleListview1.Fill(_subtitle, _subtitleAlternate);
                        SubtitleListview1.Items[firstSelectedIndex - 1].Selected = true;
                    }
                    SubtitleListview1.SelectIndexAndEnsureVisible(firstSelectedIndex - 1, true);
                    ShowSource();
                    ShowStatus(_language.LinesMerged);
                    SubtitleListview1.SelectedIndexChanged += SubtitleListview1_SelectedIndexChanged;
                    RefreshSelectedParagraph();
                }
            }
        }

        private void MergeSelectedLines()
        {
            if (_subtitle.Paragraphs.Count > 0 && SubtitleListview1.SelectedItems.Count > 1)
            {
                var sb = new StringBuilder();
                var deleteIndices = new List<int>();
                bool first = true;
                int firstIndex = 0;
                double durationMilliseconds = 0;
                int next = 0;
                foreach (int index in SubtitleListview1.SelectedIndices)
                {
                    if (first)
                    {
                        firstIndex = index;
                        next = index + 1;
                    }
                    else
                    {
                        deleteIndices.Add(index);
                        if (next != index)
                            return;
                        next++;
                    }
                    first = false;
                    sb.AppendLine(_subtitle.Paragraphs[index].Text);
                    durationMilliseconds += _subtitle.Paragraphs[index].Duration.TotalMilliseconds;
                }

                if (sb.Length > 200)
                    return;

                SubtitleListview1.SelectedIndexChanged -= SubtitleListview1_SelectedIndexChanged;
                MakeHistoryForUndo(_language.BeforeMergeLines);

                Paragraph currentParagraph = _subtitle.Paragraphs[firstIndex];
                string text = sb.ToString();
                text = Utilities.FixInvalidItalicTags(text);
                text = ChangeAllLinesItalictoSingleItalic(text);
                text = Utilities.AutoBreakLine(text, Utilities.AutoDetectGoogleLanguage(_subtitle));
                currentParagraph.Text = text;

                //display time
                currentParagraph.EndTime.TotalMilliseconds = currentParagraph.StartTime.TotalMilliseconds + durationMilliseconds;

                Paragraph nextParagraph = _subtitle.GetParagraphOrDefault(next);
                if (nextParagraph != null && currentParagraph.EndTime.TotalMilliseconds > nextParagraph.StartTime.TotalMilliseconds && currentParagraph.StartTime.TotalMilliseconds < nextParagraph.StartTime.TotalMilliseconds)
                {
                    currentParagraph.EndTime.TotalMilliseconds = nextParagraph.StartTime.TotalMilliseconds - 1;
                }

                // original subtitle
                if (_subtitleAlternate != null && Configuration.Settings.General.AllowEditOfOriginalSubtitle)
                {
                    Paragraph original = Utilities.GetOriginalParagraph(firstIndex, currentParagraph, _subtitleAlternate.Paragraphs);
                    if (original != null)
                    {
                        var originalTexts = new StringBuilder();
                        originalTexts.Append(original.Text + " ");
                        for (int i = 0; i < deleteIndices.Count; i++)
                        {
                            Paragraph originalNext = Utilities.GetOriginalParagraph(deleteIndices[i], _subtitle.Paragraphs[deleteIndices[i]], _subtitleAlternate.Paragraphs);
                            if (originalNext != null)
                                originalTexts.Append(originalNext.Text + " ");
                        }
                        for (int i = deleteIndices.Count - 1; i >= 0; i--)
                        {
                            Paragraph originalNext = Utilities.GetOriginalParagraph(deleteIndices[i], _subtitle.Paragraphs[deleteIndices[i]], _subtitleAlternate.Paragraphs);
                            if (originalNext != null)
                                _subtitleAlternate.Paragraphs.Remove(originalNext);
                        }
                        original.Text = originalTexts.ToString().Replace("  ", " ");
                        original.Text = original.Text.Replace(Environment.NewLine, " ");
                        original.Text = ChangeAllLinesItalictoSingleItalic(original.Text);
                        original.Text = Utilities.AutoBreakLine(original.Text);
                        original.EndTime.TotalMilliseconds = currentParagraph.EndTime.TotalMilliseconds;
                        _subtitleAlternate.Renumber(1);
                    }
                }

                if (_networkSession != null)
                {
                    _networkSession.TimerStop();
                    _networkSession.UpdateLine(firstIndex, currentParagraph);
                    NetworkGetSendUpdates(deleteIndices, 0, null);
                }
                else
                {
                    for (int i = deleteIndices.Count - 1; i >= 0; i--)
                        _subtitle.Paragraphs.RemoveAt(deleteIndices[i]);
                    _subtitle.Renumber(1);
                    SubtitleListview1.Fill(_subtitle, _subtitleAlternate);
                }
                ShowSource();
                ShowStatus(_language.LinesMerged);
                SubtitleListview1.SelectIndexAndEnsureVisible(firstIndex, true);
                SubtitleListview1.SelectedIndexChanged += SubtitleListview1_SelectedIndexChanged;
                RefreshSelectedParagraph();
            }
        }

        private static string ChangeAllLinesItalictoSingleItalic(string text)
        {
            bool allLinesStartAndEndsWithItalic = text.Contains("<i>");
            foreach (string line in text.Split(Utilities.NewLineChars, StringSplitOptions.RemoveEmptyEntries))
            {
                if (!line.TrimStart().StartsWith("<i>") || !line.TrimEnd().EndsWith("</i>"))
                    allLinesStartAndEndsWithItalic = false;
            }
            if (allLinesStartAndEndsWithItalic)
            {
                text = HtmlUtils.RemoveOpenCloseTags(text, HtmlUtils.TagItalic).Trim();
                text = "<i>" + text + "</i>";
            }
            return text;
        }

        private void MergeAfterToolStripMenuItemClick(object sender, EventArgs e)
        {
            if (_subtitle.Paragraphs.Count > 0 && SubtitleListview1.SelectedItems.Count > 0)
            {
                if (SubtitleListview1.SelectedItems.Count > 2)
                {
                    MergeSelectedLines();
                    return;
                }

                MergeWithLineAfter(false);
            }
        }

        private void MergeWithLineAfter(bool insertDash)
        {
            int firstSelectedIndex = SubtitleListview1.SelectedItems[0].Index;

            Paragraph currentParagraph = _subtitle.GetParagraphOrDefault(firstSelectedIndex);
            Paragraph nextParagraph = _subtitle.GetParagraphOrDefault(firstSelectedIndex + 1);

            if (nextParagraph != null && currentParagraph != null)
            {
                SubtitleListview1.SelectedIndexChanged -= SubtitleListview1_SelectedIndexChanged;
                MakeHistoryForUndo(_language.BeforeMergeLines);

                if (_subtitleAlternate != null)
                {
                    Paragraph original = Utilities.GetOriginalParagraph(firstSelectedIndex, currentParagraph, _subtitleAlternate.Paragraphs);
                    Paragraph originalNext = Utilities.GetOriginalParagraph(firstSelectedIndex + 1, nextParagraph, _subtitleAlternate.Paragraphs);

                    if (originalNext != null)
                    {
                        if (original == null)
                        {
                            originalNext.StartTime.TotalMilliseconds = currentParagraph.StartTime.TotalMilliseconds;
                            originalNext.EndTime.TotalMilliseconds = nextParagraph.EndTime.TotalMilliseconds;
                        }
                        else
                        {
                            if (insertDash)
                            {
                                string s = Utilities.UnbreakLine(original.Text);
                                if (s.StartsWith('-') || s.StartsWith("<i>-"))
                                    original.Text = s;
                                else if (s.StartsWith("<i>"))
                                    original.Text = s.Insert(3, "- ");
                                else
                                    original.Text = "- " + s;

                                s = Utilities.UnbreakLine(originalNext.Text);
                                if (s.StartsWith('-') || s.StartsWith("<i>-"))
                                    original.Text += Environment.NewLine + s;
                                else if (s.StartsWith("<i>"))
                                    original.Text += Environment.NewLine + s.Insert(3, "- ");
                                else
                                    original.Text += Environment.NewLine + "- " + s;

                                original.Text = original.Text.Replace("</i>" + Environment.NewLine + "<i>", Environment.NewLine);
                            }
                            else
                            {
                                string old1 = original.Text;
                                string old2 = originalNext.Text;
                                original.Text = original.Text.Replace(Environment.NewLine, " ");
                                original.Text += Environment.NewLine + originalNext.Text.Replace(Environment.NewLine, " ");
                                original.Text = ChangeAllLinesItalictoSingleItalic(original.Text);

                                if (old1.Contains(Environment.NewLine) || old2.Contains(Environment.NewLine) ||
                                    old1.Length > Configuration.Settings.General.SubtitleLineMaximumLength || old2.Length > Configuration.Settings.General.SubtitleLineMaximumLength)
                                    original.Text = Utilities.AutoBreakLine(original.Text, Utilities.AutoDetectGoogleLanguage(_subtitleAlternate));

                                if (string.IsNullOrWhiteSpace(old1))
                                    original.Text = original.Text.TrimStart();

                                if (string.IsNullOrWhiteSpace(old2))
                                    original.Text = original.Text.TrimEnd();
                            }
                            original.EndTime = originalNext.EndTime;
                            _subtitleAlternate.Paragraphs.Remove(originalNext);
                        }
                        _subtitleAlternate.Renumber(1);
                    }
                }

                if (insertDash)
                {
                    string s = Utilities.UnbreakLine(currentParagraph.Text);
                    if (s.StartsWith('-') || s.StartsWith("<i>-"))
                        currentParagraph.Text = s;
                    else if (s.StartsWith("<i>"))
                        currentParagraph.Text = s.Insert(3, "- ");
                    else
                        currentParagraph.Text = "- " + s;

                    s = Utilities.UnbreakLine(nextParagraph.Text);
                    if (s.StartsWith('-') || s.StartsWith("<i>-"))
                        currentParagraph.Text += Environment.NewLine + s;
                    else if (s.StartsWith("<i>"))
                        currentParagraph.Text += Environment.NewLine + s.Insert(3, "- ");
                    else
                        currentParagraph.Text += Environment.NewLine + "- " + s;

                    currentParagraph.Text = currentParagraph.Text.Replace("</i>" + Environment.NewLine + "<i>", Environment.NewLine);
                }
                else
                {
                    string old1 = currentParagraph.Text;
                    string old2 = nextParagraph.Text;
                    currentParagraph.Text = currentParagraph.Text.Replace(Environment.NewLine, " ");
                    currentParagraph.Text += Environment.NewLine + nextParagraph.Text.Replace(Environment.NewLine, " ");
                    currentParagraph.Text = ChangeAllLinesItalictoSingleItalic(currentParagraph.Text);

                    if (old1.Contains(Environment.NewLine) || old2.Contains(Environment.NewLine) ||
                        old1.Length > Configuration.Settings.General.SubtitleLineMaximumLength || old2.Length > Configuration.Settings.General.SubtitleLineMaximumLength)
                        currentParagraph.Text = Utilities.AutoBreakLine(currentParagraph.Text, Utilities.AutoDetectGoogleLanguage(_subtitle));

                    if (string.IsNullOrWhiteSpace(old1))
                        currentParagraph.Text = currentParagraph.Text.TrimStart();

                    if (string.IsNullOrWhiteSpace(old2))
                        currentParagraph.Text = currentParagraph.Text.TrimEnd();
                }

                //currentParagraph.EndTime.TotalMilliseconds = currentParagraph.EndTime.TotalMilliseconds + nextParagraph.Duration.TotalMilliseconds; //nextParagraph.EndTime;
                currentParagraph.EndTime.TotalMilliseconds = nextParagraph.EndTime.TotalMilliseconds;

                if (_networkSession != null)
                {
                    _networkSession.TimerStop();
                    SetDurationInSeconds(currentParagraph.Duration.TotalSeconds);
                    _networkSession.UpdateLine(_subtitle.GetIndex(currentParagraph), currentParagraph);
                    List<int> deleteIndices = new List<int>();
                    deleteIndices.Add(_subtitle.GetIndex(nextParagraph));
                    NetworkGetSendUpdates(deleteIndices, 0, null);
                }
                else
                {
                    _subtitle.Paragraphs.Remove(nextParagraph);
                    _subtitle.Renumber(1);
                    SubtitleListview1.Fill(_subtitle, _subtitleAlternate);
                }
                ShowSource();
                ShowStatus(_language.LinesMerged);
                SubtitleListview1.SelectIndexAndEnsureVisible(firstSelectedIndex);
                SubtitleListview1.SelectedIndexChanged += SubtitleListview1_SelectedIndexChanged;
                RefreshSelectedParagraph();
                SubtitleListview1.SelectIndexAndEnsureVisible(firstSelectedIndex, true);
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
                if (GetCurrentSubtitleFormat().IsFrameBased)
                {
                    p.CalculateFrameNumbersFromTimeCodes(CurrentFrameRate);
                    p.CalculateTimeCodesFromFrameNumbers(CurrentFrameRate);
                }

                StartUpdateListSyntaxColoring();
            }
        }

        private void StartUpdateListSyntaxColoring()
        {
            if (!_timerDoSyntaxColoring.Enabled)
                _timerDoSyntaxColoring.Start();
        }

        private void UpdateListSyntaxColoring()
        {
            if (_loading)
                return;

            if (!IsSubtitleLoaded || _subtitleListViewIndex < 0 || _subtitleListViewIndex >= _subtitle.Paragraphs.Count)
                return;

            SubtitleListview1.SyntaxColorLine(_subtitle.Paragraphs, _subtitleListViewIndex, _subtitle.Paragraphs[_subtitleListViewIndex]);
            Paragraph next = _subtitle.GetParagraphOrDefault(_subtitleListViewIndex + 1);
            if (next != null)
                SubtitleListview1.SyntaxColorLine(_subtitle.Paragraphs, _subtitleListViewIndex + 1, _subtitle.Paragraphs[_subtitleListViewIndex + 1]);
        }

        private void UpdateOverlapErrors(TimeCode startTime)
        {
            labelStartTimeWarning.Text = string.Empty;
            labelDurationWarning.Text = string.Empty;
            if (_subtitle.Paragraphs.Count > 0 && SubtitleListview1.SelectedItems.Count > 0 && startTime != null)
            {

                int firstSelectedIndex = SubtitleListview1.SelectedItems[0].Index;

                Paragraph prevParagraph = _subtitle.GetParagraphOrDefault(firstSelectedIndex - 1);
                if (prevParagraph != null && prevParagraph.EndTime.TotalMilliseconds > startTime.TotalMilliseconds && Configuration.Settings.Tools.ListViewSyntaxColorOverlap)
                    labelStartTimeWarning.Text = string.Format(_languageGeneral.OverlapPreviousLineX, prevParagraph.EndTime.TotalSeconds - startTime.TotalSeconds);

                Paragraph nextParagraph = _subtitle.GetParagraphOrDefault(firstSelectedIndex + 1);
                if (nextParagraph != null)
                {
                    double durationMilliSeconds = GetDurationInMilliseconds();
                    if (startTime.TotalMilliseconds + durationMilliSeconds > nextParagraph.StartTime.TotalMilliseconds && Configuration.Settings.Tools.ListViewSyntaxColorOverlap)
                    {
                        labelDurationWarning.Text = string.Format(_languageGeneral.OverlapX, ((startTime.TotalMilliseconds + durationMilliSeconds) - nextParagraph.StartTime.TotalMilliseconds) / 1000.0);
                    }

                    if (labelStartTimeWarning.Text.Length == 0 &&
                        startTime.TotalMilliseconds > nextParagraph.StartTime.TotalMilliseconds && Configuration.Settings.Tools.ListViewSyntaxColorOverlap)
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

        private double GetDurationInMilliseconds()
        {
            if (Configuration.Settings.General.UseTimeFormatHHMMSSFF)
            {
                int seconds = (int)numericUpDownDuration.Value;
                int frames = (int)Math.Round((Convert.ToDouble(numericUpDownDuration.Value) % 1.0 * 100.0));
                return seconds * 1000.0 + frames * (1000.0 / Configuration.Settings.General.CurrentFrameRate);
            }
            return ((double)numericUpDownDuration.Value * 1000.0);
        }

        private void SetDurationInSeconds(double seconds)
        {
            if (Configuration.Settings.General.UseTimeFormatHHMMSSFF)
            {
                int wholeSeconds = (int)seconds;
                int frames = SubtitleFormat.MillisecondsToFrames(seconds % 1.0 * 1000.0);
                int extraSeconds = (int)(frames / Configuration.Settings.General.CurrentFrameRate);
                int restFrames = (int)(frames % Configuration.Settings.General.CurrentFrameRate);
                numericUpDownDuration.Value = (decimal)(wholeSeconds + extraSeconds + restFrames / 100.0);
            }
            else
            {
                var d = (decimal)seconds;
                if (d > numericUpDownDuration.Maximum)
                    numericUpDownDuration.Value = numericUpDownDuration.Maximum;
                else if (d < numericUpDownDuration.Minimum)
                    numericUpDownDuration.Value = numericUpDownDuration.Minimum;
                else
                    numericUpDownDuration.Value = d;
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

                    // update _subtitle + listview
                    string oldDuration = currentParagraph.Duration.ToString();

                    var temp = new Paragraph(currentParagraph);

                    if (Configuration.Settings.General.UseTimeFormatHHMMSSFF)
                    {
                        int seconds = (int)numericUpDownDuration.Value;
                        int frames = Convert.ToInt32((numericUpDownDuration.Value - seconds) * 100);
                        if (frames > Math.Round(Configuration.Settings.General.CurrentFrameRate) - 1)
                        {
                            numericUpDownDuration.ValueChanged -= NumericUpDownDurationValueChanged;
                            if (frames >= 99)
                                numericUpDownDuration.Value = (decimal)(seconds + ((Math.Round((Configuration.Settings.General.CurrentFrameRate - 1))) / 100.0));
                            else
                                numericUpDownDuration.Value = seconds + 1;
                            numericUpDownDuration.ValueChanged += NumericUpDownDurationValueChanged;
                        }
                    }
                    temp.EndTime.TotalMilliseconds = currentParagraph.StartTime.TotalMilliseconds + GetDurationInMilliseconds();

                    MakeHistoryForUndoOnlyIfNotResent(string.Format(_language.DisplayTimeAdjustedX, "#" + currentParagraph.Number + ": " + oldDuration + " -> " + temp.Duration));

                    currentParagraph.EndTime.TotalMilliseconds = temp.EndTime.TotalMilliseconds;
                    SubtitleListview1.SetDuration(firstSelectedIndex, currentParagraph);

                    UpdateOverlapErrors(timeUpDownStartTime.TimeCode);
                    UpdateListViewTextCharactersPerSeconds(labelCharactersPerSecond, currentParagraph);

                    if (Configuration.Settings.General.AllowEditOfOriginalSubtitle && _subtitleAlternate != null && _subtitleAlternate.Paragraphs.Count > 0)
                    {
                        Paragraph original = Utilities.GetOriginalParagraph(firstSelectedIndex, currentParagraph, _subtitleAlternate.Paragraphs);
                        if (original != null)
                        {
                            original.EndTime.TotalMilliseconds = currentParagraph.EndTime.TotalMilliseconds;
                        }
                    }

                    StartUpdateListSyntaxColoring();

                    if (GetCurrentSubtitleFormat().IsFrameBased)
                    {
                        currentParagraph.CalculateFrameNumbersFromTimeCodes(CurrentFrameRate);
                        currentParagraph.CalculateTimeCodesFromFrameNumbers(CurrentFrameRate);
                    }
                }
                labelStatus.Text = string.Empty;
                StartUpdateListSyntaxColoring();
            }
        }

        private void InitializeListViewEditBoxAlternate(Paragraph p, int firstSelectedIndex)
        {
            if (_subtitleAlternate != null && _subtitleAlternate.Paragraphs.Count > 0)
            {
                Paragraph original = Utilities.GetOriginalParagraph(firstSelectedIndex, p, _subtitleAlternate.Paragraphs);
                if (original == null)
                {
                    textBoxListViewTextAlternate.Enabled = false;
                    textBoxListViewTextAlternate.Text = string.Empty;
                    labelAlternateCharactersPerSecond.Text = string.Empty;
                }
                else
                {
                    textBoxListViewTextAlternate.Enabled = true;
                    textBoxListViewTextAlternate.TextChanged -= TextBoxListViewTextAlternateTextChanged;
                    textBoxListViewTextAlternate.Text = original.Text;
                    textBoxListViewTextAlternate.TextChanged += TextBoxListViewTextAlternateTextChanged;
                    UpdateListViewTextCharactersPerSeconds(labelAlternateCharactersPerSecond, original);
                    _listViewAlternateTextUndoLast = original.Text;
                }
            }
        }

        private void InitializeListViewEditBox(Paragraph p)
        {
            textBoxListViewText.TextChanged -= TextBoxListViewTextTextChanged;
            textBoxListViewText.Text = p.Text;
            textBoxListViewText.TextChanged += TextBoxListViewTextTextChanged;
            _listViewTextUndoLast = p.Text;

            timeUpDownStartTime.MaskedTextBox.TextChanged -= MaskedTextBoxTextChanged;
            timeUpDownStartTime.TimeCode = p.StartTime;
            timeUpDownStartTime.MaskedTextBox.TextChanged += MaskedTextBoxTextChanged;

            numericUpDownDuration.ValueChanged -= NumericUpDownDurationValueChanged;
            if (p.Duration.TotalSeconds > (double)numericUpDownDuration.Maximum)
                SetDurationInSeconds((double)numericUpDownDuration.Maximum);
            else
                SetDurationInSeconds(p.Duration.TotalSeconds);
            numericUpDownDuration.ValueChanged += NumericUpDownDurationValueChanged;

            UpdateOverlapErrors(timeUpDownStartTime.TimeCode);
            UpdateListViewTextCharactersPerSeconds(labelCharactersPerSecond, p);
            if (_subtitle != null && _subtitle.Paragraphs.Count > 0)
                textBoxListViewText.Enabled = true;

            StartUpdateListSyntaxColoring();
        }

        private void MaskedTextBoxTextChanged(object sender, EventArgs e)
        {
            if (_subtitleListViewIndex >= 0)
            {
                MakeHistoryForUndoOnlyIfNotResent(string.Format(_language.StarTimeAdjustedX, "#" + (_subtitleListViewIndex + 1) + ": " + timeUpDownStartTime.TimeCode));

                int firstSelectedIndex = FirstSelectedIndex;
                Paragraph oldParagraph = _subtitle.GetParagraphOrDefault(firstSelectedIndex);
                if (oldParagraph != null)
                    oldParagraph = new Paragraph(oldParagraph);

                UpdateStartTimeInfo(timeUpDownStartTime.TimeCode);

                UpdateOriginalTimeCodes(oldParagraph);
                labelStatus.Text = string.Empty;
            }
        }

        private void UpdateOriginalTimeCodes(Paragraph currentPargraphBeforeChange)
        {
            if (Configuration.Settings.General.AllowEditOfOriginalSubtitle && _subtitleAlternate != null && _subtitleAlternate.Paragraphs.Count > 0)
            {
                int firstSelectedIndex = FirstSelectedIndex;
                Paragraph p = _subtitle.GetParagraphOrDefault(firstSelectedIndex);
                if (currentPargraphBeforeChange != null && p != null)
                {
                    Paragraph original = Utilities.GetOriginalParagraph(FirstSelectedIndex, currentPargraphBeforeChange, _subtitleAlternate.Paragraphs);
                    if (original != null)
                    {
                        original.StartTime.TotalMilliseconds = p.StartTime.TotalMilliseconds;
                        original.EndTime.TotalMilliseconds = p.EndTime.TotalMilliseconds;
                    }
                }
            }
        }

        private void FormMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            _lastDoNotPrompt = string.Empty;
            ReloadFromSourceView();
            if (!ContinueNewOrExit())
            {
                e.Cancel = true;
            }
            else
            {
                if (_networkSession != null)
                {
                    try
                    {
                        _networkSession.TimerStop();
                        _networkSession.Leave();
                    }
                    catch
                    {
                    }
                }

                if (Configuration.Settings.General.StartRememberPositionAndSize && WindowState != FormWindowState.Minimized)
                {
                    Configuration.Settings.General.StartPosition = Left + ";" + Top;
                    if (WindowState == FormWindowState.Maximized)
                        Configuration.Settings.General.StartSize = "Maximized";
                    else
                        Configuration.Settings.General.StartSize = Width + ";" + Height;

                    Configuration.Settings.General.SplitContainerMainSplitterDistance = splitContainerMain.SplitterDistance;
                    Configuration.Settings.General.SplitContainer1SplitterDistance = splitContainer1.SplitterDistance;
                    Configuration.Settings.General.SplitContainerListViewAndTextSplitterDistance = splitContainerListViewAndText.SplitterDistance;
                }
                Configuration.Settings.General.AutoRepeatOn = checkBoxAutoRepeatOn.Checked;
                Configuration.Settings.General.AutoRepeatCount = Convert.ToInt32(comboBoxAutoRepeat.Text);
                Configuration.Settings.General.AutoContinueOn = checkBoxAutoContinue.Checked;
                Configuration.Settings.General.SyncListViewWithVideoWhilePlaying = checkBoxSyncListViewWithVideoWhilePlaying.Checked;
                if (audioVisualizer != null)
                {
                    Configuration.Settings.General.ShowWaveform = audioVisualizer.ShowWaveform;
                    Configuration.Settings.General.ShowSpectrogram = audioVisualizer.ShowSpectrogram;
                }
                if (!string.IsNullOrEmpty(_fileName))
                    Configuration.Settings.RecentFiles.Add(_fileName, FirstVisibleIndex, FirstSelectedIndex, _videoFileName, _subtitleAlternateFileName);
                Configuration.Settings.General.RightToLeftMode = toolStripMenuItemRightToLeftMode.Checked;

                SaveUndockedPositions();
                SaveListViewWidths();
                Configuration.Settings.Save();

                if (mediaPlayer.VideoPlayer != null)
                {
                    mediaPlayer.VideoPlayer.DisposeVideoPlayer();
                }

            }
        }

        private void SaveListViewWidths()
        {
            if (Configuration.Settings.General.ListViewColumnsRememberSize)
            {
                Configuration.Settings.General.ListViewNumberWidth = SubtitleListview1.Columns[0].Width;
                Configuration.Settings.General.ListViewStartWidth = SubtitleListview1.Columns[1].Width;
                Configuration.Settings.General.ListViewEndWidth = SubtitleListview1.Columns[2].Width;
                Configuration.Settings.General.ListViewDurationWidth = SubtitleListview1.Columns[3].Width;
                Configuration.Settings.General.ListViewTextWidth = SubtitleListview1.Columns[4].Width;
            }
        }

        private void SaveUndockedPositions()
        {
            if (_videoPlayerUndocked != null && !_videoPlayerUndocked.IsDisposed)
                Configuration.Settings.General.UndockedVideoPosition = _videoPlayerUndocked.Left + @";" + _videoPlayerUndocked.Top + @";" + _videoPlayerUndocked.Width + @";" + _videoPlayerUndocked.Height;
            if (_waveformUndocked != null && !_waveformUndocked.IsDisposed)
                Configuration.Settings.General.UndockedWaveformPosition = _waveformUndocked.Left + @";" + _waveformUndocked.Top + @";" + _waveformUndocked.Width + @";" + _waveformUndocked.Height;
            if (_videoControlsUndocked != null && !_videoControlsUndocked.IsDisposed)
                Configuration.Settings.General.UndockedVideoControlsPosition = _videoControlsUndocked.Left + @";" + _videoControlsUndocked.Top + @";" + _videoControlsUndocked.Width + @";" + _videoControlsUndocked.Height;
        }

        private void ButtonUnBreakClick(object sender, EventArgs e)
        {
            if (SubtitleListview1.SelectedItems.Count > 1)
            {
                MakeHistoryForUndo(_language.BeforeRemoveLineBreaksInSelectedLines);

                SubtitleListview1.BeginUpdate();
                foreach (int index in SubtitleListview1.SelectedIndices)
                {
                    Paragraph p = _subtitle.GetParagraphOrDefault(index);
                    p.Text = Utilities.UnbreakLine(p.Text);
                    SubtitleListview1.SetText(index, p.Text);
                }
                SubtitleListview1.EndUpdate();
                RefreshSelectedParagraph();
            }
            else
            {
                textBoxListViewText.Text = Utilities.UnbreakLine(textBoxListViewText.Text);
            }
        }

        private void TabControlSubtitleSelectedIndexChanged(object sender, EventArgs e)
        {
            var format = GetCurrentSubtitleFormat();
            if (tabControlSubtitle.SelectedIndex == TabControlSourceView)
            {
                ShowSource();
                ShowSourceLineNumber();
                if (textBoxSource.CanFocus)
                    textBoxSource.Focus();

                // go to correct line in source view
                if (SubtitleListview1.SelectedItems.Count > 0)
                {
                    if (format.GetType() == typeof(SubRip))
                    {
                        Paragraph p = _subtitle.GetParagraphOrDefault(FirstSelectedIndex);
                        if (p != null)
                        {
                            string tc = p.StartTime + " --> " + p.EndTime;
                            int start = textBoxSource.Text.IndexOf(tc, StringComparison.Ordinal);
                            if (start > 0)
                            {
                                textBoxSource.SelectionStart = start + tc.Length + Environment.NewLine.Length;
                                textBoxSource.SelectionLength = 0;
                                textBoxSource.ScrollToCaret();
                            }
                        }
                    }
                    else if (format.GetType() == typeof(SubStationAlpha) || format.GetType() == typeof(AdvancedSubStationAlpha))
                    {
                        Paragraph p = _subtitle.GetParagraphOrDefault(FirstSelectedIndex);
                        if (p != null)
                        {
                            const string timeCodeFormat = "{0}:{1:00}:{2:00}.{3:00}"; // h:mm:ss.cc
                            string startTC = string.Format(timeCodeFormat, p.StartTime.Hours, p.StartTime.Minutes, p.StartTime.Seconds, p.StartTime.Milliseconds / 10);
                            string endTC = string.Format(timeCodeFormat, p.EndTime.Hours, p.EndTime.Minutes, p.EndTime.Seconds, p.EndTime.Milliseconds / 10);
                            string tc = startTC + "," + endTC;
                            int start = textBoxSource.Text.IndexOf(tc, StringComparison.Ordinal);
                            if (start > 0)
                            {
                                int start2 = textBoxSource.Text.LastIndexOf("Dialogue:", start, StringComparison.Ordinal);
                                if (start2 > 0)
                                    start2 = (textBoxSource.Text + Environment.NewLine).IndexOf(Environment.NewLine, start2, StringComparison.Ordinal);
                                if (start2 > 0)
                                    start = start2;
                                textBoxSource.SelectionStart = start;
                                textBoxSource.SelectionLength = 0;
                                textBoxSource.ScrollToCaret();
                            }
                        }
                    }
                }
            }
            else
            {
                ReloadFromSourceView();
                ShowLineInformationListView();
                if (SubtitleListview1.CanFocus)
                    SubtitleListview1.Focus();

                // go to (select + focus) correct line in list view
                if (textBoxSource.SelectionStart > 0 && textBoxSource.TextLength > 30)
                {
                    if (format.GetType() == typeof(SubRip))
                    {
                        int pos = textBoxSource.SelectionStart;
                        if (pos + 35 < textBoxSource.TextLength)
                            pos += 35;
                        string s = textBoxSource.Text.Substring(0, pos);
                        int lastTimeCode = s.LastIndexOf(" --> ", StringComparison.Ordinal); // 00:02:26,407 --> 00:02:31,356
                        if (lastTimeCode > 14 && lastTimeCode + 16 >= s.Length)
                        {
                            s = s.Substring(0, lastTimeCode - 5);
                            lastTimeCode = s.LastIndexOf(" --> ", StringComparison.Ordinal);
                        }

                        if (lastTimeCode > 14 && lastTimeCode + 16 < s.Length)
                        {
                            string tc = s.Substring(lastTimeCode - 13, 30).Trim();
                            int index = 0;
                            foreach (Paragraph p in _subtitle.Paragraphs)
                            {
                                if (tc == p.StartTime + " --> " + p.EndTime)
                                {
                                    SubtitleListview1.SelectIndexAndEnsureVisible(index, true);
                                    break;
                                }
                                index++;
                            }
                        }
                    }
                    else if (format.GetType() == typeof(SubStationAlpha) || format.GetType() == typeof(AdvancedSubStationAlpha))
                    {
                        int pos = textBoxSource.SelectionStart;
                        string s = textBoxSource.Text;
                        if (pos > 0)
                            pos--;
                        while (pos > 0 && pos + 3 < s.Length && !s.Substring(pos, 3).StartsWith(Environment.NewLine))
                            pos--;
                        s = s.Substring(pos).Trim();
                        int lastTimeCode = s.IndexOf("Dialogue:", StringComparison.Ordinal);

                        if (lastTimeCode >= 0)
                        {
                            string tc = s.Substring(lastTimeCode).Trim();
                            while (tc.Length > 0 && !char.IsDigit(tc[0]))
                                tc = tc.Remove(0, 1);
                            if (tc.Length > 12)
                            {
                                tc = tc.Substring(0, 13);
                                string[] timeCode = tc.Split(new[] { ':', '.', ',' }, StringSplitOptions.RemoveEmptyEntries);
                                TimeCode realTC = new TimeCode(0, 0, 0, 0);
                                try
                                {
                                    realTC = new TimeCode(int.Parse(timeCode[1]), int.Parse(timeCode[2]), int.Parse(timeCode[3]), int.Parse(timeCode[4]) * 10);
                                }
                                catch
                                {
                                    SubtitleListview1.SelectIndexAndEnsureVisible(0, true);
                                    return;
                                }

                                int index = 0;
                                foreach (Paragraph p in _subtitle.Paragraphs)
                                {
                                    if (Math.Abs(realTC.TotalMilliseconds - p.StartTime.TotalMilliseconds) < 50)
                                    {
                                        SubtitleListview1.SelectIndexAndEnsureVisible(index, true);
                                        break;
                                    }
                                    index++;
                                }
                            }
                        }
                    }
                }
                else if (textBoxSource.SelectionStart == 0 && textBoxSource.TextLength > 30)
                {
                    SubtitleListview1.SelectIndexAndEnsureVisible(0, true);
                }
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
                            SetFontColor(p, color);
                            SubtitleListview1.SetText(item.Index, p.Text);
                            if (_subtitleAlternate != null && Configuration.Settings.General.AllowEditOfOriginalSubtitle && SubtitleListview1.IsAlternateTextColumnVisible)
                            {
                                Paragraph original = Utilities.GetOriginalParagraph(item.Index, p, _subtitleAlternate.Paragraphs);
                                if (original != null)
                                {
                                    SetFontColor(original, color);
                                    SubtitleListview1.SetAlternateText(item.Index, original.Text);
                                }
                            }
                        }
                    }
                    RefreshSelectedParagraph();
                }
            }
        }

        private static void SetFontColor(Paragraph p, string color)
        {
            if (p == null)
                return;

            string s = p.Text;
            if (s.StartsWith("<font "))
            {
                int end = s.IndexOf('>');
                if (end > 0)
                {
                    string f = s.Substring(0, end);

                    if (f.Contains(" face=") && !f.Contains(" color="))
                    {
                        var start = s.IndexOf(" face=", StringComparison.Ordinal);
                        s = s.Insert(start, string.Format(" color=\"{0}\"", color));
                        p.Text = s;
                        return;
                    }

                    var colorStart = f.IndexOf(" color=", StringComparison.Ordinal);
                    if (colorStart >= 0)
                    {
                        if (s.IndexOf('"', colorStart + 8) > 0)
                            end = s.IndexOf('"', colorStart + 8);
                        s = s.Substring(0, colorStart) + string.Format(" color=\"{0}", color) + s.Substring(end);
                        p.Text = s;
                        return;
                    }
                }
            }

            p.Text = string.Format("<font color=\"{0}\">{1}</font>", color, p.Text);
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
                            SetFontName(p);
                            SubtitleListview1.SetText(item.Index, p.Text);
                            if (_subtitleAlternate != null && Configuration.Settings.General.AllowEditOfOriginalSubtitle && SubtitleListview1.IsAlternateTextColumnVisible)
                            {
                                Paragraph original = Utilities.GetOriginalParagraph(item.Index, p, _subtitleAlternate.Paragraphs);
                                if (original != null)
                                {
                                    SetFontName(original);
                                    SubtitleListview1.SetAlternateText(item.Index, original.Text);
                                }
                            }
                        }
                    }
                    RefreshSelectedParagraph();
                }
            }
        }

        private void SetFontName(Paragraph p)
        {
            if (p == null)
                return;

            string s = p.Text;
            if (s.StartsWith("<font "))
            {
                var end = s.IndexOf('>');
                if (end > 0)
                {
                    var f = s.Substring(0, end);

                    if (f.Contains(" color=") && !f.Contains(" face="))
                    {
                        var start = s.IndexOf(" color=", StringComparison.Ordinal);
                        p.Text = s.Insert(start, string.Format(" face=\"{0}\"", fontDialog1.Font.Name));
                        return;
                    }

                    var faceStart = f.IndexOf(" face=", StringComparison.Ordinal);
                    if (f.Contains(" face="))
                    {
                        if (s.IndexOf('"', faceStart + 7) > 0)
                            end = s.IndexOf('"', faceStart + 7);
                        p.Text = s.Substring(0, faceStart) + string.Format(" face=\"{0}", fontDialog1.Font.Name) + s.Substring(end);
                        return;
                    }
                }
            }

            p.Text = string.Format("<font face=\"{0}\">{1}</font>", fontDialog1.Font.Name, s);
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
                    int lastSelectedIndex = SubtitleListview1.SelectedItems[0].Index;
                    int index = lastSelectedIndex;
                    _subtitle.Paragraphs.RemoveAt(index);
                    bool isframeBased = GetCurrentSubtitleFormat().IsFrameBased;
                    foreach (Paragraph p in typewriter.TypewriterParagraphs)
                    {
                        if (isframeBased)
                        {
                            p.CalculateFrameNumbersFromTimeCodes(CurrentFrameRate);
                            p.CalculateTimeCodesFromFrameNumbers(CurrentFrameRate);
                        }
                        _subtitle.Paragraphs.Insert(index, p);
                        index++;
                    }
                    _subtitle.Renumber(1);
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
                    int lastSelectedIndex = SubtitleListview1.SelectedItems[0].Index;
                    bool isframeBased = GetCurrentSubtitleFormat().IsFrameBased;

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
                                if (isframeBased)
                                {
                                    p.CalculateFrameNumbersFromTimeCodes(CurrentFrameRate);
                                    p.CalculateTimeCodesFromFrameNumbers(CurrentFrameRate);
                                }
                                _subtitle.Paragraphs.Insert(index, kp);
                                index++;
                            }
                        }
                        i--;
                    }

                    _subtitle.Renumber(1);
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
                openFileDialog1.InitialDirectory = Path.GetDirectoryName(openFileDialog1.FileName);
                ImportSubtitleFromMatroskaFile(openFileDialog1.FileName);
            }
        }

        private void ImportSubtitleFromMatroskaFile(string fileName)
        {
            bool isValid;
            var matroska = new Matroska();
            var subtitleList = matroska.GetMatroskaSubtitleTracks(fileName, out isValid);
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
                            if (_loading)
                            {
                                subtitleChooser.Icon = (Icon)this.Icon.Clone();
                                subtitleChooser.ShowInTaskbar = true;
                                subtitleChooser.ShowIcon = true;
                            }
                            if (subtitleChooser.ShowDialog(this) == DialogResult.OK)
                            {
                                LoadMatroskaSubtitle(subtitleList[subtitleChooser.SelectedIndex], fileName, false);
                                if (Path.GetExtension(fileName).Equals(".mkv", StringComparison.OrdinalIgnoreCase))
                                    OpenVideo(fileName);
                            }
                        }
                        else
                        {
                            LoadMatroskaSubtitle(subtitleList[0], fileName, false);
                            if (Path.GetExtension(fileName).Equals(".mkv", StringComparison.OrdinalIgnoreCase))
                                OpenVideo(fileName);
                        }
                    }
                }
            }
            else
            {
                MessageBox.Show(string.Format(_language.NotAValidMatroskaFileX, fileName));
            }
        }

        private void MatroskaProgress(long position, long total)
        {
            ShowStatus(string.Format("{0}, {1:0}%", _language.ParsingMatroskaFile, position * 100 / total));
            statusStrip1.Refresh();
            if (DateTime.Now.Ticks % 10 == 0)
                Application.DoEvents();
        }

        internal Subtitle LoadMatroskaSubtitleForSync(MatroskaSubtitleInfo matroskaSubtitleInfo, string fileName)
        {
            Subtitle subtitle = new Subtitle();
            bool isValid;
            bool isSsa = false;
            var matroska = new Matroska();

            if (matroskaSubtitleInfo.CodecId.Equals("S_VOBSUB", StringComparison.OrdinalIgnoreCase))
            {
                return subtitle;
            }
            if (matroskaSubtitleInfo.CodecId.Equals("S_HDMV/PGS", StringComparison.OrdinalIgnoreCase))
            {
                return subtitle;
            }

            List<SubtitleSequence> sub = matroska.GetMatroskaSubtitle(fileName, (int)matroskaSubtitleInfo.TrackNumber, out isValid, MatroskaProgress);
            if (isValid)
            {
                SubtitleFormat format;
                if (matroskaSubtitleInfo.CodecPrivate.Contains("[script info]", StringComparison.OrdinalIgnoreCase))
                {
                    if (matroskaSubtitleInfo.CodecPrivate.Contains("[V4 Styles]", StringComparison.OrdinalIgnoreCase))
                        format = new SubStationAlpha();
                    else
                        format = new AdvancedSubStationAlpha();
                    isSsa = true;
                }
                else
                {
                    format = new SubRip();
                }

                if (isSsa)
                {
                    foreach (Paragraph p in LoadMatroskaSSA(matroskaSubtitleInfo, fileName, format, sub).Paragraphs)
                    {
                        subtitle.Paragraphs.Add(p);
                    }
                }
                else
                {
                    foreach (SubtitleSequence p in sub)
                    {
                        subtitle.Paragraphs.Add(new Paragraph(p.Text, p.StartMilliseconds, p.EndMilliseconds));
                    }
                }

            }
            return subtitle;
        }

        internal void LoadMatroskaSubtitle(MatroskaSubtitleInfo matroskaSubtitleInfo, string fileName, bool batchMode)
        {
            bool isValid;
            bool isSsa = false;
            var matroska = new Matroska();

            if (matroskaSubtitleInfo.CodecId.Equals("S_VOBSUB", StringComparison.OrdinalIgnoreCase))
            {
                if (batchMode)
                    return;
                LoadVobSubFromMatroska(matroskaSubtitleInfo, fileName);
                return;
            }
            if (matroskaSubtitleInfo.CodecId.Equals("S_HDMV/PGS", StringComparison.OrdinalIgnoreCase))
            {
                if (batchMode)
                    return;
                LoadBluRaySubFromMatroska(matroskaSubtitleInfo, fileName);
                return;
            }

            ShowStatus(_language.ParsingMatroskaFile);
            Refresh();
            Cursor.Current = Cursors.WaitCursor;
            List<SubtitleSequence> sub = matroska.GetMatroskaSubtitle(fileName, (int)matroskaSubtitleInfo.TrackNumber, out isValid, MatroskaProgress);
            Cursor.Current = Cursors.Default;
            if (isValid)
            {
                MakeHistoryForUndo(_language.BeforeImportFromMatroskaFile);
                _subtitleListViewIndex = -1;
                if (!batchMode)
                    ResetSubtitle();
                _subtitle.Paragraphs.Clear();

                SubtitleFormat format;
                if (matroskaSubtitleInfo.CodecPrivate.Contains("[script info]", StringComparison.OrdinalIgnoreCase))
                {
                    if (matroskaSubtitleInfo.CodecPrivate.Contains("[V4 Styles]", StringComparison.OrdinalIgnoreCase))
                        format = new SubStationAlpha();
                    else
                        format = new AdvancedSubStationAlpha();
                    isSsa = true;
                    if (_networkSession == null)
                    {
                        SubtitleListview1.ShowExtraColumn(Configuration.Settings.Language.General.Style);
                        SubtitleListview1.DisplayExtraFromExtra = true;
                    }
                }
                else
                {
                    format = new SubRip();
                    if (_networkSession == null && SubtitleListview1.IsExtraColumnVisible)
                        SubtitleListview1.HideExtraColumn();
                }

                comboBoxSubtitleFormats.SelectedIndexChanged -= ComboBoxSubtitleFormatsSelectedIndexChanged;
                SetCurrentFormat(format);
                comboBoxSubtitleFormats.SelectedIndexChanged += ComboBoxSubtitleFormatsSelectedIndexChanged;

                if (isSsa)
                {
                    foreach (Paragraph p in LoadMatroskaSSA(matroskaSubtitleInfo, fileName, format, sub).Paragraphs)
                    {
                        _subtitle.Paragraphs.Add(p);
                    }

                    if (!string.IsNullOrEmpty(matroskaSubtitleInfo.CodecPrivate))
                    {
                        bool eventsStarted = false;
                        bool fontsStarted = false;
                        bool graphicsStarted = false;
                        var header = new StringBuilder();
                        foreach (string line in matroskaSubtitleInfo.CodecPrivate.Replace(Environment.NewLine, "\n").Split('\n'))
                        {
                            if (!eventsStarted && !fontsStarted && !graphicsStarted)
                            {
                                header.AppendLine(line);
                            }
                            else if (line.TrimStart().StartsWith("dialog:", StringComparison.OrdinalIgnoreCase))
                            {
                                eventsStarted = true;
                                fontsStarted = false;
                                graphicsStarted = false;
                            }
                            else if (line.Trim().Equals("[events]", StringComparison.OrdinalIgnoreCase))
                            {
                                eventsStarted = true;
                                fontsStarted = false;
                                graphicsStarted = false;
                            }
                            else if (line.Trim().Equals("[fonts]", StringComparison.OrdinalIgnoreCase))
                            {
                                eventsStarted = false;
                                fontsStarted = true;
                                graphicsStarted = false;
                            }
                            else if (line.Trim().Equals("[graphics]", StringComparison.OrdinalIgnoreCase))
                            {
                                eventsStarted = false;
                                fontsStarted = false;
                                graphicsStarted = true;
                            }
                        }
                        _subtitle.Header = header.ToString();
                    }
                }
                else
                {
                    foreach (SubtitleSequence p in sub)
                    {
                        _subtitle.Paragraphs.Add(new Paragraph(p.Text, p.StartMilliseconds, p.EndMilliseconds));
                    }
                }

                SetEncoding(Encoding.UTF8);
                ShowStatus(_language.SubtitleImportedFromMatroskaFile);
                _subtitle.Renumber(1);
                _subtitle.WasLoadedWithFrameNumbers = false;
                if (fileName.EndsWith(".mkv", StringComparison.OrdinalIgnoreCase) || fileName.EndsWith(".mks", StringComparison.OrdinalIgnoreCase))
                {
                    _fileName = fileName.Substring(0, fileName.Length - 4);
                    Text = Title + " - " + _fileName;
                }
                else
                {
                    Text = Title;
                }
                _fileDateTime = new DateTime();

                _converted = true;

                if (batchMode)
                    return;

                SubtitleListview1.Fill(_subtitle, _subtitleAlternate);
                if (_subtitle.Paragraphs.Count > 0)
                    SubtitleListview1.SelectIndexAndEnsureVisible(0);

                ShowSource();
            }
        }

        public static Subtitle LoadMatroskaSSA(MatroskaSubtitleInfo matroskaSubtitleInfo, string fileName, SubtitleFormat format, List<SubtitleSequence> sub)
        {
            var subtitle = new Subtitle();
            subtitle.Header = matroskaSubtitleInfo.CodecPrivate;
            var lines = new List<string>();
            foreach (string l in subtitle.Header.Trim().Replace(Environment.NewLine, "\n").Split('\n'))
                lines.Add(l);
            var footer = new StringBuilder();
            var comments = new Subtitle();
            if (!string.IsNullOrEmpty(matroskaSubtitleInfo.CodecPrivate))
            {
                bool footerOn = false;
                foreach (string line in lines)
                {
                    if (footerOn)
                    {
                        footer.AppendLine(line);
                    }
                    else if (line.Trim() == "[Events]")
                    {
                        footerOn = false;
                    }
                    else if (line.Trim() == "[Fonts]" || line.Trim() == "[Graphics]")
                    {
                        footerOn = true;
                        footer.AppendLine();
                        footer.AppendLine();
                        footer.AppendLine(line);
                    }
                    else if (line.StartsWith("Comment:"))
                    {
                        var arr = line.Split(',');
                        if (arr.Length > 3)
                        {
                            arr = arr[1].Split(new[] { ':', '.' });
                            if (arr.Length == 4)
                            {
                                int hour;
                                int min;
                                int sec;
                                int ms;
                                if (int.TryParse(arr[0], out hour) && int.TryParse(arr[1], out min) &&
                                    int.TryParse(arr[2], out sec) && int.TryParse(arr[3], out ms))
                                {
                                    comments.Paragraphs.Add(new Paragraph(new TimeCode(hour, min, sec, ms * 10), new TimeCode(0, 0, 0, 0), line));
                                }
                            }
                        }
                    }
                }
            }

            if (!subtitle.Header.Contains("[Events]"))
            {
                subtitle.Header = subtitle.Header.Trim() + Environment.NewLine +
                                   Environment.NewLine +
                                   "[Events]" + Environment.NewLine +
                                   "Format: Layer, Start, End, Style, Name, MarginL, MarginR, MarginV, Effect, Text" + Environment.NewLine;
            }
            else
            {
                subtitle.Header = subtitle.Header.Remove(subtitle.Header.IndexOf("[Events]", StringComparison.Ordinal));
                subtitle.Header = subtitle.Header.Trim() + Environment.NewLine +
                                   Environment.NewLine +
                                   "[Events]" + Environment.NewLine +
                                   "Format: Layer, Start, End, Style, Name, MarginL, MarginR, MarginV, Effect, Text" + Environment.NewLine;
            }
            lines = new List<string>();
            foreach (string l in subtitle.Header.Trim().Replace(Environment.NewLine, "\n").Split('\n'))
                lines.Add(l);

            const string timeCodeFormat = "{0}:{1:00}:{2:00}.{3:00}"; // h:mm:ss.cc
            foreach (SubtitleSequence mp in sub)
            {
                Paragraph p = new Paragraph(string.Empty, mp.StartMilliseconds, mp.EndMilliseconds);
                string start = string.Format(timeCodeFormat, p.StartTime.Hours, p.StartTime.Minutes, p.StartTime.Seconds, p.StartTime.Milliseconds / 10);
                string end = string.Format(timeCodeFormat, p.EndTime.Hours, p.EndTime.Minutes, p.EndTime.Seconds, p.EndTime.Milliseconds / 10);

                //MKS contains this: ReadOrder, Layer, Style, Name, MarginL, MarginR, MarginV, Effect, Text

                for (int commentIndex = 0; commentIndex < comments.Paragraphs.Count; commentIndex++)
                {
                    var cp = comments.Paragraphs[commentIndex];
                    if (cp.StartTime.TotalMilliseconds <= p.StartTime.TotalMilliseconds)
                        lines.Add(cp.Text);
                }
                for (int commentIndex = comments.Paragraphs.Count - 1; commentIndex >= 0; commentIndex--)
                {
                    var cp = comments.Paragraphs[commentIndex];
                    if (cp.StartTime.TotalMilliseconds <= p.StartTime.TotalMilliseconds)
                        comments.Paragraphs.RemoveAt(commentIndex);
                }

                string text = mp.Text.Replace(Environment.NewLine, "\\N");
                int idx = text.IndexOf(',') + 1;
                if (idx > 0 && idx < text.Length)
                {
                    text = text.Remove(0, idx); // remove ReadOrder
                    idx = text.IndexOf(',');
                    text = text.Insert(idx, "," + start + "," + end);
                    lines.Add("Dialogue: " + text);
                }
            }
            for (int commentIndex = 0; commentIndex < comments.Paragraphs.Count; commentIndex++)
            {
                var cp = comments.Paragraphs[commentIndex];
                lines.Add(cp.Text);
            }

            foreach (string l in footer.ToString().Replace(Environment.NewLine, "\n").Split('\n'))
                lines.Add(l);

            format.LoadSubtitle(subtitle, lines, fileName);
            return subtitle;
        }

        public static void CopyStream(Stream input, Stream output)
        {
            byte[] buffer = new byte[128000];
            int len;
            while ((len = input.Read(buffer, 0, 128000)) > 0)
            {
                output.Write(buffer, 0, len);
            }
            output.Flush();
        }

        private void LoadVobSubFromMatroska(MatroskaSubtitleInfo matroskaSubtitleInfo, string fileName)
        {
            if (matroskaSubtitleInfo.ContentEncodingType == 1)
            {
                MessageBox.Show("Encrypted VobSub content not supported");
            }

            bool isValid;
            var matroska = new Matroska();

            ShowStatus(_language.ParsingMatroskaFile);
            Refresh();
            Cursor.Current = Cursors.WaitCursor;
            List<SubtitleSequence> sub = matroska.GetMatroskaSubtitle(fileName, (int)matroskaSubtitleInfo.TrackNumber, out isValid, MatroskaProgress);
            Cursor.Current = Cursors.Default;

            if (isValid)
            {
                MakeHistoryForUndo(_language.BeforeImportFromMatroskaFile);
                _subtitleListViewIndex = -1;
                _subtitle.Paragraphs.Clear();

                List<VobSubMergedPack> mergedVobSubPacks = new List<VobSubMergedPack>();
                Nikse.SubtitleEdit.Logic.VobSub.Idx idx = new Logic.VobSub.Idx(matroskaSubtitleInfo.CodecPrivate.Split(Utilities.NewLineChars, StringSplitOptions.RemoveEmptyEntries));
                foreach (SubtitleSequence p in sub)
                {
                    if (matroskaSubtitleInfo.ContentEncodingType == 0) // compressed with zlib
                    {
                        bool error = false;
                        MemoryStream outStream = new MemoryStream();
                        var outZStream = new zlib.ZOutputStream(outStream);
                        MemoryStream inStream = new MemoryStream(p.BinaryData);
                        byte[] buffer = null;
                        try
                        {
                            CopyStream(inStream, outZStream);
                            buffer = new byte[outZStream.TotalOut];
                            outStream.Position = 0;
                            outStream.Read(buffer, 0, buffer.Length);
                        }
                        catch (Exception exception)
                        {
                            MessageBox.Show(exception.Message + Environment.NewLine + Environment.NewLine + exception.StackTrace);
                            error = true;
                        }
                        finally
                        {
                            outZStream.Close();
                            inStream.Close();
                        }

                        if (!error)
                            mergedVobSubPacks.Add(new VobSubMergedPack(buffer, TimeSpan.FromMilliseconds(p.StartMilliseconds), 32, null));
                    }
                    else
                    {
                        mergedVobSubPacks.Add(new VobSubMergedPack(p.BinaryData, TimeSpan.FromMilliseconds(p.StartMilliseconds), 32, null));
                    }
                    mergedVobSubPacks[mergedVobSubPacks.Count - 1].EndTime = TimeSpan.FromMilliseconds(p.EndMilliseconds);

                    // fix overlapping (some versions of Handbrake makes overlapping time codes - thx Hawke)
                    if (mergedVobSubPacks.Count > 1 && mergedVobSubPacks[mergedVobSubPacks.Count - 2].EndTime > mergedVobSubPacks[mergedVobSubPacks.Count - 1].StartTime)
                        mergedVobSubPacks[mergedVobSubPacks.Count - 2].EndTime = TimeSpan.FromMilliseconds(mergedVobSubPacks[mergedVobSubPacks.Count - 1].StartTime.TotalMilliseconds - 1);
                }

                var formSubOcr = new VobSubOcr();
                _formPositionsAndSizes.SetPositionAndSize(formSubOcr);
                formSubOcr.Initialize(mergedVobSubPacks, idx.Palette, Configuration.Settings.VobSubOcr, null); //TODO - language???
                if (_loading)
                {
                    formSubOcr.Icon = (Icon)this.Icon.Clone();
                    formSubOcr.ShowInTaskbar = true;
                    formSubOcr.ShowIcon = true;
                }
                if (formSubOcr.ShowDialog(this) == DialogResult.OK)
                {
                    ResetSubtitle();
                    _subtitle.Paragraphs.Clear();
                    _subtitle.WasLoadedWithFrameNumbers = false;
                    foreach (Paragraph p in formSubOcr.SubtitleFromOcr.Paragraphs)
                        _subtitle.Paragraphs.Add(p);

                    ShowSource();
                    SubtitleListview1.Fill(_subtitle, _subtitleAlternate);
                    _subtitleListViewIndex = -1;
                    SubtitleListview1.FirstVisibleIndex = -1;
                    SubtitleListview1.SelectIndexAndEnsureVisible(0);

                    _fileName = Path.GetFileNameWithoutExtension(fileName);
                    _converted = true;
                    Text = Title;

                    Configuration.Settings.Save();
                }
                _formPositionsAndSizes.SavePositionAndSize(formSubOcr);
            }
        }

        private void LoadBluRaySubFromMatroska(MatroskaSubtitleInfo matroskaSubtitleInfo, string fileName)
        {
            if (matroskaSubtitleInfo.ContentEncodingType == 1)
            {
                MessageBox.Show("Encrypted vobsub content not supported");
            }

            bool isValid;
            var matroska = new Matroska();

            ShowStatus(_language.ParsingMatroskaFile);
            Refresh();
            Cursor.Current = Cursors.WaitCursor;
            List<SubtitleSequence> sub = matroska.GetMatroskaSubtitle(fileName, (int)matroskaSubtitleInfo.TrackNumber, out isValid, MatroskaProgress);
            Cursor.Current = Cursors.Default;
            int noOfErrors = 0;
            string lastError = string.Empty;

            if (isValid)
            {
                MakeHistoryForUndo(_language.BeforeImportFromMatroskaFile);
                _subtitleListViewIndex = -1;
                _subtitle.Paragraphs.Clear();
                var subtitles = new List<BluRaySupParser.PcsData>();
                StringBuilder log = new StringBuilder();
                foreach (SubtitleSequence p in sub)
                {
                    byte[] buffer = null;
                    if (matroskaSubtitleInfo.ContentEncodingType == 0) // compressed with zlib
                    {
                        MemoryStream outStream = new MemoryStream();
                        var outZStream = new zlib.ZOutputStream(outStream);
                        MemoryStream inStream = new MemoryStream(p.BinaryData);
                        try
                        {
                            CopyStream(inStream, outZStream);
                            buffer = new byte[outZStream.TotalOut];
                            outStream.Position = 0;
                            outStream.Read(buffer, 0, buffer.Length);
                        }
                        catch (Exception exception)
                        {
                            var tc = new TimeCode(p.StartMilliseconds);
                            lastError = tc + ": " + exception.Message + ": " + exception.StackTrace;
                            noOfErrors++;
                        }
                        finally
                        {
                            outZStream.Close();
                            inStream.Close();
                        }
                    }
                    else
                    {
                        buffer = p.BinaryData;
                    }
                    if (buffer != null && buffer.Length > 100)
                    {
                        MemoryStream ms = new MemoryStream(buffer);
                        var list = BluRaySupParser.ParseBluRaySup(ms, log, true);
                        foreach (var sup in list)
                        {
                            sup.StartTime = (long)((p.StartMilliseconds - 1) * 90.0);
                            sup.EndTime = (long)((p.EndMilliseconds - 1) * 90.0);
                            subtitles.Add(sup);

                            // fix overlapping
                            if (subtitles.Count > 1 && sub[subtitles.Count - 2].EndMilliseconds > sub[subtitles.Count - 1].StartMilliseconds)
                                subtitles[subtitles.Count - 2].EndTime = subtitles[subtitles.Count - 1].StartTime - 1;
                        }
                        ms.Close();
                    }
                    else if (subtitles.Count > 0)
                    {
                        var lastSub = subtitles[subtitles.Count - 1];
                        if (lastSub.StartTime == lastSub.EndTime)
                        {
                            lastSub.EndTime = (long)((p.StartMilliseconds - 1) * 90.0);
                            if (lastSub.EndTime - lastSub.StartTime > 1000000)
                                lastSub.EndTime = lastSub.StartTime;
                        }
                    }
                }

                if (noOfErrors > 0)
                {
                    MessageBox.Show(string.Format("{0} error(s) occured during extraction of bdsup\r\n\r\n{1}", noOfErrors, lastError));
                }

                var formSubOcr = new VobSubOcr();
                _formPositionsAndSizes.SetPositionAndSize(formSubOcr);
                formSubOcr.Initialize(subtitles, Configuration.Settings.VobSubOcr, fileName);
                if (_loading)
                {
                    formSubOcr.Icon = (Icon)Icon.Clone();
                    formSubOcr.ShowInTaskbar = true;
                    formSubOcr.ShowIcon = true;
                }
                if (formSubOcr.ShowDialog(this) == DialogResult.OK)
                {
                    MakeHistoryForUndo(_language.BeforeImportingDvdSubtitle);

                    _subtitle.Paragraphs.Clear();
                    SetCurrentFormat(Configuration.Settings.General.DefaultSubtitleFormat);
                    _subtitle.WasLoadedWithFrameNumbers = false;
                    _subtitle.CalculateFrameNumbersFromTimeCodes(CurrentFrameRate);
                    foreach (Paragraph p in formSubOcr.SubtitleFromOcr.Paragraphs)
                    {
                        _subtitle.Paragraphs.Add(p);
                    }

                    ShowSource();
                    SubtitleListview1.Fill(_subtitle, _subtitleAlternate);
                    _subtitleListViewIndex = -1;
                    SubtitleListview1.FirstVisibleIndex = -1;
                    SubtitleListview1.SelectIndexAndEnsureVisible(0);

                    _fileName = string.Empty;
                    Text = Title;

                    Configuration.Settings.Save();
                }
                _formPositionsAndSizes.SavePositionAndSize(formSubOcr);
            }
        }

        private bool ImportSubtitleFromDvbSupFile(string fileName)
        {
            var subtitles = TransportStreamParser.GetDvbSup(fileName);

            var formSubOcr = new VobSubOcr();
            _formPositionsAndSizes.SetPositionAndSize(formSubOcr);
            formSubOcr.Initialize(subtitles, Configuration.Settings.VobSubOcr, fileName);
            if (formSubOcr.ShowDialog(this) == DialogResult.OK)
            {
                MakeHistoryForUndo(_language.BeforeImportingDvdSubtitle);

                _subtitle.Paragraphs.Clear();
                SetCurrentFormat(Configuration.Settings.General.DefaultSubtitleFormat);
                _subtitle.WasLoadedWithFrameNumbers = false;
                _subtitle.CalculateFrameNumbersFromTimeCodes(CurrentFrameRate);
                foreach (Paragraph p in formSubOcr.SubtitleFromOcr.Paragraphs)
                {
                    _subtitle.Paragraphs.Add(p);
                }

                ShowSource();
                SubtitleListview1.Fill(_subtitle, _subtitleAlternate);
                _subtitleListViewIndex = -1;
                SubtitleListview1.FirstVisibleIndex = -1;
                SubtitleListview1.SelectIndexAndEnsureVisible(0);

                _fileName = string.Empty;
                Text = Title;

                Configuration.Settings.Save();
                _formPositionsAndSizes.SavePositionAndSize(formSubOcr);
                return true;
            }
            _formPositionsAndSizes.SavePositionAndSize(formSubOcr);
            return false;
        }

        private bool ImportSubtitleFromTransportStream(string fileName)
        {
            if (string.IsNullOrEmpty(_language.ParsingTransportStream))
                ShowStatus("Parsing transport stream - please wait...");
            else
                ShowStatus(_language.ParsingTransportStream);
            Refresh();
            var tsParser = new TransportStreamParser();
            tsParser.ParseTSFile(fileName);
            ShowStatus(string.Empty);

            if (tsParser.SubtitlePacketIds.Count == 0)
            {
                MessageBox.Show(_language.NoSubtitlesFound);
                return false;
            }

            int packedId = tsParser.SubtitlePacketIds[0];
            if (tsParser.SubtitlePacketIds.Count > 1)
            {
                var subChooser = new TransportStreamSubtitleChooser();
                _formPositionsAndSizes.SetPositionAndSize(subChooser);
                subChooser.Initialize(tsParser, fileName);
                if (subChooser.ShowDialog(this) == DialogResult.Cancel)
                    return false;
                packedId = tsParser.SubtitlePacketIds[subChooser.SelectedIndex];
                _formPositionsAndSizes.SavePositionAndSize(subChooser);
            }
            var subtitles = tsParser.GetDvbSubtitles(packedId);

            var formSubOcr = new VobSubOcr();
            _formPositionsAndSizes.SetPositionAndSize(formSubOcr);
            formSubOcr.Initialize(subtitles, Configuration.Settings.VobSubOcr, fileName);
            if (formSubOcr.ShowDialog(this) == DialogResult.OK)
            {
                MakeHistoryForUndo(_language.BeforeImportingDvdSubtitle);

                _subtitle.Paragraphs.Clear();
                SetCurrentFormat(Configuration.Settings.General.DefaultSubtitleFormat);
                _subtitle.WasLoadedWithFrameNumbers = false;
                _subtitle.CalculateFrameNumbersFromTimeCodes(CurrentFrameRate);
                foreach (Paragraph p in formSubOcr.SubtitleFromOcr.Paragraphs)
                {
                    _subtitle.Paragraphs.Add(p);
                }

                ShowSource();
                SubtitleListview1.Fill(_subtitle, _subtitleAlternate);
                _subtitleListViewIndex = -1;
                SubtitleListview1.FirstVisibleIndex = -1;
                SubtitleListview1.SelectIndexAndEnsureVisible(0);

                _fileName = string.Empty;
                Text = Title;

                Configuration.Settings.Save();
                _formPositionsAndSizes.SavePositionAndSize(formSubOcr);
                return true;
            }
            _formPositionsAndSizes.SavePositionAndSize(formSubOcr);
            return false;
        }

        private readonly static String[] colors = {
        "{\\c&HC0C0C0&}",   // black /gray
        "{\\c&H4040FF&}",   // red
        "{\\c&H00FF00&}",   // green
        "{\\c&H00FFFF&}",   // yellow
        "{\\c&HFF409B&}",   // blue //DM15032004 081.6 int18 changed
        "{\\c&HFF00FF&}",   // magenta
        "{\\c&HFFFF00&}",   // cyan
        "{\\c&HFFFFFF&}",   // white
        };

        public static byte ByteReverse(byte n)
        {
            n = (byte)(((n >> 1) & 0x55) | ((n << 1) & 0xaa));
            n = (byte)(((n >> 2) & 0x33) | ((n << 2) & 0xcc));
            n = (byte)(((n >> 4) & 0x0f) | ((n << 4) & 0xf0));
            return n;
        }

        //private static string GetTeletext(byte[] _buffer, int offset)
        //{
        //    string text = string.Empty;
        //    bool ascii = false;
        //    const int color = 0;
        //    bool toggle = false;
        //    for (int c = offset, i = 0; c < _buffer.Length; c++, i++)
        //    {
        //        //var char_value = _buffer[c];

        //        var char_value = 0x7F & ByteReverse(_buffer[c]);

        //        if (char_value >> 3 == 0)  //0x0..7
        //        {
        //            ascii = true;
        //            text += ((color == 1) ? colors[char_value] : "");// + (char)active_set[32];
        //        }
        //        else if (char_value >> 4 == 0)   //0x8..F
        //        {
        //            text += " ";//(char)active_set[32];
        //        }
        //        else if (char_value >> 7 == 1)  //0x80..FF
        //        {
        //            text += " "; //(char)active_set[32];
        //        }
        //        else if (char_value < 27)  //0x10..1A
        //        {
        //            ascii = false;
        //            text += " "; //(char)active_set[32];
        //        }
        //        else if (char_value < 32) //0x1B..1F
        //        {
        //            if (char_value == 0x1B) //ESC
        //            {
        //                if (toggle)
        //                {
        //                    //  active_set = CharSet.getActive_G0_Set(primary_set_mapping, primary_national_set_mapping, row);
        //                    //  active_national_set = CharSet.getActiveNationalSubset(primary_set_mapping, primary_national_set_mapping, row);
        //                }
        //                else
        //                {
        //                    //active_set = CharSet.getActive_G0_Set(secondary_set_mapping, secondary_national_set_mapping, row);
        //                    //active_national_set = CharSet.getActiveNationalSubset(secondary_set_mapping, secondary_national_set_mapping, row);
        //                }
        //                toggle = !toggle;
        //            }

        //            text += " "; //(char)active_set[32];
        //            continue;
        //        }
        //        else if (char_value == 0x7F) //0x7F
        //        {
        //            text += " "; // (char)active_set[32];
        //            continue;
        //        }

        //        if (!ascii)
        //        {
        //            text += " "; // (char)active_set[32];
        //            continue;
        //        }

        //        if (false) //active_national_set != null)
        //        {
        //            // all chars 0x20..7F
        //            //    switch (char_value)  // special national characters
        //            //    {
        //            //    case 0x23:
        //            //        text += (char)active_national_set[0];
        //            //        continue loopi;
        //            //    case 0x24:
        //            //        text += (char)active_national_set[1];
        //            //        continue loopi;
        //            //    case 0x40:
        //            //        text += (char)active_national_set[2];
        //            //        continue loopi;
        //            //    case 0x5b:
        //            //        text += (char)active_national_set[3];
        //            //        continue loopi;
        //            //    case 0x5c:
        //            //        text += (char)active_national_set[4];
        //            //        continue loopi;
        //            //    case 0x5d:
        //            //        text += (char)active_national_set[5];
        //            //        continue loopi;
        //            //    case 0x5e:
        //            //        text += (char)active_national_set[6];
        //            //        continue loopi;
        //            //    case 0x5f:
        //            //        text += (char)active_national_set[7];
        //            //        continue loopi;
        //            //    case 0x60:
        //            //        text += (char)active_national_set[8];
        //            //        continue loopi;
        //            //    case 0x7b:
        //            //        text += (char)active_national_set[9];
        //            //        continue loopi;
        //            //    case 0x7c:
        //            //        text += (char)active_national_set[10];
        //            //        continue loopi;
        //            //    case 0x7d:
        //            //        text += (char)active_national_set[11];
        //            //        continue loopi;
        //            //    case 0x7e:
        //            //        text += (char)active_national_set[12];
        //            //        continue loopi;
        //            //    }
        //        }

        //        text += Encoding.Default.GetString(new byte[] { (byte)char_value });  //(char)active_set[char_value];
        //        //continue loopi;
        //    }

        //    if (color == 1)
        //        return colors[7] + text.Trim();
        //    else
        //        return text;
        //}

        private bool ImportSubtitleFromMp4(string fileName)
        {
            var mp4Parser = new Logic.Mp4.MP4Parser(fileName);
            var mp4SubtitleTracks = mp4Parser.GetSubtitleTracks();
            if (mp4SubtitleTracks.Count == 0)
            {
                MessageBox.Show(_language.NoSubtitlesFound);
                return false;
            }
            else if (mp4SubtitleTracks.Count == 1)
            {
                LoadMp4Subtitle(fileName, mp4SubtitleTracks[0]);
                return true;
            }
            else
            {
                var subtitleChooser = new MatroskaSubtitleChooser();
                subtitleChooser.Initialize(mp4SubtitleTracks);
                if (subtitleChooser.ShowDialog(this) == DialogResult.OK)
                {
                    LoadMp4Subtitle(fileName, mp4SubtitleTracks[subtitleChooser.SelectedIndex]);
                    return true;
                }
                return false;
            }
        }

        private bool ImportSubtitleFromDivX(string fileName)
        {
            var count = 0;
            var list = new List<XSub>();
            using (var f = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                var searchBuffer = new byte[2048];
                long pos = 0;
                long length = f.Length - 50;
                while (pos < length)
                {
                    f.Position = pos;
                    int readCount = f.Read(searchBuffer, 0, searchBuffer.Length);
                    for (int i = 0; i < readCount; i++)
                    {
                        if (searchBuffer[i] != 0x5b || (i + 4 < readCount && (searchBuffer[i + 1] < 0x30 || searchBuffer[i + 1] > 0x39 || searchBuffer[i + 3] != 0x3a)))
                        {
                            continue;
                        }

                        f.Position = pos + i + 1;

                        byte[] buffer = new byte[26];
                        f.Read(buffer, 0, 26);

                        if (buffer[2] == 0x3a && // :
                            buffer[5] == 0x3a && // :
                            buffer[8] == 0x2e && // .
                            buffer[12] == 0x2d && // -
                            buffer[15] == 0x3a && // :
                            buffer[18] == 0x3a && // :
                            buffer[21] == 0x2e && // .
                            buffer[25] == 0x5d) // ]
                        { // subtitle time code
                            string timeCode = Encoding.ASCII.GetString(buffer, 0, 25);

                            f.Read(buffer, 0, 2);
                            int width = (int)BitConverter.ToUInt16(buffer, 0);
                            f.Read(buffer, 0, 2);
                            int height = (int)BitConverter.ToUInt16(buffer, 0);
                            f.Read(buffer, 0, 2);
                            int x = (int)BitConverter.ToUInt16(buffer, 0);
                            f.Read(buffer, 0, 2);
                            int y = (int)BitConverter.ToUInt16(buffer, 0);
                            f.Read(buffer, 0, 2);
                            int xEnd = (int)BitConverter.ToUInt16(buffer, 0);
                            f.Read(buffer, 0, 2);
                            int yEnd = (int)BitConverter.ToUInt16(buffer, 0);
                            f.Read(buffer, 0, 2);
                            int RleLength = (int)BitConverter.ToUInt16(buffer, 0);

                            byte[] colorBuffer = new byte[4 * 3]; // four colors with rgb (3 bytes)
                            f.Read(colorBuffer, 0, colorBuffer.Length);

                            buffer = new byte[RleLength];
                            int bytesRead = f.Read(buffer, 0, buffer.Length);

                            if (width > 0 && height > 0 && bytesRead == buffer.Length)
                            {
                                var xSub = new XSub(timeCode, width, height, colorBuffer, buffer);
                                list.Add(xSub);
                                count++;
                            }
                        }
                    }
                    pos += searchBuffer.Length;
                }
            }

            if (count > 0)
            {
                var formSubOcr = new VobSubOcr();
                _formPositionsAndSizes.SetPositionAndSize(formSubOcr);
                formSubOcr.Initialize(list, Configuration.Settings.VobSubOcr, fileName); //TODO - language???
                if (formSubOcr.ShowDialog(this) == DialogResult.OK)
                {
                    MakeHistoryForUndo(_language.BeforeImportFromMatroskaFile);
                    _subtitleListViewIndex = -1;
                    FileNew();
                    _subtitle.WasLoadedWithFrameNumbers = false;
                    foreach (Paragraph p in formSubOcr.SubtitleFromOcr.Paragraphs)
                        _subtitle.Paragraphs.Add(p);

                    ShowSource();
                    SubtitleListview1.Fill(_subtitle, _subtitleAlternate);
                    _subtitleListViewIndex = -1;
                    SubtitleListview1.FirstVisibleIndex = -1;
                    SubtitleListview1.SelectIndexAndEnsureVisible(0);

                    _fileName = Path.GetFileNameWithoutExtension(fileName);
                    _converted = true;
                    Text = Title;

                    Configuration.Settings.Save();
                    OpenVideo(fileName);
                }
                _formPositionsAndSizes.SavePositionAndSize(formSubOcr);
            }

            return count > 0;
        }

        private static Subtitle LoadMp4SubtitleForSync(Logic.Mp4.Boxes.Trak mp4SubtitleTrack)
        {
            var subtitle = new Subtitle();
            if (mp4SubtitleTrack.Mdia.IsVobSubSubtitle)
            {
                return subtitle;
            }
            else
            {
                for (int i = 0; i < mp4SubtitleTrack.Mdia.Minf.Stbl.EndTimeCodes.Count; i++)
                {
                    if (mp4SubtitleTrack.Mdia.Minf.Stbl.Texts.Count > i)
                    {
                        var start = TimeSpan.FromSeconds(mp4SubtitleTrack.Mdia.Minf.Stbl.StartTimeCodes[i]);
                        var end = TimeSpan.FromSeconds(mp4SubtitleTrack.Mdia.Minf.Stbl.EndTimeCodes[i]);
                        string text = mp4SubtitleTrack.Mdia.Minf.Stbl.Texts[i];
                        Paragraph p = new Paragraph(text, start.TotalMilliseconds, end.TotalMilliseconds);
                        if (p.EndTime.TotalMilliseconds - p.StartTime.TotalMilliseconds > Configuration.Settings.General.SubtitleMaximumDisplayMilliseconds)
                            p.EndTime.TotalMilliseconds = p.StartTime.TotalMilliseconds + Configuration.Settings.General.SubtitleMaximumDisplayMilliseconds;

                        if (mp4SubtitleTrack.Mdia.IsClosedCaption && string.IsNullOrEmpty(text))
                        {
                            // do not add empty lines
                        }
                        else
                        {
                            subtitle.Paragraphs.Add(p);
                        }
                    }
                }
            }
            return subtitle;
        }

        private void LoadMp4Subtitle(string fileName, Logic.Mp4.Boxes.Trak mp4SubtitleTrack)
        {
            if (mp4SubtitleTrack.Mdia.IsVobSubSubtitle)
            {
                var subPicturesWithTimeCodes = new List<VobSubOcr.SubPicturesWithSeparateTimeCodes>();
                for (int i = 0; i < mp4SubtitleTrack.Mdia.Minf.Stbl.EndTimeCodes.Count; i++)
                {
                    if (mp4SubtitleTrack.Mdia.Minf.Stbl.SubPictures.Count > i)
                    {
                        var start = TimeSpan.FromSeconds(mp4SubtitleTrack.Mdia.Minf.Stbl.StartTimeCodes[i]);
                        var end = TimeSpan.FromSeconds(mp4SubtitleTrack.Mdia.Minf.Stbl.EndTimeCodes[i]);
                        subPicturesWithTimeCodes.Add(new VobSubOcr.SubPicturesWithSeparateTimeCodes(mp4SubtitleTrack.Mdia.Minf.Stbl.SubPictures[i], start, end));
                    }
                }

                var formSubOcr = new VobSubOcr();
                _formPositionsAndSizes.SetPositionAndSize(formSubOcr);
                formSubOcr.Initialize(subPicturesWithTimeCodes, Configuration.Settings.VobSubOcr, fileName); //TODO - language???
                if (formSubOcr.ShowDialog(this) == DialogResult.OK)
                {
                    MakeHistoryForUndo(_language.BeforeImportFromMatroskaFile);
                    _subtitleListViewIndex = -1;
                    FileNew();
                    _subtitle.WasLoadedWithFrameNumbers = false;
                    foreach (Paragraph p in formSubOcr.SubtitleFromOcr.Paragraphs)
                        _subtitle.Paragraphs.Add(p);

                    ShowSource();
                    SubtitleListview1.Fill(_subtitle, _subtitleAlternate);
                    _subtitleListViewIndex = -1;
                    SubtitleListview1.FirstVisibleIndex = -1;
                    SubtitleListview1.SelectIndexAndEnsureVisible(0);

                    _fileName = Path.GetFileNameWithoutExtension(fileName);
                    _converted = true;
                    Text = Title;

                    Configuration.Settings.Save();
                }
                _formPositionsAndSizes.SavePositionAndSize(formSubOcr);
            }
            else
            {
                MakeHistoryForUndo(_language.BeforeImportFromMatroskaFile);
                _subtitleListViewIndex = -1;
                FileNew();

                for (int i = 0; i < mp4SubtitleTrack.Mdia.Minf.Stbl.EndTimeCodes.Count; i++)
                {
                    if (mp4SubtitleTrack.Mdia.Minf.Stbl.Texts.Count > i)
                    {
                        var start = TimeSpan.FromSeconds(mp4SubtitleTrack.Mdia.Minf.Stbl.StartTimeCodes[i]);
                        var end = TimeSpan.FromSeconds(mp4SubtitleTrack.Mdia.Minf.Stbl.EndTimeCodes[i]);
                        string text = mp4SubtitleTrack.Mdia.Minf.Stbl.Texts[i];
                        Paragraph p = new Paragraph(text, start.TotalMilliseconds, end.TotalMilliseconds);
                        if (p.EndTime.TotalMilliseconds - p.StartTime.TotalMilliseconds > Configuration.Settings.General.SubtitleMaximumDisplayMilliseconds)
                            p.EndTime.TotalMilliseconds = p.StartTime.TotalMilliseconds + Configuration.Settings.General.SubtitleMaximumDisplayMilliseconds;

                        if (mp4SubtitleTrack.Mdia.IsClosedCaption && string.IsNullOrEmpty(text))
                        {
                            // do not add empty lines
                        }
                        else
                        {
                            _subtitle.Paragraphs.Add(p);
                        }
                    }
                }

                SetEncoding(Encoding.UTF8);
                ShowStatus(_language.SubtitleImportedFromMatroskaFile);
                _subtitle.Renumber(1);
                _subtitle.WasLoadedWithFrameNumbers = false;
                if (fileName.EndsWith(".mp4", StringComparison.OrdinalIgnoreCase) || fileName.EndsWith(".m4v", StringComparison.OrdinalIgnoreCase))
                {
                    _fileName = fileName.Substring(0, fileName.Length - 4);
                    Text = Title + " - " + _fileName;
                }
                else
                {
                    Text = Title;
                }
                _fileDateTime = new DateTime();

                _converted = true;

                SubtitleListview1.Fill(_subtitle, _subtitleAlternate);
                if (_subtitle.Paragraphs.Count > 0)
                    SubtitleListview1.SelectIndexAndEnsureVisible(0);
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
            _dragAndDropFiles = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (_dragAndDropFiles.Length == 1)
            {
                _dragAndDropTimer.Start();
            }
            else
            {
                MessageBox.Show(_language.DropOnlyOneFile);
            }
        }

        private void DoSubtitleListview1Drop(object sender, EventArgs e)
        {
            _dragAndDropTimer.Stop();

            if (ContinueNewOrExit())
            {
                string fileName = _dragAndDropFiles[0];

                saveFileDialog1.InitialDirectory = Path.GetDirectoryName(fileName);
                openFileDialog1.InitialDirectory = Path.GetDirectoryName(fileName);

                var fi = new FileInfo(fileName);
                string ext = Path.GetExtension(fileName).ToLower();

                if (ext == ".mkv")
                {
                    bool isValid;
                    var matroska = new Matroska();
                    var subtitleList = matroska.GetMatroskaSubtitleTracks(fileName, out isValid);
                    if (isValid)
                    {
                        if (subtitleList.Count == 0)
                        {
                            MessageBox.Show(_language.NoSubtitlesFound);
                        }
                        else if (subtitleList.Count > 1)
                        {
                            MatroskaSubtitleChooser subtitleChooser = new MatroskaSubtitleChooser();
                            subtitleChooser.Initialize(subtitleList);
                            if (subtitleChooser.ShowDialog(this) == DialogResult.OK)
                            {
                                LoadMatroskaSubtitle(subtitleList[subtitleChooser.SelectedIndex], fileName, false);
                            }
                        }
                        else
                        {
                            LoadMatroskaSubtitle(subtitleList[0], fileName, false);
                        }
                    }
                    return;
                }

                if (fi.Length < 1024 * 1024 * 2) // max 2 mb
                {
                    OpenSubtitle(fileName, null);
                }
                else if (fi.Length < 150000000 && ext == ".sub" && IsVobSubFile(fileName, true)) // max 150 mb
                {
                    OpenSubtitle(fileName, null);
                }
                else if (fi.Length < 250000000 && ext == ".sup" && FileUtils.IsBluRaySup(fileName)) // max 250 mb
                {
                    OpenSubtitle(fileName, null);
                }
                else if ((ext == ".ts" || ext == ".rec" || ext == ".mpg" || ext == ".mpeg") && FileUtils.IsTransportStream(fileName))
                {
                    OpenSubtitle(fileName, null);
                }
                else if (ext == ".m2ts" && FileUtils.IsM2TransportStream(fileName))
                {
                    OpenSubtitle(fileName, null);
                }
                else
                {
                    MessageBox.Show(string.Format(_language.DropFileXNotAccepted, fileName));
                }
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
            openFileDialog1.Filter = Utilities.GetOpenDialogFilter();
            if (openFileDialog1.ShowDialog(this) == DialogResult.OK)
            {
                var chooseEncoding = new ChooseEncoding();
                chooseEncoding.Initialize(openFileDialog1.FileName);
                if (chooseEncoding.ShowDialog(this) == DialogResult.OK)
                {
                    Encoding encoding = chooseEncoding.GetEncoding();
                    SetEncoding(Encoding.UTF8);
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
            if (IsSubtitleLoaded)
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
                    changeCasing.FixCasing(selectedLines, Utilities.AutoDetectLanguageName(Configuration.Settings.General.SpellCheckLanguage, _subtitle));
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
                            _subtitleListViewIndex = -1;
                            RestoreSubtitleListviewIndexes();
                            UpdateSourceView();
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

        private void ToolStripMenuItemChangeFrameRateClick(object sender, EventArgs e)
        {
            if (IsSubtitleLoaded)
            {
                int lastSelectedIndex = 0;
                if (SubtitleListview1.SelectedItems.Count > 0)
                    lastSelectedIndex = SubtitleListview1.SelectedItems[0].Index;

                ReloadFromSourceView();
                var changeFrameRate = new ChangeFrameRate();
                _formPositionsAndSizes.SetPositionAndSize(changeFrameRate);
                changeFrameRate.Initialize(CurrentFrameRate.ToString());
                if (changeFrameRate.ShowDialog(this) == DialogResult.OK)
                {
                    MakeHistoryForUndo(_language.BeforeChangeFrameRate);

                    double oldFrameRate = changeFrameRate.OldFrameRate;
                    double newFrameRate = changeFrameRate.NewFrameRate;
                    _subtitle.ChangeFrameRate(oldFrameRate, newFrameRate);

                    ShowStatus(string.Format(_language.FrameRateChangedFromXToY, oldFrameRate, newFrameRate));
                    toolStripComboBoxFrameRate.Text = newFrameRate.ToString();

                    ShowSource();
                    SubtitleListview1.Fill(_subtitle, _subtitleAlternate);
                    _subtitleListViewIndex = -1;
                    SubtitleListview1.SelectIndexAndEnsureVisible(lastSelectedIndex);
                }
                _formPositionsAndSizes.SavePositionAndSize(changeFrameRate);
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
                bool isHeaderOk = FileUtils.IsVobSub(subFileName);
                if (isHeaderOk)
                {
                    if (!verbose)
                        return true;

                    string idxFileName = Path.Combine(Path.GetDirectoryName(subFileName), Path.GetFileNameWithoutExtension(subFileName) + ".idx");
                    if (File.Exists(idxFileName))
                        return true;
                    return (MessageBox.Show(string.Format(_language.IdxFileNotFoundWarning, idxFileName), _title, MessageBoxButtons.YesNo) == DialogResult.Yes);
                }
                if (verbose)
                    MessageBox.Show(string.Format(_language.InvalidVobSubHeader, subFileName));
            }
            catch (Exception ex)
            {
                if (verbose)
                    MessageBox.Show(ex.Message);
            }
            return false;
        }

        private void ImportAndOcrSpDvdSup(string fileName, bool showInTaskbar)
        {
            var spList = new List<SpHeader>();

            using (var fs = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                byte[] buffer = new byte[SpHeader.SpHeaderLength];
                int bytesRead = fs.Read(buffer, 0, buffer.Length);
                var header = new SpHeader(buffer);

                while (header.Identifier == "SP" && bytesRead > 0 && header.NextBlockPosition > 4)
                {
                    buffer = new byte[header.NextBlockPosition];
                    bytesRead = fs.Read(buffer, 0, buffer.Length);
                    if (bytesRead == buffer.Length)
                    {
                        header.AddPicture(buffer);
                        spList.Add(header);
                    }

                    buffer = new byte[SpHeader.SpHeaderLength];
                    bytesRead = fs.Read(buffer, 0, buffer.Length);
                    header = new SpHeader(buffer);
                }
            }

            var vobSubOcr = new VobSubOcr();
            if (showInTaskbar)
            {
                vobSubOcr.Icon = (Icon)this.Icon.Clone();
                vobSubOcr.ShowInTaskbar = true;
                vobSubOcr.ShowIcon = true;
            }
            _formPositionsAndSizes.SetPositionAndSize(vobSubOcr);
            vobSubOcr.Initialize(fileName, null, Configuration.Settings.VobSubOcr, spList);
            if (vobSubOcr.ShowDialog(this) == DialogResult.OK)
            {
                MakeHistoryForUndo(_language.BeforeImportingVobSubFile);
                FileNew();
                _subtitle.Paragraphs.Clear();
                SetCurrentFormat(Configuration.Settings.General.DefaultSubtitleFormat);
                _subtitle.WasLoadedWithFrameNumbers = false;
                _subtitle.CalculateFrameNumbersFromTimeCodes(CurrentFrameRate);
                foreach (Paragraph p in vobSubOcr.SubtitleFromOcr.Paragraphs)
                {
                    _subtitle.Paragraphs.Add(p);
                }

                ShowSource();
                SubtitleListview1.Fill(_subtitle, _subtitleAlternate);
                _subtitleListViewIndex = -1;
                SubtitleListview1.FirstVisibleIndex = -1;
                SubtitleListview1.SelectIndexAndEnsureVisible(0);

                _fileName = Path.ChangeExtension(vobSubOcr.FileName, ".srt");
                SetTitle();
                _converted = true;

                Configuration.Settings.Save();
            }
            _formPositionsAndSizes.SavePositionAndSize(vobSubOcr);
        }

        private void ImportAndOcrVobSubSubtitleNew(string fileName, bool showInTaskbar)
        {
            if (IsVobSubFile(fileName, true))
            {
                var vobSubOcr = new VobSubOcr();
                if (showInTaskbar)
                {
                    vobSubOcr.Icon = (Icon)this.Icon.Clone();
                    vobSubOcr.ShowInTaskbar = true;
                    vobSubOcr.ShowIcon = true;
                }
                _formPositionsAndSizes.SetPositionAndSize(vobSubOcr);
                if (vobSubOcr.Initialize(fileName, Configuration.Settings.VobSubOcr, this))
                {
                    if (vobSubOcr.ShowDialog(this) == DialogResult.OK)
                    {
                        MakeHistoryForUndo(_language.BeforeImportingVobSubFile);
                        FileNew();
                        _subtitle.Paragraphs.Clear();
                        SetCurrentFormat(Configuration.Settings.General.DefaultSubtitleFormat);
                        _subtitle.WasLoadedWithFrameNumbers = false;
                        _subtitle.CalculateFrameNumbersFromTimeCodes(CurrentFrameRate);
                        foreach (Paragraph p in vobSubOcr.SubtitleFromOcr.Paragraphs)
                        {
                            _subtitle.Paragraphs.Add(p);
                        }

                        ShowSource();
                        SubtitleListview1.Fill(_subtitle, _subtitleAlternate);
                        _subtitleListViewIndex = -1;
                        SubtitleListview1.FirstVisibleIndex = -1;
                        SubtitleListview1.SelectIndexAndEnsureVisible(0);

                        _fileName = Path.ChangeExtension(vobSubOcr.FileName, ".srt");
                        SetTitle();
                        _converted = true;

                        Configuration.Settings.Save();
                    }
                }
                _formPositionsAndSizes.SavePositionAndSize(vobSubOcr);
            }
        }

        private void ToolStripMenuItemMergeLinesClick(object sender, EventArgs e)
        {
            if (_subtitle.Paragraphs.Count > 0 && SubtitleListview1.SelectedItems.Count >= 1)
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
            if (IsSubtitleLoaded)
            {
                if (_showEarlierOrLater != null && !_showEarlierOrLater.IsDisposed)
                {
                    _showEarlierOrLater.WindowState = FormWindowState.Normal;
                    _showEarlierOrLater.Focus();
                    return;
                }

                bool waveformEnabled = timerWaveform.Enabled;
                timerWaveform.Stop();
                timer1.Stop();

                _showEarlierOrLater = new ShowEarlierLater();
                if (!_formPositionsAndSizes.SetPositionAndSize(_showEarlierOrLater))
                {
                    _showEarlierOrLater.Top = this.Top + 100;
                    _showEarlierOrLater.Left = this.Left + (this.Width / 2) - (_showEarlierOrLater.Width / 3);
                }
                _showEarlierOrLater.Initialize(ShowEarlierOrLater, _formPositionsAndSizes, true);
                MakeHistoryForUndo(_language.BeforeShowSelectedLinesEarlierLater);
                _showEarlierOrLater.Show(this);

                timerWaveform.Enabled = waveformEnabled;
                timer1.Start();

                RefreshSelectedParagraph();
            }
            else
            {
                MessageBox.Show(_language.NoSubtitleLoaded, Title, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        public static Control FindFocusedControl(Control control)
        {
            var container = control as ContainerControl;
            while (container != null)
            {
                control = container.ActiveControl;
                container = control as ContainerControl;
            }
            return control;
        }

        private static string SerializeSubtitle(Subtitle subtitle)
        {
            var sb = new StringBuilder();
            for (int i = 0; i < subtitle.Paragraphs.Count; i++)
            {
                Paragraph p = subtitle.Paragraphs[i];
                sb.Append(p.StartTime.TotalMilliseconds + (p.EndTime.TotalMilliseconds * 127));
                sb.Append(p.Text);
            }
            return sb.ToString().TrimEnd();
        }

        internal void MainKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Modifiers == Keys.Alt && e.KeyCode == (Keys.RButton | Keys.ShiftKey) && textBoxListViewText.Focused)
            { // annoying that focus leaves textbox while typing, when pressing Alt alone
                e.SuppressKeyPress = true;
                return;
            }

            if (e.Modifiers == Keys.Alt && e.KeyCode == (Keys.RButton | Keys.ShiftKey))
                return;

            if (e.Modifiers == Keys.Shift && e.KeyCode == Keys.ShiftKey)
                return;
            if (e.Modifiers == Keys.Control && e.KeyCode == (Keys.LButton | Keys.ShiftKey))
                return;
            if (e.Modifiers == (Keys.Control | Keys.Shift) && e.KeyCode == Keys.ShiftKey)
                return;

            var fc = FindFocusedControl(this);
            if (fc != null && e.Modifiers != Keys.Control && e.Modifiers != (Keys.Control | Keys.Shift) && e.Modifiers != (Keys.Control | Keys.Shift | Keys.Alt))
            {
                // do not check for shortcuts if text is being entered and a textbox is focused
                if ((fc.Name == textBoxListViewText.Name || fc.Name == textBoxListViewTextAlternate.Name || fc.Name == textBoxSearchWord.Name) && e.KeyCode >= Keys.A && e.KeyCode <= Keys.Z)
                    return;

                // do not check for shortcuts if a number is being entered and a time box is focused
                if (fc.Parent != null && (fc.Parent.Name == timeUpDownStartTime.Name || fc.Parent.Name == numericUpDownDuration.Name) &&
                    (e.KeyCode >= Keys.NumPad0 && e.KeyCode <= Keys.NumPad9 || e.KeyValue >= 48 && e.KeyValue <= 57))
                    return;
            }

            bool inListView = tabControlSubtitle.SelectedIndex == TabControlListView;

            if (e.KeyCode == Keys.Escape && !_cancelWordSpellCheck)
            {
                _cancelWordSpellCheck = true;
            }
            else if (inListView && (Keys.Shift | Keys.Control) == e.Modifiers && e.KeyCode == Keys.B)
            {
                AutoBalanceLinesAndAllow2PlusLines();
                e.SuppressKeyPress = true;
            }
            else if (audioVisualizer != null && audioVisualizer.Visible & e.KeyData == _waveformVerticalZoom)
            {
                if (audioVisualizer.VerticalZoomPercent >= 0.1)
                    audioVisualizer.VerticalZoomPercent -= 0.05;
                e.SuppressKeyPress = true;
            }
            else if (audioVisualizer != null && audioVisualizer.Visible & e.KeyData == _waveformVerticalZoomOut)
            {
                if (audioVisualizer.VerticalZoomPercent < 1)
                    audioVisualizer.VerticalZoomPercent += 0.05;
                e.SuppressKeyPress = true;
            }
            if (audioVisualizer != null && audioVisualizer.Visible & e.KeyData == _waveformZoomIn)
            {
                audioVisualizer.ZoomIn();
                e.SuppressKeyPress = true;
            }
            if (audioVisualizer != null && audioVisualizer.Visible & e.KeyData == _waveformZoomOut)
            {
                audioVisualizer.ZoomOut();
                e.SuppressKeyPress = true;
            }
            else if (e.KeyData == _videoPlayFirstSelected && !string.IsNullOrEmpty(_videoFileName))
            {
                PlayFirstSelectedSubtitle();
            }
            else if (audioVisualizer != null && audioVisualizer.Visible & e.KeyData == _waveformPlaySelection)
            {
                toolStripMenuItemWaveformPlaySelection_Click(null, null);
                e.SuppressKeyPress = true;
            }
            else if (audioVisualizer != null && audioVisualizer.Visible & e.KeyData == _waveformSearchSilenceForward)
            {
                audioVisualizer.FindDataBelowThreshold(Configuration.Settings.VideoControls.WaveformSeeksSilenceMaxVolume, Configuration.Settings.VideoControls.WaveformSeeksSilenceDurationSeconds);
                e.SuppressKeyPress = true;
            }
            else if (audioVisualizer != null && audioVisualizer.Visible & e.KeyData == _waveformSearchSilenceBack)
            {
                audioVisualizer.FindDataBelowThresholdBack(Configuration.Settings.VideoControls.WaveformSeeksSilenceMaxVolume, Configuration.Settings.VideoControls.WaveformSeeksSilenceDurationSeconds);
                e.SuppressKeyPress = true;
            }
            else if (_mainInsertBefore == e.KeyData && inListView)
            {
                InsertBefore();
                e.SuppressKeyPress = true;
                textBoxListViewText.Focus();
            }
            else if (_mainMergeDialog == e.KeyData && inListView)
            {
                MergeDialogs();
                e.SuppressKeyPress = true;
            }
            else if (_mainListViewToggleDashes == e.KeyData && inListView)
            {
                ToggleDashes();
                e.SuppressKeyPress = true;
            }
            else if (!toolStripMenuItemReverseRightToLeftStartEnd.Visible && _mainEditReverseStartAndEndingForRTL == e.KeyData && inListView)
            {
                ReverseStartAndEndingForRTL();
                e.SuppressKeyPress = true;
            }
            else if (toolStripMenuItemUndo.ShortcutKeys == e.KeyData) // undo
            {
                UndoLastAction();
                e.SuppressKeyPress = true;
            }
            else if (toolStripMenuItemRedo.ShortcutKeys == e.KeyData) // redo
            {
                RedoLastAction();
                e.SuppressKeyPress = true;
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
            else if (_mainGeneralGoToNextSubtitle == e.KeyData)
            {
                if (AutoRepeatContinueOn)
                    Next();
                else
                    ButtonNextClick(null, null);
                e.SuppressKeyPress = true;
            }
            else if (_mainGeneralGoToPrevSubtitle == e.KeyData)
            {
                if (AutoRepeatContinueOn)
                    PlayPrevious();
                else
                    ButtonPreviousClick(null, null);
                e.SuppressKeyPress = true;
            }
            else if (_mainGeneralGoToStartOfCurrentSubtitle == e.KeyData)
            {
                if (SubtitleListview1.SelectedItems.Count == 1 && mediaPlayer.VideoPlayer != null)
                {
                    mediaPlayer.CurrentPosition = _subtitle.Paragraphs[SubtitleListview1.SelectedItems[0].Index].StartTime.TotalSeconds;
                    e.SuppressKeyPress = true;
                }
                e.SuppressKeyPress = true;
            }
            else if (_mainGeneralGoToEndOfCurrentSubtitle == e.KeyData)
            {
                if (SubtitleListview1.SelectedItems.Count == 1 && mediaPlayer.VideoPlayer != null)
                {
                    mediaPlayer.CurrentPosition = _subtitle.Paragraphs[SubtitleListview1.SelectedItems[0].Index].EndTime.TotalSeconds;
                    e.SuppressKeyPress = true;
                }
                e.SuppressKeyPress = true;
            }
            else if (_mainGeneralFileSaveAll == e.KeyData)
            {
                ToolStripButtonSaveClick(sender, e);
                e.SuppressKeyPress = true;
            }
            else if (_mainToggleFocus == e.KeyData && inListView)
            {
                if (SubtitleListview1.Focused)
                    textBoxListViewText.Focus();
                else
                    SubtitleListview1.Focus();
                e.SuppressKeyPress = true;
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
            else if (_mainGeneralGoToFirstSelectedLine == e.KeyData) //Locate first selected line in subtitle listview
            {
                if (SubtitleListview1.SelectedItems.Count > 0)
                    SubtitleListview1.SelectedItems[0].EnsureVisible();
                e.SuppressKeyPress = true;
            }
            else if (_mainGeneralGoToFirstEmptyLine == e.KeyData) //Go to first empty line - if any
            {
                GoToFirstEmptyLine();
                e.SuppressKeyPress = true;
            }
            else if (_mainGeneralMergeSelectedLines == e.KeyData)
            {
                if (_subtitle.Paragraphs.Count > 0 && SubtitleListview1.SelectedItems.Count >= 1)
                {
                    e.SuppressKeyPress = true;
                    if (SubtitleListview1.SelectedItems.Count == 2)
                        MergeAfterToolStripMenuItemClick(null, null);
                    else
                        MergeSelectedLines();
                }
            }
            else if (_mainGeneralMergeSelectedLinesOnlyFirstText == e.KeyData)
            {
                if (_subtitle.Paragraphs.Count > 0 && SubtitleListview1.SelectedItems.Count >= 1)
                {
                    e.SuppressKeyPress = true;
                    MergeSelectedLinesOnlyFirstText();
                }
            }
            else if (_mainGeneralToggleTranslationMode == e.KeyData)
            { // toggle translator mode
                EditToolStripMenuItemDropDownOpening(null, null);
                toolStripMenuItemTranslationMode_Click(null, null);
            }
            else if (e.KeyData == _videoPlayPauseToggle)
            {
                if (mediaPlayer.VideoPlayer != null)
                {
                    _endSeconds = -1;
                    mediaPlayer.TogglePlayPause();
                    e.SuppressKeyPress = true;
                }
            }
            else if (e.KeyData == _videoPause)
            {
                if (mediaPlayer.VideoPlayer != null)
                {
                    _endSeconds = -1;
                    mediaPlayer.Pause();
                    e.SuppressKeyPress = true;
                }
            }
            else if (e.Modifiers == (Keys.Control | Keys.Shift) && e.KeyCode == Keys.Right)
            {
                if (!textBoxListViewText.Focused && !textBoxListViewTextAlternate.Focused)
                {
                    mediaPlayer.CurrentPosition += 1.0;
                    e.SuppressKeyPress = true;
                }
            }
            else if (e.Modifiers == (Keys.Control | Keys.Shift) && e.KeyCode == Keys.Left)
            {
                if (!textBoxListViewText.Focused && !textBoxListViewTextAlternate.Focused)
                {
                    mediaPlayer.CurrentPosition -= 1.0;
                    e.SuppressKeyPress = true;
                }
            }
            else if (e.Modifiers == Keys.None && e.KeyCode == Keys.Space)
            {
                if (!textBoxListViewText.Focused && !textBoxListViewTextAlternate.Focused && !textBoxSource.Focused && mediaPlayer.VideoPlayer != null)
                {
                    if (audioVisualizer != null && audioVisualizer.Focused || mediaPlayer.Focused || SubtitleListview1.Focused)
                    {
                        _endSeconds = -1;
                        mediaPlayer.TogglePlayPause();
                        e.SuppressKeyPress = true;
                    }
                }
            }
            else if (e.Modifiers == Keys.Alt && e.KeyCode == Keys.D1)
            {
                if (SubtitleListview1.SelectedItems.Count > 0 && _subtitle != null && mediaPlayer.VideoPlayer != null)
                {
                    Paragraph p = _subtitle.GetParagraphOrDefault(SubtitleListview1.SelectedItems[0].Index);
                    if (p != null)
                    {
                        mediaPlayer.CurrentPosition = p.StartTime.TotalSeconds;
                        e.SuppressKeyPress = true;
                    }
                }
            }
            else if (e.Modifiers == Keys.Alt && e.KeyCode == Keys.D2)
            {
                if (SubtitleListview1.SelectedItems.Count > 0 && _subtitle != null && mediaPlayer.VideoPlayer != null)
                {
                    Paragraph p = _subtitle.GetParagraphOrDefault(SubtitleListview1.SelectedItems[0].Index);
                    if (p != null)
                    {
                        mediaPlayer.CurrentPosition = p.EndTime.TotalSeconds;
                        e.SuppressKeyPress = true;
                    }
                }
            }
            else if (e.Modifiers == Keys.Alt && e.KeyCode == Keys.D3)
            {
                if (SubtitleListview1.SelectedItems.Count > 0 && _subtitle != null && mediaPlayer.VideoPlayer != null)
                {
                    int index = SubtitleListview1.SelectedItems[0].Index - 1;
                    Paragraph p = _subtitle.GetParagraphOrDefault(index);
                    if (p != null)
                    {
                        SubtitleListview1.SelectIndexAndEnsureVisible(index);
                        mediaPlayer.CurrentPosition = p.StartTime.TotalSeconds;
                        e.SuppressKeyPress = true;
                    }
                }
            }
            else if (e.Modifiers == Keys.Alt && e.KeyCode == Keys.D4)
            {
                if (SubtitleListview1.SelectedItems.Count > 0 && _subtitle != null && mediaPlayer.VideoPlayer != null)
                {
                    int index = SubtitleListview1.SelectedItems[0].Index + 1;
                    Paragraph p = _subtitle.GetParagraphOrDefault(index);
                    if (p != null)
                    {
                        SubtitleListview1.SelectIndexAndEnsureVisible(index);
                        mediaPlayer.CurrentPosition = p.StartTime.TotalSeconds;
                        e.SuppressKeyPress = true;
                    }
                }
            }
            else if (e.Modifiers == Keys.None && e.KeyCode == Keys.F4)
            {
                if (SubtitleListview1.SelectedItems.Count > 0 && _subtitle != null && mediaPlayer.VideoPlayer != null)
                {
                    mediaPlayer.Pause();
                    Paragraph p = _subtitle.GetParagraphOrDefault(SubtitleListview1.SelectedItems[0].Index);
                    if (p != null)
                    {
                        if (Math.Abs(mediaPlayer.CurrentPosition - p.StartTime.TotalSeconds) < 0.1)
                            mediaPlayer.CurrentPosition = p.EndTime.TotalSeconds;
                        else
                            mediaPlayer.CurrentPosition = p.StartTime.TotalSeconds;
                        e.SuppressKeyPress = true;
                    }

                }
            }
            else if (e.Modifiers == Keys.None && e.KeyCode == Keys.F5)
            {
                if (SubtitleListview1.SelectedItems.Count > 0 && _subtitle != null && mediaPlayer.VideoPlayer != null)
                {
                    Paragraph p = _subtitle.GetParagraphOrDefault(SubtitleListview1.SelectedItems[0].Index);
                    if (p != null)
                    {
                        mediaPlayer.CurrentPosition = p.StartTime.TotalSeconds;
                        ShowSubtitle();
                        mediaPlayer.Play();
                        _endSeconds = p.EndTime.TotalSeconds;
                        e.SuppressKeyPress = true;
                    }
                }
            }
            else if (e.Modifiers == Keys.None && e.KeyCode == Keys.F6)
            {
                if (mediaPlayer.VideoPlayer != null)
                {
                    GotoSubPositionAndPause();
                }
            }
            else if (e.Modifiers == Keys.None && e.KeyCode == Keys.F7)
            {
                if (mediaPlayer.VideoPlayer != null)
                {
                    GoBackSeconds(3);
                }
            }
            else if (e.Modifiers == Keys.None && e.KeyCode == Keys.F8)
            {
                if (mediaPlayer.VideoPlayer != null)
                {
                    _endSeconds = -1;
                    mediaPlayer.TogglePlayPause();
                    e.SuppressKeyPress = true;
                }
            }
            else if (e.Modifiers == (Keys.Control | Keys.Alt | Keys.Shift) && e.KeyCode == Keys.W) // watermak
            {
                Encoding enc = GetCurrentEncoding();
                if (enc != Encoding.UTF8 && enc != Encoding.UTF32 && enc != Encoding.Unicode && enc != Encoding.UTF7)
                {
                    MessageBox.Show("Watermark only works with unicode file encoding");
                }
                else
                {
                    Watermark watermarkForm = new Watermark();
                    watermarkForm.Initialize(_subtitle, FirstSelectedIndex);
                    if (watermarkForm.ShowDialog(this) == DialogResult.OK)
                    {
                        watermarkForm.AddOrRemove(_subtitle);
                        RefreshSelectedParagraph();
                    }
                }
            }
            else if (e.Modifiers == (Keys.Control | Keys.Alt | Keys.Shift) && e.KeyCode == Keys.F) // Toggle HHMMSSFF / HHMMSSMMM
            {
                Configuration.Settings.General.UseTimeFormatHHMMSSFF = !Configuration.Settings.General.UseTimeFormatHHMMSSFF;
                RefreshTimeCodeMode();
            }
            else if (_mainGeneralSwitchTranslationAndOriginal == e.KeyData) // switch original/current
            {
                if (_subtitleAlternate != null && _subtitleAlternate.Paragraphs.Count > 0 && _networkSession == null)
                {
                    int firstIndex = FirstSelectedIndex;
                    double firstMs = -1;
                    if (firstIndex >= 0)
                        firstMs = _subtitle.Paragraphs[firstIndex].StartTime.TotalMilliseconds;

                    Subtitle temp = _subtitle;
                    _subtitle = _subtitleAlternate;
                    _subtitleAlternate = temp;

                    string tempName = _fileName;
                    _fileName = _subtitleAlternateFileName;
                    _subtitleAlternateFileName = tempName;

                    string tempChangeSubText = _changeSubtitleToString;
                    _changeSubtitleToString = _changeAlternateSubtitleToString;
                    _changeAlternateSubtitleToString = tempChangeSubText;

                    SubtitleListview1.Fill(_subtitle, _subtitleAlternate);

                    _subtitleListViewIndex = -1;
                    if (firstIndex >= 0 && _subtitle.Paragraphs.Count > firstIndex && _subtitle.Paragraphs[firstIndex].StartTime.TotalMilliseconds == firstMs)
                        SubtitleListview1.SelectIndexAndEnsureVisible(firstIndex);
                    else
                        RefreshSelectedParagraph();

                    SetTitle();

                    _fileDateTime = new DateTime();
                }
            }
            else if (_mainGeneralMergeTranslationAndOriginal == e.KeyData) // Merge translation and original
            {
                if (_subtitleAlternate != null && _subtitleAlternate.Paragraphs.Count > 0 && _networkSession == null)
                {
                    if (ContinueNewOrExit())
                    {
                        Subtitle subtitle = new Subtitle();
                        foreach (var p in _subtitle.Paragraphs)
                        {
                            var newP = new Paragraph(p);
                            var original = Utilities.GetOriginalParagraph(_subtitle.GetIndex(p), p, _subtitleAlternate.Paragraphs);
                            if (original != null)
                                newP.Text += Environment.NewLine + Environment.NewLine + original.Text;
                            subtitle.Paragraphs.Add(newP);
                        }
                        RemoveAlternate(true);
                        FileNew();
                        _subtitle = subtitle;
                        _subtitleListViewIndex = -1;
                        ShowSource();
                        SubtitleListview1.Fill(_subtitle, _subtitleAlternate);
                        SubtitleListview1.SelectIndexAndEnsureVisible(0);

                        e.SuppressKeyPress = true;
                    }
                }
            }
            else if (e.KeyData == _toggleVideoDockUndock)
            {
                if (_isVideoControlsUndocked)
                    RedockVideoControlsToolStripMenuItemClick(null, null);
                else
                    UndockVideoControlsToolStripMenuItemClick(null, null);
            }
            else if (mediaPlayer.VideoPlayer != null && e.KeyData == _video1FrameLeft)
            {
                MoveVideoSeconds(-1.0 / Configuration.Settings.General.CurrentFrameRate);
                e.SuppressKeyPress = true;
            }
            else if (mediaPlayer.VideoPlayer != null && e.KeyData == _video1FrameRight)
            {
                MoveVideoSeconds(1.0 / Configuration.Settings.General.CurrentFrameRate);
                e.SuppressKeyPress = true;
            }
            else if (mediaPlayer.VideoPlayer != null && e.KeyData == _video100MsLeft)
            {
                MoveVideoSeconds(-0.1);
                e.SuppressKeyPress = true;
            }
            else if (mediaPlayer.VideoPlayer != null && e.KeyData == _video100MsRight)
            {
                MoveVideoSeconds(0.1);
                e.SuppressKeyPress = true;
            }
            else if (mediaPlayer.VideoPlayer != null && e.KeyData == _video500MsLeft)
            {
                MoveVideoSeconds(-0.5);
                e.SuppressKeyPress = true;
            }
            else if (mediaPlayer.VideoPlayer != null && e.KeyData == _video500MsRight)
            {
                MoveVideoSeconds(0.5);
                e.SuppressKeyPress = true;
            }
            else if (mediaPlayer.VideoPlayer != null && e.KeyData == _video1000MsLeft)
            {
                MoveVideoSeconds(-1.0);
                e.SuppressKeyPress = true;
            }
            else if (mediaPlayer.VideoPlayer != null && e.KeyData == _video1000MsRight)
            {
                MoveVideoSeconds(1.0);
                e.SuppressKeyPress = true;
            }
            else if (_mainToolsBeamer == e.KeyData)
            {
                var beamer = new Beamer(this, _subtitle, _subtitleListViewIndex);
                beamer.ShowDialog(this);
            }
            else if (e.KeyData == _mainVideoFullscreen) // fullscreen
            {
                GoFullscreen();
            }
            else if (audioVisualizer.Focused && audioVisualizer.NewSelectionParagraph != null && e.KeyData == _waveformAddTextAtHere)
            {
                addParagraphHereToolStripMenuItem_Click(null, null);
                e.SuppressKeyPress = true;
            }
            else if (audioVisualizer.Focused && e.KeyData == _waveformFocusListView)
            {
                SubtitleListview1.Focus();
                e.SuppressKeyPress = true;
            }
            else if (audioVisualizer.Focused && e.KeyCode == Keys.Delete)
            {
                ToolStripMenuItemDeleteClick(null, null);
                e.SuppressKeyPress = true;
            }
            else if (_mainToolsAutoDuration == e.KeyData)
            {
                MakeAutoDuration();
                e.SuppressKeyPress = true;
            }
            else if (e.Modifiers == (Keys.Control | Keys.Alt | Keys.Shift) && e.KeyCode == Keys.I) // watermak
            {
                var form = new ImportUnknownFormat(string.Empty);
                if (form.ShowDialog(this) == DialogResult.OK)
                {
                    _subtitle = form.ImportedSubitle;
                    _fileName = null;
                    SubtitleListview1.Fill(_subtitle, _subtitleAlternate);
                }
                e.SuppressKeyPress = true;
            }
            else if ((textBoxListViewText.Focused || (SubtitleListview1.Focused && SubtitleListview1.SelectedItems.Count == 1) || (audioVisualizer.Focused && SubtitleListview1.SelectedItems.Count == 1)) && _mainTextBoxMoveLastWordDown == e.KeyData)
            {
                MoveLastWordDown();
                e.SuppressKeyPress = true;
            }
            else if ((textBoxListViewText.Focused || (SubtitleListview1.Focused && SubtitleListview1.SelectedItems.Count == 1) || (audioVisualizer.Focused && SubtitleListview1.SelectedItems.Count == 1)) && _mainTextBoxMoveFirstWordFromNextUp == e.KeyData)
            {
                MoveFirstWordInNextUp();
                e.SuppressKeyPress = true;
            }

            // TABS - MUST BE LAST
            else if (tabControlButtons.SelectedTab == tabPageAdjust)
            {
                if (_mainAdjustSelected100MsForward == e.KeyData)
                {
                    ShowEarlierOrLater(100, SelectionChoice.SelectionOnly);
                    e.SuppressKeyPress = true;
                }
                else if (_mainAdjustSelected100MsBack == e.KeyData)
                {
                    ShowEarlierOrLater(-100, SelectionChoice.SelectionOnly);
                    e.SuppressKeyPress = true;
                }
                else if (mediaPlayer.VideoPlayer != null)
                {
                    if (_mainAdjustSetStartAndOffsetTheRest == e.KeyData)
                    {
                        ButtonSetStartAndOffsetRestClick(null, null);
                        e.SuppressKeyPress = true;
                    }
                    else if (_mainAdjustSetEndAndOffsetTheRest == e.KeyData)
                    {
                        SetEndAndOffsetTheRest(false);
                        e.SuppressKeyPress = true;
                    }
                    else if (_mainAdjustSetEndAndOffsetTheRestAndGoToNext == e.KeyData)
                    {
                        SetEndAndOffsetTheRest(true);
                        e.SuppressKeyPress = true;
                    }
                    else if (_mainAdjustSetEndAndGotoNext == e.KeyData)
                    {
                        ButtonSetEndAndGoToNextClick(null, null);
                        e.SuppressKeyPress = true;
                    }
                    else if (e.Modifiers == Keys.None && e.KeyCode == Keys.F9)
                    {
                        ButtonSetStartAndOffsetRestClick(null, null);
                        e.SuppressKeyPress = true;
                    }
                    else if (e.Modifiers == Keys.None && e.KeyCode == Keys.F10)
                    {
                        ButtonSetEndAndGoToNextClick(null, null);
                        e.SuppressKeyPress = true;
                    }
                    else if ((e.Modifiers == Keys.None && e.KeyCode == Keys.F11) || _mainAdjustSetStart == e.KeyData)
                    {
                        buttonSetStartTime_Click(null, null);
                        e.SuppressKeyPress = true;
                    }
                    else if (_mainAdjustSetStartKeepDuration == e.KeyData)
                    {
                        SetStartTime(true);
                        e.SuppressKeyPress = true;
                    }
                    else if (e.Modifiers == Keys.None && e.KeyCode == Keys.F11)
                    {
                        SetStartTime(false);
                        e.SuppressKeyPress = true;
                    }
                    else if ((e.Modifiers == Keys.None && e.KeyCode == Keys.F12) || _mainAdjustSetEnd == e.KeyData)
                    {
                        StopAutoDuration();
                        ButtonSetEndClick(null, null);
                        e.SuppressKeyPress = true;
                    }
                    else if (_mainAdjustInsertViaEndAutoStartAndGoToNext == e.KeyData)
                    {
                        SetCurrentViaEndPositionAndGotoNext(FirstSelectedIndex);
                        e.SuppressKeyPress = true;
                    }
                    else if (_mainAdjustSetStartAutoDurationAndGoToNext == e.KeyData)
                    {
                        SetCurrentStartAutoDurationAndGotoNext(FirstSelectedIndex);
                        e.SuppressKeyPress = true;
                    }
                    else if (_mainAdjustSetEndNextStartAndGoToNext == e.KeyData)
                    {
                        SetCurrentEndNextStartAndGoToNext(FirstSelectedIndex);
                        e.SuppressKeyPress = true;
                    }
                    else if (_mainAdjustStartDownEndUpAndGoToNext == e.KeyData && _mainAdjustStartDownEndUpAndGoToNextParagraph == null)
                    {
                        _mainAdjustStartDownEndUpAndGoToNextParagraph = _subtitle.GetParagraphOrDefault(FirstSelectedIndex);
                        SetStartTime(true);
                        e.SuppressKeyPress = true;
                    }
                }
            }
            else if (tabControlButtons.SelectedTab == tabPageCreate && mediaPlayer.VideoPlayer != null)
            {
                if (e.Modifiers == Keys.Control && e.KeyCode == Keys.F9)
                {
                    InsertNewTextAtVideoPosition();
                    e.SuppressKeyPress = true;
                }
                else if ((e.Modifiers == Keys.Shift && e.KeyCode == Keys.F9) || _mainCreateInsertSubAtVideoPos == e.KeyData)
                {
                    var p = InsertNewTextAtVideoPosition();
                    p.Text = string.Empty; // p.StartTime.ToShortString();
                    SubtitleListview1.SetText(_subtitle.GetIndex(p), p.Text);
                    textBoxListViewText.Text = p.Text;
                    e.SuppressKeyPress = true;
                }
                else if (e.Modifiers == Keys.Alt && e.KeyCode == Keys.F9)
                {
                    StopAutoDuration();
                    ButtonSetEndClick(null, null);
                    e.SuppressKeyPress = true;
                }
                else if (e.Modifiers == Keys.None && e.KeyCode == Keys.F9)
                {
                    ButtonInsertNewTextClick(null, null);
                    e.SuppressKeyPress = true;
                }
                else if ((e.Modifiers == Keys.None && e.KeyCode == Keys.F10) || _mainCreatePlayFromJustBefore == e.KeyData)
                {
                    buttonBeforeText_Click(null, null);
                    e.SuppressKeyPress = true;
                }
                else if ((e.Modifiers == Keys.None && e.KeyCode == Keys.F11) || _mainCreateSetStart == e.KeyData)
                {
                    buttonSetStartTime_Click(null, null);
                    e.SuppressKeyPress = true;
                }
                else if ((e.Modifiers == Keys.None && e.KeyCode == Keys.F12) || _mainCreateSetEnd == e.KeyData)
                {
                    StopAutoDuration();
                    ButtonSetEndClick(null, null);
                    e.SuppressKeyPress = true;
                }
                else if (_mainCreateSetEndAddNewAndGoToNew == e.KeyData)
                {
                    StopAutoDuration();
                    e.SuppressKeyPress = true;

                    if (SubtitleListview1.SelectedItems.Count == 1)
                    {
                        double videoPosition = mediaPlayer.CurrentPosition;
                        if (!mediaPlayer.IsPaused)
                            videoPosition -= Configuration.Settings.General.SetStartEndHumanDelay / 1000.0;
                        int index = SubtitleListview1.SelectedItems[0].Index;
                        MakeHistoryForUndoOnlyIfNotResent(string.Format(_language.VideoControls.BeforeChangingTimeInWaveformX, "#" + _subtitle.Paragraphs[index].Number + " " + _subtitle.Paragraphs[index].Text));

                        _subtitle.Paragraphs[index].EndTime = TimeCode.FromSeconds(videoPosition);
                        SubtitleListview1.SetStartTimeAndDuration(index, _subtitle.Paragraphs[index]);

                        SetDurationInSeconds(_subtitle.Paragraphs[index].Duration.TotalSeconds);

                        ButtonInsertNewTextClick(null, null);
                    }

                }
                else if (_mainCreateStartDownEndUp == e.KeyData)
                {
                    if (_mainCreateStartDownEndUpParagraph == null)
                        _mainCreateStartDownEndUpParagraph = InsertNewTextAtVideoPosition();
                    e.SuppressKeyPress = true;
                }
                else if (_mainAdjustSelected100MsForward == e.KeyData)
                {
                    ShowEarlierOrLater(100, SelectionChoice.SelectionOnly);
                    e.SuppressKeyPress = true;
                }
                else if (_mainAdjustSelected100MsBack == e.KeyData)
                {
                    ShowEarlierOrLater(-100, SelectionChoice.SelectionOnly);
                    e.SuppressKeyPress = true;
                }
            }
            else if (tabControlButtons.SelectedTab == tabPageTranslate)
            {
                if (_mainTranslateCustomSearch1 == e.KeyData)
                {
                    e.SuppressKeyPress = true;
                    RunCustomSearch(Configuration.Settings.VideoControls.CustomSearchUrl1);
                }
                else if (_mainTranslateCustomSearch2 == e.KeyData)
                {
                    e.SuppressKeyPress = true;
                    RunCustomSearch(Configuration.Settings.VideoControls.CustomSearchUrl2);
                }
                else if (_mainTranslateCustomSearch3 == e.KeyData)
                {
                    e.SuppressKeyPress = true;
                    RunCustomSearch(Configuration.Settings.VideoControls.CustomSearchUrl3);
                }
                else if (_mainTranslateCustomSearch4 == e.KeyData)
                {
                    e.SuppressKeyPress = true;
                    RunCustomSearch(Configuration.Settings.VideoControls.CustomSearchUrl4);
                }
                else if (_mainTranslateCustomSearch5 == e.KeyData)
                {
                    e.SuppressKeyPress = true;
                    RunCustomSearch(Configuration.Settings.VideoControls.CustomSearchUrl5);
                }
                else if (_mainTranslateCustomSearch6 == e.KeyData)
                {
                    e.SuppressKeyPress = true;
                    RunCustomSearch(Configuration.Settings.VideoControls.CustomSearchUrl6);
                }
            }
            // put new entries above tabs
        }

        private void MergeSelectedLinesOnlyFirstText()
        {
            if (_subtitle.Paragraphs.Count > 0 && SubtitleListview1.SelectedItems.Count > 1)
            {
                var sb = new StringBuilder();
                var deleteIndices = new List<int>();
                bool first = true;
                int firstIndex = 0;
                int next = 0;
                string text = string.Empty;
                double endTime = 0;
                foreach (int index in SubtitleListview1.SelectedIndices)
                {
                    if (first)
                    {
                        firstIndex = index;
                        next = index + 1;
                    }
                    else
                    {
                        deleteIndices.Add(index);
                        if (next != index)
                            return;
                        next++;
                    }
                    first = false;
                    if (string.IsNullOrEmpty(text))
                        text = _subtitle.Paragraphs[index].Text.Trim();
                    endTime = _subtitle.Paragraphs[index].EndTime.TotalMilliseconds;
                }

                SubtitleListview1.SelectedIndexChanged -= SubtitleListview1_SelectedIndexChanged;
                MakeHistoryForUndo(_language.BeforeMergeLines);

                Paragraph currentParagraph = _subtitle.Paragraphs[firstIndex];
                currentParagraph.Text = text;
                currentParagraph.EndTime.TotalMilliseconds = endTime;

                Paragraph nextParagraph = _subtitle.GetParagraphOrDefault(next);
                if (nextParagraph != null && currentParagraph.EndTime.TotalMilliseconds > nextParagraph.StartTime.TotalMilliseconds && currentParagraph.StartTime.TotalMilliseconds < nextParagraph.StartTime.TotalMilliseconds)
                {
                    currentParagraph.EndTime.TotalMilliseconds = nextParagraph.StartTime.TotalMilliseconds - 1;
                }

                // original subtitle
                if (_subtitleAlternate != null && Configuration.Settings.General.AllowEditOfOriginalSubtitle)
                {
                    Paragraph original = Utilities.GetOriginalParagraph(firstIndex, currentParagraph, _subtitleAlternate.Paragraphs);
                    if (original != null)
                    {
                        string originalText = string.Empty;
                        for (int i = 0; i < deleteIndices.Count; i++)
                        {
                            Paragraph originalNext = Utilities.GetOriginalParagraph(deleteIndices[i], _subtitle.Paragraphs[deleteIndices[i]], _subtitleAlternate.Paragraphs);
                            if (originalNext != null && string.IsNullOrEmpty(originalText))
                                originalText = originalNext.Text;
                        }
                        for (int i = deleteIndices.Count - 1; i >= 0; i--)
                        {
                            Paragraph originalNext = Utilities.GetOriginalParagraph(deleteIndices[i], _subtitle.Paragraphs[deleteIndices[i]], _subtitleAlternate.Paragraphs);
                            if (originalNext != null)
                                _subtitleAlternate.Paragraphs.Remove(originalNext);
                        }
                        original.Text = originalText;
                        original.EndTime.TotalMilliseconds = currentParagraph.EndTime.TotalMilliseconds;
                        _subtitleAlternate.Renumber(1);
                    }
                }

                if (_networkSession != null)
                {
                    _networkSession.TimerStop();
                    _networkSession.UpdateLine(firstIndex, currentParagraph);
                    NetworkGetSendUpdates(deleteIndices, 0, null);
                }
                else
                {
                    for (int i = deleteIndices.Count - 1; i >= 0; i--)
                        _subtitle.Paragraphs.RemoveAt(deleteIndices[i]);
                    _subtitle.Renumber(1);
                    SubtitleListview1.Fill(_subtitle, _subtitleAlternate);
                }
                ShowSource();
                ShowStatus(_language.LinesMerged);
                SubtitleListview1.SelectIndexAndEnsureVisible(firstIndex, true);
                SubtitleListview1.SelectedIndexChanged += SubtitleListview1_SelectedIndexChanged;
                RefreshSelectedParagraph();
            }
        }

        private void GoToFirstEmptyLine()
        {
            var index = FirstSelectedIndex + 1;
            for (; index < _subtitle.Paragraphs.Count; index++)
            {
                if (string.IsNullOrWhiteSpace(_subtitle.Paragraphs[index].Text))
                {
                    SubtitleListview1.SelectIndexAndEnsureVisible(index);
                    return;
                }
            }
        }

        private void PlayFirstSelectedSubtitle()
        {
            if (_subtitleListViewIndex >= 0 && mediaPlayer.VideoPlayer != null)
            {
                GotoSubtitleIndex(_subtitleListViewIndex);
                var paragraph = _subtitle.Paragraphs[_subtitleListViewIndex];
                double startSeconds = paragraph.StartTime.TotalSeconds;
                _endSeconds = paragraph.EndTime.TotalSeconds;
                mediaPlayer.CurrentPosition = startSeconds;
                ShowSubtitle();
                mediaPlayer.Play();
            }
        }

        private void SetEndAndOffsetTheRest(bool goToNext)
        {
            if (SubtitleListview1.SelectedItems.Count == 1)
            {
                bool oldSync = checkBoxSyncListViewWithVideoWhilePlaying.Checked;
                checkBoxSyncListViewWithVideoWhilePlaying.Checked = false;

                int index = SubtitleListview1.SelectedItems[0].Index;
                double videoPosition = mediaPlayer.CurrentPosition;
                if (!mediaPlayer.IsPaused)
                    videoPosition -= Configuration.Settings.General.SetStartEndHumanDelay / 1000.0;

                var tc = TimeCode.FromSeconds(videoPosition);

                double offset = tc.TotalMilliseconds - _subtitle.Paragraphs[index].EndTime.TotalMilliseconds;
                if (_subtitle.Paragraphs[index].StartTime.TotalMilliseconds + 100 > tc.TotalMilliseconds || offset > Configuration.Settings.General.SubtitleMaximumDisplayMilliseconds)
                    return;

                MakeHistoryForUndo(_language.BeforeSetEndTimeAndOffsetTheRest + @"  " + _subtitle.Paragraphs[index].Number + @" - " + tc);

                numericUpDownDuration.ValueChanged -= NumericUpDownDurationValueChanged;
                _subtitle.Paragraphs[index].EndTime.TotalSeconds = videoPosition;
                SubtitleListview1.SetDuration(index, _subtitle.Paragraphs[index]);
                checkBoxSyncListViewWithVideoWhilePlaying.Checked = oldSync;
                numericUpDownDuration.Value = (decimal)_subtitle.Paragraphs[index].Duration.TotalSeconds;
                numericUpDownDuration.ValueChanged += NumericUpDownDurationValueChanged;

                for (int i = index + 1; i < SubtitleListview1.Items.Count; i++)
                {
                    if (!_subtitle.Paragraphs[i].StartTime.IsMaxTime)
                    {
                        _subtitle.Paragraphs[i].StartTime = new TimeCode(_subtitle.Paragraphs[i].StartTime.TotalMilliseconds + offset);
                        _subtitle.Paragraphs[i].EndTime = new TimeCode(_subtitle.Paragraphs[i].EndTime.TotalMilliseconds + offset);
                        SubtitleListview1.SetDuration(i, _subtitle.Paragraphs[i]);
                    }
                }

                if (Configuration.Settings.General.AllowEditOfOriginalSubtitle && _subtitleAlternate != null && _subtitleAlternate.Paragraphs.Count > 0)
                {
                    Paragraph original = Utilities.GetOriginalParagraph(index, _subtitle.Paragraphs[index], _subtitleAlternate.Paragraphs);
                    if (original != null)
                    {
                        index = _subtitleAlternate.GetIndex(original);
                        for (int i = index; i < _subtitleAlternate.Paragraphs.Count; i++)
                        {
                            if (!_subtitleAlternate.Paragraphs[i].StartTime.IsMaxTime)
                            {
                                _subtitleAlternate.Paragraphs[i].StartTime = new TimeCode(_subtitleAlternate.Paragraphs[i].StartTime.TotalMilliseconds + offset);
                                _subtitleAlternate.Paragraphs[i].EndTime = new TimeCode(_subtitleAlternate.Paragraphs[i].EndTime.TotalMilliseconds + offset);
                            }
                        }
                    }
                }
                if (IsFramesRelevant && CurrentFrameRate > 0)
                {
                    _subtitle.CalculateFrameNumbersFromTimeCodesNoCheck(CurrentFrameRate);
                    if (tabControlSubtitle.SelectedIndex == TabControlSourceView)
                        ShowSource();
                }

                checkBoxSyncListViewWithVideoWhilePlaying.Checked = oldSync;
                numericUpDownDuration.ValueChanged += NumericUpDownDurationValueChanged;

                if (goToNext)
                {
                    SubtitleListview1.SelectIndexAndEnsureVisible(index + 1);
                }
            }
        }

        private void MoveVideoSeconds(double seconds)
        {
            if (mediaPlayer.IsPaused && Configuration.Settings.General.MoveVideo100Or500MsPlaySmallSample)
            {
                double p = mediaPlayer.CurrentPosition + seconds;
                mediaPlayer.CurrentPosition = p;
                mediaPlayer.Play();
                System.Threading.Thread.Sleep(99);
                mediaPlayer.Stop();
                mediaPlayer.CurrentPosition = p;
            }
            else
            {
                mediaPlayer.CurrentPosition += seconds;
            }
        }

        private void RunCustomSearch(string url)
        {
            if (!string.IsNullOrEmpty(url) && !string.IsNullOrEmpty(textBoxSearchWord.Text))
            {
                if (url.Contains("{0}"))
                    url = string.Format(url, Utilities.UrlEncode(textBoxSearchWord.Text));
                System.Diagnostics.Process.Start(url);
            }
        }

        private void GoFullscreen()
        {
            if (mediaPlayer.VideoPlayer == null)
                return;

            mediaPlayer.ShowFullScreenControls();
            bool setRedockOnFullscreenEnd = false;

            if (_videoPlayerUndocked == null || _videoPlayerUndocked.IsDisposed)
            {
                UndockVideoControlsToolStripMenuItemClick(null, null);
                setRedockOnFullscreenEnd = true;
            }

            if (_videoPlayerUndocked != null && !_videoPlayerUndocked.IsDisposed)
            {
                _videoPlayerUndocked.Focus();
                _videoPlayerUndocked.GoFullscreen();
                if (setRedockOnFullscreenEnd)
                    _videoPlayerUndocked.RedockOnFullscreenEnd = true;
            }
        }

        private void RefreshTimeCodeMode()
        {
            if (Configuration.Settings.General.UseTimeFormatHHMMSSFF)
            {
                numericUpDownDuration.DecimalPlaces = 2;
                numericUpDownDuration.Increment = (decimal)(0.01);

                toolStripSeparatorFrameRate.Visible = true;
                toolStripLabelFrameRate.Visible = true;
                toolStripComboBoxFrameRate.Visible = true;
                toolStripButtonGetFrameRate.Visible = true;
            }
            else
            {
                numericUpDownDuration.DecimalPlaces = 3;
                numericUpDownDuration.Increment = (decimal)(0.1);

                toolStripSeparatorFrameRate.Visible = Configuration.Settings.General.ShowFrameRate;
                toolStripLabelFrameRate.Visible = Configuration.Settings.General.ShowFrameRate;
                toolStripComboBoxFrameRate.Visible = Configuration.Settings.General.ShowFrameRate;
                toolStripButtonGetFrameRate.Visible = Configuration.Settings.General.ShowFrameRate;
            }

            SaveSubtitleListviewIndexes();
            SubtitleListview1.Fill(_subtitle, _subtitleAlternate);
            RestoreSubtitleListviewIndexes();
            RefreshSelectedParagraph();
        }

        private void ReverseStartAndEndingForRTL()
        {
            int selectedIndex = FirstSelectedIndex;
            foreach (int index in SubtitleListview1.SelectedIndices)
            {
                Paragraph p = _subtitle.Paragraphs[index];
                p.Text = ReverseStartAndEndingForRTL(p.Text);
                SubtitleListview1.SetText(index, p.Text);
                if (index == selectedIndex)
                    textBoxListViewText.Text = p.Text;
            }
        }

        private static string ReverseString(string s)
        {
            char[] charArray = s.ToCharArray();
            Array.Reverse(charArray);
            return new string(charArray);
        }

        private static string ReverseParethesis(string s)
        {
            const string k = "@__<<>___@";

            s = s.Replace("(", k);
            s = s.Replace(")", "(");
            s = s.Replace(k, ")");

            s = s.Replace("[", k);
            s = s.Replace("]", "[");
            s = s.Replace(k, "]");

            return s;
        }

        private static string ReverseStartAndEndingForRTL(string s)
        {
            var lines = s.Split(Utilities.NewLineChars, StringSplitOptions.RemoveEmptyEntries);
            var newLines = new StringBuilder();
            foreach (string line in lines)
            {
                string s2 = line;

                bool startsWithItalic = false;
                if (s2.StartsWith("<i>"))
                {
                    startsWithItalic = true;
                    s2 = s2.Remove(0, 3);
                }
                bool endsWithItalic = false;
                if (s2.EndsWith("</i>"))
                {
                    endsWithItalic = true;
                    s2 = s2.Remove(s2.Length - 4, 4);
                }

                var pre = new StringBuilder();
                var post = new StringBuilder();

                int i = 0;
                while (i < s2.Length && @"- !?.""،,():;[]".Contains(s2[i]))
                {
                    pre.Append(s2[i]);
                    i++;
                }
                int j = s2.Length - 1;
                while (j > i && @"- !?.""،,():;[]".Contains(s2[j]))
                {
                    post.Append(s2[j]);
                    j--;
                }
                if (startsWithItalic)
                    newLines.Append("<i>");
                newLines.Append(ReverseParethesis(post.ToString()));
                newLines.Append(s2.Substring(pre.Length, s2.Length - (pre.Length + post.Length)));
                newLines.Append(ReverseParethesis(ReverseString(pre.ToString())));
                if (endsWithItalic)
                    newLines.Append("</i>");
                newLines.AppendLine();

            }
            return newLines.ToString().Trim();
        }

        private void MergeDialogs()
        {
            if (SubtitleListview1.SelectedItems.Count == 2 && SubtitleListview1.SelectedIndices[0] + 1 == SubtitleListview1.SelectedIndices[1])
                MergeWithLineAfter(true);
        }

        private void ToggleDashes()
        {
            int index = FirstSelectedIndex;
            if (index >= 0)
            {
                bool hasStartDash = false;
                var p = _subtitle.Paragraphs[index];
                string[] lines = p.Text.Split(Utilities.NewLineChars, StringSplitOptions.RemoveEmptyEntries);
                foreach (string line in lines)
                {
                    if (line.TrimStart().StartsWith('-') || line.TrimStart().StartsWith("<i>-") || line.TrimStart().StartsWith("<i> -"))
                        hasStartDash = true;
                }
                MakeHistoryForUndo(_language.BeforeToggleDialogDashes);
                if (hasStartDash)
                    RemoveDashes();
                else
                    AddDashes();
            }
        }

        private void AddDashes()
        {
            foreach (int index in SubtitleListview1.SelectedIndices)
            {
                var p = _subtitle.Paragraphs[index];
                string[] lines = p.Text.Split(Utilities.NewLineChars, StringSplitOptions.RemoveEmptyEntries);
                var sb = new StringBuilder();
                foreach (string line in lines)
                {
                    if (line.TrimStart().StartsWith('-') || line.TrimStart().StartsWith("<i>-") || line.TrimStart().StartsWith("<i> -"))
                        sb.AppendLine(line);
                    else if (line.TrimStart().StartsWith("<i>") && line.Trim().Length > 3)
                        sb.AppendLine("<i>-" + line.Substring(3));
                    else
                        sb.AppendLine("- " + line);
                }
                string text = sb.ToString().Trim();
                _subtitle.Paragraphs[index].Text = text;
                SubtitleListview1.SetText(index, text);
                if (index == _subtitleListViewIndex)
                    textBoxListViewText.Text = text;
            }
        }

        private void RemoveDashes()
        {
            foreach (int index in SubtitleListview1.SelectedIndices)
            {
                var p = _subtitle.Paragraphs[index];
                string[] lines = p.Text.Split(Utilities.NewLineChars, StringSplitOptions.RemoveEmptyEntries);
                var sb = new StringBuilder();
                foreach (string line in lines)
                {
                    if (line.TrimStart().StartsWith('-'))
                        sb.AppendLine(line.TrimStart().TrimStart('-').TrimStart());
                    else if (line.TrimStart().StartsWith("<i>-") || line.TrimStart().StartsWith("<i> -"))
                        sb.AppendLine("<i>" + line.TrimStart().Substring(3).TrimStart().TrimStart('-').TrimStart());
                    else
                        sb.AppendLine(line);
                }
                string text = sb.ToString().Trim();
                _subtitle.Paragraphs[index].Text = text;
                SubtitleListview1.SetText(index, text);
                if (index == _subtitleListViewIndex)
                    textBoxListViewText.Text = text;
            }
        }

        private void SetTitle()
        {
            Text = Title;

            string separator = " - ";
            if (!string.IsNullOrEmpty(_fileName))
            {
                Text = Text + separator + _fileName;
                separator = " + ";
            }

            if (Configuration.Settings.General.AllowEditOfOriginalSubtitle && _subtitleAlternate != null && _subtitleAlternate.Paragraphs.Count > 0)
            {
                Text = Text + separator;
                if (string.IsNullOrEmpty(_fileName))
                    Text = Text + Configuration.Settings.Language.Main.New + " + ";
                if (!string.IsNullOrEmpty(_subtitleAlternateFileName))
                    Text = Text + _subtitleAlternateFileName;
                else
                    Text = Text + Configuration.Settings.Language.Main.New;
            }
        }

        private void SubtitleListview1KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.C && e.Modifiers == Keys.Control) //Ctrl+c = Copy to clipboard
            {
                var tmp = new Subtitle();
                foreach (int i in SubtitleListview1.SelectedIndices)
                {
                    Paragraph p = _subtitle.GetParagraphOrDefault(i);
                    if (p != null)
                        tmp.Paragraphs.Add(new Paragraph(p));
                }
                if (tmp.Paragraphs.Count > 0)
                {
                    Clipboard.SetText(tmp.ToText(new SubRip()));
                }
                e.SuppressKeyPress = true;
            }
            else if (e.KeyData == _mainListViewCopyText)
            {
                StringBuilder sb = new StringBuilder();
                foreach (int i in SubtitleListview1.SelectedIndices)
                {
                    Paragraph p = _subtitle.GetParagraphOrDefault(i);
                    if (p != null)
                        sb.AppendLine(p.Text + Environment.NewLine);
                }
                if (sb.Length > 0)
                {
                    Clipboard.SetText(sb.ToString().Trim());
                }
                e.SuppressKeyPress = true;
            }
            else if (e.KeyData == _mainListViewAutoDuration)
            {
                MakeAutoDurationSelectedLines();
            }
            else if (e.KeyData == _mainListViewFocusWaveform)
            {
                if (audioVisualizer.CanFocus)
                {
                    audioVisualizer.Focus();
                    e.SuppressKeyPress = true;
                }
            }
            else if (e.KeyData == _mainListViewGoToNextError)
            {
                GoToNextSynaxError();
                e.SuppressKeyPress = true;
            }
            else if (e.KeyCode == Keys.V && e.Modifiers == Keys.Control) //Ctrl+vPaste from clipboard
            {
                if (Clipboard.ContainsText())
                {
                    string text = Clipboard.GetText();
                    var tmp = new Subtitle();
                    var format = new SubRip();
                    var list = new List<string>();
                    foreach (string line in text.Replace(Environment.NewLine, "\n").Split('\n'))
                        list.Add(line);
                    format.LoadSubtitle(tmp, list, null);
                    if (SubtitleListview1.SelectedItems.Count == 1 && tmp.Paragraphs.Count > 0)
                    {
                        MakeHistoryForUndo(_language.BeforeInsertLine);
                        _makeHistoryPaused = true;
                        Paragraph lastParagraph = null;
                        Paragraph lastTempParagraph = null;
                        foreach (Paragraph p in tmp.Paragraphs)
                        {
                            InsertAfter();
                            textBoxListViewText.Text = p.Text;
                            if (lastParagraph != null)
                            {
                                double millisecondsBetween = p.StartTime.TotalMilliseconds - lastTempParagraph.EndTime.TotalMilliseconds;
                                timeUpDownStartTime.TimeCode = new TimeCode(lastParagraph.EndTime.TotalMilliseconds + millisecondsBetween);
                            }
                            SetDurationInSeconds(p.Duration.TotalSeconds);
                            lastParagraph = _subtitle.GetParagraphOrDefault(_subtitleListViewIndex);
                            lastTempParagraph = p;
                        }
                        RestartHistory();
                    }
                    else if (SubtitleListview1.Items.Count == 0 && tmp.Paragraphs.Count > 0)
                    { // insert into empty subtitle
                        MakeHistoryForUndo(_language.BeforeInsertLine);
                        foreach (Paragraph p in tmp.Paragraphs)
                        {
                            _subtitle.Paragraphs.Add(p);
                        }
                        SubtitleListview1.Fill(_subtitle, _subtitleAlternate);
                        SubtitleListview1.SelectIndexAndEnsureVisible(0, true);
                    }
                    else if (list.Count > 1 && list.Count < 2000)
                    {
                        MakeHistoryForUndo(_language.BeforeInsertLine);
                        _makeHistoryPaused = true;
                        foreach (string line in list)
                        {
                            if (!string.IsNullOrWhiteSpace(line))
                            {
                                InsertAfter();
                                textBoxListViewText.Text = Utilities.AutoBreakLine(line);
                            }
                        }
                        RestartHistory();
                    }
                }
                e.SuppressKeyPress = true;
            }
            else if (e.KeyCode == Keys.X && e.Modifiers == Keys.Control) //Ctrl+X = Cut to clipboard
            {
                var tmp = new Subtitle();
                foreach (int i in SubtitleListview1.SelectedIndices)
                {
                    Paragraph p = _subtitle.GetParagraphOrDefault(i);
                    if (p != null)
                        tmp.Paragraphs.Add(new Paragraph(p));
                }
                e.SuppressKeyPress = true;
                _cutText = tmp.ToText(new SubRip());
                ToolStripMenuItemDeleteClick(null, null);
            }
            else if (e.KeyCode == Keys.A && e.Modifiers == Keys.Control) //SelectAll
            {
                foreach (ListViewItem item in SubtitleListview1.Items)
                    item.Selected = true;
                e.SuppressKeyPress = true;
            }
            else if (e.KeyCode == Keys.D && e.Modifiers == Keys.Control) //SelectFirstSelectedItemOnly
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
            else if (e.KeyData == _mainInsertBefore)
            {
                InsertBefore();
                e.SuppressKeyPress = true;
            }
            else if (e.KeyData == _mainInsertAfter)
            {
                InsertAfter();
                e.SuppressKeyPress = true;
            }
            else if (e.Modifiers == Keys.Control && e.KeyCode == Keys.Home)
            {
                SubtitleListview1.FirstVisibleIndex = -1;
                SubtitleListview1.SelectIndexAndEnsureVisible(0, true);
                e.SuppressKeyPress = true;
            }
            else if (e.Modifiers == Keys.Control && e.KeyCode == Keys.End)
            {
                SubtitleListview1.SelectIndexAndEnsureVisible(SubtitleListview1.Items.Count - 1, true);
                e.SuppressKeyPress = true;
            }
            else if (e.Modifiers == Keys.None && e.KeyCode == Keys.Enter)
            {
                SubtitleListview1_MouseDoubleClick(null, null);
            }
        }

        private void GoToNextSynaxError()
        {
            int idx = FirstSelectedIndex + 1;
            try
            {
                for (int i = idx; i < _subtitle.Paragraphs.Count - 1; i++)
                {
                    ListViewItem item = SubtitleListview1.Items[i];
                    if (item.SubItems[SubtitleListView.ColumnIndexDuration].BackColor == Configuration.Settings.Tools.ListViewSyntaxErrorColor ||
                        item.SubItems[SubtitleListView.ColumnIndexText].BackColor == Configuration.Settings.Tools.ListViewSyntaxErrorColor ||
                        item.SubItems[SubtitleListView.ColumnIndexStart].BackColor == Configuration.Settings.Tools.ListViewSyntaxErrorColor)
                    {
                        SubtitleListview1.SelectIndexAndEnsureVisible(i, true);
                        return;
                    }
                }
            }
            catch
            {
            }
        }

        private void RestartHistory()
        {
            _listViewTextUndoLast = null;
            _listViewTextUndoIndex = -1;
            _listViewTextTicks = -1;
            _listViewAlternateTextUndoLast = null;
            _listViewAlternateTextTicks = -1;
            _undoIndex = _subtitle.HistoryItems.Count - 1;
            _makeHistoryPaused = false;
        }

        private void AutoBalanceLinesAndAllow2PlusLines()
        {
            if (_subtitle.Paragraphs.Count > 0 && SubtitleListview1.SelectedItems.Count > 0)
            {
                MakeHistoryForUndo(_language.BeforeAutoBalanceSelectedLines);
                string language = Utilities.AutoDetectGoogleLanguage(_subtitle);
                string languageOriginal = string.Empty;
                if (_subtitleAlternate != null)
                    Utilities.AutoDetectGoogleLanguage(_subtitleAlternate);
                foreach (ListViewItem item in SubtitleListview1.SelectedItems)
                {
                    Paragraph p = _subtitle.GetParagraphOrDefault(item.Index);
                    if (p != null)
                    {
                        string s = Utilities.AutoBreakLineMoreThanTwoLines(p.Text, Configuration.Settings.General.SubtitleLineMaximumLength, language);
                        if (s != p.Text)
                        {
                            p.Text = s;
                            SubtitleListview1.SetText(item.Index, p.Text);
                        }
                        if (_subtitleAlternate != null)
                        {
                            Paragraph original = Utilities.GetOriginalParagraph(item.Index, p, _subtitleAlternate.Paragraphs);
                            if (original != null)
                            {
                                string s2 = Utilities.AutoBreakLineMoreThanTwoLines(original.Text, Configuration.Settings.General.SubtitleLineMaximumLength, languageOriginal);
                                if (s2 != original.Text)
                                {
                                    original.Text = s2;
                                    SubtitleListview1.SetAlternateText(item.Index, original.Text);
                                }
                            }
                        }
                    }
                }
                ShowSource();
                RefreshSelectedParagraph();
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
            var regex = new Regex(@"\b(\w+)\s+\1\b");
            _findHelper = new FindReplaceDialogHelper(FindType.RegEx, string.Format(_language.DoubleWordsViaRegEx, regex), new List<string>(), regex, string.Empty, 0, 0, _subtitleListViewIndex);

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
            if (descendingToolStripMenuItem.Checked)
                _subtitle.Paragraphs.Reverse();
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

        private void textCharssecToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SortSubtitle(SubtitleSortCriteria.TextCharactersPerSeconds, (sender as ToolStripItem).Text);
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
                else
                {
                    Configuration.Settings.Language = Language.Load(Path.Combine(Configuration.BaseDirectory, "Languages") + Path.DirectorySeparatorChar + cultureName + ".xml");
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
            if (_subtitleAlternate != null && _subtitleAlternateFileName != null)
                compareForm.Initialize(_subtitle, _fileName, _subtitleAlternate, _subtitleAlternateFileName);
            else
                compareForm.Initialize(_subtitle, _fileName, Configuration.Settings.Language.General.CurrentSubtitle);
            compareForm.Show(this);
        }

        private void ToolStripMenuItemAutoBreakLinesClick(object sender, EventArgs e)
        {
            if (IsSubtitleLoaded)
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
                    string language = Utilities.AutoDetectGoogleLanguage(_subtitle);
                    SubtitleListview1.BeginUpdate();
                    foreach (int index in SubtitleListview1.SelectedIndices)
                    {
                        Paragraph p = _subtitle.GetParagraphOrDefault(index);

                        int indexFixed = autoBreakUnbreakLines.FixedParagraphs.IndexOf(p);
                        if (indexFixed >= 0)
                        {
                            p.Text = Utilities.AutoBreakLine(p.Text, 5, autoBreakUnbreakLines.MergeLinesShorterThan, language);
                            SubtitleListview1.SetText(index, p.Text);
                            SubtitleListview1.SyntaxColorLine(_subtitle.Paragraphs, index, p);
                        }
                    }
                    SubtitleListview1.EndUpdate();
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
            if (IsSubtitleLoaded)
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
                            SubtitleListview1.SyntaxColorLine(_subtitle.Paragraphs, index, p);
                        }
                    }
                    SubtitleListview1.EndUpdate();
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
            _formPositionsAndSizes.SetPositionAndSize(multipleReplace);
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
                ShowStatus(string.Format(_language.NumberOfLinesReplacedX, multipleReplace.FixCount));
            }
            _formPositionsAndSizes.SavePositionAndSize(multipleReplace);
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
                    if (formSubRip.Languages.Count == 1 || showSubtitles.ShowDialog(this) == DialogResult.OK)
                    {
                        var formSubOcr = new VobSubOcr();
                        _formPositionsAndSizes.SetPositionAndSize(formSubOcr);
                        var subs = formSubRip.MergedVobSubPacks;
                        if (showSubtitles.SelectedVobSubMergedPacks != null)
                            subs = showSubtitles.SelectedVobSubMergedPacks;
                        formSubOcr.Initialize(subs, formSubRip.Palette, Configuration.Settings.VobSubOcr, formSubRip.SelectedLanguage);
                        if (formSubOcr.ShowDialog(this) == DialogResult.OK)
                        {
                            MakeHistoryForUndo(_language.BeforeImportingDvdSubtitle);
                            FileNew();
                            _subtitle.Paragraphs.Clear();
                            SetCurrentFormat(Configuration.Settings.General.DefaultSubtitleFormat);
                            _subtitle.WasLoadedWithFrameNumbers = false;
                            _subtitle.CalculateFrameNumbersFromTimeCodes(CurrentFrameRate);
                            foreach (Paragraph p in formSubOcr.SubtitleFromOcr.Paragraphs)
                            {
                                _subtitle.Paragraphs.Add(p);
                            }

                            ShowSource();
                            SubtitleListview1.Fill(_subtitle, _subtitleAlternate);
                            _subtitleListViewIndex = -1;
                            SubtitleListview1.FirstVisibleIndex = -1;
                            SubtitleListview1.SelectIndexAndEnsureVisible(0, true);

                            _fileName = string.Empty;
                            Text = Title;

                            Configuration.Settings.Save();
                        }
                        _formPositionsAndSizes.SavePositionAndSize(formSubOcr);
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
                    ImportAndOcrVobSubSubtitleNew(openFileDialog1.FileName, false);
                    openFileDialog1.InitialDirectory = Path.GetDirectoryName(openFileDialog1.FileName);
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
                GotoSubPositionAndPause(-0.5);
            }
            else if (Configuration.Settings.General.ListViewDoubleClickAction == 4)
            {
                GotoSubPositionAndPause(-1.0);
            }
            else if (Configuration.Settings.General.ListViewDoubleClickAction == 5)
            {
                if (AutoRepeatContinueOn)
                    PlayCurrent();
                else
                {
                    if (SubtitleListview1.SelectedItems.Count > 0)
                    {
                        int index = SubtitleListview1.SelectedItems[0].Index;

                        mediaPlayer.Pause();
                        double pos = _subtitle.Paragraphs[index].StartTime.TotalSeconds;
                        if (pos > 1)
                            mediaPlayer.CurrentPosition = (_subtitle.Paragraphs[index].StartTime.TotalSeconds) - 1.0;
                        else
                            mediaPlayer.CurrentPosition = _subtitle.Paragraphs[index].StartTime.TotalSeconds;
                        mediaPlayer.Play();
                    }
                }
            }
            else if (Configuration.Settings.General.ListViewDoubleClickAction == 6)
            {
                GotoSubPositionAndPause();
                textBoxListViewText.Focus();
            }
            else if (Configuration.Settings.General.ListViewDoubleClickAction == 7)
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

        private bool IsUnicode
        {
            get
            {
                var enc = GetCurrentEncoding();
                return enc == Encoding.UTF8 || enc == Encoding.Unicode || enc == Encoding.UTF7 || enc == Encoding.UTF32 || enc == Encoding.BigEndianUnicode;
            }
        }

        private void EditToolStripMenuItemDropDownOpening(object sender, EventArgs e)
        {
            toolStripMenuItemRtlUnicodeControlChars.Visible = IsUnicode;
            if (!IsUnicode || _subtitleListViewIndex == -1)
            {
                toolStripMenuItemInsertUnicodeCharacter.Visible = false;
                toolStripSeparatorInsertUnicodeCharacter.Visible = false;
            }
            else
            {
                if (toolStripMenuItemInsertUnicodeCharacter.DropDownItems.Count == 0)
                {
                    foreach (string s in Configuration.Settings.Tools.UnicodeSymbolsToInsert.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        toolStripMenuItemInsertUnicodeCharacter.DropDownItems.Add(s, null, InsertUnicodeGlyph);
                        if (Environment.OSVersion.Version.Major < 6 && Configuration.Settings.General.SubtitleFontName == Utilities.WinXP2KUnicodeFontName) // 6 == Vista/Win2008Server/Win7
                            toolStripMenuItemInsertUnicodeCharacter.DropDownItems[toolStripMenuItemInsertUnicodeCharacter.DropDownItems.Count - 1].Font = new Font(Utilities.WinXP2KUnicodeFontName, toolStripMenuItemInsertUnicodeSymbol.Font.Size);
                    }
                }
                toolStripMenuItemInsertUnicodeCharacter.Visible = toolStripMenuItemInsertUnicodeCharacter.DropDownItems.Count > 0;
                toolStripSeparatorInsertUnicodeCharacter.Visible = toolStripMenuItemInsertUnicodeCharacter.DropDownItems.Count > 0;
            }
            toolStripMenuItemUndo.Enabled = _subtitle != null && _subtitle.CanUndo && _undoIndex >= 0;
            toolStripMenuItemRedo.Enabled = _subtitle != null && _subtitle.CanUndo && _undoIndex < _subtitle.HistoryItems.Count - 1;
            showHistoryforUndoToolStripMenuItem.Enabled = _subtitle != null && _subtitle.CanUndo;
            toolStripMenuItemShowOriginalInPreview.Visible = SubtitleListview1.IsAlternateTextColumnVisible;

            if (_networkSession != null)
            {
                toolStripMenuItemUndo.Enabled = false;
                toolStripMenuItemRedo.Enabled = false;
                showHistoryforUndoToolStripMenuItem.Enabled = false;
            }
        }

        private void InsertUnicodeGlyph(object sender, EventArgs e)
        {
            var item = sender as ToolStripItem;
            if (item != null)
                PasteIntoActiveTextBox(item.Text);
        }

        private void ToolStripMenuItemAutoMergeShortLinesClick(object sender, EventArgs e)
        {
            if (IsSubtitleLoaded)
            {
                ReloadFromSourceView();
                var formMergeShortLines = new MergeShortLines();
                _formPositionsAndSizes.SetPositionAndSize(formMergeShortLines);
                formMergeShortLines.Initialize(_subtitle);
                if (formMergeShortLines.ShowDialog(this) == DialogResult.OK)
                {
                    MakeHistoryForUndo(_language.BeforeMergeShortLines);
                    _subtitle.Paragraphs.Clear();
                    foreach (Paragraph p in formMergeShortLines.MergedSubtitle.Paragraphs)
                        _subtitle.Paragraphs.Add(p);
                    ShowStatus(string.Format(_language.MergedShortLinesX, formMergeShortLines.NumberOfMerges));
                    SaveSubtitleListviewIndexes();
                    ShowSource();
                    SubtitleListview1.Fill(_subtitle, _subtitleAlternate);
                    RestoreSubtitleListviewIndexes();
                }
                _formPositionsAndSizes.SavePositionAndSize(formMergeShortLines);
            }
            else
            {
                MessageBox.Show(_language.NoSubtitleLoaded, Title, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void toolStripMenuItemAutoSplitLongLines_Click(object sender, EventArgs e)
        {
            if (IsSubtitleLoaded)
            {
                ReloadFromSourceView();
                var splitLongLines = new SplitLongLines();
                _formPositionsAndSizes.SetPositionAndSize(splitLongLines);
                splitLongLines.Initialize(_subtitle);
                if (splitLongLines.ShowDialog(this) == DialogResult.OK)
                {
                    MakeHistoryForUndo(_language.BeforeMergeShortLines);
                    _subtitle.Paragraphs.Clear();
                    foreach (Paragraph p in splitLongLines.SplittedSubtitle.Paragraphs)
                        _subtitle.Paragraphs.Add(p);
                    ShowStatus(string.Format(_language.MergedShortLinesX, splitLongLines.NumberOfSplits));
                    SaveSubtitleListviewIndexes();
                    ShowSource();
                    SubtitleListview1.Fill(_subtitle, _subtitleAlternate);
                    RestoreSubtitleListviewIndexes();
                }
                _formPositionsAndSizes.SavePositionAndSize(splitLongLines);
            }
            else
            {
                MessageBox.Show(_language.NoSubtitleLoaded, Title, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void SetMinimalDisplayTimeDifferenceToolStripMenuItemClick(object sender, EventArgs e)
        {
            if (!IsSubtitleLoaded)
            {
                MessageBox.Show(_language.NoSubtitleLoaded, Title, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            var setMinDisplayDiff = new SetMinimumDisplayTimeBetweenParagraphs();
            _formPositionsAndSizes.SetPositionAndSize(setMinDisplayDiff);
            setMinDisplayDiff.Initialize(_subtitle);
            if (setMinDisplayDiff.ShowDialog() == DialogResult.OK && setMinDisplayDiff.FixCount > 0)
            {
                MakeHistoryForUndo(_language.BeforeSetMinimumDisplayTimeBetweenParagraphs);
                _subtitle.Paragraphs.Clear();
                foreach (Paragraph p in setMinDisplayDiff.FixedSubtitle.Paragraphs)
                    _subtitle.Paragraphs.Add(p);
                _subtitle.CalculateFrameNumbersFromTimeCodesNoCheck(CurrentFrameRate);
                ShowStatus(string.Format(_language.XMinimumDisplayTimeBetweenParagraphsChanged, setMinDisplayDiff.FixCount));
                SaveSubtitleListviewIndexes();
                ShowSource();
                SubtitleListview1.Fill(_subtitle, _subtitleAlternate);
                RestoreSubtitleListviewIndexes();
            }
            _formPositionsAndSizes.SavePositionAndSize(setMinDisplayDiff);
        }

        private void ToolStripMenuItemImportTextClick(object sender, EventArgs e)
        {
            var importText = new ImportText();
            if (importText.ShowDialog(this) == DialogResult.OK)
            {
                if (ContinueNewOrExit())
                {
                    MakeHistoryForUndo(_language.BeforeImportText);
                    ResetSubtitle();
                    if (!string.IsNullOrEmpty(importText.VideoFileName))
                        OpenVideo(importText.VideoFileName);

                    ResetSubtitle();
                    _subtitleListViewIndex = -1;
                    _subtitle = importText.FixedSubtitle;
                    _subtitle.CalculateFrameNumbersFromTimeCodesNoCheck(CurrentFrameRate);
                    ShowStatus(_language.TextImported);
                    ShowSource();
                    SubtitleListview1.Fill(_subtitle, _subtitleAlternate);
                    SubtitleListview1.SelectIndexAndEnsureVisible(0, true);
                }
            }
        }

        private void toolStripMenuItemPointSync_Click(object sender, EventArgs e)
        {
            SyncPointsSync pointSync = new SyncPointsSync();
            pointSync.Initialize(_subtitle, _fileName, _videoFileName, _videoAudioTrackNumber);
            _formPositionsAndSizes.SetPositionAndSize(pointSync);
            mediaPlayer.Pause();
            if (pointSync.ShowDialog(this) == DialogResult.OK)
            {
                _subtitleListViewIndex = -1;
                MakeHistoryForUndo(_language.BeforePointSynchronization);
                _subtitle.Paragraphs.Clear();
                foreach (Paragraph p in pointSync.FixedSubtitle.Paragraphs)
                    _subtitle.Paragraphs.Add(p);
                _subtitle.CalculateFrameNumbersFromTimeCodesNoCheck(CurrentFrameRate);
                ShowStatus(_language.PointSynchronizationDone);
                ShowSource();
                SubtitleListview1.Fill(_subtitle, _subtitleAlternate);
            }
            Activate();
            _videoFileName = pointSync.VideoFileName;
            _formPositionsAndSizes.SavePositionAndSize(pointSync);
        }

        private void pointSyncViaOtherSubtitleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SyncPointsSync pointSync = new SyncPointsSync();
            openFileDialog1.Title = _language.OpenOtherSubtitle;
            openFileDialog1.FileName = string.Empty;
            openFileDialog1.Filter = Utilities.GetOpenDialogFilter();
            if (openFileDialog1.ShowDialog() == DialogResult.OK && File.Exists(openFileDialog1.FileName))
            {
                Subtitle sub = new Subtitle();
                string fileName = openFileDialog1.FileName;

                //TODO: Check for mkv etc
                if (Path.GetExtension(fileName).Equals(".sub", StringComparison.OrdinalIgnoreCase) && IsVobSubFile(fileName, false))
                {
                    MessageBox.Show("VobSub files not supported here");
                    return;
                }

                if (Path.GetExtension(fileName).Equals(".sup", StringComparison.OrdinalIgnoreCase))
                {
                    if (FileUtils.IsBluRaySup(fileName))
                    {
                        MessageBox.Show("Bluray sup files not supported here");
                        return;
                    }
                    else if (FileUtils.IsSpDvdSup(fileName))
                    {
                        MessageBox.Show("DVD sup files not supported here");
                        return;
                    }
                }

                if (Path.GetExtension(fileName).Equals(".mkv", StringComparison.OrdinalIgnoreCase) || Path.GetExtension(fileName).Equals(".mks", StringComparison.OrdinalIgnoreCase))
                {
                    Matroska mkv = new Matroska();
                    bool isValid = false;
                    bool hasConstantFrameRate = false;
                    double frameRate = 0;
                    int width = 0;
                    int height = 0;
                    double milliseconds = 0;
                    string videoCodec = string.Empty;
                    mkv.GetMatroskaInfo(fileName, ref isValid, ref hasConstantFrameRate, ref frameRate, ref width, ref height, ref milliseconds, ref videoCodec);
                    if (isValid)
                    {
                        var subtitleList = mkv.GetMatroskaSubtitleTracks(fileName, out isValid);
                        if (isValid)
                        {
                            if (subtitleList.Count == 0)
                            {
                                MessageBox.Show(_language.NoSubtitlesFound);
                                return;
                            }
                            else
                            {
                                if (subtitleList.Count > 1)
                                {
                                    MatroskaSubtitleChooser subtitleChooser = new MatroskaSubtitleChooser();
                                    subtitleChooser.Initialize(subtitleList);
                                    if (_loading)
                                    {
                                        subtitleChooser.Icon = (Icon)this.Icon.Clone();
                                        subtitleChooser.ShowInTaskbar = true;
                                        subtitleChooser.ShowIcon = true;
                                    }
                                    if (subtitleChooser.ShowDialog(this) == DialogResult.OK)
                                    {
                                        sub = LoadMatroskaSubtitleForSync(subtitleList[subtitleChooser.SelectedIndex], fileName);
                                    }
                                }
                                else
                                {
                                    sub = LoadMatroskaSubtitleForSync(subtitleList[0], fileName);
                                }
                            }
                        }
                    }
                }

                if (Path.GetExtension(fileName).Equals(".divx", StringComparison.OrdinalIgnoreCase) || Path.GetExtension(fileName).Equals(".avi", StringComparison.OrdinalIgnoreCase))
                {
                    MessageBox.Show("Divx files not supported here");
                    return;
                }

                var fi = new FileInfo(fileName);

                if ((Path.GetExtension(fileName).Equals(".mp4", StringComparison.OrdinalIgnoreCase) || Path.GetExtension(fileName).Equals(".m4v", StringComparison.OrdinalIgnoreCase) || Path.GetExtension(fileName).Equals(".3gp", StringComparison.OrdinalIgnoreCase))
                    && fi.Length > 10000)
                {
                    var mp4Parser = new Logic.Mp4.MP4Parser(fileName);
                    var mp4SubtitleTracks = mp4Parser.GetSubtitleTracks();
                    if (mp4SubtitleTracks.Count == 0)
                    {
                        MessageBox.Show(_language.NoSubtitlesFound);
                        return;
                    }
                    else if (mp4SubtitleTracks.Count == 1)
                    {
                        sub = LoadMp4SubtitleForSync(mp4SubtitleTracks[0]);
                    }
                    else
                    {
                        var subtitleChooser = new MatroskaSubtitleChooser();
                        subtitleChooser.Initialize(mp4SubtitleTracks);
                        if (subtitleChooser.ShowDialog(this) == DialogResult.OK)
                        {
                            sub = LoadMp4SubtitleForSync(mp4SubtitleTracks[0]);
                        }
                    }
                }

                if (fi.Length > 1024 * 1024 * 10 && sub.Paragraphs.Count == 0) // max 10 mb
                {
                    if (MessageBox.Show(this, string.Format(_language.FileXIsLargerThan10MB + Environment.NewLine +
                                                      Environment.NewLine +
                                                      _language.ContinueAnyway,
                                                      fileName), Title, MessageBoxButtons.YesNoCancel) != DialogResult.Yes)
                        return;
                }

                sub.Renumber(1);
                if (sub.Paragraphs.Count == 0)
                {
                    Encoding enc;
                    SubtitleFormat f = sub.LoadSubtitle(fileName, out enc, null);
                    if (f == null)
                    {
                        ShowUnknownSubtitle();
                        return;
                    }
                }

                pointSync.Initialize(_subtitle, _fileName, _videoFileName, _videoAudioTrackNumber, fileName, sub);
                mediaPlayer.Pause();
                if (pointSync.ShowDialog(this) == DialogResult.OK)
                {
                    _subtitleListViewIndex = -1;
                    MakeHistoryForUndo(_language.BeforePointSynchronization);
                    _subtitle.Paragraphs.Clear();
                    foreach (Paragraph p in pointSync.FixedSubtitle.Paragraphs)
                        _subtitle.Paragraphs.Add(p);
                    _subtitle.CalculateFrameNumbersFromTimeCodesNoCheck(CurrentFrameRate);
                    ShowStatus(_language.PointSynchronizationDone);
                    ShowSource();
                    SubtitleListview1.Fill(_subtitle, _subtitleAlternate);
                }
                _videoFileName = pointSync.VideoFileName;
            }
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
            openFileDialog1.Filter = Utilities.GetOpenDialogFilter();
            if (openFileDialog1.ShowDialog(this) == DialogResult.OK)
            {
                Encoding encoding;
                Subtitle timeCodeSubtitle = new Subtitle();
                SubtitleFormat format = timeCodeSubtitle.LoadSubtitle(openFileDialog1.FileName, out encoding, null);
                if (format == null)
                {
                    ShowUnknownSubtitle();
                    return;
                }

                if (timeCodeSubtitle.Paragraphs.Count != _subtitle.Paragraphs.Count && !string.IsNullOrEmpty(_language.ImportTimeCodesDifferentNumberOfLinesWarning))
                {
                    if (MessageBox.Show(string.Format(_language.ImportTimeCodesDifferentNumberOfLinesWarning, timeCodeSubtitle.Paragraphs.Count, _subtitle.Paragraphs.Count), _title, MessageBoxButtons.YesNo) == DialogResult.No)
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
            }

        }

        private void toolStripMenuItemTranslationMode_Click(object sender, EventArgs e)
        {
            if (SubtitleListview1.IsAlternateTextColumnVisible)
            {
                SubtitleListview1.HideAlternateTextColumn();
                SubtitleListview1.AutoSizeAllColumns(this);
                _subtitleAlternate = new Subtitle();
                _subtitleAlternateFileName = null;

                buttonUnBreak.Visible = true;
                buttonAutoBreak.Visible = true;
                textBoxListViewTextAlternate.Visible = false;
                labelAlternateText.Visible = false;
                labelAlternateCharactersPerSecond.Visible = false;
                labelTextAlternateLineLengths.Visible = false;
                labelAlternateSingleLine.Visible = false;
                labelTextAlternateLineTotal.Visible = false;
                textBoxListViewText.Width = (groupBoxEdit.Width - (textBoxListViewText.Left + 8 + buttonUnBreak.Width));
                textBoxListViewText.Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right;

                labelCharactersPerSecond.Left = textBoxListViewText.Left + (textBoxListViewText.Width - labelCharactersPerSecond.Width);
                labelTextLineTotal.Left = textBoxListViewText.Left + (textBoxListViewText.Width - labelTextLineTotal.Width);
            }
            else
            {
                OpenAlternateSubtitle();
            }
            SetTitle();
        }

        private void OpenAlternateSubtitle()
        {
            if (ContinueNewOrExitAlternate())
            {
                SaveSubtitleListviewIndexes();
                openFileDialog1.Title = Configuration.Settings.Language.General.OpenOriginalSubtitleFile;
                openFileDialog1.FileName = string.Empty;
                openFileDialog1.Filter = Utilities.GetOpenDialogFilter();
                if (openFileDialog1.ShowDialog(this) != DialogResult.OK)
                    return;

                if (!LoadAlternateSubtitleFile(openFileDialog1.FileName))
                    return;

                SubtitleListview1.Fill(_subtitle, _subtitleAlternate);
                RestoreSubtitleListviewIndexes();

                Configuration.Settings.RecentFiles.Add(_fileName, FirstVisibleIndex, FirstSelectedIndex, _videoFileName, _subtitleAlternateFileName);
                Configuration.Settings.Save();
                UpdateRecentFilesUI();
            }
        }

        private bool LoadAlternateSubtitleFile(string fileName)
        {
            if (!File.Exists(fileName))
                return false;

            if (Path.GetExtension(fileName).Equals(".sub", StringComparison.OrdinalIgnoreCase) && IsVobSubFile(fileName, false))
                return false;

            var fi = new FileInfo(fileName);
            if (fi.Length > 1024 * 1024 * 10) // max 10 mb
            {
                if (MessageBox.Show(string.Format(_language.FileXIsLargerThan10MB + Environment.NewLine +
                                                    Environment.NewLine +
                                                    _language.ContinueAnyway,
                                                    fileName), Title, MessageBoxButtons.YesNoCancel) != DialogResult.Yes)
                    return false;
            }

            Encoding encoding;
            _subtitleAlternate = new Subtitle();
            _subtitleAlternateFileName = fileName;
            SubtitleFormat format = _subtitleAlternate.LoadSubtitle(fileName, out encoding, null);

            if (format == null)
            {
                var ebu = new Ebu();
                if (ebu.IsMine(null, fileName))
                {
                    ebu.LoadSubtitle(_subtitleAlternate, null, fileName);
                    format = ebu;
                }
            }
            if (format == null)
            {
                var pac = new Pac();
                if (pac.IsMine(null, fileName))
                {
                    pac.BatchMode = true;
                    pac.LoadSubtitle(_subtitleAlternate, null, fileName);
                    format = pac;
                }
            }
            if (format == null)
            {
                var cavena890 = new Cavena890();
                if (cavena890.IsMine(null, fileName))
                {
                    cavena890.LoadSubtitle(_subtitleAlternate, null, fileName);
                    format = cavena890;
                }
            }
            if (format == null)
            {
                var spt = new Spt();
                if (spt.IsMine(null, fileName))
                {
                    spt.LoadSubtitle(_subtitleAlternate, null, fileName);
                    format = spt;
                }
            }
            if (format == null)
            {
                var cheetahCaption = new CheetahCaption();
                if (cheetahCaption.IsMine(null, fileName))
                {
                    cheetahCaption.LoadSubtitle(_subtitleAlternate, null, fileName);
                    format = cheetahCaption;
                }
            }
            if (format == null)
            {
                var capMakerPlus = new CapMakerPlus();
                if (capMakerPlus.IsMine(null, fileName))
                {
                    capMakerPlus.LoadSubtitle(_subtitleAlternate, null, fileName);
                    format = capMakerPlus;
                }
            }
            if (format == null)
            {
                var captionate = new Captionate();
                if (captionate.IsMine(null, fileName))
                {
                    captionate.LoadSubtitle(_subtitleAlternate, null, fileName);
                    format = captionate;
                }
            }
            if (format == null)
            {
                var ultech130 = new Ultech130();
                if (ultech130.IsMine(null, fileName))
                {
                    ultech130.LoadSubtitle(_subtitleAlternate, null, fileName);
                    format = ultech130;
                }
            }
            if (format == null)
            {
                var nciCaption = new NciCaption();
                if (nciCaption.IsMine(null, fileName))
                {
                    nciCaption.LoadSubtitle(_subtitleAlternate, null, fileName);
                    format = nciCaption;
                }
            }
            if (format == null)
            {
                var tsb4 = new TSB4();
                if (tsb4.IsMine(null, fileName))
                {
                    tsb4.LoadSubtitle(_subtitleAlternate, null, fileName);
                    format = tsb4;
                }
            }
            if (format == null)
            {
                var avidStl = new AvidStl();
                if (avidStl.IsMine(null, fileName))
                {
                    avidStl.LoadSubtitle(_subtitleAlternate, null, fileName);
                    format = avidStl;
                }
            }

            if (format == null)
                return false;

            if (format.IsFrameBased)
                _subtitleAlternate.CalculateTimeCodesFromFrameNumbers(CurrentFrameRate);
            else
                _subtitleAlternate.CalculateFrameNumbersFromTimeCodes(CurrentFrameRate);

            SetupAlternateEdit();
            return true;
        }

        private void SetupAlternateEdit()
        {
            if (Configuration.Settings.General.AllowEditOfOriginalSubtitle && _subtitleAlternate.Paragraphs.Count > 1)
            {
                InsertMissingParagraphs(_subtitle, _subtitleAlternate);
                InsertMissingParagraphs(_subtitleAlternate, _subtitle);

                buttonUnBreak.Visible = false;
                buttonAutoBreak.Visible = false;
                buttonSplitLine.Visible = false;

                textBoxListViewText.Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Bottom;
                textBoxListViewText.Width = (groupBoxEdit.Width - (textBoxListViewText.Left + 10)) / 2;
                textBoxListViewTextAlternate.Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Bottom;
                textBoxListViewTextAlternate.Left = textBoxListViewText.Left + textBoxListViewText.Width + 3;
                textBoxListViewTextAlternate.Width = textBoxListViewText.Width;
                textBoxListViewTextAlternate.Visible = true;
                labelAlternateText.Text = Configuration.Settings.Language.General.OriginalText;
                labelAlternateText.Visible = true;
                labelAlternateCharactersPerSecond.Visible = true;
                labelTextAlternateLineLengths.Visible = true;
                labelAlternateSingleLine.Visible = true;
                labelTextAlternateLineTotal.Visible = true;

                labelCharactersPerSecond.Left = textBoxListViewText.Left + (textBoxListViewText.Width - labelCharactersPerSecond.Width);
                labelTextLineTotal.Left = textBoxListViewText.Left + (textBoxListViewText.Width - labelTextLineTotal.Width);
                Main_Resize(null, null);
                _changeAlternateSubtitleToString = _subtitleAlternate.ToText(new SubRip()).Trim();

                SetTitle();
            }

            SubtitleListview1.ShowAlternateTextColumn(Configuration.Settings.Language.General.OriginalText);
            SubtitleListview1.AutoSizeAllColumns(this);
        }

        private static void InsertMissingParagraphs(Subtitle masterSubtitle, Subtitle insertIntoSubtitle)
        {
            int index = 0;
            foreach (Paragraph p in masterSubtitle.Paragraphs)
            {

                Paragraph insertParagraph = Utilities.GetOriginalParagraph(index, p, insertIntoSubtitle.Paragraphs);
                if (insertParagraph == null)
                {
                    insertParagraph = new Paragraph(p);
                    insertParagraph.Text = string.Empty;
                    insertIntoSubtitle.InsertParagraphInCorrectTimeOrder(insertParagraph);
                }
                index++;
            }
            insertIntoSubtitle.Renumber(1);
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private void OpenVideo(string fileName)
        {
            if (!string.IsNullOrEmpty(fileName) && File.Exists(fileName))
            {
                if (_loading)
                {
                    _videoFileName = fileName;
                    return;
                }

                FileInfo fi = new FileInfo(fileName);
                if (fi.Length < 1000)
                    return;

                ShowSubtitleTimer.Stop();
                Cursor = Cursors.WaitCursor;
                VideoFileName = fileName;
                if (mediaPlayer.VideoPlayer != null)
                {
                    mediaPlayer.Pause();
                    mediaPlayer.VideoPlayer.DisposeVideoPlayer();
                }
                _endSeconds = -1;

                _videoInfo = ShowVideoInfo(fileName);
                if (_videoInfo.FramesPerSecond > 0)
                    toolStripComboBoxFrameRate.Text = string.Format("{0:0.###}", _videoInfo.FramesPerSecond);

                Utilities.InitializeVideoPlayerAndContainer(fileName, _videoInfo, mediaPlayer, VideoLoaded, VideoEnded);
                mediaPlayer.ShowFullscreenButton = Configuration.Settings.General.VideoPlayerShowFullscreenButton;
                mediaPlayer.OnButtonClicked -= MediaPlayer_OnButtonClicked;
                mediaPlayer.OnButtonClicked += MediaPlayer_OnButtonClicked;
                mediaPlayer.Volume = 0;

                if (_videoInfo.VideoCodec != null)
                    labelVideoInfo.Text = Path.GetFileName(fileName) + " " + _videoInfo.Width + "x" + _videoInfo.Height + " " + _videoInfo.VideoCodec.Trim();
                else
                    labelVideoInfo.Text = Path.GetFileName(fileName) + " " + _videoInfo.Width + "x" + _videoInfo.Height;

                if (_videoInfo.FramesPerSecond > 0)
                    labelVideoInfo.Text = labelVideoInfo.Text + " " + string.Format("{0:0.0##}", _videoInfo.FramesPerSecond);

                string peakWaveFileName = GetPeakWaveFileName(fileName);
                string spectrogramFolder = GetSpectrogramFolder(fileName);
                if (File.Exists(peakWaveFileName))
                {
                    audioVisualizer.WavePeaks = new WavePeakGenerator(peakWaveFileName);
                    audioVisualizer.ResetSpectrogram();
                    audioVisualizer.InitializeSpectrogram(spectrogramFolder);
                    toolStripComboBoxWaveform_SelectedIndexChanged(null, null);
                    audioVisualizer.WavePeaks.GenerateAllSamples();
                    audioVisualizer.WavePeaks.Close();
                    SetWaveformPosition(0, 0, 0);
                    timerWaveform.Start();
                }
                Cursor = Cursors.Default;

                SetUndockedWindowsTitle();
                ShowSubtitleTimer.Start();
            }
        }

        private void MediaPlayer_OnButtonClicked(object sender, EventArgs e)
        {
            var pb = (PictureBox)(sender as PictureBox);
            if (pb != null && pb.Name == "_pictureBoxFullscreenOver")
            {
                if (_videoPlayerUndocked != null && !_videoPlayerUndocked.IsDisposed && _videoPlayerUndocked.IsFullscreen)
                    _videoPlayerUndocked.NoFullscreen();
                else
                    GoFullscreen();
            }
        }

        private void SetWaveformPosition(double startPositionSeconds, double currentVideoPositionSeconds, int subtitleIndex)
        {
            if (SubtitleListview1.IsAlternateTextColumnVisible && Configuration.Settings.General.ShowOriginalAsPreviewIfAvailable)
            {
                int index = -1;
                if (SubtitleListview1.SelectedItems.Count > 0 && _subtitle.Paragraphs.Count > 0)
                {
                    int i = SubtitleListview1.SelectedItems[0].Index;
                    var p = Utilities.GetOriginalParagraph(i, _subtitle.Paragraphs[i], _subtitleAlternate.Paragraphs);
                    index = _subtitleAlternate.GetIndex(p);
                }
                audioVisualizer.SetPosition(startPositionSeconds, _subtitleAlternate, currentVideoPositionSeconds, index, SubtitleListview1.SelectedIndices);
            }
            else
            {
                audioVisualizer.SetPosition(startPositionSeconds, _subtitle, currentVideoPositionSeconds, subtitleIndex, SubtitleListview1.SelectedIndices);
            }
        }

        private void VideoLoaded(object sender, EventArgs e)
        {
            mediaPlayer.Volume = Configuration.Settings.General.VideoPlayerDefaultVolume;
            timer1.Start();

            trackBarWaveformPosition.Maximum = (int)mediaPlayer.Duration;

            if (_videoLoadedGoToSubPosAndPause)
            {
                Application.DoEvents();
                _videoLoadedGoToSubPosAndPause = false;
                GotoSubPositionAndPause();
            }
            mediaPlayer.Pause();
        }

        private void VideoEnded(object sender, EventArgs e)
        {
            mediaPlayer.Pause();
        }

        private static VideoInfo ShowVideoInfo(string fileName)
        {
            return Utilities.GetVideoInfo(fileName);
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
            else if (fileNameNoExtension.Contains('.'))
            {
                fileNameNoExtension = fileNameNoExtension.Substring(0, fileNameNoExtension.LastIndexOf('.'));
                TryToFindAndOpenVideoFile(fileNameNoExtension);
            }
        }

        internal void GoBackSeconds(double seconds)
        {
            if (mediaPlayer.CurrentPosition > seconds)
                mediaPlayer.CurrentPosition -= seconds;
            else
                mediaPlayer.CurrentPosition = 0;
            ShowSubtitle();
        }

        private void ButtonStartHalfASecondBackClick(object sender, EventArgs e)
        {
            GoBackSeconds(0.5);
        }

        private void ButtonStartThreeSecondsBackClick(object sender, EventArgs e)
        {
            GoBackSeconds(3.0);
        }

        private void ButtonStartOneMinuteBackClick(object sender, EventArgs e)
        {
            GoBackSeconds(60);
        }

        private void ButtonStartHalfASecondAheadClick(object sender, EventArgs e)
        {
            GoBackSeconds(-0.5);
        }

        private void ButtonStartThreeSecondsAheadClick(object sender, EventArgs e)
        {
            GoBackSeconds(-3);
        }

        private void ButtonStartOneMinuteAheadClick(object sender, EventArgs e)
        {
            GoBackSeconds(-60);
        }

        private void ShowSubtitleTimerTick(object sender, EventArgs e)
        {
            ShowSubtitleTimer.Stop();
            int oldIndex = FirstSelectedIndex;
            int index = ShowSubtitle();
            if (index != -1 && checkBoxSyncListViewWithVideoWhilePlaying.Checked && oldIndex != index)
            {
                if ((DateTime.Now.Ticks - _lastTextKeyDownTicks) > 10000 * 700) // only if last typed char was entered > 700 milliseconds
                {
                    if (_endSeconds <= 0 || !checkBoxAutoRepeatOn.Checked)
                    {
                        if (!timerAutoDuration.Enabled && !mediaPlayer.IsPaused)
                        {
                            SubtitleListview1.BeginUpdate();
                            SubtitleListview1.SelectIndexAndEnsureVisible(index, true);
                            SubtitleListview1.EndUpdate();
                        }
                    }
                }
            }

            if (string.CompareOrdinal(_changeSubtitleToString, SerializeSubtitle(_subtitle)) != 0)
            {
                if (!Text.EndsWith('*'))
                    Text = Text.TrimEnd() + "*";
            }
            else
            {
                if (Text.EndsWith('*'))
                    Text = Text.TrimEnd('*').TrimEnd();
            }
            ShowSubtitleTimer.Start();
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
            mediaPlayer.Pause();

            int textHeight = splitContainerListViewAndText.Height - splitContainerListViewAndText.SplitterDistance;

            splitContainer1.Panel2Collapsed = true;
            splitContainerMain.Panel2Collapsed = true;
            Main_Resize(null, null);

            splitContainerListViewAndText.SplitterDistance = splitContainerListViewAndText.Height - textHeight;
        }

        private void ShowVideoPlayer()
        {
            if (_isVideoControlsUndocked)
            {
                ShowHideUndockedVideoControls();
            }
            else
            {
                if (toolStripButtonToggleVideo.Checked && toolStripButtonToggleWaveform.Checked)
                {
                    splitContainer1.Panel2Collapsed = false;
                    MoveVideoUp();
                }
                else
                {
                    splitContainer1.Panel2Collapsed = true;
                    MoveVideoDown();
                }

                splitContainerMain.Panel2Collapsed = false;
                if (toolStripButtonToggleVideo.Checked)
                {
                    if (audioVisualizer.Visible)
                    {
                        audioVisualizer.Left = tabControlButtons.Left + tabControlButtons.Width + 5;
                    }
                    else
                    {
                        panelVideoPlayer.Left = tabControlButtons.Left + tabControlButtons.Width + 5;
                    }
                }
                else if (audioVisualizer.Visible)
                {
                    audioVisualizer.Left = tabControlButtons.Left + tabControlButtons.Width + 5;
                }
                audioVisualizer.Width = groupBoxVideo.Width - (audioVisualizer.Left + 10);

                checkBoxSyncListViewWithVideoWhilePlaying.Left = tabControlButtons.Left + tabControlButtons.Width + 5;
                panelWaveformControls.Left = audioVisualizer.Left;
                trackBarWaveformPosition.Left = panelWaveformControls.Left + panelWaveformControls.Width + 5;
                trackBarWaveformPosition.Width = audioVisualizer.Left + audioVisualizer.Width - trackBarWaveformPosition.Left + 5;
            }

            if (mediaPlayer.VideoPlayer == null && !string.IsNullOrEmpty(_fileName))
                TryToFindAndOpenVideoFile(Path.Combine(Path.GetDirectoryName(_fileName), Path.GetFileNameWithoutExtension(_fileName)));
            Main_Resize(null, null);
        }

        private void ShowHideUndockedVideoControls()
        {
            if (_videoPlayerUndocked == null || _videoPlayerUndocked.IsDisposed)
                UnDockVideoPlayer();
            _videoPlayerUndocked.Visible = false;
            if (toolStripButtonToggleVideo.Checked)
            {
                _videoPlayerUndocked.Show(this);
                if (_videoPlayerUndocked.WindowState == FormWindowState.Minimized)
                    _videoPlayerUndocked.WindowState = FormWindowState.Normal;
            }

            if (_waveformUndocked == null || _waveformUndocked.IsDisposed)
                UnDockWaveform();
            _waveformUndocked.Visible = false;
            if (toolStripButtonToggleWaveform.Checked)
            {
                _waveformUndocked.Show(this);
                if (_waveformUndocked.WindowState == FormWindowState.Minimized)
                    _waveformUndocked.WindowState = FormWindowState.Normal;
            }

            if (toolStripButtonToggleVideo.Checked || toolStripButtonToggleWaveform.Checked)
            {
                if (_videoControlsUndocked == null || _videoControlsUndocked.IsDisposed)
                    UnDockVideoButtons();
                _videoControlsUndocked.Visible = false;
                _videoControlsUndocked.Show(this);
            }
            else
            {
                if (_videoControlsUndocked != null && !_videoControlsUndocked.IsDisposed)
                    _videoControlsUndocked.Visible = false;
            }
        }

        private void MoveVideoUp()
        {
            if (splitContainer1.Panel2.Controls.Count == 0)
            {
                var control = panelVideoPlayer;
                groupBoxVideo.Controls.Remove(control);
                splitContainer1.Panel2.Controls.Add(control);
            }
            panelVideoPlayer.Top = 0;
            panelVideoPlayer.Left = 0;
            panelVideoPlayer.Height = splitContainer1.Panel2.Height - 2;
            panelVideoPlayer.Width = splitContainer1.Panel2.Width - 2;
        }

        private void MoveVideoDown()
        {
            if (splitContainer1.Panel2.Controls.Count > 0)
            {
                var control = panelVideoPlayer;
                splitContainer1.Panel2.Controls.Clear();
                groupBoxVideo.Controls.Add(control);
            }
            panelVideoPlayer.Top = 32;
            panelVideoPlayer.Left = tabControlButtons.Left + tabControlButtons.Width + 5;
            panelVideoPlayer.Height = groupBoxVideo.Height - (panelVideoPlayer.Top + 5);
            panelVideoPlayer.Width = groupBoxVideo.Width - (panelVideoPlayer.Left + 5);
        }

        private void FixLargeFonts()
        {
            Graphics graphics = this.CreateGraphics();
            SizeF textSize = graphics.MeasureString(buttonPlayPrevious.Text, this.Font);
            if (textSize.Height > buttonPlayPrevious.Height - 4)
            {
                int newButtonHeight = (int)(textSize.Height + 7 + 0.5);
                Utilities.SetButtonHeight(this, newButtonHeight, 1);

                // List view
                SubtitleListview1.InitializeTimestampColumnWidths(this);
                const int adjustUp = 8;
                SubtitleListview1.Height = SubtitleListview1.Height - adjustUp;
                groupBoxEdit.Top = groupBoxEdit.Top - adjustUp;
                groupBoxEdit.Height = groupBoxEdit.Height + adjustUp;
                numericUpDownDuration.Left = timeUpDownStartTime.Left + timeUpDownStartTime.Width;
                numericUpDownDuration.Width = numericUpDownDuration.Width + 5;
                labelDuration.Left = numericUpDownDuration.Left - 3;
                labelAutoDuration.Left = labelDuration.Left - (labelAutoDuration.Width - 5);

                // Video controls - Create
                timeUpDownVideoPosition.Left = labelVideoPosition.Left + labelVideoPosition.Width;
                int buttonWidth = labelVideoPosition.Width + timeUpDownVideoPosition.Width;
                buttonInsertNewText.Width = buttonWidth;
                buttonBeforeText.Width = buttonWidth;
                buttonGotoSub.Width = buttonWidth;
                buttonSetStartTime.Width = buttonWidth;
                buttonSetEnd.Width = buttonWidth;
                int FKeyLeft = buttonInsertNewText.Left + buttonInsertNewText.Width;
                labelCreateF9.Left = FKeyLeft;
                labelCreateF10.Left = FKeyLeft;
                labelCreateF11.Left = FKeyLeft;
                labelCreateF12.Left = FKeyLeft;
                buttonForward1.Left = buttonInsertNewText.Left + buttonInsertNewText.Width - buttonForward1.Width;
                numericUpDownSec1.Width = buttonInsertNewText.Width - (numericUpDownSec1.Left + buttonForward1.Width);
                buttonForward2.Left = buttonInsertNewText.Left + buttonInsertNewText.Width - buttonForward2.Width;
                numericUpDownSec2.Width = buttonInsertNewText.Width - (numericUpDownSec2.Left + buttonForward2.Width);

                // Video controls - Adjust
                timeUpDownVideoPositionAdjust.Left = labelVideoPosition2.Left + labelVideoPosition2.Width;
                buttonSetStartAndOffsetRest.Width = buttonWidth;
                buttonSetEndAndGoToNext.Width = buttonWidth;
                buttonAdjustSetStartTime.Width = buttonWidth;
                buttonAdjustSetEndTime.Width = buttonWidth;
                buttonAdjustPlayBefore.Width = buttonWidth;
                buttonAdjustGoToPosAndPause.Width = buttonWidth;
                labelAdjustF9.Left = FKeyLeft;
                labelAdjustF10.Left = FKeyLeft;
                labelAdjustF11.Left = FKeyLeft;
                labelAdjustF12.Left = FKeyLeft;
                buttonAdjustSecForward1.Left = buttonInsertNewText.Left + buttonInsertNewText.Width - buttonAdjustSecForward1.Width;
                numericUpDownSecAdjust1.Width = buttonInsertNewText.Width - (numericUpDownSecAdjust2.Left + buttonAdjustSecForward1.Width);
                buttonAdjustSecForward2.Left = buttonInsertNewText.Left + buttonInsertNewText.Width - buttonAdjustSecForward2.Width;
                numericUpDownSecAdjust2.Width = buttonInsertNewText.Width - (numericUpDownSecAdjust2.Left + buttonAdjustSecForward2.Width);

                tabControl1_SelectedIndexChanged(null, null);
            }
        }

        private void Main_Resize(object sender, EventArgs e)
        {
            if (_loading)
                return;

            panelVideoPlayer.Invalidate();

            MainResize();

            // Due to strange bug in listview when maximizing
            SaveSubtitleListviewIndexes();
            SubtitleListview1.Fill(_subtitle, _subtitleAlternate);
            RestoreSubtitleListviewIndexes();

            panelVideoPlayer.Refresh();
        }

        private void MainResize()
        {
            if (Configuration.Settings.General.AllowEditOfOriginalSubtitle && _subtitleAlternate != null &&
                _subtitleAlternate.Paragraphs.Count > 0)
            {
                textBoxListViewText.Width = (groupBoxEdit.Width - (textBoxListViewText.Left + 10)) / 2;
                textBoxListViewTextAlternate.Left = textBoxListViewText.Left + textBoxListViewText.Width + 3;
                labelAlternateText.Left = textBoxListViewTextAlternate.Left;

                textBoxListViewTextAlternate.Width = textBoxListViewText.Width;

                labelAlternateCharactersPerSecond.Left = textBoxListViewTextAlternate.Left +
                                                         (textBoxListViewTextAlternate.Width -
                                                          labelAlternateCharactersPerSecond.Width);
                labelTextAlternateLineLengths.Left = textBoxListViewTextAlternate.Left;
                labelAlternateSingleLine.Left = labelTextAlternateLineLengths.Left + labelTextAlternateLineLengths.Width;
                labelTextAlternateLineTotal.Left = textBoxListViewTextAlternate.Left +
                                                   (textBoxListViewTextAlternate.Width - labelTextAlternateLineTotal.Width);
            }

            labelCharactersPerSecond.Left = textBoxListViewText.Left +
                                            (textBoxListViewText.Width - labelCharactersPerSecond.Width);
            labelTextLineTotal.Left = textBoxListViewText.Left + (textBoxListViewText.Width - labelTextLineTotal.Width);
            SubtitleListview1.AutoSizeAllColumns(this);
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
            if (mediaPlayer.VideoPlayer != null)
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
                ShowSubtitle();
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
                ShowSubtitle();
                _subtitleListViewIndex = newIndex;
                PlayCurrent();
            }
        }

        private void GotoSubtitleIndex(int index)
        {
            if (mediaPlayer.VideoPlayer != null && mediaPlayer.Duration > 0)
            {
                mediaPlayer.CurrentPosition = _subtitle.Paragraphs[index].StartTime.TotalSeconds;
            }
        }

        private void PlayPart(Paragraph paragraph)
        {
            if (mediaPlayer.VideoPlayer != null)
            {
                double startSeconds = paragraph.StartTime.TotalSeconds;
                if (startSeconds > 0.2)
                    startSeconds -= 0.2; // go a little back

                _endSeconds = paragraph.EndTime.TotalSeconds;
                if (mediaPlayer.Duration > _endSeconds + 0.2)
                    _endSeconds += 0.2; // go a little forward

                mediaPlayer.CurrentPosition = startSeconds;
                ShowSubtitle();
                mediaPlayer.Play();
            }
        }

        private void buttonSetStartTime_Click(object sender, EventArgs e)
        {
            SetStartTime(false);
        }

        private void SetStartTime(bool adjustEndTime)
        {
            if (SubtitleListview1.SelectedItems.Count == 1)
            {
                timeUpDownStartTime.MaskedTextBox.TextChanged -= MaskedTextBoxTextChanged;
                int index = SubtitleListview1.SelectedItems[0].Index;
                var p = _subtitle.Paragraphs[index];
                var oldParagraph = new Paragraph(p);
                if (oldParagraph.StartTime.IsMaxTime || oldParagraph.EndTime.IsMaxTime)
                    adjustEndTime = true;
                double videoPosition = mediaPlayer.CurrentPosition;
                if (!mediaPlayer.IsPaused)
                    videoPosition -= Configuration.Settings.General.SetStartEndHumanDelay / 1000.0;

                MakeHistoryForUndoOnlyIfNotResent(string.Format(_language.VideoControls.BeforeChangingTimeInWaveformX, "#" + p.Number + " " + p.Text));

                timeUpDownStartTime.TimeCode = TimeCode.FromSeconds(videoPosition);

                var duration = p.Duration.TotalMilliseconds;

                p.StartTime.TotalMilliseconds = videoPosition * 1000.0;
                if (adjustEndTime)
                    p.EndTime.TotalMilliseconds = p.StartTime.TotalMilliseconds + duration;
                if (oldParagraph.StartTime.IsMaxTime)
                    p.EndTime.TotalMilliseconds = p.StartTime.TotalMilliseconds + Utilities.GetOptimalDisplayMilliseconds(p.Text);

                SubtitleListview1.SetStartTimeAndDuration(index, p);
                timeUpDownStartTime.TimeCode = p.StartTime;
                timeUpDownStartTime.MaskedTextBox.TextChanged += MaskedTextBoxTextChanged;

                if (!adjustEndTime)
                    SetDurationInSeconds(p.Duration.TotalSeconds);

                UpdateOriginalTimeCodes(oldParagraph);
                if (IsFramesRelevant && CurrentFrameRate > 0)
                {
                    _subtitle.CalculateFrameNumbersFromTimeCodesNoCheck(CurrentFrameRate);
                    if (tabControlSubtitle.SelectedIndex == TabControlSourceView)
                        ShowSource();
                }
            }
        }

        private void buttonSetEndTime_Click(object sender, EventArgs e)
        {
            if (SubtitleListview1.SelectedItems.Count == 1)
            {
                int index = SubtitleListview1.SelectedItems[0].Index;
                double videoPosition = mediaPlayer.CurrentPosition;
                if (!mediaPlayer.IsPaused)
                    videoPosition -= Configuration.Settings.General.SetStartEndHumanDelay / 1000.0;

                MakeHistoryForUndoOnlyIfNotResent(string.Format(_language.VideoControls.BeforeChangingTimeInWaveformX, "#" + _subtitle.Paragraphs[index].Number + " " + _subtitle.Paragraphs[index].Text));

                _subtitle.Paragraphs[index].EndTime = TimeCode.FromSeconds(videoPosition);
                SubtitleListview1.SetStartTimeAndDuration(index, _subtitle.Paragraphs[index]);

                if (index + 1 < _subtitle.Paragraphs.Count)
                {
                    SubtitleListview1.SelectedItems[0].Selected = false;
                    SubtitleListview1.Items[index + 1].Selected = true;
                    _subtitle.Paragraphs[index + 1].StartTime = TimeCode.FromSeconds(videoPosition);
                    SubtitleListview1.AutoScrollOffset.Offset(0, index * 16);
                    SubtitleListview1.EnsureVisible(Math.Min(SubtitleListview1.Items.Count - 1, index + 5));
                }
                else
                {
                    SetDurationInSeconds(_subtitle.Paragraphs[index].Duration.TotalSeconds);
                }
                if (IsFramesRelevant && CurrentFrameRate > 0)
                {
                    _subtitle.CalculateFrameNumbersFromTimeCodesNoCheck(CurrentFrameRate);
                    if (tabControlSubtitle.SelectedIndex == TabControlSourceView)
                        ShowSource();
                }
            }
        }

        private void ButtonSetEndClick(object sender, EventArgs e)
        {
            if (SubtitleListview1.SelectedItems.Count == 1)
            {
                double videoPosition = mediaPlayer.CurrentPosition;
                if (!mediaPlayer.IsPaused)
                    videoPosition -= Configuration.Settings.General.SetStartEndHumanDelay / 1000.0;

                int index = SubtitleListview1.SelectedItems[0].Index;
                MakeHistoryForUndoOnlyIfNotResent(string.Format(_language.VideoControls.BeforeChangingTimeInWaveformX, "#" + _subtitle.Paragraphs[index].Number + " " + _subtitle.Paragraphs[index].Text));

                if (_subtitle.Paragraphs[index].StartTime.IsMaxTime)
                {
                    timeUpDownStartTime.MaskedTextBox.TextChanged -= MaskedTextBoxTextChanged;
                    _subtitle.Paragraphs[index].EndTime.TotalSeconds = videoPosition;
                    _subtitle.Paragraphs[index].StartTime.TotalMilliseconds = _subtitle.Paragraphs[index].EndTime.TotalMilliseconds - Utilities.GetOptimalDisplayMilliseconds(_subtitle.Paragraphs[index].Text);
                    if (_subtitle.Paragraphs[index].StartTime.TotalMilliseconds < 0)
                        _subtitle.Paragraphs[index].StartTime.TotalMilliseconds = 0;
                    timeUpDownStartTime.TimeCode = _subtitle.Paragraphs[index].StartTime;
                    SubtitleListview1.SetStartTime(index, _subtitle.Paragraphs[index]);
                    timeUpDownStartTime.MaskedTextBox.TextChanged += MaskedTextBoxTextChanged;
                }
                else
                {
                    _subtitle.Paragraphs[index].EndTime = TimeCode.FromSeconds(videoPosition);
                    if (_subtitle.Paragraphs[index].Duration.TotalMilliseconds < Configuration.Settings.General.SubtitleMinimumDisplayMilliseconds)
                        _subtitle.Paragraphs[index].Duration.TotalMilliseconds = Configuration.Settings.General.SubtitleMinimumDisplayMilliseconds;
                }
                SubtitleListview1.SetStartTimeAndDuration(index, _subtitle.Paragraphs[index]);
                SetDurationInSeconds(_subtitle.Paragraphs[index].Duration.TotalSeconds);
            }
        }

        private void ButtonInsertNewTextClick(object sender, EventArgs e)
        {
            mediaPlayer.Pause();

            var newParagraph = InsertNewTextAtVideoPosition();

            textBoxListViewText.Focus();
            timerAutoDuration.Start();

            ShowStatus(string.Format(_language.VideoControls.NewTextInsertAtX, newParagraph.StartTime.ToShortString()));
        }

        private Paragraph InsertNewTextAtVideoPosition()
        {
            // current movie Position
            double videoPositionInMilliseconds = mediaPlayer.CurrentPosition * 1000.0;
            if (!mediaPlayer.IsPaused)
                videoPositionInMilliseconds -= Configuration.Settings.General.SetStartEndHumanDelay;

            var tc = new TimeCode(videoPositionInMilliseconds);
            MakeHistoryForUndo(_language.BeforeInsertSubtitleAtVideoPosition + "  " + tc);

            // find index where to insert
            int index = 0;
            foreach (Paragraph p in _subtitle.Paragraphs)
            {
                if (p.StartTime.TotalMilliseconds > videoPositionInMilliseconds)
                    break;
                index++;
            }

            // create and insert
            var newParagraph = new Paragraph("", videoPositionInMilliseconds, videoPositionInMilliseconds + Configuration.Settings.General.NewEmptyDefaultMs);
            if (GetCurrentSubtitleFormat().IsFrameBased)
            {
                newParagraph.CalculateFrameNumbersFromTimeCodes(CurrentFrameRate);
                newParagraph.CalculateTimeCodesFromFrameNumbers(CurrentFrameRate);
            }

            if (_networkSession != null)
            {
                _networkSession.TimerStop();
                NetworkGetSendUpdates(new List<int>(), index, newParagraph);
            }
            else
            {
                _subtitle.Paragraphs.Insert(index, newParagraph);

                // check if original is available - and insert new paragraph in the original too
                if (Configuration.Settings.General.AllowEditOfOriginalSubtitle && _subtitleAlternate != null && _subtitleAlternate.Paragraphs.Count > 0)
                {
                    _subtitleAlternate.InsertParagraphInCorrectTimeOrder(new Paragraph(newParagraph));
                    _subtitleAlternate.Renumber(1);
                }

                _subtitleListViewIndex = -1;
                _subtitle.Renumber(1);
                SubtitleListview1.Fill(_subtitle, _subtitleAlternate);
            }
            SubtitleListview1.SelectIndexAndEnsureVisible(index);
            return newParagraph;
        }

        private void timerAutoDuration_Tick(object sender, EventArgs e)
        {
            labelAutoDuration.Visible = !labelAutoDuration.Visible;
            double duration = Utilities.GetOptimalDisplayMilliseconds(textBoxListViewText.Text);
            SetDurationInSeconds(duration / 1000.0);

            // update _subtitle + listview
            if (SubtitleListview1.SelectedItems.Count > 0)
            {
                try
                {
                    int firstSelectedIndex = SubtitleListview1.SelectedItems[0].Index;
                    Paragraph currentParagraph = _subtitle.Paragraphs[firstSelectedIndex];
                    currentParagraph.EndTime.TotalMilliseconds = currentParagraph.StartTime.TotalMilliseconds + duration;
                    SubtitleListview1.SetDuration(firstSelectedIndex, currentParagraph);
                }
                catch
                {
                }
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
            GotoSubPositionAndPause(0);
        }

        private void GotoSubPositionAndPause(double adjustSeconds)
        {
            if (SubtitleListview1.SelectedItems.Count > 0)
            {
                int index = SubtitleListview1.SelectedItems[0].Index;
                if (index == -1)
                    return;

                mediaPlayer.Pause();
                if (_subtitle.Paragraphs[index].StartTime.IsMaxTime)
                    return;

                double newPos = _subtitle.Paragraphs[index].StartTime.TotalSeconds + adjustSeconds;
                if (newPos < 0)
                    newPos = 0;
                mediaPlayer.CurrentPosition = newPos;
                ShowSubtitle();

                double startPos = mediaPlayer.CurrentPosition - 1;
                if (startPos < 0)
                    startPos = 0;

                SetWaveformPosition(startPos, mediaPlayer.CurrentPosition, index);
            }
        }

        private void buttonGotoSub_Click(object sender, EventArgs e)
        {
            GotoSubPositionAndPause();
        }

        private void buttonOpenVideo_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(openFileDialog1.InitialDirectory) && !string.IsNullOrEmpty(_fileName))
                openFileDialog1.InitialDirectory = Path.GetDirectoryName(_fileName);
            openFileDialog1.Title = Configuration.Settings.Language.General.OpenVideoFileTitle;
            openFileDialog1.FileName = string.Empty;
            openFileDialog1.Filter = Utilities.GetVideoFileFilter(true);

            openFileDialog1.FileName = string.Empty;
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                mediaPlayer.Offset = 0;
                if (audioVisualizer != null)
                    audioVisualizer.Offset = 0;

                if (audioVisualizer.WavePeaks != null)
                {
                    audioVisualizer.WavePeaks = null;
                    audioVisualizer.ResetSpectrogram();
                    audioVisualizer.Invalidate();
                }
                openFileDialog1.InitialDirectory = Path.GetDirectoryName(openFileDialog1.FileName);
                if (!panelVideoPlayer.Visible)
                    toolStripButtonToggleVideo_Click(null, null);
                OpenVideo(openFileDialog1.FileName);
            }
        }

        private void toolStripButtonToggleVideo_Click(object sender, EventArgs e)
        {
            toolStripButtonToggleVideo.Checked = !toolStripButtonToggleVideo.Checked;
            panelVideoPlayer.Visible = toolStripButtonToggleVideo.Checked;
            mediaPlayer.BringToFront();
            if (!toolStripButtonToggleVideo.Checked && !toolStripButtonToggleWaveform.Checked)
            {
                if (_isVideoControlsUndocked)
                    ShowHideUndockedVideoControls();
                else
                    HideVideoPlayer();
            }
            else
            {
                ShowVideoPlayer();
            }
            Configuration.Settings.General.ShowVideoPlayer = toolStripButtonToggleVideo.Checked;
            if (!_loading)
                Refresh();
        }

        private void toolStripButtonToggleWaveform_Click(object sender, EventArgs e)
        {
            toolStripButtonToggleWaveform.Checked = !toolStripButtonToggleWaveform.Checked;
            audioVisualizer.Visible = toolStripButtonToggleWaveform.Checked;
            trackBarWaveformPosition.Visible = toolStripButtonToggleWaveform.Checked;
            panelWaveformControls.Visible = toolStripButtonToggleWaveform.Checked;
            if (!toolStripButtonToggleWaveform.Checked && !toolStripButtonToggleVideo.Checked)
            {
                if (_isVideoControlsUndocked)
                    ShowHideUndockedVideoControls();
                else
                    HideVideoPlayer();
            }
            else
            {
                ShowVideoPlayer();
            }
            Configuration.Settings.General.ShowAudioVisualizer = toolStripButtonToggleWaveform.Checked;
            Refresh();
        }

        public void ShowEarlierOrLater(double adjustMilliseconds, SelectionChoice selection)
        {
            var tc = new TimeCode(adjustMilliseconds);
            MakeHistoryForUndo(_language.BeforeShowSelectedLinesEarlierLater + ": " + tc);
            if (adjustMilliseconds < 0)
            {
                if (selection == SelectionChoice.AllLines)
                    ShowStatus(string.Format(_language.ShowAllLinesXSecondsLinesEarlier, adjustMilliseconds / -1000.0));
                else if (selection == SelectionChoice.SelectionOnly)
                    ShowStatus(string.Format(_language.ShowSelectedLinesXSecondsLinesEarlier, adjustMilliseconds / -1000.0));
                else if (selection == SelectionChoice.SelectionAndForward)
                    ShowStatus(string.Format(_language.ShowSelectionAndForwardXSecondsLinesEarlier, adjustMilliseconds / -1000.0));
            }
            else
            {
                if (selection == SelectionChoice.AllLines)
                    ShowStatus(string.Format(_language.ShowAllLinesXSecondsLinesLater, adjustMilliseconds / 1000.0));
                else if (selection == SelectionChoice.SelectionOnly)
                    ShowStatus(string.Format(_language.ShowSelectedLinesXSecondsLinesLater, adjustMilliseconds / 1000.0));
                else if (selection == SelectionChoice.SelectionAndForward)
                    ShowStatus(string.Format(_language.ShowSelectionAndForwardXSecondsLinesLater, adjustMilliseconds / 1000.0));
            }

            double frameRate = CurrentFrameRate;
            SubtitleListview1.BeginUpdate();

            int startFrom = 0;
            if (selection == SelectionChoice.SelectionAndForward)
            {
                if (SubtitleListview1.SelectedItems.Count > 0)
                    startFrom = SubtitleListview1.SelectedItems[0].Index;
                else
                    startFrom = _subtitle.Paragraphs.Count;
            }

            for (int i = startFrom; i < _subtitle.Paragraphs.Count; i++)
            {
                switch (selection)
                {
                    case SelectionChoice.SelectionOnly:
                        if (SubtitleListview1.Items[i].Selected)
                            ShowEarlierOrLaterParagraph(adjustMilliseconds, i);
                        break;
                    case SelectionChoice.AllLines:
                        ShowEarlierOrLaterParagraph(adjustMilliseconds, i);
                        break;
                    case SelectionChoice.SelectionAndForward:
                        ShowEarlierOrLaterParagraph(adjustMilliseconds, i);
                        break;
                }
            }

            SubtitleListview1.EndUpdate();
            if (_subtitle.WasLoadedWithFrameNumbers)
                _subtitle.CalculateFrameNumbersFromTimeCodesNoCheck(frameRate);
            RefreshSelectedParagraph();
            UpdateSourceView();
            UpdateListSyntaxColoring();
        }

        private void ShowEarlierOrLaterParagraph(double adjustMilliseconds, int i)
        {
            Paragraph p = _subtitle.GetParagraphOrDefault(i);
            if (p != null && !p.StartTime.IsMaxTime)
            {
                if (_subtitleAlternate != null)
                {
                    Paragraph original = Utilities.GetOriginalParagraph(i, p, _subtitleAlternate.Paragraphs);
                    if (original != null)
                    {
                        original.StartTime.TotalMilliseconds += adjustMilliseconds;
                        original.EndTime.TotalMilliseconds += adjustMilliseconds;
                    }
                }

                p.StartTime.TotalMilliseconds += adjustMilliseconds;
                p.EndTime.TotalMilliseconds += adjustMilliseconds;
                SubtitleListview1.SetStartTime(i, p);
            }
        }

        private void UpdateSourceView()
        {
            if (tabControlSubtitle.SelectedIndex == TabControlSourceView)
                ShowSource();
        }

        private void toolStripMenuItemAdjustAllTimes_Click(object sender, EventArgs e)
        {
            if (SubtitleListview1.SelectedItems.Count > 1)
            {
                ShowSelectedLinesEarlierlaterToolStripMenuItemClick(null, null);
            }
            else
            {
                if (IsSubtitleLoaded)
                {
                    mediaPlayer.Pause();

                    if (_showEarlierOrLater != null && !_showEarlierOrLater.IsDisposed)
                    {
                        _showEarlierOrLater.WindowState = FormWindowState.Normal;
                        _showEarlierOrLater.Focus();
                        return;
                    }

                    _showEarlierOrLater = new ShowEarlierLater();
                    if (!_formPositionsAndSizes.SetPositionAndSize(_showEarlierOrLater))
                    {
                        _showEarlierOrLater.Top = this.Top + 100;
                        _showEarlierOrLater.Left = this.Left + (this.Width / 2) - (_showEarlierOrLater.Width / 3);
                    }
                    SaveSubtitleListviewIndexes();
                    _showEarlierOrLater.Initialize(ShowEarlierOrLater, _formPositionsAndSizes, false);
                    _showEarlierOrLater.Show(this);
                }
                else
                {
                    MessageBox.Show(_language.NoSubtitleLoaded, Title, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (mediaPlayer.VideoPlayer != null)
            {
                if (!mediaPlayer.IsPaused)
                {
                    timeUpDownVideoPosition.Enabled = false;
                    timeUpDownVideoPositionAdjust.Enabled = false;

                    if (_endSeconds >= 0 && mediaPlayer.CurrentPosition > _endSeconds && !AutoRepeatContinueOn)
                    {
                        mediaPlayer.Pause();
                        mediaPlayer.CurrentPosition = _endSeconds + EndDelay;
                        _endSeconds = -1;
                    }

                    if (AutoRepeatContinueOn)
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
                                if (_subtitleListViewIndex >= 0 && _subtitleListViewIndex < _subtitle.Paragraphs.Count)
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
                int index = ShowSubtitle();

                double pos = mediaPlayer.CurrentPosition * 1000.0;
                if (!timeUpDownVideoPosition.MaskedTextBox.Focused && timeUpDownVideoPosition.TimeCode.TotalMilliseconds != pos)
                    timeUpDownVideoPosition.TimeCode = new TimeCode(pos);

                if (!timeUpDownVideoPositionAdjust.MaskedTextBox.Focused && timeUpDownVideoPositionAdjust.TimeCode.TotalMilliseconds != pos)
                    timeUpDownVideoPositionAdjust.TimeCode = new TimeCode(pos);

                mediaPlayer.RefreshProgressBar();

                trackBarWaveformPosition.ValueChanged -= trackBarWaveformPosition_ValueChanged;
                int value = (int)mediaPlayer.CurrentPosition;
                if (value > trackBarWaveformPosition.Maximum)
                    value = trackBarWaveformPosition.Maximum;
                if (value < trackBarWaveformPosition.Minimum)
                    value = trackBarWaveformPosition.Minimum;
                trackBarWaveformPosition.Value = value;
                trackBarWaveformPosition.ValueChanged += trackBarWaveformPosition_ValueChanged;
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
            if (AutoRepeatContinueOn && !textBoxSearchWord.Focused && textBoxListViewText.Focused)
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
            textBoxListViewText.ClearUndo();
            UpdatePositionAndTotalLength(labelTextLineTotal, textBoxListViewText);
        }

        private void buttonGoogleIt_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("http://www.google.com/search?q=" + Utilities.UrlEncode(textBoxSearchWord.Text));
        }

        private void buttonGoogleTranslateIt_Click(object sender, EventArgs e)
        {
            string languageId = Utilities.AutoDetectGoogleLanguage(_subtitle);
            System.Diagnostics.Process.Start("http://translate.google.com/#auto|" + languageId + "|" + Utilities.UrlEncode(textBoxSearchWord.Text));
        }

        private void ButtonPlayCurrentClick(object sender, EventArgs e)
        {
            PlayCurrent();
        }

        private void buttonPlayNext_Click(object sender, EventArgs e)
        {
            Next();
        }

        private void buttonPlayPrevious_Click(object sender, EventArgs e)
        {
            PlayPrevious();
        }

        private void buttonStop_Click(object sender, EventArgs e)
        {
            _endSeconds = -1;
            timerAutoContinue.Stop();
            mediaPlayer.Pause();
            labelStatus.Text = string.Empty;
        }

        private void fileToolStripMenuItem_DropDownOpening(object sender, EventArgs e)
        {
            toolStripMenuItemOpenContainingFolder.Visible = !string.IsNullOrEmpty(_fileName) && File.Exists(_fileName);
            bool subtitleLoaded = IsSubtitleLoaded;
            toolStripMenuItemStatistics.Visible = subtitleLoaded;
            toolStripSeparator22.Visible = subtitleLoaded;
            toolStripMenuItemExport.Visible = subtitleLoaded;
            openOriginalToolStripMenuItem.Visible = subtitleLoaded;
            toolStripMenuItemOpenKeepVideo.Visible = _videoFileName != null;
            if (subtitleLoaded && Configuration.Settings.General.AllowEditOfOriginalSubtitle && _subtitleAlternate != null && _subtitleAlternate.Paragraphs.Count > 0)
            {
                saveOriginalToolStripMenuItem.Visible = true;
                saveOriginalAstoolStripMenuItem.Visible = true;
                removeOriginalToolStripMenuItem.Visible = true;
            }
            else
            {
                saveOriginalToolStripMenuItem.Visible = false;
                saveOriginalAstoolStripMenuItem.Visible = false;
                if (subtitleLoaded && SubtitleListview1.IsAlternateTextColumnVisible && _subtitleAlternate != null && _subtitleAlternate.Paragraphs.Count > 0)
                    removeOriginalToolStripMenuItem.Visible = true;
                else
                    removeOriginalToolStripMenuItem.Visible = false;
            }
            var format = GetCurrentSubtitleFormat();
            if (format.GetType() == typeof(AdvancedSubStationAlpha))
            {
                toolStripMenuItemSubStationAlpha.Visible = true;
                toolStripMenuItemSubStationAlpha.Text = Configuration.Settings.Language.Main.Menu.File.AdvancedSubStationAlphaProperties;
            }
            else if (format.GetType() == typeof(SubStationAlpha))
            {
                toolStripMenuItemSubStationAlpha.Visible = true;
                toolStripMenuItemSubStationAlpha.Text = Configuration.Settings.Language.Main.Menu.File.SubStationAlphaProperties;
            }
            else
            {
                toolStripMenuItemSubStationAlpha.Visible = false;
            }

            if (format.GetType() == typeof(Ebu))
            {
                toolStripMenuItemEbuProperties.Text = Configuration.Settings.Language.Main.Menu.File.EbuProperties;
                toolStripMenuItemEbuProperties.Visible = !string.IsNullOrEmpty(Configuration.Settings.Language.Main.Menu.File.EbuProperties);
            }
            else
            {
                toolStripMenuItemEbuProperties.Visible = false;
            }

            if (format.GetType() == typeof(DCSubtitle) || format.GetType() == typeof(DCinemaSmpte2010) || format.GetType() == typeof(DCinemaSmpte2007))
            {
                toolStripMenuItemDCinemaProperties.Visible = true;
            }
            else
            {
                toolStripMenuItemDCinemaProperties.Visible = false;
            }

            if (format.GetType() == typeof(TimedText10) || format.GetType() == typeof(ItunesTimedText))
            {
                toolStripMenuItemTTProperties.Visible = true;
            }
            else
            {
                toolStripMenuItemTTProperties.Visible = false;
            }

            toolStripMenuItemNuendoProperties.Visible = format.Name == "Nuendo";
            toolStripMenuItemFcpProperties.Visible = format.GetType() == typeof(FinalCutProXml);

            toolStripSeparator20.Visible = subtitleLoaded;

            toolStripMenuItemImportXSub.Visible = !string.IsNullOrEmpty(_language.OpenXSubFiles) && !string.IsNullOrEmpty(_language.XSubFiles) && !string.IsNullOrEmpty(_language.Menu.File.ImportXSub); //TODO: remove in 3.4

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

            if (!_isVideoControlsUndocked)
            {
                if (toolStripButtonToggleWaveform.Checked)
                    audioVisualizer.Left = tabControlButtons.Left + tabControlButtons.Width + 5;
                if (!toolStripButtonToggleWaveform.Checked && toolStripButtonToggleVideo.Checked)
                {
                    panelVideoPlayer.Left = tabControlButtons.Left + tabControlButtons.Width + 5;
                    panelVideoPlayer.Width = groupBoxVideo.Width - (panelVideoPlayer.Left + 10);
                }

                audioVisualizer.Width = groupBoxVideo.Width - (audioVisualizer.Left + 10);
                panelWaveformControls.Left = audioVisualizer.Left;
                trackBarWaveformPosition.Left = panelWaveformControls.Left + panelWaveformControls.Width + 5;
                trackBarWaveformPosition.Width = groupBoxVideo.Width - (trackBarWaveformPosition.Left + 10);
                Main_Resize(null, null);
                checkBoxSyncListViewWithVideoWhilePlaying.Left = tabControlButtons.Left + tabControlButtons.Width + 5;
                if (!_loading)
                    Refresh();
            }
            else if (_videoControlsUndocked != null && !_videoControlsUndocked.IsDisposed)
            {
                _videoControlsUndocked.Width = tabControlButtons.Width + 20;
                _videoControlsUndocked.Height = tabControlButtons.Height + 65;
            }
        }

        private void buttonSecBack1_Click(object sender, EventArgs e)
        {
            GoBackSeconds((double)numericUpDownSec1.Value);
        }

        private void buttonForward1_Click(object sender, EventArgs e)
        {
            GoBackSeconds(-(double)numericUpDownSec1.Value);
        }

        private void ButtonSetStartAndOffsetRestClick(object sender, EventArgs e)
        {
            if (SubtitleListview1.SelectedItems.Count == 1)
            {
                bool oldSync = checkBoxSyncListViewWithVideoWhilePlaying.Checked;
                checkBoxSyncListViewWithVideoWhilePlaying.Checked = false;

                timeUpDownStartTime.MaskedTextBox.TextChanged -= MaskedTextBoxTextChanged;
                int index = SubtitleListview1.SelectedItems[0].Index;
                double videoPosition = mediaPlayer.CurrentPosition;
                if (!mediaPlayer.IsPaused)
                    videoPosition -= Configuration.Settings.General.SetStartEndHumanDelay / 1000.0;
                var tc = TimeCode.FromSeconds(videoPosition);
                timeUpDownStartTime.TimeCode = tc;

                MakeHistoryForUndo(_language.BeforeSetStartTimeAndOffsetTheRest + @"  " + _subtitle.Paragraphs[index].Number + @" - " + tc);

                double offset = _subtitle.Paragraphs[index].StartTime.TotalMilliseconds - tc.TotalMilliseconds;

                if (_subtitle.Paragraphs[index].StartTime.IsMaxTime)
                {
                    _subtitle.Paragraphs[index].StartTime.TotalSeconds = videoPosition;
                    _subtitle.Paragraphs[index].EndTime.TotalMilliseconds = _subtitle.Paragraphs[index].StartTime.TotalMilliseconds + Utilities.GetOptimalDisplayMilliseconds(_subtitle.Paragraphs[index].Text);
                    SubtitleListview1.SetStartTime(index, _subtitle.Paragraphs[index]);
                    checkBoxSyncListViewWithVideoWhilePlaying.Checked = oldSync;
                    timeUpDownStartTime.MaskedTextBox.TextChanged += MaskedTextBoxTextChanged;
                    return;
                }

                _subtitle.Paragraphs[index].StartTime = new TimeCode(_subtitle.Paragraphs[index].StartTime.TotalMilliseconds - offset);
                _subtitle.Paragraphs[index].EndTime = new TimeCode(_subtitle.Paragraphs[index].EndTime.TotalMilliseconds - offset);
                SubtitleListview1.SetStartTime(index, _subtitle.Paragraphs[index]);

                for (int i = index + 1; i < SubtitleListview1.Items.Count; i++)
                {
                    if (!_subtitle.Paragraphs[i].StartTime.IsMaxTime)
                    {
                        _subtitle.Paragraphs[i].StartTime = new TimeCode(_subtitle.Paragraphs[i].StartTime.TotalMilliseconds - offset);
                        _subtitle.Paragraphs[i].EndTime = new TimeCode(_subtitle.Paragraphs[i].EndTime.TotalMilliseconds - offset);
                        SubtitleListview1.SetStartTime(i, _subtitle.Paragraphs[i]);
                    }
                }

                if (Configuration.Settings.General.AllowEditOfOriginalSubtitle && _subtitleAlternate != null && _subtitleAlternate.Paragraphs.Count > 0)
                {
                    Paragraph original = Utilities.GetOriginalParagraph(index, _subtitle.Paragraphs[index], _subtitleAlternate.Paragraphs);
                    if (original != null)
                    {
                        index = _subtitleAlternate.GetIndex(original);
                        for (int i = index; i < _subtitleAlternate.Paragraphs.Count; i++)
                        {
                            if (!_subtitleAlternate.Paragraphs[i].StartTime.IsMaxTime)
                            {
                                _subtitleAlternate.Paragraphs[i].StartTime = new TimeCode(_subtitleAlternate.Paragraphs[i].StartTime.TotalMilliseconds - offset);
                                _subtitleAlternate.Paragraphs[i].EndTime = new TimeCode(_subtitleAlternate.Paragraphs[i].EndTime.TotalMilliseconds - offset);
                            }
                        }
                    }
                }
                if (IsFramesRelevant && CurrentFrameRate > 0)
                {
                    _subtitle.CalculateFrameNumbersFromTimeCodesNoCheck(CurrentFrameRate);
                    if (tabControlSubtitle.SelectedIndex == TabControlSourceView)
                        ShowSource();
                }

                checkBoxSyncListViewWithVideoWhilePlaying.Checked = oldSync;
                timeUpDownStartTime.MaskedTextBox.TextChanged += MaskedTextBoxTextChanged;
            }
        }

        private void ButtonSetEndAndGoToNextClick(object sender, EventArgs e)
        {
            if (SubtitleListview1.SelectedItems.Count == 1)
            {
                int index = SubtitleListview1.SelectedItems[0].Index;
                double videoPosition = mediaPlayer.CurrentPosition;
                if (!mediaPlayer.IsPaused)
                    videoPosition -= Configuration.Settings.General.SetStartEndHumanDelay / 1000.0;

                string oldDuration = _subtitle.Paragraphs[index].Duration.ToString();
                var temp = new Paragraph(_subtitle.Paragraphs[index]);
                temp.EndTime.TotalMilliseconds = TimeCode.FromSeconds(videoPosition).TotalMilliseconds;
                MakeHistoryForUndo(string.Format(_language.DisplayTimeAdjustedX, "#" + _subtitle.Paragraphs[index].Number + ": " + oldDuration + " -> " + temp.Duration));
                _makeHistoryPaused = true;

                if (_subtitle.Paragraphs[index].StartTime.IsMaxTime)
                {
                    timeUpDownStartTime.MaskedTextBox.TextChanged -= MaskedTextBoxTextChanged;
                    _subtitle.Paragraphs[index].EndTime.TotalSeconds = videoPosition;
                    _subtitle.Paragraphs[index].StartTime.TotalMilliseconds = _subtitle.Paragraphs[index].EndTime.TotalMilliseconds - Utilities.GetOptimalDisplayMilliseconds(_subtitle.Paragraphs[index].Text);
                    if (_subtitle.Paragraphs[index].StartTime.TotalMilliseconds < 0)
                        _subtitle.Paragraphs[index].StartTime.TotalMilliseconds = 0;
                    timeUpDownStartTime.TimeCode = _subtitle.Paragraphs[index].StartTime;
                    SubtitleListview1.SetStartTime(index, _subtitle.Paragraphs[index]);
                    timeUpDownStartTime.MaskedTextBox.TextChanged += MaskedTextBoxTextChanged;
                }
                else
                {
                    _subtitle.Paragraphs[index].EndTime = TimeCode.FromSeconds(videoPosition);
                }
                SubtitleListview1.SetDuration(index, _subtitle.Paragraphs[index]);
                SetDurationInSeconds(_subtitle.Paragraphs[index].Duration.TotalSeconds);

                if (index + 1 < _subtitle.Paragraphs.Count)
                {
                    SubtitleListview1.Items[index].Selected = false;
                    SubtitleListview1.Items[index + 1].Selected = true;
                    if (!_subtitle.Paragraphs[index + 1].StartTime.IsMaxTime)
                        _subtitle.Paragraphs[index + 1].StartTime = TimeCode.FromSeconds(videoPosition + 0.001);

                    if (IsFramesRelevant && CurrentFrameRate > 0)
                    {
                        _subtitle.CalculateFrameNumbersFromTimeCodesNoCheck(CurrentFrameRate);
                        if (tabControlSubtitle.SelectedIndex == TabControlSourceView)
                            ShowSource();
                    }
                    SubtitleListview1.SetStartTime(index + 1, _subtitle.Paragraphs[index + 1]);
                    SubtitleListview1.SelectIndexAndEnsureVisible(index + 1, true);
                }
                _makeHistoryPaused = false;
            }
        }

        private void ButtonAdjustSecBackClick(object sender, EventArgs e)
        {
            GoBackSeconds((double)numericUpDownSecAdjust1.Value);
        }

        private void ButtonAdjustSecForwardClick(object sender, EventArgs e)
        {
            GoBackSeconds(-(double)numericUpDownSecAdjust1.Value);
        }

        private void Main_Shown(object sender, EventArgs e)
        {
            toolStripButtonToggleVideo.Checked = !Configuration.Settings.General.ShowVideoPlayer;
            toolStripButtonToggleVideo_Click(null, null);

            _timerAutoSave.Tick += TimerAutoSaveTick;
            if (Configuration.Settings.General.AutoBackupSeconds > 0)
            {
                _timerAutoSave.Interval = 1000 * Configuration.Settings.General.AutoBackupSeconds; // take backup every x second if changes were made
                _timerAutoSave.Start();
            }
            ToolStripMenuItemPlayRateNormalClick(null, null);

            SetPositionFromXYString(Configuration.Settings.General.UndockedVideoPosition, "VideoPlayerUndocked");
            SetPositionFromXYString(Configuration.Settings.General.UndockedWaveformPosition, "WaveformUndocked");
            SetPositionFromXYString(Configuration.Settings.General.UndockedVideoControlsPosition, "VideoControlsUndocked");
            if (Configuration.Settings.General.Undocked && Configuration.Settings.General.StartRememberPositionAndSize)
            {
                Configuration.Settings.General.Undocked = false;
                UndockVideoControlsToolStripMenuItemClick(null, null);
            }
            Main_Resize(null, null);

            toolStripButtonLockCenter.Checked = Configuration.Settings.General.WaveformCenter;
            audioVisualizer.Locked = toolStripButtonLockCenter.Checked;

            numericUpDownSec1.Value = (decimal)(Configuration.Settings.General.SmallDelayMilliseconds / 1000.0);
            numericUpDownSec2.Value = (decimal)(Configuration.Settings.General.LargeDelayMilliseconds / 1000.0);

            numericUpDownSecAdjust1.Value = (decimal)(Configuration.Settings.General.SmallDelayMilliseconds / 1000.0);
            numericUpDownSecAdjust2.Value = (decimal)(Configuration.Settings.General.LargeDelayMilliseconds / 1000.0);

            SetShortcuts();

            if (Configuration.Settings.General.StartInSourceView)
            {
                textBoxSource.Focus();
            }
            else
            {
                SubtitleListview1.Focus();
                int index = FirstSelectedIndex;
                if (index > 0 && SubtitleListview1.Items.Count > index)
                {
                    SubtitleListview1.Focus();
                    SubtitleListview1.Items[index].Focused = true;
                }
            }
            MainResize();
            _loading = false;
            OpenVideo(_videoFileName);
            timerTextUndo.Start();
            timerAlternateTextUndo.Start();
            if (Configuration.IsRunningOnLinux())
            {
                numericUpDownDuration.Left = timeUpDownStartTime.Left + timeUpDownStartTime.Width + 10;
                numericUpDownDuration.Width = numericUpDownDuration.Width + 10;
                numericUpDownSec1.Width = numericUpDownSec1.Width + 10;
                numericUpDownSec2.Width = numericUpDownSec2.Width + 10;
                numericUpDownSecAdjust1.Width = numericUpDownSecAdjust1.Width + 10;
                numericUpDownSecAdjust2.Width = numericUpDownSecAdjust2.Width + 10;
                labelDuration.Left = numericUpDownDuration.Left;
            }

            _timerDoSyntaxColoring.Interval = 100;
            _timerDoSyntaxColoring.Tick += _timerDoSyntaxColoring_Tick;

            if (Configuration.Settings.General.ShowBetaStuff)
            {
                generateDatetimeInfoFromVideoToolStripMenuItem.Visible = true;
                toolStripMenuItemExportCaptionInc.Visible = true;
                toolStripMenuItemExportUltech130.Visible = true;
                toolStripMenuItemInverseSelection.Visible = true;
                toolStripMenuItemSpellCheckFromCurrentLine.Visible = true;
                toolStripMenuItemImportOcrHardSub.Visible = true;
                toolStripMenuItemMeasurementConverter.Visible = true;
                toolStripMenuItemOpenDvd.Visible = true;
            }
            else
            {
                generateDatetimeInfoFromVideoToolStripMenuItem.Visible = false;
                toolStripMenuItemExportCaptionInc.Visible = false;
                toolStripMenuItemExportUltech130.Visible = false;
                toolStripMenuItemInverseSelection.Visible = false;
                toolStripMenuItemSpellCheckFromCurrentLine.Visible = false;
                toolStripMenuItemImportOcrHardSub.Visible = false;
                toolStripMenuItemMeasurementConverter.Visible = false;
                toolStripMenuItemOpenDvd.Visible = false;
            }

            if (Configuration.Settings.General.StartRememberPositionAndSize &&
                Configuration.Settings.General.SplitContainerMainSplitterDistance > 0 &&
                Configuration.Settings.General.SplitContainer1SplitterDistance > 0 &&
                Configuration.Settings.General.SplitContainerListViewAndTextSplitterDistance > 0)
            {
                splitContainerMain.SplitterDistance = Configuration.Settings.General.SplitContainerMainSplitterDistance;
                splitContainer1.SplitterDistance = Configuration.Settings.General.SplitContainer1SplitterDistance;
                splitContainerListViewAndText.SplitterDistance = Configuration.Settings.General.SplitContainerListViewAndTextSplitterDistance;
            }
            mediaPlayer.InitializeVolume(Configuration.Settings.General.VideoPlayerDefaultVolume);
            LoadPlugins();
            tabControlSubtitle.Invalidate();

            if (string.IsNullOrEmpty(Configuration.Settings.Language.CheckForUpdates.CheckingForUpdates))
            {
                checkForUpdatesToolStripMenuItem.Visible = false;
                toolStripMenuItemSplitterCheckForUpdates.Visible = false;
            }
            else if (Configuration.Settings.General.CheckForUpdates && Configuration.Settings.General.LastCheckForUpdates < DateTime.Now.AddDays(-5))
            {
                _checkForUpdatesHelper = new Nikse.SubtitleEdit.Logic.Forms.CheckForUpdatesHelper();
                _checkForUpdatesHelper.CheckForUpdates();
                _timerCheckForUpdates = new Timer();
                _timerCheckForUpdates.Interval = 7000;
                _timerCheckForUpdates.Tick += TimerCheckForUpdatesTick;
                _timerCheckForUpdates.Start();
                Configuration.Settings.General.LastCheckForUpdates = DateTime.Now;
            }
            _dragAndDropTimer.Interval = 50;
            _dragAndDropTimer.Tick += DoSubtitleListview1Drop;
        }

        private void TimerCheckForUpdatesTick(object sender, EventArgs e)
        {
            _timerCheckForUpdates.Stop();
            if (_checkForUpdatesHelper.IsUpdateAvailable())
            {
                var form = new CheckForUpdates(this, _checkForUpdatesHelper);
                form.ShowDialog(this);
            }
            _checkForUpdatesHelper = null;
            _timerCheckForUpdates = null;
        }

        private void _timerDoSyntaxColoring_Tick(object sender, EventArgs e)
        {
            UpdateListSyntaxColoring();
            _timerDoSyntaxColoring.Stop();
        }

        private void SetPositionFromXYString(string positionAndSize, string name)
        {
            string[] parts = positionAndSize.Split(';');
            if (parts.Length == 4)
            {
                try
                {
                    int x = int.Parse(parts[0]);
                    int y = int.Parse(parts[1]);
                    int w = int.Parse(parts[2]);
                    int h = int.Parse(parts[3]);
                    _formPositionsAndSizes.AddPositionAndSize(new PositionAndSize() { Left = x, Top = y, Size = new Size(w, h), Name = name });
                }
                catch (Exception exception)
                {
                    System.Diagnostics.Debug.WriteLine(exception.Message);
                }
            }
        }

        private void SetShortcuts()
        {
            _mainGeneralGoToFirstSelectedLine = Utilities.GetKeys(Configuration.Settings.Shortcuts.GeneralGoToFirstSelectedLine);
            _mainGeneralGoToFirstEmptyLine = Utilities.GetKeys(Configuration.Settings.Shortcuts.GeneralGoToNextEmptyLine);
            _mainGeneralMergeSelectedLines = Utilities.GetKeys(Configuration.Settings.Shortcuts.GeneralMergeSelectedLines);
            _mainGeneralMergeSelectedLinesOnlyFirstText = Utilities.GetKeys(Configuration.Settings.Shortcuts.GeneralMergeSelectedLinesOnlyFirstText);
            _mainGeneralToggleTranslationMode = Utilities.GetKeys(Configuration.Settings.Shortcuts.GeneralToggleTranslationMode);
            _mainGeneralSwitchTranslationAndOriginal = Utilities.GetKeys(Configuration.Settings.Shortcuts.GeneralSwitchOriginalAndTranslation);
            _mainGeneralMergeTranslationAndOriginal = Utilities.GetKeys(Configuration.Settings.Shortcuts.GeneralMergeOriginalAndTranslation);
            _mainGeneralGoToNextSubtitle = Utilities.GetKeys(Configuration.Settings.Shortcuts.GeneralGoToNextSubtitle);
            _mainGeneralGoToPrevSubtitle = Utilities.GetKeys(Configuration.Settings.Shortcuts.GeneralGoToPrevSubtitle);
            _mainGeneralGoToStartOfCurrentSubtitle = Utilities.GetKeys(Configuration.Settings.Shortcuts.GeneralGoToStartOfCurrentSubtitle);
            _mainGeneralGoToEndOfCurrentSubtitle = Utilities.GetKeys(Configuration.Settings.Shortcuts.GeneralGoToEndOfCurrentSubtitle);

            newToolStripMenuItem.ShortcutKeys = Utilities.GetKeys(Configuration.Settings.Shortcuts.MainFileNew);
            openToolStripMenuItem.ShortcutKeys = Utilities.GetKeys(Configuration.Settings.Shortcuts.MainFileOpen);
            toolStripMenuItemOpenKeepVideo.ShortcutKeys = Utilities.GetKeys(Configuration.Settings.Shortcuts.MainFileOpenKeepVideo);
            saveToolStripMenuItem.ShortcutKeys = Utilities.GetKeys(Configuration.Settings.Shortcuts.MainFileSave);
            saveOriginalToolStripMenuItem.ShortcutKeys = Utilities.GetKeys(Configuration.Settings.Shortcuts.MainFileSaveOriginal);
            saveOriginalAstoolStripMenuItem.ShortcutKeys = Utilities.GetKeys(Configuration.Settings.Shortcuts.MainFileSaveOriginalAs);
            saveAsToolStripMenuItem.ShortcutKeys = Utilities.GetKeys(Configuration.Settings.Shortcuts.MainFileSaveAs);
            _mainGeneralFileSaveAll = Utilities.GetKeys(Configuration.Settings.Shortcuts.MainFileSaveAll);
            eBUSTLToolStripMenuItem.ShortcutKeys = Utilities.GetKeys(Configuration.Settings.Shortcuts.MainFileExportEbu);

            toolStripMenuItemUndo.ShortcutKeys = Utilities.GetKeys(Configuration.Settings.Shortcuts.MainEditUndo);
            toolStripMenuItemRedo.ShortcutKeys = Utilities.GetKeys(Configuration.Settings.Shortcuts.MainEditRedo);
            findToolStripMenuItem.ShortcutKeys = Utilities.GetKeys(Configuration.Settings.Shortcuts.MainEditFind);
            findNextToolStripMenuItem.ShortcutKeys = Utilities.GetKeys(Configuration.Settings.Shortcuts.MainEditFindNext);
            replaceToolStripMenuItem.ShortcutKeys = Utilities.GetKeys(Configuration.Settings.Shortcuts.MainEditReplace);
            multipleReplaceToolStripMenuItem.ShortcutKeys = Utilities.GetKeys(Configuration.Settings.Shortcuts.MainEditMultipleReplace);
            gotoLineNumberToolStripMenuItem.ShortcutKeys = Utilities.GetKeys(Configuration.Settings.Shortcuts.MainEditGoToLineNumber);
            toolStripMenuItemRightToLeftMode.ShortcutKeys = Utilities.GetKeys(Configuration.Settings.Shortcuts.MainEditRightToLeft);
            toolStripMenuItemShowOriginalInPreview.ShortcutKeys = Utilities.GetKeys(Configuration.Settings.Shortcuts.MainEditToggleTranslationOriginalInPreviews);
            toolStripMenuItemInverseSelection.ShortcutKeys = Utilities.GetKeys(Configuration.Settings.Shortcuts.MainEditInverseSelection);
            toolStripMenuItemModifySelection.ShortcutKeys = Utilities.GetKeys(Configuration.Settings.Shortcuts.MainEditModifySelection);

            fixToolStripMenuItem.ShortcutKeys = Utilities.GetKeys(Configuration.Settings.Shortcuts.MainToolsFixCommonErrors);
            toolStripMenuItemAutoMergeShortLines.ShortcutKeys = Utilities.GetKeys(Configuration.Settings.Shortcuts.MainToolsMergeShortLines);
            toolStripMenuItemAutoSplitLongLines.ShortcutKeys = Utilities.GetKeys(Configuration.Settings.Shortcuts.MainToolsSplitLongLines);
            startNumberingFromToolStripMenuItem.ShortcutKeys = Utilities.GetKeys(Configuration.Settings.Shortcuts.MainToolsRenumber);
            removeTextForHearImparedToolStripMenuItem.ShortcutKeys = Utilities.GetKeys(Configuration.Settings.Shortcuts.MainToolsRemoveTextForHI);
            ChangeCasingToolStripMenuItem.ShortcutKeys = Utilities.GetKeys(Configuration.Settings.Shortcuts.MainToolsChangeCasing);
            toolStripMenuItemShowOriginalInPreview.ShortcutKeys = Utilities.GetKeys(Configuration.Settings.Shortcuts.MainEditToggleTranslationOriginalInPreviews);
            toolStripMenuItemBatchConvert.ShortcutKeys = Utilities.GetKeys(Configuration.Settings.Shortcuts.MainToolsBatchConvert);

            showhideVideoToolStripMenuItem.ShortcutKeys = Utilities.GetKeys(Configuration.Settings.Shortcuts.MainVideoShowHideVideo);
            _toggleVideoDockUndock = Utilities.GetKeys(Configuration.Settings.Shortcuts.MainVideoToggleVideoControls);
            _videoPause = Utilities.GetKeys(Configuration.Settings.Shortcuts.MainVideoPause);
            _videoPlayPauseToggle = Utilities.GetKeys(Configuration.Settings.Shortcuts.MainVideoPlayPauseToggle);
            _video1FrameLeft = Utilities.GetKeys(Configuration.Settings.Shortcuts.MainVideo1FrameLeft);
            _video1FrameRight = Utilities.GetKeys(Configuration.Settings.Shortcuts.MainVideo1FrameRight);
            _video100MsLeft = Utilities.GetKeys(Configuration.Settings.Shortcuts.MainVideo100MsLeft);
            _video100MsRight = Utilities.GetKeys(Configuration.Settings.Shortcuts.MainVideo100MsRight);
            _video500MsLeft = Utilities.GetKeys(Configuration.Settings.Shortcuts.MainVideo500MsLeft);
            _video500MsRight = Utilities.GetKeys(Configuration.Settings.Shortcuts.MainVideo500MsRight);
            _video1000MsLeft = Utilities.GetKeys(Configuration.Settings.Shortcuts.MainVideo1000MsLeft);
            _video1000MsRight = Utilities.GetKeys(Configuration.Settings.Shortcuts.MainVideo1000MsRight);
            _videoPlayFirstSelected = Utilities.GetKeys(Configuration.Settings.Shortcuts.GeneralPlayFirstSelected);
            _mainVideoFullscreen = Utilities.GetKeys(Configuration.Settings.Shortcuts.MainVideoFullscreen);

            spellCheckToolStripMenuItem.ShortcutKeys = Utilities.GetKeys(Configuration.Settings.Shortcuts.MainSpellCheck);
            findDoubleWordsToolStripMenuItem.ShortcutKeys = Utilities.GetKeys(Configuration.Settings.Shortcuts.MainSpellCheckFindDoubleWords);
            addWordToNamesetcListToolStripMenuItem.ShortcutKeys = Utilities.GetKeys(Configuration.Settings.Shortcuts.MainSpellCheckAddWordToNames);

            toolStripMenuItemAdjustAllTimes.ShortcutKeys = Utilities.GetKeys(Configuration.Settings.Shortcuts.MainSynchronizationAdjustTimes);
            visualSyncToolStripMenuItem.ShortcutKeys = Utilities.GetKeys(Configuration.Settings.Shortcuts.MainSynchronizationVisualSync);
            toolStripMenuItemPointSync.ShortcutKeys = Utilities.GetKeys(Configuration.Settings.Shortcuts.MainSynchronizationPointSync);
            toolStripMenuItemChangeFrameRate2.ShortcutKeys = Utilities.GetKeys(Configuration.Settings.Shortcuts.MainSynchronizationChangeFrameRate);
            italicToolStripMenuItem.ShortcutKeys = Utilities.GetKeys(Configuration.Settings.Shortcuts.MainListViewItalic);
            _mainToolsAutoDuration = Utilities.GetKeys(Configuration.Settings.Shortcuts.MainToolsAutoDuration);
            _mainToolsBeamer = Utilities.GetKeys(Configuration.Settings.Shortcuts.MainToolsBeamer);
            _mainListViewToggleDashes = Utilities.GetKeys(Configuration.Settings.Shortcuts.MainListViewToggleDashes);
            toolStripMenuItemAlignment.ShortcutKeys = Utilities.GetKeys(Configuration.Settings.Shortcuts.MainListViewAlignment);
            _mainListViewAutoDuration = Utilities.GetKeys(Configuration.Settings.Shortcuts.MainListViewAutoDuration);
            _mainListViewFocusWaveform = Utilities.GetKeys(Configuration.Settings.Shortcuts.MainListViewFocusWaveform);
            _mainListViewGoToNextError = Utilities.GetKeys(Configuration.Settings.Shortcuts.MainListViewGoToNextError);
            _mainEditReverseStartAndEndingForRTL = Utilities.GetKeys(Configuration.Settings.Shortcuts.MainEditReverseStartAndEndingForRTL);
            _mainListViewCopyText = Utilities.GetKeys(Configuration.Settings.Shortcuts.MainListViewCopyText);
            toolStripMenuItemColumnDeleteText.ShortcutKeys = Utilities.GetKeys(Configuration.Settings.Shortcuts.MainListViewColumnDeleteText);
            ShiftTextCellsDownToolStripMenuItem.ShortcutKeys = Utilities.GetKeys(Configuration.Settings.Shortcuts.MainListViewColumnInsertText);
            toolStripMenuItemPasteSpecial.ShortcutKeys = Utilities.GetKeys(Configuration.Settings.Shortcuts.MainListViewColumnPaste);
            toolStripMenuItemReverseRightToLeftStartEnd.ShortcutKeys = _mainEditReverseStartAndEndingForRTL;
            italicToolStripMenuItem1.ShortcutKeys = Utilities.GetKeys(Configuration.Settings.Shortcuts.MainTextBoxItalic);
            _mainTextBoxSplitAtCursor = Utilities.GetKeys(Configuration.Settings.Shortcuts.MainTextBoxSplitAtCursor);
            _mainTextBoxMoveLastWordDown = Utilities.GetKeys(Configuration.Settings.Shortcuts.MainTextBoxMoveLastWordDown);
            _mainTextBoxMoveFirstWordFromNextUp = Utilities.GetKeys(Configuration.Settings.Shortcuts.MainTextBoxMoveFirstWordFromNextUp);
            _mainTextBoxSelectionToLower = Utilities.GetKeys(Configuration.Settings.Shortcuts.MainTextBoxSelectionToLower);
            _mainTextBoxSelectionToUpper = Utilities.GetKeys(Configuration.Settings.Shortcuts.MainTextBoxSelectionToUpper);
            _mainCreateInsertSubAtVideoPos = Utilities.GetKeys(Configuration.Settings.Shortcuts.MainCreateInsertSubAtVideoPos);
            _mainCreatePlayFromJustBefore = Utilities.GetKeys(Configuration.Settings.Shortcuts.MainCreatePlayFromJustBefore);
            _mainCreateSetStart = Utilities.GetKeys(Configuration.Settings.Shortcuts.MainCreateSetStart);
            _mainCreateSetEnd = Utilities.GetKeys(Configuration.Settings.Shortcuts.MainCreateSetEnd);
            _mainCreateStartDownEndUp = Utilities.GetKeys(Configuration.Settings.Shortcuts.MainCreateStartDownEndUp);
            _mainCreateSetEndAddNewAndGoToNew = Utilities.GetKeys(Configuration.Settings.Shortcuts.MainCreateSetEndAddNewAndGoToNew);
            _mainAdjustSetStartAndOffsetTheRest = Utilities.GetKeys(Configuration.Settings.Shortcuts.MainAdjustSetStartAndOffsetTheRest);
            _mainAdjustSetEndAndOffsetTheRest = Utilities.GetKeys(Configuration.Settings.Shortcuts.MainAdjustSetEndAndOffsetTheRest);
            _mainAdjustSetEndAndOffsetTheRestAndGoToNext = Utilities.GetKeys(Configuration.Settings.Shortcuts.MainAdjustSetEndAndOffsetTheRestAndGoToNext);
            _mainAdjustSetEndAndGotoNext = Utilities.GetKeys(Configuration.Settings.Shortcuts.MainAdjustSetEndAndGotoNext);
            _mainAdjustInsertViaEndAutoStartAndGoToNext = Utilities.GetKeys(Configuration.Settings.Shortcuts.MainAdjustViaEndAutoStartAndGoToNext);
            _mainAdjustSetStartAutoDurationAndGoToNext = Utilities.GetKeys(Configuration.Settings.Shortcuts.MainAdjustSetStartAutoDurationAndGoToNext);
            _mainAdjustSetEndNextStartAndGoToNext = Utilities.GetKeys(Configuration.Settings.Shortcuts.MainAdjustSetEndNextStartAndGoToNext);
            _mainAdjustStartDownEndUpAndGoToNext = Utilities.GetKeys(Configuration.Settings.Shortcuts.MainAdjustStartDownEndUpAndGoToNext);
            _mainAdjustSetStart = Utilities.GetKeys(Configuration.Settings.Shortcuts.MainAdjustSetStart);
            _mainAdjustSetStartKeepDuration = Utilities.GetKeys(Configuration.Settings.Shortcuts.MainAdjustSetStartKeepDuration);
            _mainAdjustSetEnd = Utilities.GetKeys(Configuration.Settings.Shortcuts.MainAdjustSetEnd);
            _mainAdjustSelected100MsForward = Utilities.GetKeys(Configuration.Settings.Shortcuts.MainAdjustSelected100MsForward);
            _mainAdjustSelected100MsBack = Utilities.GetKeys(Configuration.Settings.Shortcuts.MainAdjustSelected100MsBack);
            _mainInsertAfter = Utilities.GetKeys(Configuration.Settings.Shortcuts.MainInsertAfter);
            _mainInsertBefore = Utilities.GetKeys(Configuration.Settings.Shortcuts.MainInsertBefore);
            _mainTextBoxInsertAfter = Utilities.GetKeys(Configuration.Settings.Shortcuts.MainTextBoxInsertAfter);
            _mainTextBoxAutoBreak = Utilities.GetKeys(Configuration.Settings.Shortcuts.MainTextBoxAutoBreak);
            _mainTextBoxUnbreak = Utilities.GetKeys(Configuration.Settings.Shortcuts.MainTextBoxUnbreak);
            _mainMergeDialog = Utilities.GetKeys(Configuration.Settings.Shortcuts.MainMergeDialog);
            _mainToggleFocus = Utilities.GetKeys(Configuration.Settings.Shortcuts.MainToggleFocus);
            _waveformVerticalZoom = Utilities.GetKeys(Configuration.Settings.Shortcuts.WaveformVerticalZoom);
            _waveformVerticalZoomOut = Utilities.GetKeys(Configuration.Settings.Shortcuts.WaveformVerticalZoomOut);
            _waveformZoomIn = Utilities.GetKeys(Configuration.Settings.Shortcuts.WaveformZoomIn);
            _waveformZoomOut = Utilities.GetKeys(Configuration.Settings.Shortcuts.WaveformZoomOut);
            _waveformPlaySelection = Utilities.GetKeys(Configuration.Settings.Shortcuts.WaveformPlaySelection);
            _waveformSearchSilenceForward = Utilities.GetKeys(Configuration.Settings.Shortcuts.WaveformSearchSilenceForward);
            _waveformSearchSilenceBack = Utilities.GetKeys(Configuration.Settings.Shortcuts.WaveformSearchSilenceBack);
            _waveformAddTextAtHere = Utilities.GetKeys(Configuration.Settings.Shortcuts.WaveformAddTextHere);
            _waveformFocusListView = Utilities.GetKeys(Configuration.Settings.Shortcuts.WaveformFocusListView);
            _mainTranslateCustomSearch1 = Utilities.GetKeys(Configuration.Settings.Shortcuts.MainTranslateCustomSearch1);
            _mainTranslateCustomSearch2 = Utilities.GetKeys(Configuration.Settings.Shortcuts.MainTranslateCustomSearch2);
            _mainTranslateCustomSearch3 = Utilities.GetKeys(Configuration.Settings.Shortcuts.MainTranslateCustomSearch3);
            _mainTranslateCustomSearch4 = Utilities.GetKeys(Configuration.Settings.Shortcuts.MainTranslateCustomSearch4);
            _mainTranslateCustomSearch5 = Utilities.GetKeys(Configuration.Settings.Shortcuts.MainTranslateCustomSearch5);
            _mainTranslateCustomSearch6 = Utilities.GetKeys(Configuration.Settings.Shortcuts.MainTranslateCustomSearch6);

            if (audioVisualizer != null)
            {
                audioVisualizer.InsertAtVideoPositionShortcut = Utilities.GetKeys(Configuration.Settings.Shortcuts.MainWaveformInsertAtCurrentPosition);
            }
        }

        public static object GetPropertiesAndDoAction(string pluginFileName, out string name, out string text, out decimal version, out string description, out string actionType, out string shortcut, out System.Reflection.MethodInfo mi)
        {
            name = null;
            text = null;
            version = 0;
            description = null;
            actionType = null;
            shortcut = null;
            mi = null;
            System.Reflection.Assembly assembly;
            try
            {
                assembly = System.Reflection.Assembly.Load(File.ReadAllBytes(pluginFileName));
            }
            catch
            {
                return null;
            }
            string objectName = Path.GetFileNameWithoutExtension(pluginFileName);
            if (assembly != null)
            {
                Type pluginType = assembly.GetType("Nikse.SubtitleEdit.PluginLogic." + objectName);
                if (pluginType == null)
                    return null;
                object pluginObject = Activator.CreateInstance(pluginType);

                // IPlugin
                Type[] tt = pluginType.GetInterfaces();
                Type t = null;
                foreach (Type t2 in tt)
                {
                    if (t2.Name == "IPlugin")
                    {
                        t = t2;
                        break;
                    }
                }

                System.Reflection.PropertyInfo pi = t.GetProperty("Name");
                if (pi != null)
                    name = (string)pi.GetValue(pluginObject, null);

                pi = t.GetProperty("Text");
                if (pi != null)
                    text = (string)pi.GetValue(pluginObject, null);

                pi = t.GetProperty("Description");
                if (pi != null)
                    description = (string)pi.GetValue(pluginObject, null);

                pi = t.GetProperty("Version");
                if (pi != null)
                    version = Convert.ToDecimal(pi.GetValue(pluginObject, null));

                pi = t.GetProperty("ActionType");
                if (pi != null)
                    actionType = (string)pi.GetValue(pluginObject, null);

                mi = t.GetMethod("DoAction");

                pi = t.GetProperty("Shortcut");
                if (pi != null)
                    shortcut = (string)pi.GetValue(pluginObject, null);

                return pluginObject;
            }
            return null;
        }

        private void LoadPlugins()
        {
            string path = Configuration.PluginsDirectory;
            if (!Directory.Exists(path))
                return;
            string[] pluginFiles = Directory.GetFiles(path, "*.DLL");

            int filePluginCount = 0;
            int toolsPluginCount = 0;
            int syncPluginCount = 0;

            for (int k = fileToolStripMenuItem.DropDownItems.Count - 1; k > 0; k--)
            {
                ToolStripItem x = fileToolStripMenuItem.DropDownItems[k];
                if (x.Name.StartsWith("Plugin"))
                    fileToolStripMenuItem.DropDownItems.Remove(x);
            }
            for (int k = toolsToolStripMenuItem.DropDownItems.Count - 1; k > 0; k--)
            {
                ToolStripItem x = toolsToolStripMenuItem.DropDownItems[k];
                if (x.Name.StartsWith("Plugin"))
                    toolsToolStripMenuItem.DropDownItems.Remove(x);
            }
            for (int k = toolStripMenuItemSpellCheckMain.DropDownItems.Count - 1; k > 0; k--)
            {
                ToolStripItem x = toolStripMenuItemSpellCheckMain.DropDownItems[k];
                if (x.Name.StartsWith("Plugin"))
                    toolStripMenuItemSpellCheckMain.DropDownItems.Remove(x);
            }
            for (int k = toolStripMenuItemSynchronization.DropDownItems.Count - 1; k > 0; k--)
            {
                ToolStripItem x = toolStripMenuItemSynchronization.DropDownItems[k];
                if (x.Name.StartsWith("Plugin"))
                    toolStripMenuItemSynchronization.DropDownItems.Remove(x);
            }
            for (int k = toolStripMenuItemAutoTranslate.DropDownItems.Count - 1; k > 0; k--)
            {
                ToolStripItem x = toolStripMenuItemAutoTranslate.DropDownItems[k];
                if (x.Name.StartsWith("Plugin"))
                    toolStripMenuItemAutoTranslate.DropDownItems.Remove(x);
            }

            foreach (string pluginFileName in pluginFiles)
            {
                try
                {
                    string name, description, text, shortcut, actionType;
                    decimal version;
                    System.Reflection.MethodInfo mi;
                    GetPropertiesAndDoAction(pluginFileName, out name, out text, out version, out description, out actionType, out shortcut, out mi);
                    if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(actionType) && mi != null)
                    {
                        var item = new ToolStripMenuItem();
                        item.Name = "Plugin" + toolsPluginCount;
                        item.Text = text;
                        item.Tag = pluginFileName;

                        if (!string.IsNullOrEmpty(shortcut))
                            item.ShortcutKeys = Utilities.GetKeys(shortcut);

                        if (actionType.Equals("File", StringComparison.OrdinalIgnoreCase))
                        {
                            if (filePluginCount == 0)
                            {
                                var tss = new ToolStripSeparator();
                                tss.Name = "PluginSepFile";
                                fileToolStripMenuItem.DropDownItems.Insert(fileToolStripMenuItem.DropDownItems.Count - 2, tss);
                            }
                            item.Click += PluginToolClick;
                            fileToolStripMenuItem.DropDownItems.Insert(fileToolStripMenuItem.DropDownItems.Count - 2, item);
                            filePluginCount++;
                        }
                        else if (actionType.Equals("Tool", StringComparison.OrdinalIgnoreCase))
                        {
                            if (toolsPluginCount == 0)
                            {
                                var tss = new ToolStripSeparator();
                                tss.Name = "PluginSepTool";
                                toolsToolStripMenuItem.DropDownItems.Add(tss);
                            }
                            item.Click += PluginToolClick;
                            toolsToolStripMenuItem.DropDownItems.Add(item);
                            toolsPluginCount++;
                        }
                        else if (actionType.Equals("Sync", StringComparison.OrdinalIgnoreCase))
                        {
                            if (syncPluginCount == 0)
                            {
                                var tss = new ToolStripSeparator();
                                tss.Name = "PluginSepSync";
                                toolStripMenuItemSynchronization.DropDownItems.Add(tss);
                            }
                            item.Click += PluginToolClick;
                            toolStripMenuItemSynchronization.DropDownItems.Add(item);
                            syncPluginCount++;
                        }
                        else if (actionType.Equals("Translate", StringComparison.OrdinalIgnoreCase))
                        {
                            if (syncPluginCount == 0)
                            {
                                var tss = new ToolStripSeparator();
                                tss.Name = "PluginSepTranslate";
                                toolStripMenuItemAutoTranslate.DropDownItems.Add(tss);
                            }
                            item.Click += PluginToolClick;
                            toolStripMenuItemAutoTranslate.DropDownItems.Add(item);
                            syncPluginCount++;
                        }
                        else if (actionType.Equals("SpellCheck", StringComparison.OrdinalIgnoreCase))
                        {
                            if (syncPluginCount == 0)
                            {
                                var tss = new ToolStripSeparator();
                                tss.Name = "PluginSepSpellCheck";
                                toolStripMenuItemSpellCheckMain.DropDownItems.Add(tss);
                            }
                            item.Click += PluginToolClick;
                            toolStripMenuItemSpellCheckMain.DropDownItems.Add(item);
                            syncPluginCount++;
                        }
                    }
                }
                catch (Exception exception)
                {
                    MessageBox.Show(string.Format(_language.ErrorLoadingPluginXErrorY, pluginFileName, exception.Message));
                }
            }
        }

        private void PluginToolClick(object sender, EventArgs e)
        {
            try
            {
                var item = (ToolStripItem)sender;
                string name, description, text, shortcut, actionType;
                decimal version;
                System.Reflection.MethodInfo mi;
                object pluginObject = GetPropertiesAndDoAction(item.Tag.ToString(), out name, out text, out version, out description, out actionType, out shortcut, out mi);

                string rawText = null;
                SubtitleFormat format = GetCurrentSubtitleFormat();
                if (format != null)
                {
                    if (GetCurrentSubtitleFormat().IsFrameBased)
                        _subtitle.CalculateTimeCodesFromFrameNumbers(CurrentFrameRate);
                    else
                        _subtitle.CalculateFrameNumbersFromTimeCodes(CurrentFrameRate);
                    rawText = _subtitle.ToText(format);
                }

                string pluginResult = (string)mi.Invoke(pluginObject,
                                      new object[]
                                      {
                                        this,
                                        _subtitle.ToText(new SubRip()),
                                        Configuration.Settings.General.CurrentFrameRate,
                                        Configuration.Settings.General.ListViewLineSeparatorString,
                                        _fileName,
                                        _videoFileName,
                                        rawText
                                      });

                if (!string.IsNullOrEmpty(pluginResult) && pluginResult.Length > 10 && text != pluginResult)
                {
                    var lines = new List<string>();
                    foreach (string line in pluginResult.Replace(Environment.NewLine, "\n").Split('\n'))
                        lines.Add(line);

                    MakeHistoryForUndo(string.Format(_language.BeforeRunningPluginXVersionY, name, version));

                    var s = new Subtitle();
                    SubtitleFormat newFormat = null;
                    foreach (SubtitleFormat subtitleFormat in SubtitleFormat.AllSubtitleFormats)
                    {
                        if (subtitleFormat.IsMine(lines, null))
                        {
                            subtitleFormat.LoadSubtitle(s, lines, null);
                            newFormat = subtitleFormat;
                            break;
                        }
                    }

                    if (newFormat != null)
                    {
                        _subtitle.Paragraphs.Clear();
                        _subtitle.Header = s.Header;
                        _subtitle.Footer = s.Footer;
                        foreach (Paragraph p in s.Paragraphs)
                            _subtitle.Paragraphs.Add(p);

                        SaveSubtitleListviewIndexes();
                        SubtitleListview1.Fill(_subtitle, _subtitleAlternate);
                        RestoreSubtitleListviewIndexes();
                        ShowSource();

                        if (!string.IsNullOrEmpty(_language.PluginXExecuted))
                            ShowStatus(string.Format(_language.PluginXExecuted, name));
                    }
                    else
                    {
                        MessageBox.Show(_language.UnableToReadPluginResult);
                    }
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
            }
        }

        private void TimerAutoSaveTick(object sender, EventArgs e)
        {
            string currentText = string.Empty;
            if (_subtitle != null && _subtitle.Paragraphs.Count > 0)
            {
                currentText = _subtitle.ToText(GetCurrentSubtitleFormat());
                if (_textAutoSave == null)
                    _textAutoSave = _changeSubtitleToString;
                if (!string.IsNullOrEmpty(_textAutoSave) && currentText.Trim() != _textAutoSave.Trim() && !string.IsNullOrWhiteSpace(currentText))
                {
                    if (!Directory.Exists(Configuration.AutoBackupFolder))
                    {
                        try
                        {
                            Directory.CreateDirectory(Configuration.AutoBackupFolder);
                        }
                        catch (Exception exception)
                        {
                            MessageBox.Show(string.Format(_language.UnableToCreateBackupDirectory, Configuration.AutoBackupFolder, exception.Message));
                        }
                    }
                    string title = string.Empty;
                    if (!string.IsNullOrEmpty(_fileName))
                        title = "_" + Path.GetFileNameWithoutExtension(_fileName);
                    string fileName = string.Format("{0}{1:0000}-{2:00}-{3:00}_{4:00}-{5:00}-{6:00}{7}{8}", Configuration.AutoBackupFolder, DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second, title, GetCurrentSubtitleFormat().Extension);
                    File.WriteAllText(fileName, currentText);
                }
            }
            _textAutoSave = currentText;

            if (_subtitleAlternateFileName != null && _subtitleAlternate != null && _subtitleAlternate.Paragraphs.Count > 0)
            {
                string currentTextAlternate = _subtitleAlternate.ToText(GetCurrentSubtitleFormat());
                if (_subtitleAlternate != null && _subtitleAlternate.Paragraphs.Count > 0)
                {
                    if (_textAutoSaveOriginal == null)
                        _textAutoSaveOriginal = _changeSubtitleToString;
                    if (!string.IsNullOrEmpty(_textAutoSaveOriginal) && currentTextAlternate.Trim() != _textAutoSaveOriginal.Trim() && !string.IsNullOrWhiteSpace(currentTextAlternate))
                    {
                        if (!Directory.Exists(Configuration.AutoBackupFolder))
                        {
                            try
                            {
                                Directory.CreateDirectory(Configuration.AutoBackupFolder);
                            }
                            catch
                            {
                            }
                        }
                        string title = string.Empty;
                        if (!string.IsNullOrEmpty(_subtitleAlternateFileName))
                            title = "_" + Path.GetFileNameWithoutExtension(_subtitleAlternateFileName);
                        string fileName = string.Format("{0}{1:0000}-{2:00}-{3:00}_{4:00}-{5:00}-{6:00}{7}{8}", Configuration.AutoBackupFolder, DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second, "_Original" + title, GetCurrentSubtitleFormat().Extension);
                        File.WriteAllText(fileName, currentTextAlternate);
                    }
                }
                _textAutoSaveOriginal = currentTextAlternate;
            }

        }

        private void mediaPlayer_DragDrop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (files.Length == 1)
            {
                string fileName = files[0];
                string ext = Path.GetExtension(fileName).ToLower();
                if (Utilities.GetVideoFileFilter(true).Contains(ext))
                {
                    if (string.IsNullOrEmpty(_fileName))
                    {
                        saveFileDialog1.InitialDirectory = Path.GetDirectoryName(fileName);
                        openFileDialog1.InitialDirectory = Path.GetDirectoryName(fileName);
                    }
                    OpenVideo(fileName);
                }
                else
                {
                    try
                    {
                        var fi = new FileInfo(fileName);
                        if (fi.Length < 1024 * 500)
                        {
                            var lines = new List<string>();
                            foreach (string line in File.ReadAllLines(fileName))
                                lines.Add(line);
                            foreach (SubtitleFormat format in SubtitleFormat.AllSubtitleFormats)
                            {
                                if (format.IsMine(lines, fileName))
                                {
                                    OpenSubtitle(fileName, null);
                                    return;
                                }
                            }
                        }
                    }
                    catch
                    {
                    }

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
            GoBackSeconds((double)numericUpDownSec2.Value);
        }

        private void buttonForward2_Click(object sender, EventArgs e)
        {
            GoBackSeconds(-(double)numericUpDownSec2.Value);
        }

        private void buttonAdjustSecBack2_Click(object sender, EventArgs e)
        {
            GoBackSeconds((double)numericUpDownSecAdjust2.Value);
        }

        private void buttonAdjustSecForward2_Click(object sender, EventArgs e)
        {
            GoBackSeconds(-(double)numericUpDownSecAdjust2.Value);
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

        private static string GetPeakWaveFileName(string videoFileName)
        {
            string dir = Configuration.WaveformsFolder.TrimEnd(Path.DirectorySeparatorChar);
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            FileInfo fi = new FileInfo(videoFileName);
            string wavePeakName = Sha256Hash(Path.GetFileName(videoFileName) + fi.Length + fi.CreationTimeUtc.ToShortDateString()) + ".wav";
            wavePeakName = wavePeakName.Replace("=", string.Empty).Replace("/", string.Empty).Replace(",", string.Empty).Replace("?", string.Empty).Replace("*", string.Empty).Replace("+", string.Empty).Replace("\\", string.Empty);
            wavePeakName = Path.Combine(dir, wavePeakName);
            return wavePeakName;
        }

        private static string GetSpectrogramFolder(string videoFileName)
        {
            string dir = Configuration.SpectrogramsFolder.TrimEnd(Path.DirectorySeparatorChar);
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            FileInfo fi = new FileInfo(videoFileName);
            string name = Sha256Hash(Path.GetFileName(videoFileName) + fi.Length + fi.CreationTimeUtc.ToShortDateString());
            name = name.Replace("=", string.Empty).Replace("/", string.Empty).Replace(",", string.Empty).Replace("?", string.Empty).Replace("*", string.Empty).Replace("+", string.Empty).Replace("\\", string.Empty);
            name = Path.Combine(dir, name);
            return name;
        }

        private void AudioWaveform_Click(object sender, EventArgs e)
        {
            if (audioVisualizer.WavePeaks == null)
            {
                if (string.IsNullOrEmpty(_videoFileName))
                {
                    buttonOpenVideo_Click(sender, e);
                    if (string.IsNullOrEmpty(_videoFileName))
                        return;
                }

                mediaPlayer.Pause();
                var addWaveform = new AddWaveform();
                string peakWaveFileName = GetPeakWaveFileName(_videoFileName);
                string spectrogramFolder = GetSpectrogramFolder(_videoFileName);
                addWaveform.Initialize(_videoFileName, spectrogramFolder, _videoAudioTrackNumber);
                if (addWaveform.ShowDialog() == DialogResult.OK)
                {
                    addWaveform.WavePeak.WritePeakSamples(peakWaveFileName);
                    var audioPeakWave = new WavePeakGenerator(peakWaveFileName);
                    audioPeakWave.GenerateAllSamples();
                    audioPeakWave.Close();
                    audioVisualizer.WavePeaks = audioPeakWave;
                    if (addWaveform.SpectrogramBitmaps != null)
                        audioVisualizer.InitializeSpectrogram(addWaveform.SpectrogramBitmaps, spectrogramFolder);
                    timerWaveform.Start();
                }
            }
        }

        private void timerWaveform_Tick(object sender, EventArgs e)
        {
            if (audioVisualizer.Visible && mediaPlayer.VideoPlayer != null && audioVisualizer.WavePeaks != null)
            {
                int index = -1;
                if (SubtitleListview1.SelectedItems.Count > 0)
                    index = SubtitleListview1.SelectedItems[0].Index;

                if (audioVisualizer.Locked)
                {
                    double startPos = mediaPlayer.CurrentPosition - ((audioVisualizer.EndPositionSeconds - audioVisualizer.StartPositionSeconds) / 2.0);
                    if (startPos < 0)
                        startPos = 0;
                    SetWaveformPosition(startPos, mediaPlayer.CurrentPosition, index);
                }
                else if (mediaPlayer.CurrentPosition > audioVisualizer.EndPositionSeconds || mediaPlayer.CurrentPosition < audioVisualizer.StartPositionSeconds)
                {
                    double startPos = mediaPlayer.CurrentPosition - 0.01;
                    if (startPos < 0)
                        startPos = 0;
                    audioVisualizer.ClearSelection();
                    SetWaveformPosition(startPos, mediaPlayer.CurrentPosition, index);
                }
                else
                {
                    SetWaveformPosition(audioVisualizer.StartPositionSeconds, mediaPlayer.CurrentPosition, index);
                }

                bool paused = mediaPlayer.IsPaused;
                toolStripButtonWaveformPause.Visible = !paused;
                toolStripButtonWaveformPlay.Visible = paused;
            }
            else
            {
                toolStripButtonWaveformPlay.Visible = true;
                toolStripButtonWaveformPause.Visible = false;
            }
        }

        private void addParagraphHereToolStripMenuItem_Click(object sender, EventArgs e)
        {
            audioVisualizer.ClearSelection();
            var newParagraph = new Paragraph(audioVisualizer.NewSelectionParagraph);
            var format = GetCurrentSubtitleFormat();
            bool useExtraForStyle = format.HasStyleSupport;
            var styles = new List<string>();
            if (format.GetType() == typeof(AdvancedSubStationAlpha) || format.GetType() == typeof(SubStationAlpha))
                styles = AdvancedSubStationAlpha.GetStylesFromHeader(_subtitle.Header);
            else if (format.GetType() == typeof(TimedText10) || format.GetType() == typeof(ItunesTimedText))
                styles = TimedText10.GetStylesFromHeader(_subtitle.Header);
            else if (format.GetType() == typeof(Sami) || format.GetType() == typeof(SamiModern))
                styles = Sami.GetStylesFromHeader(_subtitle.Header);
            string style = "Default";
            if (styles.Count > 0)
                style = styles[0];
            if (useExtraForStyle)
                newParagraph.Extra = style;

            mediaPlayer.Pause();

            // find index where to insert
            int index = 0;
            foreach (Paragraph p in _subtitle.Paragraphs)
            {
                if (p.StartTime.TotalMilliseconds > newParagraph.StartTime.TotalMilliseconds)
                    break;
                index++;
            }

            MakeHistoryForUndo(Configuration.Settings.Language.Main.BeforeInsertLine);

            // create and insert
            if (format.IsFrameBased)
            {
                newParagraph.CalculateFrameNumbersFromTimeCodes(CurrentFrameRate);
                newParagraph.CalculateTimeCodesFromFrameNumbers(CurrentFrameRate);
            }

            if (_networkSession != null)
            {
                _networkSession.TimerStop();
                NetworkGetSendUpdates(new List<int>(), index, newParagraph);
            }
            else
            {
                _subtitle.Paragraphs.Insert(index, newParagraph);

                if (_subtitleAlternate != null && SubtitleListview1.IsAlternateTextColumnVisible && Configuration.Settings.General.AllowEditOfOriginalSubtitle)
                {
                    _subtitleAlternate.InsertParagraphInCorrectTimeOrder(new Paragraph(newParagraph));
                    _subtitleAlternate.Renumber(1);
                }

                _subtitleListViewIndex = -1;
                _subtitle.Renumber(1);
                SubtitleListview1.Fill(_subtitle, _subtitleAlternate);
            }
            SubtitleListview1.SelectIndexAndEnsureVisible(index);

            textBoxListViewText.Focus();
            audioVisualizer.NewSelectionParagraph = null;

            ShowStatus(string.Format(_language.VideoControls.NewTextInsertAtX, newParagraph.StartTime.ToShortString()));
            audioVisualizer.Invalidate();
        }

        private void addParagraphAndPasteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            addParagraphHereToolStripMenuItem_Click(sender, e);
            textBoxListViewText.Text = Clipboard.GetText();
        }

        private void mergeWithPreviousToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int index = _subtitle.GetIndex(audioVisualizer.RightClickedParagraph);
            if (index >= 0)
            {
                SubtitleListview1.SelectIndexAndEnsureVisible(index);
                MergeBeforeToolStripMenuItemClick(null, null);
            }
            audioVisualizer.Invalidate();
        }

        private void deleteParagraphToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int index = _subtitle.GetIndex(audioVisualizer.RightClickedParagraph);
            if (index >= 0)
            {
                SubtitleListview1.SelectIndexAndEnsureVisible(index);
                ToolStripMenuItemDeleteClick(null, null);
            }
            audioVisualizer.Invalidate();
        }

        private void splitToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < _subtitle.Paragraphs.Count; i++)
            {
                if (audioVisualizer.RightClickedParagraph.StartTime.TotalMilliseconds == _subtitle.Paragraphs[i].StartTime.TotalMilliseconds &&
                    audioVisualizer.RightClickedParagraph.EndTime.TotalMilliseconds == _subtitle.Paragraphs[i].EndTime.TotalMilliseconds)
                {
                    SubtitleListview1.SelectIndexAndEnsureVisible(i);
                    SplitSelectedParagraph(_audioWaveformRightClickSeconds, null);
                    break;
                }
            }
            audioVisualizer.Invalidate();
        }

        private void mergeWithNextToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int index = _subtitle.GetIndex(audioVisualizer.RightClickedParagraph);
            if (index >= 0)
            {
                SubtitleListview1.SelectIndexAndEnsureVisible(index);
                MergeAfterToolStripMenuItemClick(null, null);
            }
            audioVisualizer.Invalidate();
        }

        private void buttonWaveformZoomIn_Click(object sender, EventArgs e)
        {
            if (audioVisualizer.WavePeaks != null && audioVisualizer.Visible)
            {
                audioVisualizer.ZoomFactor += 0.1;
            }
        }

        private void buttonWaveformZoomOut_Click(object sender, EventArgs e)
        {
            if (audioVisualizer.WavePeaks != null && audioVisualizer.Visible)
            {
                audioVisualizer.ZoomFactor -= 0.1;
            }
        }

        private void buttonWaveformZoomReset_Click(object sender, EventArgs e)
        {
            if (audioVisualizer.WavePeaks != null && audioVisualizer.Visible)
            {
                audioVisualizer.ZoomFactor = 1.0;
            }
        }

        private void toolStripMenuItemWaveformPlaySelection_Click(object sender, EventArgs e)
        {
            if (mediaPlayer.VideoPlayer != null)
            {
                Paragraph p = audioVisualizer.NewSelectionParagraph;
                if (p == null)
                    p = audioVisualizer.RightClickedParagraph;

                if (p != null)
                {
                    mediaPlayer.CurrentPosition = p.StartTime.TotalSeconds;
                    Utilities.ShowSubtitle(_subtitle.Paragraphs, mediaPlayer);
                    mediaPlayer.Play();
                    _endSeconds = p.EndTime.TotalSeconds;
                }
            }
        }

        private void toolStripButtonWaveformZoomIn_Click(object sender, EventArgs e)
        {
            if (audioVisualizer.WavePeaks != null && audioVisualizer.Visible)
            {
                audioVisualizer.ZoomFactor += 0.1;
                SelectZoomTextInComboBox();
            }
        }

        private void toolStripButtonWaveformZoomOut_Click(object sender, EventArgs e)
        {
            if (audioVisualizer.WavePeaks != null && audioVisualizer.Visible)
            {
                audioVisualizer.ZoomFactor -= 0.1;
                SelectZoomTextInComboBox();
            }
        }

        private void toolStripComboBoxWaveform_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                ComboBoxZoomItem item = toolStripComboBoxWaveform.SelectedItem as ComboBoxZoomItem;
                if (item != null)
                    audioVisualizer.ZoomFactor = item.ZoomFactor;
            }
            catch
            {
            }
        }

        private void SelectZoomTextInComboBox()
        {
            int i = 0;
            foreach (object obj in toolStripComboBoxWaveform.Items)
            {
                ComboBoxZoomItem item = obj as ComboBoxZoomItem;
                if (Math.Abs(audioVisualizer.ZoomFactor - item.ZoomFactor) < 0.001)
                {
                    toolStripComboBoxWaveform.SelectedIndex = i;
                    return;
                }
                i++;
            }
        }

        private void toolStripButtonWaveformPause_Click(object sender, EventArgs e)
        {
            mediaPlayer.Pause();
        }

        private void toolStripButtonWaveformPlay_Click(object sender, EventArgs e)
        {
            mediaPlayer.Play();
        }

        private void toolStripButtonLockCenter_Click(object sender, EventArgs e)
        {
            toolStripButtonLockCenter.Checked = !toolStripButtonLockCenter.Checked;
            audioVisualizer.Locked = toolStripButtonLockCenter.Checked;
            Configuration.Settings.General.WaveformCenter = audioVisualizer.Locked;
        }

        private void trackBarWaveformPosition_ValueChanged(object sender, EventArgs e)
        {
            mediaPlayer.CurrentPosition = trackBarWaveformPosition.Value;
        }

        private void buttonCustomUrl_Click(object sender, EventArgs e)
        {
            RunCustomSearch(Configuration.Settings.VideoControls.CustomSearchUrl1);
        }

        private void buttonCustomUrl2_Click(object sender, EventArgs e)
        {
            RunCustomSearch(Configuration.Settings.VideoControls.CustomSearchUrl2);
        }

        private void ShowhideWaveformToolStripMenuItemClick(object sender, EventArgs e)
        {
            toolStripButtonToggleWaveform_Click(null, null);
        }

        private void AudioWaveformDragEnter(object sender, DragEventArgs e)
        {
            // make sure they're actually dropping files (not text or anything else)
            if (e.Data.GetDataPresent(DataFormats.FileDrop, false))
                e.Effect = DragDropEffects.All;
        }

        private void AudioWaveformDragDrop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            string fileName = files[0];
            if (files.Length != 1)
            {
                MessageBox.Show(_language.DropOnlyOneFile);
                return;
            }

            string ext = Path.GetExtension(fileName).ToLower();
            if (ext != ".wav")
            {
                if (audioVisualizer.WavePeaks == null && (Utilities.GetMovieFileExtensions().Contains(ext) || ext == ".mp3"))
                {
                    _videoFileName = fileName;
                    AudioWaveform_Click(null, null);
                    OpenVideo(_videoFileName);
                    return;
                }
                try
                {
                    var fi = new FileInfo(fileName);
                    if (fi.Length < 1024 * 500)
                    {
                        var lines = new List<string>();
                        foreach (string line in File.ReadAllLines(fileName))
                            lines.Add(line);
                        foreach (SubtitleFormat format in SubtitleFormat.AllSubtitleFormats)
                        {
                            if (format.IsMine(lines, fileName))
                            {
                                OpenSubtitle(fileName, null);
                                return;
                            }
                        }
                    }
                }
                catch
                {
                }
            }

            if (string.IsNullOrEmpty(_videoFileName))
                buttonOpenVideo_Click(null, null);
            if (_videoFileName == null)
                return;

            if (ext != ".wav")
            {
                MessageBox.Show(".wav only!");
                return;
            }

            var addWaveform = new AddWaveform();
            string spectrogramFolder = GetSpectrogramFolder(_videoFileName);
            addWaveform.InitializeViaWaveFile(fileName, spectrogramFolder);
            if (addWaveform.ShowDialog() == DialogResult.OK)
            {
                string peakWaveFileName = GetPeakWaveFileName(_videoFileName);
                addWaveform.WavePeak.WritePeakSamples(peakWaveFileName);
                var audioPeakWave = new WavePeakGenerator(peakWaveFileName);
                audioPeakWave.GenerateAllSamples();
                audioVisualizer.WavePeaks = audioPeakWave;
                timerWaveform.Start();
            }
        }

        private void toolStripMenuItemImportBluRaySup_Click(object sender, EventArgs e)
        {
            if (ContinueNewOrExit())
            {
                openFileDialog1.Title = _language.OpenBluRaySupFile;
                openFileDialog1.FileName = string.Empty;
                openFileDialog1.Filter = _language.BluRaySupFiles + "|*.sup";
                if (openFileDialog1.ShowDialog(this) == DialogResult.OK)
                {
                    ImportAndOcrBluRaySup(openFileDialog1.FileName, false);
                }
            }
        }

        private void ImportAndOcrBluRaySup(string fileName, bool showInTaskbar)
        {
            var log = new StringBuilder();
            var subtitles = BluRaySupParser.ParseBluRaySup(fileName, log);
            if (subtitles.Count > 0)
            {
                var vobSubOcr = new VobSubOcr();
                if (showInTaskbar)
                {
                    vobSubOcr.Icon = (Icon)Icon.Clone();
                    vobSubOcr.ShowInTaskbar = true;
                    vobSubOcr.ShowIcon = true;
                }
                _formPositionsAndSizes.SetPositionAndSize(vobSubOcr);
                vobSubOcr.Initialize(subtitles, Configuration.Settings.VobSubOcr, fileName);
                vobSubOcr.FileName = Path.GetFileName(fileName);
                if (vobSubOcr.ShowDialog(this) == DialogResult.OK)
                {
                    MakeHistoryForUndo(_language.BeforeImportingBluRaySupFile);
                    FileNew();
                    _subtitle.Paragraphs.Clear();
                    SetCurrentFormat(Configuration.Settings.General.DefaultSubtitleFormat);
                    _subtitle.WasLoadedWithFrameNumbers = false;
                    _subtitle.CalculateFrameNumbersFromTimeCodes(CurrentFrameRate);
                    foreach (Paragraph p in vobSubOcr.SubtitleFromOcr.Paragraphs)
                    {
                        _subtitle.Paragraphs.Add(p);
                    }

                    ShowSource();
                    SubtitleListview1.Fill(_subtitle, _subtitleAlternate);
                    _subtitleListViewIndex = -1;
                    SubtitleListview1.FirstVisibleIndex = -1;
                    SubtitleListview1.SelectIndexAndEnsureVisible(0);

                    _fileName = Path.ChangeExtension(vobSubOcr.FileName, ".srt");
                    SetTitle();
                    _converted = true;

                    Configuration.Settings.Save();
                }
                _formPositionsAndSizes.SavePositionAndSize(vobSubOcr);
            }
        }

        private void selectAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (textBoxListViewTextAlternate.Focused)
                textBoxListViewTextAlternate.SelectAll();
            else
                textBoxListViewText.SelectAll();
        }

        private void cutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (textBoxListViewTextAlternate.Focused)
                textBoxListViewTextAlternate.Cut();
            else
                textBoxListViewText.Cut();
        }

        private void copyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (textBoxListViewTextAlternate.Focused)
                textBoxListViewTextAlternate.Copy();
            else
                textBoxListViewText.Copy();
        }

        private void PasteToolStripMenuItemClick(object sender, EventArgs e)
        {
            if (textBoxListViewTextAlternate.Focused)
                textBoxListViewTextAlternate.Paste();
            else
                textBoxListViewText.Paste();
        }

        private void DeleteToolStripMenuItemClick(object sender, EventArgs e)
        {
            if (textBoxListViewTextAlternate.Focused)
                textBoxListViewTextAlternate.SelectedText = string.Empty;
            else
                textBoxListViewText.SelectedText = string.Empty;
        }

        private void NormalToolStripMenuItem1Click(object sender, EventArgs e)
        {
            TextBox tb;
            if (textBoxListViewTextAlternate.Focused)
                tb = textBoxListViewTextAlternate;
            else
                tb = textBoxListViewText;

            string text = tb.SelectedText;
            int selectionStart = tb.SelectionStart;
            text = Utilities.RemoveHtmlTags(text);
            tb.SelectedText = text;
            tb.SelectionStart = selectionStart;
            tb.SelectionLength = text.Length;
        }

        private void TextBoxListViewToggleTag(string tag)
        {
            TextBox tb;
            if (textBoxListViewTextAlternate.Focused)
                tb = textBoxListViewTextAlternate;
            else
                tb = textBoxListViewText;

            string text = string.Empty;
            int selectionStart = tb.SelectionStart;

            // No text selected.
            if (tb.SelectedText.Length == 0)
            {
                text = tb.Text;
                // Split lines (split a subtitle into its lines).
                var lines = text.Split(Utilities.NewLineChars, StringSplitOptions.RemoveEmptyEntries);
                // Get current line index (the line where the cursor is).
                int selectedLineNumber = tb.GetLineFromCharIndex(tb.SelectionStart);

                Boolean isDialog = false;
                int lineNumber = 0;
                string templine = string.Empty;
                var lineSb = new StringBuilder();
                int tagLength = 0;
                // See if lines start with "-".
                foreach (string line in lines)
                {
                    // Append line break in every line except the first one
                    if (lineNumber > 0)
                        lineSb.Append(Environment.NewLine);
                    templine = line;

                    string positionTag = string.Empty;
                    int indexOfEndBracket = templine.IndexOf('}');
                    if (templine.StartsWith("{\\") && indexOfEndBracket > 1 && indexOfEndBracket < 6)
                    {
                        // Find position tag and remove it from string.
                        positionTag = templine.Substring(0, indexOfEndBracket + 1);
                        templine = templine.Remove(0, indexOfEndBracket + 1);
                    }

                    if (templine.StartsWith('-') || templine.StartsWith("<" + tag + ">-"))
                    {
                        isDialog = true;
                        // Apply tags to current line (it is the selected line). Or remove them.
                        if (selectedLineNumber == lineNumber)
                        {
                            // Remove tags if present.
                            if (templine.Contains("<" + tag + ">"))
                            {
                                templine = templine.Replace("<" + tag + ">", string.Empty);
                                templine = templine.Replace("</" + tag + ">", string.Empty);
                                tagLength = -3;
                            }
                            else
                            {
                                // Add tags.
                                templine = string.Format("<{0}>{1}</{0}>", tag, templine);
                                tagLength = 3;
                            }
                        }
                    }
                    lineSb.Append(positionTag + templine);
                    lineNumber++;
                }
                if (isDialog)
                {
                    text = lineSb.ToString();
                    tb.Text = text;
                    tb.SelectionStart = selectionStart + tagLength;
                    tb.SelectionLength = 0;
                }
                // There are no dialog lines present.
                else
                {
                    // Remove tags if present.
                    if (text.Contains("<" + tag + ">"))
                    {
                        text = text.Replace("<" + tag + ">", string.Empty);
                        text = text.Replace("</" + tag + ">", string.Empty);
                        tb.Text = text;
                        tb.SelectionStart = selectionStart - 3;
                        tb.SelectionLength = 0;
                    }
                    else
                    {
                        // Add tags.
                        int indexOfEndBracket = text.IndexOf('}');
                        if (text.StartsWith("{\\") && indexOfEndBracket > 1 && indexOfEndBracket < 6)
                            text = string.Format("{2}<{0}>{1}</{0}>", tag, text.Remove(0, indexOfEndBracket + 1), text.Substring(0, indexOfEndBracket + 1));
                        else
                            text = string.Format("<{0}>{1}</{0}>", tag, text);
                        tb.Text = text;
                        tb.SelectionStart = selectionStart + 3;
                        tb.SelectionLength = 0;
                    }
                }
            }
            else
            {
                string post = string.Empty;
                string pre = string.Empty;
                // There is text selected
                text = tb.SelectedText;
                while (text.EndsWith(' ') || text.EndsWith(Environment.NewLine) || text.StartsWith(' ') || text.StartsWith(Environment.NewLine))
                {
                    if (text.EndsWith(' '))
                    {
                        post += " ";
                        text = text.Remove(text.Length - 1);
                    }
                    if (text.EndsWith(Environment.NewLine))
                    {
                        post += Environment.NewLine;
                        text = text.Remove(text.Length - 2);
                    }
                    if (text.StartsWith(' '))
                    {
                        pre += " ";
                        text = text.Remove(0, 1);
                    }
                    if (text.StartsWith(Environment.NewLine))
                    {
                        pre += Environment.NewLine;
                        text = text.Remove(0, 2);
                    }
                }

                // Remove tags if present.
                if (text.Contains("<" + tag + ">"))
                {
                    text = text.Replace("<" + tag + ">", string.Empty);
                    text = text.Replace("</" + tag + ">", string.Empty);
                }
                else
                {
                    // Add tags.
                    int indexOfEndBracket = text.IndexOf('}');
                    if (text.StartsWith("{\\") && indexOfEndBracket > 1 && indexOfEndBracket < 6)
                    {
                        text = string.Format("{2}<{0}>{1}</{0}>", tag, text.Remove(0, indexOfEndBracket + 1), text.Substring(0, indexOfEndBracket + 1));
                    }
                    else
                    {
                        text = string.Format("<{0}>{1}</{0}>", tag, text);
                    }
                }
                // Update text and maintain selection.
                if (pre.Length > 0)
                {
                    text = pre + text;
                    selectionStart += pre.Length;
                }
                if (post.Length > 0)
                {
                    text = text + post;
                }
                tb.SelectedText = text;
                tb.SelectionStart = selectionStart;
                tb.SelectionLength = text.Length;
            }
        }

        private void BoldToolStripMenuItem1Click(object sender, EventArgs e)
        {
            TextBoxListViewToggleTag("b");
        }

        private void ItalicToolStripMenuItem1Click(object sender, EventArgs e)
        {
            TextBoxListViewToggleTag("i");
        }

        private void UnderlineToolStripMenuItem1Click(object sender, EventArgs e)
        {
            TextBoxListViewToggleTag("u");
        }

        private void ColorToolStripMenuItem1Click(object sender, EventArgs e)
        {
            TextBox tb;
            if (textBoxListViewTextAlternate.Focused)
                tb = textBoxListViewTextAlternate;
            else
                tb = textBoxListViewText;

            //color
            string text = tb.SelectedText;
            int selectionStart = tb.SelectionStart;

            if (colorDialog1.ShowDialog(this) == DialogResult.OK)
            {
                string color = Utilities.ColorToHex(colorDialog1.Color);
                bool done = false;
                string s = text;
                if (s.StartsWith("<font "))
                {
                    int end = s.IndexOf('>');
                    if (end > 0)
                    {
                        string f = s.Substring(0, end);
                        if (f.Contains(" face=") && !f.Contains(" color="))
                        {
                            var start = s.IndexOf(" face=", StringComparison.Ordinal);
                            s = s.Insert(start, string.Format(" color=\"{0}\"", color));
                            text = s;
                            done = true;
                        }
                        else if (f.Contains(" color="))
                        {
                            int colorStart = f.IndexOf(" color=", StringComparison.Ordinal);
                            if (s.IndexOf('"', colorStart + " color=".Length + 1) > 0)
                                end = s.IndexOf('"', colorStart + " color=".Length + 1);
                            s = s.Substring(0, colorStart) + string.Format(" color=\"{0}", color) + s.Substring(end);
                            text = s;
                            done = true;
                        }
                    }
                }

                if (!done)
                    text = string.Format("<font color=\"{0}\">{1}</font>", color, text);

                tb.SelectedText = text;
                tb.SelectionStart = selectionStart;
                tb.SelectionLength = text.Length;
            }
        }

        private void FontNameToolStripMenuItemClick(object sender, EventArgs e)
        {
            TextBox tb;
            if (textBoxListViewTextAlternate.Focused)
                tb = textBoxListViewTextAlternate;
            else
                tb = textBoxListViewText;

            // font name
            string text = tb.SelectedText;
            int selectionStart = tb.SelectionStart;

            if (fontDialog1.ShowDialog(this) == DialogResult.OK)
            {
                bool done = false;

                if (text.StartsWith("<font "))
                {
                    int end = text.IndexOf('>');
                    if (end > 0)
                    {
                        string f = text.Substring(0, end);
                        if (f.Contains(" color=") && !f.Contains(" face="))
                        {
                            var start = text.IndexOf(" color=", StringComparison.Ordinal);
                            text = text.Insert(start, string.Format(" face=\"{0}\"", fontDialog1.Font.Name));
                            done = true;
                        }
                        else if (f.Contains(" face="))
                        {
                            int faceStart = f.IndexOf(" face=", StringComparison.Ordinal);
                            if (text.IndexOf('"', faceStart + " face=".Length + 1) > 0)
                                end = text.IndexOf('"', faceStart + " face=".Length + 1);
                            text = text.Substring(0, faceStart) + string.Format(" face=\"{0}", fontDialog1.Font.Name) + text.Substring(end);
                            done = true;
                        }
                    }
                }
                if (!done)
                    text = string.Format("<font face=\"{0}\">{1}</font>", fontDialog1.Font.Name, text);

                tb.SelectedText = text;
                tb.SelectionStart = selectionStart;
                tb.SelectionLength = text.Length;
            }
        }

        public void SetSubtitle(Subtitle subtitle, string message)
        {
            _subtitle = subtitle;
            SubtitleListview1.Fill(subtitle, _subtitleAlternate);
            ShowStatus(message);
        }

        #region Networking

        private void startServerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _networkSession = new NikseWebServiceSession(_subtitle, _subtitleAlternate, TimerWebServiceTick, OnUpdateUserLogEntries);
            NetworkStart networkNew = new NetworkStart();
            networkNew.Initialize(_networkSession, _fileName);
            _formPositionsAndSizes.SetPositionAndSize(networkNew);
            if (networkNew.ShowDialog(this) == DialogResult.OK)
            {
                if (GetCurrentSubtitleFormat().HasStyleSupport)
                {
                    SubtitleListview1.HideExtraColumn();
                }

                _networkSession.AppendToLog(string.Format(_language.XStartedSessionYAtZ, _networkSession.CurrentUser.UserName, _networkSession.SessionId, DateTime.Now.ToLongTimeString()));
                toolStripStatusNetworking.Visible = true;
                toolStripStatusNetworking.Text = _language.NetworkMode;
                EnableDisableControlsNotWorkingInNetworkMode(false);
                SubtitleListview1.ShowExtraColumn(_language.UserAndAction);
                SubtitleListview1.AutoSizeAllColumns(this);
                TimerWebServiceTick(null, null);
            }
            else
            {
                _networkSession = null;
            }
            _formPositionsAndSizes.SavePositionAndSize(networkNew);
        }

        private void joinSessionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _networkSession = new NikseWebServiceSession(_subtitle, _subtitleAlternate, TimerWebServiceTick, OnUpdateUserLogEntries);
            NetworkJoin networkJoin = new NetworkJoin();
            networkJoin.Initialize(_networkSession);

            if (networkJoin.ShowDialog(this) == DialogResult.OK)
            {
                _subtitle = _networkSession.Subtitle;
                _subtitleAlternate = _networkSession.OriginalSubtitle;
                if (_subtitleAlternate != null && _subtitleAlternate.Paragraphs.Count > 0)
                    SubtitleListview1.ShowAlternateTextColumn(Configuration.Settings.Language.General.OriginalText);
                _fileName = networkJoin.FileName;
                SetTitle();
                Text = Title;
                toolStripStatusNetworking.Visible = true;
                toolStripStatusNetworking.Text = _language.NetworkMode;
                EnableDisableControlsNotWorkingInNetworkMode(false);
                _networkSession.AppendToLog(string.Format(_language.XStartedSessionYAtZ, _networkSession.CurrentUser.UserName, _networkSession.SessionId, DateTime.Now.ToLongTimeString()));
                SubtitleListview1.ShowExtraColumn(_language.UserAndAction);
                SubtitleListview1.AutoSizeAllColumns(this);
                _subtitleListViewIndex = -1;
                _oldSelectedParagraph = null;

                if (Configuration.Settings.General.AllowEditOfOriginalSubtitle && _subtitleAlternate != null && _subtitleAlternate.Paragraphs.Count > 0)
                {
                    buttonUnBreak.Visible = false;
                    buttonAutoBreak.Visible = false;
                    buttonSplitLine.Visible = false;

                    textBoxListViewText.Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Bottom;
                    textBoxListViewText.Width = (groupBoxEdit.Width - (textBoxListViewText.Left + 10)) / 2;
                    textBoxListViewTextAlternate.Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Bottom;
                    textBoxListViewTextAlternate.Left = textBoxListViewText.Left + textBoxListViewText.Width + 3;
                    textBoxListViewTextAlternate.Width = textBoxListViewText.Width;
                    textBoxListViewTextAlternate.Visible = true;
                    labelAlternateText.Text = Configuration.Settings.Language.General.OriginalText;
                    labelAlternateText.Visible = true;
                    labelAlternateCharactersPerSecond.Visible = true;
                    labelTextAlternateLineLengths.Visible = true;
                    labelAlternateSingleLine.Visible = true;
                    labelTextAlternateLineTotal.Visible = true;

                    labelCharactersPerSecond.Left = textBoxListViewText.Left + (textBoxListViewText.Width - labelCharactersPerSecond.Width);
                    labelTextLineTotal.Left = textBoxListViewText.Left + (textBoxListViewText.Width - labelTextLineTotal.Width);
                    AddAlternate();
                    Main_Resize(null, null);
                    _changeAlternateSubtitleToString = _subtitleAlternate.ToText(new SubRip()).Trim();
                }
                else
                {
                    RemoveAlternate(false);
                }
                SubtitleListview1.Fill(_subtitle, _subtitleAlternate);
                SubtitleListview1.SelectIndexAndEnsureVisible(0);
                TimerWebServiceTick(null, null);
            }
            else
            {
                _networkSession = null;
            }
        }

        private void EnableDisableControlsNotWorkingInNetworkMode(bool enabled)
        {
            //Top menu
            newToolStripMenuItem.Enabled = enabled;
            openToolStripMenuItem.Enabled = enabled;
            reopenToolStripMenuItem.Enabled = enabled;
            toolStripMenuItemOpenContainingFolder.Enabled = enabled;
            toolStripMenuItemCompare.Enabled = enabled;
            toolStripMenuItemImportDvdSubtitles.Enabled = enabled;
            toolStripMenuItemSubIdx.Enabled = enabled;
            toolStripMenuItemImportBluRaySup.Enabled = enabled;
            matroskaImportStripMenuItem.Enabled = enabled;
            toolStripMenuItemManualAnsi.Enabled = enabled;
            toolStripMenuItemImportText.Enabled = enabled;
            toolStripMenuItemImportTimeCodes.Enabled = enabled;

            showHistoryforUndoToolStripMenuItem.Enabled = enabled;
            multipleReplaceToolStripMenuItem.Enabled = enabled;

            toolsToolStripMenuItem.Enabled = enabled;

            toolStripMenuItemSynchronization.Enabled = enabled;

            toolStripMenuItemAutoTranslate.Enabled = enabled;

            //Toolbar
            toolStripButtonFileNew.Enabled = enabled;
            toolStripButtonFileOpen.Enabled = enabled;
            toolStripMenuItemOpenKeepVideo.Enabled = enabled;
            toolStripMenuItemRestoreAutoBackup.Enabled = enabled;
            toolStripButtonVisualSync.Enabled = enabled;

            // textbox source
            textBoxSource.ReadOnly = !enabled;
        }

        internal void TimerWebServiceTick(object sender, EventArgs e)
        {
            if (_networkSession == null)
                return;

            List<int> deleteIndices = new List<int>();
            NetworkGetSendUpdates(deleteIndices, 0, null);
        }

        private void NetworkGetSendUpdates(List<int> deleteIndices, int insertIndex, Paragraph insertParagraph)
        {
            _networkSession.TimerStop();
            if (_networkSession == null)
                return;

            bool doReFill = false;
            bool updateListViewStatus = false;
            SubtitleListview1.SelectedIndexChanged -= SubtitleListview1_SelectedIndexChanged;
            string message = string.Empty;

            int numberOfLines = 0;
            List<SeNetworkService.SeUpdate> updates = null;
            int numberOfRetries = 10;
            while (numberOfRetries > 0)
            {
                numberOfRetries--;
                try
                {
                    updates = _networkSession.GetUpdates(out message, out numberOfLines);
                    numberOfRetries = 0;
                }
                catch (Exception exception)
                {
                    if (numberOfRetries <= 0)
                    {
                        if (exception.InnerException != null)
                            MessageBox.Show(string.Format(_language.NetworkUnableToConnectToServer, exception.InnerException.Message + Environment.NewLine + exception.InnerException.StackTrace));
                        else
                            MessageBox.Show(string.Format(_language.NetworkUnableToConnectToServer, exception.Message + Environment.NewLine + exception.StackTrace));
                        _networkSession.TimerStop();
                        if (_networkChat != null && !_networkChat.IsDisposed)
                        {
                            _networkChat.Close();
                            _networkChat = null;
                        }
                        _networkSession = null;
                        EnableDisableControlsNotWorkingInNetworkMode(true);
                        toolStripStatusNetworking.Visible = false;
                        SubtitleListview1.HideExtraColumn();
                        _networkChat = null;
                        return;
                    }
                    else
                    {
                        Application.DoEvents();
                        System.Threading.Thread.Sleep(250);
                    }
                }
            }
            int currentSelectedIndex = -1;
            if (SubtitleListview1.SelectedItems.Count > 0)
                currentSelectedIndex = SubtitleListview1.SelectedItems[0].Index;
            int oldCurrentSelectedIndex = currentSelectedIndex;
            if (message == "OK")
            {
                foreach (var update in updates)
                {
                    if (!string.IsNullOrEmpty(update.Text))
                    {
                        if (!update.Text.Contains(Environment.NewLine))
                            update.Text = update.Text.Replace("\n", Environment.NewLine);
                        update.Text = WebUtility.HtmlDecode(update.Text).Replace("<br />", Environment.NewLine);
                    }
                    if (update.User.Ip != _networkSession.CurrentUser.Ip || update.User.UserName != _networkSession.CurrentUser.UserName)
                    {
                        if (update.Action == "USR")
                        {
                            _networkSession.Users.Add(update.User);
                            if (_networkChat != null && !_networkChat.IsDisposed)
                            {
                                _networkChat.AddUser(update.User);
                            }
                            _networkSession.AppendToLog(string.Format(_language.NetworkNewUser, update.User.UserName, update.User.Ip));
                        }
                        else if (update.Action == "MSG")
                        {
                            _networkSession.ChatLog.Add(new NikseWebServiceSession.ChatEntry() { User = update.User, Message = update.Text });
                            if (_networkChat == null || _networkChat.IsDisposed)
                            {
                                _networkChat = new NetworkChat();
                                _networkChat.Initialize(_networkSession);
                                _networkChat.Show(this);
                            }
                            else
                            {
                                _networkChat.AddChatMessage(update.User, update.Text);
                            }
                            _networkSession.AppendToLog(string.Format(_language.NetworkMessage, update.User.UserName, update.User.Ip, update.Text));
                        }
                        else if (update.Action == "DEL")
                        {
                            doReFill = true;
                            _subtitle.Paragraphs.RemoveAt(update.Index);
                            if (_networkSession.LastSubtitle != null)
                                _networkSession.LastSubtitle.Paragraphs.RemoveAt(update.Index);
                            _networkSession.AppendToLog(string.Format(_language.NetworkDelete, update.User.UserName, update.User.Ip, update.Index));
                            _networkSession.AdjustUpdateLogToDelete(update.Index);

                            if (deleteIndices.Count > 0)
                            {
                                for (int i = deleteIndices.Count - 1; i >= 0; i--)
                                {
                                    int index = deleteIndices[i];
                                    if (index == update.Index)
                                        deleteIndices.RemoveAt(i);
                                    else if (index > update.Index)
                                        deleteIndices[i] = index - 1;
                                }
                            }

                            if (insertIndex > update.Index)
                                insertIndex--;
                            if (currentSelectedIndex >= 0 && currentSelectedIndex > update.Index)
                                currentSelectedIndex--;
                        }
                        else if (update.Action == "INS")
                        {
                            doReFill = true;
                            Paragraph p = new Paragraph(update.Text, update.StartMilliseconds, update.EndMilliseconds);
                            _subtitle.Paragraphs.Insert(update.Index, p);
                            if (_networkSession.LastSubtitle != null)
                                _networkSession.LastSubtitle.Paragraphs.Insert(update.Index, new Paragraph(p));
                            _networkSession.AppendToLog(string.Format(_language.NetworkInsert, update.User.UserName, update.User.Ip, update.Index, update.Text.Replace(Environment.NewLine, Configuration.Settings.General.ListViewLineSeparatorString)));
                            _networkSession.AddToWsUserLog(update.User, update.Index, update.Action, false);
                            updateListViewStatus = true;
                            _networkSession.AdjustUpdateLogToInsert(update.Index);

                            if (deleteIndices.Count > 0)
                            {
                                for (int i = deleteIndices.Count - 1; i >= 0; i--)
                                {
                                    int index = deleteIndices[i];
                                    if (index > update.Index)
                                        deleteIndices[i] = index + 1;
                                }
                            }
                            if (insertIndex > update.Index)
                                insertIndex++;
                            if (currentSelectedIndex >= 0 && currentSelectedIndex > update.Index)
                                currentSelectedIndex++;
                        }
                        else if (update.Action == "UPD")
                        {
                            updateListViewStatus = true;
                            Paragraph p = _subtitle.GetParagraphOrDefault(update.Index);
                            if (p != null)
                            {
                                p.StartTime.TotalMilliseconds = update.StartMilliseconds;
                                p.EndTime.TotalMilliseconds = update.EndMilliseconds;
                                p.Text = update.Text;
                                SubtitleListview1.SetTimeAndText(update.Index, p);
                                _networkSession.AppendToLog(string.Format(_language.NetworkUpdate, update.User.UserName, update.User.Ip, update.Index, update.Text.Replace(Environment.NewLine, Configuration.Settings.General.ListViewLineSeparatorString)));
                                _networkSession.AddToWsUserLog(update.User, update.Index, update.Action, true);
                                updateListViewStatus = true;
                            }
                            if (_networkSession.LastSubtitle != null)
                            {
                                p = _networkSession.LastSubtitle.GetParagraphOrDefault(update.Index);
                                if (p != null)
                                {
                                    p.StartTime.TotalMilliseconds = update.StartMilliseconds;
                                    p.EndTime.TotalMilliseconds = update.EndMilliseconds;
                                    p.Text = update.Text;
                                }
                            }
                        }
                        else if (update.Action == "BYE")
                        {
                            if (_networkChat != null && !_networkChat.IsDisposed)
                                _networkChat.RemoveUser(update.User);

                            SeNetworkService.SeUser removeUser = null;
                            foreach (var user in _networkSession.Users)
                            {
                                if (user.UserName == update.User.UserName)
                                {
                                    removeUser = user;
                                    break;
                                }
                            }
                            if (removeUser != null)
                                _networkSession.Users.Remove(removeUser);

                            _networkSession.AppendToLog(string.Format(_language.NetworkByeUser, update.User.UserName, update.User.Ip));
                        }
                        else
                        {
                            _networkSession.AppendToLog("UNKNOWN ACTION: " + update.Action + " by " + update.User.UserName + " (" + update.User.Ip + ")");
                        }
                    }
                }
                if (numberOfLines != _subtitle.Paragraphs.Count)
                {
                    _subtitle = _networkSession.ReloadSubtitle();
                    SubtitleListview1.Fill(_subtitle, _subtitleAlternate);
                    UpdateListviewWithUserLogEntries();
                    _networkSession.LastSubtitle = new Subtitle(_subtitle);
                    _oldSelectedParagraph = null;
                    SubtitleListview1.SelectedIndexChanged += SubtitleListview1_SelectedIndexChanged;
                    _networkSession.TimerStart();
                    RefreshSelectedParagraph();
                    return;
                }
                if (deleteIndices.Count > 0)
                {
                    deleteIndices.Sort();
                    deleteIndices.Reverse();
                    foreach (int i in deleteIndices)
                    {
                        _subtitle.Paragraphs.RemoveAt(i);
                        if (_networkSession != null && _networkSession.LastSubtitle != null && i < _networkSession.LastSubtitle.Paragraphs.Count)
                            _networkSession.LastSubtitle.Paragraphs.RemoveAt(i);
                    }

                    _networkSession.DeleteLines(deleteIndices);
                    doReFill = true;
                }
                if (insertIndex >= 0 && insertParagraph != null)
                {
                    _subtitle.Paragraphs.Insert(insertIndex, insertParagraph);
                    if (_networkSession != null && _networkSession.LastSubtitle != null && insertIndex < _networkSession.LastSubtitle.Paragraphs.Count)
                        _networkSession.LastSubtitle.Paragraphs.Insert(insertIndex, insertParagraph);
                    _networkSession.InsertLine(insertIndex, insertParagraph);
                    doReFill = true;
                }
                _networkSession.CheckForAndSubmitUpdates(); // updates only (no inserts/deletes)
            }
            else
            {
                if (message == "Session not found!")
                {
                    message = _networkSession.Restart();
                    if (message == "Reload")
                    {
                        _subtitle = _networkSession.ReloadSubtitle();
                        SubtitleListview1.Fill(_subtitle, _subtitleAlternate);
                        UpdateListviewWithUserLogEntries();
                        _networkSession.LastSubtitle = new Subtitle(_subtitle);
                        _oldSelectedParagraph = null;
                        SubtitleListview1.SelectedIndexChanged += SubtitleListview1_SelectedIndexChanged;
                        _networkSession.TimerStart();
                        RefreshSelectedParagraph();
                        return;
                    }
                    if (message == "OK")
                    {
                        _networkSession.TimerStart();
                        RefreshSelectedParagraph();
                        return;
                    }
                }
                else if (message == "User not found!")
                {
                    message = _networkSession.ReJoin();
                    if (message == "Reload")
                    {
                        _subtitle = _networkSession.ReloadSubtitle();
                        SubtitleListview1.Fill(_subtitle, _subtitleAlternate);
                        UpdateListviewWithUserLogEntries();
                        _networkSession.LastSubtitle = new Subtitle(_subtitle);
                        _oldSelectedParagraph = null;
                        SubtitleListview1.SelectedIndexChanged += SubtitleListview1_SelectedIndexChanged;
                        _networkSession.TimerStart();
                        RefreshSelectedParagraph();
                        return;
                    }
                }

                MessageBox.Show(message);
                LeaveSessionToolStripMenuItemClick(null, null);
                SubtitleListview1.SelectedIndexChanged += SubtitleListview1_SelectedIndexChanged;
                return;
            }
            if (doReFill)
            {
                _subtitle.Renumber(1);
                SubtitleListview1.Fill(_subtitle, _subtitleAlternate);
                UpdateListviewWithUserLogEntries();

                if (oldCurrentSelectedIndex != currentSelectedIndex)
                {
                    _oldSelectedParagraph = null;
                    _subtitleListViewIndex = currentSelectedIndex;
                    SubtitleListview1.SelectIndexAndEnsureVisible(_subtitleListViewIndex);
                }
                else if (_oldSelectedParagraph != null)
                {
                    Paragraph p = _subtitle.GetFirstAlike(_oldSelectedParagraph);
                    if (p == null)
                    {
                        Paragraph tmp = new Paragraph(_oldSelectedParagraph);
                        tmp.Text = textBoxListViewText.Text;
                        p = _subtitle.GetFirstAlike(tmp);
                    }

                    if (p == null)
                    {
                        int idx = oldCurrentSelectedIndex;
                        if (idx >= _subtitle.Paragraphs.Count)
                            idx = _subtitle.Paragraphs.Count - 1;

                        if (idx >= 0 && idx < _subtitle.Paragraphs.Count)
                        {
                            SubtitleListview1.SelectIndexAndEnsureVisible(idx);
                            _listViewTextUndoIndex = -1;
                            SubtitleListView1SelectedIndexChange();
                            textBoxListViewText.Text = _subtitle.Paragraphs[idx].Text;
                        }
                    }
                    else
                    {
                        _subtitleListViewIndex = _subtitle.GetIndex(p);
                        SubtitleListview1.SelectIndexAndEnsureVisible(_subtitleListViewIndex);
                        _listViewTextUndoIndex = -1;
                        SubtitleListView1SelectedIndexChange();
                    }
                }
            }
            else if (updateListViewStatus)
            {
                UpdateListviewWithUserLogEntries();
            }
            _networkSession.LastSubtitle = new Subtitle(_subtitle);
            SubtitleListview1.SelectedIndexChanged += SubtitleListview1_SelectedIndexChanged;
            _networkSession.TimerStart();
        }

        private void UpdateListviewWithUserLogEntries()
        {
            SubtitleListview1.BeginUpdate();
            foreach (UpdateLogEntry entry in _networkSession.UpdateLog)
                SubtitleListview1.SetExtraText(entry.Index, entry.ToString(), Utilities.GetColorFromUserName(entry.UserName));
            SubtitleListview1.EndUpdate();
        }

        private void LeaveSessionToolStripMenuItemClick(object sender, EventArgs e)
        {
            if (_networkSession != null)
            {
                _networkSession.TimerStop();
                _networkSession.Leave();
            }
            if (_networkChat != null && !_networkChat.IsDisposed)
            {
                _networkChat.Close();
                _networkChat = null;
            }
            _networkSession = null;
            EnableDisableControlsNotWorkingInNetworkMode(true);
            toolStripStatusNetworking.Visible = false;
            SubtitleListview1.HideExtraColumn();
            _networkChat = null;

            var format = GetCurrentSubtitleFormat();
            if (format.HasStyleSupport && _networkSession == null)
            {
                if (format.GetType() == typeof(Sami) || format.GetType() == typeof(SamiModern))
                    SubtitleListview1.ShowExtraColumn(Configuration.Settings.Language.General.Class);
                else
                    SubtitleListview1.ShowExtraColumn(Configuration.Settings.Language.General.Style);
                SubtitleListview1.DisplayExtraFromExtra = true;
            }
        }

        private void toolStripMenuItemNetworking_DropDownOpening(object sender, EventArgs e)
        {
            startServerToolStripMenuItem.Visible = _networkSession == null;
            joinSessionToolStripMenuItem.Visible = _networkSession == null;
            showSessionKeyLogToolStripMenuItem.Visible = _networkSession != null;
            leaveSessionToolStripMenuItem.Visible = _networkSession != null;
            chatToolStripMenuItem.Visible = _networkSession != null;
        }

        internal void OnUpdateUserLogEntries(object sender, EventArgs e)
        {
            UpdateListviewWithUserLogEntries();
        }

        private void toolStripStatusNetworking_Click(object sender, EventArgs e)
        {
            showSessionKeyLogToolStripMenuItem_Click(null, null);
        }

        private void showSessionKeyLogToolStripMenuItem_Click(object sender, EventArgs e)
        {
            NetworkLogAndInfo networkLog = new NetworkLogAndInfo();
            networkLog.Initialize(_networkSession);
            networkLog.ShowDialog(this);
        }

        private void chatToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_networkSession != null)
            {
                if (_networkChat == null || _networkChat.IsDisposed)
                {
                    _networkChat = new NetworkChat();
                    _networkChat.Initialize(_networkSession);
                    _networkChat.Show(this);
                }
                else
                {
                    _networkChat.WindowState = FormWindowState.Normal;
                }
            }
        }

        #endregion Networking

        private void UnDockVideoPlayer()
        {
            bool firstUndock = _videoPlayerUndocked != null && !_videoPlayerUndocked.IsDisposed;

            _videoPlayerUndocked = new VideoPlayerUndocked(this, _formPositionsAndSizes, mediaPlayer);
            _formPositionsAndSizes.SetPositionAndSize(_videoPlayerUndocked);

            if (firstUndock)
            {
                Configuration.Settings.General.UndockedVideoPosition = _videoPlayerUndocked.Left + @";" + _videoPlayerUndocked.Top + @";" + _videoPlayerUndocked.Width + @";" + _videoPlayerUndocked.Height;
            }

            Control control = null;
            if (splitContainer1.Panel2.Controls.Count == 0)
            {
                control = panelVideoPlayer;
                groupBoxVideo.Controls.Remove(control);
            }
            else if (splitContainer1.Panel2.Controls.Count > 0)
            {
                control = panelVideoPlayer;
                splitContainer1.Panel2.Controls.Clear();
            }
            if (control != null)
            {
                control.Top = 0;
                control.Left = 0;
                control.Width = _videoPlayerUndocked.PanelContainer.Width;
                control.Height = _videoPlayerUndocked.PanelContainer.Height;
                _videoPlayerUndocked.PanelContainer.Controls.Add(control);
            }
        }

        public void ReDockVideoPlayer(Control control)
        {
            groupBoxVideo.Controls.Add(control);
            mediaPlayer.FontSizeFactor = 1.0F;
            mediaPlayer.SetSubtitleFont();
            mediaPlayer.SubtitleText = string.Empty;
        }

        private void UnDockWaveform()
        {
            _waveformUndocked = new WaveformUndocked(this, _formPositionsAndSizes);
            _formPositionsAndSizes.SetPositionAndSize(_waveformUndocked);

            var control = audioVisualizer;
            groupBoxVideo.Controls.Remove(control);
            control.Top = 0;
            control.Left = 0;
            control.Width = _waveformUndocked.PanelContainer.Width;
            control.Height = _waveformUndocked.PanelContainer.Height - panelWaveformControls.Height;
            _waveformUndocked.PanelContainer.Controls.Add(control);

            var control2 = (Control)panelWaveformControls;
            groupBoxVideo.Controls.Remove(control2);
            control2.Top = control.Height;
            control2.Left = 0;
            _waveformUndocked.PanelContainer.Controls.Add(control2);

            var control3 = (Control)trackBarWaveformPosition;
            groupBoxVideo.Controls.Remove(control3);
            control3.Top = control.Height;
            control3.Left = control2.Width + 2;
            control3.Width = _waveformUndocked.PanelContainer.Width - control3.Left;
            _waveformUndocked.PanelContainer.Controls.Add(control3);
        }

        public void ReDockWaveform(Control waveform, Control buttons, Control trackBar)
        {
            groupBoxVideo.Controls.Add(waveform);
            waveform.Top = 30;
            waveform.Height = groupBoxVideo.Height - (waveform.Top + buttons.Height + 10);

            groupBoxVideo.Controls.Add(buttons);
            buttons.Top = waveform.Top + waveform.Height + 5;

            groupBoxVideo.Controls.Add(trackBar);
            trackBar.Top = buttons.Top;
        }

        private void UnDockVideoButtons()
        {
            _videoControlsUndocked = new VideoControlsUndocked(this, _formPositionsAndSizes);
            _formPositionsAndSizes.SetPositionAndSize(_videoControlsUndocked);
            var control = tabControlButtons;
            groupBoxVideo.Controls.Remove(control);
            control.Top = 25;
            control.Left = 0;
            _videoControlsUndocked.PanelContainer.Controls.Add(control);

            groupBoxVideo.Controls.Remove(checkBoxSyncListViewWithVideoWhilePlaying);
            _videoControlsUndocked.PanelContainer.Controls.Add(checkBoxSyncListViewWithVideoWhilePlaying);
            checkBoxSyncListViewWithVideoWhilePlaying.Top = 5;
            checkBoxSyncListViewWithVideoWhilePlaying.Left = 5;

            splitContainerMain.Panel2Collapsed = true;
            splitContainer1.Panel2Collapsed = true;
        }

        public void ReDockVideoButtons(Control videoButtons, Control checkBoxSyncSubWithVideo)
        {
            groupBoxVideo.Controls.Add(videoButtons);
            videoButtons.Top = 12;
            videoButtons.Left = 5;

            groupBoxVideo.Controls.Add(checkBoxSyncSubWithVideo);
            checkBoxSyncSubWithVideo.Top = 11;
            checkBoxSyncSubWithVideo.Left = videoButtons.Left + videoButtons.Width + 5;
        }

        private void UndockVideoControlsToolStripMenuItemClick(object sender, EventArgs e)
        {
            if (Configuration.Settings.General.Undocked)
                return;

            Configuration.Settings.General.Undocked = true;

            UnDockVideoPlayer();
            splitContainerListViewAndText.SplitterDistance = splitContainerListViewAndText.Height - 109;
            if (toolStripButtonToggleVideo.Checked)
            {
                _videoPlayerUndocked.Show(this);
                if (_videoPlayerUndocked.Top < -999 || _videoPlayerUndocked.Left < -999)
                {
                    _videoPlayerUndocked.WindowState = FormWindowState.Minimized;
                    _videoPlayerUndocked.Top = Top + 40;
                    _videoPlayerUndocked.Left = Math.Abs(Left - 20);
                    _videoPlayerUndocked.Width = 600;
                    _videoPlayerUndocked.Height = 400;
                }
            }

            UnDockWaveform();
            if (toolStripButtonToggleWaveform.Checked)
            {
                _waveformUndocked.Show(this);
                if (_waveformUndocked.Top < -999 || _waveformUndocked.Left < -999)
                {
                    _waveformUndocked.WindowState = FormWindowState.Minimized;
                    _waveformUndocked.Top = Top + 60;
                    _waveformUndocked.Left = Math.Abs(Left - 15);
                    _waveformUndocked.Width = 600;
                    _waveformUndocked.Height = 200;
                }
            }

            UnDockVideoButtons();
            _videoControlsUndocked.Show(this);
            if (_videoControlsUndocked.Top < -999 || _videoControlsUndocked.Left < -999)
            {
                _videoControlsUndocked.WindowState = FormWindowState.Minimized;
                _videoControlsUndocked.Top = Top + 40;
                _videoControlsUndocked.Left = Math.Abs(Left - 10);
                _videoControlsUndocked.Width = tabControlButtons.Width + 20;
                _videoControlsUndocked.Height = tabControlButtons.Height + 65;
            }

            _isVideoControlsUndocked = true;
            SetUndockedWindowsTitle();

            undockVideoControlsToolStripMenuItem.Visible = false;
            redockVideoControlsToolStripMenuItem.Visible = true;

            tabControl1_SelectedIndexChanged(null, null);
        }

        public void RedockVideoControlsToolStripMenuItemClick(object sender, EventArgs e)
        {
            if (!Configuration.Settings.General.Undocked)
                return;

            mediaPlayer.ShowNonFullScreenControls();

            SaveUndockedPositions();

            Configuration.Settings.General.Undocked = false;

            if (_videoControlsUndocked != null && !_videoControlsUndocked.IsDisposed)
            {
                var control = _videoControlsUndocked.PanelContainer.Controls[0];
                var controlCheckBox = _videoControlsUndocked.PanelContainer.Controls[1];
                _videoControlsUndocked.PanelContainer.Controls.Clear();
                ReDockVideoButtons(control, controlCheckBox);
                _videoControlsUndocked.Close();
                _videoControlsUndocked = null;
            }

            if (_waveformUndocked != null && !_waveformUndocked.IsDisposed)
            {
                var controlWaveform = _waveformUndocked.PanelContainer.Controls[0];
                var controlButtons = _waveformUndocked.PanelContainer.Controls[1];
                var controlTrackBar = _waveformUndocked.PanelContainer.Controls[2];
                _waveformUndocked.PanelContainer.Controls.Clear();
                ReDockWaveform(controlWaveform, controlButtons, controlTrackBar);
                _waveformUndocked.Close();
                _waveformUndocked = null;
            }

            if (_videoPlayerUndocked != null && !_videoPlayerUndocked.IsDisposed)
            {
                var control = _videoPlayerUndocked.PanelContainer.Controls[0];
                _videoPlayerUndocked.PanelContainer.Controls.Remove(control);
                ReDockVideoPlayer(control);
                _videoPlayerUndocked.Close();
                _videoPlayerUndocked = null;
                mediaPlayer.ShowFullscreenButton = Configuration.Settings.General.VideoPlayerShowFullscreenButton;
            }

            _isVideoControlsUndocked = false;
            _videoPlayerUndocked = null;
            _waveformUndocked = null;
            _videoControlsUndocked = null;
            ShowVideoPlayer();

            audioVisualizer.Visible = toolStripButtonToggleWaveform.Checked;
            trackBarWaveformPosition.Visible = toolStripButtonToggleWaveform.Checked;
            panelWaveformControls.Visible = toolStripButtonToggleWaveform.Checked;
            if (!toolStripButtonToggleVideo.Checked)
                HideVideoPlayer();

            mediaPlayer.Invalidate();
            Refresh();

            undockVideoControlsToolStripMenuItem.Visible = true;
            redockVideoControlsToolStripMenuItem.Visible = false;
        }

        internal void SetWaveformToggleOff()
        {
            toolStripButtonToggleWaveform.Checked = false;
        }

        internal void SetVideoPlayerToggleOff()
        {
            toolStripButtonToggleVideo.Checked = false;
        }

        private void ToolStripMenuItemInsertSubtitleClick(object sender, EventArgs e)
        {
            openFileDialog1.Title = _languageGeneral.OpenSubtitle;
            openFileDialog1.FileName = string.Empty;
            openFileDialog1.Filter = Utilities.GetOpenDialogFilter();
            if (openFileDialog1.ShowDialog(this) == DialogResult.OK)
            {
                if (!File.Exists(openFileDialog1.FileName))
                    return;

                var fi = new FileInfo(openFileDialog1.FileName);
                if (fi.Length > 1024 * 1024 * 10) // max 10 mb
                {
                    if (MessageBox.Show(string.Format(_language.FileXIsLargerThan10MB + Environment.NewLine +
                                                      Environment.NewLine +
                                                      _language.ContinueAnyway,
                                                      openFileDialog1.FileName), Title, MessageBoxButtons.YesNoCancel) != DialogResult.Yes)
                        return;
                }

                MakeHistoryForUndo(string.Format(_language.BeforeInsertLine, openFileDialog1.FileName));

                Encoding encoding;
                var subtitle = new Subtitle();
                SubtitleFormat format = subtitle.LoadSubtitle(openFileDialog1.FileName, out encoding, null);

                if (format != null)
                {
                    SaveSubtitleListviewIndexes();
                    if (format.IsFrameBased)
                        subtitle.CalculateTimeCodesFromFrameNumbers(CurrentFrameRate);
                    else
                        subtitle.CalculateFrameNumbersFromTimeCodes(CurrentFrameRate);

                    if (Configuration.Settings.General.RemoveBlankLinesWhenOpening)
                        subtitle.RemoveEmptyLines();

                    int index = FirstSelectedIndex + 1;
                    if (index < 0)
                        index = 0;
                    foreach (Paragraph p in subtitle.Paragraphs)
                    {
                        _subtitle.Paragraphs.Insert(index, new Paragraph(p));
                        index++;
                    }

                    if (Configuration.Settings.General.AllowEditOfOriginalSubtitle && _subtitleAlternate != null && _subtitleAlternate.Paragraphs.Count > 0)
                    {
                        index = FirstSelectedIndex;
                        if (index < 0)
                            index = 0;
                        Paragraph current = _subtitle.GetParagraphOrDefault(index);
                        if (current != null)
                        {
                            Paragraph original = Utilities.GetOriginalParagraph(index, current, _subtitleAlternate.Paragraphs);
                            if (original != null)
                            {
                                index = _subtitleAlternate.GetIndex(original);
                                foreach (Paragraph p in subtitle.Paragraphs)
                                {
                                    _subtitleAlternate.Paragraphs.Insert(index, new Paragraph(p));
                                    index++;
                                }
                                if (subtitle.Paragraphs.Count > 0)
                                    _subtitleAlternate.Renumber(1);
                            }
                        }
                    }
                    _subtitle.Renumber(1);
                    ShowSource();
                    SubtitleListview1.Fill(_subtitle, _subtitleAlternate);
                    RestoreSubtitleListviewIndexes();
                }
            }
        }

        private void InsertLineToolStripMenuItemClick(object sender, EventArgs e)
        {
            if (_subtitle == null || _subtitle.Paragraphs.Count == 0)
            {
                InsertBefore();
            }
            else
            {
                SubtitleListview1.SelectIndexAndEnsureVisible(_subtitle.Paragraphs.Count - 1);
                InsertAfter();
                SubtitleListview1.SelectIndexAndEnsureVisible(_subtitle.Paragraphs.Count - 1);
            }
        }

        private void CloseVideoToolStripMenuItemClick(object sender, EventArgs e)
        {
            timer1.Stop();
            if (mediaPlayer.VideoPlayer != null)
            {
                mediaPlayer.SubtitleText = string.Empty;
                mediaPlayer.VideoPlayer.DisposeVideoPlayer();
            }
            mediaPlayer.SetPlayerName(string.Empty);
            mediaPlayer.ResetTimeLabel();
            mediaPlayer.VideoPlayer = null;
            _videoFileName = null;
            _videoInfo = null;
            _videoAudioTrackNumber = -1;
            labelVideoInfo.Text = Configuration.Settings.Language.General.NoVideoLoaded;
            audioVisualizer.WavePeaks = null;
            audioVisualizer.ResetSpectrogram();
            audioVisualizer.Invalidate();
        }

        private void ToolStripMenuItemVideoDropDownOpening(object sender, EventArgs e)
        {
            if (_isVideoControlsUndocked)
            {
                redockVideoControlsToolStripMenuItem.ShortcutKeys = Utilities.GetKeys(Configuration.Settings.Shortcuts.MainVideoToggleVideoControls);
                undockVideoControlsToolStripMenuItem.ShortcutKeys = Keys.None;
            }
            else
            {
                undockVideoControlsToolStripMenuItem.ShortcutKeys = Utilities.GetKeys(Configuration.Settings.Shortcuts.MainVideoToggleVideoControls);
                redockVideoControlsToolStripMenuItem.ShortcutKeys = Keys.None;
            }

            closeVideoToolStripMenuItem.Visible = !string.IsNullOrEmpty(_videoFileName);
            setVideoOffsetToolStripMenuItem.Visible = !string.IsNullOrEmpty(_videoFileName) && Configuration.Settings.General.ShowBetaStuff;

            toolStripMenuItemSetAudioTrack.Visible = false;
            var libVlc = mediaPlayer.VideoPlayer as LibVlcDynamic;
            if (libVlc != null)
            {
                int numberOfTracks = libVlc.AudioTrackCount;
                _videoAudioTrackNumber = libVlc.AudioTrackNumber;
                if (numberOfTracks > 1)
                {
                    toolStripMenuItemSetAudioTrack.DropDownItems.Clear();
                    for (int i = 0; i < numberOfTracks; i++)
                    {
                        toolStripMenuItemSetAudioTrack.DropDownItems.Add((i + 1).ToString(), null, ChooseAudioTrack);
                        if (i == _videoAudioTrackNumber)
                            toolStripMenuItemSetAudioTrack.DropDownItems[toolStripMenuItemSetAudioTrack.DropDownItems.Count - 1].Select();
                    }
                    toolStripMenuItemSetAudioTrack.Visible = true;
                }
            }

            if (mediaPlayer.VideoPlayer != null && audioVisualizer != null && audioVisualizer.WavePeaks != null && audioVisualizer.WavePeaks.AllSamples.Count > 0)
            {
                toolStripMenuItemImportSceneChanges.Visible = true;
                toolStripMenuItemRemoveSceneChanges.Visible = audioVisualizer.SceneChanges.Count > 0;
            }
            else
            {
                toolStripMenuItemImportSceneChanges.Visible = false;
                toolStripMenuItemRemoveSceneChanges.Visible = false;
            }
        }

        private void ChooseAudioTrack(object sender, EventArgs e)
        {
            var libVlc = mediaPlayer.VideoPlayer as LibVlcDynamic;
            if (libVlc != null)
            {
                var item = sender as ToolStripItem;

                int number = int.Parse(item.Text);
                number--;
                libVlc.AudioTrackNumber = number;
                _videoAudioTrackNumber = number;
            }
        }

        private void textBoxListViewTextAlternate_TextChanged(object sender, EventArgs e)
        {
            if (_subtitleAlternate == null || _subtitleAlternate.Paragraphs.Count < 1)
                return;

            if (_subtitleListViewIndex >= 0)
            {
                Paragraph original = Utilities.GetOriginalParagraph(_subtitleListViewIndex, _subtitle.Paragraphs[_subtitleListViewIndex], _subtitleAlternate.Paragraphs);
                if (original != null)
                {
                    string text = textBoxListViewTextAlternate.Text.TrimEnd();

                    // update _subtitle + listview
                    original.Text = text;
                    UpdateListViewTextInfo(labelTextAlternateLineLengths, labelAlternateSingleLine, labelTextAlternateLineTotal, labelAlternateCharactersPerSecond, original, textBoxListViewTextAlternate);
                    SubtitleListview1.SetAlternateText(_subtitleListViewIndex, text);
                }
            }
        }

        private void TextBoxListViewTextAlternateKeyDown(object sender, KeyEventArgs e)
        {
            _listViewAlternateTextTicks = DateTime.Now.Ticks;
            if (_subtitleAlternate == null || _subtitleAlternate.Paragraphs.Count < 1)
                return;

            int numberOfNewLines = textBoxListViewTextAlternate.Text.Length - textBoxListViewTextAlternate.Text.Replace(Environment.NewLine, " ").Length;

            //Utilities.CheckAutoWrap(textBoxListViewTextAlternate, e, numberOfNewLines);

            if (e.KeyCode == Keys.Enter && e.Modifiers == Keys.None && numberOfNewLines > 1)
            {
                e.SuppressKeyPress = true;
            }
            else if (e.KeyData == _mainTextBoxAutoBreak)
            {
                if (textBoxListViewTextAlternate.Text.Length > 0)
                    textBoxListViewTextAlternate.Text = Utilities.AutoBreakLine(textBoxListViewTextAlternate.Text);
                e.SuppressKeyPress = true;
            }
            else if (e.KeyData == _mainTextBoxUnbreak)
            {
                textBoxListViewTextAlternate.Text = Utilities.UnbreakLine(textBoxListViewTextAlternate.Text);
                e.SuppressKeyPress = true;
            }
            else if (e.Modifiers == Keys.Alt && e.KeyCode == Keys.I)
            {
                if (textBoxListViewTextAlternate.SelectionLength == 0)
                {
                    if (textBoxListViewTextAlternate.Text.Contains("<i>"))
                    {
                        textBoxListViewTextAlternate.Text = HtmlUtils.RemoveOpenCloseTags(textBoxListViewTextAlternate.Text, HtmlUtils.TagItalic);
                    }
                    else
                    {
                        textBoxListViewTextAlternate.Text = string.Format("<i>{0}</i>", textBoxListViewTextAlternate.Text);
                    }
                }
                else
                {
                    TextBoxListViewToggleTag("i");
                }
            }
            if (e.Modifiers == Keys.Control && e.KeyCode == Keys.D)
            {
                textBoxListViewTextAlternate.SelectionLength = 0;
                e.SuppressKeyPress = true;
            }
            else if (_mainTextBoxSelectionToLower == e.KeyData) // selection to lowercase
            {
                if (textBoxListViewTextAlternate.SelectionLength > 0)
                {
                    int start = textBoxListViewTextAlternate.SelectionStart;
                    int length = textBoxListViewTextAlternate.SelectionLength;
                    textBoxListViewTextAlternate.SelectedText = textBoxListViewTextAlternate.SelectedText.ToLower();
                    textBoxListViewTextAlternate.SelectionStart = start;
                    textBoxListViewTextAlternate.SelectionLength = length;
                    e.SuppressKeyPress = true;
                }
            }
            else if (_mainTextBoxSelectionToUpper == e.KeyData) // selection to uppercase
            {
                if (textBoxListViewTextAlternate.SelectionLength > 0)
                {
                    int start = textBoxListViewTextAlternate.SelectionStart;
                    int length = textBoxListViewTextAlternate.SelectionLength;
                    textBoxListViewTextAlternate.SelectedText = textBoxListViewTextAlternate.SelectedText.ToUpper();
                    textBoxListViewTextAlternate.SelectionStart = start;
                    textBoxListViewTextAlternate.SelectionLength = length;
                    e.SuppressKeyPress = true;
                }
            }

            // last key down in text
            _lastTextKeyDownTicks = DateTime.Now.Ticks;

            UpdatePositionAndTotalLength(labelTextAlternateLineTotal, textBoxListViewTextAlternate);
        }

        private void OpenOriginalToolStripMenuItemClick(object sender, EventArgs e)
        {
            OpenAlternateSubtitle();
        }

        private void SaveOriginalAstoolStripMenuItemClick(object sender, EventArgs e)
        {
            SubtitleFormat currentFormat = GetCurrentSubtitleFormat();
            Utilities.SetSaveDialogFilter(saveFileDialog1, currentFormat);

            saveFileDialog1.Title = _language.SaveOriginalSubtitleAs;
            saveFileDialog1.DefaultExt = "*" + currentFormat.Extension;
            saveFileDialog1.AddExtension = true;

            if (!string.IsNullOrEmpty(_videoFileName))
                saveFileDialog1.FileName = Path.GetFileNameWithoutExtension(_videoFileName);
            else
                saveFileDialog1.FileName = Path.GetFileNameWithoutExtension(_subtitleAlternateFileName);

            if (!string.IsNullOrEmpty(openFileDialog1.InitialDirectory))
                saveFileDialog1.InitialDirectory = openFileDialog1.InitialDirectory;

            DialogResult result = saveFileDialog1.ShowDialog(this);
            if (result == DialogResult.OK)
            {
                _subtitleAlternateFileName = saveFileDialog1.FileName;
                SaveOriginalSubtitle(GetCurrentSubtitleFormat());
                Configuration.Settings.RecentFiles.Add(_fileName, FirstVisibleIndex, FirstSelectedIndex, _videoFileName, _subtitleAlternateFileName);
            }
        }

        private void SaveOriginalToolStripMenuItemClick(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(_subtitleAlternateFileName))
            {
                SaveOriginalAstoolStripMenuItemClick(null, null);
                return;
            }

            try
            {
                SaveOriginalSubtitle(GetCurrentSubtitleFormat());
            }
            catch
            {
                MessageBox.Show(string.Format(_language.UnableToSaveSubtitleX, _subtitleAlternateFileName));
            }
        }

        private void RemoveOriginalToolStripMenuItemClick(object sender, EventArgs e)
        {
            if (ContinueNewOrExitAlternate())
            {
                RemoveAlternate(true);
            }
        }

        private void RemoveAlternate(bool removeFromListView)
        {
            if (removeFromListView)
            {
                SubtitleListview1.HideAlternateTextColumn();
                SubtitleListview1.AutoSizeAllColumns(this);
                _subtitleAlternate = new Subtitle();
                _subtitleAlternateFileName = null;

                if (_fileName != null)
                {
                    Configuration.Settings.RecentFiles.Add(_fileName, FirstVisibleIndex, FirstSelectedIndex, _videoFileName, _subtitleAlternateFileName);
                    Configuration.Settings.Save();
                    UpdateRecentFilesUI();
                }
            }

            buttonUnBreak.Visible = true;
            buttonAutoBreak.Visible = true;
            textBoxListViewTextAlternate.Visible = false;
            labelAlternateText.Visible = false;
            labelAlternateCharactersPerSecond.Visible = false;
            labelTextAlternateLineLengths.Visible = false;
            labelAlternateSingleLine.Visible = false;
            labelTextAlternateLineTotal.Visible = false;
            textBoxListViewText.Width = (groupBoxEdit.Width - (textBoxListViewText.Left + 8 + buttonUnBreak.Width));
            textBoxListViewText.Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right | AnchorStyles.Bottom;

            labelCharactersPerSecond.Left = textBoxListViewText.Left + (textBoxListViewText.Width - labelCharactersPerSecond.Width);
            labelTextLineTotal.Left = textBoxListViewText.Left + (textBoxListViewText.Width - labelTextLineTotal.Width);

            SetTitle();
        }

        private void ToolStripMenuItemSpellCheckMainDropDownOpening(object sender, EventArgs e)
        {
            toolStripSeparator9.Visible = true;
            GetDictionariesToolStripMenuItem.Visible = true;
            addWordToNamesetcListToolStripMenuItem.Visible = true;
        }

        private void ToolStripMenuItemPlayRateSlowClick(object sender, EventArgs e)
        {
            if (mediaPlayer.VideoPlayer != null)
            {
                toolStripMenuItemPlayRateSlow.Checked = true;
                toolStripMenuItemPlayRateNormal.Checked = false;
                toolStripMenuItemPlayRateFast.Checked = false;
                toolStripMenuItemPlayRateVeryFast.Checked = false;
                mediaPlayer.VideoPlayer.PlayRate = 0.8;
                toolStripSplitButtonPlayRate.Image = imageListPlayRate.Images[1];
            }
        }

        private void ToolStripMenuItemPlayRateNormalClick(object sender, EventArgs e)
        {
            if (mediaPlayer.VideoPlayer != null)
            {
                toolStripMenuItemPlayRateSlow.Checked = false;
                toolStripMenuItemPlayRateNormal.Checked = true;
                toolStripMenuItemPlayRateFast.Checked = false;
                toolStripMenuItemPlayRateVeryFast.Checked = false;
                mediaPlayer.VideoPlayer.PlayRate = 1.0;
                toolStripSplitButtonPlayRate.Image = imageListPlayRate.Images[0];
            }
        }

        private void ToolStripMenuItemPlayRateFastClick(object sender, EventArgs e)
        {
            if (mediaPlayer.VideoPlayer != null)
            {
                toolStripMenuItemPlayRateSlow.Checked = false;
                toolStripMenuItemPlayRateNormal.Checked = false;
                toolStripMenuItemPlayRateFast.Checked = true;
                toolStripMenuItemPlayRateVeryFast.Checked = false;
                mediaPlayer.VideoPlayer.PlayRate = 1.2;
                toolStripSplitButtonPlayRate.Image = imageListPlayRate.Images[1];
            }
        }

        private void VeryFastToolStripMenuItemClick(object sender, EventArgs e)
        {
            if (mediaPlayer.VideoPlayer != null)
            {
                toolStripMenuItemPlayRateSlow.Checked = false;
                toolStripMenuItemPlayRateNormal.Checked = false;
                toolStripMenuItemPlayRateFast.Checked = false;
                toolStripMenuItemPlayRateVeryFast.Checked = true;
                mediaPlayer.VideoPlayer.PlayRate = 1.6;
                toolStripSplitButtonPlayRate.Image = imageListPlayRate.Images[1];
            }
        }

        private void SplitContainer1SplitterMoved(object sender, SplitterEventArgs e)
        {
            Main_Resize(null, null);
        }

        private void ButtonSplitLineClick(object sender, EventArgs e)
        {
            SplitSelectedParagraph(null, null);
        }

        private void ToolStripMenuItemCopySourceTextClick(object sender, EventArgs e)
        {
            Subtitle selectedLines = new Subtitle(_subtitle);
            selectedLines.Paragraphs.Clear();
            foreach (int index in SubtitleListview1.SelectedIndices)
                selectedLines.Paragraphs.Add(_subtitle.Paragraphs[index]);
            Clipboard.SetText(selectedLines.ToText(GetCurrentSubtitleFormat()));
        }

        public void PlayPause()
        {
            mediaPlayer.TogglePlayPause();
        }

        public void SetCurrentViaEndPositionAndGotoNext(int index)
        {
            Paragraph p = _subtitle.GetParagraphOrDefault(index);
            if (p == null)
                return;

            if (mediaPlayer.VideoPlayer == null || string.IsNullOrEmpty(_videoFileName))
            {
                MessageBox.Show(Configuration.Settings.Language.General.NoVideoLoaded);
                return;
            }

            //if (autoDuration)
            //{
            //    //TODO: auto duration
            //    //TODO: search for start via wave file (must only be minor adjustment)
            //}

            // current movie Position
            double durationTotalMilliseconds = p.Duration.TotalMilliseconds;
            double totalMillisecondsEnd = mediaPlayer.CurrentPosition * 1000.0;

            var tc = new TimeCode(totalMillisecondsEnd - durationTotalMilliseconds);
            MakeHistoryForUndo(_language.BeforeSetEndAndVideoPosition + "  " + tc);
            _makeHistoryPaused = true;

            if (p.StartTime.IsMaxTime)
            {
                p.EndTime.TotalSeconds = mediaPlayer.CurrentPosition;
                p.StartTime.TotalMilliseconds = p.EndTime.TotalMilliseconds - Utilities.GetOptimalDisplayMilliseconds(p.Text);
            }
            else
            {
                p.StartTime.TotalMilliseconds = totalMillisecondsEnd - durationTotalMilliseconds;
                p.EndTime.TotalMilliseconds = totalMillisecondsEnd;
            }

            timeUpDownStartTime.TimeCode = p.StartTime;
            var durationInSeconds = (decimal)(p.Duration.TotalSeconds);
            if (durationInSeconds >= numericUpDownDuration.Minimum && durationInSeconds <= numericUpDownDuration.Maximum)
                SetDurationInSeconds((double)durationInSeconds);

            SubtitleListview1.SelectIndexAndEnsureVisible(index + 1);
            ShowStatus(string.Format(_language.VideoControls.AdjustedViaEndTime, p.StartTime.ToShortString()));
            audioVisualizer.Invalidate();
            _makeHistoryPaused = false;
        }

        public void SetCurrentStartAutoDurationAndGotoNext(int index)
        {
            Paragraph prev = _subtitle.GetParagraphOrDefault(index - 1);
            Paragraph p = _subtitle.GetParagraphOrDefault(index);
            if (p == null)
                return;

            if (mediaPlayer.VideoPlayer == null || string.IsNullOrEmpty(_videoFileName))
            {
                MessageBox.Show(Configuration.Settings.Language.General.NoVideoLoaded);
                return;
            }

            MakeHistoryForUndoOnlyIfNotResent(string.Format(_language.VideoControls.BeforeChangingTimeInWaveformX, "#" + p.Number + " " + p.Text));

            timeUpDownStartTime.MaskedTextBox.TextChanged -= MaskedTextBoxTextChanged;
            var oldParagraph = new Paragraph(_subtitle.Paragraphs[index]);
            double videoPosition = mediaPlayer.CurrentPosition;
            if (!mediaPlayer.IsPaused)
                videoPosition -= Configuration.Settings.General.SetStartEndHumanDelay / 1000.0;

            timeUpDownStartTime.TimeCode = TimeCode.FromSeconds(videoPosition);

            double duration = Utilities.GetOptimalDisplayMilliseconds(textBoxListViewText.Text);

            _subtitle.Paragraphs[index].StartTime.TotalMilliseconds = TimeSpan.FromSeconds(videoPosition).TotalMilliseconds;
            if (prev != null && prev.EndTime.TotalMilliseconds > _subtitle.Paragraphs[index].StartTime.TotalMilliseconds)
            {
                int minDiff = Configuration.Settings.General.MininumMillisecondsBetweenLines + 1;
                if (minDiff < 1)
                    minDiff = 1;
                prev.EndTime.TotalMilliseconds = _subtitle.Paragraphs[index].StartTime.TotalMilliseconds - minDiff;
            }
            _subtitle.Paragraphs[index].EndTime.TotalMilliseconds = _subtitle.Paragraphs[index].StartTime.TotalMilliseconds + duration;
            SubtitleListview1.SetStartTimeAndDuration(index, _subtitle.Paragraphs[index]);
            timeUpDownStartTime.TimeCode = _subtitle.Paragraphs[index].StartTime;
            timeUpDownStartTime.MaskedTextBox.TextChanged += MaskedTextBoxTextChanged;
            UpdateOriginalTimeCodes(oldParagraph);
            _subtitleListViewIndex = -1;
            SubtitleListview1.SelectIndexAndEnsureVisible(index + 1);
            audioVisualizer.Invalidate();
        }

        public void SetCurrentEndNextStartAndGoToNext(int index)
        {
            Paragraph p = _subtitle.GetParagraphOrDefault(index);
            Paragraph next = _subtitle.GetParagraphOrDefault(index + 1);
            if (p == null)
                return;

            if (mediaPlayer.VideoPlayer == null || string.IsNullOrEmpty(_videoFileName))
            {
                MessageBox.Show(Configuration.Settings.Language.General.NoVideoLoaded);
                return;
            }

            MakeHistoryForUndoOnlyIfNotResent(string.Format(_language.VideoControls.BeforeChangingTimeInWaveformX, "#" + p.Number + " " + p.Text));

            double videoPosition = mediaPlayer.CurrentPosition;
            if (!mediaPlayer.IsPaused)
                videoPosition -= Configuration.Settings.General.SetStartEndHumanDelay / 1000.0;

            p.EndTime = TimeCode.FromSeconds(videoPosition);
            if (p.StartTime.IsMaxTime)
            {
                timeUpDownStartTime.MaskedTextBox.TextChanged -= MaskedTextBoxTextChanged;
                p.StartTime.TotalMilliseconds = p.EndTime.TotalMilliseconds - Utilities.GetOptimalDisplayMilliseconds(p.Text);
                if (p.StartTime.TotalMilliseconds < 0)
                    p.StartTime.TotalMilliseconds = 0;
                timeUpDownStartTime.TimeCode = p.StartTime;
                SubtitleListview1.SetStartTime(index, p);
                timeUpDownStartTime.MaskedTextBox.TextChanged += MaskedTextBoxTextChanged;

            }
            if (p.Duration.TotalSeconds < 0 || p.Duration.TotalSeconds > 10)
                p.EndTime.TotalMilliseconds = p.StartTime.TotalMilliseconds + Utilities.GetOptimalDisplayMilliseconds(p.Text);

            SubtitleListview1.SetStartTimeAndDuration(index, p);

            SetDurationInSeconds(_subtitle.Paragraphs[index].Duration.TotalSeconds + 0.001);
            if (next != null)
            {
                int addMilliseconds = Configuration.Settings.General.MininumMillisecondsBetweenLines;
                if (addMilliseconds < 1 || addMilliseconds > 500)
                    addMilliseconds = 1;

                var oldDuration = next.Duration.TotalMilliseconds;
                if (next.StartTime.IsMaxTime || next.EndTime.IsMaxTime)
                    oldDuration = Utilities.GetOptimalDisplayMilliseconds(p.Text);
                next.StartTime.TotalMilliseconds = p.EndTime.TotalMilliseconds + addMilliseconds;
                next.EndTime.TotalMilliseconds = next.StartTime.TotalMilliseconds + oldDuration;
                SubtitleListview1.SelectIndexAndEnsureVisible(index + 1);
            }
            audioVisualizer.Invalidate();
        }

        private void EditSelectAllToolStripMenuItemClick(object sender, EventArgs e)
        {
            for (int i = 0; i < SubtitleListview1.Items.Count; i++)
                SubtitleListview1.Items[i].Selected = true;
        }

        private void ToolStripMenuItemSplitTextAtCursorClick(object sender, EventArgs e)
        {
            TextBox tb = textBoxListViewText;
            if (textBoxListViewTextAlternate.Focused)
                tb = textBoxListViewTextAlternate;

            int? pos = null;
            if (tb.SelectionStart > 2 && tb.SelectionStart < tb.Text.Length - 2)
                pos = tb.SelectionStart;
            SplitSelectedParagraph(null, pos);
            tb.Focus();
            tb.SelectionStart = tb.Text.Length;
        }

        private void ContextMenuStripTextBoxListViewOpening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            TextBox tb = textBoxListViewText;
            if (textBoxListViewTextAlternate.Focused)
                tb = textBoxListViewTextAlternate;
            toolStripMenuItemSplitTextAtCursor.Visible = tb.Text.Length > 5 && tb.SelectionStart > 2 && tb.SelectionStart < tb.Text.Length - 2;

            if (IsUnicode)
            {
                if (toolStripMenuItemInsertUnicodeSymbol.DropDownItems.Count == 0)
                {
                    foreach (string s in Configuration.Settings.Tools.UnicodeSymbolsToInsert.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        toolStripMenuItemInsertUnicodeSymbol.DropDownItems.Add(s, null, InsertUnicodeGlyph);
                        if (Environment.OSVersion.Version.Major < 6 && Configuration.Settings.General.SubtitleFontName == Utilities.WinXP2KUnicodeFontName) // 6 == Vista/Win2008Server/Win7
                            toolStripMenuItemInsertUnicodeSymbol.DropDownItems[toolStripMenuItemInsertUnicodeSymbol.DropDownItems.Count - 1].Font = new Font(Utilities.WinXP2KUnicodeFontName, toolStripMenuItemInsertUnicodeSymbol.Font.Size);
                    }
                }
                toolStripMenuItemInsertUnicodeSymbol.Visible = toolStripMenuItemInsertUnicodeSymbol.DropDownItems.Count > 0;
                toolStripSeparator26.Visible = toolStripMenuItemInsertUnicodeSymbol.DropDownItems.Count > 0;

                superscriptToolStripMenuItem.Visible = tb.SelectionLength > 0;
                subscriptToolStripMenuItem.Visible = tb.SelectionLength > 0;
                toolStripMenuItemInsertUnicodeControlCharacters.Visible = true;
            }
            else
            {
                toolStripMenuItemInsertUnicodeSymbol.Visible = false;
                toolStripSeparator26.Visible = false;
                superscriptToolStripMenuItem.Visible = false;
                subscriptToolStripMenuItem.Visible = false;
                toolStripMenuItemInsertUnicodeControlCharacters.Visible = false;
            }

            var formatType = GetCurrentSubtitleFormat().GetType();
            if ((formatType == typeof(WebVTT) && tb.SelectionLength > 0))
            {
                toolStripSeparatorWebVTT.Visible = true;
                toolStripMenuItemWebVttVoice.Visible = true;
                var voices = WebVTT.GetVoices(_subtitle);
                toolStripMenuItemWebVttVoice.DropDownItems.Clear();
                foreach (string style in voices)
                {
                    toolStripMenuItemWebVttVoice.DropDownItems.Add(style, null, WebVTTSetVoiceTextBox);
                }
                toolStripMenuItemWebVttVoice.DropDownItems.Add("Set new voice...", null, WebVTTSetNewVoiceTextBox); //TODO: Translate
            }
            else
            {
                toolStripSeparatorWebVTT.Visible = false;
                toolStripMenuItemWebVttVoice.Visible = false;
            }

        }

        private void ToolStripMenuItemExportPngXmlClick(object sender, EventArgs e)
        {
            var exportBdnXmlPng = new ExportPngXml();
            exportBdnXmlPng.Initialize(_subtitle, GetCurrentSubtitleFormat(), "BDNXML", _fileName, _videoInfo);
            exportBdnXmlPng.ShowDialog(this);
        }

        private void TabControlSubtitleSelecting(object sender, TabControlCancelEventArgs e)
        {
            if (tabControlSubtitle.SelectedIndex != TabControlSourceView && textBoxSource.Text.Trim().Length > 1)
            {
                var currentFormat = GetCurrentSubtitleFormat();
                if (currentFormat != null && !currentFormat.IsTextBased)
                    return;

                var temp = new Subtitle(_subtitle);
                SubtitleFormat format = temp.ReloadLoadSubtitle(new List<string>(textBoxSource.Lines), null);
                if (format == null)
                    e.Cancel = true;
            }
        }

        private void ToolStripComboBoxFrameRateTextChanged(object sender, EventArgs e)
        {
            Configuration.Settings.General.CurrentFrameRate = CurrentFrameRate;
            if (_loading)
                return;

            SubtitleListview1.UpdateFrames(_subtitle);
        }

        private void ToolStripMenuItemGoogleMicrosoftTranslateSelLineClick(object sender, EventArgs e)
        {
            int firstSelectedIndex = FirstSelectedIndex;
            if (firstSelectedIndex >= 0)
            {
                Paragraph p = _subtitle.GetParagraphOrDefault(firstSelectedIndex);
                if (p != null)
                {
                    string defaultFromLanguage = Utilities.AutoDetectGoogleLanguage(_subtitle);
                    string defaultToLanguage = defaultFromLanguage;
                    if (_subtitleAlternate != null)
                    {
                        Paragraph o = Utilities.GetOriginalParagraph(firstSelectedIndex, p, _subtitleAlternate.Paragraphs);
                        if (o != null)
                        {
                            p = o;
                            defaultFromLanguage = Utilities.AutoDetectGoogleLanguage(_subtitleAlternate);
                        }
                    }
                    Cursor = Cursors.WaitCursor;
                    if (_googleOrMicrosoftTranslate == null || _googleOrMicrosoftTranslate.IsDisposed)
                    {
                        _googleOrMicrosoftTranslate = new GoogleOrMicrosoftTranslate();
                        _googleOrMicrosoftTranslate.InitializeFromLanguage(defaultFromLanguage, defaultToLanguage);
                    }
                    _googleOrMicrosoftTranslate.Initialize(p);
                    Cursor = Cursors.Default;
                    if (_googleOrMicrosoftTranslate.ShowDialog() == DialogResult.OK)
                    {
                        textBoxListViewText.Text = _googleOrMicrosoftTranslate.TranslatedText;
                    }
                }
            }
        }

        private void NumericUpDownSec1ValueChanged(object sender, EventArgs e)
        {
            Configuration.Settings.General.SmallDelayMilliseconds = (int)(numericUpDownSec1.Value * 1000);
        }

        private void NumericUpDownSec2ValueChanged(object sender, EventArgs e)
        {
            Configuration.Settings.General.LargeDelayMilliseconds = (int)(numericUpDownSec2.Value * 1000);
        }

        private void NumericUpDownSecAdjust1ValueChanged(object sender, EventArgs e)
        {
            Configuration.Settings.General.SmallDelayMilliseconds = (int)(numericUpDownSecAdjust1.Value * 1000);
        }

        private void NumericUpDownSecAdjust2ValueChanged(object sender, EventArgs e)
        {
            Configuration.Settings.General.LargeDelayMilliseconds = (int)(numericUpDownSecAdjust2.Value * 1000);
        }

        private void ToolStripMenuItemMakeEmptyFromCurrentClick(object sender, EventArgs e)
        {
            if (ContinueNewOrExit())
            {
                bool isAlternateVisible = SubtitleListview1.IsAlternateTextColumnVisible;
                _subtitleAlternate = new Subtitle(_subtitle);
                _subtitleAlternateFileName = null;
                int oldIndex = FirstSelectedIndex;
                if (oldIndex < 0)
                    oldIndex = 0;

                foreach (Paragraph p in _subtitle.Paragraphs)
                {
                    if (Configuration.Settings.General.RemoveBlankLinesWhenOpening && string.IsNullOrEmpty(Configuration.Settings.Tools.NewEmptyTranslationText))
                        p.Text = "-";
                    else if (Configuration.Settings.Tools.NewEmptyTranslationText != null)
                        p.Text = Configuration.Settings.Tools.NewEmptyTranslationText;
                    else
                        p.Text = string.Empty;
                }
                SubtitleListview1.ShowAlternateTextColumn(Configuration.Settings.Language.General.OriginalText);
                _subtitleListViewIndex = -1;
                SubtitleListview1.Fill(_subtitle, _subtitleAlternate);
                SubtitleListview1.SelectIndexAndEnsureVisible(oldIndex);
                textBoxListViewText.Focus();
                Configuration.Settings.General.ShowOriginalAsPreviewIfAvailable = true;

                _subtitleAlternateFileName = _fileName;
                _fileName = null;
                SetupAlternateEdit();
                ResetHistory();

                if (!isAlternateVisible)
                {
                    toolStripMenuItemShowOriginalInPreview.Checked = false;
                    Configuration.Settings.General.ShowOriginalAsPreviewIfAvailable = false;
                    audioVisualizer.Invalidate();
                }
                else if (toolStripMenuItemShowOriginalInPreview.Checked)
                {
                    toolStripMenuItemShowOriginalInPreview.Checked = false;
                    Configuration.Settings.General.ShowOriginalAsPreviewIfAvailable = false;
                    audioVisualizer.Invalidate();
                }
            }
        }

        private void ToolStripMenuItemShowOriginalInPreviewClick(object sender, EventArgs e)
        {
            toolStripMenuItemShowOriginalInPreview.Checked = !toolStripMenuItemShowOriginalInPreview.Checked;
            Configuration.Settings.General.ShowOriginalAsPreviewIfAvailable = toolStripMenuItemShowOriginalInPreview.Checked;
        }

        private void ToolStripMenuItemVideoDropDownClosed(object sender, EventArgs e)
        {
            redockVideoControlsToolStripMenuItem.ShortcutKeys = Keys.None;
            undockVideoControlsToolStripMenuItem.ShortcutKeys = Keys.None;

        }

        private void ToolsToolStripMenuItemDropDownOpening(object sender, EventArgs e)
        {
            if (_subtitle != null && _subtitle.Paragraphs.Count > 0 && _networkSession == null)
            {
                toolStripSeparator23.Visible = true;
                toolStripMenuItemMakeEmptyFromCurrent.Visible = _subtitle != null && _subtitle.Paragraphs.Count > 0 && !SubtitleListview1.IsAlternateTextColumnVisible;
                toolStripMenuItemShowOriginalInPreview.Checked = Configuration.Settings.General.ShowOriginalAsPreviewIfAvailable;
            }
            else
            {
                toolStripSeparator23.Visible = false;
                toolStripMenuItemMakeEmptyFromCurrent.Visible = false;
                toolStripMenuItemShowOriginalInPreview.Checked = false;
            }
            styleToolStripMenuItem.Visible = GetCurrentSubtitleFormat().HasStyleSupport;
        }

        private void ContextMenuStripWaveformOpening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (audioVisualizer.IsSpectrogramAvailable)
            {
                if (audioVisualizer.ShowSpectrogram && audioVisualizer.ShowWaveform)
                {
                    showWaveformAndSpectrogramToolStripMenuItem.Visible = false;
                    showOnlyWaveformToolStripMenuItem.Visible = true;
                    showOnlySpectrogramToolStripMenuItem.Visible = true;
                    toolStripSeparatorGuessTimeCodes.Visible = true;
                }
                else if (audioVisualizer.ShowSpectrogram)
                {
                    showWaveformAndSpectrogramToolStripMenuItem.Visible = true;
                    showOnlyWaveformToolStripMenuItem.Visible = true;
                    showOnlySpectrogramToolStripMenuItem.Visible = false;
                    toolStripSeparatorGuessTimeCodes.Visible = true;
                }
                else
                {
                    showWaveformAndSpectrogramToolStripMenuItem.Visible = true;
                    showOnlyWaveformToolStripMenuItem.Visible = false;
                    showOnlySpectrogramToolStripMenuItem.Visible = true;
                    toolStripSeparatorGuessTimeCodes.Visible = true;
                }
            }
            else
            {
                toolStripSeparator24.Visible = false;
                showWaveformAndSpectrogramToolStripMenuItem.Visible = false;
                showOnlyWaveformToolStripMenuItem.Visible = false;
                showOnlySpectrogramToolStripMenuItem.Visible = false;
                toolStripSeparatorGuessTimeCodes.Visible = false;
            }
        }

        private void ShowWaveformAndSpectrogramToolStripMenuItemClick(object sender, EventArgs e)
        {
            audioVisualizer.ShowSpectrogram = true;
            audioVisualizer.ShowWaveform = true;
        }

        private void ShowOnlyWaveformToolStripMenuItemClick(object sender, EventArgs e)
        {
            audioVisualizer.ShowSpectrogram = false;
            audioVisualizer.ShowWaveform = true;
        }

        private void ShowOnlySpectrogramToolStripMenuItemClick(object sender, EventArgs e)
        {
            audioVisualizer.ShowSpectrogram = true;
            audioVisualizer.ShowWaveform = false;
        }

        private void SplitContainerMainSplitterMoved(object sender, SplitterEventArgs e)
        {
            mediaPlayer.Refresh();
        }

        private void FindDoubleLinesToolStripMenuItemClick(object sender, EventArgs e)
        {
            for (int i = FirstSelectedIndex + 1; i < _subtitle.Paragraphs.Count; i++)
            {
                var current = _subtitle.GetParagraphOrDefault(i);
                var next = _subtitle.GetParagraphOrDefault(i + 1);
                if (current != null && next != null)
                {
                    if (current.Text.Trim().Equals(next.Text.Trim(), StringComparison.OrdinalIgnoreCase))
                    {
                        SubtitleListview1.SelectIndexAndEnsureVisible(i);
                        SubtitleListview1.Items[i + 1].Selected = true;
                        break;
                    }
                }
            }
        }

        private void TextBoxListViewTextAlternateMouseMove(object sender, MouseEventArgs e)
        {
            if (Control.ModifierKeys == Keys.Control && MouseButtons == MouseButtons.Left)
            {
                if (!string.IsNullOrEmpty(textBoxListViewTextAlternate.SelectedText))
                    textBoxListViewTextAlternate.DoDragDrop(textBoxListViewTextAlternate.SelectedText, DragDropEffects.Copy);
                else
                    textBoxListViewTextAlternate.DoDragDrop(textBoxListViewTextAlternate.Text, DragDropEffects.Copy);
            }
            else if (AutoRepeatContinueOn && !textBoxSearchWord.Focused && textBoxListViewTextAlternate.Focused)
            {
                string selectedText = textBoxListViewTextAlternate.SelectedText;
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

        private void EBustlToolStripMenuItemClick(object sender, EventArgs e)
        {
            var ebu = new Ebu();
            saveFileDialog1.Filter = ebu.Name + "|*" + ebu.Extension;
            saveFileDialog1.Title = _language.SaveSubtitleAs;
            saveFileDialog1.DefaultExt = "*" + ebu.Extension;
            saveFileDialog1.AddExtension = true;

            if (!string.IsNullOrEmpty(_videoFileName))
                saveFileDialog1.FileName = Path.GetFileNameWithoutExtension(_videoFileName);
            else
                saveFileDialog1.FileName = Path.GetFileNameWithoutExtension(_fileName);

            if (!string.IsNullOrEmpty(openFileDialog1.InitialDirectory))
                saveFileDialog1.InitialDirectory = openFileDialog1.InitialDirectory;

            DialogResult result = saveFileDialog1.ShowDialog(this);
            if (result == DialogResult.OK)
            {
                openFileDialog1.InitialDirectory = saveFileDialog1.InitialDirectory;
                string fileName = saveFileDialog1.FileName;
                string ext = Path.GetExtension(fileName);
                bool extOk = ext.Equals(ebu.Extension, StringComparison.OrdinalIgnoreCase);
                if (!extOk)
                {
                    if (fileName.EndsWith('.'))
                        fileName = fileName.Substring(0, fileName.Length - 1);
                    fileName += ebu.Extension;
                }
                Ebu.Save(fileName, _subtitle);
            }
        }

        private void ToolStripMenuItemCavena890Click(object sender, EventArgs e)
        {
            var cavena890 = new Cavena890();
            saveFileDialog1.Filter = cavena890.Name + "|*" + cavena890.Extension;
            saveFileDialog1.Title = _language.SaveSubtitleAs;
            saveFileDialog1.DefaultExt = "*" + cavena890.Extension;
            saveFileDialog1.AddExtension = true;

            if (!string.IsNullOrEmpty(_videoFileName))
                saveFileDialog1.FileName = Path.GetFileNameWithoutExtension(_videoFileName);
            else
                saveFileDialog1.FileName = Path.GetFileNameWithoutExtension(_fileName);

            if (!string.IsNullOrEmpty(openFileDialog1.InitialDirectory))
                saveFileDialog1.InitialDirectory = openFileDialog1.InitialDirectory;

            DialogResult result = saveFileDialog1.ShowDialog(this);
            if (result == DialogResult.OK)
            {
                openFileDialog1.InitialDirectory = saveFileDialog1.InitialDirectory;
                string fileName = saveFileDialog1.FileName;
                string ext = Path.GetExtension(fileName);
                bool extOk = ext.Equals(cavena890.Extension, StringComparison.OrdinalIgnoreCase);
                if (!extOk)
                {
                    if (fileName.EndsWith('.'))
                        fileName = fileName.Substring(0, fileName.Length - 1);
                    fileName += cavena890.Extension;
                }
                cavena890.Save(fileName, _subtitle);
            }
        }

        private void PAcScreenElectronicsToolStripMenuItemClick(object sender, EventArgs e)
        {
            var pac = new Pac();
            saveFileDialog1.Filter = pac.Name + "|*" + pac.Extension;
            saveFileDialog1.Title = _language.SaveSubtitleAs;
            saveFileDialog1.DefaultExt = "*" + pac.Extension;
            saveFileDialog1.AddExtension = true;

            if (!string.IsNullOrEmpty(_videoFileName))
                saveFileDialog1.FileName = Path.GetFileNameWithoutExtension(_videoFileName);
            else
                saveFileDialog1.FileName = Path.GetFileNameWithoutExtension(_fileName);

            if (!string.IsNullOrEmpty(openFileDialog1.InitialDirectory))
                saveFileDialog1.InitialDirectory = openFileDialog1.InitialDirectory;

            DialogResult result = saveFileDialog1.ShowDialog(this);
            if (result == DialogResult.OK)
            {
                openFileDialog1.InitialDirectory = saveFileDialog1.InitialDirectory;
                string fileName = saveFileDialog1.FileName;
                string ext = Path.GetExtension(fileName);
                bool extOk = ext.Equals(pac.Extension, StringComparison.OrdinalIgnoreCase);
                if (!extOk)
                {
                    if (fileName.EndsWith('.'))
                        fileName = fileName.Substring(0, fileName.Length - 1);
                    fileName += pac.Extension;
                }
                pac.Save(fileName, _subtitle);
            }
        }

        private void TextBoxListViewTextEnter(object sender, EventArgs e)
        {
            if (_findHelper != null)
                _findHelper.MatchInOriginal = false;
        }

        private void TextBoxListViewTextAlternateEnter(object sender, EventArgs e)
        {
            if (_findHelper != null)
                _findHelper.MatchInOriginal = true;
        }

        private void PlainTextToolStripMenuItemClick(object sender, EventArgs e)
        {
            var exportText = new ExportText();
            exportText.Initialize(_subtitle, _fileName);
            if (exportText.ShowDialog() == DialogResult.OK)
            {
                ShowStatus(Configuration.Settings.Language.Main.SubtitleExported);
            }
        }

        private void BluraySupToolStripMenuItemClick(object sender, EventArgs e)
        {
            var exportBdnXmlPng = new ExportPngXml();
            exportBdnXmlPng.Initialize(_subtitle, GetCurrentSubtitleFormat(), "BLURAYSUP", _fileName, _videoInfo);
            exportBdnXmlPng.ShowDialog(this);
        }

        private void VobSubsubidxToolStripMenuItemClick(object sender, EventArgs e)
        {
            var exportBdnXmlPng = new ExportPngXml();
            exportBdnXmlPng.Initialize(_subtitle, GetCurrentSubtitleFormat(), "VOBSUB", _fileName, _videoInfo);
            exportBdnXmlPng.ShowDialog(this);
        }

        private void TextBoxListViewTextAlternateKeyUp(object sender, KeyEventArgs e)
        {
            textBoxListViewTextAlternate.ClearUndo();
            UpdatePositionAndTotalLength(labelTextAlternateLineTotal, textBoxListViewTextAlternate);
        }

        private void TimerTextUndoTick(object sender, EventArgs e)
        {
            int index = _listViewTextUndoIndex;
            if (_listViewTextTicks == -1 || !this.CanFocus || _subtitle == null || _subtitle.Paragraphs.Count == 0 || index < 0 || index >= _subtitle.Paragraphs.Count)
                return;

            if ((DateTime.Now.Ticks - _listViewTextTicks) > 10000 * 700) // only if last typed char was entered > 700 milliseconds
            {
                if (index < 0 || index >= _subtitle.Paragraphs.Count)
                    return;
                string newText = _subtitle.Paragraphs[index].Text.TrimEnd();
                string oldText = _listViewTextUndoLast;
                if (oldText == null)
                    return;

                if (_listViewTextUndoLast != newText)
                {
                    MakeHistoryForUndo(Configuration.Settings.Language.General.Text + ": " + _listViewTextUndoLast.TrimEnd() + " -> " + newText, false);
                    int hidx = _subtitle.HistoryItems.Count - 1;
                    if (hidx >= 0 && hidx < _subtitle.HistoryItems.Count)
                        _subtitle.HistoryItems[hidx].Subtitle.Paragraphs[index].Text = _listViewTextUndoLast;

                    _listViewTextUndoLast = newText;
                    _listViewTextUndoIndex = -1;
                }
            }
        }

        private void TimerAlternateTextUndoTick(object sender, EventArgs e)
        {
            if (Configuration.Settings.General.AllowEditOfOriginalSubtitle && _subtitleAlternate != null && _subtitleAlternate.Paragraphs.Count > 0)
            {
                int index = _listViewTextUndoIndex;
                if (_listViewAlternateTextTicks == -1 || !this.CanFocus | _subtitleAlternate == null || _subtitleAlternate.Paragraphs.Count == 0 || index < 0 || index >= _subtitleAlternate.Paragraphs.Count)
                    return;

                if ((DateTime.Now.Ticks - _listViewAlternateTextTicks) > 10000 * 700) // only if last typed char was entered > 700 milliseconds
                {
                    var original = Utilities.GetOriginalParagraph(index, _subtitle.Paragraphs[index], _subtitleAlternate.Paragraphs);
                    if (original != null)
                        index = _subtitleAlternate.Paragraphs.IndexOf(original);
                    else
                        return;

                    string newText = _subtitleAlternate.Paragraphs[index].Text.TrimEnd();
                    string oldText = _listViewAlternateTextUndoLast;
                    if (oldText == null)
                        return;

                    if (_listViewAlternateTextUndoLast != newText)
                    {
                        MakeHistoryForUndo(Configuration.Settings.Language.General.Text + ": " + _listViewAlternateTextUndoLast.TrimEnd() + " -> " + newText, false);
                        _subtitle.HistoryItems[_subtitle.HistoryItems.Count - 1].OriginalSubtitle.Paragraphs[index].Text = _listViewAlternateTextUndoLast;

                        _listViewAlternateTextUndoLast = newText;
                        _listViewTextUndoIndex = -1;
                    }
                }
            }
        }

        private void UpdatePositionAndTotalLength(Label lineTotal, TextBox textBox)
        {
            if (string.IsNullOrEmpty(textBox.Text))
            {
                lineTotal.Text = string.Empty;
                return;
            }

            int extraNewLineLength = Environment.NewLine.Length - 1;
            int lineBreakPos = textBox.Text.IndexOf(Environment.NewLine, StringComparison.Ordinal);
            int pos = textBox.SelectionStart;
            var s = Utilities.RemoveHtmlTags(textBox.Text, true).Replace(Environment.NewLine, string.Empty); // we don't count new line in total length... correct?
            int totalLength = s.Length;
            string totalL = "     " + string.Format(_languageGeneral.TotalLengthX, totalLength);
            if (lineBreakPos == -1 || pos <= lineBreakPos)
            {
                lineTotal.Text = "1," + (pos + 1) + totalL;
                lineTotal.Left = textBox.Left + (textBox.Width - lineTotal.Width);
                return;
            }
            int secondLineBreakPos = textBox.Text.IndexOf(Environment.NewLine, lineBreakPos + 1, StringComparison.Ordinal);
            if (secondLineBreakPos == -1 || pos <= secondLineBreakPos + extraNewLineLength)
            {
                lineTotal.Text = "2," + (pos - (lineBreakPos + extraNewLineLength)) + totalL;
                lineTotal.Left = textBox.Left + (textBox.Width - lineTotal.Width);
                return;
            }
            int thirdLineBreakPos = textBox.Text.IndexOf(Environment.NewLine, secondLineBreakPos + 1, StringComparison.Ordinal);
            if (thirdLineBreakPos == -1 || pos < thirdLineBreakPos + (extraNewLineLength * 2))
            {
                lineTotal.Text = "3," + (pos - (secondLineBreakPos + extraNewLineLength)) + totalL;
                lineTotal.Left = textBox.Left + (textBox.Width - lineTotal.Width);
                return;
            }
            int forthLineBreakPos = textBox.Text.IndexOf(Environment.NewLine, thirdLineBreakPos + 1, StringComparison.Ordinal);
            if (forthLineBreakPos == -1 || pos < forthLineBreakPos + (extraNewLineLength * 3))
            {
                lineTotal.Text = "4," + (pos - (thirdLineBreakPos + extraNewLineLength)) + totalL;
                lineTotal.Left = textBox.Left + (textBox.Width - lineTotal.Width);
                return;
            }
            lineTotal.Text = string.Empty;
        }

        private void TextBoxListViewTextMouseClick(object sender, MouseEventArgs e)
        {
            UpdatePositionAndTotalLength(labelTextLineTotal, textBoxListViewText);
        }

        private void TextBoxListViewTextAlternateMouseClick(object sender, MouseEventArgs e)
        {
            UpdatePositionAndTotalLength(labelTextAlternateLineTotal, textBoxListViewTextAlternate);
        }

        private void TabControlButtonsDrawItem(object sender, DrawItemEventArgs e)
        {
            var tc = (TabControl)sender;
            var textBrush = new SolidBrush(ForeColor);
            var tabFont = new Font(tc.Font, FontStyle.Regular);
            if (e.State == DrawItemState.Selected)
            {
                tabFont = new Font(tc.Font, FontStyle.Bold);
                e.Graphics.FillRectangle(new SolidBrush(SystemColors.Window), e.Bounds);
            }
            Rectangle tabBounds = tc.GetTabRect(e.Index);
            var stringFlags = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
            e.Graphics.DrawString(tc.TabPages[e.Index].Text.Trim(), tabFont, textBrush, tabBounds, new StringFormat(stringFlags));
            //tc.DrawMode = TabDrawMode.Normal;
        }

        public void GotoNextSubPosFromVideoPos()
        {
            if (mediaPlayer.VideoPlayer != null && _subtitle != null)
            {
                double ms = mediaPlayer.VideoPlayer.CurrentPosition * 1000.0;
                foreach (Paragraph p in _subtitle.Paragraphs)
                {
                    if (p.EndTime.TotalMilliseconds > ms && p.StartTime.TotalMilliseconds < ms)
                    {
                        // current sub
                    }
                    else if (p.Duration.TotalSeconds < 10 && p.StartTime.TotalMilliseconds > ms)
                    {
                        mediaPlayer.VideoPlayer.CurrentPosition = p.StartTime.TotalSeconds;
                        SubtitleListview1.SelectIndexAndEnsureVisible(_subtitle.GetIndex(p));
                        return;
                    }
                }
            }
        }

        public void GotoPrevSubPosFromvideoPos()
        {
            if (mediaPlayer.VideoPlayer != null && _subtitle != null)
            {
                double ms = mediaPlayer.VideoPlayer.CurrentPosition * 1000.0;
                int i = _subtitle.Paragraphs.Count - 1;
                while (i > 0)
                {
                    Paragraph p = _subtitle.Paragraphs[i];
                    if (p.EndTime.TotalMilliseconds > ms && p.StartTime.TotalMilliseconds < ms)
                    {
                        // current sub
                    }
                    else if (p.Duration.TotalSeconds < 10 && p.StartTime.TotalMilliseconds < ms)
                    {
                        mediaPlayer.VideoPlayer.CurrentPosition = p.StartTime.TotalSeconds;
                        SubtitleListview1.SelectIndexAndEnsureVisible(_subtitle.GetIndex(p));
                        return;
                    }
                    i--;
                }
            }
        }

        private void AdobeEncoreFabImageScriptToolStripMenuItemClick(object sender, EventArgs e)
        {
            var exportBdnXmlPng = new ExportPngXml();
            exportBdnXmlPng.Initialize(_subtitle, GetCurrentSubtitleFormat(), "FAB", _fileName, _videoInfo);
            exportBdnXmlPng.ShowDialog(this);
        }

        private void ToolStripMenuItemMergeDialogClick(object sender, EventArgs e)
        {
            MergeDialogs();
        }

        private void MainKeyUp(object sender, KeyEventArgs e)
        {
            if (_mainCreateStartDownEndUpParagraph != null)
            {
                var p = _subtitle.Paragraphs[_subtitleListViewIndex];
                if (p.ToString() == _mainCreateStartDownEndUpParagraph.ToString())
                    ButtonSetEndClick(null, null);
                _mainCreateStartDownEndUpParagraph = null;
            }
            else if (_mainAdjustStartDownEndUpAndGoToNextParagraph != null)
            {
                var p = _subtitle.Paragraphs[_subtitleListViewIndex];
                if (p.ToString() == _mainAdjustStartDownEndUpAndGoToNextParagraph.ToString())
                {
                    double videoPositionInSeconds = mediaPlayer.CurrentPosition;
                    if (p.StartTime.TotalSeconds + 0.1 < videoPositionInSeconds)
                        ButtonSetEndClick(null, null);
                    SubtitleListview1.SelectIndexAndEnsureVisible(_subtitleListViewIndex + 1);
                }
                _mainAdjustStartDownEndUpAndGoToNextParagraph = null;
            }
        }

        private void ToolStripMenuItemSurroundWithMusicSymbolsClick(object sender, EventArgs e)
        {
            const string tag = "♪";
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
                    if (_subtitleAlternate != null && Configuration.Settings.General.AllowEditOfOriginalSubtitle)
                    {
                        Paragraph original = Utilities.GetOriginalParagraph(i, _subtitle.Paragraphs[i], _subtitleAlternate.Paragraphs);
                        if (original != null)
                        {
                            if (original.Text.Contains(tag))
                            {
                                original.Text = original.Text.Replace(tag, string.Empty);
                                original.Text = original.Text.Replace(Environment.NewLine + " ", Environment.NewLine).Replace(" " + Environment.NewLine, Environment.NewLine).Trim();
                            }
                            else
                            {
                                if (Configuration.Settings.Tools.MusicSymbolStyle.Equals("single", StringComparison.OrdinalIgnoreCase))
                                    original.Text = string.Format("{0} {1}", tag, original.Text.Replace(Environment.NewLine, Environment.NewLine + tag + " "));
                                else
                                    original.Text = string.Format("{0} {1} {0}", tag, original.Text.Replace(Environment.NewLine, " " + tag + Environment.NewLine + tag + " "));
                            }
                            SubtitleListview1.SetAlternateText(i, original.Text);
                        }
                    }

                    if (_subtitle.Paragraphs[i].Text.Contains(tag))
                    {
                        _subtitle.Paragraphs[i].Text = _subtitle.Paragraphs[i].Text.Replace("♪", string.Empty).Replace(Environment.NewLine + " ", Environment.NewLine).Replace(" " + Environment.NewLine, Environment.NewLine).Trim();
                    }
                    else
                    {
                        if (Configuration.Settings.Tools.MusicSymbolStyle.Equals("single", StringComparison.OrdinalIgnoreCase))
                            _subtitle.Paragraphs[i].Text = string.Format("{0} {1}", tag, _subtitle.Paragraphs[i].Text.Replace(Environment.NewLine, Environment.NewLine + tag + " "));
                        else
                            _subtitle.Paragraphs[i].Text = string.Format("{0} {1} {0}", tag, _subtitle.Paragraphs[i].Text.Replace(Environment.NewLine, " " + tag + Environment.NewLine + tag + " "));
                    }
                    SubtitleListview1.SetText(i, _subtitle.Paragraphs[i].Text);
                }
                SubtitleListview1.EndUpdate();

                ShowStatus(string.Format(_language.TagXAdded, tag));
                ShowSource();
                RefreshSelectedParagraph();
                SubtitleListview1.SelectedIndexChanged += SubtitleListview1_SelectedIndexChanged;
            }
        }

        private void SuperscriptToolStripMenuItemClick(object sender, EventArgs e)
        {
            TextBox tb;
            if (textBoxListViewTextAlternate.Focused)
                tb = textBoxListViewTextAlternate;
            else
                tb = textBoxListViewText;

            string text = tb.SelectedText;
            int selectionStart = tb.SelectionStart;
            text = Utilities.ToSuperscript(text);
            tb.SelectedText = text;
            tb.SelectionStart = selectionStart;
            tb.SelectionLength = text.Length;
        }

        private void SubscriptToolStripMenuItemClick(object sender, EventArgs e)
        {
            TextBox tb;
            if (textBoxListViewTextAlternate.Focused)
                tb = textBoxListViewTextAlternate;
            else
                tb = textBoxListViewText;

            string text = tb.SelectedText;
            int selectionStart = tb.SelectionStart;
            text = Utilities.ToSubscript(text);
            tb.SelectedText = text;
            tb.SelectionStart = selectionStart;
            tb.SelectionLength = text.Length;
        }

        private void ToolStripMenuItemImagePerFrameClick(object sender, EventArgs e)
        {
            var exportBdnXmlPng = new ExportPngXml();
            exportBdnXmlPng.Initialize(_subtitle, GetCurrentSubtitleFormat(), "IMAGE/FRAME", _fileName, _videoInfo);
            exportBdnXmlPng.ShowDialog(this);
        }

        private void toolStripMenuItemApplyDisplayTimeLimits_Click(object sender, EventArgs e)
        {
            ApplyDisplayTimeLimits(false);
        }

        private void ApplyDisplayTimeLimits(bool onlySelectedLines)
        {
            if (IsSubtitleLoaded)
            {
                ReloadFromSourceView();
                var applyDurationLimits = new ApplyDurationLimits();
                _formPositionsAndSizes.SetPositionAndSize(applyDurationLimits);

                if (onlySelectedLines)
                {
                    var selectedLines = new Subtitle { WasLoadedWithFrameNumbers = _subtitle.WasLoadedWithFrameNumbers };
                    foreach (int index in SubtitleListview1.SelectedIndices)
                        selectedLines.Paragraphs.Add(_subtitle.Paragraphs[index]);
                    applyDurationLimits.Initialize(selectedLines);
                }
                else
                {
                    applyDurationLimits.Initialize(_subtitle);
                }

                applyDurationLimits.Initialize(_subtitle);
                if (applyDurationLimits.ShowDialog(this) == DialogResult.OK)
                {
                    MakeHistoryForUndo(_language.BeforeDisplayTimeAdjustment);

                    if (onlySelectedLines)
                    { // we only update selected lines
                        int i = 0;
                        foreach (int index in SubtitleListview1.SelectedIndices)
                        {
                            _subtitle.Paragraphs[index] = applyDurationLimits.FixedSubtitle.Paragraphs[i];
                            i++;
                        }
                        ShowStatus(_language.VisualSyncPerformedOnSelectedLines);
                    }
                    else
                    {
                        SaveSubtitleListviewIndexes();
                        _subtitle.Paragraphs.Clear();
                        foreach (Paragraph p in applyDurationLimits.FixedSubtitle.Paragraphs)
                            _subtitle.Paragraphs.Add(new Paragraph(p));
                        SubtitleListview1.Fill(_subtitle, _subtitleAlternate);
                        RestoreSubtitleListviewIndexes();
                    }

                    if (IsFramesRelevant && CurrentFrameRate > 0)
                        _subtitle.CalculateFrameNumbersFromTimeCodesNoCheck(CurrentFrameRate);
                    ShowSource();
                }
                _formPositionsAndSizes.SavePositionAndSize(applyDurationLimits);
            }
            else
            {
                MessageBox.Show(_language.NoSubtitleLoaded, Title, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void generateDatetimeInfoFromVideoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ReloadFromSourceView();
            var extractDateTimeInfo = new ExtractDateTimeInfo();
            _formPositionsAndSizes.SetPositionAndSize(extractDateTimeInfo);

            if (extractDateTimeInfo.ShowDialog(this) == DialogResult.OK)
            {
                if (ContinueNewOrExit())
                {
                    MakeHistoryForUndo(_language.BeforeDisplayTimeAdjustment);

                    ResetSubtitle();
                    _subtitle.Paragraphs.Clear();
                    foreach (Paragraph p in extractDateTimeInfo.DateTimeSubtitle.Paragraphs)
                        _subtitle.Paragraphs.Add(new Paragraph(p));
                    SubtitleListview1.Fill(_subtitle, _subtitleAlternate);
                    SubtitleListview1.SelectIndexAndEnsureVisible(0);

                    if (IsFramesRelevant && CurrentFrameRate > 0)
                        _subtitle.CalculateFrameNumbersFromTimeCodesNoCheck(CurrentFrameRate);
                    ShowSource();

                    OpenVideo(extractDateTimeInfo.VideoFileName);
                }
            }
            _formPositionsAndSizes.SavePositionAndSize(extractDateTimeInfo);
        }

        private void ToolStripMenuItemRightToLeftModeClick(object sender, EventArgs e)
        {
            toolStripMenuItemRightToLeftMode.Checked = !toolStripMenuItemRightToLeftMode.Checked;
            if (textBoxListViewText.RightToLeft == RightToLeft.Yes)
            {
                textBoxListViewText.RightToLeft = RightToLeft.No;
                SubtitleListview1.RightToLeft = RightToLeft.No;
                textBoxSource.RightToLeft = RightToLeft.No;
                mediaPlayer.TextRightToLeft = RightToLeft.No;
            }
            else
            {
                textBoxListViewText.RightToLeft = RightToLeft.Yes;
                SubtitleListview1.RightToLeft = RightToLeft.Yes;
                textBoxSource.RightToLeft = RightToLeft.Yes;
                mediaPlayer.TextRightToLeft = RightToLeft.Yes;
            }
        }

        private void joinSubtitlesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ReloadFromSourceView();
            var joinSubtitles = new JoinSubtitles();
            _formPositionsAndSizes.SetPositionAndSize(joinSubtitles);
            if (joinSubtitles.ShowDialog(this) == DialogResult.OK)
            {
                if (joinSubtitles.JoinedSubtitle != null && joinSubtitles.JoinedSubtitle.Paragraphs.Count > 0 && ContinueNewOrExit())
                {
                    MakeHistoryForUndo(_language.BeforeDisplaySubtitleJoin);

                    ResetSubtitle();
                    _subtitle = joinSubtitles.JoinedSubtitle;
                    SetCurrentFormat(joinSubtitles.JoinedFormat);
                    SubtitleListview1.Fill(_subtitle, _subtitleAlternate);
                    SubtitleListview1.SelectIndexAndEnsureVisible(0);

                    if (IsFramesRelevant && CurrentFrameRate > 0)
                        _subtitle.CalculateFrameNumbersFromTimeCodesNoCheck(CurrentFrameRate);
                    ShowSource();

                    ShowStatus(_language.SubtitlesJoined);
                }
            }
            _formPositionsAndSizes.SavePositionAndSize(joinSubtitles);
        }

        private void toolStripMenuItemReverseRightToLeftStartEnd_Click(object sender, EventArgs e)
        {
            ReverseStartAndEndingForRTL();
        }

        private void toolStripMenuItemExportCapMakerPlus_Click(object sender, EventArgs e)
        {
            var capMakerPlus = new CapMakerPlus();
            saveFileDialog1.Filter = capMakerPlus.Name + "|*" + capMakerPlus.Extension;
            saveFileDialog1.Title = _language.SaveSubtitleAs;
            saveFileDialog1.DefaultExt = "*" + capMakerPlus.Extension;
            saveFileDialog1.AddExtension = true;

            if (!string.IsNullOrEmpty(_videoFileName))
                saveFileDialog1.FileName = Path.GetFileNameWithoutExtension(_videoFileName);
            else
                saveFileDialog1.FileName = Path.GetFileNameWithoutExtension(_fileName);

            if (!string.IsNullOrEmpty(openFileDialog1.InitialDirectory))
                saveFileDialog1.InitialDirectory = openFileDialog1.InitialDirectory;

            DialogResult result = saveFileDialog1.ShowDialog(this);
            if (result == DialogResult.OK)
            {
                openFileDialog1.InitialDirectory = saveFileDialog1.InitialDirectory;
                string fileName = saveFileDialog1.FileName;
                string ext = Path.GetExtension(fileName);
                bool extOk = ext.Equals(capMakerPlus.Extension, StringComparison.OrdinalIgnoreCase);
                if (!extOk)
                {
                    if (fileName.EndsWith('.'))
                        fileName = fileName.Substring(0, fileName.Length - 1);
                    fileName += capMakerPlus.Extension;
                }
                CapMakerPlus.Save(fileName, _subtitle);
            }
        }

        private void toolStripMenuItemExportCheetahCap_Click(object sender, EventArgs e)
        {
            var cheetahCaption = new CheetahCaption();
            saveFileDialog1.Filter = cheetahCaption.Name + "|*" + cheetahCaption.Extension;
            saveFileDialog1.Title = _language.SaveSubtitleAs;
            saveFileDialog1.DefaultExt = "*" + cheetahCaption.Extension;
            saveFileDialog1.AddExtension = true;

            if (!string.IsNullOrEmpty(_videoFileName))
                saveFileDialog1.FileName = Path.GetFileNameWithoutExtension(_videoFileName);
            else
                saveFileDialog1.FileName = Path.GetFileNameWithoutExtension(_fileName);

            if (!string.IsNullOrEmpty(openFileDialog1.InitialDirectory))
                saveFileDialog1.InitialDirectory = openFileDialog1.InitialDirectory;

            DialogResult result = saveFileDialog1.ShowDialog(this);
            if (result == DialogResult.OK)
            {
                openFileDialog1.InitialDirectory = saveFileDialog1.InitialDirectory;
                string fileName = saveFileDialog1.FileName;
                string ext = Path.GetExtension(fileName);
                bool extOk = ext.Equals(cheetahCaption.Extension, StringComparison.OrdinalIgnoreCase);
                if (!extOk)
                {
                    if (fileName.EndsWith('.'))
                        fileName = fileName.Substring(0, fileName.Length - 1);
                    fileName += cheetahCaption.Extension;
                }
                CheetahCaption.Save(fileName, _subtitle);
            }
        }

        private void toolStripMenuItemExportCaptionInc_Click(object sender, EventArgs e)
        {
            var captionInc = new CaptionsInc();
            saveFileDialog1.Filter = captionInc.Name + "|*" + captionInc.Extension;
            saveFileDialog1.Title = _language.SaveSubtitleAs;
            saveFileDialog1.DefaultExt = "*" + captionInc.Extension;
            saveFileDialog1.AddExtension = true;

            if (!string.IsNullOrEmpty(_videoFileName))
                saveFileDialog1.FileName = Path.GetFileNameWithoutExtension(_videoFileName);
            else
                saveFileDialog1.FileName = Path.GetFileNameWithoutExtension(_fileName);

            if (!string.IsNullOrEmpty(openFileDialog1.InitialDirectory))
                saveFileDialog1.InitialDirectory = openFileDialog1.InitialDirectory;

            DialogResult result = saveFileDialog1.ShowDialog(this);
            if (result == DialogResult.OK)
            {
                openFileDialog1.InitialDirectory = saveFileDialog1.InitialDirectory;
                string fileName = saveFileDialog1.FileName;
                string ext = Path.GetExtension(fileName);
                bool extOk = ext.Equals(captionInc.Extension, StringComparison.OrdinalIgnoreCase);
                if (!extOk)
                {
                    if (fileName.EndsWith('.'))
                        fileName = fileName.Substring(0, fileName.Length - 1);
                    fileName += captionInc.Extension;
                }
                CaptionsInc.Save(fileName, _subtitle);
            }
        }

        private void toolStripMenuItemExportUltech130_Click(object sender, EventArgs e)
        {
            var ultech130 = new Ultech130();
            saveFileDialog1.Filter = ultech130.Name + "|*" + ultech130.Extension;
            saveFileDialog1.Title = _language.SaveSubtitleAs;
            saveFileDialog1.DefaultExt = "*" + ultech130.Extension;
            saveFileDialog1.AddExtension = true;

            if (!string.IsNullOrEmpty(_videoFileName))
                saveFileDialog1.FileName = Path.GetFileNameWithoutExtension(_videoFileName);
            else
                saveFileDialog1.FileName = Path.GetFileNameWithoutExtension(_fileName);

            if (!string.IsNullOrEmpty(openFileDialog1.InitialDirectory))
                saveFileDialog1.InitialDirectory = openFileDialog1.InitialDirectory;

            DialogResult result = saveFileDialog1.ShowDialog(this);
            if (result == DialogResult.OK)
            {
                openFileDialog1.InitialDirectory = saveFileDialog1.InitialDirectory;
                string fileName = saveFileDialog1.FileName;
                string ext = Path.GetExtension(fileName);
                bool extOk = ext.Equals(ultech130.Extension, StringComparison.OrdinalIgnoreCase);
                if (!extOk)
                {
                    if (fileName.EndsWith('.'))
                        fileName = fileName.Substring(0, fileName.Length - 1);
                    fileName += ultech130.Extension;
                }
                Ultech130.Save(fileName, _subtitle);
            }
        }

        private void toolStripMenuItemAssStyles_Click(object sender, EventArgs e)
        {
            var formatType = GetCurrentSubtitleFormat().GetType();
            if (formatType == typeof(AdvancedSubStationAlpha) || formatType == typeof(SubStationAlpha))
            {
                var styles = new SubStationAlphaStyles(_subtitle, GetCurrentSubtitleFormat());
                if (styles.ShowDialog(this) == DialogResult.OK)
                {
                    _subtitle.Header = styles.Header;
                    var styleList = AdvancedSubStationAlpha.GetStylesFromHeader(_subtitle.Header);
                    if (styleList.Count > 0)
                    {
                        for (int i = 0; i < _subtitle.Paragraphs.Count; i++)
                        {
                            Paragraph p = _subtitle.Paragraphs[i];

                            if (p.Extra == null)
                            {
                                p.Extra = styleList[0];
                                SubtitleListview1.SetExtraText(i, p.Extra, SubtitleListview1.ForeColor);
                            }
                            else
                            {
                                bool found = false;
                                foreach (string s in styleList)
                                {
                                    if (s.Equals(p.Extra, StringComparison.OrdinalIgnoreCase))
                                        found = true;
                                }
                                if (!found)
                                {
                                    p.Extra = styleList[0];
                                    SubtitleListview1.SetExtraText(i, p.Extra, SubtitleListview1.ForeColor);
                                }
                            }
                        }

                    }

                }
            }
            else if (formatType == typeof(TimedText10) || formatType == typeof(ItunesTimedText))
            {
                var styles = new TimedTextStyles(_subtitle);
                if (styles.ShowDialog(this) == DialogResult.OK)
                    _subtitle.Header = styles.Header;
            }
        }

        private void toolStripMenuItemSubStationAlpha_Click(object sender, EventArgs e)
        {
            var properties = new SubStationAlphaProperties(_subtitle, GetCurrentSubtitleFormat(), _videoFileName, _fileName);
            _formPositionsAndSizes.SetPositionAndSize(properties, true);
            properties.ShowDialog(this);
            _formPositionsAndSizes.SavePositionAndSize(properties);
        }

        private static void SetAlignTag(Paragraph p, string tag)
        {
            if (p.Text.StartsWith("{\\a") && p.Text.Length > 5 && p.Text[5] == '}')
                p.Text = p.Text.Remove(0, 6);
            else if (p.Text.StartsWith("{\\a") && p.Text.Length > 4 && p.Text[4] == '}')
                p.Text = p.Text.Remove(0, 5);
            p.Text = string.Format(@"{0}{1}", tag, p.Text);
        }

        private void toolStripMenuItemAlignment_Click(object sender, EventArgs e)
        {
            var f = new AlignmentPicker();
            f.TopMost = true;
            f.StartPosition = FormStartPosition.Manual;
            f.Left = Cursor.Position.X - 150;
            f.Top = Cursor.Position.Y - 75;
            if (f.ShowDialog(this) == DialogResult.OK)
            {
                string tag = string.Empty;
                var format = GetCurrentSubtitleFormat();
                if (format.GetType() == typeof(SubStationAlpha))
                {
                    //1: Bottom left
                    //2: Bottom center
                    //3: Bottom right
                    //9: Middle left
                    //10: Middle center
                    //11: Middle right
                    //5: Top left
                    //6: Top center
                    //7: Top right
                    switch (f.Alignment)
                    {
                        case ContentAlignment.BottomLeft:
                            tag = "{\\a1}";
                            break;
                        case ContentAlignment.BottomCenter:
                            tag = "{\\a2}";
                            break;
                        case ContentAlignment.BottomRight:
                            tag = "{\\a3}";
                            break;
                        case ContentAlignment.MiddleLeft:
                            tag = "{\\a9}";
                            break;
                        case ContentAlignment.MiddleCenter:
                            tag = "{\\a10}";
                            break;
                        case ContentAlignment.MiddleRight:
                            tag = "{\\a11}";
                            break;
                        case ContentAlignment.TopLeft:
                            tag = "{\\a5}";
                            break;
                        case ContentAlignment.TopCenter:
                            tag = "{\\a6}";
                            break;
                        case ContentAlignment.TopRight:
                            tag = "{\\a7}";
                            break;
                    }
                }
                else
                {
                    //1: Bottom left
                    //2: Bottom center
                    //3: Bottom right
                    //4: Middle left
                    //5: Middle center
                    //6: Middle right
                    //7: Top left
                    //8: Top center
                    //9: Top right
                    switch (f.Alignment)
                    {
                        case ContentAlignment.BottomLeft:
                            tag = "{\\an1}";
                            break;
                        case ContentAlignment.BottomCenter:
                            if (format.GetType() == typeof(SubRip))
                                tag = string.Empty;
                            else
                                tag = "{\\an2}";
                            break;
                        case ContentAlignment.BottomRight:
                            tag = "{\\an3}";
                            break;
                        case ContentAlignment.MiddleLeft:
                            tag = "{\\an4}";
                            break;
                        case ContentAlignment.MiddleCenter:
                            tag = "{\\an5}";
                            break;
                        case ContentAlignment.MiddleRight:
                            tag = "{\\an6}";
                            break;
                        case ContentAlignment.TopLeft:
                            tag = "{\\an7}";
                            break;
                        case ContentAlignment.TopCenter:
                            tag = "{\\an8}";
                            break;
                        case ContentAlignment.TopRight:
                            tag = "{\\an9}";
                            break;
                    }
                }
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
                        if (_subtitleAlternate != null && Configuration.Settings.General.AllowEditOfOriginalSubtitle)
                        {
                            Paragraph original = Utilities.GetOriginalParagraph(i, _subtitle.Paragraphs[i], _subtitleAlternate.Paragraphs);
                            if (original != null)
                            {
                                SetAlignTag(original, tag);
                                SubtitleListview1.SetAlternateText(i, original.Text);
                            }
                        }
                        SetAlignTag(_subtitle.Paragraphs[i], tag);
                        SubtitleListview1.SetText(i, _subtitle.Paragraphs[i].Text);
                    }
                    SubtitleListview1.EndUpdate();

                    ShowStatus(string.Format(_language.TagXAdded, tag));
                    ShowSource();
                    RefreshSelectedParagraph();
                    SubtitleListview1.SelectedIndexChanged += SubtitleListview1_SelectedIndexChanged;
                }
            }
        }

        private void toolStripMenuItemRestoreAutoBackup_Click(object sender, EventArgs e)
        {
            _lastDoNotPrompt = string.Empty;
            var restoreAutoBackup = new RestoreAutoBackup();
            _formPositionsAndSizes.SetPositionAndSize(restoreAutoBackup);
            if (restoreAutoBackup.ShowDialog(this) == DialogResult.OK && !string.IsNullOrEmpty(restoreAutoBackup.AutoBackupFileName))
            {
                if (ContinueNewOrExit())
                {
                    OpenSubtitle(restoreAutoBackup.AutoBackupFileName, null);
                    _fileName = _fileName.Remove(0, Configuration.AutoBackupFolder.Length).TrimStart(Path.DirectorySeparatorChar);
                    _converted = true;
                    SetTitle();
                }
            }
            _formPositionsAndSizes.SavePositionAndSize(restoreAutoBackup);
        }

        private void labelStatus_Click(object sender, EventArgs e)
        {
            if (_statusLog.Length > 0)
            {
                var statusLog = new StatusLog(_statusLog.ToString());
                _formPositionsAndSizes.SetPositionAndSize(statusLog);
                statusLog.ShowDialog(this);
                _formPositionsAndSizes.SavePositionAndSize(statusLog);
            }
        }

        private void toolStripMenuItemStatistics_Click(object sender, EventArgs e)
        {
            var stats = new Statistics(_subtitle, _fileName, GetCurrentSubtitleFormat());
            _formPositionsAndSizes.SetPositionAndSize(stats);
            stats.ShowDialog(this);
            _formPositionsAndSizes.SavePositionAndSize(stats);
        }

        private void toolStripMenuItemDCinemaProperties_Click(object sender, EventArgs e)
        {
            if (GetCurrentSubtitleFormat().GetType() == typeof(DCSubtitle))
            {
                var properties = new DCinemaPropertiesInterop();
                _formPositionsAndSizes.SetPositionAndSize(properties, true);
                properties.ShowDialog(this);
                _formPositionsAndSizes.SavePositionAndSize(properties);
            }
            else
            {
                var properties = new DCinemaPropertiesSmpte();
                _formPositionsAndSizes.SetPositionAndSize(properties, true);
                properties.ShowDialog(this);
                _formPositionsAndSizes.SavePositionAndSize(properties);
            }
        }

        private void toolStripMenuItemTextTimeCodePair_Click(object sender, EventArgs e)
        {
            if (saveFileDialog1.ShowDialog(this) == DialogResult.OK)
            {
                saveFileDialog1.Filter = _language.TextFiles + "|*.txt";
                saveFileDialog1.Title = _language.SaveSubtitleAs;
                saveFileDialog1.DefaultExt = "*.txt";
                saveFileDialog1.AddExtension = true;

                string fname = saveFileDialog1.FileName;
                if (string.IsNullOrEmpty(fname))
                    fname = "ATS";
                if (!fname.EndsWith(".txt"))
                    fname += ".txt";
                string fileNameTimeCode = fname.Insert(fname.Length - 4, "_timecode");
                string fileNameText = fname.Insert(fname.Length - 4, "_text");

                var timeCodeLines = new StringBuilder();
                var textLines = new StringBuilder();

                foreach (Paragraph p in _subtitle.Paragraphs)
                {
                    timeCodeLines.AppendLine(string.Format("{0:00}:{1:00}:{2:00}:{3:00}", p.StartTime.Hours, p.StartTime.Minutes, p.StartTime.Seconds, SubtitleFormat.MillisecondsToFramesMaxFrameRate(p.StartTime.Milliseconds)));
                    timeCodeLines.AppendLine(string.Format("{0:00}:{1:00}:{2:00}:{3:00}", p.EndTime.Hours, p.EndTime.Minutes, p.EndTime.Seconds, SubtitleFormat.MillisecondsToFramesMaxFrameRate(p.EndTime.Milliseconds)));

                    textLines.AppendLine(Utilities.RemoveHtmlTags(p.Text).Replace(Environment.NewLine, "|"));
                    textLines.AppendLine();
                }

                File.WriteAllText(fileNameTimeCode, timeCodeLines.ToString(), Encoding.UTF8);
                File.WriteAllText(fileNameText, textLines.ToString(), Encoding.UTF8);
            }
        }

        private void textWordsPerMinutewpmToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SortSubtitle(SubtitleSortCriteria.WordsPerMinute, (sender as ToolStripItem).Text);
        }

        private void toolStripMenuItemTTPropertiesClick(object sender, EventArgs e)
        {
            if (GetCurrentSubtitleFormat().GetType() == typeof(TimedText10) || GetCurrentSubtitleFormat().GetType() == typeof(ItunesTimedText))
            {
                var properties = new TimedTextProperties(_subtitle);
                _formPositionsAndSizes.SetPositionAndSize(properties, true);
                properties.ShowDialog(this);
                _formPositionsAndSizes.SavePositionAndSize(properties);
            }
        }

        private void ToolStripMenuItemSaveSelectedLinesClick(object sender, EventArgs e)
        {
            var newSub = new Subtitle(_subtitle);
            newSub.Header = _subtitle.Header;
            newSub.Paragraphs.Clear();
            foreach (int index in SubtitleListview1.SelectedIndices)
                newSub.Paragraphs.Add(_subtitle.Paragraphs[index]);

            SubtitleFormat currentFormat = GetCurrentSubtitleFormat();
            Utilities.SetSaveDialogFilter(saveFileDialog1, currentFormat);
            saveFileDialog1.Title = _language.SaveSubtitleAs;
            saveFileDialog1.DefaultExt = "*" + currentFormat.Extension;
            saveFileDialog1.AddExtension = true;
            if (!string.IsNullOrEmpty(_fileName))
                saveFileDialog1.InitialDirectory = Path.GetDirectoryName(_fileName);

            if (saveFileDialog1.ShowDialog(this) == DialogResult.OK)
            {
                int index = 0;
                foreach (SubtitleFormat format in SubtitleFormat.AllSubtitleFormats)
                {
                    if (saveFileDialog1.FilterIndex == index + 1)
                    {
                        if (format.IsTextBased)
                        {
                            // only allow current extension or ".txt"
                            string fileName = saveFileDialog1.FileName;
                            string ext = Path.GetExtension(fileName).ToLower();
                            bool extOk = ext.Equals(format.Extension, StringComparison.OrdinalIgnoreCase) || format.AlternateExtensions.Contains(ext) || ext == ".txt";
                            if (!extOk)
                            {
                                if (fileName.EndsWith('.'))
                                    fileName = fileName.Substring(0, _fileName.Length - 1);
                                fileName += format.Extension;
                            }

                            string allText = newSub.ToText(format);
                            File.WriteAllText(fileName, allText, GetCurrentEncoding());
                            ShowStatus(string.Format(_language.XLinesSavedAsY, newSub.Paragraphs.Count, fileName));
                            return;
                        }
                    }
                    index++;
                }
            }
        }

        private void GuessTimeCodesToolStripMenuItemClick(object sender, EventArgs e)
        {
            var form = new WaveformGenerateTimeCodes();
            if (form.ShowDialog(this) == DialogResult.OK)
            {
                MakeHistoryForUndoOnlyIfNotResent(string.Format(_language.BeforeGuessingTimeCodes));

                double startFrom = 0;
                if (form.StartFromVideoPosition)
                    startFrom = mediaPlayer.CurrentPosition;

                if (form.DeleteAll)
                {
                    _subtitle.Paragraphs.Clear();
                }
                else if (form.DeleteForward)
                {
                    for (int i = _subtitle.Paragraphs.Count - 1; i > 0; i--)
                    {
                        if (_subtitle.Paragraphs[i].EndTime.TotalSeconds + 1 > startFrom)
                            _subtitle.Paragraphs.RemoveAt(i);
                    }
                }
                audioVisualizer.GenerateTimeCodes(form.BlockSize, form.VolumeMinimum, form.VolumeMaximum, form.DefaultMilliseconds);
                if (IsFramesRelevant && CurrentFrameRate > 0)
                    _subtitle.CalculateFrameNumbersFromTimeCodesNoCheck(CurrentFrameRate);
                SubtitleListview1.Fill(_subtitle, _subtitleAlternate);
                RefreshSelectedParagraph();
            }
        }

        private void DvdStudioProStl_Click(object sender, EventArgs e)
        {
            var exportBdnXmlPng = new ExportPngXml();
            exportBdnXmlPng.Initialize(_subtitle, GetCurrentSubtitleFormat(), "STL", _fileName, _videoInfo);
            exportBdnXmlPng.ShowDialog(this);
        }

        private void toolStripMenuItemPlugins_Click(object sender, EventArgs e)
        {
            var form = new PluginsGet();
            form.ShowDialog(this);
            LoadPlugins();
        }

        private void toolStripMenuItemUndo_Click(object sender, EventArgs e)
        {
            UndoLastAction();
        }

        private void toolStripMenuItemRedo_Click(object sender, EventArgs e)
        {
            RedoLastAction();
        }

        private void seekSilenceToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (audioVisualizer == null)
                return;

            var form = new SeekSilence();
            if (form.ShowDialog(this) == DialogResult.OK)
            {
                if (form.SeekForward)
                {
                    audioVisualizer.FindDataBelowThreshold(form.VolumeBelow, form.SecondsDuration);
                }
                else
                {
                    audioVisualizer.FindDataBelowThresholdBack(form.VolumeBelow, form.SecondsDuration);
                }
            }
        }

        private void toolStripMenuItemPasteSpecial_Click(object sender, EventArgs e)
        {
            string text = Clipboard.GetText();
            var tmp = new Subtitle();
            var format = new SubRip();
            var list = new List<string>();
            foreach (string line in text.Replace(Environment.NewLine, "\n").Split('\n'))
                list.Add(line);
            format.LoadSubtitle(tmp, list, null);

            if (SubtitleListview1.SelectedItems.Count == 1 && text.Length > 0)
            {
                var form = new ColumnPaste(SubtitleListview1.IsAlternateTextColumnVisible && _subtitleAlternate != null && Configuration.Settings.General.AllowEditOfOriginalSubtitle, tmp.Paragraphs.Count == 0);
                if (form.ShowDialog(this) == DialogResult.OK)
                {
                    MakeHistoryForUndo(_language.BeforeColumnPaste);

                    if (tmp.Paragraphs.Count == 0)
                    {
                        foreach (string line in text.Replace(Environment.NewLine, "\n").Split('\n'))
                            tmp.Paragraphs.Add(new Paragraph(0, 0, line));
                    }

                    int index = FirstSelectedIndex;

                    if (!form.PasteOverwrite)
                    {
                        for (int i = 0; i < tmp.Paragraphs.Count; i++)
                        {
                            if (form.PasteAll)
                            {
                                for (int k = _subtitle.Paragraphs.Count - 2; k > index; k--)
                                {
                                    _subtitle.Paragraphs[k + 1] = new Paragraph(_subtitle.Paragraphs[k]);
                                }
                                if (index + i < _subtitle.Paragraphs.Count)
                                    _subtitle.Paragraphs[index + i].Text = string.Empty;
                            }
                            else if (form.PasteTimeCodesOnly)
                            {
                                for (int k = _subtitle.Paragraphs.Count - 2; k > index; k--)
                                {
                                    _subtitle.Paragraphs[k + 1].StartTime.TotalMilliseconds = _subtitle.Paragraphs[k].StartTime.TotalMilliseconds;
                                    _subtitle.Paragraphs[k + 1].EndTime.TotalMilliseconds = _subtitle.Paragraphs[k].EndTime.TotalMilliseconds;
                                    _subtitle.Paragraphs[k + 1].StartFrame = _subtitle.Paragraphs[k].StartFrame;
                                    _subtitle.Paragraphs[k + 1].EndFrame = _subtitle.Paragraphs[k].EndFrame;
                                }
                            }
                            else if (form.PasteTextOnly)
                            {
                                for (int k = _subtitle.Paragraphs.Count - 2; k > index; k--)
                                {
                                    _subtitle.Paragraphs[k + 1].Text = _subtitle.Paragraphs[k].Text;
                                }
                            }
                            else if (form.PasteOriginalTextOnly)
                            {
                                for (int k = _subtitle.Paragraphs.Count - 2; k > index; k--)
                                {

                                    Paragraph original = Utilities.GetOriginalParagraph(index, _subtitle.Paragraphs[k], _subtitleAlternate.Paragraphs);
                                    Paragraph originalNext = Utilities.GetOriginalParagraph(index, _subtitle.Paragraphs[k + 1], _subtitleAlternate.Paragraphs);
                                    if (original != null)
                                    {
                                        originalNext.Text = original.Text;
                                    }
                                }
                                if (index + i < _subtitle.Paragraphs.Count)
                                {
                                    Paragraph original = Utilities.GetOriginalParagraph(index, _subtitle.Paragraphs[index + i], _subtitleAlternate.Paragraphs);
                                    if (original != null)
                                        original.Text = string.Empty;
                                }
                            }
                        }
                    }
                    if (form.PasteOverwrite)
                    {
                        for (int i = 0; i + index < _subtitle.Paragraphs.Count && i < tmp.Paragraphs.Count; i++)
                            _subtitle.Paragraphs[index + i].Text = tmp.Paragraphs[i].Text;
                    }
                    else
                    {
                        for (int i = 0; i + index < _subtitle.Paragraphs.Count && i < tmp.Paragraphs.Count; i++)
                            _subtitle.Paragraphs[index + i + 1].Text = tmp.Paragraphs[i].Text;
                    }

                    SubtitleListview1.Fill(_subtitle, _subtitleAlternate);
                    SubtitleListview1.SelectIndexAndEnsureVisible(index, true);
                    RefreshSelectedParagraph();
                }
            }
        }

        private void deleteAndShiftCellsUpToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (SubtitleListview1.SelectedIndices.Count < 1)
                return;

            int first = FirstSelectedIndex;
            List<int> list = new List<int>();
            foreach (int index in SubtitleListview1.SelectedIndices)
                list.Add(index);
            list.Sort();
            list.Reverse();

            MakeHistoryForUndo(_language.BeforeColumnDelete);
            foreach (int index in list)
            {
                for (int k = index; k < _subtitle.Paragraphs.Count - 1; k++)
                {
                    _subtitle.Paragraphs[k].Text = _subtitle.Paragraphs[k + 1].Text;
                }
                _subtitle.Paragraphs[_subtitle.Paragraphs.Count - 1].Text = string.Empty;

            }
            SubtitleListview1.Fill(_subtitle, _subtitleAlternate);
            SubtitleListview1.SelectIndexAndEnsureVisible(first, true);
            RefreshSelectedParagraph();
        }

        private void toolStripMenuItemColumnImportText_Click(object sender, EventArgs e)
        {
            if (SubtitleListview1.SelectedIndices.Count < 1)
                return;

            var importText = new ImportText();
            if (importText.ShowDialog(this) == DialogResult.OK)
            {
                MakeHistoryForUndo(_language.BeforeColumnImportText);
                int index = FirstSelectedIndex;
                for (int i = 0; i < importText.FixedSubtitle.Paragraphs.Count; i++)
                {
                    for (int k = _subtitle.Paragraphs.Count - 2; k > index; k--)
                    {
                        _subtitle.Paragraphs[k + 1].Text = _subtitle.Paragraphs[k].Text;
                    }
                    if (index + i < _subtitle.Paragraphs.Count)
                        _subtitle.Paragraphs[index + i].Text = string.Empty;
                }

                for (int i = 0; i + index < _subtitle.Paragraphs.Count && i < importText.FixedSubtitle.Paragraphs.Count; i++)
                    _subtitle.Paragraphs[index + i].Text = importText.FixedSubtitle.Paragraphs[i].Text;

                SubtitleListview1.Fill(_subtitle, _subtitleAlternate);
                SubtitleListview1.SelectIndexAndEnsureVisible(index, true);
                RefreshSelectedParagraph();
            }
        }

        private void ShiftTextCellsDownToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (SubtitleListview1.SelectedIndices.Count < 1)
                return;

            int index = FirstSelectedIndex;
            int count = SubtitleListview1.SelectedIndices.Count;
            MakeHistoryForUndo(_language.BeforeColumnShiftCellsDown);
            for (int i = 0; i < count; i++)
            {
                for (int k = _subtitle.Paragraphs.Count - 2; k >= index; k--)
                {
                    _subtitle.Paragraphs[k + 1].Text = _subtitle.Paragraphs[k].Text;
                }
                if (index + i < _subtitle.Paragraphs.Count)
                    _subtitle.Paragraphs[index + i].Text = string.Empty;
            }

            SubtitleListview1.Fill(_subtitle, _subtitleAlternate);
            SubtitleListview1.SelectIndexAndEnsureVisible(index, true);
            RefreshSelectedParagraph();
        }

        private void toolStripMenuItemInsertTextFromSub_Click(object sender, EventArgs e)
        {
            openFileDialog1.Title = _languageGeneral.OpenSubtitle;
            openFileDialog1.FileName = string.Empty;
            openFileDialog1.Filter = Utilities.GetOpenDialogFilter();
            if (openFileDialog1.ShowDialog(this) == DialogResult.OK)
            {
                if (!File.Exists(openFileDialog1.FileName))
                    return;

                var fi = new FileInfo(openFileDialog1.FileName);
                if (fi.Length > 1024 * 1024 * 10) // max 10 mb
                {
                    if (MessageBox.Show(string.Format(_language.FileXIsLargerThan10MB + Environment.NewLine +
                                                      Environment.NewLine +
                                                      _language.ContinueAnyway,
                                                      openFileDialog1.FileName), Title, MessageBoxButtons.YesNoCancel) != DialogResult.Yes)
                        return;
                }

                Encoding encoding;
                var tmp = new Subtitle();
                SubtitleFormat format = tmp.LoadSubtitle(openFileDialog1.FileName, out encoding, null);

                if (format != null)
                {
                    if (format.IsFrameBased)
                        tmp.CalculateTimeCodesFromFrameNumbers(CurrentFrameRate);
                    else
                        tmp.CalculateFrameNumbersFromTimeCodes(CurrentFrameRate);

                    if (Configuration.Settings.General.RemoveBlankLinesWhenOpening)
                        tmp.RemoveEmptyLines();

                    if (SubtitleListview1.SelectedIndices.Count < 1)
                        return;

                    MakeHistoryForUndo(_language.BeforeColumnShiftCellsDown);

                    int index = FirstSelectedIndex;
                    for (int i = 0; i < tmp.Paragraphs.Count; i++)
                    {

                        {
                            for (int k = _subtitle.Paragraphs.Count - 2; k > index; k--)
                            {
                                _subtitle.Paragraphs[k + 1].Text = _subtitle.Paragraphs[k].Text;
                            }
                        }
                    }
                    for (int i = 0; i + index < _subtitle.Paragraphs.Count && i < tmp.Paragraphs.Count; i++)
                        _subtitle.Paragraphs[index + i].Text = tmp.Paragraphs[i].Text;
                    if (IsFramesRelevant && CurrentFrameRate > 0)
                        _subtitle.CalculateFrameNumbersFromTimeCodesNoCheck(CurrentFrameRate);
                    SubtitleListview1.Fill(_subtitle, _subtitleAlternate);
                    SubtitleListview1.SelectIndexAndEnsureVisible(index, true);
                    RefreshSelectedParagraph();
                }
            }

        }

        private void toolStripMenuItemOpenKeepVideo_Click(object sender, EventArgs e)
        {
            openToolStripMenuItem.Enabled = false;
            ReloadFromSourceView();
            _resetVideo = false;
            OpenNewFile();
            _resetVideo = true;
            openToolStripMenuItem.Enabled = true;
        }

        private void changeSpeedInPercentToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (IsSubtitleLoaded)
            {
                int lastSelectedIndex = 0;
                if (SubtitleListview1.SelectedItems.Count > 0)
                    lastSelectedIndex = SubtitleListview1.SelectedItems[0].Index;

                ReloadFromSourceView();
                var form = new ChangeSpeedInPercent(SubtitleListview1.SelectedItems.Count);
                _formPositionsAndSizes.SetPositionAndSize(form);
                if (form.ShowDialog(this) == DialogResult.OK)
                {
                    MakeHistoryForUndo(_language.BeforeAdjustSpeedInPercent);
                    SaveSubtitleListviewIndexes();
                    if (form.AdjustAllLines)
                    {
                        _subtitle = form.AdjustAllParagraphs(_subtitle);
                        if (_subtitleAlternate != null && Configuration.Settings.General.AllowEditOfOriginalSubtitle && SubtitleListview1.IsAlternateTextColumnVisible)
                            _subtitleAlternate = form.AdjustAllParagraphs(_subtitleAlternate);
                    }
                    else
                    {
                        foreach (int index in SubtitleListview1.SelectedIndices)
                        {
                            Paragraph p = _subtitle.GetParagraphOrDefault(index);
                            if (p != null)
                            {
                                form.AdjustParagraph(p);
                                if (_subtitleAlternate != null && Configuration.Settings.General.AllowEditOfOriginalSubtitle && SubtitleListview1.IsAlternateTextColumnVisible)
                                {
                                    Paragraph original = Utilities.GetOriginalParagraph(index, p, _subtitle.Paragraphs);
                                    if (original != null)
                                        form.AdjustParagraph(original);
                                }
                            }
                        }
                    }
                    if (IsFramesRelevant && CurrentFrameRate > 0)
                        _subtitle.CalculateFrameNumbersFromTimeCodesNoCheck(CurrentFrameRate);
                    ShowSource();
                    SubtitleListview1.Fill(_subtitle, _subtitleAlternate);
                    RestoreSubtitleListviewIndexes();
                }
                _formPositionsAndSizes.SavePositionAndSize(form);
            }
            else
            {
                MessageBox.Show(_language.NoSubtitleLoaded, Title, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void toolStripMenuItemAvidStl_Click(object sender, EventArgs e)
        {
            var avidStl = new AvidStl();
            saveFileDialog1.Filter = avidStl.Name + "|*" + avidStl.Extension;
            saveFileDialog1.Title = _language.SaveSubtitleAs;
            saveFileDialog1.DefaultExt = "*" + avidStl.Extension;
            saveFileDialog1.AddExtension = true;

            if (!string.IsNullOrEmpty(_videoFileName))
                saveFileDialog1.FileName = Path.GetFileNameWithoutExtension(_videoFileName);
            else
                saveFileDialog1.FileName = Path.GetFileNameWithoutExtension(_fileName);

            if (!string.IsNullOrEmpty(openFileDialog1.InitialDirectory))
                saveFileDialog1.InitialDirectory = openFileDialog1.InitialDirectory;

            DialogResult result = saveFileDialog1.ShowDialog(this);
            if (result == DialogResult.OK)
            {
                openFileDialog1.InitialDirectory = saveFileDialog1.InitialDirectory;
                string fileName = saveFileDialog1.FileName;
                string ext = Path.GetExtension(fileName);
                bool extOk = ext.Equals(avidStl.Extension, StringComparison.OrdinalIgnoreCase);
                if (!extOk)
                {
                    if (fileName.EndsWith('.'))
                        fileName = fileName.Substring(0, fileName.Length - 1);
                    fileName += avidStl.Extension;
                }
                AvidStl.Save(fileName, _subtitle);
            }
        }

        private void columnDeleteTextOnlyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (SubtitleListview1.SelectedIndices.Count < 1)
                return;

            foreach (int index in SubtitleListview1.SelectedIndices)
            {
                _subtitle.Paragraphs[index].Text = string.Empty;
                SubtitleListview1.SetText(index, string.Empty);
                SubtitleListview1.SyntaxColorLine(_subtitle.Paragraphs, index, _subtitle.Paragraphs[index]);
            }
            RefreshSelectedParagraph();
        }

        private void toolStripMenuItemBatchConvert_Click(object sender, EventArgs e)
        {
            this.Visible = false;
            var form = new BatchConvert(this.Icon);
            _formPositionsAndSizes.SetPositionAndSize(form);
            form.ShowDialog(this);
            _formPositionsAndSizes.SavePositionAndSize(form);
            this.Visible = true;
        }

        private void copyOriginalTextToCurrentToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_subtitleAlternate == null || !SubtitleListview1.IsAlternateTextColumnVisible || SubtitleListview1.SelectedIndices.Count < 1)
                return;

            bool first = true;
            foreach (int index in SubtitleListview1.SelectedIndices)
            {
                Paragraph original = Utilities.GetOriginalParagraph(index, _subtitle.Paragraphs[index], _subtitleAlternate.Paragraphs);
                if (original != null)
                {
                    if (first)
                    {
                        MakeHistoryForUndo(_language.BeforeColumnPaste);
                    }
                    SubtitleListview1.SetText(index, original.Text);
                    _subtitle.Paragraphs[index].Text = original.Text;
                    SubtitleListview1.SyntaxColorLine(_subtitle.Paragraphs, index, _subtitle.Paragraphs[index]);
                    first = false;
                }
            }
            RefreshSelectedParagraph();
        }

        private void toolStripMenuItemColumn_DropDownOpening(object sender, EventArgs e)
        {
            copyOriginalTextToCurrentToolStripMenuItem.Visible = !string.IsNullOrEmpty(copyOriginalTextToCurrentToolStripMenuItem.Text) &&
                                                                 SubtitleListview1.IsAlternateTextColumnVisible &&
                                                                 _subtitleAlternate != null;
        }

        private void toolStripMenuItemMergeDuplicateText_Click(object sender, EventArgs e)
        {
            if (IsSubtitleLoaded)
            {
                ReloadFromSourceView();
                var form = new MergeDoubleLines();
                _formPositionsAndSizes.SetPositionAndSize(form);
                form.Initialize(_subtitle);
                if (form.ShowDialog(this) == DialogResult.OK)
                {
                    if (!string.IsNullOrEmpty(_language.BeforeMergeLinesWithSameText)) //TODO: Remove in Subtitle Edit 3.3.4
                        MakeHistoryForUndo(_language.BeforeMergeLinesWithSameText);
                    else
                        MakeHistoryForUndo(_language.BeforeMergeShortLines);
                    _subtitle.Paragraphs.Clear();
                    foreach (Paragraph p in form.MergedSubtitle.Paragraphs)
                        _subtitle.Paragraphs.Add(p);
                    ShowStatus(string.Format(_language.MergedShortLinesX, form.NumberOfMerges));
                    SaveSubtitleListviewIndexes();
                    ShowSource();
                    SubtitleListview1.Fill(_subtitle, _subtitleAlternate);
                    RestoreSubtitleListviewIndexes();
                }
                _formPositionsAndSizes.SavePositionAndSize(form);
            }
            else
            {
                MessageBox.Show(_language.NoSubtitleLoaded, Title, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void toolStripMenuItemMergeLinesWithSameTimeCodes_Click(object sender, EventArgs e)
        {
            if (IsSubtitleLoaded)
            {
                ReloadFromSourceView();
                var form = new MergeTextWithSameTimeCodes();
                _formPositionsAndSizes.SetPositionAndSize(form);
                form.Initialize(_subtitle);
                if (form.ShowDialog(this) == DialogResult.OK)
                {
                    if (!string.IsNullOrEmpty(_language.BeforeMergeLinesWithSameText)) //TODO: Remove in SE 3.3.4
                        MakeHistoryForUndo(_language.BeforeMergeLinesWithSameText);
                    else
                        MakeHistoryForUndo(_language.BeforeMergeShortLines);
                    _subtitle.Paragraphs.Clear();
                    foreach (Paragraph p in form.MergedSubtitle.Paragraphs)
                        _subtitle.Paragraphs.Add(p);
                    ShowStatus(string.Format(_language.MergedShortLinesX, form.NumberOfMerges));
                    SaveSubtitleListviewIndexes();
                    ShowSource();
                    SubtitleListview1.Fill(_subtitle, _subtitleAlternate);
                    RestoreSubtitleListviewIndexes();
                }
                _formPositionsAndSizes.SavePositionAndSize(form);
            }
            else
            {
                MessageBox.Show(_language.NoSubtitleLoaded, Title, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void toolStripMenuItem2_Click(object sender, EventArgs e)
        {
            var exportBdnXmlPng = new ExportPngXml();
            exportBdnXmlPng.Initialize(_subtitle, GetCurrentSubtitleFormat(), "SPUMUX", _fileName, _videoInfo);
            _formPositionsAndSizes.SetPositionAndSize(exportBdnXmlPng);
            exportBdnXmlPng.ShowDialog(this);
            _formPositionsAndSizes.SavePositionAndSize(exportBdnXmlPng);
        }

        private void toolStripMenuItemModifySelection_Click(object sender, EventArgs e)
        {
            if (!IsSubtitleLoaded)
            {
                MessageBox.Show(_language.NoSubtitleLoaded, Title, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            var form = new ModifySelection(_subtitle, SubtitleListview1);
            _formPositionsAndSizes.SetPositionAndSize(form);
            form.ShowDialog(this);
            _formPositionsAndSizes.SavePositionAndSize(form);
        }

        private void toolStripMenuItemInverseSelection_Click(object sender, EventArgs e)
        {
            if (!IsSubtitleLoaded)
            {
                MessageBox.Show(_language.NoSubtitleLoaded, Title, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            foreach (ListViewItem item in SubtitleListview1.Items)
                item.Selected = !item.Selected;
        }

        private void toolStripMenuItemSpellCheckFromCurrentLine_Click(object sender, EventArgs e)
        {
            _spellCheckForm = null;
            SpellCheck(true, FirstSelectedIndex);
        }

        private void toolStripMenuItemImportXSub_Click(object sender, EventArgs e)
        {
            if (ContinueNewOrExit())
            {
                openFileDialog1.Title = _language.OpenXSubFiles;
                openFileDialog1.FileName = string.Empty;
                openFileDialog1.Filter = _language.XSubFiles + "|*.divx;*.avi";
                if (openFileDialog1.ShowDialog(this) == DialogResult.OK)
                {
                    ShowStatus(Configuration.Settings.Language.General.PleaseWait);
                    if (ImportSubtitleFromDivX(openFileDialog1.FileName))
                    {
                        ShowStatus(string.Format(_language.LoadedSubtitleX, openFileDialog1.FileName));
                    }
                    else
                    {
                        ShowStatus(string.Empty);
                        if (!string.IsNullOrEmpty(_language.NotAValidXSubFile))
                            MessageBox.Show(_language.NotAValidXSubFile);
                        else
                            MessageBox.Show("Not a valid XSub file!");
                    }
                }
            }
        }

        private void toolStripMenuItemImportOcrHardSub_Click(object sender, EventArgs e)
        {
            var form = new HardSubExtract(_videoFileName);
            if (form.ShowDialog(this) == DialogResult.OK)
            {

                if (!string.IsNullOrEmpty(form.OcrFileName))
                {
                    MakeHistoryForUndo(_language.BeforeAutoBalanceSelectedLines); //TODO: Fix text
                    OpenSubtitle(form.OcrFileName, null);
                }
            }
        }

        private void toolStripMenuItemExportFcpIImage_Click(object sender, EventArgs e)
        {
            var exportBdnXmlPng = new ExportPngXml();
            exportBdnXmlPng.Initialize(_subtitle, GetCurrentSubtitleFormat(), "FCP", _fileName, _videoInfo);
            exportBdnXmlPng.ShowDialog(this);
        }

        private void ToolStripMenuItemNuendoPropertiesClick(object sender, EventArgs e)
        {
            var form = new NuendoProperties();
            _formPositionsAndSizes.SetPositionAndSize(form);
            if (form.ShowDialog(this) == DialogResult.OK)
            {
                Configuration.Settings.SubtitleSettings.NuendoCharacterListFile = form.CharacterListFile;
            }
            _formPositionsAndSizes.SavePositionAndSize(form);
        }

        private void toolStripMenuItemDost_Click(object sender, EventArgs e)
        {
            var exportBdnXmlPng = new ExportPngXml();
            exportBdnXmlPng.Initialize(_subtitle, GetCurrentSubtitleFormat(), "DOST", _fileName, _videoInfo);
            exportBdnXmlPng.ShowDialog(this);
        }

        private void toolStripMenuItemMeasurementConverter_Click(object sender, EventArgs e)
        {
            var form = new MeasurementConverter();
            _formPositionsAndSizes.SetPositionAndSize(form);
            form.Show(this);
            //            _formPositionsAndSizes.SavePositionAndSize(form);
        }

        private void toolStripMenuItemImportSceneChanges_Click(object sender, EventArgs e)
        {
            var form = new ImportSceneChanges(_videoInfo);
            _formPositionsAndSizes.SetPositionAndSize(form);
            if (form.ShowDialog(this) == DialogResult.OK)
            {
                audioVisualizer.SceneChanges = form.SceneChangesInSeconds;
                ShowStatus(string.Format(Configuration.Settings.Language.Main.XSceneChangesImported, form.SceneChangesInSeconds.Count));
            }
            _formPositionsAndSizes.SavePositionAndSize(form);
        }

        private void toolStripMenuItemRemoveSceneChanges_Click(object sender, EventArgs e)
        {
            if (audioVisualizer != null && audioVisualizer.SceneChanges != null)
                audioVisualizer.SceneChanges = new List<double>();
        }

        private void toolStripMenuItemDurationBridgeGaps_Click(object sender, EventArgs e)
        {
            if (!IsSubtitleLoaded)
            {
                MessageBox.Show(_language.NoSubtitleLoaded, Title, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            var form = new DurationsBridgeGaps(_subtitle);
            _formPositionsAndSizes.SetPositionAndSize(form);
            if (form.ShowDialog(this) == DialogResult.OK)
            {
                int index = FirstSelectedIndex;
                if (index < 0)
                    index = 0;
                MakeHistoryForUndo("Before bridge small gaps"); //TODO: Fix text in SE 3.4
                _subtitle.Paragraphs.Clear();
                foreach (Paragraph p in form.FixedSubtitle.Paragraphs)
                    _subtitle.Paragraphs.Add(p);

                SubtitleListview1.Fill(_subtitle, _subtitleAlternate);
                SubtitleListview1.SelectIndexAndEnsureVisible(index);
            }
            _formPositionsAndSizes.SavePositionAndSize(form);
        }

        private void toolStripMenuItemOpenDvd_Click(object sender, EventArgs e)
        {
            var form = new OpenVideoDvd();
            _formPositionsAndSizes.SetPositionAndSize(form);
            if (form.ShowDialog(this) == DialogResult.OK && !string.IsNullOrEmpty(form.DvdPath))
            {
                VideoFileName = form.DvdPath;
                if (mediaPlayer.VideoPlayer != null)
                {
                    mediaPlayer.Pause();
                    mediaPlayer.VideoPlayer.DisposeVideoPlayer();
                }
                _endSeconds = -1;

                _videoInfo = new VideoInfo();
                _videoInfo.Width = 720;
                _videoInfo.Height = 576;
                _videoInfo.FramesPerSecond = 25;
                _videoInfo.VideoCodec = "MPEG2";
                toolStripComboBoxFrameRate.Text = string.Format("{0:0.###}", _videoInfo.FramesPerSecond);

                string oldVideoPlayer = Configuration.Settings.General.VideoPlayer;
                Configuration.Settings.General.VideoPlayer = "VLC";
                Utilities.InitializeVideoPlayerAndContainer(VideoFileName, _videoInfo, mediaPlayer, VideoLoaded, VideoEnded);
                mediaPlayer.ShowFullscreenButton = Configuration.Settings.General.VideoPlayerShowFullscreenButton;
                mediaPlayer.OnButtonClicked -= MediaPlayer_OnButtonClicked;
                mediaPlayer.OnButtonClicked += MediaPlayer_OnButtonClicked;
                mediaPlayer.Volume = 0;
                labelVideoInfo.Text = "DVD" + " " + _videoInfo.Width + "x" + _videoInfo.Height + " " + _videoInfo.VideoCodec.Trim();
                if (_videoInfo.FramesPerSecond > 0)
                    labelVideoInfo.Text = labelVideoInfo.Text + " " + string.Format("{0:0.0##}", _videoInfo.FramesPerSecond);
                Configuration.Settings.General.VideoPlayer = oldVideoPlayer;
            }
            _formPositionsAndSizes.SavePositionAndSize(form);
        }

        private void toolStripMenuItemFcpProperties_Click(object sender, EventArgs e)
        {
            var form = new FcpProperties();
            _formPositionsAndSizes.SetPositionAndSize(form);
            if (form.ShowDialog(this) == DialogResult.OK)
            {
                Configuration.Settings.SubtitleSettings.FcpFontSize = form.FcpFontSize;
                Configuration.Settings.SubtitleSettings.FcpFontName = form.FcpFontName;
            }
            _formPositionsAndSizes.SavePositionAndSize(form);
        }

        private void styleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SortSubtitle(SubtitleSortCriteria.Style, (sender as ToolStripItem).Text);
        }

        private void toolStripMenuItemFocusTextbox_Click(object sender, EventArgs e)
        {
            int index = _subtitle.GetIndex(audioVisualizer.RightClickedParagraph);
            if (index >= 0)
                SubtitleListview1.SelectIndexAndEnsureVisible(index);
            textBoxListViewText.Focus();
            textBoxListViewText.SelectAll();
        }

        private void AscendingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            descendingToolStripMenuItem.Checked = false;
            AscendingToolStripMenuItem.Checked = true;
            toolsToolStripMenuItem.ShowDropDown();
            toolStripMenuItem1.ShowDropDown();
        }

        private void descendingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AscendingToolStripMenuItem.Checked = false;
            descendingToolStripMenuItem.Checked = true;
            toolsToolStripMenuItem.ShowDropDown();
            toolStripMenuItem1.ShowDropDown();
        }

        private void exportCustomTextFormatToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var form = new ExportCustomText(_subtitle, _subtitleAlternate, _fileName);
            _formPositionsAndSizes.SetPositionAndSize(form);
            if (form.ShowDialog(this) == DialogResult.OK)
            {
                ShowStatus(form.LogMessage);
            }
            _formPositionsAndSizes.SavePositionAndSize(form);
        }

        private void PasteIntoActiveTextBox(string s)
        {
            if (tabControlSubtitle.SelectedIndex == TabControlSourceView)
            {
                textBoxSource.Text = textBoxSource.Text.Insert(textBoxSource.SelectionStart, s);
            }
            else
            {
                if (textBoxListViewTextAlternate.Visible && textBoxListViewTextAlternate.Enabled && textBoxListViewTextAlternate.Focused)
                    textBoxListViewTextAlternate.Text = textBoxListViewTextAlternate.Text.Insert(textBoxListViewTextAlternate.SelectionStart, s);
                else
                    textBoxListViewText.Text = textBoxListViewText.Text.Insert(textBoxListViewText.SelectionStart, s);
            }
        }

        private void leftToolStripMenuItem_Click(object sender, EventArgs e)
        {
            PasteIntoActiveTextBox(Convert.ToChar(8207).ToString());
        }

        private void righttoleftMarkToolStripMenuItem_Click(object sender, EventArgs e)
        {
            PasteIntoActiveTextBox(Convert.ToChar(8206).ToString());
        }

        private void startOfLefttorightEmbeddingLREToolStripMenuItem_Click(object sender, EventArgs e)
        {
            PasteIntoActiveTextBox(Convert.ToChar(0x202A).ToString());
        }

        private void startOfRighttoleftEmbeddingRLEToolStripMenuItem_Click(object sender, EventArgs e)
        {
            PasteIntoActiveTextBox(Convert.ToChar(0x202B).ToString());
        }

        private void startOfLefttorightOverrideLROToolStripMenuItem_Click(object sender, EventArgs e)
        {
            PasteIntoActiveTextBox(Convert.ToChar(0x202D).ToString());
        }

        private void startOfRighttoleftOverrideRLOToolStripMenuItem_Click(object sender, EventArgs e)
        {
            PasteIntoActiveTextBox(Convert.ToChar(0x202E).ToString());
        }

        private void toolStripMenuItemRtlUnicodeControlChar_Click(object sender, EventArgs e)
        {
            string rtl = Convert.ToChar(0x202B).ToString();
            int selectedIndex = FirstSelectedIndex;
            foreach (int index in SubtitleListview1.SelectedIndices)
            {
                Paragraph p = _subtitle.Paragraphs[index];
                string text = p.Text.Replace(rtl, string.Empty);
                p.Text = rtl + text.Replace(Environment.NewLine, Environment.NewLine + rtl);
                SubtitleListview1.SetText(index, p.Text);
                if (index == selectedIndex)
                    textBoxListViewText.Text = p.Text;
            }
            RefreshSelectedParagraph();
        }

        private void toolStripMenuItemImportImages_Click(object sender, EventArgs e)
        {
            if (!ContinueNewOrExit())
                return;

            if (!string.IsNullOrEmpty(_videoFileName) && mediaPlayer.VideoPlayer != null)
                mediaPlayer.Pause();

            var form = new ImportImages();
            _formPositionsAndSizes.SetPositionAndSize(form);
            if (form.ShowDialog(this) == DialogResult.OK && form.Subtitle.Paragraphs.Count > 0)
            {
                ImportAndOcrSrt(form.Subtitle);
            }
            _formPositionsAndSizes.SavePositionAndSize(form);
        }

        private void audioVisualizer_MouseEnter(object sender, EventArgs e)
        {
            if (!textBoxListViewText.Focused && !textBoxListViewTextAlternate.Focused && Configuration.Settings.VideoControls.WaveformFocusOnMouseEnter && audioVisualizer.WavePeaks != null && !audioVisualizer.Focused && audioVisualizer.CanFocus)
                audioVisualizer.Focus();
        }

        private void SubtitleListview1_MouseEnter(object sender, EventArgs e)
        {

            if (!textBoxListViewText.Focused && !textBoxListViewTextAlternate.Focused && Configuration.Settings.VideoControls.WaveformFocusOnMouseEnter &&
                Configuration.Settings.VideoControls.WaveformListViewFocusOnMouseEnter && !SubtitleListview1.Focused && SubtitleListview1.CanFocus)
                SubtitleListview1.Focus();
        }

        private void toolStripButtonFixCommonErrors_Click(object sender, EventArgs e)
        {
            FixCommonErrors(false);
        }

        private void toolStripMenuItemExportDcinemaInteropClick(object sender, EventArgs e)
        {
            var exportBdnXmlPng = new ExportPngXml();
            exportBdnXmlPng.Initialize(_subtitle, GetCurrentSubtitleFormat(), "DCINEMA_INTEROP", _fileName, _videoInfo);
            exportBdnXmlPng.ShowDialog(this);
        }

        internal Subtitle UndoFromSpellCheck(Subtitle subtitle)
        {
            var idx = FirstSelectedIndex;
            for (int i = 0; i < _subtitle.Paragraphs.Count; i++)
            {
                if (_subtitle.Paragraphs[i].Text != subtitle.Paragraphs[i].Text)
                {
                    _subtitle.Paragraphs[i].Text = subtitle.Paragraphs[i].Text;
                    SubtitleListview1.SetText(i, _subtitle.Paragraphs[i].Text);
                }
                if (idx == i)
                {
                    SubtitleListview1.SetText(idx, _subtitle.Paragraphs[idx].Text);
                }
            }
            RefreshSelectedParagraph();
            return _subtitle;
        }

        private void checkForUpdatesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (_timerCheckForUpdates != null)
                    _timerCheckForUpdates.Stop();
            }
            catch
            {
            }

            var form = new CheckForUpdates(this);
            form.ShowDialog(this);
            Configuration.Settings.General.LastCheckForUpdates = DateTime.Now;
        }

        private void setVideoOffsetToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(_videoFileName) && mediaPlayer.VideoPlayer != null)
                mediaPlayer.Pause();

            var form = new SetVideoOffset();
            form.VideoOffset = new TimeCode(10, 0, 0, 0);
            if (mediaPlayer.Offset > 0.001)
                form.VideoOffset = TimeCode.FromSeconds(mediaPlayer.Offset);
            _formPositionsAndSizes.SetPositionAndSize(form);
            if (form.ShowDialog(this) == DialogResult.OK)
            {
                var offsetInSeconds = form.VideoOffset.TotalSeconds;
                if (form.FromCurrentVideoPosition)
                    offsetInSeconds -= mediaPlayer.VideoPlayer.CurrentPosition;
                mediaPlayer.Offset = offsetInSeconds;
                if (audioVisualizer != null)
                    audioVisualizer.Offset = offsetInSeconds;
            }
            _formPositionsAndSizes.SavePositionAndSize(form);
        }

        private void toolStripMenuItemEbuProperties_Click(object sender, EventArgs e)
        {
            var properties = new EbuSaveOptions();

            var header = new Ebu.EbuGeneralSubtitleInformation();
            if (_subtitle != null && _subtitle.Header != null && (_subtitle.Header.Contains("STL2") || _subtitle.Header.Contains("STL3")))
            {
                header = Ebu.ReadHeader(Encoding.UTF8.GetBytes(_subtitle.Header));
                properties.Initialize(header, 0, null, _subtitle);
            }
            else
            {
                if (!string.IsNullOrEmpty(_fileName) && new Ebu().IsMine(null, _fileName))
                    properties.Initialize(header, 0, _fileName, _subtitle);
                else
                    properties.Initialize(header, 0, null, _subtitle);
            }
            _formPositionsAndSizes.SetPositionAndSize(properties, true);
            properties.ShowDialog(this);
            _formPositionsAndSizes.SavePositionAndSize(properties);
        }

    }
}
