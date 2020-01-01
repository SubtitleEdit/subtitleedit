namespace Nikse.SubtitleEdit.Forms.Ocr
{
    partial class AddBeterMultiMatch
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
            this.groupBoxInspectItems = new System.Windows.Forms.GroupBox();
            this.label1 = new System.Windows.Forms.Label();
            this.checkBoxItalic = new System.Windows.Forms.CheckBox();
            this.labelText = new System.Windows.Forms.Label();
            this.textBoxText = new System.Windows.Forms.TextBox();
            this.numericUpDownExpandCount = new System.Windows.Forms.NumericUpDown();
            this.labelImageInfo = new System.Windows.Forms.Label();
            this.pictureBoxInspectItem = new System.Windows.Forms.PictureBox();
            this.listBoxInspectItems = new System.Windows.Forms.ListBox();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.buttonOK = new System.Windows.Forms.Button();
            this.groupBoxInspectItems.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownExpandCount)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxInspectItem)).BeginInit();
            this.SuspendLayout();
            // 
            // groupBoxInspectItems
            // 
            this.groupBoxInspectItems.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxInspectItems.Controls.Add(this.label1);
            this.groupBoxInspectItems.Controls.Add(this.checkBoxItalic);
            this.groupBoxInspectItems.Controls.Add(this.labelText);
            this.groupBoxInspectItems.Controls.Add(this.textBoxText);
            this.groupBoxInspectItems.Controls.Add(this.numericUpDownExpandCount);
            this.groupBoxInspectItems.Controls.Add(this.labelImageInfo);
            this.groupBoxInspectItems.Controls.Add(this.pictureBoxInspectItem);
            this.groupBoxInspectItems.Controls.Add(this.listBoxInspectItems);
            this.groupBoxInspectItems.Location = new System.Drawing.Point(12, 12);
            this.groupBoxInspectItems.Name = "groupBoxInspectItems";
            this.groupBoxInspectItems.Size = new System.Drawing.Size(521, 284);
            this.groupBoxInspectItems.TabIndex = 1;
            this.groupBoxInspectItems.TabStop = false;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(253, 19);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(73, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Expand count";
            // 
            // checkBoxItalic
            // 
            this.checkBoxItalic.AutoSize = true;
            this.checkBoxItalic.Location = new System.Drawing.Point(253, 130);
            this.checkBoxItalic.Name = "checkBoxItalic";
            this.checkBoxItalic.Size = new System.Drawing.Size(58, 17);
            this.checkBoxItalic.TabIndex = 4;
            this.checkBoxItalic.Text = "Is &italic";
            this.checkBoxItalic.UseVisualStyleBackColor = true;
            // 
            // labelText
            // 
            this.labelText.AutoSize = true;
            this.labelText.Location = new System.Drawing.Point(252, 86);
            this.labelText.Name = "labelText";
            this.labelText.Size = new System.Drawing.Size(28, 13);
            this.labelText.TabIndex = 31;
            this.labelText.Text = "Text";
            // 
            // textBoxText
            // 
            this.textBoxText.Font = new System.Drawing.Font("Tahoma", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBoxText.Location = new System.Drawing.Point(252, 102);
            this.textBoxText.Name = "textBoxText";
            this.textBoxText.Size = new System.Drawing.Size(100, 23);
            this.textBoxText.TabIndex = 3;
            // 
            // numericUpDownExpandCount
            // 
            this.numericUpDownExpandCount.Location = new System.Drawing.Point(252, 38);
            this.numericUpDownExpandCount.Maximum = new decimal(new int[] {
            3,
            0,
            0,
            0});
            this.numericUpDownExpandCount.Minimum = new decimal(new int[] {
            2,
            0,
            0,
            0});
            this.numericUpDownExpandCount.Name = "numericUpDownExpandCount";
            this.numericUpDownExpandCount.Size = new System.Drawing.Size(63, 20);
            this.numericUpDownExpandCount.TabIndex = 2;
            this.numericUpDownExpandCount.Value = new decimal(new int[] {
            2,
            0,
            0,
            0});
            this.numericUpDownExpandCount.ValueChanged += new System.EventHandler(this.NumericUpDownExpandCountValueChanged);
            // 
            // labelImageInfo
            // 
            this.labelImageInfo.AutoSize = true;
            this.labelImageInfo.Location = new System.Drawing.Point(253, 168);
            this.labelImageInfo.Name = "labelImageInfo";
            this.labelImageInfo.Size = new System.Drawing.Size(45, 13);
            this.labelImageInfo.TabIndex = 5;
            this.labelImageInfo.Text = "Preview";
            // 
            // pictureBoxInspectItem
            // 
            this.pictureBoxInspectItem.BackColor = System.Drawing.Color.Red;
            this.pictureBoxInspectItem.Location = new System.Drawing.Point(253, 184);
            this.pictureBoxInspectItem.Name = "pictureBoxInspectItem";
            this.pictureBoxInspectItem.Size = new System.Drawing.Size(52, 52);
            this.pictureBoxInspectItem.TabIndex = 23;
            this.pictureBoxInspectItem.TabStop = false;
            // 
            // listBoxInspectItems
            // 
            this.listBoxInspectItems.Enabled = false;
            this.listBoxInspectItems.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.listBoxInspectItems.FormattingEnabled = true;
            this.listBoxInspectItems.Location = new System.Drawing.Point(6, 19);
            this.listBoxInspectItems.Name = "listBoxInspectItems";
            this.listBoxInspectItems.SelectionMode = System.Windows.Forms.SelectionMode.MultiSimple;
            this.listBoxInspectItems.Size = new System.Drawing.Size(240, 251);
            this.listBoxInspectItems.TabIndex = 0;
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonCancel.Location = new System.Drawing.Point(458, 302);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 0;
            this.buttonCancel.Text = "C&ancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // buttonOK
            // 
            this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.buttonOK.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonOK.Location = new System.Drawing.Point(377, 302);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(75, 23);
            this.buttonOK.TabIndex = 2;
            this.buttonOK.Text = "&OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
            // 
            // AddBeterMultiMatch
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(545, 335);
            this.Controls.Add(this.groupBoxInspectItems);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonOK);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.KeyPreview = true;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "AddBeterMultiMatch";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Add better multi match";
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.AddBeterMultiMatch_KeyDown);
            this.groupBoxInspectItems.ResumeLayout(false);
            this.groupBoxInspectItems.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownExpandCount)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxInspectItem)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.GroupBox groupBoxInspectItems;
        private System.Windows.Forms.Label labelImageInfo;
        private System.Windows.Forms.PictureBox pictureBoxInspectItem;
        private System.Windows.Forms.ListBox listBoxInspectItems;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.NumericUpDown numericUpDownExpandCount;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.CheckBox checkBoxItalic;
        private System.Windows.Forms.Label labelText;
        private System.Windows.Forms.TextBox textBoxText;
    }
}