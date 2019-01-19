using Nikse.SubtitleEdit.Core;
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
        private Subtitle _subtitle;
        private XmlDocument _xml;
        private XmlNamespaceManager _nsmgr;
        private string _NA;

        public TimedTextProperties(Subtitle subtitle)
        {
            UiUtil.PreInitialize(this);
            InitializeComponent();
            UiUtil.FixFonts(this);
            Application.DoEvents();

            _subtitle = subtitle;
            _NA = "[" + Configuration.Settings.Language.General.NotAvailable + "]";
            comboBoxDropMode.Items[0] = _NA;
            comboBoxTimeBase.Items[0] = _NA;
            comboBoxDefaultStyle.Items.Add(_NA);
            comboBoxDefaultRegion.Items.Add(_NA);

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
            _nsmgr = new XmlNamespaceManager(_xml.NameTable);
            _nsmgr.AddNamespace("ttml", "http://www.w3.org/ns/ttml");

            XmlNode node = _xml.DocumentElement.SelectSingleNode("ttml:head/ttml:metadata/ttml:title", _nsmgr);
            if (node != null)
            {
                textBoxTitle.Text = node.InnerText;
            }

            node = _xml.DocumentElement.SelectSingleNode("ttml:head/ttml:metadata/ttml:desc", _nsmgr);
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
                node = _xml.DocumentElement.SelectSingleNode("ttml:body", _nsmgr);
                if (node != null && node.Attributes["style"] != null && style == node.Attributes["style"].Value)
                {
                    comboBoxDefaultStyle.SelectedIndex = comboBoxDefaultStyle.Items.Count - 1;
                }
            }
            foreach (string region in TimedText10.GetRegionsFromHeader(_subtitle.Header))
            {
                comboBoxDefaultRegion.Items.Add(region);
                node = _xml.DocumentElement.SelectSingleNode("ttml:body", _nsmgr);
                if (node != null && node.Attributes["region"] != null && region == node.Attributes["region"].Value)
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
                    return;
                }
            }
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            XmlNode node = _xml.DocumentElement.SelectSingleNode("ttml:head/ttml:metadata/ttml:title", _nsmgr);
            if (node != null)
            {
                if (string.IsNullOrWhiteSpace(textBoxTitle.Text) && string.IsNullOrWhiteSpace(textBoxDescription.Text))
                {
                    _xml.DocumentElement.SelectSingleNode("ttml:head", _nsmgr).RemoveChild(_xml.DocumentElement.SelectSingleNode("ttml:head/ttml:metadata", _nsmgr));
                }
                else
                {
                    node.InnerText = textBoxTitle.Text;
                }
            }
            else if (!string.IsNullOrWhiteSpace(textBoxTitle.Text))
            {
                var head = _xml.DocumentElement.SelectSingleNode("ttml:head", _nsmgr);
                if (head == null)
                {
                    head = _xml.CreateElement("ttml", "head", _nsmgr.LookupNamespace("ttml"));
                    _xml.DocumentElement.PrependChild(head);
                }

                var metadata = _xml.DocumentElement.SelectSingleNode("ttml:head/ttml:metadata", _nsmgr);
                if (metadata == null)
                {
                    metadata = _xml.CreateElement("ttml", "metadata", _nsmgr.LookupNamespace("ttml"));
                    head.PrependChild(metadata);
                }

                var title = _xml.CreateElement("ttml", "title", _nsmgr.LookupNamespace("ttml"));
                metadata.InnerText = textBoxTitle.Text;
                metadata.AppendChild(title);
            }

            node = _xml.DocumentElement.SelectSingleNode("ttml:head/ttml:metadata/ttml:desc", _nsmgr);
            if (node != null)
            {
                node.InnerText = textBoxDescription.Text;
            }
            else if (!string.IsNullOrWhiteSpace(textBoxDescription.Text))
            {
                var head = _xml.DocumentElement.SelectSingleNode("ttml:head", _nsmgr);
                if (head == null)
                {
                    head = _xml.CreateElement("ttml", "head", _nsmgr.LookupNamespace("ttml"));
                    _xml.DocumentElement.PrependChild(head);
                }

                var metadata = _xml.DocumentElement.SelectSingleNode("ttml:head/ttml:metadata", _nsmgr);
                if (metadata == null)
                {
                    metadata = _xml.CreateElement("ttml", "metadata", _nsmgr.LookupNamespace("ttml"));
                    head.PrependChild(metadata);
                }

                var desc = _xml.CreateElement("ttml", "desc", _nsmgr.LookupNamespace("ttml"));
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
                attr = _xml.CreateAttribute("xml", "lang", _nsmgr.LookupNamespace("xml"));
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
                attr = _xml.CreateAttribute("ttp", "timeBase", _nsmgr.LookupNamespace("ttp"));
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
                attr = _xml.CreateAttribute("ttp", "frameRate", _nsmgr.LookupNamespace("ttp"));
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
                attr = _xml.CreateAttribute("ttp", "frameRateMultiplier", _nsmgr.LookupNamespace("ttp"));
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
                attr = _xml.CreateAttribute("ttp", "dropMode", _nsmgr.LookupNamespace("ttp"));
                attr.Value = comboBoxDropMode.Text;
                _xml.DocumentElement.Attributes.Append(attr);
            }

            node = _xml.DocumentElement.SelectSingleNode("ttml:body", _nsmgr);
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

            node = _xml.DocumentElement.SelectSingleNode("ttml:body", _nsmgr);
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

            DialogResult = DialogResult.OK;
        }

    }
}
