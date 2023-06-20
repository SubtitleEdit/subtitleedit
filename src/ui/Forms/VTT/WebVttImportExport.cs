using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Logic;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.Forms.VTT
{
    public sealed partial class WebVttImportExport : Form
    {
        private readonly List<WebVttStyle> _stylesToExport;
        private readonly SubtitleFormat _format;
        public List<WebVttStyle> ImportExportStyles { get; set; }

        public WebVttImportExport(List<WebVttStyle> stylesToExport)
        {
            InitializeComponent();
            UiUtil.FixFonts(this);

            _stylesToExport = stylesToExport;

            listViewExportStyles.Columns[0].Width = listViewExportStyles.Width - 20;
            foreach (var style in stylesToExport)
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
            saveFileDialogStyle.Filter = SubStationAlpha.NameOfFormat + "|*.webvtt|" + LanguageSettings.Current.General.AllFiles + "|*.*";
            saveFileDialogStyle.FileName = "my_styles.webvtt";

            if (saveFileDialogStyle.ShowDialog(this) == DialogResult.OK)
            {
                if (File.Exists(saveFileDialogStyle.FileName))
                {
                    var s = new Subtitle();
                    var format = s.LoadSubtitle(saveFileDialogStyle.FileName, out _, null);
                    if (format == null)
                    {
                        MessageBox.Show("Not subtitle format: " + _format.Name);
                        return;
                    }

                    if (format.Name != _format.Name)
                    {
                        MessageBox.Show(string.Format("Cannot save {1} style in {0} file!", format.Name, _format.Name));
                    }
                    else
                    {
                        var sb = new StringBuilder();
                        var stylesOn = false;
                        var done = false;
                        var styleFormat = SsaStyle.DefaultAssStyleFormat;
                        foreach (var line in File.ReadAllLines(saveFileDialogStyle.FileName))
                        {
                            if (line.StartsWith("format:", StringComparison.OrdinalIgnoreCase))
                            {
                                styleFormat = line;
                            }
                            else if (line.StartsWith("style:", StringComparison.OrdinalIgnoreCase))
                            {
                                stylesOn = true;
                            }
                            else if (stylesOn && !done)
                            {
                                done = true;
                                foreach (var style in ImportExportStyles)
                                {
                                    sb.AppendLine(style.ToString());
                                }
                            }
                            sb.AppendLine(line);
                            foreach (var style in ImportExportStyles)
                            {
                                var toLower = line.Trim().ToLowerInvariant();
                                while (toLower.Contains(": "))
                                {
                                    toLower = toLower.Replace(": ", ":");
                                }

                                while (toLower.Contains(" :"))
                                {
                                    toLower = toLower.Replace(" :", ":");
                                }

                                if (stylesOn && toLower.StartsWith("cue:(" + style.Name.Trim() + ")", StringComparison.OrdinalIgnoreCase))
                                {
                                    MessageBox.Show(string.Format(LanguageSettings.Current.SubStationAlphaStyles.StyleAlreadyExits, style.Name));
                                    return;
                                }
                            }
                        }
                        File.WriteAllText(saveFileDialogStyle.FileName, sb.ToString(), Encoding.UTF8);
                    }
                }
                else
                {
                    var sb = new StringBuilder();
                    foreach (var style in ImportExportStyles)
                    {
                        sb.Append("cue:(" + style.Name.Trim() + ") { " + style.ToString() + " }");
                        sb.Append(Environment.NewLine);
                    }

                    var content = "WEBVTT" + Environment.NewLine +
                                         Environment.NewLine +
                                         "STYLE" + Environment.NewLine +
                                         sb.ToString().Trim();
                    File.WriteAllText(saveFileDialogStyle.FileName, content, Encoding.UTF8);
                    DialogResult = DialogResult.OK;
                }
            }
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
            foreach (ListViewItem item in listViewExportStyles.Items)
            {
                item.Checked = true;
            }
        }

        private void toolStripMenuItemInverseSelection_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in listViewExportStyles.Items)
            {
                item.Checked = !item.Checked;
            }
        }
    }
}
