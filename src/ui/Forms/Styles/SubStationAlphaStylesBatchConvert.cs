﻿using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Logic;
using System;
using System.Drawing;
using System.Globalization;
using System.Text;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.Forms.Styles
{
    public sealed partial class SubStationAlphaStylesBatchConvert : StylesForm
    {

        private string _header;
        private bool _doUpdate;
        private readonly bool _isSubStationAlpha;
        private Bitmap _backgroundImage;
        private bool _backgroundImageDark;

        public SubStationAlphaStylesBatchConvert(Subtitle subtitle, SubtitleFormat format)
            : base(subtitle)
        {
            UiUtil.PreInitialize(this);
            InitializeComponent();
            UiUtil.FixFonts(this);

            comboBoxWrapStyle.SelectedIndex = 2;
            comboBoxCollision.SelectedIndex = 0;
            _header = subtitle.Header;
            _isSubStationAlpha = format.Name == SubStationAlpha.NameOfFormat;
            _backgroundImageDark = Configuration.Settings.General.UseDarkTheme;
            if (_header == null || !_header.Contains("style:", StringComparison.OrdinalIgnoreCase))
            {
                ResetHeader();
            }

            comboBoxFontName.Items.Clear();
            foreach (var x in FontHelper.GetAllSupportedFontFamilies())
            {
                comboBoxFontName.Items.Add(x.Name);
            }

            var l = LanguageSettings.Current.SubStationAlphaStyles;
            Text = l.Title;
            groupBoxProperties.Text = l.Properties;
            groupBoxFont.Text = l.Font;
            labelFontName.Text = l.FontName;
            labelFontSize.Text = l.FontSize;
            checkBoxFontItalic.Text = LanguageSettings.Current.General.Italic;
            checkBoxFontBold.Text = LanguageSettings.Current.General.Bold;
            checkBoxFontUnderline.Text = LanguageSettings.Current.General.Underline;
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
            labelMarginVertical.Text = l.MarginVertical;
            groupBoxBorder.Text = l.Border;
            radioButtonOutline.Text = l.Outline;
            labelShadow.Text = l.PlusShadow;
            radioButtonOpaqueBox.Text = l.OpaqueBox;
            groupBoxPreview.Text = LanguageSettings.Current.General.Preview;

            if (_isSubStationAlpha)
            {
                Text = l.TitleSubstationAlpha;
                buttonOutlineColor.Text = l.Tertiary;
                buttonBackColor.Text = l.Back;
                checkBoxFontUnderline.Visible = false;

                labelWrapStyle.Visible = false;
                comboBoxWrapStyle.Visible = false;
                checkBoxScaleBorderAndShadow.Visible = false;

                numericUpDownOutline.Increment = 1;
                numericUpDownOutline.DecimalPlaces = 0;
                numericUpDownShadowWidth.Increment = 1;
                numericUpDownShadowWidth.DecimalPlaces = 0;
            }

            buttonOK.Text = LanguageSettings.Current.General.Ok;
            buttonCancel.Text = LanguageSettings.Current.General.Cancel;

            UiUtil.FixLargeFonts(this, buttonCancel);

            comboBoxFontName.Left = labelFontName.Left + labelFontName.Width + 10;
            numericUpDownFontSize.Left = labelFontSize.Left + labelFontSize.Width + 10;
            if (comboBoxFontName.Left > numericUpDownFontSize.Left)
            {
                numericUpDownFontSize.Left = comboBoxFontName.Left;
            }
            else
            {
                comboBoxFontName.Left = numericUpDownFontSize.Left;
            }

            numericUpDownOutline.Left = radioButtonOutline.Left + radioButtonOutline.Width + 5;
            labelShadow.Left = numericUpDownOutline.Left + numericUpDownOutline.Width + 5;
            numericUpDownShadowWidth.Left = labelShadow.Left + labelShadow.Width + 5;
            checkBoxFontItalic.Left = checkBoxFontBold.Left + checkBoxFontBold.Width + 12;
            checkBoxFontUnderline.Left = checkBoxFontItalic.Left + checkBoxFontItalic.Width + 12;

            var l2 = LanguageSettings.Current.SubStationAlphaProperties;
            groupBoxResolution.Text = l2.Resolution;
            labelVideoResolution.Text = l2.VideoResolution;
            groupBoxOptions.Text = l2.Options;
            labelCollision.Text = l2.Collision;
            labelWrapStyle.Text = l2.WrapStyle;
            checkBoxScaleBorderAndShadow.Text = l2.ScaleBorderAndShadow;
        }

        private SsaStyle GetSsaStyle(string styleName)
        {
            return AdvancedSubStationAlpha.GetSsaStyle(styleName, _header);
        }

        private void SetSsaStyle(string styleName, string propertyName, string propertyValue)
        {
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
                        var format = line.Substring(6).Split(',');
                        for (int i = 0; i < format.Length; i++)
                        {
                            string f = format[i].Trim();
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

        private void buttonOK_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void SubStationAlphaStylesBatchConvert_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                DialogResult = DialogResult.Cancel;
            }
        }

        private void buttonReset_Click(object sender, EventArgs e)
        {
            comboBoxWrapStyle.SelectedIndex = 2;
            comboBoxCollision.SelectedIndex = 0;
            numericUpDownVideoWidth.Value = 0;
            numericUpDownVideoHeight.Value = 0;
            checkBoxScaleBorderAndShadow.Checked = false;

            ResetHeader();
            textBoxRawHeader_Validated(sender, e);
        }

        private void buttonGetResolutionFromVideo_Click(object sender, EventArgs e)
        {
            openFileDialog1.Title = LanguageSettings.Current.General.OpenVideoFileTitle;
            openFileDialog1.FileName = string.Empty;
            openFileDialog1.Filter = UiUtil.GetVideoFileFilter(false);
            openFileDialog1.FileName = string.Empty;
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                VideoInfo info = UiUtil.GetVideoInfo(openFileDialog1.FileName);
                if (info != null && info.Success)
                {
                    numericUpDownVideoWidth.Value = info.Width;
                    numericUpDownVideoHeight.Value = info.Height;
                }
            }
        }

        private string GetSsaColorString(Color c)
        {
            if (_isSubStationAlpha)
            {
                return Color.FromArgb(0, c.B, c.G, c.R).ToArgb().ToString(CultureInfo.InvariantCulture);
            }

            return AdvancedSubStationAlpha.GetSsaColorString(c);
        }

        private void buttonPrimaryColor_Click(object sender, EventArgs e)
        {
            string name = CurrentStyleName;
            if (_isSubStationAlpha)
            {
                using (var colorChooser = new ColorChooser { Color = panelPrimaryColor.BackColor, ShowAlpha = false })
                {
                    if (colorChooser.ShowDialog() == DialogResult.OK)
                    {
                        panelPrimaryColor.BackColor = colorChooser.Color;
                        SetSsaStyle(name, "primarycolour", GetSsaColorString(colorChooser.Color));
                        GeneratePreviewAndUpdateRawHeader();
                    }
                }
            }
            else
            {
                using (var colorChooser = new ColorChooser { Color = panelPrimaryColor.BackColor })
                {
                    if (colorChooser.ShowDialog() == DialogResult.OK)
                    {
                        panelPrimaryColor.BackColor = colorChooser.Color;
                        SetSsaStyle(name, "primarycolour", GetSsaColorString(panelPrimaryColor.BackColor));
                        GeneratePreviewAndUpdateRawHeader();
                    }
                }
            }
        }

        private void buttonSecondaryColor_Click(object sender, EventArgs e)
        {
            string name = CurrentStyleName;
            if (_isSubStationAlpha)
            {
                using (var colorChooser = new ColorChooser { Color = panelSecondaryColor.BackColor, ShowAlpha = false })
                {
                    if (colorChooser.ShowDialog() == DialogResult.OK)
                    {
                        panelSecondaryColor.BackColor = colorChooser.Color;
                        SetSsaStyle(name, "secondarycolour", GetSsaColorString(colorChooser.Color));
                        GeneratePreviewAndUpdateRawHeader();
                    }
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
                        GeneratePreviewAndUpdateRawHeader();
                    }
                }
            }
        }

        private void buttonOutlineColor_Click(object sender, EventArgs e)
        {
            string name = CurrentStyleName;
            if (_isSubStationAlpha)
            {
                using (var colorChooser = new ColorChooser { Color = panelOutlineColor.BackColor, ShowAlpha = false })
                {
                    if (colorChooser.ShowDialog() == DialogResult.OK)
                    {
                        panelOutlineColor.BackColor = colorChooser.Color;
                        SetSsaStyle(name, "tertiarycolour", GetSsaColorString(colorChooser.Color));
                        GeneratePreviewAndUpdateRawHeader();
                    }
                }
            }
            else
            {
                using (var colorChooser = new ColorChooser { Color = panelOutlineColor.BackColor })
                {
                    if (colorChooser.ShowDialog() == DialogResult.OK)
                    {
                        panelOutlineColor.BackColor = colorChooser.Color;
                        SetSsaStyle(name, "outlinecolour", GetSsaColorString(panelOutlineColor.BackColor));
                        GeneratePreviewAndUpdateRawHeader();
                    }
                }
            }
        }

        private void buttonBackColor_Click(object sender, EventArgs e)
        {
            string name = CurrentStyleName;
            if (_isSubStationAlpha)
            {
                using (var colorChooser = new ColorChooser { Color = panelBackColor.BackColor, ShowAlpha = false })
                {
                    if (colorChooser.ShowDialog() == DialogResult.OK)
                    {
                        panelBackColor.BackColor = colorChooser.Color;
                        SetSsaStyle(name, "backcolour", GetSsaColorString(colorChooser.Color));
                        GeneratePreviewAndUpdateRawHeader();
                    }
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
                        GeneratePreviewAndUpdateRawHeader();
                    }
                }
            }
        }

        private void comboBoxFontName_TextChanged(object sender, EventArgs e)
        {
            var text = comboBoxFontName.Text;
            if (_doUpdate && !string.IsNullOrEmpty(text))
            {
                string name = CurrentStyleName;
                SetSsaStyle(name, "fontname", text);
                GeneratePreview();
            }
        }

        private void comboBoxFontName_KeyUp(object sender, KeyEventArgs e)
        {
            if (_doUpdate)
            {
                string name = CurrentStyleName;
                var item = comboBoxFontName.SelectedItem;
                if (item != null)
                {
                    SetSsaStyle(name, "fontname", item.ToString());
                }

                GeneratePreviewAndUpdateRawHeader();
            }
        }

        private void numericUpDownFontSize_ValueChanged(object sender, EventArgs e)
        {
            if (_doUpdate)
            {
                string name = CurrentStyleName;
                SetSsaStyle(name, "fontsize", numericUpDownFontSize.Value.ToString(CultureInfo.InvariantCulture));
                GeneratePreviewAndUpdateRawHeader();
            }
        }

        private void checkBoxFontBold_CheckedChanged(object sender, EventArgs e)
        {
            if (_doUpdate)
            {
                string name = CurrentStyleName;
                if (checkBoxFontBold.Checked)
                {
                    SetSsaStyle(name, "bold", "-1");
                }
                else
                {
                    SetSsaStyle(name, "bold", "0");
                }

                GeneratePreviewAndUpdateRawHeader();
            }
        }

        private void checkBoxFontItalic_CheckedChanged(object sender, EventArgs e)
        {
            if (_doUpdate)
            {
                string name = CurrentStyleName;
                if (checkBoxFontItalic.Checked)
                {
                    SetSsaStyle(name, "italic", "-1");
                }
                else
                {
                    SetSsaStyle(name, "italic", "0");
                }

                GeneratePreviewAndUpdateRawHeader();
            }
        }

        private void checkBoxFontUnderline_CheckedChanged(object sender, EventArgs e)
        {
            if (_doUpdate)
            {
                string name = CurrentStyleName;
                if (checkBoxFontUnderline.Checked)
                {
                    SetSsaStyle(name, "underline", "-1");
                }
                else
                {
                    SetSsaStyle(name, "underline", "0");
                }

                GeneratePreviewAndUpdateRawHeader();
            }
        }

        private void radioButtonTopLeft_CheckedChanged(object sender, EventArgs e)
        {
            if (_doUpdate && ((RadioButton)sender).Checked)
            {
                string name = CurrentStyleName;
                SetSsaStyle(name, "alignment", _isSubStationAlpha ? "5" : "7");
                GeneratePreviewAndUpdateRawHeader();
            }
        }

        private void radioButtonTopCenter_CheckedChanged(object sender, EventArgs e)
        {
            if (_doUpdate && ((RadioButton)sender).Checked)
            {
                string name = CurrentStyleName;
                SetSsaStyle(name, "alignment", _isSubStationAlpha ? "6" : "8");
                GeneratePreviewAndUpdateRawHeader();
            }
        }

        private void radioButtonTopRight_CheckedChanged(object sender, EventArgs e)
        {
            if (_doUpdate && ((RadioButton)sender).Checked)
            {
                string name = CurrentStyleName;
                SetSsaStyle(name, "alignment", _isSubStationAlpha ? "7" : "9");
                GeneratePreviewAndUpdateRawHeader();
            }
        }

        private void radioButtonMiddleLeft_CheckedChanged(object sender, EventArgs e)
        {
            if (_doUpdate && ((RadioButton)sender).Checked)
            {
                string name = CurrentStyleName;
                SetSsaStyle(name, "alignment", _isSubStationAlpha ? "9" : "4");
                GeneratePreviewAndUpdateRawHeader();
            }
        }

        private void radioButtonMiddleCenter_CheckedChanged(object sender, EventArgs e)
        {
            if (_doUpdate && ((RadioButton)sender).Checked)
            {
                string name = CurrentStyleName;
                SetSsaStyle(name, "alignment", _isSubStationAlpha ? "10" : "5");
                GeneratePreviewAndUpdateRawHeader();
            }
        }

        private void radioButtonMiddleRight_CheckedChanged(object sender, EventArgs e)
        {
            if (_doUpdate && ((RadioButton)sender).Checked)
            {
                string name = CurrentStyleName;
                SetSsaStyle(name, "alignment", _isSubStationAlpha ? "11" : "6");
                GeneratePreviewAndUpdateRawHeader();
            }
        }

        private void radioButtonBottomLeft_CheckedChanged(object sender, EventArgs e)
        {
            if (_doUpdate && ((RadioButton)sender).Checked)
            {
                SetSsaStyle(CurrentStyleName, "alignment", "1");
                GeneratePreviewAndUpdateRawHeader();
            }
        }

        private void radioButtonBottomCenter_CheckedChanged(object sender, EventArgs e)
        {
            if (_doUpdate && ((RadioButton)sender).Checked)
            {
                SetSsaStyle(CurrentStyleName, "alignment", "2");
                GeneratePreviewAndUpdateRawHeader();
            }
        }

        private void radioButtonBottomRight_CheckedChanged(object sender, EventArgs e)
        {
            if (_doUpdate && ((RadioButton)sender).Checked)
            {
                SetSsaStyle(CurrentStyleName, "alignment", "3");
                GeneratePreviewAndUpdateRawHeader();
            }
        }

        private void numericUpDownMarginLeft_ValueChanged(object sender, EventArgs e)
        {
            if (_doUpdate)
            {
                SetSsaStyle(CurrentStyleName, "marginl", numericUpDownMarginLeft.Value.ToString(CultureInfo.InvariantCulture));
                GeneratePreviewAndUpdateRawHeader();
            }
        }

        private void numericUpDownMarginRight_ValueChanged(object sender, EventArgs e)
        {
            if (_doUpdate)
            {
                SetSsaStyle(CurrentStyleName, "marginr", numericUpDownMarginRight.Value.ToString(CultureInfo.InvariantCulture));
                GeneratePreviewAndUpdateRawHeader();
            }
        }

        private void numericUpDownMarginVertical_ValueChanged(object sender, EventArgs e)
        {
            if (_doUpdate)
            {
                SetSsaStyle(CurrentStyleName, "marginv", numericUpDownMarginVertical.Value.ToString(CultureInfo.InvariantCulture));
                GeneratePreviewAndUpdateRawHeader();
            }
        }

        private void radioButtonOutline_CheckedChanged(object sender, EventArgs e)
        {
            if (sender is RadioButton rb && _doUpdate && rb.Checked)
            {
                string name = CurrentStyleName;
                SetSsaStyle(name, "outline", numericUpDownOutline.Value.ToString(CultureInfo.InvariantCulture));
                SetSsaStyle(name, "borderstyle", "1");
                GeneratePreviewAndUpdateRawHeader();
            }
        }

        private void numericUpDownOutline_ValueChanged(object sender, EventArgs e)
        {
            if (_doUpdate)
            {
                SetSsaStyle(CurrentStyleName, "outline", numericUpDownOutline.Value.ToString(CultureInfo.InvariantCulture));
                GeneratePreviewAndUpdateRawHeader();
            }
        }

        private void numericUpDownShadowWidth_ValueChanged(object sender, EventArgs e)
        {
            if (_doUpdate)
            {
                SetSsaStyle(CurrentStyleName, "shadow", numericUpDownShadowWidth.Value.ToString(CultureInfo.InvariantCulture));
                GeneratePreviewAndUpdateRawHeader();
            }
        }

        private string CurrentStyleName
        {
            get
            {
                //TODO: fix
                return "Default";
            }
        }

        public override string Header
        {
            get
            {
                return _header;
            }
        }

        private void GeneratePreviewAndUpdateRawHeader()
        {
            GeneratePreview();
            UpdateRawHeader();
        }

        private void UpdateRawHeader()
        {
            if (_header == null)
            {
                textBoxRawHeader.Text = string.Empty;
            }
            else
            {
                textBoxRawHeader.Text = _header.Replace("[Events]", string.Empty).TrimEnd();
            }
        }

        private void UpdatePropertiesTag(string tag, string text, bool remove)
        {
            if (_header == null)
            {
                return;
            }

            bool scriptInfoOn = false;
            var sb = new StringBuilder();
            bool found = false;
            foreach (string line in _header.SplitToLines())
            {
                if (line.StartsWith("[script info]", StringComparison.OrdinalIgnoreCase))
                {
                    scriptInfoOn = true;
                }
                else if (line.StartsWith('['))
                {
                    if (!found && scriptInfoOn && !remove)
                    {
                        sb = new StringBuilder(sb.ToString().Trim() + Environment.NewLine);
                        sb.AppendLine(tag + ": " + text);
                    }
                    sb = new StringBuilder(sb.ToString().TrimEnd());
                    sb.AppendLine();
                    sb.AppendLine();
                    scriptInfoOn = false;
                }

                string s = line.ToLowerInvariant();
                if (s.StartsWith(tag.ToLowerInvariant() + ":", StringComparison.Ordinal))
                {
                    if (!remove)
                    {
                        sb.AppendLine(line.Substring(0, tag.Length) + ": " + text);
                    }

                    found = true;
                }
                else
                {
                    sb.AppendLine(line);
                }
            }
            _header = sb.ToString().Trim();
        }

        private void NumericUpDownVideoWidthOrHeightValueChanged(object sender, EventArgs e)
        {
            UpdatePropertiesTag("PlayResX", numericUpDownVideoWidth.Value.ToString(CultureInfo.InvariantCulture), numericUpDownVideoWidth.Value == 0);
            UpdatePropertiesTag("PlayResY", numericUpDownVideoHeight.Value.ToString(CultureInfo.InvariantCulture), numericUpDownVideoHeight.Value == 0);
            UpdateRawHeader();
        }
        private void comboBoxCollision_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBoxCollision.SelectedIndex == 0)
            {
                UpdatePropertiesTag("collisions", "Normal", false); // normal
            }
            else
            {
                UpdatePropertiesTag("collisions", "Reverse", false); // reverse
            }

            UpdateRawHeader();
        }

        private void comboBoxWrapStyle_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!_isSubStationAlpha)
            {
                UpdatePropertiesTag("WrapStyle", comboBoxWrapStyle.SelectedIndex.ToString(CultureInfo.InvariantCulture), false);
            }
            UpdateRawHeader();
        }

        private void checkBoxScaleBorderAndShadow_CheckedChanged(object sender, EventArgs e)
        {
            if (!_isSubStationAlpha)
            {
                if (checkBoxScaleBorderAndShadow.Checked)
                {
                    UpdatePropertiesTag("ScaledBorderAndShadow", "Yes", false);
                }
                else
                {
                    UpdatePropertiesTag("ScaledBorderAndShadow", "No", false);
                }
            }
            UpdateRawHeader();
        }

        private void SubStationAlphaStylesBatchConvert_ResizeEnd(object sender, EventArgs e)
        {
            GeneratePreview();
        }

        private void SubStationAlphaStylesBatchConvert_SizeChanged(object sender, EventArgs e)
        {
            GeneratePreview();
        }

        private void SubStationAlphaStylesBatchConvert_Shown(object sender, EventArgs e)
        {
            string styleName = CurrentStyleName;
            SsaStyle style = GetSsaStyle(styleName);
            SetControlsFromStyle(style);
            SetControlsFromHeader();
            _doUpdate = true;
            groupBoxProperties.Enabled = true;
            GeneratePreviewAndUpdateRawHeader();
        }

        private void SetControlsFromHeader()
        {
            if (_header != null)
            {
                foreach (string line in _header.SplitToLines())
                {
                    string s = line.ToLowerInvariant().Trim();
                    if (s.StartsWith("collisions:", StringComparison.Ordinal))
                    {
                        if (s.Remove(0, 11).Trim() == "reverse")
                        {
                            comboBoxCollision.SelectedIndex = 1;
                        }
                    }
                    else if (s.StartsWith("playresx:", StringComparison.Ordinal))
                    {
                        if (int.TryParse(s.Remove(0, 9).Trim(), out var number))
                        {
                            numericUpDownVideoWidth.Value = number;
                        }
                    }
                    else if (s.StartsWith("playresy:", StringComparison.Ordinal))
                    {
                        if (int.TryParse(s.Remove(0, 9).Trim(), out var number))
                        {
                            numericUpDownVideoHeight.Value = number;
                        }
                    }
                    else if (s.StartsWith("scaledborderandshadow:", StringComparison.Ordinal))
                    {
                        checkBoxScaleBorderAndShadow.Checked = s.Remove(0, 22).Trim().ToLowerInvariant().Equals("yes");
                    }
                    else if (s.StartsWith("wrapstyle:", StringComparison.Ordinal))
                    {
                        var wrapStyle = s.Remove(0, 10).Trim();
                        for (int i = 0; i < comboBoxWrapStyle.Items.Count; i++)
                        {
                            if (i.ToString(CultureInfo.InvariantCulture) == wrapStyle)
                            {
                                comboBoxWrapStyle.SelectedIndex = i;
                            }
                        }
                    }
                }
            }
        }

        private void SetControlsFromStyle(SsaStyle style)
        {
            comboBoxFontName.Text = style.FontName;
            checkBoxFontItalic.Checked = style.Italic;
            checkBoxFontBold.Checked = style.Bold;
            checkBoxFontUnderline.Checked = style.Underline;

            if (style.FontSize > 0 && style.FontSize <= numericUpDownFontSize.Maximum)
            {
                numericUpDownFontSize.Value = style.FontSize;
            }
            else
            {
                numericUpDownFontSize.Value = 20;
            }

            panelPrimaryColor.BackColor = style.Primary;
            panelSecondaryColor.BackColor = style.Secondary;
            if (_isSubStationAlpha)
            {
                panelOutlineColor.BackColor = style.Tertiary;
            }
            else
            {
                panelOutlineColor.BackColor = style.Outline;
            }

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

        protected override void GeneratePreviewReal()
        {
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

        private void radioButtonOpaqueBox_CheckedChanged(object sender, EventArgs e)
        {
            if (sender is RadioButton rb && _doUpdate && rb.Checked)
            {
                string name = CurrentStyleName;
                SetSsaStyle(name, "outline", numericUpDownOutline.Value.ToString(CultureInfo.InvariantCulture));
                SetSsaStyle(name, "borderstyle", "3");
                GeneratePreview();
            }
        }

        private void textBoxRawHeader_TextChanged(object sender, EventArgs e)
        {
            if (_doUpdate)
            {
                _header = textBoxRawHeader.Text.Trim() + Environment.NewLine + Environment.NewLine + "[Events]";
                SetControlsFromHeader();
            }
        }

        private void textBoxRawHeader_Validated(object sender, EventArgs e)
        {
            _doUpdate = false;
            SsaStyle style = GetSsaStyle(CurrentStyleName);
            SetControlsFromStyle(style);
            _doUpdate = true;
            GeneratePreview();
        }

        private void pictureBoxPreview_Click(object sender, EventArgs e)
        {
            _backgroundImageDark = !_backgroundImageDark;
            _backgroundImage?.Dispose();
            _backgroundImage = null;
            GeneratePreview();
        }
    }
}
