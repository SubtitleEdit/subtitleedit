﻿using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Logic;
using System;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.Forms
{
    public sealed partial class ChangeCasing : PositionAndSizeForm
    {
        public ChangeCasing()
        {
            UiUtil.PreInitialize(this);
            InitializeComponent();
            UiUtil.FixFonts(this);

            var language = LanguageSettings.Current.ChangeCasing;
            Text = language.Title;
            groupBoxChangeCasing.Text = language.ChangeCasingTo;
            radioButtonNormal.Text = language.NormalCasing;
            checkBoxFixNames.Text = language.FixNamesCasing;
            radioButtonFixOnlyNames.Text = language.FixOnlyNamesCasing;
            checkBoxOnlyAllUpper.Text = language.OnlyChangeAllUppercaseLines;
            checkBoxProperCaseOnlyUpper.Text = LanguageSettings.Current.ChangeCasing.OnlyChangeAllUppercaseLines;
            radioButtonUppercase.Text = language.AllUppercase;
            radioButtonLowercase.Text = language.AllLowercase;
            radioButtonProperCase.Text = language.ProperCase;
            buttonOK.Text = LanguageSettings.Current.General.Ok;
            buttonCancel.Text = LanguageSettings.Current.General.Cancel;
            FixLargeFonts();

            if (Configuration.Settings.Tools.ChangeCasingChoice == "NamesOnly")
            {
                radioButtonFixOnlyNames.Checked = true;
            }
            else if (Configuration.Settings.Tools.ChangeCasingChoice == "Uppercase")
            {
                radioButtonUppercase.Checked = true;
            }
            else if (Configuration.Settings.Tools.ChangeCasingChoice == "Lowercase")
            {
                radioButtonLowercase.Checked = true;
            }
            else if (Configuration.Settings.Tools.ChangeCasingChoice == "ProperCase")
            {
                radioButtonProperCase.Checked = true;
            }
        }

        public int LinesChanged { get; private set; }

        public bool ChangeNamesToo => radioButtonFixOnlyNames.Checked || radioButtonNormal.Checked && checkBoxFixNames.Checked;

        public bool OnlyAllUpper => checkBoxOnlyAllUpper.Checked;

        private void FixLargeFonts()
        {
            if (radioButtonNormal.Left + radioButtonNormal.Width + 40 > Width)
            {
                Width = radioButtonNormal.Left + radioButtonNormal.Width + 40;
            }

            UiUtil.FixLargeFonts(this, buttonOK);
        }

        internal void FixCasing(Subtitle subtitle, string language)
        {
            var fixCasing = new FixCasing(language)
            {
                FixNormal = radioButtonNormal.Checked,
                FixNormalOnlyAllUppercase = checkBoxOnlyAllUpper.Checked,
                FixMakeUppercase = radioButtonUppercase.Checked,
                FixMakeLowercase = radioButtonLowercase.Checked,
                FixMakeProperCase = radioButtonProperCase.Checked,
                FixProperCaseOnlyAllUppercase = checkBoxProperCaseOnlyUpper.Checked,
                Format = subtitle.OriginalFormat,
            };
            fixCasing.Fix(subtitle);
            LinesChanged = fixCasing.NoOfLinesChanged;
        }

        private void FormChangeCasing_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                DialogResult = DialogResult.Cancel;
            }
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            if (radioButtonNormal.Checked)
            {
                Configuration.Settings.Tools.ChangeCasingChoice = "Normal";
            }
            else if (radioButtonFixOnlyNames.Checked)
            {
                Configuration.Settings.Tools.ChangeCasingChoice = "NamesOnly";
            }
            else if (radioButtonUppercase.Checked)
            {
                Configuration.Settings.Tools.ChangeCasingChoice = "Uppercase";
            }
            else if (radioButtonLowercase.Checked)
            {
                Configuration.Settings.Tools.ChangeCasingChoice = "Lowercase";
            }
            else if (radioButtonProperCase.Checked)
            {
                Configuration.Settings.Tools.ChangeCasingChoice = "ProperCase";
            }

            DialogResult = DialogResult.OK;
        }

        private void RadioButton_CheckedChanged(object sender, EventArgs e)
        {
            var isNormalCasing = sender == radioButtonNormal;
            checkBoxFixNames.Enabled = isNormalCasing;
            checkBoxOnlyAllUpper.Enabled = isNormalCasing;
        }
    }
}
