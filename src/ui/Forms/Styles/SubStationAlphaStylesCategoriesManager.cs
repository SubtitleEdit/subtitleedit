using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Logic;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Xml;

namespace Nikse.SubtitleEdit.Forms.Styles
{
    public sealed partial class SubStationAlphaStylesCategoriesManager : Form
    {
        internal const string Category = "Category";
        internal const string CategoryName = "Name";
        internal const string CategoryIsDefault = "CategoryIsDefault";
        internal const string Style = "Style";
        internal const string StyleName = "Name";
        internal const string StyleFontName = "FontName";
        internal const string StyleFontSize = "FontSize";
        internal const string StylePrimaryColor = "PrimaryColor";
        internal const string StyleSecondaryColor = "SecondaryColor";
        internal const string StyleOutlineColor = "OutlineColor";
        internal const string StyleBackgroundColor = "BackgroundColor";
        internal const string StyleShadowWidth = "ShadowWidth";
        internal const string OutlineWidth = "OutlineWidth";
        internal const string Alignment = "Alignment";
        internal const string MarginLeft = "MarginLeft";
        internal const string MarginRight = "MarginRight";
        internal const string MarginVertical = "MarginVertical";
        internal const string BorderStyle = "BorderStyle";

        private const string TemplateFilterExtension = "|*.template";

        public AssaStorageCategory SelectedCategory =>
            GetCategoryByName(listViewCategories.SelectedItems?[0].Text);

        private List<AssaStorageCategory> _assaCategories;
        private readonly List<AssaStorageCategory> _oldAssaCategories = new List<AssaStorageCategory>();

        public SubStationAlphaStylesCategoriesManager(List<AssaStorageCategory> currentAssaCategories, string focusCategory)
        {
            InitializeComponent();
            UiUtil.FixFonts(this);
            InitializeLanguage();

            _assaCategories = currentAssaCategories;
            foreach (var category in currentAssaCategories)
            {
                var lvi = new ListViewItem(category.Name);
                if (category.Name == focusCategory)
                {
                    lvi.Selected = true;
                    lvi.EnsureVisible();
                    lvi.Focused = true;
                }

                listViewCategories.Items.Add(lvi);
                _oldAssaCategories.Add(category);
            }

            listViewCategories.Focus();
        }

        private void InitializeLanguage()
        {
            groupBoxCategories.Text = LanguageSettings.Current.SubStationAlphaStyles.Categories;
            buttonNewCategory.Text = LanguageSettings.Current.SubStationAlphaStyles.New;
            buttonRemoveCategory.Text = LanguageSettings.Current.SubStationAlphaStyles.Remove;
            buttonSetDefaultCategory.Text = LanguageSettings.Current.SubStationAlphaStyles.CategorySetDefault;
            buttonExportCategories.Text = LanguageSettings.Current.SubStationAlphaStyles.Export;
            buttonImportCategories.Text = LanguageSettings.Current.SubStationAlphaStyles.Import;

            newToolStripMenuItem.Text = LanguageSettings.Current.SubStationAlphaStyles.NewCategory;
            toolStripMenuItemRename.Text = LanguageSettings.Current.SubStationAlphaStyles.CategoryRename;
            deleteToolStripMenuItem1.Text = LanguageSettings.Current.SubStationAlphaStyles.Remove;
            moveUpToolStripMenuItem.Text = LanguageSettings.Current.DvdSubRip.MoveUp;
            moveDownToolStripMenuItem.Text = LanguageSettings.Current.DvdSubRip.MoveDown;
            moveToTopToolStripMenuItem.Text = LanguageSettings.Current.MultipleReplace.MoveToTop; ;
            moveToBottomToolStripMenuItem.Text = LanguageSettings.Current.MultipleReplace.MoveToBottom;
            importToolStripMenuItem.Text = LanguageSettings.Current.SubStationAlphaStyles.Import;
            exportToolStripMenuItem.Text = LanguageSettings.Current.SubStationAlphaStyles.Export;

            buttonOK.Text = LanguageSettings.Current.General.Ok;
            buttonCancel.Text = LanguageSettings.Current.General.Cancel;
        }

        private AssaStorageCategory GetCategoryByName(string categoryName) =>
            categoryName != null ?_assaCategories.SingleOrDefault(category => category.Name == categoryName) : null;

        private void UpdateSelectedIndices(int startingIndex = -1, int numberOfSelectedItems = 1)
        {
            if (numberOfSelectedItems == 0)
            {
                return;
            }

            if (startingIndex == -1 || startingIndex >= listViewCategories.Items.Count)
            {
                startingIndex = listViewCategories.Items.Count - 1;
            }

            if (startingIndex - numberOfSelectedItems < -1)
            {
                return;
            }

            listViewCategories.SelectedItems.Clear();
            for (int i = 0; i < numberOfSelectedItems; i++)
            {
                listViewCategories.Items[startingIndex - i].Selected = true;
                listViewCategories.Items[startingIndex - i].EnsureVisible();
                listViewCategories.Items[startingIndex - i].Focused = true;
            }

            listViewCategories.Focus();
        }

        public static string FixDuplicateName(string newCategoryName, List<AssaStorageCategory> existingCategories)
        {
            if (existingCategories.All(p => p.Name != newCategoryName))
            {
                return newCategoryName;
            }

            for (int i = 1; i < int.MaxValue; i++)
            {
                var name = $"{newCategoryName}_{i}";
                if (existingCategories.All(p => p.Name != name))
                {
                    return name;
                }
            }

            return Guid.NewGuid().ToString();
        }

        private void ListViewCategories_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listViewCategories.SelectedItems.Count == 1)
            {
                var isDefaultCategory = !SelectedCategory.IsDefault;
                buttonSetDefaultCategory.Enabled = isDefaultCategory;
                buttonRemoveCategory.Enabled = isDefaultCategory;
            }
            else
            {
                buttonSetDefaultCategory.Enabled = false;
            }
        }

        private void ButtonNewCategory_Click(object sender, EventArgs e)
        {
            using (var form = new TextPrompt(LanguageSettings.Current.SubStationAlphaStyles.NewCategory, LanguageSettings.Current.SubStationAlphaStyles.CategoryName, string.Empty))
            {
                if (form.ShowDialog() == DialogResult.OK)
                {
                    int insertIndex = listViewCategories.Items.Count;
                    var newName = form.InputText;
                    var overridingDefault = false;
                    if (_assaCategories.Exists(category => category.Name == newName))
                    {
                        var result = MessageBox.Show(string.Format(LanguageSettings.Current.SubStationAlphaStyles.OverwriteX, newName), string.Empty, MessageBoxButtons.YesNoCancel);
                        if (result == DialogResult.Yes)
                        {
                            var overriddenCategory = _assaCategories.Single(category => category.Name == newName);
                            overridingDefault = overriddenCategory.IsDefault;
                            _assaCategories.Remove(overriddenCategory);
                            insertIndex = listViewCategories.FindItemWithText(newName, false, 0, false).Index;
                            listViewCategories.Items.RemoveAt(insertIndex);
                        }
                        else if (result == DialogResult.No)
                        {
                            newName = FixDuplicateName(newName, _assaCategories);
                        }
                        else
                        {
                            return;
                        }
                    }

                    var newCategory = new AssaStorageCategory { Name = newName, IsDefault = overridingDefault, Styles = new List<SsaStyle>() };
                    if (overridingDefault)
                    {
                        newCategory.Styles.Add(new SsaStyle());
                    }
                    _assaCategories.Insert(insertIndex, newCategory);
                    listViewCategories.Items.Insert(insertIndex, new ListViewItem(newName));
                    UpdateSelectedIndices(insertIndex);
                }
            }
        }

        private void ButtonRemoveCategory_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem selectedItem in listViewCategories.SelectedItems)
            {
                if (GetCategoryByName(selectedItem.Text).IsDefault)
                {
                    continue;
                }

                listViewCategories.Items.Remove(selectedItem);
                _assaCategories.RemoveAll(category => category.Name == selectedItem.Text);
            }

            UpdateSelectedIndices();
        }

        private void ButtonSetDefaultCategory_Click(object sender, EventArgs e)
        {
            var oldDefaultCategory = _assaCategories.Single(category => category.IsDefault);
            oldDefaultCategory.IsDefault = false;
            SelectedCategory.IsDefault = true;
            buttonSetDefaultCategory.Enabled = false;
            buttonRemoveCategory.Enabled = false;

            if (SelectedCategory.Styles.Count == 0)
            {
                var defaultStyle = new SsaStyle();
                SelectedCategory.Styles.Add(defaultStyle);
            }

            listViewCategories.Focus();
        }

        private void ButtonExportCategories_Click(object sender, EventArgs e)
        {
            if (listViewCategories.Items.Count == 0)
            {
                return;
            }

            using (var exportForm = new SubStationAlphaStylesCategoriesImportExport(_assaCategories, true))
            {
                exportForm.ShowDialog(this);
            }
        }

        private void ButtonImportCategories_Click(object sender, EventArgs e)
        {
            openFileDialogImport.Title = LanguageSettings.Current.SubStationAlphaStyles.ImportCategoriesTitle;
            openFileDialogImport.Filter = LanguageSettings.Current.SubStationAlphaStyles.Categories + TemplateFilterExtension;
            if (openFileDialogImport.ShowDialog(this) == DialogResult.OK)
            {
                try
                {
                    var importCategories = ImportGroupsFile(openFileDialogImport.FileName);
                    if (importCategories.Count == 0)
                    {
                        MessageBox.Show(LanguageSettings.Current.MultipleReplace.NothingToImport);
                        return;
                    }

                    using (var inputForm = new SubStationAlphaStylesCategoriesImportExport(importCategories, false))
                    {
                        if (inputForm.ShowDialog(this) == DialogResult.OK)
                        {
                            var oldCategorySelectedIndex = listViewCategories.SelectedIndices.Count == 0 ? -1 : listViewCategories.SelectedIndices[0];
                            foreach (var importCategory in importCategories.Where(Category => inputForm.ChosenCategories.Contains(Category.Name)))
                            {
                                int insertIndex = listViewCategories.Items.Count;
                                var importedName = importCategory.Name;
                                var overridingDefault = false;
                                if (_assaCategories.Exists(category => category.Name == importedName))
                                {
                                    var result = MessageBox.Show(string.Format(LanguageSettings.Current.SubStationAlphaStyles.OverwriteX, importedName), string.Empty, MessageBoxButtons.YesNoCancel);
                                    if (result == DialogResult.Yes)
                                    {
                                        var overriddenCategory = _assaCategories.Single(category => category.Name == importedName);
                                        overridingDefault = overriddenCategory.IsDefault;
                                        _assaCategories.Remove(overriddenCategory);
                                        insertIndex = listViewCategories.FindItemWithText(importedName, false, 0, false).Index;
                                        listViewCategories.Items.RemoveAt(insertIndex);
                                    }
                                    else if (result == DialogResult.No)
                                    {
                                        importedName = FixDuplicateName(importedName, _assaCategories);
                                        importCategory.Name = importedName;
                                    }
                                    else
                                    {
                                        return;
                                    }
                                }

                                importCategory.IsDefault = false;
                                _assaCategories.Insert(insertIndex, importCategory);
                                listViewCategories.Items.Insert(insertIndex, new ListViewItem(importedName));
                            }

                            UpdateSelectedIndices(oldCategorySelectedIndex);
                        }
                    }
                }
                catch (Exception exception)
                {
                    MessageBox.Show(exception.Message);
                    return;
                }
            }
        }

        private List<AssaStorageCategory> ImportGroupsFile(string fileName)
        {
            var list = new List<AssaStorageCategory>();
            var doc = new XmlDocument { XmlResolver = null };
            doc.Load(fileName);
            var categories = doc.DocumentElement?.SelectNodes($"//{Category}");
            if (categories != null)
            {
                foreach (XmlNode categoryNode in categories)
                {
                    var category = new AssaStorageCategory();
                    var nameNode = categoryNode.SelectSingleNode(CategoryName);
                    var isDefaultNode = categoryNode.SelectSingleNode(CategoryIsDefault);

                    category.Name = nameNode?.InnerText ?? "Untitled";
                    category.IsDefault = isDefaultNode != null && Convert.ToBoolean(isDefaultNode.InnerText);

                    category.Styles = new List<SsaStyle>();
                    list.Add(category);

                    var styles = categoryNode.SelectNodes("Style");
                    if (styles != null)
                    {
                        foreach (XmlNode styleNode in styles)
                        {
                            var style = new SsaStyle();
                            var subNode = styleNode.SelectSingleNode("Name");
                            if (subNode != null)
                            {
                                style.Name = subNode.InnerText;
                            }

                            subNode = styleNode.SelectSingleNode(StyleFontName);
                            if (subNode != null)
                            {
                                style.FontName = subNode.InnerText;
                            }

                            subNode = styleNode.SelectSingleNode("FontSize");
                            if (subNode != null)
                            {
                                style.FontSize = Convert.ToSingle(subNode.InnerText);
                            }

                            subNode = styleNode.SelectSingleNode("Primary");
                            if (subNode != null)
                            {
                                style.Primary = ColorTranslator.FromHtml(subNode.InnerText);
                            }

                            subNode = styleNode.SelectSingleNode("Secondary");
                            if (subNode != null)
                            {
                                style.Secondary = ColorTranslator.FromHtml(subNode.InnerText);
                            }

                            subNode = styleNode.SelectSingleNode("Outline");
                            if (subNode != null)
                            {
                                style.Outline = ColorTranslator.FromHtml(subNode.InnerText);
                            }

                            subNode = styleNode.SelectSingleNode("Background");
                            if (subNode != null)
                            {
                                style.Background = ColorTranslator.FromHtml(subNode.InnerText);
                            }

                            subNode = styleNode.SelectSingleNode("ShadowWidth");
                            if (subNode != null)
                            {
                                style.ShadowWidth = Convert.ToDecimal(subNode.InnerText);
                            }

                            subNode = styleNode.SelectSingleNode("OutlineWidth");
                            if (subNode != null)
                            {
                                style.OutlineWidth = Convert.ToDecimal(subNode.InnerText);
                            }

                            subNode = styleNode.SelectSingleNode("Alignment");
                            if (subNode != null)
                            {
                                style.Alignment = subNode.InnerText;
                            }

                            subNode = styleNode.SelectSingleNode("MarginLeft");
                            if (subNode != null)
                            {
                                style.MarginLeft = Convert.ToInt32(subNode.InnerText);
                            }

                            subNode = styleNode.SelectSingleNode("MarginRight");
                            if (subNode != null)
                            {
                                style.MarginRight = Convert.ToInt32(subNode.InnerText);
                            }

                            subNode = styleNode.SelectSingleNode("MarginVertical");
                            if (subNode != null)
                            {
                                style.MarginVertical = Convert.ToInt32(subNode.InnerText);
                            }

                            subNode = styleNode.SelectSingleNode("BorderStyle");
                            if (subNode != null)
                            {
                                style.BorderStyle = subNode.InnerText;
                            }

                            category.Styles.Add(style);
                        }
                    }
                }
            }

            return list;
        }

        private void ContextMenuStripGroups_Opening(object sender, EventArgs e)
        {
            var onlyOneSelected = listViewCategories.SelectedItems.Count == 1;
            toolStripMenuItemRename.Enabled = onlyOneSelected;
            toolStripMenuItemRename.Enabled = onlyOneSelected;

            var moreThanOneExist = listViewCategories.Items.Count > 1;
            toolStripSeparator.Visible = onlyOneSelected && moreThanOneExist;
            moveUpToolStripMenuItem.Visible = onlyOneSelected && moreThanOneExist;
            moveDownToolStripMenuItem.Visible = onlyOneSelected && moreThanOneExist;
            moveToTopToolStripMenuItem.Visible = onlyOneSelected && moreThanOneExist;
            moveToBottomToolStripMenuItem.Visible = onlyOneSelected && moreThanOneExist;

            var moreThanOneSelected = listViewCategories.SelectedItems.Count > 1;
            deleteToolStripMenuItem1.Visible = moreThanOneSelected;

            var isDefaultCategory = listViewCategories.SelectedItems.Count == 1 &&
                                    !_assaCategories.Single(category => category.Name == listViewCategories.SelectedItems[0].Text).IsDefault;
            deleteToolStripMenuItem1.Visible = isDefaultCategory;
        }

        private void NewToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ButtonNewCategory_Click(sender, e);
        }

        private void ToolStripMenuItemRenameClick(object sender, EventArgs e)
        {
            if (listViewCategories.SelectedItems.Count != 1)
            {
                return;
            }

            using (var form = new TextPrompt(string.Empty, LanguageSettings.Current.SubStationAlphaStyles.CategoryName, listViewCategories.SelectedItems[0].Text))
            {
                if (form.ShowDialog() == DialogResult.OK)
                {
                    var newName = form.InputText;
                    newName = FixDuplicateName(newName, _assaCategories);
                    SelectedCategory.Name = newName;
                    listViewCategories.SelectedItems[0].Text = newName;
                }
            }
        }

        private void DeleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ButtonRemoveCategory_Click(sender, e);
        }

        private void MoveUpToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listViewCategories.SelectedItems.Count != 1)
            {
                return;
            }

            var idx = listViewCategories.SelectedItems[0].Index;
            if (idx == 0)
            {
                return;
            }

            var item = listViewCategories.SelectedItems[0];
            var category = _assaCategories[idx];
            _assaCategories.RemoveAt(idx);
            listViewCategories.Items.RemoveAt(idx);

            idx--;
            _assaCategories.Insert(idx, category);
            listViewCategories.Items.Insert(idx, item);
            UpdateSelectedIndices(idx);
        }

        private void MoveDownToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listViewCategories.SelectedItems.Count != 1)
            {
                return;
            }

            var idx = listViewCategories.SelectedItems[0].Index;
            if (idx >= listViewCategories.Items.Count - 1)
            {
                return;
            }

            var item = listViewCategories.SelectedItems[0];
            var category = _assaCategories[idx];
            _assaCategories.RemoveAt(idx);
            listViewCategories.Items.RemoveAt(idx);

            idx++;
            _assaCategories.Insert(idx, category);
            listViewCategories.Items.Insert(idx, item);
            UpdateSelectedIndices(idx);
        }

        private void MoveToTopToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listViewCategories.SelectedItems.Count != 1)
            {
                return;
            }

            var idx = listViewCategories.SelectedItems[0].Index;
            if (idx == 0)
            {
                return;
            }

            var item = listViewCategories.SelectedItems[0];
            var category = _assaCategories[idx];
            _assaCategories.RemoveAt(idx);
            listViewCategories.Items.RemoveAt(idx);

            idx = 0;
            _assaCategories.Insert(0, category);
            listViewCategories.Items.Insert(idx, item);
            UpdateSelectedIndices(idx);
        }

        private void MoveToBottomToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listViewCategories.SelectedItems.Count != 1)
            {
                return;
            }

            var idx = listViewCategories.SelectedItems[0].Index;
            if (idx == listViewCategories.Items.Count - 1)
            {
                return;
            }

            var item = listViewCategories.SelectedItems[0];
            var category = _assaCategories[idx];
            _assaCategories.RemoveAt(idx);
            listViewCategories.Items.RemoveAt(idx);

            _assaCategories.Add(category);
            listViewCategories.Items.Add(item);
            UpdateSelectedIndices();
        }

        private void ImportToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ButtonImportCategories_Click(sender, e);
        }

        private void ExportToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ButtonExportCategories_Click(sender, e);
        }

        private void ButtonCancel_Click(object sender, EventArgs e)
        {
            _assaCategories = _oldAssaCategories;
            DialogResult = DialogResult.Cancel;
        }

        private void ButtonOK_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
        }

        private void ListViewCategories_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.A && e.Modifiers == Keys.Control)
            {
                listViewCategories.SelectAll();
                e.SuppressKeyPress = true;
            }
            else if (e.KeyCode == Keys.D && e.Modifiers == Keys.Control)
            {
                listViewCategories.SelectFirstSelectedItemOnly();
                e.SuppressKeyPress = true;
            }
            else if (e.KeyCode == Keys.I && e.Modifiers == (Keys.Control | Keys.Shift))
            {
                listViewCategories.InverseSelection();
                e.SuppressKeyPress = true;
            }
            else if (e.KeyData == UiUtil.GetKeys(Configuration.Settings.Shortcuts.MainToggleFocus))
            {
                listViewCategories.Focus();
            }
        }

        private void SubStationAlphaStylesCategoriesManager_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyData == Keys.Escape)
            {
                ButtonCancel_Click(sender, e);
            }
        }

        private void SubStationAlphaStylesCategoriesManager_Shown(object sender, EventArgs e)
        {
            SubStationAlphaStylesCategoriesManager_ResizeEnd(sender, e);
        }

        private void SubStationAlphaStylesCategoriesManager_ResizeEnd(object sender, EventArgs e)
        {
            listViewCategories.Columns[listViewCategories.Columns.Count - 1].Width = listViewCategories.ClientSize.Width;
        }
    }
}
