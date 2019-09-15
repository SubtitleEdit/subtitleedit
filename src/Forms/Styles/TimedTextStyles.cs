using Nikse.SubtitleEdit.Core;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Logic;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Text;
using System.Windows.Forms;
using System.Xml;

namespace Nikse.SubtitleEdit.Forms.Styles
{
    public sealed partial class TimedTextStyles : StylesForm
    {
        private readonly XmlDocument _xml;
        private readonly XmlNode _xmlHead;
        private readonly XmlNamespaceManager _nsmgr;
        private bool _doUpdate;

        public TimedTextStyles(Subtitle subtitle)
            : base(subtitle)
        {
            UiUtil.PreInitialize(this);
            InitializeComponent();
            UiUtil.FixFonts(this);

            _xml = new XmlDocument();
            try
            {
                _xml.LoadXml(subtitle.Header);
                var xnsmgr = new XmlNamespaceManager(_xml.NameTable);
                xnsmgr.AddNamespace("ttml", "http://www.w3.org/ns/ttml");
                if (_xml.DocumentElement.SelectSingleNode("ttml:head", xnsmgr) == null)
                {
                    _xml.LoadXml(new TimedText10().ToText(new Subtitle(), "tt")); // load default xml
                }
            }
            catch
            {
                if (subtitle.Header != null && (subtitle.Header.Contains("[V4+ Styles]") || subtitle.Header.Contains("[V4 Styles]")))
                {
                    subtitle.Header = TimedText10.SubStationAlphaHeaderToTimedText(subtitle); // save new header with TTML styles (converted from ssa/ass)
                }

                try
                {
                    _xml.LoadXml(subtitle.Header);
                    var xnsmgr = new XmlNamespaceManager(_xml.NameTable);
                    xnsmgr.AddNamespace("ttml", "http://www.w3.org/ns/ttml");
                    if (_xml.DocumentElement.SelectSingleNode("ttml:head", xnsmgr) == null)
                    {
                        _xml.LoadXml(new TimedText10().ToText(new Subtitle(), "tt")); // load default xml
                    }
                }
                catch
                {
                    _xml.LoadXml(new TimedText10().ToText(new Subtitle(), "tt")); // load default xml
                }
            }
            _nsmgr = new XmlNamespaceManager(_xml.NameTable);
            _nsmgr.AddNamespace("ttml", "http://www.w3.org/ns/ttml");
            _xmlHead = _xml.DocumentElement.SelectSingleNode("ttml:head", _nsmgr);

            foreach (var ff in FontFamily.Families)
            {
                if (ff.Name.Length > 0)
                {
                    comboBoxFontName.Items.Add(char.ToLower(ff.Name[0]) + ff.Name.Substring(1));
                }
            }

            InitializeListView();
        }

        public override string Header => _xml.OuterXml;

        protected override void GeneratePreviewReal()
        {
            if (listViewStyles.SelectedItems.Count != 1)
            {
                return;
            }

            pictureBoxPreview.Image?.Dispose();
            var bmp = new Bitmap(pictureBoxPreview.Width, pictureBoxPreview.Height);

            using (Graphics g = Graphics.FromImage(bmp))
            {
                // Draw background
                const int rectangleSize = 9;
                for (int y = 0; y < bmp.Height; y += rectangleSize)
                {
                    for (int x = 0; x < bmp.Width; x += rectangleSize)
                    {
                        Color c = Color.DarkGray;
                        if (y % (rectangleSize * 2) == 0)
                        {
                            if (x % (rectangleSize * 2) == 0)
                            {
                                c = Color.LightGray;
                            }
                        }
                        else
                        {
                            if (x % (rectangleSize * 2) != 0)
                            {
                                c = Color.LightGray;
                            }
                        }
                        g.FillRectangle(new SolidBrush(c), x, y, rectangleSize, rectangleSize);
                    }
                }

                // Draw text
                Font font;
                try
                {
                    var fontSize = 20.0f;
                    if (int.TryParse(textBoxFontSize.Text.Replace("px", string.Empty), out var fontSizeInt))
                    {
                        fontSize = fontSizeInt;
                    }
                    else if (textBoxFontSize.Text.EndsWith('%'))
                    {
                        if (int.TryParse(textBoxFontSize.Text.TrimEnd('%'), out fontSizeInt))
                        {
                            fontSize *= fontSizeInt / 100.0f;
                        }
                    }
                    font = new Font(comboBoxFontName.Text, fontSize);
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
                sb.Append(Configuration.Settings.General.PreviewAssaText);
                const float left = 5f;
                const float top = 2f;
                const int leftMargin = 0;
                int pathPointsStart = -1;
                TextDraw.DrawText(font, sf, path, sb, comboBoxFontStyle.Text == "italic", comboBoxFontWeight.Text == "bold", false, left, top, ref newLine, leftMargin, ref pathPointsStart);
                g.FillPath(new SolidBrush(panelFontColor.BackColor), path);
            }
            pictureBoxPreview.Image = bmp;
        }

        private void InitializeListView()
        {
            XmlNode head = _xml.DocumentElement.SelectSingleNode("ttml:head", _nsmgr);
            if (head == null)
            {
                return;
            }

            XmlNode styling = head.SelectSingleNode("ttml:styling", _nsmgr);
            if (styling == null)
            {
                return;
            }

            foreach (XmlNode node in styling.SelectNodes("ttml:style", _nsmgr))
            {
                string name = "default";
                if (node.Attributes["xml:id"] != null)
                {
                    name = node.Attributes["xml:id"].Value;
                }
                else if (node.Attributes["id"] != null)
                {
                    name = node.Attributes["id"].Value;
                }

                string fontFamily = "Arial";
                if (node.Attributes["tts:fontFamily"] != null)
                {
                    fontFamily = node.Attributes["tts:fontFamily"].Value;
                }

                string fontWeight = "normal";
                if (node.Attributes["tts:fontWeight"] != null)
                {
                    fontWeight = node.Attributes["tts:fontWeight"].Value;
                }

                string fontStyle = "normal";
                if (node.Attributes["tts:fontStyle"] != null)
                {
                    fontStyle = node.Attributes["tts:fontStyle"].Value;
                }

                string fontColor = "white";
                if (node.Attributes["tts:color"] != null)
                {
                    fontColor = node.Attributes["tts:color"].Value;
                }

                string fontSize = "100%";
                if (node.Attributes["tts:fontSize"] != null)
                {
                    fontSize = node.Attributes["tts:fontSize"].Value;
                }

                AddStyle(name, fontFamily, fontColor, fontSize);
            }
            if (listViewStyles.Items.Count > 0)
            {
                listViewStyles.Items[0].Selected = true;
            }
        }

        private void AddStyle(string name, string fontFamily, string color, string fontSize)
        {
            var item = new ListViewItem(name.Trim());
            item.UseItemStyleForSubItems = false;

            var subItem = new ListViewItem.ListViewSubItem(item, fontFamily);
            item.SubItems.Add(subItem);

            subItem = new ListViewItem.ListViewSubItem(item, fontSize);
            item.SubItems.Add(subItem);

            subItem = new ListViewItem.ListViewSubItem(item, string.Empty);
            subItem.Text = color;
            Color c = Color.White;
            try
            {
                if (color.StartsWith("rgb(", StringComparison.Ordinal))
                {
                    string[] arr = color.Remove(0, 4).TrimEnd(')').Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    c = Color.FromArgb(int.Parse(arr[0]), int.Parse(arr[1]), int.Parse(arr[2]));
                }
                else
                {
                    c = ColorTranslator.FromHtml(color);
                }
            }
            catch
            {
                // ignored
            }

            subItem.BackColor = c;
            item.SubItems.Add(subItem);

            int count = 0;
            foreach (var p in Subtitle.Paragraphs)
            {
                if (string.IsNullOrEmpty(p.Extra) && name.Trim() == "Default" ||
                    p.Extra != null && name.Trim().Equals(p.Extra.TrimStart(), StringComparison.OrdinalIgnoreCase))
                {
                    count++;
                }
            }
            subItem = new ListViewItem.ListViewSubItem(item, count.ToString());
            item.SubItems.Add(subItem);
            listViewStyles.Items.Add(item);
        }

        private void TimedTextStyles_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                DialogResult = DialogResult.Cancel;
            }
        }

        private void listViewStyles_SelectedIndexChanged(object sender, EventArgs e)
        {
            _doUpdate = false;
            if (listViewStyles.SelectedItems.Count == 1)
            {
                string styleName = listViewStyles.SelectedItems[0].Text;
                LoadStyle(styleName);
                GeneratePreview();
            }
            _doUpdate = true;
        }

        private void LoadStyle(string styleName)
        {
            XmlNode head = _xml.DocumentElement.SelectSingleNode("ttml:head", _nsmgr);
            foreach (XmlNode node in head.SelectNodes("//ttml:style", _nsmgr))
            {
                string name = "default";
                if (node.Attributes["xml:id"] != null)
                {
                    name = node.Attributes["xml:id"].Value;
                }
                else if (node.Attributes["id"] != null)
                {
                    name = node.Attributes["id"].Value;
                }

                if (name == styleName)
                {
                    string fontFamily = "Arial";
                    if (node.Attributes["tts:fontFamily"] != null)
                    {
                        fontFamily = node.Attributes["tts:fontFamily"].Value;
                    }

                    string fontWeight = "normal";
                    if (node.Attributes["tts:fontWeight"] != null)
                    {
                        fontWeight = node.Attributes["tts:fontWeight"].Value;
                    }

                    string fontStyle = "normal";
                    if (node.Attributes["tts:fontStyle"] != null)
                    {
                        fontStyle = node.Attributes["tts:fontStyle"].Value;
                    }

                    string fontColor = "white";
                    if (node.Attributes["tts:color"] != null)
                    {
                        fontColor = node.Attributes["tts:color"].Value;
                    }

                    string fontSize = "100%";
                    if (node.Attributes["tts:fontSize"] != null)
                    {
                        fontSize = node.Attributes["tts:fontSize"].InnerText;
                    }

                    textBoxStyleName.Text = name;
                    comboBoxFontName.Text = fontFamily;

                    textBoxFontSize.Text = fontSize;

                    // normal | italic | oblique
                    comboBoxFontStyle.SelectedIndex = 0;
                    if (fontStyle.Equals("italic", StringComparison.OrdinalIgnoreCase))
                    {
                        comboBoxFontStyle.SelectedIndex = 1;
                    }
                    if (fontStyle.Equals("oblique", StringComparison.OrdinalIgnoreCase))
                    {
                        comboBoxFontStyle.SelectedIndex = 2;
                    }

                    // normal | bold
                    comboBoxFontWeight.SelectedIndex = 0;
                    if (fontWeight.Equals("bold", StringComparison.OrdinalIgnoreCase))
                    {
                        comboBoxFontWeight.SelectedIndex = 1;
                    }

                    Color color = Color.White;
                    try
                    {
                        if (fontColor.StartsWith("rgb(", StringComparison.Ordinal))
                        {
                            string[] arr = fontColor.Remove(0, 4).TrimEnd(')').Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                            color = Color.FromArgb(int.Parse(arr[0]), int.Parse(arr[1]), int.Parse(arr[2]));
                        }
                        else
                        {
                            color = ColorTranslator.FromHtml(fontColor);
                        }
                    }
                    catch (Exception exception)
                    {
                        MessageBox.Show("Unable to read color: " + fontColor + " - " + exception.Message);
                    }
                    panelFontColor.BackColor = color;
                }
            }
        }

        private void buttonFontColor_Click(object sender, EventArgs e)
        {
            colorDialogStyle.Color = panelFontColor.BackColor;
            if (colorDialogStyle.ShowDialog() == DialogResult.OK)
            {
                listViewStyles.SelectedItems[0].SubItems[3].BackColor = colorDialogStyle.Color;
                listViewStyles.SelectedItems[0].SubItems[3].Text = Utilities.ColorToHex(colorDialogStyle.Color);
                panelFontColor.BackColor = colorDialogStyle.Color;
                UpdateHeaderXml(listViewStyles.SelectedItems[0].Text, "tts:color", Utilities.ColorToHex(colorDialogStyle.Color));
                GeneratePreview();
            }
        }

        private void buttonRemoveAll_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in listViewStyles.Items)
            {
                UpdateHeaderXmlRemoveStyle(item.Text);
            }
            listViewStyles.Items.Clear();
        }

        private void buttonRemove_Click(object sender, EventArgs e)
        {
            if (listViewStyles.SelectedItems.Count == 1)
            {
                int index = listViewStyles.SelectedItems[0].Index;
                string name = listViewStyles.SelectedItems[0].Text;

                UpdateHeaderXmlRemoveStyle(listViewStyles.SelectedItems[0].Text);
                listViewStyles.Items.RemoveAt(listViewStyles.SelectedItems[0].Index);

                if (index >= listViewStyles.Items.Count)
                {
                    index--;
                }
                listViewStyles.Items[index].Selected = true;
            }
        }

        private void buttonAdd_Click(object sender, EventArgs e)
        {
            string name = "new";
            int count = 2;
            while (StyleExists(name))
            {
                name = "new" + count;
                count++;
            }
            AddStyle(name, "Arial", "white", "100%");
            AddStyleToXml(name, "Arial", "normal", "normal", "white", "100%");
            listViewStyles.Items[listViewStyles.Items.Count - 1].Selected = true;
        }

        private void AddStyleToXml(string name, string fontFamily, string fontWeight, string fontStyle, string color, string fontSize)
        {
            TimedText10.AddStyleToXml(_xml, _xmlHead, _nsmgr, name, fontFamily, fontWeight, fontStyle, color, fontSize);
        }

        private bool StyleExists(string name)
        {
            foreach (ListViewItem item in listViewStyles.Items)
            {
                if (item.Text == name)
                {
                    return true;
                }
            }
            return false;
        }

        private void textBoxStyleName_TextChanged(object sender, EventArgs e)
        {
            if (listViewStyles.SelectedItems.Count == 1)
            {
                if (_doUpdate)
                {
                    if (!StyleExists(textBoxStyleName.Text))
                    {
                        UpdateHeaderXml(listViewStyles.SelectedItems[0].Text, "xml:id", textBoxStyleName.Text);
                        textBoxStyleName.BackColor = listViewStyles.BackColor;
                        listViewStyles.SelectedItems[0].Text = textBoxStyleName.Text;
                    }
                    else
                    {
                        textBoxStyleName.BackColor = Color.LightPink;
                    }
                }
            }
        }

        private void UpdateHeaderXml(string id, string tag, string value)
        {
            XmlNodeList styles = _xml.DocumentElement.SelectNodes("//ttml:head//ttml:styling/ttml:style", _nsmgr);

            foreach (XmlNode style in styles)
            {
                XmlAttribute idAttr = style.Attributes["xml:id"];

                if (idAttr == null)
                {
                    idAttr = style.Attributes["id"];
                }

                if (idAttr != null && idAttr.Value == id)
                {
                    if (tag == "id" || tag == "xml:id")
                    {
                        idAttr.Value = value;
                    }
                    else
                    {
                        XmlAttribute attrToChange = style.Attributes[tag];

                        if (attrToChange == null)
                        {
                            attrToChange = _xml.CreateAttribute(tag, TimedText10.TtmlStylingNamespace);
                            style.Attributes.Append(attrToChange);
                        }

                        attrToChange.Value = value;
                    }
                }
            }
        }

        private void UpdateHeaderXmlRemoveStyle(string id)
        {
            foreach (XmlNode innerNode in _xmlHead)
            {
                if (innerNode.Name == "styling")
                {
                    foreach (XmlNode innerInnerNode in innerNode)
                    {
                        if (innerInnerNode.Name == "style")
                        {
                            XmlAttribute idAttr = innerInnerNode.Attributes["xml:id"];
                            if (idAttr != null && idAttr.InnerText == id)
                            {
                                innerNode.RemoveChild(innerInnerNode);
                                break;
                            }
                        }
                    }
                }
            }
        }

        private void comboBoxFontName_TextChanged(object sender, EventArgs e)
        {
            if (listViewStyles.SelectedItems.Count == 1 && _doUpdate)
            {
                listViewStyles.SelectedItems[0].SubItems[1].Text = comboBoxFontName.Text;
                UpdateHeaderXml(listViewStyles.SelectedItems[0].Text, "tts:fontFamily", comboBoxFontName.Text);
                GeneratePreview();
            }
        }

        private void textBoxFontSize_TextChanged(object sender, EventArgs e)
        {
            if (listViewStyles.SelectedItems.Count == 1 && _doUpdate)
            {
                listViewStyles.SelectedItems[0].SubItems[2].Text = textBoxFontSize.Text;
                UpdateHeaderXml(listViewStyles.SelectedItems[0].Text, "tts:fontSize", textBoxFontSize.Text);
                GeneratePreview();
            }
        }

        private void comboBoxFontStyle_TextChanged(object sender, EventArgs e)
        {
            if (listViewStyles.SelectedItems.Count == 1 && _doUpdate)
            {
                UpdateHeaderXml(listViewStyles.SelectedItems[0].Text, "tts:fontStyle", comboBoxFontStyle.Text);
                GeneratePreview();
            }
        }

        private void comboBoxFontWeight_TextChanged(object sender, EventArgs e)
        {
            if (listViewStyles.SelectedItems.Count == 1 && _doUpdate)
            {
                UpdateHeaderXml(listViewStyles.SelectedItems[0].Text, "tts:fontWeight", comboBoxFontWeight.Text);
                GeneratePreview();
            }
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void TimedTextStyles_ResizeEnd(object sender, EventArgs e)
        {
            GeneratePreview();
        }

        private void TimedTextStyles_SizeChanged(object sender, EventArgs e)
        {
            GeneratePreview();
        }

    }
}
