﻿using Nikse.SubtitleEdit.Forms.Styles;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.SubtitleFormats;
using System;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.Forms
{
    public partial class ChooseStyle : Form
    {
        public string SelectedStyleName { get; set; }

        private Subtitle _subtitle;
        private bool _isSubStationAlpha;

        public ChooseStyle(Subtitle subtitle, bool isSubStationAlpha)
        {
            SelectedStyleName = null;
            InitializeComponent();
            _subtitle = subtitle;
            _isSubStationAlpha = isSubStationAlpha;

            var l = Configuration.Settings.Language.SubStationAlphaStyles;
            Text = l.ChooseStyle;
            listViewStyles.Columns[0].Text = l.Name;
            listViewStyles.Columns[1].Text = l.FontName;
            listViewStyles.Columns[2].Text = l.FontSize;
            listViewStyles.Columns[3].Text = l.UseCount;
            listViewStyles.Columns[4].Text = l.Primary;
            listViewStyles.Columns[5].Text = l.Outline;
            buttonOK.Text = Configuration.Settings.Language.General.Ok;
            buttonCancel.Text = Configuration.Settings.Language.General.Cancel;

            InitializeListView();
            Utilities.FixLargeFonts(this, buttonOK);
        }

        private void ChooseStyle_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
                DialogResult = DialogResult.Cancel;
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            if (listViewStyles.SelectedItems.Count > 0)
                SelectedStyleName = listViewStyles.SelectedItems[0].Text;
            DialogResult = DialogResult.OK;
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void InitializeListView()
        {
            var styles = AdvancedSubStationAlpha.GetStylesFromHeader(_subtitle.Header);
            listViewStyles.Items.Clear();
            foreach (string style in styles)
            {
                SsaStyle ssaStyle = AdvancedSubStationAlpha.GetSsaStyle(style, _subtitle.Header);
                SubStationAlphaStyles.AddStyle(listViewStyles, ssaStyle, _subtitle, _isSubStationAlpha);
            }
            if (listViewStyles.Items.Count > 0)
                listViewStyles.Items[0].Selected = true;
        }

    }
}