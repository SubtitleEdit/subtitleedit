using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using Nikse.SubtitleEdit.Logic;
using System.Collections.Generic;

namespace Nikse.SubtitleEdit.Forms
{
    public sealed partial class ExportPngXml : Form
    {
        Subtitle _subtitle;
        Color _subtitleColor = Color.White;
        string _subtitleFontName = "Verdana";
        float _subtitleFontSize = 75.0f;
        Color _borderColor = Color.Black;
        float _borderWidth = 2.0f;
        bool _isLoading = true;
        string _exportType = "BDNXML";

        public ExportPngXml()
        {
            InitializeComponent();
        }

        private static string BdnXmlTimeCode(TimeCode timecode)
        {
            int frames = timecode.Milliseconds / 40; // 40==25fps (1000/25)
            return string.Format("{0:00}:{1:00}:{2:00}:{3:00}", timecode.Hours, timecode.Minutes, timecode.Seconds, frames);
        }

        private void buttonExport_Click(object sender, EventArgs e)
        {
            SetupImageParameters();

            saveFileDialog1.Title = "Choose Blu-ray sup file name...";
            saveFileDialog1.DefaultExt = ".sup";
            saveFileDialog1.AddExtension = true;

            if (_exportType == "BLURAYSUP" &&  saveFileDialog1.ShowDialog(this) == DialogResult.OK || 
                _exportType == "BDNXML" && folderBrowserDialog1.ShowDialog(this) == DialogResult.OK)
            {
                FileStream bluRaySupFile = null;
                if (_exportType == "BLURAYSUP")
                    bluRaySupFile = new FileStream(saveFileDialog1.FileName, FileMode.Create);

                progressBar1.Value = 0;
                progressBar1.Maximum = _subtitle.Paragraphs.Count-1;
                progressBar1.Visible = true;

                int width = 1920;
                int height = 1080;
                if (comboBoxResolution.SelectedIndex == 1)
                { 
                    width = 1280;
                    height = 720;
                }
                else if (comboBoxResolution.SelectedIndex == 2)
                { 
                    width = 848;
                    height = 480;
                }
                else if (comboBoxResolution.SelectedIndex == 3)
                { 
                    width = 720;
                    height = 576;
                }
                else if (comboBoxResolution.SelectedIndex == 4)
                { 
                    width = 720;
                    height = 480;
                }

                const int border = 25;
                int imagesSavedCount = 0;
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < _subtitle.Paragraphs.Count; i++)
                {
                    Paragraph p = _subtitle.Paragraphs[i];
                    Bitmap bmp = GenerateImageFromTextWithStyle(p.Text);
                    string numberString = string.Format("{0:0000}", i + 1);
                    if (bmp != null)
                    {
                        if (_exportType == "BLURAYSUP")
                        {
                            Nikse.SubtitleEdit.Logic.BluRaySup.BluRaySupPicture brSub = new Logic.BluRaySup.BluRaySupPicture();
                            brSub.StartTime = (long)p.StartTime.TotalMilliseconds;
                            brSub.EndTime = (long)p.EndTime.TotalMilliseconds;
                            brSub.Width = height;
                            brSub.Height = width;
                            byte[] buffer = Nikse.SubtitleEdit.Logic.BluRaySup.BluRaySupPicture.CreateSupFrame(brSub, bmp);
                            bluRaySupFile.Write(buffer, 0, buffer.Length);
                        }
                        else
                        {
                            string fileName = Path.Combine(folderBrowserDialog1.SelectedPath, numberString + ".png");
                            bmp.Save(fileName, System.Drawing.Imaging.ImageFormat.Png);
                            imagesSavedCount++;

                            //<Event InTC="00:00:24:07" OutTC="00:00:31:13" Forced="False">
                            //  <Graphic Width="696" Height="111" X="612" Y="930">subtitle_exp_0001.png</Graphic>
                            //</Event>
                            sb.AppendLine("<Event InTC=\"" + BdnXmlTimeCode(p.StartTime) + "\" OutTC=\"" + BdnXmlTimeCode(p.EndTime) + "\" Forced=\"False\">");
                            int x = (width - bmp.Width) / 2;
                            int y = height - (bmp.Height + border);
                            sb.AppendLine("  <Graphic Width=\"" + bmp.Width.ToString() + "\" Height=\"" + bmp.Height.ToString() + "\" X=\"" + x.ToString() + "\" Y=\"" + y.ToString() + "\">" + numberString + ".png</Graphic>");
                            sb.AppendLine("</Event>");
                        }
                        bmp.Dispose();
                        progressBar1.Value = i;
                    }
                }
                progressBar1.Visible = false;
                if (_exportType == "BLURAYSUP")
                {
                    bluRaySupFile.Close();
                    MessageBox.Show(string.Format(Configuration.Settings.Language.Main.SavedSubtitleX, saveFileDialog1.FileName));
                }
                else
                {
                    XmlDocument doc = new XmlDocument();
                    Paragraph first = _subtitle.Paragraphs[0];
                    Paragraph last = _subtitle.Paragraphs[_subtitle.Paragraphs.Count - 1];
                    doc.LoadXml("<?xml version=\"1.0\" encoding=\"UTF-8\"?>" + Environment.NewLine +
                                "<BDN Version=\"0.93\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xsi:noNamespaceSchemaLocation=\"BD-03-006-0093b BDN File Format.xsd\">" + Environment.NewLine +
                                "<Description>" + Environment.NewLine +
                                "<Name Title=\"subtitle_exp\" Content=\"\"/>" + Environment.NewLine +
                                "<Language Code=\"eng\"/>" + Environment.NewLine +
                                "<Format VideoFormat=\"1080p\" FrameRate=\"25\" DropFrame=\"False\"/>" + Environment.NewLine +
                                "<Events Type=\"Graphic\" FirstEventInTC=\"" + BdnXmlTimeCode(first.StartTime) + "\" LastEventOutTC=\"" + BdnXmlTimeCode(last.EndTime) + "\" NumberofEvents=\"" + imagesSavedCount.ToString() + "\"/>" + Environment.NewLine +
                                "</Description>" + Environment.NewLine +
                                "<Events>" + Environment.NewLine +
                                "</Events>" + Environment.NewLine +
                                "</BDN>");
                    XmlNode events = doc.DocumentElement.SelectSingleNode("Events");
                    events.InnerXml = sb.ToString();
                    File.WriteAllText(Path.Combine(folderBrowserDialog1.SelectedPath, "BDN_Index.xml"), doc.OuterXml);
                    MessageBox.Show(string.Format(Configuration.Settings.Language.ExportPngXml.XImagesSavedInY, imagesSavedCount, folderBrowserDialog1.SelectedPath));
                }
            }
        }

        private void SetupImageParameters()
        {
            if (_isLoading)
                return;

            _subtitleColor = panelColor.BackColor;
            _borderColor = panelBorderColor.BackColor;
            _subtitleFontName = comboBoxSubtitleFont.SelectedItem.ToString();
            _subtitleFontSize = float.Parse(comboBoxSubtitleFontSize.SelectedItem.ToString());
            _borderWidth = float.Parse(comboBoxBorderWidth.SelectedItem.ToString());
        }

        private Bitmap GenerateImageFromTextWithStyle(string text)
        {
            // remove styles for display text (except italic)
            text = RemoveSubStationAlphaFormatting(text);
            text = text.Replace("<b>", string.Empty);
            text = text.Replace("</b>", string.Empty);
            text = text.Replace("<B>", string.Empty);
            text = text.Replace("</B>", string.Empty);
            text = text.Replace("<u>", string.Empty);
            text = text.Replace("</u>", string.Empty);
            text = text.Replace("<U>", string.Empty);
            text = text.Replace("</U>", string.Empty);
            text = Logic.Utilities.RemoveHtmlFontTag(text);

            Font font;
            try
            {
                font = new System.Drawing.Font(_subtitleFontName, _subtitleFontSize);
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
                font = new System.Drawing.Font("Verdana", _subtitleFontSize);
            }
            Bitmap bmp = new Bitmap(400, 200);
            Graphics g = Graphics.FromImage(bmp);

            SizeF textSize = g.MeasureString("Hj!", font);
            var lineHeight = (textSize.Height * 0.64f);
            float italicSpacing = textSize.Width / 8;

            textSize = g.MeasureString(text, font);
            g.Dispose();
            bmp.Dispose();
            bmp = new Bitmap((int)(textSize.Width * 0.8), (int)(textSize.Height * 0.7)+10);
            g = Graphics.FromImage(bmp);

            List<float> lefts = new List<float>();
            foreach (string line in text.Replace("<i>", "i").Replace("</i>", "i").Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries))
            {
                if (comboBoxHAlign.SelectedIndex == 0) // left
                    lefts.Add(5);
                else
                    lefts.Add((float)(bmp.Width - g.MeasureString(line, font).Width * 0.8) / 2);
            }
           

            if (checkBoxAntiAlias.Checked)
            {
                g.TextRenderingHint = TextRenderingHint.AntiAlias;
                g.SmoothingMode = SmoothingMode.AntiAlias;
            }
            StringFormat sf = new StringFormat();
            sf.Alignment = StringAlignment.Near;
            sf.LineAlignment = StringAlignment.Near;// draw the text to a path
            System.Drawing.Drawing2D.GraphicsPath path = new System.Drawing.Drawing2D.GraphicsPath();

            // display italic
            StringBuilder sb = new StringBuilder();
            int i = 0;
            bool isItalic = false;
            float left = 5;
            if (lefts.Count >= 0)
                left = lefts[0];
            float top = 5;
            bool newLine = false;
            float addX = 0;
            int lineNumber = 0;
            float leftMargin = left;
            bool italicFromStart = false;
            while (i < text.Length)
            {
                if (text.Substring(i).ToLower().StartsWith("<i>"))
                {
                    italicFromStart = i == 0;
                    if (sb.Length > 0)
                    {
                        TextDraw.DrawText(font, sf, path, sb, isItalic, left, top, ref newLine, addX, leftMargin);
                        addX = 0;
                    }
                    isItalic = true;
                    i += 2;
                }
                else if (text.Substring(i).ToLower().StartsWith("</i>") && isItalic)
                {
                    if (italicFromStart)
                        addX = 0;
                    else
                        addX = italicSpacing;
                    TextDraw.DrawText(font, sf, path, sb, isItalic, left, top, ref newLine, addX, leftMargin);
                    addX = 1;
                    if (_subtitleFontName.StartsWith("Arial"))
                        addX = 3;
                    isItalic = false;
                    i += 3;
                }
                else if (text.Substring(i).StartsWith(Environment.NewLine))
                {
                    if (italicFromStart)
                        addX = 0;
                    else
                        addX = italicSpacing;

                    TextDraw.DrawText(font, sf, path, sb, isItalic, left, top, ref newLine, addX, leftMargin);

                    addX = 0;
                    top += lineHeight;
                    newLine = true;
                    i += Environment.NewLine.Length - 1;
                    addX = 0;
                    lineNumber++;
                    if (lineNumber < lefts.Count)
                        leftMargin = lefts[lineNumber];
                    if (isItalic)
                        italicFromStart = true;
                }
                else
                {
                    sb.Append(text.Substring(i, 1));
                }
                i++;
            }
            if (sb.Length > 0)
            {
                if (italicFromStart)
                    addX = 0;
                else
                    addX = italicSpacing;
                TextDraw.DrawText(font, sf, path, sb, isItalic, left, top, ref newLine, addX, leftMargin);
            }

            if (_borderWidth > 0)
                g.DrawPath(new Pen(_borderColor, _borderWidth), path);
            g.FillPath(new SolidBrush(_subtitleColor), path);
            g.Dispose();
            return bmp;
        }

        private static string RemoveSubStationAlphaFormatting(string s)
        {
            int indexOfBegin = s.IndexOf("{");
            while (indexOfBegin >= 0 && s.IndexOf("}") > indexOfBegin)
            {
                int indexOfEnd = s.IndexOf("}");
                s = s.Remove(indexOfBegin, (indexOfEnd - indexOfBegin) + 1);
                indexOfBegin = s.IndexOf("{");
            }
            return s;
        }

        internal void Initialize(Subtitle subtitle, string exportType)
        {
            _exportType = exportType;
            if (exportType == "BLURAYSUP")
            {
                this.Text = "Blu-ray SUP";
            }
            else
            {
                this.Text = Configuration.Settings.Language.ExportPngXml.Title;
            }            
            groupBoxImageSettings.Text = Configuration.Settings.Language.ExportPngXml.ImageSettings;
            labelSubtitleFont.Text = Configuration.Settings.Language.ExportPngXml.FontFamily;
            labelSubtitleFontSize.Text = Configuration.Settings.Language.ExportPngXml.FontSize;
            buttonColor.Text = Configuration.Settings.Language.ExportPngXml.FontColor;
            checkBoxAntiAlias.Text = Configuration.Settings.Language.ExportPngXml.AntiAlias;
            buttonBorderColor.Text = Configuration.Settings.Language.ExportPngXml.BorderColor;
            labelBorderWidth.Text = Configuration.Settings.Language.ExportPngXml.BorderWidth;
            buttonExport.Text = Configuration.Settings.Language.ExportPngXml.ExportAllLines;
            buttonCancel.Text = Configuration.Settings.Language.General.Cancel;
            labelImageResolution.Text = string.Empty;
            subtitleListView1.InitializeLanguage(Configuration.Settings.Language.General, Configuration.Settings);
            Utilities.InitializeSubtitleFont(subtitleListView1);
            subtitleListView1.AutoSizeAllColumns(this);

            _subtitle = subtitle;
            panelColor.BackColor = _subtitleColor;
            panelBorderColor.BackColor = _borderColor;
            comboBoxBorderWidth.SelectedIndex = 2;
            comboBoxHAlign.SelectedIndex = 1;
            comboBoxResolution.SelectedIndex = 0;

            foreach (var x in System.Drawing.FontFamily.Families)
            {
                comboBoxSubtitleFont.Items.Add(x.Name);
                if (string.Compare(x.Name, _subtitleFontName, true) == 0)
                    comboBoxSubtitleFont.SelectedIndex = comboBoxSubtitleFont.Items.Count - 1;
            }

            subtitleListView1.Fill(_subtitle);
            subtitleListView1.SelectIndexAndEnsureVisible(0);
        }

        private void subtitleListView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            SetupImageParameters();
            if (subtitleListView1.SelectedItems.Count > 0)
            {
                var bmp = GenerateImageFromTextWithStyle(_subtitle.Paragraphs[subtitleListView1.SelectedItems[0].Index].Text);
                pictureBox1.Image = bmp;
                labelImageResolution.Text = string.Format("{0}x{1}", bmp.Width, bmp.Height);
            }
        }

        private void buttonColor_Click(object sender, EventArgs e)
        {
            colorDialog1.Color = panelColor.BackColor;
            if (colorDialog1.ShowDialog() == DialogResult.OK)
            {
                panelColor.BackColor = colorDialog1.Color;
                subtitleListView1_SelectedIndexChanged(null, null);
            }
        }

        private void panelColor_MouseClick(object sender, MouseEventArgs e)
        {
            buttonColor_Click(null, null);
        }


        private void buttonBorderColor_Click(object sender, EventArgs e)
        {
            colorDialog1.Color = panelBorderColor.BackColor;
            if (colorDialog1.ShowDialog() == DialogResult.OK)
            {
                panelBorderColor.BackColor = colorDialog1.Color;
                subtitleListView1_SelectedIndexChanged(null, null);
            }
        }

        private void panelBorderColor_MouseClick(object sender, MouseEventArgs e)
        {
            buttonBorderColor_Click(null, null);
        }

        private void comboBoxSubtitleFont_SelectedValueChanged(object sender, EventArgs e)
        {
            subtitleListView1_SelectedIndexChanged(null, null);
        }

        private void comboBoxSubtitleFontSize_SelectedIndexChanged(object sender, EventArgs e)
        {
            subtitleListView1_SelectedIndexChanged(null, null);
        }

        private void comboBoxBorderWidth_SelectedIndexChanged(object sender, EventArgs e)
        {
            subtitleListView1_SelectedIndexChanged(null, null);
        }

        private void checkBoxAntiAlias_CheckedChanged(object sender, EventArgs e)
        {
            subtitleListView1_SelectedIndexChanged(null, null);
        }

        private void ExportPngXml_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                DialogResult = DialogResult.Cancel;
            }
            else if (e.KeyCode == Keys.F1)
            {
                Utilities.ShowHelp(string.Empty);
                e.SuppressKeyPress = true;
            }
        }

        private void ExportPngXml_Shown(object sender, EventArgs e)
        {
            _isLoading = false;
            comboBoxSubtitleFontSize.SelectedIndex = 15;
        }

        private void comboBoxHAlign_SelectedIndexChanged(object sender, EventArgs e)
        {
            subtitleListView1_SelectedIndexChanged(null, null);
        }

    }
}
