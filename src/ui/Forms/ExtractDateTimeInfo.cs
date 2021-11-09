using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.ContainerFormats.Mp4;
using Nikse.SubtitleEdit.Logic;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.Forms
{
    public sealed partial class ExtractDateTimeInfo : PositionAndSizeForm
    {
        public Subtitle DateTimeSubtitle { get; private set; }
        public string VideoFileName { get; private set; }
        private readonly List<string> _formats = new List<string>();

        public ExtractDateTimeInfo()
        {
            UiUtil.PreInitialize(this);
            InitializeComponent();
            UiUtil.FixFonts(this);
            comboBoxDateTimeFormats.SelectedIndex = 0;
            labelVideoFileName.Text = string.Empty;
            timeUpDownStartTime.TimeCode = new TimeCode();
            timeUpDownDuration.TimeCode = new TimeCode(1, 0, 0, 0);
            comboBoxDateTimeFormats.Items.Clear();
            buttonCancel.Text = LanguageSettings.Current.General.Cancel;
            foreach (string format in Configuration.Settings.Tools.GenerateTimeCodePatterns.Split(';'))
            {
                _formats.Add(format);
                comboBoxDateTimeFormats.Items.Add(format);
            }
            if (_formats.Count > 0)
            {
                comboBoxDateTimeFormats.SelectedIndex = 0;
            }

            var l = LanguageSettings.Current.ExtractDateTimeInfo;
            Text = l.Title;
            labelChooseVideoFile.Text = l.OpenVideoFile;
            labelStartFrom.Text = l.StartFrom;
            labelDuration.Text = LanguageSettings.Current.General.Duration;
            labelExample.Text = l.Example;
            buttonOK.Text = l.GenerateSubtitle;
            labelWriteFormat.Text = l.DateTimeFormat;
        }

        private static string DecodeFormat(DateTime dateTime, string format)
        {
            try
            {
                var sb = new StringBuilder();
                foreach (string s in format.Replace("<br />", "|").Replace("<BR />", "|").Replace("<br/>", "|").Replace("<BR/>", "|").Replace("<br>", "|").Replace("<BR>", "|").Split('|'))
                {
                    sb.AppendLine(dateTime.ToString(s));
                }
                return sb.ToString().Trim();
            }
            catch
            {
                return "Error";
            }
        }

        private void buttonOpenVideo_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog(this) == DialogResult.OK)
            {
                VideoFileName = openFileDialog1.FileName;
                labelVideoFileName.Text = VideoFileName;

                DateTime start;
                double durationInSeconds;
                string ext = Path.GetExtension(VideoFileName).ToLowerInvariant();
                if (ext == ".mp4" || ext == ".m4v" || ext == ".3gp")
                {
                    MP4Parser mp4Parser = new MP4Parser(VideoFileName);
                    start = mp4Parser.CreationDate;
                    durationInSeconds = mp4Parser.Duration.TotalSeconds;
                }
                else
                {
                    var fi = new FileInfo(VideoFileName);
                    start = fi.CreationTime;
                    VideoInfo vi = UiUtil.GetVideoInfo(VideoFileName);
                    durationInSeconds = vi.TotalMilliseconds / TimeCode.BaseUnit;
                    if (durationInSeconds < 1)
                    {
                        MessageBox.Show("Unable to get duration");
                        durationInSeconds = 60 * 60;
                    }
                }
                dateTimePicker1.Value = start;
                timeUpDownStartTime.TimeCode = new TimeCode(start.Hour, start.Minute, start.Second, start.Millisecond);
                timeUpDownDuration.TimeCode = TimeCode.FromSeconds(durationInSeconds);
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
                p.StartTime = TimeCode.FromSeconds(i);
                p.EndTime = TimeCode.FromSeconds(i + 0.999);
                DateTimeSubtitle.Paragraphs.Add(p);
            }
            DialogResult = DialogResult.OK;
        }

        private string FormatDateTime(DateTime dt)
        {
            return DecodeFormat(dt, comboBoxDateTimeFormats.Text);
        }

        private void comboBoxDateTimeFormats_SelectedIndexChanged(object sender, EventArgs e)
        {
            textBoxExample.Text = FormatDateTime(DateTime.Now);
        }

        private void comboBoxDateTimeFormats_TextChanged(object sender, EventArgs e)
        {
            textBoxExample.Text = FormatDateTime(DateTime.Now);
        }

        private void ExtractDateTimeInfo_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                e.SuppressKeyPress = true;
                DialogResult = DialogResult.Cancel;
            }
        }

    }
}
