using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Windows.Forms;
using System.Xml;
using Nikse.SubtitleEdit.Logic;

namespace Nikse.SubtitleEdit.Forms
{
    public partial class PluginsGet : Form
    {
        private XmlDocument _pluginDoc = new XmlDocument();
        private string _downloadedPluginName;

        public PluginsGet()
        {
            InitializeComponent();

            try
            {
                labelPleaseWait.Text = Configuration.Settings.Language.General.PleaseWait;
                this.Refresh();
                string url = "http://www.nikse.dk/Content/SubtitleEdit/Plugins/Index.xml";
                var wc = new WebClient { Proxy = Utilities.GetProxy() };
                wc.DownloadDataCompleted += new DownloadDataCompletedEventHandler(PluginListDownloadDataCompleted);
                wc.DownloadDataAsync(new Uri(url));

                ShowInstalledPlugins();
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

        private void ShowInstalledPlugins()
        {
            string path = Path.Combine(Configuration.BaseDirectory, "Plugins");
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
                        ListViewItem item = new ListViewItem(name);
                        item.Tag = pluginFileName;
                        item.SubItems.Add("Description");
                        item.SubItems.Add("Version");
                        item.SubItems.Add("Date");
                        listViewInstalledPlugins.Items.Add(item);
                    }
                    catch (Exception exception)
                    {
                        MessageBox.Show("Error loading plugin:" + pluginFileName + ": " + exception.Message);
                    }
                }
            }
        }

        void PluginListDownloadDataCompleted(object sender, DownloadDataCompletedEventArgs e)
        {
            labelPleaseWait.Text = string.Empty;
            if (e.Error != null)
            {
                MessageBox.Show("Download of plugin list failed: " + e.Error.Message);
                return;
            }
            try
            {
                _pluginDoc.LoadXml(System.Text.Encoding.UTF8.GetString(e.Result));
                string[] arr = _pluginDoc.DocumentElement.SelectSingleNode("SubtitleEditVersion").InnerText.Split("., ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                double requiredVersion = Convert.ToDouble(arr[0] + "." + arr[1]);

                string[] versionInfo = Utilities.AssemblyVersion.Split('.');
                double currentVersion = Convert.ToDouble(versionInfo[0] + "." + versionInfo[1]);

                if (currentVersion < requiredVersion)
                {
                    MessageBox.Show("You need a newer version of Subtitle Edit!");
                    DialogResult = DialogResult.Cancel;
                    return;
                }

                foreach (XmlNode node in _pluginDoc.DocumentElement.SelectNodes("Plugin"))
                {
                    ListViewItem item = new ListViewItem(node.SelectSingleNode("Name").InnerText);
                    item.SubItems.Add(node.SelectSingleNode("Description").InnerText);
                    item.SubItems.Add(node.SelectSingleNode("Version").InnerText);
                    item.SubItems.Add(node.SelectSingleNode("Date").InnerText);
                    listViewGetPlugins.Items.Add(item);
                }
            }
            catch
            {
                MessageBox.Show("Load of downloaded xml plugin-list faild!");
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

            string pluginsFolder = Path.Combine(Configuration.DataDirectory, "Plugins");
            if (!Directory.Exists(pluginsFolder))
                Directory.CreateDirectory(pluginsFolder);

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
            MessageBox.Show(string.Format("Plugin '{0}' downloaded", _downloadedPluginName));
            ShowInstalledPlugins();
        }

        private void linkLabelOpenDictionaryFolder_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            string pluginsFolder = Path.Combine(Configuration.DataDirectory, "Plugins");
            if (!Directory.Exists(pluginsFolder))
                Directory.CreateDirectory(pluginsFolder);

            System.Diagnostics.Process.Start(pluginsFolder);
        }

        private void PluginsGet_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
                DialogResult = DialogResult.Cancel;
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
