using System.IO;
using Nikse.SubtitleEdit.Core;
using Nikse.SubtitleEdit.Logic;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.Forms
{
    public sealed partial class UnknownSubtitle : Form
    {
        public bool ImportPlainText { get; set; }

        public UnknownSubtitle()
        {
            UiUtil.PreInitialize(this);
            InitializeComponent();
            UiUtil.FixFonts(this);

            Text = Configuration.Settings.Language.UnknownSubtitle.Title;
            labelTitle.Text = Configuration.Settings.Language.UnknownSubtitle.Title;
            richTextBoxMessage.Text = Configuration.Settings.Language.UnknownSubtitle.Message;
            buttonOK.Text = Configuration.Settings.Language.General.Ok;
            buttonImportPlainText.Text = Configuration.Settings.Language.UnknownSubtitle.ImportAsPlainText;
            UiUtil.FixLargeFonts(this, buttonOK);
        }

        public void Initialize(string title, string fileName, bool allowImportPlainText)
        {
            Text = title;
            try
            {
                var file = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

                int length = (int)file.Length;
                if (length > 100000)
                {
                    length = 100000;
                }

                file.Position = 0;
                var fileBuffer = new byte[length];
                file.Read(fileBuffer, 0, length);
                file.Close();

                var encoding = LanguageAutoDetect.DetectAnsiEncoding(fileBuffer);
                LabelPreview.Text = Configuration.Settings.Language.General.Preview + " - " + fileName;
                textBoxPreview.Text = encoding.GetString(fileBuffer).Replace("\0", " ");
                buttonImportPlainText.Visible = allowImportPlainText && !string.IsNullOrWhiteSpace(textBoxPreview.Text);
            }
            catch
            {
                // ignored
            }
        }

        private void FormUnknownSubtitle_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                DialogResult = DialogResult.Cancel;
            }
        }

        private void buttonImportPlainText_Click(object sender, System.EventArgs e)
        {
            ImportPlainText = true;
            DialogResult = DialogResult.OK;
        }
    }
}
