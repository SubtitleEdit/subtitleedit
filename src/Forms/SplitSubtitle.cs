using System;
using System.IO;
using System.Text;
using System.Windows.Forms;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.SubtitleFormats;

namespace Nikse.SubtitleEdit.Forms
{
    public sealed partial class SplitSubtitle : Form
    {
        private Subtitle _subtitle;
        private SubtitleFormat _format;
        private Encoding _encoding;
        private string _fileName;
        public bool ShowAdvanced { get; private set; }

        public SplitSubtitle()
        {
            InitializeComponent();

            Text = Configuration.Settings.Language.SplitSubtitle.Title;
            label1.Text = Configuration.Settings.Language.SplitSubtitle.Description1;
            label2.Text = Configuration.Settings.Language.SplitSubtitle.Description2;
            buttonSplit.Text = Configuration.Settings.Language.SplitSubtitle.Split;
            buttonDone.Text = Configuration.Settings.Language.SplitSubtitle.Done;
            buttonAdvanced.Text = Configuration.Settings.Language.General.Advanced;
            labelHourMinSecMilliSecond.Text = Configuration.Settings.Language.General.HourMinutesSecondsMilliseconds;
            buttonGetFrameRate.Left = splitTimeUpDownAdjust.Left + splitTimeUpDownAdjust.Width;

            label2.Top = label1.Bottom;
            if (Width < label1.Right + 5)
                Width = label1.Right + 5;

            Utilities.FixLargeFonts(this, buttonSplit);
        }

        public void Initialize(Subtitle subtitle, string fileName, SubtitleFormat format, Encoding encoding, double lengthInSeconds)
        {
            ShowAdvanced = false;
            _subtitle = subtitle;
            _fileName = fileName;
            _format = format;
            _encoding = encoding;
            splitTimeUpDownAdjust.TimeCode = TimeCode.FromSeconds(lengthInSeconds);
        }

        private void FormSplitSubtitle_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
                DialogResult = DialogResult.Cancel;
        }

        private TimeSpan GetSplitTime()
        {
            return splitTimeUpDownAdjust.TimeCode.TimeSpan;
        }

        private void ButtonSplitClick(object sender, EventArgs e)
        {
            var splitTime = GetSplitTime();
            if (splitTime.TotalSeconds > 0)
            {
                var part1 = new Subtitle();
                var part2 = new Subtitle();
                part1.Header = _subtitle.Header;
                part2.Header = _subtitle.Header;

                foreach (Paragraph p in _subtitle.Paragraphs)
                {
                    if (p.StartTime.TotalMilliseconds < splitTime.TotalMilliseconds)
                    {
                        part1.Paragraphs.Add(new Paragraph(p));
                    }

                    if (p.StartTime.TotalMilliseconds >= splitTime.TotalMilliseconds)
                    {
                        part2.Paragraphs.Add(new Paragraph(p));
                    }
                    else if (p.EndTime.TotalMilliseconds > splitTime.TotalMilliseconds)
                    {
                        part1.Paragraphs[part1.Paragraphs.Count - 1].EndTime = new TimeCode(splitTime.TotalMilliseconds);
                        part2.Paragraphs.Add(new Paragraph(p) { StartTime = new TimeCode(splitTime.TotalMilliseconds) });
                    }
                }
                if (part1.Paragraphs.Count > 0 && part2.Paragraphs.Count > 0)
                {
                    SavePart(part1, Configuration.Settings.Language.SplitSubtitle.SavePartOneAs, Configuration.Settings.Language.SplitSubtitle.Part1);

                    part2.AddTimeToAllParagraphs(TimeSpan.FromMilliseconds(-splitTime.TotalMilliseconds));
                    part2.Renumber();
                    SavePart(part2, Configuration.Settings.Language.SplitSubtitle.SavePartTwoAs, Configuration.Settings.Language.SplitSubtitle.Part2);

                    DialogResult = DialogResult.OK;
                    return;
                }
                MessageBox.Show(Configuration.Settings.Language.SplitSubtitle.NothingToSplit);
            }
            DialogResult = DialogResult.Cancel;
        }

        private void SavePart(Subtitle part, string title, string name)
        {
            saveFileDialog1.Title = title;
            saveFileDialog1.FileName = name;
            Utilities.SetSaveDialogFilter(saveFileDialog1, _format);
            saveFileDialog1.DefaultExt = "*" + _format.Extension;
            saveFileDialog1.AddExtension = true;

            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                string fileName = saveFileDialog1.FileName;

                try
                {
                    if (File.Exists(fileName))
                        File.Delete(fileName);

                    int index = 0;
                    foreach (SubtitleFormat format in SubtitleFormat.AllSubtitleFormats)
                    {
                        if (saveFileDialog1.FilterIndex == index + 1)
                        {
                            if (format.IsFrameBased)
                                part.CalculateFrameNumbersFromTimeCodesNoCheck(Configuration.Settings.General.CurrentFrameRate);
                            File.WriteAllText(fileName, part.ToText(format), _encoding);
                        }
                        index++;
                    }
                }
                catch
                {
                    MessageBox.Show(string.Format(Configuration.Settings.Language.SplitSubtitle.UnableToSaveFileX, fileName));
                }
            }
        }

        private void buttonGetFrameRate_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(openFileDialog1.InitialDirectory) && !string.IsNullOrEmpty(_fileName))
                openFileDialog1.InitialDirectory = Path.GetDirectoryName(_fileName);

            openFileDialog1.Title = Configuration.Settings.Language.General.OpenVideoFileTitle;
            openFileDialog1.FileName = string.Empty;
            openFileDialog1.Filter = Utilities.GetVideoFileFilter(true);
            openFileDialog1.FileName = string.Empty;
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                VideoInfo info = Utilities.GetVideoInfo(openFileDialog1.FileName);
                if (info != null && info.Success)
                {
                    splitTimeUpDownAdjust.TimeCode = new TimeCode(info.TotalMilliseconds);
                }
            }
        }

        private void buttonAdvanced_Click(object sender, EventArgs e)
        {
            ShowAdvanced = true;
            DialogResult = DialogResult.Cancel;
        }

    }
}