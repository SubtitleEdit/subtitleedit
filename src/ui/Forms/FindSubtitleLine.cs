using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Logic;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.Forms
{
    public sealed partial class FindSubtitleLine : Form
    {
        private int _startFindIndex = -1;
        private List<Paragraph> _paragraphs = new List<Paragraph>();
        private readonly Keys _mainGeneralGoToNextSubtitle = UiUtil.GetKeys(Configuration.Settings.Shortcuts.GeneralGoToNextSubtitle);
        private readonly Keys _mainGeneralGoToNextSubtitlePlayTranslate = UiUtil.GetKeys(Configuration.Settings.Shortcuts.GeneralGoToNextSubtitlePlayTranslate);
        private readonly Keys _mainGeneralGoToPrevSubtitle = UiUtil.GetKeys(Configuration.Settings.Shortcuts.GeneralGoToPrevSubtitle);
        private readonly Keys _mainGeneralGoToPrevSubtitlePlayTranslate = UiUtil.GetKeys(Configuration.Settings.Shortcuts.GeneralGoToPrevSubtitlePlayTranslate);

        public int SelectedIndex
        {
            get;
            private set;
        }

        public FindSubtitleLine()
        {
            UiUtil.PreInitialize(this);
            InitializeComponent();
            UiUtil.FixFonts(this);
            Icon = Properties.Resources.SEIcon;

            Text = LanguageSettings.Current.FindSubtitleLine.Title;
            buttonFind.Text = LanguageSettings.Current.FindSubtitleLine.Find;
            buttonFindNext.Text = LanguageSettings.Current.FindSubtitleLine.FindNext;
            buttonOK.Text = LanguageSettings.Current.General.Ok;
            buttonCancel.Text = LanguageSettings.Current.General.Cancel;
            subtitleListView1.InitializeLanguage(LanguageSettings.Current.General, Configuration.Settings);
            UiUtil.InitializeSubtitleFont(subtitleListView1);
            subtitleListView1.AutoSizeAllColumns(this);
            FixLargeFonts();
        }

        private void FixLargeFonts()
        {
            using (var graphics = CreateGraphics())
            {
                var textSize = graphics.MeasureString(buttonOK.Text, Font);
                if (textSize.Height > buttonOK.Height - 4)
                {
                    subtitleListView1.InitializeTimestampColumnWidths(this);
                    int newButtonHeight = (int)(textSize.Height + 7 + 0.5);
                    UiUtil.SetButtonHeight(this, newButtonHeight, 1);
                }
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
            if (string.IsNullOrWhiteSpace(textBoxFindText.Text))
            {
                return;
            }

            for (var index = 0; index < _paragraphs.Count; index++)
            {
                if (index > _startFindIndex
                    && _paragraphs[index].Text.Contains(textBoxFindText.Text, StringComparison.OrdinalIgnoreCase))
                {
                    subtitleListView1.Items[index].Selected = true;
                    subtitleListView1.HideSelection = false;
                    subtitleListView1.Items[index].EnsureVisible();
                    subtitleListView1.Items[index].Focused = true;
                    _startFindIndex = index;
                    return;
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
            {
                ButtonFindClick(sender, null);
            }
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
            {
                FindText();
            }
            else if (e.KeyCode == Keys.Escape)
            {
                DialogResult = DialogResult.Cancel;
            }
            else if (_mainGeneralGoToNextSubtitle == e.KeyData || _mainGeneralGoToNextSubtitlePlayTranslate == e.KeyData)
            {
                int selectedIndex = 0;
                if (subtitleListView1.SelectedItems.Count > 0)
                {
                    selectedIndex = subtitleListView1.SelectedItems[0].Index;
                    selectedIndex++;
                }
                subtitleListView1.SelectIndexAndEnsureVisible(selectedIndex);
            }
            else if (_mainGeneralGoToPrevSubtitle == e.KeyData || _mainGeneralGoToPrevSubtitlePlayTranslate == e.KeyData)
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
                using (var goToLine = new GoToLine())
                {
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
        }

        private void FormFindSubtitleLine_Load(object sender, EventArgs e)
        {
            SetFocus();
        }

        private void FormFindSubtitleLine_Shown(object sender, EventArgs e)
        {
            SetFocus();
        }

        private void SetFocus()
        {
            if (textBoxFindText.CanFocus)
            {
                textBoxFindText.Focus();
            }
        }

        private void SubtitleListView1MouseDoubleClick(object sender, MouseEventArgs e)
        {
            ButtonOkClick(null, null);
            Close();
        }

    }
}
