using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Logic;
using System.IO;
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

            Text = LanguageSettings.Current.UnknownSubtitle.Title;
            labelTitle.Text = LanguageSettings.Current.UnknownSubtitle.Title;
            richTextBoxMessage.Text = LanguageSettings.Current.UnknownSubtitle.Message;
            buttonOK.Text = LanguageSettings.Current.General.Ok;
            buttonImportPlainText.Text = LanguageSettings.Current.UnknownSubtitle.ImportAsPlainText;
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
                LabelPreview.Text = LanguageSettings.Current.General.Preview + " - " + fileName;
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

        private void UnknownSubtitle_Shown(object sender, System.EventArgs e)
        {
            buttonOK.Focus();
        }
    }
}
