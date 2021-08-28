using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Forms.Assa;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.VideoPlayers;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Text;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.Forms.Styles
{
    public sealed partial class SubStationAlphaStyles : StylesForm
    {
        public class NameEdit
        {
            public string OldName { get; set; }
            public string NewName { get; set; }

            public NameEdit(string oldName, string newName)
            {
                OldName = oldName;
                NewName = newName;
            }
        }

        public List<NameEdit> RenameActions { get; set; }
        private string _startName;
        private string _editedName;
        private string _header;
        private readonly List<SsaStyle> _currentFileStyles;
        private bool _doUpdate;
        private string _oldSsaName;
        private readonly SubtitleFormat _format;
        private readonly bool _isSubStationAlpha;
        private Bitmap _backgroundImage;
        private bool _backgroundImageDark;
        private bool _fileStyleActive = true;
        private FormWindowState _lastFormWindowState;
        private readonly Main _mainForm;
        private readonly List<AssaAttachmentFont> _fontAttachments;
        private LibMpvDynamic _mpv;
        private string _mpvTextFileName;

        public SubStationAlphaStyles(Subtitle subtitle, SubtitleFormat format, Main mainForm)
            : base(subtitle)
        {
            UiUtil.PreInitialize(this);
            InitializeComponent();
            UiUtil.FixFonts(this);

            RenameActions = new List<NameEdit>();
            labelStatus.Text = string.Empty;
            _header = subtitle.Header;
            _format = format;
            _mainForm = mainForm;
            buttonApply.Visible = _mainForm != null;
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

            _currentFileStyles = new List<SsaStyle>();
            foreach (var styleName in AdvancedSubStationAlpha.GetStylesFromHeader(_header))
            {
                var style = AdvancedSubStationAlpha.GetSsaStyle(styleName, _header);
                if (style != null)
                {
                    _currentFileStyles.Add(style);
                }
            }

            _fontAttachments = new List<AssaAttachmentFont>();
            if (subtitle.Footer != null)
            {
                GetFonts(subtitle.Footer.SplitToLines());
            }

            comboBoxFontName.Items.Clear();
            foreach (var x in FontFamily.Families)
            {
                comboBoxFontName.Items.Add(x.Name);
            }

            var l = LanguageSettings.Current.SubStationAlphaStyles;
            Text = l.Title;
            groupBoxStyles.Text = _isSubStationAlpha ? l.Styles : l.StyleCurrentFile;
            listViewStyles.Columns[0].Text = l.Name;
            listViewStyles.Columns[1].Text = l.FontName;
            listViewStyles.Columns[2].Text = l.FontSize;
            listViewStyles.Columns[3].Text = l.UseCount;
            listViewStyles.Columns[4].Text = l.Primary;
            listViewStyles.Columns[5].Text = l.Outline;
            groupBoxProperties.Text = l.Properties;
            labelStyleName.Text = l.Name;
            groupBoxFont.Text = l.Font;
            labelFontName.Text = l.FontName;
            labelFontSize.Text = l.FontSize;
            checkBoxFontItalic.Text = LanguageSettings.Current.General.Italic;
            checkBoxFontBold.Text = LanguageSettings.Current.General.Bold;
            checkBoxFontUnderline.Text = LanguageSettings.Current.General.Underline;
            checkBoxStrikeout.Text = LanguageSettings.Current.General.Strikeout;
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

            labelScaleX.Text = l.ScaleX;
            labelScaleY.Text = l.ScaleY;
            labelSpacing.Text = l.Spacing;
            labelAngle.Text = l.Angle;
            labelScaleX.Text = l.ScaleX;
            numericUpDownScaleX.Left = labelScaleX.Left + labelScaleX.Width + 2;
            labelScaleY.Left = numericUpDownScaleX.Left + numericUpDownScaleX.Width + 9;
            numericUpDownScaleY.Left = labelScaleY.Left + labelScaleY.Width + 2;
            labelSpacing.Left = numericUpDownScaleY.Left + numericUpDownScaleY.Width + 9;
            numericUpDownSpacing.Left = labelSpacing.Left + labelSpacing.Width + 2;
            labelAngle.Left = numericUpDownSpacing.Left + numericUpDownSpacing.Width + 9;
            numericUpDownAngle.Left = labelAngle.Left + labelAngle.Width + 2;
            deleteToolStripMenuItem.Text = l.Remove;
            toolStripMenuItemRemoveAll.Text = l.RemoveAll;
            addToStorageToolStripMenuItem1.Text = l.AddToStorage;
            moveUpToolStripMenuItem.Text = LanguageSettings.Current.DvdSubRip.MoveUp;
            moveDownToolStripMenuItem.Text = LanguageSettings.Current.DvdSubRip.MoveDown;
            moveTopToolStripMenuItem.Text = LanguageSettings.Current.MultipleReplace.MoveToTop;
            moveBottomToolStripMenuItem.Text = LanguageSettings.Current.MultipleReplace.MoveToBottom;
            newToolStripMenuItemNew.Text = l.New;
            copyToolStripMenuItemCopy.Text = l.Copy;
            toolStripMenuItemImport.Text = l.Import;
            toolStripMenuItemExport.Text = l.Export;
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
                checkBoxStrikeout.Visible = false;
                labelScaleX.Visible = false;
                labelScaleY.Visible = false;
                labelSpacing.Visible = false;
                labelAngle.Visible = false;
                numericUpDownScaleX.Visible = false;
                numericUpDownScaleY.Visible = false;
                numericUpDownSpacing.Visible = false;
                numericUpDownAngle.Visible = false;
                numericUpDownOutline.Increment = 1;
                numericUpDownOutline.DecimalPlaces = 0;
                numericUpDownShadowWidth.Increment = 1;
                numericUpDownShadowWidth.DecimalPlaces = 0;

                splitContainer1.Panel2Collapsed = true;
                buttonAddStyleToStorage.Visible = false;
            }

            buttonApply.Text = LanguageSettings.Current.General.Apply;
            buttonOK.Text = LanguageSettings.Current.General.Ok;
            buttonCancel.Text = LanguageSettings.Current.General.Cancel;

            InitializeStylesListView();
            UiUtil.FixLargeFonts(this, buttonCancel);

            comboBoxFontName.Left = labelFontName.Left + labelFontName.Width + 2;
            buttonPickAttachmentFont.Visible = subtitle.Footer != null &&
                                               subtitle.Footer.Contains("[Fonts]") &&
                                               subtitle.Footer.Contains("fontname:");
            buttonPickAttachmentFont.Left = comboBoxFontName.Left + comboBoxFontName.Width + 3;
            var controlLeftOfFontSize = buttonPickAttachmentFont.Visible ? buttonPickAttachmentFont : (Control)comboBoxFontName;
            labelFontSize.Left = controlLeftOfFontSize.Left + controlLeftOfFontSize.Width + 15;
            numericUpDownFontSize.Left = labelFontSize.Left + labelFontSize.Width + 2;

            numericUpDownOutline.Left = radioButtonOutline.Left + radioButtonOutline.Width + 3;
            labelShadow.Left = numericUpDownOutline.Left + numericUpDownOutline.Width + 3;
            numericUpDownShadowWidth.Left = numericUpDownOutline.Left + numericUpDownOutline.Width + 3;
            checkBoxFontItalic.Left = checkBoxFontBold.Left + checkBoxFontBold.Width + 12;
            checkBoxFontUnderline.Left = checkBoxFontItalic.Left + checkBoxFontItalic.Width + 12;
            checkBoxStrikeout.Left = checkBoxFontUnderline.Left + checkBoxFontUnderline.Width + 12;
        }

        public override string Header => GetFileHeader(_currentFileStyles);

        private void ResetHeader()
        {
            var format = _isSubStationAlpha ? (SubtitleFormat)new SubStationAlpha() : new AdvancedSubStationAlpha();
            var sub = new Subtitle();
            var text = format.ToText(sub, string.Empty);
            var lines = text.SplitToLines();
            format.LoadSubtitle(sub, lines, string.Empty);
            _header = sub.Header;
        }

        private void GetFonts(List<string> lines)
        {
            bool attachmentOn = false;
            var attachmentContent = new StringBuilder();
            var attachmentFileName = string.Empty;
            var category = string.Empty;
            foreach (var line in lines)
            {
                var s = line.Trim();
                if (attachmentOn)
                {
                    if (s == "[V4+ Styles]" || s == "[Events]")
                    {
                        SaveFontNames(attachmentFileName, attachmentContent.ToString(), category);
                        attachmentOn = false;
                        attachmentContent = new StringBuilder();
                        attachmentFileName = string.Empty;
                    }
                    else if (s.Length == 0)
                    {
                        SaveFontNames(attachmentFileName, attachmentContent.ToString(), category);
                        attachmentContent = new StringBuilder();
                        attachmentFileName = string.Empty;
                    }
                    else if (s.StartsWith("filename:", StringComparison.Ordinal) || s.StartsWith("fontname:", StringComparison.Ordinal))
                    {
                        SaveFontNames(attachmentFileName, attachmentContent.ToString(), category);
                        attachmentContent = new StringBuilder();
                        attachmentFileName = s.Remove(0, 9).Trim();
                    }
                    else
                    {
                        attachmentContent.AppendLine(s);
                    }
                }
                else if (s == "[Fonts]" || s == "[Graphics]")
                {
                    category = s;
                    attachmentOn = true;
                    attachmentContent = new StringBuilder();
                    attachmentFileName = string.Empty;
                }
            }

            SaveFontNames(attachmentFileName, attachmentContent.ToString(), category);
        }

        private void SaveFontNames(string attachmentFileName, string attachmentContent, string category)
        {
            var content = attachmentContent.Trim();
            if (string.IsNullOrEmpty(attachmentFileName) || content.Length == 0 || !attachmentFileName.EndsWith(".ttf", StringComparison.InvariantCultureIgnoreCase))
            {
                return;
            }

            var bytes = UUEncoding.UUDecode(content);
            foreach (var fontName in GetFontNames(bytes))
            {
                _fontAttachments.Add(new AssaAttachmentFont { FileName = attachmentFileName, FontName = fontName, Bytes = bytes, Category = category, Content = content });
            }
        }

        private List<string> GetFontNames(byte[] fontBytes)
        {
            var privateFontCollection = new PrivateFontCollection();
            var handle = GCHandle.Alloc(fontBytes, GCHandleType.Pinned);
            var pointer = handle.AddrOfPinnedObject();
            try
            {
                privateFontCollection.AddMemoryFont(pointer, fontBytes.Length);
            }
            finally
            {
                handle.Free();
            }

            var resultList = new List<string>();
            foreach (var font in privateFontCollection.Families)
            {
                resultList.Add(font.Name);
            }

            privateFontCollection.Dispose();
            return resultList;
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
            var format = new SubStationAlpha();
            var subtitle = new Subtitle();
            var p = new Paragraph(Configuration.Settings.General.PreviewAssaText, 0, 10000);
            subtitle.Paragraphs.Add(p);
            try
            {
                p.Extra = listViewStyles.SelectedItems[0].Text;
            }
            catch
            {
                return;
            }
            subtitle.Header = GetFileHeader(_currentFileStyles);
            var text = subtitle.ToText(format);
            _mpvTextFileName = FileUtil.GetTempFileName(format.Extension);
            File.WriteAllText(_mpvTextFileName, text);
            _mpv.LoadSubtitle(_mpvTextFileName);
            _mpv.Pause();
            _mpv.CurrentPosition = 0.5;
        }

        protected override void GeneratePreviewReal()
        {
            if (listViewStyles.SelectedItems.Count != 1 || pictureBoxPreview.Width <= 0 || pictureBoxPreview.Height <= 0)
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

            var outlineWidth = (float)numericUpDownOutline.Value;
            var shadowWidth = (float)numericUpDownShadowWidth.Value;
            var outlineColor = _isSubStationAlpha ? panelBackColor.BackColor : panelOutlineColor.BackColor;

            Font font = null;
            var privateFontCollection = new PrivateFontCollection();
            try
            {
                var fontName = comboBoxFontName.Text;

                // try to load font from memory
                var fontAttachment = _fontAttachments.Find(p => p.FontName == fontName);
                if (fontAttachment != null)
                {
                    var handle = GCHandle.Alloc(fontAttachment.Bytes, GCHandleType.Pinned);
                    var pointer = handle.AddrOfPinnedObject();
                    try
                    {
                        privateFontCollection.AddMemoryFont(pointer, fontAttachment.Bytes.Length);
                        var fontFamily = privateFontCollection.Families.FirstOrDefault();
                        if (fontFamily != null)
                        {
                            font = new Font(fontFamily, (float)numericUpDownFontSize.Value * 1.1f, checkBoxFontBold.Checked ? FontStyle.Bold : FontStyle.Regular);
                            if (checkBoxFontItalic.Checked)
                            {
                                font = new Font(fontFamily, (float)numericUpDownFontSize.Value * 1.1f, font.Style | FontStyle.Italic);
                            }

                            if (checkBoxFontUnderline.Checked)
                            {
                                font = new Font(fontFamily, (float)numericUpDownFontSize.Value * 1.1f, font.Style | FontStyle.Underline);
                            }

                            if (checkBoxStrikeout.Checked)
                            {
                                font = new Font(fontFamily, (float)numericUpDownFontSize.Value * 1.1f, font.Style | FontStyle.Strikeout);
                            }
                        }
                    }
                    catch
                    {
                        font = null;
                    }
                    finally
                    {
                        handle.Free();
                    }
                }

                if (font == null)
                {
                    font = new Font(fontName, (float)numericUpDownFontSize.Value * 1.1f, checkBoxFontBold.Checked ? FontStyle.Bold : FontStyle.Regular);
                    if (checkBoxFontItalic.Checked)
                    {
                        font = new Font(fontName, (float)numericUpDownFontSize.Value * 1.1f, font.Style | FontStyle.Italic);
                    }
                    if (checkBoxFontUnderline.Checked)
                    {
                        font = new Font(fontName, (float)numericUpDownFontSize.Value * 1.1f, font.Style | FontStyle.Underline);
                    }
                    if (checkBoxStrikeout.Checked)
                    {
                        font = new Font(fontName, (float)numericUpDownFontSize.Value * 1.1f, font.Style | FontStyle.Strikeout);
                    }
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
            privateFontCollection.Dispose();
        }

        private void InitializeStylesListView()
        {
            if (_currentFileStyles.Count == 0)
            {
                AddStyleToHeader(new SsaStyle());
            }

            listViewStyles.Items.Clear();
            foreach (var style in _currentFileStyles)
            {
                AddStyle(listViewStyles, style, Subtitle, _isSubStationAlpha);
            }

            if (listViewStyles.Items.Count > 0)
            {
                listViewStyles.Items[0].Selected = true;
            }
        }

        private static string FixDuplicateStyleName(string newStyleName, List<SsaStyle> existingStyles)
        {
            if (existingStyles.All(p => p.Name != newStyleName))
            {
                return newStyleName;
            }

            for (int i = 1; i < int.MaxValue; i++)
            {
                var name = $"{newStyleName}_{i}";
                if (existingStyles.All(p => p.Name != name))
                {
                    return name;
                }
            }

            return Guid.NewGuid().ToString();
        }

        public static void AddStyle(ListView lv, SsaStyle ssaStyle, Subtitle subtitle, bool isSubstationAlpha)
        {
            AddStyle(lv, ssaStyle, subtitle, isSubstationAlpha, lv.Items.Count);
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
            foreach (var p in subtitle.Paragraphs)
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
            try
            {
                return ColorTranslator.FromWin32(int.Parse(color));
            }
            catch
            {
                return Color.Yellow;
            }
        }

        private bool SetSsaStyle(string styleName, string propertyName, string propertyValue)
        {
            var style = _currentFileStyles.Find(p => p.Name == styleName);
            if (style == null)
            {
                return false;
            }

            if (propertyName == "name")
            {
                style.Name = propertyValue;
            }
            else if (propertyName == "fontname")
            {
                style.FontName = propertyValue;
            }
            else if (propertyName == "fontsize")
            {
                style.FontSize = float.Parse(propertyValue, CultureInfo.InvariantCulture);
            }
            else if (propertyName == "bold")
            {
                style.Bold = propertyValue != "0";
            }
            else if (propertyName == "italic")
            {
                style.Italic = propertyValue != "0";
            }
            else if (propertyName == "underline")
            {
                style.Underline = propertyValue != "0";
            }
            else if (propertyName == "strikeout")
            {
                style.Strikeout = propertyValue != "0";
            }
            else if (propertyName == "alignment")
            {
                style.Alignment = propertyValue;
            }
            else if (propertyName == "marginl")
            {
                style.MarginLeft = int.Parse(propertyValue, CultureInfo.InvariantCulture);
            }
            else if (propertyName == "marginr")
            {
                style.MarginRight = int.Parse(propertyValue, CultureInfo.InvariantCulture);
            }
            else if (propertyName == "marginv")
            {
                style.MarginVertical = int.Parse(propertyValue, CultureInfo.InvariantCulture);
            }
            else if (propertyName == "outline")
            {
                style.OutlineWidth = decimal.Parse(propertyValue, CultureInfo.InvariantCulture);
            }
            else if (propertyName == "shadow")
            {
                style.ShadowWidth = decimal.Parse(propertyValue, CultureInfo.InvariantCulture);
            }
            else if (propertyName == "borderstyle")
            {
                style.BorderStyle = propertyValue;
            }
            else if (propertyName == "primarycolour")
            {
                style.Primary = GetColorFromSsa(propertyValue);
            }
            else if (propertyName == "secondarycolour")
            {
                style.Secondary = GetColorFromSsa(propertyValue);
            }
            else if (propertyName == "outlinecolour")
            {
                style.Outline = GetColorFromSsa(propertyValue);
            }
            else if (propertyName == "tertiarycolour")
            {
                style.Tertiary = GetColorFromSsa(propertyValue);
            }
            else if (propertyName == "backcolour")
            {
                style.Background = GetColorFromSsa(propertyValue);
            }
            else if (propertyName == "scalex")
            {
                style.ScaleX = decimal.Parse(propertyValue, CultureInfo.InvariantCulture);
            }
            else if (propertyName == "scaley")
            {
                style.ScaleY = decimal.Parse(propertyValue, CultureInfo.InvariantCulture);
            }
            else if (propertyName == "spacing")
            {
                style.Spacing = decimal.Parse(propertyValue, CultureInfo.InvariantCulture);
            }
            else if (propertyName == "angle")
            {
                style.Angle = decimal.Parse(propertyValue, CultureInfo.InvariantCulture);
            }
            else
            {
                return false;
            }

            return true;
        }

        private SsaStyle GetSsaStyle(string styleName) => GetSsaStyleFile(styleName);

        private SsaStyle GetSsaStyleFile(string styleName) => _currentFileStyles.Find(p => p.Name == styleName);

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

        private void buttonOK_Click(object sender, EventArgs e)
        {
            _header = GetFileHeader(_currentFileStyles);
            LogNameChanges();
            DialogResult = DialogResult.OK;
        }

        private string GetFileHeader(List<SsaStyle> styles)
        {
            var sb = new StringBuilder();
            var format = SsaStyle.DefaultAssStyleFormat;
            var stylesAdded = false;
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
                else if (!stylesAdded && line.Trim().StartsWith("Style:", StringComparison.OrdinalIgnoreCase))
                {
                    stylesAdded = true;
                    foreach (var style in styles)
                    {
                        sb.AppendLine(style.ToRawSsa(format));
                    }

                    if (styles.Count == 0)
                    {
                        sb.AppendLine(new SsaStyle().ToRawSsa(format));
                    }

                    sb.AppendLine();
                    continue;
                }
                else if (stylesAdded && line.Trim().StartsWith("Style:", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                sb.AppendLine(line);
            }

            sb = new StringBuilder(sb.ToString().TrimEnd());
            sb.AppendLine();
            sb.AppendLine();
            sb.AppendLine("[Events]");
            return sb.ToString();
        }

        private void UpdateSelectedIndices(ListView listview, int startingIndex = -1, int numberOfSelectedItems = 1)
        {
            if (numberOfSelectedItems == 0)
            {
                return;
            }

            if (startingIndex == -1 || startingIndex >= listview.Items.Count)
            {
                startingIndex = listview.Items.Count - 1;
            }

            if (startingIndex - numberOfSelectedItems < -1)
            {
                return;
            }

            listview.SelectedItems.Clear();
            for (int i = 0; i < numberOfSelectedItems; i++)
            {
                listview.Items[startingIndex - i].Selected = true;
                listview.Items[startingIndex - i].EnsureVisible();
                listview.Items[startingIndex - i].Focused = true;
            }
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
                SsaStyle style = GetSsaStyleFile(styleName);
                SetControlsFromStyle(style);
                _doUpdate = true;
                groupBoxProperties.Enabled = true;
                GeneratePreview();
            }
            else
            {
                groupBoxProperties.Enabled = false;
                _doUpdate = false;
                pictureBoxPreview.Image?.Dispose();
                pictureBoxPreview.Image = new Bitmap(1, 1);
            }

            UpdateCurrentFileButtonsState();
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
            checkBoxStrikeout.Checked = style.Strikeout;
            numericUpDownScaleX.Value = style.ScaleX;
            numericUpDownScaleY.Value = style.ScaleY;
            numericUpDownSpacing.Value = style.Spacing;
            numericUpDownAngle.Value = style.Angle;

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
            var name = listViewStyles.SelectedItems[0].Text;
            colorDialogSSAStyle.Color = panelPrimaryColor.BackColor;
            if (colorDialogSSAStyle.ShowDialog() == DialogResult.OK)
            {
                listViewStyles.SelectedItems[0].SubItems[4].BackColor = colorDialogSSAStyle.Color;
                listViewStyles.SelectedItems[0].SubItems[5].ForeColor = colorDialogSSAStyle.Color;
                panelPrimaryColor.BackColor = colorDialogSSAStyle.Color;
                SetSsaStyle(name, "primarycolour", GetSsaColorString(colorDialogSSAStyle.Color));
                GeneratePreview();
            }
        }

        private void buttonSecondaryColor_Click(object sender, EventArgs e)
        {
            var name = listViewStyles.SelectedItems[0].Text;
            colorDialogSSAStyle.Color = panelSecondaryColor.BackColor;
            if (colorDialogSSAStyle.ShowDialog() == DialogResult.OK)
            {
                panelSecondaryColor.BackColor = colorDialogSSAStyle.Color;
                SetSsaStyle(name, "secondarycolour", GetSsaColorString(colorDialogSSAStyle.Color));
                GeneratePreview();
            }
        }

        private void buttonOutlineColor_Click(object sender, EventArgs e)
        {
            var name = listViewStyles.SelectedItems[0].Text;
            colorDialogSSAStyle.Color = panelOutlineColor.BackColor;
            if (colorDialogSSAStyle.ShowDialog() == DialogResult.OK)
            {
                panelOutlineColor.BackColor = colorDialogSSAStyle.Color;
                SetSsaStyle(name, "tertiarycolour", GetSsaColorString(colorDialogSSAStyle.Color));
                GeneratePreview();
            }
        }

        private void buttonShadowColor_Click(object sender, EventArgs e)
        {
            var name = listViewStyles.SelectedItems[0].Text;
            colorDialogSSAStyle.Color = panelBackColor.BackColor;
            if (colorDialogSSAStyle.ShowDialog() == DialogResult.OK)
            {
                listViewStyles.SelectedItems[0].SubItems[4].BackColor = colorDialogSSAStyle.Color;
                panelBackColor.BackColor = colorDialogSSAStyle.Color;
                SetSsaStyle(name, "backcolour", GetSsaColorString(colorDialogSSAStyle.Color));
                GeneratePreview();
            }
        }

        private static string GetSsaColorString(Color c)
        {
            return Color.FromArgb(0, c.B, c.G, c.R).ToArgb().ToString(); ;
        }

        private void buttonCopy_Click(object sender, EventArgs e)
        {
            var selectionCount = listViewStyles.SelectedItems.Count;
            if (selectionCount <= 0)
            {
                return;
            }

            foreach (ListViewItem selectedItem in listViewStyles.SelectedItems)
            {
                var styleName = selectedItem.Text;
                var oldStyle = GetSsaStyleFile(styleName);
                var style = new SsaStyle(oldStyle) { Name = string.Format(LanguageSettings.Current.SubStationAlphaStyles.CopyOfY, styleName) }; // Copy constructor

                if (GetSsaStyleFile(style.Name) != null)
                {
                    var count = 2;
                    var doRepeat = true;
                    while (doRepeat)
                    {
                        style.Name = string.Format(LanguageSettings.Current.SubStationAlphaStyles.CopyXOfY, count, styleName);
                        doRepeat = GetSsaStyleFile(style.Name) != null;
                        count++;
                    }
                }

                _doUpdate = false;
                AddStyle(listViewStyles, style, Subtitle, _isSubStationAlpha);
                AddStyleToHeader(style);
                _doUpdate = true;
            }

            UpdateSelectedIndices(listViewStyles, numberOfSelectedItems: selectionCount);
        }

        private void AddStyleToHeader(SsaStyle style)
        {
            _currentFileStyles.Add(style);
        }

        private void RemoveStyleFromHeader(string name)
        {
            _currentFileStyles.Remove(_currentFileStyles.Find(p => p.Name == name));
        }

        private void ReplaceStyleInHeader(SsaStyle style)
        {
            var hit = _currentFileStyles.Find(p => p.Name == style.Name);
            if (hit == null)
            {
                return;
            }

            var index = _currentFileStyles.IndexOf(hit);
            _currentFileStyles.RemoveAt(index);
            _currentFileStyles.Insert(index, style);
        }

        private void buttonAdd_Click(object sender, EventArgs e)
        {
            var name = LanguageSettings.Current.SubStationAlphaStyles.New;
            if (GetSsaStyleFile(name) != null)
            {
                var count = 2;
                var doRepeat = true;
                while (doRepeat)
                {
                    name = LanguageSettings.Current.SubStationAlphaStyles.New + count;
                    doRepeat = GetSsaStyleFile(name) != null;
                    count++;
                }
            }

            _doUpdate = false;
            var style = new SsaStyle { Name = name };
            AddStyle(listViewStyles, style, Subtitle, _isSubStationAlpha);
            AddStyleToHeader(style);
            _doUpdate = true;
            UpdateSelectedIndices(listViewStyles);
            SetControlsFromStyle(style);
            textBoxStyleName.Focus();
        }

        private void textBoxStyleName_TextChanged(object sender, EventArgs e)
        {
            if (!_doUpdate)
            {
                return;
            }

            if (listViewStyles.SelectedItems.Count == 1 && _fileStyleActive)
            {
                if (GetSsaStyle(textBoxStyleName.Text.Trim()) == null ||
                    _startName == textBoxStyleName.Text.RemoveChar(',').Trim() ||
                    listViewStyles.SelectedItems[0].Text == textBoxStyleName.Text.RemoveChar(',').Trim())
                {
                    textBoxStyleName.BackColor = listViewStyles.BackColor;
                    listViewStyles.SelectedItems[0].Text = textBoxStyleName.Text.RemoveChar(',').Trim();
                    var found = SetSsaStyle(_oldSsaName, "name", textBoxStyleName.Text.RemoveChar(',').Trim());
                    if (!found)
                    {
                        SetSsaStyle(_oldSsaName, "name", textBoxStyleName.Text.RemoveChar(',').Trim());
                    }

                    _oldSsaName = textBoxStyleName.Text.Trim();
                    _editedName = _oldSsaName;
                }
                else
                {
                    textBoxStyleName.BackColor = Configuration.Settings.Tools.ListViewSyntaxErrorColor;
                }
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

            var askText = listViewStyles.SelectedItems.Count > 1 ?
                string.Format(LanguageSettings.Current.Main.DeleteXLinesPrompt, listViewStyles.SelectedItems.Count) :
                LanguageSettings.Current.Main.DeleteOneLinePrompt;

            if (Configuration.Settings.General.PromptDeleteLines &&
                MessageBox.Show(askText, string.Empty, MessageBoxButtons.YesNoCancel) != DialogResult.Yes)
            {
                return;
            }

            foreach (ListViewItem selectedItem in listViewStyles.SelectedItems)
            {
                var name = selectedItem.Text;
                listViewStyles.Items.RemoveAt(listViewStyles.SelectedItems[0].Index);
                RemoveStyleFromHeader(name);
            }

            if (listViewStyles.Items.Count == 0)
            {
                InitializeStylesListView();
            }

            UpdateSelectedIndices(listViewStyles);
        }

        private void buttonRemoveAll_Click(object sender, EventArgs e)
        {
            var askText = listViewStyles.Items.Count == 1 ?
                LanguageSettings.Current.Main.DeleteOneLinePrompt :
                string.Format(LanguageSettings.Current.Main.DeleteXLinesPrompt, listViewStyles.Items.Count);

            if (MessageBox.Show(askText, string.Empty, MessageBoxButtons.YesNoCancel) != DialogResult.Yes)
            {
                return;
            }

            listViewStyles.Items.Clear();
            _currentFileStyles.Clear();
            InitializeStylesListView();
            UpdateSelectedIndices(listViewStyles);
        }

        private void comboBoxFontName_TextChanged(object sender, EventArgs e)
        {
            var text = comboBoxFontName.Text;
            if (_doUpdate && !string.IsNullOrEmpty(text) && listViewStyles.SelectedItems.Count > 0)
            {
                listViewStyles.SelectedItems[0].SubItems[1].Text = text;
                var name = listViewStyles.SelectedItems[0].Text;
                SetSsaStyle(name, "fontname", text);
                GeneratePreview();
            }
        }

        private void comboBoxFontName_KeyUp(object sender, KeyEventArgs e)
        {
            if (listViewStyles.SelectedItems.Count == 1 && _doUpdate)
            {
                var name = listViewStyles.SelectedItems[0].Text;
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
            if (listViewStyles.SelectedItems.Count == 1 && _doUpdate)
            {
                listViewStyles.SelectedItems[0].SubItems[2].Text = numericUpDownFontSize.Value.ToString(CultureInfo.InvariantCulture);
                var name = listViewStyles.SelectedItems[0].Text;
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

                if (style.Strikeout)
                {
                    fontStyle |= FontStyle.Strikeout;
                }

                var subItem = listViewStyles.SelectedItems[0].SubItems[5];
                subItem.Font = new Font(style.FontName, subItem.Font.Size, fontStyle);
            }
            catch
            {
                // ignored
            }
        }

        private void checkBoxFontBold_CheckedChanged(object sender, EventArgs e)
        {
            if (listViewStyles.SelectedItems.Count == 1 && _doUpdate)
            {
                var name = listViewStyles.SelectedItems[0].Text;
                SetSsaStyle(name, "bold", checkBoxFontBold.Checked ? "-1" : "0");
                UpdateListViewFontStyle(GetSsaStyle(name));
                GeneratePreview();
            }
        }

        private void checkBoxFontItalic_CheckedChanged(object sender, EventArgs e)
        {
            if (listViewStyles.SelectedItems.Count == 1 && _doUpdate)
            {
                var name = listViewStyles.SelectedItems[0].Text;
                SetSsaStyle(name, "italic", checkBoxFontItalic.Checked ? "-1" : "0");
                UpdateListViewFontStyle(GetSsaStyle(name));
                GeneratePreview();
            }
        }

        private void checkBoxUnderline_CheckedChanged(object sender, EventArgs e)
        {
            if (listViewStyles.SelectedItems.Count == 1 && _doUpdate)
            {
                var name = listViewStyles.SelectedItems[0].Text;
                SetSsaStyle(name, "underline", checkBoxFontUnderline.Checked ? "-1" : "0");
                UpdateListViewFontStyle(GetSsaStyle(name));
                GeneratePreview();
            }
        }

        private void checkBoxStrikeout_CheckedChanged(object sender, EventArgs e)
        {
            if (listViewStyles.SelectedItems.Count == 1 && _doUpdate)
            {
                var name = listViewStyles.SelectedItems[0].Text;
                SetSsaStyle(name, "strikeout", checkBoxStrikeout.Checked ? "-1" : "0");
                UpdateListViewFontStyle(GetSsaStyle(name));
                GeneratePreview();
            }
        }

        private void radioButtonBottomLeft_CheckedChanged(object sender, EventArgs e)
        {
            if (listViewStyles.SelectedItems.Count == 1 && _doUpdate && ((RadioButton)sender).Checked)
            {
                var name = listViewStyles.SelectedItems[0].Text;
                SetSsaStyle(name, "alignment", "1");
                GeneratePreview();
            }
        }

        private void radioButtonBottomCenter_CheckedChanged(object sender, EventArgs e)
        {
            if (listViewStyles.SelectedItems.Count == 1 && _doUpdate && ((RadioButton)sender).Checked)
            {
                var name = listViewStyles.SelectedItems[0].Text;
                SetSsaStyle(name, "alignment", "2");
                GeneratePreview();
            }
        }

        private void radioButtonBottomRight_CheckedChanged(object sender, EventArgs e)
        {
            if (listViewStyles.SelectedItems.Count == 1 && _doUpdate && ((RadioButton)sender).Checked)
            {
                var name = listViewStyles.SelectedItems[0].Text;
                SetSsaStyle(name, "alignment", "3");
                GeneratePreview();
            }
        }

        private void radioButtonMiddleLeft_CheckedChanged(object sender, EventArgs e)
        {
            if (listViewStyles.SelectedItems.Count == 1 && _doUpdate && ((RadioButton)sender).Checked)
            {
                var name = listViewStyles.SelectedItems[0].Text;
                SetSsaStyle(name, "alignment", _isSubStationAlpha ? "9" : "4");
                GeneratePreview();
            }
        }

        private void radioButtonMiddleCenter_CheckedChanged(object sender, EventArgs e)
        {
            if (listViewStyles.SelectedItems.Count == 1 && _doUpdate && ((RadioButton)sender).Checked)
            {
                var name = listViewStyles.SelectedItems[0].Text;
                SetSsaStyle(name, "alignment", _isSubStationAlpha ? "10" : "5");
                GeneratePreview();
            }
        }

        private void radioButtonMiddleRight_CheckedChanged(object sender, EventArgs e)
        {
            if (listViewStyles.SelectedItems.Count == 1 && _doUpdate && ((RadioButton)sender).Checked)
            {
                var name = listViewStyles.SelectedItems[0].Text;
                SetSsaStyle(name, "alignment", _isSubStationAlpha ? "11" : "6");
                GeneratePreview();
            }
        }

        private void radioButtonTopLeft_CheckedChanged(object sender, EventArgs e)
        {
            if (listViewStyles.SelectedItems.Count == 1 && _doUpdate && ((RadioButton)sender).Checked)
            {
                var name = listViewStyles.SelectedItems[0].Text;
                SetSsaStyle(name, "alignment", _isSubStationAlpha ? "5" : "7");
                GeneratePreview();
            }
        }

        private void radioButtonTopCenter_CheckedChanged(object sender, EventArgs e)
        {
            if (listViewStyles.SelectedItems.Count == 1 && _doUpdate && ((RadioButton)sender).Checked)
            {
                var name = listViewStyles.SelectedItems[0].Text;
                SetSsaStyle(name, "alignment", _isSubStationAlpha ? "6" : "8");
                GeneratePreview();
            }
        }

        private void radioButtonTopRight_CheckedChanged(object sender, EventArgs e)
        {
            if (listViewStyles.SelectedItems.Count == 1 && _doUpdate && ((RadioButton)sender).Checked)
            {
                var name = listViewStyles.SelectedItems[0].Text;
                SetSsaStyle(name, "alignment", _isSubStationAlpha ? "7" : "9");
                GeneratePreview();
            }
        }

        private void numericUpDownMarginLeft_ValueChanged(object sender, EventArgs e)
        {
            if (listViewStyles.SelectedItems.Count == 1 && _doUpdate)
            {
                var name = listViewStyles.SelectedItems[0].Text;
                SetSsaStyle(name, "marginl", numericUpDownMarginLeft.Value.ToString(CultureInfo.InvariantCulture));
                GeneratePreview();
            }
        }

        private void numericUpDownMarginRight_ValueChanged(object sender, EventArgs e)
        {
            if (listViewStyles.SelectedItems.Count == 1 && _doUpdate)
            {
                var name = listViewStyles.SelectedItems[0].Text;
                SetSsaStyle(name, "marginr", numericUpDownMarginRight.Value.ToString(CultureInfo.InvariantCulture));
                GeneratePreview();
            }
        }

        private void numericUpDownMarginVertical_ValueChanged(object sender, EventArgs e)
        {
            if (listViewStyles.SelectedItems.Count == 1 && _doUpdate)
            {
                var name = listViewStyles.SelectedItems[0].Text;
                SetSsaStyle(name, "marginv", numericUpDownMarginVertical.Value.ToString(CultureInfo.InvariantCulture));
                GeneratePreview();
            }
        }

        private void SetLastColumnWidth()
        {
            listViewStyles.AutoSizeLastColumn();
        }

        private void listViewStyles_ClientSizeChanged(object sender, EventArgs e)
        {
            SetLastColumnWidth();
        }

        private void listViewStorage_ClientSizeChanged(object sender, EventArgs e)
        {
            SetLastColumnWidth();
        }

        private void SubStationAlphaStyles_ResizeEnd(object sender, EventArgs e)
        {
            _backgroundImage?.Dispose();
            _backgroundImage = null;
            GeneratePreview();
            _lastFormWindowState = WindowState;
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

        private void SubStationAlphaStyles_Shown(object sender, EventArgs e)
        {
            SetLastColumnWidth();
        }

        private void numericUpDownOutline_ValueChanged(object sender, EventArgs e)
        {
            if (listViewStyles.SelectedItems.Count == 1 && _doUpdate)
            {
                var name = listViewStyles.SelectedItems[0].Text;
                SetSsaStyle(name, "outline", numericUpDownOutline.Value.ToString(CultureInfo.InvariantCulture));
                GeneratePreview();
            }
        }

        private void numericUpDownShadowWidth_ValueChanged(object sender, EventArgs e)
        {
            if (listViewStyles.SelectedItems.Count == 1 && _doUpdate)
            {
                var name = listViewStyles.SelectedItems[0].Text;
                SetSsaStyle(name, "shadow", numericUpDownShadowWidth.Value.ToString(CultureInfo.InvariantCulture));
                GeneratePreview();
            }
        }

        private void radioButtonOutline_CheckedChanged(object sender, EventArgs e)
        {
            if (sender is RadioButton rb && listViewStyles.SelectedItems.Count == 1 && _doUpdate && rb.Checked)
            {
                var name = listViewStyles.SelectedItems[0].Text;
                SetSsaStyle(name, "outline", numericUpDownOutline.Value.ToString(CultureInfo.InvariantCulture));
                SetSsaStyle(name, "borderstyle", "1");
                GeneratePreview();
            }
        }

        private void radioButtonOpaqueBox_CheckedChanged(object sender, EventArgs e)
        {
            if (sender is RadioButton rb && listViewStyles.SelectedItems.Count == 1 && _doUpdate && rb.Checked)
            {
                var name = listViewStyles.SelectedItems[0].Text;
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
                                    var style = AdvancedSubStationAlpha.GetSsaStyle(styleName, s.Header);
                                    if (GetSsaStyleFile(style.Name) != null && GetSsaStyleFile(style.Name) != null)
                                    {
                                        int count = 2;
                                        bool doRepeat = GetSsaStyleFile(style.Name + count) != null;
                                        while (doRepeat)
                                        {
                                            doRepeat = GetSsaStyleFile(style.Name + count) != null;
                                            count++;
                                        }
                                        style.RawLine = style.RawLine.Replace(" " + style.Name + ",", " " + style.Name + count + ",");
                                        style.Name += count;
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

                                    UpdateSelectedIndices(listViewStyles);
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

            using (var form = new SubStationAlphaStylesExport(_currentFileStyles, _isSubStationAlpha, _format))
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

        private void listViewStyles_Enter(object sender, EventArgs e)
        {
            groupBoxStyles.Font = new Font(groupBoxStyles.Font, FontStyle.Bold);
            _fileStyleActive = true;
            listViewStyles_SelectedIndexChanged(null, null);
        }

        private void UpdateCurrentFileButtonsState()
        {
            bool oneOrMoreSelected = listViewStyles.SelectedItems.Count > 0;
            buttonRemove.Enabled = oneOrMoreSelected;
            buttonCopy.Enabled = oneOrMoreSelected;
            buttonAddStyleToStorage.Enabled = oneOrMoreSelected;
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

            var style = _currentFileStyles[idx];
            _currentFileStyles.RemoveAt(idx);
            _currentFileStyles.Insert(idx - 1, style);


            idx--;
            listView.Items.Insert(idx, item);
            UpdateSelectedIndices(listView, idx);
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

            var style = _currentFileStyles[idx];
            _currentFileStyles.RemoveAt(idx);
            _currentFileStyles.Insert(idx + 1, style);

            idx++;
            listView.Items.Insert(idx, item);
            UpdateSelectedIndices(listView, idx);
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

            var style = _currentFileStyles[idx];
            _currentFileStyles.RemoveAt(idx);
            _currentFileStyles.Insert(0, style);

            idx = 0;
            listView.Items.Insert(idx, item);
            UpdateSelectedIndices(listView, idx);
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

            var style = _currentFileStyles[idx];
            _currentFileStyles.RemoveAt(idx);
            _currentFileStyles.Add(style);

            listView.Items.Add(item);
            UpdateSelectedIndices(listView);
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

        private void contextMenuStripFile_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            var isNotEmpty = listViewStyles.Items.Count > 0;
            toolStripMenuItemRemoveAll.Visible = isNotEmpty;
            toolStripSeparator4.Visible = isNotEmpty;
            toolStripMenuItemExport.Visible = isNotEmpty;

            var oneOrMoreSelected = listViewStyles.SelectedItems.Count > 0;
            deleteToolStripMenuItem.Visible = oneOrMoreSelected;
            addToStorageToolStripMenuItem1.Visible = oneOrMoreSelected;
            toolStripSeparator4.Visible = oneOrMoreSelected;
            copyToolStripMenuItemCopy.Visible = oneOrMoreSelected;

            var moreThanOneExists = listViewStyles.Items.Count > 1;
            moveUpToolStripMenuItem.Visible = oneOrMoreSelected && moreThanOneExists;
            moveBottomToolStripMenuItem.Visible = oneOrMoreSelected && moreThanOneExists;
            moveTopToolStripMenuItem.Visible = oneOrMoreSelected && moreThanOneExists;
            moveDownToolStripMenuItem.Visible = oneOrMoreSelected && moreThanOneExists;
            toolStripSeparator3.Visible = oneOrMoreSelected && moreThanOneExists;
        }



        private void buttonApply_Click(object sender, EventArgs e)
        {
            _header = Header;
            LogNameChanges();
            _mainForm?.ApplySsaStyles(this);
        }

        private void buttonPickAttachmentFont_Click(object sender, EventArgs e)
        {
            using (var form = new ChooseAssaFontName(_fontAttachments))
            {
                if (form.ShowDialog(this) == DialogResult.OK)
                {
                    comboBoxFontName.Text = form.FontName;
                }
            }
        }

        private void SubStationAlphaStyles_FormClosing(object sender, FormClosingEventArgs e)
        {
            _mpv?.Dispose();
        }

        private void numericUpDownScaleX_ValueChanged(object sender, EventArgs e)
        {
            if (listViewStyles.SelectedItems.Count == 1 && _doUpdate)
            {
                var name = listViewStyles.SelectedItems[0].Text;
                SetSsaStyle(name, "scalex", numericUpDownScaleX.Value.ToString(CultureInfo.InvariantCulture));
                UpdateListViewFontStyle(GetSsaStyle(name));
                GeneratePreview();
            }
        }

        private void numericUpDownScaleY_ValueChanged(object sender, EventArgs e)
        {
            if (listViewStyles.SelectedItems.Count == 1 && _doUpdate)
            {
                var name = listViewStyles.SelectedItems[0].Text;
                SetSsaStyle(name, "scaley", numericUpDownScaleY.Value.ToString(CultureInfo.InvariantCulture));
                UpdateListViewFontStyle(GetSsaStyle(name));
                GeneratePreview();
            }
        }

        private void numericUpDownSpacing_ValueChanged(object sender, EventArgs e)
        {
            if (listViewStyles.SelectedItems.Count == 1 && _doUpdate)
            {
                var name = listViewStyles.SelectedItems[0].Text;
                SetSsaStyle(name, "spacing", numericUpDownSpacing.Value.ToString(CultureInfo.InvariantCulture));
                UpdateListViewFontStyle(GetSsaStyle(name));
                GeneratePreview();
            }
        }

        private void numericUpDownAngle_ValueChanged(object sender, EventArgs e)
        {
            if (listViewStyles.SelectedItems.Count == 1 && _doUpdate)
            {
                var name = listViewStyles.SelectedItems[0].Text;
                SetSsaStyle(name, "angle", numericUpDownAngle.Value.ToString(CultureInfo.InvariantCulture));
                UpdateListViewFontStyle(GetSsaStyle(name));
                GeneratePreview();
            }
        }
    }
}
