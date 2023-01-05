using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Logic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.Forms.Assa
{
    public sealed partial class ReplaceStyleWith : Form
    {
        private readonly SsaStyle _style;
        private readonly List<AssaStorageCategory> _storageCategories;
        private readonly List<SsaStyle> _currentFileStyles;
        private readonly Subtitle _subtitle;
        public Subtitle NewSubtitle { get; set; }
        public SsaStyle NewStorageStyle { get; set; }
        public SsaStyle NewFileStyle { get; set; }

        public ReplaceStyleWith(SsaStyle style, List<SsaStyle> currentFileStyles, List<AssaStorageCategory> storageCategories, Subtitle subtitle)
        {
            UiUtil.PreInitialize(this);
            InitializeComponent();
            UiUtil.FixFonts(this);

            _style = style;
            _currentFileStyles = currentFileStyles;
            _storageCategories = storageCategories;
            _subtitle = subtitle;

            var l = LanguageSettings.Current.SubStationAlphaStyles;
            Text = l.ReplaceWith.Trim('.');
            labelFrom.Text = LanguageSettings.Current.GoogleTranslate.From;
            labelStorageCategory.Text = LanguageSettings.Current.SubStationAlphaStylesCategoriesManager.Category;
            labelStyle.Text = LanguageSettings.Current.General.Style;
            comboBoxFrom.Items.Clear();
            comboBoxFrom.Items.Add(l.StyleCurrentFile);
            comboBoxFrom.Items.Add(l.StyleStorage);

            comboboxStorageCategories.Items.Clear();
            foreach (var category in storageCategories)
            {
                comboboxStorageCategories.Items.Add(category.Name);
            }

            comboBoxFrom.SelectedIndex = 0;

            buttonOK.Text = LanguageSettings.Current.General.Ok;
            buttonCancel.Text = LanguageSettings.Current.General.Cancel;
            UiUtil.FixLargeFonts(this, buttonOK);

            UpdateButtonOkEnabled();
        }

        private void UpdateButtonOkEnabled()
        {
            buttonOK.Enabled = comboBoxStyle.SelectedIndex >= 0;
        }

        private void comboBoxFrom_SelectedIndexChanged(object sender, EventArgs e)
        {
            comboBoxStyle.BeginUpdate();
            comboBoxStyle.Items.Clear();

            if (comboBoxFrom.SelectedIndex == 0)
            {
                labelStorageCategory.Enabled = false;
                comboboxStorageCategories.Enabled = false;

                foreach (var style in _currentFileStyles)
                {
                    if (style.Name != _style.Name)
                    {
                        comboBoxStyle.Items.Add(style.Name);
                    }
                }

                if (comboBoxStyle.Items.Count > 0)
                {
                    comboBoxStyle.SelectedIndex = 0;
                }
            }
            else if (comboBoxFrom.SelectedIndex == 1)
            {
                labelStorageCategory.Enabled = true;
                comboboxStorageCategories.Enabled = true;

                if (_storageCategories.Count > 0)
                {
                    comboboxStorageCategories.SelectedIndex = 0;
                }
            }
            else
            {
                labelStorageCategory.Enabled = true;
                comboboxStorageCategories.Enabled = true;
            }

            comboBoxStyle.EndUpdate();
        }

        private void comboboxStorageCategories_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!comboboxStorageCategories.Enabled || comboboxStorageCategories.SelectedIndex < 0)
            {
                return;
            }

            comboBoxStyle.BeginUpdate();
            comboBoxStyle.Items.Clear();

            var cat = _storageCategories[comboboxStorageCategories.SelectedIndex];
            foreach (var style in cat.Styles)
            {
                comboBoxStyle.Items.Add(style.Name);
            }

            comboBoxStyle.EndUpdate();

            if (comboBoxStyle.Items.Count > 0)
            {
                comboBoxStyle.SelectedIndex = 0;
            }

            UpdateButtonOkEnabled();
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            var idx = comboBoxStyle.SelectedIndex;
            if (idx < 0)
            {
                return;
            }

            NewSubtitle = new Subtitle(_subtitle);
            if (comboBoxFrom.SelectedIndex == 0) // current file styles
            {
                var style = _currentFileStyles[idx];
                foreach (var paragraph in NewSubtitle.Paragraphs)
                {
                    if (paragraph.Extra == _style.Name)
                    {
                        paragraph.Extra = style.Name;
                    }
                }

                NewFileStyle = style;
            }
            else if (comboBoxFrom.SelectedIndex == 1) // storage style
            {
                var category = _storageCategories[comboboxStorageCategories.SelectedIndex];
                var style = category.Styles[idx];
                foreach (var paragraph in NewSubtitle.Paragraphs)
                {
                    if (paragraph.Extra == _style.Name)
                    {
                        paragraph.Extra = style.Name;
                    }
                }

                NewStorageStyle = style;
                if (_currentFileStyles.Any(p => p.Name == NewStorageStyle.Name))
                {
                    var count = 2;
                    var doRepeat = true;
                    while (doRepeat)
                    {
                        NewStorageStyle.Name = LanguageSettings.Current.SubStationAlphaStyles.New + count;
                        doRepeat = _currentFileStyles.Any(p => p.Name == NewStorageStyle.Name);
                        count++;
                    }
                }
            }
            else
            {
                return;
            }

            DialogResult = DialogResult.OK;
        }

        private void comboBoxStyle_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateButtonOkEnabled();
        }
    }
}
