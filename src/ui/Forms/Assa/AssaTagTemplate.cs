using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Logic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.Forms.Assa
{
    public partial class AssaTagTemplate : Form
    {
        private readonly List<AssaTemplateItem> _templates = new List<AssaTemplateItem>();
        private AssaTemplateItem _activeTemplate;

        public AssaTagTemplate()
        {
            UiUtil.PreInitialize(this);
            InitializeComponent();
            UiUtil.FixFonts(this);

            listViewTemplates_SelectedIndexChanged(null, null);
            _templates.AddRange(Configuration.Settings.Tools.AssaTagTemplates);
            ShowTemplates(_templates, null);

            buttonOK.Text = LanguageSettings.Current.General.Ok;
            buttonCancel.Text = LanguageSettings.Current.General.Cancel;
            UiUtil.FixLargeFonts(this, buttonOK);
        }

        private void ShowTemplates(List<AssaTemplateItem> templates, AssaTemplateItem focusTemplate)
        {
            listViewTemplates.BeginUpdate();
            listViewTemplates.Items.Clear();
            for (var index = 0; index < templates.Count; index++)
            {
                var template = templates[index];
                var item = new ListViewItem { Text = template.Tag, Tag = template };
                item.SubItems.Add(template.Hint);
                listViewTemplates.Items.Add(item);
                if (template == focusTemplate)
                {
                    listViewTemplates.Items[index].Selected = true;
                }
            }

            listViewTemplates.EndUpdate();
            if (templates.Count > 0 && listViewTemplates.SelectedItems.Count == 0)
            {
                listViewTemplates.Items[0].Selected = true;
            }
        }

        private void SaveTemplates()
        {
            Configuration.Settings.Tools.AssaTagTemplates.Clear();
            Configuration.Settings.Tools.AssaTagTemplates.AddRange(_templates);
        }


        private void AssaTagTemplate_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                DialogResult = DialogResult.Cancel;
            }
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            SaveTemplates();
            DialogResult = DialogResult.OK;
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void buttonAdd_Click(object sender, EventArgs e)
        {
            var newItem = new AssaTemplateItem { Tag = "{\\}" };
            _templates.Add(newItem);
            ShowTemplates(_templates, newItem);
            textBoxTag.Focus();
        }

        private void listViewTemplates_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listViewTemplates.SelectedItems.Count == 0)
            {
                _activeTemplate = null;
                textBoxTag.Text = string.Empty;
                textBoxHint.Text = string.Empty;
                groupBoxTemplateItem.Enabled = false;
                buttonCopy.Enabled = false;
                return;
            }

            var idx = listViewTemplates.SelectedItems[0].Index;
            _activeTemplate = listViewTemplates.Items[idx].Tag as AssaTemplateItem;
            if (_activeTemplate != null)
            {
                textBoxTag.Text = _activeTemplate.Tag;
                textBoxHint.Text = _activeTemplate.Hint;
                groupBoxTemplateItem.Enabled = true;
                buttonCopy.Enabled = true;
            }
        }

        private void textBoxTag_TextChanged(object sender, EventArgs e)
        {
            if (_activeTemplate == null)
            {
                return;
            }

            _activeTemplate.Tag = textBoxTag.Text;
            UpdateListView();
        }

        private void UpdateListView()
        {
            if (_activeTemplate == null)
            {
                return;
            }

            foreach (ListViewItem item in listViewTemplates.Items)
            {
                if (item.Tag as AssaTemplateItem != _activeTemplate)
                {
                    continue;
                }

                item.SubItems[0].Text = _activeTemplate.Tag;
                item.SubItems[1].Text = _activeTemplate.Hint;
                break;
            }
        }

        private void textBoxHint_TextChanged(object sender, EventArgs e)
        {
            if (_activeTemplate == null)
            {
                return;
            }

            _activeTemplate.Hint = textBoxHint.Text;
            UpdateListView();
        }

        private void buttonRemove_Click(object sender, EventArgs e)
        {
            if (listViewTemplates.SelectedItems.Count == 0)
            {
                return;
            }

            _activeTemplate = null;
            var idx = listViewTemplates.SelectedItems[0].Index;
            _templates.RemoveAt(idx);

            var focusItem = _templates.Count > idx ? _templates[idx] : _templates.LastOrDefault();
            ShowTemplates(_templates, focusItem);
        }

        private void buttonRemoveAll_Click(object sender, EventArgs e)
        {
            _activeTemplate = null;
            _templates.Clear();
            ShowTemplates(_templates, null);
        }

        private void buttonCopy_Click(object sender, EventArgs e)
        {
            if (listViewTemplates.SelectedItems.Count == 0)
            {
                return;
            }

            _activeTemplate = null;
            var idx = listViewTemplates.SelectedItems[0].Index;
            var template = _templates[idx];
            var newItem = new AssaTemplateItem { Tag = template.Tag, Hint = template.Hint };
            _templates.Add(newItem);
            ShowTemplates(_templates, newItem);
        }

        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            buttonRemove_Click(null, null);
        }

        private void toolStripMenuItemRemoveAll_Click(object sender, EventArgs e)
        {
            buttonRemoveAll_Click(null, null);
        }

        private void newToolStripMenuItemNew_Click(object sender, EventArgs e)
        {
            buttonAdd_Click(null, null);
        }

        private void copyToolStripMenuItemCopy_Click(object sender, EventArgs e)
        {
            buttonCopy_Click(null, null);
        }

        private void MoveUp(ListView listView)
        {
            if (listView.SelectedItems.Count == 0)
            {
                return;
            }

            var idx = listView.SelectedItems[0].Index;
            if (idx == 0)
            {
                return; // already top
            }

            var item = _templates[idx];
            _templates.RemoveAt(idx);
            idx--;
            _templates.Insert(idx, item);
            ShowTemplates(_templates, item);
        }

        private void MoveDown(ListView listView)
        {
            if (listView.SelectedItems.Count == 0)
            {
                return;
            }

            var idx = listView.SelectedItems[0].Index;
            if (idx >= listView.Items.Count - 1)
            {
                return; // already last
            }

            var item = _templates[idx];
            _templates.RemoveAt(idx);
            idx++;
            _templates.Insert(idx, item);
            ShowTemplates(_templates, item);
        }

        private void MoveToTop(ListView listView)
        {
            if (listView.SelectedItems.Count == 0)
            {
                return;
            }

            var idx = listView.SelectedItems[0].Index;
            if (idx == 0)
            {
                return; // already top
            }

            var item = _templates[idx];
            _templates.RemoveAt(idx);
            idx = 0;
            _templates.Insert(idx, item);
            ShowTemplates(_templates, item);
        }

        private void MoveToBottom(ListView listView)
        {
            if (listView.SelectedItems.Count == 0)
            {
                return;
            }

            var idx = listView.SelectedItems[0].Index;
            if (idx == listView.Items.Count - 1)
            {
                return; // already last
            }

            var item = _templates[idx];
            _templates.RemoveAt(idx);
            _templates.Add(item);
            ShowTemplates(_templates, item);
        }

        private void moveUpToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MoveUp(listViewTemplates);
        }

        private void moveDownToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MoveDown(listViewTemplates);
        }

        private void moveTopToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MoveToTop(listViewTemplates);
        }

        private void moveBottomToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MoveToBottom(listViewTemplates);
        }
    }
}
