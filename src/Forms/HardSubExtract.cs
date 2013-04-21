using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.VideoPlayers;

namespace Nikse.SubtitleEdit.Forms
{
    public partial class HardSubExtract : Form
    {
        private string _videoFileName;
        private LibVlc11xDynamic _libVlc;
        private VideoInfo _videoInfo;
        private string _folderName;
        long startMilliseconds = 0;
        int colorChooser = -1;
        Subtitle _subtitle;
        public Subtitle SubtitleFromOcr;
        NikseBitmap _lastImage;
        double _lastImagePixelPercent;
        bool _abort = false;

        public HardSubExtract(string videoFileName)
        {
            InitializeComponent();
            _videoFileName = videoFileName;
            labelClickOnTextColor.Visible = false;
            SubtitleFromOcr = new Subtitle();
            labelStatus.Text = string.Empty;
        }

        private void HardSubExtract_Shown(object sender, EventArgs e)
        {
            _folderName = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            System.IO.Directory.CreateDirectory(_folderName);

            if (openFileDialogVideo.ShowDialog(this) == DialogResult.OK)
            {
                _videoFileName = openFileDialogVideo.FileName;
                _videoInfo = Utilities.GetVideoInfo(_videoFileName, null);
                var oldPlayer = Configuration.Settings.General.VideoPlayer;
                Configuration.Settings.General.VideoPlayer = "VLC";
                Utilities.InitializeVideoPlayerAndContainer(_videoFileName, _videoInfo, mediaPlayer, VideoLoaded, null);
                Configuration.Settings.General.VideoPlayer = oldPlayer;
                _libVlc = mediaPlayer.VideoPlayer as LibVlc11xDynamic;

                pictureBoxColor1.BackColor = Color.FromArgb(253, 253, 253);
                pictureBoxColor2.BackColor = Color.FromArgb(250, 250, 250);
                _subtitle = new Subtitle();
            }
            else
            {
                DialogResult = DialogResult.Cancel;
            }
        }

        void VideoLoaded(object sender, EventArgs e)
        {
            mediaPlayer.Stop();
            mediaPlayer.Volume = Configuration.Settings.General.VideoPlayerDefaultVolume;
            pictureBox2.Image = GetSnapShot(startMilliseconds);
            mediaPlayer.Pause();
            timerRefreshProgressbar.Start();
        }

        private Bitmap GetSnapShot(long milliseconds)
        {
            string fileName = Path.Combine(_folderName, Guid.NewGuid().ToString() + ".png");
            _libVlc.CurrentPosition = milliseconds / 1000.0;
            _libVlc.TakeSnapshot(fileName, (uint)_videoInfo.Width, (uint)_videoInfo.Height);
            int i=0;
            while (i < 100 && !File.Exists(fileName))
            {
                System.Threading.Thread.Sleep(5);
                Application.DoEvents();
                i++;
            }
            System.Threading.Thread.Sleep(5);
            Application.DoEvents();
            Bitmap bmp = null;
            try
            {
                using (var ms = new MemoryStream(File.ReadAllBytes(fileName)))
                {
                    bmp = ((Bitmap)Bitmap.FromStream(ms)); // avoid locking file
                }
                File.Delete(fileName);
            }
            catch
            {
            }
            return bmp;
        }        

        private void pictureBoxBackground_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Click on screenshot to get color 1");
            labelClickOnTextColor.Visible = true;
            colorChooser = 1;
        }

        private void pictureBox2_MouseClick(object sender, MouseEventArgs e)
        {
            if (pictureBox2.Image != null)
            {
                Bitmap bmp = pictureBox2.Image as Bitmap;
                if (bmp != null)
                {
                    if (colorChooser == 1)
                    {
                        pictureBoxColor1.BackColor = bmp.GetPixel(e.X, e.Y);
                    }
                    else if (colorChooser == 2)
                    {
                        pictureBoxColor2.BackColor = bmp.GetPixel(e.X, e.Y);
                    }
                    labelClickOnTextColor.Visible = false;
                    colorChooser = -1;
                }
            }
        }

        private void pictureBoxColor2_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Click on screenshot to get color 2");
            labelClickOnTextColor.Visible = true;
            colorChooser = 2;
        }

        private void pictureBox2_Paint(object sender, PaintEventArgs e)
        {            
            Bitmap bmp = pictureBox2.Image as Bitmap;
            if (bmp != null)
            {
                Pen p = new Pen(Brushes.Red);                
                int value = Convert.ToInt32(numericUpDownPixelsBottom.Value);
                if (value > bmp.Height)
                    value = bmp.Height - 2;
                e.Graphics.DrawRectangle(p, 0, bmp.Height - value, bmp.Width - 1, bmp.Height - (bmp.Height - value) - 1);
                p.Dispose();
            }
        }

        static public Bitmap CopyBitmapSection(Bitmap srcBitmap, Rectangle section)
        {
            Bitmap bmp = new Bitmap(section.Width, section.Height);
            Graphics g = Graphics.FromImage(bmp);
            g.DrawImage(srcBitmap, 0, 0, section, GraphicsUnit.Pixel);
            g.Dispose();
            return bmp;
        }

        private void GetText(Bitmap bmp)
        {
            if (numericUpDownPixelsBottom.Value < bmp.Height)
            {
                int h = Convert.ToInt32(numericUpDownPixelsBottom.Value);
                bmp = CopyBitmapSection(bmp, new Rectangle(0, bmp.Height - h, bmp.Width, h));
            }            

            NikseBitmap nbmp = new NikseBitmap(bmp);
            int diff = Convert.ToInt32(numericUpDownFont1Diff.Value);
            int count = nbmp.MakeOneColorRemoverOthers(pictureBoxColor1.BackColor, pictureBoxColor2.BackColor, diff);
        //    int count = nbmp.RemoveAloneColors(); // remove noise
            double percent = count * 100.0 / (nbmp.Width * nbmp.Height);
            if (count > 10 && percent > 0.1)
            {
                nbmp.CropTransparentSidesAndBottom(2, true);
                nbmp.CropSidesAndBottom(2, Color.Transparent, true);

                bool isSameImage = false;
                if (_lastImage != null && Math.Abs(_lastImagePixelPercent - percent) < 0.3)
                {
                        Paragraph p = _subtitle.Paragraphs[_subtitle.Paragraphs.Count - 1];
                        p.EndTime.TotalMilliseconds = startMilliseconds + 100;
                        isSameImage = true;
                }

                if (!isSameImage)
                {
                    string name = Path.Combine(_folderName, _subtitle.Paragraphs.Count.ToString() + ".png");
                    nbmp.GetBitmap().Save(name, System.Drawing.Imaging.ImageFormat.Png);
                    _subtitle.Paragraphs.Add(new Paragraph(name, startMilliseconds, startMilliseconds + 100));
                    labelStatus.Text = string.Format("{0} possible subtitles found", _subtitle.Paragraphs.Count);
                }

                _lastImage = nbmp;
                _lastImagePixelPercent = percent;
            }
            else
            {
                _lastImage = null;
            }
        }

        private void buttonStartClick(object sender, EventArgs e)
        {
            _lastImage = null;
            _abort = false;
            var durationInMilliseconds = _libVlc.Duration * 1000.0;
            startMilliseconds = Convert.ToInt64(_libVlc.CurrentPosition * 1000.0);
            buttonOK.Enabled = false;
            buttonCancel.Enabled = false;
            _subtitle.Paragraphs.Clear();
            while (!_abort && startMilliseconds < durationInMilliseconds)
            {
                Bitmap bmp = GetSnapShot(startMilliseconds);
                GetText(bmp);
                bmp.Dispose();
                startMilliseconds += Convert.ToInt32(numericUpDownInterval.Value);
            }
            if (!_abort)
                GoOcr();
            buttonOK.Enabled = false;
            buttonCancel.Enabled = false;
        }

        private void GoOcr()
        { 
            var formSubOcr = new VobSubOcr();
            formSubOcr.Initialize(_subtitle, Configuration.Settings.VobSubOcr, false);
            if (formSubOcr.ShowDialog(this) == DialogResult.OK)
            {
                SubtitleFromOcr = formSubOcr.SubtitleFromOcr;
            }
        }

        private void buttonStop_Click(object sender, EventArgs e)
        {
            _abort = true;
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void HardSubExtract_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                if (System.IO.Directory.Exists(_folderName))
                    System.IO.Directory.Delete(_folderName);
            }
            catch
            { 
            }
        }

        private void buttonShowImg_Click(object sender, EventArgs e)
        {
            pictureBox2.Image = GetSnapShot(Convert.ToInt64(_libVlc.CurrentPosition * 1000.0));
        }

        private void TimerRefreshProgressbarTick(object sender, EventArgs e)
        {        
            if (mediaPlayer != null)
            {
                mediaPlayer.RefreshProgressBar();
            }        
        }

        private void numericUpDownPixelsBottom_ValueChanged(object sender, EventArgs e)
        {
            pictureBox2.Invalidate();
        }

    }
}
