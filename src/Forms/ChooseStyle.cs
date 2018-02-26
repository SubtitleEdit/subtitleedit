﻿using Nikse.SubtitleEdit.Core;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Forms.Styles;
using Nikse.SubtitleEdit.Logic;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.Forms
{
    public partial class ChooseStyle : Form
    {
        public List<string> SelectedStyleNames { get; set; }

        private Subtitle _subtitle;
        private bool _isSubStationAlpha;

        public ChooseStyle(Subtitle subtitle, bool isSubStationAlpha)
        {
            SelectedStyleNames = new List<string>();
            UiUtil.PreInitialize(this);
            InitializeComponent();
            UiUtil.FixFonts(this);
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
            UiUtil.FixLargeFonts(this, buttonOK);
        }

        private void ChooseStyle_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
                DialogResult = DialogResult.Cancel;
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in listViewStyles.Items)
            {
                if (item.Checked)
                    SelectedStyleNames.Add(item.Text);
            }
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
