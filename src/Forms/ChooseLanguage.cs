using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Windows.Forms;
using Nikse.SubtitleEdit.Logic;
using System.Xml;

namespace Nikse.SubtitleEdit.Forms
{
    public sealed partial class ChooseLanguage : PositionAndSizeForm
    {
        public class CultureListItem
        {
            private CultureInfo _cultureInfo;

            public CultureListItem(CultureInfo cultureInfo)
            {
                _cultureInfo = cultureInfo;
            }

            public override string ToString()
            {
                return char.ToUpper(_cultureInfo.NativeName[0]) + _cultureInfo.NativeName.Substring(1);
            }

            public string Name
            {
                get { return _cultureInfo.Name; }
            }
        }

        public string CultureName
        {
            get
            {
                int index = comboBoxLanguages.SelectedIndex;
                if (index == -1)
                    return "en-US";
                else
                    return (comboBoxLanguages.Items[index] as CultureListItem).Name;
            }
        }

        public ChooseLanguage()
        {
            InitializeComponent();

            List<string> list = new List<string>();
            if (Directory.Exists(Path.Combine(Configuration.BaseDirectory, "Languages")))
            {
                string[] versionInfo = Utilities.AssemblyVersion.Split('.');
                string currentVersion = String.Format("{0}.{1}.{2}", versionInfo[0], versionInfo[1], versionInfo[2]);

                foreach (string fileName in Directory.GetFiles(Path.Combine(Configuration.BaseDirectory, "Languages"), "*.xml"))
                {
                    string cultureName = Path.GetFileNameWithoutExtension(fileName);
                    XmlDocument doc = new XmlDocument();
                    doc.Load(fileName);
                    try
                    {
                        string version = doc.DocumentElement.SelectSingleNode("General/Version").InnerText;
                        if (version == currentVersion)
                            list.Add(cultureName);
                    }
                    catch
                    {
                    }
                }
            }
            list.Sort();
            comboBoxLanguages.Items.Add(new CultureListItem(CultureInfo.CreateSpecificCulture("en-US")));
            foreach (string cultureName in list)
            {
                try
                {
                    if (cultureName.Equals("zh-CHS", StringComparison.OrdinalIgnoreCase))
                        comboBoxLanguages.Items.Add(new CultureListItem(new CultureInfo(0x4)));
                    else if (cultureName.Equals("zh-CHT", StringComparison.OrdinalIgnoreCase))
                        comboBoxLanguages.Items.Add(new CultureListItem(new CultureInfo(0x7C04)));
                    else
                        comboBoxLanguages.Items.Add(new CultureListItem(CultureInfo.CreateSpecificCulture(cultureName)));
                }
                catch (ArgumentException)
                {
                    System.Diagnostics.Debug.WriteLine(cultureName + " is not a valid culture");
                }
            }

            int index = 0;
            for (int i = 0; i < comboBoxLanguages.Items.Count; i++)
            {
                var item = (CultureListItem)comboBoxLanguages.Items[i];
                if (item.Name == Configuration.Settings.Language.General.CultureName)
                    index = i;
            }
            comboBoxLanguages.SelectedIndex = index;

            Text = Configuration.Settings.Language.ChooseLanguage.Title;
            labelLanguage.Text = Configuration.Settings.Language.ChooseLanguage.Language;
            buttonOK.Text = Configuration.Settings.Language.General.Ok;
            buttonCancel.Text = Configuration.Settings.Language.General.Cancel;
            Utilities.FixLargeFonts(this, buttonOK);
        }

        private void ChangeLanguage_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                DialogResult = DialogResult.Cancel;
            }
            else if (e.Shift && e.Control && e.Alt && e.KeyCode == Keys.L)
            {
                Configuration.Settings.Language.Save(Path.Combine(Configuration.BaseDirectory, "LanguageMaster.xml"));
            }
            else if (e.KeyCode == Keys.F1)
            {
                Utilities.ShowHelp("#translate");
                e.SuppressKeyPress = true;
            }
        }

    }
}