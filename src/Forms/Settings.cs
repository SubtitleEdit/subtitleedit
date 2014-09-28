using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.VideoPlayers;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace Nikse.SubtitleEdit.Forms
{
    public sealed partial class Settings : Form
    {
        private string _ssaFontName;
        private double _ssaFontSize;
        private int _ssaFontColor;
        private string _listBoxSearchString = string.Empty;
        private DateTime _listBoxSearchStringLastUsed = DateTime.Now;
        private readonly List<string> _wordListNamesEtc = new List<string>();
        private List<string> _userWordList = new List<string>();
        private OcrFixReplaceList _ocrFixReplaceList;
        private readonly string _oldVlcLocation;
        private readonly string _oldVlcLocationRelative;

        private class ComboBoxLanguage
        {
            public CultureInfo CultureInfo { get; set; }
            public override string ToString()
            {
                return CultureInfo.NativeName;
            }
        }

        private static string GetRelativePath(string fileName)
        {
            string folder = Configuration.BaseDirectory;

            if (string.IsNullOrEmpty(fileName) || !fileName.StartsWith(folder.Substring(0, 2), StringComparison.OrdinalIgnoreCase))
                return string.Empty;

            Uri pathUri = new Uri(fileName);
            if (!folder.EndsWith(Path.DirectorySeparatorChar))
                folder += Path.DirectorySeparatorChar;
            Uri folderUri = new Uri(folder);
            return Uri.UnescapeDataString(folderUri.MakeRelativeUri(pathUri).ToString().Replace('/', Path.DirectorySeparatorChar));
        }

        public Settings()
        {
            InitializeComponent();

            labelStatus.Text = string.Empty;

            GeneralSettings gs = Configuration.Settings.General;

            checkBoxToolbarNew.Checked = gs.ShowToolbarNew;
            checkBoxToolbarOpen.Checked = gs.ShowToolbarOpen;
            checkBoxToolbarSave.Checked = gs.ShowToolbarSave;
            checkBoxToolbarSaveAs.Checked = gs.ShowToolbarSaveAs;
            checkBoxToolbarFind.Checked = gs.ShowToolbarFind;
            checkBoxReplace.Checked = gs.ShowToolbarReplace;
            checkBoxTBFixCommonErrors.Checked = gs.ShowToolbarFixCommonErrors;
            checkBoxVisualSync.Checked = gs.ShowToolbarVisualSync;
            checkBoxSettings.Checked = gs.ShowToolbarSettings;
            checkBoxSpellCheck.Checked = gs.ShowToolbarSpellCheck;
            checkBoxHelp.Checked = gs.ShowToolbarHelp;

            comboBoxFrameRate.Items.Add((23.976).ToString());
            comboBoxFrameRate.Items.Add((24.0).ToString());
            comboBoxFrameRate.Items.Add((25.0).ToString());
            comboBoxFrameRate.Items.Add((29.97).ToString());

            checkBoxShowFrameRate.Checked = gs.ShowFrameRate;
            comboBoxFrameRate.Text = gs.DefaultFrameRate.ToString();

            comboBoxEncoding.Items.Clear();
            int encodingSelectedIndex = 0;
            comboBoxEncoding.Items.Add(Encoding.UTF8.EncodingName);
            foreach (EncodingInfo ei in Encoding.GetEncodings())
            {
                if (ei.Name != Encoding.UTF8.BodyName && ei.CodePage >= 949 && !ei.DisplayName.Contains("EBCDIC") && ei.CodePage != 1047)
                {
                    comboBoxEncoding.Items.Add(ei.CodePage + ": " + ei.DisplayName);
                    if (ei.Name == gs.DefaultEncoding || ei.CodePage + ": " + ei.DisplayName == gs.DefaultEncoding)
                        encodingSelectedIndex = comboBoxEncoding.Items.Count - 1;
                }
            }
            comboBoxEncoding.SelectedIndex = encodingSelectedIndex;

            checkBoxAutoDetectAnsiEncoding.Checked = gs.AutoGuessAnsiEncoding;
            comboBoxSubtitleFontSize.Text = gs.SubtitleFontSize.ToString();
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
            checkBoxAutoWrapWhileTyping.Checked = gs.AutoWrapLineWhileTyping;
            textBoxShowLineBreaksAs.Text = gs.ListViewLineSeparatorString;

            numericUpDownDurationMin.Value = Configuration.Settings.General.SubtitleMinimumDisplayMilliseconds;
            numericUpDownDurationMax.Value = Configuration.Settings.General.SubtitleMaximumDisplayMilliseconds;

            if (Configuration.Settings.General.MininumMillisecondsBetweenLines >= numericUpDownMinGapMs.Minimum &&
                Configuration.Settings.General.MininumMillisecondsBetweenLines <= numericUpDownMinGapMs.Maximum)
                numericUpDownMinGapMs.Value = Configuration.Settings.General.MininumMillisecondsBetweenLines;

            if (gs.VideoPlayer.Trim().Equals("VLC", StringComparison.OrdinalIgnoreCase) && LibVlcDynamic.IsInstalled)
                radioButtonVideoPlayerVLC.Checked = true;
            else if (gs.VideoPlayer.Trim().Equals("MPlayer", StringComparison.OrdinalIgnoreCase) && Utilities.IsMPlayerAvailable)
                radioButtonVideoPlayerMPlayer.Checked = true;
            else if (gs.VideoPlayer.Trim().Equals("MPC-HC", StringComparison.OrdinalIgnoreCase) && Utilities.IsMpcHcInstalled)
                radioButtonVideoPlayerMpcHc.Checked = true;
            else if (Utilities.IsQuartsDllInstalled)
                radioButtonVideoPlayerDirectShow.Checked = true;
            else if (Utilities.IsMPlayerAvailable)
                radioButtonVideoPlayerMPlayer.Checked = true;
            else if (LibVlcDynamic.IsInstalled)
                radioButtonVideoPlayerVLC.Checked = true;

            if (!LibVlcDynamic.IsInstalled)
                radioButtonVideoPlayerVLC.Enabled = false;
            if (!Utilities.IsMPlayerAvailable)
                radioButtonVideoPlayerMPlayer.Enabled = false;
            if (!Utilities.IsQuartsDllInstalled)
                radioButtonVideoPlayerDirectShow.Enabled = false;

            textBoxVlcPath.Text = Configuration.Settings.General.VlcLocation;
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

            WordListSettings wordListSettings = Configuration.Settings.WordLists;
            checkBoxNamesEtcOnline.Checked = wordListSettings.UseOnlineNamesEtc;
            textBoxNamesEtcOnline.Text = wordListSettings.NamesEtcUrl;

            SubtitleSettings ssa = Configuration.Settings.SubtitleSettings;
            _ssaFontName = ssa.SsaFontName;
            _ssaFontSize = ssa.SsaFontSize;
            _ssaFontColor = ssa.SsaFontColorArgb;
            fontDialogSSAStyle.Font = new Font(ssa.SsaFontName, (float)ssa.SsaFontSize);
            fontDialogSSAStyle.Color = Color.FromArgb(_ssaFontColor);
            if (ssa.SsaOutline >= numericUpDownSsaOutline.Minimum && ssa.SsaOutline <= numericUpDownSsaOutline.Maximum)
                numericUpDownSsaOutline.Value = ssa.SsaOutline;
            if (ssa.SsaShadow >= numericUpDownSsaShadow.Minimum && ssa.SsaShadow <= numericUpDownSsaShadow.Maximum)
                numericUpDownSsaShadow.Value = ssa.SsaShadow;
            checkBoxSsaOpaqueBox.Checked = ssa.SsaOpaqueBox;
            UpdateSsaExample();

            var proxy = Configuration.Settings.Proxy;
            textBoxProxyAddress.Text = proxy.ProxyAddress;
            textBoxProxyUserName.Text = proxy.UserName;
            if (proxy.Password == null)
                textBoxProxyPassword.Text = string.Empty;
            else
                textBoxProxyPassword.Text = proxy.DecodePassword();
            textBoxProxyDomain.Text = proxy.Domain;

            checkBoxSyntaxColorDurationTooSmall.Checked = Configuration.Settings.Tools.ListViewSyntaxColorDurationSmall;
            checkBoxSyntaxColorDurationTooLarge.Checked = Configuration.Settings.Tools.ListViewSyntaxColorDurationBig;
            checkBoxSyntaxColorTextTooLong.Checked = Configuration.Settings.Tools.ListViewSyntaxColorLongLines;
            checkBoxSyntaxColorTextMoreThanTwoLines.Checked = Configuration.Settings.Tools.ListViewSyntaxMoreThanXLines;
            numericUpDownSyntaxColorTextMoreThanXLines.Value = Configuration.Settings.Tools.ListViewSyntaxMoreThanXLinesX;
            checkBoxSyntaxOverlap.Checked = Configuration.Settings.Tools.ListViewSyntaxColorOverlap;
            panelListViewSyntaxColorError.BackColor = Configuration.Settings.Tools.ListViewSyntaxErrorColor;

            // Language
            var language = Configuration.Settings.Language.Settings;
            Text = language.Title;
            tabPageGenerel.Text = language.General;
            tabPageVideoPlayer.Text = language.VideoPlayer;
            tabPageWaveform.Text = language.WaveformAndSpectrogram;
            tabPageWordLists.Text = language.WordLists;
            tabPageTools.Text = language.Tools;
            tabPageSsaStyle.Text = language.SsaStyle;
            tabPageProxy.Text = language.Proxy;
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
            labelTBVisualSync.Text = language.VisualSync;
            labelTBSpellCheck.Text = language.SpellCheck;
            labelTBSettings.Text = language.SettingsName;
            labelTBHelp.Text = language.Help;
            checkBoxToolbarNew.Text = Configuration.Settings.Language.General.Visible;
            checkBoxToolbarOpen.Text = Configuration.Settings.Language.General.Visible;
            checkBoxToolbarSave.Text = Configuration.Settings.Language.General.Visible;
            checkBoxToolbarSaveAs.Text = Configuration.Settings.Language.General.Visible;
            checkBoxToolbarFind.Text = Configuration.Settings.Language.General.Visible;
            checkBoxReplace.Text = Configuration.Settings.Language.General.Visible;
            checkBoxTBFixCommonErrors.Text = Configuration.Settings.Language.General.Visible;
            checkBoxVisualSync.Text = Configuration.Settings.Language.General.Visible;
            checkBoxSpellCheck.Text = Configuration.Settings.Language.General.Visible;
            checkBoxSettings.Text = Configuration.Settings.Language.General.Visible;
            checkBoxHelp.Text = Configuration.Settings.Language.General.Visible;

            groupBoxMiscellaneous.Text = language.General;
            checkBoxShowFrameRate.Text = language.ShowFrameRate;
            labelDefaultFrameRate.Text = language.DefaultFrameRate;
            labelDefaultFileEncoding.Text = language.DefaultFileEncoding;
            labelAutoDetectAnsiEncoding.Text = language.AutoDetectAnsiEncoding;
            labelSubMaxLen.Text = language.SubtitleLineMaximumLength;
            labelMaxCharsPerSecond.Text = language.MaximumCharactersPerSecond;
            checkBoxAutoWrapWhileTyping.Text = language.AutoWrapWhileTyping;

            labelMinDuration.Text = language.DurationMinimumMilliseconds;
            labelMaxDuration.Text = language.DurationMaximumMilliseconds;
            labelMinGapMs.Text = language.MinimumGapMilliseconds;
            if (string.IsNullOrEmpty(language.MinimumGapMilliseconds))
            {
                labelMinGapMs.Visible = false;
                numericUpDownMinGapMs.Visible = false;
            }
            if (labelSubMaxLen.Left + labelSubMaxLen.Width > numericUpDownSubtitleLineMaximumLength.Left)
                numericUpDownSubtitleLineMaximumLength.Left = labelSubMaxLen.Left + labelSubMaxLen.Width + 3;
            if (labelMaxCharsPerSecond.Left + labelMaxCharsPerSecond.Width > numericUpDownMaxCharsSec.Left)
                numericUpDownMaxCharsSec.Left = labelMaxCharsPerSecond.Left + labelMaxCharsPerSecond.Width + 3;
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
            labelAutoBackup.Text = language.AutoBackup;
            comboBoxAutoBackup.Left = labelAutoBackup.Left + labelAutoBackup.Width + 3;
            checkBoxCheckForUpdates.Text = language.CheckForUpdates;
            checkBoxAllowEditOfOriginalSubtitle.Text = language.AllowEditOfOriginalSubtitle;
            checkBoxPromptDeleteLines.Text = language.PromptDeleteLines;

            comboBoxTimeCodeMode.Items.Clear();
            comboBoxTimeCodeMode.Items.Add(language.TimeCodeModeHHMMSSMS);
            comboBoxTimeCodeMode.Items.Add(language.TimeCodeModeHHMMSSFF);
            if (Configuration.Settings.General.UseTimeFormatHHMMSSFF)
                comboBoxTimeCodeMode.SelectedIndex = 1;
            else
                comboBoxTimeCodeMode.SelectedIndex = 0;
            labelTimeCodeMode.Text = language.TimeCodeMode;
            comboBoxTimeCodeMode.Left = labelTimeCodeMode.Left + labelTimeCodeMode.Width + 4;

            comboBoxAutoBackup.Items[0] = Configuration.Settings.Language.General.None;
            comboBoxAutoBackup.Items[1] = language.AutoBackupEveryMinute;
            comboBoxAutoBackup.Items[2] = language.AutoBackupEveryFiveMinutes;
            comboBoxAutoBackup.Items[3] = language.AutoBackupEveryFifteenMinutes;

            groupBoxVideoEngine.Text = language.VideoEngine;
            radioButtonVideoPlayerDirectShow.Text = language.DirectShow;

            labelDirectShowDescription.Text = language.DirectShowDescription;

            if (!string.IsNullOrEmpty(language.MpcHc))
            {
                radioButtonVideoPlayerMpcHc.Text = language.MpcHc;
                labelMpcHcDescription.Text = language.MpcHcDescription;
            }

            radioButtonVideoPlayerMPlayer.Text = language.MPlayer;
            labelVideoPlayerMPlayer.Text = language.MPlayerDescription;
            if (!string.IsNullOrEmpty(language.VlcMediaPlayer))
            {
                radioButtonVideoPlayerVLC.Text = language.VlcMediaPlayer;
                labelVideoPlayerVLC.Text = language.VlcMediaPlayerDescription;
            }
            if (!string.IsNullOrEmpty(language.VlcMediaPlayer))
                Configuration.Settings.General.VlcLocation = textBoxVlcPath.Text;

            checkBoxVideoPlayerShowStopButton.Text = language.ShowStopButton;
            checkBoxVideoPlayerShowMuteButton.Text = language.ShowMuteButton;
            checkBoxVideoPlayerShowFullscreenButton.Text = language.ShowFullscreenButton;

            labelVideoPlayerPreviewFontSize.Text = language.PreviewFontSize;
            comboBoxlVideoPlayerPreviewFontSize.Left = labelVideoPlayerPreviewFontSize.Left + labelVideoPlayerPreviewFontSize.Width;
            checkBoxVideoPlayerPreviewFontBold.Text = language.SubtitleBold;
            checkBoxVideoPlayerPreviewFontBold.Left = comboBoxlVideoPlayerPreviewFontSize.Left;

            groupBoxMainWindowVideoControls.Text = language.MainWindowVideoControls;
            labelCustomSearch.Text = language.CustomSearchTextAndUrl;

            groupBoxWaveformAppearence.Text = language.WaveformAppearance;
            checkBoxWaveformShowGrid.Text = language.WaveformShowGridLines;
            checkBoxReverseMouseWheelScrollDirection.Text = language.ReverseMouseWheelScrollDirection;
            checkBoxAllowOverlap.Text = language.WaveformAllowOverlap;
            checkBoxWaveformHoverFocus.Text = language.WaveformFocusMouseEnter;
            checkBoxListViewMouseEnterFocus.Text = language.WaveformListViewFocusMouseEnter;
            if (string.IsNullOrEmpty(language.WaveformListViewFocusMouseEnter)) //TODO: Remove in SE 3.4
            {
                checkBoxWaveformHoverFocus.Visible = false;
                checkBoxListViewMouseEnterFocus.Visible = false;
            }
            labelWaveformBorderHitMs1.Text = language.WaveformBorderHitMs1;
            labelWaveformBorderHitMs2.Text = language.WaveformBorderHitMs2;
            numericUpDownWaveformBorderHitMs.Left = labelWaveformBorderHitMs1.Left + labelWaveformBorderHitMs1.Width;
            labelWaveformBorderHitMs2.Left = numericUpDownWaveformBorderHitMs.Left + numericUpDownWaveformBorderHitMs.Width + 2;
            if (string.IsNullOrEmpty(language.WaveformBorderHitMs1))
            {
                labelWaveformBorderHitMs1.Visible = false;
                numericUpDownWaveformBorderHitMs.Visible = false;
                labelWaveformBorderHitMs2.Visible = false;
            }

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

            buttonWaveformsFolderEmpty.Text = language.WaveformAndSpectrogramsFolderEmpty;
            InitializeWaveformsAndSpectrogramsFolderEmpty(language);

            if (!string.IsNullOrEmpty(language.WaveformFFmpegPath)) //TODO: Remove in SE 3.4
            {
                checkBoxUseFFmpeg.Text = language.WaveformUseFFmpeg;
                labelFFmpegPath.Text = language.WaveformFFmpegPath;
            }

            groupBoxSsaStyle.Text = language.SubStationAlphaStyle;
            buttonSSAChooseFont.Text = language.ChooseFont;
            buttonSSAChooseColor.Text = language.ChooseColor;

            if (language.SsaOutline != null) // TODO: Remove in SE 3.4
            {
                labelSsaOutline.Text = language.SsaOutline;
                labelSsaShadow.Text = language.SsaShadow;
                checkBoxSsaOpaqueBox.Text = language.SsaOpaqueBox;
            }
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

            groupBoxProxySettings.Text = language.ProxyServerSettings;
            labelProxyAddress.Text = language.ProxyAddress;
            groupBoxProxyAuthentication.Text = language.ProxyAuthentication;
            labelProxyUserName.Text = language.ProxyUserName;
            labelProxyPassword.Text = language.ProxyPassword;
            labelProxyDomain.Text = language.ProxyDomain;

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
            if (string.IsNullOrEmpty(language.FixCommonerrorsFixShortDisplayTimesAllowMoveStartTime))
                checkBoxFixShortDisplayTimesAllowMoveStartTime.Visible = false; // TODO: remove in SE 3.4
            groupBoxSpellCheck.Text = language.SpellCheck;
            checkBoxSpellCheckAutoChangeNames.Text = Configuration.Settings.Language.SpellCheck.AutoFixNames;
            checkBoxSpellCheckOneLetterWords.Text = Configuration.Settings.Language.SpellCheck.CheckOneLetterWords;
            if (string.IsNullOrEmpty(Configuration.Settings.Language.SpellCheck.CheckOneLetterWords)) // TODO: remove in SE 3.4
                checkBoxSpellCheckOneLetterWords.Visible = false;
            checkBoxTreatINQuoteAsING.Text = Configuration.Settings.Language.SpellCheck.TreatINQuoteAsING;
            if (string.IsNullOrEmpty(Configuration.Settings.Language.SpellCheck.TreatINQuoteAsING)) // TODO: remove in SE 3.4
                checkBoxTreatINQuoteAsING.Visible = false;

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

            if (Configuration.Settings.General.ListViewDoubleClickAction >= 0 && Configuration.Settings.General.ListViewDoubleClickAction < comboBoxListViewDoubleClickEvent.Items.Count)
                comboBoxListViewDoubleClickEvent.SelectedIndex =
                    Configuration.Settings.General.ListViewDoubleClickAction;

            if (gs.AutoBackupSeconds == 60)
                comboBoxAutoBackup.SelectedIndex = 1;
            else if (gs.AutoBackupSeconds == 60 * 5)
                comboBoxAutoBackup.SelectedIndex = 2;
            else if (gs.AutoBackupSeconds == 60 * 15)
                comboBoxAutoBackup.SelectedIndex = 3;
            else
                comboBoxAutoBackup.SelectedIndex = 0;

            checkBoxCheckForUpdates.Checked = Configuration.Settings.General.CheckForUpdates;

            if (gs.SpellChecker.Contains("word", StringComparison.OrdinalIgnoreCase))
                comboBoxSpellChecker.SelectedIndex = 1;
            else
                comboBoxSpellChecker.SelectedIndex = 0;

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
            checkBoxSpellCheckAutoChangeNames.Checked = toolsSettings.SpellCheckAutoChangeNames;
            checkBoxSpellCheckOneLetterWords.Checked = toolsSettings.SpellCheckOneLetterWords;
            checkBoxTreatINQuoteAsING.Checked = toolsSettings.SpellCheckEnglishAllowInQuoteAsIng;
            checkBoxUseDoNotBreakAfterList.Checked = toolsSettings.UseNoLineBreakAfter;

            buttonOK.Text = Configuration.Settings.Language.General.Ok;
            buttonCancel.Text = Configuration.Settings.Language.General.Cancel;

            ListWordListLanguages();

            checkBoxWaveformShowGrid.Checked = Configuration.Settings.VideoControls.WaveformDrawGrid;
            panelWaveformGridColor.BackColor = Configuration.Settings.VideoControls.WaveformGridColor;
            panelWaveformSelectedColor.BackColor = Configuration.Settings.VideoControls.WaveformSelectedColor;
            panelWaveformColor.BackColor = Configuration.Settings.VideoControls.WaveformColor;
            panelWaveformBackgroundColor.BackColor = Configuration.Settings.VideoControls.WaveformBackgroundColor;
            panelWaveformTextColor.BackColor = Configuration.Settings.VideoControls.WaveformTextColor;
            checkBoxGenerateSpectrogram.Checked = Configuration.Settings.VideoControls.GenerateSpectrogram;
            if (Configuration.Settings.VideoControls.SpectrogramAppearance == "OneColorGradient")
                comboBoxSpectrogramAppearance.SelectedIndex = 0;
            else
                comboBoxSpectrogramAppearance.SelectedIndex = 1;
            checkBoxReverseMouseWheelScrollDirection.Checked = Configuration.Settings.VideoControls.WaveformMouseWheelScrollUpIsForward;
            checkBoxAllowOverlap.Checked = Configuration.Settings.VideoControls.WaveformAllowOverlap;
            checkBoxWaveformHoverFocus.Checked = Configuration.Settings.VideoControls.WaveformFocusOnMouseEnter;
            checkBoxListViewMouseEnterFocus.Checked = Configuration.Settings.VideoControls.WaveformListViewFocusOnMouseEnter;
            checkBoxListViewMouseEnterFocus.Enabled = Configuration.Settings.VideoControls.WaveformFocusOnMouseEnter;
            if (Configuration.Settings.VideoControls.WaveformBorderHitMs >= numericUpDownWaveformBorderHitMs.Minimum &&
                Configuration.Settings.VideoControls.WaveformBorderHitMs <= numericUpDownWaveformBorderHitMs.Maximum)
                numericUpDownWaveformBorderHitMs.Value = Configuration.Settings.VideoControls.WaveformBorderHitMs;
            checkBoxUseFFmpeg.Checked = Configuration.Settings.General.UseFFmpegForWaveExtraction;
            textBoxFFmpegPath.Text = Configuration.Settings.General.FFmpegLocation;
            var generalNode = new TreeNode(Configuration.Settings.Language.General.GeneralText);
            generalNode.Nodes.Add(language.MergeSelectedLines + GetShortcutText(Configuration.Settings.Shortcuts.GeneralMergeSelectedLines));
            if (!string.IsNullOrEmpty(Configuration.Settings.Language.Settings.MergeSelectedLinesOnlyFirstText))
                generalNode.Nodes.Add(Configuration.Settings.Language.Settings.MergeSelectedLinesOnlyFirstText + GetShortcutText(Configuration.Settings.Shortcuts.GeneralMergeSelectedLinesOnlyFirstText));
            generalNode.Nodes.Add(language.MergeOriginalAndTranslation + GetShortcutText(Configuration.Settings.Shortcuts.GeneralMergeOriginalAndTranslation));
            generalNode.Nodes.Add(language.ToggleTranslationMode + GetShortcutText(Configuration.Settings.Shortcuts.GeneralToggleTranslationMode));
            generalNode.Nodes.Add(language.SwitchOriginalAndTranslation + GetShortcutText(Configuration.Settings.Shortcuts.GeneralSwitchOriginalAndTranslation));
            if (!string.IsNullOrEmpty(Configuration.Settings.Language.Settings.WaveformPlayFirstSelectedSubtitle))
                generalNode.Nodes.Add(Configuration.Settings.Language.Settings.WaveformPlayFirstSelectedSubtitle + GetShortcutText(Configuration.Settings.Shortcuts.GeneralPlayFirstSelected));
            generalNode.Nodes.Add(language.GoToFirstSelectedLine + GetShortcutText(Configuration.Settings.Shortcuts.GeneralGoToFirstSelectedLine));
            if (!string.IsNullOrEmpty(Configuration.Settings.Language.Settings.GoToNextEmptyLine))
                generalNode.Nodes.Add(Configuration.Settings.Language.Settings.GoToNextEmptyLine + GetShortcutText(Configuration.Settings.Shortcuts.GeneralGoToNextEmptyLine));
            generalNode.Nodes.Add(Configuration.Settings.Language.Settings.GoToNext + GetShortcutText(Configuration.Settings.Shortcuts.GeneralGoToNextSubtitle));
            generalNode.Nodes.Add(Configuration.Settings.Language.Settings.GoToPrevious + GetShortcutText(Configuration.Settings.Shortcuts.GeneralGoToPrevSubtitle));
            if (!string.IsNullOrEmpty(Configuration.Settings.Language.Settings.GoToCurrentSubtitleStart))
                generalNode.Nodes.Add(Configuration.Settings.Language.Settings.GoToCurrentSubtitleStart + GetShortcutText(Configuration.Settings.Shortcuts.GeneralGoToStartOfCurrentSubtitle));
            if (!string.IsNullOrEmpty(Configuration.Settings.Language.Settings.GoToCurrentSubtitleEnd))
                generalNode.Nodes.Add(Configuration.Settings.Language.Settings.GoToCurrentSubtitleEnd + GetShortcutText(Configuration.Settings.Shortcuts.GeneralGoToEndOfCurrentSubtitle));
            treeViewShortcuts.Nodes.Add(generalNode);

            var fileNode = new TreeNode(Configuration.Settings.Language.Main.Menu.File.Title);
            fileNode.Nodes.Add(Configuration.Settings.Language.Main.Menu.File.New + GetShortcutText(Configuration.Settings.Shortcuts.MainFileNew));
            fileNode.Nodes.Add(Configuration.Settings.Language.Main.Menu.File.Open + GetShortcutText(Configuration.Settings.Shortcuts.MainFileOpen));
            fileNode.Nodes.Add(Configuration.Settings.Language.Main.Menu.File.OpenKeepVideo + GetShortcutText(Configuration.Settings.Shortcuts.MainFileOpenKeepVideo));
            fileNode.Nodes.Add(Configuration.Settings.Language.Main.Menu.File.Save + GetShortcutText(Configuration.Settings.Shortcuts.MainFileSave));
            fileNode.Nodes.Add(Configuration.Settings.Language.Main.Menu.File.SaveAs + GetShortcutText(Configuration.Settings.Shortcuts.MainFileSaveAs));
            fileNode.Nodes.Add(Configuration.Settings.Language.Main.Menu.File.SaveOriginal + GetShortcutText(Configuration.Settings.Shortcuts.MainFileSaveOriginal));
            fileNode.Nodes.Add(Configuration.Settings.Language.Main.SaveOriginalSubtitleAs + GetShortcutText(Configuration.Settings.Shortcuts.MainFileSaveOriginalAs));
            if (!string.IsNullOrEmpty(Configuration.Settings.Language.Settings.MainFileSaveAll))
                fileNode.Nodes.Add(Configuration.Settings.Language.Settings.MainFileSaveAll + GetShortcutText(Configuration.Settings.Shortcuts.MainFileSaveAll));
            fileNode.Nodes.Add(Configuration.Settings.Language.Main.Menu.File.Export + " -> " + Configuration.Settings.Language.Main.Menu.File.ExportEbu + GetShortcutText(Configuration.Settings.Shortcuts.MainFileExportEbu));
            treeViewShortcuts.Nodes.Add(fileNode);

            var editNode = new TreeNode(Configuration.Settings.Language.Main.Menu.Edit.Title);
            editNode.Nodes.Add(Configuration.Settings.Language.Main.Menu.Edit.Undo + GetShortcutText(Configuration.Settings.Shortcuts.MainEditUndo));
            editNode.Nodes.Add(Configuration.Settings.Language.Main.Menu.Edit.Redo + GetShortcutText(Configuration.Settings.Shortcuts.MainEditRedo));
            editNode.Nodes.Add(Configuration.Settings.Language.Main.Menu.Edit.Find + GetShortcutText(Configuration.Settings.Shortcuts.MainEditFind));
            editNode.Nodes.Add(Configuration.Settings.Language.Main.Menu.Edit.FindNext + GetShortcutText(Configuration.Settings.Shortcuts.MainEditFindNext));
            editNode.Nodes.Add(Configuration.Settings.Language.Main.Menu.Edit.Replace + GetShortcutText(Configuration.Settings.Shortcuts.MainEditReplace));
            editNode.Nodes.Add(Configuration.Settings.Language.Main.Menu.Edit.MultipleReplace + GetShortcutText(Configuration.Settings.Shortcuts.MainEditMultipleReplace));
            editNode.Nodes.Add(Configuration.Settings.Language.Main.Menu.Edit.GoToSubtitleNumber + GetShortcutText(Configuration.Settings.Shortcuts.MainEditGoToLineNumber));
            editNode.Nodes.Add(Configuration.Settings.Language.VobSubOcr.RightToLeft + GetShortcutText(Configuration.Settings.Shortcuts.MainEditRightToLeft));
            editNode.Nodes.Add(Configuration.Settings.Language.Settings.ReverseStartAndEndingForRTL + GetShortcutText(Configuration.Settings.Shortcuts.MainEditReverseStartAndEndingForRTL));
            editNode.Nodes.Add(Configuration.Settings.Language.Settings.ToggleTranslationAndOriginalInPreviews + GetShortcutText(Configuration.Settings.Shortcuts.MainEditToggleTranslationOriginalInPreviews));
            treeViewShortcuts.Nodes.Add(editNode);

            var toolsNode = new TreeNode(Configuration.Settings.Language.Main.Menu.Tools.Title);
            toolsNode.Nodes.Add(Configuration.Settings.Language.Main.Menu.Tools.FixCommonErrors + GetShortcutText(Configuration.Settings.Shortcuts.MainToolsFixCommonErrors));
            toolsNode.Nodes.Add(Configuration.Settings.Language.Main.Menu.Tools.StartNumberingFrom + GetShortcutText(Configuration.Settings.Shortcuts.MainToolsRenumber));
            toolsNode.Nodes.Add(Configuration.Settings.Language.Main.Menu.Tools.RemoveTextForHearingImpaired + GetShortcutText(Configuration.Settings.Shortcuts.MainToolsRemoveTextForHI));
            toolsNode.Nodes.Add(Configuration.Settings.Language.Main.Menu.Tools.ChangeCasing + GetShortcutText(Configuration.Settings.Shortcuts.MainToolsChangeCasing));
            toolsNode.Nodes.Add(Configuration.Settings.Language.Main.Menu.Tools.SplitLongLines + GetShortcutText(Configuration.Settings.Shortcuts.MainToolsSplitLongLines));
            toolsNode.Nodes.Add(Configuration.Settings.Language.Main.Menu.Tools.MergeShortLines + GetShortcutText(Configuration.Settings.Shortcuts.MainToolsMergeShortLines));
            toolsNode.Nodes.Add(Configuration.Settings.Language.Main.Menu.ContextMenu.AutoDurationCurrentLine + GetShortcutText(Configuration.Settings.Shortcuts.MainToolsAutoDuration));
            toolsNode.Nodes.Add(Configuration.Settings.Language.Settings.ShowBeamer + GetShortcutText(Configuration.Settings.Shortcuts.MainToolsBeamer));
            treeViewShortcuts.Nodes.Add(toolsNode);

            var videoNode = new TreeNode(Configuration.Settings.Language.Main.Menu.Video.Title);
            videoNode.Nodes.Add(Configuration.Settings.Language.Settings.TogglePlayPause + GetShortcutText(Configuration.Settings.Shortcuts.MainVideoPlayPauseToggle));
            videoNode.Nodes.Add(Configuration.Settings.Language.Settings.Pause + GetShortcutText(Configuration.Settings.Shortcuts.MainVideoPause));
            videoNode.Nodes.Add(Configuration.Settings.Language.Main.Menu.Video.ShowHideVideo + GetShortcutText(Configuration.Settings.Shortcuts.MainVideoShowHideVideo));
            videoNode.Nodes.Add(Configuration.Settings.Language.Settings.ToggleDockUndockOfVideoControls + GetShortcutText(Configuration.Settings.Shortcuts.MainVideoToggleVideoControls));
            if (!string.IsNullOrEmpty(Configuration.Settings.Language.Settings.GoBack1Frame))
                videoNode.Nodes.Add(Configuration.Settings.Language.Settings.GoBack1Frame + GetShortcutText(Configuration.Settings.Shortcuts.MainVideo1FrameLeft));
            if (!string.IsNullOrEmpty(Configuration.Settings.Language.Settings.GoForward1Frame))
                videoNode.Nodes.Add(Configuration.Settings.Language.Settings.GoForward1Frame + GetShortcutText(Configuration.Settings.Shortcuts.MainVideo1FrameRight));
            videoNode.Nodes.Add(Configuration.Settings.Language.Settings.GoBack100Milliseconds + GetShortcutText(Configuration.Settings.Shortcuts.MainVideo100MsLeft));
            videoNode.Nodes.Add(Configuration.Settings.Language.Settings.GoForward100Milliseconds + GetShortcutText(Configuration.Settings.Shortcuts.MainVideo100MsRight));
            videoNode.Nodes.Add(Configuration.Settings.Language.Settings.GoBack500Milliseconds + GetShortcutText(Configuration.Settings.Shortcuts.MainVideo500MsLeft));
            videoNode.Nodes.Add(Configuration.Settings.Language.Settings.GoForward500Milliseconds + GetShortcutText(Configuration.Settings.Shortcuts.MainVideo500MsRight));
            if (!string.IsNullOrEmpty(Configuration.Settings.Language.Settings.GoBack1Second))
                videoNode.Nodes.Add(Configuration.Settings.Language.Settings.GoBack1Second + GetShortcutText(Configuration.Settings.Shortcuts.MainVideo1000MsLeft));
            if (!string.IsNullOrEmpty(Configuration.Settings.Language.Settings.GoForward1Second))
                videoNode.Nodes.Add(Configuration.Settings.Language.Settings.GoForward1Second + GetShortcutText(Configuration.Settings.Shortcuts.MainVideo1000MsRight));
            videoNode.Nodes.Add(Configuration.Settings.Language.Settings.Fullscreen + GetShortcutText(Configuration.Settings.Shortcuts.MainVideoFullscreen));
            treeViewShortcuts.Nodes.Add(videoNode);

            var spellCheckNode = new TreeNode(Configuration.Settings.Language.Main.Menu.SpellCheck.Title);
            spellCheckNode.Nodes.Add(Configuration.Settings.Language.Main.Menu.SpellCheck.Title + GetShortcutText(Configuration.Settings.Shortcuts.MainSpellCheck));
            spellCheckNode.Nodes.Add(Configuration.Settings.Language.Main.Menu.SpellCheck.FindDoubleWords + GetShortcutText(Configuration.Settings.Shortcuts.MainSpellCheckFindDoubleWords));
            spellCheckNode.Nodes.Add(Configuration.Settings.Language.Main.Menu.SpellCheck.AddToNamesEtcList + GetShortcutText(Configuration.Settings.Shortcuts.MainSpellCheckAddWordToNames));
            treeViewShortcuts.Nodes.Add(spellCheckNode);

            var syncNode = new TreeNode(Configuration.Settings.Language.Main.Menu.Synchronization.Title);
            syncNode.Nodes.Add(Configuration.Settings.Language.Main.Menu.Synchronization.AdjustAllTimes + GetShortcutText(Configuration.Settings.Shortcuts.MainSynchronizationAdjustTimes));
            syncNode.Nodes.Add(Configuration.Settings.Language.Main.Menu.Synchronization.VisualSync + GetShortcutText(Configuration.Settings.Shortcuts.MainSynchronizationVisualSync));
            syncNode.Nodes.Add(Configuration.Settings.Language.Main.Menu.Synchronization.PointSync + GetShortcutText(Configuration.Settings.Shortcuts.MainSynchronizationPointSync));
            syncNode.Nodes.Add(Configuration.Settings.Language.Main.Menu.Tools.ChangeFrameRate + GetShortcutText(Configuration.Settings.Shortcuts.MainSynchronizationChangeFrameRate));
            treeViewShortcuts.Nodes.Add(syncNode);

            var listViewNode = new TreeNode(Configuration.Settings.Language.Main.Controls.ListView);
            listViewNode.Nodes.Add(Configuration.Settings.Language.General.Italic + GetShortcutText(Configuration.Settings.Shortcuts.MainListViewItalic));
            listViewNode.Nodes.Add(Configuration.Settings.Language.Main.Menu.ContextMenu.InsertAfter + GetShortcutText(Configuration.Settings.Shortcuts.MainInsertAfter));
            listViewNode.Nodes.Add(Configuration.Settings.Language.Main.Menu.ContextMenu.InsertBefore + GetShortcutText(Configuration.Settings.Shortcuts.MainInsertBefore));
            listViewNode.Nodes.Add(Configuration.Settings.Language.Settings.MergeDialogue + GetShortcutText(Configuration.Settings.Shortcuts.MainMergeDialogue));
            listViewNode.Nodes.Add(Configuration.Settings.Language.Settings.ToggleFocus + GetShortcutText(Configuration.Settings.Shortcuts.MainToogleFocus));
            listViewNode.Nodes.Add(Configuration.Settings.Language.Settings.ToggleDialogueDashes + GetShortcutText(Configuration.Settings.Shortcuts.MainListViewToggleDashes));
            listViewNode.Nodes.Add(Configuration.Settings.Language.Settings.Alignment + GetShortcutText(Configuration.Settings.Shortcuts.MainListViewAlignment));
            listViewNode.Nodes.Add(Configuration.Settings.Language.Settings.CopyTextOnly + GetShortcutText(Configuration.Settings.Shortcuts.MainListViewCopyText));
            listViewNode.Nodes.Add(Configuration.Settings.Language.Settings.AutoDurationSelectedLines + GetShortcutText(Configuration.Settings.Shortcuts.MainListViewAutoDuration));
            listViewNode.Nodes.Add(Configuration.Settings.Language.Settings.ListViewColumnDelete + GetShortcutText(Configuration.Settings.Shortcuts.MainListViewColumnDeleteText));
            listViewNode.Nodes.Add(Configuration.Settings.Language.Settings.ListViewColumnInsert + GetShortcutText(Configuration.Settings.Shortcuts.MainListViewColumnInsertText));
            listViewNode.Nodes.Add(Configuration.Settings.Language.Settings.ListViewColumnPaste + GetShortcutText(Configuration.Settings.Shortcuts.MainListViewColumnPaste));
            if (!string.IsNullOrEmpty(Configuration.Settings.Language.Settings.ListViewFocusWaveform))
                listViewNode.Nodes.Add(Configuration.Settings.Language.Settings.ListViewFocusWaveform + GetShortcutText(Configuration.Settings.Shortcuts.MainListViewFocusWaveform));
            if (!string.IsNullOrEmpty(Configuration.Settings.Language.Settings.ListViewGoToNextError))
                listViewNode.Nodes.Add(Configuration.Settings.Language.Settings.ListViewGoToNextError + GetShortcutText(Configuration.Settings.Shortcuts.MainListViewGoToNextError));
            treeViewShortcuts.Nodes.Add(listViewNode);

            var textBoxNode = new TreeNode(Configuration.Settings.Language.Settings.TextBox);
            textBoxNode.Nodes.Add(Configuration.Settings.Language.General.Italic + GetShortcutText(Configuration.Settings.Shortcuts.MainTextBoxItalic));
            textBoxNode.Nodes.Add(Configuration.Settings.Language.Main.Menu.ContextMenu.SplitLineAtCursorPosition + GetShortcutText(Configuration.Settings.Shortcuts.MainTextBoxSplitAtCursor));
            if (!string.IsNullOrEmpty(Configuration.Settings.Language.Settings.MainTextBoxMoveLastWordDown))
                textBoxNode.Nodes.Add(Configuration.Settings.Language.Settings.MainTextBoxMoveLastWordDown + GetShortcutText(Configuration.Settings.Shortcuts.MainTextBoxMoveLastWordDown));
            if (!string.IsNullOrEmpty(Configuration.Settings.Language.Settings.MainTextBoxMoveFirstWordFromNextUp))
                textBoxNode.Nodes.Add(Configuration.Settings.Language.Settings.MainTextBoxMoveFirstWordFromNextUp + GetShortcutText(Configuration.Settings.Shortcuts.MainTextBoxMoveFirstWordFromNextUp));
            if (!string.IsNullOrEmpty(Configuration.Settings.Language.Settings.MainTextBoxSelectionToLower))
                textBoxNode.Nodes.Add(Configuration.Settings.Language.Settings.MainTextBoxSelectionToLower + GetShortcutText(Configuration.Settings.Shortcuts.MainTextBoxSelectionToLower));
            if (!string.IsNullOrEmpty(Configuration.Settings.Language.Settings.MainTextBoxSelectionToUpper))
                textBoxNode.Nodes.Add(Configuration.Settings.Language.Settings.MainTextBoxSelectionToUpper + GetShortcutText(Configuration.Settings.Shortcuts.MainTextBoxSelectionToUpper));
            textBoxNode.Nodes.Add(Configuration.Settings.Language.Main.Menu.ContextMenu.InsertAfter + GetShortcutText(Configuration.Settings.Shortcuts.MainTextBoxInsertAfter));
            if (!string.IsNullOrEmpty(Configuration.Settings.Language.Settings.MainTextBoxAutoBreak))
                textBoxNode.Nodes.Add(Configuration.Settings.Language.Settings.MainTextBoxAutoBreak + GetShortcutText(Configuration.Settings.Shortcuts.MainTextBoxAutoBreak));
            if (!string.IsNullOrEmpty(Configuration.Settings.Language.Settings.MainTextBoxUnbreak))
                textBoxNode.Nodes.Add(Configuration.Settings.Language.Settings.MainTextBoxUnbreak + GetShortcutText(Configuration.Settings.Shortcuts.MainTextBoxUnbreak));
            treeViewShortcuts.Nodes.Add(textBoxNode);

            var createNode = new TreeNode(Configuration.Settings.Language.Main.VideoControls.Create);
            createNode.Nodes.Add(Configuration.Settings.Language.Main.VideoControls.InsertNewSubtitleAtVideoPosition + GetShortcutText(Configuration.Settings.Shortcuts.MainCreateInsertSubAtVideoPos));
            createNode.Nodes.Add(Configuration.Settings.Language.Main.VideoControls.PlayFromJustBeforeText + GetShortcutText(Configuration.Settings.Shortcuts.MainCreatePlayFromJustBefore));
            createNode.Nodes.Add(Configuration.Settings.Language.Main.VideoControls.SetStartTime + GetShortcutText(Configuration.Settings.Shortcuts.MainCreateSetStart));
            createNode.Nodes.Add(Configuration.Settings.Language.Main.VideoControls.SetEndTime + GetShortcutText(Configuration.Settings.Shortcuts.MainCreateSetEnd));
            createNode.Nodes.Add(Configuration.Settings.Language.Settings.MainCreateStartDownEndUp + GetShortcutText(Configuration.Settings.Shortcuts.MainCreateStartDownEndUp));
            createNode.Nodes.Add(Configuration.Settings.Language.Settings.CreateSetEndAddNewAndGoToNew + GetShortcutText(Configuration.Settings.Shortcuts.MainCreateSetEndAddNewAndGoToNew));
            treeViewShortcuts.Nodes.Add(createNode);

            var translateNote = new TreeNode(Configuration.Settings.Language.Main.VideoControls.Translate);
            translateNote.Nodes.Add(Configuration.Settings.Language.Settings.CustomSearch1 + GetShortcutText(Configuration.Settings.Shortcuts.MainTranslateCustomSearch1));
            translateNote.Nodes.Add(Configuration.Settings.Language.Settings.CustomSearch2 + GetShortcutText(Configuration.Settings.Shortcuts.MainTranslateCustomSearch2));
            translateNote.Nodes.Add(Configuration.Settings.Language.Settings.CustomSearch3 + GetShortcutText(Configuration.Settings.Shortcuts.MainTranslateCustomSearch3));
            translateNote.Nodes.Add(Configuration.Settings.Language.Settings.CustomSearch4 + GetShortcutText(Configuration.Settings.Shortcuts.MainTranslateCustomSearch4));
            translateNote.Nodes.Add(Configuration.Settings.Language.Settings.CustomSearch5 + GetShortcutText(Configuration.Settings.Shortcuts.MainTranslateCustomSearch5));
            translateNote.Nodes.Add(Configuration.Settings.Language.Settings.CustomSearch6 + GetShortcutText(Configuration.Settings.Shortcuts.MainTranslateCustomSearch6));
            treeViewShortcuts.Nodes.Add(translateNote);

            var adjustNode = new TreeNode(Configuration.Settings.Language.Main.VideoControls.Adjust);
            adjustNode.Nodes.Add(Configuration.Settings.Language.Main.VideoControls.SetstartTimeAndOffsetOfRest + GetShortcutText(Configuration.Settings.Shortcuts.MainAdjustSetStartAndOffsetTheRest));
            adjustNode.Nodes.Add(Configuration.Settings.Language.Main.VideoControls.SetEndTimeAndGoToNext + GetShortcutText(Configuration.Settings.Shortcuts.MainAdjustSetEndAndGotoNext));
            adjustNode.Nodes.Add(Configuration.Settings.Language.Settings.AdjustViaEndAutoStartAndGoToNext + GetShortcutText(Configuration.Settings.Shortcuts.MainAdjustViaEndAutoStartAndGoToNext));
            adjustNode.Nodes.Add(Configuration.Settings.Language.Settings.AdjustSetStartAutoDurationAndGoToNext + GetShortcutText(Configuration.Settings.Shortcuts.MainAdjustSetStartAutoDurationAndGoToNext));
            adjustNode.Nodes.Add(Configuration.Settings.Language.Settings.AdjustSetEndNextStartAndGoToNext + GetShortcutText(Configuration.Settings.Shortcuts.MainAdjustSetEndNextStartAndGoToNext));
            adjustNode.Nodes.Add(Configuration.Settings.Language.Settings.AdjustStartDownEndUpAndGoToNext + GetShortcutText(Configuration.Settings.Shortcuts.MainAdjustStartDownEndUpAndGoToNext));
            adjustNode.Nodes.Add(Configuration.Settings.Language.Main.VideoControls.SetStartTime + GetShortcutText(Configuration.Settings.Shortcuts.MainAdjustSetStart));
            if (!string.IsNullOrEmpty(Configuration.Settings.Language.Settings.AdjustSetStartTimeKeepDuration))
                adjustNode.Nodes.Add(Configuration.Settings.Language.Settings.AdjustSetStartTimeKeepDuration + GetShortcutText(Configuration.Settings.Shortcuts.MainAdjustSetStartKeepDuration));
            adjustNode.Nodes.Add(Configuration.Settings.Language.Main.VideoControls.SetEndTime + GetShortcutText(Configuration.Settings.Shortcuts.MainAdjustSetEnd));
            adjustNode.Nodes.Add(Configuration.Settings.Language.Settings.AdjustSelected100MsForward + GetShortcutText(Configuration.Settings.Shortcuts.MainAdjustSelected100MsForward));
            adjustNode.Nodes.Add(Configuration.Settings.Language.Settings.AdjustSelected100MsBack + GetShortcutText(Configuration.Settings.Shortcuts.MainAdjustSelected100MsBack));
            if (!string.IsNullOrEmpty(Configuration.Settings.Language.Settings.AdjustSetEndAndOffsetTheRest))
                adjustNode.Nodes.Add(Configuration.Settings.Language.Settings.AdjustSetEndAndOffsetTheRest + GetShortcutText(Configuration.Settings.Shortcuts.MainAdjustSetEndAndOffsetTheRest));
            if (!string.IsNullOrEmpty(Configuration.Settings.Language.Settings.AdjustSetEndAndOffsetTheRestAndGoToNext))
                adjustNode.Nodes.Add(Configuration.Settings.Language.Settings.AdjustSetEndAndOffsetTheRestAndGoToNext + GetShortcutText(Configuration.Settings.Shortcuts.MainAdjustSetEndAndOffsetTheRestAndGoToNext));
            treeViewShortcuts.Nodes.Add(adjustNode);

            var audioVisualizerNode = new TreeNode(Configuration.Settings.Language.Settings.WaveformAndSpectrogram);
            audioVisualizerNode.Nodes.Add(Configuration.Settings.Language.Waveform.ZoomIn + GetShortcutText(Configuration.Settings.Shortcuts.WaveformZoomIn));
            audioVisualizerNode.Nodes.Add(Configuration.Settings.Language.Waveform.ZoomOut + GetShortcutText(Configuration.Settings.Shortcuts.WaveformZoomOut));
            audioVisualizerNode.Nodes.Add(Configuration.Settings.Language.Settings.VerticalZoom + GetShortcutText(Configuration.Settings.Shortcuts.WaveformVerticalZoom));
            if (!string.IsNullOrEmpty(Configuration.Settings.Language.Settings.VerticalZoomOut))
                audioVisualizerNode.Nodes.Add(Configuration.Settings.Language.Settings.VerticalZoomOut + GetShortcutText(Configuration.Settings.Shortcuts.WaveformVerticalZoomOut));
            audioVisualizerNode.Nodes.Add(Configuration.Settings.Language.Settings.WaveformSeekSilenceForward + GetShortcutText(Configuration.Settings.Shortcuts.WaveformSearchSilenceForward));
            audioVisualizerNode.Nodes.Add(Configuration.Settings.Language.Settings.WaveformSeekSilenceBack + GetShortcutText(Configuration.Settings.Shortcuts.WaveformSearchSilenceBack));
            audioVisualizerNode.Nodes.Add(Configuration.Settings.Language.Settings.WaveformAddTextHere + GetShortcutText(Configuration.Settings.Shortcuts.WaveformAddTextHere));
            if (!string.IsNullOrEmpty(Configuration.Settings.Language.Settings.WaveformPlayNewSelection))
                audioVisualizerNode.Nodes.Add(Configuration.Settings.Language.Settings.WaveformPlayNewSelection + GetShortcutText(Configuration.Settings.Shortcuts.WaveformPlaySelection));
            audioVisualizerNode.Nodes.Add(Configuration.Settings.Language.Main.VideoControls.InsertNewSubtitleAtVideoPosition + GetShortcutText(Configuration.Settings.Shortcuts.MainWaveformInsertAtCurrentPosition));
            if (!string.IsNullOrEmpty(Configuration.Settings.Language.Settings.WaveformFocusListView))
                audioVisualizerNode.Nodes.Add(Configuration.Settings.Language.Settings.WaveformFocusListView + GetShortcutText(Configuration.Settings.Shortcuts.WaveformFocusListView));
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
            checkBoxSyntaxColorTextMoreThanTwoLines.Text = language.SyntaxColorTextMoreThanXLines;
            numericUpDownSyntaxColorTextMoreThanXLines.Left = checkBoxSyntaxColorTextMoreThanTwoLines.Left + checkBoxSyntaxColorTextMoreThanTwoLines.Width + 4;
            checkBoxSyntaxOverlap.Text = language.SyntaxColorOverlap;
            buttonListViewSyntaxColorError.Text = language.SyntaxErrorColor;

            FixLargeFonts();

            checkBoxShortcutsControl.Left = labelShortcut.Left + labelShortcut.Width + 9;
            checkBoxShortcutsAlt.Left = checkBoxShortcutsControl.Left + checkBoxShortcutsControl.Width + 9;
            checkBoxShortcutsShift.Left = checkBoxShortcutsAlt.Left + checkBoxShortcutsAlt.Width + 9;
            labelShortcutKey.Left = checkBoxShortcutsShift.Left + checkBoxShortcutsShift.Width + 9;
            comboBoxShortcutKey.Left = labelShortcutKey.Left + labelShortcutKey.Width + 2;
            buttonUpdateShortcut.Left = comboBoxShortcutKey.Left + comboBoxShortcutKey.Width + 15;

            _oldVlcLocation = Configuration.Settings.General.VlcLocation;
            _oldVlcLocationRelative = Configuration.Settings.General.VlcLocationRelative;

            labelPlatform.Text = (IntPtr.Size * 8) + "-bit";
        }

        private static string GetShortcutText(string shortcut)
        {
            if (string.IsNullOrEmpty(shortcut))
                shortcut = Configuration.Settings.Language.General.None;
            return string.Format(" [{0}]", shortcut);
        }

        private void FixLargeFonts()
        {
            var graphics = CreateGraphics();
            var textSize = graphics.MeasureString(buttonOK.Text, Font);
            if (textSize.Height > buttonOK.Height - 4)
            {
                int newButtonHeight = (int)(textSize.Height + 7 + 0.5);
                Utilities.SetButtonHeight(this, newButtonHeight, 1);
                Utilities.SetButtonHeight(groupBoxSsaStyle, newButtonHeight, 2);
                Utilities.SetButtonHeight(groupBoxWaveformAppearence, newButtonHeight, 1);
            }
        }

        private void InitializeWaveformsAndSpectrogramsFolderEmpty(LanguageStructure.Settings language)
        {
            string waveformsFolder = Configuration.WaveformsFolder.TrimEnd(Path.DirectorySeparatorChar);
            string spectrogramsFolder = Configuration.SpectrogramsFolder.TrimEnd(Path.DirectorySeparatorChar);
            long bytes = 0;
            int count = 0;

            if (Directory.Exists(waveformsFolder))
            {
                DirectoryInfo di = new DirectoryInfo(waveformsFolder);

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
                DirectoryInfo di = new DirectoryInfo(spectrogramsFolder);

                // spectrogram data
                foreach (DirectoryInfo dir in di.GetDirectories())
                {
                    DirectoryInfo spectrogramDir = new DirectoryInfo(dir.FullName);
                    foreach (FileInfo fi in spectrogramDir.GetFiles("*.gif"))
                    {
                        bytes += fi.Length;
                        count++;
                    }
                    foreach (FileInfo fi in spectrogramDir.GetFiles("*.db"))
                    {
                        bytes += fi.Length;
                        count++;
                    }
                    string xmlFileName = Path.Combine(dir.FullName, "Info.xml");
                    if (File.Exists(xmlFileName))
                    {
                        FileInfo fi = new FileInfo(xmlFileName);
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

        public void Initialize(Icon icon, Image newFile, Image openFile, Image saveFile, Image saveFileAs, Image find, Image replace, Image fixCommonErrors,
                               Image visualSync, Image spellCheck, Image settings, Image help)
        {
            Icon = (Icon)icon.Clone();
            pictureBoxNew.Image = (Image)newFile.Clone();
            pictureBoxOpen.Image = (Image)openFile.Clone();
            pictureBoxSave.Image = (Image)saveFile.Clone();
            pictureBoxSaveAs.Image = (Image)saveFileAs.Clone();
            pictureBoxFind.Image = (Image)find.Clone();
            pictureBoxReplace.Image = (Image)replace.Clone();
            pictureBoxTBFixCommonErrors.Image = (Image)fixCommonErrors.Clone();
            pictureBoxVisualSync.Image = (Image)visualSync.Clone();
            pictureBoxSpellCheck.Image = (Image)spellCheck.Clone();
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
                        if (Directory.GetFiles(dir, name.Replace("-", "_") + "_user.xml").Length == 1)
                        {
                            if (!cultures.Contains(culture))
                                cultures.Add(culture);
                        }

                        if (Directory.GetFiles(dir, name.Replace("-", "_") + "_names_etc.xml").Length == 1)
                        {
                            if (!cultures.Contains(culture))
                                cultures.Add(culture);
                        }

                        if (Directory.GetFiles(dir, culture.ThreeLetterISOLanguageName + "_OCRFixReplaceList.xml").Length == 1)
                        {
                            bool found = false;
                            foreach (CultureInfo ci in cultures)
                            {
                                if (ci.ThreeLetterISOLanguageName == culture.ThreeLetterISOLanguageName)
                                    found = true;
                            }
                            if (!found)
                                cultures.Add(culture);
                        }
                        else if (Directory.GetFiles(dir, culture.ThreeLetterISOLanguageName + "_OCRFixReplaceList_User.xml").Length == 1)
                        {
                            bool found = false;
                            foreach (CultureInfo ci in cultures)
                            {
                                if (ci.ThreeLetterISOLanguageName == culture.ThreeLetterISOLanguageName)
                                    found = true;
                            }
                            if (!found)
                                cultures.Add(culture);
                        }
                    }

                }

                foreach (var culture in CultureInfo.GetCultures(CultureTypes.NeutralCultures))
                {
                    if (Directory.GetFiles(dir, culture.ThreeLetterISOLanguageName + "_OCRFixReplaceList.xml").Length == 1)
                    {
                        bool found = false;
                        foreach (CultureInfo ci in cultures)
                        {
                            if (ci.ThreeLetterISOLanguageName == culture.ThreeLetterISOLanguageName)
                                found = true;
                        }
                        if (!found)
                            cultures.Add(culture);
                    }
                    else if (Directory.GetFiles(dir, culture.ThreeLetterISOLanguageName + "_OCRFixReplaceList_User.xml").Length == 1)
                    {
                        bool found = false;
                        foreach (CultureInfo ci in cultures)
                        {
                            if (ci.ThreeLetterISOLanguageName == culture.ThreeLetterISOLanguageName)
                                found = true;
                        }
                        if (!found)
                            cultures.Add(culture);
                    }
                }

                comboBoxWordListLanguage.Items.Clear();
                if (Configuration.Settings.WordLists.LastLanguage == null)
                    Configuration.Settings.WordLists.LastLanguage = "en-US";
                foreach (CultureInfo ci in cultures)
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
            GeneralSettings gs = Configuration.Settings.General;

            gs.ShowToolbarNew = checkBoxToolbarNew.Checked;
            gs.ShowToolbarOpen = checkBoxToolbarOpen.Checked;
            gs.ShowToolbarSave = checkBoxToolbarSave.Checked;
            gs.ShowToolbarSaveAs = checkBoxToolbarSaveAs.Checked;
            gs.ShowToolbarFind = checkBoxToolbarFind.Checked;
            gs.ShowToolbarReplace = checkBoxReplace.Checked;
            gs.ShowToolbarFixCommonErrors = checkBoxTBFixCommonErrors.Checked;
            gs.ShowToolbarVisualSync = checkBoxVisualSync.Checked;
            gs.ShowToolbarSettings = checkBoxSettings.Checked;
            gs.ShowToolbarSpellCheck = checkBoxSpellCheck.Checked;
            gs.ShowToolbarHelp = checkBoxHelp.Checked;

            gs.ShowFrameRate = checkBoxShowFrameRate.Checked;
            double outFrameRate;
            if (double.TryParse(comboBoxFrameRate.Text.Replace(",", "."), NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out outFrameRate))
                gs.DefaultFrameRate = outFrameRate;

            gs.DefaultEncoding = Encoding.UTF8.BodyName;
            foreach (EncodingInfo ei in Encoding.GetEncodings())
            {
                if (ei.CodePage + ": " + ei.DisplayName == comboBoxEncoding.Text)
                    gs.DefaultEncoding = comboBoxEncoding.Text;
            }

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
            gs.MininumMillisecondsBetweenLines = (int)numericUpDownMinGapMs.Value;

            if (comboBoxAutoBackup.SelectedIndex == 1)
                gs.AutoBackupSeconds = 60;
            else if (comboBoxAutoBackup.SelectedIndex == 2)
                gs.AutoBackupSeconds = 60 * 5;
            else if (comboBoxAutoBackup.SelectedIndex == 3)
                gs.AutoBackupSeconds = 60 * 15;
            else
                gs.AutoBackupSeconds = 0;

            Configuration.Settings.General.CheckForUpdates = checkBoxCheckForUpdates.Checked;

            if (comboBoxTimeCodeMode.Visible)
                Configuration.Settings.General.UseTimeFormatHHMMSSFF = comboBoxTimeCodeMode.SelectedIndex == 1;

            if (comboBoxSpellChecker.SelectedIndex == 1)
                gs.SpellChecker = "word";
            else
                gs.SpellChecker = "hunspell";

            gs.AllowEditOfOriginalSubtitle = checkBoxAllowEditOfOriginalSubtitle.Checked;
            gs.PromptDeleteLines = checkBoxPromptDeleteLines.Checked;

            if (radioButtonVideoPlayerMPlayer.Checked)
                gs.VideoPlayer = "MPlayer";
            //else if (radioButtonVideoPlayerManagedDirectX.Checked)
            //    gs.VideoPlayer = "ManagedDirectX";
            else if (radioButtonVideoPlayerMpcHc.Checked)
                gs.VideoPlayer = "MPC-HC";
            else if (radioButtonVideoPlayerVLC.Checked)
                gs.VideoPlayer = "VLC";
            else
                gs.VideoPlayer = "DirectShow";

            gs.VlcLocation = textBoxVlcPath.Text;

            gs.VideoPlayerShowStopButton = checkBoxVideoPlayerShowStopButton.Checked;
            gs.VideoPlayerShowMuteButton = checkBoxVideoPlayerShowMuteButton.Checked;
            gs.VideoPlayerShowFullscreenButton = checkBoxVideoPlayerShowFullscreenButton.Checked;
            gs.VideoPlayerPreviewFontSize = int.Parse(comboBoxlVideoPlayerPreviewFontSize.Items[0].ToString()) + comboBoxlVideoPlayerPreviewFontSize.SelectedIndex;
            gs.VideoPlayerPreviewFontBold = checkBoxVideoPlayerPreviewFontBold.Checked;

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

            gs.AutoWrapLineWhileTyping = checkBoxAutoWrapWhileTyping.Checked;

            if (comboBoxSubtitleFont.SelectedItem != null)
                gs.SubtitleFontName = comboBoxSubtitleFont.SelectedItem.ToString();

            ToolsSettings toolsSettings = Configuration.Settings.Tools;
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

            WordListSettings wordListSettings = Configuration.Settings.WordLists;
            wordListSettings.UseOnlineNamesEtc = checkBoxNamesEtcOnline.Checked;
            wordListSettings.NamesEtcUrl = textBoxNamesEtcOnline.Text;
            if (comboBoxWordListLanguage.Items.Count > 0 && comboBoxWordListLanguage.SelectedIndex >= 0)
            {
                var ci = comboBoxWordListLanguage.Items[comboBoxWordListLanguage.SelectedIndex] as ComboBoxLanguage;
                if (ci != null)
                    Configuration.Settings.WordLists.LastLanguage = ci.CultureInfo.Name;
            }

            SubtitleSettings ssa = Configuration.Settings.SubtitleSettings;
            ssa.SsaFontName = _ssaFontName;
            ssa.SsaFontSize = _ssaFontSize;
            ssa.SsaFontColorArgb = _ssaFontColor;
            ssa.SsaOutline = (int)numericUpDownSsaOutline.Value;
            ssa.SsaShadow = (int)numericUpDownSsaShadow.Value;
            ssa.SsaOpaqueBox = checkBoxSsaOpaqueBox.Checked;

            ProxySettings proxy = Configuration.Settings.Proxy;
            proxy.ProxyAddress = textBoxProxyAddress.Text;
            proxy.UserName = textBoxProxyUserName.Text;
            if (string.IsNullOrWhiteSpace(textBoxProxyPassword.Text))
                proxy.Password = null;
            else
                proxy.EncodePassword(textBoxProxyPassword.Text);
            proxy.Domain = textBoxProxyDomain.Text;

            Configuration.Settings.Tools.ListViewSyntaxColorDurationSmall = checkBoxSyntaxColorDurationTooSmall.Checked;
            Configuration.Settings.Tools.ListViewSyntaxColorDurationBig = checkBoxSyntaxColorDurationTooLarge.Checked;
            Configuration.Settings.Tools.ListViewSyntaxColorLongLines = checkBoxSyntaxColorTextTooLong.Checked;
            Configuration.Settings.Tools.ListViewSyntaxMoreThanXLines = checkBoxSyntaxColorTextMoreThanTwoLines.Checked;
            Configuration.Settings.Tools.ListViewSyntaxMoreThanXLinesX = (int)numericUpDownSyntaxColorTextMoreThanXLines.Value;
            Configuration.Settings.Tools.ListViewSyntaxColorOverlap = checkBoxSyntaxOverlap.Checked;
            Configuration.Settings.Tools.ListViewSyntaxErrorColor = panelListViewSyntaxColorError.BackColor;

            Configuration.Settings.VideoControls.WaveformDrawGrid = checkBoxWaveformShowGrid.Checked;
            Configuration.Settings.VideoControls.WaveformGridColor = panelWaveformGridColor.BackColor;
            Configuration.Settings.VideoControls.WaveformSelectedColor = panelWaveformSelectedColor.BackColor;
            Configuration.Settings.VideoControls.WaveformColor = panelWaveformColor.BackColor;
            Configuration.Settings.VideoControls.WaveformBackgroundColor = panelWaveformBackgroundColor.BackColor;
            Configuration.Settings.VideoControls.WaveformTextColor = panelWaveformTextColor.BackColor;
            Configuration.Settings.VideoControls.GenerateSpectrogram = checkBoxGenerateSpectrogram.Checked;
            if (comboBoxSpectrogramAppearance.SelectedIndex == 0)
                Configuration.Settings.VideoControls.SpectrogramAppearance = "OneColorGradient";
            else
                Configuration.Settings.VideoControls.SpectrogramAppearance = "Classic";
            Configuration.Settings.VideoControls.WaveformMouseWheelScrollUpIsForward = checkBoxReverseMouseWheelScrollDirection.Checked;
            Configuration.Settings.VideoControls.WaveformAllowOverlap = checkBoxAllowOverlap.Checked;
            Configuration.Settings.VideoControls.WaveformFocusOnMouseEnter = checkBoxWaveformHoverFocus.Checked;
            Configuration.Settings.VideoControls.WaveformListViewFocusOnMouseEnter = checkBoxListViewMouseEnterFocus.Checked;
            Configuration.Settings.VideoControls.WaveformBorderHitMs = Convert.ToInt32(numericUpDownWaveformBorderHitMs.Value);
            Configuration.Settings.General.UseFFmpegForWaveExtraction = checkBoxUseFFmpeg.Checked;
            Configuration.Settings.General.FFmpegLocation = textBoxFFmpegPath.Text;

            //Main General
            foreach (TreeNode node in treeViewShortcuts.Nodes[0].Nodes)
            {
                var indexOfBracket = node.Text.IndexOf('[');
                if (indexOfBracket >= 0)
                {
                    string text = node.Text.Substring(0, indexOfBracket).Trim();
                    if (text == Configuration.Settings.Language.Settings.GoToFirstSelectedLine.Replace("&", string.Empty))
                        Configuration.Settings.Shortcuts.GeneralGoToFirstSelectedLine = GetShortcut(node.Text);
                    else if (!string.IsNullOrEmpty(Configuration.Settings.Language.Settings.GoToNextEmptyLine) && text == (Configuration.Settings.Language.Settings.GoToNextEmptyLine).Replace("&", string.Empty))
                        Configuration.Settings.Shortcuts.GeneralGoToNextEmptyLine = GetShortcut(node.Text);
                    else if (text == Configuration.Settings.Language.Settings.MergeSelectedLines.Replace("&", string.Empty))
                        Configuration.Settings.Shortcuts.GeneralMergeSelectedLines = GetShortcut(node.Text);
                    else if (!string.IsNullOrEmpty(Configuration.Settings.Language.Settings.MergeSelectedLinesOnlyFirstText) && text == Configuration.Settings.Language.Settings.MergeSelectedLinesOnlyFirstText.Replace("&", string.Empty))
                        Configuration.Settings.Shortcuts.GeneralMergeSelectedLinesOnlyFirstText = GetShortcut(node.Text);
                    else if (text == Configuration.Settings.Language.Settings.ToggleTranslationMode.Replace("&", string.Empty))
                        Configuration.Settings.Shortcuts.GeneralToggleTranslationMode = GetShortcut(node.Text);
                    else if (text == Configuration.Settings.Language.Settings.SwitchOriginalAndTranslation.Replace("&", string.Empty))
                        Configuration.Settings.Shortcuts.GeneralSwitchOriginalAndTranslation = GetShortcut(node.Text);
                    else if (text == Configuration.Settings.Language.Settings.MergeOriginalAndTranslation.Replace("&", string.Empty))
                        Configuration.Settings.Shortcuts.GeneralMergeOriginalAndTranslation = GetShortcut(node.Text);
                    else if (text == Configuration.Settings.Language.Settings.GoToNext.Replace("&", string.Empty))
                        Configuration.Settings.Shortcuts.GeneralGoToNextSubtitle = GetShortcut(node.Text);
                    else if (text == Configuration.Settings.Language.Settings.GoToPrevious.Replace("&", string.Empty))
                        Configuration.Settings.Shortcuts.GeneralGoToPrevSubtitle = GetShortcut(node.Text);
                    else if (text == Configuration.Settings.Language.Settings.GoToCurrentSubtitleStart.Replace("&", string.Empty))
                        Configuration.Settings.Shortcuts.GeneralGoToStartOfCurrentSubtitle = GetShortcut(node.Text);
                    else if (text == Configuration.Settings.Language.Settings.GoToCurrentSubtitleEnd.Replace("&", string.Empty))
                        Configuration.Settings.Shortcuts.GeneralGoToEndOfCurrentSubtitle = GetShortcut(node.Text);
                    else if (!string.IsNullOrEmpty(Configuration.Settings.Language.Settings.WaveformPlayFirstSelectedSubtitle) && text == (Configuration.Settings.Language.Settings.WaveformPlayFirstSelectedSubtitle).Replace("&", string.Empty))
                        Configuration.Settings.Shortcuts.GeneralPlayFirstSelected = GetShortcut(node.Text);
                }
            }

            //Main File
            foreach (TreeNode node in treeViewShortcuts.Nodes[1].Nodes)
            {
                if (node.Text.Contains('['))
                {
                    string text = node.Text.Substring(0, node.Text.IndexOf('[')).Trim();
                    if (text == Configuration.Settings.Language.Main.Menu.File.New.Replace("&", string.Empty))
                        Configuration.Settings.Shortcuts.MainFileNew = GetShortcut(node.Text);
                    else if (text == Configuration.Settings.Language.Main.Menu.File.Open.Replace("&", string.Empty))
                        Configuration.Settings.Shortcuts.MainFileOpen = GetShortcut(node.Text);
                    else if (text == Configuration.Settings.Language.Main.Menu.File.OpenKeepVideo.Replace("&", string.Empty))
                        Configuration.Settings.Shortcuts.MainFileOpenKeepVideo = GetShortcut(node.Text);
                    else if (text == Configuration.Settings.Language.Main.Menu.File.Save.Replace("&", string.Empty))
                        Configuration.Settings.Shortcuts.MainFileSave = GetShortcut(node.Text);
                    else if (text == Configuration.Settings.Language.Main.Menu.File.SaveAs.Replace("&", string.Empty))
                        Configuration.Settings.Shortcuts.MainFileSaveAs = GetShortcut(node.Text);
                    else if (text == Configuration.Settings.Language.Main.Menu.File.SaveOriginal.Replace("&", string.Empty))
                        Configuration.Settings.Shortcuts.MainFileSaveOriginal = GetShortcut(node.Text);
                    else if (text == Configuration.Settings.Language.Main.SaveOriginalSubtitleAs.Replace("&", string.Empty))
                        Configuration.Settings.Shortcuts.MainFileSaveOriginalAs = GetShortcut(node.Text);
                    else if (!string.IsNullOrEmpty(Configuration.Settings.Language.Settings.MainFileSaveAll) && text == Configuration.Settings.Language.Settings.MainFileSaveAll.Replace("&", string.Empty))
                        Configuration.Settings.Shortcuts.MainFileSaveAll = GetShortcut(node.Text);
                    else if (text == (Configuration.Settings.Language.Main.Menu.File.Export + " -> " + Configuration.Settings.Language.Main.Menu.File.ExportEbu).Replace("&", string.Empty))
                        Configuration.Settings.Shortcuts.MainFileExportEbu = GetShortcut(node.Text);
                }
            }

            //Main Edit
            foreach (TreeNode node in treeViewShortcuts.Nodes[2].Nodes)
            {
                if (node.Text.Contains('['))
                {
                    string text = node.Text.Substring(0, node.Text.IndexOf('[')).Trim();
                    if (text == Configuration.Settings.Language.Main.Menu.Edit.Undo.Replace("&", string.Empty))
                        Configuration.Settings.Shortcuts.MainEditUndo = GetShortcut(node.Text);
                    else if (text == Configuration.Settings.Language.Main.Menu.Edit.Redo.Replace("&", string.Empty))
                        Configuration.Settings.Shortcuts.MainEditRedo = GetShortcut(node.Text);
                    else if (text == Configuration.Settings.Language.Main.Menu.Edit.Find.Replace("&", string.Empty))
                        Configuration.Settings.Shortcuts.MainEditFind = GetShortcut(node.Text);
                    else if (text == Configuration.Settings.Language.Main.Menu.Edit.FindNext.Replace("&", string.Empty))
                        Configuration.Settings.Shortcuts.MainEditFindNext = GetShortcut(node.Text);
                    else if (text == Configuration.Settings.Language.Main.Menu.Edit.Replace.Replace("&", string.Empty))
                        Configuration.Settings.Shortcuts.MainEditReplace = GetShortcut(node.Text);
                    else if (text == Configuration.Settings.Language.Main.Menu.Edit.MultipleReplace.Replace("&", string.Empty))
                        Configuration.Settings.Shortcuts.MainEditMultipleReplace = GetShortcut(node.Text);
                    else if (text == Configuration.Settings.Language.Main.Menu.Edit.GoToSubtitleNumber.Replace("&", string.Empty))
                        Configuration.Settings.Shortcuts.MainEditGoToLineNumber = GetShortcut(node.Text);
                    else if (text == Configuration.Settings.Language.VobSubOcr.RightToLeft.Replace("&", string.Empty))
                        Configuration.Settings.Shortcuts.MainEditRightToLeft = GetShortcut(node.Text);
                    else if (Configuration.Settings.Language.Settings.ReverseStartAndEndingForRTL != null && text == Configuration.Settings.Language.Settings.ReverseStartAndEndingForRTL.Replace("&", string.Empty))
                        Configuration.Settings.Shortcuts.MainEditReverseStartAndEndingForRTL = GetShortcut(node.Text);
                    else if (text == Configuration.Settings.Language.Settings.ToggleTranslationAndOriginalInPreviews.Replace("&", string.Empty))
                        Configuration.Settings.Shortcuts.MainEditToggleTranslationOriginalInPreviews = GetShortcut(node.Text);
                }
            }

            //Main Tools
            foreach (TreeNode node in treeViewShortcuts.Nodes[3].Nodes)
            {
                if (node.Text.Contains('['))
                {
                    string text = node.Text.Substring(0, node.Text.IndexOf('[')).Trim();
                    if (text == Configuration.Settings.Language.Main.Menu.Tools.FixCommonErrors.Replace("&", string.Empty))
                        Configuration.Settings.Shortcuts.MainToolsFixCommonErrors = GetShortcut(node.Text);
                    else if (text == Configuration.Settings.Language.Main.Menu.Tools.StartNumberingFrom.Replace("&", string.Empty))
                        Configuration.Settings.Shortcuts.MainToolsRenumber = GetShortcut(node.Text);
                    else if (text == Configuration.Settings.Language.Main.Menu.Tools.RemoveTextForHearingImpaired.Replace("&", string.Empty))
                        Configuration.Settings.Shortcuts.MainToolsRemoveTextForHI = GetShortcut(node.Text);
                    else if (text == Configuration.Settings.Language.Main.Menu.Tools.ChangeCasing.Replace("&", string.Empty))
                        Configuration.Settings.Shortcuts.MainToolsChangeCasing = GetShortcut(node.Text);
                    else if (text == Configuration.Settings.Language.Main.Menu.Tools.SplitLongLines.Replace("&", string.Empty))
                        Configuration.Settings.Shortcuts.MainToolsSplitLongLines = GetShortcut(node.Text);
                    else if (text == Configuration.Settings.Language.Main.Menu.Tools.MergeShortLines.Replace("&", string.Empty))
                        Configuration.Settings.Shortcuts.MainToolsMergeShortLines = GetShortcut(node.Text);
                    else if (text == Configuration.Settings.Language.Main.Menu.ContextMenu.AutoDurationCurrentLine.Replace("&", string.Empty))
                        Configuration.Settings.Shortcuts.MainToolsAutoDuration = GetShortcut(node.Text);
                    else if (text == Configuration.Settings.Language.Settings.ShowBeamer.Replace("&", string.Empty))
                        Configuration.Settings.Shortcuts.MainToolsBeamer = GetShortcut(node.Text);
                }
            }

            //Main Video
            foreach (TreeNode node in treeViewShortcuts.Nodes[4].Nodes)
            {
                if (node.Text.Contains('['))
                {
                    string text = node.Text.Substring(0, node.Text.IndexOf('[')).Trim();
                    if (text == Configuration.Settings.Language.Main.Menu.Video.ShowHideVideo.Replace("&", string.Empty))
                        Configuration.Settings.Shortcuts.MainVideoShowHideVideo = GetShortcut(node.Text);
                    else if (text == Configuration.Settings.Language.Settings.ToggleDockUndockOfVideoControls.Replace("&", string.Empty))
                        Configuration.Settings.Shortcuts.MainVideoToggleVideoControls = GetShortcut(node.Text);
                    else if (!string.IsNullOrEmpty(Configuration.Settings.Language.Settings.GoBack1Frame) && text == Configuration.Settings.Language.Settings.GoBack1Frame.Replace("&", string.Empty))
                        Configuration.Settings.Shortcuts.MainVideo1FrameLeft = GetShortcut(node.Text);
                    else if (!string.IsNullOrEmpty(Configuration.Settings.Language.Settings.GoForward1Frame) && text == Configuration.Settings.Language.Settings.GoForward1Frame.Replace("&", string.Empty))
                        Configuration.Settings.Shortcuts.MainVideo1FrameRight = GetShortcut(node.Text);
                    else if (text == Configuration.Settings.Language.Settings.GoBack100Milliseconds.Replace("&", string.Empty))
                        Configuration.Settings.Shortcuts.MainVideo100MsLeft = GetShortcut(node.Text);
                    else if (text == Configuration.Settings.Language.Settings.GoForward100Milliseconds.Replace("&", string.Empty))
                        Configuration.Settings.Shortcuts.MainVideo100MsRight = GetShortcut(node.Text);
                    else if (text == Configuration.Settings.Language.Settings.GoBack500Milliseconds.Replace("&", string.Empty))
                        Configuration.Settings.Shortcuts.MainVideo500MsLeft = GetShortcut(node.Text);
                    else if (text == Configuration.Settings.Language.Settings.GoForward500Milliseconds.Replace("&", string.Empty))
                        Configuration.Settings.Shortcuts.MainVideo500MsRight = GetShortcut(node.Text);
                    else if (!string.IsNullOrEmpty(Configuration.Settings.Language.Settings.GoBack1Second) && text == Configuration.Settings.Language.Settings.GoBack1Second.Replace("&", string.Empty))
                        Configuration.Settings.Shortcuts.MainVideo1000MsLeft = GetShortcut(node.Text);
                    else if (!string.IsNullOrEmpty(Configuration.Settings.Language.Settings.GoForward1Second) && text == Configuration.Settings.Language.Settings.GoForward1Second.Replace("&", string.Empty))
                        Configuration.Settings.Shortcuts.MainVideo1000MsRight = GetShortcut(node.Text);
                    else if (text == Configuration.Settings.Language.Settings.Fullscreen.Replace("&", string.Empty))
                        Configuration.Settings.Shortcuts.MainVideoFullscreen = GetShortcut(node.Text);
                    else if (text == Configuration.Settings.Language.Settings.Pause.Replace("&", string.Empty))
                        Configuration.Settings.Shortcuts.MainVideoPause = GetShortcut(node.Text);
                    else if (text == Configuration.Settings.Language.Settings.TogglePlayPause.Replace("&", string.Empty))
                        Configuration.Settings.Shortcuts.MainVideoPlayPauseToggle = GetShortcut(node.Text);
                }
            }

            //Main Spell check
            foreach (TreeNode node in treeViewShortcuts.Nodes[5].Nodes)
            {
                if (node.Text.Contains('['))
                {
                    string text = node.Text.Substring(0, node.Text.IndexOf('[')).Trim();
                    if (text == Configuration.Settings.Language.Main.Menu.SpellCheck.Title.Replace("&", string.Empty))
                        Configuration.Settings.Shortcuts.MainSpellCheck = GetShortcut(node.Text);
                    else if (text == Configuration.Settings.Language.Main.Menu.SpellCheck.FindDoubleWords.Replace("&", string.Empty))
                        Configuration.Settings.Shortcuts.MainSpellCheckFindDoubleWords = GetShortcut(node.Text);
                    else if (text == Configuration.Settings.Language.Main.Menu.SpellCheck.AddToNamesEtcList.Replace("&", string.Empty))
                        Configuration.Settings.Shortcuts.MainSpellCheckAddWordToNames = GetShortcut(node.Text);
                }
            }

            //Main Sync
            foreach (TreeNode node in treeViewShortcuts.Nodes[6].Nodes)
            {
                if (node.Text.Contains('['))
                {
                    string text = node.Text.Substring(0, node.Text.IndexOf('[')).Trim();
                    if (text == Configuration.Settings.Language.Main.Menu.Synchronization.AdjustAllTimes.Replace("&", string.Empty))
                        Configuration.Settings.Shortcuts.MainSynchronizationAdjustTimes = GetShortcut(node.Text);
                    else if (text == Configuration.Settings.Language.Main.Menu.Synchronization.VisualSync.Replace("&", string.Empty))
                        Configuration.Settings.Shortcuts.MainSynchronizationVisualSync = GetShortcut(node.Text);
                    else if (text == Configuration.Settings.Language.Main.Menu.Synchronization.PointSync.Replace("&", string.Empty))
                        Configuration.Settings.Shortcuts.MainSynchronizationPointSync = GetShortcut(node.Text);
                    else if (text == Configuration.Settings.Language.Main.Menu.Tools.ChangeFrameRate.Replace("&", string.Empty))
                        Configuration.Settings.Shortcuts.MainSynchronizationChangeFrameRate = GetShortcut(node.Text);
                }
            }

            //Main List view
            foreach (TreeNode node in treeViewShortcuts.Nodes[7].Nodes)
            {
                if (node.Text.Contains('['))
                {
                    string text = node.Text.Substring(0, node.Text.IndexOf('[')).Trim();
                    if (text == Configuration.Settings.Language.General.Italic.Replace("&", string.Empty))
                        Configuration.Settings.Shortcuts.MainListViewItalic = GetShortcut(node.Text);
                    else if (text == Configuration.Settings.Language.Main.Menu.ContextMenu.InsertAfter.Replace("&", string.Empty))
                        Configuration.Settings.Shortcuts.MainInsertAfter = GetShortcut(node.Text);
                    else if (text == Configuration.Settings.Language.Main.Menu.ContextMenu.InsertBefore.Replace("&", string.Empty))
                        Configuration.Settings.Shortcuts.MainInsertBefore = GetShortcut(node.Text);
                    else if (text == Configuration.Settings.Language.Settings.MergeDialogue.Replace("&", string.Empty))
                        Configuration.Settings.Shortcuts.MainMergeDialogue = GetShortcut(node.Text);
                    else if (text == Configuration.Settings.Language.Settings.ToggleFocus.Replace("&", string.Empty))
                        Configuration.Settings.Shortcuts.MainToogleFocus = GetShortcut(node.Text);
                    else if (text == Configuration.Settings.Language.Settings.ToggleDialogueDashes.Replace("&", string.Empty))
                        Configuration.Settings.Shortcuts.MainListViewToggleDashes = GetShortcut(node.Text);
                    else if (text == Configuration.Settings.Language.Settings.Alignment.Replace("&", string.Empty))
                        Configuration.Settings.Shortcuts.MainListViewAlignment = GetShortcut(node.Text);
                    else if (text == Configuration.Settings.Language.Settings.CopyTextOnly.Replace("&", string.Empty))
                        Configuration.Settings.Shortcuts.MainListViewCopyText = GetShortcut(node.Text);
                    else if (text == Configuration.Settings.Language.Settings.AutoDurationSelectedLines.Replace("&", string.Empty))
                        Configuration.Settings.Shortcuts.MainListViewAutoDuration = GetShortcut(node.Text);
                    else if (text == Configuration.Settings.Language.Settings.ListViewColumnDelete.Replace("&", string.Empty))
                        Configuration.Settings.Shortcuts.MainListViewColumnDeleteText = GetShortcut(node.Text);
                    else if (text == Configuration.Settings.Language.Settings.ListViewColumnInsert.Replace("&", string.Empty))
                        Configuration.Settings.Shortcuts.MainListViewColumnInsertText = GetShortcut(node.Text);
                    else if (text == Configuration.Settings.Language.Settings.ListViewColumnPaste.Replace("&", string.Empty))
                        Configuration.Settings.Shortcuts.MainListViewColumnPaste = GetShortcut(node.Text);
                    else if (Configuration.Settings.Language.Settings.ListViewFocusWaveform != null && text == Configuration.Settings.Language.Settings.ListViewFocusWaveform.Replace("&", string.Empty))
                        Configuration.Settings.Shortcuts.MainListViewFocusWaveform = GetShortcut(node.Text);
                    else if (Configuration.Settings.Language.Settings.ListViewGoToNextError != null && text == Configuration.Settings.Language.Settings.ListViewGoToNextError.Replace("&", string.Empty))
                        Configuration.Settings.Shortcuts.MainListViewGoToNextError = GetShortcut(node.Text);
                }
            }

            //Main text box
            foreach (TreeNode node in treeViewShortcuts.Nodes[8].Nodes)
            {
                if (node.Text.Contains('['))
                {
                    string text = node.Text.Substring(0, node.Text.IndexOf('[')).Trim();
                    if (text == Configuration.Settings.Language.General.Italic.Replace("&", string.Empty))
                        Configuration.Settings.Shortcuts.MainTextBoxItalic = GetShortcut(node.Text);
                    else if (text == Configuration.Settings.Language.Main.Menu.ContextMenu.SplitLineAtCursorPosition.Replace("&", string.Empty))
                        Configuration.Settings.Shortcuts.MainTextBoxSplitAtCursor = GetShortcut(node.Text);
                    else if (Configuration.Settings.Language.Settings.MainTextBoxMoveLastWordDown != null && text == Configuration.Settings.Language.Settings.MainTextBoxMoveLastWordDown.Replace("&", string.Empty))
                        Configuration.Settings.Shortcuts.MainTextBoxMoveLastWordDown = GetShortcut(node.Text);
                    else if (Configuration.Settings.Language.Settings.MainTextBoxMoveFirstWordFromNextUp != null && text == Configuration.Settings.Language.Settings.MainTextBoxMoveFirstWordFromNextUp.Replace("&", string.Empty))
                        Configuration.Settings.Shortcuts.MainTextBoxMoveFirstWordFromNextUp = GetShortcut(node.Text);
                    else if (Configuration.Settings.Language.Settings.MainTextBoxSelectionToLower != null && text == Configuration.Settings.Language.Settings.MainTextBoxSelectionToLower.Replace("&", string.Empty))
                        Configuration.Settings.Shortcuts.MainTextBoxSelectionToLower = GetShortcut(node.Text);
                    else if (Configuration.Settings.Language.Settings.MainTextBoxSelectionToUpper != null && text == Configuration.Settings.Language.Settings.MainTextBoxSelectionToUpper.Replace("&", string.Empty))
                        Configuration.Settings.Shortcuts.MainTextBoxSelectionToUpper = GetShortcut(node.Text);
                    else if (Configuration.Settings.Language.Main.Menu.ContextMenu.InsertAfter != null && text == Configuration.Settings.Language.Main.Menu.ContextMenu.InsertAfter.Replace("&", string.Empty))
                        Configuration.Settings.Shortcuts.MainTextBoxInsertAfter = GetShortcut(node.Text);
                    else if (Configuration.Settings.Language.Settings.MainTextBoxAutoBreak != null && text == Configuration.Settings.Language.Settings.MainTextBoxAutoBreak.Replace("&", string.Empty))
                        Configuration.Settings.Shortcuts.MainTextBoxAutoBreak = GetShortcut(node.Text);
                    else if (Configuration.Settings.Language.Settings.MainTextBoxUnbreak != null && text == Configuration.Settings.Language.Settings.MainTextBoxUnbreak.Replace("&", string.Empty))
                        Configuration.Settings.Shortcuts.MainTextBoxUnbreak = GetShortcut(node.Text);
                }
            }

            //Create
            foreach (TreeNode node in treeViewShortcuts.Nodes[9].Nodes)
            {
                if (node.Text.Contains('['))
                {
                    string text = node.Text.Substring(0, node.Text.IndexOf('[')).Trim();
                    if (text == Configuration.Settings.Language.Main.VideoControls.InsertNewSubtitleAtVideoPosition.Replace("&", string.Empty))
                        Configuration.Settings.Shortcuts.MainCreateInsertSubAtVideoPos = GetShortcut(node.Text);
                    else if (text == Configuration.Settings.Language.Main.VideoControls.PlayFromJustBeforeText.Replace("&", string.Empty))
                        Configuration.Settings.Shortcuts.MainCreatePlayFromJustBefore = GetShortcut(node.Text);
                    else if (text == Configuration.Settings.Language.Main.VideoControls.SetStartTime.Replace("&", string.Empty))
                        Configuration.Settings.Shortcuts.MainCreateSetStart = GetShortcut(node.Text);
                    else if (text == Configuration.Settings.Language.Main.VideoControls.SetEndTime.Replace("&", string.Empty))
                        Configuration.Settings.Shortcuts.MainCreateSetEnd = GetShortcut(node.Text);
                    else if (text == Configuration.Settings.Language.Settings.MainCreateStartDownEndUp.Replace("&", string.Empty))
                        Configuration.Settings.Shortcuts.MainCreateStartDownEndUp = GetShortcut(node.Text);
                    else if (text == Configuration.Settings.Language.Settings.CreateSetEndAddNewAndGoToNew.Replace("&", string.Empty))
                        Configuration.Settings.Shortcuts.MainCreateSetEndAddNewAndGoToNew = GetShortcut(node.Text);
                }
            }

            //Translate
            foreach (TreeNode node in treeViewShortcuts.Nodes[10].Nodes)
            {
                if (node.Text.Contains('['))
                {
                    string text = node.Text.Substring(0, node.Text.IndexOf('[')).Trim();
                    if (text == Configuration.Settings.Language.Settings.CustomSearch1.Replace("&", string.Empty))
                        Configuration.Settings.Shortcuts.MainTranslateCustomSearch1 = GetShortcut(node.Text);
                    else if (text == Configuration.Settings.Language.Settings.CustomSearch2.Replace("&", string.Empty))
                        Configuration.Settings.Shortcuts.MainTranslateCustomSearch2 = GetShortcut(node.Text);
                    else if (text == Configuration.Settings.Language.Settings.CustomSearch3.Replace("&", string.Empty))
                        Configuration.Settings.Shortcuts.MainTranslateCustomSearch3 = GetShortcut(node.Text);
                    else if (text == Configuration.Settings.Language.Settings.CustomSearch4.Replace("&", string.Empty))
                        Configuration.Settings.Shortcuts.MainTranslateCustomSearch4 = GetShortcut(node.Text);
                    else if (text == Configuration.Settings.Language.Settings.CustomSearch5.Replace("&", string.Empty))
                        Configuration.Settings.Shortcuts.MainTranslateCustomSearch5 = GetShortcut(node.Text);
                    else if (text == Configuration.Settings.Language.Settings.CustomSearch6.Replace("&", string.Empty))
                        Configuration.Settings.Shortcuts.MainTranslateCustomSearch6 = GetShortcut(node.Text);
                }
            }

            //Adjust
            foreach (TreeNode node in treeViewShortcuts.Nodes[11].Nodes)
            {
                if (node.Text.Contains('['))
                {
                    string text = node.Text.Substring(0, node.Text.IndexOf('[')).Trim();
                    if (text == Configuration.Settings.Language.Settings.AdjustViaEndAutoStartAndGoToNext.Replace("&", string.Empty))
                        Configuration.Settings.Shortcuts.MainAdjustViaEndAutoStartAndGoToNext = GetShortcut(node.Text);
                    else if (text == Configuration.Settings.Language.Main.VideoControls.SetstartTimeAndOffsetOfRest.Replace("&", string.Empty))
                        Configuration.Settings.Shortcuts.MainAdjustSetStartAndOffsetTheRest = GetShortcut(node.Text);
                    else if (text == Configuration.Settings.Language.Main.VideoControls.SetEndTimeAndGoToNext.Replace("&", string.Empty))
                        Configuration.Settings.Shortcuts.MainAdjustSetEndAndGotoNext = GetShortcut(node.Text);
                    else if (Configuration.Settings.Language.Settings.AdjustSetStartAutoDurationAndGoToNext != null && text == Configuration.Settings.Language.Settings.AdjustSetStartAutoDurationAndGoToNext.Replace("&", string.Empty))
                        Configuration.Settings.Shortcuts.MainAdjustSetStartAutoDurationAndGoToNext = GetShortcut(node.Text);
                    else if (Configuration.Settings.Language.Settings.AdjustSetEndNextStartAndGoToNext != null && text == Configuration.Settings.Language.Settings.AdjustSetEndNextStartAndGoToNext.Replace("&", string.Empty))
                        Configuration.Settings.Shortcuts.MainAdjustSetEndNextStartAndGoToNext = GetShortcut(node.Text);
                    else if (Configuration.Settings.Language.Settings.AdjustStartDownEndUpAndGoToNext != null && text == Configuration.Settings.Language.Settings.AdjustStartDownEndUpAndGoToNext.Replace("&", string.Empty))
                        Configuration.Settings.Shortcuts.MainAdjustStartDownEndUpAndGoToNext = GetShortcut(node.Text);
                    else if (text == Configuration.Settings.Language.Main.VideoControls.SetStartTime.Replace("&", string.Empty))
                        Configuration.Settings.Shortcuts.MainAdjustSetStart = GetShortcut(node.Text);
                    else if (Configuration.Settings.Language.Settings.AdjustSetStartTimeKeepDuration != null && text == Configuration.Settings.Language.Settings.AdjustSetStartTimeKeepDuration.Replace("&", string.Empty))
                        Configuration.Settings.Shortcuts.MainAdjustSetStartKeepDuration = GetShortcut(node.Text);
                    else if (text == Configuration.Settings.Language.Main.VideoControls.SetEndTime.Replace("&", string.Empty))
                        Configuration.Settings.Shortcuts.MainAdjustSetEnd = GetShortcut(node.Text);
                    else if (Configuration.Settings.Language.Settings.AdjustSelected100MsForward != null && text == Configuration.Settings.Language.Settings.AdjustSelected100MsForward.Replace("&", string.Empty))
                        Configuration.Settings.Shortcuts.MainAdjustSelected100MsForward = GetShortcut(node.Text);
                    else if (Configuration.Settings.Language.Settings.AdjustSelected100MsBack != null && text == Configuration.Settings.Language.Settings.AdjustSelected100MsBack.Replace("&", string.Empty))
                        Configuration.Settings.Shortcuts.MainAdjustSelected100MsBack = GetShortcut(node.Text);
                    else if (Configuration.Settings.Language.Settings.AdjustSetEndAndOffsetTheRest != null && text == Configuration.Settings.Language.Settings.AdjustSetEndAndOffsetTheRest.Replace("&", string.Empty))
                        Configuration.Settings.Shortcuts.MainAdjustSetEndAndOffsetTheRest = GetShortcut(node.Text);
                    else if (Configuration.Settings.Language.Settings.AdjustSetEndAndOffsetTheRestAndGoToNext != null && text == Configuration.Settings.Language.Settings.AdjustSetEndAndOffsetTheRestAndGoToNext.Replace("&", string.Empty))
                        Configuration.Settings.Shortcuts.MainAdjustSetEndAndOffsetTheRestAndGoToNext = GetShortcut(node.Text);
                }
            }

            //Audio-visualizer
            foreach (TreeNode node in treeViewShortcuts.Nodes[12].Nodes)
            {
                if (node.Text.Contains('['))
                {
                    string text = node.Text.Substring(0, node.Text.IndexOf('[')).Trim();
                    if (text == (Configuration.Settings.Language.Waveform.ZoomIn).Replace("&", string.Empty))
                        Configuration.Settings.Shortcuts.WaveformZoomIn = GetShortcut(node.Text);
                    else if (text == (Configuration.Settings.Language.Waveform.ZoomOut).Replace("&", string.Empty))
                        Configuration.Settings.Shortcuts.WaveformZoomOut = GetShortcut(node.Text);
                    else if (!string.IsNullOrEmpty(Configuration.Settings.Language.Settings.WaveformPlayNewSelection) && text == (Configuration.Settings.Language.Settings.WaveformPlayNewSelection).Replace("&", string.Empty))
                        Configuration.Settings.Shortcuts.WaveformPlaySelection = GetShortcut(node.Text);
                    else if (text == (Configuration.Settings.Language.Settings.VerticalZoom).Replace("&", string.Empty))
                        Configuration.Settings.Shortcuts.WaveformVerticalZoom = GetShortcut(node.Text);
                    else if (!string.IsNullOrEmpty(Configuration.Settings.Language.Settings.VerticalZoomOut) && text == (Configuration.Settings.Language.Settings.VerticalZoomOut).Replace("&", string.Empty))
                        Configuration.Settings.Shortcuts.WaveformVerticalZoomOut = GetShortcut(node.Text);
                    else if (text == (Configuration.Settings.Language.Settings.WaveformSeekSilenceForward).Replace("&", string.Empty))
                        Configuration.Settings.Shortcuts.WaveformSearchSilenceForward = GetShortcut(node.Text);
                    else if (text == (Configuration.Settings.Language.Settings.WaveformSeekSilenceBack).Replace("&", string.Empty))
                        Configuration.Settings.Shortcuts.WaveformSearchSilenceBack = GetShortcut(node.Text);
                    else if (text == (Configuration.Settings.Language.Settings.WaveformAddTextHere).Replace("&", string.Empty))
                        Configuration.Settings.Shortcuts.WaveformAddTextHere = GetShortcut(node.Text);
                    else if (Configuration.Settings.Language.Main.VideoControls.InsertNewSubtitleAtVideoPosition != null && text == Configuration.Settings.Language.Main.VideoControls.InsertNewSubtitleAtVideoPosition.Replace("&", string.Empty))
                        Configuration.Settings.Shortcuts.MainWaveformInsertAtCurrentPosition = GetShortcut(node.Text);
                    else if (Configuration.Settings.Language.Settings.WaveformFocusListView != null && text == Configuration.Settings.Language.Settings.WaveformFocusListView.Replace("&", string.Empty))
                        Configuration.Settings.Shortcuts.WaveformFocusListView = GetShortcut(node.Text);

                }
            }

            Configuration.Settings.Save();
        }

        private void FormSettings_KeyDown(object sender, KeyEventArgs e)
        {
            if (comboBoxShortcutKey.Focused)
                return;

            if (e.KeyCode == Keys.Escape)
                DialogResult = DialogResult.Cancel;
            else if (e.KeyCode == Keys.F1)
            {
                Utilities.ShowHelp("#Settings");
                e.SuppressKeyPress = true;
            }
        }

        private void ButtonSsaChooseFontClick(object sender, EventArgs e)
        {
            if (fontDialogSSAStyle.ShowDialog() == DialogResult.OK)
            {
                _ssaFontName = fontDialogSSAStyle.Font.Name;
                _ssaFontSize = fontDialogSSAStyle.Font.Size;
                UpdateSsaExample();
            }
        }

        private void UpdateSsaExample()
        {
            labelSSAFont.Text = string.Format("{0}, size {1}", fontDialogSSAStyle.Font.Name, fontDialogSSAStyle.Font.Size);
            GeneratePreviewReal();
        }

        private void GeneratePreviewReal()
        {
            if (pictureBoxPreview.Image != null)
                pictureBoxPreview.Image.Dispose();
            var bmp = new Bitmap(pictureBoxPreview.Width, pictureBoxPreview.Height);

            using (Graphics g = Graphics.FromImage(bmp))
            {

                // Draw background
                const int rectangleSize = 9;
                for (int y = 0; y < bmp.Height; y += rectangleSize)
                {
                    for (int x = 0; x < bmp.Width; x += rectangleSize)
                    {
                        Color c = Color.WhiteSmoke;
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
                    font = new Font(_ssaFontName, (float)_ssaFontSize);
                }
                catch
                {
                    font = new Font(Font, FontStyle.Regular);
                }
                g.TextRenderingHint = TextRenderingHint.AntiAlias;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                var sf = new StringFormat { Alignment = StringAlignment.Near, LineAlignment = StringAlignment.Near };
                var path = new GraphicsPath();

                bool newLine = false;
                var sb = new StringBuilder();
                sb.Append("This is a test!");

                var measuredWidth = TextDraw.MeasureTextWidth(font, sb.ToString(), false) + 1;
                var measuredHeight = TextDraw.MeasureTextHeight(font, sb.ToString(), false) + 1;

                float left = ((float)(bmp.Width - measuredWidth * 0.8 + 15) / 2);

                float top = bmp.Height - measuredHeight - 10;

                const int leftMargin = 0;
                int pathPointsStart = -1;

                if (checkBoxSsaOpaqueBox.Checked)
                {
                    g.FillRectangle(new SolidBrush(Color.Black), left, top, measuredWidth + 3, measuredHeight + 3);
                }

                TextDraw.DrawText(font, sf, path, sb, false, false, false, left, top, ref newLine, leftMargin, ref pathPointsStart);

                int outline = (int)numericUpDownSsaOutline.Value;

                // draw shadow
                if (numericUpDownSsaShadow.Value > 0 && !checkBoxSsaOpaqueBox.Checked)
                {
                    for (int i = 0; i < (int)numericUpDownSsaShadow.Value; i++)
                    {
                        var shadowPath = new GraphicsPath();
                        sb = new StringBuilder();
                        sb.Append("This is a test!");
                        int pathPointsStart2 = -1;
                        TextDraw.DrawText(font, sf, shadowPath, sb, false, false, false, left + i + outline, top + i + outline, ref newLine, leftMargin, ref pathPointsStart2);
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

        private void ButtonSsaChooseColorClick(object sender, EventArgs e)
        {
            colorDialogSSAStyle.Color = Color.FromArgb(_ssaFontColor);
            if (colorDialogSSAStyle.ShowDialog() == DialogResult.OK)
            {
                _ssaFontColor = colorDialogSSAStyle.Color.ToArgb();
                UpdateSsaExample();
            }
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

            if (reloadListBox)
                listBoxOcrFixList.Items.Clear();
            _ocrFixReplaceList = OcrFixReplaceList.FromLanguageId(cb.CultureInfo.ThreeLetterISOLanguageName);
            if (reloadListBox)
            {
                listBoxOcrFixList.BeginUpdate();
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
                _wordListNamesEtc.Clear();
                Utilities.LoadNamesEtcWordLists(_wordListNamesEtc, _wordListNamesEtc, language);
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
            var cb = comboBoxWordListLanguage.Items[comboBoxWordListLanguage.SelectedIndex] as ComboBoxLanguage;
            if (cb != null)
                return cb.CultureInfo.Name.Replace("-", "_");

            return null;
        }

        private void ButtonAddNamesEtcClick(object sender, EventArgs e)
        {
            string language = GetCurrentWordListLanguage();
            string text = textBoxNameEtc.Text.Trim();
            if (!string.IsNullOrEmpty(language) && text.Length > 1 && !_wordListNamesEtc.Contains(text))
            {
                Utilities.AddWordToLocalNamesEtcList(text, language);
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
                    var namesEtc = new List<string>();
                    var globalNamesEtc = new List<string>();
                    string localNamesEtcFileName = Utilities.LoadLocalNamesEtc(namesEtc, namesEtc, language);
                    Utilities.LoadGlobalNamesEtc(globalNamesEtc, globalNamesEtc);
                    for (int idx = listBoxNamesEtc.SelectedIndices.Count - 1; idx >= 0; idx--)
                    {
                        index = listBoxNamesEtc.SelectedIndices[idx];
                        text = listBoxNamesEtc.Items[index].ToString();
                        if (namesEtc.Contains(text))
                        {
                            namesEtc.Remove(text);
                            removeCount++;
                        }
                        if (globalNamesEtc.Contains(text))
                        {
                            globalNamesEtc.Remove(text);
                            removeCount++;
                        }
                        listBoxNamesEtc.Items.RemoveAt(index);
                    }

                    if (removeCount > 0)
                    {
                        namesEtc.Sort();
                        var doc = new XmlDocument();
                        doc.Load(localNamesEtcFileName);
                        doc.DocumentElement.RemoveAll();
                        foreach (string name in namesEtc)
                        {
                            XmlNode node = doc.CreateElement("name");
                            node.InnerText = name;
                            doc.DocumentElement.AppendChild(node);
                        }
                        doc.Save(localNamesEtcFileName);
                        LoadNamesEtc(language, false); // reload

                        globalNamesEtc.Sort();
                        doc = new XmlDocument();
                        doc.Load(Utilities.DictionaryFolder + "names_etc.xml");
                        doc.DocumentElement.RemoveAll();
                        foreach (string name in globalNamesEtc)
                        {
                            XmlNode node = doc.CreateElement("name");
                            node.InnerText = name;
                            doc.DocumentElement.AppendChild(node);
                        }
                        doc.Save(Utilities.DictionaryFolder + "names_etc.xml");
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
            string language = GetCurrentWordListLanguage();
            string text = textBoxUserWord.Text.Trim().ToLower();
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
            string key = textBoxOcrFixKey.Text.Trim();
            string value = textBoxOcrFixValue.Text.Trim();
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
            var cb = comboBoxWordListLanguage.Items[comboBoxWordListLanguage.SelectedIndex] as ComboBoxLanguage;
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
                e.KeyCode == Keys.F1 ||
                e.KeyCode == Keys.Home ||
                e.KeyCode == Keys.End
                )
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
            ComboBox cb = (ComboBox)sender;
            TextBox tb = textBoxCustomSearchUrl1;
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
            string waveformsFolder = Configuration.WaveformsFolder.TrimEnd(Path.DirectorySeparatorChar);
            if (Directory.Exists(waveformsFolder))
            {
                var di = new DirectoryInfo(waveformsFolder);

                foreach (FileInfo fileName in di.GetFiles("*.wav"))
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

            string spectrogramsFolder = Configuration.SpectrogramsFolder.TrimEnd(Path.DirectorySeparatorChar);
            if (Directory.Exists(spectrogramsFolder))
            {
                var di = new DirectoryInfo(spectrogramsFolder);

                foreach (DirectoryInfo dir in di.GetDirectories())
                {
                    var spectrogramDir = new DirectoryInfo(dir.FullName);
                    foreach (FileInfo fileName in spectrogramDir.GetFiles("*.gif"))
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

        private void buttonUpdateShortcut_Click(object sender, EventArgs e)
        {
            if (treeViewShortcuts.SelectedNode != null && treeViewShortcuts.SelectedNode.Text.Contains('['))
            {
                string text = treeViewShortcuts.SelectedNode.Text.Substring(0, treeViewShortcuts.SelectedNode.Text.IndexOf('[')).Trim();

                if (comboBoxShortcutKey.SelectedIndex == 0)
                {
                    treeViewShortcuts.SelectedNode.Text = text + " [" + Configuration.Settings.Language.General.None + "]";
                    return;
                }

                var sb = new StringBuilder(@"[");
                if (checkBoxShortcutsControl.Checked)
                    sb.Append("Control+");
                if (checkBoxShortcutsAlt.Checked)
                    sb.Append("Alt+");
                if (checkBoxShortcutsShift.Checked)
                    sb.Append("Shift+");
                sb.Append(comboBoxShortcutKey.Items[comboBoxShortcutKey.SelectedIndex]);
                sb.Append(']');

                if (sb.Length < 3 || sb.ToString().EndsWith("+]"))
                {
                    MessageBox.Show(string.Format(Configuration.Settings.Language.Settings.ShortcutIsNotValid, sb));
                    return;
                }
                else if (sb.ToString() == "[CapsLock]")
                {
                    MessageBox.Show(string.Format(Configuration.Settings.Language.Settings.ShortcutIsNotValid, sb));
                    return;
                }
                treeViewShortcuts.SelectedNode.Text = text + " " + sb;

                var existsIn = new StringBuilder();
                foreach (TreeNode node in treeViewShortcuts.Nodes)
                {
                    foreach (TreeNode subNode in node.Nodes)
                    {
                        if (subNode.Text.Contains(sb.ToString()) && treeViewShortcuts.SelectedNode.Text != subNode.Text)
                            existsIn.AppendLine(string.Format(Configuration.Settings.Language.Settings.ShortcutIsAlreadyDefinedX, node.Text + " -> " + subNode.Text));
                    }
                }
                if (existsIn.Length > 0)
                {
                    MessageBox.Show(existsIn.ToString(), string.Empty, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
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
                textBoxVlcPath.Text = Path.GetDirectoryName(openFileDialogFFmpeg.FileName);
                Configuration.Settings.General.VlcLocation = textBoxVlcPath.Text;
                Configuration.Settings.General.VlcLocationRelative = GetRelativePath(textBoxVlcPath.Text);
                radioButtonVideoPlayerVLC.Enabled = LibVlcDynamic.IsInstalled;
            }
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            Configuration.Settings.General.VlcLocation = _oldVlcLocation;
            Configuration.Settings.General.VlcLocationRelative = _oldVlcLocationRelative;

            DialogResult = DialogResult.Cancel;
        }

        private void buttonEditDoNotBreakAfterList_Click(object sender, EventArgs e)
        {
            var form = new DoNotBreakAfterListEdit();
            form.ShowDialog(this);
        }

    }
}
