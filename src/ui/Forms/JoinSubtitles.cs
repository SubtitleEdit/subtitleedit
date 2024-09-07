﻿using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Logic;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Nikse.SubtitleEdit.Core.Enums;
using MessageBox = Nikse.SubtitleEdit.Forms.SeMsgBox.MessageBox;

namespace Nikse.SubtitleEdit.Forms
{
    public sealed partial class JoinSubtitles : PositionAndSizeForm
    {
        private readonly List<string> _fileNamesToJoin = new List<string>();
        public Subtitle JoinedSubtitle { get; set; }
        public SubtitleFormat JoinedFormat { get; private set; }

        public JoinSubtitles()
        {
            UiUtil.PreInitialize(this);
            InitializeComponent();
            UiUtil.FixFonts(this);
            JoinedSubtitle = new Subtitle();
            labelTotalLines.Text = string.Empty;

            listViewParts.Columns[0].Text = LanguageSettings.Current.JoinSubtitles.NumberOfLines;
            listViewParts.Columns[1].Text = LanguageSettings.Current.JoinSubtitles.StartTime;
            listViewParts.Columns[2].Text = LanguageSettings.Current.JoinSubtitles.EndTime;
            listViewParts.Columns[3].Text = LanguageSettings.Current.JoinSubtitles.FileName;

            moveUpToolStripMenuItem.Text = LanguageSettings.Current.DvdSubRip.MoveUp;
            moveDownToolStripMenuItem.Text = LanguageSettings.Current.DvdSubRip.MoveDown;
            moveTopToolStripMenuItem.Text = LanguageSettings.Current.MultipleReplace.MoveToTop;
            moveBottomToolStripMenuItem.Text = LanguageSettings.Current.MultipleReplace.MoveToBottom;

            buttonAddFile.Text = LanguageSettings.Current.DvdSubRip.Add;
            buttonRemoveFile.Text = LanguageSettings.Current.DvdSubRip.Remove;
            buttonClear.Text = LanguageSettings.Current.DvdSubRip.Clear;

            Text = LanguageSettings.Current.JoinSubtitles.Title;
            groupBoxPreview.Text = LanguageSettings.Current.JoinSubtitles.Information;
            buttonJoin.Text = LanguageSettings.Current.JoinSubtitles.Join;
            buttonCancel.Text = LanguageSettings.Current.General.Cancel;

            radioButtonJoinPlain.Text = LanguageSettings.Current.JoinSubtitles.AlreadyCorrectTimeCodes;
            radioButtonJoinAddTime.Text = LanguageSettings.Current.JoinSubtitles.AppendTimeCodes;
            labelAddTime.Text = LanguageSettings.Current.JoinSubtitles.AddMs;

            labelAddTime.Left = radioButtonJoinAddTime.Left + radioButtonJoinAddTime.Width + 20;
            numericUpDownAddMs.Left = labelAddTime.Left + labelAddTime.Width + 5;

            UiUtil.FixLargeFonts(this, buttonCancel);

            if (Configuration.Settings.Tools.JoinCorrectTimeCodes)
            {
                radioButtonJoinPlain.Checked = true;
            }
            else
            {
                radioButtonJoinAddTime.Checked = true;
            }
            numericUpDownAddMs.Value = Configuration.Settings.Tools.JoinAddMs;
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void buttonSplit_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
        }

        private void JoinSubtitles_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                DialogResult = DialogResult.Cancel;
            }
        }

        private void listViewParts_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop, false))
            {
                e.Effect = DragDropEffects.All;
            }
        }

        private void listViewParts_DragDrop(object sender, DragEventArgs e)
        {
            var files = (string[])e.Data.GetData(DataFormats.FileDrop);

            TaskDelayHelper.RunDelayed(TimeSpan.FromMilliseconds(1), () =>
            {
                var fileNames = files.ToList();
                fileNames.Sort(ListViewSorter.NaturalComparer);
                foreach (var fileName in fileNames)
                {
                    if (!_fileNamesToJoin.Any(file => file.Equals(fileName, StringComparison.OrdinalIgnoreCase)))
                    {
                        _fileNamesToJoin.Add(fileName);
                    }
                }

                SortAndLoad();
            });
        }

        private void SortAndLoad()
        {
            JoinedFormat = new SubRip(); // default subtitle format
            string header = null;
            SubtitleFormat lastFormat = null;
            var subtitles = new List<Subtitle>();
            for (var k = 0; k < _fileNamesToJoin.Count; k++)
            {
                var fileName = _fileNamesToJoin[k];
                try
                {
                    var sub = new Subtitle();
                    SubtitleFormat format = null;

                    if (fileName.EndsWith(".ismt", StringComparison.InvariantCultureIgnoreCase) ||
                        fileName.EndsWith(".mp4", StringComparison.InvariantCultureIgnoreCase) ||
                        fileName.EndsWith(".m4v", StringComparison.InvariantCultureIgnoreCase) ||
                        fileName.EndsWith(".3gp", StringComparison.InvariantCultureIgnoreCase))
                    {
                        format = new IsmtDfxp();
                        if (format.IsMine(null, fileName))
                        {
                            var s = new Subtitle();
                            format.LoadSubtitle(s, null, fileName);
                            if (s.Paragraphs.Count > 0)
                            {
                                lastFormat = format;
                            }
                        }
                    }

                    var lines = FileUtil.ReadAllLinesShared(fileName, LanguageAutoDetect.GetEncodingFromFile(fileName));
                    if (lastFormat != null && lastFormat.IsMine(lines, fileName))
                    {
                        format = lastFormat;
                        format.LoadSubtitle(sub, lines, fileName);
                    }

                    if (sub.Paragraphs.Count == 0 || format == null)
                    {
                        format = sub.LoadSubtitle(fileName, out _, null);
                    }

                    if (format == null && lines.Count > 0 && lines.Count < 10 && lines[0].Trim() == "WEBVTT")
                    {
                        format = new WebVTT(); // empty WebVTT
                    }

                    if (format == null)
                    {
                        foreach (var binaryFormat in SubtitleFormat.GetBinaryFormats(true))
                        {
                            if (binaryFormat.IsMine(null, fileName))
                            {
                                binaryFormat.LoadSubtitle(sub, null, fileName);
                                format = binaryFormat;
                                break;
                            }
                        }
                    }

                    if (format == null)
                    {
                        foreach (var f in SubtitleFormat.GetTextOtherFormats())
                        {
                            if (f.IsMine(lines, fileName))
                            {
                                f.LoadSubtitle(sub, lines, fileName);
                                format = f;
                                break;
                            }
                        }
                    }

                    if (format == null)
                    {
                        Revert(k, LanguageSettings.Current.UnknownSubtitle.Title + Environment.NewLine + fileName);
                        break;
                    }

                    if (sub.Header != null)
                    {
                        if (format.Name == AdvancedSubStationAlpha.NameOfFormat)
                        {
                            sub.Header = sub.Header.Replace("*Default", "Default");
                            foreach (var subParagraph in sub.Paragraphs)
                            {
                                if (subParagraph.Extra == "*Default")
                                {
                                    subParagraph.Extra = "Default";
                                }
                            }
                        }

                        if (format.Name == AdvancedSubStationAlpha.NameOfFormat && header != null)
                        {
                            var oldPlayResX = AdvancedSubStationAlpha.GetTagFromHeader("PlayResX", "[Script Info]", header);
                            var oldPlayResY = AdvancedSubStationAlpha.GetTagFromHeader("PlayResY", "[Script Info]", header);
                            var newPlayResX = AdvancedSubStationAlpha.GetTagFromHeader("PlayResX", "[Script Info]", sub.Header);
                            var newPlayResY = AdvancedSubStationAlpha.GetTagFromHeader("PlayResY", "[Script Info]", sub.Header);

                            var stylesInHeader = AdvancedSubStationAlpha.GetStylesFromHeader(header);
                            var styles = new List<SsaStyle>();
                            foreach (var styleName in stylesInHeader)
                            {
                                styles.Add(AdvancedSubStationAlpha.GetSsaStyle(styleName, header));
                            }

                            foreach (var newStyle in AdvancedSubStationAlpha.GetStylesFromHeader(sub.Header))
                            {
                                if (stylesInHeader.Any(p => p == newStyle))
                                {
                                    if (IsStyleDifferent(newStyle, sub, header))
                                    {
                                        var styleToBeRenamed = AdvancedSubStationAlpha.GetSsaStyle(newStyle, sub.Header);
                                        var newName = styleToBeRenamed.Name + "_" + Guid.NewGuid();
                                        foreach (var p in sub.Paragraphs.Where(p => p.Extra == styleToBeRenamed.Name))
                                        {
                                            p.Extra = newName;
                                        }

                                        styleToBeRenamed.Name = newName;
                                        styles.Add(styleToBeRenamed);
                                    }
                                }
                                else
                                {
                                    styles.Add(AdvancedSubStationAlpha.GetSsaStyle(newStyle, sub.Header));
                                }
                            }

                            header = AdvancedSubStationAlpha.GetHeaderAndStylesFromAdvancedSubStationAlpha(header, styles);
                            if (!string.IsNullOrEmpty(oldPlayResX) && string.IsNullOrEmpty(newPlayResX))
                            {
                                header = AdvancedSubStationAlpha.AddTagToHeader("PlayResX", oldPlayResX, "[Script Info]", header);
                            }
                            if (!string.IsNullOrEmpty(oldPlayResY) && string.IsNullOrEmpty(newPlayResY))
                            {
                                header = AdvancedSubStationAlpha.AddTagToHeader("PlayResY", oldPlayResY, "[Script Info]", header);
                            }
                        }
                        else
                        {
                            header = sub.Header;
                        }
                    }

                    lastFormat = lastFormat == null || lastFormat.FriendlyName == format.FriendlyName ? format : new SubRip();

                    subtitles.Add(sub);
                }
                catch (Exception exception)
                {
                    Revert(k, exception.Message);
                    return;
                }
            }
            JoinedFormat = lastFormat;


            if (!radioButtonJoinAddTime.Checked)
            {
                for (var outer = 0; outer < subtitles.Count; outer++)
                {
                    for (var inner = 1; inner < subtitles.Count; inner++)
                    {
                        var a = subtitles[inner - 1];
                        var b = subtitles[inner];
                        if (a.Paragraphs.Count > 0 && b.Paragraphs.Count > 0 && a.Paragraphs[0].StartTime.TotalMilliseconds > b.Paragraphs[0].StartTime.TotalMilliseconds)
                        {
                            (_fileNamesToJoin[inner - 1], _fileNamesToJoin[inner]) = (_fileNamesToJoin[inner], _fileNamesToJoin[inner - 1]);
                            (subtitles[inner - 1], subtitles[inner]) = (subtitles[inner], subtitles[inner - 1]);
                        }
                    }
                }
            }

            listViewParts.BeginUpdate();
            listViewParts.Items.Clear();
            int i = 0;
            foreach (var fileName in _fileNamesToJoin)
            {
                var sub = subtitles[i];
                var lvi = new ListViewItem($"{sub.Paragraphs.Count:#,###,###}");
                lvi.Tag = fileName;
                if (sub.Paragraphs.Count > 0)
                {
                    lvi.SubItems.Add(sub.Paragraphs[0].StartTime.ToString());
                    lvi.SubItems.Add(sub.Paragraphs[sub.Paragraphs.Count - 1].StartTime.ToString());
                }
                else
                {
                    lvi.SubItems.Add("-");
                    lvi.SubItems.Add("-");
                }
                lvi.SubItems.Add(fileName);
                listViewParts.Items.Add(lvi);
                i++;
            }
            listViewParts.EndUpdate();

            JoinedSubtitle = new Subtitle();
            if (JoinedFormat != null && JoinedFormat.FriendlyName != SubRip.NameOfFormat)
            {
                JoinedSubtitle.Header = header;
            }

            var addTime = radioButtonJoinAddTime.Checked;
            foreach (var sub in subtitles)
            {
                double addMs = 0;
                if (addTime && JoinedSubtitle.Paragraphs.Count > 0)
                {
                    addMs = JoinedSubtitle.Paragraphs.Last().EndTime.TotalMilliseconds + Convert.ToDouble(numericUpDownAddMs.Value);
                }
                foreach (var p in sub.Paragraphs)
                {
                    p.StartTime.TotalMilliseconds += addMs;
                    p.EndTime.TotalMilliseconds += addMs;
                    JoinedSubtitle.Paragraphs.Add(p);
                }
            }

            if (radioButtonJoinPlain.Checked)
            {
                JoinedSubtitle.Sort(SubtitleSortCriteria.StartTime);
            }

            JoinedSubtitle.Renumber();
            labelTotalLines.Text = string.Format(LanguageSettings.Current.JoinSubtitles.TotalNumberOfLinesX, JoinedSubtitle.Paragraphs.Count);
        }

        private static bool IsStyleDifferent(string styleName, Subtitle newSubtitle, string oldHeader)
        {
            var newStyle = AdvancedSubStationAlpha.GetSsaStyle(styleName, newSubtitle.Header);
            var oldStyle = AdvancedSubStationAlpha.GetSsaStyle(styleName, oldHeader);
            if (oldStyle == null || newStyle == null)
            {
                return true;
            }

            return newStyle.ToRawAss() != oldStyle.ToRawAss();
        }

        private void Revert(int idx, string message)
        {
            for (int i = _fileNamesToJoin.Count - 1; i >= idx; i--)
            {
                _fileNamesToJoin.RemoveAt(i);
            }
            MessageBox.Show(message);
        }

        private void JoinSubtitles_Resize(object sender, EventArgs e)
        {
            columnHeaderFileName.Width = -2;
        }

        private void ButtonAddSubtitleClick(object sender, EventArgs e)
        {
            openFileDialog1.Title = LanguageSettings.Current.General.OpenSubtitle;
            openFileDialog1.FileName = string.Empty;
            openFileDialog1.Filter = UiUtil.SubtitleExtensionFilter.Value;
            openFileDialog1.Multiselect = true;
            if (openFileDialog1.ShowDialog(this) == DialogResult.OK)
            {
                var sb = new StringBuilder();
                var fileNames = openFileDialog1.FileNames.ToList();
                fileNames.Sort(ListViewSorter.NaturalComparer);
                foreach (var fileName in fileNames)
                {
                    Application.DoEvents();
                    if (File.Exists(fileName))
                    {
                        var fileInfo = new FileInfo(fileName);
                        if (fileInfo.Length < Subtitle.MaxFileSize)
                        {
                            if (!_fileNamesToJoin.Any(file => file.Equals(fileName, StringComparison.OrdinalIgnoreCase)))
                            {
                                _fileNamesToJoin.Add(fileName);
                            }
                        }
                        else
                        {
                            sb.AppendLine(string.Format(LanguageSettings.Current.Main.FileXIsLargerThan10MB, fileName));
                        }
                    }
                }

                SortAndLoad();
                if (sb.Length > 0)
                {
                    MessageBox.Show(sb.ToString());
                }

                JoinSubtitles_Resize(sender, e);
            }
        }

        private void ButtonRemoveSubtitle_Click(object sender, EventArgs e)
        {
            for (int i = listViewParts.SelectedIndices.Count - 1; i >= 0; i--)
            {
                _fileNamesToJoin.RemoveAt(listViewParts.SelectedIndices[i]);
            }
            if (_fileNamesToJoin.Count == 0)
            {
                buttonClear_Click(null, null);
            }
            else
            {
                SortAndLoad();
            }
        }

        private void buttonClear_Click(object sender, EventArgs e)
        {
            _fileNamesToJoin.Clear();
            listViewParts.Items.Clear();
            JoinedSubtitle = new Subtitle();
        }

        private void JoinSubtitles_Shown(object sender, EventArgs e)
        {
            JoinSubtitles_Resize(sender, e);
        }

        private void JoinSubtitles_FormClosing(object sender, FormClosingEventArgs e)
        {
            Configuration.Settings.Tools.JoinCorrectTimeCodes = radioButtonJoinPlain.Checked;
            Configuration.Settings.Tools.JoinAddMs = (int)numericUpDownAddMs.Value;
        }

        private void RadioButtonJoinAddTime_CheckedChanged(object sender, EventArgs e)
        {
            numericUpDownAddMs.Enabled = radioButtonJoinAddTime.Checked;
            labelAddTime.Enabled = radioButtonJoinAddTime.Checked;
        }

        private void RadioButtonJoinPlain_CheckedChanged(object sender, EventArgs e)
        {
            numericUpDownAddMs.Enabled = radioButtonJoinAddTime.Checked;
            labelAddTime.Enabled = radioButtonJoinAddTime.Checked;
            SortAndLoad();
            ListViewSorter.SetSortArrow(listViewParts.Columns[3], SortOrder.None);
        }

        private void contextMenuStripParts_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (radioButtonJoinPlain.Checked)
            {
                e.Cancel = true;
            }
        }

        private void MoveUp(ListView listView)
        {
            if (listView.SelectedItems.Count != 1)
            {
                return;
            }

            var idx = listView.SelectedItems[0].Index;
            if (idx == 0)
            {
                return;
            }

            var item = listView.SelectedItems[0];
            listView.Items.RemoveAt(idx);
            var style = _fileNamesToJoin[idx];
            _fileNamesToJoin.RemoveAt(idx);
            _fileNamesToJoin.Insert(idx - 1, style);

            idx--;
            listView.Items.Insert(idx, item);
            UpdateSelectedIndices(listView, idx);
        }

        private void MoveDown(ListView listView)
        {
            if (listView.SelectedItems.Count != 1)
            {
                return;
            }

            var idx = listView.SelectedItems[0].Index;
            if (idx >= listView.Items.Count - 1)
            {
                return;
            }

            var item = listView.SelectedItems[0];
            listView.Items.RemoveAt(idx);
            var style = _fileNamesToJoin[idx];
            _fileNamesToJoin.RemoveAt(idx);
            _fileNamesToJoin.Insert(idx + 1, style);

            idx++;
            listView.Items.Insert(idx, item);
            UpdateSelectedIndices(listView, idx);
        }

        private void MoveToTop(ListView listView)
        {
            if (listView.SelectedItems.Count != 1)
            {
                return;
            }

            var idx = listView.SelectedItems[0].Index;
            if (idx == 0)
            {
                return;
            }

            var item = listView.SelectedItems[0];
            listView.Items.RemoveAt(idx);
            var style = _fileNamesToJoin[idx];
            _fileNamesToJoin.RemoveAt(idx);
            _fileNamesToJoin.Insert(0, style);

            idx = 0;
            listView.Items.Insert(idx, item);
            UpdateSelectedIndices(listView, idx);
        }

        private void MoveToBottom(ListView listView)
        {
            if (listView.SelectedItems.Count != 1)
            {
                return;
            }

            var idx = listView.SelectedItems[0].Index;
            if (idx == listView.Items.Count - 1)
            {
                return;
            }

            var item = listView.SelectedItems[0];
            listView.Items.RemoveAt(idx);
            var style = _fileNamesToJoin[idx];
            _fileNamesToJoin.RemoveAt(idx);
            _fileNamesToJoin.Add(style);

            listView.Items.Add(item);
            UpdateSelectedIndices(listView);
        }

        private static void UpdateSelectedIndices(ListView listView, int startingIndex = -1, int numberOfSelectedItems = 1)
        {
            if (numberOfSelectedItems == 0)
            {
                return;
            }

            if (startingIndex == -1 || startingIndex >= listView.Items.Count)
            {
                startingIndex = listView.Items.Count - 1;
            }

            if (startingIndex - numberOfSelectedItems < -1)
            {
                return;
            }

            listView.SelectedItems.Clear();
            for (var i = 0; i < numberOfSelectedItems; i++)
            {
                listView.Items[startingIndex - i].Selected = true;
                listView.Items[startingIndex - i].EnsureVisible();
                listView.Items[startingIndex - i].Focused = true;
            }
        }

        private void moveUpToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (radioButtonJoinPlain.Checked)
            {
                return;
            }

            ListViewSorter.SetSortArrow(listViewParts.Columns[3], SortOrder.None);
            MoveUp(listViewParts);
        }

        private void moveDownToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (radioButtonJoinPlain.Checked)
            {
                return;
            }

            ListViewSorter.SetSortArrow(listViewParts.Columns[3], SortOrder.None);
            MoveDown(listViewParts);
        }

        private void moveTopToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (radioButtonJoinPlain.Checked)
            {
                return;
            }

            ListViewSorter.SetSortArrow(listViewParts.Columns[3], SortOrder.None);
            MoveToTop(listViewParts);
        }

        private void moveBottomToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (radioButtonJoinPlain.Checked)
            {
                return;
            }

            ListViewSorter.SetSortArrow(listViewParts.Columns[3], SortOrder.None);
            MoveToBottom(listViewParts);
        }

        private void listViewParts_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            if (radioButtonJoinPlain.Checked || e.Column != 3)
            {
                return;
            }

            var lv = (ListView)sender;
            if (!(lv.ListViewItemSorter is ListViewSorter sorter))
            {
                sorter = new ListViewSorter
                {
                    ColumnNumber = e.Column,
                };
                lv.ListViewItemSorter = sorter;
            }

            if (e.Column == sorter.ColumnNumber)
            {
                sorter.Descending = !sorter.Descending; // inverse sort direction
            }
            else
            {
                sorter.ColumnNumber = e.Column;
            }

            lv.Sort();

            ListViewSorter.SetSortArrow(listViewParts.Columns[e.Column], sorter.Descending ? SortOrder.Descending : SortOrder.Ascending);

            _fileNamesToJoin.Clear();
            foreach (ListViewItem item in listViewParts.Items)
            {
                _fileNamesToJoin.Add((string)item.Tag);
            }
            SortAndLoad();
        }
    }
}
