using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Logic;
using System;
using System.Text;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.Forms
{

    public sealed partial class PacEncoding : Form
    {
        private const string PreviewChars = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
        public int CodePageIndex { get; set; }

        private readonly byte[] _previewBuffer;

        public PacEncoding(byte[] previewBuffer, string fileName)
        {
            CodePageIndex = Configuration.Settings.General.LastPacCodePage;
            UiUtil.PreInitialize(this);
            InitializeComponent();
            UiUtil.FixFonts(this);
            Text = System.IO.Path.GetFileName(fileName);
            _previewBuffer = previewBuffer;
            if (CodePageIndex >= 0 && CodePageIndex < comboBoxCodePage.Items.Count)
            {
                comboBoxCodePage.SelectedIndex = CodePageIndex;
            }

            if (previewBuffer == null)
            {
                labelPreview.Visible = false;
                textBoxPreview.Visible = false;
                Height -= textBoxPreview.Height;
            }
            UiUtil.FixLargeFonts(this, buttonOK);
        }

        private void PacEncoding_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                DialogResult = DialogResult.Cancel;
            }
        }

        private void comboBoxCodePage_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBoxCodePage.SelectedIndex >= 0)
            {
                CodePageIndex = comboBoxCodePage.SelectedIndex;
                if (_previewBuffer != null)
                {
                    var encoding = Pac.GetEncoding(CodePageIndex);
                    const int feIndex = 0;
                    const int endDelimiter = 0x00;
                    int index;

                    var indexOfW16 = Encoding.ASCII.GetString(_previewBuffer).IndexOf("W16", StringComparison.Ordinal);
                    if (indexOfW16 > 0 && indexOfW16 < _previewBuffer.Length - 5)
                    {
                        index = indexOfW16 + 4;
                        if (CodePageIndex == Pac.CodePageChineseSimplified)
                        {
                            textBoxPreview.Text = Encoding.GetEncoding(Pac.EncodingChineseSimplified).GetString(_previewBuffer, index, _previewBuffer.Length - index);
                            return;
                        }

                        if (CodePageIndex == Pac.CodePageChineseTraditional)
                        {
                            textBoxPreview.Text = Encoding.GetEncoding(Pac.EncodingChineseTraditional).GetString(_previewBuffer, index, _previewBuffer.Length - index);
                            return;
                        }

                        if (CodePageIndex == Pac.CodePageKorean)
                        {
                            textBoxPreview.Text = Encoding.GetEncoding(Pac.EncodingKorean).GetString(_previewBuffer, index, _previewBuffer.Length - index);
                            return;
                        }

                        if (CodePageIndex == Pac.CodePageJapanese)
                        {
                            textBoxPreview.Text = Encoding.GetEncoding(Pac.EncodingJapanese).GetString(_previewBuffer, index, _previewBuffer.Length - index);
                            return;
                        }
                    }

                    var sb = new StringBuilder();
                    index = feIndex + 3;
                    while (index < _previewBuffer.Length && _previewBuffer[index] != endDelimiter)
                    {
                        if (_previewBuffer[index] == 0xFE)
                        {
                            sb.AppendLine();
                            index += 2;
                        }
                        else if (_previewBuffer[index] == 0xFF)
                        {
                            sb.Append(' ');
                        }
                        else if (CodePageIndex == Pac.CodePageLatin)
                        {
                            sb.Append(Pac.GetLatinString(encoding, _previewBuffer, ref index));
                        }
                        else if (CodePageIndex == Pac.CodePageArabic)
                        {
                            sb.Append(Pac.GetArabicString(_previewBuffer, ref index));
                        }
                        else if (CodePageIndex == Pac.CodePageHebrew)
                        {
                            sb.Append(Pac.GetHebrewString(_previewBuffer, ref index));
                        }
                        else if (CodePageIndex == Pac.CodePageCyrillic)
                        {
                            sb.Append(Pac.GetCyrillicString(_previewBuffer, ref index));
                        }
                        else if (CodePageIndex == Pac.CodePageGreek)
                        {
                            sb.Append(Pac.GetGreekString(_previewBuffer, ref index));
                        }
                        else
                        {
                            sb.Append(encoding.GetString(_previewBuffer, index, 1));
                        }

                        index++;
                    }

                    if (CodePageIndex == Pac.CodePageArabic)
                    {
                        textBoxPreview.Text = Utilities.FixEnglishTextInRightToLeftLanguage(sb.ToString(), PreviewChars);
                    }
                    else
                    {
                        textBoxPreview.Text = sb.ToString();
                    }
                }
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
    }
}
