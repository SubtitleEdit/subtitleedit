using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Logic;
using System;
using System.Globalization;
using System.Windows.Forms;
using System.Xml;

namespace Nikse.SubtitleEdit.Forms
{
    public partial class TimedTextProperties : PositionAndSizeForm
    {
        private readonly Subtitle _subtitle;
        private readonly XmlDocument _xml;
        private readonly XmlNamespaceManager _namespaceManager;

        public TimedTextProperties(Subtitle subtitle)
        {
            UiUtil.PreInitialize(this);
            InitializeComponent();
            UiUtil.FixFonts(this);
            Application.DoEvents();

            _subtitle = subtitle;
            var notAvailable = "[" + LanguageSettings.Current.General.NotAvailable + "]";
            comboBoxDropMode.Items[0] = notAvailable;
            comboBoxTimeBase.Items[0] = notAvailable;
            comboBoxDefaultStyle.Items.Add(notAvailable);
            comboBoxDefaultRegion.Items.Add(notAvailable);

            _xml = new XmlDocument();
            try
            {
                _xml.LoadXml(subtitle.Header);
            }
            catch
            {
                subtitle.Header = new TimedText10().ToText(new Subtitle(), "tt");
                _xml.LoadXml(subtitle.Header); // load default xml
            }
            _namespaceManager = new XmlNamespaceManager(_xml.NameTable);
            _namespaceManager.AddNamespace("ttml", TimedText10.TtmlNamespace);
            _namespaceManager.AddNamespace("ttp", TimedText10.TtmlParameterNamespace);
            _namespaceManager.AddNamespace("tts", TimedText10.TtmlStylingNamespace);
            _namespaceManager.AddNamespace("ttm", TimedText10.TtmlMetadataNamespace);

            XmlNode node = _xml.DocumentElement.SelectSingleNode("ttml:head/ttml:metadata/ttml:title", _namespaceManager);
            if (node != null)
            {
                textBoxTitle.Text = node.InnerText;
            }

            node = _xml.DocumentElement.SelectSingleNode("ttml:head/ttml:metadata/ttml:desc", _namespaceManager);
            if (node != null)
            {
                textBoxDescription.Text = node.InnerText;
            }

            foreach (CultureInfo ci in CultureInfo.GetCultures(CultureTypes.SpecificCultures))
            {
                comboBoxLanguage.Items.Add(ci.Name);
            }
            XmlAttribute attr = _xml.DocumentElement.Attributes["xml:lang"];
            if (attr != null)
            {
                comboBoxLanguage.Text = attr.InnerText;
            }

            attr = _xml.DocumentElement.Attributes["ttp:timeBase"];
            if (attr != null)
            {
                comboBoxTimeBase.Text = attr.InnerText;
            }

            comboBoxFrameRate.Items.Add("23.976");
            comboBoxFrameRate.Items.Add("24.0");
            comboBoxFrameRate.Items.Add("25.0");
            comboBoxFrameRate.Items.Add("29.97");
            comboBoxFrameRate.Items.Add("30.0");
            attr = _xml.DocumentElement.Attributes["ttp:frameRate"];
            if (attr != null)
            {
                comboBoxFrameRate.Text = attr.InnerText;
            }

            attr = _xml.DocumentElement.Attributes["ttp:frameRateMultiplier"];
            if (attr != null)
            {
                comboBoxFrameRateMultiplier.Text = attr.InnerText;
            }

            attr = _xml.DocumentElement.Attributes["ttp:dropMode"];
            if (attr != null)
            {
                comboBoxDropMode.Text = attr.InnerText;
            }

            foreach (string style in TimedText10.GetStylesFromHeader(_subtitle.Header))
            {
                comboBoxDefaultStyle.Items.Add(style);
                node = _xml.DocumentElement.SelectSingleNode("ttml:body", _namespaceManager);
                if (node?.Attributes?["style"] != null && style == node.Attributes["style"].Value)
                {
                    comboBoxDefaultStyle.SelectedIndex = comboBoxDefaultStyle.Items.Count - 1;
                }
            }
            foreach (string region in TimedText10.GetRegionsFromHeader(_subtitle.Header))
            {
                comboBoxDefaultRegion.Items.Add(region);
                node = _xml.DocumentElement.SelectSingleNode("ttml:body", _namespaceManager);
                if (node?.Attributes?["region"] != null && region == node.Attributes["region"].Value)
                {
                    comboBoxDefaultRegion.SelectedIndex = comboBoxDefaultRegion.Items.Count - 1;
                }
            }

            var timeCodeFormat = Configuration.Settings.SubtitleSettings.TimedText10TimeCodeFormat.Trim().ToLowerInvariant();
            comboBoxTimeCodeFormat.SelectedIndex = 0;
            for (int index = 0; index < comboBoxTimeCodeFormat.Items.Count; index++)
            {
                var item = comboBoxTimeCodeFormat.Items[index];
                if (item.ToString().ToLowerInvariant() == timeCodeFormat)
                {
                    comboBoxTimeCodeFormat.SelectedIndex = index;
                    break;
                }
            }

            var ext = Configuration.Settings.SubtitleSettings.TimedText10FileExtension;
            comboBoxFileExtensions.SelectedIndex = 0;
            for (var index = 0; index < comboBoxFileExtensions.Items.Count; index++)
            {
                var item = comboBoxFileExtensions.Items[index];
                if (item.ToString() == ext)
                {
                    comboBoxFileExtensions.SelectedIndex = index;
                    break;
                }
            }
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            XmlNode node = _xml.DocumentElement.SelectSingleNode("ttml:head/ttml:metadata/ttml:title", _namespaceManager);
            if (node != null)
            {
                if (string.IsNullOrWhiteSpace(textBoxTitle.Text) && string.IsNullOrWhiteSpace(textBoxDescription.Text))
                {
                    _xml.DocumentElement.SelectSingleNode("ttml:head", _namespaceManager).RemoveChild(_xml.DocumentElement.SelectSingleNode("ttml:head/ttml:metadata", _namespaceManager));
                }
                else
                {
                    node.InnerText = textBoxTitle.Text;
                }
            }
            else if (!string.IsNullOrWhiteSpace(textBoxTitle.Text))
            {
                var head = _xml.DocumentElement.SelectSingleNode("ttml:head", _namespaceManager);
                if (head == null)
                {
                    head = _xml.CreateElement("ttml", "head", _namespaceManager.LookupNamespace("ttml"));
                    _xml.DocumentElement.PrependChild(head);
                }

                var metadata = _xml.DocumentElement.SelectSingleNode("ttml:head/ttml:metadata", _namespaceManager);
                if (metadata == null)
                {
                    metadata = _xml.CreateElement("ttml", "metadata", _namespaceManager.LookupNamespace("ttml"));
                    head.PrependChild(metadata);
                }

                var title = _xml.CreateElement("ttml", "title", _namespaceManager.LookupNamespace("ttml"));
                metadata.InnerText = textBoxTitle.Text;
                metadata.AppendChild(title);
            }

            node = _xml.DocumentElement.SelectSingleNode("ttml:head/ttml:metadata/ttml:desc", _namespaceManager);
            if (node != null)
            {
                node.InnerText = textBoxDescription.Text;
            }
            else if (!string.IsNullOrWhiteSpace(textBoxDescription.Text))
            {
                var head = _xml.DocumentElement.SelectSingleNode("ttml:head", _namespaceManager);
                if (head == null)
                {
                    head = _xml.CreateElement("ttml", "head", _namespaceManager.LookupNamespace("ttml"));
                    _xml.DocumentElement.PrependChild(head);
                }

                var metadata = _xml.DocumentElement.SelectSingleNode("ttml:head/ttml:metadata", _namespaceManager);
                if (metadata == null)
                {
                    metadata = _xml.CreateElement("ttml", "metadata", _namespaceManager.LookupNamespace("ttml"));
                    head.PrependChild(metadata);
                }

                var desc = _xml.CreateElement("ttml", "desc", _namespaceManager.LookupNamespace("ttml"));
                desc.InnerText = textBoxDescription.Text;
                metadata.AppendChild(desc);
            }

            XmlAttribute attr = _xml.DocumentElement.Attributes["xml:lang"];
            if (attr != null)
            {
                attr.Value = comboBoxLanguage.Text;
                if (attr.Value.Length == 0)
                {
                    _xml.DocumentElement.Attributes.Remove(attr);
                }
            }
            else if (comboBoxLanguage.Text.Length > 0)
            {
                attr = _xml.CreateAttribute("xml", "lang", _namespaceManager.LookupNamespace("xml"));
                attr.Value = comboBoxLanguage.Text;
                _xml.DocumentElement.Attributes.Prepend(attr);
            }

            attr = _xml.DocumentElement.Attributes["ttp:timeBase"];
            if (attr != null)
            {
                attr.InnerText = comboBoxTimeBase.Text;
                if (attr.Value.Length == 0)
                {
                    _xml.DocumentElement.Attributes.Remove(attr);
                }
            }
            else if (comboBoxTimeBase.Text.Length > 0)
            {
                attr = _xml.CreateAttribute("ttp", "timeBase", _namespaceManager.LookupNamespace("ttp"));
                attr.Value = comboBoxTimeBase.Text;
                _xml.DocumentElement.Attributes.Append(attr);
            }

            attr = _xml.DocumentElement.Attributes["ttp:frameRate"];
            if (attr != null)
            {
                attr.InnerText = comboBoxFrameRate.Text;
                if (attr.Value.Length == 0)
                {
                    _xml.DocumentElement.Attributes.Remove(attr);
                }
            }
            else if (comboBoxFrameRate.Text.Length > 0)
            {
                attr = _xml.CreateAttribute("ttp", "frameRate", _namespaceManager.LookupNamespace("ttp"));
                attr.Value = comboBoxFrameRate.Text;
                _xml.DocumentElement.Attributes.Append(attr);
            }

            attr = _xml.DocumentElement.Attributes["ttp:frameRateMultiplier"];
            if (attr != null)
            {
                attr.InnerText = comboBoxFrameRateMultiplier.Text;
                if (attr.Value.Length == 0)
                {
                    _xml.DocumentElement.Attributes.Remove(attr);
                }
            }
            else if (comboBoxFrameRateMultiplier.Text.Length > 0)
            {
                attr = _xml.CreateAttribute("ttp", "frameRateMultiplier", _namespaceManager.LookupNamespace("ttp"));
                attr.Value = comboBoxFrameRateMultiplier.Text;
                _xml.DocumentElement.Attributes.Append(attr);
            }

            attr = _xml.DocumentElement.Attributes["ttp:dropMode"];
            if (attr != null)
            {
                attr.InnerText = comboBoxDropMode.Text;
                if (attr.Value.Length == 0)
                {
                    _xml.DocumentElement.Attributes.Remove(attr);
                }
            }
            else if (comboBoxDropMode.Text.Length > 0)
            {
                attr = _xml.CreateAttribute("ttp", "dropMode", _namespaceManager.LookupNamespace("ttp"));
                attr.Value = comboBoxDropMode.Text;
                _xml.DocumentElement.Attributes.Append(attr);
            }

            node = _xml.DocumentElement.SelectSingleNode("ttml:body", _namespaceManager);
            if (node != null && node.Attributes["style"] != null)
            {
                node.Attributes["style"].Value = comboBoxDefaultStyle.Text;
            }
            else if (comboBoxDefaultStyle.Text.Length > 0 && node != null)
            {
                attr = _xml.CreateAttribute("style");
                attr.Value = comboBoxDefaultStyle.Text;
                node.Attributes.Append(attr);
            }

            node = _xml.DocumentElement.SelectSingleNode("ttml:body", _namespaceManager);
            if (node != null && node.Attributes["region"] != null)
            {
                node.Attributes["region"].Value = comboBoxDefaultRegion.Text;
            }
            else if (comboBoxDefaultRegion.Text.Length > 0 && node != null)
            {
                attr = _xml.CreateAttribute("region");
                attr.Value = comboBoxDefaultRegion.Text;
                node.Attributes.Append(attr);
            }

            _subtitle.Header = _xml.OuterXml;

            Configuration.Settings.SubtitleSettings.TimedText10TimeCodeFormat = comboBoxTimeCodeFormat.SelectedItem.ToString();

            var currentTimedTextExt = Configuration.Settings.SubtitleSettings.TimedText10FileExtension;
            var newTimedTextExt = comboBoxFileExtensions.SelectedItem.ToString();
            if (currentTimedTextExt != newTimedTextExt)
            {
                var favoriteFormats = Configuration.Settings.General.FavoriteSubtitleFormats;
                var currentTimedTextWithExt = $"Timed Text 1.0 ({currentTimedTextExt})";
                var newTimedTextWithExt = $"Timed Text 1.0 ({newTimedTextExt})";
                if (favoriteFormats != null && favoriteFormats.Contains(currentTimedTextWithExt))
                {
                    Configuration.Settings.General.FavoriteSubtitleFormats = favoriteFormats.Replace(currentTimedTextWithExt, newTimedTextWithExt);
                }

                Configuration.Settings.SubtitleSettings.TimedText10FileExtension = newTimedTextExt;
            }

            DialogResult = DialogResult.OK;
        }

        private void TimedTextProperties_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                DialogResult = DialogResult.Cancel;
            }
        }
    }
}
