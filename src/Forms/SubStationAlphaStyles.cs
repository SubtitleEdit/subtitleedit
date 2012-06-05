using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Text;
using System.Windows.Forms;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.SubtitleFormats;

namespace Nikse.SubtitleEdit.Forms
{

    public partial class SubStationAlphaStyles : Form
    {
        public string Header { get; private set; }
        private Subtitle _subtitle = null;
        private bool _doUpdate = false;
        private string _oldSsaName = null;
        private Timer _previewTimer = new Timer();
        private SubtitleFormat _format = null;
        private bool _isSubStationAlpha = false;

        private class SsaStyle
        {
            public string Name { get; set; }
            public string FontName { get; set; }
            public int FontSize { get; set; }
            public bool Italic { get; set; }
            public bool Bold { get; set; }
            public bool Underline { get; set; }
            public Color Primary { get; set; }
            public Color Secondary { get; set; }
            public Color Tertiary { get; set; }
            public Color Outline { get; set; }
            public Color Background { get; set; }
            public int ShadowWidth { get; set; }
            public int OutlineWidth { get; set; }
            public string Alignment { get; set; }
            public int MarginLeft { get; set; }
            public int MarginRight { get; set; }
            public int MarginVertical { get; set; }
            public string BorderStyle { get; set; }
            public string RawLine { get; set; }
            public bool LoadedFromHeader { get; set; }

            public SsaStyle()
            {
                FontName = Configuration.Settings.SubtitleSettings.SsaFontName;
                FontSize = (int)Configuration.Settings.SubtitleSettings.SsaFontSize;
                Primary = Color.FromArgb(Configuration.Settings.SubtitleSettings.SsaFontColorArgb);
                Secondary = Color.Yellow;
                Outline = Color.Black;
                Background = Color.Black;
                Alignment = "2";
                OutlineWidth = 2;
                ShadowWidth = 2;
                MarginLeft = 10;
                MarginRight = 10;
                MarginVertical = 10;
                BorderStyle = "1";
                RawLine = string.Empty;
                LoadedFromHeader = false;
            }
        }

        public SubStationAlphaStyles(Subtitle subtitle, SubtitleFormat format)
        {
            InitializeComponent();

            Header = subtitle.Header;
            _format = format;
            _isSubStationAlpha = _format.FriendlyName == new SubStationAlpha().FriendlyName;
            if (Header == null || !Header.ToLower().Contains("style:"))
                ResetHeader();

            _subtitle = subtitle;

            comboBoxFontName.Items.Clear();
            foreach (var x in System.Drawing.FontFamily.Families)
                comboBoxFontName.Items.Add(x.Name);

            var l = Configuration.Settings.Language.SubStationAlphaStyles;
            Text = l.Title;
            groupBoxStyles.Text = l.Styles;
            listViewStyles.Columns[0].Text = l.Name;
            listViewStyles.Columns[1].Text = l.FontName;
            listViewStyles.Columns[2].Text = l.FontSize;
            listViewStyles.Columns[3].Text = l.UseCount;
            listViewStyles.Columns[4].Text = l.Primary;
            listViewStyles.Columns[5].Text = l.Outline;
            groupBoxProperties.Text = l.Properties;
            labelStyleName.Text = l.Name;
            groupBoxFont.Text = l.FontSize;
            labelFontName.Text = l.FontName;
            labelFontSize.Text = l.FontSize;
            checkBoxFontItalic.Text = Configuration.Settings.Language.General.Italic;
            checkBoxFontBold.Text = Configuration.Settings.Language.General.Bold;
            checkBoxFontUnderline.Text = Configuration.Settings.Language.General.Underline;
            groupBoxAlignment.Text = l.Alignment;
            radioButtonTopLeft.Text = l.TopLeft;
            radioButtonTopCenter.Text = l.TopCenter;
            radioButtonTopRight.Text = l.TopRight;
            radioButtonMiddleLeft.Text = l.MiddleLeft;
            radioButtonMiddleCenter.Text = l.MiddleCenter;
            radioButtonMiddleRight.Text = l.MiddleRight;
            radioButtonBottomLeft.Text = l.BottomLeft;
            radioButtonBottomCenter.Text = l.BottomCenter;
            radioButtonBottomRight.Text = l.BottomRight;
            groupBoxColors.Text = l.Colors;
            buttonPrimaryColor.Text = l.Primary;
            buttonSecondaryColor.Text = l.Secondary;
            buttonOutlineColor.Text = l.Outline;
            buttonBackColor.Text = l.Shadow;
            groupBoxMargins.Text = l.Margins;
            labelMarginLeft.Text = l.MarginLeft;
            labelMarginRight.Text = l.MarginRight;
            labelMarginRight.Text = l.MarginRight;
            labelMarginVertical.Text = l.MarginVertical;
            groupBoxBorder.Text = l.Border;
            radioButtonOutline.Text = l.Outline;
            labelShadow.Text = l.PlusShadow;
            radioButtonOpaqueBox.Text = l.OpaqueBox;
            buttonImport.Text = l.Import;
            buttonCopy.Text = l.Copy;
            buttonAdd.Text = l.New;
            buttonRemove.Text = l.Remove;
            buttonRemoveAll.Text = l.RemoveAll;

            if (_isSubStationAlpha)
            {
                Text = l.TitleSubstationAlpha;
                buttonOutlineColor.Text = l.Tertiary;
                buttonBackColor.Text = l.Back;
                listViewStyles.Columns[5].Text = l.Back;
                checkBoxFontUnderline.Visible = false;
            }

            buttonOK.Text = Configuration.Settings.Language.General.OK;
            buttonCancel.Text = Configuration.Settings.Language.General.Cancel;

            InitializeListView();
            FixLargeFonts();
            _previewTimer.Interval = 200;
            _previewTimer.Tick += RefreshTimerTick;

            comboBoxFontName.Left = labelFontName.Left + labelFontName.Width + 10;
            numericUpDownFontSize.Left = labelFontSize.Left + labelFontSize.Width + 10;
            if (comboBoxFontName.Left > numericUpDownFontSize.Left)
                numericUpDownFontSize.Left = comboBoxFontName.Left;
            else
                comboBoxFontName.Left = numericUpDownFontSize.Left;

            numericUpDownOutline.Left = radioButtonOutline.Left + radioButtonOutline.Width + 5;
            labelShadow.Left = numericUpDownOutline.Left + numericUpDownOutline.Width + 5;
            numericUpDownShadowWidth.Left = labelShadow.Left + labelShadow.Width + 5;

            listViewStyles.Columns[5].Width = -2;
        }

        void RefreshTimerTick(object sender, EventArgs e)
        {
            _previewTimer.Stop();
            GeneratePreviewReal();
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
            if (listViewStyles.SelectedItems.Count != 1)
                return;

            if (pictureBoxPreview.Image != null)
                pictureBoxPreview.Image.Dispose();
            var bmp = new Bitmap(pictureBoxPreview.Width, pictureBoxPreview.Height);

            using (Graphics g = Graphics.FromImage(bmp))
            {

                // Draw background
                const int rectangleSize = 9;
                for (int y = 0; y < bmp.Height; y += rectangleSize)
                {
                    for (int x = 0; x < bmp.Width; x += rectangleSize)
                    {
                        Color c = Color.WhiteSmoke;
                        if (y % (rectangleSize * 2) == 0)
                        {
                            if (x % (rectangleSize * 2) == 0)
                                c = Color.LightGray;
                        }
                        else
                        {
                            if (x % (rectangleSize * 2) != 0)
                                c = Color.LightGray;
                        }
                        g.FillRectangle(new SolidBrush(c), x, y, rectangleSize, rectangleSize);
                    }
                }

                // Draw text
                Font font;
                try
                {
                    font = new Font(comboBoxFontName.Text, (float)numericUpDownFontSize.Value);
                }
                catch
                {
                    font = new Font(Font, FontStyle.Regular);
                }
                g.TextRenderingHint = TextRenderingHint.AntiAlias;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                var sf = new StringFormat { Alignment = StringAlignment.Near, LineAlignment = StringAlignment.Near };
                var path = new GraphicsPath();

                bool newLine = false;
                var sb = new StringBuilder();
                sb.Append("This is a test!");

                var measuredWidth = TextDraw.MeasureTextWidth(font, sb.ToString(), checkBoxFontBold.Checked);
                var measuredHeight = g.MeasureString(sb.ToString(), font).Height -9;

                float left = 5;
                if (radioButtonTopLeft.Checked || radioButtonMiddleLeft.Checked || radioButtonBottomLeft.Checked)
                    left = (float)numericUpDownMarginLeft.Value;
                else if (radioButtonTopRight.Checked || radioButtonMiddleRight.Checked || radioButtonBottomRight.Checked)
                    left = bmp.Width - (measuredWidth + ((float)numericUpDownMarginRight.Value));
                else
                    left = ((float)(bmp.Width - measuredWidth * 0.8 + 15) / 2);


                float top = 2;
                if (radioButtonTopLeft.Checked || radioButtonTopCenter.Checked || radioButtonTopRight.Checked)
                    top = (float)numericUpDownMarginVertical.Value;
                else if (radioButtonMiddleLeft.Checked || radioButtonMiddleCenter.Checked || radioButtonMiddleRight.Checked)
                    top = (bmp.Height - measuredHeight) / 2;
                else
                    top = bmp.Height - measuredHeight - ((int)numericUpDownMarginVertical.Value);

                int addX = 0;
                int leftMargin = 0;
                int pathPointsStart = -1;

                if (radioButtonOpaqueBox.Checked)
                {
                    if (_isSubStationAlpha)
                        g.FillRectangle(new SolidBrush(panelBackColor.BackColor), left, top, measuredWidth + 3, measuredHeight);
                    else
                        g.FillRectangle(new SolidBrush(panelOutlineColor.BackColor), left, top, measuredWidth + 3, measuredHeight);
                }

                TextDraw.DrawText(font, sf, path, sb, checkBoxFontItalic.Checked, checkBoxFontBold.Checked, checkBoxFontUnderline.Checked, left, top, ref newLine, addX, leftMargin, ref pathPointsStart);

                int outline = (int)numericUpDownOutline.Value;

                // draw shadow
                if (numericUpDownShadowWidth.Value > 0 && radioButtonOutline.Checked)
                {
                    for (int i = 0; i < (int)numericUpDownShadowWidth.Value; i++)
                    {
                        var shadowPath = new GraphicsPath();
                        sb = new StringBuilder();
                        sb.Append("This is a test!");
                        int pathPointsStart2 = -1;
                        TextDraw.DrawText(font, sf, shadowPath, sb, checkBoxFontItalic.Checked, checkBoxFontBold.Checked, checkBoxFontUnderline.Checked, left + i + outline, top + i + outline, ref newLine, addX, leftMargin, ref pathPointsStart2);
                        g.FillPath(new SolidBrush(Color.FromArgb(200, panelBackColor.BackColor)), shadowPath);
                    }
                }

                if (outline > 0 && radioButtonOutline.Checked)
                {
                    if (_isSubStationAlpha)
                        g.DrawPath(new Pen(panelBackColor.BackColor, outline), path);
                    else
                        g.DrawPath(new Pen(panelOutlineColor.BackColor, outline), path);
                }
                g.FillPath(new SolidBrush(panelPrimaryColor.BackColor), path);


            }
            pictureBoxPreview.Image = bmp;
        }

        private void FixLargeFonts()
        {
            Graphics graphics = CreateGraphics();
            SizeF textSize = graphics.MeasureString(buttonCancel.Text, Font);
            if (textSize.Height > buttonCancel.Height - 4)
            {
                int newButtonHeight = (int)(textSize.Height + 7 + 0.5);
                Utilities.SetButtonHeight(this, newButtonHeight, 1);
            }
        }

        private void InitializeListView()
        {
            var styles = AdvancedSubStationAlpha.GetStylesFromHeader(Header);
            listViewStyles.Items.Clear();
            foreach (string style in styles)
            {
                SsaStyle ssaStyle = GetSsaStyle(style);
                AddStyle(ssaStyle);
            }
            if (listViewStyles.Items.Count > 0)
                listViewStyles.Items[0].Selected = true;
        }

        private void AddStyle(SsaStyle ssaStyle)
        {
            var item = new ListViewItem(ssaStyle.Name);
            item.UseItemStyleForSubItems = false;

            var subItem = new ListViewItem.ListViewSubItem(item, ssaStyle.FontName);
            item.SubItems.Add(subItem);

            subItem = new ListViewItem.ListViewSubItem(item, ssaStyle.FontSize.ToString());
            item.SubItems.Add(subItem);

            int count = 0;
            foreach (Paragraph p in _subtitle.Paragraphs)
            {
                if (string.IsNullOrEmpty(p.Extra) && ssaStyle.Name.TrimStart('*') == "Default")
                    count++;
                else if (ssaStyle.Name.TrimStart('*').ToLower() == p.Extra.TrimStart('*').ToLower())
                    count++;
            }
            subItem = new ListViewItem.ListViewSubItem(item, count.ToString());
            item.SubItems.Add(subItem);

            subItem = new ListViewItem.ListViewSubItem(item, string.Empty);
            subItem.BackColor = ssaStyle.Primary;
            item.SubItems.Add(subItem);

            subItem = new ListViewItem.ListViewSubItem(item, string.Empty);
            if (_isSubStationAlpha)
                subItem.BackColor = ssaStyle.Background;
            else
                subItem.BackColor = ssaStyle.Outline;
            item.SubItems.Add(subItem);

            listViewStyles.Items.Add(item);
        }

        private void SetSsaStyle(string styleName, string propertyName, string propertyValue)
        {
            int propertyIndex = -1;
            int nameIndex = -1;
            var sb = new StringBuilder();
            foreach (string line in Header.Split(Environment.NewLine.ToCharArray(), StringSplitOptions.None))
            {
                string s = line.Trim().ToLower();
                if (s.StartsWith("format:"))
                {
                    if (line.Length > 10)
                    {
                        var format = line.ToLower().Substring(8).Split(',');
                        for (int i = 0; i < format.Length; i++)
                        {
                            string f = format[i].Trim().ToLower();
                            if (f == "name")
                                nameIndex = i;
                            if (f == propertyName)
                                propertyIndex = i;
                        }
                    }
                    sb.AppendLine(line);
                }
                else if (s.Replace(" ", string.Empty).StartsWith("style:"))
                {
                    if (line.Length > 10)
                    {
                        bool correctLine = false;
                        var format = line.Substring(6).Split(',');
                        for (int i = 0; i < format.Length; i++)
                        {
                            string f = format[i].Trim();
                            if (i == nameIndex)
                                correctLine = f.ToLower() == styleName.ToLower();
                        }
                        if (correctLine)
                        {
                            sb.Append(line.Substring(0, 6) + " ");
                            format = line.Substring(6).Split(',');
                            for (int i = 0; i < format.Length; i++)
                            {
                                string f = format[i].Trim();
                                if (i == propertyIndex)
                                    sb.Append(propertyValue);
                                else
                                    sb.Append(f);
                                if (i < format.Length - 1)
                                    sb.Append(",");
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
            Header = sb.ToString().Replace(Environment.NewLine + Environment.NewLine, Environment.NewLine);
        }

        private SsaStyle GetSsaStyle(string styleName)
        {
            var style = new SsaStyle();
            style.Name = styleName;

            int nameIndex = -1;
            int fontNameIndex = -1;
            int fontsizeIndex = -1;
            int primaryColourIndex = -1;
            int secondaryColourIndex = -1;
            int tertiaryColourIndex = -1;
            int outlineColourIndex = -1;
            int backColourIndex = -1;
            int boldIndex = -1;
            int italicIndex = -1;
            int underlineIndex = -1;
            int outlineIndex = -1;
            int shadowIndex = -1;
            int alignmentIndex = -1;
            int marginLIndex = -1;
            int marginRIndex = -1;
            int marginVIndex = -1;
            int borderStyleIndex = -1;

            foreach (string line in Header.Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries))
            {
                string s = line.Trim().ToLower();
                if (s.StartsWith("format:"))
                {
                    if (line.Length > 10)
                    {
                        var format = line.ToLower().Substring(8).Split(',');
                        for (int i = 0; i < format.Length; i++)
                        {
                            string f = format[i].Trim().ToLower();
                            if (f == "name")
                                nameIndex = i;
                            else if (f == "fontname")
                                fontNameIndex = i;
                            else if (f == "fontsize")
                                fontsizeIndex = i;
                            else if (f == "primarycolour")
                                primaryColourIndex = i;
                            else if (f == "secondarycolour")
                                secondaryColourIndex = i;
                            else if (f == "tertiarycolour")
                                tertiaryColourIndex = i;
                            else if (f == "outlinecolour")
                                outlineColourIndex = i;
                            else if (f == "backcolour")
                                backColourIndex = i;
                            else if (f == "bold")
                                boldIndex = i;
                            else if (f == "italic")
                                italicIndex = i;
                            else if (f == "underline")
                                underlineIndex = i;
                            else if (f == "outline")
                                outlineIndex = i;
                            else if (f == "shadow")
                                shadowIndex = i;
                            else if (f == "alignment")
                                alignmentIndex = i;
                            else if (f == "marginl")
                                marginLIndex = i;
                            else if (f == "marginr")
                                marginRIndex = i;
                            else if (f == "marginv")
                                marginVIndex = i;
                            else if (f == "borderstyle")
                                borderStyleIndex = i;
                        }
                    }
                }
                else if (s.Replace(" ", string.Empty).StartsWith("style:"))
                {
                    if (line.Length > 10)
                    {
                        style.RawLine = line;
                        var format = line.Substring(6).Split(',');
                        for (int i = 0; i < format.Length; i++)
                        {
                            string f = format[i].Trim().ToLower();
                            if (i == nameIndex)
                            {
                                style.Name = format[i].Trim();
                            }
                            else if (i == fontNameIndex)
                            {
                                style.FontName = f;
                            }
                            else if (i == fontsizeIndex)
                            {
                                int number;
                                if (int.TryParse(f, out number))
                                    style.FontSize = number;
                            }
                            else if (i == primaryColourIndex)
                            {
                                style.Primary = AdvancedSubStationAlpha.GetSsaColor(f, Color.White);
                            }
                            else if (i == secondaryColourIndex)
                            {
                                style.Secondary = AdvancedSubStationAlpha.GetSsaColor(f, Color.Yellow);
                            }
                            else if (i == tertiaryColourIndex)
                            {
                                style.Tertiary = AdvancedSubStationAlpha.GetSsaColor(f, Color.Yellow);
                            }
                            else if (i == outlineColourIndex)
                            {
                                style.Outline = AdvancedSubStationAlpha.GetSsaColor(f, Color.Black);
                            }
                            else if (i == backColourIndex)
                            {
                                style.Background = AdvancedSubStationAlpha.GetSsaColor(f, Color.Black);
                            }
                            else if (i == boldIndex)
                            {
                                style.Bold = f == "1";
                            }
                            else if (i == italicIndex)
                            {
                                style.Italic = f == "1";
                            }
                            else if (i == underlineIndex)
                            {
                                style.Underline = f == "1";
                            }
                            else if (i == outlineIndex)
                            {
                                int number;
                                if (int.TryParse(f, out number))
                                    style.OutlineWidth = number;
                            }
                            else if (i == shadowIndex)
                            {
                                int number;
                                if (int.TryParse(f, out number))
                                    style.ShadowWidth = number;
                            }
                            else if (i == alignmentIndex)
                            {
                                style.Alignment = f;
                            }
                            else if (i == marginLIndex)
                            {
                                int number;
                                if (int.TryParse(f, out number))
                                    style.MarginLeft = number;
                            }
                            else if (i == marginRIndex)
                            {
                                int number;
                                if (int.TryParse(f, out number))
                                    style.MarginRight = number;
                            }
                            else if (i == marginVIndex)
                            {
                                int number;
                                if (int.TryParse(f, out number))
                                    style.MarginVertical = number;
                            }
                            else if (i == borderStyleIndex)
                            {
                                style.BorderStyle = f;
                            }
                        }
                    }
                    if (styleName != null && style.Name != null && styleName.ToLower() == style.Name.ToLower())
                    {
                        style.LoadedFromHeader = true;
                        return style;
                    }
                }
            }

            return new SsaStyle() { Name = styleName };
        }

        private void ResetHeader()
        {
            SubtitleFormat format;
            if (_isSubStationAlpha)
                format = new SubStationAlpha();
            else
                format = new AdvancedSubStationAlpha();
            var sub = new Subtitle();
            string text = format.ToText(sub, string.Empty);
            string[] lineArray = text.Split(Environment.NewLine.ToCharArray());
            var lines = new List<string>();
            foreach (string line in lineArray)
                lines.Add(line);
            format.LoadSubtitle(sub, lines, string.Empty);
            Header = sub.Header;
        }

        private void SubStationAlphaStyles_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
                DialogResult = DialogResult.Cancel;
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void buttonNextFinish_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
        }

        private void listViewStyles_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listViewStyles.SelectedItems.Count == 1)
            {
                string styleName = listViewStyles.SelectedItems[0].Text;
                _oldSsaName = styleName;
                SsaStyle style = GetSsaStyle(styleName);
                textBoxStyleName.Text = style.Name;
                textBoxStyleName.BackColor = listViewStyles.BackColor;
                comboBoxFontName.Text = style.FontName;
                checkBoxFontItalic.Checked = style.Italic;
                checkBoxFontBold.Checked = style.Bold;
                checkBoxFontUnderline.Checked = style.Underline;
                numericUpDownFontSize.Value = style.FontSize;
                panelPrimaryColor.BackColor = style.Primary;
                panelSecondaryColor.BackColor = style.Secondary;
                if (_isSubStationAlpha)
                    panelOutlineColor.BackColor = style.Tertiary;
                else
                    panelOutlineColor.BackColor = style.Outline;
                panelBackColor.BackColor = style.Background;
                numericUpDownOutline.Value = style.OutlineWidth;
                numericUpDownShadowWidth.Value = style.ShadowWidth;

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

                numericUpDownMarginLeft.Value = style.MarginLeft;
                numericUpDownMarginRight.Value = style.MarginRight;
                numericUpDownMarginVertical.Value = style.MarginVertical;
                if (style.BorderStyle == "3")
                {
                    radioButtonOpaqueBox.Checked = true;
                }
                else
                {
                    radioButtonOutline.Checked = true;
                }
                _doUpdate = true;
                groupBoxProperties.Enabled = true;
                GeneratePreview();
                if (listViewStyles.SelectedItems[0].Index == 0)
                {
                    buttonRemove.Enabled = false;
                    textBoxStyleName.Enabled = false;
                }
                else
                {
                    buttonRemove.Enabled = true;
                    textBoxStyleName.Enabled = true;
                }
            }
            else
            {
                groupBoxProperties.Enabled = false;
                _doUpdate = false;
            }
        }

        private void buttonPrimaryColor_Click(object sender, EventArgs e)
        {
            colorDialogSSAStyle.Color = panelPrimaryColor.BackColor;
            if (colorDialogSSAStyle.ShowDialog() == DialogResult.OK)
            {
                listViewStyles.SelectedItems[0].SubItems[3].BackColor = colorDialogSSAStyle.Color;
                panelPrimaryColor.BackColor = colorDialogSSAStyle.Color;
                string name = listViewStyles.SelectedItems[0].Text;
                SetSsaStyle(name, "primarycolour", GetSsaColorString(colorDialogSSAStyle.Color));
                GeneratePreview();
            }
        }

        private void buttonSecondaryColor_Click(object sender, EventArgs e)
        {
            colorDialogSSAStyle.Color = panelSecondaryColor.BackColor;
            if (colorDialogSSAStyle.ShowDialog() == DialogResult.OK)
            {
                panelSecondaryColor.BackColor = colorDialogSSAStyle.Color;
                string name = listViewStyles.SelectedItems[0].Text;
                SetSsaStyle(name, "secondarycolour", GetSsaColorString(colorDialogSSAStyle.Color));
                GeneratePreview();
            }
        }

        private void buttonOutlineColor_Click(object sender, EventArgs e)
        {
            colorDialogSSAStyle.Color = panelOutlineColor.BackColor;
            if (colorDialogSSAStyle.ShowDialog() == DialogResult.OK)
            {
                if (!_isSubStationAlpha)
                    listViewStyles.SelectedItems[0].SubItems[4].BackColor = colorDialogSSAStyle.Color;
                panelOutlineColor.BackColor = colorDialogSSAStyle.Color;
                string name = listViewStyles.SelectedItems[0].Text;
                if (_isSubStationAlpha)
                    SetSsaStyle(name, "tertiarycolour", GetSsaColorString(colorDialogSSAStyle.Color));
                else
                    SetSsaStyle(name, "outlinecolour", GetSsaColorString(colorDialogSSAStyle.Color));
                GeneratePreview();
            }
        }

        private void buttonShadowColor_Click(object sender, EventArgs e)
        {
            colorDialogSSAStyle.Color = panelBackColor.BackColor;
            if (colorDialogSSAStyle.ShowDialog() == DialogResult.OK)
            {
                if (_isSubStationAlpha)
                    listViewStyles.SelectedItems[0].SubItems[4].BackColor = colorDialogSSAStyle.Color;
                panelBackColor.BackColor = colorDialogSSAStyle.Color;
                string name = listViewStyles.SelectedItems[0].Text;
                SetSsaStyle(name, "backcolour", GetSsaColorString(colorDialogSSAStyle.Color));
                GeneratePreview();
            }
        }

        private string GetSsaColorString(Color c)
        {
            if (_isSubStationAlpha)
                return Color.FromArgb(c.B, c.G, c.R).ToArgb().ToString();
            return string.Format("&H00{0:x2}{1:x2}{2:x2}", c.B, c.G, c.R).ToUpper();
        }

        private void buttonCopy_Click(object sender, EventArgs e)
        {
            if (listViewStyles.SelectedItems.Count == 1)
            {
                string styleName = listViewStyles.SelectedItems[0].Text;
                SsaStyle oldStyle = GetSsaStyle(styleName);
                SsaStyle style = GetSsaStyle(styleName);
                style.Name = string.Format("Copy {0} of {1}", string.Empty, styleName);

                if (GetSsaStyle(style.Name).LoadedFromHeader)
                {
                    int count = 2;
                    bool doRepeat = true;
                    while (doRepeat)
                    {
                        style.Name = string.Format("Copy {0} of {1}", count, styleName);
                        doRepeat = GetSsaStyle(style.Name).LoadedFromHeader;
                        count++;
                    }
                }

                _doUpdate = false;
                AddStyle(style);
                listViewStyles.Items[listViewStyles.Items.Count - 1].Selected = true;
                listViewStyles.Items[listViewStyles.Items.Count - 1].EnsureVisible();
                listViewStyles.Items[listViewStyles.Items.Count - 1].Focused = true;
                textBoxStyleName.Text = style.Name;
                textBoxStyleName.Focus();
                AddStyleToHeader(style, oldStyle);
                _doUpdate = true;
                listViewStyles_SelectedIndexChanged(null, null);
            }
        }

        private void AddStyleToHeader(SsaStyle newStyle, SsaStyle oldStyle)
        {
            if (listViewStyles.SelectedItems.Count == 1)
            {
                string newLine = oldStyle.RawLine;
                newLine = newLine.Replace(oldStyle.Name + ",", newStyle.Name + ",");

                int indexOfEvents = Header.IndexOf("[Events]");
                if (indexOfEvents > 0)
                {
                    int i = indexOfEvents-1;
                    while (i > 0 && Environment.NewLine.Contains(Header[i].ToString()))
                        i--;
                    Header = Header.Insert(i, Environment.NewLine + newLine);
                }
                else
                {
                    Header += Environment.NewLine + newLine;
                }
            }
        }

        private void buttonAdd_Click(object sender, EventArgs e)
        {
            SsaStyle style = GetSsaStyle(Configuration.Settings.Language.SubStationAlphaStyles.New);
            if (GetSsaStyle(style.Name).LoadedFromHeader)
            {
                int count = 2;
                bool doRepeat = true;
                while (doRepeat)
                {
                    style = GetSsaStyle(Configuration.Settings.Language.SubStationAlphaStyles.New + count.ToString());
                    doRepeat = GetSsaStyle(style.Name).LoadedFromHeader;
                    count++;
                }
            }

            _doUpdate = false;
            AddStyle(style);
            listViewStyles.Items[listViewStyles.Items.Count - 1].Selected = true;
            listViewStyles.Items[listViewStyles.Items.Count - 1].EnsureVisible();
            listViewStyles.Items[listViewStyles.Items.Count - 1].Focused = true;
            textBoxStyleName.Text = style.Name;
            textBoxStyleName.Focus();
            SsaStyle oldStyle = GetSsaStyle(listViewStyles.Items[0].Text);
            AddStyleToHeader(style, oldStyle);
            _doUpdate = true;
            listViewStyles_SelectedIndexChanged(null, null);
        }

        private void textBoxStyleName_TextChanged(object sender, EventArgs e)
        {
            if (listViewStyles.SelectedItems.Count == 1)
            {
                if (_doUpdate)
                {
                    if (!GetSsaStyle(textBoxStyleName.Text).LoadedFromHeader)
                    {
                        textBoxStyleName.BackColor = listViewStyles.BackColor;
                        listViewStyles.SelectedItems[0].Text = textBoxStyleName.Text;
                        SetSsaStyle(_oldSsaName, "name", textBoxStyleName.Text);
                        _oldSsaName = textBoxStyleName.Text;
                    }
                    else
                    {
                        textBoxStyleName.BackColor = Color.LightPink;
                    }
                }
            }
        }

        private void buttonRemove_Click(object sender, EventArgs e)
        {
            if (listViewStyles.SelectedItems.Count == 1)
            {
                int index = listViewStyles.SelectedItems[0].Index;
                string name = listViewStyles.SelectedItems[0].Text;
                listViewStyles.Items.RemoveAt(listViewStyles.SelectedItems[0].Index);

                if (listViewStyles.Items.Count == 0)
                {
                    buttonRemoveAll_Click(null, null);
                }
                else
                {
                    if (index >= listViewStyles.Items.Count)
                        index--;
                    listViewStyles.Items[index].Selected = true;
                }
            }
        }

        private void buttonRemoveAll_Click(object sender, EventArgs e)
        {
            listViewStyles.Items.Clear();
            var ass = new AdvancedSubStationAlpha();
            Subtitle sub = new Subtitle();
            var text = ass.ToText(sub, string.Empty);
            string[] lineArray = text.Split(Environment.NewLine.ToCharArray());
            var lines = new List<string>();
            foreach (string line in lineArray)
                lines.Add(line);
            ass.LoadSubtitle(sub, lines, string.Empty);
            Header = sub.Header;
            InitializeListView();
        }

        private void comboBoxFontName_SelectedValueChanged(object sender, EventArgs e)
        {
            if (listViewStyles.SelectedItems.Count == 1 && _doUpdate)
            {
                listViewStyles.SelectedItems[0].SubItems[1].Text = comboBoxFontName.SelectedItem.ToString();
                string name = listViewStyles.SelectedItems[0].Text;
                SetSsaStyle(name, "fontname", comboBoxFontName.SelectedItem.ToString());
                GeneratePreview();
            }
        }

        private void comboBoxFontName_KeyUp(object sender, KeyEventArgs e)
        {
            if (listViewStyles.SelectedItems.Count == 1 && _doUpdate)
            {
                string name = listViewStyles.SelectedItems[0].Text;
                SetSsaStyle(name, "fontname", comboBoxFontName.SelectedItem.ToString());
                GeneratePreview();
            }
        }

        private void numericUpDownFontSize_ValueChanged(object sender, EventArgs e)
        {
            if (listViewStyles.SelectedItems.Count == 1 && _doUpdate)
            {
                listViewStyles.SelectedItems[0].SubItems[2].Text = numericUpDownFontSize.Value.ToString();
                string name = listViewStyles.SelectedItems[0].Text;
                SetSsaStyle(name, "fontsize", numericUpDownFontSize.Value.ToString());
                GeneratePreview();
            }
        }

        private void checkBoxFontBold_CheckedChanged(object sender, EventArgs e)
        {
            if (listViewStyles.SelectedItems.Count == 1 && _doUpdate)
            {
                string name = listViewStyles.SelectedItems[0].Text;
                if (checkBoxFontBold.Checked)
                    SetSsaStyle(name, "bold", "1");
                else
                    SetSsaStyle(name, "bold", "0");
                GeneratePreview();
            }
        }

        private void checkBoxFontItalic_CheckedChanged(object sender, EventArgs e)
        {
            if (listViewStyles.SelectedItems.Count == 1 && _doUpdate)
            {
                string name = listViewStyles.SelectedItems[0].Text;
                if (checkBoxFontItalic.Checked)
                    SetSsaStyle(name, "italic", "1");
                else
                    SetSsaStyle(name, "italic", "0");
                GeneratePreview();
            }
        }

        private void checkBoxUnderline_CheckedChanged(object sender, EventArgs e)
        {
            if (listViewStyles.SelectedItems.Count == 1 && _doUpdate)
            {
                string name = listViewStyles.SelectedItems[0].Text;
                if (checkBoxFontUnderline.Checked)
                    SetSsaStyle(name, "underline", "1");
                else
                    SetSsaStyle(name, "underline", "0");
                GeneratePreview();
            }
        }

        private void radioButtonBottomLeft_CheckedChanged(object sender, EventArgs e)
        {
            if (listViewStyles.SelectedItems.Count == 1 && _doUpdate && (sender as RadioButton).Checked)
            {
                string name = listViewStyles.SelectedItems[0].Text;
                SetSsaStyle(name, "alignment", "1");
                GeneratePreview();
            }
        }

        private void radioButtonBottomCenter_CheckedChanged(object sender, EventArgs e)
        {
            if (listViewStyles.SelectedItems.Count == 1 && _doUpdate && (sender as RadioButton).Checked)
            {
                string name = listViewStyles.SelectedItems[0].Text;
                SetSsaStyle(name, "alignment", "2");
                GeneratePreview();
            }
        }

        private void radioButtonBottomRight_CheckedChanged(object sender, EventArgs e)
        {
            if (listViewStyles.SelectedItems.Count == 1 && _doUpdate && (sender as RadioButton).Checked)
            {
                string name = listViewStyles.SelectedItems[0].Text;
                SetSsaStyle(name, "alignment", "3");
                GeneratePreview();
            }
        }

        private void radioButtonMiddleLeft_CheckedChanged(object sender, EventArgs e)
        {
            if (listViewStyles.SelectedItems.Count == 1 && _doUpdate && (sender as RadioButton).Checked)
            {
                string name = listViewStyles.SelectedItems[0].Text;
                if (_isSubStationAlpha)
                    SetSsaStyle(name, "alignment", "9");
                else
                    SetSsaStyle(name, "alignment", "4");
                GeneratePreview();
            }
        }

        private void radioButtonMiddleCenter_CheckedChanged(object sender, EventArgs e)
        {
            if (listViewStyles.SelectedItems.Count == 1 && _doUpdate && (sender as RadioButton).Checked)
            {
                string name = listViewStyles.SelectedItems[0].Text;
                if (_isSubStationAlpha)
                    SetSsaStyle(name, "alignment", "10");
                else
                    SetSsaStyle(name, "alignment", "5");
                GeneratePreview();
            }
        }

        private void radioButtonMiddleRight_CheckedChanged(object sender, EventArgs e)
        {
            if (listViewStyles.SelectedItems.Count == 1 && _doUpdate && (sender as RadioButton).Checked)
            {
                string name = listViewStyles.SelectedItems[0].Text;
                if (_isSubStationAlpha)
                    SetSsaStyle(name, "alignment", "11");
                else
                    SetSsaStyle(name, "alignment", "6");
                GeneratePreview();
            }
        }

        private void radioButtonTopLeft_CheckedChanged(object sender, EventArgs e)
        {
            if (listViewStyles.SelectedItems.Count == 1 && _doUpdate && (sender as RadioButton).Checked)
            {
                string name = listViewStyles.SelectedItems[0].Text;
                if (_isSubStationAlpha)
                    SetSsaStyle(name, "alignment", "5");
                else
                    SetSsaStyle(name, "alignment", "7");
                GeneratePreview();
            }
        }

        private void radioButtonTopCenter_CheckedChanged(object sender, EventArgs e)
        {
            if (listViewStyles.SelectedItems.Count == 1 && _doUpdate && (sender as RadioButton).Checked)
            {
                string name = listViewStyles.SelectedItems[0].Text;
                if (_isSubStationAlpha)
                    SetSsaStyle(name, "alignment", "6");
                else
                    SetSsaStyle(name, "alignment", "8");
                GeneratePreview();
            }
        }

        private void radioButtonTopRight_CheckedChanged(object sender, EventArgs e)
        {
            if (listViewStyles.SelectedItems.Count == 1 && _doUpdate && (sender as RadioButton).Checked)
            {
                string name = listViewStyles.SelectedItems[0].Text;
                if (_isSubStationAlpha)
                    SetSsaStyle(name, "alignment", "7");
                else
                    SetSsaStyle(name, "alignment", "9");
                GeneratePreview();
            }
        }

        private void numericUpDownMarginLeft_ValueChanged(object sender, EventArgs e)
        {
            if (listViewStyles.SelectedItems.Count == 1 && _doUpdate)
            {
                string name = listViewStyles.SelectedItems[0].Text;
                SetSsaStyle(name, "marginl", numericUpDownMarginLeft.Value.ToString());
                GeneratePreview();
            }
        }

        private void numericUpDownMarginRight_ValueChanged(object sender, EventArgs e)
        {
            if (listViewStyles.SelectedItems.Count == 1 && _doUpdate)
            {
                string name = listViewStyles.SelectedItems[0].Text;
                SetSsaStyle(name, "marginr", numericUpDownMarginRight.Value.ToString());
                GeneratePreview();
            }
        }

        private void numericUpDownMarginVertical_ValueChanged(object sender, EventArgs e)
        {
            if (listViewStyles.SelectedItems.Count == 1 && _doUpdate)
            {
                string name = listViewStyles.SelectedItems[0].Text;
                SetSsaStyle(name, "marginv", numericUpDownMarginVertical.Value.ToString());
                GeneratePreview();
            }
        }

        private void SubStationAlphaStyles_ResizeEnd(object sender, EventArgs e)
        {
            listViewStyles.Columns[5].Width = -2;
            GeneratePreview();
        }

        private void numericUpDownOutline_ValueChanged(object sender, EventArgs e)
        {
            if (listViewStyles.SelectedItems.Count == 1 && _doUpdate)
            {
                string name = listViewStyles.SelectedItems[0].Text;
                SetSsaStyle(name, "outline", numericUpDownOutline.Value.ToString());
                GeneratePreview();
            }
        }

        private void numericUpDownShadowWidth_ValueChanged(object sender, EventArgs e)
        {
            if (listViewStyles.SelectedItems.Count == 1 && _doUpdate)
            {
                string name = listViewStyles.SelectedItems[0].Text;
                SetSsaStyle(name, "shadow", numericUpDownShadowWidth.Value.ToString());
                GeneratePreview();
            }
        }

        private void radioButtonOutline_CheckedChanged(object sender, EventArgs e)
        {
            if (listViewStyles.SelectedItems.Count == 1 && _doUpdate && (sender as RadioButton).Checked)
            {
                numericUpDownShadowWidth.Value = 2;
                string name = listViewStyles.SelectedItems[0].Text;
                SetSsaStyle(name, "outline", numericUpDownOutline.Value.ToString());
                SetSsaStyle(name, "borderstyle", "1");
                GeneratePreview();
            }
            numericUpDownOutline.Enabled = (sender as RadioButton).Checked;
            labelShadow.Enabled = (sender as RadioButton).Checked;
            numericUpDownShadowWidth.Enabled = (sender as RadioButton).Checked;
        }

        private void radioButtonOpaqueBox_CheckedChanged(object sender, EventArgs e)
        {
            if (listViewStyles.SelectedItems.Count == 1 && _doUpdate && (sender as RadioButton).Checked)
            {
                numericUpDownShadowWidth.Value = 0;
                string name = listViewStyles.SelectedItems[0].Text;
                SetSsaStyle(name, "outline", numericUpDownOutline.Value.ToString());
                SetSsaStyle(name, "borderstyle", "3");
                GeneratePreview();
            }
            numericUpDownOutline.Enabled = !(sender as RadioButton).Checked;
            labelShadow.Enabled = !(sender as RadioButton).Checked;
            numericUpDownShadowWidth.Enabled = !(sender as RadioButton).Checked;
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

        private void contextMenuStripStyles_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            copyToolStripMenuItem.Visible = listViewStyles.SelectedItems.Count == 1;
            newToolStripMenuItem.Visible = listViewStyles.SelectedItems.Count == 1;
            removeToolStripMenuItem.Visible = listViewStyles.SelectedItems.Count == 1;
        }

        private void copyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            buttonCopy_Click(null, null);
        }

        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
            buttonAdd_Click(null, null);
        }

        private void removeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            buttonRemove_Click(null, null);
        }

        private void removeAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            buttonRemoveAll_Click(null, null);
        }

        private void SubStationAlphaStyles_SizeChanged(object sender, EventArgs e)
        {
            GeneratePreview();
        }

        private void buttonImport_Click(object sender, EventArgs e)
        {
            openFileDialogImport.Title = "Import style from file...";
            openFileDialogImport.FileName = string.Empty;
            if (_isSubStationAlpha)
                openFileDialogImport.Filter = new SubStationAlpha().FriendlyName + "|*.ssa|" + Configuration.Settings.Language.General.AllFiles + "|*.*";
            else
                openFileDialogImport.Filter = new AdvancedSubStationAlpha().FriendlyName + "|*.ass|" + Configuration.Settings.Language.General.AllFiles + "|*.*";

            if (openFileDialogImport.ShowDialog(this) == DialogResult.OK)
            {
            }
        }

    }
}
