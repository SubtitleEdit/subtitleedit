using System.Drawing;
using System.Windows.Forms;
using Nikse.SubtitleEdit.Logic;
using System;

namespace Nikse.SubtitleEdit.Forms
{
    public partial class ApplyDurationLimits : Form
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
            numericUpDownDurationMin.Value = Configuration.Settings.General.SubtitleMinimumDisplayMilliseconds;
            numericUpDownDurationMax.Value = Configuration.Settings.General.SubtitleMaximumDisplayMilliseconds;
            buttonOK.Text = Configuration.Settings.Language.General.OK;
            buttonCancel.Text = Configuration.Settings.Language.General.Cancel;
            subtitleListView1.InitializeLanguage(Configuration.Settings.Language.General, Configuration.Settings);
            FixLargeFonts();
            _refreshTimer.Interval = 400;
            _refreshTimer.Tick += RefreshTimerTick;
        }

        void RefreshTimerTick(object sender, EventArgs e)
        {
            _refreshTimer.Stop();
            GeneratePreviewReal();
        }

        private void FixLargeFonts()
        {
            if (labelNote.Left + labelNote.Width + 5 > Width)
                Width = labelNote.Left + labelNote.Width + 5;

            Graphics graphics = CreateGraphics();
            SizeF textSize = graphics.MeasureString(buttonOK.Text, this.Font);
            if (textSize.Height > buttonOK.Height - 4)
            {
                int newButtonHeight = (int)(textSize.Height + 7 + 0.5);
                Utilities.SetButtonHeight(this, newButtonHeight, 1);
            }
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
            FixShortDisplayTimes();
            FixLongDisplayTimes();

            groupBoxFixesAvailable.Text = string.Format("Fixes available: {0}", _totalFixes);
            groupBoxUnfixable.Text = string.Format("Unable to fix: {0}", _totalErrors);
        }

        private void AddFixToListView(Paragraph p, string before, string after)
        {
            if (_onlyListFixes)
            {
                var item = new ListViewItem(string.Empty) { Checked = true };

                var subItem = new ListViewItem.ListViewSubItem(item, p.Number.ToString());
                item.SubItems.Add(subItem);
                subItem = new ListViewItem.ListViewSubItem(item, before.Replace(Environment.NewLine, Configuration.Settings.General.ListViewLineSeparatorString));
                item.SubItems.Add(subItem);
                subItem = new ListViewItem.ListViewSubItem(item, after.Replace(Environment.NewLine, Configuration.Settings.General.ListViewLineSeparatorString));
                item.SubItems.Add(subItem);

                item.Tag = p; // save paragraph in Tag

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
            int noOfShortDisplayTimes = 0;
            for (int i = 0; i < _working.Paragraphs.Count; i++)
            {
                Paragraph p = _working.Paragraphs[i];
                double minDisplayTime = (double)numericUpDownDurationMin.Value;
                double displayTime = p.Duration.TotalMilliseconds;
                if (displayTime < minDisplayTime)
                {
                    Paragraph next = _working.GetParagraphOrDefault(i + 1);
                    if (next == null || (p.StartTime.TotalMilliseconds + Utilities.GetDisplayMillisecondsFromText(p.Text)) < next.StartTime.TotalMilliseconds)
                    {
                        if (AllowFix(p))
                        {
                            string before = p.StartTime.ToShortString() + " --> " + p.EndTime.ToShortString() + " - " + p.Duration.ToShortString();
                            p.EndTime.TotalMilliseconds = p.StartTime.TotalMilliseconds + Utilities.GetDisplayMillisecondsFromText(p.Text);
                            string after = p.StartTime.ToShortString() + " --> " + p.EndTime.ToShortString() + " - " + p.Duration.ToShortString();
                            _totalFixes++;
                            noOfShortDisplayTimes++;
                            AddFixToListView(p, before, after);
                        }
                    }
                    else
                    {
                        unfixables.Paragraphs.Add(new Paragraph(p));
                        //LogStatus(fixAction, string.Format(Configuration.Settings.Language.FixCommonErrors.UnableToFixTextXY, i + 1, p));
                        _totalErrors++;
                    }
                }
            }
            subtitleListView1.Fill(unfixables);
        }

        public void FixLongDisplayTimes()
        {
            string fixAction = Configuration.Settings.Language.FixCommonErrors.FixLongDisplayTime;
            int noOfLongDisplayTimes = 0;
            for (int i = 0; i < _working.Paragraphs.Count; i++)
            {
                Paragraph p = _working.Paragraphs[i];
                double displayTime = p.Duration.TotalMilliseconds;
                double maxDisplayTime = (double)numericUpDownDurationMax.Value;
                if (displayTime > maxDisplayTime)
                {
                    if (AllowFix(p))
                    {
                        string before = p.StartTime.ToShortString() + " --> " + p.EndTime.ToShortString() + " - " + p.Duration.ToShortString();
                        p.EndTime.TotalMilliseconds = p.StartTime.TotalMilliseconds + maxDisplayTime;
                        string after = p.StartTime.ToShortString() + " --> " + p.EndTime.ToShortString() + " - " + p.Duration.ToShortString();
                        _totalFixes++;
                        noOfLongDisplayTimes++;
                        AddFixToListView(p, before, after);
                    }
                }
            }
        }

        private void ApplyDurationLimits_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
                DialogResult = System.Windows.Forms.DialogResult.Cancel;
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
