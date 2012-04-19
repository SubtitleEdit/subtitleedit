namespace Nikse.SubtitleEdit.Forms
{
    partial class Split
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
            this.groupBoxParts = new System.Windows.Forms.GroupBox();
            this.RadioButtonLines = new System.Windows.Forms.RadioButton();
            this.radioButtonTime = new System.Windows.Forms.RadioButton();
            this.numericUpDownParts = new System.Windows.Forms.NumericUpDown();
            this.labelNumberOfParts = new System.Windows.Forms.Label();
            this.groupBoxOutput = new System.Windows.Forms.GroupBox();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.folderBrowserDialog1 = new System.Windows.Forms.FolderBrowserDialog();
            this.buttonChooseFolder = new System.Windows.Forms.Button();
            this.labelChooseOutputFolder = new System.Windows.Forms.Label();
            this.labelOutputFormat = new System.Windows.Forms.Label();
            this.groupBoxPreview = new System.Windows.Forms.GroupBox();
            this.comboBoxOutputFormat = new System.Windows.Forms.ComboBox();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.buttonSplit = new System.Windows.Forms.Button();
            this.listViewFixes = new System.Windows.Forms.ListView();
            this.columnHeader4 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader5 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader6 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.groupBoxParts.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownParts)).BeginInit();
            this.groupBoxOutput.SuspendLayout();
            this.groupBoxPreview.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBoxParts
            // 
            this.groupBoxParts.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxParts.Controls.Add(this.labelNumberOfParts);
            this.groupBoxParts.Controls.Add(this.numericUpDownParts);
            this.groupBoxParts.Controls.Add(this.radioButtonTime);
            this.groupBoxParts.Controls.Add(this.RadioButtonLines);
            this.groupBoxParts.Location = new System.Drawing.Point(13, 13);
            this.groupBoxParts.Name = "groupBoxParts";
            this.groupBoxParts.Size = new System.Drawing.Size(457, 71);
            this.groupBoxParts.TabIndex = 0;
            this.groupBoxParts.TabStop = false;
            this.groupBoxParts.Text = "Number of parts";
            // 
            // RadioButtonLines
            // 
            this.RadioButtonLines.AutoSize = true;
            this.RadioButtonLines.Checked = true;
            this.RadioButtonLines.Location = new System.Drawing.Point(15, 20);
            this.RadioButtonLines.Name = "RadioButtonLines";
            this.RadioButtonLines.Size = new System.Drawing.Size(50, 17);
            this.RadioButtonLines.TabIndex = 0;
            this.RadioButtonLines.TabStop = true;
            this.RadioButtonLines.Text = "Lines";
            this.RadioButtonLines.UseVisualStyleBackColor = true;
            // 
            // radioButtonTime
            // 
            this.radioButtonTime.AutoSize = true;
            this.radioButtonTime.Location = new System.Drawing.Point(15, 43);
            this.radioButtonTime.Name = "radioButtonTime";
            this.radioButtonTime.Size = new System.Drawing.Size(48, 17);
            this.radioButtonTime.TabIndex = 1;
            this.radioButtonTime.Text = "Time";
            this.radioButtonTime.UseVisualStyleBackColor = true;
            // 
            // numericUpDownParts
            // 
            this.numericUpDownParts.Location = new System.Drawing.Point(117, 38);
            this.numericUpDownParts.Minimum = new decimal(new int[] {
            2,
            0,
            0,
            0});
            this.numericUpDownParts.Name = "numericUpDownParts";
            this.numericUpDownParts.Size = new System.Drawing.Size(50, 20);
            this.numericUpDownParts.TabIndex = 2;
            this.numericUpDownParts.Value = new decimal(new int[] {
            2,
            0,
            0,
            0});
            // 
            // labelNumberOfParts
            // 
            this.labelNumberOfParts.AutoSize = true;
            this.labelNumberOfParts.Location = new System.Drawing.Point(114, 22);
            this.labelNumberOfParts.Name = "labelNumberOfParts";
            this.labelNumberOfParts.Size = new System.Drawing.Size(111, 13);
            this.labelNumberOfParts.TabIndex = 3;
            this.labelNumberOfParts.Text = "Number of equal parts";
            // 
            // groupBoxOutput
            // 
            this.groupBoxOutput.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxOutput.Controls.Add(this.comboBoxOutputFormat);
            this.groupBoxOutput.Controls.Add(this.labelOutputFormat);
            this.groupBoxOutput.Controls.Add(this.labelChooseOutputFolder);
            this.groupBoxOutput.Controls.Add(this.buttonChooseFolder);
            this.groupBoxOutput.Controls.Add(this.textBox1);
            this.groupBoxOutput.Location = new System.Drawing.Point(16, 90);
            this.groupBoxOutput.Name = "groupBoxOutput";
            this.groupBoxOutput.Size = new System.Drawing.Size(457, 106);
            this.groupBoxOutput.TabIndex = 1;
            this.groupBoxOutput.TabStop = false;
            this.groupBoxOutput.Text = "Output";
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(7, 39);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(402, 20);
            this.textBox1.TabIndex = 0;
            // 
            // buttonChooseFolder
            // 
            this.buttonChooseFolder.Location = new System.Drawing.Point(415, 37);
            this.buttonChooseFolder.Name = "buttonChooseFolder";
            this.buttonChooseFolder.Size = new System.Drawing.Size(26, 23);
            this.buttonChooseFolder.TabIndex = 1;
            this.buttonChooseFolder.Text = "...";
            this.buttonChooseFolder.UseVisualStyleBackColor = true;
            // 
            // labelChooseOutputFolder
            // 
            this.labelChooseOutputFolder.AutoSize = true;
            this.labelChooseOutputFolder.Location = new System.Drawing.Point(6, 23);
            this.labelChooseOutputFolder.Name = "labelChooseOutputFolder";
            this.labelChooseOutputFolder.Size = new System.Drawing.Size(105, 13);
            this.labelChooseOutputFolder.TabIndex = 4;
            this.labelChooseOutputFolder.Text = "Choose output folder";
            // 
            // labelOutputFormat
            // 
            this.labelOutputFormat.AutoSize = true;
            this.labelOutputFormat.Location = new System.Drawing.Point(6, 72);
            this.labelOutputFormat.Name = "labelOutputFormat";
            this.labelOutputFormat.Size = new System.Drawing.Size(71, 13);
            this.labelOutputFormat.TabIndex = 5;
            this.labelOutputFormat.Text = "Output format";
            // 
            // groupBoxPreview
            // 
            this.groupBoxPreview.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxPreview.Controls.Add(this.listViewFixes);
            this.groupBoxPreview.Location = new System.Drawing.Point(13, 202);
            this.groupBoxPreview.Name = "groupBoxPreview";
            this.groupBoxPreview.Size = new System.Drawing.Size(460, 148);
            this.groupBoxPreview.TabIndex = 2;
            this.groupBoxPreview.TabStop = false;
            this.groupBoxPreview.Text = "Prevew";
            // 
            // comboBoxOutputFormat
            // 
            this.comboBoxOutputFormat.FormattingEnabled = true;
            this.comboBoxOutputFormat.Location = new System.Drawing.Point(84, 72);
            this.comboBoxOutputFormat.Name = "comboBoxOutputFormat";
            this.comboBoxOutputFormat.Size = new System.Drawing.Size(325, 21);
            this.comboBoxOutputFormat.TabIndex = 6;
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonCancel.Location = new System.Drawing.Point(398, 356);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 21);
            this.buttonCancel.TabIndex = 4;
            this.buttonCancel.Text = "C&ancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            // 
            // buttonSplit
            // 
            this.buttonSplit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonSplit.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonSplit.Location = new System.Drawing.Point(317, 356);
            this.buttonSplit.Name = "buttonSplit";
            this.buttonSplit.Size = new System.Drawing.Size(75, 21);
            this.buttonSplit.TabIndex = 3;
            this.buttonSplit.Text = "&Split";
            this.buttonSplit.UseVisualStyleBackColor = true;
            // 
            // listViewFixes
            // 
            this.listViewFixes.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader4,
            this.columnHeader5,
            this.columnHeader6});
            this.listViewFixes.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listViewFixes.FullRowSelect = true;
            this.listViewFixes.HideSelection = false;
            this.listViewFixes.Location = new System.Drawing.Point(3, 16);
            this.listViewFixes.Name = "listViewFixes";
            this.listViewFixes.Size = new System.Drawing.Size(454, 129);
            this.listViewFixes.TabIndex = 101;
            this.listViewFixes.UseCompatibleStateImageBehavior = false;
            this.listViewFixes.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader4
            // 
            this.columnHeader4.Text = "File name";
            this.columnHeader4.Width = 280;
            // 
            // columnHeader5
            // 
            this.columnHeader5.Text = "#Lines";
            this.columnHeader5.Width = 61;
            // 
            // columnHeader6
            // 
            this.columnHeader6.Text = "Time";
            this.columnHeader6.Width = 61;
            // 
            // Split
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(485, 388);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonSplit);
            this.Controls.Add(this.groupBoxPreview);
            this.Controls.Add(this.groupBoxOutput);
            this.Controls.Add(this.groupBoxParts);
            this.Name = "Split";
            this.Text = "Split";
            this.groupBoxParts.ResumeLayout(false);
            this.groupBoxParts.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownParts)).EndInit();
            this.groupBoxOutput.ResumeLayout(false);
            this.groupBoxOutput.PerformLayout();
            this.groupBoxPreview.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBoxParts;
        private System.Windows.Forms.Label labelNumberOfParts;
        private System.Windows.Forms.NumericUpDown numericUpDownParts;
        private System.Windows.Forms.RadioButton radioButtonTime;
        private System.Windows.Forms.RadioButton RadioButtonLines;
        private System.Windows.Forms.GroupBox groupBoxOutput;
        private System.Windows.Forms.ComboBox comboBoxOutputFormat;
        private System.Windows.Forms.Label labelOutputFormat;
        private System.Windows.Forms.Label labelChooseOutputFolder;
        private System.Windows.Forms.Button buttonChooseFolder;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog1;
        private System.Windows.Forms.GroupBox groupBoxPreview;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Button buttonSplit;
        private System.Windows.Forms.ListView listViewFixes;
        private System.Windows.Forms.ColumnHeader columnHeader4;
        private System.Windows.Forms.ColumnHeader columnHeader5;
        private System.Windows.Forms.ColumnHeader columnHeader6;
    }
}