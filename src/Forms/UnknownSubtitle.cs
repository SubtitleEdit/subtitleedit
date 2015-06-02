using System.Windows.Forms;
using Nikse.SubtitleEdit.Logic;

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
            buttonOK.Text = Configuration.Settings.Language.General.Ok;
            Utilities.FixLargeFonts(this, buttonOK);
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
