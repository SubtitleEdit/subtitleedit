using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Windows.Forms;
using System.Xml;
using Nikse.SubtitleEdit.Logic;
using System.Drawing;
using System.Globalization;

namespace Nikse.SubtitleEdit.Forms
{
    public partial class PluginsGet : Form
    {
        private XmlDocument _pluginDoc = new XmlDocument();
        private string _downloadedPluginName;
        readonly LanguageStructure.PluginsGet _language;
        bool _firstTry = true;

        public PluginsGet()
        {
            InitializeComponent();
            _language = Configuration.Settings.Language.PluginsGet;
            Text = _language.Title;
            tabPageInstalledPlugins.Text = _language.InstalledPlugins;
            tabPageGetPlugins.Text = _language.GetPlugins;

            buttonDownload.Text = _language.Download;
            buttonRemove.Text = _language.Remove;
            buttonOK.Text = Configuration.Settings.Language.General.OK;
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

            try
            {
                labelPleaseWait.Text = Configuration.Settings.Language.General.PleaseWait;
                this.Refresh();
                ShowInstalledPlugins();
                string url = "http://www.nikse.dk/Content/SubtitleEdit/Plugins/Plugins.xml";
                var wc = new WebClient { Proxy = Utilities.GetProxy() };
                wc.Encoding = System.Text.Encoding.UTF8;
                wc.Headers.Add("Accept-Encoding", "");
                wc.DownloadStringCompleted += new DownloadStringCompletedEventHandler(wc_DownloadStringCompleted);
                wc.DownloadStringAsync(new Uri(url));
            }
            catch (Exception exception)
            {
                labelPleaseWait.Text = string.Empty;
                buttonOK.Enabled = true;
                buttonDownload.Enabled = true;
                listViewGetPlugins.Enabled = true;
                MessageBox.Show(exception.Message + Environment.NewLine + Environment.NewLine + exception.StackTrace);
            }
        }

        void wc_DownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            if (e.Error != null && _firstTry)
            {
                _firstTry = false;
                string url = "http://subtitleedit.googlecode.com/files/Plugins.xml"; // retry with alternate download url
                var wc = new WebClient { Proxy = Utilities.GetProxy() };
                wc.Encoding = System.Text.Encoding.UTF8;
                wc.Headers.Add("Accept-Encoding", "");
                wc.DownloadStringCompleted += new DownloadStringCompletedEventHandler(wc_DownloadStringCompleted);
                wc.DownloadStringAsync(new Uri(url));
                return;
            }

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
            try
            {
                _pluginDoc.LoadXml(e.Result);
                string[] arr = _pluginDoc.DocumentElement.SelectSingleNode("SubtitleEditVersion").InnerText.Split("., ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                double requiredVersion = Convert.ToDouble(arr[0] + "." + arr[1], CultureInfo.InvariantCulture);

                string[] versionInfo = Utilities.AssemblyVersion.Split('.');
                double currentVersion = Convert.ToDouble(versionInfo[0] + "." + versionInfo[1], CultureInfo.InvariantCulture);

                if (currentVersion < requiredVersion)
                {
                    MessageBox.Show(_language.NewVersionOfSubtitleEditRequired);
                    DialogResult = DialogResult.Cancel;
                    return;
                }

                foreach (XmlNode node in _pluginDoc.DocumentElement.SelectNodes("Plugin"))
                {
                    ListViewItem item = new ListViewItem(node.SelectSingleNode("Name").InnerText.Trim('.'));
                    item.SubItems.Add(node.SelectSingleNode("Description").InnerText);
                    item.SubItems.Add(node.SelectSingleNode("Version").InnerText);
                    item.SubItems.Add(node.SelectSingleNode("Date").InnerText);
                    listViewGetPlugins.Items.Add(item);

                    foreach (ListViewItem installed in listViewInstalledPlugins.Items)
                    {
                        if (installed.Text.TrimEnd('.') == node.SelectSingleNode("Name").InnerText.TrimEnd('.') &&
                            installed.SubItems[2].Text.Replace(",", ".") != node.SelectSingleNode("Version").InnerText.Replace(",", "."))
                        {
                            item.BackColor = Color.LightGreen;
                            installed.BackColor = Color.LightPink;
                            installed.SubItems[1].Text = _language.UpdateAvailable + " " + installed.SubItems[1].Text;
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show(string.Format(_language.UnableToDownloadPluginListX, exception.Source + ": " + exception.Message + Environment.NewLine + Environment.NewLine + exception.StackTrace));
            }
        }

        private void ShowInstalledPlugins()
        {
            string path = Configuration.PluginsDirectory;
            if (!Directory.Exists(path))
                return;

            listViewInstalledPlugins.Items.Clear();
            string[] pluginFiles = Directory.GetFiles(path, "*.DLL");
            foreach (string pluginFileName in pluginFiles)
            {
                string name, description, text, shortcut, actionType;
                decimal version;
                System.Reflection.MethodInfo mi;
                Main.GetPropertiesAndDoAction(pluginFileName, out name, out text, out version, out description, out actionType, out shortcut, out mi);
                if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(actionType) && mi != null)
                {
                    try
                    {
                        ListViewItem item = new ListViewItem(name.Trim('.'));
                        item.Tag = pluginFileName;
                        item.SubItems.Add(description);
                        item.SubItems.Add(version.ToString());
                        item.SubItems.Add(actionType);
                        listViewInstalledPlugins.Items.Add(item);
                    }
                    catch (Exception exception)
                    {
                        MessageBox.Show("Error loading plugin:" + pluginFileName + ": " + exception.Message);
                    }
                }
            }
        }

        private void buttonDownload_Click(object sender, EventArgs e)
        {
            if (listViewGetPlugins.SelectedItems.Count == 0)
                return;

            try
            {
                labelPleaseWait.Text = Configuration.Settings.Language.General.PleaseWait;
                buttonOK.Enabled = false;
                buttonDownload.Enabled = false;
                listViewGetPlugins.Enabled = false;
                this.Refresh();
                Cursor = Cursors.WaitCursor;

                int index = listViewGetPlugins.SelectedItems[0].Index;
                string url = _pluginDoc.DocumentElement.SelectNodes("Plugin")[index].SelectSingleNode("Url").InnerText;
                _downloadedPluginName = _pluginDoc.DocumentElement.SelectNodes("Plugin")[index].SelectSingleNode("Name").InnerText;

                var wc = new WebClient { Proxy = Utilities.GetProxy() };
                wc.DownloadDataCompleted += new DownloadDataCompletedEventHandler(wc_DownloadDataCompleted);
                wc.DownloadDataAsync(new Uri(url));
                Cursor = Cursors.Default;
            }
            catch (Exception exception)
            {
                labelPleaseWait.Text = string.Empty;
                buttonOK.Enabled = true;
                buttonDownload.Enabled = true;
                listViewGetPlugins.Enabled = true;
                Cursor = Cursors.Default;
                MessageBox.Show(exception.Message + Environment.NewLine + Environment.NewLine + exception.StackTrace);
            }
        }

        void wc_DownloadDataCompleted(object sender, DownloadDataCompletedEventArgs e)
        {
            labelPleaseWait.Text = string.Empty;
            if (e.Error != null)
            {
                MessageBox.Show("Download failed!");
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
                    MessageBox.Show("Unable to create plugin folder " + pluginsFolder + ": " + exception.Message);
                    return;
                }
            }
            var ms = new MemoryStream(e.Result);

            ZipExtractor zip = ZipExtractor.Open(ms);
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
                        MessageBox.Show(string.Format("{0} already exists - unable to overwrite it", fullPath));
                        Cursor = Cursors.Default;
                        labelPleaseWait.Text = string.Empty;
                        buttonOK.Enabled = true;
                        buttonDownload.Enabled = true;
                        listViewGetPlugins.Enabled = true;
                        return;
                    }
                }
                zip.ExtractFile(entry, fullPath);
            }
            zip.Close();
            ms.Close();
            Cursor = Cursors.Default;
            labelPleaseWait.Text = string.Empty;
            buttonOK.Enabled = true;
            buttonDownload.Enabled = true;
            listViewGetPlugins.Enabled = true;
            MessageBox.Show(string.Format(_language.PluginXDownloaded, _downloadedPluginName));
            ShowInstalledPlugins();
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
                    MessageBox.Show("Unable to create plugin folder " + pluginsFolder + ": " + exception.Message);
                    return;
                }
            }
            try
            {
                System.Diagnostics.Process.Start(pluginsFolder);
            }
            catch (Exception exception)
            {
                MessageBox.Show("Cannot open folder: " + pluginsFolder + Environment.NewLine + Environment.NewLine + exception.Source + ":" + exception.Message);
            }
        }

        private void PluginsGet_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                DialogResult = DialogResult.Cancel;
            }
            else if (e.KeyCode == Keys.F1)
            {
                Utilities.ShowHelp("#plugins");
                e.SuppressKeyPress = true;
            }
        }

        private void buttonRemove_Click(object sender, EventArgs e)
        {
            if (listViewInstalledPlugins.SelectedItems.Count < 1)
                return;

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
                index--;
            if (index >= 0)
            {
                listViewInstalledPlugins.Items[index].Selected = true;
                listViewInstalledPlugins.Items[index].Focused = true;
            }
        }

    }
}
