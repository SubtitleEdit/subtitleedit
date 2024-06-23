using Nikse.SubtitleEdit.Controls;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Logic;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using static Nikse.SubtitleEdit.Core.Settings.VerifyCompletenessSettings;

namespace Nikse.SubtitleEdit.Forms
{
    public partial class VerifyCompleteness : Form
    {
        private readonly Subtitle _subtitle;
        private Subtitle _controlSubtitle;
        private readonly Action<double> _seekAction;
        private readonly Action<Paragraph> _insertAction;

        private List<Tuple<Paragraph, double>> sortedControlParagraphsWithCoverage = new List<Tuple<Paragraph, double>>();

        public VerifyCompleteness(Subtitle subtitle, Subtitle controlSubtitle, Action<double> seekAction, Action<Paragraph> insertAction)
        {
            _subtitle = subtitle;
            _controlSubtitle = controlSubtitle;
            _seekAction = seekAction;
            _insertAction = insertAction;

            UiUtil.PreInitialize(this);
            InitializeComponent();
            UiUtil.FixFonts(this);

            var language = LanguageSettings.Current.VerifyCompleteness;
            Text = language.Title;
            buttonReload.Text = language.Reload;
            buttonDismiss.Text = language.Dismiss;
            buttonDismissAndNext.Text = language.DismissAndNext;
            buttonInsert.Text = language.Insert;
            buttonInsertAndNext.Text = language.InsertAndNext;
            toolStripMenuItemSortByCoverage.Text = language.SortByCoverage;
            toolStripMenuItemSortByTime.Text = language.SortByTime;

            subtitleListView.HideColumn(SubtitleListView.SubtitleColumn.CharactersPerSeconds);
            subtitleListView.HideColumn(SubtitleListView.SubtitleColumn.WordsPerMinute);
            subtitleListView.HideColumn(SubtitleListView.SubtitleColumn.Gap);
            subtitleListView.SetCustomResize(subtitleListView_Resize);

            buttonOK.Text = LanguageSettings.Current.General.Ok;
            UiUtil.FixLargeFonts(this, buttonOK);

            var settings = Configuration.Settings.VerifyCompleteness;
            toolStripMenuItemSortByCoverage.Checked = settings.ListSort == ListSortEnum.Coverage;
            toolStripMenuItemSortByTime.Checked = settings.ListSort == ListSortEnum.Time;

            subtitleListView.InitializeLanguage(LanguageSettings.Current.General, Configuration.Settings);
            UiUtil.InitializeSubtitleFont(subtitleListView);
            subtitleListView.ShowExtraColumn(language.Coverage);
            subtitleListView.AutoSizeAllColumns(this);

            subtitleListView.HeaderStyle = ColumnHeaderStyle.Clickable;

            LoadData();
        }

        private void LoadData()
        {
            Cursor = Cursors.WaitCursor;

            // Update filename label
            labelControlSubtitleFilename.Text = String.Format(LanguageSettings.Current.VerifyCompleteness.ControlSubtitleX, _controlSubtitle.FileName);

            // Clear previous control paragraphs
            sortedControlParagraphsWithCoverage.Clear();

            // Calculate coverages and sort paragraphs
            foreach (Paragraph p in _controlSubtitle.Paragraphs)
            {
                double overlapTotal = 0;

                foreach (Paragraph q in _subtitle.Paragraphs)
                {
                    if (p.StartTime.TotalSeconds <= q.EndTime.TotalSeconds && p.EndTime.TotalSeconds >= q.StartTime.TotalSeconds)
                    {
                        double overlapStart = Math.Max(p.StartTime.TotalSeconds, q.StartTime.TotalSeconds);
                        double overlapEnd = Math.Min(p.EndTime.TotalSeconds, q.EndTime.TotalSeconds);
                        double overlapLength = overlapEnd - overlapStart;

                        double paragraphLength = p.EndTime.TotalSeconds - p.StartTime.TotalSeconds;

                        overlapTotal += overlapLength / paragraphLength;
                    }
                }

                sortedControlParagraphsWithCoverage.Add(new Tuple<Paragraph, double>(p, overlapTotal));
            }

            // Populate list view
            PopulateListView();

            Cursor = Cursors.Default;
        }

        private void PopulateListView()
        {
            // Sort the paragraphs
            if (toolStripMenuItemSortByTime.Checked)
            {
                sortedControlParagraphsWithCoverage = sortedControlParagraphsWithCoverage.OrderBy(t => t.Item1.StartTime.TotalMilliseconds).ToList();
            } 
            else
            {
                sortedControlParagraphsWithCoverage = sortedControlParagraphsWithCoverage.OrderBy(t => t.Item2).ToList();
            }

            // Init listview            
            subtitleListView.Items.Clear();
            subtitleListView.BeginUpdate();

            // Fill the listview with the sorted control paragraphs
            subtitleListView.Fill(sortedControlParagraphsWithCoverage.Select(t => t.Item1).ToList());

            // Show coverage
            for (int i = 0; i < sortedControlParagraphsWithCoverage.Count; i++)
            {
                var overlap = sortedControlParagraphsWithCoverage[i].Item2;

                subtitleListView.SetExtraText(i, String.Format(LanguageSettings.Current.VerifyCompleteness.CoveragePercentageX, overlap * 100.0), subtitleListView.ForeColor);
                subtitleListView.SetBackgroundColor(i, CalculateColor(overlap * 100), subtitleListView.ColumnIndexExtra);
            }

            // Finalize update
            subtitleListView.EndUpdate();
        }

        private Color CalculateColor(double percentage)
        {
            double red = (percentage < 50) ? 255 : 256 - (percentage - 50) * 5.12;
            double green = (percentage > 50) ? 255 : percentage * 5.12;

            red = (red / 2) + 127.5;
            green = (green / 2) + 127.5;

            return Color.FromArgb(255, (byte)red, (byte)green, 128);
        }

        private void buttonOpenControlSubtitle_Click(object sender, System.EventArgs e)
        {
            openFileDialog.Title = LanguageSettings.Current.VerifyCompleteness.OpenControlSubtitle;
            openFileDialog.FileName = string.Empty;
            openFileDialog.Filter = UiUtil.SubtitleExtensionFilter.Value;
            if (openFileDialog.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }

            TryLoadSubtitle(openFileDialog.FileName);
        }

        private void TryLoadSubtitle(string fileName)
        {
            var controlSubtitle = LoadSubtitle(fileName);
            if (controlSubtitle.Paragraphs.Count > 0)
            {
                _controlSubtitle = controlSubtitle;
                LoadData();
            }
            else
            {
                MessageBox.Show(this, LanguageSettings.Current.VerifyCompleteness.ControlSubtitleError, LanguageSettings.Current.General.Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void buttonReload_Click(object sender, System.EventArgs e)
        {
            LoadData();
        }

        private void buttonDismiss_Click(object sender, EventArgs e)
        {
            ProcessParagraph(false, false);
        }

        private void buttonDismissAndNext_Click(object sender, System.EventArgs e)
        {
            ProcessParagraph(false, true);
        }

        private void buttonInsert_Click(object sender, EventArgs e)
        {
            ProcessParagraph(true, false);
        }

        private void buttonInsertAndNext_Click(object sender, System.EventArgs e)
        {
            ProcessParagraph(true, true);
        }

        private void ProcessParagraph(bool insertIntoSubtitle, bool goToNext)
        {
            if (subtitleListView.SelectedIndices.Count == 1)
            {
                var index = subtitleListView.SelectedIndices[0];
                var paragraph = sortedControlParagraphsWithCoverage[index].Item1;

                // Insert the paragragh in the parent subtitle if requested
                if (insertIntoSubtitle)
                {
                    _insertAction.Invoke(paragraph);
                }

                // Remove the paragragh from the list
                sortedControlParagraphsWithCoverage.RemoveAt(index);
                subtitleListView.Items.RemoveAt(index);

                // Select the next paragraph
                if (goToNext)
                {
                    subtitleListView.SelectIndexAndEnsureVisible(index);
                }
            }
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            // Save settings
            Configuration.Settings.VerifyCompleteness.ListSort = toolStripMenuItemSortByTime.Checked ? ListSortEnum.Time : ListSortEnum.Coverage;

            this.Close();
        }

        private void subtitleListView_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            if (subtitleListView.SelectedIndices.Count == 1)
            {
                var index = subtitleListView.SelectedIndices[0];
                var paragraph = sortedControlParagraphsWithCoverage[index].Item1;

                // Tell parent to seek to the paragraph's time
                _seekAction.Invoke(paragraph.StartTime.TotalSeconds);

                EnableButtons(true);
            }
            else
            {
                EnableButtons(false);
            }
        }

        private void EnableButtons(bool enable)
        {
            buttonDismiss.Enabled = enable;
            buttonDismissAndNext.Enabled = enable;
            buttonInsert.Enabled = enable;
            buttonInsertAndNext.Enabled = enable;
        }

        private void subtitleListView_Click(object sender, EventArgs e)
        {
            subtitleListView_SelectedIndexChanged(sender, e);
        }

        private void subtitleListView_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete && subtitleListView.SelectedItems.Count > 0)
            {
                buttonDismissAndNext_Click(sender, e);
            }
        }

        private void subtitleListView_Resize(object sender, EventArgs e)
        {
            const int lastColumnWidth = 100;
            var columnsCount = subtitleListView.Columns.Count - 1;
            var width = 0;
            for (int i = 0; i < columnsCount - 1; i++)
            {
                width += subtitleListView.Columns[i].Width;
            }
            subtitleListView.Columns[columnsCount - 1].Width = subtitleListView.Width - (width + lastColumnWidth);
            subtitleListView.Columns[columnsCount].Width = -2;
        }

        private void subtitleListView_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = e.Data.GetDataPresent(DataFormats.FileDrop) ? DragDropEffects.Copy : DragDropEffects.None;
        }

        private void subtitleListView_DragDrop(object sender, DragEventArgs e)
        {
            if (!(e.Data.GetData(DataFormats.FileDrop) is string[] files))
            {
                return;
            }

            if (files.Length > 1)
            {
                MessageBox.Show(this, LanguageSettings.Current.Main.DropOnlyOneFile, LanguageSettings.Current.General.Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var filePath = files[0];
            if (FileUtil.IsDirectory(filePath))
            {
                MessageBox.Show(this, LanguageSettings.Current.Main.ErrorDirectoryDropNotAllowed, LanguageSettings.Current.General.Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            TryLoadSubtitle(filePath);
        }

        private void toolStripMenuItemSortByCoverage_Click(object sender, EventArgs e)
        {
            ChangeListSort(ListSortEnum.Coverage);
        }

        private void toolStripMenuItemSortByTime_Click(object sender, EventArgs e)
        {
            ChangeListSort(ListSortEnum.Time);
        }

        private void ChangeListSort(ListSortEnum sort)
        {
            // Update context menu items checked staed
            toolStripMenuItemSortByCoverage.Checked = sort == ListSortEnum.Coverage;
            toolStripMenuItemSortByTime.Checked = sort == ListSortEnum.Time;

            // Save selected paragraph, if any
            Paragraph selectedParagraph = null;

            if (subtitleListView.SelectedIndices.Count == 1)
            {
                var index = subtitleListView.SelectedIndices[0];
                var paragraph = sortedControlParagraphsWithCoverage[index].Item1;

                selectedParagraph = paragraph;
            }

            // Reload listview
            Cursor = Cursors.WaitCursor;
            PopulateListView();
            Cursor = Cursors.Default;

            // Restore selected index
            if (selectedParagraph != null)
            {
                subtitleListView.SelectIndexAndEnsureVisible(selectedParagraph);
            }
        }

        private void subtitleListView_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            if (e.Column == subtitleListView.ColumnIndexExtra)
            {
                if (!toolStripMenuItemSortByCoverage.Checked)
                {
                    ChangeListSort(ListSortEnum.Coverage);
                }
            }
            else if (e.Column == subtitleListView.ColumnIndexNumber || e.Column == subtitleListView.ColumnIndexStart || e.Column == subtitleListView.ColumnIndexEnd)
            {
                if (!toolStripMenuItemSortByTime.Checked)
                {
                    ChangeListSort(ListSortEnum.Time);
                }
            }
        }

        private void VerifyCompleteness_Shown(object sender, EventArgs e)
        {
            subtitleListView.Focus();
        }

        private void VerifyCompleteness_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                this.Close();
            }
        }


        // Static helpers

        public static Subtitle LoadSubtitle(string fileName)
        {
            var subtitle = new Subtitle();
            var format = subtitle.LoadSubtitle(fileName, out _, null);
            if (format == null)
            {
                foreach (var f in SubtitleFormat.GetBinaryFormats(false))
                {
                    if (f.IsMine(null, fileName))
                    {
                        f.LoadSubtitle(subtitle, null, fileName);
                        break; // format found, exit the loop
                    }
                }
            }

            return subtitle;
        }
    }
}
