using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Logic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.Forms.Options
{
    public sealed partial class DefaultLanguagesChooser : Form
    {
        public string DefaultLanguages { get; set; }

        private readonly List<FormRemoveTextForHearImpaired.LanguageItem> _languageItems;

        public DefaultLanguagesChooser(string defaultLanguages)
        {
            UiUtil.PreInitialize(this);
            InitializeComponent();
            UiUtil.FixFonts(this);

            Text = LanguageSettings.Current.General.ChangeLanguageFilter.TrimEnd('.');
            buttonOK.Text = LanguageSettings.Current.General.Ok;
            buttonCancel.Text = LanguageSettings.Current.General.Cancel;
            toolStripMenuItemSelAll.Text = LanguageSettings.Current.Main.Menu.ContextMenu.SelectAll;
            toolStripMenuItemInvertSel.Text = LanguageSettings.Current.Main.Menu.Edit.InverseSelection;

            DefaultLanguages = string.Empty;
            listView1.CheckBoxes = true;
            listView1.BeginUpdate();
            listView1.Items.Clear();

            var languages = defaultLanguages == null ? Array.Empty<string>() : defaultLanguages.Split(';');
            _languageItems = new List<FormRemoveTextForHearImpaired.LanguageItem>();
            foreach (var ci in Utilities.GetSubtitleLanguageCultures(false).OrderBy(p => p.EnglishName))
            {
                var x = new FormRemoveTextForHearImpaired.LanguageItem(ci, ci.EnglishName);
                _languageItems.Add(x);
                var listViewItem = new ListViewItem(x.ToString())
                {
                    Tag = x,
                    Checked = languages.Contains(ci.TwoLetterISOLanguageName),
                };
                listView1.Items.Add(listViewItem);
            }

            listView1.HeaderStyle = ColumnHeaderStyle.None;
            listView1.EndUpdate();
            listView1.AutoSizeLastColumn();

            InitLanguageList();
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
        }

        private void DefaultLanguagesChooser_Shown(object sender, EventArgs e)
        {
            BringToFront();
            Activate();
        }

        private void buttonShortcutsClear_Click(object sender, EventArgs e)
        {
            textBoxShortcutSearch.Text = string.Empty;
        }

        private void textBoxShortcutSearch_TextChanged(object sender, EventArgs e)
        {
            buttonShortcutsClear.Enabled = textBoxShortcutSearch.Text.Length > 0;
            listView1.BeginUpdate();
            listView1.Items.Clear();
            foreach (var x in _languageItems)
            {
                if (x.Name.Contains(textBoxShortcutSearch.Text) ||
                    x.Code.TwoLetterISOLanguageName == textBoxShortcutSearch.Text ||
                    x.Code.ThreeLetterISOLanguageName == textBoxShortcutSearch.Text)
                {
                    var listViewItem = new ListViewItem(x.ToString())
                    {
                        Tag = x,
                        Checked = DefaultLanguages.Contains(x.Code.TwoLetterISOLanguageName),
                    };
                    listView1.Items.Add(listViewItem);
                }
            }

            listView1.HeaderStyle = ColumnHeaderStyle.None;
            listView1.EndUpdate();
        }

        private void listView1_ItemChecked(object sender, ItemCheckedEventArgs e)
        {
            if (e.Item.Checked)
            {
                if (e.Item.Tag is FormRemoveTextForHearImpaired.LanguageItem li)
                {
                    DefaultLanguages = DefaultLanguages.TrimEnd(';') + ";" + li.Code.TwoLetterISOLanguageName;
                }
            }
            else
            {
                if (e.Item.Tag is FormRemoveTextForHearImpaired.LanguageItem li)
                {
                    DefaultLanguages = DefaultLanguages.Replace(";" + li.Code.TwoLetterISOLanguageName, string.Empty);
                    DefaultLanguages = DefaultLanguages.Replace(li.Code.TwoLetterISOLanguageName, string.Empty);
                    DefaultLanguages = DefaultLanguages.Replace(";;", ";").Trim(';');
                }
            }

            InitLanguageList();
        }

        private void InitLanguageList()
        {
            if (string.IsNullOrEmpty(DefaultLanguages))
            {
                labelDefaultLanguagesList.Text = LanguageSettings.Current.General.All;
                return;
            }

            var arr = DefaultLanguages.Split(';');
            if (DefaultLanguages.Length > 30)
            {
                labelDefaultLanguagesList.Text = arr.Length.ToString();
                return;
            }

            labelDefaultLanguagesList.Text = string.Join(", ", arr);
        }

        private void DefaultLanguagesChooser_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                DialogResult = DialogResult.Cancel;
            }
        }

        private void toolStripMenuItemSelAll_Click(object sender, EventArgs e)
        {
            DoSelection(true);
        }

        private void toolStripMenuItemInvertSel_Click(object sender, EventArgs e)
        {
            DoSelection(false);
        }

        private void DoSelection(bool selectAll)
        {
            if (listView1.Items.Count == 0)
            {
                return;
            }

            foreach (ListViewItem item in listView1.Items)
            {
                item.Checked = selectAll || !item.Checked;
            }
        }
    }
}
