﻿using Nikse.SubtitleEdit.Core;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Logic;
using System;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.Forms
{
    /// <summary>
    /// Export to FCP XML for import into other programs, like DaVinci Resolve
    /// 
    /// Elements from FCP XML 1.5 DTD
    // <!ATTLIST text-style ref IDREF #IMPLIED>
    // <!ATTLIST text-style font CDATA #IMPLIED>
    // <!ATTLIST text-style fontSize CDATA #IMPLIED>
    // <!ATTLIST text-style fontFace CDATA #IMPLIED> - e.g. "Regular"
    // <!ATTLIST text-style fontColor CDATA #IMPLIED> - e.g. "0.793266 0.793391 0.793221 1", R G B Alpha
    // <!ATTLIST text-style bold (0 | 1) #IMPLIED> - "1" = bold
    // <!ATTLIST text-style italic(0 | 1) #IMPLIED> - "1" = italic
    // <!ATTLIST text-style strokeColor CDATA #IMPLIED>
    // <!ATTLIST text-style strokeWidth CDATA #IMPLIED>
    // <!ATTLIST text-style baseline CDATA #IMPLIED> - number, e.g. "29"
    // <!ATTLIST text-style shadowColor CDATA #IMPLIED>
    // <!ATTLIST text-style shadowOffset CDATA #IMPLIED>
    // <!ATTLIST text-style shadowBlurRadius CDATA #IMPLIED>
    // <!ATTLIST text-style kerning CDATA #IMPLIED>
    // <!ATTLIST text-style alignment (left | center | right | justified) #IMPLIED>
    // <!ATTLIST text-style lineSpacing CDATA #IMPLIED>
    // <!ATTLIST text-style tabStops CDATA #IMPLIED>
    /// </summary>
    public partial class ExportFcpXmlAdvanced : Form
    {

        private string _fontName = "Arial";
        private Subtitle _subtitle;
        private string _videoFileName;

        public ExportFcpXmlAdvanced(Subtitle subtitle, string videoFileName)
        {
            UiUtil.PreInitialize(this);
            InitializeComponent();
            UiUtil.FixFonts(this);

            _subtitle = new Subtitle(subtitle);
            _videoFileName = videoFileName;

            Text = Configuration.Settings.Language.ExportFcpXmlAdvanced.Title;
            labelSubtitleFont.Text = Configuration.Settings.Language.ExportFcpXmlAdvanced.FontName;
            labelSubtitleFontSize.Text = Configuration.Settings.Language.ExportFcpXmlAdvanced.FontSize;
            labelSubtitleFontFace.Text = Configuration.Settings.Language.ExportFcpXmlAdvanced.FontFace;
            labelHorizontalAlign.Text = Configuration.Settings.Language.ExportFcpXmlAdvanced.Alignment;
            labelBaseline.Text = Configuration.Settings.Language.ExportFcpXmlAdvanced.Baseline;
            labelFrameRate.Text = Configuration.Settings.Language.General.FrameRate;
            labelResolution.Text = Configuration.Settings.Language.ExportPngXml.VideoResolution;
            buttonSave.Text = Configuration.Settings.Language.ExportCustomText.SaveAs;
            buttonCancel.Text = Configuration.Settings.Language.General.Cancel;

            foreach (var x in FontFamily.Families)
            {
                if (x.IsStyleAvailable(FontStyle.Regular) || x.IsStyleAvailable(FontStyle.Bold))
                {
                    comboBoxFontName.Items.Add(x.Name);
                    if (x.Name.Equals(_fontName, StringComparison.OrdinalIgnoreCase))
                        comboBoxFontName.SelectedIndex = comboBoxFontName.Items.Count - 1;
                }
            }

            comboBoxFrameRate.Items.Add((23.976).ToString(CultureInfo.CurrentCulture));
            comboBoxFrameRate.Items.Add((24.0).ToString(CultureInfo.CurrentCulture));
            comboBoxFrameRate.Items.Add((25.0).ToString(CultureInfo.CurrentCulture));
            comboBoxFrameRate.Items.Add((29.97).ToString(CultureInfo.CurrentCulture));
            comboBoxFrameRate.Items.Add((30.00).ToString(CultureInfo.CurrentCulture));
            comboBoxFrameRate.Items.Add((50.00).ToString(CultureInfo.CurrentCulture));
            comboBoxFrameRate.Items.Add((59.94).ToString(CultureInfo.CurrentCulture));
            comboBoxFrameRate.Items.Add((60.00).ToString(CultureInfo.CurrentCulture));

            comboBoxResolution.Items.Clear();
            comboBoxResolution.Items.Add("NTSC-601");
            comboBoxResolution.Items.Add("PAL-601");
            comboBoxResolution.Items.Add("square");
            comboBoxResolution.Items.Add("DVCPROHD-720P");
            comboBoxResolution.Items.Add("HD-(960x720)");
            comboBoxResolution.Items.Add("DVCPROHD-1080i60");
            comboBoxResolution.Items.Add("HD-(1280x1080)");
            comboBoxResolution.Items.Add("FullHD 1920x1080");
            comboBoxResolution.Items.Add("DVCPROHD-1080i50");
            comboBoxResolution.Items.Add("HD-(1440x1080)");
            comboBoxResolution.SelectedIndex = 7; // FullHD

            comboBoxFontName.SelectedIndex = 0;
            for (int i = 0; i < comboBoxFontName.Items.Count; i++)
            {
                if (Configuration.Settings.FcpExportSettings.FontName == comboBoxFontName.Items[i].ToString())
                {
                    comboBoxFontName.SelectedIndex = i;
                    break;
                }
            }

            comboBoxFontSize.SelectedIndex = 21;
            for (int i = 0; i < comboBoxFontSize.Items.Count; i++)
            {
                if (Configuration.Settings.FcpExportSettings.FontSize == int.Parse(comboBoxFontSize.Items[i].ToString()))
                {
                    comboBoxFontSize.SelectedIndex = i;
                    break;
                }
            }

            comboBoxFontFace.SelectedIndex = 0;

            comboBoxHAlign.SelectedIndex = 1;
            for (int i = 0; i < comboBoxHAlign.Items.Count; i++)
            {
                if (Configuration.Settings.FcpExportSettings.Alignment == comboBoxHAlign.Items[i].ToString())
                {
                    comboBoxHAlign.SelectedIndex = i;
                    break;
                }
            }

            comboBoxBaseline.SelectedIndex = 27;
            for (int i = 0; i < comboBoxBaseline.Items.Count; i++)
            {
                if (Configuration.Settings.FcpExportSettings.Baseline == int.Parse(comboBoxBaseline.Items[i].ToString()))
                {
                    comboBoxBaseline.SelectedIndex = i;
                    break;
                }
            }

            comboBoxFrameRate.SelectedItem = Configuration.Settings.General.CurrentFrameRate.ToString(CultureInfo.CurrentCulture);

            panelColor.BackColor = Configuration.Settings.FcpExportSettings.Color;

            SubtitleListview.InitializeLanguage(Configuration.Settings.Language.General, Configuration.Settings);
            SubtitleListview.InitializeTimestampColumnWidths(this);
            UiUtil.InitializeSubtitleFont(SubtitleListview);
            SubtitleListview.AutoSizeAllColumns(this);
            UiUtil.FixLargeFonts(this, buttonCancel);
            SubtitleListview.Fill(_subtitle.Paragraphs);
        }

        private void ExportFcpXmlAdvanced_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                e.SuppressKeyPress = true;
                DialogResult = DialogResult.Cancel;
            }
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void buttonSave_Click(object sender, EventArgs e)
        {
            saveFileDialog1.Title = Configuration.Settings.Language.ExportPngXml.SaveFcpAs;
            saveFileDialog1.DefaultExt = "*.fcpxml";
            saveFileDialog1.AddExtension = true;
            saveFileDialog1.Filter = "FCP XML files|*.fcpxml";
            if (saveFileDialog1.ShowDialog(this) == DialogResult.OK)
            {
                var oldFrameRate = Configuration.Settings.General.CurrentFrameRate;
                double d;
                if (double.TryParse(comboBoxFrameRate.SelectedItem.ToString(), out d))
                    Configuration.Settings.General.CurrentFrameRate = d;

                var format = new FinalCutProXml15();
                format.DefaultStyle.FontName = comboBoxFontName.SelectedItem.ToString();
                format.DefaultStyle.FontSize = int.Parse(comboBoxFontSize.SelectedItem.ToString());
                format.DefaultStyle.FontFace = comboBoxFontFace.SelectedItem.ToString();
                format.DefaultStyle.Alignment = comboBoxHAlign.SelectedItem.ToString();
                format.DefaultStyle.Baseline = int.Parse(comboBoxBaseline.SelectedItem.ToString());
                int height, width;
                GetResolution(out width, out height);
                format.DefaultStyle.Width = width;
                format.DefaultStyle.Height = height;

                var text = _subtitle.ToText(format);
                File.WriteAllText(saveFileDialog1.FileName, text, Encoding.UTF8);
                Configuration.Settings.General.CurrentFrameRate = oldFrameRate;
                MessageBox.Show(string.Format(Configuration.Settings.Language.Main.SavedSubtitleX, saveFileDialog1.FileName));
                DialogResult = DialogResult.OK;
            }
        }

        private void buttonColor_Click(object sender, EventArgs e)
        {
            bool showAlpha = true;
            using (var colorChooser = new ColorChooser { Color = panelColor.BackColor, ShowAlpha = showAlpha })
            {
                if (colorChooser.ShowDialog() == DialogResult.OK)
                {
                    panelColor.BackColor = colorChooser.Color;
                }
            }
        }

        private void panelColor_Click(object sender, EventArgs e)
        {
            buttonColor_Click(sender, e);
        }

        private void GetResolution(out int width, out int height)
        {
            width = 1920;
            height = 1080;
            if (comboBoxResolution.SelectedIndex < 0)
                return;

            string text = comboBoxResolution.Text.Trim();
            if (text == "NTSC-601")
            {
                width = 720;
                height = 480;
                return;
            }

            if (text == "PAL-601")
            {
                width = 720;
                height = 576;
                return;
            }

            if (text == "square")
            {
                width = 640;
                height = 480;
                return;
            }

            if (text == "DVCPROHD-720P")
            {
                width = 1280;
                height = 720;
                return;
            }

            if (text == "HD-(960x720)")
            {
                width = 960;
                height = 720;
                return;
            }

            if (text == "FullHD 1920x1080")
            {
                width = 1920;
                height = 1080;
                return;
            }

            if (text == "DVCPROHD-1080i60")
            {
                width = 1920;
                height = 1080;
                return;
            }

            if (text == "HD-(1280x1080)")
            {
                width = 1280;
                height = 1080;
                return;
            }

            if (text == "DVCPROHD-1080i50")
            {
                width = 1920;
                height = 1080;
                return;
            }

            if (text == "HD-(1440x1080)")
            {
                width = 1440;
                height = 1080;
                return;
            }

            if (text.Contains('('))
                text = text.Remove(0, text.IndexOf('(')).Trim();
            text = text.TrimStart('(').TrimEnd(')').Trim();
            string[] arr = text.Split('x');
            width = int.Parse(arr[0]);
            height = int.Parse(arr[1]);
        }

        private void buttonCustomResolution_Click(object sender, EventArgs e)
        {
            using (var cr = new ChooseResolution())
            {
                if (cr.ShowDialog(this) == DialogResult.OK)
                {
                    comboBoxResolution.Items[comboBoxResolution.Items.Count - 1] = cr.VideoWidth + "x" + cr.VideoHeight;
                    comboBoxResolution.SelectedIndex = comboBoxResolution.Items.Count - 1;
                }
            }
        }

        private void SetResolution(string xAndY)
        {
            if (string.IsNullOrEmpty(xAndY))
                return;

            xAndY = xAndY.ToLower();

            switch (xAndY)
            {
                case "720x480":
                    xAndY = "NTSC-601";
                    break;
                case "720x576":
                    xAndY = "PAL-601";
                    break;
                case "640x480":
                    xAndY = "square";
                    break;
                case "1280x720":
                    xAndY = "DVCPROHD-720P";
                    break;
                case "960x720":
                    xAndY = "HD-(960x720)";
                    break;
                case "1920x1080":
                    xAndY = "FullHD 1920x1080";
                    break;
                case "1280x1080":
                    xAndY = "HD-(1280x1080)";
                    break;
                case "1440x1080":
                    xAndY = "HD-(1440x1080)";
                    break;
            }

            if (Regex.IsMatch(xAndY, @"\d+x\d+", RegexOptions.IgnoreCase))
            {
                for (int i = 0; i < comboBoxResolution.Items.Count; i++)
                {
                    if (comboBoxResolution.Items[i].ToString().Contains(xAndY))
                    {
                        comboBoxResolution.SelectedIndex = i;
                        return;
                    }
                }
                comboBoxResolution.Items[comboBoxResolution.Items.Count - 1] = xAndY;
                comboBoxResolution.SelectedIndex = comboBoxResolution.Items.Count - 1;
            }
        }

        private void ExportFcpXmlAdvanced_Load(object sender, EventArgs e)
        {

        }

        private void ExportFcpXmlAdvanced_Shown(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(_videoFileName))
            {
                var videoInfo = UiUtil.GetVideoInfo(_videoFileName);
                if (videoInfo != null && videoInfo.Width > 0 && videoInfo.Height > 0)
                {
                    SetResolution($"{videoInfo.Width}x{videoInfo.Height}");
                }
            }
        }

        private void ExportFcpXmlAdvanced_FormClosing(object sender, FormClosingEventArgs e)
        {
            Configuration.Settings.FcpExportSettings.FontName = comboBoxFontName.SelectedItem.ToString();
            Configuration.Settings.FcpExportSettings.FontSize = int.Parse(comboBoxFontSize.SelectedItem.ToString());
            Configuration.Settings.FcpExportSettings.Baseline = int.Parse(comboBoxBaseline.SelectedItem.ToString());
            Configuration.Settings.FcpExportSettings.Alignment = comboBoxHAlign.SelectedItem.ToString();
            Configuration.Settings.FcpExportSettings.Color = panelColor.BackColor;
        }
    }
}