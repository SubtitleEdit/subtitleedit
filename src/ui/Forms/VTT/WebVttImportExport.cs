using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Logic;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Forms;
using MessageBox = Nikse.SubtitleEdit.Forms.SeMsgBox.MessageBox;

namespace Nikse.SubtitleEdit.Forms.VTT
{
    public sealed partial class WebVttImportExport : Form
    {
        private readonly List<WebVttStyle> _styles;
        private readonly bool _export;
        public List<WebVttStyle> ImportExportStyles { get; set; }
        public string FileName { get; set; }

        public WebVttImportExport(List<WebVttStyle> styles, string fileName = null)
        {
            InitializeComponent();
            UiUtil.FixFonts(this);

            _styles = styles;
            _export = fileName == null;

            if (!_export)
            {
                _styles = WebVttHelper.GetStyles(FileUtil.ReadAllTextShared(fileName, Encoding.UTF8));
            }

            listViewExportStyles.Columns[0].Width = listViewExportStyles.Width - 20;
            foreach (var style in _styles)
            {
                listViewExportStyles.Items.Add(new ListViewItem(style.Name) { Checked = true, Tag = style });
            }

            Text = LanguageSettings.Current.SubStationAlphaStyles.Export;
            labelStyles.Text = LanguageSettings.Current.SubStationAlphaStyles.Styles;
            toolStripMenuItemInverseSelection.Text = LanguageSettings.Current.Main.Menu.Edit.InverseSelection;
            toolStripMenuItemSelectAll.Text = LanguageSettings.Current.Main.Menu.ContextMenu.SelectAll;
            buttonOK.Text = LanguageSettings.Current.General.Ok;
            buttonCancel.Text = LanguageSettings.Current.General.Cancel;
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            if (_export)
            {
                ExportStyles();
            }
            else
            {
                ImportStyles();
            }
        }

        private void ImportStyles()
        {
            ImportExportStyles = new List<WebVttStyle>();
            foreach (ListViewItem listViewItem in listViewExportStyles.Items)
            {
                if (listViewItem.Checked)
                {
                    ImportExportStyles.Add((WebVttStyle)listViewItem.Tag);
                }
            }

            if (ImportExportStyles.Count > 0)
            {
                DialogResult= DialogResult.OK;
            }
            else
            {
                DialogResult = DialogResult.Cancel;
            }
        }

        private void ExportStyles()
        {
            var exportNames = new List<string>();
            var occursMoreThanOnce = new List<string>();
            ImportExportStyles = new List<WebVttStyle>();
            foreach (ListViewItem item in listViewExportStyles.Items)
            {
                if (item.Checked)
                {
                    ImportExportStyles.Add((WebVttStyle)item.Tag);
                    if (exportNames.Contains(item.Text))
                    {
                        occursMoreThanOnce.Add(item.Text);
                    }
                    else
                    {
                        exportNames.Add(item.Text);
                    }
                }
            }

            if (occursMoreThanOnce.Count > 0)
            {
                MessageBox.Show("Style name must be unique - can only export one style with name: " + string.Join(", ", occursMoreThanOnce));
                return;
            }

            if (ImportExportStyles.Count == 0)
            {
                return;
            }

            saveFileDialogStyle.Title = LanguageSettings.Current.SubStationAlphaStyles.ExportStyleToFile;
            saveFileDialogStyle.InitialDirectory = Configuration.DataDirectory;
            saveFileDialogStyle.Filter = WebVTT.NameOfFormat + "|*.webvtt;*.vtt|" + LanguageSettings.Current.General.AllFiles + "|*.*";
            saveFileDialogStyle.FileName = "my_styles.webvtt";
            if (saveFileDialogStyle.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            var sb = new StringBuilder();
            foreach (var style in ImportExportStyles)
            {
                sb.Append("::cue(" + style.Name.Trim() + ") { " + style + " }");
                sb.Append(Environment.NewLine);
            }

            var content = "WEBVTT" + Environment.NewLine +
                          Environment.NewLine +
                          "STYLE" + Environment.NewLine +
                          sb.ToString().Trim();
            File.WriteAllText(saveFileDialogStyle.FileName, content, Encoding.UTF8);
            DialogResult = DialogResult.OK;
            FileName = saveFileDialogStyle.FileName;
        }


        private void WebVttImportExport_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                DialogResult = DialogResult.Cancel;
            }
        }

        private void toolStripMenuItemSelectAll_Click(object sender, EventArgs e)
        {
            listViewExportStyles.CheckAll();
        }

        private void toolStripMenuItemInverseSelection_Click(object sender, EventArgs e)
        {
            listViewExportStyles.InvertCheck();
        }
    }
}
