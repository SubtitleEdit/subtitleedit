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
        Subtitle _subtitle;
        SubtitleFormat _format;
        Encoding _encoding;
        private string _fileName;

        public SplitSubtitle()
        {
            InitializeComponent();

            Text = Configuration.Settings.Language.SplitSubtitle.Title;
            label1.Text = Configuration.Settings.Language.SplitSubtitle.Description1;
            label2.Text = Configuration.Settings.Language.SplitSubtitle.Description2;
            buttonSplit.Text = Configuration.Settings.Language.SplitSubtitle.Split;
            buttonDone.Text = Configuration.Settings.Language.SplitSubtitle.Done;
            labelHoursMinSecsMilliSecs.Text = Configuration.Settings.Language.General.HourMinutesSecondsMilliseconds;
        }

        public void Initialize(Subtitle subtitle, string fileName, SubtitleFormat format, Encoding encoding, double lengthInSeconds)
        {
            _subtitle = subtitle;
            _fileName = fileName;
            _format = format;
            _encoding = encoding;
            splitTimeUpDownAdjust.TimeCode = new TimeCode(TimeSpan.FromSeconds(lengthInSeconds));
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

        private void ButtonFixClick(object sender, EventArgs e)
        {
            TimeSpan splitTime = GetSplitTime();
            if (splitTime.TotalSeconds > 0)
            {
                var part1 = new Subtitle();
                var part2 = new Subtitle();

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
                        p.StartTime = new TimeCode(0, 0, 0, 1);
                    }
                }
                if (part1.Paragraphs.Count > 0 && part2.Paragraphs.Count > 0)
                {
                    SavePart(part1, Configuration.Settings.Language.SplitSubtitle.SavePartOneAs, Configuration.Settings.Language.SplitSubtitle.Part1);

                    part2.AddTimeToAllParagraphs(TimeSpan.FromMilliseconds(-splitTime.TotalMilliseconds));
                    part2.Renumber(1);
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
                            File.WriteAllText(fileName, part.ToText(format), _encoding);
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

            openFileDialog1.Title = Configuration.Settings.Language.General.OpenVideoFile;
            openFileDialog1.FileName = string.Empty;
            openFileDialog1.Filter = Utilities.GetVideoFileFilter();
            openFileDialog1.FileName = string.Empty;
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                VideoInfo info = Utilities.GetVideoInfo(openFileDialog1.FileName, delegate { Application.DoEvents(); });
                if (info != null && info.Success)
                {
                    splitTimeUpDownAdjust.TimeCode = new TimeCode(TimeSpan.FromMilliseconds(info.TotalMilliseconds));
                }
            }
        }

    }
}
