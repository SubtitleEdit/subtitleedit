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
    public sealed partial class SubStationAlphaStylesCategoriesImportExport : Form
    {
        public List<string> ChosenCategories;
        private readonly List<AssaStorageCategory> _categories;
        private readonly bool _export;

        public SubStationAlphaStylesCategoriesImportExport(List<AssaStorageCategory> categories, bool export)
        {
            InitializeComponent();
            UiUtil.FixFonts(this);

            _categories = categories;
            _export = export;

            listViewCategories.Columns[0].Width = listViewCategories.Width - 20;
            foreach (var category in categories)
            {
                listViewCategories.Items.Add(new ListViewItem(category.Name) { Checked = true });
            }

            Text = export ?
                LanguageSettings.Current.SubStationAlphaStyles.Export :
                LanguageSettings.Current.SubStationAlphaStyles.Import;
            labelCategories.Text = string.Format(LanguageSettings.Current.SubStationAlphaStylesCategoriesManager.ChooseCategories, Text.ToLowerInvariant());
            toolStripMenuItemSelectAll.Text = LanguageSettings.Current.FixCommonErrors.SelectAll;
            toolStripMenuItemInverseSelection.Text = LanguageSettings.Current.FixCommonErrors.InverseSelection;
            buttonOK.Text = LanguageSettings.Current.General.Ok;
            buttonCancel.Text = LanguageSettings.Current.General.Cancel;
        }

        private void Import()
        {
            ChosenCategories = new List<string>();
            foreach (ListViewItem item in listViewCategories.Items)
            {
                if (item.Checked)
                {
                    ChosenCategories.Add(item.Text);
                }
            }

            DialogResult = DialogResult.OK;
        }

        private void Export()
        {
            ChosenCategories = new List<string>();
            foreach (ListViewItem item in listViewCategories.Items)
            {
                if (item.Checked)
                {
                    ChosenCategories.Add(item.Text);
                }
            }

            if (ChosenCategories.Count == 0)
            {
                return;
            }

            saveFileDialog.InitialDirectory = Configuration.DataDirectory;
            saveFileDialog.FileName = "my_assa_categories.template";
            saveFileDialog.Title = LanguageSettings.Current.SubStationAlphaStylesCategoriesManager.ExportCategoriesTitle;
            saveFileDialog.Filter = LanguageSettings.Current.SubStationAlphaStylesCategoriesManager.Categories + "|*.template";

            if (saveFileDialog.ShowDialog(this) == DialogResult.OK)
            {
                WriteTemplate(saveFileDialog.FileName, ChosenCategories, _categories);
                DialogResult = DialogResult.OK;
            }
        }

        private void WriteTemplate(string fileName, List<string> exportedCategories, List<AssaStorageCategory> categories)
        {
            var textWriter = new XmlTextWriter(fileName, null) { Formatting = Formatting.Indented };
            textWriter.WriteStartDocument();
            textWriter.WriteStartElement("Settings", string.Empty);
            textWriter.WriteStartElement("AssaStorageCategories", string.Empty);
            foreach (var category in categories.Where(g => exportedCategories.Contains(g.Name)))
            {
                textWriter.WriteStartElement(SubStationAlphaStylesCategoriesManager.Category, string.Empty);
                textWriter.WriteElementString(SubStationAlphaStylesCategoriesManager.CategoryName, category.Name);
                textWriter.WriteElementString(SubStationAlphaStylesCategoriesManager.CategoryIsDefault, category.IsDefault.ToString());
                foreach (var style in category.Styles)
                {
                    textWriter.WriteStartElement(SubStationAlphaStylesCategoriesManager.Style, string.Empty);
                    textWriter.WriteElementString(SubStationAlphaStylesCategoriesManager.StyleName, style.Name);
                    textWriter.WriteElementString(SubStationAlphaStylesCategoriesManager.StyleFontName, style.FontName);
                    textWriter.WriteElementString(SubStationAlphaStylesCategoriesManager.StyleFontSize, style.FontSize.ToString());
                    textWriter.WriteElementString(SubStationAlphaStylesCategoriesManager.StylePrimaryColor, ColorTranslator.ToHtml(style.Primary));
                    textWriter.WriteElementString(SubStationAlphaStylesCategoriesManager.StyleSecondaryColor, ColorTranslator.ToHtml(style.Secondary));
                    textWriter.WriteElementString(SubStationAlphaStylesCategoriesManager.StyleOutlineColor, ColorTranslator.ToHtml(style.Outline));
                    textWriter.WriteElementString(SubStationAlphaStylesCategoriesManager.StyleBackgroundColor, ColorTranslator.ToHtml(style.Background));
                    textWriter.WriteElementString(SubStationAlphaStylesCategoriesManager.StyleShadowWidth, style.ShadowWidth.ToString());
                    textWriter.WriteElementString(SubStationAlphaStylesCategoriesManager.OutlineWidth, style.OutlineWidth.ToString());
                    textWriter.WriteElementString(SubStationAlphaStylesCategoriesManager.Alignment, style.Alignment);
                    textWriter.WriteElementString(SubStationAlphaStylesCategoriesManager.MarginLeft, style.MarginLeft.ToString());
                    textWriter.WriteElementString(SubStationAlphaStylesCategoriesManager.MarginRight, style.MarginRight.ToString());
                    textWriter.WriteElementString(SubStationAlphaStylesCategoriesManager.MarginVertical, style.MarginVertical.ToString());
                    textWriter.WriteElementString(SubStationAlphaStylesCategoriesManager.BorderStyle, style.BorderStyle);
                    textWriter.WriteEndElement();
                }
                textWriter.WriteEndElement();
            }
            textWriter.WriteEndElement();
            textWriter.WriteEndElement();
            textWriter.WriteEndDocument();
            textWriter.Close();
        }

        private void ButtonOK_Click(object sender, EventArgs e)
        {
            if (_export)
            {
                Export();
            }
            else
            {
                Import();
            }
        }

        private void SubStationAlphaCategoriesImportExport_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                DialogResult = DialogResult.Cancel;
            }
        }

        private void ToolStripMenuItemSelectAll_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in listViewCategories.Items)
            {
                item.Checked = true;
            }
        }

        private void ToolStripMenuItemInverseSelection_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in listViewCategories.Items)
            {
                item.Checked = !item.Checked;
            }
        }
    }
}
