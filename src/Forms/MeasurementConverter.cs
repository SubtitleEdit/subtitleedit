using System;
using System.Drawing;
using System.Windows.Forms;
using Nikse.SubtitleEdit.Logic;

namespace Nikse.SubtitleEdit.Forms
{
    public partial class MeasurementConverter : Form
    {
        public MeasurementConverter()
        {
            InitializeComponent();

            var l = Configuration.Settings.Language.MeasurementConverter;
            Text = l.Title;
            labelConvertFrom.Text = l.ConvertFrom;
            labelConvertTo.Text = l.ConvertTo;
            linkLabel1.Text = l.CopyToClipboard;
            buttonOK.Text = Configuration.Settings.Language.General.OK;

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
            FixLargeFonts();
        }

        private void FixLargeFonts()
        {
            Graphics graphics = this.CreateGraphics();
            SizeF textSize = graphics.MeasureString(buttonOK.Text, this.Font);
            if (textSize.Height > buttonOK.Height - 4)
            {
                int newButtonHeight = (int)(textSize.Height + 7 + 0.5);
                Utilities.SetButtonHeight(this, newButtonHeight, 1);
            }
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
                comboBoxTo.SelectedIndex = 0;
            numericUpDown1_ValueChanged(null, null);
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            if (comboBoxFrom.SelectedIndex == -1 || comboBoxTo.SelectedIndex == -1)
                return;

            string text = comboBoxFrom.SelectedItem.ToString();
            string textTo = comboBoxTo.SelectedItem.ToString();
            var l = Configuration.Settings.Language.MeasurementConverter;
            if (text == l.Fahrenheit)
            {
                ShowResult((numericUpDown1.Value - 32) * 5 / 9);                               
            }
            else if (text == l.Celsius)
            {
                ShowResult(Convert.ToDouble(numericUpDown1.Value) * 1.80 + 32);
            }

            else if (text == l.Miles)
            {
                if (textTo == l.Kilometers)
                {
                    ShowResult(Convert.ToDouble(numericUpDown1.Value) * 0.621371192);
                } 
                

                //comboBoxTo.Items.Add(l.Kilometers);
                //comboBoxTo.Items.Add(l.Meters);
                //comboBoxTo.Items.Add(l.Yards);
                //comboBoxTo.Items.Add(l.Feet);
                //comboBoxTo.Items.Add(l.Inches);
            }
            else if (text == l.Kilometers)
            {
                if (textTo == l.Miles)
                {
                    ShowResult(Convert.ToDouble(numericUpDown1.Value) / 0.621371192);
                }

                //comboBoxTo.Items.Add(l.Miles);
                //comboBoxTo.Items.Add(l.Meters);
                //comboBoxTo.Items.Add(l.Yards);
                //comboBoxTo.Items.Add(l.Feet);
                //comboBoxTo.Items.Add(l.Inches);
            }
            else if (text == l.Meters)
            {
                //comboBoxTo.Items.Add(l.Miles);
                //comboBoxTo.Items.Add(l.Kilometers);
                //comboBoxTo.Items.Add(l.Yards);
                //comboBoxTo.Items.Add(l.Feet);
                //comboBoxTo.Items.Add(l.Inches);
            }
            else if (text == l.Yards)
            {
                //comboBoxTo.Items.Add(l.Miles);
                //comboBoxTo.Items.Add(l.Kilometers);
                //comboBoxTo.Items.Add(l.Meters);
                //comboBoxTo.Items.Add(l.Feet);
                //comboBoxTo.Items.Add(l.Inches);
            }
            else if (text == l.Feet)
            {
                //comboBoxTo.Items.Add(l.Miles);
                //comboBoxTo.Items.Add(l.Kilometers);
                //comboBoxTo.Items.Add(l.Meters);
                //comboBoxTo.Items.Add(l.Yards);
                //comboBoxTo.Items.Add(l.Inches);
            }
            else if (text == l.Inches)
            {
                //comboBoxTo.Items.Add(l.Miles);
                //comboBoxTo.Items.Add(l.Kilometers);
                //comboBoxTo.Items.Add(l.Meters);
                //comboBoxTo.Items.Add(l.Yards);
                //comboBoxTo.Items.Add(l.Feet);
            }

            else if (text == l.Pounds)
            {
                ShowResult(Convert.ToDouble(numericUpDown1.Value) * 0.45359237);
            }
            else if (text == l.Kilos)
            {
                 ShowResult(Convert.ToDouble(numericUpDown1.Value) / 0.45359237);
            }
        }

        private void ShowResult(double d)
        {
            textBoxResult.Text = string.Format("{0:0.##}", d);
        }

        private void ShowResult(decimal d)
        {
            textBoxResult.Text = string.Format("{0:0.##}", d);
        }

        private void numericUpDown1_KeyUp(object sender, KeyEventArgs e)
        {
            numericUpDown1_ValueChanged(null, null);
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (textBoxResult.Text.Length > 0)
                Clipboard.SetText(textBoxResult.Text);
        }

        private void MeasurementConverter_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
                this.Close();
        }

    }
}
