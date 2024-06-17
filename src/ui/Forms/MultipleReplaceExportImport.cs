﻿using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Logic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Xml;

namespace Nikse.SubtitleEdit.Forms
{
    public sealed partial class MultipleReplaceExportImport : Form
    {
        public List<string> ChosenGroups;
        private readonly List<MultipleSearchAndReplaceGroup> _groups;
        private readonly bool _export;

        public MultipleReplaceExportImport(List<MultipleSearchAndReplaceGroup> groups, bool export)
        {
            InitializeComponent();
            UiUtil.FixFonts(this);

            _groups = groups;
            _export = export;
            ChosenGroups = new List<string>();

            listViewExportStyles.Columns[0].Width = listViewExportStyles.Width - 20;
            foreach (var group in groups)
            {
                listViewExportStyles.Items.Add(new ListViewItem(group.Name) { Checked = true });
            }

            selectAllToolStripMenuItem.Text = LanguageSettings.Current.FixCommonErrors.SelectAll;
            inverseSelectionToolStripMenuItem.Text = LanguageSettings.Current.FixCommonErrors.InverseSelection;
            buttonOK.Text = LanguageSettings.Current.General.Ok;
            buttonCancel.Text = LanguageSettings.Current.General.Cancel;
            labelGroups.Text = export
                ? LanguageSettings.Current.MultipleReplace.ChooseGroupsToExport
                : LanguageSettings.Current.MultipleReplace.ChooseGroupsToImport;
            Text = export ? LanguageSettings.Current.MultipleReplace.Export : LanguageSettings.Current.MultipleReplace.Import;
        }

        private void MultipleReplaceExportImport_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                DialogResult = DialogResult.Cancel;
            }
        }

        private void buttonOK_Click(object sender, EventArgs e)
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

        private void Import()
        {
            ChosenGroups = new List<string>();
            foreach (ListViewItem item in listViewExportStyles.Items)
            {
                if (item.Checked)
                {
                    ChosenGroups.Add(item.Text);
                }
            }
            DialogResult = DialogResult.OK;
        }

        private void Export()
        {
            ChosenGroups = new List<string>();
            foreach (ListViewItem item in listViewExportStyles.Items)
            {
                if (item.Checked)
                {
                    ChosenGroups.Add(item.Text);
                }
            }

            if (ChosenGroups.Count == 0)
            {
                return;
            }

            saveFileDialogStyle.InitialDirectory = Configuration.DataDirectory;
            saveFileDialogStyle.FileName = "multiple_replace_groups.template";
            saveFileDialogStyle.Title = LanguageSettings.Current.MultipleReplace.ExportRulesTitle;
            saveFileDialogStyle.Filter = LanguageSettings.Current.MultipleReplace.Rules + "|*.template";
            saveFileDialogStyle.OverwritePrompt = true;

            if (saveFileDialogStyle.ShowDialog(this) == DialogResult.OK)
            {
                WriteTemplate(saveFileDialogStyle.FileName, ChosenGroups, _groups);
                DialogResult = DialogResult.OK;
            }
        }

        private void WriteTemplate(string fileName, List<string> exportedGroups, List<MultipleSearchAndReplaceGroup> groups)
        {
            var textWriter = new XmlTextWriter(fileName, null) { Formatting = Formatting.Indented };
            textWriter.WriteStartDocument();
            textWriter.WriteStartElement("Settings", string.Empty);
            textWriter.WriteStartElement("MultipleSearchAndReplaceList", string.Empty);
            foreach (var group in groups.Where(g => exportedGroups.Contains(g.Name)))
            {
                textWriter.WriteStartElement(MultipleReplace.Group, string.Empty);
                textWriter.WriteElementString(MultipleReplace.GroupName, group.Name);
                textWriter.WriteElementString(MultipleReplace.GroupEnabled, group.Enabled.ToString());
                foreach (var item in group.Rules)
                {
                    textWriter.WriteStartElement(MultipleReplace.MultipleSearchAndReplaceItem, string.Empty);
                    textWriter.WriteElementString(MultipleReplace.RuleEnabled, item.Enabled.ToString());
                    textWriter.WriteElementString(MultipleReplace.FindWhat, item.FindWhat);
                    textWriter.WriteElementString(MultipleReplace.ReplaceWith, item.ReplaceWith);
                    textWriter.WriteElementString(MultipleReplace.SearchType, item.SearchType);
                    textWriter.WriteElementString(MultipleReplace.Description, item.Description);
                    textWriter.WriteEndElement();
                }
                textWriter.WriteEndElement();
            }
            textWriter.WriteEndElement();
            textWriter.WriteEndElement();
            textWriter.WriteEndDocument();
            textWriter.Close();
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void selectAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            listViewExportStyles.CheckAll();
        }

        private void inverseSelectionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            listViewExportStyles.InvertCheck();
        }
    }
}
