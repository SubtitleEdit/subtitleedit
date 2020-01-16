using Nikse.SubtitleEdit.Core;
using Nikse.SubtitleEdit.Logic;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.Forms
{
    public sealed partial class ApplyDurationLimits : PositionAndSizeForm
    {
        private int _totalFixes;
        private int _totalErrors;
        private Subtitle _subtitle;
        private Subtitle _working;
        private bool _onlyListFixes = true;
        private readonly Timer _refreshTimer = new Timer();
        private readonly Color _warningColor = Color.FromArgb(255, 253, 145);
        private Subtitle _unfixables = new Subtitle();

        public ApplyDurationLimits()
        {
            UiUtil.PreInitialize(this);
            InitializeComponent();
            UiUtil.FixFonts(this);
            Text = Configuration.Settings.Language.ApplyDurationLimits.Title;
            checkBoxMinDuration.Text = Configuration.Settings.Language.Settings.DurationMinimumMilliseconds;
            checkBoxMaxDuration.Text = Configuration.Settings.Language.Settings.DurationMaximumMilliseconds;
            checkBoxMinDuration.Checked = Configuration.Settings.Tools.ApplyMinimumDurationLimit;
            checkBoxMaxDuration.Checked = Configuration.Settings.Tools.ApplyMaximumDurationLimit;
            labelNote.Text = Configuration.Settings.Language.AdjustDisplayDuration.Note;
            numericUpDownDurationMin.Value = Configuration.Settings.General.SubtitleMinimumDisplayMilliseconds;
            numericUpDownDurationMax.Value = Configuration.Settings.General.SubtitleMaximumDisplayMilliseconds;
            buttonOK.Text = Configuration.Settings.Language.General.Ok;
            buttonCancel.Text = Configuration.Settings.Language.General.Cancel;
            subtitleListView1.InitializeLanguage(Configuration.Settings.Language.General, Configuration.Settings);
            UiUtil.InitializeSubtitleFont(subtitleListView1);
            subtitleListView1.AutoSizeAllColumns(this);
            listViewFixes.Columns[0].Text = Configuration.Settings.Language.General.Apply;
            listViewFixes.Columns[1].Text = Configuration.Settings.Language.General.LineNumber;
            listViewFixes.Columns[2].Text = Configuration.Settings.Language.General.Before;
            listViewFixes.Columns[3].Text = Configuration.Settings.Language.General.After;
            numericUpDownDurationMin.Left = checkBoxMinDuration.Left + checkBoxMinDuration.Width + 6;
            numericUpDownDurationMax.Left = checkBoxMaxDuration.Left + checkBoxMaxDuration.Width + 6;
            if (Math.Abs(numericUpDownDurationMin.Left - numericUpDownDurationMax.Left) < 10)
            {
                numericUpDownDurationMin.Left = Math.Max(numericUpDownDurationMin.Left, numericUpDownDurationMax.Left);
                numericUpDownDurationMax.Left = Math.Max(numericUpDownDurationMin.Left, numericUpDownDurationMax.Left);
            }
            FixLargeFonts();
            _refreshTimer.Interval = 400;
            _refreshTimer.Tick += RefreshTimerTick;
            UiUtil.FixLargeFonts(this, buttonOK);
        }

        private void RefreshTimerTick(object sender, EventArgs e)
        {
            _refreshTimer.Stop();
            GeneratePreviewReal();
        }

        private void FixLargeFonts()
        {
            if (labelNote.Right + 5 > Width)
            {
                Width = labelNote.Right + 5;
            }

            UiUtil.FixLargeFonts(this, buttonOK);
        }

        public void Initialize(Subtitle subtitle)
        {
            _subtitle = subtitle;
            GeneratePreview();
        }

        private void GeneratePreview()
        {
            if (_refreshTimer.Enabled)
            {
                _refreshTimer.Stop();
            }
            _refreshTimer.Start();
        }

        private void GeneratePreviewReal()
        {
            _totalFixes = 0;
            _totalErrors = 0;
            _onlyListFixes = true;

            _working = new Subtitle(_subtitle);
            listViewFixes.BeginUpdate();
            listViewFixes.Items.Clear();
            if (checkBoxMinDuration.Checked)
            {
                FixShortDisplayTimes();
            }
            if (checkBoxMaxDuration.Checked)
            {
                FixLongDisplayTimes();
            }
            listViewFixes.EndUpdate();

            groupBoxFixesAvailable.Text = string.Format(Configuration.Settings.Language.ApplyDurationLimits.FixesAvailable, _totalFixes);
            groupBoxUnfixable.Text = string.Format(Configuration.Settings.Language.ApplyDurationLimits.UnableToFix, _totalErrors);
        }

        private void AddFixToListView(Paragraph p, string before, string after, Color backgroundColor)
        {
            if (_onlyListFixes)
            {
                var item = new ListViewItem(string.Empty) { Checked = true, Tag = p, BackColor = backgroundColor };
                item.SubItems.Add(p.Number.ToString());
                item.SubItems.Add(UiUtil.GetListViewTextFromString(before));
                item.SubItems.Add(UiUtil.GetListViewTextFromString(after));
                listViewFixes.Items.Add(item);
            }
        }

        public bool AllowFix(Paragraph p)
        {
            if (_onlyListFixes)
            {
                return true;
            }

            string ln = p.Number.ToString();
            foreach (ListViewItem item in listViewFixes.Items)
            {
                if (item.SubItems[1].Text == ln)
                {
                    return item.Checked;
                }
            }
            return false;
        }

        private void FixShortDisplayTimes()
        {
            _unfixables = new Subtitle();
            double minDisplayTime = (double)numericUpDownDurationMin.Value;
            for (int i = 0; i < _working.Paragraphs.Count; i++)
            {
                Paragraph p = _working.Paragraphs[i];
                double displayTime = p.Duration.TotalMilliseconds;
                if (displayTime < minDisplayTime)
                {
                    var next = _working.GetParagraphOrDefault(i + 1);
                    var wantedEndMs = p.StartTime.TotalMilliseconds + minDisplayTime;
                    if (next == null || wantedEndMs < next.StartTime.TotalMilliseconds - Configuration.Settings.General.MinimumMillisecondsBetweenLines && AllowFix(p))
                    {
                        AddFix(p, wantedEndMs, DefaultBackColor);
                    }
                    else
                    {
                        var nextBestEndMs = next.StartTime.TotalMilliseconds - Configuration.Settings.General.MinimumMillisecondsBetweenLines;
                        if (nextBestEndMs > p.EndTime.TotalMilliseconds)
                        {
                            AddFix(p, nextBestEndMs, _warningColor);
                            _unfixables.Paragraphs.Add(new Paragraph(p) { Extra = "Warning" });
                        }
                        else
                        {
                            _unfixables.Paragraphs.Add(new Paragraph(p));
                        }
                        _totalErrors++;
                    }
                }
            }
            subtitleListView1.Fill(_unfixables);
            for (int index = 0; index < _unfixables.Paragraphs.Count; index++)
            {
                var p = _unfixables.Paragraphs[index];
                subtitleListView1.SetBackgroundColor(index, p.Extra == "Warning" ? _warningColor : Configuration.Settings.Tools.ListViewSyntaxErrorColor);
            }
        }

        private void AddFix(Paragraph p, double endMs, Color backgroundColor)
        {
            string before = p.StartTime.ToShortString() + " --> " + p.EndTime.ToShortString() + " - " + p.Duration.ToShortString();
            p.EndTime.TotalMilliseconds = endMs;
            string after = p.StartTime.ToShortString() + " --> " + p.EndTime.ToShortString() + " - " + p.Duration.ToShortString();
            _totalFixes++;
            AddFixToListView(p, before, after, backgroundColor);
        }

        public void FixLongDisplayTimes()
        {
            double maxDisplayTime = (double)numericUpDownDurationMax.Value;
            for (int i = 0; i < _working.Paragraphs.Count; i++)
            {
                Paragraph p = _working.Paragraphs[i];
                double displayTime = p.Duration.TotalMilliseconds;
                if (displayTime > maxDisplayTime && AllowFix(p))
                {
                    AddFix(p, p.StartTime.TotalMilliseconds + maxDisplayTime, DefaultBackColor);
                }
            }
        }

        private void ApplyDurationLimits_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                DialogResult = DialogResult.Cancel;
            }
        }

        private void numericUpDownDurationMin_ValueChanged(object sender, EventArgs e)
        {
            GeneratePreview();
        }

        private void numericUpDownDurationMax_ValueChanged(object sender, EventArgs e)
        {
            GeneratePreview();
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            Configuration.Settings.Tools.ApplyMinimumDurationLimit = checkBoxMinDuration.Checked;
            Configuration.Settings.Tools.ApplyMaximumDurationLimit = checkBoxMaxDuration.Checked;

            _onlyListFixes = false;
            _working = new Subtitle(_subtitle);
            if (checkBoxMinDuration.Checked)
            {
                FixShortDisplayTimes();
            }
            if (checkBoxMaxDuration.Checked)
            {
                FixLongDisplayTimes();
            }
            DialogResult = DialogResult.OK;
        }

        public Subtitle FixedSubtitle => _working;

        private void numericUpDownDurationMin_KeyUp(object sender, KeyEventArgs e)
        {
            GeneratePreview();
        }

        private void numericUpDownDurationMax_KeyUp(object sender, KeyEventArgs e)
        {
            GeneratePreview();
        }

        private void numericUpDownDurationMin_MouseUp(object sender, MouseEventArgs e)
        {
            GeneratePreview();
        }

        private void numericUpDownDurationMax_MouseUp(object sender, MouseEventArgs e)
        {
            GeneratePreview();
        }

        private void ApplyDurationLimits_Shown(object sender, EventArgs e)
        {
            listViewFixes.Focus();
        }

        private void checkBoxMinDuration_CheckedChanged(object sender, EventArgs e)
        {
            numericUpDownDurationMin.Enabled = checkBoxMinDuration.Checked;
            GeneratePreview();

        }

        private void checkBoxMaxDuration_CheckedChanged(object sender, EventArgs e)
        {
            numericUpDownDurationMax.Enabled = checkBoxMaxDuration.Checked;
            GeneratePreview();
        }

        private void listViewFixes_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listViewFixes.SelectedIndices.Count > 0)
            {
                int index = listViewFixes.SelectedIndices[0];
                ListViewItem item = listViewFixes.Items[index];
                var number = int.Parse(item.SubItems[1].Text);
                foreach (var p in _unfixables.Paragraphs)
                {
                    if (p.Number == number)
                    {
                        index = _unfixables.GetIndex(p);
                        subtitleListView1.SelectIndexAndEnsureVisible(index);
                        return;
                    }
                }
            }
        }
    }
}
