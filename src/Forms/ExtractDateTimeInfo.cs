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

        public ExtractDateTimeInfo()
        {
            InitializeComponent();
            comboBoxDateTimeFormats.SelectedIndex = 0;
            labelVideoFileName.Text = string.Empty;
            timeUpDownStartTime.TimeCode = new TimeCode(12, 0, 0, 0);
            timeUpDownDuration.TimeCode = new TimeCode(1, 0, 0, 0);
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
            DateTime start; 
            double durationInSeconds;

            if (Path.GetExtension(VideoFileName).ToLower() == ".mp4" || Path.GetExtension(VideoFileName).ToLower() == ".m4v" || Path.GetExtension(VideoFileName).ToLower() == ".3gp")
            {
                Logic.Mp4.Mp4Parser mp4Parser = new Logic.Mp4.Mp4Parser(VideoFileName);                
                start = mp4Parser.CreationDate;
                durationInSeconds = mp4Parser.Duration.TotalSeconds;
            }
            else if (Path.GetExtension(VideoFileName).ToLower() == ".mkv" || Path.GetExtension(VideoFileName).ToLower() == ".webm")
            {
                bool isValid;
                var matroska = new Matroska();
                var subtitleList = matroska.GetMatroskaSubtitleTracks(VideoFileName, out isValid);
                if (isValid)
                {

                    bool hasConstantFrameRate = false;
                    double frameRate = 0;
                    int pixelWidth = 0;
                    int pixelHeight = 0;
                    double millisecsDuration = 0;
                    string videoCodec = string.Empty;
                    matroska.GetMatroskaInfo(VideoFileName, ref isValid, ref hasConstantFrameRate, ref frameRate, ref pixelWidth, ref pixelHeight, ref millisecsDuration, ref videoCodec);
                    durationInSeconds = millisecsDuration / 1000.0;

                    //TODO: read creation date from mkv file!??
                    FileInfo fi = new FileInfo(VideoFileName);
                    start = fi.CreationTime;
                }
                else
                {
                    MessageBox.Show("Invalid matroska file");
                    return;
                }
            }
            else
            {
                FileInfo fi = new FileInfo(VideoFileName);
                start = fi.CreationTime;
                VideoInfo vi = Utilities.GetVideoInfo(VideoFileName, null);                                        
                durationInSeconds = vi.TotalSeconds;
                if (durationInSeconds < 1)
                {
                    MessageBox.Show("Unable to get duration");
                    return;
                }
            }

            start = GetStartDateTime();
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
            if (comboBoxDateTimeFormats.SelectedIndex == 4)
                return dt.ToString(CultureInfo.CurrentUICulture);
            else if (comboBoxDateTimeFormats.SelectedIndex == 3)
                return dt.ToString("dd MMM yyyy, HH:mm:ss");
            else if (comboBoxDateTimeFormats.SelectedIndex == 2)
                return dt.ToString("F");
            else if (comboBoxDateTimeFormats.SelectedIndex == 1)
                return dt.ToString("M/d/yyyy") + Environment.NewLine + dt.ToString("hh:mm:ss");
            else
                return dt.ToString("M/d/yyyy hh:mm:ss");
        }

        private void comboBoxDateTimeFormats_SelectedIndexChanged(object sender, EventArgs e)
        {
            textBoxExample.Text = FormatDateTime(DateTime.Now);
        }
    }
}
