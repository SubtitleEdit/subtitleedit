using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Logic;
using System;
using System.Collections.Generic;
using System.Windows.Forms;
using static Nikse.SubtitleEdit.Forms.BeautifyTimeCodes.BeautifyTimeCodes;

namespace Nikse.SubtitleEdit.Forms.BeautifyTimeCodes
{
    public partial class BeautifyTimeCodesUnfixableParagraphs : Form
    {
        private readonly List<UnfixableParagraphsPair> _paragraphs;
        private readonly Action<Paragraph> _selectParagraphAction;

        public BeautifyTimeCodesUnfixableParagraphs(List<UnfixableParagraphsPair> paragraphs, Action<Paragraph> selectParagraphAction)
        {
            _paragraphs = paragraphs;
            _selectParagraphAction = selectParagraphAction;

            UiUtil.PreInitialize(this);
            InitializeComponent();
            UiUtil.FixFonts(this);

            var language = LanguageSettings.Current.BeautifyTimeCodes;
            Text = language.UnfixableParagraphsTitle;
            labelInstructions.Text = language.UnfixableParagraphsInstructions;
            columnHeaderParagraphs.Text = language.UnfixableParagraphsColumnParagraphs;
            columnHeaderGap.Text = language.UnfixableParagraphsColumnGap;

            buttonClose.Text = LanguageSettings.Current.General.Close;
            UiUtil.FixLargeFonts(this, buttonClose);

            PopulateListView();
        }

        private void PopulateListView()
        {
            listView.BeginUpdate();
            foreach (var pair in _paragraphs)
            {
                listView.Items.Add(new ListViewItem(new[] {
                    String.Format(LanguageSettings.Current.BeautifyTimeCodes.UnfixableParagraphsColumnParagraphsFormat, pair.leftParagraph.Number, pair.rightParagraph.Number),
                    pair.gapFrames.ToString()
                }));
            }
            listView.EndUpdate();
        }

        private void BeautifyTimeCodesUnfixableParagraphs_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                this.Close();
            }
        }

        private void buttonClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void listView_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listView.SelectedIndices.Count > 0)
            {
                var index = listView.SelectedIndices[0];
                var pair = _paragraphs[index];

                _selectParagraphAction.Invoke(pair.rightParagraph);
            }
        }

        private void listView_Click(object sender, EventArgs e)
        {
            listView_SelectedIndexChanged(sender, e);
        }
    }
}
