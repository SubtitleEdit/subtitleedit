using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Logic;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.Forms
{
    public partial class ConvertActor : PositionAndSizeForm
    {
        public Subtitle Subtitle { get; set; }
        private SubtitleFormat _subtitleFormat;
        private bool _loading = true;
        private List<FixListItem> _fixItems;

        public class FixListItem
        {
            public Paragraph Paragraph { get; set; }
            public bool Checked { get; set; }
            public bool IsNext { get; set; }
        }

        public int NumberOfActorConversions { get; private set; }

        public ConvertActor()
        {
            UiUtil.PreInitialize(this);
            InitializeComponent();
            UiUtil.FixFonts(this);
            _fixItems = new List<FixListItem>();
            UiUtil.FixLargeFonts(this, buttonOK);
        }

        public void Initialize(Subtitle subtitle, SubtitleFormat subtitleFormat)
        {
            Subtitle = new Subtitle(subtitle, false);
            _subtitleFormat = subtitleFormat;

            Text = LanguageSettings.Current.ConvertActor.Title;

            listViewFixes.Columns[0].Text = LanguageSettings.Current.General.Apply;
            listViewFixes.Columns[1].Text = LanguageSettings.Current.General.LineNumber;
            listViewFixes.Columns[2].Text = LanguageSettings.Current.General.Before;
            listViewFixes.Columns[3].Text = LanguageSettings.Current.General.After;

            buttonOK.Text = LanguageSettings.Current.General.Ok;
            buttonCancel.Text = LanguageSettings.Current.General.Cancel;

            checkBoxColor.Checked = Configuration.Settings.Tools.ConvertActorColorAdd;
            panelColor.BackColor = Configuration.Settings.Tools.ConvertActorColor;
            checkBoxChangeCasing.Checked = Configuration.Settings.Tools.ConvertActorCasing;

            labelConvertFrom.Text = LanguageSettings.Current.ConvertActor.ConvertActorFrom;
            labelConvertTo.Text = LanguageSettings.Current.ConvertActor.ConvertActorTo;
            var left = Math.Max(labelConvertFrom.Right, labelConvertTo.Right) + 5;
            nikseComboBoxConvertFrom.Left = left;
            nikseComboBoxConvertTo.Left = left;

            nikseComboBoxConvertFrom.Items.Clear();
            nikseComboBoxConvertFrom.Items.Add(string.Format(LanguageSettings.Current.ConvertActor.InlineActorViaX, "[]"));
            nikseComboBoxConvertFrom.Items.Add(string.Format(LanguageSettings.Current.ConvertActor.InlineActorViaX, "()"));
            nikseComboBoxConvertFrom.Items.Add(string.Format(LanguageSettings.Current.ConvertActor.InlineActorViaX, ":"));
            nikseComboBoxConvertFrom.Items.Add(LanguageSettings.Current.General.Actor);
            if (_subtitleFormat.Name == AdvancedSubStationAlpha.NameOfFormat)
            {
                nikseComboBoxConvertFrom.SelectedIndex = 3;
            }
            else
            {
                nikseComboBoxConvertFrom.SelectedIndex = 0;
            }


            nikseComboBoxConvertTo.Items.Clear();
            nikseComboBoxConvertTo.Items.Add(string.Format(LanguageSettings.Current.ConvertActor.InlineActorViaX, "[]"));
            nikseComboBoxConvertTo.Items.Add(string.Format(LanguageSettings.Current.ConvertActor.InlineActorViaX, "()"));
            nikseComboBoxConvertTo.Items.Add(string.Format(LanguageSettings.Current.ConvertActor.InlineActorViaX, ":"));
            nikseComboBoxConvertTo.Items.Add(LanguageSettings.Current.General.Actor);
            if (nikseComboBoxConvertFrom.SelectedIndex == 0)
            {
                nikseComboBoxConvertTo.SelectedIndex = 1;
            }
            else
            {
                nikseComboBoxConvertTo.SelectedIndex = 0;
            }

            nikseComboBoxCasing.Items.Clear();
            nikseComboBoxCasing.Items.Add(LanguageSettings.Current.ChangeCasing.NormalCasing);
            nikseComboBoxCasing.Items.Add(LanguageSettings.Current.ChangeCasing.AllUppercase);
            nikseComboBoxCasing.Items.Add(LanguageSettings.Current.ChangeCasing.AllLowercase);
            nikseComboBoxCasing.Items.Add(LanguageSettings.Current.ChangeCasing.ProperCase);
            nikseComboBoxCasing.SelectedIndex = 0;
        }

        private ListViewItem MakeListViewItem(Paragraph p, bool selected, int lineNumber, string newText, string oldText, bool isNext)
        {
            var fixItem = new FixListItem { Checked = true, Paragraph = p, IsNext = isNext };
            _fixItems.Add(fixItem);

            var item = new ListViewItem(string.Empty) { Tag = p, Checked = selected || !checkBoxOnlyNames.Checked };
            item.SubItems.Add(lineNumber.ToString());
            item.SubItems.Add(UiUtil.GetListViewTextFromString(oldText));
            item.SubItems.Add(UiUtil.GetListViewTextFromString(newText));
            return item;
        }

        private void GeneratePreview()
        {
            if (Subtitle == null || _loading)
            {
                return;
            }

            ConvertActors(new Subtitle(Subtitle, false), out var numberOfConversions, true);
            NumberOfActorConversions = numberOfConversions;

            groupBoxLinesFound.Text = string.Format(LanguageSettings.Current.ConvertActor.NumberOfConversionsX, NumberOfActorConversions);
        }

        public Subtitle ConvertActors(Subtitle subtitle, out int numberOfConversions, bool clearFixes)
        {
            if (!_loading)
            {
                listViewFixes.ItemChecked -= listViewFixes_ItemChecked;
            }

            if (clearFixes)
            {
                listViewFixes.Items.Clear();
                _fixItems = new List<FixListItem>();
            }

            numberOfConversions = 0;
            var convertedSubtitle = new Subtitle();
            Paragraph p = null;
            var lineNumbers = new List<int>();
            var listViewItems = new List<ListViewItem>();
            var languageCode = LanguageAutoDetect.AutoDetectGoogleLanguage(subtitle);   

            var converter = new ActorConverter(_subtitleFormat, languageCode);

            var from = nikseComboBoxConvertFrom.SelectedText;
            var to = nikseComboBoxConvertTo.SelectedText;
            var fromSquare = from == string.Format(LanguageSettings.Current.ConvertActor.InlineActorViaX, "[]");
            converter.ToSquare = to == string.Format(LanguageSettings.Current.ConvertActor.InlineActorViaX, "[]");
            var fromParentheses = from == string.Format(LanguageSettings.Current.ConvertActor.InlineActorViaX, "()");
            converter.ToParentheses = to == string.Format(LanguageSettings.Current.ConvertActor.InlineActorViaX, "()");
            var fromColon = from == string.Format(LanguageSettings.Current.ConvertActor.InlineActorViaX, ":");
            converter.ToColon = to == string.Format(LanguageSettings.Current.ConvertActor.InlineActorViaX, ":");
            var fromActor = from == LanguageSettings.Current.General.Actor;
            converter.ToActor = to == LanguageSettings.Current.General.Actor;
            var changeCasing = checkBoxChangeCasing.Checked ? nikseComboBoxCasing.SelectedIndex : (int?)null;
            var color = checkBoxColor.Checked ? panelColor.BackColor : (Color?)null;

            for (var i = 0; i < subtitle.Paragraphs.Count; i++)
            {
                p = subtitle.Paragraphs[i];
                var oldText = p.Text;

                if (fromSquare && Contains(p.Text, '[', ']'))
                {
                    var result = converter.FixActors(p, '[', ']', changeCasing, color);
                    if (result.Skip)
                    {
                        continue;
                    }

                    p.Text = result.Paragraph.Text;
                    p.Actor = result.Paragraph.Actor;
                    listViewItems.Add(MakeListViewItem(p, result.Selected, p.Number, p.Text, oldText, false));
                    numberOfConversions++;

                    if (converter.ToActor && result.NextParagraph != null)
                    {
                        listViewItems.Add(MakeListViewItem(result.NextParagraph, result.Selected, p.Number, result.NextParagraph.Text, oldText, true));
                    }
                }
                else if (fromParentheses && Contains(p.Text, '(', ')'))
                {
                    var result = converter.FixActors(p, '(', ')', changeCasing, color);
                    if (result.Skip)
                    {
                        continue;
                    }

                    p.Text = result.Paragraph.Text;
                    p.Actor = result.Paragraph.Actor;
                    listViewItems.Add(MakeListViewItem(p, result.Selected, p.Number, p.Text, oldText, false));
                    numberOfConversions++;

                    if (converter.ToActor && result.NextParagraph != null)
                    {
                        listViewItems.Add(MakeListViewItem(result.NextParagraph, result.Selected, p.Number, result.NextParagraph.Text, oldText, true));
                    }
                }
                else if (fromColon && p.Text.Contains(':'))
                {
                    var result = converter.FixActorsFromBeforeColon(p, ':', changeCasing, color);
                    p.Text = result;
                    listViewItems.Add(MakeListViewItem(p, true, p.Number, p.Text, oldText, false));
                    numberOfConversions++;
                }
                else if (fromActor && !string.IsNullOrEmpty(p.Actor))
                {
                    p.Text = converter.FixActorsFromActor(p, changeCasing, color);
                    listViewItems.Add(MakeListViewItem(p, true, p.Number, p.Text, oldText, false));
                    numberOfConversions++;
                }
                else
                {
                    continue;
                }
            }

            listViewFixes.Items.AddRange(listViewItems.ToArray());

            if (!_loading)
            {
                listViewFixes.ItemChecked += listViewFixes_ItemChecked;
            }

            convertedSubtitle.Renumber();
            return convertedSubtitle;
        }


        private bool Contains(string text, char start, char end)
        {
            if (string.IsNullOrEmpty(text))
            {
                return false;
            }

            var startIdx = text.IndexOf(start);
            if (startIdx < 0)
            {
                return false;
            }

            var endIdx = text.IndexOf(end);
            return endIdx > startIdx;
        }

        private void ButtonCancelClick(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void ButtonOkClick(object sender, EventArgs e)
        {
            foreach (var fixItem in _fixItems)
            {
                if (!fixItem.Checked)
                {
                    continue;
                }

                if (fixItem.IsNext)
                {
                    Subtitle.InsertParagraphInCorrectTimeOrder(fixItem.Paragraph);
                }
                else
                {
                    foreach (var p in Subtitle.Paragraphs)
                    {
                        if (p.Id == fixItem.Paragraph.Id)
                        {
                            p.Text = fixItem.Paragraph.Text;
                            p.Actor = fixItem.Paragraph.Actor;
                            p.Style = fixItem.Paragraph.Style;
                            p.Extra = fixItem.Paragraph.Extra;
                            break;
                        }
                    }
                }
            }

            Subtitle.Renumber();

            DialogResult = DialogResult.OK;
        }

        private void listViewFixes_SelectedIndexChanged(object sender, EventArgs e)
        {
        }

        private void listViewFixes_ItemChecked(object sender, ItemCheckedEventArgs e)
        {
            if (_loading)
            {
                return;
            }

            _fixItems[e.Item.Index].Checked = e.Item.Checked;
            NumberOfActorConversions = 0;
            groupBoxLinesFound.Text = string.Format(LanguageSettings.Current.ConvertActor.NumberOfConversionsX, NumberOfActorConversions);
        }


        private void ConvertActor_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                DialogResult = DialogResult.Cancel;
            }
        }

        private void ConvertActor_Shown(object sender, EventArgs e)
        {
            ConvertActor_ResizeEnd(sender, e);

            _loading = false;

            GeneratePreview();
            listViewFixes.Focus();
            if (listViewFixes.Items.Count > 0)
            {
                listViewFixes.Items[0].Selected = true;
            }

            listViewFixes.ItemChecked += listViewFixes_ItemChecked;
        }

        private void checkBoxFixIncrementing_CheckedChanged(object sender, EventArgs e)
        {
            Cursor = Cursors.WaitCursor;
            GeneratePreview();
            Cursor = Cursors.Default;
        }

        private void ConvertActor_ResizeEnd(object sender, EventArgs e)
        {
            columnHeaderAfter.Width = -2;
        }

        private void toolStripMenuItemSelectAll_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in listViewFixes.Items)
            {
                _fixItems[item.Index].Checked = true;
                item.Checked = true;
            }
        }

        private void toolStripMenuItemInverseSelection_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in listViewFixes.Items)
            {
                _fixItems[item.Index].Checked = !item.Checked;
                item.Checked = !item.Checked;
            }
        }

        private void ConvertActor_FormClosing(object sender, FormClosingEventArgs e)
        {
            Configuration.Settings.Tools.ConvertActorColorAdd = checkBoxColor.Checked;
            Configuration.Settings.Tools.ConvertActorColor = panelColor.BackColor;
            Configuration.Settings.Tools.ConvertActorCasing = checkBoxChangeCasing.Checked;
        }

        private void buttonColor_Click(object sender, EventArgs e)
        {
            using (var colorChooser = new ColorChooser { Color = panelColor.BackColor })
            {
                if (colorChooser.ShowDialog() == DialogResult.OK)
                {
                    panelColor.BackColor = colorChooser.Color;
                    GeneratePreview();
                }
            }
        }

        private void panelColor_MouseClick(object sender, MouseEventArgs e)
        {
            buttonColor_Click(sender, e);
        }

        private void nikseComboBoxConvertFrom_SelectedIndexChanged(object sender, EventArgs e)
        {
            GeneratePreview();
        }

        private void nikseComboBoxConvertTo_SelectedIndexChanged(object sender, EventArgs e)
        {
            GeneratePreview();
        }

        private void checkBoxChangeCasing_CheckedChanged(object sender, EventArgs e)
        {
            GeneratePreview();
        }

        private void nikseComboBoxCasing_SelectedIndexChanged(object sender, EventArgs e)
        {
            GeneratePreview();
        }

        private void checkBoxOnlyNames_CheckedChanged(object sender, EventArgs e)
        {
            GeneratePreview();
        }
    }
}
