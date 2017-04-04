﻿using Nikse.SubtitleEdit.Core;
using Nikse.SubtitleEdit.Core.VobSub;
using Nikse.SubtitleEdit.Logic;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Nikse.SubtitleEdit.Forms.Ocr;

namespace Nikse.SubtitleEdit.Forms
{
    public sealed partial class DvdSubRipChooseLanguage : Form
    {
        private class SubListBoxItem
        {
            public string Name { get; set; }
            public VobSubMergedPack SubPack { get; set; }
            public override string ToString()
            {
                return Name;
            }

            public SubListBoxItem(string name, VobSubMergedPack subPack)
            {
                Name = name;
                SubPack = subPack;
            }
        }

        private List<VobSubMergedPack> _mergedVobSubPacks;
        private List<Color> _palette;
        private List<string> _languages;

        public List<VobSubMergedPack> SelectedVobSubMergedPacks { get; private set; }
        public string SelectedLanguageString { get; private set; }

        public DvdSubRipChooseLanguage()
        {
            InitializeComponent();
            Text = Configuration.Settings.Language.DvdSubRipChooseLanguage.Title;
            labelChooseLanguage.Text = Configuration.Settings.Language.DvdSubRipChooseLanguage.ChooseLanguageStreamId;
            buttonOK.Text = Configuration.Settings.Language.General.Ok;
            buttonCancel.Text = Configuration.Settings.Language.General.Cancel;
            buttonSaveAs.Text = Configuration.Settings.Language.Main.Menu.File.SaveAs;
            groupBoxImage.Text = Configuration.Settings.Language.DvdSubRipChooseLanguage.SubtitleImage;
            UiUtil.FixLargeFonts(this, buttonOK);
        }

        internal void Initialize(List<VobSubMergedPack> mergedVobSubPacks, List<Color> palette, List<string> languages, string selectedLanguage)
        {
            _mergedVobSubPacks = mergedVobSubPacks;
            _palette = palette;

            var uniqueLanguageStreamIds = new List<int>();
            foreach (var pack in mergedVobSubPacks)
            {
                if (!uniqueLanguageStreamIds.Contains(pack.StreamId))
                    uniqueLanguageStreamIds.Add(pack.StreamId);
            }

            comboBoxLanguages.Items.Clear();
            foreach (string languageName in languages)
            {
                if (uniqueLanguageStreamIds.Contains(GetLanguageIdFromString(languageName))) // only list languages actually found in vob
                {
                    comboBoxLanguages.Items.Add(languageName);
                    if (languageName == selectedLanguage)
                        comboBoxLanguages.SelectedIndex = comboBoxLanguages.Items.Count - 1;
                    uniqueLanguageStreamIds.Remove(GetLanguageIdFromString(languageName));
                }
            }

            foreach (var existingLanguageId in uniqueLanguageStreamIds) // subtitle tracks not supplied from IFO
            {
                // Use U+200E (LEFT-TO-RIGHT MARK) to support right-to-left scripts
                comboBoxLanguages.Items.Add(string.Format("{0} \x200E(0x{1:x})", Configuration.Settings.Language.DvdSubRipChooseLanguage.UnknownLanguage, existingLanguageId));
            }

            if (comboBoxLanguages.Items.Count > 0 && comboBoxLanguages.SelectedIndex < 0)
                comboBoxLanguages.SelectedIndex = 0;

            _languages = languages;
        }

        private static string ShowInSrtFormat(TimeSpan ts)
        {
            return string.Format("{0:00}:{1:00}:{2:00},{3:000}", ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds);
        }

        private void ListBox1SelectedIndexChanged(object sender, EventArgs e)
        {
            var x = listBox1.Items[listBox1.SelectedIndex] as SubListBoxItem;

            Bitmap bmp = x.SubPack.SubPicture.GetBitmap(_palette, Color.Transparent, Color.Wheat, Color.Black, Color.DarkGray, false);
            if (bmp.Width > pictureBoxImage.Width || bmp.Height > pictureBoxImage.Height)
            {
                float width = bmp.Width;
                float height = bmp.Height;
                while (width > pictureBoxImage.Width || height > pictureBoxImage.Height)
                {
                    width = width * 95 / 100;
                    height = height * 95 / 100;
                }

                var temp = new Bitmap((int)width, (int)height);
                using (var g = Graphics.FromImage(temp))
                    g.DrawImage(bmp, 0, 0, (int)width, (int)height);
                bmp = temp;
            }
            pictureBoxImage.Image = bmp;
            groupBoxImage.Text = string.Format(Configuration.Settings.Language.DvdSubRipChooseLanguage.SubtitleImageXofYAndWidthXHeight, listBox1.SelectedIndex + 1, listBox1.Items.Count, bmp.Width, bmp.Height);
        }

        private static int GetLanguageIdFromString(string currentLanguage)
        {
            currentLanguage = currentLanguage.Substring(currentLanguage.IndexOf("0x", StringComparison.Ordinal) + 2).TrimEnd(')');
            return Convert.ToInt32(currentLanguage, 16);
        }

        private void ComboBoxLanguagesSelectedIndexChanged(object sender, EventArgs e)
        {
            int chosenStreamId = GetLanguageIdFromString(comboBoxLanguages.Items[comboBoxLanguages.SelectedIndex].ToString());

            listBox1.Items.Clear();
            for (int i = 0; i < _mergedVobSubPacks.Count; i++)
            {
                var x = _mergedVobSubPacks[i];
                if (x.StreamId == chosenStreamId)
                {
                    string s = string.Format("#{0:000}: Stream-id=0X{1:X} - {2} --> {3}", i, x.StreamId, ShowInSrtFormat(x.StartTime), ShowInSrtFormat(x.EndTime));
                    listBox1.Items.Add(new SubListBoxItem(s, x));
                }
            }
            if (listBox1.Items.Count > 0)
                listBox1.SelectedIndex = 0;
        }

        private void ButtonOkClick(object sender, EventArgs e)
        {
            if (_languages != null && comboBoxLanguages.SelectedIndex >= 0 && comboBoxLanguages.SelectedIndex < _languages.Count)
                SelectedLanguageString = _languages[comboBoxLanguages.SelectedIndex];
            else
                SelectedLanguageString = null;

            SelectedVobSubMergedPacks = new List<VobSubMergedPack>();
            foreach (var x in listBox1.Items)
            {
                SelectedVobSubMergedPacks.Add((x as SubListBoxItem).SubPack);
            }
            DialogResult = DialogResult.OK;
        }

        private void ButtonCancelClick(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void DvdSubRipShowSubtitles_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                e.SuppressKeyPress = true;
                DialogResult = DialogResult.Cancel;
            }
        }

        private void buttonSaveAs_Click(object sender, EventArgs e)
        {
            if (_languages != null && comboBoxLanguages.SelectedIndex >= 0 && comboBoxLanguages.SelectedIndex < _languages.Count)
                SelectedLanguageString = _languages[comboBoxLanguages.SelectedIndex];
            else
                SelectedLanguageString = null;

            var subs = new List<VobSubMergedPack>();
            foreach (var x in listBox1.Items)
            {
                subs.Add((x as SubListBoxItem).SubPack);
            }

            using (var formSubOcr = new VobSubOcr())
            {
                formSubOcr.InitializeQuick(subs, _palette, Configuration.Settings.VobSubOcr, SelectedLanguageString);
                var subtitle = formSubOcr.ReadyVobSubRip();

                using (var exportBdnXmlPng = new ExportPngXml())
                {
                    exportBdnXmlPng.InitializeFromVobSubOcr(subtitle, new Core.SubtitleFormats.SubRip(), "VOBSUB", "DVD", formSubOcr, SelectedLanguageString);
                    exportBdnXmlPng.ShowDialog(this);
                }
            }
        }

    }
}
