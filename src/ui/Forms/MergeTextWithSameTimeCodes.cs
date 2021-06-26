using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Logic;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.Forms
{
    public partial class MergeTextWithSameTimeCodes : Form
    {
        public class FixListItem
        {
            public List<int> LineNumbers { get; set; }
            public bool Checked { get; set; }

            public FixListItem()
            {
                LineNumbers = new List<int>();
            }
        }

        public Subtitle MergedSubtitle { get; private set; }
        public int NumberOfMerges { get; private set; }

        private Subtitle _subtitle;
        private bool _loading = true;
        private readonly Timer _previewTimer = new Timer();
        private string _language;
        private Dictionary<int, bool> _isFixAllowedList = new Dictionary<int, bool>();
        private List<FixListItem> _fixItems;

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

            Text = LanguageSettings.Current.MergeTextWithSameTimeCodes.Title;
            labelMaxDifferenceMS.Text = LanguageSettings.Current.MergeTextWithSameTimeCodes.MaxDifferenceMilliseconds;
            checkBoxAutoBreakOn.Text = LanguageSettings.Current.MergeTextWithSameTimeCodes.ReBreakLines;
            listViewFixes.Columns[0].Text = LanguageSettings.Current.General.Apply;
            listViewFixes.Columns[1].Text = LanguageSettings.Current.General.LineNumber;
            listViewFixes.Columns[2].Text = LanguageSettings.Current.MergeTextWithSameTimeCodes.MergedText;

            toolStripMenuItemInverseSelection.Text = LanguageSettings.Current.Main.Menu.Edit.InverseSelection;
            toolStripMenuItemSelectAll.Text = LanguageSettings.Current.Main.Menu.ContextMenu.SelectAll;
            buttonOK.Text = LanguageSettings.Current.General.Ok;
            buttonCancel.Text = LanguageSettings.Current.General.Cancel;
            SubtitleListview1.InitializeLanguage(LanguageSettings.Current.General, Configuration.Settings);
            UiUtil.InitializeSubtitleFont(SubtitleListview1);
            SubtitleListview1.AutoSizeAllColumns(this);
            NumberOfMerges = 0;
            _subtitle = subtitle;
            _language = LanguageAutoDetect.AutoDetectGoogleLanguage(subtitle);
        }

        private void previewTimer_Tick(object sender, EventArgs e)
        {
            _previewTimer.Stop();
            Cursor = Cursors.WaitCursor;
            GeneratePreview();
            Cursor = Cursors.Default;
        }

        private ListViewItem MakeListViewItem(Paragraph p, string lineNumbers, string newText)
        {
            var item = new ListViewItem(string.Empty) { Tag = p, Checked = true };
            var subItem = new ListViewItem.ListViewSubItem(item, lineNumbers.TrimEnd(','));
            item.SubItems.Add(subItem);
            subItem = new ListViewItem.ListViewSubItem(item, UiUtil.GetListViewTextFromString(newText));
            item.SubItems.Add(subItem);

            var fixItem = new FixListItem { Checked = true };
            _fixItems.Add(fixItem);

            foreach (var number in lineNumbers.TrimEnd(',').Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
            {
                int key = Convert.ToInt32(number);
                fixItem.LineNumbers.Add(key);
                if (!_isFixAllowedList.ContainsKey(key))
                {
                    _isFixAllowedList.Add(key, true);
                }
            }

            return item;
        }

        private void GeneratePreview()
        {
            if (_subtitle == null)
            {
                return;
            }

            var mergedIndexes = new List<int>();
            _fixItems = new List<FixListItem>();

            NumberOfMerges = 0;
            SubtitleListview1.Items.Clear();
            SubtitleListview1.BeginUpdate();
            MergedSubtitle = MergeLinesWithSameTimeCodes(_subtitle, mergedIndexes, out var count, true, checkBoxAutoBreakOn.Checked, (int)numericUpDownMaxMillisecondsBetweenLines.Value, _language);
            NumberOfMerges = count;

            SubtitleListview1.Fill(_subtitle);
            SetBackgroundColors();
            SubtitleListview1.EndUpdate();
            groupBoxLinesFound.Text = string.Format(LanguageSettings.Current.MergeTextWithSameTimeCodes.NumberOfMergesX, NumberOfMerges);
        }

        private void SetBackgroundColors()
        {
            int colorIdx = 0;
            var colors = new List<Color>
            {
                Color.Green,
                Color.LimeGreen,
                Color.GreenYellow,
            };

            foreach (var fixItem in _fixItems.Where(p => p.Checked))
            {
                foreach (var number in fixItem.LineNumbers)
                {
                    SubtitleListview1.SetBackgroundColor(number - 1, colors[colorIdx]);
                }

                colorIdx++;
                if (colorIdx >= colors.Count)
                {
                    colorIdx = 0;
                }
            }
        }

        private void listViewFixes_ItemChecked(object sender, ItemCheckedEventArgs e)
        {
            if (_loading)
            {
                return;
            }

            foreach (var number in e.Item.SubItems[1].Text.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
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
            MergedSubtitle = MergeLinesWithSameTimeCodes(_subtitle, mergedIndexes, out var count, false, checkBoxAutoBreakOn.Checked, (int)numericUpDownMaxMillisecondsBetweenLines.Value, _language);
            NumberOfMerges = count;
            SubtitleListview1.Fill(_subtitle);
            SetBackgroundColors();
            SubtitleListview1.EndUpdate();
            Cursor = Cursors.Default;
            groupBoxLinesFound.Text = string.Format(LanguageSettings.Current.MergeTextWithSameTimeCodes.NumberOfMergesX, NumberOfMerges);
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
            var listViewItems = new List<ListViewItem>();
            foreach (var p in info.Paragraphs)
            {
                listViewItems.Add(MakeListViewItem(p, p.Extra, p.Text));
            }
            listViewFixes.Items.AddRange(listViewItems.ToArray());
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
            MergeTextWithSameTimeCodes_ResizeEnd(sender, e);

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

        private void toolStripMenuItemSelectAll_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in listViewFixes.Items)
            {
                item.Checked = true;
            }
        }

        private void toolStripMenuItemInverseSelection_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in listViewFixes.Items)
            {
                item.Checked = !item.Checked;
            }
        }
    }
}
