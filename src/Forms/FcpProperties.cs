﻿using System;
using System.Windows.Forms;
using Nikse.SubtitleEdit.Logic;

namespace Nikse.SubtitleEdit.Forms
{
    public partial class FcpProperties : PositionAndSizeForm
    {
        public int FcpFontSize { get; set; }
        public string FcpFontName { get; set; }

        public FcpProperties()
        {
            InitializeComponent();
            textBoxFontName.Text = Configuration.Settings.SubtitleSettings.FcpFontName;
            try
            {
                numericUpDownFontSize.Value = Configuration.Settings.SubtitleSettings.FcpFontSize;
            }
            catch
            {
                numericUpDownFontSize.Value = 18;
            }
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            FcpFontName = textBoxFontName.Text;
            FcpFontSize = (int)numericUpDownFontSize.Value;
            DialogResult = DialogResult.OK;
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void FcpProperties_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
                DialogResult = DialogResult.Cancel;
        }
    }
}