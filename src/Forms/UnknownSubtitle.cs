using System.Windows.Forms;
using Nikse.SubtitleEdit.Logic;
using System.Drawing;

namespace Nikse.SubtitleEdit.Forms
{
    public sealed partial class UnknownSubtitle : Form
    {
        public UnknownSubtitle()
        {
            InitializeComponent();

            Text = Configuration.Settings.Language.UnknownSubtitle.Title;
            labelTitle.Text = Configuration.Settings.Language.UnknownSubtitle.Title;
            richTextBoxMessage.Text = Configuration.Settings.Language.UnknownSubtitle.Message;
            buttonOK.Text = Configuration.Settings.Language.General.OK;
            FixLargeFonts();
        }

        private void FixLargeFonts()
        {
            Graphics graphics = this.CreateGraphics();
            SizeF textSize = graphics.MeasureString(buttonOK.Text, this.Font);
            if (textSize.Height > buttonOK.Height - 4)
            {
                int newButtonHeight = (int)(textSize.Height + 7 + 0.5);
                Utilities.SetButtonHeight(this, newButtonHeight, 1);
            }
        }

        public void Initialize(string title)
        {
            Text = title;
        }

        private void FormUnknownSubtitle_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
                DialogResult = DialogResult.Cancel;
        }
    }
}
