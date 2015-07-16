using System;
using System.Windows.Forms;
using Nikse.SubtitleEdit.Logic;

namespace Nikse.SubtitleEdit.Forms
{
    public partial class ApplyDurationLimits : PositionAndSizeForm
    {
        private int _totalFixes;
        private int _totalErrors;
        private Subtitle _subtitle;
        private Subtitle _working;
        private bool _onlyListFixes = true;
        private readonly Timer _refreshTimer = new Timer();

        public ApplyDurationLimits()
        {
            InitializeComponent();
            Text = Configuration.Settings.Language.ApplyDurationLimits.Title;
            labelMinDuration.Text = Configuration.Settings.Language.Settings.DurationMinimumMilliseconds;
            labelMaxDuration.Text = Configuration.Settings.Language.Settings.DurationMaximumMilliseconds;
            labelNote.Text = Configuration.Settings.Language.AdjustDisplayDuration.Note;
            numericUpDownDurationMin.Value = Configuration.Settings.General.SubtitleMinimumDisplayMilliseconds;
            numericUpDownDurationMax.Value = Configuration.Settings.General.SubtitleMaximumDisplayMilliseconds;
            buttonOK.Text = Configuration.Settings.Language.General.Ok;
            buttonCancel.Text = Configuration.Settings.Language.General.Cancel;
            subtitleListView1.InitializeLanguage(Configuration.Settings.Language.General, Configuration.Settings);
            listViewFixes.Columns[0].Text = Configuration.Settings.Language.General.Apply;
            listViewFixes.Columns[1].Text = Configuration.Settings.Language.General.LineNumber;
            listViewFixes.Columns[2].Text = Configuration.Settings.Language.General.Before;
            listViewFixes.Columns[3].Text = Configuration.Settings.Language.General.After;
            FixLargeFonts();
            _refreshTimer.Interval = 400;
            _refreshTimer.Tick += RefreshTimerTick;
        }

        private void RefreshTimerTick(object sender, EventArgs e)
        {
            _refreshTimer.Stop();
            GeneratePreviewReal();
        }

        private void FixLargeFonts()
        {
            if (labelNote.Left + labelNote.Width + 5 > Width)
                Width = labelNote.Left + labelNote.Width + 5;
            Utilities.FixLargeFonts(this, buttonOK);
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
                _refreshTimer.Start();
            }
            else
            {
                _refreshTimer.Start();
            }
        }

        private void GeneratePreviewReal()
        {
            _totalFixes = 0;
            _totalErrors = 0;
            _onlyListFixes = true;

            _working = new Subtitle(_subtitle);
            listViewFixes.BeginUpdate();
            listViewFixes.Items.Clear();
            FixShortDisplayTimes();
            FixLongDisplayTimes();
            listViewFixes.EndUpdate();

            groupBoxFixesAvailable.Text = string.Format(Configuration.Settings.Language.ApplyDurationLimits.FixesAvailable, _totalFixes);
            groupBoxUnfixable.Text = string.Format(Configuration.Settings.Language.ApplyDurationLimits.UnableToFix, _totalErrors);
        }

        private void AddFixToListView(Paragraph p, string before, string after)
        {
            if (_onlyListFixes)
            {
                var item = new ListViewItem(string.Empty) { Checked = true, Tag = p };
                item.SubItems.Add(p.Number.ToString());
                item.SubItems.Add(before.Replace(Environment.NewLine, Configuration.Settings.General.ListViewLineSeparatorString));
                item.SubItems.Add(after.Replace(Environment.NewLine, Configuration.Settings.General.ListViewLineSeparatorString));
                listViewFixes.Items.Add(item);
            }
        }

        public bool AllowFix(Paragraph p)
        {
            if (_onlyListFixes)
                return true;

            string ln = p.Number.ToString();
            foreach (ListViewItem item in listViewFixes.Items)
            {
                if (item.SubItems[1].Text == ln)
                    return item.Checked;
            }
            return false;
        }

        private void FixShortDisplayTimes()
        {
            Subtitle unfixables = new Subtitle();
            string fixAction = Configuration.Settings.Language.FixCommonErrors.FixShortDisplayTime;
            for (int i = 0; i < _working.Paragraphs.Count; i++)
            {
                Paragraph p = _working.Paragraphs[i];

                double minDisplayTime = (double)numericUpDownDurationMin.Value;
                //var minCharSecMs = Utilities.GetOptimalDisplayMilliseconds(p.Text, Configuration.Settings.General.SubtitleMaximumCharactersPerSeconds);
                //if (minCharSecMs > minDisplayTime)
                //    minDisplayTime = minCharSecMs;

                double displayTime = p.Duration.TotalMilliseconds;
                if (displayTime < minDisplayTime)
                {
                    Paragraph next = _working.GetParagraphOrDefault(i + 1);
                    if (next == null || (p.StartTime.TotalMilliseconds + minDisplayTime < next.StartTime.TotalMilliseconds) && AllowFix(p))
                    {
                        string before = p.StartTime.ToShortString() + " --> " + p.EndTime.ToShortString() + " - " + p.Duration.ToShortString();
                        p.EndTime.TotalMilliseconds = p.StartTime.TotalMilliseconds + minDisplayTime;

                        string after = p.StartTime.ToShortString() + " --> " + p.EndTime.ToShortString() + " - " + p.Duration.ToShortString();
                        _totalFixes++;
                        AddFixToListView(p, before, after);
                    }
                    else
                    {
                        unfixables.Paragraphs.Add(new Paragraph(p));
                        _totalErrors++;
                    }
                }
            }
            subtitleListView1.Fill(unfixables);
        }

        public void FixLongDisplayTimes()
        {
            string fixAction = Configuration.Settings.Language.FixCommonErrors.FixLongDisplayTime;
            for (int i = 0; i < _working.Paragraphs.Count; i++)
            {
                Paragraph p = _working.Paragraphs[i];
                double displayTime = p.Duration.TotalMilliseconds;
                double maxDisplayTime = (double)numericUpDownDurationMax.Value;
                if (displayTime > maxDisplayTime && AllowFix(p))
                {
                    string before = p.StartTime.ToShortString() + " --> " + p.EndTime.ToShortString() + " - " + p.Duration.ToShortString();
                    p.EndTime.TotalMilliseconds = p.StartTime.TotalMilliseconds + maxDisplayTime;
                    string after = p.StartTime.ToShortString() + " --> " + p.EndTime.ToShortString() + " - " + p.Duration.ToShortString();
                    _totalFixes++;
                    AddFixToListView(p, before, after);
                }
            }
        }

        private void ApplyDurationLimits_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
                DialogResult = DialogResult.Cancel;
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
            _onlyListFixes = false;
            _working = new Subtitle(_subtitle);
            FixShortDisplayTimes();
            FixLongDisplayTimes();
            DialogResult = DialogResult.OK;
        }

        public Subtitle FixedSubtitle
        {
            get { return _working; }
        }
    }
}