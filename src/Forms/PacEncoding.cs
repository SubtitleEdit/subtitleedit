using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.SubtitleFormats;

namespace Nikse.SubtitleEdit.Forms
{

    public partial class PacEncoding : Form
    {
        public int CodePageIndex { get; set; }

        private byte[] _previewBuffer;

        public PacEncoding(byte[] previewBuffer, string fileName)
        {
            CodePageIndex = Configuration.Settings.General.LastPacCodePage;
            InitializeComponent();
            Text = System.IO.Path.GetFileName(fileName);
            _previewBuffer = previewBuffer;
            if (CodePageIndex >= 0 && CodePageIndex < comboBoxCodePage.Items.Count)
                comboBoxCodePage.SelectedIndex = CodePageIndex;

            if (previewBuffer == null)
            {
                labelPreview.Visible = false;
                textBoxPreview.Visible = false;
                Height -= textBoxPreview.Height;
            }

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

        private void PacEncoding_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
                DialogResult = DialogResult.Cancel;
        }

        private void comboBoxCodePage_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            if (comboBoxCodePage.SelectedIndex >= 0)
            {
                CodePageIndex = comboBoxCodePage.SelectedIndex;
                if (_previewBuffer != null)
                {
                    Encoding encoding = Pac.GetEncoding(CodePageIndex);
                    int FEIndex = 0;
                    int endDelimiter = 0x00;
                    StringBuilder sb = new StringBuilder();
                    int index = FEIndex + 3;
                    while (index < _previewBuffer.Length && _previewBuffer[index] != endDelimiter)
                    {
                        if (_previewBuffer[index] == 0xFE)
                        {
                            sb.AppendLine();
                            index += 2;
                        }
                        else if (_previewBuffer[index] == 0xFF)
                            sb.Append(" ");
                        else if (CodePageIndex == 0)
                            sb.Append(Pac.GetLatinString(encoding, _previewBuffer, ref index));
                        else if (CodePageIndex == 3)
                            sb.Append(Pac.GetArabicString(_previewBuffer, ref index));
                        else if (CodePageIndex == 4)
                            sb.Append(Pac.GetHebrewString(_previewBuffer, ref index));
                        else
                            sb.Append(encoding.GetString(_previewBuffer, index, 1));

                        index++;
                    }
                    if (CodePageIndex == 3)
                        textBoxPreview.Text = Utilities.FixEnglishTextInRightToLeftLanguage(sb.ToString(), "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ");
                    else
                        textBoxPreview.Text = sb.ToString();
                }
            }

        }

        private void buttonOK_Click(object sender, System.EventArgs e)
        {
            DialogResult = DialogResult.OK;
        }

        private void buttonCancel_Click(object sender, System.EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

    }
}
