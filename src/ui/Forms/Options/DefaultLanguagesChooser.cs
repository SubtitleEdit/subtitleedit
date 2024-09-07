using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Logic;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.Forms.Options
{
    public sealed partial class DefaultLanguagesChooser : Form
    {
        public class LanguageItem
        {
            public CultureInfo Code { get; }
            public string Name { get; }
            public bool Checked { get; set; }

            public LanguageItem(CultureInfo code, string name)
            {
                Code = code;
                Name = name;
            }

            public override string ToString()
            {
                return Name;
            }
        }

        public string DefaultLanguages { get; set; }
        private readonly List<string> _defaultLanguageArray;

        private readonly List<LanguageItem> _languageItems;

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

            var languages = defaultLanguages == null ? Array.Empty<string>() : defaultLanguages.Trim(';').Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            _defaultLanguageArray = languages.ToList();
            _languageItems = new List<LanguageItem>();
            foreach (var ci in Utilities.GetSubtitleLanguageCultures(false).OrderBy(p => p.EnglishName))
            {
                var x = new LanguageItem(ci, ci.EnglishName);
                x.Checked = languages.Contains(ci.TwoLetterISOLanguageName);
                _languageItems.Add(x);
                var listViewItem = new ListViewItem(x.ToString())
                {
                    Tag = x,
                    Checked = x.Checked,
                };
                listView1.Items.Add(listViewItem);
            }

            listView1.HeaderStyle = ColumnHeaderStyle.None;
            listView1.EndUpdate();
            listView1.AutoSizeLastColumn();

            InitLanguageList();

            textBoxSearch.Left = labelSearch.Right + 2;
            buttonSearchClear.Left = textBoxSearch.Right + 2;
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == listView1.Items.Count)
            {
                DefaultLanguages = string.Empty;
            }

            DialogResult = DialogResult.OK;
        }

        private void DefaultLanguagesChooser_Shown(object sender, EventArgs e)
        {
            BringToFront();
            Activate();
        }

        private void buttonShortcutsClear_Click(object sender, EventArgs e)
        {
            textBoxSearch.Text = string.Empty;
        }

        private void textBoxShortcutSearch_TextChanged(object sender, EventArgs e)
        {
            buttonSearchClear.Enabled = textBoxSearch.Text.Length > 0;
            listView1.BeginUpdate();
            listView1.Items.Clear();
            foreach (var x in _languageItems)
            {
                if (x.Name.Contains(textBoxSearch.Text, StringComparison.OrdinalIgnoreCase) ||
                    x.Code.TwoLetterISOLanguageName.Equals(textBoxSearch.Text, StringComparison.OrdinalIgnoreCase) ||
                    x.Code.ThreeLetterISOLanguageName.Equals(textBoxSearch.Text, StringComparison.OrdinalIgnoreCase))
                {
                    var listViewItem = new ListViewItem(x.ToString())
                    {
                        Tag = x,
                        Checked = x.Checked,
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
                if (e.Item.Tag is LanguageItem li)
                {
                    li.Checked = true;
                    if (!_defaultLanguageArray.Contains(li.Code.TwoLetterISOLanguageName))
                    {
                        _defaultLanguageArray.Add(li.Code.TwoLetterISOLanguageName);
                    }
                }
            }
            else
            {
                if (e.Item.Tag is LanguageItem li)
                {
                    li.Checked = false;

                    if (_defaultLanguageArray.Contains(li.Code.TwoLetterISOLanguageName))
                    {
                        _defaultLanguageArray.Remove(li.Code.TwoLetterISOLanguageName);
                    }
                }
            }

            DefaultLanguages = string.Join(";", _defaultLanguageArray);
            InitLanguageList();
        }

        private void InitLanguageList()
        {
            if (string.IsNullOrEmpty(DefaultLanguages))
            {
                labelDefaultLanguagesList.Text = LanguageSettings.Current.General.All;
                return;
            }

            if (_defaultLanguageArray.Count > 10)
            {
                labelDefaultLanguagesList.Text = _defaultLanguageArray.Count.ToString();
                return;
            }

            labelDefaultLanguagesList.Text = string.Join(", ", _defaultLanguageArray);
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

            listView1.ItemChecked -= listView1_ItemChecked;

            foreach (var item in _languageItems)
            {
                item.Checked = selectAll || !item.Checked;
                if (item.Checked)
                {
                    if (!_defaultLanguageArray.Contains(item.Code.TwoLetterISOLanguageName))
                    {
                        _defaultLanguageArray.Add(item.Code.TwoLetterISOLanguageName);
                    }
                }
                else
                {
                    if (_defaultLanguageArray.Contains(item.Code.TwoLetterISOLanguageName))
                    {
                        _defaultLanguageArray.Remove(item.Code.TwoLetterISOLanguageName);
                    }
                }
            }

            DefaultLanguages = string.Join(";", _defaultLanguageArray);

            foreach (ListViewItem item in listView1.Items)
            {
                if (item.Tag is LanguageItem li)
                {
                    item.Checked = li.Checked;
                }
            }

            listView1.ItemChecked += listView1_ItemChecked;
            InitLanguageList();
        }

        private void DefaultLanguagesChooser_ResizeEnd(object sender, EventArgs e)
        {
            listView1.AutoSizeLastColumn();
        }
    }
}
