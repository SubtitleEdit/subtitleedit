using Nikse.SubtitleEdit.Core;
using Nikse.SubtitleEdit.Logic;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.Forms
{
    public partial class MergeTextWithSameTimeCodes : Form
    {
        private Subtitle _subtitle;
        private Subtitle _mergedSubtitle;
        private bool _loading = true;
        private readonly Timer _previewTimer = new Timer();
        public int NumberOfMerges { get; private set; }
        private string _language;
        private Dictionary<int, bool> _isFixAllowedList = new Dictionary<int, bool>();

        public Subtitle MergedSubtitle => _mergedSubtitle;

        public MergeTextWithSameTimeCodes()
        {
            UiUtil.PreInitialize(this);
            InitializeComponent();
            UiUtil.FixFonts(this);

            _previewTimer.Tick += previewTimer_Tick;
            _previewTimer.Interval = 250;

            UiUtil.FixLargeFonts(this, buttonOK);
        }

        public void Initialize(Subtitle subtitle)
        {
            if (subtitle.Paragraphs.Count > 0)
            {
                subtitle.Renumber(subtitle.Paragraphs[0].Number);
            }

            Text = Configuration.Settings.Language.MergeTextWithSameTimeCodes.Title;
            labelMaxDifferenceMS.Text = Configuration.Settings.Language.MergeTextWithSameTimeCodes.MaxDifferenceMilliseconds;
            checkBoxAutoBreakOn.Text = Configuration.Settings.Language.MergeTextWithSameTimeCodes.ReBreakLines;
            listViewFixes.Columns[0].Text = Configuration.Settings.Language.General.Apply;
            listViewFixes.Columns[1].Text = Configuration.Settings.Language.General.LineNumber;
            listViewFixes.Columns[2].Text = Configuration.Settings.Language.MergeTextWithSameTimeCodes.MergedText;

            buttonOK.Text = Configuration.Settings.Language.General.Ok;
            buttonCancel.Text = Configuration.Settings.Language.General.Cancel;
            SubtitleListview1.InitializeLanguage(Configuration.Settings.Language.General, Configuration.Settings);
            UiUtil.InitializeSubtitleFont(SubtitleListview1);
            SubtitleListview1.AutoSizeAllColumns(this);
            NumberOfMerges = 0;
            _subtitle = subtitle;
            MergeTextWithSameTimeCodes_ResizeEnd(null, null);
            _language = LanguageAutoDetect.AutoDetectGoogleLanguage(subtitle);
        }

        private void previewTimer_Tick(object sender, EventArgs e)
        {
            _previewTimer.Stop();
            Cursor = Cursors.WaitCursor;
            GeneratePreview();
            Cursor = Cursors.Default;
        }

        private void AddToListView(Paragraph p, string lineNumbers, string newText)
        {
            var item = new ListViewItem(string.Empty) { Tag = p, Checked = true };

            var subItem = new ListViewItem.ListViewSubItem(item, lineNumbers.TrimEnd(','));
            item.SubItems.Add(subItem);
            subItem = new ListViewItem.ListViewSubItem(item, UiUtil.GetListViewTextFromString(newText));
            item.SubItems.Add(subItem);

            listViewFixes.Items.Add(item);

            foreach (string number in lineNumbers.TrimEnd(',').Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
            {
                int key = Convert.ToInt32(number);
                if (!_isFixAllowedList.ContainsKey(key))
                {
                    _isFixAllowedList.Add(key, true);
                }
            }
        }

        private void GeneratePreview()
        {
            if (_subtitle == null)
            {
                return;
            }

            var mergedIndexes = new List<int>();

            NumberOfMerges = 0;
            SubtitleListview1.Items.Clear();
            SubtitleListview1.BeginUpdate();
            _mergedSubtitle = MergeLinesWithSameTimeCodes(_subtitle, mergedIndexes, out var count, true, checkBoxAutoBreakOn.Checked, (int)numericUpDownMaxMillisecondsBetweenLines.Value, _language);
            NumberOfMerges = count;

            SubtitleListview1.Fill(_subtitle);
            foreach (var index in mergedIndexes)
            {
                SubtitleListview1.SetBackgroundColor(index, Color.Green);
            }
            SubtitleListview1.EndUpdate();
            groupBoxLinesFound.Text = string.Format(Configuration.Settings.Language.MergeTextWithSameTimeCodes.NumberOfMergesX, NumberOfMerges);
        }

        private void listViewFixes_ItemChecked(object sender, ItemCheckedEventArgs e)
        {
            if (_loading)
            {
                return;
            }

            foreach (string number in e.Item.SubItems[1].Text.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
            {
                int no = Convert.ToInt32(number);
                if (_isFixAllowedList.ContainsKey(no))
                {
                    _isFixAllowedList[no] = e.Item.Checked;
                }
            }

            var mergedIndexes = new List<int>();

            NumberOfMerges = 0;
            Cursor = Cursors.WaitCursor;
            SubtitleListview1.Items.Clear();
            SubtitleListview1.BeginUpdate();
            _mergedSubtitle = MergeLinesWithSameTimeCodes(_subtitle, mergedIndexes, out var count, false, checkBoxAutoBreakOn.Checked, (int)numericUpDownMaxMillisecondsBetweenLines.Value, _language);
            NumberOfMerges = count;
            SubtitleListview1.Fill(_subtitle);
            foreach (var index in mergedIndexes)
            {
                SubtitleListview1.SetBackgroundColor(index, Color.Green);
            }
            SubtitleListview1.EndUpdate();
            Cursor = Cursors.Default;
            groupBoxLinesFound.Text = string.Format(Configuration.Settings.Language.MergeTextWithSameTimeCodes.NumberOfMergesX, NumberOfMerges);
        }

        public Subtitle MergeLinesWithSameTimeCodes(Subtitle subtitle, List<int> mergedIndexes, out int numberOfMerges, bool clearFixes, bool reBreak, int maxMsBetween, string language)
        {
            listViewFixes.BeginUpdate();
            var removed = new List<int>();
            if (!_loading)
            {
                listViewFixes.ItemChecked -= listViewFixes_ItemChecked;
            }

            if (clearFixes)
            {
                listViewFixes.Items.Clear();
                _isFixAllowedList = new Dictionary<int, bool>();
            }
            var info = new Subtitle();
            var mergedSub = Core.Forms.MergeLinesWithSameTimeCodes.Merge(subtitle, mergedIndexes, out numberOfMerges, clearFixes, reBreak, maxMsBetween, language, removed, _isFixAllowedList, info);
            foreach (var p in info.Paragraphs)
            {
                AddToListView(p, p.Extra, p.Text);
            }
            listViewFixes.EndUpdate();
            if (!_loading)
            {
                listViewFixes.ItemChecked += listViewFixes_ItemChecked;
            }

            return mergedSub;
        }

        private void MergeTextWithSameTimeCodes_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                DialogResult = DialogResult.Cancel;
            }
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void MergeTextWithSameTimeCodes_ResizeEnd(object sender, EventArgs e)
        {
            columnHeaderText.Width = -2;
        }

        private void MergeTextWithSameTimeCodes_Shown(object sender, EventArgs e)
        {
            GeneratePreview();
            listViewFixes.Focus();
            if (listViewFixes.Items.Count > 0)
            {
                listViewFixes.Items[0].Selected = true;
            }

            _loading = false;
            listViewFixes.ItemChecked += listViewFixes_ItemChecked;
        }

        private void checkBoxAutoBreakOn_CheckedChanged(object sender, EventArgs e)
        {
            Cursor = Cursors.WaitCursor;
            GeneratePreview();
            Cursor = Cursors.Default;
        }

        private void numericUpDownMaxMillisecondsBetweenLines_ValueChanged(object sender, EventArgs e)
        {
            Cursor = Cursors.WaitCursor;
            GeneratePreview();
            Cursor = Cursors.Default;
        }

        private void listViewFixes_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listViewFixes.SelectedIndices.Count > 0)
            {
                int index = listViewFixes.SelectedIndices[0];
                ListViewItem item = listViewFixes.Items[index];
                string[] numbers = item.SubItems[1].Text.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string number in numbers)
                {
                    foreach (Paragraph p in _subtitle.Paragraphs)
                    {
                        if (p.Number.ToString(CultureInfo.InvariantCulture) == number)
                        {
                            index = _subtitle.GetIndex(p);
                            SubtitleListview1.EnsureVisible(index);
                        }
                    }
                }
            }
        }
    }
}
