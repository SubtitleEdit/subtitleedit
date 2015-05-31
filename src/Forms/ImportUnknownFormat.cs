using System;
using System.IO;
using System.Text;
using System.Windows.Forms;
using Nikse.SubtitleEdit.Logic;

namespace Nikse.SubtitleEdit.Forms
{
    public partial class ImportUnknownFormat : Form
    {

        public Subtitle ImportedSubitle;
        private readonly Timer _refreshTimer = new Timer();

        public ImportUnknownFormat(string fileName)
        {
            InitializeComponent();
            _refreshTimer.Interval = 400;
            _refreshTimer.Tick += RefreshTimerTick;

            buttonOK.Text = Configuration.Settings.Language.General.Ok;
            buttonCancel.Text = Configuration.Settings.Language.General.Cancel;
            SubtitleListview1.InitializeLanguage(Configuration.Settings.Language.General, Configuration.Settings);
            Utilities.InitializeSubtitleFont(SubtitleListview1);
            SubtitleListview1.AutoSizeAllColumns(this);

            if (!string.IsNullOrEmpty(fileName))
            {
                LoadTextFile(fileName);
                GeneratePreview();
            }
        }

        private void GeneratePreview()
        {
            if (_refreshTimer.Enabled)
            {
                _refreshTimer.Stop();
                _refreshTimer.Start();
            }
            else
            {
                _refreshTimer.Start();
            }
        }

        private void GeneratePreviewReal()
        {
            var uknownFormatImporter = new UknownFormatImporter();
            uknownFormatImporter.UseFrames = radioButtonTimeCodeFrames.Checked;
            ImportedSubitle = uknownFormatImporter.AutoGuessImport(textBoxText.Lines);
            groupBoxImportResult.Text = string.Format(Configuration.Settings.Language.ImportText.PreviewLinesModifiedX, ImportedSubitle.Paragraphs.Count);
            SubtitleListview1.Fill(ImportedSubitle);
            if (ImportedSubitle.Paragraphs.Count > 0)
                SubtitleListview1.SelectIndexAndEnsureVisible(0);
        }

        private void RefreshTimerTick(object sender, EventArgs e)
        {
            _refreshTimer.Stop();
            GeneratePreviewReal();
        }

        private void LoadTextFile(string fileName)
        {
            try
            {
                SubtitleListview1.Items.Clear();
                Encoding encoding = Utilities.GetEncodingFromFile(fileName);
                textBoxText.Text = File.ReadAllText(fileName, encoding);

                // check for RTF file
                if (fileName.EndsWith(".rtf", StringComparison.OrdinalIgnoreCase) && !textBoxText.Text.TrimStart().StartsWith("{\\rtf"))
                {
                    using (var rtb = new RichTextBox())
                    {
                        rtb.Rtf = textBoxText.Text;
                        textBoxText.Text = rtb.Text;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void buttonOpenText_Click(object sender, EventArgs e)
        {
            openFileDialog1.Title = buttonOpenText.Text;
            openFileDialog1.Filter = Configuration.Settings.Language.ImportText.TextFiles + "|*.txt|" + Configuration.Settings.Language.General.AllFiles + "|*.*";
            openFileDialog1.FileName = string.Empty;
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                LoadTextFile(openFileDialog1.FileName);
            }
            GeneratePreview();
        }

        private void buttonRefresh_Click(object sender, EventArgs e)
        {
            GeneratePreviewReal();
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
        }
    }
}