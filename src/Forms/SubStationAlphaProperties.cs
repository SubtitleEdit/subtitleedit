using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.SubtitleFormats;

namespace Nikse.SubtitleEdit.Forms
{
    public sealed partial class SubStationAlphaProperties : Form
    {
        private Subtitle _subtitle;
        private bool _isSubStationAlpha;
        private string _videoFileName;

        public SubStationAlphaProperties(Subtitle subtitle, SubtitleFormat format, string videoFileName)
        {
            InitializeComponent();
            _subtitle = subtitle;
            _isSubStationAlpha = format.FriendlyName == new SubStationAlpha().FriendlyName;
            _videoFileName = videoFileName;

            var l = Configuration.Settings.Language.SubStationAlphaProperties;
            if (_isSubStationAlpha)
            {
                Text = l.TitleSubstationAlpha;
                labelWrapStyle.Visible = false;
                comboBoxWrapStyle.Visible = false;
                checkBoxScaleBorderAndShadow.Visible = false;
                Height = Height - (comboBoxWrapStyle.Height + checkBoxScaleBorderAndShadow.Height + 8);
            }
            else
            {
                Text = l.Title;
            }
            groupBoxScript.Text = l.Script;
            labelTitle.Text = l.ScriptTitle;

            comboBoxWrapStyle.SelectedIndex = 2;
            comboBoxCollision.SelectedIndex = 0;
            if (subtitle.Header != null)
            {
                foreach (string line in subtitle.Header.Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries))
                {
                    string s = line.ToLower();
                    if (s.StartsWith("title:"))
                    {
                        textBoxTitle.Text = s.Remove(0, 6).Trim();
                    }
                    else if (s.StartsWith("original script:"))
                    {
                        textBoxOriginalScript.Text = s.Remove(0, 16).Trim();
                    }
                    else if (s.StartsWith("original translation:"))
                    {
                        textBoxTranslation.Text = s.Remove(0, 21).Trim();
                    }
                    else if (s.StartsWith("original editing:"))
                    {
                        textBoxEditing.Text = s.Remove(0, 17).Trim();
                    }
                    else if (s.StartsWith("original timing:"))
                    {
                        textBoxTiming.Text = s.Remove(0, 16).Trim();
                    }
                    else if (s.StartsWith("synch point:"))
                    {
                        textBoxSyncPoint.Text = s.Remove(0, 12).Trim();
                    }
                    else if (s.StartsWith("script updated by:"))
                    {
                        textBoxUpdatedBy.Text = s.Remove(0, 18).Trim();
                    }
                    else if (s.StartsWith("update details:"))
                    {
                        textBoxUpdateDetails.Text = s.Remove(0, 15).Trim();
                    }
                    else if (s.StartsWith("collisions:"))
                    {
                        if (s.Remove(0, 11).Trim() == "reverse")
                            comboBoxCollision.SelectedIndex = 1;
                    }
                    else if (s.StartsWith("playresx:"))
                    {
                        int number;
                        if (int.TryParse(s.Remove(0, 9).Trim(), out number))
                            numericUpDownVideoWidth.Value = number;
                    }
                    else if (s.StartsWith("playresy:"))
                    {
                        int number;
                        if (int.TryParse(s.Remove(0, 9).Trim(), out number))
                            numericUpDownVideoHeight.Value = number;
                    }
                    else if (s.StartsWith("scaledborderandshadow:"))
                    {
                        checkBoxScaleBorderAndShadow.Checked = s.Remove(0, 22).Trim().ToLower() == "yes";
                    }
                }
            }
            buttonOK.Text = Configuration.Settings.Language.General.OK;
            buttonCancel.Text = Configuration.Settings.Language.General.Cancel;

            FixLargeFonts();
        }

        private void FixLargeFonts()
        {
            Graphics graphics = CreateGraphics();
            SizeF textSize = graphics.MeasureString(buttonCancel.Text, Font);
            if (textSize.Height > buttonCancel.Height - 4)
            {
                int newButtonHeight = (int)(textSize.Height + 7 + 0.5);
                Utilities.SetButtonHeight(this, newButtonHeight, 1);
            }
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private string GetDefaultHeader()
        {
            SubtitleFormat format;
            if (_isSubStationAlpha)
                format = new SubStationAlpha();
            else
                format = new AdvancedSubStationAlpha();
            var sub = new Subtitle();
            string text = format.ToText(sub, string.Empty);
            string[] lineArray = text.Split(Environment.NewLine.ToCharArray());
            var lines = new List<string>();
            foreach (string line in lineArray)
                lines.Add(line);
            format.LoadSubtitle(sub, lines, string.Empty);
            return sub.Header.Trim();
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            if (_subtitle.Header == null || !_subtitle.Header.ToLower().Contains("[script info]"))
                _subtitle.Header = GetDefaultHeader();

            string title = textBoxTitle.Text;
            if (title.Trim().Length == 0)
                title = "untitled";
            UpdateTag("Title", title);
            UpdateTag("Original Script", textBoxTitle.Text);
            UpdateTag("Original Translation", textBoxTranslation.Text);
            UpdateTag("Original Editing", textBoxEditing.Text);
            UpdateTag("Original Timing", textBoxTiming.Text);
            UpdateTag("Synch Point", textBoxSyncPoint.Text);
            UpdateTag("Script Updated By", textBoxUpdatedBy.Text);
            UpdateTag("Update Details", textBoxUpdateDetails.Text);
            UpdateTag("PlayResX", numericUpDownVideoWidth.Value.ToString());
            UpdateTag("PlayResY", numericUpDownVideoHeight.Value.ToString());
            if (comboBoxCollision.SelectedIndex == 0)
                UpdateTag("collisions", "Normal"); // normal
            else
                UpdateTag("collisions", "Reverse"); // reverse

            if (!_isSubStationAlpha)
            {
                UpdateTag("wrapstyle", comboBoxWrapStyle.SelectedIndex.ToString());
                if (checkBoxScaleBorderAndShadow.Checked)
                    UpdateTag("ScaledBorderAndShadow", "yes");
                else
                    UpdateTag("ScaledBorderAndShadow", "no"); // no
            }

            DialogResult = DialogResult.OK;
        }

        private void UpdateTag(string tag, string text)
        {
            bool scriptInfoOn = false;
            var sb = new StringBuilder();
            bool found = false;
            foreach (string line in _subtitle.Header.Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries))
            {
                if (line.ToLower().StartsWith("[script info]"))
                {
                    scriptInfoOn = true;
                }
                else if (line.StartsWith("["))
                {
                    if (!found && scriptInfoOn)
                        sb.AppendLine(tag + ": " + text);
                    sb.AppendLine();
                    scriptInfoOn = false;
                }

                string s = line.ToLower();
                if (s.StartsWith(tag.ToLower() + ":"))
                {
                    if (text.Length > 0)
                        sb.AppendLine(line.Substring(0, tag.Length) + ": " + text);
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
                DialogResult = DialogResult.Cancel;
        }

        private void buttonGetResolutionFromVideo_Click(object sender, EventArgs e)
        {
            openFileDialog1.Title = Configuration.Settings.Language.General.OpenVideoFileTitle;
            openFileDialog1.FileName = string.Empty;
            openFileDialog1.Filter = Utilities.GetVideoFileFilter();
            openFileDialog1.FileName = string.Empty;
            if (string.IsNullOrEmpty(openFileDialog1.InitialDirectory) && !string.IsNullOrEmpty(_videoFileName))
            {
                openFileDialog1.InitialDirectory = System.IO.Path.GetDirectoryName(_videoFileName);
                openFileDialog1.FileName = System.IO.Path.GetFileName(_videoFileName);
            }

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                VideoInfo info = Utilities.GetVideoInfo(openFileDialog1.FileName, delegate { Application.DoEvents(); });
                if (info != null && info.Success)
                {
                    numericUpDownVideoWidth.Value = info.Width;
                    numericUpDownVideoHeight.Value = info.Height;
                }
            }
        }

    }
}
