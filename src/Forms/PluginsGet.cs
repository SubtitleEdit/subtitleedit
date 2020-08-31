﻿using Nikse.SubtitleEdit.Core;
using Nikse.SubtitleEdit.Logic;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Cache;
using System.Windows.Forms;
using System.Xml;

namespace Nikse.SubtitleEdit.Forms
{
    public sealed partial class PluginsGet : Form
    {
        public class PluginInfoItem
        {
            public string Version { get; set; }
            public string Name { get; set; }
            public string Description { get; set; }
            public string Date { get; set; }
            public string Url { get; set; }
        }

        private List<PluginInfoItem> _downloadList;

        private string _downloadedPluginName;
        private readonly LanguageStructure.PluginsGet _language;
        private List<string> _updateAllListUrls;
        private bool _updatingAllPlugins;
        private int _updatingAllPluginsCount;
        private bool _fetchingData;
        private WebClient _wc;

        private static string GetPluginXmlFileUrl()
        {
            if (Environment.Version.Major < 4)
            {
                return "https://raw.github.com/SubtitleEdit/plugins/master/Plugins2.xml"; // .net 2-3.5
            }

            return "https://raw.github.com/SubtitleEdit/plugins/master/Plugins4.xml"; // .net 4-?
        }

        public PluginsGet()
        {
            UiUtil.PreInitialize(this);
            InitializeComponent();
            UiUtil.FixFonts(this);
            _language = Configuration.Settings.Language.PluginsGet;
            Text = _language.Title;
            tabPageInstalledPlugins.Text = _language.InstalledPlugins;
            tabPageGetPlugins.Text = _language.GetPlugins;

            buttonDownload.Text = _language.Download;
            buttonRemove.Text = _language.Remove;
            buttonOK.Text = Configuration.Settings.Language.General.Ok;
            linkLabelOpenPluginFolder.Text = _language.OpenPluginsFolder;
            labelDescription1.Text = _language.GetPluginsInfo1;
            labelClickDownload.Text = _language.GetPluginsInfo2;

            columnHeaderName.Text = Configuration.Settings.Language.General.Name;
            columnHeaderDescription.Text = _language.Description;
            columnHeaderVersion.Text = _language.Version;
            columnHeaderDate.Text = _language.Date;

            columnHeaderInsName.Text = Configuration.Settings.Language.General.Name;
            columnHeaderInsDescription.Text = _language.Description;
            columnHeaderInsVersion.Text = _language.Version;
            columnHeaderInsType.Text = _language.Type;

            labelShortcutsSearch.Text = Configuration.Settings.Language.General.Search;
            labelShortcutsSearch.Left = textBoxSearch.Left - labelShortcutsSearch.Width - 9;
            buttonSearchClear.Text = Configuration.Settings.Language.DvdSubRip.Clear;

            buttonUpdateAll.Visible = false;

            _wc = new WebClient
            {
                Proxy = Utilities.GetProxy(),
                Encoding = System.Text.Encoding.UTF8,
                CachePolicy = new RequestCachePolicy(RequestCacheLevel.NoCacheNoStore)
            };
            _wc.Headers.Add("Accept-Encoding", "");

            DownloadPluginMetadataInfos();
        }

        private void DownloadPluginMetadataInfos()
        {
            try
            {
                labelPleaseWait.Text = Configuration.Settings.Language.General.PleaseWait;
                Refresh();
                ShowInstalledPlugins();
                string url = GetPluginXmlFileUrl();
                _wc.DownloadStringCompleted += wc_DownloadStringCompleted;
                _fetchingData = true;
                _wc.DownloadStringAsync(new Uri(url));
            }
            catch (Exception exception)
            {
                ChangeControlsState(true);
                MessageBox.Show(exception.Message + Environment.NewLine + Environment.NewLine + exception.StackTrace);
            }
        }

        private void wc_DownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            _fetchingData = false;
            labelPleaseWait.Text = string.Empty;
            if (e.Error != null)
            {
                MessageBox.Show(string.Format(_language.UnableToDownloadPluginListX, e.Error.Message));
                if (e.Error.InnerException != null)
                {
                    MessageBox.Show(e.Error.InnerException.Source + ": " + e.Error.InnerException.Message + Environment.NewLine + Environment.NewLine + e.Error.InnerException.StackTrace);
                }
                return;
            }

            var pluginDoc = new XmlDocument();
            _downloadList = new List<PluginInfoItem>();
            listViewGetPlugins.BeginUpdate();
            try
            {
                pluginDoc.LoadXml(e.Result);
                string[] arr = pluginDoc.DocumentElement.SelectSingleNode("SubtitleEditVersion").InnerText.Split(new[] { '.', ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
                double requiredVersion = Convert.ToDouble(arr[0] + "." + arr[1], CultureInfo.InvariantCulture);

                string[] versionInfo = Utilities.AssemblyVersion.Split('.');
                double currentVersion = Convert.ToDouble(versionInfo[0] + "." + versionInfo[1], CultureInfo.InvariantCulture);

                if (currentVersion < requiredVersion)
                {
                    MessageBox.Show(_language.NewVersionOfSubtitleEditRequired);
                    DialogResult = DialogResult.Cancel;
                    return;
                }

                _updateAllListUrls = new List<string>();
                LoadAvailablePlugins(pluginDoc);
                ShowAvailablePlugins();
            }
            catch (Exception exception)
            {
                MessageBox.Show(string.Format(_language.UnableToDownloadPluginListX, exception.Source + ": " + exception.Message + Environment.NewLine + Environment.NewLine + exception.StackTrace));
            }
            listViewGetPlugins.EndUpdate();

            if (_updateAllListUrls.Count > 0)
            {
                buttonUpdateAll.BackColor = Color.LightGreen;
                if (Configuration.Settings.Language.PluginsGet.UpdateAllX != null)
                {
                    buttonUpdateAll.Text = string.Format(Configuration.Settings.Language.PluginsGet.UpdateAllX, _updateAllListUrls.Count);
                }
                else
                {
                    buttonUpdateAll.Text = Configuration.Settings.Language.PluginsGet.UpdateAll;
                }

                buttonUpdateAll.Visible = true;
            }
        }

        private void LoadAvailablePlugins(XmlDocument doc)
        {
            foreach (XmlNode node in doc.DocumentElement.SelectNodes("Plugin"))
            {
                var item = new PluginInfoItem
                {
                    Name = node.SelectSingleNode("Name").InnerText.Trim('.'),
                    Description = node.SelectSingleNode("Description").InnerText.Trim('.'),
                    Version = node.SelectSingleNode("Version").InnerText,
                    Date = node.SelectSingleNode("Date").InnerText,
                    Url = node.SelectSingleNode("Url").InnerText,
                };
                _downloadList.Add(item);

                foreach (ListViewItem installed in listViewInstalledPlugins.Items)
                {
                    var installedVer = Convert.ToDouble(installed.SubItems[2].Text.Replace(',', '.').Replace(CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator, "."), CultureInfo.InvariantCulture);
                    var currentVer = Convert.ToDouble(node.SelectSingleNode("Version").InnerText.Replace(',', '.').Replace(CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator, "."), CultureInfo.InvariantCulture);

                    if (string.Compare(installed.Text, node.SelectSingleNode("Name").InnerText.Trim('.'), StringComparison.OrdinalIgnoreCase) == 0 && installedVer < currentVer)
                    {
                        installed.BackColor = Color.LightPink;
                        installed.SubItems[1].Text = $"{_language.UpdateAvailable} {installed.SubItems[1].Text}";
                        buttonUpdateAll.Visible = true;
                        _updateAllListUrls.Add(item.Url);
                    }
                }
            }
        }

        private void ShowAvailablePlugins()
        {
            bool search = textBoxSearch.Text.Length > 1;
            string searchText = textBoxSearch.Text;
            listViewGetPlugins.BeginUpdate();
            if (listViewGetPlugins.Items.Count > 0)
            {
                listViewGetPlugins.Items.Clear();
            }
            foreach (var plugin in _downloadList)
            {
                var item = new ListViewItem(plugin.Name) { Tag = plugin };
                item.SubItems.Add(plugin.Description);
                item.SubItems.Add(plugin.Version);
                item.SubItems.Add(plugin.Date);

                if (!search ||
                    plugin.Name.Contains(searchText, StringComparison.OrdinalIgnoreCase) ||
                    plugin.Description.Contains(searchText, StringComparison.OrdinalIgnoreCase) ||
                    plugin.Version.Contains(searchText, StringComparison.OrdinalIgnoreCase))
                {
                    listViewGetPlugins.Items.Add(item);
                }
            }
            listViewGetPlugins.EndUpdate();
        }

        private void ShowInstalledPlugins()
        {
            string path = Configuration.PluginsDirectory;
            if (!Directory.Exists(path))
            {
                return;
            }

            listViewInstalledPlugins.BeginUpdate();
            listViewInstalledPlugins.Items.Clear();
            foreach (string pluginFileName in Directory.GetFiles(path, "*.DLL"))
            {
                Main.GetPropertiesAndDoAction(pluginFileName, out var name, out _, out var version, out var description, out var actionType, out _, out var mi);
                if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(actionType) && mi != null)
                {
                    try
                    {
                        var item = new ListViewItem(name.Trim('.')) { Tag = pluginFileName };
                        item.SubItems.Add(description);
                        item.SubItems.Add(version.ToString(CultureInfo.InvariantCulture));
                        item.SubItems.Add(actionType);
                        listViewInstalledPlugins.Items.Add(item);
                    }
                    catch (Exception exception)
                    {
                        MessageBox.Show($"Error loading plugin \"{pluginFileName}\": {exception.Message}");
                    }
                }
            }
            listViewInstalledPlugins.EndUpdate();
        }

        private void buttonDownload_Click(object sender, EventArgs e)
        {
            if (listViewGetPlugins.SelectedItems.Count == 0)
            {
                return;
            }

            try
            {
                labelPleaseWait.Text = Configuration.Settings.Language.General.PleaseWait;
                ChangeControlsState(false);
                Refresh();
                Cursor = Cursors.WaitCursor;

                var plugin = (PluginInfoItem)listViewGetPlugins.SelectedItems[0].Tag;

                string url = plugin.Url;
                _downloadedPluginName = plugin.Name;

                var wc = new WebClient { Proxy = Utilities.GetProxy(), CachePolicy = new RequestCachePolicy(RequestCacheLevel.NoCacheNoStore) };
                wc.DownloadDataCompleted += wc_DownloadDataCompleted;
                wc.DownloadDataAsync(new Uri(url));
            }
            catch (Exception exception)
            {
                ChangeControlsState(true);
                Cursor = Cursors.Default;
                MessageBox.Show(exception.Message + Environment.NewLine + Environment.NewLine + exception.StackTrace);
            }
        }

        private void wc_DownloadDataCompleted(object sender, DownloadDataCompletedEventArgs e)
        {
            labelPleaseWait.Text = string.Empty;
            if (e.Error != null)
            {
                MessageBox.Show(Configuration.Settings.Language.GetTesseractDictionaries.DownloadFailed);
                ChangeControlsState(true);
                Cursor = Cursors.Default;
                DialogResult = DialogResult.Cancel;
                return;
            }

            string pluginsFolder = Configuration.PluginsDirectory;
            if (!Directory.Exists(pluginsFolder))
            {
                try
                {
                    Directory.CreateDirectory(pluginsFolder);
                }
                catch (Exception exception)
                {
                    MessageBox.Show($"Unable to create plugin folder {pluginsFolder}: {exception.Message}");
                    ChangeControlsState(true);
                    Cursor = Cursors.Default;
                    return;
                }
            }

            var ms = new MemoryStream(e.Result);
            using (ZipExtractor zip = ZipExtractor.Open(ms))
            {
                List<ZipExtractor.ZipFileEntry> dir = zip.ReadCentralDir();

                // Extract dic/aff files in dictionary folder
                foreach (ZipExtractor.ZipFileEntry entry in dir)
                {
                    string fileName = Path.GetFileName(entry.FilenameInZip);
                    string fullPath = Path.Combine(pluginsFolder, fileName);
                    if (File.Exists(fullPath))
                    {
                        try
                        {
                            File.Delete(fullPath);
                        }
                        catch
                        {
                            MessageBox.Show($"{fullPath} already exists - unable to overwrite it");
                            Cursor = Cursors.Default;
                            ChangeControlsState(true);
                            return;
                        }
                    }
                    zip.ExtractFile(entry, fullPath);
                }
            }
            Cursor = Cursors.Default;
            ChangeControlsState(true);
            if (_updatingAllPlugins)
            {
                _updatingAllPluginsCount++;
                if (_updatingAllPluginsCount == _updateAllListUrls.Count)
                {
                    MessageBox.Show(string.Format(_language.XPluginsUpdated, _updatingAllPluginsCount));
                }
            }
            else
            {
                MessageBox.Show(string.Format(_language.PluginXDownloaded, _downloadedPluginName));
            }
            ShowInstalledPlugins();
        }

        private void ChangeControlsState(bool enable)
        {
            if (enable)
            {
                labelPleaseWait.Text = string.Empty;
            }
            buttonOK.Enabled = enable;
            buttonDownload.Enabled = enable;
            listViewGetPlugins.Enabled = enable;
        }

        private void linkLabelOpenDictionaryFolder_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            string pluginsFolder = Configuration.PluginsDirectory;
            if (!Directory.Exists(pluginsFolder))
            {
                try
                {
                    Directory.CreateDirectory(pluginsFolder);
                }
                catch (Exception exception)
                {
                    MessageBox.Show($"Unable to create plugin folder {pluginsFolder}: {exception.Message}");
                    return;
                }
            }
            UiUtil.OpenFolder(pluginsFolder);
        }

        private void PluginsGet_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                DialogResult = DialogResult.Cancel;
            }
            else if (e.KeyCode == UiUtil.HelpKeys)
            {
                Utilities.ShowHelp("#plugins");
                e.SuppressKeyPress = true;
            }
            else if (e.KeyCode == Keys.F5 && !_fetchingData)
            {
                DownloadPluginMetadataInfos();
            }
        }

        private void buttonRemove_Click(object sender, EventArgs e)
        {
            if (listViewInstalledPlugins.SelectedItems.Count < 1)
            {
                return;
            }

            string fileName = listViewInstalledPlugins.SelectedItems[0].Tag.ToString();
            int index = listViewInstalledPlugins.SelectedItems[0].Index;
            if (File.Exists(fileName))
            {
                try
                {
                    File.Delete(fileName);
                }
                catch (Exception exception)
                {
                    MessageBox.Show(exception.Message);
                    return;
                }
            }
            listViewInstalledPlugins.Items.RemoveAt(index);
            if (index >= listViewInstalledPlugins.Items.Count)
            {
                index--;
            }

            if (index >= 0)
            {
                listViewInstalledPlugins.Items[index].Selected = true;
                listViewInstalledPlugins.Items[index].Focused = true;
            }
            buttonUpdateAll.Visible = false;
            DownloadPluginMetadataInfos();
        }

        private void buttonUpdateAll_Click(object sender, EventArgs e)
        {
            buttonUpdateAll.Enabled = false;
            buttonUpdateAll.BackColor = DefaultBackColor;
            try
            {
                labelPleaseWait.Text = Configuration.Settings.Language.General.PleaseWait;
                ChangeControlsState(false);
                Refresh();
                Cursor = Cursors.WaitCursor;
                _updatingAllPluginsCount = 0;
                _updatingAllPlugins = true;
                for (int i = 0; i < _updateAllListUrls.Count; i++)
                {
                    var wc = new WebClient { Proxy = Utilities.GetProxy() };
                    wc.DownloadDataCompleted += wc_DownloadDataCompleted;
                    wc.DownloadDataAsync(new Uri(_updateAllListUrls[i]));
                }
            }
            catch (Exception exception)
            {
                ChangeControlsState(true);
                Cursor = Cursors.Default;
                MessageBox.Show(exception.Message + Environment.NewLine + Environment.NewLine + exception.StackTrace);
            }
        }

        private void buttonSearchClear_Click(object sender, EventArgs e)
        {
            textBoxSearch.Text = string.Empty;
        }

        private void textBoxSearch_TextChanged(object sender, EventArgs e)
        {
            listViewGetPlugins.BeginUpdate();
            listViewGetPlugins.Items.Clear();
            ShowAvailablePlugins();
            listViewGetPlugins.EndUpdate();
            buttonSearchClear.Enabled = textBoxSearch.Text.Length > 0;
        }
    }
}
