using System;
using System.Drawing;
using System.Windows.Forms;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.SubtitleFormats;
using System.Collections.Generic;
using System.Text;
using System.Drawing.Text;
using System.Drawing.Drawing2D;

namespace Nikse.SubtitleEdit.Forms
{

    public partial class SubStationAlphaStyles : Form
    {
        public string Header { get; private set; }
        private Subtitle _subtitle = null;
        private bool _doUpdate = false;
        private string _oldSsaName = null;
        private Timer _previewTimer = new Timer();

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
            public Color Outline { get; set; }
            public Color Background { get; set; }
            public int ShadowWidth { get; set; }
            public int OutlineWidth { get; set; }
            public string Alignment { get; set; }
            public int MarginLeft { get; set; }
            public int MarginRight { get; set; }
            public int MarginVertical { get; set; }

            public SsaStyle()
            {
                FontName = "Verdena";
                FontSize = 22;
                Primary = Color.White;
                Secondary = Color.Yellow;
                Outline = Color.Black;
                Background = Color.Black;
                Alignment = "2";
                OutlineWidth = 1;
                ShadowWidth = 1;
                MarginLeft = 1;
                MarginRight = 1;
                MarginVertical = 1;
                OutlineWidth = 1;
                ShadowWidth = 1;
            }
        }

        public SubStationAlphaStyles(Subtitle subtitle)
        {
            InitializeComponent();

            Header = subtitle.Header;
            if (Header == null || !Header.ToLower().Contains("style:"))
                ResetHeader();

            _subtitle = subtitle;

            comboBoxFontName.Items.Clear();
            foreach (var x in System.Drawing.FontFamily.Families)
                comboBoxFontName.Items.Add(x.Name);

            InitializeListView();
            FixLargeFonts();
            _previewTimer.Interval = 400;
            _previewTimer.Tick += RefreshTimerTick;
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
                        g.FillRectangle(new SolidBrush(c), x, y, 15, 15);
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

                float left = 5;
                if (radioButtonTopLeft.Checked || radioButtonMiddleLeft.Checked || radioButtonBottomLeft.Checked)
                    left = 5;
                else if (radioButtonTopRight.Checked || radioButtonMiddleRight.Checked || radioButtonBottomRight.Checked)
                    left = bmp.Width - (TextDraw.MeasureTextWidth(font, sb.ToString(), checkBoxFontBold.Checked) + 15);
                else
                    left = ((float)(bmp.Width - g.MeasureString(sb.ToString(), font).Width * 0.8 + 15) / 2);


                float top = 2;
                if (radioButtonTopLeft.Checked || radioButtonTopCenter.Checked || radioButtonTopRight.Checked)
                    top = 5;
                else if (radioButtonMiddleLeft.Checked || radioButtonMiddleCenter.Checked || radioButtonMiddleRight.Checked)
                    top = (bmp.Height - (g.MeasureString(sb.ToString(), font).Height)) / 2;
                else
                    top = bmp.Height - (g.MeasureString(sb.ToString(), font).Height) + 2;

                int addX = 0;
                int leftMargin = 0;
                int pathPointsStart = -1;
                TextDraw.DrawText(font, sf, path, sb, checkBoxFontItalic.Checked, checkBoxFontBold.Checked, checkBoxFontUnderline.Checked, left, top, ref newLine, addX, leftMargin, ref pathPointsStart);

                int outline = (int)numericUpDownOutline.Value;
                if (outline > 0)
                    g.DrawPath(new Pen(panelOutlineColor.BackColor, outline), path);
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

            subItem = new ListViewItem.ListViewSubItem(item, string.Empty);
            subItem.BackColor = ssaStyle.Primary;
            item.SubItems.Add(subItem);

            subItem = new ListViewItem.ListViewSubItem(item, string.Empty);
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
                            else if (f == propertyName)
                                propertyIndex = i;
                        }
                    }
                    sb.AppendLine(line);
                }
                else if (s.Replace(" ", string.Empty).StartsWith("style:" + styleName.ToLower()))
                {
                    if (line.Length > 10)
                    {
                        sb.Append(line.Substring(0, 6) + " ");
                        var format = line.Substring(6).Split(',');
                        for (int i = 0; i < format.Length; i++)
                        {
                            string f = format[i].Trim();
                            if (i == propertyIndex)
                                sb.Append(propertyValue);
                            else
                                sb.Append(f);
                            if (i < format.Length - 2)
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
            Header = sb.ToString().Replace(Environment.NewLine + Environment.NewLine, Environment.NewLine);
        }

        private SsaStyle GetSsaStyle(string styleName)
        {
            SsaStyle style = new SsaStyle();
            style.Name = styleName;

            int nameIndex = -1;
            int fontNameIndex = -1;
            int fontsizeIndex = -1;
            int primaryColourIndex = -1;
            int secondaryColourIndex = -1;
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
                        }
                    }
                }
                else if (s.Replace(" ", string.Empty).StartsWith("style:" + styleName.ToLower()))
                {
                    if (line.Length > 10)
                    {
                        var format = line.ToLower().Substring(6).Split(',');
                        for (int i = 0; i < format.Length; i++)
                        {
                            string f = format[i].Trim();
                            if (i == fontNameIndex)
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
                                style.Primary = GetSsaColor(f, Color.White);
                            }
                            else if (i == secondaryColourIndex)
                            {
                                style.Secondary = GetSsaColor(f, Color.Yellow);
                            }
                            else if (i == outlineColourIndex)
                            {
                                style.Outline = GetSsaColor(f, Color.Black);
                            }
                            else if (i == backColourIndex)
                            {
                                style.Background = GetSsaColor(f, Color.Black);
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
                        }
                    }
                }
            }

            return style;
        }

        private void ResetHeader()
        {
            AdvancedSubStationAlpha ass = new AdvancedSubStationAlpha();
            Subtitle sub = new Subtitle();
            string text = ass.ToText(sub, string.Empty);
            string[] lineArray = text.Split(Environment.NewLine.ToCharArray());
            var lines = new List<string>();
            foreach (string line in lineArray)
                lines.Add(line);
            ass.LoadSubtitle(sub, lines, string.Empty);
            Header = sub.Header;
        }

        /// <summary>
        /// BGR color like this: &HBBGGRR& (where BB, GG, and RR are hex values in uppercase)
        /// </summary>
        /// <param name="f">Input string</param>
        /// <param name="defaultColor">Default color</param>
        /// <returns>Input string as color, or default color if problems</returns>
        private Color GetSsaColor(string f, Color defaultColor)
        {
            //Red = &H0000FF&
            //Green = &H00FF00&
            //Blue = &HFF0000&
            //White = &HFFFFFF&
            //Black = &H000000&
            string s = f.Trim().Trim('&');
            if (s.ToLower().StartsWith("h") && s.Length == 7)
            {
                s = s.Substring(1);
                string hexColor = "#" + s.Substring(4, 2) + s.Substring(2, 2) + s.Substring(0, 2);
                try
                {
                    return System.Drawing.ColorTranslator.FromHtml(hexColor);
                }
                catch
                {
                    return defaultColor;
                }
            }
            else if (s.ToLower().StartsWith("h") && s.Length == 9)
            {
                s = s.Substring(3);
                string hexColor = "#" + s.Substring(4, 2) + s.Substring(2, 2) + s.Substring(0, 2);
                try
                {
                    var c = System.Drawing.ColorTranslator.FromHtml(hexColor);

                    return c;
                }
                catch
                {
                    return defaultColor;
                }
            }
            return defaultColor;
        }

        private string GetSsaColorString(Color c)
        {
            return string.Format("&H00{0:x2}{1:x2}{2:x2}&", c.B, c.G, c.R).ToUpper();
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
            DialogResult = DialogResult.Cancel;
        }

        private void listViewStyles_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listViewStyles.SelectedItems.Count == 1)
            {
                string styleName = listViewStyles.SelectedItems[0].Text;
                _oldSsaName = styleName;
                SsaStyle style = GetSsaStyle(styleName);
                textBoxStyleName.Text = style.Name;
                comboBoxFontName.Text = style.FontName;
                checkBoxFontItalic.Checked = style.Italic;
                checkBoxFontBold.Checked = style.Bold;
                numericUpDownFontSize.Value = style.FontSize;
                panelPrimaryColor.BackColor = style.Primary;
                panelSecondaryColor.BackColor = style.Secondary;
                panelOutlineColor.BackColor = style.Outline;
                panelBackColor.BackColor = style.Background;
                numericUpDownOutline.Value = style.OutlineWidth;
                numericUpDownShadowWidth.Value = style.ShadowWidth;

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

                numericUpDownMarginLeft.Value = style.MarginLeft;
                numericUpDownMarginRight.Value = style.MarginRight;
                numericUpDownMarginVertical.Value = style.MarginVertical;
                _doUpdate = true;
                groupBoxProperties.Enabled = true;
                GeneratePreview();
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
                panelPrimaryColor.BackColor = colorDialogSSAStyle.Color;
                string name = listViewStyles.SelectedItems[0].Text;
                SetSsaStyle(name, "primarycolor", GetSsaColorString(colorDialogSSAStyle.Color));
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
                SetSsaStyle(name, "secondarycolor", GetSsaColorString(colorDialogSSAStyle.Color));
                GeneratePreview();
            }
        }

        private void buttonOutlineColor_Click(object sender, EventArgs e)
        {
            colorDialogSSAStyle.Color = panelOutlineColor.BackColor;
            if (colorDialogSSAStyle.ShowDialog() == DialogResult.OK)
            {
                panelOutlineColor.BackColor = colorDialogSSAStyle.Color;
                string name = listViewStyles.SelectedItems[0].Text;
                SetSsaStyle(name, "outlinecolor", GetSsaColorString(colorDialogSSAStyle.Color));
                GeneratePreview();
            }
        }

        private void buttonShadowColor_Click(object sender, EventArgs e)
        {
            colorDialogSSAStyle.Color = panelBackColor.BackColor;
            if (colorDialogSSAStyle.ShowDialog() == DialogResult.OK)
            {
                panelBackColor.BackColor = colorDialogSSAStyle.Color;
                string name = listViewStyles.SelectedItems[0].Text;
                SetSsaStyle(name, "backcolor", GetSsaColorString(colorDialogSSAStyle.Color));
            }
        }

        private void buttonCopy_Click(object sender, EventArgs e)
        {
            if (listViewStyles.SelectedItems.Count == 1)
            {
                string styleName = listViewStyles.SelectedItems[0].Text;
                SsaStyle style = GetSsaStyle(styleName);
                style.Name = "New";
                AddStyle(style);
                listViewStyles.Items[listViewStyles.Items.Count - 1].Selected = true;
                listViewStyles.Items[listViewStyles.Items.Count - 1].EnsureVisible();
                listViewStyles.Items[listViewStyles.Items.Count - 1].Focused = true;
                textBoxStyleName.Focus();
            }
        }

        private void buttonAdd_Click(object sender, EventArgs e)
        {
            SsaStyle style = GetSsaStyle("New");
            style.Name = string.Empty;
            AddStyle(style);
            listViewStyles.Items[listViewStyles.Items.Count - 1].Selected = true;
            listViewStyles.Items[listViewStyles.Items.Count - 1].EnsureVisible();
            listViewStyles.Items[listViewStyles.Items.Count - 1].Focused = true;
            textBoxStyleName.Focus();
        }

        private void textBoxStyleName_TextChanged(object sender, EventArgs e)
        { //todo: check duplicate name!!!
            if (listViewStyles.SelectedItems.Count == 1)
            {
                listViewStyles.SelectedItems[0].Text = textBoxStyleName.Text;
                if (_doUpdate)
                {
                    SetSsaStyle(_oldSsaName, "name", textBoxStyleName.Text);
                    _oldSsaName = textBoxStyleName.Text;
                }
            }
        }

        private void buttonRemove_Click(object sender, EventArgs e)
        {
            if (listViewStyles.SelectedItems.Count == 1)
            {
                string name = listViewStyles.SelectedItems[0].Text;
                listViewStyles.Items.RemoveAt(listViewStyles.SelectedItems[0].Index);
            }
        }

        private void buttonRemoveAll_Click(object sender, EventArgs e)
        {
            listViewStyles.Items.Clear();
            AdvancedSubStationAlpha ass = new AdvancedSubStationAlpha();
            Subtitle sub = new Subtitle();
            string text = ass.ToText(sub, string.Empty);
            string[] lineArray = text.Split(Environment.NewLine.ToCharArray());
            var lines = new List<string>();
            foreach (string line in lineArray)
                lines.Add(line);
            ass.LoadSubtitle(sub, lines, string.Empty);
            _subtitle.Header = sub.Header;
            InitializeListView();
        }

        private void comboBoxFontName_SelectedValueChanged(object sender, EventArgs e)
        {
            if (listViewStyles.SelectedItems.Count == 1 && _doUpdate)
            {
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
            if (listViewStyles.SelectedItems.Count == 1 && _doUpdate)
            {
                string name = listViewStyles.SelectedItems[0].Text;
                SetSsaStyle(name, "alignment", "1");
                GeneratePreview();
            }
        }

        private void radioButtonBottomCenter_CheckedChanged(object sender, EventArgs e)
        {
            if (listViewStyles.SelectedItems.Count == 1 && _doUpdate)
            {
                string name = listViewStyles.SelectedItems[0].Text;
                SetSsaStyle(name, "alignment", "2");
                GeneratePreview();
            }
        }

        private void radioButtonBottomRight_CheckedChanged(object sender, EventArgs e)
        {
            if (listViewStyles.SelectedItems.Count == 1 && _doUpdate)
            {
                string name = listViewStyles.SelectedItems[0].Text;
                SetSsaStyle(name, "alignment", "3");
                GeneratePreview();
            }
        }

        private void radioButtonMiddleLeft_CheckedChanged(object sender, EventArgs e)
        {
            if (listViewStyles.SelectedItems.Count == 1 && _doUpdate)
            {
                string name = listViewStyles.SelectedItems[0].Text;
                SetSsaStyle(name, "alignment", "4");
                GeneratePreview();
            }
        }

        private void radioButtonMiddleCenter_CheckedChanged(object sender, EventArgs e)
        {
            if (listViewStyles.SelectedItems.Count == 1 && _doUpdate)
            {
                string name = listViewStyles.SelectedItems[0].Text;
                SetSsaStyle(name, "alignment", "5");
                GeneratePreview();
            }
        }

        private void radioButtonMiddleRight_CheckedChanged(object sender, EventArgs e)
        {
            if (listViewStyles.SelectedItems.Count == 1 && _doUpdate)
            {
                string name = listViewStyles.SelectedItems[0].Text;
                SetSsaStyle(name, "alignment", "6");
                GeneratePreview();
            }
        }

        private void radioButtonTopLeft_CheckedChanged(object sender, EventArgs e)
        {
            if (listViewStyles.SelectedItems.Count == 1 && _doUpdate)
            {
                string name = listViewStyles.SelectedItems[0].Text;
                SetSsaStyle(name, "alignment", "7");
                GeneratePreview();
            }
        }

        private void radioButtonTopCenter_CheckedChanged(object sender, EventArgs e)
        {
            if (listViewStyles.SelectedItems.Count == 1 && _doUpdate)
            {
                string name = listViewStyles.SelectedItems[0].Text;
                SetSsaStyle(name, "alignment", "8");
                GeneratePreview();
            }
        }

        private void radioButtonTopRight_CheckedChanged(object sender, EventArgs e)
        {
            if (listViewStyles.SelectedItems.Count == 1 && _doUpdate)
            {
                string name = listViewStyles.SelectedItems[0].Text;
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
                SetSsaStyle(name, "marginr", numericUpDownMarginLeft.Value.ToString());
                GeneratePreview();
            }
        }

        private void numericUpDownMarginVertical_ValueChanged(object sender, EventArgs e)
        {
            if (listViewStyles.SelectedItems.Count == 1 && _doUpdate)
            {
                string name = listViewStyles.SelectedItems[0].Text;
                SetSsaStyle(name, "marginv", numericUpDownMarginLeft.Value.ToString());
                GeneratePreview();
            }
        }

        private void SubStationAlphaStyles_ResizeEnd(object sender, EventArgs e)
        {
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

    }
}
