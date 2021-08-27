using Nikse.SubtitleEdit.Controls;
using Nikse.SubtitleEdit.Core.BluRaySup;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.ContainerFormats;
using Nikse.SubtitleEdit.Core.ContainerFormats.TransportStream;
using Nikse.SubtitleEdit.Core.Interfaces;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Core.VobSub;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Ocr;
using Nikse.SubtitleEdit.Logic.Ocr.Binary;
using Nikse.SubtitleEdit.Logic.Ocr.Tesseract;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace Nikse.SubtitleEdit.Forms.Ocr
{
    using LogItem = OcrFixEngine.LogItem;

    public sealed partial class VobSubOcr : PositionAndSizeForm, IBinaryParagraphList
    {
        private static readonly Color _listViewGreen = Configuration.Settings.General.UseDarkTheme ? Color.Green : Color.LightGreen;
        private static readonly Color _listViewYellow = Configuration.Settings.General.UseDarkTheme ? Color.FromArgb(218, 135, 32) : Color.Yellow;
        private static readonly Color _listViewOrange = Configuration.Settings.General.UseDarkTheme ? Color.OrangeRed : Color.Orange;

        internal class CompareItem
        {
            public ManagedBitmap Bitmap { get; }
            public string Name { get; }
            public bool Italic { get; set; }
            public int ExpandCount { get; }
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
            public SubPicture Picture { get; }
            public TimeSpan Start { get; }
            public TimeSpan End { get; }

            public SubPicturesWithSeparateTimeCodes(SubPicture subPicture, TimeSpan start, TimeSpan end)
            {
                Picture = subPicture;
                Start = start;
                End = end;
            }
        }

        internal class NOcrThreadParameter
        {
            public int Index { get; set; }
            public int Increment { get; set; }
            public string ResultText { get; set; }
            public List<CompareMatch> ResultMatches { get; set; }
            public NOcrDb NOcrDb { get; set; }
            public BackgroundWorker Self { get; set; }
            public double UnItalicFactor { get; set; }
            public bool AdvancedItalicDetection { get; set; }
            public int NOcrLastLowercaseHeight;
            public int NOcrLastUppercaseHeight;
            public int NumberOfPixelsIsSpace;
            public bool RightToLeft;

            public NOcrThreadParameter(int index, NOcrDb nOcrDb, BackgroundWorker self, int increment, double unItalicFactor, bool advancedItalicDetection, int numberOfPixelsIsSpace, bool rightToLeft)
            {
                Self = self;
                Index = index;
                NOcrDb = nOcrDb;
                Increment = increment;
                UnItalicFactor = unItalicFactor;
                AdvancedItalicDetection = advancedItalicDetection;
                NOcrLastLowercaseHeight = -1;
                NOcrLastUppercaseHeight = -1;
                NumberOfPixelsIsSpace = numberOfPixelsIsSpace;
                RightToLeft = rightToLeft;
            }
        }

        internal class NOcrThreadResult
        {
            public string ResultText { get; set; }
            public List<CompareMatch> ResultMatches { get; set; }

            public NOcrThreadResult(NOcrThreadParameter p)
            {
                ResultMatches = new List<CompareMatch>();
                ResultMatches.AddRange(p.ResultMatches);
                ResultText = p.ResultText;
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

            public ImageCompareThreadParameter(Bitmap picture, int index, List<CompareItem> compareBitmaps, BackgroundWorker self, int increment, int numberOfPixelsIsSpace, bool rightToLeft, float maxErrorPercent)
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
            }
        }

        public class CompareMatch
        {
            public string Text { get; set; }
            public bool Italic { get; set; }
            public int ExpandCount { get; set; }
            public string Name { get; set; }
            public NOcrChar NOcrCharacter { get; set; }
            public ImageSplitterItem ImageSplitterItem { get; set; }
            public int X { get; set; }
            public int Y { get; set; }
            public List<ImageSplitterItem> Extra { get; set; }

            public CompareMatch(string text, bool italic, int expandCount, string name)
            {
                Text = text;
                Italic = italic;
                ExpandCount = expandCount;
                Name = name;
            }

            public CompareMatch(string text, bool italic, int expandCount, string name, NOcrChar character)
                : this(text, italic, expandCount, name)
            {
                NOcrCharacter = character;
            }

            public CompareMatch(string text, bool italic, int expandCount, string name, ImageSplitterItem imageSplitterItem)
                : this(text, italic, expandCount, name)
            {
                ImageSplitterItem = imageSplitterItem;
            }

            public override string ToString()
            {
                if (Italic)
                {
                    return Text + " (italic)";
                }

                if (Text == null)
                {
                    return string.Empty;
                }

                return Text;
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

            public override string ToString()
            {
                if (Image == null)
                {
                    return Text;
                }

                if (Italic)
                {
                    return Text + " (" + Image.Width + "x" + Image.Height + ", italic)";
                }

                return Text + " (" + Image.Width + "x" + Image.Height + ")";
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

        private class OcrFix : LogItem
        {
            public OcrFix(int index, string oldLine, string newLine)
                : base(index + 1, $"{oldLine.Replace(Environment.NewLine, " ")} ⇒ {newLine.Replace(Environment.NewLine, " ")}")
            {
            }
        }

        public delegate void ProgressCallbackDelegate(string progress);
        public ProgressCallbackDelegate ProgressCallback { get; set; }

        private Main _main;
        public string FileName { get; set; }
        private Subtitle _subtitle = new Subtitle();
        private List<CompareItem> _compareBitmaps;
        private XmlDocument _compareDoc = new XmlDocument();
        private Point _manualOcrDialogPosition = new Point(-1, -1);
        private volatile bool _abort;
        private int _selectedIndex = -1;
        private VobSubOcrSettings _vobSubOcrSettings;
        private bool _italicCheckedLast;
        private double _unItalicFactor = 0.33;

        private BinaryOcrDb _binaryOcrDb;

        private long _ocrLowercaseHeightsTotal;
        private int _ocrLowercaseHeightsTotalCount;
        private long _ocrUppercaseHeightsTotal;
        private int _ocrUppercaseHeightsTotalCount;
        private long _ocrLetterHeightsTotal;
        private int _ocrLetterHeightsTotalCount;
        private int _ocrMinLineHeight = -1;

        private bool _captureTopAlign;
        private int _captureTopAlignHeight = -1;
        private int _captureTopAlignHeightThird = -1;

        private Timer _mainOcrTimer;
        private int _mainOcrTimerMax;
        private int _mainOcrIndex;
        private bool _mainOcrRunning;
        private Bitmap _mainOcrBitmap;

        private Type _modiType;
        private object _modiDoc;
        private bool _modiEnabled;

        private bool _fromMenuItem;

        // DVD rip/vobsub
        private List<VobSubMergedPack> _vobSubMergedPackListOriginal;
        private List<VobSubMergedPack> _vobSubMergedPackList;
        private List<Color> _palette;

        // Blu-ray sup
        private List<BluRaySupParser.PcsData> _bluRaySubtitlesOriginal;
        private List<BluRaySupParser.PcsData> _bluRaySubtitles;

        // SP list
        private List<SpHeader> _spList;

        // SP vobsub list (mp4)
        private List<SubPicturesWithSeparateTimeCodes> _mp4List;

        // XSub (divx)
        private List<XSub> _xSubList;

        // DVB (from transport stream)
        private List<TransportStreamSubtitle> _dvbSubtitles;
        private List<Color> _dvbSubColor;
        private bool _transportStreamUseColor;

        // DVB (from transport stream inside mkv)
        private List<DvbSubPes> _dvbPesSubtitles;

        private string _lastLine;
        private string _languageId;
        private string _importLanguageString;

        // Dictionaries/spellchecking/fixing
        private OcrFixEngine _ocrFixEngine;
        private int _tesseractOcrAutoFixes;
        private string Tesseract5Version = "5.00 Alpha 2021-08-11";

        private Subtitle _bdnXmlOriginal;
        private Subtitle _bdnXmlSubtitle;
        private XmlDocument _bdnXmlDocument;
        private string _bdnFileName;
        private bool _isSon;

        private List<ImageCompareAddition> _lastAdditions = new List<ImageCompareAddition>();
        private readonly VobSubOcrCharacter _vobSubOcrCharacter = new VobSubOcrCharacter();

        private NOcrDb _nOcrDb;
        private readonly VobSubOcrNOcrCharacter _vobSubOcrNOcrCharacter = new VobSubOcrNOcrCharacter();
        public const int NOcrMinColor = 300;
        private NOcrDb _nOcrDbThread;
        private NOcrThreadResult[] _nOcrThreadResults;
        private bool _ocrThreadStop;

        private readonly Keys _italicShortcut = UiUtil.GetKeys(Configuration.Settings.Shortcuts.MainListViewItalic);
        private readonly Keys _mainGeneralGoToNextSubtitle = UiUtil.GetKeys(Configuration.Settings.Shortcuts.GeneralGoToNextSubtitle);
        private readonly Keys _mainGeneralGoToPrevSubtitle = UiUtil.GetKeys(Configuration.Settings.Shortcuts.GeneralGoToPrevSubtitle);

        private string[] _tesseractAsyncStrings;
        private int _tesseractAsyncIndex;
        private int _tesseractEngineMode = 3;

        private bool _okClicked;
        private readonly Dictionary<string, int> _unknownWordsDictionary;

        // optimization vars
        private int _numericUpDownPixelsIsSpace = 12;
        private bool _autoLineHeight = true;
        private double _numericUpDownMaxErrorPct = 6;
        private int _ocrMethodIndex;
        private bool _autoBreakLines;
        private bool _hasForcedSubtitles;

        private readonly int _ocrMethodBinaryImageCompare = -1;
        private readonly int _ocrMethodTesseract302 = -1;
        private readonly int _ocrMethodTesseract4 = -1;
        private readonly int _ocrMethodModi = -1;
        private readonly int _ocrMethodNocr = -1;

        private FindReplaceDialogHelper _findHelper;

        public static void SetDoubleBuffered(Control c)
        {
            //Taxes: Remote Desktop Connection and painting http://blogs.msdn.com/oldnewthing/archive/2006/01/03/508694.aspx
            if (SystemInformation.TerminalServerSession)
            {
                return;
            }

            PropertyInfo aProp = typeof(Control).GetProperty("DoubleBuffered", BindingFlags.NonPublic | BindingFlags.Instance);
            aProp?.SetValue(c, true, null);
        }

        public VobSubOcr()
        {
            UiUtil.PreInitialize(this);
            InitializeComponent();
            UiUtil.FixFonts(this);
            SetDoubleBuffered(subtitleListView1);

            _unknownWordsDictionary = new Dictionary<string, int>();
            var language = LanguageSettings.Current.VobSubOcr;
            Text = language.Title;
            groupBoxOcrMethod.Text = language.OcrMethod;
            labelTesseractLanguage.Text = language.Language;
            labelTesseractEngineMode.Text = language.TesseractEngineMode;
            labelImageDatabase.Text = language.ImageDatabase;
            labelNoOfPixelsIsSpace.Text = language.NoOfPixelsIsSpace;
            labelMaxErrorPercent.Text = language.MaxErrorPercent;
            buttonStartOcr.Text = language.StartOcr;
            buttonStop.Text = language.Stop;
            labelStartFrom.Text = language.StartOcrFrom;
            labelStatus.Text = language.LoadingVobSubImages;
            groupBoxSubtitleImage.Text = language.SubtitleImage;
            labelSubtitleText.Text = language.SubtitleText;
            buttonOK.Text = LanguageSettings.Current.General.Ok;
            buttonCancel.Text = LanguageSettings.Current.General.Cancel;
            subtitleListView1.InitializeLanguage(LanguageSettings.Current.General, Configuration.Settings);
            subtitleListView1.HideColumn(SubtitleListView.SubtitleColumn.CharactersPerSeconds);
            subtitleListView1.HideColumn(SubtitleListView.SubtitleColumn.Actor);
            subtitleListView1.HideColumn(SubtitleListView.SubtitleColumn.WordsPerMinute);
            subtitleListView1.HideColumn(SubtitleListView.SubtitleColumn.Region);
            subtitleListView1.AutoSizeColumns();

            groupBoxImagePalette.Text = language.ImagePalette;
            checkBoxCustomFourColors.Text = language.UseCustomColors;
            checkBoxBackgroundTransparent.Text = language.Transparent;
            labelMinAlpha.Text = language.TransparentMinAlpha;
            checkBoxPatternTransparent.Text = language.Transparent;
            checkBoxEmphasis1Transparent.Text = language.Transparent;
            checkBoxEmphasis2Transparent.Text = language.Transparent;
            toolStripMenuItemCaptureTopAlign.Text = language.CaptureTopAlign;
            captureTopAlignmentToolStripMenuItem.Text = language.CaptureTopAlign;
            checkBoxPromptForUnknownWords.Text = language.PromptForUnknownWords;
            checkBoxPromptForUnknownWords.Checked = Configuration.Settings.VobSubOcr.PromptForUnknownWords;
            checkBoxGuessUnknownWords.Checked = Configuration.Settings.VobSubOcr.GuessUnknownWords;
            checkBoxAutoFixCommonErrors.Checked = Configuration.Settings.VobSubOcr.FixOcrErrors;

            groupBoxTransportStream.Text = language.TransportStream;
            checkBoxTransportStreamGrayscale.Text = language.TransportStreamGrayscale;
            checkBoxTransportStreamGetColorAndSplit.Text = language.TransportStreamGetColor;
            checkBoxTransportStreamGetColorAndSplit.Left = checkBoxTransportStreamGrayscale.Left + checkBoxTransportStreamGrayscale.Width + 9;

            groupBoxOcrAutoFix.Text = language.OcrAutoCorrectionSpellChecking;
            checkBoxGuessUnknownWords.Text = language.TryToGuessUnkownWords;
            checkBoxAutoBreakLines.Text = language.AutoBreakSubtitleIfMoreThanTwoLines;
            checkBoxAutoBreakLines.Checked = Configuration.Settings.VobSubOcr.AutoBreakSubtitleIfMoreThanTwoLines;
            tabControlLogs.TabPages[0].Text = language.UnknownWords;
            tabControlLogs.TabPages[1].Text = language.AllFixes;
            tabControlLogs.TabPages[2].Text = language.GuessesUsed;

            buttonUknownToNames.Text = LanguageSettings.Current.SpellCheck.AddToNamesAndIgnoreList;
            buttonUknownToUserDic.Text = LanguageSettings.Current.SpellCheck.AddToUserDictionary;
            buttonAddToOcrReplaceList.Text = LanguageSettings.Current.SpellCheck.AddToOcrReplaceList;
            buttonGoogleIt.Text = LanguageSettings.Current.Main.VideoControls.GoogleIt;

            numericUpDownPixelsIsSpace.Left = labelNoOfPixelsIsSpace.Left + labelNoOfPixelsIsSpace.Width + 5;
            numericUpDownMaxErrorPct.Left = numericUpDownPixelsIsSpace.Left;
            groupBoxSubtitleImage.Text = string.Empty;
            labelFixesMade.Text = string.Empty;
            labelFixesMade.Left = checkBoxAutoFixCommonErrors.Left + checkBoxAutoFixCommonErrors.Width;

            labelDictionaryLoaded.Text = string.Format(language.DictionaryX, string.Empty);
            comboBoxDictionaries.Left = labelDictionaryLoaded.Left + labelDictionaryLoaded.Width;

            groupBoxImageCompareMethod.Text = string.Empty;
            groupBoxModiMethod.Text = string.Empty;
            GroupBoxTesseractMethod.Text = string.Empty;

            checkBoxAutoFixCommonErrors.Text = language.FixOcrErrors;
            checkBoxRightToLeft.Text = language.RightToLeft;
            checkBoxRightToLeft.Left = numericUpDownPixelsIsSpace.Left;
            if (checkBoxRightToLeft.Left + checkBoxRightToLeft.Width > labelMinLineSplitHeight.Left)
            {
                checkBoxRightToLeft.Left = labelMaxErrorPercent.Left;
            }

            labelMinLineSplitHeight.Text = language.MinLineSplitHeight;
            groupBoxOCRControls.Text = string.Empty;

            OcrTrainingToolStripMenuItem.Text = language.OcrTraining;
            toolStripMenuItemSetUnItalicFactor.Text = language.SetItalicAngle;

            FillSpellCheckDictionaries();

            cutToolStripMenuItem.Text = LanguageSettings.Current.Main.Menu.ContextMenu.Cut;
            copyToolStripMenuItem.Text = LanguageSettings.Current.Main.Menu.ContextMenu.Copy;
            pasteToolStripMenuItem.Text = LanguageSettings.Current.Main.Menu.ContextMenu.Paste;
            deleteToolStripMenuItem.Text = LanguageSettings.Current.Main.Menu.ContextMenu.Delete;
            selectAllToolStripMenuItem.Text = LanguageSettings.Current.Main.Menu.ContextMenu.SelectAll;
            normalToolStripMenuItem1.Text = LanguageSettings.Current.Main.Menu.ContextMenu.RemoveFormattingAll;
            boldToolStripMenuItem1.Text = LanguageSettings.Current.General.Bold;
            italicToolStripMenuItem1.Text = LanguageSettings.Current.General.Italic;
            underlineToolStripMenuItem1.Text = LanguageSettings.Current.Main.Menu.ContextMenu.Underline;

            InitializeModi();
            comboBoxOcrMethod.Items.Clear();
            _ocrMethodBinaryImageCompare = comboBoxOcrMethod.Items.Add(language.OcrViaImageCompare);
            if (Configuration.IsRunningOnLinux || Configuration.IsRunningOnMac)
            {
                Tesseract5Version = "4";
                checkBoxTesseractMusicOn.Checked = false;
                checkBoxTesseractMusicOn.Visible = false;
                checkBoxTesseractFallback.Checked = false;
                checkBoxTesseractFallback.Visible = false;
            }
            else
            {
                _ocrMethodTesseract302 = comboBoxOcrMethod.Items.Add(string.Format(language.OcrViaTesseractVersionX, "3.02"));
            }
            _ocrMethodTesseract4 = comboBoxOcrMethod.Items.Add(string.Format(language.OcrViaTesseractVersionX, Tesseract5Version));
            if (_modiEnabled)
            {
                _ocrMethodModi = comboBoxOcrMethod.Items.Add(language.OcrViaModi);
            }

            _ocrMethodNocr = comboBoxOcrMethod.Items.Add(language.OcrViaNOCR);

            checkBoxTesseractItalicsOn.Checked = Configuration.Settings.VobSubOcr.UseItalicsInTesseract;
            checkBoxTesseractItalicsOn.Text = LanguageSettings.Current.General.Italic;

            comboBoxTesseractEngineMode.Items.Clear();
            comboBoxTesseractEngineMode.Items.Add(language.TesseractEngineModeLegacy);
            comboBoxTesseractEngineMode.Items.Add(language.TesseractEngineModeNeural);
            comboBoxTesseractEngineMode.Items.Add(language.TesseractEngineModeBoth);
            comboBoxTesseractEngineMode.Items.Add(language.TesseractEngineModeDefault);
            if (Configuration.Settings.VobSubOcr.TesseractEngineMode >= 0 &&
                Configuration.Settings.VobSubOcr.TesseractEngineMode < comboBoxTesseractEngineMode.Items.Count)
            {
                comboBoxTesseractEngineMode.SelectedIndex = Configuration.Settings.VobSubOcr.TesseractEngineMode;
            }
            comboBoxTesseractEngineMode.Left = labelTesseractEngineMode.Left + labelTesseractEngineMode.Width + 5;
            comboBoxTesseractEngineMode.Width = GroupBoxTesseractMethod.Width - comboBoxTesseractEngineMode.Left - 10;

            checkBoxTesseractMusicOn.Checked = Configuration.Settings.VobSubOcr.UseMusicSymbolsInTesseract;
            checkBoxTesseractMusicOn.Text = LanguageSettings.Current.Settings.MusicSymbol;
            checkBoxTesseractMusicOn.Left = checkBoxTesseractItalicsOn.Left + checkBoxTesseractItalicsOn.Width + 15;
            checkBoxTesseractFallback.Checked = Configuration.Settings.VobSubOcr.UseTesseractFallback;
            toolStripMenuItemCaptureTopAlign.Checked = Configuration.Settings.VobSubOcr.CaptureTopAlign;
            captureTopAlignmentToolStripMenuItem.Checked = Configuration.Settings.VobSubOcr.CaptureTopAlign;

            if (Configuration.Settings.VobSubOcr.ItalicFactor >= 0.1 && Configuration.Settings.VobSubOcr.ItalicFactor < 1)
            {
                _unItalicFactor = Configuration.Settings.VobSubOcr.ItalicFactor;
            }

            numericUpDownPixelsIsSpace.Value = Configuration.Settings.VobSubOcr.XOrMorePixelsMakesSpace;
            numericUpDownNumberOfPixelsIsSpaceNOCR.Value = Configuration.Settings.VobSubOcr.XOrMorePixelsMakesSpace;

            checkBoxShowOnlyForced.Text = language.ShowOnlyForcedSubtitles;
            checkBoxUseTimeCodesFromIdx.Text = language.UseTimeCodesFromIdx;

            normalToolStripMenuItem.Text = LanguageSettings.Current.Main.Menu.ContextMenu.RemoveFormattingAll;
            italicToolStripMenuItem.Text = LanguageSettings.Current.General.Italic;
            importTextWithMatchingTimeCodesToolStripMenuItem.Text = language.ImportTextWithMatchingTimeCodes;
            importNewTimeCodesToolStripMenuItem.Text = language.ImportNewTimeCodes;
            saveImageAsToolStripMenuItem.Text = language.SaveSubtitleImageAs;
            toolStripMenuItemImageSaveAs.Text = language.SaveSubtitleImageAs;
            previewToolStripMenuItem.Text = LanguageSettings.Current.General.Preview;
            saveAllImagesWithHtmlIndexViewToolStripMenuItem.Text = language.SaveAllSubtitleImagesWithHtml;
            inspectImageCompareMatchesForCurrentImageToolStripMenuItem.Text = language.InspectCompareMatchesForCurrentImage;
            EditLastAdditionsToolStripMenuItem.Text = language.EditLastAdditions;
            checkBoxRightToLeft.Checked = Configuration.Settings.VobSubOcr.RightToLeft;
            toolStripMenuItemSetUnItalicFactor.Text = language.SetItalicAngle;
            setItalicAngleToolStripMenuItem.Text = language.SetItalicAngle;
            deleteToolStripMenuItem.Text = LanguageSettings.Current.Main.Menu.ContextMenu.Delete;
            imagePreprocessingToolStripMenuItem1.Text = language.ImagePreProcessing;
            ImagePreProcessingToolStripMenuItem.Text = language.ImagePreProcessing;
            autoTransparentBackgroundToolStripMenuItem.Text = language.AutoTransparentBackground;
            toolStripMenuItemSaveSubtitleAs.Text = LanguageSettings.Current.Main.SaveSubtitleAs;
            toolStripMenuItemExport.Text = LanguageSettings.Current.Main.Menu.File.Export;
            vobSubToolStripMenuItem.Text = LanguageSettings.Current.Main.Menu.File.ExportVobSub;
            bDNXMLToolStripMenuItem.Text = LanguageSettings.Current.Main.Menu.File.ExportBdnXml;
            bluraySupToolStripMenuItem.Text = LanguageSettings.Current.Main.Menu.File.ExportBluRaySup;

            toolStripMenuItemClearFixes.Text = LanguageSettings.Current.DvdSubRip.Clear;
            toolStripMenuItemClearGuesses.Text = LanguageSettings.Current.DvdSubRip.Clear;
            clearToolStripMenuItem.Text = LanguageSettings.Current.DvdSubRip.Clear;

            checkBoxNOcrDrawUnknownLetters.Checked = Configuration.Settings.VobSubOcr.LineOcrDraw;
            comboBoxNOcrLineSplitMinHeight.SelectedIndex = Configuration.Settings.VobSubOcr.LineOcrMaxLineHeight;
            checkBoxNOcrItalic.Checked = Configuration.Settings.VobSubOcr.LineOcrAdvancedItalic;
            numericUpDownNOcrMaxWrongPixels.Value = Configuration.Settings.VobSubOcr.LineOcrMaxErrorPixels;

            comboBoxTesseractLanguages.Left = labelTesseractLanguage.Left + labelTesseractLanguage.Width;
            buttonGetTesseractDictionaries.Left = comboBoxTesseractLanguages.Left + comboBoxTesseractLanguages.Width + 5;

            UiUtil.InitializeSubtitleFont(subtitleListView1);
            subtitleListView1.AutoSizeAllColumns(this);

            UiUtil.InitializeSubtitleFont(textBoxCurrentText);

            italicToolStripMenuItem.ShortcutKeys = UiUtil.GetKeys(Configuration.Settings.Shortcuts.MainListViewItalic);

            comboBoxTesseractLanguages.Left = labelTesseractLanguage.Left + labelTesseractLanguage.Width + 3;
            comboBoxModiLanguage.Left = label1.Left + label1.Width + 3;

            comboBoxCharacterDatabase.Left = labelImageDatabase.Left + labelImageDatabase.Width + 3;
            comboBoxCharacterDatabase.Width = buttonChooseEditBinaryImageCompareDb.Left - comboBoxCharacterDatabase.Left - 10;
            numericUpDownPixelsIsSpace.Left = labelNoOfPixelsIsSpace.Left + labelNoOfPixelsIsSpace.Width + 3;
            checkBoxRightToLeft.Left = numericUpDownPixelsIsSpace.Left;

            UiUtil.FixLargeFonts(this, buttonCancel);

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

            try
            {
                numericUpDownMaxErrorPct.Value = (decimal)Configuration.Settings.VobSubOcr.AllowDifferenceInPercent;
            }
            catch
            {
                numericUpDownMaxErrorPct.Value = 1.1m;
            }

            comboBoxLineSplitMinLineHeight.SelectedIndex = 0;

            if (comboBoxDictionaries.SelectedIndex == -1)
            {
                comboBoxDictionaries.SelectedIndex = 0;
            }
        }

        private void FillSpellCheckDictionaries()
        {
            comboBoxDictionaries.SelectedIndexChanged -= comboBoxDictionaries_SelectedIndexChanged;
            comboBoxDictionaries.Items.Clear();
            comboBoxDictionaries.Items.Add(LanguageSettings.Current.General.None);
            foreach (string name in Utilities.GetDictionaryLanguages())
            {
                comboBoxDictionaries.Items.Add(name);
            }

            comboBoxDictionaries.SelectedIndexChanged += comboBoxDictionaries_SelectedIndexChanged;
        }

        internal void InitializeBatch(string vobSubFileName, VobSubOcrSettings vobSubOcrSettings, bool forcedOnly, string ocrEngine, string language = null)
        {
            Initialize(vobSubFileName, vobSubOcrSettings, null, true);
            FormVobSubOcr_Shown(null, null);
            checkBoxShowOnlyForced.Checked = forcedOnly;
            checkBoxPromptForUnknownWords.Checked = false;

            if (ocrEngine?.ToLowerInvariant() == "nocr")
            {
                var oldNOcrDrawText = checkBoxNOcrDrawUnknownLetters.Checked;
                InitializeNOcrForBatch(language);
                checkBoxShowOnlyForced.Checked = forcedOnly;
                DoBatch();
                checkBoxNOcrDrawUnknownLetters.Checked = oldNOcrDrawText;
                return;
            }

            _ocrMethodIndex = Configuration.Settings.VobSubOcr.LastOcrMethod == "Tesseract4" ? _ocrMethodTesseract4 : _ocrMethodTesseract302;
            if (language == null)
            {
                language = Configuration.Settings.VobSubOcr.TesseractLastLanguage;
            }
            if (string.IsNullOrEmpty(language))
            {
                language = "en";
            }
            InitializeTesseract(language);
            SetTesseractLanguageFromLanguageString(language);

            int max = GetSubtitleCount();
            if (_tesseractAsyncStrings == null)
            {
                _tesseractAsyncStrings = new string[max];
                _tesseractAsyncIndex = (int)numericUpDownStartNumber.Value + 5;
            }

            System.Threading.Thread.Sleep(1000);
            subtitleListView1.SelectedIndexChanged -= SubtitleListView1SelectedIndexChanged;
            textBoxCurrentText.TextChanged -= TextBoxCurrentTextTextChanged;

            _abort = false;
            for (int i = 0; i < max; i++)
            {
                if (ProgressCallback != null)
                {
                    var percent = (int)Math.Round((i + 1) * 100.0 / max);
                    ProgressCallback?.Invoke($"{percent}%");
                }

                _selectedIndex = i;
                subtitleListView1.SelectIndexAndEnsureVisible(i);

                string text = OcrViaTesseract(GetSubtitleBitmap(i), i);

                _lastLine = text;

                text = text.Replace("<i>-</i>", "-");
                text = text.Replace("<i>- </i>", "- ");
                text = text.Replace("<i> - </i>", "- ");
                text = text.Replace("<i> -</i>", "- ");
                text = text.Replace("<i>a</i>", "a");
                text = text.Replace("<i>.</i>", ".");
                text = text.Replace("  ", " ");
                text = text.Trim();

                text = text.Replace(" " + Environment.NewLine, Environment.NewLine);
                text = text.Replace(Environment.NewLine + " ", Environment.NewLine);

                // max allow 2 lines
                if (checkBoxAutoBreakLines.Checked && Utilities.GetNumberOfLines(text) > 2)
                {
                    text = text.Replace(" " + Environment.NewLine, Environment.NewLine);
                    text = text.Replace(Environment.NewLine + " ", Environment.NewLine);
                    while (text.Contains(Environment.NewLine + Environment.NewLine))
                    {
                        text = text.Replace(Environment.NewLine + Environment.NewLine, Environment.NewLine);
                    }

                    if (Utilities.GetNumberOfLines(text) > 2)
                    {
                        text = Utilities.AutoBreakLine(text);
                    }
                }

                text = text.Trim();
                text = text.Replace("  ", " ");
                text = text.Replace(Environment.NewLine + Environment.NewLine, Environment.NewLine);
                text = text.Replace("  ", " ");
                text = text.Replace(Environment.NewLine + Environment.NewLine, Environment.NewLine);

                _subtitle.Paragraphs[i].Text = text;

                Application.DoEvents();
                if (_abort)
                {
                    SetButtonsEnabledAfterOcrDone();
                    return;
                }
            }

            SetButtonsEnabledAfterOcrDone();
            checkBoxPromptForUnknownWords.Checked = Configuration.Settings.VobSubOcr.PromptForUnknownWords;
        }

        internal bool Initialize(string vobSubFileName, VobSubOcrSettings vobSubOcrSettings, Main main, bool batchMode = false)
        {
            _main = main;
            SetButtonsStartOcr();
            progressBar1.Visible = false;
            progressBar1.Maximum = 100;
            progressBar1.Value = 0;
            numericUpDownPixelsIsSpace.Value = vobSubOcrSettings.XOrMorePixelsMakesSpace;
            numericUpDownNumberOfPixelsIsSpaceNOCR.Value = vobSubOcrSettings.XOrMorePixelsMakesSpace;
            _vobSubOcrSettings = vobSubOcrSettings;

            InitializeTesseract();
            LoadImageCompareCharacterDatabaseList(Configuration.Settings.VobSubOcr.LastBinaryImageCompareDb);

            SetOcrMethod();

            FileName = vobSubFileName;
            Text += " - " + Path.GetFileName(FileName);

            return InitializeSubIdx(vobSubFileName, batchMode);
        }

        internal void Initialize(List<VobSubMergedPack> vobSubMergedPackList, List<Color> palette, VobSubOcrSettings vobSubOcrSettings, string languageString)
        {
            SetButtonsStartOcr();
            progressBar1.Visible = false;
            progressBar1.Maximum = 100;
            progressBar1.Value = 0;
            numericUpDownPixelsIsSpace.Value = vobSubOcrSettings.XOrMorePixelsMakesSpace;
            numericUpDownNumberOfPixelsIsSpaceNOCR.Value = vobSubOcrSettings.XOrMorePixelsMakesSpace;
            _vobSubOcrSettings = vobSubOcrSettings;

            InitializeTesseract();
            LoadImageCompareCharacterDatabaseList(Configuration.Settings.VobSubOcr.LastBinaryImageCompareDb);

            SetOcrMethod();

            _vobSubMergedPackList = vobSubMergedPackList;
            _palette = palette;

            if (_palette == null)
            {
                checkBoxCustomFourColors.Checked = true;
            }

            SetTesseractLanguageFromLanguageString(languageString);
            _importLanguageString = languageString;
        }

        internal void Initialize(Subtitle subtitle, List<DvbSubPes> subtitleImages, VobSubOcrSettings vobSubOcrSettings)
        {
            SetButtonsStartOcr();
            progressBar1.Visible = false;
            progressBar1.Maximum = 100;
            progressBar1.Value = 0;
            numericUpDownPixelsIsSpace.Value = vobSubOcrSettings.XOrMorePixelsMakesSpace;
            numericUpDownNumberOfPixelsIsSpaceNOCR.Value = vobSubOcrSettings.XOrMorePixelsMakesSpace;
            _vobSubOcrSettings = vobSubOcrSettings;

            InitializeTesseract();
            LoadImageCompareCharacterDatabaseList(Configuration.Settings.VobSubOcr.LastBinaryImageCompareDb);

            SetOcrMethod();
            groupBoxImagePalette.Visible = false;
            _dvbPesSubtitles = subtitleImages;
            _subtitle = subtitle;
            _subtitle.Renumber();
            subtitleListView1.Fill(_subtitle);
            subtitleListView1.SelectIndexAndEnsureVisible(0);
            autoTransparentBackgroundToolStripMenuItem.Checked = false;
            autoTransparentBackgroundToolStripMenuItem.Visible = false;
        }

        internal void InitializeQuick(List<VobSubMergedPack> vobSubMergedPackist, List<Color> palette, VobSubOcrSettings vobSubOcrSettings, string languageString)
        {
            SetButtonsStartOcr();
            progressBar1.Visible = false;
            progressBar1.Maximum = 100;
            progressBar1.Value = 0;
            numericUpDownPixelsIsSpace.Value = vobSubOcrSettings.XOrMorePixelsMakesSpace;
            numericUpDownNumberOfPixelsIsSpaceNOCR.Value = vobSubOcrSettings.XOrMorePixelsMakesSpace;
            _vobSubOcrSettings = vobSubOcrSettings;
            _vobSubMergedPackList = vobSubMergedPackist;
            _palette = palette;

            if (_palette == null)
            {
                checkBoxCustomFourColors.Checked = true;
            }

            _importLanguageString = languageString;
            if (_importLanguageString != null && _importLanguageString.Contains('(') && !_importLanguageString.StartsWith('('))
            {
                _importLanguageString = _importLanguageString.Substring(0, languageString.IndexOf('(') - 1).Trim();
            }
        }

        internal void InitializeBatch(IList<IBinaryParagraph> subtitles, VobSubOcrSettings vobSubOcrSettings, string fileName, bool forcedOnly, string language = null, string ocrEngine = null)
        {
            if (subtitles.Count == 0)
            {
                return;
            }

            if (subtitles.First() is TransportStreamSubtitle)
            {
                var tssList = new List<TransportStreamSubtitle>();
                foreach (var binaryParagraph in subtitles)
                {
                    tssList.Add(binaryParagraph as TransportStreamSubtitle);
                }

                InitializeBatch(tssList, vobSubOcrSettings, fileName, forcedOnly, language, ocrEngine);
            }
        }

        internal void InitializeBatch(List<TransportStreamSubtitle> subtitles, VobSubOcrSettings vobSubOcrSettings, string fileName, bool forcedOnly, string language = null, string ocrEngine = null)
        {
            Initialize(subtitles, vobSubOcrSettings, fileName, language);
            _ocrMethodIndex = Configuration.Settings.VobSubOcr.LastOcrMethod == "Tesseract4" ? _ocrMethodTesseract4 : _ocrMethodTesseract302;
            var oldNOcrDrawText = checkBoxNOcrDrawUnknownLetters.Checked;

            if (ocrEngine?.ToLowerInvariant() == "nocr")
            {
                InitializeNOcrForBatch(language);
            }
            else
            {
                if (string.IsNullOrEmpty(language))
                {
                    language = Configuration.Settings.VobSubOcr.TesseractLastLanguage;
                }

                if (string.IsNullOrEmpty(language))
                {
                    language = "en";
                }

                InitializeTesseract(language);
                SetTesseractLanguageFromLanguageString(language);
            }

            checkBoxShowOnlyForced.Checked = forcedOnly;
            DoBatch();
            checkBoxNOcrDrawUnknownLetters.Checked = oldNOcrDrawText;
        }

        internal void InitializeBatch(List<BluRaySupParser.PcsData> subtitles, VobSubOcrSettings vobSubOcrSettings, string fileName, bool forcedOnly, string language = null, string ocrEngine = null)
        {
            Initialize(subtitles, vobSubOcrSettings, fileName);
            _ocrMethodIndex = Configuration.Settings.VobSubOcr.LastOcrMethod == "Tesseract4" ? _ocrMethodTesseract4 : _ocrMethodTesseract302;
            var oldNOcrDrawText = checkBoxNOcrDrawUnknownLetters.Checked;

            InitializeOcrEngineBatch(language, ocrEngine);

            checkBoxShowOnlyForced.Checked = forcedOnly;
            DoBatch();
            checkBoxNOcrDrawUnknownLetters.Checked = oldNOcrDrawText;
        }

        private void InitializeOcrEngineBatch(string language, string ocrEngine)
        {
            if (ocrEngine?.ToLowerInvariant() == "nocr")
            {
                InitializeNOcrForBatch(language);
            }
            else
            {
                if (string.IsNullOrEmpty(language))
                {
                    language = Configuration.Settings.VobSubOcr.TesseractLastLanguage;
                }

                if (string.IsNullOrEmpty(language))
                {
                    language = "en";
                }

                InitializeTesseract(language);
                SetTesseractLanguageFromLanguageString(language);
            }
        }

        internal void InitializeBatch(List<VobSubMergedPack> vobSubMergedPackList, List<Color> palette, VobSubOcrSettings vobSubOcrSettings, string fileName, bool forcedOnly, string language, string ocrEngine)
        {
            Initialize(vobSubMergedPackList, palette, vobSubOcrSettings, language);
            checkBoxShowOnlyForced.Checked = forcedOnly;
            InitializeOcrEngineBatch(language, ocrEngine);
            DoBatch();
        }

        internal void InitializeBatch(List<SubPicturesWithSeparateTimeCodes> list, string fileName, string language, string ocrEngine)
        {
            Initialize(list, Configuration.Settings.VobSubOcr, fileName);
            InitializeOcrEngineBatch(language, ocrEngine);
            DoBatch();
        }

        private void DoBatch()
        {
            _abort = false;
            FormVobSubOcr_Shown(null, null);
            checkBoxPromptForUnknownWords.Checked = false;

            int max = GetSubtitleCount();
            if (_ocrMethodIndex != _ocrMethodTesseract4 && _ocrMethodIndex != _ocrMethodTesseract302 && _ocrMethodIndex != _ocrMethodNocr)
            {
                _ocrMethodIndex = _ocrMethodTesseract302;
            }

            if (_ocrMethodIndex == _ocrMethodTesseract4 && _tesseractAsyncStrings == null)
            {
                _tesseractAsyncStrings = new string[max];
                _tesseractAsyncIndex = (int)numericUpDownStartNumber.Value + 5;
            }

            System.Threading.Thread.Sleep(1000);
            subtitleListView1.SelectedIndexChanged -= SubtitleListView1SelectedIndexChanged;
            textBoxCurrentText.TextChanged -= TextBoxCurrentTextTextChanged;
            for (int i = 0; i < max; i++)
            {
                _selectedIndex = i;
                Application.DoEvents();
                if (_abort)
                {
                    SetButtonsEnabledAfterOcrDone();
                    return;
                }
                if (ProgressCallback != null)
                {
                    var percent = (int)Math.Round((i + 1) * 100.0 / max);
                    ProgressCallback?.Invoke($"{percent}%");
                }

                subtitleListView1.SelectIndexAndEnsureVisible(i);
                string text;
                if (_ocrMethodIndex == _ocrMethodNocr)
                {
                    text = OcrViaNOCR(GetSubtitleBitmap(i), i);
                }
                else
                {
                    text = OcrViaTesseract(GetSubtitleBitmap(i), i);
                }

                _lastLine = text;

                text = text.Replace("<i>-</i>", "-");
                text = text.Replace("<i>a</i>", "a");
                text = text.Replace("  ", " ");
                text = text.Trim();

                text = text.Replace(" " + Environment.NewLine, Environment.NewLine);
                text = text.Replace(Environment.NewLine + " ", Environment.NewLine);

                // max allow 2 lines
                if (checkBoxAutoBreakLines.Checked && Utilities.GetNumberOfLines(text) > 2)
                {
                    text = text.Replace(" " + Environment.NewLine, Environment.NewLine);
                    text = text.Replace(Environment.NewLine + " ", Environment.NewLine);
                    while (text.Contains(Environment.NewLine + Environment.NewLine))
                    {
                        text = text.Replace(Environment.NewLine + Environment.NewLine, Environment.NewLine);
                    }

                    if (Utilities.GetNumberOfLines(text) > 2)
                    {
                        text = Utilities.AutoBreakLine(text);
                    }
                }

                text = text.Trim();
                text = text.Replace("  ", " ");
                text = text.Replace(Environment.NewLine + Environment.NewLine, Environment.NewLine);
                text = text.Replace("  ", " ");
                text = text.Replace(Environment.NewLine + Environment.NewLine, Environment.NewLine);

                _subtitle.Paragraphs[i].Text = text;

                Application.DoEvents();
                if (_abort)
                {
                    SetButtonsEnabledAfterOcrDone();
                    return;
                }
            }
            checkBoxPromptForUnknownWords.Checked = Configuration.Settings.VobSubOcr.PromptForUnknownWords;
        }

        internal void InitializeBatch(Subtitle imageListSubtitle, VobSubOcrSettings vobSubOcrSettings, bool isSon, string language, string ocrEngine)
        {
            Initialize(imageListSubtitle, vobSubOcrSettings, isSon);
            _bdnXmlOriginal = imageListSubtitle;
            _bdnFileName = imageListSubtitle.FileName;
            _isSon = isSon;
            if (_isSon)
            {
                checkBoxCustomFourColors.Checked = true;
                pictureBoxBackground.BackColor = Color.Transparent;
                pictureBoxPattern.BackColor = Color.DarkGray;
                pictureBoxEmphasis1.BackColor = Color.Black;
                pictureBoxEmphasis2.BackColor = Color.White;
            }

            InitializeOcrEngineBatch(language, ocrEngine);
            DoBatch();
        }

        internal void Initialize(List<BluRaySupParser.PcsData> subtitles, VobSubOcrSettings vobSubOcrSettings, string fileName)
        {
            SetButtonsStartOcr();
            progressBar1.Visible = false;
            progressBar1.Maximum = 100;
            progressBar1.Value = 0;
            numericUpDownPixelsIsSpace.Value = vobSubOcrSettings.XOrMorePixelsMakesSpace;
            numericUpDownNumberOfPixelsIsSpaceNOCR.Value = vobSubOcrSettings.XOrMorePixelsMakesSpace;
            _vobSubOcrSettings = vobSubOcrSettings;

            InitializeTesseract();
            LoadImageCompareCharacterDatabaseList(Configuration.Settings.VobSubOcr.LastBinaryImageCompareDb);

            SetOcrMethod();

            _bluRaySubtitlesOriginal = subtitles;

            groupBoxImagePalette.Visible = false;

            Text = LanguageSettings.Current.VobSubOcr.TitleBluRay;
            if (!string.IsNullOrEmpty(fileName))
            {
                Text += " - " + Path.GetFileName(fileName);
                FileName = fileName;
                _subtitle.FileName = fileName;
            }

            autoTransparentBackgroundToolStripMenuItem.Checked = false;
            autoTransparentBackgroundToolStripMenuItem.Visible = false;
        }

        private void LoadImageCompareCharacterDatabaseList(string db)
        {
            if (_ocrMethodIndex != _ocrMethodBinaryImageCompare)
            {
                return;
            }

            try
            {
                string characterDatabasePath = Configuration.OcrDirectory.TrimEnd(Path.DirectorySeparatorChar);
                if (!Directory.Exists(characterDatabasePath))
                {
                    Directory.CreateDirectory(characterDatabasePath);
                }

                comboBoxCharacterDatabase.Items.Clear();
                var binaryOcrDbs = BinaryOcrDb.GetDatabases();
                var imageCompareDbName = string.Empty;
                if (!string.IsNullOrEmpty(db))
                {
                    var parts = db.Split('+');
                    if (parts.Length > 0 && binaryOcrDbs.Contains(parts[0]))
                    {
                        imageCompareDbName = parts[0];
                        if (parts.Length > 1)
                        {
                            imageCompareDbName = db;
                            var nOcrDbName = parts[1];
                            _nOcrDb = new NOcrDb(Path.Combine(Configuration.OcrDirectory, nOcrDbName + ".nocr"));
                            binaryOcrDbs.Insert(0, db);
                        }
                    }
                }

                foreach (string s in binaryOcrDbs)
                {
                    comboBoxCharacterDatabase.Items.Add(s);
                    if (s == imageCompareDbName)
                    {
                        comboBoxCharacterDatabase.SelectedIndex = comboBoxCharacterDatabase.Items.Count - 1;
                    }
                }

                if (comboBoxCharacterDatabase.Items.Count == 0)
                {
                    comboBoxCharacterDatabase.Items.Add("Latin"); // if no database, create an empty one called "Latin"
                }

                if (comboBoxCharacterDatabase.SelectedIndex < 0 && comboBoxCharacterDatabase.Items.Count > 0)
                {
                    comboBoxCharacterDatabase.SelectedIndex = 0;
                }

                for (int i = 0; i < comboBoxDictionaries.Items.Count; i++)
                {
                    if (comboBoxDictionaries.Items[i].ToString() == Configuration.Settings.VobSubOcr.LastBinaryImageSpellCheck)
                    {
                        comboBoxDictionaries.SelectedIndex = i;
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format(LanguageSettings.Current.VobSubOcr.UnableToCreateCharacterDatabaseFolder, ex.Message));
            }
        }

        private void LoadImageCompareBitmaps()
        {
            DisposeImageCompareBitmaps();
            _binaryOcrDb = null;
            _nOcrDb = null;

            if (_ocrMethodIndex == _ocrMethodBinaryImageCompare)
            {
                var binaryOcrDbs = BinaryOcrDb.GetDatabases();
                var parts = comboBoxCharacterDatabase.SelectedItem.ToString().Split('+');
                if (parts.Length > 0 && binaryOcrDbs.Contains(parts[0]))
                {
                    var imageCompareDbName = parts[0];
                    if (parts.Length > 1)
                    {
                        var nOcrDbName = parts[1];
                        _nOcrDb = new NOcrDb(Path.Combine(Configuration.OcrDirectory, nOcrDbName + ".nocr"));
                    }
                    var db = Path.Combine(Configuration.OcrDirectory, imageCompareDbName + ".db");
                    _binaryOcrDb = new BinaryOcrDb(db, true);
                }
            }
        }

        private void DisposeImageCompareBitmaps()
        {
            _compareBitmaps = null;
        }

        private bool InitializeSubIdx(string vobSubFileName, bool batchMode = false)
        {
            var vobSubParser = new VobSubParser(true);
            string idxFileName = Path.ChangeExtension(vobSubFileName, ".idx");
            vobSubParser.OpenSubIdx(vobSubFileName, idxFileName);
            _vobSubMergedPackList = vobSubParser.MergeVobSubPacks();
            _palette = vobSubParser.IdxPalette;
            vobSubParser.VobSubPacks.Clear();

            var languageStreamIds = new List<int>();
            foreach (var pack in _vobSubMergedPackList)
            {
                if (pack.SubPicture.Delay.TotalMilliseconds > 500 && !languageStreamIds.Contains(pack.StreamId))
                {
                    languageStreamIds.Add(pack.StreamId);
                }
            }

            if (languageStreamIds.Count > 1)
            {
                using (var chooseLanguage = new DvdSubRipChooseLanguage())
                {
                    if (ShowInTaskbar)
                    {
                        chooseLanguage.Icon = (Icon)Icon.Clone();
                        chooseLanguage.ShowInTaskbar = true;
                        chooseLanguage.ShowIcon = true;
                    }

                    chooseLanguage.Initialize(_vobSubMergedPackList, _palette, vobSubParser.IdxLanguages, string.Empty);
                    var form = _main ?? (Form)this;
                    if (batchMode)
                    {
                        chooseLanguage.SelectActive();
                        _vobSubMergedPackList = chooseLanguage.SelectedVobSubMergedPacks;
                        SetTesseractLanguageFromLanguageString(chooseLanguage.SelectedLanguageString);
                        _importLanguageString = chooseLanguage.SelectedLanguageString;
                        return true;
                    }

                    chooseLanguage.Activate();
                    if (chooseLanguage.ShowDialog(form) == DialogResult.OK)
                    {
                        _vobSubMergedPackList = chooseLanguage.SelectedVobSubMergedPacks;
                        SetTesseractLanguageFromLanguageString(chooseLanguage.SelectedLanguageString);
                        _importLanguageString = chooseLanguage.SelectedLanguageString;
                        return true;
                    }

                    return false;
                }
            }

            return true;
        }

        private void SetTesseractLanguageFromLanguageString(string languageString)
        {
            // try to match language from vob to Tesseract language
            if (comboBoxTesseractLanguages.SelectedIndex >= 0 && comboBoxTesseractLanguages.Items.Count > 1 && languageString != null)
            {
                languageString = languageString.ToLowerInvariant();
                for (int i = 0; i < comboBoxTesseractLanguages.Items.Count; i++)
                {
                    var tl = comboBoxTesseractLanguages.Items[i] as TesseractLanguage;
                    if (tl.Text.StartsWith("Chinese", StringComparison.OrdinalIgnoreCase) && (languageString.StartsWith("chinese", StringComparison.OrdinalIgnoreCase) || languageString.StartsWith("中文", StringComparison.OrdinalIgnoreCase)))
                    {
                        comboBoxTesseractLanguages.SelectedIndex = i;
                        break;
                    }

                    if (tl.Text.StartsWith("Korean", StringComparison.OrdinalIgnoreCase) && (languageString.StartsWith("korean", StringComparison.OrdinalIgnoreCase) || languageString.StartsWith("한국어", StringComparison.OrdinalIgnoreCase)) ||
                        tl.Text.StartsWith("Korean", StringComparison.OrdinalIgnoreCase) && languageString.Equals("kor", StringComparison.OrdinalIgnoreCase))
                    {
                        comboBoxTesseractLanguages.SelectedIndex = i;
                        break;
                    }

                    if (tl.Text.StartsWith("Swedish", StringComparison.OrdinalIgnoreCase) && languageString.StartsWith("svenska", StringComparison.OrdinalIgnoreCase) ||
                        tl.Text.StartsWith("Swedish", StringComparison.OrdinalIgnoreCase) && languageString.Equals("swe", StringComparison.OrdinalIgnoreCase))
                    {
                        comboBoxTesseractLanguages.SelectedIndex = i;
                        break;
                    }

                    if (tl.Text.StartsWith("Norwegian", StringComparison.OrdinalIgnoreCase) && languageString.StartsWith("norsk", StringComparison.OrdinalIgnoreCase) ||
                        tl.Text.StartsWith("Norwegian", StringComparison.OrdinalIgnoreCase) && languageString.StartsWith("nor", StringComparison.OrdinalIgnoreCase))
                    {
                        comboBoxTesseractLanguages.SelectedIndex = i;
                        break;
                    }

                    if (tl.Text.StartsWith("Dutch", StringComparison.OrdinalIgnoreCase) && languageString.StartsWith("Nederlands", StringComparison.OrdinalIgnoreCase) ||
                        tl.Text.StartsWith("Dutch", StringComparison.OrdinalIgnoreCase) && languageString.Equals("nld", StringComparison.OrdinalIgnoreCase))
                    {
                        comboBoxTesseractLanguages.SelectedIndex = i;
                        break;
                    }

                    if (tl.Text.StartsWith("Danish", StringComparison.OrdinalIgnoreCase) && languageString.StartsWith("dansk", StringComparison.OrdinalIgnoreCase) ||
                        tl.Text.StartsWith("Danish", StringComparison.OrdinalIgnoreCase) && languageString.Equals("dnk", StringComparison.OrdinalIgnoreCase))
                    {
                        comboBoxTesseractLanguages.SelectedIndex = i;
                        break;
                    }

                    if (tl.Text.StartsWith("English", StringComparison.OrdinalIgnoreCase) && languageString.StartsWith("English", StringComparison.OrdinalIgnoreCase))
                    {
                        comboBoxTesseractLanguages.SelectedIndex = i;
                        break;
                    }

                    if (tl.Text.StartsWith("French", StringComparison.OrdinalIgnoreCase) && (languageString.StartsWith("french", StringComparison.OrdinalIgnoreCase) || languageString.StartsWith("français", StringComparison.OrdinalIgnoreCase)) ||
                        tl.Text.StartsWith("French", StringComparison.OrdinalIgnoreCase) && languageString.Equals("fra", StringComparison.OrdinalIgnoreCase))
                    {
                        comboBoxTesseractLanguages.SelectedIndex = i;
                        break;
                    }

                    if (tl.Text.StartsWith("Spanish", StringComparison.OrdinalIgnoreCase) && (languageString.StartsWith("spanish", StringComparison.OrdinalIgnoreCase) || languageString.StartsWith("españo", StringComparison.OrdinalIgnoreCase)) ||
                        tl.Text.StartsWith("Spanish", StringComparison.OrdinalIgnoreCase) && languageString.Equals("esp", StringComparison.OrdinalIgnoreCase))
                    {
                        comboBoxTesseractLanguages.SelectedIndex = i;
                        break;
                    }

                    if (tl.Text.StartsWith("Finnish", StringComparison.OrdinalIgnoreCase) && languageString.StartsWith("suomi", StringComparison.OrdinalIgnoreCase) ||
                        tl.Text.StartsWith("Finnish", StringComparison.OrdinalIgnoreCase) && languageString.Equals("fin", StringComparison.OrdinalIgnoreCase))
                    {
                        comboBoxTesseractLanguages.SelectedIndex = i;
                        break;
                    }

                    if (tl.Text.StartsWith("Italian", StringComparison.OrdinalIgnoreCase) && languageString.StartsWith("itali", StringComparison.OrdinalIgnoreCase) ||
                        tl.Text.StartsWith("Italian", StringComparison.OrdinalIgnoreCase) && languageString.Equals("ita", StringComparison.OrdinalIgnoreCase))
                    {
                        comboBoxTesseractLanguages.SelectedIndex = i;
                        break;
                    }

                    if (tl.Text.StartsWith("German", StringComparison.OrdinalIgnoreCase) && languageString.StartsWith("deutsch", StringComparison.OrdinalIgnoreCase) ||
                        tl.Text.StartsWith("German", StringComparison.OrdinalIgnoreCase) && languageString.Equals("ger", StringComparison.OrdinalIgnoreCase))
                    {
                        comboBoxTesseractLanguages.SelectedIndex = i;
                        break;
                    }

                    if (tl.Text.StartsWith("Portuguese", StringComparison.OrdinalIgnoreCase) && languageString.StartsWith("português", StringComparison.OrdinalIgnoreCase) ||
                        tl.Text.StartsWith("Portuguese", StringComparison.OrdinalIgnoreCase) && languageString.Equals("prt", StringComparison.OrdinalIgnoreCase))
                    {
                        comboBoxTesseractLanguages.SelectedIndex = i;
                        break;
                    }

                    if (tl.Id.Equals(languageString, StringComparison.OrdinalIgnoreCase))
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
                if (checkBoxShowOnlyForced.Checked && x.Forced ||
                    checkBoxShowOnlyForced.Checked == false)
                {
                    _bdnXmlSubtitle.Paragraphs.Add(new Paragraph(x));
                    _subtitle.Paragraphs.Add(new Paragraph(x) { Text = string.Empty });
                }
            }

            _subtitle.Renumber();

            FixShortDisplayTimes(_subtitle);

            subtitleListView1.Fill(_subtitle);
            subtitleListView1.SelectIndexAndEnsureVisible(0);

            numericUpDownStartNumber.Maximum = max;
            if (numericUpDownStartNumber.Maximum > 0 && numericUpDownStartNumber.Minimum <= 1)
            {
                numericUpDownStartNumber.Value = 1;
            }

            SetButtonsEnabledAfterOcrDone();
            buttonStartOcr.Focus();
        }

        private void LoadBluRaySup()
        {
            _subtitle = new Subtitle();

            _bluRaySubtitles = new List<BluRaySupParser.PcsData>();
            int max = _bluRaySubtitlesOriginal.Count;
            for (int i = 0; i < max; i++)
            {
                var x = _bluRaySubtitlesOriginal[i];
                if (checkBoxShowOnlyForced.Checked && x.IsForced || checkBoxShowOnlyForced.Checked == false)
                {
                    _bluRaySubtitles.Add(x);
                    _subtitle.Paragraphs.Add(new Paragraph
                    {
                        StartTime = new TimeCode(x.StartTime / 90.0),
                        EndTime = new TimeCode(x.EndTime / 90.0)
                    });
                }
            }

            _subtitle.Renumber();

            FixShortDisplayTimes(_subtitle);

            subtitleListView1.Fill(_subtitle);
            subtitleListView1.SelectIndexAndEnsureVisible(0);

            numericUpDownStartNumber.Maximum = max;
            if (numericUpDownStartNumber.Maximum > 0 && numericUpDownStartNumber.Minimum <= 1)
            {
                numericUpDownStartNumber.Value = 1;
            }

            SetButtonsEnabledAfterOcrDone();
            buttonStartOcr.Focus();
        }

        private void LoadVobRip()
        {
            _subtitle = new Subtitle();
            _vobSubMergedPackList = new List<VobSubMergedPack>();
            int max = _vobSubMergedPackListOriginal.Count;
            for (int i = 0; i < max; i++)
            {
                var x = _vobSubMergedPackListOriginal[i];
                if (checkBoxShowOnlyForced.Checked && x.SubPicture.Forced ||
                    checkBoxShowOnlyForced.Checked == false)
                {
                    _vobSubMergedPackList.Add(x);
                    Paragraph p = new Paragraph(string.Empty, x.StartTime.TotalMilliseconds, x.EndTime.TotalMilliseconds);
                    if (checkBoxUseTimeCodesFromIdx.Checked && x.IdxLine != null)
                    {
                        double durationMilliseconds = p.Duration.TotalMilliseconds;
                        p.StartTime = new TimeCode(x.IdxLine.StartTime.TotalMilliseconds);
                        p.EndTime = new TimeCode(x.IdxLine.StartTime.TotalMilliseconds + durationMilliseconds);
                    }

                    _subtitle.Paragraphs.Add(p);
                }
            }

            _subtitle.Renumber();

            FixShortDisplayTimes(_subtitle);

            subtitleListView1.Fill(_subtitle);
            subtitleListView1.SelectIndexAndEnsureVisible(0);

            numericUpDownStartNumber.Maximum = max;
            if (numericUpDownStartNumber.Maximum > 0 && numericUpDownStartNumber.Minimum <= 1)
            {
                numericUpDownStartNumber.Value = 1;
            }

            SetButtonsEnabledAfterOcrDone();
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
                    {
                        p.EndTime.TotalMilliseconds = newEndTime;
                    }
                    else
                    {
                        p.EndTime.TotalMilliseconds = next.StartTime.TotalMilliseconds - 1;
                    }
                }
            }
        }

        public bool GetIsForced(int index)
        {
            if (_mp4List != null)
            {
                return _mp4List[index].Picture.Forced;
            }

            if (_spList != null)
            {
                return _spList[index].Picture.Forced;
            }

            if (_bdnXmlSubtitle != null)
            {
                return false;
            }

            if (_xSubList != null)
            {
                return false;
            }

            if (_dvbSubtitles != null)
            {
                //                return _dvbSubtitles[index]. ??
                return false;
            }

            if (_dvbPesSubtitles != null)
            {
                return false;
            }

            if (_bluRaySubtitlesOriginal != null)
            {
                return _bluRaySubtitles[index].IsForced;
            }

            if (_vobSubMergedPackList != null && checkBoxCustomFourColors.Checked)
            {
                return _vobSubMergedPackList[index].SubPicture.Forced;
            }

            if (_vobSubMergedPackList != null && index < _vobSubMergedPackList.Count)
            {
                return _vobSubMergedPackList[index].SubPicture.Forced;
            }

            return false;
        }

        public Bitmap GetSubtitleBitmap(int index, bool crop = true)
        {
            Bitmap returnBmp = null;
            Color background;
            Color pattern;
            Color emphasis1;
            Color emphasis2;

            if (_mp4List != null)
            {
                if (index >= 0 && index < _mp4List.Count)
                {
                    if (checkBoxCustomFourColors.Checked)
                    {
                        GetCustomColors(out background, out pattern, out emphasis1, out emphasis2);

                        returnBmp = _mp4List[index].Picture.GetBitmap(null, background, pattern, emphasis1, emphasis2, true);
                        if (autoTransparentBackgroundToolStripMenuItem.Checked)
                        {
                            returnBmp.MakeTransparent();
                        }
                    }
                    else
                    {
                        returnBmp = _mp4List[index].Picture.GetBitmap(null, Color.Transparent, Color.Black, Color.White, Color.Black, false);
                        if (autoTransparentBackgroundToolStripMenuItem.Checked)
                        {
                            returnBmp.MakeTransparent();
                        }
                    }
                }
            }
            else if (_spList != null)
            {
                if (index >= 0 && index < _spList.Count)
                {
                    if (checkBoxCustomFourColors.Checked)
                    {
                        GetCustomColors(out background, out pattern, out emphasis1, out emphasis2);

                        returnBmp = _spList[index].Picture.GetBitmap(null, background, pattern, emphasis1, emphasis2, true);
                        if (autoTransparentBackgroundToolStripMenuItem.Checked)
                        {
                            returnBmp.MakeTransparent();
                        }
                    }
                    else
                    {
                        returnBmp = _spList[index].Picture.GetBitmap(null, Color.Transparent, Color.Black, Color.White, Color.Black, false);
                        if (autoTransparentBackgroundToolStripMenuItem.Checked)
                        {
                            returnBmp.MakeTransparent();
                        }
                    }
                }
            }
            else if (_bdnXmlSubtitle != null)
            {
                if (index >= 0 && index < _bdnXmlSubtitle.Paragraphs.Count)
                {
                    var fileNames = _bdnXmlSubtitle.Paragraphs[index].Text.SplitToLines();
                    var bitmaps = new List<Bitmap>();
                    int maxWidth = 0;
                    int totalHeight = 0;

                    string fullFileName = string.Empty;
                    if (!string.IsNullOrEmpty(_bdnXmlSubtitle.Paragraphs[index].Extra))
                    {
                        fullFileName = Path.Combine(Path.GetDirectoryName(_bdnFileName), _bdnXmlSubtitle.Paragraphs[index].Extra.Replace("file://", string.Empty));
                    }

                    if (File.Exists(fullFileName))
                    {
                        try
                        {
                            returnBmp = new Bitmap(fullFileName);
                            if (autoTransparentBackgroundToolStripMenuItem.Checked)
                            {
                                returnBmp.MakeTransparent();
                            }
                        }
                        catch
                        {
                            // ignore
                        }
                    }
                    else
                    {

                        foreach (string fn in fileNames)
                        {
                            fullFileName = Path.Combine(Path.GetDirectoryName(_bdnFileName), fn);
                            if (!File.Exists(fullFileName))
                            {
                                // fix AVISubDetector lines
                                int idxOfIEquals = fn.IndexOf("i=", StringComparison.OrdinalIgnoreCase);
                                if (idxOfIEquals >= 0)
                                {
                                    int idxOfSpace = fn.IndexOf(' ', idxOfIEquals);
                                    if (idxOfSpace > 0)
                                    {
                                        fullFileName = Path.Combine(Path.GetDirectoryName(_bdnFileName), fn.Remove(0, idxOfSpace).Trim());
                                    }
                                }
                            }

                            if (File.Exists(fullFileName))
                            {
                                try
                                {
                                    var temp = new Bitmap(fullFileName);
                                    if (temp.Width > maxWidth)
                                    {
                                        maxWidth = temp.Width;
                                    }

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
                                if (autoTransparentBackgroundToolStripMenuItem.Checked)
                                {
                                    part.MakeTransparent();
                                }

                                using (var g = Graphics.FromImage(merged))
                                {
                                    g.DrawImage(part, 0, y);
                                }

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
                                        {
                                            fbmp.SetPixel(x, y, emphasis2);
                                        }
                                        else if (c.R == Color.Blue.R && c.G == Color.Blue.G && c.B == Color.Blue.B) // normally text?
                                        {
                                            fbmp.SetPixel(x, y, pattern);
                                        }
                                        else if (c.R == Color.White.R && c.G == Color.White.G && c.B == Color.White.B) // normally background
                                        {
                                            fbmp.SetPixel(x, y, background);
                                        }
                                        else if (c.R == Color.Black.R && c.G == Color.Black.G && c.B == Color.Black.B) // outline/border
                                        {
                                            fbmp.SetPixel(x, y, emphasis1);
                                        }
                                        else
                                        {
                                            fbmp.SetPixel(x, y, c);
                                        }
                                    }
                                }

                                fbmp.UnlockImage();
                            }

                            if (autoTransparentBackgroundToolStripMenuItem.Checked)
                            {
                                b.MakeTransparent();
                            }

                            returnBmp = b;
                        }
                    }
                }
            }
            else if (_xSubList != null)
            {
                if (index >= 0 && index < _xSubList.Count)
                {
                    if (checkBoxCustomFourColors.Checked)
                    {
                        GetCustomColors(out background, out pattern, out emphasis1, out emphasis2);
                        returnBmp = _xSubList[index].GetImage(background, pattern, emphasis1, emphasis2);
                    }
                    else
                    {
                        returnBmp = _xSubList[index].GetImage();
                    }
                }
            }
            else if (_dvbSubtitles != null)
            {
                if (index >= 0 && index < _dvbSubtitles.Count)
                {
                    var dvbBmp = _dvbSubtitles[index].GetBitmap();
                    var nDvbBmp = new NikseBitmap(dvbBmp);
                    nDvbBmp.CropTopTransparent(2);
                    nDvbBmp.CropTransparentSidesAndBottom(2, true);
                    if (_transportStreamUseColor)
                    {
                        _dvbSubColor[index] = nDvbBmp.GetBrightestColorWhiteIsTransparent();
                    }

                    if (autoTransparentBackgroundToolStripMenuItem.Checked)
                    {
                        nDvbBmp.MakeBackgroundTransparent((int)numericUpDownAutoTransparentAlphaMax.Value);
                    }

                    if (checkBoxTransportStreamGrayscale.Checked)
                    {
                        nDvbBmp.GrayScale();
                    }

                    dvbBmp.Dispose();
                    returnBmp = nDvbBmp.GetBitmap();
                }
            }
            else if (_dvbPesSubtitles != null)
            {
                if (index >= 0 && index < _dvbPesSubtitles.Count)
                {
                    var dvbBmp = _dvbPesSubtitles[index].GetImageFull();
                    var nDvbBmp = new NikseBitmap(dvbBmp);
                    nDvbBmp.CropTopTransparent(2);
                    nDvbBmp.CropTransparentSidesAndBottom(2, true);
                    if (_transportStreamUseColor)
                    {
                        _dvbSubColor[index] = nDvbBmp.GetBrightestColorWhiteIsTransparent();
                    }

                    if (autoTransparentBackgroundToolStripMenuItem.Checked)
                    {
                        nDvbBmp.MakeBackgroundTransparent((int)numericUpDownAutoTransparentAlphaMax.Value);
                    }

                    if (checkBoxTransportStreamGrayscale.Checked)
                    {
                        nDvbBmp.GrayScale();
                    }

                    dvbBmp.Dispose();
                    returnBmp = nDvbBmp.GetBitmap();
                }
            }
            else if (_bluRaySubtitlesOriginal != null)
            {
                if (_bluRaySubtitles != null)
                {
                    if (index >= 0 && index < _bluRaySubtitles.Count)
                    {
                        returnBmp = _bluRaySubtitles[index].GetBitmap();
                    }
                }
                else
                {
                    if (index >= 0 && index < _bluRaySubtitlesOriginal.Count)
                    {
                        returnBmp = _bluRaySubtitlesOriginal[index].GetBitmap();
                    }
                }
            }
            else if (index >= 0 && index < _vobSubMergedPackList.Count)
            {
                if (checkBoxCustomFourColors.Checked)
                {
                    GetCustomColors(out background, out pattern, out emphasis1, out emphasis2);

                    returnBmp = _vobSubMergedPackList[index].SubPicture.GetBitmap(null, background, pattern, emphasis1, emphasis2, true);
                    if (autoTransparentBackgroundToolStripMenuItem.Checked)
                    {
                        returnBmp.MakeTransparent();
                    }
                }
                else
                {
                    returnBmp = _vobSubMergedPackList[index].SubPicture.GetBitmap(_palette, Color.Transparent, Color.Black, Color.White, Color.Black, false, crop);
                    if (autoTransparentBackgroundToolStripMenuItem.Checked)
                    {
                        returnBmp.MakeTransparent();
                    }
                }
            }

            if (returnBmp == null)
            {
                return null;
            }

            if (_ocrMethodIndex == _ocrMethodTesseract4 && !_fromMenuItem)
            {
                var nb = new NikseBitmap(returnBmp);

                if (_preprocessingSettings != null && _preprocessingSettings.CropTransparentColors)
                {
                    nb.CropSidesAndBottom(2, Color.FromArgb(0, 0, 0, 0), true);
                    nb.CropTop(2, Color.FromArgb(0, 0, 0, 0));
                    nb.CropSidesAndBottom(2, Color.Transparent, true);
                    nb.CropTop(2, Color.Transparent);
                }

                nb.AddMargin(10);

                if (_preprocessingSettings != null && _preprocessingSettings.InvertColors)
                {
                    nb.InvertColors();
                }

                if (_preprocessingSettings != null && _preprocessingSettings.ScalingPercent > 100)
                {
                    var bTemp = nb.GetBitmap();
                    var f = _preprocessingSettings.ScalingPercent / 100.0;
                    var b = ResizeBitmap(bTemp, (int)Math.Round(bTemp.Width * f), (int)Math.Round(bTemp.Height * f));
                    bTemp.Dispose();
                    nb = new NikseBitmap(b);
                }

                if (_preprocessingSettings != null && _preprocessingSettings.InvertColors)
                {
                    nb.MakeTwoColor(_preprocessingSettings?.BinaryImageCompareThreshold ?? Configuration.Settings.Tools.OcrTesseract4RgbThreshold, Color.Black, Color.White);
                }
                else
                {
                    nb.MakeTwoColor(_preprocessingSettings?.BinaryImageCompareThreshold ?? Configuration.Settings.Tools.OcrTesseract4RgbThreshold, Color.White, Color.Black);
                }

                returnBmp.Dispose();
                return nb.GetBitmap();
            }

            if (_binaryOcrDb == null && _nOcrDb == null || _fromMenuItem)
            {
                if (_preprocessingSettings == null || !_preprocessingSettings.Active)
                {
                    return returnBmp;
                }

                var nb = new NikseBitmap(returnBmp);
                nb.CropSidesAndBottom(2, Color.FromArgb(0, 0, 0, 0), true);
                nb.CropTop(2, Color.FromArgb(0, 0, 0, 0));
                nb.CropSidesAndBottom(2, Color.Transparent, true);
                nb.CropTop(2, Color.Transparent);
                if (_preprocessingSettings.InvertColors)
                {
                    nb.InvertColors();
                }

                if (_preprocessingSettings.YellowToWhite)
                {
                    nb.ReplaceYellowWithWhite();
                }

                if (_preprocessingSettings.ColorToWhite != Color.Transparent)
                {
                    nb.ReplaceColor(_preprocessingSettings.ColorToWhite.A, _preprocessingSettings.ColorToWhite.R, _preprocessingSettings.ColorToWhite.G, _preprocessingSettings.ColorToWhite.B, 255, 255, 255, 255);
                }

                if (_preprocessingSettings.ColorToRemove.A > 0)
                {
                    nb.ReplaceColor(_preprocessingSettings.ColorToRemove.A, _preprocessingSettings.ColorToRemove.R, _preprocessingSettings.ColorToRemove.G, _preprocessingSettings.ColorToRemove.B, Color.Transparent.A, Color.Transparent.R, Color.Transparent.G, Color.Transparent.B);
                }

                returnBmp.Dispose();
                return nb.GetBitmap();
            }

            var n = new NikseBitmap(returnBmp);
            n.CropSidesAndBottom(2, Color.FromArgb(0, 0, 0, 0), true);
            n.CropTop(2, Color.FromArgb(0, 0, 0, 0));
            n.CropSidesAndBottom(2, Color.Transparent, true);
            n.CropTop(2, Color.Transparent);
            if (_preprocessingSettings != null && _preprocessingSettings.Active)
            {
                if (_preprocessingSettings.InvertColors)
                {
                    n.InvertColors();
                }

                if (_preprocessingSettings.YellowToWhite)
                {
                    n.ReplaceYellowWithWhite();
                }

                if (_preprocessingSettings.ColorToWhite != Color.Transparent)
                {
                    n.ReplaceColor(_preprocessingSettings.ColorToWhite.A, _preprocessingSettings.ColorToWhite.R, _preprocessingSettings.ColorToWhite.G, _preprocessingSettings.ColorToWhite.B, 255, 255, 255, 255);
                }

                if (_preprocessingSettings.ColorToRemove.A > 0)
                {
                    n.ReplaceColor(_preprocessingSettings.ColorToRemove.A, _preprocessingSettings.ColorToRemove.R, _preprocessingSettings.ColorToRemove.G, _preprocessingSettings.ColorToRemove.B, Color.Transparent.A, Color.Transparent.R, Color.Transparent.G, Color.Transparent.B);
                }

                if (_preprocessingSettings.ScalingPercent > 100)
                {
                    var bTemp = n.GetBitmap();
                    var f = _preprocessingSettings.ScalingPercent / 100.0;
                    var b = ResizeBitmap(bTemp, (int)Math.Round(bTemp.Width * f), (int)Math.Round(bTemp.Height * f));
                    bTemp.Dispose();
                    n = new NikseBitmap(b);
                }
            }

            n.MakeTwoColor(_preprocessingSettings?.BinaryImageCompareThreshold ?? Configuration.Settings.Tools.OcrBinaryImageCompareRgbThreshold);
            returnBmp.Dispose();
            return n.GetBitmap();
        }

        private void GetCustomColors(out Color background, out Color pattern, out Color emphasis1, out Color emphasis2)
        {
            background = pictureBoxBackground.BackColor;
            pattern = pictureBoxPattern.BackColor;
            emphasis1 = pictureBoxEmphasis1.BackColor;
            emphasis2 = pictureBoxEmphasis2.BackColor;

            if (checkBoxBackgroundTransparent.Checked)
            {
                background = Color.Transparent;
            }

            if (checkBoxPatternTransparent.Checked)
            {
                pattern = Color.Transparent;
            }

            if (checkBoxEmphasis1Transparent.Checked)
            {
                emphasis1 = Color.Transparent;
            }

            if (checkBoxEmphasis2Transparent.Checked)
            {
                emphasis2 = Color.Transparent;
            }
        }

        private void GetSubtitleTime(int index, out TimeCode start, out TimeCode end)
        {
            if (_mp4List != null)
            {
                var item = _mp4List[index];
                start = new TimeCode(item.Start.TotalMilliseconds);
                end = new TimeCode(item.End.TotalMilliseconds);
            }
            else if (_spList != null)
            {
                var item = _spList[index];
                start = new TimeCode(item.StartTime.TotalMilliseconds);
                end = new TimeCode(item.StartTime.TotalMilliseconds + item.Picture.Delay.TotalMilliseconds);
            }
            else if (_bdnXmlSubtitle != null)
            {
                var item = _bdnXmlSubtitle.Paragraphs[index];
                start = new TimeCode(item.StartTime.TotalMilliseconds);
                end = new TimeCode(item.EndTime.TotalMilliseconds);
            }
            else if (_bluRaySubtitlesOriginal != null)
            {
                var item = _bluRaySubtitles[index];
                start = new TimeCode(item.StartTime / 90.0);
                end = new TimeCode(item.EndTime / 90.0);
            }
            else if (_xSubList != null)
            {
                var item = _xSubList[index];
                start = new TimeCode(item.Start.TotalMilliseconds);
                end = new TimeCode(item.End.TotalMilliseconds);
            }
            else if (_dvbSubtitles != null)
            {
                var item = _dvbSubtitles[index];
                start = new TimeCode(item.StartMilliseconds);
                end = new TimeCode(item.EndMilliseconds);
            }
            else if (_dvbPesSubtitles != null)
            {
                var item = _subtitle.Paragraphs[index];
                start = item.StartTime;
                end = item.EndTime;
            }
            else
            {
                var item = _vobSubMergedPackList[index];
                start = new TimeCode(item.StartTime.TotalMilliseconds);
                end = new TimeCode(item.EndTime.TotalMilliseconds);
            }
        }

        private int GetSubtitleCount()
        {
            if (_mp4List != null)
            {
                return _mp4List.Count;
            }

            if (_spList != null)
            {
                return _spList.Count;
            }

            if (_bdnXmlSubtitle != null)
            {
                return _bdnXmlSubtitle.Paragraphs.Count;
            }

            if (_bluRaySubtitlesOriginal != null)
            {
                return _bluRaySubtitles?.Count ?? _bluRaySubtitlesOriginal.Count;
            }

            if (_xSubList != null)
            {
                return _xSubList.Count;
            }

            if (_dvbSubtitles != null)
            {
                return _dvbSubtitles.Count;
            }

            if (_dvbPesSubtitles != null)
            {
                return _dvbPesSubtitles.Count;
            }

            return _vobSubMergedPackList.Count;
        }

        /// <summary>
        /// Get top position of sub + sub height
        /// </summary>
        private void GetSubtitleTopAndHeight(int index, out int left, out int top, out int width, out int height)
        {
            if (_mp4List != null)
            {
                left = 0;
                top = 0;
                width = 0;
                height = 0;
                return;
            }

            if (_spList != null)
            {
                var item = _spList[index];
                left = item.Picture.ImageDisplayArea.Left;
                top = item.Picture.ImageDisplayArea.Top;
                width = item.Picture.ImageDisplayArea.Width;
                height = item.Picture.ImageDisplayArea.Bottom;
                return;
            }

            if (_bdnXmlSubtitle != null)
            {
                var p = _subtitle.GetParagraphOrDefault(index);
                if (p != null && p.Extra != null)
                {
                    var parts = p.Extra.Split(',');
                    if (parts.Length == 2)
                    {
                        left = int.Parse(parts[0]);
                        top = int.Parse(parts[1]);
                        var bmp = GetSubtitleBitmap(index, false);
                        width = bmp.Width;
                        height = bmp.Height;
                        bmp.Dispose();
                        return;
                    }
                }

                left = 0;
                top = 0;
                width = 0;
                height = 0;
                return;
            }

            if (_bluRaySubtitlesOriginal != null)
            {
                var item = _bluRaySubtitles[index];
                var bmp = item.GetBitmap();
                height = bmp.Height;
                width = bmp.Width;
                bmp.Dispose();
                left = item.PcsObjects.Min(p => p.Origin.X);
                top = item.PcsObjects.Max(p => p.Origin.Y);
                return;
            }

            if (_xSubList != null)
            {
                left = 0;
                top = 0;
                width = 0;
                height = 0;
                return;
            }

            if (_dvbSubtitles != null)
            {
                var item = _dvbSubtitles[index];
                var pos = item.GetPosition();
                var bmp = item.GetBitmap();
                var nbmp = new NikseBitmap(bmp);
                top = pos.Top + nbmp.CropTopTransparent(0);
                left = pos.Left + nbmp.CropSidesAndBottom(0, Color.FromArgb(0, 0, 0, 0), true);
                width = nbmp.Width;
                height = nbmp.Height;
                bmp.Dispose();
                return;
            }

            if (_dvbPesSubtitles != null)
            {
                var item = _subtitle.Paragraphs[index];
                //TODO
                left = 0;
                top = 0;
                width = 0;
                height = 0;
                return;
            }

            if (_vobSubMergedPackList != null)
            {
                var item = _vobSubMergedPackList[index];
                left = item.SubPicture.ImageDisplayArea.Left;
                top = item.SubPicture.ImageDisplayArea.Top;
                var bmp = item.SubPicture.GetBitmap(_palette, Color.Transparent, Color.Black, Color.White, Color.Black, false);
                width = bmp.Width;
                height = bmp.Height;
                bmp.Dispose();
                return;
            }

            left = 0;
            top = 0;
            width = 0;
            height = 0;
        }

        private void GetSubtitleScreenSize(int index, out int width, out int height)
        {
            width = 0;
            height = 0;

            if (_spList != null)
            {
                var item = _spList[index];
                width = item.Picture.ImageDisplayArea.Width;
                height = item.Picture.ImageDisplayArea.Bottom;
                return;
            }

            if (_bdnXmlSubtitle != null && File.Exists(_bdnFileName))
            {
                width = 0;
                height = 0;
                try
                {
                    if (_bdnXmlDocument == null)
                    {
                        _bdnXmlDocument = new XmlDocument { XmlResolver = null };
                        _bdnXmlDocument.Load(_bdnFileName);
                    }

                    var formatNode = _bdnXmlDocument.DocumentElement.SelectSingleNode("Description/Format");
                    var videoFormat = formatNode?.Attributes["VideoFormat"].InnerText;
                    if (videoFormat == "480i" || videoFormat == "480p")
                    {
                        width = 720; // not certain
                        height = 480;
                    }
                    else if (videoFormat == "576i" || videoFormat == "576p")
                    {
                        width = 720; // not certain
                        height = 576;
                    }
                    else if (videoFormat == "720i" || videoFormat == "720p")
                    {
                        width = 1280;
                        height = 720;
                    }
                    else if (videoFormat == "1080i" || videoFormat == "1080p")
                    {
                        width = 1920;
                        height = 1080;
                    }
                    else if (videoFormat == "2160i" || videoFormat == "2160p")
                    {
                        width = 3840;
                        height = 2160;
                    }
                    else if (videoFormat == "4320i" || videoFormat == "4320p")
                    {
                        width = 7680;
                        height = 4320;
                    }
                    else if (videoFormat.Contains("x"))
                    {
                        var parts = videoFormat.Split('x');
                        if (parts.Length == 2)
                        {
                            width = int.Parse(parts[0]);
                            height = int.Parse(parts[1]);
                        }
                    }
                }
                catch
                {
                    width = 0;
                    height = 0;
                }

                return;
            }

            if (_bluRaySubtitlesOriginal != null)
            {
                var item = _bluRaySubtitles[index];
                height = item.Size.Height;
                width = item.Size.Width;
            }

            if (_dvbPesSubtitles != null)
            {
                var size = _dvbPesSubtitles[index].GetScreenSize();
                width = size.Width;
                height = size.Height;
            }

            if (_dvbSubtitles != null)
            {
                width = 720;
                height = 576;
            }

            if (_vobSubMergedPackList != null && index < _vobSubMergedPackList.Count)
            {
                var item = _vobSubMergedPackList[index];
                width = item.SubPicture.ImageDisplayArea.Width + +item.SubPicture.ImageDisplayArea.Location.X + 1;
                height = item.SubPicture.ImageDisplayArea.Height + item.SubPicture.ImageDisplayArea.Location.Y + 1;
            }
        }

        private Bitmap ShowSubtitleImage(int index)
        {
            int numberOfImages = GetSubtitleCount();
            Bitmap bmp;
            if (index < numberOfImages)
            {
                bmp = GetSubtitleBitmap(index);
                if (bmp == null)
                {
                    bmp = new Bitmap(1, 1);
                }

                groupBoxSubtitleImage.Text = string.Format(LanguageSettings.Current.VobSubOcr.SubtitleImageXofY, index + 1, numberOfImages) + "   " + bmp.Width + "x" + bmp.Height;
            }
            else
            {
                groupBoxSubtitleImage.Text = LanguageSettings.Current.VobSubOcr.SubtitleImage;
                bmp = new Bitmap(1, 1);
            }

            var old = pictureBoxSubtitleImage.Image as Bitmap;
            pictureBoxSubtitleImage.Image = bmp.Clone() as Bitmap;
            pictureBoxSubtitleImage.Invalidate();
            old?.Dispose();
            return bmp;
        }

        private void ShowSubtitleImage(int index, Bitmap bmp)
        {
            try
            {
                int numberOfImages = GetSubtitleCount();
                if (index < numberOfImages)
                {
                    groupBoxSubtitleImage.Text = string.Format(LanguageSettings.Current.VobSubOcr.SubtitleImageXofY, index + 1, numberOfImages) + "   " + bmp.Width + "x" + bmp.Height;
                }
                else
                {
                    groupBoxSubtitleImage.Text = LanguageSettings.Current.VobSubOcr.SubtitleImage;
                }

                Bitmap old = pictureBoxSubtitleImage.Image as Bitmap;
                pictureBoxSubtitleImage.Image = bmp.Clone() as Bitmap;
                pictureBoxSubtitleImage.Invalidate();
                old?.Dispose();
            }
            catch
            {
                // can crash if user is clicking around...
            }
        }

        private static Point MakePointItalic(Point p, int height, int moveLeftPixels, double unItalicFactor)
        {
            //TODO: TEst+fix + move to NOcrChar
            return new Point((int)Math.Round(p.X + (height - p.Y) * unItalicFactor - moveLeftPixels), p.Y);
        }

        private static NOcrChar NOcrFindBestMatch(ImageSplitterItem targetItem, int topMargin, out bool italic, NOcrChar[] nOcrChars, double unItalicFactor, bool tryItalicScaling, bool deepSeek)
        {
            italic = false;
            var nbmp = targetItem.NikseBitmap;
            int index;
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
                                if (c.A > 150 && c.R + c.G + c.B > NOcrMinColor)
                                {
                                }
                                else
                                {
                                    Point p = new Point(point.X - 1, point.Y);
                                    if (p.X < 0)
                                    {
                                        p.X = 1;
                                    }

                                    c = nbmp.GetPixel(p.X, p.Y);
                                    if (nbmp.Width > 20 && c.A > 150 && c.R + c.G + c.B > NOcrMinColor)
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
                                if (c.A > 150 && c.R + c.G + c.B > NOcrMinColor)
                                {
                                    Point p = new Point(point.X, point.Y);
                                    if (oc.Width > 19 && point.X > 0)
                                    {
                                        p.X = p.X - 1;
                                    }

                                    c = nbmp.GetPixel(p.X, p.Y);
                                    if (c.A > 150 && c.R + c.G + c.B > NOcrMinColor)
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
                    {
                        return oc;
                    }
                }
            }

            foreach (NOcrChar oc in nOcrChars)
            {
                int marginTopDiff = Math.Abs(oc.MarginTop - topMargin);
                if (Math.Abs(oc.Width - nbmp.Width) < 5 && Math.Abs(oc.Height - nbmp.Height) < 5 && marginTopDiff < 9)
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
                                if (c.A > 150 && c.R + c.G + c.B > NOcrMinColor)
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
                                if (c.A > 150 && c.R + c.G + c.B > NOcrMinColor)
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
                        return oc;
                    }
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
                                    if (c.A > 150 && c.R + c.G + c.B > NOcrMinColor)
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
                                    if (c.A > 150 && c.R + c.G + c.B > NOcrMinColor)
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
                            return oc;
                        }
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
                                    if (c.A > 150 && c.R + c.G + c.B > NOcrMinColor)
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
                                    if (c.A > 150 && c.R + c.G + c.B > NOcrMinColor)
                                    {
                                        ok = false;
                                        break;
                                    }
                                }
                            }
                        }

                        if (ok)
                        {
                            return oc;
                        }
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
                                    if (c.A > 150 && c.R + c.G + c.B > NOcrMinColor)
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
                                    if (c.A > 150 && c.R + c.G + c.B > NOcrMinColor)
                                    {
                                        ok = false;
                                        break;
                                    }
                                }
                            }
                        }

                        if (ok)
                        {
                            return oc;
                        }
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
                                    if (c.A > 150 && c.R + c.G + c.B > NOcrMinColor)
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
                                    if (c.A > 150 && c.R + c.G + c.B > NOcrMinColor)
                                    {
                                        ok = false;
                                        break;
                                    }
                                }
                            }
                        }

                        if (ok)
                        {
                            return oc;
                        }
                    }
                }
            }

            if (tryItalicScaling)
            {
                int maxMoveLeft = 9;
                if (nbmp.Width < 20)
                {
                    maxMoveLeft = 7;
                }

                if (nbmp.Width < 16)
                {
                    maxMoveLeft = 4;
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
                                foreach (Point p in op.ScaledGetPoints(o, nbmp.Width, nbmp.Height))
                                {
                                    if (p.X >= 2 && p.Y >= 2 && p.X < nbmp.Width && p.Y < nbmp.Height)
                                    {
                                        Color c = nbmp.GetPixel(p.X, p.Y);
                                        if (c.A > 150 && c.R + c.G + c.B > NOcrMinColor)
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
                                        if (c.A > 150 && c.R + c.G + c.B > NOcrMinColor)
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
                                        if (c.A > 150 && c.R + c.G + c.B > NOcrMinColor)
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
                                foreach (Point p in op.ScaledGetPoints(o, nbmp.Width - 4, nbmp.Height))
                                {
                                    if (p.X >= 0 && p.Y >= 0 && p.X < nbmp.Width && p.Y < nbmp.Height)
                                    {
                                        Color c = nbmp.GetPixel(p.X, p.Y);
                                        if (c.A > 150 && c.R + c.G + c.B > NOcrMinColor)
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
                                        if (c.A > 150 && c.R + c.G + c.B > NOcrMinColor)
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
                                        if (c.A > 150 && c.R + c.G + c.B > NOcrMinColor)
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

        private static NOcrChar NOcrFindBestMatchNew(ImageSplitterItem targetItem, NOcrDb nOcrDb, bool deepSeek, int maxWrongPixels)
        {
            return nOcrDb?.GetMatch(targetItem.NikseBitmap, targetItem.Top, deepSeek, maxWrongPixels);
        }

        private static NOcrChar MakeItalicNOcrChar(NOcrChar oldChar, int movePixelsLeft, double unItalicFactor)
        {
            var c = new NOcrChar();
            foreach (var op in oldChar.LinesForeground)
            {
                c.LinesForeground.Add(new NOcrPoint(MakePointItalic(op.Start, oldChar.Height, movePixelsLeft, unItalicFactor), MakePointItalic(op.End, oldChar.Height, movePixelsLeft, unItalicFactor)));
            }

            foreach (var op in oldChar.LinesBackground)
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

        private static readonly HashSet<string> UppercaseLikeLowercase = new HashSet<string> { "V", "W", "U", "S", "Z", "O", "X", "Ø", "C" };
        private static readonly HashSet<string> LowercaseLikeUppercase = new HashSet<string> { "v", "w", "u", "s", "z", "o", "x", "ø", "c" };
        private static readonly HashSet<string> UppercaseWithAccent = new HashSet<string> { "Č", "Š", "Ž", "Ś", "Ż", "Ś", "Ö", "Ü", "Ú", "Ï", "Í", "Ç", "Ì", "Ò", "Ù", "Ó", "Í" };
        private static readonly HashSet<string> LowercaseWithAccent = new HashSet<string> { "č", "š", "ž", "ś", "ż", "ś", "ö", "ü", "ú", "ï", "í", "ç", "ì", "ò", "ù", "ó", "í" };

        /// <summary>
        /// Fix uppercase/lowercase issues (not I/l)
        /// </summary>
        private string FixUppercaseLowercaseIssues(ImageSplitterItem targetItem, NOcrChar result)
        {
            if (result.Text == "e" || result.Text == "a" || result.Text == "d" || result.Text == "t")
            {
                _ocrLowercaseHeightsTotalCount++;
                _ocrLowercaseHeightsTotal += targetItem.NikseBitmap.Height;
                if (_ocrUppercaseHeightsTotalCount < 3)
                {
                    _ocrUppercaseHeightsTotalCount++;
                    _ocrUppercaseHeightsTotal += targetItem.NikseBitmap.Height + 10;
                }
            }

            if (result.Text == "E" || result.Text == "H" || result.Text == "R" || result.Text == "D" || result.Text == "T" || result.Text == "M")
            {
                _ocrUppercaseHeightsTotalCount++;
                _ocrUppercaseHeightsTotal += targetItem.NikseBitmap.Height;
                if (_ocrLowercaseHeightsTotalCount < 3 && targetItem.NikseBitmap.Height > 20)
                {
                    _ocrLowercaseHeightsTotalCount++;
                    _ocrLowercaseHeightsTotal += targetItem.NikseBitmap.Height - 10;
                }
            }

            if (_ocrLowercaseHeightsTotalCount <= 2 || _ocrUppercaseHeightsTotalCount <= 2)
            {
                return result.Text;
            }

            // Latin letters where lowercase versions look like uppercase version 
            if (UppercaseLikeLowercase.Contains(result.Text))
            {
                var averageLowercase = _ocrLowercaseHeightsTotal / _ocrLowercaseHeightsTotalCount;
                var averageUppercase = _ocrUppercaseHeightsTotal / _ocrUppercaseHeightsTotalCount;
                if (Math.Abs(averageLowercase - targetItem.NikseBitmap.Height) < Math.Abs(averageUppercase - targetItem.NikseBitmap.Height))
                {
                    return result.Text.ToLowerInvariant();
                }

                return result.Text;
            }

            if (LowercaseLikeUppercase.Contains(result.Text))
            {
                var averageLowercase = _ocrLowercaseHeightsTotal / _ocrLowercaseHeightsTotalCount;
                var averageUppercase = _ocrUppercaseHeightsTotal / _ocrUppercaseHeightsTotalCount;
                if (Math.Abs(averageLowercase - targetItem.NikseBitmap.Height) > Math.Abs(averageUppercase - targetItem.NikseBitmap.Height))
                {
                    return result.Text.ToUpperInvariant();
                }

                return result.Text;
            }

            if (UppercaseWithAccent.Contains(result.Text))
            {
                var averageUppercase = _ocrUppercaseHeightsTotal / (double)_ocrUppercaseHeightsTotalCount;
                if (targetItem.NikseBitmap.Height < averageUppercase + 3)
                {
                    return result.Text.ToLowerInvariant();
                }

                return result.Text;
            }

            if (LowercaseWithAccent.Contains(result.Text))
            {
                var averageUppercase = _ocrUppercaseHeightsTotal / (double)_ocrUppercaseHeightsTotalCount;
                if (targetItem.NikseBitmap.Height > averageUppercase + 4)
                {
                    return result.Text.ToUpperInvariant();
                }
            }

            return result.Text;
        }

        internal CompareMatch GetNOcrCompareMatchNew(ImageSplitterItem targetItem, NikseBitmap parentBitmap, NOcrDb nOcrDb, bool tryItalicScaling, bool deepSeek, int index, List<ImageSplitterItem> list)
        {
            deepSeek = true;
            var expandedResult = nOcrDb.GetMatchExpanded(parentBitmap, targetItem, index, list);
            if (expandedResult != null)
            {
                return new CompareMatch(expandedResult.Text, expandedResult.Italic, expandedResult.ExpandCount, null, expandedResult) { ImageSplitterItem = targetItem };
            }

            var result = NOcrFindBestMatchNew(targetItem, nOcrDb, deepSeek, (int)numericUpDownNOcrMaxWrongPixels.Value);
            if (result == null)
            {

                // try to make letter normal via un-italic angle
                if (tryItalicScaling && targetItem.NikseBitmap != null)
                {
                    var unItalicNikseBitmap = new NikseBitmap(targetItem.NikseBitmap);
                    unItalicNikseBitmap.ReplaceColor(255, 0, 0, 0, 0, 0, 0, 0);
                    unItalicNikseBitmap.MakeTwoColor(200);
                    var oldBmp = unItalicNikseBitmap.GetBitmap();
                    var unItalicImage = UnItalic(oldBmp, _unItalicFactor); //TODO: make un-italic in NikseBitmap
                    unItalicNikseBitmap = new NikseBitmap(unItalicImage);
                    unItalicNikseBitmap.CropTransparentSidesAndBottom(0, false);
                    oldBmp.Dispose();
                    unItalicImage.Dispose();
                    var unItalicTargetItem = new ImageSplitterItem(targetItem.X, targetItem.Y, unItalicNikseBitmap) { Top = targetItem.Top };
                    result = NOcrFindBestMatchNew(unItalicTargetItem, nOcrDb, deepSeek, (int)numericUpDownNOcrMaxWrongPixels.Value);
                    if (result != null)
                    {
                        result.Italic = true;
                        _italicFixes++;
                    }
                }

                if (result == null)
                {
                    if (checkBoxNOcrDrawUnknownLetters.Checked)
                    {
                        return null;
                    }

                    return new CompareMatch("*", false, 0, null);
                }
            }

            var text = FixUppercaseLowercaseIssues(targetItem, result);
            return new CompareMatch(text, result.Italic, 0, null, result) { Y = targetItem.Y };
        }

        public static int _italicFixes = 0;

        private CompareMatch GetCompareMatchNew(ImageSplitterItem targetItem, out CompareMatch secondBestGuess, List<ImageSplitterItem> list, int listIndex, BinaryOcrDb binaryOcrDb)
        {
            double maxDiff = _numericUpDownMaxErrorPct;
            secondBestGuess = null;
            int index = 0;
            int smallestDifference = 10000;
            var target = targetItem.NikseBitmap;
            if (binaryOcrDb == null)
            {
                return null;
            }

            var bob = new BinaryOcrBitmap(target) { X = targetItem.X, Y = targetItem.Top };

            // precise expanded match
            for (int k = 0; k < binaryOcrDb.CompareImagesExpanded.Count; k++)
            {
                var b = binaryOcrDb.CompareImagesExpanded[k];
                if (bob.Hash == b.Hash && bob.Width == b.Width && bob.Height == b.Height && bob.NumberOfColoredPixels == b.NumberOfColoredPixels)
                {
                    bool ok = false;
                    for (int i = 0; i < b.ExpandedList.Count; i++)
                    {
                        if (listIndex + i + 1 < list.Count && list[listIndex + i + 1].NikseBitmap != null)
                        {
                            var bobNext = new BinaryOcrBitmap(list[listIndex + i + 1].NikseBitmap);
                            if (b.ExpandedList[i].Hash == bobNext.Hash)
                            {
                                ok = true;
                            }
                            else
                            {
                                ok = false;
                                break;
                            }
                        }
                        else
                        {
                            ok = false;
                            break;
                        }
                    }

                    if (ok)
                    {
                        return new CompareMatch(b.Text, b.Italic, b.ExpandCount, b.Key);
                    }
                }
            }


            // allow for error %
            for (int k = 0; k < binaryOcrDb.CompareImagesExpanded.Count; k++)
            {
                var b = binaryOcrDb.CompareImagesExpanded[k];
                if (Math.Abs(bob.Width - b.Width) < 3 && Math.Abs(bob.Height - b.Height) < 3 && Math.Abs(bob.NumberOfColoredPixels - b.NumberOfColoredPixels) < 5 && GetPixelDifPercentage(b, bob, target, maxDiff) <= maxDiff)
                {
                    bool ok = false;
                    for (int i = 0; i < b.ExpandedList.Count; i++)
                    {
                        if (listIndex + i + 1 < list.Count && list[listIndex + i + 1].NikseBitmap != null)
                        {
                            var bobNext = new BinaryOcrBitmap(list[listIndex + i + 1].NikseBitmap);
                            if (b.ExpandedList[i].Hash == bobNext.Hash)
                            {
                                ok = true;
                            }
                            else if (Math.Abs(b.ExpandedList[i].Y - bobNext.Y) < 6 && GetPixelDifPercentage(b.ExpandedList[i], bobNext, list[listIndex + i + 1].NikseBitmap, maxDiff) <= maxDiff)
                            {
                                ok = true;
                            }
                            else
                            {
                                ok = false;
                                break;
                            }
                        }
                        else
                        {
                            ok = false;
                            break;
                        }
                    }

                    if (ok)
                    {
                        return new CompareMatch(b.Text, b.Italic, b.ExpandCount, b.Key);
                    }
                }
            }

            FindBestMatchNew(ref index, ref smallestDifference, out var hit, target, binaryOcrDb, bob, maxDiff);
            if (maxDiff > 0)
            {
                if (target.Width > 16 && target.Height > 16 && (hit == null || smallestDifference * 100.0 / (target.Width * target.Height) > maxDiff))
                {
                    var t2 = target.CopyRectangle(new Rectangle(0, 1, target.Width, target.Height - 1));
                    FindBestMatchNew(ref index, ref smallestDifference, out hit, t2, binaryOcrDb, bob, maxDiff);
                }

                if (target.Width > 16 && target.Height > 16 && (hit == null || smallestDifference * 100.0 / (target.Width * target.Height) > maxDiff))
                {
                    var t2 = target.CopyRectangle(new Rectangle(1, 0, target.Width - 1, target.Height));
                    FindBestMatchNew(ref index, ref smallestDifference, out hit, t2, binaryOcrDb, bob, maxDiff);
                }

                if (target.Width > 16 && target.Height > 16 && (hit == null || smallestDifference * 100.0 / (target.Width * target.Height) > maxDiff))
                {
                    var t2 = target.CopyRectangle(new Rectangle(0, 0, target.Width - 1, target.Height));
                    FindBestMatchNew(ref index, ref smallestDifference, out hit, t2, binaryOcrDb, bob, maxDiff);
                }
            }

            if (hit != null)
            {
                double differencePercentage = smallestDifference * 100.0 / (target.Width * target.Height);
                if (differencePercentage <= maxDiff)
                {
                    string text = hit.Text;
                    if (smallestDifference > 0)
                    {
                        int h = hit.Height;
                        if (text == "V" || text == "W" || text == "U" || text == "S" || text == "Z" || text == "O" || text == "X" || text == "Ø" || text == "C")
                        {
                            if (_ocrLowercaseHeightsTotal > 10 && h - _ocrLowercaseHeightsTotal / _ocrLowercaseHeightsTotalCount < 2.0)
                            {
                                text = text.ToLowerInvariant();
                            }
                        }
                        else if (text == "v" || text == "w" || text == "u" || text == "s" || text == "z" || text == "o" || text == "x" || text == "ø" || text == "c")
                        {
                            if (_ocrUppercaseHeightsTotal > 10 && _ocrUppercaseHeightsTotal / _ocrUppercaseHeightsTotalCount - h < 2)
                            {
                                text = text.ToUpperInvariant();
                            }
                        }
                    }
                    else
                    {
                        SetBinOcrLowercaseUppercase(hit.Height, text);
                    }

                    if (differencePercentage > 0)
                    {
                        bool dummy;
                        if ((hit.Text == "l" || hit.Text == "!") && bob.IsLowercaseI(out dummy))
                        {
                            hit = null;
                        }
                        else if ((hit.Text == "i" || hit.Text == "!") && bob.IsLowercaseL())
                        {
                            hit = null;
                        }
                        else if ((hit.Text == "o" || hit.Text == "O") && bob.IsC())
                        {
                            return new CompareMatch(hit.Text == "o" ? "c" : "C", false, 0, null);
                        }
                        else if ((hit.Text == "c" || hit.Text == "C") && !bob.IsC() && bob.IsO())
                        {
                            return new CompareMatch(hit.Text == "c" ? "o" : "O", false, 0, null);
                        }
                    }

                    if (hit != null)
                    {
                        if (differencePercentage < 9 && (text == "e" || text == "d" || text == "a"))
                        {
                            _ocrLowercaseHeightsTotalCount++;
                            _ocrLowercaseHeightsTotal += bob.Height;
                        }

                        return new CompareMatch(text, hit.Italic, hit.ExpandCount, hit.Key);
                    }
                }

                if (hit != null)
                {
                    secondBestGuess = new CompareMatch(hit.Text, hit.Italic, hit.ExpandCount, hit.Key);
                }
            }

            if (maxDiff > 1 && _isLatinDb)
            {
                if (bob.IsPeriod())
                {
                    ImageSplitterItem next = null;
                    if (listIndex + 1 < list.Count)
                    {
                        next = list[listIndex + 1];
                    }

                    if (next?.NikseBitmap == null)
                    {
                        return new CompareMatch(".", false, 0, null);
                    }

                    var nextBob = new BinaryOcrBitmap(next.NikseBitmap) { X = next.X, Y = next.Top };
                    if (!nextBob.IsPeriodAtTop(GetLastBinOcrLowercaseHeight())) // avoid italic ":"
                    {
                        return new CompareMatch(".", false, 0, null);
                    }
                }

                if (bob.IsComma())
                {
                    ImageSplitterItem next = null;
                    if (listIndex + 1 < list.Count)
                    {
                        next = list[listIndex + 1];
                    }

                    if (next?.NikseBitmap == null)
                    {
                        return new CompareMatch(",", false, 0, null);
                    }

                    var nextBob = new BinaryOcrBitmap(next.NikseBitmap) { X = next.X, Y = next.Top };
                    if (!nextBob.IsPeriodAtTop(GetLastBinOcrLowercaseHeight())) // avoid italic ";"
                    {
                        return new CompareMatch(",", false, 0, null);
                    }
                }

                if (bob.IsApostrophe())
                {
                    return new CompareMatch("'", false, 0, null);
                }

                if (bob.IsLowercaseJ()) // "j" detection must be before "i"
                {
                    return new CompareMatch("j", false, 0, null);
                }

                if (bob.IsLowercaseI(out var italicLowercaseI))
                {
                    return new CompareMatch("i", italicLowercaseI, 0, null);
                }

                if (bob.IsColon())
                {
                    return new CompareMatch(":", false, 0, null);
                }

                if (bob.IsExclamationMark())
                {
                    return new CompareMatch("!", false, 0, null);
                }

                if (bob.IsDash())
                {
                    return new CompareMatch("-", false, 0, null);
                }
            }

            return null;
        }

        private static double GetPixelDifPercentage(BinaryOcrBitmap expanded, BinaryOcrBitmap bobNext, NikseBitmap nbmpNext, double maxDiff)
        {
            var difColoredPercentage = (Math.Abs(expanded.NumberOfColoredPixels - bobNext.NumberOfColoredPixels)) * 100.0 / (bobNext.Width * bobNext.Height);
            if (difColoredPercentage > 1 && expanded.Width < 3 || bobNext.Width < 3)
            {
                return 100;
            }

            int dif = int.MaxValue;
            if (expanded.Height == bobNext.Height && expanded.Width == bobNext.Width)
            {
                dif = NikseBitmapImageSplitter.IsBitmapsAlike(nbmpNext, expanded);
            }
            else if (maxDiff > 0)
            {
                if (expanded.Height == bobNext.Height && expanded.Width == bobNext.Width + 1)
                {
                    dif = NikseBitmapImageSplitter.IsBitmapsAlike(nbmpNext, expanded);
                }
                else if (expanded.Height == bobNext.Height && expanded.Width == bobNext.Width - 1)
                {
                    dif = NikseBitmapImageSplitter.IsBitmapsAlike(expanded, nbmpNext);
                }
                else if (expanded.Width == bobNext.Width && expanded.Height == bobNext.Height + 1)
                {
                    dif = NikseBitmapImageSplitter.IsBitmapsAlike(nbmpNext, expanded);
                }
                else if (expanded.Width == bobNext.Width && expanded.Height == bobNext.Height - 1)
                {
                    dif = NikseBitmapImageSplitter.IsBitmapsAlike(expanded, nbmpNext);
                }
            }

            var percentage = dif * 100.0 / (bobNext.Width * bobNext.Height);
            return percentage;
        }

        private static void FindBestMatchNew(ref int index, ref int smallestDifference, out BinaryOcrBitmap hit, NikseBitmap target, BinaryOcrDb binOcrDb, BinaryOcrBitmap bob, double maxDiff)
        {
            hit = null;
            var bobExactMatch = binOcrDb.FindExactMatch(bob);
            if (bobExactMatch >= 0)
            {
                var m = binOcrDb.CompareImages[bobExactMatch];
                index = bobExactMatch;
                smallestDifference = 0;
                hit = m;
                return;
            }

            var tWidth = target.Width;
            var tHeight = target.Height;
            if (maxDiff < 0.2 || tWidth < 3 || tHeight < 5)
            {
                return;
            }

            int numberOfForegroundColors = bob.NumberOfColoredPixels;
            const int minForeColorMatch = 90;

            foreach (var compareItem in binOcrDb.CompareImages)
            {
                if (compareItem.Width == tWidth && compareItem.Height == tHeight) // precise math in size
                {
                    if (Math.Abs(compareItem.NumberOfColoredPixels - numberOfForegroundColors) < 3)
                    {
                        int dif = NikseBitmapImageSplitter.IsBitmapsAlike(compareItem, target);
                        if (dif < smallestDifference)
                        {
                            if (!BinaryOcrDb.AllowEqual(compareItem, bob))
                            {
                                continue;
                            }

                            smallestDifference = dif;
                            hit = compareItem;
                            if (dif < 3)
                            {
                                break; // foreach ending
                            }
                        }
                    }
                }
            }

            if (smallestDifference > 1)
            {
                foreach (var compareItem in binOcrDb.CompareImages)
                {
                    if (compareItem.Width == tWidth && compareItem.Height == tHeight) // precise math in size
                    {
                        if (Math.Abs(compareItem.NumberOfColoredPixels - numberOfForegroundColors) < 40)
                        {
                            int dif = NikseBitmapImageSplitter.IsBitmapsAlike(compareItem, target);
                            if (dif < smallestDifference)
                            {
                                if (!BinaryOcrDb.AllowEqual(compareItem, bob))
                                {
                                    continue;
                                }

                                smallestDifference = dif;
                                hit = compareItem;
                                if (dif == 0)
                                {
                                    break; // foreach ending
                                }
                            }
                        }
                    }
                }
            }

            if (tWidth > 16 && tHeight > 16 && smallestDifference > 2) // for other than very narrow letter (like 'i' and 'l' and 'I'), try more sizes
            {
                foreach (var compareItem in binOcrDb.CompareImages)
                {
                    if (compareItem.Width == tWidth && compareItem.Height == tHeight - 1)
                    {
                        if (Math.Abs(compareItem.NumberOfColoredPixels - numberOfForegroundColors) < minForeColorMatch)
                        {
                            int dif = NikseBitmapImageSplitter.IsBitmapsAlike(compareItem, target);
                            if (dif < smallestDifference)
                            {
                                if (!BinaryOcrDb.AllowEqual(compareItem, bob))
                                {
                                    continue;
                                }

                                smallestDifference = dif;
                                hit = compareItem;
                                if (dif == 0)
                                {
                                    break; // foreach ending
                                }
                            }
                        }
                    }
                }

                if (smallestDifference > 2)
                {
                    foreach (var compareItem in binOcrDb.CompareImages)
                    {
                        if (compareItem.Width == tWidth && compareItem.Height == tHeight + 1)
                        {
                            if (Math.Abs(compareItem.NumberOfColoredPixels - numberOfForegroundColors) < minForeColorMatch)
                            {
                                int dif = NikseBitmapImageSplitter.IsBitmapsAlike(target, compareItem);
                                if (dif < smallestDifference)
                                {
                                    if (!BinaryOcrDb.AllowEqual(compareItem, bob))
                                    {
                                        continue;
                                    }

                                    smallestDifference = dif;
                                    hit = compareItem;
                                    if (dif == 0)
                                    {
                                        break; // foreach ending
                                    }
                                }
                            }
                        }
                    }
                }

                if (smallestDifference > 3)
                {
                    foreach (var compareItem in binOcrDb.CompareImages)
                    {
                        if (compareItem.Width == tWidth + 1 && compareItem.Height == tHeight + 1)
                        {
                            if (Math.Abs(compareItem.NumberOfColoredPixels - numberOfForegroundColors) < minForeColorMatch)
                            {
                                int dif = NikseBitmapImageSplitter.IsBitmapsAlike(target, compareItem);
                                if (dif < smallestDifference)
                                {
                                    if (!BinaryOcrDb.AllowEqual(compareItem, bob))
                                    {
                                        continue;
                                    }

                                    smallestDifference = dif;
                                    hit = compareItem;
                                    if (dif == 0)
                                    {
                                        break; // foreach ending
                                    }
                                }
                            }
                        }
                    }
                }

                if (smallestDifference > 5)
                {
                    foreach (var compareItem in binOcrDb.CompareImages)
                    {
                        if (compareItem.Width == tWidth - 1 && compareItem.Height == tHeight - 1)
                        {
                            if (Math.Abs(compareItem.NumberOfColoredPixels - numberOfForegroundColors) < minForeColorMatch)
                            {
                                int dif = NikseBitmapImageSplitter.IsBitmapsAlike(compareItem, target);
                                if (dif < smallestDifference)
                                {
                                    if (!BinaryOcrDb.AllowEqual(compareItem, bob))
                                    {
                                        continue;
                                    }

                                    smallestDifference = dif;
                                    hit = compareItem;
                                    if (dif == 0)
                                    {
                                        break; // foreach ending
                                    }
                                }
                            }
                        }
                    }
                }

                if (smallestDifference > 5)
                {
                    foreach (var compareItem in binOcrDb.CompareImages)
                    {
                        if (compareItem.Width - 1 == tWidth && compareItem.Height == tHeight)
                        {
                            if (Math.Abs(compareItem.NumberOfColoredPixels - numberOfForegroundColors) < minForeColorMatch)
                            {
                                int dif = NikseBitmapImageSplitter.IsBitmapsAlike(target, compareItem);
                                if (dif < smallestDifference)
                                {
                                    if (!BinaryOcrDb.AllowEqual(compareItem, bob))
                                    {
                                        continue;
                                    }

                                    smallestDifference = dif;
                                    hit = compareItem;
                                    if (dif == 0)
                                    {
                                        break; // foreach ending
                                    }
                                }
                            }
                        }
                    }
                }

                if (smallestDifference > 9 && tWidth > 11)
                {
                    foreach (var compareItem in binOcrDb.CompareImages)
                    {
                        if (compareItem.Width == tWidth - 2 && compareItem.Height == tHeight)
                        {
                            if (Math.Abs(compareItem.NumberOfColoredPixels - numberOfForegroundColors) < minForeColorMatch)
                            {
                                int dif = NikseBitmapImageSplitter.IsBitmapsAlike(compareItem, target);
                                if (dif < smallestDifference)
                                {
                                    if (!BinaryOcrDb.AllowEqual(compareItem, bob))
                                    {
                                        continue;
                                    }

                                    smallestDifference = dif;
                                    hit = compareItem;
                                    if (dif == 0)
                                    {
                                        break; // foreach ending
                                    }
                                }
                            }
                        }
                    }
                }

                if (smallestDifference > 9 && tWidth > 14)
                {
                    foreach (var compareItem in binOcrDb.CompareImages)
                    {
                        if (compareItem.Width == tWidth - 3 && compareItem.Height == tHeight)
                        {
                            if (Math.Abs(compareItem.NumberOfColoredPixels - numberOfForegroundColors) < minForeColorMatch)
                            {
                                int dif = NikseBitmapImageSplitter.IsBitmapsAlike(compareItem, target);
                                if (dif < smallestDifference)
                                {
                                    if (!BinaryOcrDb.AllowEqual(compareItem, bob))
                                    {
                                        continue;
                                    }

                                    smallestDifference = dif;
                                    hit = compareItem;
                                    if (dif == 0)
                                    {
                                        break; // foreach ending
                                    }
                                }
                            }
                        }
                    }
                }

                if (smallestDifference > 9 && tWidth > 14)
                {
                    foreach (var compareItem in binOcrDb.CompareImages)
                    {
                        if (compareItem.Width == tWidth && compareItem.Height == tHeight - 3)
                        {
                            if (Math.Abs(compareItem.NumberOfColoredPixels - numberOfForegroundColors) < minForeColorMatch)
                            {
                                int dif = NikseBitmapImageSplitter.IsBitmapsAlike(compareItem, target);
                                if (dif < smallestDifference)
                                {
                                    if (!BinaryOcrDb.AllowEqual(compareItem, bob))
                                    {
                                        continue;
                                    }

                                    smallestDifference = dif;
                                    hit = compareItem;
                                    if (dif == 0)
                                    {
                                        break; // foreach ending
                                    }
                                }
                            }
                        }
                    }
                }

                if (smallestDifference > 9 && tWidth > 14)
                {
                    foreach (var compareItem in binOcrDb.CompareImages)
                    {
                        if (compareItem.Width - 2 == tWidth && compareItem.Height == tHeight)
                        {
                            if (Math.Abs(compareItem.NumberOfColoredPixels - numberOfForegroundColors) < minForeColorMatch)
                            {
                                int dif = NikseBitmapImageSplitter.IsBitmapsAlike(target, compareItem);
                                if (dif < smallestDifference)
                                {
                                    if (!BinaryOcrDb.AllowEqual(compareItem, bob))
                                    {
                                        continue;
                                    }

                                    smallestDifference = dif;
                                    hit = compareItem;
                                    if (dif == 0)
                                    {
                                        break; // foreach ending
                                    }
                                }
                            }
                        }
                    }
                }
            }

            if (smallestDifference == 0)
            {
                if (binOcrDb.CompareImages.IndexOf(hit) > 200)
                {
                    lock (BinOcrDbMoveFirstLock)
                    {
                        binOcrDb.CompareImages.Remove(hit);
                        binOcrDb.CompareImages.Insert(0, hit);
                        index = 0;
                    }
                }
            }
        }

        private static readonly object BinOcrDbMoveFirstLock = new object();

        private string SaveCompareItemNew(ImageSplitterItem newTarget, string text, bool isItalic, List<ImageSplitterItem> expandList)
        {
            int expandCount = 0;
            if (expandList != null)
            {
                expandCount = expandList.Count;
            }

            if (expandCount > 0)
            {
                var bob = new BinaryOcrBitmap(expandList[0].NikseBitmap, isItalic, expandCount, text, expandList[0].X, expandList[0].Top) { ExpandedList = new List<BinaryOcrBitmap>() };
                for (int j = 1; j < expandList.Count; j++)
                {
                    var expandedBob = new BinaryOcrBitmap(expandList[j].NikseBitmap)
                    {
                        X = expandList[j].X,
                        Y = expandList[j].Top
                    };
                    bob.ExpandedList.Add(expandedBob);
                }

                _binaryOcrDb.Add(bob);
                _binaryOcrDb.Save();
                return bob.Key;
            }
            else
            {
                var bob = new BinaryOcrBitmap(newTarget.NikseBitmap, isItalic, expandCount, text, newTarget.X, newTarget.Top);
                _binaryOcrDb.Add(bob);
                _binaryOcrDb.Save();
                return bob.Key;
            }
        }

        private void ColorLineByNumberOfUnknownWords(int index, int wordsNotFound, string line)
        {
            SetUnknownWordsColor(index, wordsNotFound, line);

            var p = _subtitle.GetParagraphOrDefault(index);
            if (p != null && _unknownWordsDictionary.ContainsKey(p.Id))
            {
                _unknownWordsDictionary[p.Id] = wordsNotFound;
            }
            else if (p != null)
            {
                _unknownWordsDictionary.Add(p.Id, wordsNotFound);
            }
        }

        private void SetUnknownWordsColor(int index, int wordsNotFound, string line)
        {
            if (_ocrFixEngine == null || !_ocrFixEngine.IsDictionaryLoaded)
            {
                subtitleListView1.SetBackgroundColor(index, DefaultBackColor);
                return;
            }

            if (wordsNotFound >= 2)
            {
                subtitleListView1.SetBackgroundColor(index, _listViewOrange);
            }
            else if (wordsNotFound == 1 || line.Length == 1 || line.Contains('_') || HasSingleLetters(line))
            {
                subtitleListView1.SetBackgroundColor(index, _listViewYellow);
            }
            else if (wordsNotFound == 1)
            {
                subtitleListView1.SetBackgroundColor(index, _listViewYellow);
            }
            else if (string.IsNullOrWhiteSpace(line))
            {
                subtitleListView1.SetBackgroundColor(index, _listViewOrange);
            }
            else
            {
                subtitleListView1.SetBackgroundColor(index, _listViewGreen);
            }
        }

        private long _ocrCount;
        private double _ocrHeight = 20;

        /// <summary>
        /// Ocr via binary (two color) image compare
        /// </summary>
        private string SplitAndOcrBinaryImageCompare(Bitmap bitmap, int listViewIndex)
        {
            if (_ocrFixEngine == null)
            {
                LoadOcrFixEngine(string.Empty, LanguageString);
            }

            var matches = new List<CompareMatch>();
            var parentBitmap = new NikseBitmap(bitmap);
            int minLineHeight = GetMinLineHeight();
            var list = NikseBitmapImageSplitter.SplitBitmapToLettersNew(parentBitmap, _numericUpDownPixelsIsSpace, checkBoxRightToLeft.Checked, Configuration.Settings.VobSubOcr.TopToBottom, minLineHeight, _autoLineHeight, _ocrCount > 20 ? _ocrHeight : -1);
            UpdateLineHeights(list);
            int index = 0;
            bool expandSelection = false;
            bool shrinkSelection = false;
            var expandSelectionList = new List<ImageSplitterItem>();
            while (index < list.Count)
            {
                var item = list[index];
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

                    item = GetExpandedSelectionNew(parentBitmap, expandSelectionList);

                    _vobSubOcrCharacter.Initialize(bitmap, item, _manualOcrDialogPosition, _italicCheckedLast, expandSelectionList.Count > 1, null, _lastAdditions, this);
                    DialogResult result = _vobSubOcrCharacter.ShowDialog(this);
                    _manualOcrDialogPosition = _vobSubOcrCharacter.FormPosition;
                    if (result == DialogResult.Cancel && _vobSubOcrCharacter.SkipImage)
                    {
                        break;
                    }

                    if (result == DialogResult.OK && _vobSubOcrCharacter.ShrinkSelection)
                    {
                        shrinkSelection = true;
                        index--;
                        if (expandSelectionList.Count > 0)
                        {
                            expandSelectionList.RemoveAt(expandSelectionList.Count - 1);
                        }
                    }
                    else if (result == DialogResult.OK && _vobSubOcrCharacter.ExpandSelection)
                    {
                        expandSelection = true;
                    }
                    else if (result == DialogResult.OK)
                    {
                        string text = _vobSubOcrCharacter.ManualRecognizedCharacters;
                        string name = SaveCompareItemNew(item, text, _vobSubOcrCharacter.IsItalic, expandSelectionList);
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
                    _ocrCount++;
                    _ocrHeight += (item.NikseBitmap.Height - _ocrHeight) / _ocrCount;
                    var match = GetCompareMatchNew(item, out var bestGuess, list, index, _binaryOcrDb);
                    if (match == null) // Try nOCR (line OCR) if no image compare match
                    {
                        if (_nOcrDb != null && _nOcrDb.OcrCharacters.Count > 0)
                        {
                            match = GetNOcrCompareMatchNew(item, parentBitmap, _nOcrDb, true, true, index, list);
                        }
                    }

                    if (match == null)
                    {
                        int nextIndex = index + 1;
                        var allowExpand = nextIndex < list.Count && (list[nextIndex].SpecialCharacter != Environment.NewLine && list[nextIndex].SpecialCharacter != " ");

                        _vobSubOcrCharacter.Initialize(bitmap, item, _manualOcrDialogPosition, _italicCheckedLast, false, bestGuess, _lastAdditions, this, allowExpand);
                        DialogResult result = _vobSubOcrCharacter.ShowDialog(this);
                        _manualOcrDialogPosition = _vobSubOcrCharacter.FormPosition;

                        if (result == DialogResult.Cancel && _vobSubOcrCharacter.SkipImage)
                        {
                            break;
                        }

                        if (result == DialogResult.OK && _vobSubOcrCharacter.ExpandSelection)
                        {
                            expandSelectionList.Add(item);
                            expandSelection = true;
                        }
                        else if (result == DialogResult.OK)
                        {
                            string text = _vobSubOcrCharacter.ManualRecognizedCharacters;
                            string name = SaveCompareItemNew(item, text, _vobSubOcrCharacter.IsItalic, null);
                            var addition = new ImageCompareAddition(name, text, item.NikseBitmap, _vobSubOcrCharacter.IsItalic, listViewIndex);
                            _lastAdditions.Add(addition);
                            matches.Add(new CompareMatch(text, _vobSubOcrCharacter.IsItalic, 0, null, item));
                            SetBinOcrLowercaseUppercase(item.NikseBitmap.Height, text);
                        }
                        else if (result == DialogResult.Abort)
                        {
                            _abort = true;
                        }
                        else
                        {
                            matches.Add(new CompareMatch("*", false, 0, null, item));
                        }

                        _italicCheckedLast = _vobSubOcrCharacter.IsItalic;
                    }
                    else // found image match
                    {
                        matches.Add(new CompareMatch(match.Text, match.Italic, 0, null, item));
                        if (match.ExpandCount > 0)
                        {
                            index += match.ExpandCount - 1;
                        }
                    }
                }

                if (_abort)
                {
                    return MatchesToItalicStringConverter.GetStringWithItalicTags(matches);
                }

                if (!expandSelection && !shrinkSelection)
                {
                    index++;
                }

                if (shrinkSelection && expandSelectionList.Count < 2)
                {
                    shrinkSelection = false;
                    expandSelectionList = new List<ImageSplitterItem>();
                }
            }

            string line = MatchesToItalicStringConverter.GetStringWithItalicTags(matches);

            if (checkBoxAutoFixCommonErrors.Checked && _ocrFixEngine != null)
            {
                line = _ocrFixEngine.FixOcrErrorsViaHardcodedRules(line, _lastLine, null); // TODO: Add abbreviations list
            }

            if (checkBoxRightToLeft.Checked)
            {
                line = ReverseNumberStrings(line);
            }

            //OCR fix engine
            string textWithOutFixes = line;
            if (_ocrFixEngine != null && _ocrFixEngine.IsDictionaryLoaded)
            {
                var autoGuessLevel = OcrFixEngine.AutoGuessLevel.None;
                if (checkBoxGuessUnknownWords.Checked)
                {
                    autoGuessLevel = OcrFixEngine.AutoGuessLevel.Aggressive;
                }

                if (checkBoxAutoFixCommonErrors.Checked)
                {
                    line = _ocrFixEngine.FixOcrErrors(line, listViewIndex, _lastLine, true, autoGuessLevel);
                }

                int wordsNotFound = _ocrFixEngine.CountUnknownWordsViaDictionary(line, out var correctWords);

                // smaller space pixels for italic
                if (wordsNotFound > 0 && line.Contains("<i>", StringComparison.Ordinal))
                {
                    AddItalicCouldBeSpace(matches, parentBitmap, _unItalicFactor, _numericUpDownPixelsIsSpace);
                }
                if (wordsNotFound > 0 && line.Contains("<i>", StringComparison.Ordinal) && matches.Any(p => p?.ImageSplitterItem?.CouldBeSpaceBefore == true))
                {
                    int j = 1;
                    while (j < matches.Count)
                    {
                        var match = matches[j];
                        var prevMatch = matches[j - 1];
                        if (match.ImageSplitterItem?.CouldBeSpaceBefore == true)
                        {
                            match.ImageSplitterItem.CouldBeSpaceBefore = false;
                            if (prevMatch.Italic)
                            {
                                matches.Insert(j, new CompareMatch(" ", false, 0, string.Empty, new ImageSplitterItem(" ")));
                            }
                        }

                        j++;
                    }

                    var tempLine = MatchesToItalicStringConverter.GetStringWithItalicTags(matches);
                    var oldAutoGuessesUsed = new List<LogItem>(_ocrFixEngine.AutoGuessesUsed);
                    var oldUnknownWordsFound = new List<LogItem>(_ocrFixEngine.UnknownWordsFound);
                    _ocrFixEngine.AutoGuessesUsed.Clear();
                    _ocrFixEngine.UnknownWordsFound.Clear();
                    if (checkBoxAutoFixCommonErrors.Checked)
                    {
                        tempLine = _ocrFixEngine.FixOcrErrors(tempLine, listViewIndex, _lastLine, true, autoGuessLevel);
                    }

                    int tempWordsNotFound = _ocrFixEngine.CountUnknownWordsViaDictionary(tempLine, out var tempCorrectWords);
                    if (tempWordsNotFound <= wordsNotFound && tempCorrectWords > correctWords)
                    {
                        wordsNotFound = tempWordsNotFound;
                        correctWords = tempCorrectWords;
                        line = tempLine;
                    }
                    else
                    {
                        _ocrFixEngine.AutoGuessesUsed = oldAutoGuessesUsed;
                        _ocrFixEngine.UnknownWordsFound = oldUnknownWordsFound;
                    }
                }

                if (wordsNotFound > 0 || correctWords == 0 || textWithOutFixes != null && string.IsNullOrWhiteSpace(textWithOutFixes.Replace("~", string.Empty)))
                {
                    _ocrFixEngine.AutoGuessesUsed.Clear();
                    _ocrFixEngine.UnknownWordsFound.Clear();
                    line = _ocrFixEngine.FixUnknownWordsViaGuessOrPrompt(out wordsNotFound, line, listViewIndex, bitmap, checkBoxAutoFixCommonErrors.Checked, checkBoxPromptForUnknownWords.Checked, true, autoGuessLevel);
                }

                if (_ocrFixEngine.Abort)
                {
                    ButtonStopClick(null, null);
                    _ocrFixEngine.Abort = false;

                    if (_ocrFixEngine.LastAction == OcrSpellCheck.Action.InspectCompareMatches)
                    {
                        InspectImageCompareMatchesForCurrentImageToolStripMenuItem_Click(null, null);
                    }

                    return string.Empty;
                }

                // Log used word guesses (via word replace list)
                foreach (var guess in _ocrFixEngine.AutoGuessesUsed)
                {
                    listBoxLogSuggestions.Items.Add(guess);
                }

                _ocrFixEngine.AutoGuessesUsed.Clear();

                // Log unknown words guess (found via spelling dictionaries)
                LogUnknownWords();

                ColorLineByNumberOfUnknownWords(listViewIndex, wordsNotFound, line);
            }

            if (textWithOutFixes.Trim() != line.Trim())
            {
                _tesseractOcrAutoFixes++;
                labelFixesMade.Text = $" - {_tesseractOcrAutoFixes}";
                LogOcrFix(listViewIndex, textWithOutFixes, line);
            }

            return line;
        }

        private static void AddItalicCouldBeSpace(List<CompareMatch> matches, NikseBitmap parentBitmap, double unItalicFactor, int pixelsIsSpace)
        {
            foreach (var match in matches)
            {
                if (match.ImageSplitterItem != null)
                {
                    match.ImageSplitterItem.CouldBeSpaceBefore = false;
                }
            }

            for (int i = 0; i < matches.Count - 1; i++)
            {
                var match = matches[i];
                var matchNext = matches[i + 1];
                if (!match.Italic || matchNext.Text == "," ||
                    string.IsNullOrWhiteSpace(match.Text) || string.IsNullOrWhiteSpace(matchNext.Text) ||
                    match.ImageSplitterItem == null || matchNext.ImageSplitterItem == null)
                {

                    continue;
                }

                int blankVerticalLines = IsVerticalAngledLineTransparent(parentBitmap, match, matchNext, unItalicFactor);
                if (match.Text == "f" || match.Text == "," || matchNext.Text.StartsWith('y') || matchNext.Text.StartsWith('j'))
                {
                    blankVerticalLines++;
                }

                if (blankVerticalLines >= pixelsIsSpace)
                {
                    matchNext.ImageSplitterItem.CouldBeSpaceBefore = true;
                }
            }
        }

        private static int IsVerticalAngledLineTransparent(NikseBitmap parentBitmap, CompareMatch match, CompareMatch next, double unItalicFactor)
        {
            int blanks = 0;
            var min = match.ImageSplitterItem.X + match.ImageSplitterItem.NikseBitmap.Width;
            var max = next.ImageSplitterItem.X + next.ImageSplitterItem.NikseBitmap.Width / 2;
            for (int startX = min; startX < max; startX++)
            {
                var lineBlank = true;
                for (int y = match.ImageSplitterItem.Y; y < match.ImageSplitterItem.Y + match.ImageSplitterItem.NikseBitmap.Height; y++)
                {
                    var x = startX - (y - match.ImageSplitterItem.Y) * unItalicFactor;
                    if (x >= 0)
                    {
                        var color = parentBitmap.GetPixel((int)Math.Round(x), y);
                        if (color.A != 0)
                        {
                            lineBlank = false;
                            if (blanks > 0)
                            {
                                return blanks;
                            }
                        }
                    }
                }

                if (lineBlank)
                {
                    blanks++;
                }
            }

            return blanks;
        }

        private void SetBinOcrLowercaseUppercase(int height, string text)
        {
            if (text == "e" || text == "a")
            {
                _ocrLowercaseHeightsTotalCount++;
                _ocrLowercaseHeightsTotal += height;
            }

            if (text == "E" || text == "H" || text == "R" || text == "D" || text == "T")
            {
                _ocrUppercaseHeightsTotalCount++;
                _ocrUppercaseHeightsTotal += height;
            }
        }

        private void SaveNOcr()
        {
            try
            {
                _nOcrDb.Save();
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
            }
        }

        private void NOCRIntialize(Bitmap bitmap)
        {
            var nikseBitmap = new NikseBitmap(bitmap);
            var list = NikseBitmapImageSplitter.SplitBitmapToLettersNew(nikseBitmap, _numericUpDownPixelsIsSpace, checkBoxRightToLeft.Checked, Configuration.Settings.VobSubOcr.TopToBottom, 12, _autoLineHeight);
            UpdateLineHeights(list);
            foreach (var item in list)
            {
                if (item.NikseBitmap != null)
                {
                    var bmp = item.NikseBitmap;
                    bmp.ReplaceNonWhiteWithTransparent();
                    item.Y += bmp.CropTopTransparent(0);
                    bmp.CropTransparentSidesAndBottom(0, true);
                    GetNOcrCompareMatchNew(item, nikseBitmap, _nOcrDb, false, false, list.IndexOf(item), list);
                }
            }
        }

        private string OcrViaNOCR(Bitmap bitmap, int listViewIndex)
        {
            if (_ocrFixEngine == null)
            {
                comboBoxDictionaries_SelectedIndexChanged(null, null);
            }

            string line = string.Empty;
            var matches = new List<CompareMatch>();
            var nbmpInput = new NikseBitmap(bitmap);

            if (_nOcrThreadResults != null && _nOcrThreadResults.Length > listViewIndex && _nOcrThreadResults[listViewIndex] != null)
            {
                line = _nOcrThreadResults[listViewIndex].ResultText;
                matches.AddRange(_nOcrThreadResults[listViewIndex].ResultMatches);
                _nOcrThreadResults[listViewIndex] = null;
            }

            if (string.IsNullOrEmpty(line))
            {
                matches = new List<CompareMatch>();
                var list = NikseBitmapImageSplitter.SplitBitmapToLettersNew(nbmpInput, _numericUpDownPixelsIsSpace, checkBoxRightToLeftNOCR.Checked, Configuration.Settings.VobSubOcr.TopToBottom, GetMinLineHeight(), _autoLineHeight);
                UpdateLineHeights(list);

                int index = 0;
                bool expandSelection = false;
                bool shrinkSelection = false;
                var expandSelectionList = new List<ImageSplitterItem>();
                while (index < list.Count)
                {
                    var item = list[index];
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

                        item = GetExpandedSelectionNew(nbmpInput, expandSelectionList);
                        _vobSubOcrNOcrCharacter.Initialize(bitmap, item, _manualOcrDialogPosition, _italicCheckedLast, true, expandSelectionList.Count > 1, string.Empty);
                        var result = _vobSubOcrNOcrCharacter.ShowDialog(this);
                        _manualOcrDialogPosition = _vobSubOcrNOcrCharacter.FormPosition;
                        if (result == DialogResult.OK && _vobSubOcrNOcrCharacter.ShrinkSelection)
                        {
                            shrinkSelection = true;
                            index--;
                            if (expandSelectionList.Count > 0)
                            {
                                expandSelectionList.RemoveAt(expandSelectionList.Count - 1);
                            }
                        }
                        else if (result == DialogResult.OK && _vobSubOcrNOcrCharacter.ExpandSelection)
                        {
                            expandSelection = true;
                        }
                        else if (result == DialogResult.OK)
                        {
                            var c = _vobSubOcrNOcrCharacter.NOcrChar;
                            if (expandSelectionList.Count > 1)
                            {
                                c.ExpandCount = expandSelectionList.Count;
                                c.MarginTop = expandSelectionList.First().Top - expandSelectionList.Min(p => p.Top);
                            }

                            _nOcrDb.Add(c);
                            SaveNOcrWithCurrentLanguage();
                            var text = _vobSubOcrNOcrCharacter.NOcrChar.Text;
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
                        var match = GetNOcrCompareMatchNew(item, nbmpInput, _nOcrDb, checkBoxNOcrItalic.Checked, !checkBoxNOcrDrawUnknownLetters.Checked, index, list);
                        if (match == null)
                        {
                            _vobSubOcrNOcrCharacter.Initialize(bitmap, item, _manualOcrDialogPosition, _italicCheckedLast, true, false, string.Empty);
                            var result = _vobSubOcrNOcrCharacter.ShowDialog(this);
                            _manualOcrDialogPosition = _vobSubOcrNOcrCharacter.FormPosition;
                            if (result == DialogResult.OK && _vobSubOcrNOcrCharacter.ExpandSelection)
                            {
                                expandSelectionList.Add(item);
                                expandSelection = true;
                            }
                            else if (result == DialogResult.OK)
                            {
                                _nOcrDb.Add(_vobSubOcrNOcrCharacter.NOcrChar);
                                SaveNOcrWithCurrentLanguage();
                                string text = _vobSubOcrNOcrCharacter.NOcrChar.Text;
                                matches.Add(new CompareMatch(text, _vobSubOcrNOcrCharacter.IsItalic, 0, null) { ImageSplitterItem = item });
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
                            matches.Add(new CompareMatch(match.Text, match.Italic, 0, null) { ImageSplitterItem = item });
                            if (match.ExpandCount > 0)
                            {
                                index += match.ExpandCount - 1;
                            }
                        }
                    }

                    if (_abort)
                    {
                        return MatchesToItalicStringConverter.GetStringWithItalicTags(matches);
                    }

                    if (!expandSelection && !shrinkSelection)
                    {
                        index++;
                    }

                    if (shrinkSelection && expandSelectionList.Count < 2)
                    {
                        shrinkSelection = false;
                        expandSelectionList = new List<ImageSplitterItem>();
                    }
                }

                line = MatchesToItalicStringConverter.GetStringWithItalicTags(matches);
            }

            var fixCommonErrors = checkBoxAutoFixCommonErrors.Checked;
            if (fixCommonErrors)
            {
                line = FixNocrHardcodedStuff(line);
                line = FixZeroInsideWords(line);
            }

            //OCR fix engine
            string textWithOutFixes = line;
            if (_ocrFixEngine != null && _ocrFixEngine.IsDictionaryLoaded)
            {
                if (fixCommonErrors)
                {
                    line = _ocrFixEngine.FixOcrErrors(line, listViewIndex, _lastLine, true, GetAutoGuessLevel());
                }

                int wordsNotFound = _ocrFixEngine.CountUnknownWordsViaDictionary(line, out var correctWords);

                // smaller space pixels for italic
                if (wordsNotFound > 0 && !string.IsNullOrEmpty(line) && line.Contains("<i>", StringComparison.Ordinal))
                {
                    AddItalicCouldBeSpace(matches, nbmpInput, _unItalicFactor, _numericUpDownPixelsIsSpace);
                }
                if (wordsNotFound > 0 && !string.IsNullOrEmpty(line) && line.Contains("<i>", StringComparison.Ordinal) && matches.Any(p => p?.ImageSplitterItem?.CouldBeSpaceBefore == true))
                {
                    int j = 1;
                    while (j < matches.Count)
                    {
                        var match = matches[j];
                        var prevMatch = matches[j - 1];
                        if (match.ImageSplitterItem?.CouldBeSpaceBefore == true)
                        {
                            match.ImageSplitterItem.CouldBeSpaceBefore = false;
                            if (prevMatch.Italic)
                            {
                                matches.Insert(j, new CompareMatch(" ", false, 0, string.Empty, new ImageSplitterItem(" ")));
                            }
                        }

                        j++;
                    }
                    var tempLine = MatchesToItalicStringConverter.GetStringWithItalicTags(matches);
                    var oldAutoGuessesUsed = new List<LogItem>(_ocrFixEngine.AutoGuessesUsed);
                    var oldUnknownWordsFound = new List<LogItem>(_ocrFixEngine.UnknownWordsFound);
                    _ocrFixEngine.AutoGuessesUsed.Clear();
                    _ocrFixEngine.UnknownWordsFound.Clear();
                    if (fixCommonErrors)
                    {
                        tempLine = _ocrFixEngine.FixOcrErrors(tempLine, listViewIndex, _lastLine, true, GetAutoGuessLevel());
                    }

                    int tempWordsNotFound = _ocrFixEngine.CountUnknownWordsViaDictionary(tempLine, out var tempCorrectWords);
                    if (tempWordsNotFound <= wordsNotFound && tempCorrectWords > correctWords)
                    {
                        wordsNotFound = tempWordsNotFound;
                        correctWords = tempCorrectWords;
                        line = tempLine;
                        textWithOutFixes = tempLine;
                        if (fixCommonErrors)
                        {
                            line = FixNocrHardcodedStuff(line);
                            line = FixZeroInsideWords(line);
                        }
                    }
                    else
                    {
                        _ocrFixEngine.AutoGuessesUsed = oldAutoGuessesUsed;
                        _ocrFixEngine.UnknownWordsFound = oldUnknownWordsFound;
                    }
                }

                // prompt for user input for unknown words
                if (wordsNotFound > 0 || correctWords == 0 || textWithOutFixes != null && string.IsNullOrWhiteSpace(textWithOutFixes.Replace("~", string.Empty)))
                {
                    _ocrFixEngine.AutoGuessesUsed.Clear();
                    _ocrFixEngine.UnknownWordsFound.Clear();
                    line = _ocrFixEngine.FixUnknownWordsViaGuessOrPrompt(out wordsNotFound, line, listViewIndex, bitmap, checkBoxAutoFixCommonErrors.Checked, checkBoxPromptForUnknownWords.Checked, true, GetAutoGuessLevel());
                }

                if (_ocrFixEngine.Abort)
                {
                    ButtonStopClick(null, null);
                    _ocrFixEngine.Abort = false;

                    if (_ocrFixEngine.LastAction == OcrSpellCheck.Action.InspectCompareMatches)
                    {
                        toolStripMenuItemInspectNOcrMatches_Click(null, null);
                    }

                    return string.Empty;
                }

                // Log used word guesses (via word replace list)
                foreach (var guess in _ocrFixEngine.AutoGuessesUsed)
                {
                    listBoxLogSuggestions.Items.Add(guess);
                }

                _ocrFixEngine.AutoGuessesUsed.Clear();

                // Log unknown words guess (found via spelling dictionaries)
                LogUnknownWords();

                if (fixCommonErrors || checkBoxPromptForUnknownWords.Checked)
                {
                    ColorLineByNumberOfUnknownWords(listViewIndex, wordsNotFound, line);
                }
            }

            if (textWithOutFixes != null && textWithOutFixes.Trim() != line.Trim())
            {
                _tesseractOcrAutoFixes++;
                labelFixesMade.Text = $" - {_tesseractOcrAutoFixes}";
                LogOcrFix(listViewIndex, textWithOutFixes, line);
            }

            return line;
        }

        private void UpdateLineHeights(List<ImageSplitterItem> list)
        {
            if (_ocrLetterHeightsTotalCount < 1000)
            {
                foreach (var letter in list)
                {
                    if (letter.NikseBitmap != null)
                    {
                        _ocrLetterHeightsTotal += letter.NikseBitmap.Height;
                        _ocrLetterHeightsTotalCount++;
                    }
                }
            }
        }

        private int GetMinLineHeight()
        {
            if (_ocrMinLineHeight > 0)
            {
                return _ocrMinLineHeight;
            }

            if (_ocrLetterHeightsTotalCount > 20)
            {
                var averageLineHeight = _ocrLetterHeightsTotal / _ocrLetterHeightsTotalCount;
                return (int)Math.Round(averageLineHeight * 0.9);
            }

            return _bluRaySubtitlesOriginal != null ? 25 : 12;
        }

        private static string FixZeroInsideWords(string line)
        {
            if (!Configuration.Settings.Tools.OcrFixUseHardcodedRules || !line.Contains('0') || line.Length < 3)
            {
                return line;
            }

            var sb = new StringBuilder(line.Length);
            if (line[0] == '0' && char.IsLetter(line[1]))
            {
                sb.Append('o');
            }
            else
            {
                sb.Append(line[0]);
            }

            for (var index = 1; index < line.Length - 1; index++)
            {
                var ch = line[index];
                if (ch == '0' && (index < 2 || !char.IsNumber(line[index - 2])))
                {
                    var prev = line[index - 1];
                    var next = line[index + 1];
                    if (char.IsLetter(prev) && char.IsLetter(next))
                    {
                        if (prev == char.ToUpperInvariant(prev) && next == char.ToUpperInvariant(next))
                        {
                            sb.Append("O");
                        }
                        else
                        {
                            sb.Append("o");
                        }
                    }
                    else
                    {
                        sb.Append(ch);
                    }
                }
                else
                {
                    sb.Append(ch);
                }
            }
            sb.Append(line[line.Length - 1]);
            return sb.ToString();
        }

        private string FixNocrHardcodedStuff(string input)
        {
            if (!Configuration.Settings.Tools.OcrFixUseHardcodedRules || string.IsNullOrEmpty(input))
            {
                return input;
            }

            var line = input;

            var l = LanguageString;
            if (l != null && l.StartsWith("en", StringComparison.OrdinalIgnoreCase))
            {
                // fix I/l
                int start = line.IndexOf('I');
                while (start > 0)
                {
                    if (start > 0 && char.IsLower(line[start - 1]))
                    {
                        line = line.Remove(start, 1).Insert(start, "l");
                    }
                    else if (start < line.Length - 1 && char.IsLower(line[start + 1]))
                    {
                        line = line.Remove(start, 1).Insert(start, "l");
                    }

                    start++;
                    start = line.IndexOf('I', start);
                }

                start = line.IndexOf('l');
                while (start > 0)
                {
                    if (start < line.Length - 1 && char.IsUpper(line[start + 1]))
                    {
                        line = line.Remove(start, 1).Insert(start, "I");
                    }

                    start++;
                    start = line.IndexOf('l', start);
                }

                if (line.Contains('l'))
                {
                    if (line.StartsWith('l'))
                    {
                        line = @"I" + line.Substring(1);
                    }

                    if (line.StartsWith("<i>l", StringComparison.Ordinal))
                    {
                        line = line.Remove(3, 1).Insert(3, "I");
                    }

                    if (line.StartsWith("- l", StringComparison.Ordinal))
                    {
                        line = line.Remove(2, 1).Insert(2, "I");
                    }

                    if (line.StartsWith("-l", StringComparison.Ordinal))
                    {
                        line = line.Remove(1, 1).Insert(1, "I");
                    }

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

                if (line.Contains('I'))
                {
                    line = line.Replace("II", "ll");
                }
            }

            // fix periods with space between
            line = line.Replace(".   .", "..");
            line = line.Replace(".  .", "..");
            line = line.Replace(". .", "..");
            line = line.Replace(". .", "..");
            line = line.Replace(" ." + Environment.NewLine, "." + Environment.NewLine);
            if (line.EndsWith(" .", StringComparison.Ordinal))
            {
                line = line.Remove(line.Length - 2, 1);
            }

            // fix no space before comma
            line = line.Replace(" ,", ",");

            // fix O => 0
            if (line.Contains('O'))
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

            if (checkBoxAutoFixCommonErrors.Checked && _ocrFixEngine != null)
            {
                line = _ocrFixEngine.FixOcrErrorsViaHardcodedRules(line, _lastLine, null); // TODO: Add abbreviations list
            }

            if (checkBoxRightToLeft.Checked)
            {
                line = ReverseNumberStrings(line);
            }

            return line;
        }

        private static string ReverseNumberStrings(string line)
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

        internal static ImageSplitterItem GetExpandedSelectionNew(NikseBitmap bitmap, List<ImageSplitterItem> expandSelectionList)
        {
            int minimumX = expandSelectionList[0].X;
            int maximumX = expandSelectionList[expandSelectionList.Count - 1].X + expandSelectionList[expandSelectionList.Count - 1].NikseBitmap.Width;
            int minimumY = expandSelectionList[0].Y;
            int maximumY = expandSelectionList[0].Y + expandSelectionList[0].NikseBitmap.Height;
            var nbmp = new NikseBitmap(bitmap.Width, bitmap.Height);
            foreach (ImageSplitterItem item in expandSelectionList)
            {
                for (int y = 0; y < item.NikseBitmap.Height; y++)
                {
                    for (int x = 0; x < item.NikseBitmap.Width; x++)
                    {
                        var c = item.NikseBitmap.GetPixel(x, y);
                        if (c.A > 100 && c.R + c.G + c.B > 100)
                        {
                            nbmp.SetPixel(item.X + x, item.Y + y, Color.White);
                        }
                    }
                }

                if (item.Y < minimumY)
                {
                    minimumY = item.Y;
                }

                if (item.Y + item.NikseBitmap.Height > maximumY)
                {
                    maximumY = item.Y + item.NikseBitmap.Height;
                }

                if (item.X < minimumX)
                {
                    minimumX = item.X;
                }

                if (item.X + item.NikseBitmap.Width > maximumX)
                {
                    maximumX = item.X + item.NikseBitmap.Width;
                }
            }

            nbmp.CropTransparentSidesAndBottom(0, true);
            nbmp = NikseBitmapImageSplitter.CropTopAndBottom(nbmp, out _);

            return new ImageSplitterItem(minimumX, minimumY, nbmp);
        }

        public Subtitle SubtitleFromOcr => _subtitle;

        private void FormVobSubOcr_Shown(object sender, EventArgs e)
        {
            if (_mp4List != null)
            {
                checkBoxShowOnlyForced.Visible = false;
                checkBoxUseTimeCodesFromIdx.Visible = false;

                SetButtonsEnabledAfterOcrDone();
                buttonStartOcr.Focus();
            }
            else if (_spList != null)
            {
                checkBoxShowOnlyForced.Visible = false;
                checkBoxUseTimeCodesFromIdx.Visible = false;

                SetButtonsEnabledAfterOcrDone();
                buttonStartOcr.Focus();
            }
            else if (_dvbSubtitles != null)
            {
                checkBoxShowOnlyForced.Visible = false;
                checkBoxUseTimeCodesFromIdx.Visible = false;

                SetButtonsEnabledAfterOcrDone();
                buttonStartOcr.Focus();
            }
            else if (_dvbPesSubtitles != null)
            {
                checkBoxShowOnlyForced.Visible = false;
                checkBoxUseTimeCodesFromIdx.Visible = false;

                SetButtonsEnabledAfterOcrDone();
                buttonStartOcr.Focus();
            }
            else if (_bdnXmlOriginal != null)
            {
                LoadBdnXml();
                _hasForcedSubtitles = false;
                foreach (var x in _bdnXmlOriginal.Paragraphs)
                {
                    if (x.Forced)
                    {
                        _hasForcedSubtitles = true;
                        break;
                    }
                }

                checkBoxShowOnlyForced.Enabled = _hasForcedSubtitles;
                checkBoxUseTimeCodesFromIdx.Visible = false;
            }
            else if (_bluRaySubtitlesOriginal != null)
            {
                var v = (decimal)Configuration.Settings.VobSubOcr.BlurayAllowDifferenceInPercent;
                if (v >= numericUpDownMaxErrorPct.Minimum && v <= numericUpDownMaxErrorPct.Maximum)
                {
                    numericUpDownMaxErrorPct.Value = (decimal)Configuration.Settings.VobSubOcr.BlurayAllowDifferenceInPercent;
                }

                LoadBluRaySup();
                _hasForcedSubtitles = false;
                foreach (var x in _bluRaySubtitlesOriginal)
                {
                    if (x.IsForced)
                    {
                        _hasForcedSubtitles = true;
                        break;
                    }
                }

                checkBoxShowOnlyForced.Enabled = _hasForcedSubtitles;
                checkBoxUseTimeCodesFromIdx.Visible = false;
            }
            else if (_xSubList != null)
            {
                checkBoxShowOnlyForced.Visible = false;
                checkBoxUseTimeCodesFromIdx.Visible = false;

                SetButtonsEnabledAfterOcrDone();
                buttonStartOcr.Focus();
            }
            else
            {
                ReadyVobSubRip();
            }

            VobSubOcr_Resize(null, null);

            if (Configuration.Settings.VobSubOcr.BinaryAutoDetectBestDb)
            {
                SelectBestImageCompareDatabase();
            }

            textBoxCurrentText.BackColor = SystemColors.ActiveBorder;
        }

        public void DoHide()
        {
            SetVisibleCore(false);
        }

        public Subtitle ReadyVobSubRip()
        {
            _vobSubMergedPackListOriginal = new List<VobSubMergedPack>();
            bool hasIdxTimeCodes = false;
            _hasForcedSubtitles = false;
            if (_vobSubMergedPackList == null)
            {
                return null;
            }

            foreach (var x in _vobSubMergedPackList)
            {
                _vobSubMergedPackListOriginal.Add(x);
                if (x.IdxLine != null)
                {
                    hasIdxTimeCodes = true;
                }

                if (x.SubPicture.Forced)
                {
                    _hasForcedSubtitles = true;
                }
            }

            checkBoxUseTimeCodesFromIdx.CheckedChanged -= checkBoxUseTimeCodesFromIdx_CheckedChanged;
            checkBoxUseTimeCodesFromIdx.Visible = hasIdxTimeCodes;
            checkBoxUseTimeCodesFromIdx.Checked = hasIdxTimeCodes;
            checkBoxUseTimeCodesFromIdx.CheckedChanged += checkBoxUseTimeCodesFromIdx_CheckedChanged;
            checkBoxShowOnlyForced.Enabled = _hasForcedSubtitles;
            if (!hasIdxTimeCodes)
            {
                checkBoxCustomFourColors.Checked = true;
            }
            LoadVobRip();
            return _subtitle;
        }

        private void ButtonOkClick(object sender, EventArgs e)
        {
            _okClicked = true; // don't ask about discard changes
            if (_dvbSubtitles != null && _transportStreamUseColor)
            {
                MergeDvbForEachSubImage();
            }

            DialogResult = DialogResult.OK;
        }

        private void SetButtonsStartOcr()
        {
            buttonOK.Enabled = false;
            buttonCancel.Enabled = false;
            buttonStartOcr.Enabled = false;
            buttonStop.Enabled = true;
            buttonChooseEditBinaryImageCompareDb.Enabled = false;
            checkBoxTransportStreamGrayscale.Enabled = false;
            checkBoxTransportStreamGetColorAndSplit.Enabled = false;
            _mainOcrRunning = true;
            progressBar1.Visible = true;
            subtitleListView1.MultiSelect = false;
            checkBoxUseTimeCodesFromIdx.Enabled = false;
            checkBoxShowOnlyForced.Enabled = false;
            labelStatus.Text = string.Empty;
        }

        private void SetButtonsEnabledAfterOcrDone()
        {
            buttonOK.Enabled = true;
            buttonCancel.Enabled = true;
            buttonStartOcr.Enabled = true;
            buttonStop.Enabled = false;
            buttonChooseEditBinaryImageCompareDb.Enabled = true;
            checkBoxTransportStreamGrayscale.Enabled = true;
            checkBoxTransportStreamGetColorAndSplit.Enabled = true;
            _mainOcrRunning = false;
            labelStatus.Text = string.Empty;
            progressBar1.Visible = false;
            subtitleListView1.MultiSelect = true;
            checkBoxUseTimeCodesFromIdx.Enabled = checkBoxUseTimeCodesFromIdx.Visible;
            checkBoxShowOnlyForced.Enabled = _hasForcedSubtitles;
        }

        private bool _isLatinDb;

        private void ButtonStartOcrClick(object sender, EventArgs e)
        {
            if (_subtitle.Paragraphs.Count == 0)
            {
                return;
            }

            if (comboBoxDictionaries.SelectedIndex <= 0)
            {
                _ocrFixEngine = new OcrFixEngine(string.Empty, string.Empty, this, _ocrMethodIndex == _ocrMethodBinaryImageCompare || _ocrMethodIndex == _ocrMethodNocr);
            }

            InitializeTopAlign();

            if (_ocrMethodIndex == _ocrMethodTesseract302 || _ocrMethodIndex == _ocrMethodTesseract4)
            {
                labelStatus.Text = LanguageSettings.Current.General.PleaseWait;
                _tesseractThreadRunner?.Cancel();
                _tesseractThreadRunner = new TesseractThreadRunner(OcrDone);
                _tesseractRunner = new TesseractRunner();
            }

            if (_ocrMethodIndex == _ocrMethodTesseract4 && comboBoxTesseractLanguages.Items.Count == 0)
            {
                buttonGetTesseractDictionaries_Click(sender, e);
                return;
            }

            _mainOcrBitmap = null;
            _tesseractEngineMode = comboBoxTesseractEngineMode.SelectedIndex;
            _isLatinDb = comboBoxCharacterDatabase.SelectedItem != null && comboBoxCharacterDatabase.SelectedItem.ToString().Equals("Latin", StringComparison.Ordinal);
            Configuration.Settings.VobSubOcr.RightToLeft = checkBoxRightToLeft.Checked;
            _lastLine = null;

            SetButtonsStartOcr();
            _fromMenuItem = false;
            _abort = false;
            _autoBreakLines = checkBoxAutoBreakLines.Checked;

            CleanLogsGreaterThanOrEqualTo(numericUpDownStartNumber.Value);

            int max = GetSubtitleCount();

            if ((_ocrMethodIndex == _ocrMethodTesseract4 || _ocrMethodIndex == _ocrMethodTesseract302) &&
                _tesseractAsyncStrings == null)
            {
                _nOcrDb = null;
                _tesseractAsyncStrings = new string[max];
                _tesseractAsyncIndex = (int)numericUpDownStartNumber.Value + 5;
            }
            else if (_ocrMethodIndex == _ocrMethodNocr)
            {
                if (_nOcrDb == null)
                {
                    LoadNOcrWithCurrentLanguage();
                }

                if (_nOcrDb == null)
                {
                    MessageBox.Show("Fatal - No NOCR dictionary loaded!");
                    SetButtonsEnabledAfterOcrDone();
                    return;
                }


                _autoLineHeight = comboBoxNOcrLineSplitMinHeight.SelectedIndex == 0;
                if (comboBoxNOcrLineSplitMinHeight.Visible && comboBoxNOcrLineSplitMinHeight.SelectedIndex > 0)
                {
                    _ocrMinLineHeight = int.Parse(comboBoxNOcrLineSplitMinHeight.Text);
                }
                else
                {
                    _ocrMinLineHeight = -1;
                }

                InitializeNOcrThreads(max);
            }
            else if (_ocrMethodIndex == _ocrMethodBinaryImageCompare)
            {
                if (_binaryOcrDb == null)
                {
                    _binaryOcrDb = new BinaryOcrDb(Configuration.OcrDirectory + "Latin.db", true);
                }

                checkBoxNOcrDrawUnknownLetters.Checked = true;
                _numericUpDownMaxErrorPct = (double)numericUpDownMaxErrorPct.Value;
                _autoLineHeight = comboBoxLineSplitMinLineHeight.SelectedIndex == 0;
                if (comboBoxLineSplitMinLineHeight.Visible && comboBoxLineSplitMinLineHeight.SelectedIndex > 0)
                {
                    _ocrMinLineHeight = int.Parse(comboBoxLineSplitMinLineHeight.Text);
                }
                else
                {
                    _ocrMinLineHeight = -1;
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
            subtitleListView1.MultiSelect = false;
            mainOcrTimer_Tick(null, null);
        }

        private void InitializeNOcrThreads(int max)
        {
            _nOcrDbThread = new NOcrDb(_nOcrDb, null);
            _nOcrThreadResults = new NOcrThreadResult[max];

            int noOfThreads = Environment.ProcessorCount - 1;
            if (noOfThreads >= max)
            {
                noOfThreads = max - 1;
            }

            _ocrThreadStop = false;
            int start = (int)numericUpDownStartNumber.Value + 5;
            if (noOfThreads >= 1 && max > 5)
            {
                // find letter size (uppercase/lowercase)
                int testIndex = 0;
                var noOfSubs = GetSubtitleCount();
                while (testIndex < noOfSubs && testIndex < 10 && (_ocrLowercaseHeightsTotalCount < 25 || _ocrUppercaseHeightsTotalCount < 25))
                {
                    NOCRIntialize(GetSubtitleBitmap(testIndex));
                    testIndex++;
                }

                for (int i = 0; i < noOfThreads; i++)
                {
                    if (start + i < max)
                    {
                        var bw = new BackgroundWorker();
                        var p = new NOcrThreadParameter(start + i, _nOcrDbThread, bw, noOfThreads, _unItalicFactor, checkBoxNOcrItalic.Checked, _numericUpDownPixelsIsSpace, checkBoxRightToLeftNOCR.Checked)
                        {
                            NOcrLastLowercaseHeight = GetLastBinOcrLowercaseHeight(),
                            NOcrLastUppercaseHeight = GetLastBinOcrUppercaseHeight(),
                        };
                        bw.DoWork += NOcrThreadDoWork;
                        bw.RunWorkerCompleted += NOcrThreadRunWorkerCompleted;
                        bw.RunWorkerAsync(p);
                        Application.DoEvents();
                    }
                }
            }
        }

        private void NOcrThreadRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (_ocrThreadStop)
            {
                return;
            }

            var p = (NOcrThreadParameter)e.Result;
            if (_nOcrThreadResults != null && p.Index < _nOcrThreadResults.Length && _nOcrThreadResults[p.Index] == null)
            {
                _nOcrThreadResults[p.Index] = new NOcrThreadResult(p);
            }

            p.Index += p.Increment;
            if (p.Index < _subtitle.Paragraphs.Count)
            {
                p = new NOcrThreadParameter(p.Index, _nOcrDbThread, p.Self, p.Increment, _unItalicFactor, checkBoxNOcrItalic.Checked, _numericUpDownPixelsIsSpace, checkBoxRightToLeftNOCR.Checked);
                p.Self.RunWorkerAsync(p);
            }
        }

        private void NOcrThreadDoWork(object sender, DoWorkEventArgs e)
        {
            var p = (NOcrThreadParameter)e.Argument;
            e.Result = p;
            var bmp = GetSubtitleBitmap(p.Index);
            var parentBitmap = new NikseBitmap(bmp);
            bmp.Dispose();
            var minLineHeight = GetMinLineHeight();
            var list = NikseBitmapImageSplitter.SplitBitmapToLettersNew(parentBitmap, p.NumberOfPixelsIsSpace, p.RightToLeft, Configuration.Settings.VobSubOcr.TopToBottom, minLineHeight, _autoLineHeight);
            p.ResultMatches = new List<CompareMatch>();
            int index = 0;
            while (index < list.Count)
            {
                var item = list[index];
                if (item.NikseBitmap == null)
                {
                    p.ResultMatches.Add(new CompareMatch(item.SpecialCharacter, false, 0, null));
                }
                else
                {
                    var match = GetNOcrCompareMatchNew(item, parentBitmap, p.NOcrDb, p.AdvancedItalicDetection, true, index, list);
                    if (match == null)
                    {
                        p.ResultText = string.Empty;
                        return;
                    }

                    p.ResultMatches.Add(new CompareMatch(match.Text, match.Italic, 0, null) { ImageSplitterItem = item });
                    if (match.ExpandCount > 0)
                    {
                        index += match.ExpandCount - 1;
                    }
                }
                index++;
            }
            p.ResultText = MatchesToItalicStringConverter.GetStringWithItalicTags(p.ResultMatches);
        }

        private void CleanLogsGreaterThanOrEqualTo(decimal value)
        {
            var start = (int)Math.Round(value);
            CleanLogGreaterThanOrEqualTo(listBoxUnknownWords, start);
            CleanLogGreaterThanOrEqualTo(listBoxLog, start);
            CleanLogGreaterThanOrEqualTo(listBoxLogSuggestions, start);
        }

        private void CleanLogGreaterThanOrEqualTo(ListBox listBox, int start)
        {
            listBox.BeginUpdate();
            for (int i = listBox.Items.Count - 1; i >= 0; i--)
            {
                var text = listBox.Items[i].ToString();
                var idx = text.IndexOf(':');
                if (idx > 0)
                {
                    var num = text.Substring(0, idx).TrimStart('#');
                    if (int.TryParse(num, out var n) && n >= start)
                    {
                        listBox.Items.RemoveAt(i);
                    }
                }
            }
            listBox.EndUpdate();
        }

        private void InitializeTopAlign()
        {
            _captureTopAlign = toolStripMenuItemCaptureTopAlign.Checked;
            if (_captureTopAlign && _captureTopAlignHeight == -1 && _subtitle.Paragraphs.Count > 2)
            {
                int maxHeight = -1;
                var idxList = new List<int> { 0, 1, _subtitle.Paragraphs.Count / 2, _subtitle.Paragraphs.Count - 1 };
                foreach (var idx in idxList)
                {
                    GetSubtitleTopAndHeight(idx, out _, out var top, out _, out var height);
                    if (top + height > maxHeight)
                    {
                        maxHeight = top + height;
                    }
                }

                _captureTopAlignHeightThird = maxHeight / 3;

                if (maxHeight >= 720)
                {
                    _captureTopAlignHeight = maxHeight / 2 - 10;
                }
                else if (maxHeight > 320)
                {
                    _captureTopAlignHeight = maxHeight / 3;
                }
            }
        }

        private TesseractThreadRunner _tesseractThreadRunner;

        private readonly object _lockObj = new object();
        public void OcrDone(int index, TesseractThreadRunner.ImageJob job)
        {
            if (_abort)
            {
                return;
            }

            _tesseractAsyncStrings[index] = job.Result;
            Application.DoEvents();
            lock (_lockObj)
            {
                var text = OcrViaTesseract(job.Bitmap, index);
                _lastLine = text;

                text = text.Replace("<i>-</i>", "-");
                text = text.Replace("<i>a</i>", "a");
                text = text.Replace("<i>.</i>", ".");
                text = text.Replace("  ", " ");
                text = text.Trim();

                text = text.Replace(" " + Environment.NewLine, Environment.NewLine);
                text = text.Replace(Environment.NewLine + " ", Environment.NewLine);

                // max allow 2 lines
                if (_autoBreakLines && Utilities.GetNumberOfLines(text) > 2)
                {
                    text = text.Replace(" " + Environment.NewLine, Environment.NewLine);
                    text = text.Replace(Environment.NewLine + " ", Environment.NewLine);
                    while (text.Contains(Environment.NewLine + Environment.NewLine))
                    {
                        text = text.Replace(Environment.NewLine + Environment.NewLine, Environment.NewLine);
                    }

                    if (Utilities.GetNumberOfLines(text) > 2)
                    {
                        text = Utilities.AutoBreakLine(text);
                    }
                }

                if (_dvbSubtitles != null && _transportStreamUseColor)
                {
                    if (_dvbSubColor[index] != Color.Transparent)
                    {
                        text = "<font color=\"" + ColorTranslator.ToHtml(_dvbSubColor[index]) + "\">" + text + "</font>";
                    }
                }

                if (_abort)
                {
                    textBoxCurrentText.Text = text;
                    _mainOcrRunning = false;
                    SetButtonsEnabledAfterOcrDone();
                }

                text = text.Trim();
                text = text.Replace("  ", " ");
                text = text.Replace(Environment.NewLine + Environment.NewLine, Environment.NewLine);
                text = text.Replace("  ", " ");
                text = text.Replace(Environment.NewLine + Environment.NewLine, Environment.NewLine);
                text = SetTopAlign(index, text);

                if (index >= subtitleListView1.Items.Count)
                {
                    return;
                }

                var item = subtitleListView1.Items[index];
                item.Selected = true;
                item.EnsureVisible();

                Paragraph p = _subtitle.GetParagraphOrDefault(index);
                if (p != null)
                {
                    p.Text = text;
                }

                if (subtitleListView1.SelectedItems.Count == 1 && subtitleListView1.SelectedItems[0].Index == index)
                {
                    textBoxCurrentText.Text = text;
                }
                else
                {
                    subtitleListView1.SetText(index, text);
                }

                var max = GetSubtitleCount();
                GetSubtitleTime(index, out var startTime, out var endTime);
                labelStatus.Text = $"{index + 1} / {max}: {startTime} - {endTime}";
                progressBar1.Value = index + 1;
                if (ProgressCallback != null)
                {
                    var percent = (int)Math.Round((index + 1) * 100.0 / max);
                    ProgressCallback?.Invoke($"{percent}%");
                }
                labelStatus.Refresh();
                progressBar1.Refresh();

                job.Bitmap.Dispose();
                if (index >= max - 1)
                {
                    SetButtonsEnabledAfterOcrDone();
                    _mainOcrRunning = false;
                }
            }

            Application.DoEvents();
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
            GetSubtitleTime(i, out var startTime, out var endTime);
            labelStatus.Text = $"{i + 1} / {max}: {startTime} - {endTime}";
            progressBar1.Value = i + 1;
            if (ProgressCallback != null)
            {
                var percent = (int)Math.Round((i + 1) * 100.0 / max);
                ProgressCallback?.Invoke($"{percent}%");
            }
            labelStatus.Refresh();
            progressBar1.Refresh();
            if (_abort)
            {
                bmp.Dispose();
                SetButtonsEnabledAfterOcrDone();
                _mainOcrRunning = false;
                return true;
            }

            _mainOcrBitmap = bmp;

            int j = i;
            subtitleListView1.Items[j].Selected = true;
            subtitleListView1.Items[j].Focused = true;
            if (j < max - 1)
            {
                j++;
            }
            if (j < max - 1)
            {
                j++;
            }

            if (i % 3 == 0)
            {
                subtitleListView1.Items[j].EnsureVisible();
            }

            string text = string.Empty;
            if (_ocrMethodIndex == _ocrMethodBinaryImageCompare)
            {
                text = SplitAndOcrBinaryImageCompare(bmp, i);
            }
            else if (_ocrMethodIndex == _ocrMethodNocr)
            {
                text = OcrViaNOCR(bmp, i);
            }
            else if (_ocrMethodIndex == _ocrMethodModi)
            {
                text = CallModi(i);
            }

            _lastLine = text;

            text = text.Replace("<i>-</i>", "-");
            text = text.Replace("<i>a</i>", "a");
            text = text.Replace("<i>.</i>", ".");
            text = text.Replace("<i>,</i>", ",");
            text = text.Replace("  ", " ");
            text = text.Trim();

            text = text.Replace(" " + Environment.NewLine, Environment.NewLine);
            text = text.Replace(Environment.NewLine + " ", Environment.NewLine);

            // max allow 2 lines
            if (_autoBreakLines && Utilities.GetNumberOfLines(text) > 2)
            {
                text = text.Replace(" " + Environment.NewLine, Environment.NewLine);
                text = text.Replace(Environment.NewLine + " ", Environment.NewLine);
                while (text.Contains(Environment.NewLine + Environment.NewLine))
                {
                    text = text.Replace(Environment.NewLine + Environment.NewLine, Environment.NewLine);
                }

                if (Utilities.GetNumberOfLines(text) > 2)
                {
                    text = Utilities.AutoBreakLine(text);
                }
            }

            if (_dvbSubtitles != null && _transportStreamUseColor)
            {
                if (_dvbSubColor[i] != Color.Transparent)
                {
                    text = "<font color=\"" + ColorTranslator.ToHtml(_dvbSubColor[i]) + "\">" + text + "</font>";
                }
            }

            if (_abort)
            {
                textBoxCurrentText.Text = text;
                _mainOcrRunning = false;
                SetButtonsEnabledAfterOcrDone();
                return true;
            }

            text = text.Trim();
            text = text.Replace("  ", " ");
            text = text.Replace(Environment.NewLine + Environment.NewLine, Environment.NewLine);
            text = text.Replace("  ", " ");
            text = text.Replace(Environment.NewLine + Environment.NewLine, Environment.NewLine);
            text = SetTopAlign(i, text);

            Paragraph p = _subtitle.GetParagraphOrDefault(i);
            if (p != null)
            {
                p.Text = text;
            }

            if (subtitleListView1.SelectedItems.Count == 1 && subtitleListView1.SelectedItems[0].Index == i)
            {
                textBoxCurrentText.Text = text;
            }
            else
            {
                subtitleListView1.SetText(i, text);
            }

            return false;
        }

        private string SetTopAlign(int i, string text)
        {
            if (_captureTopAlign && _captureTopAlignHeight > 0)
            {
                GetSubtitleTopAndHeight(i, out _, out var top, out _, out var height);
                if (top + height < _captureTopAlignHeight && top < _captureTopAlignHeightThird)
                {
                    text = "{\\an8}" + text;
                }
            }

            return text;
        }

        private bool MainLoopTesseract(int max, int i)
        {
            if (i >= max)
            {
                _tesseractThreadRunner.CheckQueue();
                return false;
            }

            if (_abort)
            {
                SetButtonsEnabledAfterOcrDone();
                _mainOcrRunning = false;
                return true;
            }

            var bmp = GetSubtitleBitmap(i);
            bool is302 = _ocrMethodIndex == _ocrMethodTesseract302;
            _tesseractThreadRunner.AddImageJob(bmp, i, _languageId, string.Empty, _tesseractEngineMode.ToString(CultureInfo.InvariantCulture), is302, is302 && checkBoxTesseractMusicOn.Checked);
            _tesseractThreadRunner.CheckQueue();
            return false;
        }

        private void mainOcrTimer_Tick(object sender, EventArgs e)
        {
            _mainOcrTimer.Stop();
            bool done = _ocrMethodIndex == _ocrMethodTesseract4 || _ocrMethodIndex == _ocrMethodTesseract302 ? MainLoopTesseract(_mainOcrTimerMax, _mainOcrIndex) : MainLoop(_mainOcrTimerMax, _mainOcrIndex);
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
                _nOcrDb = new NOcrDb(fileName);
            }
        }

        internal void SaveNOcrWithCurrentLanguage()
        {
            SaveNOcr();
        }

        public static Bitmap ResizeBitmap(Bitmap b, int width, int height)
        {
            var result = new Bitmap(width, height);
            using (var g = Graphics.FromImage(result))
            {
                g.DrawImage(b, 0, 0, width, height);
            }

            return result;
        }

        public static Bitmap UnItalic(Bitmap bmp, double factor)
        {
            int left = (int)(bmp.Height * factor);
            var unItaliced = new Bitmap(bmp.Width + left + 4, bmp.Height);
            using (var g = Graphics.FromImage(unItaliced))
            {
                g.DrawImage(bmp, new[]
                {
                    new Point(0, 0), // destination for upper-left point of original
                    new Point(bmp.Width, 0), // destination for upper-right point of original
                    new Point(left, bmp.Height) // destination for lower-left point of original
                });
            }

            return unItaliced;
        }

        private TesseractRunner _tesseractRunner;

        private string Tesseract3DoOcrViaExe(Bitmap bmp, string language, string psmMode, int tesseractEngineMode)
        {
            if (_tesseractRunner == null)
            {
                //_tesseractThreadRunner = new TesseractThreadRunner(OcrDone);
                _tesseractRunner = new TesseractRunner();
            }

            string pngFileName = Path.GetTempPath() + Guid.NewGuid() + ".png";
            bmp.Save(pngFileName, System.Drawing.Imaging.ImageFormat.Png);
            var result = _tesseractRunner.Run(language, psmMode, tesseractEngineMode.ToString(CultureInfo.InvariantCulture), pngFileName, _ocrMethodIndex != _ocrMethodTesseract4);
            if (_tesseractRunner.TesseractErrors.Count <= 2 && !string.IsNullOrEmpty(_tesseractRunner.LastError))
            {
                MessageBox.Show(_tesseractRunner.LastError);
            }

            return result;
        }

        private bool HasSingleLetters(string line)
        {
            if (!_ocrFixEngine.IsDictionaryLoaded || !_ocrFixEngine.SpellCheckDictionaryName.StartsWith("en_", StringComparison.Ordinal))
            {
                return false;
            }

            line = line.RemoveChar('[');
            line = line.RemoveChar(']');
            line = HtmlUtil.RemoveOpenCloseTags(line, HtmlUtil.TagItalic);

            var arr = line.Replace("a.m", string.Empty).Replace("p.m", string.Empty).Replace("o.r", string.Empty)
                .Replace("e.g", string.Empty).Replace("Ph.D", string.Empty).Replace("d.t.s", string.Empty)
                .Split(new[] { ' ', ',', '.', '?', '!', '(', ')', '\r', '\n', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string s in arr)
            {
                if (s.Length == 1 && !@"♪♫-:'”1234567890&aAI""".Contains(s))
                {
                    return true;
                }
            }

            return false;
        }

        private string OcrViaTesseract(Bitmap bitmap, int index)
        {
            if (bitmap == null)
            {
                return string.Empty;
            }

            if (_ocrFixEngine == null)
            {
                comboBoxDictionaries_SelectedIndexChanged(null, null);
                if (_ocrFixEngine == null)
                {
                    _ocrFixEngine = new OcrFixEngine(string.Empty, string.Empty, this, _ocrMethodIndex == _ocrMethodBinaryImageCompare || _ocrMethodIndex == _ocrMethodNocr);
                }
            }

            const int badWords = 0;
            string textWithOutFixes;
            if (!string.IsNullOrEmpty(_tesseractAsyncStrings?[index]))
            {
                textWithOutFixes = _tesseractAsyncStrings[index];
            }
            else
            {
                if (_tesseractAsyncIndex <= index)
                {
                    _tesseractAsyncIndex = index + 10;
                }

                textWithOutFixes = Tesseract3DoOcrViaExe(bitmap, _languageId, "6", _tesseractEngineMode); // 6 = Assume a single uniform block of text.
            }

            if ((!textWithOutFixes.Contains(Environment.NewLine) || Utilities.CountTagInText(textWithOutFixes, '\n') > 2)
                && (textWithOutFixes.Length < 17 || bitmap.Height < 50))
            {
                string psm = Tesseract3DoOcrViaExe(bitmap, _languageId, "7", _tesseractEngineMode); // 7 = Treat the image as a single text line.

                // sometimes short texts are not recognized - this resize seems to help
                if (psm == string.Empty && textWithOutFixes == string.Empty ||
                    psm.Length < 5 && !psm.Contains('.') && psm == psm.ToUpperInvariant()) // e.g. "SEN" (could be more...) - see https://github.com/SubtitleEdit/subtitleedit/issues/3833
                {
                    using (var b = ResizeBitmap(bitmap, bitmap.Width * 2, (int)Math.Round(bitmap.Height * 2.5)))
                    {
                        psm = Tesseract3DoOcrViaExe(b, _languageId, string.Empty, _tesseractEngineMode);
                    }
                }

                if (textWithOutFixes != psm)
                {
                    if (string.IsNullOrWhiteSpace(textWithOutFixes))
                    {
                        textWithOutFixes = psm;
                    }
                    else if (psm.Length > textWithOutFixes.Length)
                    {
                        if (!psm.Contains('9') && textWithOutFixes.Contains('9') ||
                            !psm.Contains('6') && textWithOutFixes.Contains('6') ||
                            !psm.Contains('5') && textWithOutFixes.Contains('5') ||
                            !psm.Contains('3') && textWithOutFixes.Contains('3') ||
                            !psm.Contains('1') && textWithOutFixes.Contains('1') ||
                            !psm.Contains('$') && textWithOutFixes.Contains('$') ||
                            !psm.Contains('•') && textWithOutFixes.Contains('•') ||
                            !psm.Contains('Y') && textWithOutFixes.Contains('Y') ||
                            !psm.Contains('\'') && textWithOutFixes.Contains('\'') ||
                            !psm.Contains('€') && textWithOutFixes.Contains('€'))
                        {
                            textWithOutFixes = psm;
                        }
                        else if (_ocrFixEngine != null && !psm.Contains('$') && !psm.Contains('•') && !psm.Contains('€'))
                        {
                            int wordsNotFoundNoFixes = _ocrFixEngine.CountUnknownWordsViaDictionary(textWithOutFixes, out var correctWordsNoFixes);
                            int wordsNotFoundPsm7 = _ocrFixEngine.CountUnknownWordsViaDictionary(psm, out var correctWordsPsm7);
                            if (wordsNotFoundPsm7 <= wordsNotFoundNoFixes && correctWordsPsm7 > correctWordsNoFixes)
                            {
                                textWithOutFixes = psm;
                            }
                        }
                    }
                    else if (psm.Length == textWithOutFixes.Length &&
                             (!psm.Contains('0') && textWithOutFixes.Contains('0') || // these chars are often mistaken
                              !psm.Contains('9') && textWithOutFixes.Contains('9') ||
                              !psm.Contains('8') && textWithOutFixes.Contains('8') ||
                              !psm.Contains('5') && textWithOutFixes.Contains('5') ||
                              !psm.Contains('3') && textWithOutFixes.Contains('3') ||
                              !psm.Contains('1') && textWithOutFixes.Contains('1') ||
                              !psm.Contains('$') && textWithOutFixes.Contains('$') ||
                              !psm.Contains('€') && textWithOutFixes.Contains('€') ||
                              !psm.Contains('•') && textWithOutFixes.Contains('•') ||
                              !psm.Contains('Y') && textWithOutFixes.Contains('Y') ||
                              !psm.Contains('\'') && textWithOutFixes.Contains('\'') ||
                              !psm.Contains('/') && textWithOutFixes.Contains('/') ||
                              !psm.Contains('(') && textWithOutFixes.Contains('(') ||
                              !psm.Contains(')') && textWithOutFixes.Contains(')') ||
                              !psm.Contains('_') && textWithOutFixes.Contains('_')))
                    {
                        textWithOutFixes = psm;
                    }
                    else if (psm.Length == textWithOutFixes.Length && psm.EndsWith('.') && !textWithOutFixes.EndsWith('.'))
                    {
                        textWithOutFixes = psm;
                    }
                }
            }

            if (!checkBoxTesseractItalicsOn.Checked)
            {
                textWithOutFixes = HtmlUtil.RemoveOpenCloseTags(textWithOutFixes, HtmlUtil.TagItalic);
            }

            // Sometimes Tesseract has problems with small fonts - it helps to make the image larger
            if (HtmlUtil.RemoveOpenCloseTags(textWithOutFixes, HtmlUtil.TagItalic).Replace("@", string.Empty).Replace("%", string.Empty).Replace("|", string.Empty).Trim().Length < 3
                || Utilities.CountTagInText(textWithOutFixes, '\n') > 2)
            {
                string rs = TesseractResizeAndRetry(bitmap);
                textWithOutFixes = rs;
                if (!checkBoxTesseractItalicsOn.Checked)
                {
                    textWithOutFixes = HtmlUtil.RemoveOpenCloseTags(textWithOutFixes, HtmlUtil.TagItalic);
                }
            }

            // fix italics
            textWithOutFixes = FixItalics(textWithOutFixes);

            int numberOfWords = textWithOutFixes.Split(new[] { ' ', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).Length;

            string line = textWithOutFixes.Trim();
            if (_ocrFixEngine.IsDictionaryLoaded)
            {
                if (checkBoxAutoFixCommonErrors.Checked)
                {
                    line = _ocrFixEngine.FixOcrErrors(line, index, _lastLine, true, GetAutoGuessLevel());
                }

                int wordsNotFound = _ocrFixEngine.CountUnknownWordsViaDictionary(line, out int correctWords);
                int oldCorrectWords = correctWords;

                if (wordsNotFound > 0 || correctWords == 0)
                {
                    var oldUnkownWords = new List<LogItem>();
                    oldUnkownWords.AddRange(_ocrFixEngine.UnknownWordsFound);
                    _ocrFixEngine.UnknownWordsFound.Clear();

                    string newUnfixedText = TesseractResizeAndRetry(bitmap);
                    string newText = _ocrFixEngine.FixOcrErrors(newUnfixedText, index, _lastLine, true, GetAutoGuessLevel());
                    int newWordsNotFound = _ocrFixEngine.CountUnknownWordsViaDictionary(newText, out correctWords);

                    if (newWordsNotFound >= wordsNotFound && oldCorrectWords >= correctWords &&
                        checkBoxTesseractFallback.Visible && checkBoxTesseractFallback.Checked)
                    {
                        var oldOcrMethodIndex = _ocrMethodIndex;
                        _ocrMethodIndex = _ocrMethodIndex == _ocrMethodTesseract4 ? _ocrMethodTesseract302 : _ocrMethodTesseract4;
                        newUnfixedText = Tesseract3DoOcrViaExe(bitmap, _languageId, "6", _tesseractEngineMode); // 6 = Assume a single uniform block of text.
                        newText = _ocrFixEngine.FixOcrErrors(newUnfixedText, index, _lastLine, true, GetAutoGuessLevel());
                        newWordsNotFound = _ocrFixEngine.CountUnknownWordsViaDictionary(newText, out correctWords);
                        _ocrMethodIndex = oldOcrMethodIndex;
                    }

                    if (wordsNotFound == 1 && newWordsNotFound == 1 && newUnfixedText.EndsWith("!!", StringComparison.Ordinal) && textWithOutFixes.EndsWith('u') && newText.Length > 1)
                    {
                        _ocrFixEngine.UnknownWordsFound.Clear();
                        newText = textWithOutFixes.Substring(0, textWithOutFixes.Length - 1) + "!!";
                        newWordsNotFound = _ocrFixEngine.CountUnknownWordsViaDictionary(newText, out correctWords);
                    }
                    else if (correctWords >= oldCorrectWords &&
                             (!newText.Contains('9') || textWithOutFixes.Contains('9')) &&
                             (!newText.Replace("</i>", string.Empty).Contains('/') || textWithOutFixes.Replace("</i>", string.Empty).Contains('/')) &&
                             !string.IsNullOrWhiteSpace(newUnfixedText) &&
                             newWordsNotFound < wordsNotFound || (newWordsNotFound == wordsNotFound && newText.EndsWith('!') && textWithOutFixes.EndsWith('l')))
                    {
                        wordsNotFound = newWordsNotFound;
                        if (textWithOutFixes.Length > 3 && textWithOutFixes.EndsWith("...", StringComparison.Ordinal) && !newText.EndsWith('.') && !newText.EndsWith(',') && !newText.EndsWith('!') &&
                            !newText.EndsWith('?') && !newText.EndsWith("</i>", StringComparison.Ordinal))
                        {
                            newText = newText.TrimEnd() + "...";
                        }
                        else if (textWithOutFixes.Length > 0 && textWithOutFixes.EndsWith('.') && !newText.EndsWith('.') && !newText.EndsWith(',') && !newText.EndsWith('!') &&
                                 !newText.EndsWith('?') && !newText.EndsWith("</i>", StringComparison.Ordinal))
                        {
                            newText = newText.TrimEnd() + ".";
                        }
                        else if (textWithOutFixes.Length > 0 && textWithOutFixes.EndsWith('?') && !newText.EndsWith('.') && !newText.EndsWith(',') && !newText.EndsWith('!') &&
                                 !newText.EndsWith('?') && !newText.EndsWith("</i>", StringComparison.Ordinal))
                        {
                            newText = newText.TrimEnd() + "?";
                        }

                        textWithOutFixes = newUnfixedText;
                        line = FixItalics(newText);
                    }
                    else if (correctWords > oldCorrectWords + 1 || (correctWords > oldCorrectWords && !textWithOutFixes.Contains(' ')))
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

                if (wordsNotFound > 0 || correctWords == 0 || textWithOutFixes != null && textWithOutFixes.Replace("~", string.Empty).Trim().Length < 2)
                {
                    if (_bluRaySubtitles != null && !line.Contains("<i>"))
                    {
                        _ocrFixEngine.AutoGuessesUsed.Clear();
                        _ocrFixEngine.UnknownWordsFound.Clear();

                        // which is best - normal image or one color image?
                        var nbmp = new NikseBitmap(bitmap);
                        nbmp.MakeOneColor(Color.White);
                        Bitmap oneColorBitmap = nbmp.GetBitmap();
                        string oneColorText = Tesseract3DoOcrViaExe(oneColorBitmap, _languageId, "6", _tesseractEngineMode); // 6 = Assume a single uniform block of text.
                        oneColorBitmap.Dispose();

                        if (oneColorText.Length > 1 &&
                            !oneColorText.Contains("CD") &&
                            (!oneColorText.Contains('0') || line.Contains('0')) &&
                            (!oneColorText.Contains('2') || line.Contains('2')) &&
                            (!oneColorText.Contains('3') || line.Contains('4')) &&
                            (!oneColorText.Contains('5') || line.Contains('5')) &&
                            (!oneColorText.Contains('9') || line.Contains('9')) &&
                            (!oneColorText.Contains('•') || line.Contains('•')) &&
                            (!oneColorText.Contains(')') || line.Contains(')')) &&
                            Utilities.CountTagInText(oneColorText, '(') < 2 && Utilities.CountTagInText(oneColorText, ')') < 2 &&
                            Utilities.GetNumberOfLines(oneColorText) < 4)
                        {
                            int modiWordsNotFound = _ocrFixEngine.CountUnknownWordsViaDictionary(oneColorText, out var modiCorrectWords);
                            string modiTextOcrFixed = oneColorText;
                            if (checkBoxAutoFixCommonErrors.Checked)
                            {
                                modiTextOcrFixed = _ocrFixEngine.FixOcrErrors(oneColorText, index, _lastLine, false, GetAutoGuessLevel());
                            }

                            int modiOcrCorrectedWordsNotFound = _ocrFixEngine.CountUnknownWordsViaDictionary(modiTextOcrFixed, out var modiOcrCorrectedCorrectWords);
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
                                {
                                    line = _ocrFixEngine.FixOcrErrors(line, index, _lastLine, true, GetAutoGuessLevel());
                                }
                            }
                            else if (wordsNotFound == modiWordsNotFound && oneColorText.EndsWith('!') && (line.EndsWith('l') || line.EndsWith('ﬂ')))
                            {
                                line = FixItalics(oneColorText);
                                wordsNotFound = modiWordsNotFound;
                                correctWords = modiCorrectWords;
                                if (checkBoxAutoFixCommonErrors.Checked)
                                {
                                    line = _ocrFixEngine.FixOcrErrors(line, index, _lastLine, true, GetAutoGuessLevel());
                                }
                            }
                        }
                    }
                }

                if (checkBoxTesseractItalicsOn.Checked)
                {
                    if (line.Contains("<i>") || wordsNotFound > 0 || correctWords == 0 || textWithOutFixes != null && textWithOutFixes.Replace("~", string.Empty).Trim().Length < 2)
                    {
                        _ocrFixEngine.AutoGuessesUsed.Clear();
                        _ocrFixEngine.UnknownWordsFound.Clear();

                        // which is best - normal image or de-italic'ed? We find out here
                        var unItalicedBmp = UnItalic(bitmap, _unItalicFactor);
                        string unItalicText = Tesseract3DoOcrViaExe(unItalicedBmp, _languageId, "6", _tesseractEngineMode); // 6 = Assume a single uniform block of text.
                        unItalicedBmp.Dispose();

                        if (unItalicText.Replace("<i>", string.Empty).Replace("</i>", string.Empty).Length > 1)
                        {
                            int modiCorrectWords;
                            int modiWordsNotFound = _ocrFixEngine.CountUnknownWordsViaDictionary(unItalicText, out modiCorrectWords);
                            string modiTextOcrFixed = unItalicText;
                            if (checkBoxAutoFixCommonErrors.Checked)
                            {
                                modiTextOcrFixed = _ocrFixEngine.FixOcrErrors(unItalicText, index, _lastLine, false, GetAutoGuessLevel());
                            }

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
                            {
                                ok = wordsNotFound == modiWordsNotFound && unItalicText.EndsWith('!') && (line.EndsWith('l') || line.EndsWith('ﬂ'));
                            }

                            if (!ok)
                            {
                                ok = wordsNotFound == modiWordsNotFound && line.StartsWith("<i>", StringComparison.Ordinal) && line.EndsWith("</i>", StringComparison.Ordinal);
                            }

                            if (ok && Utilities.CountTagInText(unItalicText, '/') > Utilities.CountTagInText(line, '/') + 1)
                            {
                                ok = false;
                            }

                            if (ok && Utilities.CountTagInText(unItalicText, '\\') > Utilities.CountTagInText(line, '\\'))
                            {
                                ok = false;
                            }

                            if (ok && Utilities.CountTagInText(unItalicText, ')') > Utilities.CountTagInText(line, ')') + 1)
                            {
                                ok = false;
                            }

                            if (ok && Utilities.CountTagInText(unItalicText, '(') > Utilities.CountTagInText(line, '(') + 1)
                            {
                                ok = false;
                            }

                            if (ok && Utilities.CountTagInText(unItalicText, '$') > Utilities.CountTagInText(line, '$') + 1)
                            {
                                ok = false;
                            }

                            if (ok && Utilities.CountTagInText(unItalicText, '€') > Utilities.CountTagInText(line, '€') + 1)
                            {
                                ok = false;
                            }

                            if (ok && Utilities.CountTagInText(unItalicText, '•') > Utilities.CountTagInText(line, '•'))
                            {
                                ok = false;
                            }

                            if (ok)
                            {
                                wordsNotFound = modiWordsNotFound;
                                correctWords = modiCorrectWords;

                                line = HtmlUtil.RemoveOpenCloseTags(line, HtmlUtil.TagItalic).Trim();

                                if (line.Length > 7 && unItalicText.Length > 7 && unItalicText.StartsWith("I ", StringComparison.Ordinal) &&
                                    line.StartsWith(unItalicText.Remove(0, 2).Substring(0, 4), StringComparison.Ordinal))
                                {
                                    unItalicText = unItalicText.Remove(0, 2);
                                }

                                if (checkBoxTesseractMusicOn.Checked)
                                {
                                    if ((line.StartsWith("J' ", StringComparison.Ordinal) || line.StartsWith("J“ ", StringComparison.Ordinal) || line.StartsWith("J* ", StringComparison.CurrentCulture) || line.StartsWith("♪ ", StringComparison.Ordinal)) && unItalicText.Length > 3 && HtmlUtil.RemoveOpenCloseTags(unItalicText, HtmlUtil.TagItalic).Substring(1, 2) == "' ")
                                    {
                                        unItalicText = "♪ " + unItalicText.Remove(0, 2).TrimStart();
                                    }

                                    if ((line.StartsWith("J' ", StringComparison.Ordinal) || line.StartsWith("J“ ", StringComparison.Ordinal) || line.StartsWith("J* ", StringComparison.Ordinal) || line.StartsWith("♪ ", StringComparison.Ordinal)) && unItalicText.Length > 3 && HtmlUtil.RemoveOpenCloseTags(unItalicText, HtmlUtil.TagItalic)[1] == ' ')
                                    {
                                        bool ita = unItalicText.StartsWith("<i>", StringComparison.Ordinal) && unItalicText.EndsWith("</i>", StringComparison.Ordinal);
                                        unItalicText = HtmlUtil.RemoveHtmlTags(unItalicText);
                                        unItalicText = "♪ " + unItalicText.Remove(0, 2).TrimStart();
                                        if (ita)
                                        {
                                            unItalicText = "<i>" + unItalicText + "</i>";
                                        }
                                    }

                                    if ((line.StartsWith("J' ", StringComparison.Ordinal) || line.StartsWith("J“ ", StringComparison.Ordinal) || line.StartsWith("J* ", StringComparison.Ordinal) || line.StartsWith("♪ ", StringComparison.Ordinal)) && unItalicText.Length > 3 && HtmlUtil.RemoveOpenCloseTags(unItalicText, HtmlUtil.TagItalic)[2] == ' ')
                                    {
                                        bool ita = unItalicText.StartsWith("<i>", StringComparison.Ordinal) && unItalicText.EndsWith("</i>", StringComparison.Ordinal);
                                        unItalicText = HtmlUtil.RemoveHtmlTags(unItalicText);
                                        unItalicText = "♪ " + unItalicText.Remove(0, 2).TrimStart();
                                        if (ita)
                                        {
                                            unItalicText = "<i>" + unItalicText + "</i>";
                                        }
                                    }

                                    if (unItalicText.StartsWith("J'", StringComparison.Ordinal) && (line.StartsWith('♪') || textWithOutFixes.StartsWith('♪') || textWithOutFixes.StartsWith("<i>♪", StringComparison.Ordinal) || unItalicText.EndsWith('♪')))
                                    {
                                        bool ita = unItalicText.StartsWith("<i>", StringComparison.Ordinal) && unItalicText.EndsWith("</i>", StringComparison.Ordinal);
                                        unItalicText = HtmlUtil.RemoveHtmlTags(unItalicText);
                                        unItalicText = "♪ " + unItalicText.Remove(0, 2).TrimStart();
                                        if (ita)
                                        {
                                            unItalicText = "<i>" + unItalicText + "</i>";
                                        }
                                    }

                                    if ((line.StartsWith("J` ", StringComparison.Ordinal) || line.StartsWith("J“ ", StringComparison.Ordinal) || line.StartsWith("J' ", StringComparison.Ordinal) || line.StartsWith("J* ", StringComparison.Ordinal)) && unItalicText.StartsWith("S ", StringComparison.Ordinal))
                                    {
                                        bool ita = unItalicText.StartsWith("<i>", StringComparison.Ordinal) && unItalicText.EndsWith("</i>", StringComparison.Ordinal);
                                        unItalicText = HtmlUtil.RemoveHtmlTags(unItalicText);
                                        unItalicText = "♪ " + unItalicText.Remove(0, 2).TrimStart();
                                        if (ita)
                                        {
                                            unItalicText = "<i>" + unItalicText + "</i>";
                                        }
                                    }

                                    if ((line.StartsWith("J` ", StringComparison.Ordinal) || line.StartsWith("J“ ", StringComparison.Ordinal) || line.StartsWith("J' ", StringComparison.Ordinal) || line.StartsWith("J* ", StringComparison.Ordinal)) && unItalicText.StartsWith("<i>S</i> ", StringComparison.Ordinal))
                                    {
                                        bool ita = unItalicText.StartsWith("<i>", StringComparison.Ordinal) && unItalicText.EndsWith("</i>", StringComparison.Ordinal);
                                        unItalicText = HtmlUtil.RemoveHtmlTags(unItalicText);
                                        unItalicText = "♪ " + unItalicText.Remove(0, 8).TrimStart();
                                        if (ita)
                                        {
                                            unItalicText = "<i>" + unItalicText + "</i>";
                                        }
                                    }

                                    if (unItalicText.StartsWith(";'", StringComparison.Ordinal) && (line.StartsWith('♪') || textWithOutFixes.StartsWith('♪') || textWithOutFixes.StartsWith("<i>♪", StringComparison.Ordinal) || unItalicText.EndsWith('♪')))
                                    {
                                        bool ita = unItalicText.StartsWith("<i>", StringComparison.Ordinal) && unItalicText.EndsWith("</i>", StringComparison.Ordinal);
                                        unItalicText = HtmlUtil.RemoveHtmlTags(unItalicText);
                                        unItalicText = "♪ " + unItalicText.Remove(0, 2).TrimStart();
                                        if (ita)
                                        {
                                            unItalicText = "<i>" + unItalicText + "</i>";
                                        }
                                    }

                                    if (unItalicText.StartsWith(",{*", StringComparison.Ordinal) && (line.StartsWith('♪') || textWithOutFixes.StartsWith('♪') || textWithOutFixes.StartsWith("<i>♪", StringComparison.Ordinal) || unItalicText.EndsWith('♪')))
                                    {
                                        bool ita = unItalicText.StartsWith("<i>", StringComparison.Ordinal) && unItalicText.EndsWith("</i>", StringComparison.Ordinal);
                                        unItalicText = HtmlUtil.RemoveHtmlTags(unItalicText);
                                        unItalicText = "♪ " + unItalicText.Remove(0, 3).TrimStart();
                                        if (ita)
                                        {
                                            unItalicText = "<i>" + unItalicText + "</i>";
                                        }
                                    }

                                    if (unItalicText.EndsWith("J'", StringComparison.Ordinal) && (line.EndsWith('♪') || textWithOutFixes.EndsWith('♪') || textWithOutFixes.EndsWith("♪</i>", StringComparison.Ordinal) || unItalicText.StartsWith('♪')))
                                    {
                                        bool ita = unItalicText.StartsWith("<i>", StringComparison.Ordinal) && unItalicText.EndsWith("</i>", StringComparison.Ordinal);
                                        unItalicText = HtmlUtil.RemoveHtmlTags(unItalicText);
                                        unItalicText = unItalicText.Remove(unItalicText.Length - 3, 2).TrimEnd() + " ♪";
                                        if (ita)
                                        {
                                            unItalicText = "<i>" + unItalicText + "</i>";
                                        }
                                    }
                                }

                                if (unItalicText.StartsWith('[') && !line.StartsWith('['))
                                {
                                    unItalicText = unItalicText.Remove(0, 1);
                                    if (unItalicText.EndsWith(']'))
                                    {
                                        unItalicText = unItalicText.TrimEnd(']');
                                    }
                                }

                                if (unItalicText.StartsWith('{') && !line.StartsWith('{'))
                                {
                                    unItalicText = unItalicText.Remove(0, 1);
                                    if (unItalicText.EndsWith('}'))
                                    {
                                        unItalicText = unItalicText.TrimEnd('}');
                                    }
                                }

                                if (unItalicText.EndsWith('}') && !line.EndsWith('}'))
                                {
                                    unItalicText = unItalicText.TrimEnd('}');
                                }

                                if (line.EndsWith("...", StringComparison.Ordinal) && unItalicText.EndsWith("”!", StringComparison.Ordinal))
                                {
                                    unItalicText = unItalicText.TrimEnd('!').TrimEnd('”') + ".";
                                }

                                if (line.EndsWith("...", StringComparison.Ordinal) && unItalicText.EndsWith("\"!", StringComparison.Ordinal))
                                {
                                    unItalicText = unItalicText.TrimEnd('!').TrimEnd('"') + ".";
                                }

                                if (line.EndsWith('.') && !unItalicText.EndsWith('.') && !unItalicText.EndsWith(".</i>", StringComparison.Ordinal))
                                {
                                    string post = string.Empty;
                                    if (unItalicText.EndsWith("</i>", StringComparison.Ordinal))
                                    {
                                        post = "</i>";
                                        unItalicText = unItalicText.Remove(unItalicText.Length - 4);
                                    }

                                    if (unItalicText.EndsWith('\'') && !line.EndsWith("'.", StringComparison.Ordinal))
                                    {
                                        unItalicText = unItalicText.TrimEnd('\'');
                                    }

                                    unItalicText += "." + post;
                                }

                                if (unItalicText.EndsWith('.') && !unItalicText.EndsWith("...", StringComparison.Ordinal) && !unItalicText.EndsWith("...</i>", StringComparison.Ordinal) && line.EndsWith("...", StringComparison.Ordinal))
                                {
                                    string post = string.Empty;
                                    if (unItalicText.EndsWith("</i>", StringComparison.Ordinal))
                                    {
                                        post = "</i>";
                                        unItalicText = unItalicText.Remove(unItalicText.Length - 4);
                                    }

                                    unItalicText += ".." + post;
                                }

                                if (unItalicText.EndsWith("..", StringComparison.Ordinal) && !unItalicText.EndsWith("...", StringComparison.Ordinal) && !unItalicText.EndsWith("...</i>", StringComparison.Ordinal) && line.EndsWith("...", StringComparison.Ordinal))
                                {
                                    string post = string.Empty;
                                    if (unItalicText.EndsWith("</i>", StringComparison.Ordinal))
                                    {
                                        post = "</i>";
                                        unItalicText = unItalicText.Remove(unItalicText.Length - 4);
                                    }

                                    unItalicText += "." + post;
                                }

                                if (line.EndsWith('!') && !unItalicText.EndsWith('!') && !unItalicText.EndsWith("!</i>", StringComparison.Ordinal))
                                {
                                    if (unItalicText.EndsWith("!'", StringComparison.Ordinal))
                                    {
                                        unItalicText = unItalicText.TrimEnd('\'');
                                    }
                                    else
                                    {
                                        if (unItalicText.EndsWith("l</i>", StringComparison.Ordinal) && _ocrFixEngine != null)
                                        {
                                            string w = unItalicText.Substring(0, unItalicText.Length - 4);
                                            int wIdx = w.Length - 1;
                                            while (wIdx >= 0 && !@" .,!?<>:;'-$@£()[]<>/""".Contains(w[wIdx]))
                                            {
                                                wIdx--;
                                            }

                                            if (wIdx + 1 < w.Length && unItalicText.Length > 5)
                                            {
                                                w = w.Substring(wIdx + 1);
                                                if (!_ocrFixEngine.DoSpell(w))
                                                {
                                                    unItalicText = unItalicText.Remove(unItalicText.Length - 5, 1);
                                                }
                                            }

                                            unItalicText = unItalicText.Insert(unItalicText.Length - 4, "!");
                                        }
                                        else if (unItalicText.EndsWith('l') && _ocrFixEngine != null)
                                        {
                                            string w = unItalicText;
                                            int wIdx = w.Length - 1;
                                            while (wIdx >= 0 && !@" .,!?<>:;'-$@£()[]<>/""".Contains(w[wIdx]))
                                            {
                                                wIdx--;
                                            }

                                            if (wIdx + 1 < w.Length && unItalicText.Length > 5)
                                            {
                                                w = w.Substring(wIdx + 1);
                                                if (!_ocrFixEngine.DoSpell(w))
                                                {
                                                    unItalicText = unItalicText.Remove(unItalicText.Length - 1, 1);
                                                }
                                            }

                                            unItalicText += "!";
                                        }
                                        else
                                        {
                                            unItalicText += "!";
                                        }
                                    }
                                }

                                if (line.EndsWith('?') && !unItalicText.EndsWith('?') && !unItalicText.EndsWith("?</i>", StringComparison.Ordinal))
                                {
                                    if (unItalicText.EndsWith("?'", StringComparison.Ordinal))
                                    {
                                        unItalicText = unItalicText.TrimEnd('\'');
                                    }
                                    else
                                    {
                                        unItalicText += "?";
                                    }
                                }

                                line = HtmlUtil.RemoveOpenCloseTags(unItalicText, HtmlUtil.TagItalic);
                                if (checkBoxAutoFixCommonErrors.Checked)
                                {
                                    if (line.Contains("'.") && !textWithOutFixes.Contains("'.") && textWithOutFixes.Contains(':') && !line.EndsWith("'.", StringComparison.Ordinal) && Configuration.Settings.Tools.OcrFixUseHardcodedRules)
                                    {
                                        line = line.Replace("'.", ":");
                                    }

                                    line = _ocrFixEngine.FixOcrErrors(line, index, _lastLine, true, GetAutoGuessLevel());
                                }

                                line = "<i>" + line + "</i>";
                            }
                            else
                            {
                                unItalicText = unItalicText.Replace("</i>", string.Empty);
                                if (line.EndsWith("</i>", StringComparison.Ordinal) && unItalicText.EndsWith('.'))
                                {
                                    line = line.Remove(line.Length - 4, 4);
                                    if (line.EndsWith('-'))
                                    {
                                        line = line.TrimEnd('-') + ".";
                                    }

                                    if (char.IsLetter(line[line.Length - 1]))
                                    {
                                        line += ".";
                                    }

                                    line += "</i>";
                                }
                            }
                        }
                    }
                }

                if (checkBoxTesseractMusicOn.Checked)
                {
                    if (line == "[J'J'J~]" || line == "[J'J'J']")
                    {
                        line = "[ ♪ ♪ ♪ ]";
                    }

                    line = line.Replace(" J' ", " ♪ ");

                    if (line.StartsWith("J'", StringComparison.Ordinal))
                    {
                        line = "♪ " + line.Remove(0, 2).TrimStart();
                    }

                    if (line.StartsWith("<i>J'", StringComparison.Ordinal))
                    {
                        line = "<i>♪ " + line.Remove(0, 5).TrimStart();
                    }

                    if (line.StartsWith("[J'", StringComparison.Ordinal))
                    {
                        line = "[♪ " + line.Remove(0, 3).TrimStart();
                    }

                    if (line.StartsWith("<i>[J'", StringComparison.Ordinal))
                    {
                        line = "<i>[♪ " + line.Remove(0, 6).TrimStart();
                    }

                    if (line.EndsWith("J'", StringComparison.Ordinal))
                    {
                        line = line.Remove(line.Length - 2, 2).TrimEnd() + " ♪";
                    }

                    if (line.EndsWith("J'</i>", StringComparison.Ordinal))
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

                if (wordsNotFound > 0 || correctWords == 0 || textWithOutFixes != null && textWithOutFixes.Replace("~", string.Empty).Trim().Length < 2)
                {
                    _ocrFixEngine.AutoGuessesUsed.Clear();
                    _ocrFixEngine.UnknownWordsFound.Clear();
                    if (subtitleListView1.SelectedItems.Count != 1 || subtitleListView1.SelectedItems[0].Index != index)
                    {
                        subtitleListView1.SelectIndexAndEnsureVisible(index);
                    }
                    line = _ocrFixEngine.FixUnknownWordsViaGuessOrPrompt(out wordsNotFound, line, index, bitmap, checkBoxAutoFixCommonErrors.Checked, checkBoxPromptForUnknownWords.Checked, true, GetAutoGuessLevel());
                }

                if (_ocrFixEngine.Abort)
                {
                    ButtonStopClick(null, null);
                    _ocrFixEngine.Abort = false;
                    return string.Empty;
                }

                // Log used word guesses (via word replace list)
                foreach (var guess in _ocrFixEngine.AutoGuessesUsed)
                {
                    listBoxLogSuggestions.Items.Add(guess);
                }

                _ocrFixEngine.AutoGuessesUsed.Clear();

                // Log unkown words guess (found via spelling dictionaries)
                LogUnknownWords();

                ColorLineByNumberOfUnknownWords(index, wordsNotFound, line);
            }
            else
            { // no dictionary :(
                if (checkBoxAutoFixCommonErrors.Checked)
                {
                    line = _ocrFixEngine.FixOcrErrors(line, index, _lastLine, true, GetAutoGuessLevel());
                }

                ColorLineByNumberOfUnknownWords(index, badWords, line);
            }

            if (textWithOutFixes.Trim() != line.Trim())
            {
                _tesseractOcrAutoFixes++;
                labelFixesMade.Text = $" - {_tesseractOcrAutoFixes}";
                LogOcrFix(index, textWithOutFixes, line);
            }

            if (_vobSubMergedPackList != null)
            {
                bitmap.Dispose();
            }

            return line;
        }

        private static string FixItalics(string input)
        {
            int italicStartCount = Utilities.CountTagInText(input, "<i>");
            if (italicStartCount == 0)
            {
                return input;
            }

            var s = input.Replace(Environment.NewLine + " ", Environment.NewLine);
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
            {
                s = s.Replace("</i> / <i>", " I ").Replace("  ", " ");
            }

            if (s.StartsWith("/ <i>", StringComparison.Ordinal))
            {
                s = ("<i>I " + s.Remove(0, 5)).Replace("  ", " ");
            }

            if (s.StartsWith("I <i>", StringComparison.Ordinal))
            {
                s = ("<i>I " + s.Remove(0, 5)).Replace("  ", " ");
            }
            else if (italicStartCount == 1 && s.Length > 20 &&
                     s.IndexOf("<i>", StringComparison.Ordinal) > 1 && s.IndexOf("<i>", StringComparison.Ordinal) < 10 && s.EndsWith("</i>", StringComparison.Ordinal))
            {
                s = "<i>" + HtmlUtil.RemoveOpenCloseTags(s, HtmlUtil.TagItalic) + "</i>";
            }

            s = s.Replace("</i>" + Environment.NewLine + "<i>", Environment.NewLine);

            s = s.Replace("</i> a <i>", " a ");

            return HtmlUtil.FixInvalidItalicTags(s);
        }

        private void LogUnknownWords()
        {
            foreach (var unknownWord in _ocrFixEngine.UnknownWordsFound)
            {
                listBoxUnknownWords.Items.Add(unknownWord);
            }

            _ocrFixEngine.UnknownWordsFound.Clear();
        }

        private string TesseractResizeAndRetry(Bitmap bitmap)
        {
            string result;
            using (var b = ResizeBitmap(bitmap, bitmap.Width * 3, bitmap.Height * 2))
            {
                result = Tesseract3DoOcrViaExe(b, _languageId, null, _tesseractEngineMode);
            }

            if (string.IsNullOrWhiteSpace(result))
            {
                using (var b = ResizeBitmap(bitmap, bitmap.Width * 4, bitmap.Height * 2))
                {
                    result = Tesseract3DoOcrViaExe(b, _languageId, "7", _tesseractEngineMode);
                }
            }

            return result.TrimEnd();
        }

        private void LogOcrFix(int index, string oldLine, string newLine)
        {
            listBoxLog.Items.Add(new OcrFix(index, oldLine, newLine));
        }

        private string CallModi(int i)
        {
            Bitmap bmp;
            try
            {
                var tmp = GetSubtitleBitmap(i);
                if (tmp == null)
                {
                    return string.Empty;
                }

                bmp = tmp.Clone() as Bitmap;
                tmp.Dispose();
            }
            catch
            {
                return string.Empty;
            }

            var mp = new ModiParameter { Bitmap = bmp, Text = "", Language = GetModiLanguage() };

            // We call in a separate thread... or app will crash sometimes :(
            var modiThread = new System.Threading.Thread(DoWork);
            modiThread.Start(mp);
            modiThread.Join(3000); // wait max 3 seconds
            modiThread.Abort();

            if (!string.IsNullOrEmpty(mp.Text) && mp.Text.Length > 3 && mp.Text.EndsWith(";0]", StringComparison.Ordinal))
            {
                mp.Text = mp.Text.Substring(0, mp.Text.Length - 3);
            }

            // Try to avoid blank lines by resizing image
            if (string.IsNullOrEmpty(mp.Text))
            {
                bmp = ResizeBitmap(bmp, (int)(bmp.Width * 1.2), (int)(bmp.Height * 1.2));
                mp = new ModiParameter { Bitmap = bmp, Text = "", Language = GetModiLanguage() };

                // We call in a separate thread... or app will crash sometimes :(
                modiThread = new System.Threading.Thread(DoWork);
                modiThread.Start(mp);
                modiThread.Join(3000); // wait max 3 seconds
                modiThread.Abort();
            }

            int k = 0;
            while (string.IsNullOrEmpty(mp.Text) && k < 5)
            {
                if (string.IsNullOrEmpty(mp.Text))
                {
                    bmp = ResizeBitmap(bmp, (int)(bmp.Width * 1.3), (int)(bmp.Height * 1.4)); // a bit scaling
                    mp = new ModiParameter { Bitmap = bmp, Text = "", Language = GetModiLanguage() };

                    // We call in a separate thread... or app will crash sometimes :(
                    modiThread = new System.Threading.Thread(DoWork);
                    modiThread.Start(mp);
                    modiThread.Join(3000); // wait max 3 seconds
                    modiThread.Abort();
                    k++;
                }
            }

            bmp?.Dispose();

            if (mp.Text != null)
            {
                mp.Text = mp.Text.Replace("•", "o");
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
                // ignored
            }

            if (ocrResult != null)
            {
                paramter.Text = ocrResult.ToString().Trim();
            }
        }

        private void InitializeModi()
        {
            _modiEnabled = false;
            comboBoxModiLanguage.Enabled = false;

            if (!Configuration.IsRunningOnWindows)
            {
                return;
            }

            try
            {
                InitializeModiLanguages();

                _modiType = Type.GetTypeFromProgID("MODI.Document");
                _modiDoc = Activator.CreateInstance(_modiType);

                _modiEnabled = _modiDoc != null;
                comboBoxModiLanguage.Enabled = _modiEnabled;
            }
            catch
            {
                _modiEnabled = false;
            }
        }

        private void InitializeNOcrForBatch(string db)
        {
            _ocrMethodIndex = _ocrMethodNocr;
            checkBoxNOcrDrawUnknownLetters.Checked = false;
            var fileName = string.Empty;
            if (!string.IsNullOrEmpty(db))
            {
                fileName = Path.Combine(Configuration.OcrDirectory, db);
                if (!fileName.EndsWith(".nocr", StringComparison.OrdinalIgnoreCase))
                {
                    fileName += ".nocr";
                }

                if (!File.Exists(fileName))
                {
                    fileName = string.Empty;
                }
            }

            if (string.IsNullOrEmpty(fileName))
            {
                fileName = Configuration.Settings.VobSubOcr.LineOcrLastLanguages;
                if (string.IsNullOrEmpty(fileName))
                {
                    fileName = "Latin";
                }

                if (!string.IsNullOrEmpty(fileName))
                {
                    fileName = Path.Combine(Configuration.OcrDirectory, Configuration.Settings.VobSubOcr.LineOcrLastLanguages + ".nocr");
                }
            }

            _nOcrDb = new NOcrDb(fileName);
            InitializeNOcrThreads(GetSubtitleCount());
        }

        private void InitializeTesseract(string chosenLanguage = null)
        {
            if (Configuration.IsRunningOnWindows &&
                !Directory.Exists(Configuration.Tesseract302Directory) &&
                Directory.Exists(Configuration.TesseractOriginalDirectory))
            {
                foreach (string dirPath in Directory.GetDirectories(Configuration.TesseractOriginalDirectory, "*", SearchOption.AllDirectories))
                {
                    Directory.CreateDirectory(dirPath.Replace(Configuration.TesseractOriginalDirectory, Configuration.Tesseract302Directory));
                }

                foreach (string newPath in Directory.GetFiles(Configuration.TesseractOriginalDirectory, "*.*", SearchOption.AllDirectories))
                {
                    File.Copy(newPath, newPath.Replace(Configuration.TesseractOriginalDirectory, Configuration.Tesseract302Directory), true);
                }
            }

            string dir = _ocrMethodIndex == _ocrMethodTesseract302 ? Configuration.Tesseract302DataDirectory : Configuration.TesseractDataDirectory;
            if (Directory.Exists(dir))
            {
                comboBoxTesseractLanguages.Items.Clear();
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
                        comboBoxTesseractLanguages.Items.Add(new TesseractLanguage { Id = tesseractName, Text = cultureName });
                    }
                }
            }

            if (comboBoxTesseractLanguages.Items.Count > 0)
            {
                for (int i = 0; i < comboBoxTesseractLanguages.Items.Count; i++)
                {
                    if (chosenLanguage != null && chosenLanguage == (comboBoxTesseractLanguages.Items[i] as TesseractLanguage).Text)
                    {
                        comboBoxTesseractLanguages.SelectedIndex = i;
                        break;
                    }

                    if (chosenLanguage != null && chosenLanguage == (comboBoxTesseractLanguages.Items[i] as TesseractLanguage).Id)
                    {
                        comboBoxTesseractLanguages.SelectedIndex = i;
                        break;
                    }

                    if ((comboBoxTesseractLanguages.Items[i] as TesseractLanguage).Id == Configuration.Settings.VobSubOcr.TesseractLastLanguage)
                    {
                        comboBoxTesseractLanguages.SelectedIndex = i;
                    }
                }

                if (comboBoxTesseractLanguages.SelectedIndex == -1)
                {
                    comboBoxTesseractLanguages.SelectedIndex = 0;
                }
            }

            if (string.IsNullOrEmpty(_languageId))
            {
                if (string.IsNullOrEmpty(chosenLanguage))
                {
                    _languageId = "eng";
                }
                else
                {
                    _languageId = chosenLanguage;
                }
            }
        }

        private void InitializeModiLanguages()
        {
            foreach (var ml in ModiLanguage.AllLanguages)
            {
                comboBoxModiLanguage.Items.Add(ml);
                if (_vobSubOcrSettings != null && ml.Id == _vobSubOcrSettings.LastModiLanguageId)
                {
                    comboBoxModiLanguage.SelectedIndex = comboBoxModiLanguage.Items.Count - 1;
                }
            }
        }

        private int GetModiLanguage()
        {
            if (comboBoxModiLanguage.SelectedIndex < 0)
            {
                return ModiLanguage.DefaultLanguageId;
            }

            return ((ModiLanguage)comboBoxModiLanguage.SelectedItem).Id;
        }

        private void ButtonStopClick(object sender, EventArgs e)
        {
            _mainOcrTimer?.Stop();
            _abort = true;
            _ocrThreadStop = true;
            _tesseractThreadRunner?.Cancel();
            buttonStop.Enabled = false;
            progressBar1.Visible = false;
            labelStatus.Text = string.Empty;
            SetButtonsEnabledAfterOcrDone();
        }

        private void ChangeDelayTimerTick(object sender, EventArgs e)
        {
            _changeDelayTimer.Enabled = false;
            _changeDelayTimer.Dispose();
            _changeDelayTimer = null;
            _slvSelIdxChangedTicks = 0;
            if (_slvSelIdx.Key == _slvSelIdxChangedTicks)
            {
                _selectedIndex = _slvSelIdx.Value;
                if (_selectedIndex < 0)
                {
                    textBoxCurrentText.Text = string.Empty;
                    return;
                }
                SelectedIndexChangedAction();
            }
        }

        private long _slvSelIdxChangedTicks;
        private Timer _changeDelayTimer = null;
        private KeyValuePair<long, int> _slvSelIdx;
        private void SubtitleListView1SelectedIndexChanged(object sender, EventArgs e)
        {
            if (subtitleListView1.SelectedItems.Count > 0)
            {
                try
                {
                    if (subtitleListView1.SelectedItems.Count > 1)
                    {
                        if (DateTime.UtcNow.Ticks - _slvSelIdxChangedTicks < 100000)
                        {
                            _slvSelIdx = new KeyValuePair<long, int>(_slvSelIdxChangedTicks, subtitleListView1.SelectedItems[0].Index);
                            if (_changeDelayTimer == null)
                            {
                                _changeDelayTimer = new Timer();
                                _changeDelayTimer.Tick += ChangeDelayTimerTick;
                                _changeDelayTimer.Interval = 200;
                            }
                            else
                            {
                                _changeDelayTimer.Stop();
                                _changeDelayTimer.Start();
                            }
                            _slvSelIdxChangedTicks = DateTime.UtcNow.Ticks;
                            return;
                        }
                        _slvSelIdxChangedTicks = DateTime.UtcNow.Ticks;
                    }
                    _selectedIndex = subtitleListView1.SelectedItems[0].Index;
                }
                catch
                {
                    return;
                }

                SelectedIndexChangedAction();
            }
            else
            {
                _selectedIndex = -1;
                textBoxCurrentText.Text = string.Empty;
            }
        }

        private void SelectedIndexChangedAction()
        {
            textBoxCurrentText.Text = _subtitle.Paragraphs[_selectedIndex].Text;
            if (_mainOcrRunning && _mainOcrBitmap != null)
            {
                ShowSubtitleImage(_selectedIndex, _mainOcrBitmap);
            }
            else
            {
                var bmp = ShowSubtitleImage(_selectedIndex);
                bmp.Dispose();
            }

            numericUpDownStartNumber.Value = _selectedIndex + 1;
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

        private static void FixVerticalScrollBars(SETextBox tb)
        {
            if (Utilities.GetNumberOfLines(tb.Text) > 5)
            {
                tb.ScrollBars = RichTextBoxScrollBars.Vertical;
            }
            else
            {
                tb.ScrollBars = RichTextBoxScrollBars.None;
            }
        }

        private void ButtonNewCharacterDatabaseClick(object sender, EventArgs e)
        {

        }

        private void ComboBoxCharacterDatabaseSelectedIndexChanged(object sender, EventArgs e)
        {
            LoadImageCompareBitmaps();
            if (_vobSubOcrSettings != null)
            {
                _vobSubOcrSettings.LastImageCompareFolder = comboBoxCharacterDatabase.SelectedItem.ToString();
            }

            bool lineWidthVisible = !comboBoxCharacterDatabase.SelectedItem.ToString().Equals("Latin", StringComparison.Ordinal);
            labelMinLineSplitHeight.Visible = lineWidthVisible;
            comboBoxLineSplitMinLineHeight.Visible = lineWidthVisible;
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
            using (var formVobSubEditCharacters = new VobSubEditCharacters(null, _binaryOcrDb))
            {
                formVobSubEditCharacters.Initialize(name, text);
                DialogResult result = formVobSubEditCharacters.ShowDialog();
                if (result == DialogResult.OK)
                {
                    if (_binaryOcrDb != null)
                    {
                        _binaryOcrDb.Save();
                    }

                    return result;
                }

                Cursor = Cursors.WaitCursor;
                if (formVobSubEditCharacters.ChangesMade)
                {
                    _binaryOcrDb.LoadCompareImages();
                }

                Cursor = Cursors.Default;
                return result;
            }
        }

        private void VobSubOcr_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyData == UiUtil.HelpKeys)
            {
                UiUtil.ShowHelp("#importvobsub");
                e.SuppressKeyPress = true;
            }
            else if (e.KeyData == UiUtil.GetKeys(Configuration.Settings.Shortcuts.GeneralGoToNextSubtitlePlayTranslate))
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
            else if (e.Modifiers == Keys.Control && e.KeyCode == Keys.R)
            {
                e.SuppressKeyPress = true;
                SelectBestImageCompareDatabase();
            }
            else if (e.Modifiers == Keys.Control && e.KeyCode == Keys.T)
            {
                e.SuppressKeyPress = true;
                OcrTrainingToolStripMenuItem_Click(null, null);
            }
            else if (e.Modifiers == Keys.Control && e.KeyCode == Keys.P)
            {
                e.SuppressKeyPress = true;
                previewToolStripMenuItem_Click(null, null);
            }
            else if (e.Modifiers == Keys.Control && e.KeyCode == Keys.F)
            {
                e.SuppressKeyPress = true;
                Find();
            }
            else if (e.Modifiers == Keys.None && e.KeyCode == Keys.F3)
            {
                e.SuppressKeyPress = true;
                FindNext();
            }
            else if (e.Modifiers == (Keys.Control | Keys.Shift) && e.KeyCode == Keys.P)
            {
                e.SuppressKeyPress = true;
                ImagePreProcessingToolStripMenuItem_Click(null, null);
            }
            else if (e.Modifiers == (Keys.Control | Keys.Shift) && e.KeyCode == Keys.I && (_ocrMethodIndex == _ocrMethodBinaryImageCompare || _ocrMethodIndex == _ocrMethodNocr))
            {
                e.SuppressKeyPress = true;
                var bmp = (Bitmap)pictureBoxSubtitleImage.Image;
                if (bmp != null)
                {
                    var nBmp = new NikseBitmap(bmp);
                    bmp.Dispose();
                    var italicFactor = _unItalicFactor;

                    for (var startX = 20.0; startX < nBmp.Width; startX += 20.0)
                    {
                        var x = startX;
                        for (int y = 0; y < nBmp.Height; y++)
                        {
                            x = startX - y * italicFactor;
                            if (x >= 0)
                            {
                                nBmp.SetPixel((int)Math.Round(x), y, Color.Red);
                            }
                        }
                    }

                    pictureBoxSubtitleImage.Image = nBmp.GetBitmap();
                }
            }
            else if (e.Modifiers == (Keys.Control | Keys.Shift) && e.KeyCode == Keys.H && (_ocrMethodIndex == _ocrMethodBinaryImageCompare || _ocrMethodIndex == _ocrMethodNocr))
            {
                e.SuppressKeyPress = true;
                var bmp = (Bitmap)pictureBoxSubtitleImage.Image;
                if (bmp != null)
                {
                    var nBmp = new NikseBitmap(bmp);
                    bmp.Dispose();
                    for (var startY = 20; startY < nBmp.Height; startY += 20)
                    {
                        for (int x = 0; x < nBmp.Width; x++)
                        {
                            nBmp.SetPixel(x, startY, Color.Red);
                        }
                    }

                    pictureBoxSubtitleImage.Image = nBmp.GetBitmap();
                }
            }
            else if (e.Modifiers == Keys.Control && e.KeyCode == Keys.H && (_ocrMethodIndex == _ocrMethodBinaryImageCompare || _ocrMethodIndex == _ocrMethodNocr))
            {
                e.SuppressKeyPress = true;

                if (comboBoxNOcrLineSplitMinHeight.Visible && comboBoxNOcrLineSplitMinHeight.SelectedIndex > 0)
                {
                    _ocrMinLineHeight = int.Parse(comboBoxNOcrLineSplitMinHeight.Text);
                }
                else if (comboBoxLineSplitMinLineHeight.Visible && comboBoxLineSplitMinLineHeight.SelectedIndex > 0)
                {
                    _ocrMinLineHeight = int.Parse(comboBoxLineSplitMinLineHeight.Text);
                }
                else
                {
                    _ocrMinLineHeight = -1;
                }

                int minLineHeight = GetMinLineHeight();
                var bitmap = GetSubtitleBitmap(_selectedIndex);
                var nikseBitmap = new NikseBitmap(bitmap);
                nikseBitmap.MakeTwoColor(200);
                var list = NikseBitmapImageSplitter.SplitBitmapToLines(nikseBitmap, _numericUpDownPixelsIsSpace, checkBoxRightToLeftNOCR.Checked, Configuration.Settings.VobSubOcr.TopToBottom, minLineHeight, _autoLineHeight);

                var lineSplitImage = new NikseBitmap(nikseBitmap.Width + 10, nikseBitmap.Height + list.Count * 10 + 10);
                lineSplitImage.Fill(Color.Red);
                for (var index = 0; index < list.Count; index++)
                {
                    var imageSplitterItem = list[index];
                    if (imageSplitterItem.NikseBitmap != null)
                    {
                        for (int y = 0; y < imageSplitterItem.NikseBitmap.Height; y++)
                        {
                            for (int x = 0; x < imageSplitterItem.NikseBitmap.Width; x++)
                            {
                                lineSplitImage.SetPixel(x, y + imageSplitterItem.Y + index * 5, imageSplitterItem.NikseBitmap.GetPixel(x, y));
                            }
                        }
                    }
                }

                lineSplitImage.ReplaceTransparentWith(Color.Black);
                var bmp = lineSplitImage.GetBitmap();
                using (var form = new ExportPngXmlPreview(bmp))
                {
                    form.ShowDialog(this);
                }
                bmp.Dispose();
            }
        }

        private void Find()
        {
            using (var findDialog = new FindDialog(_subtitle))
            {
                var idx = _selectedIndex + 1;

                findDialog.Initialize(string.Empty, _findHelper);
                if (findDialog.ShowDialog(this) == DialogResult.OK)
                {
                    _findHelper = findDialog.GetFindDialogHelper(idx);
                    if (_findHelper.Find(_subtitle, null, idx + 1))
                    {
                        subtitleListView1.SelectIndexAndEnsureVisible(_findHelper.SelectedIndex, true);
                    }
                }
            }
        }

        private void FindNext()
        {
            if (_findHelper == null)
            {
                return;
            }

            var idx = _selectedIndex + 1;
            if (_findHelper.Find(_subtitle, null, idx + 1))
            {
                subtitleListView1.SelectIndexAndEnsureVisible(_findHelper.SelectedIndex, true);
            }
        }

        private void ComboBoxTesseractLanguagesSelectedIndexChanged(object sender, EventArgs e)
        {
            var l = (comboBoxTesseractLanguages.SelectedItem as TesseractLanguage).Id;
            Configuration.Settings.VobSubOcr.TesseractLastLanguage = l;
            LoadOcrFixEngine(string.Empty, string.Empty);

            if (_ocrMethodIndex == _ocrMethodTesseract4)
            {
                var ok = Configuration.IsRunningOnWindows &&
                         File.Exists(Path.Combine(Configuration.Tesseract302Directory, "tesseract.exe")) &&
                         File.Exists(Path.Combine(Configuration.Tesseract302DataDirectory, l + ".traineddata"));
                checkBoxTesseractFallback.Visible = ok;
                if (!ok)
                {
                    checkBoxTesseractFallback.Checked = false;
                }
            }
            else if (_ocrMethodIndex == _ocrMethodTesseract302)
            {
                var ok = File.Exists(Path.Combine(Configuration.TesseractDirectory, "tesseract.exe")) &&
                         File.Exists(Path.Combine(Configuration.TesseractDataDirectory, l + ".traineddata"));
                checkBoxTesseractFallback.Visible = ok;
                if (!ok)
                {
                    checkBoxTesseractFallback.Checked = false;
                }
            }

            if (comboBoxDictionaries.SelectedIndex == -1)
            {
                comboBoxDictionaries.SelectedIndex = 0;
            }
        }

        private void LoadOcrFixEngine(string threeLetterIsoLanguageName, string hunspellName)
        {
            if (_ocrMethodIndex != _ocrMethodTesseract4 && _ocrMethodIndex != _ocrMethodTesseract302)
            {
                try
                {
                    if (!string.IsNullOrEmpty(LanguageString))
                    {
                        var ci = CultureInfo.GetCultureInfo(LanguageString.Replace("_", "-"));
                        _languageId = ci.GetThreeLetterIsoLanguageName();
                    }
                }
                catch
                {
                    // ignored
                }
            }
            else if (string.IsNullOrEmpty(threeLetterIsoLanguageName) && comboBoxTesseractLanguages.SelectedItem != null)
            {
                _languageId = (comboBoxTesseractLanguages.SelectedItem as TesseractLanguage).Id;
                threeLetterIsoLanguageName = _languageId;
            }

            var tempOcrFixEngine = new OcrFixEngine(threeLetterIsoLanguageName, hunspellName, this, _ocrMethodIndex == _ocrMethodBinaryImageCompare || _ocrMethodIndex == _ocrMethodNocr);
            if (tempOcrFixEngine.IsDictionaryLoaded)
            {
                _ocrFixEngine?.Dispose();
                _ocrFixEngine = tempOcrFixEngine;
                string loadedDictionaryName = _ocrFixEngine.SpellCheckDictionaryName;
                var found = false;
                comboBoxDictionaries.SelectedIndexChanged -= comboBoxDictionaries_SelectedIndexChanged;
                if (_ocrMethodIndex == _ocrMethodTesseract4 &&
                    !string.IsNullOrEmpty(Configuration.Settings.VobSubOcr.LastTesseractSpellCheck) &&
                    Configuration.Settings.VobSubOcr.LastTesseractSpellCheck.Length > 1 &&
                    loadedDictionaryName.Length > 1 &&
                    Configuration.Settings.VobSubOcr.LastTesseractSpellCheck.StartsWith(loadedDictionaryName.Substring(0, 2), StringComparison.OrdinalIgnoreCase))
                {
                    for (var index = 0; index < comboBoxDictionaries.Items.Count; index++)
                    {
                        var item = (string)comboBoxDictionaries.Items[index];
                        if (item.Contains("[" + Configuration.Settings.VobSubOcr.LastTesseractSpellCheck + "]"))
                        {
                            comboBoxDictionaries.SelectedIndex = index;
                            found = true;
                            break;
                        }
                    }
                }

                if (!found)
                {
                    for (var index = 0; index < comboBoxDictionaries.Items.Count; index++)
                    {
                        var item = (string)comboBoxDictionaries.Items[index];
                        if (item.Contains("[" + loadedDictionaryName + "]"))
                        {
                            comboBoxDictionaries.SelectedIndex = index;
                            break;
                        }
                    }
                }

                comboBoxDictionaries.SelectedIndexChanged += comboBoxDictionaries_SelectedIndexChanged;
                comboBoxDictionaries.Left = labelDictionaryLoaded.Left + labelDictionaryLoaded.Width;
                comboBoxDictionaries.Width = groupBoxOcrAutoFix.Width - (comboBoxDictionaries.Left + 10 + buttonSpellCheckDownload.Width);
            }
            else
            {
                tempOcrFixEngine.Dispose();
                comboBoxModiLanguage.SelectedIndex = -1;
            }
        }

        private void ComboBoxOcrMethodSelectedIndexChanged(object sender, EventArgs e)
        {
            _abort = true;
            _binaryOcrDb = null;
            _nOcrDb = null;
            _ocrMethodIndex = comboBoxOcrMethod.SelectedIndex;
            if (_ocrMethodIndex == _ocrMethodTesseract4)
            {
                ResetTesseractThread();
                InitializeTesseract();
                ShowOcrMethodGroupBox(GroupBoxTesseractMethod);
                Configuration.Settings.VobSubOcr.LastOcrMethod = "Tesseract4";
                comboBoxTesseractEngineMode.Visible = true;
                labelTesseractEngineMode.Visible = true;
                checkBoxTesseractFallback.Text = string.Format(LanguageSettings.Current.VobSubOcr.FallbackToX, "Tesseract 3.02");
                if (Configuration.IsRunningOnWindows && !File.Exists(Path.Combine(Configuration.TesseractDirectory, "tesseract.exe")))
                {
                    if (MessageBox.Show($"{LanguageSettings.Current.GetTesseractDictionaries.Download} Tesseract {Tesseract5Version}", LanguageSettings.Current.General.Title, MessageBoxButtons.YesNoCancel) == DialogResult.Yes)
                    {
                        comboBoxTesseractLanguages.Items.Clear();
                        using (var form = new DownloadTesseract5(Tesseract5Version))
                        {
                            if (form.ShowDialog(this) == DialogResult.OK)
                            {
                                buttonGetTesseractDictionaries_Click(sender, e);
                            }
                        }
                    }
                    else
                    {
                        comboBoxOcrMethod.SelectedIndex = _ocrMethodBinaryImageCompare;
                        return;
                    }
                }
            }
            else if (_ocrMethodIndex == _ocrMethodTesseract302)
            {
                ResetTesseractThread();
                InitializeTesseract();
                ShowOcrMethodGroupBox(GroupBoxTesseractMethod);
                Configuration.Settings.VobSubOcr.LastOcrMethod = "Tesseract302";
                comboBoxTesseractEngineMode.Visible = false;
                labelTesseractEngineMode.Visible = false;
                checkBoxTesseractFallback.Text = string.Format(LanguageSettings.Current.VobSubOcr.FallbackToX, "Tesseract " + Tesseract5Version);
                if (!File.Exists(Path.Combine(Configuration.Tesseract302Directory, "tesseract.exe")))
                {
                    if (MessageBox.Show(LanguageSettings.Current.GetTesseractDictionaries.Download + " Tesseract 3.02", LanguageSettings.Current.General.Title, MessageBoxButtons.YesNoCancel) == DialogResult.Yes)
                    {
                        using (var form = new DownloadTesseract302())
                        {
                            form.ShowDialog(this);
                        }
                    }
                    else
                    {
                        comboBoxOcrMethod.SelectedIndex = _ocrMethodBinaryImageCompare;
                        return;
                    }
                }
            }
            else if (_ocrMethodIndex == _ocrMethodNocr)
            {
                ShowOcrMethodGroupBox(groupBoxNOCR);
                Configuration.Settings.VobSubOcr.LastOcrMethod = "nOCR";
                SetSpellCheckLanguage(Configuration.Settings.VobSubOcr.LineOcrLastSpellCheck);

                comboBoxNOcrLanguage.Items.Clear();
                int index = 0;
                int selIndex = 0;
                foreach (var s in NOcrDb.GetDatabases())
                {
                    if (s == Configuration.Settings.VobSubOcr.LineOcrLastLanguages)
                    {
                        selIndex = index;
                    }

                    comboBoxNOcrLanguage.Items.Add(s);
                    index++;
                }

                if (comboBoxNOcrLanguage.Items.Count > 0)
                {
                    comboBoxNOcrLanguage.SelectedIndex = selIndex;
                }
            }
            else if (_ocrMethodIndex == _ocrMethodBinaryImageCompare)
            {
                ShowOcrMethodGroupBox(groupBoxImageCompareMethod);
                Configuration.Settings.VobSubOcr.LastOcrMethod = "BinaryImageCompare";
                numericUpDownMaxErrorPct.Minimum = 0;
                LoadImageCompareCharacterDatabaseList(comboBoxCharacterDatabase.SelectedItem.ToString());
            }
            else if (_ocrMethodIndex == _ocrMethodModi)
            {
                ShowOcrMethodGroupBox(groupBoxModiMethod);
                Configuration.Settings.VobSubOcr.LastOcrMethod = "MODI";
            }

            _ocrFixEngine = null;
            SubtitleListView1SelectedIndexChanged(null, null);
        }

        private string GetNOcrLanguageFileName()
        {
            if (comboBoxNOcrLanguage.SelectedIndex < 0)
            {
                return null;
            }

            return Configuration.OcrDirectory + comboBoxNOcrLanguage.Items[comboBoxNOcrLanguage.SelectedIndex] + ".nocr";
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
            if (sender is ListBox lb && lb.SelectedIndex >= 0)
            {
                if (lb.Items[lb.SelectedIndex] is LogItem item && item.Line > 0)
                {
                    subtitleListView1.SelectIndexAndEnsureVisible(item.Line - 1);
                }
            }
        }

        private void ContextMenuStripListviewOpening(object sender, CancelEventArgs e)
        {
            if (subtitleListView1.SelectedItems.Count == 0)
            {
                e.Cancel = true;
            }

            // Enable toolstrips if event was raised by Subtitle listview.
            bool enableIfRaisedBySubListView = contextMenuStripListview.SourceControl == subtitleListView1;
            normalToolStripMenuItem.Visible = enableIfRaisedBySubListView;
            italicToolStripMenuItem.Visible = enableIfRaisedBySubListView;
            toolStripSeparator1.Visible = enableIfRaisedBySubListView && subtitleListView1.SelectedItems.Count == 1;
            saveImageAsToolStripMenuItem.Visible = !enableIfRaisedBySubListView || subtitleListView1.SelectedItems.Count == 1;

            // Image compare.
            bool enableIfImageCompare = _ocrMethodIndex == _ocrMethodBinaryImageCompare;
            inspectImageCompareMatchesForCurrentImageToolStripMenuItem.Visible = enableIfImageCompare;
            EditLastAdditionsToolStripMenuItem.Visible = enableIfImageCompare && _lastAdditions != null && _lastAdditions.Count > 0;

            // Use N-OCR compare. (Only available in Beta mode).
            bool useNocrCompare = _ocrMethodIndex == _ocrMethodNocr;
            toolStripMenuItemInspectNOcrMatches.Visible = useNocrCompare;
            OcrTrainingToolStripMenuItem.Visible = useNocrCompare || enableIfImageCompare;

            toolStripSeparatorImageCompare.Visible = useNocrCompare || enableIfImageCompare;
        }

        private void SaveImageAsToolStripMenuItemClick(object sender, EventArgs e)
        {
            saveFileDialog1.Title = LanguageSettings.Current.VobSubOcr.SaveSubtitleImageAs;
            saveFileDialog1.AddExtension = true;
            saveFileDialog1.FileName = "Image" + (_selectedIndex + 1);
            saveFileDialog1.Filter = "PNG image|*.png|BMP image|*.bmp|GIF image|*.gif|TIFF image|*.tiff";
            saveFileDialog1.FilterIndex = 0;

            DialogResult result = saveFileDialog1.ShowDialog(this);
            if (result == DialogResult.OK)
            {
                var bmp = GetSubtitleBitmap(_selectedIndex);
                if (bmp == null)
                {
                    MessageBox.Show("No image!");
                    return;
                }

                try
                {
                    if (saveFileDialog1.FilterIndex == 0)
                    {
                        bmp.Save(saveFileDialog1.FileName, System.Drawing.Imaging.ImageFormat.Png);
                    }
                    else if (saveFileDialog1.FilterIndex == 1)
                    {
                        bmp.Save(saveFileDialog1.FileName);
                    }
                    else if (saveFileDialog1.FilterIndex == 2)
                    {
                        bmp.Save(saveFileDialog1.FileName, System.Drawing.Imaging.ImageFormat.Gif);
                    }
                    else
                    {
                        bmp.Save(saveFileDialog1.FileName, System.Drawing.Imaging.ImageFormat.Tiff);
                    }
                }
                catch (Exception exception)
                {
                    MessageBox.Show(exception.Message);
                }
                finally
                {
                    bmp.Dispose();
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
                        p.Text = HtmlUtil.RemoveHtmlTags(p.Text);
                        subtitleListView1.SetText(item.Index, p.Text);
                        if (item.Index == _selectedIndex)
                        {
                            textBoxCurrentText.Text = p.Text;
                        }
                    }
                }
            }
        }

        private void ItalicToolStripMenuItemClick(object sender, EventArgs e)
        {
            const string tag = "i";
            var removeTag = true;
            if (_subtitle.Paragraphs.Count > 0 && subtitleListView1.SelectedItems.Count > 0)
            {
                foreach (ListViewItem item in subtitleListView1.SelectedItems)
                {
                    var p = _subtitle.GetParagraphOrDefault(item.Index);
                    if (p != null)
                    {
                        if (!Utilities.RemoveSsaTags(p.Text).StartsWith("<i>"))
                        {
                            removeTag = false;
                            break;
                        }
                    }
                }
            }

            if (_subtitle.Paragraphs.Count > 0 && subtitleListView1.SelectedItems.Count > 0)
            {
                foreach (ListViewItem item in subtitleListView1.SelectedItems)
                {
                    var p = _subtitle.GetParagraphOrDefault(item.Index);
                    if (p != null)
                    {
                        if (p.Text.Contains("<" + tag + ">"))
                        {
                            p.Text = p.Text.Replace("<" + tag + ">", string.Empty);
                            p.Text = p.Text.Replace("</" + tag + ">", string.Empty);
                        }

                        if (!removeTag)
                        {
                            p.Text = $"<{tag}>{p.Text}</{tag}>";
                        }

                        subtitleListView1.SetText(item.Index, p.Text);
                        if (item.Index == _selectedIndex)
                        {
                            textBoxCurrentText.Text = p.Text;
                        }
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
            _ocrThreadStop = true;
            _tesseractThreadRunner?.Cancel();
            if (_tesseractAsyncStrings != null)
            {
                for (int i = 0; i < _tesseractAsyncStrings.Length; i++)
                {
                    _tesseractAsyncStrings[i] = string.Empty;
                }
            }

            _tesseractAsyncIndex = 0;
        }

        private void PictureBoxColorChooserClick(object sender, EventArgs e)
        {
            if (colorDialog1.ShowDialog(this) == DialogResult.OK)
            {
                (sender as PictureBox).BackColor = colorDialog1.Color;
            }

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


        private Subtitle _beforeOnlyForced;

        private void checkBoxShowOnlyForced_CheckedChanged(object sender, EventArgs e)
        {
            if (_tesseractThreadRunner != null)
            {
                _tesseractThreadRunner.Cancel();
                for (int i = 0; i < 10; i++)
                {
                    System.Threading.Thread.Sleep(100);
                }

                _tesseractAsyncStrings = null;
            }

            var oldSubtitle = new Subtitle(_subtitle);
            if (checkBoxShowOnlyForced.Checked)
            {
                _beforeOnlyForced = oldSubtitle;
            }

            subtitleListView1.BeginUpdate();
            if (_bdnXmlOriginal != null)
            {
                LoadBdnXml();
            }
            else if (_bluRaySubtitlesOriginal != null)
            {
                LoadBluRaySup();
            }
            else if (_dvbSubtitles != null)
            {
                LoadDvbSubtitles();
            }
            else
            {
                LoadVobRip();
            }

            for (int i = 0; i < _subtitle.Paragraphs.Count; i++)
            {
                var current = _subtitle.Paragraphs[i];
                foreach (var old in oldSubtitle.Paragraphs)
                {
                    if (Math.Abs(current.StartTime.TotalMilliseconds - old.StartTime.TotalMilliseconds) < 0.01 &&
                        Math.Abs(current.Duration.TotalMilliseconds - old.Duration.TotalMilliseconds) < 0.01)
                    {
                        current.Text = old.Text;
                        break;
                    }
                }
            }

            if (!checkBoxShowOnlyForced.Checked && _beforeOnlyForced != null)
            {
                for (int i = 0; i < _subtitle.Paragraphs.Count; i++)
                {
                    var current = _subtitle.Paragraphs[i];
                    var old = _beforeOnlyForced.Paragraphs[i];
                    if (string.IsNullOrEmpty(current.Text))
                    {
                        current.Text = old.Text;
                    }
                }
            }

            subtitleListView1.Fill(_subtitle);
            subtitleListView1.EndUpdate();
        }

        private void LoadDvbSubtitles()
        {

        }

        private void checkBoxUseTimeCodesFromIdx_CheckedChanged(object sender, EventArgs e)
        {
            var oldSubtitle = new Subtitle(_subtitle);
            subtitleListView1.BeginUpdate();
            LoadVobRip();
            for (int i = 0; i < _subtitle.Paragraphs.Count; i++)
            {
                var p = oldSubtitle.GetParagraphOrDefault(i);
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
                if (comboBoxDictionaries.SelectedItem != null)
                {
                    var name = comboBoxDictionaries.SelectedItem.ToString();
                    int start = name.LastIndexOf('[') + 1;
                    int end = name.LastIndexOf(']');
                    if (start > 0 && end > start)
                    {
                        return name.Substring(start, end - start);
                    }
                }
                return null;
            }
        }

        private void SetSpellCheckLanguage(string languageString)
        {
            if (string.IsNullOrEmpty(languageString))
            {
                return;
            }

            int i = 0;
            foreach (string item in comboBoxDictionaries.Items)
            {
                if (item.Contains("[" + languageString + "]"))
                {
                    comboBoxDictionaries.SelectedIndex = i;
                }

                i++;
            }
        }

        private void comboBoxDictionaries_SelectedIndexChanged(object sender, EventArgs e)
        {
            Configuration.Settings.General.SpellCheckLanguage = LanguageString;
            if (_ocrMethodIndex == _ocrMethodTesseract4 || _ocrMethodIndex == _ocrMethodTesseract302)
            {
                Configuration.Settings.VobSubOcr.LastTesseractSpellCheck = LanguageString;
            }

            string threeLetterIsoLanguageName = string.Empty;
            var language = LanguageString;
            if (language == null)
            {
                _ocrFixEngine?.Dispose();
                _ocrFixEngine = new OcrFixEngine(string.Empty, string.Empty, this, _ocrMethodIndex == _ocrMethodBinaryImageCompare || _ocrMethodIndex == _ocrMethodNocr);
                return;
            }

            try
            {
                _ocrFixEngine?.Dispose();
                _ocrFixEngine = null;
                var ci = CultureInfo.GetCultureInfo(language.Replace("_", "-"));
                threeLetterIsoLanguageName = ci.GetThreeLetterIsoLanguageName();
            }
            catch
            {
                var arr = language.Split('-', '_');
                if (arr.Length > 1 && arr[0].Length == 2)
                {
                    foreach (var x in CultureInfo.GetCultures(CultureTypes.NeutralCultures))
                    {
                        if (string.Equals(x.TwoLetterISOLanguageName, arr[0], StringComparison.OrdinalIgnoreCase))
                        {
                            threeLetterIsoLanguageName = x.GetThreeLetterIsoLanguageName();
                            break;
                        }
                    }
                }
            }

            if (string.IsNullOrEmpty(threeLetterIsoLanguageName) && !string.IsNullOrEmpty(language) && language.Length >= 2)
            {
                var twoLetterCode = language.Substring(0, 2);
                var threeLetters = Iso639Dash2LanguageCode.GetThreeLetterCodeFromTwoLetterCode(twoLetterCode);
                if (!string.IsNullOrEmpty(threeLetters))
                {
                    threeLetterIsoLanguageName = threeLetters;
                }
            }

            LoadOcrFixEngine(threeLetterIsoLanguageName, LanguageString);
        }

        internal void Initialize(Subtitle bdnSubtitle, VobSubOcrSettings vobSubOcrSettings, bool isSon)
        {
            if (!string.IsNullOrEmpty(bdnSubtitle.FileName) && bdnSubtitle.FileName != new Subtitle().FileName)
            {
                FileName = bdnSubtitle.FileName;
            }

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

            SetButtonsStartOcr();
            progressBar1.Visible = false;
            progressBar1.Maximum = 100;
            progressBar1.Value = 0;
            numericUpDownPixelsIsSpace.Value = vobSubOcrSettings.XOrMorePixelsMakesSpace;
            _vobSubOcrSettings = vobSubOcrSettings;

            InitializeTesseract();
            LoadImageCompareCharacterDatabaseList(Configuration.Settings.VobSubOcr.LastBinaryImageCompareDb);

            SetOcrMethod();

            groupBoxImagePalette.Visible = isSon;

            Text = LanguageSettings.Current.VobSubOcr.TitleBluRay;
            Text += " - " + Path.GetFileName(_bdnFileName);

            autoTransparentBackgroundToolStripMenuItem.Checked = true;
            autoTransparentBackgroundToolStripMenuItem.Visible = true;

        }

        private void SetOcrMethod()
        {
            if (Configuration.Settings.VobSubOcr.LastOcrMethod == "BinaryImageCompare" && comboBoxOcrMethod.Items.Count > _ocrMethodBinaryImageCompare)
            {
                comboBoxOcrMethod.SelectedIndex = _ocrMethodBinaryImageCompare;
            }
            else if (Configuration.Settings.VobSubOcr.LastOcrMethod == "MODI" && comboBoxOcrMethod.Items.Count > _ocrMethodModi)
            {
                comboBoxOcrMethod.SelectedIndex = _ocrMethodModi;
            }
            else if (Configuration.Settings.VobSubOcr.LastOcrMethod == "nOCR" && comboBoxOcrMethod.Items.Count > _ocrMethodNocr)
            {
                comboBoxOcrMethod.SelectedIndex = _ocrMethodNocr;
            }
            else if (Configuration.Settings.VobSubOcr.LastOcrMethod == "Tesseract302" && comboBoxOcrMethod.Items.Count > _ocrMethodTesseract302)
            {
                comboBoxOcrMethod.SelectedIndex = _ocrMethodTesseract302;
            }
            else if (Configuration.Settings.VobSubOcr.LastOcrMethod == "Tesseract4" && comboBoxOcrMethod.Items.Count > _ocrMethodTesseract302)
            {
                comboBoxOcrMethod.SelectedIndex = _ocrMethodTesseract4;
            }
            else
            {
                comboBoxOcrMethod.SelectedIndex = 0;
            }
        }

        internal void StartOcrFromDelayed()
        {
            if (_lastAdditions.Count > 0)
            {
                var last = _lastAdditions[_lastAdditions.Count - 1];
                numericUpDownStartNumber.Value = last.Index + 1;

                // Simulate a click on ButtonStartOcr in 200ms.
                var uiContext = TaskScheduler.FromCurrentSynchronizationContext();
                Utilities.TaskDelay(200).ContinueWith(_ => ButtonStartOcrClick(null, null), uiContext);
            }
        }

        private void VobSubOcr_Resize(object sender, EventArgs e)
        {
            const int originalTopHeight = 105;

            int adjustPercent = (int)(Height * 0.15);
            groupBoxSubtitleImage.Height = originalTopHeight + adjustPercent;
            groupBoxOcrMethod.Height = groupBoxSubtitleImage.Height;

            splitContainerBottom.Top = groupBoxSubtitleImage.Top + groupBoxSubtitleImage.Height + 5;
            splitContainerBottom.Height = progressBar1.Top - (splitContainerBottom.Top + 20);
            checkBoxUseTimeCodesFromIdx.Left = groupBoxOCRControls.Left + 1;
            checkBoxShowOnlyForced.Left = checkBoxUseTimeCodesFromIdx.Left;

            listBoxUnknownWords.Top = listBoxLog.Top;
            listBoxUnknownWords.Left = listBoxLog.Left;

            // Hack for resize after minimize...
            groupBoxSubtitleImage.Width = Width - groupBoxSubtitleImage.Left - 25;
            splitContainerBottom.Width = Width - 40;
        }

        private void importTextWithMatchingTimeCodesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            openFileDialog1.Title = LanguageSettings.Current.General.OpenSubtitle;
            openFileDialog1.FileName = string.Empty;
            openFileDialog1.Filter = UiUtil.SubtitleExtensionFilter.Value;
            if (openFileDialog1.ShowDialog(this) == DialogResult.OK)
            {
                string fileName = openFileDialog1.FileName;
                if (!File.Exists(fileName))
                {
                    return;
                }

                var fi = new FileInfo(fileName);
                if (fi.Length > 1024 * 1024 * 10) // max 10 mb
                {
                    if (MessageBox.Show(string.Format(LanguageSettings.Current.Main.FileXIsLargerThan10MB + Environment.NewLine +
                                                      Environment.NewLine +
                                                      LanguageSettings.Current.Main.ContinueAnyway,
                            fileName), Text, MessageBoxButtons.YesNoCancel) != DialogResult.Yes)
                    {
                        return;
                    }
                }

                Subtitle sub = new Subtitle();
                Encoding encoding;
                SubtitleFormat format = sub.LoadSubtitle(fileName, out encoding, null);
                if (format == null)
                {
                    return;
                }

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

        internal void SaveAllImagesWithHtmlIndexViewToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialog1.ShowDialog(this) == DialogResult.OK)
            {
                progressBar1.Maximum = _subtitle.Paragraphs.Count - 1;
                progressBar1.Value = 0;
                progressBar1.Visible = true;
                int imagesSavedCount = 0;
                var sb = new StringBuilder();
                sb.AppendLine("<!DOCTYPE html>");
                sb.AppendLine("<html>");
                sb.AppendLine("<head>");
                sb.AppendLine("   <meta charset=\"UTF-8\" />");
                sb.AppendLine("   <title>Subtitle images</title>");
                sb.AppendLine("</head>");
                sb.AppendLine("<body>");
                _fromMenuItem = true;
                for (int i = 0; i < _subtitle.Paragraphs.Count; i++)
                {
                    progressBar1.Value = i;
                    var bmp = GetSubtitleBitmap(i);
                    if (bmp != null)
                    {
                        var fileName = string.Format(CultureInfo.InvariantCulture, "{0:0000}.png", i + 1);
                        var filePath = Path.Combine(folderBrowserDialog1.SelectedPath, fileName);
                        bmp.Save(filePath, System.Drawing.Imaging.ImageFormat.Png);
                        imagesSavedCount++;
                        var p = _subtitle.Paragraphs[i];
                        sb.AppendFormat(CultureInfo.InvariantCulture, "#{3}:{0}->{1}<div style='text-align:center'><img src='{2}' />", p.StartTime.ToShortString(), p.EndTime.ToShortString(), fileName, i + 1);
                        if (!string.IsNullOrEmpty(p.Text))
                        {
                            var backgroundColor = ColorTranslator.ToHtml(subtitleListView1.GetBackgroundColor(i));
                            var text = WebUtility.HtmlEncode(p.Text.Replace("<i>", "@1__").Replace("</i>", "@2__")).Replace("@1__", "<i>").Replace("@2__", "</i>").Replace(Environment.NewLine, "<br />");
                            sb.Append("<br /><div style='font-size:22px; background-color:").Append(backgroundColor).Append("'>").Append(text).Append("</div>");
                        }

                        sb.AppendLine("</div><br /><hr />");
                        bmp.Dispose();
                    }
                }

                _fromMenuItem = false;
                sb.AppendLine("</body>");
                sb.AppendLine("</html>");
                var htmlFileName = Path.Combine(folderBrowserDialog1.SelectedPath, "index.html");
                File.WriteAllText(htmlFileName, sb.ToString(), Encoding.UTF8);
                progressBar1.Visible = false;
                MessageBox.Show($"{imagesSavedCount} images saved in {folderBrowserDialog1.SelectedPath}");
                UiUtil.OpenFile(htmlFileName);
            }
        }

        private void InspectImageCompareMatchesForCurrentImageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (subtitleListView1.SelectedItems.Count != 1)
            {
                return;
            }

            if (_compareBitmaps == null)
            {
                LoadImageCompareBitmaps();
            }

            Cursor = Cursors.WaitCursor;
            var bitmap = GetSubtitleBitmap(subtitleListView1.SelectedItems[0].Index);
            var parentBitmap = new NikseBitmap(bitmap);
            int minLineHeight = GetMinLineHeight();
            Cursor = Cursors.Default;
            using (var inspect = new VobSubOcrCharacterInspect())
            {
                do
                {
                    var matches = new List<CompareMatch>();
                    var sourceList = NikseBitmapImageSplitter.SplitBitmapToLettersNew(parentBitmap, (int)numericUpDownPixelsIsSpace.Value, checkBoxRightToLeft.Checked, Configuration.Settings.VobSubOcr.TopToBottom, minLineHeight, _autoLineHeight);
                    var imageSources = CalcInspectMatches(sourceList, matches, parentBitmap);
                    inspect.Initialize(comboBoxCharacterDatabase.SelectedItem.ToString(), matches, imageSources, _binaryOcrDb, sourceList);
                    var result = inspect.ShowDialog(this);
                    if (result == DialogResult.OK)
                    {
                        Cursor = Cursors.WaitCursor;
                        if (_binaryOcrDb != null)
                        {
                            _binaryOcrDb.Save();
                            Cursor = Cursors.Default;
                        }
                        else
                        {
                            _compareDoc = inspect.ImageCompareDocument;
                            string path = Configuration.VobSubCompareDirectory + comboBoxCharacterDatabase.SelectedItem + Path.DirectorySeparatorChar;
                            _compareDoc.Save(path + "Images.xml");
                            LoadImageCompareBitmaps();
                            Cursor = Cursors.Default;
                        }
                    }
                } while (inspect.DeleteMultiMatch);
            }

            _binaryOcrDb?.LoadCompareImages();
            Cursor = Cursors.Default;
        }

        private List<Bitmap> CalcInspectMatches(List<ImageSplitterItem> sourceList, List<CompareMatch> matches, NikseBitmap parentBitmap)
        {
            int index = 0;
            var imageSources = new List<Bitmap>();
            while (index < sourceList.Count)
            {
                var item = sourceList[index];
                if (item.NikseBitmap == null)
                {
                    matches.Add(new CompareMatch(item.SpecialCharacter, false, 0, null));
                    imageSources.Add(null);
                }
                else
                {
                    var match = GetCompareMatchNew(item, out _, sourceList, index, _binaryOcrDb);
                    if (match == null)
                    {
                        var cm = new CompareMatch(LanguageSettings.Current.VobSubOcr.NoMatch, false, 0, null)
                        {
                            ImageSplitterItem = item
                        };
                        matches.Add(cm);
                        imageSources.Add(item.NikseBitmap.GetBitmap());
                    }
                    else // found image match
                    {
                        if (match.ExpandCount > 0)
                        {
                            List<ImageSplitterItem> expandSelectionList = new List<ImageSplitterItem>();
                            for (int i = 0; i < match.ExpandCount; i++)
                            {
                                expandSelectionList.Add(sourceList[index + i]);
                            }

                            item = GetExpandedSelectionNew(parentBitmap, expandSelectionList);
                            matches.Add(new CompareMatch(match.Text, match.Italic, 0, match.Name, item) { Extra = expandSelectionList });
                            imageSources.Add(item.NikseBitmap.GetBitmap());
                        }
                        else
                        {
                            matches.Add(new CompareMatch(match.Text, match.Italic, 0, match.Name, item));
                            imageSources.Add(item.NikseBitmap.GetBitmap());
                        }

                        if (match.ExpandCount > 0)
                        {
                            index += match.ExpandCount - 1;
                        }
                    }
                }

                index++;
            }

            return imageSources;
        }

        private void inspectLastAdditionsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (var formVobSubEditCharacters = new VobSubEditCharacters(_lastAdditions, _binaryOcrDb))
            {
                if (formVobSubEditCharacters.ShowDialog(this) == DialogResult.OK)
                {
                    _lastAdditions = formVobSubEditCharacters.Additions;
                    if (_binaryOcrDb != null)
                    {
                        _binaryOcrDb.Save();
                    }
                }
            }
        }

        private void checkBoxAutoTransparentBackground_CheckedChanged(object sender, EventArgs e)
        {
            ResetTesseractThread();
            SubtitleListView1SelectedIndexChanged(null, null);
            if (autoTransparentBackgroundToolStripMenuItem.Checked && _dvbSubtitles != null)
            {
                numericUpDownAutoTransparentAlphaMax.Visible = true;
            }
            else
            {
                numericUpDownAutoTransparentAlphaMax.Visible = false;
            }

            labelMinAlpha.Visible = numericUpDownAutoTransparentAlphaMax.Visible;
        }

        internal void Initialize(string fileName, List<Color> palette, VobSubOcrSettings vobSubOcrSettings, List<SpHeader> spList)
        {
            _spList = spList;
            SetButtonsStartOcr();
            progressBar1.Visible = false;
            progressBar1.Maximum = 100;
            progressBar1.Value = 0;
            numericUpDownPixelsIsSpace.Value = vobSubOcrSettings.XOrMorePixelsMakesSpace;
            _vobSubOcrSettings = vobSubOcrSettings;

            InitializeTesseract();
            LoadImageCompareCharacterDatabaseList(Configuration.Settings.VobSubOcr.LastBinaryImageCompareDb);

            _palette = palette;

            if (_palette == null)
            {
                checkBoxCustomFourColors.Checked = true;
            }

            SetOcrMethod();

            FileName = fileName;
            Text += " - " + Path.GetFileName(FileName);

            foreach (SpHeader header in _spList)
            {
                Paragraph p = new Paragraph(string.Empty, header.StartTime.TotalMilliseconds, header.StartTime.TotalMilliseconds + header.Picture.Delay.TotalMilliseconds);
                _subtitle.Paragraphs.Add(p);
            }

            _subtitle.Renumber();
            subtitleListView1.Fill(_subtitle);
            subtitleListView1.SelectIndexAndEnsureVisible(0);
        }

        private void textBoxCurrentText_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyData == _italicShortcut) // Ctrl+I (or custom) = Italic
            {
                var tb = textBoxCurrentText;
                string text = tb.SelectedText;
                int selectionStart = tb.SelectionStart;
                if (text.Contains("<i>"))
                {
                    text = HtmlUtil.RemoveOpenCloseTags(text, HtmlUtil.TagItalic);
                }
                else
                {
                    text = $"<i>{text}</i>";
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

            SetButtonsStartOcr();
            progressBar1.Visible = false;
            progressBar1.Maximum = 100;
            progressBar1.Value = 0;
            numericUpDownPixelsIsSpace.Value = vobSubOcrSettings.XOrMorePixelsMakesSpace;
            _vobSubOcrSettings = vobSubOcrSettings;

            InitializeTesseract();
            LoadImageCompareCharacterDatabaseList(Configuration.Settings.VobSubOcr.LastBinaryImageCompareDb);

            if (_palette == null)
            {
                checkBoxCustomFourColors.Checked = true;
            }

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

            SetButtonsStartOcr();
            progressBar1.Visible = false;
            progressBar1.Maximum = 100;
            progressBar1.Value = 0;
            numericUpDownPixelsIsSpace.Value = vobSubOcrSettings.XOrMorePixelsMakesSpace;
            _vobSubOcrSettings = vobSubOcrSettings;

            InitializeTesseract();
            LoadImageCompareCharacterDatabaseList(Configuration.Settings.VobSubOcr.LastBinaryImageCompareDb);

            checkBoxCustomFourColors.Enabled = true;
            checkBoxCustomFourColors.Checked = true;
            autoTransparentBackgroundToolStripMenuItem.Checked = false;
            autoTransparentBackgroundToolStripMenuItem.Visible = false;

            SetOcrMethod();

            FileName = fileName;
            Text += " - " + Path.GetFileName(FileName);

            foreach (XSub subItem in _xSubList)
            {
                var p = new Paragraph(string.Empty, subItem.Start.TotalMilliseconds, subItem.End.TotalMilliseconds);
                _subtitle.Paragraphs.Add(p);
            }

            _subtitle.Renumber();
            subtitleListView1.Fill(_subtitle);
            subtitleListView1.SelectIndexAndEnsureVisible(0);
        }

        private bool HasChangesBeenMade()
        {
            return _subtitle != null && _subtitle.Paragraphs.Any(p => !string.IsNullOrWhiteSpace(p.Text));
        }

        private void VobSubOcr_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!_okClicked && HasChangesBeenMade())
            {
                if (MessageBox.Show(LanguageSettings.Current.VobSubOcr.DiscardText, LanguageSettings.Current.VobSubOcr.DiscardTitle, MessageBoxButtons.YesNo) == DialogResult.No)
                {
                    e.Cancel = true;
                    return;
                }
            }

            _ocrThreadStop = true;
            _abort = true;
            _mainOcrTimer?.Stop();

            _tesseractThreadRunner?.Cancel();
            _tesseractAsyncIndex = 10000;

            System.Threading.Thread.Sleep(100);
            DisposeImageCompareBitmaps();

            Configuration.Settings.VobSubOcr.UseItalicsInTesseract = checkBoxTesseractItalicsOn.Checked;
            if (comboBoxTesseractEngineMode.SelectedIndex != -1)
            {
                Configuration.Settings.VobSubOcr.TesseractEngineMode = comboBoxTesseractEngineMode.SelectedIndex;
            }

            Configuration.Settings.VobSubOcr.ItalicFactor = _unItalicFactor;
            Configuration.Settings.VobSubOcr.FixOcrErrors = checkBoxAutoFixCommonErrors.Checked;
            Configuration.Settings.VobSubOcr.PromptForUnknownWords = checkBoxPromptForUnknownWords.Checked;
            Configuration.Settings.VobSubOcr.GuessUnknownWords = checkBoxGuessUnknownWords.Checked;
            Configuration.Settings.VobSubOcr.AutoBreakSubtitleIfMoreThanTwoLines = checkBoxAutoBreakLines.Checked;
            Configuration.Settings.VobSubOcr.LineOcrDraw = checkBoxNOcrDrawUnknownLetters.Checked;
            Configuration.Settings.VobSubOcr.LineOcrMaxLineHeight = comboBoxNOcrLineSplitMinHeight.SelectedIndex;
            Configuration.Settings.VobSubOcr.LineOcrAdvancedItalic = checkBoxNOcrItalic.Checked;
            Configuration.Settings.VobSubOcr.XOrMorePixelsMakesSpace = (int)numericUpDownPixelsIsSpace.Value;
            if (_ocrMethodIndex == _ocrMethodNocr)
            {
                Configuration.Settings.VobSubOcr.XOrMorePixelsMakesSpace = (int)numericUpDownNumberOfPixelsIsSpaceNOCR.Value;
            }
            Configuration.Settings.VobSubOcr.LineOcrMaxErrorPixels = (int)numericUpDownNOcrMaxWrongPixels.Value;
            Configuration.Settings.VobSubOcr.UseTesseractFallback = checkBoxTesseractFallback.Checked;
            Configuration.Settings.VobSubOcr.CaptureTopAlign = toolStripMenuItemCaptureTopAlign.Checked;

            if (_ocrMethodIndex == _ocrMethodBinaryImageCompare)
            {
                if (comboBoxCharacterDatabase.SelectedItem != null)
                {
                    Configuration.Settings.VobSubOcr.LastBinaryImageCompareDb = comboBoxCharacterDatabase.SelectedItem.ToString();
                }

                if (comboBoxDictionaries.SelectedItem != null)
                {
                    Configuration.Settings.VobSubOcr.LastBinaryImageSpellCheck = comboBoxDictionaries.SelectedItem.ToString();
                }
            }

            if (_ocrMethodIndex == _ocrMethodTesseract4 || _ocrMethodTesseract4 == _ocrMethodTesseract302)
            {
                Configuration.Settings.VobSubOcr.LastTesseractSpellCheck = LanguageString;
            }

            if (_bluRaySubtitlesOriginal != null)
            {
                Configuration.Settings.VobSubOcr.BlurayAllowDifferenceInPercent = (double)numericUpDownMaxErrorPct.Value;
            }
            else
            {
                Configuration.Settings.VobSubOcr.AllowDifferenceInPercent = (double)numericUpDownMaxErrorPct.Value;
            }

            if (_ocrMethodIndex == _ocrMethodNocr)
            {
                Configuration.Settings.VobSubOcr.LineOcrLastSpellCheck = LanguageString;
                if (comboBoxNOcrLanguage.Items.Count > 0 && comboBoxNOcrLanguage.SelectedIndex >= 0)
                {
                    Configuration.Settings.VobSubOcr.LineOcrLastLanguages = comboBoxNOcrLanguage.Items[comboBoxNOcrLanguage.SelectedIndex].ToString();
                }
            }
        }

        private void subtitleListView1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Modifiers == Keys.Control && e.KeyCode == Keys.A)
            {
                subtitleListView1.SelectedIndexChanged -= SubtitleListView1SelectedIndexChanged;
                subtitleListView1.SelectAll();
                subtitleListView1.SelectedIndexChanged += SubtitleListView1SelectedIndexChanged;
                e.SuppressKeyPress = true;
            }

            if (e.Modifiers == Keys.Control && e.KeyCode == Keys.D)
            {
                subtitleListView1.SelectedIndexChanged -= SubtitleListView1SelectedIndexChanged;
                subtitleListView1.SelectFirstSelectedItemOnly();
                subtitleListView1.SelectedIndexChanged += SubtitleListView1SelectedIndexChanged;
                e.SuppressKeyPress = true;
            }
            else if (e.KeyCode == Keys.I && e.Modifiers == (Keys.Control | Keys.Shift)) //InverseSelection
            {
                subtitleListView1.SelectedIndexChanged -= SubtitleListView1SelectedIndexChanged;
                subtitleListView1.InverseSelection();
                subtitleListView1.SelectedIndexChanged += SubtitleListView1SelectedIndexChanged;
                e.SuppressKeyPress = true;
            }
            else if (e.Modifiers == Keys.None && e.KeyCode == Keys.Delete)
            {
                DeleteToolStripMenuItemClick(sender, e);
                e.SuppressKeyPress = true;
                subtitleListView1.Focus();
            }
        }

        private void DeleteToolStripMenuItemClick(object sender, EventArgs e)
        {
            if (subtitleListView1.SelectedItems.Count == 0)
            {
                return;
            }

            string askText;
            if (subtitleListView1.SelectedItems.Count > 1)
            {
                askText = string.Format(LanguageSettings.Current.Main.DeleteXLinesPrompt, subtitleListView1.SelectedItems.Count);
            }
            else
            {
                askText = LanguageSettings.Current.Main.DeleteOneLinePrompt;
            }

            if (Configuration.Settings.General.PromptDeleteLines && MessageBox.Show(askText, "", MessageBoxButtons.YesNo) == DialogResult.No)
            {
                return;
            }

            ResetTesseractThread();

            int selIdx = subtitleListView1.SelectedItems[0].Index;
            List<int> indices = new List<int>();
            foreach (int idx in subtitleListView1.SelectedIndices)
            {
                indices.Add(idx);
            }
            indices.Reverse();

            if (_mp4List != null)
            {
                foreach (int idx in indices)
                {
                    _mp4List.RemoveAt(idx);
                }
            }
            else if (_spList != null)
            {
                foreach (int idx in indices)
                {
                    _spList.RemoveAt(idx);
                }
            }
            else if (_dvbSubtitles != null)
            {
                foreach (int idx in indices)
                {
                    _dvbSubtitles.RemoveAt(idx);
                    _dvbSubColor.RemoveAt(idx);
                }
            }
            else if (_dvbPesSubtitles != null)
            {
                foreach (int idx in indices)
                {
                    _dvbPesSubtitles.RemoveAt(idx);
                }
            }
            else if (_bdnXmlSubtitle != null)
            {
                foreach (int idx in indices)
                {
                    _bdnXmlSubtitle.Paragraphs.RemoveAt(idx);
                }
            }
            else if (_xSubList != null)
            {
                foreach (int idx in indices)
                {
                    _xSubList.RemoveAt(idx);
                }
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
                    var x1 = _vobSubMergedPackList[idx];
                    int i = 0;
                    while (i < _vobSubMergedPackListOriginal.Count)
                    {
                        var x2 = _vobSubMergedPackListOriginal[i];
                        if (Math.Abs(x2.StartTime.TotalMilliseconds - x1.StartTime.TotalMilliseconds) < 0.01)
                        {
                            _vobSubMergedPackListOriginal.Remove(x2);
                            break;
                        }

                        i++;
                    }

                    _vobSubMergedPackList.RemoveAt(idx);
                }
            }

            UpdateLogLineNumbersAfterDelete(listBoxUnknownWords, indices);
            UpdateLogLineNumbersAfterDelete(listBoxLog, indices);
            UpdateLogLineNumbersAfterDelete(listBoxLogSuggestions, indices);

            foreach (int idx in indices)
            {
                _subtitle.Paragraphs.RemoveAt(idx);
            }

            subtitleListView1.Fill(_subtitle);

            if (selIdx < subtitleListView1.Items.Count)
            {
                subtitleListView1.SelectIndexAndEnsureVisible(selIdx, true);
            }
            else
            {
                subtitleListView1.SelectIndexAndEnsureVisible(subtitleListView1.Items.Count - 1, true);
            }

            subtitleListView1.BeginUpdate();
            foreach (var p in _subtitle.Paragraphs)
            {
                if (_unknownWordsDictionary.ContainsKey(p.Id))
                {
                    SetUnknownWordsColor(_subtitle.Paragraphs.IndexOf(p), _unknownWordsDictionary[p.Id], p.Text);
                }
            }

            subtitleListView1.EndUpdate();
        }

        private void UpdateLogLineNumbersAfterDelete(ListBox listBox, List<int> indices)
        {
            int index;
            listBox.BeginUpdate();
            for (int i = listBox.Items.Count - 1; i >= 0; i--)
            {
                if (listBox.Items[i] is LogItem item && (index = item.Line - 1) >= 0)
                {
                    if (indices.Contains(index))
                    {
                        listBox.Items.RemoveAt(i);
                    }
                    else
                    {
                        var c = indices.Count(p => p < index);
                        if (c > 0)
                        {
                            item.Line -= c;
                        }
                    }
                }
            }
            listBox.EndUpdate();
        }

        private void buttonUknownToNames_Click(object sender, EventArgs e)
        {
            if (listBoxUnknownWords.Items.Count > 0 && listBoxUnknownWords.SelectedItems.Count > 0)
            {
                if (listBoxUnknownWords.SelectedItems[0] is LogItem uw && uw.Line > 0)
                {
                    if (_ocrFixEngine == null)
                    {
                        comboBoxDictionaries_SelectedIndexChanged(null, null);
                    }

                    using (var form = new AddToNameList())
                    {
                        form.Initialize(_subtitle, comboBoxDictionaries.Text, uw.Text);
                        if (form.ShowDialog(this) == DialogResult.OK)
                        {
                            comboBoxDictionaries_SelectedIndexChanged(null, null);
                            UpdateUnknownWordColoring(form.NewName, StringComparison.Ordinal);
                            ShowStatus(string.Format(LanguageSettings.Current.Main.NameXAddedToNameList, form.NewName));
                        }
                        else if (!string.IsNullOrEmpty(form.NewName))
                        {
                            MessageBox.Show(string.Format(LanguageSettings.Current.Main.NameXNotAddedToNameList, form.NewName));
                        }
                    }
                }
            }
        }

        private void UpdateUnknownWordColoring(string name, StringComparison comparison)
        {
            if (string.IsNullOrEmpty(name))
            {
                return;
            }

            for (int i = 0; i < _subtitle.Paragraphs.Count; i++)
            {
                var p = _subtitle.Paragraphs[i];
                if (_unknownWordsDictionary.ContainsKey(p.Id))
                {
                    string text = " " + HtmlUtil.RemoveHtmlTags(p.Text, true).Replace(Environment.NewLine, " ") + " ";
                    int start = 0;
                    int count = 0;
                    while (text.IndexOf(name, start, comparison) > start)
                    {
                        count++;
                        start = text.IndexOf(name, start, comparison) + 1;
                    }

                    if (count > 0)
                    {
                        ColorLineByNumberOfUnknownWords(i, _unknownWordsDictionary[p.Id] - count, p.Text);
                    }
                }
            }
        }

        private void buttonUnknownToUserDic_Click(object sender, EventArgs e)
        {
            if (listBoxUnknownWords.Items.Count > 0 && listBoxUnknownWords.SelectedItems.Count > 0)
            {
                if (listBoxUnknownWords.SelectedItems[0] is LogItem uw && uw.Line > 0)
                {
                    using (var form = new AddToUserDic())
                    {
                        form.Initialize(comboBoxDictionaries.Text, uw.Text.ToLowerInvariant());
                        if (form.ShowDialog(this) == DialogResult.OK)
                        {
                            comboBoxDictionaries_SelectedIndexChanged(null, null);
                            UpdateUnknownWordColoring(form.NewWord, StringComparison.OrdinalIgnoreCase);
                            ShowStatus(string.Format(LanguageSettings.Current.Main.WordXAddedToUserDic, form.NewWord));
                        }
                        else if (!string.IsNullOrEmpty(form.NewWord))
                        {
                            MessageBox.Show(string.Format(LanguageSettings.Current.Main.WordXNotAddedToUserDic, form.NewWord));
                        }
                    }
                }
            }
        }

        private void buttonAddToOcrReplaceList_Click(object sender, EventArgs e)
        {
            if (listBoxUnknownWords.Items.Count > 0 && listBoxUnknownWords.SelectedItems.Count > 0)
            {
                if (listBoxUnknownWords.SelectedItems[0] is LogItem uw && uw.Line > 0)
                {
                    using (var form = new AddToOcrReplaceList())
                    {
                        form.Initialize(_languageId, comboBoxDictionaries.Text, uw.Text);
                        if (form.ShowDialog(this) == DialogResult.OK)
                        {
                            comboBoxDictionaries_SelectedIndexChanged(null, null);
                            ShowStatus(string.Format(LanguageSettings.Current.Main.OcrReplacePairXAdded, form.NewSource, form.NewTarget));
                        }
                        else
                        {
                            MessageBox.Show(string.Format(LanguageSettings.Current.Main.OcrReplacePairXNotAdded, form.NewSource, form.NewTarget));
                        }
                    }
                }
            }
        }

        private void buttonGoogleIt_Click(object sender, EventArgs e)
        {
            if (listBoxUnknownWords.Items.Count > 0 && listBoxUnknownWords.SelectedItems.Count > 0)
            {
                if (listBoxUnknownWords.SelectedItems[0] is LogItem uw && uw.Line > 0)
                {
                    UiUtil.OpenUrl("https://www.google.com/search?q=" + Utilities.UrlEncode(uw.Text));
                }
            }
        }

        private void listBoxCopyToClipboard_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.C && e.Modifiers == Keys.Control)
            {
                if (sender is ListBox lb && lb.Items.Count > 0 && lb.SelectedItems.Count > 0)
                {
                    try
                    {
                        string text = lb.SelectedItems[0].ToString();
                        Clipboard.SetText(text);
                    }
                    catch
                    {
                        // ignored
                    }
                }
            }
        }

        private void toolStripMenuItemSetUnItalicFactor_Click(object sender, EventArgs e)
        {
            using (var form = new VobSubOcrSetItalicFactor(GetSubtitleBitmap(_selectedIndex), _unItalicFactor))
            {
                form.ShowDialog(this);
                _unItalicFactor = form.GetUnItalicFactor();
            }
        }

        private PreprocessingSettings _preprocessingSettings;

        private void ImagePreProcessingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _fromMenuItem = true;
            var temp = _preprocessingSettings;
            _preprocessingSettings = null;
            var bmp = GetSubtitleBitmap(_selectedIndex);
            _preprocessingSettings = temp;
            _fromMenuItem = false;

            if (_ocrMethodIndex == _ocrMethodTesseract4)
            {
                var ps = new PreprocessingSettings
                {
                    BinaryImageCompareThreshold = Configuration.Settings.Tools.OcrTesseract4RgbThreshold,
                    InvertColors = _preprocessingSettings != null ? _preprocessingSettings.InvertColors : false,
                    CropTransparentColors = _preprocessingSettings != null ? _preprocessingSettings.CropTransparentColors : false,
                };
                using (var form = new OcrPreprocessingT4(bmp, ps))
                {
                    if (form.ShowDialog(this) == DialogResult.OK)
                    {
                        ResetTesseractThread();
                        _preprocessingSettings = form.PreprocessingSettings;
                        Configuration.Settings.Tools.OcrTesseract4RgbThreshold = _preprocessingSettings.BinaryImageCompareThreshold;
                        SubtitleListView1SelectedIndexChanged(null, null);
                    }
                }

                return;
            }

            using (var form = new OcrPreprocessingSettings(bmp, _ocrMethodIndex == _ocrMethodBinaryImageCompare || _ocrMethodIndex == _ocrMethodNocr, _preprocessingSettings))
            {
                if (form.ShowDialog(this) == DialogResult.OK)
                {
                    ResetTesseractThread();
                    _preprocessingSettings = form.PreprocessingSettings;
                    Configuration.Settings.Tools.OcrBinaryImageCompareRgbThreshold = _preprocessingSettings.BinaryImageCompareThreshold;
                    SubtitleListView1SelectedIndexChanged(null, null);
                }
            }
        }

        internal void VobSubToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ExportToPngXml(ExportPngXml.ExportFormats.VobSub);
        }

        internal void BluraySupToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ExportToPngXml(ExportPngXml.ExportFormats.BluraySup);
        }

        internal void BDNXMLToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ExportToPngXml(ExportPngXml.ExportFormats.BdnXml);
        }

        private void ExportToPngXml(string exportType)
        {
            using (var exportBdnXmlPng = new ExportPngXml())
            {
                _fromMenuItem = true;
                exportBdnXmlPng.InitializeFromVobSubOcr(_subtitle, new SubRip(), exportType, FileName, this, _importLanguageString);

                if (_dvbPesSubtitles != null && _dvbPesSubtitles.Count > 0)
                {
                    var size = _dvbPesSubtitles[0].GetScreenSize();
                    exportBdnXmlPng.SetResolution(new Point(size.Width, size.Height));
                }
                else if (_dvbSubtitles != null && _dvbSubtitles.Count > 0)
                {
                    var size = _dvbSubtitles[0].GetScreenSize();
                    exportBdnXmlPng.SetResolution(new Point(size.Width, size.Height));
                }

                exportBdnXmlPng.ShowDialog(this);
                _fromMenuItem = false;
            }
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
            if (_ocrMethodIndex == _ocrMethodTesseract302)
            {
                using (var form = new GetTesseract302Dictionaries())
                {
                    form.ShowDialog(this);
                    InitializeTesseract(form.ChosenLanguage);
                }

                return;
            }

            using (var form = new GetTesseractDictionaries(comboBoxTesseractLanguages.Items.Count == 0))
            {
                form.ShowDialog(this);
                InitializeTesseract(form.ChosenLanguage);
            }
        }

        private void toolStripMenuItemInspectNOcrMatches_Click(object sender, EventArgs e)
        {
            if (subtitleListView1.SelectedItems.Count != 1)
            {
                return;
            }

            if (_nOcrDb == null)
            {
                LoadNOcrWithCurrentLanguage();
                if (_nOcrDb == null)
                {
                    MessageBox.Show("Unable to load OCR database.", string.Empty, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
            }

            Cursor = Cursors.WaitCursor;
            var bitmap = GetSubtitleBitmap(subtitleListView1.SelectedItems[0].Index);
            Cursor = Cursors.Default;
            using (var inspect = new VobSubNOcrCharacterInspect())
            {
                int minLineHeight = GetMinLineHeight();
                inspect.Initialize(bitmap, _numericUpDownPixelsIsSpace, checkBoxRightToLeftNOCR.Checked, _nOcrDb, this, checkBoxNOcrItalic.Checked, minLineHeight, !checkBoxNOcrDrawUnknownLetters.Checked);
                if (inspect.ShowDialog(this) == DialogResult.OK)
                {
                    Cursor = Cursors.WaitCursor;
                    SaveNOcrWithCurrentLanguage();
                    _nOcrDb.LoadOcrCharacters();
                    Cursor = Cursors.Default;
                }
                else
                {
                    Cursor = Cursors.WaitCursor;
                    _nOcrDb.LoadOcrCharacters();
                    Cursor = Cursors.Default;
                }
            }
        }

        private void buttonLineOcrEditLanguage_Click(object sender, EventArgs e)
        {
            if (_nOcrDb == null)
            {
                if (string.IsNullOrEmpty(GetNOcrLanguageFileName()))
                {
                    MessageBox.Show("No line OCR language loaded - please re-install");
                    return;
                }

                LoadNOcrWithCurrentLanguage();
            }

            using (var form = new VobSubNOcrEdit(_nOcrDb, null, _nOcrDb.FileName))
            {
                if (form.ShowDialog(this) == DialogResult.OK && form.Changed)
                {
                    Cursor = Cursors.WaitCursor;
                    try
                    {
                        SaveNOcrWithCurrentLanguage();
                    }
                    finally
                    {
                        Cursor = Cursors.Default;
                    }
                }
                else
                {
                    LoadNOcrWithCurrentLanguage();
                }
            }
        }

        private void buttonLineOcrNewLanguage_Click(object sender, EventArgs e)
        {
            using (var newFolder = new VobSubOcrNewFolder(false))
            {
                if (newFolder.ShowDialog(this) == DialogResult.OK)
                {
                    string s = newFolder.FolderName;
                    if (string.IsNullOrEmpty(s))
                    {
                        return;
                    }

                    s = s.RemoveChar('?', '/', '*', '\\');
                    if (string.IsNullOrEmpty(s))
                    {
                        return;
                    }

                    if (File.Exists(Configuration.DictionariesDirectory + "nOCR_" + newFolder.FolderName + ".xml"))
                    {
                        MessageBox.Show("Line OCR language file already exists!");
                        return;
                    }

                    _nOcrDb = null;
                    comboBoxNOcrLanguage.Items.Add(s);
                    comboBoxNOcrLanguage.SelectedIndex = comboBoxNOcrLanguage.Items.Count - 1;
                }
            }
        }

        private void comboBoxNOcrLanguage_SelectedIndexChanged(object sender, EventArgs e)
        {
            _nOcrDb = null;
        }

        private void buttonSpellCheckDownload_Click(object sender, EventArgs e)
        {
            using (var form = new GetDictionaries())
            {
                form.ShowDialog(this);
                FillSpellCheckDictionaries();
                if (form.LastDownload != null && form.LastDownload.Length > 3 && !string.IsNullOrEmpty(form.SelectedEnglishName))
                {
                    var lc = Path.GetFileNameWithoutExtension(form.LastDownload.Substring(0, 4).Replace('_', '-'));
                    for (int i = 0; i < comboBoxDictionaries.Items.Count; i++)
                    {
                        string item = (string)comboBoxDictionaries.Items[i];
                        if (item.Contains("[" + lc) || item.Contains(form.SelectedEnglishName))
                        {
                            comboBoxDictionaries.SelectedIndex = i;
                            break;
                        }
                    }
                }
            }

            if (comboBoxDictionaries.Items.Count > 0 && comboBoxDictionaries.SelectedIndex < 0)
            {
                comboBoxDictionaries.SelectedIndex = 0;
            }
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

        internal void DOSTToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (var exportBdnXmlPng = new ExportPngXml())
            {
                _fromMenuItem = true;
                exportBdnXmlPng.InitializeFromVobSubOcr(_subtitle, new SubRip(), ExportPngXml.ExportFormats.Dost, FileName, this, _importLanguageString);
                exportBdnXmlPng.ShowDialog(this);
                _fromMenuItem = false;
            }
        }

        internal void finalCutProImageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (var exportBdnXmlPng = new ExportPngXml())
            {
                _fromMenuItem = true;
                exportBdnXmlPng.InitializeFromVobSubOcr(_subtitle, new SubRip(), ExportPngXml.ExportFormats.Fcp, FileName, this, _importLanguageString);
                exportBdnXmlPng.ShowDialog(this);
                _fromMenuItem = false;
            }
        }

        internal void Initialize(List<TransportStreamSubtitle> subtitles, VobSubOcrSettings vobSubOcrSettings, string fileName, string language, bool skipMakeBinary = false)
        {
            SetButtonsStartOcr();
            progressBar1.Visible = false;
            progressBar1.Maximum = 100;
            progressBar1.Value = 0;
            numericUpDownPixelsIsSpace.Value = vobSubOcrSettings.XOrMorePixelsMakesSpace;
            numericUpDownNumberOfPixelsIsSpaceNOCR.Value = vobSubOcrSettings.XOrMorePixelsMakesSpace;
            _vobSubOcrSettings = vobSubOcrSettings;

            InitializeTesseract(language);
            SetTesseractLanguageFromLanguageString(language);
            LoadImageCompareCharacterDatabaseList(Configuration.Settings.VobSubOcr.LastBinaryImageCompareDb);

            SetOcrMethod();

            _dvbSubtitles = subtitles;
            InitializeDvbSubColor();

            ShowDvbSubs();

            FileName = fileName;
            _subtitle.FileName = fileName;
            Text += " - " + Path.GetFileName(fileName);

            groupBoxImagePalette.Visible = false;
            groupBoxTransportStream.Left = groupBoxImagePalette.Left;
            groupBoxTransportStream.Top = groupBoxImagePalette.Top;
            groupBoxTransportStream.Visible = true;
            checkBoxTransportStreamGetColorAndSplit.Visible = subtitles.Count > 0 && subtitles[0].IsDvbSub;
            _fromMenuItem = skipMakeBinary;
        }

        private void InitializeDvbSubColor()
        {
            _dvbSubColor = new List<Color>(_dvbSubtitles.Count);
            for (int i = 0; i < _dvbSubtitles.Count; i++)
            {
                _dvbSubColor.Add(Color.Transparent);
            }
        }

        private void checkBoxTransportStreamGrayscale_CheckedChanged(object sender, EventArgs e)
        {
            SubtitleListView1SelectedIndexChanged(null, null);
        }

        private void checkBoxTransportStreamGetColorAndSplit_CheckedChanged(object sender, EventArgs e)
        {
            _transportStreamUseColor = checkBoxTransportStreamGetColorAndSplit.Checked;

            if (_ocrMethodIndex == _ocrMethodTesseract4)
            {
                _abort = true;
                ResetTesseractThread();
            }

            if (checkBoxTransportStreamGetColorAndSplit.Checked)
            {
                SplitDvbForEachSubImage();
            }
            else
            {
                MergeDvbForEachSubImage();
            }

            InitializeDvbSubColor();
            SubtitleListView1SelectedIndexChanged(null, null);
        }

        private void MergeDvbForEachSubImage()
        {
            int i = 0;
            while (i < _dvbSubtitles.Count)
            {
                var dvbSub = _dvbSubtitles[i];
                dvbSub.TransportStreamPosition = null;
                dvbSub.ActiveImageIndex = null;
                if (i < _dvbSubtitles.Count - 1 && dvbSub.Pes == _dvbSubtitles[i + 1].Pes)
                {
                    var next = _subtitle.GetParagraphOrDefault(i + 1);
                    if (!string.IsNullOrEmpty(next.Text))
                    {
                        var p = _subtitle.Paragraphs[i];
                        p.Text = (p.Text + Environment.NewLine + next.Text).Trim();
                    }

                    _subtitle.Paragraphs.RemoveAt(i + 1);
                    _dvbSubtitles.RemoveAt(i + 1);
                }
                else
                {
                    i++;
                }
            }

            _tesseractAsyncStrings = null;
            _subtitle.Renumber();
            subtitleListView1.Fill(_subtitle);
            subtitleListView1.SelectIndexAndEnsureVisible(0);
        }

        private void SplitDvbForEachSubImage()
        {
            var list = new List<TransportStreamSubtitle>();
            foreach (var dvbSub in _dvbSubtitles)
            {
                if (dvbSub.ActiveImageIndex == null)
                {
                    var tempList = new List<TransportStreamSubtitle>();
                    for (int i = 0; i < dvbSub.Pes.ObjectDataList.Count; i++)
                    {
                        var ods = dvbSub.Pes.ObjectDataList[i];
                        if (ods.TopFieldDataBlockLength > 8)
                        {
                            var pos = dvbSub.Pes.GetImagePosition(ods);
                            tempList.Add(new TransportStreamSubtitle
                            {
                                Pes = dvbSub.Pes,
                                ActiveImageIndex = i,
                                StartMilliseconds = dvbSub.StartMilliseconds,
                                EndMilliseconds = dvbSub.EndMilliseconds,
                                TransportStreamPosition = new Position(pos.X, pos.Y)
                            });
                        }
                    }

                    if (tempList.Count > 1)
                    {
                        var lastColor = Color.Transparent;
                        bool allAlike = true;
                        foreach (var item in tempList)
                        {
                            var dvbBmp = item.GetBitmap();
                            var nDvbBmp = new NikseBitmap(dvbBmp);
                            var color = nDvbBmp.GetBrightestColor();
                            if (lastColor != Color.Transparent && (Math.Abs(color.R - lastColor.R) > 10 || Math.Abs(color.G - lastColor.G) > 10 || Math.Abs(color.B - lastColor.B) > 10))
                            {
                                allAlike = false;
                                break;
                            }

                            lastColor = color;
                        }

                        if (allAlike)
                        {
                            tempList.Clear();
                            tempList.Add(dvbSub);
                        }
                    }

                    list.AddRange(tempList);
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

            _subtitle.Renumber();
            subtitleListView1.Fill(_subtitle);
            subtitleListView1.SelectIndexAndEnsureVisible(0);
        }

        private void numericUpDownAutoTransparentAlphaMax_ValueChanged(object sender, EventArgs e)
        {
            SubtitleListView1SelectedIndexChanged(null, null);
        }

        private void toolStripMenuItemImageSaveAs_Click(object sender, EventArgs e)
        {
            SaveImageAsToolStripMenuItemClick(sender, e);
        }

        private void OcrTrainingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_ocrMethodIndex == _ocrMethodBinaryImageCompare)
            {
                using (var form = new BinaryOcrTrain())
                {
                    form.ShowDialog(this);
                }
            }
            else if (_ocrMethodIndex == _ocrMethodNocr)
            {
                using (var form = new VobSubNOcrTrain())
                {
                    form.Initialize();
                    form.ShowDialog(this);
                    ComboBoxOcrMethodSelectedIndexChanged(null, null);
                }
            }
        }

        private OcrFixEngine.AutoGuessLevel GetAutoGuessLevel()
        {
            var autoGuessLevel = OcrFixEngine.AutoGuessLevel.None;
            if (checkBoxGuessUnknownWords.Checked)
            {
                autoGuessLevel = OcrFixEngine.AutoGuessLevel.Aggressive;
            }

            return autoGuessLevel;
        }

        private void importNewTimeCodesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            openFileDialog1.Title = LanguageSettings.Current.General.OpenSubtitle;
            openFileDialog1.FileName = string.Empty;
            openFileDialog1.Filter = UiUtil.SubtitleExtensionFilter.Value;
            if (openFileDialog1.ShowDialog(this) == DialogResult.OK)
            {
                string fileName = openFileDialog1.FileName;
                if (!File.Exists(fileName))
                {
                    return;
                }

                var fi = new FileInfo(fileName);
                if (fi.Length > 1024 * 1024 * 10) // max 10 mb
                {
                    if (MessageBox.Show(string.Format(LanguageSettings.Current.Main.FileXIsLargerThan10MB + Environment.NewLine +
                                                      Environment.NewLine +
                                                      LanguageSettings.Current.Main.ContinueAnyway,
                            fileName), Text, MessageBoxButtons.YesNoCancel) != DialogResult.Yes)
                    {
                        return;
                    }
                }

                var sub = new Subtitle();
                SubtitleFormat format = sub.LoadSubtitle(fileName, out _, null);
                if (format == null)
                {
                    return;
                }

                int index = 0;
                int newSubCount = sub.Paragraphs.Count;
                int currentSubCount = _subtitle.Paragraphs.Count;
                while (index < newSubCount && index < currentSubCount)
                {
                    Paragraph newP = sub.Paragraphs[index];
                    Paragraph currentP = _subtitle.Paragraphs[index];
                    currentP.StartTime.TotalMilliseconds = newP.StartTime.TotalMilliseconds;
                    currentP.EndTime.TotalMilliseconds = newP.EndTime.TotalMilliseconds;
                    subtitleListView1.SetStartTimeAndDuration(index, currentP, _subtitle.GetParagraphOrDefault(index + 1), _subtitle.GetParagraphOrDefault(index - 1));
                    index++;
                }
            }
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
                if (_ocrFixEngine != null)
                {
                    _ocrFixEngine.Dispose();
                    _ocrFixEngine = null;
                }
            }

            base.Dispose(disposing);
        }

        private void numericUpDownMaxErrorPct_ValueChanged(object sender, EventArgs e)
        {
            _numericUpDownMaxErrorPct = (double)numericUpDownMaxErrorPct.Value;
        }

        private void subtitleListView1_DoubleClick(object sender, EventArgs e)
        {
            if (subtitleListView1.SelectedItems.Count > 0 && _ocrMethodIndex == _ocrMethodBinaryImageCompare)
            {
                InspectImageCompareMatchesForCurrentImageToolStripMenuItem_Click(null, null);
            }
            else if (subtitleListView1.SelectedItems.Count > 0 && _ocrMethodIndex == _ocrMethodNocr)
            {
                toolStripMenuItemInspectNOcrMatches_Click(null, null);
            }
        }

        private void comboBoxTesseractEngineMode_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_tesseractAsyncStrings != null)
            {
                _tesseractAsyncStrings = new string[GetSubtitleCount()];
            }
        }

        private void toolStripMenuItemSaveSubtitleAs_Click(object sender, EventArgs e)
        {
            var format = new SubRip();
            saveFileDialog1.Title = LanguageSettings.Current.Main.Menu.File.SaveAs;
            saveFileDialog1.FileName = Path.GetFileNameWithoutExtension(FileName);
            saveFileDialog1.Filter = format.Name + "|*" + format.Extension;
            saveFileDialog1.DefaultExt = "*" + format.Extension;
            saveFileDialog1.AddExtension = true;
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                var allText = format.ToText(_subtitle, null);
                File.WriteAllText(saveFileDialog1.FileName, allText, Encoding.UTF8);
                FileName = saveFileDialog1.FileName;
            }
        }

        private void SelectBestImageCompareDatabase()
        {
            if (_ocrMethodIndex == _ocrMethodBinaryImageCompare)
            {
                var s = FindBestImageCompareDatabase();
                if (string.IsNullOrEmpty(s))
                {
                    return;
                }

                if (Configuration.Settings.VobSubOcr.LastBinaryImageCompareDb != null &&
                    Configuration.Settings.VobSubOcr.LastBinaryImageCompareDb.Contains("+"))
                {
                    s += "+" + Configuration.Settings.VobSubOcr.LastBinaryImageCompareDb.Split('+')[1];
                }

                Configuration.Settings.VobSubOcr.LastBinaryImageCompareDb = s;
                for (int i = 0; i < comboBoxCharacterDatabase.Items.Count; i++)
                {
                    if (comboBoxCharacterDatabase.Items[i].ToString() == Configuration.Settings.VobSubOcr.LastBinaryImageCompareDb)
                    {
                        comboBoxCharacterDatabase.SelectedIndex = i;
                        return;
                    }
                }

                comboBoxCharacterDatabase.Items.Insert(0, s);
                comboBoxCharacterDatabase.SelectedIndex = 0;
            }
        }

        private string FindBestImageCompareDatabase()
        {
            var bestDbName = string.Empty;
            int bestHits = -1;
            foreach (string s in BinaryOcrDb.GetDatabases())
            {
                var binaryOcrDb = new BinaryOcrDb(Path.Combine(Configuration.OcrDirectory, s + ".db"), true);
                var bitmap = GetSubtitleBitmap(0);
                if (bitmap == null)
                {
                    return string.Empty;
                }
                var parentBitmap = new NikseBitmap(bitmap);
                int minLineHeight = GetMinLineHeight();
                var sourceList = NikseBitmapImageSplitter.SplitBitmapToLettersNew(parentBitmap, (int)numericUpDownPixelsIsSpace.Value, checkBoxRightToLeft.Checked, Configuration.Settings.VobSubOcr.TopToBottom, minLineHeight, _autoLineHeight);
                int index = 0;
                int hits = 0;
                foreach (var item in sourceList)
                {
                    if (item?.NikseBitmap != null && GetCompareMatchNew(item, out _, sourceList, index, binaryOcrDb) != null)
                    {
                        hits++;
                    }
                }

                if (hits > bestHits)
                {
                    bestDbName = s;
                    bestHits = hits;
                }
            }
            return bestDbName;
        }

        private int GetLastBinOcrLowercaseHeight()
        {
            var lowercaseHeight = 25;
            if (_ocrLowercaseHeightsTotalCount > 5)
            {
                lowercaseHeight = (int)Math.Round((double)_ocrLowercaseHeightsTotal / _ocrLowercaseHeightsTotalCount);
            }

            return lowercaseHeight;
        }

        private int GetLastBinOcrUppercaseHeight()
        {
            var uppercaseHeight = 35;
            if (_ocrUppercaseHeightsTotalCount > 5)
            {
                uppercaseHeight = (int)Math.Round((double)_ocrUppercaseHeightsTotal / _ocrUppercaseHeightsTotalCount);
            }

            return uppercaseHeight;
        }

        private void contextMenuStripImage_Opening(object sender, CancelEventArgs e)
        {
            GetSubtitleScreenSize(_selectedIndex, out var width, out var height);
            previewToolStripMenuItem.Visible = width > 0 && height > 0;
        }

        private void previewToolStripMenuItem_Click(object sender, EventArgs e)
        {
            GetSubtitleScreenSize(_selectedIndex, out var width, out var height);
            if (width == 0 || height == 0)
            {
                return;
            }

            bool goNext;
            bool goPrevious;
            Cursor = Cursors.WaitCursor;
            try
            {
                GetSubtitleTopAndHeight(_selectedIndex, out var left, out var top, out _, out _);
                using (var bmp = new Bitmap(width, height))
                {
                    using (var g = Graphics.FromImage(bmp))
                    {
                        var p = _subtitle.Paragraphs[subtitleListView1.SelectedItems[0].Index];
                        FillPreviewBackground(bmp, g, p);
                        _fromMenuItem = true;
                        var subBitmap = GetSubtitleBitmap(_selectedIndex, false);
                        _fromMenuItem = false;
                        g.DrawImageUnscaled(subBitmap, new Point(left, top));
                    }

                    using (var form = new ExportPngXmlPreview(bmp))
                    {

                        Cursor = Cursors.Default;
                        form.AllowNext = _selectedIndex < _subtitle.Paragraphs.Count - 1;
                        form.AllowPrevious = _selectedIndex > 0;
                        form.ShowDialog(this);
                        goNext = form.NextPressed;
                        goPrevious = form.PreviousPressed;
                    }
                }
            }
            finally
            {
                Cursor = Cursors.Default;
            }

            if (goNext)
            {
                subtitleListView1.SelectIndexAndEnsureVisible(_selectedIndex + 1);
                previewToolStripMenuItem_Click(null, null);
            }
            else if (goPrevious)
            {
                subtitleListView1.SelectIndexAndEnsureVisible(_selectedIndex - 1);
                previewToolStripMenuItem_Click(null, null);
            }
        }

        private void FillPreviewBackground(Bitmap bmp, Graphics g, Paragraph p)
        {
            // Draw background with generated image
            var rect = new Rectangle(0, 0, bmp.Width - 1, bmp.Height - 1);
            using (var br = new LinearGradientBrush(rect, Color.Black, Color.Black, 0, false))
            {
                var cb = new ColorBlend
                {
                    Positions = new[] { 0, 1 / 6f, 2 / 6f, 3 / 6f, 4 / 6f, 5 / 6f, 1 },
                    Colors = new[] { Color.Black, Color.Black, Color.White, Color.Black, Color.Black, Color.White, Color.Black }
                };
                br.InterpolationColors = cb;
                br.RotateTransform(0);
                g.FillRectangle(br, rect);
            }
        }

        private void captureTopAlignmentToolStripMenuItem_Click(object sender, EventArgs e)
        {
            toolStripMenuItemCaptureTopAlign.Checked = captureTopAlignmentToolStripMenuItem.Checked;
        }

        private void toolStripMenuItemCaptureTopAlign_Click(object sender, EventArgs e)
        {
            captureTopAlignmentToolStripMenuItem.Checked = toolStripMenuItemCaptureTopAlign.Checked;
        }

        private void imagePreprocessingToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            ImagePreProcessingToolStripMenuItem_Click(null, null);
        }

        private void setItalicAngleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            toolStripMenuItemSetUnItalicFactor_Click(null, null);
        }

        private void buttonChooseEditBinaryImageCompareDb_Click(object sender, EventArgs e)
        {
            using (var form = new BinaryOcrChooseEditDb(comboBoxCharacterDatabase.Text))
            {
                if (form.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }

                LoadImageCompareCharacterDatabaseList(form.ImageCompareDatabaseName);
            }
        }

        private void numericUpDownNumberOfPixelsIsSpaceNOCR_ValueChanged(object sender, EventArgs e)
        {
            if (_ocrMethodIndex == _ocrMethodNocr)
            {
                numericUpDownPixelsIsSpace.Value = numericUpDownNumberOfPixelsIsSpaceNOCR.Value;
            }

            _numericUpDownPixelsIsSpace = (int)numericUpDownNumberOfPixelsIsSpaceNOCR.Value;
        }

        private void numericUpDownPixelsIsSpace_ValueChanged(object sender, EventArgs e)
        {
            if (_ocrMethodIndex == _ocrMethodBinaryImageCompare)
            {
                numericUpDownNumberOfPixelsIsSpaceNOCR.Value = numericUpDownPixelsIsSpace.Value;
            }

            _numericUpDownPixelsIsSpace = (int)numericUpDownNumberOfPixelsIsSpaceNOCR.Value;
        }

        private void cutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            textBoxCurrentText.Cut();
        }

        private void copyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            textBoxCurrentText.Copy();
        }

        private void pasteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            textBoxCurrentText.Paste();
        }

        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            textBoxCurrentText.SelectedText = string.Empty;
        }

        private void selectAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            textBoxCurrentText.SelectAll();
        }

        private void normalToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            var tb = textBoxCurrentText;
            if (tb.SelectionLength == 0)
            {
                var allText = HtmlUtil.RemoveHtmlTags(tb.Text);
                allText = NetflixImsc11Japanese.RemoveTags(allText);
                tb.Text = allText;
                return;
            }

            string text = tb.SelectedText;
            int selectionStart = tb.SelectionStart;
            text = HtmlUtil.RemoveHtmlTags(text);
            text = NetflixImsc11Japanese.RemoveTags(text);
            tb.SelectedText = text;
            tb.SelectionStart = selectionStart;
            tb.SelectionLength = text.Length;
        }

        private void TextBoxListViewToggleTag(string tag)
        {
            var tb = textBoxCurrentText;
            string text;
            int selectionStart = tb.SelectionStart;

            // No text selected.
            if (tb.SelectedText.Length == 0)
            {
                text = tb.Text;

                // Split lines (split a subtitle into its lines).
                var lines = text.SplitToLines();

                // Get current line index (the line where the cursor is current located).
                int numberOfNewLines = 0;
                for (int i = 0; i < tb.SelectionStart && i < text.Length; i++)
                {
                    if (text[i] == '\n')
                    {
                        numberOfNewLines++;
                    }
                }
                int selectedLineIdx = numberOfNewLines; // Do not use 'GetLineFromCharIndex' as it also counts when lines are wrapped

                // Get line from index.
                string selectedLine = lines[selectedLineIdx];

                // Test if line at where cursor is current at is a dialog.
                bool isDialog = selectedLine.StartsWith('-') ||
                                selectedLine.StartsWith("<" + tag + ">-", StringComparison.OrdinalIgnoreCase);

                // Will be used keep cursor in its previous location after toggle/untoggle.
                int textLen = text.Length;

                // 1st set the cursor position to zero.
                tb.SelectionStart = 0;

                // If is dialog, only toggle/Untoggle line where caret/cursor is current at.
                if (isDialog)
                {
                    lines[selectedLineIdx] = HtmlUtil.ToggleTag(selectedLine, tag, false, false);
                    text = string.Join(Environment.NewLine, lines);
                }
                else
                {
                    text = HtmlUtil.ToggleTag(text, tag, false, false);
                }

                tb.Text = text;
                // Note: Math.Max will prevent blowing if caret is at the begining and tag was untoggled.
                tb.SelectionStart = textLen > text.Length ? Math.Max(selectionStart - 3, 0) : selectionStart + 3;
            }
            else
            {
                string post = string.Empty;
                string pre = string.Empty;
                // There is text selected
                text = tb.SelectedText;
                while (text.EndsWith(' ') || text.EndsWith(Environment.NewLine, StringComparison.Ordinal) || text.StartsWith(' ') || text.StartsWith(Environment.NewLine, StringComparison.Ordinal))
                {
                    if (text.EndsWith(' '))
                    {
                        post += " ";
                        text = text.Remove(text.Length - 1);
                    }

                    if (text.EndsWith(Environment.NewLine, StringComparison.Ordinal))
                    {
                        post += Environment.NewLine;
                        text = text.Remove(text.Length - 2);
                    }

                    if (text.StartsWith(' '))
                    {
                        pre += " ";
                        text = text.Remove(0, 1);
                    }

                    if (text.StartsWith(Environment.NewLine, StringComparison.Ordinal))
                    {
                        pre += Environment.NewLine;
                        text = text.Remove(0, 2);
                    }
                }

                text = HtmlUtil.ToggleTag(text, tag, false, false);
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

        private void boldToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            TextBoxListViewToggleTag(HtmlUtil.TagBold);
        }

        private void italicToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            TextBoxListViewToggleTag(HtmlUtil.TagItalic);
        }

        private void underlineToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            TextBoxListViewToggleTag(HtmlUtil.TagUnderline);
        }
    }
}
