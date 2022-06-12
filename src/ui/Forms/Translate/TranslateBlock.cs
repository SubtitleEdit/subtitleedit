using Nikse.SubtitleEdit.Core.Translate;
using Nikse.SubtitleEdit.Logic;
using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Nikse.SubtitleEdit.Core.Common;

namespace Nikse.SubtitleEdit.Forms.Translate
{
    public sealed partial class TranslateBlock : Form
    {
        private readonly CopyPasteBlock _sourceBlock;
        public string TargetText { get; set; }

        public TranslateBlock(CopyPasteBlock source, string title, bool autoCopy)
        {
            UiUtil.PreInitialize(this);
            InitializeComponent();
            UiUtil.FixFonts(this);

            labelInfo.Text = LanguageSettings.Current.GoogleTranslate.TranslateBlockInfo;
            buttonGetTargetGet.Text = LanguageSettings.Current.GoogleTranslate.TranslateBlockGetFromClipboard;
            buttonCopySourceTextToClipboard.Text = LanguageSettings.Current.GoogleTranslate.TranslateBlockCopySourceText;
            buttonExport.Text = LanguageSettings.Current.Main.Menu.File.SaveAs;

            _sourceBlock = source;
            Text = title;
            if (autoCopy)
            {
                buttonCopySourceTextToClipboard_Click(null, null);
                buttonCopySourceTextToClipboard.Font = new Font(Font.FontFamily.Name, buttonCopySourceTextToClipboard.Font.Size, FontStyle.Regular);
            }
            else
            {
                buttonCopySourceTextToClipboard.Font = new Font(Font.FontFamily.Name, buttonCopySourceTextToClipboard.Font.Size, FontStyle.Bold);
            }
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void buttonGetTargetGet_Click(object sender, EventArgs e)
        {
            if (Clipboard.ContainsText(TextDataFormat.Text))
            {
                var x = Clipboard.GetData(DataFormats.UnicodeText);
                var text = x.ToString();
                if (text.Trim() == _sourceBlock.TargetText.Trim())
                {
                    MessageBox.Show(LanguageSettings.Current.GoogleTranslate.TranslateBlockClipboardError1 + Environment.NewLine +
                        Environment.NewLine +
                        LanguageSettings.Current.GoogleTranslate.TranslateBlockClipboardError2);
                    return;
                }
                TargetText = text;
            }
            DialogResult = DialogResult.OK;
        }

        private void buttonCopySourceTextToClipboard_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(_sourceBlock.TargetText);
            buttonCopySourceTextToClipboard.Font = new Font(Font.FontFamily.Name, buttonCopySourceTextToClipboard.Font.Size, FontStyle.Regular);
        }

        private void TranslateBlock_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                DialogResult = DialogResult.Cancel;
            }
            else if (e.Control && e.KeyCode == Keys.V)
            {
                buttonGetTargetGet_Click(sender, e);
            }
        }

        private void buttonExport_Click(object sender, EventArgs e)
        {
            using (var saveFileDialog = new SaveFileDialog
            {
                Title = LanguageSettings.Current.General.OpenSubtitle,
                FileName = "translate.txt",
                Filter = "Text|*.txt",
            })
            {
                if (saveFileDialog.ShowDialog(this) == DialogResult.OK)
                {
                    File.WriteAllText(saveFileDialog.FileName, _sourceBlock.TargetText);
                    UiUtil.OpenFolderFromFileName(saveFileDialog.FileName);
                }
            }
        }

        private void buttonImport_Click(object sender, EventArgs e)
        {
            using (var openFileDialog1 = new OpenFileDialog())
            {
                openFileDialog1.Title = LanguageSettings.Current.General.OpenSubtitle;
                openFileDialog1.FileName = string.Empty;
                openFileDialog1.Filter = "Text files|*.txt";
                openFileDialog1.FileName = string.Empty;
                var result = openFileDialog1.ShowDialog(this);
                if (result != DialogResult.OK)
                {
                    return;
                }

                var encoding = LanguageAutoDetect.GetEncodingFromFile(openFileDialog1.FileName);
                var text = FileUtil.ReadAllTextShared(openFileDialog1.FileName, encoding).Trim();
                if (text == _sourceBlock.TargetText.Trim())
                {
                    MessageBox.Show(LanguageSettings.Current.GoogleTranslate.TranslateBlockClipboardError1 + Environment.NewLine +
                                    Environment.NewLine +
                                    LanguageSettings.Current.GoogleTranslate.TranslateBlockClipboardError2);
                    return;
                }
                TargetText = text;

                DialogResult = DialogResult.OK;
            }
        }
    }
}
