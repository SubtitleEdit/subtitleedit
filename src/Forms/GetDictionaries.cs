using Nikse.SubtitleEdit.Core;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.Forms
{
    public sealed partial class GetDictionaries : Form
    {
        private List<string> _dictionaryDownloadLinks = new List<string>();
        private List<string> _descriptions = new List<string>();
        private int _testAllIndex = -1;
        private volatile int _downloadCount;

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
                ControlsState(false);
                this.Refresh();
                Cursor = Cursors.WaitCursor;
                var dicInfo = (DictionaryInfo)comboBoxDictionaries.SelectedItem;
                _downloadCount = 1;
                if (comboBoxProviders.SelectedIndex > 0)
                {
                    _downloadCount++;
                }
                // GC.Collect();
                foreach (var downloadLink in dicInfo.DownloadLinks)
                {
                    // WebClient doesn't support concurrent io, so keep this inside loop...
                    var client = new WebClient() { Proxy = Utilities.GetProxy() };
                    client.DownloadDataCompleted += wc_DownloadDataCompleted;
                    client.DownloadDataAsync(downloadLink, downloadLink.ToString().EndsWith(".dic", StringComparison.OrdinalIgnoreCase));
                }
            }
            catch (Exception exception)
            {
                _downloadCount--;
                if (_downloadCount == 0)
                {
                    labelPleaseWait.Text = string.Empty;
                    ControlsState(true);
                    Cursor = Cursors.Default;
                    MessageBox.Show(exception.Message + Environment.NewLine + Environment.NewLine + exception.StackTrace);
                }
            }
        }

        private void ControlsState(bool state)
        {
            buttonOK.Enabled = state;
            buttonDownload.Enabled = state;
            buttonDownloadAll.Enabled = state;
            comboBoxDictionaries.Enabled = state;
        }

        private void wc_DownloadDataCompleted(object sender, DownloadDataCompletedEventArgs e)
        {
            _downloadCount--;
            Cursor = Cursors.Default;
            if (e.Error != null)
            {
                if (_downloadCount > 0)
                {
                    return;
                }

                DictionaryProvider currentProvider = (DictionaryProvider)comboBoxProviders.SelectedItem;
                string failMessage = $"Unable to connect to {currentProvider.Name} provider... switch provider and try again!";
                MessageBox.Show(failMessage);
                labelPleaseWait.Text = string.Empty;
                ControlsState(true);
                Cursor = Cursors.Default;
                return;
            }

            byte[] data = e.Result;
            string dictionaryFolder = Utilities.DictionaryFolder;
            if (!Directory.Exists(dictionaryFolder))
            {
                try
                {
                    Directory.CreateDirectory(dictionaryFolder);
                }
                catch
                {
                    return;
                }
            }

            if (comboBoxProviders.SelectedItem is LibreOfficeProvider libreOfficeProvider)
            {
                libreOfficeProvider.ProccessDownloadedData(dictionaryFolder, data);
            }
            else
            {
                var gitHubProvider = (GitHubProvider)comboBoxProviders.SelectedItem;
                var dicInfo = (DictionaryInfo)comboBoxDictionaries.SelectedItem;
                gitHubProvider.ProccessDownloadedData(dictionaryFolder, dicInfo, data, (bool)e.UserState ? ".dic" : ".aff");
            }

            if (_downloadCount == 0)
            {
                Cursor = Cursors.Default;
                labelPleaseWait.Text = string.Empty;
                ControlsState(true);
                if (_testAllIndex >= 0)
                {
                    DownloadNext();
                    return;
                }
                MessageBox.Show(string.Format(Configuration.Settings.Language.GetDictionaries.XDownloaded, comboBoxDictionaries.SelectedItem));
            }
        }

        private void comboBoxDictionaries_SelectedIndexChanged(object sender, EventArgs e)
        {
            var dicInfo = (DictionaryInfo)comboBoxDictionaries.SelectedItem;
            labelPleaseWait.Text = dicInfo.Description;
            if (labelPleaseWait.Width > comboBoxProviders.Width)
            {
                int len = labelPleaseWait.Text.Length;
                labelPleaseWait.Text = labelPleaseWait.Text.Remove(len - 13) + "...";
            }
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
