using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Logic;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.Forms
{
    public partial class AutoBreakUnbreakLines : PositionAndSizeForm
    {
        private List<Paragraph> _paragraphs;
        private bool _modeAutoBalance;
        private HashSet<string> _notAllowedFixes = new HashSet<string>();

        private Dictionary<string, string> _fixedText = new Dictionary<string, string>();

        private string _language;

        public Dictionary<string, string> FixedText => _fixedText;

        public AutoBreakUnbreakLines()
        {
            UiUtil.PreInitialize(this);
            InitializeComponent();
            UiUtil.FixFonts(this);
            groupBoxLinesFound.Text = string.Empty;
            listViewFixes.Columns[2].Width = 290;
            listViewFixes.Columns[3].Width = 290;

            listViewFixes.Columns[0].Text = LanguageSettings.Current.General.Apply;
            listViewFixes.Columns[1].Text = LanguageSettings.Current.General.LineNumber;
            listViewFixes.Columns[2].Text = LanguageSettings.Current.General.Before;
            listViewFixes.Columns[3].Text = LanguageSettings.Current.General.After;
            toolStripMenuItemInverseSelection.Text = LanguageSettings.Current.Main.Menu.Edit.InverseSelection;
            toolStripMenuItemSelectAll.Text = LanguageSettings.Current.Main.Menu.ContextMenu.SelectAll;
            buttonOK.Text = LanguageSettings.Current.General.Ok;
            buttonCancel.Text = LanguageSettings.Current.General.Cancel;
            UiUtil.FixLargeFonts(this, buttonOK);
        }

        public void Initialize(Subtitle subtitle, bool autoBalance)
        {
            _language = LanguageAutoDetect.AutoDetectGoogleLanguage(subtitle);
            _modeAutoBalance = autoBalance;
            _paragraphs = new List<Paragraph>();

            foreach (Paragraph p in subtitle.Paragraphs)
            {
                _paragraphs.Add(p);
            }

            if (autoBalance)
            {
                labelCondition.Text = LanguageSettings.Current.AutoBreakUnbreakLines.OnlyBreakLinesLongerThan;
                const int start = 10;
                const int max = 60;
                for (int i = start; i <= max; i++)
                {
                    comboBoxConditions.Items.Add(i.ToString(CultureInfo.InvariantCulture));
                    if (i == Configuration.Settings.General.MergeLinesShorterThan - 1)
                    {
                        comboBoxConditions.SelectedIndex = comboBoxConditions.Items.Count - 1;
                    }
                }

                if (comboBoxConditions.SelectedIndex < 0)
                {
                    comboBoxConditions.SelectedIndex = 30;
                }

                AutoBalance();
            }
            else
            {
                labelCondition.Text = LanguageSettings.Current.AutoBreakUnbreakLines.OnlyUnbreakLinesLongerThan;
                for (int i = 5; i < 51; i++)
                {
                    comboBoxConditions.Items.Add(i.ToString(CultureInfo.InvariantCulture));
                    if (i == Configuration.Settings.General.MergeLinesShorterThan - 1)
                    {
                        comboBoxConditions.SelectedIndex = comboBoxConditions.Items.Count - 1;
                    }
                }
                if (comboBoxConditions.SelectedIndex < 0)
                {
                    comboBoxConditions.SelectedIndex = 5;
                }
                Unbreak();
            }
            comboBoxConditions.SelectedIndexChanged += ComboBoxConditionsSelectedIndexChanged;
        }

        public int MinimumLength
        {
            get
            {
                return int.Parse(comboBoxConditions.Items[comboBoxConditions.SelectedIndex].ToString());
            }
        }

        public int MergeLinesShorterThan
        {
            get
            {
                if (Configuration.Settings.General.MergeLinesShorterThan > MinimumLength)
                {
                    return MinimumLength - 1;
                }

                return Configuration.Settings.General.MergeLinesShorterThan;
            }
        }

        private void AutoBalance()
        {
            listViewFixes.ItemChecked -= listViewFixes_ItemChecked;
            _notAllowedFixes = new HashSet<string>();
            _fixedText = new Dictionary<string, string>();
            int minLength = MinimumLength;
            Text = LanguageSettings.Current.AutoBreakUnbreakLines.TitleAutoBreak;


            listViewFixes.BeginUpdate();
            listViewFixes.Items.Clear();
            foreach (Paragraph p in _paragraphs)
            {
                if (HtmlUtil.RemoveHtmlTags(p.Text, true).Length > minLength || p.Text.Contains(Environment.NewLine))
                {
                    var text = Utilities.AutoBreakLine(p.Text, 5, MergeLinesShorterThan, _language);
                    if (text != p.Text)
                    {
                        AddToListView(p, text);
                        _fixedText.Add(p.Id, text);
                    }
                }
            }
            listViewFixes.EndUpdate();
            groupBoxLinesFound.Text = string.Format(LanguageSettings.Current.AutoBreakUnbreakLines.LinesFoundX, listViewFixes.Items.Count);
            listViewFixes.ItemChecked += listViewFixes_ItemChecked;
        }

        private void Unbreak()
        {
            listViewFixes.ItemChecked -= listViewFixes_ItemChecked;
            _notAllowedFixes = new HashSet<string>();
            _fixedText = new Dictionary<string, string>();
            int minLength = int.Parse(comboBoxConditions.Items[comboBoxConditions.SelectedIndex].ToString());
            Text = LanguageSettings.Current.AutoBreakUnbreakLines.TitleUnbreak;
            listViewFixes.BeginUpdate();
            listViewFixes.Items.Clear();
            foreach (Paragraph p in _paragraphs)
            {
                if (p.Text.Contains(Environment.NewLine) && HtmlUtil.RemoveHtmlTags(p.Text, true).Length > minLength)
                {
                    var text = Utilities.UnbreakLine(p.Text);
                    if (text != p.Text)
                    {
                        AddToListView(p, text);
                        _fixedText.Add(p.Id, text);
                    }
                }
            }
            listViewFixes.EndUpdate();
            groupBoxLinesFound.Text = string.Format(LanguageSettings.Current.AutoBreakUnbreakLines.LinesFoundX, listViewFixes.Items.Count);
            listViewFixes.ItemChecked += listViewFixes_ItemChecked;
        }

        private void AutoBreakUnbreakLinesKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                DialogResult = DialogResult.Cancel;
            }
        }

        private void AutoBreakUnbreakLines_ResizeEnd(object sender, EventArgs e)
        {
            listViewFixes.AutoSizeLastColumn();
        }

        private void AutoBreakUnbreakLines_Shown(object sender, EventArgs e)
        {
            AutoBreakUnbreakLines_ResizeEnd(sender, e);
        }

        private void AddToListView(Paragraph p, string newText)
        {
            var item = new ListViewItem(string.Empty) { Tag = p, Checked = true };
            item.SubItems.Add(p.Number.ToString(CultureInfo.InvariantCulture));
            item.SubItems.Add(UiUtil.GetListViewTextFromString(p.Text));
            item.SubItems.Add(UiUtil.GetListViewTextFromString(newText));
            listViewFixes.Items.Add(item);
        }

        private void ButtonOkClick(object sender, EventArgs e)
        {
            for (int i = _paragraphs.Count - 1; i >= 0; i--)
            {
                var p = _paragraphs[i];
                if (_notAllowedFixes.Contains(p.Id))
                {
                    _fixedText.Remove(p.Id);
                }
            }
            DialogResult = DialogResult.OK;
        }

        private void ComboBoxConditionsSelectedIndexChanged(object sender, EventArgs e)
        {
            if (_modeAutoBalance)
            {
                AutoBalance();
            }
            else
            {
                Unbreak();
            }
        }

        private void listViewFixes_ItemChecked(object sender, ItemCheckedEventArgs e)
        {
            if (e.Item == null)
            {
                return;
            }

            var p = e.Item.Tag as Paragraph;
            if (p == null)
            {
                return;
            }

            if (e.Item.Checked)
            {
                _notAllowedFixes.Remove(p.Id);
            }
            else
            {
                _notAllowedFixes.Add(p.Id);
            }
        }

        private void listViewFixes_Resize(object sender, EventArgs e)
        {
            var newWidth = (listViewFixes.Width - (listViewFixes.Columns[0].Width + listViewFixes.Columns[1].Width)) / 2;
            listViewFixes.Columns[3].Width = listViewFixes.Columns[2].Width = newWidth;
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
