using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Nikse.SubtitleEdit.Logic;
using System.Globalization;
using System.IO;

namespace Nikse.SubtitleEdit.Forms
{
    public partial class ExtractDateTimeInfo : Form
    {
        public Subtitle DateTimeSubtitle { get; private set; }
        public string VideoFileName { get; private set; }
        private List<string> _formats = new List<string>();

        public ExtractDateTimeInfo()
        {
            InitializeComponent();
            comboBoxDateTimeFormats.SelectedIndex = 0;
            labelVideoFileName.Text = string.Empty;
            timeUpDownStartTime.TimeCode = new TimeCode(0, 0, 0, 0);
            timeUpDownDuration.TimeCode = new TimeCode(1, 0, 0, 0);
            comboBoxDateTimeFormats.Items.Clear();
            foreach (string format in Configuration.Settings.Tools.GenerateTimeCodePatterns.Split(';'))
            {
                _formats.Add(format);
                comboBoxDateTimeFormats.Items.Add(format + "   " + DecodeFormat(DateTime.Now, format).Replace(Environment.NewLine, "<br />"));
            }
            if (_formats.Count > 0)
                comboBoxDateTimeFormats.SelectedIndex = 0;
        }

        private string DecodeFormat(DateTime dateTime, string format)
        {
            try
            {
                var sb = new StringBuilder();
                foreach (string s in format.Replace("<br />", "@").Replace("<br/>", "@").Replace("<br>", "@").Split('@'))
                {
                    sb.AppendLine(dateTime.ToString(format));
                }
                return sb.ToString().Trim();
            }
            catch
            {
                return "Error";
            }
        }

        private void buttonGenerateTimeInfo_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
        }

        private void buttonOpenVideo_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog(this) == DialogResult.OK)
            {
                VideoFileName = openFileDialog1.FileName;
                labelVideoFileName.Text = VideoFileName;

                DateTime start;
                double durationInSeconds;
                string ext = Path.GetExtension(VideoFileName).ToLower();
                if (ext == ".mp4" || ext == ".m4v" || ext == ".3gp")
                {
                    Logic.Mp4.Mp4Parser mp4Parser = new Logic.Mp4.Mp4Parser(VideoFileName);
                    start = mp4Parser.CreationDate;
                    durationInSeconds = mp4Parser.Duration.TotalSeconds;
                }
                else
                {
                    var fi = new FileInfo(VideoFileName);
                    start = fi.CreationTime;
                    VideoInfo vi = Utilities.GetVideoInfo(VideoFileName, null);
                    durationInSeconds = vi.TotalMilliseconds / 1000.0;
                    if (durationInSeconds < 1)
                    {
                        MessageBox.Show("Unable to get duration");
                        durationInSeconds = 60 * 60;
                    }
                }
                dateTimePicker1.Value = start;
                timeUpDownStartTime.TimeCode = new TimeCode(start.Hour, start.Minute, start.Second, start.Millisecond);
                timeUpDownDuration.TimeCode = new TimeCode(TimeSpan.FromSeconds(durationInSeconds));
            }
        }

        private DateTime GetStartDateTime()
        {
            return new DateTime(dateTimePicker1.Value.Year, dateTimePicker1.Value.Month, dateTimePicker1.Value.Day,
                                timeUpDownStartTime.TimeCode.Hours, timeUpDownStartTime.TimeCode.Minutes,
                                timeUpDownStartTime.TimeCode.Seconds, timeUpDownStartTime.TimeCode.Milliseconds);
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            DateTimeSubtitle = new Subtitle();
            DateTime start = GetStartDateTime();
            double durationInSeconds = timeUpDownDuration.TimeCode.TotalSeconds;
            for (int i = 0; i < durationInSeconds; i++)
            {
                Paragraph p = new Paragraph();
                p.Text = FormatDateTime(start);
                start = start.AddSeconds(1);
                p.StartTime = new TimeCode(TimeSpan.FromSeconds(i));
                p.EndTime = new TimeCode(TimeSpan.FromSeconds(i + 0.999));
                DateTimeSubtitle.Paragraphs.Add(p);
            }
            DialogResult = DialogResult.OK;
        }

        private string FormatDateTime(DateTime dt)
        {
            if (comboBoxDateTimeFormats.SelectedIndex < _formats.Count)
                return DecodeFormat(dt, _formats[comboBoxDateTimeFormats.SelectedIndex]);
            return "";
        }

        private void comboBoxDateTimeFormats_SelectedIndexChanged(object sender, EventArgs e)
        {
            textBoxExample.Text = FormatDateTime(DateTime.Now);
        }
    }
}
