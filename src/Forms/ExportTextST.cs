// WORK IN PROGRESS - DO NOT REFACTOR //
// WORK IN PROGRESS - DO NOT REFACTOR //
// WORK IN PROGRESS - DO NOT REFACTOR //

using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.SubtitleFormats;

namespace Nikse.SubtitleEdit.Forms
{
    public partial class ExportTextST : Form
    {
        private Subtitle _subtitle;
        private TreeNode _root;
        private string _fileName;
        private TextST _textST;
        private TextST.Palette _currentPalette;
        private TextST.RegionStyle _currentRegionStyle;        

        public ExportTextST(Subtitle subtitle)
        {
            InitializeComponent();

            _subtitle = subtitle;

            subtitleListView1.InitializeLanguage(Configuration.Settings.Language.General, Configuration.Settings);
            subtitleListView1.Fill(_subtitle);

            groupBoxPropertiesPalette.Left = groupBoxPropertiesRoot.Left;
            groupBoxPropertiesPalette.Top = groupBoxPropertiesRoot.Top;
            groupBoxPropertiesPalette.Size = groupBoxPropertiesRoot.Size;
            groupBoxPropertiesPalette.Anchor = groupBoxPropertiesRoot.Anchor;

            groupBoxPropertiesRegionStyle.Left = groupBoxPropertiesRoot.Left;
            groupBoxPropertiesRegionStyle.Top = groupBoxPropertiesRoot.Top;
            groupBoxPropertiesRegionStyle.Size = groupBoxPropertiesRoot.Size;
            groupBoxPropertiesRegionStyle.Anchor = groupBoxPropertiesRoot.Anchor;

            groupBoxPropertiesUserStyle.Left = groupBoxPropertiesRoot.Left;
            groupBoxPropertiesUserStyle.Top = groupBoxPropertiesRoot.Top;
            groupBoxPropertiesUserStyle.Size = groupBoxPropertiesRoot.Size;
            groupBoxPropertiesUserStyle.Anchor = groupBoxPropertiesRoot.Anchor;
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
                _subtitle = new Subtitle();
                _fileName = openFileDialog1.FileName;
                _textST.LoadSubtitle(_subtitle, null, _fileName);
                groupBoxTextST.Text = "TextST structure: " + Path.GetFileName(_fileName);
                
                subtitleListView1.Fill(_subtitle);
                
                treeView1.Nodes.Clear();
                _root = new TreeNode("TextST");
                treeView1.Nodes.Add(_root);
                if (_textST.StyleSegment != null)
                {
                    var styleNode = new TreeNode(string.Format("Style segment", _textST.StyleSegment));
                    _root.Nodes.Add(styleNode);

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

                    var palettesNode = new TreeNode(string.Format("Palettes ({0})", _textST.StyleSegment.Palettes.Count)) { Tag = _textST.StyleSegment.Palettes };
                    styleNode.Nodes.Add(palettesNode);
                    foreach (TextST.Palette palette in _textST.StyleSegment.Palettes)
                    {
                        var paletteNode = new TreeNode("Palette") { Tag = palette };
                        palettesNode.Nodes.Add(paletteNode);
                    }                    
                }

                if (_textST.PresentationSegments != null)
                {
                    var presentationSegmentsNode = new TreeNode(string.Format("Presentation segments ({0})", _textST.PresentationSegments.Count));
                    _root.Nodes.Add(presentationSegmentsNode);
                    int count = 0;
                    foreach (TextST.DialogPresentationSegment segment in _textST.PresentationSegments)
                    {
                        count++;
                        var presentationSegmentNode = new TreeNode(string.Format("Segment {0}: {1} -- > {2}", count, 
                            new TimeCode(segment.StartPtsMilliseconds), new TimeCode(segment.EndPtsMilliseconds))) { Tag = segment };
                        presentationSegmentsNode.Nodes.Add(presentationSegmentNode);
                    }
                }
                treeView1.ExpandAll();
                treeView1.SelectedNode = _root;
            }
        }

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            groupBoxPropertiesRoot.Visible = false;
            groupBoxPropertiesPalette.Visible = false;
            groupBoxPropertiesRegionStyle.Visible = false;
            groupBoxPropertiesUserStyle.Visible = false;
            if (e.Node != null && _textST != null)
            {
                if (e.Node == _root)
                {
                    groupBoxPropertiesRoot.Visible = true;
                    textBoxRoot.Text = "File name: " + _fileName + Environment.NewLine +
                                       "Number of region styles: " + _textST.StyleSegment.RegionStyles.Count + Environment.NewLine +
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

                    numericUpDownRegionStyleFontSize.Value = _currentRegionStyle.FontSize;

                }
                else if (e.Node.Tag is TextST.UserStyle)
                {
                    groupBoxPropertiesUserStyle.Visible = true;
                }
                else if (e.Node.Tag is TextST.DialogPresentationSegment)
                {
                    
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
                    foreach (Paragraph p in _subtitle.Paragraphs)
                    {
                        TextST.DialogPresentationSegment.WriteToStream(fs, p.Text, p.StartTime, p.EndTime, 1, false);
                    }
                }
            }
            MessageBox.Show("TextST PES packets saved as " + saveFileDialog1.FileName);
        }

        private void buttonSaveAsM2ts_Click(object sender, EventArgs e)
        {
            saveFileDialog1.DefaultExt = ".m2ts";
            saveFileDialog1.FileName = string.Empty;
            saveFileDialog1.Filter = "m2ts files|*.m2ts";
            if (saveFileDialog1.ShowDialog(this) == DialogResult.OK)
            {
                using (var fs = new FileStream(saveFileDialog1.FileName, FileMode.Create))
                {
                    foreach (Paragraph p in _subtitle.Paragraphs)
                    {
                        TextST.DialogPresentationSegment.WriteToStream(fs, p.Text, p.StartTime, p.EndTime, 1, false);
                    }
                }
            }
            MessageBox.Show("TextST M2TS file saved as " + saveFileDialog1.FileName);
        }

    }
}
