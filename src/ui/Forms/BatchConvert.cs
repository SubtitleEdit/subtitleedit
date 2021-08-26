using Nikse.SubtitleEdit.Core.BluRaySup;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.ContainerFormats.Matroska;
using Nikse.SubtitleEdit.Core.ContainerFormats.TransportStream;
using Nikse.SubtitleEdit.Core.Forms;
using Nikse.SubtitleEdit.Core.Interfaces;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Core.VobSub;
using Nikse.SubtitleEdit.Forms.Ocr;
using Nikse.SubtitleEdit.Forms.Styles;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.CommandLineConvert;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.Forms
{
    public sealed partial class BatchConvert : PositionAndSizeForm
    {
        private const string AddUnicode = "ADD_UNICODE";
        private const string RemoveUnicode = "REMOVE_UNICODE";
        private const string ReverseStartEnd = "REVERSE_START_END";

        public class ThreadDoWorkParameter
        {
            public bool FixCommonErrors { get; set; }
            public bool MultipleReplaceActive { get; set; }
            public bool FixRtl { get; set; }
            public bool SplitLongLinesActive { get; set; }
            public bool AutoBalanceActive { get; set; }
            public bool SetMinDisplayTimeBetweenSubtitles { get; set; }
            public ListViewItem Item { get; set; }
            public Subtitle Subtitle { get; set; }
            public SubtitleFormat Format { get; set; }
            public TextEncoding Encoding { get; set; }
            public string Language { get; set; }
            public string Error { get; set; }
            public string FileName { get; set; }
            public string ToFormat { get; set; }
            public SubtitleFormat SourceFormat { get; set; }
            public List<IBinaryParagraph> BinaryParagraphs { get; set; }
            public ThreadDoWorkParameter(
                bool fixCommonErrors,
                bool multipleReplace,
                bool fixRtl,
                bool splitLongLinesActive,
                bool autoBalance,
                bool setMinDisplayTimeBetweenSubtitles,
                ListViewItem item,
                Subtitle subtitle,
                SubtitleFormat format,
                TextEncoding encoding,
                string language,
                string fileName,
                string toFormat,
                SubtitleFormat sourceFormat,
                List<IBinaryParagraph> binaryParagraphs)
            {
                FixCommonErrors = fixCommonErrors;
                MultipleReplaceActive = multipleReplace;
                FixRtl = fixRtl;
                SplitLongLinesActive = splitLongLinesActive;
                AutoBalanceActive = autoBalance;
                SetMinDisplayTimeBetweenSubtitles = setMinDisplayTimeBetweenSubtitles;
                Item = item;
                Subtitle = subtitle;
                Format = format;
                Encoding = encoding;
                Language = language;
                FileName = fileName;
                ToFormat = toFormat;
                SourceFormat = sourceFormat;
                BinaryParagraphs = binaryParagraphs;
            }
        }

        private string _assStyle;
        private string _ssaStyle;
        private readonly RemoveTextForHI _removeTextForHearingImpaired;
        private readonly ChangeCasing _changeCasing = new ChangeCasing();
        private readonly ChangeCasingNames _changeCasingNames = new ChangeCasingNames();
        private bool _converting;
        private int _count;
        private int _converted;
        private int _errors;
        private readonly List<SubtitleFormat> _allFormats;
        private bool _searching;
        private bool _abort;
        private Ebu.EbuGeneralSubtitleInformation _ebuGeneralInformation;
        public static string BluRaySubtitle => "Blu-ray sup";
        public static string VobSubSubtitle => "VobSub";
        public static string DostImageSubtitle => "Dost-image";
        public static string BdnXmlSubtitle => "BDN-XML";
        public static string FcpImageSubtitle => "FCP-image";
        private string _customTextTemplate;
        private readonly DurationsBridgeGaps _bridgeGaps;
        private const int ConvertMaxFileSize = 1024 * 1024 * 10; // 10 MB
        private Dictionary<string, List<BluRaySupParser.PcsData>> _bdLookup = new Dictionary<string, List<BluRaySupParser.PcsData>>();
        RemoveTextForHISettings _removeTextForHiSettings;

        public BatchConvert(Icon icon)
        {
            UiUtil.PreInitialize(this);
            InitializeComponent();
            UiUtil.FixFonts(this);
            Icon = (Icon)icon.Clone();

            progressBar1.Visible = false;
            labelStatus.Text = string.Empty;
            var l = LanguageSettings.Current.BatchConvert;
            Text = l.Title;
            groupBoxInput.Text = l.Input;
            labelChooseInputFiles.Text = l.InputDescription;
            groupBoxOutput.Text = l.Output;
            radioButtonSaveInSourceFolder.Text = l.SaveInSourceFolder;
            radioButtonSaveInOutputFolder.Text = l.SaveInOutputFolder;
            checkBoxOverwrite.Text = l.OverwriteFiles;
            labelOutputFormat.Text = LanguageSettings.Current.Main.Controls.SubtitleFormat;
            labelEncoding.Text = LanguageSettings.Current.Main.Controls.FileEncoding;
            buttonStyles.Text = l.Style;
            checkBoxUseStyleFromSource.Text = l.UseStyleFromSource;
            groupBoxConvertOptions.Text = l.ConvertOptions;
            columnHeaderFName.Text = LanguageSettings.Current.JoinSubtitles.FileName;
            columnHeaderFormat.Text = LanguageSettings.Current.Main.Controls.SubtitleFormat;
            columnHeaderSize.Text = LanguageSettings.Current.General.Size;
            columnHeaderStatus.Text = l.Status;
            linkLabelOpenOutputFolder.Text = LanguageSettings.Current.Main.Menu.File.Open;
            buttonSearchFolder.Text = l.ScanFolder;
            buttonConvert.Text = l.Convert;
            buttonCancel.Text = LanguageSettings.Current.General.Ok;
            checkBoxScanFolderRecursive.Text = l.Recursive;
            checkBoxScanFolderRecursive.Left = buttonSearchFolder.Left - checkBoxScanFolderRecursive.Width - 5;
            buttonTransportStreamSettings.Text = l.TransportStreamSettingsButton;
            groupBoxChangeFrameRate.Text = LanguageSettings.Current.ChangeFrameRate.Title;
            groupBoxOffsetTimeCodes.Text = l.OffsetTimeCodes;
            groupBoxSpeed.Text = LanguageSettings.Current.ChangeSpeedInPercent.TitleShort;
            labelFromFrameRate.Text = LanguageSettings.Current.ChangeFrameRate.FromFrameRate;
            labelToFrameRate.Text = LanguageSettings.Current.ChangeFrameRate.ToFrameRate;
            labelHourMinSecMilliSecond.Text = Configuration.Settings.General.UseTimeFormatHHMMSSFF ? LanguageSettings.Current.General.HourMinutesSecondsFrames : string.Format(LanguageSettings.Current.General.HourMinutesSecondsDecimalSeparatorMilliseconds, UiUtil.DecimalSeparator);
            openContainingFolderToolStripMenuItem.Text = LanguageSettings.Current.Main.Menu.File.OpenContainingFolder;
            removeToolStripMenuItem.Text = LanguageSettings.Current.MultipleReplace.Remove;
            removeAllToolStripMenuItem.Text = LanguageSettings.Current.MultipleReplace.RemoveAll;
            groupBoxRemoveStyle.Text = l.RemoveStyleActor;
            labelStyleName.Text = $"{LanguageSettings.Current.General.Style}/{LanguageSettings.Current.General.Actor}";
            labelDeleteFirstLines.Text = l.DeleteFirstLines;
            labelDeleteLastLines.Text = l.DeleteLastLines;
            labelDeleteLinesContaining.Text = l.DeleteContaining;
            numericUpDownDeleteFirst.Left = labelDeleteFirstLines.Left + labelDeleteFirstLines.Width + 5;
            numericUpDownDeleteLast.Left = labelDeleteLastLines.Left + labelDeleteLastLines.Width + 5;
            groupBoxAdjustDuration.Text = LanguageSettings.Current.AdjustDisplayDuration.Title;
            addFilesToolStripMenuItem.Text = l.AddFiles;
            groupBoxDeleteLines.Text = l.DeleteLines;

            comboBoxFrameRateFrom.Left = labelFromFrameRate.Left + labelFromFrameRate.Width + 3;
            comboBoxFrameRateTo.Left = labelToFrameRate.Left + labelToFrameRate.Width + 3;
            if (comboBoxFrameRateFrom.Left > comboBoxFrameRateTo.Left)
            {
                comboBoxFrameRateTo.Left = comboBoxFrameRateFrom.Left;
            }
            else
            {
                comboBoxFrameRateFrom.Left = comboBoxFrameRateTo.Left;
            }

            buttonSwapFrameRate.Left = comboBoxFrameRateFrom.Left + comboBoxFrameRateFrom.Width + 10;
            buttonSwapFrameRate.Top = comboBoxFrameRateFrom.Top + ((comboBoxFrameRateTo.Top + comboBoxFrameRateTo.Height - comboBoxFrameRateFrom.Top) / 2) - (buttonSwapFrameRate.Height / 2) + 1;

            comboBoxSubtitleFormats.Left = labelOutputFormat.Left + labelOutputFormat.Width + 3;
            comboBoxEncoding.Left = labelEncoding.Left + labelEncoding.Width + 3;
            if (comboBoxSubtitleFormats.Left > comboBoxEncoding.Left)
            {
                comboBoxEncoding.Left = comboBoxSubtitleFormats.Left;
            }
            else
            {
                comboBoxSubtitleFormats.Left = comboBoxEncoding.Left;
            }
            buttonBrowseEncoding.Left = comboBoxEncoding.Left + comboBoxEncoding.Width + 5;
            buttonStyles.Left = comboBoxSubtitleFormats.Left + comboBoxSubtitleFormats.Width + 5;
            buttonTransportStreamSettings.Left = buttonStyles.Left;

            timeUpDownAdjust.MaskedTextBox.Text = "000000000";

            comboBoxFrameRateFrom.Items.Add(23.976);
            comboBoxFrameRateFrom.Items.Add(24.0);
            comboBoxFrameRateFrom.Items.Add(25.0);
            comboBoxFrameRateFrom.Items.Add(29.97);
            comboBoxFrameRateFrom.Items.Add(30.0);

            comboBoxFrameRateTo.Items.Add(23.976);
            comboBoxFrameRateTo.Items.Add(24.0);
            comboBoxFrameRateTo.Items.Add(25.0);
            comboBoxFrameRateTo.Items.Add(29.97);
            comboBoxFrameRateTo.Items.Add(30.0);

            UiUtil.FixLargeFonts(this, buttonCancel);

            _allFormats = new List<SubtitleFormat> { new Pac() };
            var formatNames = new List<string>();
            foreach (var format in SubtitleFormat.AllSubtitleFormats)
            {
                if (!format.IsVobSubIndexFile)
                {
                    formatNames.Add(format.Name);
                    _allFormats.Add(format);
                }
            }
            formatNames.Add("PAC");
            formatNames.Add(new Ayato().Name);
            formatNames.Add(l.PlainText);
            formatNames.Add(BluRaySubtitle);
            formatNames.Add(VobSubSubtitle);
            formatNames.Add(DostImageSubtitle);
            formatNames.Add(BdnXmlSubtitle);
            formatNames.Add(FcpImageSubtitle);
            formatNames.Add(LanguageSettings.Current.ExportCustomText.Title);
            UiUtil.InitializeSubtitleFormatComboBox(comboBoxSubtitleFormats, formatNames, Configuration.Settings.Tools.BatchConvertFormat);

            UiUtil.InitializeTextEncodingComboBox(comboBoxEncoding);
            comboBoxEncoding.Items.Add(new TextEncoding(Encoding.UTF8, l.TryToUseSourceEncoding));

            if (string.IsNullOrEmpty(Configuration.Settings.Tools.BatchConvertOutputFolder) || !Directory.Exists(Configuration.Settings.Tools.BatchConvertOutputFolder))
            {
                textBoxOutputFolder.Text = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            }
            else
            {
                textBoxOutputFolder.Text = Configuration.Settings.Tools.BatchConvertOutputFolder;
            }

            checkBoxOverwrite.Checked = Configuration.Settings.Tools.BatchConvertOverwriteExisting;
            buttonConvertOptionsSettings.Text = l.Settings;
            radioButtonShowEarlier.Text = LanguageSettings.Current.ShowEarlierLater.ShowEarlier.RemoveChar('&');
            radioButtonShowLater.Text = LanguageSettings.Current.ShowEarlierLater.ShowLater.RemoveChar('&');
            radioButtonSpeedCustom.Text = LanguageSettings.Current.ChangeSpeedInPercent.Custom;
            radioButtonSpeedFromDropFrame.Text = LanguageSettings.Current.ChangeSpeedInPercent.FromDropFrame;
            radioButtonToDropFrame.Text = LanguageSettings.Current.ChangeSpeedInPercent.ToDropFrame;
            if (Configuration.Settings.Tools.BatchConvertSaveInSourceFolder)
            {
                radioButtonSaveInSourceFolder.Checked = true;
            }
            else
            {
                radioButtonSaveInOutputFolder.Checked = true;
            }

            groupBoxChangeCasing.Text = LanguageSettings.Current.ChangeCasing.ChangeCasingTo;
            radioButtonNormal.Text = LanguageSettings.Current.ChangeCasing.NormalCasing;
            radioButtonFixOnlyNames.Text = LanguageSettings.Current.ChangeCasing.FixOnlyNamesCasing;
            radioButtonUppercase.Text = LanguageSettings.Current.ChangeCasing.AllUppercase;
            radioButtonLowercase.Text = LanguageSettings.Current.ChangeCasing.AllLowercase;
            if (Configuration.Settings.Tools.ChangeCasingChoice == "NamesOnly")
            {
                radioButtonFixOnlyNames.Checked = true;
            }
            else if (Configuration.Settings.Tools.ChangeCasingChoice == "Uppercase")
            {
                radioButtonUppercase.Checked = true;
            }
            else if (Configuration.Settings.Tools.ChangeCasingChoice == "Lowercase")
            {
                radioButtonLowercase.Checked = true;
            }

            _removeTextForHearingImpaired = new RemoveTextForHI(new RemoveTextForHISettings(new Subtitle()));
            _removeTextForHiSettings = _removeTextForHearingImpaired.Settings;

            labelFilter.Text = l.Filter;
            comboBoxFilter.Items[0] = LanguageSettings.Current.General.AllFiles;
            comboBoxFilter.Items[1] = l.FilterSrtNoUtf8BOM;
            comboBoxFilter.Items[2] = l.FilterMoreThanTwoLines;
            comboBoxFilter.Items[3] = l.FilterContains;
            comboBoxFilter.Items[4] = l.FilterFileNameContains;
            comboBoxFilter.Items[5] = l.MkvLanguageCodeContains;
            comboBoxFilter.SelectedIndex = 0;
            comboBoxFilter.Left = labelFilter.Left + labelFilter.Width + 4;
            textBoxFilter.Left = comboBoxFilter.Left + comboBoxFilter.Width + 4;

            _assStyle = Configuration.Settings.Tools.BatchConvertAssStyles;
            _ssaStyle = Configuration.Settings.Tools.BatchConvertSsaStyles;
            checkBoxUseStyleFromSource.Checked = Configuration.Settings.Tools.BatchConvertUseStyleFromSource;
            _customTextTemplate = Configuration.Settings.Tools.BatchConvertExportCustomTextTemplate;

            comboBoxSubtitleFormats.AutoCompleteSource = AutoCompleteSource.ListItems;
            comboBoxSubtitleFormats.AutoCompleteMode = AutoCompleteMode.Append;

            _bridgeGaps = new DurationsBridgeGaps(null);
            _bridgeGaps.InitializeSettingsOnly();
            buttonTransportStreamSettings.Visible = false;

            groupBoxFixRtl.Text = LanguageSettings.Current.BatchConvert.Settings;
            radioButtonAddUnicode.Text = LanguageSettings.Current.BatchConvert.FixRtlAddUnicode;
            radioButtonRemoveUnicode.Text = LanguageSettings.Current.BatchConvert.FixRtlRemoveUnicode;
            radioButtonReverseStartEnd.Text = LanguageSettings.Current.BatchConvert.FixRtlReverseStartEnd;


            labelAdjustDurationVia.Text = LanguageSettings.Current.AdjustDisplayDuration.AdjustVia;
            comboBoxAdjustDurationVia.Left = labelAdjustDurationVia.Left + labelAdjustDurationVia.Width + 5;
            comboBoxAdjustDurationVia.Items.Clear();
            comboBoxAdjustDurationVia.Items.Add(LanguageSettings.Current.AdjustDisplayDuration.AddSeconds);
            comboBoxAdjustDurationVia.Items.Add(LanguageSettings.Current.AdjustDisplayDuration.Percent);
            comboBoxAdjustDurationVia.Items.Add(LanguageSettings.Current.AdjustDisplayDuration.Recalculate);
            comboBoxAdjustDurationVia.Items.Add(LanguageSettings.Current.AdjustDisplayDuration.Fixed);
            switch (Configuration.Settings.Tools.AdjustDurationLast)
            {
                case AdjustDisplayDuration.Sec:
                    comboBoxAdjustDurationVia.SelectedIndex = 0;
                    break;
                case AdjustDisplayDuration.Per:
                    comboBoxAdjustDurationVia.SelectedIndex = 1;
                    break;
                case AdjustDisplayDuration.Recal:
                    comboBoxAdjustDurationVia.SelectedIndex = 2;
                    break;
                case AdjustDisplayDuration.Fixed:
                    comboBoxAdjustDurationVia.SelectedIndex = 3;
                    break;
                default:
                    comboBoxAdjustDurationVia.SelectedIndex = 0;
                    break;
            }
            decimal adjustSeconds = Configuration.Settings.Tools.AdjustDurationSeconds;
            if (adjustSeconds >= numericUpDownSeconds.Minimum && adjustSeconds <= numericUpDownSeconds.Maximum)
            {
                numericUpDownSeconds.Value = adjustSeconds;
            }

            int adjustPercent = Configuration.Settings.Tools.AdjustDurationPercent;
            if (adjustPercent >= numericUpDownAdjustViaPercent.Minimum && adjustPercent <= numericUpDownAdjustViaPercent.Maximum)
            {
                numericUpDownAdjustViaPercent.Value = adjustPercent;
            }

            numericUpDownOptimalCharsSec.Value = (decimal)Configuration.Settings.General.SubtitleOptimalCharactersPerSeconds;
            numericUpDownMaxCharsSec.Value = (decimal)Configuration.Settings.General.SubtitleMaximumCharactersPerSeconds;
            checkBoxExtendOnly.Checked = Configuration.Settings.Tools.AdjustDurationExtendOnly;

            labelOptimalCharsSec.Text = LanguageSettings.Current.Settings.OptimalCharactersPerSecond;
            labelMaxCharsPerSecond.Text = LanguageSettings.Current.Settings.MaximumCharactersPerSecond;
            labelAddSeconds.Text = LanguageSettings.Current.AdjustDisplayDuration.AddSeconds;
            labelMillisecondsFixed.Text = LanguageSettings.Current.AdjustDisplayDuration.Milliseconds;
            checkBoxExtendOnly.Text = LanguageSettings.Current.AdjustDisplayDuration.ExtendOnly;
            labelAdjustViaPercent.Text = LanguageSettings.Current.AdjustDisplayDuration.SetAsPercent;



            var mode = Configuration.Settings.Tools.BatchConvertFixRtlMode;
            if (mode == RemoveUnicode)
            {
                radioButtonRemoveUnicode.Checked = true;
            }
            else if (mode == ReverseStartEnd)
            {
                radioButtonReverseStartEnd.Checked = true;
            }
            else  // fix with unicode char
            {
                radioButtonAddUnicode.Checked = true;
            }

            groupBoxMergeShortLines.Text = LanguageSettings.Current.MergedShortLines.Title;
            labelMaxCharacters.Text = LanguageSettings.Current.MergedShortLines.MaximumCharacters;
            labelMaxMillisecondsBetweenLines.Text = LanguageSettings.Current.MergedShortLines.MaximumMillisecondsBetween;
            checkBoxOnlyContinuationLines.Text = LanguageSettings.Current.MergedShortLines.OnlyMergeContinuationLines;
            if (Configuration.Settings.General.SubtitleLineMaximumLength > numericUpDownMaxCharacters.Maximum)
            {
                numericUpDownMaxCharacters.Value = numericUpDownMaxCharacters.Maximum;
            }
            else if (Configuration.Settings.General.SubtitleLineMaximumLength < numericUpDownMaxCharacters.Minimum)
            {
                numericUpDownMaxCharacters.Value = numericUpDownMaxCharacters.Minimum;
            }
            else
            {
                numericUpDownMaxCharacters.Value = Configuration.Settings.General.SubtitleLineMaximumLength;
            }

            if (Configuration.Settings.Tools.MergeShortLinesMaxGap >= numericUpDownMaxMillisecondsBetweenLines.Minimum &&
                Configuration.Settings.Tools.MergeShortLinesMaxGap <= numericUpDownMaxMillisecondsBetweenLines.Maximum)
            {
                numericUpDownMaxMillisecondsBetweenLines.Value = Configuration.Settings.Tools.MergeShortLinesMaxGap;
            }

            checkBoxOnlyContinuationLines.Checked = Configuration.Settings.Tools.MergeShortLinesOnlyContinuous;

            inverseSelectionToolStripMenuItem.Text = LanguageSettings.Current.Main.Menu.Edit.InverseSelection;
            toolStripMenuItemSelectAll.Text = LanguageSettings.Current.Main.Menu.ContextMenu.SelectAll;
            UpdateNumberOfFiles();

            var fixItems = new List<FixActionItem>
            {
                new FixActionItem
                {
                    Text = l.RemoveFormatting,
                    Checked = Configuration.Settings.Tools.BatchConvertRemoveFormatting,
                    Action = CommandLineConverter.BatchAction.RemoveFormatting
                },
                new FixActionItem
                {
                    Text = l.RemoveStyleActor,
                    Checked = Configuration.Settings.Tools.BatchConvertRemoveStyle,
                    Action = CommandLineConverter.BatchAction.RemoveStyle,
                    Control = groupBoxRemoveStyle
                },
                new FixActionItem
                {
                    Text = l.RedoCasing,
                    Checked = Configuration.Settings.Tools.BatchConvertFixCasing,
                    Action = CommandLineConverter.BatchAction.RedoCasing,
                    Control = groupBoxChangeCasing
                },
                new FixActionItem
                {
                    Text = l.RemoveTextForHI,
                    Checked = Configuration.Settings.Tools.BatchConvertRemoveTextForHI,
                    Action = CommandLineConverter.BatchAction.RemoveTextForHI,
                    Control = buttonConvertOptionsSettings
                },
                new FixActionItem
                {
                    Text = l.BridgeGaps,
                    Checked = Configuration.Settings.Tools.BatchConvertBridgeGaps,
                    Action = CommandLineConverter.BatchAction.BridgeGaps,
                    Control = buttonConvertOptionsSettings
                },
                new FixActionItem
                {
                    Text = LanguageSettings.Current.FixCommonErrors.Title,
                    Checked = Configuration.Settings.Tools.BatchConvertFixCommonErrors,
                    Action = CommandLineConverter.BatchAction.FixCommonErrors,
                    Control = buttonConvertOptionsSettings
                },
                new FixActionItem
                {
                    Text = LanguageSettings.Current.MultipleReplace.Title,
                    Checked = Configuration.Settings.Tools.BatchConvertMultipleReplace,
                    Action = CommandLineConverter.BatchAction.MultipleReplace,
                    Control = buttonConvertOptionsSettings
                },
                new FixActionItem
                {
                    Text = l.FixRtl,
                    Checked = Configuration.Settings.Tools.BatchConvertFixRtl,
                    Action = CommandLineConverter.BatchAction.FixRtl,
                    Control = groupBoxFixRtl
                },
                new FixActionItem
                {
                    Text = l.SplitLongLines,
                    Checked = Configuration.Settings.Tools.BatchConvertSplitLongLines,
                    Action = CommandLineConverter.BatchAction.SplitLongLines
                },
                new FixActionItem
                {
                    Text = l.AutoBalance,
                    Checked = Configuration.Settings.Tools.BatchConvertAutoBalance,
                    Action = CommandLineConverter.BatchAction.BalanceLines
                },
                new FixActionItem
                {
                    Text = LanguageSettings.Current.SetMinimumDisplayTimeBetweenParagraphs.Title,
                    Checked = Configuration.Settings.Tools.BatchConvertSetMinDisplayTimeBetweenSubtitles,
                    Action = CommandLineConverter.BatchAction.SetMinGap
                },
                new FixActionItem
                {
                    Text = LanguageSettings.Current.MergedShortLines.Title,
                    Checked = Configuration.Settings.Tools.BatchConvertMergeShortLines,
                    Action = CommandLineConverter.BatchAction.MergeShortLines,
                    Control = groupBoxMergeShortLines
                },
                new FixActionItem
                {
                    Text = LanguageSettings.Current.BatchConvert.RemoveLineBreaks,
                    Checked = Configuration.Settings.Tools.BatchConvertRemoveLineBreaks,
                    Action = CommandLineConverter.BatchAction.RemoveLineBreaks
                },
                new FixActionItem
                {
                    Text = LanguageSettings.Current.MergeDoubleLines.Title,
                    Checked = Configuration.Settings.Tools.BatchConvertMergeSameText,
                    Action = CommandLineConverter.BatchAction.MergeSameTexts
                },
                new FixActionItem
                {
                    Text = LanguageSettings.Current.MergeTextWithSameTimeCodes.Title,
                    Checked = Configuration.Settings.Tools.BatchConvertMergeSameTimeCodes,
                    Action = CommandLineConverter.BatchAction.MergeSameTimeCodes
                },
                new FixActionItem
                {
                    Text = LanguageSettings.Current.ChangeFrameRate.Title,
                    Checked = Configuration.Settings.Tools.BatchConvertChangeFrameRate,
                    Action = CommandLineConverter.BatchAction.ChangeFrameRate,
                    Control = groupBoxChangeFrameRate
                },
                new FixActionItem
                {
                    Text = l.OffsetTimeCodes,
                    Checked = Configuration.Settings.Tools.BatchConvertOffsetTimeCodes,
                    Action = CommandLineConverter.BatchAction.OffsetTimeCodes,
                    Control = groupBoxOffsetTimeCodes
                },
                new FixActionItem
                {
                    Text =  LanguageSettings.Current.ChangeSpeedInPercent.TitleShort,
                    Checked = Configuration.Settings.Tools.BatchConvertChangeSpeed,
                    Action = CommandLineConverter.BatchAction.ChangeSpeed,
                    Control = groupBoxSpeed
                },
                new FixActionItem
                {
                    Text =  LanguageSettings.Current.AdjustDisplayDuration.Title,
                    Checked = Configuration.Settings.Tools.BatchConvertAdjustDisplayDuration,
                    Action = CommandLineConverter.BatchAction.AdjustDisplayDuration,
                    Control = groupBoxAdjustDuration
                },
                new FixActionItem
                {
                    Text =  LanguageSettings.Current.ApplyDurationLimits.Title,
                    Checked = Configuration.Settings.Tools.BatchConvertApplyDurationLimits,
                    Action = CommandLineConverter.BatchAction.ApplyDurationLimits
                },
                new FixActionItem
                {
                    Text =  l.DeleteLines,
                    Checked = Configuration.Settings.Tools.BatchConvertDeleteLines,
                    Action = CommandLineConverter.BatchAction.DeleteLines,
                    Control = groupBoxDeleteLines
                }

            };
            foreach (var fixItem in fixItems)
            {
                var listViewItem = new ListViewItem { Tag = fixItem };
                listViewItem.SubItems.Add(fixItem.Text);
                listViewItem.Checked = fixItem.Checked;
                listViewConvertOptions.Items.Add(listViewItem);
            }

            listViewConvertOptions_SelectedIndexChanged(null, null);

            if (Configuration.IsRunningOnWindows)
            {
                buttonSwapFrameRate.Text = "🡙";
            }
            else
            {
                buttonSwapFrameRate.Text = "<->";
                buttonSwapFrameRate.Width = 35;
                buttonSwapFrameRate.Font = new Font(Font.FontFamily, Font.Size);
            }

            SetMkvLanguageMenuItem();
        }

        private void SetMkvLanguageMenuItem()
        {
            var styleName = LanguageSettings.Current.BatchConvert.MkvLanguageStyleThreeLetter;
            if (Configuration.Settings.Tools.BatchConvertMkvLanguageCodeStyle == "2")
            {
                styleName = LanguageSettings.Current.BatchConvert.MkvLanguageStyleTwoLetter;
            }
            else if (Configuration.Settings.Tools.BatchConvertMkvLanguageCodeStyle == "0")
            {
                styleName = LanguageSettings.Current.BatchConvert.MkvLanguageStyleEmpty;
            }

            convertMkvSettingsToolStripMenuItem.Text = string.Format(LanguageSettings.Current.BatchConvert.MkvLanguageInOutputFileNameX, styleName);
        }

        public class FixActionItem
        {
            public string Text { get; set; }
            public bool Checked { get; set; }
            public CommandLineConverter.BatchAction Action { get; set; }
            public Control Control { get; set; }
        }


        private void buttonInputBrowse_Click(object sender, EventArgs e)
        {
            buttonInputBrowse.Enabled = false;
            openFileDialog1.Title = LanguageSettings.Current.General.OpenSubtitle;
            openFileDialog1.FileName = string.Empty;
            openFileDialog1.Filter = UiUtil.SubtitleExtensionFilter.Value;
            openFileDialog1.Multiselect = true;
            if (openFileDialog1.ShowDialog(this) == DialogResult.OK)
            {
                try
                {
                    Cursor = Cursors.WaitCursor;
                    labelStatus.Text = LanguageSettings.Current.General.PleaseWait;
                    listViewInputFiles.BeginUpdate();
                    foreach (string fileName in openFileDialog1.FileNames)
                    {
                        AddInputFile(fileName);
                        Application.DoEvents();
                    }
                }
                finally
                {
                    listViewInputFiles.EndUpdate();
                    Cursor = Cursors.Default;
                    labelStatus.Text = string.Empty;
                }
            }

            buttonInputBrowse.Enabled = true;
        }

        private void AddInputFile(string fileName)
        {
            if (comboBoxFilter.SelectedIndex == 4 && textBoxFilter.Text.Length > 0 && !fileName.Contains(textBoxFilter.Text, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }
            try
            {
                foreach (ListViewItem lvi in listViewInputFiles.Items)
                {
                    if (lvi.Text.Equals(fileName, StringComparison.OrdinalIgnoreCase))
                    {
                        return;
                    }
                }

                var fi = new FileInfo(fileName);
                var ext = fi.Extension.ToLowerInvariant();
                var item = new ListViewItem(fileName);
                item.SubItems.Add(Utilities.FormatBytesToDisplayFileSize(fi.Length));
                var isMkv = false;
                var mkvPgs = new List<string>();
                var mkvVobSub = new List<string>();
                var mkvSrt = new List<string>();
                var mkvSsa = new List<string>();
                var mkvAss = new List<string>();
                int mkvCount = 0;
                var isTs = false;

                SubtitleFormat format = null;
                var sub = new Subtitle();
                if (fi.Length < ConvertMaxFileSize)
                {
                    if (!FileUtil.IsBluRaySup(fileName) && !FileUtil.IsVobSub(fileName) &&
                        !((ext == ".mkv" || ext == ".mks") && FileUtil.IsMatroskaFile(fileName)))
                    {
                        format = sub.LoadSubtitle(fileName, out _, null);

                        if (format == null)
                        {
                            foreach (var f in SubtitleFormat.GetBinaryFormats(true))
                            {
                                if (f.IsMine(null, fileName))
                                {
                                    f.LoadSubtitle(sub, null, fileName);
                                    format = f;
                                    break;
                                }
                            }
                        }

                        if (format == null)
                        {
                            var encoding = LanguageAutoDetect.GetEncodingFromFile(fileName);
                            var lines = FileUtil.ReadAllTextShared(fileName, encoding).SplitToLines();
                            foreach (var f in SubtitleFormat.GetTextOtherFormats())
                            {
                                if (f.IsMine(lines, fileName))
                                {
                                    f.LoadSubtitle(sub, lines, fileName);
                                    format = f;
                                    break;
                                }
                            }
                        }
                    }
                }

                if (format == null)
                {
                    if (FileUtil.IsBluRaySup(fileName))
                    {
                        item.SubItems.Add("Blu-ray");
                    }
                    else if (FileUtil.IsVobSub(fileName))
                    {
                        item.SubItems.Add("VobSub");
                    }
                    else if (ext == ".mkv" || ext == ".mks")
                    {
                        isMkv = true;
                        using (var matroska = new MatroskaFile(fileName))
                        {
                            if (matroska.IsValid)
                            {
                                foreach (var track in matroska.GetTracks(true))
                                {
                                    if (track.CodecId.Equals("S_VOBSUB", StringComparison.OrdinalIgnoreCase))
                                    {
                                        mkvVobSub.Add(MakeMkvTrackInfoString(track));
                                    }
                                    else if (track.CodecId.Equals("S_HDMV/PGS", StringComparison.OrdinalIgnoreCase))
                                    {
                                        mkvPgs.Add(MakeMkvTrackInfoString(track));
                                    }
                                    else if (track.CodecId.Equals("S_TEXT/UTF8", StringComparison.OrdinalIgnoreCase))
                                    {
                                        mkvSrt.Add(MakeMkvTrackInfoString(track));
                                    }
                                    else if (track.CodecId.Equals("S_TEXT/SSA", StringComparison.OrdinalIgnoreCase))
                                    {
                                        mkvSsa.Add(MakeMkvTrackInfoString(track));
                                    }
                                    else if (track.CodecId.Equals("S_TEXT/ASS", StringComparison.OrdinalIgnoreCase))
                                    {
                                        mkvAss.Add(MakeMkvTrackInfoString(track));
                                    }
                                }
                            }
                        }
                        if (mkvVobSub.Count + mkvPgs.Count + mkvSrt.Count + mkvSsa.Count + mkvAss.Count <= 0)
                        {
                            item.SubItems.Add(LanguageSettings.Current.UnknownSubtitle.Title);
                        }
                    }
                    else if ((ext == ".ts" || ext == ".m2ts" || ext == ".mts" || ext == ".mpg" || ext == ".mpeg") &&
                             (FileUtil.IsTransportStream(fileName) || FileUtil.IsM2TransportStream(fileName)))
                    {
                        isTs = true;
                    }
                    else
                    {
                        item.SubItems.Add(LanguageSettings.Current.UnknownSubtitle.Title);
                    }
                }
                else
                {
                    item.SubItems.Add(format.Name);
                }
                item.SubItems.Add("-");

                if (isMkv)
                {
                    if (mkvCount > 0)
                    {
                        listViewInputFiles.Items.Add(item);
                    }

                    bool mkvLangCodeFilterActive = comboBoxFilter.SelectedIndex == 5 && textBoxFilter.Text.Length > 0;
                    var mkvSubFormats = new Dictionary<string, List<string>>
                    {
                        { "PGS", mkvPgs },
                        { "VobSub", mkvVobSub },
                        { "SRT", mkvSrt },
                        { "SSA", mkvSsa },
                        { "ASS", mkvAss }
                    };

                    foreach (var mkvSubFormat in mkvSubFormats)
                    {
                        foreach (var lang in mkvSubFormat.Value)
                        {
                            if (mkvLangCodeFilterActive && !lang.Contains(textBoxFilter.Text, StringComparison.OrdinalIgnoreCase))
                            {
                                continue;
                            }

                            item = new ListViewItem(fileName);
                            item.SubItems.Add(Utilities.FormatBytesToDisplayFileSize(fi.Length));
                            listViewInputFiles.Items.Add(item);
                            item.SubItems.Add($"Matroska/{mkvSubFormat.Key} - {lang}");
                            item.SubItems.Add("-");
                        }
                    }
                }
                else if (isTs)
                {
                    item = new ListViewItem(fileName);
                    item.SubItems.Add(Utilities.FormatBytesToDisplayFileSize(fi.Length));
                    listViewInputFiles.Items.Add(item);
                    item.SubItems.Add("Transport Stream");
                    item.SubItems.Add("-");
                }
                else
                {
                    listViewInputFiles.Items.Add(item);
                }

                if (isTs)
                {
                    buttonTransportStreamSettings.Visible = true;
                }
            }
            catch
            {
                // ignored
            }

            UpdateNumberOfFiles();
        }

        private static string MakeMkvTrackInfoString(MatroskaTrackInfo track)
        {
            return (track.Language ?? "undefined") + (track.IsForced ? " (forced)" : string.Empty) + " #" + track.TrackNumber;
        }

        private string GetMkvLanguage(string languageCode)
        {
            if (Configuration.Settings.Tools.BatchConvertMkvLanguageCodeStyle == "0")
            {
                return string.Empty;
            }

            if (string.IsNullOrEmpty(languageCode))
            {
                return "undefined.";
            }

            if (Configuration.Settings.Tools.BatchConvertMkvLanguageCodeStyle == "2" &&
                !string.IsNullOrEmpty(Iso639Dash2LanguageCode.GetTwoLetterCodeFromThreeLetterCode(languageCode)))
            {
                return Iso639Dash2LanguageCode.GetTwoLetterCodeFromThreeLetterCode(languageCode) + ".";
            }

            return string.IsNullOrEmpty(languageCode) ? string.Empty : languageCode.TrimEnd('.') + ".";
        }

        private void listViewInputFiles_DragEnter(object sender, DragEventArgs e)
        {
            if (_converting || _searching)
            {
                e.Effect = DragDropEffects.None;
                return;
            }

            if (e.Data.GetDataPresent(DataFormats.FileDrop, false))
            {
                e.Effect = DragDropEffects.All;
            }
        }

        private void listViewInputFiles_DragDrop(object sender, DragEventArgs e)
        {
            if (_converting || _searching)
            {
                return;
            }

            try
            {
                var fileNames = (string[])e.Data.GetData(DataFormats.FileDrop);
                labelStatus.Text = LanguageSettings.Current.General.PleaseWait;
                listViewInputFiles.BeginUpdate();
                foreach (string fileName in fileNames)
                {
                    if (FileUtil.IsDirectory(fileName))
                    {
                        SearchFolder(fileName);
                    }
                    else
                    {
                        AddInputFile(fileName);
                    }
                }
            }
            finally
            {
                labelStatus.Text = string.Empty;
                listViewInputFiles.EndUpdate();
            }
        }

        private void radioButtonSpeedCustom_CheckedChanged(object sender, EventArgs e)
        {
            numericUpDownPercent.Enabled = true;
        }

        private void radioButtonSpeedFromDropFrame_CheckedChanged(object sender, EventArgs e)
        {
            numericUpDownPercent.Value = Convert.ToDecimal(099.98887);
            numericUpDownPercent.Enabled = false;
        }

        private void radioButtonToDropFrame_CheckedChanged(object sender, EventArgs e)
        {
            numericUpDownPercent.Value = Convert.ToDecimal(100.1001001);
            numericUpDownPercent.Enabled = false;
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private TextEncoding GetCurrentEncoding(string fileName)
        {
            bool useEncodingFromFile = comboBoxEncoding.SelectedIndex == comboBoxEncoding.Items.Count - 1;
            if (useEncodingFromFile)
            {
                if (string.IsNullOrEmpty(fileName) || !File.Exists(fileName))
                {
                    if (Configuration.Settings.General.DefaultEncoding == TextEncoding.Utf8WithoutBom)
                    {
                        return new TextEncoding(Encoding.UTF8, TextEncoding.Utf8WithoutBom);
                    }
                    return new TextEncoding(Encoding.UTF8, TextEncoding.Utf8WithBom);
                }
                var enc = LanguageAutoDetect.GetEncodingFromFile(fileName);
                return new TextEncoding(enc, null);
            }

            return UiUtil.GetTextEncodingComboBoxCurrentEncoding(comboBoxEncoding);
        }

        private SubtitleFormat GetCurrentSubtitleFormat()
        {
            var format = Utilities.GetSubtitleFormatByFriendlyName(comboBoxSubtitleFormats.SelectedItem.ToString());
            return format ?? new SubRip();
        }

        private void buttonConvert_Click(object sender, EventArgs e)
        {
            if (buttonConvert.Text == LanguageSettings.Current.General.Cancel)
            {
                _abort = true;
                return;
            }

            UpdateChangeCasingSettings();
            UpdateRtlSettings();
            UpdateActionEnabledCache();
            if (listViewInputFiles.Items.Count == 0)
            {
                MessageBox.Show(LanguageSettings.Current.BatchConvert.NothingToConvert);
                return;
            }
            if (!checkBoxOverwrite.Checked)
            {
                if (textBoxOutputFolder.Text.Length < 2)
                {
                    MessageBox.Show(LanguageSettings.Current.BatchConvert.PleaseChooseOutputFolder);
                    return;
                }
                if (!Directory.Exists(textBoxOutputFolder.Text))
                {
                    try
                    {
                        Directory.CreateDirectory(textBoxOutputFolder.Text);
                    }
                    catch (Exception exception)
                    {
                        MessageBox.Show(exception.Message);
                        return;
                    }
                }
            }
            _converting = true;
            progressBar1.Style = ProgressBarStyle.Blocks;
            progressBar1.Maximum = listViewInputFiles.Items.Count;
            progressBar1.Value = 0;
            progressBar1.Visible = progressBar1.Maximum > 2;
            string toFormat = comboBoxSubtitleFormats.Text;
            SetControlState(false);

            _count = 0;
            _converted = 0;
            _errors = 0;
            _abort = false;

            var worker1 = SpawnWorker();
            var worker2 = SpawnWorker();
            var worker3 = SpawnWorker();

            listViewInputFiles.BeginUpdate();
            foreach (ListViewItem item in listViewInputFiles.Items)
            {
                item.SubItems[3].Text = "-";
            }

            listViewInputFiles.EndUpdate();
            var mkvFileNames = new List<string>();
            Refresh();
            int index = 0;
            while (index < listViewInputFiles.Items.Count && !_abort)
            {
                ListViewItem item = listViewInputFiles.Items[index];
                string fileName = item.Text;
                try
                {
                    var binaryParagraphs = new List<IBinaryParagraph>();
                    SubtitleFormat format = null;
                    var sub = new Subtitle();
                    var fi = new FileInfo(fileName);
                    if (fi.Length < ConvertMaxFileSize && !FileUtil.IsBluRaySup(fileName) && !FileUtil.IsVobSub(fileName))
                    {
                        format = sub.LoadSubtitle(fileName, out _, null);

                        if (format == null)
                        {
                            foreach (var f in SubtitleFormat.GetBinaryFormats(true))
                            {
                                if (f.IsMine(null, fileName))
                                {
                                    f.LoadSubtitle(sub, null, fileName);
                                    format = f;
                                    break;
                                }
                            }
                        }

                        if (format == null)
                        {
                            var encoding = LanguageAutoDetect.GetEncodingFromFile(fileName);
                            var lines = FileUtil.ReadAllTextShared(fileName, encoding).SplitToLines();
                            foreach (var f in SubtitleFormat.GetTextOtherFormats())
                            {
                                if (f.IsMine(lines, fileName))
                                {
                                    f.LoadSubtitle(sub, lines, fileName);
                                    format = f;
                                    break;
                                }
                            }
                        }

                        if (format == null)
                        {
                            var enc = LanguageAutoDetect.GetEncodingFromFile(fileName);
                            var s = File.ReadAllText(fileName, enc);

                            // check for RTF file
                            if (fileName.EndsWith(".rtf", StringComparison.OrdinalIgnoreCase) && s.TrimStart().StartsWith("{\\rtf", StringComparison.Ordinal))
                            {
                                using (var rtb = new RichTextBox { Rtf = s })
                                {
                                    s = rtb.Text;
                                }
                            }
                            var unknownFormatImporter = new UnknownFormatImporter { UseFrames = true };
                            var genericParseSubtitle = unknownFormatImporter.AutoGuessImport(s.SplitToLines());
                            if (genericParseSubtitle.Paragraphs.Count > 1)
                            {
                                sub = genericParseSubtitle;
                                format = new SubRip();
                            }
                        }

                        if (format != null && format.GetType() == typeof(MicroDvd))
                        {
                            if (sub != null && sub.Paragraphs.Count > 0 && sub.Paragraphs[0].Duration.TotalMilliseconds < 1001)
                            {
                                if (sub.Paragraphs[0].Text.StartsWith("29.", StringComparison.Ordinal) || sub.Paragraphs[0].Text.StartsWith("23.", StringComparison.Ordinal) ||
                                sub.Paragraphs[0].Text.StartsWith("29,", StringComparison.Ordinal) || sub.Paragraphs[0].Text.StartsWith("23,", StringComparison.Ordinal) ||
                                sub.Paragraphs[0].Text == "24" || sub.Paragraphs[0].Text == "25" ||
                                sub.Paragraphs[0].Text == "30" || sub.Paragraphs[0].Text == "60")
                                {
                                    sub.Paragraphs.RemoveAt(0);
                                }
                            }
                        }
                    }
                    var bluRaySubtitles = new List<BluRaySupParser.PcsData>();
                    bool isVobSub = false;
                    bool isMatroska = false;
                    bool isTs = false;
                    if (format == null && fileName.EndsWith(".sup", StringComparison.OrdinalIgnoreCase) && FileUtil.IsBluRaySup(fileName))
                    {
                        var log = new StringBuilder();
                        bluRaySubtitles = BluRaySupParser.ParseBluRaySup(fileName, log);
                    }
                    else if (format == null && fileName.EndsWith(".sub", StringComparison.OrdinalIgnoreCase) && FileUtil.IsVobSub(fileName))
                    {
                        isVobSub = true;
                    }
                    else if (format == null && (fileName.EndsWith(".mkv", StringComparison.OrdinalIgnoreCase) || fileName.EndsWith(".mks", StringComparison.OrdinalIgnoreCase)) && item.SubItems[2].Text.StartsWith("Matroska", StringComparison.Ordinal))
                    {
                        isMatroska = true;
                    }
                    else if (format == null &&
                             (fileName.EndsWith(".ts", StringComparison.OrdinalIgnoreCase) ||
                              fileName.EndsWith(".m2ts", StringComparison.OrdinalIgnoreCase) ||
                              fileName.EndsWith(".mts", StringComparison.OrdinalIgnoreCase) ||
                              fileName.EndsWith(".mpg", StringComparison.OrdinalIgnoreCase) ||
                              fileName.EndsWith(".mpeg", StringComparison.OrdinalIgnoreCase)) &&
                             item.SubItems[2].Text.StartsWith("Transport Stream", StringComparison.Ordinal))
                    {
                        isTs = true;
                    }
                    if (format == null && bluRaySubtitles.Count == 0 && !isVobSub && !isMatroska && !isTs)
                    {
                        IncrementAndShowProgress();
                    }
                    else
                    {
                        if (isMatroska)
                        {
                            using (var matroska = new MatroskaFile(fileName))
                            {
                                if (matroska.IsValid)
                                {
                                    var trackId = item.SubItems[2].Text;
                                    if (trackId.Contains("#"))
                                    {
                                        trackId = trackId.Remove(0, trackId.IndexOf("#", StringComparison.Ordinal) + 1);
                                    }

                                    foreach (var track in matroska.GetTracks(true))
                                    {
                                        if (track.CodecId.Equals("S_VOBSUB", StringComparison.OrdinalIgnoreCase))
                                        {
                                            if (trackId == track.TrackNumber.ToString(CultureInfo.InvariantCulture))
                                            {
                                                var vobSubs = LoadVobSubFromMatroska(track, matroska, out var idx);
                                                if (vobSubs.Count > 0)
                                                {
                                                    item.SubItems[3].Text = LanguageSettings.Current.BatchConvert.Ocr;
                                                    using (var vobSubOcr = new VobSubOcr())
                                                    {
                                                        vobSubOcr.ProgressCallback = progress =>
                                                        {
                                                            item.SubItems[3].Text = LanguageSettings.Current.BatchConvert.Ocr + "  " + progress;
                                                            listViewInputFiles.Refresh();
                                                        };
                                                        vobSubOcr.FileName = Path.GetFileName(fileName);
                                                        vobSubOcr.InitializeBatch(vobSubs, idx.Palette, Configuration.Settings.VobSubOcr, fileName, false, track.Language, null);
                                                        sub = vobSubOcr.SubtitleFromOcr;
                                                    }
                                                }

                                                fileName = fileName.Substring(0, fileName.LastIndexOf('.')) + "." + GetMkvLanguage(track.Language).Replace("undefined.", string.Empty) + "mkv";
                                                if (mkvFileNames.Contains(fileName))
                                                {
                                                    fileName = fileName.Substring(0, fileName.LastIndexOf('.')) + ".#" + trackId + "." + GetMkvLanguage(track.Language) + "mkv";
                                                }
                                                mkvFileNames.Add(fileName);

                                                break;
                                            }
                                        }
                                        else if (track.CodecId.Equals("S_HDMV/PGS", StringComparison.OrdinalIgnoreCase))
                                        {
                                            if (trackId == track.TrackNumber.ToString(CultureInfo.InvariantCulture))
                                            {
                                                bluRaySubtitles = LoadBluRaySupFromMatroska(track, matroska, Handle);

                                                fileName = fileName.Substring(0, fileName.LastIndexOf('.')) + "." + GetMkvLanguage(track.Language).Replace("undefined.", string.Empty) + "mkv";
                                                if (mkvFileNames.Contains(fileName))
                                                {
                                                    fileName = fileName.Substring(0, fileName.LastIndexOf('.')) + ".#" + trackId + "." + GetMkvLanguage(track.Language) + "mkv";
                                                }
                                                mkvFileNames.Add(fileName);

                                                if ((toFormat == BdnXmlSubtitle || toFormat == BluRaySubtitle ||
                                                     toFormat == VobSubSubtitle || toFormat == DostImageSubtitle) &&
                                                    AllowImageToImage())
                                                {
                                                    foreach (var b in bluRaySubtitles)
                                                    {
                                                        sub.Paragraphs.Add(new Paragraph(b.StartTimeCode, b.EndTimeCode, string.Empty));
                                                    }
                                                    if (!_bdLookup.ContainsKey(fileName))
                                                    {
                                                        _bdLookup.Add(fileName, bluRaySubtitles);
                                                    }
                                                }
                                                else
                                                {
                                                    if (bluRaySubtitles.Count > 0)
                                                    {
                                                        item.SubItems[3].Text = LanguageSettings.Current.BatchConvert.Ocr;
                                                        using (var vobSubOcr = new VobSubOcr())
                                                        {
                                                            vobSubOcr.ProgressCallback = progress =>
                                                            {
                                                                item.SubItems[3].Text = LanguageSettings.Current.BatchConvert.Ocr + "  " + progress;
                                                                listViewInputFiles.Refresh();
                                                            };
                                                            vobSubOcr.FileName = Path.GetFileName(fileName);
                                                            vobSubOcr.InitializeBatch(bluRaySubtitles, Configuration.Settings.VobSubOcr, fileName, false, track.Language);
                                                            sub = vobSubOcr.SubtitleFromOcr;
                                                        }
                                                    }
                                                }
                                                break;
                                            }
                                        }
                                        else if (track.CodecId.Equals("S_TEXT/UTF8", StringComparison.OrdinalIgnoreCase) || track.CodecId.Equals("S_TEXT/SSA", StringComparison.OrdinalIgnoreCase) || track.CodecId.Equals("S_TEXT/ASS", StringComparison.OrdinalIgnoreCase))
                                        {
                                            if (trackId == track.TrackNumber.ToString(CultureInfo.InvariantCulture))
                                            {
                                                var mkvSub = matroska.GetSubtitle(track.TrackNumber, null);
                                                Utilities.LoadMatroskaTextSubtitle(track, matroska, mkvSub, sub);
                                                fileName = fileName.Substring(0, fileName.LastIndexOf('.')) + "." + GetMkvLanguage(track.Language).Replace("undefined.", string.Empty) + "mkv";
                                                if (mkvFileNames.Contains(fileName))
                                                {
                                                    fileName = fileName.Substring(0, fileName.LastIndexOf('.')) + ".#" + trackId + "." + GetMkvLanguage(track.Language) + "mkv";
                                                }
                                                mkvFileNames.Add(fileName);
                                                break;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        else if (bluRaySubtitles.Count > 0)
                        {
                            if ((toFormat == BdnXmlSubtitle || toFormat == BluRaySubtitle ||
                                toFormat == VobSubSubtitle || toFormat == DostImageSubtitle) &&
                                AllowImageToImage())
                            {
                                foreach (var b in bluRaySubtitles)
                                {
                                    sub.Paragraphs.Add(new Paragraph(b.StartTimeCode, b.EndTimeCode, string.Empty));
                                }
                            }
                            else
                            {
                                item.SubItems[3].Text = LanguageSettings.Current.BatchConvert.Ocr;
                                using (var vobSubOcr = new VobSubOcr())
                                {
                                    vobSubOcr.ProgressCallback = progress =>
                                    {
                                        item.SubItems[3].Text = LanguageSettings.Current.BatchConvert.Ocr + "  " + progress;
                                        listViewInputFiles.Refresh();
                                    };
                                    vobSubOcr.FileName = Path.GetFileName(fileName);
                                    vobSubOcr.InitializeBatch(bluRaySubtitles, Configuration.Settings.VobSubOcr, fileName, false);
                                    sub = vobSubOcr.SubtitleFromOcr;
                                }
                            }
                        }
                        else if (isVobSub)
                        {
                            item.SubItems[3].Text = LanguageSettings.Current.BatchConvert.Ocr;
                            using (var vobSubOcr = new VobSubOcr())
                            {
                                vobSubOcr.ProgressCallback = progress =>
                                {
                                    item.SubItems[3].Text = LanguageSettings.Current.BatchConvert.Ocr + "  " + progress;
                                    listViewInputFiles.Refresh();
                                };
                                vobSubOcr.InitializeBatch(fileName, Configuration.Settings.VobSubOcr, false, null);
                                sub = vobSubOcr.SubtitleFromOcr;
                            }
                        }
                        else if (isTs)
                        {
                            int tsConvertedCount = 0;
                            var programMapTableParser = new ProgramMapTableParser();
                            programMapTableParser.Parse(fileName); // get languages
                            var tsParser = new TransportStreamParser();
                            tsParser.Parse(fileName, (position, total) =>
                            {
                                var percent = (int)Math.Round(position * 100.0 / total);
                                item.SubItems[3].Text = $"Read: {percent}%";
                                listViewInputFiles.Refresh();
                            });

                            var outputFolder = textBoxOutputFolder.Text;
                            var overwrite = checkBoxOverwrite.Checked;
                            if (radioButtonSaveInSourceFolder.Checked)
                            {
                                outputFolder = Path.GetDirectoryName(fileName);
                            }

                            var targetEncoding = GetCurrentEncoding(fileName);

                            var targetFrameRate = 0.0;
                            if (double.TryParse(comboBoxFrameRateTo.Text.Replace(',', '.').Replace(CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator, "."), NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var toFrameRate))
                            {
                                targetFrameRate = toFrameRate;
                            }

                            // images
                            if (!Configuration.Settings.Tools.BatchConvertTsOnlyTeletext)
                            {
                                foreach (int id in tsParser.SubtitlePacketIds)
                                {
                                    void ProgressCallback(string progress)
                                    {
                                        item.SubItems[3].Text = progress;
                                        listViewInputFiles.Refresh();
                                    }

                                    if (BluRaySubtitle.RemoveChar(' ').Equals(toFormat.RemoveChar(' '), StringComparison.OrdinalIgnoreCase))
                                    {
                                        TsToBluRaySup.WriteTrack(fileName, outputFolder, overwrite, 0, null, ProgressCallback, null, programMapTableParser, id, tsParser);
                                        tsConvertedCount++;
                                    }
                                    else if (BdnXmlSubtitle.RemoveChar(' ').Equals(toFormat.RemoveChar(' '), StringComparison.OrdinalIgnoreCase))
                                    {
                                        TsToBdnXml.WriteTrack(fileName, outputFolder, overwrite, null, ProgressCallback, null, programMapTableParser, id, tsParser);
                                        tsConvertedCount++;
                                    }
                                    else
                                    {
                                        var tsBinaryParagraphs = new List<IBinaryParagraph>();
                                        var subtitle = new Subtitle();
                                        foreach (var transportStreamSubtitle in tsParser.GetDvbSubtitles(id))
                                        {
                                            tsBinaryParagraphs.Add(transportStreamSubtitle);
                                            subtitle.Paragraphs.Add(new Paragraph(string.Empty, transportStreamSubtitle.StartMilliseconds, transportStreamSubtitle.EndMilliseconds));
                                        }

                                        using (var vobSubOcr = new VobSubOcr())
                                        {
                                            vobSubOcr.ProgressCallback = progress =>
                                            {
                                                item.SubItems[3].Text = $"OCR: {progress}";
                                                listViewInputFiles.Refresh();
                                            };
                                            var language = programMapTableParser.GetSubtitleLanguage(id);
                                            language = string.IsNullOrEmpty(language) ? null : language;
                                            vobSubOcr.FileName = Path.GetFileName(fileName);
                                            vobSubOcr.InitializeBatch(tsBinaryParagraphs, Configuration.Settings.VobSubOcr, fileName, false, language, null);
                                            subtitle = vobSubOcr.SubtitleFromOcr;
                                        }

                                        // apply fixes step 1
                                        subtitle = ApplyFixesStep1(subtitle, null);

                                        // apply fixes step 2
                                        var parameter = new ThreadDoWorkParameter(
                                            IsActionEnabled(CommandLineConverter.BatchAction.FixCommonErrors),
                                            IsActionEnabled(CommandLineConverter.BatchAction.MultipleReplace),
                                            IsActionEnabled(CommandLineConverter.BatchAction.FixRtl),
                                            IsActionEnabled(CommandLineConverter.BatchAction.SplitLongLines),
                                            IsActionEnabled(CommandLineConverter.BatchAction.BalanceLines),
                                            IsActionEnabled(CommandLineConverter.BatchAction.SetMinGap),
                                            item,
                                            subtitle,
                                            GetCurrentSubtitleFormat(),
                                            GetCurrentEncoding(fileName),
                                            Configuration.Settings.Tools.BatchConvertLanguage,
                                            fileName,
                                            toFormat,
                                            format,
                                            binaryParagraphs);
                                        ApplyFixesStep2(parameter, Configuration.Settings.Tools.BatchConvertFixRtlMode);

                                        var preExt = TsToBluRaySup.GetFileNameEnding(programMapTableParser, id);
                                        int dummy = 0;
                                        CommandLineConverter.BatchConvertSave(toFormat, TimeSpan.Zero, targetEncoding, outputFolder, 0, ref dummy, ref dummy, SubtitleFormat.AllSubtitleFormats.ToList(), fileName, parameter.Subtitle, new SubRip(), null, overwrite, 0, targetFrameRate, null, new List<CommandLineConverter.BatchAction>(), null, true, null, null, null, preExt);
                                        tsConvertedCount++;
                                    }
                                }
                            }

                            // teletext
                            foreach (var program in tsParser.TeletextSubtitlesLookup)
                            {
                                foreach (var kvp in program.Value)
                                {
                                    var subtitle = new Subtitle(kvp.Value);
                                    var preExt = TsToBluRaySup.GetFileNameEnding(programMapTableParser, kvp.Key);
                                    int dummy = 0;

                                    // apply fixes step 1
                                    subtitle = ApplyFixesStep1(subtitle, null);

                                    // apply fixes step 2
                                    var parameter = new ThreadDoWorkParameter(
                                        IsActionEnabled(CommandLineConverter.BatchAction.FixCommonErrors),
                                        IsActionEnabled(CommandLineConverter.BatchAction.MultipleReplace),
                                        IsActionEnabled(CommandLineConverter.BatchAction.FixRtl),
                                        IsActionEnabled(CommandLineConverter.BatchAction.SplitLongLines),
                                        IsActionEnabled(CommandLineConverter.BatchAction.BalanceLines),
                                        IsActionEnabled(CommandLineConverter.BatchAction.SetMinGap),
                                        item,
                                        subtitle,
                                        GetCurrentSubtitleFormat(),
                                        GetCurrentEncoding(fileName),
                                        Configuration.Settings.Tools.BatchConvertLanguage,
                                        fileName,
                                        toFormat,
                                        format,
                                        binaryParagraphs);
                                    ApplyFixesStep2(parameter, Configuration.Settings.Tools.BatchConvertFixRtlMode);

                                    CommandLineConverter.BatchConvertSave(toFormat, TimeSpan.Zero, targetEncoding, outputFolder, 0, ref dummy, ref dummy, SubtitleFormat.AllSubtitleFormats.ToList(), fileName, parameter.Subtitle, new SubRip(), null, overwrite, 0, targetFrameRate, null, new List<CommandLineConverter.BatchAction>(), null, true, null, null, null, preExt);
                                    tsConvertedCount++;
                                }
                            }

                            if (tsConvertedCount > 0)
                            {
                                item.SubItems[3].Text = LanguageSettings.Current.BatchConvert.Converted;
                            }
                        }

                        if (comboBoxSubtitleFormats.Text == AdvancedSubStationAlpha.NameOfFormat && _assStyle != null)
                        {
                            if (!string.IsNullOrWhiteSpace(_assStyle) && !checkBoxUseStyleFromSource.Checked)
                            {
                                sub.Header = _assStyle;
                            }
                        }
                        else if (comboBoxSubtitleFormats.Text == SubStationAlpha.NameOfFormat && _ssaStyle != null)
                        {
                            if (!string.IsNullOrWhiteSpace(_ssaStyle) && !checkBoxUseStyleFromSource.Checked)
                            {
                                sub.Header = _ssaStyle;
                            }
                        }

                        bool skip = CheckSkipFilter(fileName, format, sub);
                        if (skip)
                        {
                            item.SubItems[3].Text = LanguageSettings.Current.BatchConvert.FilterSkipped;
                        }
                        else
                        {
                            sub = ApplyFixesStep1(sub, bluRaySubtitles);

                            while (worker1.IsBusy && worker2.IsBusy && worker3.IsBusy)
                            {
                                Application.DoEvents();
                                System.Threading.Thread.Sleep(100);
                            }

                            var parameter = new ThreadDoWorkParameter(
                                IsActionEnabled(CommandLineConverter.BatchAction.FixCommonErrors),
                                IsActionEnabled(CommandLineConverter.BatchAction.MultipleReplace),
                                IsActionEnabled(CommandLineConverter.BatchAction.FixRtl),
                                IsActionEnabled(CommandLineConverter.BatchAction.SplitLongLines),
                                IsActionEnabled(CommandLineConverter.BatchAction.BalanceLines),
                                IsActionEnabled(CommandLineConverter.BatchAction.SetMinGap),
                                item,
                                sub,
                                GetCurrentSubtitleFormat(),
                                GetCurrentEncoding(fileName),
                                Configuration.Settings.Tools.BatchConvertLanguage,
                                fileName,
                                toFormat,
                                format,
                                binaryParagraphs);

                            if (!worker1.IsBusy)
                            {
                                worker1.RunWorkerAsync(parameter);
                            }
                            else if (!worker2.IsBusy)
                            {
                                worker2.RunWorkerAsync(parameter);
                            }
                            else if (!worker3.IsBusy)
                            {
                                worker3.RunWorkerAsync(parameter);
                            }
                        }
                    }
                }
                catch (Exception exception)
                {
                    System.Diagnostics.Debug.WriteLine(exception);
                    IncrementAndShowProgress();
                }
                index++;
            }
            while (worker1.IsBusy || worker2.IsBusy || worker3.IsBusy)
            {
                try
                {
                    Application.DoEvents();
                }
                catch
                {
                    // ignored
                }
                System.Threading.Thread.Sleep(100);
            }

            // dispose workers
            worker1.Dispose();
            worker2.Dispose();
            worker3.Dispose();

            _converting = false;
            labelStatus.Text = string.Empty;
            progressBar1.Visible = false;
            TaskbarList.SetProgressState(Handle, TaskbarButtonProgressFlags.NoProgress);
            SetControlState(true);
            _bdLookup = new Dictionary<string, List<BluRaySupParser.PcsData>>();
        }

        private Subtitle ApplyFixesStep1(Subtitle sub, List<BluRaySupParser.PcsData> bluRaySubtitles)
        {
            if (IsActionEnabled(CommandLineConverter.BatchAction.BridgeGaps))
            {
                Core.Forms.DurationsBridgeGaps.BridgeGaps(sub, _bridgeGaps.MinMsBetweenLines, !_bridgeGaps.PreviousSubtitleTakesAllTime, Configuration.Settings.Tools.BridgeGapMilliseconds, null, null, false);
            }

            if (IsActionEnabled(CommandLineConverter.BatchAction.ApplyDurationLimits))
            {
                var fixDurationLimits = new FixDurationLimits(Configuration.Settings.General.SubtitleMinimumDisplayMilliseconds, Configuration.Settings.General.SubtitleMaximumDisplayMilliseconds);
                sub = fixDurationLimits.Fix(sub);
            }

            if (IsActionEnabled(CommandLineConverter.BatchAction.DeleteLines))
            {
                DeleteLines(sub);
            }

            if (IsActionEnabled(CommandLineConverter.BatchAction.RemoveStyle) && !string.IsNullOrEmpty(textBoxRemoveStyle.Text))
            {
                sub.Paragraphs.RemoveAll(p => p.Extra == textBoxRemoveStyle.Text || p.Style == textBoxRemoveStyle.Text);
                sub.Renumber();
            }

            if (IsActionEnabled(CommandLineConverter.BatchAction.AdjustDisplayDuration))
            {
                var adjustmentType = comboBoxAdjustDurationVia.Text;
                if (adjustmentType == LanguageSettings.Current.AdjustDisplayDuration.Percent)
                {
                    sub.AdjustDisplayTimeUsingPercent((double)numericUpDownAdjustViaPercent.Value, null);
                }
                else if (adjustmentType == LanguageSettings.Current.AdjustDisplayDuration.Recalculate)
                {
                    sub.RecalculateDisplayTimes((double)numericUpDownMaxCharsSec.Value, null, (double)numericUpDownOptimalCharsSec.Value, checkBoxExtendOnly.Checked);
                }
                else if (adjustmentType == LanguageSettings.Current.AdjustDisplayDuration.Fixed)
                {
                    sub.SetFixedDuration(null, (double)numericUpDownFixedMilliseconds.Value);
                }
                else
                {
                    sub.AdjustDisplayTimeUsingSeconds((double)numericUpDownSeconds.Value, null);
                }
            }

            var prev = sub.GetParagraphOrDefault(0);
            var first = true;
            foreach (var p in sub.Paragraphs)
            {
                if (IsActionEnabled(CommandLineConverter.BatchAction.RemoveTextForHI))
                {
                    _removeTextForHearingImpaired.Settings = _removeTextForHiSettings;
                    p.Text = _removeTextForHearingImpaired.RemoveTextFromHearImpaired(p.Text, sub, sub.Paragraphs.IndexOf(p));
                }

                if (IsActionEnabled(CommandLineConverter.BatchAction.RemoveFormatting))
                {
                    p.Text = HtmlUtil.RemoveHtmlTags(p.Text, true);
                }

                if (!numericUpDownPercent.Value.Equals(100) && IsActionEnabled(CommandLineConverter.BatchAction.ChangeSpeed))
                {
                    var toSpeedPercentage = Convert.ToDouble(numericUpDownPercent.Value) / 100.0;
                    p.StartTime.TotalMilliseconds = p.StartTime.TotalMilliseconds * toSpeedPercentage;
                    p.EndTime.TotalMilliseconds = p.EndTime.TotalMilliseconds * toSpeedPercentage;

                    if (first)
                    {
                        first = false;
                    }
                    else
                    {
                        if (prev.EndTime.TotalMilliseconds >= p.StartTime.TotalMilliseconds)
                        {
                            prev.EndTime.TotalMilliseconds = p.StartTime.TotalMilliseconds - 1;
                        }
                    }
                }

                prev = p;
            }

            if (bluRaySubtitles == null || bluRaySubtitles.Count == 0)
            {
                sub.RemoveEmptyLines(); //TODO: only for image export?
            }

            if (IsActionEnabled(CommandLineConverter.BatchAction.RedoCasing))
            {
                _changeCasing.FixCasing(sub, LanguageAutoDetect.AutoDetectGoogleLanguage(sub));
                _changeCasingNames.Initialize(sub);
                _changeCasingNames.FixCasing();
            }

            if (IsActionEnabled(CommandLineConverter.BatchAction.MergeShortLines))
            {
                var mergedShortLinesSub = MergeShortLinesUtils.MergeShortLinesInSubtitle(sub, Configuration.Settings.Tools.MergeShortLinesMaxGap, Configuration.Settings.General.SubtitleLineMaximumLength, Configuration.Settings.Tools.MergeShortLinesOnlyContinuous);
                if (mergedShortLinesSub.Paragraphs.Count != sub.Paragraphs.Count)
                {
                    sub.Paragraphs.Clear();
                    sub.Paragraphs.AddRange(mergedShortLinesSub.Paragraphs);
                }
            }

            if (IsActionEnabled(CommandLineConverter.BatchAction.RemoveLineBreaks))
            {
                foreach (var paragraph in sub.Paragraphs)
                {
                    paragraph.Text = Utilities.UnbreakLine(paragraph.Text);
                }
            }

            if (IsActionEnabled(CommandLineConverter.BatchAction.MergeSameTexts))
            {
                var mergedSameTextsSub = MergeLinesSameTextUtils.MergeLinesWithSameTextInSubtitle(sub, true, 250);
                if (mergedSameTextsSub.Paragraphs.Count != sub.Paragraphs.Count)
                {
                    sub.Paragraphs.Clear();
                    sub.Paragraphs.AddRange(mergedSameTextsSub.Paragraphs);
                }
            }

            if (IsActionEnabled(CommandLineConverter.BatchAction.MergeSameTimeCodes))
            {
                var mergedSameTimeCodesSub = MergeLinesWithSameTimeCodes.Merge(sub, new List<int>(), out _, true, false, 1000, "en", new List<int>(), new Dictionary<int, bool>(), new Subtitle());
                if (mergedSameTimeCodesSub.Paragraphs.Count != sub.Paragraphs.Count)
                {
                    sub.Paragraphs.Clear();
                    sub.Paragraphs.AddRange(mergedSameTimeCodesSub.Paragraphs);
                }
            }

            if (IsActionEnabled(CommandLineConverter.BatchAction.ChangeFrameRate) &&
                double.TryParse(comboBoxFrameRateFrom.Text.Replace(',', '.').Replace(CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator, "."), NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var fromFrameRate) &&
                double.TryParse(comboBoxFrameRateTo.Text.Replace(',', '.').Replace(CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator, "."), NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var toFrameRate))
            {
                sub.ChangeFrameRate(fromFrameRate, toFrameRate);
            }

            if (IsActionEnabled(CommandLineConverter.BatchAction.OffsetTimeCodes) && timeUpDownAdjust.TimeCode.TotalMilliseconds > 0.00001)
            {
                var totalMilliseconds = timeUpDownAdjust.TimeCode.TotalMilliseconds;
                if (radioButtonShowEarlier.Checked)
                {
                    totalMilliseconds *= -1;
                }

                sub.AddTimeToAllParagraphs(TimeSpan.FromMilliseconds(totalMilliseconds));
            }

            return sub;
        }

        private void DeleteLines(Subtitle sub)
        {
            var skipFirst = (int)numericUpDownDeleteFirst.Value;
            var skipLast = (int)numericUpDownDeleteLast.Value;
            var deleteContains = textBoxDeleteContains.Text;
            if (skipFirst == 0 && skipLast == 0 && string.IsNullOrWhiteSpace(deleteContains))
            {
                return;
            }

            var paragraphs = sub.Paragraphs.Skip(skipFirst).ToList();
            paragraphs = paragraphs.Take(paragraphs.Count - skipLast).ToList();
            if (!string.IsNullOrWhiteSpace(deleteContains))
            {
                paragraphs = paragraphs.Where(p => !p.Text.Contains(deleteContains)).ToList();
            }

            sub.Paragraphs.Clear();
            sub.Paragraphs.AddRange(paragraphs);
            sub.Renumber();
        }

        private void UpdateChangeCasingSettings()
        {
            if (radioButtonNormal.Checked)
            {
                Configuration.Settings.Tools.ChangeCasingChoice = "Normal";
            }
            else if (radioButtonFixOnlyNames.Checked)
            {
                Configuration.Settings.Tools.ChangeCasingChoice = "NamesOnly";
            }
            else if (radioButtonUppercase.Checked)
            {
                Configuration.Settings.Tools.ChangeCasingChoice = "Uppercase";
            }
            else if (radioButtonLowercase.Checked)
            {
                Configuration.Settings.Tools.ChangeCasingChoice = "Lowercase";
            }
        }

        private Dictionary<CommandLineConverter.BatchAction, bool> _actionEnabledCache;

        private void UpdateActionEnabledCache()
        {
            _actionEnabledCache = new Dictionary<CommandLineConverter.BatchAction, bool>();
            foreach (ListViewItem item in listViewConvertOptions.Items)
            {
                var fixItem = item.Tag as FixActionItem;
                _actionEnabledCache.Add(fixItem.Action, item.Checked);
            }
        }

        private bool IsActionEnabled(CommandLineConverter.BatchAction action)
        {
            if (_actionEnabledCache != null)
            {
                return _actionEnabledCache[action];
            }

            foreach (ListViewItem item in listViewConvertOptions.Items)
            {
                if (item.Tag is FixActionItem fixItem && fixItem.Action == action)
                {
                    return item.Checked;
                }
            }
            return false;
        }

        private BackgroundWorker SpawnWorker()
        {
            var worker = new BackgroundWorker();
            worker.DoWork += DoThreadWork;
            worker.RunWorkerCompleted += ThreadWorkerCompleted;
            return worker;
        }

        /// <summary>
        /// Text based functions requires text, so no image to image convert
        /// </summary>
        /// <returns></returns>
        private bool AllowImageToImage()
        {
            return !IsActionEnabled(CommandLineConverter.BatchAction.BalanceLines) &&
                   !IsActionEnabled(CommandLineConverter.BatchAction.RedoCasing) &&
                   !IsActionEnabled(CommandLineConverter.BatchAction.FixCommonErrors) &&
                   !IsActionEnabled(CommandLineConverter.BatchAction.FixRtl) &&
                   !IsActionEnabled(CommandLineConverter.BatchAction.MultipleReplace) &&
                   !IsActionEnabled(CommandLineConverter.BatchAction.RemoveFormatting) &&
                   !IsActionEnabled(CommandLineConverter.BatchAction.SplitLongLines) &&
                   !IsActionEnabled(CommandLineConverter.BatchAction.RemoveTextForHI) &&
                   !IsActionEnabled(CommandLineConverter.BatchAction.MergeSameTexts) &&
                   !IsActionEnabled(CommandLineConverter.BatchAction.MergeSameTimeCodes) &&
                   !IsActionEnabled(CommandLineConverter.BatchAction.MergeShortLines) &&
                   !IsActionEnabled(CommandLineConverter.BatchAction.RemoveLineBreaks);
        }

        internal static List<VobSubMergedPack> LoadVobSubFromMatroska(MatroskaTrackInfo matroskaSubtitleInfo, MatroskaFile matroska, out Core.VobSub.Idx idx)
        {
            var mergedVobSubPacks = new List<VobSubMergedPack>();
            if (matroskaSubtitleInfo.ContentEncodingType == 1)
            {
                idx = null;
                return mergedVobSubPacks;
            }

            var sub = matroska.GetSubtitle(matroskaSubtitleInfo.TrackNumber, null);
            idx = new Core.VobSub.Idx(matroskaSubtitleInfo.GetCodecPrivate().SplitToLines());
            foreach (var p in sub)
            {
                mergedVobSubPacks.Add(new VobSubMergedPack(p.GetData(matroskaSubtitleInfo), TimeSpan.FromMilliseconds(p.Start), 32, null));
                if (mergedVobSubPacks.Count > 0)
                {
                    mergedVobSubPacks[mergedVobSubPacks.Count - 1].EndTime = TimeSpan.FromMilliseconds(p.End);
                }

                // fix overlapping (some versions of Handbrake makes overlapping time codes - thx Hawke)
                if (mergedVobSubPacks.Count > 1 && mergedVobSubPacks[mergedVobSubPacks.Count - 2].EndTime > mergedVobSubPacks[mergedVobSubPacks.Count - 1].StartTime)
                {
                    mergedVobSubPacks[mergedVobSubPacks.Count - 2].EndTime = TimeSpan.FromMilliseconds(mergedVobSubPacks[mergedVobSubPacks.Count - 1].StartTime.TotalMilliseconds - 1);
                }
            }
            return mergedVobSubPacks;
        }

        internal static List<BluRaySupParser.PcsData> LoadBluRaySupFromMatroska(MatroskaTrackInfo track, MatroskaFile matroska, IntPtr handle)
        {
            if (track.ContentEncodingType == 1)
            {
                return new List<BluRaySupParser.PcsData>();
            }

            var sub = matroska.GetSubtitle(track.TrackNumber, null);
            TaskbarList.SetProgressState(handle, TaskbarButtonProgressFlags.NoProgress);
            var subtitles = new List<BluRaySupParser.PcsData>();
            var log = new StringBuilder();
            var clusterStream = new MemoryStream();
            var lastPalettes = new Dictionary<int, List<PaletteInfo>>();
            var lastBitmapObjects = new Dictionary<int, List<BluRaySupParser.OdsData>>();
            foreach (var p in sub)
            {
                byte[] buffer = p.GetData(track);
                if (buffer != null && buffer.Length > 2)
                {
                    clusterStream.Write(buffer, 0, buffer.Length);
                    if (ContainsBluRayStartSegment(buffer))
                    {
                        if (subtitles.Count > 0 && subtitles[subtitles.Count - 1].StartTime == subtitles[subtitles.Count - 1].EndTime)
                        {
                            subtitles[subtitles.Count - 1].EndTime = (long)((p.Start - 1) * 90.0);
                        }
                        clusterStream.Position = 0;
                        var list = BluRaySupParser.ParseBluRaySup(clusterStream, log, true, lastPalettes, lastBitmapObjects);
                        foreach (var sup in list)
                        {
                            sup.StartTime = (long)((p.Start - 1) * 90.0);
                            sup.EndTime = (long)((p.End - 1) * 90.0);
                            subtitles.Add(sup);

                            // fix overlapping
                            if (subtitles.Count > 1 && sub[subtitles.Count - 2].End > sub[subtitles.Count - 1].Start)
                            {
                                subtitles[subtitles.Count - 2].EndTime = subtitles[subtitles.Count - 1].StartTime - 1;
                            }
                        }
                        clusterStream = new MemoryStream();
                    }
                }
                else if (subtitles.Count > 0)
                {
                    var lastSub = subtitles[subtitles.Count - 1];
                    if (lastSub.StartTime == lastSub.EndTime)
                    {
                        lastSub.EndTime = (long)((p.Start - 1) * 90.0);
                        if (lastSub.EndTime - lastSub.StartTime > 1000000)
                        {
                            lastSub.EndTime = lastSub.StartTime;
                        }
                    }
                }
            }

            clusterStream.Dispose();
            return subtitles;
        }

        private static bool ContainsBluRayStartSegment(byte[] buffer)
        {
            const int epochStart = 0x80;
            var position = 0;
            while (position + 3 <= buffer.Length)
            {
                var segmentType = buffer[position];
                if (segmentType == epochStart)
                {
                    return true;
                }

                int length = BluRaySupParser.BigEndianInt16(buffer, position + 1) + 3;
                position += length;
            }
            return false;
        }

        private bool CheckSkipFilter(string fileName, SubtitleFormat format, Subtitle sub)
        {
            bool skip = false;
            if (comboBoxFilter.SelectedIndex == 1)
            {
                if (format != null && format.GetType() == typeof(SubRip) && FileUtil.HasUtf8Bom(fileName))
                {
                    skip = true;
                }
            }
            else if (comboBoxFilter.SelectedIndex == 2)
            {
                skip = true;
                foreach (Paragraph p in sub.Paragraphs)
                {
                    if (p.Text != null && Utilities.GetNumberOfLines(p.Text) > 2)
                    {
                        skip = false;
                        break;
                    }
                }
            }
            else if (comboBoxFilter.SelectedIndex == 3 && !string.IsNullOrWhiteSpace(textBoxFilter.Text))
            {
                skip = true;
                foreach (Paragraph p in sub.Paragraphs)
                {
                    if (p.Text != null && p.Text.Contains(textBoxFilter.Text, StringComparison.Ordinal))
                    {
                        skip = false;
                        break;
                    }
                }
            }
            return skip;
        }

        private void IncrementAndShowProgress()
        {
            if (progressBar1.Value < progressBar1.Maximum)
            {
                progressBar1.Value++;
            }
            else
            {
                progressBar1.Value = progressBar1.Maximum;
            }
            progressBar1.Refresh();

            TaskbarList.SetProgressValue(Handle, progressBar1.Value, progressBar1.Maximum);
            labelStatus.Text = progressBar1.Value + " / " + progressBar1.Maximum;
            if (progressBar1.Value == progressBar1.Maximum)
            {
                labelStatus.Text = string.Empty;
            }

            Application.DoEvents();
        }

        private static void DoThreadWork(object sender, DoWorkEventArgs e)
        {
            var p = (ThreadDoWorkParameter)e.Argument;
            var mode = Configuration.Settings.Tools.BatchConvertFixRtlMode;

            ApplyFixesStep2(p, mode);

            e.Result = p;
        }

        private static void ApplyFixesStep2(ThreadDoWorkParameter p, string mode)
        {
            if (p.FixRtl && mode == RemoveUnicode)
            {
                for (int i = 0; i < p.Subtitle.Paragraphs.Count; i++)
                {
                    var paragraph = p.Subtitle.Paragraphs[i];
                    paragraph.Text = Utilities.RemoveUnicodeControlChars(paragraph.Text);
                }
            }

            if (p.FixCommonErrors)
            {
                try
                {
                    using (var fixCommonErrors = new FixCommonErrors { BatchMode = true })
                    {
                        var l = Configuration.Settings.Tools.BatchConvertLanguage;
                        if (string.IsNullOrEmpty(l))
                        {
                            l = LanguageAutoDetect.AutoDetectGoogleLanguage(p.Subtitle);
                        }

                        for (int i = 0; i < 3; i++)
                        {
                            fixCommonErrors.RunBatch(p.Subtitle, p.Format, p.Encoding.Encoding, l);
                            p.Subtitle = fixCommonErrors.FixedSubtitle;
                        }
                    }
                }
                catch (Exception exception)
                {
                    p.Error = string.Format(LanguageSettings.Current.BatchConvert.FixCommonErrorsErrorX, exception.Message);
                }
            }

            if (p.MultipleReplaceActive)
            {
                try
                {
                    using (var form = new MultipleReplace())
                    {
                        form.RunFromBatch(p.Subtitle);
                        p.Subtitle = form.FixedSubtitle;
                        p.Subtitle.RemoveParagraphsByIndices(form.DeleteIndices);
                    }
                }
                catch (Exception exception)
                {
                    p.Error = string.Format(LanguageSettings.Current.BatchConvert.MultipleReplaceErrorX, exception.Message);
                }
            }

            if (p.SplitLongLinesActive)
            {
                try
                {
                    p.Subtitle = SplitLongLinesHelper.SplitLongLinesInSubtitle(p.Subtitle, Configuration.Settings.General.SubtitleLineMaximumLength * 2, Configuration.Settings.General.SubtitleLineMaximumLength);
                }
                catch (Exception exception)
                {
                    p.Error = string.Format(LanguageSettings.Current.BatchConvert.AutoBalanceErrorX, exception.Message);
                }
            }

            if (p.AutoBalanceActive)
            {
                try
                {
                    var l = LanguageAutoDetect.AutoDetectGoogleLanguageOrNull(p.Subtitle);
                    foreach (var paragraph in p.Subtitle.Paragraphs)
                    {
                        paragraph.Text = Utilities.AutoBreakLine(paragraph.Text, l ?? p.Language);
                    }
                }
                catch (Exception exception)
                {
                    p.Error = string.Format(LanguageSettings.Current.BatchConvert.AutoBalanceErrorX, exception.Message);
                }
            }

            if (p.SetMinDisplayTimeBetweenSubtitles)
            {
                double minimumMillisecondsBetweenLines = Configuration.Settings.General.MinimumMillisecondsBetweenLines;
                for (int i = 0; i < p.Subtitle.Paragraphs.Count - 1; i++)
                {
                    var current = p.Subtitle.GetParagraphOrDefault(i);
                    var next = p.Subtitle.GetParagraphOrDefault(i + 1);
                    var gapsBetween = next.StartTime.TotalMilliseconds - current.EndTime.TotalMilliseconds;
                    if (gapsBetween < minimumMillisecondsBetweenLines && current.Duration.TotalMilliseconds > minimumMillisecondsBetweenLines)
                    {
                        current.EndTime.TotalMilliseconds -= (minimumMillisecondsBetweenLines - gapsBetween);
                    }
                }
            }

            if (p.FixRtl && mode == ReverseStartEnd)
            {
                for (int i = 0; i < p.Subtitle.Paragraphs.Count; i++)
                {
                    var paragraph = p.Subtitle.Paragraphs[i];
                    paragraph.Text = Utilities.ReverseStartAndEndingForRightToLeft(paragraph.Text);
                }
            }
            else if (p.FixRtl && mode == AddUnicode) // fix with unicode char
            {
                for (int i = 0; i < p.Subtitle.Paragraphs.Count; i++)
                {
                    var paragraph = p.Subtitle.Paragraphs[i];
                    paragraph.Text = Utilities.FixRtlViaUnicodeChars(paragraph.Text);
                }
            }

            // always re-number
            p.Subtitle.Renumber();
        }

        private void ThreadWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            CommandLineConverter.BatchConvertProgress progressCallback = null;
            var p = (ThreadDoWorkParameter)e.Result;
            if (p.Item.Index + 2 < listViewInputFiles.Items.Count)
            {
                listViewInputFiles.EnsureVisible(p.Item.Index + 2);
            }
            else
            {
                listViewInputFiles.EnsureVisible(p.Item.Index);
            }

            if (!string.IsNullOrEmpty(p.Error))
            {
                p.Item.SubItems[3].Text = p.Error;
            }
            else
            {
                if (p.SourceFormat == null)
                {
                    var ext = Path.GetExtension(p.FileName);
                    if (ext != null &&
                        (ext.Equals(".ts", StringComparison.OrdinalIgnoreCase) ||
                         ext.Equals(".m2ts", StringComparison.OrdinalIgnoreCase) ||
                         ext.Equals(".mts", StringComparison.OrdinalIgnoreCase) ||
                         ext.Equals(".mpg", StringComparison.OrdinalIgnoreCase) ||
                         ext.Equals(".mpeg", StringComparison.OrdinalIgnoreCase)) &&
                        (FileUtil.IsTransportStream(p.FileName) || FileUtil.IsM2TransportStream(p.FileName)))
                    {
                        IncrementAndShowProgress();
                        return;
                    }

                    p.SourceFormat = new SubRip();
                }

                if (p.ToFormat == Ebu.NameOfFormat)
                {
                    p.Subtitle.Header = _ebuGeneralInformation.ToString();
                }

                var targetFormat = p.ToFormat;
                if (targetFormat == LanguageSettings.Current.ExportCustomText.Title)
                {
                    targetFormat = "CustomText:" + _customTextTemplate;
                }

                try
                {
                    var binaryParagraphs = new List<IBinaryParagraph>();
                    if (p.FileName != null && !p.Subtitle.Paragraphs.Any(s => !string.IsNullOrEmpty(s.Text)) &&
                        p.FileName.EndsWith(".sup", StringComparison.OrdinalIgnoreCase) &&
                        FileUtil.IsBluRaySup(p.FileName) && AllowImageToImage())
                    {
                        binaryParagraphs = BluRaySupParser.ParseBluRaySup(p.FileName, new StringBuilder()).Cast<IBinaryParagraph>().ToList();
                    }
                    else if (p.FileName != null && _bdLookup.ContainsKey(p.FileName))
                    {
                        binaryParagraphs = _bdLookup[p.FileName].Cast<IBinaryParagraph>().ToList();
                    }
                    var dir = textBoxOutputFolder.Text;
                    var overwrite = checkBoxOverwrite.Checked;
                    if (radioButtonSaveInSourceFolder.Checked)
                    {
                        dir = Path.GetDirectoryName(p.FileName);
                    }
                    var success = CommandLineConverter.BatchConvertSave(targetFormat, TimeSpan.Zero, GetCurrentEncoding(p.FileName), dir, _count, ref _converted, ref _errors, _allFormats, p.FileName, p.Subtitle, p.SourceFormat, binaryParagraphs, overwrite, -1, null, null, null, null, false, progressCallback);
                    if (success)
                    {
                        p.Item.SubItems[3].Text = LanguageSettings.Current.BatchConvert.Converted;
                    }
                    else
                    {
                        p.Item.SubItems[3].Text = LanguageSettings.Current.BatchConvert.NotConverted + "  " + p.Item.SubItems[3].Text.Trim('-').Trim();
                    }
                }
                catch (Exception exception)
                {
                    p.Error = $"Save: {exception.InnerException?.Message ?? exception.Message}";
                    p.Item.SubItems[3].Text = p.Error;
                }

                IncrementAndShowProgress();
            }
        }

        private void ComboBoxSubtitleFormatsSelectedIndexChanged(object sender, EventArgs e)
        {
            checkBoxUseStyleFromSource.Visible = false;
            if (comboBoxSubtitleFormats.Text == AdvancedSubStationAlpha.NameOfFormat || comboBoxSubtitleFormats.Text == SubStationAlpha.NameOfFormat)
            {
                buttonStyles.Text = LanguageSettings.Current.BatchConvert.Style;
                buttonStyles.Visible = true;
                comboBoxEncoding.Enabled = true;
                buttonBrowseEncoding.Visible = true;
                checkBoxUseStyleFromSource.Visible = true;
                checkBoxUseStyleFromSource.Left = buttonStyles.Left + buttonStyles.Width - checkBoxUseStyleFromSource.Width;
            }
            else if (comboBoxSubtitleFormats.Text == Ebu.NameOfFormat)
            {
                buttonStyles.Text = LanguageSettings.Current.BatchConvert.Settings;
                buttonStyles.Visible = true;
                if (_ebuGeneralInformation == null)
                {
                    _ebuGeneralInformation = new Ebu.EbuGeneralSubtitleInformation();
                }

                comboBoxEncoding.Enabled = true;
                buttonBrowseEncoding.Visible = true;
            }
            else if (comboBoxSubtitleFormats.Text == BluRaySubtitle ||
                     comboBoxSubtitleFormats.Text == VobSubSubtitle ||
                     comboBoxSubtitleFormats.Text == DostImageSubtitle ||
                     comboBoxSubtitleFormats.Text == BdnXmlSubtitle ||
                     comboBoxSubtitleFormats.Text == FcpImageSubtitle ||
                     comboBoxSubtitleFormats.Text == LanguageSettings.Current.ExportCustomText.Title)
            {
                buttonStyles.Text = LanguageSettings.Current.BatchConvert.Settings;
                buttonStyles.Visible = true;
                comboBoxEncoding.Enabled = false;
                buttonBrowseEncoding.Visible = false;
            }
            else if (comboBoxSubtitleFormats.Text == LanguageSettings.Current.BatchConvert.PlainText)
            {
                buttonStyles.Text = LanguageSettings.Current.BatchConvert.Settings;
                buttonStyles.Visible = true;
                comboBoxEncoding.Enabled = true;
                buttonBrowseEncoding.Visible = true;
            }
            else
            {
                buttonStyles.Visible = false;
                comboBoxEncoding.Enabled = true;
                buttonBrowseEncoding.Visible = true;
            }
        }

        private void ButtonStylesClick(object sender, EventArgs e)
        {
            if (comboBoxSubtitleFormats.Text == AdvancedSubStationAlpha.NameOfFormat || comboBoxSubtitleFormats.Text == SubStationAlpha.NameOfFormat)
            {
                ShowAssSsaStyles();
            }
            else if (comboBoxSubtitleFormats.Text == Ebu.NameOfFormat)
            {
                ShowEbuSettings();
            }
            else if (comboBoxSubtitleFormats.Text == BluRaySubtitle)
            {
                ImageExportSettings(ExportPngXml.ExportFormats.BluraySup);
            }
            else if (comboBoxSubtitleFormats.Text == VobSubSubtitle)
            {
                ImageExportSettings(ExportPngXml.ExportFormats.VobSub);
            }
            else if (comboBoxSubtitleFormats.Text == DostImageSubtitle)
            {
                ImageExportSettings(ExportPngXml.ExportFormats.Dost);
            }
            else if (comboBoxSubtitleFormats.Text == BdnXmlSubtitle)
            {
                ImageExportSettings(ExportPngXml.ExportFormats.BdnXml);
            }
            else if (comboBoxSubtitleFormats.Text == FcpImageSubtitle)
            {
                ImageExportSettings(ExportPngXml.ExportFormats.Fcp);
            }
            else if (comboBoxSubtitleFormats.Text == LanguageSettings.Current.BatchConvert.PlainText)
            {
                using (var form = new ExportText())
                {
                    var s = new Subtitle();
                    s.Paragraphs.Add(new Paragraph("Test 123." + Environment.NewLine + "Test 456.", 0, 4000));
                    s.Paragraphs.Add(new Paragraph("Test 789.", 5000, 9000));
                    form.Initialize(s, null);
                    form.PrepareForBatchSettings();
                    form.ShowDialog(this);
                }
            }
            else if (comboBoxSubtitleFormats.Text == LanguageSettings.Current.ExportCustomText.Title)
            {
                ShowExportCustomTextSettings();
            }
        }

        private void ShowExportCustomTextSettings()
        {
            var s = new Subtitle();
            s.Paragraphs.Add(new Paragraph("Test 123." + Environment.NewLine + "Test 456.", 0, 4000));
            s.Paragraphs.Add(new Paragraph("Test 777." + Environment.NewLine + "Test 888.", 0, 4000));
            using (var properties = new ExportCustomText(s, null, "Test"))
            {
                properties.InitializeForBatchConvert(_customTextTemplate);
                if (properties.ShowDialog(this) == DialogResult.OK)
                {
                    _customTextTemplate = properties.CurrentFormatName;
                }
            }
        }

        private void ImageExportSettings(string format)
        {
            using (var properties = new ExportPngXml())
            {
                var s = new Subtitle();
                s.Paragraphs.Add(new Paragraph("Test 123." + Environment.NewLine + "Test 456.", 0, 4000));
                properties.Initialize(s, new SubRip(), format, null, null, null);
                properties.DisableSaveButtonAndCheckBoxes();
                properties.ShowDialog(this);
            }
        }

        private void ShowEbuSettings()
        {
            using (var properties = new EbuSaveOptions())
            {
                properties.Initialize(_ebuGeneralInformation, 0, null, null);
                properties.ShowDialog(this);
            }
        }

        private void ShowAssSsaStyles()
        {
            SubStationAlphaStylesBatchConvert form = null;
            try
            {
                var assa = new AdvancedSubStationAlpha();
                var sub = new Subtitle();
                if (comboBoxSubtitleFormats.Text == assa.Name)
                {
                    if (!string.IsNullOrEmpty(_assStyle))
                    {
                        sub.Header = _assStyle;
                    }
                    form = new SubStationAlphaStylesBatchConvert(sub, assa);
                    if (form.ShowDialog(this) == DialogResult.OK)
                    {
                        _assStyle = form.Header;
                    }
                }
                else
                {
                    if (!string.IsNullOrEmpty(_ssaStyle))
                    {
                        sub.Header = _ssaStyle;
                    }
                    var ssa = new SubStationAlpha();
                    if (comboBoxSubtitleFormats.Text == ssa.Name)
                    {
                        form = new SubStationAlphaStylesBatchConvert(sub, ssa);
                        if (form.ShowDialog(this) == DialogResult.OK)
                        {
                            _ssaStyle = form.Header;
                        }
                    }
                }
            }
            finally
            {
                form?.Dispose();
            }
        }

        private void ContextMenuStripFilesOpening(object sender, CancelEventArgs e)
        {
            if (_converting)
            {
                e.Cancel = true;
                return;
            }
            removeToolStripMenuItem.Visible = listViewInputFiles.SelectedItems.Count > 0;
            openContainingFolderToolStripMenuItem.Visible = listViewInputFiles.SelectedItems.Count == 1;
            removeAllToolStripMenuItem.Visible = listViewInputFiles.Items.Count > 0;
            toolStripSeparator1.Visible = listViewInputFiles.Items.Count > 0;
        }

        private void RemoveAllToolStripMenuItemClick(object sender, EventArgs e)
        {
            listViewInputFiles.Items.Clear();
            UpdateNumberOfFiles();
            UpdateTransportStreamSettings();
        }

        private void RemoveSelectedFiles()
        {
            if (_converting)
            {
                return;
            }

            var first = int.MaxValue;
            for (int i = listViewInputFiles.SelectedIndices.Count - 1; i >= 0; i--)
            {
                var idx = listViewInputFiles.SelectedIndices[i];
                if (idx < first)
                {
                    first = idx;
                }

                listViewInputFiles.Items.RemoveAt(idx);
            }

            // keep an item selected/focused for improved UX
            if (first < listViewInputFiles.Items.Count && listViewInputFiles.Items.Count > 0)
            {
                listViewInputFiles.Items[first].Selected = true;
                listViewInputFiles.FocusedItem = listViewInputFiles.Items[first];
            }
            else if (listViewInputFiles.Items.Count > 0)
            {
                listViewInputFiles.Items[listViewInputFiles.Items.Count - 1].Selected = true;
                listViewInputFiles.FocusedItem = listViewInputFiles.Items[listViewInputFiles.Items.Count - 1];
            }

            UpdateNumberOfFiles();
            UpdateTransportStreamSettings();
        }

        private void RemoveToolStripMenuItemClick(object sender, EventArgs e)
        {
            RemoveSelectedFiles();
        }

        private void UpdateTransportStreamSettings()
        {
            bool hasTransportStream = false;
            foreach (ListViewItem lvi in listViewInputFiles.Items)
            {
                if (lvi.SubItems[2].Text.Equals("Transport Stream", StringComparison.OrdinalIgnoreCase))
                {
                    hasTransportStream = true;
                    break;
                }
            }

            buttonTransportStreamSettings.Visible = hasTransportStream;
        }

        private void ListViewInputFilesKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {
                RemoveSelectedFiles();
            }
            else if (e.KeyCode == Keys.A && e.Modifiers == Keys.Control)
            {
                listViewInputFiles.SelectAll();
                e.SuppressKeyPress = true;
            }
            else if (e.KeyCode == Keys.D && e.Modifiers == Keys.Control)
            {
                listViewInputFiles.SelectFirstSelectedItemOnly();
                e.SuppressKeyPress = true;
            }
            else if (e.KeyCode == Keys.I && e.Modifiers == (Keys.Control | Keys.Shift)) //InverseSelection
            {
                listViewInputFiles.InverseSelection();
                e.SuppressKeyPress = true;
            }
            else if (e.KeyData == UiUtil.HelpKeys)
            {
                UiUtil.ShowHelp("#batchconvert");
                e.SuppressKeyPress = true;
            }
        }

        private void BatchConvert_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (_converting)
            {
                e.Cancel = true;
                return;
            }
            if (_searching)
            {
                _abort = true;
            }

            Configuration.Settings.Tools.BatchConvertFixCasing = IsActionEnabled(CommandLineConverter.BatchAction.RedoCasing);
            Configuration.Settings.Tools.BatchConvertFixCommonErrors = IsActionEnabled(CommandLineConverter.BatchAction.FixCommonErrors);
            Configuration.Settings.Tools.BatchConvertMultipleReplace = IsActionEnabled(CommandLineConverter.BatchAction.MultipleReplace);
            Configuration.Settings.Tools.BatchConvertFixRtl = IsActionEnabled(CommandLineConverter.BatchAction.FixRtl);
            Configuration.Settings.Tools.BatchConvertSplitLongLines = IsActionEnabled(CommandLineConverter.BatchAction.SplitLongLines);
            Configuration.Settings.Tools.BatchConvertAutoBalance = IsActionEnabled(CommandLineConverter.BatchAction.BalanceLines);
            Configuration.Settings.Tools.BatchConvertRemoveFormatting = IsActionEnabled(CommandLineConverter.BatchAction.RemoveFormatting);
            Configuration.Settings.Tools.BatchConvertBridgeGaps = IsActionEnabled(CommandLineConverter.BatchAction.BridgeGaps);
            Configuration.Settings.Tools.BatchConvertRemoveTextForHI = IsActionEnabled(CommandLineConverter.BatchAction.RemoveTextForHI);
            Configuration.Settings.Tools.BatchConvertSetMinDisplayTimeBetweenSubtitles = IsActionEnabled(CommandLineConverter.BatchAction.SetMinGap);
            Configuration.Settings.Tools.BatchConvertOutputFolder = textBoxOutputFolder.Text;
            Configuration.Settings.Tools.BatchConvertOverwriteExisting = checkBoxOverwrite.Checked;
            Configuration.Settings.Tools.BatchConvertFormat = comboBoxSubtitleFormats.SelectedItem.ToString();
            Configuration.Settings.Tools.BatchConvertAssStyles = _assStyle;
            Configuration.Settings.Tools.BatchConvertSsaStyles = _ssaStyle;
            Configuration.Settings.Tools.BatchConvertUseStyleFromSource = checkBoxUseStyleFromSource.Checked;
            Configuration.Settings.Tools.BatchConvertExportCustomTextTemplate = _customTextTemplate;
            Configuration.Settings.Tools.BatchConvertSaveInSourceFolder = radioButtonSaveInSourceFolder.Checked;
            Configuration.Settings.Tools.BatchConvertMergeShortLines = IsActionEnabled(CommandLineConverter.BatchAction.MergeShortLines);
            Configuration.Settings.Tools.BatchConvertRemoveLineBreaks = IsActionEnabled(CommandLineConverter.BatchAction.RemoveLineBreaks);
            Configuration.Settings.Tools.BatchConvertMergeSameText = IsActionEnabled(CommandLineConverter.BatchAction.MergeSameTexts);
            Configuration.Settings.Tools.BatchConvertMergeSameTimeCodes = IsActionEnabled(CommandLineConverter.BatchAction.MergeSameTimeCodes);
            Configuration.Settings.Tools.BatchConvertChangeSpeed = IsActionEnabled(CommandLineConverter.BatchAction.ChangeSpeed);
            Configuration.Settings.Tools.BatchConvertChangeFrameRate = IsActionEnabled(CommandLineConverter.BatchAction.ChangeFrameRate);
            Configuration.Settings.Tools.BatchConvertOffsetTimeCodes = IsActionEnabled(CommandLineConverter.BatchAction.OffsetTimeCodes);
            Configuration.Settings.Tools.BatchConvertApplyDurationLimits = IsActionEnabled(CommandLineConverter.BatchAction.ApplyDurationLimits);
            Configuration.Settings.Tools.MergeShortLinesMaxGap = (int)numericUpDownMaxMillisecondsBetweenLines.Value;
            Configuration.Settings.Tools.MergeShortLinesOnlyContinuous = checkBoxOnlyContinuationLines.Checked;

            UpdateRtlSettings();
        }

        private void UpdateRtlSettings()
        {
            if (radioButtonRemoveUnicode.Checked)
            {
                Configuration.Settings.Tools.BatchConvertFixRtlMode = RemoveUnicode;
            }
            else if (radioButtonReverseStartEnd.Checked)
            {
                Configuration.Settings.Tools.BatchConvertFixRtlMode = ReverseStartEnd;
            }
            else
            {
                Configuration.Settings.Tools.BatchConvertFixRtlMode = AddUnicode;
            }
        }

        private string _rootFolder;
        private void buttonSearchFolder_Click(object sender, EventArgs e)
        {
            folderBrowserDialog1.ShowNewFolderButton = false;
            folderBrowserDialog1.SelectedPath = _rootFolder;
            if (folderBrowserDialog1.ShowDialog(this) == DialogResult.OK)
            {
                listViewInputFiles.BeginUpdate();
                progressBar1.Style = ProgressBarStyle.Marquee;
                progressBar1.Visible = true;
                SetControlState(false);
                labelStatus.Text = string.Empty;

                SearchFolder(folderBrowserDialog1.SelectedPath);
                _rootFolder = folderBrowserDialog1.SelectedPath;

                labelStatus.Text = string.Empty;

                SetControlState(true);
                listViewInputFiles.EndUpdate();
            }
            progressBar1.Value = 0;
            progressBar1.Visible = false;
            UpdateNumberOfFiles();
        }

        private void SetControlState(bool enabled)
        {
            labelStatus.Text = string.Empty;
            buttonCancel.Enabled = enabled;
            groupBoxOutput.Enabled = enabled;
            groupBoxConvertOptions.Enabled = enabled;
            buttonInputBrowse.Enabled = enabled;
            buttonSearchFolder.Enabled = enabled;
            checkBoxScanFolderRecursive.Enabled = enabled;
            comboBoxFilter.Enabled = enabled;

            if (enabled)
            {
                buttonConvert.Text = LanguageSettings.Current.BatchConvert.Convert;
            }
            else
            {
                buttonConvert.Text = LanguageSettings.Current.General.Cancel;
            }
        }

        private void SearchFolder(string path)
        {
            _abort = false;
            _searching = true;
            try
            {
                if (checkBoxScanFolderRecursive.Checked)
                {
                    ScanFiles(Directory.EnumerateFiles(path, "*", SearchOption.AllDirectories));
                }
                else
                {
                    ScanFiles(Directory.EnumerateFiles(path));
                }
            }
            finally
            {
                _searching = false;
            }
        }

        private static readonly HashSet<string> SearchExtBlackList = new HashSet<string>
        {
            ".png",
            ".jpg",
            ".jpeg",
            ".tif",
            ".tiff",
            ".gif",
            ".bmp",
            ".clpi",
            ".mpls",
            ".wav",
            ".mp3",
            ".avi",
            ".mpeg",
            ".mpg",
            ".tar",
            ".docx",
            ".pptx",
            ".xlsx",
            ".odt",
            ".tex",
            ".pdf",
            ".dll",
            ".exe",
            ".rar",
            ".7z",
            ".zip",
            ".tar"
        };

        private void ScanFiles(IEnumerable<string> fileNames)
        {
            foreach (string fileName in fileNames)
            {
                labelStatus.Text = fileName;
                try
                {
                    string ext = Path.GetExtension(fileName).ToLowerInvariant();
                    if (!SearchExtBlackList.Contains(ext))
                    {
                        labelStatus.Refresh();
                        var fi = new FileInfo(fileName);
                        if (comboBoxFilter.SelectedIndex == 4 && textBoxFilter.Text.Length > 0 && !fileName.Contains(textBoxFilter.Text, StringComparison.OrdinalIgnoreCase))
                        {
                            // skip
                        }
                        else if (ext == ".sub" && FileUtil.IsVobSub(fileName))
                        {
                            AddFromSearch(fileName, fi, "VobSub");
                        }
                        else if (ext == ".sup" && FileUtil.IsBluRaySup(fileName))
                        {
                            AddFromSearch(fileName, fi, "Blu-ray");
                        }
                        else if (ext == ".mkv")
                        {
                            // skip for now
                        }
                        else if (ext == ".mks")
                        {
                            // skip for now
                        }
                        else if (ext == ".mp4")
                        {
                            // skip for now
                        }
                        else
                        {
                            if (fi.Length < ConvertMaxFileSize)
                            {
                                var sub = new Subtitle();
                                var enc = LanguageAutoDetect.GetEncodingFromFile(fileName, true);
                                var format = sub.LoadSubtitle(fileName, out _, enc, true, null, false);
                                if (format == null)
                                {
                                    foreach (var f in SubtitleFormat.GetBinaryFormats(true))
                                    {
                                        if (f.IsMine(null, fileName))
                                        {
                                            f.LoadSubtitle(sub, null, fileName);
                                            format = f;
                                            break;
                                        }
                                    }
                                }

                                if (format == null)
                                {
                                    var lines = FileUtil.ReadAllTextShared(fileName, enc).SplitToLines();
                                    foreach (var f in SubtitleFormat.GetTextOtherFormats())
                                    {
                                        if (f.IsMine(lines, fileName))
                                        {
                                            f.LoadSubtitle(sub, lines, fileName);
                                            format = f;
                                            break;
                                        }
                                    }
                                }

                                if (format != null)
                                {
                                    AddFromSearch(fileName, fi, format.Name);
                                }
                            }
                        }
                        progressBar1.Refresh();
                        Application.DoEvents();
                        if (_abort)
                        {
                            progressBar1.Value = 0;
                            progressBar1.Visible = false;
                            return;
                        }
                    }
                }
                catch
                {
                    // ignored
                }
            }
        }

        private void AddFromSearch(string fileName, FileInfo fi, string nameOfFormat)
        {
            var item = new ListViewItem(fileName);
            item.SubItems.Add(Utilities.FormatBytesToDisplayFileSize(fi.Length));
            item.SubItems.Add(nameOfFormat);
            item.SubItems.Add("-");
            listViewInputFiles.Items.Add(item);
            UpdateNumberOfFiles();
        }

        private void BatchConvert_KeyDown(object sender, KeyEventArgs e)
        {
            if (_converting || _searching)
            {
                if (e.KeyCode == Keys.Escape)
                {
                    _abort = true;
                    e.SuppressKeyPress = true;
                }
            }
            else if (e.KeyCode == Keys.Escape)
            {
                Close();
            }
            else if (e.KeyData == (Keys.Control | Keys.O)) // Open file/s
            {
                buttonInputBrowse_Click(null, EventArgs.Empty);
            }
        }

        private void BatchConvert_Shown(object sender, EventArgs e)
        {
            BatchConvert_ResizeEnd(sender, e);
        }

        private void BatchConvert_ResizeEnd(object sender, EventArgs e)
        {
            listViewInputFiles.AutoSizeLastColumn();
            listViewConvertOptions.AutoSizeLastColumn();
        }

        private void comboBoxFilter_SelectedIndexChanged(object sender, EventArgs e)
        {
            textBoxFilter.Visible = comboBoxFilter.SelectedIndex == 3 || comboBoxFilter.SelectedIndex == 4 || comboBoxFilter.SelectedIndex == 5;
        }

        private void textBoxFilter_TextChanged(object sender, EventArgs e)
        {
            if (listViewInputFiles.Items.Count == 0)
            {
                return;
            }

            if (comboBoxFilter.SelectedIndex == 5)
            {
                var listViewItems = new List<ListViewItem>();
                foreach (ListViewItem item in listViewInputFiles.Items)
                {
                    listViewItems.Add(item);
                }

                listViewInputFiles.BeginUpdate();
                listViewInputFiles.Items.Clear();
                listViewInputFiles.Items.AddRange(listViewItems.FindAll(item => item.SubItems[2].Text.Contains(textBoxFilter.Text, StringComparison.OrdinalIgnoreCase)).ToArray());
                listViewInputFiles.EndUpdate();
                UpdateNumberOfFiles();
            }
        }

        private void buttonTransportStreamSettings_Click(object sender, EventArgs e)
        {
            using (var form = new BatchConvertTsSettings())
            {
                form.ShowDialog(this);
            }
        }

        private void radioButtonSaveInSourceFolder_CheckedChanged(object sender, EventArgs e)
        {
            textBoxOutputFolder.Enabled = false;
            buttonChooseFolder.Enabled = false;
        }

        private void radioButtonSaveInOutputFolder_CheckedChanged(object sender, EventArgs e)
        {
            textBoxOutputFolder.Enabled = true;
            buttonChooseFolder.Enabled = true;
        }

        private void linkLabelOpenOutputFolder_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (Directory.Exists(textBoxOutputFolder.Text))
            {
                UiUtil.OpenFolder(textBoxOutputFolder.Text);
            }
            else
            {
                MessageBox.Show(string.Format(LanguageSettings.Current.SplitSubtitle.FolderNotFoundX, textBoxOutputFolder.Text));
            }
        }

        private void buttonChooseFolder_Click(object sender, EventArgs e)
        {
            folderBrowserDialog1.ShowNewFolderButton = true;
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                textBoxOutputFolder.Text = folderBrowserDialog1.SelectedPath;
            }
        }

        private void listViewConvertOptions_SelectedIndexChanged(object sender, EventArgs e)
        {
            groupBoxMergeShortLines.Visible = false;
            groupBoxChangeFrameRate.Visible = false;
            groupBoxSpeed.Visible = false;
            groupBoxOffsetTimeCodes.Visible = false;
            buttonConvertOptionsSettings.Visible = false;
            groupBoxFixRtl.Visible = false;
            groupBoxChangeCasing.Visible = false;
            groupBoxRemoveStyle.Visible = false;
            groupBoxAdjustDuration.Visible = false;
            groupBoxDeleteLines.Visible = false;

            if (listViewConvertOptions.SelectedIndices.Count != 1)
            {
                return;
            }

            var idx = listViewConvertOptions.SelectedIndices[0];
            var fixItem = listViewConvertOptions.Items[idx].Tag as FixActionItem;
            if (fixItem?.Control != null)
            {
                fixItem.Control.Top = listViewConvertOptions.Top;
                fixItem.Control.Left = listViewConvertOptions.Left + listViewConvertOptions.Width + 5;
                fixItem.Control.Visible = true;
                fixItem.Control.BringToFront();
                if (fixItem.Control is GroupBox groupBox)
                {
                    groupBox.Height = listViewConvertOptions.Height;
                    groupBox.Width = groupBoxConvertOptions.Width - listViewConvertOptions.Width - listViewConvertOptions.Left - 15;
                }
            }
        }

        private void ButtonOptionConvertSettings(object sender, EventArgs e)
        {
            if (listViewConvertOptions.SelectedIndices.Count != 1)
            {
                return;
            }
            var idx = listViewConvertOptions.SelectedIndices[0];
            var fixItem = listViewConvertOptions.Items[idx].Tag as FixActionItem;
            if (fixItem.Action == CommandLineConverter.BatchAction.RemoveTextForHI)
            {
                using (var form = new FormRemoveTextForHearImpaired(null, new Subtitle()))
                {
                    form.InitializeSettingsOnly();
                    form.ShowDialog(this);
                    _removeTextForHiSettings = form.GetSettings(new Subtitle());
                }
            }
            else if (fixItem.Action == CommandLineConverter.BatchAction.FixCommonErrors)
            {
                using (var form = new FixCommonErrors { BatchMode = true })
                {
                    form.RunBatchSettings(new Subtitle(), GetCurrentSubtitleFormat(), GetCurrentEncoding(null), Configuration.Settings.Tools.BatchConvertLanguage);
                    form.ShowDialog(this);
                    Configuration.Settings.Tools.BatchConvertLanguage = form.Language;
                }
            }
            else if (fixItem.Action == CommandLineConverter.BatchAction.BridgeGaps)
            {
                _bridgeGaps.ShowDialog(this);
            }
            else if (fixItem.Action == CommandLineConverter.BatchAction.MultipleReplace)
            {
                using (var form = new MultipleReplace())
                {
                    form.Initialize(new Subtitle());
                    form.ShowDialog(this);
                }
            }
        }

        private void buttonSwapFrameRate_Click(object sender, EventArgs e)
        {
            string oldFrameRate = comboBoxFrameRateFrom.Text;
            string newFrameRate = comboBoxFrameRateTo.Text;

            comboBoxFrameRateFrom.Text = newFrameRate;
            comboBoxFrameRateTo.Text = oldFrameRate;
        }

        private void listViewConvertOptions_ItemChecked(object sender, ItemCheckedEventArgs e)
        {
            var count = listViewConvertOptions.CheckedItems.Count;
            if (count > 0)
            {
                groupBoxConvertOptions.Text = LanguageSettings.Current.BatchConvert.ConvertOptions + "  " + count;
            }
            else
            {
                groupBoxConvertOptions.Text = LanguageSettings.Current.BatchConvert.ConvertOptions;
            }
        }

        private void toolStripMenuItemSelectAll_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in listViewConvertOptions.Items)
            {
                item.Checked = true;
            }
        }

        private void inverseSelectionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in listViewConvertOptions.Items)
            {
                item.Checked = !item.Checked;
            }
        }

        private void listViewInputFiles_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            if (_converting)
            {
                return;
            }

            var sorter = (ListViewSorter)listViewInputFiles.ListViewItemSorter;
            if (sorter == null)
            {
                sorter = new ListViewSorter
                {
                    ColumnNumber = e.Column,
                    IsNumber = false,
                    IsDisplayFileSize = e.Column == columnHeaderSize.DisplayIndex
                };
                listViewInputFiles.ListViewItemSorter = sorter;
            }

            if (e.Column == sorter.ColumnNumber)
            {
                sorter.Descending = !sorter.Descending; // inverse sort direction
            }
            else
            {
                sorter.ColumnNumber = e.Column;
                sorter.Descending = false;
                sorter.IsNumber = false;
                sorter.IsDisplayFileSize = e.Column == columnHeaderSize.DisplayIndex;
            }
            listViewInputFiles.Sort();
        }

        private void buttonBrowseEncoding_Click(object sender, EventArgs e)
        {
            openFileDialog1.Title = LanguageSettings.Current.Main.OpenAnsiSubtitle;
            openFileDialog1.FileName = string.Empty;
            openFileDialog1.Filter = UiUtil.SubtitleExtensionFilter.Value;
            if (openFileDialog1.ShowDialog(this) == DialogResult.OK)
            {
                using (var chooseEncoding = new ChooseEncoding())
                {
                    chooseEncoding.Initialize(openFileDialog1.FileName);
                    if (chooseEncoding.ShowDialog(this) == DialogResult.OK)
                    {
                        var encoding = chooseEncoding.GetEncoding();
                        for (var i = 0; i < comboBoxEncoding.Items.Count; i++)
                        {
                            var item = comboBoxEncoding.Items[i];
                            if (item is TextEncoding te)
                            {
                                if (te.Encoding.WebName == encoding.WebName)
                                {
                                    comboBoxEncoding.SelectedIndex = i;
                                    break;
                                }
                            }
                        }
                    }
                }
            }
        }

        private void UpdateNumberOfFiles()
        {
            if (listViewInputFiles.Items.Count > 0)
            {
                labelNumberOfFiles.Text = $"{listViewInputFiles.Items.Count:#,###,##0}";
            }
            else
            {
                labelNumberOfFiles.Text = string.Empty;
            }
        }

        private void openContainingFolderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_converting || listViewInputFiles.SelectedIndices.Count != 1)
            {
                return;
            }

            int idx = listViewInputFiles.SelectedIndices[0];
            var fileName = listViewInputFiles.Items[idx].Text;
            UiUtil.OpenFolderFromFileName(fileName);
        }

        private void convertMkvThreeLetterLanguageCodesToTwoLettersToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (var form = new BatchConvertMkvEnding { LanguageCodeStyle = Configuration.Settings.Tools.BatchConvertMkvLanguageCodeStyle })
            {
                var result = form.ShowDialog(this);
                if (result == DialogResult.OK)
                {
                    Configuration.Settings.Tools.BatchConvertMkvLanguageCodeStyle = form.LanguageCodeStyle;
                    SetMkvLanguageMenuItem();
                }
            }
        }

        private void comboBoxAdjustDurationVia_SelectedIndexChanged(object sender, EventArgs e)
        {
            panelAdjustDurationAddSeconds.Visible = false;
            panelAdjustDurationAddPercent.Visible = false;
            panelAdjustDurationFixed.Visible = false;
            panelAdjustDurationRecalc.Visible = false;

            var panel = panelAdjustDurationAddSeconds;
            if (comboBoxAdjustDurationVia.SelectedIndex == 1)
            {
                panel = panelAdjustDurationAddPercent;
            }
            else if (comboBoxAdjustDurationVia.SelectedIndex == 2)
            {
                panel = panelAdjustDurationRecalc;
            }
            else if (comboBoxAdjustDurationVia.SelectedIndex == 3)
            {
                panel = panelAdjustDurationFixed;
            }

            panel.Visible = true;
            panel.Left = 2;
            panel.Top = labelAdjustDurationVia.Top + labelAdjustDurationVia.Height + 9;
        }

        private void addFilesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            buttonInputBrowse_Click(sender, e);
        }
    }
}
