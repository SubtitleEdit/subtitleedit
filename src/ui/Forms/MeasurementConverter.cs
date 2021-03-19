using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Logic;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.Forms
{
    public sealed partial class MeasurementConverter : Form
    {
        public class InsertEventArgs : EventArgs
        {
            public string Result { get; set; }
            public bool IsOriginalActive { get; set; }
        }

        public event EventHandler<InsertEventArgs> InsertClicked;

        public string Input { get; set; }
        public bool IsOriginalActive { get; set; }

        private string Output { get; set; }

        private readonly Color _defaultBackColor;

        private readonly string[] _fields = Configuration.Settings.General.MeasurementConverterCategories.Split(';');

        private readonly List<string> _length = new List<string> { LanguageSettings.Current.MeasurementConverter.Kilometers,
            LanguageSettings.Current.MeasurementConverter.Meters,
            LanguageSettings.Current.MeasurementConverter.Centimeters,
            LanguageSettings.Current.MeasurementConverter.Millimeters,
            LanguageSettings.Current.MeasurementConverter.Micrometers,
            LanguageSettings.Current.MeasurementConverter.Nanometers,
            LanguageSettings.Current.MeasurementConverter.Angstroms,
            LanguageSettings.Current.MeasurementConverter.MilesTerrestial,
            LanguageSettings.Current.MeasurementConverter.MilesNautical,
            LanguageSettings.Current.MeasurementConverter.Yards,
            LanguageSettings.Current.MeasurementConverter.Feet,
            LanguageSettings.Current.MeasurementConverter.Inches,
            LanguageSettings.Current.MeasurementConverter.Chains,
            LanguageSettings.Current.MeasurementConverter.Fathoms,
            LanguageSettings.Current.MeasurementConverter.Hands,
            LanguageSettings.Current.MeasurementConverter.Rods,
            LanguageSettings.Current.MeasurementConverter.Spans };

        private readonly List<string> _mass = new List<string> { LanguageSettings.Current.MeasurementConverter.LongTonnes,
            LanguageSettings.Current.MeasurementConverter.ShortTonnes,
            LanguageSettings.Current.MeasurementConverter.Tonnes,
            LanguageSettings.Current.MeasurementConverter.Kilos,
            LanguageSettings.Current.MeasurementConverter.Grams,
            LanguageSettings.Current.MeasurementConverter.Milligrams,
            LanguageSettings.Current.MeasurementConverter.Micrograms,
            LanguageSettings.Current.MeasurementConverter.Pounds,
            LanguageSettings.Current.MeasurementConverter.Ounces,
            LanguageSettings.Current.MeasurementConverter.Carats,
            LanguageSettings.Current.MeasurementConverter.Drams,
            LanguageSettings.Current.MeasurementConverter.Grains,
            LanguageSettings.Current.MeasurementConverter.Stones };

        private readonly List<string> _volume = new List<string> { LanguageSettings.Current.MeasurementConverter.CubicKilometers,
            LanguageSettings.Current.MeasurementConverter.CubicMeters,
            LanguageSettings.Current.MeasurementConverter.Litres,
            LanguageSettings.Current.MeasurementConverter.CubicCentimeters,
            LanguageSettings.Current.MeasurementConverter.CubicMillimeters,
            LanguageSettings.Current.MeasurementConverter.CubicMiles,
            LanguageSettings.Current.MeasurementConverter.CubicYards,
            LanguageSettings.Current.MeasurementConverter.CubicFTs,
            LanguageSettings.Current.MeasurementConverter.CubicInches,
            LanguageSettings.Current.MeasurementConverter.OilBarrels,
            LanguageSettings.Current.MeasurementConverter.GallonUS,
            LanguageSettings.Current.MeasurementConverter.QuartsUS,
            LanguageSettings.Current.MeasurementConverter.PintsUS,
            LanguageSettings.Current.MeasurementConverter.FluidOuncesUS,
            LanguageSettings.Current.MeasurementConverter.Bushels,
            LanguageSettings.Current.MeasurementConverter.Pecks,
            LanguageSettings.Current.MeasurementConverter.GallonsUK,
            LanguageSettings.Current.MeasurementConverter.QuartsUK,
            LanguageSettings.Current.MeasurementConverter.PintsUK,
            LanguageSettings.Current.MeasurementConverter.FluidOuncesUK };

        private readonly List<string> _area = new List<string> {LanguageSettings.Current.MeasurementConverter.SquareKilometers,
            LanguageSettings.Current.MeasurementConverter.SquareMeters,
            LanguageSettings.Current.MeasurementConverter.SquareCentimeters,
            LanguageSettings.Current.MeasurementConverter.SquareMillimeters,
            LanguageSettings.Current.MeasurementConverter.SquareMiles,
            LanguageSettings.Current.MeasurementConverter.SquareYards,
            LanguageSettings.Current.MeasurementConverter.SquareFTs,
            LanguageSettings.Current.MeasurementConverter.SquareInches,
            LanguageSettings.Current.MeasurementConverter.Hectares,
            LanguageSettings.Current.MeasurementConverter.Acres,
            LanguageSettings.Current.MeasurementConverter.Ares };

        private readonly List<string> _time = new List<string> { LanguageSettings.Current.MeasurementConverter.Hours,
            LanguageSettings.Current.MeasurementConverter.Minutes,
            LanguageSettings.Current.MeasurementConverter.Seconds,
            LanguageSettings.Current.MeasurementConverter.Milliseconds,
            LanguageSettings.Current.MeasurementConverter.Microseconds };

        private readonly List<string> _temperature = new List<string> { LanguageSettings.Current.MeasurementConverter.Fahrenheit,
            LanguageSettings.Current.MeasurementConverter.Celsius,
            LanguageSettings.Current.MeasurementConverter.Kelvin };

        private readonly List<string> _velocity = new List<string> { LanguageSettings.Current.MeasurementConverter.KilometersPerHour,
            LanguageSettings.Current.MeasurementConverter.MetersPerSecond,
            LanguageSettings.Current.MeasurementConverter.MilesPerHour,
            LanguageSettings.Current.MeasurementConverter.YardsPerMinute,
            LanguageSettings.Current.MeasurementConverter.FTsPerSecond,
            LanguageSettings.Current.MeasurementConverter.Knots };

        private readonly List<string> _force = new List<string> { LanguageSettings.Current.MeasurementConverter.PoundsForce,
            LanguageSettings.Current.MeasurementConverter.Newtons,
            LanguageSettings.Current.MeasurementConverter.KilosForce };

        private readonly List<string> _energy = new List<string> { LanguageSettings.Current.MeasurementConverter.Jouls,
            LanguageSettings.Current.MeasurementConverter.Calories,
            LanguageSettings.Current.MeasurementConverter.Ergs,
            LanguageSettings.Current.MeasurementConverter.ElectronVolts,
            LanguageSettings.Current.MeasurementConverter.Btus };

        private readonly List<string> _power = new List<string> { LanguageSettings.Current.MeasurementConverter.Watts,
            LanguageSettings.Current.MeasurementConverter.Horsepower };

        private readonly List<string> _pressure = new List<string> { LanguageSettings.Current.MeasurementConverter.Atmospheres,
            LanguageSettings.Current.MeasurementConverter.Bars,
            LanguageSettings.Current.MeasurementConverter.Pascals,
            LanguageSettings.Current.MeasurementConverter.MillimetersOfMercury,
            LanguageSettings.Current.MeasurementConverter.PoundPerSquareInch,
            LanguageSettings.Current.MeasurementConverter.KilogramPerSquareCentimeter,
            LanguageSettings.Current.MeasurementConverter.KiloPascals };

        public MeasurementConverter()
        {
            UiUtil.PreInitialize(this);
            InitializeComponent();
            UiUtil.FixFonts(this);
            _defaultBackColor = Color.White;
            var l = LanguageSettings.Current.MeasurementConverter;
            Text = l.Title;
            labelConvertFrom.Text = l.ConvertFrom;
            labelConvertTo.Text = l.ConvertTo;
            linkLabel1.Text = l.CopyToClipboard;
            buttonOK.Text = LanguageSettings.Current.General.Ok;
            checkBoxCloseOnInsert.Text = l.CloseOnInsert;
            checkBoxCloseOnInsert.Checked = Configuration.Settings.General.MeasurementConverterCloseOnInsert;

            _defaultBackColor = textBoxInput.BackColor;

            listBoxCategory.Items.Add(l.Length);
            listBoxCategory.Items.Add(l.Mass);
            listBoxCategory.Items.Add(l.Volume);
            listBoxCategory.Items.Add(l.Area);
            listBoxCategory.Items.Add(l.Time);
            listBoxCategory.Items.Add(l.Temperature);
            listBoxCategory.Items.Add(l.Velocity);
            listBoxCategory.Items.Add(l.Force);
            listBoxCategory.Items.Add(l.Energy);
            listBoxCategory.Items.Add(l.Power);
            listBoxCategory.Items.Add(l.Pressure);

            listBoxCategory.SelectedItem = _fields[0];

            UiUtil.FixLargeFonts(this, buttonOK);
        }

        private void comboBoxFrom_SelectedIndexChanged(object sender, EventArgs e)
        {
            string cat = listBoxCategory.SelectedItem.ToString();
            string text = comboBoxFrom.SelectedItem.ToString();
            var l = LanguageSettings.Current.MeasurementConverter;

            comboBoxTo.Items.Clear();

            if (cat == l.Length)
            {
                comboBoxTo.Items.AddRange(_length.ToArray<object>());
            }
            else if (cat == l.Mass)
            {
                comboBoxTo.Items.AddRange(_mass.ToArray<object>());
            }
            else if (cat == l.Volume)
            {
                comboBoxTo.Items.AddRange(_volume.ToArray<object>());
            }
            else if (cat == l.Area)
            {
                comboBoxTo.Items.AddRange(_area.ToArray<object>());
            }
            else if (cat == l.Time)
            {
                comboBoxTo.Items.AddRange(_time.ToArray<object>());
            }
            else if (cat == l.Temperature)
            {
                comboBoxTo.Items.AddRange(_temperature.ToArray<object>());
            }
            else if (cat == l.Velocity)
            {
                comboBoxTo.Items.AddRange(_velocity.ToArray<object>());
            }
            else if (cat == l.Force)
            {
                comboBoxTo.Items.AddRange(_force.ToArray<object>());
            }
            else if (cat == l.Energy)
            {
                comboBoxTo.Items.AddRange(_energy.ToArray<object>());
            }
            else if (cat == l.Power)
            {
                comboBoxTo.Items.AddRange(_power.ToArray<object>());
            }
            else if (cat == l.Pressure)
            {
                comboBoxTo.Items.AddRange(_pressure.ToArray<object>());
            }
            comboBoxTo.Items.Remove(text);

            if (comboBoxTo.Items.Count > 0)
            {
                if (comboBoxTo.Items.Contains(_fields[2]))
                {
                    comboBoxTo.SelectedItem = _fields[2];
                }
                else
                {
                    comboBoxTo.SelectedIndex = 0;
                }
            }

            textBoxInput_TextChanged(null, null);
        }

        private void ShowResult(double d)
        {
            textBoxResult.Text = $"{d:0.##}";
            Output = $"{d:0.##}";
        }

        private void comboBoxTo_SelectedIndexChanged(object sender, EventArgs e)
        {
            textBoxInput_TextChanged(null, null);
        }

        private void buttonInsert_Click(object sender, EventArgs e)
        {
            InsertClicked?.Invoke(this, new InsertEventArgs { Result = Output, IsOriginalActive = IsOriginalActive });

            if (checkBoxCloseOnInsert.Checked)
            {
                Configuration.Settings.General.MeasurementConverterCloseOnInsert = checkBoxCloseOnInsert.Checked;
                Configuration.Settings.General.MeasurementConverterCategories = listBoxCategory.SelectedItem + ";" + comboBoxFrom.SelectedItem + ";" + comboBoxTo.SelectedItem;
                Close();
            }
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            Configuration.Settings.General.MeasurementConverterCloseOnInsert = checkBoxCloseOnInsert.Checked;
            Configuration.Settings.General.MeasurementConverterCategories = listBoxCategory.SelectedItem + ";" + comboBoxFrom.SelectedItem + ";" + comboBoxTo.SelectedItem;
            Close();
        }

        private void MeasurementConverter_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                Close();
            }
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

            string cat = listBoxCategory.SelectedItem.ToString();
            var l = LanguageSettings.Current.MeasurementConverter;

            if (cat == l.Length)
            {
                if (text == l.Kilometers)
                {
                    if (textTo == l.Meters)
                    {
                        ShowResult(Convert.ToDouble(d) * 1000);
                    }
                    else if (textTo == l.Centimeters)
                    {
                        ShowResult(Convert.ToDouble(d) * 100000);
                    }
                    else if (textTo == l.Millimeters)
                    {
                        ShowResult(Convert.ToDouble(d) * 1000000);
                    }
                    else if (textTo == l.Micrometers)
                    {
                        ShowResult(Convert.ToDouble(d) * 1000000000);
                    }
                    else if (textTo == l.Nanometers)
                    {
                        ShowResult(Convert.ToDouble(d) * 1000000000000);
                    }
                    else if (textTo == l.Angstroms)
                    {
                        ShowResult(Convert.ToDouble(d) * 10000000000000);
                    }
                    else if (textTo == l.MilesTerrestial)
                    {
                        ShowResult(Convert.ToDouble(d) / 1.609);
                    }
                    else if (textTo == l.MilesNautical)
                    {
                        ShowResult(Convert.ToDouble(d) / 1.852);
                    }
                    else if (textTo == l.Yards)
                    {
                        ShowResult(Convert.ToDouble(d) * 1093.61);
                    }
                    else if (textTo == l.Feet)
                    {
                        ShowResult(Convert.ToDouble(d) * 3280.84);
                    }
                    else if (textTo == l.Inches)
                    {
                        ShowResult(Convert.ToDouble(d) * 39370.1);
                    }
                    else if (textTo == l.Chains)
                    {
                        ShowResult(Convert.ToDouble(d) * 49.71);
                    }
                    else if (textTo == l.Fathoms)
                    {
                        ShowResult(Convert.ToDouble(d) * 546.807);
                    }
                    else if (textTo == l.Hands)
                    {
                        ShowResult(Convert.ToDouble(d) * 9842.52);
                    }
                    else if (textTo == l.Rods)
                    {
                        ShowResult(Convert.ToDouble(d) * 198.839);
                    }
                    else if (textTo == l.Spans)
                    {
                        ShowResult(Convert.ToDouble(d) * 4374.45);
                    }
                }
                else if (text == l.Meters)
                {
                    if (textTo == l.Kilometers)
                    {
                        ShowResult(Convert.ToDouble(d) / 1000);
                    }
                    else if (textTo == l.Centimeters)
                    {
                        ShowResult(Convert.ToDouble(d) * 100);
                    }
                    else if (textTo == l.Millimeters)
                    {
                        ShowResult(Convert.ToDouble(d) * 1000);
                    }
                    else if (textTo == l.Micrometers)
                    {
                        ShowResult(Convert.ToDouble(d) * 1000000);
                    }
                    else if (textTo == l.Nanometers)
                    {
                        ShowResult(Convert.ToDouble(d) * 1000000000);
                    }
                    else if (textTo == l.Angstroms)
                    {
                        ShowResult(Convert.ToDouble(d) * 10000000000);
                    }
                    else if (textTo == l.MilesTerrestial)
                    {
                        ShowResult(Convert.ToDouble(d) / 1609);
                    }
                    else if (textTo == l.MilesNautical)
                    {
                        ShowResult(Convert.ToDouble(d) / 1852);
                    }
                    else if (textTo == l.Yards)
                    {
                        ShowResult(Convert.ToDouble(d) * 1.094);
                    }
                    else if (textTo == l.Feet)
                    {
                        ShowResult(Convert.ToDouble(d) * 3.281);
                    }
                    else if (textTo == l.Inches)
                    {
                        ShowResult(Convert.ToDouble(d) * 39.37);
                    }
                    else if (textTo == l.Chains)
                    {
                        ShowResult(Convert.ToDouble(d) / 20.117);
                    }
                    else if (textTo == l.Fathoms)
                    {
                        ShowResult(Convert.ToDouble(d) / 1.829);
                    }
                    else if (textTo == l.Hands)
                    {
                        ShowResult(Convert.ToDouble(d) * 9.843);
                    }
                    else if (textTo == l.Rods)
                    {
                        ShowResult(Convert.ToDouble(d) / 5.029);
                    }
                    else if (textTo == l.Spans)
                    {
                        ShowResult(Convert.ToDouble(d) * 4.374);
                    }
                }
                else if (text == l.Centimeters)
                {
                    if (textTo == l.Kilometers)
                    {
                        ShowResult(Convert.ToDouble(d) / 100000);
                    }
                    else if (textTo == l.Meters)
                    {
                        ShowResult(Convert.ToDouble(d) / 100);
                    }
                    else if (textTo == l.Millimeters)
                    {
                        ShowResult(Convert.ToDouble(d) * 10);
                    }
                    else if (textTo == l.Micrometers)
                    {
                        ShowResult(Convert.ToDouble(d) * 10000);
                    }
                    else if (textTo == l.Nanometers)
                    {
                        ShowResult(Convert.ToDouble(d) * 1e+7);
                    }
                    else if (textTo == l.Angstroms)
                    {
                        ShowResult(Convert.ToDouble(d) * 1e+8);
                    }
                    else if (textTo == l.MilesTerrestial)
                    {
                        ShowResult(Convert.ToDouble(d) / 160934);
                    }
                    else if (textTo == l.MilesNautical)
                    {
                        ShowResult(Convert.ToDouble(d) / 185200);
                    }
                    else if (textTo == l.Yards)
                    {
                        ShowResult(Convert.ToDouble(d) / 91.44);
                    }
                    else if (textTo == l.Feet)
                    {
                        ShowResult(Convert.ToDouble(d) / 30.48);
                    }
                    else if (textTo == l.Inches)
                    {
                        ShowResult(Convert.ToDouble(d) / 2.54);
                    }
                    else if (textTo == l.Chains)
                    {
                        ShowResult(Convert.ToDouble(d) / 2012);
                    }
                    else if (textTo == l.Fathoms)
                    {
                        ShowResult(Convert.ToDouble(d) / 183);
                    }
                    else if (textTo == l.Hands)
                    {
                        ShowResult(Convert.ToDouble(d) / 10.16);
                    }
                    else if (textTo == l.Rods)
                    {
                        ShowResult(Convert.ToDouble(d) / 503);
                    }
                    else if (textTo == l.Spans)
                    {
                        ShowResult(Convert.ToDouble(d) / 22.86);
                    }
                }
                else if (text == l.Millimeters)
                {
                    if (textTo == l.Kilometers)
                    {
                        ShowResult(Convert.ToDouble(d) / 1e+6);
                    }
                    else if (textTo == l.Meters)
                    {
                        ShowResult(Convert.ToDouble(d) / 1000);
                    }
                    else if (textTo == l.Centimeters)
                    {
                        ShowResult(Convert.ToDouble(d) / 10);
                    }
                    else if (textTo == l.Micrometers)
                    {
                        ShowResult(Convert.ToDouble(d) * 1000);
                    }
                    else if (textTo == l.Nanometers)
                    {
                        ShowResult(Convert.ToDouble(d) * 1e+6);
                    }
                    else if (textTo == l.Angstroms)
                    {
                        ShowResult(Convert.ToDouble(d) * 1e+7);
                    }
                    else if (textTo == l.MilesTerrestial)
                    {
                        ShowResult(Convert.ToDouble(d) / 1.609e+6);
                    }
                    else if (textTo == l.MilesNautical)
                    {
                        ShowResult(Convert.ToDouble(d) / 1.852e+6);
                    }
                    else if (textTo == l.Yards)
                    {
                        ShowResult(Convert.ToDouble(d) / 914);
                    }
                    else if (textTo == l.Feet)
                    {
                        ShowResult(Convert.ToDouble(d) / 305);
                    }
                    else if (textTo == l.Inches)
                    {
                        ShowResult(Convert.ToDouble(d) / 25.4);
                    }
                    else if (textTo == l.Chains)
                    {
                        ShowResult(Convert.ToDouble(d) / 20117);
                    }
                    else if (textTo == l.Fathoms)
                    {
                        ShowResult(Convert.ToDouble(d) / 1829);
                    }
                    else if (textTo == l.Hands)
                    {
                        ShowResult(Convert.ToDouble(d) / 102);
                    }
                    else if (textTo == l.Rods)
                    {
                        ShowResult(Convert.ToDouble(d) / 5029);
                    }
                    else if (textTo == l.Spans)
                    {
                        ShowResult(Convert.ToDouble(d) / 229);
                    }
                }
                else if (text == l.Micrometers)
                {
                    if (textTo == l.Kilometers)
                    {
                        ShowResult(Convert.ToDouble(d) / 1e+9);
                    }
                    else if (textTo == l.Meters)
                    {
                        ShowResult(Convert.ToDouble(d) / 1e+6);
                    }
                    else if (textTo == l.Centimeters)
                    {
                        ShowResult(Convert.ToDouble(d) / 10000);
                    }
                    else if (textTo == l.Millimeters)
                    {
                        ShowResult(Convert.ToDouble(d) / 1000);
                    }
                    else if (textTo == l.Nanometers)
                    {
                        ShowResult(Convert.ToDouble(d) * 1000);
                    }
                    else if (textTo == l.Angstroms)
                    {
                        ShowResult(Convert.ToDouble(d) * 10000);
                    }
                    else if (textTo == l.MilesTerrestial)
                    {
                        ShowResult(Convert.ToDouble(d) / 1.609e+9);
                    }
                    else if (textTo == l.MilesNautical)
                    {
                        ShowResult(Convert.ToDouble(d) / 1.852e+9);
                    }
                    else if (textTo == l.Yards)
                    {
                        ShowResult(Convert.ToDouble(d) / 914400);
                    }
                    else if (textTo == l.Feet)
                    {
                        ShowResult(Convert.ToDouble(d) / 304800);
                    }
                    else if (textTo == l.Inches)
                    {
                        ShowResult(Convert.ToDouble(d) / 25400);
                    }
                    else if (textTo == l.Chains)
                    {
                        ShowResult(Convert.ToDouble(d) / 2.012e+7);
                    }
                    else if (textTo == l.Fathoms)
                    {
                        ShowResult(Convert.ToDouble(d) / 1.829e+6);
                    }
                    else if (textTo == l.Hands)
                    {
                        ShowResult(Convert.ToDouble(d) / 101600);
                    }
                    else if (textTo == l.Rods)
                    {
                        ShowResult(Convert.ToDouble(d) / 5.029e+6);
                    }
                    else if (textTo == l.Spans)
                    {
                        ShowResult(Convert.ToDouble(d) / 228600);
                    }
                }
                else if (text == l.Nanometers)
                {
                    if (textTo == l.Kilometers)
                    {
                        ShowResult(Convert.ToDouble(d) / 1e+12);
                    }
                    else if (textTo == l.Meters)
                    {
                        ShowResult(Convert.ToDouble(d) / 1e+9);
                    }
                    else if (textTo == l.Centimeters)
                    {
                        ShowResult(Convert.ToDouble(d) / 1e+7);
                    }
                    else if (textTo == l.Millimeters)
                    {
                        ShowResult(Convert.ToDouble(d) / 1e+6);
                    }
                    else if (textTo == l.Micrometers)
                    {
                        ShowResult(Convert.ToDouble(d) / 1000);
                    }
                    else if (textTo == l.Angstroms)
                    {
                        ShowResult(Convert.ToDouble(d) * 10);
                    }
                    else if (textTo == l.MilesTerrestial)
                    {
                        ShowResult(Convert.ToDouble(d) / 1.609e+12);
                    }
                    else if (textTo == l.MilesNautical)
                    {
                        ShowResult(Convert.ToDouble(d) / 1.852e+12);
                    }
                    else if (textTo == l.Yards)
                    {
                        ShowResult(Convert.ToDouble(d) / 9.144e+8);
                    }
                    else if (textTo == l.Feet)
                    {
                        ShowResult(Convert.ToDouble(d) / 3.048e+8);
                    }
                    else if (textTo == l.Inches)
                    {
                        ShowResult(Convert.ToDouble(d) / 2.54e+7);
                    }
                    else if (textTo == l.Chains)
                    {
                        ShowResult(Convert.ToDouble(d) / 2.012e+10);
                    }
                    else if (textTo == l.Fathoms)
                    {
                        ShowResult(Convert.ToDouble(d) / 1.829e+9);
                    }
                    else if (textTo == l.Hands)
                    {
                        ShowResult(Convert.ToDouble(d) / 1.016e+8);
                    }
                    else if (textTo == l.Rods)
                    {
                        ShowResult(Convert.ToDouble(d) / 5.029e+9);
                    }
                    else if (textTo == l.Spans)
                    {
                        ShowResult(Convert.ToDouble(d) / 2.286e+8);
                    }
                }
                else if (text == l.Angstroms)
                {
                    if (textTo == l.Kilometers)
                    {
                        ShowResult(Convert.ToDouble(d) / 1e+13);
                    }
                    else if (textTo == l.Meters)
                    {
                        ShowResult(Convert.ToDouble(d) / 1e+10);
                    }
                    else if (textTo == l.Centimeters)
                    {
                        ShowResult(Convert.ToDouble(d) / 1e+8);
                    }
                    else if (textTo == l.Millimeters)
                    {
                        ShowResult(Convert.ToDouble(d) / 1e+7);
                    }
                    else if (textTo == l.Micrometers)
                    {
                        ShowResult(Convert.ToDouble(d) / 10000);
                    }
                    else if (textTo == l.Nanometers)
                    {
                        ShowResult(Convert.ToDouble(d) / 10);
                    }
                    else if (textTo == l.MilesTerrestial)
                    {
                        ShowResult(Convert.ToDouble(d) / 1.609e+13);
                    }
                    else if (textTo == l.MilesNautical)
                    {
                        ShowResult(Convert.ToDouble(d) / 1.852e+13);
                    }
                    else if (textTo == l.Yards)
                    {
                        ShowResult(Convert.ToDouble(d) / 9.144e+9);
                    }
                    else if (textTo == l.Feet)
                    {
                        ShowResult(Convert.ToDouble(d) / 3.048e+9);
                    }
                    else if (textTo == l.Inches)
                    {
                        ShowResult(Convert.ToDouble(d) / 2.54e+8);
                    }
                    else if (textTo == l.Chains)
                    {
                        ShowResult(Convert.ToDouble(d) / 2.012e+11);
                    }
                    else if (textTo == l.Fathoms)
                    {
                        ShowResult(Convert.ToDouble(d) / 1.829e+10);
                    }
                    else if (textTo == l.Hands)
                    {
                        ShowResult(Convert.ToDouble(d) / 1.016e+9);
                    }
                    else if (textTo == l.Rods)
                    {
                        ShowResult(Convert.ToDouble(d) / 5.029e+10);
                    }
                    else if (textTo == l.Spans)
                    {
                        ShowResult(Convert.ToDouble(d) / 2.286e+9);
                    }
                }
                else if (text == l.MilesTerrestial)
                {
                    if (textTo == l.Kilometers)
                    {
                        ShowResult(Convert.ToDouble(d) * 1.609);
                    }
                    else if (textTo == l.Meters)
                    {
                        ShowResult(Convert.ToDouble(d) * 1609);
                    }
                    else if (textTo == l.Centimeters)
                    {
                        ShowResult(Convert.ToDouble(d) * 160934);
                    }
                    else if (textTo == l.Millimeters)
                    {
                        ShowResult(Convert.ToDouble(d) * 1.609e+6);
                    }
                    else if (textTo == l.Micrometers)
                    {
                        ShowResult(Convert.ToDouble(d) * 1.609e+9);
                    }
                    else if (textTo == l.Nanometers)
                    {
                        ShowResult(Convert.ToDouble(d) * 1.609e+12);
                    }
                    else if (textTo == l.Angstroms)
                    {
                        ShowResult(Convert.ToDouble(d) * 1.609e+13);
                    }
                    else if (textTo == l.MilesNautical)
                    {
                        ShowResult(Convert.ToDouble(d) / 1.151);
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
                    else if (textTo == l.Chains)
                    {
                        ShowResult(Convert.ToDouble(d) * 80);
                    }
                    else if (textTo == l.Fathoms)
                    {
                        ShowResult(Convert.ToDouble(d) * 880);
                    }
                    else if (textTo == l.Hands)
                    {
                        ShowResult(Convert.ToDouble(d) * 15840);
                    }
                    else if (textTo == l.Rods)
                    {
                        ShowResult(Convert.ToDouble(d) * 320);
                    }
                    else if (textTo == l.Spans)
                    {
                        ShowResult(Convert.ToDouble(d) * 7040);
                    }
                }
                else if (text == l.MilesNautical)
                {
                    if (textTo == l.Kilometers)
                    {
                        ShowResult(Convert.ToDouble(d) * 1.852);
                    }
                    else if (textTo == l.Meters)
                    {
                        ShowResult(Convert.ToDouble(d) * 1852);
                    }
                    else if (textTo == l.Centimeters)
                    {
                        ShowResult(Convert.ToDouble(d) * 185200);
                    }
                    else if (textTo == l.Millimeters)
                    {
                        ShowResult(Convert.ToDouble(d) * 1.852e+6);
                    }
                    else if (textTo == l.Micrometers)
                    {
                        ShowResult(Convert.ToDouble(d) * 1.852e+9);
                    }
                    else if (textTo == l.Nanometers)
                    {
                        ShowResult(Convert.ToDouble(d) * 1.852e+12);
                    }
                    else if (textTo == l.Angstroms)
                    {
                        ShowResult(Convert.ToDouble(d) * 1.852e+13);
                    }
                    else if (textTo == l.MilesTerrestial)
                    {
                        ShowResult(Convert.ToDouble(d) * 1.151);
                    }
                    else if (textTo == l.Yards)
                    {
                        ShowResult(Convert.ToDouble(d) * 2025);
                    }
                    else if (textTo == l.Feet)
                    {
                        ShowResult(Convert.ToDouble(d) * 6076);
                    }
                    else if (textTo == l.Inches)
                    {
                        ShowResult(Convert.ToDouble(d) * 72913);
                    }
                    else if (textTo == l.Chains)
                    {
                        ShowResult(Convert.ToDouble(d) * 92.062);
                    }
                    else if (textTo == l.Fathoms)
                    {
                        ShowResult(Convert.ToDouble(d) * 1013);
                    }
                    else if (textTo == l.Hands)
                    {
                        ShowResult(Convert.ToDouble(d) * 18228);
                    }
                    else if (textTo == l.Rods)
                    {
                        ShowResult(Convert.ToDouble(d) * 368);
                    }
                    else if (textTo == l.Spans)
                    {
                        ShowResult(Convert.ToDouble(d) * 8101);
                    }
                }
                else if (text == l.Yards)
                {
                    if (textTo == l.Kilometers)
                    {
                        ShowResult(Convert.ToDouble(d) / 1094);
                    }
                    else if (textTo == l.Meters)
                    {
                        ShowResult(Convert.ToDouble(d) / 1.094);
                    }
                    else if (textTo == l.Centimeters)
                    {
                        ShowResult(Convert.ToDouble(d) * 91.44);
                    }
                    else if (textTo == l.Millimeters)
                    {
                        ShowResult(Convert.ToDouble(d) * 914);
                    }
                    else if (textTo == l.Micrometers)
                    {
                        ShowResult(Convert.ToDouble(d) * 914400);
                    }
                    else if (textTo == l.Nanometers)
                    {
                        ShowResult(Convert.ToDouble(d) * 9.144e+8);
                    }
                    else if (textTo == l.Angstroms)
                    {
                        ShowResult(Convert.ToDouble(d) * 9.144e+9);
                    }
                    else if (textTo == l.MilesTerrestial)
                    {
                        ShowResult(Convert.ToDouble(d) / 1760);
                    }
                    else if (textTo == l.MilesNautical)
                    {
                        ShowResult(Convert.ToDouble(d) / 2025);
                    }
                    else if (textTo == l.Feet)
                    {
                        ShowResult(Convert.ToDouble(d) * 3);
                    }
                    else if (textTo == l.Inches)
                    {
                        ShowResult(Convert.ToDouble(d) * 36);
                    }
                    else if (textTo == l.Chains)
                    {
                        ShowResult(Convert.ToDouble(d) / 22);
                    }
                    else if (textTo == l.Fathoms)
                    {
                        ShowResult(Convert.ToDouble(d) / 2);
                    }
                    else if (textTo == l.Hands)
                    {
                        ShowResult(Convert.ToDouble(d) * 9);
                    }
                    else if (textTo == l.Rods)
                    {
                        ShowResult(Convert.ToDouble(d) / 5.5);
                    }
                    else if (textTo == l.Spans)
                    {
                        ShowResult(Convert.ToDouble(d) * 4);
                    }
                }
                else if (text == l.Feet)
                {
                    if (textTo == l.Kilometers)
                    {
                        ShowResult(Convert.ToDouble(d) / 3281);
                    }
                    else if (textTo == l.Meters)
                    {
                        ShowResult(Convert.ToDouble(d) / 3.281);
                    }
                    else if (textTo == l.Centimeters)
                    {
                        ShowResult(Convert.ToDouble(d) * 30.48);
                    }
                    else if (textTo == l.Millimeters)
                    {
                        ShowResult(Convert.ToDouble(d) * 305);
                    }
                    else if (textTo == l.Micrometers)
                    {
                        ShowResult(Convert.ToDouble(d) * 304800);
                    }
                    else if (textTo == l.Nanometers)
                    {
                        ShowResult(Convert.ToDouble(d) * 3.048e+8);
                    }
                    else if (textTo == l.Angstroms)
                    {
                        ShowResult(Convert.ToDouble(d) * 3.048e+9);
                    }
                    else if (textTo == l.MilesTerrestial)
                    {
                        ShowResult(Convert.ToDouble(d) / 5280);
                    }
                    else if (textTo == l.MilesNautical)
                    {
                        ShowResult(Convert.ToDouble(d) / 6076);
                    }
                    else if (textTo == l.Yards)
                    {
                        ShowResult(Convert.ToDouble(d) / 3);
                    }
                    else if (textTo == l.Inches)
                    {
                        ShowResult(Convert.ToDouble(d) * 12);
                    }
                    else if (textTo == l.Chains)
                    {
                        ShowResult(Convert.ToDouble(d) / 66);
                    }
                    else if (textTo == l.Fathoms)
                    {
                        ShowResult(Convert.ToDouble(d) / 6);
                    }
                    else if (textTo == l.Hands)
                    {
                        ShowResult(Convert.ToDouble(d) * 3);
                    }
                    else if (textTo == l.Rods)
                    {
                        ShowResult(Convert.ToDouble(d) / 16.5);
                    }
                    else if (textTo == l.Spans)
                    {
                        ShowResult(Convert.ToDouble(d) * 1.333);
                    }
                }
                else if (text == l.Inches)
                {
                    if (textTo == l.Kilometers)
                    {
                        ShowResult(Convert.ToDouble(d) / 39370);
                    }
                    else if (textTo == l.Meters)
                    {
                        ShowResult(Convert.ToDouble(d) / 39.37);
                    }
                    else if (textTo == l.Centimeters)
                    {
                        ShowResult(Convert.ToDouble(d) * 2.54);
                    }
                    else if (textTo == l.Millimeters)
                    {
                        ShowResult(Convert.ToDouble(d) * 25.4);
                    }
                    else if (textTo == l.Micrometers)
                    {
                        ShowResult(Convert.ToDouble(d) * 25400);
                    }
                    else if (textTo == l.Nanometers)
                    {
                        ShowResult(Convert.ToDouble(d) * 2.54e+7);
                    }
                    else if (textTo == l.Angstroms)
                    {
                        ShowResult(Convert.ToDouble(d) * 2.54e+8);
                    }
                    else if (textTo == l.MilesTerrestial)
                    {
                        ShowResult(Convert.ToDouble(d) / 63360);
                    }
                    else if (textTo == l.MilesNautical)
                    {
                        ShowResult(Convert.ToDouble(d) / 72913);
                    }
                    else if (textTo == l.Yards)
                    {
                        ShowResult(Convert.ToDouble(d) / 36);
                    }
                    else if (textTo == l.Feet)
                    {
                        ShowResult(Convert.ToDouble(d) / 12);
                    }
                    else if (textTo == l.Chains)
                    {
                        ShowResult(Convert.ToDouble(d) / 792);
                    }
                    else if (textTo == l.Fathoms)
                    {
                        ShowResult(Convert.ToDouble(d) / 72);
                    }
                    else if (textTo == l.Hands)
                    {
                        ShowResult(Convert.ToDouble(d) / 4);
                    }
                    else if (textTo == l.Rods)
                    {
                        ShowResult(Convert.ToDouble(d) / 198);
                    }
                    else if (textTo == l.Spans)
                    {
                        ShowResult(Convert.ToDouble(d) / 9);
                    }
                }
                else if (text == l.Chains)
                {
                    if (textTo == l.Kilometers)
                    {
                        ShowResult(Convert.ToDouble(d) / 49.71);
                    }
                    else if (textTo == l.Meters)
                    {
                        ShowResult(Convert.ToDouble(d) * 20.117);
                    }
                    else if (textTo == l.Centimeters)
                    {
                        ShowResult(Convert.ToDouble(d) * 2012);
                    }
                    else if (textTo == l.Millimeters)
                    {
                        ShowResult(Convert.ToDouble(d) * 20117);
                    }
                    else if (textTo == l.Micrometers)
                    {
                        ShowResult(Convert.ToDouble(d) * 2.012e+7);
                    }
                    else if (textTo == l.Nanometers)
                    {
                        ShowResult(Convert.ToDouble(d) * 2.012e+10);
                    }
                    else if (textTo == l.Angstroms)
                    {
                        ShowResult(Convert.ToDouble(d) * 2.012e+11);
                    }
                    else if (textTo == l.MilesTerrestial)
                    {
                        ShowResult(Convert.ToDouble(d) / 80);
                    }
                    else if (textTo == l.MilesNautical)
                    {
                        ShowResult(Convert.ToDouble(d) / 92.062);
                    }
                    else if (textTo == l.Yards)
                    {
                        ShowResult(Convert.ToDouble(d) * 22);
                    }
                    else if (textTo == l.Feet)
                    {
                        ShowResult(Convert.ToDouble(d) * 66);
                    }
                    else if (textTo == l.Inches)
                    {
                        ShowResult(Convert.ToDouble(d) * 792);
                    }
                    else if (textTo == l.Fathoms)
                    {
                        ShowResult(Convert.ToDouble(d) * 11);
                    }
                    else if (textTo == l.Hands)
                    {
                        ShowResult(Convert.ToDouble(d) * 198);
                    }
                    else if (textTo == l.Rods)
                    {
                        ShowResult(Convert.ToDouble(d) * 4);
                    }
                    else if (textTo == l.Spans)
                    {
                        ShowResult(Convert.ToDouble(d) * 88);
                    }
                }
                else if (text == l.Fathoms)
                {
                    if (textTo == l.Kilometers)
                    {
                        ShowResult(Convert.ToDouble(d) / 547);
                    }
                    else if (textTo == l.Meters)
                    {
                        ShowResult(Convert.ToDouble(d) * 1.829);
                    }
                    else if (textTo == l.Centimeters)
                    {
                        ShowResult(Convert.ToDouble(d) * 183);
                    }
                    else if (textTo == l.Millimeters)
                    {
                        ShowResult(Convert.ToDouble(d) * 1829);
                    }
                    else if (textTo == l.Micrometers)
                    {
                        ShowResult(Convert.ToDouble(d) * 1.829e+6);
                    }
                    else if (textTo == l.Nanometers)
                    {
                        ShowResult(Convert.ToDouble(d) * 1.829e+9);
                    }
                    else if (textTo == l.Angstroms)
                    {
                        ShowResult(Convert.ToDouble(d) * 1.829e+10);
                    }
                    else if (textTo == l.MilesTerrestial)
                    {
                        ShowResult(Convert.ToDouble(d) / 880);
                    }
                    else if (textTo == l.MilesNautical)
                    {
                        ShowResult(Convert.ToDouble(d) / 1013);
                    }
                    else if (textTo == l.Yards)
                    {
                        ShowResult(Convert.ToDouble(d) * 2);
                    }
                    else if (textTo == l.Feet)
                    {
                        ShowResult(Convert.ToDouble(d) * 6);
                    }
                    else if (textTo == l.Inches)
                    {
                        ShowResult(Convert.ToDouble(d) * 72);
                    }
                    else if (textTo == l.Chains)
                    {
                        ShowResult(Convert.ToDouble(d) / 11);
                    }
                    else if (textTo == l.Hands)
                    {
                        ShowResult(Convert.ToDouble(d) * 18);
                    }
                    else if (textTo == l.Rods)
                    {
                        ShowResult(Convert.ToDouble(d) / 2.75);
                    }
                    else if (textTo == l.Spans)
                    {
                        ShowResult(Convert.ToDouble(d) * 8);
                    }
                }
                else if (text == l.Hands)
                {
                    if (textTo == l.Kilometers)
                    {
                        ShowResult(Convert.ToDouble(d) / 9843);
                    }
                    else if (textTo == l.Meters)
                    {
                        ShowResult(Convert.ToDouble(d) / 9.843);
                    }
                    else if (textTo == l.Centimeters)
                    {
                        ShowResult(Convert.ToDouble(d) * 10.16);
                    }
                    else if (textTo == l.Millimeters)
                    {
                        ShowResult(Convert.ToDouble(d) * 102);
                    }
                    else if (textTo == l.Micrometers)
                    {
                        ShowResult(Convert.ToDouble(d) * 101600);
                    }
                    else if (textTo == l.Nanometers)
                    {
                        ShowResult(Convert.ToDouble(d) * 1.016e+8);
                    }
                    else if (textTo == l.Angstroms)
                    {
                        ShowResult(Convert.ToDouble(d) * 1.016e+9);
                    }
                    else if (textTo == l.MilesTerrestial)
                    {
                        ShowResult(Convert.ToDouble(d) / 15840);
                    }
                    else if (textTo == l.MilesNautical)
                    {
                        ShowResult(Convert.ToDouble(d) / 18228);
                    }
                    else if (textTo == l.Yards)
                    {
                        ShowResult(Convert.ToDouble(d) / 9);
                    }
                    else if (textTo == l.Feet)
                    {
                        ShowResult(Convert.ToDouble(d) / 3);
                    }
                    else if (textTo == l.Inches)
                    {
                        ShowResult(Convert.ToDouble(d) * 4);
                    }
                    else if (textTo == l.Chains)
                    {
                        ShowResult(Convert.ToDouble(d) / 198);
                    }
                    else if (textTo == l.Fathoms)
                    {
                        ShowResult(Convert.ToDouble(d) / 18);
                    }
                    else if (textTo == l.Rods)
                    {
                        ShowResult(Convert.ToDouble(d) / 49.5);
                    }
                    else if (textTo == l.Spans)
                    {
                        ShowResult(Convert.ToDouble(d) / 2.25);
                    }
                }
                else if (text == l.Rods)
                {
                    if (textTo == l.Kilometers)
                    {
                        ShowResult(Convert.ToDouble(d) / 199);
                    }
                    else if (textTo == l.Meters)
                    {
                        ShowResult(Convert.ToDouble(d) * 5.029);
                    }
                    else if (textTo == l.Centimeters)
                    {
                        ShowResult(Convert.ToDouble(d) * 503);
                    }
                    else if (textTo == l.Millimeters)
                    {
                        ShowResult(Convert.ToDouble(d) * 5029);
                    }
                    else if (textTo == l.Micrometers)
                    {
                        ShowResult(Convert.ToDouble(d) * 5.029e+6);
                    }
                    else if (textTo == l.Nanometers)
                    {
                        ShowResult(Convert.ToDouble(d) * 5.029e+9);
                    }
                    else if (textTo == l.Angstroms)
                    {
                        ShowResult(Convert.ToDouble(d) * 5.029e+10);
                    }
                    else if (textTo == l.MilesTerrestial)
                    {
                        ShowResult(Convert.ToDouble(d) / 320);
                    }
                    else if (textTo == l.MilesNautical)
                    {
                        ShowResult(Convert.ToDouble(d) / 368);
                    }
                    else if (textTo == l.Yards)
                    {
                        ShowResult(Convert.ToDouble(d) * 5.5);
                    }
                    else if (textTo == l.Feet)
                    {
                        ShowResult(Convert.ToDouble(d) * 16.5);
                    }
                    else if (textTo == l.Inches)
                    {
                        ShowResult(Convert.ToDouble(d) * 198);
                    }
                    else if (textTo == l.Chains)
                    {
                        ShowResult(Convert.ToDouble(d) / 4);
                    }
                    else if (textTo == l.Fathoms)
                    {
                        ShowResult(Convert.ToDouble(d) * 2.75);
                    }
                    else if (textTo == l.Hands)
                    {
                        ShowResult(Convert.ToDouble(d) * 49.5);
                    }
                    else if (textTo == l.Spans)
                    {
                        ShowResult(Convert.ToDouble(d) * 22);
                    }
                }
                else if (text == l.Spans)
                {
                    if (textTo == l.Kilometers)
                    {
                        ShowResult(Convert.ToDouble(d) / 4374);
                    }
                    else if (textTo == l.Meters)
                    {
                        ShowResult(Convert.ToDouble(d) / 4.374);
                    }
                    else if (textTo == l.Centimeters)
                    {
                        ShowResult(Convert.ToDouble(d) * 22.86);
                    }
                    else if (textTo == l.Millimeters)
                    {
                        ShowResult(Convert.ToDouble(d) * 229);
                    }
                    else if (textTo == l.Micrometers)
                    {
                        ShowResult(Convert.ToDouble(d) * 228600);
                    }
                    else if (textTo == l.Nanometers)
                    {
                        ShowResult(Convert.ToDouble(d) * 2.286e+8);
                    }
                    else if (textTo == l.Angstroms)
                    {
                        ShowResult(Convert.ToDouble(d) * 2.286e+9);
                    }
                    else if (textTo == l.MilesTerrestial)
                    {
                        ShowResult(Convert.ToDouble(d) / 7040);
                    }
                    else if (textTo == l.MilesNautical)
                    {
                        ShowResult(Convert.ToDouble(d) / 8101);
                    }
                    else if (textTo == l.Yards)
                    {
                        ShowResult(Convert.ToDouble(d) / 4);
                    }
                    else if (textTo == l.Feet)
                    {
                        ShowResult(Convert.ToDouble(d) / 1.333);
                    }
                    else if (textTo == l.Inches)
                    {
                        ShowResult(Convert.ToDouble(d) * 9);
                    }
                    else if (textTo == l.Chains)
                    {
                        ShowResult(Convert.ToDouble(d) / 88);
                    }
                    else if (textTo == l.Fathoms)
                    {
                        ShowResult(Convert.ToDouble(d) / 8);
                    }
                    else if (textTo == l.Hands)
                    {
                        ShowResult(Convert.ToDouble(d) * 2.25);
                    }
                    else if (textTo == l.Rods)
                    {
                        ShowResult(Convert.ToDouble(d) / 22);
                    }
                }
            }
            else if (cat == l.Mass)
            {
                if (text == l.LongTonnes)
                {
                    if (textTo == l.ShortTonnes)
                    {
                        ShowResult(Convert.ToDouble(d) * 1.12);
                    }
                    else if (textTo == l.Tonnes)
                    {
                        ShowResult(Convert.ToDouble(d) * 1.016);
                    }
                    else if (textTo == l.Kilos)
                    {
                        ShowResult(Convert.ToDouble(d) * 1016);
                    }
                    else if (textTo == l.Grams)
                    {
                        ShowResult(Convert.ToDouble(d) * 1.016e+6);
                    }
                    else if (textTo == l.Milligrams)
                    {
                        ShowResult(Convert.ToDouble(d) * 1.016e+9);
                    }
                    else if (textTo == l.Micrograms)
                    {
                        ShowResult(Convert.ToDouble(d) * 1.016e+12);
                    }
                    else if (textTo == l.Pounds)
                    {
                        ShowResult(Convert.ToDouble(d) * 2240);
                    }
                    else if (textTo == l.Ounces)
                    {
                        ShowResult(Convert.ToDouble(d) * 35840);
                    }
                    else if (textTo == l.Carats)
                    {
                        ShowResult(Convert.ToDouble(d) * 5.08e+6);
                    }
                    else if (textTo == l.Drams)
                    {
                        ShowResult(Convert.ToDouble(d) * 573438);
                    }
                    else if (textTo == l.Grains)
                    {
                        ShowResult(Convert.ToDouble(d) * 1.568e+7);
                    }
                    else if (textTo == l.Stones)
                    {
                        ShowResult(Convert.ToDouble(d) * 160);
                    }
                }
                else if (text == l.ShortTonnes)
                {
                    if (textTo == l.LongTonnes)
                    {
                        ShowResult(Convert.ToDouble(d) / 1.12);
                    }
                    else if (textTo == l.Tonnes)
                    {
                        ShowResult(Convert.ToDouble(d) / 1.102);
                    }
                    else if (textTo == l.Kilos)
                    {
                        ShowResult(Convert.ToDouble(d) * 907);
                    }
                    else if (textTo == l.Grams)
                    {
                        ShowResult(Convert.ToDouble(d) * 907185);
                    }
                    else if (textTo == l.Milligrams)
                    {
                        ShowResult(Convert.ToDouble(d) * 9.072e+8);
                    }
                    else if (textTo == l.Micrograms)
                    {
                        ShowResult(Convert.ToDouble(d) * 9.072e+11);
                    }
                    else if (textTo == l.Pounds)
                    {
                        ShowResult(Convert.ToDouble(d) * 2000);
                    }
                    else if (textTo == l.Ounces)
                    {
                        ShowResult(Convert.ToDouble(d) * 32000);
                    }
                    else if (textTo == l.Carats)
                    {
                        ShowResult(Convert.ToDouble(d) * 4.536e+6);
                    }
                    else if (textTo == l.Drams)
                    {
                        ShowResult(Convert.ToDouble(d) * 511999);
                    }
                    else if (textTo == l.Grains)
                    {
                        ShowResult(Convert.ToDouble(d) * 1.4e+7);
                    }
                    else if (textTo == l.Stones)
                    {
                        ShowResult(Convert.ToDouble(d) * 143);
                    }
                }
                else if (text == l.Tonnes)
                {
                    if (textTo == l.LongTonnes)
                    {
                        ShowResult(Convert.ToDouble(d) / 1.016);
                    }
                    else if (textTo == l.ShortTonnes)
                    {
                        ShowResult(Convert.ToDouble(d) * 1.102);
                    }
                    else if (textTo == l.Kilos)
                    {
                        ShowResult(Convert.ToDouble(d) * 1000);
                    }
                    else if (textTo == l.Grams)
                    {
                        ShowResult(Convert.ToDouble(d) * 1e+6);
                    }
                    else if (textTo == l.Milligrams)
                    {
                        ShowResult(Convert.ToDouble(d) * 1e+9);
                    }
                    else if (textTo == l.Micrograms)
                    {
                        ShowResult(Convert.ToDouble(d) * 1e+12);
                    }
                    else if (textTo == l.Pounds)
                    {
                        ShowResult(Convert.ToDouble(d) * 2205);
                    }
                    else if (textTo == l.Ounces)
                    {
                        ShowResult(Convert.ToDouble(d) * 35274);
                    }
                    else if (textTo == l.Carats)
                    {
                        ShowResult(Convert.ToDouble(d) * 5e+6);
                    }
                    else if (textTo == l.Drams)
                    {
                        ShowResult(Convert.ToDouble(d) * 564382);
                    }
                    else if (textTo == l.Grains)
                    {
                        ShowResult(Convert.ToDouble(d) * 1.543e+7);
                    }
                    else if (textTo == l.Stones)
                    {
                        ShowResult(Convert.ToDouble(d) * 157);
                    }
                }
                else if (text == l.Kilos)
                {
                    if (textTo == l.LongTonnes)
                    {
                        ShowResult(Convert.ToDouble(d) / 1016);
                    }
                    else if (textTo == l.ShortTonnes)
                    {
                        ShowResult(Convert.ToDouble(d) / 907);
                    }
                    else if (textTo == l.Tonnes)
                    {
                        ShowResult(Convert.ToDouble(d) / 1000);
                    }
                    else if (textTo == l.Grams)
                    {
                        ShowResult(Convert.ToDouble(d) * 1000);
                    }
                    else if (textTo == l.Milligrams)
                    {
                        ShowResult(Convert.ToDouble(d) * 1e+6);
                    }
                    else if (textTo == l.Micrograms)
                    {
                        ShowResult(Convert.ToDouble(d) * 1e+9);
                    }
                    else if (textTo == l.Pounds)
                    {
                        ShowResult(Convert.ToDouble(d) * 2.205);
                    }
                    else if (textTo == l.Ounces)
                    {
                        ShowResult(Convert.ToDouble(d) * 35.274);
                    }
                    else if (textTo == l.Carats)
                    {
                        ShowResult(Convert.ToDouble(d) * 5000);
                    }
                    else if (textTo == l.Drams)
                    {
                        ShowResult(Convert.ToDouble(d) * 564);
                    }
                    else if (textTo == l.Grains)
                    {
                        ShowResult(Convert.ToDouble(d) * 15432);
                    }
                    else if (textTo == l.Stones)
                    {
                        ShowResult(Convert.ToDouble(d) / 6.35);
                    }
                }
                else if (text == l.Grams)
                {
                    if (textTo == l.LongTonnes)
                    {
                        ShowResult(Convert.ToDouble(d) / 1.016e+6);
                    }
                    else if (textTo == l.ShortTonnes)
                    {
                        ShowResult(Convert.ToDouble(d) / 907185);
                    }
                    else if (textTo == l.Tonnes)
                    {
                        ShowResult(Convert.ToDouble(d) / 1e+6);
                    }
                    else if (textTo == l.Kilos)
                    {
                        ShowResult(Convert.ToDouble(d) / 1000);
                    }
                    else if (textTo == l.Milligrams)
                    {
                        ShowResult(Convert.ToDouble(d) * 1000);
                    }
                    else if (textTo == l.Micrograms)
                    {
                        ShowResult(Convert.ToDouble(d) * 1e+6);
                    }
                    else if (textTo == l.Pounds)
                    {
                        ShowResult(Convert.ToDouble(d) / 454);
                    }
                    else if (textTo == l.Ounces)
                    {
                        ShowResult(Convert.ToDouble(d) / 28.35);
                    }
                    else if (textTo == l.Carats)
                    {
                        ShowResult(Convert.ToDouble(d) * 5);
                    }
                    else if (textTo == l.Drams)
                    {
                        ShowResult(Convert.ToDouble(d) / 1.772);
                    }
                    else if (textTo == l.Grains)
                    {
                        ShowResult(Convert.ToDouble(d) * 15.432);
                    }
                    else if (textTo == l.Stones)
                    {
                        ShowResult(Convert.ToDouble(d) / 6350);
                    }
                }
                else if (text == l.Milligrams)
                {
                    if (textTo == l.LongTonnes)
                    {
                        ShowResult(Convert.ToDouble(d) / 1.016e+9);
                    }
                    else if (textTo == l.ShortTonnes)
                    {
                        ShowResult(Convert.ToDouble(d) / 9.072e+8);
                    }
                    else if (textTo == l.Tonnes)
                    {
                        ShowResult(Convert.ToDouble(d) / 1e+9);
                    }
                    else if (textTo == l.Kilos)
                    {
                        ShowResult(Convert.ToDouble(d) / 1e+6);
                    }
                    else if (textTo == l.Grams)
                    {
                        ShowResult(Convert.ToDouble(d) / 1000);
                    }
                    else if (textTo == l.Micrograms)
                    {
                        ShowResult(Convert.ToDouble(d) * 1000);
                    }
                    else if (textTo == l.Pounds)
                    {
                        ShowResult(Convert.ToDouble(d) / 453592);
                    }
                    else if (textTo == l.Ounces)
                    {
                        ShowResult(Convert.ToDouble(d) / 28350);
                    }
                    else if (textTo == l.Carats)
                    {
                        ShowResult(Convert.ToDouble(d) / 200);
                    }
                    else if (textTo == l.Drams)
                    {
                        ShowResult(Convert.ToDouble(d) / 1772);
                    }
                    else if (textTo == l.Grains)
                    {
                        ShowResult(Convert.ToDouble(d) / 64.799);
                    }
                    else if (textTo == l.Stones)
                    {
                        ShowResult(Convert.ToDouble(d) / 6.35e+6);
                    }
                }
                else if (text == l.Micrograms)
                {
                    if (textTo == l.LongTonnes)
                    {
                        ShowResult(Convert.ToDouble(d) / 1.016e+12);
                    }
                    else if (textTo == l.ShortTonnes)
                    {
                        ShowResult(Convert.ToDouble(d) / 9.072e+11);
                    }
                    else if (textTo == l.Tonnes)
                    {
                        ShowResult(Convert.ToDouble(d) / 1e+12);
                    }
                    else if (textTo == l.Kilos)
                    {
                        ShowResult(Convert.ToDouble(d) / 1e+9);
                    }
                    else if (textTo == l.Grams)
                    {
                        ShowResult(Convert.ToDouble(d) / 1e+6);
                    }
                    else if (textTo == l.Milligrams)
                    {
                        ShowResult(Convert.ToDouble(d) / 1000);
                    }
                    else if (textTo == l.Pounds)
                    {
                        ShowResult(Convert.ToDouble(d) / 4.536e+8);
                    }
                    else if (textTo == l.Ounces)
                    {
                        ShowResult(Convert.ToDouble(d) / 2.835e+7);
                    }
                    else if (textTo == l.Carats)
                    {
                        ShowResult(Convert.ToDouble(d) / 200000);
                    }
                    else if (textTo == l.Drams)
                    {
                        ShowResult(Convert.ToDouble(d) / 1.772e+6);
                    }
                    else if (textTo == l.Grains)
                    {
                        ShowResult(Convert.ToDouble(d) / 64799);
                    }
                    else if (textTo == l.Stones)
                    {
                        ShowResult(Convert.ToDouble(d) / 6.35e+9);
                    }
                }
                else if (text == l.Pounds)
                {
                    if (textTo == l.LongTonnes)
                    {
                        ShowResult(Convert.ToDouble(d) / 2240);
                    }
                    else if (textTo == l.ShortTonnes)
                    {
                        ShowResult(Convert.ToDouble(d) / 2000);
                    }
                    else if (textTo == l.Tonnes)
                    {
                        ShowResult(Convert.ToDouble(d) / 2205);
                    }
                    else if (textTo == l.Kilos)
                    {
                        ShowResult(Convert.ToDouble(d) / 2.205);
                    }
                    else if (textTo == l.Grams)
                    {
                        ShowResult(Convert.ToDouble(d) * 454);
                    }
                    else if (textTo == l.Milligrams)
                    {
                        ShowResult(Convert.ToDouble(d) * 453592);
                    }
                    else if (textTo == l.Micrograms)
                    {
                        ShowResult(Convert.ToDouble(d) * 4.536e+8);
                    }
                    else if (textTo == l.Ounces)
                    {
                        ShowResult(Convert.ToDouble(d) * 16);
                    }
                    else if (textTo == l.Carats)
                    {
                        ShowResult(Convert.ToDouble(d) * 2268);
                    }
                    else if (textTo == l.Drams)
                    {
                        ShowResult(Convert.ToDouble(d) * 256);
                    }
                    else if (textTo == l.Grains)
                    {
                        ShowResult(Convert.ToDouble(d) * 7000);
                    }
                    else if (textTo == l.Stones)
                    {
                        ShowResult(Convert.ToDouble(d) / 14);
                    }
                }
                else if (text == l.Ounces)
                {
                    if (textTo == l.LongTonnes)
                    {
                        ShowResult(Convert.ToDouble(d) / 35840);
                    }
                    else if (textTo == l.ShortTonnes)
                    {
                        ShowResult(Convert.ToDouble(d) / 32000);
                    }
                    else if (textTo == l.Tonnes)
                    {
                        ShowResult(Convert.ToDouble(d) / 35274);
                    }
                    else if (textTo == l.Kilos)
                    {
                        ShowResult(Convert.ToDouble(d) / 35.274);
                    }
                    else if (textTo == l.Grams)
                    {
                        ShowResult(Convert.ToDouble(d) * 28.35);
                    }
                    else if (textTo == l.Milligrams)
                    {
                        ShowResult(Convert.ToDouble(d) * 28350);
                    }
                    else if (textTo == l.Micrograms)
                    {
                        ShowResult(Convert.ToDouble(d) * 2.835e+7);
                    }
                    else if (textTo == l.Pounds)
                    {
                        ShowResult(Convert.ToDouble(d) / 16);
                    }
                    else if (textTo == l.Carats)
                    {
                        ShowResult(Convert.ToDouble(d) * 142);
                    }
                    else if (textTo == l.Drams)
                    {
                        ShowResult(Convert.ToDouble(d) * 16);
                    }
                    else if (textTo == l.Grains)
                    {
                        ShowResult(Convert.ToDouble(d) * 438);
                    }
                    else if (textTo == l.Stones)
                    {
                        ShowResult(Convert.ToDouble(d) / 224);
                    }
                }
                else if (text == l.Carats)
                {
                    if (textTo == l.LongTonnes)
                    {
                        ShowResult(Convert.ToDouble(d) / 5.08e+6);
                    }
                    else if (textTo == l.ShortTonnes)
                    {
                        ShowResult(Convert.ToDouble(d) / 4.536e+6);
                    }
                    else if (textTo == l.Tonnes)
                    {
                        ShowResult(Convert.ToDouble(d) / 5e+6);
                    }
                    else if (textTo == l.Kilos)
                    {
                        ShowResult(Convert.ToDouble(d) / 5000);
                    }
                    else if (textTo == l.Grams)
                    {
                        ShowResult(Convert.ToDouble(d) / 5);
                    }
                    else if (textTo == l.Milligrams)
                    {
                        ShowResult(Convert.ToDouble(d) * 200);
                    }
                    else if (textTo == l.Micrograms)
                    {
                        ShowResult(Convert.ToDouble(d) * 200000);
                    }
                    else if (textTo == l.Pounds)
                    {
                        ShowResult(Convert.ToDouble(d) / 2268);
                    }
                    else if (textTo == l.Ounces)
                    {
                        ShowResult(Convert.ToDouble(d) / 142);
                    }
                    else if (textTo == l.Drams)
                    {
                        ShowResult(Convert.ToDouble(d) / 8.859);
                    }
                    else if (textTo == l.Grains)
                    {
                        ShowResult(Convert.ToDouble(d) * 3.086);
                    }
                    else if (textTo == l.Stones)
                    {
                        ShowResult(Convert.ToDouble(d) / 31751);
                    }
                }
                else if (text == l.Drams)
                {
                    if (textTo == l.LongTonnes)
                    {
                        ShowResult(Convert.ToDouble(d) / 573438);
                    }
                    else if (textTo == l.ShortTonnes)
                    {
                        ShowResult(Convert.ToDouble(d) / 511999);
                    }
                    else if (textTo == l.Tonnes)
                    {
                        ShowResult(Convert.ToDouble(d) / 564382);
                    }
                    else if (textTo == l.Kilos)
                    {
                        ShowResult(Convert.ToDouble(d) / 564);
                    }
                    else if (textTo == l.Grams)
                    {
                        ShowResult(Convert.ToDouble(d) * 1.772);
                    }
                    else if (textTo == l.Milligrams)
                    {
                        ShowResult(Convert.ToDouble(d) * 1772);
                    }
                    else if (textTo == l.Micrograms)
                    {
                        ShowResult(Convert.ToDouble(d) * 1.772e+6);
                    }
                    else if (textTo == l.Pounds)
                    {
                        ShowResult(Convert.ToDouble(d) / 256);
                    }
                    else if (textTo == l.Ounces)
                    {
                        ShowResult(Convert.ToDouble(d) / 16);
                    }
                    else if (textTo == l.Carats)
                    {
                        ShowResult(Convert.ToDouble(d) * 8.859);
                    }
                    else if (textTo == l.Grains)
                    {
                        ShowResult(Convert.ToDouble(d) * 27.344);
                    }
                    else if (textTo == l.Stones)
                    {
                        ShowResult(Convert.ToDouble(d) / 3584);
                    }
                }
                else if (text == l.Grains)
                {
                    if (textTo == l.LongTonnes)
                    {
                        ShowResult(Convert.ToDouble(d) / 1.568e+7);
                    }
                    else if (textTo == l.ShortTonnes)
                    {
                        ShowResult(Convert.ToDouble(d) / 1.4e+7);
                    }
                    else if (textTo == l.Tonnes)
                    {
                        ShowResult(Convert.ToDouble(d) / 1.543e+7);
                    }
                    else if (textTo == l.Kilos)
                    {
                        ShowResult(Convert.ToDouble(d) / 15432);
                    }
                    else if (textTo == l.Grams)
                    {
                        ShowResult(Convert.ToDouble(d) / 15.432);
                    }
                    else if (textTo == l.Milligrams)
                    {
                        ShowResult(Convert.ToDouble(d) * 64.799);
                    }
                    else if (textTo == l.Micrograms)
                    {
                        ShowResult(Convert.ToDouble(d) * 64799);
                    }
                    else if (textTo == l.Pounds)
                    {
                        ShowResult(Convert.ToDouble(d) / 7000);
                    }
                    else if (textTo == l.Ounces)
                    {
                        ShowResult(Convert.ToDouble(d) / 438);
                    }
                    else if (textTo == l.Carats)
                    {
                        ShowResult(Convert.ToDouble(d) / 3.086);
                    }
                    else if (textTo == l.Drams)
                    {
                        ShowResult(Convert.ToDouble(d) / 27.344);
                    }
                    else if (textTo == l.Stones)
                    {
                        ShowResult(Convert.ToDouble(d) / 98000);
                    }
                }
                else if (text == l.Stones)
                {
                    if (textTo == l.LongTonnes)
                    {
                        ShowResult(Convert.ToDouble(d) / 160);
                    }
                    else if (textTo == l.ShortTonnes)
                    {
                        ShowResult(Convert.ToDouble(d) / 143);
                    }
                    else if (textTo == l.Tonnes)
                    {
                        ShowResult(Convert.ToDouble(d) / 157);
                    }
                    else if (textTo == l.Kilos)
                    {
                        ShowResult(Convert.ToDouble(d) * 6.35);
                    }
                    else if (textTo == l.Grams)
                    {
                        ShowResult(Convert.ToDouble(d) * 6350);
                    }
                    else if (textTo == l.Milligrams)
                    {
                        ShowResult(Convert.ToDouble(d) * 6.35e+6);
                    }
                    else if (textTo == l.Micrograms)
                    {
                        ShowResult(Convert.ToDouble(d) * 6.35e+9);
                    }
                    else if (textTo == l.Pounds)
                    {
                        ShowResult(Convert.ToDouble(d) * 14);
                    }
                    else if (textTo == l.Ounces)
                    {
                        ShowResult(Convert.ToDouble(d) * 224);
                    }
                    else if (textTo == l.Carats)
                    {
                        ShowResult(Convert.ToDouble(d) * 31751);
                    }
                    else if (textTo == l.Drams)
                    {
                        ShowResult(Convert.ToDouble(d) * 3584);
                    }
                    else if (textTo == l.Grains)
                    {
                        ShowResult(Convert.ToDouble(d) * 98000);
                    }
                }
            }
            else if (cat == l.Volume)
            {
                if (text == l.CubicKilometers)
                {
                    if (textTo == l.CubicMeters)
                    {
                        ShowResult(Convert.ToDouble(d) * 1e+9);
                    }
                    else if (textTo == l.Litres)
                    {
                        ShowResult(Convert.ToDouble(d) * 1e+12);
                    }
                    else if (textTo == l.CubicCentimeters)
                    {
                        ShowResult(Convert.ToDouble(d) * 1e+15);
                    }
                    else if (textTo == l.CubicMillimeters)
                    {
                        ShowResult(Convert.ToDouble(d) * 1e+18);
                    }
                    else if (textTo == l.CubicMiles)
                    {
                        ShowResult(Convert.ToDouble(d) / 4.168);
                    }
                    else if (textTo == l.CubicYards)
                    {
                        ShowResult(Convert.ToDouble(d) * 1.308e+9);
                    }
                    else if (textTo == l.CubicFTs)
                    {
                        ShowResult(Convert.ToDouble(d) * 3.531e+10);
                    }
                    else if (textTo == l.CubicInches)
                    {
                        ShowResult(Convert.ToDouble(d) * 6.102e+13);
                    }
                    else if (textTo == l.OilBarrels)
                    {
                        ShowResult(Convert.ToDouble(d) * 6.29e+9);
                    }
                    else if (textTo == l.GallonUS)
                    {
                        ShowResult(Convert.ToDouble(d) * 2.642e+11);
                    }
                    else if (textTo == l.QuartsUS)
                    {
                        ShowResult(Convert.ToDouble(d) * 1.057e+12);
                    }
                    else if (textTo == l.PintsUS)
                    {
                        ShowResult(Convert.ToDouble(d) * 2.113e+12);
                    }
                    else if (textTo == l.FluidOuncesUS)
                    {
                        ShowResult(Convert.ToDouble(d) * 3.381e+13);
                    }
                    else if (textTo == l.Bushels)
                    {
                        ShowResult(Convert.ToDouble(d) * 2.838e+10);
                    }
                    else if (textTo == l.Pecks)
                    {
                        ShowResult(Convert.ToDouble(d) * 1.135e+11);
                    }
                    else if (textTo == l.GallonsUK)
                    {
                        ShowResult(Convert.ToDouble(d) * 2.2e+11);
                    }
                    else if (textTo == l.QuartsUK)
                    {
                        ShowResult(Convert.ToDouble(d) * 8.799e+11);
                    }
                    else if (textTo == l.PintsUK)
                    {
                        ShowResult(Convert.ToDouble(d) * 1.76e+12);
                    }
                    else if (textTo == l.FluidOuncesUK)
                    {
                        ShowResult(Convert.ToDouble(d) * 3.52e+13);
                    }
                }
                else if (text == l.CubicMeters)
                {
                    if (textTo == l.CubicKilometers)
                    {
                        ShowResult(Convert.ToDouble(d) / 1e+9);
                    }
                    else if (textTo == l.Litres)
                    {
                        ShowResult(Convert.ToDouble(d) * 1000);
                    }
                    else if (textTo == l.CubicCentimeters)
                    {
                        ShowResult(Convert.ToDouble(d) * 1e+6);
                    }
                    else if (textTo == l.CubicMillimeters)
                    {
                        ShowResult(Convert.ToDouble(d) * 1e+9);
                    }
                    else if (textTo == l.CubicMiles)
                    {
                        ShowResult(Convert.ToDouble(d) / 4.168e+9);
                    }
                    else if (textTo == l.CubicYards)
                    {
                        ShowResult(Convert.ToDouble(d) * 1.308);
                    }
                    else if (textTo == l.CubicFTs)
                    {
                        ShowResult(Convert.ToDouble(d) * 35.315);
                    }
                    else if (textTo == l.CubicInches)
                    {
                        ShowResult(Convert.ToDouble(d) * 61024);
                    }
                    else if (textTo == l.OilBarrels)
                    {
                        ShowResult(Convert.ToDouble(d) * 6.29);
                    }
                    else if (textTo == l.GallonUS)
                    {
                        ShowResult(Convert.ToDouble(d) * 264);
                    }
                    else if (textTo == l.QuartsUS)
                    {
                        ShowResult(Convert.ToDouble(d) * 1057);
                    }
                    else if (textTo == l.PintsUS)
                    {
                        ShowResult(Convert.ToDouble(d) * 2113);
                    }
                    else if (textTo == l.FluidOuncesUS)
                    {
                        ShowResult(Convert.ToDouble(d) * 33814);
                    }
                    else if (textTo == l.Bushels)
                    {
                        ShowResult(Convert.ToDouble(d) * 28.378);
                    }
                    else if (textTo == l.Pecks)
                    {
                        ShowResult(Convert.ToDouble(d) * 114);
                    }
                    else if (textTo == l.GallonsUK)
                    {
                        ShowResult(Convert.ToDouble(d) * 220);
                    }
                    else if (textTo == l.QuartsUK)
                    {
                        ShowResult(Convert.ToDouble(d) * 880);
                    }
                    else if (textTo == l.PintsUK)
                    {
                        ShowResult(Convert.ToDouble(d) * 1760);
                    }
                    else if (textTo == l.FluidOuncesUK)
                    {
                        ShowResult(Convert.ToDouble(d) * 35195);
                    }
                }
                else if (text == l.Litres)
                {
                    if (textTo == l.CubicKilometers)
                    {
                        ShowResult(Convert.ToDouble(d) / 1e+12);
                    }
                    else if (textTo == l.CubicMeters)
                    {
                        ShowResult(Convert.ToDouble(d) / 1000);
                    }
                    else if (textTo == l.CubicCentimeters)
                    {
                        ShowResult(Convert.ToDouble(d) * 1000);
                    }
                    else if (textTo == l.CubicMillimeters)
                    {
                        ShowResult(Convert.ToDouble(d) * 1e+6);
                    }
                    else if (textTo == l.CubicMiles)
                    {
                        ShowResult(Convert.ToDouble(d) / 4.168e+12);
                    }
                    else if (textTo == l.CubicYards)
                    {
                        ShowResult(Convert.ToDouble(d) / 765);
                    }
                    else if (textTo == l.CubicFTs)
                    {
                        ShowResult(Convert.ToDouble(d) / 28.317);
                    }
                    else if (textTo == l.CubicInches)
                    {
                        ShowResult(Convert.ToDouble(d) * 61.024);
                    }
                    else if (textTo == l.OilBarrels)
                    {
                        ShowResult(Convert.ToDouble(d) / 159);
                    }
                    else if (textTo == l.GallonUS)
                    {
                        ShowResult(Convert.ToDouble(d) / 3.785);
                    }
                    else if (textTo == l.QuartsUS)
                    {
                        ShowResult(Convert.ToDouble(d) * 1.057);
                    }
                    else if (textTo == l.PintsUS)
                    {
                        ShowResult(Convert.ToDouble(d) * 2.113);
                    }
                    else if (textTo == l.FluidOuncesUS)
                    {
                        ShowResult(Convert.ToDouble(d) * 33.814);
                    }
                    else if (textTo == l.Bushels)
                    {
                        ShowResult(Convert.ToDouble(d) / 35.239);
                    }
                    else if (textTo == l.Pecks)
                    {
                        ShowResult(Convert.ToDouble(d) / 8.81);
                    }
                    else if (textTo == l.GallonsUK)
                    {
                        ShowResult(Convert.ToDouble(d) / 4.546);
                    }
                    else if (textTo == l.QuartsUK)
                    {
                        ShowResult(Convert.ToDouble(d) / 1.137);
                    }
                    else if (textTo == l.PintsUK)
                    {
                        ShowResult(Convert.ToDouble(d) * 1.76);
                    }
                    else if (textTo == l.FluidOuncesUK)
                    {
                        ShowResult(Convert.ToDouble(d) * 35.195);
                    }
                }
                else if (text == l.CubicCentimeters)
                {
                    if (textTo == l.CubicKilometers)
                    {
                        ShowResult(Convert.ToDouble(d) / 1e+15);
                    }
                    else if (textTo == l.CubicMeters)
                    {
                        ShowResult(Convert.ToDouble(d) / 1e+6);
                    }
                    else if (textTo == l.Litres)
                    {
                        ShowResult(Convert.ToDouble(d) / 1000);
                    }
                    else if (textTo == l.CubicMillimeters)
                    {
                        ShowResult(Convert.ToDouble(d) * 1000);
                    }
                    else if (textTo == l.CubicMiles)
                    {
                        ShowResult(Convert.ToDouble(d) / 4.168e+15);
                    }
                    else if (textTo == l.CubicYards)
                    {
                        ShowResult(Convert.ToDouble(d) / 764555);
                    }
                    else if (textTo == l.CubicFTs)
                    {
                        ShowResult(Convert.ToDouble(d) / 28317);
                    }
                    else if (textTo == l.CubicInches)
                    {
                        ShowResult(Convert.ToDouble(d) / 16.387);
                    }
                    else if (textTo == l.OilBarrels)
                    {
                        ShowResult(Convert.ToDouble(d) / 158987);
                    }
                    else if (textTo == l.GallonUS)
                    {
                        ShowResult(Convert.ToDouble(d) / 3785);
                    }
                    else if (textTo == l.QuartsUS)
                    {
                        ShowResult(Convert.ToDouble(d) / 946);
                    }
                    else if (textTo == l.PintsUS)
                    {
                        ShowResult(Convert.ToDouble(d) / 473);
                    }
                    else if (textTo == l.FluidOuncesUS)
                    {
                        ShowResult(Convert.ToDouble(d) / 29.574);
                    }
                    else if (textTo == l.Bushels)
                    {
                        ShowResult(Convert.ToDouble(d) / 35239);
                    }
                    else if (textTo == l.Pecks)
                    {
                        ShowResult(Convert.ToDouble(d) / 8810);
                    }
                    else if (textTo == l.GallonsUK)
                    {
                        ShowResult(Convert.ToDouble(d) / 4546);
                    }
                    else if (textTo == l.QuartsUK)
                    {
                        ShowResult(Convert.ToDouble(d) / 1137);
                    }
                    else if (textTo == l.PintsUK)
                    {
                        ShowResult(Convert.ToDouble(d) / 568);
                    }
                    else if (textTo == l.FluidOuncesUK)
                    {
                        ShowResult(Convert.ToDouble(d) / 28.413);
                    }
                }
                else if (text == l.CubicMillimeters)
                {
                    if (textTo == l.CubicKilometers)
                    {
                        ShowResult(Convert.ToDouble(d) / 1e+18);
                    }
                    else if (textTo == l.CubicMeters)
                    {
                        ShowResult(Convert.ToDouble(d) / 1e+9);
                    }
                    else if (textTo == l.Litres)
                    {
                        ShowResult(Convert.ToDouble(d) / 1e+6);
                    }
                    else if (textTo == l.CubicCentimeters)
                    {
                        ShowResult(Convert.ToDouble(d) / 1000);
                    }
                    else if (textTo == l.CubicMiles)
                    {
                        ShowResult(Convert.ToDouble(d) / 4.168e+18);
                    }
                    else if (textTo == l.CubicYards)
                    {
                        ShowResult(Convert.ToDouble(d) / 7.646e+8);
                    }
                    else if (textTo == l.CubicFTs)
                    {
                        ShowResult(Convert.ToDouble(d) / 2.832e+7);
                    }
                    else if (textTo == l.CubicInches)
                    {
                        ShowResult(Convert.ToDouble(d) / 16387);
                    }
                    else if (textTo == l.OilBarrels)
                    {
                        ShowResult(Convert.ToDouble(d) / 1.59e+8);
                    }
                    else if (textTo == l.GallonUS)
                    {
                        ShowResult(Convert.ToDouble(d) / 3.785e+6);
                    }
                    else if (textTo == l.QuartsUS)
                    {
                        ShowResult(Convert.ToDouble(d) / 946353);
                    }
                    else if (textTo == l.PintsUS)
                    {
                        ShowResult(Convert.ToDouble(d) / 473176);
                    }
                    else if (textTo == l.FluidOuncesUS)
                    {
                        ShowResult(Convert.ToDouble(d) / 29574);
                    }
                    else if (textTo == l.Bushels)
                    {
                        ShowResult(Convert.ToDouble(d) / 3.524e+7);
                    }
                    else if (textTo == l.Pecks)
                    {
                        ShowResult(Convert.ToDouble(d) / 8.81e+6);
                    }
                    else if (textTo == l.GallonsUK)
                    {
                        ShowResult(Convert.ToDouble(d) / 4.546e+6);
                    }
                    else if (textTo == l.QuartsUK)
                    {
                        ShowResult(Convert.ToDouble(d) / 1.137e+6);
                    }
                    else if (textTo == l.PintsUK)
                    {
                        ShowResult(Convert.ToDouble(d) / 568261);
                    }
                    else if (textTo == l.FluidOuncesUK)
                    {
                        ShowResult(Convert.ToDouble(d) / 28413);
                    }
                }
                else if (text == l.CubicMiles)
                {
                    if (textTo == l.CubicKilometers)
                    {
                        ShowResult(Convert.ToDouble(d) * 4.168);
                    }
                    else if (textTo == l.CubicMeters)
                    {
                        ShowResult(Convert.ToDouble(d) * 4.168e+9);
                    }
                    else if (textTo == l.Litres)
                    {
                        ShowResult(Convert.ToDouble(d) * 4.168e+12);
                    }
                    else if (textTo == l.CubicCentimeters)
                    {
                        ShowResult(Convert.ToDouble(d) * 4.168e+15);
                    }
                    else if (textTo == l.CubicMillimeters)
                    {
                        ShowResult(Convert.ToDouble(d) * 4.168e+18);
                    }
                    else if (textTo == l.CubicYards)
                    {
                        ShowResult(Convert.ToDouble(d) * 5.452e+9);
                    }
                    else if (textTo == l.CubicFTs)
                    {
                        ShowResult(Convert.ToDouble(d) * 1.472e+11);
                    }
                    else if (textTo == l.CubicInches)
                    {
                        ShowResult(Convert.ToDouble(d) * 2.544e+14);
                    }
                    else if (textTo == l.OilBarrels)
                    {
                        ShowResult(Convert.ToDouble(d) * 2.622e+10);
                    }
                    else if (textTo == l.GallonUS)
                    {
                        ShowResult(Convert.ToDouble(d) * 1.101e+12);
                    }
                    else if (textTo == l.QuartsUS)
                    {
                        ShowResult(Convert.ToDouble(d) * 4.404e+12);
                    }
                    else if (textTo == l.PintsUS)
                    {
                        ShowResult(Convert.ToDouble(d) * 8.809e+12);
                    }
                    else if (textTo == l.FluidOuncesUS)
                    {
                        ShowResult(Convert.ToDouble(d) * 1.409e+14);
                    }
                    else if (textTo == l.Bushels)
                    {
                        ShowResult(Convert.ToDouble(d) * 1.183e+11);
                    }
                    else if (textTo == l.Pecks)
                    {
                        ShowResult(Convert.ToDouble(d) * 4.731e+11);
                    }
                    else if (textTo == l.GallonsUK)
                    {
                        ShowResult(Convert.ToDouble(d) * 9.169e+11);
                    }
                    else if (textTo == l.QuartsUK)
                    {
                        ShowResult(Convert.ToDouble(d) * 3.667e+12);
                    }
                    else if (textTo == l.PintsUK)
                    {
                        ShowResult(Convert.ToDouble(d) * 7.335e+12);
                    }
                    else if (textTo == l.FluidOuncesUK)
                    {
                        ShowResult(Convert.ToDouble(d) * 1.467e+14);
                    }
                }
                else if (text == l.CubicYards)
                {
                    if (textTo == l.CubicKilometers)
                    {
                        ShowResult(Convert.ToDouble(d) / 1.308e+9);
                    }
                    else if (textTo == l.CubicMeters)
                    {
                        ShowResult(Convert.ToDouble(d) / 1.308);
                    }
                    else if (textTo == l.Litres)
                    {
                        ShowResult(Convert.ToDouble(d) * 765);
                    }
                    else if (textTo == l.CubicCentimeters)
                    {
                        ShowResult(Convert.ToDouble(d) * 764555);
                    }
                    else if (textTo == l.CubicMillimeters)
                    {
                        ShowResult(Convert.ToDouble(d) * 7.646e+8);
                    }
                    else if (textTo == l.CubicMiles)
                    {
                        ShowResult(Convert.ToDouble(d) / 5.452e+9);
                    }
                    else if (textTo == l.CubicFTs)
                    {
                        ShowResult(Convert.ToDouble(d) * 27);
                    }
                    else if (textTo == l.CubicInches)
                    {
                        ShowResult(Convert.ToDouble(d) * 46656);
                    }
                    else if (textTo == l.OilBarrels)
                    {
                        ShowResult(Convert.ToDouble(d) * 4.809);
                    }
                    else if (textTo == l.GallonUS)
                    {
                        ShowResult(Convert.ToDouble(d) * 202);
                    }
                    else if (textTo == l.QuartsUS)
                    {
                        ShowResult(Convert.ToDouble(d) * 808);
                    }
                    else if (textTo == l.PintsUS)
                    {
                        ShowResult(Convert.ToDouble(d) * 1616);
                    }
                    else if (textTo == l.FluidOuncesUS)
                    {
                        ShowResult(Convert.ToDouble(d) * 25853);
                    }
                    else if (textTo == l.Bushels)
                    {
                        ShowResult(Convert.ToDouble(d) * 21.696);
                    }
                    else if (textTo == l.Pecks)
                    {
                        ShowResult(Convert.ToDouble(d) * 86.785);
                    }
                    else if (textTo == l.GallonsUK)
                    {
                        ShowResult(Convert.ToDouble(d) * 168);
                    }
                    else if (textTo == l.QuartsUK)
                    {
                        ShowResult(Convert.ToDouble(d) * 673);
                    }
                    else if (textTo == l.PintsUK)
                    {
                        ShowResult(Convert.ToDouble(d) * 1345);
                    }
                    else if (textTo == l.FluidOuncesUK)
                    {
                        ShowResult(Convert.ToDouble(d) * 26909);
                    }
                }
                else if (text == l.CubicFTs)
                {
                    if (textTo == l.CubicKilometers)
                    {
                        ShowResult(Convert.ToDouble(d) / 3.531e+10);
                    }
                    else if (textTo == l.CubicMeters)
                    {
                        ShowResult(Convert.ToDouble(d) / 35.315);
                    }
                    else if (textTo == l.Litres)
                    {
                        ShowResult(Convert.ToDouble(d) * 28.317);
                    }
                    else if (textTo == l.CubicCentimeters)
                    {
                        ShowResult(Convert.ToDouble(d) * 28317);
                    }
                    else if (textTo == l.CubicMillimeters)
                    {
                        ShowResult(Convert.ToDouble(d) * 2.832e+7);
                    }
                    else if (textTo == l.CubicMiles)
                    {
                        ShowResult(Convert.ToDouble(d) / 1.472e+11);
                    }
                    else if (textTo == l.CubicYards)
                    {
                        ShowResult(Convert.ToDouble(d) / 27);
                    }
                    else if (textTo == l.CubicInches)
                    {
                        ShowResult(Convert.ToDouble(d) * 1728);
                    }
                    else if (textTo == l.OilBarrels)
                    {
                        ShowResult(Convert.ToDouble(d) / 5.615);
                    }
                    else if (textTo == l.GallonUS)
                    {
                        ShowResult(Convert.ToDouble(d) * 7.481);
                    }
                    else if (textTo == l.QuartsUS)
                    {
                        ShowResult(Convert.ToDouble(d) * 29.922);
                    }
                    else if (textTo == l.PintsUS)
                    {
                        ShowResult(Convert.ToDouble(d) * 59.844);
                    }
                    else if (textTo == l.FluidOuncesUS)
                    {
                        ShowResult(Convert.ToDouble(d) * 958);
                    }
                    else if (textTo == l.Bushels)
                    {
                        ShowResult(Convert.ToDouble(d) / 1.244);
                    }
                    else if (textTo == l.Pecks)
                    {
                        ShowResult(Convert.ToDouble(d) * 3.214);
                    }
                    else if (textTo == l.GallonsUK)
                    {
                        ShowResult(Convert.ToDouble(d) * 6.229);
                    }
                    else if (textTo == l.QuartsUK)
                    {
                        ShowResult(Convert.ToDouble(d) * 24.915);
                    }
                    else if (textTo == l.PintsUK)
                    {
                        ShowResult(Convert.ToDouble(d) * 49.831);
                    }
                    else if (textTo == l.FluidOuncesUK)
                    {
                        ShowResult(Convert.ToDouble(d) * 997);
                    }
                }
                else if (text == l.CubicInches)
                {
                    if (textTo == l.CubicKilometers)
                    {
                        ShowResult(Convert.ToDouble(d) / 6.102e+13);
                    }
                    else if (textTo == l.CubicMeters)
                    {
                        ShowResult(Convert.ToDouble(d) / 61024);
                    }
                    else if (textTo == l.Litres)
                    {
                        ShowResult(Convert.ToDouble(d) / 61.024);
                    }
                    else if (textTo == l.CubicCentimeters)
                    {
                        ShowResult(Convert.ToDouble(d) * 16.387);
                    }
                    else if (textTo == l.CubicMillimeters)
                    {
                        ShowResult(Convert.ToDouble(d) * 16387);
                    }
                    else if (textTo == l.CubicMiles)
                    {
                        ShowResult(Convert.ToDouble(d) / 2.544e+14);
                    }
                    else if (textTo == l.CubicYards)
                    {
                        ShowResult(Convert.ToDouble(d) / 46656);
                    }
                    else if (textTo == l.CubicFTs)
                    {
                        ShowResult(Convert.ToDouble(d) / 1728);
                    }
                    else if (textTo == l.OilBarrels)
                    {
                        ShowResult(Convert.ToDouble(d) / 9702);
                    }
                    else if (textTo == l.GallonUS)
                    {
                        ShowResult(Convert.ToDouble(d) / 231);
                    }
                    else if (textTo == l.QuartsUS)
                    {
                        ShowResult(Convert.ToDouble(d) / 57.75);
                    }
                    else if (textTo == l.PintsUS)
                    {
                        ShowResult(Convert.ToDouble(d) / 28.875);
                    }
                    else if (textTo == l.FluidOuncesUS)
                    {
                        ShowResult(Convert.ToDouble(d) / 1.805);
                    }
                    else if (textTo == l.Bushels)
                    {
                        ShowResult(Convert.ToDouble(d) / 2150);
                    }
                    else if (textTo == l.Pecks)
                    {
                        ShowResult(Convert.ToDouble(d) / 538);
                    }
                    else if (textTo == l.GallonsUK)
                    {
                        ShowResult(Convert.ToDouble(d) / 277);
                    }
                    else if (textTo == l.QuartsUK)
                    {
                        ShowResult(Convert.ToDouble(d) / 69.355);
                    }
                    else if (textTo == l.PintsUK)
                    {
                        ShowResult(Convert.ToDouble(d) / 34.677);
                    }
                    else if (textTo == l.FluidOuncesUK)
                    {
                        ShowResult(Convert.ToDouble(d) / 1.734);
                    }
                }
                else if (text == l.OilBarrels)
                {
                    if (textTo == l.CubicKilometers)
                    {
                        ShowResult(Convert.ToDouble(d) / 6.29e+9);
                    }
                    else if (textTo == l.CubicMeters)
                    {
                        ShowResult(Convert.ToDouble(d) / 6.29);
                    }
                    else if (textTo == l.Litres)
                    {
                        ShowResult(Convert.ToDouble(d) * 159);
                    }
                    else if (textTo == l.CubicCentimeters)
                    {
                        ShowResult(Convert.ToDouble(d) * 158987);
                    }
                    else if (textTo == l.CubicMillimeters)
                    {
                        ShowResult(Convert.ToDouble(d) * 1.59e+8);
                    }
                    else if (textTo == l.CubicMiles)
                    {
                        ShowResult(Convert.ToDouble(d) / 2.622e+10);
                    }
                    else if (textTo == l.CubicYards)
                    {
                        ShowResult(Convert.ToDouble(d) / 4.809);
                    }
                    else if (textTo == l.CubicFTs)
                    {
                        ShowResult(Convert.ToDouble(d) * 5.615);
                    }
                    else if (textTo == l.CubicInches)
                    {
                        ShowResult(Convert.ToDouble(d) * 9702);
                    }
                    else if (textTo == l.GallonUS)
                    {
                        ShowResult(Convert.ToDouble(d) * 42);
                    }
                    else if (textTo == l.QuartsUS)
                    {
                        ShowResult(Convert.ToDouble(d) * 168);
                    }
                    else if (textTo == l.PintsUS)
                    {
                        ShowResult(Convert.ToDouble(d) * 336);
                    }
                    else if (textTo == l.FluidOuncesUS)
                    {
                        ShowResult(Convert.ToDouble(d) * 5376);
                    }
                    else if (textTo == l.Bushels)
                    {
                        ShowResult(Convert.ToDouble(d) * 4.512);
                    }
                    else if (textTo == l.Pecks)
                    {
                        ShowResult(Convert.ToDouble(d) * 18.047);
                    }
                    else if (textTo == l.GallonsUK)
                    {
                        ShowResult(Convert.ToDouble(d) * 34.972);
                    }
                    else if (textTo == l.QuartsUK)
                    {
                        ShowResult(Convert.ToDouble(d) * 140);
                    }
                    else if (textTo == l.PintsUK)
                    {
                        ShowResult(Convert.ToDouble(d) * 280);
                    }
                    else if (textTo == l.FluidOuncesUK)
                    {
                        ShowResult(Convert.ToDouble(d) * 5596);
                    }
                }
                else if (text == l.GallonUS)
                {
                    if (textTo == l.CubicKilometers)
                    {
                        ShowResult(Convert.ToDouble(d) / 2.642e+11);
                    }
                    else if (textTo == l.CubicMeters)
                    {
                        ShowResult(Convert.ToDouble(d) / 264);
                    }
                    else if (textTo == l.Litres)
                    {
                        ShowResult(Convert.ToDouble(d) * 3.785);
                    }
                    else if (textTo == l.CubicCentimeters)
                    {
                        ShowResult(Convert.ToDouble(d) * 3785);
                    }
                    else if (textTo == l.CubicMillimeters)
                    {
                        ShowResult(Convert.ToDouble(d) * 3.785e+6);
                    }
                    else if (textTo == l.CubicMiles)
                    {
                        ShowResult(Convert.ToDouble(d) / 1.101e+12);
                    }
                    else if (textTo == l.CubicYards)
                    {
                        ShowResult(Convert.ToDouble(d) / 202);
                    }
                    else if (textTo == l.CubicFTs)
                    {
                        ShowResult(Convert.ToDouble(d) / 7.481);
                    }
                    else if (textTo == l.CubicInches)
                    {
                        ShowResult(Convert.ToDouble(d) * 231);
                    }
                    else if (textTo == l.OilBarrels)
                    {
                        ShowResult(Convert.ToDouble(d) / 42);
                    }
                    else if (textTo == l.QuartsUS)
                    {
                        ShowResult(Convert.ToDouble(d) * 4);
                    }
                    else if (textTo == l.PintsUS)
                    {
                        ShowResult(Convert.ToDouble(d) * 8);
                    }
                    else if (textTo == l.FluidOuncesUS)
                    {
                        ShowResult(Convert.ToDouble(d) * 128);
                    }
                    else if (textTo == l.Bushels)
                    {
                        ShowResult(Convert.ToDouble(d) / 9.309);
                    }
                    else if (textTo == l.Pecks)
                    {
                        ShowResult(Convert.ToDouble(d) / 2.327);
                    }
                    else if (textTo == l.GallonsUK)
                    {
                        ShowResult(Convert.ToDouble(d) / 1.201);
                    }
                    else if (textTo == l.QuartsUK)
                    {
                        ShowResult(Convert.ToDouble(d) * 3.331);
                    }
                    else if (textTo == l.PintsUK)
                    {
                        ShowResult(Convert.ToDouble(d) * 6.661);
                    }
                    else if (textTo == l.FluidOuncesUK)
                    {
                        ShowResult(Convert.ToDouble(d) * 133);
                    }
                }
                else if (text == l.QuartsUS)
                {
                    if (textTo == l.CubicKilometers)
                    {
                        ShowResult(Convert.ToDouble(d) / 1.057e+12);
                    }
                    else if (textTo == l.CubicMeters)
                    {
                        ShowResult(Convert.ToDouble(d) / 1057);
                    }
                    else if (textTo == l.Litres)
                    {
                        ShowResult(Convert.ToDouble(d) / 1.057);
                    }
                    else if (textTo == l.CubicCentimeters)
                    {
                        ShowResult(Convert.ToDouble(d) * 946);
                    }
                    else if (textTo == l.CubicMillimeters)
                    {
                        ShowResult(Convert.ToDouble(d) * 946353);
                    }
                    else if (textTo == l.CubicMiles)
                    {
                        ShowResult(Convert.ToDouble(d) / 4.404e+12);
                    }
                    else if (textTo == l.CubicYards)
                    {
                        ShowResult(Convert.ToDouble(d) / 808);
                    }
                    else if (textTo == l.CubicFTs)
                    {
                        ShowResult(Convert.ToDouble(d) / 29.922);
                    }
                    else if (textTo == l.CubicInches)
                    {
                        ShowResult(Convert.ToDouble(d) * 57.75);
                    }
                    else if (textTo == l.OilBarrels)
                    {
                        ShowResult(Convert.ToDouble(d) / 168);
                    }
                    else if (textTo == l.GallonUS)
                    {
                        ShowResult(Convert.ToDouble(d) / 4);
                    }
                    else if (textTo == l.PintsUS)
                    {
                        ShowResult(Convert.ToDouble(d) * 2);
                    }
                    else if (textTo == l.FluidOuncesUS)
                    {
                        ShowResult(Convert.ToDouble(d) * 32);
                    }
                    else if (textTo == l.Bushels)
                    {
                        ShowResult(Convert.ToDouble(d) / 37.237);
                    }
                    else if (textTo == l.Pecks)
                    {
                        ShowResult(Convert.ToDouble(d) / 9.309);
                    }
                    else if (textTo == l.GallonsUK)
                    {
                        ShowResult(Convert.ToDouble(d) / 4.804);
                    }
                    else if (textTo == l.QuartsUK)
                    {
                        ShowResult(Convert.ToDouble(d) / 1.201);
                    }
                    else if (textTo == l.PintsUK)
                    {
                        ShowResult(Convert.ToDouble(d) * 1.665);
                    }
                    else if (textTo == l.FluidOuncesUK)
                    {
                        ShowResult(Convert.ToDouble(d) * 33.307);
                    }
                }
                else if (text == l.PintsUS)
                {
                    if (textTo == l.CubicKilometers)
                    {
                        ShowResult(Convert.ToDouble(d) / 2.113e+12);
                    }
                    else if (textTo == l.CubicMeters)
                    {
                        ShowResult(Convert.ToDouble(d) / 2113);
                    }
                    else if (textTo == l.Litres)
                    {
                        ShowResult(Convert.ToDouble(d) / 2.113);
                    }
                    else if (textTo == l.CubicCentimeters)
                    {
                        ShowResult(Convert.ToDouble(d) * 473);
                    }
                    else if (textTo == l.CubicMillimeters)
                    {
                        ShowResult(Convert.ToDouble(d) * 473176);
                    }
                    else if (textTo == l.CubicMiles)
                    {
                        ShowResult(Convert.ToDouble(d) / 8.809e+12);
                    }
                    else if (textTo == l.CubicYards)
                    {
                        ShowResult(Convert.ToDouble(d) / 1616);
                    }
                    else if (textTo == l.CubicFTs)
                    {
                        ShowResult(Convert.ToDouble(d) / 59.844);
                    }
                    else if (textTo == l.CubicInches)
                    {
                        ShowResult(Convert.ToDouble(d) * 28.875);
                    }
                    else if (textTo == l.OilBarrels)
                    {
                        ShowResult(Convert.ToDouble(d) / 336);
                    }
                    else if (textTo == l.GallonUS)
                    {
                        ShowResult(Convert.ToDouble(d) / 8);
                    }
                    else if (textTo == l.QuartsUS)
                    {
                        ShowResult(Convert.ToDouble(d) / 2);
                    }
                    else if (textTo == l.FluidOuncesUS)
                    {
                        ShowResult(Convert.ToDouble(d) * 16);
                    }
                    else if (textTo == l.Bushels)
                    {
                        ShowResult(Convert.ToDouble(d) / 74.473);
                    }
                    else if (textTo == l.Pecks)
                    {
                        ShowResult(Convert.ToDouble(d) / 18.618);
                    }
                    else if (textTo == l.GallonsUK)
                    {
                        ShowResult(Convert.ToDouble(d) / 9.608);
                    }
                    else if (textTo == l.QuartsUK)
                    {
                        ShowResult(Convert.ToDouble(d) / 2.402);
                    }
                    else if (textTo == l.PintsUK)
                    {
                        ShowResult(Convert.ToDouble(d) / 1.201);
                    }
                    else if (textTo == l.FluidOuncesUK)
                    {
                        ShowResult(Convert.ToDouble(d) * 16.653);
                    }
                }
                else if (text == l.FluidOuncesUS)
                {
                    if (textTo == l.CubicKilometers)
                    {
                        ShowResult(Convert.ToDouble(d) / 3.381e+13);
                    }
                    else if (textTo == l.CubicMeters)
                    {
                        ShowResult(Convert.ToDouble(d) / 33814);
                    }
                    else if (textTo == l.Litres)
                    {
                        ShowResult(Convert.ToDouble(d) / 33.814);
                    }
                    else if (textTo == l.CubicCentimeters)
                    {
                        ShowResult(Convert.ToDouble(d) * 29.574);
                    }
                    else if (textTo == l.CubicMillimeters)
                    {
                        ShowResult(Convert.ToDouble(d) * 29574);
                    }
                    else if (textTo == l.CubicMiles)
                    {
                        ShowResult(Convert.ToDouble(d) / 1.409e+14);
                    }
                    else if (textTo == l.CubicYards)
                    {
                        ShowResult(Convert.ToDouble(d) / 25853);
                    }
                    else if (textTo == l.CubicFTs)
                    {
                        ShowResult(Convert.ToDouble(d) / 958);
                    }
                    else if (textTo == l.CubicInches)
                    {
                        ShowResult(Convert.ToDouble(d) * 1.805);
                    }
                    else if (textTo == l.OilBarrels)
                    {
                        ShowResult(Convert.ToDouble(d) / 5376);
                    }
                    else if (textTo == l.GallonUS)
                    {
                        ShowResult(Convert.ToDouble(d) / 128);
                    }
                    else if (textTo == l.QuartsUS)
                    {
                        ShowResult(Convert.ToDouble(d) / 32);
                    }
                    else if (textTo == l.PintsUS)
                    {
                        ShowResult(Convert.ToDouble(d) / 16);
                    }
                    else if (textTo == l.Bushels)
                    {
                        ShowResult(Convert.ToDouble(d) / 1192);
                    }
                    else if (textTo == l.Pecks)
                    {
                        ShowResult(Convert.ToDouble(d) / 298);
                    }
                    else if (textTo == l.GallonsUK)
                    {
                        ShowResult(Convert.ToDouble(d) / 154);
                    }
                    else if (textTo == l.QuartsUK)
                    {
                        ShowResult(Convert.ToDouble(d) / 38.43);
                    }
                    else if (textTo == l.PintsUK)
                    {
                        ShowResult(Convert.ToDouble(d) / 19.215);
                    }
                    else if (textTo == l.FluidOuncesUK)
                    {
                        ShowResult(Convert.ToDouble(d) * 1.041);
                    }
                }
                else if (text == l.Bushels)
                {
                    if (textTo == l.CubicKilometers)
                    {
                        ShowResult(Convert.ToDouble(d) / 2.838e+10);
                    }
                    else if (textTo == l.CubicMeters)
                    {
                        ShowResult(Convert.ToDouble(d) / 28.378);
                    }
                    else if (textTo == l.Litres)
                    {
                        ShowResult(Convert.ToDouble(d) * 35.239);
                    }
                    else if (textTo == l.CubicCentimeters)
                    {
                        ShowResult(Convert.ToDouble(d) * 35239);
                    }
                    else if (textTo == l.CubicMillimeters)
                    {
                        ShowResult(Convert.ToDouble(d) * 3.524e+7);
                    }
                    else if (textTo == l.CubicMiles)
                    {
                        ShowResult(Convert.ToDouble(d) / 1.183e+11);
                    }
                    else if (textTo == l.CubicYards)
                    {
                        ShowResult(Convert.ToDouble(d) / 21.696);
                    }
                    else if (textTo == l.CubicFTs)
                    {
                        ShowResult(Convert.ToDouble(d) * 1.244);
                    }
                    else if (textTo == l.CubicInches)
                    {
                        ShowResult(Convert.ToDouble(d) * 2150);
                    }
                    else if (textTo == l.OilBarrels)
                    {
                        ShowResult(Convert.ToDouble(d) / 4.512);
                    }
                    else if (textTo == l.GallonUS)
                    {
                        ShowResult(Convert.ToDouble(d) * 9.309);
                    }
                    else if (textTo == l.QuartsUS)
                    {
                        ShowResult(Convert.ToDouble(d) * 37.237);
                    }
                    else if (textTo == l.PintsUS)
                    {
                        ShowResult(Convert.ToDouble(d) * 74.473);
                    }
                    else if (textTo == l.FluidOuncesUS)
                    {
                        ShowResult(Convert.ToDouble(d) * 1192);
                    }
                    else if (textTo == l.Pecks)
                    {
                        ShowResult(Convert.ToDouble(d) * 4);
                    }
                    else if (textTo == l.GallonsUK)
                    {
                        ShowResult(Convert.ToDouble(d) * 7.752);
                    }
                    else if (textTo == l.QuartsUK)
                    {
                        ShowResult(Convert.ToDouble(d) * 31.006);
                    }
                    else if (textTo == l.PintsUK)
                    {
                        ShowResult(Convert.ToDouble(d) * 62.012);
                    }
                    else if (textTo == l.FluidOuncesUK)
                    {
                        ShowResult(Convert.ToDouble(d) * 1240);
                    }
                }
                else if (text == l.Pecks)
                {
                    if (textTo == l.CubicKilometers)
                    {
                        ShowResult(Convert.ToDouble(d) / 1.135e+11);
                    }
                    else if (textTo == l.CubicMeters)
                    {
                        ShowResult(Convert.ToDouble(d) / 114);
                    }
                    else if (textTo == l.Litres)
                    {
                        ShowResult(Convert.ToDouble(d) * 8.81);
                    }
                    else if (textTo == l.CubicCentimeters)
                    {
                        ShowResult(Convert.ToDouble(d) * 8810);
                    }
                    else if (textTo == l.CubicMillimeters)
                    {
                        ShowResult(Convert.ToDouble(d) * 8.81e+6);
                    }
                    else if (textTo == l.CubicMiles)
                    {
                        ShowResult(Convert.ToDouble(d) / 4.731e+11);
                    }
                    else if (textTo == l.CubicYards)
                    {
                        ShowResult(Convert.ToDouble(d) / 86.785);
                    }
                    else if (textTo == l.CubicFTs)
                    {
                        ShowResult(Convert.ToDouble(d) / 3.214);
                    }
                    else if (textTo == l.CubicInches)
                    {
                        ShowResult(Convert.ToDouble(d) * 538);
                    }
                    else if (textTo == l.OilBarrels)
                    {
                        ShowResult(Convert.ToDouble(d) / 18.047);
                    }
                    else if (textTo == l.GallonUS)
                    {
                        ShowResult(Convert.ToDouble(d) * 2.327);
                    }
                    else if (textTo == l.QuartsUS)
                    {
                        ShowResult(Convert.ToDouble(d) * 9.309);
                    }
                    else if (textTo == l.PintsUS)
                    {
                        ShowResult(Convert.ToDouble(d) * 18.618);
                    }
                    else if (textTo == l.FluidOuncesUS)
                    {
                        ShowResult(Convert.ToDouble(d) * 298);
                    }
                    else if (textTo == l.Bushels)
                    {
                        ShowResult(Convert.ToDouble(d) / 4);
                    }
                    else if (textTo == l.GallonsUK)
                    {
                        ShowResult(Convert.ToDouble(d) * 1.938);
                    }
                    else if (textTo == l.QuartsUK)
                    {
                        ShowResult(Convert.ToDouble(d) * 7.752);
                    }
                    else if (textTo == l.PintsUK)
                    {
                        ShowResult(Convert.ToDouble(d) * 15.503);
                    }
                    else if (textTo == l.FluidOuncesUK)
                    {
                        ShowResult(Convert.ToDouble(d) * 310);
                    }
                }
                else if (text == l.GallonsUK)
                {
                    if (textTo == l.CubicKilometers)
                    {
                        ShowResult(Convert.ToDouble(d) / 2.2e+11);
                    }
                    else if (textTo == l.CubicMeters)
                    {
                        ShowResult(Convert.ToDouble(d) / 220);
                    }
                    else if (textTo == l.Litres)
                    {
                        ShowResult(Convert.ToDouble(d) * 4.546);
                    }
                    else if (textTo == l.CubicCentimeters)
                    {
                        ShowResult(Convert.ToDouble(d) * 4546);
                    }
                    else if (textTo == l.CubicMillimeters)
                    {
                        ShowResult(Convert.ToDouble(d) * 4.546e+6);
                    }
                    else if (textTo == l.CubicMiles)
                    {
                        ShowResult(Convert.ToDouble(d) / 9.169e+11);
                    }
                    else if (textTo == l.CubicYards)
                    {
                        ShowResult(Convert.ToDouble(d) / 168);
                    }
                    else if (textTo == l.CubicFTs)
                    {
                        ShowResult(Convert.ToDouble(d) / 6.229);
                    }
                    else if (textTo == l.CubicInches)
                    {
                        ShowResult(Convert.ToDouble(d) * 277);
                    }
                    else if (textTo == l.OilBarrels)
                    {
                        ShowResult(Convert.ToDouble(d) / 34.972);
                    }
                    else if (textTo == l.GallonUS)
                    {
                        ShowResult(Convert.ToDouble(d) * 1.201);
                    }
                    else if (textTo == l.QuartsUS)
                    {
                        ShowResult(Convert.ToDouble(d) * 4.804);
                    }
                    else if (textTo == l.PintsUS)
                    {
                        ShowResult(Convert.ToDouble(d) * 9.608);
                    }
                    else if (textTo == l.FluidOuncesUS)
                    {
                        ShowResult(Convert.ToDouble(d) * 154);
                    }
                    else if (textTo == l.Bushels)
                    {
                        ShowResult(Convert.ToDouble(d) / 7.752);
                    }
                    else if (textTo == l.Pecks)
                    {
                        ShowResult(Convert.ToDouble(d) / 1.938);
                    }
                    else if (textTo == l.QuartsUK)
                    {
                        ShowResult(Convert.ToDouble(d) * 4);
                    }
                    else if (textTo == l.PintsUK)
                    {
                        ShowResult(Convert.ToDouble(d) * 8);
                    }
                    else if (textTo == l.FluidOuncesUK)
                    {
                        ShowResult(Convert.ToDouble(d) * 160);
                    }
                }
                else if (text == l.QuartsUK)
                {
                    if (textTo == l.CubicKilometers)
                    {
                        ShowResult(Convert.ToDouble(d) / 2.2e+11);
                    }
                    else if (textTo == l.CubicMeters)
                    {
                        ShowResult(Convert.ToDouble(d) / 880);
                    }
                    else if (textTo == l.Litres)
                    {
                        ShowResult(Convert.ToDouble(d) * 1.137);
                    }
                    else if (textTo == l.CubicCentimeters)
                    {
                        ShowResult(Convert.ToDouble(d) * 4546);
                    }
                    else if (textTo == l.CubicMillimeters)
                    {
                        ShowResult(Convert.ToDouble(d) * 4.546e+6);
                    }
                    else if (textTo == l.CubicMiles)
                    {
                        ShowResult(Convert.ToDouble(d) / 9.169e+11);
                    }
                    else if (textTo == l.CubicYards)
                    {
                        ShowResult(Convert.ToDouble(d) / 168);
                    }
                    else if (textTo == l.CubicFTs)
                    {
                        ShowResult(Convert.ToDouble(d) / 24.915);
                    }
                    else if (textTo == l.CubicInches)
                    {
                        ShowResult(Convert.ToDouble(d) * 69.355);
                    }
                    else if (textTo == l.OilBarrels)
                    {
                        ShowResult(Convert.ToDouble(d) / 34.972);
                    }
                    else if (textTo == l.GallonUS)
                    {
                        ShowResult(Convert.ToDouble(d) / 3.331);
                    }
                    else if (textTo == l.QuartsUS)
                    {
                        ShowResult(Convert.ToDouble(d) * 1.201);
                    }
                    else if (textTo == l.PintsUS)
                    {
                        ShowResult(Convert.ToDouble(d) * 2.402);
                    }
                    else if (textTo == l.FluidOuncesUS)
                    {
                        ShowResult(Convert.ToDouble(d) * 38.43);
                    }
                    else if (textTo == l.Bushels)
                    {
                        ShowResult(Convert.ToDouble(d) / 7.752);
                    }
                    else if (textTo == l.Pecks)
                    {
                        ShowResult(Convert.ToDouble(d) / 1.938);
                    }
                    else if (textTo == l.GallonsUK)
                    {
                        ShowResult(Convert.ToDouble(d) / 4);
                    }
                    else if (textTo == l.PintsUK)
                    {
                        ShowResult(Convert.ToDouble(d) * 4);
                    }
                    else if (textTo == l.FluidOuncesUK)
                    {
                        ShowResult(Convert.ToDouble(d) * 160);
                    }
                }
                else if (text == l.PintsUK)
                {
                    if (textTo == l.CubicKilometers)
                    {
                        ShowResult(Convert.ToDouble(d) / 1.76e+12);
                    }
                    else if (textTo == l.CubicMeters)
                    {
                        ShowResult(Convert.ToDouble(d) / 1760);
                    }
                    else if (textTo == l.Litres)
                    {
                        ShowResult(Convert.ToDouble(d) / 1.76);
                    }
                    else if (textTo == l.CubicCentimeters)
                    {
                        ShowResult(Convert.ToDouble(d) * 568);
                    }
                    else if (textTo == l.CubicMillimeters)
                    {
                        ShowResult(Convert.ToDouble(d) * 568261);
                    }
                    else if (textTo == l.CubicMiles)
                    {
                        ShowResult(Convert.ToDouble(d) / 7.335e+12);
                    }
                    else if (textTo == l.CubicYards)
                    {
                        ShowResult(Convert.ToDouble(d) / 1345);
                    }
                    else if (textTo == l.CubicFTs)
                    {
                        ShowResult(Convert.ToDouble(d) / 49.831);
                    }
                    else if (textTo == l.CubicInches)
                    {
                        ShowResult(Convert.ToDouble(d) * 34.677);
                    }
                    else if (textTo == l.OilBarrels)
                    {
                        ShowResult(Convert.ToDouble(d) / 280);
                    }
                    else if (textTo == l.GallonUS)
                    {
                        ShowResult(Convert.ToDouble(d) / 6.661);
                    }
                    else if (textTo == l.QuartsUS)
                    {
                        ShowResult(Convert.ToDouble(d) / 1.665);
                    }
                    else if (textTo == l.PintsUS)
                    {
                        ShowResult(Convert.ToDouble(d));
                    }
                    else if (textTo == l.FluidOuncesUS)
                    {
                        ShowResult(Convert.ToDouble(d) * 1.201);
                    }
                    else if (textTo == l.Bushels)
                    {
                        ShowResult(Convert.ToDouble(d) / 62.012);
                    }
                    else if (textTo == l.Pecks)
                    {
                        ShowResult(Convert.ToDouble(d) / 15.503);
                    }
                    else if (textTo == l.GallonsUK)
                    {
                        ShowResult(Convert.ToDouble(d) / 8);
                    }
                    else if (textTo == l.QuartsUK)
                    {
                        ShowResult(Convert.ToDouble(d) / 2);
                    }
                    else if (textTo == l.FluidOuncesUK)
                    {
                        ShowResult(Convert.ToDouble(d) * 20);
                    }
                }
                else if (text == l.FluidOuncesUK)
                {
                    if (textTo == l.CubicKilometers)
                    {
                        ShowResult(Convert.ToDouble(d) / 3.52e+13);
                    }
                    else if (textTo == l.CubicMeters)
                    {
                        ShowResult(Convert.ToDouble(d) / 35195);
                    }
                    else if (textTo == l.Litres)
                    {
                        ShowResult(Convert.ToDouble(d) / 35.195);
                    }
                    else if (textTo == l.CubicCentimeters)
                    {
                        ShowResult(Convert.ToDouble(d) * 28.413);
                    }
                    else if (textTo == l.CubicMillimeters)
                    {
                        ShowResult(Convert.ToDouble(d) * 28413);
                    }
                    else if (textTo == l.CubicMiles)
                    {
                        ShowResult(Convert.ToDouble(d) / 1.467e+14);
                    }
                    else if (textTo == l.CubicYards)
                    {
                        ShowResult(Convert.ToDouble(d) / 26909);
                    }
                    else if (textTo == l.CubicFTs)
                    {
                        ShowResult(Convert.ToDouble(d) / 997);
                    }
                    else if (textTo == l.CubicInches)
                    {
                        ShowResult(Convert.ToDouble(d) * 1.734);
                    }
                    else if (textTo == l.OilBarrels)
                    {
                        ShowResult(Convert.ToDouble(d) / 5596);
                    }
                    else if (textTo == l.GallonUS)
                    {
                        ShowResult(Convert.ToDouble(d) / 133);
                    }
                    else if (textTo == l.QuartsUS)
                    {
                        ShowResult(Convert.ToDouble(d) / 33.307);
                    }
                    else if (textTo == l.PintsUS)
                    {
                        ShowResult(Convert.ToDouble(d) / 16.653);
                    }
                    else if (textTo == l.FluidOuncesUS)
                    {
                        ShowResult(Convert.ToDouble(d) / 1.041);
                    }
                    else if (textTo == l.Bushels)
                    {
                        ShowResult(Convert.ToDouble(d) / 1240);
                    }
                    else if (textTo == l.Pecks)
                    {
                        ShowResult(Convert.ToDouble(d) / 310);
                    }
                    else if (textTo == l.GallonsUK)
                    {
                        ShowResult(Convert.ToDouble(d) / 160);
                    }
                    else if (textTo == l.QuartsUK)
                    {
                        ShowResult(Convert.ToDouble(d) / 40);
                    }
                    else if (textTo == l.PintsUK)
                    {
                        ShowResult(Convert.ToDouble(d) / 20);
                    }
                }
            }
            else if (cat == l.Area)
            {
                if (text == l.SquareKilometers)
                {
                    if (textTo == l.SquareMeters)
                    {
                        ShowResult(Convert.ToDouble(d) * 1e+6);
                    }
                    else if (textTo == l.SquareCentimeters)
                    {
                        ShowResult(Convert.ToDouble(d) * 1e+10);
                    }
                    else if (textTo == l.SquareMillimeters)
                    {
                        ShowResult(Convert.ToDouble(d) * 1e+12);
                    }
                    else if (textTo == l.SquareMiles)
                    {
                        ShowResult(Convert.ToDouble(d) / 2.59);
                    }
                    else if (textTo == l.SquareYards)
                    {
                        ShowResult(Convert.ToDouble(d) * 1.196e+6);
                    }
                    else if (textTo == l.SquareFTs)
                    {
                        ShowResult(Convert.ToDouble(d) * 1.076e+7);
                    }
                    else if (textTo == l.SquareInches)
                    {
                        ShowResult(Convert.ToDouble(d) * 1.55e+9);
                    }
                    else if (textTo == l.Hectares)
                    {
                        ShowResult(Convert.ToDouble(d) * 100);
                    }
                    else if (textTo == l.Acres)
                    {
                        ShowResult(Convert.ToDouble(d) * 247);
                    }
                    else if (textTo == l.Ares)
                    {
                        ShowResult(Convert.ToDouble(d) * 10000);
                    }
                }
                else if (text == l.SquareMeters)
                {
                    if (textTo == l.SquareKilometers)
                    {
                        ShowResult(Convert.ToDouble(d) / 1e+6);
                    }
                    else if (textTo == l.SquareCentimeters)
                    {
                        ShowResult(Convert.ToDouble(d) * 10000);
                    }
                    else if (textTo == l.SquareMillimeters)
                    {
                        ShowResult(Convert.ToDouble(d) * 1e+6);
                    }
                    else if (textTo == l.SquareMiles)
                    {
                        ShowResult(Convert.ToDouble(d) / 2.59e+6);
                    }
                    else if (textTo == l.SquareYards)
                    {
                        ShowResult(Convert.ToDouble(d) * 1.196);
                    }
                    else if (textTo == l.SquareFTs)
                    {
                        ShowResult(Convert.ToDouble(d) * 10.764);
                    }
                    else if (textTo == l.SquareInches)
                    {
                        ShowResult(Convert.ToDouble(d) * 1550);
                    }
                    else if (textTo == l.Hectares)
                    {
                        ShowResult(Convert.ToDouble(d) / 10000);
                    }
                    else if (textTo == l.Acres)
                    {
                        ShowResult(Convert.ToDouble(d) / 4047);
                    }
                    else if (textTo == l.Ares)
                    {
                        ShowResult(Convert.ToDouble(d) * 100);
                    }
                }
                else if (text == l.SquareCentimeters)
                {
                    if (textTo == l.SquareKilometers)
                    {
                        ShowResult(Convert.ToDouble(d) / 1e+10);
                    }
                    else if (textTo == l.SquareMeters)
                    {
                        ShowResult(Convert.ToDouble(d) / 10000);
                    }
                    else if (textTo == l.SquareMillimeters)
                    {
                        ShowResult(Convert.ToDouble(d) * 100);
                    }
                    else if (textTo == l.SquareMiles)
                    {
                        ShowResult(Convert.ToDouble(d) / 2.59e+10);
                    }
                    else if (textTo == l.SquareYards)
                    {
                        ShowResult(Convert.ToDouble(d) / 8361);
                    }
                    else if (textTo == l.SquareFTs)
                    {
                        ShowResult(Convert.ToDouble(d) / 929);
                    }
                    else if (textTo == l.SquareInches)
                    {
                        ShowResult(Convert.ToDouble(d) / 6.452);
                    }
                    else if (textTo == l.Hectares)
                    {
                        ShowResult(Convert.ToDouble(d) / 1e+8);
                    }
                    else if (textTo == l.Acres)
                    {
                        ShowResult(Convert.ToDouble(d) / 4.047e+7);
                    }
                    else if (textTo == l.Ares)
                    {
                        ShowResult(Convert.ToDouble(d) / 1e+6);
                    }
                }
                else if (text == l.SquareMillimeters)
                {
                    if (textTo == l.SquareKilometers)
                    {
                        ShowResult(Convert.ToDouble(d) / 1e+12);
                    }
                    else if (textTo == l.SquareMeters)
                    {
                        ShowResult(Convert.ToDouble(d) / 1e+6);
                    }
                    else if (textTo == l.SquareCentimeters)
                    {
                        ShowResult(Convert.ToDouble(d) / 100);
                    }
                    else if (textTo == l.SquareMiles)
                    {
                        ShowResult(Convert.ToDouble(d) / 2.59e+12);
                    }
                    else if (textTo == l.SquareYards)
                    {
                        ShowResult(Convert.ToDouble(d) / 836127);
                    }
                    else if (textTo == l.SquareFTs)
                    {
                        ShowResult(Convert.ToDouble(d) / 92903);
                    }
                    else if (textTo == l.SquareInches)
                    {
                        ShowResult(Convert.ToDouble(d) / 645);
                    }
                    else if (textTo == l.Hectares)
                    {
                        ShowResult(Convert.ToDouble(d) / 1e+10);
                    }
                    else if (textTo == l.Acres)
                    {
                        ShowResult(Convert.ToDouble(d) / 4.047e+9);
                    }
                    else if (textTo == l.Ares)
                    {
                        ShowResult(Convert.ToDouble(d) / 1e+8);
                    }
                }
                else if (text == l.SquareMiles)
                {
                    if (textTo == l.SquareKilometers)
                    {
                        ShowResult(Convert.ToDouble(d) * 2.59);
                    }
                    else if (textTo == l.SquareMeters)
                    {
                        ShowResult(Convert.ToDouble(d) * 2.59e+6);
                    }
                    else if (textTo == l.SquareCentimeters)
                    {
                        ShowResult(Convert.ToDouble(d) * 2.59e+10);
                    }
                    else if (textTo == l.SquareMillimeters)
                    {
                        ShowResult(Convert.ToDouble(d) * 2.59e+12);
                    }
                    else if (textTo == l.SquareYards)
                    {
                        ShowResult(Convert.ToDouble(d) * 3.098e+6);
                    }
                    else if (textTo == l.SquareFTs)
                    {
                        ShowResult(Convert.ToDouble(d) * 2.788e+7);
                    }
                    else if (textTo == l.SquareInches)
                    {
                        ShowResult(Convert.ToDouble(d) * 4.014e+9);
                    }
                    else if (textTo == l.Hectares)
                    {
                        ShowResult(Convert.ToDouble(d) * 259);
                    }
                    else if (textTo == l.Acres)
                    {
                        ShowResult(Convert.ToDouble(d) * 640);
                    }
                    else if (textTo == l.Ares)
                    {
                        ShowResult(Convert.ToDouble(d) * 25900);
                    }
                }
                else if (text == l.SquareYards)
                {
                    if (textTo == l.SquareKilometers)
                    {
                        ShowResult(Convert.ToDouble(d) / 1.196e+6);
                    }
                    else if (textTo == l.SquareMeters)
                    {
                        ShowResult(Convert.ToDouble(d) / 1.196);
                    }
                    else if (textTo == l.SquareCentimeters)
                    {
                        ShowResult(Convert.ToDouble(d) * 8361);
                    }
                    else if (textTo == l.SquareMillimeters)
                    {
                        ShowResult(Convert.ToDouble(d) * 836127);
                    }
                    else if (textTo == l.SquareMiles)
                    {
                        ShowResult(Convert.ToDouble(d) / 3.098e+6);
                    }
                    else if (textTo == l.SquareFTs)
                    {
                        ShowResult(Convert.ToDouble(d) * 9);
                    }
                    else if (textTo == l.SquareInches)
                    {
                        ShowResult(Convert.ToDouble(d) * 1296);
                    }
                    else if (textTo == l.Hectares)
                    {
                        ShowResult(Convert.ToDouble(d) / 11960);
                    }
                    else if (textTo == l.Acres)
                    {
                        ShowResult(Convert.ToDouble(d) / 4840);
                    }
                    else if (textTo == l.Ares)
                    {
                        ShowResult(Convert.ToDouble(d) / 120);
                    }
                }
                else if (text == l.SquareFTs)
                {
                    if (textTo == l.SquareKilometers)
                    {
                        ShowResult(Convert.ToDouble(d) / 1.076e+7);
                    }
                    else if (textTo == l.SquareMeters)
                    {
                        ShowResult(Convert.ToDouble(d) / 10.764);
                    }
                    else if (textTo == l.SquareCentimeters)
                    {
                        ShowResult(Convert.ToDouble(d) * 929);
                    }
                    else if (textTo == l.SquareMillimeters)
                    {
                        ShowResult(Convert.ToDouble(d) * 92903);
                    }
                    else if (textTo == l.SquareMiles)
                    {
                        ShowResult(Convert.ToDouble(d) / 2.788e+7);
                    }
                    else if (textTo == l.SquareYards)
                    {
                        ShowResult(Convert.ToDouble(d) / 9);
                    }
                    else if (textTo == l.SquareInches)
                    {
                        ShowResult(Convert.ToDouble(d));
                    }
                    else if (textTo == l.Hectares)
                    {
                        ShowResult(Convert.ToDouble(d) * 144);
                    }
                    else if (textTo == l.Acres)
                    {
                        ShowResult(Convert.ToDouble(d) / 43560);
                    }
                    else if (textTo == l.Ares)
                    {
                        ShowResult(Convert.ToDouble(d) / 1076);
                    }
                }
                else if (text == l.SquareInches)
                {
                    if (textTo == l.SquareKilometers)
                    {
                        ShowResult(Convert.ToDouble(d) / 1.55e+9);
                    }
                    else if (textTo == l.SquareMeters)
                    {
                        ShowResult(Convert.ToDouble(d) / 1550);
                    }
                    else if (textTo == l.SquareCentimeters)
                    {
                        ShowResult(Convert.ToDouble(d) * 6.452);
                    }
                    else if (textTo == l.SquareMillimeters)
                    {
                        ShowResult(Convert.ToDouble(d) * 645);
                    }
                    else if (textTo == l.SquareMiles)
                    {
                        ShowResult(Convert.ToDouble(d) / 4.014e+9);
                    }
                    else if (textTo == l.SquareYards)
                    {
                        ShowResult(Convert.ToDouble(d) / 1296);
                    }
                    else if (textTo == l.SquareFTs)
                    {
                        ShowResult(Convert.ToDouble(d) / 144);
                    }
                    else if (textTo == l.Hectares)
                    {
                        ShowResult(Convert.ToDouble(d) / 1.55e+7);
                    }
                    else if (textTo == l.Acres)
                    {
                        ShowResult(Convert.ToDouble(d) / 6.273e+6);
                    }
                    else if (textTo == l.Ares)
                    {
                        ShowResult(Convert.ToDouble(d) / 6.273e+6);
                    }
                }
                else if (text == l.Hectares)
                {
                    if (textTo == l.SquareKilometers)
                    {
                        ShowResult(Convert.ToDouble(d) / 100);
                    }
                    else if (textTo == l.SquareMeters)
                    {
                        ShowResult(Convert.ToDouble(d) * 10000);
                    }
                    else if (textTo == l.SquareCentimeters)
                    {
                        ShowResult(Convert.ToDouble(d) * 1e+8);
                    }
                    else if (textTo == l.SquareMillimeters)
                    {
                        ShowResult(Convert.ToDouble(d) * 1e+10);
                    }
                    else if (textTo == l.SquareMiles)
                    {
                        ShowResult(Convert.ToDouble(d) / 259);
                    }
                    else if (textTo == l.SquareYards)
                    {
                        ShowResult(Convert.ToDouble(d) * 11960);
                    }
                    else if (textTo == l.SquareFTs)
                    {
                        ShowResult(Convert.ToDouble(d) * 107639);
                    }
                    else if (textTo == l.SquareInches)
                    {
                        ShowResult(Convert.ToDouble(d) * 1.55e+7);
                    }
                    else if (textTo == l.Acres)
                    {
                        ShowResult(Convert.ToDouble(d) * 2.471);
                    }
                    else if (textTo == l.Ares)
                    {
                        ShowResult(Convert.ToDouble(d) * 100);
                    }
                }
                else if (text == l.Acres)
                {
                    if (textTo == l.SquareKilometers)
                    {
                        ShowResult(Convert.ToDouble(d) / 247);
                    }
                    else if (textTo == l.SquareMeters)
                    {
                        ShowResult(Convert.ToDouble(d) * 4047);
                    }
                    else if (textTo == l.SquareCentimeters)
                    {
                        ShowResult(Convert.ToDouble(d) * 4.047e+7);
                    }
                    else if (textTo == l.SquareMillimeters)
                    {
                        ShowResult(Convert.ToDouble(d) * 4.047e+9);
                    }
                    else if (textTo == l.SquareMiles)
                    {
                        ShowResult(Convert.ToDouble(d) / 640);
                    }
                    else if (textTo == l.SquareYards)
                    {
                        ShowResult(Convert.ToDouble(d) * 4840);
                    }
                    else if (textTo == l.SquareFTs)
                    {
                        ShowResult(Convert.ToDouble(d) * 43560);
                    }
                    else if (textTo == l.SquareInches)
                    {
                        ShowResult(Convert.ToDouble(d) * 6.273e+6);
                    }
                    else if (textTo == l.Hectares)
                    {
                        ShowResult(Convert.ToDouble(d) / 2.471);
                    }
                    else if (textTo == l.Ares)
                    {
                        ShowResult(Convert.ToDouble(d) * 40.469);
                    }
                }
                else if (text == l.Ares)
                {
                    if (textTo == l.SquareKilometers)
                    {
                        ShowResult(Convert.ToDouble(d) / 10000);
                    }
                    else if (textTo == l.SquareMeters)
                    {
                        ShowResult(Convert.ToDouble(d) * 100);
                    }
                    else if (textTo == l.SquareCentimeters)
                    {
                        ShowResult(Convert.ToDouble(d) * 1e+6);
                    }
                    else if (textTo == l.SquareMillimeters)
                    {
                        ShowResult(Convert.ToDouble(d) * 1e+8);
                    }
                    else if (textTo == l.SquareMiles)
                    {
                        ShowResult(Convert.ToDouble(d) / 25900);
                    }
                    else if (textTo == l.SquareYards)
                    {
                        ShowResult(Convert.ToDouble(d) * 120);
                    }
                    else if (textTo == l.SquareFTs)
                    {
                        ShowResult(Convert.ToDouble(d) * 1076);
                    }
                    else if (textTo == l.SquareInches)
                    {
                        ShowResult(Convert.ToDouble(d) * 155000);
                    }
                    else if (textTo == l.Hectares)
                    {
                        ShowResult(Convert.ToDouble(d) / 100);
                    }
                    else if (textTo == l.Acres)
                    {
                        ShowResult(Convert.ToDouble(d) / 40.469);
                    }
                }
            }
            else if (cat == l.Time)
            {
                if (text == l.Hours)
                {
                    if (textTo == l.Minutes)
                    {
                        ShowResult(Convert.ToDouble(d) * 60);
                    }
                    else if (textTo == l.Seconds)
                    {
                        ShowResult(Convert.ToDouble(d) * 3600);
                    }
                    else if (textTo == l.Milliseconds)
                    {
                        ShowResult(Convert.ToDouble(d) * 3.6e+6);
                    }
                    else if (textTo == l.Microseconds)
                    {
                        ShowResult(Convert.ToDouble(d) * 3.6e+9);
                    }
                }
                else if (text == l.Minutes)
                {
                    if (textTo == l.Hours)
                    {
                        ShowResult(Convert.ToDouble(d) / 60);
                    }
                    else if (textTo == l.Seconds)
                    {
                        ShowResult(Convert.ToDouble(d) * 60);
                    }
                    else if (textTo == l.Milliseconds)
                    {
                        ShowResult(Convert.ToDouble(d) * 60000);
                    }
                    else if (textTo == l.Microseconds)
                    {
                        ShowResult(Convert.ToDouble(d) * 6e+7);
                    }
                }
                else if (text == l.Seconds)
                {
                    if (textTo == l.Hours)
                    {
                        ShowResult(Convert.ToDouble(d) / 3600);
                    }
                    else if (textTo == l.Minutes)
                    {
                        ShowResult(Convert.ToDouble(d) / 60);
                    }
                    else if (textTo == l.Milliseconds)
                    {
                        ShowResult(Convert.ToDouble(d) * TimeCode.BaseUnit);
                    }
                    else if (textTo == l.Microseconds)
                    {
                        ShowResult(Convert.ToDouble(d) * 1e+6);
                    }
                }
                else if (text == l.Milliseconds)
                {
                    if (textTo == l.Hours)
                    {
                        ShowResult(Convert.ToDouble(d) / 3.6e+6);
                    }
                    else if (textTo == l.Minutes)
                    {
                        ShowResult(Convert.ToDouble(d) / 60000);
                    }
                    else if (textTo == l.Seconds)
                    {
                        ShowResult(Convert.ToDouble(d) / 1000);
                    }
                    else if (textTo == l.Microseconds)
                    {
                        ShowResult(Convert.ToDouble(d) * 1000);
                    }
                }
                else if (text == l.Microseconds)
                {
                    if (textTo == l.Hours)
                    {
                        ShowResult(Convert.ToDouble(d) / 3.6e+9);
                    }
                    else if (textTo == l.Minutes)
                    {
                        ShowResult(Convert.ToDouble(d) / 6e+7);
                    }
                    else if (textTo == l.Seconds)
                    {
                        ShowResult(Convert.ToDouble(d) / 1e+6);
                    }
                    else if (textTo == l.Milliseconds)
                    {
                        ShowResult(Convert.ToDouble(d) / 1000);
                    }
                }
            }
            else if (cat == l.Temperature)
            {
                if (text == l.Fahrenheit)
                {
                    if (textTo == l.Celsius)
                    {
                        ShowResult((Convert.ToDouble(d) - 32) * 5 / 9);
                    }
                    else if (textTo == l.Kelvin)
                    {
                        ShowResult((Convert.ToDouble(d) - 32) * 5 / 9 + 273.15);
                    }
                }
                else if (text == l.Celsius)
                {
                    if (textTo == l.Fahrenheit)
                    {
                        ShowResult(Convert.ToDouble(d) * 1.80 + 32);
                    }
                    else if (textTo == l.Kelvin)
                    {
                        ShowResult(Convert.ToDouble(d) + 273.15);
                    }
                }
                else if (text == l.Kelvin)
                {
                    if (textTo == l.Fahrenheit)
                    {
                        ShowResult((Convert.ToDouble(d) - 273.15) * 9 / 5 + 32);
                    }
                    else if (textTo == l.Celsius)
                    {
                        ShowResult(Convert.ToDouble(d) - 273.15);
                    }
                }
            }
            else if (cat == l.Velocity)
            {
                if (text == l.KilometersPerHour)
                {
                    if (textTo == l.MetersPerSecond)
                    {
                        ShowResult(Convert.ToDouble(d) / 3.6);
                    }
                    else if (textTo == l.MilesPerHour)
                    {
                        ShowResult(Convert.ToDouble(d) / 1.609);
                    }
                    else if (textTo == l.YardsPerMinute)
                    {
                        ShowResult(Convert.ToDouble(d) * 18.227);
                    }
                    else if (textTo == l.FTsPerSecond)
                    {
                        ShowResult(Convert.ToDouble(d) / 1.097);
                    }
                    else if (textTo == l.Knots)
                    {
                        ShowResult(Convert.ToDouble(d) / 1.852);
                    }
                }
                if (text == l.MetersPerSecond)
                {
                    if (textTo == l.KilometersPerHour)
                    {
                        ShowResult(Convert.ToDouble(d) * 3.6);
                    }
                    else if (textTo == l.MilesPerHour)
                    {
                        ShowResult(Convert.ToDouble(d) * 2.237);
                    }
                    else if (textTo == l.YardsPerMinute)
                    {
                        ShowResult(Convert.ToDouble(d) * 65.617);
                    }
                    else if (textTo == l.FTsPerSecond)
                    {
                        ShowResult(Convert.ToDouble(d) * 3.281);
                    }
                    else if (textTo == l.Knots)
                    {
                        ShowResult(Convert.ToDouble(d) * 1.944);
                    }
                }
                if (text == l.MilesPerHour)
                {
                    if (textTo == l.KilometersPerHour)
                    {
                        ShowResult(Convert.ToDouble(d) * 1.609);
                    }
                    else if (textTo == l.MetersPerSecond)
                    {
                        ShowResult(Convert.ToDouble(d) / 2.237);
                    }
                    else if (textTo == l.YardsPerMinute)
                    {
                        ShowResult(Convert.ToDouble(d) * 29.333);
                    }
                    else if (textTo == l.FTsPerSecond)
                    {
                        ShowResult(Convert.ToDouble(d) * 1.467);
                    }
                    else if (textTo == l.Knots)
                    {
                        ShowResult(Convert.ToDouble(d) / 1.151);
                    }
                }
                if (text == l.YardsPerMinute)
                {
                    if (textTo == l.KilometersPerHour)
                    {
                        ShowResult(Convert.ToDouble(d) / 18.227);
                    }
                    else if (textTo == l.MetersPerSecond)
                    {
                        ShowResult(Convert.ToDouble(d) / 65.617);
                    }
                    else if (textTo == l.MilesPerHour)
                    {
                        ShowResult(Convert.ToDouble(d) / 29.333);
                    }
                    else if (textTo == l.FTsPerSecond)
                    {
                        ShowResult(Convert.ToDouble(d) / 20);
                    }
                    else if (textTo == l.Knots)
                    {
                        ShowResult(Convert.ToDouble(d) / 33.756);
                    }
                }
                if (text == l.FTsPerSecond)
                {
                    if (textTo == l.KilometersPerHour)
                    {
                        ShowResult(Convert.ToDouble(d) * 1.097);
                    }
                    else if (textTo == l.MetersPerSecond)
                    {
                        ShowResult(Convert.ToDouble(d) / 3.281);
                    }
                    else if (textTo == l.MilesPerHour)
                    {
                        ShowResult(Convert.ToDouble(d) / 1.467);
                    }
                    else if (textTo == l.YardsPerMinute)
                    {
                        ShowResult(Convert.ToDouble(d) * 20);
                    }
                    else if (textTo == l.Knots)
                    {
                        ShowResult(Convert.ToDouble(d) / 1.688);
                    }
                }
                if (text == l.Knots)
                {
                    if (textTo == l.KilometersPerHour)
                    {
                        ShowResult(Convert.ToDouble(d) * 1.852);
                    }
                    else if (textTo == l.MetersPerSecond)
                    {
                        ShowResult(Convert.ToDouble(d) / 1.944);
                    }
                    else if (textTo == l.MilesPerHour)
                    {
                        ShowResult(Convert.ToDouble(d) * 1.151);
                    }
                    else if (textTo == l.YardsPerMinute)
                    {
                        ShowResult(Convert.ToDouble(d) * 33.756);
                    }
                    else if (textTo == l.FTsPerSecond)
                    {
                        ShowResult(Convert.ToDouble(d) * 1.688);
                    }
                }
            }
            else if (cat == l.Force)
            {
                if (text == l.PoundsForce)
                {
                    if (textTo == l.Newtons)
                    {
                        ShowResult(Convert.ToDouble(d) * 4.448);
                    }
                    else if (textTo == l.KilosForce)
                    {
                        ShowResult(Convert.ToDouble(d) / 2.205);
                    }
                }
                if (text == l.Newtons)
                {
                    if (textTo == l.PoundsForce)
                    {
                        ShowResult(Convert.ToDouble(d) / 4.448);
                    }
                    else if (textTo == l.KilosForce)
                    {
                        ShowResult(Convert.ToDouble(d) / 9.807);
                    }
                }
                if (text == l.KilosForce)
                {
                    if (textTo == l.PoundsForce)
                    {
                        ShowResult(Convert.ToDouble(d) * 2.205);
                    }
                    else if (textTo == l.Newtons)
                    {
                        ShowResult(Convert.ToDouble(d) * 9.807);
                    }
                }
            }
            else if (cat == l.Energy)
            {
                if (text == l.Jouls)
                {
                    if (textTo == l.Calories)
                    {
                        ShowResult(Convert.ToDouble(d) / 4.184);
                    }
                    else if (textTo == l.Ergs)
                    {
                        ShowResult(Convert.ToDouble(d) / 10000000);
                    }
                    else if (textTo == l.ElectronVolts)
                    {
                        ShowResult(Convert.ToDouble(d) * 6.242e+18);
                    }
                    else if (textTo == l.Btus)
                    {
                        ShowResult(Convert.ToDouble(d) / 1055);
                    }
                }
                else if (text == l.Calories)
                {
                    if (textTo == l.Jouls)
                    {
                        ShowResult(Convert.ToDouble(d) * 4.184);
                    }
                    else if (textTo == l.Ergs)
                    {
                        ShowResult(Convert.ToDouble(d) * 41840000);
                    }
                    else if (textTo == l.ElectronVolts)
                    {
                        ShowResult(Convert.ToDouble(d) * 9.223e+18);
                    }
                    else if (textTo == l.Btus)
                    {
                        ShowResult(Convert.ToDouble(d) * 3.966);
                    }
                }
                else if (text == l.Ergs)
                {
                    if (textTo == l.Jouls)
                    {
                        ShowResult(Convert.ToDouble(d) / 10000000);
                    }
                    else if (textTo == l.Calories)
                    {
                        ShowResult(Convert.ToDouble(d) / 4.184e+10);
                    }
                    else if (textTo == l.ElectronVolts)
                    {
                        ShowResult(Convert.ToDouble(d) * 6.242e+11);
                    }
                    else if (textTo == l.Btus)
                    {
                        ShowResult(Convert.ToDouble(d) / 1.055e+10);
                    }
                }
                else if (text == l.ElectronVolts)
                {
                    if (textTo == l.Jouls)
                    {
                        ShowResult(Convert.ToDouble(d) / 6.242e+18);
                    }
                    else if (textTo == l.Calories)
                    {
                        ShowResult(Convert.ToDouble(d) / 9.223e+18);
                    }
                    else if (textTo == l.Ergs)
                    {
                        ShowResult(Convert.ToDouble(d) / 6.242e+11);
                    }
                    else if (textTo == l.Btus)
                    {
                        ShowResult(Convert.ToDouble(d) / 9.223e+18);
                    }
                }
                else if (text == l.Btus)
                {
                    if (textTo == l.Jouls)
                    {
                        ShowResult(Convert.ToDouble(d) * 1055);
                    }
                    else if (textTo == l.Calories)
                    {
                        ShowResult(Convert.ToDouble(d) / 3.966);
                    }
                    else if (textTo == l.Ergs)
                    {
                        ShowResult(Convert.ToDouble(d) * 1.055e+10);
                    }
                    else if (textTo == l.ElectronVolts)
                    {
                        ShowResult(Convert.ToDouble(d) * 9.223e+18);
                    }
                }
            }
            else if (cat == l.Power)
            {
                if (text == l.Watts)
                {
                    if (textTo == l.Horsepower)
                    {
                        ShowResult(Convert.ToDouble(d) / 746);
                    }
                }
                else if (text == l.Horsepower)
                {
                    if (textTo == l.Watts)
                    {
                        ShowResult(Convert.ToDouble(d) * 746);
                    }
                }
            }
            else if (cat == l.Pressure)
            {
                if (text == l.Atmospheres)
                {
                    if (textTo == l.Bars)
                    {
                        ShowResult(Convert.ToDouble(d) * 1.013);
                    }
                    else if (textTo == l.Pascals)
                    {
                        ShowResult(Convert.ToDouble(d) * 101325);
                    }
                    else if (textTo == l.MillimetersOfMercury)
                    {
                        ShowResult(Convert.ToDouble(d) * 760);
                    }
                    else if (textTo == l.PoundPerSquareInch)
                    {
                        ShowResult(Convert.ToDouble(d) * 14.696);
                    }
                    else if (textTo == l.KilogramPerSquareCentimeter)
                    {
                        ShowResult(Convert.ToDouble(d) * 1.033);
                    }
                    else if (textTo == l.KiloPascals)
                    {
                        ShowResult(Convert.ToDouble(d) * 101);
                    }
                }
                if (text == l.Bars)
                {
                    if (textTo == l.Atmospheres)
                    {
                        ShowResult(Convert.ToDouble(d) / 1.013);
                    }
                    else if (textTo == l.Pascals)
                    {
                        ShowResult(Convert.ToDouble(d) * 100000);
                    }
                    else if (textTo == l.MillimetersOfMercury)
                    {
                        ShowResult(Convert.ToDouble(d) * 750);
                    }
                    else if (textTo == l.PoundPerSquareInch)
                    {
                        ShowResult(Convert.ToDouble(d) * 14.504);
                    }
                    else if (textTo == l.KilogramPerSquareCentimeter)
                    {
                        ShowResult(Convert.ToDouble(d) * 1.02);
                    }
                    else if (textTo == l.KiloPascals)
                    {
                        ShowResult(Convert.ToDouble(d) * 100);
                    }
                }
                if (text == l.Pascals)
                {
                    if (textTo == l.Atmospheres)
                    {
                        ShowResult(Convert.ToDouble(d) / 101325);
                    }
                    else if (textTo == l.Bars)
                    {
                        ShowResult(Convert.ToDouble(d) / 100000);
                    }
                    else if (textTo == l.MillimetersOfMercury)
                    {
                        ShowResult(Convert.ToDouble(d) / 133);
                    }
                    else if (textTo == l.PoundPerSquareInch)
                    {
                        ShowResult(Convert.ToDouble(d) / 6895);
                    }
                    else if (textTo == l.KilogramPerSquareCentimeter)
                    {
                        ShowResult(Convert.ToDouble(d) / 98067);
                    }
                    else if (textTo == l.KiloPascals)
                    {
                        ShowResult(Convert.ToDouble(d) / 1000);
                    }
                }
                if (text == l.MillimetersOfMercury)
                {
                    if (textTo == l.Atmospheres)
                    {
                        ShowResult(Convert.ToDouble(d) / 760);
                    }
                    else if (textTo == l.Bars)
                    {
                        ShowResult(Convert.ToDouble(d) / 750);
                    }
                    else if (textTo == l.Pascals)
                    {
                        ShowResult(Convert.ToDouble(d) * 133);
                    }
                    else if (textTo == l.PoundPerSquareInch)
                    {
                        ShowResult(Convert.ToDouble(d) / 51.715);
                    }
                    else if (textTo == l.KilogramPerSquareCentimeter)
                    {
                        ShowResult(Convert.ToDouble(d) / 736);
                    }
                    else if (textTo == l.KiloPascals)
                    {
                        ShowResult(Convert.ToDouble(d) / 7.501);
                    }
                }
                if (text == l.PoundPerSquareInch)
                {
                    if (textTo == l.Atmospheres)
                    {
                        ShowResult(Convert.ToDouble(d) / 14.696);
                    }
                    else if (textTo == l.Bars)
                    {
                        ShowResult(Convert.ToDouble(d) / 14.504);
                    }
                    else if (textTo == l.Pascals)
                    {
                        ShowResult(Convert.ToDouble(d) * 6895);
                    }
                    else if (textTo == l.MillimetersOfMercury)
                    {
                        ShowResult(Convert.ToDouble(d) * 51.715);
                    }
                    else if (textTo == l.KilogramPerSquareCentimeter)
                    {
                        ShowResult(Convert.ToDouble(d) / 14.223);
                    }
                    else if (textTo == l.KiloPascals)
                    {
                        ShowResult(Convert.ToDouble(d) * 6.895);
                    }
                }
                if (text == l.KilogramPerSquareCentimeter)
                {
                    if (textTo == l.Atmospheres)
                    {
                        ShowResult(Convert.ToDouble(d) / 1.033);
                    }
                    else if (textTo == l.Bars)
                    {
                        ShowResult(Convert.ToDouble(d) / 1.02);
                    }
                    else if (textTo == l.Pascals)
                    {
                        ShowResult(Convert.ToDouble(d) * 98067);
                    }
                    else if (textTo == l.MillimetersOfMercury)
                    {
                        ShowResult(Convert.ToDouble(d) * 736);
                    }
                    else if (textTo == l.PoundPerSquareInch)
                    {
                        ShowResult(Convert.ToDouble(d) * 14.223);
                    }
                    else if (textTo == l.KiloPascals)
                    {
                        ShowResult(Convert.ToDouble(d) * 98.066);
                    }
                }
                if (text == l.KiloPascals)
                {
                    if (textTo == l.Atmospheres)
                    {
                        ShowResult(Convert.ToDouble(d) / 101);
                    }
                    else if (textTo == l.Bars)
                    {
                        ShowResult(Convert.ToDouble(d) / 100);
                    }
                    else if (textTo == l.Pascals)
                    {
                        ShowResult(Convert.ToDouble(d) * 1000);
                    }
                    else if (textTo == l.MillimetersOfMercury)
                    {
                        ShowResult(Convert.ToDouble(d) * 7.501);
                    }
                    else if (textTo == l.PoundPerSquareInch)
                    {
                        ShowResult(Convert.ToDouble(d) / 6.895);
                    }
                    else if (textTo == l.KilogramPerSquareCentimeter)
                    {
                        ShowResult(Convert.ToDouble(d) / 98.066);
                    }
                }
            }
        }

        private void textBoxInput_KeyUp(object sender, KeyEventArgs e)
        {
            textBoxInput_TextChanged(null, null);
        }

        private void textBoxInput_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (char.IsControl(e.KeyChar) || char.IsDigit(e.KeyChar))
            {
                return;
            }

            if (e.KeyChar == '.' && !((sender as TextBox).Text.IndexOf('.') > -1) || e.KeyChar == ',' || e.KeyChar == '-' && !((sender as TextBox).Text.IndexOf('-') > -1))
            {
                return;
            }

            e.Handled = true;
        }

        private void listBoxCategory_SelectedIndexChanged(object sender, EventArgs e)
        {
            string cat = listBoxCategory.SelectedItem.ToString();
            var l = LanguageSettings.Current.MeasurementConverter;

            comboBoxFrom.Items.Clear();

            if (cat == l.Length)
            {
                comboBoxFrom.Items.AddRange(_length.ToArray<object>());
            }
            else if (cat == l.Mass)
            {
                comboBoxFrom.Items.AddRange(_mass.ToArray<object>());
            }
            else if (cat == l.Volume)
            {
                comboBoxFrom.Items.AddRange(_volume.ToArray<object>());
            }
            else if (cat == l.Area)
            {
                comboBoxFrom.Items.AddRange(_area.ToArray<object>());
            }
            else if (cat == l.Time)
            {
                comboBoxFrom.Items.AddRange(_time.ToArray<object>());
            }
            else if (cat == l.Temperature)
            {
                comboBoxFrom.Items.AddRange(_temperature.ToArray<object>());
            }
            else if (cat == l.Velocity)
            {
                comboBoxFrom.Items.AddRange(_velocity.ToArray<object>());
            }
            else if (cat == l.Force)
            {
                comboBoxFrom.Items.AddRange(_force.ToArray<object>());
            }
            else if (cat == l.Energy)
            {
                comboBoxFrom.Items.AddRange(_energy.ToArray<object>());
            }
            else if (cat == l.Power)
            {
                comboBoxFrom.Items.AddRange(_power.ToArray<object>());
            }
            else if (cat == l.Pressure)
            {
                comboBoxFrom.Items.AddRange(_pressure.ToArray<object>());
            }

            if (comboBoxFrom.Items.Count > 0)
            {
                if (comboBoxFrom.Items.Contains(_fields[1]))
                {
                    comboBoxFrom.SelectedItem = _fields[1];
                }
                else
                {
                    comboBoxFrom.SelectedIndex = 0;
                }
            }

            comboBoxFrom_SelectedIndexChanged(null, null);
        }

        private void listBoxCategory_LostFocus(object sender, EventArgs e)
        {
            // avoid flickering when losing focus
            listBoxCategory.Update();
        }

        private void MeasurementConverter_Activated(object sender, EventArgs e)
        {
            Opacity = 1;
            textBoxInput.Text = double.TryParse(Input, out _) ? Input : "1";
        }

        private void MeasurementConverter_FormClosed(object sender, FormClosedEventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void MeasurementConverter_Deactivate(object sender, EventArgs e)
        {
            if (DialogResult != DialogResult.Cancel)
            {
                Opacity = 0.5;
            }
        }

        private void MeasurementConverter_Load(object sender, EventArgs e)
        {
            CenterToParent();
        }
    }
}
