using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Logic;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using static Nikse.SubtitleEdit.Forms.ExportPngXml;

namespace Nikse.SubtitleEdit.Forms.BinaryEdit
{
    public partial class BinEditNewText : Form
    {
        private readonly bool _loading;
        private readonly Dictionary<string, int> _lineHeights;

        public Bitmap Bitmap { get; set; }

        public BinEditNewText(string text)
        {
            UiUtil.PreInitialize(this);
            InitializeComponent();
            UiUtil.FixFonts(this);

            _loading = true;
            _lineHeights = new Dictionary<string, int>();
            panelColor.BackColor = Color.FromArgb(byte.MaxValue, Configuration.Settings.Tools.ExportFontColor);
            panelBorderColor.BackColor = Configuration.Settings.Tools.ExportBorderColor;
            panelShadowColor.BackColor = Configuration.Settings.Tools.ExportShadowColor;
            checkBoxBold.Checked = Configuration.Settings.Tools.ExportLastFontBold;

            comboBoxHAlign.SelectedIndex = 0;
            comboBoxBorderWidth.SelectedIndex = 2;
            comboBoxShadowWidth.SelectedIndex = 2;

            _lineHeights.Clear();
            _lineHeights.Add(string.Empty, (int)numericUpDownLineSpacing.Value);

            SetFonts();
            SetFontSize();
            _loading = false;
            textBoxText.Text = text;

            buttonOK.Text = LanguageSettings.Current.General.Ok;
            buttonCancel.Text = LanguageSettings.Current.General.Cancel;
            UiUtil.FixLargeFonts(this, buttonOK);
        }

        private void SetFontSize()
        {
            var fontSize = Configuration.Settings.Tools.ExportBluRayFontSize;
            comboBoxSubtitleFontSize.SelectedIndex = 16;
            int i = 0;
            foreach (string item in comboBoxSubtitleFontSize.Items)
            {
                if (item == Convert.ToInt32(fontSize).ToString(CultureInfo.InvariantCulture))
                {
                    comboBoxSubtitleFontSize.SelectedIndex = i;
                    break;
                }

                i++;
            }
        }

        private void SetFonts()
        {
            if (string.IsNullOrEmpty(Configuration.Settings.Tools.ExportBluRayFontName))
            {
                Configuration.Settings.Tools.ExportBluRayFontName = "Arial";
            }

            foreach (var x in FontFamily.Families)
            {
                if (x.IsStyleAvailable(FontStyle.Regular) || x.IsStyleAvailable(FontStyle.Bold))
                {
                    comboBoxSubtitleFont.Items.Add(x.Name);
                    if (x.Name.Equals(Configuration.Settings.Tools.ExportBluRayFontName, StringComparison.OrdinalIgnoreCase))
                    {
                        comboBoxSubtitleFont.SelectedIndex = comboBoxSubtitleFont.Items.Count - 1;
                    }
                }
            }

            if (comboBoxSubtitleFont.SelectedIndex == -1)
            {
                comboBoxSubtitleFont.SelectedIndex = 0;
            }
        }

        private void textBoxText_TextChanged(object sender, EventArgs e)
        {
            if (_loading)
            {
                return;
            }

            var mp = MakeBitmapParameter();
            var bmp = GenerateImageFromTextWithStyle(mp);
            pictureBox1.Image?.Dispose();
            pictureBox1.Image = bmp;
        }

        private MakeBitmapParameter MakeBitmapParameter()
        {
            return new MakeBitmapParameter
            {
                Type = ExportFormats.BluraySup,
                SubtitleColor = panelColor.BackColor,
                SubtitleFontName = comboBoxSubtitleFont.SelectedItem.ToString(),
                SubtitleFontSize = int.Parse(comboBoxSubtitleFontSize.SelectedItem.ToString()),
                SubtitleFontBold = checkBoxBold.Checked,
                BorderColor = panelBorderColor.BackColor,
                BorderWidth = int.Parse(comboBoxBorderWidth.SelectedItem.ToString()),
                SimpleRendering = checkBoxSimpleRender.Checked,
                AlignLeft = comboBoxHAlign.SelectedIndex == 0,
                AlignRight = comboBoxHAlign.SelectedIndex == 2,
                JustifyLeft = false, //GetJustifyLeft(p.Text), // center, left justify
                JustifyTop = comboBoxHAlign.SelectedIndex == 4, // center, top justify
                JustifyRight = comboBoxHAlign.SelectedIndex == 5, // center, right justify
                ScreenWidth = 1920,
                ScreenHeight = 1080,
                VideoResolution = "1920x1080",
                Bitmap = null,
                FramesPerSeconds = 24,
                BottomMargin = 50, // GetBottomMarginInPixels(p),
                LeftMargin = 50, //GetLeftMarginInPixels(p),
                RightMargin = 50, //GetRightMarginInPixels(p),
                Saved = false,
                Alignment = ContentAlignment.BottomCenter,
                Type3D = 0,
                Depth3D = 0,
                BackgroundColor = Color.Transparent,
                SavDialogFileName = string.Empty,
                ShadowColor = panelShadowColor.BackColor,
                ShadowWidth = int.Parse(comboBoxShadowWidth.SelectedItem.ToString()),
                ShadowAlpha = (int)numericUpDownShadowTransparency.Value,
                LineHeight = _lineHeights,
                FullFrame = false,
                FullFrameBackgroundColor = Color.Transparent,
                P = new Paragraph(textBoxText.Text, 0, 2000),
            };
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            Bitmap = (Bitmap)pictureBox1.Image;
            DialogResult = DialogResult.OK;
        }

        private void BinEditNewText_Shown(object sender, EventArgs e)
        {
            textBoxText.Focus();
        }

        private void buttonColor_Click(object sender, EventArgs e)
        {
            const bool showAlpha = true;
            using (var colorChooser = new ColorChooser { Color = panelColor.BackColor, ShowAlpha = showAlpha })
            {
                if (colorChooser.ShowDialog() == DialogResult.OK)
                {
                    panelColor.BackColor = colorChooser.Color;
                    textBoxText_TextChanged(null, null);
                }
            }
        }

        private void buttonBorderColor_Click(object sender, EventArgs e)
        {
            using (var colorChooser = new ColorChooser { Color = panelBorderColor.BackColor })
            {
                if (colorChooser.ShowDialog() == DialogResult.OK)
                {
                    panelBorderColor.BackColor = colorChooser.Color;
                    textBoxText_TextChanged(null, null);
                }
            }
        }

        private void buttonShadowColor_Click(object sender, EventArgs e)
        {
            using (var colorChooser = new ColorChooser { Color = panelShadowColor.BackColor })
            {
                if (colorChooser.ShowDialog() == DialogResult.OK)
                {
                    panelShadowColor.BackColor = colorChooser.Color;
                    textBoxText_TextChanged(null, null);
                    numericUpDownShadowTransparency.Value = colorChooser.Color.A;
                }
            }
        }

        private void numericUpDownLineSpacing_ValueChanged(object sender, EventArgs e)
        {
            _lineHeights.Clear();
            _lineHeights.Add(string.Empty, (int)numericUpDownLineSpacing.Value);
            textBoxText_TextChanged(null, null);
        }

        private float GetFontHeight()
        {
            if (comboBoxSubtitleFont.SelectedItem == null || comboBoxSubtitleFontSize.SelectedItem == null)
            {
                return Configuration.Settings.Tools.ExportLastLineHeight;
            }

            var mbp = new MakeBitmapParameter
            {
                SubtitleFontName = comboBoxSubtitleFont.SelectedItem.ToString(),
                SubtitleFontSize = int.Parse(comboBoxSubtitleFontSize.SelectedItem.ToString()),
                SubtitleFontBold = checkBoxBold.Checked,
            };
            var fontSize = (float)TextDraw.GetFontSize(mbp.SubtitleFontSize);
            using (var font = GetFont(mbp, fontSize))
            using (var bmp = new Bitmap(100, 100))
            using (var g = Graphics.FromImage(bmp))
            {
                var textSize = g.MeasureString("Hj!", font);
                return textSize.Height;
            }
        }

        private static Font GetFont(MakeBitmapParameter parameter, float fontSize)
        {
            Font font;
            try
            {
                var fontStyle = FontStyle.Regular;
                if (parameter.SubtitleFontBold)
                {
                    fontStyle = FontStyle.Bold;
                }

                font = new Font(parameter.SubtitleFontName, fontSize, fontStyle);
            }
            catch (Exception exception)
            {
                try
                {
                    var fontStyle = FontStyle.Regular;
                    if (!parameter.SubtitleFontBold)
                    {
                        fontStyle = FontStyle.Bold;
                    }

                    font = new Font(parameter.SubtitleFontName, fontSize, fontStyle);
                }
                catch
                {
                    MessageBox.Show(exception.Message);

                    if (FontFamily.Families[0].IsStyleAvailable(FontStyle.Regular))
                    {
                        font = new Font(FontFamily.Families[0].Name, fontSize);
                    }
                    else if (FontFamily.Families.Length > 1 && FontFamily.Families[1].IsStyleAvailable(FontStyle.Regular))
                    {
                        font = new Font(FontFamily.Families[1].Name, fontSize);
                    }
                    else if (FontFamily.Families.Length > 2 && FontFamily.Families[1].IsStyleAvailable(FontStyle.Regular))
                    {
                        font = new Font(FontFamily.Families[2].Name, fontSize);
                    }
                    else
                    {
                        font = new Font("Arial", fontSize);
                    }
                }
            }
            return font;
        }

        private void comboBoxSubtitleFontSize_SelectedIndexChanged(object sender, EventArgs e)
        {
            var lineHeight = (int)Math.Round(GetFontHeight() * 0.64f);
            if (lineHeight >= numericUpDownLineSpacing.Minimum &&
                lineHeight <= numericUpDownLineSpacing.Maximum)
            {
                numericUpDownLineSpacing.Value = lineHeight;
            }

            textBoxText_TextChanged(null, null);
        }

        private void BinEditNewText_FormClosing(object sender, FormClosingEventArgs e)
        {
            Configuration.Settings.Tools.ExportFontColor = panelColor.BackColor;
            Configuration.Settings.Tools.ExportBorderColor = panelBorderColor.BackColor;
            Configuration.Settings.Tools.ExportShadowColor = panelShadowColor.BackColor;
            Configuration.Settings.Tools.ExportBluRayFontName = comboBoxSubtitleFont.SelectedItem.ToString();
            Configuration.Settings.Tools.ExportBluRayFontSize = int.Parse(comboBoxSubtitleFontSize.SelectedItem.ToString());
            Configuration.Settings.Tools.ExportLastBorderWidth = int.Parse(comboBoxSubtitleFontSize.SelectedItem.ToString());
            Configuration.Settings.Tools.ExportLastBorderWidth = comboBoxBorderWidth.SelectedIndex;
            Configuration.Settings.Tools.ExportLastFontBold = checkBoxBold.Checked;
        }

        private void BinEditNewText_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                DialogResult = DialogResult.Cancel;
            }
        }

        private void textBoxText_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Modifiers == Keys.Control && e.KeyCode == Keys.I)
            {
                if (textBoxText.Text.Contains("<i>", StringComparison.Ordinal))
                {
                    textBoxText.Text = HtmlUtil.RemoveOpenCloseTags(textBoxText.Text.Trim(), HtmlUtil.TagItalic).Trim();
                }
                else
                {
                    textBoxText.Text = $"<i>{textBoxText.Text.Trim()}</i>";
                }
                e.SuppressKeyPress = true;
            }
        }
    }
}
