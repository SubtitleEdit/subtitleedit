using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.SubtitleFormats;

namespace Nikse.SubtitleEdit.Forms
{
    public partial class Statistics : Form
    {
        private Subtitle _subtitle;
        private SubtitleFormat _format;
        LanguageStructure.Statistics _l;

        public Statistics(Subtitle subtitle, string fileName, SubtitleFormat format)
        {
            InitializeComponent();

            _subtitle = subtitle;
            _format = format;

            _l = Configuration.Settings.Language.Statistics;
            if (string.IsNullOrEmpty(fileName))
                Text = _l.Title;
            else
                Text = string.Format(_l.TitleWithFileName, fileName);
            groupBoxGeneral.Text = _l.GeneralStatistics;
            groupBoxMostUsed.Text = _l.MostUsed;
            labelMostUsedWords.Text = _l.MostUsedWords;
            labelMostUsedLines.Text = _l.MostUsedLines;
            buttonOK.Text = Configuration.Settings.Language.General.OK;
            FixLargeFonts();

            CalculateGeneralStatistics();
            CalculateMostUsedWords();
            CalculateMostUsedLines();
        }

        private void CalculateGeneralStatistics()
        {
            if (_subtitle == null || _subtitle.Paragraphs.Count == 0)
            {
                textBoxGeneral.Text = _l.NothingFound;
                return;
            }

            var allText = new StringBuilder();
            int minimumLineLength = 99999999;
            int maximumLineLength = 0;
            long totalLineLength = 0;
            int minimumSingleLineLength = 99999999;
            int maximumSingleLineLength = 0;
            long totalSingleLineLength = 0;
            long totalSingleLines = 0;
            double minimumDuration = 100000000;
            double maximumDuration = 0;
            double totalDuration = 0;
            foreach (Paragraph p in _subtitle.Paragraphs)
            {
                allText.Append(p.Text);
                
                int len = p.Text.Length;
                if (len < minimumLineLength)
                    minimumLineLength = len;
                if (len > maximumLineLength)
                    maximumLineLength = len;
                totalLineLength += len;

                double duration = p.Duration.TotalMilliseconds;
                if (duration < minimumDuration)
                    minimumDuration = duration;
                if (duration > maximumDuration)
                    maximumDuration = duration;
                totalDuration += duration;

                foreach (string line in p.Text.Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries))
                {
                    len = line.Length;
                    if (len < minimumSingleLineLength)
                        minimumSingleLineLength = len;
                    if (len > maximumSingleLineLength)
                        maximumSingleLineLength = len;
                    totalSingleLineLength += len;
                    totalSingleLines++;
                }
            }

            var sb = new StringBuilder();
            int sourceLength = _subtitle.ToText(_format).Length;
            sb.AppendLine(string.Format(_l.NumberOfLinesX, _subtitle.Paragraphs.Count));
            sb.AppendLine(string.Format(_l.LengthInFormatXinCharactersY, _format.FriendlyName, sourceLength));
            sb.AppendLine(string.Format(_l.NumberOfCharactersInTextOnly, allText.Length));
            sb.AppendLine(string.Format(_l.TotalCharsPerSecond, Utilities.RemoveHtmlTags(allText.ToString()).Length / (totalDuration / 1000.0)));
            sb.AppendLine(string.Format(_l.NumberOfItalicTags, Utilities.CountTagInText(allText.ToString().ToLower(), "<i>")));
            sb.AppendLine(string.Format(_l.NumberOfBoldTags, Utilities.CountTagInText(allText.ToString().ToLower(), "<b>")));
            sb.AppendLine(string.Format(_l.NumberOfUnderlineTags, Utilities.CountTagInText(allText.ToString().ToLower(), "<u>")));
            sb.AppendLine(string.Format(_l.NumberOfFontTags, Utilities.CountTagInText(allText.ToString().ToLower(), "<font ")));
            sb.AppendLine(string.Format(_l.NumberOfAlignmentTags, Utilities.CountTagInText(allText.ToString().ToLower(), "{\\a")));
            sb.AppendLine();
            sb.AppendLine(string.Format(_l.LineLengthMinimum, minimumLineLength));
            sb.AppendLine(string.Format(_l.LineLengthMaximum, maximumLineLength));
            sb.AppendLine(string.Format(_l.LineLengthAvarage, totalLineLength / _subtitle.Paragraphs.Count));
            sb.AppendLine(string.Format(_l.LinesPerSubtitleAvarage, (((double)totalSingleLines) / _subtitle.Paragraphs.Count)));
            sb.AppendLine();
            sb.AppendLine(string.Format(_l.SingleLineLengthMinimum, minimumSingleLineLength));
            sb.AppendLine(string.Format(_l.SingleLineLengthMaximum, maximumSingleLineLength));
            sb.AppendLine(string.Format(_l.SingleLineLengthAvarage, totalSingleLineLength / totalSingleLines));
            sb.AppendLine();
            sb.AppendLine(string.Format(_l.DurationMinimum, minimumDuration / 1000.0));
            sb.AppendLine(string.Format(_l.DurationMaximum, maximumDuration / 1000.0));
            sb.AppendLine(string.Format(_l.DurationAvarage, totalDuration / _subtitle.Paragraphs.Count / 1000.0));
            sb.AppendLine();
            textBoxGeneral.Text = sb.ToString().Trim();
            textBoxGeneral.SelectionStart = 0;
            textBoxGeneral.SelectionLength = 0;
            textBoxGeneral.ScrollToCaret();
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
        private void Statistics_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
                DialogResult = DialogResult.Cancel;
        }

        private void MostUsedWordsAdd(Dictionary<string, string> hashtable, string lastLine)
        {
            lastLine = lastLine.Trim('\'');
            lastLine = lastLine.Replace("\"", "");
            lastLine = lastLine.Replace("<i>", "");
            lastLine = lastLine.Replace("</i>", ".");
            lastLine = lastLine.Replace("<I>", "");
            lastLine = lastLine.Replace("</I>", ".");
            lastLine = lastLine.Replace("<b>", "");
            lastLine = lastLine.Replace("</b>", ".");
            lastLine = lastLine.Replace("<B>", "");
            lastLine = lastLine.Replace("</B>", ".");
            string[] words = lastLine.Split(" ,!?.:;-_@<>/0123456789".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

            foreach (string word in words)
            {
                string s = word.Trim();

                if (s.Length > 1 && hashtable.ContainsKey(s))
                {
                    int hits = int.Parse(hashtable[s].ToString());
                    hits++;
                    hashtable[s] = hits.ToString();
                }
                else if (s.Length > 1)
                {
                    hashtable.Add(s, "1");
                }
            }
        }

        private void MostUsedLinesAdd(Dictionary<string, string> hashtable, string lastLine)
        {
            lastLine = lastLine.Trim('\'');
            lastLine = lastLine.Replace("\"", "");
            lastLine = lastLine.Replace("<i>", "");
            lastLine = lastLine.Replace("</i>", ".");
            lastLine = lastLine.Replace("<I>", "");
            lastLine = lastLine.Replace("</I>", ".");
            lastLine = lastLine.Replace("<b>", "");
            lastLine = lastLine.Replace("</b>", ".");
            lastLine = lastLine.Replace("<B>", "");
            lastLine = lastLine.Replace("</B>", ".");
            lastLine = lastLine.Replace('!', '.');
            lastLine = lastLine.Replace('?', '.');
            lastLine = lastLine.Replace("...", ".");
            lastLine = lastLine.Replace("..", ".");
            lastLine = lastLine.Replace("-", " ");
            lastLine = lastLine.Replace("   ", " ");
            lastLine = lastLine.Replace("  ", " ");
            string[] lines = lastLine.Split('.');

            foreach (string line in lines)
            {
                string s = line.Trim();

                if (hashtable.ContainsKey(s))
                {
                    int hits = int.Parse(hashtable[s].ToString());
                    hits++;
                    hashtable[s] = hits.ToString();
                }
                else if (s.Length > 0)
                {
                    if (s.Contains(" "))
                    {
                        hashtable.Add(s, "1");
                    }
                }
            }
        }

        private void CalculateMostUsedWords()
        {
            Dictionary<string, string> hashtable = new Dictionary<string, string>();

            foreach (Paragraph p in _subtitle.Paragraphs)
                MostUsedWordsAdd(hashtable, p.Text);

            SortedDictionary<string, string> sortedTable = new SortedDictionary<string, string>();
            foreach (KeyValuePair<string, string> item in hashtable)
            {
                if (int.Parse(item.Value) > 1)
                {
                    string s = item.Value;
                    while (s.Length < 4)
                        s = "0" + s;
                    sortedTable.Add(s + "_" + item.Key, item.Value + ": " + item.Key);
                }
            }

            var sb = new StringBuilder();
            if (sortedTable.Count > 0)
            {
                string temp = "";
                foreach (KeyValuePair<string, string> item in sortedTable)
                {
                    temp = item.Value + Environment.NewLine + temp;
                }
                sb.AppendLine(temp);
            }
            else
            {
                sb.AppendLine(_l.NothingFound);
            }
            textBoxMostUsedWords.Text = sb.ToString();
        }

        private void CalculateMostUsedLines()
        {
            Dictionary<string, string> hashtable = new Dictionary<string, string>();
            
            foreach (Paragraph p in _subtitle.Paragraphs)
                MostUsedLinesAdd(hashtable, p.Text.Replace(Environment.NewLine, " ").Replace("  ", " "));
            

            SortedDictionary<string, string> sortedTable = new SortedDictionary<string, string>();
            foreach (KeyValuePair<string, string> item in hashtable)
            {
                if (int.Parse(item.Value) > 1)
                {
                    string s = item.Value;
                    while (s.Length < 4)
                        s = "0" + s;
                    sortedTable.Add(s + "_" + item.Key, item.Value + ": " + item.Key);
                }
            }

            var sb = new StringBuilder();
            if (sortedTable.Count > 0)
            {
                string temp = "";
                foreach (KeyValuePair<string, string> item in sortedTable)
                {
                    temp = item.Value + Environment.NewLine + temp;
                }
                sb.AppendLine(temp);
            }
            else
            {
                sb.AppendLine(_l.NothingFound);
            }
            textBoxMostUsedLines.Text = sb.ToString();
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
        }

    }
}
