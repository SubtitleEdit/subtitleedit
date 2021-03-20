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
        internal static readonly Color _defaultCategoryColor = Configuration.Settings.General.UseDarkTheme? Color.LimeGreen : Color.Green;

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

        private readonly List<AssaStorageCategory> _assaCategories;
        private readonly List<AssaStorageCategory> _oldAssaCategories = new List<AssaStorageCategory>();

        public AssaStorageCategory SelectedCategory =>
            GetCategoryByName(listViewCategories.SelectedItems.Count > 0 ? listViewCategories.SelectedItems[0].Text : listViewCategories.Items[0].Text);

        public SubStationAlphaStylesCategoriesManager(List<AssaStorageCategory> currentAssaCategories, string focusCategory)
        {
            InitializeComponent();
            UiUtil.FixFonts(this);
            InitializeLanguage();
            SetControlsSize();

            _assaCategories = currentAssaCategories;
            foreach (var category in currentAssaCategories)
            {
                var lvi = GetCategoryListViewItem(category);
                if (category.Name == focusCategory)
                {
                    lvi.Selected = true;
                    lvi.EnsureVisible();
                    lvi.Focused = true;
                }

                listViewCategories.Items.Add(lvi);
                _oldAssaCategories.Add(new AssaStorageCategory() { Name = category.Name, IsDefault = category.IsDefault, Styles = category.Styles });
            }

            listViewCategories.Focus();
        }

        private void InitializeLanguage()
        {
            groupBoxCategories.Text = LanguageSettings.Current.SubStationAlphaStylesCategoriesManager.Categories;
            buttonNewCategory.Text = LanguageSettings.Current.SubStationAlphaStyles.New;
            buttonRemoveCategory.Text = LanguageSettings.Current.SubStationAlphaStyles.Remove;
            buttonSetDefaultCategory.Text = LanguageSettings.Current.SubStationAlphaStylesCategoriesManager.CategorySetDefault;
            buttonExportCategories.Text = LanguageSettings.Current.SubStationAlphaStyles.Export;
            buttonImportCategories.Text = LanguageSettings.Current.SubStationAlphaStyles.Import;

            columnHeaderForName.Text = LanguageSettings.Current.SubStationAlphaStylesCategoriesManager.CategoryName;
            columnHeaderForStyles.Text = LanguageSettings.Current.SubStationAlphaStylesCategoriesManager.NumberOfStyles;
            columnHeaderForDefault.Text = LanguageSettings.Current.SubStationAlphaStylesCategoriesManager.CategoryDefault;

            newToolStripMenuItem.Text = LanguageSettings.Current.SubStationAlphaStylesCategoriesManager.NewCategory;
            toolStripMenuItemRename.Text = LanguageSettings.Current.SubStationAlphaStylesCategoriesManager.CategoryRename;
            deleteToolStripMenuItem.Text = LanguageSettings.Current.SubStationAlphaStyles.Remove;
            deleteAllToolStripMenuItem.Text = LanguageSettings.Current.SubStationAlphaStyles.RemoveAll;
            moveUpToolStripMenuItem.Text = LanguageSettings.Current.DvdSubRip.MoveUp;
            moveDownToolStripMenuItem.Text = LanguageSettings.Current.DvdSubRip.MoveDown;
            moveToTopToolStripMenuItem.Text = LanguageSettings.Current.MultipleReplace.MoveToTop; ;
            moveToBottomToolStripMenuItem.Text = LanguageSettings.Current.MultipleReplace.MoveToBottom;
            importToolStripMenuItem.Text = LanguageSettings.Current.SubStationAlphaStyles.Import;
            exportToolStripMenuItem.Text = LanguageSettings.Current.SubStationAlphaStyles.Export;

            buttonOK.Text = LanguageSettings.Current.General.Ok;
            buttonCancel.Text = LanguageSettings.Current.General.Cancel;
        }

        private void SetControlsSize()
        {
            var firstButtonLeft = listViewCategories.Left;
            var singleButtonWidth = listViewCategories.Width;
            var twoButtonsWidth = (listViewCategories.Width / 2) - 3;

            buttonNewCategory.Left = firstButtonLeft;
            buttonNewCategory.Width = twoButtonsWidth;

            buttonRemoveCategory.Left = buttonNewCategory.Right + 7;
            buttonRemoveCategory.Width = twoButtonsWidth;

            buttonSetDefaultCategory.Left = firstButtonLeft;
            buttonSetDefaultCategory.Width = singleButtonWidth;

            buttonImportCategories.Left = firstButtonLeft;
            buttonImportCategories.Width = twoButtonsWidth;

            buttonExportCategories.Left = buttonImportCategories.Right + 7;
            buttonExportCategories.Width = twoButtonsWidth;

            var singleColumnWidth = listViewCategories.ClientSize.Width / listViewCategories.Columns.Count;
            foreach (ColumnHeader column in listViewCategories.Columns)
            {
                column.Width = singleColumnWidth;
            }

            listViewCategories.AutoSizeLastColumn();
        }

        private AssaStorageCategory GetCategoryByName(string categoryName) =>
            categoryName != null ?_assaCategories.SingleOrDefault(category => category.Name == categoryName) : null;

        private ListViewItem GetCategoryListViewItem(AssaStorageCategory category)
        {
            var lvi = new ListViewItem(category.Name);
            lvi.SubItems.Add(category.Styles.Count.ToString());
            lvi.SubItems.Add(category.IsDefault.ToString());
            SetDefaultSubItemColor(lvi, category.IsDefault);
            return lvi;
        }

        private void SetDefaultSubItemColor(ListViewItem lvi, bool isDefault)
        {
            lvi.UseItemStyleForSubItems = !isDefault;
            lvi.SubItems[lvi.SubItems.Count - 1].Font = new Font(listViewCategories.Font, isDefault ? FontStyle.Bold : FontStyle.Regular);
            lvi.SubItems[lvi.SubItems.Count - 1].ForeColor = isDefault ? _defaultCategoryColor : UiUtil.ForeColor;
        }

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
            if (listViewCategories.SelectedItems.Count > 0)
            {
                var onlyOneSelected = listViewCategories.SelectedItems.Count == 1;
                var moreThanOneSelected = listViewCategories.SelectedItems.Count > 1;
                var selectedIsNotDefault = onlyOneSelected && !SelectedCategory.IsDefault;
                buttonSetDefaultCategory.Enabled = selectedIsNotDefault;
                buttonRemoveCategory.Enabled = selectedIsNotDefault || moreThanOneSelected;
            }
            else
            {
                buttonRemoveCategory.Enabled = false;
                buttonSetDefaultCategory.Enabled = false;
            }
        }

        private void ButtonNewCategory_Click(object sender, EventArgs e)
        {
            using (var form = new TextPrompt(LanguageSettings.Current.SubStationAlphaStylesCategoriesManager.NewCategory, LanguageSettings.Current.SubStationAlphaStylesCategoriesManager.CategoryName, string.Empty))
            {
                if (form.ShowDialog() == DialogResult.OK)
                {
                    var newName = form.InputText;
                    if (string.IsNullOrWhiteSpace(newName))
                    {
                        return;
                    }

                    var insertIndex = listViewCategories.Items.Count;
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

                    var lvi = GetCategoryListViewItem(newCategory);
                    listViewCategories.Items.Insert(insertIndex, lvi);
                    UpdateSelectedIndices(insertIndex);
                }
            }
        }

        private void ButtonRemoveCategory_Click(object sender, EventArgs e)
        {
            var result = Configuration.Settings.General.PromptDeleteLines ?
                MessageBox.Show(LanguageSettings.Current.SubStationAlphaStylesCategoriesManager.CategoryDelete, string.Empty, MessageBoxButtons.YesNo) :
                DialogResult.Yes;
            if (result == DialogResult.Yes)
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
        }

        private void ButtonSetDefaultCategory_Click(object sender, EventArgs e)
        {
            var oldDefaultCategory = _assaCategories.Single(category => category.IsDefault);
            var oldDefaultCategoryListViewItem = listViewCategories.Items[_assaCategories.IndexOf(oldDefaultCategory)];
            oldDefaultCategory.IsDefault = false;
            oldDefaultCategoryListViewItem.SubItems[columnHeaderForDefault.Index].Text = oldDefaultCategory.IsDefault.ToString();
            SetDefaultSubItemColor(oldDefaultCategoryListViewItem, oldDefaultCategory.IsDefault);

            var newDefault = SelectedCategory;
            var newDefaultCategoryListViewItem = listViewCategories.Items[_assaCategories.IndexOf(newDefault)];
            newDefault.IsDefault = true;
            if (newDefault.Styles.Count == 0)
            {
                var defaultStyle = new SsaStyle();
                SelectedCategory.Styles.Add(defaultStyle);
            }
            newDefaultCategoryListViewItem.SubItems[columnHeaderForStyles.Index].Text = newDefault.Styles.Count.ToString();
            newDefaultCategoryListViewItem.SubItems[columnHeaderForDefault.Index].Text = newDefault.IsDefault.ToString();
            SetDefaultSubItemColor(newDefaultCategoryListViewItem, newDefault.IsDefault);

            buttonSetDefaultCategory.Enabled = false;
            buttonRemoveCategory.Enabled = false;

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
            openFileDialogImport.Title = LanguageSettings.Current.SubStationAlphaStylesCategoriesManager.ImportCategoriesTitle;
            openFileDialogImport.Filter = LanguageSettings.Current.SubStationAlphaStylesCategoriesManager.Categories + TemplateFilterExtension;
            if (openFileDialogImport.ShowDialog(this) == DialogResult.OK)
            {
                try
                {
                    var importCategories = ImportCategoriesFile(openFileDialogImport.FileName);
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

                                importCategory.IsDefault = overridingDefault;
                                _assaCategories.Insert(insertIndex, importCategory);

                                var lvi = GetCategoryListViewItem(importCategory);
                                listViewCategories.Items.Insert(insertIndex, lvi);
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

        private List<AssaStorageCategory> ImportCategoriesFile(string fileName)
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

        private void ContextMenuStripCategories_Opening(object sender, EventArgs e)
        {
            var onlyOneSelected = listViewCategories.SelectedItems.Count == 1;
            toolStripMenuItemRename.Visible = onlyOneSelected;

            var moreThanOneExist = listViewCategories.Items.Count > 1;
            deleteAllToolStripMenuItem.Visible = moreThanOneExist;
            toolStripSeparator.Visible = onlyOneSelected && moreThanOneExist;
            moveUpToolStripMenuItem.Visible = onlyOneSelected && moreThanOneExist;
            moveDownToolStripMenuItem.Visible = onlyOneSelected && moreThanOneExist;
            moveToTopToolStripMenuItem.Visible = onlyOneSelected && moreThanOneExist;
            moveToBottomToolStripMenuItem.Visible = onlyOneSelected && moreThanOneExist;

            var moreThanOneSelected = listViewCategories.SelectedItems.Count > 1;
            var selectedIsNotDefault = onlyOneSelected && !SelectedCategory.IsDefault;
            deleteToolStripMenuItem.Visible = selectedIsNotDefault || moreThanOneSelected;
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

            using (var form = new TextPrompt(string.Empty, LanguageSettings.Current.SubStationAlphaStylesCategoriesManager.CategoryName, listViewCategories.SelectedItems[0].Text))
            {
                if (form.ShowDialog() == DialogResult.OK)
                {
                    var newName = form.InputText;
                    if (string.IsNullOrWhiteSpace(newName))
                    {
                        return;
                    }

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

        private void DeleteAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var result = Configuration.Settings.General.PromptDeleteLines ?
                MessageBox.Show(LanguageSettings.Current.SubStationAlphaStylesCategoriesManager.CategoryDelete, string.Empty, MessageBoxButtons.YesNo) :
                DialogResult.Yes;
            if (result == DialogResult.Yes)
            {
                foreach (ListViewItem item in listViewCategories.Items)
                {
                    if (GetCategoryByName(item.Text).IsDefault)
                    {
                        continue;
                    }

                    listViewCategories.Items.Remove(item);
                    _assaCategories.RemoveAll(category => category.Name == item.Text);
                }

                UpdateSelectedIndices();
            }
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
            _assaCategories.Clear();
            _oldAssaCategories.ForEach(category => _assaCategories.Add(category));
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

        private void SubStationAlphaStylesCategoriesManager_ResizeEnd(object sender, EventArgs e)
        {
            SetControlsSize();
        }
    }
}
