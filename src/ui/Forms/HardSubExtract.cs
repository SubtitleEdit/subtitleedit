using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.VideoPlayers;
using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

//using Nikse.SubtitleEdit.Logic.DirectShow.Custom;

namespace Nikse.SubtitleEdit.Forms
{
    public partial class HardSubExtract : Form
    {
        // HardExtractCapture cam = null;
        private string _videoFileName;
        private LibVlcDynamic _libVlc;
        private VideoInfo _videoInfo;
        //long startMilliseconds = 0;
        public Subtitle SubtitleFromOcr;
        //private System.Windows.Forms.Timer timer1;
        private const int lineChecksWidth = 50;
        private const int lineChecksHeight = 25;
        public string OcrFileName;

        public HardSubExtract(string videoFileName)
        {
            UiUtil.PreInitialize(this);
            InitializeComponent();
            UiUtil.FixFonts(this);
            _videoFileName = videoFileName;
            labelClickOnTextColor.Visible = false;
            SubtitleFromOcr = new Subtitle();
            labelStatus.Text = string.Empty;
            tbFileName.Text = videoFileName;
            pictureBoxCustomColor.BackColor = Color.FromArgb(233, 233, 233);
            SetCustumRGB();
        }

        private void HardSubExtract_Shown(object sender, EventArgs e)
        {
            if (openFileDialogVideo.ShowDialog(this) == DialogResult.OK)
            {
                _videoFileName = openFileDialogVideo.FileName;
                tbFileName.Text = openFileDialogVideo.FileName;
                _videoInfo = UiUtil.GetVideoInfo(_videoFileName);
                var oldPlayer = Configuration.Settings.General.VideoPlayer;
                Configuration.Settings.General.VideoPlayer = "VLC";
                UiUtil.InitializeVideoPlayerAndContainer(_videoFileName, _videoInfo, mediaPlayer, VideoLoaded, null);
                Configuration.Settings.General.VideoPlayer = oldPlayer;
                _libVlc = mediaPlayer.VideoPlayer as LibVlcDynamic;
            }
            else
            {
                DialogResult = DialogResult.Cancel;
            }
        }

        private void VideoLoaded(object sender, EventArgs e)
        {
            mediaPlayer.Stop();
            mediaPlayer.Volume = Configuration.Settings.General.VideoPlayerDefaultVolume;
            //            pictureBox2.Image = GetSnapShot(startMilliseconds);
            mediaPlayer.Pause();
            timerRefreshProgressbar.Start();
            pictureBox2.Refresh();
        }

        //private Bitmap GetSnapShot(long milliseconds)
        //{
        //    string fileName = Path.Combine(_folderName, Guid.NewGuid().ToString() + ".png");
        //    _libVlc.CurrentPosition = milliseconds / TimeCode.BaseUnit;
        //    _libVlc.TakeSnapshot(fileName, (uint)_videoInfo.Width, (uint)_videoInfo.Height);
        //    int i=0;
        //    while (i < 100 && !File.Exists(fileName))
        //    {
        //        System.Threading.Thread.Sleep(5);
        //        Application.DoEvents();
        //        i++;
        //    }
        //    System.Threading.Thread.Sleep(5);
        //    Application.DoEvents();
        //    Bitmap bmp = null;
        //    try
        //    {
        //        using (var ms = new MemoryStream(File.ReadAllBytes(fileName)))
        //        {
        //            ms.Position = 0;
        //            bmp = ((Bitmap)Bitmap.FromStream(ms)); // avoid locking file
        //        }
        //        File.Delete(fileName);
        //    }
        //    catch
        //    {
        //    }
        //    return bmp;
        //}

        private void pictureBox2_MouseClick(object sender, MouseEventArgs e)
        {
            if (pictureBox2.Image != null)
            {
                Bitmap bmp = pictureBox2.Image as Bitmap;
                if (bmp != null)
                {
                    pictureBoxCustomColor.BackColor = bmp.GetPixel(e.X, e.Y);
                    SetCustumRGB();
                }
            }
        }

        private void pictureBox2_Paint(object sender, PaintEventArgs e)
        {
            Bitmap bmp = pictureBox2.Image as Bitmap;
            if (bmp != null)
            {
                using (Pen p = new Pen(Brushes.Red))
                {
                    int value = Convert.ToInt32(numericUpDownPixelsBottom.Value);
                    if (value > bmp.Height)
                    {
                        value = bmp.Height - 2;
                    }

                    e.Graphics.DrawRectangle(p, 0, bmp.Height - value, bmp.Width - 1, bmp.Height - (bmp.Height - value) - 1);
                }
            }
        }

        private void TimerRefreshProgressbarTick(object sender, EventArgs e)
        {
            mediaPlayer.RefreshProgressBar();
        }

        private void numericUpDownPixelsBottom_ValueChanged(object sender, EventArgs e)
        {
            pictureBox2.Invalidate();
        }

        private void StartStop_Click(object sender, EventArgs e)
        {
            //Cursor.Current = Cursors.WaitCursor;
            //buttonStop.Visible = true;
            //StartStop.Enabled = false;
            //cam = new HardExtractCapture(tbFileName.Text, (int)numericUpDownPixelsBottom.Value, checkBoxCustomColor.Checked, checkBoxYellow.Checked, pictureBoxCustomColor.BackColor, (int)numericUpDownCustomMaxDiff.Value);

            //// Start displaying statistics
            //this.timer1 = new System.Windows.Forms.Timer(this.components);
            //this.timer1.Interval = 1000;
            //this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            //timer1.Enabled = true;
            //cam.Start();
            //cam.WaitUntilDone();
            //timer1.Enabled = false;

            //// Final update
            //tbFrameNum.Text = cam.count.ToString();
            //tbBlacks.Text = cam.blacks.ToString();

            //string fileNameNoExt = Path.GetTempFileName();

            //Cursor.Current = Cursors.Default;

            //var sub = new Subtitle();
            //for (int i = 0; i < cam.Images.Count; i++)
            //{
            //    if (cam.StartTimes.Count > i)
            //    {
            //        Paragraph p = new Paragraph();
            //        p.StartTime.TotalSeconds = cam.StartTimes[i];
            //        if (cam.EndTimes.Count > i)
            //        {
            //            p.EndTime.TotalSeconds = cam.EndTimes[i];
            //        }
            //        else
            //        {
            //            p.EndTime.TotalSeconds = p.StartTime.TotalSeconds + 2.5;
            //        }
            //        p.Text = fileNameNoExt + string.Format("{0:0000}", i) + ".bmp";
            //        sub.Paragraphs.Add(p);
            //        var bmp = cam.Images[i].GetBitmap();
            //        bmp.Save(p.Text);
            //        bmp.Dispose();
            //    }
            //}
            //sub.Renumber();
            //if (sub.Paragraphs.Count > 0)
            //{
            //    OcrFileName = fileNameNoExt + ".srt";
            //    File.WriteAllText(OcrFileName, sub.ToText(new SubRip()));
            //}
            //lock (this)
            //{
            //    cam.Dispose();
            //    cam = null;
            //}
            //buttonStop.Visible = false;
            //StartStop.Enabled = true;
        }

        //private void timer1_Tick(object sender, System.EventArgs e)
        //{
        //if (cam != null)
        //{
        //    tbFrameNum.Text = cam.count.ToString();
        //    tbBlacks.Text = cam.blacks.ToString();
        //    if (cam.Images.Count > 0)
        //    {
        //        var old = pictureBox2.Image as Bitmap;
        //        pictureBox2.Image = cam.Images[cam.Images.Count - 1].GetBitmap();
        //        if (old != null)
        //            old.Dispose();
        //    }
        //}
        //}

        private void pictureBoxCustomColor_Click(object sender, EventArgs e)
        {
            colorDialog1.Color = pictureBoxCustomColor.BackColor;
            if (colorDialog1.ShowDialog(this) == DialogResult.OK)
            {
                pictureBoxCustomColor.BackColor = colorDialog1.Color;
                SetCustumRGB();
            }
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void buttonStop_Click(object sender, EventArgs e)
        {
            //if (cam != null)
            //    cam.Cancel = true;
            //buttonStop.Visible = false;
            //StartStop.Enabled = true;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var fileName = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".bmp");
            try
            {
                _libVlc.TakeSnapshot(fileName, (uint)_videoInfo.Width, (uint)_videoInfo.Height);
                System.Threading.Thread.Sleep(100);
                pictureBox2.Image = Image.FromFile(fileName);
                System.Threading.Thread.Sleep(50);
            }
            catch (FileNotFoundException)
            {
                // the screenshot was not taken
            }
            catch
            {
                // TODO: Avoid catching all exceptions
            }
            finally
            {
                // whatever happens delete the screenshot if it exists
                File.Delete(fileName);
            }
        }

        private void SetCustumRGB()
        {
            labelCustomRgb.Text = string.Format("(Red={0},Green={1},Blue={2})", pictureBoxCustomColor.BackColor.R, pictureBoxCustomColor.BackColor.G, pictureBoxCustomColor.BackColor.B);
        }

        private void saveImageAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            saveFileDialog1.Title = LanguageSettings.Current.VobSubOcr.SaveSubtitleImageAs;
            saveFileDialog1.AddExtension = true;
            saveFileDialog1.FileName = "Image";
            saveFileDialog1.Filter = "PNG image|*.png|BMP image|*.bmp|GIF image|*.gif|TIFF image|*.tiff";
            saveFileDialog1.FilterIndex = 0;

            DialogResult result = saveFileDialog1.ShowDialog(this);
            if (result == DialogResult.OK)
            {
                using (var bmp = pictureBox2.Image as Bitmap)
                {
                    if (bmp == null)
                    {
                        MessageBox.Show("No image!");
                        return;
                    }

                    try
                    {
                        if (saveFileDialog1.FilterIndex == 0)
                        {
                            bmp.Save(saveFileDialog1.FileName, System.Drawing.Imaging.ImageFormat.Png);
                        }
                        else if (saveFileDialog1.FilterIndex == 1)
                        {
                            bmp.Save(saveFileDialog1.FileName);
                        }
                        else if (saveFileDialog1.FilterIndex == 2)
                        {
                            bmp.Save(saveFileDialog1.FileName, System.Drawing.Imaging.ImageFormat.Gif);
                        }
                        else
                        {
                            bmp.Save(saveFileDialog1.FileName, System.Drawing.Imaging.ImageFormat.Tiff);
                        }
                    }
                    catch (Exception exception)
                    {
                        MessageBox.Show(exception.Message);
                    }
                }
            }
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
                if (_libVlc != null)
                {
                    _libVlc.Dispose();
                    _libVlc = null;
                }
            }
            base.Dispose(disposing);
        }

    }
}
