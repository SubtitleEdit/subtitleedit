using Nikse.SubtitleEdit.Logic;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.Forms.Ocr
{
    public partial class DownloadPaddleGpuOrCpu : Form
    {
        public bool GpuSelected { get; set; }
        public bool CpuSelected { get; set; }

        public DownloadPaddleGpuOrCpu()
        {
            UiUtil.PreInitialize(this);
            InitializeComponent();
            UiUtil.FixFonts(this);
            Text = LanguageSettings.Current.GetTesseractDictionaries.Download + " PaddleOCR";
        }

        private void buttonCancel_Click(object sender, System.EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void buttonDownloadGpu_Click(object sender, System.EventArgs e)
        {
            GpuSelected = true;
            CpuSelected = false;
            DialogResult = DialogResult.OK;
        }

        private void buttonDownloadCpu_Click(object sender, System.EventArgs e)
        {
            GpuSelected = false;
            CpuSelected = true;
            DialogResult = DialogResult.OK;
        }

        private void DownloadPaddleGpuOrCpu_KeyDown(object sender, KeyEventArgs e)
        {
            if  (e.KeyCode == Keys.Escape)
            {
                DialogResult = DialogResult.Cancel;
            }
        }
    }
}
