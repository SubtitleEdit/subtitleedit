﻿using System;
using System.Globalization;
using System.Windows.Forms;
using Nikse.SubtitleEdit.Logic;
using System.Collections.Generic;
using Nikse.SubtitleEdit.Core;

namespace Nikse.SubtitleEdit.Forms
{
    public partial class AutoBreakUnbreakLines : PositionAndSizeForm
    {
        private List<Paragraph> _paragraphs;
        private int _changes;
        private bool _modeAutoBalance;
        private HashSet<string> _notAllowedFixes = new HashSet<string>();

        private Dictionary<string, string> _fixedText = new Dictionary<string, string>();
        public Dictionary<string, string> FixedText
        {
            get { return _fixedText; }
        }

        public int Changes
        {
            get { return _changes; }
        }

        public AutoBreakUnbreakLines()
        {
            InitializeComponent();
            groupBoxLinesFound.Text = string.Empty;
            listViewFixes.Columns[2].Width = 290;
            listViewFixes.Columns[3].Width = 290;

            listViewFixes.Columns[0].Text = Configuration.Settings.Language.General.Apply;
            listViewFixes.Columns[1].Text = Configuration.Settings.Language.General.LineNumber;
            listViewFixes.Columns[2].Text = Configuration.Settings.Language.General.Before;
            listViewFixes.Columns[3].Text = Configuration.Settings.Language.General.After;
            buttonOK.Text = Configuration.Settings.Language.General.Ok;
            buttonCancel.Text = Configuration.Settings.Language.General.Cancel;
            Utilities.FixLargeFonts(this, buttonOK);
        }

        public void Initialize(Subtitle subtitle, bool autoBalance)
        {
            _modeAutoBalance = autoBalance;
            _paragraphs = new List<Paragraph>();

            foreach (Paragraph p in subtitle.Paragraphs)
                _paragraphs.Add(p);

            if (autoBalance)
            {
                labelCondition.Text = Configuration.Settings.Language.AutoBreakUnbreakLines.OnlyBreakLinesLongerThan;
                const int start = 10;
                const int max = 60;
                for (int i = start; i <= max; i++)
                    comboBoxConditions.Items.Add(i.ToString(CultureInfo.InvariantCulture));

                int index = Configuration.Settings.Tools.MergeLinesShorterThan - (start + 1);
                if (index > 0 && index < max)
                    comboBoxConditions.SelectedIndex = index;
                else
                    comboBoxConditions.SelectedIndex = 30;

                AutoBalance();
            }
            else
            {
                labelCondition.Text = Configuration.Settings.Language.AutoBreakUnbreakLines.OnlyUnbreakLinesLongerThan;
                for (int i = 5; i < 51; i++)
                    comboBoxConditions.Items.Add(i.ToString(CultureInfo.InvariantCulture));
                comboBoxConditions.SelectedIndex = 5;

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
                if (Configuration.Settings.Tools.MergeLinesShorterThan > MinimumLength)
                    return MinimumLength - 1;
                return Configuration.Settings.Tools.MergeLinesShorterThan;
            }
        }

        private void AutoBalance()
        {
            listViewFixes.ItemChecked -= listViewFixes_ItemChecked;
            _notAllowedFixes = new HashSet<string>();
            _fixedText = new Dictionary<string, string>();
            int minLength = MinimumLength;
            Text = Configuration.Settings.Language.AutoBreakUnbreakLines.TitleAutoBreak;

            var sub = new Subtitle();
            foreach (Paragraph p in _paragraphs)
                sub.Paragraphs.Add(p);
            var language = Utilities.AutoDetectGoogleLanguage(sub);

            listViewFixes.BeginUpdate();
            listViewFixes.Items.Clear();
            foreach (Paragraph p in _paragraphs)
            {
                if (HtmlUtil.RemoveHtmlTags(p.Text, true).Length > minLength || p.Text.Contains(Environment.NewLine))
                {
                    var text = Utilities.AutoBreakLine(p.Text, 5, MergeLinesShorterThan, language);
                    if (text != p.Text)
                    {
                        AddToListView(p, text);
                        _fixedText.Add(p.ID, text);
                        _changes++;
                    }
                }
            }
            listViewFixes.EndUpdate();
            groupBoxLinesFound.Text = string.Format(Configuration.Settings.Language.AutoBreakUnbreakLines.LinesFoundX, listViewFixes.Items.Count);
            listViewFixes.ItemChecked += listViewFixes_ItemChecked;
        }

        private void Unbreak()
        {
            listViewFixes.ItemChecked -= listViewFixes_ItemChecked;
            _notAllowedFixes = new HashSet<string>();
            _fixedText = new Dictionary<string, string>();
            int minLength = int.Parse(comboBoxConditions.Items[comboBoxConditions.SelectedIndex].ToString());
            Text = Configuration.Settings.Language.AutoBreakUnbreakLines.TitleUnbreak;
            listViewFixes.BeginUpdate();
            listViewFixes.Items.Clear();
            foreach (Paragraph p in _paragraphs)
            {
                if (p.Text != null && p.Text.Contains(Environment.NewLine) && HtmlUtil.RemoveHtmlTags(p.Text).Length > minLength)
                {
                    var text = Utilities.UnbreakLine(p.Text);
                    if (text != p.Text)
                    {
                        AddToListView(p, text);
                        _fixedText.Add(p.ID, text);
                        _changes++;
                    }
                }
            }
            listViewFixes.EndUpdate();
            groupBoxLinesFound.Text = string.Format(Configuration.Settings.Language.AutoBreakUnbreakLines.LinesFoundX, listViewFixes.Items.Count);
            listViewFixes.ItemChecked += listViewFixes_ItemChecked;
        }

        private void AutoBreakUnbreakLinesKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
                DialogResult = DialogResult.Cancel;
        }

        private void AddToListView(Paragraph p, string newText)
        {
            var item = new ListViewItem(string.Empty) { Tag = p, Checked = true };
            item.SubItems.Add(p.Number.ToString(CultureInfo.InvariantCulture));
            item.SubItems.Add(p.Text.Replace(Environment.NewLine, Configuration.Settings.General.ListViewLineSeparatorString));
            item.SubItems.Add(newText.Replace(Environment.NewLine, Configuration.Settings.General.ListViewLineSeparatorString));
            listViewFixes.Items.Add(item);
        }

        private void ButtonOkClick(object sender, EventArgs e)
        {
            for (int i = _paragraphs.Count - 1; i >= 0; i--)
            {
                var p = _paragraphs[i];
                if (_notAllowedFixes.Contains(p.ID))
                {
                    _fixedText.Remove(p.ID);
                }
            }
            DialogResult = DialogResult.OK;
        }

        private void ComboBoxConditionsSelectedIndexChanged(object sender, EventArgs e)
        {
            if (_modeAutoBalance)
                AutoBalance();
            else
                Unbreak();
        }

        private void listViewFixes_ItemChecked(object sender, ItemCheckedEventArgs e)
        {
            if (e.Item == null)
                return;

            var p = e.Item.Tag as Paragraph;
            if (p == null)
                return;

            if (e.Item.Checked)
                _notAllowedFixes.Remove(p.ID);
            else
                _notAllowedFixes.Add(p.ID);
        }

        private void listViewFixes_Resize(object sender, EventArgs e)
        {
            var newWidth = (listViewFixes.Width - (listViewFixes.Columns[0].Width + listViewFixes.Columns[1].Width)) / 2;
            listViewFixes.Columns[3].Width = listViewFixes.Columns[2].Width = newWidth;
        }

    }
}