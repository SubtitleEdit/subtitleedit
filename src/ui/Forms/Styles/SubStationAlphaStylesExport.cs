using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Logic;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.Forms.Styles
{
    public sealed partial class SubStationAlphaStylesExport : Form
    {
        private readonly List<SsaStyle> _stylesToExport;
        private readonly bool _isSubStationAlpha;
        private readonly SubtitleFormat _format;
        public List<string> ExportedStyles { get; set; }

        public SubStationAlphaStylesExport(List<SsaStyle> stylesToExport, bool isSubStationAlpha, SubtitleFormat format)
        {
            InitializeComponent();
            UiUtil.FixFonts(this);

            _stylesToExport = stylesToExport;
            _isSubStationAlpha = isSubStationAlpha;
            _format = format;

            listViewExportStyles.Columns[0].Width = listViewExportStyles.Width - 20;
            foreach (var style in stylesToExport)
            {
                listViewExportStyles.Items.Add(new ListViewItem(style.Name) { Checked = true });
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
            ExportedStyles = new List<string>();
            foreach (ListViewItem item in listViewExportStyles.Items)
            {
                if (item.Checked)
                {
                    ExportedStyles.Add(item.Text);
                }
            }
            if (ExportedStyles.Count == 0)
            {
                return;
            }

            saveFileDialogStyle.Title = LanguageSettings.Current.SubStationAlphaStyles.ExportStyleToFile;
            saveFileDialogStyle.InitialDirectory = Configuration.DataDirectory;
            if (_isSubStationAlpha)
            {
                saveFileDialogStyle.Filter = SubStationAlpha.NameOfFormat + "|*.ssa|" + LanguageSettings.Current.General.AllFiles + "|*.*";
                saveFileDialogStyle.FileName = "my_styles.ssa";
            }
            else
            {
                saveFileDialogStyle.Filter = AdvancedSubStationAlpha.NameOfFormat + "|*.ass|" + LanguageSettings.Current.General.AllFiles + "|*.*";
                saveFileDialogStyle.FileName = "my_styles.ass";
            }

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
                        bool stylesOn = false;
                        bool done = false;
                        string styleFormat = SsaStyle.DefaultAssStyleFormat;
                        foreach (string line in File.ReadAllLines(saveFileDialogStyle.FileName))
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
                                foreach (var styleName in ExportedStyles)
                                {
                                    SsaStyle style = _stylesToExport.Single(styleItem => styleItem.Name == styleName);
                                    if (_isSubStationAlpha)
                                    {
                                        sb.AppendLine(style.ToRawSsa(styleFormat));
                                    }
                                    else
                                    {
                                        sb.AppendLine(style.ToRawAss(styleFormat));
                                    }
                                }
                            }
                            sb.AppendLine(line);
                            foreach (var styleName in ExportedStyles)
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

                                if (stylesOn && toLower.StartsWith("style:" + styleName.Trim() + ",", StringComparison.OrdinalIgnoreCase))
                                {
                                    MessageBox.Show(string.Format(LanguageSettings.Current.SubStationAlphaStyles.StyleAlreadyExits, styleName));
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
                    foreach (var styleName in ExportedStyles)
                    {
                        var style = _stylesToExport.Single(styleItem => styleItem.Name == styleName);
                        sb.Append(style.ToRawAss());
                        sb.Append(Environment.NewLine);
                    }

                    string content = string.Format(AdvancedSubStationAlpha.HeaderNoStyles, "Subtitle Edit styles file", sb.ToString().Trim());
                    content = content.Trim() + Environment.NewLine + "Dialogue: 0,0:00:31.91,0:00:33.91,Default,,0,0,0,,My Styles :)";
                    File.WriteAllText(saveFileDialogStyle.FileName, content, Encoding.UTF8);
                    DialogResult = DialogResult.OK;
                }
            }
        }

        private void SubStationAlphaStylesExport_KeyDown(object sender, KeyEventArgs e)
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
