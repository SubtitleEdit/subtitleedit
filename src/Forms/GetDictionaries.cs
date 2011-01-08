using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Windows.Forms;
using System.Xml;
using Nikse.SubtitleEdit.Logic;
using System.Drawing;

namespace Nikse.SubtitleEdit.Forms
{
    public sealed partial class GetDictionaries : Form
    {
        List<string> _dictionaryDownloadLinks = new List<string>();
        List<string> _descriptions = new List<string>();

        public GetDictionaries()
        {
            InitializeComponent();

            Text = Configuration.Settings.Language.GetDictionaries.Title;
            labelDescription1.Text = Configuration.Settings.Language.GetDictionaries.DescriptionLine1;
            labelDescription2.Text = Configuration.Settings.Language.GetDictionaries.DescriptionLine2;
            linkLabelOpenDictionaryFolder.Text = Configuration.Settings.Language.GetDictionaries.OpenDictionariesFolder;
            labelChooseLanguageAndClickDownload.Text = Configuration.Settings.Language.GetDictionaries.ChooseLanguageAndClickDownload;
            buttonDownload.Text = Configuration.Settings.Language.GetDictionaries.Download;
            buttonOK.Text = Configuration.Settings.Language.General.OK;
            labelPleaseWait.Text = Configuration.Settings.Language.General.PleaseWait;

            System.Reflection.Assembly asm = System.Reflection.Assembly.GetExecutingAssembly();
            Stream strm = asm.GetManifestResourceStream("Nikse.SubtitleEdit.Resources.OpenOfficeDictionaries.xml.zip");
            if (strm != null)
            {
                XmlDocument doc = new XmlDocument();                    
                var rdr = new StreamReader(strm);
                using (var zip = new GZipStream(rdr.BaseStream, CompressionMode.Decompress))
                {
                    byte[] data = new byte[175000];
                    int count = zip.Read(data, 0, 175000);
                    doc.LoadXml(System.Text.Encoding.UTF8.GetString(data));
                }
                rdr.Close();
                
                foreach (XmlNode node in doc.DocumentElement.SelectNodes("Dictionary"))
                {
                    string englishName = node.SelectSingleNode("EnglishName").InnerText;
                    string nativeName = node.SelectSingleNode("NativeName").InnerText;
                    string downloadLink = node.SelectSingleNode("DownloadLink").InnerText;

                    string description = string.Empty;
                    if (node.SelectSingleNode("Description") != null)
                        description = node.SelectSingleNode("Description").InnerText;

                    if (!string.IsNullOrEmpty(downloadLink))
                    {
                        string name = englishName;
                        if (!string.IsNullOrEmpty(nativeName))
                            name += " - " + nativeName;
                        
                        comboBoxDictionaries.Items.Add(name);
                        _dictionaryDownloadLinks.Add(downloadLink);
                        _descriptions.Add(description);
                    }
                    comboBoxDictionaries.SelectedIndex = 0;
                }
            }
            FixLargeFonts();
        }

        private void FixLargeFonts()
        {
            if (labelDescription1.Left + labelDescription1.Width + 5 > Width)
                Width = labelDescription1.Left + labelDescription1.Width + 5;

            Graphics graphics = this.CreateGraphics();
            SizeF textSize = graphics.MeasureString(buttonOK.Text, this.Font);
            if (textSize.Height > buttonOK.Height - 4)
            {
                int newButtonHeight = (int)(textSize.Height + 7 + 0.5);
                Utilities.SetButtonHeight(this, newButtonHeight, 1);
            }
        }

        private void FormGetDictionaries_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
                DialogResult = DialogResult.Cancel;
        }

        private void LinkLabel3LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("http://nhunspell.sourceforge.net/");                        
        }

        private void LinkLabel4LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            string dictionaryFolder = Utilities.DictionaryFolder;
            if (!Directory.Exists(dictionaryFolder))
                Directory.CreateDirectory(dictionaryFolder);

            System.Diagnostics.Process.Start(dictionaryFolder);            
        }

        private void buttonDownload_Click(object sender, System.EventArgs e)
        {
            try
            {
                labelPleaseWait.Text = Configuration.Settings.Language.General.PleaseWait;
                buttonOK.Enabled = false;
                buttonDownload.Enabled = false;
                comboBoxDictionaries.Enabled = false;
                this.Refresh();
                Cursor = Cursors.WaitCursor;
                string dictionaryFolder = Utilities.DictionaryFolder;
                if (!Directory.Exists(dictionaryFolder))
                    Directory.CreateDirectory(dictionaryFolder);

                int index = comboBoxDictionaries.SelectedIndex;
                string url = _dictionaryDownloadLinks[index];

                var wc = new WebClient { Proxy = Utilities.GetProxy() };
                var ms = new MemoryStream(wc.DownloadData(url));
                var reader = new StreamReader(ms);

                ZipExtractor zip = ZipExtractor.Open(ms);
                List<ZipExtractor.ZipFileEntry> dir = zip.ReadCentralDir();

                // Extract dic/aff files in dictionary folder
                string path;
                bool result;
                foreach (ZipExtractor.ZipFileEntry entry in dir)
                {
                    if (entry.FilenameInZip.ToLower().EndsWith(".dic") || entry.FilenameInZip.ToLower().EndsWith(".aff"))
                    {
                        string fileName = Path.GetFileName(entry.FilenameInZip);

                        // French fix
                        if (fileName.StartsWith("fr-moderne"))
                            fileName = fileName.Replace("fr-moderne", "fr_FR");

                        // German fix
                        if (fileName.StartsWith("de_DE_frami"))
                            fileName = fileName.Replace("de_DE_frami", "de_DE");                       

                        path = Path.Combine(dictionaryFolder, fileName);
                        result = zip.ExtractFile(entry, path);
                    }
                }
                zip.Close();
                ms.Close();
                Cursor = Cursors.Default;
                labelPleaseWait.Text = string.Empty;
                buttonOK.Enabled = true;
                buttonDownload.Enabled = true;
                comboBoxDictionaries.Enabled = true;
                MessageBox.Show(string.Format(Configuration.Settings.Language.GetDictionaries.XDownloaded, comboBoxDictionaries.Items[index]));
            }
            catch (Exception exception)
            {
                labelPleaseWait.Text = string.Empty;
                buttonOK.Enabled = true;
                buttonDownload.Enabled = true;
                comboBoxDictionaries.Enabled = true;
                Cursor = Cursors.Default;
                MessageBox.Show(exception.Message + Environment.NewLine + Environment.NewLine + exception.StackTrace);
            }            
        }

        private void comboBoxDictionaries_SelectedIndexChanged(object sender, EventArgs e)
        {
            int index = comboBoxDictionaries.SelectedIndex;
            labelPleaseWait.Text = _descriptions[index];                
        }

    }
}
