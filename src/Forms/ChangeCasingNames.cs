using Nikse.SubtitleEdit.Core;
using Nikse.SubtitleEdit.Core.Casing;
using Nikse.SubtitleEdit.Logic;
using System;
using System.Globalization;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.Forms
{
    public sealed partial class ChangeCasingNames : Form
    {
        private Subtitle _subtitle;
        private NameCaseConverter _nameConverter;
        //private const string ExpectedEndChars = " ,.!?:;')]<-\"\r\n";
        private string _language;

        public ChangeCasingNames()
        {
            UiUtil.PreInitialize(this);
            InitializeComponent();
            UiUtil.FixFonts(this);
            labelXLinesSelected.Text = string.Empty;
            Text = Configuration.Settings.Language.ChangeCasingNames.Title;
            groupBoxNames.Text = string.Empty;
            listViewNames.Columns[0].Text = Configuration.Settings.Language.ChangeCasingNames.Enabled;
            listViewNames.Columns[1].Text = Configuration.Settings.Language.ChangeCasingNames.Name;
            groupBoxLinesFound.Text = string.Empty;
            listViewFixes.Columns[0].Text = Configuration.Settings.Language.General.Apply;
            listViewFixes.Columns[1].Text = Configuration.Settings.Language.General.LineNumber;
            listViewFixes.Columns[2].Text = Configuration.Settings.Language.General.Before;
            listViewFixes.Columns[3].Text = Configuration.Settings.Language.General.After;

            buttonSelectAll.Text = Configuration.Settings.Language.FixCommonErrors.SelectAll;
            buttonInverseSelection.Text = Configuration.Settings.Language.FixCommonErrors.InverseSelection;
            labelExtraNames.Text = Configuration.Settings.Language.ChangeCasingNames.ExtraNames;
            buttonAddCustomNames.Text = Configuration.Settings.Language.DvdSubRip.Add;

            buttonOK.Text = Configuration.Settings.Language.General.Ok;
            buttonCancel.Text = Configuration.Settings.Language.General.Cancel;
            listViewFixes.Resize += delegate
            {
                var width = (listViewFixes.Width - (listViewFixes.Columns[0].Width + listViewFixes.Columns[1].Width)) / 2;
                listViewFixes.Columns[2].Width = width;
                listViewFixes.Columns[3].Width = width;
            };
            UiUtil.FixLargeFonts(this, buttonOK);
        }

        public int LinesChanged { get; private set; }

        private void ChangeCasingNames_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
                DialogResult = DialogResult.Cancel;
        }

        public void Initialize(Subtitle subtitle, NameCaseConverter nameConverter)
        {
            _subtitle = subtitle;
            _nameConverter = nameConverter;

            // hook text changed handler
            nameConverter.TextChanged += (s, e) => AddToPreviewListView(e);

            _language = LanguageAutoDetect.AutoDetectGoogleLanguage(_subtitle);
            if (string.IsNullOrEmpty(_language))
                _language = "en_US";

            GeneratePreview();
        }

        private void GeneratePreview()
        {
            Cursor = Cursors.WaitCursor;
            listViewFixes.BeginUpdate();
            listViewFixes.Items.Clear();

            var context = new CasingContext()
            {
                Culture = CultureInfo.CurrentCulture,
                Language = _language
            };

            _nameConverter.Convert(_subtitle, context);
            FindAllNames();

            listViewFixes.EndUpdate();
            groupBoxLinesFound.Text = string.Format(Configuration.Settings.Language.ChangeCasingNames.LinesFoundX, _nameConverter.Count);
            Cursor = Cursors.Default;
        }

        private void AddToPreviewListView(TextChangedEventArgs textChanged)
        {
            Paragraph p = textChanged.Paragraph;
            var item = new ListViewItem(string.Empty) { Tag = textChanged, Checked = true };
            item.SubItems.Add(p.Number.ToString(CultureInfo.InvariantCulture));
            item.SubItems.Add(textChanged.Before.Replace(Environment.NewLine, Configuration.Settings.General.ListViewLineSeparatorString));
            item.SubItems.Add(textChanged.After.Replace(Environment.NewLine, Configuration.Settings.General.ListViewLineSeparatorString));
            listViewFixes.Items.Add(item);
        }

        private void AddCustomNames()
        {
            foreach (string s in textBoxExtraNames.Text.Split(','))
            {
                var name = s.Trim();
                if (name.Length > 1 && !_nameConverter.NamesInSubtitle.Contains(name))
                {
                    _nameConverter.NamesInSubtitle.Add(name);
                }
            }
        }

        private void FindAllNames()
        {
            listViewNames.BeginUpdate();
            foreach (var name in _nameConverter.NamesInSubtitle)
            {
                var lvi = new ListViewItem(string.Empty) { Checked = true };
                lvi.SubItems.Add(name);
                listViewNames.Items.Add(lvi);
            }
            listViewNames.EndUpdate();
            groupBoxNames.Text = string.Format(Configuration.Settings.Language.ChangeCasingNames.NamesFoundInSubtitleX, listViewNames.Items.Count);
        }

        private void ListViewNamesSelectedIndexChanged(object sender, EventArgs e)
        {
            labelXLinesSelected.Text = string.Empty;
            if (listViewNames.SelectedItems.Count != 1)
                return;

            string name = listViewNames.SelectedItems[0].SubItems[1].Text;
            listViewFixes.BeginUpdate();

            foreach (ListViewItem item in listViewFixes.Items)
            {
                item.Selected = false;

                string text = item.SubItems[2].Text.Replace(Configuration.Settings.General.ListViewLineSeparatorString, Environment.NewLine);

                // ignore... could be number
                if (name.Length <= 1 || name.ToLower().Equals(name))
                {
                    continue;
                }

                int idx = text.IndexOf(name, StringComparison.Ordinal);

                // name not present in text
                if (idx < 0)
                {
                    continue;
                }

                if (idx > 0)
                {
                    // check if character before first character in name is non-word boundary char
                    if (!NameCaseConverter.RegexNonWordChar.IsMatch(text[idx - 1].ToString()))
                    {
                        continue;
                    }
                }

                if (idx + name.Length < text.Length)
                {
                    // check the character after last character in *name
                    if (!NameCaseConverter.RegexNonWordChar.IsMatch(text[idx + name.Length].ToString()))
                    {
                        continue;
                    }
                }

                item.Selected = true;
            }

            listViewFixes.EndUpdate();

            if (listViewFixes.SelectedItems.Count > 0)
                listViewFixes.EnsureVisible(listViewFixes.SelectedItems[0].Index);
        }

        private void ListViewNamesItemChecked(object sender, ItemCheckedEventArgs e)
        {
            GeneratePreview();
        }

        private void ChangeCasingNames_Shown(object sender, EventArgs e)
        {
            listViewNames.ItemChecked += ListViewNamesItemChecked;
        }

        internal void FixCasing()
        {
            foreach (ListViewItem item in listViewFixes.Items)
            {
                if (item.Checked)
                {
                    LinesChanged++;
                    if (item.Tag is TextChangedEventArgs textChangedEventArgs)
                    {
                        textChangedEventArgs.Paragraph.Text = textChangedEventArgs.After;
                    }
                }
            }
        }

        private void ButtonOkClick(object sender, EventArgs e) => DialogResult = DialogResult.OK;

        private void listViewFixes_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listViewFixes.SelectedItems.Count > 1)
                labelXLinesSelected.Text = string.Format(Configuration.Settings.Language.Main.XLinesSelected, listViewFixes.SelectedItems.Count);
            else
                labelXLinesSelected.Text = string.Empty;
        }

        private void buttonSelectAll_Click(object sender, EventArgs e) => DoSelection(true);

        private void buttonInverseSelection_Click(object sender, EventArgs e) => DoSelection(false);

        private void DoSelection(bool selectAll)
        {
            listViewNames.ItemChecked -= ListViewNamesItemChecked;
            listViewNames.BeginUpdate();
            foreach (ListViewItem item in listViewNames.Items)
            {
                if (selectAll)
                    item.Checked = true;
                else
                    item.Checked = !item.Checked;
            }
            listViewNames.EndUpdate();
            listViewNames.ItemChecked += ListViewNamesItemChecked;
            GeneratePreview();
        }

        private void buttonAddCustomNames_Click(object sender, EventArgs e)
        {
            AddCustomNames();
            textBoxExtraNames.Text = string.Empty;
            FindAllNames();
        }

    }
}