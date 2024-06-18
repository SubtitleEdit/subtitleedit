using Nikse.SubtitleEdit.Controls;
using Nikse.SubtitleEdit.Core.AutoTranslate;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.Enums;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Forms.BeautifyTimeCodes;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.VideoPlayers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using MessageBox = Nikse.SubtitleEdit.Forms.SeMsgBox.MessageBox;

namespace Nikse.SubtitleEdit.Forms.Options
{
    public sealed partial class Settings : PositionAndSizeForm
    {
        private const int GeneralSection = 0;
        private const int SubtitleFormatsSection = 1;
        private const int ShortcutsSection = 2;
        private const int SyntaxColoringSection = 3;
        private const int VideoPlayerSection = 4;
        private const int WaveformAndSpectrogramSection = 5;
        private const int ToolsSection = 6;
        private const int AutoTranslateSection = 7;
        private const int ToolbarSection = 8;
        private const int AppearanceSection = 9;
        private const int NetworkSection = 10;
        private const int FileTypeAssociationSection = 11;

        private string _oldVlcLocation;
        private string _oldVlcLocationRelative;
        private bool _oldListViewShowCps;
        private bool _oldListViewShowWpm;
        private readonly Dictionary<ShortcutHelper, string> _newShortcuts = new Dictionary<ShortcutHelper, string>();
        private List<RulesProfile> _rulesProfiles;
        private List<PluginShortcut> _pluginShortcuts;
        private readonly bool _loading;
        private string _defaultLanguages;

        private static IEnumerable<string> GetSubtitleFormats() => SubtitleFormat.AllSubtitleFormats.Where(format => !format.IsVobSubIndexFile).Select(format => format.FriendlyName);

        internal class ComboBoxLanguage
        {
            public CultureInfo CultureInfo { get; set; }
            public override string ToString()
            {
                return CultureInfo.NativeName.CapitalizeFirstLetter();
            }
        }

        public class ShortcutNode
        {
            public string Text { get; set; }
            public string ShortcutText { get; set; }
            public ShortcutHelper Shortcut { get; set; }
            public List<ShortcutNode> Nodes { get; set; }

            public ShortcutNode(string text)
            {
                Text = text;
                Nodes = new List<ShortcutNode>();
            }
        }

        public class ShortcutHelper
        {
            public ShortcutHelper(PropertyInfo shortcut, bool isMenuItem, bool isPlugin = false)
            {
                Shortcut = shortcut;
                IsMenuItem = isMenuItem;
                IsPlugin = isPlugin;
            }

            public PropertyInfo Shortcut { get; set; }
            public bool IsMenuItem { get; set; }
            public bool IsPlugin { get; set; }
        }

        private static string GetRelativePath(string fileName)
        {
            var folder = Configuration.BaseDirectory;

            if (string.IsNullOrEmpty(fileName) || !fileName.StartsWith(folder.Substring(0, 2), StringComparison.OrdinalIgnoreCase))
            {
                return string.Empty;
            }

            var pathUri = new Uri(fileName);
            if (!folder.EndsWith(Path.DirectorySeparatorChar))
            {
                folder += Path.DirectorySeparatorChar;
            }

            var folderUri = new Uri(folder);
            return Uri.UnescapeDataString(folderUri.MakeRelativeUri(pathUri).ToString().Replace('/', Path.DirectorySeparatorChar));
        }

        private readonly string _oldSettings;

        public Settings()
        {
            _loading = true;
            UiUtil.PreInitialize(this);
            InitializeComponent();
            UiUtil.FixFonts(this);
            UiUtil.FixLargeFonts(this, buttonOK);

            listBoxSection.Items.Clear();
            listBoxSection.Items.AddRange(new object[] {
                "General",
                "Subtitle formats",
                "Shortcuts",
                "Syntax coloring",
                "Video player",
                "Waveform/spectrogram",
                "Tools",
                "Auto translate",
                "Toolbar",
                "Font",
                "Network",
                "File type associations"});

            Init();
            _loading = false;

            _oldSettings = Core.Common.Settings.CustomSerialize(Configuration.Settings);
        }

        public void Init()
        {
            MakeShortcutsTreeView(LanguageSettings.Current.Settings);
            labelStatus.Text = string.Empty;
            _rulesProfiles = new List<RulesProfile>(Configuration.Settings.General.Profiles);
            var gs = Configuration.Settings.General;

            listBoxSection.SelectedIndex = GeneralSection;

            checkBoxToolbarNew.Checked = gs.ShowToolbarNew;
            checkBoxTBOpen.Checked = gs.ShowToolbarOpen;
            checkBoxTBOpenVideo.Checked = gs.ShowToolbarOpenVideo;
            checkBoxTBSave.Checked = gs.ShowToolbarSave;
            checkBoxTBSaveAs.Checked = gs.ShowToolbarSaveAs;
            checkBoxTBFind.Checked = gs.ShowToolbarFind;
            checkBoxTBReplace.Checked = gs.ShowToolbarReplace;
            checkBoxTBFixCommonErrors.Checked = gs.ShowToolbarFixCommonErrors;
            checkBoxTBRemoveTextForHi.Checked = gs.ShowToolbarRemoveTextForHi;
            checkBoxTBToggleSourceView.Checked = gs.ShowToolbarToggleSourceView;
            checkBoxTBVisualSync.Checked = gs.ShowToolbarVisualSync;
            checkBoxTBBurnIn.Checked = gs.ShowToolbarBurnIn;
            checkBoxTBSettings.Checked = gs.ShowToolbarSettings;
            checkBoxTBSpellCheck.Checked = gs.ShowToolbarSpellCheck;
            checkBoxTBNetflixQualityCheck.Checked = gs.ShowToolbarNetflixGlyphCheck;
            checkBoxTBBeautifyTimeCodes.Checked = gs.ShowToolbarBeautifyTimeCodes;
            checkBoxTBHelp.Checked = gs.ShowToolbarHelp;

            comboBoxFrameRate.Items.Clear();
            comboBoxFrameRate.Items.Add(23.976.ToString(CultureInfo.CurrentCulture));
            comboBoxFrameRate.Items.Add(24.0.ToString(CultureInfo.CurrentCulture));
            comboBoxFrameRate.Items.Add(25.0.ToString(CultureInfo.CurrentCulture));
            comboBoxFrameRate.Items.Add(29.97.ToString(CultureInfo.CurrentCulture));
            comboBoxFrameRate.Items.Add(30.00.ToString(CultureInfo.CurrentCulture));
            comboBoxFrameRate.Items.Add(50.00.ToString(CultureInfo.CurrentCulture));
            comboBoxFrameRate.Items.Add(59.94.ToString(CultureInfo.CurrentCulture));
            comboBoxFrameRate.Items.Add(60.00.ToString(CultureInfo.CurrentCulture));

            comboBoxOpaqueBoxStyle.Items.Clear();
            comboBoxOpaqueBoxStyle.Items.Add(LanguageSettings.Current.SubStationAlphaStyles.BoxPerLine);
            comboBoxOpaqueBoxStyle.Items.Add(LanguageSettings.Current.SubStationAlphaStyles.BoxMultiLine);
            comboBoxOpaqueBoxStyle.SelectedIndex = 0;

            checkBoxShowFrameRate.Checked = gs.ShowFrameRate;
            comboBoxFrameRate.Text = gs.DefaultFrameRate.ToString(CultureInfo.CurrentCulture);

            UiUtil.InitializeTextEncodingComboBox(comboBoxEncoding);

            checkBoxAutoDetectAnsiEncoding.Checked = gs.AutoGuessAnsiEncoding;
            comboBoxSubtitleFontSize.Text = gs.SubtitleTextBoxFontSize.ToString(CultureInfo.InvariantCulture);
            comboBoxSubtitleListViewFontSize.Text = gs.SubtitleListViewFontSize.ToString(CultureInfo.InvariantCulture);
            checkBoxSubtitleFontBold.Checked = gs.SubtitleTextBoxFontBold;
            checkBoxSubtitleTextBoxSyntaxColor.Checked = gs.SubtitleTextBoxSyntaxColor;
            panelTextBoxHtmlColor.BackColor = gs.SubtitleTextBoxHtmlColor;
            panelTextBoxAssColor.BackColor = gs.SubtitleTextBoxAssColor;
            panelDarkThemeBackColor.BackColor = gs.DarkThemeBackColor;
            panelDarkThemeColor.BackColor = gs.DarkThemeForeColor;
            checkBoxDarkThemeEnabled.Checked = gs.UseDarkTheme;
            checkBoxDarkThemeShowListViewGridLines.Checked = gs.DarkThemeShowListViewGridLines;
            checkBoxSubtitleListViewFontBold.Checked = gs.SubtitleListViewFontBold;
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
            numericUpDownOptimalCharsSec.Value = (decimal)gs.SubtitleOptimalCharactersPerSeconds;
            numericUpDownMaxCharsSec.Value = (decimal)gs.SubtitleMaximumCharactersPerSeconds;
            numericUpDownMaxWordsMin.Value = (decimal)gs.SubtitleMaximumWordsPerMinute;
            checkBoxAutoWrapWhileTyping.Checked = gs.AutoWrapLineWhileTyping;
            textBoxShowLineBreaksAs.Text = gs.ListViewLineSeparatorString;

            numericUpDownDurationMin.Value = gs.SubtitleMinimumDisplayMilliseconds;
            numericUpDownDurationMax.Value = gs.SubtitleMaximumDisplayMilliseconds;

            if (gs.MinimumMillisecondsBetweenLines >= numericUpDownMinGapMs.Minimum &&
                gs.MinimumMillisecondsBetweenLines <= numericUpDownMinGapMs.Maximum)
            {
                numericUpDownMinGapMs.Value = gs.MinimumMillisecondsBetweenLines;
            }

            if (gs.VideoPlayer.Trim().Equals("VLC", StringComparison.OrdinalIgnoreCase) && LibVlcDynamic.IsInstalled)
            {
                radioButtonVideoPlayerVLC.Checked = true;
            }
            else if (gs.VideoPlayer.Trim().Equals("MPV", StringComparison.OrdinalIgnoreCase) && LibMpvDynamic.IsInstalled)
            {
                radioButtonVideoPlayerMPV.Checked = true;
            }
            else if (gs.VideoPlayer.Trim().Equals("MPC-HC", StringComparison.OrdinalIgnoreCase) && UiUtil.IsMpcInstalled)
            {
                radioButtonVideoPlayerMpcHc.Checked = true;
            }
            else if (UiUtil.IsQuartsDllInstalled)
            {
                radioButtonVideoPlayerDirectShow.Checked = true;
            }
            else if (LibMpvDynamic.IsInstalled)
            {
                radioButtonVideoPlayerMPV.Checked = true;
            }
            else if (LibVlcDynamic.IsInstalled)
            {
                radioButtonVideoPlayerVLC.Checked = true;
            }

            if (!LibVlcDynamic.IsInstalled)
            {
                radioButtonVideoPlayerVLC.Enabled = false;
            }

            if (!UiUtil.IsQuartsDllInstalled)
            {
                radioButtonVideoPlayerDirectShow.Enabled = false;
            }

            if (Logic.VideoPlayers.MpcHC.MpcHc.GetMpcFileName() == null)
            {
                radioButtonVideoPlayerMpcHc.Enabled = false;
            }

            RefreshMpvSettings();
            buttonMpvSettings.Text = LanguageSettings.Current.SettingsMpv.DownloadMpv;
            checkBoxMpvHandlesPreviewText.Checked = gs.MpvHandlesPreviewText;

            textBoxVlcPath.Text = gs.VlcLocation;
            textBoxVlcPath.Left = labelVideoPlayerVLC.Left + labelVideoPlayerVLC.Width + 5;
            textBoxVlcPath.Width = buttonVlcPathBrowse.Left - textBoxVlcPath.Left - 5;

            labelVlcPath.Text = LanguageSettings.Current.Settings.VlcBrowseToLabel;

            checkBoxVideoPlayerShowStopButton.Checked = gs.VideoPlayerShowStopButton;
            checkBoxVideoPlayerShowMuteButton.Checked = gs.VideoPlayerShowMuteButton;
            checkBoxVideoPlayerShowFullscreenButton.Checked = gs.VideoPlayerShowFullscreenButton;

            comboBoxlVideoPlayerPreviewFontSize.Items.Clear();
            for (var i = 8; i <= 120; i++)
            {
                comboBoxlVideoPlayerPreviewFontSize.Items.Add(i.ToString());
            }
            comboBoxlVideoPlayerPreviewFontSize.Text = gs.VideoPlayerPreviewFontSize.ToString();
            if (string.IsNullOrEmpty(comboBoxlVideoPlayerPreviewFontSize.Text))
            {
                comboBoxlVideoPlayerPreviewFontSize.SelectedIndex = 3;
            }

            var verticalMargin = gs.MpvPreviewTextMarginVertical;
            if (verticalMargin >= numericUpDownMarginVertical.Minimum && verticalMargin <= numericUpDownMarginVertical.Maximum)
            {
                numericUpDownMarginVertical.Value = verticalMargin;
            }
            else
            {
                numericUpDownMarginVertical.Value = 10;
            }

            numericUpDownMpvOutline.Value = gs.MpvPreviewTextOutlineWidth;
            numericUpDownMpvShadowWidth.Value = gs.MpvPreviewTextShadowWidth;
            checkBoxVideoPlayerPreviewFontBold.Checked = gs.VideoPlayerPreviewFontBold;
            radioButtonMpvOpaqueBox.Checked = gs.MpvPreviewTextOpaqueBox;
            radioButtonMpvOutline.Checked = !gs.MpvPreviewTextOpaqueBox;
            if (gs.MpvPreviewTextOpaqueBox && gs.MpvPreviewTextOpaqueBoxStyle == "3")
            {
                comboBoxOpaqueBoxStyle.SelectedIndex = 0;
            }
            else if (gs.MpvPreviewTextOpaqueBox)
            {
                comboBoxOpaqueBoxStyle.SelectedIndex = 1;
            }

            panelMpvPrimaryColor.BackColor = gs.MpvPreviewTextPrimaryColor;
            panelMpvOutlineColor.BackColor = gs.MpvPreviewTextOutlineColor;
            panelMpvBackColor.BackColor = gs.MpvPreviewTextBackgroundColor;

            checkBoxVideoAutoOpen.Checked = !gs.DisableVideoAutoLoading;
            checkBoxAllowVolumeBoost.Checked = gs.AllowVolumeBoost;

            comboBoxCustomSearch1.Text = Configuration.Settings.VideoControls.CustomSearchText1;
            comboBoxCustomSearch2.Text = Configuration.Settings.VideoControls.CustomSearchText2;
            comboBoxCustomSearch3.Text = Configuration.Settings.VideoControls.CustomSearchText3;
            comboBoxCustomSearch4.Text = Configuration.Settings.VideoControls.CustomSearchText4;
            comboBoxCustomSearch5.Text = Configuration.Settings.VideoControls.CustomSearchText5;
            textBoxCustomSearchUrl1.Text = Configuration.Settings.VideoControls.CustomSearchUrl1;
            textBoxCustomSearchUrl2.Text = Configuration.Settings.VideoControls.CustomSearchUrl2;
            textBoxCustomSearchUrl3.Text = Configuration.Settings.VideoControls.CustomSearchUrl3;
            textBoxCustomSearchUrl4.Text = Configuration.Settings.VideoControls.CustomSearchUrl4;
            textBoxCustomSearchUrl5.Text = Configuration.Settings.VideoControls.CustomSearchUrl5;

            comboBoxSubtitleFont.BeginUpdate();
            comboBoxVideoPlayerPreviewFontName.BeginUpdate();
            comboBoxSubtitleFont.Items.Clear();
            comboBoxVideoPlayerPreviewFontName.Items.Clear();
            var comboBoxSubtitleFontList = new List<string>();
            var comboBoxSubtitleFontIndex = 0;
            var comboBoxVideoPlayerPreviewFontIndex = 0;
            foreach (var fontFamily in FontHelper.GetAllSupportedFontFamilies())
            {
                comboBoxSubtitleFontList.Add(fontFamily.Name);
                if (fontFamily.Name.Equals(gs.SubtitleFontName, StringComparison.OrdinalIgnoreCase))
                {
                    comboBoxSubtitleFontIndex = comboBoxSubtitleFontList.Count - 1;
                }

                if (fontFamily.Name.Equals(gs.VideoPlayerPreviewFontName, StringComparison.OrdinalIgnoreCase))
                {
                    comboBoxVideoPlayerPreviewFontIndex = comboBoxSubtitleFontList.Count - 1;
                }
            }
            comboBoxSubtitleFont.Items.AddRange(comboBoxSubtitleFontList.ToArray<object>());
            comboBoxVideoPlayerPreviewFontName.Items.AddRange(comboBoxSubtitleFontList.ToArray<object>());
            comboBoxSubtitleFont.SelectedIndex = comboBoxSubtitleFontIndex;
            comboBoxVideoPlayerPreviewFontName.SelectedIndex = comboBoxVideoPlayerPreviewFontIndex;
            comboBoxSubtitleFont.EndUpdate();
            comboBoxVideoPlayerPreviewFontName.EndUpdate();

            var proxy = Configuration.Settings.Proxy;
            textBoxProxyAddress.Text = proxy.ProxyAddress;
            textBoxProxyUserName.Text = proxy.UserName;
            textBoxProxyPassword.Text = proxy.Password == null ? string.Empty : proxy.DecodePassword();
            textBoxProxyDomain.Text = proxy.Domain;
            comboBoxProxyAuthType.Text = proxy.AuthType;
            checkBoxProxyUseDefaultCredentials.Checked = proxy.UseDefaultCredentials;

            textBoxNetworkSessionNewMessageSound.Text = Configuration.Settings.NetworkSettings.NewMessageSound;

            checkBoxSyntaxColorDurationTooSmall.Checked = Configuration.Settings.Tools.ListViewSyntaxColorDurationSmall;
            checkBoxSyntaxColorDurationTooLarge.Checked = Configuration.Settings.Tools.ListViewSyntaxColorDurationBig;
            checkBoxSyntaxColorTextTooLong.Checked = Configuration.Settings.Tools.ListViewSyntaxColorLongLines;
            checkBoxSyntaxColorTextTooWide.Checked = Configuration.Settings.Tools.ListViewSyntaxColorWideLines;
            checkBoxSyntaxColorTextMoreThanTwoLines.Checked = Configuration.Settings.Tools.ListViewSyntaxMoreThanXLines;
            if (Configuration.Settings.General.MaxNumberOfLines >= numericUpDownMaxNumberOfLines.Minimum &&
                Configuration.Settings.General.MaxNumberOfLines <= numericUpDownMaxNumberOfLines.Maximum)
            {
                numericUpDownMaxNumberOfLines.Value = Configuration.Settings.General.MaxNumberOfLines;
            }
            checkBoxSyntaxOverlap.Checked = Configuration.Settings.Tools.ListViewSyntaxColorOverlap;
            checkBoxSyntaxColorGapTooSmall.Checked = Configuration.Settings.Tools.ListViewSyntaxColorGap;
            panelListViewSyntaxColorError.BackColor = Configuration.Settings.Tools.ListViewSyntaxErrorColor;

            // Language and controls initialization
            var language = LanguageSettings.Current.Settings;
            listBoxSection.Items[GeneralSection] = language.General;
            listBoxSection.Items[SubtitleFormatsSection] = language.SubtitleFormats;
            listBoxSection.Items[ShortcutsSection] = language.Shortcuts;
            listBoxSection.Items[SyntaxColoringSection] = language.SyntaxColoring;
            listBoxSection.Items[VideoPlayerSection] = language.VideoPlayer;
            listBoxSection.Items[WaveformAndSpectrogramSection] = language.WaveformAndSpectrogram;
            listBoxSection.Items[ToolsSection] = language.Tools;
            listBoxSection.Items[AutoTranslateSection] = LanguageSettings.Current.Main.VideoControls.AutoTranslate;
            listBoxSection.Items[ToolbarSection] = language.Toolbar;
            listBoxSection.Items[AppearanceSection] = language.Appearance;
            listBoxSection.Items[NetworkSection] = language.Network;
            listBoxSection.Items[FileTypeAssociationSection] = language.FileTypeAssociations;

            Text = language.Title;
            panelGeneral.Text = language.General;
            panelSubtitleFormats.Text = language.SubtitleFormats;
            panelVideoPlayer.Text = language.VideoPlayer;
            panelWaveform.Text = language.WaveformAndSpectrogram;
            panelTools.Text = language.Tools;
            panelNetwork.Text = language.Network;
            panelToolBar.Text = language.Toolbar;
            panelFont.Text = LanguageSettings.Current.DCinemaProperties.Font;
            panelShortcuts.Text = language.Shortcuts;
            panelSyntaxColoring.Text = language.SyntaxColoring;
            groupBoxShowToolBarButtons.Text = language.ShowToolBarButtons;
            labelTBNew.Text = language.New;
            labelTBOpen.Text = language.Open;
            labelTBOpenVideo.Text = LanguageSettings.Current.Main.Menu.Video.OpenVideo.Trim('.');
            labelTBSave.Text = language.Save;
            labelTBSaveAs.Text = language.SaveAs;
            labelTBFind.Text = language.Find;
            labelTBReplace.Text = language.Replace;
            labelTBFixCommonErrors.Text = language.FixCommonerrors;
            labelTBRemoveTextForHi.Text = language.RemoveTextForHi;
            labelTBVisualSync.Text = language.VisualSync;
            labelTBBurnIn.Text = language.BurnIn;
            labelTBSpellCheck.Text = language.SpellCheck;
            labelTBNetflixQualityCheck.Text = language.NetflixQualityCheck;
            labelTBBeautifyTimeCodes.Text = language.BeautifyTimeCodes;
            labelTBToggleSourceView.Text = language.ToggleView;
            labelTBSettings.Text = language.SettingsName;
            labelTBHelp.Text = language.Help;
            labelToolbarIconTheme.Text = language.Theme;
            checkBoxToolbarNew.Text = LanguageSettings.Current.General.Visible;
            checkBoxTBOpen.Text = LanguageSettings.Current.General.Visible;
            checkBoxTBOpenVideo.Text = LanguageSettings.Current.General.Visible;
            checkBoxTBSave.Text = LanguageSettings.Current.General.Visible;
            checkBoxTBSaveAs.Text = LanguageSettings.Current.General.Visible;
            checkBoxTBFind.Text = LanguageSettings.Current.General.Visible;
            checkBoxTBReplace.Text = LanguageSettings.Current.General.Visible;
            checkBoxTBFixCommonErrors.Text = LanguageSettings.Current.General.Visible;
            checkBoxTBRemoveTextForHi.Text = LanguageSettings.Current.General.Visible;
            checkBoxTBVisualSync.Text = LanguageSettings.Current.General.Visible;
            checkBoxTBBurnIn.Text = LanguageSettings.Current.General.Visible;
            checkBoxTBSpellCheck.Text = LanguageSettings.Current.General.Visible;
            checkBoxTBNetflixQualityCheck.Text = LanguageSettings.Current.General.Visible;
            checkBoxTBBeautifyTimeCodes.Text = LanguageSettings.Current.General.Visible;
            checkBoxTBSettings.Text = LanguageSettings.Current.General.Visible;
            checkBoxTBHelp.Text = LanguageSettings.Current.General.Visible;
            checkBoxTBToggleSourceView.Text = LanguageSettings.Current.General.Visible;

            // Toolbar icons first row
            var tbTabIndex = checkBoxToolbarNew.TabIndex;
            tbTabIndex++;

            labelTBOpen.Left = Math.Max(labelTBNew.Right, checkBoxToolbarNew.Right) + 18;
            pictureBoxTBOpen.Left = labelTBOpen.Left;
            checkBoxTBOpen.Left = labelTBOpen.Left;
            checkBoxTBOpen.TabIndex = tbTabIndex;
            tbTabIndex++;

            labelTBOpenVideo.Left = Math.Max(labelTBOpen.Right, checkBoxTBOpen.Right) + 18;
            pictureBoxTBOpenVideo.Left = labelTBOpenVideo.Left;
            checkBoxTBOpenVideo.Left = labelTBOpenVideo.Left;
            checkBoxTBOpenVideo.TabIndex = tbTabIndex;
            tbTabIndex++;

            labelTBSave.Left = Math.Max(labelTBOpenVideo.Right, checkBoxTBOpenVideo.Right) + 18;
            pictureBoxTBSave.Left = labelTBSave.Left;
            checkBoxTBSave.Left = labelTBSave.Left;
            checkBoxTBSave.TabIndex = tbTabIndex;
            tbTabIndex++;

            labelTBSaveAs.Left = Math.Max(labelTBSave.Right, checkBoxTBSave.Right) + 18;
            pictureBoxTBSaveAs.Left = labelTBSaveAs.Left;
            checkBoxTBSaveAs.Left = labelTBSaveAs.Left;
            checkBoxTBSaveAs.TabIndex = tbTabIndex;
            tbTabIndex++;

            labelTBFind.Left = Math.Max(labelTBSaveAs.Right, checkBoxTBSaveAs.Right) + 18;
            pictureBoxTBFind.Left = labelTBFind.Left;
            checkBoxTBFind.Left = labelTBFind.Left;
            checkBoxTBFind.TabIndex = tbTabIndex;
            tbTabIndex++;

            labelTBReplace.Left = Math.Max(labelTBFind.Right, checkBoxTBFind.Right) + 18;
            pictureBoxTBReplace.Left = labelTBReplace.Left;
            checkBoxTBReplace.Left = labelTBReplace.Left;
            checkBoxTBReplace.TabIndex = tbTabIndex;
            tbTabIndex++;

            labelTBFixCommonErrors.Left = Math.Max(labelTBReplace.Right, checkBoxTBReplace.Right) + 18;
            pictureBoxTBFixCommonErrors.Left = labelTBFixCommonErrors.Left;
            checkBoxTBFixCommonErrors.Left = labelTBFixCommonErrors.Left;
            checkBoxTBFixCommonErrors.TabIndex = tbTabIndex;
            tbTabIndex++;

            // Toolbar icons second row
            checkBoxTBRemoveTextForHi.TabIndex = tbTabIndex;
            tbTabIndex++;

            labelTBVisualSync.Left = Math.Max(labelTBRemoveTextForHi.Right, checkBoxTBRemoveTextForHi.Right) + 18;
            pictureBoxTBVisualSync.Left = labelTBVisualSync.Left;
            checkBoxTBVisualSync.Left = labelTBVisualSync.Left;
            checkBoxTBVisualSync.TabIndex = tbTabIndex;
            tbTabIndex++;

            labelTBBurnIn.Left = Math.Max(labelTBVisualSync.Right, checkBoxTBVisualSync.Right) + 18;
            pictureBoxTBBurnIn.Left = labelTBBurnIn.Left;
            checkBoxTBBurnIn.Left = labelTBBurnIn.Left;
            checkBoxTBBurnIn.TabIndex = tbTabIndex;
            tbTabIndex++;

            labelTBSpellCheck.Left = Math.Max(labelTBBurnIn.Right, checkBoxTBBurnIn.Right) + 18;
            pictureBoxTBSpellCheck.Left = labelTBSpellCheck.Left;
            checkBoxTBSpellCheck.Left = labelTBSpellCheck.Left;
            checkBoxTBSpellCheck.TabIndex = tbTabIndex;
            tbTabIndex++;

            labelTBSettings.Left = Math.Max(labelTBSpellCheck.Right, checkBoxTBSpellCheck.Right) + 18;
            pictureBoxTBSettings.Left = labelTBSettings.Left;
            checkBoxTBSettings.Left = labelTBSettings.Left;
            checkBoxTBSettings.TabIndex = tbTabIndex;
            tbTabIndex++;

            labelTBNetflixQualityCheck.Left = Math.Max(labelTBSettings.Right, checkBoxTBSettings.Right) + 18;
            pictureBoxTBNetflixQualityCheck.Left = labelTBNetflixQualityCheck.Left;
            checkBoxTBNetflixQualityCheck.Left = labelTBNetflixQualityCheck.Left;
            checkBoxTBNetflixQualityCheck.TabIndex = tbTabIndex;
            tbTabIndex++;

            // Toolbar icons third row
            checkBoxTBBeautifyTimeCodes.TabIndex = tbTabIndex;
            tbTabIndex++;

            labelTBHelp.Left = Math.Max(labelTBBeautifyTimeCodes.Right, checkBoxTBBeautifyTimeCodes.Right) + 18;
            pictureBoxTBHelp.Left = labelTBHelp.Left;
            checkBoxTBHelp.Left = labelTBHelp.Left;
            checkBoxTBHelp.TabIndex = tbTabIndex;
            tbTabIndex++;

            labelTBToggleSourceView.Left = Math.Max(labelTBHelp.Right, checkBoxTBHelp.Right) + 18;
            pictureBoxTBToggleSourceView.Left = labelTBToggleSourceView.Left;
            checkBoxTBToggleSourceView.Left = labelTBToggleSourceView.Left;
            checkBoxTBToggleSourceView.TabIndex = tbTabIndex;

            groupBoxMiscellaneous.Text = language.General;
            groupBoxToolsMisc.Text = language.Miscellaneous;
            groupBoxGeneralRules.Text = language.Rules;
            labelRulesProfileName.Text = language.Profile;
            checkBoxShowFrameRate.Text = language.ShowFrameRate;
            labelDefaultFrameRate.Text = language.DefaultFrameRate;
            labelDefaultFileEncoding.Text = language.DefaultFileEncoding;
            labelAutoDetectAnsiEncoding.Text = language.AutoDetectAnsiEncoding;
            labelDefaultLanguages.Text = language.LanguageFilter;
            labelSubMaxLen.Text = language.SubtitleLineMaximumLength;
            labelOptimalCharsPerSecond.Text = language.OptimalCharactersPerSecond;
            labelMaxCharsPerSecond.Text = language.MaximumCharactersPerSecond;
            labelMaxWordsPerMin.Text = language.MaximumWordsPerMinute;
            checkBoxAutoWrapWhileTyping.Text = language.AutoWrapWhileTyping;
            groupBoxAppearance.Text = language.Appearance;
            groupBoxFontInUI.Text = language.FontInUi;
            groupBoxFontGeneral.Text = language.General;
            groupBoxFontListViews.Text = language.ListView;
            groupBoxFontTextBox.Text = language.TextBox;
            labelFontNote.Text = language.FontNote;
            labelMinDuration.Text = language.DurationMinimumMilliseconds;
            labelMaxDuration.Text = language.DurationMaximumMilliseconds;
            labelMinGapMs.Text = language.MinimumGapMilliseconds;
            labelMaxLines.Text = language.MaximumLines;
            if (labelSubMaxLen.Left + labelSubMaxLen.Width > numericUpDownSubtitleLineMaximumLength.Left)
            {
                numericUpDownSubtitleLineMaximumLength.Left = labelSubMaxLen.Left + labelSubMaxLen.Width + 3;
            }

            if (labelMaxCharsPerSecond.Left + labelMaxCharsPerSecond.Width > numericUpDownMaxCharsSec.Left)
            {
                numericUpDownMaxCharsSec.Left = labelMaxCharsPerSecond.Left + labelMaxCharsPerSecond.Width + 3;
            }

            if (labelMaxWordsPerMin.Left + labelMaxWordsPerMin.Width > numericUpDownMaxWordsMin.Left)
            {
                numericUpDownMaxWordsMin.Left = labelMaxWordsPerMin.Left + labelMaxWordsPerMin.Width + 3;
            }

            if (labelMinDuration.Left + labelMinDuration.Width > numericUpDownDurationMin.Left)
            {
                numericUpDownDurationMin.Left = labelMinDuration.Left + labelMinDuration.Width + 3;
            }

            if (labelMaxDuration.Left + labelMaxDuration.Width > numericUpDownDurationMax.Left)
            {
                numericUpDownDurationMax.Left = labelMaxDuration.Left + labelMaxDuration.Width + 3;
            }

            if (labelMinGapMs.Left + labelMinGapMs.Width > numericUpDownMinGapMs.Left)
            {
                numericUpDownMinGapMs.Left = labelMinGapMs.Left + labelMinGapMs.Width + 3;
                buttonGapChoose.Left = numericUpDownMinGapMs.Left + numericUpDownMinGapMs.Width + 5;
            }

            if (labelMergeShortLines.Left + labelMergeShortLines.Width > comboBoxMergeShortLineLength.Left)
            {
                comboBoxMergeShortLineLength.Left = labelMergeShortLines.Left + labelMergeShortLines.Width + 3;
            }

            labelSubtitleFont.Text = language.SubtitleFont;
            labelSubtitleFontSize.Text = language.SubtitleFontSize;
            labelSubtitleListViewFontSize.Text = language.SubtitleFontSize;
            checkBoxSubtitleFontBold.Text = language.SubtitleBold;
            checkBoxSubtitleTextBoxSyntaxColor.Text = language.UseSyntaxColoring;
            buttonTextBoxHtmlColor.Text = language.HtmlColor;
            buttonTextBoxAssColor.Text = language.AssaColor;
            groupBoxDarkTheme.Text = language.DarkTheme;
            groupBoxGraphicsButtons.Text = language.GraphicsButtons;
            checkBoxDarkThemeEnabled.Text = language.DarkThemeEnabled;
            checkBoxDarkThemeShowListViewGridLines.Text = language.DarkThemeShowGridViewLines;
            buttonDarkThemeColor.Text = language.WaveformTextColor;
            buttonDarkThemeBackColor.Text = language.WaveformBackgroundColor;
            checkBoxSubtitleListViewFontBold.Text = language.SubtitleBold;
            checkBoxSubtitleCenter.Text = language.SubtitleCenter;
            checkBoxSubtitleCenter.Left = checkBoxSubtitleFontBold.Left;
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

            var appearanceSubFontLeft = labelSubtitleFont.Left + 9 +
                Math.Max(Math.Max(labelSubtitleFont.Width, labelSubtitleFontColor.Width), labelSubtitleFontBackgroundColor.Width);
            comboBoxSubtitleFont.Left = appearanceSubFontLeft;
            panelSubtitleFontColor.Left = appearanceSubFontLeft;
            panelSubtitleBackgroundColor.Left = appearanceSubFontLeft;

            labelShowLineBreaksAs.Text = language.ShowLineBreaksAs;
            textBoxShowLineBreaksAs.Left = labelShowLineBreaksAs.Left + labelShowLineBreaksAs.Width;
            labelListViewDoubleClickEvent.Text = language.MainListViewDoubleClickAction;
            labelSaveAsFileNameFrom.Text = language.SaveAsFileNameFrom;
            labelAutoBackup.Text = language.AutoBackup;
            labelAutoBackupDeleteAfter.Text = language.AutoBackupDeleteAfter;
            labelTranslationAutoSuffix.Text = language.TranslationAutoSuffix;
            comboBoxTranslationAutoSuffix.Left = labelTranslationAutoSuffix.Right + 3;
            buttonTranslationAutoSuffix.Left = comboBoxTranslationAutoSuffix.Right + 3;
            comboBoxAutoBackup.Left = labelAutoBackup.Left + labelAutoBackup.Width + 1;
            labelAutoBackupDeleteAfter.Left = comboBoxAutoBackup.Left + comboBoxAutoBackup.Width + 5;
            comboBoxAutoBackupDeleteAfter.Left = labelAutoBackupDeleteAfter.Left + labelAutoBackupDeleteAfter.Width + 1;
            checkBoxCheckForUpdates.Text = language.CheckForUpdates;
            checkBoxAutoSave.Text = language.AutoSave;
            checkBoxCheckForUpdates.Left = checkBoxAutoSave.Left + checkBoxAutoSave.Width + 15;
            checkBoxAllowEditOfOriginalSubtitle.Text = language.AllowEditOfOriginalSubtitle;
            checkBoxPromptDeleteLines.Text = language.PromptDeleteLines;

            comboBoxTimeCodeMode.Items.Clear();
            comboBoxTimeCodeMode.Items.Add(language.TimeCodeModeHHMMSSMS);
            comboBoxTimeCodeMode.Items.Add(language.TimeCodeModeHHMMSSFF);
            comboBoxTimeCodeMode.SelectedIndex = gs.UseTimeFormatHHMMSSFF ? 1 : 0;
            labelTimeCodeMode.Text = language.TimeCodeMode;
            comboBoxTimeCodeMode.Left = labelTimeCodeMode.Left + labelTimeCodeMode.Width + 4;

            comboBoxSplitBehavior.Items.Clear();
            comboBoxSplitBehavior.Items.Add(language.SplitBehaviorPrevious);
            comboBoxSplitBehavior.Items.Add(language.SplitBehaviorHalf);
            comboBoxSplitBehavior.Items.Add(language.SplitBehaviorNext);
            comboBoxSplitBehavior.SelectedIndex = gs.SplitBehavior;
            labelSplitBehavior.Text = language.SplitBehavior;
            comboBoxSplitBehavior.Left = labelTimeCodeMode.Left + labelTimeCodeMode.Width + 4;
            if (labelSplitBehavior.Width > labelTimeCodeMode.Width)
            {
                comboBoxSplitBehavior.Left = labelSplitBehavior.Left + labelSplitBehavior.Width + 4;
            }

            var dropDownSplitBehaviorWidth = comboBoxSplitBehavior.Width;
            using (var g = Graphics.FromHwnd(IntPtr.Zero))
            {
                foreach (var item in comboBoxSplitBehavior.Items)
                {
                    var itemWidth = (int)g.MeasureString((string)item, Font).Width + 5;
                    dropDownSplitBehaviorWidth = Math.Max(itemWidth, dropDownSplitBehaviorWidth);
                }
            }
            comboBoxSplitBehavior.DropDownWidth = dropDownSplitBehaviorWidth;


            comboBoxAutoBackup.Items[0] = LanguageSettings.Current.General.None;
            comboBoxAutoBackup.Items[1] = language.AutoBackupEveryMinute;
            comboBoxAutoBackup.Items[2] = language.AutoBackupEveryFiveMinutes;
            comboBoxAutoBackup.Items[3] = language.AutoBackupEveryFifteenMinutes;

            comboBoxAutoBackupDeleteAfter.Items.Clear();
            comboBoxAutoBackupDeleteAfter.Items.Add(language.AutoBackupDeleteAfterOneMonth);
            for (var i = 2; i <= 24; i++)
            {
                comboBoxAutoBackupDeleteAfter.Items.Add(string.Format(language.AutoBackupDeleteAfterXMonths, i));
            }

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

            groupBoxMpvBorder.Text = LanguageSettings.Current.SubStationAlphaStyles.Border;
            labelMpvShadow.Text = LanguageSettings.Current.SubStationAlphaStyles.Shadow;
            radioButtonMpvOutline.Text = LanguageSettings.Current.SubStationAlphaStyles.Outline;
            radioButtonMpvOpaqueBox.Text = LanguageSettings.Current.SubStationAlphaStyles.OpaqueBox;

            radioButtonVideoPlayerVLC.Text = language.VlcMediaPlayer;
            labelVideoPlayerVLC.Text = language.VlcMediaPlayerDescription;
            gs.VlcLocation = textBoxVlcPath.Text;

            checkBoxVideoPlayerShowStopButton.Text = language.ShowStopButton;
            checkBoxVideoPlayerShowMuteButton.Text = language.ShowMuteButton;
            checkBoxVideoPlayerShowFullscreenButton.Text = language.ShowFullscreenButton;

            labelVideoPlayerPreviewFontName.Text = language.PreviewFontName;
            labelVideoPlayerPreviewFontSize.Text = language.PreviewFontSize;
            buttonMpvPrimaryColor.Text = LanguageSettings.Current.SubStationAlphaStyles.Primary;
            buttonMpvOutlineColor.Text = LanguageSettings.Current.SubStationAlphaStyles.Outline;
            buttonMpvBackColor.Text = LanguageSettings.Current.SubStationAlphaStyles.Shadow;
            labelMarginVertical.Text = language.PreviewVerticalMargin;
            numericUpDownMarginVertical.Left = labelMarginVertical.Right + 5;

            checkBoxVideoPlayerPreviewFontBold.Text = language.SubtitleBold;
            var left = labelVideoPlayerPreviewFontName.Left + 5 +
                        Math.Max(labelVideoPlayerPreviewFontName.Width, labelVideoPlayerPreviewFontSize.Width);
            comboBoxVideoPlayerPreviewFontName.Left = left;
            comboBoxlVideoPlayerPreviewFontSize.Left = left;
            panelMpvPrimaryColor.Left = buttonMpvPrimaryColor.Right + 5;
            panelMpvOutlineColor.Left = buttonMpvOutlineColor.Right + 5;
            panelMpvBackColor.Left = buttonMpvBackColor.Right + 5;
            checkBoxVideoPlayerPreviewFontBold.Left = comboBoxVideoPlayerPreviewFontName.Right + 6;

            checkBoxVideoAutoOpen.Text = language.VideoAutoOpen;
            checkBoxAllowVolumeBoost.Text = language.AllowVolumeBoost;

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
            checkBoxWaveformSingleClickSelect.Text = language.WaveformSingleClickSelect;
            checkBoxWaveformSnapToShotChanges.Text = language.WaveformSnapToShotChanges;
            buttonEditShotChangesProfile.Text = language.WaveformEditShotChangesProfile;
            checkBoxWaveformAutoGen.Text = language.WaveformAutoGen;
            labelWaveformBorderHitMs1.Text = language.WaveformBorderHitMs1;
            labelWaveformBorderHitMs2.Text = language.WaveformBorderHitMs2;
            numericUpDownWaveformBorderHitMs.Left = labelWaveformBorderHitMs1.Left + labelWaveformBorderHitMs1.Width;
            labelWaveformBorderHitMs2.Left = numericUpDownWaveformBorderHitMs.Left + numericUpDownWaveformBorderHitMs.Width + 2;

            buttonWaveformGridColor.Text = language.WaveformGridColor;
            buttonWaveformColor.Text = language.WaveformColor;
            buttonWaveformSelectedColor.Text = language.WaveformSelectedColor;
            buttonWaveformTextColor.Text = language.WaveformTextColor;
            buttonWaveformBackgroundColor.Text = language.WaveformBackgroundColor;
            buttonWaveformCursorColor.Text = language.WaveformCursorColor;
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
            checkBoxFfmpegUseCenterChannel.Text = language.WaveformUseCenterChannelOnly;
            buttonDownloadFfmpeg.Text = string.Format(language.DownloadX, "FFmpeg");
            if (!Configuration.IsRunningOnWindows)
            {
                buttonDownloadFfmpeg.Visible = false;
                labelFFmpegPath.Visible = false;
                textBoxFFmpegPath.Visible = false;
                buttonBrowseToFFmpeg.Visible = false;

                buttonMpvSettings.Visible = false;
                radioButtonVideoPlayerDirectShow.Enabled = false;
                radioButtonVideoPlayerMpcHc.Enabled = false;
                var isLibVlcInstalled = LibVlcDynamic.IsInstalled;
                radioButtonVideoPlayerVLC.Enabled = isLibVlcInstalled;
                if (gs.VideoPlayer.Trim().Equals("VLC", StringComparison.OrdinalIgnoreCase) && isLibVlcInstalled)
                {
                    radioButtonVideoPlayerVLC.Checked = true;
                }
                if (LibMpvDynamic.IsInstalled)
                {
                    radioButtonVideoPlayerMPV.Enabled = true;
                    radioButtonVideoPlayerMPV.Checked = !(gs.VideoPlayer.Trim().Equals("VLC", StringComparison.OrdinalIgnoreCase) && isLibVlcInstalled);
                    ShowMpvVideoOutput();
                }
                textBoxVlcPath.Visible = false;
                labelVlcPath.Visible = false;
                buttonVlcPathBrowse.Visible = false;
            }

            labelFFmpegPath.Text = language.WaveformFFmpegPath;

            groupBoxProxySettings.Text = language.ProxyServerSettings;
            labelProxyAddress.Text = language.ProxyAddress;
            groupBoxProxyAuthentication.Text = language.ProxyAuthentication;
            labelProxyUserName.Text = language.ProxyUserName;
            labelProxyPassword.Text = language.ProxyPassword;
            labelProxyDomain.Text = language.ProxyDomain;
            labelProxyAuthType.Text = language.ProxyAuthType;
            checkBoxProxyUseDefaultCredentials.Text = language.ProxyUseDefaultCredentials;

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
            labelDialogStyle.Text = language.DialogStyle;
            labelContinuationStyle.Text = language.ContinuationStyle;
            labelCpsLineLenCalc.Text = language.CpsLineLengthStyle;
            labelToolsMusicSymbol.Text = language.MusicSymbol;
            labelToolsMusicSymbolsToReplace.Text = language.MusicSymbolsReplace;
            checkBoxFixCommonOcrErrorsUsingHardcodedRules.Text = language.FixCommonOcrErrorsUseHardcodedRules;
            checkBoxUseWordSplitList.Text = language.UseWordSplitList;
            checkBoxUseWordSplitListAvoidPropercase.Text = language.AvoidPropercase;
            checkBoxFixShortDisplayTimesAllowMoveStartTime.Text = language.FixCommonerrorsFixShortDisplayTimesAllowMoveStartTime;
            checkBoxFceSkipStep1.Text = language.FixCommonErrorsSkipStepOne;
            groupBoxSpellCheck.Text = language.SpellCheck;
            checkBoxSpellCheckAutoChangeNames.Text = LanguageSettings.Current.SpellCheck.AutoFixNames;
            checkBoxSpellCheckAutoChangeNamesViaSuggestions.Text = LanguageSettings.Current.SpellCheck.AutoFixNamesViaSuggestions;
            checkBoxSpellCheckOneLetterWords.Text = LanguageSettings.Current.SpellCheck.CheckOneLetterWords;
            checkBoxTreatINQuoteAsING.Text = LanguageSettings.Current.SpellCheck.TreatINQuoteAsING;
            checkBoxUseAlwaysToFile.Text = LanguageSettings.Current.SpellCheck.RememberUseAlwaysList;
            checkBoxLiveSpellCheck.Text = LanguageSettings.Current.SpellCheck.LiveSpellCheck;
            buttonFixContinuationStyleSettings.Text = language.EditFixContinuationStyleSettings;

            groupBoxToolsAutoBr.Text = LanguageSettings.Current.Main.Controls.AutoBreak.Replace("&", string.Empty);
            checkBoxUseDoNotBreakAfterList.Text = language.UseDoNotBreakAfterList;
            checkBoxToolsBreakEarlyComma.Text = language.BreakEarlyForComma;
            checkBoxToolsBreakEarlyDash.Text = language.BreakEarlyForDashDialog;
            checkBoxToolsBreakEarlyLineEnding.Text = language.BreakEarlyForLineEnding;
            checkBoxToolsBreakByPixelWidth.Text = language.BreakByPixelWidth;
            checkBoxToolsBreakPreferBottomHeavy.Text = language.BreakPreferBottomHeavy;
            numericUpDownToolsBreakPreferBottomHeavy.Left = checkBoxToolsBreakPreferBottomHeavy.Left + checkBoxToolsBreakPreferBottomHeavy.Width + 9;
            labelToolsBreakBottomHeavyPercent.Left = numericUpDownToolsBreakPreferBottomHeavy.Left + numericUpDownToolsBreakPreferBottomHeavy.Width + 2;
            buttonEditDoNotBreakAfterList.Text = LanguageSettings.Current.VobSubOcr.Edit;

            groupBoxMiscellaneous.Text = language.Miscellaneous;
            labelBDOpensIn.Text = language.BDOpensIn;
            comboBoxBDOpensIn.Left = labelBDOpensIn.Left + labelBDOpensIn.Width + 5;
            comboBoxBDOpensIn.Items.Clear();
            comboBoxBDOpensIn.Items.Add(language.BDOpensInOcr);
            comboBoxBDOpensIn.Items.Add(language.BDOpensInEdit);
            comboBoxBDOpensIn.SelectedIndex = Configuration.Settings.Tools.BDOpenIn == "EDIT" ? 1 : 0;

            checkBoxShortcutsAllowLetterOrNumberInTextBox.Text = language.ShortcutsAllowSingleLetterOrNumberInTextBox;
            checkBoxShortcutsAllowLetterOrNumberInTextBox.Checked = Configuration.Settings.General.AllowLetterShortcutsInTextBox;

            labelShortcutCustomToggle.Text = language.ShortcutCustomToggle;
            comboBoxCustomToggleStart.Left = labelShortcutCustomToggle.Right + 5;
            comboBoxCustomToggleEnd.Left = comboBoxCustomToggleStart.Right + 5;
            var customTags = Configuration.Settings.General.TagsInToggleCustomTags.Split('Æ');
            switch (customTags.Length)
            {
                case 1:
                    comboBoxCustomToggleStart.Text = customTags[0];
                    comboBoxCustomToggleEnd.Text = customTags[0];
                    break;
                case 2:
                    comboBoxCustomToggleStart.Text = customTags[0];
                    comboBoxCustomToggleEnd.Text = customTags[1];
                    break;
            }

            groupBoxGoogleTranslate.Text = language.GoogleTranslate;
            labelGoogleTranslateApiKey.Text = language.GoogleTranslateApiKey;
            linkLabelGoogleTranslateSignUp.Text = language.HowToSignUp;
            linkLabelGoogleTranslateSignUp.Left = textBoxGoogleTransleApiKey.Left + textBoxGoogleTransleApiKey.Width - linkLabelGoogleTranslateSignUp.Width;

            labelLibreApiKey.Text = language.GoogleTranslateApiKey;
            nikseTextBoxLibreTranslateApiKey.Left = labelLibreApiKey.Right + 3;
            nikseTextBoxLibreTranslateApiKey.Width = nikseTextBoxLibreTranslateUrl.Width - labelLibreApiKey.Width - 3;

            labelLibreUrl.Text = LanguageSettings.Current.Main.Url;
            labelNllbApiUrl.Text = LanguageSettings.Current.Main.Url;
            labelNllbServeUrl.Text = LanguageSettings.Current.Main.Url;
            labelUrlChatGpt.Text = LanguageSettings.Current.Main.Url;
            labelMyMemoryApiKey.Text = language.GoogleTranslateApiKey;
            labelDeepLUrl.Text = LanguageSettings.Current.Main.Url;
            labelDeepLApiKey.Text = language.GoogleTranslateApiKey;
            nikseTextBoxDeepLApiKey.Left = labelDeepLApiKey.Right + 3;
            nikseTextBoxDeepLApiKey.Width = nikseTextBoxDeepLUrl.Width - labelDeepLApiKey.Width - 3;

            labelApiKeyChatGpt.Text = language.GoogleTranslateApiKey;
            labelChatGptModel.Text = LanguageSettings.Current.AudioToText.Model;
            nikseTextBoxChatGptApiKey.Left = labelApiKeyChatGpt.Right + 3;
            nikseTextBoxChatGptApiKey.Width = nikseTextBoxChatGptUrl.Width - labelApiKeyChatGpt.Width - 3;
            nikseComboBoxChatGptModel.Left = labelChatGptModel.Right + 3;
            nikseComboBoxChatGptModel.Width = nikseTextBoxChatGptUrl.Width - labelChatGptModel.Width - 3;

            nikseTextBoxPapagoClientSecret.Left = labelSecretPapago.Right + 3;
            nikseTextBoxPapagoClientSecret.Width = nikseTextBoxPapagoClientId.Width - labelSecretPapago.Width - 3;

            groupBoxBing.Text = language.MicrosoftBingTranslator;
            labelBingApiKey.Text = language.MicrosoftTranslateApiKey;
            labelBingTokenEndpoint.Text = language.MicrosoftTranslateTokenEndpoint;
            comboBoxBoxBingTokenEndpoint.Left = labelBingTokenEndpoint.Right + 3;
            comboBoxBoxBingTokenEndpoint.Width = textBoxBingClientSecret.Width - labelBingTokenEndpoint.Width - 3;
            linkLabelBingSubscribe.Text = language.HowToSignUp;
            linkLabelBingSubscribe.Left = textBoxBingClientSecret.Left + textBoxBingClientSecret.Width - linkLabelGoogleTranslateSignUp.Width;

            groupBoxNllbServe.Text = new NoLanguageLeftBehindServe().Name;
            groupBoxNllbApi.Text = new NoLanguageLeftBehindApi().Name;
            groupBoxLibreTranslate.Text = new LibreTranslate().Name;

            linkLabelNllbServe.Text = LanguageSettings.Current.General.MoreInfo;
            linkLabelNllbApi.Text = LanguageSettings.Current.General.MoreInfo;
            linkLabelLibreTranslateApi.Text = LanguageSettings.Current.General.MoreInfo;
            linkLabelMoreInfoDeepl.Text = LanguageSettings.Current.General.MoreInfo;
            linkLabelMyMemoryTranslate.Text = LanguageSettings.Current.General.MoreInfo;
            linkLabelPapago.Text = LanguageSettings.Current.General.MoreInfo;
            linkLabelMoreInfoChatGpt.Text = LanguageSettings.Current.General.MoreInfo;

            labelNllbServeModel.Text = LanguageSettings.Current.AudioToText.Model;
            nikseTextBoxNllbServeModel.Left = labelNllbServeModel.Right + 3;
            nikseTextBoxNllbServeModel.Width = nikseTextBoxNllbServeUrl.Width - labelNllbServeModel.Width - 3;

            comboBoxListViewDoubleClickEvent.Items.Clear();
            comboBoxListViewDoubleClickEvent.Items.Add(language.MainListViewNothing);
            comboBoxListViewDoubleClickEvent.Items.Add(language.MainListViewVideoGoToPositionAndPause);
            comboBoxListViewDoubleClickEvent.Items.Add(language.MainListViewVideoGoToPositionAndPlay);
            comboBoxListViewDoubleClickEvent.Items.Add(language.MainListViewVideoGoToPositionMinusHalfSecAndPause);
            comboBoxListViewDoubleClickEvent.Items.Add(language.MainListViewVideoGoToPositionMinus1SecAndPause);
            comboBoxListViewDoubleClickEvent.Items.Add(language.MainListViewVideoGoToPositionMinus1SecAndPlay);
            comboBoxListViewDoubleClickEvent.Items.Add(language.MainListViewEditTextAndPause);
            comboBoxListViewDoubleClickEvent.Items.Add(language.MainListViewEditText);
            comboBoxListViewDoubleClickEvent.Items.Add(language.MainListViewVideoGoToPositionAndPlayCurrentAndPause);

            comboBoxSaveAsFileNameFrom.Items.Clear();
            comboBoxSaveAsFileNameFrom.Items.Add(language.VideoFileName);
            comboBoxSaveAsFileNameFrom.Items.Add(language.ExistingFileName);

            comboBoxTranslationAutoSuffix.Items.Clear();
            comboBoxTranslationAutoSuffix.Items.Add("<" + LanguageSettings.Current.ImportText.Auto + ">");
            foreach (var suffix in Configuration.Settings.General.TranslationAutoSuffixes.Split(';'))
            {
                if (suffix.StartsWith('<'))
                {
                    continue;
                }

                comboBoxTranslationAutoSuffix.Items.Add(suffix);
                if (suffix == Configuration.Settings.General.TranslationAutoSuffixDefault)
                {
                    comboBoxTranslationAutoSuffix.SelectedIndex = comboBoxTranslationAutoSuffix.Items.Count - 1;
                }
            }
            if (comboBoxTranslationAutoSuffix.SelectedIndex < 0)
            {
                comboBoxTranslationAutoSuffix.SelectedIndex = 0;
            }

            if (gs.ListViewDoubleClickAction >= 0 && gs.ListViewDoubleClickAction < comboBoxListViewDoubleClickEvent.Items.Count)
            {
                comboBoxListViewDoubleClickEvent.SelectedIndex = gs.ListViewDoubleClickAction;
            }

            comboBoxSaveAsFileNameFrom.SelectedIndex = gs.SaveAsUseFileNameFrom.Equals("video", StringComparison.OrdinalIgnoreCase) ? 0 : 1;

            if (gs.AutoBackupSeconds == 60)
            {
                comboBoxAutoBackup.SelectedIndex = 1;
            }
            else if (gs.AutoBackupSeconds == 60 * 5)
            {
                comboBoxAutoBackup.SelectedIndex = 2;
            }
            else if (gs.AutoBackupSeconds == 60 * 15)
            {
                comboBoxAutoBackup.SelectedIndex = 3;
            }
            else
            {
                comboBoxAutoBackup.SelectedIndex = 0;
            }

            var deleteAfterIdx = gs.AutoBackupDeleteAfterMonths - 1;
            if (deleteAfterIdx >= comboBoxAutoBackupDeleteAfter.Items.Count - 1)
            {
                comboBoxAutoBackupDeleteAfter.SelectedIndex = comboBoxAutoBackupDeleteAfter.Items.Count - 1;
            }
            else
            {
                comboBoxAutoBackupDeleteAfter.SelectedIndex = deleteAfterIdx;
            }

            checkBoxCheckForUpdates.Checked = gs.CheckForUpdates;
            checkBoxAutoSave.Checked = gs.AutoSave;

            comboBoxSpellChecker.SelectedIndex = gs.SpellChecker.Contains("word", StringComparison.OrdinalIgnoreCase) ? 1 : 0;

            if (Configuration.IsRunningOnLinux || Configuration.IsRunningOnMac)
            {
                comboBoxSpellChecker.SelectedIndex = 0;
                comboBoxSpellChecker.Enabled = false;
            }

            checkBoxAllowEditOfOriginalSubtitle.Checked = gs.AllowEditOfOriginalSubtitle;
            checkBoxPromptDeleteLines.Checked = gs.PromptDeleteLines;

            var toolsSettings = Configuration.Settings.Tools;
            if (toolsSettings.VerifyPlaySeconds - 2 >= 0 && toolsSettings.VerifyPlaySeconds - 2 < comboBoxToolsVerifySeconds.Items.Count)
            {
                comboBoxToolsVerifySeconds.SelectedIndex = toolsSettings.VerifyPlaySeconds - 2;
            }
            else
            {
                comboBoxToolsVerifySeconds.SelectedIndex = 0;
            }

            if (toolsSettings.StartSceneIndex >= 0 && toolsSettings.StartSceneIndex < comboBoxToolsStartSceneIndex.Items.Count)
            {
                comboBoxToolsStartSceneIndex.SelectedIndex = toolsSettings.StartSceneIndex;
            }
            else
            {
                comboBoxToolsStartSceneIndex.SelectedIndex = 0;
            }

            if (toolsSettings.EndSceneIndex >= 0 && toolsSettings.EndSceneIndex < comboBoxToolsEndSceneIndex.Items.Count)
            {
                comboBoxToolsEndSceneIndex.SelectedIndex = toolsSettings.EndSceneIndex;
            }
            else
            {
                comboBoxToolsEndSceneIndex.SelectedIndex = 0;
            }

            comboBoxMergeShortLineLength.Items.Clear();
            var comboBoxMergeShortLineLengthList = new List<string>(100);
            for (int i = 1; i < 100; i++)
            {
                comboBoxMergeShortLineLengthList.Add(i.ToString(CultureInfo.InvariantCulture));
            }
            comboBoxMergeShortLineLength.Items.AddRange(comboBoxMergeShortLineLengthList.ToArray<object>());
            comboBoxMergeShortLineLength.SelectedIndex = 0;
            var selMatch = gs.MergeLinesShorterThan.ToString();
            for (int i = 0; i < comboBoxMergeShortLineLength.Items.Count; i++)
            {
                object item = comboBoxMergeShortLineLength.Items[i];
                if (item.ToString() == selMatch)
                {
                    comboBoxMergeShortLineLength.SelectedIndex = i;
                    break;
                }
            }

            SetDialogStyle(Configuration.Settings.General.DialogStyle);
            SetCpsLineLengthStyle(Configuration.Settings.General.CpsLineLengthStrategy);

            buttonEditCustomContinuationStyle.Visible = Configuration.Settings.General.ContinuationStyle == ContinuationStyle.Custom;
            comboBoxContinuationStyle.Width = Configuration.Settings.General.ContinuationStyle == ContinuationStyle.Custom ? (buttonEditCustomContinuationStyle.Left - comboBoxContinuationStyle.Left - 6) : (comboBoxDialogStyle.Right - comboBoxContinuationStyle.Left);

            UpdateProfileNames(gs.Profiles);

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

            textBoxMusicSymbolsToReplace.Text = toolsSettings.MusicSymbolReplace;
            checkBoxFixCommonOcrErrorsUsingHardcodedRules.Checked = toolsSettings.OcrFixUseHardcodedRules;
            checkBoxUseWordSplitList.Checked = toolsSettings.OcrUseWordSplitList;
            checkBoxUseWordSplitListAvoidPropercase.Checked = toolsSettings.OcrUseWordSplitListAvoidPropercase;
            checkBoxFixShortDisplayTimesAllowMoveStartTime.Checked = toolsSettings.FixShortDisplayTimesAllowMoveStartTime;
            checkBoxFceSkipStep1.Checked = toolsSettings.FixCommonErrorsSkipStepOne;
            checkBoxSpellCheckAutoChangeNames.Checked = toolsSettings.SpellCheckAutoChangeNameCasing;
            checkBoxSpellCheckAutoChangeNamesViaSuggestions.Checked = toolsSettings.SpellCheckAutoChangeNamesUseSuggestions;
            checkBoxSpellCheckOneLetterWords.Checked = toolsSettings.CheckOneLetterWords;
            checkBoxTreatINQuoteAsING.Checked = toolsSettings.SpellCheckEnglishAllowInQuoteAsIng;
            checkBoxUseAlwaysToFile.Checked = toolsSettings.RememberUseAlwaysList;
            checkBoxLiveSpellCheck.Checked = toolsSettings.LiveSpellCheck;
            checkBoxSubtitleTextBoxSyntaxColor_CheckedChanged(this, EventArgs.Empty);
            checkBoxUseDoNotBreakAfterList.Checked = toolsSettings.UseNoLineBreakAfter;
            checkBoxToolsBreakEarlyComma.Checked = toolsSettings.AutoBreakCommaBreakEarly;
            checkBoxToolsBreakEarlyDash.Checked = toolsSettings.AutoBreakDashEarly;
            checkBoxToolsBreakEarlyLineEnding.Checked = toolsSettings.AutoBreakLineEndingEarly;
            checkBoxToolsBreakByPixelWidth.Checked = toolsSettings.AutoBreakUsePixelWidth;
            checkBoxToolsBreakPreferBottomHeavy.Checked = toolsSettings.AutoBreakPreferBottomHeavy;
            numericUpDownToolsBreakPreferBottomHeavy.Value = (decimal)toolsSettings.AutoBreakPreferBottomPercent;

            textBoxBingClientSecret.Text = Configuration.Settings.Tools.MicrosoftTranslatorApiKey;
            comboBoxBoxBingTokenEndpoint.Text = Configuration.Settings.Tools.MicrosoftTranslatorTokenEndpoint;
            textBoxGoogleTransleApiKey.Text = toolsSettings.GoogleApiV2Key;
            nikseTextBoxNllbApiUrl.Text = Configuration.Settings.Tools.AutoTranslateNllbApiUrl;
            nikseTextBoxNllbServeUrl.Text = Configuration.Settings.Tools.AutoTranslateNllbServeUrl;
            nikseTextBoxNllbServeModel.Text = Configuration.Settings.Tools.AutoTranslateNllbServeModel;
            nikseTextBoxLibreTranslateUrl.Text = Configuration.Settings.Tools.AutoTranslateLibreUrl;
            nikseTextBoxLibreTranslateApiKey.Text = Configuration.Settings.Tools.AutoTranslateLibreApiKey;
            nikseTextBoxMyMemoryApiKey.Text = Configuration.Settings.Tools.AutoTranslateMyMemoryApiKey;
            nikseTextBoxDeepLUrl.Text = Configuration.Settings.Tools.AutoTranslateDeepLUrl;
            nikseTextBoxDeepLApiKey.Text = Configuration.Settings.Tools.AutoTranslateDeepLApiKey;
            nikseTextBoxChatGptUrl.Text = Configuration.Settings.Tools.ChatGptUrl;
            nikseTextBoxChatGptApiKey.Text = Configuration.Settings.Tools.ChatGptApiKey;

            nikseComboBoxChatGptModel.Text = Configuration.Settings.Tools.ChatGptModel;
            if (string.IsNullOrEmpty(nikseComboBoxChatGptModel.Text))
            {
                nikseComboBoxChatGptModel.Text = "gpt-3.5-turbo";
            }

            nikseTextBoxPapagoClientId.Text = Configuration.Settings.Tools.AutoTranslatePapagoApiKeyId;
            nikseTextBoxPapagoClientSecret.Text = Configuration.Settings.Tools.AutoTranslatePapagoApiKey;

            buttonReset.Text = LanguageSettings.Current.Settings.RestoreDefaultSettings;
            buttonOK.Text = LanguageSettings.Current.General.Ok;
            buttonCancel.Text = LanguageSettings.Current.General.Cancel;

            checkBoxWaveformShowGrid.Checked = Configuration.Settings.VideoControls.WaveformDrawGrid;
            checkBoxWaveformShowCps.Checked = Configuration.Settings.VideoControls.WaveformDrawCps;
            checkBoxWaveformShowWpm.Checked = Configuration.Settings.VideoControls.WaveformDrawWpm;
            panelWaveformGridColor.BackColor = Configuration.Settings.VideoControls.WaveformGridColor;
            panelWaveformSelectedColor.BackColor = Configuration.Settings.VideoControls.WaveformSelectedColor;
            panelWaveformColor.BackColor = Configuration.Settings.VideoControls.WaveformColor;
            panelWaveformBackgroundColor.BackColor = Configuration.Settings.VideoControls.WaveformBackgroundColor;
            panelWaveformTextColor.BackColor = Configuration.Settings.VideoControls.WaveformTextColor;
            panelWaveformCursorColor.BackColor = Configuration.Settings.VideoControls.WaveformCursorColor;
            checkBoxGenerateSpectrogram.Checked = Configuration.Settings.VideoControls.GenerateSpectrogram;
            comboBoxSpectrogramAppearance.SelectedIndex = Configuration.Settings.VideoControls.SpectrogramAppearance == "OneColorGradient" ? 0 : 1;
            comboBoxWaveformTextSize.Text = Configuration.Settings.VideoControls.WaveformTextSize.ToString(CultureInfo.InvariantCulture);
            checkBoxWaveformTextBold.Checked = Configuration.Settings.VideoControls.WaveformTextBold;
            checkBoxReverseMouseWheelScrollDirection.Checked = Configuration.Settings.VideoControls.WaveformMouseWheelScrollUpIsForward;
            checkBoxAllowOverlap.Checked = Configuration.Settings.VideoControls.WaveformAllowOverlap;
            checkBoxWaveformSetVideoPosMoveStartEnd.Checked = Configuration.Settings.VideoControls.WaveformSetVideoPositionOnMoveStartEnd;
            checkBoxWaveformHoverFocus.Checked = Configuration.Settings.VideoControls.WaveformFocusOnMouseEnter;
            checkBoxListViewMouseEnterFocus.Checked = Configuration.Settings.VideoControls.WaveformListViewFocusOnMouseEnter;
            checkBoxWaveformSingleClickSelect.Checked = Configuration.Settings.VideoControls.WaveformSingleClickSelect;
            checkBoxWaveformSnapToShotChanges.Checked = Configuration.Settings.VideoControls.WaveformSnapToShotChanges;
            checkBoxWaveformAutoGen.Checked = Configuration.Settings.General.WaveformAutoGenWhenOpeningVideo;
            if (Configuration.Settings.VideoControls.WaveformBorderHitMs >= numericUpDownWaveformBorderHitMs.Minimum &&
                Configuration.Settings.VideoControls.WaveformBorderHitMs <= numericUpDownWaveformBorderHitMs.Maximum)
            {
                numericUpDownWaveformBorderHitMs.Value = Configuration.Settings.VideoControls.WaveformBorderHitMs;
            }

            buttonEditShotChangesProfile.Left = checkBoxWaveformSnapToShotChanges.Left + checkBoxWaveformSnapToShotChanges.Width + 16;

            checkBoxUseFFmpeg.Checked = gs.UseFFmpegForWaveExtraction;
            checkBoxFfmpegUseCenterChannel.Checked = gs.FFmpegUseCenterChannelOnly;
            textBoxFFmpegPath.Text = gs.FFmpegLocation;
            if (string.IsNullOrEmpty(textBoxFFmpegPath.Text) && Configuration.IsRunningOnWindows)
            {
                var guessPath = Path.Combine(Configuration.DataDirectory, "ffmpeg", "ffmpeg.exe");
                if (File.Exists(guessPath))
                {
                    textBoxFFmpegPath.Text = guessPath;
                    checkBoxUseFFmpeg.Checked = true;
                }
            }

            toolStripMenuItemShortcutsCollapse.Text = LanguageSettings.Current.General.Collapse;
            importShortcutsToolStripMenuItem.Text = LanguageSettings.Current.MultipleReplace.Import;
            exportShortcutsToolStripMenuItem.Text = LanguageSettings.Current.MultipleReplace.Export;
            exportAsHtmlToolStripMenuItem.Text = LanguageSettings.Current.Settings.ExportAsHtml;
            labelShortcutsSearch.Text = LanguageSettings.Current.General.Search;
            buttonShortcutsClear.Text = LanguageSettings.Current.DvdSubRip.Clear;
            textBoxShortcutSearch.Left = labelShortcutsSearch.Left + labelShortcutsSearch.Width + 5;
            buttonShortcutsClear.Left = textBoxShortcutSearch.Left + textBoxShortcutSearch.Width + 5;
            labelShortcutsFilter.Text = LanguageSettings.Current.BatchConvert.Filter;
            nikseComboBoxShortcutsFilter.Left = labelShortcutsFilter.Right + 5;
            nikseComboBoxShortcutsFilter.SelectedIndex = 0;
            nikseComboBoxShortcutsFilter.Enabled = true;
            nikseComboBoxShortcutsFilter.Items[0] = LanguageSettings.Current.General.All;
            nikseComboBoxShortcutsFilter.Items[1] = LanguageSettings.Current.Settings.Used;
            nikseComboBoxShortcutsFilter.Items[2] = LanguageSettings.Current.Settings.Unused;

            nikseComboBoxShortcutsFilter.DropDownStyle = ComboBoxStyle.DropDownList;

            // Subtitle formats
            groupBoxSubtitleFormats.Text = language.SubtitleFormats;
            labelDefaultSubtitleFormat.Text = language.DefaultFormat;
            labelDefaultSaveAsFormat.Text = language.DefaultSaveAsFormat;
            groupBoxFavoriteSubtitleFormats.Text = language.Favorites;
            labelFavoriteFormats.Text = language.FavoriteFormats;
            buttonRemoveFromFavoriteFormats.Text = LanguageSettings.Current.MultipleReplace.Remove;
            labelFormats.Text = LanguageSettings.Current.ExportCustomText.Formats;
            labelFormatsSearch.Text = LanguageSettings.Current.General.Search;
            buttonFormatsSearchClear.Text = LanguageSettings.Current.DvdSubRip.Clear;
            labelFavoriteSubtitleFormatsNote.Text = language.FavoriteSubtitleFormatsNote;

            deleteToolStripMenuItem.Text = LanguageSettings.Current.MultipleReplace.Remove;
            deleteAllToolStripMenuItem.Text = LanguageSettings.Current.MultipleReplace.RemoveAll;
            moveUpToolStripMenuItem.Text = LanguageSettings.Current.DvdSubRip.MoveUp;
            moveDownToolStripMenuItem.Text = LanguageSettings.Current.DvdSubRip.MoveDown;
            moveToTopToolStripMenuItem.Text = LanguageSettings.Current.MultipleReplace.MoveToTop;
            moveToBottomToolStripMenuItem.Text = LanguageSettings.Current.MultipleReplace.MoveToBottom;

            var cmoboBoxLeft = labelDefaultSubtitleFormat.Right > labelDefaultSaveAsFormat.Right ? labelDefaultSubtitleFormat.Right + 5 : labelDefaultSaveAsFormat.Right + 5;
            comboBoxSubtitleFormats.Left = cmoboBoxLeft;
            comboBoxSubtitleSaveAsFormats.Left = cmoboBoxLeft;
            labelFavoriteFormats.Left = listBoxFavoriteSubtitleFormats.Left;
            labelFormats.Left = listBoxSubtitleFormats.Left;
            if (labelFormatsSearch.Right > textBoxFormatsSearch.Left)
            {
                textBoxFormatsSearch.Left = labelFormatsSearch.Right + 4;
                buttonFormatsSearchClear.Left = textBoxFormatsSearch.Right + 4;
                buttonFormatsSearchClear.Width = listBoxFavoriteSubtitleFormats.Width - labelFormatsSearch.Width - textBoxFormatsSearch.Width - 9;
            }

            UiUtil.InitializeSubtitleFormatComboBox(comboBoxSubtitleFormats, Configuration.Settings.General.DefaultSubtitleFormat);

            var formatNamesForSave = SubtitleFormat.AllSubtitleFormats.Where(format => !format.IsVobSubIndexFile).Select(format => format.FriendlyName).ToList();
            formatNamesForSave.Insert(0, LanguageSettings.Current.Settings.DefaultSaveAsFormatAuto);
            UiUtil.InitializeSubtitleFormatComboBox(comboBoxSubtitleSaveAsFormats, formatNamesForSave, Configuration.Settings.General.DefaultSaveAsFormat);
            if (string.IsNullOrEmpty(Configuration.Settings.General.DefaultSaveAsFormat) || comboBoxSubtitleSaveAsFormats.SelectedIndex == -1)
            {
                comboBoxSubtitleSaveAsFormats.SelectedIndex = 0;
            }

            var formatNames = GetSubtitleFormats();
            listBoxSubtitleFormats.Items.Clear();
            listBoxSubtitleFormats.Items.AddRange(formatNames.ToArray<object>());

            listBoxFavoriteSubtitleFormats.Items.Clear();
            if (!string.IsNullOrEmpty(Configuration.Settings.General.FavoriteSubtitleFormats))
            {
                var favoriteFormats = Configuration.Settings.General.FavoriteSubtitleFormats.Split(';');
                if (favoriteFormats.Length > 0)
                {
                    listBoxFavoriteSubtitleFormats.Items.AddRange(favoriteFormats.ToArray<object>());
                }
            }

            using (var graphics = Graphics.FromHwnd(IntPtr.Zero))
            {
                var comboBoxSubtitleFormatsTextWidth = graphics.MeasureString(comboBoxSubtitleFormats.Text, comboBoxSubtitleFormats.Font);
                if (comboBoxSubtitleFormatsTextWidth.Width > comboBoxSubtitleFormats.Width)
                {
                    comboBoxSubtitleFormats.Width = (int)Math.Round(comboBoxSubtitleFormatsTextWidth.Width + 17.5);
                }

                var comboBoxSubtitleSaveFormatsTextWidth = graphics.MeasureString(comboBoxSubtitleSaveAsFormats.Text, comboBoxSubtitleSaveAsFormats.Font);
                if (comboBoxSubtitleSaveFormatsTextWidth.Width > comboBoxSubtitleSaveAsFormats.Width)
                {
                    comboBoxSubtitleSaveAsFormats.Width = (int)Math.Round(comboBoxSubtitleSaveFormatsTextWidth.Width + 17.5);
                }
            }

            // Shortcuts
            groupBoxShortcuts.Text = language.Shortcuts;
            labelShortcut.Text = language.Shortcut;
            checkBoxShortcutsControl.Text = language.Control;
            checkBoxShortcutsAlt.Text = language.Alt;
            checkBoxShortcutsShift.Text = language.Shift;
            buttonUpdateShortcut.Text = language.UpdateShortcut;
            buttonClearShortcut.Text = LanguageSettings.Current.DvdSubRip.Clear;

            labelShortcutKey.Text = language.Key;
            comboBoxShortcutKey.Left = labelShortcutKey.Left + labelShortcutKey.Width;
            comboBoxShortcutKey.Items[0] = LanguageSettings.Current.General.None;

            groupBoxListViewSyntaxColoring.Text = language.ListViewSyntaxColoring;
            checkBoxSyntaxColorDurationTooSmall.Text = language.SyntaxColorDurationIfTooSmall;
            checkBoxSyntaxColorDurationTooLarge.Text = language.SyntaxColorDurationIfTooLarge;
            checkBoxSyntaxColorTextTooLong.Text = language.SyntaxColorTextIfTooLong;
            checkBoxSyntaxColorTextTooWide.Text = language.SyntaxColorTextIfTooWide;
            buttonLineWidthSettings.Text = language.SyntaxLineWidthSettings;
            checkBoxSyntaxColorTextMoreThanTwoLines.Text = string.Format(language.SyntaxColorTextMoreThanMaxLines, Configuration.Settings.General.MaxNumberOfLines);
            checkBoxSyntaxOverlap.Text = language.SyntaxColorOverlap;
            checkBoxSyntaxColorGapTooSmall.Text = language.SyntaxColorGap;
            buttonListViewSyntaxColorError.Text = language.SyntaxErrorColor;

            UiUtil.FixLargeFonts(this, buttonOK);

            checkBoxShortcutsControl.Left = labelShortcut.Left + labelShortcut.Width + 9;
            checkBoxShortcutsAlt.Left = checkBoxShortcutsControl.Left + checkBoxShortcutsControl.Width + 9;
            checkBoxShortcutsShift.Left = checkBoxShortcutsAlt.Left + checkBoxShortcutsAlt.Width + 9;
            labelShortcutKey.Left = checkBoxShortcutsShift.Left + checkBoxShortcutsShift.Width + 9;
            comboBoxShortcutKey.Left = labelShortcutKey.Left + labelShortcutKey.Width + 2;
            buttonUpdateShortcut.Left = comboBoxShortcutKey.Left + comboBoxShortcutKey.Width + 15;
            buttonClearShortcut.Left = buttonUpdateShortcut.Left + buttonUpdateShortcut.Width + 15;
            buttonLineWidthSettings.Left = checkBoxSyntaxColorTextTooWide.Left + checkBoxSyntaxColorTextTooWide.Width + 9;

            _oldVlcLocation = gs.VlcLocation;
            _oldVlcLocationRelative = gs.VlcLocationRelative;

            _oldListViewShowCps = Configuration.Settings.Tools.ListViewShowColumnCharsPerSec;
            _oldListViewShowWpm = Configuration.Settings.Tools.ListViewShowColumnWordsPerMin;

            labelPlatform.Text = (IntPtr.Size * 8) + "-bit";

            buttonUpdateFileTypeAssociations.Text = language.UpdateFileTypeAssociations;
            labelUpdateFileTypeAssociationsStatus.Text = string.Empty;

            numericUpDownMpvOutline.Left = radioButtonMpvOutline.Right + 9;
            numericUpDownMpvShadowWidth.Left = numericUpDownMpvOutline.Right + 9;
            labelMpvShadow.Left = numericUpDownMpvShadowWidth.Left;

            _defaultLanguages = Configuration.Settings.General.DefaultLanguages ?? string.Empty;
            InitLanguageList();

            checkBoxDarkThemeEnabled_CheckedChanged(null, null);

            ToolbarIconThemeInit();
        }

        private void ToolbarIconThemeInit()
        {
            if (!Directory.Exists(Configuration.IconsDirectory))
            {
                comboBoxToolbarIconTheme.Visible = false;
                labelToolbarIconTheme.Visible = false;
                return;
            }

            comboBoxToolbarIconTheme.SelectedIndexChanged -= comboBoxToolbarIconTheme_SelectedIndexChanged;
            var directories = Directory.GetDirectories(Configuration.IconsDirectory);
            comboBoxToolbarIconTheme.Items.Clear();
            comboBoxToolbarIconTheme.Items.Add($"- {LanguageSettings.Current.Settings.Automatic} -");
            comboBoxToolbarIconTheme.SelectedIndex = 0;
            foreach (var dir in directories)
            {
                if (File.Exists(Path.Combine(dir, "new.png")))
                {
                    var d = Path.GetFileName(dir);
                    comboBoxToolbarIconTheme.Items.Add(d);
                    if (Configuration.Settings.General.ToolbarIconTheme != null &&
                        Configuration.Settings.General.ToolbarIconTheme.Equals(d, StringComparison.InvariantCultureIgnoreCase))
                    {
                        comboBoxToolbarIconTheme.SelectedIndex = comboBoxToolbarIconTheme.Items.Count - 1;
                    }
                }
            }

            comboBoxToolbarIconTheme.SelectedIndexChanged += comboBoxToolbarIconTheme_SelectedIndexChanged;
        }

        private void ShowMpvVideoOutput()
        {
            if (!Configuration.IsRunningOnLinux || string.IsNullOrEmpty(Configuration.Settings.General.MpvVideoOutputLinux))
            {
                labelMpvSettings.Text = string.Empty;
            }
            else
            {
                labelMpvSettings.Text = "--vo=" + Configuration.Settings.General.MpvVideoOutputLinux;
            }
        }

        private void SetDialogStyle(DialogType dialogStyle)
        {
            comboBoxDialogStyle.Items.Clear();
            comboBoxDialogStyle.Items.Add(LanguageSettings.Current.Settings.DialogStyleDashBothLinesWithSpace);
            comboBoxDialogStyle.Items.Add(LanguageSettings.Current.Settings.DialogStyleDashBothLinesWithoutSpace);
            comboBoxDialogStyle.Items.Add(LanguageSettings.Current.Settings.DialogStyleDashSecondLineWithSpace);
            comboBoxDialogStyle.Items.Add(LanguageSettings.Current.Settings.DialogStyleDashSecondLineWithoutSpace);
            comboBoxDialogStyle.SelectedIndex = 0;
            toolTipDialogStylePreview.RemoveAll();
            switch (dialogStyle)
            {
                case DialogType.DashBothLinesWithSpace:
                    comboBoxDialogStyle.SelectedIndex = 0;
                    toolTipDialogStylePreview.SetToolTip(comboBoxDialogStyle, DialogSplitMerge.GetDialogStylePreview(DialogType.DashBothLinesWithSpace));
                    break;
                case DialogType.DashBothLinesWithoutSpace:
                    comboBoxDialogStyle.SelectedIndex = 1;
                    toolTipDialogStylePreview.SetToolTip(comboBoxDialogStyle, DialogSplitMerge.GetDialogStylePreview(DialogType.DashBothLinesWithoutSpace));
                    break;
                case DialogType.DashSecondLineWithSpace:
                    comboBoxDialogStyle.SelectedIndex = 2;
                    toolTipDialogStylePreview.SetToolTip(comboBoxDialogStyle, DialogSplitMerge.GetDialogStylePreview(DialogType.DashSecondLineWithSpace));
                    break;
                case DialogType.DashSecondLineWithoutSpace:
                    comboBoxDialogStyle.SelectedIndex = 3;
                    toolTipDialogStylePreview.SetToolTip(comboBoxDialogStyle, DialogSplitMerge.GetDialogStylePreview(DialogType.DashSecondLineWithoutSpace));
                    break;
            }
        }

        private void SetCpsLineLengthStyle(string cpsLineLengthStyle)
        {
            comboBoxCpsLineLenCalc.Items.Clear();
            foreach (var calc in CpsLineLength.List())
            {
                comboBoxCpsLineLenCalc.Items.Add(calc);
                if (cpsLineLengthStyle == calc.Code)
                {
                    comboBoxCpsLineLenCalc.SelectedIndex = comboBoxCpsLineLenCalc.Items.Count - 1;
                }
            }

            if (comboBoxCpsLineLenCalc.SelectedIndex < 0)
            {
                comboBoxCpsLineLenCalc.SelectedIndex = 0;
            }
        }

        private void SetContinuationStyle(ContinuationStyle continuationStyle)
        {
            comboBoxContinuationStyle.Items.Clear();
            foreach (var style in ContinuationUtilities.ContinuationStyles)
            {
                comboBoxContinuationStyle.Items.Add(UiUtil.GetContinuationStyleName(style));
            }
            comboBoxContinuationStyle.SelectedIndex = 0;
            toolTipContinuationPreview.RemoveAll();
            toolTipContinuationPreview.SetToolTip(comboBoxContinuationStyle, ContinuationUtilities.GetContinuationStylePreview(continuationStyle));
            comboBoxContinuationStyle.SelectedIndex = ContinuationUtilities.GetIndexFromContinuationStyle(continuationStyle);

            var dropDownContinuationWidth = comboBoxContinuationStyle.Width;
            var dropDownDialogStyleWidth = comboBoxDialogStyle.Width;
            using (var g = Graphics.FromHwnd(IntPtr.Zero))
            {
                foreach (var item in comboBoxContinuationStyle.Items)
                {
                    var itemWidth = (int)g.MeasureString((string)item, Font).Width + 5;
                    dropDownContinuationWidth = Math.Max(itemWidth, dropDownContinuationWidth);
                }

                foreach (var item in comboBoxDialogStyle.Items)
                {
                    var itemWidth = (int)g.MeasureString((string)item, Font).Width + 5;
                    dropDownDialogStyleWidth = Math.Max(itemWidth, dropDownDialogStyleWidth);
                }
            }

            comboBoxContinuationStyle.DropDownWidth = dropDownContinuationWidth;
            comboBoxDialogStyle.DropDownWidth = dropDownDialogStyleWidth;
        }

        private Guid _oldProfileId = Guid.Empty;
        private void UpdateProfileNames(List<RulesProfile> profiles)
        {
            comboBoxRulesProfileName.BeginUpdate();
            comboBoxRulesProfileName.Items.Clear();
            foreach (var profile in profiles)
            {
                comboBoxRulesProfileName.Items.Add(profile.Name);
                if (_oldProfileId == Guid.Empty && profile.Name == Configuration.Settings.General.CurrentProfile || profile.Id == _oldProfileId)
                {
                    comboBoxRulesProfileName.SelectedIndex = comboBoxRulesProfileName.Items.Count - 1;
                    _oldProfileId = profile.Id;
                }
            }
            comboBoxRulesProfileName.EndUpdate();
            if (comboBoxRulesProfileName.SelectedIndex < 0 && comboBoxRulesProfileName.Items.Count > 0 && comboBoxRulesProfileName.Items.Count > 0)
            {
                comboBoxRulesProfileName.SelectedIndex = 0;
            }
        }

        private ShortcutNode _shortcuts = new ShortcutNode("root");

        private void MakeShortcutsTreeView(LanguageStructure.Settings language)
        {
            treeViewShortcuts.Nodes.Clear();
            _newShortcuts.Clear();

            _shortcuts = new ShortcutNode("root");

            var generalNode = new ShortcutNode(LanguageSettings.Current.General.GeneralText);
            AddNode(generalNode, language.MergeSelectedLines, nameof(Configuration.Settings.Shortcuts.GeneralMergeSelectedLines));
            AddNode(generalNode, language.MergeWithPrevious, nameof(Configuration.Settings.Shortcuts.GeneralMergeWithPrevious));
            AddNode(generalNode, language.MergeWithNext, nameof(Configuration.Settings.Shortcuts.GeneralMergeWithNext));
            AddNode(generalNode, language.MergeWithPreviousAndUnbreak, nameof(Configuration.Settings.Shortcuts.GeneralMergeWithPreviousAndUnbreak));
            AddNode(generalNode, language.MergeWithNextAndUnbreak, nameof(Configuration.Settings.Shortcuts.GeneralMergeWithNextAndUnbreak));
            AddNode(generalNode, language.MergeWithPreviousAndBreak, nameof(Configuration.Settings.Shortcuts.GeneralMergeWithPreviousAndBreak));
            AddNode(generalNode, language.MergeWithNextAndBreak, nameof(Configuration.Settings.Shortcuts.GeneralMergeWithNextAndBreak));
            AddNode(generalNode, language.MergeSelectedLinesAndAutoBreak, nameof(Configuration.Settings.Shortcuts.GeneralMergeSelectedLinesAndAutoBreak));
            AddNode(generalNode, language.MergeSelectedLinesAndUnbreak, nameof(Configuration.Settings.Shortcuts.GeneralMergeSelectedLinesAndUnbreak));
            AddNode(generalNode, language.MergeSelectedLinesAndUnbreakCjk, nameof(Configuration.Settings.Shortcuts.GeneralMergeSelectedLinesAndUnbreakCjk));
            AddNode(generalNode, language.MergeSelectedLinesOnlyFirstText, nameof(Configuration.Settings.Shortcuts.GeneralMergeSelectedLinesOnlyFirstText));
            AddNode(generalNode, language.MergeSelectedLinesBilingual, nameof(Configuration.Settings.Shortcuts.GeneralMergeSelectedLinesBilingual));
            AddNode(generalNode, language.MergeWithPreviousBilingual, nameof(Configuration.Settings.Shortcuts.GeneralMergeWithPreviousBilingual));
            AddNode(generalNode, language.MergeWithNextBilingual, nameof(Configuration.Settings.Shortcuts.GeneralMergeWithNextBilingual));
            AddNode(generalNode, language.MergeOriginalAndTranslation, nameof(Configuration.Settings.Shortcuts.GeneralMergeOriginalAndTranslation));
            AddNode(generalNode, language.ToggleTranslationMode, nameof(Configuration.Settings.Shortcuts.GeneralToggleTranslationMode));
            AddNode(generalNode, language.SwitchOriginalAndTranslation, nameof(Configuration.Settings.Shortcuts.GeneralSwitchOriginalAndTranslation));
            AddNode(generalNode, language.SwitchOriginalAndTranslationTextBoxes, nameof(Configuration.Settings.Shortcuts.GeneralSwitchOriginalAndTranslationTextBoxes));
            AddNode(generalNode, LanguageSettings.Current.Main.ChooseLayout, nameof(Configuration.Settings.Shortcuts.GeneralLayoutChoose));
            AddNode(generalNode, string.Format(language.ChooseLayoutX, 1), nameof(Configuration.Settings.Shortcuts.GeneralLayoutChoose1));
            AddNode(generalNode, string.Format(language.ChooseLayoutX, 2), nameof(Configuration.Settings.Shortcuts.GeneralLayoutChoose2));
            AddNode(generalNode, string.Format(language.ChooseLayoutX, 3), nameof(Configuration.Settings.Shortcuts.GeneralLayoutChoose3));
            AddNode(generalNode, string.Format(language.ChooseLayoutX, 4), nameof(Configuration.Settings.Shortcuts.GeneralLayoutChoose4));
            AddNode(generalNode, string.Format(language.ChooseLayoutX, 5), nameof(Configuration.Settings.Shortcuts.GeneralLayoutChoose5));
            AddNode(generalNode, string.Format(language.ChooseLayoutX, 6), nameof(Configuration.Settings.Shortcuts.GeneralLayoutChoose6));
            AddNode(generalNode, string.Format(language.ChooseLayoutX, 7), nameof(Configuration.Settings.Shortcuts.GeneralLayoutChoose7));
            AddNode(generalNode, string.Format(language.ChooseLayoutX, 8), nameof(Configuration.Settings.Shortcuts.GeneralLayoutChoose8));
            AddNode(generalNode, string.Format(language.ChooseLayoutX, 9), nameof(Configuration.Settings.Shortcuts.GeneralLayoutChoose9));
            AddNode(generalNode, string.Format(language.ChooseLayoutX, 10), nameof(Configuration.Settings.Shortcuts.GeneralLayoutChoose10));
            AddNode(generalNode, string.Format(language.ChooseLayoutX, 11), nameof(Configuration.Settings.Shortcuts.GeneralLayoutChoose11));
            AddNode(generalNode, string.Format(language.ChooseLayoutX, 12), nameof(Configuration.Settings.Shortcuts.GeneralLayoutChoose12));
            AddNode(generalNode, language.WaveformPlayFirstSelectedSubtitle, nameof(Configuration.Settings.Shortcuts.GeneralPlayFirstSelected));
            AddNode(generalNode, language.GoToFirstSelectedLine, nameof(Configuration.Settings.Shortcuts.GeneralGoToFirstSelectedLine));
            AddNode(generalNode, language.GoToNextEmptyLine, nameof(Configuration.Settings.Shortcuts.GeneralGoToNextEmptyLine));
            AddNode(generalNode, language.GoToNext, nameof(Configuration.Settings.Shortcuts.GeneralGoToNextSubtitle));
            AddNode(generalNode, language.GoToNextPlayTranslate, nameof(Configuration.Settings.Shortcuts.GeneralGoToNextSubtitlePlayTranslate));
            AddNode(generalNode, language.GoToNextCursorAtEnd, nameof(Configuration.Settings.Shortcuts.GeneralGoToNextSubtitleCursorAtEnd));
            AddNode(generalNode, language.GoToPrevious, nameof(Configuration.Settings.Shortcuts.GeneralGoToPrevSubtitle));
            AddNode(generalNode, language.GoToPreviousPlayTranslate, nameof(Configuration.Settings.Shortcuts.GeneralGoToPrevSubtitlePlayTranslate));
            AddNode(generalNode, language.GoToCurrentSubtitleStart, nameof(Configuration.Settings.Shortcuts.GeneralGoToStartOfCurrentSubtitle));
            AddNode(generalNode, language.GoToCurrentSubtitleEnd, nameof(Configuration.Settings.Shortcuts.GeneralGoToEndOfCurrentSubtitle));
            AddNode(generalNode, language.GoToPreviousSubtitleAndFocusVideo, nameof(Configuration.Settings.Shortcuts.GeneralGoToPreviousSubtitleAndFocusVideo));
            AddNode(generalNode, language.GoToNextSubtitleAndFocusVideo, nameof(Configuration.Settings.Shortcuts.GeneralGoToNextSubtitleAndFocusVideo));
            AddNode(generalNode, language.GoToPrevSubtitleAndPlay, nameof(Configuration.Settings.Shortcuts.GeneralGoToPrevSubtitleAndPlay));
            AddNode(generalNode, language.GoToNextSubtitleAndPlay, nameof(Configuration.Settings.Shortcuts.GeneralGoToNextSubtitleAndPlay));
            AddNode(generalNode, language.GoToPreviousSubtitleAndFocusWaveform, nameof(Configuration.Settings.Shortcuts.GeneralGoToPreviousSubtitleAndFocusWaveform));
            AddNode(generalNode, language.GoToNextSubtitleAndFocusWaveform, nameof(Configuration.Settings.Shortcuts.GeneralGoToNextSubtitleAndFocusWaveform));
            AddNode(generalNode, language.ToggleBookmarks, nameof(Configuration.Settings.Shortcuts.GeneralToggleBookmarks));
            AddNode(generalNode, language.FocusTextBox, nameof(Configuration.Settings.Shortcuts.GeneralFocusTextBox));
            AddNode(generalNode, language.ToggleBookmarksWithComment, nameof(Configuration.Settings.Shortcuts.GeneralToggleBookmarksWithText), true);
            AddNode(generalNode, LanguageSettings.Current.Bookmarks.EditBookmark, nameof(Configuration.Settings.Shortcuts.GeneralEditBookmarks), true);
            AddNode(generalNode, language.ClearBookmarks, nameof(Configuration.Settings.Shortcuts.GeneralClearBookmarks));
            AddNode(generalNode, language.GoToBookmark, nameof(Configuration.Settings.Shortcuts.GeneralGoToBookmark));
            AddNode(generalNode, language.GoToPreviousBookmark, nameof(Configuration.Settings.Shortcuts.GeneralGoToPreviousBookmark));
            AddNode(generalNode, language.GoToNextBookmark, nameof(Configuration.Settings.Shortcuts.GeneralGoToNextBookmark));
            AddNode(generalNode, language.ChooseProfile, nameof(Configuration.Settings.Shortcuts.GeneralChooseProfile));
            AddNode(generalNode, language.DuplicateLine, nameof(Configuration.Settings.Shortcuts.GeneralDuplicateLine));
            AddNode(generalNode, language.OpenDataFolder, nameof(Configuration.Settings.Shortcuts.OpenDataFolder));
            AddNode(generalNode, LanguageSettings.Current.Main.Menu.File.OpenContainingFolder, nameof(Configuration.Settings.Shortcuts.OpenContainingFolder), true);
            AddNode(generalNode, language.ToggleView, nameof(Configuration.Settings.Shortcuts.GeneralToggleView), true);
            AddNode(generalNode, language.ToggleMode, nameof(Configuration.Settings.Shortcuts.GeneralToggleMode));
            AddNode(generalNode, language.TogglePreviewOnVideo, nameof(Configuration.Settings.Shortcuts.GeneralTogglePreviewOnVideo));
            AddNode(generalNode, language.RemoveBlankLines, nameof(Configuration.Settings.Shortcuts.GeneralRemoveBlankLines));
            AddNode(generalNode, language.ApplyAssaOverrideTags, nameof(Configuration.Settings.Shortcuts.GeneralApplyAssaOverrideTags), true);
            AddNode(generalNode, language.SetAssaPosition, nameof(Configuration.Settings.Shortcuts.GeneralSetAssaPosition), true);
            AddNode(generalNode, language.SetAssaResolution, nameof(Configuration.Settings.Shortcuts.GeneralSetAssaResolution));
            AddNode(generalNode, language.SetAssaBgBox, nameof(Configuration.Settings.Shortcuts.GeneralSetAssaBgBox), true);
            AddNode(generalNode, LanguageSettings.Current.ImageColorPicker.Title, nameof(Configuration.Settings.Shortcuts.GeneralColorPicker));
            AddNode(generalNode, language.TakeAutoBackup, nameof(Configuration.Settings.Shortcuts.GeneralTakeAutoBackup));
            AddNode(generalNode, language.Help, nameof(Configuration.Settings.Shortcuts.GeneralHelp), true);
            _shortcuts.Nodes.Add(generalNode);

            var fileNode = new ShortcutNode(LanguageSettings.Current.Main.Menu.File.Title);
            AddNode(fileNode, LanguageSettings.Current.Main.Menu.File.New, nameof(Configuration.Settings.Shortcuts.MainFileNew), true);
            AddNode(fileNode, LanguageSettings.Current.Main.Menu.File.Open, nameof(Configuration.Settings.Shortcuts.MainFileOpen), true);
            AddNode(fileNode, LanguageSettings.Current.Main.Menu.File.OpenKeepVideo, nameof(Configuration.Settings.Shortcuts.MainFileOpenKeepVideo), true);
            AddNode(fileNode, LanguageSettings.Current.Main.Menu.File.Save, nameof(Configuration.Settings.Shortcuts.MainFileSave), true);
            AddNode(fileNode, LanguageSettings.Current.Main.Menu.File.SaveAs, nameof(Configuration.Settings.Shortcuts.MainFileSaveAs), true);
            AddNode(fileNode, LanguageSettings.Current.Main.Menu.File.SaveOriginal, nameof(Configuration.Settings.Shortcuts.MainFileSaveOriginal), true);
            AddNode(fileNode, LanguageSettings.Current.Main.SaveOriginalSubtitleAs, nameof(Configuration.Settings.Shortcuts.MainFileSaveOriginalAs), true);
            AddNode(fileNode, LanguageSettings.Current.Main.Menu.File.OpenOriginal, nameof(Configuration.Settings.Shortcuts.MainFileOpenOriginal), true);
            AddNode(fileNode, LanguageSettings.Current.Main.Menu.File.CloseOriginal, nameof(Configuration.Settings.Shortcuts.MainFileCloseOriginal), true);
            AddNode(fileNode, LanguageSettings.Current.Main.Menu.File.CloseTranslation, nameof(Configuration.Settings.Shortcuts.MainFileCloseTranslation), true);
            AddNode(fileNode, language.MainFileSaveAll, nameof(Configuration.Settings.Shortcuts.MainFileSaveAll));
            AddNode(fileNode, LanguageSettings.Current.Main.Menu.File.Compare, nameof(Configuration.Settings.Shortcuts.MainFileCompare), true);
            AddNode(fileNode, LanguageSettings.Current.Main.Menu.File.VerifyCompleteness, nameof(Configuration.Settings.Shortcuts.MainFileVerifyCompleteness), true);
            AddNode(fileNode, LanguageSettings.Current.Main.Menu.File.Import + " -> " + LanguageSettings.Current.Main.Menu.File.ImportText, nameof(Configuration.Settings.Shortcuts.MainFileImportPlainText), true);
            AddNode(fileNode, LanguageSettings.Current.Main.Menu.File.Import + " -> " + LanguageSettings.Current.Main.Menu.File.ImportBluRaySupFileEdit, nameof(Configuration.Settings.Shortcuts.MainFileImportBdSupForEdit), true);
            AddNode(fileNode, LanguageSettings.Current.Main.Menu.File.Import + " -> " + LanguageSettings.Current.Main.Menu.File.ImportTimecodes, nameof(Configuration.Settings.Shortcuts.MainFileImportTimeCodes), true);
            AddNode(fileNode, LanguageSettings.Current.Main.Menu.File.Export + " -> " + LanguageSettings.Current.Main.Menu.File.ExportEbu, nameof(Configuration.Settings.Shortcuts.MainFileExportEbu), true);
            AddNode(fileNode, LanguageSettings.Current.Main.Menu.File.Export + " -> " + LanguageSettings.Current.Main.Menu.File.ExportPac, nameof(Configuration.Settings.Shortcuts.MainFileExportPac), true);
            AddNode(fileNode, LanguageSettings.Current.Main.Menu.File.Export + " -> " + LanguageSettings.Current.Main.Menu.File.ExportBluRaySup, nameof(Configuration.Settings.Shortcuts.MainFileExportBdSup), true);
            AddNode(fileNode, LanguageSettings.Current.Main.Menu.File.Export + " -> EDL/CLIPNAME", nameof(Configuration.Settings.Shortcuts.MainFileExportEdlClip), true);
            AddNode(fileNode, LanguageSettings.Current.Main.Menu.File.Export + " -> " + LanguageSettings.Current.Main.Menu.File.ExportPlainText, nameof(Configuration.Settings.Shortcuts.MainFileExportPlainText), true);
            AddNode(fileNode, LanguageSettings.Current.Main.Menu.File.Exit, nameof(Configuration.Settings.Shortcuts.MainFileExit), true);
            _shortcuts.Nodes.Add(fileNode);

            var editNode = new ShortcutNode(LanguageSettings.Current.Main.Menu.Edit.Title);
            AddNode(editNode, LanguageSettings.Current.Main.Menu.Edit.Undo, nameof(Configuration.Settings.Shortcuts.MainEditUndo), true);
            AddNode(editNode, LanguageSettings.Current.Main.Menu.Edit.Redo, nameof(Configuration.Settings.Shortcuts.MainEditRedo), true);
            AddNode(editNode, LanguageSettings.Current.Main.Menu.Edit.Find, nameof(Configuration.Settings.Shortcuts.MainEditFind), true);
            AddNode(editNode, LanguageSettings.Current.Main.Menu.Edit.FindNext, nameof(Configuration.Settings.Shortcuts.MainEditFindNext), true);
            AddNode(editNode, LanguageSettings.Current.Main.Menu.Edit.Replace, nameof(Configuration.Settings.Shortcuts.MainEditReplace), true);
            AddNode(editNode, LanguageSettings.Current.Main.Menu.Edit.MultipleReplace, nameof(Configuration.Settings.Shortcuts.MainEditMultipleReplace), true);
            AddNode(editNode, LanguageSettings.Current.Main.Menu.Edit.ModifySelection, nameof(Configuration.Settings.Shortcuts.MainEditModifySelection), true);
            AddNode(editNode, LanguageSettings.Current.Main.Menu.Edit.GoToSubtitleNumber, nameof(Configuration.Settings.Shortcuts.MainEditGoToLineNumber), true);
            AddNode(editNode, LanguageSettings.Current.VobSubOcr.RightToLeft, nameof(Configuration.Settings.Shortcuts.MainEditRightToLeft), true);
            AddNode(editNode, language.FixRTLViaUnicodeChars, nameof(Configuration.Settings.Shortcuts.MainEditFixRTLViaUnicodeChars), true);
            AddNode(editNode, language.RemoveRTLUnicodeChars, nameof(Configuration.Settings.Shortcuts.MainEditRemoveRTLUnicodeChars), true);
            AddNode(editNode, language.ReverseStartAndEndingForRtl, nameof(Configuration.Settings.Shortcuts.MainEditReverseStartAndEndingForRTL), true);
            AddNode(editNode, language.ToggleTranslationAndOriginalInPreviews, nameof(Configuration.Settings.Shortcuts.MainEditToggleTranslationOriginalInPreviews), true);
            _shortcuts.Nodes.Add(editNode);

            var toolsNode = new ShortcutNode(LanguageSettings.Current.Main.Menu.Tools.Title);
            AddNode(toolsNode, LanguageSettings.Current.Main.Menu.Tools.AdjustDisplayDuration, nameof(Configuration.Settings.Shortcuts.MainToolsAdjustDuration), true);
            AddNode(toolsNode, LanguageSettings.Current.Main.Menu.Tools.ApplyDurationLimits.Trim('.'), nameof(Configuration.Settings.Shortcuts.MainToolsAdjustDurationLimits), true);
            AddNode(toolsNode, LanguageSettings.Current.Main.Menu.Tools.SubtitlesBridgeGaps, nameof(Configuration.Settings.Shortcuts.MainToolsDurationsBridgeGap), true);
            AddNode(toolsNode, LanguageSettings.Current.Main.Menu.Tools.MinimumDisplayTimeBetweenParagraphs, nameof(Configuration.Settings.Shortcuts.MainToolsMinimumDisplayTimeBetweenParagraphs), true);
            AddNode(toolsNode, LanguageSettings.Current.Main.Menu.Tools.FixCommonErrors, nameof(Configuration.Settings.Shortcuts.MainToolsFixCommonErrors), true);
            AddNode(toolsNode, LanguageSettings.Current.Main.Menu.Tools.StartNumberingFrom, nameof(Configuration.Settings.Shortcuts.MainToolsRenumber), true);
            AddNode(toolsNode, LanguageSettings.Current.Main.Menu.Tools.RemoveTextForHearingImpaired, nameof(Configuration.Settings.Shortcuts.MainToolsRemoveTextForHI), true);
            AddNode(toolsNode, LanguageSettings.Current.Main.Menu.Tools.ConvertColorsToDialog, nameof(Configuration.Settings.Shortcuts.MainToolsConvertColorsToDialog), true);
            AddNode(toolsNode, LanguageSettings.Current.Main.Menu.Tools.ChangeCasing, nameof(Configuration.Settings.Shortcuts.MainToolsChangeCasing), true);
            AddNode(toolsNode, LanguageSettings.Current.Main.Menu.Tools.MakeNewEmptyTranslationFromCurrentSubtitle, nameof(Configuration.Settings.Shortcuts.MainToolsMakeEmptyFromCurrent), true);
            AddNode(toolsNode, LanguageSettings.Current.Main.Menu.Tools.MergeShortLines, nameof(Configuration.Settings.Shortcuts.MainToolsMergeShortLines), true);
            AddNode(toolsNode, LanguageSettings.Current.Main.Menu.Tools.MergeDuplicateText, nameof(Configuration.Settings.Shortcuts.MainToolsMergeDuplicateText), true);
            AddNode(toolsNode, LanguageSettings.Current.Main.Menu.Tools.MergeSameTimeCodes, nameof(Configuration.Settings.Shortcuts.MainToolsMergeSameTimeCodes), true);
            AddNode(toolsNode, LanguageSettings.Current.Main.Menu.Tools.SplitLongLines, nameof(Configuration.Settings.Shortcuts.MainToolsSplitLongLines), true);
            AddNode(toolsNode, LanguageSettings.Current.Main.Menu.Tools.BatchConvert, nameof(Configuration.Settings.Shortcuts.MainToolsBatchConvert));
            AddNode(toolsNode, LanguageSettings.Current.Main.Menu.Tools.MeasurementConverter, nameof(Configuration.Settings.Shortcuts.MainToolsMeasurementConverter), true);
            AddNode(toolsNode, LanguageSettings.Current.Main.Menu.Tools.SplitSubtitle, nameof(Configuration.Settings.Shortcuts.MainToolsSplit), true);
            AddNode(toolsNode, LanguageSettings.Current.Main.Menu.Tools.AppendSubtitle, nameof(Configuration.Settings.Shortcuts.MainToolsAppend), true);
            AddNode(toolsNode, LanguageSettings.Current.Main.Menu.Tools.JoinSubtitles, nameof(Configuration.Settings.Shortcuts.MainToolsJoin), true);
            AddNode(toolsNode, LanguageSettings.Current.Main.Menu.ContextMenu.AutoDurationCurrentLine, nameof(Configuration.Settings.Shortcuts.MainToolsAutoDuration));
            AddNode(toolsNode, language.ShowStyleManager, nameof(Configuration.Settings.Shortcuts.MainToolsStyleManager));
            _shortcuts.Nodes.Add(toolsNode);

            var videoNode = new ShortcutNode(LanguageSettings.Current.Main.Menu.Video.Title);
            AddNode(videoNode, LanguageSettings.Current.Main.Menu.Video.OpenVideo, nameof(Configuration.Settings.Shortcuts.MainVideoOpen), true);
            AddNode(videoNode, LanguageSettings.Current.Main.Menu.Video.CloseVideo, nameof(Configuration.Settings.Shortcuts.MainVideoClose), true);
            AddNode(videoNode, language.TogglePlayPause, nameof(Configuration.Settings.Shortcuts.MainVideoPlayPauseToggle));
            AddNode(videoNode, language.Pause, nameof(Configuration.Settings.Shortcuts.MainVideoPause));
            AddNode(videoNode, LanguageSettings.Current.Main.VideoControls.Stop, nameof(Configuration.Settings.Shortcuts.MainVideoStop));
            AddNode(videoNode, LanguageSettings.Current.Main.VideoControls.PlayFromJustBeforeText, nameof(Configuration.Settings.Shortcuts.MainVideoPlayFromJustBefore));
            AddNode(videoNode, LanguageSettings.Current.Main.VideoControls.PlayFromBeginning, nameof(Configuration.Settings.Shortcuts.MainVideoPlayFromBeginning));
            AddNode(videoNode, language.FocusSetVideoPosition, nameof(Configuration.Settings.Shortcuts.MainVideoFocusSetVideoPosition));
            AddNode(videoNode, language.ToggleDockUndockOfVideoControls, nameof(Configuration.Settings.Shortcuts.MainVideoToggleVideoControls), true);
            AddNode(videoNode, language.GoBack1Frame, nameof(Configuration.Settings.Shortcuts.MainVideo1FrameLeft));
            AddNode(videoNode, language.GoForward1Frame, nameof(Configuration.Settings.Shortcuts.MainVideo1FrameRight));
            AddNode(videoNode, language.GoBack1FrameWithPlay, nameof(Configuration.Settings.Shortcuts.MainVideo1FrameLeftWithPlay));
            AddNode(videoNode, language.GoForward1FrameWithPlay, nameof(Configuration.Settings.Shortcuts.MainVideo1FrameRightWithPlay));
            AddNode(videoNode, language.GoBack100Milliseconds, nameof(Configuration.Settings.Shortcuts.MainVideo100MsLeft));
            AddNode(videoNode, language.GoForward100Milliseconds, nameof(Configuration.Settings.Shortcuts.MainVideo100MsRight));
            AddNode(videoNode, language.GoBack500Milliseconds, nameof(Configuration.Settings.Shortcuts.MainVideo500MsLeft));
            AddNode(videoNode, language.GoForward500Milliseconds, nameof(Configuration.Settings.Shortcuts.MainVideo500MsRight));
            AddNode(videoNode, language.GoBack1Second, nameof(Configuration.Settings.Shortcuts.MainVideo1000MsLeft));
            AddNode(videoNode, language.GoForward1Second, nameof(Configuration.Settings.Shortcuts.MainVideo1000MsRight));
            AddNode(videoNode, language.GoBack3Seconds, nameof(Configuration.Settings.Shortcuts.MainVideo3000MsLeft));
            AddNode(videoNode, language.GoForward3Seconds, nameof(Configuration.Settings.Shortcuts.MainVideo3000MsRight));
            AddNode(videoNode, language.GoBack5Seconds, nameof(Configuration.Settings.Shortcuts.MainVideo5000MsLeft));
            AddNode(videoNode, language.GoForward5Seconds, nameof(Configuration.Settings.Shortcuts.MainVideo5000MsRight));
            AddNode(videoNode, language.GoBackXSSeconds, nameof(Configuration.Settings.Shortcuts.MainVideoXSMsLeft));
            AddNode(videoNode, language.GoForwardXSSeconds, nameof(Configuration.Settings.Shortcuts.MainVideoXSMsRight));
            AddNode(videoNode, language.GoBackXLSeconds, nameof(Configuration.Settings.Shortcuts.MainVideoXLMsLeft));
            AddNode(videoNode, language.GoForwardXLSeconds, nameof(Configuration.Settings.Shortcuts.MainVideoXLMsRight));
            AddNode(videoNode, language.GoToStartCurrent, nameof(Configuration.Settings.Shortcuts.MainVideoGoToStartCurrent));
            AddNode(videoNode, language.ToggleStartEndCurrent, nameof(Configuration.Settings.Shortcuts.MainVideoToggleStartEndCurrent));
            AddNode(videoNode, language.PlaySelectedLines, nameof(Configuration.Settings.Shortcuts.MainVideoPlaySelectedLines));
            AddNode(videoNode, language.LoopSelectedLines, nameof(Configuration.Settings.Shortcuts.MainVideoLoopSelectedLines));
            AddNode(videoNode, language.WaveformGoToPrevSubtitle, nameof(Configuration.Settings.Shortcuts.MainVideoGoToPrevSubtitle));
            AddNode(videoNode, language.WaveformGoToNextSubtitle, nameof(Configuration.Settings.Shortcuts.MainVideoGoToNextSubtitle));
            AddNode(videoNode, language.WaveformGoToPrevTimeCode, nameof(Configuration.Settings.Shortcuts.MainVideoGoToPrevTimeCode));
            AddNode(videoNode, language.WaveformGoToNextTimeCode, nameof(Configuration.Settings.Shortcuts.MainVideoGoToNextTimeCode));
            AddNode(videoNode, language.WaveformGoToPrevChapter, nameof(Configuration.Settings.Shortcuts.MainVideoGoToPrevChapter));
            AddNode(videoNode, language.WaveformGoToNextChapter, nameof(Configuration.Settings.Shortcuts.MainVideoGoToNextChapter));
            AddNode(videoNode, language.WaveformSelectNextSubtitle, nameof(Configuration.Settings.Shortcuts.MainVideoSelectNextSubtitle));
            AddNode(videoNode, language.Fullscreen, nameof(Configuration.Settings.Shortcuts.MainVideoFullscreen));
            AddNode(videoNode, language.Play150Speed, nameof(Configuration.Settings.Shortcuts.MainVideoPlay150Speed));
            AddNode(videoNode, language.Play200Speed, nameof(Configuration.Settings.Shortcuts.MainVideoPlay200Speed));
            AddNode(videoNode, language.PlayRateSlower, nameof(Configuration.Settings.Shortcuts.MainVideoSlower));
            AddNode(videoNode, language.PlayRateFaster, nameof(Configuration.Settings.Shortcuts.MainVideoFaster));
            AddNode(videoNode, language.PlayRateToggle, nameof(Configuration.Settings.Shortcuts.MainVideoSpeedToggle));
            AddNode(videoNode, language.VideoResetSpeedAndZoom, nameof(Configuration.Settings.Shortcuts.MainVideoReset));
            AddNode(videoNode, language.MainToggleVideoControls, nameof(Configuration.Settings.Shortcuts.MainVideoToggleControls));
            AddNode(videoNode, string.Format(language.AudioToTextX, "Vosk"), nameof(Configuration.Settings.Shortcuts.MainVideoAudioToTextVosk));
            AddNode(videoNode, string.Format(language.AudioToTextX, "Whisper"), nameof(Configuration.Settings.Shortcuts.MainVideoAudioToTextWhisper));
            AddNode(videoNode, LanguageSettings.Current.TextToSpeech.Title, nameof(Configuration.Settings.Shortcuts.MainVideoTextToSpeech));
            AddNode(videoNode, language.AudioExtractSelectedLines, nameof(Configuration.Settings.Shortcuts.MainVideoAudioExtractAudioSelectedLines));
            AddNode(videoNode, language.VideoToggleContrast, nameof(Configuration.Settings.Shortcuts.MainVideoToggleContrast));
            AddNode(videoNode, language.VideoToggleBrightness, nameof(Configuration.Settings.Shortcuts.MainVideoToggleBrightness));
            _shortcuts.Nodes.Add(videoNode);

            var spellCheckNode = new ShortcutNode(LanguageSettings.Current.Main.Menu.SpellCheck.Title);
            AddNode(spellCheckNode, LanguageSettings.Current.Main.Menu.SpellCheck.Title, nameof(Configuration.Settings.Shortcuts.MainSpellCheck), true);
            AddNode(spellCheckNode, LanguageSettings.Current.Main.Menu.SpellCheck.FindDoubleWords, nameof(Configuration.Settings.Shortcuts.MainSpellCheckFindDoubleWords), true);
            AddNode(spellCheckNode, LanguageSettings.Current.Main.Menu.SpellCheck.AddToNameList, nameof(Configuration.Settings.Shortcuts.MainSpellCheckAddWordToNames), true);
            _shortcuts.Nodes.Add(spellCheckNode);

            var syncNode = new ShortcutNode(LanguageSettings.Current.Main.Menu.Synchronization.Title);
            AddNode(syncNode, LanguageSettings.Current.Main.Menu.Synchronization.AdjustAllTimes, nameof(Configuration.Settings.Shortcuts.MainSynchronizationAdjustTimes), true);
            AddNode(syncNode, LanguageSettings.Current.Main.Menu.Synchronization.VisualSync, nameof(Configuration.Settings.Shortcuts.MainSynchronizationVisualSync), true);
            AddNode(syncNode, LanguageSettings.Current.Main.Menu.Synchronization.PointSync, nameof(Configuration.Settings.Shortcuts.MainSynchronizationPointSync), true);
            AddNode(syncNode, LanguageSettings.Current.Main.Menu.Synchronization.PointSyncViaOtherSubtitle, nameof(Configuration.Settings.Shortcuts.MainSynchronizationPointSyncViaFile), true);
            AddNode(syncNode, LanguageSettings.Current.Main.Menu.Tools.ChangeFrameRate, nameof(Configuration.Settings.Shortcuts.MainSynchronizationChangeFrameRate), true);
            _shortcuts.Nodes.Add(syncNode);

            var listViewAndTextBoxNode = new ShortcutNode(language.ListViewAndTextBox);
            AddNode(listViewAndTextBoxNode, LanguageSettings.Current.Main.Menu.ContextMenu.InsertAfter, nameof(Configuration.Settings.Shortcuts.MainInsertAfter));
            AddNode(listViewAndTextBoxNode, LanguageSettings.Current.Main.Menu.ContextMenu.InsertBefore, nameof(Configuration.Settings.Shortcuts.MainInsertBefore));
            AddNode(listViewAndTextBoxNode, LanguageSettings.Current.General.Italic, nameof(Configuration.Settings.Shortcuts.MainListViewItalic), true);
            AddNode(listViewAndTextBoxNode, LanguageSettings.Current.General.Bold, nameof(Configuration.Settings.Shortcuts.MainListViewBold), true);
            AddNode(listViewAndTextBoxNode, LanguageSettings.Current.General.Underline, nameof(Configuration.Settings.Shortcuts.MainListViewUnderline), true);
            AddNode(listViewAndTextBoxNode, LanguageSettings.Current.Main.Menu.ContextMenu.Box, nameof(Configuration.Settings.Shortcuts.MainListViewBox), true);
            AddNode(listViewAndTextBoxNode, language.ToggleQuotes, nameof(Configuration.Settings.Shortcuts.MainListViewToggleQuotes), true);
            AddNode(listViewAndTextBoxNode, language.ToggleHiTags, nameof(Configuration.Settings.Shortcuts.MainListViewToggleHiTags), true);
            AddNode(listViewAndTextBoxNode, language.ToggleCustomTags, nameof(Configuration.Settings.Shortcuts.MainListViewToggleCustomTags), false);
            AddNode(listViewAndTextBoxNode, language.MainTextBoxSelectionToggleCasing, nameof(Configuration.Settings.Shortcuts.MainTextBoxSelectionToggleCasing));
            AddNode(listViewAndTextBoxNode, LanguageSettings.Current.General.SplitLine.Replace("!", string.Empty), nameof(Configuration.Settings.Shortcuts.MainListViewSplit), true);
            AddNode(listViewAndTextBoxNode, language.ToggleMusicSymbols, nameof(Configuration.Settings.Shortcuts.MainListViewToggleMusicSymbols), true);
            AddNode(listViewAndTextBoxNode, language.AlignmentN1, nameof(Configuration.Settings.Shortcuts.MainListViewAlignmentN1));
            AddNode(listViewAndTextBoxNode, language.AlignmentN2, nameof(Configuration.Settings.Shortcuts.MainListViewAlignmentN2));
            AddNode(listViewAndTextBoxNode, language.AlignmentN3, nameof(Configuration.Settings.Shortcuts.MainListViewAlignmentN3));
            AddNode(listViewAndTextBoxNode, language.AlignmentN4, nameof(Configuration.Settings.Shortcuts.MainListViewAlignmentN4));
            AddNode(listViewAndTextBoxNode, language.AlignmentN5, nameof(Configuration.Settings.Shortcuts.MainListViewAlignmentN5));
            AddNode(listViewAndTextBoxNode, language.AlignmentN6, nameof(Configuration.Settings.Shortcuts.MainListViewAlignmentN6));
            AddNode(listViewAndTextBoxNode, language.AlignmentN7, nameof(Configuration.Settings.Shortcuts.MainListViewAlignmentN7));
            AddNode(listViewAndTextBoxNode, language.AlignmentN8, nameof(Configuration.Settings.Shortcuts.MainListViewAlignmentN8));
            AddNode(listViewAndTextBoxNode, language.AlignmentN9, nameof(Configuration.Settings.Shortcuts.MainListViewAlignmentN9));
            AddNode(listViewAndTextBoxNode, string.Format(language.ColorX, "1", ColorTranslator.ToHtml(Configuration.Settings.Tools.Color1)), nameof(Configuration.Settings.Shortcuts.MainListViewColor1));
            AddNode(listViewAndTextBoxNode, string.Format(language.ColorX, "2", ColorTranslator.ToHtml(Configuration.Settings.Tools.Color2)), nameof(Configuration.Settings.Shortcuts.MainListViewColor2));
            AddNode(listViewAndTextBoxNode, string.Format(language.ColorX, "3", ColorTranslator.ToHtml(Configuration.Settings.Tools.Color3)), nameof(Configuration.Settings.Shortcuts.MainListViewColor3));
            AddNode(listViewAndTextBoxNode, string.Format(language.ColorX, "4", ColorTranslator.ToHtml(Configuration.Settings.Tools.Color4)), nameof(Configuration.Settings.Shortcuts.MainListViewColor4));
            AddNode(listViewAndTextBoxNode, string.Format(language.ColorX, "5", ColorTranslator.ToHtml(Configuration.Settings.Tools.Color5)), nameof(Configuration.Settings.Shortcuts.MainListViewColor5));
            AddNode(listViewAndTextBoxNode, string.Format(language.ColorX, "6", ColorTranslator.ToHtml(Configuration.Settings.Tools.Color6)), nameof(Configuration.Settings.Shortcuts.MainListViewColor6));
            AddNode(listViewAndTextBoxNode, string.Format(language.ColorX, "7", ColorTranslator.ToHtml(Configuration.Settings.Tools.Color7)), nameof(Configuration.Settings.Shortcuts.MainListViewColor7));
            AddNode(listViewAndTextBoxNode, string.Format(language.ColorX, "8", ColorTranslator.ToHtml(Configuration.Settings.Tools.Color8)), nameof(Configuration.Settings.Shortcuts.MainListViewColor8));
            AddNode(listViewAndTextBoxNode, LanguageSettings.Current.DCinemaProperties.FontColor, nameof(Configuration.Settings.Shortcuts.MainListViewColorChoose), true);
            AddNode(listViewAndTextBoxNode, LanguageSettings.Current.Main.Menu.ContextMenu.RemoveFormattingAll, nameof(Configuration.Settings.Shortcuts.MainRemoveFormatting), true);
            AddNode(listViewAndTextBoxNode, language.RemoveTimeCodes, nameof(Configuration.Settings.Shortcuts.MainListViewRemoveTimeCodes));
            AddNode(listViewAndTextBoxNode, language.MainTextBoxUnbreak, nameof(Configuration.Settings.Shortcuts.MainTextBoxUnbreak));
            AddNode(listViewAndTextBoxNode, language.MainTextBoxUnbreakNoSpace, nameof(Configuration.Settings.Shortcuts.MainTextBoxUnbreakNoSpace));
            AddNode(listViewAndTextBoxNode, language.SetNewActor, nameof(Configuration.Settings.Shortcuts.MainListViewSetNewActor));
            AddNode(listViewAndTextBoxNode, string.Format(language.SetActorX, "1"), nameof(Configuration.Settings.Shortcuts.MainListViewSetActor1), true);
            AddNode(listViewAndTextBoxNode, string.Format(language.SetActorX, "2"), nameof(Configuration.Settings.Shortcuts.MainListViewSetActor2), true);
            AddNode(listViewAndTextBoxNode, string.Format(language.SetActorX, "3"), nameof(Configuration.Settings.Shortcuts.MainListViewSetActor3), true);
            AddNode(listViewAndTextBoxNode, string.Format(language.SetActorX, "4"), nameof(Configuration.Settings.Shortcuts.MainListViewSetActor4), true);
            AddNode(listViewAndTextBoxNode, string.Format(language.SetActorX, "5"), nameof(Configuration.Settings.Shortcuts.MainListViewSetActor5), true);
            AddNode(listViewAndTextBoxNode, string.Format(language.SetActorX, "6"), nameof(Configuration.Settings.Shortcuts.MainListViewSetActor6), true);
            AddNode(listViewAndTextBoxNode, string.Format(language.SetActorX, "7"), nameof(Configuration.Settings.Shortcuts.MainListViewSetActor7), true);
            AddNode(listViewAndTextBoxNode, string.Format(language.SetActorX, "8"), nameof(Configuration.Settings.Shortcuts.MainListViewSetActor8), true);
            AddNode(listViewAndTextBoxNode, string.Format(language.SetActorX, "9"), nameof(Configuration.Settings.Shortcuts.MainListViewSetActor9), true);
            AddNode(listViewAndTextBoxNode, string.Format(language.SetActorX, "10"), nameof(Configuration.Settings.Shortcuts.MainListViewSetActor10), true);
            _shortcuts.Nodes.Add(listViewAndTextBoxNode);

            var listViewNode = new ShortcutNode(language.ListView);
            AddNode(listViewNode, language.MergeDialog, nameof(Configuration.Settings.Shortcuts.MainMergeDialog));
            AddNode(listViewNode, language.MergeDialogWithNext, nameof(Configuration.Settings.Shortcuts.MainMergeDialogWithNext));
            AddNode(listViewNode, language.MergeDialogWithPrevious, nameof(Configuration.Settings.Shortcuts.MainMergeDialogWithPrevious));
            AddNode(listViewNode, language.AutoBalanceSelectedLines, nameof(Configuration.Settings.Shortcuts.MainAutoBalanceSelectedLines), true);
            AddNode(listViewNode, language.EvenlyDistributeSelectedLines, nameof(Configuration.Settings.Shortcuts.MainEvenlyDistributeSelectedLines), true);
            AddNode(listViewNode, language.ToggleFocus, nameof(Configuration.Settings.Shortcuts.MainToggleFocus));
            AddNode(listViewNode, language.ToggleFocusWaveform, nameof(Configuration.Settings.Shortcuts.MainToggleFocusWaveform));
            AddNode(listViewNode, language.ToggleFocusWaveformTextBox, nameof(Configuration.Settings.Shortcuts.MainToggleFocusWaveformTextBox));
            AddNode(listViewNode, language.ToggleDialogDashes, nameof(Configuration.Settings.Shortcuts.MainListViewToggleDashes));
            AddNode(listViewNode, language.Alignment, nameof(Configuration.Settings.Shortcuts.MainListViewAlignment), true);
            AddNode(listViewNode, language.CopyTextOnly, nameof(Configuration.Settings.Shortcuts.MainListViewCopyText));
            AddNode(listViewNode, language.CopyPlainText, nameof(Configuration.Settings.Shortcuts.MainListViewCopyPlainText));
            AddNode(listViewNode, language.CopyTextOnlyFromOriginalToCurrent, nameof(Configuration.Settings.Shortcuts.MainListViewCopyTextFromOriginalToCurrent), true);
            AddNode(listViewNode, language.AutoDurationSelectedLines, nameof(Configuration.Settings.Shortcuts.MainListViewAutoDuration));
            AddNode(listViewNode, language.ListViewColumnDelete, nameof(Configuration.Settings.Shortcuts.MainListViewColumnDeleteText), true);
            AddNode(listViewNode, language.ListViewColumnDeleteAndShiftUp, nameof(Configuration.Settings.Shortcuts.MainListViewColumnDeleteTextAndShiftUp), true);
            AddNode(listViewNode, language.ListViewColumnInsert, nameof(Configuration.Settings.Shortcuts.MainListViewColumnInsertText), true);
            AddNode(listViewNode, language.ListViewColumnPaste, nameof(Configuration.Settings.Shortcuts.MainListViewColumnPaste), true);
            AddNode(listViewNode, language.ListViewColumnTextUp, nameof(Configuration.Settings.Shortcuts.MainListViewColumnTextUp), true);
            AddNode(listViewNode, language.ListViewColumnTextDown, nameof(Configuration.Settings.Shortcuts.MainListViewColumnTextDown), true);
            AddNode(listViewNode, language.ListViewGoToNextError, nameof(Configuration.Settings.Shortcuts.MainListViewGoToNextError));
            AddNode(listViewNode, language.ListViewListErrors, nameof(Configuration.Settings.Shortcuts.MainListViewListErrors), true);
            AddNode(listViewNode, string.Format(language.ListViewListSortByX, LanguageSettings.Current.General.Number), nameof(Configuration.Settings.Shortcuts.MainListViewSortByNumber), true);
            AddNode(listViewNode, string.Format(language.ListViewListSortByX, LanguageSettings.Current.General.StartTime), nameof(Configuration.Settings.Shortcuts.MainListViewSortByStartTime), true);
            AddNode(listViewNode, string.Format(language.ListViewListSortByX, LanguageSettings.Current.General.EndTime), nameof(Configuration.Settings.Shortcuts.MainListViewSortByEndTime), true);
            AddNode(listViewNode, string.Format(language.ListViewListSortByX, LanguageSettings.Current.General.Duration), nameof(Configuration.Settings.Shortcuts.MainListViewSortByDuration), true);
            AddNode(listViewNode, string.Format(language.ListViewListSortByX, LanguageSettings.Current.General.Gap), nameof(Configuration.Settings.Shortcuts.MainListViewSortByGap), true);
            AddNode(listViewNode, string.Format(language.ListViewListSortByX, LanguageSettings.Current.Main.Menu.Tools.TextAlphabetically), nameof(Configuration.Settings.Shortcuts.MainListViewSortByText), true);
            AddNode(listViewNode, string.Format(language.ListViewListSortByX, LanguageSettings.Current.Main.Menu.Tools.TextSingleLineMaximumLength), nameof(Configuration.Settings.Shortcuts.MainListViewSortBySingleLineMaxLen), true);
            AddNode(listViewNode, string.Format(language.ListViewListSortByX, LanguageSettings.Current.Main.Menu.Tools.TextTotalLength), nameof(Configuration.Settings.Shortcuts.MainListViewSortByTextTotalLength), true);
            AddNode(listViewNode, string.Format(language.ListViewListSortByX, LanguageSettings.Current.General.CharsPerSec), nameof(Configuration.Settings.Shortcuts.MainListViewSortByCps), true);
            AddNode(listViewNode, string.Format(language.ListViewListSortByX, LanguageSettings.Current.General.WordsPerMin), nameof(Configuration.Settings.Shortcuts.MainListViewSortByWpm), true);
            AddNode(listViewNode, string.Format(language.ListViewListSortByX, LanguageSettings.Current.Main.Menu.Tools.TextNumberOfLines), nameof(Configuration.Settings.Shortcuts.MainListViewSortByNumberOfLines), true);
            AddNode(listViewNode, string.Format(language.ListViewListSortByX, LanguageSettings.Current.General.Style), nameof(Configuration.Settings.Shortcuts.MainListViewSortByStyle), true);
            AddNode(listViewNode, string.Format(language.ListViewListSortByX, LanguageSettings.Current.General.Actor), nameof(Configuration.Settings.Shortcuts.MainListViewSortByActor), true);
            _shortcuts.Nodes.Add(listViewNode);

            var textBoxNode = new ShortcutNode(language.TextBox);
            AddNode(textBoxNode, LanguageSettings.Current.Main.Menu.ContextMenu.SplitLineAtCursorPosition, nameof(Configuration.Settings.Shortcuts.MainTextBoxSplitAtCursor));
            AddNode(textBoxNode, LanguageSettings.Current.Main.Menu.ContextMenu.SplitLineAtCursorPositionAndAutoBr, nameof(Configuration.Settings.Shortcuts.MainTextBoxSplitAtCursorAndAutoBr));
            AddNode(textBoxNode, LanguageSettings.Current.Main.Menu.ContextMenu.SplitLineAtCursorAndWaveformPosition, nameof(Configuration.Settings.Shortcuts.MainTextBoxSplitAtCursorAndVideoPos));
            AddNode(textBoxNode, language.SplitSelectedLineBilingual, nameof(Configuration.Settings.Shortcuts.MainTextBoxSplitSelectedLineBilingual));
            AddNode(textBoxNode, language.MainTextBoxMoveLastWordDown, nameof(Configuration.Settings.Shortcuts.MainTextBoxMoveLastWordDown));
            AddNode(textBoxNode, language.MainTextBoxMoveFirstWordFromNextUp, nameof(Configuration.Settings.Shortcuts.MainTextBoxMoveFirstWordFromNextUp));
            AddNode(textBoxNode, language.MainTextBoxMoveLastWordDownCurrent, nameof(Configuration.Settings.Shortcuts.MainTextBoxMoveLastWordDownCurrent));
            AddNode(textBoxNode, language.MainTextBoxMoveFirstWordUpCurrent, nameof(Configuration.Settings.Shortcuts.MainTextBoxMoveFirstWordUpCurrent));
            AddNode(textBoxNode, language.MainTextBoxMoveFromCursorToNext, nameof(Configuration.Settings.Shortcuts.MainTextBoxMoveFromCursorToNextAndGoToNext));
            AddNode(textBoxNode, language.MainTextBoxSelectionToLower, nameof(Configuration.Settings.Shortcuts.MainTextBoxSelectionToLower));
            AddNode(textBoxNode, language.MainTextBoxSelectionToUpper, nameof(Configuration.Settings.Shortcuts.MainTextBoxSelectionToUpper));
            AddNode(textBoxNode, language.MainTextBoxSelectionToRuby, nameof(Configuration.Settings.Shortcuts.MainTextBoxSelectionToRuby), true);
            AddNode(textBoxNode, language.MainTextBoxToggleAutoDuration, nameof(Configuration.Settings.Shortcuts.MainTextBoxToggleAutoDuration));
            AddNode(textBoxNode, language.MainTextBoxAutoBreak, nameof(Configuration.Settings.Shortcuts.MainTextBoxAutoBreak));
            AddNode(textBoxNode, language.MainTextBoxAutoBreakFromPos, nameof(Configuration.Settings.Shortcuts.MainTextBoxBreakAtPosition));
            AddNode(textBoxNode, language.MainTextBoxAutoBreakFromPosAndGoToNext, nameof(Configuration.Settings.Shortcuts.MainTextBoxBreakAtPositionAndGoToNext));
            AddNode(textBoxNode, language.MainTextBoxDictate, nameof(Configuration.Settings.Shortcuts.MainTextBoxRecord));
            AddNode(textBoxNode, language.MainTextBoxAssaIntellisense, nameof(Configuration.Settings.Shortcuts.MainTextBoxAssaIntellisense));
            AddNode(textBoxNode, language.MainTextBoxAssaRemoveTag, nameof(Configuration.Settings.Shortcuts.MainTextBoxAssaRemoveTag));
            _shortcuts.Nodes.Add(textBoxNode);

            var translateNode = new ShortcutNode(LanguageSettings.Current.Main.VideoControls.Translate);
            AddNode(translateNode, LanguageSettings.Current.Main.VideoControls.GoogleIt, nameof(Configuration.Settings.Shortcuts.MainTranslateGoogleIt));
            AddNode(translateNode, LanguageSettings.Current.Main.VideoControls.GoogleTranslate, nameof(Configuration.Settings.Shortcuts.MainTranslateGoogleTranslateIt));
            AddNode(translateNode, LanguageSettings.Current.Main.VideoControls.AutoTranslate, nameof(Configuration.Settings.Shortcuts.MainTranslateAuto), true);
            AddNode(translateNode, language.AutoTranslateSelectedLines, nameof(Configuration.Settings.Shortcuts.MainTranslateAutoSelectedLines), true);
            AddNode(translateNode, language.CustomSearch1, nameof(Configuration.Settings.Shortcuts.MainTranslateCustomSearch1));
            AddNode(translateNode, language.CustomSearch2, nameof(Configuration.Settings.Shortcuts.MainTranslateCustomSearch2));
            AddNode(translateNode, language.CustomSearch3, nameof(Configuration.Settings.Shortcuts.MainTranslateCustomSearch3));
            AddNode(translateNode, language.CustomSearch4, nameof(Configuration.Settings.Shortcuts.MainTranslateCustomSearch4));
            AddNode(translateNode, language.CustomSearch5, nameof(Configuration.Settings.Shortcuts.MainTranslateCustomSearch5));
            _shortcuts.Nodes.Add(translateNode);

            var createAndAdjustNode = new ShortcutNode(LanguageSettings.Current.Main.VideoControls.CreateAndAdjust);
            AddNode(createAndAdjustNode, LanguageSettings.Current.Main.VideoControls.InsertNewSubtitleAtVideoPosition, nameof(Configuration.Settings.Shortcuts.MainCreateInsertSubAtVideoPos));
            AddNode(createAndAdjustNode, LanguageSettings.Current.Main.VideoControls.InsertNewSubtitleAtVideoPositionNoTextBoxFocus, nameof(Configuration.Settings.Shortcuts.MainCreateInsertSubAtVideoPosNoTextBoxFocus));
            AddNode(createAndAdjustNode, LanguageSettings.Current.Main.VideoControls.InsertNewSubtitleAtVideoPositionMax, nameof(Configuration.Settings.Shortcuts.MainCreateInsertSubAtVideoPosMax));
            AddNode(createAndAdjustNode, language.MainCreateStartDownEndUp, nameof(Configuration.Settings.Shortcuts.MainCreateStartDownEndUp));
            AddNode(createAndAdjustNode, LanguageSettings.Current.Main.VideoControls.SetStartTime, nameof(Configuration.Settings.Shortcuts.MainCreateSetStart));
            AddNode(createAndAdjustNode, language.AdjustSetStartTimeKeepDuration, nameof(Configuration.Settings.Shortcuts.MainAdjustSetStartKeepDuration));
            AddNode(createAndAdjustNode, language.AdjustVideoSetStartForAppropriateLine, nameof(Configuration.Settings.Shortcuts.MainAdjustVideoSetStartForAppropriateLine));
            AddNode(createAndAdjustNode, language.AdjustVideoSetEndForAppropriateLine, nameof(Configuration.Settings.Shortcuts.MainAdjustVideoSetEndForAppropriateLine));
            AddNode(createAndAdjustNode, LanguageSettings.Current.Main.VideoControls.SetStartTimeAndOffsetTheRest, nameof(Configuration.Settings.Shortcuts.MainAdjustSetStartAndOffsetTheRest));
            AddNode(createAndAdjustNode, LanguageSettings.Current.Main.VideoControls.SetStartTimeAndOffsetTheRest, nameof(Configuration.Settings.Shortcuts.MainAdjustSetStartAndOffsetTheRest2));
            AddNode(createAndAdjustNode, language.AdjustSetStartAndOffsetTheWholeSubtitle, nameof(Configuration.Settings.Shortcuts.MainAdjustSetStartAndOffsetTheWholeSubtitle));
            AddNode(createAndAdjustNode, language.AdjustSetStartAutoDurationAndGoToNext, nameof(Configuration.Settings.Shortcuts.MainAdjustSetStartAutoDurationAndGoToNext));
            AddNode(createAndAdjustNode, language.AdjustStartDownEndUpAndGoToNext, nameof(Configuration.Settings.Shortcuts.MainAdjustStartDownEndUpAndGoToNext));
            AddNode(createAndAdjustNode, language.AdjustSetStartAndEndOfPrevious, nameof(Configuration.Settings.Shortcuts.MainAdjustSetStartAndEndOfPrevious));
            AddNode(createAndAdjustNode, language.AdjustSetStartAndEndOfPreviousAndGoToNext, nameof(Configuration.Settings.Shortcuts.MainAdjustSetStartAndEndOfPreviousAndGoToNext));
            AddNode(createAndAdjustNode, LanguageSettings.Current.Main.VideoControls.SetEndTime, nameof(Configuration.Settings.Shortcuts.MainCreateSetEnd));
            AddNode(createAndAdjustNode, language.AdjustSetEndTimeAndPause, nameof(Configuration.Settings.Shortcuts.MainAdjustSetEndAndPause));
            AddNode(createAndAdjustNode, language.CreateSetEndAddNewAndGoToNew, nameof(Configuration.Settings.Shortcuts.MainCreateSetEndAddNewAndGoToNew));
            AddNode(createAndAdjustNode, language.AdjustSetStartTimeAndGoToNext, nameof(Configuration.Settings.Shortcuts.MainAdjustSetStartAndGotoNext));
            AddNode(createAndAdjustNode, language.AdjustSetEndTimeAndGoToNext, nameof(Configuration.Settings.Shortcuts.MainAdjustSetEndAndGotoNext));
            AddNode(createAndAdjustNode, language.AdjustSetEndAndOffsetTheRest, nameof(Configuration.Settings.Shortcuts.MainAdjustSetEndAndOffsetTheRest));
            AddNode(createAndAdjustNode, language.AdjustSetEndAndOffsetTheRestAndGoToNext, nameof(Configuration.Settings.Shortcuts.MainAdjustSetEndAndOffsetTheRestAndGoToNext));
            AddNode(createAndAdjustNode, language.AdjustSetEndNextStartAndGoToNext, nameof(Configuration.Settings.Shortcuts.MainAdjustSetEndNextStartAndGoToNext));
            AddNode(createAndAdjustNode, language.AdjustSetEndMinusGapAndStartNextHere, nameof(Configuration.Settings.Shortcuts.MainAdjustSetEndMinusGapAndStartNextHere));
            AddNode(createAndAdjustNode, language.AdjustSetEndAndStartNextAfterGap, nameof(Configuration.Settings.Shortcuts.MainSetEndAndStartNextAfterGap));
            AddNode(createAndAdjustNode, language.AdjustViaEndAutoStart, nameof(Configuration.Settings.Shortcuts.MainAdjustViaEndAutoStart));
            AddNode(createAndAdjustNode, language.AdjustViaEndAutoStartAndGoToNext, nameof(Configuration.Settings.Shortcuts.MainAdjustViaEndAutoStartAndGoToNext));
            AddNode(createAndAdjustNode, language.AdjustSelected100MsBack, nameof(Configuration.Settings.Shortcuts.MainAdjustSelected100MsBack));
            AddNode(createAndAdjustNode, language.AdjustSelected100MsForward, nameof(Configuration.Settings.Shortcuts.MainAdjustSelected100MsForward));
            AddNode(createAndAdjustNode, string.Format(language.AdjustStartXMsBack, Configuration.Settings.Tools.MoveStartEndMs), nameof(Configuration.Settings.Shortcuts.MainAdjustStartXMsBack));
            AddNode(createAndAdjustNode, string.Format(language.AdjustStartXMsForward, Configuration.Settings.Tools.MoveStartEndMs), nameof(Configuration.Settings.Shortcuts.MainAdjustStartXMsForward));
            AddNode(createAndAdjustNode, string.Format(language.AdjustEndXMsBack, Configuration.Settings.Tools.MoveStartEndMs), nameof(Configuration.Settings.Shortcuts.MainAdjustEndXMsBack));
            AddNode(createAndAdjustNode, string.Format(language.AdjustEndXMsForward, Configuration.Settings.Tools.MoveStartEndMs), nameof(Configuration.Settings.Shortcuts.MainAdjustEndXMsForward));
            AddNode(createAndAdjustNode, language.AdjustStartOneFrameBack, nameof(Configuration.Settings.Shortcuts.MoveStartOneFrameBack));
            AddNode(createAndAdjustNode, language.AdjustStartOneFrameForward, nameof(Configuration.Settings.Shortcuts.MoveStartOneFrameForward));
            AddNode(createAndAdjustNode, language.AdjustEndOneFrameBack, nameof(Configuration.Settings.Shortcuts.MoveEndOneFrameBack));
            AddNode(createAndAdjustNode, language.AdjustEndOneFrameForward, nameof(Configuration.Settings.Shortcuts.MoveEndOneFrameForward));
            AddNode(createAndAdjustNode, language.AdjustStartOneFrameBackKeepGapPrev, nameof(Configuration.Settings.Shortcuts.MoveStartOneFrameBackKeepGapPrev));
            AddNode(createAndAdjustNode, language.AdjustStartOneFrameForwardKeepGapPrev, nameof(Configuration.Settings.Shortcuts.MoveStartOneFrameForwardKeepGapPrev));
            AddNode(createAndAdjustNode, language.AdjustEndOneFrameBackKeepGapNext, nameof(Configuration.Settings.Shortcuts.MoveEndOneFrameBackKeepGapNext));
            AddNode(createAndAdjustNode, language.AdjustEndOneFrameForwardKeepGapNext, nameof(Configuration.Settings.Shortcuts.MoveEndOneFrameForwardKeepGapNext));
            AddNode(createAndAdjustNode, language.RecalculateDurationOfCurrentSubtitle, nameof(Configuration.Settings.Shortcuts.GeneralAutoCalcCurrentDuration));
            AddNode(createAndAdjustNode, language.RecalculateDurationOfCurrentSubtitleByOptimalReadingSpeed, nameof(Configuration.Settings.Shortcuts.GeneralAutoCalcCurrentDurationByOptimalReadingSpeed));
            AddNode(createAndAdjustNode, language.RecalculateDurationOfCurrentSubtitleByMinReadingSpeed, nameof(Configuration.Settings.Shortcuts.GeneralAutoCalcCurrentDurationByMinReadingSpeed));
            AddNode(createAndAdjustNode, language.AdjustSnapStartToNextShotChange, nameof(Configuration.Settings.Shortcuts.MainAdjustSnapStartToNextShotChange));
            AddNode(createAndAdjustNode, language.AdjustSnapEndToPreviousShotChange, nameof(Configuration.Settings.Shortcuts.MainAdjustSnapEndToPreviousShotChange));
            AddNode(createAndAdjustNode, language.AdjustExtendToNextShotChange, nameof(Configuration.Settings.Shortcuts.MainAdjustExtendToNextShotChange));
            AddNode(createAndAdjustNode, language.AdjustExtendToPreviousShotChange, nameof(Configuration.Settings.Shortcuts.MainAdjustExtendToPreviousShotChange));
            AddNode(createAndAdjustNode, language.AdjustExtendToNextSubtitle, nameof(Configuration.Settings.Shortcuts.MainAdjustExtendToNextSubtitle));
            AddNode(createAndAdjustNode, language.AdjustExtendToPreviousSubtitle, nameof(Configuration.Settings.Shortcuts.MainAdjustExtendToPreviousSubtitle));
            AddNode(createAndAdjustNode, language.AdjustExtendToNextSubtitleMinusChainingGap, nameof(Configuration.Settings.Shortcuts.MainAdjustExtendToNextSubtitleMinusChainingGap));
            AddNode(createAndAdjustNode, language.AdjustExtendToPreviousSubtitleMinusChainingGap, nameof(Configuration.Settings.Shortcuts.MainAdjustExtendToPreviousSubtitleMinusChainingGap));
            AddNode(createAndAdjustNode, language.AdjustExtendCurrentSubtitle, nameof(Configuration.Settings.Shortcuts.MainAdjustExtendCurrentSubtitle));
            AddNode(createAndAdjustNode, language.AdjustExtendPreviousLineEndToCurrentStart, nameof(Configuration.Settings.Shortcuts.MainAdjustExtendPreviousLineEndToCurrentStart));
            AddNode(createAndAdjustNode, language.AdjustExtendNextLineStartToCurrentEnd, nameof(Configuration.Settings.Shortcuts.MainAdjustExtendNextLineStartToCurrentEnd));
            AddNode(createAndAdjustNode, language.SetInCueToClosestShotChangeLeftGreenZone, nameof(Configuration.Settings.Shortcuts.MainSetInCueToClosestShotChangeLeftGreenZone));
            AddNode(createAndAdjustNode, language.SetInCueToClosestShotChangeRightGreenZone, nameof(Configuration.Settings.Shortcuts.MainSetInCueToClosestShotChangeRightGreenZone));
            AddNode(createAndAdjustNode, language.SetOutCueToClosestShotChangeLeftGreenZone, nameof(Configuration.Settings.Shortcuts.MainSetOutCueToClosestShotChangeLeftGreenZone));
            AddNode(createAndAdjustNode, language.SetOutCueToClosestShotChangeRightGreenZone, nameof(Configuration.Settings.Shortcuts.MainSetOutCueToClosestShotChangeRightGreenZone));
            _shortcuts.Nodes.Add(createAndAdjustNode);

            var audioVisualizerNode = new ShortcutNode(language.WaveformAndSpectrogram);
            AddNode(audioVisualizerNode, LanguageSettings.Current.Waveform.AddWaveformAndSpectrogram, nameof(Configuration.Settings.Shortcuts.WaveformAdd));
            AddNode(audioVisualizerNode, LanguageSettings.Current.Waveform.ZoomIn, nameof(Configuration.Settings.Shortcuts.WaveformZoomIn));
            AddNode(audioVisualizerNode, LanguageSettings.Current.Waveform.ZoomOut, nameof(Configuration.Settings.Shortcuts.WaveformZoomOut));
            AddNode(audioVisualizerNode, language.VerticalZoom, nameof(Configuration.Settings.Shortcuts.WaveformVerticalZoom));
            AddNode(audioVisualizerNode, language.VerticalZoomOut, nameof(Configuration.Settings.Shortcuts.WaveformVerticalZoomOut));
            AddNode(audioVisualizerNode, LanguageSettings.Current.Main.Menu.ContextMenu.Split, nameof(Configuration.Settings.Shortcuts.WaveformSplit));
            AddNode(audioVisualizerNode, language.WaveformSeekSilenceForward, nameof(Configuration.Settings.Shortcuts.WaveformSearchSilenceForward));
            AddNode(audioVisualizerNode, language.WaveformSeekSilenceBack, nameof(Configuration.Settings.Shortcuts.WaveformSearchSilenceBack));
            AddNode(audioVisualizerNode, language.WaveformAddTextHere, nameof(Configuration.Settings.Shortcuts.WaveformAddTextHere));
            AddNode(audioVisualizerNode, language.WaveformAddTextHereFromClipboard, nameof(Configuration.Settings.Shortcuts.WaveformAddTextHereFromClipboard));
            AddNode(audioVisualizerNode, language.SetParagraphAsSelection, nameof(Configuration.Settings.Shortcuts.WaveformSetParagraphAsSelection));
            AddNode(audioVisualizerNode, language.WaveformPlayNewSelection, nameof(Configuration.Settings.Shortcuts.WaveformPlaySelection));
            AddNode(audioVisualizerNode, language.WaveformPlayNewSelectionEnd, nameof(Configuration.Settings.Shortcuts.WaveformPlaySelectionEnd));
            AddNode(audioVisualizerNode, LanguageSettings.Current.Main.VideoControls.InsertNewSubtitleAtVideoPosition, nameof(Configuration.Settings.Shortcuts.MainWaveformInsertAtCurrentPosition));
            AddNode(audioVisualizerNode, language.WaveformGoToPreviousShotChange, nameof(Configuration.Settings.Shortcuts.WaveformGoToPreviousShotChange));
            AddNode(audioVisualizerNode, language.WaveformGoToNextShotChange, nameof(Configuration.Settings.Shortcuts.WaveformGoToNextShotChange));
            AddNode(audioVisualizerNode, language.WaveformToggleShotChange, nameof(Configuration.Settings.Shortcuts.WaveformToggleShotChange));
            AddNode(audioVisualizerNode, language.WaveformRemoveOrExportShotChanges, nameof(Configuration.Settings.Shortcuts.WaveformListShotChanges), true);
            AddNode(audioVisualizerNode, language.WaveformGuessStart, nameof(Configuration.Settings.Shortcuts.WaveformGuessStart));
            AddNode(audioVisualizerNode, language.GoBack100Milliseconds, nameof(Configuration.Settings.Shortcuts.Waveform100MsLeft));
            AddNode(audioVisualizerNode, language.GoForward100Milliseconds, nameof(Configuration.Settings.Shortcuts.Waveform100MsRight));
            AddNode(audioVisualizerNode, language.GoBack1Second, nameof(Configuration.Settings.Shortcuts.Waveform1000MsLeft));
            AddNode(audioVisualizerNode, language.GoForward1Second, nameof(Configuration.Settings.Shortcuts.Waveform1000MsRight));
            AddNode(audioVisualizerNode, string.Format(language.AudioToTextSelectedLinesX, "Vosk"), nameof(Configuration.Settings.Shortcuts.WaveformAudioToTextVosk));
            AddNode(audioVisualizerNode, string.Format(language.AudioToTextSelectedLinesX, "Whisper"), nameof(Configuration.Settings.Shortcuts.WaveformAudioToTextWhisper));
            _shortcuts.Nodes.Add(audioVisualizerNode);

            LoadPluginsShortcuts();
        }

        private void ShowShortcutsTreeView()
        {
            treeViewShortcuts.BeginUpdate();
            treeViewShortcuts.Nodes.Clear();

            foreach (var parent in _shortcuts.Nodes)
            {
                var parentNode = new TreeNode(parent.Text);
                foreach (ShortcutNode node in parent.Nodes)
                {
                    AddNode(parentNode, node.Text, node.Shortcut);
                }
                if (parentNode.Nodes.Count > 0)
                {
                    treeViewShortcuts.Nodes.Add(parentNode);
                }
            }
            foreach (TreeNode node in treeViewShortcuts.Nodes)
            {
                node.Text = node.Text.RemoveChar('&');
                foreach (TreeNode subNode in node.Nodes)
                {
                    subNode.Text = subNode.Text.RemoveChar('&');
                    foreach (TreeNode subSubNode in subNode.Nodes)
                    {
                        subSubNode.Text = subSubNode.Text.RemoveChar('&');
                    }
                }
            }

            treeViewShortcuts.ExpandAll();
            treeViewShortcuts.EndUpdate();
            if (treeViewShortcuts.Nodes.Count > 0)
            {
                treeViewShortcuts.SelectedNode = treeViewShortcuts.Nodes[0];
            }
        }

        private void AddNode(TreeNode parentNode, string text, ShortcutHelper shortcut)
        {
            var emptyText = "[" + LanguageSettings.Current.General.None + "]";

            if (nikseComboBoxShortcutsFilter.SelectedIndex == 1) // used
            {
                if (text.Contains(emptyText))
                {
                    return;
                }
            }

            if (nikseComboBoxShortcutsFilter.SelectedIndex == 2) // unused
            {
                if (!text.Contains(emptyText))
                {
                    return;
                }
            }

            var normalizeAmpersand = text.Replace("&&", "@_____@").Replace("&", string.Empty).Replace("@_____@", "&");
            if (textBoxShortcutSearch.Left < 2 || normalizeAmpersand.Contains(textBoxShortcutSearch.Text, StringComparison.OrdinalIgnoreCase))
            {
                parentNode.Nodes.Add(new TreeNode(text) { Tag = shortcut });
            }
        }

        private void AddNode(ShortcutNode node, string text, string shortcut, bool isMenuItem = false)
        {
            var prop = Configuration.Settings.Shortcuts.GetType().GetProperty(shortcut);
            if (prop != null)
            {
                var s = text + GetShortcutText((string)prop.GetValue(Configuration.Settings.Shortcuts, null));
                node.Nodes.Add(new ShortcutNode(s) { Shortcut = new ShortcutHelper(prop, isMenuItem), ShortcutText = shortcut });
            }
        }

        private static string GetShortcutText(string shortcut)
        {
            if (string.IsNullOrEmpty(shortcut))
            {
                shortcut = LanguageSettings.Current.General.None;
            }

            return $" [{shortcut}]";
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

        public void Initialize(Icon icon, Image newFile, Image openFile, Image openVideo, Image saveFile, Image saveFileAs, Image find, Image replace, Image fixCommonErrors, Image removeTextForHi,
                               Image visualSync, Image burnIn, Image spellCheck, Image netflixGlyphCheck, Image beautifyTimeCodes, Image settings, Image help, Image toggleSourceView)
        {
            Icon = (Icon)icon.Clone();
            pictureBoxFileNew.Image = (Image)newFile.Clone();
            pictureBoxPreview1.Image = (Image)newFile.Clone();
            pictureBoxTBOpen.Image = (Image)openFile.Clone();
            pictureBoxTBOpenVideo.Image = (Image)openVideo.Clone();
            pictureBoxPreview2.Image = (Image)openFile.Clone();
            pictureBoxTBSave.Image = (Image)saveFile.Clone();
            pictureBoxPreview3.Image = (Image)saveFile.Clone();
            pictureBoxTBSaveAs.Image = (Image)saveFileAs.Clone();
            pictureBoxTBFind.Image = (Image)find.Clone();
            pictureBoxTBReplace.Image = (Image)replace.Clone();
            pictureBoxTBFixCommonErrors.Image = (Image)fixCommonErrors.Clone();
            pictureBoxRemoveTextForHi.Image = (Image)removeTextForHi.Clone();
            pictureBoxTBToggleSourceView.Image = (Image)toggleSourceView.Clone();
            pictureBoxTBVisualSync.Image = (Image)visualSync.Clone();
            pictureBoxTBBurnIn.Image = (Image)burnIn.Clone();
            pictureBoxTBSpellCheck.Image = (Image)spellCheck.Clone();
            pictureBoxTBNetflixQualityCheck.Image = (Image)netflixGlyphCheck.Clone();
            pictureBoxTBBeautifyTimeCodes.Image = (Image)beautifyTimeCodes.Clone();
            pictureBoxTBSettings.Image = (Image)settings.Clone();
            pictureBoxTBHelp.Image = (Image)help.Clone();
        }
        private void ButtonOkClick(object sender, EventArgs e)
        {
            var gs = Configuration.Settings.General;

            gs.ShowToolbarNew = checkBoxToolbarNew.Checked;
            gs.ShowToolbarOpen = checkBoxTBOpen.Checked;
            gs.ShowToolbarOpenVideo = checkBoxTBOpenVideo.Checked;
            gs.ShowToolbarSave = checkBoxTBSave.Checked;
            gs.ShowToolbarSaveAs = checkBoxTBSaveAs.Checked;
            gs.ShowToolbarFind = checkBoxTBFind.Checked;
            gs.ShowToolbarReplace = checkBoxTBReplace.Checked;
            gs.ShowToolbarFixCommonErrors = checkBoxTBFixCommonErrors.Checked;
            gs.ShowToolbarRemoveTextForHi = checkBoxTBRemoveTextForHi.Checked;
            gs.ShowToolbarToggleSourceView = checkBoxTBToggleSourceView.Checked;
            gs.ShowToolbarVisualSync = checkBoxTBVisualSync.Checked;
            gs.ShowToolbarBurnIn = checkBoxTBBurnIn.Checked;
            gs.ShowToolbarSettings = checkBoxTBSettings.Checked;
            gs.ShowToolbarSpellCheck = checkBoxTBSpellCheck.Checked;
            gs.ShowToolbarNetflixGlyphCheck = checkBoxTBNetflixQualityCheck.Checked;
            gs.ShowToolbarBeautifyTimeCodes = checkBoxTBBeautifyTimeCodes.Checked;
            gs.ShowToolbarHelp = checkBoxTBHelp.Checked;

            gs.ShowFrameRate = checkBoxShowFrameRate.Checked;
            if (double.TryParse(comboBoxFrameRate.Text.Replace(',', '.').Replace(CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator, "."), NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var outFrameRate))
            {
                gs.DefaultFrameRate = outFrameRate;
            }

            gs.DefaultEncoding = ((TextEncoding)comboBoxEncoding.Items[comboBoxEncoding.SelectedIndex]).ToString();

            gs.AutoGuessAnsiEncoding = checkBoxAutoDetectAnsiEncoding.Checked;
            gs.SubtitleTextBoxFontSize = int.Parse(comboBoxSubtitleFontSize.Text);
            gs.SubtitleListViewFontSize = int.Parse(comboBoxSubtitleListViewFontSize.Text);
            gs.SubtitleTextBoxFontBold = checkBoxSubtitleFontBold.Checked;
            gs.SubtitleTextBoxSyntaxColor = checkBoxSubtitleTextBoxSyntaxColor.Checked;
            gs.SubtitleTextBoxHtmlColor = panelTextBoxHtmlColor.BackColor;
            gs.SubtitleTextBoxAssColor = panelTextBoxAssColor.BackColor;
            gs.DarkThemeBackColor = panelDarkThemeBackColor.BackColor;
            gs.DarkThemeForeColor = panelDarkThemeColor.BackColor;
            gs.UseDarkTheme = checkBoxDarkThemeEnabled.Checked;
            gs.DarkThemeShowListViewGridLines = checkBoxDarkThemeShowListViewGridLines.Checked;
            gs.SubtitleListViewFontBold = checkBoxSubtitleListViewFontBold.Checked;
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
            {
                gs.ListViewLineSeparatorString = "<br />";
            }

            gs.ListViewDoubleClickAction = comboBoxListViewDoubleClickEvent.SelectedIndex;

            gs.Profiles = _rulesProfiles;

            gs.TranslationAutoSuffixDefault = comboBoxTranslationAutoSuffix.Text;
            var suffixes = new List<string>();
            foreach (var suffix in comboBoxTranslationAutoSuffix.Items)
            {
                suffixes.Add(suffix.ToString());
            }
            gs.TranslationAutoSuffixes = string.Join(";", suffixes);

            gs.SaveAsUseFileNameFrom = comboBoxSaveAsFileNameFrom.SelectedIndex == 0 ? "video" : "file";

            gs.SubtitleMinimumDisplayMilliseconds = (int)numericUpDownDurationMin.Value;
            gs.SubtitleMaximumDisplayMilliseconds = (int)numericUpDownDurationMax.Value;
            gs.MinimumMillisecondsBetweenLines = (int)numericUpDownMinGapMs.Value;
            gs.CurrentProfile = comboBoxRulesProfileName.Text;

            if (comboBoxAutoBackup.SelectedIndex == 1)
            {
                gs.AutoBackupSeconds = 60;
            }
            else if (comboBoxAutoBackup.SelectedIndex == 2)
            {
                gs.AutoBackupSeconds = 60 * 5;
            }
            else if (comboBoxAutoBackup.SelectedIndex == 3)
            {
                gs.AutoBackupSeconds = 60 * 15;
            }
            else
            {
                gs.AutoBackupSeconds = 0;
            }

            gs.AutoBackupDeleteAfterMonths = comboBoxAutoBackupDeleteAfter.SelectedIndex + 1;

            gs.CheckForUpdates = checkBoxCheckForUpdates.Checked;
            gs.AutoSave = checkBoxAutoSave.Checked;

            if (comboBoxTimeCodeMode.Visible)
            {
                gs.UseTimeFormatHHMMSSFF = comboBoxTimeCodeMode.SelectedIndex == 1;
            }

            gs.SplitBehavior = comboBoxSplitBehavior.SelectedIndex;

            gs.SpellChecker = comboBoxSpellChecker.SelectedIndex == 1 ? "word" : "hunspell";

            gs.AllowEditOfOriginalSubtitle = checkBoxAllowEditOfOriginalSubtitle.Checked;
            gs.PromptDeleteLines = checkBoxPromptDeleteLines.Checked;

            if (radioButtonVideoPlayerMPV.Checked)
            {
                gs.VideoPlayer = "MPV";
            }
            else if (radioButtonVideoPlayerMpcHc.Checked)
            {
                gs.VideoPlayer = "MPC-HC";
            }
            else if (radioButtonVideoPlayerVLC.Checked)
            {
                gs.VideoPlayer = "VLC";
            }
            else if (!string.IsNullOrEmpty(gs.VideoPlayer))
            {
                gs.VideoPlayer = "DirectShow";
            }

            gs.MpvHandlesPreviewText = checkBoxMpvHandlesPreviewText.Checked;
            gs.VlcLocation = textBoxVlcPath.Text;

            gs.VideoPlayerShowStopButton = checkBoxVideoPlayerShowStopButton.Checked;
            gs.VideoPlayerShowMuteButton = checkBoxVideoPlayerShowMuteButton.Checked;
            gs.VideoPlayerShowFullscreenButton = checkBoxVideoPlayerShowFullscreenButton.Checked;
            gs.VideoPlayerPreviewFontName = comboBoxVideoPlayerPreviewFontName.SelectedItem.ToString();
            gs.VideoPlayerPreviewFontSize = int.Parse(comboBoxlVideoPlayerPreviewFontSize.Items[0].ToString()) + comboBoxlVideoPlayerPreviewFontSize.SelectedIndex;
            gs.MpvPreviewTextMarginVertical = (int)numericUpDownMarginVertical.Value;
            gs.VideoPlayerPreviewFontBold = checkBoxVideoPlayerPreviewFontBold.Checked;
            gs.MpvPreviewTextPrimaryColor = panelMpvPrimaryColor.BackColor;
            gs.MpvPreviewTextOutlineColor = panelMpvOutlineColor.BackColor;
            gs.MpvPreviewTextBackgroundColor = panelMpvBackColor.BackColor;

            gs.DisableVideoAutoLoading = !checkBoxVideoAutoOpen.Checked;
            gs.AllowVolumeBoost = checkBoxAllowVolumeBoost.Checked;
            gs.MpvPreviewTextOpaqueBox = radioButtonMpvOpaqueBox.Checked;
            gs.MpvPreviewTextOutlineWidth = numericUpDownMpvOutline.Value;
            gs.MpvPreviewTextShadowWidth = numericUpDownMpvShadowWidth.Value;

            if (radioButtonMpvOpaqueBox.Checked)
            {
                if (comboBoxOpaqueBoxStyle.SelectedIndex == 0)
                {
                    gs.MpvPreviewTextOpaqueBoxStyle = "3";
                }
                else
                {
                    gs.MpvPreviewTextOpaqueBoxStyle = "4";
                }
            }
            else
            {
                gs.MpvPreviewTextOpaqueBoxStyle = "1";
            }

            Configuration.Settings.VideoControls.CustomSearchText1 = comboBoxCustomSearch1.Text;
            Configuration.Settings.VideoControls.CustomSearchText2 = comboBoxCustomSearch2.Text;
            Configuration.Settings.VideoControls.CustomSearchText3 = comboBoxCustomSearch3.Text;
            Configuration.Settings.VideoControls.CustomSearchText4 = comboBoxCustomSearch4.Text;
            Configuration.Settings.VideoControls.CustomSearchText5 = comboBoxCustomSearch5.Text;
            Configuration.Settings.VideoControls.CustomSearchUrl1 = textBoxCustomSearchUrl1.Text;
            Configuration.Settings.VideoControls.CustomSearchUrl2 = textBoxCustomSearchUrl2.Text;
            Configuration.Settings.VideoControls.CustomSearchUrl3 = textBoxCustomSearchUrl3.Text;
            Configuration.Settings.VideoControls.CustomSearchUrl4 = textBoxCustomSearchUrl4.Text;
            Configuration.Settings.VideoControls.CustomSearchUrl5 = textBoxCustomSearchUrl5.Text;

            int maxLength = (int)numericUpDownSubtitleLineMaximumLength.Value;
            if (maxLength > 9 && maxLength < 1000)
            {
                gs.SubtitleLineMaximumLength = maxLength;
            }
            else if (maxLength > 999)
            {
                gs.SubtitleLineMaximumLength = 999;
            }
            else
            {
                gs.SubtitleLineMaximumLength = 45;
            }

            gs.SubtitleOptimalCharactersPerSeconds = (double)numericUpDownOptimalCharsSec.Value;
            gs.SubtitleMaximumCharactersPerSeconds = (double)numericUpDownMaxCharsSec.Value;
            gs.SubtitleMaximumWordsPerMinute = (double)numericUpDownMaxWordsMin.Value;
            gs.MaxNumberOfLines = (int)numericUpDownMaxNumberOfLines.Value;

            gs.AutoWrapLineWhileTyping = checkBoxAutoWrapWhileTyping.Checked;

            if (comboBoxSubtitleFont.SelectedItem != null)
            {
                gs.SubtitleFontName = comboBoxSubtitleFont.SelectedItem.ToString();
            }

            gs.DefaultSubtitleFormat = comboBoxSubtitleFormats.Text;
            gs.DefaultSaveAsFormat = comboBoxSubtitleSaveAsFormats.Text == LanguageSettings.Current.Settings.DefaultSaveAsFormatAuto ? string.Empty : comboBoxSubtitleSaveAsFormats.Text;
            if (listBoxFavoriteSubtitleFormats.Items.Count >= 0)
            {
                var sb = new StringBuilder();
                for (int i = 0; i < listBoxFavoriteSubtitleFormats.Items.Count; i++)
                {
                    sb.Append(listBoxFavoriteSubtitleFormats.Items[i]);
                    if (i < listBoxFavoriteSubtitleFormats.Items.Count - 1)
                    {
                        sb.Append(";");
                    }
                }

                var favorites = sb.ToString();
                if (favorites != Configuration.Settings.General.FavoriteSubtitleFormats)
                {
                    gs.FavoriteSubtitleFormats = favorites;
                    SubtitleFormat.SubtitleFormatsOrderChanged = true;
                }
            }


            var toolsSettings = Configuration.Settings.Tools;
            toolsSettings.VerifyPlaySeconds = comboBoxToolsVerifySeconds.SelectedIndex + 2;
            toolsSettings.StartSceneIndex = comboBoxToolsStartSceneIndex.SelectedIndex;
            toolsSettings.EndSceneIndex = comboBoxToolsEndSceneIndex.SelectedIndex;
            if (comboBoxMergeShortLineLength.SelectedIndex >= 0 && !string.IsNullOrEmpty(comboBoxMergeShortLineLength.Text))
            {
                gs.MergeLinesShorterThan = int.Parse(comboBoxMergeShortLineLength.Text, CultureInfo.InvariantCulture);
            }

            if (gs.MergeLinesShorterThan > gs.SubtitleLineMaximumLength + 1)
            {
                gs.MergeLinesShorterThan = gs.SubtitleLineMaximumLength;
            }

            gs.DialogStyle = DialogSplitMerge.GetDialogStyleFromIndex(comboBoxDialogStyle.SelectedIndex);
            gs.ContinuationStyle = ContinuationUtilities.GetContinuationStyleFromIndex(comboBoxContinuationStyle.SelectedIndex);
            gs.CpsLineLengthStrategy = (comboBoxCpsLineLenCalc.SelectedItem as CpsLineLength)?.Code;

            toolsSettings.MusicSymbol = comboBoxToolsMusicSymbol.SelectedItem.ToString();
            toolsSettings.MusicSymbolReplace = textBoxMusicSymbolsToReplace.Text;
            toolsSettings.SpellCheckAutoChangeNameCasing = checkBoxSpellCheckAutoChangeNames.Checked;
            toolsSettings.SpellCheckAutoChangeNamesUseSuggestions = checkBoxSpellCheckAutoChangeNamesViaSuggestions.Checked;
            toolsSettings.CheckOneLetterWords = checkBoxSpellCheckOneLetterWords.Checked;
            toolsSettings.SpellCheckEnglishAllowInQuoteAsIng = checkBoxTreatINQuoteAsING.Checked;
            toolsSettings.RememberUseAlwaysList = checkBoxUseAlwaysToFile.Checked;
            toolsSettings.LiveSpellCheck = checkBoxLiveSpellCheck.Checked;
            toolsSettings.UseNoLineBreakAfter = checkBoxUseDoNotBreakAfterList.Checked;
            toolsSettings.AutoBreakCommaBreakEarly = checkBoxToolsBreakEarlyComma.Checked;
            toolsSettings.AutoBreakLineEndingEarly = checkBoxToolsBreakEarlyLineEnding.Checked;
            toolsSettings.AutoBreakUsePixelWidth = checkBoxToolsBreakByPixelWidth.Checked;
            toolsSettings.AutoBreakPreferBottomHeavy = checkBoxToolsBreakPreferBottomHeavy.Checked;
            toolsSettings.AutoBreakPreferBottomPercent = (double)numericUpDownToolsBreakPreferBottomHeavy.Value;
            toolsSettings.AutoBreakDashEarly = checkBoxToolsBreakEarlyDash.Checked;

            Configuration.Settings.Tools.BDOpenIn = comboBoxBDOpensIn.SelectedIndex == 0 ? "OCR" : "EDIT";
            Configuration.Settings.General.AllowLetterShortcutsInTextBox = checkBoxShortcutsAllowLetterOrNumberInTextBox.Checked;
            Configuration.Settings.General.TagsInToggleCustomTags = comboBoxCustomToggleStart.Text + "Æ" + comboBoxCustomToggleEnd.Text;

            toolsSettings.OcrFixUseHardcodedRules = checkBoxFixCommonOcrErrorsUsingHardcodedRules.Checked;
            toolsSettings.OcrUseWordSplitList = checkBoxUseWordSplitList.Checked;
            toolsSettings.OcrUseWordSplitListAvoidPropercase = checkBoxUseWordSplitListAvoidPropercase.Checked;
            toolsSettings.FixShortDisplayTimesAllowMoveStartTime = checkBoxFixShortDisplayTimesAllowMoveStartTime.Checked;
            toolsSettings.FixCommonErrorsSkipStepOne = checkBoxFceSkipStep1.Checked;
            toolsSettings.MicrosoftTranslatorApiKey = textBoxBingClientSecret.Text.Trim();
            toolsSettings.MicrosoftTranslatorTokenEndpoint = comboBoxBoxBingTokenEndpoint.Text.Trim();
            toolsSettings.GoogleApiV2Key = textBoxGoogleTransleApiKey.Text.Trim();
            toolsSettings.AutoTranslateNllbApiUrl = nikseTextBoxNllbApiUrl.Text;
            toolsSettings.AutoTranslateNllbServeUrl = nikseTextBoxNllbServeUrl.Text;
            toolsSettings.AutoTranslateNllbServeModel = nikseTextBoxNllbServeModel.Text;
            toolsSettings.AutoTranslateLibreUrl = nikseTextBoxLibreTranslateUrl.Text;
            toolsSettings.AutoTranslateLibreApiKey = nikseTextBoxLibreTranslateApiKey.Text.Trim();
            toolsSettings.AutoTranslateMyMemoryApiKey = nikseTextBoxMyMemoryApiKey.Text;
            toolsSettings.AutoTranslateDeepLUrl = nikseTextBoxDeepLUrl.Text;
            toolsSettings.AutoTranslateDeepLApiKey = nikseTextBoxDeepLApiKey.Text;
            toolsSettings.ChatGptUrl = nikseTextBoxChatGptUrl.Text;
            toolsSettings.ChatGptApiKey = nikseTextBoxChatGptApiKey.Text;
            toolsSettings.ChatGptModel = nikseComboBoxChatGptModel.Text;
            toolsSettings.AutoTranslatePapagoApiKeyId = nikseTextBoxPapagoClientId.Text.Trim();
            toolsSettings.AutoTranslatePapagoApiKey = nikseTextBoxPapagoClientSecret.Text.Trim();

            var proxy = Configuration.Settings.Proxy;
            proxy.ProxyAddress = textBoxProxyAddress.Text;
            proxy.UserName = textBoxProxyUserName.Text;
            if (string.IsNullOrWhiteSpace(textBoxProxyPassword.Text))
            {
                proxy.Password = null;
            }
            else
            {
                proxy.EncodePassword(textBoxProxyPassword.Text);
            }

            proxy.Domain = textBoxProxyDomain.Text;
            proxy.AuthType = comboBoxProxyAuthType.Text;
            proxy.UseDefaultCredentials = checkBoxProxyUseDefaultCredentials.Checked;

            Configuration.Settings.NetworkSettings.NewMessageSound = textBoxNetworkSessionNewMessageSound.Text;

            Configuration.Settings.Tools.ListViewSyntaxColorDurationSmall = checkBoxSyntaxColorDurationTooSmall.Checked;
            Configuration.Settings.Tools.ListViewSyntaxColorDurationBig = checkBoxSyntaxColorDurationTooLarge.Checked;
            Configuration.Settings.Tools.ListViewSyntaxColorLongLines = checkBoxSyntaxColorTextTooLong.Checked;
            Configuration.Settings.Tools.ListViewSyntaxColorWideLines = checkBoxSyntaxColorTextTooWide.Checked;
            Configuration.Settings.Tools.ListViewSyntaxMoreThanXLines = checkBoxSyntaxColorTextMoreThanTwoLines.Checked;
            Configuration.Settings.Tools.ListViewSyntaxColorOverlap = checkBoxSyntaxOverlap.Checked;
            Configuration.Settings.Tools.ListViewSyntaxColorGap = checkBoxSyntaxColorGapTooSmall.Checked;
            Configuration.Settings.Tools.ListViewSyntaxErrorColor = panelListViewSyntaxColorError.BackColor;

            Configuration.Settings.VideoControls.WaveformDrawGrid = checkBoxWaveformShowGrid.Checked;
            Configuration.Settings.VideoControls.WaveformDrawCps = checkBoxWaveformShowCps.Checked;
            Configuration.Settings.VideoControls.WaveformDrawWpm = checkBoxWaveformShowWpm.Checked;
            Configuration.Settings.VideoControls.WaveformGridColor = panelWaveformGridColor.BackColor;
            Configuration.Settings.VideoControls.WaveformSelectedColor = panelWaveformSelectedColor.BackColor;
            Configuration.Settings.VideoControls.WaveformColor = panelWaveformColor.BackColor;
            Configuration.Settings.VideoControls.WaveformBackgroundColor = panelWaveformBackgroundColor.BackColor;
            Configuration.Settings.VideoControls.WaveformTextColor = panelWaveformTextColor.BackColor;
            Configuration.Settings.VideoControls.WaveformCursorColor = panelWaveformCursorColor.BackColor;
            Configuration.Settings.VideoControls.GenerateSpectrogram = checkBoxGenerateSpectrogram.Checked;
            Configuration.Settings.VideoControls.SpectrogramAppearance = comboBoxSpectrogramAppearance.SelectedIndex == 0 ? "OneColorGradient" : "Classic";

            Configuration.Settings.VideoControls.WaveformTextSize = int.Parse(comboBoxWaveformTextSize.Text);
            Configuration.Settings.VideoControls.WaveformTextBold = checkBoxWaveformTextBold.Checked;
            Configuration.Settings.VideoControls.WaveformMouseWheelScrollUpIsForward = checkBoxReverseMouseWheelScrollDirection.Checked;
            Configuration.Settings.VideoControls.WaveformAllowOverlap = checkBoxAllowOverlap.Checked;
            Configuration.Settings.VideoControls.WaveformSetVideoPositionOnMoveStartEnd = checkBoxWaveformSetVideoPosMoveStartEnd.Checked;
            Configuration.Settings.VideoControls.WaveformFocusOnMouseEnter = checkBoxWaveformHoverFocus.Checked;
            Configuration.Settings.VideoControls.WaveformListViewFocusOnMouseEnter = checkBoxListViewMouseEnterFocus.Checked;
            Configuration.Settings.VideoControls.WaveformSingleClickSelect = checkBoxWaveformSingleClickSelect.Checked;
            Configuration.Settings.VideoControls.WaveformSnapToShotChanges = checkBoxWaveformSnapToShotChanges.Checked;
            Configuration.Settings.General.WaveformAutoGenWhenOpeningVideo = checkBoxWaveformAutoGen.Checked;
            Configuration.Settings.VideoControls.WaveformBorderHitMs = Convert.ToInt32(numericUpDownWaveformBorderHitMs.Value);
            gs.UseFFmpegForWaveExtraction = checkBoxUseFFmpeg.Checked;
            gs.FFmpegUseCenterChannelOnly = checkBoxFfmpegUseCenterChannel.Checked;
            gs.FFmpegLocation = textBoxFFmpegPath.Text;
            gs.DefaultLanguages = _defaultLanguages;

            gs.ToolbarIconTheme = comboBoxToolbarIconTheme.SelectedIndex > 0 ? comboBoxToolbarIconTheme.Text : "Auto";

            // save shortcuts
            Configuration.Settings.Shortcuts.PluginShortcuts = _pluginShortcuts;
            foreach (var kvp in _newShortcuts)
            {
                if (!kvp.Key.IsPlugin)
                {
                    kvp.Key.Shortcut.SetValue(Configuration.Settings.Shortcuts, kvp.Value, null);
                }
            }

            if (_oldSettings == Core.Common.Settings.CustomSerialize(Configuration.Settings))
            {
                DialogResult = DialogResult.Cancel;
                return;
            }

            Configuration.Settings.Save();
        }

        private void FormSettings_KeyDown(object sender, KeyEventArgs e)
        {
            if (comboBoxShortcutKey.Focused)
            {
                return;
            }

            if (e.KeyCode == Keys.Escape)
            {
                DialogResult = DialogResult.Cancel;
            }
            else if (e.KeyData == UiUtil.HelpKeys)
            {
                UiUtil.ShowHelp("#settings");
                e.SuppressKeyPress = true;
            }
        }

        private int _lastSelectionIndex = -1;
        private void ListBoxSectionSelectedIndexChanged(object sender, EventArgs e)
        {
            if (_lastSelectionIndex == listBoxSection.SelectedIndex)
            {
                return;
            }

            labelStatus.Text = string.Empty;

            panelGeneral.Visible = false;
            panelSubtitleFormats.Visible = false;
            panelShortcuts.Visible = false;
            panelSyntaxColoring.Visible = false;
            panelVideoPlayer.Visible = false;
            panelWaveform.Visible = false;
            panelTools.Visible = false;
            panelAutoTranslate.Visible = false;
            panelToolBar.Visible = false;
            panelFont.Visible = false;
            panelNetwork.Visible = false;
            panelFileTypeAssociations.Visible = false;

            var section = panelGeneral;
            switch (listBoxSection.SelectedIndex)
            {
                case SubtitleFormatsSection:
                    section = panelSubtitleFormats;
                    break;
                case ShortcutsSection:
                    section = panelShortcuts;
                    Cursor = Cursors.WaitCursor;
                    ShowShortcutsTreeView();
                    Cursor = Cursors.Default;
                    break;
                case SyntaxColoringSection:
                    section = panelSyntaxColoring;
                    break;
                case VideoPlayerSection:
                    section = panelVideoPlayer;
                    break;
                case WaveformAndSpectrogramSection:
                    section = panelWaveform;
                    break;
                case ToolsSection:
                    section = panelTools;
                    break;
                case AutoTranslateSection:
                    section = panelAutoTranslate;
                    break;
                case ToolbarSection:
                    section = panelToolBar;
                    break;
                case AppearanceSection:
                    TryLoadIcon(pictureBoxPreview1, "New");
                    TryLoadIcon(pictureBoxPreview2, "Open");
                    TryLoadIcon(pictureBoxPreview3, "Save");
                    section = panelFont;
                    break;
                case NetworkSection:
                    section = panelNetwork;
                    break;
                case FileTypeAssociationSection:
                    section = panelFileTypeAssociations;
                    if (listViewFileTypeAssociations.Items.Count == 0)
                    {
                        FillFileTypeAssociationsListView();
                    }

                    break;
            }

            section.Top = panelGeneral.Top;
            section.Height = panelGeneral.Height;
            section.Left = panelGeneral.Left;
            section.Width = panelGeneral.Width;
            section.Visible = true;

            _lastSelectionIndex = listBoxSection.SelectedIndex;
        }

        private void LoadPluginsShortcuts()
        {
            if (_pluginShortcuts == null)
            {
                _pluginShortcuts = Configuration.Settings.Shortcuts.PluginShortcuts.Select(p => new PluginShortcut { Name = p.Name, Shortcut = p.Shortcut }).ToList();
            }

            if (!Directory.Exists(Configuration.PluginsDirectory.TrimEnd(Path.DirectorySeparatorChar)))
            {
                return;
            }

            var pluginsNode = new ShortcutNode(LanguageSettings.Current.PluginsGet.Title);
            foreach (var pluginFileName in Configuration.GetPlugins())
            {
                Main.GetPropertiesAndDoAction(pluginFileName, out var name, out _, out var version, out var description, out var actionType, out _, out var mi);
                if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(actionType) && mi != null)
                {
                    var text = GetPluginShortcutText(name);
                    var shortcutNode = new ShortcutNode(text)
                    {
                        Text = text,
                        ShortcutText = name,
                        Shortcut = new ShortcutHelper(null, true, true),
                    };
                    pluginsNode.Nodes.Add(shortcutNode);
                }
            }

            if (pluginsNode.Nodes.Count > 0)
            {
                _shortcuts.Nodes.Add(pluginsNode);
            }
        }

        private string GetPluginShortcutText(string name)
        {
            var shortcut = _pluginShortcuts.FirstOrDefault(p => p.Name == name);
            return shortcut == null ? $"{name} [{LanguageSettings.Current.General.None}]" : $"{name} [{shortcut.Shortcut}]";
        }

        private void comboBoxCustomSearch_SelectedIndexChanged(object sender, EventArgs e)
        {
            var cb = (NikseComboBox)sender;
            var tb = textBoxCustomSearchUrl1;
            if (cb == comboBoxCustomSearch2)
            {
                tb = textBoxCustomSearchUrl2;
            }
            else if (cb == comboBoxCustomSearch3)
            {
                tb = textBoxCustomSearchUrl3;
            }
            else if (cb == comboBoxCustomSearch4)
            {
                tb = textBoxCustomSearchUrl4;
            }
            else if (cb == comboBoxCustomSearch5)
            {
                tb = textBoxCustomSearchUrl5;
            }

            if (cb.SelectedIndex >= 0)
            {
                if (cb.SelectedIndex == 0)
                {
                    tb.Text = "https://www.dictionary.com/browse/{0}";
                }
                else if (cb.SelectedIndex == 1)
                {
                    tb.Text = "http://www.learnersdictionary.com/search/{0}";
                }
                else if (cb.SelectedIndex == 2)
                {
                    tb.Text = "https://www.merriam-webster.com/dictionary/{0}";
                }
                else if (cb.SelectedIndex == 3)
                {
                    tb.Text = "https://www.thefreedictionary.com/{0}";
                }
                else if (cb.SelectedIndex == 4)
                {
                    tb.Text = "https://www.thesaurus.com/browse/{0}";
                }
                else if (cb.SelectedIndex == 5)
                {
                    tb.Text = "https://www.urbandictionary.com/define.php?term={0}";
                }
                else if (cb.SelectedIndex == 6)
                {
                    tb.Text = "https://visuwords.com/?word={0}";
                }
                else if (cb.SelectedIndex == 7)
                {
                    tb.Text = "https://en.wikipedia.org/wiki?search={0}";
                }
            }
        }

        private void buttonWaveformSelectedColor_Click(object sender, EventArgs e)
        {
            using (var colorChooser = new ColorChooser { Color = panelWaveformSelectedColor.BackColor, ShowAlpha = false })
            {
                if (colorChooser.ShowDialog() == DialogResult.OK)
                {
                    panelWaveformSelectedColor.BackColor = colorChooser.Color;
                }
            }
        }

        private void buttonWaveformColor_Click(object sender, EventArgs e)
        {
            using (var colorChooser = new ColorChooser { Color = panelWaveformColor.BackColor, ShowAlpha = false })
            {
                if (colorChooser.ShowDialog() == DialogResult.OK)
                {
                    panelWaveformColor.BackColor = colorChooser.Color;
                }
            }
        }

        private void buttonWaveformBackgroundColor_Click(object sender, EventArgs e)
        {
            using (var colorChooser = new ColorChooser { Color = panelWaveformBackgroundColor.BackColor, ShowAlpha = false })
            {
                if (colorChooser.ShowDialog() == DialogResult.OK)
                {
                    panelWaveformBackgroundColor.BackColor = colorChooser.Color;
                }
            }
        }

        private void buttonWaveformGridColor_Click(object sender, EventArgs e)
        {
            using (var colorChooser = new ColorChooser { Color = panelWaveformGridColor.BackColor, ShowAlpha = false })
            {
                if (colorChooser.ShowDialog() == DialogResult.OK)
                {
                    panelWaveformGridColor.BackColor = colorChooser.Color;
                }
            }
        }

        private void buttonWaveformTextColor_Click(object sender, EventArgs e)
        {
            using (var colorChooser = new ColorChooser { Color = panelWaveformTextColor.BackColor, ShowAlpha = false })
            {
                if (colorChooser.ShowDialog() == DialogResult.OK)
                {
                    panelWaveformTextColor.BackColor = colorChooser.Color;
                }
            }
        }

        private void buttonWaveformCursorColor_Click(object sender, EventArgs e)
        {
            using (var colorChooser = new ColorChooser { Color = panelWaveformCursorColor.BackColor, ShowAlpha = false })
            {
                if (colorChooser.ShowDialog() == DialogResult.OK)
                {
                    panelWaveformCursorColor.BackColor = colorChooser.Color;
                }
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
                    {
                        File.Delete(imageDbFileName);
                    }

                    string xmlFileName = Path.Combine(dir.FullName, "Info.xml");
                    if (File.Exists(xmlFileName))
                    {
                        File.Delete(xmlFileName);
                    }

                    Directory.Delete(dir.FullName);
                }
            }

            InitializeWaveformsAndSpectrogramsFolderEmpty(LanguageSettings.Current.Settings);
        }

        private void checkBoxRememberRecentFiles_CheckedChanged(object sender, EventArgs e)
        {
            checkBoxReopenLastOpened.Enabled = checkBoxRememberRecentFiles.Checked;
            checkBoxRememberSelectedLine.Enabled = checkBoxRememberRecentFiles.Checked;
        }

        private void buttonWaveformSelectedColor_Click(object sender, MouseEventArgs e)
        {
            using (var colorChooser = new ColorChooser { Color = panelWaveformSelectedColor.BackColor, ShowAlpha = false })
            {
                if (colorChooser.ShowDialog() == DialogResult.OK)
                {
                    panelWaveformSelectedColor.BackColor = colorChooser.Color;
                }
            }
        }

        private void panelSubtitleFontColor_Click(object sender, EventArgs e)
        {
            using (var colorChooser = new ColorChooser { Color = panelSubtitleFontColor.BackColor, ShowAlpha = false })
            {
                if (colorChooser.ShowDialog() == DialogResult.OK)
                {
                    panelSubtitleFontColor.BackColor = colorChooser.Color;
                }
            }
        }

        private void panelSubtitleBackgroundColor_Click(object sender, EventArgs e)
        {
            using (var colorChooser = new ColorChooser { Color = panelSubtitleBackgroundColor.BackColor, ShowAlpha = false })
            {
                if (colorChooser.ShowDialog() == DialogResult.OK)
                {
                    panelSubtitleBackgroundColor.BackColor = colorChooser.Color;
                }
            }
        }

        private void treeViewShortcuts_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (e?.Node?.Nodes.Count == 0)
            {
                checkBoxShortcutsControl.Checked = false;
                checkBoxShortcutsAlt.Checked = false;
                checkBoxShortcutsShift.Checked = false;

                checkBoxShortcutsControl.Enabled = true;
                checkBoxShortcutsAlt.Enabled = true;
                checkBoxShortcutsShift.Enabled = true;

                comboBoxShortcutKey.SelectedIndex = 0;

                comboBoxShortcutKey.Enabled = true;
                buttonUpdateShortcut.Enabled = true;
                buttonClearShortcut.Enabled = true;

                string shortcut = GetShortcut(e.Node.Text);

                string[] parts = shortcut.ToLowerInvariant().Split(new[] { '+' }, StringSplitOptions.RemoveEmptyEntries);
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
            if (shortcut == LanguageSettings.Current.General.None)
            {
                return string.Empty;
            }

            return shortcut;
        }

        private string GetCurrentShortcutText()
        {
            var sb = new StringBuilder(@"[");
            if (checkBoxShortcutsControl.Checked)
            {
                sb.Append("Control+");
            }

            if (checkBoxShortcutsAlt.Checked)
            {
                sb.Append("Alt+");
            }

            if (checkBoxShortcutsShift.Checked)
            {
                sb.Append("Shift+");
            }

            sb.Append(comboBoxShortcutKey.Items[comboBoxShortcutKey.SelectedIndex]);
            sb.Append(']');
            return sb.ToString();
        }

        private void buttonUpdateShortcut_Click(object sender, EventArgs e)
        {
            if (!IsShortcutValid())
            {
                return;
            }

            string text = treeViewShortcuts.SelectedNode.Text.Substring(0, treeViewShortcuts.SelectedNode.Text.IndexOf('[')).Trim();
            var shortcutText = GetCurrentShortcutText();
            var existsIn = new StringBuilder();
            var sh = (ShortcutHelper)treeViewShortcuts.SelectedNode.Tag;

            if (sh.IsPlugin)
            {
                var cleanShortcut = shortcutText.RemoveChar('[').RemoveChar(']');
                var existingShortCut = _pluginShortcuts.FirstOrDefault(p => p.Name == text);
                if (existingShortCut == null)
                {
                    _pluginShortcuts.Add(new PluginShortcut { Name = text, Shortcut = cleanShortcut });
                }
                else
                {
                    existingShortCut.Shortcut = cleanShortcut;
                }
            }

            foreach (ShortcutNode parent in _shortcuts.Nodes)
            {
                foreach (ShortcutNode subNode in parent.Nodes)
                {
                    if (sh != null && !sh.IsPlugin && !subNode.Shortcut.IsPlugin && subNode.Shortcut.Shortcut.Name == sh.Shortcut.Name)
                    {
                        subNode.Text = text + " " + shortcutText;
                    }
                    else if (subNode.Text.Contains(shortcutText) && treeViewShortcuts.SelectedNode.Text != subNode.Text)
                    {
                        existsIn.AppendLine(string.Format(LanguageSettings.Current.Settings.ShortcutIsAlreadyDefinedX, parent.Text + " -> " + subNode.Text));
                    }
                }
            }
            if (existsIn.Length > 0 && comboBoxShortcutKey.SelectedIndex > 0)
            {
                if (MessageBox.Show(existsIn.ToString(), string.Empty, MessageBoxButtons.OKCancel, MessageBoxIcon.Warning) == DialogResult.Cancel)
                {
                    return;
                }
            }
            treeViewShortcuts.SelectedNode.Text = text + " " + shortcutText;
            AddToSaveList((ShortcutHelper)treeViewShortcuts.SelectedNode.Tag, shortcutText);
            treeViewShortcuts.Focus();
        }

        private void AddToSaveList(ShortcutHelper helper, string shortcutText)
        {
            _newShortcuts[helper] = GetShortcut(shortcutText);
        }

        private void buttonListViewSyntaxColorError_Click(object sender, EventArgs e)
        {
            using (var colorChooser = new ColorChooser { Color = panelListViewSyntaxColorError.BackColor, ShowAlpha = false })
            {
                if (colorChooser.ShowDialog() == DialogResult.OK)
                {
                    panelListViewSyntaxColorError.BackColor = colorChooser.Color;
                }
            }
        }

        private void comboBoxShortcutKey_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Tab ||
                e.KeyCode == Keys.Down ||
                e.KeyCode == Keys.Up ||
                e.KeyCode == Keys.Enter ||
                e.KeyCode == Keys.None)
            {
                return;
            }

            var i = 0;
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

        private void buttonBrowseToFFmpeg_Click(object sender, EventArgs e)
        {
            openFileDialogFFmpeg.FileName = string.Empty;
            openFileDialogFFmpeg.Title = LanguageSettings.Current.Settings.WaveformBrowseToFFmpeg;
            if (Configuration.IsRunningOnWindows)
            {
                openFileDialogFFmpeg.Filter = "FFmpeg (ffmpeg.exe)|ffmpeg.exe";
            }
            if (openFileDialogFFmpeg.ShowDialog(this) == DialogResult.OK)
            {
                textBoxFFmpegPath.Text = openFileDialogFFmpeg.FileName;
            }
        }

        private void checkBoxWaveformHoverFocus_CheckedChanged(object sender, EventArgs e)
        {
            checkBoxListViewMouseEnterFocus.Enabled = checkBoxWaveformHoverFocus.Checked;
        }

        private void buttonVlcPathBrowse_Click(object sender, EventArgs e)
        {
            openFileDialogFFmpeg.FileName = string.Empty;
            openFileDialogFFmpeg.Title = LanguageSettings.Current.Settings.WaveformBrowseToVLC;
            if (Configuration.IsRunningOnWindows)
            {
                openFileDialogFFmpeg.Filter = $"{LanguageSettings.Current.Settings.VlcMediaPlayer} (vlc.exe)|vlc.exe|" + LanguageSettings.Current.General.AllFiles + "|*.*";
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
            openFileDialogFFmpeg.Title = LanguageSettings.Current.Settings.WaveformBrowseToFFmpeg;
            openFileDialogFFmpeg.Filter = $"{LanguageSettings.Current.General.AudioFiles} (*.wav)|*.wav";
            if (openFileDialogFFmpeg.ShowDialog(this) == DialogResult.OK)
            {
                textBoxNetworkSessionNewMessageSound.Text = openFileDialogFFmpeg.FileName;
            }
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
                    {
                        radioButtonVideoPlayerMPV.Checked = true;
                    }
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
            checkBoxMpvHandlesPreviewText.Enabled = radioButtonVideoPlayerMPV.Checked;
            if (!radioButtonVideoPlayerMPV.Enabled)
            {
                buttonMpvSettings.Font = new Font(buttonMpvSettings.Font.FontFamily, buttonMpvSettings.Font.Size, FontStyle.Bold);
            }

            ShowMpvVideoOutput();
        }

        private void linkLabelBingSubscribe_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            UiUtil.OpenUrl("https://learn.microsoft.com/en-us/azure/ai-services/translator/create-translator-resource");
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
            checkBoxSyntaxColorTextMoreThanTwoLines.Text = string.Format(LanguageSettings.Current.Settings.SyntaxColorTextMoreThanMaxLines, numericUpDownMaxNumberOfLines.Value);
            ProfileUiValueChanged(sender, e);
        }

        private void radioButtonVideoPlayerMPV_CheckedChanged(object sender, EventArgs e)
        {
            checkBoxMpvHandlesPreviewText.Enabled = radioButtonVideoPlayerMPV.Checked;
            groupBoxMpvBorder.Visible = radioButtonVideoPlayerMPV.Checked && checkBoxMpvHandlesPreviewText.Checked;
            buttonMpvPrimaryColor.Visible = radioButtonVideoPlayerMPV.Checked && checkBoxMpvHandlesPreviewText.Checked;
            panelMpvPrimaryColor.Visible = radioButtonVideoPlayerMPV.Checked && checkBoxMpvHandlesPreviewText.Checked;
            buttonMpvOutlineColor.Visible = radioButtonVideoPlayerMPV.Checked && checkBoxMpvHandlesPreviewText.Checked;
            panelMpvOutlineColor.Visible = radioButtonVideoPlayerMPV.Checked && checkBoxMpvHandlesPreviewText.Checked;
            buttonMpvBackColor.Visible = radioButtonVideoPlayerMPV.Checked && checkBoxMpvHandlesPreviewText.Checked;
            panelMpvBackColor.Visible = radioButtonVideoPlayerMPV.Checked && checkBoxMpvHandlesPreviewText.Checked;
            labelMarginVertical.Visible = radioButtonVideoPlayerMPV.Checked && checkBoxMpvHandlesPreviewText.Checked;
            numericUpDownMarginVertical.Visible = radioButtonVideoPlayerMPV.Checked && checkBoxMpvHandlesPreviewText.Checked;
        }

        private void checkBoxMpvHandlesPreviewText_CheckedChanged(object sender, EventArgs e)
        {
            checkBoxMpvHandlesPreviewText.Enabled = radioButtonVideoPlayerMPV.Checked;
            groupBoxMpvBorder.Visible = radioButtonVideoPlayerMPV.Checked && checkBoxMpvHandlesPreviewText.Checked;
            buttonMpvPrimaryColor.Visible = radioButtonVideoPlayerMPV.Checked && checkBoxMpvHandlesPreviewText.Checked;
            panelMpvPrimaryColor.Visible = radioButtonVideoPlayerMPV.Checked && checkBoxMpvHandlesPreviewText.Checked;
            buttonMpvOutlineColor.Visible = radioButtonVideoPlayerMPV.Checked && checkBoxMpvHandlesPreviewText.Checked;
            panelMpvOutlineColor.Visible = radioButtonVideoPlayerMPV.Checked && checkBoxMpvHandlesPreviewText.Checked;
            buttonMpvBackColor.Visible = radioButtonVideoPlayerMPV.Checked && checkBoxMpvHandlesPreviewText.Checked;
            panelMpvBackColor.Visible = radioButtonVideoPlayerMPV.Checked && checkBoxMpvHandlesPreviewText.Checked;
            labelMarginVertical.Visible = radioButtonVideoPlayerMPV.Checked && checkBoxMpvHandlesPreviewText.Checked;
            numericUpDownMarginVertical.Visible = radioButtonVideoPlayerMPV.Checked && checkBoxMpvHandlesPreviewText.Checked;
        }

        private void buttonClearShortcut_Click(object sender, EventArgs e)
        {
            checkBoxShortcutsControl.Checked = false;
            checkBoxShortcutsAlt.Checked = false;
            checkBoxShortcutsShift.Checked = false;
            comboBoxShortcutKey.SelectedIndex = 0;
            buttonUpdateShortcut_Click(null, null);
        }

        private void buttonDownloadFfmpeg_Click(object sender, EventArgs e)
        {
            using (var form = new DownloadFfmpeg("FFmpeg"))
            {
                if (form.ShowDialog(this) == DialogResult.OK && !string.IsNullOrEmpty(form.FFmpegPath))
                {
                    textBoxFFmpegPath.Text = form.FFmpegPath;
                    checkBoxUseFFmpeg.Checked = true;
                }
            }
        }

        private void textBoxShortcutSearch_TextChanged(object sender, EventArgs e)
        {
            var oldControl = checkBoxShortcutsControl.Checked;
            var oldAlt = checkBoxShortcutsAlt.Checked;
            var oldShift = checkBoxShortcutsShift.Checked;
            var oldKeyIndex = comboBoxShortcutKey.SelectedIndex;
            ShowShortcutsTreeView();
            buttonShortcutsClear.Enabled = textBoxShortcutSearch.Text.Length > 0;
            if (treeViewShortcuts.SelectedNode?.Tag is ShortcutHelper selected)
            {
                foreach (TreeNode parentNode in treeViewShortcuts.Nodes)
                {
                    foreach (TreeNode node in parentNode.Nodes)
                    {
                        if (node.Tag is ShortcutHelper sh && sh.Shortcut.Name == selected.Shortcut.Name)
                        {
                            treeViewShortcuts.SelectedNode = node;
                            checkBoxShortcutsControl.Checked = oldControl;
                            checkBoxShortcutsAlt.Checked = oldAlt;
                            checkBoxShortcutsShift.Checked = oldShift;
                            comboBoxShortcutKey.SelectedIndex = oldKeyIndex;
                            return;
                        }
                    }
                }
            }
            comboBoxShortcutKey.Enabled = false;
            buttonUpdateShortcut.Enabled = false;
            buttonClearShortcut.Enabled = false;
        }

        private void buttonShortcutsClear_Click(object sender, EventArgs e)
        {
            textBoxShortcutSearch.Text = string.Empty;
        }

        private void linkLabelGoogleTranslateSignUp_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            UiUtil.OpenUrl("https://www.google.com/search?q=google+cloud+get+api+key");
        }

        private void buttonEditProfile_Click(object sender, EventArgs e)
        {
            using (var form = new SettingsProfile(_rulesProfiles, comboBoxRulesProfileName.Text))
            {
                if (form.ShowDialog(this) == DialogResult.OK)
                {
                    _rulesProfiles = form.RulesProfiles;
                    UpdateProfileNames(_rulesProfiles);
                }
            }
        }

        private bool _editProfileOn;
        private void comboBoxRulesProfileName_SelectedIndexChanged(object sender, EventArgs e)
        {
            _editProfileOn = true;
            var profile = _rulesProfiles.First(p => p.Name == comboBoxRulesProfileName.Text);
            numericUpDownSubtitleLineMaximumLength.Value = profile.SubtitleLineMaximumLength;
            numericUpDownOptimalCharsSec.Value = profile.SubtitleOptimalCharactersPerSeconds;
            numericUpDownMaxCharsSec.Value = profile.SubtitleMaximumCharactersPerSeconds;
            numericUpDownMaxWordsMin.Value = profile.SubtitleMaximumWordsPerMinute;
            numericUpDownDurationMin.Value = profile.SubtitleMinimumDisplayMilliseconds;
            numericUpDownDurationMax.Value = profile.SubtitleMaximumDisplayMilliseconds;
            if (profile.MinimumMillisecondsBetweenLines >= numericUpDownMinGapMs.Minimum &&
                profile.MinimumMillisecondsBetweenLines <= numericUpDownMinGapMs.Maximum)
            {
                numericUpDownMinGapMs.Value = profile.MinimumMillisecondsBetweenLines;
            }
            if (profile.MaxNumberOfLines >= numericUpDownMaxNumberOfLines.Minimum &&
                profile.MaxNumberOfLines <= numericUpDownMaxNumberOfLines.Maximum)
            {
                numericUpDownMaxNumberOfLines.Value = profile.MaxNumberOfLines;
            }
            else
            {
                numericUpDownMaxNumberOfLines.Value = numericUpDownMaxNumberOfLines.Minimum;
            }

            if (profile.MergeLinesShorterThan >= 1 && profile.MergeLinesShorterThan < comboBoxMergeShortLineLength.Items.Count)
            {
                comboBoxMergeShortLineLength.SelectedIndex = profile.MergeLinesShorterThan - 1;
            }
            else
            {
                comboBoxMergeShortLineLength.SelectedIndex = 0;
            }

            SetDialogStyle(profile.DialogStyle);
            SetContinuationStyle(profile.ContinuationStyle);
            SetCpsLineLengthStyle(profile.CpsLineLengthStrategy);
            _oldProfileId = profile.Id;
            _editProfileOn = false;
        }

        private void ProfileUiValueChanged(object sender, EventArgs e)
        {
            var idx = comboBoxRulesProfileName.SelectedIndex;
            if (idx < 0 || _editProfileOn || idx >= _rulesProfiles.Count)
            {
                return;
            }
            _rulesProfiles[idx].SubtitleLineMaximumLength = (int)numericUpDownSubtitleLineMaximumLength.Value;
            _rulesProfiles[idx].SubtitleOptimalCharactersPerSeconds = numericUpDownOptimalCharsSec.Value;
            _rulesProfiles[idx].SubtitleMaximumCharactersPerSeconds = numericUpDownMaxCharsSec.Value;
            _rulesProfiles[idx].SubtitleMinimumDisplayMilliseconds = (int)numericUpDownDurationMin.Value;
            _rulesProfiles[idx].SubtitleMaximumDisplayMilliseconds = (int)numericUpDownDurationMax.Value;
            _rulesProfiles[idx].MinimumMillisecondsBetweenLines = (int)numericUpDownMinGapMs.Value;
            _rulesProfiles[idx].MaxNumberOfLines = (int)numericUpDownMaxNumberOfLines.Value;
            _rulesProfiles[idx].SubtitleMaximumWordsPerMinute = (int)numericUpDownMaxWordsMin.Value;
            _rulesProfiles[idx].CpsLineLengthStrategy = (comboBoxCpsLineLenCalc.SelectedItem as CpsLineLength)?.Code;
            _rulesProfiles[idx].MergeLinesShorterThan = int.Parse(comboBoxMergeShortLineLength.Text, CultureInfo.InvariantCulture);
            _rulesProfiles[idx].DialogStyle = DialogSplitMerge.GetDialogStyleFromIndex(comboBoxDialogStyle.SelectedIndex);
            _rulesProfiles[idx].ContinuationStyle = ContinuationUtilities.GetContinuationStyleFromIndex(comboBoxContinuationStyle.SelectedIndex);

            toolTipContinuationPreview.RemoveAll();
            toolTipContinuationPreview.SetToolTip(comboBoxContinuationStyle, ContinuationUtilities.GetContinuationStylePreview(_rulesProfiles[idx].ContinuationStyle));

            toolTipDialogStylePreview.RemoveAll();
            toolTipDialogStylePreview.SetToolTip(comboBoxDialogStyle, DialogSplitMerge.GetDialogStylePreview(_rulesProfiles[idx].DialogStyle));

            buttonEditCustomContinuationStyle.Visible = _rulesProfiles[idx].ContinuationStyle == ContinuationStyle.Custom;
            comboBoxContinuationStyle.Width = _rulesProfiles[idx].ContinuationStyle == ContinuationStyle.Custom ? (buttonEditCustomContinuationStyle.Left - comboBoxContinuationStyle.Left - 6) : (comboBoxDialogStyle.Right - comboBoxContinuationStyle.Left);
        }

        private void checkBoxToolsBreakByPixelWidth_CheckedChanged(object sender, EventArgs e)
        {
            checkBoxToolsBreakPreferBottomHeavy.Enabled = checkBoxToolsBreakByPixelWidth.Checked;
            numericUpDownToolsBreakPreferBottomHeavy.Enabled = checkBoxToolsBreakByPixelWidth.Checked;
        }

        private void buttonReset_Click(object sender, EventArgs e)
        {
            ResetApplied = true;
            var result = MessageBox.Show(LanguageSettings.Current.Settings.RestoreDefaultSettingsMsg, LanguageSettings.Current.General.Title, MessageBoxButtons.YesNo);
            if (result == DialogResult.Yes)
            {
                Configuration.Settings.Reset();
                Configuration.Settings.General.VideoPlayer = "MPV";
                Configuration.Settings.Shortcuts = new Shortcuts();
                Init();
            }
        }

        public bool ResetApplied { get; private set; }

        private void toolStripMenuItemShortcutsCollapse_Click(object sender, EventArgs e)
        {
            treeViewShortcuts.CollapseAll();
        }

        private void buttonLineWidthSettings_Click(object sender, EventArgs e)
        {
            using (var form = new SettingsLineWidth())
            {
                if (form.ShowDialog(this) == DialogResult.OK)
                {
                    // Saving settings handled by dialog
                }
            }
        }

        private void buttonFixContinuationStyleSettings_Click(object sender, EventArgs e)
        {
            using (var form = new SettingsFixContinuationStyle())
            {
                if (form.ShowDialog(this) == DialogResult.OK)
                {
                    // Saving settings handled by dialog
                    // Quick refresh in case custom style was changed
                    ProfileUiValueChanged(sender, e);
                }
            }
        }

        private void buttonGapChoose_Click(object sender, EventArgs e)
        {
            using (var form = new SettingsGapChoose())
            {
                if (form.ShowDialog(this) == DialogResult.OK)
                {
                    numericUpDownMinGapMs.Value = form.MinGapMs;
                }
            }
        }

        private void importShortcutsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            openFileDialogFFmpeg.Title = null;
            openFileDialogFFmpeg.Filter = "Xml files|*.xml";
            openFileDialogFFmpeg.FileName = "SE_Shortcuts";
            if (openFileDialogFFmpeg.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }

            try
            {
                Configuration.Settings.Shortcuts = Shortcuts.Load(openFileDialogFFmpeg.FileName);
                textBoxShortcutSearch.Text = string.Empty;
                MakeShortcutsTreeView(LanguageSettings.Current.Settings);
                ShowShortcutsTreeView();
            }
            catch (Exception exception)
            {
                MessageBox.Show("Shortcuts not imported!" + Environment.NewLine + Environment.NewLine +
                                exception.Message + Environment.NewLine +
                                exception.StackTrace);
                SeLogger.Error(exception, "Failed to import shortcuts");
            }
        }

        private void exportShortcutsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            saveFileDialog1.Filter = "Xml files|*.xml";
            saveFileDialog1.FileName = "SE_Shortcuts";
            if (saveFileDialog1.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }

            try
            {
                var shortcuts = Configuration.Settings.Shortcuts.Clone();
                foreach (var kvp in _newShortcuts)
                {
                    kvp.Key.Shortcut.SetValue(shortcuts, kvp.Value, null);
                }

                Shortcuts.Save(saveFileDialog1.FileName, shortcuts);
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message + Environment.NewLine +
                                exception.StackTrace);
                SeLogger.Error(exception, "Failed to export shortcuts");
            }
        }

        private void buttonTextBoxHtmlColor_Click(object sender, EventArgs e)
        {
            using (var colorChooser = new ColorChooser { Color = panelTextBoxHtmlColor.BackColor, ShowAlpha = false })
            {
                if (colorChooser.ShowDialog() == DialogResult.OK)
                {
                    panelTextBoxHtmlColor.BackColor = colorChooser.Color;
                }
            }
        }

        private void buttonTextBoxAssColor_Click(object sender, EventArgs e)
        {
            using (var colorChooser = new ColorChooser { Color = panelTextBoxAssColor.BackColor, ShowAlpha = false })
            {
                if (colorChooser.ShowDialog() == DialogResult.OK)
                {
                    panelTextBoxAssColor.BackColor = colorChooser.Color;
                }
            }
        }

        private void checkBoxSubtitleTextBoxSyntaxColor_CheckedChanged(object sender, EventArgs e)
        {
            buttonTextBoxHtmlColor.Enabled = checkBoxSubtitleTextBoxSyntaxColor.Checked;
            panelTextBoxHtmlColor.Enabled = checkBoxSubtitleTextBoxSyntaxColor.Checked;
            buttonTextBoxAssColor.Enabled = checkBoxSubtitleTextBoxSyntaxColor.Checked;
            panelTextBoxAssColor.Enabled = checkBoxSubtitleTextBoxSyntaxColor.Checked;
            checkBoxLiveSpellCheck.Enabled = checkBoxSubtitleTextBoxSyntaxColor.Checked;
        }

        private void panelTextBoxHtmlColor_MouseClick(object sender, MouseEventArgs e)
        {
            buttonTextBoxHtmlColor_Click(null, null);
        }

        private void panelTextBoxAssColor_MouseClick(object sender, MouseEventArgs e)
        {
            buttonTextBoxAssColor_Click(null, null);
        }

        private void buttonDarkThemeColor_Click(object sender, EventArgs e)
        {
            using (var colorChooser = new ColorChooser { Color = panelDarkThemeColor.BackColor, ShowAlpha = false })
            {
                if (colorChooser.ShowDialog() == DialogResult.OK)
                {
                    panelDarkThemeColor.BackColor = colorChooser.Color;
                }
            }
        }

        private void buttonDarkThemeBackColor_Click(object sender, EventArgs e)
        {
            using (var colorChooser = new ColorChooser { Color = panelDarkThemeBackColor.BackColor, ShowAlpha = false })
            {
                if (colorChooser.ShowDialog() == DialogResult.OK)
                {
                    panelDarkThemeBackColor.BackColor = colorChooser.Color;
                }
            }
        }

        private void listBoxSection_LostFocus(object sender, EventArgs e)
        {
            // avoid flickering when losing focus
            listBoxSection.Update();
        }

        private void checkBoxDarkThemeEnabled_CheckedChanged(object sender, EventArgs e)
        {
            var enabled = checkBoxDarkThemeEnabled.Checked;
            buttonDarkThemeColor.Enabled = enabled;
            panelDarkThemeColor.Enabled = enabled;
            buttonDarkThemeBackColor.Enabled = enabled;
            panelDarkThemeBackColor.Enabled = enabled;
            checkBoxDarkThemeShowListViewGridLines.Enabled = enabled;

            comboBoxToolbarIconTheme_SelectedIndexChanged(null, null);
        }

        private void listBoxFavoriteSubtitleFormats_SelectedIndexChanged(object sender, EventArgs e)
        {
            buttonRemoveFromFavoriteFormats.Enabled = listBoxFavoriteSubtitleFormats.SelectedItems.Count > 0;
        }

        private void listBoxFavoriteSubtitleFormats_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyData == (Keys.Control | Keys.A))
            {
                listBoxFavoriteSubtitleFormats.SelectAll();
                e.SuppressKeyPress = true;
            }
            else if (e.KeyData == (Keys.Control | Keys.Shift | Keys.I))
            {
                listBoxFavoriteSubtitleFormats.InverseSelection();
                e.SuppressKeyPress = true;
            }
        }

        private void listBoxFavoriteSubtitleFormats_LostFocus(object sender, EventArgs e)
        {
            // avoid flickering when losing focus
            listBoxFavoriteSubtitleFormats.Update();
        }

        private void buttonMoveToFavorites_Click(object sender, EventArgs e)
        {
            foreach (var selectedItem in listBoxSubtitleFormats.SelectedItems)
            {
                if (!listBoxFavoriteSubtitleFormats.Items.Contains(selectedItem))
                {
                    listBoxFavoriteSubtitleFormats.Items.Add(selectedItem);
                }
            }
        }

        private void buttonRemoveFromFavoriteFormats_Click(object sender, EventArgs e)
        {
            if (listBoxFavoriteSubtitleFormats.SelectedIndices.Count < 1)
            {
                return;
            }

            for (int i = listBoxFavoriteSubtitleFormats.SelectedIndices.Count - 1; i >= 0; i--)
            {
                listBoxFavoriteSubtitleFormats.Items.RemoveAt(listBoxFavoriteSubtitleFormats.SelectedIndices[i]);
            }
        }

        private void textBoxFormatsSearch_TextChanged(object sender, EventArgs e)
        {
            listBoxSubtitleFormats.BeginUpdate();
            listBoxSubtitleFormats.Items.Clear();
            if (textBoxFormatsSearch.Text.Length > 0)
            {
                buttonFormatsSearchClear.Enabled = true;
                var results = SubtitleFormat.AllSubtitleFormats.Where(format => !format.IsVobSubIndexFile && format.FriendlyName.Contains(textBoxFormatsSearch.Text, StringComparison.OrdinalIgnoreCase)).Select(format => format.FriendlyName);
                listBoxSubtitleFormats.Items.AddRange(results.ToArray<object>());
            }
            else
            {
                var formatNames = GetSubtitleFormats();
                listBoxSubtitleFormats.Items.AddRange(formatNames.ToArray<object>());
                buttonFormatsSearchClear.Enabled = false;
            }
            listBoxSubtitleFormats.EndUpdate();

            buttonMoveToFavoriteFormats.Enabled = listBoxSubtitleFormats.SelectedItems.Count > 0;
        }

        private void buttonFormatsSearchClear_Click(object sender, EventArgs e)
        {
            textBoxFormatsSearch.Text = string.Empty;
            labelFormatsSearch.Focus();
        }

        private void listBoxSubtitleFormats_SelectedIndexChanged(object sender, EventArgs e)
        {
            var enabled = listBoxSubtitleFormats.SelectedItems.Count > 0;
            if (!buttonMoveToFavoriteFormats.Enabled)
            {
                buttonMoveToFavoriteFormats.Enabled = listBoxSubtitleFormats.SelectedItems.Count > 0;
            }
        }

        private void listBoxSubtitleFormats_LostFocus(object sender, EventArgs e)
        {
            // avoid flickering when losing focus
            listBoxSubtitleFormats.Update();
        }

        private void contextMenuStripFavoriteFormats_Opening(object sender, CancelEventArgs e)
        {
            var oneOrMoreSelected = listBoxFavoriteSubtitleFormats.SelectedItems.Count > 0;
            deleteToolStripMenuItem.Enabled = oneOrMoreSelected;

            var oneOrMoreExist = listBoxFavoriteSubtitleFormats.Items.Count > 0;
            deleteAllToolStripMenuItem.Enabled = oneOrMoreExist;

            var onlyOneSelected = listBoxFavoriteSubtitleFormats.SelectedItems.Count == 1;
            var moreThanOneExist = listBoxFavoriteSubtitleFormats.Items.Count > 1;
            moveUpToolStripMenuItem.Enabled = onlyOneSelected && moreThanOneExist;
            moveDownToolStripMenuItem.Enabled = onlyOneSelected && moreThanOneExist;
            moveToTopToolStripMenuItem.Enabled = onlyOneSelected && moreThanOneExist;
            moveToBottomToolStripMenuItem.Enabled = onlyOneSelected && moreThanOneExist;
        }

        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            buttonRemoveFromFavoriteFormats_Click(sender, e);
        }

        private void deleteAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            listBoxFavoriteSubtitleFormats.Items.Clear();
        }

        private void moveUpToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listBoxFavoriteSubtitleFormats.SelectedItems.Count != 1 || listBoxFavoriteSubtitleFormats.Items.Count < 2)
            {
                return;
            }

            var idx = listBoxFavoriteSubtitleFormats.SelectedIndex;
            if (idx <= 0)
            {
                return;
            }

            var item = listBoxFavoriteSubtitleFormats.SelectedItem;
            listBoxFavoriteSubtitleFormats.Items.RemoveAt(idx);
            idx--;
            listBoxFavoriteSubtitleFormats.Items.Insert(idx, item);
            listBoxFavoriteSubtitleFormats.SetSelected(idx, true);
        }

        private void moveDownToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listBoxFavoriteSubtitleFormats.SelectedItems.Count != 1 || listBoxFavoriteSubtitleFormats.Items.Count < 2)
            {
                return;
            }

            var idx = listBoxFavoriteSubtitleFormats.SelectedIndex;
            if (idx >= listBoxFavoriteSubtitleFormats.Items.Count - 1)
            {
                return;
            }

            var item = listBoxFavoriteSubtitleFormats.SelectedItem;
            listBoxFavoriteSubtitleFormats.Items.RemoveAt(idx);
            idx++;
            listBoxFavoriteSubtitleFormats.Items.Insert(idx, item);
            listBoxFavoriteSubtitleFormats.SetSelected(idx, true);
        }

        private void moveToTopToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listBoxFavoriteSubtitleFormats.SelectedItems.Count != 1 || listBoxFavoriteSubtitleFormats.Items.Count < 2)
            {
                return;
            }

            var idx = listBoxFavoriteSubtitleFormats.SelectedIndex;
            if (idx == 0)
            {
                return;
            }

            var item = listBoxFavoriteSubtitleFormats.SelectedItem;
            listBoxFavoriteSubtitleFormats.Items.RemoveAt(idx);
            listBoxFavoriteSubtitleFormats.Items.Insert(0, item);
            listBoxFavoriteSubtitleFormats.SetSelected(0, true);
        }

        private void moveToBottomToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listBoxFavoriteSubtitleFormats.SelectedItems.Count != 1 || listBoxFavoriteSubtitleFormats.Items.Count < 2)
            {
                return;
            }

            var idx = listBoxFavoriteSubtitleFormats.SelectedIndex;
            if (idx == listBoxFavoriteSubtitleFormats.Items.Count - 1)
            {
                return;
            }

            var item = listBoxFavoriteSubtitleFormats.SelectedItem;
            listBoxFavoriteSubtitleFormats.Items.RemoveAt(idx);
            listBoxFavoriteSubtitleFormats.Items.Insert(listBoxFavoriteSubtitleFormats.Items.Count, item);
            listBoxFavoriteSubtitleFormats.SetSelected(listBoxFavoriteSubtitleFormats.Items.Count - 1, true);
        }

        private void FillFileTypeAssociationsListView()
        {
            var iconDir = Path.Combine(Configuration.BaseDirectory, "Icons");
            if (!Directory.Exists(iconDir))
            {
                listViewFileTypeAssociations.Visible = false;
                buttonUpdateFileTypeAssociations.Visible = false;
                return;
            }

            var iconFileNames = Directory.GetFiles(iconDir, "*.ico");
            imageListFileTypeAssociations.Images.Clear();
            listViewFileTypeAssociations.HeaderStyle = ColumnHeaderStyle.None;
            listViewFileTypeAssociations.SmallImageList = imageListFileTypeAssociations;
            listViewFileTypeAssociations.BeginUpdate();
            listViewFileTypeAssociations.Items.Clear();
            listViewFileTypeAssociations.GridLines = false;
            listViewFileTypeAssociations.FullRowSelect = true;
            foreach (var iconFileName in iconFileNames)
            {
                if (iconFileName.EndsWith("uninstall.ico", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                var friendlyName = "." + Path.GetFileNameWithoutExtension(iconFileName).ToUpperInvariant();
                var icon = new Icon(iconFileName);
                imageListFileTypeAssociations.Images.Add(icon);
                var item = new ListViewItem(string.Empty);
                item.SubItems.Add(friendlyName);
                item.ImageIndex = imageListFileTypeAssociations.Images.Count - 1;
                item.Checked = FileTypeAssociations.GetChecked("." + Path.GetFileNameWithoutExtension(iconFileName).ToLowerInvariant(), "SubtitleEdit");
                item.Tag = iconFileName;
                listViewFileTypeAssociations.Items.Add(item);
            }
            listViewFileTypeAssociations.EndUpdate();
            listViewFileTypeAssociations.AutoSizeLastColumn();
        }

        private void buttonUpdateFileTypeAssociations_Click(object sender, EventArgs e)
        {
            var exeFileName = Assembly.GetEntryAssembly().Location;
            foreach (ListViewItem item in listViewFileTypeAssociations.Items)
            {
                var ext = item.SubItems[1].Text.ToLowerInvariant();
                if (item.Checked)
                {
                    var iconFileName = (string)item.Tag;
                    FileTypeAssociations.SetFileAssociationViaRegistry(ext, exeFileName, iconFileName, "SubtitleEdit");
                }
                else
                {
                    FileTypeAssociations.DeleteFileAssociationViaRegistry(ext, "SubtitleEdit");
                }
            }

            labelUpdateFileTypeAssociationsStatus.Text = LanguageSettings.Current.Settings.FileTypeAssociationsUpdated;
            FileTypeAssociations.Refresh();
            TaskDelayHelper.RunDelayed(TimeSpan.FromMilliseconds(3000), () => labelUpdateFileTypeAssociationsStatus.Text = string.Empty);
        }

        private void buttonEditCustomContinuationStyle_Click(object sender, EventArgs e)
        {
            using (var form = new SettingsCustomContinuationStyle())
            {
                if (form.ShowDialog(this) == DialogResult.OK)
                {
                    // Saving settings handled by dialog
                    // Quick refresh
                    ProfileUiValueChanged(sender, e);
                }
            }
        }

        private void buttonTranslationAutoSuffix_Click(object sender, EventArgs e)
        {
            var suffixes = new List<string>();
            foreach (var item in comboBoxTranslationAutoSuffix.Items)
            {
                suffixes.Add(item.ToString());
            }

            using (var form = new TranslationAutoSuffix(suffixes))
            {
                if (form.ShowDialog(this) == DialogResult.OK)
                {
                    comboBoxTranslationAutoSuffix.Items.Clear();
                    comboBoxTranslationAutoSuffix.BeginUpdate();
                    foreach (var suffix in form.Suffixes)
                    {
                        comboBoxTranslationAutoSuffix.Items.Add(suffix);
                    }
                    comboBoxTranslationAutoSuffix.EndUpdate();
                }
            }
        }

        private void exportAsHtmlToolStripMenuItem_Click(object sender, EventArgs e)
        {
            saveFileDialog1.Filter = "Html files|*.html";
            saveFileDialog1.FileName = "SE_Shortcuts";
            if (saveFileDialog1.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }

            var html = new StringBuilder();
            html.AppendLine("<!DOCTYPE html>");
            html.AppendLine("<html lang=\"en\">");
            html.AppendLine("<head>");
            html.AppendLine("  <title>Subtitle Edit shortcuts</title>");
            html.AppendLine("  <link rel=\"stylesheet\" href=\"https://maxcdn.bootstrapcdn.com/bootstrap/3.4.1/css/bootstrap.min.css\">");
            html.AppendLine("</head>");
            html.AppendLine("<body>");
            html.AppendLine("<h2>Subtitle Edit shortcuts</h2>");
            html.AppendLine("<br />");
            html.AppendLine("<table id=\"tableShortcuts\" class=\"table table-striped\">");
            html.AppendLine("  <thead>");
            html.AppendLine("    <tr>");
            html.AppendLine("      <th>Control</th>");
            html.AppendLine("      <th>Function</th>");
            html.AppendLine("      <th>Shortcut</th>");
            html.AppendLine("    </tr>");
            html.AppendLine("  </thead>");
            html.AppendLine("  <tbody>");
            foreach (TreeNode node in treeViewShortcuts.Nodes)
            {
                html.AppendLine("      <tr>");
                html.AppendLine($"        <td><b>{node.Text}</b></td>");
                html.AppendLine("        <td></td>");
                html.AppendLine("        <td></td>");
                html.AppendLine("      </tr>");
                foreach (TreeNode shortcutNode in node.Nodes)
                {
                    var indexOfBracket = shortcutNode.Text.IndexOf('[');
                    var description = shortcutNode.Text
                        .Replace("&", "&amp;")
                        .Replace("<", "&lt;")
                        .Replace(">", "&gt;")
                        .Replace("\"", "&quot;")
                        .Replace("{", "&#123;")
                        .Replace("}", "&#125;");
                    var shortcut = string.Empty;
                    if (indexOfBracket >= 0)
                    {
                        shortcut = description.Substring(indexOfBracket).Trim(' ', '[', ']');
                        if (shortcut == "None")
                        {
                            shortcut = string.Empty;
                        }

                        description = description.Substring(0, indexOfBracket).Trim(' ', '[');
                    }

                    html.AppendLine("      <tr>");
                    html.AppendLine($"        <td>{node.Text}</td>");
                    html.AppendLine($"        <td>{description}</td>");
                    html.AppendLine($"        <td>{shortcut}</td>");
                    html.AppendLine("      </tr>");
                }
            }

            html.AppendLine("  </tbody>");
            html.AppendLine("</body>");
            html.AppendLine("</html>");

            File.WriteAllText(saveFileDialog1.FileName, html.ToString());
            UiUtil.OpenFolderFromFileName(saveFileDialog1.FileName);
        }

        private void buttonMpvPrimaryColor_Click(object sender, EventArgs e)
        {
            using (var colorChooser = new ColorChooser { Color = panelMpvPrimaryColor.BackColor, ShowAlpha = false })
            {
                if (colorChooser.ShowDialog() == DialogResult.OK)
                {
                    panelMpvPrimaryColor.BackColor = colorChooser.Color;
                }
            }
        }

        private void buttonMpvOutlineColor_Click(object sender, EventArgs e)
        {
            using (var colorChooser = new ColorChooser { Color = panelMpvOutlineColor.BackColor })
            {
                if (colorChooser.ShowDialog() == DialogResult.OK)
                {
                    panelMpvOutlineColor.BackColor = colorChooser.Color;
                }
            }
        }

        private void buttonMpvBackColor_Click(object sender, EventArgs e)
        {
            using (var colorChooser = new ColorChooser { Color = panelMpvBackColor.BackColor })
            {
                if (colorChooser.ShowDialog() == DialogResult.OK)
                {
                    panelMpvBackColor.BackColor = colorChooser.Color;
                }
            }
        }

        private void buttonEditShotChangesProfile_Click(object sender, EventArgs e)
        {
            using (var form = new BeautifyTimeCodesProfile(Configuration.Settings.General.CurrentFrameRate))
            {
                form.ShowDialog(this);
            }
        }

        private void comboBoxToolbarIconTheme_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_loading)
            {
                return;
            }

            TryLoadIcon(pictureBoxPreview1, "New");
            TryLoadIcon(pictureBoxPreview2, "Open");
            TryLoadIcon(pictureBoxPreview3, "Save");

            TryLoadIcon(pictureBoxFileNew, "New");
            TryLoadIcon(pictureBoxTBOpen, "Open");
            TryLoadIcon(pictureBoxTBSave, "Save");
            TryLoadIcon(pictureBoxTBSaveAs, "SaveAs");
            TryLoadIcon(pictureBoxTBFind, "Find");
            TryLoadIcon(pictureBoxTBReplace, "Replace");
            TryLoadIcon(pictureBoxTBFixCommonErrors, "FixCommonErrors");
            TryLoadIcon(pictureBoxRemoveTextForHi, "RemoveTextForHi");
            TryLoadIcon(pictureBoxTBVisualSync, "VisualSync");
            TryLoadIcon(pictureBoxTBBurnIn, "BurnIn");
            TryLoadIcon(pictureBoxTBSpellCheck, "SpellCheck");
            TryLoadIcon(pictureBoxTBNetflixQualityCheck, "Netflix");
            TryLoadIcon(pictureBoxTBBeautifyTimeCodes, "BeautifyTimeCodes");
            TryLoadIcon(pictureBoxAssStyleManager, "AssaStyle");
            TryLoadIcon(pictureBoxAssProperties, "AssaProperties");
            TryLoadIcon(pictureBoxAssAttachments, "AssaAttachments");
            TryLoadIcon(pictureBoxAssaDraw, "AssaDraw");
            TryLoadIcon(pictureBoxTBSettings, "Settings");
            TryLoadIcon(pictureBoxTBHelp, "Help");
            TryLoadIcon(pictureBoxToggleWaveform, "WaveformToggle");
            TryLoadIcon(pictureBoxToggleVideo, "VideoToggle");
            TryLoadIcon(pictureBoxTBToggleSourceView, "SourceView");
            TryLoadIcon(pictureBoxIttProperties, "IttProperties");
            TryLoadIcon(pictureBoxWebVttProperties, "WebVttProperties");
            TryLoadIcon(pictureBoxWebVttStyle, "WebVttStyle");
            TryLoadIcon(pictureBoxEbuProperties, "EbuProperties");
        }

        private void TryLoadIcon(PictureBox pictureBox, string iconName)
        {
            pictureBox.Image?.Dispose();
            pictureBox.Image = null;

            var theme = checkBoxDarkThemeEnabled.Checked ? "DarkTheme" : "DefaultTheme";
            if (comboBoxToolbarIconTheme.SelectedIndex != 0)
            {
                theme = comboBoxToolbarIconTheme.Text;
            }

            var themeFullPath = Path.Combine(Configuration.IconsDirectory, theme, iconName + ".png");
            if (File.Exists(themeFullPath))
            {
                pictureBox.Image = new Bitmap(themeFullPath);
                return;
            }

            var fullPath = Path.Combine(Configuration.IconsDirectory, "DefaultTheme", iconName + ".png");
            if (File.Exists(fullPath))
            {
                pictureBox.Image = new Bitmap(fullPath);
            }
        }

        private void buttonDefaultLanguages_Click(object sender, EventArgs e)
        {
            using (var form = new DefaultLanguagesChooser(_defaultLanguages))
            {
                if (form.ShowDialog(this) == DialogResult.OK)
                {
                    _defaultLanguages = form.DefaultLanguages;
                    InitLanguageList();
                }
            }
        }

        private void InitLanguageList()
        {
            if (string.IsNullOrEmpty(_defaultLanguages))
            {
                labelDefaultLanguagesList.Text = LanguageSettings.Current.General.All;
                return;
            }

            var arr = _defaultLanguages.Split(';');
            if (_defaultLanguages.Length > 25)
            {
                labelDefaultLanguagesList.Text = arr.Length.ToString();
                return;
            }

            labelDefaultLanguagesList.Text = string.Join(", ", arr);
        }

        private void linkLabelNllbApi_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            UiUtil.OpenUrl(new NoLanguageLeftBehindApi().Url);
        }

        private void linkLabelNllbServe_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            UiUtil.OpenUrl(new NoLanguageLeftBehindServe().Url);
        }

        private void linkLabelLibreTranslateApi_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            UiUtil.OpenUrl(new LibreTranslate().Url);
        }

        private void linkLabelMyMemoryTranslate_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            UiUtil.OpenUrl(new MyMemoryApi().Url);
        }

        private void LinkLabelMoreInfoDeepLLinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            UiUtil.OpenUrl(new DeepLTranslate().Url);
        }

        private void nikseComboBoxShortcutsFilter_SelectedIndexChanged(object sender, EventArgs e)
        {
            textBoxShortcutSearch_TextChanged(null, null);
        }
    }
}
