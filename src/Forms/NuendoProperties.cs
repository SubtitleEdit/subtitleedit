using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Nikse.SubtitleEdit.Logic;

namespace Nikse.SubtitleEdit.Forms
{
    public partial class NuendoProperties : Form
    {

        public string CharacterListFile { get; set; }

        public NuendoProperties()
        {
            InitializeComponent();
            labelStatus.Text = string.Empty;
            textBoxCharacterFile.Text = Configuration.Settings.SubtitleSettings.NuendoCharacterListFile;
        }

        private void ButtonChooseCharacter_Click(object sender, EventArgs e)
        {
            openFileDialog1.Filter = "Csv files|*.csv";
            openFileDialog1.FileName = string.Empty;
            if (openFileDialog1.ShowDialog(this) == DialogResult.OK)
            {
                textBoxCharacterFile.Text = openFileDialog1.FileName;

                int count = LoadCharacters(openFileDialog1.FileName).Count;
                if (count == 0)
                {
                    labelStatus.Text = "No characters found!";
                }
                else
                {
                    labelStatus.Text = string.Format("{0} characters found", count);
                }
            }
        }

        private void Csv2Properties_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
                DialogResult = DialogResult.Cancel;
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            CharacterListFile = textBoxCharacterFile.Text;
            DialogResult = DialogResult.OK;
        }

        public static List<string> LoadCharacters(string fileName)
        {
            int lineNumber = 0;
            const string Seperator = ";";
            List<string> characters = new List<string>();
            foreach (string line in System.IO.File.ReadAllLines(fileName))
            {
                string[] parts = line.Split(Seperator.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length > 0)
                {
                    try
                    {
                        string text = Utilities.FixQuotes(parts[0]);
                        if (text.Trim().Length > 0)
                        {
                            if (characters.IndexOf(text) == -1)
                            {
                                if (parts.Length > 1)
                                    text = text + " [" + Utilities.FixQuotes(parts[1]) + "]";

                                if (lineNumber == 0 && (text.ToLower().StartsWith("character [") || text.ToLower() == "character"))
                                {
                                }
                                else
                                {
                                    characters.Add(text);
                                }
                            }
                        }
                    }
                    catch
                    {
                    }
                }
                lineNumber++;
            }
            return characters;
        }

    }
}
