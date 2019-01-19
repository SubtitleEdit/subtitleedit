using Nikse.SubtitleEdit.Core;
using Nikse.SubtitleEdit.Logic;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.Forms
{
    public sealed partial class ShowHistory : Form
    {
        private int _selectedIndex = -1;
        private Subtitle _subtitle;
        private int _undoIndex;

        public ShowHistory()
        {
            UiUtil.PreInitialize(this);
            InitializeComponent();
            UiUtil.FixFonts(this);

            Text = Configuration.Settings.Language.ShowHistory.Title;
            label1.Text = Configuration.Settings.Language.ShowHistory.SelectRollbackPoint;
            listViewHistory.Columns[0].Text = Configuration.Settings.Language.ShowHistory.Time;
            listViewHistory.Columns[1].Text = Configuration.Settings.Language.ShowHistory.Description;
            buttonCompare.Text = Configuration.Settings.Language.ShowHistory.CompareWithCurrent;
            buttonCompareHistory.Text = Configuration.Settings.Language.ShowHistory.CompareHistoryItems;
            buttonRollback.Text = Configuration.Settings.Language.ShowHistory.Rollback;
            buttonCancel.Text = Configuration.Settings.Language.General.Cancel;
            UiUtil.FixLargeFonts(this, buttonRollback);
        }

        public int SelectedIndex => _selectedIndex;

        public void Initialize(Subtitle subtitle, int undoIndex)
        {
            _subtitle = subtitle;
            _undoIndex = undoIndex;
            int i = 0;
            foreach (HistoryItem item in subtitle.HistoryItems)
            {
                AddHistoryItemToListView(item, i++);
            }
            ListViewHistorySelectedIndexChanged(null, null);
            if (listViewHistory.Items.Count > 0 && _undoIndex >= 0 && _undoIndex < listViewHistory.Items.Count)
            {
                listViewHistory.Items[_undoIndex].Selected = true;
            }
        }

        private void AddHistoryItemToListView(HistoryItem hi, int index)
        {
            var item = new ListViewItem(string.Empty)
            {
                Tag = hi,
                Text = hi.ToHHMMSS()
            };

            if (index > _undoIndex)
            {
                item.UseItemStyleForSubItems = true;
                item.Font = new Font(item.Font.FontFamily, item.Font.SizeInPoints, FontStyle.Italic);
                item.ForeColor = Color.Gray;
            }
            item.SubItems.Add(hi.Description);
            listViewHistory.Items.Add(item);
        }

        private void FormShowHistory_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                DialogResult = DialogResult.Cancel;
            }
        }

        private void ButtonOkClick(object sender, EventArgs e)
        {
            if (listViewHistory.SelectedItems.Count > 0)
            {
                _selectedIndex = listViewHistory.SelectedItems[0].Index;
                DialogResult = DialogResult.OK;
            }
            else
            {
                DialogResult = DialogResult.Cancel;
            }
        }

        private void ButtonCompareClick(object sender, EventArgs e)
        {
            if (listViewHistory.SelectedItems.Count == 1)
            {
                HistoryItem h2 = _subtitle.HistoryItems[listViewHistory.SelectedItems[0].Index];
                string descr2 = h2.ToHHMMSS() + " - " + h2.Description;
                using (var compareForm = new Compare())
                {
                    compareForm.Initialize(_subtitle, Configuration.Settings.Language.General.CurrentSubtitle, h2.Subtitle, descr2);
                    compareForm.ShowDialog(this);
                }
            }
        }

        private void ListViewHistorySelectedIndexChanged(object sender, EventArgs e)
        {
            buttonRollback.Enabled = listViewHistory.SelectedItems.Count == 1;
            buttonCompare.Enabled = listViewHistory.SelectedItems.Count == 1;
            buttonCompareHistory.Enabled = listViewHistory.SelectedItems.Count == 2;
            buttonRollback.Enabled = listViewHistory.SelectedItems.Count == 1 && listViewHistory.SelectedItems[0].Index <= _undoIndex;
        }

        private void ButtonCompareHistoryClick(object sender, EventArgs e)
        {
            if (listViewHistory.SelectedItems.Count == 2)
            {
                HistoryItem h1 = _subtitle.HistoryItems[listViewHistory.SelectedItems[0].Index];
                HistoryItem h2 = _subtitle.HistoryItems[listViewHistory.SelectedItems[1].Index];
                string descr1 = h1.ToHHMMSS() + " - " + h1.Description;
                string descr2 = h2.ToHHMMSS() + " - " + h2.Description;
                using (var compareForm = new Compare())
                {
                    compareForm.Initialize(h1.Subtitle, descr1, h2.Subtitle, descr2);
                    compareForm.ShowDialog(this);
                }
            }
        }

    }
}
