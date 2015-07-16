using System;
using System.Globalization;
using System.Text;
using System.Windows.Forms;
using Nikse.SubtitleEdit.Core;
using Nikse.SubtitleEdit.Logic;
using System.Collections.Generic;

namespace Nikse.SubtitleEdit.Forms
{
    public sealed partial class Watermark : Form
    {
        private const char ZeroWidthSpace = '\u200B';
        private const char zeroWidthNoBreakSpace = '\uFEFF';

        private int _firstSelectedIndex;

        public Watermark()
        {
            InitializeComponent();
            Utilities.FixLargeFonts(this, buttonOK);
        }

        internal void Initialize(Logic.Subtitle subtitle, int firstSelectedIndex)
        {
            StringBuilder sb = new StringBuilder();
            foreach (Paragraph p in subtitle.Paragraphs)
                sb.AppendLine(p.Text);

            string watermark = ReadWaterMark(sb.ToString().Trim());
            LabelWatermark.Text = string.Format("Watermark: {0}", watermark);
            if (watermark.Length == 0)
            {
                buttonRemove.Enabled = false;
                textBoxWatermark.Focus();
            }
            else
            {
                groupBoxGenerate.Enabled = false;
                buttonOK.Focus();
            }

            _firstSelectedIndex = firstSelectedIndex;
            Paragraph current = subtitle.GetParagraphOrDefault(_firstSelectedIndex);
            if (current != null)
                radioButtonCurrentLine.Text = radioButtonCurrentLine.Text + " " + current.Text.Replace(Environment.NewLine, Configuration.Settings.General.ListViewLineSeparatorString);
            else
                radioButtonCurrentLine.Enabled = false;
        }

        private static string ReadWaterMark(string input)
        {
            if (!input.Contains(ZeroWidthSpace))
                return string.Empty;
            int i = 0;
            StringBuilder sb = new StringBuilder();
            bool letterOn = false;
            int letter = 0;
            while (i < input.Length)
            {
                var c = input[i];
                if (c == ZeroWidthSpace)
                {
                    if (letter > 0)
                        sb.Append(Encoding.ASCII.GetString(new byte[] { (byte)letter }));
                    letterOn = true;
                    letter = 0;
                }
                else if (c == zeroWidthNoBreakSpace && letterOn)
                {
                    letter++;
                }
                else
                {
                    if (letter > 0)
                        sb.Append(Encoding.ASCII.GetString(new byte[] { (byte)letter }));
                    letterOn = false;
                    letter = 0;
                }
                i++;
            }
            return sb.ToString();
        }

        private void AddWaterMark(Subtitle subtitle, string input)
        {
            if (subtitle == null || subtitle.Paragraphs.Count == 0)
            {
                return;
            }

            byte[] buffer = Encoding.ASCII.GetBytes(input);

            if (radioButtonCurrentLine.Checked)
            {
                StringBuilder sb = new StringBuilder();
                foreach (byte b in buffer)
                {
                    sb.Append(ZeroWidthSpace);
                    for (int i = 0; i < b; i++)
                        sb.Append(zeroWidthNoBreakSpace);
                }
                Paragraph p = subtitle.GetParagraphOrDefault(_firstSelectedIndex);
                if (p != null)
                {
                    if (p.Text.Length > 1)
                        p.Text = p.Text.Insert(p.Text.Length / 2, sb.ToString());
                    else
                        p.Text = sb + p.Text;
                }
            }
            else
            {
                Random r = new Random();
                List<int> indices = new List<int>();
                foreach (byte b in buffer)
                {
                    int number = r.Next(subtitle.Paragraphs.Count - 1);
                    if (indices.Contains(number))
                        number = r.Next(subtitle.Paragraphs.Count - 1);
                    if (indices.Contains(number))
                        number = r.Next(subtitle.Paragraphs.Count - 1);
                    indices.Add(number);
                }

                indices.Sort();
                int j = 0;
                foreach (byte b in buffer)
                {
                    StringBuilder sb = new StringBuilder();
                    Paragraph p = subtitle.Paragraphs[indices[j]];
                    sb.Append(ZeroWidthSpace);
                    for (int i = 0; i < b; i++)
                        sb.Append(zeroWidthNoBreakSpace);
                    if (p.Text.Length > 1)
                        p.Text = p.Text.Insert(p.Text.Length / 2, sb.ToString());
                    else
                        p.Text = sb + p.Text;
                    j++;
                }
            }
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        internal void AddOrRemove(Subtitle subtitle)
        {
            if (groupBoxGenerate.Enabled)
                AddWaterMark(subtitle, textBoxWatermark.Text);
            else
                RemoveWaterMark(subtitle);
        }

        private static void RemoveWaterMark(Subtitle subtitle)
        {
            var zws = ZeroWidthSpace.ToString(CultureInfo.InvariantCulture);
            var zwnbs = zeroWidthNoBreakSpace.ToString(CultureInfo.InvariantCulture);
            foreach (Paragraph p in subtitle.Paragraphs)
                p.Text = p.Text.Replace(zws, string.Empty).Replace(zwnbs, string.Empty);
        }

        private void buttonGenerate_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
        }

        private void buttonRemove_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
        }

        private void Watermark_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
                DialogResult = DialogResult.Cancel;
        }
    }
}
