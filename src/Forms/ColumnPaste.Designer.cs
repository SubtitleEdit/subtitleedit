namespace Nikse.SubtitleEdit.Forms
{
    sealed partial class ColumnPaste
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
            this.buttonCancel = new System.Windows.Forms.Button();
            this.buttonOK = new System.Windows.Forms.Button();
            this.radioButtonTextOnly = new System.Windows.Forms.RadioButton();
            this.radioButtonTimeCodes = new System.Windows.Forms.RadioButton();
            this.radioButtonOriginalText = new System.Windows.Forms.RadioButton();
            this.radioButtonAll = new System.Windows.Forms.RadioButton();
            this.groupBoxChooseColumn = new System.Windows.Forms.GroupBox();
            this.groupBoxOverwriteOrInsert = new System.Windows.Forms.GroupBox();
            this.radioButtonShiftCellsDown = new System.Windows.Forms.RadioButton();
            this.radioButtonOverwrite = new System.Windows.Forms.RadioButton();
            this.groupBoxChooseColumn.SuspendLayout();
            this.groupBoxOverwriteOrInsert.SuspendLayout();
            this.SuspendLayout();
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonCancel.Location = new System.Drawing.Point(378, 136);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 4;
            this.buttonCancel.Text = "C&ancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // buttonOK
            // 
            this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOK.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonOK.Location = new System.Drawing.Point(297, 136);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(75, 23);
            this.buttonOK.TabIndex = 3;
            this.buttonOK.Text = "&OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
            // 
            // radioButtonTextOnly
            // 
            this.radioButtonTextOnly.AutoSize = true;
            this.radioButtonTextOnly.Checked = true;
            this.radioButtonTextOnly.Location = new System.Drawing.Point(6, 65);
            this.radioButtonTextOnly.Name = "radioButtonTextOnly";
            this.radioButtonTextOnly.Size = new System.Drawing.Size(68, 17);
            this.radioButtonTextOnly.TabIndex = 5;
            this.radioButtonTextOnly.TabStop = true;
            this.radioButtonTextOnly.Text = "Text only";
            this.radioButtonTextOnly.UseVisualStyleBackColor = true;
            // 
            // radioButtonTimeCodes
            // 
            this.radioButtonTimeCodes.AutoSize = true;
            this.radioButtonTimeCodes.Location = new System.Drawing.Point(6, 42);
            this.radioButtonTimeCodes.Name = "radioButtonTimeCodes";
            this.radioButtonTimeCodes.Size = new System.Drawing.Size(102, 17);
            this.radioButtonTimeCodes.TabIndex = 6;
            this.radioButtonTimeCodes.TabStop = true;
            this.radioButtonTimeCodes.Text = "Time codes only";
            this.radioButtonTimeCodes.UseVisualStyleBackColor = true;
            // 
            // radioButtonOriginalText
            // 
            this.radioButtonOriginalText.AutoSize = true;
            this.radioButtonOriginalText.Location = new System.Drawing.Point(6, 90);
            this.radioButtonOriginalText.Name = "radioButtonOriginalText";
            this.radioButtonOriginalText.Size = new System.Drawing.Size(102, 17);
            this.radioButtonOriginalText.TabIndex = 7;
            this.radioButtonOriginalText.TabStop = true;
            this.radioButtonOriginalText.Text = "Original text only";
            this.radioButtonOriginalText.UseVisualStyleBackColor = true;
            // 
            // radioButtonAll
            // 
            this.radioButtonAll.AutoSize = true;
            this.radioButtonAll.Location = new System.Drawing.Point(6, 19);
            this.radioButtonAll.Name = "radioButtonAll";
            this.radioButtonAll.Size = new System.Drawing.Size(36, 17);
            this.radioButtonAll.TabIndex = 8;
            this.radioButtonAll.TabStop = true;
            this.radioButtonAll.Text = "All";
            this.radioButtonAll.UseVisualStyleBackColor = true;
            // 
            // groupBoxChooseColumn
            // 
            this.groupBoxChooseColumn.Controls.Add(this.radioButtonAll);
            this.groupBoxChooseColumn.Controls.Add(this.radioButtonTextOnly);
            this.groupBoxChooseColumn.Controls.Add(this.radioButtonTimeCodes);
            this.groupBoxChooseColumn.Controls.Add(this.radioButtonOriginalText);
            this.groupBoxChooseColumn.Location = new System.Drawing.Point(12, 12);
            this.groupBoxChooseColumn.Name = "groupBoxChooseColumn";
            this.groupBoxChooseColumn.Size = new System.Drawing.Size(237, 116);
            this.groupBoxChooseColumn.TabIndex = 9;
            this.groupBoxChooseColumn.TabStop = false;
            this.groupBoxChooseColumn.Text = "Choose column";
            // 
            // groupBoxOverwriteOrInsert
            // 
            this.groupBoxOverwriteOrInsert.Controls.Add(this.radioButtonShiftCellsDown);
            this.groupBoxOverwriteOrInsert.Controls.Add(this.radioButtonOverwrite);
            this.groupBoxOverwriteOrInsert.Location = new System.Drawing.Point(255, 12);
            this.groupBoxOverwriteOrInsert.Name = "groupBoxOverwriteOrInsert";
            this.groupBoxOverwriteOrInsert.Size = new System.Drawing.Size(200, 116);
            this.groupBoxOverwriteOrInsert.TabIndex = 10;
            this.groupBoxOverwriteOrInsert.TabStop = false;
            this.groupBoxOverwriteOrInsert.Text = "Overwrite/Shift cells down";
            // 
            // radioButtonShiftCellsDown
            // 
            this.radioButtonShiftCellsDown.AutoSize = true;
            this.radioButtonShiftCellsDown.Location = new System.Drawing.Point(6, 42);
            this.radioButtonShiftCellsDown.Name = "radioButtonShiftCellsDown";
            this.radioButtonShiftCellsDown.Size = new System.Drawing.Size(99, 17);
            this.radioButtonShiftCellsDown.TabIndex = 10;
            this.radioButtonShiftCellsDown.TabStop = true;
            this.radioButtonShiftCellsDown.Text = "Shift cells down";
            this.radioButtonShiftCellsDown.UseVisualStyleBackColor = true;
            // 
            // radioButtonOverwrite
            // 
            this.radioButtonOverwrite.AutoSize = true;
            this.radioButtonOverwrite.Checked = true;
            this.radioButtonOverwrite.Location = new System.Drawing.Point(6, 19);
            this.radioButtonOverwrite.Name = "radioButtonOverwrite";
            this.radioButtonOverwrite.Size = new System.Drawing.Size(70, 17);
            this.radioButtonOverwrite.TabIndex = 9;
            this.radioButtonOverwrite.TabStop = true;
            this.radioButtonOverwrite.Text = "Overwrite";
            this.radioButtonOverwrite.UseVisualStyleBackColor = true;
            // 
            // ColumnPaste
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(465, 169);
            this.Controls.Add(this.groupBoxOverwriteOrInsert);
            this.Controls.Add(this.groupBoxChooseColumn);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonOK);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.KeyPreview = true;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ColumnPaste";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Column paste";
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.PasteSpecial_KeyDown);
            this.groupBoxChooseColumn.ResumeLayout(false);
            this.groupBoxChooseColumn.PerformLayout();
            this.groupBoxOverwriteOrInsert.ResumeLayout(false);
            this.groupBoxOverwriteOrInsert.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.RadioButton radioButtonTextOnly;
        private System.Windows.Forms.RadioButton radioButtonTimeCodes;
        private System.Windows.Forms.RadioButton radioButtonOriginalText;
        private System.Windows.Forms.RadioButton radioButtonAll;
        private System.Windows.Forms.GroupBox groupBoxChooseColumn;
        private System.Windows.Forms.GroupBox groupBoxOverwriteOrInsert;
        private System.Windows.Forms.RadioButton radioButtonShiftCellsDown;
        private System.Windows.Forms.RadioButton radioButtonOverwrite;
    }
}