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
        int startMilliseconds = 6000;

        public HardSubExtract(string videoFileName)
        {
            InitializeComponent();
            _videoFileName = videoFileName;
            _libVlc = new LibVlc11xDynamic();
        }

        private void HardSubExtract_Shown(object sender, EventArgs e)
        {
            _videoInfo = Utilities.GetVideoInfo(_videoFileName, null);
            _libVlc.Initialize(panelVlc, _videoFileName, null, null);
        }

        private Bitmap GetSnapShot(long milliseconds)
        {
            string fileName = Path.GetTempFileName() + ".png";
            _libVlc.CurrentPosition = milliseconds / 1000.0;
            _libVlc.TakeSnapshot(fileName, (uint)_videoInfo.Width, (uint)_videoInfo.Height);
            int i=0;
            while (i < 100 && !File.Exists(fileName))
            {
                System.Threading.Thread.Sleep(50);
                Application.DoEvents();
                i++;
            }
            System.Threading.Thread.Sleep(10);
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

        private void button2_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < 1; i++)
            {
                Bitmap oldBitmap = pictureBox2.Image as Bitmap;
                pictureBox2.Image = GetSnapShot(startMilliseconds);
                if (oldBitmap != null)
                    oldBitmap.Dispose();
                startMilliseconds += 100;
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            NikseBitmap nbmp = new NikseBitmap(pictureBox2.Image as Bitmap);
            nbmp.MakeOneColorRemoverOthers(Color.Black, 100);
            pictureBox2.Image = nbmp.GetBitmap();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            NikseBitmap nbmp = new NikseBitmap(pictureBox2.Image as Bitmap);
            nbmp.MakeOneColorRemoverOthers(Color.White, 50);
            pictureBox2.Image = nbmp.GetBitmap();
        }
      
    }
}
