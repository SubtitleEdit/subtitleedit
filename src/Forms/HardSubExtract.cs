using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.VideoPlayers;
using System.Runtime.InteropServices;
using System.Collections.Generic;

namespace Nikse.SubtitleEdit.Forms
{
    public partial class HardSubExtract : Form
    {

        public class MyFrameBuffer
        {
            int size; // = 640 * 480 * 3 * 10; // 921600
            public MyFrameBuffer(int bufferSize)
            {
                size = bufferSize;
            }

            IntPtr myMemory = IntPtr.Zero;

            public IntPtr AllocateUnmanaged()
            {
                if (myMemory == IntPtr.Zero)
                    myMemory = Marshal.AllocHGlobal(size);
                return myMemory;
            }

            public void FreeUnmanaged()
            {
                if (myMemory != IntPtr.Zero)
                    System.Runtime.InteropServices.Marshal.FreeHGlobal(myMemory);
            }

            public byte[] GetSafeCopy()
            {
                byte[] safeCopy = new byte[size];
                Marshal.Copy(myMemory, safeCopy, 0, size);
                return safeCopy;
            }

        }

        public class CallbackContext
        {
            public MyFrameBuffer framebuf;
            public System.Threading.Mutex mutex;

            public CallbackContext(int size)
            { 
                framebuf = new MyFrameBuffer(size);
                mutex = new System.Threading.Mutex();
            }

        }



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
        int frameCount = 1;
        byte[] buf;
        private static bool _done = false;


        List<long> sceneChangeFrames = new List<long>();

        // Callback instances
        private Nikse.SubtitleEdit.Logic.VideoPlayers.LibVlc11xDynamic.LockCallbackDelegate m_cb_lock;// = new Core.Interops.Signatures.LibVlc.MediaPlayer.Video.LockCallbackDelegate(CB_Lock);
        private Nikse.SubtitleEdit.Logic.VideoPlayers.LibVlc11xDynamic.UnlockCallbackDelegate m_cb_unlock;// = new Core.Interops.Signatures.LibVlc.MediaPlayer.Video.UnlockCallbackDelegate(CB_Unlock);
        private Nikse.SubtitleEdit.Logic.VideoPlayers.LibVlc11xDynamic.DisplayCallbackDelegate m_cb_display;// = new Core.Interops.Signatures.LibVlc.MediaPlayer.Video.DisplayCallbackDelegate(CB_Display);

        // Context ("opaque")
        private CallbackContext m_cb_ctx;

        // frame counter
        private long frameCounter = 0;


        int currentBestDiff = 0;
        int maxDiffARGB = 25;
        private const int lineChecksWidth = 50;
        private const int lineChecksHeight = 25;
        int maxDiffPixels = lineChecksWidth * lineChecksHeight / 4;
        double factorWidth;
        double factorHeight;



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
            if (bmp == null)
                return;

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
                    Bitmap newBitmap = nbmp.GetBitmap();
                    newBitmap.Save(name, System.Drawing.Imaging.ImageFormat.Png);
                    newBitmap.Dispose();
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
                if (bmp != null)
                {
                    GetText(bmp);
                    bmp.Dispose();
                }
                startMilliseconds += Convert.ToInt32(numericUpDownInterval.Value);
            }
            //if (!_abort)
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

//static void* lock(void* userData, void** p_pixels) {
//    char* buffer = (char* )userData;
//    *p_pixels = buffer;
//    cout << "lock no: " << frameCount << endl;
//    return NULL;
//}

//static void unlock(void* userData, void* picture, void *const * p_pixels) {
//    cout << "unlock no: " << frameCount++ << endl;
//}

//    libvlc_instance_t* vlcInstance=libvlc_new(0, NULL);
//    libvlc_media_t* media = libvlc_media_new_path(vlcInstance, fname.c_str());
//    libvlc_media_player_t* mediaPlayer = libvlc_media_player_new_from_media(media);
//    libvlc_media_release(media);

//    int wd = 2096, ht = 1132;
//    buf = new char[wd*ht*4];
//    libvlc_video_set_callbacks(mediaPlayer, lock, unlock, NULL, buf);
//    libvlc_video_set_format(mediaPlayer, "RV32", wd, ht, 4*wd);

//    libvlc_media_player_play (mediaPlayer);

        void OnpLock(IntPtr opaque, ref IntPtr plane)
        {
            //PixelData* px = (PixelData*)opaque;
            //*plane = px->pPixelData;
            //return null;
        }

        void OnpUnlock(IntPtr opaque, IntPtr picture, ref IntPtr plane)
        {
            frameCount++;
        }

        unsafe void OnpDisplay(void* opaque, void* picture)
        {
            //lock (m_lock)
            //{
            //    try
            //    {
            //        PixelData* px = (PixelData*)opaque;
            //        MemoryHeap.CopyMemory(m_pBuffer, px->pPixelData, px->size);

            //        m_frameRate++;
            //        if (m_callback != null)
            //        {
            //            using (Bitmap frame = GetBitmap())
            //            {
            //                m_callback(frame);
            //            }
            //        }
            //    }
            //    catch (Exception ex)
            //    {
            //        if (m_excHandler != null)
            //        {
            //            m_excHandler(ex);
            //        }
            //        else
            //        {
            //            throw ex;
            //        }
            //    }
            //}
        }

        public void VlcLock(IntPtr opaque, ref IntPtr planes)
        {
            m_cb_ctx.mutex.WaitOne();
        //    log("CB_Lock");
            planes = m_cb_ctx.framebuf.AllocateUnmanaged();
        }

        public void VlcUnlock(IntPtr opaque, IntPtr picture, ref IntPtr planes)
        {
            //    log("CB_Unlock");
            m_cb_ctx.mutex.ReleaseMutex();
        //    labelStatus.Text = frameCounter.ToString();
            _done = true;
        }

        public void VlcDisplay(IntPtr opaque, IntPtr picture)
        {
            _done = true;
            return;
            //log("CB_Display");
           // frameCounter++;
            int w = _videoInfo.Width;
            int h = _videoInfo.Height;

            byte[] frame = m_cb_ctx.framebuf.GetSafeCopy(); // get copy of the frame data
            Bitmap bmp = new Bitmap(w, h);
            // convert to bmp
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    int pos = (y * w + x) * 3;
                    Color color = Color.FromArgb(frame[pos], frame[pos + 1], frame[pos + 2]);
                    bmp.SetPixel(x, y, color);
                }
            }
            bmp.Save(@"D:\Download\callbacks\frame" + frameCounter.ToString() + ".bmp");
        }

        public IntPtr GetIntPtrOfObject(object o)
        {
            // extract the IntPtr "pointing" the context object
            // http://msdn.microsoft.com/en-us/library/system.runtime.interopservices.gchandle.tointptr.aspx

            GCHandle handle = GCHandle.Alloc(o);
            IntPtr opaque = GCHandle.ToIntPtr(handle);
            //handle.Free();    // TODO: ok?
            return opaque;
        }


        private void button1_Click(object sender, EventArgs e)
        {
            m_cb_lock = new Nikse.SubtitleEdit.Logic.VideoPlayers.LibVlc11xDynamic.LockCallbackDelegate(VlcLock);
            m_cb_unlock = new Nikse.SubtitleEdit.Logic.VideoPlayers.LibVlc11xDynamic.UnlockCallbackDelegate(VlcUnlock);
            m_cb_display = new Nikse.SubtitleEdit.Logic.VideoPlayers.LibVlc11xDynamic.DisplayCallbackDelegate(VlcDisplay);

            uint w = (uint)_videoInfo.Width;
            uint h = (uint)_videoInfo.Height;

            m_cb_ctx = new CallbackContext(_videoInfo.Width * _videoInfo.Height * 3 * 10);
            IntPtr opaque = GetIntPtrOfObject(m_cb_ctx);
            string videoFile = _videoFileName; // @"D:\Download\a.mkv";
            frameCounter = 0;
            //            _libVlc = new LibVlc11xDynamic();
            //            _libVlc.InitializeAndStartFrameGrabbing(videoFile, w, h, m_cb_lock, m_cb_unlock, m_cb_display, opaque);
            _libVlc.Play();
            _libVlc.Pause();
            _libVlc.CurrentPosition = 0;
            Application.DoEvents();
            long ticks = DateTime.Now.Ticks;
            Bitmap prev = null;
            Bitmap current = null;
            int numberOfFrames = (int)_videoInfo.TotalFrames;
            for (int i = 0; i < numberOfFrames; i++)
            {
                _done = false;
                _libVlc.GetNextFrame();
                
                //int vlcState = _libVlc.VlcState;
                //while (vlcState != 0 && vlcState != 4)
                //{
                //    Application.DoEvents();
                //    vlcState = _libVlc.VlcState;
                //}
                //while (!_done)
                //{
                //    Application.DoEvents();
                //}
                frameCounter++;
                string fileName = @"D:\Download\callbacks\frame" + frameCounter.ToString() + ".png";

                _libVlc.TakeSnapshot(fileName, (uint)_videoInfo.Width, (uint)_videoInfo.Height);
                Application.DoEvents();
                if (File.Exists(fileName))
                {
                    current = new Bitmap(fileName);
                    if (prev != null)
                    {
                        CheckForSceneChanges(prev, current, frameCounter);
                        prev.Dispose();
                    }
                    prev = current;
                }
            }
            MessageBox.Show((DateTime.Now.Ticks - ticks).ToString());
        }

        private void CheckForSceneChanges(Bitmap prev, Bitmap current, long frameNumber)
        {
            int diff = 0;
            factorWidth = prev.Width / lineChecksWidth;
            factorHeight = prev.Height / lineChecksHeight;            
            for (int yCounter = 0; yCounter < lineChecksHeight; yCounter++)
            {
                int y = (int)Math.Round(yCounter * factorHeight);                
                for (int xCounter = 0; xCounter < lineChecksWidth; xCounter++)
                {
                    int x = (int)Math.Round(xCounter * factorWidth);
                    Color prevColor = prev.GetPixel(x, y);
                    Color curColor = current.GetPixel(x, y);
                    if (Math.Abs(prevColor.A - curColor.A) > maxDiffARGB ||
                        Math.Abs(prevColor.R - curColor.R) > maxDiffARGB ||
                        Math.Abs(prevColor.G - curColor.G) > maxDiffARGB ||
                        Math.Abs(prevColor.B - curColor.B) > maxDiffARGB)
                    {
                        diff++;
                        //if (diff > maxDiffPixels)
                        //{
                        //    sceneChangeFrames.Add(frameNumber);
                        //    labelStatus.Text = frameNumber.ToString() + " " + _libVlc.CurrentPosition.ToString() + "   " + diff.ToString();
                        //    labelStatus.Refresh();
                        //    Application.DoEvents();
                        //    return;
                        //}
                    }                        
                }
            }
            if (diff > maxDiffPixels)
            {
              sceneChangeFrames.Add(frameNumber);
            }
            if (diff > currentBestDiff)
                currentBestDiff = diff;
            labelStatus.Text = frameNumber.ToString() + " " + _libVlc.CurrentPosition.ToString() + "   " + diff.ToString() + "  all time best diff:" + currentBestDiff;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            MessageBox.Show(frameCounter.ToString());
            _libVlc.Stop();
        }

    }
}
