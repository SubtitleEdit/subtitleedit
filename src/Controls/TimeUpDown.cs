using System;
using System.Windows.Forms;
using Nikse.SubtitleEdit.Logic;

namespace Nikse.SubtitleEdit.Controls
{
    public partial class TimeUpDown : UserControl
    {

        public enum TimeMode
        {
            HHMMSSMS,
            HHMMSSFF
        }

        const int NumericUpDownValue = 50;

        public EventHandler TimeCodeChanged;

        public TimeMode Mode
        {
            get 
            { 
                if (Configuration.Settings == null)
                    return TimeMode.HHMMSSMS;
                if (Configuration.Settings.General.UseTimeFormatHHMMSSFF)
                    return TimeMode.HHMMSSFF;
                return TimeMode.HHMMSSMS;                
            }
        }

        public TimeUpDown()
        {
            InitializeComponent();
            numericUpDown1.ValueChanged += NumericUpDownValueChanged;
            numericUpDown1.Value = NumericUpDownValue;
            maskedTextBox1.InsertKeyMode = InsertKeyMode.Overwrite;
        }

        void NumericUpDownValueChanged(object sender, EventArgs e)
        {            
            double? millisecs = GetTotalMilliseconds();
            if (millisecs.HasValue)
            {

                if (Mode == TimeMode.HHMMSSMS)
                {
                    if (numericUpDown1.Value > NumericUpDownValue)
                    {
                        SetTotalMilliseconds(millisecs.Value + 100);
                    }
                    else if (numericUpDown1.Value < NumericUpDownValue)
                    {
                        if (millisecs.Value - 100 > 0)
                            SetTotalMilliseconds(millisecs.Value - 100);
                        else if (millisecs.Value > 0)
                            SetTotalMilliseconds(0);
                    }
                }
                else
                {
                    if (numericUpDown1.Value > NumericUpDownValue)
                    {
                        SetTotalMilliseconds(millisecs.Value + Logic.SubtitleFormats.SubtitleFormat.FramesToMilliseconds(1));
                    }
                    else if (numericUpDown1.Value < NumericUpDownValue)
                    {
                        if (millisecs.Value - 100 > 0)
                            SetTotalMilliseconds(millisecs.Value - Logic.SubtitleFormats.SubtitleFormat.FramesToMilliseconds(1));
                        else if (millisecs.Value > 0)
                            SetTotalMilliseconds(0);
                    }                    
                }

                if (TimeCodeChanged != null)
                    TimeCodeChanged.Invoke(this, e);
            }
            numericUpDown1.Value = NumericUpDownValue;
        }

        public MaskedTextBox MaskedTextBox
        {
            get
            {
                return maskedTextBox1;
            }
        }

        public void SetTotalMilliseconds(double milliseconds)
        {
            TimeSpan ts = TimeSpan.FromMilliseconds(milliseconds);
            if (Mode == TimeMode.HHMMSSMS)
                maskedTextBox1.Text = new TimeCode(ts).ToString();
            else
                maskedTextBox1.Text = new TimeCode(ts).ToString().Substring(0,9) + string.Format("{0:00}", Logic.SubtitleFormats.SubtitleFormat.MillisecondsToFrames(ts.Milliseconds));
        }

        public double? GetTotalMilliseconds()
        {
            TimeCode tc = TimeCode;
            if (tc != null)
                return tc.TotalMilliseconds;
            return null;
        }

        public TimeCode TimeCode
        {
            get
            {
                string startTime = maskedTextBox1.Text;
                startTime = startTime.Replace(' ', '0');

                if (Mode == TimeMode.HHMMSSMS)
                {
                    if (startTime.EndsWith(System.Globalization.CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator))
                        startTime += "000";

                    string[] times = startTime.Split(":,.".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

                    if (times.Length == 4)
                    {
                        int hours = 0;
                        if (Utilities.IsInteger(times[0]))
                            hours = int.Parse(times[0]);

                        int minutes = 0;
                        if (Utilities.IsInteger(times[1]))
                            minutes = int.Parse(times[1]);

                        int seconds = 0;
                        if (Utilities.IsInteger(times[2]))
                            seconds = int.Parse(times[2]);

                        int milliSeconds = 0;
                        if (Utilities.IsInteger(times[3]))
                            milliSeconds = int.Parse(times[3]);

                        return new TimeCode(hours, minutes, seconds, milliSeconds);
                    }
                }
                else
                {
                    if (startTime.EndsWith(System.Globalization.CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator) || startTime.EndsWith(":"))
                        startTime += "00";

                    string[] times = startTime.Split(":,.".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

                    if (times.Length == 4)
                    {
                        int hours = 0;
                        if (Utilities.IsInteger(times[0]))
                            hours = int.Parse(times[0]);

                        int minutes = 0;
                        if (Utilities.IsInteger(times[1]))
                            minutes = int.Parse(times[1]);

                        int seconds = 0;
                        if (Utilities.IsInteger(times[2]))
                            seconds = int.Parse(times[2]);

                        int milliSeconds = 0;
                        if (Utilities.IsInteger(times[3]))
                            milliSeconds = Logic.SubtitleFormats.SubtitleFormat.FramesToMilliseconds(int.Parse(times[3]));

                        return new TimeCode(hours, minutes, seconds, milliSeconds);
                    }
                }
                return null;
            }
            set
            {
                if (Mode == TimeMode.HHMMSSMS)
                    maskedTextBox1.Mask = "00:00:00.000";
                else
                    maskedTextBox1.Mask = "00:00:00:00";
                if (value != null)
                {
                    if (Mode == TimeMode.HHMMSSMS)
                        maskedTextBox1.Text = value.ToString();
                    else
                        maskedTextBox1.Text = value.ToHHMMSSFF();                          
                }
                else
                {
                    maskedTextBox1.Text = new TimeCode(TimeSpan.FromMilliseconds(0)).ToString();
                }
            }
        }

        private void MaskedTextBox1KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Up)
            {
                numericUpDown1.UpButton();
                e.SuppressKeyPress = true;
            }
            else if (e.KeyCode == Keys.Down)
            {
                numericUpDown1.DownButton();
                e.SuppressKeyPress = true;
            }
        }

    }
}
