namespace Nikse.SubtitleEdit.Forms
{
    sealed partial class ModifySelection
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
            this.groupBoxWhatToDo = new System.Windows.Forms.GroupBox();
            this.radioButtonIntersect = new System.Windows.Forms.RadioButton();
            this.radioButtonSubtractFromSelection = new System.Windows.Forms.RadioButton();
            this.radioButtonAddToSelection = new System.Windows.Forms.RadioButton();
            this.radioButtonNewSelection = new System.Windows.Forms.RadioButton();
            this.groupBoxRule = new System.Windows.Forms.GroupBox();
            this.numericUpDownDuration = new System.Windows.Forms.NumericUpDown();
            this.checkBoxCaseSensitive = new System.Windows.Forms.CheckBox();
            this.textBoxText = new System.Windows.Forms.TextBox();
            this.comboBoxRule = new System.Windows.Forms.ComboBox();
            this.listViewStyles = new System.Windows.Forms.ListView();
            this.columnHeaderStyleName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.buttonCancel = new System.Windows.Forms.Button();
            this.buttonOK = new System.Windows.Forms.Button();
            this.buttonApply = new System.Windows.Forms.Button();
            this.groupBoxPreview = new System.Windows.Forms.GroupBox();
            this.listViewFixes = new System.Windows.Forms.ListView();
            this.columnHeaderApply = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderLine = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderText = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderStyle = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.labelInfo = new System.Windows.Forms.Label();
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.toolStripMenuItemSelectAll = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemInverseSelection = new System.Windows.Forms.ToolStripMenuItem();
            this.groupBoxWhatToDo.SuspendLayout();
            this.groupBoxRule.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownDuration)).BeginInit();
            this.groupBoxPreview.SuspendLayout();
            this.contextMenuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBoxWhatToDo
            // 
            this.groupBoxWhatToDo.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxWhatToDo.Controls.Add(this.radioButtonIntersect);
            this.groupBoxWhatToDo.Controls.Add(this.radioButtonSubtractFromSelection);
            this.groupBoxWhatToDo.Controls.Add(this.radioButtonAddToSelection);
            this.groupBoxWhatToDo.Controls.Add(this.radioButtonNewSelection);
            this.groupBoxWhatToDo.Location = new System.Drawing.Point(380, 12);
            this.groupBoxWhatToDo.Name = "groupBoxWhatToDo";
            this.groupBoxWhatToDo.Size = new System.Drawing.Size(411, 135);
            this.groupBoxWhatToDo.TabIndex = 1;
            this.groupBoxWhatToDo.TabStop = false;
            this.groupBoxWhatToDo.Text = "What to do with matches";
            // 
            // radioButtonIntersect
            // 
            this.radioButtonIntersect.AutoSize = true;
            this.radioButtonIntersect.Location = new System.Drawing.Point(7, 89);
            this.radioButtonIntersect.Name = "radioButtonIntersect";
            this.radioButtonIntersect.Size = new System.Drawing.Size(172, 17);
            this.radioButtonIntersect.TabIndex = 3;
            this.radioButtonIntersect.Text = "Intersect with  current selection";
            this.radioButtonIntersect.UseVisualStyleBackColor = true;
            this.radioButtonIntersect.CheckedChanged += new System.EventHandler(this.radioButton_CheckedChanged);
            // 
            // radioButtonSubtractFromSelection
            // 
            this.radioButtonSubtractFromSelection.AutoSize = true;
            this.radioButtonSubtractFromSelection.Location = new System.Drawing.Point(7, 66);
            this.radioButtonSubtractFromSelection.Name = "radioButtonSubtractFromSelection";
            this.radioButtonSubtractFromSelection.Size = new System.Drawing.Size(169, 17);
            this.radioButtonSubtractFromSelection.TabIndex = 2;
            this.radioButtonSubtractFromSelection.Text = "Subtract from current selection";
            this.radioButtonSubtractFromSelection.UseVisualStyleBackColor = true;
            this.radioButtonSubtractFromSelection.CheckedChanged += new System.EventHandler(this.radioButton_CheckedChanged);
            // 
            // radioButtonAddToSelection
            // 
            this.radioButtonAddToSelection.AutoSize = true;
            this.radioButtonAddToSelection.Location = new System.Drawing.Point(7, 43);
            this.radioButtonAddToSelection.Name = "radioButtonAddToSelection";
            this.radioButtonAddToSelection.Size = new System.Drawing.Size(137, 17);
            this.radioButtonAddToSelection.TabIndex = 1;
            this.radioButtonAddToSelection.Text = "Add to current selection";
            this.radioButtonAddToSelection.UseVisualStyleBackColor = true;
            this.radioButtonAddToSelection.CheckedChanged += new System.EventHandler(this.radioButton_CheckedChanged);
            // 
            // radioButtonNewSelection
            // 
            this.radioButtonNewSelection.AutoSize = true;
            this.radioButtonNewSelection.Checked = true;
            this.radioButtonNewSelection.Location = new System.Drawing.Point(7, 20);
            this.radioButtonNewSelection.Name = "radioButtonNewSelection";
            this.radioButtonNewSelection.Size = new System.Drawing.Size(120, 17);
            this.radioButtonNewSelection.TabIndex = 0;
            this.radioButtonNewSelection.TabStop = true;
            this.radioButtonNewSelection.Text = "Make new selection";
            this.radioButtonNewSelection.UseVisualStyleBackColor = true;
            this.radioButtonNewSelection.CheckedChanged += new System.EventHandler(this.radioButton_CheckedChanged);
            // 
            // groupBoxRule
            // 
            this.groupBoxRule.Controls.Add(this.numericUpDownDuration);
            this.groupBoxRule.Controls.Add(this.checkBoxCaseSensitive);
            this.groupBoxRule.Controls.Add(this.textBoxText);
            this.groupBoxRule.Controls.Add(this.comboBoxRule);
            this.groupBoxRule.Controls.Add(this.listViewStyles);
            this.groupBoxRule.Location = new System.Drawing.Point(12, 12);
            this.groupBoxRule.Name = "groupBoxRule";
            this.groupBoxRule.Size = new System.Drawing.Size(362, 135);
            this.groupBoxRule.TabIndex = 0;
            this.groupBoxRule.TabStop = false;
            this.groupBoxRule.Text = "Rule";
            // 
            // numericUpDownDuration
            // 
            this.numericUpDownDuration.Location = new System.Drawing.Point(198, 22);
            this.numericUpDownDuration.Maximum = new decimal(new int[] {
            10000,
            0,
            0,
            0});
            this.numericUpDownDuration.Name = "numericUpDownDuration";
            this.numericUpDownDuration.Size = new System.Drawing.Size(82, 20);
            this.numericUpDownDuration.TabIndex = 4;
            this.numericUpDownDuration.ValueChanged += new System.EventHandler(this.numericUpDownDuration_ValueChanged);
            // 
            // checkBoxCaseSensitive
            // 
            this.checkBoxCaseSensitive.AutoSize = true;
            this.checkBoxCaseSensitive.Location = new System.Drawing.Point(6, 47);
            this.checkBoxCaseSensitive.Name = "checkBoxCaseSensitive";
            this.checkBoxCaseSensitive.Size = new System.Drawing.Size(94, 17);
            this.checkBoxCaseSensitive.TabIndex = 2;
            this.checkBoxCaseSensitive.Text = "Case sensitive";
            this.checkBoxCaseSensitive.UseVisualStyleBackColor = true;
            this.checkBoxCaseSensitive.CheckedChanged += new System.EventHandler(this.checkBoxCaseSensitive_CheckedChanged);
            // 
            // textBoxText
            // 
            this.textBoxText.Location = new System.Drawing.Point(198, 21);
            this.textBoxText.Name = "textBoxText";
            this.textBoxText.Size = new System.Drawing.Size(158, 20);
            this.textBoxText.TabIndex = 1;
            this.textBoxText.TextChanged += new System.EventHandler(this.textBox1_TextChanged);
            // 
            // comboBoxRule
            // 
            this.comboBoxRule.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxRule.FormattingEnabled = true;
            this.comboBoxRule.Items.AddRange(new object[] {
            "Contains",
            "Starts with",
            "Ends with",
            "Not contains",
            "RegEx"});
            this.comboBoxRule.Location = new System.Drawing.Point(6, 20);
            this.comboBoxRule.Name = "comboBoxRule";
            this.comboBoxRule.Size = new System.Drawing.Size(185, 21);
            this.comboBoxRule.TabIndex = 0;
            this.comboBoxRule.SelectedIndexChanged += new System.EventHandler(this.comboBoxRule_SelectedIndexChanged);
            // 
            // listViewStyles
            // 
            this.listViewStyles.CheckBoxes = true;
            this.listViewStyles.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeaderStyleName});
            this.listViewStyles.FullRowSelect = true;
            this.listViewStyles.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            this.listViewStyles.HideSelection = false;
            this.listViewStyles.Location = new System.Drawing.Point(198, 21);
            this.listViewStyles.Name = "listViewStyles";
            this.listViewStyles.Size = new System.Drawing.Size(158, 108);
            this.listViewStyles.TabIndex = 3;
            this.listViewStyles.UseCompatibleStateImageBehavior = false;
            this.listViewStyles.View = System.Windows.Forms.View.Details;
            this.listViewStyles.ItemChecked += new System.Windows.Forms.ItemCheckedEventHandler(this.listViewStyles_ItemChecked);
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonCancel.Location = new System.Drawing.Point(621, 446);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 6;
            this.buttonCancel.Text = "C&ancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // buttonOK
            // 
            this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOK.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonOK.Location = new System.Drawing.Point(540, 446);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(75, 23);
            this.buttonOK.TabIndex = 5;
            this.buttonOK.Text = "&OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
            // 
            // buttonApply
            // 
            this.buttonApply.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonApply.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonApply.Location = new System.Drawing.Point(702, 446);
            this.buttonApply.Name = "buttonApply";
            this.buttonApply.Size = new System.Drawing.Size(89, 23);
            this.buttonApply.TabIndex = 7;
            this.buttonApply.Text = "&Apply";
            this.buttonApply.UseVisualStyleBackColor = true;
            this.buttonApply.Click += new System.EventHandler(this.buttonApply_Click);
            // 
            // groupBoxPreview
            // 
            this.groupBoxPreview.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxPreview.Controls.Add(this.listViewFixes);
            this.groupBoxPreview.Location = new System.Drawing.Point(12, 153);
            this.groupBoxPreview.Name = "groupBoxPreview";
            this.groupBoxPreview.Size = new System.Drawing.Size(779, 286);
            this.groupBoxPreview.TabIndex = 2;
            this.groupBoxPreview.TabStop = false;
            this.groupBoxPreview.Text = "Matching lines";
            // 
            // listViewFixes
            // 
            this.listViewFixes.CheckBoxes = true;
            this.listViewFixes.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeaderApply,
            this.columnHeaderLine,
            this.columnHeaderText,
            this.columnHeaderStyle});
            this.listViewFixes.ContextMenuStrip = this.contextMenuStrip1;
            this.listViewFixes.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listViewFixes.FullRowSelect = true;
            this.listViewFixes.HideSelection = false;
            this.listViewFixes.Location = new System.Drawing.Point(3, 16);
            this.listViewFixes.Name = "listViewFixes";
            this.listViewFixes.Size = new System.Drawing.Size(773, 267);
            this.listViewFixes.TabIndex = 1;
            this.listViewFixes.UseCompatibleStateImageBehavior = false;
            this.listViewFixes.View = System.Windows.Forms.View.Details;
            // 
            // columnHeaderApply
            // 
            this.columnHeaderApply.Text = "Apply";
            this.columnHeaderApply.Width = 50;
            // 
            // columnHeaderLine
            // 
            this.columnHeaderLine.Text = "Line#";
            this.columnHeaderLine.Width = 122;
            // 
            // columnHeaderText
            // 
            this.columnHeaderText.Text = "Text";
            this.columnHeaderText.Width = 450;
            // 
            // columnHeaderStyle
            // 
            this.columnHeaderStyle.Text = "Style";
            // 
            // labelInfo
            // 
            this.labelInfo.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.labelInfo.AutoSize = true;
            this.labelInfo.Location = new System.Drawing.Point(12, 446);
            this.labelInfo.Name = "labelInfo";
            this.labelInfo.Size = new System.Drawing.Size(47, 13);
            this.labelInfo.TabIndex = 3;
            this.labelInfo.Text = "labelInfo";
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItemSelectAll,
            this.toolStripMenuItemInverseSelection});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(181, 70);
            // 
            // toolStripMenuItemSelectAll
            // 
            this.toolStripMenuItemSelectAll.Name = "toolStripMenuItemSelectAll";
            this.toolStripMenuItemSelectAll.Size = new System.Drawing.Size(180, 22);
            this.toolStripMenuItemSelectAll.Text = "Select all";
            this.toolStripMenuItemSelectAll.Click += new System.EventHandler(this.toolStripMenuItemSelectAll_Click);
            // 
            // toolStripMenuItemInverseSelection
            // 
            this.toolStripMenuItemInverseSelection.Name = "toolStripMenuItemInverseSelection";
            this.toolStripMenuItemInverseSelection.Size = new System.Drawing.Size(180, 22);
            this.toolStripMenuItemInverseSelection.Text = "Inverse selection";
            this.toolStripMenuItemInverseSelection.Click += new System.EventHandler(this.toolStripMenuItemInverseSelection_Click);
            // 
            // ModifySelection
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(803, 478);
            this.Controls.Add(this.labelInfo);
            this.Controls.Add(this.groupBoxWhatToDo);
            this.Controls.Add(this.groupBoxPreview);
            this.Controls.Add(this.buttonApply);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonOK);
            this.Controls.Add(this.groupBoxRule);
            this.KeyPreview = true;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(700, 400);
            this.Name = "ModifySelection";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Create/modify selection";
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.ModifySelection_KeyDown);
            this.Resize += new System.EventHandler(this.ModifySelection_Resize);
            this.groupBoxWhatToDo.ResumeLayout(false);
            this.groupBoxWhatToDo.PerformLayout();
            this.groupBoxRule.ResumeLayout(false);
            this.groupBoxRule.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownDuration)).EndInit();
            this.groupBoxPreview.ResumeLayout(false);
            this.contextMenuStrip1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBoxWhatToDo;
        private System.Windows.Forms.RadioButton radioButtonSubtractFromSelection;
        private System.Windows.Forms.RadioButton radioButtonAddToSelection;
        private System.Windows.Forms.RadioButton radioButtonNewSelection;
        private System.Windows.Forms.GroupBox groupBoxRule;
        private System.Windows.Forms.TextBox textBoxText;
        private System.Windows.Forms.ComboBox comboBoxRule;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.Button buttonApply;
        private System.Windows.Forms.GroupBox groupBoxPreview;
        private System.Windows.Forms.ListView listViewFixes;
        private System.Windows.Forms.ColumnHeader columnHeaderApply;
        private System.Windows.Forms.ColumnHeader columnHeaderLine;
        private System.Windows.Forms.ColumnHeader columnHeaderText;
        private System.Windows.Forms.Label labelInfo;
        private System.Windows.Forms.CheckBox checkBoxCaseSensitive;
        private System.Windows.Forms.RadioButton radioButtonIntersect;
        private System.Windows.Forms.ColumnHeader columnHeaderStyle;
        private System.Windows.Forms.ListView listViewStyles;
        private System.Windows.Forms.ColumnHeader columnHeaderStyleName;
        private System.Windows.Forms.NumericUpDown numericUpDownDuration;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemSelectAll;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemInverseSelection;
    }
}