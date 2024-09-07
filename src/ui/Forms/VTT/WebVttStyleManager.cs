﻿using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.VideoPlayers;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Text;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using MessageBox = Nikse.SubtitleEdit.Forms.SeMsgBox.MessageBox;

namespace Nikse.SubtitleEdit.Forms.VTT
{
    public sealed partial class WebVttStyleManager : Form
    {
        public string Header { get; set; }
        private readonly List<WebVttStyle> _webVttStyles;
        private readonly List<WebVttStyle> _originalWebVttStyles;
        private WebVttStyle _currentStyle;
        private readonly Subtitle _subtitle;
        private bool _doUpdate;
        private readonly Timer _previewTimer = new Timer();
        private Bitmap _backgroundImage;
        private readonly bool _backgroundImageDark;
        private LibMpvDynamic _mpv;
        private string _mpvTextFileName;

        public WebVttStyleManager(Subtitle subtitle, int index)
        {
            UiUtil.PreInitialize(this);
            InitializeComponent();
            UiUtil.FixFonts(this);
            UiUtil.FixLargeFonts(this, buttonOK);

            _subtitle = subtitle;
            _webVttStyles = WebVttHelper.GetStyles(subtitle.Header);
            _originalWebVttStyles = WebVttHelper.GetStyles(subtitle.Header);
            Header = subtitle.Header;
            InitializeStylesListView();
            CheckDuplicateStyles();

            var fontNames = new List<string>();
            foreach (var x in FontHelper.GetAllSupportedFontFamilies())
            {
                fontNames.Add(x.Name);
            }

            comboBoxFontName.Items.Clear();
            comboBoxFontName.Items.AddRange(fontNames.ToArray<object>());
            labelInfo.Text = string.Empty;

            Text = LanguageSettings.Current.WebVttStyleManager.Title;
            var l = LanguageSettings.Current.SubStationAlphaStyles;
            groupBoxStyles.Text = l.Styles;
            listViewStyles.Columns[0].Text = l.Name;
            listViewStyles.Columns[1].Text = l.FontName;
            listViewStyles.Columns[2].Text = l.FontSize;
            listViewStyles.Columns[3].Text = LanguageSettings.Current.General.Italic;
            listViewStyles.Columns[4].Text = LanguageSettings.Current.GenerateBlankVideo.Background;
            listViewStyles.Columns[5].Text = LanguageSettings.Current.General.Text;
            listViewStyles.Columns[6].Text = l.UseCount;
            groupBoxProperties.Text = l.Properties;
            labelStyleName.Text = l.Name;
            groupBoxFont.Text = l.Font;
            labelFontName.Text = l.FontName;
            labelFontSize.Text = l.FontSize;
            checkBoxFontItalic.Text = LanguageSettings.Current.General.Italic;
            checkBoxFontBold.Text = LanguageSettings.Current.General.Bold;
            checkBoxFontUnderline.Text = LanguageSettings.Current.General.Underline;
            checkBoxStrikeout.Text = LanguageSettings.Current.General.Strikeout;
            buttonPrimaryColor.Text = l.Primary;
            buttonShadowColor.Text = l.Shadow;
            buttonBackgroundColor.Text = l.Back;
            labelShadow.Text = l.Shadow;
            buttonImport.Text = l.Import;
            buttonExport.Text = l.Export;
            buttonCopy.Text = l.Copy;
            buttonAdd.Text = l.New;
            buttonRemove.Text = l.Remove;
            buttonRemoveAll.Text = l.RemoveAll;
            groupBoxPreview.Text = LanguageSettings.Current.General.Preview;

            deleteToolStripMenuItem.Text = l.Remove;
            //removeAndReplaceWithToolStripMenuItem.Text = l.ReplaceWith;
            toolStripMenuItemRemoveAll.Text = l.RemoveAll;
            moveUpToolStripMenuItem.Text = LanguageSettings.Current.DvdSubRip.MoveUp;
            moveDownToolStripMenuItem.Text = LanguageSettings.Current.DvdSubRip.MoveDown;
            moveTopToolStripMenuItem.Text = LanguageSettings.Current.MultipleReplace.MoveToTop;
            moveBottomToolStripMenuItem.Text = LanguageSettings.Current.MultipleReplace.MoveToBottom;
            newToolStripMenuItemNew.Text = l.New;
            copyToolStripMenuItemCopy.Text = l.Copy;
            toolStripMenuItemImport.Text = l.Import;
            toolStripMenuItemExport.Text = l.Export;

            checkBoxColorEnabled.Text = LanguageSettings.Current.MultipleReplace.Enabled;
            checkBoxBackgroundColorEnabled.Text = LanguageSettings.Current.MultipleReplace.Enabled;
            checkBoxShadowEnabled.Text = LanguageSettings.Current.MultipleReplace.Enabled;

            buttonApply.Text = LanguageSettings.Current.General.Apply;
            buttonOK.Text = LanguageSettings.Current.General.Ok;
            buttonCancel.Text = LanguageSettings.Current.General.Cancel;

            comboBoxFontName.Left = labelFontName.Right + 5;
            labelFontSize.Left = comboBoxFontName.Right + 15;
            numericUpDownFontSize.Left = labelFontSize.Right + 5;

            checkBoxFontItalic.Left = checkBoxFontBold.Right + 15;
            checkBoxFontUnderline.Left = checkBoxFontItalic.Right + 15;
            checkBoxStrikeout.Left = checkBoxFontUnderline.Right + 15;

            groupBoxBefore.Text = LanguageSettings.Current.General.Before;
            groupBoxAfter.Text = LanguageSettings.Current.General.After;

            _backgroundImageDark = Configuration.Settings.General.UseDarkTheme;
            _previewTimer.Interval = 200;
            _previewTimer.Tick += PreviewTimerTick;
        }

        private void PreviewTimerTick(object sender, EventArgs e)
        {
            _previewTimer.Stop();
            GeneratePreviewReal();
        }

        private void InitializeStylesListView()
        {
            listViewStyles.Items.Clear();
            foreach (var style in _webVttStyles)
            {
                AddStyle(listViewStyles, style, _subtitle);
            }

            if (listViewStyles.Items.Count > 0)
            {
                listViewStyles.Items[0].Selected = true;
            }
        }

        public static void AddStyle(ListView lv, WebVttStyle style, Subtitle subtitle)
        {
            AddStyle(lv, style, subtitle, lv.Items.Count);
        }

        public static void AddStyle(ListView lv, WebVttStyle style, Subtitle subtitle, int insertAt)
        {
            // Style name - 0
            var item = new ListViewItem(style.Name.Trim().TrimStart('.'))
            {
                Checked = true,
                UseItemStyleForSubItems = false,
                Tag = style,
            };

            // Font name - 1
            var subItem = new ListViewItem.ListViewSubItem(item, string.IsNullOrEmpty(style.FontName) ? "- " : style.FontName);
            item.SubItems.Add(subItem);

            // Font size - 2
            subItem = new ListViewItem.ListViewSubItem(item, style.FontSize.HasValue ? style.FontSize.HasValue.ToString(CultureInfo.InvariantCulture) : "-");
            item.SubItems.Add(subItem);

            // Italic - 3
            subItem = new ListViewItem.ListViewSubItem(item, style.Italic.HasValue && style.Italic.Value ? LanguageSettings.Current.General.Yes : "-");
            item.SubItems.Add(subItem);

            // Background color - 4
            subItem = new ListViewItem.ListViewSubItem(item, string.Empty) { BackColor = style.BackgroundColor.HasValue ? style.BackgroundColor.Value : UiUtil.BackColor };
            item.SubItems.Add(subItem);

            // Color - 5
            subItem = new ListViewItem.ListViewSubItem(item, string.Empty)
            {
                BackColor = style.Color ?? UiUtil.BackColor,
                Text = LanguageSettings.Current.General.Text,
            };

            try
            {
                var fontStyle = FontStyle.Regular;
                if (style.Bold.HasValue && style.Bold == true)
                {
                    fontStyle |= FontStyle.Bold;
                }

                if (style.Italic.HasValue && style.Italic == true)
                {
                    fontStyle |= FontStyle.Italic;
                }

                if (style.Underline.HasValue && style.Underline == true)
                {
                    fontStyle |= FontStyle.Underline;
                }

                if (style.StrikeThrough.HasValue && style.StrikeThrough == true)
                {
                    fontStyle |= FontStyle.Strikeout;
                }

                subItem.Font = new Font(style.FontName, subItem.Font.Size, fontStyle);
            }
            catch
            {
                // Ignored
            }

            item.SubItems.Add(subItem);


            // Use count
            var count = 0;
            foreach (var p in subtitle.Paragraphs)
            {
                if (p.Text.Contains(style.Name + ".", StringComparison.Ordinal) ||
                    p.Text.Contains(style.Name + ">", StringComparison.Ordinal))
                {
                    count++;
                }
            }

            subItem = new ListViewItem.ListViewSubItem(item, count.ToString());
            item.SubItems.Add(subItem);

            lv.Items.Insert(insertAt, item);
        }

        private void listViewStyles_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listViewStyles.SelectedItems.Count == 1)
            {
                var styleName = "." + listViewStyles.SelectedItems[0].Text;
                var style = _webVttStyles.FirstOrDefault(p => p.Name == styleName);
                SetControlsFromStyle(style);
                _doUpdate = true;
                groupBoxProperties.Enabled = true;
            }
            else
            {
                groupBoxProperties.Enabled = false;
                _doUpdate = false;
                _currentStyle = null;
                pictureBoxPreview.Image?.Dispose();
                pictureBoxPreview.Image = new Bitmap(1, 1);
            }

            UpdateCurrentFileButtonsState();
            GeneratePreview();
        }

        private void SetControlsFromStyle(WebVttStyle style)
        {
            _currentStyle = style;

            textBoxStyleName.Text = style.Name.RemoveChar('.');
            textBoxStyleName.BackColor = listViewStyles.BackColor;
            comboBoxFontName.Text = style.FontName;

            checkBoxFontItalic.Checked = style.Italic.HasValue && style.Italic.Value;
            checkBoxFontBold.Checked = style.Bold.HasValue && style.Bold.Value;
            checkBoxFontUnderline.Checked = style.Underline.HasValue && style.Underline.Value;
            checkBoxStrikeout.Checked = style.StrikeThrough.HasValue && style.StrikeThrough.Value;

            if (style.FontSize.HasValue && style.FontSize > 0 && style.FontSize <= numericUpDownFontSize.Maximum)
            {
                numericUpDownFontSize.Value = style.FontSize.Value;
            }
            else
            {
                numericUpDownFontSize.Value = 0;
            }

            if (style.Color.HasValue)
            {
                panelPrimaryColor.BackColor = style.Color.Value;
                checkBoxColorEnabled.Checked = true;
            }
            else
            {
                checkBoxColorEnabled.Checked = false;
                panelPrimaryColor.BackColor = Color.Transparent;
            }

            if (style.BackgroundColor.HasValue)
            {
                panelBackgroundColor.BackColor = style.BackgroundColor.Value;
                checkBoxBackgroundColorEnabled.Checked = true;
            }
            else
            {
                checkBoxBackgroundColorEnabled.Checked = false;
                panelBackgroundColor.BackColor = Color.Transparent;
            }

            if (style.ShadowColor.HasValue)
            {
                panelBackgroundColor.BackColor = style.ShadowColor.Value;
            }
            else
            {
                panelShadowColor.BackColor = Color.Transparent;
            }

            if (style.ShadowWidth.HasValue && style.ShadowWidth >= 0 && style.ShadowWidth <= numericUpDownShadowWidth.Maximum)
            {
                numericUpDownShadowWidth.Value = style.ShadowWidth.Value;
            }
            else
            {
                numericUpDownShadowWidth.Value = 0;
            }

            var beforeStyle = _originalWebVttStyles.FirstOrDefault(p => p.Name == style.Name);
            if (beforeStyle == null)
            {
                labelBefore.Text = string.Empty;
            }
            else
            {
                labelBefore.Text = beforeStyle.ToString().Replace("; ", ";" + Environment.NewLine);
            }

            UpdateRawBeforeStyle();
        }

        private void UpdateRawBeforeStyle()
        {
            if (_currentStyle == null)
            {
                labelAfter.Text = string.Empty;
                labelBefore.Text = string.Empty;
                return;
            }

            labelAfter.Text = _currentStyle.ToString().Replace("; ", ";" + Environment.NewLine);
        }

        private void UpdateCurrentFileButtonsState()
        {
            var oneOrMoreSelected = listViewStyles.SelectedItems.Count > 0;
            buttonRemove.Enabled = oneOrMoreSelected;
            buttonCopy.Enabled = oneOrMoreSelected;
        }

        private void panelPrimaryColor_Click(object sender, EventArgs e)
        {
            using (var colorChooser = new ColorChooser { Color = panelPrimaryColor.BackColor })
            {
                if (colorChooser.ShowDialog() != DialogResult.OK || _currentStyle == null)
                {
                    return;
                }

                checkBoxColorEnabled.Checked = true;
                panelPrimaryColor.BackColor = colorChooser.Color;
                _currentStyle.Color = colorChooser.Color;
                listViewStyles.SelectedItems[0].SubItems[5].BackColor = colorChooser.Color;
                UpdateRawBeforeStyle();
            }
        }

        private void buttonPrimaryColor_Click(object sender, EventArgs e)
        {
            panelPrimaryColor_Click(null, null);
        }

        private void panelBackgroundColor_Click(object sender, EventArgs e)
        {
            using (var colorChooser = new ColorChooser { Color = panelBackgroundColor.BackColor })
            {
                if (colorChooser.ShowDialog() != DialogResult.OK || _currentStyle == null)
                {
                    return;
                }

                checkBoxBackgroundColorEnabled.Checked = true;
                panelBackgroundColor.BackColor = colorChooser.Color;
                _currentStyle.BackgroundColor = colorChooser.Color;
                listViewStyles.SelectedItems[0].SubItems[4].BackColor = colorChooser.Color;
                UpdateRawBeforeStyle();
            }
        }

        private void buttonBackgroundColor_Click(object sender, EventArgs e)
        {
            panelBackgroundColor_Click(null, null);
        }

        private void panelShadowColor_Click(object sender, EventArgs e)
        {
            using (var colorChooser = new ColorChooser { Color = panelShadowColor.BackColor })
            {
                if (colorChooser.ShowDialog() != DialogResult.OK || _currentStyle == null)
                {
                    return;
                }

                checkBoxShadowEnabled.Checked = true;
                panelShadowColor.BackColor = colorChooser.Color;
                _currentStyle.ShadowColor = colorChooser.Color;
            }

            UpdateRawBeforeStyle();
        }

        private void checkBoxColorEnabled_CheckedChanged(object sender, EventArgs e)
        {
            if (!_doUpdate || _currentStyle == null)
            {
                return;
            }

            var checkBox = (CheckBox)sender;
            if (checkBox.Checked)
            {
                _currentStyle.Color = panelPrimaryColor.BackColor;
            }
            else
            {
                _currentStyle.Color = null;
            }

            listViewStyles.SelectedItems[0].SubItems[5].BackColor = _currentStyle.Color ?? UiUtil.BackColor;
            UpdateRawBeforeStyle();
        }

        private void checkBoxBackgroundColorEnabled_CheckedChanged(object sender, EventArgs e)
        {
            if (!_doUpdate || _currentStyle == null)
            {
                return;
            }

            var checkBox = (CheckBox)sender;
            if (checkBox.Checked)
            {
                _currentStyle.BackgroundColor = panelBackgroundColor.BackColor;
            }
            else
            {
                _currentStyle.BackgroundColor = null;
            }

            listViewStyles.SelectedItems[0].SubItems[4].BackColor = _currentStyle.BackgroundColor ?? UiUtil.BackColor;
            UpdateRawBeforeStyle();
        }

        private void checkBoxShadowEnabled_CheckedChanged(object sender, EventArgs e)
        {
            if (!_doUpdate || _currentStyle == null)
            {
                return;
            }

            var checkBox = (CheckBox)sender;
            if (checkBox.Checked)
            {
                _currentStyle.ShadowColor = panelBackgroundColor.BackColor;
            }
            else
            {
                _currentStyle.ShadowColor = null;
            }

            UpdateRawBeforeStyle();
        }

        private void comboBoxFontName_TextChanged(object sender, EventArgs e)
        {
            if (!_doUpdate || _currentStyle == null)
            {
                return;
            }

            if (string.IsNullOrEmpty(comboBoxFontName.Text))
            {
                _currentStyle.FontName = null;
            }
            else
            {
                _currentStyle.FontName = comboBoxFontName.Text;
            }

            listViewStyles.SelectedItems[0].SubItems[1].Text = comboBoxFontName.Text;
            UpdateRawBeforeStyle();
        }

        private void numericUpDownFontSize_ValueChanged(object sender, EventArgs e)
        {
            if (!_doUpdate || _currentStyle == null)
            {
                return;
            }

            if (numericUpDownFontSize.Value == 0)
            {
                _currentStyle.FontSize = null;
            }
            else
            {
                _currentStyle.FontSize = numericUpDownFontSize.Value;
            }

            listViewStyles.SelectedItems[0].SubItems[2].Text = _currentStyle.FontSize == null ? "-" : numericUpDownFontSize.Value.ToString();
            UpdateRawBeforeStyle();
        }

        private void buttonAdd_Click(object sender, EventArgs e)
        {
            var style = new WebVttStyle { Name = GetStyleName() };
            _webVttStyles.Add(style);
            AddStyle(listViewStyles, style, _subtitle);

            foreach (ListViewItem listViewItem in listViewStyles.Items)
            {
                listViewItem.Selected = false;
            }

            listViewStyles.Items[listViewStyles.Items.Count - 1].Selected = true;
            listViewStyles.EnsureVisible(listViewStyles.Items.Count - 1);

            CheckDuplicateStyles();
        }

        private string GetStyleName()
        {
            const string baseName = ".new";
            var name = baseName;
            var count = 2;
            while (_webVttStyles.Any(p => p.Name == name))
            {
                name = baseName + " " + count;
                count++;
            }

            return name;
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            SetHeader();
            DialogResult = DialogResult.OK;
        }

        private void SetHeader()
        {
            var header = _subtitle.Header ?? string.Empty;
            if (!header.StartsWith("WEBVTT"))
            {
                header = "WEBVTT";
            }

            var styleOn = false;
            var sb = new StringBuilder();
            foreach (var line in header.SplitToLines())
            {
                if (line == "STYLE")
                {
                    styleOn = true;
                }
                else if (line == string.Empty && styleOn)
                {
                    styleOn = false;
                }
                else if (styleOn)
                {
                    // ignore
                }
                else
                {
                    sb.AppendLine(line);
                }
            }

            var sbStyles = new StringBuilder();
            sbStyles.AppendLine("STYLE");
            foreach (var style in _webVttStyles)
            {
                var rawStyle = "::cue(." + style.Name.RemoveChar('.') + ") { " + WebVttHelper.GetCssProperties(style) + " }";
                sbStyles.AppendLine(rawStyle);
            }

            header = sb.ToString().Trim() + Environment.NewLine + Environment.NewLine + sbStyles;
            Header = header;
        }

        private void buttonApply_Click(object sender, EventArgs e)
        {
            SetHeader();
            _subtitle.Header = Header;
        }

        private void buttonRemove_Click(object sender, EventArgs e)
        {
            if (listViewStyles.SelectedItems.Count == 0)
            {
                return;
            }

            var askText = listViewStyles.SelectedItems.Count > 1 ? string.Format(LanguageSettings.Current.Main.DeleteXLinesPrompt, listViewStyles.SelectedItems.Count) : LanguageSettings.Current.Main.DeleteOneLinePrompt;

            if (Configuration.Settings.General.PromptDeleteLines &&
                MessageBox.Show(askText, string.Empty, MessageBoxButtons.YesNoCancel) != DialogResult.Yes)
            {
                return;
            }

            foreach (ListViewItem selectedItem in listViewStyles.SelectedItems)
            {
                var name = "." + selectedItem.Text;
                listViewStyles.Items.RemoveAt(selectedItem.Index);
                _webVttStyles.RemoveAll(p => p.Name == name);
            }

            if (listViewStyles.Items.Count == 0)
            {
                InitializeStylesListView();
            }

            CheckDuplicateStyles();
        }

        private void CheckDuplicateStyles()
        {
            labelDuplicateStyleNames.Text = string.Empty;
            var duplicateStyles = new List<string>();
            foreach (var style in _webVttStyles)
            {
                if (_webVttStyles.Count(p => p.Name == style.Name) > 1 && !duplicateStyles.Contains(style.Name))
                {
                    duplicateStyles.Add(style.Name);
                }
            }

            if (duplicateStyles.Count > 0)
            {
                labelDuplicateStyleNames.Text = string.Format(LanguageSettings.Current.SubStationAlphaStyles.DuplicateStyleNames, string.Join(", ", duplicateStyles));
            }
        }

        private void buttonRemoveAll_Click(object sender, EventArgs e)
        {
            var askText = listViewStyles.Items.Count == 1 ? LanguageSettings.Current.Main.DeleteOneLinePrompt : string.Format(LanguageSettings.Current.Main.DeleteXLinesPrompt, listViewStyles.Items.Count);

            if (MessageBox.Show(askText, string.Empty, MessageBoxButtons.YesNoCancel) != DialogResult.Yes)
            {
                return;
            }

            listViewStyles.Items.Clear();
            _webVttStyles.Clear();
            InitializeStylesListView();
            CheckDuplicateStyles();
        }

        private void buttonCopy_Click(object sender, EventArgs e)
        {
            var selectionCount = listViewStyles.SelectedItems.Count;
            if (selectionCount <= 0)
            {
                return;
            }

            var tempStyles = new List<WebVttStyle>();
            foreach (ListViewItem selectedItem in listViewStyles.SelectedItems)
            {
                var styleName = selectedItem.Text;
                var oldStyle = _currentStyle;
                var style = new WebVttStyle(oldStyle) { Name = "." + string.Format(LanguageSettings.Current.SubStationAlphaStyles.CopyOfY, styleName.RemoveChar('.')) }; // Copy constructor
                style.Name = style.Name.Replace(" ", "-");

                if (_webVttStyles.FirstOrDefault(p => p.Name == style.Name) != null)
                {
                    var count = 2;
                    var doRepeat = true;
                    while (doRepeat)
                    {
                        style.Name = "." + string.Format(LanguageSettings.Current.SubStationAlphaStyles.CopyXOfY, count, styleName.RemoveChar('.'));
                        style.Name = style.Name.Replace(" ", "-");
                        doRepeat = _webVttStyles.FirstOrDefault(p => p.Name == style.Name) != null;
                        count++;
                    }
                }

                _doUpdate = false;
                tempStyles.Add(style);
                _doUpdate = true;

                selectedItem.Selected = false;
            }

            foreach (var style in tempStyles)
            {
                AddStyle(listViewStyles, style, _subtitle);
                _webVttStyles.Add(style);
            }

            listViewStyles.Items[listViewStyles.Items.Count - 1].Selected = true;
            listViewStyles.Items[listViewStyles.Items.Count - 1].Focused = true;

            CheckDuplicateStyles();
        }

        private void buttonExport_Click(object sender, EventArgs e)
        {
            if (listViewStyles.Items.Count == 0)
            {
                return;
            }

            var styles = new List<WebVttStyle>();
            foreach (ListViewItem item in listViewStyles.Items)
            {
                styles.Add((WebVttStyle)item.Tag);
            }

            using (var form = new WebVttImportExport(styles))
            {
                if (form.ShowDialog(this) == DialogResult.OK)
                {
                    var styleNames = "(" + form.ImportExportStyles.Count + ")";
                    labelInfo.Text = string.Format(LanguageSettings.Current.SubStationAlphaStyles.StyleXExportedToFileY, styleNames, form.FileName);

                    TaskDelayHelper.RunDelayed(TimeSpan.FromMilliseconds(3500), () =>
                    {
                        try
                        {
                            labelInfo.Text = string.Empty;
                        }
                        catch
                        {
                            // ignore
                        }
                    });
                }
            }
        }

        private void buttonImport_Click(object sender, EventArgs e)
        {
            using (var openFileDialog1 = new OpenFileDialog())
            {
                openFileDialog1.Title = LanguageSettings.Current.General.OpenSubtitle;
                openFileDialog1.FileName = string.Empty;
                openFileDialog1.Filter = "WebVTT files|*.webvtt;*.vtt";
                openFileDialog1.FileName = string.Empty;
                if (openFileDialog1.ShowDialog() != DialogResult.OK)
                {
                    return;
                }

                using (var form = new WebVttImportExport(_webVttStyles, openFileDialog1.FileName))
                {
                    if (form.ShowDialog(this) == DialogResult.OK)
                    {
                        foreach (var style in form.ImportExportStyles)
                        {
                            style.Name = GetUniqueName(style);
                            AddStyle(listViewStyles, style, _subtitle);
                            _webVttStyles.Add(style);
                        }
                    }
                }
            }

            GeneratePreview();
        }

        private string GetUniqueName(WebVttStyle style)
        {
            var styleName = style.Name;
            var newName = styleName;
            if (_webVttStyles.FirstOrDefault(p => p.Name == style.Name) != null)
            {
                var count = 2;
                var doRepeat = true;
                while (doRepeat)
                {
                    newName = "." + string.Format(LanguageSettings.Current.SubStationAlphaStyles.CopyXOfY, count, styleName.RemoveChar('.'));
                    newName = newName.Replace(" ", "-");
                    doRepeat = _webVttStyles.FirstOrDefault(p => p.Name == style.Name) != null;
                    count++;
                }
            }

            return newName;
        }

        private void listViewStyles_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Modifiers == Keys.Control && e.KeyCode == Keys.C)
            {
                buttonCopy_Click(null, null);
            }
            else if (e.Modifiers == Keys.None && e.KeyCode == Keys.Delete)
            {
                buttonRemove_Click(null, null);
            }
            else if (e.KeyCode == Keys.A && e.Modifiers == Keys.Control)
            {
                listViewStyles.SelectAll();
                e.SuppressKeyPress = true;
            }
            else if (e.KeyCode == Keys.D && e.Modifiers == Keys.Control)
            {
                listViewStyles.SelectFirstSelectedItemOnly();
                e.SuppressKeyPress = true;
            }
            else if (e.KeyCode == Keys.I && e.Modifiers == (Keys.Control | Keys.Shift))
            {
                listViewStyles.InverseSelection();
                e.SuppressKeyPress = true;
            }
        }

        private void textBoxStyleName_TextChanged(object sender, EventArgs e)
        {
            if (!_doUpdate || listViewStyles.SelectedItems.Count != 1)
            {
                return;
            }

            var newName = textBoxStyleName.Text.RemoveChar('.', ' ').Trim();
            if (_currentStyle.Name == "." + newName)
            {
                return;
            }

            if (string.IsNullOrEmpty(newName) || _webVttStyles.Any(p => p.Name == "." + newName))
            {
                textBoxStyleName.BackColor = Configuration.Settings.Tools.ListViewSyntaxErrorColor;
            }
            else
            {
                textBoxStyleName.BackColor = listViewStyles.BackColor;
                _currentStyle.Name = "." + newName;
                listViewStyles.SelectedItems[0].Text = newName;
            }

            GeneratePreview();
        }

        private void checkBoxFontBold_CheckedChanged(object sender, EventArgs e)
        {
            if (!_doUpdate || listViewStyles.SelectedItems.Count != 1)
            {
                return;
            }

            if (checkBoxFontBold.Checked)
            {
                _currentStyle.Bold = true;
            }
            else
            {
                _currentStyle.Bold = null;
            }

            UpdateRawBeforeStyle();
            GeneratePreview();
        }

        private void checkBoxFontItalic_CheckedChanged(object sender, EventArgs e)
        {
            if (!_doUpdate || listViewStyles.SelectedItems.Count != 1)
            {
                return;
            }

            if (checkBoxFontItalic.Checked)
            {
                _currentStyle.Italic = true;
            }
            else
            {
                _currentStyle.Italic = null;
            }

            listViewStyles.SelectedItems[0].SubItems[3].Text = _currentStyle.Italic.HasValue && _currentStyle.Italic.Value ? LanguageSettings.Current.General.Yes : "-";
            UpdateRawBeforeStyle();
            GeneratePreview();
        }

        private void checkBoxFontUnderline_CheckedChanged(object sender, EventArgs e)
        {
            if (!_doUpdate || listViewStyles.SelectedItems.Count != 1)
            {
                return;
            }

            if (checkBoxFontUnderline.Checked)
            {
                _currentStyle.Underline = true;
            }
            else
            {
                _currentStyle.Underline = null;
            }

            UpdateRawBeforeStyle();
            GeneratePreview();
        }

        private void checkBoxStrikeout_CheckedChanged(object sender, EventArgs e)
        {
            if (!_doUpdate || listViewStyles.SelectedItems.Count != 1)
            {
                return;
            }

            if (checkBoxStrikeout.Checked)
            {
                _currentStyle.StrikeThrough = true;
            }
            else
            {
                _currentStyle.StrikeThrough = null;
            }

            UpdateRawBeforeStyle();
            GeneratePreview();
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

        private void toolStripMenuItemImport_Click(object sender, EventArgs e)
        {
            buttonImport_Click(null, null);
        }

        private void toolStripMenuItemExport_Click(object sender, EventArgs e)
        {
            buttonExport_Click(null, null);
        }

        private void moveUpToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MoveUp(listViewStyles);
        }

        private void MoveUp(ListView listView)
        {
            if (listView.SelectedItems.Count != 1)
            {
                return;
            }

            var idx = listView.SelectedItems[0].Index;
            if (idx == 0)
            {
                return;
            }

            var item = listView.SelectedItems[0];
            listView.Items.RemoveAt(idx);

            var style = _webVttStyles[idx];
            _webVttStyles.RemoveAt(idx);
            _webVttStyles.Insert(idx - 1, style);

            idx--;
            listView.Items.Insert(idx, item);
        }

        private void MoveDown(ListView listView)
        {
            if (listView.SelectedItems.Count != 1)
            {
                return;
            }

            var idx = listView.SelectedItems[0].Index;
            if (idx >= listView.Items.Count - 1)
            {
                return;
            }

            var item = listView.SelectedItems[0];
            listView.Items.RemoveAt(idx);
            var style = _webVttStyles[idx];
            _webVttStyles.RemoveAt(idx);
            _webVttStyles.Insert(idx + 1, style);
            idx++;
            listView.Items.Insert(idx, item);
        }

        private void MoveToTop(ListView listView)
        {
            if (listView.SelectedItems.Count != 1)
            {
                return;
            }

            var idx = listView.SelectedItems[0].Index;
            if (idx == 0)
            {
                return;
            }

            var item = listView.SelectedItems[0];
            listView.Items.RemoveAt(idx);

            var style = _webVttStyles[idx];
            _webVttStyles.RemoveAt(idx);
            _webVttStyles.Insert(0, style);

            idx = 0;
            listView.Items.Insert(idx, item);
        }

        private void MoveToBottom(ListView listView)
        {
            if (listView.SelectedItems.Count != 1)
            {
                return;
            }

            var idx = listView.SelectedItems[0].Index;
            if (idx == listView.Items.Count - 1)
            {
                return;
            }

            var item = listView.SelectedItems[0];
            listView.Items.RemoveAt(idx);

            var style = _webVttStyles[idx];
            _webVttStyles.RemoveAt(idx);
            _webVttStyles.Add(style);

            listView.Items.Add(item);
        }

        private void moveDownToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MoveDown(listViewStyles);
        }

        private void moveTopToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MoveToTop(listViewStyles);
        }

        private void moveBottomToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MoveToBottom(listViewStyles);
        }

        private void GeneratePreview()
        {
            if (_previewTimer.Enabled)
            {
                _previewTimer.Stop();
                _previewTimer.Start();
            }
            else
            {
                _previewTimer.Start();
            }
        }

        private void GeneratePreviewReal()
        {
            if (_currentStyle == null || listViewStyles.SelectedItems.Count != 1 || pictureBoxPreview.Width <= 0 || pictureBoxPreview.Height <= 0)
            {
                return;
            }

            if (GeneratePreviewViaMpv())
            {
                return;
            }

            if (_backgroundImage == null)
            {
                const int rectangleSize = 9;
                _backgroundImage = TextDesigner.MakeBackgroundImage(pictureBoxPreview.Width, pictureBoxPreview.Height, rectangleSize, _backgroundImageDark);
            }

            var defaultStyle = new SsaStyle();
            var shadowWidth = (float)numericUpDownShadowWidth.Value;
            var outlineColor = _currentStyle.BackgroundColor ?? defaultStyle.Background;

            Font font;
            var privateFontCollection = new PrivateFontCollection();
            try
            {
                var fontSize = defaultStyle.FontSize;
                if (_currentStyle.FontSize.HasValue && _currentStyle.FontSize.Value > 0.1m)
                {
                    fontSize = _currentStyle.FontSize.Value;
                }

                var fontName = _currentStyle.FontName;
                if (string.IsNullOrEmpty(fontName))
                {
                    fontName = Font.FontFamily.Name;
                }

                font = new Font(fontName, (float)fontSize * 1.1f, checkBoxFontBold.Checked ? FontStyle.Bold : FontStyle.Regular);
                if (_currentStyle.Italic.HasValue && _currentStyle.Italic.Value)
                {
                    font = new Font(fontName, (float)fontSize * 1.1f, font.Style | FontStyle.Italic);
                }

                if (_currentStyle.Underline.HasValue && _currentStyle.Underline.Value)
                {
                    font = new Font(fontName, (float)fontSize * 1.1f, font.Style | FontStyle.Underline);
                }

                if (_currentStyle.StrikeThrough.HasValue && _currentStyle.StrikeThrough.Value)
                {
                    font = new Font(fontName, (float)fontSize * 1.1f, font.Style | FontStyle.Strikeout);
                }
            }
            catch
            {
                font = new Font(Font, FontStyle.Regular);
            }

            var measureBmp = TextDesigner.MakeTextBitmapAssa(
                Configuration.Settings.General.PreviewAssaText,
                0,
                0,
                font,
                pictureBoxPreview.Width,
                pictureBoxPreview.Height,
                5, //outlineWidth,
                shadowWidth,
                null,
                _currentStyle.Color ?? defaultStyle.Primary,
                outlineColor,
                _currentStyle.ShadowColor ?? defaultStyle.Outline,
                !(_currentStyle.BackgroundColor.HasValue && _currentStyle.BackgroundColor.Value.A == 0));
            var nBmp = new NikseBitmap(measureBmp);
            var measuredWidth = nBmp.GetNonTransparentWidth();
            var measuredHeight = nBmp.GetNonTransparentHeight();

            var left = (pictureBoxPreview.Width - measuredWidth) / 2.0f;

            float top = pictureBoxPreview.Height - measuredHeight - 15;

            var designedText = TextDesigner.MakeTextBitmapAssa(
                Configuration.Settings.General.PreviewAssaText,
                (int)Math.Round(left),
                (int)Math.Round(top),
                font,
                pictureBoxPreview.Width,
                pictureBoxPreview.Height,
                5, //outlineWidth,
                shadowWidth,
                _backgroundImage,
                _currentStyle.Color ?? defaultStyle.Primary,
                outlineColor,
                _currentStyle.ShadowColor ?? defaultStyle.Outline,
                !(_currentStyle.BackgroundColor.HasValue && _currentStyle.BackgroundColor.Value.A == 0));

            pictureBoxPreview.Image?.Dispose();
            pictureBoxPreview.Image = designedText;
            font.Dispose();
            privateFontCollection.Dispose();
        }

        private bool GeneratePreviewViaMpv()
        {
            var fileName = VideoPreviewGenerator.GetVideoPreviewFileName();
            if (string.IsNullOrEmpty(fileName) || !LibMpvDynamic.IsInstalled)
            {
                return false;
            }

            if (_mpv == null)
            {
                _mpv = new LibMpvDynamic();
                _mpv.Initialize(pictureBoxPreview, fileName, VideoStartLoaded, null);
            }
            else
            {
                VideoStartLoaded(null, null);
            }

            return true;
        }

        private void VideoStartLoaded(object sender, EventArgs e)
        {
            if (_currentStyle == null)
            {
                return;
            }

            var subtitle = new Subtitle();
            var p = new Paragraph(Configuration.Settings.General.PreviewAssaText, 0, 10000);
            subtitle.Paragraphs.Add(p);
            subtitle.Header = WebVttHelper.AddStyleToHeader(null, _currentStyle);
            var assa = WebVttToAssa.Convert(subtitle, new SsaStyle(), _mpv.VideoWidth, _mpv.VideoHeight);
            assa.Paragraphs[0].Extra = _currentStyle.Name;
            var format = new AdvancedSubStationAlpha();
            var text = assa.ToText(format);
            _mpvTextFileName = FileUtil.GetTempFileName(format.Extension);
            File.WriteAllText(_mpvTextFileName, text);
            _mpv.LoadSubtitle(_mpvTextFileName);
            _mpv.Pause();
            _mpv.CurrentPosition = 0.5;
        }

        private void WebVttStyleManager_FormClosing(object sender, FormClosingEventArgs e)
        {
            _mpv?.Dispose();
        }

        private void WebVttStyleManager_Shown(object sender, EventArgs e)
        {
            listViewStyles.AutoSizeLastColumn();
        }

        private void WebVttStyleManager_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                DialogResult = DialogResult.Cancel;
            }
        }
    }
}
