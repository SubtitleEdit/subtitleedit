using Nikse.SubtitleEdit.Core;
using Nikse.SubtitleEdit.Logic;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.Forms
{
    public sealed partial class MeasurementConverter : Form
    {
        private readonly Color _defaultBackColor;
        public MeasurementConverter()
        {
            UiUtil.PreInitialize(this);
            InitializeComponent();
            UiUtil.FixFonts(this);
            _defaultBackColor = Color.White;
            var l = Configuration.Settings.Language.MeasurementConverter;
            Text = l.Title;
            labelConvertFrom.Text = l.ConvertFrom;
            labelConvertTo.Text = l.ConvertTo;
            linkLabel1.Text = l.CopyToClipboard;
            buttonOK.Text = Configuration.Settings.Language.General.Ok;

            _defaultBackColor = textBoxInput.BackColor;
            textBoxInput.Text = "1";

            comboBoxFrom.Items.Add(l.Fahrenheit);
            comboBoxFrom.Items.Add(l.Celsius);

            comboBoxFrom.Items.Add(l.Miles);
            comboBoxFrom.Items.Add(l.Kilometers);
            comboBoxFrom.Items.Add(l.Meters);
            comboBoxFrom.Items.Add(l.Yards);
            comboBoxFrom.Items.Add(l.Feet);
            comboBoxFrom.Items.Add(l.Inches);

            comboBoxFrom.Items.Add(l.Pounds);
            comboBoxFrom.Items.Add(l.Kilos);

            comboBoxFrom.SelectedIndex = 0;

            UiUtil.FixLargeFonts(this, buttonOK);
        }

        private void comboBoxFrom_SelectedIndexChanged(object sender, EventArgs e)
        {
            string text = comboBoxFrom.SelectedItem.ToString();
            comboBoxTo.Items.Clear();
            var l = Configuration.Settings.Language.MeasurementConverter;
            if (text == l.Fahrenheit)
            {
                comboBoxTo.Items.Add(l.Celsius);
            }
            else if (text == l.Celsius)
            {
                comboBoxTo.Items.Add(l.Fahrenheit);
            }
            else if (text == l.Miles)
            {
                comboBoxTo.Items.Add(l.Kilometers);
                comboBoxTo.Items.Add(l.Meters);
                comboBoxTo.Items.Add(l.Yards);
                comboBoxTo.Items.Add(l.Feet);
                comboBoxTo.Items.Add(l.Inches);
            }
            else if (text == l.Kilometers)
            {
                comboBoxTo.Items.Add(l.Miles);
                comboBoxTo.Items.Add(l.Meters);
                comboBoxTo.Items.Add(l.Yards);
                comboBoxTo.Items.Add(l.Feet);
                comboBoxTo.Items.Add(l.Inches);
            }
            else if (text == l.Meters)
            {
                comboBoxTo.Items.Add(l.Miles);
                comboBoxTo.Items.Add(l.Kilometers);
                comboBoxTo.Items.Add(l.Yards);
                comboBoxTo.Items.Add(l.Feet);
                comboBoxTo.Items.Add(l.Inches);
            }
            else if (text == l.Yards)
            {
                comboBoxTo.Items.Add(l.Miles);
                comboBoxTo.Items.Add(l.Kilometers);
                comboBoxTo.Items.Add(l.Meters);
                comboBoxTo.Items.Add(l.Feet);
                comboBoxTo.Items.Add(l.Inches);
            }
            else if (text == l.Feet)
            {
                comboBoxTo.Items.Add(l.Miles);
                comboBoxTo.Items.Add(l.Kilometers);
                comboBoxTo.Items.Add(l.Meters);
                comboBoxTo.Items.Add(l.Yards);
                comboBoxTo.Items.Add(l.Inches);
            }
            else if (text == l.Inches)
            {
                comboBoxTo.Items.Add(l.Miles);
                comboBoxTo.Items.Add(l.Kilometers);
                comboBoxTo.Items.Add(l.Meters);
                comboBoxTo.Items.Add(l.Yards);
                comboBoxTo.Items.Add(l.Feet);
            }
            else if (text == l.Pounds)
            {
                comboBoxTo.Items.Add(l.Kilos);
            }
            else if (text == l.Kilos)
            {
                comboBoxTo.Items.Add(l.Pounds);
            }
            if (comboBoxTo.Items.Count > 0)
            {
                comboBoxTo.SelectedIndex = 0;
            }

            textBoxInput_TextChanged(null, null);
        }

        private void ShowResult(double d)
        {
            textBoxResult.Text = $"{d:0.##}";
        }

        private void comboBoxTo_SelectedIndexChanged(object sender, EventArgs e)
        {
            textBoxInput_TextChanged(null, null);
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (textBoxResult.Text.Length > 0)
            {
                Clipboard.SetText(textBoxResult.Text);
            }
        }

        private void textBoxInput_TextChanged(object sender, EventArgs e)
        {
            if (comboBoxFrom.SelectedIndex == -1 || comboBoxTo.SelectedIndex == -1)
            {
                return;
            }

            if (!double.TryParse(textBoxInput.Text, out var d))
            {
                textBoxInput.BackColor = Configuration.Settings.Tools.ListViewSyntaxErrorColor;
                return;
            }
            textBoxInput.BackColor = _defaultBackColor;

            string text = comboBoxFrom.SelectedItem.ToString();
            string textTo = comboBoxTo.SelectedItem.ToString();
            var l = Configuration.Settings.Language.MeasurementConverter;
            if (text == l.Fahrenheit)
            {
                ShowResult((d - 32) * 5 / 9);
            }
            else if (text == l.Celsius)
            {
                ShowResult(Convert.ToDouble(d) * 1.80 + 32);
            }
            else if (text == l.Miles)
            {
                if (textTo == l.Kilometers)
                {
                    ShowResult(Convert.ToDouble(d) / 0.621371192);
                }
                else if (textTo == l.Meters)
                {
                    ShowResult(Convert.ToDouble(d) / 0.000621371192);
                }
                else if (textTo == l.Yards)
                {
                    ShowResult(Convert.ToDouble(d) * 1760);
                }
                else if (textTo == l.Feet)
                {
                    ShowResult(Convert.ToDouble(d) * 5280);
                }
                else if (textTo == l.Inches)
                {
                    ShowResult(Convert.ToDouble(d) * 63360);
                }
            }
            else if (text == l.Kilometers)
            {
                if (textTo == l.Miles)
                {
                    ShowResult(Convert.ToDouble(d) * 0.621371192);
                }
                else if (textTo == l.Yards)
                {
                    ShowResult(Convert.ToDouble(d) * 1093.61);
                }
                else if (textTo == l.Meters)
                {
                    ShowResult(Convert.ToDouble(d) * TimeCode.BaseUnit);
                }
                else if (textTo == l.Feet)
                {
                    ShowResult(Convert.ToDouble(d) / 0.0003048);
                }
                else if (textTo == l.Inches)
                {
                    ShowResult(Convert.ToDouble(d) * 39370.0787);
                }
            }
            else if (text == l.Meters)
            {
                if (textTo == l.Miles)
                {
                    ShowResult(Convert.ToDouble(d) * 0.000621371192);
                }
                else if (textTo == l.Kilometers)
                {
                    ShowResult(Convert.ToDouble(d) / TimeCode.BaseUnit);
                }
                else if (textTo == l.Yards)
                {
                    ShowResult(Convert.ToDouble(d) * 1.0936133);
                }
                else if (textTo == l.Feet)
                {
                    ShowResult(Convert.ToDouble(d) * 3.28084);
                }
                else if (textTo == l.Inches)
                {
                    ShowResult(Convert.ToDouble(d) * 39.3700787);
                }
            }
            else if (text == l.Yards)
            {
                if (textTo == l.Kilometers)
                {
                    ShowResult(Convert.ToDouble(d) * 0.0009144);
                }
                else if (textTo == l.Miles)
                {
                    ShowResult(Convert.ToDouble(d) * 0.000568181818);
                }
                else if (textTo == l.Meters)
                {
                    ShowResult(Convert.ToDouble(d) * 0.9144);
                }
                else if (textTo == l.Feet)
                {
                    ShowResult(Convert.ToDouble(d) * 3);
                }
                else if (textTo == l.Inches)
                {
                    ShowResult(Convert.ToDouble(d) * 36);
                }
            }
            else if (text == l.Feet)
            {
                if (textTo == l.Kilometers)
                {
                    ShowResult(Convert.ToDouble(d) * 0.0003048);
                }
                else if (textTo == l.Miles)
                {
                    ShowResult(Convert.ToDouble(d) / 5280);
                }
                else if (textTo == l.Meters)
                {
                    ShowResult(Convert.ToDouble(d) * 0.3048);
                }
                else if (textTo == l.Yards)
                {
                    ShowResult(Convert.ToDouble(d) / 3);
                }
                else if (textTo == l.Inches)
                {
                    ShowResult(Convert.ToDouble(d) * 12);
                }
            }
            else if (text == l.Inches)
            {
                if (textTo == l.Kilometers)
                {
                    ShowResult(Convert.ToDouble(d) / 39370.0787);
                }
                else if (textTo == l.Miles)
                {
                    ShowResult(Convert.ToDouble(d) / 63360);
                }
                else if (textTo == l.Meters)
                {
                    ShowResult(Convert.ToDouble(d) * 0.0254);
                }
                else if (textTo == l.Yards)
                {
                    ShowResult(Convert.ToDouble(d) * 0.0277777778);
                }
                else if (textTo == l.Feet)
                {
                    ShowResult(Convert.ToDouble(d) * 0.0833333333);
                }
            }
            else if (text == l.Pounds)
            {
                ShowResult(Convert.ToDouble(d) * 0.45359237);
            }
            else if (text == l.Kilos)
            {
                ShowResult(Convert.ToDouble(d) / 0.45359237);
            }
        }

        private void textBoxInput_KeyUp(object sender, KeyEventArgs e)
        {
            textBoxInput_TextChanged(null, null);
        }

        private void textBoxInput_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (Char.IsDigit(e.KeyChar))
            {
                return;
            }

            if (e.KeyChar == Convert.ToChar(Keys.Back) || (e.KeyChar == '.') || (e.KeyChar == ',') || (e.KeyChar == '-'))
            {
                return;
            }

            e.Handled = true;
        }

    }
}
