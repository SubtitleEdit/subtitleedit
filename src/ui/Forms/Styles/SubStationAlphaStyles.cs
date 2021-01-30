using Nikse.SubtitleEdit.Core;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Logic;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.Forms.Styles
{
    public sealed partial class SubStationAlphaStyles : StylesForm
    {
        public class NameEdit
        {
            public NameEdit(string oldName, string newName)
            {
                OldName = oldName;
                NewName = newName;
            }
            public string OldName { get; set; }
            public string NewName { get; set; }
        }

        public List<NameEdit> RenameActions { get; set; }
        private string _startName;
        private string _editedName;
        private string _header;
        private bool _doUpdate;
        private string _oldSsaName;
        private readonly SubtitleFormat _format;
        private readonly bool _isSubStationAlpha;
        private Bitmap _backgroundImage;
        private Bitmap _fixedBackgroundImage;
        private bool _backgroundImageDark;
        private bool _fileStyleActive = true;
        private List<SsaStyle> _storageStyles;
        private FormWindowState _lastFormWindowState = FormWindowState.Normal;

        private ListView ActiveListView
        {
            get
            {
                return _fileStyleActive ? listViewStyles : listViewStorage;
            }
        }

        public SubStationAlphaStyles(Subtitle subtitle, SubtitleFormat format)
            : base(subtitle)
        {
            UiUtil.PreInitialize(this);
            InitializeComponent();
            UiUtil.FixFonts(this);

            RenameActions = new List<NameEdit>();
            labelStatus.Text = string.Empty;
            _header = subtitle.Header;
            _format = format;
            _isSubStationAlpha = _format.Name == SubStationAlpha.NameOfFormat;
            _backgroundImageDark = Configuration.Settings.General.UseDarkTheme;

            if (_header != null && _header.Contains("http://www.w3.org/ns/ttml"))
            {
                var s = new Subtitle { Header = _header };
                AdvancedSubStationAlpha.LoadStylesFromTimedText10(s, string.Empty, _header, AdvancedSubStationAlpha.HeaderNoStyles, new StringBuilder());
                _header = s.Header;
            }

            if (_header == null || !_header.Contains("style:", StringComparison.OrdinalIgnoreCase))
            {
                ResetHeader();
            }

            comboBoxFontName.Items.Clear();
            foreach (var x in FontFamily.Families)
            {
                comboBoxFontName.Items.Add(x.Name);
            }

            var l = LanguageSettings.Current.SubStationAlphaStyles;
            Text = l.Title;
            groupBoxStyles.Text = l.Styles;
            listViewStyles.Columns[0].Text = l.Name;
            listViewStyles.Columns[1].Text = l.FontName;
            listViewStyles.Columns[2].Text = l.FontSize;
            listViewStyles.Columns[3].Text = l.UseCount;
            listViewStyles.Columns[4].Text = l.Primary;
            listViewStyles.Columns[5].Text = l.Outline;
            listViewStorage.Columns[0].Text = l.Name;
            listViewStorage.Columns[5].Text = l.Outline;
            listViewStorage.Columns[1].Text = l.FontName;
            listViewStorage.Columns[2].Text = l.FontSize;
            listViewStorage.Columns[3].Text = l.UseCount;
            listViewStorage.Columns[4].Text = l.Primary;
            groupBoxProperties.Text = l.Properties;
            labelStyleName.Text = l.Name;
            groupBoxFont.Text = l.Font;
            labelFontName.Text = l.FontName;
            labelFontSize.Text = l.FontSize;
            checkBoxFontItalic.Text = LanguageSettings.Current.General.Italic;
            checkBoxFontBold.Text = LanguageSettings.Current.General.Bold;
            checkBoxFontUnderline.Text = LanguageSettings.Current.General.Underline;
            groupBoxAlignment.Text = l.Alignment;
            groupBoxColors.Text = l.Colors;
            buttonPrimaryColor.Text = l.Primary;
            buttonSecondaryColor.Text = l.Secondary;
            buttonOutlineColor.Text = l.Outline;
            buttonBackColor.Text = l.Shadow;
            groupBoxMargins.Text = l.Margins;
            labelMarginLeft.Text = LanguageSettings.Current.ExportPngXml.Left;
            labelMarginRight.Text = LanguageSettings.Current.ExportPngXml.Right;
            labelMarginVertical.Text = l.Vertical;
            groupBoxBorder.Text = l.Border;
            radioButtonOutline.Text = l.Outline;
            labelShadow.Text = l.Shadow;
            radioButtonOpaqueBox.Text = l.OpaqueBox;
            buttonImport.Text = l.Import;
            buttonExport.Text = l.Export;
            buttonCopy.Text = l.Copy;
            buttonAdd.Text = l.New;
            buttonRemove.Text = l.Remove;
            buttonRemoveAll.Text = l.RemoveAll;
            groupBoxPreview.Text = LanguageSettings.Current.General.Preview;

            groupBoxStorage.Text = l.StyleStorage;
            buttonStorageImport.Text = l.Import;
            buttonStorageExport.Text = l.Export;
            buttonStorageAdd.Text = l.New;
            buttonStorageCopy.Text = l.Copy;
            buttonStorageRemove.Text = l.Remove;
            buttonStorageRemoveAll.Text = l.RemoveAll;

            buttonAddToFile.Text = l.AddToFile;
            buttonAddStyleToStorage.Text = l.AddToStorage;

            deleteToolStripMenuItem.Text = LanguageSettings.Current.MultipleReplace.Remove;
            toolStripMenuItemRemoveAll.Text = LanguageSettings.Current.MultipleReplace.RemoveAll;
            addToStorageToolStripMenuItem1.Text = l.AddToStorage;
            moveUpToolStripMenuItem.Text = LanguageSettings.Current.DvdSubRip.MoveUp;
            moveDownToolStripMenuItem.Text = LanguageSettings.Current.DvdSubRip.MoveDown;
            moveTopToolStripMenuItem.Text = LanguageSettings.Current.MultipleReplace.MoveToTop;
            moveBottomToolStripMenuItem.Text = LanguageSettings.Current.MultipleReplace.MoveToBottom;
            newToolStripMenuItemNew.Text = LanguageSettings.Current.SubStationAlphaStyles.New;
            copyToolStripMenuItemCopy.Text = LanguageSettings.Current.SubStationAlphaStyles.Copy;
            toolStripMenuItemImport.Text = LanguageSettings.Current.MultipleReplace.Import;
            toolStripMenuItemExport.Text = LanguageSettings.Current.MultipleReplace.Export;

            toolStripMenuItemStorageRemove.Text = LanguageSettings.Current.MultipleReplace.Remove;
            toolStripMenuItemStorageRemoveAll.Text = LanguageSettings.Current.MultipleReplace.RemoveAll;
            addToFileStylesToolStripMenuItem.Text = l.AddToFile;
            toolStripMenuItemStorageMoveUp.Text = LanguageSettings.Current.DvdSubRip.MoveUp;
            toolStripMenuItemStorageMoveDown.Text = LanguageSettings.Current.DvdSubRip.MoveDown;
            toolStripMenuItemStorageMoveTop.Text = LanguageSettings.Current.MultipleReplace.MoveToTop;
            toolStripMenuItemStorageMoveBottom.Text = LanguageSettings.Current.MultipleReplace.MoveToBottom;
            toolStripMenuItemStorageNew.Text = LanguageSettings.Current.SubStationAlphaStyles.New;
            toolStripMenuItemStorageCopy.Text = LanguageSettings.Current.SubStationAlphaStyles.Copy;
            toolStripMenuItemStorageImport.Text = LanguageSettings.Current.MultipleReplace.Import;
            toolStripMenuItemStorageExport.Text = LanguageSettings.Current.MultipleReplace.Export;

            setPreviewTextToolStripMenuItem.Text = l.SetPreviewText;

            using (var graphics = CreateGraphics())
            {
                var w = (int)graphics.MeasureString(buttonPrimaryColor.Text, Font).Width;
                buttonPrimaryColor.Width = w + 15;
                panelPrimaryColor.Left = buttonPrimaryColor.Left + buttonPrimaryColor.Width + 4;

                buttonSecondaryColor.Left = panelPrimaryColor.Left + panelPrimaryColor.Width + 12;
                w = (int)graphics.MeasureString(buttonSecondaryColor.Text, Font).Width;
                buttonSecondaryColor.Width = w + 15;
                panelSecondaryColor.Left = buttonSecondaryColor.Left + buttonSecondaryColor.Width + 4;

                buttonOutlineColor.Left = panelSecondaryColor.Left + panelSecondaryColor.Width + 12;
                w = (int)graphics.MeasureString(buttonOutlineColor.Text, Font).Width;
                buttonOutlineColor.Width = w + 15;
                panelOutlineColor.Left = buttonOutlineColor.Left + buttonOutlineColor.Width + 4;

                buttonBackColor.Left = panelOutlineColor.Left + panelOutlineColor.Width + 12;
                w = (int)graphics.MeasureString(buttonBackColor.Text, Font).Width;
                buttonBackColor.Width = w + 15;
                panelBackColor.Left = buttonBackColor.Left + buttonBackColor.Width + 4;
            }

            if (_isSubStationAlpha)
            {
                Text = l.TitleSubstationAlpha;
                buttonOutlineColor.Text = l.Tertiary;
                buttonBackColor.Text = l.Back;
                listViewStyles.Columns[5].Text = l.Back;
                checkBoxFontUnderline.Visible = false;
                numericUpDownOutline.Increment = 1;
                numericUpDownOutline.DecimalPlaces = 0;
                numericUpDownShadowWidth.Increment = 1;
                numericUpDownShadowWidth.DecimalPlaces = 0;

                splitContainer1.Panel2Collapsed = true;
                groupBoxStorage.Visible = false;
                buttonAddStyleToStorage.Visible = false;
            }
            else
            {
                _storageStyles = new List<SsaStyle>();
                var styles = AdvancedSubStationAlpha.GetStylesFromHeader(Configuration.Settings.SubtitleSettings.AssaStyleStorage);
                foreach (var styleName in styles)
                {
                    if (!string.IsNullOrEmpty(styleName))
                    {
                        var storageStyle = AdvancedSubStationAlpha.GetSsaStyle(styleName, Configuration.Settings.SubtitleSettings.AssaStyleStorage);
                        _storageStyles.Add(storageStyle);
                        AddStyle(listViewStorage, storageStyle, Subtitle, _isSubStationAlpha);
                    }
                }
            }

            buttonOK.Text = LanguageSettings.Current.General.Ok;
            buttonCancel.Text = LanguageSettings.Current.General.Cancel;

            InitializeListView();
            UiUtil.FixLargeFonts(this, buttonCancel);

            comboBoxFontName.Left = labelFontName.Left + labelFontName.Width + 5;
            labelFontSize.Left = comboBoxFontName.Left + comboBoxFontName.Width + 20;
            numericUpDownFontSize.Left = labelFontSize.Left + labelFontSize.Width + 5;

            numericUpDownOutline.Left = radioButtonOutline.Left + radioButtonOutline.Width + 5;
            labelShadow.Left = numericUpDownOutline.Left + numericUpDownOutline.Width + 5;
            numericUpDownShadowWidth.Left = numericUpDownOutline.Left + numericUpDownOutline.Width + 5;
            listViewStyles.Columns[5].Width = -2;
            checkBoxFontItalic.Left = checkBoxFontBold.Left + checkBoxFontBold.Width + 12;
            checkBoxFontUnderline.Left = checkBoxFontItalic.Left + checkBoxFontItalic.Width + 12;
        }

        public override string Header => _header;

        protected override void GeneratePreviewReal()
        {
            if (listViewStyles.SelectedItems.Count != 1 || pictureBoxPreview.Width <= 0 || pictureBoxPreview.Height <= 0)
            {
                return;
            }

            if (_fixedBackgroundImage != null)
            {
                _backgroundImage = (Bitmap)_fixedBackgroundImage.Clone();
            }

            if (_backgroundImage == null)
            {
                const int rectangleSize = 9;
                _backgroundImage = TextDesigner.MakeBackgroundImage(pictureBoxPreview.Width, pictureBoxPreview.Height, rectangleSize, _backgroundImageDark);
            }

            var outlineWidth = (float)numericUpDownOutline.Value;
            var shadowWidth = (float)numericUpDownShadowWidth.Value;
            var outlineColor = _isSubStationAlpha ? panelBackColor.BackColor : panelOutlineColor.BackColor;

            Font font;
            try
            {
                font = new Font(comboBoxFontName.Text, (float)numericUpDownFontSize.Value * 1.1f, checkBoxFontBold.Checked ? FontStyle.Bold : FontStyle.Regular);
                if (checkBoxFontItalic.Checked)
                {
                    font = new Font(comboBoxFontName.Text, (float)numericUpDownFontSize.Value * 1.1f, font.Style | FontStyle.Italic);
                }
                if (checkBoxFontUnderline.Checked)
                {
                    font = new Font(comboBoxFontName.Text, (float)numericUpDownFontSize.Value * 1.1f, font.Style | FontStyle.Underline);
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
                outlineWidth,
                shadowWidth,
                null,
                panelPrimaryColor.BackColor,
                outlineColor,
                panelBackColor.BackColor,
                radioButtonOpaqueBox.Checked);
            var nBmp = new NikseBitmap(measureBmp);
            var measuredWidth = nBmp.GetNonTransparentWidth();
            var measuredHeight = nBmp.GetNonTransparentHeight();

            float left;
            if (radioButtonTopLeft.Checked || radioButtonMiddleLeft.Checked || radioButtonBottomLeft.Checked)
            {
                left = (float)numericUpDownMarginLeft.Value;
            }
            else if (radioButtonTopRight.Checked || radioButtonMiddleRight.Checked || radioButtonBottomRight.Checked)
            {
                left = pictureBoxPreview.Width - (measuredWidth + (float)numericUpDownMarginRight.Value);
            }
            else
            {
                left = (pictureBoxPreview.Width - measuredWidth) / 2.0f;
            }

            float top;
            if (radioButtonTopLeft.Checked || radioButtonTopCenter.Checked || radioButtonTopRight.Checked)
            {
                top = (float)numericUpDownMarginVertical.Value;
            }
            else if (radioButtonMiddleLeft.Checked || radioButtonMiddleCenter.Checked || radioButtonMiddleRight.Checked)
            {
                top = (pictureBoxPreview.Height - measuredHeight) / 2.0f;
            }
            else
            {
                top = pictureBoxPreview.Height - measuredHeight - (int)numericUpDownMarginVertical.Value;
            }

            var designedText = TextDesigner.MakeTextBitmapAssa(
                Configuration.Settings.General.PreviewAssaText,
                (int)Math.Round(left),
                (int)Math.Round(top),
                font,
                pictureBoxPreview.Width,
                pictureBoxPreview.Height,
                outlineWidth,
                shadowWidth,
                _backgroundImage,
                panelPrimaryColor.BackColor,
                panelOutlineColor.BackColor,
                panelBackColor.BackColor,
                radioButtonOpaqueBox.Checked);

            pictureBoxPreview.Image?.Dispose();
            pictureBoxPreview.Image = designedText;
            font.Dispose();
        }

        private void InitializeListView()
        {
            var styles = AdvancedSubStationAlpha.GetStylesFromHeader(_header);
            listViewStyles.Items.Clear();
            foreach (var style in styles)
            {
                var ssaStyle = GetSsaStyle(style);
                if (ssaStyle != null)
                {
                    AddStyle(listViewStyles, ssaStyle, Subtitle, _isSubStationAlpha);
                }
            }

            if (listViewStyles.Items.Count > 0)
            {
                listViewStyles.Items[0].Selected = true;
            }
        }

        public static void AddStyle(ListView lv, SsaStyle ssaStyle, Subtitle subtitle, bool isSubstationAlpha)
        {
            AddStyle(lv, ssaStyle, subtitle, isSubstationAlpha, lv.Items.Count); ;
        }

        public static void AddStyle(ListView lv, SsaStyle ssaStyle, Subtitle subtitle, bool isSubstationAlpha, int insertAt)
        {
            var item = new ListViewItem(ssaStyle.Name.Trim())
            {
                Checked = true,
                UseItemStyleForSubItems = false
            };

            var subItem = new ListViewItem.ListViewSubItem(item, ssaStyle.FontName);
            item.SubItems.Add(subItem);

            subItem = new ListViewItem.ListViewSubItem(item, ssaStyle.FontSize.ToString(CultureInfo.InvariantCulture));
            item.SubItems.Add(subItem);

            int count = 0;
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                if (string.IsNullOrEmpty(p.Extra) && ssaStyle.Name.TrimStart('*') == "Default" ||
                    p.Extra != null && ssaStyle.Name.TrimStart('*').Equals(p.Extra.TrimStart('*'), StringComparison.OrdinalIgnoreCase))
                {
                    count++;
                }
            }
            subItem = new ListViewItem.ListViewSubItem(item, count.ToString());
            item.SubItems.Add(subItem);

            subItem = new ListViewItem.ListViewSubItem(item, string.Empty) { BackColor = ssaStyle.Primary };
            item.SubItems.Add(subItem);

            subItem = new ListViewItem.ListViewSubItem(item, string.Empty)
            {
                BackColor = isSubstationAlpha ? ssaStyle.Background : ssaStyle.Outline,
                Text = LanguageSettings.Current.General.Text,
                ForeColor = ssaStyle.Primary
            };
            try
            {
                var fontStyle = FontStyle.Regular;
                if (ssaStyle.Bold)
                {
                    fontStyle |= FontStyle.Bold;
                }
                if (ssaStyle.Italic)
                {
                    fontStyle |= FontStyle.Italic;
                }
                if (ssaStyle.Underline)
                {
                    fontStyle |= FontStyle.Underline;
                }
                subItem.Font = new Font(ssaStyle.FontName, subItem.Font.Size, fontStyle);
            }
            catch
            {
                // ignored
            }

            item.SubItems.Add(subItem);

            lv.Items.Insert(insertAt, item);
        }

        private static Color GetColorFromSsa(string color)
        {
            var c = color.RemoveChar('&').TrimStart('H');
            c = c.PadLeft(6, '0');
            string alpha = "ff";
            if (int.TryParse(c.Substring(0, 2), NumberStyles.HexNumber, null, out var a))
            {
                alpha = $"{255 - a:X2}";
            }
            c = $"#{alpha}{c.Substring(c.Length - 2, 2)}{c.Substring(c.Length - 4, 2)}{c.Substring(c.Length - 6, 2)}";
            c = c.ToLowerInvariant();
            try
            {
                return ColorTranslator.FromHtml(c);
            }
            catch
            {
                return Color.Yellow;
            }
        }

        private bool SetSsaStyle(string styleName, string propertyName, string propertyValue, bool trimStyles = true)
        {
            if (!_fileStyleActive)
            {
                var style = _storageStyles.FirstOrDefault(p => p.Name == styleName);
                if (style == null)
                {
                    return false;
                }

                if (propertyName == "name")
                {
                    style.Name = propertyValue;
                    return true;
                }
                else if (propertyName == "fontname")
                {
                    style.FontName = propertyValue;
                    return true;
                }
                else if (propertyName == "fontsize")
                {
                    style.FontSize = float.Parse(propertyValue, CultureInfo.InvariantCulture);
                    return true;
                }
                else if (propertyName == "bold")
                {
                    style.Bold = propertyValue != "0";
                    return true;
                }
                else if (propertyName == "italic")
                {
                    style.Italic = propertyValue != "0";
                    return true;
                }
                else if (propertyName == "underline")
                {
                    style.Underline = propertyValue != "0";
                    return true;
                }
                else if (propertyName == "alignment")
                {
                    style.Alignment = propertyValue;
                    return true;
                }
                else if (propertyName == "marginl")
                {
                    style.MarginLeft = int.Parse(propertyValue, CultureInfo.InvariantCulture);
                    return true;
                }
                else if (propertyName == "marginr")
                {
                    style.MarginRight = int.Parse(propertyValue, CultureInfo.InvariantCulture);
                    return true;
                }
                else if (propertyName == "marginv")
                {
                    style.MarginVertical = int.Parse(propertyValue, CultureInfo.InvariantCulture);
                    return true;
                }
                else if (propertyName == "outline")
                {
                    style.OutlineWidth = decimal.Parse(propertyValue, CultureInfo.InvariantCulture);
                    return true;
                }
                else if (propertyName == "shadow")
                {
                    style.ShadowWidth = decimal.Parse(propertyValue, CultureInfo.InvariantCulture);
                    return true;
                }
                else if (propertyName == "borderstyle")
                {
                    style.BorderStyle = propertyValue;
                    return true;
                }
                else if (propertyName == "primarycolour")
                {
                    style.Primary = GetColorFromSsa(propertyValue);
                    return true;
                }
                else if (propertyName == "secondarycolour")
                {
                    style.Secondary = GetColorFromSsa(propertyValue);
                    return true;
                }
                else if (propertyName == "outlinecolour")
                {
                    style.Outline = GetColorFromSsa(propertyValue);
                    return true;
                }
                else if (propertyName == "tertiarycolour")
                {
                    style.Tertiary = GetColorFromSsa(propertyValue);
                    return true;
                }
                else if (propertyName == "backcolour")
                {
                    style.Background = GetColorFromSsa(propertyValue);
                    return true;
                }


                return false;
            }


            bool found = false;
            int propertyIndex = -1;
            int nameIndex = -1;
            var sb = new StringBuilder();
            foreach (var line in _header.Split(Utilities.NewLineChars, StringSplitOptions.None))
            {
                string s = line.Trim().ToLowerInvariant();
                if (s.StartsWith("format:", StringComparison.Ordinal))
                {
                    if (line.Length > 10)
                    {
                        var format = line.ToLowerInvariant().Substring(8).Split(',');
                        for (int i = 0; i < format.Length; i++)
                        {
                            string f = format[i].Trim().ToLowerInvariant();
                            if (f == "name")
                            {
                                nameIndex = i;
                            }
                            if (f == propertyName)
                            {
                                propertyIndex = i;
                            }
                        }
                    }
                    sb.AppendLine(line);
                }
                else if (s.RemoveChar(' ').StartsWith("style:", StringComparison.Ordinal))
                {
                    if (line.Length > 10)
                    {
                        bool correctLine = false;
                        var format = line.Substring(6).Trim().Split(',');
                        for (int i = 0; i < format.Length; i++)
                        {
                            string f = format[i];
                            if (trimStyles)
                            {
                                f = f.Trim();
                            }
                            if (i == nameIndex)
                            {
                                correctLine = f.Equals(styleName, StringComparison.OrdinalIgnoreCase);
                            }
                        }
                        if (correctLine)
                        {
                            sb.Append(line.Substring(0, 6) + " ");
                            format = line.Substring(6).Split(',');
                            for (int i = 0; i < format.Length; i++)
                            {
                                string f = format[i].Trim();
                                if (i == propertyIndex)
                                {
                                    sb.Append(propertyValue);
                                    found = true;
                                }
                                else
                                {
                                    sb.Append(f);
                                }
                                if (i < format.Length - 1)
                                {
                                    sb.Append(',');
                                }
                            }
                            sb.AppendLine();
                        }
                        else
                        {
                            sb.AppendLine(line);
                        }
                    }
                    else
                    {
                        sb.AppendLine(line);
                    }
                }
                else
                {
                    sb.AppendLine(line);
                }
            }
            _header = sb.ToString().Replace(Environment.NewLine + Environment.NewLine, Environment.NewLine);
            return found;
        }

        private SsaStyle GetSsaStyle(string styleName)
        {
            if (_fileStyleActive)
            {
                return GetSsaStyleFile(styleName);
            }

            return GetSsaStyleStorage(styleName);
        }

        private SsaStyle GetSsaStyleFile(string styleName)
        {
            return AdvancedSubStationAlpha.GetSsaStyle(styleName, _header);
        }

        private SsaStyle GetSsaStyleStorage(string styleName)
        {
            return _storageStyles.FirstOrDefault(p => p.Name == styleName);
        }

        private void ResetHeader()
        {
            SubtitleFormat format;
            if (_isSubStationAlpha)
            {
                format = new SubStationAlpha();
            }
            else
            {
                format = new AdvancedSubStationAlpha();
            }
            var sub = new Subtitle();
            string text = format.ToText(sub, string.Empty);
            var lines = text.SplitToLines();
            format.LoadSubtitle(sub, lines, string.Empty);
            _header = sub.Header;
        }

        private void SubStationAlphaStyles_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                DialogResult = DialogResult.Cancel;
            }
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void buttonNextFinish_Click(object sender, EventArgs e)
        {
            if (!_isSubStationAlpha)
            {
                Configuration.Settings.SubtitleSettings.AssaStyleStorage = GetStorageHeader();
            }

            LogNameChanges();
            DialogResult = DialogResult.OK;
        }

        private string GetStorageHeader()
        {
            var header = AdvancedSubStationAlpha.DefaultHeader;
            var end = header.IndexOf(Environment.NewLine + "Style:");
            header = header.Substring(0, end).Trim();
            foreach (var style in _storageStyles)
            {
                header = AdvancedSubStationAlpha.AddSsaStyle(style, header);
            }

            return header;
        }

        private void listViewStyles_SelectedIndexChanged(object sender, EventArgs e)
        {
            LogNameChanges();

            if (listViewStyles.SelectedItems.Count == 1)
            {
                string styleName = listViewStyles.SelectedItems[0].Text;
                _startName = styleName;
                _editedName = null;
                _oldSsaName = styleName;
                SsaStyle style = GetSsaStyle(styleName);
                SetControlsFromStyle(style);
                _doUpdate = true;
                groupBoxProperties.Enabled = true;
                GeneratePreview();
                buttonRemove.Enabled = listViewStyles.Items.Count > 1;
            }
            else
            {
                groupBoxProperties.Enabled = false;
                _doUpdate = false;
            }
        }

        private void LogNameChanges()
        {
            if (_startName != null && _editedName != null && _startName != _editedName)
            {
                RenameActions.Add(new NameEdit(_startName, _editedName));
                _startName = null;
                _editedName = null;
            }
        }

        private void SetControlsFromStyle(SsaStyle style)
        {
            textBoxStyleName.Text = style.Name;
            textBoxStyleName.BackColor = listViewStyles.BackColor;
            comboBoxFontName.Text = style.FontName;
            checkBoxFontItalic.Checked = style.Italic;
            checkBoxFontBold.Checked = style.Bold;
            checkBoxFontUnderline.Checked = style.Underline;

            if (style.FontSize > 0 && style.FontSize <= (float)numericUpDownFontSize.Maximum)
            {
                numericUpDownFontSize.Value = (decimal)style.FontSize;
            }
            else
            {
                numericUpDownFontSize.Value = 20;
            }

            panelPrimaryColor.BackColor = style.Primary;
            panelSecondaryColor.BackColor = style.Secondary;
            panelOutlineColor.BackColor = _isSubStationAlpha ? style.Tertiary : style.Outline;
            panelBackColor.BackColor = style.Background;

            if (style.OutlineWidth >= 0 && style.OutlineWidth <= numericUpDownOutline.Maximum)
            {
                numericUpDownOutline.Value = style.OutlineWidth;
            }
            else
            {
                numericUpDownOutline.Value = 2;
            }

            if (style.ShadowWidth >= 0 && style.ShadowWidth <= numericUpDownShadowWidth.Maximum)
            {
                numericUpDownShadowWidth.Value = style.ShadowWidth;
            }
            else
            {
                numericUpDownShadowWidth.Value = 1;
            }

            if (_isSubStationAlpha)
            {
                switch (style.Alignment)
                {
                    case "1":
                        radioButtonBottomLeft.Checked = true;
                        break;
                    case "3":
                        radioButtonBottomRight.Checked = true;
                        break;
                    case "9":
                        radioButtonMiddleLeft.Checked = true;
                        break;
                    case "10":
                        radioButtonMiddleCenter.Checked = true;
                        break;
                    case "11":
                        radioButtonMiddleRight.Checked = true;
                        break;
                    case "5":
                        radioButtonTopLeft.Checked = true;
                        break;
                    case "6":
                        radioButtonTopCenter.Checked = true;
                        break;
                    case "7":
                        radioButtonTopRight.Checked = true;
                        break;
                    default:
                        radioButtonBottomCenter.Checked = true;
                        break;
                }
            }
            else
            {
                switch (style.Alignment)
                {
                    case "1":
                        radioButtonBottomLeft.Checked = true;
                        break;
                    case "3":
                        radioButtonBottomRight.Checked = true;
                        break;
                    case "4":
                        radioButtonMiddleLeft.Checked = true;
                        break;
                    case "5":
                        radioButtonMiddleCenter.Checked = true;
                        break;
                    case "6":
                        radioButtonMiddleRight.Checked = true;
                        break;
                    case "7":
                        radioButtonTopLeft.Checked = true;
                        break;
                    case "8":
                        radioButtonTopCenter.Checked = true;
                        break;
                    case "9":
                        radioButtonTopRight.Checked = true;
                        break;
                    default:
                        radioButtonBottomCenter.Checked = true;
                        break;
                }
            }

            if (style.MarginLeft >= 0 && style.MarginLeft <= numericUpDownMarginLeft.Maximum)
            {
                numericUpDownMarginLeft.Value = style.MarginLeft;
            }
            else
            {
                numericUpDownMarginLeft.Value = 10;
            }

            if (style.MarginRight >= 0 && style.MarginRight <= numericUpDownMarginRight.Maximum)
            {
                numericUpDownMarginRight.Value = style.MarginRight;
            }
            else
            {
                numericUpDownMarginRight.Value = 10;
            }

            if (style.MarginVertical >= 0 && style.MarginVertical <= numericUpDownMarginVertical.Maximum)
            {
                numericUpDownMarginVertical.Value = style.MarginVertical;
            }
            else
            {
                numericUpDownMarginVertical.Value = 10;
            }

            if (style.BorderStyle == "3")
            {
                radioButtonOpaqueBox.Checked = true;
            }
            else
            {
                radioButtonOutline.Checked = true;
            }
        }

        private void buttonPrimaryColor_Click(object sender, EventArgs e)
        {
            string name = ActiveListView.SelectedItems[0].Text;
            if (_isSubStationAlpha)
            {
                colorDialogSSAStyle.Color = panelPrimaryColor.BackColor;
                if (colorDialogSSAStyle.ShowDialog() == DialogResult.OK)
                {
                    ActiveListView.SelectedItems[0].SubItems[4].BackColor = colorDialogSSAStyle.Color;
                    ActiveListView.SelectedItems[0].SubItems[5].ForeColor = colorDialogSSAStyle.Color;
                    panelPrimaryColor.BackColor = colorDialogSSAStyle.Color;
                    SetSsaStyle(name, "primarycolour", GetSsaColorString(colorDialogSSAStyle.Color));
                    GeneratePreview();
                }
            }
            else
            {
                using (var colorChooser = new ColorChooser { Color = panelPrimaryColor.BackColor })
                {
                    if (colorChooser.ShowDialog() == DialogResult.OK)
                    {
                        panelPrimaryColor.BackColor = colorChooser.Color;
                        ActiveListView.SelectedItems[0].SubItems[4].BackColor = panelPrimaryColor.BackColor;
                        ActiveListView.SelectedItems[0].SubItems[5].ForeColor = panelPrimaryColor.BackColor;
                        ActiveListView.SelectedItems[0].SubItems[4].BackColor = panelPrimaryColor.BackColor;
                        SetSsaStyle(name, "primarycolour", GetSsaColorString(panelPrimaryColor.BackColor));
                        GeneratePreview();
                    }
                }
            }
        }

        private void buttonSecondaryColor_Click(object sender, EventArgs e)
        {
            string name = ActiveListView.SelectedItems[0].Text;
            if (_isSubStationAlpha)
            {
                colorDialogSSAStyle.Color = panelSecondaryColor.BackColor;
                if (colorDialogSSAStyle.ShowDialog() == DialogResult.OK)
                {
                    panelSecondaryColor.BackColor = colorDialogSSAStyle.Color;
                    SetSsaStyle(name, "secondarycolour", GetSsaColorString(colorDialogSSAStyle.Color));
                    GeneratePreview();
                }
            }
            else
            {
                using (var colorChooser = new ColorChooser { Color = panelSecondaryColor.BackColor })
                {
                    if (colorChooser.ShowDialog() == DialogResult.OK)
                    {
                        panelSecondaryColor.BackColor = colorChooser.Color;
                        SetSsaStyle(name, "secondarycolour", GetSsaColorString(panelSecondaryColor.BackColor));
                        GeneratePreview();
                    }
                }
            }
        }

        private void buttonOutlineColor_Click(object sender, EventArgs e)
        {
            string name = ActiveListView.SelectedItems[0].Text;
            if (_isSubStationAlpha)
            {
                colorDialogSSAStyle.Color = panelOutlineColor.BackColor;
                if (colorDialogSSAStyle.ShowDialog() == DialogResult.OK)
                {
                    panelOutlineColor.BackColor = colorDialogSSAStyle.Color;
                    SetSsaStyle(name, "tertiarycolour", GetSsaColorString(colorDialogSSAStyle.Color));
                    GeneratePreview();
                }
            }
            else
            {
                using (var colorChooser = new ColorChooser { Color = panelOutlineColor.BackColor })
                {
                    if (colorChooser.ShowDialog() == DialogResult.OK)
                    {
                        panelOutlineColor.BackColor = colorChooser.Color;
                        ActiveListView.SelectedItems[0].SubItems[5].BackColor = panelOutlineColor.BackColor;
                        SetSsaStyle(name, "outlinecolour", GetSsaColorString(panelOutlineColor.BackColor));
                        GeneratePreview();
                    }
                }
            }
        }

        private void buttonShadowColor_Click(object sender, EventArgs e)
        {
            string name = ActiveListView.SelectedItems[0].Text;
            if (_isSubStationAlpha)
            {
                colorDialogSSAStyle.Color = panelBackColor.BackColor;
                if (colorDialogSSAStyle.ShowDialog() == DialogResult.OK)
                {
                    ActiveListView.SelectedItems[0].SubItems[4].BackColor = colorDialogSSAStyle.Color;
                    panelBackColor.BackColor = colorDialogSSAStyle.Color;
                    SetSsaStyle(name, "backcolour", GetSsaColorString(colorDialogSSAStyle.Color));
                    GeneratePreview();
                }
            }
            else
            {
                using (var colorChooser = new ColorChooser { Color = panelBackColor.BackColor })
                {
                    if (colorChooser.ShowDialog() == DialogResult.OK)
                    {
                        panelBackColor.BackColor = colorChooser.Color;
                        SetSsaStyle(name, "backcolour", GetSsaColorString(panelBackColor.BackColor));
                        GeneratePreview();
                    }
                }
            }
        }

        private string GetSsaColorString(Color c)
        {
            if (_isSubStationAlpha)
            {
                return Color.FromArgb(0, c.B, c.G, c.R).ToArgb().ToString();
            }
            return AdvancedSubStationAlpha.GetSsaColorString(c);
        }

        private void buttonCopy_Click(object sender, EventArgs e)
        {
            if (listViewStyles.SelectedItems.Count == 1)
            {
                string styleName = listViewStyles.SelectedItems[0].Text;
                SsaStyle oldStyle = GetSsaStyle(styleName);
                var style = new SsaStyle(oldStyle) { Name = string.Format(LanguageSettings.Current.SubStationAlphaStyles.CopyOfY, styleName) }; // Copy contructor

                if (GetSsaStyle(style.Name).LoadedFromHeader)
                {
                    int count = 2;
                    bool doRepeat = true;
                    while (doRepeat)
                    {
                        style.Name = string.Format(LanguageSettings.Current.SubStationAlphaStyles.CopyXOfY, count, styleName);
                        doRepeat = GetSsaStyle(style.Name).LoadedFromHeader;
                        count++;
                    }
                }

                _doUpdate = false;
                AddStyle(listViewStyles, style, Subtitle, _isSubStationAlpha);
                listViewStyles.Items[listViewStyles.Items.Count - 1].Selected = true;
                listViewStyles.Items[listViewStyles.Items.Count - 1].EnsureVisible();
                listViewStyles.Items[listViewStyles.Items.Count - 1].Focused = true;
                textBoxStyleName.Text = style.Name;
                textBoxStyleName.Focus();
                AddStyleToHeader(style);
                _doUpdate = true;
                listViewStyles_SelectedIndexChanged(null, null);
            }
        }

        private void AddStyleToHeader(SsaStyle style)
        {
            var sb = new StringBuilder();
            var format = "Format: Name, Fontname, Fontsize, PrimaryColour, SecondaryColour, OutlineColour, BackColour, Bold, Italic, Underline, StrikeOut, ScaleX, ScaleY, Spacing, Angle, BorderStyle, Outline, Shadow, Alignment, MarginL, MarginR, MarginV, Encoding";
            foreach (var line in _header.Split(new[] { Environment.NewLine }, StringSplitOptions.None))
            {
                if (line.Trim().StartsWith("Format:", StringComparison.OrdinalIgnoreCase))
                {
                    format = line.Trim();
                }
                else if (line.Trim() == "[Events]")
                {
                    break;
                }

                sb.AppendLine(line);
            }

            sb = new StringBuilder(sb.ToString().TrimEnd());
            sb.AppendLine();

            if (_isSubStationAlpha)
            {
                sb.AppendLine(style.ToRawSsa(format));
            }
            else
            {
                sb.AppendLine(style.ToRawAss(format));
            }

            sb.AppendLine();
            sb.AppendLine("[Events]");
            _header = sb.ToString();
        }


        private void RemoveStyleFromHeader(string name)
        {
            var sb = new StringBuilder();
            foreach (var line in _header.Split(new[] { Environment.NewLine }, StringSplitOptions.None))
            {
                var lineIsStyle = line.ToLowerInvariant().RemoveChar(' ').StartsWith("style:" + name.ToLowerInvariant().RemoveChar(' ') + ",", StringComparison.Ordinal) &&
                                  line.ToLowerInvariant().Contains(name.ToLowerInvariant());
                if (!lineIsStyle)
                {
                    sb.AppendLine(line);
                }
            }
            _header = sb.ToString();
        }

        private void ReplaceStyleInHeader(SsaStyle style)
        {
            var name = style.Name;
            var sb = new StringBuilder();
            var format = "Format: Name, Fontname, Fontsize, PrimaryColour, SecondaryColour, OutlineColour, BackColour, Bold, Italic, Underline, StrikeOut, ScaleX, ScaleY, Spacing, Angle, BorderStyle, Outline, Shadow, Alignment, MarginL, MarginR, MarginV, Encoding";
            foreach (var line in _header.Split(new[] { Environment.NewLine }, StringSplitOptions.None))
            {
                if (line.Trim().StartsWith("Format:", StringComparison.OrdinalIgnoreCase))
                {
                    format = line.Trim();
                }

                var lineIsStyle = line.ToLowerInvariant().RemoveChar(' ').StartsWith("style:" + name.ToLowerInvariant().RemoveChar(' ') + ",", StringComparison.Ordinal) &&
                                  line.ToLowerInvariant().Contains(name.ToLowerInvariant());
                if (!lineIsStyle)
                {
                    sb.AppendLine(line);
                }
                else
                {
                    if (_isSubStationAlpha)
                    {
                        sb.AppendLine(style.ToRawAss(format));
                    }
                    else
                    {
                        sb.AppendLine(style.ToRawSsa(format));
                    }
                }

            }
            _header = sb.ToString();
        }

        private void buttonAdd_Click(object sender, EventArgs e)
        {
            if (listViewStyles.SelectedItems.Count > 0)
            {
                listViewStyles.SelectedItems[0].Selected = false;
            }

            var style = new SsaStyle { Name = LanguageSettings.Current.SubStationAlphaStyles.New };
            if (GetSsaStyle(style.Name).LoadedFromHeader)
            {
                int count = 2;
                bool doRepeat = true;
                while (doRepeat)
                {
                    style = new SsaStyle { Name = LanguageSettings.Current.SubStationAlphaStyles.New + count };
                    doRepeat = GetSsaStyle(style.Name).LoadedFromHeader;
                    count++;
                }
            }

            _doUpdate = false;
            AddStyle(listViewStyles, style, Subtitle, _isSubStationAlpha);
            listViewStyles.Items[listViewStyles.Items.Count - 1].Selected = true;
            listViewStyles.Items[listViewStyles.Items.Count - 1].EnsureVisible();
            listViewStyles.Items[listViewStyles.Items.Count - 1].Focused = true;
            textBoxStyleName.Focus();
            AddStyleToHeader(style);
            _doUpdate = true;
            textBoxStyleName.Text = style.Name;
            SetControlsFromStyle(style);
            listViewStyles_SelectedIndexChanged(null, null);
        }

        private void textBoxStyleName_TextChanged(object sender, EventArgs e)
        {
            if (!_doUpdate)
            {
                return;
            }

            if (listViewStyles.SelectedItems.Count == 1 && _fileStyleActive)
            {
                if (!GetSsaStyle(textBoxStyleName.Text.TrimEnd()).LoadedFromHeader ||
                    _startName == textBoxStyleName.Text.RemoveChar(',').TrimEnd() ||
                    ActiveListView.SelectedItems[0].Text == textBoxStyleName.Text.RemoveChar(',').TrimEnd())
                {
                    textBoxStyleName.BackColor = ActiveListView.BackColor;
                    ActiveListView.SelectedItems[0].Text = textBoxStyleName.Text.RemoveChar(',').TrimEnd();
                    bool found = SetSsaStyle(_oldSsaName, "name", textBoxStyleName.Text.RemoveChar(',').TrimEnd());
                    if (!found)
                    {
                        SetSsaStyle(_oldSsaName, "name", textBoxStyleName.Text.RemoveChar(',').TrimEnd(), false);
                    }

                    _oldSsaName = textBoxStyleName.Text.TrimEnd();
                    _editedName = _oldSsaName;
                }
                else
                {
                    textBoxStyleName.BackColor = Configuration.Settings.Tools.ListViewSyntaxErrorColor;
                }
            }
            else if (listViewStorage.SelectedItems.Count == 1 && !_fileStyleActive)
            {
                textBoxStyleName.BackColor = ActiveListView.BackColor;
                ActiveListView.SelectedItems[0].Text = textBoxStyleName.Text.RemoveChar(',').TrimEnd();
                var idx = ActiveListView.SelectedItems[0].Index;
                _storageStyles[idx].Name = textBoxStyleName.Text.RemoveChar(',').TrimEnd();

                for (int i = 0; i < _storageStyles.Count; i++)
                {
                    var storageName = _storageStyles[i].Name;
                    if (idx != i && storageName == textBoxStyleName.Text.RemoveChar(',').TrimEnd())
                    {
                        textBoxStyleName.BackColor = Configuration.Settings.Tools.ListViewSyntaxErrorColor;
                    }
                }

                _oldSsaName = textBoxStyleName.Text.TrimEnd();
                _editedName = _oldSsaName;
            }

            if (textBoxStyleName.Text.Contains(','))
            {
                textBoxStyleName.BackColor = Configuration.Settings.Tools.ListViewSyntaxErrorColor;
            }
        }

        private void buttonRemove_Click(object sender, EventArgs e)
        {
            if (listViewStyles.SelectedItems.Count == 0)
            {
                return;
            }

            string askText;
            if (listViewStyles.SelectedItems.Count > 1)
            {
                askText = string.Format(LanguageSettings.Current.Main.DeleteXLinesPrompt, listViewStyles.SelectedItems.Count);
            }
            else
            {
                askText = LanguageSettings.Current.Main.DeleteOneLinePrompt;
            }

            if (Configuration.Settings.General.PromptDeleteLines && MessageBox.Show(askText, string.Empty, MessageBoxButtons.YesNoCancel) != DialogResult.Yes)
            {
                return;
            }

            int index = listViewStyles.SelectedItems[0].Index;
            string name = listViewStyles.SelectedItems[0].Text;
            listViewStyles.Items.RemoveAt(listViewStyles.SelectedItems[0].Index);
            RemoveStyleFromHeader(name);

            if (listViewStyles.Items.Count == 0)
            {
                buttonRemoveAll_Click(null, null);
            }
            else
            {
                if (index >= listViewStyles.Items.Count)
                {
                    index--;
                }
                listViewStyles.Items[index].Selected = true;
            }
        }

        private void buttonRemoveAll_Click(object sender, EventArgs e)
        {
            string askText = null;
            if (listViewStyles.Items.Count > 1)
            {
                askText = string.Format(LanguageSettings.Current.Main.DeleteXLinesPrompt, listViewStyles.Items.Count);
            }
            else if (listViewStyles.Items.Count == 1)
            {
                askText = LanguageSettings.Current.Main.DeleteOneLinePrompt;
            }

            if (askText != null && MessageBox.Show(askText, string.Empty, MessageBoxButtons.YesNoCancel) != DialogResult.Yes)
            {
                return;
            }

            listViewStyles.Items.Clear();
            var sub = new Subtitle();
            if (_isSubStationAlpha)
            {
                var ssa = new SubStationAlpha();
                var text = ssa.ToText(sub, string.Empty);
                var lineArray = text.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
                var lines = new List<string>();
                foreach (string line in lineArray)
                {
                    lines.Add(line);
                }
                ssa.LoadSubtitle(sub, lines, string.Empty);
                _header = _header.Remove(_header.IndexOf("[V4 Styles]", StringComparison.Ordinal)) + sub.Header.Substring(sub.Header.IndexOf("[V4 Styles]", StringComparison.Ordinal));
            }
            else
            {
                var ass = new AdvancedSubStationAlpha();
                var text = ass.ToText(sub, string.Empty);
                var lineArray = text.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
                var lines = new List<string>();
                foreach (string line in lineArray)
                {
                    lines.Add(line);
                }
                ass.LoadSubtitle(sub, lines, string.Empty);
                _header = _header.Remove(_header.IndexOf("[V4+ Styles]", StringComparison.Ordinal)) + sub.Header.Substring(sub.Header.IndexOf("[V4+ Styles]", StringComparison.Ordinal));
            }
            InitializeListView();
        }

        private void comboBoxFontName_TextChanged(object sender, EventArgs e)
        {
            var text = comboBoxFontName.Text;
            if (_doUpdate && !string.IsNullOrEmpty(text) && ActiveListView.SelectedItems.Count > 0)
            {
                ActiveListView.SelectedItems[0].SubItems[1].Text = text;
                string name = ActiveListView.SelectedItems[0].Text;
                SetSsaStyle(name, "fontname", text);
                GeneratePreview();
            }
        }

        private void comboBoxFontName_KeyUp(object sender, KeyEventArgs e)
        {
            if (listViewStyles.SelectedItems.Count == 1 && _doUpdate)
            {
                string name = listViewStyles.SelectedItems[0].Text;
                var item = comboBoxFontName.SelectedItem;
                if (item != null)
                {
                    SetSsaStyle(name, "fontname", item.ToString());
                }
                GeneratePreview();
            }
        }

        private void numericUpDownFontSize_ValueChanged(object sender, EventArgs e)
        {
            if (ActiveListView.SelectedItems.Count == 1 && _doUpdate)
            {
                ActiveListView.SelectedItems[0].SubItems[2].Text = numericUpDownFontSize.Value.ToString(CultureInfo.InvariantCulture);
                string name = ActiveListView.SelectedItems[0].Text;
                SetSsaStyle(name, "fontsize", numericUpDownFontSize.Value.ToString(CultureInfo.InvariantCulture));
                GeneratePreview();
            }
        }

        private void UpdateListViewFontStyle(SsaStyle style)
        {
            try
            {
                var fontStyle = FontStyle.Regular;
                if (style.Bold)
                {
                    fontStyle |= FontStyle.Bold;
                }

                if (style.Italic)
                {
                    fontStyle |= FontStyle.Italic;
                }

                if (style.Underline)
                {
                    fontStyle |= FontStyle.Underline;
                }

                var subItem = ActiveListView.SelectedItems[0].SubItems[5];
                subItem.Font = new Font(style.FontName, subItem.Font.Size, fontStyle);
            }
            catch
            {
                // ignored
            }
        }

        private void checkBoxFontBold_CheckedChanged(object sender, EventArgs e)
        {
            if (ActiveListView.SelectedItems.Count == 1 && _doUpdate)
            {
                string name = ActiveListView.SelectedItems[0].Text;
                SetSsaStyle(name, "bold", checkBoxFontBold.Checked ? "-1" : "0");
                UpdateListViewFontStyle(GetSsaStyle(name));
                GeneratePreview();
            }
        }

        private void checkBoxFontItalic_CheckedChanged(object sender, EventArgs e)
        {
            if (ActiveListView.SelectedItems.Count == 1 && _doUpdate)
            {
                string name = ActiveListView.SelectedItems[0].Text;
                SetSsaStyle(name, "italic", checkBoxFontItalic.Checked ? "-1" : "0");
                UpdateListViewFontStyle(GetSsaStyle(name));
                GeneratePreview();
            }
        }

        private void checkBoxUnderline_CheckedChanged(object sender, EventArgs e)
        {
            if (ActiveListView.SelectedItems.Count == 1 && _doUpdate)
            {
                string name = ActiveListView.SelectedItems[0].Text;
                SetSsaStyle(name, "underline", checkBoxFontUnderline.Checked ? "-1" : "0");
                UpdateListViewFontStyle(GetSsaStyle(name));
                GeneratePreview();
            }
        }

        private void radioButtonBottomLeft_CheckedChanged(object sender, EventArgs e)
        {
            if (ActiveListView.SelectedItems.Count == 1 && _doUpdate && ((RadioButton)sender).Checked)
            {
                string name = ActiveListView.SelectedItems[0].Text;
                SetSsaStyle(name, "alignment", "1");
                GeneratePreview();
            }
        }

        private void radioButtonBottomCenter_CheckedChanged(object sender, EventArgs e)
        {
            if (ActiveListView.SelectedItems.Count == 1 && _doUpdate && ((RadioButton)sender).Checked)
            {
                string name = ActiveListView.SelectedItems[0].Text;
                SetSsaStyle(name, "alignment", "2");
                GeneratePreview();
            }
        }

        private void radioButtonBottomRight_CheckedChanged(object sender, EventArgs e)
        {
            if (ActiveListView.SelectedItems.Count == 1 && _doUpdate && ((RadioButton)sender).Checked)
            {
                string name = ActiveListView.SelectedItems[0].Text;
                SetSsaStyle(name, "alignment", "3");
                GeneratePreview();
            }
        }

        private void radioButtonMiddleLeft_CheckedChanged(object sender, EventArgs e)
        {
            if (ActiveListView.SelectedItems.Count == 1 && _doUpdate && ((RadioButton)sender).Checked)
            {
                string name = ActiveListView.SelectedItems[0].Text;
                SetSsaStyle(name, "alignment", _isSubStationAlpha ? "9" : "4");
                GeneratePreview();
            }
        }

        private void radioButtonMiddleCenter_CheckedChanged(object sender, EventArgs e)
        {
            if (ActiveListView.SelectedItems.Count == 1 && _doUpdate && ((RadioButton)sender).Checked)
            {
                string name = ActiveListView.SelectedItems[0].Text;
                SetSsaStyle(name, "alignment", _isSubStationAlpha ? "10" : "5");
                GeneratePreview();
            }
        }

        private void radioButtonMiddleRight_CheckedChanged(object sender, EventArgs e)
        {
            if (ActiveListView.SelectedItems.Count == 1 && _doUpdate && ((RadioButton)sender).Checked)
            {
                string name = ActiveListView.SelectedItems[0].Text;
                SetSsaStyle(name, "alignment", _isSubStationAlpha ? "11" : "6");
                GeneratePreview();
            }
        }

        private void radioButtonTopLeft_CheckedChanged(object sender, EventArgs e)
        {
            if (ActiveListView.SelectedItems.Count == 1 && _doUpdate && ((RadioButton)sender).Checked)
            {
                string name = ActiveListView.SelectedItems[0].Text;
                SetSsaStyle(name, "alignment", _isSubStationAlpha ? "5" : "7");
                GeneratePreview();
            }
        }

        private void radioButtonTopCenter_CheckedChanged(object sender, EventArgs e)
        {
            if (ActiveListView.SelectedItems.Count == 1 && _doUpdate && ((RadioButton)sender).Checked)
            {
                string name = ActiveListView.SelectedItems[0].Text;
                SetSsaStyle(name, "alignment", _isSubStationAlpha ? "6" : "8");
                GeneratePreview();
            }
        }

        private void radioButtonTopRight_CheckedChanged(object sender, EventArgs e)
        {
            if (ActiveListView.SelectedItems.Count == 1 && _doUpdate && ((RadioButton)sender).Checked)
            {
                string name = ActiveListView.SelectedItems[0].Text;
                SetSsaStyle(name, "alignment", _isSubStationAlpha ? "7" : "9");
                GeneratePreview();
            }
        }

        private void numericUpDownMarginLeft_ValueChanged(object sender, EventArgs e)
        {
            if (ActiveListView.SelectedItems.Count == 1 && _doUpdate)
            {
                string name = ActiveListView.SelectedItems[0].Text;
                SetSsaStyle(name, "marginl", numericUpDownMarginLeft.Value.ToString(CultureInfo.InvariantCulture));
                GeneratePreview();
            }
        }

        private void numericUpDownMarginRight_ValueChanged(object sender, EventArgs e)
        {
            if (ActiveListView.SelectedItems.Count == 1 && _doUpdate)
            {
                string name = ActiveListView.SelectedItems[0].Text;
                SetSsaStyle(name, "marginr", numericUpDownMarginRight.Value.ToString(CultureInfo.InvariantCulture));
                GeneratePreview();
            }
        }

        private void numericUpDownMarginVertical_ValueChanged(object sender, EventArgs e)
        {
            if (ActiveListView.SelectedItems.Count == 1 && _doUpdate)
            {
                string name = ActiveListView.SelectedItems[0].Text;
                SetSsaStyle(name, "marginv", numericUpDownMarginVertical.Value.ToString(CultureInfo.InvariantCulture));
                GeneratePreview();
            }
        }

        private void SubStationAlphaStyles_Resize(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Maximized)
            {
                SubStationAlphaStyles_ResizeEnd(sender, e);
                _lastFormWindowState = WindowState;
                return;
            }
            else if (WindowState == FormWindowState.Normal && _lastFormWindowState == FormWindowState.Maximized)
            {
                System.Threading.SynchronizationContext.Current.Post(TimeSpan.FromMilliseconds(25), () =>
                {
                    SubStationAlphaStyles_ResizeEnd(sender, e);
                });
            }

            _lastFormWindowState = WindowState;
        }

        private void SetLastColumnWidth()
        {
            listViewStyles.Columns[listViewStyles.Columns.Count - 1].Width = -2;
            listViewStorage.Columns[listViewStorage.Columns.Count - 1].Width = -2;
        }

        private void SubStationAlphaStyles_ResizeEnd(object sender, EventArgs e)
        {
            SetLastColumnWidth();
            _backgroundImage?.Dispose();
            _backgroundImage = null;
            GeneratePreview();
            _lastFormWindowState = WindowState;
        }

        private void SubStationAlphaStyles_Shown(object sender, EventArgs e)
        {
            SetLastColumnWidth();
        }

        private void numericUpDownOutline_ValueChanged(object sender, EventArgs e)
        {
            if (ActiveListView.SelectedItems.Count == 1 && _doUpdate)
            {
                string name = ActiveListView.SelectedItems[0].Text;
                SetSsaStyle(name, "outline", numericUpDownOutline.Value.ToString(CultureInfo.InvariantCulture));
                GeneratePreview();
            }
        }

        private void numericUpDownShadowWidth_ValueChanged(object sender, EventArgs e)
        {
            if (ActiveListView.SelectedItems.Count == 1 && _doUpdate)
            {
                string name = ActiveListView.SelectedItems[0].Text;
                SetSsaStyle(name, "shadow", numericUpDownShadowWidth.Value.ToString(CultureInfo.InvariantCulture));
                GeneratePreview();
            }
        }

        private void radioButtonOutline_CheckedChanged(object sender, EventArgs e)
        {
            if (sender is RadioButton rb && ActiveListView.SelectedItems.Count == 1 && _doUpdate && rb.Checked)
            {
                string name = ActiveListView.SelectedItems[0].Text;
                SetSsaStyle(name, "outline", numericUpDownOutline.Value.ToString(CultureInfo.InvariantCulture));
                SetSsaStyle(name, "borderstyle", "1");
                GeneratePreview();
            }
        }

        private void radioButtonOpaqueBox_CheckedChanged(object sender, EventArgs e)
        {
            if (sender is RadioButton rb && ActiveListView.SelectedItems.Count == 1 && _doUpdate && rb.Checked)
            {
                string name = ActiveListView.SelectedItems[0].Text;
                SetSsaStyle(name, "outline", numericUpDownOutline.Value.ToString(CultureInfo.InvariantCulture));
                SetSsaStyle(name, "borderstyle", "3");
                GeneratePreview();
            }
        }

        private void SubStationAlphaStyles_SizeChanged(object sender, EventArgs e)
        {
            GeneratePreview();
        }

        private void buttonImport_Click(object sender, EventArgs e)
        {
            openFileDialogImport.Title = LanguageSettings.Current.SubStationAlphaStyles.ImportStyleFromFile;
            openFileDialogImport.InitialDirectory = Configuration.DataDirectory;
            if (_isSubStationAlpha)
            {
                openFileDialogImport.Filter = SubStationAlpha.NameOfFormat + "|*.ssa|" + LanguageSettings.Current.General.AllFiles + "|*.*";
                saveFileDialogStyle.FileName = "my_styles.ssa";
            }
            else
            {
                openFileDialogImport.Filter = AdvancedSubStationAlpha.NameOfFormat + "|*.ass|" + LanguageSettings.Current.General.AllFiles + "|*.*";
                saveFileDialogStyle.FileName = "my_styles.ass";
            }

            if (openFileDialogImport.ShowDialog(this) == DialogResult.OK)
            {
                var s = new Subtitle();
                var format = s.LoadSubtitle(openFileDialogImport.FileName, out _, null);
                if (format != null && format.HasStyleSupport)
                {
                    var styles = AdvancedSubStationAlpha.GetStylesFromHeader(s.Header);
                    if (styles.Count > 0)
                    {
                        using (var cs = new ChooseStyle(s, format.GetType() == typeof(SubStationAlpha)))
                        {
                            if (cs.ShowDialog(this) == DialogResult.OK && cs.SelectedStyleNames.Count > 0)
                            {
                                var styleNames = string.Join(", ", cs.SelectedStyleNames.ToArray());

                                foreach (var styleName in cs.SelectedStyleNames)
                                {
                                    SsaStyle style = AdvancedSubStationAlpha.GetSsaStyle(styleName, s.Header);
                                    if (GetSsaStyleFile(style.Name) != null && GetSsaStyleFile(style.Name).LoadedFromHeader)
                                    {
                                        int count = 2;
                                        bool doRepeat = GetSsaStyleFile(style.Name + count).LoadedFromHeader;
                                        while (doRepeat)
                                        {
                                            doRepeat = GetSsaStyleFile(style.Name + count).LoadedFromHeader;
                                            count++;
                                        }
                                        style.RawLine = style.RawLine.Replace(" " + style.Name + ",", " " + style.Name + count + ",");
                                        style.Name = style.Name + count;
                                    }

                                    _doUpdate = false;
                                    AddStyle(listViewStyles, style, Subtitle, _isSubStationAlpha);
                                    _header = _header.Trim();
                                    if (_header.EndsWith("[Events]", StringComparison.Ordinal))
                                    {
                                        _header = _header.Substring(0, _header.Length - "[Events]".Length).Trim();
                                        _header += Environment.NewLine + style.RawLine;
                                        _header += Environment.NewLine + Environment.NewLine + "[Events]" + Environment.NewLine;
                                    }
                                    else
                                    {
                                        _header = _header.Trim() + Environment.NewLine + style.RawLine + Environment.NewLine;
                                    }

                                    listViewStyles.Items[listViewStyles.Items.Count - 1].Selected = true;
                                    listViewStyles.Items[listViewStyles.Items.Count - 1].EnsureVisible();
                                    listViewStyles.Items[listViewStyles.Items.Count - 1].Focused = true;
                                    textBoxStyleName.Text = style.Name;
                                    textBoxStyleName.Focus();
                                    _doUpdate = true;
                                    SetControlsFromStyle(style);
                                    listViewStyles_SelectedIndexChanged(null, null);
                                }

                                labelStatus.Text = string.Format(LanguageSettings.Current.SubStationAlphaStyles.StyleXImportedFromFileY, styleNames, openFileDialogImport.FileName);
                                timerClearStatus.Start();
                            }
                        }
                    }
                }
            }
        }

        private void buttonExport_Click(object sender, EventArgs e)
        {
            if (listViewStyles.Items.Count == 0)
            {
                return;
            }

            using (var form = new SubStationAlphaStylesExport(_header, _isSubStationAlpha, _format))
            {
                if (form.ShowDialog(this) == DialogResult.OK)
                {
                    var styleNames = string.Join(", ", form.ExportedStyles.ToArray());
                    labelStatus.Text = string.Format(LanguageSettings.Current.SubStationAlphaStyles.StyleXExportedToFileY, styleNames, saveFileDialogStyle.FileName);
                    timerClearStatus.Start();
                }
            }
        }

        private void timerClearStatus_Tick(object sender, EventArgs e)
        {
            timerClearStatus.Stop();
            labelStatus.Text = string.Empty;
        }

        private void pictureBoxPreview_Click(object sender, EventArgs e)
        {
            _backgroundImageDark = !_backgroundImageDark;
            _backgroundImage?.Dispose();
            _backgroundImage = null;
            GeneratePreview();
        }

        private void buttonAddStyleToStorage_Click(object sender, EventArgs e)
        {
            if (listViewStyles.SelectedItems.Count != 1)
            {
                return;
            }

            string styleName = listViewStyles.SelectedItems[0].Text;
            SsaStyle oldStyle = AdvancedSubStationAlpha.GetSsaStyle(styleName, _header);
            var style = new SsaStyle(oldStyle);

            if (StyleExistsInListView(styleName, listViewStorage))
            {
                DialogResult result = DialogResult.Yes;
                if (Configuration.Settings.General.PromptDeleteLines)
                {
                    result = MessageBox.Show(string.Format(LanguageSettings.Current.SubStationAlphaStyles.OverwriteX, styleName), string.Empty, MessageBoxButtons.YesNoCancel);
                }

                if (result != DialogResult.Yes)
                {
                    return;
                }

                var idx = _storageStyles.IndexOf(_storageStyles.First(p => p.Name == styleName));
                _storageStyles[idx] = style;
                listViewStorage.Items.RemoveAt(idx);
                AddStyle(listViewStorage, style, Subtitle, _isSubStationAlpha, idx);
                listViewStorage.Items[idx].Selected = true;
                listViewStorage.Items[idx].EnsureVisible();
                listViewStorage.Items[idx].Focused = true;
                return;
            }

            _storageStyles.Add(style);
            AddStyle(listViewStorage, style, Subtitle, _isSubStationAlpha);
            listViewStorage.Items[listViewStorage.Items.Count - 1].Selected = true;
            listViewStorage.Items[listViewStorage.Items.Count - 1].EnsureVisible();
            listViewStorage.Items[listViewStorage.Items.Count - 1].Focused = true;
        }

        private void listViewStorage_SelectedIndexChanged(object sender, EventArgs e)
        {
            LogNameChanges();

            if (listViewStorage.SelectedItems.Count == 1)
            {
                string styleName = listViewStorage.SelectedItems[0].Text;
                _startName = styleName;
                _editedName = null;
                _oldSsaName = styleName;
                SsaStyle style = _storageStyles.First(p => p.Name == styleName);
                SetControlsFromStyle(style);
                _doUpdate = true;
                groupBoxProperties.Enabled = true;
                GeneratePreview();
                buttonRemove.Enabled = listViewStorage.Items.Count > 1;
            }
            else
            {
                groupBoxProperties.Enabled = false;
                _doUpdate = false;
            }
        }

        private void buttonStorageRemoveAll_Click(object sender, EventArgs e)
        {
            if (_storageStyles.Count == 0)
            {
                return;
            }

            string askText;
            if (_storageStyles.Count > 1)
            {
                askText = string.Format(LanguageSettings.Current.Main.DeleteXLinesPrompt, _storageStyles.Count);
            }
            else
            {
                askText = LanguageSettings.Current.Main.DeleteOneLinePrompt;
            }

            if (MessageBox.Show(askText, string.Empty, MessageBoxButtons.YesNoCancel) != DialogResult.Yes)
            {
                return;
            }

            _storageStyles.Clear();
            listViewStorage.Items.Clear();
        }

        private void buttonStorageRemove_Click(object sender, EventArgs e)
        {
            if (listViewStorage.SelectedItems.Count == 0)
            {
                return;
            }

            string askText;
            if (listViewStorage.SelectedItems.Count > 1)
            {
                askText = string.Format(LanguageSettings.Current.Main.DeleteXLinesPrompt, listViewStorage.SelectedItems.Count);
            }
            else
            {
                askText = LanguageSettings.Current.Main.DeleteOneLinePrompt;
            }

            if (Configuration.Settings.General.PromptDeleteLines && MessageBox.Show(askText, string.Empty, MessageBoxButtons.YesNoCancel) != DialogResult.Yes)
            {
                return;
            }

            if (listViewStorage.SelectedItems.Count == 1)
            {
                int index = listViewStorage.SelectedItems[0].Index;
                _storageStyles.RemoveAt(index);
                listViewStorage.Items.RemoveAt(index);

                if (listViewStorage.Items.Count == 0)
                {
                    buttonStorageRemoveAll_Click(null, null);
                }
                else
                {
                    if (index >= listViewStorage.Items.Count)
                    {
                        index--;
                    }
                    listViewStorage.Items[index].Selected = true;
                }
            }
        }

        private void buttonStorageAdd_Click(object sender, EventArgs e)
        {
            var name = LanguageSettings.Current.SubStationAlphaStyles.New;
            if (_storageStyles.Any(p => p.Name == name))
            {
                int count = 2;
                bool doRepeat = true;
                while (doRepeat)
                {
                    name = LanguageSettings.Current.SubStationAlphaStyles.New + count;
                    doRepeat = _storageStyles.Any(p => p.Name == name);
                    count++;
                }
            }

            _doUpdate = false;
            var style = new SsaStyle { Name = name };
            AddStyle(listViewStorage, style, Subtitle, _isSubStationAlpha);
            _storageStyles.Add(style);
            listViewStorage.Items[listViewStorage.Items.Count - 1].Selected = true;
            listViewStorage.Items[listViewStorage.Items.Count - 1].EnsureVisible();
            listViewStorage.Items[listViewStorage.Items.Count - 1].Focused = true;
            textBoxStyleName.Focus();
            _doUpdate = true;
            textBoxStyleName.Text = style.Name;
            SetControlsFromStyle(style);
            listViewStorage_SelectedIndexChanged(null, null);
        }

        private void buttonStorageCopy_Click(object sender, EventArgs e)
        {
            if (listViewStorage.SelectedItems.Count != 1)
            {
                return;
            }

            var index = listViewStorage.SelectedItems[0].Index;
            SsaStyle oldStyle = _storageStyles[index];
            var style = new SsaStyle(oldStyle) { Name = string.Format(LanguageSettings.Current.SubStationAlphaStyles.CopyOfY, oldStyle.Name) }; // Copy contructor
            var styleName = style.Name;
            if (_storageStyles.Any(p => p.Name == styleName))
            {
                int count = 2;
                bool doRepeat = true;
                while (doRepeat)
                {
                    style.Name = string.Format(LanguageSettings.Current.SubStationAlphaStyles.CopyXOfY, count, styleName);
                    doRepeat = _storageStyles.Any(p => p.Name == styleName);
                    count++;
                }
            }

            _doUpdate = false;
            _storageStyles.Add(style);
            AddStyle(listViewStorage, style, Subtitle, _isSubStationAlpha);
            listViewStorage.Items[listViewStorage.Items.Count - 1].Selected = true;
            listViewStorage.Items[listViewStorage.Items.Count - 1].EnsureVisible();
            listViewStorage.Items[listViewStorage.Items.Count - 1].Focused = true;
            textBoxStyleName.Text = style.Name;
            textBoxStyleName.Focus();
            _doUpdate = true;
            listViewStorage_SelectedIndexChanged(null, null);
        }

        private void buttonStorageImport_Click(object sender, EventArgs e)
        {
            openFileDialogImport.Title = LanguageSettings.Current.SubStationAlphaStyles.ImportStyleFromFile;
            openFileDialogImport.InitialDirectory = Configuration.DataDirectory;
            if (_isSubStationAlpha)
            {
                openFileDialogImport.Filter = SubStationAlpha.NameOfFormat + "|*.ssa|" + LanguageSettings.Current.General.AllFiles + "|*.*";
                saveFileDialogStyle.FileName = "my_styles.ssa";
            }
            else
            {
                openFileDialogImport.Filter = AdvancedSubStationAlpha.NameOfFormat + "|*.ass|" + LanguageSettings.Current.General.AllFiles + "|*.*";
                saveFileDialogStyle.FileName = "my_styles.ass";
            }

            if (openFileDialogImport.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }

            var s = new Subtitle();
            var format = s.LoadSubtitle(openFileDialogImport.FileName, out _, null);
            if (format != null && format.HasStyleSupport)
            {
                var styles = AdvancedSubStationAlpha.GetStylesFromHeader(s.Header);
                if (styles.Count > 0)
                {
                    using (var cs = new ChooseStyle(s, format.GetType() == typeof(SubStationAlpha)))
                    {
                        if (cs.ShowDialog(this) == DialogResult.OK && cs.SelectedStyleNames.Count > 0)
                        {
                            var styleNames = string.Join(", ", cs.SelectedStyleNames.ToArray());

                            foreach (var styleName in cs.SelectedStyleNames)
                            {
                                SsaStyle style = AdvancedSubStationAlpha.GetSsaStyle(styleName, s.Header);
                                if (GetSsaStyleStorage(style.Name) != null && GetSsaStyleStorage(style.Name).LoadedFromHeader)
                                {
                                    int count = 2;
                                    bool doRepeat = GetSsaStyleStorage(style.Name + count) != null;
                                    while (doRepeat)
                                    {
                                        doRepeat = GetSsaStyleStorage(style.Name + count) != null;
                                        count++;
                                    }
                                    style.RawLine = style.RawLine.Replace(" " + style.Name + ",", " " + style.Name + count + ",");
                                    style.Name = style.Name + count;
                                }

                                _doUpdate = false;
                                _storageStyles.Add(style);
                                AddStyle(listViewStorage, style, Subtitle, _isSubStationAlpha);
                                listViewStorage.Items[listViewStorage.Items.Count - 1].Selected = true;
                                listViewStorage.Items[listViewStorage.Items.Count - 1].EnsureVisible();
                                listViewStorage.Items[listViewStorage.Items.Count - 1].Focused = true;
                                textBoxStyleName.Text = style.Name;
                                textBoxStyleName.Focus();
                                _doUpdate = true;
                                SetControlsFromStyle(style);
                                listViewStorage_SelectedIndexChanged(null, null);
                            }

                            labelStatus.Text = string.Format(LanguageSettings.Current.SubStationAlphaStyles.StyleXImportedFromFileY, styleNames, openFileDialogImport.FileName);
                            timerClearStatus.Start();
                        }
                    }
                }
            }
        }

        private void buttonStorageExport_Click(object sender, EventArgs e)
        {
            if (listViewStorage.Items.Count == 0)
            {
                return;
            }

            using (var form = new SubStationAlphaStylesExport(GetStorageHeader(), _isSubStationAlpha, _format))
            {
                if (form.ShowDialog(this) == DialogResult.OK)
                {
                    var styleNames = string.Join(", ", form.ExportedStyles.ToArray());
                    labelStatus.Text = string.Format(LanguageSettings.Current.SubStationAlphaStyles.StyleXExportedToFileY, styleNames, saveFileDialogStyle.FileName);
                    timerClearStatus.Start();
                }
            }
        }

        private void listViewStyles_Enter(object sender, EventArgs e)
        {
            groupBoxStorage.Font = new Font(groupBoxStyles.Font, FontStyle.Regular);
            groupBoxStyles.Font = new Font(groupBoxStyles.Font, FontStyle.Bold);
            _fileStyleActive = true;
            listViewStyles_SelectedIndexChanged(null, null);

            buttonAddStyleToStorage.Enabled = true;
            buttonAddToFile.Enabled = false;
        }

        private void listViewStorage_Enter(object sender, EventArgs e)
        {
            groupBoxStyles.Font = new Font(groupBoxStyles.Font, FontStyle.Regular);
            groupBoxStorage.Font = new Font(groupBoxStyles.Font, FontStyle.Bold);
            _fileStyleActive = false;
            listViewStorage_SelectedIndexChanged(null, null);

            buttonAddStyleToStorage.Enabled = false;
            buttonAddToFile.Enabled = true;
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
        }

        private void listViewStorage_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Modifiers == Keys.Control && e.KeyCode == Keys.C)
            {
                buttonStorageCopy_Click(null, null);
            }
            else if (e.Modifiers == Keys.None && e.KeyCode == Keys.Delete)
            {
                buttonStorageRemove_Click(null, null);
            }
        }

        private bool StyleExistsInListView(string styleName, ListView listView)
        {
            foreach (ListViewItem item in listView.Items)
            {
                if (styleName == item.Text)
                {
                    return true;
                }
            }

            return false;
        }

        private List<ListViewItem> GetListItemsAsList(ListView listView)
        {
            var list = new List<ListViewItem>(listView.Items.Count);
            foreach (ListViewItem item in listView.Items)
            {
                list.Add(item);
            }

            return list;
        }

        private void buttonAddToFile_Click(object sender, EventArgs e)
        {
            if (listViewStorage.SelectedItems.Count != 1)
            {
                return;
            }

            string styleName = listViewStorage.SelectedItems[0].Text;
            SsaStyle oldStyle = _storageStyles.FirstOrDefault(p => p.Name == styleName);
            var style = new SsaStyle(oldStyle);

            if (StyleExistsInListView(styleName, listViewStyles))
            {
                DialogResult result = DialogResult.Yes;
                if (Configuration.Settings.General.PromptDeleteLines)
                {
                    result = MessageBox.Show(string.Format(LanguageSettings.Current.SubStationAlphaStyles.OverwriteX, styleName), string.Empty, MessageBoxButtons.YesNoCancel);
                }

                if (result != DialogResult.Yes)
                {
                    return;
                }

                var items = GetListItemsAsList(listViewStyles);
                var idx = items.IndexOf(items.First(p => p.Text == styleName));
                listViewStyles.Items.RemoveAt(idx);
                ReplaceStyleInHeader(style);
                AddStyle(listViewStyles, style, Subtitle, _isSubStationAlpha, idx);
                listViewStyles.Items[idx].Selected = true;
                listViewStyles.Items[idx].EnsureVisible();
                listViewStyles.Items[idx].Focused = true;
                return;
            }

            AddStyle(listViewStyles, style, Subtitle, _isSubStationAlpha);
            AddStyleToHeader(style);
            listViewStyles.Items[listViewStyles.Items.Count - 1].Selected = true;
            listViewStyles.Items[listViewStyles.Items.Count - 1].EnsureVisible();
            listViewStyles.Items[listViewStyles.Items.Count - 1].Focused = true;
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
            if (listView == listViewStorage)
            {
                var style = _storageStyles[idx];
                _storageStyles.RemoveAt(idx);
                _storageStyles.Insert(idx - 1, style);
            }

            idx--;
            listView.Items.Insert(idx, item);
            listView.Items[idx].Selected = true;
            listView.Items[idx].EnsureVisible();
            listView.Items[idx].Focused = true;
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
            if (listView == listViewStorage)
            {
                var style = _storageStyles[idx];
                _storageStyles.RemoveAt(idx);
                _storageStyles.Insert(idx + 1, style);
            }

            idx++;
            listView.Items.Insert(idx, item);
            listView.Items[idx].Selected = true;
            listView.Items[idx].EnsureVisible();
            listView.Items[idx].Focused = true;
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
            if (listView == listViewStorage)
            {
                var style = _storageStyles[idx];
                _storageStyles.RemoveAt(idx);
                _storageStyles.Insert(0, style);
            }

            idx = 0;
            listView.Items.Insert(idx, item);
            listView.Items[idx].Selected = true;
            listView.Items[idx].EnsureVisible();
            listView.Items[idx].Focused = true;
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
            if (listView == listViewStorage)
            {
                var style = _storageStyles[idx];
                _storageStyles.RemoveAt(idx);
                _storageStyles.Insert(listView.Items.Count - 1, style);
            }

            idx = listView.Items.Count - 1;
            listView.Items.Insert(idx, item);
            listView.Items[idx].Selected = true;
            listView.Items[idx].EnsureVisible();
            listView.Items[idx].Focused = true;
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

        private void toolStripMenuItem3_Click(object sender, EventArgs e)
        {
            MoveUp(listViewStorage);
        }

        private void toolStripMenuItemStorageMoveDown_Click(object sender, EventArgs e)
        {
            MoveDown(listViewStorage);
        }

        private void toolStripMenuItemStorageMoveTop_Click(object sender, EventArgs e)
        {
            MoveToTop(listViewStorage);
        }

        private void toolStripMenuItemStorageMoveBottom_Click(object sender, EventArgs e)
        {
            MoveToBottom(listViewStorage);
        }

        private void chooseBackgroundImageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            openFileDialogImport.Filter = "Images|*.png;*.jpg";
            openFileDialogImport.FileName = string.Empty;
            if (openFileDialogImport.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }

            _fixedBackgroundImage = ExportPngXml.ResizeBitmap((Bitmap)Image.FromFile(openFileDialogImport.FileName), pictureBoxPreview.Width, pictureBoxPreview.Height);
            GeneratePreview();
        }

        private void setPreviewTextToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (var form = new TextPrompt(string.Empty, LanguageSettings.Current.SubStationAlphaStyles.SetPreviewText.TrimEnd('.'), Configuration.Settings.General.PreviewAssaText))
            {
                if (form.ShowDialog(this) == DialogResult.OK)
                {
                    Configuration.Settings.General.PreviewAssaText = form.InputText;
                    GeneratePreview();
                }
            }
        }

        private void contextMenuStripStorage_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            var isNotEmpty = listViewStorage.Items.Count > 0;
            toolStripMenuItemStorageRemove.Visible = isNotEmpty;
            toolStripMenuItemStorageRemoveAll.Visible = isNotEmpty;
            toolStripSeparator2.Visible = isNotEmpty;
            toolStripMenuItemStorageCopy.Visible = isNotEmpty;

            var moreThanOne = listViewStorage.Items.Count > 1;
            toolStripMenuItemStorageMoveUp.Visible = moreThanOne;
            toolStripMenuItemStorageMoveBottom.Visible = moreThanOne;
            toolStripMenuItemStorageMoveTop.Visible = moreThanOne;
            toolStripMenuItemStorageMoveDown.Visible = moreThanOne;
            toolStripSeparator5.Visible = moreThanOne;
        }

        private void contextMenuStripFile_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            var moreThanOne = listViewStorage.Items.Count > 1;
            moveUpToolStripMenuItem.Visible = moreThanOne;
            moveBottomToolStripMenuItem.Visible = moreThanOne;
            moveTopToolStripMenuItem.Visible = moreThanOne;
            moveDownToolStripMenuItem.Visible = moreThanOne;
            toolStripSeparator3.Visible = moreThanOne;
            toolStripMenuItemRemoveAll.Visible = moreThanOne;
        }
    }
}
