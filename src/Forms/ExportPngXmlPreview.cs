using System.Drawing;
using System.Windows.Forms;
using Nikse.SubtitleEdit.Logic;

namespace Nikse.SubtitleEdit.Forms
{
    public sealed partial class ExportPngXmlPreview : Form
    {
        public ExportPngXmlPreview(Bitmap bmp)
        {
            InitializeComponent();
            pictureBox1.Image = bmp;
            pictureBox1.Width = bmp.Width;
            pictureBox1.Height = bmp.Height;
            MaximumSize = new Size(bmp.Width + (Width - ClientSize.Width), bmp.Height + (Height - ClientSize.Height));
            if (Screen.PrimaryScreen.Bounds.Width > bmp.Width &&
                Screen.PrimaryScreen.Bounds.Height > bmp.Height)
            {
                ClientSize = new Size(bmp.Width, bmp.Height);
            }
            else
            {
                WindowState = FormWindowState.Maximized;
            }

            pictureBox2.Width = 1;
            pictureBox2.Height = 1;
            pictureBox2.Top = bmp.Height - 2;

            Text = string.Format("{0} {1}x{2}", Configuration.Settings.Language.General.Preview, bmp.Width, bmp.Height); 
        }

        private void ExportPngXmlPreview_Shown(object sender, System.EventArgs e)
        {
            panel1.ScrollControlIntoView(pictureBox2);
            pictureBox2.Visible = false;
        }

        private void ExportPngXmlPreview_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                DialogResult = DialogResult.OK;
            }
        }
        
    }
}
