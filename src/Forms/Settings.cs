﻿using Nikse.SubtitleEdit.Core;
using Nikse.SubtitleEdit.Core.Dictionaries;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.VideoPlayers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace Nikse.SubtitleEdit.Forms
{
    public sealed partial class Settings : PositionAndSizeForm
    {
        private string _ssaFontName;
        private double _ssaFontSize;
        private int _ssaFontColor;
        private string _listBoxSearchString = string.Empty;
        private DateTime _listBoxSearchStringLastUsed = DateTime.Now;
        private List<string> _wordListNamesEtc = new List<string>();
        private List<string> _userWordList = new List<string>();
        private OcrFixReplaceList _ocrFixReplaceList;
        private readonly string _oldVlcLocation;
        private readonly string _oldVlcLocationRelative;
        private readonly bool _oldListViewShowCps;
        private readonly bool _oldListViewShowWpm;

        private readonly Dictionary<ShortcutHelper, string> _newShortcuts = new Dictionary<ShortcutHelper, string>();

        private class ComboBoxLanguage
        {
            public CultureInfo CultureInfo { get; set; }
            public override string ToString()
            {
                return CultureInfo.NativeName.CapitalizeFirstLetter();
            }
        }

        public class ShortcutHelper
        {
            public ShortcutHelper(PropertyInfo shortcut, bool isMenuItem)
            {
                Shortcut = shortcut;
                IsMenuItem = isMenuItem;
            }
            public PropertyInfo Shortcut { get; set; }
            public bool IsMenuItem { get; set; }
        }

        private static string GetRelativePath(string fileName)
        {
            string folder = Configuration.BaseDirectory;

            if (string.IsNullOrEmpty(fileName) || !fileName.StartsWith(folder.Substring(0, 2), StringComparison.OrdinalIgnoreCase))
                return string.Empty;

            var pathUri = new Uri(fileName);
            if (!folder.EndsWith(Path.DirectorySeparatorChar))
                folder += Path.DirectorySeparatorChar;
            var folderUri = new Uri(folder);
            return Uri.UnescapeDataString(folderUri.MakeRelativeUri(pathUri).ToString().Replace('/', Path.DirectorySeparatorChar));
        }

        public Settings()
        {
            InitializeComponent();

            labelStatus.Text = string.Empty;

            var gs = Configuration.Settings.General;

            checkBoxToolbarNew.Checked = gs.ShowToolbarNew;
            checkBoxToolbarOpen.Checked = gs.ShowToolbarOpen;
            checkBoxToolbarSave.Checked = gs.ShowToolbarSave;
            checkBoxToolbarSaveAs.Checked = gs.ShowToolbarSaveAs;
            checkBoxToolbarFind.Checked = gs.ShowToolbarFind;
            checkBoxReplace.Checked = gs.ShowToolbarReplace;
            checkBoxTBFixCommonErrors.Checked = gs.ShowToolbarFixCommonErrors;
            checkBoxTBRemoveTextForHi.Checked = gs.ShowToolbarRemoveTextForHi;
            checkBoxVisualSync.Checked = gs.ShowToolbarVisualSync;
            checkBoxSettings.Checked = gs.ShowToolbarSettings;
            checkBoxSpellCheck.Checked = gs.ShowToolbarSpellCheck;
            checkBoxNetflixQualityCheck.Checked = gs.ShowToolbarNetflixGlyphCheck;
            checkBoxHelp.Checked = gs.ShowToolbarHelp;

            comboBoxFrameRate.Items.Add((23.976).ToString(CultureInfo.CurrentCulture));
            comboBoxFrameRate.Items.Add((24.0).ToString(CultureInfo.CurrentCulture));
            comboBoxFrameRate.Items.Add((25.0).ToString(CultureInfo.CurrentCulture));
            comboBoxFrameRate.Items.Add((29.97).ToString(CultureInfo.CurrentCulture));
            comboBoxFrameRate.Items.Add((30.00).ToString(CultureInfo.CurrentCulture));

            checkBoxShowFrameRate.Checked = gs.ShowFrameRate;
            comboBoxFrameRate.Text = gs.DefaultFrameRate.ToString(CultureInfo.CurrentCulture);

            UiUtil.InitializeTextEncodingComboBox(comboBoxEncoding);

            checkBoxAutoDetectAnsiEncoding.Checked = gs.AutoGuessAnsiEncoding;
            comboBoxSubtitleFontSize.Text = gs.SubtitleFontSize.ToString(CultureInfo.InvariantCulture);
            checkBoxSubtitleFontBold.Checked = gs.SubtitleFontBold;
            checkBoxSubtitleCenter.Checked = gs.CenterSubtitleInTextBox;
            panelSubtitleFontColor.BackColor = gs.SubtitleFontColor;
            panelSubtitleBackgroundColor.BackColor = gs.SubtitleBackgroundColor;
            checkBoxRememberRecentFiles.Checked = gs.ShowRecentFiles;
            checkBoxRememberRecentFiles_CheckedChanged(null, null);
            checkBoxRememberSelectedLine.Checked = gs.RememberSelectedLine;
            checkBoxReopenLastOpened.Checked = gs.StartLoadLastFile;
            checkBoxStartInSourceView.Checked = gs.StartInSourceView;
            checkBoxRemoveBlankLinesWhenOpening.Checked = gs.RemoveBlankLinesWhenOpening;
            checkBoxRememberWindowPosition.Checked = gs.StartRememberPositionAndSize;
            numericUpDownSubtitleLineMaximumLength.Value = gs.SubtitleLineMaximumLength;
            numericUpDownMaxCharsSec.Value = (decimal)gs.SubtitleMaximumCharactersPerSeconds;
            numericUpDownMaxWordsMin.Value = (decimal)gs.SubtitleMaximumWordsPerMinute;
            checkBoxAutoWrapWhileTyping.Checked = gs.AutoWrapLineWhileTyping;
            textBoxShowLineBreaksAs.Text = gs.ListViewLineSeparatorString;

            numericUpDownDurationMin.Value = gs.SubtitleMinimumDisplayMilliseconds;
            numericUpDownDurationMax.Value = gs.SubtitleMaximumDisplayMilliseconds;

            if (gs.MinimumMillisecondsBetweenLines >= numericUpDownMinGapMs.Minimum &&
                gs.MinimumMillisecondsBetweenLines <= numericUpDownMinGapMs.Maximum)
                numericUpDownMinGapMs.Value = gs.MinimumMillisecondsBetweenLines;

            if (gs.VideoPlayer.Trim().Equals("VLC", StringComparison.OrdinalIgnoreCase) && LibVlcDynamic.IsInstalled)
                radioButtonVideoPlayerVLC.Checked = true;
            else if (gs.VideoPlayer.Trim().Equals("MPV", StringComparison.OrdinalIgnoreCase) && LibMpvDynamic.IsInstalled)
                radioButtonVideoPlayerMPV.Checked = true;
            else if (gs.VideoPlayer.Trim().Equals("MPC-HC", StringComparison.OrdinalIgnoreCase) && UiUtil.IsMpcHcInstalled)
                radioButtonVideoPlayerMpcHc.Checked = true;
            else if (UiUtil.IsQuartsDllInstalled)
                radioButtonVideoPlayerDirectShow.Checked = true;
            else if (LibMpvDynamic.IsInstalled)
                radioButtonVideoPlayerMPV.Checked = true;
            else if (LibVlcDynamic.IsInstalled)
                radioButtonVideoPlayerVLC.Checked = true;

            if (!LibVlcDynamic.IsInstalled)
                radioButtonVideoPlayerVLC.Enabled = false;
            if (!UiUtil.IsQuartsDllInstalled)
                radioButtonVideoPlayerDirectShow.Enabled = false;
            if (Logic.VideoPlayers.MpcHC.MpcHc.GetMpcHcFileName() == null)
                radioButtonVideoPlayerMpcHc.Enabled = false;
            RefreshMpvSettings();
            buttonMpvSettings.Text = Configuration.Settings.Language.SettingsMpv.DownloadMpv;
            checkBoxMpvHandlesPreviewText.Checked = gs.MpvHandlesPreviewText;

            textBoxVlcPath.Text = gs.VlcLocation;
            textBoxVlcPath.Left = labelVideoPlayerVLC.Left + labelVideoPlayerVLC.Width + 5;
            textBoxVlcPath.Width = buttonVlcPathBrowse.Left - textBoxVlcPath.Left - 5;

            labelVlcPath.Text = Configuration.Settings.Language.Settings.VlcBrowseToLabel;

            checkBoxVideoPlayerShowStopButton.Checked = gs.VideoPlayerShowStopButton;
            checkBoxVideoPlayerShowMuteButton.Checked = gs.VideoPlayerShowMuteButton;
            checkBoxVideoPlayerShowFullscreenButton.Checked = gs.VideoPlayerShowFullscreenButton;

            int videoPlayerPreviewFontSizeIndex = gs.VideoPlayerPreviewFontSize - int.Parse(comboBoxlVideoPlayerPreviewFontSize.Items[0].ToString());
            if (videoPlayerPreviewFontSizeIndex >= 0 && videoPlayerPreviewFontSizeIndex < comboBoxlVideoPlayerPreviewFontSize.Items.Count)
                comboBoxlVideoPlayerPreviewFontSize.SelectedIndex = videoPlayerPreviewFontSizeIndex;
            else
                comboBoxlVideoPlayerPreviewFontSize.SelectedIndex = 3;
            checkBoxVideoPlayerPreviewFontBold.Checked = gs.VideoPlayerPreviewFontBold;

            checkBoxVideoAutoOpen.Checked = !gs.DisableVideoAutoLoading;

            comboBoxCustomSearch1.Text = Configuration.Settings.VideoControls.CustomSearchText1;
            comboBoxCustomSearch2.Text = Configuration.Settings.VideoControls.CustomSearchText2;
            comboBoxCustomSearch3.Text = Configuration.Settings.VideoControls.CustomSearchText3;
            comboBoxCustomSearch4.Text = Configuration.Settings.VideoControls.CustomSearchText4;
            comboBoxCustomSearch5.Text = Configuration.Settings.VideoControls.CustomSearchText5;
            comboBoxCustomSearch6.Text = Configuration.Settings.VideoControls.CustomSearchText6;
            textBoxCustomSearchUrl1.Text = Configuration.Settings.VideoControls.CustomSearchUrl1;
            textBoxCustomSearchUrl2.Text = Configuration.Settings.VideoControls.CustomSearchUrl2;
            textBoxCustomSearchUrl3.Text = Configuration.Settings.VideoControls.CustomSearchUrl3;
            textBoxCustomSearchUrl4.Text = Configuration.Settings.VideoControls.CustomSearchUrl4;
            textBoxCustomSearchUrl5.Text = Configuration.Settings.VideoControls.CustomSearchUrl5;
            textBoxCustomSearchUrl6.Text = Configuration.Settings.VideoControls.CustomSearchUrl6;

            foreach (var x in FontFamily.Families)
            {
                if (x.IsStyleAvailable(FontStyle.Regular) && x.IsStyleAvailable(FontStyle.Bold))
                {
                    comboBoxSubtitleFont.Items.Add(x.Name);
                    if (x.Name.Equals(gs.SubtitleFontName, StringComparison.OrdinalIgnoreCase))
                        comboBoxSubtitleFont.SelectedIndex = comboBoxSubtitleFont.Items.Count - 1;
                }
            }

            var wordListSettings = Configuration.Settings.WordLists;
            checkBoxNamesEtcOnline.Checked = wordListSettings.UseOnlineNamesEtc;
            textBoxNamesEtcOnline.Text = wordListSettings.NamesEtcUrl;

            comboBoxFontName.Items.Clear();
            foreach (var x in FontFamily.Families)
                comboBoxFontName.Items.Add(x.Name);
            var ssa = Configuration.Settings.SubtitleSettings;
            _ssaFontName = ssa.SsaFontName;
            _ssaFontSize = ssa.SsaFontSize;
            _ssaFontColor = ssa.SsaFontColorArgb;
            if (ssa.SsaOutline >= numericUpDownSsaOutline.Minimum && ssa.SsaOutline <= numericUpDownSsaOutline.Maximum)
                numericUpDownSsaOutline.Value = ssa.SsaOutline;
            if (ssa.SsaShadow >= numericUpDownSsaShadow.Minimum && ssa.SsaShadow <= numericUpDownSsaShadow.Maximum)
                numericUpDownSsaShadow.Value = ssa.SsaShadow;
            numericUpDownSsaMarginLeft.Value = ssa.SsaMarginLeft;
            numericUpDownSsaMarginRight.Value = ssa.SsaMarginRight;
            numericUpDownSsaMarginVertical.Value = ssa.SsaMarginTopBottom;
            checkBoxSsaFontBold.Checked = ssa.SsaFontBold;
            checkBoxSsaOpaqueBox.Checked = ssa.SsaOpaqueBox;
            numericUpDownFontSize.Value = (decimal)ssa.SsaFontSize;
            comboBoxFontName.Text = ssa.SsaFontName;
            panelPrimaryColor.BackColor = Color.FromArgb(_ssaFontColor);
            UpdateSsaExample();

            var proxy = Configuration.Settings.Proxy;
            textBoxProxyAddress.Text = proxy.ProxyAddress;
            textBoxProxyUserName.Text = proxy.UserName;
            textBoxProxyPassword.Text = proxy.Password == null ? string.Empty : proxy.DecodePassword();
            textBoxProxyDomain.Text = proxy.Domain;

            textBoxNetworkSessionNewMessageSound.Text = Configuration.Settings.NetworkSettings.NewMessageSound;

            checkBoxSyntaxColorDurationTooSmall.Checked = Configuration.Settings.Tools.ListViewSyntaxColorDurationSmall;
            checkBoxSyntaxColorDurationTooLarge.Checked = Configuration.Settings.Tools.ListViewSyntaxColorDurationBig;
            checkBoxSyntaxColorTextTooLong.Checked = Configuration.Settings.Tools.ListViewSyntaxColorLongLines;
            checkBoxSyntaxColorTextMoreThanTwoLines.Checked = Configuration.Settings.Tools.ListViewSyntaxMoreThanXLines;
            if (Configuration.Settings.Tools.ListViewSyntaxMoreThanXLinesX >= numericUpDownMaxNumberOfLines.Minimum &&
                Configuration.Settings.Tools.ListViewSyntaxMoreThanXLinesX <= numericUpDownMaxNumberOfLines.Maximum)
            {
                numericUpDownMaxNumberOfLines.Value = Configuration.Settings.Tools.ListViewSyntaxMoreThanXLinesX;
            }
            checkBoxSyntaxOverlap.Checked = Configuration.Settings.Tools.ListViewSyntaxColorOverlap;
            panelListViewSyntaxColorError.BackColor = Configuration.Settings.Tools.ListViewSyntaxErrorColor;

            // Language
            var language = Configuration.Settings.Language.Settings;
            Text = language.Title;
            tabPageGeneral.Text = language.General;
            tabPageVideoPlayer.Text = language.VideoPlayer;
            tabPageWaveform.Text = language.WaveformAndSpectrogram;
            tabPageWordLists.Text = language.WordLists;
            tabPageTools.Text = language.Tools;
            tabPageSsaStyle.Text = language.SsaStyle;
            tabPageNetwork.Text = language.Network;
            tabPageToolBar.Text = language.Toolbar;
            tabPageShortcuts.Text = language.Shortcuts;
            tabPageSyntaxColoring.Text = language.SyntaxColoring;
            groupBoxShowToolBarButtons.Text = language.ShowToolBarButtons;
            labelTBNew.Text = language.New;
            labelTBOpen.Text = language.Open;
            labelTBSave.Text = language.Save;
            labelTBSaveAs.Text = language.SaveAs;
            labelTBFind.Text = language.Find;
            labelTBReplace.Text = language.Replace;
            labelTBFixCommonErrors.Text = language.FixCommonerrors;
            labelTBRemoveTextForHi.Text = language.RemoveTextForHi;
            labelTBVisualSync.Text = language.VisualSync;
            labelTBSpellCheck.Text = language.SpellCheck;
            labelTBNetflixQualityCheck.Text = language.NetflixQualityCheck;
            labelTBSettings.Text = language.SettingsName;
            labelTBHelp.Text = language.Help;
            checkBoxToolbarNew.Text = Configuration.Settings.Language.General.Visible;
            checkBoxToolbarOpen.Text = Configuration.Settings.Language.General.Visible;
            checkBoxToolbarSave.Text = Configuration.Settings.Language.General.Visible;
            checkBoxToolbarSaveAs.Text = Configuration.Settings.Language.General.Visible;
            checkBoxToolbarFind.Text = Configuration.Settings.Language.General.Visible;
            checkBoxReplace.Text = Configuration.Settings.Language.General.Visible;
            checkBoxTBFixCommonErrors.Text = Configuration.Settings.Language.General.Visible;
            checkBoxTBRemoveTextForHi.Text = Configuration.Settings.Language.General.Visible;
            checkBoxVisualSync.Text = Configuration.Settings.Language.General.Visible;
            checkBoxSpellCheck.Text = Configuration.Settings.Language.General.Visible;
            checkBoxNetflixQualityCheck.Text = Configuration.Settings.Language.General.Visible;
            checkBoxSettings.Text = Configuration.Settings.Language.General.Visible;
            checkBoxHelp.Text = Configuration.Settings.Language.General.Visible;

            groupBoxMiscellaneous.Text = language.General;
            checkBoxShowFrameRate.Text = language.ShowFrameRate;
            labelDefaultFrameRate.Text = language.DefaultFrameRate;
            labelDefaultFileEncoding.Text = language.DefaultFileEncoding;
            labelAutoDetectAnsiEncoding.Text = language.AutoDetectAnsiEncoding;
            labelSubMaxLen.Text = language.SubtitleLineMaximumLength;
            labelMaxCharsPerSecond.Text = language.MaximumCharactersPerSecond;
            labelMaxWordsPerMin.Text = language.MaximumWordssPerMinute;
            checkBoxAutoWrapWhileTyping.Text = language.AutoWrapWhileTyping;

            labelMinDuration.Text = language.DurationMinimumMilliseconds;
            labelMaxDuration.Text = language.DurationMaximumMilliseconds;
            labelMinGapMs.Text = language.MinimumGapMilliseconds;
            labelMaxLines.Text = language.MaximumLines;
            if (labelSubMaxLen.Left + labelSubMaxLen.Width > numericUpDownSubtitleLineMaximumLength.Left)
                numericUpDownSubtitleLineMaximumLength.Left = labelSubMaxLen.Left + labelSubMaxLen.Width + 3;
            if (labelMaxCharsPerSecond.Left + labelMaxCharsPerSecond.Width > numericUpDownMaxCharsSec.Left)
                numericUpDownMaxCharsSec.Left = labelMaxCharsPerSecond.Left + labelMaxCharsPerSecond.Width + 3;
            if (labelMaxWordsPerMin.Left + labelMaxWordsPerMin.Width > numericUpDownMaxWordsMin.Left)
                numericUpDownMaxWordsMin.Left = labelMaxWordsPerMin.Left + labelMaxWordsPerMin.Width + 3;
            if (labelMinDuration.Left + labelMinDuration.Width > numericUpDownDurationMin.Left)
                numericUpDownDurationMin.Left = labelMinDuration.Left + labelMinDuration.Width + 3;
            if (labelMaxDuration.Left + labelMaxDuration.Width > numericUpDownDurationMax.Left)
                numericUpDownDurationMax.Left = labelMaxDuration.Left + labelMaxDuration.Width + 3;
            if (labelMinGapMs.Left + labelMinGapMs.Width > numericUpDownMinGapMs.Left)
                numericUpDownMinGapMs.Left = labelMinGapMs.Left + labelMinGapMs.Width + 3;
            if (labelMergeShortLines.Left + labelMergeShortLines.Width > comboBoxMergeShortLineLength.Left)
                comboBoxMergeShortLineLength.Left = labelMergeShortLines.Left + labelMergeShortLines.Width + 3;

            labelSubtitleFont.Text = language.SubtitleFont;
            labelSubtitleFontSize.Text = language.SubtitleFontSize;
            checkBoxSubtitleFontBold.Text = language.SubtitleBold;
            checkBoxSubtitleCenter.Text = language.SubtitleCenter;
            checkBoxSubtitleCenter.Left = checkBoxSubtitleFontBold.Left + checkBoxSubtitleFontBold.Width + 4;
            labelSubtitleFontColor.Text = language.SubtitleFontColor;
            labelSubtitleFontBackgroundColor.Text = language.SubtitleBackgroundColor;
            labelSpellChecker.Text = language.SpellChecker;
            comboBoxSpellChecker.Left = labelSpellChecker.Left + labelSpellChecker.Width + 4;
            checkBoxRememberRecentFiles.Text = language.RememberRecentFiles;
            checkBoxReopenLastOpened.Text = language.StartWithLastFileLoaded;
            checkBoxRememberSelectedLine.Text = language.RememberSelectedLine;
            checkBoxStartInSourceView.Text = language.StartInSourceView;
            checkBoxRemoveBlankLinesWhenOpening.Text = language.RemoveBlankLinesWhenOpening;
            checkBoxRememberWindowPosition.Text = language.RememberPositionAndSize;

            labelShowLineBreaksAs.Text = language.ShowLineBreaksAs;
            textBoxShowLineBreaksAs.Left = labelShowLineBreaksAs.Left + labelShowLineBreaksAs.Width;
            labelListViewDoubleClickEvent.Text = language.MainListViewDoubleClickAction;
            labelListviewColumns.Text = language.MainListViewColumns;
            buttonListviewColumns.Text = GetListViewColumns();
            labelAutoBackup.Text = language.AutoBackup;
            labelAutoBackupDeleteAfter.Text = language.AutoBackupDeleteAfter;
            comboBoxAutoBackup.Left = labelAutoBackup.Left + labelAutoBackup.Width + 3;
            labelAutoBackupDeleteAfter.Left = comboBoxAutoBackup.Left + comboBoxAutoBackup.Width + 5;
            comboBoxAutoBackupDeleteAfter.Left = labelAutoBackupDeleteAfter.Left + labelAutoBackupDeleteAfter.Width + 3;
            checkBoxCheckForUpdates.Text = language.CheckForUpdates;
            checkBoxAllowEditOfOriginalSubtitle.Text = language.AllowEditOfOriginalSubtitle;
            checkBoxPromptDeleteLines.Text = language.PromptDeleteLines;

            comboBoxTimeCodeMode.Items.Clear();
            comboBoxTimeCodeMode.Items.Add(language.TimeCodeModeHHMMSSMS);
            comboBoxTimeCodeMode.Items.Add(language.TimeCodeModeHHMMSSFF);
            comboBoxTimeCodeMode.SelectedIndex = gs.UseTimeFormatHHMMSSFF ? 1 : 0;
            labelTimeCodeMode.Text = language.TimeCodeMode;
            comboBoxTimeCodeMode.Left = labelTimeCodeMode.Left + labelTimeCodeMode.Width + 4;

            comboBoxAutoBackup.Items[0] = Configuration.Settings.Language.General.None;
            comboBoxAutoBackup.Items[1] = language.AutoBackupEveryMinute;
            comboBoxAutoBackup.Items[2] = language.AutoBackupEveryFiveMinutes;
            comboBoxAutoBackup.Items[3] = language.AutoBackupEveryFifteenMinutes;

            comboBoxAutoBackupDeleteAfter.Items[0] = language.AutoBackupDeleteAfterOneMonth;
            comboBoxAutoBackupDeleteAfter.Items[1] = language.AutoBackupDeleteAfterThreeMonths;
            comboBoxAutoBackupDeleteAfter.Items[2] = language.AutoBackupDeleteAfterSixMonths;

            groupBoxVideoEngine.Text = language.VideoEngine;
            radioButtonVideoPlayerDirectShow.Text = language.DirectShow;

            labelDirectShowDescription.Text = language.DirectShowDescription;

            radioButtonVideoPlayerMpcHc.Text = language.MpcHc;
            labelMpcHcDescription.Text = language.MpcHcDescription;

            radioButtonVideoPlayerMPV.Text = language.MpvPlayer;
            labelVideoPlayerMPlayer.Text = language.MpvPlayerDescription;
            buttonMpvSettings.Left = labelVideoPlayerMPlayer.Left + labelVideoPlayerMPlayer.Width + 5;
            labelMpvSettings.Left = buttonMpvSettings.Left + buttonMpvSettings.Width + 5;
            checkBoxMpvHandlesPreviewText.Text = language.MpvHandlesPreviewText;

            radioButtonVideoPlayerVLC.Text = language.VlcMediaPlayer;
            labelVideoPlayerVLC.Text = language.VlcMediaPlayerDescription;
            gs.VlcLocation = textBoxVlcPath.Text;

            checkBoxVideoPlayerShowStopButton.Text = language.ShowStopButton;
            checkBoxVideoPlayerShowMuteButton.Text = language.ShowMuteButton;
            checkBoxVideoPlayerShowFullscreenButton.Text = language.ShowFullscreenButton;

            labelVideoPlayerPreviewFontSize.Text = language.PreviewFontSize;
            comboBoxlVideoPlayerPreviewFontSize.Left = labelVideoPlayerPreviewFontSize.Left + labelVideoPlayerPreviewFontSize.Width;
            checkBoxVideoPlayerPreviewFontBold.Text = language.SubtitleBold;
            checkBoxVideoPlayerPreviewFontBold.Left = comboBoxlVideoPlayerPreviewFontSize.Left;

            checkBoxVideoAutoOpen.Text = language.VideoAutoOpen;

            groupBoxMainWindowVideoControls.Text = language.MainWindowVideoControls;
            labelCustomSearch.Text = language.CustomSearchTextAndUrl;

            groupBoxWaveformAppearence.Text = language.WaveformAppearance;
            checkBoxWaveformShowGrid.Text = language.WaveformShowGridLines;
            checkBoxWaveformShowCps.Text = language.WaveformShowCps;
            checkBoxWaveformShowWpm.Text = language.WaveformShowWpm;
            checkBoxReverseMouseWheelScrollDirection.Text = language.ReverseMouseWheelScrollDirection;
            checkBoxAllowOverlap.Text = language.WaveformAllowOverlap;
            checkBoxWaveformSetVideoPosMoveStartEnd.Text = language.WaveformSetVideoPosMoveStartEnd;
            checkBoxWaveformHoverFocus.Text = language.WaveformFocusMouseEnter;
            checkBoxListViewMouseEnterFocus.Text = language.WaveformListViewFocusMouseEnter;
            labelWaveformBorderHitMs1.Text = language.WaveformBorderHitMs1;
            labelWaveformBorderHitMs2.Text = language.WaveformBorderHitMs2;
            numericUpDownWaveformBorderHitMs.Left = labelWaveformBorderHitMs1.Left + labelWaveformBorderHitMs1.Width;
            labelWaveformBorderHitMs2.Left = numericUpDownWaveformBorderHitMs.Left + numericUpDownWaveformBorderHitMs.Width + 2;

            buttonWaveformGridColor.Text = language.WaveformGridColor;
            buttonWaveformColor.Text = language.WaveformColor;
            buttonWaveformSelectedColor.Text = language.WaveformSelectedColor;
            buttonWaveformTextColor.Text = language.WaveformTextColor;
            buttonWaveformBackgroundColor.Text = language.WaveformBackgroundColor;
            groupBoxSpectrogram.Text = language.Spectrogram;
            checkBoxGenerateSpectrogram.Text = language.GenerateSpectrogram;
            labelSpectrogramAppearance.Text = language.SpectrogramAppearance;
            comboBoxSpectrogramAppearance.Items.Clear();
            comboBoxSpectrogramAppearance.Items.Add(language.SpectrogramOneColorGradient);
            comboBoxSpectrogramAppearance.Items.Add(language.SpectrogramClassic);
            labelWaveformTextSize.Text = language.WaveformTextFontSize;
            comboBoxWaveformTextSize.Left = labelWaveformTextSize.Left + labelWaveformTextSize.Width + 5;
            checkBoxWaveformTextBold.Text = language.SubtitleBold;
            checkBoxWaveformTextBold.Left = comboBoxWaveformTextSize.Left + comboBoxWaveformTextSize.Width + 5;

            buttonWaveformsFolderEmpty.Text = language.WaveformAndSpectrogramsFolderEmpty;
            InitializeWaveformsAndSpectrogramsFolderEmpty(language);

            checkBoxUseFFmpeg.Text = language.WaveformUseFFmpeg;
            labelFFmpegPath.Text = language.WaveformFFmpegPath;

            groupBoxSsaStyle.Text = language.SubStationAlphaStyle;

            var ssaStyles = Configuration.Settings.Language.SubStationAlphaStyles;
            labelSsaFontSize.Text = ssaStyles.FontSize;
            labelFontName.Text = ssaStyles.FontName;
            buttonSsaColor.Text = Configuration.Settings.Language.Settings.ChooseColor;
            groupSsaBoxFont.Text = ssaStyles.Font;
            groupBoxSsaBorder.Text = ssaStyles.Border;
            groupBoxMargins.Text = ssaStyles.Margins;
            labelMarginLeft.Text = ssaStyles.MarginLeft;
            labelMarginRight.Text = ssaStyles.MarginRight;
            labelMarginVertical.Text = ssaStyles.MarginVertical;
            labelSsaOutline.Text = language.SsaOutline;
            labelSsaShadow.Text = language.SsaShadow;
            checkBoxSsaOpaqueBox.Text = language.SsaOpaqueBox;
            checkBoxSsaFontBold.Text = Configuration.Settings.Language.General.Bold;

            groupBoxPreview.Text = Configuration.Settings.Language.General.Preview;

            numericUpDownSsaOutline.Left = labelSsaOutline.Left + labelSsaOutline.Width + 4;
            numericUpDownSsaShadow.Left = labelSsaShadow.Left + labelSsaShadow.Width + 4;
            if (Math.Abs(numericUpDownSsaOutline.Left - numericUpDownSsaShadow.Left) < 9)
            {
                if (numericUpDownSsaOutline.Left > numericUpDownSsaShadow.Left)
                    numericUpDownSsaShadow.Left = numericUpDownSsaOutline.Left;
                else
                    numericUpDownSsaOutline.Left = numericUpDownSsaShadow.Left;
            }

            groupBoxWordLists.Text = language.WordLists;
            labelWordListLanguage.Text = language.Language;
            comboBoxWordListLanguage.Left = labelWordListLanguage.Left + labelWordListLanguage.Width + 4;
            groupBoxNamesIgonoreLists.Text = language.NamesIgnoreLists;
            groupBoxUserWordList.Text = language.UserWordList;
            groupBoxOcrFixList.Text = language.OcrFixList;
            buttonRemoveNameEtc.Text = language.Remove;
            buttonRemoveUserWord.Text = language.Remove;
            buttonRemoveOcrFix.Text = language.Remove;
            buttonAddNamesEtc.Text = language.AddNameEtc;
            buttonAddUserWord.Text = language.AddWord;
            buttonAddOcrFix.Text = language.AddPair;
            groupBoxWordListLocation.Text = language.Location;
            checkBoxNamesEtcOnline.Text = language.UseOnlineNamesEtc;
            linkLabelOpenDictionaryFolder.Text = Configuration.Settings.Language.GetDictionaries.OpenDictionariesFolder;

            groupBoxProxySettings.Text = language.ProxyServerSettings;
            labelProxyAddress.Text = language.ProxyAddress;
            groupBoxProxyAuthentication.Text = language.ProxyAuthentication;
            labelProxyUserName.Text = language.ProxyUserName;
            labelProxyPassword.Text = language.ProxyPassword;
            labelProxyDomain.Text = language.ProxyDomain;

            groupBoxNetworkSession.Text = language.NetworkSessionSettings;
            labelNetworkSessionNewMessageSound.Text = language.NetworkSessionNewSound;

            groupBoxToolsVisualSync.Text = language.VisualSync;
            labelVerifyButton.Text = language.PlayXSecondsAndBack;
            labelToolsStartScene.Text = language.StartSceneIndex;
            labelToolsEndScene.Text = language.EndSceneIndex;
            comboBoxToolsStartSceneIndex.Items.Clear();
            comboBoxToolsStartSceneIndex.Items.Add(string.Format(language.FirstPlusX, 0));
            comboBoxToolsStartSceneIndex.Items.Add(string.Format(language.FirstPlusX, 1));
            comboBoxToolsStartSceneIndex.Items.Add(string.Format(language.FirstPlusX, 2));
            comboBoxToolsStartSceneIndex.Items.Add(string.Format(language.FirstPlusX, 3));
            comboBoxToolsEndSceneIndex.Items.Clear();
            comboBoxToolsEndSceneIndex.Items.Add(string.Format(language.LastMinusX, 0));
            comboBoxToolsEndSceneIndex.Items.Add(string.Format(language.LastMinusX, 1));
            comboBoxToolsEndSceneIndex.Items.Add(string.Format(language.LastMinusX, 2));
            comboBoxToolsEndSceneIndex.Items.Add(string.Format(language.LastMinusX, 3));
            int visAdjustTextMax = Math.Max(labelVerifyButton.Width, labelToolsStartScene.Width);
            visAdjustTextMax = Math.Max(visAdjustTextMax, labelToolsEndScene.Width);
            comboBoxToolsVerifySeconds.Left = groupBoxToolsVisualSync.Left + visAdjustTextMax + 12;
            comboBoxToolsStartSceneIndex.Left = comboBoxToolsVerifySeconds.Left;
            comboBoxToolsEndSceneIndex.Left = comboBoxToolsVerifySeconds.Left;

            groupBoxFixCommonErrors.Text = language.FixCommonerrors;
            labelMergeShortLines.Text = language.MergeLinesShorterThan;
            labelToolsMusicSymbol.Text = language.MusicSymbol;
            labelToolsMusicSymbolsToReplace.Text = language.MusicSymbolsToReplace;
            checkBoxFixCommonOcrErrorsUsingHardcodedRules.Text = language.FixCommonOcrErrorsUseHardcodedRules;
            checkBoxFixShortDisplayTimesAllowMoveStartTime.Text = language.FixCommonerrorsFixShortDisplayTimesAllowMoveStartTime;
            checkBoxFceSkipStep1.Text = language.FixCommonErrorsSkipStepOne;
            groupBoxSpellCheck.Text = language.SpellCheck;
            checkBoxSpellCheckAutoChangeNames.Text = Configuration.Settings.Language.SpellCheck.AutoFixNames;
            checkBoxSpellCheckOneLetterWords.Text = Configuration.Settings.Language.SpellCheck.CheckOneLetterWords;
            checkBoxTreatINQuoteAsING.Text = Configuration.Settings.Language.SpellCheck.TreatINQuoteAsING;

            groupBoxToolsMisc.Text = language.Miscellaneous;
            checkBoxUseDoNotBreakAfterList.Text = language.UseDoNotBreakAfterList;
            buttonEditDoNotBreakAfterList.Text = Configuration.Settings.Language.VobSubOcr.Edit;

            comboBoxListViewDoubleClickEvent.Items.Clear();
            comboBoxListViewDoubleClickEvent.Items.Add(language.MainListViewNothing);
            comboBoxListViewDoubleClickEvent.Items.Add(language.MainListViewVideoGoToPositionAndPause);
            comboBoxListViewDoubleClickEvent.Items.Add(language.MainListViewVideoGoToPositionAndPlay);
            comboBoxListViewDoubleClickEvent.Items.Add(language.MainListViewVideoGoToPositionMinusHalfSecAndPause);
            comboBoxListViewDoubleClickEvent.Items.Add(language.MainListViewVideoGoToPositionMinus1SecAndPause);
            comboBoxListViewDoubleClickEvent.Items.Add(language.MainListViewVideoGoToPositionMinus1SecAndPlay);
            comboBoxListViewDoubleClickEvent.Items.Add(language.MainListViewEditTextAndPause);
            comboBoxListViewDoubleClickEvent.Items.Add(language.MainListViewEditText);

            groupBoxBing.Text = language.MicrosoftBingTranslator;
            linkLabelBingSubscribe.Text = language.HowToSignUp;
            labelClientId.Text = language.ClientId;
            labelClientSecret.Text = language.ClientSecret;

            if (gs.ListViewDoubleClickAction >= 0 && gs.ListViewDoubleClickAction < comboBoxListViewDoubleClickEvent.Items.Count)
                comboBoxListViewDoubleClickEvent.SelectedIndex = gs.ListViewDoubleClickAction;

            if (gs.AutoBackupSeconds == 60)
                comboBoxAutoBackup.SelectedIndex = 1;
            else if (gs.AutoBackupSeconds == 60 * 5)
                comboBoxAutoBackup.SelectedIndex = 2;
            else if (gs.AutoBackupSeconds == 60 * 15)
                comboBoxAutoBackup.SelectedIndex = 3;
            else
                comboBoxAutoBackup.SelectedIndex = 0;

            if (gs.AutoBackupDeleteAfterMonths == 3)
                comboBoxAutoBackupDeleteAfter.SelectedIndex = 1;
            else if (gs.AutoBackupDeleteAfterMonths == 1)
                comboBoxAutoBackupDeleteAfter.SelectedIndex = 0;
            else
                comboBoxAutoBackupDeleteAfter.SelectedIndex = 2;

            checkBoxCheckForUpdates.Checked = gs.CheckForUpdates;

            comboBoxSpellChecker.SelectedIndex = gs.SpellChecker.Contains("word", StringComparison.OrdinalIgnoreCase) ? 1 : 0;

            if (Configuration.IsRunningOnLinux() || Configuration.IsRunningOnMac())
            {
                comboBoxSpellChecker.SelectedIndex = 0;
                comboBoxSpellChecker.Enabled = false;
            }

            checkBoxAllowEditOfOriginalSubtitle.Checked = gs.AllowEditOfOriginalSubtitle;
            checkBoxPromptDeleteLines.Checked = gs.PromptDeleteLines;

            ToolsSettings toolsSettings = Configuration.Settings.Tools;
            if (toolsSettings.VerifyPlaySeconds - 2 >= 0 && toolsSettings.VerifyPlaySeconds - 2 < comboBoxToolsVerifySeconds.Items.Count)
                comboBoxToolsVerifySeconds.SelectedIndex = toolsSettings.VerifyPlaySeconds - 2;
            else
                comboBoxToolsVerifySeconds.SelectedIndex = 0;
            if (toolsSettings.StartSceneIndex >= 0 && toolsSettings.StartSceneIndex < comboBoxToolsStartSceneIndex.Items.Count)
                comboBoxToolsStartSceneIndex.SelectedIndex = toolsSettings.StartSceneIndex;
            else
                comboBoxToolsStartSceneIndex.SelectedIndex = 0;
            if (toolsSettings.EndSceneIndex >= 0 && toolsSettings.EndSceneIndex < comboBoxToolsEndSceneIndex.Items.Count)
                comboBoxToolsEndSceneIndex.SelectedIndex = toolsSettings.EndSceneIndex;
            else
                comboBoxToolsEndSceneIndex.SelectedIndex = 0;

            comboBoxMergeShortLineLength.Items.Clear();
            for (int i = 10; i < 100; i++)
                comboBoxMergeShortLineLength.Items.Add(i.ToString(CultureInfo.InvariantCulture));

            if (toolsSettings.MergeLinesShorterThan >= 10 && toolsSettings.MergeLinesShorterThan - 10 < comboBoxMergeShortLineLength.Items.Count)
                comboBoxMergeShortLineLength.SelectedIndex = toolsSettings.MergeLinesShorterThan - 10;
            else
                comboBoxMergeShortLineLength.SelectedIndex = 0;

            // Music notes / music symbols
            if (!Utilities.IsRunningOnMono() && Environment.OSVersion.Version.Major < 6) // 6 == Vista/Win2008Server/Win7
            {
                float fontSize = comboBoxToolsMusicSymbol.Font.Size;
                const string unicodeFontName = Utilities.WinXP2KUnicodeFontName;
                listBoxNamesEtc.Font = new Font(unicodeFontName, fontSize);
                listBoxUserWordLists.Font = new Font(unicodeFontName, fontSize);
                listBoxOcrFixList.Font = new Font(unicodeFontName, fontSize);
                comboBoxToolsMusicSymbol.Font = new Font(unicodeFontName, fontSize);
                textBoxMusicSymbolsToReplace.Font = new Font(unicodeFontName, fontSize);
                textBoxNameEtc.Font = new Font(unicodeFontName, fontSize);
                textBoxUserWord.Font = new Font(unicodeFontName, fontSize);
                textBoxOcrFixKey.Font = new Font(unicodeFontName, fontSize);
                textBoxOcrFixValue.Font = new Font(unicodeFontName, fontSize);
            }

            comboBoxToolsMusicSymbol.Items.Clear();
            comboBoxToolsMusicSymbol.Items.Add("♪");
            comboBoxToolsMusicSymbol.Items.Add("♫");
            comboBoxToolsMusicSymbol.Items.Add("♪♪");
            comboBoxToolsMusicSymbol.Items.Add("*");
            comboBoxToolsMusicSymbol.Items.Add("#");
            if (toolsSettings.MusicSymbol == "♪")
            {
                comboBoxToolsMusicSymbol.SelectedIndex = 0;
            }
            else if (toolsSettings.MusicSymbol == "♫")
            {
                comboBoxToolsMusicSymbol.SelectedIndex = 1;
            }
            else if (toolsSettings.MusicSymbol == "♪♪")
            {
                comboBoxToolsMusicSymbol.SelectedIndex = 2;
            }
            else if (toolsSettings.MusicSymbol == "*")
            {
                comboBoxToolsMusicSymbol.SelectedIndex = 3;
            }
            else if (toolsSettings.MusicSymbol == "#")
            {
                comboBoxToolsMusicSymbol.SelectedIndex = 4;
            }
            else
            {
                comboBoxToolsMusicSymbol.Items.Add(toolsSettings.MusicSymbol);
                comboBoxToolsMusicSymbol.SelectedIndex = 5;
            }

            textBoxMusicSymbolsToReplace.Text = toolsSettings.MusicSymbolToReplace;
            checkBoxFixCommonOcrErrorsUsingHardcodedRules.Checked = toolsSettings.OcrFixUseHardcodedRules;
            checkBoxFixShortDisplayTimesAllowMoveStartTime.Checked = toolsSettings.FixShortDisplayTimesAllowMoveStartTime;
            checkBoxFceSkipStep1.Checked = toolsSettings.FixCommonErrorsSkipStepOne;
            checkBoxSpellCheckAutoChangeNames.Checked = toolsSettings.SpellCheckAutoChangeNames;
            checkBoxSpellCheckOneLetterWords.Checked = toolsSettings.SpellCheckOneLetterWords;
            checkBoxTreatINQuoteAsING.Checked = toolsSettings.SpellCheckEnglishAllowInQuoteAsIng;
            checkBoxUseDoNotBreakAfterList.Checked = toolsSettings.UseNoLineBreakAfter;

            textBoxBingClientId.Text = Configuration.Settings.Tools.MicrosoftBingClientId;
            textBoxBingClientSecret.Text = Configuration.Settings.Tools.MicrosoftBingClientSecret;

            buttonOK.Text = Configuration.Settings.Language.General.Ok;
            buttonCancel.Text = Configuration.Settings.Language.General.Cancel;

            ListWordListLanguages();

            checkBoxWaveformShowGrid.Checked = Configuration.Settings.VideoControls.WaveformDrawGrid;
            checkBoxWaveformShowCps.Checked = Configuration.Settings.VideoControls.WaveformDrawCps;
            checkBoxWaveformShowWpm.Checked = Configuration.Settings.VideoControls.WaveformDrawWpm;
            panelWaveformGridColor.BackColor = Configuration.Settings.VideoControls.WaveformGridColor;
            panelWaveformSelectedColor.BackColor = Configuration.Settings.VideoControls.WaveformSelectedColor;
            panelWaveformColor.BackColor = Configuration.Settings.VideoControls.WaveformColor;
            panelWaveformBackgroundColor.BackColor = Configuration.Settings.VideoControls.WaveformBackgroundColor;
            panelWaveformTextColor.BackColor = Configuration.Settings.VideoControls.WaveformTextColor;
            checkBoxGenerateSpectrogram.Checked = Configuration.Settings.VideoControls.GenerateSpectrogram;
            comboBoxSpectrogramAppearance.SelectedIndex = Configuration.Settings.VideoControls.SpectrogramAppearance == "OneColorGradient" ? 0 : 1;
            comboBoxWaveformTextSize.Text = Configuration.Settings.VideoControls.WaveformTextSize.ToString(CultureInfo.InvariantCulture);
            checkBoxWaveformTextBold.Checked = Configuration.Settings.VideoControls.WaveformTextBold;
            checkBoxReverseMouseWheelScrollDirection.Checked = Configuration.Settings.VideoControls.WaveformMouseWheelScrollUpIsForward;
            checkBoxAllowOverlap.Checked = Configuration.Settings.VideoControls.WaveformAllowOverlap;
            checkBoxWaveformSetVideoPosMoveStartEnd.Checked = Configuration.Settings.VideoControls.WaveformSetVideoPositionOnMoveStartEnd;
            checkBoxWaveformHoverFocus.Checked = Configuration.Settings.VideoControls.WaveformFocusOnMouseEnter;
            checkBoxListViewMouseEnterFocus.Checked = Configuration.Settings.VideoControls.WaveformListViewFocusOnMouseEnter;
            checkBoxListViewMouseEnterFocus.Enabled = Configuration.Settings.VideoControls.WaveformFocusOnMouseEnter;
            if (Configuration.Settings.VideoControls.WaveformBorderHitMs >= numericUpDownWaveformBorderHitMs.Minimum &&
                Configuration.Settings.VideoControls.WaveformBorderHitMs <= numericUpDownWaveformBorderHitMs.Maximum)
                numericUpDownWaveformBorderHitMs.Value = Configuration.Settings.VideoControls.WaveformBorderHitMs;
            checkBoxUseFFmpeg.Checked = gs.UseFFmpegForWaveExtraction;
            textBoxFFmpegPath.Text = gs.FFmpegLocation;
            var generalNode = new TreeNode(Configuration.Settings.Language.General.GeneralText);
            AddNode(generalNode, language.MergeSelectedLines, nameof(Configuration.Settings.Shortcuts.GeneralMergeSelectedLines));
            AddNode(generalNode, language.MergeSelectedLinesOnlyFirstText, nameof(Configuration.Settings.Shortcuts.GeneralMergeSelectedLinesOnlyFirstText));
            AddNode(generalNode, language.MergeOriginalAndTranslation, nameof(Configuration.Settings.Shortcuts.GeneralMergeOriginalAndTranslation));
            AddNode(generalNode, language.MergeWithPrevious, nameof(Configuration.Settings.Shortcuts.GeneralMergeWithPrevious));
            AddNode(generalNode, language.MergeWithNext, nameof(Configuration.Settings.Shortcuts.GeneralMergeWithNext));
            AddNode(generalNode, language.ToggleTranslationMode, nameof(Configuration.Settings.Shortcuts.GeneralToggleTranslationMode));
            AddNode(generalNode, language.SwitchOriginalAndTranslation, nameof(Configuration.Settings.Shortcuts.GeneralSwitchOriginalAndTranslation));
            AddNode(generalNode, language.WaveformPlayFirstSelectedSubtitle, nameof(Configuration.Settings.Shortcuts.GeneralPlayFirstSelected));
            AddNode(generalNode, language.GoToFirstSelectedLine, nameof(Configuration.Settings.Shortcuts.GeneralGoToFirstSelectedLine));
            AddNode(generalNode, language.GoToNextEmptyLine, nameof(Configuration.Settings.Shortcuts.GeneralGoToNextEmptyLine));
            AddNode(generalNode, language.GoToNext, nameof(Configuration.Settings.Shortcuts.GeneralGoToNextSubtitle));
            AddNode(generalNode, language.GoToPrevious, nameof(Configuration.Settings.Shortcuts.GeneralGoToPrevSubtitle));
            AddNode(generalNode, language.GoToCurrentSubtitleStart, nameof(Configuration.Settings.Shortcuts.GeneralGoToStartOfCurrentSubtitle));
            AddNode(generalNode, language.GoToCurrentSubtitleEnd, nameof(Configuration.Settings.Shortcuts.GeneralGoToEndOfCurrentSubtitle));
            AddNode(generalNode, language.GoToPreviousSubtitleAndFocusVideo, nameof(Configuration.Settings.Shortcuts.GeneralGoToPreviousSubtitleAndFocusVideo));
            AddNode(generalNode, language.GoToNextSubtitleAndFocusVideo, nameof(Configuration.Settings.Shortcuts.GeneralGoToNextSubtitleAndFocusVideo));
            AddNode(generalNode, language.Help, nameof(Configuration.Settings.Shortcuts.GeneralHelp));
            treeViewShortcuts.Nodes.Add(generalNode);

            var fileNode = new TreeNode(Configuration.Settings.Language.Main.Menu.File.Title);
            AddNode(fileNode, Configuration.Settings.Language.Main.Menu.File.New, nameof(Configuration.Settings.Shortcuts.MainFileNew), true);
            AddNode(fileNode, Configuration.Settings.Language.Main.Menu.File.Open, nameof(Configuration.Settings.Shortcuts.MainFileOpen), true);
            AddNode(fileNode, Configuration.Settings.Language.Main.Menu.File.OpenKeepVideo, nameof(Configuration.Settings.Shortcuts.MainFileOpenKeepVideo), true);
            AddNode(fileNode, Configuration.Settings.Language.Main.Menu.File.Save, nameof(Configuration.Settings.Shortcuts.MainFileSave), true);
            AddNode(fileNode, Configuration.Settings.Language.Main.Menu.File.SaveAs, nameof(Configuration.Settings.Shortcuts.MainFileSaveAs), true);
            AddNode(fileNode, Configuration.Settings.Language.Main.Menu.File.SaveOriginal, nameof(Configuration.Settings.Shortcuts.MainFileSaveOriginal), true);
            AddNode(fileNode, Configuration.Settings.Language.Main.SaveOriginalSubtitleAs, nameof(Configuration.Settings.Shortcuts.MainFileSaveOriginalAs), true);
            AddNode(fileNode, language.MainFileSaveAll, nameof(Configuration.Settings.Shortcuts.MainFileSaveAll));
            AddNode(fileNode, Configuration.Settings.Language.Main.Menu.File.Export + " -> " + Configuration.Settings.Language.Main.Menu.File.ExportEbu, nameof(Configuration.Settings.Shortcuts.MainFileExportEbu), true);
            treeViewShortcuts.Nodes.Add(fileNode);

            var editNode = new TreeNode(Configuration.Settings.Language.Main.Menu.Edit.Title);
            AddNode(editNode, Configuration.Settings.Language.Main.Menu.Edit.Undo, nameof(Configuration.Settings.Shortcuts.MainEditUndo), true);
            AddNode(editNode, Configuration.Settings.Language.Main.Menu.Edit.Redo, nameof(Configuration.Settings.Shortcuts.MainEditRedo), true);
            AddNode(editNode, Configuration.Settings.Language.Main.Menu.Edit.Find, nameof(Configuration.Settings.Shortcuts.MainEditFind), true);
            AddNode(editNode, Configuration.Settings.Language.Main.Menu.Edit.FindNext, nameof(Configuration.Settings.Shortcuts.MainEditFindNext), true);
            AddNode(editNode, Configuration.Settings.Language.Main.Menu.Edit.Replace, nameof(Configuration.Settings.Shortcuts.MainEditReplace), true);
            AddNode(editNode, Configuration.Settings.Language.Main.Menu.Edit.MultipleReplace, nameof(Configuration.Settings.Shortcuts.MainEditMultipleReplace), true);
            AddNode(editNode, Configuration.Settings.Language.Main.Menu.Edit.GoToSubtitleNumber, nameof(Configuration.Settings.Shortcuts.MainEditGoToLineNumber), true);
            AddNode(editNode, Configuration.Settings.Language.VobSubOcr.RightToLeft, nameof(Configuration.Settings.Shortcuts.MainEditRightToLeft), true);
            AddNode(editNode, language.ReverseStartAndEndingForRTL, nameof(Configuration.Settings.Shortcuts.MainEditReverseStartAndEndingForRTL), true);
            AddNode(editNode, language.ToggleTranslationAndOriginalInPreviews, nameof(Configuration.Settings.Shortcuts.MainEditToggleTranslationOriginalInPreviews), true);
            treeViewShortcuts.Nodes.Add(editNode);

            var toolsNode = new TreeNode(Configuration.Settings.Language.Main.Menu.Tools.Title);
            AddNode(toolsNode, Configuration.Settings.Language.Main.Menu.Tools.FixCommonErrors, nameof(Configuration.Settings.Shortcuts.MainToolsFixCommonErrors), true);
            AddNode(toolsNode, Configuration.Settings.Language.Main.Menu.Tools.StartNumberingFrom, nameof(Configuration.Settings.Shortcuts.MainToolsRenumber), true);
            AddNode(toolsNode, Configuration.Settings.Language.Main.Menu.Tools.RemoveTextForHearingImpaired, nameof(Configuration.Settings.Shortcuts.MainToolsRemoveTextForHI), true);
            AddNode(toolsNode, Configuration.Settings.Language.Main.Menu.Tools.ChangeCasing, nameof(Configuration.Settings.Shortcuts.MainToolsChangeCasing), true);
            AddNode(toolsNode, Configuration.Settings.Language.Main.Menu.Tools.SplitLongLines, nameof(Configuration.Settings.Shortcuts.MainToolsSplitLongLines), true);
            AddNode(toolsNode, Configuration.Settings.Language.Main.Menu.Tools.MergeShortLines, nameof(Configuration.Settings.Shortcuts.MainToolsMergeShortLines), true);
            AddNode(toolsNode, Configuration.Settings.Language.Main.Menu.ContextMenu.AutoDurationCurrentLine, nameof(Configuration.Settings.Shortcuts.MainToolsAutoDuration));
            AddNode(toolsNode, language.ShowBeamer, nameof(Configuration.Settings.Shortcuts.MainToolsBeamer));
            treeViewShortcuts.Nodes.Add(toolsNode);

            var videoNode = new TreeNode(Configuration.Settings.Language.Main.Menu.Video.Title);
            AddNode(videoNode, language.TogglePlayPause, nameof(Configuration.Settings.Shortcuts.MainVideoPlayPauseToggle));
            AddNode(videoNode, language.Pause, nameof(Configuration.Settings.Shortcuts.MainVideoPause));
            AddNode(videoNode, Configuration.Settings.Language.Main.Menu.Video.ShowHideVideo, nameof(Configuration.Settings.Shortcuts.MainVideoShowHideVideo), true);
            AddNode(videoNode, language.ToggleDockUndockOfVideoControls, nameof(Configuration.Settings.Shortcuts.MainVideoToggleVideoControls));
            AddNode(videoNode, language.GoBack1Frame, nameof(Configuration.Settings.Shortcuts.MainVideo1FrameLeft));
            AddNode(videoNode, language.GoForward1Frame, nameof(Configuration.Settings.Shortcuts.MainVideo1FrameRight));
            AddNode(videoNode, language.GoBack100Milliseconds, nameof(Configuration.Settings.Shortcuts.MainVideo100MsLeft));
            AddNode(videoNode, language.GoForward100Milliseconds, nameof(Configuration.Settings.Shortcuts.MainVideo100MsRight));
            AddNode(videoNode, language.GoBack500Milliseconds, nameof(Configuration.Settings.Shortcuts.MainVideo500MsLeft));
            AddNode(videoNode, language.GoForward500Milliseconds, nameof(Configuration.Settings.Shortcuts.MainVideo500MsRight));
            AddNode(videoNode, language.GoBack1Second, nameof(Configuration.Settings.Shortcuts.MainVideo1000MsLeft));
            AddNode(videoNode, language.GoForward1Second, nameof(Configuration.Settings.Shortcuts.MainVideo1000MsRight));
            AddNode(videoNode, language.GoBack5Seconds, nameof(Configuration.Settings.Shortcuts.MainVideo5000MsLeft));
            AddNode(videoNode, language.GoForward5Seconds, nameof(Configuration.Settings.Shortcuts.MainVideo5000MsRight));
            AddNode(videoNode, language.Fullscreen, nameof(Configuration.Settings.Shortcuts.MainVideoFullscreen));
            treeViewShortcuts.Nodes.Add(videoNode);

            var spellCheckNode = new TreeNode(Configuration.Settings.Language.Main.Menu.SpellCheck.Title);
            AddNode(spellCheckNode, Configuration.Settings.Language.Main.Menu.SpellCheck.Title, nameof(Configuration.Settings.Shortcuts.MainSpellCheck));
            AddNode(spellCheckNode, Configuration.Settings.Language.Main.Menu.SpellCheck.FindDoubleWords, nameof(Configuration.Settings.Shortcuts.MainSpellCheckFindDoubleWords));
            AddNode(spellCheckNode, Configuration.Settings.Language.Main.Menu.SpellCheck.AddToNamesEtcList, nameof(Configuration.Settings.Shortcuts.MainSpellCheckAddWordToNames));
            treeViewShortcuts.Nodes.Add(spellCheckNode);

            var syncNode = new TreeNode(Configuration.Settings.Language.Main.Menu.Synchronization.Title);
            AddNode(syncNode, Configuration.Settings.Language.Main.Menu.Synchronization.AdjustAllTimes, nameof(Configuration.Settings.Shortcuts.MainSynchronizationAdjustTimes), true);
            AddNode(syncNode, Configuration.Settings.Language.Main.Menu.Synchronization.VisualSync, nameof(Configuration.Settings.Shortcuts.MainSynchronizationVisualSync), true);
            AddNode(syncNode, Configuration.Settings.Language.Main.Menu.Synchronization.PointSync, nameof(Configuration.Settings.Shortcuts.MainSynchronizationPointSync), true);
            AddNode(syncNode, Configuration.Settings.Language.Main.Menu.Tools.ChangeFrameRate, nameof(Configuration.Settings.Shortcuts.MainSynchronizationChangeFrameRate), true);
            treeViewShortcuts.Nodes.Add(syncNode);

            var listViewNode = new TreeNode(Configuration.Settings.Language.Main.Controls.ListView);
            AddNode(listViewNode, Configuration.Settings.Language.General.Italic, nameof(Configuration.Settings.Shortcuts.MainListViewItalic), true);
            AddNode(listViewNode, Configuration.Settings.Language.Main.Menu.ContextMenu.InsertAfter, nameof(Configuration.Settings.Shortcuts.MainInsertAfter));
            AddNode(listViewNode, Configuration.Settings.Language.Main.Menu.ContextMenu.InsertBefore, nameof(Configuration.Settings.Shortcuts.MainInsertBefore));
            AddNode(listViewNode, language.MergeDialog, nameof(Configuration.Settings.Shortcuts.MainMergeDialog));
            AddNode(listViewNode, language.ToggleFocus, nameof(Configuration.Settings.Shortcuts.MainToggleFocus));
            AddNode(listViewNode, language.ToggleDialogDashes, nameof(Configuration.Settings.Shortcuts.MainListViewToggleDashes));
            AddNode(listViewNode, language.Alignment, nameof(Configuration.Settings.Shortcuts.MainListViewAlignment), true);
            AddNode(listViewNode, language.CopyTextOnly, nameof(Configuration.Settings.Shortcuts.MainListViewCopyText));
            AddNode(listViewNode, language.CopyTextOnlyFromOriginalToCurrent, nameof(Configuration.Settings.Shortcuts.MainListViewCopyTextFromOriginalToCurrent), true);
            AddNode(listViewNode, language.AutoDurationSelectedLines, nameof(Configuration.Settings.Shortcuts.MainListViewAutoDuration));
            AddNode(listViewNode, language.ListViewColumnDelete, nameof(Configuration.Settings.Shortcuts.MainListViewColumnDeleteText), true);
            AddNode(listViewNode, language.ListViewColumnInsert, nameof(Configuration.Settings.Shortcuts.MainListViewColumnInsertText), true);
            AddNode(listViewNode, language.ListViewColumnPaste, nameof(Configuration.Settings.Shortcuts.MainListViewColumnPaste), true);
            AddNode(listViewNode, language.ListViewFocusWaveform, nameof(Configuration.Settings.Shortcuts.MainListViewFocusWaveform));
            AddNode(listViewNode, language.ListViewGoToNextError, nameof(Configuration.Settings.Shortcuts.MainListViewGoToNextError));
            treeViewShortcuts.Nodes.Add(listViewNode);

            var textBoxNode = new TreeNode(language.TextBox);
            AddNode(textBoxNode, Configuration.Settings.Language.General.Italic, nameof(Configuration.Settings.Shortcuts.MainTextBoxItalic));
            AddNode(textBoxNode, Configuration.Settings.Language.Main.Menu.ContextMenu.SplitLineAtCursorPosition, nameof(Configuration.Settings.Shortcuts.MainTextBoxSplitAtCursor));
            AddNode(textBoxNode, language.MainTextBoxMoveLastWordDown, nameof(Configuration.Settings.Shortcuts.MainTextBoxMoveLastWordDown));
            AddNode(textBoxNode, language.MainTextBoxMoveFirstWordFromNextUp, nameof(Configuration.Settings.Shortcuts.MainTextBoxMoveFirstWordFromNextUp));
            AddNode(textBoxNode, language.MainTextBoxSelectionToLower, nameof(Configuration.Settings.Shortcuts.MainTextBoxSelectionToLower));
            AddNode(textBoxNode, language.MainTextBoxSelectionToUpper, nameof(Configuration.Settings.Shortcuts.MainTextBoxSelectionToUpper));
            AddNode(textBoxNode, language.MainTextBoxToggleAutoDuration, nameof(Configuration.Settings.Shortcuts.MainTextBoxToggleAutoDuration));
            AddNode(textBoxNode, Configuration.Settings.Language.Main.Menu.ContextMenu.InsertAfter, nameof(Configuration.Settings.Shortcuts.MainTextBoxInsertAfter));
            AddNode(textBoxNode, language.MainTextBoxAutoBreak, nameof(Configuration.Settings.Shortcuts.MainTextBoxAutoBreak));
            AddNode(textBoxNode, language.MainTextBoxUnbreak, nameof(Configuration.Settings.Shortcuts.MainTextBoxUnbreak));
            treeViewShortcuts.Nodes.Add(textBoxNode);

            var createNode = new TreeNode(Configuration.Settings.Language.Main.VideoControls.Create);
            AddNode(createNode, Configuration.Settings.Language.Main.VideoControls.InsertNewSubtitleAtVideoPosition, nameof(Configuration.Settings.Shortcuts.MainCreateInsertSubAtVideoPos));
            AddNode(createNode, Configuration.Settings.Language.Main.VideoControls.PlayFromJustBeforeText, nameof(Configuration.Settings.Shortcuts.MainCreatePlayFromJustBefore));
            AddNode(createNode, Configuration.Settings.Language.Main.VideoControls.SetStartTime, nameof(Configuration.Settings.Shortcuts.MainCreateSetStart));
            AddNode(createNode, Configuration.Settings.Language.Main.VideoControls.SetEndTime, nameof(Configuration.Settings.Shortcuts.MainCreateSetEnd));
            AddNode(createNode, language.MainCreateStartDownEndUp, nameof(Configuration.Settings.Shortcuts.MainCreateStartDownEndUp));
            AddNode(createNode, language.CreateSetEndAddNewAndGoToNew, nameof(Configuration.Settings.Shortcuts.MainCreateSetEndAddNewAndGoToNew));
            treeViewShortcuts.Nodes.Add(createNode);

            var translateNote = new TreeNode(Configuration.Settings.Language.Main.VideoControls.Translate);
            AddNode(translateNote, language.CustomSearch1, nameof(Configuration.Settings.Shortcuts.MainTranslateCustomSearch1));
            AddNode(translateNote, language.CustomSearch2, nameof(Configuration.Settings.Shortcuts.MainTranslateCustomSearch2));
            AddNode(translateNote, language.CustomSearch3, nameof(Configuration.Settings.Shortcuts.MainTranslateCustomSearch3));
            AddNode(translateNote, language.CustomSearch4, nameof(Configuration.Settings.Shortcuts.MainTranslateCustomSearch4));
            AddNode(translateNote, language.CustomSearch5, nameof(Configuration.Settings.Shortcuts.MainTranslateCustomSearch5));
            AddNode(translateNote, language.CustomSearch6, nameof(Configuration.Settings.Shortcuts.MainTranslateCustomSearch6));
            treeViewShortcuts.Nodes.Add(translateNote);

            var adjustNode = new TreeNode(Configuration.Settings.Language.Main.VideoControls.Adjust);
            AddNode(adjustNode, Configuration.Settings.Language.Main.VideoControls.SetstartTimeAndOffsetOfRest, nameof(Configuration.Settings.Shortcuts.MainAdjustSetStartAndOffsetTheRest));
            AddNode(adjustNode, language.AdjustSetEndTimeAndGoToNext, nameof(Configuration.Settings.Shortcuts.MainAdjustSetEndAndGotoNext));
            AddNode(adjustNode, language.AdjustViaEndAutoStartAndGoToNext, nameof(Configuration.Settings.Shortcuts.MainAdjustViaEndAutoStartAndGoToNext));
            AddNode(adjustNode, language.AdjustSetStartAutoDurationAndGoToNext, nameof(Configuration.Settings.Shortcuts.MainAdjustSetStartAutoDurationAndGoToNext));
            AddNode(adjustNode, language.AdjustSetEndNextStartAndGoToNext, nameof(Configuration.Settings.Shortcuts.MainAdjustSetEndNextStartAndGoToNext));
            AddNode(adjustNode, language.AdjustStartDownEndUpAndGoToNext, nameof(Configuration.Settings.Shortcuts.MainAdjustStartDownEndUpAndGoToNext));
            AddNode(adjustNode, Configuration.Settings.Language.Main.VideoControls.SetStartTime, nameof(Configuration.Settings.Shortcuts.MainAdjustSetStart));
            AddNode(adjustNode, language.AdjustSetStartTimeKeepDuration, nameof(Configuration.Settings.Shortcuts.MainAdjustSetStartKeepDuration));
            AddNode(adjustNode, Configuration.Settings.Language.Main.VideoControls.SetEndTime, nameof(Configuration.Settings.Shortcuts.MainAdjustSetEnd));
            AddNode(adjustNode, language.AdjustSelected100MsForward, nameof(Configuration.Settings.Shortcuts.MainAdjustSelected100MsForward));
            AddNode(adjustNode, language.AdjustSelected100MsBack, nameof(Configuration.Settings.Shortcuts.MainAdjustSelected100MsBack));
            AddNode(adjustNode, language.AdjustSetEndAndOffsetTheRest, nameof(Configuration.Settings.Shortcuts.MainAdjustSetEndAndOffsetTheRest));
            AddNode(adjustNode, language.AdjustSetEndAndOffsetTheRestAndGoToNext, nameof(Configuration.Settings.Shortcuts.MainAdjustSetEndAndOffsetTheRestAndGoToNext));
            AddNode(adjustNode, language.AdjustExtendCurrentSubtitle, nameof(Configuration.Settings.Shortcuts.GeneralExtendCurrentSubtitle));
            AddNode(adjustNode, language.RecalculateDurationOfCurrentSubtitle, nameof(Configuration.Settings.Shortcuts.GeneralAutoCalcCurrentDuration));
            treeViewShortcuts.Nodes.Add(adjustNode);

            var audioVisualizerNode = new TreeNode(language.WaveformAndSpectrogram);
            AddNode(audioVisualizerNode, Configuration.Settings.Language.Waveform.ZoomIn, nameof(Configuration.Settings.Shortcuts.WaveformZoomIn));
            AddNode(audioVisualizerNode, Configuration.Settings.Language.Waveform.ZoomOut, nameof(Configuration.Settings.Shortcuts.WaveformZoomOut));
            AddNode(audioVisualizerNode, language.VerticalZoom, nameof(Configuration.Settings.Shortcuts.WaveformVerticalZoom));
            AddNode(audioVisualizerNode, language.VerticalZoomOut, nameof(Configuration.Settings.Shortcuts.WaveformVerticalZoomOut));
            AddNode(audioVisualizerNode, language.WaveformSeekSilenceForward, nameof(Configuration.Settings.Shortcuts.WaveformSearchSilenceForward));
            AddNode(audioVisualizerNode, language.WaveformSeekSilenceBack, nameof(Configuration.Settings.Shortcuts.WaveformSearchSilenceBack));
            AddNode(audioVisualizerNode, language.WaveformAddTextHere, nameof(Configuration.Settings.Shortcuts.WaveformAddTextHere));
            AddNode(audioVisualizerNode, language.WaveformAddTextHereFromClipboard, nameof(Configuration.Settings.Shortcuts.WaveformAddTextHereFromClipboard));
            AddNode(audioVisualizerNode, language.WaveformPlayNewSelection, nameof(Configuration.Settings.Shortcuts.WaveformPlaySelection));
            AddNode(audioVisualizerNode, language.WaveformPlayNewSelectionEnd, nameof(Configuration.Settings.Shortcuts.WaveformPlaySelectionEnd));
            AddNode(audioVisualizerNode, Configuration.Settings.Language.Main.VideoControls.InsertNewSubtitleAtVideoPosition, nameof(Configuration.Settings.Shortcuts.MainWaveformInsertAtCurrentPosition));
            AddNode(audioVisualizerNode, language.WaveformFocusListView, nameof(Configuration.Settings.Shortcuts.WaveformFocusListView));
            AddNode(audioVisualizerNode, language.WaveformGoToNextSceneChange, nameof(Configuration.Settings.Shortcuts.WaveformGoToNextSceneChange));
            AddNode(audioVisualizerNode, language.WaveformToggleSceneChange, nameof(Configuration.Settings.Shortcuts.WaveformToggleSceneChange));
            treeViewShortcuts.Nodes.Add(audioVisualizerNode);

            foreach (TreeNode node in treeViewShortcuts.Nodes)
            {
                node.Text = node.Text.Replace("&", string.Empty);
                foreach (TreeNode subNode in node.Nodes)
                {
                    subNode.Text = subNode.Text.Replace("&", string.Empty);
                    foreach (TreeNode subSubNode in subNode.Nodes)
                    {
                        subSubNode.Text = subSubNode.Text.Replace("&", string.Empty);
                    }
                }
            }

            treeViewShortcuts.ExpandAll();

            groupBoxShortcuts.Text = language.Shortcuts;
            labelShortcut.Text = language.Shortcut;
            checkBoxShortcutsControl.Text = language.Control;
            checkBoxShortcutsAlt.Text = language.Alt;
            checkBoxShortcutsShift.Text = language.Shift;
            buttonUpdateShortcut.Text = language.UpdateShortcut;
            labelShortcutKey.Text = language.Key;
            comboBoxShortcutKey.Left = labelShortcutKey.Left + labelShortcutKey.Width;
            comboBoxShortcutKey.Items[0] = Configuration.Settings.Language.General.None;

            groupBoxListViewSyntaxColoring.Text = language.ListViewSyntaxColoring;
            checkBoxSyntaxColorDurationTooSmall.Text = language.SyntaxColorDurationIfTooSmall;
            checkBoxSyntaxColorDurationTooLarge.Text = language.SyntaxColorDurationIfTooLarge;
            checkBoxSyntaxColorTextTooLong.Text = language.SyntaxColorTextIfTooLong;
            checkBoxSyntaxColorTextMoreThanTwoLines.Text = string.Format(language.SyntaxColorTextMoreThanMaxLines, Configuration.Settings.Tools.ListViewSyntaxMoreThanXLinesX);
            checkBoxSyntaxOverlap.Text = language.SyntaxColorOverlap;
            buttonListViewSyntaxColorError.Text = language.SyntaxErrorColor;

            UiUtil.FixLargeFonts(this, buttonOK);

            checkBoxShortcutsControl.Left = labelShortcut.Left + labelShortcut.Width + 9;
            checkBoxShortcutsAlt.Left = checkBoxShortcutsControl.Left + checkBoxShortcutsControl.Width + 9;
            checkBoxShortcutsShift.Left = checkBoxShortcutsAlt.Left + checkBoxShortcutsAlt.Width + 9;
            labelShortcutKey.Left = checkBoxShortcutsShift.Left + checkBoxShortcutsShift.Width + 9;
            comboBoxShortcutKey.Left = labelShortcutKey.Left + labelShortcutKey.Width + 2;
            buttonUpdateShortcut.Left = comboBoxShortcutKey.Left + comboBoxShortcutKey.Width + 15;

            _oldVlcLocation = gs.VlcLocation;
            _oldVlcLocationRelative = gs.VlcLocationRelative;

            _oldListViewShowCps = Configuration.Settings.Tools.ListViewShowColumnCharsPerSec;
            _oldListViewShowWpm = Configuration.Settings.Tools.ListViewShowColumnWordsPerMin;

            labelPlatform.Text = (IntPtr.Size * 8) + "-bit";
        }

        private string GetListViewColumns()
        {
            var sb = new StringBuilder();
            sb.Append(Configuration.Settings.Language.General.NumberSymbol + ", ");
            sb.Append(Configuration.Settings.Language.General.StartTime + ", ");
            sb.Append(Configuration.Settings.Language.General.EndTime + ", ");
            sb.Append(Configuration.Settings.Language.General.Duration + ", ");
            if (Configuration.Settings.Tools.ListViewShowColumnCharsPerSec)
                sb.Append(Configuration.Settings.Language.General.CharsPerSec + ", ");
            if (Configuration.Settings.Tools.ListViewShowColumnWordsPerMin)
                sb.Append(Configuration.Settings.Language.General.WordsPerMin + ", ");
            sb.Append(Configuration.Settings.Language.General.Text + ", ");
            return sb.ToString().TrimEnd().TrimEnd(',');
        }

        private void AddNode(TreeNode node, string text, string shortcut, bool isMenuItem = false)
        {
            var prop = Configuration.Settings.Shortcuts.GetType().GetProperty(shortcut);
            node.Nodes.Add(new TreeNode(text + GetShortcutText((string)prop.GetValue(Configuration.Settings.Shortcuts, null))) { Tag = new ShortcutHelper(prop, isMenuItem) });
        }

        private static string GetShortcutText(string shortcut)
        {
            if (string.IsNullOrEmpty(shortcut))
                shortcut = Configuration.Settings.Language.General.None;
            return string.Format(" [{0}]", shortcut);
        }

        private void InitializeWaveformsAndSpectrogramsFolderEmpty(LanguageStructure.Settings language)
        {
            string waveformsFolder = Configuration.WaveformsDirectory.TrimEnd(Path.DirectorySeparatorChar);
            string spectrogramsFolder = Configuration.SpectrogramsDirectory.TrimEnd(Path.DirectorySeparatorChar);
            long bytes = 0;
            int count = 0;

            if (Directory.Exists(waveformsFolder))
            {
                var di = new DirectoryInfo(waveformsFolder);

                // waveform data
                bytes = 0;
                count = 0;
                foreach (FileInfo fi in di.GetFiles("*.wav"))
                {
                    bytes += fi.Length;
                    count++;
                }
            }

            if (Directory.Exists(spectrogramsFolder))
            {
                var di = new DirectoryInfo(spectrogramsFolder);

                // spectrogram data
                foreach (var dir in di.GetDirectories())
                {
                    var spectrogramDir = new DirectoryInfo(dir.FullName);
                    foreach (var fi in spectrogramDir.GetFiles("*.gif"))
                    {
                        bytes += fi.Length;
                        count++;
                    }
                    foreach (var fi in spectrogramDir.GetFiles("*.db"))
                    {
                        bytes += fi.Length;
                        count++;
                    }
                    string xmlFileName = Path.Combine(dir.FullName, "Info.xml");
                    if (File.Exists(xmlFileName))
                    {
                        var fi = new FileInfo(xmlFileName);
                        bytes += fi.Length;
                        count++;
                    }
                }
            }

            if (count > 0)
            {
                buttonWaveformsFolderEmpty.Enabled = true;
                labelWaveformsFolderInfo.Text = string.Format(language.WaveformAndSpectrogramsFolderInfo, count, bytes / 1024.0 / 1024.0);
            }
            else
            {
                buttonWaveformsFolderEmpty.Enabled = false;
                labelWaveformsFolderInfo.Text = string.Format(language.WaveformAndSpectrogramsFolderInfo, 0, 0);
            }
        }

        public void Initialize(Icon icon, Image newFile, Image openFile, Image saveFile, Image saveFileAs, Image find, Image replace, Image fixCommonErrors, Image removeTextForHi,
                               Image visualSync, Image spellCheck, Image NetflixGlyphCheck, Image settings, Image help)
        {
            Icon = (Icon)icon.Clone();
            pictureBoxNew.Image = (Image)newFile.Clone();
            pictureBoxOpen.Image = (Image)openFile.Clone();
            pictureBoxSave.Image = (Image)saveFile.Clone();
            pictureBoxSaveAs.Image = (Image)saveFileAs.Clone();
            pictureBoxFind.Image = (Image)find.Clone();
            pictureBoxReplace.Image = (Image)replace.Clone();
            pictureBoxTBFixCommonErrors.Image = (Image)fixCommonErrors.Clone();
            pictureBoxTBRemoveTextForHi.Image = (Image)removeTextForHi.Clone();
            pictureBoxVisualSync.Image = (Image)visualSync.Clone();
            pictureBoxSpellCheck.Image = (Image)spellCheck.Clone();
            pictureBoxNetflixQualityCheck.Image = (Image)NetflixGlyphCheck.Clone();
            pictureBoxSettings.Image = (Image)settings.Clone();
            pictureBoxHelp.Image = (Image)help.Clone();
        }

        private void ListWordListLanguages()
        {
            //Examples: da_DK_user.xml, eng_OCRFixReplaceList.xml, en_US_names_etc.xml

            string dir = Utilities.DictionaryFolder;

            if (Directory.Exists(dir))
            {
                var cultures = new List<CultureInfo>();
                foreach (var culture in CultureInfo.GetCultures(CultureTypes.SpecificCultures))
                {
                    string name = culture.Name;

                    if (!string.IsNullOrEmpty(name))
                    {
                        if (Directory.GetFiles(dir, name.Replace('-', '_') + "_user.xml").Length == 1)
                        {
                            if (!cultures.Contains(culture))
                                cultures.Add(culture);
                        }

                        if (Directory.GetFiles(dir, name.Replace('-', '_') + "_names_etc.xml").Length == 1)
                        {
                            if (!cultures.Contains(culture))
                                cultures.Add(culture);
                        }
                    }
                }

                foreach (var culture in CultureInfo.GetCultures(CultureTypes.NeutralCultures))
                {
                    if (Directory.GetFiles(dir, culture.ThreeLetterISOLanguageName + "_OCRFixReplaceList.xml").Length == 1 ||
                        Directory.GetFiles(dir, culture.ThreeLetterISOLanguageName + "_OCRFixReplaceList_User.xml").Length == 1)
                    {
                        foreach (var ci in cultures)
                        {
                            if (ci.ThreeLetterISOLanguageName == culture.ThreeLetterISOLanguageName)
                            {
                                cultures.Add(culture);
                                break;
                            }
                        }
                    }
                }

                comboBoxWordListLanguage.Items.Clear();
                if (Configuration.Settings.WordLists.LastLanguage == null)
                    Configuration.Settings.WordLists.LastLanguage = "en-US";
                foreach (var ci in cultures)
                {
                    comboBoxWordListLanguage.Items.Add(new ComboBoxLanguage { CultureInfo = ci });
                    if (ci.Name == Configuration.Settings.WordLists.LastLanguage)
                        comboBoxWordListLanguage.SelectedIndex = comboBoxWordListLanguage.Items.Count - 1;
                }
                if (comboBoxWordListLanguage.Items.Count > 0 && comboBoxWordListLanguage.SelectedIndex == -1)
                    comboBoxWordListLanguage.SelectedIndex = 0;
            }
            else
            {
                groupBoxWordLists.Enabled = false;
            }
        }

        private void ButtonOkClick(object sender, EventArgs e)
        {
            var gs = Configuration.Settings.General;

            gs.ShowToolbarNew = checkBoxToolbarNew.Checked;
            gs.ShowToolbarOpen = checkBoxToolbarOpen.Checked;
            gs.ShowToolbarSave = checkBoxToolbarSave.Checked;
            gs.ShowToolbarSaveAs = checkBoxToolbarSaveAs.Checked;
            gs.ShowToolbarFind = checkBoxToolbarFind.Checked;
            gs.ShowToolbarReplace = checkBoxReplace.Checked;
            gs.ShowToolbarFixCommonErrors = checkBoxTBFixCommonErrors.Checked;
            gs.ShowToolbarRemoveTextForHi = checkBoxTBRemoveTextForHi.Checked;
            gs.ShowToolbarVisualSync = checkBoxVisualSync.Checked;
            gs.ShowToolbarSettings = checkBoxSettings.Checked;
            gs.ShowToolbarSpellCheck = checkBoxSpellCheck.Checked;
            gs.ShowToolbarNetflixGlyphCheck = checkBoxNetflixQualityCheck.Checked;
            gs.ShowToolbarHelp = checkBoxHelp.Checked;

            gs.ShowFrameRate = checkBoxShowFrameRate.Checked;
            double outFrameRate;
            if (double.TryParse(comboBoxFrameRate.Text.Replace(',', '.').Replace(CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator, "."), NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out outFrameRate))
                gs.DefaultFrameRate = outFrameRate;

            gs.DefaultEncoding = UiUtil.GetTextEncodingComboBoxCurrentEncoding(comboBoxEncoding).WebName;

            gs.AutoGuessAnsiEncoding = checkBoxAutoDetectAnsiEncoding.Checked;
            gs.SubtitleFontSize = int.Parse(comboBoxSubtitleFontSize.Text);
            gs.SubtitleFontBold = checkBoxSubtitleFontBold.Checked;
            gs.CenterSubtitleInTextBox = checkBoxSubtitleCenter.Checked;
            gs.SubtitleFontColor = panelSubtitleFontColor.BackColor;
            gs.SubtitleBackgroundColor = panelSubtitleBackgroundColor.BackColor;
            gs.ShowRecentFiles = checkBoxRememberRecentFiles.Checked;
            gs.RememberSelectedLine = checkBoxRememberSelectedLine.Checked;
            gs.StartLoadLastFile = checkBoxReopenLastOpened.Checked;
            gs.StartRememberPositionAndSize = checkBoxRememberWindowPosition.Checked;
            gs.StartInSourceView = checkBoxStartInSourceView.Checked;
            gs.RemoveBlankLinesWhenOpening = checkBoxRemoveBlankLinesWhenOpening.Checked;
            gs.ListViewLineSeparatorString = textBoxShowLineBreaksAs.Text;
            if (string.IsNullOrWhiteSpace(gs.ListViewLineSeparatorString))
                gs.ListViewLineSeparatorString = "<br />";
            gs.ListViewDoubleClickAction = comboBoxListViewDoubleClickEvent.SelectedIndex;

            gs.SubtitleMinimumDisplayMilliseconds = (int)numericUpDownDurationMin.Value;
            gs.SubtitleMaximumDisplayMilliseconds = (int)numericUpDownDurationMax.Value;
            gs.MinimumMillisecondsBetweenLines = (int)numericUpDownMinGapMs.Value;

            if (comboBoxAutoBackup.SelectedIndex == 1)
                gs.AutoBackupSeconds = 60;
            else if (comboBoxAutoBackup.SelectedIndex == 2)
                gs.AutoBackupSeconds = 60 * 5;
            else if (comboBoxAutoBackup.SelectedIndex == 3)
                gs.AutoBackupSeconds = 60 * 15;
            else
                gs.AutoBackupSeconds = 0;

            if (comboBoxAutoBackupDeleteAfter.SelectedIndex == 2)
                gs.AutoBackupDeleteAfterMonths = 6;
            else if (comboBoxAutoBackupDeleteAfter.SelectedIndex == 1)
                gs.AutoBackupDeleteAfterMonths = 3;
            else
                gs.AutoBackupDeleteAfterMonths = 1;

            gs.CheckForUpdates = checkBoxCheckForUpdates.Checked;

            if (comboBoxTimeCodeMode.Visible)
                gs.UseTimeFormatHHMMSSFF = comboBoxTimeCodeMode.SelectedIndex == 1;

            gs.SpellChecker = comboBoxSpellChecker.SelectedIndex == 1 ? "word" : "hunspell";

            gs.AllowEditOfOriginalSubtitle = checkBoxAllowEditOfOriginalSubtitle.Checked;
            gs.PromptDeleteLines = checkBoxPromptDeleteLines.Checked;

            if (radioButtonVideoPlayerMPV.Checked)
                gs.VideoPlayer = "MPV";
            //else if (radioButtonVideoPlayerManagedDirectX.Checked)
            //    gs.VideoPlayer = "ManagedDirectX";
            else if (radioButtonVideoPlayerMpcHc.Checked)
                gs.VideoPlayer = "MPC-HC";
            else if (radioButtonVideoPlayerVLC.Checked)
                gs.VideoPlayer = "VLC";
            else
                gs.VideoPlayer = "DirectShow";
            gs.MpvHandlesPreviewText = checkBoxMpvHandlesPreviewText.Checked;
            gs.VlcLocation = textBoxVlcPath.Text;

            gs.VideoPlayerShowStopButton = checkBoxVideoPlayerShowStopButton.Checked;
            gs.VideoPlayerShowMuteButton = checkBoxVideoPlayerShowMuteButton.Checked;
            gs.VideoPlayerShowFullscreenButton = checkBoxVideoPlayerShowFullscreenButton.Checked;
            gs.VideoPlayerPreviewFontSize = int.Parse(comboBoxlVideoPlayerPreviewFontSize.Items[0].ToString()) + comboBoxlVideoPlayerPreviewFontSize.SelectedIndex;
            gs.VideoPlayerPreviewFontBold = checkBoxVideoPlayerPreviewFontBold.Checked;
            gs.DisableVideoAutoLoading = !checkBoxVideoAutoOpen.Checked;

            Configuration.Settings.VideoControls.CustomSearchText1 = comboBoxCustomSearch1.Text;
            Configuration.Settings.VideoControls.CustomSearchText2 = comboBoxCustomSearch2.Text;
            Configuration.Settings.VideoControls.CustomSearchText3 = comboBoxCustomSearch3.Text;
            Configuration.Settings.VideoControls.CustomSearchText4 = comboBoxCustomSearch4.Text;
            Configuration.Settings.VideoControls.CustomSearchText5 = comboBoxCustomSearch5.Text;
            Configuration.Settings.VideoControls.CustomSearchText6 = comboBoxCustomSearch6.Text;
            Configuration.Settings.VideoControls.CustomSearchUrl1 = textBoxCustomSearchUrl1.Text;
            Configuration.Settings.VideoControls.CustomSearchUrl2 = textBoxCustomSearchUrl2.Text;
            Configuration.Settings.VideoControls.CustomSearchUrl3 = textBoxCustomSearchUrl3.Text;
            Configuration.Settings.VideoControls.CustomSearchUrl4 = textBoxCustomSearchUrl4.Text;
            Configuration.Settings.VideoControls.CustomSearchUrl5 = textBoxCustomSearchUrl5.Text;
            Configuration.Settings.VideoControls.CustomSearchUrl6 = textBoxCustomSearchUrl6.Text;

            int maxLength = (int)numericUpDownSubtitleLineMaximumLength.Value;
            if (maxLength > 9 && maxLength < 1000)
                gs.SubtitleLineMaximumLength = maxLength;
            else if (maxLength > 999)
                gs.SubtitleLineMaximumLength = 999;
            else
                gs.SubtitleLineMaximumLength = 45;

            gs.SubtitleMaximumCharactersPerSeconds = (double)numericUpDownMaxCharsSec.Value;
            gs.SubtitleMaximumWordsPerMinute = (double)numericUpDownMaxWordsMin.Value;

            gs.AutoWrapLineWhileTyping = checkBoxAutoWrapWhileTyping.Checked;

            if (comboBoxSubtitleFont.SelectedItem != null)
                gs.SubtitleFontName = comboBoxSubtitleFont.SelectedItem.ToString();

            var toolsSettings = Configuration.Settings.Tools;
            toolsSettings.VerifyPlaySeconds = comboBoxToolsVerifySeconds.SelectedIndex + 2;
            toolsSettings.StartSceneIndex = comboBoxToolsStartSceneIndex.SelectedIndex;
            toolsSettings.EndSceneIndex = comboBoxToolsEndSceneIndex.SelectedIndex;
            toolsSettings.MergeLinesShorterThan = comboBoxMergeShortLineLength.SelectedIndex + 10;
            if (toolsSettings.MergeLinesShorterThan > gs.SubtitleLineMaximumLength + 1)
                toolsSettings.MergeLinesShorterThan = gs.SubtitleLineMaximumLength;
            toolsSettings.MusicSymbol = comboBoxToolsMusicSymbol.SelectedItem.ToString();
            toolsSettings.MusicSymbolToReplace = textBoxMusicSymbolsToReplace.Text;
            toolsSettings.SpellCheckAutoChangeNames = checkBoxSpellCheckAutoChangeNames.Checked;
            toolsSettings.SpellCheckOneLetterWords = checkBoxSpellCheckOneLetterWords.Checked;
            toolsSettings.SpellCheckEnglishAllowInQuoteAsIng = checkBoxTreatINQuoteAsING.Checked;
            toolsSettings.UseNoLineBreakAfter = checkBoxUseDoNotBreakAfterList.Checked;
            toolsSettings.OcrFixUseHardcodedRules = checkBoxFixCommonOcrErrorsUsingHardcodedRules.Checked;
            toolsSettings.FixShortDisplayTimesAllowMoveStartTime = checkBoxFixShortDisplayTimesAllowMoveStartTime.Checked;
            toolsSettings.FixCommonErrorsSkipStepOne = checkBoxFceSkipStep1.Checked;
            toolsSettings.MicrosoftBingClientId = textBoxBingClientId.Text.Trim();
            toolsSettings.MicrosoftBingClientSecret = textBoxBingClientSecret.Text.Trim();

            var wordListSettings = Configuration.Settings.WordLists;
            wordListSettings.UseOnlineNamesEtc = checkBoxNamesEtcOnline.Checked;
            wordListSettings.NamesEtcUrl = textBoxNamesEtcOnline.Text;
            if (comboBoxWordListLanguage.Items.Count > 0 && comboBoxWordListLanguage.SelectedIndex >= 0)
            {
                var ci = comboBoxWordListLanguage.Items[comboBoxWordListLanguage.SelectedIndex] as ComboBoxLanguage;
                if (ci != null)
                    Configuration.Settings.WordLists.LastLanguage = ci.CultureInfo.Name;
            }

            var ssa = Configuration.Settings.SubtitleSettings;
            ssa.SsaFontName = _ssaFontName;
            ssa.SsaFontSize = _ssaFontSize;
            ssa.SsaFontColorArgb = _ssaFontColor;
            ssa.SsaFontBold = checkBoxSsaFontBold.Checked;
            ssa.SsaOutline = (int)numericUpDownSsaOutline.Value;
            ssa.SsaShadow = (int)numericUpDownSsaShadow.Value;
            ssa.SsaOpaqueBox = checkBoxSsaOpaqueBox.Checked;
            ssa.SsaMarginLeft = (int)numericUpDownSsaMarginLeft.Value;
            ssa.SsaMarginRight = (int)numericUpDownSsaMarginRight.Value;
            ssa.SsaMarginTopBottom = (int)numericUpDownSsaMarginVertical.Value;

            var proxy = Configuration.Settings.Proxy;
            proxy.ProxyAddress = textBoxProxyAddress.Text;
            proxy.UserName = textBoxProxyUserName.Text;
            if (string.IsNullOrWhiteSpace(textBoxProxyPassword.Text))
                proxy.Password = null;
            else
                proxy.EncodePassword(textBoxProxyPassword.Text);
            proxy.Domain = textBoxProxyDomain.Text;

            Configuration.Settings.NetworkSettings.NewMessageSound = textBoxNetworkSessionNewMessageSound.Text;

            Configuration.Settings.Tools.ListViewSyntaxColorDurationSmall = checkBoxSyntaxColorDurationTooSmall.Checked;
            Configuration.Settings.Tools.ListViewSyntaxColorDurationBig = checkBoxSyntaxColorDurationTooLarge.Checked;
            Configuration.Settings.Tools.ListViewSyntaxColorLongLines = checkBoxSyntaxColorTextTooLong.Checked;
            Configuration.Settings.Tools.ListViewSyntaxMoreThanXLines = checkBoxSyntaxColorTextMoreThanTwoLines.Checked;
            Configuration.Settings.Tools.ListViewSyntaxMoreThanXLinesX = (int)numericUpDownMaxNumberOfLines.Value;
            Configuration.Settings.Tools.ListViewSyntaxColorOverlap = checkBoxSyntaxOverlap.Checked;
            Configuration.Settings.Tools.ListViewSyntaxErrorColor = panelListViewSyntaxColorError.BackColor;

            Configuration.Settings.VideoControls.WaveformDrawGrid = checkBoxWaveformShowGrid.Checked;
            Configuration.Settings.VideoControls.WaveformDrawCps = checkBoxWaveformShowCps.Checked;
            Configuration.Settings.VideoControls.WaveformDrawWpm = checkBoxWaveformShowWpm.Checked;
            Configuration.Settings.VideoControls.WaveformGridColor = panelWaveformGridColor.BackColor;
            Configuration.Settings.VideoControls.WaveformSelectedColor = panelWaveformSelectedColor.BackColor;
            Configuration.Settings.VideoControls.WaveformColor = panelWaveformColor.BackColor;
            Configuration.Settings.VideoControls.WaveformBackgroundColor = panelWaveformBackgroundColor.BackColor;
            Configuration.Settings.VideoControls.WaveformTextColor = panelWaveformTextColor.BackColor;
            Configuration.Settings.VideoControls.GenerateSpectrogram = checkBoxGenerateSpectrogram.Checked;
            Configuration.Settings.VideoControls.SpectrogramAppearance = comboBoxSpectrogramAppearance.SelectedIndex == 0 ? "OneColorGradient" : "Classic";

            Configuration.Settings.VideoControls.WaveformTextSize = int.Parse(comboBoxWaveformTextSize.Text);
            Configuration.Settings.VideoControls.WaveformTextBold = checkBoxWaveformTextBold.Checked;
            Configuration.Settings.VideoControls.WaveformMouseWheelScrollUpIsForward = checkBoxReverseMouseWheelScrollDirection.Checked;
            Configuration.Settings.VideoControls.WaveformAllowOverlap = checkBoxAllowOverlap.Checked;
            Configuration.Settings.VideoControls.WaveformSetVideoPositionOnMoveStartEnd = checkBoxWaveformSetVideoPosMoveStartEnd.Checked;
            Configuration.Settings.VideoControls.WaveformFocusOnMouseEnter = checkBoxWaveformHoverFocus.Checked;
            Configuration.Settings.VideoControls.WaveformListViewFocusOnMouseEnter = checkBoxListViewMouseEnterFocus.Checked;
            Configuration.Settings.VideoControls.WaveformBorderHitMs = Convert.ToInt32(numericUpDownWaveformBorderHitMs.Value);
            gs.UseFFmpegForWaveExtraction = checkBoxUseFFmpeg.Checked;
            gs.FFmpegLocation = textBoxFFmpegPath.Text;

            // save shortcuts
            foreach (var kvp in _newShortcuts)
            {
                kvp.Key.Shortcut.SetValue(Configuration.Settings.Shortcuts, kvp.Value, null);
            }

            Configuration.Settings.Save();
        }

        private void FormSettings_KeyDown(object sender, KeyEventArgs e)
        {
            if (comboBoxShortcutKey.Focused)
                return;

            if (e.KeyCode == Keys.Escape)
                DialogResult = DialogResult.Cancel;
            else if (e.KeyCode == UiUtil.HelpKeys)
            {
                Utilities.ShowHelp("#Settings");
                e.SuppressKeyPress = true;
            }
        }

        private void UpdateSsaExample()
        {
            GeneratePreviewReal();
        }

        private void GeneratePreviewReal()
        {
            pictureBoxPreview.Image?.Dispose();
            var bmp = new Bitmap(pictureBoxPreview.Width, pictureBoxPreview.Height);

            using (var g = Graphics.FromImage(bmp))
            {
                // Draw background
                const int rectangleSize = 9;
                for (int y = 0; y < bmp.Height; y += rectangleSize)
                {
                    for (int x = 0; x < bmp.Width; x += rectangleSize)
                    {
                        var c = Color.WhiteSmoke;
                        if (y % (rectangleSize * 2) == 0)
                        {
                            if (x % (rectangleSize * 2) == 0)
                                c = Color.LightGray;
                        }
                        else
                        {
                            if (x % (rectangleSize * 2) != 0)
                                c = Color.LightGray;
                        }
                        g.FillRectangle(new SolidBrush(c), x, y, rectangleSize, rectangleSize);
                    }
                }

                // Draw text
                Font font;
                try
                {
                    font = checkBoxSsaFontBold.Checked ? new Font(_ssaFontName, (float)_ssaFontSize, FontStyle.Bold) : new Font(_ssaFontName, (float)_ssaFontSize);
                }
                catch
                {
                    font = checkBoxSsaFontBold.Checked ? new Font(Font, FontStyle.Bold) : new Font(Font, FontStyle.Regular);
                }
                g.TextRenderingHint = TextRenderingHint.AntiAlias;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                var sf = new StringFormat { Alignment = StringAlignment.Near, LineAlignment = StringAlignment.Near };
                var path = new GraphicsPath();

                bool newLine = false;
                var sb = new StringBuilder();
                sb.Append("This is a test!");

                var measuredWidth = TextDraw.MeasureTextWidth(font, sb.ToString(), checkBoxSsaFontBold.Checked) + 1;
                var measuredHeight = TextDraw.MeasureTextHeight(font, sb.ToString(), checkBoxSsaFontBold.Checked) + 1;

                float left = (float)(bmp.Width - measuredWidth * 0.8 + 15) / 2;

                float top = bmp.Height - measuredHeight - (int)numericUpDownSsaMarginVertical.Value;

                const int leftMargin = 0;
                int pathPointsStart = -1;

                if (checkBoxSsaOpaqueBox.Checked)
                {
                    g.FillRectangle(new SolidBrush(Color.Black), left, top, measuredWidth + 3, measuredHeight + 3);
                }

                TextDraw.DrawText(font, sf, path, sb, false, checkBoxSsaFontBold.Checked, false, left, top, ref newLine, leftMargin, ref pathPointsStart);

                int outline = (int)numericUpDownSsaOutline.Value;

                // draw shadow
                if (numericUpDownSsaShadow.Value > 0 && !checkBoxSsaOpaqueBox.Checked)
                {
                    for (int i = 0; i < (int)numericUpDownSsaShadow.Value; i++)
                    {
                        var shadowPath = new GraphicsPath();
                        sb.Clear();
                        sb.Append("This is a test!");
                        int pathPointsStart2 = -1;
                        TextDraw.DrawText(font, sf, shadowPath, sb, false, checkBoxSsaFontBold.Checked, false, left + i + outline, top + i + outline, ref newLine, leftMargin, ref pathPointsStart2);
                        g.FillPath(new SolidBrush(Color.FromArgb(200, Color.Black)), shadowPath);
                    }
                }

                if (outline > 0 && !checkBoxSsaOpaqueBox.Checked)
                {
                    g.DrawPath(new Pen(Color.Black, outline), path);
                }
                g.FillPath(new SolidBrush(Color.FromArgb(_ssaFontColor)), path);
            }
            pictureBoxPreview.Image = bmp;
        }

        private void ComboBoxWordListLanguageSelectedIndexChanged(object sender, EventArgs e)
        {
            buttonRemoveNameEtc.Enabled = false;
            buttonAddNamesEtc.Enabled = false;
            buttonRemoveUserWord.Enabled = false;
            buttonAddUserWord.Enabled = false;
            buttonRemoveOcrFix.Enabled = false;
            buttonAddOcrFix.Enabled = false;

            listBoxNamesEtc.Items.Clear();
            listBoxUserWordLists.Items.Clear();
            listBoxOcrFixList.Items.Clear();
            var cb = comboBoxWordListLanguage.Items[comboBoxWordListLanguage.SelectedIndex] as ComboBoxLanguage;
            if (cb != null)
            {
                string language = GetCurrentWordListLanguage();

                buttonAddNamesEtc.Enabled = true;
                buttonAddUserWord.Enabled = true;
                buttonAddOcrFix.Enabled = true;

                // user word list
                LoadUserWords(language, true);

                // OCR fix words
                LoadOcrFixList(true);

                LoadNamesEtc(language, true);
            }
        }

        private void LoadOcrFixList(bool reloadListBox)
        {
            var cb = comboBoxWordListLanguage.Items[comboBoxWordListLanguage.SelectedIndex] as ComboBoxLanguage;
            if (cb == null)
                return;
            _ocrFixReplaceList = OcrFixReplaceList.FromLanguageId(cb.CultureInfo.ThreeLetterISOLanguageName);
            if (reloadListBox)
            {
                listBoxOcrFixList.BeginUpdate();
                listBoxOcrFixList.Items.Clear();
                foreach (var pair in _ocrFixReplaceList.WordReplaceList)
                {
                    listBoxOcrFixList.Items.Add(pair.Key + " --> " + pair.Value);
                }
                foreach (var pair in _ocrFixReplaceList.PartialLineWordBoundaryReplaceList)
                {
                    listBoxOcrFixList.Items.Add(pair.Key + " --> " + pair.Value);
                }
                listBoxOcrFixList.Sorted = true;
                listBoxOcrFixList.EndUpdate();
            }
        }

        private void LoadUserWords(string language, bool reloadListBox)
        {
            _userWordList = new List<string>();
            Utilities.LoadUserWordList(_userWordList, language);
            _userWordList.Sort();

            if (reloadListBox)
            {
                listBoxUserWordLists.Items.Clear();
                listBoxUserWordLists.BeginUpdate();
                foreach (string name in _userWordList)
                    listBoxUserWordLists.Items.Add(name);
                listBoxUserWordLists.EndUpdate();
            }
        }

        private void LoadNamesEtc(string language, bool reloadListBox)
        {
            var task = Task.Factory.StartNew(() =>
            {
                // names etc
                var namesList = new NamesList(Configuration.DictionariesDirectory, language, Configuration.Settings.WordLists.UseOnlineNamesEtc, Configuration.Settings.WordLists.NamesEtcUrl);
                _wordListNamesEtc = namesList.GetAllNames();
                _wordListNamesEtc.Sort();
                return _wordListNamesEtc;
            });

            if (reloadListBox)
            {
                // reload the listbox on a continuation ui thead
                var uiContext = TaskScheduler.FromCurrentSynchronizationContext();
                task.ContinueWith(originalTask =>
                {
                    listBoxNamesEtc.BeginUpdate();
                    listBoxNamesEtc.Items.Clear();
                    foreach (var name in originalTask.Result)
                    {
                        listBoxNamesEtc.Items.Add(name);
                    }
                    listBoxNamesEtc.EndUpdate();
                }, uiContext);
            }
        }

        private string GetCurrentWordListLanguage()
        {
            var idx = comboBoxWordListLanguage.SelectedIndex;
            if (idx < 0)
                return null;

            var cb = comboBoxWordListLanguage.Items[idx] as ComboBoxLanguage;
            return cb?.CultureInfo.Name.Replace('-', '_');
        }

        private void ButtonAddNamesEtcClick(object sender, EventArgs e)
        {
            var sidx = comboBoxWordListLanguage.SelectedIndex;
            if (sidx < 0)
                return;

            string language = GetCurrentWordListLanguage();
            string text = textBoxNameEtc.Text.RemoveControlCharacters().Trim();
            if (!string.IsNullOrEmpty(language) && text.Length > 1 && !_wordListNamesEtc.Contains(text))
            {
                var namesList = new NamesList(Configuration.DictionariesDirectory, language, Configuration.Settings.WordLists.UseOnlineNamesEtc, Configuration.Settings.WordLists.NamesEtcUrl);
                namesList.Add(text);
                LoadNamesEtc(language, true);
                labelStatus.Text = string.Format(Configuration.Settings.Language.Settings.WordAddedX, text);
                textBoxNameEtc.Text = string.Empty;
                textBoxNameEtc.Focus();
                for (int i = 0; i < listBoxNamesEtc.Items.Count; i++)
                {
                    if (listBoxNamesEtc.Items[i].ToString() == text)
                    {
                        listBoxNamesEtc.SelectedIndex = i;
                        int top = i - 5;
                        if (top < 0)
                            top = 0;
                        listBoxNamesEtc.TopIndex = top;
                        break;
                    }
                }
            }
            else
            {
                MessageBox.Show(Configuration.Settings.Language.Settings.WordAlreadyExists);
            }
        }

        private void ListBoxNamesEtcSelectedIndexChanged(object sender, EventArgs e)
        {
            buttonRemoveNameEtc.Enabled = listBoxNamesEtc.SelectedIndex >= 0;
        }

        private void ButtonRemoveNameEtcClick(object sender, EventArgs e)
        {
            if (listBoxNamesEtc.SelectedIndices.Count == 0)
                return;

            string language = GetCurrentWordListLanguage();
            int index = listBoxNamesEtc.SelectedIndex;
            string text = listBoxNamesEtc.Items[index].ToString();
            int itemsToRemoveCount = listBoxNamesEtc.SelectedIndices.Count;
            if (!string.IsNullOrEmpty(language) && index >= 0)
            {
                DialogResult result;
                if (itemsToRemoveCount == 1)
                    result = MessageBox.Show(string.Format(Configuration.Settings.Language.Settings.RemoveX, text), "Subtitle Edit", MessageBoxButtons.YesNo);
                else
                    result = MessageBox.Show(string.Format(Configuration.Settings.Language.Main.DeleteXLinesPrompt, itemsToRemoveCount), "Subtitle Edit", MessageBoxButtons.YesNo);
                if (result == DialogResult.Yes)
                {
                    int removeCount = 0;
                    var namesList = new NamesList(Configuration.DictionariesDirectory, language, Configuration.Settings.WordLists.UseOnlineNamesEtc, Configuration.Settings.WordLists.NamesEtcUrl);
                    for (int idx = listBoxNamesEtc.SelectedIndices.Count - 1; idx >= 0; idx--)
                    {
                        index = listBoxNamesEtc.SelectedIndices[idx];
                        text = listBoxNamesEtc.Items[index].ToString();
                        namesList.Remove(text);
                        removeCount++;
                        listBoxNamesEtc.Items.RemoveAt(index);
                    }

                    if (removeCount > 0)
                    {
                        LoadNamesEtc(language, true); // reload

                        if (index < listBoxNamesEtc.Items.Count)
                            listBoxNamesEtc.SelectedIndex = index;
                        else if (listBoxNamesEtc.Items.Count > 0)
                            listBoxNamesEtc.SelectedIndex = index - 1;
                        listBoxNamesEtc.Focus();

                        buttonRemoveNameEtc.Enabled = false;
                        return;
                    }

                    if (removeCount < itemsToRemoveCount && Configuration.Settings.WordLists.UseOnlineNamesEtc && !string.IsNullOrEmpty(Configuration.Settings.WordLists.NamesEtcUrl))
                    {
                        MessageBox.Show(Configuration.Settings.Language.Settings.CannotUpdateNamesEtcOnline);
                        return;
                    }

                    if (removeCount == 0)
                        MessageBox.Show(Configuration.Settings.Language.Settings.WordNotFound);
                }
            }
        }

        private void TextBoxNameEtcKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                ButtonAddNamesEtcClick(null, null);
            }
        }

        private void ButtonAddUserWordClick(object sender, EventArgs e)
        {
            var sidx = comboBoxWordListLanguage.SelectedIndex;
            if (sidx < 0)
                return;

            string language = GetCurrentWordListLanguage();
            string text = textBoxUserWord.Text.RemoveControlCharacters().Trim().ToLower();
            if (!string.IsNullOrEmpty(language) && text.Length > 0 && !_userWordList.Contains(text))
            {
                Utilities.AddToUserDictionary(text, language);
                LoadUserWords(language, true);
                labelStatus.Text = string.Format(Configuration.Settings.Language.Settings.WordAddedX, text);
                textBoxUserWord.Text = string.Empty;
                textBoxUserWord.Focus();

                for (int i = 0; i < listBoxUserWordLists.Items.Count; i++)
                {
                    if (listBoxUserWordLists.Items[i].ToString() == text)
                    {
                        listBoxUserWordLists.SelectedIndex = i;
                        int top = i - 5;
                        if (top < 0)
                            top = 0;
                        listBoxUserWordLists.TopIndex = top;
                        break;
                    }
                }
            }
            else
            {
                MessageBox.Show(Configuration.Settings.Language.Settings.WordAlreadyExists);
            }
        }

        private void TextBoxUserWordKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                ButtonAddUserWordClick(null, null);
            }
        }

        private void ButtonRemoveUserWordClick(object sender, EventArgs e)
        {
            if (listBoxUserWordLists.SelectedIndices.Count == 0)
                return;

            string language = GetCurrentWordListLanguage();
            int index = listBoxUserWordLists.SelectedIndex;
            int itemsToRemoveCount = listBoxUserWordLists.SelectedIndices.Count;
            string text = listBoxUserWordLists.Items[index].ToString();
            if (!string.IsNullOrEmpty(language) && index >= 0)
            {
                DialogResult result;
                if (itemsToRemoveCount == 1)
                    result = MessageBox.Show(string.Format(Configuration.Settings.Language.Settings.RemoveX, text), "Subtitle Edit", MessageBoxButtons.YesNo);
                else
                    result = MessageBox.Show(string.Format(Configuration.Settings.Language.Main.DeleteXLinesPrompt, itemsToRemoveCount), "Subtitle Edit", MessageBoxButtons.YesNo);
                if (result == DialogResult.Yes)
                {
                    int removeCount = 0;
                    var words = new List<string>();
                    string userWordFileName = Utilities.LoadUserWordList(words, language);

                    for (int idx = listBoxUserWordLists.SelectedIndices.Count - 1; idx >= 0; idx--)
                    {
                        index = listBoxUserWordLists.SelectedIndices[idx];
                        text = listBoxUserWordLists.Items[index].ToString();

                        if (words.Contains(text))
                        {
                            words.Remove(text);
                            removeCount++;
                        }
                        listBoxUserWordLists.Items.RemoveAt(index);
                    }

                    if (removeCount > 0)
                    {
                        words.Sort();
                        var doc = new XmlDocument();
                        doc.Load(userWordFileName);
                        doc.DocumentElement.RemoveAll();
                        foreach (string word in words)
                        {
                            XmlNode node = doc.CreateElement("word");
                            node.InnerText = word;
                            doc.DocumentElement.AppendChild(node);
                        }
                        doc.Save(userWordFileName);
                        LoadUserWords(language, false); // reload
                        buttonRemoveUserWord.Enabled = false;

                        if (index < listBoxUserWordLists.Items.Count)
                            listBoxUserWordLists.SelectedIndex = index;
                        else if (listBoxUserWordLists.Items.Count > 0)
                            listBoxUserWordLists.SelectedIndex = index - 1;
                        listBoxUserWordLists.Focus();
                        return;
                    }

                    if (removeCount < itemsToRemoveCount)
                        MessageBox.Show(Configuration.Settings.Language.Settings.WordNotFound);
                }
            }
        }

        private void ListBoxUserWordListsSelectedIndexChanged(object sender, EventArgs e)
        {
            buttonRemoveUserWord.Enabled = listBoxUserWordLists.SelectedIndex >= 0;
        }

        private void ButtonAddOcrFixClick(object sender, EventArgs e)
        {
            string key = textBoxOcrFixKey.Text.RemoveControlCharacters().Trim();
            string value = textBoxOcrFixValue.Text.RemoveControlCharacters().Trim();
            if (key.Length == 0 || value.Length == 0 || key == value || Utilities.IsInteger(key))
                return;

            var cb = comboBoxWordListLanguage.Items[comboBoxWordListLanguage.SelectedIndex] as ComboBoxLanguage;
            if (cb == null)
                return;

            var added = _ocrFixReplaceList.AddWordOrPartial(key, value);
            if (!added)
            {
                MessageBox.Show(Configuration.Settings.Language.Settings.WordAlreadyExists);
                return;
            }

            LoadOcrFixList(true);
            textBoxOcrFixKey.Text = string.Empty;
            textBoxOcrFixValue.Text = string.Empty;
            textBoxOcrFixKey.Focus();

            for (int i = 0; i < listBoxOcrFixList.Items.Count; i++)
            {
                if (listBoxOcrFixList.Items[i].ToString() == key + " --> " + value)
                {
                    listBoxOcrFixList.SelectedIndex = i;
                    int top = i - 5;
                    if (top < 0)
                        top = 0;
                    listBoxOcrFixList.TopIndex = top;
                    break;
                }
            }
        }

        private void ListBoxOcrFixListSelectedIndexChanged(object sender, EventArgs e)
        {
            buttonRemoveOcrFix.Enabled = listBoxOcrFixList.SelectedIndex >= 0;
        }

        private void ButtonRemoveOcrFixClick(object sender, EventArgs e)
        {
            var sidx = comboBoxWordListLanguage.SelectedIndex;
            if (sidx < 0)
                return;
            var cb = comboBoxWordListLanguage.Items[sidx] as ComboBoxLanguage;
            if (cb == null)
                return;

            if (listBoxOcrFixList.SelectedIndices.Count == 0)
                return;

            int itemsToRemoveCount = listBoxOcrFixList.SelectedIndices.Count;

            int index = listBoxOcrFixList.SelectedIndex;
            string text = listBoxOcrFixList.Items[index].ToString();
            string key = text.Substring(0, text.IndexOf(" --> ", StringComparison.Ordinal));

            if (_ocrFixReplaceList.WordReplaceList.ContainsKey(key) || _ocrFixReplaceList.PartialLineWordBoundaryReplaceList.ContainsKey(key))
            {
                DialogResult result;
                if (itemsToRemoveCount == 1)
                    result = MessageBox.Show(string.Format(Configuration.Settings.Language.Settings.RemoveX, text), "Subtitle Edit", MessageBoxButtons.YesNo);
                else
                    result = MessageBox.Show(string.Format(Configuration.Settings.Language.Main.DeleteXLinesPrompt, itemsToRemoveCount), "Subtitle Edit", MessageBoxButtons.YesNo);
                if (result == DialogResult.Yes)
                {
                    for (int idx = listBoxOcrFixList.SelectedIndices.Count - 1; idx >= 0; idx--)
                    {
                        index = listBoxOcrFixList.SelectedIndices[idx];
                        text = listBoxOcrFixList.Items[index].ToString();
                        key = text.Substring(0, text.IndexOf(" --> ", StringComparison.Ordinal)).Trim();

                        if (_ocrFixReplaceList.WordReplaceList.ContainsKey(key) || _ocrFixReplaceList.PartialLineWordBoundaryReplaceList.ContainsKey(key))
                        {
                            _ocrFixReplaceList.RemoveWordOrPartial(key);
                        }
                        listBoxOcrFixList.Items.RemoveAt(index);
                    }

                    LoadOcrFixList(false);
                    buttonRemoveOcrFix.Enabled = false;

                    if (index < listBoxOcrFixList.Items.Count)
                        listBoxOcrFixList.SelectedIndex = index;
                    else if (listBoxOcrFixList.Items.Count > 0)
                        listBoxOcrFixList.SelectedIndex = index - 1;
                    listBoxOcrFixList.Focus();
                }
            }
        }

        private void TextBoxOcrFixValueKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                ButtonAddOcrFixClick(null, null);
            }
        }

        private void TabControlSettingsSelectedIndexChanged(object sender, EventArgs e)
        {
            labelStatus.Text = string.Empty;
        }

        private void ListBoxKeyDownSearch(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape ||
                e.KeyCode == Keys.Tab ||
                e.KeyCode == Keys.Return ||
                e.KeyCode == Keys.Enter ||
                e.KeyCode == Keys.Down ||
                e.KeyCode == Keys.Up ||
                e.KeyCode == Keys.PageDown ||
                e.KeyCode == Keys.PageUp ||
                e.KeyCode == Keys.None ||
                e.KeyCode == UiUtil.HelpKeys ||
                e.KeyCode == Keys.Home ||
                e.KeyCode == Keys.End)
                return;

            if (TimeSpan.FromTicks(_listBoxSearchStringLastUsed.Ticks).TotalMilliseconds + 1800 <
                TimeSpan.FromTicks(DateTime.Now.Ticks).TotalMilliseconds)
                _listBoxSearchString = string.Empty;

            if (e.KeyCode == Keys.Delete)
            {
                if (_listBoxSearchString.Length > 0)
                    _listBoxSearchString = _listBoxSearchString.Remove(_listBoxSearchString.Length - 1, 1);
            }
            else
            {
                _listBoxSearchString += e.KeyCode.ToString();
            }

            _listBoxSearchStringLastUsed = DateTime.Now;
            FindAndSelectListBoxItem(sender as ListBox);
            e.SuppressKeyPress = true;
        }

        private void FindAndSelectListBoxItem(ListBox listBox)
        {
            int i = 0;
            foreach (string s in listBox.Items)
            {
                if (s.StartsWith(_listBoxSearchString, StringComparison.OrdinalIgnoreCase))
                {
                    listBox.SelectedIndex = i;
                    break;
                }
                i++;
            }
        }

        private void ListBoxSearchReset(object sender, EventArgs e)
        {
            _listBoxSearchString = string.Empty;
        }

        private void comboBoxCustomSearch_SelectedIndexChanged(object sender, EventArgs e)
        {
            var cb = (ComboBox)sender;
            var tb = textBoxCustomSearchUrl1;
            if (cb == comboBoxCustomSearch2)
                tb = textBoxCustomSearchUrl2;
            else if (cb == comboBoxCustomSearch3)
                tb = textBoxCustomSearchUrl3;
            else if (cb == comboBoxCustomSearch4)
                tb = textBoxCustomSearchUrl4;
            else if (cb == comboBoxCustomSearch5)
                tb = textBoxCustomSearchUrl5;
            else if (cb == comboBoxCustomSearch6)
                tb = textBoxCustomSearchUrl6;

            if (cb.SelectedIndex >= 0)
            {
                if (cb.SelectedIndex == 0)
                    tb.Text = "http://dictionary.reference.com/browse/{0}";
                else if (cb.SelectedIndex == 1)
                    tb.Text = "http://www.learnersdictionary.com/search/{0}";
                else if (cb.SelectedIndex == 2)
                    tb.Text = "http://www.merriam-webster.com/dictionary/{0}";
                else if (cb.SelectedIndex == 3)
                    tb.Text = "http://www.thefreedictionary.com/{0}";
                else if (cb.SelectedIndex == 4)
                    tb.Text = "http://thesaurus.com/browse/{0}";
                else if (cb.SelectedIndex == 5)
                    tb.Text = "http://www.urbandictionary.com/define.php?term={0}";
                else if (cb.SelectedIndex == 6)
                    tb.Text = "http://www.visuwords.com/?word={0}";
                else if (cb.SelectedIndex == 7)
                    tb.Text = "http://en.m.wikipedia.org/wiki?search={0}";
            }
        }

        private void buttonWaveformSelectedColor_Click(object sender, EventArgs e)
        {
            colorDialogSSAStyle.Color = panelWaveformSelectedColor.BackColor;
            if (colorDialogSSAStyle.ShowDialog() == DialogResult.OK)
            {
                panelWaveformSelectedColor.BackColor = colorDialogSSAStyle.Color;
            }
        }

        private void buttonWaveformColor_Click(object sender, EventArgs e)
        {
            colorDialogSSAStyle.Color = panelWaveformColor.BackColor;
            if (colorDialogSSAStyle.ShowDialog() == DialogResult.OK)
            {
                panelWaveformColor.BackColor = colorDialogSSAStyle.Color;
            }
        }

        private void buttonWaveformBackgroundColor_Click(object sender, EventArgs e)
        {
            colorDialogSSAStyle.Color = panelWaveformBackgroundColor.BackColor;
            if (colorDialogSSAStyle.ShowDialog() == DialogResult.OK)
            {
                panelWaveformBackgroundColor.BackColor = colorDialogSSAStyle.Color;
            }
        }

        private void buttonWaveformGridColor_Click(object sender, EventArgs e)
        {
            colorDialogSSAStyle.Color = panelWaveformGridColor.BackColor;
            if (colorDialogSSAStyle.ShowDialog() == DialogResult.OK)
            {
                panelWaveformGridColor.BackColor = colorDialogSSAStyle.Color;
            }
        }

        private void buttonWaveformTextColor_Click(object sender, EventArgs e)
        {
            colorDialogSSAStyle.Color = panelWaveformTextColor.BackColor;
            if (colorDialogSSAStyle.ShowDialog() == DialogResult.OK)
            {
                panelWaveformTextColor.BackColor = colorDialogSSAStyle.Color;
            }
        }

        private void buttonWaveformsFolderEmpty_Click(object sender, EventArgs e)
        {
            string waveformsFolder = Configuration.WaveformsDirectory.TrimEnd(Path.DirectorySeparatorChar);
            if (Directory.Exists(waveformsFolder))
            {
                var di = new DirectoryInfo(waveformsFolder);

                foreach (var fileName in di.GetFiles("*.wav"))
                {
                    try
                    {
                        File.Delete(fileName.FullName);
                    }
                    catch (Exception exception)
                    {
                        MessageBox.Show(exception.Message);
                    }
                }
            }

            string spectrogramsFolder = Configuration.SpectrogramsDirectory.TrimEnd(Path.DirectorySeparatorChar);
            if (Directory.Exists(spectrogramsFolder))
            {
                var di = new DirectoryInfo(spectrogramsFolder);

                foreach (var dir in di.GetDirectories())
                {
                    var spectrogramDir = new DirectoryInfo(dir.FullName);
                    foreach (var fileName in spectrogramDir.GetFiles("*.gif"))
                    {
                        File.Delete(fileName.FullName);
                    }
                    string imageDbFileName = Path.Combine(dir.FullName, "Images.db");
                    if (File.Exists(imageDbFileName))
                        File.Delete(imageDbFileName);
                    string xmlFileName = Path.Combine(dir.FullName, "Info.xml");
                    if (File.Exists(xmlFileName))
                        File.Delete(xmlFileName);
                    Directory.Delete(dir.FullName);
                }
            }

            InitializeWaveformsAndSpectrogramsFolderEmpty(Configuration.Settings.Language.Settings);
        }

        private void checkBoxRememberRecentFiles_CheckedChanged(object sender, EventArgs e)
        {
            checkBoxReopenLastOpened.Enabled = checkBoxRememberRecentFiles.Checked;
            checkBoxRememberSelectedLine.Enabled = checkBoxRememberRecentFiles.Checked;
        }

        private void buttonWaveformSelectedColor_Click(object sender, MouseEventArgs e)
        {
            colorDialogSSAStyle.Color = panelWaveformSelectedColor.BackColor;
            if (colorDialogSSAStyle.ShowDialog() == DialogResult.OK)
            {
                panelWaveformSelectedColor.BackColor = colorDialogSSAStyle.Color;
            }
        }

        private void panelSubtitleFontColor_Click(object sender, EventArgs e)
        {
            colorDialogSSAStyle.Color = panelSubtitleFontColor.BackColor;
            if (colorDialogSSAStyle.ShowDialog() == DialogResult.OK)
            {
                panelSubtitleFontColor.BackColor = colorDialogSSAStyle.Color;
            }
        }

        private void panelSubtitleBackgroundColor_Click(object sender, EventArgs e)
        {
            colorDialogSSAStyle.Color = panelSubtitleBackgroundColor.BackColor;
            if (colorDialogSSAStyle.ShowDialog() == DialogResult.OK)
            {
                panelSubtitleBackgroundColor.BackColor = colorDialogSSAStyle.Color;
            }
        }

        private void treeViewShortcuts_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (e.Node == null || e.Node.Nodes.Count > 0)
            {
                checkBoxShortcutsControl.Checked = false;
                checkBoxShortcutsControl.Enabled = false;
                checkBoxShortcutsAlt.Checked = false;
                checkBoxShortcutsAlt.Enabled = false;
                checkBoxShortcutsShift.Checked = false;
                checkBoxShortcutsShift.Enabled = false;
                comboBoxShortcutKey.SelectedIndex = 0;
                comboBoxShortcutKey.Enabled = false;
                buttonUpdateShortcut.Enabled = false;
            }
            else if (e.Node != null || e.Node.Nodes.Count == 0)
            {
                checkBoxShortcutsControl.Enabled = true;
                checkBoxShortcutsAlt.Enabled = true;
                checkBoxShortcutsShift.Enabled = true;

                checkBoxShortcutsControl.Checked = false;
                checkBoxShortcutsAlt.Checked = false;
                checkBoxShortcutsShift.Checked = false;

                comboBoxShortcutKey.SelectedIndex = 0;

                comboBoxShortcutKey.Enabled = true;
                buttonUpdateShortcut.Enabled = true;

                string shortcut = GetShortcut(e.Node.Text);

                string[] parts = shortcut.ToLower().Split(new[] { '+' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string k in parts)
                {
                    if (k.Equals("CONTROL", StringComparison.OrdinalIgnoreCase))
                    {
                        checkBoxShortcutsControl.Checked = true;
                    }
                    else if (k.Equals("ALT", StringComparison.OrdinalIgnoreCase))
                    {
                        checkBoxShortcutsAlt.Checked = true;
                    }
                    else if (k.Equals("SHIFT", StringComparison.OrdinalIgnoreCase))
                    {
                        checkBoxShortcutsShift.Checked = true;
                    }
                    else
                    {
                        int i = 0;
                        foreach (string value in comboBoxShortcutKey.Items)
                        {
                            if (value.Equals(k, StringComparison.OrdinalIgnoreCase))
                            {
                                comboBoxShortcutKey.SelectedIndex = i;
                                break;
                            }
                            i++;
                        }
                    }
                }
            }
        }

        private static string GetShortcut(string text)
        {
            string shortcut = text.Substring(text.IndexOf('['));
            shortcut = shortcut.TrimEnd(']').TrimStart('[');
            if (shortcut == Configuration.Settings.Language.General.None)
                return string.Empty;
            return shortcut;
        }

        private string GetCurrentShortcutText()
        {
            var sb = new StringBuilder(@"[");
            if (checkBoxShortcutsControl.Checked)
                sb.Append("Control+");
            if (checkBoxShortcutsAlt.Checked)
                sb.Append("Alt+");
            if (checkBoxShortcutsShift.Checked)
                sb.Append("Shift+");
            sb.Append(comboBoxShortcutKey.Items[comboBoxShortcutKey.SelectedIndex]);
            sb.Append(']');
            return sb.ToString();
        }

        private void buttonUpdateShortcut_Click(object sender, EventArgs e)
        {
            if (!IsShortcutValid())
                return;

            string text = treeViewShortcuts.SelectedNode.Text.Substring(0, treeViewShortcuts.SelectedNode.Text.IndexOf('[')).Trim();
            var shortcutText = GetCurrentShortcutText();
            var existsIn = new StringBuilder();
            if (comboBoxShortcutKey.SelectedIndex > 0)
            {
                foreach (TreeNode node in treeViewShortcuts.Nodes)
                {
                    foreach (TreeNode subNode in node.Nodes)
                    {
                        if (subNode.Text.Contains(shortcutText) && treeViewShortcuts.SelectedNode.Text != subNode.Text)
                            existsIn.AppendLine(string.Format(Configuration.Settings.Language.Settings.ShortcutIsAlreadyDefinedX, node.Text + " -> " + subNode.Text));
                    }
                }
                if (existsIn.Length > 0)
                {
                    MessageBox.Show(existsIn.ToString(), string.Empty, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
            }
            treeViewShortcuts.SelectedNode.Text = text + " " + shortcutText;
            AddToSaveList((ShortcutHelper)treeViewShortcuts.SelectedNode.Tag, shortcutText);
            treeViewShortcuts.Focus();
        }

        private void AddToSaveList(ShortcutHelper helper, string shortcutText)
        {
            if (_newShortcuts.ContainsKey(helper))
            {
                _newShortcuts[helper] = GetShortcut(shortcutText);
            }
            else
            {
                _newShortcuts.Add(helper, GetShortcut(shortcutText));
            }
        }

        private void buttonListViewSyntaxColorError_Click(object sender, EventArgs e)
        {
            colorDialogSSAStyle.Color = panelListViewSyntaxColorError.BackColor;
            if (colorDialogSSAStyle.ShowDialog() == DialogResult.OK)
                panelListViewSyntaxColorError.BackColor = colorDialogSSAStyle.Color;
        }

        private void comboBoxShortcutKey_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Tab ||
                e.KeyCode == Keys.Down ||
                e.KeyCode == Keys.Up ||
                e.KeyCode == Keys.Enter ||
                e.KeyCode == Keys.None)
                return;

            int i = 0;
            foreach (var item in comboBoxShortcutKey.Items)
            {
                if (item.ToString() == e.KeyCode.ToString())
                {
                    comboBoxShortcutKey.SelectedIndex = i;
                    e.SuppressKeyPress = true;
                    return;
                }
                i++;
            }
        }

        private void numericUpDownSsaOutline_ValueChanged(object sender, EventArgs e)
        {
            UpdateSsaExample();
        }

        private void numericUpDownSsaShadow_ValueChanged(object sender, EventArgs e)
        {
            UpdateSsaExample();
        }

        private void checkBoxSsaOpaqueBox_CheckedChanged(object sender, EventArgs e)
        {
            numericUpDownSsaOutline.Enabled = !checkBoxSsaOpaqueBox.Checked;
            numericUpDownSsaShadow.Enabled = !checkBoxSsaOpaqueBox.Checked;
            UpdateSsaExample();
        }

        private void buttonBrowseToFFmpeg_Click(object sender, EventArgs e)
        {
            openFileDialogFFmpeg.FileName = string.Empty;
            openFileDialogFFmpeg.Title = Configuration.Settings.Language.Settings.WaveformBrowseToFFmpeg;
            if (!Configuration.IsRunningOnLinux() && !Configuration.IsRunningOnMac())
            {
                openFileDialogFFmpeg.Filter = "FFmpeg.exe|FFmpeg.exe";
            }
            if (openFileDialogFFmpeg.ShowDialog(this) == DialogResult.OK)
                textBoxFFmpegPath.Text = openFileDialogFFmpeg.FileName;
        }

        private void checkBoxWaveformHoverFocus_CheckedChanged(object sender, EventArgs e)
        {
            checkBoxListViewMouseEnterFocus.Enabled = checkBoxWaveformHoverFocus.Checked;
        }

        private void buttonVlcPathBrowse_Click(object sender, EventArgs e)
        {
            openFileDialogFFmpeg.FileName = string.Empty;
            openFileDialogFFmpeg.Title = Configuration.Settings.Language.Settings.WaveformBrowseToVLC;
            if (!Configuration.IsRunningOnLinux() && !Configuration.IsRunningOnMac())
            {
                openFileDialogFFmpeg.Filter = "vlc.exe|vlc.exe";
            }
            if (openFileDialogFFmpeg.ShowDialog(this) == DialogResult.OK)
            {
                EnableVlc(openFileDialogFFmpeg.FileName);
            }
        }

        private void EnableVlc(string fileName)
        {
            textBoxVlcPath.Text = Path.GetDirectoryName(fileName);
            Configuration.Settings.General.VlcLocation = textBoxVlcPath.Text;
            Configuration.Settings.General.VlcLocationRelative = GetRelativePath(textBoxVlcPath.Text);
            radioButtonVideoPlayerVLC.Enabled = LibVlcDynamic.IsInstalled;
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            Configuration.Settings.General.VlcLocation = _oldVlcLocation;
            Configuration.Settings.General.VlcLocationRelative = _oldVlcLocationRelative;
            Configuration.Settings.Tools.ListViewShowColumnCharsPerSec = _oldListViewShowCps;
            Configuration.Settings.Tools.ListViewShowColumnWordsPerMin = _oldListViewShowWpm;

            DialogResult = DialogResult.Cancel;
        }

        private void buttonEditDoNotBreakAfterList_Click(object sender, EventArgs e)
        {
            using (var form = new DoNotBreakAfterListEdit())
            {
                form.ShowDialog(this);
            }
        }

        private void linkLabelOpenDictionaryFolder_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            string dictionaryFolder = Utilities.DictionaryFolder;
            if (!Directory.Exists(dictionaryFolder))
                Directory.CreateDirectory(dictionaryFolder);

            Process.Start(dictionaryFolder);
        }

        private void textBoxVlcPath_MouseLeave(object sender, EventArgs e)
        {
            try
            {
                var path = textBoxVlcPath.Text.Trim('\"');
                if (path.Length > 3 && Path.IsPathRooted(path) && Path.GetFileName(path).Equals("vlc.exe", StringComparison.OrdinalIgnoreCase) && File.Exists(path))
                {
                    EnableVlc(path);
                }
            }
            catch
            {
                // ignored
            }
        }

        private void buttonNetworkSessionNewMessageSound_Click(object sender, EventArgs e)
        {
            openFileDialogFFmpeg.FileName = string.Empty;
            openFileDialogFFmpeg.Title = Configuration.Settings.Language.Settings.WaveformBrowseToFFmpeg;
            openFileDialogFFmpeg.Filter = Configuration.Settings.Language.General.AudioFiles + "|*.wav";
            if (openFileDialogFFmpeg.ShowDialog(this) == DialogResult.OK)
                textBoxNetworkSessionNewMessageSound.Text = openFileDialogFFmpeg.FileName;
        }

        private void panelPrimaryColor_MouseClick(object sender, MouseEventArgs e)
        {
            colorDialogSSAStyle.Color = Color.FromArgb(_ssaFontColor);
            if (colorDialogSSAStyle.ShowDialog() == DialogResult.OK)
            {
                _ssaFontColor = colorDialogSSAStyle.Color.ToArgb();
                panelPrimaryColor.BackColor = colorDialogSSAStyle.Color;
                UpdateSsaExample();
            }
        }

        private void numericUpDownSsaMarginVertical_ValueChanged(object sender, EventArgs e)
        {
            UpdateSsaExample();
        }

        private void comboBoxFontName_TextChanged(object sender, EventArgs e)
        {
            _ssaFontName = comboBoxFontName.Text;
            UpdateSsaExample();
        }

        private void numericUpDownFontSize_ValueChanged(object sender, EventArgs e)
        {
            _ssaFontSize = (int)numericUpDownFontSize.Value;
            UpdateSsaExample();
        }

        private void buttonSsaColor_Click(object sender, EventArgs e)
        {
            panelPrimaryColor_MouseClick(sender, null);
        }

        private void checkBoxSsaFontBold_CheckedChanged(object sender, EventArgs e)
        {
            UpdateSsaExample();
        }

        private void buttonMpvSettings_Click(object sender, EventArgs e)
        {
            using (var form = new SettingsMpv())
            {
                var oldMpvEnabled = radioButtonVideoPlayerMPV.Enabled;
                if (form.ShowDialog(this) == DialogResult.OK)
                {
                    RefreshMpvSettings();
                    if (radioButtonVideoPlayerMPV.Enabled && !oldMpvEnabled)
                        radioButtonVideoPlayerMPV.Checked = true;
                }
                else
                {
                    RefreshMpvSettings();
                }
            }
        }

        private void RefreshMpvSettings()
        {
            radioButtonVideoPlayerMPV.Enabled = LibMpvDynamic.IsInstalled;
            checkBoxMpvHandlesPreviewText.Enabled = radioButtonVideoPlayerMPV.Enabled;
            if (!radioButtonVideoPlayerMPV.Enabled)
            {
                buttonMpvSettings.Font = new Font(buttonMpvSettings.Font.FontFamily, buttonMpvSettings.Font.Size, FontStyle.Bold);
            }

            if (Configuration.IsRunningOnLinux() && Configuration.Settings.General.MpvVideoOutput.StartsWith("direct3d"))
                labelMpvSettings.Text = "--vo=vaapi";
            else
                labelMpvSettings.Text = "--vo=" + Configuration.Settings.General.MpvVideoOutput;
        }

        private void linkLabelBingSubscribe_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("http://blogs.msdn.com/b/translation/p/gettingstarted1.aspx");
        }

        private void ValidateShortcut(object sender, EventArgs e)
        {
            buttonUpdateShortcut.Enabled = IsShortcutValid();
        }

        private bool IsShortcutValid()
        {
            if (treeViewShortcuts.SelectedNode == null || !treeViewShortcuts.SelectedNode.Text.Contains('['))
            {
                return false;
            }

            var shortcutText = GetCurrentShortcutText();
            if (shortcutText == "[CapsLock]" || shortcutText.Length < 3 || shortcutText.EndsWith("+]"))
            {
                return false;
            }

            if (comboBoxShortcutKey.SelectedIndex == 0 && !checkBoxShortcutsControl.Checked && !checkBoxShortcutsAlt.Checked && !checkBoxShortcutsShift.Checked)
            {
                return true;
            }

            var helper = (ShortcutHelper)treeViewShortcuts.SelectedNode.Tag;
            if (helper.IsMenuItem)
            {
                try
                {
                    new ToolStripMenuItem().ShortcutKeys = UiUtil.GetKeys(GetShortcut(shortcutText));
                }
                catch (InvalidEnumArgumentException)
                {
                    return false;
                }
            }
            return true;
        }

        private void numericUpDownMaxNumberOfLines_ValueChanged(object sender, EventArgs e)
        {
            checkBoxSyntaxColorTextMoreThanTwoLines.Text = string.Format(Configuration.Settings.Language.Settings.SyntaxColorTextMoreThanMaxLines, numericUpDownMaxNumberOfLines.Value);
        }

        private void buttonListviewColumns_Click(object sender, EventArgs e)
        {
            using (var form = new SettingsListViewColumns())
            {
                if (form.ShowDialog(this) == DialogResult.OK)
                {
                    Configuration.Settings.Tools.ListViewShowColumnEndTime = form.ShowEndTime;
                    Configuration.Settings.Tools.ListViewShowColumnDuration = form.ShowDuration;
                    Configuration.Settings.Tools.ListViewShowColumnCharsPerSec = form.ShowCps;
                    Configuration.Settings.Tools.ListViewShowColumnWordsPerMin = form.ShowWpm;
                    buttonListviewColumns.Text = GetListViewColumns();
                }
            }
        }

        private void radioButtonVideoPlayerMPV_CheckedChanged(object sender, EventArgs e)
        {
            checkBoxMpvHandlesPreviewText.Enabled = radioButtonVideoPlayerMPV.Checked;
        }

    }
}
