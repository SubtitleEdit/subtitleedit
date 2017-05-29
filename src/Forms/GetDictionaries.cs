using Nikse.SubtitleEdit.Core;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Windows.Forms;
using System.Xml;

namespace Nikse.SubtitleEdit.Forms
{
    public sealed partial class GetDictionaries : Form
    {
        private List<string> _dictionaryDownloadLinks = new List<string>();
        private List<string> _descriptions = new List<string>();
        private string _xmlName;
        private int _testAllIndex = -1;

        public GetDictionaries()
        {
            InitializeComponent();

            Text = Configuration.Settings.Language.GetDictionaries.Title;
            labelDescription1.Text = Configuration.Settings.Language.GetDictionaries.DescriptionLine1;
            labelDescription2.Text = Configuration.Settings.Language.GetDictionaries.DescriptionLine2;
            linkLabelOpenDictionaryFolder.Text = Configuration.Settings.Language.GetDictionaries.OpenDictionariesFolder;
            labelChooseLanguageAndClickDownload.Text = Configuration.Settings.Language.GetDictionaries.ChooseLanguageAndClickDownload;
            buttonDownload.Text = Configuration.Settings.Language.GetDictionaries.Download;
            buttonOK.Text = Configuration.Settings.Language.General.Ok;
            labelPleaseWait.Text = string.Empty;

            FixLargeFonts();
#if DEBUG
            buttonDownloadAll.Visible = true; // For testing all download links
#endif

            // initialize combobox providers
            foreach (DictionaryProvider dicProvider in GetProviders())
            {
                comboBoxProviders.Items.Add(dicProvider);
            }
            if (comboBoxProviders.Items.Count > 0)
            {
                comboBoxProviders.SelectedIndex = 0;
            }
        }

        private static IList<DictionaryProvider> GetProviders()
        {
            return new List<DictionaryProvider>
            {
                new LibreOfficeProvider("Nikse.SubtitleEdit.Resources.HunspellDictionaries.xml.gz"),
                new GitHubProvider("Nikse.SubtitleEdit.Resources.HunspellDictionaries-github.xml.gz")
            };
        }

        private void FixLargeFonts()
        {
            if (labelDescription1.Left + labelDescription1.Width + 5 > Width)
                Width = labelDescription1.Left + labelDescription1.Width + 5;
            UiUtil.FixLargeFonts(this, buttonOK);
        }

        private void FormGetDictionaries_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                DialogResult = DialogResult.Cancel;
            }
            else if (e.KeyCode == UiUtil.HelpKeys)
            {
                Utilities.ShowHelp("#spellcheck");
                e.SuppressKeyPress = true;
            }
        }

        private void LinkLabel4LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            string dictionaryFolder = Utilities.DictionaryFolder;
            if (!Directory.Exists(dictionaryFolder))
                Directory.CreateDirectory(dictionaryFolder);

            System.Diagnostics.Process.Start(dictionaryFolder);
        }

        private void buttonDownload_Click(object sender, EventArgs e)
        {
            try
            {
                labelPleaseWait.Text = Configuration.Settings.Language.General.PleaseWait;
                buttonOK.Enabled = false;
                buttonDownload.Enabled = false;
                buttonDownloadAll.Enabled = false;
                comboBoxDictionaries.Enabled = false;
                this.Refresh();
                Cursor = Cursors.WaitCursor;

                var wc = new WebClient { Proxy = Utilities.GetProxy() };
                wc.DownloadDataCompleted += wc_DownloadDataCompleted;
                var dicInfo = (DictionaryInfo)comboBoxDictionaries.SelectedItem;
                foreach (Uri downloadLink in dicInfo.DownloadLinks)
                {
                    wc.DownloadDataAsync(downloadLink);
                }

            }
            catch (Exception exception)
            {
                labelPleaseWait.Text = string.Empty;
                buttonOK.Enabled = true;
                buttonDownload.Enabled = true;
                buttonDownloadAll.Enabled = true;
                comboBoxDictionaries.Enabled = true;
                Cursor = Cursors.Default;
                MessageBox.Show(exception.Message + Environment.NewLine + Environment.NewLine + exception.StackTrace);
            }
        }

        private void wc_DownloadDataCompleted(object sender, DownloadDataCompletedEventArgs e)
        {
            Cursor = Cursors.Default;

            if (e.Error != null)
            {
                // TODO:...

                if (_xmlName.Equals("Nikse.SubtitleEdit.Resources.HunspellDictionaries.xml.gz", StringComparison.Ordinal))
                {
                    MessageBox.Show("Unable to connect to extensions.services.openoffice.org... Switching host - please re-try!");
                    // TODO: LoadDictionaryList("Nikse.SubtitleEdit.Resources.HunspellBackupDictionaries.xml.gz");
                    labelPleaseWait.Text = string.Empty;
                    buttonOK.Enabled = true;
                    buttonDownload.Enabled = true;
                    buttonDownloadAll.Enabled = true;
                    comboBoxDictionaries.Enabled = true;
                    Cursor = Cursors.Default;
                    return;
                }
                else
                {
                    MessageBox.Show(Configuration.Settings.Language.GetTesseractDictionaries.DownloadFailed + Environment.NewLine +
                                Environment.NewLine +
                                e.Error.Message);
                    DialogResult = DialogResult.Cancel;
                    return;
                }
            }

            byte[] data = e.Result;
            string dictionaryFolder = Utilities.DictionaryFolder;
            if (!Directory.Exists(dictionaryFolder))
            {
                Directory.CreateDirectory(dictionaryFolder);
            }

            if (comboBoxProviders.SelectedItem is LibreOfficeProvider libreOfficeProvider)
            {
                libreOfficeProvider.ProccessDownloadedData(dictionaryFolder, data);
            }
            else
            {
                // github provider
                var gitHubProvider = (GitHubProvider)comboBoxProviders.SelectedItem;
                gitHubProvider.ProccessDownloadedData(dictionaryFolder, data);
            }

            Cursor = Cursors.Default;
            labelPleaseWait.Text = string.Empty;
            buttonOK.Enabled = true;
            buttonDownload.Enabled = true;
            buttonDownloadAll.Enabled = true;
            comboBoxDictionaries.Enabled = true;
            if (_testAllIndex >= 0)
            {
                DownloadNext();
                return;
            }
            // TODO: MessageBox.Show(string.Format(Configuration.Settings.Language.GetDictionaries.XDownloaded, comboBoxDictionaries.Items[index]));
        }



        private void comboBoxDictionaries_SelectedIndexChanged(object sender, EventArgs e)
        {
            var dicInfo = (DictionaryInfo)comboBoxDictionaries.SelectedItem;
            labelPleaseWait.Text = dicInfo.Description;
        }

        private void buttonDownloadAll_Click(object sender, EventArgs e)
        {
            _testAllIndex = -1;
            DownloadNext();
        }

        private void DownloadNext()
        {
            _testAllIndex++;
            if (_testAllIndex < comboBoxDictionaries.Items.Count)
            {
                comboBoxDictionaries.SelectedIndex = _testAllIndex;
                buttonDownload_Click(null, null);
            }
        }

        private void comboBoxProviders_SelectedIndexChanged(object sender, EventArgs e)
        {
            DictionaryProvider dicProvider = (DictionaryProvider)comboBoxProviders.SelectedItem;
            if (dicProvider == null)
            {
                throw new InvalidOperationException("Invalid provider selected!");
            }
            comboBoxDictionaries.Items.Clear();
            foreach (DictionaryInfo dicInfo in dicProvider.Dictionaries)
            {
                comboBoxDictionaries.Items.Add(dicInfo);
            }
            if (comboBoxDictionaries.Items.Count > 0)
            {
                comboBoxDictionaries.SelectedIndex = 0;
            }
        }
    }
}
