using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Logic;
using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.Forms.Assa
{
    public sealed partial class AssaProperties : PositionAndSizeForm
    {
        private readonly Subtitle _subtitle;
        private readonly string _videoFileName;
        private readonly VideoInfo _currentVideoInfo;
        private readonly int _height;

        public AssaProperties(Subtitle subtitle, string videoFileName, VideoInfo currentVideoInfo, string subtitleFileName)
        {
            UiUtil.PreInitialize(this);
            InitializeComponent();
            UiUtil.FixFonts(this);
            _subtitle = subtitle;
            _videoFileName = videoFileName;
            _currentVideoInfo = currentVideoInfo;

            var l = LanguageSettings.Current.SubStationAlphaProperties;
            _height = 500;
            Text = l.Title;
            comboBoxWrapStyle.SelectedIndex = 2;

            if (string.IsNullOrWhiteSpace(subtitle.Header))
            {
                subtitle.Header = AdvancedSubStationAlpha.DefaultHeader;
            }

            var header = subtitle.Header;
            foreach (var line in header.SplitToLines())
            {
                var s = line.ToLowerInvariant().Trim();
                if (s.StartsWith("title:", StringComparison.Ordinal))
                {
                    textBoxTitle.Text = line.Trim().Remove(0, 6).Trim();
                }
                else if (s.StartsWith("original script:", StringComparison.Ordinal))
                {
                    textBoxOriginalScript.Text = line.Trim().Remove(0, 16).Trim();
                }
                else if (s.StartsWith("original translation:", StringComparison.Ordinal))
                {
                    textBoxTranslation.Text = line.Trim().Remove(0, 21).Trim();
                }
                else if (s.StartsWith("original editing:", StringComparison.Ordinal))
                {
                    textBoxEditing.Text = line.Trim().Remove(0, 17).Trim();
                }
                else if (s.StartsWith("original timing:", StringComparison.Ordinal))
                {
                    textBoxTiming.Text = line.Trim().Remove(0, 16).Trim();
                }
                else if (s.StartsWith("synch point:", StringComparison.Ordinal))
                {
                    textBoxSyncPoint.Text = line.Trim().Remove(0, 12).Trim();
                }
                else if (s.StartsWith("script updated by:", StringComparison.Ordinal))
                {
                    textBoxUpdatedBy.Text = line.Trim().Remove(0, 18).Trim();
                }
                else if (s.StartsWith("update details:", StringComparison.Ordinal))
                {
                    textBoxUpdateDetails.Text = line.Trim().Remove(0, 15).Trim();
                }
                else if (s.StartsWith("playresx:", StringComparison.Ordinal))
                {
                    if (int.TryParse(line.Trim().Remove(0, 9).Trim(), out var number))
                    {
                        numericUpDownVideoWidth.Value = number;
                    }
                }
                else if (s.StartsWith("playresy:", StringComparison.Ordinal))
                {
                    if (int.TryParse(line.Trim().Remove(0, 9).Trim(), out var number))
                    {
                        numericUpDownVideoHeight.Value = number;
                    }
                }
                else if (s.StartsWith("scaledborderandshadow:", StringComparison.Ordinal))
                {
                    checkBoxScaleBorderAndShadow.Checked = line.Trim().Remove(0, 22).Trim().ToLowerInvariant().Equals("yes");
                }
                else if (s.StartsWith("wrapstyle:", StringComparison.Ordinal))
                {
                    var wrapStyle = line.Trim().Remove(0, 10).Trim();
                    for (int i = 0; i < comboBoxWrapStyle.Items.Count; i++)
                    {
                        if (i.ToString(CultureInfo.InvariantCulture) == wrapStyle)
                        {
                            comboBoxWrapStyle.SelectedIndex = i;
                        }
                    }
                }
            }

            groupBoxScript.Text = l.Script;
            labelTitle.Text = l.ScriptTitle;
            labelOriginalScript.Text = l.OriginalScript;
            labelTranslation.Text = l.Translation;
            labelEditing.Text = l.Editing;
            labelTiming.Text = l.Timing;
            labelSyncPoint.Text = l.SyncPoint;
            labelUpdatedBy.Text = l.UpdatedBy;
            labelUpdateDetails.Text = l.UpdateDetails;
            groupBoxResolution.Text = l.Resolution;
            labelVideoResolution.Text = l.VideoResolution;
            buttonGetResolutionFromCurrentVideo.Text = l.FromCurrentVideo;
            groupBoxOptions.Text = l.Options;
            labelWrapStyle.Text = l.WrapStyle;
            checkBoxScaleBorderAndShadow.Text = l.ScaleBorderAndShadow;

            comboBoxWrapStyle.Left = labelWrapStyle.Right + 10;
            comboBoxWrapStyle.Width = groupBoxOptions.Right - comboBoxWrapStyle.Left - 25;
            checkBoxScaleBorderAndShadow.Left = labelWrapStyle.Right + 10;

            comboBoxWrapStyle.Items.Clear();
            comboBoxWrapStyle.Items.Add(l.WrapStyle0);
            comboBoxWrapStyle.Items.Add(l.WrapStyle1);
            comboBoxWrapStyle.Items.Add(l.WrapStyle2);
            comboBoxWrapStyle.Items.Add(l.WrapStyle3);

            buttonOK.Text = LanguageSettings.Current.General.Ok;
            buttonCancel.Text = LanguageSettings.Current.General.Cancel;

            UiUtil.FixLargeFonts(this, buttonCancel);

            buttonGetResolutionFromCurrentVideo.Enabled = !string.IsNullOrEmpty(videoFileName);
            Height = _height;
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private string GetDefaultHeader()
        {
            SubtitleFormat format = new AdvancedSubStationAlpha();
            var sub = new Subtitle();
            string text = format.ToText(sub, string.Empty);
            var lines = text.SplitToLines();
            format.LoadSubtitle(sub, lines, string.Empty);
            return sub.Header.Trim();
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            if (_subtitle.Header == null || !_subtitle.Header.Contains("[script info]", StringComparison.OrdinalIgnoreCase))
            {
                _subtitle.Header = GetDefaultHeader();
            }

            string title = textBoxTitle.Text;
            if (string.IsNullOrWhiteSpace(title))
            {
                title = "untitled";
            }

            UpdateTag("Title", title, false);
            UpdateTag("Original Script", textBoxOriginalScript.Text, string.IsNullOrWhiteSpace(textBoxOriginalScript.Text));
            UpdateTag("Original Translation", textBoxTranslation.Text, string.IsNullOrWhiteSpace(textBoxTranslation.Text));
            UpdateTag("Original Editing", textBoxEditing.Text, string.IsNullOrWhiteSpace(textBoxEditing.Text));
            UpdateTag("Original Timing", textBoxTiming.Text, string.IsNullOrWhiteSpace(textBoxTiming.Text));
            UpdateTag("Synch Point", textBoxSyncPoint.Text, string.IsNullOrWhiteSpace(textBoxSyncPoint.Text));
            UpdateTag("Script Updated By", textBoxUpdatedBy.Text, string.IsNullOrWhiteSpace(textBoxUpdatedBy.Text));
            UpdateTag("Update Details", textBoxUpdateDetails.Text, string.IsNullOrWhiteSpace(textBoxUpdateDetails.Text));
            UpdateTag("PlayResX", numericUpDownVideoWidth.Value.ToString(CultureInfo.InvariantCulture), numericUpDownVideoWidth.Value == 0);
            UpdateTag("PlayResY", numericUpDownVideoHeight.Value.ToString(CultureInfo.InvariantCulture), numericUpDownVideoHeight.Value == 0);

            UpdateTag("WrapStyle", comboBoxWrapStyle.SelectedIndex.ToString(CultureInfo.InvariantCulture), false);
            if (checkBoxScaleBorderAndShadow.Checked)
            {
                UpdateTag("ScaledBorderAndShadow", "Yes", false);
            }
            else
            {
                UpdateTag("ScaledBorderAndShadow", "No", false);
            }

            DialogResult = DialogResult.OK;
        }

        private void UpdateTag(string tag, string text, bool remove)
        {
            bool scriptInfoOn = false;
            var sb = new StringBuilder();
            bool found = false;
            foreach (string line in _subtitle.Header.SplitToLines())
            {
                if (line.StartsWith("[script info]", StringComparison.OrdinalIgnoreCase))
                {
                    scriptInfoOn = true;
                }
                else if (line.StartsWith('['))
                {
                    if (!found && scriptInfoOn && !remove)
                    {
                        sb = new StringBuilder(sb.ToString().Trim() + Environment.NewLine);
                        sb.AppendLine(tag + ": " + text);
                    }
                    sb = new StringBuilder(sb.ToString().TrimEnd());
                    sb.AppendLine();
                    sb.AppendLine();
                    scriptInfoOn = false;
                }

                string s = line.ToLowerInvariant();
                if (s.StartsWith(tag.ToLowerInvariant() + ":", StringComparison.Ordinal))
                {
                    if (!remove)
                    {
                        sb.AppendLine(line.Substring(0, tag.Length) + ": " + text);
                    }

                    found = true;
                }
                else
                {
                    sb.AppendLine(line);
                }
            }
            _subtitle.Header = sb.ToString().Trim();
        }

        private void SubStationAlphaProperties_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                DialogResult = DialogResult.Cancel;
            }
        }

        private void buttonGetResolutionFromVideo_Click(object sender, EventArgs e)
        {
            openFileDialog1.Title = LanguageSettings.Current.General.OpenVideoFileTitle;
            openFileDialog1.FileName = string.Empty;
            openFileDialog1.Filter = UiUtil.GetVideoFileFilter(false);
            openFileDialog1.FileName = string.Empty;
            if (string.IsNullOrEmpty(openFileDialog1.InitialDirectory) && !string.IsNullOrEmpty(_videoFileName))
            {
                openFileDialog1.InitialDirectory = Path.GetDirectoryName(_videoFileName);
                openFileDialog1.FileName = Path.GetFileName(_videoFileName);
            }

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                VideoInfo info = UiUtil.GetVideoInfo(openFileDialog1.FileName);
                if (info != null && info.Success)
                {
                    numericUpDownVideoWidth.Value = info.Width;
                    numericUpDownVideoHeight.Value = info.Height;
                }
            }
        }

        private void buttonGetResolutionFromCurrentVideo_Click(object sender, EventArgs e)
        {
            if (_currentVideoInfo is null)
            {
                return;
            }

            numericUpDownVideoWidth.Value = _currentVideoInfo.Width;
            numericUpDownVideoHeight.Value = _currentVideoInfo.Height;
        }

        private void SubStationAlphaProperties_Shown(object sender, EventArgs e)
        {
            Height = _height;
        }
    }
}
