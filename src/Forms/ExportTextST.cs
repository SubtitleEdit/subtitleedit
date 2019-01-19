using Nikse.SubtitleEdit.Core;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Logic;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.Forms
{
    public partial class ExportTextST : Form
    {
        private Subtitle _subtitle;
        private TreeNode _rootNode;
        private TreeNode _palettesNode;
        private string _fileName;
        private TextST _textST;
        private TextST.Palette _currentPalette;
        private TextST.RegionStyle _currentRegionStyle;
        private TextST.UserStyle _currentUserStyle;
        private TextST.SubtitleRegion _currentSubtitleRegion;
        private TextST.SubtitleRegionContentChangeFontStyle _currentSubtitleFontStyle;
        private TextST.SubtitleRegionContentChangeFontSize _currentSubtitleFontSize;
        private TextST.SubtitleRegionContentText _currentSubtitleText;
        private TextST.SubtitleRegionContentChangeFontColor _currentSubtitleFontColor;
        private TextST.SubtitleRegionContentChangeFontSet _currentSubtitleFontSet;
        private TreeNode _currentNode;

        private void SetGroupBoxProperties(GroupBox gb)
        {
            gb.Left = groupBoxPropertiesRoot.Left;
            gb.Top = groupBoxPropertiesRoot.Top;
            gb.Size = groupBoxPropertiesRoot.Size;
            gb.Anchor = groupBoxPropertiesRoot.Anchor;
        }

        public ExportTextST(Subtitle subtitle)
        {
            UiUtil.PreInitialize(this);
            InitializeComponent();
            UiUtil.FixFonts(this);

            SetGroupBoxProperties(groupBoxPropertiesPalette);
            SetGroupBoxProperties(groupBoxPropertiesRegionStyle);
            SetGroupBoxProperties(groupBoxPropertiesUserStyle);
            SetGroupBoxProperties(groupBoxPresentationSegmentRegion);
            SetGroupBoxProperties(groupBoxFontStyle);
            SetGroupBoxProperties(groupBoxChangeFontSize);
            SetGroupBoxProperties(groupBoxSubtitleText);
            SetGroupBoxProperties(groupBoxChangeFontColor);
            SetGroupBoxProperties(groupBoxFontSet);

            _subtitle = subtitle;

            _textST = new TextST
            {
                StyleSegment = TextST.DialogStyleSegment.DefaultDialogStyleSegment,
                PresentationSegments = new List<TextST.DialogPresentationSegment>(),
            };
            _textST.StyleSegment.NumberOfDialogPresentationSegments = _subtitle.Paragraphs.Count;
            foreach (var paragraph in _subtitle.Paragraphs)
            {
                var dps = new TextST.DialogPresentationSegment(paragraph, _textST.StyleSegment.RegionStyles[0]);
                _textST.PresentationSegments.Add(dps);
            }

            UpdateTreeview();
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void ButtonImportClick(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog(this) == DialogResult.OK)
            {
                _textST = new TextST();
                if (!_textST.IsMine(null, openFileDialog1.FileName))
                {
                    MessageBox.Show("Not a valid TextST file");
                    return;
                }
                _subtitle = new Subtitle();
                _fileName = openFileDialog1.FileName;
                _textST.LoadSubtitle(_subtitle, null, _fileName);
                groupBoxTextST.Text = "TextST structure: " + Path.GetFileName(_fileName);
                UpdateTreeview();
            }
        }

        private void UpdateTreeview()
        {
            treeView1.Nodes.Clear();
            treeView1.BeginUpdate();
            _rootNode = new TreeNode("TextST");
            treeView1.Nodes.Add(_rootNode);
            if (_textST.StyleSegment != null)
            {
                var styleNode = new TreeNode("Style segment");
                _rootNode.Nodes.Add(styleNode);

                var regionStylesNode = new TreeNode(string.Format("Region styles ({0})", _textST.StyleSegment.RegionStyles.Count));
                styleNode.Nodes.Add(regionStylesNode);
                foreach (TextST.RegionStyle regionStyle in _textST.StyleSegment.RegionStyles)
                {
                    var regionStyleNode = new TreeNode("Region style") { Tag = regionStyle };
                    regionStylesNode.Nodes.Add(regionStyleNode);
                }

                var userStylesNode = new TreeNode(string.Format("User styles ({0})", _textST.StyleSegment.UserStyles.Count));
                styleNode.Nodes.Add(userStylesNode);
                foreach (TextST.UserStyle userStyle in _textST.StyleSegment.UserStyles)
                {
                    var regionStyleNode = new TreeNode("User style") { Tag = userStyle };
                    userStylesNode.Nodes.Add(regionStyleNode);
                }

                _palettesNode = new TreeNode(string.Format("Palettes ({0})", _textST.StyleSegment.Palettes.Count)) { Tag = _textST.StyleSegment.Palettes };
                styleNode.Nodes.Add(_palettesNode);
                foreach (TextST.Palette palette in _textST.StyleSegment.Palettes)
                {
                    var paletteNode = new TreeNode("Palette " + palette.PaletteEntryId) { Tag = palette };
                    _palettesNode.Nodes.Add(paletteNode);
                }
            }

            if (_textST.PresentationSegments != null)
            {
                var presentationSegmentsNode = new TreeNode(string.Format("Presentation segments ({0})", _textST.PresentationSegments.Count));
                _rootNode.Nodes.Add(presentationSegmentsNode);
                int count = 0;
                foreach (var segment in _textST.PresentationSegments)
                {
                    count++;
                    var presentationSegmentNode = new TreeNode(string.Format("Segment {0}: {1} -- > {2}", count,
                        new TimeCode(segment.StartPtsMilliseconds), new TimeCode(segment.EndPtsMilliseconds)))
                    { Tag = segment };
                    presentationSegmentsNode.Nodes.Add(presentationSegmentNode);

                    foreach (var subtitleRegion in segment.Regions)
                    {
                        var subtitleRegionNode = new TreeNode("Subtitle region") { Tag = subtitleRegion };
                        presentationSegmentNode.Nodes.Add(subtitleRegionNode);

                        foreach (var content in subtitleRegion.Content)
                        {
                            if (content is TextST.SubtitleRegionContentText)
                            {
                                var textContent = content as TextST.SubtitleRegionContentText;
                                var contentNode = new TreeNode(content.Name + ": " + textContent.Text) { Tag = content };
                                subtitleRegionNode.Nodes.Add(contentNode);
                            }
                            else if (content is TextST.SubtitleRegionContentChangeFontStyle)
                            {
                                var fontStyleContent = content as TextST.SubtitleRegionContentChangeFontStyle;
                                var contentNode = new TreeNode(content.Name + ": " + GetNameFromFrontStyle(fontStyleContent.FontStyle)) { Tag = content };
                                subtitleRegionNode.Nodes.Add(contentNode);
                            }
                            else
                            {
                                var contentNode = new TreeNode(content.Name) { Tag = content };
                                subtitleRegionNode.Nodes.Add(contentNode);
                            }
                        }
                    }
                }
            }
            treeView1.ExpandAll();
            treeView1.EndUpdate();
            treeView1.SelectedNode = _rootNode;
        }

        private string GetNameFromFrontStyle(int fontStyle)
        {
            switch (fontStyle)
            {
                case 0:
                    return "Normal";
                case 1:
                    return "Bold";
                case 2:
                    return "Italic";
                case 3:
                    return "Bold and italic";
                case 4:
                    return "Outline-bordered";
                case 5:
                    return "Bold and outline-bordered";
                case 6:
                    return "Italic and outline-bordered";
                case 7:
                    return "Bold and italic and outline-bordered";
                default:
                    return "Unknown";
            }
        }

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            groupBoxPropertiesRoot.Visible = false;
            groupBoxPropertiesPalette.Visible = false;
            groupBoxPropertiesRegionStyle.Visible = false;
            groupBoxPropertiesUserStyle.Visible = false;
            groupBoxPresentationSegmentRegion.Visible = false;
            groupBoxFontStyle.Visible = false;
            groupBoxChangeFontSize.Visible = false;
            groupBoxSubtitleText.Visible = false;
            groupBoxChangeFontColor.Visible = false;
            groupBoxFontSet.Visible = false;
            if (e.Node != null && _textST != null)
            {
                _currentNode = e.Node;
                if (e.Node == _rootNode)
                {
                    groupBoxPropertiesRoot.Visible = true;
                    textBoxRoot.Text = "Number of region styles: " + _textST.StyleSegment.RegionStyles.Count + Environment.NewLine +
                                       "Number of user styles: " + _textST.StyleSegment.UserStyles.Count + Environment.NewLine +
                                       "Number of palettes: " + _textST.StyleSegment.Palettes.Count + Environment.NewLine +
                                       "Number of subtitles: " + _textST.StyleSegment.NumberOfDialogPresentationSegments + Environment.NewLine;
                }
                else if (e.Node.Tag is TextST.Palette)
                {
                    groupBoxPropertiesPalette.Visible = true;
                    _currentPalette = e.Node.Tag as TextST.Palette;
                    numericUpDownPaletteEntry.Value = _currentPalette.PaletteEntryId;
                    numericUpDownPaletteY.Value = _currentPalette.Y;
                    numericUpDownPaletteCr.Value = _currentPalette.Cr;
                    numericUpDownPaletteCb.Value = _currentPalette.Cb;
                    numericUpDownPaletteOpacity.Value = _currentPalette.T;
                    panelPaletteColor.BackColor = Color.FromArgb(255, _currentPalette.Color);
                }
                else if (e.Node.Tag is TextST.RegionStyle)
                {
                    groupBoxPropertiesRegionStyle.Visible = true;
                    _currentRegionStyle = e.Node.Tag as TextST.RegionStyle;
                    numericUpDownRegionStyleId.Value = _currentRegionStyle.RegionStyleId;
                    numericUpDownRegionStyleHPos.Value = _currentRegionStyle.RegionHorizontalPosition;
                    numericUpDownRegionStyleVPos.Value = _currentRegionStyle.RegionVerticalPosition;
                    numericUpDownRegionStyleWidth.Value = _currentRegionStyle.RegionWidth;
                    numericUpDownRegionStyleHeight.Value = _currentRegionStyle.RegionHeight;
                    numericUpDownRegionStylePaletteEntryId.Value = _currentRegionStyle.RegionBgPaletteEntryIdRef;
                    numericUpDownRegionStyleTBHorPos.Value = _currentRegionStyle.TextBoxHorizontalPosition;
                    numericUpDownRegionStyleTBVerPos.Value = _currentRegionStyle.RegionVerticalPosition;
                    numericUpDownRegionStyleTBWidth.Value = _currentRegionStyle.TextBoxWidth;
                    numericUpDownRegionStyleTBHeight.Value = _currentRegionStyle.TextBoxHeight;
                    numericUpDownRegionStyleTextFlow.Value = _currentRegionStyle.TextFlow;
                    numericUpDownRegionStyleTextHorAlign.Value = _currentRegionStyle.TextHorizontalAlignment;
                    numericUpDownRegionStyleTextVerAlign.Value = _currentRegionStyle.TextVerticalAlignment;
                    numericUpDownRegionStyleLineSpace.Value = _currentRegionStyle.LineSpace;
                    numericUpDownRegionStyleFontIdRef.Value = _currentRegionStyle.FontIdRef;
                    numericUpDownRegionStyleFontStyle.Value = _currentRegionStyle.FontStyle;
                    numericUpDownRegionStyleFontSize.Value = _currentRegionStyle.FontSize;
                    numericUpDownRegionStyleFontPaletteId.Value = _currentRegionStyle.FontPaletteEntryIdRef;
                    numericUpDownRegionStyleFontOutlinePaletteId.Value = _currentRegionStyle.FontOutlinePaletteEntryIdRef;
                    numericUpDownRegionStyleFontOutlineThickness.Value = _currentRegionStyle.FontOutlineThickness;
                }
                else if (e.Node.Tag is TextST.UserStyle)
                {
                    groupBoxPropertiesUserStyle.Visible = true;
                    _currentUserStyle = e.Node.Tag as TextST.UserStyle;
                    numericUpDownUserStyleId.Value = _currentUserStyle.UserStyleId;
                    numericUpDownUserStyleHorPosDir.Value = _currentUserStyle.RegionHorizontalPositionDirection;
                    numericUpDownUserStyleHorPosDelta.Value = _currentUserStyle.RegionHorizontalPositionDelta;
                    numericUpDownUserStyleVerPosDir.Value = _currentUserStyle.RegionVerticalPositionDirection;
                    numericUpDownUserStyleVerPosDelta.Value = _currentUserStyle.RegionVerticalPositionDelta;
                    numericUpDownUserStyleFontSizeIncDec.Value = _currentUserStyle.FontSizeIncDec;
                    numericUpDownUserStyleFontSizeDelta.Value = _currentUserStyle.FontSizeDelta;
                    numericUpDownUserStyleTBHorPosDir.Value = _currentUserStyle.TextBoxHorizontalPositionDirection;
                    numericUpDownUserStyleTBHorPosDelta.Value = _currentUserStyle.TextBoxHorizontalPositionDelta;
                    numericUpDownUserStyleTBVerPosDir.Value = _currentUserStyle.TextBoxVerticalPositionDirection;
                    numericUpDownUserStyleTBVerPosDelta.Value = _currentUserStyle.TextBoxVerticalPositionDelta;
                    numericUpDownUserStyleTBWidthIncDec.Value = _currentUserStyle.TextBoxWidthIncDec;
                    numericUpDownUserStyleTBWidthDelta.Value = _currentUserStyle.TextBoxWidthDelta;
                    numericUpDownUserStyleTBHeightIncDec.Value = _currentUserStyle.TextBoxHeightIncDec;
                    numericUpDownUserStyleTBHeightDelta.Value = _currentUserStyle.TextBoxHeightDelta;
                    numericUpDownUserStyleLineSpaceIncDec.Value = _currentUserStyle.LineSpaceIncDec;
                    numericUpDownUserStyleLineSpaceDelta.Value = _currentUserStyle.LineSpaceDelta;
                }
                else if (e.Node.Tag is TextST.SubtitleRegion)
                {
                    groupBoxPresentationSegmentRegion.Visible = true;
                    _currentSubtitleRegion = e.Node.Tag as TextST.SubtitleRegion;
                    checkBoxSubRegionContinuous.Checked = _currentSubtitleRegion.ContinuousPresentation;
                    checkBoxSubRegionForced.Checked = _currentSubtitleRegion.Forced;
                    numericUpDownSubRegionStyleIdRef.Value = _currentSubtitleRegion.RegionStyleId;
                }
                else if (e.Node.Tag is TextST.SubtitleRegionContentChangeFontStyle)
                {
                    groupBoxFontStyle.Visible = true;
                    _currentSubtitleFontStyle = e.Node.Tag as TextST.SubtitleRegionContentChangeFontStyle;
                    comboBoxChangeFontStyleFontStyle.SelectedIndex = _currentSubtitleFontStyle.FontStyle;
                    numericUpDownChangeFontStyleOutlinePaletteId.Value = _currentSubtitleFontStyle.FontOutlinePaletteId;
                    comboBoxChangeFontStyleOutlineThickness.SelectedIndex = _currentSubtitleFontStyle.FontOutlineThickness - 1;
                }
                else if (e.Node.Tag is TextST.SubtitleRegionContentChangeFontSize)
                {
                    groupBoxChangeFontSize.Visible = true;
                    _currentSubtitleFontSize = e.Node.Tag as TextST.SubtitleRegionContentChangeFontSize;
                    numericUpDownChangeFontSize.Value = _currentSubtitleFontSize.FontSize;
                }
                else if (e.Node.Tag is TextST.SubtitleRegionContentText)
                {
                    groupBoxSubtitleText.Visible = true;
                    _currentSubtitleText = e.Node.Tag as TextST.SubtitleRegionContentText;
                    textBoxSubtitleText.Text = _currentSubtitleText.Text;
                }
                else if (e.Node.Tag is TextST.SubtitleRegionContentChangeFontColor)
                {
                    groupBoxChangeFontColor.Visible = true;
                    _currentSubtitleFontColor = e.Node.Tag as TextST.SubtitleRegionContentChangeFontColor;
                    numericUpDownChangeFontColor.Value = _currentSubtitleFontColor.FontPaletteId;
                }
                else if (e.Node.Tag is TextST.SubtitleRegionContentChangeFontSet)
                {
                    groupBoxFontSet.Visible = true;
                    _currentSubtitleFontSet = e.Node.Tag as TextST.SubtitleRegionContentChangeFontSet;
                    numericUpDownFontSetFontId.Value = _currentSubtitleFontSet.FontId;
                }
            }
        }

        private void buttonColor_Click(object sender, EventArgs e)
        {
            using (var colorChooser = new ColorChooser { Color = panelPaletteColor.BackColor, ShowAlpha = true })
            {
                if (colorChooser.ShowDialog() == DialogResult.OK)
                {
                    panelPaletteColor.BackColor = colorChooser.Color;
                }
            }
        }

        private void buttonSaveAsPes_Click(object sender, EventArgs e)
        {
            saveFileDialog1.DefaultExt = ".textst";
            saveFileDialog1.FileName = string.Empty;
            saveFileDialog1.Filter = "TextST files|*.textst";
            if (saveFileDialog1.ShowDialog(this) == DialogResult.OK)
            {
                using (var fs = new FileStream(saveFileDialog1.FileName, FileMode.Create))
                {
                    _textST.StyleSegment.WriteToStream(fs, _subtitle.Paragraphs.Count);
                    foreach (var presentationSegment in _textST.PresentationSegments)
                    {
                        presentationSegment.WriteToStream(fs);
                    }
                }
                MessageBox.Show("TextST PES packets saved as " + saveFileDialog1.FileName);
            }
        }

        private void buttonSaveAsM2ts_Click(object sender, EventArgs e)
        {
            //saveFileDialog1.DefaultExt = ".m2ts";
            //saveFileDialog1.FileName = string.Empty;
            //saveFileDialog1.Filter = "m2ts files|*.m2ts";
            //if (saveFileDialog1.ShowDialog(this) == DialogResult.OK)
            //{
            //    using (var fs = new FileStream(saveFileDialog1.FileName, FileMode.Create))
            //    {

            //    }
            //    MessageBox.Show("TextST M2TS file saved as " + saveFileDialog1.FileName);
            //}
        }

        private int GetIntFromNumericUpDown(object sender)
        {
            var numericUpDown = sender as NumericUpDown;
            if (numericUpDown != null)
            {
                return (int)numericUpDown.Value;
            }

            return 0;
        }

        private void numericUpDownRegionStyleId_ValueChanged(object sender, EventArgs e)
        {
            _currentRegionStyle.RegionStyleId = GetIntFromNumericUpDown(sender);
        }

        private void numericUpDownRegionStyleHPos_ValueChanged(object sender, EventArgs e)
        {
            _currentRegionStyle.RegionHorizontalPosition = GetIntFromNumericUpDown(sender);
        }

        private void numericUpDownRegionStyleVPos_ValueChanged(object sender, EventArgs e)
        {
            _currentRegionStyle.RegionVerticalPosition = GetIntFromNumericUpDown(sender);
        }

        private void numericUpDownRegionStyleWidth_ValueChanged(object sender, EventArgs e)
        {
            _currentRegionStyle.RegionWidth = GetIntFromNumericUpDown(sender);
        }

        private void numericUpDownRegionStyleHeight_ValueChanged(object sender, EventArgs e)
        {
            _currentRegionStyle.RegionHeight = GetIntFromNumericUpDown(sender);
        }

        private void numericUpDownRegionStylePaletteEntryId_ValueChanged(object sender, EventArgs e)
        {
            _currentRegionStyle.RegionBgPaletteEntryIdRef = GetIntFromNumericUpDown(sender);
        }

        private void numericUpDownRegionStyleTBHorPos_ValueChanged(object sender, EventArgs e)
        {
            _currentRegionStyle.TextBoxHorizontalPosition = GetIntFromNumericUpDown(sender);
        }

        private void numericUpDownRegionStyleTBVerPos_ValueChanged(object sender, EventArgs e)
        {
            _currentRegionStyle.TextBoxVerticalPosition = GetIntFromNumericUpDown(sender);
        }

        private void numericUpDownRegionStyleTBWidth_ValueChanged(object sender, EventArgs e)
        {
            _currentRegionStyle.TextBoxWidth = GetIntFromNumericUpDown(sender);
        }

        private void numericUpDownRegionStyleTBHeight_ValueChanged(object sender, EventArgs e)
        {
            _currentRegionStyle.TextBoxHeight = GetIntFromNumericUpDown(sender);
        }

        private void numericUpDownRegionStyleTextFlow_ValueChanged(object sender, EventArgs e)
        {
            _currentRegionStyle.TextFlow = GetIntFromNumericUpDown(sender);
        }

        private void numericUpDownRegionStyleTextHorAlign_ValueChanged(object sender, EventArgs e)
        {
            _currentRegionStyle.TextHorizontalAlignment = GetIntFromNumericUpDown(sender);
        }

        private void numericUpDownRegionStyleTextVerAlign_ValueChanged(object sender, EventArgs e)
        {
            _currentRegionStyle.TextVerticalAlignment = GetIntFromNumericUpDown(sender);
        }

        private void numericUpDownRegionStyleLineSpace_ValueChanged(object sender, EventArgs e)
        {
            _currentRegionStyle.LineSpace = GetIntFromNumericUpDown(sender);
        }

        private void numericUpDownRegionStyleFontIdRef_ValueChanged(object sender, EventArgs e)
        {
            _currentRegionStyle.FontIdRef = GetIntFromNumericUpDown(sender);
        }

        private void numericUpDownRegionStyleFontStyle_ValueChanged(object sender, EventArgs e)
        {
            _currentRegionStyle.FontStyle = GetIntFromNumericUpDown(sender);
        }

        private void numericUpDownRegionStyleFontSize_ValueChanged(object sender, EventArgs e)
        {
            _currentRegionStyle.FontSize = GetIntFromNumericUpDown(sender);
        }

        private void numericUpDownRegionStyleFontPaletteId_ValueChanged(object sender, EventArgs e)
        {
            _currentRegionStyle.FontPaletteEntryIdRef = GetIntFromNumericUpDown(sender);
        }

        private void numericUpDownRegionStyleFontOutlinePaletteId_ValueChanged(object sender, EventArgs e)
        {
            _currentRegionStyle.FontOutlinePaletteEntryIdRef = GetIntFromNumericUpDown(sender);
        }

        private void numericUpDownRegionStyleFontOutlineThickness_ValueChanged(object sender, EventArgs e)
        {
            _currentRegionStyle.FontOutlineThickness = GetIntFromNumericUpDown(sender);
        }

        private void numericUpDownUserStyleId_ValueChanged(object sender, EventArgs e)
        {
            _currentUserStyle.UserStyleId = GetIntFromNumericUpDown(sender);
        }

        private void numericUpDownUserStyleHorPosDir_ValueChanged(object sender, EventArgs e)
        {
            _currentUserStyle.RegionHorizontalPositionDirection = GetIntFromNumericUpDown(sender);
        }

        private void numericUpDownUserStyleHorPosDelta_ValueChanged(object sender, EventArgs e)
        {
            _currentUserStyle.RegionHorizontalPositionDelta = GetIntFromNumericUpDown(sender);
        }

        private void numericUpDownUserStyleVerPosDir_ValueChanged(object sender, EventArgs e)
        {
            _currentUserStyle.RegionVerticalPositionDirection = GetIntFromNumericUpDown(sender);
        }

        private void numericUpDownUserStyleVerPosDelta_ValueChanged(object sender, EventArgs e)
        {
            _currentUserStyle.RegionVerticalPositionDelta = GetIntFromNumericUpDown(sender);
        }

        private void numericUpDownUserStyleFontSizeIncDec_ValueChanged(object sender, EventArgs e)
        {
            _currentUserStyle.FontSizeIncDec = GetIntFromNumericUpDown(sender);
        }

        private void numericUpDownUserStyleFontSizeDelta_ValueChanged(object sender, EventArgs e)
        {
            _currentUserStyle.FontSizeDelta = GetIntFromNumericUpDown(sender);
        }

        private void numericUpDownUserStyleTBHorPosDir_ValueChanged(object sender, EventArgs e)
        {
            _currentUserStyle.TextBoxHorizontalPositionDirection = GetIntFromNumericUpDown(sender);
        }

        private void numericUpDownUserStyleTBHorPosDelta_ValueChanged(object sender, EventArgs e)
        {
            _currentUserStyle.TextBoxHorizontalPositionDelta = GetIntFromNumericUpDown(sender);
        }

        private void numericUpDownUserStyleTBVerPosDir_ValueChanged(object sender, EventArgs e)
        {
            _currentUserStyle.TextBoxVerticalPositionDirection = GetIntFromNumericUpDown(sender);
        }

        private void numericUpDownUserStyleTBVerPosDelta_ValueChanged(object sender, EventArgs e)
        {
            _currentUserStyle.TextBoxVerticalPositionDelta = GetIntFromNumericUpDown(sender);
        }

        private void numericUpDownUserStyleTBWidthIncDec_ValueChanged(object sender, EventArgs e)
        {
            _currentUserStyle.TextBoxWidthIncDec = GetIntFromNumericUpDown(sender);
        }

        private void numericUpDownUserStyleTBWidthDelta_ValueChanged(object sender, EventArgs e)
        {
            _currentUserStyle.TextBoxWidthDelta = GetIntFromNumericUpDown(sender);
        }

        private void numericUpDownUserStyleTBHeightIncDec_ValueChanged(object sender, EventArgs e)
        {
            _currentUserStyle.TextBoxHeightIncDec = GetIntFromNumericUpDown(sender);
        }

        private void numericUpDownUserStyleTBHeightDelta_ValueChanged(object sender, EventArgs e)
        {
            _currentUserStyle.TextBoxHeightDelta = GetIntFromNumericUpDown(sender);
        }

        private void numericUpDownUserStyleLineSpaceIncDec_ValueChanged(object sender, EventArgs e)
        {
            _currentUserStyle.LineSpaceIncDec = GetIntFromNumericUpDown(sender);
        }

        private void numericUpDownUserStyleLineSpaceDelta_ValueChanged(object sender, EventArgs e)
        {
            _currentUserStyle.LineSpaceDelta = GetIntFromNumericUpDown(sender);
        }

        private void numericUpDownPaletteEntry_ValueChanged(object sender, EventArgs e)
        {
            _currentPalette.PaletteEntryId = GetIntFromNumericUpDown(sender);
        }

        private void numericUpDownPaletteY_ValueChanged(object sender, EventArgs e)
        {
            _currentPalette.Y = GetIntFromNumericUpDown(sender);
        }

        private void numericUpDownPaletteCr_ValueChanged(object sender, EventArgs e)
        {
            _currentPalette.Cr = GetIntFromNumericUpDown(sender);
        }

        private void numericUpDownPaletteCb_ValueChanged(object sender, EventArgs e)
        {
            _currentPalette.Cb = GetIntFromNumericUpDown(sender);
        }

        private void numericUpDownPaletteOpacity_ValueChanged(object sender, EventArgs e)
        {
            _currentPalette.T = GetIntFromNumericUpDown(sender);
        }

        private void checkBoxSubRegionContinuous_CheckedChanged(object sender, EventArgs e)
        {
            _currentSubtitleRegion.ContinuousPresentation = (sender as CheckBox).Checked;
        }

        private void checkBoxSubRegionForced_CheckedChanged(object sender, EventArgs e)
        {
            _currentSubtitleRegion.Forced = (sender as CheckBox).Checked;
        }

        private void numericUpDownSubRegionStyleIdRef_ValueChanged(object sender, EventArgs e)
        {
            _currentSubtitleRegion.RegionStyleId = GetIntFromNumericUpDown(sender);
        }

        private void treeView1_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                var p = new Point(e.X, e.Y);
                TreeNode node = treeView1.GetNodeAt(p);
                if (node != null)
                {
                    treeView1.SelectedNode = node;
                    if (node.Tag is List<TextST.Palette>)
                    {
                        contextMenuStripAddPalette.Show(treeView1, p);
                    }
                    else if (node.Tag is TextST.SubtitleRegion)
                    {
                        contextMenuStripAddSubtitleContentFromSubRegion.Show(treeView1, p);
                    }
                    else if (node.Tag is TextST.SubtitleRegionContent)
                    {
                        contextMenuStripAddSubtitleContent.Show(treeView1, p);
                    }
                    else if (node.Tag is TextST.RegionStyle)
                    {
                        contextMenuStripRegionStyle.Show(treeView1, p);
                    }
                }
            }
        }

        private void addPaletteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_textST.StyleSegment.Palettes.Count < 254)
            {
                int id = 1;
                while (_textST.StyleSegment.Palettes.Any(p => p.PaletteEntryId == id))
                {
                    id++;
                }

                var palette = new TextST.Palette
                {
                    PaletteEntryId = id,
                    Y = 235,
                    Cr = 128,
                    Cb = 128,
                    T = 255
                };

                _textST.StyleSegment.Palettes.Add(palette);

                var paletteNode = new TreeNode("Palette " + id) { Tag = palette };
                _palettesNode.Nodes.Add(paletteNode);
                treeView1.SelectedNode = paletteNode;
            }
        }

        private void textBoxSubtitleText_TextChanged(object sender, EventArgs e)
        {
            _currentSubtitleText.Text = (sender as TextBox).Text;
            _currentNode.Text = "Text: " + _currentSubtitleText.Text;
        }

        private void numericUpDownChangeFontColor_ValueChanged(object sender, EventArgs e)
        {
            _currentSubtitleFontColor.FontPaletteId = GetIntFromNumericUpDown(sender);
        }

        private List<TextST.SubtitleRegionContent> GetContentList(TextST.SubtitleRegionContent content)
        {
            foreach (var presentationSegment in _textST.PresentationSegments)
            {
                foreach (var region in presentationSegment.Regions)
                {
                    if (region.Content.Contains(content))
                    {
                        return region.Content;
                    }
                }
            }
            return null;
        }

        private void AddSubtitleContent(TextST.SubtitleRegionContent newContent, string title)
        {
            var content = _currentNode.Tag as TextST.SubtitleRegionContent;
            if (content != null)
            {
                var list = GetContentList(content);
                if (list != null)
                {
                    var newNode = new TreeNode(title) { Tag = newContent };
                    _currentNode.Parent.Nodes.Insert(_currentNode.Parent.Nodes.IndexOf(_currentNode) + 1, newNode);
                    int index = list.IndexOf(content);
                    list.Insert(index + 1, newContent);
                    treeView1.SelectedNode = newNode;
                }
            }
        }

        private void addLineBreakToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var newContent = new TextST.SubtitleRegionContentLineBreak();
            AddSubtitleContent(newContent, newContent.Name);
        }

        private void addEndOfInlineStyleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var newContent = new TextST.SubtitleRegionContentEndOfInlineStyle();
            AddSubtitleContent(newContent, newContent.Name);
        }

        private void addFontSizeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var newContent = new TextST.SubtitleRegionContentChangeFontSize { FontSize = 45 };
            AddSubtitleContent(newContent, newContent.Name);
        }

        private void addFontSetToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var newContent = new TextST.SubtitleRegionContentChangeFontSet();
            AddSubtitleContent(newContent, newContent.Name);
        }

        private void addFontStyleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var newContent = new TextST.SubtitleRegionContentChangeFontStyle { FontStyle = 0, FontOutlinePaletteId = 0, FontOutlineThickness = 2 };
            AddSubtitleContent(newContent, newContent.Name);
        }

        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            var newContent = new TextST.SubtitleRegionContentText();
            AddSubtitleContent(newContent, newContent.Name + ": ");
        }

        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var content = _currentNode.Tag as TextST.SubtitleRegionContent;
            if (content != null)
            {
                var list = GetContentList(content);
                if (list != null)
                {
                    list.Remove(content);
                    _currentNode.Parent.Nodes.Remove(_currentNode);
                }
            }
        }

        private void AddSubtitleContentFromRoot(TextST.SubtitleRegionContent newContent, string title)
        {
            var region = _currentNode.Tag as TextST.SubtitleRegion;
            if (region != null)
            {
                var list = region.Content;
                if (list != null)
                {
                    var newNode = new TreeNode(title) { Tag = newContent };
                    _currentNode.Nodes.Insert(0, newNode);
                    region.Content.Insert(0, newContent);
                    treeView1.SelectedNode = newNode;
                }
            }
        }

        private void toolStripMenuItemRegionAddLineBreak_Click(object sender, EventArgs e)
        {
            var newContent = new TextST.SubtitleRegionContentLineBreak();
            AddSubtitleContentFromRoot(newContent, newContent.Name);
        }

        private void toolStripMenuItemRegionAddText_Click(object sender, EventArgs e)
        {
            var newContent = new TextST.SubtitleRegionContentText();
            AddSubtitleContentFromRoot(newContent, newContent.Name + ": ");
        }

        private void toolStripMenuItemRegionAddFontStyle_Click(object sender, EventArgs e)
        {
            var newContent = new TextST.SubtitleRegionContentChangeFontStyle { FontStyle = 0, FontOutlinePaletteId = 0, FontOutlineThickness = 2 };
            AddSubtitleContentFromRoot(newContent, newContent.Name);
        }

        private void toolStripMenuItemRegionAddFontSize_Click(object sender, EventArgs e)
        {
            var newContent = new TextST.SubtitleRegionContentChangeFontSize { FontSize = 45 };
            AddSubtitleContentFromRoot(newContent, newContent.Name);
        }

        private void toolStripMenuItemRegionAddInlineStyle_Click(object sender, EventArgs e)
        {
            var newContent = new TextST.SubtitleRegionContentEndOfInlineStyle();
            AddSubtitleContentFromRoot(newContent, newContent.Name);
        }

        private void numericUpDownFontSetFontId_ValueChanged(object sender, EventArgs e)
        {
            _currentSubtitleFontSet.FontId = GetIntFromNumericUpDown(sender);
        }

        private void toolStripMenuItemDuplicateRegionStyle_Click(object sender, EventArgs e)
        {
            var regionStyle = _currentNode.Tag as TextST.RegionStyle;
            if (regionStyle != null)
            {
                int regionStyleId = 0;
                foreach (var rs in _textST.StyleSegment.RegionStyles)
                {
                    regionStyleId = Math.Max(regionStyleId, rs.RegionStyleId);
                }
                regionStyleId++;

                var newRegionStyle = new TextST.RegionStyle(regionStyle) { RegionStyleId = regionStyleId };
                _textST.StyleSegment.RegionStyles.Add(newRegionStyle);
                var newNode = new TreeNode("Region style") { Tag = newRegionStyle };
                _currentNode.Parent.Nodes.Add(newNode);
                _currentNode.Parent.Text = "Region styles (" + _textST.StyleSegment.RegionStyles.Count + ")";
                treeView1.SelectedNode = newNode;
            }
        }

    }
}
