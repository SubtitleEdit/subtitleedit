using System;
using System.Windows.Forms;
using Nikse.SubtitleEdit.Logic;
using System.Collections.Generic;
using System.Drawing;

namespace Nikse.SubtitleEdit.Forms
{
    public sealed partial class FindSubtitleLine : Form
    {
        int _startFindIndex = -1;
        List<Paragraph> _paragraphs = new List<Paragraph>();
        Keys _mainGeneralGoToNextSubtitle = Utilities.GetKeys(Configuration.Settings.Shortcuts.GeneralGoToNextSubtitle);
        Keys _mainGeneralGoToPrevSubtitle = Utilities.GetKeys(Configuration.Settings.Shortcuts.GeneralGoToPrevSubtitle);

        public int SelectedIndex
        {
            get;
            set;
        }

        public FindSubtitleLine()
        {
            InitializeComponent();

            Text = Configuration.Settings.Language.FindSubtitleLine.Title;
            buttonFind.Text = Configuration.Settings.Language.FindSubtitleLine.Find;
            buttonFindNext.Text = Configuration.Settings.Language.FindSubtitleLine.FindNext;
            buttonOK.Text = Configuration.Settings.Language.General.OK;
            buttonCancel.Text = Configuration.Settings.Language.General.Cancel;
            subtitleListView1.InitializeLanguage(Configuration.Settings.Language.General, Configuration.Settings);
            FixLargeFonts();
        }

        private void FixLargeFonts()
        {
            Graphics graphics = this.CreateGraphics();
            SizeF textSize = graphics.MeasureString(buttonOK.Text, this.Font);
            if (textSize.Height > buttonOK.Height - 4)
            {
                subtitleListView1.InitializeTimeStampColumWidths(this);
                int newButtonHeight = (int)(textSize.Height + 7 + 0.5);
                Utilities.SetButtonHeight(this, newButtonHeight, 1);
            }
        }

        public void Initialize(List<Paragraph> paragraphs, string appendTitle)
        {
            Text += appendTitle;
            _paragraphs = paragraphs;
            subtitleListView1.Fill(paragraphs);
            _startFindIndex = -1;
        }

        private void ButtonFindClick(object sender, EventArgs e)
        {
            _startFindIndex = -1;
            FindText();
        }

        private void FindText()
        {
            if (textBoxFindText.Text.Trim().Length > 0)
            {
                int index = 0;
                foreach (Paragraph p in _paragraphs)
                {
                    if (index > _startFindIndex)
                    {
                        if (p.Text.ToLower().Contains(textBoxFindText.Text.ToLower()))
                        {
                            subtitleListView1.Items[index].Selected = true;
                            subtitleListView1.HideSelection = false;
                            subtitleListView1.Items[index].EnsureVisible();
                            subtitleListView1.Items[index].Focused = true;
                            _startFindIndex = index;
                            return;
                        }
                    }
                    index++;
                }
            }
        }

        private void ButtonCancelClick(object sender, EventArgs e)
        {
            SelectedIndex = -1;
        }

        private void ButtonOkClick(object sender, EventArgs e)
        {
            if (subtitleListView1.SelectedItems.Count > 0)
            {
                SelectedIndex = subtitleListView1.SelectedItems[0].Index;
            }
            else
            {
                SelectedIndex = -1;
            }
        }

        private void TextBoxFindTextKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                ButtonFindClick(sender, null);
        }

        private void TextBoxFindTextTextChanged(object sender, EventArgs e)
        {
            _startFindIndex = -1;
        }

        private void ButtonFindNextClick(object sender, EventArgs e)
        {
            FindText();
        }

        private void FormFindSubtitleLine_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F3)
                FindText();
            else if (e.KeyCode == Keys.Escape)
            {
                DialogResult = DialogResult.Cancel;
            }
            else if (_mainGeneralGoToNextSubtitle == e.KeyData || (e.KeyCode == Keys.Down && e.Modifiers == Keys.Alt))
            {
                int selectedIndex = 0;
                if (subtitleListView1.SelectedItems.Count > 0)
                {
                    selectedIndex = subtitleListView1.SelectedItems[0].Index;
                    selectedIndex++;
                }
                subtitleListView1.SelectIndexAndEnsureVisible(selectedIndex);
            }
            else if (_mainGeneralGoToPrevSubtitle == e.KeyData || (e.KeyCode == Keys.Up && e.Modifiers == Keys.Alt))
            {
                int selectedIndex = 0;
                if (subtitleListView1.SelectedItems.Count > 0)
                {
                    selectedIndex = subtitleListView1.SelectedItems[0].Index;
                    selectedIndex--;
                }
                subtitleListView1.SelectIndexAndEnsureVisible(selectedIndex);
            }
            else if (e.KeyCode == Keys.Home && e.Alt)
            {
                subtitleListView1.SelectIndexAndEnsureVisible(0);
                e.SuppressKeyPress = true;
            }
            else if (e.KeyCode == Keys.End && e.Alt)
            {
                subtitleListView1.SelectIndexAndEnsureVisible(subtitleListView1.Items.Count - 1);
                e.SuppressKeyPress = true;
            }
            else if (e.Modifiers == Keys.Control && e.KeyCode == Keys.G && subtitleListView1.Items.Count > 1)
            {
                var goToLine = new GoToLine();
                goToLine.Initialize(1, subtitleListView1.Items.Count);
                if (goToLine.ShowDialog(this) == DialogResult.OK)
                {
                    subtitleListView1.SelectNone();
                    subtitleListView1.Items[goToLine.LineNumber - 1].Selected = true;
                    subtitleListView1.Items[goToLine.LineNumber - 1].EnsureVisible();
                    subtitleListView1.Items[goToLine.LineNumber - 1].Focused = true;
                }
            }
        }

        private void FormFindSubtitleLine_Load(object sender, EventArgs e)
        {
            if (textBoxFindText.CanFocus)
                textBoxFindText.Focus();
        }

        private void FormFindSubtitleLine_Shown(object sender, EventArgs e)
        {
            if (textBoxFindText.CanFocus)
                textBoxFindText.Focus();
        }

        private void SubtitleListView1MouseDoubleClick(object sender, MouseEventArgs e)
        {
            ButtonOkClick(null, null);
            Close();
        }

    }
}
