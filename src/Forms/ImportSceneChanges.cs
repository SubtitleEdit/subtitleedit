using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using Nikse.SubtitleEdit.Logic;

namespace Nikse.SubtitleEdit.Forms
{
    public partial class ImportSceneChanges : Form
    {

        public List<double> SceneChangesInSeconds = new List<double>();
        private double _frameRate = 25;
        public ImportSceneChanges(VideoInfo videoInfo, string videoFileName)
        {
            InitializeComponent();
            if (videoInfo != null && videoInfo.FramesPerSecond > 1)
                _frameRate = videoInfo.FramesPerSecond;

            Text = Configuration.Settings.Language.ImportSceneChanges.Title;
            groupBoxImportText.Text = Configuration.Settings.Language.ImportSceneChanges.Title;
            buttonOpenText.Text = Configuration.Settings.Language.ImportSceneChanges.OpenTextFile;
            groupBoxImportOptions.Text = Configuration.Settings.Language.ImportSceneChanges.ImportOptions;
            radioButtonFrames.Text = Configuration.Settings.Language.ImportSceneChanges.Frames;
            radioButtonSeconds.Text = Configuration.Settings.Language.ImportSceneChanges.Seconds;
            radioButtonMilliseconds.Text = Configuration.Settings.Language.ImportSceneChanges.Milliseconds;
            groupBoxTimeCodes.Text = Configuration.Settings.Language.ImportSceneChanges.TimeCodes;
            buttonOK.Text = Configuration.Settings.Language.General.OK;
            buttonCancel.Text = Configuration.Settings.Language.General.Cancel;
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

        private void buttonOpenText_Click(object sender, EventArgs e)
        {
            openFileDialog1.Title = buttonOpenText.Text;
            openFileDialog1.Filter = Configuration.Settings.Language.ImportText.TextFiles + "|*.txt|" + Configuration.Settings.Language.General.AllFiles + "|*.*";
            openFileDialog1.FileName = string.Empty;
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                LoadTextFile(openFileDialog1.FileName);
            }
        }

        private void LoadTextFile(string fileName)
        {
            try
            {
                Encoding encoding = Utilities.GetEncodingFromFile(fileName);
                string s = File.ReadAllText(fileName, encoding).Trim();
                if (s.Contains("."))
                    radioButtonSeconds.Checked = true;
                if (!s.Contains(Environment.NewLine) && s.Contains(";"))
                {
                    var sb = new StringBuilder();
                    foreach (string line in s.Split(';'))
                    {
                        if (line.Trim().Length > 0)
                            sb.AppendLine(line.Trim());
                    }
                    textBoxText.Text = sb.ToString();
                }
                else
                {
                    textBoxText.Text = s;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            SceneChangesInSeconds = new List<double>();
            foreach (string line in textBoxText.Lines)
            {
                double d;
                if (double.TryParse(line, out d))
                {
                    if (radioButtonFrames.Checked)
                    {
                        SceneChangesInSeconds.Add(d / _frameRate);
                    }
                    else if (radioButtonSeconds.Checked)
                    {
                        SceneChangesInSeconds.Add(d);
                    }
                    else
                    {
                        SceneChangesInSeconds.Add(d / 1000.0);
                    }
                }
            }
            DialogResult = DialogResult.OK;
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void ImportSceneChanges_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
                DialogResult = DialogResult.Cancel;
        }

    }
}
