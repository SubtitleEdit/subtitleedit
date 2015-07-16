using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.IO;
using System.Text;
using System.Windows.Forms;
using Nikse.SubtitleEdit.Core;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.SubtitleFormats;

namespace Nikse.SubtitleEdit.Forms.Styles
{
    public partial class SubStationAlphaStyles : StylesForm
    {
        private string _header;
        private bool _doUpdate;
        private string _oldSsaName;
        private readonly SubtitleFormat _format;
        private readonly bool _isSubStationAlpha;

        public SubStationAlphaStyles(Subtitle subtitle, SubtitleFormat format)
            : base(subtitle)
        {
            InitializeComponent();

            labelStatus.Text = string.Empty;
            _header = subtitle.Header;
            _format = format;
            _isSubStationAlpha = _format.Name == SubStationAlpha.NameOfFormat;
            if (_header == null || !_header.Contains("style:", StringComparison.OrdinalIgnoreCase))
                ResetHeader();

            comboBoxFontName.Items.Clear();
            foreach (var x in FontFamily.Families)
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
            groupBoxFont.Text = l.Font;
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
            buttonExport.Text = l.Export;
            buttonCopy.Text = l.Copy;
            buttonAdd.Text = l.New;
            buttonRemove.Text = l.Remove;
            buttonRemoveAll.Text = l.RemoveAll;
            groupBoxPreview.Text = Configuration.Settings.Language.General.Preview;

            if (_isSubStationAlpha)
            {
                Text = l.TitleSubstationAlpha;
                buttonOutlineColor.Text = l.Tertiary;
                buttonBackColor.Text = l.Back;
                listViewStyles.Columns[5].Text = l.Back;
                checkBoxFontUnderline.Visible = false;
            }

            buttonOK.Text = Configuration.Settings.Language.General.Ok;
            buttonCancel.Text = Configuration.Settings.Language.General.Cancel;

            InitializeListView();
            Utilities.FixLargeFonts(this, buttonCancel);

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
            checkBoxFontItalic.Left = checkBoxFontBold.Left + checkBoxFontBold.Width + 12;
            checkBoxFontUnderline.Left = checkBoxFontItalic.Left + checkBoxFontItalic.Width + 12;
        }

        public override string Header
        {
            get
            {
                return _header;
            }
        }

        protected override void GeneratePreviewReal()
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

                var measuredWidth = TextDraw.MeasureTextWidth(font, sb.ToString(), checkBoxFontBold.Checked) + 1;
                var measuredHeight = TextDraw.MeasureTextHeight(font, sb.ToString(), checkBoxFontBold.Checked) + 1;

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
                top -= (int)numericUpDownShadowWidth.Value;
                if (radioButtonTopCenter.Checked || radioButtonMiddleCenter.Checked || radioButtonBottomCenter.Checked)
                    left -= (int)(numericUpDownShadowWidth.Value / 2);

                const int leftMargin = 0;
                int pathPointsStart = -1;

                if (radioButtonOpaqueBox.Checked)
                {
                    if (_isSubStationAlpha)
                        g.FillRectangle(new SolidBrush(panelBackColor.BackColor), left, top, measuredWidth + 3, measuredHeight + 3);
                    else
                        g.FillRectangle(new SolidBrush(panelOutlineColor.BackColor), left, top, measuredWidth + 3, measuredHeight + 3);
                }

                TextDraw.DrawText(font, sf, path, sb, checkBoxFontItalic.Checked, checkBoxFontBold.Checked, checkBoxFontUnderline.Checked, left, top, ref newLine, leftMargin, ref pathPointsStart);

                int outline = (int)numericUpDownOutline.Value;

                // draw shadow
                if (numericUpDownShadowWidth.Value > 0 && radioButtonOutline.Checked)
                {
                    var shadowPath = (GraphicsPath)path.Clone();
                    for (int i = 0; i < (int)numericUpDownShadowWidth.Value; i++)
                    {
                        var translateMatrix = new Matrix();
                        translateMatrix.Translate(1, 1);
                        shadowPath.Transform(translateMatrix);

                        var p1 = new Pen(Color.FromArgb(250, panelBackColor.BackColor), outline);
                        g.DrawPath(p1, shadowPath);
                        p1.Dispose();
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

        private void InitializeListView()
        {
            var styles = AdvancedSubStationAlpha.GetStylesFromHeader(_header);
            listViewStyles.Items.Clear();
            foreach (string style in styles)
            {
                SsaStyle ssaStyle = GetSsaStyle(style);
                AddStyle(listViewStyles, ssaStyle, Subtitle, _isSubStationAlpha);
            }
            if (listViewStyles.Items.Count > 0)
                listViewStyles.Items[0].Selected = true;
        }

        public static void AddStyle(ListView lv, SsaStyle ssaStyle, Subtitle subtitle, bool isSubstationAlpha)
        {
            var item = new ListViewItem(ssaStyle.Name.Trim());
            item.UseItemStyleForSubItems = false;

            var subItem = new ListViewItem.ListViewSubItem(item, ssaStyle.FontName);
            item.SubItems.Add(subItem);

            subItem = new ListViewItem.ListViewSubItem(item, ssaStyle.FontSize.ToString());
            item.SubItems.Add(subItem);

            int count = 0;
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                if (string.IsNullOrEmpty(p.Extra) && ssaStyle.Name.TrimStart('*') == "Default")
                    count++;
                else if (p.Extra != null && ssaStyle.Name.TrimStart('*').Equals(p.Extra.TrimStart('*'), StringComparison.OrdinalIgnoreCase))
                    count++;
            }
            subItem = new ListViewItem.ListViewSubItem(item, count.ToString());
            item.SubItems.Add(subItem);

            subItem = new ListViewItem.ListViewSubItem(item, string.Empty);
            subItem.BackColor = ssaStyle.Primary;
            item.SubItems.Add(subItem);

            subItem = new ListViewItem.ListViewSubItem(item, string.Empty);
            if (isSubstationAlpha)
                subItem.BackColor = ssaStyle.Background;
            else
                subItem.BackColor = ssaStyle.Outline;
            subItem.Text = Configuration.Settings.Language.General.Text;
            subItem.ForeColor = ssaStyle.Primary;
            try
            {
                if (ssaStyle.Bold || ssaStyle.Italic)
                    subItem.Font = new Font(ssaStyle.FontName, subItem.Font.Size, FontStyle.Bold | FontStyle.Italic);
                else if (ssaStyle.Bold)
                    subItem.Font = new Font(ssaStyle.FontName, subItem.Font.Size, FontStyle.Bold);
                else if (ssaStyle.Italic)
                    subItem.Font = new Font(ssaStyle.FontName, subItem.Font.Size, FontStyle.Italic);
                else if (ssaStyle.Italic)
                    subItem.Font = new Font(ssaStyle.FontName, subItem.Font.Size, FontStyle.Regular);
            }
            catch
            {
            }
            item.SubItems.Add(subItem);

            lv.Items.Add(item);
        }

        private void SetSsaStyle(string styleName, string propertyName, string propertyValue)
        {
            int propertyIndex = -1;
            int nameIndex = -1;
            var sb = new StringBuilder();
            foreach (var line in _header.Split(Utilities.NewLineChars, StringSplitOptions.None))
            {
                string s = line.Trim().ToLower();
                if (s.StartsWith("format:", StringComparison.Ordinal))
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
                else if (s.Replace(" ", string.Empty).StartsWith("style:", StringComparison.Ordinal))
                {
                    if (line.Length > 10)
                    {
                        bool correctLine = false;
                        var format = line.Substring(6).Split(',');
                        for (int i = 0; i < format.Length; i++)
                        {
                            string f = format[i].Trim();
                            if (i == nameIndex)
                                correctLine = f.Equals(styleName, StringComparison.OrdinalIgnoreCase);
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
                                    sb.Append(',');
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
        }

        private SsaStyle GetSsaStyle(string styleName)
        {
            return AdvancedSubStationAlpha.GetSsaStyle(styleName, _header);
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
            var lines = new List<string>();
            foreach (string line in text.SplitToLines())
                lines.Add(line);
            format.LoadSubtitle(sub, lines, string.Empty);
            _header = sub.Header;
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
                SetControlsFromStyle(style);
                _doUpdate = true;
                groupBoxProperties.Enabled = true;
                GeneratePreview();
                //if (listViewStyles.SelectedItems[0].Index == 0)
                //{
                //    textBoxStyleName.Enabled = false;
                //}
                //else
                //{
                //    textBoxStyleName.Enabled = true;
                //}
                buttonRemove.Enabled = listViewStyles.Items.Count > 1;
            }
            else
            {
                groupBoxProperties.Enabled = false;
                _doUpdate = false;
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

            if (style.FontSize > 0 && style.FontSize <= numericUpDownFontSize.Maximum)
                numericUpDownFontSize.Value = style.FontSize;
            else
                numericUpDownFontSize.Value = 20;

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

            if (style.MarginLeft >= 0 && style.MarginLeft <= numericUpDownMarginLeft.Maximum)
                numericUpDownMarginLeft.Value = style.MarginLeft;
            else
                numericUpDownMarginLeft.Value = 10;

            if (style.MarginRight >= 0 && style.MarginRight <= numericUpDownMarginRight.Maximum)
                numericUpDownMarginRight.Value = style.MarginRight;
            else
                numericUpDownMarginRight.Value = 10;

            if (style.MarginVertical >= 0 && style.MarginVertical <= numericUpDownMarginVertical.Maximum)
                numericUpDownMarginVertical.Value = style.MarginVertical;
            else
                numericUpDownMarginVertical.Value = 10;

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
            colorDialogSSAStyle.Color = panelPrimaryColor.BackColor;
            if (colorDialogSSAStyle.ShowDialog() == DialogResult.OK)
            {
                listViewStyles.SelectedItems[0].SubItems[4].BackColor = colorDialogSSAStyle.Color;
                listViewStyles.SelectedItems[0].SubItems[5].ForeColor = colorDialogSSAStyle.Color;
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
                return Color.FromArgb(0, c.B, c.G, c.R).ToArgb().ToString();
            return string.Format("&H00{0:x2}{1:x2}{2:x2}", c.B, c.G, c.R).ToUpper();
        }

        private void buttonCopy_Click(object sender, EventArgs e)
        {
            if (listViewStyles.SelectedItems.Count == 1)
            {
                string styleName = listViewStyles.SelectedItems[0].Text;
                SsaStyle oldStyle = GetSsaStyle(styleName);
                SsaStyle style = new SsaStyle(oldStyle); // Copy contructor
                style.Name = string.Format(Configuration.Settings.Language.SubStationAlphaStyles.CopyOfY, styleName);

                if (GetSsaStyle(style.Name).LoadedFromHeader)
                {
                    int count = 2;
                    bool doRepeat = true;
                    while (doRepeat)
                    {
                        style.Name = string.Format(Configuration.Settings.Language.SubStationAlphaStyles.CopyXOfY, count, styleName);
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

                int indexOfEvents = _header.IndexOf("[Events]", StringComparison.Ordinal);
                if (indexOfEvents > 0)
                {
                    int i = indexOfEvents - 1;
                    while (i > 0 && Environment.NewLine.Contains(_header[i]))
                        i--;
                    _header = _header.Insert(i + 1, Environment.NewLine + newLine);
                }
                else
                {
                    _header += Environment.NewLine + newLine;
                }
            }
        }

        private void RemoveStyleFromHeader(string name)
        {
            var sb = new StringBuilder();
            foreach (var line in _header.Split(new[] { Environment.NewLine }, StringSplitOptions.None))
            {
                if (!line.ToLower().Replace(" ", string.Empty).StartsWith("style:" + name.ToLower() + ","))
                    sb.AppendLine(line);
            }
            _header = sb.ToString();
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
                    style = GetSsaStyle(Configuration.Settings.Language.SubStationAlphaStyles.New + count);
                    doRepeat = GetSsaStyle(style.Name).LoadedFromHeader;
                    count++;
                }
            }

            _doUpdate = false;
            AddStyle(listViewStyles, style, Subtitle, _isSubStationAlpha);
            SsaStyle oldStyle = GetSsaStyle(listViewStyles.Items[0].Text);
            AddStyleToHeader(style, oldStyle);
            listViewStyles.Items[listViewStyles.Items.Count - 1].Selected = true;
            listViewStyles.Items[listViewStyles.Items.Count - 1].EnsureVisible();
            listViewStyles.Items[listViewStyles.Items.Count - 1].Focused = true;
            textBoxStyleName.Text = style.Name;
            textBoxStyleName.Focus();
            _doUpdate = true;
            SetControlsFromStyle(style);
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
                RemoveStyleFromHeader(name);

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
            var sub = new Subtitle();
            if (_isSubStationAlpha)
            {
                var ssa = new SubStationAlpha();
                string text = ssa.ToText(sub, string.Empty);
                string[] lineArray = text.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
                var lines = new List<string>();
                foreach (string line in lineArray)
                    lines.Add(line);
                ssa.LoadSubtitle(sub, lines, string.Empty);
                _header = _header.Remove(_header.IndexOf("[V4 Styles]", StringComparison.Ordinal)) + sub.Header.Substring(sub.Header.IndexOf("[V4 Styles]", StringComparison.Ordinal));
            }
            else
            {
                var ass = new AdvancedSubStationAlpha();
                string text = ass.ToText(sub, string.Empty);
                string[] lineArray = text.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
                var lines = new List<string>();
                foreach (string line in lineArray)
                    lines.Add(line);
                ass.LoadSubtitle(sub, lines, string.Empty);
                _header = _header.Remove(_header.IndexOf("[V4+ Styles]", StringComparison.Ordinal)) + sub.Header.Substring(sub.Header.IndexOf("[V4+ Styles]", StringComparison.Ordinal));
            }
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
                var item = comboBoxFontName.SelectedItem;
                if (item != null)
                    SetSsaStyle(name, "fontname", item.ToString());
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
            var rb = (sender as RadioButton);
            if (rb != null && listViewStyles.SelectedItems.Count == 1 && _doUpdate && rb.Checked)
            {
                numericUpDownShadowWidth.Value = 2;
                string name = listViewStyles.SelectedItems[0].Text;
                SetSsaStyle(name, "outline", numericUpDownOutline.Value.ToString());
                SetSsaStyle(name, "borderstyle", "1");
                GeneratePreview();

                numericUpDownOutline.Enabled = rb.Checked;
                labelShadow.Enabled = rb.Checked;
                numericUpDownShadowWidth.Enabled = rb.Checked;
            }
        }

        private void radioButtonOpaqueBox_CheckedChanged(object sender, EventArgs e)
        {
            var rb = (sender as RadioButton);
            if (rb != null && listViewStyles.SelectedItems.Count == 1 && _doUpdate && rb.Checked)
            {
                numericUpDownShadowWidth.Value = 0;
                string name = listViewStyles.SelectedItems[0].Text;
                SetSsaStyle(name, "outline", numericUpDownOutline.Value.ToString());
                SetSsaStyle(name, "borderstyle", "3");
                GeneratePreview();

                numericUpDownOutline.Enabled = !rb.Checked;
                labelShadow.Enabled = !rb.Checked;
                numericUpDownShadowWidth.Enabled = !rb.Checked;
            }
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
            openFileDialogImport.Title = Configuration.Settings.Language.SubStationAlphaStyles.ImportStyleFromFile;
            openFileDialogImport.InitialDirectory = Configuration.DataDirectory;
            if (_isSubStationAlpha)
            {
                openFileDialogImport.Filter = SubStationAlpha.NameOfFormat + "|*.ssa|" + Configuration.Settings.Language.General.AllFiles + "|*.*";
                saveFileDialogStyle.FileName = "my_styles.ssa";
            }
            else
            {
                openFileDialogImport.Filter = AdvancedSubStationAlpha.NameOfFormat + "|*.ass|" + Configuration.Settings.Language.General.AllFiles + "|*.*";
                saveFileDialogStyle.FileName = "my_styles.ass";
            }

            if (openFileDialogImport.ShowDialog(this) == DialogResult.OK)
            {
                Encoding encoding = null;
                var s = new Subtitle();
                var format = s.LoadSubtitle(openFileDialogImport.FileName, out encoding, null);
                if (format != null && format.HasStyleSupport)
                {
                    var styles = AdvancedSubStationAlpha.GetStylesFromHeader(s.Header);
                    if (styles.Count > 0)
                    {
                        var cs = new ChooseStyle(s, format.GetType() == typeof(SubStationAlpha));
                        if (cs.ShowDialog(this) == DialogResult.OK && cs.SelectedStyleName != null)
                        {
                            SsaStyle style = AdvancedSubStationAlpha.GetSsaStyle(cs.SelectedStyleName, s.Header);
                            if (GetSsaStyle(style.Name).LoadedFromHeader)
                            {
                                int count = 2;
                                bool doRepeat = GetSsaStyle(style.Name + count).LoadedFromHeader;
                                while (doRepeat)
                                {
                                    doRepeat = GetSsaStyle(style.Name + count).LoadedFromHeader;
                                    count++;
                                }
                                style.RawLine = style.RawLine.Replace(" " + style.Name + ",", " " + style.Name + count + ",");
                                style.Name = style.Name + count;
                            }

                            _doUpdate = false;
                            AddStyle(listViewStyles, style, Subtitle, _isSubStationAlpha);
                            SsaStyle oldStyle = GetSsaStyle(listViewStyles.Items[0].Text);
                            _header = _header.Trim();
                            if (_header.EndsWith("[Events]"))
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

                            labelStatus.Text = string.Format(Configuration.Settings.Language.SubStationAlphaStyles.StyleXImportedFromFileY, style.Name, openFileDialogImport.FileName);
                            timerClearStatus.Start();
                        }
                    }
                }
            }
        }

        internal void MakeOnlyOneStyle()
        {
            groupBoxPreview.Top = groupBoxStyles.Top;
            groupBoxPreview.Height = groupBoxProperties.Height;
            groupBoxStyles.SendToBack();
        }

        private void buttonExport_Click(object sender, EventArgs e)
        {
            if (listViewStyles.SelectedItems.Count != 1)
                return;

            string styleName = listViewStyles.SelectedItems[0].Text;

            saveFileDialogStyle.Title = Configuration.Settings.Language.SubStationAlphaStyles.ExportStyleToFile;
            saveFileDialogStyle.InitialDirectory = Configuration.DataDirectory;
            if (_isSubStationAlpha)
            {
                saveFileDialogStyle.Filter = SubStationAlpha.NameOfFormat + "|*.ssa|" + Configuration.Settings.Language.General.AllFiles + "|*.*";
                saveFileDialogStyle.FileName = "my_styles.ssa";
            }
            else
            {
                saveFileDialogStyle.Filter = AdvancedSubStationAlpha.NameOfFormat + "|*.ass|" + Configuration.Settings.Language.General.AllFiles + "|*.*";
                saveFileDialogStyle.FileName = "my_styles.ass";
            }

            if (saveFileDialogStyle.ShowDialog(this) == DialogResult.OK)
            {
                if (File.Exists(saveFileDialogStyle.FileName))
                {
                    Encoding encoding = null;
                    var s = new Subtitle();
                    var format = s.LoadSubtitle(saveFileDialogStyle.FileName, out encoding, null);
                    if (format == null)
                    {
                        MessageBox.Show("Not subtitle format: " + _format.Name);
                        return;
                    }
                    else if (format.Name != _format.Name)
                    {
                        MessageBox.Show(string.Format("Cannot save {1} style in {0} file!", format.Name, _format.Name));
                        return;
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
                                SsaStyle style = GetSsaStyle(styleName);
                                if (_isSubStationAlpha)
                                {
                                    sb.AppendLine(style.ToRawSsa(styleFormat));
                                }
                                else
                                {
                                    sb.AppendLine(style.ToRawAss(styleFormat));
                                }
                            }
                            sb.AppendLine(line);
                            if (stylesOn && line.Replace(" ", string.Empty).TrimStart().StartsWith("style:" + styleName.Replace(" ", string.Empty).Trim() + ",", StringComparison.OrdinalIgnoreCase))
                            {
                                MessageBox.Show(string.Format(Configuration.Settings.Language.SubStationAlphaStyles.StyleAlreadyExits, styleName));
                                return;
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
                            if (line.ToLower().Replace(" ", string.Empty).StartsWith("style:" + styleName.ToLower().Trim()))
                                sb.AppendLine(line);
                        }
                        else
                        {
                            sb.AppendLine(line);
                        }
                    }
                    string content = sb.ToString();
                    if (content.TrimEnd().EndsWith("[Events]"))
                    {
                        content = content.Trim() + Environment.NewLine +
                            "Format: Layer, Start, End, Style, Actor, MarginL, MarginR, MarginV, Effect, Text" + Environment.NewLine +
                            "Dialogue: 0,0:00:31.91,0:00:33.91,Default,,0,0,0,,My Styles :)";
                    }
                    File.WriteAllText(saveFileDialogStyle.FileName, content, Encoding.UTF8);
                }
            }
            labelStatus.Text = string.Format(Configuration.Settings.Language.SubStationAlphaStyles.StyleXExportedToFileY, styleName, saveFileDialogStyle.FileName);
            timerClearStatus.Start();
        }

        private void timerClearStatus_Tick(object sender, EventArgs e)
        {
            timerClearStatus.Stop();
            labelStatus.Text = string.Empty;
        }

    }
}