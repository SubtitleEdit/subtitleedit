using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Windows.Forms;
using System.Xml;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.VideoPlayers;

namespace Nikse.SubtitleEdit.Forms
{
    public sealed partial class Settings : Form
    {
        string _ssaFontName;
        double _ssaFontSize;
        int _ssaFontColor;
        bool _starting = true;
        private string _listBoxSearchString = string.Empty;
        private DateTime _listBoxSearchStringLastUsed = DateTime.Now;

        List<string> _wordListNamesEtc = new List<string>();
        List<string> _userWordList = new List<string>();
        Dictionary<string, string> _ocrFixWords = new Dictionary<string, string>();
        Dictionary<string, string> _ocrFixPartialLines = new Dictionary<string, string>();       

        class ComboBoxLanguage
        {
            public CultureInfo CultureInfo { get; set; }
            public override string ToString()
            {
                return CultureInfo.NativeName;
            }
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
            checkBoxVisualSync.Checked = gs.ShowToolbarVisualSync;
            checkBoxSettings.Checked = gs.ShowToolbarSettings;
            checkBoxSpellCheck.Checked = gs.ShowToolbarSpellCheck;
            checkBoxHelp.Checked = gs.ShowToolbarHelp;

            comboBoxFramerate.Items.Add((23.976).ToString());
            comboBoxFramerate.Items.Add((24.0).ToString());
            comboBoxFramerate.Items.Add((25.0).ToString());
            comboBoxFramerate.Items.Add((29.97).ToString());

            checkBoxShowFrameRate.Checked = gs.ShowFrameRate;
            comboBoxFramerate.Text = gs.DefaultFrameRate.ToString();
            comboBoxEncoding.Text = gs.DefaultEncoding;
            checkBoxAutoDetectAnsiEncoding.Checked = gs.AutoGuessAnsiEncoding;
            comboBoxSubtitleFontSize.Text = gs.SubtitleFontSize.ToString();
            checkBoxSubtitleFontBold.Checked = gs.SubtitleFontBold;
            panelSubtitleFontColor.BackColor = gs.SubtitleFontColor;
            panelSubtitleBackgroundColor.BackColor = gs.SubtitleBackgroundColor;
            checkBoxRememberRecentFiles.Checked = gs.ShowRecentFiles;
            checkBoxRememberRecentFiles_CheckedChanged(null, null);
            checkBoxRememberSelectedLine.Checked = gs.RememberSelectedLine;
            checkBoxReopenLastOpened.Checked = gs.StartLoadLastFile;
            checkBoxStartInSourceView.Checked = gs.StartInSourceView;
            checkBoxRemoveBlankLinesWhenOpening.Checked = gs.RemoveBlankLinesWhenOpening;
            checkBoxRememberWindowPosition.Checked = gs.StartRememberPositionAndSize;
            textBoxSubtitleLineMaximumLength.Text = gs.SubtitleLineMaximumLength.ToString();
            textBoxShowLineBreaksAs.Text = gs.ListViewLineSeparatorString;

            if (string.Compare(gs.VideoPlayer.Trim(), "VLC", true) == 0)
                radioButtonVideoPlayerVLC.Checked = true;
            else if (string.Compare(gs.VideoPlayer.Trim(), "WindowsMediaPlayer", true) == 0)
                radioButtonVideoPlayerWmp.Checked = true;
            //else if (string.Compare(gs.VideoPlayer.Trim(), "ManagedDirectX", true) == 0)
            //    radioButtonVideoPlayerManagedDirectX.Checked = true;
            else
                radioButtonVideoPlayerDirectShow.Checked = true;
            if (!Utilities.IsWmpAvailable)
            {
                radioButtonVideoPlayerWmp.Visible = false;
                labelVideoPlayerWmp.Visible = false;
            }
            //if (!Utilities.IsManagedDirectXInstalled)
            //    radioButtonVideoPlayerManagedDirectX.Enabled = false;
            if (!Utilities.IsQuartsDllInstalled)
                radioButtonVideoPlayerDirectShow.Enabled = false;
            if (!LibVlc11xDynamic.IsInstalled)
            {
                radioButtonVideoPlayerVLC.Enabled = false;
            }


            comboBoxVideoPlayerDefaultVolume.Items.Clear();
            for (int i=0; i<= 100; i++)
                comboBoxVideoPlayerDefaultVolume.Items.Add(i.ToString());
            if (gs.VideoPlayerDefaultVolume >= 0 && gs.VideoPlayerDefaultVolume <= 100)
                comboBoxVideoPlayerDefaultVolume.SelectedIndex = gs.VideoPlayerDefaultVolume;
            checkBoxVideoPlayerShowStopButton.Checked = gs.VideoPlayerShowStopButton;

            comboBoxCustomSearch.Text = Configuration.Settings.VideoControls.CustomSearchText;
            textBoxCustomSearchUrl.Text = Configuration.Settings.VideoControls.CustomSearchUrl;

            foreach (var x in System.Drawing.FontFamily.Families)
            {               
                comboBoxSubtitleFont.Items.Add(x.Name);
                if (string.Compare(x.Name, gs.SubtitleFontName, true) == 0)
                    comboBoxSubtitleFont.SelectedIndex = comboBoxSubtitleFont.Items.Count - 1;
            }

            WordListSettings wordListSettings = Configuration.Settings.WordLists;
            checkBoxNamesEtcOnline.Checked = wordListSettings.UseOnlineNamesEtc;
            textBoxNamesEtcOnline.Text = wordListSettings.NamesEtcUrl;

            SsaStyleSettings ssa = Configuration.Settings.SsaStyle;
            _ssaFontName = ssa.FontName;
            _ssaFontSize = ssa.FontSize;
            _ssaFontColor = ssa.FontColorArgb;
            fontDialogSSAStyle.Font = new System.Drawing.Font(ssa.FontName, (float)ssa.FontSize);
            fontDialogSSAStyle.Color = System.Drawing.Color.FromArgb(_ssaFontColor);
            UpdateSsaExample();



            var proxy = Configuration.Settings.Proxy;
            textBoxProxyAddress.Text = proxy.ProxyAddress;
            textBoxProxyUserName.Text = proxy.UserName;
            if (proxy.Password == null)
                textBoxProxyPassword.Text = string.Empty;
            else
                textBoxProxyPassword.Text = proxy.DecodePassword();
            textBoxProxyDomain.Text = proxy.Domain;

            // Language
            var language = Configuration.Settings.Language.Settings;
            Text = language.Title;
            tabPageGenerel.Text = language.General;
            tabPageVideoPlayer.Text = language.VideoPlayer;
            tabPageWaveForm.Text = language.WaveForm;
            tabPageWordLists.Text = language.WordLists;
            tabPageTools.Text = language.Tools;
            tabPageSsaStyle.Text = language.SsaStyle;
            tabPageProxy.Text = language.Proxy;
            tabPageToolBar.Text = language.Toolbar;
            groupBoxShowToolBarButtons.Text = language.ShowToolBarButtons;
            labelTBNew.Text = language.New;
            labelTBOpen.Text = language.Open;
            labelTBSave.Text = language.Save;
            labelTBSaveAs.Text = language.SaveAs;
            labelTBFind.Text = language.Find;
            labelTBReplace.Text = language.Replace;
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
            labelSubtitleFont.Text = language.SubtitleFont;
            labelSubtitleFontSize.Text = language.SubtitleFontSize;
            labelSubtitleFontColor.Text = language.SubtitleFontColor;
            labelSubtitleFontBackgroundColor.Text = language.SubtitleBackgroundColor;
            labelSpellChecker.Text = language.SubtitleBackgroundColor;
            checkBoxSubtitleFontBold.Text = Configuration.Settings.Language.General.Bold;
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
            comboBoxAutoBackup.Items[0] = Configuration.Settings.Language.General.None;
            comboBoxAutoBackup.Items[1] = language.AutoBackupEveryMinute;
            comboBoxAutoBackup.Items[2] = language.AutoBackupEveryFiveMinutes;
            comboBoxAutoBackup.Items[3] = language.AutoBackupEveryFifteenMinutes;

            groupBoxVideoEngine.Text = language.VideoEngine;
            radioButtonVideoPlayerDirectShow.Text = language.DirectShow;
            labelDirectShowDescription.Text = language.DirectShowDescription;
            radioButtonVideoPlayerManagedDirectX.Text = language.ManagedDirectX;
            labelManagedDirectXDescription.Text = language.ManagedDirectXDescription;
            radioButtonVideoPlayerWmp.Text = language.WindowsMediaPlayer;
            labelVideoPlayerWmp.Text = language.WindowsMediaPlayerDescription;
            if (!string.IsNullOrEmpty(language.VlcMediaPlayer))
            {
                radioButtonVideoPlayerVLC.Text = language.VlcMediaPlayer;
                labelVideoPlayerVLC.Text = language.VlcMediaPlayerDescription;
            }

            checkBoxVideoPlayerShowStopButton.Text = language.ShowStopButton;
            labelDefaultVol.Text = language.DefaultVolume;
            labelVolDescr.Text = language.VolumeNotes;

            groupBoxMainWindowVideoControls.Text = language.MainWindowVideoControls;
            labelCustomSearch.Text = language.CustomSearchTextAndUrl;

            groupBoxWaveFormAppearence.Text = language.WaveFormAppearance;
            checkBoxWaveFormShowGrid.Text = language.WaveFormShowGridLines;
            buttonWaveFormGridColor.Text = language.WaveFormGridColor;
            buttonWaveFormColor.Text = language.WaveFormColor;
            buttonWaveFormSelectedColor.Text = language.WaveFormSelectedColor;
            buttonWaveFormTextColor.Text = language.WaveFormTextColor;
            buttonWaveFormBackgroundColor.Text = language.WaveFormBackgroundColor;

            buttonWaveFormsFolderEmpty.Text = language.WaveFormsFolderEmpty;
            InitializeWaveFormsFolderEmpty(language);


            groupBoxSsaStyle.Text = language.SubStationAlphaStyle;
            buttonSSAChooseFont.Text = language.ChooseFont;
            buttonSSAChooseColor.Text = language.ChooseColor;
            labelExampleColon.Text = language.Example;
            labelSSAExample.Text = language.Testing123;

            groupBoxWordLists.Text = language.WordLists;
            labelWordListLanguage.Text = language.Language;
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
            groupBoxFixCommonErrors.Text = language.FixCommonerrors;
            labelMergeShortLines.Text = language.MergeLinesShorterThan;
            labelToolsMusicSymbol.Text = language.MusicSymbol;
            labelToolsMusicSymbolsToReplace.Text = language.MusicSymbolsToReplace;
            groupBoxSpellCheck.Text = language.SpellCheck;
            checkBoxSpellCheckAutoChangeNames.Text = Configuration.Settings.Language.SpellCheck.AutoFixNames;

            comboBoxListViewDoubleClickEvent.Items.Clear();
            comboBoxListViewDoubleClickEvent.Items.Add(language.MainListViewNothing);
            comboBoxListViewDoubleClickEvent.Items.Add(language.MainListViewVideoGoToPositionAndPause);
            comboBoxListViewDoubleClickEvent.Items.Add(language.MainListViewVideoGoToPositionAndPlay);
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

            if (gs.SpellChecker.ToLower().Contains("word"))
                comboBoxSpellChecker.SelectedIndex = 1;
            else
                comboBoxSpellChecker.SelectedIndex = 0;

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
                comboBoxMergeShortLineLength.Items.Add(i.ToString());
                
            if (toolsSettings.MergeLinesShorterThan >= 10 && toolsSettings.MergeLinesShorterThan - 10 < comboBoxMergeShortLineLength.Items.Count)
                comboBoxMergeShortLineLength.SelectedIndex = toolsSettings.MergeLinesShorterThan - 10;
            else
                comboBoxMergeShortLineLength.SelectedIndex = 0;


            // Music notes / music symbols
            if (!Utilities.IsRunningOnMono() && Environment.OSVersion.Version.Major < 6) // 6 == Vista/Win2008Server/Win7
            {

                float fontSize = comboBoxToolsMusicSymbol.Font.Size;
                listBoxNamesEtc.Font = new System.Drawing.Font("Lucida Sans Unicode", fontSize);
                listBoxUserWordLists.Font = new System.Drawing.Font("Lucida Sans Unicode", fontSize);
                listBoxOcrFixList.Font = new System.Drawing.Font("Lucida Sans Unicode", fontSize);
                comboBoxToolsMusicSymbol.Font = new System.Drawing.Font("Lucida Sans Unicode", fontSize);
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

            checkBoxSpellCheckAutoChangeNames.Checked = toolsSettings.SpellCheckAutoChangeNames;
            
            
            buttonOK.Text = Configuration.Settings.Language.General.OK;
            buttonCancel.Text = Configuration.Settings.Language.General.Cancel;

            ListWordListLanguages();
            _starting = false;

            checkBoxWaveFormShowGrid.Checked = Configuration.Settings.VideoControls.WaveFormDrawGrid;
            panelWaveFormGridColor.BackColor = Configuration.Settings.VideoControls.WaveFormGridColor;
            panelWaveFormSelectedColor.BackColor = Configuration.Settings.VideoControls.WaveFormSelectedColor;
            panelWaveFormColor.BackColor = Configuration.Settings.VideoControls.WaveFormColor;
            panelWaveFormBackgroundColor.BackColor = Configuration.Settings.VideoControls.WaveFormBackgroundColor;
            panelWaveFormTextColor.BackColor = Configuration.Settings.VideoControls.WaveFormTextColor;

            FixLargeFonts();
        }

        private void FixLargeFonts()
        {
            Graphics graphics = this.CreateGraphics();
            SizeF textSize = graphics.MeasureString(buttonOK.Text, this.Font);
            if (textSize.Height > buttonOK.Height - 4)
            {
                int newButtonHeight = (int)(textSize.Height + 7 + 0.5);
                Utilities.SetButtonHeight(this, newButtonHeight, 1);
            }
        }

        private void InitializeWaveFormsFolderEmpty(LanguageStructure.Settings language)
        {
            string waveFormsFolder = Configuration.WaveFormsFolder.TrimEnd(Path.DirectorySeparatorChar);
            if (Directory.Exists(waveFormsFolder))
            {
                long bytes = 0;
                int count = 0;
                DirectoryInfo di = new DirectoryInfo(waveFormsFolder);
                foreach (FileInfo fi in di.GetFiles("*.wav"))
                {
                    bytes += fi.Length;
                    count++;
                }
                labelWaveFormsFolderInfo.Text = string.Format(language.WaveFormsFolderInfo, count, bytes / 1024.0 / 1024.0);
                buttonWaveFormsFolderEmpty.Enabled = count > 0;
            }
            else
            {
                buttonWaveFormsFolderEmpty.Enabled = false;
                labelWaveFormsFolderInfo.Text = string.Format(language.WaveFormsFolderInfo, 0, 0);
            }
        }

        public void Initialize(Icon icon, Image newFile, Image openFile, Image saveFile, Image SaveFileAs, Image find, Image replace, 
                               Image visualSync, Image spellCheck, Image settings, Image help)
        {
            this.Icon = (Icon)icon.Clone();
            pictureBoxNew.Image = (Image)newFile.Clone();
            pictureBoxOpen.Image = (Image)openFile.Clone();
            pictureBoxSave.Image = (Image)saveFile.Clone();
            pictureBoxSaveAs.Image = (Image)SaveFileAs.Clone();
            pictureBoxFind.Image = (Image)find.Clone();
            pictureBoxReplace.Image = (Image)replace.Clone();
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
                    string name = culture.CompareInfo.Name;

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
            gs.ShowToolbarVisualSync = checkBoxVisualSync.Checked;
            gs.ShowToolbarSettings = checkBoxSettings.Checked;
            gs.ShowToolbarSpellCheck = checkBoxSpellCheck.Checked;
            gs.ShowToolbarHelp = checkBoxHelp.Checked;

            gs.ShowFrameRate = checkBoxShowFrameRate.Checked;
            gs.DefaultFrameRate = double.Parse(comboBoxFramerate.Text);
            gs.DefaultEncoding = comboBoxEncoding.Text;
            gs.AutoGuessAnsiEncoding = checkBoxAutoDetectAnsiEncoding.Checked;
            gs.SubtitleFontSize = int.Parse(comboBoxSubtitleFontSize.Text);
            gs.SubtitleFontBold = checkBoxSubtitleFontBold.Checked;
            gs.SubtitleFontColor = panelSubtitleFontColor.BackColor;
            gs.SubtitleBackgroundColor = panelSubtitleBackgroundColor.BackColor;
            gs.ShowRecentFiles = checkBoxRememberRecentFiles.Checked;
            gs.RememberSelectedLine = checkBoxRememberSelectedLine.Checked;
            gs.StartLoadLastFile = checkBoxReopenLastOpened.Checked;
            gs.StartRememberPositionAndSize = checkBoxRememberWindowPosition.Checked;
            gs.StartInSourceView = checkBoxStartInSourceView.Checked;
            gs.RemoveBlankLinesWhenOpening = checkBoxRemoveBlankLinesWhenOpening.Checked;
            gs.ListViewLineSeparatorString = textBoxShowLineBreaksAs.Text;
            if (gs.ListViewLineSeparatorString.Trim().Length == 0)
                gs.ListViewLineSeparatorString = Environment.NewLine;
            gs.ListViewDoubleClickAction = comboBoxListViewDoubleClickEvent.SelectedIndex;

            if (comboBoxAutoBackup.SelectedIndex == 1)
                gs.AutoBackupSeconds = 60;
            else if (comboBoxAutoBackup.SelectedIndex == 2)
                gs.AutoBackupSeconds = 60 * 5;
            else if (comboBoxAutoBackup.SelectedIndex == 3)
                gs.AutoBackupSeconds = 60 * 15;
            else
                gs.AutoBackupSeconds = 0;

            if (comboBoxSpellChecker.SelectedIndex == 1)
                gs.SpellChecker = "word";
            else
                gs.SpellChecker = "hunspell";

            if (radioButtonVideoPlayerWmp.Checked)
                gs.VideoPlayer = "WindowsMediaPlayer";
            //else if (radioButtonVideoPlayerManagedDirectX.Checked)
            //    gs.VideoPlayer = "ManagedDirectX";
            else if (radioButtonVideoPlayerVLC.Checked)
                gs.VideoPlayer = "VLC";
            else
                gs.VideoPlayer = "DirectShow";
            gs.VideoPlayerDefaultVolume = comboBoxVideoPlayerDefaultVolume.SelectedIndex;
            if (gs.VideoPlayerDefaultVolume < 0 || gs.VideoPlayerDefaultVolume > 100)
                comboBoxVideoPlayerDefaultVolume.SelectedIndex = 50;
            gs.VideoPlayerShowStopButton = checkBoxVideoPlayerShowStopButton.Checked;

            Configuration.Settings.VideoControls.CustomSearchText = comboBoxCustomSearch.Text;
            Configuration.Settings.VideoControls.CustomSearchUrl = textBoxCustomSearchUrl.Text;

            int maxLength;
            if (int.TryParse(textBoxSubtitleLineMaximumLength.Text, out maxLength))
            {
                if (maxLength > 15 && maxLength < 200)
                    gs.SubtitleLineMaximumLength = maxLength;
                else
                    gs.SubtitleLineMaximumLength = 68;
            }
            else
            {
                gs.SubtitleLineMaximumLength = 68;
            }
            
            if (comboBoxSubtitleFont.SelectedItem != null)
                gs.SubtitleFontName = comboBoxSubtitleFont.SelectedItem.ToString();

            ToolsSettings toolsSettings = Configuration.Settings.Tools;
            toolsSettings.VerifyPlaySeconds = comboBoxToolsVerifySeconds.SelectedIndex + 2;
            toolsSettings.StartSceneIndex = comboBoxToolsStartSceneIndex.SelectedIndex;
            toolsSettings.EndSceneIndex = comboBoxToolsEndSceneIndex.SelectedIndex;
            toolsSettings.MergeLinesShorterThan = comboBoxMergeShortLineLength.SelectedIndex + 10;
            toolsSettings.MusicSymbol = comboBoxToolsMusicSymbol.SelectedItem.ToString();
            toolsSettings.MusicSymbolToReplace = textBoxMusicSymbolsToReplace.Text;
            toolsSettings.SpellCheckAutoChangeNames = checkBoxSpellCheckAutoChangeNames.Checked;

            WordListSettings wordListSettings = Configuration.Settings.WordLists;
            wordListSettings.UseOnlineNamesEtc = checkBoxNamesEtcOnline.Checked;
            wordListSettings.NamesEtcUrl = textBoxNamesEtcOnline.Text;
            if (comboBoxWordListLanguage.Items.Count > 0 && comboBoxWordListLanguage.SelectedIndex >= 0)
            {
                var ci = comboBoxWordListLanguage.Items[comboBoxWordListLanguage.SelectedIndex] as ComboBoxLanguage;
                if (ci != null)
                    Configuration.Settings.WordLists.LastLanguage = ci.CultureInfo.Name;
            }

            SsaStyleSettings ssa = Configuration.Settings.SsaStyle;
            ssa.FontName = _ssaFontName;
            ssa.FontSize = _ssaFontSize;
            ssa.FontColorArgb = _ssaFontColor;

            ProxySettings proxy = Configuration.Settings.Proxy;
            proxy.ProxyAddress = textBoxProxyAddress.Text ;
            proxy.UserName = textBoxProxyUserName.Text;
            if (textBoxProxyPassword.Text.Trim().Length == 0)
                proxy.Password = null;
            else
                proxy.EncodePassword(textBoxProxyPassword.Text);
            proxy.Domain = textBoxProxyDomain.Text;

            Configuration.Settings.VideoControls.WaveFormDrawGrid = checkBoxWaveFormShowGrid.Checked;
            Configuration.Settings.VideoControls.WaveFormGridColor = panelWaveFormGridColor.BackColor;
            Configuration.Settings.VideoControls.WaveFormSelectedColor = panelWaveFormSelectedColor.BackColor;
            Configuration.Settings.VideoControls.WaveFormColor = panelWaveFormColor.BackColor;
            Configuration.Settings.VideoControls.WaveFormBackgroundColor = panelWaveFormBackgroundColor.BackColor;
            Configuration.Settings.VideoControls.WaveFormTextColor = panelWaveFormTextColor.BackColor;

            Configuration.Settings.Save();
        }

        private void FormSettings_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
                DialogResult = DialogResult.Cancel;
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
            labelSSAExample.ForeColor = System.Drawing.Color.FromArgb(_ssaFontColor);
            labelSSAExample.Font = new System.Drawing.Font(_ssaFontName, (float)_ssaFontSize);

            labelSSAFont.Text = string.Format("{0}, size {1}",
                                fontDialogSSAStyle.Font.Name,
                                fontDialogSSAStyle.Font.Size);
        }

        private void ButtonSsaChooseColorClick(object sender, EventArgs e)
        {
            colorDialogSSAStyle.Color = labelSSAExample.ForeColor;
            if (colorDialogSSAStyle.ShowDialog() == DialogResult.OK)
            {
                labelSSAExample.ForeColor = colorDialogSSAStyle.Color;
                _ssaFontColor = colorDialogSSAStyle.Color.ToArgb();
                UpdateSsaExample();
            }
        }

        private void TextBoxAjustSecondsKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.D0 ||
                e.KeyCode == Keys.D1 ||
                e.KeyCode == Keys.D2 ||
                e.KeyCode == Keys.D3 ||
                e.KeyCode == Keys.D4 ||
                e.KeyCode == Keys.D5 ||
                e.KeyCode == Keys.D6 ||
                e.KeyCode == Keys.D7 ||
                e.KeyCode == Keys.D8 ||
                e.KeyCode == Keys.D9 ||
                e.KeyCode == Keys.Delete ||
                e.KeyCode == Keys.Left ||
                e.KeyCode == Keys.Right ||
                e.KeyCode == Keys.Back ||
                (e.KeyValue >= 96 && e.KeyValue <= 105))
            {
            }
            else
            {
                e.Handled = true;
                e.SuppressKeyPress = true;
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

                // ocr fix words
                LoadOcrFixList(true);

                LoadNamesEtc(language, _starting, true);
            }
        }

        private void LoadOcrFixList(bool reloadListBox)
        {
            var cb = comboBoxWordListLanguage.Items[comboBoxWordListLanguage.SelectedIndex] as ComboBoxLanguage;
            if (cb == null)
                return;            

            _ocrFixWords = new Dictionary<string, string>();
            _ocrFixPartialLines = new Dictionary<string, string>();

            if (reloadListBox)
                listBoxOcrFixList.Items.Clear();
            string replaceListXmlFileName = Utilities.DictionaryFolder + cb.CultureInfo.ThreeLetterISOLanguageName + "_OCRFixReplaceList.xml";
            if (File.Exists(replaceListXmlFileName))
            {
                var doc = new XmlDocument();
                doc.Load(replaceListXmlFileName);
                _ocrFixWords = Logic.OCR.OcrFixEngine.LoadReplaceList(doc, "WholeWords");
                _ocrFixPartialLines = Logic.OCR.OcrFixEngine.LoadReplaceList(doc, "PartialLines");

                if (reloadListBox)
                {
                    listBoxOcrFixList.BeginUpdate();
                    foreach (var pair in _ocrFixWords)
                    {
                        listBoxOcrFixList.Items.Add(pair.Key + " --> " + pair.Value);
                    }
                    foreach (var pair in _ocrFixPartialLines)
                    {
                        listBoxOcrFixList.Items.Add(pair.Key + " --> " + pair.Value);
                    }
                    listBoxOcrFixList.EndUpdate();
                }
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

        private void LoadNamesEtc(string language, bool async, bool reloadListBox)
        {
            if (reloadListBox)
                listBoxNamesEtc.Items.Clear();
            _wordListNamesEtc = new List<string>();

            if (async)
            {
                var bw = new System.ComponentModel.BackgroundWorker();
                bw.RunWorkerCompleted += BwRunWorkerCompleted;
                bw.DoWork += BwDoWork;
                bw.RunWorkerAsync(language);
            }
            else
            {
                // names etc
                Utilities.LoadNamesEtcWordLists(_wordListNamesEtc, _wordListNamesEtc, language);
                _wordListNamesEtc.Sort();

                if (reloadListBox)
                {
                    listBoxNamesEtc.BeginUpdate();
                    foreach (string name in _wordListNamesEtc)
                        listBoxNamesEtc.Items.Add(name);
                    listBoxNamesEtc.EndUpdate();
                }
            }
        }

        void BwDoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            Utilities.LoadNamesEtcWordLists(_wordListNamesEtc, _wordListNamesEtc, e.Argument.ToString());
            _wordListNamesEtc.Sort();
        }

        void BwRunWorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            try
            {
                listBoxNamesEtc.BeginUpdate();
                foreach (string name in _wordListNamesEtc)
                {
                    listBoxNamesEtc.Items.Add(name);
                }
                listBoxNamesEtc.EndUpdate();
            }
            catch
            { 
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
                LoadNamesEtc(language, false, true);
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
            string language = GetCurrentWordListLanguage();
            int index = listBoxNamesEtc.SelectedIndex;
            string text = listBoxNamesEtc.Items[index].ToString();
            if (!string.IsNullOrEmpty(language) && index >= 0)
            {
                if (MessageBox.Show(string.Format(Configuration.Settings.Language.Settings.RemoveX, text), null, MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    // Check local names etc. first
                    var namesEtc = new List<string>();
                    string localNamesEtcFileName = Utilities.LoadLocalNamesEtc(namesEtc, namesEtc, language);
                    if (namesEtc.Contains(text))
                    {
                        namesEtc.Remove(text);
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
                        LoadNamesEtc(language, false, false); // reload
                        buttonRemoveNameEtc.Enabled = false;

                        listBoxNamesEtc.Items.RemoveAt(index);
                        if (index < listBoxNamesEtc.Items.Count)
                            listBoxNamesEtc.SelectedIndex = index;
                        else if (listBoxNamesEtc.Items.Count > 0)
                            listBoxNamesEtc.SelectedIndex = index - 1;
                        listBoxNamesEtc.Focus();

                        return;
                    }

                    if (Configuration.Settings.WordLists.UseOnlineNamesEtc && !string.IsNullOrEmpty(Configuration.Settings.WordLists.NamesEtcUrl))
                    {
                        MessageBox.Show(Configuration.Settings.Language.Settings.CannotUpdateNamesEtcOnline);
                        return;
                    }

                    namesEtc = new List<string>();
                    Utilities.LoadGlobalNamesEtc(namesEtc, namesEtc);
                    if (namesEtc.Contains(text))
                    {
                        namesEtc.Remove(text);
                        namesEtc.Sort();
                        var doc = new XmlDocument();
                        doc.Load(Utilities.DictionaryFolder + "names_etc.xml");
                        doc.DocumentElement.RemoveAll();
                        foreach (string name in namesEtc)
                        {
                            XmlNode node = doc.CreateElement("name");
                            node.InnerText = name;
                            doc.DocumentElement.AppendChild(node);
                        }
                        doc.Save(Utilities.DictionaryFolder + "names_etc.xml");
                        LoadNamesEtc(language, false, true); // reload
                        return;
                    }
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
            if (!string.IsNullOrEmpty(language) && text.Length > 1 && !_userWordList.Contains(text))
            {
                Utilities.AddToUserDictionary(text , language);
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
            string language = GetCurrentWordListLanguage();
            int index = listBoxUserWordLists.SelectedIndex;
            string text = listBoxUserWordLists.Items[index].ToString();
            if (!string.IsNullOrEmpty(language) && index >= 0)
            {
                if (MessageBox.Show(string.Format(Configuration.Settings.Language.Settings.RemoveX, text), null, MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    var words = new List<string>();
                    string userWordFileName = Utilities.LoadUserWordList(words, language);
                    if (words.Contains(text))
                    {
                        words.Remove(text);
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

                        listBoxUserWordLists.Items.RemoveAt(index);
                        if (index < listBoxUserWordLists.Items.Count)
                            listBoxUserWordLists.SelectedIndex = index;
                        else if (listBoxUserWordLists.Items.Count > 0)
                            listBoxUserWordLists.SelectedIndex = index - 1;
                        listBoxUserWordLists.Focus();
                        return;
                    }

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
            if (key.Length == 0 || value.Length == 0)
                return;

            Dictionary<string, string> dictionary = _ocrFixWords;
            string elementName = "Word";
            string parentName = "WholeWords";

            if (key.Contains(" "))
            {
                dictionary = _ocrFixPartialLines;
                elementName = "LinePart";
                parentName = "PartialLines";
            }

            var cb = comboBoxWordListLanguage.Items[comboBoxWordListLanguage.SelectedIndex] as ComboBoxLanguage;
            if (cb == null)
                return;

            if (dictionary.ContainsKey(key))
            {
                MessageBox.Show(Configuration.Settings.Language.Settings.WordAlreadyExists);
                return;
            }

            dictionary.Add(key, value);

            //Sort
            var sortedDictionary = new SortedDictionary<string, string>();
            foreach (var pair in dictionary)
                sortedDictionary.Add(pair.Key, pair.Value);
            
            string replaceListXmlFileName = Utilities.DictionaryFolder + cb.CultureInfo.ThreeLetterISOLanguageName + "_OCRFixReplaceList.xml";
            var doc = new XmlDocument();
            if (File.Exists(replaceListXmlFileName))
                doc.Load(replaceListXmlFileName);
            else
                doc.LoadXml("<OCRFixReplaceList><WholeWords/><PartialWords/><PartialLines/><BeginLines/><EndLines/><WholeLines/></OCRFixReplaceList>");

            XmlNode wholeWords = doc.DocumentElement.SelectSingleNode(parentName);
            wholeWords.RemoveAll();
            foreach (var pair in sortedDictionary)
            {
                XmlNode node = doc.CreateElement(elementName);

                XmlAttribute wordFrom = doc.CreateAttribute("from");
                wordFrom.InnerText = pair.Key;
                node.Attributes.Append(wordFrom);

                XmlAttribute wordTo = doc.CreateAttribute("to");
                wordTo.InnerText = pair.Value;
                node.Attributes.Append(wordTo);

                wholeWords.AppendChild(node);
            }
            doc.Save(replaceListXmlFileName);
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

            int index = listBoxOcrFixList.SelectedIndex;
            string text = listBoxOcrFixList.Items[index].ToString();
            string key = text.Substring(0, text.IndexOf(" --> ")).Trim();

            if (_ocrFixWords.ContainsKey(key))
            {
                if (MessageBox.Show(string.Format(Configuration.Settings.Language.Settings.RemoveX, text), null, MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    _ocrFixWords.Remove(key);
                    string replaceListXmlFileName = Utilities.DictionaryFolder + cb.CultureInfo.ThreeLetterISOLanguageName + "_OCRFixReplaceList.xml";
                    var doc = new XmlDocument();
                    doc.Load(replaceListXmlFileName);

                    XmlNode wholeWords = doc.DocumentElement.SelectSingleNode("WholeWords");
                    wholeWords.RemoveAll();
                    foreach (var pair in _ocrFixWords)
                    {
                        XmlNode node = doc.CreateElement("Word");

                        XmlAttribute wordFrom = doc.CreateAttribute("from");
                        wordFrom.InnerText = pair.Key;
                        node.Attributes.Append(wordFrom);

                        XmlAttribute wordTo = doc.CreateAttribute("to");
                        wordTo.InnerText = pair.Value;
                        node.Attributes.Append(wordTo);

                        wholeWords.AppendChild(node);
                    }
                    doc.Save(replaceListXmlFileName);

                    LoadOcrFixList(false);
                    buttonRemoveOcrFix.Enabled = false;

                    listBoxOcrFixList.Items.RemoveAt(index);
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
                 e.KeyCode == Keys.Up  ||
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
                if (s.ToLower().StartsWith(_listBoxSearchString.ToLower()))
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
            if (comboBoxCustomSearch.SelectedIndex >= 0)
            {
                if (comboBoxCustomSearch.SelectedIndex == 0)
                    textBoxCustomSearchUrl.Text = "http://encarta.msn.com/encnet/features/dictionary/DictionaryResults.aspx?lextype=2&search={0}";
                else if (comboBoxCustomSearch.SelectedIndex == 1)
                    textBoxCustomSearchUrl.Text = "http://dictionary.reference.com/browse/{0}";
                else if (comboBoxCustomSearch.SelectedIndex == 2)
                    textBoxCustomSearchUrl.Text = "http://www.thefreedictionary.com/{0}";
                else if (comboBoxCustomSearch.SelectedIndex == 3)
                    textBoxCustomSearchUrl.Text = "http://www.visuwords.com/?word={0}";                
            }
        }

        private void buttonWaveFormSelectedColor_Click(object sender, EventArgs e)
        {
            colorDialogSSAStyle.Color = panelWaveFormSelectedColor.BackColor;
            if (colorDialogSSAStyle.ShowDialog() == DialogResult.OK)
            {
                panelWaveFormSelectedColor.BackColor = colorDialogSSAStyle.Color;
            }
        }

        private void buttonWaveFormColor_Click(object sender, EventArgs e)
        {
            colorDialogSSAStyle.Color = panelWaveFormColor.BackColor;
            if (colorDialogSSAStyle.ShowDialog() == DialogResult.OK)
            {
                panelWaveFormColor.BackColor = colorDialogSSAStyle.Color;
            }
        }

        private void buttonWaveFormBackgroundColor_Click(object sender, EventArgs e)
        {
            colorDialogSSAStyle.Color = panelWaveFormBackgroundColor.BackColor;
            if (colorDialogSSAStyle.ShowDialog() == DialogResult.OK)
            {
                panelWaveFormBackgroundColor.BackColor = colorDialogSSAStyle.Color;
            }
        }

        private void buttonWaveFormGridColor_Click(object sender, EventArgs e)
        {
            colorDialogSSAStyle.Color = panelWaveFormGridColor.BackColor;
            if (colorDialogSSAStyle.ShowDialog() == DialogResult.OK)
            {
                panelWaveFormGridColor.BackColor = colorDialogSSAStyle.Color;
            }
        }

        private void buttonWaveFormTextColor_Click(object sender, EventArgs e)
        {
            colorDialogSSAStyle.Color = panelWaveFormTextColor.BackColor;
            if (colorDialogSSAStyle.ShowDialog() == DialogResult.OK)
            {
                panelWaveFormTextColor.BackColor = colorDialogSSAStyle.Color;
            }
        }

        private void buttonWaveFormsFolderEmpty_Click(object sender, EventArgs e)
        {
            string waveFormsFolder = Configuration.WaveFormsFolder.TrimEnd(Path.DirectorySeparatorChar);
            if (Directory.Exists(waveFormsFolder))
            {
                DirectoryInfo di = new DirectoryInfo(waveFormsFolder);
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
            InitializeWaveFormsFolderEmpty(Configuration.Settings.Language.Settings);
        }

        private void checkBoxRememberRecentFiles_CheckedChanged(object sender, EventArgs e)
        {
            checkBoxReopenLastOpened.Enabled = checkBoxRememberRecentFiles.Checked;
            checkBoxRememberSelectedLine.Enabled = checkBoxRememberRecentFiles.Checked;
        }

        private void buttonWaveFormSelectedColor_Click(object sender, MouseEventArgs e)
        {
            colorDialogSSAStyle.Color = panelWaveFormSelectedColor.BackColor;
            if (colorDialogSSAStyle.ShowDialog() == DialogResult.OK)
            {
                panelWaveFormSelectedColor.BackColor = colorDialogSSAStyle.Color;
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

    }
}
