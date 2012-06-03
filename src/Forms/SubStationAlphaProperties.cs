using System;
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

        public SubStationAlphaProperties(Subtitle subtitle, SubtitleFormat format)
        {
            InitializeComponent();
            _subtitle = subtitle;
            _isSubStationAlpha = format.FriendlyName == new SubStationAlpha().FriendlyName;


            var l = Configuration.Settings.Language.SubStationAlphaProperties;
            if (_isSubStationAlpha)
                Text = l.TitleSubstationAlpha;
            else
                Text = l.Title;
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
                        textBoxUpdateDetails.Text = s.Remove(0, 18).Trim();
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
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            UpdateTag("Title", textBoxTitle.Text);
            UpdateTag("Original Script", textBoxTitle.Text);
            UpdateTag("Original Translation", textBoxTranslation.Text);
            UpdateTag("Original Editing", textBoxEditing.Text);
            UpdateTag("Original Timing", textBoxTiming.Text);
            UpdateTag("Synch Point", textBoxSyncPoint.Text);
            UpdateTag("Script Updated By", textBoxUpdatedBy.Text);
            UpdateTag("Update Details", textBoxUpdateDetails.Text);
            UpdateTag("PlayResX", numericUpDownVideoWidth.Value.ToString());
            UpdateTag("PlayResY", numericUpDownVideoHeight.Value.ToString());
            UpdateTag("wrapstyle", comboBoxWrapStyle.SelectedIndex.ToString());
            if (comboBoxCollision.SelectedIndex == 0)
                UpdateTag("collisions", "normal");
            else
                UpdateTag("collisions", "reverse");
            if (checkBoxScaleBorderAndShadow.Checked)
                UpdateTag("ScaledBorderAndShadow", "yes");
            else
                UpdateTag("ScaledBorderAndShadow", "no");
            DialogResult = DialogResult.OK;
        }

        private void UpdateTag(string tag, string text)
        {
            if (_subtitle.Header == null)
            {
            }

            var sb = new StringBuilder();
            foreach (string line in _subtitle.Header.Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries))
            { 
                string s = line.ToLower();
                if (s.StartsWith(tag.ToLower() + ":"))
                {
                    sb.AppendLine(line.Substring(0, tag.Length) + ": " + text);
                }
                else
                {
                    sb.AppendLine(line);
                }
            }
            _subtitle.Header = sb.ToString();
        }

        private void SubStationAlphaProperties_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
                DialogResult = DialogResult.Cancel;
        }
    }
}
