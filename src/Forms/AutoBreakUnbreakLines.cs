using System;
using System.Windows.Forms;
using Nikse.SubtitleEdit.Logic;
using System.Collections.Generic;
using System.Drawing;

namespace Nikse.SubtitleEdit.Forms
{
    public partial class AutoBreakUnbreakLines : Form
    {
        List<Paragraph> _paragraphs;
        int _changes;
        bool _modeAutoBalance = false;

        public List<Paragraph> FixedParagraphs
        {
            get
            {
                return _paragraphs;
            }
        }

        public AutoBreakUnbreakLines()
        {
            InitializeComponent();
            groupBoxLinesFound.Text = string.Empty;
            listViewFixes.Columns[2].Width = 290;
            listViewFixes.Columns[3].Width = 290;

            listViewFixes.Columns[0].Text = Configuration.Settings.Language.General.Apply;
            listViewFixes.Columns[1].Text = Configuration.Settings.Language.AutoBreakUnbreakLines.LineNumber;
            listViewFixes.Columns[2].Text = Configuration.Settings.Language.AutoBreakUnbreakLines.Before;
            listViewFixes.Columns[3].Text = Configuration.Settings.Language.AutoBreakUnbreakLines.After;
            buttonOK.Text = Configuration.Settings.Language.General.OK;
            buttonCancel.Text = Configuration.Settings.Language.General.Cancel;
            FixLargeFonts();
        }

        private void FixLargeFonts()
        {
            Graphics graphics = this.CreateGraphics();
            SizeF textSize = graphics.MeasureString(buttonOK.Text, this.Font);
            if (textSize.Height > buttonOK.Height - 4)
            {
                int newButtonHeight = (int)(textSize.Height + 7 + 0.5);
                Utilities.SetButtonHeight(this, newButtonHeight, 1);
            }
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
                    comboBoxConditions.Items.Add(i.ToString());

                int index = Configuration.Settings.Tools.MergeLinesShorterThan - (start +1);
                if (index > 0 && index < max)
                    comboBoxConditions.SelectedIndex = index;
                else
                    comboBoxConditions.SelectedIndex = 30;

                AutoBalance();
            }
            else
            {
                labelCondition.Text = Configuration.Settings.Language.AutoBreakUnbreakLines.OnlyUnbreakLinesShorterThan;
                for (int i = 5; i < 51; i++)
                    comboBoxConditions.Items.Add(i.ToString());
                comboBoxConditions.SelectedIndex = 5;

                Unbreak();
            }
        }

        private void AutoBalance()
        {
            int minLength = int.Parse(comboBoxConditions.Items[comboBoxConditions.SelectedIndex].ToString());
            Text = Configuration.Settings.Language.AutoBreakUnbreakLines.TitleAutoBreak;
            listViewFixes.BeginUpdate();
            listViewFixes.Items.Clear();
            foreach (Paragraph p in _paragraphs)
            {
                if (p.Text.Length > minLength || p.Text.Contains(Environment.NewLine))
                {
                    string text = Utilities.AutoBreakLine(p.Text);
                    if (text != p.Text)
                    {
                        AddToListView(p, text);
                        _changes++;
                    }
                }
            }
            listViewFixes.EndUpdate();
            groupBoxLinesFound.Text = string.Format(Configuration.Settings.Language.AutoBreakUnbreakLines.LinesFoundX, listViewFixes.Items.Count);
        }

        private void Unbreak()
        {
            int minLength = int.Parse(comboBoxConditions.Items[comboBoxConditions.SelectedIndex].ToString());
            Text = Configuration.Settings.Language.AutoBreakUnbreakLines.TitleUnbreak;
            listViewFixes.BeginUpdate();
            listViewFixes.Items.Clear();
            foreach (Paragraph p in _paragraphs)
            {
                if (p.Text != null && p.Text.Contains(Environment.NewLine) && p.Text.Length > minLength)
                {
                    string text = p.Text.Replace(Environment.NewLine, " ");
                    while (text.Contains("  "))
                        text = text.Replace("  ", " ");

                    if (text != p.Text)
                    {
                        AddToListView(p, text);
                        _changes++;
                    }
                }
            }
            listViewFixes.EndUpdate();
            groupBoxLinesFound.Text = string.Format(Configuration.Settings.Language.AutoBreakUnbreakLines.LinesFoundX, listViewFixes.Items.Count);
        }

        private void AutoBreakUnbreakLines_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
                DialogResult = DialogResult.Cancel;
        }

        private void AddToListView(Paragraph p, string newText)
        {
            var item = new ListViewItem(string.Empty) {Tag = p, Checked = true};

            var subItem = new ListViewItem.ListViewSubItem(item, p.Number.ToString());
            item.SubItems.Add(subItem);
            subItem = new ListViewItem.ListViewSubItem(item, p.Text.Replace(Environment.NewLine, Configuration.Settings.General.ListViewLineSeparatorString));
            item.SubItems.Add(subItem);
            subItem = new ListViewItem.ListViewSubItem(item, newText.Replace(Environment.NewLine, Configuration.Settings.General.ListViewLineSeparatorString));
            item.SubItems.Add(subItem);

            listViewFixes.Items.Add(item);
        }

        private bool IsFixAllowed(Paragraph p)
        {
            foreach (ListViewItem item in listViewFixes.Items)
            {
                if (item.Tag.ToString() == p.ToString())
                    return item.Checked;
            }
            return false;
        }

        private void ButtonOkClick(object sender, EventArgs e)
        {
            for (int i = _paragraphs.Count - 1; i > 0; i--)
            {
                Paragraph p = _paragraphs[i];
                if (!IsFixAllowed(p))
                    _paragraphs.Remove(p);
            }
            DialogResult = DialogResult.OK;
        }

        private void comboBoxConditions_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_modeAutoBalance)
                AutoBalance();
            else
                Unbreak();
        }
    }
}
