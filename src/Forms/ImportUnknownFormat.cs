using Nikse.SubtitleEdit.Core;
using Nikse.SubtitleEdit.Logic;
using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.Forms
{
    public partial class ImportUnknownFormat : Form
    {
        public Subtitle ImportedSubitle { get; private set; }
        private readonly Timer _refreshTimer = new Timer();

        public ImportUnknownFormat(string fileName)
        {
            UiUtil.PreInitialize(this);
            InitializeComponent();
            UiUtil.FixFonts(this);
            _refreshTimer.Interval = 400;
            _refreshTimer.Tick += RefreshTimerTick;

            buttonOK.Text = Configuration.Settings.Language.General.Ok;
            buttonCancel.Text = Configuration.Settings.Language.General.Cancel;
            SubtitleListview1.InitializeLanguage(Configuration.Settings.Language.General, Configuration.Settings);
            UiUtil.InitializeSubtitleFont(SubtitleListview1);
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
            }

            _refreshTimer.Start();
        }

        private void GeneratePreviewReal()
        {
            var uknownFormatImporter = new UnknownFormatImporter { UseFrames = radioButtonTimeCodeFrames.Checked };
            ImportedSubitle = uknownFormatImporter.AutoGuessImport(textBoxText.Lines.ToList());
            groupBoxImportResult.Text = string.Format(Configuration.Settings.Language.ImportText.PreviewLinesModifiedX, ImportedSubitle.Paragraphs.Count);
            SubtitleListview1.Fill(ImportedSubitle);
            if (ImportedSubitle.Paragraphs.Count > 0)
            {
                SubtitleListview1.SelectIndexAndEnsureVisible(0);
            }
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
                var encoding = LanguageAutoDetect.GetEncodingFromFile(fileName);
                textBoxText.Text = File.ReadAllText(fileName, encoding);

                // check for RTF file
                if (fileName.EndsWith(".rtf", StringComparison.OrdinalIgnoreCase) && !textBoxText.Text.TrimStart().StartsWith("{\\rtf", StringComparison.Ordinal))
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
