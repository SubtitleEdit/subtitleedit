﻿using Nikse.SubtitleEdit.Core;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Logic;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.Forms.Styles
{
    public sealed partial class SubStationAlphaStylesExport : Form
    {
        private readonly string _header;
        private readonly bool _isSubStationAlpha;
        private readonly SubtitleFormat _format;
        public List<string> ExportedStyles { get; set; }

        public SubStationAlphaStylesExport(string header, bool isSubStationAlpha, SubtitleFormat format)
        {
            InitializeComponent();
            UiUtil.FixFonts(this);

            _header = header;
            _isSubStationAlpha = isSubStationAlpha;
            _format = format;
            var styles = AdvancedSubStationAlpha.GetStylesFromHeader(_header);

            listViewExportStyles.Columns[0].Width = listViewExportStyles.Width - 20;
            foreach (var style in styles)
            {
                listViewExportStyles.Items.Add(new ListViewItem(style) { Checked = true });
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
                        string styleFormat = "Format: Name, Fontname, Fontsize, PrimaryColour, SecondaryColour, OutlineColour, BackColour, Bold, Italic, Underline, StrikeOut, ScaleX, ScaleY, Spacing, Angle, BorderStyle, Outline, Shadow, Alignment, MarginL, MarginR, MarginV, Encoding";
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
                                    SsaStyle style = AdvancedSubStationAlpha.GetSsaStyle(styleName, _header);
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
                    foreach (var line in _header.Replace(Environment.NewLine, "\n").Split('\n'))
                    {
                        if (line.StartsWith("style:", StringComparison.OrdinalIgnoreCase))
                        {
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

                                if (toLower.StartsWith("style:" + styleName.ToLowerInvariant().Trim(), StringComparison.Ordinal))
                                {
                                    sb.AppendLine(line);
                                }
                            }
                        }
                        else
                        {
                            sb.AppendLine(line);
                        }
                    }
                    string content = sb.ToString();
                    if (content.TrimEnd().EndsWith("[Events]", StringComparison.Ordinal))
                    {
                        content = content.Trim() + Environment.NewLine +
                            "Format: Layer, Start, End, Style, Actor, MarginL, MarginR, MarginV, Effect, Text" + Environment.NewLine +
                            "Dialogue: 0,0:00:31.91,0:00:33.91,Default,,0,0,0,,My Styles :)";
                    }
                    else if (!content.Contains("[Events]", StringComparison.OrdinalIgnoreCase))
                    {
                        content = content.Trim() + Environment.NewLine +
                            Environment.NewLine +
                            "[Events]" + Environment.NewLine +
                            "Format: Layer, Start, End, Style, Actor, MarginL, MarginR, MarginV, Effect, Text" + Environment.NewLine +
                            "Dialogue: 0,0:00:31.91,0:00:33.91,Default,,0,0,0,,My Styles :)";
                    }
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
