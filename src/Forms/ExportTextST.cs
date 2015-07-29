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

        private void buttonOK_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
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
                    var regionsNode = new TreeNode(string.Format("Regions ({0})", _textST.StyleSegment.Regions.Count));
                    _root.Nodes.Add(regionsNode);
                    foreach (TextST.Region region in _textST.StyleSegment.Regions)
                    {
                        var regionNode = new TreeNode("Region") { Tag = region };
                        regionsNode.Nodes.Add(regionNode);

                        var regionStyleNode = new TreeNode("Region style") { Tag = region.RegionStyle };
                        regionNode.Nodes.Add(regionStyleNode);

                        var userStylesNode = new TreeNode(string.Format("User styles ({0})", region.UserStyles.Count)) { Tag = region.UserStyles };
                        regionNode.Nodes.Add(userStylesNode);
                        foreach (var userStyle in region.UserStyles)
                        {
                            var userStyleNode = new TreeNode("User style") { Tag = userStyle };
                            userStylesNode.Nodes.Add(userStyleNode);
                        }
                    }

                    var palettesNode = new TreeNode(string.Format("Palettes ({0})", _textST.StyleSegment.Palettes.Count)) { Tag = _textST.StyleSegment.Palettes };
                    _root.Nodes.Add(palettesNode);
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
                                       "Number of style regions: " + _textST.StyleSegment.Regions.Count + Environment.NewLine +
                                       "Number of style palettes: " + _textST.StyleSegment.Palettes.Count + Environment.NewLine +
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

    }
}
