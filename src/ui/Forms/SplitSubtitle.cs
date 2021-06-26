using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Logic;
using System;
using System.IO;
using System.Text;
using System.Windows.Forms;

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
            UiUtil.PreInitialize(this);
            InitializeComponent();
            UiUtil.FixFonts(this);

            Text = LanguageSettings.Current.SplitSubtitle.Title;
            label1.Text = LanguageSettings.Current.SplitSubtitle.Description1;
            label2.Text = LanguageSettings.Current.SplitSubtitle.Description2;
            buttonSplit.Text = LanguageSettings.Current.SplitSubtitle.Split;
            buttonDone.Text = LanguageSettings.Current.SplitSubtitle.Done;
            buttonAdvanced.Text = LanguageSettings.Current.General.Advanced;
            labelHourMinSecMilliSecond.Text = Configuration.Settings.General.UseTimeFormatHHMMSSFF ? LanguageSettings.Current.General.HourMinutesSecondsFrames : string.Format(LanguageSettings.Current.General.HourMinutesSecondsDecimalSeparatorMilliseconds, UiUtil.DecimalSeparator);
            buttonGetFrameRate.Left = splitTimeUpDownAdjust.Left + splitTimeUpDownAdjust.Width;

            label2.Top = label1.Bottom;
            if (Width < label1.Right + 5)
            {
                Width = label1.Right + 5;
            }

            UiUtil.FixLargeFonts(this, buttonSplit);
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
            {
                DialogResult = DialogResult.Cancel;
            }
        }

        private void ButtonSplitClick(object sender, EventArgs e)
        {
            var splitTimeTotalMilliseconds = splitTimeUpDownAdjust.TimeCode.TotalMilliseconds;
            if (splitTimeTotalMilliseconds > 0)
            {
                var part1 = new Subtitle();
                var part2 = new Subtitle();
                part1.Header = _subtitle.Header;
                part1.Footer = _subtitle.Footer;
                part2.Header = _subtitle.Header;
                part2.Footer = _subtitle.Footer;

                foreach (Paragraph p in _subtitle.Paragraphs)
                {
                    if (p.StartTime.TotalMilliseconds < splitTimeTotalMilliseconds)
                    {
                        part1.Paragraphs.Add(new Paragraph(p));
                    }

                    if (p.StartTime.TotalMilliseconds >= splitTimeTotalMilliseconds)
                    {
                        part2.Paragraphs.Add(new Paragraph(p));
                    }
                    else if (p.EndTime.TotalMilliseconds > splitTimeTotalMilliseconds)
                    {
                        part1.Paragraphs[part1.Paragraphs.Count - 1].EndTime = new TimeCode(splitTimeTotalMilliseconds);
                        part2.Paragraphs.Add(new Paragraph(p) { StartTime = new TimeCode(splitTimeTotalMilliseconds) });
                    }
                }
                if (part1.Paragraphs.Count > 0 && part2.Paragraphs.Count > 0)
                {
                    SavePart(part1, LanguageSettings.Current.SplitSubtitle.SavePartOneAs, LanguageSettings.Current.SplitSubtitle.Part1);

                    part2.AddTimeToAllParagraphs(TimeSpan.FromMilliseconds(-splitTimeTotalMilliseconds));
                    part2.Renumber();
                    SavePart(part2, LanguageSettings.Current.SplitSubtitle.SavePartTwoAs, LanguageSettings.Current.SplitSubtitle.Part2);

                    DialogResult = DialogResult.OK;
                    return;
                }
                MessageBox.Show(LanguageSettings.Current.SplitSubtitle.NothingToSplit);
            }
            DialogResult = DialogResult.Cancel;
        }

        private void SavePart(Subtitle part, string title, string name)
        {
            saveFileDialog1.Title = title;
            saveFileDialog1.FileName = name;
            UiUtil.SetSaveDialogFilter(saveFileDialog1, _format);
            saveFileDialog1.DefaultExt = "*" + _format.Extension;
            saveFileDialog1.AddExtension = true;

            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                string fileName = saveFileDialog1.FileName;

                try
                {
                    if (File.Exists(fileName))
                    {
                        File.Delete(fileName);
                    }

                    int index = 0;
                    foreach (SubtitleFormat format in SubtitleFormat.AllSubtitleFormats)
                    {
                        if (saveFileDialog1.FilterIndex == index + 1)
                        {
                            File.WriteAllText(fileName, part.ToText(format), _encoding);
                        }
                        index++;
                    }
                }
                catch
                {
                    MessageBox.Show(string.Format(LanguageSettings.Current.SplitSubtitle.UnableToSaveFileX, fileName));
                }
            }
        }

        private void buttonGetFrameRate_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(openFileDialog1.InitialDirectory) && !string.IsNullOrEmpty(_fileName))
            {
                openFileDialog1.InitialDirectory = Path.GetDirectoryName(_fileName);
            }

            openFileDialog1.Title = LanguageSettings.Current.General.OpenVideoFileTitle;
            openFileDialog1.FileName = string.Empty;
            openFileDialog1.Filter = UiUtil.GetVideoFileFilter(true);
            openFileDialog1.FileName = string.Empty;
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                VideoInfo info = UiUtil.GetVideoInfo(openFileDialog1.FileName);
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
