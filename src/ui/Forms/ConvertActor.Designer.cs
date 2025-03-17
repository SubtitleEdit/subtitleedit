namespace Nikse.SubtitleEdit.Forms
{
    partial class ConvertActor
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.columnHeaderAction = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.groupBoxLinesFound = new System.Windows.Forms.GroupBox();
            this.listViewFixes = new System.Windows.Forms.ListView();
            this.columnHeaderLineNumber = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderBefore = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderAfter = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.toolStripMenuItemSelectAll = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemInverseSelection = new System.Windows.Forms.ToolStripMenuItem();
            this.buttonOK = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.checkBoxColor = new System.Windows.Forms.CheckBox();
            this.labelConvertFrom = new System.Windows.Forms.Label();
            this.nikseComboBoxConvertFrom = new Nikse.SubtitleEdit.Controls.NikseComboBox();
            this.nikseComboBoxConvertTo = new Nikse.SubtitleEdit.Controls.NikseComboBox();
            this.labelConvertTo = new System.Windows.Forms.Label();
            this.panelColor = new System.Windows.Forms.Panel();
            this.buttonColor = new System.Windows.Forms.Button();
            this.checkBoxChangeCasing = new System.Windows.Forms.CheckBox();
            this.nikseComboBoxCasing = new Nikse.SubtitleEdit.Controls.NikseComboBox();
            this.checkBoxOnlyNames = new System.Windows.Forms.CheckBox();
            this.groupBoxLinesFound.SuspendLayout();
            this.contextMenuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // columnHeaderAction
            // 
            this.columnHeaderAction.Text = "Apply";
            this.columnHeaderAction.Width = 45;
            // 
            // groupBoxLinesFound
            // 
            this.groupBoxLinesFound.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxLinesFound.Controls.Add(this.listViewFixes);
            this.groupBoxLinesFound.Location = new System.Drawing.Point(12, 115);
            this.groupBoxLinesFound.Name = "groupBoxLinesFound";
            this.groupBoxLinesFound.Size = new System.Drawing.Size(1028, 571);
            this.groupBoxLinesFound.TabIndex = 8;
            this.groupBoxLinesFound.TabStop = false;
            this.groupBoxLinesFound.Text = "Lines that will be merged";
            // 
            // listViewFixes
            // 
            this.listViewFixes.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listViewFixes.CheckBoxes = true;
            this.listViewFixes.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeaderAction,
            this.columnHeaderLineNumber,
            this.columnHeaderBefore,
            this.columnHeaderAfter});
            this.listViewFixes.ContextMenuStrip = this.contextMenuStrip1;
            this.listViewFixes.FullRowSelect = true;
            this.listViewFixes.HideSelection = false;
            this.listViewFixes.Location = new System.Drawing.Point(6, 19);
            this.listViewFixes.Name = "listViewFixes";
            this.listViewFixes.Size = new System.Drawing.Size(1013, 538);
            this.listViewFixes.TabIndex = 0;
            this.listViewFixes.UseCompatibleStateImageBehavior = false;
            this.listViewFixes.View = System.Windows.Forms.View.Details;
            this.listViewFixes.SelectedIndexChanged += new System.EventHandler(this.listViewFixes_SelectedIndexChanged);
            // 
            // columnHeaderLineNumber
            // 
            this.columnHeaderLineNumber.Text = "Line#";
            this.columnHeaderLineNumber.Width = 80;
            // 
            // columnHeaderBefore
            // 
            this.columnHeaderBefore.Text = "Before";
            this.columnHeaderBefore.Width = 420;
            // 
            // columnHeaderAfter
            // 
            this.columnHeaderAfter.Text = "After";
            this.columnHeaderAfter.Width = 420;
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItemSelectAll,
            this.toolStripMenuItemInverseSelection});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(162, 48);
            // 
            // toolStripMenuItemSelectAll
            // 
            this.toolStripMenuItemSelectAll.Name = "toolStripMenuItemSelectAll";
            this.toolStripMenuItemSelectAll.Size = new System.Drawing.Size(161, 22);
            this.toolStripMenuItemSelectAll.Text = "Select all";
            this.toolStripMenuItemSelectAll.Click += new System.EventHandler(this.toolStripMenuItemSelectAll_Click);
            // 
            // toolStripMenuItemInverseSelection
            // 
            this.toolStripMenuItemInverseSelection.Name = "toolStripMenuItemInverseSelection";
            this.toolStripMenuItemInverseSelection.Size = new System.Drawing.Size(161, 22);
            this.toolStripMenuItemInverseSelection.Text = "Inverse selection";
            this.toolStripMenuItemInverseSelection.Click += new System.EventHandler(this.toolStripMenuItemInverseSelection_Click);
            // 
            // buttonOK
            // 
            this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOK.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonOK.Location = new System.Drawing.Point(884, 692);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(75, 23);
            this.buttonOK.TabIndex = 9;
            this.buttonOK.Text = "&OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.ButtonOkClick);
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonCancel.Location = new System.Drawing.Point(965, 692);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 10;
            this.buttonCancel.Text = "C&ancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.ButtonCancelClick);
            // 
            // checkBoxColor
            // 
            this.checkBoxColor.AutoSize = true;
            this.checkBoxColor.Checked = true;
            this.checkBoxColor.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxColor.Location = new System.Drawing.Point(302, 21);
            this.checkBoxColor.Name = "checkBoxColor";
            this.checkBoxColor.Size = new System.Drawing.Size(68, 17);
            this.checkBoxColor.TabIndex = 2;
            this.checkBoxColor.Text = "Set color";
            this.checkBoxColor.UseVisualStyleBackColor = true;
            this.checkBoxColor.CheckedChanged += new System.EventHandler(this.checkBoxFixIncrementing_CheckedChanged);
            // 
            // labelConvertFrom
            // 
            this.labelConvertFrom.AutoSize = true;
            this.labelConvertFrom.Location = new System.Drawing.Point(18, 17);
            this.labelConvertFrom.Name = "labelConvertFrom";
            this.labelConvertFrom.Size = new System.Drawing.Size(94, 13);
            this.labelConvertFrom.TabIndex = 49;
            this.labelConvertFrom.Text = "Convert actor from";
            // 
            // nikseComboBoxConvertFrom
            // 
            this.nikseComboBoxConvertFrom.BackColor = System.Drawing.SystemColors.Window;
            this.nikseComboBoxConvertFrom.BackColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.nikseComboBoxConvertFrom.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(171)))), ((int)(((byte)(173)))), ((int)(((byte)(179)))));
            this.nikseComboBoxConvertFrom.BorderColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(120)))), ((int)(((byte)(120)))), ((int)(((byte)(120)))));
            this.nikseComboBoxConvertFrom.ButtonForeColor = System.Drawing.SystemColors.ControlText;
            this.nikseComboBoxConvertFrom.ButtonForeColorDown = System.Drawing.Color.Orange;
            this.nikseComboBoxConvertFrom.ButtonForeColorOver = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.nikseComboBoxConvertFrom.DropDownHeight = 400;
            this.nikseComboBoxConvertFrom.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.nikseComboBoxConvertFrom.DropDownWidth = 0;
            this.nikseComboBoxConvertFrom.FormattingEnabled = false;
            this.nikseComboBoxConvertFrom.Items.AddRange(new string[] {
            "Inline actor",
            "Actor",
            "Style"});
            this.nikseComboBoxConvertFrom.Location = new System.Drawing.Point(118, 12);
            this.nikseComboBoxConvertFrom.MaxLength = 32767;
            this.nikseComboBoxConvertFrom.Name = "nikseComboBoxConvertFrom";
            this.nikseComboBoxConvertFrom.SelectedIndex = -1;
            this.nikseComboBoxConvertFrom.SelectedItem = null;
            this.nikseComboBoxConvertFrom.SelectedText = "";
            this.nikseComboBoxConvertFrom.Size = new System.Drawing.Size(152, 23);
            this.nikseComboBoxConvertFrom.TabIndex = 0;
            this.nikseComboBoxConvertFrom.UsePopupWindow = false;
            this.nikseComboBoxConvertFrom.SelectedIndexChanged += new System.EventHandler(this.nikseComboBoxConvertFrom_SelectedIndexChanged);
            // 
            // nikseComboBoxConvertTo
            // 
            this.nikseComboBoxConvertTo.BackColor = System.Drawing.SystemColors.Window;
            this.nikseComboBoxConvertTo.BackColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.nikseComboBoxConvertTo.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(171)))), ((int)(((byte)(173)))), ((int)(((byte)(179)))));
            this.nikseComboBoxConvertTo.BorderColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(120)))), ((int)(((byte)(120)))), ((int)(((byte)(120)))));
            this.nikseComboBoxConvertTo.ButtonForeColor = System.Drawing.SystemColors.ControlText;
            this.nikseComboBoxConvertTo.ButtonForeColorDown = System.Drawing.Color.Orange;
            this.nikseComboBoxConvertTo.ButtonForeColorOver = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.nikseComboBoxConvertTo.DropDownHeight = 400;
            this.nikseComboBoxConvertTo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.nikseComboBoxConvertTo.DropDownWidth = 0;
            this.nikseComboBoxConvertTo.FormattingEnabled = false;
            this.nikseComboBoxConvertTo.Items.AddRange(new string[] {
            "Inline actor",
            "Actor",
            "Style"});
            this.nikseComboBoxConvertTo.Location = new System.Drawing.Point(118, 48);
            this.nikseComboBoxConvertTo.MaxLength = 32767;
            this.nikseComboBoxConvertTo.Name = "nikseComboBoxConvertTo";
            this.nikseComboBoxConvertTo.SelectedIndex = -1;
            this.nikseComboBoxConvertTo.SelectedItem = null;
            this.nikseComboBoxConvertTo.SelectedText = "";
            this.nikseComboBoxConvertTo.Size = new System.Drawing.Size(152, 23);
            this.nikseComboBoxConvertTo.TabIndex = 1;
            this.nikseComboBoxConvertTo.UsePopupWindow = false;
            this.nikseComboBoxConvertTo.SelectedIndexChanged += new System.EventHandler(this.nikseComboBoxConvertTo_SelectedIndexChanged);
            // 
            // labelConvertTo
            // 
            this.labelConvertTo.AutoSize = true;
            this.labelConvertTo.Location = new System.Drawing.Point(18, 50);
            this.labelConvertTo.Name = "labelConvertTo";
            this.labelConvertTo.Size = new System.Drawing.Size(83, 13);
            this.labelConvertTo.TabIndex = 52;
            this.labelConvertTo.Text = "Convert actor to";
            // 
            // panelColor
            // 
            this.panelColor.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelColor.Location = new System.Drawing.Point(481, 20);
            this.panelColor.Name = "panelColor";
            this.panelColor.Size = new System.Drawing.Size(21, 21);
            this.panelColor.TabIndex = 4;
            this.panelColor.MouseClick += new System.Windows.Forms.MouseEventHandler(this.panelColor_MouseClick);
            // 
            // buttonColor
            // 
            this.buttonColor.Location = new System.Drawing.Point(386, 17);
            this.buttonColor.Name = "buttonColor";
            this.buttonColor.Size = new System.Drawing.Size(89, 23);
            this.buttonColor.TabIndex = 3;
            this.buttonColor.Text = "Color";
            this.buttonColor.UseVisualStyleBackColor = true;
            this.buttonColor.Click += new System.EventHandler(this.buttonColor_Click);
            // 
            // checkBoxChangeCasing
            // 
            this.checkBoxChangeCasing.AutoSize = true;
            this.checkBoxChangeCasing.Checked = true;
            this.checkBoxChangeCasing.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxChangeCasing.Location = new System.Drawing.Point(302, 56);
            this.checkBoxChangeCasing.Name = "checkBoxChangeCasing";
            this.checkBoxChangeCasing.Size = new System.Drawing.Size(97, 17);
            this.checkBoxChangeCasing.TabIndex = 5;
            this.checkBoxChangeCasing.Text = "Change casing";
            this.checkBoxChangeCasing.UseVisualStyleBackColor = true;
            this.checkBoxChangeCasing.CheckedChanged += new System.EventHandler(this.checkBoxChangeCasing_CheckedChanged);
            // 
            // nikseComboBoxCasing
            // 
            this.nikseComboBoxCasing.BackColor = System.Drawing.SystemColors.Window;
            this.nikseComboBoxCasing.BackColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.nikseComboBoxCasing.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(171)))), ((int)(((byte)(173)))), ((int)(((byte)(179)))));
            this.nikseComboBoxCasing.BorderColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(120)))), ((int)(((byte)(120)))), ((int)(((byte)(120)))));
            this.nikseComboBoxCasing.ButtonForeColor = System.Drawing.SystemColors.ControlText;
            this.nikseComboBoxCasing.ButtonForeColorDown = System.Drawing.Color.Orange;
            this.nikseComboBoxCasing.ButtonForeColorOver = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.nikseComboBoxCasing.DropDownHeight = 400;
            this.nikseComboBoxCasing.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.nikseComboBoxCasing.DropDownWidth = 0;
            this.nikseComboBoxCasing.FormattingEnabled = false;
            this.nikseComboBoxCasing.Items.AddRange(new string[] {
            "Normal",
            "Uppercase",
            "Lowerccase",
            "Propercase"});
            this.nikseComboBoxCasing.Location = new System.Drawing.Point(405, 53);
            this.nikseComboBoxCasing.MaxLength = 32767;
            this.nikseComboBoxCasing.Name = "nikseComboBoxCasing";
            this.nikseComboBoxCasing.SelectedIndex = -1;
            this.nikseComboBoxCasing.SelectedItem = null;
            this.nikseComboBoxCasing.SelectedText = "";
            this.nikseComboBoxCasing.Size = new System.Drawing.Size(219, 23);
            this.nikseComboBoxCasing.TabIndex = 6;
            this.nikseComboBoxCasing.UsePopupWindow = false;
            this.nikseComboBoxCasing.SelectedIndexChanged += new System.EventHandler(this.nikseComboBoxCasing_SelectedIndexChanged);
            // 
            // checkBoxOnlyNames
            // 
            this.checkBoxOnlyNames.AutoSize = true;
            this.checkBoxOnlyNames.Location = new System.Drawing.Point(302, 88);
            this.checkBoxOnlyNames.Name = "checkBoxOnlyNames";
            this.checkBoxOnlyNames.Size = new System.Drawing.Size(114, 17);
            this.checkBoxOnlyNames.TabIndex = 7;
            this.checkBoxOnlyNames.Text = "Only check names";
            this.checkBoxOnlyNames.UseVisualStyleBackColor = true;
            this.checkBoxOnlyNames.CheckedChanged += new System.EventHandler(this.checkBoxOnlyNames_CheckedChanged);
            // 
            // ConvertActor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1052, 725);
            this.Controls.Add(this.checkBoxOnlyNames);
            this.Controls.Add(this.nikseComboBoxCasing);
            this.Controls.Add(this.checkBoxChangeCasing);
            this.Controls.Add(this.panelColor);
            this.Controls.Add(this.buttonColor);
            this.Controls.Add(this.labelConvertTo);
            this.Controls.Add(this.nikseComboBoxConvertTo);
            this.Controls.Add(this.nikseComboBoxConvertFrom);
            this.Controls.Add(this.labelConvertFrom);
            this.Controls.Add(this.checkBoxColor);
            this.Controls.Add(this.groupBoxLinesFound);
            this.Controls.Add(this.buttonOK);
            this.Controls.Add(this.buttonCancel);
            this.KeyPreview = true;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(750, 400);
            this.Name = "ConvertActor";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Convert actor";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ConvertActor_FormClosing);
            this.Shown += new System.EventHandler(this.ConvertActor_Shown);
            this.ResizeEnd += new System.EventHandler(this.ConvertActor_ResizeEnd);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.ConvertActor_KeyDown);
            this.groupBoxLinesFound.ResumeLayout(false);
            this.contextMenuStrip1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ColumnHeader columnHeaderAction;
        private System.Windows.Forms.GroupBox groupBoxLinesFound;
        private System.Windows.Forms.ListView listViewFixes;
        private System.Windows.Forms.ColumnHeader columnHeaderLineNumber;
        private System.Windows.Forms.ColumnHeader columnHeaderBefore;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.CheckBox checkBoxColor;
        private System.Windows.Forms.Label labelConvertFrom;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemSelectAll;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemInverseSelection;
        private Controls.NikseComboBox nikseComboBoxConvertFrom;
        private Controls.NikseComboBox nikseComboBoxConvertTo;
        private System.Windows.Forms.Label labelConvertTo;
        private System.Windows.Forms.Panel panelColor;
        private System.Windows.Forms.Button buttonColor;
        private System.Windows.Forms.CheckBox checkBoxChangeCasing;
        private Controls.NikseComboBox nikseComboBoxCasing;
        private System.Windows.Forms.ColumnHeader columnHeaderAfter;
        private System.Windows.Forms.CheckBox checkBoxOnlyNames;
    }
}